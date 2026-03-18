using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 房间布局系统 - 负责房间生成、位置计算、房间连接
    /// </summary>
    public partial class RoomLayoutSystem : BaseSystem
    {
        private static RoomLayoutSystem _instance;
        public static RoomLayoutSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new RoomLayoutSystem();
                return _instance;
            }
            private set { _instance = value; }
        }

        protected override string SystemName => "RoomLayoutSystem";

        private Random _random;
        private ProceduralDungeonDatabase _database;

        public RoomLayoutSystem()
        {
            _random = new Random();
            _database = ProceduralDungeonDatabase.Instance;
        }

        /// <summary>
        /// 设置随机数种子
        /// </summary>
        public void SetSeed(int seed)
        {
            _random = seed > 0 ? new Random(seed) : new Random();
        }

        /// <summary>
        /// 生成房间布局
        /// </summary>
        public List<DungeonRoom> GenerateRoomLayout(int count, DungeonTypeConfig config, int floorNumber, bool isBossFloor)
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

            // 连接房间
            ConnectRooms(rooms);

            return rooms;
        }

        /// <summary>
        /// 选择房间类型
        /// </summary>
        public RoomType SelectRoomType(DungeonTypeConfig config)
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
        public DungeonRoom CreateRoom(RoomType type, DungeonTypeConfig config, int floor, int gridX, int gridY)
        {
            var templates = _database.GetRoomTemplates(type);
            var template = templates.Count > 0 ? templates[_random.Next(templates.Count)] : null;

            var difficulty = DungeonDifficultySystem.Instance?.CalculateDifficulty(floor, config) ?? RoomDifficulty.Normal;

            var room = new DungeonRoom
            {
                RoomId = Guid.NewGuid().ToString(),
                Type = type,
                Difficulty = difficulty,
                Width = template?.Width ?? 15,
                Height = template?.Height ?? 15,
                GridX = gridX,
                GridY = gridY
            };

            // 根据房间类型生成内容 - 委托给难度系统
            if (DungeonDifficultySystem.Instance != null)
            {
                DungeonDifficultySystem.Instance.PopulateRoomContent(room, type, floor, config);
            }

            // 添加房间修正因子
            room.RoomModifiers["enemy_strength"] = config.EnemyStrengthMultiplier;
            room.RoomModifiers["treasure_value"] = config.TreasureMultiplier;

            return room;
        }

        /// <summary>
        /// 找到可用位置
        /// </summary>
        public (int, int) FindAvailablePosition(HashSet<(int, int)> used, int attempt)
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
        public void ConnectRooms(List<DungeonRoom> rooms)
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
        /// 查找房间
        /// </summary>
        public DungeonRoom FindRoomById(List<DungeonFloor> floors, string roomId)
        {
            foreach (var floor in floors)
            {
                var room = floor.Rooms.FirstOrDefault(r => r.RoomId == roomId);
                if (room != null) return room;
            }
            return null;
        }

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary();
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            // 无需持久化数据
        }
    }
}
