// ============================================
// Relic Database - 遗物数据库
// ============================================

using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Relics
{
    public static class RelicCollectionDatabase
    {
        // 遗物配置
        public static readonly Dictionary<string, Relic> Relics = new Dictionary<string, Relic>
        {
            // === 普通遗物 ===
            ["relic_broken_sword"] = new Relic
            {
                Id = "relic_broken_sword",
                Name = "折断的剑",
                Description = "曾经英雄的剑，虽然已折断但仍蕴含力量",
                Type = RelicType.Weapon,
                Rarity = RelicRarity.Common,
                PrimaryEffect = RelicEffectType.DamageIncrease,
                PrimaryEffectValue = 0.05,
                Level = 1
            },
            ["relic_old_amulet"] = new Relic
            {
                Id = "relic_old_amulet",
                Name = "古老的护符",
                Description = "年代久远的护符，散发出微弱的光芒",
                Type = RelicType.Accessory,
                Rarity = RelicRarity.Common,
                PrimaryEffect = RelicEffectType.HealthMax,
                PrimaryEffectValue = 0.10,
                Level = 1
            },
            ["relic_torn_scroll"] = new Relic
            {
                Id = "relic_torn_scroll",
                Name = "破损的卷轴",
                Description = "记载着神秘知识的卷轴碎片",
                Type = RelicType.Passive,
                Rarity = RelicRarity.Common,
                PrimaryEffect = RelicEffectType.ExperienceGain,
                PrimaryEffectValue = 0.05,
                Level = 1
            },
            ["relic_coin_pouch"] = new Relic
            {
                Id = "relic_coin_pouch",
                Name = "钱袋",
                Description = "鼓鼓的钱袋，似乎永远花不完",
                Type = RelicType.Accessory,
                Rarity = RelicRarity.Common,
                PrimaryEffect = RelicEffectType.GoldGain,
                PrimaryEffectValue = 0.10,
                Level = 1
            },

            // === 优秀遗物 ===
            ["relic_shadow_dagger"] = new Relic
            {
                Id = "relic_shadow_dagger",
                Name = "暗影匕首",
                Description = "在阴影中隐藏的致命武器",
                Type = RelicType.Weapon,
                Rarity = RelicRarity.Uncommon,
                PrimaryEffect = RelicEffectType.CriticalRate,
                PrimaryEffectValue = 0.05,
                SecondaryEffect = RelicEffectType.DamageIncrease,
                SecondaryEffectValue = 0.08,
                Level = 1
            },
            ["relic_iron_shield"] = new Relic
            {
                Id = "relic_iron_shield",
                Name = "铁壁之盾",
                Description = "坚固的盾牌，能抵御一切攻击",
                Type = RelicType.Armor,
                Rarity = RelicRarity.Uncommon,
                PrimaryEffect = RelicEffectType.DamageReduction,
                PrimaryEffectValue = 0.10,
                Level = 1
            },
            ["relic_mana_crystal"] = new Relic
            {
                Id = "relic_mana_crystal",
                Name = "法力水晶",
                Description = "充盈着魔力的水晶",
                Type = RelicType.Accessory,
                Rarity = RelicRarity.Uncommon,
                PrimaryEffect = RelicEffectType.ManaMax,
                PrimaryEffectValue = 0.20,
                SecondaryEffect = RelicEffectType.ManaRegen,
                SecondaryEffectValue = 0.10,
                Level = 1
            },
            ["relic_vampire_ring"] = new Relic
            {
                Id = "relic_vampire_ring",
                Name = "吸血鬼戒指",
                Description = "蕴含黑暗力量的戒指",
                Type = RelicType.Accessory,
                Rarity = RelicRarity.Uncommon,
                PrimaryEffect = RelicEffectType.LifeSteal,
                PrimaryEffectValue = 0.05,
                Level = 1
            },

            // === 稀有遗物 ===
            ["relic_phoenix_feather"] = new Relic
            {
                Id = "relic_phoenix_feather",
                Name = "凤凰羽毛",
                Description = "蕴含重生之力的羽毛",
                Type = RelicType.Trigger,
                Rarity = RelicRarity.Rare,
                PrimaryEffect = RelicEffectType.HealthRegen,
                PrimaryEffectValue = 0.15,
                SecondaryEffect = RelicEffectType.HealthMax,
                SecondaryEffectValue = 0.15,
                Level = 1
            },
            ["relic_dragon_scale"] = new Relic
            {
                Id = "relic_dragon_scale",
                Name = "龙鳞",
                Description = "巨龙身上的鳞片，坚不可摧",
                Type = RelicType.Armor,
                Rarity = RelicRarity.Rare,
                PrimaryEffect = RelicEffectType.DamageReduction,
                PrimaryEffectValue = 0.15,
                SecondaryEffect = RelicEffectType.ElementalResist,
                SecondaryEffectValue = 0.10,
                Level = 1
            },
            ["relic_thunder_orb"] = new Relic
            {
                Id = "relic_thunder_orb",
                Name = "雷鸣宝珠",
                Description = "蕴含雷电之力的宝珠",
                Type = RelicType.Trigger,
                Rarity = RelicRarity.Rare,
                PrimaryEffect = RelicEffectType.ElementalDamage,
                PrimaryEffectValue = 0.20,
                SecondaryEffect = RelicEffectType.CriticalDamage,
                SecondaryEffectValue = 0.15,
                Level = 1
            },
            ["relic_speed_boots"] = new Relic
            {
                Id = "relic_speed_boots",
                Name = "疾风靴",
                Description = "穿上它能健步如飞",
                Type = RelicType.Armor,
                Rarity = RelicRarity.Rare,
                PrimaryEffect = RelicEffectType.MoveSpeed,
                PrimaryEffectValue = 0.15,
                SecondaryEffect = RelicEffectType.AttackSpeed,
                SecondaryEffectValue = 0.10,
                Level = 1
            },

            // === 史诗遗物 ===
            ["relic_void_scepter"] = new Relic
            {
                Id = "relic_void_scepter",
                Name = "虚空权杖",
                Description = "掌握虚空之力的权杖",
                Type = RelicType.Weapon,
                Rarity = RelicRarity.Epic,
                PrimaryEffect = RelicEffectType.DamageIncrease,
                PrimaryEffectValue = 0.25,
                SecondaryEffect = RelicEffectType.CriticalRate,
                SecondaryEffectValue = 0.10,
                Level = 1
            },
            ["relic_divine_aura"] = new Relic
            {
                Id = "relic_divine_aura",
                Name = "神圣光环",
                Description = "神圣之力形成的保护光环",
                Type = RelicType.Passive,
                Rarity = RelicRarity.Epic,
                PrimaryEffect = RelicEffectType.DamageReduction,
                PrimaryEffectValue = 0.20,
                SecondaryEffect = RelicEffectType.HealthRegen,
                SecondaryEffectValue = 0.20,
                Level = 1
            },
            ["relic_frost_crown"] = new Relic
            {
                Id = "relic_frost_crown",
                Name = "冰霜王冠",
                Description = "寒冷之力的极致体现",
                Type = RelicType.Accessory,
                Rarity = RelicRarity.Epic,
                PrimaryEffect = RelicEffectType.ElementalDamage,
                PrimaryEffectValue = 0.30,
                SecondaryEffect = RelicEffectType.CooldownReduction,
                SecondaryEffectValue = 0.10,
                Level = 1
            },
            ["relic_blood_lust"] = new Relic
            {
                Id = "relic_blood_lust",
                Name = "血之饥渴",
                Description = "渴求鲜血的诅咒之物",
                Type = RelicType.Trigger,
                Rarity = RelicRarity.Epic,
                PrimaryEffect = RelicEffectType.LifeSteal,
                PrimaryEffectValue = 0.15,
                SecondaryEffect = RelicEffectType.AttackSpeed,
                SecondaryEffectValue = 0.20,
                Level = 1
            },

            // === 传说遗物 ===
            ["relic_chaos_gauntlet"] = new Relic
            {
                Id = "relic_chaos_gauntlet",
                Name = "混沌护手",
                Description = "混沌之力凝聚而成的神器",
                Type = RelicType.Weapon,
                Rarity = RelicRarity.Legendary,
                PrimaryEffect = RelicEffectType.DamageIncrease,
                PrimaryEffectValue = 0.35,
                SecondaryEffect = RelicEffectType.CriticalDamage,
                SecondaryEffectValue = 0.30,
                Level = 1
            },
            ["relic_eternal_shield"] = new Relic
            {
                Id = "relic_eternal_shield",
                Name = "永恒之盾",
                Description = "永远不会被击破的盾牌",
                Type = RelicType.Armor,
                Rarity = RelicRarity.Legendary,
                PrimaryEffect = RelicEffectType.DamageReduction,
                PrimaryEffectValue = 0.30,
                SecondaryEffect = RelicEffectType.HealthMax,
                SecondaryEffectValue = 0.30,
                Level = 1
            },
            ["relic_time_watch"] = new Relic
            {
                Id = "relic_time_watch",
                Name = "时间手表",
                Description = "掌控时间流逝的神器",
                Type = RelicType.Accessory,
                Rarity = RelicRarity.Legendary,
                PrimaryEffect = RelicEffectType.CooldownReduction,
                PrimaryEffectValue = 0.20,
                SecondaryEffect = RelicEffectType.AttackSpeed,
                SecondaryEffectValue = 0.25,
                Level = 1
            },
            ["relic_lucky_clover"] = new Relic
            {
                Id = "relic_lucky_clover",
                Name = "幸运四叶草",
                Description = "能带来极致幸运的神器",
                Type = RelicType.Passive,
                Rarity = RelicRarity.Legendary,
                PrimaryEffect = RelicEffectType.DropRate,
                PrimaryEffectValue = 0.30,
                SecondaryEffect = RelicEffectType.GoldGain,
                SecondaryEffectValue = 0.40,
                Level = 1
            },

            // === 神器遗物 ===
            ["relic_world_tree"] = new Relic
            {
                Id = "relic_world_tree",
                Name = "世界之种",
                Description = "蕴含世界本源的神器",
                Type = RelicType.Set,
                Rarity = RelicRarity.Mythic,
                PrimaryEffect = RelicEffectType.HealthMax,
                PrimaryEffectValue = 0.50,
                SecondaryEffect = RelicEffectType.ManaMax,
                SecondaryEffectValue = 0.50,
                SetId = "set_world",
                Level = 1
            },
            ["relic_ancient_god_sword"] = new Relic
            {
                Id = "relic_ancient_god_sword",
                Name = "古神之剑",
                Description = "由古神打造的神剑",
                Type = RelicType.Weapon,
                Rarity = RelicRarity.Mythic,
                PrimaryEffect = RelicEffectType.DamageIncrease,
                PrimaryEffectValue = 0.50,
                SecondaryEffect = RelicEffectType.CriticalRate,
                SecondaryEffectValue = 0.20,
                Level = 1
            },
            ["relic_immortal_crown"] = new Relic
            {
                Id = "relic_immortal_crown",
                Name = "不朽王冠",
                Description = "戴上它将获得永生",
                Type = RelicType.Accessory,
                Rarity = RelicRarity.Mythic,
                PrimaryEffect = RelicEffectType.HealthRegen,
                PrimaryEffectValue = 0.50,
                SecondaryEffect = RelicEffectType.LifeSteal,
                SecondaryEffectValue = 0.25,
                Level = 1
            }
        };

        // 遗物套装配置
        public static readonly Dictionary<string, RelicSet> RelicSets = new Dictionary<string, RelicSet>
        {
            ["set_world"] = new RelicSet
            {
                Id = "set_world",
                Name = "世界套装",
                Description = "世界之力套装",
                RequiredCount = 3,
                SetEffect = RelicEffectType.DamageReduction,
                SetEffectValue = 0.25
            },
            ["set_dragon"] = new RelicSet
            {
                Id = "set_dragon",
                Name = "巨龙套装",
                Description = "龙之力套装",
                RequiredCount = 3,
                SetEffect = RelicEffectType.ElementalDamage,
                SetEffectValue = 0.35
            },
            ["set_shadow"] = new RelicSet
            {
                Id = "set_shadow",
                Name = "暗影套装",
                Description = "暗影之力套装",
                RequiredCount = 3,
                SetEffect = RelicEffectType.CriticalDamage,
                SetEffectValue = 0.40
            },
            ["set_divine"] = new RelicSet
            {
                Id = "set_divine",
                Name = "神圣套装",
                Description = "神圣之力套装",
                RequiredCount = 3,
                SetEffect = RelicEffectType.HealthRegen,
                SetEffectValue = 0.40
            },
            ["set_chaos"] = new RelicSet
            {
                Id = "set_chaos",
                Name = "混沌套装",
                Description = "混沌之力套装",
                RequiredCount = 3,
                SetEffect = RelicEffectType.DamageIncrease,
                SetEffectValue = 0.35
            }
        };

        // 遗物生成配置
        public static readonly RelicGenerationConfig GenerationConfig = new RelicGenerationConfig
        {
            MinRelicsPerFloor = 1,
            MaxRelicsPerFloor = 3,
            CommonChance = 0.40,
            UncommonChance = 0.30,
            RareChance = 0.18,
            EpicChance = 0.08,
            LegendaryChance = 0.03,
            MythicChance = 0.01
        };

        // 获取遗物稀有度颜色
        public static string GetRarityColor(RelicRarity rarity)
        {
            return rarity switch
            {
                RelicRarity.Common => "#FFFFFF",
                RelicRarity.Uncommon => "#1EFF00",
                RelicRarity.Rare => "#0070FF",
                RelicRarity.Epic => "#A335EE",
                RelicRarity.Legendary => "#FF8000",
                RelicRarity.Mythic => "#FF0000",
                _ => "#FFFFFF"
            };
        }

        // 获取随机遗物
        public static Relic GetRandomRelic()
        {
            var random = new Random();
            var roll = random.NextDouble();
            
            RelicRarity rarity;
            if (roll < GenerationConfig.MythicChance)
                rarity = RelicRarity.Mythic;
            else if (roll < GenerationConfig.MythicChance + GenerationConfig.LegendaryChance)
                rarity = RelicRarity.Legendary;
            else if (roll < GenerationConfig.MythicChance + GenerationConfig.LegendaryChance + GenerationConfig.EpicChance)
                rarity = RelicRarity.Epic;
            else if (roll < GenerationConfig.MythicChance + GenerationConfig.LegendaryChance + GenerationConfig.EpicChance + GenerationConfig.RareChance)
                rarity = RelicRarity.Rare;
            else if (roll < GenerationConfig.MythicChance + GenerationConfig.LegendaryChance + GenerationConfig.EpicChance + GenerationConfig.RareChance + GenerationConfig.UncommonChance)
                rarity = RelicRarity.Uncommon;
            else
                rarity = RelicRarity.Common;

            // 获取该稀有度的遗物
            var relicsOfRarity = new List<Relic>();
            foreach (var relic in Relics.Values)
            {
                if (relic.Rarity == rarity)
                    relicsOfRarity.Add(relic);
            }

            if (relicsOfRarity.Count > 0)
                return relicsOfRarity[random.Next(relicsOfRarity.Count)];
            
            return null;
        }
    }
}
