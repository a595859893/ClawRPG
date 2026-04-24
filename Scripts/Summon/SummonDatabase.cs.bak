using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 召唤系统数据库
    /// </summary>
    public static class SummonDatabase
    {
        public static readonly Dictionary<string, Summon> Summons = new Dictionary<string, Summon>();
        public static readonly Dictionary<SummonType, string[]> TypeIcons = new Dictionary<SummonType, string[]>();
        public static readonly Dictionary<SummonRarity, string> RarityColors = new Dictionary<SummonRarity, string>();

        static SummonDatabase()
        {
            InitializeRarityColors();
            InitializeSummons();
            InitializeTypeIcons();
        }

        private static void InitializeRarityColors()
        {
            RarityColors[SummonRarity.Common] = "#FFFFFF";
            RarityColors[SummonRarity.Uncommon] = "#1EFF00";
            RarityColors[SummonRarity.Rare] = "#0070DD";
            RarityColors[SummonRarity.Epic] = "#A335EE";
            RarityColors[SummonRarity.Legendary] = "#FF8000";
            RarityColors[SummonRarity.Mythic] = "#FF0000";
        }

        private static void InitializeTypeIcons()
        {
            TypeIcons[SummonType.Elemental] = new string[] { "🔥", "💧", "⚡", "❄️", "🌪️" };
            TypeIcons[SummonType.Spirit] = new string[] { "👻", "🌙", "⭐", "💫" };
            TypeIcons[SummonType.Construct] = new string[] { "⚙️", "🤖", "🗿", "🛡️" };
            TypeIcons[SummonType.Beast] = new string[] { "🐺", "🦁", "🐻", "🦅" };
            TypeIcons[SummonType.Celestial] = new string[] { "☀️", "🌟", "✨", "🌈" };
            TypeIcons[SummonType.Demon] = new string[] { "😈", "🔥", "💀", "🩸" };
            TypeIcons[SummonType.Undead] = new string[] { "💀", "🦴", "🧟", "⚰️" };
            TypeIcons[SummonType.Divine] = new string[] { "👼", "⚜️", "🔱", "💎" };
        }

        private static void InitializeSummons()
        {
            // Elementals - 元素生物
            AddSummon(new Summon
            {
                Id = "fire_elemental",
                Name = "火焰元素",
                Description = "由火焰凝聚而成的元素生物，擅长持续燃烧伤害",
                Type = SummonType.Elemental,
                Rarity = SummonRarity.Common,
                BaseStats = new SummonStats { Health = 80, Attack = 25, Defense = 10, Magic = 20, Speed = 12, CriticalRate = 0.05f, CriticalDamage = 1.5f },
                LevelRequirement = 1,
                ManaCost = 30,
                Duration = 30,
                AttackSpeed = 1.0f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "fire_burst", Name = "火焰爆发", Description = "释放一团火焰", Cooldown = 5, ManaCost = 10, DamageMultiplier = 1.5f, Effect = "burn" }
                }
            });

            AddSummon(new Summon
            {
                Id = "frost_elemental",
                Name = "冰霜元素",
                Description = "由寒冰凝聚而成的元素生物，能够冻结敌人",
                Type = SummonType.Elemental,
                Rarity = SummonRarity.Uncommon,
                BaseStats = new SummonStats { Health = 100, Attack = 20, Defense = 15, Magic = 30, Speed = 10, CriticalRate = 0.08f, CriticalDamage = 1.6f },
                LevelRequirement = 5,
                ManaCost = 40,
                Duration = 35,
                AttackSpeed = 0.9f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "ice_shard", Name = "寒冰箭", Description = "发射冰锥", Cooldown = 4, ManaCost = 12, DamageMultiplier = 1.8f, Effect = "freeze" },
                    new SummonSkill { SkillId = "frost_nova", Name = "冰霜新星", Description = "冰冻周围敌人", Cooldown = 12, ManaCost = 25, DamageMultiplier = 2.0f, Effect = "freeze" }
                }
            });

            AddSummon(new Summon
            {
                Id = "thunder_elemental",
                Name = "雷电元素",
                Description = "由闪电凝聚而成的元素生物，攻击带有麻痹效果",
                Type = SummonType.Elemental,
                Rarity = SummonRarity.Rare,
                BaseStats = new SummonStats { Health = 120, Attack = 35, Defense = 12, Magic = 35, Speed = 15, CriticalRate = 0.10f, CriticalDamage = 1.7f },
                LevelRequirement = 15,
                ManaCost = 55,
                Duration = 40,
                AttackSpeed = 1.2f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "lightning_bolt", Name = "闪电箭", Description = "快速雷电攻击", Cooldown = 3, ManaCost = 15, DamageMultiplier = 1.6f, Effect = "shock" },
                    new SummonSkill { SkillId = "thunder_storm", Name = "雷暴", Description = "大范围雷电", Cooldown = 15, ManaCost = 35, DamageMultiplier = 2.5f, Effect = "shock" }
                }
            });

            AddSummon(new Summon
            {
                Id = "arcane_elemental",
                Name = "奥术元素",
                Description = "精通奥术魔法的强大元素生物",
                Type = SummonType.Elemental,
                Rarity = SummonRarity.Epic,
                BaseStats = new SummonStats { Health = 150, Attack = 25, Defense = 20, Magic = 50, Speed = 12, CriticalRate = 0.12f, CriticalDamage = 1.8f },
                LevelRequirement = 30,
                ManaCost = 80,
                Duration = 50,
                AttackSpeed = 0.8f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "arcane_missile", Name = "奥术飞弹", Description = "发射多枚奥术弹", Cooldown = 4, ManaCost = 20, DamageMultiplier = 2.0f, Effect = "arcane" },
                    new SummonSkill { SkillId = "time_warp", Name = "时间扭曲", Description = "减缓敌人时间", Cooldown = 20, ManaCost = 50, DamageMultiplier = 0.5f, Effect = "slow" }
                }
            });

            // Spirits - 灵魂
            AddSummon(new Summon
            {
                Id = "shadow_spirit",
                Name = "暗影精灵",
                Description = "游荡在阴影中的精灵，能够隐匿身形",
                Type = SummonType.Spirit,
                Rarity = SummonRarity.Uncommon,
                BaseStats = new SummonStats { Health = 70, Attack = 40, Defense = 8, Magic = 15, Speed = 18, CriticalRate = 0.15f, CriticalDamage = 1.8f },
                LevelRequirement = 8,
                ManaCost = 45,
                Duration = 35,
                AttackSpeed = 1.4f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "shadow_strike", Name = "暗影打击", Description = "从阴影中发动攻击", Cooldown = 5, ManaCost = 15, DamageMultiplier = 2.2f, Effect = "poison" }
                }
            });

            AddSummon(new Summon
            {
                Id = "soul_wraith",
                Name = "灵魂幽魂",
                Description = "强大的灵魂生物，能够吸取敌人生命",
                Type = SummonType.Spirit,
                Rarity = SummonRarity.Epic,
                BaseStats = new SummonStats { Health = 130, Attack = 30, Defense = 15, Magic = 45, Speed = 14, CriticalRate = 0.12f, CriticalDamage = 1.9f, LifeSteal = 20 },
                LevelRequirement = 35,
                ManaCost = 90,
                Duration = 55,
                AttackSpeed = 1.0f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "soul_drain", Name = "灵魂汲取", Description = "吸取敌人生命", Cooldown = 8, ManaCost = 25, DamageMultiplier = 1.8f, Effect = "lifesteal" },
                    new SummonSkill { SkillId = "haunt", Name = "附身", Description = "暂时控制敌人", Cooldown = 25, ManaCost = 60, DamageMultiplier = 1.0f, Effect = "control" }
                }
            });

            // Constructs - 构造体
            AddSummon(new Summon
            {
                Id = "iron_golem",
                Name = "铁魔像",
                Description = "由钢铁打造的强大构造体，防御力极高",
                Type = SummonType.Construct,
                Rarity = SummonRarity.Rare,
                BaseStats = new SummonStats { Health = 250, Attack = 30, Defense = 40, Magic = 10, Speed = 6, CriticalRate = 0.03f, CriticalDamage = 1.5f, BlockRate = 0.25f },
                LevelRequirement = 20,
                ManaCost = 70,
                Duration = 45,
                AttackSpeed = 0.6f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "iron_skin", Name = "钢铁皮肤", Description = "提升防御", Cooldown = 15, ManaCost = 20, DamageMultiplier = 0, Effect = "buff_defense" },
                    new SummonSkill { SkillId = "ground_slam", Name = "大地冲击", Description = "重击地面", Cooldown = 10, ManaCost = 30, DamageMultiplier = 2.0f, Effect = "stun" }
                }
            });

            AddSummon(new Summon
            {
                Id = "crystal_construct",
                Name = "水晶构造体",
                Description = "由水晶构成的发光构造体，魔法能力强大",
                Type = SummonType.Construct,
                Rarity = SummonRarity.Legendary,
                BaseStats = new SummonStats { Health = 180, Attack = 20, Defense = 30, Magic = 60, Speed = 10, CriticalRate = 0.10f, CriticalDamage = 2.0f, MagicReflect = 30 },
                LevelRequirement = 45,
                ManaCost = 120,
                Duration = 60,
                AttackSpeed = 0.9f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "crystal_beam", Name = "水晶光束", Description = "聚焦光线", Cooldown = 5, ManaCost = 25, DamageMultiplier = 2.5f, Effect = "crystal" },
                    new SummonSkill { SkillId = "prismatic_barrier", Name = "棱镜屏障", Description = "反射魔法", Cooldown = 20, ManaCost = 40, DamageMultiplier = 0, Effect = "magic_reflect" }
                }
            });

            // Beasts - 野兽
            AddSummon(new Summon
            {
                Id = "wolf_companion",
                Name = "狼伙伴",
                Description = "忠诚的狼伙伴，与主人并肩作战",
                Type = SummonType.Beast,
                Rarity = SummonRarity.Common,
                BaseStats = new SummonStats { Health = 90, Attack = 30, Defense = 12, Magic = 8, Speed = 16, CriticalRate = 0.08f, CriticalDamage = 1.6f },
                LevelRequirement = 3,
                ManaCost = 25,
                Duration = 30,
                AttackSpeed = 1.3f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "pack_attack", Name = "群体攻击", Description = "召唤狼群", Cooldown = 8, ManaCost = 10, DamageMultiplier = 1.8f, Effect = "bleed" }
                }
            });

            AddSummon(new Summon
            {
                Id = "griffon",
                Name = "狮鹫",
                Description = "强大的飞行野兽，能够进行空中攻击",
                Type = SummonType.Beast,
                Rarity = SummonRarity.Epic,
                BaseStats = new SummonStats { Health = 160, Attack = 45, Defense = 20, Magic = 25, Speed = 20, CriticalRate = 0.12f, CriticalDamage = 1.9f },
                LevelRequirement = 32,
                ManaCost = 85,
                Duration = 50,
                AttackSpeed = 1.5f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "swoop", Name = "俯冲攻击", Description = "从天而降", Cooldown = 6, ManaCost = 20, DamageMultiplier = 2.3f, Effect = "bleed" },
                    new SummonSkill { SkillId = "screech", Name = "尖啸", Description = "恐惧敌人", Cooldown = 12, ManaCost = 25, DamageMultiplier = 1.2f, Effect = "fear" }
                }
            });

            // Celestial - 天界生物
            AddSummon(new Summon
            {
                Id = "light_angel",
                Name = "光明天使",
                Description = "来自天界的治愈型生物",
                Type = SummonType.Celestial,
                Rarity = SummonRarity.Legendary,
                BaseStats = new SummonStats { Health = 140, Attack = 25, Defense = 25, Magic = 55, Speed = 12, CriticalRate = 0.10f, CriticalDamage = 1.7f },
                LevelRequirement = 40,
                ManaCost = 110,
                Duration = 55,
                AttackSpeed = 0.8f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "healing_light", Name = "治愈之光", Description = "治疗主人", Cooldown = 8, ManaCost = 30, DamageMultiplier = -1.5f, Effect = "heal" },
                    new SummonSkill { SkillId = "divine_shield", Name = "神圣护盾", Description = "施加保护", Cooldown = 18, ManaCost = 40, DamageMultiplier = 0, Effect = "shield" }
                }
            });

            AddSummon(new Summon
            {
                Id = "phoenix",
                Name = "凤凰",
                Description = "永恒的神鸟，能够浴火重生",
                Type = SummonType.Celestial,
                Rarity = SummonRarity.Mythic,
                BaseStats = new SummonStats { Health = 200, Attack = 50, Defense = 30, Magic = 60, Speed = 16, CriticalRate = 0.15f, CriticalDamage = 2.2f, LifeSteal = 15 },
                LevelRequirement = 60,
                ManaCost = 180,
                Duration = 90,
                AttackSpeed = 1.2f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "inferno", Name = "地狱火", Description = "召唤烈火", Cooldown = 10, ManaCost = 50, DamageMultiplier = 3.0f, Effect = "burn" },
                    new SummonSkill { SkillId = "rebirth", Name = "重生", Description = "死亡后复活", Cooldown = 60, ManaCost = 100, DamageMultiplier = 0, Effect = "reborn" },
                    new SummonSkill { SkillId = "blazing_feathers", Name = "烈羽", Description = " fiery feathers", Cooldown = 5, ManaCost = 25, DamageMultiplier = 2.0f, Effect = "burn" }
                }
            });

            // Demon - 恶魔
            AddSummon(new Summon
            {
                Id = "imp",
                Name = "小恶魔",
                Description = "调皮的小恶魔擅长火焰魔法",
                Type = SummonType.Demon,
                Rarity = SummonRarity.Rare,
                BaseStats = new SummonStats { Health = 100, Attack = 35, Defense = 10, Magic = 40, Speed = 17, CriticalRate = 0.12f, CriticalDamage = 1.8f },
                LevelRequirement = 18,
                ManaCost = 60,
                Duration = 40,
                AttackSpeed = 1.4f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "fire_ball", Name = "火球", Description = "发射火球", Cooldown = 4, ManaCost = 15, DamageMultiplier = 2.0f, Effect = "burn" },
                    new SummonSkill { SkillId = "cursed_flame", Name = "诅咒之火", Description = "持续伤害", Cooldown = 10, ManaCost = 25, DamageMultiplier = 1.5f, Effect = "curse" }
                }
            });

            AddSummon(new Summon
            {
                Id = "demon_lord",
                Name = "恶魔领主",
                Description = "强大的恶魔领袖，掌控黑暗力量",
                Type = SummonType.Demon,
                Rarity = SummonRarity.Mythic,
                BaseStats = new SummonStats { Health = 280, Attack = 60, Defense = 35, Magic = 70, Speed = 14, CriticalRate = 0.18f, CriticalDamage = 2.5f, LifeSteal = 25 },
                LevelRequirement = 65,
                ManaCost = 200,
                Duration = 100,
                AttackSpeed = 1.1f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "dark_void", Name = "黑暗虚空", Description = "创造虚无", Cooldown = 15, ManaCost = 70, DamageMultiplier = 3.5f, Effect = "void" },
                    new SummonSkill { SkillId = "soul_rip", Name = "灵魂撕裂", Description = "粉碎灵魂", Cooldown = 8, ManaCost = 45, DamageMultiplier = 2.8f, Effect = "soul_damage" },
                    new SummonSkill { SkillId = "infernal_domain", Name = "地狱领域", Description = "领域效果", Cooldown = 30, ManaCost = 100, DamageMultiplier = 2.0f, Effect = "aoe" }
                }
            });

            // Undead - 不死族
            AddSummon(new Summon
            {
                Id = "skeleton_warrior",
                Name = "骷髅战士",
                Description = "不死的战士，永不疲倦",
                Type = SummonType.Undead,
                Rarity = SummonRarity.Common,
                BaseStats = new SummonStats { Health = 85, Attack = 28, Defense = 15, Magic = 5, Speed = 11, CriticalRate = 0.05f, CriticalDamage = 1.5f },
                LevelRequirement = 2,
                ManaCost = 20,
                Duration = 25,
                AttackSpeed = 1.0f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "bone_shatter", Name = "碎骨", Description = "粉碎攻击", Cooldown = 6, ManaCost = 8, DamageMultiplier = 1.6f, Effect = "bleed" }
                }
            });

            AddSummon(new Summon
            {
                Id = "lich",
                Name = "巫妖",
                Description = "强大的不死法师，精通黑暗魔法",
                Type = SummonType.Undead,
                Rarity = SummonRarity.Legendary,
                BaseStats = new SummonStats { Health = 150, Attack = 20, Defense = 20, Magic = 65, Speed = 10, CriticalRate = 0.12f, CriticalDamage = 1.9f, LifeSteal = 30 },
                LevelRequirement = 50,
                ManaCost = 140,
                Duration = 70,
                AttackSpeed = 0.7f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "death_ray", Name = "死亡射线", Description = "致命光线", Cooldown = 6, ManaCost = 35, DamageMultiplier = 2.8f, Effect = "death" },
                    new SummonSkill { SkillId = "army_of_dead", Name = "亡者大军", Description = "召唤骷髅", Cooldown = 25, ManaCost = 80, DamageMultiplier = 1.5f, Effect = "summon_skeletons" },
                    new SummonSkill { SkillId = "life_drain", Name = "生命虹吸", Description = "吸取生命", Cooldown = 10, ManaCost = 40, DamageMultiplier = 2.0f, Effect = "lifesteal" }
                }
            });

            // Divine - 神性生物
            AddSummon(new Summon
            {
                Id = "sacred_guardian",
                Name = "神圣守卫",
                Description = "受神祝福的守护者",
                Type = SummonType.Divine,
                Rarity = SummonRarity.Legendary,
                BaseStats = new SummonStats { Health = 220, Attack = 40, Defense = 45, Magic = 40, Speed = 8, CriticalRate = 0.08f, CriticalDamage = 1.8f, BlockRate = 0.30f },
                LevelRequirement = 48,
                ManaCost = 130,
                Duration = 65,
                AttackSpeed = 0.7f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "holy_strike", Name = "圣光打击", Description = "神圣攻击", Cooldown = 5, ManaCost = 30, DamageMultiplier = 2.2f, Effect = "holy" },
                    new SummonSkill { SkillId = "divine_intervention", Name = "神圣干预", Description = "保护主人", Cooldown = 25, ManaCost = 60, DamageMultiplier = 0, Effect = "invincible" }
                }
            });

            AddSummon(new Summon
            {
                Id = "god_of_war",
                Name = "战争之神",
                Description = "全能的战神化身",
                Type = SummonType.Divine,
                Rarity = SummonRarity.Mythic,
                BaseStats = new SummonStats { Health = 300, Attack = 70, Defense = 50, Magic = 55, Speed = 15, CriticalRate = 0.20f, CriticalDamage = 2.8f },
                LevelRequirement = 70,
                ManaCost = 250,
                Duration = 120,
                AttackSpeed = 1.3f,
                Skills = new List<SummonSkill>
                {
                    new SummonSkill { SkillId = "wrath_of_gods", Name = "诸神愤怒", Description = "毁灭性打击", Cooldown = 12, ManaCost = 80, DamageMultiplier = 4.0f, Effect = "divine" },
                    new SummonSkill { SkillId = "battle_cry", Name = "战斗呐喊", Description = "增强友军", Cooldown = 20, ManaCost = 50, DamageMultiplier = 0, Effect = "buff_attack" },
                    new SummonSkill { SkillId = "divine_judgment", Name = "神圣审判", Description = "审判敌人", Cooldown = 45, ManaCost = 120, DamageMultiplier = 3.5f, Effect = "judgment" }
                }
            });
        }

        private static void AddSummon(Summon summon)
        {
            if (summon.Icon == null)
            {
                summon.Icon = GetDefaultIcon(summon.Type, summon.Rarity);
            }
            Summons[summon.Id] = summon;
        }

        private static string GetDefaultIcon(SummonType type, SummonRarity rarity)
        {
            var icons = TypeIcons.ContainsKey(type) ? TypeIcons[type] : new string[] { "❓" };
            var index = (int)rarity % icons.Length;
            return icons[index];
        }

        public static Summon GetSummon(string summonId)
        {
            return Summons.ContainsKey(summonId) ? Summons[summonId] : null;
        }

        public static List<Summon> GetSummonsByType(SummonType type)
        {
            var result = new List<Summon>();
            foreach (var summon in Summons.Values)
            {
                if (summon.Type == type)
                    result.Add(summon);
            }
            return result;
        }

        public static List<Summon> GetSummonsByRarity(SummonRarity rarity)
        {
            var result = new List<Summon>();
            foreach (var summon in Summons.Values)
            {
                if (summon.Rarity == rarity)
                    result.Add(summon);
            }
            return result;
        }

        public static List<Summon> GetAvailableSummons(int playerLevel)
        {
            var result = new List<Summon>();
            foreach (var summon in Summons.Values)
            {
                if (summon.LevelRequirement <= playerLevel)
                    result.Add(summon);
            }
            return result;
        }

        public static int GetRarityLevel(SummonRarity rarity)
        {
            return (int)rarity;
        }

        public static float GetRarityStatMultiplier(SummonRarity rarity)
        {
            return 1.0f + ((int)rarity * 0.2f);
        }
    }
}
