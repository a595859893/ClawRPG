using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Rune database - stores all rune configurations
/// </summary>
public class RuneDatabase
{
    private List<RuneData> _runes = new List<RuneData>();
    private Dictionary<string, RuneData> _runeIndex = new Dictionary<string, RuneData>();
    
    public RuneDatabase()
    {
        InitializeRunes();
    }
    
    private void InitializeRunes()
    {
        // ==================== Offensive Runes ====================
        
        // Common Offensive
        AddRune(new RuneData
        {
            Id = "rune_strike_1",
            Name = "打击符文",
            Description = "增加少量攻击力",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Common,
            SlotType = RuneSlotType.Weapon,
            AttackBonus = 5,
            RequiredLevel = 1
        });
        
        AddRune(new RuneData
        {
            Id = "rune_strike_2",
            Name = "强力打击符文",
            Description = "增加中等攻击力",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Uncommon,
            SlotType = RuneSlotType.Weapon,
            AttackBonus = 12,
            RequiredLevel = 10
        });
        
        AddRune(new RuneData
        {
            Id = "rune_strike_3",
            Name = "精通打击符文",
            Description = "增加大量攻击力",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Rare,
            SlotType = RuneSlotType.Weapon,
            AttackBonus = 25,
            CritRateBonus = 1f,
            RequiredLevel = 25
        });
        
        AddRune(new RuneData
        {
            Id = "rune_strike_4",
            Name = "史诗打击符文",
            Description = "大幅增加攻击力和暴击率",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Epic,
            SlotType = RuneSlotType.Weapon,
            AttackBonus = 45,
            CritRateBonus = 3f,
            CritDamageBonus = 5f,
            RequiredLevel = 40
        });
        
        AddRune(new RuneData
        {
            Id = "rune_strike_5",
            Name = "传说打击符文",
            Description = "极致攻击力，战斗大师之选",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Weapon,
            AttackBonus = 80,
            CritRateBonus = 5f,
            CritDamageBonus = 15f,
            LifeStealBonus = 5,
            SpecialEffect = "致命一击",
            SpecialEffectValue = 3f,
            RequiredLevel = 60
        });
        
        // Critical Runes
        AddRune(new RuneData
        {
            Id = "rune_crit_1",
            Name = "暴击符文",
            Description = "增加暴击率",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Common,
            SlotType = RuneSlotType.Ring,
            CritRateBonus = 2f,
            RequiredLevel = 5
        });
        
        AddRune(new RuneData
        {
            Id = "rune_crit_2",
            Name = "强效暴击符文",
            Description = "增加暴击率和暴击伤害",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Rare,
            SlotType = RuneSlotType.Ring,
            CritRateBonus = 5f,
            CritDamageBonus = 10f,
            RequiredLevel = 20
        });
        
        AddRune(new RuneData
        {
            Id = "rune_crit_3",
            Name = "传说暴击符文",
            Description = "极高暴击率和暴击伤害",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Ring,
            CritRateBonus = 10f,
            CritDamageBonus = 25f,
            SpecialEffect = "会心",
            SpecialEffectValue = 5f,
            RequiredLevel = 50
        });
        
        // Life Steal Runes
        AddRune(new RuneData
        {
            Id = "rune_lifesteal_1",
            Name = "吸血符文",
            Description = "攻击时恢复生命",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Uncommon,
            SlotType = RuneSlotType.Weapon,
            LifeStealBonus = 3,
            RequiredLevel = 15
        });
        
        AddRune(new RuneData
        {
            Id = "rune_lifesteal_2",
            Name = "深渊吸血符文",
            Description = "强大的生命偷取能力",
            Type = RuneType.Offensive,
            Rarity = RuneRarity.Epic,
            SlotType = RuneSlotType.Weapon,
            LifeStealBonus = 8,
            AttackBonus = 20,
            RequiredLevel = 35
        });
        
        // ==================== Defensive Runes ====================
        
        // Common Defensive
        AddRune(new RuneData
        {
            Id = "rune_guard_1",
            Name = "防护符文",
            Description = "增加少量防御力",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Common,
            SlotType = RuneSlotType.Shield,
            DefenseBonus = 5,
            RequiredLevel = 1
        });
        
        AddRune(new RuneData
        {
            Id = "rune_guard_2",
            Name = "坚固防护符文",
            Description = "增加中等防御力",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Uncommon,
            SlotType = RuneSlotType.Shield,
            DefenseBonus = 12,
            RequiredLevel = 10
        });
        
        AddRune(new RuneData
        {
            Id = "rune_guard_3",
            Name = "精通防护符文",
            Description = "增加大量防御力",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Rare,
            SlotType = RuneSlotType.Shield,
            DefenseBonus = 25,
            BlockBonus = 5,
            RequiredLevel = 25
        });
        
        AddRune(new RuneData
        {
            Id = "rune_guard_4",
            Name = "史诗防护符文",
            Description = "极高防御力和格挡",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Epic,
            SlotType = RuneSlotType.Shield,
            DefenseBonus = 45,
            BlockBonus = 12,
            HealthBonus = 50,
            RequiredLevel = 40
        });
        
        AddRune(new RuneData
        {
            Id = "rune_guard_5",
            Name = "传说防护符文",
            Description = "坚不可摧的防御",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Shield,
            DefenseBonus = 80,
            BlockBonus = 20,
            HealthBonus = 100,
            SpecialEffect = "铁壁",
            SpecialEffectValue = 10f,
            RequiredLevel = 60
        });
        
        // Health Runes
        AddRune(new RuneData
        {
            Id = "rune_health_1",
            Name = "生命符文",
            Description = "增加最大生命值",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Common,
            SlotType = RuneSlotType.Chestplate,
            HealthBonus = 20,
            RequiredLevel = 1
        });
        
        AddRune(new RuneData
        {
            Id = "rune_health_2",
            Name = "强效生命符文",
            Description = "大幅增加生命值",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Rare,
            SlotType = RuneSlotType.Chestplate,
            HealthBonus = 50,
            DefenseBonus = 10,
            RequiredLevel = 20
        });
        
        AddRune(new RuneData
        {
            Id = "rune_health_3",
            Name = "传说生命符文",
            Description = "巨额生命值加成",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Chestplate,
            HealthBonus = 150,
            DefenseBonus = 25,
            SpecialEffect = "巨人之血",
            SpecialEffectValue = 10f,
            RequiredLevel = 50
        });
        
        // Dodge Runes
        AddRune(new RuneData
        {
            Id = "rune_dodge_1",
            Name = "闪避符文",
            Description = "增加闪避率",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Uncommon,
            SlotType = RuneSlotType.Boots,
            DodgeBonus = 3,
            SpeedBonus = 2,
            RequiredLevel = 10
        });
        
        AddRune(new RuneData
        {
            Id = "rune_dodge_2",
            Name = "传说闪避符文",
            Description = "极高的闪避能力",
            Type = RuneType.Defensive,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Boots,
            DodgeBonus = 10,
            SpeedBonus = 8,
            SpecialEffect = "鬼影",
            SpecialEffectValue = 3f,
            RequiredLevel = 45
        });
        
        // ==================== Utility Runes ====================
        
        // Speed Runes
        AddRune(new RuneData
        {
            Id = "rune_speed_1",
            Name = "速度符文",
            Description = "增加移动速度",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Common,
            SlotType = RuneSlotType.Boots,
            SpeedBonus = 3,
            RequiredLevel = 1
        });
        
        AddRune(new RuneData
        {
            Id = "rune_speed_2",
            Name = "疾风符文",
            Description = "大幅增加移动速度",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Rare,
            SlotType = RuneSlotType.Boots,
            SpeedBonus = 8,
            DodgeBonus = 2,
            RequiredLevel = 20
        });
        
        AddRune(new RuneData
        {
            Id = "rune_speed_3",
            Name = "传说疾风符文",
            Description = "风一般的速度",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Boots,
            SpeedBonus = 15,
            DodgeBonus = 5,
            SpecialEffect = "疾跑",
            SpecialEffectValue = 20f,
            RequiredLevel = 50
        });
        
        // Helmet Runes (Focus/Mana)
        AddRune(new RuneData
        {
            Id = "rune_focus_1",
            Name = "专注符文",
            Description = "提升战斗专注",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Common,
            SlotType = RuneSlotType.Helmet,
            CritRateBonus = 1f,
            RequiredLevel = 5
        });
        
        AddRune(new RuneData
        {
            Id = "rune_focus_2",
            Name = "深度专注符文",
            Description = "大幅提升暴击和技能效率",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Epic,
            SlotType = RuneSlotType.Helmet,
            CritRateBonus = 5f,
            CritDamageBonus = 8f,
            RequiredLevel = 35
        });
        
        // Amulet Runes (Multi-purpose)
        AddRune(new RuneData
        {
            Id = "rune_amulet_1",
            Name = "平衡符文",
            Description = "均衡提升各项属性",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Uncommon,
            SlotType = RuneSlotType.Amulet,
            AttackBonus = 5,
            DefenseBonus = 5,
            HealthBonus = 15,
            RequiredLevel = 15
        });
        
        AddRune(new RuneData
        {
            Id = "rune_amulet_2",
            Name = "全能符文",
            Description = "全面提升所有属性",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Epic,
            SlotType = RuneSlotType.Amulet,
            AttackBonus = 15,
            DefenseBonus = 15,
            HealthBonus = 40,
            SpeedBonus = 3,
            RequiredLevel = 30
        });
        
        // ==================== Special Runes ====================
        
        // Lucky Runes
        AddRune(new RuneData
        {
            Id = "rune_luck_1",
            Name = "幸运符文",
            Description = "提升运气",
            Type = RuneType.Special,
            Rarity = RuneRarity.Rare,
            SlotType = RuneSlotType.Amulet,
            CritRateBonus = 3f,
            DodgeBonus = 3,
            RequiredLevel = 25
        });
        
        AddRune(new RuneData
        {
            Id = "rune_luck_2",
            Name = "命运符文",
            Description = "极致的幸运",
            Type = RuneType.Special,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Amulet,
            CritRateBonus = 8f,
            CritDamageBonus = 12f,
            DodgeBonus = 8,
            SpecialEffect = "幸运女神",
            SpecialEffectValue = 5f,
            RequiredLevel = 55
        });
        
        // Boss Killer Runes
        AddRune(new RuneData
        {
            Id = "rune_boss_1",
            Name = "屠魔符文",
            Description = "对Boss额外伤害",
            Type = RuneType.Special,
            Rarity = RuneRarity.Epic,
            SlotType = RuneSlotType.Weapon,
            AttackBonus = 30,
            CritDamageBonus = 10f,
            SpecialEffect = "Boss杀手",
            SpecialEffectValue = 15f,
            RequiredLevel = 40
        });
        
        AddRune(new RuneData
        {
            Id = "rune_boss_2",
            Name = "传说屠魔符文",
            Description = "Boss的噩梦",
            Type = RuneType.Special,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Weapon,
            AttackBonus = 60,
            CritRateBonus = 5f,
            CritDamageBonus = 20f,
            LifeStealBonus = 5,
            SpecialEffect = "Boss杀手",
            SpecialEffectValue = 25f,
            RequiredLevel = 65
        });
        
        // Economy Runes (Gold/Resource)
        AddRune(new RuneData
        {
            Id = "rune_gold_1",
            Name = "财富符文",
            Description = "增加金币获取",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Rare,
            SlotType = RuneSlotType.Ring,
            RequiredLevel = 20,
            SpecialEffect = "财富",
            SpecialEffectValue = 10f
        });
        
        AddRune(new RuneData
        {
            Id = "rune_gold_2",
            Name = "传说财富符文",
            Description = "大幅增加金币获取",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Ring,
            RequiredLevel = 45,
            SpecialEffect = "财富",
            SpecialEffectValue = 25f
        });
        
        // Experience Runes
        AddRune(new RuneData
        {
            Id = "rune_exp_1",
            Name = "经验符文",
            Description = "增加经验获取",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Rare,
            SlotType = RuneSlotType.Amulet,
            RequiredLevel = 20,
            SpecialEffect = "经验",
            SpecialEffectValue = 10f
        });
        
        AddRune(new RuneData
        {
            Id = "rune_exp_2",
            Name = "传说经验符文",
            Description = "大幅增加经验获取",
            Type = RuneType.Utility,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Amulet,
            RequiredLevel = 45,
            SpecialEffect = "经验",
            SpecialEffectValue = 25f
        });
        
        // Ultimate Runes
        AddRune(new RuneData
        {
            Id = "rune_ultimate_1",
            Name = "元素爆发符文",
            Description = "触发元素共鸣效果",
            Type = RuneType.Special,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Weapon,
            AttackBonus = 50,
            CritRateBonus = 5f,
            CritDamageBonus = 15f,
            SpecialEffect = "元素爆发",
            SpecialEffectValue = 20f,
            RequiredLevel = 70
        });
        
        AddRune(new RuneData
        {
            Id = "rune_ultimate_2",
            Name = "不朽符文",
            Description = "死亡时原地复活",
            Type = RuneType.Special,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Chestplate,
            HealthBonus = 100,
            DefenseBonus = 30,
            LifeStealBonus = 5,
            SpecialEffect = "不朽",
            SpecialEffectValue = 1f,
            RequiredLevel = 75
        });
        
        AddRune(new RuneData
        {
            Id = "rune_ultimate_3",
            Name = "神话符文",
            Description = "所有属性大幅提升",
            Type = RuneType.Special,
            Rarity = RuneRarity.Legendary,
            SlotType = RuneSlotType.Any,
            AttackBonus = 40,
            DefenseBonus = 40,
            HealthBonus = 80,
            SpeedBonus = 10,
            CritRateBonus = 5f,
            CritDamageBonus = 10f,
            DodgeBonus = 5,
            RequiredLevel = 80
        });
    }
    
    private void AddRune(RuneData rune)
    {
        _runes.Add(rune);
        _runeIndex[rune.Id] = rune;
    }
    
    public RuneData GetRuneById(string id)
    {
        if (_runeIndex.ContainsKey(id))
        {
            return _runeIndex[id];
        }
        return null;
    }
    
    public List<RuneData> GetAllRunes()
    {
        return new List<RuneData>(_runes);
    }
    
    public List<RuneData> GetRunesByType(RuneType type)
    {
        return _runes.FindAll(r => r.Type == type);
    }
    
    public List<RuneData> GetRunesByRarity(RuneRarity rarity)
    {
        return _runes.FindAll(r => r.Rarity == rarity);
    }
    
    public List<RuneData> GetRunesBySlot(RuneSlotType slotType)
    {
        return _runes.FindAll(r => r.SlotType == slotType || r.SlotType == RuneSlotType.Any);
    }
    
    public List<RuneData> GetRunesByLevel(int playerLevel)
    {
        return _runes.FindAll(r => r.RequiredLevel <= playerLevel);
    }
    
    public int GetTotalRuneCount()
    {
        return _runes.Count;
    }
    
    public Dictionary<RuneRarity, int> GetRarityDistribution()
    {
        var distribution = new Dictionary<RuneRarity, int>();
        foreach (RuneRarity rarity in Enum.GetValues(typeof(RuneRarity)))
        {
            distribution[rarity] = _runes.FindAll(r => r.Rarity == rarity).Count;
        }
        return distribution;
    }
}
