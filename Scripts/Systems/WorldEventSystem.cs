using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 世界事件类型
    /// </summary>
    public enum WorldEventType {
        MonsterInvasion,    // 怪物入侵
        TreasureSpawn,      // 宝藏出现
        MerchantVisit,      // 商人拜访
        WeatherChange,      // 天气变化
        LuckyDrop,          // 幸运掉落
        DoubleXP,           // 双倍经验
        RareEnemySpawn,     // 稀有敌人出现
        BossRush,           // Bossrush
        PeacefulDay,       // 和平之日
        StormRush           // 风暴侵袭
    }

    /// <summary>
    /// 世界事件难度
    /// </summary>
    public enum WorldEventDifficulty {
        Easy,       // 简单
        Normal,     // 普通
        Hard,       // 困难
        Epic        // 史诗
    }

    /// <summary>
    /// 世界事件数据类
    /// </summary>
    public class WorldEvent {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public WorldEventType Type { get; set; }
        public WorldEventDifficulty Difficulty { get; set; }
        public int Duration { get; set; }  // 持续时间（秒）
        public int Cooldown { get; set; }  // 冷却时间（秒）
        public float XPMultiplier { get; set; } = 1.0f;
        public float DropMultiplier { get; set; } = 1.0f;
        public float GoldMultiplier { get; set; } = 1.0f;
        public string Icon { get; set; }
        public string Color { get; set; }
        
        // 事件特定属性
        public string SpawnEnemyId { get; set; }
        public int SpawnCount { get; set; }
        public float SpawnRadius { get; set; }
        public string WeatherType { get; set; }
        public int DiscountPercent { get; set; }
        public List<string> BonusItemIds { get; set; }

        public WorldEvent() {
            BonusItemIds = new List<string>();
        }

        public float GetDifficultyMultiplier() {
            return Difficulty switch {
                WorldEventDifficulty.Easy => 1.0f,
                WorldEventDifficulty.Normal => 1.5f,
                WorldEventDifficulty.Hard => 2.0f,
                WorldEventDifficulty.Epic => 3.0f,
                _ => 1.0f
            };
        }

        public string GetDifficultyText() {
            return Difficulty switch {
                WorldEventDifficulty.Easy => "简单",
                WorldEventDifficulty.Normal => "普通",
                WorldEventDifficulty.Hard => "困难",
                WorldEventDifficulty.Epic => "史诗",
                _ => "未知"
            };
        }
    }

    /// <summary>
    /// 世界事件数据库
    /// </summary>
    public class WorldEventDatabase {
        private static WorldEventDatabase _instance;
        public static WorldEventDatabase Instance => _instance ??= new WorldEventDatabase();

        private List<WorldEvent> _events = new List<WorldEvent>();

        public WorldEventDatabase() {
            InitializeEvents();
        }

        private void InitializeEvents() {
            // 怪物入侵
            _events.Add(new WorldEvent {
                Id = "monster_invasion_easy",
                Name = "怪物入侵",
                Description = "大量怪物从裂缝中涌出，击败它们获得额外奖励！",
                Type = WorldEventType.MonsterInvasion,
                Difficulty = WorldEventDifficulty.Easy,
                Duration = 180,
                Cooldown = 600,
                XPMultiplier = 1.5f,
                DropMultiplier = 1.5f,
                GoldMultiplier = 1.5f,
                Icon = "⚔️",
                Color = "#FF6B6B",
                SpawnEnemyId = "goblin",
                SpawnCount = 15,
                SpawnRadius = 50f
            });

            _events.Add(new WorldEvent {
                Id = "monster_invasion_hard",
                Name = "怪物大军",
                Description = "前所未有的怪物大军来袭！准备战斗！",
                Type = WorldEventType.MonsterInvasion,
                Difficulty = WorldEventDifficulty.Hard,
                Duration = 300,
                Cooldown = 1200,
                XPMultiplier = 2.5f,
                DropMultiplier = 2.5f,
                GoldMultiplier = 2.5f,
                Icon = "💀",
                Color = "#FF0000",
                SpawnEnemyId = "skeleton_warrior",
                SpawnCount = 30,
                SpawnRadius = 80f
            });

            // 宝藏出现
            _events.Add(new WorldEvent {
                Id = "treasure_spawn",
                Name = "宝藏出现",
                Description = "传说中的宝藏箱出现在世界中！",
                Type = WorldEventType.TreasureSpawn,
                Difficulty = WorldEventDifficulty.Normal,
                Duration = 120,
                Cooldown = 900,
                XPMultiplier = 1.2f,
                DropMultiplier = 3.0f,
                GoldMultiplier = 2.0f,
                Icon = "💎",
                Color = "#FFD700",
                SpawnCount = 5,
                SpawnRadius = 100f,
                BonusItemIds = new List<string> { "dragon_scale", "phoenix_feather", "shadow_crystal" }
            });

            // 商人拜访
            _events.Add(new WorldEvent {
                Id = "merchant_visit",
                Name = "神秘商人",
                Description = "神秘商人来到此地，商品打折出售！",
                Type = WorldEventType.MerchantVisit,
                Difficulty = WorldEventDifficulty.Normal,
                Duration = 300,
                Cooldown = 1800,
                XPMultiplier = 1.0f,
                DropMultiplier = 1.0f,
                GoldMultiplier = 1.0f,
                Icon = "🛒",
                Color = "#4ECDC4",
                DiscountPercent = 30
            });

            // 天气变化 - 晴天
            _events.Add(new WorldEvent {
                Id = "weather_sunny",
                Name = "晴朗之日",
                Description = "阳光普照，经验获得提升！",
                Type = WorldEventType.WeatherChange,
                Difficulty = WorldEventDifficulty.Easy,
                Duration = 600,
                Cooldown = 300,
                XPMultiplier = 1.3f,
                DropMultiplier = 1.0f,
                GoldMultiplier = 1.0f,
                Icon = "☀️",
                Color = "#FFE66D",
                WeatherType = "sunny"
            });

            // 天气变化 - 暴风雨
            _events.Add(new WorldEvent {
                Id = "weather_storm",
                Name = "暴风雨",
                Description = "暴风雨来了，敌人变得更强，但掉落更好！",
                Type = WorldEventType.WeatherChange,
                Difficulty = WorldEventDifficulty.Hard,
                Duration = 300,
                Cooldown = 900,
                XPMultiplier = 2.0f,
                DropMultiplier = 2.0f,
                GoldMultiplier = 1.5f,
                Icon = "⛈️",
                Color = "#6C5CE7",
                WeatherType = "storm"
            });

            // 幸运掉落
            _events.Add(new WorldEvent {
                Id = "lucky_drop",
                Name = "幸运时刻",
                Description = "幸运女神眷顾你！所有掉落率大幅提升！",
                Type = WorldEventType.LuckyDrop,
                Difficulty = WorldEventDifficulty.Normal,
                Duration = 180,
                Cooldown = 720,
                XPMultiplier = 1.0f,
                DropMultiplier = 3.0f,
                GoldMultiplier = 2.0f,
                Icon = "🍀",
                Color = "#00FF7F"
            });

            // 双倍经验
            _events.Add(new WorldEvent {
                Id = "double_xp",
                Name = "双倍经验",
                Description = "今日经验翻倍！是升级的好时机！",
                Type = WorldEventType.DoubleXP,
                Difficulty = WorldEventDifficulty.Normal,
                Duration = 600,
                Cooldown = 14400, // 4小时
                XPMultiplier = 2.0f,
                DropMultiplier = 1.0f,
                GoldMultiplier = 1.0f,
                Icon = "⭐",
                Color = "#00D9FF"
            });

            // 稀有敌人
            _events.Add(new WorldEvent {
                Id = "rare_enemy_golden",
                Name = "黄金生物",
                Description = "传说中的黄金生物出现了！",
                Type = WorldEventType.RareEnemySpawn,
                Difficulty = WorldEventDifficulty.Epic,
                Duration = 120,
                Cooldown = 3600,
                XPMultiplier = 3.0f,
                DropMultiplier = 5.0f,
                GoldMultiplier = 5.0f,
                Icon = "🐲",
                Color = "#FFD700",
                SpawnEnemyId = "golden_dragon",
                SpawnCount = 1,
                SpawnRadius = 30f
            });

            // Bossrush
            _events.Add(new WorldEvent {
                Id = "boss_rush",
                Name = "Bossrush",
                Description = "Boss不断来袭，挑战你的极限！",
                Type = WorldEventType.BossRush,
                Difficulty = WorldEventDifficulty.Epic,
                Duration = 600,
                Cooldown = 7200,
                XPMultiplier = 4.0f,
                DropMultiplier = 3.0f,
                GoldMultiplier = 3.0f,
                Icon = "👹",
                Color = "#FF4500",
                SpawnCount = 5,
                SpawnRadius = 100f
            });

            // 和平之日
            _events.Add(new WorldEvent {
                Id = "peaceful_day",
                Name = "和平之日",
                Description = "今天是和平之日，怪物不会主动攻击！",
                Type = WorldEventType.PeacefulDay,
                Difficulty = WorldEventDifficulty.Easy,
                Duration = 600,
                Cooldown = 10800,
                XPMultiplier = 0.5f,
                DropMultiplier = 0.8f,
                GoldMultiplier = 0.8f,
                Icon = "🕊️",
                Color = "#98FB98"
            });

            // 风暴侵袭
            _events.Add(new WorldEvent {
                Id = "storm_rush",
                Name = "风暴侵袭",
                Description = "致命风暴来袭，危险与机遇并存！",
                Type = WorldEventType.StormRush,
                Difficulty = WorldEventDifficulty.Hard,
                Duration = 240,
                Cooldown = 1800,
                XPMultiplier = 2.0f,
                DropMultiplier = 2.5f,
                GoldMultiplier = 2.0f,
                Icon = "🌪️",
                Color = "#8B0000",
                WeatherType = "storm",
                SpawnCount = 20,
                SpawnRadius = 60f
            });
        }

        public List<WorldEvent> GetAllEvents() => new List<WorldEvent>(_events);

        public List<WorldEvent> GetEventsByType(WorldEventType type) {
            return _events.FindAll(e => e.Type == type);
        }

        public List<WorldEvent> GetAvailableEvents(int playerLevel) {
            // 返回玩家等级可以参与的事件
            return _events.FindAll(e => playerLevel >= 1); // 暂时所有事件对1级以上玩家开放
        }

        public WorldEvent GetRandomEvent(int playerLevel) {
            var available = GetAvailableEvents(playerLevel);
            if (available.Count == 0) return null;
            
            var random = new Random();
            return available[random.Next(available.Count)];
        }

        public WorldEvent GetEventById(string id) {
            return _events.Find(e => e.Id == id);
        }
    }

    /// <summary>
    /// 世界事件管理器
    /// </summary>
    public class WorldEventManager : Node {
        private static WorldEventManager _instance;
        public static WorldEventManager Instance => _instance;

        [Signal]
        public delegate void EventStarted(WorldEvent evt);

        [Signal]
        public delegate void EventEnded(WorldEvent evt);

        [Signal]
        public delegate void EventUpdated(WorldEvent evt, int remainingTime);

        private WorldEvent _currentEvent;
        private int _eventTimer;
        private int _nextEventCountdown;
        private bool _isEventActive;
        private Dictionary<string, int> _eventCooldowns = new Dictionary<string, int>();
        private int _globalCooldown = 60; // 全局冷却时间

        public WorldEvent CurrentEvent => _currentEvent;
        public bool IsEventActive => _isEventActive;
        public int EventRemainingTime => _eventTimer;
        public int NextEventCountdown => _nextEventCountdown;

        public override void _Ready() {
            _instance = this;
            _nextEventCountdown = 60; // 1分钟后开始第一次事件
            GD.Print("世界事件管理器已启动");
        }

        public override void _Process(float delta) {
            // 更新事件倒计时
            if (_isEventActive && _currentEvent != null) {
                _eventTimer -= (int)(delta * 60); // 假设60fps
                
                EmitSignal(nameof(EventUpdated), _currentEvent, _eventTimer);

                if (_eventTimer <= 0) {
                    EndEvent();
                }
            } else {
                // 等待下一次事件
                _nextEventCountdown -= (int)(delta * 60);
                if (_nextEventCountdown <= 0) {
                    StartRandomEvent();
                }
            }
        }

        private void StartRandomEvent() {
            var player = GetTree().GetFirstNodeInGroup("player") as Player;
            int playerLevel = player != null ? player.Level : 1;

            var database = WorldEventDatabase.Instance;
            var availableEvents = database.GetAvailableEvents(playerLevel);

            // 过滤掉还在冷却中的事件
            availableEvents.RemoveAll(e => IsEventOnCooldown(e.Id));

            if (availableEvents.Count == 0) {
                _nextEventCountdown = _globalCooldown * 60;
                return;
            }

            // 根据权重随机选择事件
            var random = new Random();
            var selectedEvent = availableEvents[random.Next(availableEvents.Count)];

            StartEvent(selectedEvent);
        }

        public void StartEvent(WorldEvent evt) {
            if (_isEventActive) return;

            _currentEvent = evt;
            _eventTimer = evt.Duration;
            _isEventActive = true;

            // 设置事件冷却
            _eventCooldowns[evt.Id] = evt.Cooldown;

            GD.Print($"世界事件开始: {evt.Name} - {evt.Description}");

            // 应用事件效果
            ApplyEventEffects(evt);

            // 发送通知
            var main = GetTree().CurrentScene as Main;
            main?.ShowNotification($"世界事件: {evt.Name}", evt.Icon + " " + evt.Description);

            EmitSignal(nameof(EventStarted), evt);
        }

        public void EndEvent() {
            if (_currentEvent == null) return;

            var evt = _currentEvent;
            _isEventActive = false;
            _currentEvent = null;
            _nextEventCountdown = _globalCooldown * 60 + evt.Cooldown;

            GD.Print($"世界事件结束: {evt.Name}");

            // 移除事件效果
            RemoveEventEffects(evt);

            EmitSignal(nameof(EventEnded), evt);
        }

        private void ApplyEventEffects(WorldEvent evt) {
            var player = GetTree().GetFirstNodeInGroup("player") as Player;
            if (player == null) return;

            // 应用经验倍率
            player.EventXPMultiplier = evt.XPMultiplier;
            
            // 应用掉落倍率
            player.EventDropMultiplier = evt.DropMultiplier;
            
            // 应用金币倍率
            player.EventGoldMultiplier = evt.GoldMultiplier;

            // 根据事件类型应用特殊效果
            switch (evt.Type) {
                case WorldEventType.PeacefulDay:
                    // 和平模式下敌人不主动攻击
                    SetEnemyAggressive(false);
                    break;
                    
                case WorldEventType.StormRush:
                case WorldEventType.WeatherChange:
                    // 天气效果 - 可以在此处添加视觉特效
                    break;
                    
                case WorldEventType.MonsterInvasion:
                case WorldEventType.BossRush:
                case WorldEventType.RareEnemySpawn:
                    // 召唤特殊敌人
                    SpawnSpecialEnemies(evt);
                    break;
            }
        }

        private void RemoveEventEffects(WorldEvent evt) {
            var player = GetTree().GetFirstNodeInGroup("player") as Player;
            if (player == null) return;

            // 移除倍率效果
            player.EventXPMultiplier = 1.0f;
            player.EventDropMultiplier = 1.0f;
            player.EventGoldMultiplier = 1.0f;

            // 移除特殊效果
            switch (evt.Type) {
                case WorldEventType.PeacefulDay:
                    SetEnemyAggressive(true);
                    break;
            }
        }

        private void SetEnemyAggressive(bool aggressive) {
            var enemies = GetTree().GetNodesInGroup("enemy");
            foreach (var enemy in enemies) {
                if (enemy is Enemy e) {
                    e.IsPeacefulMode = !aggressive;
                }
            }
        }

        private void SpawnSpecialEnemies(WorldEvent evt) {
            if (string.IsNullOrEmpty(evt.SpawnEnemyId) && evt.SpawnCount > 0) {
                // 通用敌人生成
                return;
            }

            // 可以在这里调用敌人生成器生成特殊敌人
            var enemyDatabase = EnemyDatabase.Instance;
            if (enemyDatabase != null) {
                var enemyType = enemyDatabase.GetEnemyType(evt.SpawnEnemyId);
                if (enemyType != null) {
                    GD.Print($"世界事件生成特殊敌人: {evt.SpawnEnemyId} x{evt.SpawnCount}");
                }
            }
        }

        public bool IsEventOnCooldown(string eventId) {
            return _eventCooldowns.ContainsKey(eventId) && _eventCooldowns[eventId] > 0;
        }

        public int GetCooldownRemaining(string eventId) {
            return _eventCooldowns.ContainsKey(eventId) ? _eventCooldowns[eventId] : 0;
        }

        public float GetCurrentXPMultiplier() {
            if (_currentEvent != null) {
                return _currentEvent.XPMultiplier;
            }
            return 1.0f;
        }

        public float GetCurrentDropMultiplier() {
            if (_currentEvent != null) {
                return _currentEvent.DropMultiplier;
            }
            return 1.0f;
        }

        public float GetCurrentGoldMultiplier() {
            if (_currentEvent != null) {
                return _currentEvent.GoldMultiplier;
            }
            return 1.0f;
        }

        // 手动触发事件（用于测试或特殊活动）
        public void TriggerEvent(string eventId) {
            var evt = WorldEventDatabase.Instance.GetEventById(eventId);
            if (evt != null && !_isEventActive) {
                StartEvent(evt);
            }
        }

        // 跳过等待时间，直接开始事件
        public void SkipWait() {
            if (!_isEventActive) {
                _nextEventCountdown = 0;
            }
        }
    }
}
