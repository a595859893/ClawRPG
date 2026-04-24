// World Event System
// 988+ Systems Milestone
// Handles random world events that occur during gameplay

using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Core.Systems
{
    /// <summary>
    /// Types of world events that can occur
    /// </summary>
    public enum WorldEventType
    {
        TreasureSpawn,      // Treasure chest appears
        MonsterSurge,       // Sudden monster attack
        MerchantVisit,      // Traveling merchant appears
        WeatherChange,     // Sudden weather change
        Blessing,          // Random blessing buff
        Curse,             // Random curse debuff
        RareSpawn,         // Rare creature spawns
        ResourceBurst,     // Resource nodes spawn
        Portal,            // Mysterious portal appears
        NpcRescue         // Need to rescue NPC
    }

    /// <summary>
    /// Event rarity determines frequency and rewards
    /// </summary>
    public enum WorldEventRarity
    {
        Common,     // 60% chance
        Uncommon,   // 25% chance
        Rare,       // 10% chance
        Epic,       // 4% chance
        Legendary   // 1% chance
    }

    /// <summary>
    /// Current state of a world event
    /// </summary>
    public enum WorldEventState
    {
        Pending,    // Event announced but not started
        Active,     // Event is currently active
        Completed,  // Event completed successfully
        Failed,     // Event failed (player didn't complete in time)
        Expired     // Event expired without interaction
    }

    /// <summary>
    /// Represents a world event configuration
    /// </summary>
    public class WorldEventConfig
    {
        public string EventId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public WorldEventType Type { get; set; }
        public WorldEventRarity Rarity { get; set; }
        public int DurationSeconds { get; set; }
        public int MinPlayerLevel { get; set; }
        public float SpawnChance { get; set; }
        
        // Rewards
        public int GoldReward { get; set; }
        public int ExperienceReward { get; set; }
        public List<string> ItemRewards { get; set; }
        
        // Requirements
        public int RequiredKills { get; set; }
        public int RequiredDistance { get; set; }
        public bool RequiresGroup { get; set; }
    }

    /// <summary>
    /// Active world event instance
    /// </summary>
    public class ActiveWorldEvent
    {
        public string EventId { get; set; }
        public string ConfigId { get; set; }
        public WorldEventType Type { get; set; }
        public WorldEventRarity Rarity { get; set; }
        public WorldEventState State { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int RemainingSeconds => (int)(EndTime - DateTime.Now).TotalSeconds;
        
        // Progress tracking
        public int CurrentProgress { get; set; }
        public int RequiredProgress { get; set; }
        public float ProgressPercent => RequiredProgress > 0 ? (float)CurrentProgress / RequiredProgress * 100 : 0;
        
        // Location
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public string LocationName { get; set; }
    }

    /// <summary>
    /// Player's world event history and stats
    /// </summary>
    public class PlayerWorldEventData
    {
        public int TotalEventsParticipated { get; set; }
        public int EventsCompleted { get; set; }
        public int EventsFailed { get; set; }
        public int GoldEarned { get; set; }
        public int ExperienceEarned { get; set; }
        
        // By type
        public Dictionary<WorldEventType, int> EventsByType { get; set; }
        
        // By rarity
        public Dictionary<WorldEventRarity, int> EventsByRarity { get; set; }
        
        // History
        public List<ActiveWorldEvent> EventHistory { get; set; }
        
        // Active events
        public List<ActiveWorldEvent> ActiveEvents { get; set; }
        
        public PlayerWorldEventData()
        {
            EventsByType = new Dictionary<WorldEventType, int>();
            EventsByRarity = new Dictionary<WorldEventRarity, int>();
            EventHistory = new List<ActiveWorldEvent>();
            ActiveEvents = new List<ActiveWorldEvent>();
        }
    }

    /// <summary>
    /// World Event System - Manages random world events
    /// </summary>
    public partial class WorldEventSystem : BaseSystem
    {
        private Dictionary<string, WorldEventConfig> _eventConfigs;
        private PlayerWorldEventData _playerData;
        private List<ActiveWorldEvent> _activeEvents;
        private Random _random;
        private float _spawnChance;
        private int _baseEventInterval; // seconds
        private DateTime _lastEventCheck;
        
        protected override string SystemName => "WorldEventSystem";
        
        // Event signals
        public event Action<ActiveWorldEvent> OnEventSpawned;
        public event Action<ActiveWorldEvent> OnEventCompleted;
        public event Action<ActiveWorldEvent> OnEventFailed;
        public event Action<ActiveWorldEvent, int> OnProgressUpdate;
        
        public WorldEventSystem()
        {
            _eventConfigs = new Dictionary<string, WorldEventConfig>();
            _activeEvents = new List<ActiveWorldEvent>();
            _random = new Random();
            _spawnChance = 0.3f; // 30% chance per check
            _baseEventInterval = 300; // 5 minutes base interval
            _lastEventCheck = DateTime.Now;
            InitializeEventConfigs();
        }
        
        private void InitializeEventConfigs()
        {
            // Treasure Spawn Events
            AddEventConfig(new WorldEventConfig
            {
                EventId = "treasure_common",
                Name = "Hidden Treasure",
                Description = "A treasure chest has appeared nearby!",
                Type = WorldEventType.TreasureSpawn,
                Rarity = WorldEventRarity.Common,
                DurationSeconds = 120,
                MinPlayerLevel = 1,
                SpawnChance = 0.15f,
                GoldReward = 100,
                ExperienceReward = 50,
                ItemRewards = new List<string> { "GoldCoin", "HealthPotion" }
            });
            
            AddEventConfig(new WorldEventConfig
            {
                EventId = "treasure_rare",
                Name = "Ancient Vault",
                Description = "An ancient vault has been discovered!",
                Type = WorldEventType.TreasureSpawn,
                Rarity = WorldEventRarity.Rare,
                DurationSeconds = 180,
                MinPlayerLevel = 10,
                SpawnChance = 0.05f,
                GoldReward = 500,
                ExperienceReward = 200,
                ItemRewards = new List<string> { "RareGem", "EpicWeapon" }
            });
            
            // Monster Surge Events
            AddEventConfig(new WorldEventConfig
            {
                EventId = "surge_common",
                Name = "Monster Surge",
                Description = "Monsters are attacking the area!",
                Type = WorldEventType.MonsterSurge,
                Rarity = WorldEventRarity.Common,
                DurationSeconds = 180,
                MinPlayerLevel = 1,
                SpawnChance = 0.12f,
                GoldReward = 150,
                ExperienceReward = 100,
                RequiredKills = 10
            });
            
            AddEventConfig(new WorldEventConfig
            {
                EventId = "surge_epic",
                Name = "Demon Invasion",
                Description = "Demons are invading! Defend the realm!",
                Type = WorldEventType.MonsterSurge,
                Rarity = WorldEventRarity.Epic,
                DurationSeconds = 300,
                MinPlayerLevel = 20,
                SpawnChance = 0.03f,
                GoldReward = 1000,
                ExperienceReward = 500,
                RequiredKills = 25,
                RequiresGroup = true
            });
            
            // Merchant Visit Events
            AddEventConfig(new WorldEventConfig
            {
                EventId = "merchant_legendary",
                Name = "Mystic Merchant",
                Description = "A legendary merchant has arrived with rare goods!",
                Type = WorldEventType.MerchantVisit,
                Rarity = WorldEventRarity.Legendary,
                DurationSeconds = 600,
                MinPlayerLevel = 15,
                SpawnChance = 0.02f,
                GoldReward = 200,
                ExperienceReward = 100,
                ItemRewards = new List<string> { "RareScroll", "LegendaryMaterial" }
            });
            
            // Blessing Events
            AddEventConfig(new WorldEventConfig
            {
                EventId = "blessing_epic",
                Name = "Divine Blessing",
                Description = "The gods have blessed you with power!",
                Type = WorldEventType.Blessing,
                Rarity = WorldEventRarity.Epic,
                DurationSeconds = 3600,
                MinPlayerLevel = 5,
                SpawnChance = 0.04f,
                GoldReward = 0,
                ExperienceReward = 300
            });
            
            // Curse Events
            AddEventConfig(new WorldEventConfig
            {
                EventId = "curse_rare",
                Name = "Dark Aura",
                Description = "A dark aura surrounds this area...",
                Type = WorldEventType.Curse,
                Rarity = WorldEventRarity.Rare,
                DurationSeconds = 1800,
                MinPlayerLevel = 10,
                SpawnChance = 0.06f,
                GoldReward = 0,
                ExperienceReward = 150
            });
            
            // Rare Spawn Events
            AddEventConfig(new WorldEventConfig
            {
                EventId = "rare_dragon",
                Name = "Elder Dragon",
                Description = "An elder dragon has appeared!",
                Type = WorldEventType.RareSpawn,
                Rarity = WorldEventRarity.Legendary,
                DurationSeconds = 600,
                MinPlayerLevel = 25,
                SpawnChance = 0.01f,
                GoldReward = 5000,
                ExperienceReward = 2000,
                RequiresGroup = true
            });
            
            // Resource Burst Events
            AddEventConfig(new WorldEventConfig
            {
                EventId = "resource_common",
                Name = "Resource Rush",
                Description = "Resources are abundant in this area!",
                Type = WorldEventType.ResourceBurst,
                Rarity = WorldEventRarity.Common,
                DurationSeconds = 300,
                MinPlayerLevel = 1,
                SpawnChance = 0.1f,
                GoldReward = 50,
                ExperienceReward = 25
            });
            
            // Portal Events
            AddEventConfig(new WorldEventConfig
            {
                EventId = "portal_epic",
                Name = "Mystic Portal",
                Description = "A mystic portal to another dimension has opened!",
                Type = WorldEventType.Portal,
                Rarity = WorldEventRarity.Epic,
                DurationSeconds = 240,
                MinPlayerLevel = 15,
                SpawnChance = 0.03f,
                GoldReward = 800,
                ExperienceReward = 400
            });
            
            // NPC Rescue Events
            AddEventConfig(new WorldEventConfig
            {
                EventId = "rescue_rare",
                Name = "Lost Adventurer",
                Description = "A lost adventurer needs your help!",
                Type = WorldEventType.NPCrescue,
                Rarity = WorldEventRarity.Rare,
                DurationSeconds = 180,
                MinPlayerLevel = 5,
                SpawnChance = 0.05f,
                GoldReward = 300,
                ExperienceReward = 150,
                RequiredDistance = 500
            });
        }
        
        private void AddEventConfig(WorldEventConfig config)
        {
            _eventConfigs[config.EventId] = config;
        }
        
        public void Initialize(PlayerWorldEventData playerData)
        {
            _playerData = playerData;
        }
        
        /// <summary>
        /// Check if a new world event should spawn
        /// </summary>
        public void Update(float deltaTime)
        {
            var now = DateTime.Now;
            
            // Check every base interval
            if ((now - _lastEventCheck).TotalSeconds >= _baseEventInterval)
            {
                _lastEventCheck = now;
                
                // Try to spawn event
                if (_random.NextDouble() < _spawnChance && _activeEvents.Count < 5)
                {
                    TrySpawnEvent();
                }
            }
            
            // Update active events
            UpdateActiveEvents();
        }
        
        private void TrySpawnEvent()
        {
            // Filter available events by player level
            var availableEvents = new List<WorldEventConfig>();
            foreach (var config in _eventConfigs.Values)
            {
                // Skip if recently spawned similar event
                bool recentlySpawned = false;
                foreach (var active in _activeEvents)
                {
                    if (active.ConfigId == config.EventId)
                    {
                        recentlySpawned = true;
                        break;
                    }
                }
                
                if (!recentlySpawned && config.SpawnChance > _random.NextDouble())
                {
                    availableEvents.Add(config);
                }
            }
            
            if (availableEvents.Count > 0)
            {
                var selectedConfig = availableEvents[_random.Next(availableEvents.Count)];
                SpawnEvent(selectedConfig);
            }
        }
        
        private void SpawnEvent(WorldEventConfig config)
        {
            var newEvent = new ActiveWorldEvent
            {
                EventId = Guid.NewGuid().ToString(),
                ConfigId = config.EventId,
                Type = config.Type,
                Rarity = config.Rarity,
                State = WorldEventState.Pending,
                StartTime = DateTime.Now,
                EndTime = DateTime.Now.AddSeconds(config.DurationSeconds),
                RequiredProgress = config.RequiredKills > 0 ? config.RequiredKills : 1,
                CurrentProgress = 0,
                PositionX = _random.Next(-100, 100),
                PositionY = _random.Next(-100, 100),
                LocationName = GetRandomLocationName()
            };
            
            _activeEvents.Add(newEvent);
            OnEventSpawned?.Invoke(newEvent);
        }
        
        private string GetRandomLocationName()
        {
            string[] locations = {
                "Dark Forest", "Crystal Cavern", "Sunset Plains", 
                "Ancient Ruins", "Frozen Tundra", "Volcanic Wastes",
                "Mystic Marsh", "Shadow Valley", "Dragon's Peak"
            };
            return locations[_random.Next(locations.Length)];
        }
        
        private void UpdateActiveEvents()
        {
            var expiredEvents = new List<ActiveWorldEvent>();
            
            foreach (var evt in _activeEvents)
            {
                if (evt.RemainingSeconds <= 0)
                {
                    if (evt.State == WorldEventState.Active && evt.CurrentProgress >= evt.RequiredProgress)
                    {
                        CompleteEvent(evt);
                    }
                    else
                    {
                        FailEvent(evt);
                    }
                    expiredEvents.Add(evt);
                }
            }
            
            foreach (var evt in expiredEvents)
            {
                _activeEvents.Remove(evt);
            }
        }
        
        /// <summary>
        /// Player participates in an event
        /// </summary>
        public void ParticipateInEvent(string eventId)
        {
            var evt = _activeEvents.Find(e => e.EventId == eventId);
            if (evt != null && evt.State == WorldEventState.Pending)
            {
                evt.State = WorldEventState.Active;
            }
        }
        
        /// <summary>
        /// Update event progress (kills, distance, etc.)
        /// </summary>
        public void UpdateProgress(string eventId, int progressAmount = 1)
        {
            var evt = _activeEvents.Find(e => e.EventId == eventId);
            if (evt != null && evt.State == WorldEventState.Active)
            {
                evt.CurrentProgress += progressAmount;
                OnProgressUpdate?.Invoke(evt, evt.CurrentProgress);
                
                if (evt.CurrentProgress >= evt.RequiredProgress)
                {
                    CompleteEvent(evt);
                }
            }
        }
        
        private void CompleteEvent(ActiveWorldEvent evt)
        {
            evt.State = WorldEventState.Completed;
            
            var config = _eventConfigs[evt.ConfigId];
            
            // Update player data
            _playerData.TotalEventsParticipated++;
            _playerData.EventsCompleted++;
            _playerData.GoldEarned += config.GoldReward;
            _playerData.ExperienceEarned += config.ExperienceReward;
            
            if (!_playerData.EventsByType.ContainsKey(evt.Type))
                _playerData.EventsByType[evt.Type] = 0;
            _playerData.EventsByType[evt.Type]++;
            
            if (!_playerData.EventsByRarity.ContainsKey(evt.Rarity))
                _playerData.EventsByRarity[evt.Rarity] = 0;
            _playerData.EventsByRarity[evt.Rarity]++;
            
            _playerData.EventHistory.Add(evt);
            
            OnEventCompleted?.Invoke(evt);
        }
        
        private void FailEvent(ActiveWorldEvent evt)
        {
            evt.State = WorldEventState.Failed;
            
            _playerData.TotalEventsParticipated++;
            _playerData.EventsFailed++;
            _playerData.EventHistory.Add(evt);
            
            OnEventFailed?.Invoke(evt);
        }
        
        /// <summary>
        /// Get all active events
        /// </summary>
        public List<ActiveWorldEvent> GetActiveEvents()
        {
            return new List<ActiveWorldEvent>(_activeEvents);
        }
        
        /// <summary>
        /// Get event configuration by ID
        /// </summary>
        public WorldEventConfig GetEventConfig(string configId)
        {
            return _eventConfigs.ContainsKey(configId) ? _eventConfigs[configId] : null;
        }
        
        /// <summary>
        /// Get player statistics
        /// </summary>
        public PlayerWorldEventData GetPlayerData()
        {
            return _playerData;
        }
        
        /// <summary>
        /// Export save data (BaseSystem override)
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["totalEventsParticipated"] = _playerData.TotalEventsParticipated;
            data["eventsCompleted"] = _playerData.EventsCompleted;
            data["eventsFailed"] = _playerData.EventsFailed;
            data["goldEarned"] = _playerData.GoldEarned;
            data["experienceEarned"] = _playerData.ExperienceEarned;
            data["eventsByType"] = _playerData.EventsByType;
            data["eventsByRarity"] = _playerData.EventsByRarity;
            data["eventHistory"] = _playerData.EventHistory;
            data["activeEvents"] = _activeEvents;
            return data;
        }
        
        /// <summary>
        /// Import save data (BaseSystem override)
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("totalEventsParticipated"))
                _playerData.TotalEventsParticipated = (int)data["totalEventsParticipated"];
            if (data.ContainsKey("eventsCompleted"))
                _playerData.EventsCompleted = (int)data["eventsCompleted"];
            if (data.ContainsKey("eventsFailed"))
                _playerData.EventsFailed = (int)data["eventsFailed"];
            if (data.ContainsKey("goldEarned"))
                _playerData.GoldEarned = (int)data["goldEarned"];
            if (data.ContainsKey("experienceEarned"))
                _playerData.ExperienceEarned = (int)data["experienceEarned"];
        }
    }
}
