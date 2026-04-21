using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using ClawRPG.Scripts.Data;
using AchievementDifficulty = ClawRPG.Scripts.Data.AchievementDifficulty;

namespace ClawRPG.Scripts.Database.Loaders
{
    /// <summary>
    /// 成就配置数据（JSON反序列化用）
    /// </summary>
    public class AchievementConfigData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; }

        [JsonPropertyName("type")]
        public string Type { get; set; }

        [JsonPropertyName("difficulty")]
        public string Difficulty { get; set; }

        [JsonPropertyName("requiredValue")]
        public int RequiredValue { get; set; }

        [JsonPropertyName("rewardGold")]
        public int RewardGold { get; set; }

        [JsonPropertyName("rewardExp")]
        public int RewardExp { get; set; }
    }

    /// <summary>
    /// 成就配置文件结构
    /// </summary>
    public class AchievementsConfigFile
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }

        [JsonPropertyName("achievements")]
        public List<AchievementConfigData> Achievements { get; set; }
    }

    /// <summary>
    /// 成就配置加载器 - 负责从JSON文件加载成就数据
    /// </summary>
    public class AchievementConfigLoader
    {
        private static AchievementConfigLoader _instance;
        private AchievementsConfigFile _configFile;
        private bool _isLoaded = false;
        private string _lastError = string.Empty;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static AchievementConfigLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new AchievementConfigLoader();
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
        /// 加载成就配置文件
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
                    _lastError = $"成就配置文件不存在: {configPath}";
                    GD.PrintErr($"[AchievementConfigLoader] {_lastError}");
                    return false;
                }

                string json = System.IO.File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _configFile = JsonSerializer.Deserialize<AchievementsConfigFile>(json, options);

                if (_configFile == null || _configFile.Achievements == null)
                {
                    _lastError = "成就配置文件格式错误：无法解析数据";
                    GD.PrintErr($"[AchievementConfigLoader] {_lastError}");
                    return false;
                }

                _isLoaded = true;
                GD.Print($"[AchievementConfigLoader] 成功加载 {_configFile.Achievements.Count} 个成就配置");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"成就配置加载失败: {ex.Message}";
                GD.PrintErr($"[AchievementConfigLoader] {_lastError}");
                return false;
            }
        }

        /// <summary>
        /// 从配置数据转换为Achievement
        /// </summary>
        /// <param name="config">成就配置数据</param>
        /// <returns>Achievement实例</returns>
        public Achievement ConvertToAchievement(AchievementConfigData config)
        {
            if (config == null) return null;

            return new Achievement
            {
                Id = config.Id,
                Name = config.Name,
                Description = config.Description,
                Type = ParseAchievementType(config.Type),
                Difficulty = ParseAchievementDifficulty(config.Difficulty),
                RequiredValue = config.RequiredValue,
                RewardGold = config.RewardGold,
                RewardExp = config.RewardExp
            };
        }

        /// <summary>
        /// 解析成就类型
        /// </summary>
        private AchievementType ParseAchievementType(string type)
        {
            if (Enum.TryParse<AchievementType>(type, true, out var result))
            {
                return result;
            }
            GD.PrintErr($"[AchievementConfigLoader] 未知成就类型: {type}, 默认设置为Kill");
            return AchievementType.Kill;
        }

        /// <summary>
        /// 解析成就难度
        /// </summary>
        private AchievementDifficulty ParseAchievementDifficulty(string difficulty)
        {
            if (Enum.TryParse<AchievementDifficulty>(difficulty, true, out var result))
            {
                return result;
            }
            GD.PrintErr($"[AchievementConfigLoader] 未知成就难度: {difficulty}, 默认设置为Normal");
            return AchievementDifficulty.Normal;
        }

        /// <summary>
        /// 获取所有成就配置数据
        /// </summary>
        /// <returns>成就配置列表</returns>
        public List<AchievementConfigData> GetAllAchievementConfigs()
        {
            if (_configFile?.Achievements == null)
            {
                return new List<AchievementConfigData>();
            }
            return new List<AchievementConfigData>(_configFile.Achievements);
        }

        /// <summary>
        /// 获取所有成就列表
        /// </summary>
        /// <returns>Achievement列表</returns>
        public List<Achievement> GetAllAchievements()
        {
            var achievements = new List<Achievement>();
            if (_configFile?.Achievements == null) return achievements;

            foreach (var config in _configFile.Achievements)
            {
                var achievement = ConvertToAchievement(config);
                if (achievement != null)
                {
                    achievements.Add(achievement);
                }
            }
            return achievements;
        }

        /// <summary>
        /// 根据ID获取成就配置
        /// </summary>
        /// <param name="id">成就ID</param>
        /// <returns>成就配置数据</returns>
        public AchievementConfigData GetAchievementConfigById(string id)
        {
            if (_configFile?.Achievements == null) return null;
            return _configFile.Achievements.Find(a => a.Id == id);
        }

        /// <summary>
        /// 根据ID获取成就
        /// </summary>
        /// <param name="id">成就ID</param>
        /// <returns>Achievement实例</returns>
        public Achievement GetAchievementById(string id)
        {
            var config = GetAchievementConfigById(id);
            return config != null ? ConvertToAchievement(config) : null;
        }

        /// <summary>
        /// 获取配置版本
        /// </summary>
        public string GetVersion()
        {
            return _configFile?.Version ?? "unknown";
        }

        /// <summary>
        /// 获取成就总数
        /// </summary>
        public int GetAchievementCount()
        {
            return _configFile?.Achievements?.Count ?? 0;
        }
    }
}
