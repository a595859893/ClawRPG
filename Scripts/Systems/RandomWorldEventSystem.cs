// RandomWorldEventSystem.cs - 随机世界事件系统管理器
using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems {
    public partial class RandomWorldEventSystem : Node {
        public static RandomWorldEventSystem Instance { get; private set; }

        // 事件数据库
        private Dictionary<string, WorldEventData> _eventDatabase = new Dictionary<string, WorldEventData>();
        
        // 当前活跃事件
        private Dictionary<string, WorldEventConfig> _activeEvents = new Dictionary<string, WorldEventConfig>();
        
        // 玩家事件数据
        private PlayerWorldEventData _playerData = new PlayerWorldEventData();
        
        // 计时器
        private float _checkTimer = 0f;
        private float _checkInterval = 30f; // 每30秒检查一次
        
        // 信号
        [Signal] public delegate void EventTriggeredEventHandler(WorldEventConfig config);
        [Signal] public delegate void EventExpiredEventHandler(string eventId);
        [Signal] public delegate void EventRewardClaimedEventHandler(string eventId, int gold, int exp);

        public override void _Ready() {
            Instance = this;
            InitializeEventDatabase();
            AddToGroup("RandomWorldEventSystem");
            GD.Print("[RandomWorldEventSystem] 随机世界事件系统已初始化");
        }

        public override void _Process(double delta) {
            _checkTimer += (float)delta;
            
            if (_checkTimer >= _checkInterval) {
                _checkTimer = 0f;
                TryTriggerRandomEvent();
                UpdateActiveEvents();
            }
        }

        /// <summary>
        /// 初始化事件数据库
        /// </summary>
        private void InitializeEventDatabase() {
            // 资源刷新事件
            AddEvent(new WorldEventData {
                EventId = "resource_herb",
                EventName = "草药丰产",
                Description = "附近的草药突然成熟了！",
                EventType = WorldEventType.ResourceSpawn,
                Rarity = EventRarity.Common,
                MinPlayerLevel = 1,
                TriggerChance = 0.15f,
                CooldownMinutes = 30,
                DurationSeconds = 180,
                ExpReward = 20,
                ItemRewards = new List<string> { "herb_green", "herb_blue" }
            });

            AddEvent(new WorldEventData {
                EventId = "resource_ore",
                EventName = "矿石涌现",
                Description = "地面裂开，露出珍贵的矿石！",
                EventType = WorldEventType.ResourceSpawn,
                Rarity = EventRarity.Uncommon,
                MinPlayerLevel = 5,
                TriggerChance = 0.12f,
                CooldownMinutes = 45,
                DurationSeconds = 240,
                ExpReward = 40,
                ItemRewards = new List<string> { "ore_iron", "ore_silver" }
            });

            // 敌人入侵事件
            AddEvent(new WorldEventData {
                EventId = "enemy_goblin",
                EventName = "哥布林入侵",
                Description = "一群哥布林正在靠近！",
                EventType = WorldEventType.EnemyInvasion,
                Rarity = EventRarity.Common,
                MinPlayerLevel = 3,
                TriggerChance = 0.1f,
                CooldownMinutes = 60,
                DurationSeconds = 300,
                GoldReward = 50,
                ExpReward = 80,
                ItemRewards = new List<string> { "goblin_ear", "gold_coin" }
            });

            AddEvent(new WorldEventData {
                EventId = "enemy_skeleton",
                EventName = "骷髅大军",
                Description = "骷髅大军从墓地涌出！",
                EventType = WorldEventType.EnemyInvasion,
                Rarity = EventRarity.Rare,
                MinPlayerLevel = 10,
                TriggerChance = 0.06f,
                CooldownMinutes = 120,
                DurationSeconds = 420,
                GoldReward = 200,
                ExpReward = 300,
                ItemRewards = new List<string> { "bone", "skull", "undead_essence" }
            });

            // 宝箱事件
            AddEvent(new WorldEventData {
                EventId = "treasure_common",
                EventName = "神秘宝箱",
                Description = "地面上突然出现了一个宝箱！",
                EventType = WorldEventType.TreasureChest,
                Rarity = EventRarity.Common,
                MinPlayerLevel = 1,
                TriggerChance = 0.12f,
                CooldownMinutes = 20,
                DurationSeconds = 120,
                GoldReward = 30,
                ExpReward = 10,
                ItemRewards = new List<string> { "gold_coin", "health_potion" }
            });

            AddEvent(new WorldEventData {
                EventId = "treasure_rare",
                EventName = "稀有宝箱",
                Description = "发现了一个散发金光的稀有宝箱！",
                EventType = WorldEventType.TreasureChest,
                Rarity = EventRarity.Rare,
                MinPlayerLevel = 8,
                TriggerChance = 0.05f,
                CooldownMinutes = 90,
                DurationSeconds = 180,
                GoldReward = 150,
                ExpReward = 100,
                ItemRewards = new List<string> { "rare_gem", "epic_armor" }
            });

            // 隐藏宝箱
            AddEvent(new WorldEventData {
                EventId = "treasure_hidden",
                EventName = "隐藏宝藏",
                Description = "你发现了隐藏的宝藏！",
                EventType = WorldEventType.HiddenChest,
                Rarity = EventRarity.Epic,
                MinPlayerLevel = 15,
                TriggerChance = 0.03f,
                CooldownMinutes = 180,
                DurationSeconds = 300,
                GoldReward = 500,
                ExpReward = 500,
                ItemRewards = new List<string> { "epic_weapon", "legendary_accessory" }
            });

            // 商人到来
            AddEvent(new WorldEventData {
                EventId = "merchant_weapon",
                EventName = "武器商人",
                Description = "一位神秘的武器商人出现了！",
                EventType = WorldEventType.MerchantArrival,
                Rarity = EventRarity.Uncommon,
                MinPlayerLevel = 5,
                TriggerChance = 0.08f,
                CooldownMinutes = 60,
                DurationSeconds = 600,
                ExpReward = 30
            });

            AddEvent(new WorldEventData {
                EventId = "merchant_magic",
                EventName = "魔法商人",
                Description = "一位施法者带来了稀有物品！",
                EventType = WorldEventType.MerchantArrival,
                Rarity = EventRarity.Rare,
                MinPlayerLevel = 10,
                TriggerChance = 0.04f,
                CooldownMinutes = 120,
                DurationSeconds = 480,
                ExpReward = 80
            });

            // 天气变化
            AddEvent(new WorldEventData {
                EventId = "weather_blessing",
                EventName = "元素祝福",
                Description = "元素之力眷顾了你！",
                EventType = WorldEventType.WeatherChange,
                Rarity = EventRarity.Rare,
                MinPlayerLevel = 8,
                TriggerChance = 0.05f,
                CooldownMinutes = 90,
                DurationSeconds = 600,
                ExpReward = 200,
                ItemRewards = new List<string> { "element_orb" }
            });

            // 幸运时刻
            AddEvent(new WorldEventData {
                EventId = "lucky_moment",
                EventName = "幸运时刻",
                Description = "幸运女神在微笑！",
                EventType = WorldEventType.LuckyMoment,
                Rarity = EventRarity.Rare,
                MinPlayerLevel = 5,
                TriggerChance = 0.04f,
                CooldownMinutes = 60,
                DurationSeconds = 180,
                GoldReward = 100,
                ExpReward = 150,
                ItemRewards = new List<string> { "lucky_charm" }
            });

            // 诅咒事件
            AddEvent(new WorldEventData {
                EventId = "curse_shadow",
                EventName = "暗影诅咒",
                Description = "暗影力量侵蚀了你...",
                EventType = WorldEventType.CurseEvent,
                Rarity = EventRarity.Uncommon,
                MinPlayerLevel = 10,
                TriggerChance = 0.05f,
                CooldownMinutes = 90,
                DurationSeconds = 300,
                ExpReward = 50,
                ItemRewards = new List<string> { "curse_mark" }
            });

            // 祝福事件
            AddEvent(new WorldEventData {
                EventId = "blessing_light",
                EventName = "神圣祝福",
                Description = "圣光环绕着你！",
                EventType = WorldEventType.BlessingEvent,
                Rarity = EventRarity.Epic,
                MinPlayerLevel = 15,
                TriggerChance = 0.02f,
                CooldownMinutes = 180,
                DurationSeconds = 600,
                GoldReward = 300,
                ExpReward = 400,
                ItemRewards = new List<string> { "holy_water", "blessing_token" }
            });

            // 传说事件 - 巨龙袭击
            AddEvent(new WorldEventData {
                EventId = "dragon_attack",
                EventName = "巨龙袭击",
                Description = "一只巨龙降临了！",
                EventType = WorldEventType.RARE_DragonAttack,
                Rarity = EventRarity.Legendary,
                MinPlayerLevel = 20,
                TriggerChance = 0.01f,
                CooldownMinutes = 360,
                DurationSeconds = 600,
                GoldReward = 1000,
                ExpReward = 1000,
                ItemRewards = new List<string> { "dragon_scale", "dragon_blood", "legendary_weapon" }
            });

            GD.Print($"[RandomWorldEventSystem] 事件数据库已加载，共 {_eventDatabase.Count} 个事件");
        }

        /// <summary>
        /// 添加事件到数据库
        /// </summary>
        public void AddEvent(WorldEventData eventData) {
            if (eventData != null && !string.IsNullOrEmpty(eventData.EventId)) {
                _eventDatabase[eventData.EventId] = eventData;
            }
        }

        /// <summary>
        /// 尝试触发随机事件
        /// </summary>
        public void TryTriggerRandomEvent() {
            if (_activeEvents.Count >= 5) {
                return; // 最多同时5个活跃事件
            }

            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player == null) return;

            int playerLevel = 1;
            if (player.HasMethod("GetLevel")) {
                playerLevel = (int)player.Call("GetLevel");
            }

            // 按稀有度筛选可用事件
            var availableEvents = _eventDatabase.Values
                .Where(e => e.MinPlayerLevel <= playerLevel)
                .Where(e => CanTriggerEvent(e))
                .ToList();

            if (availableEvents.Count == 0) return;

            // 根据稀有度权重随机选择
            var random = new Random();
            var selectedEvent = availableEvents[random.Next(availableEvents.Count)];
            
            // 触发概率判定
            if (random.NextDouble() < selectedEvent.TriggerChance) {
                TriggerEvent(selectedEvent);
            }
        }

        /// <summary>
        /// 检查是否可以触发事件
        /// </summary>
        private bool CanTriggerEvent(WorldEventData eventData) {
            string eventId = eventData.EventId;
            
            // 检查冷却
            if (_playerData.LastEventTime.TryGetValue(eventId, out var lastTime)) {
                var cooldown = TimeSpan.FromMinutes(eventData.CooldownMinutes);
                if (DateTime.Now - lastTime < cooldown) {
                    return false;
                }
            }
            
            return true;
        }

        /// <summary>
        /// 触发事件
        /// </summary>
        public void TriggerEvent(WorldEventData eventData) {
            string eventId = eventData.EventId;
            
            // 更新玩家数据
            if (!_playerData.EventTriggerCount.ContainsKey(eventId)) {
                _playerData.EventTriggerCount[eventId] = 0;
            }
            _playerData.EventTriggerCount[eventId]++;
            _playerData.LastEventTime[eventId] = DateTime.Now;
            _playerData.TotalEventsTriggered++;
            
            if (eventData.Rarity == EventRarity.Legendary) {
                _playerData.LegendaryEventsWitnessed++;
            }

            // 创建事件配置
            var config = new WorldEventConfig {
                Event = eventData,
                WorldPosition = GetRandomWorldPosition(),
                TriggerTime = DateTime.Now,
                ExpireTime = DateTime.Now.AddSeconds(eventData.DurationSeconds),
                IsActive = true
            };

            _activeEvents[eventId] = config;
            _playerData.ActiveEvents.Add(eventId);

            // 发送信号
            EmitSignal(SignalName.EventTriggered, config);
            
            GD.Print($"[RandomWorldEventSystem] 事件触发: {eventData.EventName} ({eventData.Rarity})");
        }

        /// <summary>
        /// 获取随机世界位置
        /// </summary>
        private Vector2 GetRandomWorldPosition() {
            var player = GetTree().GetFirstNodeInGroup("Player");
            if (player is Node2D playerNode) {
                var random = new Random();
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = 200f + (float)(random.NextDouble() * 300f);
                return playerNode.GlobalPosition + new Vector2(
                    (float)Math.Cos(angle) * distance,
                    (float)Math.Sin(angle) * distance
                );
            }
            return Vector2.Zero;
        }

        /// <summary>
        /// 更新活跃事件状态
        /// </summary>
        private void UpdateActiveEvents() {
            var now = DateTime.Now;
            var expiredEvents = new List<string>();

            foreach (var kvp in _activeEvents) {
                if (kvp.Value.IsActive && now >= kvp.Value.ExpireTime) {
                    kvp.Value.IsActive = false;
                    expiredEvents.Add(kvp.Key);
                    _playerData.ActiveEvents.Remove(kvp.Key);
                    EmitSignal(SignalName.EventExpired, kvp.Key);
                }
            }

            if (expiredEvents.Count > 0) {
                GD.Print($"[RandomWorldEventSystem] 事件过期: {string.Join(", ", expiredEvents)}");
            }
        }

        /// <summary>
        /// 获取所有活跃事件
        /// </summary>
        public List<WorldEventConfig> GetActiveEvents() {
            return _activeEvents.Values.Where(e => e.IsActive).ToList();
        }

        /// <summary>
        /// 领取事件奖励
        /// </summary>
        public void ClaimEventReward(string eventId) {
            if (!_activeEvents.TryGetValue(eventId, out var config) || !config.IsActive) {
                return;
            }

            var eventData = config.Event;
            
            // 发放金币
            if (eventData.GoldReward > 0) {
                var player = GetTree().GetFirstNodeInGroup("Player");
                if (player != null && player.HasMethod("AddGold")) {
                    player.Call("AddGold", eventData.GoldReward);
                }
            }

            // 发放经验
            if (eventData.ExpReward > 0) {
                var player = GetTree().GetFirstNodeInGroup("Player");
                if (player != null && player.HasMethod("AddExperience")) {
                    player.Call("AddExperience", eventData.ExpReward);
                }
            }

            // 发放物品
            if (eventData.ItemRewards.Count > 0) {
                var inventory = GetTree().GetFirstNodeInGroup("InventorySystem");
                if (inventory != null) {
                    foreach (var itemId in eventData.ItemRewards) {
                        if (inventory.HasMethod("AddItem")) {
                            inventory.Call("AddItem", itemId, 1);
                        }
                    }
                }
            }

            // 标记为已领取
            config.IsActive = false;
            _playerData.ActiveEvents.Remove(eventId);
            
            EmitSignal(SignalName.EventRewardClaimed, eventId, eventData.GoldReward, eventData.ExpReward);
            
            GD.Print($"[RandomWorldEventSystem] 奖励已领取: {eventData.EventName}, 金币: {eventData.GoldReward}, 经验: {eventData.ExpReward}");
        }

        /// <summary>
        /// 获取事件统计信息
        /// </summary>
        public PlayerWorldEventData GetPlayerEventData() {
            return _playerData;
        }

        /// <summary>
        /// 获取指定类型的事件
        /// </summary>
        public List<WorldEventData> GetEventsByType(WorldEventType type) {
            return _eventDatabase.Values.Where(e => e.EventType == type).ToList();
        }

        /// <summary>
        /// 获取指定稀有度的事件
        /// </summary>
        public List<WorldEventData> GetEventsByRarity(EventRarity rarity) {
            return _eventDatabase.Values.Where(e => e.Rarity == rarity).ToList();
        }

        /// <summary>
        /// 手动触发指定事件（用于测试或特殊触发）
        /// </summary>
        public void TriggerEventById(string eventId) {
            if (_eventDatabase.TryGetValue(eventId, out var eventData)) {
                TriggerEvent(eventData);
            }
        }

        /// <summary>
        /// 存档支持 - 保存玩家数据
        /// </summary>
        public Dictionary<string, object> SaveData() {
            var data = new Dictionary<string, object>();
            
            // 保存事件触发次数
            var triggerCount = new Dictionary<string, int>();
            foreach (var kvp in _playerData.EventTriggerCount) {
                triggerCount[kvp.Key] = kvp.Value;
            }
            data["event_trigger_count"] = triggerCount;
            
            // 保存上次触发时间
            var lastTimes = new Dictionary<string, string>();
            foreach (var kvp in _playerData.LastEventTime) {
                lastTimes[kvp.Key] = kvp.Value.ToString("o");
            }
            data["last_event_time"] = lastTimes;
            
            data["total_events_triggered"] = _playerData.TotalEventsTriggered;
            data["legendary_events_witnessed"] = _playerData.LegendaryEventsWitnessed;
            
            return data;
        }

        /// <summary>
        /// 存档支持 - 加载玩家数据
        /// </summary>
        public void LoadData(Dictionary<string, object> data) {
            if (data == null) return;
            
            if (data.TryGetValue("event_trigger_count", out var tcObj) && tcObj is Dictionary<string, int> tc) {
                _playerData.EventTriggerCount = tc;
            }
            
            if (data.TryGetValue("last_event_time", out var ltObj) && ltObj is Dictionary<string, string> lt) {
                _playerData.LastEventTime = new Dictionary<string, DateTime>();
                foreach (var kvp in lt) {
                    if (DateTime.TryParse(kvp.Value, out var dt)) {
                        _playerData.LastEventTime[kvp.Key] = dt;
                    }
                }
            }
            
            if (data.TryGetValue("total_events_triggered", out var total)) {
                _playerData.TotalEventsTriggered = (int)total;
            }
            
            if (data.TryGetValue("legendary_events_witnessed", out var legendary)) {
                _playerData.LegendaryEventsWitnessed = (int)legendary;
            }
        }
    }
}
