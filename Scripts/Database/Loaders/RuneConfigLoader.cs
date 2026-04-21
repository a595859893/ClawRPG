using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;

namespace ClawRPG.Scripts.Database.Loaders
{
    /// <summary>
    /// 符文配置数据（JSON反序列化用）
    /// </summary>
    public class RuneConfigData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("rarity")]
        public string Rarity { get; set; }
        
        [JsonPropertyName("slotType")]
        public string SlotType { get; set; }
        
        [JsonPropertyName("attackBonus")]
        public float AttackBonus { get; set; }
        
        [JsonPropertyName("defenseBonus")]
        public float DefenseBonus { get; set; }
        
        [JsonPropertyName("healthBonus")]
        public float HealthBonus { get; set; }
        
        [JsonPropertyName("critRateBonus")]
        public float CritRateBonus { get; set; }
        
        [JsonPropertyName("critDamageBonus")]
        public float CritDamageBonus { get; set; }
        
        [JsonPropertyName("lifeStealBonus")]
        public float LifeStealBonus { get; set; }
        
        [JsonPropertyName("dodgeBonus")]
        public float DodgeBonus { get; set; }
        
        [JsonPropertyName("speedBonus")]
        public float SpeedBonus { get; set; }
        
        [JsonPropertyName("blockBonus")]
        public float BlockBonus { get; set; }
        
        [JsonPropertyName("specialEffect")]
        public string SpecialEffect { get; set; }
        
        [JsonPropertyName("specialEffectValue")]
        public float SpecialEffectValue { get; set; }
        
        [JsonPropertyName("requiredLevel")]
        public int RequiredLevel { get; set; }
    }

    /// <summary>
    /// 符文配置文件结构
    /// </summary>
    public class RunesConfigFile
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        
        [JsonPropertyName("runes")]
        public List<RuneConfigData> Runes { get; set; }
    }

    /// <summary>
    /// 符文配置加载器 - 负责从JSON文件加载符文数据
    /// </summary>
    public class RuneConfigLoader
    {
        private static RuneConfigLoader _instance;
        private RunesConfigFile _configFile;
        private bool _isLoaded = false;
        private string _lastError = string.Empty;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static RuneConfigLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new RuneConfigLoader();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 配置是否已加载
        /// </summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// 最后一次错误信息
        /// </summary>
        public string LastError => _lastError;

        /// <summary>
        /// 加载符文配置文件
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <returns>加载是否成功</returns>
        public bool Load(string configPath)
        {
            try
            {
                _lastError = string.Empty;

                if (!System.IO.File.Exists(configPath))
                {
                    _lastError = $"符文配置文件不存在: {configPath}";
                    GD.PrintErr($"[RuneConfigLoader] {_lastError}");
                    return false;
                }

                string json = System.IO.File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                _configFile = JsonSerializer.Deserialize<RunesConfigFile>(json, options);
                
                if (_configFile == null || _configFile.Runes == null)
                {
                    _lastError = "符文配置文件格式错误：无法解析数据";
                    GD.PrintErr($"[RuneConfigLoader] {_lastError}");
                    return false;
                }

                _isLoaded = true;
                GD.Print($"[RuneConfigLoader] 成功加载 {_configFile.Runes.Count} 个符文配置");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"符文配置加载失败: {ex.Message}";
                GD.PrintErr($"[RuneConfigLoader] {_lastError}");
                return false;
            }
        }

        /// <summary>
        /// 从配置数据转换为RuneData
        /// </summary>
        /// <param name="configData">配置数据</param>
        /// <returns>RuneData实例</returns>
        public RuneData ConvertToRuneData(RuneConfigData configData)
        {
            if (configData == null) return null;

            return new RuneData
            {
                Id = configData.Id,
                Name = configData.Name,
                Description = configData.Description,
                Type = ParseRuneType(configData.Type),
                Rarity = ParseRuneRarity(configData.Rarity),
                SlotType = ParseRuneSlotType(configData.SlotType),
                AttackBonus = configData.AttackBonus,
                DefenseBonus = configData.DefenseBonus,
                HealthBonus = configData.HealthBonus,
                CritRateBonus = configData.CritRateBonus,
                CritDamageBonus = configData.CritDamageBonus,
                LifeStealBonus = configData.LifeStealBonus,
                DodgeBonus = configData.DodgeBonus,
                SpeedBonus = configData.SpeedBonus,
                BlockBonus = configData.BlockBonus,
                SpecialEffect = configData.SpecialEffect ?? string.Empty,
                SpecialEffectValue = configData.SpecialEffectValue,
                RequiredLevel = configData.RequiredLevel
            };
        }

        /// <summary>
        /// 获取所有符文配置数据
        /// </summary>
        /// <returns>符文配置列表</returns>
        public List<RuneConfigData> GetAllRuneConfigs()
        {
            if (_configFile?.Runes == null)
            {
                return new List<RuneConfigData>();
            }
            return new List<RuneConfigData>(_configFile.Runes);
        }

        /// <summary>
        /// 获取所有符文RuneData列表
        /// </summary>
        /// <returns>RuneData列表</returns>
        public List<RuneData> GetAllRuneData()
        {
            var runeDataList = new List<RuneData>();
            if (_configFile?.Runes == null) return runeDataList;

            foreach (var config in _configFile.Runes)
            {
                var runeData = ConvertToRuneData(config);
                if (runeData != null)
                {
                    runeDataList.Add(runeData);
                }
            }
            return runeDataList;
        }

        /// <summary>
        /// 根据ID获取符文配置
        /// </summary>
        /// <param name="id">符文ID</param>
        /// <returns>符文配置数据</returns>
        public RuneConfigData GetRuneConfigById(string id)
        {
            if (_configFile?.Runes == null) return null;
            return _configFile.Runes.Find(r => r.Id == id);
        }

        /// <summary>
        /// 获取配置版本
        /// </summary>
        public string GetVersion()
        {
            return _configFile?.Version ?? "unknown";
        }

        /// <summary>
        /// 获取符文总数
        /// </summary>
        public int GetRuneCount()
        {
            return _configFile?.Runes?.Count ?? 0;
        }

        #region 解析方法

        private RuneType ParseRuneType(string type)
        {
            return type switch
            {
                "Offensive" => RuneType.Offensive,
                "Defensive" => RuneType.Defensive,
                "Utility" => RuneType.Utility,
                "Special" => RuneType.Special,
                _ => RuneType.Offensive
            };
        }

        private RuneRarity ParseRuneRarity(string rarity)
        {
            return rarity switch
            {
                "Common" => RuneRarity.Common,
                "Uncommon" => RuneRarity.Uncommon,
                "Rare" => RuneRarity.Rare,
                "Epic" => RuneRarity.Epic,
                "Legendary" => RuneRarity.Legendary,
                _ => RuneRarity.Common
            };
        }

        private RuneSlotType ParseRuneSlotType(string slotType)
        {
            return slotType switch
            {
                "Weapon" => RuneSlotType.Weapon,
                "Shield" => RuneSlotType.Shield,
                "Chestplate" => RuneSlotType.Chestplate,
                "Helmet" => RuneSlotType.Helmet,
                "Boots" => RuneSlotType.Boots,
                "Ring" => RuneSlotType.Ring,
                "Amulet" => RuneSlotType.Amulet,
                "Any" => RuneSlotType.Any,
                _ => RuneSlotType.Any
            };
        }

        #endregion
    }
}
