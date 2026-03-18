// ============================================
// Artifact Database - 神器配置数据库
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Systems.Artifact
{
    public static class ArtifactDatabase
    {
        // 基础属性加成表
        private static readonly Dictionary<ArtifactRarity, float> RarityMultiplier = new()
        {
            { ArtifactRarity.Common, 1.0f },
            { ArtifactRarity.Uncommon, 1.5f },
            { ArtifactRarity.Rare, 2.0f },
            { ArtifactRarity.Epic, 3.0f },
            { ArtifactRarity.Legendary, 5.0f },
            { ArtifactRarity.Mythic, 10.0f }
        };

        // 强化成功率表
        private static readonly Dictionary<int, float> ForgeSuccessRate = new()
        {
            { 1, 0.95f },
            { 2, 0.90f },
            { 3, 0.85f },
            { 4, 0.80f },
            { 5, 0.75f },
            { 6, 0.70f },
            { 7, 0.60f },
            { 8, 0.50f },
            { 9, 0.40f },
            { 10, 0.30f }
        };

        // 强化等级需求金币
        private static readonly Dictionary<int, long> ForgeGoldCost = new()
        {
            { 1, 1000 },
            { 2, 2500 },
            { 3, 5000 },
            { 4, 10000 },
            { 5, 20000 },
            { 6, 40000 },
            { 7, 80000 },
            { 8, 150000 },
            { 9, 300000 },
            { 10, 500000 }
        };

        // 神器配置
        private static readonly List<ArtifactConfig> _artifactConfigs = new()
        {
            // 武器神器
            new ArtifactConfig { Id = "art_001", Name = "黎明之刃", Description = "由第一缕阳光淬炼而成的神剑", Type = ArtifactType.Weapon, Slot = ArtifactSlot.Primary, Rarity = ArtifactRarity.Legendary, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 50 } } },
            new ArtifactConfig { Id = "art_002", Name = "暗影收割者", Description = "收割灵魂的黑暗镰刀", Type = ArtifactType.Weapon, Slot = ArtifactSlot.Primary, Rarity = ArtifactRarity.Epic, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 35 }, new() { Type = ArtifactEffectType.LifeSteal, BaseValue = 10 } } },
            new ArtifactConfig { Id = "art_003", Name = "雷霆战斧", Description = "蕴含雷电之力的战斧", Type = ArtifactType.Weapon, Slot = ArtifactSlot.Primary, Rarity = ArtifactRarity.Rare, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 25 }, new() { Type = ArtifactEffectType.LightningResistance, BaseValue = 15 } } },
            new ArtifactConfig { Id = "art_004", Name = "寒冰之矛", Description = "永不融化的冰霜长矛", Type = ArtifactType.Weapon, Slot = ArtifactSlot.Primary, Rarity = ArtifactRarity.Rare, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 25 }, new() { Type = ArtifactEffectType.IceResistance, BaseValue = 15 } } },
            new ArtifactConfig { Id = "art_005", Name = "新手之剑", Description = "最简单的铁剑", Type = ArtifactType.Weapon, Slot = ArtifactSlot.Primary, Rarity = ArtifactRarity.Common, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 10 } } },

            // 护甲神器
            new ArtifactConfig { Id = "art_006", Name = "泰坦护甲", Description = "古老泰坦打造的坚不可摧铠甲", Type = ArtifactType.Armor, Slot = ArtifactSlot.Chest, Rarity = ArtifactRarity.Legendary, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DefenseIncrease, BaseValue = 60 } } },
            new ArtifactConfig { Id = "art_007", Name = "幽灵斗篷", Description = "让穿戴者隐入阴影的斗篷", Type = ArtifactType.Armor, Slot = ArtifactSlot.Chest, Rarity = ArtifactRarity.Epic, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DodgeRate, BaseValue = 20 }, new() { Type = ArtifactEffectType.DefenseIncrease, BaseValue = 15 } } },
            new ArtifactConfig { Id = "art_008", Name = "火焰披风", Description = "由烈焰编织而成的披风", Type = ArtifactType.Armor, Slot = ArtifactSlot.Chest, Rarity = ArtifactRarity.Rare, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.FireResistance, BaseValue = 25 }, new() { Type = ArtifactEffectType.DefenseIncrease, BaseValue = 15 } } },
            new ArtifactConfig { Id = "art_009", Name = "皮甲", Description = "最基本的防护装备", Type = ArtifactType.Armor, Slot = ArtifactSlot.Chest, Rarity = ArtifactRarity.Common, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DefenseIncrease, BaseValue = 8 } } },

            // 饰品神器
            new ArtifactConfig { Id = "art_010", Name = "时间沙漏", Description = "掌控时间的古老神器", Type = ArtifactType.Accessory, Slot = ArtifactSlot.Amulet, Rarity = ArtifactRarity.Mythic, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.CooldownReduction, BaseValue = 30 }, new() { Type = ArtifactEffectType.AttackSpeed, BaseValue = 20 } } },
            new ArtifactConfig { Id = "art_011", Name = "生命指环", Description = "蕴含生命能量的戒指", Type = ArtifactType.Accessory, Slot = ArtifactSlot.Ring1, Rarity = ArtifactRarity.Legendary, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.HealthMax, BaseValue = 500 }, new() { Type = ArtifactEffectType.HealthRegen, BaseValue = 20 } } },
            new ArtifactConfig { Id = "art_012", Name = "法力水晶", Description = "充盈着魔力的水晶", Type = ArtifactType.Accessory, Slot = ArtifactSlot.Amulet, Rarity = ArtifactRarity.Epic, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.ManaMax, BaseValue = 300 }, new() { Type = ArtifactEffectType.ManaRegen, BaseValue = 15 } } },
            new ArtifactConfig { Id = "art_013", Name = "暴击项链", Description = "增加暴击几率的神秘项链", Type = ArtifactType.Accessory, Slot = ArtifactSlot.Amulet, Rarity = ArtifactRarity.Rare, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.CriticalRate, BaseValue = 15 } } },
            new ArtifactConfig { Id = "art_014", Name = "铁戒指", Description = "简单的金属戒指", Type = ArtifactType.Accessory, Slot = ArtifactSlot.Ring1, Rarity = ArtifactRarity.Common, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.AllAttributes, BaseValue = 5 } } },

            // 遗物神器
            new ArtifactConfig { Id = "art_015", Name = "远古之骨", Description = "蕴含古老力量的遗骨", Type = ArtifactType.Relic, Slot = ArtifactSlot.Relic1, Rarity = ArtifactRarity.Legendary, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 25 }, new() { Type = ArtifactEffectType.CriticalDamage, BaseValue = 50 } } },
            new ArtifactConfig { Id = "art_016", Name = "龙之心", Description = "巨龙的心脏化石", Type = ArtifactType.Relic, Slot = ArtifactSlot.Relic1, Rarity = ArtifactRarity.Epic, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.HealthMax, BaseValue = 300 }, new() { Type = ArtifactEffectType.FireResistance, BaseValue = 30 } } },
            new ArtifactConfig { Id = "art_017", Name = "精灵之尘", Description = "精灵族遗留的魔法尘埃", Type = ArtifactType.Relic, Slot = ArtifactSlot.Relic1, Rarity = ArtifactRarity.Rare, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.ManaRegen, BaseValue = 10 }, new() { Type = ArtifactEffectType.ExperienceGain, BaseValue = 10 } } },

            // 契约神器
            new ArtifactConfig { Id = "art_018", Name = "恶魔契约", Description = "与深渊恶魔签订的契约", Type = ArtifactType.Covenant, Slot = ArtifactSlot.Relic2, Rarity = ArtifactRarity.Mythic, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 40 }, new() { Type = ArtifactEffectType.LifeSteal, BaseValue = 20 }, new() { Type = ArtifactEffectType.ManaSteal, BaseValue = 15 } } },
            new ArtifactConfig { Id = "art_019", Name = "天使盟约", Description = "与光明诸神签订的契约", Type = ArtifactType.Covenant, Slot = ArtifactSlot.Relic2, Rarity = ArtifactRarity.Mythic, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.HealthMax, BaseValue = 600 }, new() { Type = ArtifactEffectType.HealEffect, BaseValue = 30 }, new() { Type = ArtifactEffectType.AllAttributes, BaseValue = 30 } } },

            // 传说神器
            new ArtifactConfig { Id = "art_020", Name = "创世之书", Description = "记载世界起源的神秘书籍", Type = ArtifactType.Legendary, Slot = ArtifactSlot.Amulet, Rarity = ArtifactRarity.Mythic, BaseEffects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.AllAttributes, BaseValue = 50 }, new() { Type = ArtifactEffectType.ExperienceGain, BaseValue = 25 }, new() { Type = ArtifactEffectType.GoldGain, BaseValue = 25 } } }
        };

        // 神器套装配置
        private static readonly List<ArtifactSetConfig> _setConfigs = new()
        {
            new ArtifactSetConfig
            {
                Id = "set_001",
                Name = "泰坦之力",
                RequiredArtifacts = new List<string> { "art_001", "art_006" },
                Bonuses = new List<SetBonusConfig>
                {
                    new() { PieceCount = 2, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 20 } } },
                    new() { PieceCount = 4, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 30 }, new() { Type = ArtifactEffectType.DefenseIncrease, BaseValue = 20 } } }
                }
            },
            new ArtifactSetConfig
            {
                Id = "set_002",
                Name = "元素之主",
                RequiredArtifacts = new List<string> { "art_004", "art_008", "art_012" },
                Bonuses = new List<SetBonusConfig>
                {
                    new() { PieceCount = 2, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.FireResistance, BaseValue = 20 }, new() { Type = ArtifactEffectType.IceResistance, BaseValue = 20 } } },
                    new() { PieceCount = 3, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.LightningResistance, BaseValue = 20 }, new() { Type = ArtifactEffectType.AllAttributes, BaseValue = 15 } } }
                }
            },
            new ArtifactSetConfig
            {
                Id = "set_003",
                Name = "暗影猎手",
                RequiredArtifacts = new List<string> { "art_002", "art_007" },
                Bonuses = new List<SetBonusConfig>
                {
                    new() { PieceCount = 2, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.DodgeRate, BaseValue = 15 }, new() { Type = ArtifactEffectType.CriticalRate, BaseValue = 10 } } }
                }
            },
            new ArtifactSetConfig
            {
                Id = "set_004",
                Name = "财富之路",
                RequiredArtifacts = new List<string> { "art_011", "art_014", "art_020" },
                Bonuses = new List<SetBonusConfig>
                {
                    new() { PieceCount = 2, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.GoldGain, BaseValue = 15 } } },
                    new() { PieceCount = 3, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.GoldGain, BaseValue = 25 }, new() { Type = ArtifactEffectType.DropRate, BaseValue = 10 } } }
                }
            },
            new ArtifactSetConfig
            {
                Id = "set_005",
                Name = "全能者",
                RequiredArtifacts = new List<string> { "art_010", "art_015", "art_018", "art_019", "art_020" },
                Bonuses = new List<SetBonusConfig>
                {
                    new() { PieceCount = 2, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.AllAttributes, BaseValue = 20 } } },
                    new() { PieceCount = 3, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.AllAttributes, BaseValue = 25 }, new() { Type = ArtifactEffectType.HealthMax, BaseValue = 200 } } },
                    new() { PieceCount = 4, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.AllAttributes, BaseValue = 30 }, new() { Type = ArtifactEffectType.DamageIncrease, BaseValue = 20 } } },
                    new() { PieceCount = 5, Effects = new List<ArtifactEffectConfig> { new() { Type = ArtifactEffectType.AllAttributes, BaseValue = 50 }, new() { Type = ArtifactEffectType.ExperienceGain, BaseValue = 30 } } }
                }
            }
        };

        public static Artifact GetArtifactById(string id)
        {
            var config = _artifactConfigs.FirstOrDefault(a => a.Id == id);
            if (config == null) return null;

            return CreateArtifactFromConfig(config);
        }

        public static List<Artifact> GetAllArtifacts()
        {
            return _artifactConfigs.Select(CreateArtifactFromConfig).ToList();
        }

        public static ArtifactSet GetSetById(string id)
        {
            var config = _setConfigs.FirstOrDefault(s => s.Id == id);
            if (config == null) return null;

            return new ArtifactSet
            {
                Id = config.Id,
                Name = config.Name,
                PieceCount = config.RequiredArtifacts.Count,
                SetBonuses = new Dictionary<ArtifactSetBonusType, List<ArtifactEffect>>()
            };
        }

        public static List<ArtifactSet> GetAllSets()
        {
            return _setConfigs.Select(GetSetById).ToList();
        }

        public static float GetRarityMultiplier(ArtifactRarity rarity)
        {
            return RarityMultiplier.GetValueOrDefault(rarity, 1.0f);
        }

        public static float GetForgeSuccessRate(int level)
        {
            return ForgeSuccessRate.GetValueOrDefault(level, 0.3f);
        }

        public static long GetForgeGoldCost(int level)
        {
            return ForgeGoldCost.GetValueOrDefault(level, 500000);
        }

        public static Artifact DropRandomArtifact(int playerLevel)
        {
            var random = new Random();
            var available = _artifactConfigs.Where(a => 
                (int)a.Rarity <= (playerLevel / 10) + 2
            ).ToList();

            if (!available.Any())
                available = _artifactConfigs;

            var weights = available.Select(a => 
                a.Rarity switch
                {
                    ArtifactRarity.Common => 40,
                    ArtifactRarity.Uncommon => 30,
                    ArtifactRarity.Rare => 18,
                    ArtifactRarity.Epic => 8,
                    ArtifactRarity.Legendary => 3,
                    ArtifactRarity.Mythic => 1,
                    _ => 10
                }
            ).ToList();

            var totalWeight = weights.Sum();
            var roll = random.NextDouble() * totalWeight;
            var selected = available[0];

            double cumulative = 0;
            for (int i = 0; i < available.Count; i++)
            {
                cumulative += weights[i];
                if (roll <= cumulative)
                {
                    selected = available[i];
                    break;
                }
            }

            return CreateArtifactFromConfig(selected);
        }

        private static Artifact CreateArtifactFromConfig(ArtifactConfig config)
        {
            var multiplier = GetRarityMultiplier(config.Rarity);
            var effects = config.BaseEffects.Select(e => new ArtifactEffect
            {
                Type = e.Type,
                Value = e.BaseValue * multiplier,
                Description = GetEffectDescription(e.Type, e.BaseValue * multiplier)
            }).ToList();

            return new Artifact
            {
                Id = config.Id,
                Name = config.Name,
                Description = config.Description,
                Type = config.Type,
                Rarity = config.Rarity,
                Slot = config.Slot,
                Effects = effects,
                Level = 1,
                EnhancementLevel = 0,
                IsEquipped = false,
                SetId = GetSetForArtifact(config.Id),
                AcquiredTime = DateTime.Now,
                UsageCount = 0
            };
        }

        private static string GetSetForArtifact(string artifactId)
        {
            foreach (var set in _setConfigs)
            {
                if (set.RequiredArtifacts.Contains(artifactId))
                    return set.Id;
            }
            return null;
        }

        private static string GetEffectDescription(ArtifactEffectType type, float value)
        {
            return type switch
            {
                ArtifactEffectType.DamageIncrease => $"伤害 +{value:F0}%",
                ArtifactEffectType.CriticalRate => $"暴击率 +{value:F1}%",
                ArtifactEffectType.CriticalDamage => $"暴击伤害 +{value:F0}%",
                ArtifactEffectType.DefenseIncrease => $"防御力 +{value:F0}%",
                ArtifactEffectType.HealthMax => $"最大生命 +{value:F0}",
                ArtifactEffectType.ManaMax => $"最大法力 +{value:F0}",
                ArtifactEffectType.HealthRegen => $"生命恢复 +{value:F1}/秒",
                ArtifactEffectType.ManaRegen => $"法力恢复 +{value:F1}/秒",
                ArtifactEffectType.MoveSpeed => $"移动速度 +{value:F0}%",
                ArtifactEffectType.AttackSpeed => $"攻击速度 +{value:F0}%",
                ArtifactEffectType.CooldownReduction => $"冷却缩减 +{value:F0}%",
                ArtifactEffectType.LifeSteal => $"生命偷取 +{value:F1}%",
                ArtifactEffectType.ManaSteal => $"法力偷取 +{value:F1}%",
                ArtifactEffectType.DodgeRate => $"闪避率 +{value:F1}%",
                ArtifactEffectType.BlockRate => $"格挡率 +{value:F1}%",
                _ => $"{type} +{value:F1}"
            };
        }

        private class ArtifactConfig
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public ArtifactType Type { get; set; }
            public ArtifactSlot Slot { get; set; }
            public ArtifactRarity Rarity { get; set; }
            public List<ArtifactEffectConfig> BaseEffects { get; set; }
        }

        private class ArtifactEffectConfig
        {
            public ArtifactEffectType Type { get; set; }
            public float BaseValue { get; set; }
        }

        private class ArtifactSetConfig
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public List<string> RequiredArtifacts { get; set; }
            public List<SetBonusConfig> Bonuses { get; set; }
        }

        private class SetBonusConfig
        {
            public int PieceCount { get; set; }
            public List<ArtifactEffectConfig> Effects { get; set; }
        }
    }
}
