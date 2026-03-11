using Godot;
using System;
using System.Collections.Generic;

namespace Game.Scripts.Systems.EquipmentReforging
{
    /// <summary>
    /// 装备洗练数据库
    /// </summary>
    public static class EquipmentReforgingDatabase
    {
        // 基础属性池
        private static List<ReforgeAttribute> _basicAttributes = new List<ReforgeAttribute>
        {
            new ReforgeAttribute { Name = "Attack", MinValue = 5, MaxValue = 50, Weight = 1.0f },
            new ReforgeAttribute { Name = "Defense", MinValue = 5, MaxValue = 45, Weight = 1.0f },
            new ReforgeAttribute { Name = "Health", MinValue = 50, MaxValue = 500, Weight = 1.2f },
            new ReforgeAttribute { Name = "Magic", MinValue = 5, MaxValue = 40, Weight = 0.9f },
            new ReforgeAttribute { Name = "Speed", MinValue = 1, MaxValue = 15, Weight = 0.8f }
        };

        // 稀有属性池
        private static List<ReforgeAttribute> _rareAttributes = new List<ReforgeAttribute>
        {
            new ReforgeAttribute { Name = "CriticalRate", MinValue = 1, MaxValue = 15, Weight = 1.5f },
            new ReforgeAttribute { Name = "CriticalDamage", MinValue = 10, MaxValue = 50, Weight = 1.3f },
            new ReforgeAttribute { Name = "LifeSteal", MinValue = 1, MaxValue = 10, Weight = 1.2f },
            new ReforgeAttribute { Name = "Dodge", MinValue = 1, MaxValue = 12, Weight = 1.1f },
            new ReforgeAttribute { Name = "Resilience", MinValue = 1, MaxValue = 10, Weight = 1.0f }
        };

        // 高级属性池(包含稀有属性)
        private static List<ReforgeAttribute> _advancedAttributes;

        // 传奇属性池(包含所有属性,数值更高)
        private static List<ReforgeAttribute> _legendaryAttributes;

        static EquipmentReforgingDatabase()
        {
            InitializeAdvancedAttributes();
            InitializeLegendaryAttributes();
        }

        private static void InitializeAdvancedAttributes()
        {
            _advancedAttributes = new List<ReforgeAttribute>();
            _advancedAttributes.AddRange(_basicAttributes);
            _advancedAttributes.AddRange(_rareAttributes);
            // 增强数值
            foreach (var attr in _advancedAttributes)
            {
                attr.MinValue = (float)(attr.MinValue * 1.5);
                attr.MaxValue = (float)(attr.MaxValue * 1.5);
            }
        }

        private static void InitializeLegendaryAttributes()
        {
            _legendaryAttributes = new List<ReforgeAttribute>();
            _legendaryAttributes.AddRange(_advancedAttributes);
            // 添加特殊属性
            _legendaryAttributes.Add(new ReforgeAttribute { Name = "AttackSpeed", MinValue = 1, MaxValue = 20, Weight = 1.8f });
            _legendaryAttributes.Add(new ReforgeAttribute { Name = "MoveSpeed", MinValue = 2, MaxValue = 25, Weight = 1.6f });
            // 进一步增强数值
            foreach (var attr in _legendaryAttributes)
            {
                attr.MinValue = (float)(attr.MinValue * 1.3);
                attr.MaxValue = (float)(attr.MaxValue * 1.3);
            }
        }

        /// <summary>
        /// 获取洗练配方
        /// </summary>
        public static ReforgeRecipe GetRecipe(ReforgeType type, ReforgeRarity rarity)
        {
            var recipe = new ReforgeRecipe
            {
                Type = type,
                Rarity = rarity,
                RequiredMaterials = new List<string>(),
                MaterialCosts = new Dictionary<string, int>(),
                AvailableAttributes = GetAttributesForType(type)
            };

            switch (type)
            {
                case ReforgeType.Basic:
                    recipe.GoldCost = GetGoldCostForRarity(rarity, 50);
                    recipe.SuccessRate = GetSuccessRateForRarity(rarity, 0.95f);
                    recipe.MaterialCosts["reforge_stone"] = 1;
                    break;

                case ReforgeType.Advanced:
                    recipe.GoldCost = GetGoldCostForRarity(rarity, 200);
                    recipe.SuccessRate = GetSuccessRateForRarity(rarity, 0.80f);
                    recipe.MaterialCosts["reforge_stone"] = 3;
                    recipe.MaterialCosts["reforge_crystal"] = 1;
                    break;

                case ReforgeType.Legendary:
                    recipe.GoldCost = GetGoldCostForRarity(rarity, 1000);
                    recipe.SuccessRate = GetSuccessRateForRarity(rarity, 0.60f);
                    recipe.MaterialCosts["reforge_stone"] = 5;
                    recipe.MaterialCosts["reforge_crystal"] = 3;
                    recipe.MaterialCosts["reforge_orb"] = 1;
                    break;
            }

            return recipe;
        }

        private static int GetGoldCostForRarity(ReforgeRarity rarity, int baseCost)
        {
            float multiplier = rarity switch
            {
                ReforgeRarity.Common => 1.0f,
                ReforgeRarity.Uncommon => 1.5f,
                ReforgeRarity.Rare => 2.5f,
                ReforgeRarity.Epic => 4.0f,
                ReforgeRarity.Legendary => 8.0f,
                _ => 1.0f
            };
            return (int)(baseCost * multiplier);
        }

        private static float GetSuccessRateForRarity(ReforgeRarity rarity, float baseRate)
        {
            float reduction = rarity switch
            {
                ReforgeRarity.Common => 0.0f,
                ReforgeRarity.Uncommon => 0.05f,
                ReforgeRarity.Rare => 0.10f,
                ReforgeRarity.Epic => 0.20f,
                ReforgeRarity.Legendary => 0.35f,
                _ => 0.0f
            };
            return Math.Max(0.1f, baseRate - reduction);
        }

        /// <summary>
        /// 获取指定类型可用的属性列表
        /// </summary>
        public static List<ReforgeAttribute> GetAttributesForType(ReforgeType type)
        {
            return type switch
            {
                ReforgeType.Basic => new List<ReforgeAttribute>(_basicAttributes),
                ReforgeType.Advanced => new List<ReforgeAttribute>(_advancedAttributes),
                ReforgeType.Legendary => new List<ReforgeAttribute>(_legendaryAttributes),
                _ => new List<ReforgeAttribute>(_basicAttributes)
            };
        }

        /// <summary>
        /// 获取随机属性(基于权重)
        /// </summary>
        public static ReforgeAttribute GetRandomAttribute(ReforgeType type)
        {
            var attributes = GetAttributesForType(type);
            float totalWeight = 0;
            foreach (var attr in attributes)
            {
                totalWeight += attr.Weight;
            }

            float randomValue = (float)GD.RandDouble() * totalWeight;
            float cumulative = 0;

            foreach (var attr in attributes)
            {
                cumulative += attr.Weight;
                if (randomValue <= cumulative)
                {
                    return attr;
                }
            }

            return attributes[0];
        }

        /// <summary>
        /// 计算装备稀有度
        /// </summary>
        public static ReforgeRarity CalculateRarity(Dictionary<string, float> attributes)
        {
            if (attributes == null || attributes.Count == 0)
                return ReforgeRarity.Common;

            int rareCount = 0;
            foreach (var attr in attributes.Keys)
            {
                if (_rareAttributes.Exists(a => a.Name == attr))
                    rareCount++;
            }

            return rareCount switch
            {
                0 => ReforgeRarity.Common,
                1 => ReforgeRarity.Uncommon,
                2 => ReforgeRarity.Rare,
                3 or 4 => ReforgeRarity.Epic,
                _ => ReforgeRarity.Legendary
            };
        }
    }
}
