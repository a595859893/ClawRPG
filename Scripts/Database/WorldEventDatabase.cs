// World Event Database
// Configuration database for world events

using System;
using System.Collections.Generic;

namespace ClawRPG.Core.Database
{
    /// <summary>
    /// Database configuration for world events
    /// </summary>
    public static class WorldEventDatabase
    {
        // Event type configurations
        public static Dictionary<WorldEventType, WorldEventTypeConfig> EventTypeConfigs { get; private set; }
        
        // Rarity configurations
        public static Dictionary<WorldEventRarity, RarityConfig> RarityConfigs { get; private set; }
        
        // Location configurations
        public static List<WorldEventLocation> Locations { get; private set; }
        
        // Time-based spawn modifiers
        public static Dictionary<TimePeriod, SpawnModifier> TimeSpawnModifiers { get; private set; }
        
        static WorldEventDatabase()
        {
            InitializeEventTypeConfigs();
            InitializeRarityConfigs();
            InitializeLocations();
            InitializeTimeSpawnModifiers();
        }
        
        private static void InitializeEventTypeConfigs()
        {
            EventTypeConfigs = new Dictionary<WorldEventType, WorldEventTypeConfig>
            {
                [WorldEventType.TreasureSpawn] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.TreasureSpawn,
                    BaseDuration = 120,
                    BaseGoldReward = 150,
                    BaseExpReward = 75,
                    MinPlayers = 1,
                    MaxPlayers = 1,
                    Icon = "💎",
                    Color = "§e"
                },
                
                [WorldEventType.MonsterSurge] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.MonsterSurge,
                    BaseDuration = 180,
                    BaseGoldReward = 200,
                    BaseExpReward = 150,
                    MinPlayers = 1,
                    MaxPlayers = 10,
                    Icon = "👹",
                    Color = "§c"
                },
                
                [WorldEventType.MerchantVisit] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.MerchantVisit,
                    BaseDuration = 600,
                    BaseGoldReward = 0,
                    BaseExpReward = 50,
                    MinPlayers = 1,
                    MaxPlayers = 1,
                    Icon = "🛒",
                    Color = "§6"
                },
                
                [WorldEventType.WeatherChange] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.WeatherChange,
                    BaseDuration = 300,
                    BaseGoldReward = 50,
                    BaseExpReward = 25,
                    MinPlayers = 1,
                    MaxPlayers = 1,
                    Icon = "🌤️",
                    Color = "§b"
                },
                
                [WorldEventType.Blessing] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.Blessing,
                    BaseDuration = 3600,
                    BaseGoldReward = 100,
                    BaseExpReward = 200,
                    MinPlayers = 1,
                    MaxPlayers = 1,
                    Icon = "✨",
                    Color = "§a"
                },
                
                [WorldEventType.Curse] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.Curse,
                    BaseDuration = 1800,
                    BaseGoldReward = 0,
                    BaseExpReward = 100,
                    MinPlayers = 1,
                    MaxPlayers = 1,
                    Icon = "💀",
                    Color = "§8"
                },
                
                [WorldEventType.RareSpawn] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.RareSpawn,
                    BaseDuration = 600,
                    BaseGoldReward = 3000,
                    BaseExpReward = 1500,
                    MinPlayers = 1,
                    MaxPlayers = 10,
                    Icon = "🐉",
                    Color = "§5"
                },
                
                [WorldEventType.ResourceBurst] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.ResourceBurst,
                    BaseDuration = 300,
                    BaseGoldReward = 75,
                    BaseExpReward = 50,
                    MinPlayers = 1,
                    MaxPlayers = 1,
                    Icon = "🌿",
                    Color = "§2"
                },
                
                [WorldEventType.Portal] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.Portal,
                    BaseDuration = 240,
                    BaseGoldReward = 500,
                    BaseExpReward = 300,
                    MinPlayers = 1,
                    MaxPlayers = 5,
                    Icon = "🌀",
                    Color = "§d"
                },
                
                [WorldEventType.NPCrescue] = new WorldEventTypeConfig
                {
                    Type = WorldEventType.NPCrescue,
                    BaseDuration = 180,
                    BaseGoldReward = 250,
                    BaseExpReward = 125,
                    MinPlayers = 1,
                    MaxPlayers = 1,
                    Icon = "👤",
                    Color = "§f"
                }
            };
        }
        
        private static void InitializeRarityConfigs()
        {
            RarityConfigs = new Dictionary<WorldEventRarity, RarityConfig>
            {
                [WorldEventRarity.Common] = new RarityConfig
                {
                    Rarity = WorldEventRarity.Common,
                    BaseChance = 0.60,
                    GoldMultiplier = 1.0f,
                    ExpMultiplier = 1.0f,
                    Color = "§f",
                    DisplayName = "Common"
                },
                
                [WorldEventRarity.Uncommon] = new RarityConfig
                {
                    Rarity = WorldEventRarity.Uncommon,
                    BaseChance = 0.25,
                    GoldMultiplier = 1.5f,
                    ExpMultiplier = 1.5f,
                    Color = "§a",
                    DisplayName = "Uncommon"
                },
                
                [WorldEventRarity.Rare] = new RarityConfig
                {
                    Rarity = WorldEventRarity.Rare,
                    BaseChance = 0.10,
                    GoldMultiplier = 2.5f,
                    ExpMultiplier = 2.5f,
                    Color = "§9",
                    DisplayName = "Rare"
                },
                
                [WorldEventRarity.Epic] = new RarityConfig
                {
                    Rarity = WorldEventRarity.Epic,
                    BaseChance = 0.04,
                    GoldMultiplier = 5.0f,
                    ExpMultiplier = 5.0f,
                    Color = "§5",
                    DisplayName = "Epic"
                },
                
                [WorldEventRarity.Legendary] = new RarityConfig
                {
                    Rarity = WorldEventRarity.Legendary,
                    BaseChance = 0.01,
                    GoldMultiplier = 10.0f,
                    ExpMultiplier = 10.0f,
                    Color = "§6",
                    DisplayName = "Legendary"
                }
            };
        }
        
        private static void InitializeLocations()
        {
            Locations = new List<WorldEventLocation>
            {
                new WorldEventLocation { Name = "Dark Forest", MinLevel = 1, PreferredEventTypes = new List<WorldEventType> { WorldEventType.MonsterSurge, WorldEventType.TreasureSpawn } },
                new WorldEventLocation { Name = "Crystal Cavern", MinLevel = 5, PreferredEventTypes = new List<WorldEventType> { WorldEventType.TreasureSpawn, WorldEventType.ResourceBurst } },
                new WorldEventLocation { Name = "Sunset Plains", MinLevel = 1, PreferredEventTypes = new List<WorldEventType> { WorldEventType.Blessing, WorldEventType.MerchantVisit } },
                new WorldEventLocation { Name = "Ancient Ruins", MinLevel = 10, PreferredEventTypes = new List<WorldEventType> { WorldEventType.TreasureSpawn, WorldEventType.Portal, WorldEventType.RareSpawn } },
                new WorldEventLocation { Name = "Frozen Tundra", MinLevel = 15, PreferredEventTypes = new List<WorldEventType> { WorldEventType.MonsterSurge, WorldEventType.Blessing } },
                new WorldEventLocation { Name = "Volcanic Wastes", MinLevel = 20, PreferredEventTypes = new List<WorldEventType> { WorldEventType.RareSpawn, WorldEventType.Curse } },
                new WorldEventLocation { Name = "Mystic Marsh", MinLevel = 8, PreferredEventTypes = new List<WorldEventType> { WorldEventType.ResourceBurst, WorldEventType.NPCrescue } },
                new WorldEventLocation { Name = "Shadow Valley", MinLevel = 12, PreferredEventTypes = new List<WorldEventType> { WorldEventType.Curse, WorldEventType.TreasureSpawn } },
                new WorldEventLocation { Name = "Dragon's Peak", MinLevel = 25, PreferredEventTypes = new List<WorldEventType> { WorldEventType.RareSpawn, WorldEventType.Portal } },
                new WorldEventLocation { Name = "Sacred Grove", MinLevel = 5, PreferredEventTypes = new List<WorldEventType> { WorldEventType.Blessing, WorldEventType.MerchantVisit } }
            };
        }
        
        private static void InitializeTimeSpawnModifiers()
        {
            TimeSpawnModifiers = new Dictionary<TimePeriod, SpawnModifier>
            {
                [TimePeriod.Dawn] = new SpawnModifier { Multiplier = 1.2f, PreferredEventTypes = new List<WorldEventType> { WorldEventType.Blessing, WorldEventType.TreasureSpawn } },
                [TimePeriod.Morning] = new SpawnModifier { Multiplier = 1.0f, PreferredEventTypes = new List<WorldEventType> { WorldEventType.MerchantVisit, WorldEventType.ResourceBurst } },
                [TimePeriod.Afternoon] = new SpawnModifier { Multiplier = 0.8f, PreferredEventTypes = new List<WorldEventType> { WorldEventType.MonsterSurge } },
                [TimePeriod.Evening] = new SpawnModifier { Multiplier = 1.5f, PreferredEventTypes = new List<WorldEventType> { WorldEventType.Curse, WorldEventType.RareSpawn } },
                [TimePeriod.Night] = new SpawnModifier { Multiplier = 2.0f, PreferredEventTypes = new List<WorldEventType> { WorldEventType.Portal, WorldEventType.RareSpawn, WorldEventType.Curse } },
                [TimePeriod.Midnight] = new SpawnModifier { Multiplier = 1.8f, PreferredEventTypes = new List<WorldEventType> { WorldEventType.RareSpawn, WorldEventType.Portal } }
            };
        }
        
        /// <summary>
        /// Get current time period
        /// </summary>
        public static TimePeriod GetCurrentTimePeriod()
        {
            int hour = DateTime.Now.Hour;
            
            if (hour >= 5 && hour < 7) return TimePeriod.Dawn;
            if (hour >= 7 && hour < 12) return TimePeriod.Morning;
            if (hour >= 12 && hour < 17) return TimePeriod.Afternoon;
            if (hour >= 17 && hour < 20) return TimePeriod.Evening;
            if (hour >= 20 && hour < 24) return TimePeriod.Night;
            return TimePeriod.Midnight;
        }
        
        /// <summary>
        /// Get location by name
        /// </summary>
        public static WorldEventLocation GetLocation(string name)
        {
            return Locations.Find(l => l.Name == name);
        }
        
        /// <summary>
        /// Get random location suitable for player level
        /// </summary>
        public static WorldEventLocation GetRandomLocation(int playerLevel)
        {
            var suitable = Locations.FindAll(l => l.MinLevel <= playerLevel);
            if (suitable.Count == 0) return Locations[0];
            return suitable[new Random().Next(suitable.Count)];
        }
    }
    
    // Supporting classes
    public class WorldEventTypeConfig
    {
        public WorldEventType Type { get; set; }
        public int BaseDuration { get; set; }
        public int BaseGoldReward { get; set; }
        public int BaseExpReward { get; set; }
        public int MinPlayers { get; set; }
        public int MaxPlayers { get; set; }
        public string Icon { get; set; }
        public string Color { get; set; }
    }
    
    public class RarityConfig
    {
        public WorldEventRarity Rarity { get; set; }
        public double BaseChance { get; set; }
        public float GoldMultiplier { get; set; }
        public float ExpMultiplier { get; set; }
        public string Color { get; set; }
        public string DisplayName { get; set; }
    }
    
    public class WorldEventLocation
    {
        public string Name { get; set; }
        public int MinLevel { get; set; }
        public List<WorldEventType> PreferredEventTypes { get; set; }
    }
    
    public class SpawnModifier
    {
        public float Multiplier { get; set; }
        public List<WorldEventType> PreferredEventTypes { get; set; }
    }
    
    public enum TimePeriod
    {
        Dawn,      // 5-7
        Morning,   // 7-12
        Afternoon, // 12-17
        Evening,   // 17-20
        Night,     // 20-24
        Midnight   // 0-5
    }
}
