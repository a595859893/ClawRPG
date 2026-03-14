using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Leaderboard {
    /// <summary>
    /// 排行榜类型
    /// </summary>
    public enum LeaderboardType {
        PlayerLevel,          // 玩家等级
        Gold,                 // 金币
        Achievements,         // 成就
        ArenaWins,            // 竞技场胜利
        DungeonCompleted,     // 地下城通关
        BossKills,           // Boss击杀
        PetStrength,         // 宠物强度
        CraftingMastery,     // 制作精通
        GuildPoints,         // 公会积分
        CrossServerRating,   // 跨服评分
        MythicPlusScore,     // 大秘境分数
        ComboChain,          // 连击链
        TotalDamage,         // 总伤害
        TotalHealing         // 总治疗
    }

    /// <summary>
    /// 时间周期
    /// </summary>
    public enum LeaderboardPeriod {
        AllTime,     // 全部时间
        Monthly,     // 本月
        Weekly,      // 本周
        Daily        // 今日
    }

    /// <summary>
    /// 排行榜条目
    /// </summary>
    public class LeaderboardEntry {
        public string PlayerId;
        public string PlayerName;
        public int Rank;
        public long Value;
        public int PreviousRank;
        public DateTime LastUpdated;
        public Dictionary<string, object> Metadata = new Dictionary<string, object>();
    }

    /// <summary>
    /// 排行榜数据
    /// </summary>
    public class LeaderboardData {
        public LeaderboardType Type;
        public LeaderboardPeriod Period;
        public List<LeaderboardEntry> Entries = new List<LeaderboardEntry>();
        public DateTime LastReset;
        public bool IsDirty = false;
    }

    /// <summary>
    /// 玩家排行榜数据
    /// </summary>
    public class PlayerLeaderboardData {
        public string PlayerId;
        public string PlayerName;
        public Dictionary<LeaderboardType, long> Scores = new Dictionary<LeaderboardType, long>();
        public Dictionary<LeaderboardType, int> Ranks = new Dictionary<LeaderboardType, int>();
        public Dictionary<LeaderboardType, int> PreviousRanks = new Dictionary<LeaderboardType, int>();
        public Dictionary<LeaderboardType, DateTime> LastUpdateTimes = new Dictionary<LeaderboardType, DateTime>();
    }

    /// <summary>
    /// 排行榜统计
    /// </summary>
    public class LeaderboardStatistics {
        public int TotalEntries;
        public long HighestScore;
        public string TopPlayerId;
        public string TopPlayerName;
        public DateTime LastUpdate;
    }

    /// <summary>
    /// 排行榜变化记录
    /// </summary>
    public class LeaderboardChange {
        public string PlayerId;
        public string PlayerName;
        public LeaderboardType Type;
        public int PreviousRank;
        public int NewRank;
        public long PreviousValue;
        public long NewValue;
        public DateTime Timestamp;
    }
}
