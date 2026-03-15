using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.CoopSession
{
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
}
