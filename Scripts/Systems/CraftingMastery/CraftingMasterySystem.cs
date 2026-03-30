using Godot;
using System;
using System.Collections.Generic;

public class CraftingMasterySystem : BaseSystem
{
    // Singleton instance
    public static CraftingMasterySystem Instance { get; private set; }

    // Mastery data per crafting type
    private Dictionary<CraftingType, MasteryData> masteryData = new Dictionary<CraftingType, MasteryData>();

    // Mastery levels
    public const int MAX_LEVEL = 100;
    public const int MASTER_LEVEL = 50;

    // Crafting types
    public enum CraftingType
    {
        Alchemy,
        Cooking,
        Fishing,
        Enchantment,
        Smithing,
        Tailoring,
        Jeweler,
        Herbalism,
        Mining,
        Woodcutting
    }

    // Mastery tier
    public enum MasteryTier
    {
        Novice,      // 0-9
        Apprentice,  // 10-24
        Journeyman,  // 25-39
        Expert,      // 40-49
        Master,      // 50-74
        GrandMaster, // 75-99
        Legend       // 100
    }

    [System.Serializable]
    public class MasteryData
    {
        public CraftingType Type;
        public int Level;
        public int TotalCrafts;
        public int SuccessfulCrafts;
        public int HighestRarityCrafted;
        public int TotalExperience;
        public DateTime LastCraftTime;

        public MasteryData(CraftingType type)
        {
            Type = type;
            Level = 0;
            TotalCrafts = 0;
            SuccessfulCrafts = 0;
            HighestRarityCrafted = 0;
            TotalExperience = 0;
            LastCraftTime = DateTime.MinValue;
        }

        public MasteryTier GetTier()
        {
            if (Level >= 100) return MasteryTier.Legend;
            if (Level >= 75) return MasteryTier.GrandMaster;
            if (Level >= 50) return MasteryTier.Master;
            if (Level >= 40) return MasteryTier.Expert;
            if (Level >= 25) return MasteryTier.Journeyman;
            if (Level >= 10) return MasteryTier.Apprentice;
            return MasteryTier.Novice;
        }

        public float GetSuccessRate()
        {
            if (TotalCrafts == 0) return 0f;
            return (float)SuccessfulCrafts / TotalCrafts * 100f;
        }

        public int GetExperienceForNextLevel()
        {
            return (Level + 1) * (Level + 1) * 100;
        }

        public bool HasLeveledUp()
        {
            return TotalExperience >= GetExperienceForNextLevel();
        }
    }

    public override void _Ready()
    {
        Instance = this;
        InitializeMasteryData();
    }

    private void InitializeMasteryData()
    {
        foreach (CraftingType type in Enum.GetValues(typeof(CraftingType)))
        {
            if (!masteryData.ContainsKey(type))
            {
                masteryData[type] = new MasteryData(type);
            }
        }
    }

    // Record a craft action
    public void RecordCraft(CraftingType type, bool success, int rarity)
    {
        if (!masteryData.ContainsKey(type))
        {
            masteryData[type] = new MasteryData(type);
        }

        MasteryData data = masteryData[type];
        data.TotalCrafts++;
        if (success)
        {
            data.SuccessfulCrafts++;
            data.TotalExperience += rarity * 10 + 5;
        }

        if (rarity > data.HighestRarityCrafted)
        {
            data.HighestRarityCrafted = rarity;
        }

        data.LastCraftTime = DateTime.Now;

        // Check for level up
        while (data.HasLeveledUp() && data.Level < MAX_LEVEL)
        {
            data.Level++;
            GD.Print($"[CraftingMastery] {type} leveled up to {data.Level}!");
            // Emit signal for level up
        }

        SaveMasteryData();
    }

    // Get mastery bonus for a crafting type
    public float GetMasteryBonus(CraftingType type, string bonusType)
    {
        if (!masteryData.ContainsKey(type)) return 0f;

        MasteryData data = masteryData[type];
        float bonus = 0f;

        switch (bonusType)
        {
            case "success_rate":
                // 0.5% bonus per level
                bonus = data.Level * 0.5f;
                break;
            case "quality":
                // Higher chance for better rarities
                bonus = data.Level * 0.1f;
                break;
            case "speed":
                // Faster crafting time
                bonus = data.Level * 0.2f;
                break;
            case "cost":
                // Material cost reduction
                bonus = data.Level * 0.15f;
                break;
            case "experience":
                // Extra experience gain
                bonus = data.Level * 0.3f;
                break;
        }

        return bonus;
    }

    // Get mastery data
    public MasteryData GetMasteryData(CraftingType type)
    {
        if (!masteryData.ContainsKey(type))
        {
            masteryData[type] = new MasteryData(type);
        }
        return masteryData[type];
    }

    // Get all mastery data
    public Dictionary<CraftingType, MasteryData> GetAllMasteryData()
    {
        return masteryData;
    }

    // Calculate total mastery level across all types
    public int GetTotalMasteryLevel()
    {
        int total = 0;
        foreach (var data in masteryData.Values)
        {
            total += data.Level;
        }
        return total;
    }

    // Get overall mastery tier
    public MasteryTier GetOverallMasteryTier()
    {
        int totalLevel = GetTotalMasteryLevel();
        int avgLevel = totalLevel / Enum.GetValues(typeof(CraftingType)).Length;
        
        if (avgLevel >= 100) return MasteryTier.Legend;
        if (avgLevel >= 75) return MasteryTier.GrandMaster;
        if (avgLevel >= 50) return MasteryTier.Master;
        if (avgLevel >= 40) return MasteryTier.Expert;
        if (avgLevel >= 25) return MasteryTier.Journeyman;
        if (avgLevel >= 10) return MasteryTier.Apprentice;
        return MasteryTier.Novice;
    }

    // Get mastery title
    public string GetMasteryTitle(CraftingType type)
    {
        if (!masteryData.ContainsKey(type)) return "Unknown";
        
        MasteryData data = masteryData[type];
        string tierName = data.GetTier().ToString();
        return $"{tierName} {type}";
    }

    // Check if player is a master in any category
    public bool IsMasterInAnyCategory()
    {
        foreach (var data in masteryData.Values)
        {
            if (data.Level >= MASTER_LEVEL) return true;
        }
        return false;
    }

    // Get master categories count
    public int GetMasterCategoriesCount()
    {
        int count = 0;
        foreach (var data in masteryData.Values)
        {
            if (data.Level >= MASTER_LEVEL) count++;
        }
        return count;
    }

    // Save mastery data
    private void SaveMasteryData()
    {
        // Save to player data
        if (PlayerData.Instance != null)
        {
            // Convert mastery data to dictionary for saving
            Dictionary<string, Dictionary> saveData = new Dictionary<string, Dictionary>();
            foreach (var kvp in masteryData)
            {
                string key = kvp.Key.ToString();
                MasteryData data = kvp.Value;
                saveData[key] = new Dictionary
                {
                    { "Level", data.Level },
                    { "TotalCrafts", data.TotalCrafts },
                    { "SuccessfulCrafts", data.SuccessfulCrafts },
                    { "HighestRarityCrafted", data.HighestRarityCrafted },
                    { "TotalExperience", data.TotalExperience }
                };
            }
            PlayerData.Instance.SetData("crafting_mastery", saveData);
        }
    }

    // Load mastery data
    public void LoadMasteryData(Dictionary<string, Dictionary> data)
    {
        if (data == null) return;

        foreach (var kvp in data)
        {
            if (Enum.TryParse<CraftingType>(kvp.Key, out CraftingType type))
            {
                if (!masteryData.ContainsKey(type))
                {
                    masteryData[type] = new MasteryData(type);
                }

                Dictionary dict = kvp.Value;
                MasteryData mastery = masteryData[type];
                mastery.Level = (int)dict.Get("Level", 0);
                mastery.TotalCrafts = (int)dict.Get("TotalCrafts", 0);
                mastery.SuccessfulCrafts = (int)dict.Get("SuccessfulCrafts", 0);
                mastery.HighestRarityCrafted = (int)dict.Get("HighestRarityCrafted", 0);
                mastery.TotalExperience = (int)dict.Get("TotalExperience", 0);
            }
        }
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // Convert mastery data to serializable format
        var masteryDict = new Dictionary<string, object>();
        foreach (var kvp in masteryData)
        {
            var mastery = new Dictionary<string, object>();
            mastery["level"] = kvp.Value.Level;
            mastery["total_crafts"] = kvp.Value.TotalCrafts;
            mastery["successful_crafts"] = kvp.Value.SuccessfulCrafts;
            mastery["highest_rarity_crafted"] = kvp.Value.HighestRarityCrafted;
            mastery["total_experience"] = kvp.Value.TotalExperience;
            masteryDict[kvp.Key.ToString()] = mastery;
        }
        data["mastery_data"] = masteryDict;
        
        return data;
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("mastery_data"))
        {
            var masteryDict = (Dictionary)data["mastery_data"];
            foreach (var kvp in masteryDict)
            {
                if (Enum.TryParse<CraftingType>(kvp.Key.ToString(), out CraftingType type))
                {
                    if (!masteryData.ContainsKey(type))
                    {
                        masteryData[type] = new MasteryData(type);
                    }
                    
                    var mastery = (Dictionary)kvp.Value;
                    MasteryData md = masteryData[type];
                    md.Level = (int)mastery.Get("level", 0);
                    md.TotalCrafts = (int)mastery.Get("total_crafts", 0);
                    md.SuccessfulCrafts = (int)mastery.Get("successful_crafts", 0);
                    md.HighestRarityCrafted = (int)mastery.Get("highest_rarity_crafted", 0);
                    md.TotalExperience = (int)mastery.Get("total_experience", 0);
                }
            }
        }
    }

    // Reset mastery (for prestige)
    public void ResetMastery(CraftingType type)
    {
        if (masteryData.ContainsKey(type))
        {
            masteryData[type] = new MasteryData(type);
            SaveMasteryData();
        }
    }

    // Reset all mastery
    public void ResetAllMastery()
    {
        InitializeMasteryData();
        SaveMasteryData();
    }
}
