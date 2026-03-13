using Godot;
using System;
using System.Collections.Generic;

public class ComboChainDatabase : Node
{
    // 单例实例
    private static ComboChainDatabase _instance;
    public static ComboChainDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ComboChainDatabase();
            }
            return _instance;
        }
    }
    
    // 连击类型配置
    public enum ComboType
    {
        Light = 0,      // 轻攻击连击
        Heavy = 1,     // 重攻击连击
        Mixed = 2,     // 混合连击
        Skill = 3,     // 技能连击
        Perfect = 4    // 完美连击
    }
    
    // 连击等级配置
    public Dictionary<int, ComboLevelConfig> ComboLevels { get; private set; } = new Dictionary<int, ComboLevelConfig>();
    
    // 连击类型配置
    public Dictionary<ComboType, ComboTypeConfig> ComboTypeConfigs { get; private set; } = new Dictionary<ComboType, ComboTypeConfig>();
    
    // 连击加成配置
    public Dictionary<int, ChainBonusConfig> ChainBonusConfigs { get; private set; } = new Dictionary<int, ChainBonusConfig>();
    
    // 连击等级配置结构
    public class ComboLevelConfig
    {
        public int MinHits { get; set; }
        public int MaxHits { get; set; }
        public float DamageMultiplier { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Godot.Color Color { get; set; }
    }
    
    // 连击类型配置结构
    public class ComboTypeConfig
    {
        public string Name { get; set; }
        public float DamageBonus { get; set; }
        public float ChainTimeMultiplier { get; set; }
        public Godot.Color Color { get; set; }
    }
    
    // 连击加成配置结构
    public class ChainBonusConfig
    {
        public int ChainRequired { get; set; }
        public float DamageBonus { get; set; }
        public float SpeedBonus { get; set; }
        public float CritBonus { get; set; }
        public string EffectName { get; set; }
        public Godot.Color EffectColor { get; set; }
    }
    
    public ComboChainDatabase()
    {
        InitializeComboLevels();
        InitializeComboTypeConfigs();
        InitializeChainBonusConfigs();
    }
    
    private void InitializeComboLevels()
    {
        // 连击等级配置
        ComboLevels[1] = new ComboLevelConfig
        {
            MinHits = 1,
            MaxHits = 3,
            DamageMultiplier = 1.0f,
            Name = "Novice",
            Description = "Basic combo chain",
            Color = new Godot.Color(0.7f, 0.7f, 0.7f)
        };
        
        ComboLevels[2] = new ComboLevelConfig
        {
            MinHits = 4,
            MaxHits = 6,
            DamageMultiplier = 1.1f,
            Name = "Apprentice",
            Description = "Developing combo skills",
            Color = new Godot.Color(0.3f, 0.7f, 0.3f)
        };
        
        ComboLevels[3] = new ComboLevelConfig
        {
            MinHits = 7,
            MaxHits = 9,
            DamageMultiplier = 1.2f,
            Name = "Skilled",
            Description = "Competent combatant",
            Color = new Godot.Color(0.3f, 0.5f, 0.9f)
        };
        
        ComboLevels[4] = new ComboLevelConfig
        {
            MinHits = 10,
            MaxHits = 14,
            DamageMultiplier = 1.3f,
            Name = "Expert",
            Description = "Masterful fighter",
            Color = new Godot.Color(0.6f, 0.3f, 0.8f)
        };
        
        ComboLevels[5] = new ComboLevelConfig
        {
            MinHits = 15,
            MaxHits = 24,
            DamageMultiplier = 1.5f,
            Name = "Master",
            Description = "Legendary warrior",
            Color = new Godot.Color(0.9f, 0.7f, 0.2f)
        };
        
        ComboLevels[6] = new ComboLevelConfig
        {
            MinHits = 25,
            MaxHits = 49,
            DamageMultiplier = 1.75f,
            Name = "Grandmaster",
            Description = "Unparalleled skill",
            Color = new Godot.Color(0.9f, 0.4f, 0.2f)
        };
        
        ComboLevels[7] = new ComboLevelConfig
        {
            MinHits = 50,
            MaxHits = 99,
            DamageMultiplier = 2.0f,
            Name = "Legend",
            Description = "Living legend",
            Color = new Godot.Color(0.9f, 0.2f, 0.2f)
        };
        
        ComboLevels[8] = new ComboLevelConfig
        {
            MinHits = 100,
            MaxHits = int.MaxValue,
            DamageMultiplier = 2.5f,
            Name = "Mythic",
            Description = "Mythic combatant",
            Color = new Godot.Color(0.2f, 0.9f, 0.9f)
        };
    }
    
    private void InitializeComboTypeConfigs()
    {
        // 连击类型配置
        ComboTypeConfigs[ComboType.Light] = new ComboTypeConfig
        {
            Name = "Light Combo",
            DamageBonus = 0.05f,
            ChainTimeMultiplier = 1.0f,
            Color = new Godot.Color(0.6f, 0.8f, 1.0f)
        };
        
        ComboTypeConfigs[ComboType.Heavy] = new ComboTypeConfig
        {
            Name = "Heavy Combo",
            DamageBonus = 0.15f,
            ChainTimeMultiplier = 0.8f,
            Color = new Godot.Color(1.0f, 0.4f, 0.4f)
        };
        
        ComboTypeConfigs[ComboType.Mixed] = new ComboTypeConfig
        {
            Name = "Mixed Combo",
            DamageBonus = 0.10f,
            ChainTimeMultiplier = 0.9f,
            Color = new Godot.Color(0.8f, 0.8f, 0.4f)
        };
        
        ComboTypeConfigs[ComboType.Skill] = new ComboTypeConfig
        {
            Name = "Skill Combo",
            DamageBonus = 0.20f,
            ChainTimeMultiplier = 0.7f,
            Color = new Godot.Color(0.8f, 0.4f, 1.0f)
        };
        
        ComboTypeConfigs[ComboType.Perfect] = new ComboTypeConfig
        {
            Name = "Perfect Combo",
            DamageBonus = 0.35f,
            ChainTimeMultiplier = 1.2f,
            Color = new Godot.Color(1.0f, 0.8f, 0.2f)
        };
    }
    
    private void InitializeChainBonusConfigs()
    {
        // 连击加成配置
        ChainBonusConfigs[10] = new ChainBonusConfig
        {
            ChainRequired = 10,
            DamageBonus = 0.10f,
            SpeedBonus = 0.05f,
            CritBonus = 0.02f,
            EffectName = "Rising Storm",
            EffectColor = new Godot.Color(0.3f, 0.7f, 0.9f)
        };
        
        ChainBonusConfigs[25] = new ChainBonusConfig
        {
            ChainRequired = 25,
            DamageBonus = 0.20f,
            SpeedBonus = 0.10f,
            CritBonus = 0.05f,
            EffectName = "Fury Unleashed",
            EffectColor = new Godot.Color(0.9f, 0.5f, 0.2f)
        };
        
        ChainBonusConfigs[50] = new ChainBonusConfig
        {
            ChainRequired = 50,
            DamageBonus = 0.35f,
            SpeedBonus = 0.15f,
            CritBonus = 0.08f,
            EffectName = "Dragon Rage",
            EffectColor = new Godot.Color(0.9f, 0.2f, 0.2f)
        };
        
        ChainBonusConfigs[75] = new ChainBonusConfig
        {
            ChainRequired = 75,
            DamageBonus = 0.50f,
            SpeedBonus = 0.20f,
            CritBonus = 0.10f,
            EffectName = "Chaos Emperor",
            EffectColor = new Godot.Color(0.6f, 0.2f, 0.8f)
        };
        
        ChainBonusConfigs[100] = new ChainBonusConfig
        {
            ChainRequired = 100,
            DamageBonus = 0.75f,
            SpeedBonus = 0.30f,
            CritBonus = 0.15f,
            EffectName = "Divine Wrath",
            EffectColor = new Godot.Color(1.0f, 0.9f, 0.3f)
        };
        
        ChainBonusConfigs[150] = new ChainBonusConfig
        {
            ChainRequired = 150,
            DamageBonus = 1.0f,
            SpeedBonus = 0.40f,
            CritBonus = 0.20f,
            EffectName = "Ultimate Power",
            EffectColor = new Godot.Color(0.2f, 1.0f, 0.8f)
        };
        
        ChainBonusConfigs[200] = new ChainBonusConfig
        {
            ChainRequired = 200,
            DamageBonus = 1.25f,
            SpeedBonus = 0.50f,
            CritBonus = 0.25f,
            EffectName = "Transcendence",
            EffectColor = new Godot.Color(0.0f, 0.8f, 1.0f)
        };
    }
    
    // 获取连击等级
    public int GetComboLevel(int chainCount)
    {
        foreach (var kvp in ComboLevels)
        {
            if (chainCount >= kvp.Value.MinHits && chainCount <= kvp.Value.MaxHits)
            {
                return kvp.Key;
            }
        }
        return 1;
    }
    
    // 获取连击等级配置
    public ComboLevelConfig GetComboLevelConfig(int level)
    {
        if (ComboLevels.ContainsKey(level))
        {
            return ComboLevels[level];
        }
        return ComboLevels[1];
    }
    
    // 获取连击加成
    public ChainBonusConfig GetChainBonus(int chainCount)
    {
        int nearestBonus = 0;
        foreach (var kvp in ChainBonusConfigs)
        {
            if (chainCount >= kvp.Key && kvp.Key > nearestBonus)
            {
                nearestBonus = kvp.Key;
            }
        }
        
        if (nearestBonus > 0 && ChainBonusConfigs.ContainsKey(nearestBonus))
        {
            return ChainBonusConfigs[nearestBonus];
        }
        return null;
    }
    
    // 计算连击伤害
    public float CalculateChainDamage(float baseDamage, int chainCount, ComboType comboType)
    {
        int level = GetComboLevel(chainCount);
        var levelConfig = GetComboLevelConfig(level);
        
        float damageMultiplier = levelConfig.DamageMultiplier;
        
        // 应用连击类型加成
        if (ComboTypeConfigs.ContainsKey(comboType))
        {
            damageMultiplier += ComboTypeConfigs[comboType].DamageBonus;
        }
        
        // 应用连击加成
        var bonus = GetChainBonus(chainCount);
        if (bonus != null)
        {
            damageMultiplier += bonus.DamageBonus;
        }
        
        return baseDamage * damageMultiplier;
    }
}
