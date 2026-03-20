// ============================================
// RelicConfigLoader - 圣物配置加载器
// 从 JSON 配置文件加载圣物数据
// ============================================

using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.Relics
{
    /// <summary>
    /// 圣物配置加载器
    /// 负责从 JSON 文件加载圣物配置数据
    /// </summary>
    public static class RelicConfigLoader
    {
        private static Dictionary<string, Relic> _relics;
        private static Dictionary<string, RelicSet> _relicSets;
        private static RelicGenerationConfig _generationConfig;
        private static bool _isLoaded = false;
        private static string _lastError = null;

        /// <summary>
        /// 获取所有圣物
        /// </summary>
        public static Dictionary<string, Relic> Relics => _relics;

        /// <summary>
        /// 获取所有圣物套装
        /// </summary>
        public static Dictionary<string, RelicSet> RelicSets => _relicSets;

        /// <summary>
        /// 获取生成配置
        /// </summary>
        public static RelicGenerationConfig GenerationConfig => _generationConfig;

        /// <summary>
        /// 是否已加载
        /// </summary>
        public static bool IsLoaded => _isLoaded;

        /// <summary>
        /// 上次加载错误信息
        /// </summary>
        public static string LastError => _lastError;

        /// <summary>
        /// 加载圣物配置
        /// </summary>
        /// <param name="configPath">配置文件路径 (相对于 Resources)</param>
        /// <returns>是否加载成功</returns>
        public static bool Load(string configPath = "Config/relics_config.json")
        {
            _lastError = null;
            
            try
            {
                var fullPath = ProjectSettings.GlobalizePath($"res://{configPath}");
                
                // 使用 Godot 的 FileAccess 读取文件
                using (var file = FileAccess.Open(fullPath, FileAccess.ModeFlags.Read))
                {
                    if (file == null)
                    {
                        _lastError = $"无法打开配置文件: {fullPath}, 错误码: {FileAccess.GetError()}";
                        GD.PrintErr($"[RelicConfigLoader] {_lastError}");
                        return false;
                    }

                    var jsonString = file.GetAsText();
                    ParseJson(jsonString);
                }

                _isLoaded = true;
                GD.Print($"[RelicConfigLoader] 成功加载 {_relics.Count} 个圣物, {_relicSets.Count} 个套装");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"加载配置文件时出错: {ex.Message}";
                GD.PrintErr($"[RelicConfigLoader] {_lastError}");
                return false;
            }
        }

        /// <summary>
        /// 解析 JSON 数据
        /// </summary>
        private static void ParseJson(string jsonString)
        {
            _relics = new Dictionary<string, Relic>();
            _relicSets = new Dictionary<string, RelicSet>();

            var json = new Json();
            var parseResult = json.Parse(jsonString);
            
            if (parseResult != Error.Ok)
            {
                _lastError = $"JSON 解析失败: {parseResult}";
                GD.PrintErr($"[RelicConfigLoader] {_lastError}");
                return;
            }

            var root = json.Data.AsGodotDictionary();
            
            // 解析圣物列表
            if (root.ContainsKey("relics"))
            {
                var relicsArray = root["relics"].AsGodotArray();
                foreach (var item in relicsArray)
                {
                    var relicDict = item.AsGodotDictionary();
                    var relic = ParseRelic(relicDict);
                    if (relic != null)
                    {
                        _relics[relic.Id] = relic;
                    }
                }
            }

            // 解析套装列表
            if (root.ContainsKey("relicSets"))
            {
                var setsArray = root["relicSets"].AsGodotArray();
                foreach (var item in setsArray)
                {
                    var setDict = item.AsGodotDictionary();
                    var set = ParseRelicSet(setDict);
                    if (set != null)
                    {
                        _relicSets[set.Id] = set;
                    }
                }
            }

            // 解析生成配置
            if (root.ContainsKey("generationConfig"))
            {
                _generationConfig = ParseGenerationConfig(root["generationConfig"].AsGodotDictionary());
            }
            else
            {
                // 使用默认配置
                _generationConfig = new RelicGenerationConfig
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
            }
        }

        /// <summary>
        /// 解析单个圣物
        /// </summary>
        private static Relic ParseRelic(GodotDictionary dict)
        {
            try
            {
                var relic = new Relic
                {
                    Id = dict.GetValueOrDefault("id", "").ToString(),
                    Name = dict.GetValueOrDefault("name", "").ToString(),
                    Description = dict.GetValueOrDefault("description", "").ToString(),
                    Type = ParseRelicType(dict.GetValueOrDefault("type", "Weapon").ToString()),
                    Rarity = ParseRarity(dict.GetValueOrDefault("rarity", "Common").ToString()),
                    PrimaryEffect = ParseEffectType(dict.GetValueOrDefault("primaryEffect", "DamageIncrease").ToString()),
                    PrimaryEffectValue = Convert.ToDouble(dict.GetValueOrDefault("primaryEffectValue", 0.05)),
                    Level = Convert.ToInt32(dict.GetValueOrDefault("level", 1))
                };

                // 解析可选的副效果
                var secondaryEffect = dict.GetValueOrDefault("secondaryEffect", null);
                if (secondaryEffect != null && !string.IsNullOrEmpty(secondaryEffect.ToString()))
                {
                    relic.SecondaryEffect = ParseEffectType(secondaryEffect.ToString());
                    
                    var secondaryValue = dict.GetValueOrDefault("secondaryEffectValue", null);
                    if (secondaryValue != null)
                    {
                        relic.SecondaryEffectValue = Convert.ToDouble(secondaryValue);
                    }
                }

                // 解析套装ID
                var setId = dict.GetValueOrDefault("setId", null);
                if (setId != null && !string.IsNullOrEmpty(setId.ToString()))
                {
                    relic.SetId = setId.ToString();
                }

                return relic;
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RelicConfigLoader] 解析圣物失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析单个套装
        /// </summary>
        private static RelicSet ParseRelicSet(GodotDictionary dict)
        {
            try
            {
                return new RelicSet
                {
                    Id = dict.GetValueOrDefault("id", "").ToString(),
                    Name = dict.GetValueOrDefault("name", "").ToString(),
                    Description = dict.GetValueOrDefault("description", "").ToString(),
                    RequiredCount = Convert.ToInt32(dict.GetValueOrDefault("requiredCount", 3)),
                    SetEffect = ParseEffectType(dict.GetValueOrDefault("setEffect", "DamageIncrease").ToString()),
                    SetEffectValue = Convert.ToDouble(dict.GetValueOrDefault("setEffectValue", 0.0))
                };
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RelicConfigLoader] 解析套装失败: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// 解析生成配置
        /// </summary>
        private static RelicGenerationConfig ParseGenerationConfig(GodotDictionary dict)
        {
            try
            {
                return new RelicGenerationConfig
                {
                    MinRelicsPerFloor = Convert.ToInt32(dict.GetValueOrDefault("minRelicsPerFloor", 1)),
                    MaxRelicsPerFloor = Convert.ToInt32(dict.GetValueOrDefault("maxRelicsPerFloor", 3)),
                    CommonChance = Convert.ToDouble(dict.GetValueOrDefault("commonChance", 0.40)),
                    UncommonChance = Convert.ToDouble(dict.GetValueOrDefault("uncommonChance", 0.30)),
                    RareChance = Convert.ToDouble(dict.GetValueOrDefault("rareChance", 0.18)),
                    EpicChance = Convert.ToDouble(dict.GetValueOrDefault("epicChance", 0.08)),
                    LegendaryChance = Convert.ToDouble(dict.GetValueOrDefault("legendaryChance", 0.03)),
                    MythicChance = Convert.ToDouble(dict.GetValueOrDefault("mythicChance", 0.01))
                };
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[RelicConfigLoader] 解析生成配置失败: {ex.Message}");
                return new RelicGenerationConfig();
            }
        }

        /// <summary>
        /// 解析圣物稀有度
        /// </summary>
        private static RelicRarity ParseRarity(string rarity)
        {
            return rarity.ToLower() switch
            {
                "common" => RelicRarity.Common,
                "uncommon" => RelicRarity.Uncommon,
                "rare" => RelicRarity.Rare,
                "epic" => RelicRarity.Epic,
                "legendary" => RelicRarity.Legendary,
                "mythic" => RelicRarity.Mythic,
                _ => RelicRarity.Common
            };
        }

        /// <summary>
        /// 解析圣物类型
        /// </summary>
        private static RelicType ParseRelicType(string type)
        {
            return type.ToLower() switch
            {
                "weapon" => RelicType.Weapon,
                "armor" => RelicType.Armor,
                "accessory" => RelicType.Accessory,
                "passive" => RelicType.Passive,
                "trigger" => RelicType.Trigger,
                "set" => RelicType.Set,
                _ => RelicType.Accessory
            };
        }

        /// <summary>
        /// 解析效果类型
        /// </summary>
        private static RelicEffectType ParseEffectType(string effect)
        {
            return effect.ToLower() switch
            {
                "damageincrease" => RelicEffectType.DamageIncrease,
                "damagereduction" => RelicEffectType.DamageReduction,
                "criticalrate" => RelicEffectType.CriticalRate,
                "criticaldamage" => RelicEffectType.CriticalDamage,
                "attackspeed" => RelicEffectType.AttackSpeed,
                "movespeed" => RelicEffectType.MoveSpeed,
                "healthmax" => RelicEffectType.HealthMax,
                "manamax" => RelicEffectType.ManaMax,
                "healthregen" => RelicEffectType.HealthRegen,
                "manaregen" => RelicEffectType.ManaRegen,
                "lifesteal" => RelicEffectType.LifeSteal,
                "cooldownreduction" => RelicEffectType.CooldownReduction,
                "elementaldamage" => RelicEffectType.ElementalDamage,
                "elementalresist" => RelicEffectType.ElementalResist,
                "goldgain" => RelicEffectType.GoldGain,
                "experiencegain" => RelicEffectType.ExperienceGain,
                "droprate" => RelicEffectType.DropRate,
                "enemyscale" => RelicEffectType.EnemyScale,
                "roomreward" => RelicEffectType.RoomReward,
                _ => RelicEffectType.DamageIncrease
            };
        }

        /// <summary>
        /// 重新加载配置
        /// </summary>
        public static bool Reload(string configPath = "Config/relics_config.json")
        {
            _isLoaded = false;
            return Load(configPath);
        }

        /// <summary>
        /// 获取稀有度颜色
        /// </summary>
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

        /// <summary>
        /// 获取随机遗物
        /// </summary>
        public static Relic GetRandomRelic()
        {
            if (_relics == null || _relics.Count == 0 || _generationConfig == null)
                return null;

            var random = new Random();
            var roll = random.NextDouble();
            
            RelicRarity rarity;
            if (roll < _generationConfig.MythicChance)
                rarity = RelicRarity.Mythic;
            else if (roll < _generationConfig.MythicChance + _generationConfig.LegendaryChance)
                rarity = RelicRarity.Legendary;
            else if (roll < _generationConfig.MythicChance + _generationConfig.LegendaryChance + _generationConfig.EpicChance)
                rarity = RelicRarity.Epic;
            else if (roll < _generationConfig.MythicChance + _generationConfig.LegendaryChance + _generationConfig.EpicChance + _generationConfig.RareChance)
                rarity = RelicRarity.Rare;
            else if (roll < _generationConfig.MythicChance + _generationConfig.LegendaryChance + _generationConfig.EpicChance + _generationConfig.RareChance + _generationConfig.UncommonChance)
                rarity = RelicRarity.Uncommon;
            else
                rarity = RelicRarity.Common;

            var relicsOfRarity = new List<Relic>();
            foreach (var relic in _relics.Values)
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
