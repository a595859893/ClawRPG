using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using ClawRPG.Data.FateWeaving;

namespace ClawRPG.Scripts.Database.Loaders
{
    /// <summary>
    /// 命运卡牌配置加载器 - 负责从JSON文件加载命运卡牌数据
    /// </summary>
    public class FateCardConfigLoader
    {
        private static FateCardConfigLoader _instance;
        private FateCardsConfigFile _configFile;
        private bool _isLoaded = false;
        private string _lastError = string.Empty;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static FateCardConfigLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new FateCardConfigLoader();
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
        /// 加载命运卡牌配置文件
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <returns>加载是否成功</returns>
        public bool Load(string configPath)
        {
            try
            {
                _lastError = string.Empty;

                if (!File.Exists(configPath))
                {
                    _lastError = $"命运卡牌配置文件不存在: {configPath}";
                    GD.PrintErr($"[FateCardConfigLoader] {_lastError}");
                    return false;
                }

                string json = File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };

                _configFile = JsonSerializer.Deserialize<FateCardsConfigFile>(json, options);

                if (_configFile == null)
                {
                    _lastError = "命运卡牌配置文件格式错误：无法解析数据";
                    GD.PrintErr($"[FateCardConfigLoader] {_lastError}");
                    return false;
                }

                _isLoaded = true;
                GD.Print($"[FateCardConfigLoader] 成功加载 {_configFile.paths.Count} 个路径配置和 {_configFile.choices.Count} 个选择配置");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"命运卡牌配置加载失败: {ex.Message}";
                GD.PrintErr($"[FateCardConfigLoader] {_lastError}");
                return false;
            }
        }

        /// <summary>
        /// 将路径配置数据转换为FatePathData
        /// </summary>
        /// <param name="config">路径配置数据</param>
        /// <returns>FatePathData实例</returns>
        public FatePathData ConvertToPathData(FatePathConfigData config)
        {
            if (config == null) return null;

            var pathData = new FatePathData
            {
                Name = config.name,
                Description = config.description ?? string.Empty,
                UnlockTier = config.unlockTier
            };

            // 转换路径类型
            if (Enum.TryParse<FatePathType>(config.type, true, out var pathType))
            {
                pathData.Type = pathType;
            }
            else
            {
                GD.PrintErr($"[FateCardConfigLoader] 未知路径类型: {config.type}");
                pathData.Type = FatePathType.Hero;
            }

            // 转换路径加成
            pathData.PathBonuses = new Dictionary<string, float>();
            if (config.pathBonuses != null)
            {
                foreach (var kvp in config.pathBonuses)
                {
                    pathData.PathBonuses[kvp.Key] = kvp.Value;
                }
            }

            // 转换独占选择
            pathData.ExclusiveChoices = new List<string>();
            if (config.exclusiveChoices != null)
            {
                pathData.ExclusiveChoices.AddRange(config.exclusiveChoices);
            }

            return pathData;
        }

        /// <summary>
        /// 将选择配置数据转换为FateChoice
        /// </summary>
        /// <param name="config">选择配置数据</param>
        /// <returns>FateChoice实例</returns>
        public FateChoice ConvertToChoiceData(FateChoiceConfigData config)
        {
            if (config == null) return null;

            var choice = new FateChoice
            {
                Id = config.id,
                Title = config.title,
                Description = config.description ?? string.Empty,
                ConsequenceDescription = config.consequenceDescription ?? string.Empty,
                IsSecret = config.isSecret,
                TierRequired = config.tierRequired
            };

            // 转换选择类型
            if (Enum.TryParse<FateChoiceType>(config.choiceType, true, out var choiceType))
            {
                choice.ChoiceType = choiceType;
            }
            else
            {
                GD.PrintErr($"[FateCardConfigLoader] 未知选择类型: {config.choiceType}");
                choice.ChoiceType = FateChoiceType.Moral;
            }

            // 转换路径影响
            choice.PathInfluence = new Dictionary<FatePathType, float>();
            if (config.pathInfluence != null)
            {
                foreach (var kvp in config.pathInfluence)
                {
                    if (Enum.TryParse<FatePathType>(kvp.Key, true, out var pathType))
                    {
                        choice.PathInfluence[pathType] = kvp.Value;
                    }
                    else
                    {
                        GD.PrintErr($"[FateCardConfigLoader] 未知路径类型: {kvp.Key}");
                    }
                }
            }

            // 转换属性加成
            choice.StatBonuses = new Dictionary<string, float>();
            if (config.statBonuses != null)
            {
                foreach (var kvp in config.statBonuses)
                {
                    choice.StatBonuses[kvp.Key] = kvp.Value;
                }
            }

            return choice;
        }

        /// <summary>
        /// 获取所有路径配置数据
        /// </summary>
        /// <returns>路径配置列表</returns>
        public List<FatePathConfigData> GetAllPathConfigs()
        {
            if (_configFile?.paths == null)
            {
                return new List<FatePathConfigData>();
            }
            return new List<FatePathConfigData>(_configFile.paths);
        }

        /// <summary>
        /// 获取所有路径数据
        /// </summary>
        /// <returns>FatePathData列表</returns>
        public List<FatePathData> GetAllPaths()
        {
            var paths = new List<FatePathData>();
            if (_configFile?.paths == null) return paths;

            foreach (var config in _configFile.paths)
            {
                var pathData = ConvertToPathData(config);
                if (pathData != null)
                {
                    paths.Add(pathData);
                }
            }
            return paths;
        }

        /// <summary>
        /// 获取所有选择配置数据
        /// </summary>
        /// <returns>选择配置列表</returns>
        public List<FateChoiceConfigData> GetAllChoiceConfigs()
        {
            if (_configFile?.choices == null)
            {
                return new List<FateChoiceConfigData>();
            }
            return new List<FateChoiceConfigData>(_configFile.choices);
        }

        /// <summary>
        /// 获取所有选择数据
        /// </summary>
        /// <returns>FateChoice列表</returns>
        public List<FateChoice> GetAllChoices()
        {
            var choices = new List<FateChoice>();
            if (_configFile?.choices == null) return choices;

            foreach (var config in _configFile.choices)
            {
                var choice = ConvertToChoiceData(config);
                if (choice != null)
                {
                    choices.Add(choice);
                }
            }
            return choices;
        }

        /// <summary>
        /// 根据路径类型获取路径数据
        /// </summary>
        /// <param name="type">路径类型</param>
        /// <returns>FatePathData实例</returns>
        public FatePathData GetPathByType(FatePathType type)
        {
            if (_configFile?.paths == null) return null;

            foreach (var config in _configFile.paths)
            {
                if (Enum.TryParse<FatePathType>(config.type, true, out var pathType) && pathType == type)
                {
                    return ConvertToPathData(config);
                }
            }
            return null;
        }

        /// <summary>
        /// 根据ID获取选择数据
        /// </summary>
        /// <param name="id">选择ID</param>
        /// <returns>FateChoice实例</returns>
        public FateChoice GetChoiceById(string id)
        {
            if (_configFile?.choices == null) return null;

            foreach (var config in _configFile.choices)
            {
                if (config.id == id)
                {
                    return ConvertToChoiceData(config);
                }
            }
            return null;
        }

        /// <summary>
        /// 获取指定层级的所有可用选择
        /// </summary>
        /// <param name="tier">层级</param>
        /// <returns>可用的FateChoice列表</returns>
        public List<FateChoice> GetChoicesByTier(int tier)
        {
            var result = new List<FateChoice>();
            if (_configFile?.choices == null) return result;

            foreach (var config in _configFile.choices)
            {
                if (config.tierRequired <= tier)
                {
                    var choice = ConvertToChoiceData(config);
                    if (choice != null)
                    {
                        result.Add(choice);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取指定类型的所有选择
        /// </summary>
        /// <param name="type">选择类型</param>
        /// <returns>指定类型的FateChoice列表</returns>
        public List<FateChoice> GetChoicesByType(FateChoiceType type)
        {
            var result = new List<FateChoice>();
            if (_configFile?.choices == null) return result;

            foreach (var config in _configFile.choices)
            {
                if (Enum.TryParse<FateChoiceType>(config.choiceType, true, out var choiceType) && choiceType == type)
                {
                    var choice = ConvertToChoiceData(config);
                    if (choice != null)
                    {
                        result.Add(choice);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 获取配置版本
        /// </summary>
        public string GetVersion()
        {
            return _configFile?.version ?? "unknown";
        }

        /// <summary>
        /// 获取路径总数
        /// </summary>
        public int GetPathCount()
        {
            return _configFile?.paths?.Count ?? 0;
        }

        /// <summary>
        /// 获取选择总数
        /// </summary>
        public int GetChoiceCount()
        {
            return _configFile?.choices?.Count ?? 0;
        }
    }
}
