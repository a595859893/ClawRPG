using System;
using System.Collections.Generic;

namespace ClawRPG.Core.Systems.GuildWar
{
    /// <summary>
    /// 公会战类型
    /// </summary>
    public enum GuildWarType
    {
        Territory,      // 领地战
        Resource,       // 资源战
        Elimination,    // 淘汰赛
        Conquest,       // 征服战
        Defense         // 防守战
    }

    /// <summary>
    /// 公会战状态
    /// </summary>
    public enum GuildWarState
    {
        Preparation,    // 准备阶段
        Registration,  // 报名阶段
        Active,        // 进行中
        Completed,     // 已结束
        Cancelled      // 已取消
    }

    /// <summary>
    /// 战斗结果
    /// </summary>
    public enum BattleResult
    {
        Victory,
        Defeat,
        Draw,
        Pending
    }

    /// <summary>
    /// 公会参战记录
    /// </summary>
    public class GuildWarParticipant
    {
        public string GuildId { get; set; }
        public string GuildName { get; set; }
        public int PowerLevel { get; set; }
        public int MembersOnline { get; set; }
        public int Score { get; set; }
        public int Kills { get; set; }
        public int Deaths { get; set; }
        public int ResourcesCaptured { get; set; }
        public BattleResult Result { get; set; }
        public int Rank { get; set; }
        public List<string> ActiveMembers { get; set; } = new List<string>();
    }

    /// <summary>
    /// 单场公会战
    /// </summary>
    public class GuildWar
    {
        public string WarId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public GuildWarType Type { get; set; }
        public GuildWarState State { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Duration { get; set; } // 分钟
        public int MaxGuilds { get; set; }
        public int MinGuildLevel { get; set; }
        public int EntryFee { get; set; }
        public int PrizePool { get; set; }
        public string MapId { get; set; }
        public List<GuildWarParticipant> Participants { get; set; } = new List<GuildWarParticipant>();
        public string WinnerId { get; set; }
        public Dictionary<string, int> ScoreBreakdown { get; set; } = new Dictionary<string, int>();
    }

    /// <summary>
    /// 公会战战斗记录
    /// </summary>
    public class GuildWarBattle
    {
        public string BattleId { get; set; }
        public string WarId { get; set; }
        public string AttackerGuildId { get; set; }
        public string DefenderGuildId { get; set; }
        public string AttackerName { get; set; }
        public string DefenderName { get; set; }
        public DateTime BattleTime { get; set; }
        public int AttackerScore { get; set; }
        public int DefenderScore { get; set; }
        public BattleResult Result { get; set; }
        public int Duration { get; set; } // 秒
    }

    /// <summary>
    /// 公会战统计
    /// </summary>
    public class GuildWarStatistics
    {
        public string GuildId { get; set; }
        public int TotalWars { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Draws { get; set; }
        public int TotalScore { get; set; }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int LongestWinStreak { get; set; }
        public int CurrentWinStreak { get; set; }
        public int HighestRank { get; set; }
        public int TotalPrizeEarned { get; set; }
        public List<string> WarIds { get; set; } = new List<string>();
    }

    /// <summary>
    /// 领地占领数据
    /// </summary>
    public class TerritoryControl
    {
        public string TerritoryId { get; set; }
        public string TerritoryName { get; set; }
        public string ControllingGuildId { get; set; }
        public string ControllingGuildName { get; set; }
        public DateTime CaptureTime { get; set; }
        public int DefenseLevel { get; set; }
        public int ResourceGeneration { get; set; } // 每小时
    }

    /// <summary>
    /// 公会战进度数据
    /// </summary>
    public class GuildWarProgress
    {
        public string PlayerId { get; set; }
        public List<string> RegisteredWarIds { get; set; } = new List<string>();
        public List<string> CompletedWarIds { get; set; } = new List<string>();
        public int TotalContributions { get; set; }
        public int PersonalScore { get; set; }
        public Dictionary<string, int> WarContributions { get; set; } = new Dictionary<string, int>();
    }
}
