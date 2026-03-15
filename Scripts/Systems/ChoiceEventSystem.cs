using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 事件选择系统 - Roguelike风格的事件选择系统
    /// </summary>
    public partial class ChoiceEventSystem : BaseSystem {
        public static ChoiceEventSystem Instance { get; private set; }
        
        // 玩家数据
        private PlayerChoiceEventData _playerData = new PlayerChoiceEventData();
        
        // 当前活跃事件
        private ChoiceEventData _currentEvent = null;
        
        // 事件触发间隔
        private float _eventTriggerInterval = 180f; // 3分钟
        private float _timer = 0f;
        
        // 事件冷却
        private Dictionary<string, float> _eventCooldowns = new Dictionary<string, float>();
        private float _eventRepeatCooldown = 600f; // 10分钟内不重复同一事件
        
        // 信号
        [Signal] public delegate void EventTriggeredEventHandler(ChoiceEventData eventData);
        [Signal] public delegate void ChoiceMadeEventHandler(string eventId, string optionId, string resultText);
        [Signal] public delegate void RewardGrantedEventHandler(int gold, int exp);
        
        public override void _Ready() {
            Instance = this;
            AddToGroup("systems");
            GD.Print("[ChoiceEventSystem] 事件选择系统已初始化");
        }
        
        public override void _Process(double delta) {
            float deltaFloat = (float)delta;
            
            _timer += deltaFloat;
            
            // 更新冷却
            var cooldownsToRemove = new List<string>();
            foreach (var kvp in _eventCooldowns) {
                _eventCooldowns[kvp.Key] -= deltaFloat;
                if (_eventCooldowns[kvp.Key] <= 0) {
                    cooldownsToRemove.Add(kvp.Key);
                }
            }
            foreach (var key in cooldownsToRemove) {
                _eventCooldowns.Remove(key);
            }
            
            // 尝试触发随机事件
            if (_timer >= _eventTriggerInterval && _currentEvent == null) {
                _timer = 0f;
                TryTriggerRandomEvent();
            }
        }
        
        /// <summary>
        /// 尝试触发随机事件
        /// </summary>
        public bool TryTriggerRandomEvent() {
            if (_currentEvent != null) {
                return false;
            }
            
            // 获取玩家等级
            int playerLevel = 1;
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player != null) {
                var levelProperty = player.Get("Level");
                if (levelProperty != null) {
                    playerLevel = (int)levelProperty;
                }
            }
            
            // 获取玩家当前区域
            string region = "";
            var regionManager = RegionManager.Instance;
            if (regionManager != null) {
                region = regionManager.CurrentRegion;
            }
            
            // 获取随机事件
            var events = ChoiceEventDatabase.GetAllEvents();
            var availableEvents = events.Values.Where(e => 
                e.MinPlayerLevel <= playerLevel &&
                (!_eventCooldowns.ContainsKey(e.EventId) || _eventCooldowns[e.EventId] <= 0)
            ).ToList();
            
            if (availableEvents.Count == 0) {
                return false;
            }
            
            // 随机选择事件
            _currentEvent = availableEvents[(int)(GD.Rand() * availableEvents.Count)];
            
            // 设置冷却
            _eventCooldowns[_currentEvent.EventId] = _eventRepeatCooldown;
            
            // 触发信号
            EmitSignal(SignalName.EventTriggered, _currentEvent);
            
            GD.Print($"[ChoiceEventSystem] 触发事件: {_currentEvent.Title}");
            return true;
        }
        
        /// <summary>
        /// 玩家做出选择
        /// </summary>
        public void MakeChoice(string optionId) {
            if (_currentEvent == null) {
                GD.PrintErr("[ChoiceEventSystem] 当前没有活跃事件");
                return;
            }
            
            var selectedOption = _currentEvent.Options.FirstOrDefault(o => o.OptionId == optionId);
            if (selectedOption == null) {
                GD.PrintErr($"[ChoiceEventSystem] 无效的选择: {optionId}");
                return;
            }
            
            // 检查金币要求
            if (selectedOption.RequiresGold) {
                int playerGold = GetPlayerGold();
                if (playerGold < selectedOption.GoldCost) {
                    GD.Print($"[ChoiceEventSystem] 金币不足，需要 {selectedOption.GoldCost}，拥有 {playerGold}");
                    return;
                }
                // 扣除金币
                ModifyPlayerGold(-selectedOption.GoldCost);
            }
            
            // 记录选择
            if (!_playerData.ChosenOptions.ContainsKey(_currentEvent.EventId)) {
                _playerData.ChosenOptions[_currentEvent.EventId] = new List<string>();
            }
            _playerData.ChosenOptions[_currentEvent.EventId].Add(optionId);
            _playerData.TotalChoicesMade++;
            
            // 发放奖励
            int totalGold = 0;
            int totalExp = 0;
            
            foreach (var reward in selectedOption.Rewards) {
                if (GD.Randf() <= reward.Chance) {
                    switch (reward.Type) {
                        case "Gold":
                            totalGold += reward.Amount;
                            ModifyPlayerGold(reward.Amount);
                            break;
                        case "Exp":
                            totalExp += reward.Amount;
                            GrantExperience(reward.Amount);
                            break;
                        case "Item":
                            AddItemToInventory(reward.Id, reward.Amount);
                            break;
                        case "Buff":
                            ApplyBuff(reward.Id);
                            break;
                    }
                }
            }
            
            // 应用惩罚
            foreach (var penalty in selectedOption.Penalties) {
                switch (penalty.Type) {
                    case "Health":
                        ApplyDamage(penalty.Amount);
                        break;
                    case "Gold":
                        ModifyPlayerGold(-penalty.Amount);
                        break;
                    case "Debuff":
                        ApplyDebuff(penalty.Id);
                        break;
                }
            }
            
            // 更新统计
            _playerData.TotalGoldEarned += totalGold;
            _playerData.TotalExpEarned += totalExp;
            
            // 记录完成事件
            if (!_playerData.CompletedEventIds.Contains(_currentEvent.EventId)) {
                _playerData.CompletedEventIds.Add(_currentEvent.EventId);
            }
            
            // 触发信号
            EmitSignal(SignalName.ChoiceMade, _currentEvent.EventId, optionId, selectedOption.ResultText);
            if (totalGold > 0 || totalExp > 0) {
                EmitSignal(SignalName.RewardGranted, totalGold, totalExp);
            }
            
            GD.Print($"[ChoiceEventSystem] 玩家选择: {optionId}, 获得 {totalGold}金币, {totalExp}经验");
            
            // 清除当前事件
            _currentEvent = null;
        }
        
        /// <summary>
        /// 手动触发特定事件（用于测试或特定触发点）
        /// </summary>
        public bool TriggerSpecificEvent(string eventId) {
            if (_currentEvent != null) {
                return false;
            }
            
            var evt = ChoiceEventDatabase.GetEvent(eventId);
            if (evt == null) {
                return false;
            }
            
            _currentEvent = evt;
            EmitSignal(SignalName.EventTriggered, _currentEvent);
            return true;
        }
        
        /// <summary>
        /// 手动触发特定类别的事件
        /// </summary>
        public bool TriggerEventByCategory(string category) {
            if (_currentEvent != null) {
                return false;
            }
            
            int playerLevel = 1;
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player != null) {
                var levelProperty = player.Get("Level");
                if (levelProperty != null) {
                    playerLevel = (int)levelProperty;
                }
            }
            
            _currentEvent = ChoiceEventDatabase.GetRandomEventByCategory(category, playerLevel);
            if (_currentEvent == null) {
                return false;
            }
            
            EmitSignal(SignalName.EventTriggered, _currentEvent);
            return true;
        }
        
        /// <summary>
        /// 获取当前事件
        /// </summary>
        public ChoiceEventData GetCurrentEvent() {
            return _currentEvent;
        }
        
        /// <summary>
        /// 跳过当前事件
        /// </summary>
        public void SkipCurrentEvent() {
            _currentEvent = null;
            _timer = 0f;
        }
        
        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public PlayerChoiceEventData GetPlayerData() {
            return _playerData;
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, object> GetStatistics() {
            return new Dictionary<string, object> {
                { "total_choices", _playerData.TotalChoicesMade },
                { "total_events", _playerData.CompletedEventIds.Count },
                { "total_gold", _playerData.TotalGoldEarned },
                { "total_exp", _playerData.TotalExpEarned }
            };
        }
        
        // === 辅助方法 ===
        
        private int GetPlayerGold() {
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player != null) {
                var goldProperty = player.Get("Gold");
                if (goldProperty != null) {
                    return (int)goldProperty;
                }
            }
            return 0;
        }
        
        private void ModifyPlayerGold(int amount) {
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player != null) {
                player.Call("ModifyGold", amount);
            }
        }
        
        private void GrantExperience(int exp) {
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player != null) {
                player.Call("GainExperience", exp);
            }
        }
        
        private void AddItemToInventory(string itemId, int amount) {
            var inventoryManager = InventoryManager.Instance;
            if (inventoryManager != null) {
                inventoryManager.AddItem(itemId, amount);
            }
        }
        
        private void ApplyBuff(string buffId) {
            var buffSystem = BuffSystem.Instance;
            if (buffSystem != null) {
                // 应用祝福
                GD.Print($"[ChoiceEventSystem] 应用祝福: {buffId}");
            }
        }
        
        private void ApplyDebuff(string debuffId) {
            var buffSystem = BuffSystem.Instance;
            if (buffSystem != null) {
                // 应用debuff
                GD.Print($"[ChoiceEventSystem] 应用Debuff: {debuffId}");
            }
        }
        
        private void ApplyDamage(int damage) {
            var player = GetTree().GetFirstNodeInGroup("player");
            if (player != null) {
                player.Call("TakeDamage", damage);
            }
        }
        
        // === 数据持久化接口 ===
        
        public override Dictionary ExportSaveData()
        {
            return new Dictionary(GetSaveData());
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            LoadSaveData(new Dictionary<string, object>(data));
        }
        
        // === 存档支持 ===
        
        /// <summary>
        /// 导出保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary(GetSaveData());
        }
        
        /// <summary>
        /// 导入保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            LoadSaveData(new Dictionary<string, object>(data));
        }
        
        public Dictionary<string, object> GetSaveData() {
            return new Dictionary<string, object> {
                { "completed_events", _playerData.CompletedEventIds },
                { "chosen_options", _playerData.ChosenOptions.Select(kvp => 
                    new Dictionary<string, object> {
                        { "event_id", kvp.Key },
                        { "options", kvp.Value }
                    }).ToList() },
                { "total_choices", _playerData.TotalChoicesMade },
                { "total_gold", _playerData.TotalGoldEarned },
                { "total_exp", _playerData.TotalExpEarned }
            };
        }
        
        public void LoadSaveData(Dictionary<string, object> data) {
            if (data == null) return;
            
            if (data.ContainsKey("completed_events")) {
                _playerData.CompletedEventIds = ((Godot.Collections.Array)data["completed_events"])
                    .Select(v => v.ToString()).ToList();
            }
            
            if (data.ContainsKey("total_choices")) {
                _playerData.TotalChoicesMade = Convert.ToInt32(data["total_choices"]);
            }
            
            if (data.ContainsKey("total_gold")) {
                _playerData.TotalGoldEarned = Convert.ToInt32(data["total_gold"]);
            }
            
            if (data.ContainsKey("total_exp")) {
                _playerData.TotalExpEarned = Convert.ToInt32(data["total_exp"]);
            }
            
            if (data.ContainsKey("chosen_options")) {
                _playerData.ChosenOptions.Clear();
                foreach (Godot.Collections.Dictionary optionData in (Godot.Collections.Array)data["chosen_options"]) {
                    string eventId = optionData["event_id"].ToString();
                    var options = ((Godot.Collections.Array)optionData["options"])
                        .Select(v => v.ToString()).ToList();
                    _playerData.ChosenOptions[eventId] = options;
                }
            }
            
            GD.Print($"[ChoiceEventSystem] 存档加载完成: {_playerData.CompletedEventIds.Count} 事件完成");
        }
    }
}
