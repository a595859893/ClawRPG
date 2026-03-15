using Godot;
using System;
using System.Collections.Generic;

public class AchievementData
{
    public enum AchievementCategory
    {
        Combat,
        Exploration,
        Collection,
        Social,
        Economy,
        Progression,
        Special
    }

    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    [Serializable]
    public class Achievement
    {
        public string id;
        public string name;
        public string description;
        public AchievementCategory category;
        public AchievementRarity rarity;
        public int requirement;
        public int currentProgress;
        public bool isUnlocked;
        public DateTime? unlockedAt;
        public int rewardGold;
        public int rewardExp;
    }

    public static string[] CategoryNames = new string[]
    {
        "Combat", "Exploration", "Collection", "Social", "Economy", "Progression", "Special"
    };

    public static string[] RarityNames = new string[]
    {
        "Common", "Uncommon", "Rare", "Epic", "Legendary"
    };

    public static string[] RarityColors = new string[]
    {
        "#9E9E9E", "#4CAF50", "#2196F3", "#9C27B0", "#FF9800"
    };

    public static Dictionary<AchievementRarity, float> RarityMultipliers = new Dictionary<AchievementRarity, float>()
    {
        { AchievementRarity.Common, 1.0f },
        { AchievementRarity.Uncommon, 1.5f },
        { AchievementRarity.Rare, 2.5f },
        { AchievementRarity.Epic, 4.0f },
        { AchievementRarity.Legendary, 7.0f }
    };
}

public class AchievementDatabase
{
    private static List<AchievementData.Achievement> _achievements;

    public static List<AchievementData.Achievement> GetAllAchievements()
    {
        if (_achievements == null)
        {
            InitializeAchievements();
        }
        return new List<AchievementData.Achievement>(_achievements);
    }

    private static void InitializeAchievements()
    {
        _achievements = new List<AchievementData.Achievement>();

        // Combat Achievements
        _achievements.Add(CreateAchievement("combat_kills_100", "Monster Slayer", "Defeat 100 monsters", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Common, 100, 50, 100));
        _achievements.Add(CreateAchievement("combat_kills_500", "Seasoned Warrior", "Defeat 500 monsters", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Uncommon, 500, 200, 500));
        _achievements.Add(CreateAchievement("combat_kills_1000", "Veteran Hunter", "Defeat 1,000 monsters", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Rare, 1000, 500, 1500));
        _achievements.Add(CreateAchievement("combat_kills_5000", "Master Slayer", "Defeat 5,000 monsters", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Epic, 5000, 2000, 5000));
        _achievements.Add(CreateAchievement("combat_kills_10000", "Legendary Champion", "Defeat 10,000 monsters", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Legendary, 10000, 10000, 20000));

        _achievements.Add(CreateAchievement("boss_kills_10", "Boss Hunter", "Defeat 10 bosses", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Uncommon, 10, 300, 800));
        _achievements.Add(CreateAchievement("boss_kills_50", "Boss Master", "Defeat 50 bosses", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Rare, 50, 1000, 2500));
        _achievements.Add(CreateAchievement("boss_kills_100", "Boss Legend", "Defeat 100 bosses", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Legendary, 100, 5000, 10000));

        _achievements.Add(CreateAchievement("pvp_wins_10", "Arena Fighter", "Win 10 PvP battles", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Uncommon, 10, 250, 600));
        _achievements.Add(CreateAchievement("pvp_wins_50", "Arena Champion", "Win 50 PvP battles", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Rare, 50, 1000, 2000));
        _achievements.Add(CreateAchievement("pvp_wins_100", "Arena Master", "Win 100 PvP battles", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Epic, 100, 3000, 6000));
        _achievements.Add(CreateAchievement("pvp_wins_500", "PvP Legend", "Win 500 PvP battles", 
            AchievementData.AchievementCategory.Combat, AchievementData.AchievementRarity.Legendary, 500, 15000, 30000));

        // Exploration Achievements
        _achievements.Add(CreateAchievement("explore_zones_5", "Explorer", "Discover 5 zones", 
            AchievementData.AchievementCategory.Exploration, AchievementData.AchievementRarity.Common, 5, 100, 200));
        _achievements.Add(CreateAchievement("explore_zones_10", "Adventurer", "Discover 10 zones", 
            AchievementData.AchievementCategory.Exploration, AchievementData.AchievementRarity.Uncommon, 10, 300, 600));
        _achievements.Add(CreateAchievement("explore_zones_20", "World Traveler", "Discover 20 zones", 
            AchievementData.AchievementCategory.Exploration, AchievementData.AchievementRarity.Rare, 20, 800, 1500));
        _achievements.Add(CreateAchievement("explore_zones_all", "Cartographer", "Discover all zones", 
            AchievementData.AchievementCategory.Exploration, AchievementData.AchievementRarity.Epic, 30, 3000, 5000));

        _achievements.Add(CreateAchievement("sealed_tower_10", "Tower Climber", "Reach floor 10 in Sealed Tower", 
            AchievementData.AchievementCategory.Exploration, AchievementData.AchievementRarity.Uncommon, 10, 500, 1000));
        _achievements.Add(CreateAchievement("sealed_tower_50", "Tower Master", "Reach floor 50 in Sealed Tower", 
            AchievementData.AchievementCategory.Exploration, AchievementData.AchievementRarity.Rare, 50, 2000, 4000));
        _achievements.Add(CreateAchievement("sealed_tower_100", "Tower Legend", "Reach floor 100 in Sealed Tower", 
            AchievementData.AchievementCategory.Exploration, AchievementData.AchievementRarity.Legendary, 100, 10000, 20000));

        // Collection Achievements
        _achievements.Add(CreateAchievement("pets_5", "Pet Collector", "Collect 5 unique pets", 
            AchievementData.AchievementCategory.Collection, AchievementData.AchievementRarity.Common, 5, 150, 300));
        _achievements.Add(CreateAchievement("pets_10", "Pet Master", "Collect 10 unique pets", 
            AchievementData.AchievementCategory.Collection, AchievementData.AchievementRarity.Rare, 10, 500, 1000));
        _achievements.Add(CreateAchievement("pets_all", "Pet Hoarder", "Collect all pets", 
            AchievementData.AchievementCategory.Collection, AchievementData.AchievementRarity.Legendary, 20, 5000, 10000));

        _achievements.Add(CreateAchievement("mounts_3", "Rider", "Obtain 3 mounts", 
            AchievementData.AchievementCategory.Collection, AchievementData.AchievementRarity.Common, 3, 100, 200));
        _achievements.Add(CreateAchievement("mounts_8", "Mount Master", "Obtain 8 mounts", 
            AchievementData.AchievementCategory.Collection, AchievementData.AchievementRarity.Rare, 8, 600, 1200));
        _achievements.Add(CreateAchievement("mounts_all", "Mount Legend", "Obtain all mounts", 
            AchievementData.AchievementCategory.Collection, AchievementData.AchievementRarity.Legendary, 15, 4000, 8000));

        _achievements.Add(CreateAchievement("equipment_50", "Armorer", "Collect 50 equipment pieces", 
            AchievementData.AchievementCategory.Collection, AchievementData.AchievementRarity.Uncommon, 50, 200, 400));
        _achievements.Add(CreateAchievement("equipment_200", "Weapon Master", "Collect 200 equipment pieces", 
            AchievementData.AchievementCategory.Collection, AchievementData.AchievementRarity.Rare, 200, 800, 1600));
        _achievements.Add(CreateAchievement("equipment_500", "Legendary Armory", "Collect 500 equipment pieces", 
            AchievementData.AchievementCategory.Collection, AchievementData.AchievementRarity.Epic, 500, 3000, 6000));

        // Social Achievements
        _achievements.Add(CreateAchievement("guild_join", "Guild Member", "Join a guild", 
            AchievementData.AchievementCategory.Social, AchievementData.AchievementRarity.Common, 1, 100, 200));
        _achievements.Add(CreateAchievement("guild_leader", "Guild Leader", "Become a guild leader", 
            AchievementData.AchievementCategory.Social, AchievementData.AchievementRarity.Rare, 1, 1000, 2000));
        _achievements.Add(CreateAchievement("friends_10", "Social Butterfly", "Make 10 friends", 
            AchievementData.AchievementCategory.Social, AchievementData.AchievementRarity.Uncommon, 10, 300, 600));
        _achievements.Add(CreateAchievement("friends_50", "Popular", "Make 50 friends", 
            AchievementData.AchievementCategory.Social, AchievementData.AchievementRarity.Rare, 50, 1500, 3000));

        // Economy Achievements
        _achievements.Add(CreateAchievement("gold_10000", "Wealthy", "Accumulate 10,000 gold", 
            AchievementData.AchievementCategory.Economy, AchievementData.AchievementRarity.Common, 10000, 100, 200));
        _achievements.Add(CreateAchievement("gold_100000", "Rich", "Accumulate 100,000 gold", 
            AchievementData.AchievementCategory.Economy, AchievementData.AchievementRarity.Uncommon, 100000, 400, 800));
        _achievements.Add(CreateAchievement("gold_1000000", "Millionaire", "Accumulate 1,000,000 gold", 
            AchievementData.AchievementCategory.Economy, AchievementData.AchievementRarity.Rare, 1000000, 2000, 4000));
        _achievements.Add(CreateAchievement("gold_10000000", "Tycoon", "Accumulate 10,000,000 gold", 
            AchievementData.AchievementCategory.Economy, AchievementData.AchievementRarity.Epic, 10000000, 8000, 15000));

        _achievements.Add(CreateAchievement("spend_50000", "Big Spender", "Spend 50,000 gold", 
            AchievementData.AchievementCategory.Economy, AchievementData.AchievementRarity.Uncommon, 50000, 300, 600));
        _achievements.Add(CreateAchievement("spend_500000", "Premium Customer", "Spend 500,000 gold", 
            AchievementData.AchievementCategory.Economy, AchievementData.AchievementRarity.Rare, 500000, 1500, 3000));

        // Progression Achievements
        _achievements.Add(CreateAchievement("level_10", "Rising Hero", "Reach level 10", 
            AchievementData.AchievementCategory.Progression, AchievementData.AchievementRarity.Common, 10, 100, 200));
        _achievements.Add(CreateAchievement("level_50", "Experienced Hero", "Reach level 50", 
            AchievementData.AchievementCategory.Progression, AchievementData.AchievementRarity.Uncommon, 50, 500, 1000));
        _achievements.Add(CreateAchievement("level_100", "Elite Hero", "Reach level 100", 
            AchievementData.AchievementCategory.Progression, AchievementData.AchievementRarity.Rare, 100, 2000, 4000));
        _achievements.Add(CreateAchievement("level_200", "Master Hero", "Reach level 200", 
            AchievementData.AchievementCategory.Progression, AchievementData.AchievementRarity.Epic, 200, 8000, 15000));

        _achievements.Add(CreateAchievement("skill_points_50", "Skillful", "Spend 50 skill points", 
            AchievementData.AchievementCategory.Progression, AchievementData.AchievementRarity.Uncommon, 50, 300, 600));
        _achievements.Add(CreateAchievement("skill_points_200", "Skill Master", "Spend 200 skill points", 
            AchievementData.AchievementCategory.Progression, AchievementData.AchievementRarity.Rare, 200, 1200, 2500));

        // Special Achievements
        _achievements.Add(CreateAchievement("first_blood", "First Blood", "Win your first battle", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Common, 1, 50, 100));
        _achievements.Add(CreateAchievement("playtime_1h", "Getting Started", "Play for 1 hour", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Common, 1, 100, 200));
        _achievements.Add(CreateAchievement("playtime_10h", "Dedicated Player", "Play for 10 hours", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Uncommon, 10, 400, 800));
        _achievements.Add(CreateAchievement("playtime_50h", "Veteran", "Play for 50 hours", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Rare, 50, 2000, 4000));
        _achievements.Add(CreateAchievement("playtime_100h", "Addicted", "Play for 100 hours", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Epic, 100, 8000, 15000));

        _achievements.Add(CreateAchievement("login_7", "Week Warrior", "Login 7 days in a row", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Uncommon, 7, 500, 1000));
        _achievements.Add(CreateAchievement("login_30", "Monthly Dedication", "Login 30 days in a row", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Rare, 30, 3000, 6000));

        _achievements.Add(CreateAchievement("craft_10", "Apprentice Crafter", "Craft 10 items", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Common, 10, 100, 200));
        _achievements.Add(CreateAchievement("craft_100", "Master Crafter", "Craft 100 items", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Rare, 100, 800, 1500));
        _achievements.Add(CreateAchievement("craft_500", "Legendary Artisan", "Craft 500 items", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Epic, 500, 4000, 8000));

        // ============ Quick Mode Achievements ============
        _achievements.Add(CreateAchievement("quick_first_win", "Quick Start", "Win your first Quick Mode game", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Common, 1, 100, 200));
        
        _achievements.Add(CreateAchievement("quick_wins_10", "Quick Fighter", "Win 10 Quick Mode games", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Uncommon, 10, 300, 600));
        
        _achievements.Add(CreateAchievement("quick_wins_50", "Quick Veteran", "Win 50 Quick Mode games", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Rare, 50, 1000, 2000));
        
        _achievements.Add(CreateAchievement("quick_wins_100", "Quick Master", "Win 100 Quick Mode games", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Epic, 100, 3000, 6000));
        
        _achievements.Add(CreateAchievement("quick_wins_500", "Quick Legend", "Win 500 Quick Mode games", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Legendary, 500, 10000, 20000));
        
        _achievements.Add(CreateAchievement("quick_streak_5", "Quick Streak", "Win 5 Quick Mode games in a row", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Uncommon, 5, 500, 1000));
        
        _achievements.Add(CreateAchievement("quick_streak_10", "Unstoppable", "Win 10 Quick Mode games in a row", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Rare, 10, 1500, 3000));
        
        _achievements.Add(CreateAchievement("quick_streak_25", "Godlike", "Win 25 Quick Mode games in a row", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Epic, 25, 5000, 10000));
        
        _achievements.Add(CreateAchievement("quick_speed_5", "Speed Demon", "Complete 5 Quick Mode games under target time", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Uncommon, 5, 400, 800));
        
        _achievements.Add(CreateAchievement("quick_speed_10", "Lightning Fast", "Complete 10 Quick Mode games under target time", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Rare, 10, 1200, 2500));
        
        _achievements.Add(CreateAchievement("quick_speed_25", "Time Warp", "Complete 25 Quick Mode games under target time", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Epic, 25, 4000, 8000));
        
        _achievements.Add(CreateAchievement("quick_perfect_3", "Flawless", "Complete 3 Quick Mode games without taking damage", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Rare, 3, 1500, 3000));
        
        _achievements.Add(CreateAchievement("quick_perfect_10", "Perfect Runner", "Complete 10 Quick Mode games without taking damage", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Epic, 10, 5000, 10000));
        
        _achievements.Add(CreateAchievement("quick_plays_50", "Quick Regular", "Play 50 Quick Mode games", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Uncommon, 50, 800, 1600));
        
        _achievements.Add(CreateAchievement("quick_plays_200", "Quick Addict", "Play 200 Quick Mode games", 
            AchievementData.AchievementCategory.Special, AchievementData.AchievementRarity.Rare, 200, 2500, 5000));
    }

    private static AchievementData.Achievement CreateAchievement(string id, string name, string desc, 
        AchievementData.AchievementCategory cat, AchievementData.AchievementRarity rarity, 
        int req, int gold, int exp)
    {
        return new AchievementData.Achievement
        {
            id = id,
            name = name,
            description = desc,
            category = cat,
            rarity = rarity,
            requirement = req,
            currentProgress = 0,
            isUnlocked = false,
            rewardGold = gold,
            rewardExp = exp
        };
    }
}
