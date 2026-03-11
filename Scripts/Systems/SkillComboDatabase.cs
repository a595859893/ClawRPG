using Godot;
using System;
using System.Collections.Generic;

public class SkillComboDatabase
{
    private static SkillComboDatabase _instance;
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
}
