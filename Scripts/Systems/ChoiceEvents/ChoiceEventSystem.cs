using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ChoiceEvents
{
    /// <summary>
    /// Choice event system - handles random choice events for roguelike gameplay
    /// </summary>
    public class ChoiceEventSystem : BaseSystem
    {
        private static ChoiceEventSystem _instance;
        public static ChoiceEventSystem Instance
        {
            get { return _instance; }
        }
        
        private ChoiceEventDatabase _database;
        private PlayerChoiceData _playerData;
        private ActiveChoiceEvent _currentEvent;
        private Random _random;
        
        // Signals (Godot 4 compatible)
        [Signal]
        public delegate void EventStartedDelegate(ChoiceEventType type, string eventId);
        [Signal]
        public delegate void EventEndedDelegate(ChoiceEventType type, string eventId);
        [Signal]
        public delegate void OptionSelectedDelegate(ChoiceOption option);
        [Signal]
        public delegate void RewardGrantedDelegate(int gold, int exp, List<string> items);
        
        public ChoiceEventSystem()
        {
            _instance = this;
            _random = new Random();
        }
        
        public override void _Ready()
        {
            _database = ChoiceEventDatabase.Instance;
            _database.Initialize();
            _playerData = new PlayerChoiceData();
            GD.Print("ChoiceEventSystem initialized");
        }
        
        /// <summary>
        /// Start a choice event of specified type
        /// </summary>
        public void StartChoiceEvent(ChoiceEventType eventType, int optionCount = 3)
        {
            if (_currentEvent != null && _currentEvent.IsActive)
            {
                GD.Print("Choice event already active");
                return;
            }
            
            var player = GetTree().Root.GetNode<Player>("Main/Player");
            int playerLevel = player != null ? player.Level : 1;
            
            _currentEvent = new ActiveChoiceEvent
            {
                EventId = Guid.NewGuid().ToString(),
                EventType = eventType,
                IsActive = true,
                StartTime = DateTime.Now,
                RequiredOptionCount = optionCount
            };
            
            // Generate options based on event type
            switch (eventType)
            {
                case ChoiceEventType.Upgrade:
                    _currentEvent.Title = "等级提升!";
                    _currentEvent.Description = "选择一项永久升级";
                    _currentEvent.Options = _database.GetUpgradeChoices(optionCount, playerLevel);
                    break;
                    
                case ChoiceEventType.Treasure:
                    _currentEvent.Title = "发现宝藏!";
                    _currentEvent.Description = "选择一个宝藏";
                    _currentEvent.Options = _database.GetTreasureChoices(optionCount);
                    break;
                    
                case ChoiceEventType.Blessing:
                    _currentEvent.Title = "神圣祝福!";
                    _currentEvent.Description = "选择一项祝福";
                    _currentEvent.Options = _database.GetBlessingChoices(optionCount);
                    break;
                    
                case ChoiceEventType.Curse:
                    _currentEvent.Title = "遭遇诅咒!";
                    _currentEvent.Description = "选择承受的诅咒";
                    _currentEvent.Options = _database.GetCurseChoices(optionCount);
                    break;
                    
                case ChoiceEventType.Merchant:
                    _currentEvent.Title = "神秘商人!";
                    _currentEvent.Description = "选择交易选项";
                    _currentEvent.Options = _database.GetMerchantChoices(optionCount);
                    break;
                    
                case ChoiceEventType.Challenge:
                    _currentEvent.Title = "接受挑战!";
                    _currentEvent.Description = "选择挑战难度";
                    _currentEvent.Options = _database.GetChallengeChoices(optionCount);
                    break;
                    
                case ChoiceEventType.Rest:
                    _currentEvent.Title = "休息恢复!";
                    _currentEvent.Description = "选择恢复方式";
                    _currentEvent.Options = _database.GetRestChoices(optionCount);
                    break;
                    
                case ChoiceEventType.Mystery:
                    _currentEvent.Title = "神秘事件!";
                    _currentEvent.Description = "选择你的命运";
                    _currentEvent.Options = _database.GetMysteryChoices(optionCount);
                    break;
            }
            
            // Update player data
            _playerData.TotalEvents++;
            if (!_playerData.EventCounts.ContainsKey(eventType))
                _playerData.EventCounts[eventType] = 0;
            _playerData.EventCounts[eventType]++;
            
            EventStarted?.Invoke(eventType, _currentEvent.Title);
            GD.Print($"Choice event started: {eventType}");
        }
        
        /// <summary>
        /// Select an option from current event
        /// </summary>
        public bool SelectOption(int optionIndex)
        {
            if (_currentEvent == null || !_currentEvent.IsActive)
            {
                GD.Print("No active choice event");
                return false;
            }
            
            if (optionIndex < 0 || optionIndex >= _currentEvent.Options.Count)
            {
                GD.Print("Invalid option index");
                return false;
            }
            
            var selectedOption = _currentEvent.Options[optionIndex];
            
            // Apply rewards
            ApplyOptionRewards(selectedOption);
            
            // Update player data
            _playerData.TotalChoices++;
            if (!_playerData.OptionCounts.ContainsKey(selectedOption.Id))
                _playerData.OptionCounts[selectedOption.Id] = 0;
            _playerData.OptionCounts[selectedOption.Id]++;
            
            if (!_playerData.RaritySelections.ContainsKey(selectedOption.Rarity))
                _playerData.RaritySelections[selectedOption.Rarity] = 0;
            _playerData.RaritySelections[selectedOption.Rarity]++;
            
            _playerData.ChosenOptionHistory.Add(selectedOption.Id);
            
            // End event
            _currentEvent.IsActive = false;
            var eventType = _currentEvent.EventType;
            var eventTitle = selectedOption.Name;
            
            EventEnded?.Invoke(eventType, selectedOption.Name);
            OptionSelected?.Invoke(selectedOption);
            
            GD.Print($"Option selected: {selectedOption.Name}");
            
            return true;
        }
        
        /// <summary>
        /// Apply rewards from selected option
        /// </summary>
        private void ApplyOptionRewards(ChoiceOption option)
        {
            var player = GetTree().Root.GetNode<Player>("Main/Player");
            if (player == null) return;
            
            // Grant gold
            if (option.GoldReward > 0)
            {
                player.Gold += option.GoldReward;
                GD.Print($"Granted {option.GoldReward} gold");
            }
            
            // Grant experience
            if (option.ExpReward > 0)
            {
                // Experience would be handled by a separate system
                GD.Print($"Granted {option.ExpReward} experience");
            }
            
            // Apply permanent stat bonuses
            if (option.IsPermanent)
            {
                if (option.AttackBonus > 0)
                    player.Attack += option.AttackBonus;
                if (option.DefenseBonus > 0)
                    player.Defense += option.DefenseBonus;
                if (option.HealthBonus > 0)
                    player.MaxHealth += option.HealthBonus;
                if (option.SpeedBonus > 0)
                    player.Speed += option.SpeedBonus;
                if (option.CritRateBonus > 0)
                    player.CritRate += option.CritRateBonus;
                if (option.CritDamageBonus > 0)
                    player.CritDamage += option.CritDamageBonus;
                
                GD.Print($"Applied permanent bonuses: Atk+{option.AttackBonus} Def+{option.DefenseBonus}");
            }
            
            RewardGranted?.Invoke(option.GoldReward, option.ExpReward, option.ItemRewards);
        }
        
        /// <summary>
        /// Get current active event
        /// </summary>
        public ActiveChoiceEvent GetCurrentEvent()
        {
            return _currentEvent;
        }
        
        /// <summary>
        /// Check if an event is currently active
        /// </summary>
        public bool IsEventActive()
        {
            return _currentEvent != null && _currentEvent.IsActive;
        }
        
        /// <summary>
        /// Get player statistics
        /// </summary>
        public PlayerChoiceData GetStatistics()
        {
            return _playerData;
        }
        
        /// <summary>
        /// Trigger upgrade event (e.g., on level up)
        /// </summary>
        public void TriggerUpgradeEvent()
        {
            StartChoiceEvent(ChoiceEventType.Upgrade, 3);
        }
        
        /// <summary>
        /// Trigger treasure event (e.g., on finding treasure)
        /// </summary>
        public void TriggerTreasureEvent()
        {
            StartChoiceEvent(ChoiceEventType.Treasure, 3);
        }
        
        /// <summary>
        /// Trigger blessing event (e.g., after boss defeat)
        /// </summary>
        public void TriggerBlessingEvent()
        {
            StartChoiceEvent(ChoiceEventType.Blessing, 2);
        }
        
        /// <summary>
        /// Trigger rest event (e.g., in rest room)
        /// </summary>
        public void TriggerRestEvent()
        {
            StartChoiceEvent(ChoiceEventType.Rest, 2);
        }
        
        /// <summary>
        /// Trigger mystery event (random)
        /// </summary>
        public void TriggerMysteryEvent()
        {
            StartChoiceEvent(ChoiceEventType.Mystery, 3);
        }
        
        /// <summary>
        /// Get save data
        /// </summary>
        public Dictionary<string, object> GetSaveData()
        {
            var data = new Dictionary<string, object>();
            
            data["totalEvents"] = _playerData.TotalEvents;
            data["totalChoices"] = _playerData.TotalChoices;
            
            // Serialize event counts
            var eventCounts = new Dictionary<string, int>();
            foreach (var kvp in _playerData.EventCounts)
            {
                eventCounts[kvp.Key.ToString()] = kvp.Value;
            }
            data["eventCounts"] = eventCounts;
            
            // Serialize option counts
            data["optionCounts"] = _playerData.OptionCounts;
            
            // Serialize rarity selections
            var raritySelections = new Dictionary<string, int>();
            foreach (var kvp in _playerData.RaritySelections)
            {
                raritySelections[kvp.Key.ToString()] = kvp.Value;
            }
            data["raritySelections"] = raritySelections;
            
            data["chosenHistory"] = _playerData.ChosenOptionHistory;
            
            return data;
        }
        
        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("totalEvents"))
                _playerData.TotalEvents = Convert.ToInt32(data["totalEvents"]);
            if (data.ContainsKey("totalChoices"))
                _playerData.TotalChoices = Convert.ToInt32(data["totalChoices"]);
            
            // Deserialize event counts
            if (data.ContainsKey("eventCounts"))
            {
                var eventCounts = (Dictionary<string, object>)data["eventCounts"];
                _playerData.EventCounts.Clear();
                foreach (var kvp in eventCounts)
                {
                    if (Enum.TryParse<ChoiceEventType>(kvp.Key, out var eventType))
                    {
                        _playerData.EventCounts[eventType] = Convert.ToInt32(kvp.Value);
                    }
                }
            }
            
            // Deserialize option counts
            if (data.ContainsKey("optionCounts"))
            {
                _playerData.OptionCounts = (Dictionary<string, int>)data["optionCounts"];
            }
            
            // Deserialize rarity selections
            if (data.ContainsKey("raritySelections"))
            {
                var raritySelections = (Dictionary<string, object>)data["raritySelections"];
                _playerData.RaritySelections.Clear();
                foreach (var kvp in raritySelections)
                {
                    if (Enum.TryParse<ChoiceEventRarity>(kvp.Key, out var rarity))
                    {
                        _playerData.RaritySelections[rarity] = Convert.ToInt32(kvp.Value);
                    }
                }
            }
            
            // Deserialize history
            if (data.ContainsKey("chosenHistory"))
            {
                _playerData.ChosenOptionHistory = new List<string>((IEnumerable<string>)data["chosenHistory"]);
            }
            
            GD.Print("ChoiceEventSystem save data loaded");
        }

        #region BaseSystem 持久化接口

        public override Dictionary<string, object> ExportSaveData()
        {
            return GetSaveData();
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            LoadSaveData(data);
        }

        #endregion
    }
}
