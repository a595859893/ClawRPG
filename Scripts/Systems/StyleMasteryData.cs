using Godot;
using System;
using System.Collections.Generic;

public class StyleMasteryData : Node
{
    // Style mastery records
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
    
    public override void _Ready()
    {
        LoadData();
    }
    
    public void LoadData()
    {
        // Initialize default styles
        var db = GetNode<StyleMasteryDatabase>("/root/StyleMasteryDatabase");
        if (db != null)
        {
            foreach (var style in db.Styles.Values)
            {
                if (!MasteredStyles.ContainsKey(style.StyleId))
                {
                    MasteredStyles[style.StyleId] = new StyleMasteryRecord(style.StyleId, style.StyleName);
                }
            }
        }
    }
    
    public void SaveData()
    {
        // Save logic here
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
            var db = GetNode<StyleMasteryDatabase>("/root/StyleMasteryDatabase");
            if (db != null && db.Styles.ContainsKey(styleId))
            {
                MasteredStyles[styleId] = new StyleMasteryRecord(styleId, db.Styles[styleId].StyleName);
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
}
