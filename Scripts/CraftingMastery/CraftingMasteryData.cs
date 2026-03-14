using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.CraftingMastery
{
    /// <summary>
    /// Crafting mastery level enumeration
    /// </summary>
    public enum CraftingMasteryLevel
    {
        Novice = 1,
        Apprentice = 2,
        Journeyman = 3,
        Expert = 4,
        Master = 5,
        GrandMaster = 6
    }
    
    /// <summary>
    /// Crafting category enumeration
    /// </summary>
    public enum CraftingCategory
    {
        Blacksmithing,
        Alchemy,
        Enchanting,
        Cooking,
        Leatherworking,
        Tailoring,
        Jewelcrafting,
        Inscription
    }
    
    /// <summary>
    /// Recipe difficulty level
    /// </summary>
    public enum RecipeDifficulty
    {
        Simple,
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    /// <summary>
    /// Data structure for crafting mastery
    /// </summary>
    [Serializable]
    public class CraftingMasteryData
    {
        public Dictionary<CraftingCategory, CategoryMastery> CategoryMasteries { get; set; } = new Dictionary<CraftingCategory, CategoryMastery>();
        public List<string> UnlockedRecipes { get; set; } = new List<string>();
        public List<CraftingSession> RecentSessions { get; set; } = new List<CraftingSession>();
        public int TotalCrafts { get; set; }
        public int SuccessfulCrafts { get; set; }
        public int FailedCrafts { get; set; }
        public int MasterpieceCrafts { get; set; }
        public DateTime LastCraftTime { get; set; }
    }
    
    /// <summary>
    /// Mastery data for a single crafting category
    /// </summary>
    [Serializable]
    public class CategoryMastery
    {
        public CraftingCategory Category { get; set; }
        public int Level { get; set; } = 1;
        public int Experience { get; set; }
        public int TotalCrafts { get; set; }
        public int SuccessfulCrafts { get; set; }
        public List<string> UnlockedRecipes { get; set; } = new List<string>();
        public Dictionary<string, int> RecipeUsageCount { get; set; } = new Dictionary<string, int>();
        public DateTime LastCrafted { get; set; }
        public int CurrentStreak { get; set; }
        public int BestStreak { get; set; }
    }
    
    /// <summary>
    /// Record of a crafting session
    /// </summary>
    [Serializable]
    public class CraftingSession
    {
        public string RecipeId { get; set; }
        public CraftingCategory Category { get; set; }
        public DateTime Timestamp { get; set; }
        public bool Success { get; set; }
        public bool IsMasterpiece { get; set; }
        public int ExperienceGained { get; set; }
    }
    
    /// <summary>
    /// Recipe definition
    /// </summary>
    [Serializable]
    public class CraftingRecipe
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public CraftingCategory Category { get; set; }
        public RecipeDifficulty Difficulty { get; set; }
        public int RequiredLevel { get; set; }
        public int RequiredMasteryLevel { get; set; }
        public List<RecipeComponent> Components { get; set; } = new List<RecipeComponent>();
        public List<string> RequiredRecipes { get; set; } = new List<string>();
        public int ExperienceReward { get; set; }
        public int MasterpieceBonusExp { get; set; }
        public float SuccessRate { get; set; } = 1.0f;
        public float MasterpieceChance { get; set; } = 0.05f;
    }
    
    /// <summary>
    /// Component required for a recipe
    /// </summary>
    [Serializable]
    public class RecipeComponent
    {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int Quantity { get; set; }
    }
    
    /// <summary>
    /// Active crafting progress
    /// </summary>
    [Serializable]
    public class CraftingProgress
    {
        public string RecipeId { get; set; }
        public CraftingCategory Category { get; set; }
        public DateTime StartTime { get; set; }
        public int ProgressPercent { get; set; }
        public bool IsComplete { get; set; }
        public bool Success { get; set; }
        public bool IsMasterpiece { get; set; }
    }
    
    /// <summary>
    /// Statistics for crafting mastery
    /// </summary>
    [Serializable]
    public class CraftingStatistics
    {
        public int TotalCrafts { get; set; }
        public int SuccessfulCrafts { get; set; }
        public int FailedCrafts { get; set; }
        public int MasterpieceCrafts { get; set; }
        public float AverageSuccessRate { get; set; }
        public int BestStreak { get; set; }
        public Dictionary<CraftingCategory, int> CategoryCraftCounts { get; set; } = new Dictionary<CraftingCategory, int>();
        public Dictionary<CraftingCategory, int> CategoryMasteryLevels { get; set; } = new Dictionary<CraftingCategory, int>();
        public string MostUsedRecipe { get; set; }
        public int MostUsedRecipeCount { get; set; }
    }
}
