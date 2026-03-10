using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Enchantment
{
    /// <summary>
    /// 附魔类型
    /// </summary>
    public enum EnchantmentType
    {
        Attack,      // 攻击附魔
        Defense,     // 防御附魔
        Magic,       // 魔法附魔
        Utility,     // 辅助附魔
        Legendary    // 传奇附魔
    }

    /// <summary>
    /// 附魔稀有度
    /// </summary>
    public enum EnchantmentRarity
    {
        Common,     // 普通
        Uncommon,   // 优秀
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }

    /// <summary>
    /// 附魔属性类型
    /// </summary>
    public enum EnchantmentAttribute
    {
        Damage,           // 伤害
        Defense,          // 防御
        Health,           // 生命
        Mana,             // 法力
        CriticalRate,     // 暴击率
        CriticalDamage,   // 暴击伤害
        AttackSpeed,      // 攻击速度
        MoveSpeed,        // 移动速度
        FireResistance,   // 火焰抗性
        IceResistance,    // 冰霜抗性
        LightningResistance, // 雷电抗性
        PoisonResistance, // 毒抗性
        AllAttributes     // 全属性
    }

    /// <summary>
    /// 附魔数据
    /// </summary>
    public class EnchantmentData
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EnchantmentType Type { get; set; }
        public EnchantmentRarity Rarity { get; set; }
        public EnchantmentAttribute Attribute { get; set; }
        public float AttributeValue { get; set; }
        public float SuccessRate { get; set; }
        public int Cost { get; set; }
        public int RequiredPlayerLevel { get; set; }
        public List<int> RequiredItemIds { get; set; }
        public int RequiredItemCount { get; set; }

        public Color GetRarityColor()
        {
            return Rarity switch
            {
                EnchantmentRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                EnchantmentRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                EnchantmentRarity.Rare => new Color(0.3f, 0.5f, 1.0f),
                EnchantmentRarity.Epic => new Color(0.6f, 0.3f, 0.9f),
                EnchantmentRarity.Legendary => new Color(1.0f, 0.6f, 0.0f),
                _ => Colors.White
            };
        }
    }

    /// <summary>
    /// 装备上的附魔实例
    /// </summary>
    public class EquipmentEnchantment
    {
        public int EnchantmentId { get; set; }
        public string EnchantmentName { get; set; }
        public EnchantmentRarity Rarity { get; set; }
        public EnchantmentAttribute Attribute { get; set; }
        public float AttributeValue { get; set; }
        public int Durability { get; set; }
        public int MaxDurability { get; set; }
    }

    /// <summary>
    /// 附魔数据库
    /// </summary>
    public class EnchantmentDatabase
    {
        private static EnchantmentDatabase _instance;
        public static EnchantmentDatabase Instance => _instance ??= new EnchantmentDatabase();

        private Dictionary<int, EnchantmentData> _enchantments = new Dictionary<int, EnchantmentData>();

        public EnchantmentDatabase()
        {
            InitializeEnchantments();
        }

        private void InitializeEnchantments()
        {
            // 攻击附魔
            AddEnchantment(new EnchantmentData
            {
                Id = 1,
                Name = "锋利",
                Description = "增加物理伤害",
                Type = EnchantmentType.Attack,
                Rarity = EnchantmentRarity.Common,
                Attribute = EnchantmentAttribute.Damage,
                AttributeValue = 5f,
                SuccessRate = 0.8f,
                Cost = 100,
                RequiredPlayerLevel = 1,
                RequiredItemIds = new List<int> { 301 },
                RequiredItemCount = 2
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 2,
                Name = "锐利",
                Description = "增加更多物理伤害",
                Type = EnchantmentType.Attack,
                Rarity = EnchantmentRarity.Uncommon,
                Attribute = EnchantmentAttribute.Damage,
                AttributeValue = 10f,
                SuccessRate = 0.7f,
                Cost = 250,
                RequiredPlayerLevel = 5,
                RequiredItemIds = new List<int> { 301, 302 },
                RequiredItemCount = 3
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 3,
                Name = "致命",
                Description = "增加暴击率",
                Type = EnchantmentType.Attack,
                Rarity = EnchantmentRarity.Rare,
                Attribute = EnchantmentAttribute.CriticalRate,
                AttributeValue = 5f,
                SuccessRate = 0.6f,
                Cost = 500,
                RequiredPlayerLevel = 10,
                RequiredItemIds = new List<int> { 302, 303 },
                RequiredItemCount = 2
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 4,
                Name = "撕裂",
                Description = "增加暴击伤害",
                Type = EnchantmentType.Attack,
                Rarity = EnchantmentRarity.Epic,
                Attribute = EnchantmentAttribute.CriticalDamage,
                AttributeValue = 20f,
                SuccessRate = 0.5f,
                Cost = 1000,
                RequiredPlayerLevel = 20,
                RequiredItemIds = new List<int> { 303, 304 },
                RequiredItemCount = 3
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 5,
                Name = "毁灭",
                Description = "大幅增加伤害",
                Type = EnchantmentType.Attack,
                Rarity = EnchantmentRarity.Legendary,
                Attribute = EnchantmentAttribute.Damage,
                AttributeValue = 30f,
                SuccessRate = 0.3f,
                Cost = 5000,
                RequiredPlayerLevel = 30,
                RequiredItemIds = new List<int> { 304, 305 },
                RequiredItemCount = 2
            });

            // 防御附魔
            AddEnchantment(new EnchantmentData
            {
                Id = 6,
                Name = "坚固",
                Description = "增加防御力",
                Type = EnchantmentType.Defense,
                Rarity = EnchantmentRarity.Common,
                Attribute = EnchantmentAttribute.Defense,
                AttributeValue = 5f,
                SuccessRate = 0.8f,
                Cost = 100,
                RequiredPlayerLevel = 1,
                RequiredItemIds = new List<int> { 301 },
                RequiredItemCount = 2
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 7,
                Name = "铁壁",
                Description = "增加更多防御力",
                Type = EnchantmentType.Defense,
                Rarity = EnchantmentRarity.Uncommon,
                Attribute = EnchantmentAttribute.Defense,
                AttributeValue = 10f,
                SuccessRate = 0.7f,
                Cost = 250,
                RequiredPlayerLevel = 5,
                RequiredItemIds = new List<int> { 301, 302 },
                RequiredItemCount = 3
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 8,
                Name = "重生",
                Description = "增加生命值",
                Type = EnchantmentType.Defense,
                Rarity = EnchantmentRarity.Rare,
                Attribute = EnchantmentAttribute.Health,
                AttributeValue = 50f,
                SuccessRate = 0.65f,
                Cost = 600,
                RequiredPlayerLevel = 12,
                RequiredItemIds = new List<int> { 302, 303 },
                RequiredItemCount = 3
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 9,
                Name = "护体",
                Description = "大幅增加生命值",
                Type = EnchantmentType.Defense,
                Rarity = EnchantmentRarity.Epic,
                Attribute = EnchantmentAttribute.Health,
                AttributeValue = 100f,
                SuccessRate = 0.5f,
                Cost = 1200,
                RequiredPlayerLevel = 22,
                RequiredItemIds = new List<int> { 303, 304 },
                RequiredItemCount = 3
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 10,
                Name = "不朽",
                Description = "极大增加生命值",
                Type = EnchantmentType.Defense,
                Rarity = EnchantmentRarity.Legendary,
                Attribute = EnchantmentAttribute.Health,
                AttributeValue = 200f,
                SuccessRate = 0.3f,
                Cost = 5000,
                RequiredPlayerLevel = 30,
                RequiredItemIds = new List<int> { 304, 305 },
                RequiredItemCount = 2
            });

            // 魔法附魔
            AddEnchantment(new EnchantmentData
            {
                Id = 11,
                Name = "魔力",
                Description = "增加法力值",
                Type = EnchantmentType.Magic,
                Rarity = EnchantmentRarity.Common,
                Attribute = EnchantmentAttribute.Mana,
                AttributeValue = 20f,
                SuccessRate = 0.8f,
                Cost = 100,
                RequiredPlayerLevel = 1,
                RequiredItemIds = new List<int> { 301 },
                RequiredItemCount = 2
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 12,
                Name = "、奥术",
                Description = "增加更多法力值",
                Type = EnchantmentType.Magic,
                Rarity = EnchantmentRarity.Uncommon,
                Attribute = EnchantmentAttribute.Mana,
                AttributeValue = 40f,
                SuccessRate = 0.7f,
                Cost = 250,
                RequiredPlayerLevel = 5,
                RequiredItemIds = new List<int> { 301, 302 },
                RequiredItemCount = 3
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 13,
                Name = "法力洪流",
                Description = "大幅增加法力值",
                Type = EnchantmentType.Magic,
                Rarity = EnchantmentRarity.Rare,
                Attribute = EnchantmentAttribute.Mana,
                AttributeValue = 80f,
                SuccessRate = 0.6f,
                Cost = 550,
                RequiredPlayerLevel = 10,
                RequiredItemIds = new List<int> { 302, 303 },
                RequiredItemCount = 2
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 14,
                Name = "奥术大师",
                Description = "极大增加法力值",
                Type = EnchantmentType.Magic,
                Rarity = EnchantmentRarity.Epic,
                Attribute = EnchantmentAttribute.Mana,
                AttributeValue = 150f,
                SuccessRate = 0.45f,
                Cost = 1100,
                RequiredPlayerLevel = 20,
                RequiredItemIds = new List<int> { 303, 304 },
                RequiredItemCount = 3
            });

            // 抗性附魔
            AddEnchantment(new EnchantmentData
            {
                Id = 15,
                Name = "火焰抗性",
                Description = "减少火焰伤害",
                Type = EnchantmentType.Utility,
                Rarity = EnchantmentRarity.Common,
                Attribute = EnchantmentAttribute.FireResistance,
                AttributeValue = 10f,
                SuccessRate = 0.8f,
                Cost = 120,
                RequiredPlayerLevel = 3,
                RequiredItemIds = new List<int> { 301 },
                RequiredItemCount = 2
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 16,
                Name = "冰霜抗性",
                Description = "减少冰霜伤害",
                Type = EnchantmentType.Utility,
                Rarity = EnchantmentRarity.Common,
                Attribute = EnchantmentAttribute.IceResistance,
                AttributeValue = 10f,
                SuccessRate = 0.8f,
                Cost = 120,
                RequiredPlayerLevel = 3,
                RequiredItemIds = new List<int> { 301 },
                RequiredItemCount = 2
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 17,
                Name = "雷电抗性",
                Description = "减少雷电伤害",
                Type = EnchantmentType.Utility,
                Rarity = EnchantmentRarity.Common,
                Attribute = EnchantmentAttribute.LightningResistance,
                AttributeValue = 10f,
                SuccessRate = 0.8f,
                Cost = 120,
                RequiredPlayerLevel = 3,
                RequiredItemIds = new List<int> { 301 },
                RequiredItemCount = 2
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 18,
                Name = "毒素抗性",
                Description = "减少毒素伤害",
                Type = EnchantmentType.Utility,
                Rarity = EnchantmentRarity.Common,
                Attribute = EnchantmentAttribute.PoisonResistance,
                AttributeValue = 10f,
                SuccessRate = 0.8f,
                Cost = 120,
                RequiredPlayerLevel = 3,
                RequiredItemIds = new List<int> { 301 },
                RequiredItemCount = 2
            });

            // 传奇附魔
            AddEnchantment(new EnchantmentData
            {
                Id = 19,
                Name = "全知",
                Description = "增加所有属性",
                Type = EnchantmentType.Legendary,
                Rarity = EnchantmentRarity.Legendary,
                Attribute = EnchantmentAttribute.AllAttributes,
                AttributeValue = 15f,
                SuccessRate = 0.25f,
                Cost = 8000,
                RequiredPlayerLevel = 35,
                RequiredItemIds = new List<int> { 305 },
                RequiredItemCount = 5
            });

            AddEnchantment(new EnchantmentData
            {
                Id = 20,
                Name = "极速",
                Description = "增加攻击速度和移动速度",
                Type = EnchantmentType.Legendary,
                Rarity = EnchantmentRarity.Legendary,
                Attribute = EnchantmentAttribute.AttackSpeed,
                AttributeValue = 15f,
                SuccessRate = 0.28f,
                Cost = 7000,
                RequiredPlayerLevel = 32,
                RequiredItemIds = new List<int> { 304, 305 },
                RequiredItemCount = 3
            });
        }

        private void AddEnchantment(EnchantmentData enchantment)
        {
            _enchantments[enchantment.Id] = enchantment;
        }

        public EnchantmentData GetEnchantment(int id)
        {
            return _enchantments.ContainsKey(id) ? _enchantments[id] : null;
        }

        public List<EnchantmentData> GetAllEnchantments()
        {
            return new List<EnchantmentData>(_enchantments.Values);
        }

        public List<EnchantmentData> GetEnchantmentsByType(EnchantmentType type)
        {
            List<EnchantmentData> result = new List<EnchantmentData>();
            foreach (var e in _enchantments.Values)
            {
                if (e.Type == type)
                    result.Add(e);
            }
            return result;
        }

        public List<EnchantmentData> GetAvailableEnchantments(int playerLevel)
        {
            List<EnchantmentData> result = new List<EnchantmentData>();
            foreach (var e in _enchantments.Values)
            {
                if (e.RequiredPlayerLevel <= playerLevel)
                    result.Add(e);
            }
            return result;
        }
    }
}
