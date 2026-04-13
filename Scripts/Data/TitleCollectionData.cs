using Godot;
using System;
using System.Collections.Generic;

public partial class TitleCollectionData : Resource
{
    // Title categories
    public enum TitleCategory { Combat, Exploration, Crafting, Social, Achievement, Seasonal, Special, Hidden }
    
    // Title rarity
    public enum TitleRarity { Common, Uncommon, Rare, Epic, Legendary, Mythic }
    
    // Title unlock condition type
    public enum UnlockCondition { 
        KillCount, BossKill, DungeonComplete, CraftCount, 
        AchievementCount, LevelReach, GoldEarned, TradeCount,
        PvPWins, GuildRank, QuestComplete, TimePlayed, Custom
    }
    
    // Title definition
    public class Title
    {
        public string Id;
        public string Name;
        public string Description;
        public TitleCategory Category;
        public TitleRarity Rarity;
        public int IconIndex;
        public UnlockCondition Condition;
        public int ConditionValue;
        public string CustomConditionScript;
        public bool IsHidden;
        public bool IsPermanent;
        public int SortOrder;
    }
    
    // Player's title data
    public class PlayerTitleData
    {
        public string TitleId;
        public DateTime UnlockedAt;
        public bool IsActive;
        public DateTime? EquippedAt;
    }
    
    // Player's title collection
    public class PlayerTitleCollection
    {
        public Dictionary<string, PlayerTitleData> UnlockedTitles = new();
        public string ActiveTitleId;
        public int TotalUnlocked;
        public int CategoryUnlocked = new Dictionary<TitleCategory, int>
        {
            { TitleCategory.Combat, 0 }, { TitleCategory.Exploration, 0 },
            { TitleCategory.Crafting, 0 }, { TitleCategory.Social, 0 },
            { TitleCategory.Achievement, 0 }, { TitleCategory.Seasonal, 0 },
            { TitleCategory.Special, 0 }, { TitleCategory.Hidden, 0 }
        }.Count;
    }
    
    // Title statistics
    public class TitleStatistics
    {
        public int TotalUnlocked;
        public int TotalCategories;
        public int RarityBreakdown = new Dictionary<TitleRarity, int>
        {
            { TitleRarity.Common, 0 }, { TitleRarity.Uncommon, 0 },
            { TitleRarity.Rare, 0 }, { TitleRarity.Epic, 0 },
            { TitleRarity.Legendary, 0 }, { TitleRarity.Mythic, 0 }
        }.Count;
        public string MostRecentTitle;
        public DateTime? FirstUnlockTime;
    }
}
