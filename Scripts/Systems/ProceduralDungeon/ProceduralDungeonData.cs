using Godot;
using System;
using System.Collections.Generic;

public enum DungeonShape
{
    Linear,
    Branching,
    Circular,
    HubAndSpoke,
    Maze
}

public enum RoomType
{
    Empty,
    Combat,
    Treasure,
    Boss,
    MiniBoss,
    Shop,
    Rest,
    Puzzle,
    Secret,
    Trap,
    Event
}

public enum DungeonDifficulty
{
    Easy,
    Normal,
    Hard,
    Nightmare,
    Legendary
}

[System.Serializable]
public class DungeonRoom
{
    public int id;
    public RoomType type;
    public int x;
    public int y;
    public int width;
    public int height;
    public bool isDiscovered;
    public bool isCompleted;
    public List<int> connectedRooms = new List<int>();
    public Dictionary<string, object> roomData = new Dictionary<string, object>();
    
    public DungeonRoom(int roomId, RoomType roomType, int posX, int posY)
    {
        id = roomId;
        type = roomType;
        x = posX;
        y = posY;
        width = 5 + GD.RandI() % 3;
        height = 5 + GD.RandI() % 3;
        isDiscovered = false;
        isCompleted = false;
    }
}

[System.Serializable]
public class DungeonFloor
{
    public int floorNumber;
    public DungeonShape shape;
    public DungeonDifficulty difficulty;
    public List<DungeonRoom> rooms = new List<DungeonRoom>();
    public int startRoomId;
    public int bossRoomId;
    public bool isCompleted;
    public int playerBestTime;
    
    public DungeonFloor(int floor, DungeonShape shapeType, DungeonDifficulty diff)
    {
        floorNumber = floor;
        shape = shapeType;
        difficulty = diff;
        isCompleted = false;
        playerBestTime = 0;
    }
}

[System.Serializable]
public class ProceduralDungeonData
{
    public int currentFloor;
    public DungeonShape currentShape;
    public DungeonDifficulty currentDifficulty;
    public List<DungeonFloor> floorHistory = new List<DungeonFloor>();
    public Dictionary<int, DungeonFloor> activeDungeons = new Dictionary<int, DungeonFloor>();
    public int totalRoomsGenerated;
    public int totalBossesDefeated;
    public int totalTreasureRoomsFound;
    public int fastestClearTime;
    public List<string> seedHistory = new List<string>();
}
