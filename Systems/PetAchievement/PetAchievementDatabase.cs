using System;
using System.Collections.Generic;

public class PetAchievementDatabase
{
    // Achievement definition
    public class AchievementDef
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PetAchievementData.AchievementType Type { get; set; }
        public PetAchievementData.AchievementRarity Rarity { get; set; }
        public int RequiredValue { get; set; }
        public int GoldReward { get; set; }
        public int ExpReward { get; set; }
    }
    
    // All achievement definitions
    public List<AchievementDef> AllAchievements { get; set; }
    
    // Achievement by type
    public Dictionary<PetAchievementData.AchievementType, List<AchievementDef>> AchievementsByType { get; set; }
    
    // Rarity colors
    public Dictionary<PetAchievementData.AchievementRarity, string> RarityColors { get; set; }
    
    // Rarity rewards
    public Dictionary<PetAchievementData.AchievementRarity, int> RarityGoldBonus { get; set; }
    
    public PetAchievementDatabase()
    {
        AllAchievements = new List<AchievementDef>();
        AchievementsByType = new Dictionary<PetAchievementData.AchievementType, List<AchievementDef>>();
        RarityColors = new Dictionary<PetAchievementData.AchievementRarity, string>();
        RarityGoldBonus = new Dictionary<PetAchievementData.AchievementRarity, int>();
        
        InitializeRarityConfig();
        InitializeAchievements();
    }
    
    private void InitializeRarityConfig()
    {
        // Rarity colors
        RarityColors[PetAchievementData.AchievementRarity.Common] = "#FFFFFF";
        RarityColors[PetAchievementData.AchievementRarity.Uncommon] = "#1EFF00";
        RarityColors[PetAchievementData.AchievementRarity.Rare] = "#0070DD";
        RarityColors[PetAchievementData.AchievementRarity.Epic] = "#A335EE";
        RarityColors[PetAchievementData.AchievementRarity.Legendary] = "#FF8000";
        
        // Rarity gold bonus
        RarityGoldBonus[PetAchievementData.AchievementRarity.Common] = 50;
        RarityGoldBonus[PetAchievementData.AchievementRarity.Uncommon] = 150;
        RarityGoldBonus[PetAchievementData.AchievementRarity.Rare] = 500;
        RarityGoldBonus[PetAchievementData.AchievementRarity.Epic] = 2000;
        RarityGoldBonus[PetAchievementData.AchievementRarity.Legendary] = 10000;
    }
    
    private void InitializeAchievements()
    {
        // Battle achievements
        AddAchievement("battle_first_victory", "First Victory", "Win your first battle with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Common, 1, 50, 100);
        AddAchievement("battle_10_victories", "Battle Novice", "Win 10 battles with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Common, 10, 100, 250);
        AddAchievement("battle_50_victories", "Battle Veteran", "Win 50 battles with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Uncommon, 50, 300, 500);
        AddAchievement("battle_100_victories", "Battle Master", "Win 100 battles with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Rare, 100, 800, 1000);
        AddAchievement("battle_500_victories", "Legendary Warrior", "Win 500 battles with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Epic, 500, 2000, 2500);
        AddAchievement("battle_1000_victories", "Unstoppable", "Win 1000 battles with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Legendary, 1000, 10000, 5000);
        
        // Enemy kills achievements
        AddAchievement("kills_10_enemies", "Monster Slayer", "Defeat 10 enemies with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Common, 10, 75, 150);
        AddAchievement("kills_50_enemies", "Monster Hunter", "Defeat 50 enemies with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Uncommon, 50, 200, 400);
        AddAchievement("kills_100_enemies", "Monster Champion", "Defeat 100 enemies with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Rare, 100, 600, 800);
        AddAchievement("kills_500_enemies", "Death Bringer", "Defeat 500 enemies with this pet", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Epic, 500, 1500, 2000);
        
        // Boss achievements
        AddAchievement("boss_first_kill", "Boss Slayer", "Defeat your first boss", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Uncommon, 1, 300, 500);
        AddAchievement("boss_5_kills", "Boss Hunter", "Defeat 5 bosses", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Rare, 5, 800, 1200);
        AddAchievement("boss_25_kills", "Boss Master", "Defeat 25 bosses", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Epic, 25, 2500, 3000);
        
        // Exploration achievements
        AddAchievement("explore_first_location", "Explorer", "Visit your first location with this pet", 
            PetAchievementData.AchievementType.Exploration, PetAchievementData.AchievementRarity.Common, 1, 50, 100);
        AddAchievement("explore_5_locations", "Adventurer", "Visit 5 different locations", 
            PetAchievementData.AchievementType.Exploration, PetAchievementData.AchievementRarity.Uncommon, 5, 200, 400);
        AddAchievement("explore_10_locations", "World Traveler", "Visit 10 different locations", 
            PetAchievementData.AchievementType.Exploration, PetAchievementData.AchievementRarity.Rare, 10, 600, 800);
        AddAchievement("explore_20_locations", "Grand Explorer", "Visit 20 different locations", 
            PetAchievementData.AchievementType.Exploration, PetAchievementData.AchievementRarity.Epic, 20, 2000, 2500);
        
        // Floor achievements
        AddAchievement("floor_5_reached", "Floor Climber", "Reach floor 5", 
            PetAchievementData.AchievementType.Exploration, PetAchievementData.AchievementRarity.Common, 5, 100, 200);
        AddAchievement("floor_10_reached", "Dungeon Delver", "Reach floor 10", 
            PetAchievementData.AchievementType.Exploration, PetAchievementData.AchievementRarity.Uncommon, 10, 300, 500);
        AddAchievement("floor_25_reached", "Deep Diver", "Reach floor 25", 
            PetAchievementData.AchievementType.Exploration, PetAchievementData.AchievementRarity.Rare, 25, 1000, 1500);
        AddAchievement("floor_50_reached", "Floor Master", "Reach floor 50", 
            PetAchievementData.AchievementType.Exploration, PetAchievementData.AchievementRarity.Epic, 50, 3000, 4000);
        
        // Social achievements
        AddAchievement("social_first_friend", "Social Butterfly", "Make your first friend", 
            PetAchievementData.AchievementType.Social, PetAchievementData.AchievementRarity.Common, 1, 50, 100);
        AddAchievement("social_5_friends", "Popular Pet", "Have 5 friends", 
            PetAchievementData.AchievementType.Social, PetAchievementData.AchievementRarity.Uncommon, 5, 200, 400);
        AddAchievement("social_10_friends", "Social Star", "Have 10 friends", 
            PetAchievementData.AchievementType.Social, PetAchievementData.AchievementRarity.Rare, 10, 600, 800);
        
        // Collection achievements
        AddAchievement("collect_first_item", "Collector", "Collect your first item", 
            PetAchievementData.AchievementType.Collection, PetAchievementData.AchievementRarity.Common, 1, 50, 100);
        AddAchievement("collect_10_items", "Item Hoarder", "Collect 10 items", 
            PetAchievementData.AchievementType.Collection, PetAchievementData.AchievementRarity.Uncommon, 10, 200, 400);
        AddAchievement("collect_50_items", "Treasure Hunter", "Collect 50 items", 
            PetAchievementData.AchievementType.Collection, PetAchievementData.AchievementRarity.Rare, 50, 800, 1000);
        AddAchievement("collect_100_items", "Treasure Master", "Collect 100 items", 
            PetAchievementData.AchievementType.Collection, PetAchievementData.AchievementRarity.Epic, 100, 2500, 3000);
        
        // Gold achievements
        AddAchievement("gold_100_earned", "Penny Pincher", "Earn 100 gold with this pet", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Common, 100, 50, 100);
        AddAchievement("gold_1000_earned", "Wealthy Pet", "Earn 1000 gold with this pet", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Uncommon, 1000, 200, 400);
        AddAchievement("gold_10000_earned", "Gold Magnet", "Earn 10000 gold with this pet", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Rare, 10000, 800, 1000);
        AddAchievement("gold_100000_earned", "Midas Touch", "Earn 100000 gold with this pet", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Epic, 100000, 3000, 4000);
        
        // Level achievements
        AddAchievement("level_5_reached", "Growing Up", "Reach level 5", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Common, 5, 100, 200);
        AddAchievement("level_10_reached", "Teenager", "Reach level 10", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Uncommon, 10, 300, 500);
        AddAchievement("level_25_reached", "Adult", "Reach level 25", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Rare, 25, 1000, 1500);
        AddAchievement("level_50_reached", "Elder", "Reach level 50", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Epic, 50, 3000, 4000);
        AddAchievement("level_100_reached", "Ancient One", "Reach level 100", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Legendary, 100, 10000, 10000);
        
        // Evolution achievements
        AddAchievement("evolve_first", "First Evolution", "Evolve your pet for the first time", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Uncommon, 1, 300, 500);
        AddAchievement("evolve_3_times", "Evolution Master", "Evolve 3 times", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Rare, 3, 1000, 1500);
        AddAchievement("evolve_legendary", "Legendary Being", "Reach legendary evolution", 
            PetAchievementData.AchievementType.Growth, PetAchievementData.AchievementRarity.Legendary, 1, 10000, 10000);
        
        // Special achievements
        AddAchievement("perfect_battle", "Perfect Victory", "Win a battle without taking damage", 
            PetAchievementData.AchievementType.Special, PetAchievementData.AchievementRarity.Rare, 1, 500, 800);
        AddAchievement("speed_run", "Speed Runner", "Win a battle in under 10 seconds", 
            PetAchievementData.AchievementType.Special, PetAchievementData.AchievementRarity.Rare, 1, 600, 900);
        AddAchievement("survival_expert", "Survivor", "Win a battle with less than 10% health", 
            PetAchievementData.AchievementType.Special, PetAchievementData.AchievementRarity.Epic, 1, 1500, 2000);
        AddAchievement("no_damage_run", "Untouchable", "Win 10 battles without taking any damage", 
            PetAchievementData.AchievementType.Special, PetAchievementData.AchievementRarity.Legendary, 10, 10000, 5000);
        
        // Critical achievements
        AddAchievement("crit_10_times", "Critical Striker", "Land 10 critical hits", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Common, 10, 100, 200);
        AddAchievement("crit_100_times", "Deadly Precision", "Land 100 critical hits", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Uncommon, 100, 400, 600);
        AddAchievement("crit_500_times", "Death Dealer", "Land 500 critical hits", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Rare, 500, 1200, 1500);
        
        // Combo achievements
        AddAchievement("combo_5", "Combo Starter", "Achieve a 5-hit combo", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Common, 5, 100, 200);
        AddAchievement("combo_10", "Combo Master", "Achieve a 10-hit combo", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Uncommon, 10, 300, 500);
        AddAchievement("combo_25", "Combo Legend", "Achieve a 25-hit combo", 
            PetAchievementData.AchievementType.Battle, PetAchievementData.AchievementRarity.Epic, 25, 2000, 2500);
    }
    
    private void AddAchievement(string id, string name, string description, 
        PetAchievementData.AchievementType type, PetAchievementData.AchievementRarity rarity,
        int requiredValue, int goldReward, int expReward)
    {
        var achievement = new AchievementDef
        {
            Id = id,
            Name = name,
            Description = description,
            Type = type,
            Rarity = rarity,
            RequiredValue = requiredValue,
            GoldReward = goldReward,
            ExpReward = expReward
        };
        
        AllAchievements.Add(achievement);
        
        if (!AchievementsByType.ContainsKey(type))
        {
            AchievementsByType[type] = new List<AchievementDef>();
        }
        AchievementsByType[type].Add(achievement);
    }
    
    public AchievementDef GetAchievement(string id)
    {
        foreach (var achievement in AllAchievements)
        {
            if (achievement.Id == id)
                return achievement;
        }
        return null;
    }
    
    public List<AchievementDef> GetAchievementsByType(PetAchievementData.AchievementType type)
    {
        if (AchievementsByType.ContainsKey(type))
            return AchievementsByType[type];
        return new List<AchievementDef>();
    }
    
    public string GetRarityColor(PetAchievementData.AchievementRarity rarity)
    {
        if (RarityColors.ContainsKey(rarity))
            return RarityColors[rarity];
        return "#FFFFFF";
    }
}
