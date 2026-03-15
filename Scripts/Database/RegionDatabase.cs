using Godot;
using System;
using System.Collections.Generic;
using Framework;

namespace ClawRPG.Scripts.Database {
    /// <summary>
    /// Represents a game region/zone with its properties
    /// </summary>
    [GodotClass]
    public class RegionType : Resource
    {
        [Export] public string RegionId { get; set; } = "";
        [Export] public string RegionName { get; set; } = "";
        [Export] public string Description { get; set; } = "";
        [Export] public int RequiredLevel { get; set; } = 1;
        [Export] public Color RegionColor { get; set; } = Colors.Green;
        [Export] public Vector2 MapPosition { get; set; } = Vector2.Zero;
        [Export] public string[] EnemyTypes { get; set; } = Array.Empty<string>();
        [Export] public string[] AvailableQuests { get; set; } = Array.Empty<string>();
        [Export] public string[] Shops { get; set; } = Array.Empty<string>();
        
        // Region modifiers
        [Export] public float DamageMultiplier { get; set; } = 1.0f;
        [Export] public float DefenseMultiplier { get; set; } = 1.0f;
        [Export] public float ExpMultiplier { get; set; } = 1.0f;
        [Export] public float DropRateMultiplier { get; set; } = 1.0f;
        
        // Environmental effects
        [Export] public bool HasPoisonFog { get; set; } = false; 
        [Export] public bool HasFireDamage { get; set; } = false; 
        [Export] public bool HasIceDamage { get; set; } = false; 
        [Export] public float EnvironmentalDamagePerSecond { get; set; } = 0f;
    }

    /// <summary>
    /// Database managing all game regions/zones
    /// </summary>
    [GodotClass]
    public class RegionDatabase : BaseSystem
    {
        public static RegionDatabase Instance { get; private set; }

        private Dictionary<string, RegionType> _regions = new Dictionary<string, RegionType>();

        public override void _Ready()
        {
            Instance = this;
            InitializeRegions();
        }

        private void InitializeRegions()
        {
            // Forest Region - Starting area
            CreateRegion("forest", "暮光森林", "新手区域，充满平和的氛围", 1, new Color(0.2f, 0.8f, 0.2f),
                new Vector2(200, 300), 
                new[] { "goblin", "wolf", "slime", "bat", "spider" },
                new[] { "quest_forest_1", "quest_goblin_hunter" },
                new[] { "forest_shop" },
                1.0f, 1.0f, 1.0f, 1.0f);

            // Cave Region - Level 2+
            CreateRegion("cave", "幽暗洞穴", "地下洞穴系统，栖息着危险的生物", 2, new Color(0.4f, 0.3f, 0.2f),
                new Vector2(400, 250),
                new[] { "skeleton", "cave_spider", "rock_golem", "dark_bat" },
                new[] { "quest_cave_1", "quest_ancient_treasure" },
                new[] { "cave_shop" },
                1.0f, 1.0f, 1.2f, 1.1f);

            // Fire Dungeon - Level 3+
            CreateRegion("fire_dungeon", "烈焰地牢", "炽热的地下要塞，充满火焰元素", 3, new Color(0.9f, 0.3f, 0.1f),
                new Vector2(600, 350),
                new[] { "fire_element", "magma_golem", "fire_imp", "hell_hound" },
                new[] { "quest_fire_1", "quest_demons_1" },
                new[] { "fire_shop" },
                1.2f, 0.9f, 1.5f, 1.3f, false, true, false, 2.0f);

            // Ice Dungeon - Level 4+
            CreateRegion("ice_dungeon", "冰霜地牢", "寒冷的冰雪世界，危机四伏", 4, new Color(0.5f, 0.8f, 1.0f),
                new Vector2(300, 450),
                new[] { "frost_wraith", "ice_golem", "snow_wolf", "frost_giant" },
                new[] { "quest_ice_1", "quest_ancient_treasure" },
                new[] { "ice_shop" },
                1.1f, 1.1f, 1.4f, 1.2f, false, false, true, 1.5f);

            // Shadow Dungeon - Level 5+
            CreateRegion("shadow_dungeon", "暗影地牢", "被黑暗力量侵蚀的区域", 5, new Color(0.3f, 0.1f, 0.5f),
                new Vector2(500, 400),
                new[] { "shadow_elf", "dark_knight", "shadow_assassin", "void_beast" },
                new[] { "quest_shadow_1", "quest_dark_wizard" },
                new[] { "shadow_shop" },
                1.3f, 0.8f, 1.6f, 1.4f, true, false, false, 3.0f);

            // Dragon Lair - Level 6+ (Boss territory)
            CreateRegion("dragon_lair", "巨龙巢穴", "传说中巨龙的栖息地", 6, new Color(0.6f, 0.2f, 0.1f),
                new Vector2(700, 200),
                new[] { "dragon_whelp", "dragon_guardian" },
                new[] { "quest_dragon_1" },
                new[] { "dragon_shop" },
                1.5f, 0.7f, 2.0f, 1.5f, false, true, false, 5.0f);

            // Holy Temple - Level 7+ (Endgame)
            CreateRegion("holy_temple", "神圣殿堂", "古老的神殿遗迹，充满圣光", 7, new Color(1.0f, 0.9f, 0.5f),
                new Vector2(150, 150),
                new[] { "holy_knight", "celestial_guardian", "light_elemental" },
                new[] { "quest_holy_1", "quest_holy_temple" },
                new[] { "temple_shop" },
                0.8f, 1.3f, 1.8f, 1.6f);

            GD.Print($"[RegionDatabase] Initialized {_regions.Count} regions");
        }

        private void CreateRegion(string id, string name, string description, int level, Color color,
            Vector2 mapPos, string[] enemies, string[] quests, string[] shops,
            float dmgMult, float defMult, float expMult, float dropMult,
            bool poisonFog = false, bool fireDamage = false, bool iceDamage = false, float envDmg = 0)
        {
            var region = new RegionType
            {
                RegionId = id,
                RegionName = name,
                Description = description,
                RequiredLevel = level,
                RegionColor = color,
                MapPosition = mapPos,
                EnemyTypes = enemies,
                AvailableQuests = quests,
                Shops = shops,
                DamageMultiplier = dmgMult,
                DefenseMultiplier = defMult,
                ExpMultiplier = expMult,
                DropRateMultiplier = dropMult,
                HasPoisonFog = poisonFog,
                HasFireDamage = fireDamage,
                HasIceDamage = iceDamage,
                EnvironmentalDamagePerSecond = envDmg
            };

            _regions[id] = region;
        }

        public RegionType GetRegion(string regionId)
        {
            if (_regions.TryGetValue(regionId, out var region))
                return region;
            
            GD.Warning($"[RegionDatabase] Region not found: {regionId}");
            return null;
        }

        public RegionType GetRegionByName(string regionName)
        {
            foreach (var region in _regions.Values)
            {
                if (region.RegionName == regionName)
                    return region;
            }
            return null;
        }

        public Dictionary<string, RegionType> GetAllRegions()
        {
            return new Dictionary<string, RegionType>(_regions);
        }

        public RegionType[] GetRegionsForLevel(int playerLevel)
        {
            var result = new List<RegionType>();
            foreach (var region in _regions.Values)
            {
                if (region.RequiredLevel <= playerLevel)
                    result.Add(region);
            }
            return result.ToArray();
        }

        public string[] GetUnlockedRegionIds(int playerLevel)
        {
            var result = new List<string>();
            foreach (var region in _regions.Values)
            {
                if (region.RequiredLevel <= playerLevel)
                    result.Add(region.RegionId);
            }
            return result.ToArray();
        }
    }
}
