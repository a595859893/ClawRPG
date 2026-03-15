using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts
{
    /// <summary>
    /// 成就里程碑系统 - 数据结构
    /// </summary>
    
    // 成就类型
    public enum AchievementType
    {
        Combat,           // 战斗成就
        Exploration,      // 探索成就
        Crafting,         // 制作成就
        Collection,       // 收集成就
        Social,           // 社交成就
        Progress,         // 进度成就
        Challenge,        // 挑战成就
        Special           // 特殊成就
    }
    
    // 成就稀有度
    public enum AchievementRarity
    {
        Common,           // 普通
        Uncommon,         // 优秀
        Rare,             // 稀有
        Epic,             // 史诗
        Legendary         // 传说
    }
    
    // 单个成就数据
    public class Achievement
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AchievementType Type { get; set; }
        public AchievementRarity Rarity { get; set; }
        public int RequiredValue { get; set; }
        public int CurrentValue { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedTime { get; set; }
        public List<string> Rewards { get; set; }  // 奖励ID列表
        public bool RewardsClaimed { get; set; }
        public string IconName { get; set; }
        
        public Achievement()
        {
            Rewards = new List<string>();
            CurrentValue = 0;
            IsUnlocked = false;
            RewardsClaimed = false;
        }
        
        public float Progress => RequiredValue > 0 ? (float)CurrentValue / RequiredValue : 0f;
    }
    
    // 里程碑数据
    public class Milestone
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RequiredCount { get; set; }
        public int CurrentCount { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime? CompletedTime { get; set; }
        public List<string> Rewards { get; set; }
        public bool RewardsClaimed { get; set; }
        public string IconName { get; set; }
        
        public Milestone()
        {
            Rewards = new List<string>();
            CurrentCount = 0;
            IsCompleted = false;
            RewardsClaimed = false;
        }
        
        public float Progress => RequiredCount > 0 ? (float)CurrentCount / RequiredCount : 0f;
    }
    
    // 成就类别统计
    public class AchievementCategoryStats
    {
        public AchievementType Type { get; set; }
        public int TotalCount { get; set; }
        public int UnlockedCount { get; set; }
        public int ClaimedRewards { get; set; }
        
        public float CompletionRate => TotalCount > 0 ? (float)UnlockedCount / TotalCount : 0f;
    }
    
    // 玩家成就数据
    public class PlayerAchievementData
    {
        public Dictionary<string, Achievement> Achievements { get; set; }
        public Dictionary<string, Milestone> Milestones { get; set; }
        public List<string> RecentlyUnlocked { get; set; }  // 最近解锁的成就ID
        public int TotalAchievements { get; set; }
        public int UnlockedAchievements { get; set; }
        public int TotalMilestones { get; set; }
        public int CompletedMilestones { get; set; }
        public int ClaimedRewards { get; set; }
        
        public PlayerAchievementData()
        {
            Achievements = new Dictionary<string, Achievement>();
            Milestones = new Dictionary<string, Milestone>();
            RecentlyUnlocked = new List<string>();
        }
    }
    
    // 成就进度更新记录
    public class AchievementProgressUpdate
    {
        public string AchievementID { get; set; }
        public int OldValue { get; set; }
        public int NewValue { get; set; }
        public DateTime UpdateTime { get; set; }
    }
    
    // 里程碑进度更新记录
    public class MilestoneProgressUpdate
    {
        public string MilestoneID { get; set; }
        public int OldCount { get; set; }
        public int NewCount { get; set; }
        public DateTime UpdateTime { get; set; }
    }
    
    // 统计数据
    public class AchievementStatistics
    {
        public int TotalPlayTime { get; set; }  // 分钟
        public int TotalAchievementsUnlocked { get; set; }
        public int TotalMilestonesCompleted { get; set; }
        public int TotalRewardsClaimed { get; set; }
        public int GoldEarnedFromRewards { get; set; }
        public int ExpEarnedFromRewards { get; set; }
        public Dictionary<AchievementRarity, int> RarityBreakdown { get; set; }
        public Dictionary<AchievementType, int> TypeBreakdown { get; set; }
        
        public AchievementStatistics()
        {
            RarityBreakdown = new Dictionary<AchievementRarity, int>();
            TypeBreakdown = new Dictionary<AchievementType, int>();
        }
    }
}
