using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 合作冒险会话状态
    /// </summary>
    public enum CoopSessionState
    {
        None,
        Forming,      // 组队中
        Starting,     // 即将开始
        InProgress,   // 进行中
        Paused,       // 暂停
        Completed,    // 完成
        Failed,       // 失败
        Cancelled     // 取消
    }

    /// <summary>
    /// 合作冒险类型
    /// </summary>
    public enum CoopAdventureType
    {
        Standard,     // 标准冒险
        Rush,         // 速通模式
        Challenge,    // 挑战模式
        Event         // 活动模式
    }

    /// <summary>
    /// 玩家在合作会话中的状态
    /// </summary>
    public enum CoopPlayerState
    {
        Waiting,      // 等待中
        Ready,        // 已准备
        InDungeon,    // 在地下城中
        Dead,         // 已死亡
        Disconnected  // 断开连接
    }

    /// <summary>
    /// 合作会话中的玩家数据
    /// </summary>
    public class CoopPlayerData
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        public int Level { get; set; }
        public int ClassId { get; set; }
        public CoopPlayerState State { get; set; }
        public float HealthPercent { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public string CurrentRoomId { get; set; } = "";
        public DateTime LastUpdate { get; set; }
        
        // 贡献度追踪
        public int DamageDealt { get; set; }
        public int HealingDone { get; set; }
        public int EnemiesKilled { get; set; }
        public int TimesRevived { get; set; }
        public int TreasuresCollected { get; set; }
        public int RoomsExplored { get; set; }
        
        public CoopPlayerData()
        {
            State = CoopPlayerState.Waiting;
            HealthPercent = 1.0f;
            LastUpdate = DateTime.Now;
        }
    }

    /// <summary>
    /// 合作会话中的队伍数据
    /// </summary>
    public class CoopPartyData
    {
        public string PartyId { get; set; } = "";
        public string PartyName { get; set; } = "";
        public int LeaderId { get; set; }
        public List<CoopPlayerData> Members { get; set; }
        
        public CoopPartyData()
        {
            Members = new List<CoopPlayerData>();
        }
    }

    /// <summary>
    /// 合作冒险会话
    /// </summary>
    public class CoopSession
    {
        public string SessionId { get; set; } = "";
        public string SessionName { get; set; } = "";
        public CoopAdventureType AdventureType { get; set; }
        public CoopSessionState State { get; set; }
        
        // 关联的队伍
        public CoopPartyData Party { get; set; }
        
        // 地下城信息
        public string DungeonId { get; set; } = "";
        public string DungeonName { get; set; } = "";
        public int CurrentFloor { get; set; }
        public int TotalFloors { get; set; }
        public string CurrentRoomId { get; set; } = "";
        
        // 会话配置
        public int MaxPlayers { get; set; }
        public bool IsQuickMode { get; set; }
        public int TimeLimitMinutes { get; set; }
        public float ExpMultiplier { get; set; }
        public float DropRateMultiplier { get; set; }
        
        // 计时
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan ElapsedTime { get; set; }
        
        // 进度追踪
        public int TotalRoomsCleared { get; set; }
        public int TotalEnemiesDefeated { get; set; }
        public int TotalTreasuresFound { get; set; }
        public int TotalSecretsDiscovered { get; set; }
        
        // 创建者信息
        public int CreatorId { get; set; }
        public string CreatorName { get; set; } = "";
        public DateTime CreatedAt { get; set; }
        
        public CoopSession()
        {
            Party = new CoopPartyData();
            State = CoopSessionState.Forming;
            CurrentFloor = 1;
            ExpMultiplier = 1.0f;
            DropRateMultiplier = 1.0f;
            CreatedAt = DateTime.Now;
        }
    }

    /// <summary>
    /// 收益分配记录
    /// </summary>
    public class RewardDistribution
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; } = "";
        
        // 贡献度分数 (0-100)
        public float ContributionScore { get; set; }
        
        // 分配比例
        public float ShareRatio { get; set; }
        
        // 获得的经验
        public int BaseExp { get; set; }
        public int BonusExp { get; set; }
        public int TotalExp { get; set; }
        
        // 获得的金币
        public int BaseGold { get; set; }
        public int BonusGold { get; set; }
        public int TotalGold { get; set; }
        
        // 获得的物品
        public List<string> Items { get; set; }
        
        public RewardDistribution()
        {
            Items = new List<string>();
        }
    }

    /// <summary>
    /// 收益分配结果
    /// </summary>
    public class CoopRewardResult
    {
        public string SessionId { get; set; } = "";
        public bool Success { get; set; }
        public bool IsVictory { get; set; }
        public TimeSpan CompletionTime { get; set; }
        public int TotalExp { get; set; }
        public int TotalGold { get; set; }
        public List<RewardDistribution> Distributions { get; set; }
        public List<string> SharedItems { get; set; }
        
        public CoopRewardResult()
        {
            Distributions = new List<RewardDistribution>();
            SharedItems = new List<string>();
        }
    }

    /// <summary>
    /// 合作会话配置
    /// </summary>
    public class CoopSessionConfig
    {
        // 标准配置
        public static readonly CoopSessionConfig Standard = new CoopSessionConfig
        {
            MaxPlayers = 4,
            IsQuickMode = false,
            TimeLimitMinutes = 60,
            ExpMultiplier = 1.0f,
            DropRateMultiplier = 1.0f
        };
        
        // 快速模式配置
        public static readonly CoopSessionConfig QuickMode = new CoopSessionConfig
        {
            MaxPlayers = 4,
            IsQuickMode = true,
            TimeLimitMinutes = 20,
            ExpMultiplier = 1.5f,
            DropRateMultiplier = 1.2f
        };
        
        // 双人模式配置
        public static readonly CoopSessionConfig Duo = new CoopSessionConfig
        {
            MaxPlayers = 2,
            IsQuickMode = false,
            TimeLimitMinutes = 45,
            ExpMultiplier = 1.2f,
            DropRateMultiplier = 1.1f
        };
        
        // 团队模式配置
        public static readonly CoopSessionConfig Raid = new CoopSessionConfig
        {
            MaxPlayers = 8,
            IsQuickMode = false,
            TimeLimitMinutes = 90,
            ExpMultiplier = 1.5f,
            DropRateMultiplier = 1.5f
        };
        
        public int MaxPlayers { get; set; }
        public bool IsQuickMode { get; set; }
        public int TimeLimitMinutes { get; set; }
        public float ExpMultiplier { get; set; }
        public float DropRateMultiplier { get; set; }
    }

    /// <summary>
    /// 玩家合作会话历史
    /// </summary>
    public class CoopSessionHistory
    {
        public int PlayerId { get; set; }
        public List<CoopSessionRecord> Sessions { get; set; }
        
        // 统计数据
        public int TotalSessionsJoined { get; set; }
        public int TotalSessionsCompleted { get; set; }
        public int TotalSessionsWon { get; set; }
        public int TotalExpEarned { get; set; }
        public int TotalGoldEarned { get; set; }
        
        public CoopSessionHistory()
        {
            Sessions = new List<CoopSessionRecord>();
        }
    }

    /// <summary>
    /// 单次会话记录
    /// </summary>
    public class CoopSessionRecord
    {
        public string SessionId { get; set; } = "";
        public string DungeonName { get; set; } = "";
        public CoopAdventureType AdventureType { get; set; }
        public bool WasVictory { get; set; }
        public int FloorReached { get; set; }
        public TimeSpan Duration { get; set; }
        public int ExpEarned { get; set; }
        public int GoldEarned { get; set; }
        public DateTime PlayedAt { get; set; }
    }
}
