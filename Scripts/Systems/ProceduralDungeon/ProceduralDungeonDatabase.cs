using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ProceduralDungeon
{
    /// <summary>
    /// 程序化地下城配置数据库
    /// </summary>
    public class ProceduralDungeonDatabase
    {
        private static ProceduralDungeonDatabase _instance;
        public static ProceduralDungeonDatabase Instance => _instance ?? (_instance = new ProceduralDungeonDatabase());
        
        // 地下城类型配置
        public Dictionary<string, DungeonTypeConfig> DungeonTypes { get; private set; }
        
        // 房间模板配置
        public Dictionary<RoomType, List<RoomTemplate>> RoomTemplates { get; private set; }
        
        // 敌人波次配置
        public List<EnemyWaveConfig> EnemyWaves { get; private set; }
        
        // 宝藏配置
        public List<TreasureConfig> Treasures { get; private set; }
        
        // 事件配置
        public List<DungeonEventConfig> Events { get; private set; }
        
        // 楼层配置
        public List<DungeonFloorConfig> FloorConfigs { get; private set; }
        
        public ProceduralDungeonDatabase()
        {
            InitializeDungeonTypes();
            InitializeRoomTemplates();
            InitializeEnemyWaves();
            InitializeTreasures();
            InitializeEvents();
            InitializeFloorConfigs();
        }
        
        private void InitializeDungeonTypes()
        {
            DungeonTypes = new Dictionary<string, DungeonTypeConfig>
            {
                ["AncientRuins"] = new DungeonTypeConfig
                {
                    TypeId = "AncientRuins",
                    DisplayName = "Ancient Ruins",
                    Description = "Crumbling ruins of an ancient civilization",
                    TotalFloors = 5,
                    RoomScaleFactor = 1.0f,
                    AllowedRoomTypes = new List<RoomType> 
                    { 
                        RoomType.Combat, RoomType.Treasure, RoomType.Elite, 
                        RoomType.Puzzle, RoomType.Rest, RoomType.Secret, RoomType.Event 
                    },
                    ThemeModifier = 1.2f,
                    TreasureChance = 0.3f,
                    SecretChance = 0.15f
                },
                ["DeepCavern"] = new DungeonTypeConfig
                {
                    TypeId = "DeepCavern",
                    DisplayName = "Deep Cavern",
                    Description = "Dark caverns deep beneath the surface",
                    TotalFloors = 7,
                    RoomScaleFactor = 1.5f,
                    AllowedRoomTypes = new List<RoomType>
                    {
                        RoomType.Combat, RoomType.Elite, RoomType.Boss,
                        RoomType.Rest, RoomType.Trap, RoomType.Event
                    },
                    ThemeModifier = 1.5f,
                    TreasureChance = 0.25f,
                    SecretChance = 0.1f
                },
                ["ForgottenTemple"] = new DungeonTypeConfig
                {
                    TypeId = "ForgottenTemple",
                    DisplayName = "Forgotten Temple",
                    Description = "A temple lost to time, guarded by mysteries",
                    TotalFloors = 4,
                    RoomScaleFactor = 0.8f,
                    AllowedRoomTypes = new List<RoomType>
                    {
                        RoomType.Combat, RoomType.Treasure, RoomType.Elite,
                        RoomType.Boss, RoomType.Puzzle, RoomType.Merchant, RoomType.Secret
                    },
                    ThemeModifier = 1.3f,
                    TreasureChance = 0.4f,
                    SecretChance = 0.2f
                },
                ["AbandonedFortress"] = new DungeonTypeConfig
                {
                    TypeId = "AbandonedFortress",
                    DisplayName = "Abandoned Fortress",
                    Description = "An old fortress now filled with monsters",
                    TotalFloors = 6,
                    RoomScaleFactor = 1.2f,
                    AllowedRoomTypes = new List<RoomType>
                    {
                        RoomType.Combat, RoomType.Treasure, RoomType.Elite,
                        RoomType.Boss, RoomType.Rest, RoomType.Trap, RoomType.Event
                    },
                    ThemeModifier = 1.1f,
                    TreasureChance = 0.35f,
                    SecretChance = 0.12f
                },
                ["EnchantedForest"] = new DungeonTypeConfig
                {
                    TypeId = "EnchantedForest",
                    DisplayName = "Enchanted Forest",
                    Description = "A mystical forest with magical creatures",
                    TotalFloors = 3,
                    RoomScaleFactor = 1.0f,
                    AllowedRoomTypes = new List<RoomType>
                    {
                        RoomType.Combat, RoomType.Treasure, RoomType.Rest,
                        RoomType.Secret, RoomType.Event, RoomType.Merchant
                    },
                    ThemeModifier = 1.0f,
                    TreasureChance = 0.3f,
                    SecretChance = 0.25f
                }
            };
        }
        
        private void InitializeRoomTemplates()
        {
            RoomTemplates = new Dictionary<RoomType, List<RoomTemplate>>();
            
            // Combat rooms
            RoomTemplates[RoomType.Combat] = new List<RoomTemplate>
            {
                new RoomTemplate { TemplateId = "combat_arena", Width = 20, Height = 20, Description = "Open arena combat" },
                new RoomTemplate { TemplateId = "combat_corridor", Width = 30, Height = 10, Description = "Narrow corridor fight" },
                new RoomTemplate { TemplateId = "combat_chambers", Width = 25, Height = 25, Description = "Multiple chambers" },
                new RoomTemplate { TemplateId = "combat_pillars", Width = 20, Height = 20, Description = "Pillar obstacles" }
            };
            
            // Treasure rooms
            RoomTemplates[RoomType.Treasure] = new List<RoomTemplate>
            {
                new RoomTemplate { TemplateId = "treasure_vault", Width = 15, Height = 15, Description = "Treasure vault" },
                new RoomTemplate { TemplateId = "treasure_cache", Width = 10, Height = 10, Description = "Hidden cache" }
            };
            
            // Elite rooms
            RoomTemplates[RoomType.Elite] = new List<RoomTemplate>
            {
                new RoomTemplate { TemplateId = "elite_arena", Width = 25, Height = 25, Description = "Elite battle arena" }
            };
            
            // Boss rooms
            RoomTemplates[RoomType.Boss] = new List<RoomTemplate>
            {
                new RoomTemplate { TemplateId = "boss_throne", Width = 30, Height = 30, Description = "Throne room" },
                new RoomTemplate { TemplateId = "boss_arena", Width = 35, Height = 35, Description = "Grand arena" }
            };
            
            // Other types default to basic template
            for (int i = 0; i < Enum.GetValues(typeof(RoomType)).Length; i++)
            {
                var roomType = (RoomType)i;
                if (!RoomTemplates.ContainsKey(roomType))
                {
                    RoomTemplates[roomType] = new List<RoomTemplate>
                    {
                        new RoomTemplate { TemplateId = $"{roomType.ToString().ToLower()}_default`, Width = 15, Height = 15, Description = $"{roomType} room" }
                    };
                }
            }
        }
        
        private void InitializeEnemyWaves()
        {
            EnemyWaves = new List<EnemyWaveConfig>
            {
                new EnemyWaveConfig { WaveId = "wave_basic", EnemyCount = 3, Difficulty = RoomDifficulty.Easy },
                new EnemyWaveConfig { WaveId = "wave_standard", EnemyCount = 5, Difficulty = RoomDifficulty.Normal },
                new EnemyWaveConfig { WaveId = "wave_heavy", EnemyCount = 7, Difficulty = RoomDifficulty.Hard },
                new EnemyWaveConfig { WaveId = "wave_elite", EnemyCount = 3, Difficulty = RoomDifficulty.Nightmare },
                new EnemyWaveConfig { WaveId = "wave_boss", EnemyCount = 1, Difficulty = RoomDifficulty.Legendary }
            };
        }
        
        private void InitializeTreasures()
        {
            Treasures = new List<TreasureConfig>
            {
                new TreasureConfig { TreasureId = "gold_chest", Type = "Gold", MinValue = 100, MaxValue = 500, Rarity = 0.5f },
                new TreasureConfig { TreasureId = "equipment_chest", Type = "Equipment", MinValue = 1, MaxValue = 1, Rarity = 0.3f },
                new TreasureConfig { TreasureId = "artifact_chest", Type = "Artifact", MinValue = 1, MaxValue = 1, Rarity = 0.15f },
                new TreasureConfig { TreasureId = "legendary_chest", Type = "Legendary", MinValue = 1, MaxValue = 1, Rarity = 0.05f }
            };
        }
        
        private void InitializeEvents()
        {
            Events = new List<DungeonEventConfig>
            {
                new DungeonEventConfig { EventId = "mysterious_merchant", DisplayName = "Mysterious Merchant", Description = "A wandering merchant appears", Type = "positive" },
                new DungeonEventConfig { EventId = "treasure_trap", DisplayName = "Treasure Trap", Description = "A trap springs!", Type = "negative" },
                new DungeonEventConfig { EventId = "blessing", DisplayName = "Divine Blessing", Description = "You receive a blessing", Type = "positive" },
                new DungeonEventConfig { EventId = "ambush", DisplayName = "Ambush", Description = "Enemies ambush you!", Type = "negative" },
                new DungeonEventConfig { EventId = "mystery_box", DisplayName = "Mystery Box", Description = "A mysterious box appears", Type = "neutral" }
            };
        }
        
        private void InitializeFloorConfigs()
        {
            FloorConfigs = new List<DungeonFloorConfig>
            {
                new DungeonFloorConfig 
                { 
                    FloorNumber = 1, FloorName = "Upper Level", 
                    MinRooms = 5, MaxRooms = 8, RoomScaleFactor = 1.0f,
                    EnemyStrengthMultiplier = 1.0f, TreasureMultiplier = 1.0f 
                },
                new DungeonFloorConfig 
                { 
                    FloorNumber = 2, FloorName = "Middle Level", 
                    MinRooms = 6, MaxRooms = 10, RoomScaleFactor = 1.1f,
                    EnemyStrengthMultiplier = 1.2f, TreasureMultiplier = 1.2f 
                },
                new DungeonFloorConfig 
                { 
                    FloorNumber = 3, FloorName = "Lower Level", 
                    MinRooms = 7, MaxRooms = 12, RoomScaleFactor = 1.2f,
                    EnemyStrengthMultiplier = 1.4f, TreasureMultiplier = 1.4f 
                },
                new DungeonFloorConfig 
                { 
                    FloorNumber = 4, FloorName = "Deep Level", 
                    MinRooms = 8, MaxRooms = 14, RoomScaleFactor = 1.3f,
                    EnemyStrengthMultiplier = 1.6f, TreasureMultiplier = 1.6f 
                },
                new DungeonFloorConfig 
                { 
                    FloorNumber = 5, FloorName = "Boss Level", 
                    MinRooms = 3, MaxRooms = 5, RoomScaleFactor = 1.5f,
                    EnemyStrengthMultiplier = 2.0f, TreasureMultiplier = 2.0f 
                }
            };
        }
        
        public DungeonTypeConfig GetDungeonType(string typeId)
        {
            return DungeonTypes.ContainsKey(typeId) ? DungeonTypes[typeId] : null;
        }
        
        public List<RoomTemplate> GetRoomTemplates(RoomType type)
        {
            return RoomTemplates.ContainsKey(type) ? RoomTemplates[type] : RoomTemplates[RoomType.Combat];
        }
    }
    
    /// <summary>
    /// 地下城类型配置
    /// </summary>
    public class DungeonTypeConfig
    {
        public string TypeId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public int TotalFloors { get; set; }
        public float RoomScaleFactor { get; set; }
        public List<RoomType> AllowedRoomTypes { get; set; }
        public float ThemeModifier { get; set; }
        public float TreasureChance { get; set; }
        public float SecretChance { get; set; }
    }
    
    /// <summary>
    /// 房间模板
    /// </summary>
    public class RoomTemplate
    {
        public string TemplateId { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string Description { get; set; }
    }
    
    /// <summary>
    /// 敌人波次配置
    /// </summary>
    public class EnemyWaveConfig
    {
        public string WaveId { get; set; }
        public int EnemyCount { get; set; }
        public RoomDifficulty Difficulty { get; set; }
    }
    
    /// <summary>
    /// 宝藏配置
    /// </summary>
    public class TreasureConfig
    {
        public string TreasureId { get; set; }
        public string Type { get; set; }
        public int MinValue { get; set; }
        public int MaxValue { get; set; }
        public float Rarity { get; set; }
    }
    
    /// <summary>
    /// 地下城事件配置
    /// </summary>
    public class DungeonEventConfig
    {
        public string EventId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }  // positive, negative, neutral
    }
}
