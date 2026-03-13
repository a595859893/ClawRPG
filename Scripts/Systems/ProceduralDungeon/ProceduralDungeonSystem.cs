using Godot;
using System;
using System.Collections.Generic;

public class ProceduralDungeonSystem : Node
{
    private ProceduralDungeonData data;
    private ProceduralDungeonDatabase db;
    private RandomNumberGenerator rng;
    
    public override void _Ready()
    {
        data = new ProceduralDungeonData();
        db = ProceduralDungeonDatabase.Instance;
        rng = new RandomNumberGenerator();
        rng.Randomize();
        
        GD.Print("[ProceduralDungeonSystem] Initialized - 726+ systems milestone");
    }
    
    public void SetSeed(string seed)
    {
        rng.Seed = seed.GetHashCode();
        if (!data.seedHistory.Contains(seed))
            data.seedHistory.Add(seed);
    }
    
    public DungeonFloor GenerateDungeon(int floorNumber, DungeonShape shape, DungeonDifficulty difficulty)
    {
        int roomCount = db.GetRoomCountForShape(shape);
        DungeonFloor floor = new DungeonFloor(floorNumber, shape, difficulty);
        
        // Generate rooms based on shape
        switch (shape)
        {
            case DungeonShape.Linear:
                GenerateLinearDungeon(floor, roomCount);
                break;
            case DungeonShape.Branching:
                GenerateBranchingDungeon(floor, roomCount);
                break;
            case DungeonShape.Circular:
                GenerateCircularDungeon(floor, roomCount);
                break;
            case DungeonShape.HubAndSpoke:
                GenerateHubAndSpokeDungeon(floor, roomCount);
                break;
            case DungeonShape.Maze:
                GenerateMazeDungeon(floor, roomCount);
                break;
        }
        
        // Assign special rooms
        AssignSpecialRooms(floor);
        
        // Store in active dungeons
        data.activeDungeons[floorNumber] = floor;
        data.currentFloor = floorNumber;
        data.currentShape = shape;
        data.currentDifficulty = difficulty;
        data.totalRoomsGenerated += roomCount;
        
        return floor;
    }
    
    private void GenerateLinearDungeon(DungeonFloor floor, int roomCount)
    {
        int currentX = 0;
        
        for (int i = 0; i < roomCount; i++)
        {
            RoomType type = (i == roomCount - 1) ? RoomType.Boss : 
                           (i == 0) ? RoomType.Empty : db.GetRandomRoomType(floor.difficulty);
            
            DungeonRoom room = new DungeonRoom(i, type, currentX, 0);
            floor.rooms.Add(room);
            
            if (i > 0)
            {
                floor.rooms[i - 1].connectedRooms.Add(i);
                room.connectedRooms.Add(i - 1);
            }
            
            currentX += room.width + 2;
        }
        
        floor.startRoomId = 0;
        floor.bossRoomId = roomCount - 1;
    }
    
    private void GenerateBranchingDungeon(DungeonFloor floor, int roomCount)
    {
        int roomId = 0;
        
        // Create main path
        int mainPathLength = roomCount / 2;
        int lastMainId = -1;
        
        for (int i = 0; i < mainPathLength; i++)
        {
            RoomType type = (i == mainPathLength - 1) ? RoomType.Boss : 
                           db.GetRandomRoomType(floor.difficulty);
            
            DungeonRoom room = new DungeonRoom(roomId++, type, i * 8, 0);
            floor.rooms.Add(room);
            
            if (lastMainId >= 0)
            {
                floor.rooms[lastMainId].connectedRooms.Add(room.id);
                room.connectedRooms.Add(lastMainId);
            }
            
            lastMainId = room.id;
            
            // Add branches
            if (i < mainPathLength - 1 && GD.RandD() > 0.5f)
            {
                int branchRoomId = CreateBranch(floor, room.id, 2 + GD.RandI() % 3, 3);
                if (branchRoomId >= 0)
                {
                    room.connectedRooms.Add(branchRoomId);
                    floor.rooms[branchRoomId].connectedRooms.Add(room.id);
                }
            }
        }
        
        floor.startRoomId = 0;
        floor.bossRoomId = lastMainId;
    }
    
    private int CreateBranch(DungeonFloor floor, int fromRoomId, int length, int offsetY)
    {
        int startId = floor.rooms.Count;
        int prevId = fromRoomId;
        
        for (int i = 0; i < length; i++)
        {
            RoomType type = (i == length - 1) ? RoomType.Treasure : db.GetRandomRoomType(floor.difficulty);
            DungeonRoom room = new DungeonRoom(startId + i, type, floor.rooms[fromRoomId].x + (i + 1) * 6, offsetY);
            floor.rooms.Add(room);
            
            floor.rooms[prevId].connectedRooms.Add(room.id);
            room.connectedRooms.Add(prevId);
            
            prevId = room.id;
        }
        
        return prevId;
    }
    
    private void GenerateCircularDungeon(DungeonFloor floor, int roomCount)
    {
        float radius = roomCount * 0.5f;
        
        // Create ring of rooms
        for (int i = 0; i < roomCount; i++)
        {
            float angle = (float)(2 * Math.PI * i / roomCount);
            int x = (int)(radius * Math.Cos(angle));
            int y = (int)(radius * Math.Sin(angle));
            
            RoomType type = db.GetRandomRoomType(floor.difficulty);
            DungeonRoom room = new DungeonRoom(i, type, x, y);
            floor.rooms.Add(room);
            
            // Connect to next room in ring
            int nextId = (i + 1) % roomCount;
            room.connectedRooms.Add(nextId);
            floor.rooms[nextId].connectedRooms.Add(room.id);
        }
        
        // Add center room (boss)
        DungeonRoom centerRoom = new DungeonRoom(roomCount, RoomType.Boss, 0, 0);
        floor.rooms.Add(centerRoom);
        
        // Connect some rooms to center
        for (int i = 0; i < Mathf.Min(3, roomCount); i++)
        {
            floor.rooms[i].connectedRooms.Add(centerRoom.id);
            centerRoom.connectedRooms.Add(floor.rooms[i].id);
        }
        
        floor.startRoomId = 0;
        floor.bossRoomId = centerRoom.id;
    }
    
    private void GenerateHubAndSpokeDungeon(DungeonFloor floor, int roomCount)
    {
        // Create hub (center)
        DungeonRoom hub = new DungeonRoom(0, RoomType.Rest, 0, 0);
        floor.rooms.Add(hub);
        
        int spokes = Mathf.Min(roomCount - 1, 5);
        int roomsPerSpoke = (roomCount - 1) / spokes;
        
        int roomId = 1;
        for (int s = 0; s < spokes; s++)
        {
            float angle = (float)(2 * Math.PI * s / spokes);
            int startX = (int)(5 * Math.Cos(angle));
            int startY = (int)(5 * Math.Sin(angle));
            
            for (int r = 0; r < roomsPerSpoke; r++)
            {
                RoomType type = (r == roomsPerSpoke - 1) ? RoomType.Boss : db.GetRandomRoomType(floor.difficulty);
                int x = startX + r * 3 * (int)Math.Cos(angle);
                int y = startY + r * 3 * (int)Math.Sin(angle);
                
                DungeonRoom room = new DungeonRoom(roomId++, type, x, y);
                floor.rooms.Add(room);
                
                if (r == 0)
                {
                    room.connectedRooms.Add(0);
                    hub.connectedRooms.Add(room.id);
                }
                else
                {
                    room.connectedRooms.Add(roomId - 2);
                    floor.rooms[roomId - 2].connectedRooms.Add(room.id);
                }
            }
        }
        
        floor.startRoomId = 0;
        floor.bossRoomId = roomId - 1;
    }
    
    private void GenerateMazeDungeon(DungeonFloor floor, int roomCount)
    {
        int gridSize = (int)Math.Ceiling(Math.Sqrt(roomCount));
        
        for (int i = 0; i < roomCount; i++)
        {
            int gridX = i % gridSize;
            int gridY = i / gridSize;
            
            RoomType type = (i == roomCount - 1) ? RoomType.Boss : db.GetRandomRoomType(floor.difficulty);
            DungeonRoom room = new DungeonRoom(i, type, gridX * 6, gridY * 4);
            floor.rooms.Add(room);
            
            // Connect to adjacent rooms with some randomness
            if (gridX > 0 && GD.RandD() > 0.3f)
            {
                int leftId = i - 1;
                room.connectedRooms.Add(leftId);
                floor.rooms[leftId].connectedRooms.Add(i);
            }
            
            if (gridY > 0 && GD.RandD() > 0.3f)
            {
                int upId = i - gridSize;
                if (upId >= 0)
                {
                    room.connectedRooms.Add(upId);
                    floor.rooms[upId].connectedRooms.Add(i);
                }
            }
        }
        
        // Ensure path exists using simple DFS
        EnsurePathExists(floor, 0, roomCount - 1);
        
        floor.startRoomId = 0;
        floor.bossRoomId = roomCount - 1;
    }
    
    private void EnsurePathExists(DungeonFloor floor, int startId, int endId)
    {
        HashSet<int> visited = new HashSet<int>();
        List<int> path = new List<int>();
        
        FindPath(floor, startId, endId, visited, path);
        
        // Add connections if no path exists
        if (path.Count == 0)
        {
            int mid = floor.rooms.Count / 2;
            floor.rooms[startId].connectedRooms.Add(mid);
            floor.rooms[mid].connectedRooms.Add(startId);
            floor.rooms[mid].connectedRooms.Add(endId);
            floor.rooms[endId].connectedRooms.Add(mid);
        }
    }
    
    private bool FindPath(DungeonFloor floor, int current, int target, HashSet<int> visited, List<int> path)
    {
        if (current == target)
        {
            path.Add(current);
            return true;
        }
        
        visited.Add(current);
        path.Add(current);
        
        foreach (int next in floor.rooms[current].connectedRooms)
        {
            if (!visited.Contains(next))
            {
                if (FindPath(floor, next, target, visited, path))
                    return true;
            }
        }
        
        path.RemoveAt(path.Count - 1);
        return false;
    }
    
    private void AssignSpecialRooms(DungeonFloor floor)
    {
        // Ensure start room is accessible
        if (floor.rooms.Count > 0)
        {
            floor.rooms[0].type = RoomType.Empty;
            floor.startRoomId = 0;
        }
        
        // Find or create boss room
        bool hasBoss = false;
        foreach (var room in floor.rooms)
        {
            if (room.type == RoomType.Boss)
            {
                floor.bossRoomId = room.id;
                hasBoss = true;
                break;
            }
        }
        
        if (!hasBoss && floor.rooms.Count > 0)
        {
            floor.rooms[floor.rooms.Count - 1].type = RoomType.Boss;
            floor.bossRoomId = floor.rooms.Count - 1;
        }
        
        // Count treasures
        foreach (var room in floor.rooms)
        {
            if (room.type == RoomType.Treasure)
                data.totalTreasureRoomsFound++;
        }
    }
    
    public void DiscoverRoom(int floorNumber, int roomId)
    {
        if (data.activeDungeons.ContainsKey(floorNumber))
        {
            var floor = data.activeDungeons[floorNumber];
            foreach (var room in floor.rooms)
            {
                if (room.id == roomId)
                {
                    room.isDiscovered = true;
                    
                    // Discover connected rooms
                    foreach (int connectedId in room.connectedRooms)
                    {
                        foreach (var r in floor.rooms)
                        {
                            if (r.id == connectedId)
                                r.isDiscovered = true;
                        }
                    }
                    break;
                }
            }
        }
    }
    
    public void CompleteRoom(int floorNumber, int roomId)
    {
        if (data.activeDungeons.ContainsKey(floorNumber))
        {
            var floor = data.activeDungeons[floorNumber];
            foreach (var room in floor.rooms)
            {
                if (room.id == roomId)
                {
                    room.isCompleted = true;
                    
                    if (room.type == RoomType.Boss)
                    {
                        floor.isCompleted = true;
                        data.totalBossesDefeated++;
                    }
                    break;
                }
            }
        }
    }
    
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "totalRoomsGenerated", data.totalRoomsGenerated },
            { "totalBossesDefeated", data.totalBossesDefeated },
            { "totalTreasureRoomsFound", data.totalTreasureRoomsFound },
            { "fastestClearTime", data.fastestClearTime },
            { "floorsCompleted", data.floorHistory.Count },
            { "activeDungeons", data.activeDungeons.Count }
        };
    }
    
    public ProceduralDungeonData GetData() => data;
}
