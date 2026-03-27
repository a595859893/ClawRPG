using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 技能Combo数据库。存储所有技能Combo配置和奖励信息。
/// </summary>
public class SkillComboDatabase
{
    /// <summary>
    /// 获取数据库单例实例。
    /// </summary>
    private static SkillComboDatabase _instance;

    /// <summary>
    /// 获取单例实例。
    /// </summary>
    public static SkillComboDatabase Instance => _instance ??= new SkillComboDatabase();
    
    public Dictionary<string, SkillCombo> Combos { get; private set; }
    public Dictionary<string, ComboBonus> Bonuses { get; private set; }
    
    private SkillComboDatabase()
    {
        Combos = new Dictionary<string, SkillCombo>();
        Bonuses = new Dictionary<string, ComboBonus>();
        InitializeBonuses();
        InitializeCombos();
    }
    
    private void InitializeBonuses()
    {
        // Combo bonuses with different effects
        Bonuses["combo_damage_1"] = new ComboBonus
        {
            BonusId = "combo_damage_1",
            Name = "Damage Boost I",
            Description = "Increases damage by 10%",
            DamageMultiplier = 1.10f,
            CooldownReduction = 0f,
            Duration = 5f,
            RequiredComboCount = 2
        };
        
        Bonuses["combo_damage_2"] = new ComboBonus
        {
            BonusId = "combo_damage_2",
            Name = "Damage Boost II",
            Description = "Increases damage by 20%",
            DamageMultiplier = 1.20f,
            CooldownReduction = 0f,
            Duration = 8f,
            RequiredComboCount = 3
        };
        
        Bonuses["combo_damage_3"] = new ComboBonus
        {
            BonusId = "combo_damage_3",
            Name = "Damage Boost III",
            Description = "Increases damage by 35%",
            DamageMultiplier = 1.35f,
            CooldownReduction = 0f,
            Duration = 10f,
            RequiredComboCount = 4
        };
        
        Bonuses["combo_crit_1"] = new ComboBonus
        {
            BonusId = "combo_crit_1",
            Name = "Critical Surge I",
            Description = "Increases critical rate by 15%",
            DamageMultiplier = 1f,
            CooldownReduction = 0f,
            Duration = 5f,
            RequiredComboCount = 2
        };
        
        Bonuses["combo_crit_2"] = new ComboBonus
        {
            BonusId = "combo_crit_2",
            Name = "Critical Surge II",
            Description = "Increases critical rate by 25%",
            DamageMultiplier = 1f,
            CooldownReduction = 0f,
            Duration = 8f,
            RequiredComboCount = 3
        };
        
        Bonuses["combo_cooldown"] = new ComboBonus
        {
            BonusId = "combo_cooldown",
            Name = "Haste",
            Description = "Reduces all skill cooldowns by 20%",
            DamageMultiplier = 1f,
            CooldownReduction = 0.20f,
            Duration = 6f,
            RequiredComboCount = 3
        };
        
        Bonuses["combo_ultimate"] = new ComboBonus
        {
            BonusId = "combo_ultimate",
            Name = "Ultimate Combo",
            Description = "50% damage boost + 30% critical rate",
            DamageMultiplier = 1.50f,
            CooldownReduction = 0f,
            Duration = 12f,
            RequiredComboCount = 5
        };
    }
    
    private void InitializeCombos()
    {
        // Elemental combos - same element chaining
        Combos["fire_fury"] = new SkillCombo
        {
            ComboId = "fire_fury",
            Name = "Fire Fury",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.SameElement,
            SkillIds = new List<string> { "fire_ball", "fire_blast", "inferno" },
            TimeWindow = 3f,
            Bonus = Bonuses["combo_damage_2"]
        };
        
        Combos["ice_shatter"] = new SkillCombo
        {
            ComboId = "ice_shatter",
            Name = "Ice Shatter",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.SameElement,
            SkillIds = new List<string> { "ice_shard", "frost_nova", "blizzard" },
            TimeWindow = 3f,
            Bonus = Bonuses["combo_crit_2"]
        };
        
        Combos["thunder_storm"] = new SkillCombo
        {
            ComboId = "thunder_storm",
            Name = "Thunder Storm",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.SameElement,
            SkillIds = new List<string> { "lightning_bolt", "thunder_strike", "storm" },
            TimeWindow = 3f,
            Bonus = Bonuses["combo_damage_3"]
        };
        
        // Chain reaction combos - different types
        Combos["breaker"] = new SkillCombo
        {
            ComboId = "breaker",
            Name = "Breaker",
            Type = ComboType.ChainReaction,
            Trigger = ComboTrigger.DifferentType,
            SkillIds = new List<string> { "shield_break", "power_strike", "execute" },
            TimeWindow = 2.5f,
            Bonus = Bonuses["combo_damage_3"]
        };
        
        Combos["assassin"] = new SkillCombo
        {
            ComboId = "assassin",
            Name = "Assassin Combo",
            Type = ComboType.ChainReaction,
            Trigger = ComboTrigger.DifferentType,
            SkillIds = new List<string> { "stealth", "backstab", "poison" },
            TimeWindow = 4f,
            Bonus = Bonuses["combo_crit_2"]
        };
        
        Combos["battlemaster"] = new SkillCombo
        {
            ComboId = "battlemaster",
            Name = "Battlemaster",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.Any,
            SkillIds = new List<string> { "charge", "slash", "whirlwind", "final_blow" },
            TimeWindow = 5f,
            Bonus = Bonuses["combo_ultimate"]
        };
        
        // Quick combo - fast succession
        Combos["rapid_fire"] = new SkillCombo
        {
            ComboId = "rapid_fire",
            Name = "Rapid Fire",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "quick_shot", "quick_shot", "quick_shot" },
            TimeWindow = 1.5f,
            Bonus = Bonuses["combo_damage_1"]
        };
        
        // Defensive combo
        Combos["guardian"] = new SkillCombo
        {
            ComboId = "guardian",
            Name = "Guardian Form",
            Type = ComboType.ChainReaction,
            Trigger = ComboTrigger.DifferentType,
            SkillIds = new List<string> { "block", "counter", "shield_wall" },
            TimeWindow = 3f,
            Bonus = Bonuses["combo_cooldown"]
        };
        
        // Simultaneous combo
        Combos["elemental_burst"] = new SkillCombo
        {
            ComboId = "elemental_burst",
            Name = "Elemental Burst",
            Type = ComboType.Simultaneous,
            Trigger = ComboTrigger.Any,
            SkillIds = new List<string> { "fire_bolt", "ice_shard", "lightning_bolt" },
            TimeWindow = 0.5f,
            Bonus = Bonuses["combo_damage_2"]
        };
        
        // Ultimate combos
        Combos["doom"] = new SkillCombo
        {
            ComboId = "doom",
            Name = "Doom",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.Any,
            SkillIds = new List<string> { "chaos", "annihilation", "apocalypse", "judgment" },
            TimeWindow = 8f,
            Bonus = Bonuses["combo_ultimate"]
        };

        // === 旧 ComboSystem hardcoded combos 迁移 ===
        // Offensive
        Combos["combo_double_strike"] = new SkillCombo
        {
            ComboId = "combo_double_strike",
            Name = "Double Strike",
            Description = "Strike twice in quick succession",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "basic_attack", "basic_attack" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 1.8f, CooldownReduction = 0f, Duration = 0, RequiredComboCount = 2 },
            OldComboType = ComboData.ComboType.Offensive,
            Rarity = ComboData.Rarity.Common,
            EffectName = "Double Slash",
            RequiredComboLevel = 1,
            CooldownReduction = 0f,
            ComboPointReward = 5
        };

        Combos["combo_triple_slice"] = new SkillCombo
        {
            ComboId = "combo_triple_slice",
            Name = "Triple Slice",
            Description = "Three rapid cuts dealing massive damage",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "basic_attack", "basic_attack", "basic_attack" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 2.5f, CooldownReduction = 0f, Duration = 0, RequiredComboCount = 3 },
            OldComboType = ComboData.ComboType.Offensive,
            Rarity = ComboData.Rarity.Uncommon,
            EffectName = "Triple Slash",
            RequiredComboLevel = 2,
            CooldownReduction = 0f,
            ComboPointReward = 10
        };

        Combos["combo_whirlwind"] = new SkillCombo
        {
            ComboId = "combo_whirlwind",
            Name = "Whirlwind",
            Description = "Spin attack hitting all nearby enemies",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "basic_attack", "dodge", "basic_attack" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 2.2f, CooldownReduction = 0f, Duration = 0, RequiredComboCount = 3 },
            OldComboType = ComboData.ComboType.Offensive,
            Rarity = ComboData.Rarity.Rare,
            EffectName = "Wind Blade",
            RequiredComboLevel = 3,
            CooldownReduction = 0f,
            ComboPointReward = 15
        };

        Combos["combo_fury"] = new SkillCombo
        {
            ComboId = "combo_fury",
            Name = "Fury Rush",
            Description = "Berserker combo dealing overwhelming damage",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "power_strike", "basic_attack", "power_strike" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 3.0f, CooldownReduction = 0.3f, Duration = 0, RequiredComboCount = 3 },
            OldComboType = ComboData.ComboType.Offensive,
            Rarity = ComboData.Rarity.Epic,
            EffectName = "Fury Explosion",
            RequiredComboLevel = 5,
            CooldownReduction = 0.3f,
            ComboPointReward = 25
        };

        // Defensive
        Combos["combo_block_counter"] = new SkillCombo
        {
            ComboId = "combo_block_counter",
            Name = "Block Counter",
            Description = "Block and counterattack",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "block", "basic_attack" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 1.5f, CooldownReduction = 0f, Duration = 0, RequiredComboCount = 2 },
            OldComboType = ComboData.ComboType.Defensive,
            Rarity = ComboData.Rarity.Common,
            EffectName = "Counter Strike",
            RequiredComboLevel = 1,
            CooldownReduction = 0f,
            ComboPointReward = 8
        };

        Combos["combo_shield_bash"] = new SkillCombo
        {
            ComboId = "combo_shield_bash",
            Name = "Shield Bash",
            Description = "Stun enemies with shield bash combo",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "block", "dodge", "basic_attack" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 1.8f, CooldownReduction = 0f, Duration = 0, RequiredComboCount = 3 },
            OldComboType = ComboData.ComboType.Defensive,
            Rarity = ComboData.Rarity.Uncommon,
            EffectName = "Shield Impact",
            RequiredComboLevel = 2,
            CooldownReduction = 0f,
            ComboPointReward = 12
        };

        Combos["combo_iron_will"] = new SkillCombo
        {
            ComboId = "combo_iron_will",
            Name = "Iron Will",
            Description = "Defensive stance that reflects damage",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "block", "block", "block" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 1.0f, CooldownReduction = 0.4f, Duration = 0, RequiredComboCount = 3 },
            OldComboType = ComboData.ComboType.Defensive,
            Rarity = ComboData.Rarity.Rare,
            EffectName = "Iron Reflection",
            RequiredComboLevel = 4,
            CooldownReduction = 0.4f,
            ComboPointReward = 20
        };

        // Support
        Combos["combo_healing_wave"] = new SkillCombo
        {
            ComboId = "combo_healing_wave",
            Name = "Healing Wave",
            Description = "Chain healing skills for massive recovery",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "heal", "heal" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 1.0f, CooldownReduction = 0f, Duration = 0, RequiredComboCount = 2 },
            OldComboType = ComboData.ComboType.Support,
            Rarity = ComboData.Rarity.Rare,
            EffectName = "Wave of Life",
            RequiredComboLevel = 3,
            CooldownReduction = 0f,
            ComboPointReward = 15
        };

        Combos["combo_blessing"] = new SkillCombo
        {
            ComboId = "combo_blessing",
            Name = "Divine Blessing",
            Description = "Apply multiple buffs at once",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "buff_attack", "buff_defense", "buff_speed" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 1.0f, CooldownReduction = 0.35f, Duration = 0, RequiredComboCount = 3 },
            OldComboType = ComboData.ComboType.Support,
            Rarity = ComboData.Rarity.Epic,
            EffectName = "Divine Aura",
            RequiredComboLevel = 5,
            CooldownReduction = 0.35f,
            ComboPointReward = 30
        };

        // Special
        Combos["combo_ultimate"] = new SkillCombo
        {
            ComboId = "combo_ultimate",
            Name = "Ultimate Combo",
            Description = "The ultimate skill combination",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "power_strike", "dodge", "basic_attack", "power_strike", "heal" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 4.0f, CooldownReduction = 0.5f, Duration = 0, RequiredComboCount = 5 },
            OldComboType = ComboData.ComboType.Special,
            Rarity = ComboData.Rarity.Legendary,
            EffectName = "Divine Wrath",
            RequiredComboLevel = 10,
            CooldownReduction = 0.5f,
            ComboPointReward = 100
        };

        Combos["combo_elemental_fusion"] = new SkillCombo
        {
            ComboId = "combo_elemental_fusion",
            Name = "Elemental Fusion",
            Description = "Combine elements for explosive damage",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "fire_skill", "ice_skill", "lightning_skill" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 3.5f, CooldownReduction = 0f, Duration = 0, RequiredComboCount = 3 },
            OldComboType = ComboData.ComboType.Special,
            Rarity = ComboData.Rarity.Legendary,
            EffectName = "Elemental Nova",
            RequiredComboLevel = 8,
            CooldownReduction = 0f,
            ComboPointReward = 50
        };

        // Utility
        Combos["combo_swift_escape"] = new SkillCombo
        {
            ComboId = "combo_swift_escape",
            Name = "Swift Escape",
            Description = "Quick dodge sequence for escape",
            Type = ComboType.Sequential,
            Trigger = ComboTrigger.TimeWindow,
            SkillIds = new List<string> { "dodge", "dodge", "speed_buff" },
            TimeWindow = 3f,
            Bonus = new ComboBonus { DamageMultiplier = 1.0f, CooldownReduction = 0.25f, Duration = 0, RequiredComboCount = 3 },
            OldComboType = ComboData.ComboType.Utility,
            Rarity = ComboData.Rarity.Uncommon,
            EffectName = "Shadow Step",
            RequiredComboLevel = 2,
            CooldownReduction = 0.25f,
            ComboPointReward = 10
        };
    }
    
    public SkillCombo GetCombo(string comboId)
    {
        return Combos.ContainsKey(comboId) ? Combos[comboId] : null;
    }
    
    public List<SkillCombo> GetAllCombos()
    {
        return new List<SkillCombo>(Combos.Values);
    }
    
    public List<SkillCombo> GetCombosByType(ComboType type)
    {
        List<SkillCombo> result = new List<SkillCombo>();
        foreach (var combo in Combos.Values)
        {
            if (combo.Type == type)
                result.Add(combo);
        }
        return result;
    }

    /// <summary>
    /// 按旧 ComboData.ComboType 过滤（兼容 ComboUI）
    /// </summary>
    public List<SkillCombo> GetCombosByOldType(ComboData.ComboType type)
    {
        List<SkillCombo> result = new List<SkillCombo>();
        foreach (var combo in Combos.Values)
        {
            if (combo.OldComboType == type)
                result.Add(combo);
        }
        return result;
    }
}
