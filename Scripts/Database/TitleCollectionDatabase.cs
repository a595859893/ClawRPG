using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class TitleCollectionDatabase : BaseSystem
{
    public static TitleCollectionDatabase Instance { get; private set; }
    
    private System.Collections.Generic.Dictionary<string, TitleCollectionData.Title> _titles = new();
    
    protected override void Initialize()
    {
        Instance = this;
        InitializeTitles();
        IsInitialized = true;
        GD.Print($"[TitleCollectionDatabase] Initialized with {_titles.Count} titles");
    }
    
    public override System.Collections.Generic.Dictionary<string, object> ExportSaveData()
    {
        return new System.Collections.Generic.Dictionary<string, object>();
    }
    
    public override void ImportSaveData(System.Collections.Generic.Dictionary<string, object> data)
    {
        // 称号数据是只读的，无状态需要持久化
    }
    
    private void InitializeTitles()
    {
        // Combat Titles - 15 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "combat_novice",
            Name = "Combat Novice",
            Description = "Complete your first combat encounter",
            Category = TitleCollectionData.TitleCategory.Combat,
            Rarity = TitleCollectionData.TitleRarity.Common,
            IconIndex = 0,
            Condition = TitleCollectionData.UnlockCondition.KillCount,
            ConditionValue = 1,
            SortOrder = 1
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "combat_veteran",
            Name = "Combat Veteran",
            Description = "Defeat 100 enemies",
            Category = TitleCollectionData.TitleCategory.Combat,
            Rarity = TitleCollectionData.TitleRarity.Uncommon,
            IconIndex = 1,
            Condition = TitleCollectionData.UnlockCondition.KillCount,
            ConditionValue = 100,
            SortOrder = 2
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "combat_master",
            Name = "Combat Master",
            Description = "Defeat 500 enemies",
            Category = TitleCollectionData.TitleCategory.Combat,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 2,
            Condition = TitleCollectionData.UnlockCondition.KillCount,
            ConditionValue = 500,
            SortOrder = 3
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "boss_slayer",
            Name = "Boss Slayer",
            Description = "Defeat 10 bosses",
            Category = TitleCollectionData.TitleCategory.Combat,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 3,
            Condition = TitleCollectionData.UnlockCondition.BossKill,
            ConditionValue = 10,
            SortOrder = 4
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "dragon_hunter",
            Name = "Dragon Hunter",
            Description = "Defeat 50 bosses",
            Category = TitleCollectionData.TitleCategory.Combat,
            Rarity = TitleCollectionData.TitleRarity.Epic,
            IconIndex = 4,
            Condition = TitleCollectionData.UnlockCondition.BossKill,
            ConditionValue = 50,
            SortOrder = 5
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "godslayer",
            Name = "Godslayer",
            Description = "Defeat 100 bosses",
            Category = TitleCollectionData.TitleCategory.Combat,
            Rarity = TitleCollectionData.TitleRarity.Mythic,
            IconIndex = 5,
            Condition = TitleCollectionData.UnlockCondition.BossKill,
            ConditionValue = 100,
            SortOrder = 6
        });
        
        // Exploration Titles - 12 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "explorer",
            Name = "Explorer",
            Description = "Discover 5 different locations",
            Category = TitleCollectionData.TitleCategory.Exploration,
            Rarity = TitleCollectionData.TitleRarity.Common,
            IconIndex = 10,
            Condition = TitleCollectionData.UnlockCondition.DungeonComplete,
            ConditionValue = 5,
            SortOrder = 10
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "adventurer",
            Name = "Adventurer",
            Description = "Complete 25 dungeons",
            Category = TitleCollectionData.TitleCategory.Exploration,
            Rarity = TitleCollectionData.TitleRarity.Uncommon,
            IconIndex = 11,
            Condition = TitleCollectionData.UnlockCondition.DungeonComplete,
            ConditionValue = 25,
            SortOrder = 11
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "dungeon_master",
            Name = "Dungeon Master",
            Description = "Complete 100 dungeons",
            Category = TitleCollectionData.TitleCategory.Exploration,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 12,
            Condition = TitleCollectionData.UnlockCondition.DungeonComplete,
            ConditionValue = 100,
            SortOrder = 12
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "legendary_explorer",
            Name = "Legendary Explorer",
            Description = "Complete all dungeon types",
            Category = TitleCollectionData.TitleCategory.Exploration,
            Rarity = TitleCollectionData.TitleRarity.Epic,
            IconIndex = 13,
            Condition = TitleCollectionData.UnlockCondition.DungeonComplete,
            ConditionValue = 250,
            SortOrder = 13
        });
        
        // Crafting Titles - 10 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "apprentice_crafter",
            Name = "Apprentice Crafter",
            Description = "Craft 10 items",
            Category = TitleCollectionData.TitleCategory.Crafting,
            Rarity = TitleCollectionData.TitleRarity.Common,
            IconIndex = 20,
            Condition = TitleCollectionData.UnlockCondition.CraftCount,
            ConditionValue = 10,
            SortOrder = 20
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "journeyman_crafter",
            Name = "Journeyman Crafter",
            Description = "Craft 100 items",
            Category = TitleCollectionData.TitleCategory.Crafting,
            Rarity = TitleCollectionData.TitleRarity.Uncommon,
            IconIndex = 21,
            Condition = TitleCollectionData.UnlockCondition.CraftCount,
            ConditionValue = 100,
            SortOrder = 21
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "master_crafter",
            Name = "Master Crafter",
            Description = "Craft 500 items",
            Category = TitleCollectionData.TitleCategory.Crafting,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 22,
            Condition = TitleCollectionData.UnlockCondition.CraftCount,
            ConditionValue = 500,
            SortOrder = 22
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "grandmaster_crafter",
            Name = "Grandmaster Crafter",
            Description = "Craft 1000 items",
            Category = TitleCollectionData.TitleCategory.Crafting,
            Rarity = TitleCollectionData.TitleRarity.Epic,
            IconIndex = 23,
            Condition = TitleCollectionData.UnlockCondition.CraftCount,
            ConditionValue = 1000,
            SortOrder = 23
        });
        
        // Achievement Titles - 10 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "achiever",
            Name = "Achiever",
            Description = "Unlock 10 achievements",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Common,
            IconIndex = 30,
            Condition = TitleCollectionData.UnlockCondition.AchievementCount,
            ConditionValue = 10,
            SortOrder = 30
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "high_achiever",
            Name = "High Achiever",
            Description = "Unlock 25 achievements",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Uncommon,
            IconIndex = 31,
            Condition = TitleCollectionData.UnlockCondition.AchievementCount,
            ConditionValue = 25,
            SortOrder = 31
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "completionist",
            Name = "Completionist",
            Description = "Unlock 50 achievements",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 32,
            Condition = TitleCollectionData.UnlockCondition.AchievementCount,
            ConditionValue = 50,
            SortOrder = 32
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "true_completionist",
            Name = "True Completionist",
            Description = "Unlock all achievements",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Mythic,
            IconIndex = 33,
            Condition = TitleCollectionData.UnlockCondition.AchievementCount,
            ConditionValue = 100,
            SortOrder = 33
        });
        
        // Level Titles - 8 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "novice",
            Name = "Novice",
            Description = "Reach level 10",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Common,
            IconIndex = 40,
            Condition = TitleCollectionData.UnlockCondition.LevelReach,
            ConditionValue = 10,
            SortOrder = 40
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "veteran",
            Name = "Veteran",
            Description = "Reach level 25",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Uncommon,
            IconIndex = 41,
            Condition = TitleCollectionData.UnlockCondition.LevelReach,
            ConditionValue = 25,
            SortOrder = 41
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "elite",
            Name = "Elite",
            Description = "Reach level 50",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 42,
            Condition = TitleCollectionData.UnlockCondition.LevelReach,
            ConditionValue = 50,
            SortOrder = 42
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "legend",
            Name = "Legend",
            Description = "Reach level 100",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Mythic,
            IconIndex = 43,
            Condition = TitleCollectionData.UnlockCondition.LevelReach,
            ConditionValue = 100,
            SortOrder = 43
        });
        
        // Social Titles - 8 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "socialite",
            Name = "Socialite",
            Description = "Make 10 trades",
            Category = TitleCollectionData.TitleCategory.Social,
            Rarity = TitleCollectionData.TitleRarity.Common,
            IconIndex = 50,
            Condition = TitleCollectionData.UnlockCondition.TradeCount,
            ConditionValue = 10,
            SortOrder = 50
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "merchant_prince",
            Name = "Merchant Prince",
            Description = "Make 100 trades",
            Category = TitleCollectionData.TitleCategory.Social,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 51,
            Condition = TitleCollectionData.UnlockCondition.TradeCount,
            ConditionValue = 100,
            SortOrder = 51
        });
        
        // PvP Titles - 8 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "fighter",
            Name = "Fighter",
            Description = "Win 10 PvP battles",
            Category = TitleCollectionData.TitleCategory.Combat,
            Rarity = TitleCollectionData.TitleRarity.Uncommon,
            IconIndex = 60,
            Condition = TitleCollectionData.UnlockCondition.PvPWins,
            ConditionValue = 10,
            SortOrder = 60
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "champion",
            Name = "Champion",
            Description = "Win 100 PvP battles",
            Category = TitleCollectionData.TitleCategory.Combat,
            Rarity = TitleCollectionData.TitleRarity.Epic,
            IconIndex = 61,
            Condition = TitleCollectionData.UnlockCondition.PvPWins,
            ConditionValue = 100,
            SortOrder = 61
        });
        
        // Wealth Titles - 6 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "wealthy",
            Name = "Wealthy",
            Description = "Earn 10,000 gold",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Common,
            IconIndex = 70,
            Condition = TitleCollectionData.UnlockCondition.GoldEarned,
            ConditionValue = 10000,
            SortOrder = 70
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "millionaire",
            Name = "Millionaire",
            Description = "Earn 1,000,000 gold",
            Category = TitleCollectionData.TitleCategory.Achievement,
            Rarity = TitleCollectionData.TitleRarity.Epic,
            IconIndex = 71,
            Condition = TitleCollectionData.UnlockCondition.GoldEarned,
            ConditionValue = 1000000,
            SortOrder = 71
        });
        
        // Special Titles - 10 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "first_blood",
            Name = "First Blood",
            Description = "Be the first to deal damage in a raid boss",
            Category = TitleCollectionData.TitleCategory.Special,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 80,
            Condition = TitleCollectionData.UnlockCondition.Custom,
            ConditionValue = 0,
            CustomConditionScript = "first_blood",
            SortOrder = 80
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "speed_demon",
            Name = "Speed Demon",
            Description = "Complete a dungeon in under 5 minutes",
            Category = TitleCollectionData.TitleCategory.Special,
            Rarity = TitleCollectionData.TitleRarity.Epic,
            IconIndex = 81,
            Condition = TitleCollectionData.UnlockCondition.Custom,
            ConditionValue = 0,
            CustomConditionScript = "speed_demon",
            SortOrder = 81
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "lucky_one",
            Name = "Lucky One",
            Description = "Get a legendary drop",
            Category = TitleCollectionData.TitleCategory.Special,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 82,
            Condition = TitleCollectionData.UnlockCondition.Custom,
            ConditionValue = 0,
            CustomConditionScript = "lucky_drop",
            SortOrder = 82
        });
        
        // Hidden Titles - 5 titles
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "secret_seeker",
            Name = "???",
            Description = "Find a secret area",
            Category = TitleCollectionData.TitleCategory.Hidden,
            Rarity = TitleCollectionData.TitleRarity.Rare,
            IconIndex = 90,
            Condition = TitleCollectionData.UnlockCondition.Custom,
            ConditionValue = 0,
            CustomConditionScript = "secret_area",
            IsHidden = true,
            SortOrder = 90
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "night_owl",
            Name = "Night Owl",
            Description = "Play during nighttime hours",
            Category = TitleCollectionData.TitleCategory.Hidden,
            Rarity = TitleCollectionData.TitleRarity.Uncommon,
            IconIndex = 91,
            Condition = TitleCollectionData.UnlockCondition.TimePlayed,
            ConditionValue = 3600, // 1 hour
            IsHidden = true,
            SortOrder = 91
        });
        
        RegisterTitle(new TitleCollectionData.Title
        {
            Id = "early_bird",
            Name = "Early Bird",
            Description = "Play during early morning hours",
            Category = TitleCollectionData.TitleCategory.Hidden,
            Rarity = TitleCollectionData.TitleRarity.Uncommon,
            IconIndex = 92,
            Condition = TitleCollectionData.UnlockCondition.TimePlayed,
            ConditionValue = 3600,
            IsHidden = true,
            SortOrder = 92
        });
    }
    
    private void RegisterTitle(TitleCollectionData.Title title)
    {
        _titles[title.Id] = title;
    }
    
    public TitleCollectionData.Title GetTitle(string id)
    {
        return _titles.ContainsKey(id) ? _titles[id] : null;
    }
    
    public System.Collections.Generic.Dictionary<string, TitleCollectionData.Title> GetAllTitles()
    {
        return new System.Collections.Generic.Dictionary<string, TitleCollectionData.Title>(_titles);
    }
    
    public List<TitleCollectionData.Title> GetTitlesByCategory(TitleCollectionData.TitleCategory category)
    {
        List<TitleCollectionData.Title> result = new();
        foreach (var title in _titles.Values)
        {
            if (title.Category == category)
                result.Add(title);
        }
        result.Sort((a, b) => a.SortOrder.CompareTo(b.SortOrder));
        return result;
    }
    
    public List<TitleCollectionData.Title> GetTitlesByRarity(TitleCollectionData.TitleRarity rarity)
    {
        List<TitleCollectionData.Title> result = new();
        foreach (var title in _titles.Values)
        {
            if (title.Rarity == rarity)
                result.Add(title);
        }
        return result;
    }
    
    public int GetTotalTitleCount() => _titles.Count;
    
    public int GetTitleCountByCategory(TitleCollectionData.TitleCategory category)
    {
        int count = 0;
        foreach (var title in _titles.Values)
        {
            if (title.Category == category)
                count++;
        }
        return count;
    }
}
