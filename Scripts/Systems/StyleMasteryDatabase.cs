using Godot;
/// <summary>
/// 风格精通数据库。
/// </summary>
using System;
using System.Collections.Generic;

public class StyleMasteryDatabase : BaseSystem
{
    // Style configurations
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
    
    public override void _Ready()
    {
        InitializeStyles();
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
    
    public StyleConfig GetStyle(string styleId)
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
}
