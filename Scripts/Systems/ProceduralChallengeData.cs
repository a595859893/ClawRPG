using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 随机挑战数据结构
/// 包含挑战模板、稀有度、活跃挑战等
/// </summary>
public class ProceduralChallengeData
{
    public enum ChallengeType
    {
        KillEnemies,
        SurviveWaves,
        CollectItems,
        ReachLocation,
        DefeatBoss,
        TimeTrial,
        NoDamage,
        LimitedResources,
        SoloChallenge,
        Endurance
    }

    public enum ChallengeRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    public enum ChallengeStatus
    {
        Locked,
        Available,
        InProgress,
        Completed,
        Failed
    }

    [Serializable]
    public class ChallengeTemplate
    {
        public string Id;
        public string Name;
        public string Description;
        public ChallengeType Type;
        public ChallengeRarity Rarity;
        public int BaseRequirement;
        public int BaseTimeLimit; // seconds, 0 = no limit
        public int BaseGoldReward;
        public int BaseExpReward;
        public string[] RequiredItems;
        public string[] BonusItems;
    }

    [Serializable]
    public class ActiveChallenge
    {
        public string InstanceId;
        public string TemplateId;
        public ChallengeType Type;
        public ChallengeRarity Rarity;
        public int CurrentProgress;
        public int TargetProgress;
        public int TimeRemaining; // seconds
        public int TimeLimit;
        public ChallengeStatus Status;
        public int GoldReward;
        public int ExpReward;
        public List<string> BonusItems;
        public DateTime StartTime;
    }

    [Serializable]
    public class PlayerChallengeData
    {
        public int TotalChallengesCompleted;
        public int TotalGoldEarned;
        public int TotalExpEarned;
        public Dictionary<string, int> CompletedByType = new Dictionary<string, int>();
        public Dictionary<string, int> CompletedByRarity = new Dictionary<string, int>();
    }
}
