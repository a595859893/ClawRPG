using System;
using System.Collections.Generic;

public class PetAchievementData
{
    // Pet achievement types
    public enum AchievementType
    {
        Battle,
        Exploration,
        Social,
        Collection,
        Growth,
        Special
    }
    
    // Achievement rarity
    public enum AchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    // Single achievement record
    public class Achievement
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AchievementType Type { get; set; }
        public AchievementRarity Rarity { get; set; }
        public int RequiredValue { get; set; }
        public int CurrentValue { get; set; }
        public bool IsUnlocked { get; set; }
        public DateTime? UnlockedAt { get; set; }
    }
    
    // Achievement progress per pet
    public Dictionary<string, List<Achievement>> PetAchievements { get; set; }
    
    // Global achievements
    public List<Achievement> GlobalAchievements { get; set; }
    
    // Statistics
    public int TotalAchievementsUnlocked { get; set; }
    public int TotalAchievements { get; set; }
    public Dictionary<AchievementRarity, int> RarityBreakdown { get; set; }
    public int TotalRewardsClaimed { get; set; }
    public int TotalGoldEarned { get; set; }
    
    public PetAchievementData()
    {
        PetAchievements = new Dictionary<string, List<Achievement>>();
        GlobalAchievements = new List<Achievement>();
        RarityBreakdown = new Dictionary<AchievementRarity, int>();
        
        foreach (AchievementRarity rarity in Enum.GetValues(typeof(AchievementRarity)))
        {
            RarityBreakdown[rarity] = 0;
        }
    }
}
