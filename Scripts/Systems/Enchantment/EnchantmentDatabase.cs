using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Enchantment;

public partial class EnchantmentDatabase : BaseSystem
{
    public static new EnchantmentDatabase Instance { get; private set; }

    private Dictionary<string, EnchantmentRecord> _enchantments = new();
    private Dictionary<EnchantmentType, List<EnchantmentRecord>> _byType = new();

    public override void _Ready()
    {
        base._Ready();
        Instance = this;
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // Attack enchantments
        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_fire_strike",
            Name = "火焰打击",
            Description = "攻击时附加火焰伤害",
            Type = EnchantmentType.Attack,
            Tier = EnchantmentTier.Common,
            Attribute = EnchantmentAttribute.Damage,
            AttributeValue = 5f,
            SuccessRate = 0.80f,
            RequiredPlayerLevel = 1,
            Cost = 100
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_ice_strike",
            Name = "寒冰打击",
            Description = "攻击时附加寒冰伤害",
            Type = EnchantmentType.Attack,
            Tier = EnchantmentTier.Common,
            Attribute = EnchantmentAttribute.Damage,
            AttributeValue = 5f,
            SuccessRate = 0.80f,
            RequiredPlayerLevel = 1,
            Cost = 100
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_thunder_strike",
            Name = "雷霆打击",
            Description = "攻击时附加雷电伤害",
            Type = EnchantmentType.Attack,
            Tier = EnchantmentTier.Uncommon,
            Attribute = EnchantmentAttribute.Damage,
            AttributeValue = 12f,
            SuccessRate = 0.70f,
            RequiredPlayerLevel = 5,
            Cost = 250
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_crit_boost",
            Name = "暴击强化",
            Description = "提升暴击率",
            Type = EnchantmentType.Attack,
            Tier = EnchantmentTier.Rare,
            Attribute = EnchantmentAttribute.CriticalRate,
            AttributeValue = 0.08f,
            SuccessRate = 0.60f,
            RequiredPlayerLevel = 10,
            Cost = 500
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_blade_fury",
            Name = "刀锋狂暴",
            Description = "大幅提升攻击速度和伤害",
            Type = EnchantmentType.Attack,
            Tier = EnchantmentTier.Epic,
            Attribute = EnchantmentAttribute.AttackSpeed,
            AttributeValue = 0.20f,
            SuccessRate = 0.45f,
            RequiredPlayerLevel = 20,
            Cost = 1200
        });

        // Defense enchantments
        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_steel_skin",
            Name = "钢铁皮肤",
            Description = "提升防御力",
            Type = EnchantmentType.Defense,
            Tier = EnchantmentTier.Common,
            Attribute = EnchantmentAttribute.Defense,
            AttributeValue = 5f,
            SuccessRate = 0.80f,
            RequiredPlayerLevel = 1,
            Cost = 100
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_fire_wall",
            Name = "烈焰护盾",
            Description = "火抗提升",
            Type = EnchantmentType.Defense,
            Tier = EnchantmentTier.Uncommon,
            Attribute = EnchantmentAttribute.FireResistance,
            AttributeValue = 0.15f,
            SuccessRate = 0.70f,
            RequiredPlayerLevel = 5,
            Cost = 250
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_ice_wall",
            Name = "寒冰护盾",
            Description = "冰抗提升",
            Type = EnchantmentType.Defense,
            Tier = EnchantmentTier.Uncommon,
            Attribute = EnchantmentAttribute.IceResistance,
            AttributeValue = 0.15f,
            SuccessRate = 0.70f,
            RequiredPlayerLevel = 5,
            Cost = 250
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_thunder_wall",
            Name = "雷霆护盾",
            Description = "雷抗提升",
            Type = EnchantmentType.Defense,
            Tier = EnchantmentTier.Uncommon,
            Attribute = EnchantmentAttribute.LightningResistance,
            AttributeValue = 0.15f,
            SuccessRate = 0.70f,
            RequiredPlayerLevel = 5,
            Cost = 250
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_titan",
            Name = "泰坦之力",
            Description = "大幅提升防御力和生命",
            Type = EnchantmentType.Defense,
            Tier = EnchantmentTier.Epic,
            Attribute = EnchantmentAttribute.Defense,
            AttributeValue = 25f,
            SuccessRate = 0.40f,
            RequiredPlayerLevel = 20,
            Cost = 1500
        });

        // Magic enchantments
        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_mana_flow",
            Name = "法力涌动",
            Description = "提升法力上限",
            Type = EnchantmentType.Magic,
            Tier = EnchantmentTier.Common,
            Attribute = EnchantmentAttribute.Mana,
            AttributeValue = 20f,
            SuccessRate = 0.80f,
            RequiredPlayerLevel = 1,
            Cost = 100
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_mana_regen",
            Name = "法力再生",
            Description = "提升法力回复速度",
            Type = EnchantmentType.Magic,
            Tier = EnchantmentTier.Rare,
            Attribute = EnchantmentAttribute.Mana,
            AttributeValue = 2f,
            SuccessRate = 0.60f,
            RequiredPlayerLevel = 10,
            Cost = 600
        });

        // Utility enchantments
        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_swift",
            Name = "疾风之速",
            Description = "提升移动速度",
            Type = EnchantmentType.Utility,
            Tier = EnchantmentTier.Common,
            Attribute = EnchantmentAttribute.MoveSpeed,
            AttributeValue = 0.05f,
            SuccessRate = 0.80f,
            RequiredPlayerLevel = 1,
            Cost = 100
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_poison_resist",
            Name = "毒素免疫",
            Description = "毒抗大幅提升",
            Type = EnchantmentType.Utility,
            Tier = EnchantmentTier.Uncommon,
            Attribute = EnchantmentAttribute.PoisonResistance,
            AttributeValue = 0.20f,
            SuccessRate = 0.70f,
            RequiredPlayerLevel = 5,
            Cost = 250
        });

        AddEnchantment(new EnchantmentRecord
        {
            Id = "enchant_all_stats",
            Name = "全能祝福",
            Description = "全属性提升",
            Type = EnchantmentType.Legendary,
            Tier = EnchantmentTier.Legendary,
            Attribute = EnchantmentAttribute.AllAttributes,
            AttributeValue = 10f,
            SuccessRate = 0.25f,
            RequiredPlayerLevel = 30,
            Cost = 3000
        });
    }

    private void AddEnchantment(EnchantmentRecord record)
    {
        _enchantments[record.Id] = record;
        if (!_byType.ContainsKey(record.Type))
            _byType[record.Type] = new List<EnchantmentRecord>();
        _byType[record.Type].Add(record);
    }

    public List<EnchantmentRecord> GetEnchantmentsByType(EnchantmentType type)
    {
        if (_byType.TryGetValue(type, out var list))
            return list;
        return new List<EnchantmentRecord>();
    }

    public EnchantmentRecord GetEnchantment(string id)
    {
        _enchantments.TryGetValue(id, out var record);
        return record;
    }

    public List<EnchantmentRecord> GetAllEnchantments()
    {
        return new List<EnchantmentRecord>(_enchantments.Values);
    }

    public override Dictionary<string, object> ExportSaveData()
    {
        // EnchantmentDatabase is configuration data — no runtime state to persist
        return new Dictionary<string, object>();
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        // No runtime state to restore
    }
}
