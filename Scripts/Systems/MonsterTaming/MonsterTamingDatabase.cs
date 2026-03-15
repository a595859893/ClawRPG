using Godot;
using System;
using System.Collections.Generic;

public class MonsterTamingDatabase : BaseSystem
{
    // Monster Types
    public static string[] MonsterTypes = new string[]
    {
        "Slime", "Goblin", "Skeleton", "Wolf", "Bear", "Spider", "Bat", "Ghost",
        "Orc", "Troll", "Ogre", "Dragon", "Phoenix", "Golem", "Elemental", "Chimera"
    };
    
    // Rarities
    public static string[] Rarities = new string[] { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
    
    // Monster Templates by Type
    public static Dictionary<string, MonsterTemplate> Templates = new Dictionary<string, MonsterTemplate>
    {
        ["Slime"] = new MonsterTemplate { BaseHP = 50, BaseATK = 10, BaseDEF = 5, Speed = 8, CaptureRate = 0.8f },
        ["Goblin"] = new MonsterTemplate { BaseHP = 60, BaseATK = 15, BaseDEF = 8, Speed = 12, CaptureRate = 0.7f },
        ["Skeleton"] = new MonsterTemplate { BaseHP = 45, BaseATK = 18, BaseDEF = 5, Speed = 10, CaptureRate = 0.6f },
        ["Wolf"] = new MonsterTemplate { BaseHP = 70, BaseATK = 20, BaseDEF = 10, Speed = 15, CaptureRate = 0.5f },
        ["Bear"] = new MonsterTemplate { BaseHP = 120, BaseATK = 25, BaseDEF = 20, Speed = 6, CaptureRate = 0.4f },
        ["Spider"] = new MonsterTemplate { BaseHP = 40, BaseATK = 22, BaseDEF = 6, Speed = 14, CaptureRate = 0.6f },
        ["Bat"] = new MonsterTemplate { BaseHP = 30, BaseATK = 12, BaseDEF = 3, Speed = 18, CaptureRate = 0.7f },
        ["Ghost"] = new MonsterTemplate { BaseHP = 35, BaseATK = 25, BaseDEF = 2, Speed = 16, CaptureRate = 0.3f },
        ["Orc"] = new MonsterTemplate { BaseHP = 100, BaseATK = 28, BaseDEF = 15, Speed = 8, CaptureRate = 0.4f },
        ["Troll"] = new MonsterTemplate { BaseHP = 150, BaseATK = 30, BaseDEF = 20, Speed = 5, CaptureRate = 0.25f },
        ["Ogre"] = new MonsterTemplate { BaseHP = 180, BaseATK = 35, BaseDEF = 25, Speed = 4, CaptureRate = 0.2f },
        ["Dragon"] = new MonsterTemplate { BaseHP = 250, BaseATK = 45, BaseDEF = 30, Speed = 12, CaptureRate = 0.1f },
        ["Phoenix"] = new MonsterTemplate { BaseHP = 200, BaseATK = 50, BaseDEF = 25, Speed = 15, CaptureRate = 0.08f },
        ["Golem"] = new MonsterTemplate { BaseHP = 220, BaseATK = 35, BaseDEF = 40, Speed = 3, CaptureRate = 0.15f },
        ["Elemental"] = new MonsterTemplate { BaseHP = 80, BaseATK = 40, BaseDEF = 10, Speed = 14, CaptureRate = 0.35f },
        ["Chimera"] = new MonsterTemplate { BaseHP = 280, BaseATK = 48, BaseDEF = 35, Speed = 10, CaptureRate = 0.05f }
    };
    
    // Rarity Multipliers
    public static Dictionary<string, float> RarityHPBonus = new Dictionary<string, float>
    {
        ["Common"] = 1.0f, ["Uncommon"] = 1.25f, ["Rare"] = 1.5f, ["Epic"] = 2.0f, ["Legendary"] = 3.0f
    };
    
    public static Dictionary<string, float> RarityATKBonus = new Dictionary<string, float>
    {
        ["Common"] = 1.0f, ["Uncommon"] = 1.2f, ["Rare"] = 1.5f, ["Epic"] = 2.0f, ["Legendary"] = 3.0f
    };
    
    public static Dictionary<string, float> RarityDEFBonus = new Dictionary<string, float>
    {
        ["Common"] = 1.0f, ["Uncommon"] = 1.2f, ["Rare"] = 1.5f, ["Epic"] = 2.0f, ["Legendary"] = 3.0f
    };
    
    // Capture Rate Modifiers
    public static float HealthBonus(float currentHealth, float maxHealth)
    {
        float healthPercent = currentHealth / maxHealth;
        if (healthPercent < 0.25f) return 2.0f;      // Critical low health
        if (healthPercent < 0.5f) return 1.5f;        // Low health
        if (healthPercent < 0.75f) return 1.0f;       // Normal
        return 0.75f;                                  // Full health
    }
    
    public static float RarityPenalty(string rarity)
    {
        switch (rarity)
        {
            case "Common": return 1.0f;
            case "Uncommon": return 0.8f;
            case "Rare": return 0.6f;
            case "Epic": return 0.4f;
            case "Legendary": return 0.2f;
            default: return 0.5f;
        }
    }
    
    public class MonsterTemplate
    {
        public int BaseHP { get; set; }
        public int BaseATK { get; set; }
        public int BaseDEF { get; set; }
        public float Speed { get; set; }
        public float CaptureRate { get; set; }
    }
}
