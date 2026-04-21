using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using ClawRPG.Scripts.Systems.Events;

namespace ClawRPG.Scripts.Database.Loaders
{
    /// <summary>
    /// 奖励物品配置数据（JSON反序列化用）
    /// </summary>
    public class RewardItemConfig
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        
        [JsonPropertyName("chance")]
        public float Chance { get; set; }
        
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
    
    /// <summary>
    /// 惩罚物品配置数据（JSON反序列化用）
    /// </summary>
    public class PenaltyItemConfig
    {
        [JsonPropertyName("type")]
        public string Type { get; set; }
        
        [JsonPropertyName("amount")]
        public int Amount { get; set; }
        
        [JsonPropertyName("chance")]
        public float Chance { get; set; }
        
        [JsonPropertyName("id")]
        public string Id { get; set; }
    }
    
    /// <summary>
    /// 选择选项配置数据（JSON反序列化用）
    /// </summary>
    public class ChoiceOptionConfig
    {
        [JsonPropertyName("optionId")]
        public string OptionId { get; set; }
        
        [JsonPropertyName("text")]
        public string Text { get; set; }
        
        [JsonPropertyName("resultText")]
        public string ResultText { get; set; }
        
        [JsonPropertyName("weight")]
        public float Weight { get; set; }
        
        [JsonPropertyName("requiresGold")]
        public bool RequiresGold { get; set; }
        
        [JsonPropertyName("goldCost")]
        public int GoldCost { get; set; }
        
        [JsonPropertyName("rewards")]
        public List<RewardItemConfig> Rewards { get; set; }
        
        [JsonPropertyName("penalties")]
        public List<PenaltyItemConfig> Penalties { get; set; }
    }
    
    /// <summary>
    /// 事件配置数据（JSON反序列化用）
    /// </summary>
    public class ChoiceEventConfig
    {
        [JsonPropertyName("eventId")]
        public string EventId { get; set; }
        
        [JsonPropertyName("title")]
        public string Title { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("category")]
        public string Category { get; set; }
        
        [JsonPropertyName("minPlayerLevel")]
        public int MinPlayerLevel { get; set; }
        
        [JsonPropertyName("requiredRegion")]
        public string RequiredRegion { get; set; }
        
        [JsonPropertyName("options")]
        public List<ChoiceOptionConfig> Options { get; set; }
    }
    
    /// <summary>
    /// 事件配置文件结构
    /// </summary>
    public class EventsConfigFile
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        
        [JsonPropertyName("events")]
        public List<ChoiceEventConfig> Events { get; set; }
    }

    /// <summary>
    /// 事件配置加载器 - 负责从JSON文件加载事件数据
    /// </summary>
    public class EventConfigLoader
    {
        private static EventConfigLoader _instance;
        private EventsConfigFile _configFile;
        private bool _isLoaded = false;
        private string _lastError = string.Empty;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static EventConfigLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EventConfigLoader();
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
        /// 加载事件配置文件
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
                    _lastError = $"事件配置文件不存在: {configPath}";
                    GD.PrintErr($"[EventConfigLoader] {_lastError}");
                    return false;
                }

                string json = System.IO.File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                _configFile = JsonSerializer.Deserialize<EventsConfigFile>(json, options);
                
                if (_configFile == null || _configFile.Events == null)
                {
                    _lastError = "事件配置文件格式错误：无法解析数据";
                    GD.PrintErr($"[EventConfigLoader] {_lastError}");
                    return false;
                }

                _isLoaded = true;
                GD.Print($"[EventConfigLoader] 成功加载 {_configFile.Events.Count} 个事件配置");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"事件配置加载失败: {ex.Message}";
                GD.PrintErr($"[EventConfigLoader] {_lastError}");
                return false;
            }
        }

        /// <summary>
        /// 从配置数据转换为ChoiceEventRewards
        /// </summary>
        /// <param name="config">事件配置数据</param>
        /// <returns>ChoiceEventRewards实例</returns>
        public ChoiceEventRewards ConvertToChoiceEventData(ChoiceEventConfig config)
        {
            if (config == null) return null;

            var eventData = new ChoiceEventRewards
            {
                EventId = config.EventId,
                Title = config.Title,
                Description = config.Description,
                Category = config.Category,
                MinPlayerLevel = config.MinPlayerLevel,
                RequiredRegion = config.RequiredRegion ?? string.Empty,
                Options = new List<ChoiceOption>()
            };

            if (config.Options != null)
            {
                foreach (var optionConfig in config.Options)
                {
                    var option = new ChoiceOption
                    {
                        OptionId = optionConfig.OptionId,
                        Text = optionConfig.Text,
                        ResultText = optionConfig.ResultText ?? string.Empty,
                        Weight = optionConfig.Weight,
                        RequiresGold = optionConfig.RequiresGold,
                        GoldCost = optionConfig.GoldCost,
                        Rewards = new List<RewardItem>(),
                        Penalties = new List<PenaltyItem>()
                    };

                    if (optionConfig.Rewards != null)
                    {
                        foreach (var rewardConfig in optionConfig.Rewards)
                        {
                            option.Rewards.Add(new RewardItem
                            {
                                Type = rewardConfig.Type,
                                Amount = rewardConfig.Amount,
                                Chance = rewardConfig.Chance,
                                Id = rewardConfig.Id ?? string.Empty
                            });
                        }
                    }

                    if (optionConfig.Penalties != null)
                    {
                        foreach (var penaltyConfig in optionConfig.Penalties)
                        {
                            option.Penalties.Add(new PenaltyItem
                            {
                                Type = penaltyConfig.Type,
                                Amount = penaltyConfig.Amount,
                                Chance = penaltyConfig.Chance,
                                Id = penaltyConfig.Id ?? string.Empty
                            });
                        }
                    }

                    eventData.Options.Add(option);
                }
            }

            return eventData;
        }

        /// <summary>
        /// 获取所有事件配置数据
        /// </summary>
        /// <returns>事件配置列表</returns>
        public List<ChoiceEventConfig> GetAllEventConfigs()
        {
            if (_configFile?.Events == null)
            {
                return new List<ChoiceEventConfig>();
            }
            return new List<ChoiceEventConfig>(_configFile.Events);
        }

        /// <summary>
        /// 获取所有事件ChoiceEventRewards列表
        /// </summary>
        /// <returns>ChoiceEventRewards列表</returns>
        public List<ChoiceEventRewards> GetAllChoiceEventData()
        {
            var eventDataList = new List<ChoiceEventRewards>();
            if (_configFile?.Events == null) return eventDataList;

            foreach (var config in _configFile.Events)
            {
                var eventData = ConvertToChoiceEventData(config);
                if (eventData != null)
                {
                    eventDataList.Add(eventData);
                }
            }
            return eventDataList;
        }

        /// <summary>
        /// 根据ID获取事件配置
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>事件配置数据</returns>
        public ChoiceEventConfig GetEventConfigById(string eventId)
        {
            if (_configFile?.Events == null) return null;
            return _configFile.Events.Find(e => e.EventId == eventId);
        }

        /// <summary>
        /// 根据ID获取ChoiceEventRewards
        /// </summary>
        /// <param name="eventId">事件ID</param>
        /// <returns>ChoiceEventRewards实例</returns>
        public ChoiceEventRewards GetChoiceEventDataById(string eventId)
        {
            var config = GetEventConfigById(eventId);
            return config != null ? ConvertToChoiceEventData(config) : null;
        }

        /// <summary>
        /// 获取配置版本
        /// </summary>
        public string GetVersion()
        {
            return _configFile?.Version ?? "unknown";
        }

        /// <summary>
        /// 获取事件总数
        /// </summary>
        public int GetEventCount()
        {
            return _configFile?.Events?.Count ?? 0;
        }
    }
}
