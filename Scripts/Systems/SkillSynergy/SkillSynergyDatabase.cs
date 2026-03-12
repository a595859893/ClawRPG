using Godot;
using System;
using System.Collections.Generic;

public class SkillSynergyDatabase
{
    // 协同类型
    public enum SynergyType
    {
        Offensive,
        Defensive,
        Support,
        Elemental,
        Critical,
        Utility
    }

    // 稀有度
    public enum SynergyRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    // 技能协同配置
    public class SynergyConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SynergyType Type { get; set; }
        public SynergyRarity Rarity { get; set; }
        public string[] RequiredSkills { get; set; }  // 需要按顺序使用的技能
        public float Cooldown { get; set; }
        public float Duration { get; set; }
        public int MaxStacks { get; set; }
        public Dictionary<string, float> StatBonuses { get; set; }  // 属性加成
        public float DamageMultiplier { get; set; }
        public float HealMultiplier { get; set; }
        public float CriticalChanceBonus { get; set; }
        public float CriticalDamageBonus { get; set; }
        public float ResourceCostReduction { get; set; }
        public float CooldownReduction { get; set; }
        public string TriggerMessage { get; set; }  // 触发时的消息
        public int UnlockRequirement { get; set; }  // 解锁需要的触发次数
    }

    private static SkillSynergyDatabase _instance;
    public static SkillSynergyDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new SkillSynergyDatabase();
            return _instance;
        }
    }

    public Dictionary<string, SynergyConfig> Synergies { get; private set; }

    public SkillSynergyDatabase()
    {
        Synergies = new Dictionary<string, SynergyConfig>();
        InitializeSynergies();
    }

    private void InitializeSynergies()
    {
        // ============ 攻击型协同 ============
        AddSynergy(new SynergyConfig
        {
            Id = "double_strike",
            Name = "Double Strike",
            Description = "连续攻击触发额外伤害",
            Type = SynergyType.Offensive,
            Rarity = SynergyRarity.Common,
            RequiredSkills = new string[] { "attack", "attack" },
            Cooldown = 10f,
            Duration = 8f,
            MaxStacks = 3,
            StatBonuses = new Dictionary<string, float> { { "attack", 10 } },
            DamageMultiplier = 1.5f,
            UnlockRequirement = 5,
            TriggerMessage = "⚔️ Double Strike activated!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "blade_storm",
            Name = "Blade Storm",
            Description = "剑刃风暴 - 多次攻击的极致 combo",
            Type = SynergyType.Offensive,
            Rarity = SynergyRarity.Epic,
            RequiredSkills = new string[] { "slash", "thrust", "spin", "final_blow" },
            Cooldown = 30f,
            Duration = 15f,
            MaxStacks = 1,
            StatBonuses = new Dictionary<string, float> { { "attack", 30 }, { "speed", 15 } },
            DamageMultiplier = 2.5f,
            CriticalChanceBonus = 20f,
            UnlockRequirement = 50,
            TriggerMessage = "🗡️ Blade Storm unleashed!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "combo_finisher",
            Name = "Combo Finisher",
            Description = "连击终结技",
            Type = SynergyType.Offensive,
            Rarity = SynergyRarity.Rare,
            RequiredSkills = new string[] { "quick_slash", "power_strike", "deadly_thrust" },
            Cooldown = 20f,
            Duration = 10f,
            MaxStacks = 2,
            StatBonuses = new Dictionary<string, float> { { "attack", 25 } },
            DamageMultiplier = 2.0f,
            CriticalDamageBonus = 50f,
            UnlockRequirement = 25,
            TriggerMessage = "💥 Combo Finisher!"
        });

        // ============ 防御型协同 ============
        AddSynergy(new SynergyConfig
        {
            Id = "shield_wall",
            Name = "Shield Wall",
            Description = "盾牌防御阵线",
            Type = SynergyType.Defensive,
            Rarity = SynergyRarity.Common,
            RequiredSkills = new string[] { "block", "defend", "fortify" },
            Cooldown = 15f,
            Duration = 12f,
            MaxStacks = 3,
            StatBonuses = new Dictionary<string, float> { { "defense", 20 }, { "health", 50 } },
            UnlockRequirement = 5,
            TriggerMessage = "🛡️ Shield Wall formed!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "iron_skin",
            Name = "Iron Skin",
            Description = "钢铁皮肤 - 极端防御",
            Type = SynergyType.Defensive,
            Rarity = SynergyRarity.Epic,
            RequiredSkills = new string[] { "harden", "stone_skin", "iron_body" },
            Cooldown = 45f,
            Duration = 20f,
            MaxStacks = 1,
            StatBonuses = new Dictionary<string, float> { { "defense", 50 }, { "health", 100 }, { "evasion", 10 } },
            ResourceCostReduction = 30f,
            UnlockRequirement = 40,
            TriggerMessage = "🗿 Iron Skin activated!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "counter_strike",
            Name = "Counter Strike",
            Description = "反击 - 防守反击",
            Type = SynergyType.Defensive,
            Rarity = SynergyRarity.Rare,
            RequiredSkills = new string[] { "parry", "riposte", "counter" },
            Cooldown = 20f,
            Duration = 10f,
            MaxStacks = 2,
            StatBonuses = new Dictionary<string, float> { { "defense", 15 }, { "attack", 10 } },
            DamageMultiplier = 1.8f,
            UnlockRequirement = 20,
            TriggerMessage = "⚔️ Counter Strike ready!"
        });

        // ============ 元素型协同 ============
        AddSynergy(new SynergyConfig
        {
            Id = "fire_ice_combo",
            Name = "Thermal Shock",
            Description = "热震 - 冰火交替",
            Type = SynergyType.Elemental,
            Rarity = SynergyRarity.Rare,
            RequiredSkills = new string[] { "fire_ball", "ice_lance" },
            Cooldown = 25f,
            Duration = 12f,
            MaxStacks = 2,
            StatBonuses = new Dictionary<string, float> { { "fire_damage", 30 }, { "ice_damage", 30 } },
            DamageMultiplier = 2.0f,
            UnlockRequirement = 15,
            TriggerMessage = "🔥❄️ Thermal Shock!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "thunder_fire",
            Name = "Plasma Blast",
            Description = "等离子爆破 - 雷火融合",
            Type = SynergyType.Elemental,
            Rarity = SynergyRarity.Epic,
            RequiredSkills = new string[] { "lightning_bolt", "fire_burst", "plasma_explosion" },
            Cooldown = 35f,
            Duration = 15f,
            MaxStacks = 1,
            StatBonuses = new Dictionary<string, float> { { "lightning_damage", 40 }, { "fire_damage", 40 } },
            DamageMultiplier = 2.5f,
            CriticalChanceBonus = 15f,
            UnlockRequirement = 35,
            TriggerMessage = "⚡🔥 Plasma Blast!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "holy_dark",
            Name = "Balance of Light",
            Description = "光暗平衡 - 神圣与暗影",
            Type = SynergyType.Elemental,
            Rarity = SynergyRarity.Legendary,
            RequiredSkills = new string[] { "holy_light", "dark_bolt", "void_ascension" },
            Cooldown = 60f,
            Duration = 25f,
            MaxStacks = 1,
            StatBonuses = new Dictionary<string, float> { { "holy_damage", 50 }, { "dark_damage", 50 }, { "attack", 30 } },
            DamageMultiplier = 3.0f,
            CriticalChanceBonus = 25f,
            CriticalDamageBonus = 75f,
            UnlockRequirement = 100,
            TriggerMessage = "🌟🌑 Balance of Light and Dark!"
        });

        // ============ 暴击型协同 ============
        AddSynergy(new SynergyConfig
        {
            Id = "critical_rush",
            Name = "Critical Rush",
            Description = "暴击 rush - 连续暴击",
            Type = SynergyType.Critical,
            Rarity = SynergyRarity.Uncommon,
            RequiredSkills = new string[] { "precise_strike", "critical_hit", "deadly_precision" },
            Cooldown = 20f,
            Duration = 10f,
            MaxStacks = 3,
            StatBonuses = new Dictionary<string, float> { { "critical_chance", 15 } },
            CriticalChanceBonus = 20f,
            CriticalDamageBonus = 30f,
            UnlockRequirement = 10,
            TriggerMessage = "🎯 Critical Rush!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "assassinate",
            Name = "Assassinate",
            Description = "刺杀 - 一击必杀",
            Type = SynergyType.Critical,
            Rarity = SynergyRarity.Epic,
            RequiredSkills = new string[] { "backstab", "shadow_strike", "deadly_thrust" },
            Cooldown = 45f,
            Duration = 15f,
            MaxStacks = 1,
            StatBonuses = new Dictionary<string, float> { { "critical_chance", 30 }, { "attack", 25 } },
            CriticalChanceBonus = 35f,
            CriticalDamageBonus = 100f,
            UnlockRequirement = 50,
            TriggerMessage = "🗡️💀 Assassinate!"
        });

        // ============ 治疗型协同 ============
        AddSynergy(new SynergyConfig
        {
            Id = "healing_wave",
            Name = "Healing Wave",
            Description = "治疗波 - 持续恢复",
            Type = SynergyType.Support,
            Rarity = SynergyRarity.Rare,
            RequiredSkills = new string[] { "heal", "greater_heal", "healing_wave" },
            Cooldown = 30f,
            Duration = 20f,
            MaxStacks = 2,
            StatBonuses = new Dictionary<string, float> { { "healing", 30 }, { "health", 50 } },
            HealMultiplier = 2.0f,
            UnlockRequirement = 20,
            TriggerMessage = "💚 Healing Wave!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "group_shield",
            Name = "Group Shield",
            Description = "团队护盾",
            Type = SynergyType.Support,
            Rarity = SynergyRarity.Epic,
            RequiredSkills = new string[] { "shield", "group_shield", "divine_aegis" },
            Cooldown = 50f,
            Duration = 25f,
            MaxStacks = 1,
            StatBonuses = new Dictionary<string, float> { { "defense", 35 }, { "health", 150 } },
            UnlockRequirement = 40,
            TriggerMessage = "🛡️✨ Group Shield!"
        });

        // ============ 通用 Utility ============
        AddSynergy(new SynergyConfig
        {
            Id = "resource_mastery",
            Name = "Resource Mastery",
            Description = "资源大师 - 法力/能量优化",
            Type = SynergyType.Utility,
            Rarity = SynergyRarity.Uncommon,
            RequiredSkills = new string[] { "meditate", "energy_flow", "resource_boost" },
            Cooldown = 30f,
            Duration = 15f,
            MaxStacks = 2,
            StatBonuses = new Dictionary<string, float> { { "resource_regen", 25 } },
            ResourceCostReduction = 25f,
            CooldownReduction = 15f,
            UnlockRequirement = 10,
            TriggerMessage = "💎 Resource Mastery!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "speed_surge",
            Name = "Speed Surge",
            Description = "速度 surge - 极致速度",
            Type = SynergyType.Utility,
            Rarity = SynergyRarity.Rare,
            RequiredSkills = new string[] { "haste", "wind_walk", "temporal_shift" },
            Cooldown = 25f,
            Duration = 12f,
            MaxStacks = 2,
            StatBonuses = new Dictionary<string, float> { { "speed", 30 }, { "attack_speed", 20 } },
            UnlockRequirement = 20,
            TriggerMessage = "💨 Speed Surge!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "elemental_fury",
            Name = "Elemental Fury",
            Description = "元素 fury - 元素 combo",
            Type = SynergyType.Elemental,
            Rarity = SynergyRarity.Legendary,
            RequiredSkills = new string[] { "fire_ball", "ice_storm", "lightning_bolt", "arcane_blast", "elemental_cataclysm" },
            Cooldown = 90f,
            Duration = 30f,
            MaxStacks = 1,
            StatBonuses = new Dictionary<string, float> 
            { 
                { "fire_damage", 60 }, 
                { "ice_damage", 60 },
                { "lightning_damage", 60 },
                { "attack", 40 }
            },
            DamageMultiplier = 3.5f,
            CriticalChanceBonus = 30f,
            UnlockRequirement = 100,
            TriggerMessage = "🌋❄️⚡🌪️ Elemental Fury!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "battle_cry",
            Name = "Battle Cry",
            Description = "战斗呐喊 - 团队增益",
            Type = SynergyType.Support,
            Rarity = SynergyRarity.Uncommon,
            RequiredSkills = new string[] { "war_cry", "battle_shout", "inspiring_lead" },
            Cooldown = 25f,
            Duration = 15f,
            MaxStacks = 2,
            StatBonuses = new Dictionary<string, float> { { "attack", 20 }, { "defense", 10 }, { "speed", 10 } },
            UnlockRequirement = 10,
            TriggerMessage = "📢 Battle Cry!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "nature_grace",
            Name = "Nature's Grace",
            Description = "自然优雅 - 生命与恢复",
            Type = SynergyType.Support,
            Rarity = SynergyRarity.Rare,
            RequiredSkills = new string[] { "regrowth", "rejuvenation", "nature_blessing" },
            Cooldown = 35f,
            Duration = 20f,
            MaxStacks = 2,
            StatBonuses = new Dictionary<string, float> { { "health", 100 }, { "healing", 25 }, { "health_regen", 20 } },
            HealMultiplier = 1.8f,
            UnlockRequirement = 25,
            TriggerMessage = "🌿💚 Nature's Grace!"
        });

        AddSynergy(new SynergyConfig
        {
            Id = "arcane_surge",
            Name = "Arcane Surge",
            Description = "奥术 surge - 法术增强",
            Type = SynergyType.Utility,
            Rarity = SynergyRarity.Epic,
            RequiredSkills = new string[] { "arcane_power", "mana_surge", "mystic_burst" },
            Cooldown = 40f,
            Duration = 18f,
            MaxStacks = 1,
            StatBonuses = new Dictionary<string, float> { { "magic_attack", 40 }, { "magic_defense", 20 } },
            DamageMultiplier = 2.2f,
            ResourceCostReduction = 35f,
            UnlockRequirement = 45,
            TriggerMessage = "✨ Arcane Surge!"
        });
    }

    private void AddSynergy(SynergyConfig config)
    {
        Synergies[config.Id] = config;
    }

    public SynergyConfig GetSynergy(string id)
    {
        if (Synergies.ContainsKey(id))
            return Synergies[id];
        return null;
    }

    public List<SynergyConfig> GetSynergiesByType(SynergyType type)
    {
        var list = new List<SynergyConfig>();
        foreach (var kvp in Synergies)
        {
            if (kvp.Value.Type == type)
                list.Add(kvp.Value);
        }
        return list;
    }

    public List<SynergyConfig> GetSynergiesByRarity(SynergyRarity rarity)
    {
        var list = new List<SynergyConfig>();
        foreach (var kvp in Synergies)
        {
            if (kvp.Value.Rarity == rarity)
                list.Add(kvp.Value);
        }
        return list;
    }

    public string GetRarityColor(SynergyRarity rarity)
    {
        switch (rarity)
        {
            case SynergyRarity.Common: return "#b0b0b0";
            case SynergyRarity.Uncommon: return "#1eff00";
            case SynergyRarity.Rare: return "#0070dd";
            case SynergyRarity.Epic: return "#a335ee";
            case SynergyRarity.Legendary: return "#ff8000";
            default: return "#ffffff";
        }
    }
}
