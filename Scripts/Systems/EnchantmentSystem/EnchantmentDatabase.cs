using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 附魔数据库
    /// </summary>
    public class EnchantmentDatabase {
        private static EnchantmentDatabase _instance;
        public static EnchantmentDatabase Instance {
            get {
                if (_instance == null) {
                    _instance = new EnchantmentDatabase();
                }
                return _instance;
            }
        }

        // 附魔数据库
        private Dictionary<int, EnchantmentData> _database;
        // 按类型索引
        private Dictionary<EnchantmentType, List<EnchantmentData>> _byType;
        // 按稀有度索引
        private Dictionary<EnchantmentRarity, List<EnchantmentData>> _byRarity;

        public EnchantmentDatabase() {
            _database = new Dictionary<int, EnchantmentData>();
            _byType = new Dictionary<EnchantmentType, List<EnchantmentData>>();
            _byRarity = new Dictionary<EnchantmentRarity, List<EnchantmentData>>();
            
            InitializeDatabase();
        }

        /// <summary>
        /// 初始化附魔数据库
        /// </summary>
        private void InitializeDatabase() {
            // 武器附魔
            AddEnchantment(new EnchantmentData {
                Id = 1,
                Name = "锋利",
                Description = "增加物理伤害",
                Type = EnchantmentType.Weapon,
                Rarity = EnchantmentRarity.Common,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Damage, 10 }
                },
                RequiredLevel = 1,
                GoldCost = 100,
                Tags = new List<string> { "物理", "伤害" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 2,
                Name = "闪电",
                Description = "攻击时有几率释放闪电",
                Type = EnchantmentType.Weapon,
                Rarity = EnchantmentRarity.Rare,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Damage, 15 },
                    { EnchantmentAttribute.ThunderResistance, 5 }
                },
                RequiredLevel = 10,
                GoldCost = 500,
                Tags = new List<string> { "闪电", "元素" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 3,
                Name = "燃烧",
                Description = "攻击时附加燃烧伤害",
                Type = EnchantmentType.Weapon,
                Rarity = EnchantmentRarity.Rare,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Damage, 12 },
                    { EnchantmentAttribute.FireResistance, 5 }
                },
                RequiredLevel = 15,
                GoldCost = 600,
                Tags = new List<string> { "火焰", "持续伤害" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 4,
                Name = "冰霜",
                Description = "攻击时有几率冰冻敌人",
                Type = EnchantmentType.Weapon,
                Rarity = EnchantmentRarity.Epic,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Damage, 18 },
                    { EnchantmentAttribute.IceResistance, 8 }
                },
                RequiredLevel = 20,
                GoldCost = 1000,
                Tags = new List<string> { "冰霜", "控制" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 5,
                Name = "暗影",
                Description = "攻击时汲取敌人生命",
                Type = EnchantmentType.Weapon,
                Rarity = EnchantmentRarity.Epic,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Damage, 20 },
                    { EnchantmentAttribute.HealthSteal, 5 }
                },
                RequiredLevel = 25,
                GoldCost = 1500,
                Tags = new List<string> { "暗影", "吸血" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 6,
                Name = "神圣",
                Description = "对邪恶生物造成额外伤害",
                Type = EnchantmentType.Weapon,
                Rarity = EnchantmentRarity.Legendary,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Damage, 30 },
                    { EnchantmentAttribute.CriticalRate, 5 },
                    { EnchantmentAttribute.LightResistance, 10 }
                },
                RequiredLevel = 30,
                GoldCost = 3000,
                Tags = new List<string> { "神圣", "对邪" }
            });

            // 防具附魔
            AddEnchantment(new EnchantmentData {
                Id = 11,
                Name = "坚固",
                Description = "增加防御力",
                Type = EnchantmentType.Armor,
                Rarity = EnchantmentRarity.Common,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Defense, 10 }
                },
                RequiredLevel = 1,
                GoldCost = 100,
                Tags = new List<string> { "防御", "物理" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 12,
                Name = "生命",
                Description = "增加最大生命值",
                Type = EnchantmentType.Armor,
                Rarity = EnchantmentRarity.Uncommon,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Health, 50 }
                },
                RequiredLevel = 5,
                GoldCost = 200,
                Tags = new List<string> { "生命", "生存" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 13,
                Name = "再生",
                Description = "生命值持续恢复",
                Type = EnchantmentType.Armor,
                Rarity = EnchantmentRarity.Rare,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.HealthRegen, 2 },
                    { EnchantmentAttribute.Health, 30 }
                },
                RequiredLevel = 15,
                GoldCost = 600,
                Tags = new List<string> { "恢复", "生存" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 14,
                Name = "冰盾",
                Description = "受到攻击时反击冰霜伤害",
                Type = EnchantmentType.Armor,
                Rarity = EnchantmentRarity.Epic,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Defense, 20 },
                    { EnchantmentAttribute.IceResistance, 15 }
                },
                RequiredLevel = 25,
                GoldCost = 1500,
                Tags = new List<string> { "冰霜", "反击" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 15,
                Name = "魔法屏障",
                Description = "减少受到的魔法伤害",
                Type = EnchantmentType.Armor,
                Rarity = EnchantmentRarity.Legendary,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Defense, 25 },
                    { EnchantmentAttribute.Mana, 50 },
                    { EnchantmentAttribute.ManaRegen, 3 }
                },
                RequiredLevel = 30,
                GoldCost = 3000,
                Tags = new List<string> { "魔法", "防御" }
            });

            // 饰品附魔
            AddEnchantment(new EnchantmentData {
                Id = 21,
                Name = "敏捷",
                Description = "增加移动速度",
                Type = EnchantmentType.Accessory,
                Rarity = EnchantmentRarity.Common,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.MoveSpeed, 5 }
                },
                RequiredLevel = 1,
                GoldCost = 100,
                Tags = new List<string> { "移动", "速度" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 22,
                Name = "集中",
                Description = "增加暴击率",
                Type = EnchantmentType.Accessory,
                Rarity = EnchantmentRarity.Uncommon,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.CriticalRate, 3 }
                },
                RequiredLevel = 10,
                GoldCost = 300,
                Tags = new List<string> { "暴击", "攻击" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 23,
                Name = "破坏",
                Description = "增加暴击伤害",
                Type = EnchantmentType.Accessory,
                Rarity = EnchantmentRarity.Rare,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.CriticalDamage, 15 }
                },
                RequiredLevel = 15,
                GoldCost = 700,
                Tags = new List<string> { "暴击", "伤害" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 24,
                Name = "法力之泉",
                Description = "法力持续恢复",
                Type = EnchantmentType.Accessory,
                Rarity = EnchantmentRarity.Epic,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Mana, 80 },
                    { EnchantmentAttribute.ManaRegen, 4 }
                },
                RequiredLevel = 20,
                GoldCost = 1200,
                Tags = new List<string> { "法力", "恢复" }
            });

            AddEnchantment(new EnchantmentData {
                Id = 25,
                Name = "全能",
                Description = "全属性提升",
                Type = EnchantmentType.Accessory,
                Rarity = EnchantmentRarity.Legendary,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.Damage, 10 },
                    { EnchantmentAttribute.Defense, 10 },
                    { EnchantmentAttribute.Health, 50 },
                    { EnchantmentAttribute.CriticalRate, 3 },
                    { EnchantmentAttribute.MoveSpeed, 3 }
                },
                RequiredLevel = 30,
                GoldCost = 3500,
                Tags = new List<string> { "全能", "顶级" }
            });

            // 通用附魔
            AddEnchantment(new EnchantmentData {
                Id = 31,
                Name = "和谐",
                Description = "减少所有受到的伤害",
                Type = EnchantmentType.Universal,
                Rarity = EnchantmentRarity.Epic,
                Attributes = new Dictionary<EnchantmentAttribute, float> {
                    { EnchantmentAttribute.FireResistance, 10 },
                    { EnchantmentAttribute.IceResistance, 10 },
                    { EnchantmentAttribute.ThunderResistance, 10 },
                    { EnchantmentAttribute.DarkResistance, 10 },
                    { EnchantmentAttribute.LightResistance, 10 }
                },
                RequiredLevel = 25,
                GoldCost = 2000,
                Tags = new List<string> { "全抗", "防御" }
            });
        }

        /// <summary>
        /// 添加附魔到数据库
        /// </summary>
        private void AddEnchantment(EnchantmentData enchantment) {
            _database[enchantment.Id] = enchantment;

            // 按类型索引
            if (!_byType.ContainsKey(enchantment.Type)) {
                _byType[enchantment.Type] = new List<EnchantmentData>();
            }
            _byType[enchantment.Type].Add(enchantment);

            // 按稀有度索引
            if (!_byRarity.ContainsKey(enchantment.Rarity)) {
                _byRarity[enchantment.Rarity] = new List<EnchantmentData>();
            }
            _byRarity[enchantment.Rarity].Add(enchantment);
        }

        /// <summary>
        /// 根据ID获取附魔
        /// </summary>
        public EnchantmentData GetEnchantment(int id) {
            if (_database.ContainsKey(id)) {
                return _database[id];
            }
            return null;
        }

        /// <summary>
        /// 根据类型获取附魔列表
        /// </summary>
        public List<EnchantmentData> GetEnchantmentsByType(EnchantmentType type) {
            if (_byType.ContainsKey(type)) {
                return new List<EnchantmentData>(_byType[type]);
            }
            return new List<EnchantmentData>();
        }

        /// <summary>
        /// 根据稀有度获取附魔列表
        /// </summary>
        public List<EnchantmentData> GetEnchantmentsByRarity(EnchantmentRarity rarity) {
            if (_byRarity.ContainsKey(rarity)) {
                return new List<EnchantmentData>(_byRarity[rarity]);
            }
            return new List<EnchantmentData>();
        }

        /// <summary>
        /// 获取玩家等级可用的附魔
        /// </summary>
        public List<EnchantmentData> GetAvailableEnchantments(int playerLevel) {
            List<EnchantmentData> available = new List<EnchantmentData>();
            foreach (var enchant in _database.Values) {
                if (enchant.RequiredLevel <= playerLevel) {
                    available.Add(enchant);
                }
            }
            return available;
        }

        /// <summary>
        /// 随机获取一个附魔（用于附魔台）
        /// </summary>
        public EnchantmentData GetRandomEnchantment(int playerLevel, EnchantmentType? type = null) {
            var candidates = GetAvailableEnchantments(playerLevel);
            if (type.HasValue) {
                candidates.RemoveAll(e => e.Type != type.Value);
            }
            if (candidates.Count == 0) return null;
            
            // 根据稀有度权重随机
            float totalWeight = 0;
            Dictionary<EnchantmentData, float> weights = new Dictionary<EnchantmentData, float>();
            foreach (var e in candidates) {
                float weight = GetRarityWeight(e.Rarity);
                weights[e] = weight;
                totalWeight += weight;
            }
            
            float random = (float)GD.RandDouble() * totalWeight;
            foreach (var kvp in weights) {
                random -= kvp.Value;
                if (random <= 0) {
                    return kvp.Key;
                }
            }
            return candidates[0];
        }

        /// <summary>
        /// 获取稀有度权重
        /// </summary>
        private float GetRarityWeight(EnchantmentRarity rarity) {
            switch (rarity) {
                case EnchantmentRarity.Common: return 50;
                case EnchantmentRarity.Uncommon: return 30;
                case EnchantmentRarity.Rare: return 15;
                case EnchantmentRarity.Epic: return 4;
                case EnchantmentRarity.Legendary: return 1;
                default: return 10;
            }
        }

        /// <summary>
        /// 获取所有附魔
        /// </summary>
        public List<EnchantmentData> GetAllEnchantments() {
            return new List<EnchantmentData>(_database.Values);
        }
    }
}
