using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.SecretBoss {
    /// <summary>
    /// Secret Boss Database - 隐藏Boss配置数据库
    /// </summary>
    public static class SecretBossDatabase {
        private static readonly Dictionary<string, SecretBossData> _bosses = new Dictionary<string, SecretBossData>();
        
        static SecretBossDatabase() {
            InitializeBosses();
        }
        
        private static void InitializeBosses() {
            // Shadow of the Abyss - 深渊暗影
            _bosses["shadow_abyss"] = new SecretBossData {
                BossId = "shadow_abyss",
                BossName = "深渊暗影",
                Description = "潜伏在深渊中的古老暗影，只有月光照耀时才会出现",
                Type = SecretBossType.Shadow,
                Rarity = Rarity.Epic,
                Condition = new SecretBossCondition {
                    Type = ConditionType.MoonPhase,
                    RequiredValue = 3, // 满月
                    RequiredArea = "AbyssalPit"
                },
                BaseHealth = 50000,
                BaseAttack = 800,
                BaseDefense = 400,
                AttackSpeed = 1.2f,
                MoveSpeed = 2.5f,
                SpecialAbilities = new List<string> { "ShadowStrike", "DarkPortal", "SoulDrain" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "shadow_crystal", ItemName = "暗影水晶", MinQuantity = 5, MaxQuantity = 15, DropRate = 1.0f, IsGuaranteed = true },
                    new SecretBossDrop { ItemId = "abyss_essence", ItemName = "深渊精华", MinQuantity = 1, MaxQuantity = 3, DropRate = 0.3f }
                },
                SpawnMessage = "深渊中传来一阵寒意... 暗影Boss即将出现！",
                IconPath = "res://icons/bosses/shadow_abyss.png",
                Lore = "据说这只暗影生物已经在深渊中存活了千年，专门吞噬冒险者的灵魂"
            };
            
            // Chrono Warden - 时间守护者
            _bosses["chrono_warden"] = new SecretBossData {
                BossId = "chrono_warden",
                BossName = "时间守护者",
                Description = "时间的化身，只在特定的时空裂缝中出现",
                Type = SecretBossType.Temporal,
                Rarity = Rarity.Legendary,
                Condition = new SecretBossCondition {
                    Type = ConditionType.TimeOfDay,
                    RequiredHourStart = 0,
                    RequiredHourEnd = 4,
                    RequiredArea = "AncientRuins"
                },
                BaseHealth = 100000,
                BaseAttack = 1200,
                BaseDefense = 600,
                AttackSpeed = 0.8f,
                MoveSpeed = 2.0f,
                SpecialAbilities = new List<string> { "TimeStop", "TemporalSlash", "AgeAura" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "time_essence", ItemName = "时间精华", MinQuantity = 3, MaxQuantity = 10, DropRate = 1.0f, IsGuaranteed = true },
                    new SecretBossDrop { ItemId = "chrono_crystal", ItemName = "时空水晶", MinQuantity = 1, MaxQuantity = 5, DropRate = 0.2f }
                },
                SpawnMessage = "时空开始扭曲... 时间守护者降临！",
                IconPath = "res://icons/bosses/chrono_warden.png",
                Lore = "时间守护者是一位远古法师的意志化身，守护着时空的平衡"
            };
            
            // Chaos Serpent - 混沌巨蛇
            _bosses["chaos_serpent"] = new SecretBossData {
                BossId = "chaos_serpent",
                BossName = "混沌巨蛇",
                Description = "混沌之力的化身，在世界陷入混乱时出现",
                Type = SecretBossType.Chaos,
                Rarity = Rarity.Epic,
                Condition = new SecretBossCondition {
                    Type = ConditionType.KillCount,
                    RequiredKillAmount = 100,
                    RequiredArea = "ChaosDimension"
                },
                BaseHealth = 75000,
                BaseAttack = 1000,
                BaseDefense = 350,
                AttackSpeed = 1.5f,
                MoveSpeed = 3.0f,
                SpecialAbilities = new List<string> { "ChaosBite", "EntropyField", "MadnessAura" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "chaos_scale", ItemName = "混沌鳞片", MinQuantity = 8, MaxQuantity = 20, DropRate = 1.0f, IsGuaranteed = true },
                    new SecretBossDrop { ItemId = "serpent_heart", ItemName = "巨蛇之心", MinQuantity = 1, MaxQuantity = 2, DropRate = 0.25f }
                },
                SpawnMessage = "混沌之力汇聚... 混沌巨蛇苏醒！",
                IconPath = "res://icons/bosses/chaos_serpent.png",
                Lore = "这条巨蛇在世界之初就已经存在，以混乱和无序为食"
            };
            
            // Ancient Dragon - 远古巨龙
            _bosses["ancient_dragon"] = new SecretBossData {
                BossId = "ancient_dragon",
                BossName = "远古巨龙",
                Description = "沉睡在远古遗迹中的巨龙，只有达到一定实力才能唤醒",
                Type = SecretBossType.Ancient,
                Rarity = Rarity.Legendary,
                Condition = new SecretBossCondition {
                    Type = ConditionType.PlayerLevel,
                    RequiredPlayerLevel = 50,
                    RequiredArea = "DragonLair"
                },
                BaseHealth = 200000,
                BaseAttack = 1500,
                BaseDefense = 800,
                AttackSpeed = 0.6f,
                MoveSpeed = 1.8f,
                SpecialAbilities = new List<string> { "AncientBreath", "DragonRoar", "ScaleArmor" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "dragon_scale", ItemName = "龙鳞", MinQuantity = 10, MaxQuantity = 30, DropRate = 1.0f, IsGuaranteed = true },
                    new SecretBossDrop { ItemId = "dragon_heart", ItemName = "龙心", MinQuantity = 1, MaxQuantity = 3, DropRate = 0.15f },
                    new SecretBossDrop { ItemId = "ancient_treasure", ItemName = "远古宝藏", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.05f }
                },
                SpawnMessage = "大地开始震动... 远古巨龙苏醒！",
                IconPath = "res://icons/bosses/ancient_dragon.png",
                Lore = "这条巨龙在上古时代就已经存在，是所有龙族的祖先"
            };
            
            // Celestial Guardian - 星辰守护者
            _bosses["celestial_guardian"] = new SecretBossData {
                BossId = "celestial_guardian",
                BossName = "星辰守护者",
                Description = "星辰之力的守护者，在星空最明亮时出现",
                Type = SecretBossType.Celestial,
                Rarity = Rarity.Epic,
                Condition = new SecretBossCondition {
                    Type = ConditionType.MoonPhase,
                    RequiredValue = 0, // 新月
                    RequiredArea = "CelestialGarden"
                },
                BaseHealth = 60000,
                BaseAttack = 900,
                BaseDefense = 500,
                AttackSpeed = 1.0f,
                MoveSpeed = 2.2f,
                SpecialAbilities = new List<string> { "StarFall", "CosmicRay", "StellarAura" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "star_essence", ItemName = "星辰精华", MinQuantity = 5, MaxQuantity = 15, DropRate = 1.0f, IsGuaranteed = true },
                    new SecretBossDrop { ItemId = "celestial_stone", ItemName = "天星石", MinQuantity = 1, MaxQuantity = 3, DropRate = 0.2f }
                },
                SpawnMessage = "星光汇聚... 星辰守护者降临！",
                IconPath = "res://icons/bosses/celestial_guardian.png",
                Lore = "星辰守护者是一位古老星灵的分身，守护着通往天界的门户"
            };
            
            // Abyssal Prince - 深渊王子
            _bosses["abyssal_prince"] = new SecretBossData {
                BossId = "abyssal_prince",
                BossName = "深渊王子",
                Description = "深渊王国的继承者，只有击败其手下才会出现",
                Type = SecretBossType.Abyssal,
                Rarity = Rarity.Legendary,
                Condition = new SecretBossCondition {
                    Type = ConditionType.BossDefeated,
                    RequiredBossDefeated = "shadow_abyss",
                    RequiredArea = "AbyssalPit"
                },
                BaseHealth = 150000,
                BaseAttack = 1300,
                BaseDefense = 700,
                AttackSpeed = 0.9f,
                MoveSpeed = 2.3f,
                SpecialAbilities = new List<string> { "AbyssGate", "DarkPunishment", "SoulHarvest" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "prince_crown", ItemName = "王子冠冕", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.1f },
                    new SecretBossDrop { ItemId = "abyss_throne", ItemName = "深渊王座", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.05f }
                },
                SpawnMessage = "深渊之门开启... 深渊王子驾到！",
                IconPath = "res://icons/bosses/abyssal_prince.png",
                Lore = "深渊王子是深渊王国的下一任统治者，拥有操控黑暗的力量"
            };
            
            // Phantom King - 幻影之王
            _bosses["phantom_king"] = new SecretBossData {
                BossId = "phantom_king",
                BossName = "幻影之王",
                Description = "幻影王国的统治者，只有999连击时才会出现",
                Type = SecretBossType.Phantom,
                Rarity = Rarity.Epic,
                Condition = new SecretBossCondition {
                    Type = ConditionType.ComboCount,
                    RequiredValue = 999,
                    RequiredArea = "ShadowRealm"
                },
                BaseHealth = 80000,
                BaseAttack = 1100,
                BaseDefense = 450,
                AttackSpeed = 1.3f,
                MoveSpeed = 2.8f,
                SpecialAbilities = new List<string> { "PhantomStrike", "MirrorImage", "SoulSiphon" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "phantom_crown", ItemName = "幻影王冠", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.15f },
                    new SecretBossDrop { ItemId = "shadow_weave", ItemName = "暗影编织", MinQuantity = 3, MaxQuantity = 8, DropRate = 0.5f }
                },
                SpawnMessage = "幻影领域开启... 幻影之王现身！",
                IconPath = "res://icons/bosses/phantom_king.png",
                Lore = "幻影之王是所有幻影生物的领袖，擅长制造幻象和欺骗"
            };
            
            // Divine Judge - 神性审判者
            _bosses["divine_judge"] = new SecretBossData {
                BossId = "divine_judge",
                BossName = "神性审判者",
                Description = "神的意志化身，审判所有不洁之人",
                Type = SecretBossType.Divine,
                Rarity = Rarity.Legendary,
                Condition = new SecretBossCondition {
                    Type = ConditionType.Luck,
                    RequiredLuck = 100,
                    RequiredArea = "HeavenlyTemple"
                },
                BaseHealth = 180000,
                BaseAttack = 1400,
                BaseDefense = 750,
                AttackSpeed = 0.7f,
                MoveSpeed = 1.9f,
                SpecialAbilities = new List<string> { "DivineJudgment", "HolyWrath", "Sanctuary" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "holy_grail", ItemName = "圣杯", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.08f },
                    new SecretBossDrop { ItemId = "divine_blessing", ItemName = "神圣祝福", MinQuantity = 5, MaxQuantity = 15, DropRate = 1.0f, IsGuaranteed = true }
                },
                SpawnMessage = "神圣之光降临... 神性审判者出现！",
                IconPath = "res://icons/bosses/divine_judge.png",
                Lore = "神性审判者是神在凡间的代言人，负责清除一切邪恶"
            };
            
            // Thunder Lord - 雷神
            _bosses["thunder_lord"] = new SecretBossData {
                BossId = "thunder_lord",
                BossName = "雷神",
                Description = "雷暴之主，只在雷雨天出现",
                Type = SecretBossType.Celestial,
                Rarity = Rarity.Rare,
                Condition = new SecretBossCondition {
                    Type = ConditionType.Weather,
                    RequiredWeather = WeatherType.Thunderstorm,
                    RequiredArea = "Mountain"
                },
                BaseHealth = 45000,
                BaseAttack = 750,
                BaseDefense = 350,
                AttackSpeed = 1.1f,
                MoveSpeed = 2.4f,
                SpecialAbilities = new List<string> { "ThunderStrike", "StormCall", "LightningAura" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "thunder_orb", ItemName = "雷鸣宝珠", MinQuantity = 3, MaxQuantity = 8, DropRate = 1.0f, IsGuaranteed = true },
                    new SecretBossDrop { ItemId = "storm_feather", ItemName = "风暴羽毛", MinQuantity = 1, MaxQuantity = 3, DropRate = 0.25f }
                },
                SpawnMessage = "雷云汇聚... 雷神降临！",
                IconPath = "res://icons/bosses/thunder_lord.png",
                Lore = "雷神是天气之神，在雷暴中展现神力"
            };
            
            // Frost King - 冰霜之王
            _bosses["frost_king"] = new SecretBossData {
                BossId = "frost_king",
                BossName = "冰霜之王",
                Description = "冰雪国度的统治者，在极寒之地出现",
                Type = SecretBossType.Ancient,
                Rarity = Rarity.Rare,
                Condition = new SecretBossCondition {
                    Type = ConditionType.Weather,
                    RequiredWeather = WeatherType.Blizzard,
                    RequiredArea = "IcePeak"
                },
                BaseHealth = 40000,
                BaseAttack = 700,
                BaseDefense = 400,
                AttackSpeed = 1.0f,
                MoveSpeed = 2.1f,
                SpecialAbilities = new List<string> { "IceBlast", "FrozenGround", "BlizzardAura" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "frost_crystal", ItemName = "冰晶", MinQuantity = 5, MaxQuantity = 12, DropRate = 1.0f, IsGuaranteed = true },
                    new SecretBossDrop { ItemId = "ice_crown", ItemName = "冰霜王冠", MinQuantity = 1, MaxQuantity = 1, DropRate = 0.15f }
                },
                SpawnMessage = "寒冰之力汇聚... 冰霜之王苏醒！",
                IconPath = "res://icons/bosses/frost_king.png",
                Lore = "冰霜之王是冰雪国度的永恒统治者，掌控着极寒之力"
            };
            
            // Volcanic Lord - 火山之主
            _bosses["volcanic_lord"] = new SecretBossData {
                BossId = "volcanic_lord",
                BossName = "火山之主",
                Description = "火焰与毁灭的化身，在火山地带出现",
                Type = SecretBossType.Chaos,
                Rarity = Rarity.Rare,
                Condition = new SecretBossCondition {
                    Type = ConditionType.Location,
                    RequiredArea = "VolcanicDepths"
                },
                BaseHealth = 55000,
                BaseAttack = 850,
                BaseDefense = 300,
                AttackSpeed = 1.2f,
                MoveSpeed = 2.6f,
                SpecialAbilities = new List<string> { "LavaEruption", "MoltenStrike", "FireAura" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "magma_heart", ItemName = "岩浆之心", MinQuantity = 2, MaxQuantity = 6, DropRate = 1.0f, IsGuaranteed = true },
                    new SecretBossDrop { ItemId = "volcanic_gem", ItemName = "火山宝石", MinQuantity = 1, MaxQuantity = 2, DropRate = 0.2f }
                },
                SpawnMessage = "大地颤抖... 火山之主苏醒！",
                IconPath = "res://icons/bosses/volcanic_lord.png",
                Lore = "火山之主是毁灭之神的化身，掌控着火焰与大地的力量"
            };
            
            // Forest Ancient - 森林古灵
            _bosses["forest_ancient"] = new SecretBossData {
                BossId = "forest_ancient",
                BossName = "森林古灵",
                Description = "森林的守护灵，在特定的夜晚出现",
                Type = SecretBossType.Ancient,
                Rarity = Rarity.Uncommon,
                Condition = new SecretBossCondition {
                    Type = ConditionType.TimeOfDay,
                    RequiredHourStart = 20,
                    RequiredHourEnd = 24,
                    RequiredArea = "EnchantedForest"
                },
                BaseHealth = 30000,
                BaseAttack = 500,
                BaseDefense = 350,
                AttackSpeed = 0.9f,
                MoveSpeed = 2.0f,
                SpecialAbilities = new List<string> { "RootEntangle", "NatureHeal", "ForestAura" },
                Drops = new List<SecretBossDrop> {
                    new SecretBossDrop { ItemId = "ancient_herb", ItemName = "古老草药", MinQuantity = 5, MaxQuantity = 15, DropRate = 1.0f, IsGuaranteed = true },
                    new SecretBossDrop { ItemId = "spirit_essence", ItemName = "精灵精华", MinQuantity = 1, MaxQuantity = 3, DropRate = 0.3f }
                },
                SpawnMessage = "森林中发出柔和的光芒... 森林古灵出现！",
                IconPath = "res://icons/bosses/forest_ancient.png",
                Lore = "森林古灵是森林的生命之灵，守护着每一棵树每一株草"
            };
        }
        
        public static SecretBossData GetBoss(string bossId) {
            return _bosses.ContainsKey(bossId) ? _bosses[bossId] : null;
        }
        
        public static List<SecretBossData> GetAllBosses() {
            return new List<SecretBossData>(_bosses.Values);
        }
        
        public static List<SecretBossData> GetBossesByRarity(Rarity rarity) {
            List<SecretBossData> result = new List<SecretBossData>();
            foreach (var boss in _bosses.Values) {
                if (boss.Rarity == rarity) {
                    result.Add(boss);
                }
            }
            return result;
        }
        
        public static List<SecretBossData> GetBossesByType(SecretBossType type) {
            List<SecretBossData> result = new List<SecretBossData>();
            foreach (var boss in _bosses.Values) {
                if (boss.Type == type) {
                    result.Add(boss);
                }
            }
            return result;
        }
        
        public static int GetTotalBossCount() {
            return _bosses.Count;
        }
        
        public static int GetDiscoveredCount() {
            int count = 0;
            foreach (var boss in _bosses.Values) {
                if (boss.IsDiscovered) count++;
            }
            return count;
        }
        
        public static int GetDefeatedCount() {
            int count = 0;
            foreach (var boss in _bosses.Values) {
                if (boss.IsDefeated) count++;
            }
            return count;
        }
    }
}
