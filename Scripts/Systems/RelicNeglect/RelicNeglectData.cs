using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.RelicNeglect
{
    /// <summary>
    /// 遗物被遗弃感等级
    /// </summary>
    public enum RelicNeglectLevel
    {
        Active = 0,      // 被经常使用
        Wary = 1,       // 5+场未使用，开始警觉
        Neglected = 2,  // 10+场未使用，明显冷落
        Sorrowful = 3,  // 20+场未使用，哀伤状态
        Despairing = 4  // 30+场未使用，彻底绝望
    }

    /// <summary>
    /// 单个遗物的被遗弃感数据
    /// </summary>
    public class RelicNeglectEntry
    {
        public string RelicId { get; set; }
        public int TotalBattlesCarried { get; set; }
        public int ConsecutiveBattlesUnused { get; set; }
        public int TotalTimesActivated { get; set; }
        public long LastActivatedTimestamp { get; set; }
        public bool HasShownSorrowfulNarrative { get; set; }

        public RelicNeglectEntry()
        {
            RelicId = "";
            TotalBattlesCarried = 0;
            ConsecutiveBattlesUnused = 0;
            TotalTimesActivated = 0;
            LastActivatedTimestamp = 0;
            HasShownSorrowfulNarrative = false;
        }

        public RelicNeglectEntry(string relicId)
        {
            RelicId = relicId;
            TotalBattlesCarried = 0;
            ConsecutiveBattlesUnused = 0;
            TotalTimesActivated = 0;
            LastActivatedTimestamp = 0;
            HasShownSorrowfulNarrative = false;
        }

        /// <summary>
        /// 根据未使用场次计算当前视觉等级
        /// </summary>
        public RelicNeglectLevel GetVisualLevel()
        {
            if (ConsecutiveBattlesUnused >= 30) return RelicNeglectLevel.Despairing;
            if (ConsecutiveBattlesUnused >= 20) return RelicNeglectLevel.Sorrowful;
            if (ConsecutiveBattlesUnused >= 10) return RelicNeglectLevel.Neglected;
            if (ConsecutiveBattlesUnused >= 5) return RelicNeglectLevel.Wary;
            return RelicNeglectLevel.Active;
        }
    }

    /// <summary>
    /// 遗物被遗弃感数据库 — 管理所有遗物的冷落状态
    /// </summary>
    public static class RelicNeglectDatabase
    {
        private static readonly Dictionary<string, RelicNeglectEntry> _entries = new();
        private static bool _initialized = false;

        public static void Initialize()
        {
            if (_initialized) return;
            _initialized = true;
            GD.Print("[RelicNeglectDatabase] Initialized");
        }

        /// <summary>
        /// 获取或创建遗物的被遗弃感记录
        /// </summary>
        public static RelicNeglectEntry GetOrCreateEntry(string relicId)
        {
            if (!_entries.TryGetValue(relicId, out var entry))
            {
                entry = new RelicNeglectEntry(relicId);
                _entries[relicId] = entry;
            }
            return entry;
        }

        /// <summary>
        /// 获取遗物的当前视觉状态等级
        /// </summary>
        public static RelicNeglectLevel GetVisualState(string relicId)
        {
            var entry = GetOrCreateEntry(relicId);
            return entry.GetVisualLevel();
        }

        /// <summary>
        /// 获取遗物的被遗弃感数据
        /// </summary>
        public static RelicNeglectEntry GetNeglectEntry(string relicId)
        {
            return GetOrCreateEntry(relicId);
        }

        /// <summary>
        /// 获取所有被遗弃感记录
        /// </summary>
        public static Dictionary<string, RelicNeglectEntry> GetAllEntries()
        {
            return new Dictionary<string, RelicNeglectEntry>(_entries);
        }

        /// <summary>
        /// 清除所有数据（仅用于测试或新游戏）
        /// </summary>
        public static void Clear()
        {
            _entries.Clear();
        }

        /// <summary>
        /// 批量导入数据（用于 ImportSaveData）
        /// </summary>
        public static void LoadFromData(List<RelicNeglectEntry> entries)
        {
            _entries.Clear();
            if (entries == null) return;
            foreach (var entry in entries)
            {
                if (!string.IsNullOrEmpty(entry.RelicId))
                    _entries[entry.RelicId] = entry;
            }
        }

        /// <summary>
        /// 导出所有数据（用于 ExportSaveData）
        /// </summary>
        public static List<RelicNeglectEntry> ExportAll()
        {
            return new List<RelicNeglectEntry>(_entries.Values);
        }
    }

    /// <summary>
    /// 持久化数据结构 — 用于 ExportSaveData / ImportSaveData
    /// </summary>
    [System.Serializable]
    public class RelicNeglectSaveData
    {
        public List<RelicNeglectEntrySaveData> Entries { get; set; } = new();
    }

    /// <summary>
    /// 单条被遗弃感数据的持久化格式
    /// </summary>
    [System.Serializable]
    public class RelicNeglectEntrySaveData
    {
        public string RelicId;
        public int TotalBattlesCarried;
        public int ConsecutiveBattlesUnused;
        public int TotalTimesActivated;
        public long LastActivatedTimestamp;
        public bool HasShownSorrowfulNarrative;
    }
}
