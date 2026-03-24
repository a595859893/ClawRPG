using Godot;
/// <summary>
/// 风格精通数据类。
/// </summary>
using System;
using System.Collections.Generic;

public class StyleMasterySystem : BaseSystem
{
    // Style configurations (from original StyleMasteryDatabase)
    public Dictionary<string, StyleConfig> Styles = new Dictionary<string, StyleConfig>();
    
    public class StyleConfig
    {
        public string StyleId;
        public string StyleName;
        public string Description;
        public string Category; // Offensive/Defensive/Technical/Hybrid
        public int BaseAttackBonus;
        public int BaseDefenseBonus;
        public int BaseSpeedBonus;
        public float CriticalRateBonus;
        public float EvasionBonus;
        public float DamageReduction;
        public string[] RequiredStyles; // Styles that must be unlocked first
        public int UnlockLevel; // Player level required to unlock
        public string Icon; // Icon path or emoji
        public Color DisplayColor;
        
        public StyleConfig(string id, string name, string desc, string cat, int atk, int def, int spd, float crit, float eva, float red, string[] req, int lvl, string icon, Color color)
        {
            StyleId = id;
            StyleName = name;
            Description = desc;
            Category = cat;
            BaseAttackBonus = atk;
            BaseDefenseBonus = def;
            BaseSpeedBonus = spd;
            CriticalRateBonus = crit;
            EvasionBonus = eva;
            DamageReduction = red;
            RequiredStyles = req;
            UnlockLevel = lvl;
            Icon = icon;
            DisplayColor = color;
        }
    }
    
    // Style mastery records (runtime data)
    public Dictionary<string, StyleMasteryRecord> MasteredStyles = new Dictionary<string, StyleMasteryRecord>();
    
    // Statistics
    public int TotalStylesUnlocked = 0;
    public int HighestMasteryLevel = 0;
    public string FavoriteStyle = "";
    public int TotalStyleSwitches = 0;
    public int StyleChangeStreak = 0;
    public DateTime LastStyleChangeTime = DateTime.MinValue;
    
    public Dictionary<string, int> StyleUsageCount = new Dictionary<string, int>();
    public Dictionary<string, int> EnemiesDefeatedWithStyle = new Dictionary<string, int>();
    
    public class StyleMasteryRecord
    {
        public string StyleId;
        public string StyleName;
        public int MasteryLevel; // 1-100
        public int TotalXP;
        public int EnemiesDefeated;
        public int StyleSwitchCount;
        public DateTime FirstUnlocked;
        public DateTime LastUsed;
        public bool IsActive;
        
        public StyleMasteryRecord(string id, string name)
        {
            StyleId = id;
            StyleName = name;
            MasteryLevel = 1;
            TotalXP = 0;
            EnemiesDefeated = 0;
            StyleSwitchCount = 0;
            FirstUnlocked = DateTime.Now;
            LastUsed = DateTime.Now;
            IsActive = false;
        }
    }
    
    // Track active style ID directly for fast lookup
    private string _activeStyleId = "";
    
    public override void _Ready()
    {
        InitializeStyles();
        LoadData();
    }
    
    public void InitializeStyles()
    {
        // Offensive Styles
        Styles["berserker"] = new StyleConfig(
            "berserker", "Berserker", "A fury-filled combat style that trades defense for devastating offense",
            "Offensive", 30, -10, 15, 0.15f, -0.10f, 0.0f,
            new string[] {}, 1, "⚔️", new Color(1f, 0.2f, 0.2f)
        );
        
        Styles["duelist"] = new StyleConfig(
            "duelist", "Duelist", "An elegant fighting style focused on precision strikes and counterattacks",
            "Offensive", 20, 5, 25, 0.25f, 0.10f, 0.0f,
            new string[] {"berserker"}, 10, "🤺", new Color(0.9f, 0.7f, 0.2f)
        );
        
        Styles["assassin"] = new StyleConfig(
            "assassin", "Assassin", "A stealthy combat style emphasizing critical hits and evasion",
            "Offensive", 15, -5, 30, 0.35f, 0.20f, 0.0f,
            new string[] {"duelist"}, 20, "🗡️", new Color(0.3f, 0.3f, 0.3f)
        );
        
        // Defensive Styles
        Styles["guardian"] = new StyleConfig(
            "guardian", "Guardian", "A defensive stance that maximizes protection for self and allies",
            "Defensive", -10, 40, -10, -0.05f, 0.15f, 0.30f,
            new string[] {}, 1, "🛡️", new Color(0.3f, 0.5f, 0.9f)
        );
        
        Styles["monk"] = new StyleConfig(
            "monk", "Monk", "A balanced combat style using dodging and counterattacks",
            "Defensive", 10, 20, 20, 0.10f, 0.25f, 0.15f,
            new string[] {"guardian"}, 10, "☯️", new Color(0.4f, 0.8f, 0.4f)
        );
        
        Styles["paladin"] = new StyleConfig(
            "paladin", "Paladin", "A holy warrior style combining defense with divine power",
            "Defensive", 15, 30, 5, 0.05f, 0.10f, 0.20f,
            new string[] {"monk"}, 20, "✝️", new Color(1f, 0.9f, 0.4f)
        );
        
        // Technical Styles
        Styles["samurai"] = new StyleConfig(
            "samurai", "Samurai", "A disciplined style focusing on perfect strikes and timing",
            "Technical", 25, 10, 15, 0.20f, 0.10f, 0.05f,
            new string[] {}, 5, "🎋", new Color(0.8f, 0.2f, 0.3f)
        );
        
        Styles["fencer"] = new StyleConfig(
            "fencer", "Fencer", "A precise fencing style using thrust attacks and parries",
            "Technical", 20, 15, 20, 0.25f, 0.15f, 0.05f,
            new string[] {"samurai"}, 15, "⚔️", new Color(0.6f, 0.7f, 0.9f)
        );
        
        Styles["martial_artist"] = new StyleConfig(
            "martial_artist", "Martial Artist", "A versatile hand-to-hand combat style with combo attacks",
            "Technical", 25, 5, 25, 0.15f, 0.15f, 0.0f,
            new string[] {"samurai", "monk"}, 15, "🥋", new Color(0.9f, 0.5f, 0.3f)
        );
        
        // Hybrid Styles
        Styles["dragoon"] = new StyleConfig(
            "dragoon", "Dragoon", "A balanced style combining lance combat with jump attacks",
            "Hybrid", 20, 20, 20, 0.15f, 0.10f, 0.10f,
            new string[] {"guardian", "samurai"}, 20, "🐉", new Color(0.3f, 0.6f, 0.9f)
        );
        
        Styles["spellblade"] = new StyleConfig(
            "spellblade", "Spellblade", "A hybrid style blending melee attacks with magic",
            "Hybrid", 25, 10, 15, 0.15f, 0.05f, 0.05f,
            new string[] {"duelist", "fencer"}, 25, "🪄", new Color(0.6f, 0.3f, 0.9f)
        );
        
        Styles["rune_knight"] = new StyleConfig(
            "rune_knight", "Rune Knight", "An advanced style using runic enchantments in combat",
            "Hybrid", 20, 25, 10, 0.10f, 0.10f, 0.15f,
            new string[] {"paladin", "spellblade"}, 30, "🔯", new Color(0.5f, 0.3f, 0.7f)
        );
        
        // Master Styles (require multiple prerequisites)
        Styles["grandmaster"] = new StyleConfig(
            "grandmaster", "Grandmaster", "The ultimate combat style, mastering all others",
            "Hybrid", 30, 30, 25, 0.30f, 0.25f, 0.20f,
            new string[] {"assassin", "paladin", "rune_knight"}, 50, "👑", new Color(1f, 0.8f, 0.2f)
        );
        
        Styles["legend"] = new StyleConfig(
            "legend", "Legend", "A mythic style passed down through legendary warriors",
            "Hybrid", 35, 20, 30, 0.35f, 0.20f, 0.10f,
            new string[] {"grandmaster"}, 60, "🌟", new Color(1f, 0.9f, 0.5f)
        );
    }
    
    public void LoadData()
    {
        // Initialize default styles from local Styles dictionary
        foreach (var style in Styles.Values)
        {
            if (!MasteredStyles.ContainsKey(style.StyleId))
            {
                MasteredStyles[style.StyleId] = new StyleMasteryRecord(style.StyleId, style.StyleName);
            }
        }
    }
    
    public void SaveData()
    {
        // Save logic here
    }
    
    public StyleConfig GetStyleConfig(string styleId)
    {
        if (Styles.ContainsKey(styleId))
            return Styles[styleId];
        return null;
    }
    
    public StyleConfig[] GetStylesByCategory(string category)
    {
        var result = new List<StyleConfig>();
        foreach (var style in Styles.Values)
        {
            if (style.Category == category)
                result.Add(style);
        }
        return result.ToArray();
    }
    
    public bool CanUnlockStyle(string styleId, int playerLevel, Dictionary<string, bool> unlockedStyles)
    {
        if (!Styles.ContainsKey(styleId))
            return false;
        
        var style = Styles[styleId];
        
        // Check level requirement
        if (playerLevel < style.UnlockLevel)
            return false;
        
        // Check prerequisites
        foreach (var req in style.RequiredStyles)
        {
            if (!unlockedStyles.ContainsKey(req) || !unlockedStyles[req])
                return false;
        }
        
        return true;
    }
    
    public Dictionary<string, StyleConfig> GetAllStyles()
    {
        return Styles;
    }
    
    public Dictionary<string, StyleMasteryRecord> GetMasteredStyles()
    {
        return MasteredStyles;
    }
    
    public StyleMasteryRecord GetStyle(string styleId)
    {
        if (MasteredStyles.ContainsKey(styleId))
            return MasteredStyles[styleId];
        return null;
    }
    
    public void AddStyleXP(string styleId, int xp)
    {
        if (!MasteredStyles.ContainsKey(styleId))
        {
            if (Styles.ContainsKey(styleId))
            {
                MasteredStyles[styleId] = new StyleMasteryRecord(styleId, Styles[styleId].StyleName);
            }
            else
            {
                return;
            }
        }
        
        var record = MasteredStyles[styleId];
        record.TotalXP += xp;
        
        // Level up check
        int newLevel = CalculateLevel(record.TotalXP);
        if (newLevel > record.MasteryLevel)
        {
            record.MasteryLevel = newLevel;
            if (newLevel > HighestMasteryLevel)
                HighestMasteryLevel = newLevel;
        }
        
        record.LastUsed = DateTime.Now;
        SaveData();
    }
    
    public int CalculateLevel(int xp)
    {
        // XP to level formula: level = sqrt(xp / 100)
        return Mathf.Min(100, Mathf.Max(1, (int)Mathf.Sqrt(xp / 100f) + 1));
    }
    
    public void SetActiveStyle(string styleId)
    {
        // Deactivate all styles
        foreach (var style in MasteredStyles.Values)
        {
            style.IsActive = false;
        }
        
        // Activate selected style
        if (MasteredStyles.ContainsKey(styleId))
        {
            MasteredStyles[styleId].IsActive = true;
            MasteredStyles[styleId].StyleSwitchCount++;
            MasteredStyles[styleId].LastUsed = DateTime.Now;
            
            _activeStyleId = styleId;
            TotalStyleSwitches++;
            StyleChangeStreak++;
            LastStyleChangeTime = DateTime.Now;
            
            if (!StyleUsageCount.ContainsKey(styleId))
                StyleUsageCount[styleId] = 0;
            StyleUsageCount[styleId]++;
            
            // Track favorite style
            int maxUsage = 0;
            foreach (var usage in StyleUsageCount)
            {
                if (usage.Value > maxUsage)
                {
                    maxUsage = usage.Value;
                    FavoriteStyle = usage.Key;
                }
            }
            
            SaveData();
        }
    }
    
    public string GetActiveStyle()
    {
        return _activeStyleId;
    }
    
    public void SwitchStyle(string styleId)
    {
        SetActiveStyle(styleId);
    }
    
    public Dictionary<string, object> GetStyleInfo(string styleId)
    {
        var info = new Dictionary<string, object>();
        
        if (!Styles.ContainsKey(styleId))
            return info;
        
        var config = Styles[styleId];
        var record = MasteredStyles.ContainsKey(styleId) ? MasteredStyles[styleId] : null;
        
        info["name"] = config.StyleName;
        info["category"] = config.Category;
        info["level"] = record != null ? record.MasteryLevel : 1;
        info["xp"] = record != null ? record.TotalXP : 0;
        info["enemies_defeated"] = record != null ? record.EnemiesDefeated : 0;
        info["unlock_level"] = config.UnlockLevel;
        info["description"] = config.Description;
        
        // Bonuses
        var bonuses = new Dictionary<string, float>();
        if (record != null && record.MasteryLevel > 1)
        {
            float mult = record.MasteryLevel * 0.1f;
            bonuses["attack"] = config.BaseAttackBonus * mult;
            bonuses["defense"] = config.BaseDefenseBonus * mult;
            bonuses["speed"] = config.BaseSpeedBonus * mult;
            bonuses["crit"] = config.CriticalRateBonus * mult;
            bonuses["evasion"] = config.EvasionBonus * mult;
            bonuses["damage_reduction"] = config.DamageReduction * mult;
        }
        else
        {
            bonuses["attack"] = config.BaseAttackBonus;
            bonuses["defense"] = config.BaseDefenseBonus;
            bonuses["speed"] = config.BaseSpeedBonus;
            bonuses["crit"] = config.CriticalRateBonus;
            bonuses["evasion"] = config.EvasionBonus;
            bonuses["damage_reduction"] = config.DamageReduction;
        }
        info["bonuses"] = bonuses;
        
        return info;
    }
    
    public void RecordEnemyDefeated(string styleId)
    {
        if (MasteredStyles.ContainsKey(styleId))
        {
            MasteredStyles[styleId].EnemiesDefeated++;
            
            if (!EnemiesDefeatedWithStyle.ContainsKey(styleId))
                EnemiesDefeatedWithStyle[styleId] = 0;
            EnemiesDefeatedWithStyle[styleId]++;
            
            TotalStylesUnlocked = 0;
            foreach (var style in MasteredStyles.Values)
            {
                if (style.MasteryLevel > 1)
                    TotalStylesUnlocked++;
            }
            
            SaveData();
        }
    }
    
    public Dictionary<string, object> GetStatistics()
    {
        var stats = new Dictionary<string, object>();
        stats["TotalStylesUnlocked"] = TotalStylesUnlocked;
        stats["HighestMasteryLevel"] = HighestMasteryLevel;
        stats["FavoriteStyle"] = FavoriteStyle;
        stats["TotalStyleSwitches"] = TotalStyleSwitches;
        stats["StyleChangeStreak"] = StyleChangeStreak;
        return stats;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // Serialize mastered styles
        var masteredStylesData = new List<Dictionary>();
        foreach (var kvp in MasteredStyles)
        {
            var recordData = new Dictionary
            {
                ["style_id"] = kvp.Value.StyleId,
                ["mastery_level"] = kvp.Value.MasteryLevel,
                ["total_xp"] = kvp.Value.TotalXP,
                ["enemies_defeated"] = kvp.Value.EnemiesDefeated,
                ["style_switch_count"] = kvp.Value.StyleSwitchCount,
                ["first_unlocked"] = kvp.Value.FirstUnlocked.ToString("o"),
                ["last_used"] = kvp.Value.LastUsed.ToString("o"),
                ["is_active"] = kvp.Value.IsActive
            };
            masteredStylesData.Add(recordData);
        }
        data["mastered_styles"] = masteredStylesData;
        
        // Serialize runtime stats
        data["total_styles_unlocked"] = TotalStylesUnlocked;
        data["highest_mastery_level"] = HighestMasteryLevel;
        data["favorite_style"] = FavoriteStyle;
        data["total_style_switches"] = TotalStyleSwitches;
        data["style_change_streak"] = StyleChangeStreak;
        data["last_style_change_time"] = LastStyleChangeTime == DateTime.MinValue ? "" : LastStyleChangeTime.ToString("o");
        data["active_style_id"] = _activeStyleId;
        
        // Serialize usage counts
        var usageData = new Dictionary<string, int>();
        foreach (var kvp in StyleUsageCount)
            usageData[kvp.Key] = kvp.Value;
        data["style_usage_count"] = usageData;
        
        // Serialize enemies defeated
        var enemyData = new Dictionary<string, int>();
        foreach (var kvp in EnemiesDefeatedWithStyle)
            enemyData[kvp.Key] = kvp.Value;
        data["enemies_defeated_with_style"] = enemyData;
        
        return data;
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null || data.Count == 0) return;
        
        // Deserialize mastered styles
        if (data.ContainsKey("mastered_styles") && data["mastered_styles"] is List<Dictionary> masteredList)
        {
            foreach (var recordData in masteredList)
            {
                string styleId = recordData.ContainsKey("style_id") ? (string)recordData["style_id"] : "";
                if (string.IsNullOrEmpty(styleId) || !Styles.ContainsKey(styleId)) continue;
                
                var record = new StyleMasteryRecord(styleId, Styles[styleId].StyleName);
                record.MasteryLevel = recordData.ContainsKey("mastery_level") ? (int)recordData["mastery_level"] : 1;
                record.TotalXP = recordData.ContainsKey("total_xp") ? (int)recordData["total_xp"] : 0;
                record.EnemiesDefeated = recordData.ContainsKey("enemies_defeated") ? (int)recordData["enemies_defeated"] : 0;
                record.StyleSwitchCount = recordData.ContainsKey("style_switch_count") ? (int)recordData["style_switch_count"] : 0;
                
                if (recordData.ContainsKey("first_unlocked"))
                    DateTime.TryParse((string)recordData["first_unlocked"], out record.FirstUnlocked);
                if (recordData.ContainsKey("last_used"))
                    DateTime.TryParse((string)recordData["last_used"], out record.LastUsed);
                record.IsActive = recordData.ContainsKey("is_active") ? (bool)recordData["is_active"] : false;
                
                MasteredStyles[styleId] = record;
            }
        }
        
        // Deserialize runtime stats
        TotalStylesUnlocked = data.ContainsKey("total_styles_unlocked") ? (int)data["total_styles_unlocked"] : 0;
        HighestMasteryLevel = data.ContainsKey("highest_mastery_level") ? (int)data["highest_mastery_level"] : 0;
        FavoriteStyle = data.ContainsKey("favorite_style") ? (string)data["favorite_style"] : "";
        TotalStyleSwitches = data.ContainsKey("total_style_switches") ? (int)data["total_style_switches"] : 0;
        StyleChangeStreak = data.ContainsKey("style_change_streak") ? (int)data["style_change_streak"] : 0;
        _activeStyleId = data.ContainsKey("active_style_id") ? (string)data["active_style_id"] : "";
        
        if (data.ContainsKey("last_style_change_time") && !string.IsNullOrEmpty((string)data["last_style_change_time"]))
            DateTime.TryParse((string)data["last_style_change_time"], out LastStyleChangeTime);
        
        // Deserialize usage counts
        if (data.ContainsKey("style_usage_count") && data["style_usage_count"] is Dictionary usageDict)
        {
            foreach (var kvp in usageDict)
                StyleUsageCount[kvp.Key] = (int)kvp.Value;
        }
        
        // Deserialize enemies defeated
        if (data.ContainsKey("enemies_defeated_with_style") && data["enemies_defeated_with_style"] is Dictionary enemyDict)
        {
            foreach (var kvp in enemyDict)
                EnemiesDefeatedWithStyle[kvp.Key] = (int)kvp.Value;
        }
    }
}
