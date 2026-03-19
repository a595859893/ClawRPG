using Godot;
/// <summary>
/// 风格精通系统。
/// </summary>
using System;
using System.Collections.Generic;

public class StyleMasterySystem : BaseSystem
{
    private StyleMasteryData data;
    private StyleMasteryDatabase database;
    
    public override void _Ready()
    {
        data = GetNode<StyleMasteryData>("/root/StyleMasteryData");
        database = GetNode<StyleMasteryDatabase>("/root/StyleMasteryDatabase");
        
        if (data == null)
        {
            GD.PrintErr("StyleMasteryData not found!");
            return;
        }
        
        if (database == null)
        {
            GD.PrintErr("StyleMasteryDatabase not found!");
            return;
        }
        
        GD.Print("Style Mastery System initialized");
    }
    
    // Get style bonuses based on active style and mastery level
    public Dictionary<string, float> GetStyleBonuses(string styleId)
    {
        var bonuses = new Dictionary<string, float>();
        
        if (!database.Styles.ContainsKey(styleId))
            return bonuses;
        
        var config = database.Styles[styleId];
        var record = data.GetStyle(styleId);
        
        if (record == null)
            return bonuses;
        
        float masteryMultiplier = 1.0f + (record.MasteryLevel - 1) * 0.05f;
        
        bonuses["attack"] = config.BaseAttackBonus * masteryMultiplier;
        bonuses["defense"] = config.BaseDefenseBonus * masteryMultiplier;
        bonuses["speed"] = config.BaseSpeedBonus * masteryMultiplier;
        bonuses["critical_rate"] = config.CriticalRateBonus * masteryMultiplier;
        bonuses["evasion"] = config.EvasionBonus * masteryMultiplier;
        bonuses["damage_reduction"] = config.DamageReduction * masteryMultiplier;
        
        return bonuses;
    }
    
    // Unlock a new style
    public bool UnlockStyle(string styleId, int playerLevel)
    {
        var unlockedStyles = new Dictionary<string, bool>();
        foreach (var style in data.GetMasteredStyles().Values)
        {
            unlockedStyles[style.StyleId] = style.MasteryLevel > 1;
        }
        
        if (database.CanUnlockStyle(styleId, playerLevel, unlockedStyles))
        {
            var record = data.GetStyle(styleId);
            if (record != null && record.MasteryLevel == 1)
            {
                record.MasteryLevel = 2; // Unlock by setting to level 2
                record.TotalXP += 100; // Bonus XP for unlocking
                data.SaveData();
                GD.Print("Style unlocked: " + styleId);
                return true;
            }
        }
        
        return false;
    }
    
    // Switch to a different style
    public void SwitchStyle(string styleId)
    {
        data.SetActiveStyle(styleId);
        GD.Print("Active style switched to: " + styleId);
    }
    
    // Add XP to a style after combat
    public void AddStyleXPFromCombat(string styleId, int enemiesDefeated, int damageDealt)
    {
        int xp = enemiesDefeated * 10 + damageDealt / 100;
        data.AddStyleXP(styleId, xp);
    }
    
    // Record enemy defeat with current active style
    public void RecordEnemyDefeat(string styleId)
    {
        data.RecordEnemyDefeated(styleId);
    }
    
    // Check if a style is available for unlock
    public bool IsStyleAvailable(string styleId, int playerLevel)
    {
        var unlockedStyles = new Dictionary<string, bool>();
        foreach (var style in data.GetMasteredStyles().Values)
        {
            unlockedStyles[style.StyleId] = style.MasteryLevel > 1;
        }
        
        return database.CanUnlockStyle(styleId, playerLevel, unlockedStyles);
    }
    
    // Get all available styles for player level
    public List<StyleMasteryDatabase.StyleConfig> GetAvailableStyles(int playerLevel)
    {
        var available = new List<StyleMasteryDatabase.StyleConfig>();
        
        foreach (var style in database.Styles.Values)
        {
            if (style.UnlockLevel <= playerLevel)
            {
                var record = data.GetStyle(style.StyleId);
                if (record == null || record.MasteryLevel == 1)
                {
                    available.Add(style);
                }
            }
        }
        
        return available;
    }
    
    // Get unlocked styles
    public List<StyleMasteryDatabase.StyleConfig> GetUnlockedStyles()
    {
        var unlocked = new List<StyleMasteryDatabase.StyleConfig>();
        
        foreach (var style in database.Styles.Values)
        {
            var record = data.GetStyle(style.StyleId);
            if (record != null && record.MasteryLevel > 1)
            {
                unlocked.Add(style);
            }
        }
        
        return unlocked;
    }
    
    // Get active style
    public string GetActiveStyle()
    {
        foreach (var style in data.GetMasteredStyles().Values)
        {
            if (style.IsActive)
                return style.StyleId;
        }
        return "";
    }
    
    // Get style info
    public Dictionary<string, object> GetStyleInfo(string styleId)
    {
        var info = new Dictionary<string, object>();
        
        var config = database.GetStyle(styleId);
        var record = data.GetStyle(styleId);
        
        if (config == null)
            return info;
        
        info["name"] = config.StyleName;
        info["description"] = config.Description;
        info["category"] = config.Category;
        info["level"] = record != null ? record.MasteryLevel : 1;
        info["xp"] = record != null ? record.TotalXP : 0;
        info["enemies_defeated"] = record != null ? record.EnemiesDefeated : 0;
        info["is_active"] = record != null && record.IsActive;
        info["unlock_level"] = config.UnlockLevel;
        info["icon"] = config.Icon;
        info["color"] = config.DisplayColor;
        
        // Calculate bonuses
        var bonuses = GetStyleBonuses(styleId);
        info["bonuses"] = bonuses;
        
        return info;
    }
    
    // Calculate style effectiveness in current combat
    public float CalculateStyleEffectiveness(string styleId, string enemyType)
    {
        float effectiveness = 1.0f;
        
        var config = database.GetStyle(styleId);
        if (config == null)
            return effectiveness;
        
        // Style-specific bonuses against certain enemy types
        switch (styleId)
        {
            case "berserker":
                if (enemyType == "boss" || enemyType == "elite")
                    effectiveness *= 1.3f;
                break;
            case "assassin":
                if (enemyType == "humanoid" || enemyType == "undead")
                    effectiveness *= 1.25f;
                break;
            case "guardian":
                if (enemyType == "boss")
                    effectiveness *= 1.2f;
                break;
            case "duelist":
                if (enemyType == "humanoid")
                    effectiveness *= 1.3f;
                break;
        }
        
        return effectiveness;
    }
    
    // Get statistics
    public Dictionary<string, object> GetStatistics()
    {
        return data.GetStatistics();
    }
    
    // Apply style bonuses to player stats
    public void ApplyStyleToPlayer(Player player)
    {
        string activeStyle = GetActiveStyle();
        if (string.IsNullOrEmpty(activeStyle))
            return;
        
        var bonuses = GetStyleBonuses(activeStyle);
        
        // Apply bonuses to player
        if (bonuses.ContainsKey("attack"))
            player.AttackBonus += (int)bonuses["attack"];
        
        if (bonuses.ContainsKey("defense"))
            player.DefenseBonus += (int)bonuses["defense"];
        
        if (bonuses.ContainsKey("speed"))
            player.SpeedBonus += (int)bonuses["speed"];
    }

    /// <summary>
    /// Export save data for persistence - delegates to StyleMasteryData
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary<string, object>();
    }

    /// <summary>
    /// Import save data from persistence - delegates to StyleMasteryData
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        // Delegates to StyleMasteryData which manages its own persistence
    }
}
