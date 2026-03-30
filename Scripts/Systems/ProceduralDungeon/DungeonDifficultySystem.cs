using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Godot.Collections;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 地下城难度系统 - 负责难度计算、敌人生成、奖励生成
    /// </summary>
    public partial class DungeonDifficultySystem : BaseSystem
    {
        private static DungeonDifficultySystem _instance;
        public static DungeonDifficultySystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new DungeonDifficultySystem();
                return _instance;
            }
            private set { _instance = value; }
        }

        protected override string SystemName => "DungeonDifficultySystem";

        private Random _random;
        private ProceduralDungeonDatabase _database;

        public DungeonDifficultySystem()
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
        /// 计算房间难度
        /// </summary>
        public RoomDifficulty CalculateDifficulty(int floor, DungeonTypeConfig config)
        {
            float difficulty = floor * config.ThemeModifier;

            if (difficulty < 2) return RoomDifficulty.Easy;
            if (difficulty < 4) return RoomDifficulty.Normal;
            if (difficulty < 6) return RoomDifficulty.Hard;
            if (difficulty < 8) return RoomDifficulty.Nightmare;
            return RoomDifficulty.Legendary;
        }

        /// <summary>
        /// 填充房间内容
        /// </summary>
        public void PopulateRoomContent(DungeonRoom room, RoomType type, int floor, DungeonTypeConfig config)
        {
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
        }

        /// <summary>
        /// 生成敌人列表
        /// </summary>
        public List<string> GenerateEnemyList(RoomDifficulty difficulty, bool isElite)
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
        public List<string> GenerateBossEnemy(int floor)
        {
            return new List<string> { $"Boss_Floor{floor}" };
        }

        /// <summary>
        /// 选择宝藏
        /// </summary>
        public string SelectTreasure()
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
        public string SelectEvent()
        {
            var events = _database.Events;
            return events[_random.Next(events.Count)].EventId;
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
            // 无需持久化数据
        }
    }
}
