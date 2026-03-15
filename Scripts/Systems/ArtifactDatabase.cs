using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// 神器数据库 - 配置神器数据
    /// </summary>
    public static class ArtifactDatabase
    {
        private static Dictionary<string, Artifact> artifacts = new Dictionary<string, Artifact>();
        private static Dictionary<string, List<Artifact>> artifactSets = new Dictionary<string, List<Artifact>>();
        private static bool initialized = false;

        public static void Initialize()
        {
            if (initialized) return;
            initialized = true;

            // Create weapon artifacts
            CreateWeaponArtifacts();
            
            // Create armor artifacts
            CreateArmorArtifacts();
            
            // Create accessory artifacts
            CreateAccessoryArtifacts();
            
            // Create relic artifacts
            CreateRelicArtifacts();
            
            // Build sets
            BuildArtifactSets();
        }

        private static void CreateWeaponArtifacts()
        {
            // Legendary Weapons
            artifacts["artifact_sword_001"] = new Artifact
            {
                Id = "artifact_sword_001",
                Name = "断钢剑",
                Description = "传说中的圣剑，能斩断一切邪恶",
                Type = ArtifactType.Weapon,
                Rarity = ArtifactRarity.Legendary,
                SetId = "set_holy",
                Lore = "由天使长米迦勒锻造的神剑，曾用于封印远古恶魔",
                Origin = "天使之城",
                DropRate = 0.001f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "attack", Value = 150, Description = "攻击+150" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "holy_damage", Value = 50, Description = "神圣伤害+50" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "crit_rate", Value = 10, Description = "暴击率+10%" }
                }
            };

            artifacts["artifact_sword_002"] = new Artifact
            {
                Id = "artifact_sword_002",
                Name = "暗影切割者",
                Description = "吸收月光的黑暗之剑",
                Type = ArtifactType.Weapon,
                Rarity = ArtifactRarity.Epic,
                SetId = "set_shadow",
                Lore = "在月圆之夜由暗影精灵锻造",
                Origin = "暗影森林",
                DropRate = 0.005f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "attack", Value = 100, Description = "攻击+100" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "dark_damage", Value = 40, Description = "暗影伤害+40" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.Utility, StatName = "stealth", Value = 20, Description = "隐身能力+20%" }
                }
            };

            artifacts["artifact_staff_001"] = new Artifact
            {
                Id = "artifact_staff_001",
                Name = "星界法杖",
                Description = "蕴含宇宙星辰力量的法杖",
                Type = ArtifactType.Weapon,
                Rarity = ArtifactRarity.Legendary,
                SetId = "set_cosmic",
                Lore = "由星灵法师从星辰深处带回",
                Origin = "星界",
                DropRate = 0.001f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.SkillBoost, StatName = "magic", Value = 200, Description = "魔法+200" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "mp_regen", Value = 5, Description = "MP恢复+5/秒" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.Special, StatName = "spell_amplify", Value = 15, Description = "法术强化+15%" }
                }
            };

            artifacts["artifact_bow_001"] = new Artifact
            {
                Id = "artifact_bow_001",
                Name = "风神弓",
                Description = "能射出追风之箭的神弓",
                Type = ArtifactType.Weapon,
                Rarity = ArtifactRarity.Epic,
                SetId = "set_wind",
                Lore = "风之精灵王的武器",
                Origin = "风语峡谷",
                DropRate = 0.005f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "attack", Value = 90, Description = "攻击+90" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "attack_speed", Value = 25, Description = "攻击速度+25%" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.Utility, StatName = "range", Value = 30, Description = "射程+30%" }
                }
            };

            // Rare weapons
            artifacts["artifact_axe_001"] = new Artifact
            {
                Id = "artifact_axe_001",
                Name = "碎颅者",
                Description = "战斧碎甲，无人能挡",
                Type = ArtifactType.Weapon,
                Rarity = ArtifactRarity.Rare,
                Lore = "北方蛮族的传奇战斧",
                Origin = "冰霜高原",
                DropRate = 0.01f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "attack", Value = 60, Description = "攻击+60" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "armor_pierce", Value = 15, Description = "护甲穿透+15%" }
                }
            };
        }

        private static void CreateArmorArtifacts()
        {
            artifacts["artifact_armor_001"] = new Artifact
            {
                Id = "artifact_armor_001",
                Name = "龙鳞铠甲",
                Description = "由真龙鳞片打造的超级铠甲",
                Type = ArtifactType.Armor,
                Rarity = ArtifactRarity.Legendary,
                SetId = "set_dragon",
                Lore = "屠龙者用龙鳞制成的传奇铠甲",
                Origin = "龙之巢穴",
                DropRate = 0.001f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "defense", Value = 200, Description = "防御+200" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "health", Value = 500, Description = "生命+500" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "fire_resist", Value = 50, Description = "火抗+50%" }
                }
            };

            artifacts["artifact_armor_002"] = new Artifact
            {
                Id = "artifact_armor_002",
                Name = "幽影斗篷",
                Description = "穿上它能隐入阴影",
                Type = ArtifactType.Armor,
                Rarity = ArtifactRarity.Epic,
                SetId = "set_shadow",
                Lore = "暗影刺客的专属装备",
                Origin = "暗影森林",
                DropRate = 0.005f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "defense", Value = 80, Description = "防御+80" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.Utility, StatName = "stealth", Value = 40, Description = "隐身能力+40%" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "dodge", Value = 15, Description = "闪避+15%" }
                }
            };

            artifacts["artifact_armor_003"] = new Artifact
            {
                Id = "artifact_armor_003",
                Name = "秘银锁甲",
                Description = "轻盈而坚固的秘银铠甲",
                Type = ArtifactType.Armor,
                Rarity = ArtifactRarity.Rare,
                Lore = "矮人王国的工艺杰作",
                Origin = "铁岩城",
                DropRate = 0.01f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "defense", Value = 50, Description = "防御+50" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "speed", Value = 10, Description = "速度+10%" }
                }
            };

            artifacts["artifact_shield_001"] = new Artifact
            {
                Id = "artifact_shield_001",
                Name = "光明壁垒",
                Description = "圣光形成的守护屏障",
                Type = ArtifactType.Armor,
                Rarity = ArtifactRarity.Epic,
                SetId = "set_holy",
                Lore = "光明教堂的圣物",
                Origin = "圣城",
                DropRate = 0.005f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "defense", Value = 100, Description = "防御+100" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "block", Value = 20, Description = "格挡+20%" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "holy_resist", Value = 30, Description = "神圣抗性+30%" }
                }
            };
        }

        private static void CreateAccessoryArtifacts()
        {
            artifacts["artifact_amulet_001"] = new Artifact
            {
                Id = "artifact_amulet_001",
                Name = "时间之证",
                Description = "掌控时间的魔法护符",
                Type = ArtifactType.Accessory,
                Rarity = ArtifactRarity.Mythical,
                SetId = "set_cosmic",
                Lore = "时间龙的遗物，能短暂回溯时间",
                Origin = "时间裂隙",
                DropRate = 0.0001f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.Special, StatName = "time_slow", Value = 30, Description = "时间减缓+30%" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.Special, StatName = "cd_reduction", Value = 20, Description = "冷却减免+20%" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "all_stats", Value = 25, Description = "全属性+25%" }
                }
            };

            artifacts["artifact_amulet_002"] = new Artifact
            {
                Id = "artifact_amulet_002",
                Name = "生命之心",
                Description = "蕴含生命能量的宝石",
                Type = ArtifactType.Accessory,
                Rarity = ArtifactRarity.Legendary,
                SetId = "set_nature",
                Lore = "世界树的精华结晶",
                Origin = "精灵森林",
                DropRate = 0.001f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "health", Value = 800, Description = "生命+800" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "lifesteal", Value = 15, Description = "生命偷取+15%" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "hp_regen", Value = 10, Description = "生命恢复+10/秒" }
                }
            };

            artifacts["artifact_ring_001"] = new Artifact
            {
                Id = "artifact_ring_001",
                Name = "力量之戒",
                Description = "巨人族的力量源泉",
                Type = ArtifactType.Accessory,
                Rarity = ArtifactRarity.Epic,
                Lore = "泰坦族的至宝",
                Origin = "泰坦遗迹",
                DropRate = 0.005f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "attack", Value = 80, Description = "攻击+80" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "crit_damage", Value = 30, Description = "暴击伤害+30%" }
                }
            };

            artifacts["artifact_ring_002"] = new Artifact
            {
                Id = "artifact_ring_002",
                Name = "智慧之环",
                Description = "提升智慧与魔法能力",
                Type = ArtifactType.Accessory,
                Rarity = ArtifactRarity.Epic,
                Lore = "魔法师的终极追求",
                Origin = "法师塔",
                DropRate = 0.005f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "magic", Value = 100, Description = "魔法+100" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.SkillBoost, StatName = "mp_max", Value = 200, Description = "最大MP+200" }
                }
            };

            artifacts["artifact_charm_001"] = new Artifact
            {
                Id = "artifact_charm_001",
                Name = "幸运护符",
                Description = "增加幸运值的魔法护符",
                Type = ArtifactType.Accessory,
                Rarity = ArtifactRarity.Rare,
                Lore = "幸运仙女的祝福",
                Origin = "云端",
                DropRate = 0.01f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.EconomicBonus, StatName = "luck", Value = 30, Description = "幸运+30" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.EconomicBonus, StatName = "drop_rate", Value = 15, Description = "掉落率+15%" }
                }
            };
        }

        private static void CreateRelicArtifacts()
        {
            artifacts["artifact_relic_001"] = new Artifact
            {
                Id = "artifact_relic_001",
                Name = "恶魔之心",
                Description = "蕴含强大恶魔能量的邪物",
                Type = ArtifactType.Relic,
                Rarity = ArtifactRarity.Mythical,
                SetId = "set_demon",
                Lore = "被封印的恶魔王者之心",
                Origin = "深渊",
                DropRate = 0.0001f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "attack", Value = 200, Description = "攻击+200" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "health", Value = 600, Description = "生命+600" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "dark_damage", Value = 60, Description = "暗影伤害+60" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.Special, StatName = "life_drain", Value = 10, Description = "生命汲取+10%" }
                }
            };

            artifacts["artifact_relic_002"] = new Artifact
            {
                Id = "artifact_relic_002",
                Name = "天使之羽",
                Description = "天使的羽毛，拥有神圣之力",
                Type = ArtifactType.Relic,
                Rarity = ArtifactRarity.Legendary,
                SetId = "set_holy",
                Lore = "天使遗落的羽毛",
                Origin = "天堂",
                DropRate = 0.001f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "defense", Value = 120, Description = "防御+120" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "holy_damage", Value = 40, Description = "神圣伤害+40" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "heal_boost", Value = 25, Description = "治疗效果+25%" }
                }
            };

            artifacts["artifact_relic_003"] = new Artifact
            {
                Id = "artifact_relic_003",
                Name = "龙之结晶",
                Description = "巨龙的生命精华",
                Type = ArtifactType.Relic,
                Rarity = ArtifactRarity.Epic,
                SetId = "set_dragon",
                Lore = "龙族的力量源泉",
                Origin = "龙之巢穴",
                DropRate = 0.005f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "attack", Value = 60, Description = "攻击+60" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "defense", Value = 60, Description = "防御+60" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "fire_damage", Value = 30, Description = "火焰伤害+30" }
                }
            };

            artifacts["artifact_relic_004"] = new Artifact
            {
                Id = "artifact_relic_004",
                Name = "凤凰之羽",
                Description = "浴火重生的神鸟羽毛",
                Type = ArtifactType.Relic,
                Rarity = ArtifactRarity.Epic,
                SetId = "set_phoenix",
                Lore = "凤凰每次死亡都会留下羽毛",
                Origin = "火焰山",
                DropRate = 0.005f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.CombatBonus, StatName = "fire_damage", Value = 35, Description = "火焰伤害+35" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.Special, StatName = "revive_chance", Value = 10, Description = "复活几率+10%" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "hp_regen", Value = 5, Description = "生命恢复+5/秒" }
                }
            };

            artifacts["artifact_relic_005"] = new Artifact
            {
                Id = "artifact_relic_005",
                Name = "精灵之泪",
                Description = "古老精灵的生命结晶",
                Type = ArtifactType.Relic,
                Rarity = ArtifactRarity.Rare,
                SetId = "set_nature",
                Lore = "世界树滴落的泪水",
                Origin = "精灵森林",
                DropRate = 0.01f,
                Effects = new List<ArtifactEffect>
                {
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "magic", Value = 50, Description = "魔法+50" },
                    new ArtifactEffect { EffectType = ArtifactEffectType.StatBoost, StatName = "health", Value = 200, Description = "生命+200" }
                }
            };
        }

        private static void BuildArtifactSets()
        {
            foreach (var artifact in artifacts.Values)
            {
                if (!string.IsNullOrEmpty(artifact.SetId))
                {
                    if (!artifactSets.ContainsKey(artifact.SetId))
                    {
                        artifactSets[artifact.SetId] = new List<Artifact>();
                    }
                    artifactSets[artifact.SetId].Add(artifact);
                }
            }
        }

        public static Artifact GetArtifact(string id)
        {
            Initialize();
            return artifacts.ContainsKey(id) ? artifacts[id] : null;
        }

        public static List<Artifact> GetAllArtifacts()
        {
            Initialize();
            return new List<Artifact>(artifacts.Values);
        }

        public static List<Artifact> GetArtifactsByRarity(ArtifactRarity rarity)
        {
            Initialize();
            List<Artifact> result = new List<Artifact>();
            foreach (var artifact in artifacts.Values)
            {
                if (artifact.Rarity == rarity)
                    result.Add(artifact);
            }
            return result;
        }

        public static List<Artifact> GetArtifactsByType(ArtifactType type)
        {
            Initialize();
            List<Artifact> result = new List<Artifact>();
            foreach (var artifact in artifacts.Values)
            {
                if (artifact.Type == type)
                    result.Add(artifact);
            }
            return result;
        }

        public static List<Artifact> GetArtifactSet(string setId)
        {
            Initialize();
            return artifactSets.ContainsKey(setId) ? artifactSets[setId] : new List<Artifact>();
        }

        public static Dictionary<string, List<Artifact>> GetAllSets()
        {
            Initialize();
            return artifactSets;
        }

        public static Artifact GenerateRandomArtifact(float playerLuck = 0)
        {
            Initialize();
            float roll = (float)GD.RandDouble() + playerLuck * 0.01f;
            
            ArtifactRarity rarity;
            if (roll < 0.0001f) rarity = ArtifactRarity.Mythical;
            else if (roll < 0.002f) rarity = ArtifactRarity.Legendary;
            else if (roll < 0.01f) rarity = ArtifactRarity.Epic;
            else if (roll < 0.05f) rarity = ArtifactRarity.Rare;
            else if (roll < 0.15f) rarity = ArtifactRarity.Uncommon;
            else rarity = ArtifactRarity.Common;

            List<Artifact> candidates = GetArtifactsByRarity(rarity);
            if (candidates.Count == 0)
            {
                candidates = GetAllArtifacts();
            }
            
            return candidates[GD.RandInt() % candidates.Count];
        }

        public static string GetRarityColor(ArtifactRarity rarity)
        {
            switch (rarity)
            {
                case ArtifactRarity.Common: return "#9E9E9E";
                case ArtifactRarity.Uncommon: return "#4CAF50";
                case ArtifactRarity.Rare: return "#2196F3";
                case ArtifactRarity.Epic: return "#9C27B0";
                case ArtifactRarity.Legendary: return "#FF9800";
                case ArtifactRarity.Mythical: return "#F44336";
                default: return "#FFFFFF";
            }
        }
    }
}
