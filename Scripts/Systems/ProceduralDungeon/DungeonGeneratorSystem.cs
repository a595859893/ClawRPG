using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 地下城生成系统 - 负责地图整体生成流程（入口点）
    /// </summary>
    public class DungeonGeneratorSystem : BaseSystem
    {
        private static DungeonGeneratorSystem _instance;
        public static DungeonGeneratorSystem Instance => _instance;
        
        private Random _random;
        private ProceduralDungeonDatabase _database;
        private GameModeConfig _gameModeConfig;
        
        // 信号事件
        public static Signal DungeonGenerated => new("dungeon_generated");
        
        public DungeonGeneratorSystem()
        {
            _instance = this;
            _random = new Random();
            _database = ProceduralDungeonDatabase.Instance;
            _gameModeConfig = GameModeConfig.Instance;
        }
        
        public override void _Ready()
        {
            GD.Print("Dungeon Generator System initialized");
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
            
            var dungeon = new GeneratedDungeon
            {
                DungeonId = Guid.NewGuid().ToString(),
                DungeonName = config.DisplayName,
                TotalFloors = (int)(config.TotalFloors * _gameModeConfig.GetFloorCountMultiplier()),
                CurrentFloor = 1
            };
            
            // 生成每一层
            for (int floor = 1; floor <= config.TotalFloors; floor++)
            {
                var dungeonFloor = GenerateFloor(floor, config, floor == config.TotalFloors);
                dungeon.Floors.Add(dungeonFloor);
            }
            
            GD.Print($"Generated dungeon: {dungeon.DungeonName} with {dungeon.TotalFloors} floors");
            
            DungeonGenerated?.Emit();
            
            return dungeon;
        }
        
        /// <summary>
        /// 生成单个楼层
        /// </summary>
        public DungeonFloor GenerateFloor(int floorNumber, DungeonTypeConfig config, bool isBossFloor)
        {
            var difficultySystem = DungeonDifficultySystem.Instance;
            var layoutSystem = RoomLayoutSystem.Instance;
            
            var floorConfig = difficultySystem.GetFloorConfig(floorNumber);
            
            var floor = new DungeonFloor
            {
                FloorNumber = floorNumber,
                FloorName = $"{config.DisplayName} - {floorConfig.FloorName}"
            };
            
            // 决定房间数量 - 支持快速模式
            int roomCount = difficultySystem.GetRandomRoomCount(floorConfig);
            
            // 获取难度乘数
            float enemyStrength = config.EnemyStrengthMultiplier;
            float treasureValue = config.TreasureMultiplier;
            
            // 生成房间布局
            var rooms = layoutSystem.GenerateRoomLayout(roomCount, config, floorNumber, isBossFloor, enemyStrength, treasureValue);
            
            // 填充房间内容
            foreach (var room in rooms)
            {
                difficultySystem.PopulateRoom(room, config, floorNumber);
            }
            
            floor.Rooms = rooms;
            
            // 设置入口和出口
            floor.EntranceRoom = rooms.FirstOrDefault(r => r.Type == RoomType.Entrance) ?? rooms.FirstOrDefault();
            floor.ExitRoom = isBossFloor 
                ? rooms.FirstOrDefault(r => r.Type == RoomType.Boss) 
                : rooms.LastOrDefault(r => r.Type != RoomType.Entrance);
            
            return floor;
        }
        
        /// <summary>
        /// 通过ID查找房间
        /// </summary>
        public DungeonRoom FindRoomById(GeneratedDungeon dungeon, string roomId)
        {
            if (dungeon == null) return null;
            
            foreach (var floor in dungeon.Floors)
            {
                var room = floor.Rooms.FirstOrDefault(r => r.RoomId == roomId);
                if (room != null) return room;
            }
            
            return null;
        }
        
        /// <summary>
        /// 获取当前可用的连接房间
        /// </summary>
        public List<DungeonRoom> GetConnectedRooms(GeneratedDungeon dungeon, DungeonRoom currentRoom)
        {
            if (dungeon?.CurrentRoom == null) return new List<DungeonRoom>();
            
            var connected = new List<DungeonRoom>();
            foreach (var roomId in currentRoom.ConnectedRooms)
            {
                var room = FindRoomById(dungeon, roomId);
                if (room != null) connected.Add(room);
            }
            
            return connected;
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
        {
            return new System.Collections.Generic.Dictionary<string, object>();
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
        {
            // No persistent state to restore
        }
    }
}
