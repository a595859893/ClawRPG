using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 程序化地下城生成系统
    /// </summary>
    public class ProceduralDungeonSystem : Node
    {
        private static ProceduralDungeonSystem _instance;
        public static ProceduralDungeonSystem Instance => _instance;
        
        // 当前地下城实例
        public GeneratedDungeon CurrentDungeon { get; private set; }
        
        // 玩家进度
        public DungeonProgress CurrentProgress { get; private set; }
        
        // 统计数据
        public DungeonStatistics Statistics { get; private set; }
        
        // 信号事件
        public static Signal DungeonGenerated => new("dungeon_generated");
        public static Signal RoomEntered => new("room_entered");
        public static Signal RoomCleared => new("room_cleared");
        public static Signal FloorCompleted => new("floor_completed");
        public static Signal DungeonCompleted => new("dungeon_completed");
        public static Signal TreasureFound => new("treasure_found");
        public static Signal SecretDiscovered => new("secret_discovered");
        
        private Random _random;
        private ProceduralDungeonDatabase _database;
        private GameModeConfig _gameModeConfig;
        
        public ProceduralDungeonSystem()
        {
            _instance = this;
            _random = new Random();
            _database = ProceduralDungeonDatabase.Instance;
            _gameModeConfig = GameModeConfig.Instance;
            Statistics = new DungeonStatistics();
        }
        
        public override void _Ready()
        {
            GD.Print("Procedural Dungeon System initialized");
        }
        
        /// <summary>
        /// 生成新地下城
        /// </summary>
        public GeneratedDungeon GenerateDungeon(string dungeonTypeId, int seed = -1)
        {
            if (seed > 0)
            {
                _random = new Random(seed);
            }
            else
            {
                _random = new Random();
            }
            
            var config = _database.GetDungeonType(dungeonTypeId);
            if (config == null)
            {
                GD.PrintErr($"Unknown dungeon type: {dungeonTypeId}");
                return null;
            }
            
            CurrentDungeon = new GeneratedDungeon
            {
                DungeonId = Guid.NewGuid().ToString(),
                DungeonName = config.DisplayName,
                TotalFloors = (int)(config.TotalFloors * _gameModeConfig.GetFloorCountMultiplier()),
                CurrentFloor = 1
            };
            
            CurrentProgress = new DungeonProgress
            {
                DungeonId = CurrentDungeon.DungeonId,
                CurrentFloor = 1,
                StartTime = DateTime.Now
            };
            
            // 生成每一层
            for (int floor = 1; floor <= config.TotalFloors; floor++)
            {
                var dungeonFloor = GenerateFloor(floor, config, floor == config.TotalFloors);
                CurrentDungeon.Floors.Add(dungeonFloor);
            }
            
            // 设置初始房间
            if (CurrentDungeon.Floors.Count > 0 && CurrentDungeon.Floors[0].Rooms.Count > 0)
            {
                CurrentDungeon.CurrentRoom = CurrentDungeon.Floors[0].EntranceRoom;
                CurrentDungeon.CurrentRoom.IsDiscovered = true;
                CurrentProgress.CurrentRoomId = CurrentDungeon.CurrentRoom.RoomId;
            }
            
            Statistics.TotalDungeonsEntered++;
            
            DungeonGenerated?.Emit();
            
            GD.Print($"Generated dungeon: {CurrentDungeon.DungeonName} with {CurrentDungeon.TotalFloors} floors");
            
            return CurrentDungeon;
        }
        
        /// <summary>
        /// 生成单个楼层
        /// </summary>
        private DungeonFloor GenerateFloor(int floorNumber, DungeonTypeConfig config, bool isBossFloor)
        {
            var floorConfig = _database.FloorConfigs.FirstOrDefault(f => f.FloorNumber == floorNumber) 
                ?? new DungeonFloorConfig { FloorNumber = floorNumber };
            
            var floor = new DungeonFloor
            {
                FloorNumber = floorNumber,
                FloorName = $"{config.DisplayName} - {floorConfig.FloorName}"
            };
            
            // 决定房间数量 - 支持快速模式
            int roomCount;
            bool quickMode = GameModeManager.Instance?.IsQuickMode() ?? false;
            
            if (quickMode)
            {
                // 快速模式：减少房间数量
                var gameConfig = GameModeManager.Instance;
                var (min, max) = gameConfig.GetRoomRange(floorConfig.MinRooms, floorConfig.MaxRooms);
                roomCount = _random.Next(min, max + 1);
                GD.Print($"[QuickMode] Room count reduced: {floorConfig.MinRooms}-{floorConfig.MaxRooms} -> {min}-{max}");
            }
            else
            {
                roomCount = _random.Next(floorConfig.MinRooms, floorConfig.MaxRooms + 1);
            }
            
            // 生成房间布局
            var rooms = GenerateRoomLayout(roomCount, config, floorNumber, isBossFloor);
            
            // 连接房间
            ConnectRooms(rooms);
            
            floor.Rooms = rooms;
            
            // 设置入口和出口
            floor.EntranceRoom = rooms.FirstOrDefault(r => r.Type == RoomType.Entrance) ?? rooms.FirstOrDefault();
            floor.ExitRoom = isBossFloor 
                ? rooms.FirstOrDefault(r => r.Type == RoomType.Boss) 
                : rooms.LastOrDefault(r => r.Type != RoomType.Entrance);
            
            return floor;
        }
        
        /// <summary>
        /// 生成房间布局
        /// </summary>
        private List<DungeonRoom> GenerateRoomLayout(int count, DungeonTypeConfig config, int floorNumber, bool isBossFloor)
        {
            var rooms = new List<DungeonRoom>();
            var usedPositions = new HashSet<(int, int)>();
            
            // 入口房间
            var entrance = CreateRoom(RoomType.Entrance, config, floorNumber, 0, 0);
            rooms.Add(entrance);
            usedPositions.Add((0, 0));
            
            // 生成其他房间
            for (int i = 1; i < count; i++)
            {
                RoomType roomType;
                
                if (isBossFloor && i == count - 1)
                {
                    roomType = RoomType.Boss;
                }
                else
                {
                    // 根据概率选择房间类型
                    roomType = SelectRoomType(config);
                }
                
                // 找到可用位置
                var (x, y) = FindAvailablePosition(usedPositions, i);
                usedPositions.Add((x, y));
                
                var room = CreateRoom(roomType, config, floorNumber, x, y);
                rooms.Add(room);
            }
            
            return rooms;
        }
        
        /// <summary>
        /// 选择房间类型
        /// </summary>
        private RoomType SelectRoomType(DungeonTypeConfig config)
        {
            var allowedTypes = config.AllowedRoomTypes;
            
            // 基础概率分布
            var weights = new Dictionary<RoomType, int>
            {
                [RoomType.Combat] = 40,
                [RoomType.Treasure] = 15,
                [RoomType.Elite] = 10,
                [RoomType.Rest] = 10,
                [RoomType.Event] = 10,
                [RoomType.Secret] = (int)(config.SecretChance * 100),
                [RoomType.Trap] = 5,
                [RoomType.Merchant] = 5,
                [RoomType.Puzzle] = 5
            };
            
            // 过滤允许的类型
            var availableWeights = weights.Where(w => allowedTypes.Contains(w.Key)).ToDictionary(w => w.Key, w => w.Value);
            
            int totalWeight = availableWeights.Values.Sum();
            int roll = _random.Next(totalWeight);
            
            int current = 0;
            foreach (var kvp in availableWeights)
            {
                current += kvp.Value;
                if (roll < current)
                {
                    return kvp.Key;
                }
            }
            
            return RoomType.Combat;
        }
        
        /// <summary>
        /// 创建房间
        /// </summary>
        private DungeonRoom CreateRoom(RoomType type, DungeonTypeConfig config, int floor, int gridX, int gridY)
        {
            var templates = _database.GetRoomTemplates(type);
            var template = templates.Count > 0 ? templates[_random.Next(templates.Count)] : null;
            
            var room = new DungeonRoom
            {
                RoomId = Guid.NewGuid().ToString(),
                Type = type,
                Difficulty = CalculateDifficulty(floor, config),
                Width = template?.Width ?? 15,
                Height = template?.Height ?? 15,
                GridX = gridX,
                GridY = gridY
            };
            
            // 根据房间类型生成内容
            switch (type)
            {
                case RoomType.Combat:
                case RoomType.Elite:
                    room.Enemies = GenerateEnemyList(room.Difficulty, type == RoomType.Elite);
                    break;
                case RoomType.Boss:
                    room.Enemies = GenerateBossEnemy(floor);
                    break;
                case RoomType.Treasure:
                    room.TreasureId = SelectTreasure();
                    break;
                case RoomType.Event:
                    room.EventId = SelectEvent();
                    break;
                case RoomType.Secret:
                    room.TreasureId = SelectTreasure();
                    break;
            }
            
            // 添加房间修正因子
            room.RoomModifiers["enemy_strength"] = config.EnemyStrengthMultiplier;
            room.RoomModifiers["treasure_value"] = config.TreasureMultiplier;
            
            return room;
        }
        
        /// <summary>
        /// 计算房间难度
        /// </summary>
        private RoomDifficulty CalculateDifficulty(int floor, DungeonTypeConfig config)
        {
            float difficulty = floor * config.ThemeModifier;
            
            if (difficulty < 2) return RoomDifficulty.Easy;
            if (difficulty < 4) return RoomDifficulty.Normal;
            if (difficulty < 6) return RoomDifficulty.Hard;
            if (difficulty < 8) return RoomDifficulty.Nightmare;
            return RoomDifficulty.Legendary;
        }
        
        /// <summary>
        /// 生成敌人列表
        /// </summary>
        private List<string> GenerateEnemyList(RoomDifficulty difficulty, bool isElite)
        {
            var enemies = new List<string>();
            int count = isElite ? 1 : _random.Next(2, 5);
            
            string enemyType = isElite ? "Elite" : "Basic";
            
            for (int i = 0; i < count; i++)
            {
                enemies.Add($"{enemyType}_{difficulty}_{i}");
            }
            
            return enemies;
        }
        
        /// <summary>
        /// 生成Boss敌人
        /// </summary>
        private List<string> GenerateBossEnemy(int floor)
        {
            return new List<string> { $"Boss_Floor{floor}" };
        }
        
        /// <summary>
        /// 选择宝藏
        /// </summary>
        private string SelectTreasure()
        {
            var treasures = _database.Treasures;
            float roll = (float)_random.NextDouble();
            
            foreach (var treasure in treasures.OrderByDescending(t => t.Rarity))
            {
                if (roll < treasure.Rarity)
                {
                    return treasure.TreasureId;
                }
            }
            
            return treasures[0].TreasureId;
        }
        
        /// <summary>
        /// 选择事件
        /// </summary>
        private string SelectEvent()
        {
            var events = _database.Events;
            return events[_random.Next(events.Count)].EventId;
        }
        
        /// <summary>
        /// 找到可用位置
        /// </summary>
        private (int, int) FindAvailablePosition(HashSet<(int, int)> used, int attempt)
        {
            // 使用螺旋式搜索找到空闲位置
            for (int radius = 1; radius < 20; radius++)
            {
                for (int angle = 0; angle < 8 * radius; angle++)
                {
                    double rad = angle * Math.PI / (4 * radius);
                    int x = (int)(radius * Math.Cos(rad));
                    int y = (int)(radius * Math.Sin(rad));
                    
                    if (!used.Contains((x, y)))
                    {
                        return (x, y);
                    }
                }
            }
            
            return (attempt, 0);
        }
        
        /// <summary>
        /// 连接房间
        /// </summary>
        private void ConnectRooms(List<DungeonRoom> rooms)
        {
            // 简单的邻居连接算法
            for (int i = 0; i < rooms.Count; i++)
            {
                var room = rooms[i];
                
                // 找到最近的邻居
                var neighbors = rooms
                    .Where(r => r != room)
                    .OrderBy(r => Math.Abs(r.GridX - room.GridX) + Math.Abs(r.GridY - room.GridY))
                    .Take(2);
                
                foreach (var neighbor in neighbors)
                {
                    if (!room.ConnectedRooms.Contains(neighbor.RoomId))
                    {
                        room.ConnectedRooms.Add(neighbor.RoomId);
                    }
                }
            }
        }
        
        /// <summary>
        /// 进入房间
        /// </summary>
        public bool EnterRoom(string roomId)
        {
            if (CurrentDungeon == null) return false;
            
            var room = FindRoomById(roomId);
            if (room == null) return false;
            
            CurrentDungeon.CurrentRoom = room;
            room.IsDiscovered = true;
            
            if (!CurrentProgress.ClearedRooms.Contains(roomId))
            {
                CurrentProgress.CurrentRoomId = roomId;
            }
            
            if (!CurrentDungeon.VisitedRooms.Contains(roomId))
            {
                CurrentDungeon.VisitedRooms.Add(roomId);
            }
            
            RoomEntered?.Emit();
            
            GD.Print($"Entered room: {room.Type} ({room.RoomId})");
            
            return true;
        }
        
        /// <summary>
        /// 清理房间
        /// </summary>
        public void ClearCurrentRoom()
        {
            if (CurrentDungeon?.CurrentRoom == null) return;
            
            var room = CurrentDungeon.CurrentRoom;
            if (!room.IsCleared)
            {
                room.IsCleared = true;
                
                if (!CurrentProgress.ClearedRooms.Contains(room.RoomId))
                {
                    CurrentProgress.ClearedRooms.Add(room.RoomId);
                }
                
                CurrentDungeon.TotalEnemiesDefeated++;
                Statistics.TotalEnemiesDefeated++;
                
                // 宝藏检查
                if (!string.IsNullOrEmpty(room.TreasureId))
                {
                    CurrentDungeon.TotalTreasuresFound++;
                    Statistics.TotalTreasuresFound++;
                    TreasureFound?.Emit();
                }
                
                // 密室检查
                if (room.Type == RoomType.Secret && !CurrentProgress.DiscoveredSecrets.Contains(room.RoomId))
                {
                    CurrentProgress.DiscoveredSecrets.Add(room.RoomId);
                    Statistics.TotalSecretsDiscovered++;
                    SecretDiscovered?.Emit();
                }
                
                RoomCleared?.Emit();
                
                GD.Print($"Room cleared: {room.Type}");
                
                // 检查是否是出口房间
                var currentFloor = CurrentDungeon.Floors[CurrentDungeon.CurrentFloor - 1];
                if (room == currentFloor.ExitRoom && room.Type == RoomType.Boss)
                {
                    CompleteFloor();
                }
            }
        }
        
        /// <summary>
        /// 完成楼层
        /// </summary>
        public void CompleteFloor()
        {
            if (CurrentDungeon == null) return;
            
            var currentFloor = CurrentDungeon.Floors[CurrentDungeon.CurrentFloor - 1];
            currentFloor.IsCompleted = true;
            CurrentDungeon.CurrentFloor++;
            Statistics.TotalFloorsCleared++;
            
            FloorCompleted?.Emit();
            
            GD.Print($"Floor {currentFloor.FloorNumber} completed");
            
            // 检查是否完成整个地下城
            if (CurrentDungeon.CurrentFloor > CurrentDungeon.TotalFloors)
            {
                CompleteDungeon();
            }
            else
            {
                // 进入下一层
                var nextFloor = CurrentDungeon.Floors[CurrentDungeon.CurrentFloor - 1];
                if (nextFloor?.EntranceRoom != null)
                {
                    EnterRoom(nextFloor.EntranceRoom.RoomId);
                }
                CurrentProgress.CurrentFloor = CurrentDungeon.CurrentFloor;
            }
        }
        
        /// <summary>
        /// 完成地下城
        /// </summary>
        public void CompleteDungeon()
        {
            if (CurrentDungeon == null) return;
            
            Statistics.TotalDungeonsCompleted++;
            CurrentDungeon.TimeElapsed = DateTime.Now - CurrentProgress.StartTime;
            Statistics.TotalTimeInDungeons += CurrentDungeon.TimeElapsed;
            
            DungeonCompleted?.Emit();
            
            GD.Print($"Dungeon completed: {CurrentDungeon.DungeonName}");
        }
        
        /// <summary>
        /// 通过ID查找房间
        /// </summary>
        private DungeonRoom FindRoomById(string roomId)
        {
            if (CurrentDungeon == null) return null;
            
            foreach (var floor in CurrentDungeon.Floors)
            {
                var room = floor.Rooms.FirstOrDefault(r => r.RoomId == roomId);
                if (room != null) return room;
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取当前可用的连接房间
        /// </summary>
        public List<DungeonRoom> GetConnectedRooms()
        {
            if (CurrentDungeon?.CurrentRoom == null) return new List<DungeonRoom>();
            
            var connected = new List<DungeonRoom>();
            foreach (var roomId in CurrentDungeon.CurrentRoom.ConnectedRooms)
            {
                var room = FindRoomById(roomId);
                if (room != null) connected.Add(room);
            }
            
            return connected;
        }
        
        /// <summary>
        /// 获取房间信息
        /// </summary>
        public Dictionary GetRoomInfo()
        {
            if (CurrentDungeon?.CurrentRoom == null) return null;
            
            var room = CurrentDungeon.CurrentRoom;
            return new Dictionary
            {
                ["room_id"] = room.RoomId,
                ["type"] = room.Type.ToString(),
                ["difficulty"] = room.Difficulty.ToString(),
                ["is_cleared"] = room.IsCleared,
                ["is_discovered"] = room.IsDiscovered,
                ["width"] = room.Width,
                ["height"] = room.Height,
                ["enemy_count"] = room.Enemies.Count,
                ["has_treasure"] = !string.IsNullOrEmpty(room.TreasureId),
                ["has_event"] = !string.IsNullOrEmpty(room.EventId)
            };
        }
        
        /// <summary>
        /// 获取地下城信息
        /// </summary>
        public Dictionary GetDungeonInfo()
        {
            if (CurrentDungeon == null) return null;
            
            return new Dictionary
            {
                ["dungeon_id"] = CurrentDungeon.DungeonId,
                ["dungeon_name"] = CurrentDungeon.DungeonName,
                ["current_floor"] = CurrentDungeon.CurrentFloor,
                ["total_floors"] = CurrentDungeon.TotalFloors,
                ["rooms_visited"] = CurrentDungeon.VisitedRooms.Count,
                ["treasures_found"] = CurrentDungeon.TotalTreasuresFound,
                ["enemies_defeated"] = CurrentDungeon.TotalEnemiesDefeated
            };
        }
    }
}
