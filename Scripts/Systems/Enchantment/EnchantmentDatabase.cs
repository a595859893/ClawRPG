using Godot;
using System;
using System.Collections.Generic;

public class EnchantmentDatabase
{
    private static EnchantmentDatabase _instance;
    public static EnchantmentDatabase Instance => _instance ?? (_instance = new EnchantmentDatabase());

    private Dictionary<string, EnchantmentData> _enchantments;
    private List<EnchantmentData> _allEnchantments;

    public EnchantmentDatabase()
    {
        _enchantments = new Dictionary<string, EnchantmentData>();
        _allEnchantments = new List<EnchantmentData>();
        InitializeDatabase();
    }

    private void InitializeDatabase()
    {
        // ===== 武器附魔 =====
        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_fire_weapon",
            Name = "火焰附魔",
            Description = "赋予武器火焰伤害",
            Type = EnchantmentData.EnchantmentType.Weapon,
            RarityLevel = EnchantmentData.Rarity.Uncommon,
            RequiredLevel = 10,
            MaxLevel = 5,
            BaseCost = 100,
            SuccessRate = 0.8f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Attack, 10f },
                { EnchantmentData.PropertyType.FireResistance, 5f }
            },
            IconName = "fire"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_ice_weapon",
            Name = "冰霜附魔",
            Description = "赋予武器冰霜伤害",
            Type = EnchantmentData.EnchantmentType.Weapon,
            RarityLevel = EnchantmentData.Rarity.Uncommon,
            RequiredLevel = 10,
            MaxLevel = 5,
            BaseCost = 100,
            SuccessRate = 0.8f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Attack, 8f },
                { EnchantmentData.PropertyType.IceResistance, 5f }
            },
            IconName = "ice"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_thunder_weapon",
            Name = "雷电附魔",
            Description = "赋予武器雷电伤害",
            Type = EnchantmentData.EnchantmentType.Weapon,
            RarityLevel = EnchantmentData.Rarity.Rare,
            RequiredLevel = 20,
            MaxLevel = 5,
            BaseCost = 200,
            SuccessRate = 0.7f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Attack, 12f },
                { EnchantmentData.PropertyType.LightningResistance, 8f }
            },
            IconName = "thunder"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_blood_weapon",
            Name = "嗜血附魔",
            Description = "攻击时吸取生命",
            Type = EnchantmentData.EnchantmentType.Weapon,
            RarityLevel = EnchantmentData.Rarity.Rare,
            RequiredLevel = 25,
            MaxLevel = 5,
            BaseCost = 250,
            SuccessRate = 0.65f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Attack, 15f },
                { EnchantmentData.PropertyType.LifeSteal, 5f }
            },
            IconName = "blood"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_shadow_weapon",
            Name = "暗影附魔",
            Description = "赋予武器暗影之力",
            Type = EnchantmentData.EnchantmentType.Weapon,
            RarityLevel = EnchantmentData.Rarity.Epic,
            RequiredLevel = 35,
            MaxLevel = 5,
            BaseCost = 400,
            SuccessRate = 0.55f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Attack, 20f },
                { EnchantmentData.PropertyType.Critical, 3f }
            },
            IconName = "shadow"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_divine_weapon",
            Name = "神圣附魔",
            Description = "蕴含神圣之力",
            Type = EnchantmentData.EnchantmentType.Weapon,
            RarityLevel = EnchantmentData.Rarity.Legendary,
            RequiredLevel = 45,
            MaxLevel = 5,
            BaseCost = 800,
            SuccessRate = 0.4f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Attack, 30f },
                { EnchantmentData.PropertyType.MagicAttack, 15f }
            },
            IconName = "divine"
        });

        // ===== 护甲附魔 =====
        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_steel_armor",
            Name = "钢化附魔",
            Description = "提升防御力",
            Type = EnchantmentData.EnchantmentType.Armor,
            RarityLevel = EnchantmentData.Rarity.Common,
            RequiredLevel = 5,
            MaxLevel = 5,
            BaseCost = 50,
            SuccessRate = 0.9f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Defense, 15f }
            },
            IconName = "steel"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_fire_armor",
            Name = "火焰抗性附魔",
            Description = "提升火焰抗性",
            Type = EnchantmentData.EnchantmentType.Armor,
            RarityLevel = EnchantmentData.Rarity.Uncommon,
            RequiredLevel = 15,
            MaxLevel = 5,
            BaseCost = 120,
            SuccessRate = 0.8f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Defense, 10f },
                { EnchantmentData.PropertyType.FireResistance, 15f }
            },
            IconName = "fire"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_ice_armor",
            Name = "冰霜抗性附魔",
            Description = "提升冰霜抗性",
            Type = EnchantmentData.EnchantmentType.Armor,
            RarityLevel = EnchantmentData.Rarity.Uncommon,
            RequiredLevel = 15,
            MaxLevel = 5,
            BaseCost = 120,
            SuccessRate = 0.8f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Defense, 10f },
                { EnchantmentData.PropertyType.IceResistance, 15f }
            },
            IconName = "ice"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_thunder_armor",
            Name = "雷电抗性附魔",
            Description = "提升雷电抗性",
            Type = EnchantmentData.EnchantmentType.Armor,
            RarityLevel = EnchantmentData.Rarity.Rare,
            RequiredLevel = 25,
            MaxLevel = 5,
            BaseCost = 250,
            SuccessRate = 0.7f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Defense, 15f },
                { EnchantmentData.PropertyType.LightningResistance, 20f }
            },
            IconName = "thunder"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_titan_armor",
            Name = "泰坦附魔",
            Description = "大幅提升防御和生命",
            Type = EnchantmentData.EnchantmentType.Armor,
            RarityLevel = EnchantmentData.Rarity.Epic,
            RequiredLevel = 35,
            MaxLevel = 5,
            BaseCost = 500,
            SuccessRate = 0.5f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Defense, 25f },
                { EnchantmentData.PropertyType.Health, 100f }
            },
            IconName = "titan"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_divine_armor",
            Name = "神圣护甲附魔",
            Description = "神圣之力护体",
            Type = EnchantmentData.EnchantmentType.Armor,
            RarityLevel = EnchantmentData.Rarity.Legendary,
            RequiredLevel = 50,
            MaxLevel = 5,
            BaseCost = 1000,
            SuccessRate = 0.35f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Defense, 35f },
                { EnchantmentData.PropertyType.MagicDefense, 20f },
                { EnchantmentData.PropertyType.Health, 150f }
            },
            IconName = "divine"
        });

        // ===== 饰品附魔 =====
        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_lucky_accessory",
            Name = "幸运附魔",
            Description = "提升暴击率",
            Type = EnchantmentData.EnchantmentType.Accessory,
            RarityLevel = EnchantmentData.Rarity.Uncommon,
            RequiredLevel = 10,
            MaxLevel = 5,
            BaseCost = 100,
            SuccessRate = 0.75f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Critical, 5f }
            },
            IconName = "lucky"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_speed_accessory",
            Name = "速度附魔",
            Description = "提升移动和攻击速度",
            Type = EnchantmentData.EnchantmentType.Accessory,
            RarityLevel = EnchantmentData.Rarity.Uncommon,
            RequiredLevel = 15,
            MaxLevel = 5,
            BaseCost = 150,
            SuccessRate = 0.7f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Speed, 10f }
            },
            IconName = "speed"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_vampiric_accessory",
            Name = "吸血附魔",
            Description = "攻击时吸取生命",
            Type = EnchantmentData.EnchantmentType.Accessory,
            RarityLevel = EnchantmentData.Rarity.Rare,
            RequiredLevel = 25,
            MaxLevel = 5,
            BaseCost = 300,
            SuccessRate = 0.6f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.LifeSteal, 8f }
            },
            IconName = "blood"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_arcane_accessory",
            Name = "奥术附魔",
            Description = "提升魔法攻击",
            Type = EnchantmentData.EnchantmentType.Accessory,
            RarityLevel = EnchantmentData.Rarity.Epic,
            RequiredLevel = 35,
            MaxLevel = 5,
            BaseCost = 500,
            SuccessRate = 0.5f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.MagicAttack, 25f }
            },
            IconName = "arcane"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_mythical_accessory",
            Name = "神话附魔",
            Description = "全属性大幅提升",
            Type = EnchantmentData.EnchantmentType.Accessory,
            RarityLevel = EnchantmentData.Rarity.Legendary,
            RequiredLevel = 50,
            MaxLevel = 5,
            BaseCost = 1500,
            SuccessRate = 0.3f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Attack, 15f },
                { EnchantmentData.PropertyType.Defense, 15f },
                { EnchantmentData.PropertyType.Health, 80f },
                { EnchantmentData.PropertyType.Speed, 8f },
                { EnchantmentData.PropertyType.Critical, 3f }
            },
            IconName = "mythical"
        });

        // ===== 头盔附魔 =====
        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_wisdom_helmet",
            Name = "智慧附魔",
            Description = "提升智力属性",
            Type = EnchantmentData.EnchantmentType.Helmet,
            RarityLevel = EnchantmentData.Rarity.Uncommon,
            RequiredLevel = 15,
            MaxLevel = 5,
            BaseCost = 120,
            SuccessRate = 0.75f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.MagicAttack, 12f }
            },
            IconName = "wisdom"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_protection_helmet",
            Name = "保护附魔",
            Description = "提升魔法防御",
            Type = EnchantmentData.EnchantmentType.Helmet,
            RarityLevel = EnchantmentData.Rarity.Rare,
            RequiredLevel = 25,
            MaxLevel = 5,
            BaseCost = 250,
            SuccessRate = 0.65f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.MagicDefense, 18f }
            },
            IconName = "protection"
        });

        // ===== 鞋子附魔 =====
        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_swift_boots",
            Name = "迅捷附魔",
            Description = "大幅提升速度",
            Type = EnchantmentData.EnchantmentType.Boots,
            RarityLevel = EnchantmentData.Rarity.Uncommon,
            RequiredLevel = 10,
            MaxLevel = 5,
            BaseCost = 100,
            SuccessRate = 0.8f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Speed, 15f },
                { EnchantmentData.PropertyType.Evasion, 3f }
            },
            IconName = "swift"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_agility_boots",
            Name = "敏捷附魔",
            Description = "提升闪避率",
            Type = EnchantmentData.EnchantmentType.Boots,
            RarityLevel = EnchantmentData.Rarity.Rare,
            RequiredLevel = 20,
            MaxLevel = 5,
            BaseCost = 200,
            SuccessRate = 0.7f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Speed, 10f },
                { EnchantmentData.PropertyType.Evasion, 8f }
            },
            IconName = "agility"
        });

        // ===== 手套附魔 =====
        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_power_gloves",
            Name = "力量附魔",
            Description = "提升攻击力和暴击率",
            Type = EnchantmentData.EnchantmentType.Gloves,
            RarityLevel = EnchantmentData.Rarity.Uncommon,
            RequiredLevel = 15,
            MaxLevel = 5,
            BaseCost = 150,
            SuccessRate = 0.75f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Attack, 12f },
                { EnchantmentData.PropertyType.Critical, 2f }
            },
            IconName = "power"
        });

        AddEnchantment(new EnchantmentData
        {
            Id = "enchant_assassin_gloves",
            Name = "刺客附魔",
            Description = "提升暴击和闪避",
            Type = EnchantmentData.EnchantmentType.Gloves,
            RarityLevel = EnchantmentData.Rarity.Epic,
            RequiredLevel = 30,
            MaxLevel = 5,
            BaseCost = 400,
            SuccessRate = 0.5f,
            Properties = new Dictionary<EnchantmentData.PropertyType, float>
            {
                { EnchantmentData.PropertyType.Critical, 8f },
                { EnchantmentData.PropertyType.Evasion, 10f }
            },
            IconName = "assassin"
        });
    }

    private void AddEnchantment(EnchantmentData data)
    {
        _enchantments[data.Id] = data;
        _allEnchantments.Add(data);
    }

    public EnchantmentData GetEnchantment(string id)
    {
        return _enchantments.ContainsKey(id) ? _enchantments[id] : null;
    }

    public List<EnchantmentData> GetAllEnchantments()
    {
        return new List<EnchantmentData>(_allEnchantments);
    }

    public List<EnchantmentData> GetEnchantmentsByType(EnchantmentData.EnchantmentType type)
    {
        return _allEnchantments.FindAll(e => e.Type == type);
    }

    public List<EnchantmentData> GetEnchantmentsByRarity(EnchantmentData.Rarity rarity)
    {
        return _allEnchantments.FindAll(e => e.RarityLevel == rarity);
    }

    public List<EnchantmentData> GetAvailableEnchantments(int playerLevel)
    {
        return _allEnchantments.FindAll(e => e.RequiredLevel <= playerLevel);
    }

    public string GetRarityColor(EnchantmentData.Rarity rarity)
    {
        switch (rarity)
        {
            case EnchantmentData.Rarity.Common: return "#FFFFFF";
            case EnchantmentData.Rarity.Uncommon: return "#1EFF00";
            case EnchantmentData.Rarity.Rare: return "#0070FF";
            case EnchantmentData.Rarity.Epic: return "#A335EE";
            case EnchantmentData.Rarity.Legendary: return "#FF8000";
            default: return "#FFFFFF";
        }
    }

    public int GetRarityWeight(EnchantmentData.Rarity rarity)
    {
        switch (rarity)
        {
            case EnchantmentData.Rarity.Common: return 60;
            case EnchantmentData.Rarity.Uncommon: return 25;
            case EnchantmentData.Rarity.Rare: return 10;
            case EnchantmentData.Rarity.Epic: return 4;
            case EnchantmentData.Rarity.Legendary: return 1;
            default: return 0;
        }
    }
}
