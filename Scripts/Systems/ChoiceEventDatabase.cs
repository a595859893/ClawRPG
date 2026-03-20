using Godot;
using System;
using System.Collections.Generic;
using System.IO;
using ClawRPG.Scripts.Database;
using ClawRPG.Scripts.Database.Loaders;
using ClawRPG.Scripts.Systems.Events;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 事件选择数据库 - 管理所有随机事件
    /// </summary>
    public class ChoiceEventDatabase : DatabaseBase
    {
        private static ChoiceEventDatabase _instance;
        public static ChoiceEventDatabase Instance => _instance ??= new ChoiceEventDatabase();

        private Dictionary<string, ChoiceEventRewards> _events = new Dictionary<string, ChoiceEventRewards>();

        // 玩家选择记录（按玩家ID索引）
        private Dictionary<string, PlayerEventRecord> _playerRecords = new Dictionary<string, PlayerEventRecord>();

        // 事件冷却数据（按玩家ID索引）
        private Dictionary<string, Dictionary<string, DateTime>> _eventCooldowns = new Dictionary<string, Dictionary<string, DateTime>>();

        public override object Instance => Instance;

        public override void Initialize()
        {
            InitializeEventsFromConfig();
        }

        public override bool ValidateData()
        {
            return _events != null && _events.Count > 0;
        }

        #region 玩家数据管理

        /// <summary>
        /// 记录玩家选择
        /// </summary>
        public void RecordPlayerChoice(string playerId, string eventId, string optionId)
        {
            if (!_playerRecords.ContainsKey(playerId))
            {
                _playerRecords[playerId] = new PlayerEventRecord { PlayerId = playerId };
            }

            var record = _playerRecords[playerId];
            record.ChoicesMade++;

            if (!record.EventChoiceHistory.ContainsKey(eventId))
            {
                record.EventChoiceHistory[eventId] = new List<string>();
            }
            record.EventChoiceHistory[eventId].Add(optionId);
        }

        /// <summary>
        /// 解锁事件
        /// </summary>
        public void UnlockEvent(string playerId, string eventId)
        {
            if (!_playerRecords.ContainsKey(playerId))
            {
                _playerRecords[playerId] = new PlayerEventRecord { PlayerId = playerId };
            }

            if (!_playerRecords[playerId].UnlockedEvents.Contains(eventId))
            {
                _playerRecords[playerId].UnlockedEvents.Add(eventId);
            }
        }

        /// <summary>
        /// 检查事件是否解锁
        /// </summary>
        public bool IsEventUnlocked(string playerId, string eventId)
        {
            if (_playerRecords.TryGetValue(playerId, out var record))
            {
                return record.UnlockedEvents.Contains(eventId);
            }
            return false;
        }

        /// <summary>
        /// 设置事件冷却
        /// </summary>
        public void SetEventCooldown(string playerId, string eventId, TimeSpan cooldown)
        {
            if (!_eventCooldowns.ContainsKey(playerId))
            {
                _eventCooldowns[playerId] = new Dictionary<string, DateTime>();
            }

            _eventCooldowns[playerId][eventId] = DateTime.Now + cooldown;
        }

        /// <summary>
        /// 检查事件是否在冷却中
        /// </summary>
        public bool IsEventOnCooldown(string playerId, string eventId)
        {
            if (_eventCooldowns.TryGetValue(playerId, out var cooldowns))
            {
                if (cooldowns.TryGetValue(eventId, out var cooldownEnd))
                {
                    return DateTime.Now < cooldownEnd;
                }
            }
            return false;
        }

        /// <summary>
        /// 获取玩家事件记录
        /// </summary>
        public PlayerEventRecord GetPlayerRecord(string playerId)
        {
            if (_playerRecords.TryGetValue(playerId, out var record))
            {
                return record;
            }
            return null;
        }

        #endregion

        #region 持久化

        protected override void OnExportSaveData(Godot.Collections.Dictionary saveData)
        {
            // 导出玩家选择记录
            var playerRecordsData = new Godot.Collections.Dictionary();
            foreach (var kvp in _playerRecords)
            {
                var recordDict = new Godot.Collections.Dictionary
                {
                    ["playerId"] = kvp.Value.PlayerId,
                    ["choicesMade"] = kvp.Value.ChoicesMade,
                    ["eventChoiceHistory"] = new Godot.Collections.Dictionary()
                };

                var historyDict = (Godot.Collections.Dictionary)recordDict["eventChoiceHistory"];
                foreach (var historyKvp in kvp.Value.EventChoiceHistory)
                {
                    historyDict[historyKvp.Key] = new Godot.Collections.Array(historyKvp.Value);
                }

                recordDict["unlockedEvents"] = new Godot.Collections.Array(kvp.Value.UnlockedEvents);
                playerRecordsData[kvp.Key] = recordDict;
            }
            saveData["playerRecords"] = playerRecordsData;

            // 导出事件冷却数据
            var cooldownsData = new Godot.Collections.Dictionary();
            foreach (var playerKvp in _eventCooldowns)
            {
                var playerCooldowns = new Godot.Collections.Dictionary();
                foreach (var cooldownKvp in playerKvp.Value)
                {
                    playerCooldowns[cooldownKvp.Key] = cooldownKvp.Value.Ticks;
                }
                cooldownsData[playerKvp.Key] = playerCooldowns;
            }
            saveData["eventCooldowns"] = cooldownsData;
        }

        protected override void OnImportSaveData(Godot.Collections.Dictionary saveData)
        {
            // 导入玩家选择记录
            if (saveData.TryGetValue("playerRecords", out var recordsObj) && recordsObj is Godot.Collections.Dictionary recordsData)
            {
                foreach (var playerKvp in recordsData)
                {
                    if (playerKvp.Value is Godot.Collections.Dictionary recordDict)
                    {
                        var playerId = playerKvp.Key.ToString();
                        var record = new PlayerEventRecord
                        {
                            PlayerId = playerId
                        };

                        if (recordDict.TryGetValue("choicesMade", out var choicesMade))
                            record.ChoicesMade = Convert.ToInt32(choicesMade);

                        if (recordDict.TryGetValue("unlockedEvents", out var unlockedObj) && unlockedObj is Godot.Collections.Array unlockedArray)
                        {
                            foreach (var item in unlockedArray)
                                record.UnlockedEvents.Add(item.ToString());
                        }

                        if (recordDict.TryGetValue("eventChoiceHistory", out var historyObj) && historyObj is Godot.Collections.Dictionary historyDict)
                        {
                            foreach (var historyKvp in historyDict)
                            {
                                if (historyKvp.Value is Godot.Collections.Array choiceArray)
                                {
                                    var choices = new List<string>();
                                    foreach (var choice in choiceArray)
                                        choices.Add(choice.ToString());
                                    record.EventChoiceHistory[historyKvp.Key.ToString()] = choices;
                                }
                            }
                        }

                        _playerRecords[playerId] = record;
                    }
                }
            }

            // 导入事件冷却数据
            if (saveData.TryGetValue("eventCooldowns", out var cooldownsObj) && cooldownsObj is Godot.Collections.Dictionary cooldownsData)
            {
                foreach (var playerKvp in cooldownsData)
                {
                    if (playerKvp.Value is Godot.Collections.Dictionary playerCooldowns)
                    {
                        var playerId = playerKvp.Key.ToString();
                        var cooldownDict = new Dictionary<string, DateTime>();

                        foreach (var cooldownKvp in playerCooldowns)
                        {
                            if (Convert.ToInt64(cooldownKvp.Value) > DateTime.Now.Ticks)
                            {
                                cooldownDict[cooldownKvp.Key.ToString()] = new DateTime(Convert.ToInt64(cooldownKvp.Value));
                            }
                        }

                        _eventCooldowns[playerId] = cooldownDict;
                    }
                }
            }
        }

        #endregion

        private void InitializeEventsFromConfig()
        {
            // 从JSON配置文件加载事件数据
            var loader = EventConfigLoader.Instance;
            
            // 构建配置文件路径（相对于项目根目录）
            string configPath = Path.Combine("Resources", "Config", "events_config.json");
            
            if (!loader.Load(configPath))
            {
                GD.PrintErr($"[ChoiceEventDatabase] 加载事件配置失败: {loader.LastError}");
                return;
            }
            
            // 转换并注册所有事件
            var events = loader.GetAllChoiceEventData();
            foreach (var evt in events)
            {
                AddEvent(evt);
            }
            
            GD.Print($"[ChoiceEventDatabase] 成功从配置文件加载 {events.Count} 个事件");
        }

        private void AddEvent(ChoiceEventRewards eventData)
        {
            _events[eventData.EventId] = eventData;
        }

        /// <summary>
        /// 获取所有事件
        /// </summary>
        public Dictionary<string, ChoiceEventRewards> GetAllEvents()
        {
            return new Dictionary<string, ChoiceEventRewards>(_events);
        }

        /// <summary>
        /// 根据ID获取事件
        /// </summary>
        public ChoiceEventRewards GetEvent(string eventId)
        {
            if (_events.ContainsKey(eventId))
            {
                return _events[eventId];
            }
            return null;
        }

        /// <summary>
        /// 获取随机事件（基于玩家等级和区域）
        /// </summary>
        public ChoiceEventRewards GetRandomEvent(int playerLevel, string region = "")
        {
            var validEvents = new List<ChoiceEventRewards>();

            foreach (var evt in _events.Values)
            {
                if (evt.MinPlayerLevel <= playerLevel)
                {
                    if (string.IsNullOrEmpty(evt.RequiredRegion) || evt.RequiredRegion == region)
                    {
                        validEvents.Add(evt);
                    }
                }
            }

            if (validEvents.Count == 0) return null;

            // 加权随机选择
            return GetWeightedRandomEvent(validEvents);
        }

        /// <summary>
        /// 根据类别获取随机事件
        /// </summary>
        public ChoiceEventRewards GetRandomEventByCategory(string category, int playerLevel)
        {
            var validEvents = new List<ChoiceEventRewards>();

            foreach (var evt in _events.Values)
            {
                if (evt.Category == category && evt.MinPlayerLevel <= playerLevel)
                {
                    validEvents.Add(evt);
                }
            }

            if (validEvents.Count == 0) return null;

            return GetWeightedRandomEvent(validEvents);
        }

        /// <summary>
        /// 加权随机选择
        /// </summary>
        private ChoiceEventRewards GetWeightedRandomEvent(List<ChoiceEventRewards> events)
        {
            if (events.Count == 0) return null;
            if (events.Count == 1) return events[0];

            float totalWeight = 0;
            foreach (var evt in events)
            {
                totalWeight += 1.0f; // 简单等权重
            }

            float randomValue = (float)GD.RandDouble() * totalWeight;
            float currentWeight = 0;

            foreach (var evt in events)
            {
                currentWeight += 1.0f;
                if (randomValue <= currentWeight)
                {
                    return evt;
                }
            }

            return events[0];
        }
    }
}
