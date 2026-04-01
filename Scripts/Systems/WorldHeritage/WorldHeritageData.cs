using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.WorldHeritage
{
    /// <summary>
    /// 世界遗产类型
    /// </summary>
    public enum HeritageType
    {
        BossConquest,        // Boss 征服痕迹
        SecretDiscovery,     // 秘密发现
        AchievementInscription // 成就铭刻
    }

    /// <summary>
    /// 区域 ID — 用于地图视觉变化
    /// </summary>
    public enum RegionId
    {
        None,
        Forest,
        Dungeon,
        BossArena,
        Tower,
        Cave,
        Castle
    }

    /// <summary>
    /// 单个世界遗产记录
    /// </summary>
    [System.Serializable]
    public class HeritageRecord
    {
        public string RecordId;           // 唯一 ID
        public HeritageType Type;        // 遗产类型
        public RegionId Region;          // 关联区域
        public string DisplayName;        // "森林之王"
        public string Description;        // 叙事描述
        public string VisualKey;          // 用于 UI 查找对应视觉资源
        public bool IsActive;             // 是否已激活（可显示）
        public int UnlockedRunIndex;      // 在第几次 run 时解锁
        public string SourceEvent;        // 触发来源 ("boss_fire_dragon", "secret_treasure_room")
    }

    /// <summary>
    /// 单次 run 的遗产快照 — 用于 run 结束时记录
    /// </summary>
    [System.Serializable]
    public class RunHeritageSnapshot
    {
        public int RunIndex;
        public bool Victory;
        public List<string> BossesDefeated;       // boss config IDs
        public List<string> SecretsDiscovered;     // secret event IDs
        public List<string> AchievementsUnlocked;  // achievement IDs
        public int TotalEnemiesDefeated;
        public int FloorsCleared;
    }

    /// <summary>
    /// 世界遗产保存数据
    /// </summary>
    [System.Serializable]
    public class WorldHeritageSaveData
    {
        public List<string> ActiveHeritageIds;            // 已激活的遗产 ID
        public int TotalRunsCompleted;
        public int TotalVictories;
        public Dictionary<string, int> BossKillCounts;   // 每个 boss 被击杀次数
        public Dictionary<string, int> SecretDiscoveryCounts; // 每个秘密被发现次数
        public RunHeritageSnapshot LastSnapshot;          // 最近一次 run 的快照（用于叙事）
    }

    /// <summary>
    /// 世界遗产数据库 — 预定义所有可解锁的遗产
    /// </summary>
    public class WorldHeritageDatabase
    {
        private static WorldHeritageDatabase _instance;
        public static WorldHeritageDatabase Instance => _instance ??= new WorldHeritageDatabase();

        private Dictionary<string, HeritageRecord> _records = new Dictionary<string, HeritageRecord>();

        public WorldHeritageDatabase()
        {
            InitializeRecords();
        }

        private void InitializeRecords()
        {
            // ============================================================
            // Boss 征服痕迹 — 击杀特定 Boss 后激活
            // ============================================================

            AddRecord(new HeritageRecord
            {
                RecordId = "boss_forest_guardian",
                Type = HeritageType.BossConquest,
                Region = RegionId.Forest,
                DisplayName = "🌲 森林守护者之陨",
                Description = "曾经守护这片森林的强大存在，如今只剩残骸。",
                VisualKey = "forest_boss_scar",
                IsActive = false,
                SourceEvent = "boss_forest_guardian"
            });

            AddRecord(new HeritageRecord
            {
                RecordId = "boss_dungeon_lord",
                Type = HeritageType.BossConquest,
                Region = RegionId.Dungeon,
                DisplayName = "⚔️ 地下城主的陨落",
                Description = "地下城的统治者倒下了，光明重新照进这片黑暗。",
                VisualKey = "dungeon_boss_scar",
                IsActive = false,
                SourceEvent = "boss_dungeon_lord"
            });

            AddRecord(new HeritageRecord
            {
                RecordId = "boss_fire_dragon",
                Type = HeritageType.BossConquest,
                Region = RegionId.Cave,
                DisplayName = "🔥 火焰巨龙的不灭余烬",
                Description = "龙的尸骨化作永恒燃烧的余烬，诉说着曾经的恐怖。",
                VisualKey = "cave_boss_scar",
                IsActive = false,
                SourceEvent = "boss_fire_dragon"
            });

            AddRecord(new HeritageRecord
            {
                RecordId = "boss_tower_guardian",
                Type = HeritageType.BossConquest,
                Region = RegionId.Tower,
                DisplayName = "🗼 塔顶守卫的残影",
                Description = "高塔之巅的守护者已去，往日的威严只留于废墟。",
                VisualKey = "tower_boss_scar",
                IsActive = false,
                SourceEvent = "boss_tower_guardian"
            });

            AddRecord(new HeritageRecord
            {
                RecordId = "boss_king_of_shadows",
                Type = HeritageType.BossConquest,
                Region = RegionId.Castle,
                DisplayName = "👑 暗影国王的终焉",
                Description = "暗影宫殿的王者陨落，光明终于穿透了永恒的黑暗。",
                VisualKey = "castle_boss_scar",
                IsActive = false,
                SourceEvent = "boss_king_of_shadows"
            });

            // ============================================================
            // 秘密发现 — 发现特定秘密房间/事件后激活
            // ============================================================

            AddRecord(new HeritageRecord
            {
                RecordId = "secret_treasure_room",
                Type = HeritageType.SecretDiscovery,
                Region = RegionId.Dungeon,
                DisplayName = "💎 隐藏宝库的秘密",
                Description = "你知道了一扇隐藏宝库门的位置……虽然里面的宝藏已被取走。",
                VisualKey = "secret_treasure_door",
                IsActive = false,
                SourceEvent = "secret_treasure_room"
            });

            AddRecord(new HeritageRecord
            {
                RecordId = "secret_healing_shrine",
                Type = HeritageType.SecretDiscovery,
                Region = RegionId.Forest,
                DisplayName = "✨ 治愈圣所的传说",
                Description = "你发现了圣所的入口……圣所的力量已经消散，但那宁静的氛围仍存。",
                VisualKey = "secret_shrine_effect",
                IsActive = false,
                SourceEvent = "secret_healing_shrine"
            });

            AddRecord(new HeritageRecord
            {
                RecordId = "secret_mysterious_merchant",
                Type = HeritageType.SecretDiscovery,
                Region = RegionId.Cave,
                DisplayName = "🎭 神秘商人的足迹",
                Description = "那个行踪诡秘的商人曾在这里停留……或许还会再来。",
                VisualKey = "secret_merchant_marker",
                IsActive = false,
                SourceEvent = "secret_mysterious_merchant"
            });

            // ============================================================
            // 成就铭刻 — 达成特定成就后激活
            // ============================================================

            AddRecord(new HeritageRecord
            {
                RecordId = "achieve_first_blood",
                Type = HeritageType.AchievementInscription,
                Region = RegionId.None,  // 主界面显示
                DisplayName = "🩸 首次杀戮的印记",
                Description = "你在这个世界上留下了第一道血色印记。",
                VisualKey = "achieve_first_blood_mark",
                IsActive = false,
                SourceEvent = "first_blood"
            });

            AddRecord(new HeritageRecord
            {
                RecordId = "achieve_no_hit_run",
                Type = HeritageType.AchievementInscription,
                Region = RegionId.None,
                DisplayName = "🌟 完美之证",
                Description = "无人能敌的身影，已被铭刻在世界的记忆中。",
                VisualKey = "achieve_no_hit_mark",
                IsActive = false,
                SourceEvent = "no_hit_run"
            });

            AddRecord(new HeritageRecord
            {
                RecordId = "achieve_100_combo",
                Type = HeritageType.AchievementInscription,
                Region = RegionId.None,
                DisplayName = "⚡ 百连成就",
                Description = "百连的传说已刻入世界遗产，后人将永远记得那辉煌一刻。",
                VisualKey = "achieve_combo_mark",
                IsActive = false,
                SourceEvent = "combo_100"
            });
        }

        private void AddRecord(HeritageRecord record)
        {
            _records[record.RecordId] = record;
        }

        public HeritageRecord GetRecord(string recordId)
        {
            return _records.ContainsKey(recordId) ? _records[recordId] : null;
        }

        public List<HeritageRecord> GetAllRecords()
        {
            return new List<HeritageRecord>(_records.Values);
        }

        public List<HeritageRecord> GetActiveRecords()
        {
            var result = new List<HeritageRecord>();
            foreach (var r in _records.Values)
            {
                if (r.IsActive)
                    result.Add(r);
            }
            return result;
        }

        public List<HeritageRecord> GetRecordsByRegion(RegionId region)
        {
            var result = new List<HeritageRecord>();
            foreach (var r in _records.Values)
            {
                if (r.Region == region && r.IsActive)
                    result.Add(r);
            }
            return result;
        }

        public void SetRecordActive(string recordId, int runIndex)
        {
            if (_records.ContainsKey(recordId))
            {
                _records[recordId].IsActive = true;
                _records[recordId].UnlockedRunIndex = runIndex;
            }
        }
    }
}
