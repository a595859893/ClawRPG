using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Elite Monster Database - Configuration for elite monster types, tiers, and spawn conditions
    /// </summary>
    public partial class EliteMonsterDatabase : BaseSystem
    {
        // Singleton
        public static EliteMonsterDatabase Instance { get; private set; }
        
        public override void _Ready()
        {
            Instance = this;
        }
        
        // Elite type configurations
        public Dictionary<EliteMonsterData.EliteType, EliteTypeConfig> EliteTypeConfigs = new()
        {
            { EliteMonsterData.EliteType.Champion, new EliteTypeConfig
            {
                Name = "Champion",
                Description = "Enhanced stats, balanced combatant",
                BaseHealthBonus = 2.0f,
                BaseAttackBonus = 1.5f,
                BaseDefenseBonus = 1.5f,
                BaseSpeedBonus = 1.2f,
                DropRateBonus = 1.5f,
                ColorHex = "#FFD700",
                Abilities = new List<string> { "Power Strike", "Battle Cry" }
            }},
            { EliteMonsterData.EliteType.Boss, new EliteTypeConfig
            {
                Name = "Boss",
                Description = "Mini-boss with special abilities",
                BaseHealthBonus = 3.0f,
                BaseAttackBonus = 2.0f,
                BaseDefenseBonus = 2.0f,
                BaseSpeedBonus = 1.0f,
                DropRateBonus = 2.0f,
                ColorHex = "#FF4444",
                Abilities = new List<string> { "Enrage", "Smash", "Summon Minions" }
            }},
            { EliteMonsterData.EliteType.Rogue, new EliteTypeConfig
            {
                Name = "Rogue",
                Description = "High mobility and critical strikes",
                BaseHealthBonus = 1.5f,
                BaseAttackBonus = 2.5f,
                BaseDefenseBonus = 1.0f,
                BaseSpeedBonus = 2.0f,
                DropRateBonus = 1.8f,
                ColorHex = "#9932CC",
                Abilities = new List<string> { "Critical Strike", "Dash Attack", "Backstab" }
            }},
            { EliteMonsterData.EliteType.Tank, new EliteTypeConfig
            {
                Name = "Tank",
                Description = "High defense and HP, slow but durable",
                BaseHealthBonus = 4.0f,
                BaseAttackBonus = 1.2f,
                BaseDefenseBonus = 3.0f,
                BaseSpeedBonus = 0.7f,
                DropRateBonus = 1.5f,
                ColorHex = "#4169E1",
                Abilities = new List<string> { "Shield Wall", "Taunt", "Fortify" }
            }},
            { EliteMonsterData.EliteType.Mage, new EliteTypeConfig
            {
                Name = "Mage",
                Description = "Elemental magic attacks",
                BaseHealthBonus = 1.5f,
                BaseAttackBonus = 2.5f,
                BaseDefenseBonus = 1.0f,
                BaseSpeedBonus = 1.3f,
                DropRateBonus = 2.0f,
                ColorHex = "#00CED1",
                Abilities = new List<string> { "Fireball", "Ice Blast", "Lightning Strike" }
            }},
            { EliteMonsterData.EliteType.Assassin, new EliteTypeConfig
            {
                Name = "Assassin",
                Description = "High burst damage, stealth abilities",
                BaseHealthBonus = 1.2f,
                BaseAttackBonus = 3.0f,
                BaseDefenseBonus = 0.8f,
                BaseSpeedBonus = 2.5f,
                DropRateBonus = 2.5f,
                ColorHex = "#2F4F4F",
                Abilities = new List<string> { "Shadow Strike", "Poison Blade", "Vanish" }
            }},
            { EliteMonsterData.EliteType.Healer, new EliteTypeConfig
            {
                Name = "Healer",
                Description = "Regenerates allies, support role",
                BaseHealthBonus = 2.0f,
                BaseAttackBonus = 1.0f,
                BaseDefenseBonus = 1.5f,
                BaseSpeedBonus = 1.2f,
                DropRateBonus = 1.8f,
                ColorHex = "#98FB98",
                Abilities = new List<string> { "Healing Aura", "Group Heal", "Resurrection" }
            }},
            { EliteMonsterData.EliteType.Brute, new EliteTypeConfig
            {
                Name = "Brute",
                Description = "Area of effect attacks",
                BaseHealthBonus = 3.5f,
                BaseAttackBonus = 2.2f,
                BaseDefenseBonus = 1.8f,
                BaseSpeedBonus = 0.9f,
                DropRateBonus = 1.6f,
                ColorHex = "#8B4513",
                Abilities = new List<string> { "Cleave", "Earthquake", "Rage" }
            }},
            { EliteMonsterData.EliteType.Swift, new EliteTypeConfig
            {
                Name = "Swift",
                Description = "Fast attack speed, multiple hits",
                BaseHealthBonus = 1.3f,
                BaseAttackBonus = 2.0f,
                BaseDefenseBonus = 1.0f,
                BaseSpeedBonus = 3.0f,
                DropRateBonus = 1.7f,
                ColorHex = "#FF8C00",
                Abilities = new List<string> { "Double Strike", "Whirlwind", "Speed Boost" }
            }},
            { EliteMonsterData.EliteType.Ancient, new EliteTypeConfig
            {
                Name = "Ancient",
                Description = "Rare, incredibly powerful",
                BaseHealthBonus = 5.0f,
                BaseAttackBonus = 3.0f,
                BaseDefenseBonus = 3.0f,
                BaseSpeedBonus = 1.5f,
                DropRateBonus = 5.0f,
                ColorHex = "#FFD700",
                Abilities = new List<string> { "Time Stop", "Meteor Storm", "Ultimate Defense" }
            }}
        };
        
        // Elite tier configurations
        public Dictionary<EliteMonsterData.EliteTier, EliteTierConfig> TierConfigs = new()
        {
            { EliteMonsterData.EliteTier.Normal, new EliteTierConfig
            {
                Name = "Normal",
                StatMultiplier = 1.5f,
                SpawnChance = 0.10f,
                RarityName = "Common"
            }},
            { EliteMonsterData.EliteTier.Rare, new EliteTierConfig
            {
                Name = "Rare",
                StatMultiplier = 2.0f,
                SpawnChance = 0.03f,
                RarityName = "Uncommon"
            }},
            { EliteMonsterData.EliteTier.Epic, new EliteTierConfig
            {
                Name = "Epic",
                StatMultiplier = 3.0f,
                SpawnChance = 0.01f,
                RarityName = "Rare"
            }},
            { EliteMonsterData.EliteTier.Legendary, new EliteTierConfig
            {
                Name = "Legendary",
                StatMultiplier = 5.0f,
                SpawnChance = 0.002f,
                RarityName = "Epic"
            }}
        };
        
        // Spawn condition configurations
        public Dictionary<string, SpawnConditionConfig> SpawnConditions = new()
        {
            { "floor_5", new SpawnConditionConfig { MinFloor = 5, ChanceBonus = 0.02f } },
            { "floor_10", new SpawnConditionConfig { MinFloor = 10, ChanceBonus = 0.03f } },
            { "floor_20", new SpawnConditionConfig { MinFloor = 20, ChanceBonus = 0.05f } },
            { "player_level_10", new SpawnConditionConfig { MinPlayerLevel = 10, ChanceBonus = 0.02f } },
            { "player_level_20", new SpawnConditionConfig { MinPlayerLevel = 20, ChanceBonus = 0.03f } },
            { "time_elapsed_300", new SpawnConditionConfig { MinTimeSeconds = 300, ChanceBonus = 0.01f } },
            { "combo_5", new SpawnConditionConfig { MinCombo = 5, ChanceBonus = 0.02f } },
            { "critical_kill", new SpawnConditionConfig { RequireCriticalKill = true, ChanceBonus = 0.05f } }
        };
        
        // Base spawn chance
        public float BaseSpawnChance => 0.08f;
        
        // Maximum elite monsters per encounter
        public int MaxElitePerEncounter => 3;
        
        // Get elite type by weight
        public EliteMonsterData.EliteType GetRandomEliteType()
        {
            var weights = new Dictionary<EliteMonsterData.EliteType, float>
            {
                { EliteMonsterData.EliteType.Champion, 25.0f },
                { EliteMonsterData.EliteType.Boss, 15.0f },
                { EliteMonsterData.EliteType.Rogue, 12.0f },
                { EliteMonsterData.EliteType.Tank, 10.0f },
                { EliteMonsterData.EliteType.Mage, 10.0f },
                { EliteMonsterData.EliteType.Assassin, 8.0f },
                { EliteMonsterData.EliteType.Healer, 8.0f },
                { EliteMonsterData.EliteType.Brute, 7.0f },
                { EliteMonsterData.EliteType.Swift, 4.0f },
                { EliteMonsterData.EliteType.Ancient, 1.0f }
            };
            
            return GetWeightedRandom(weights);
        }
        
        // Get elite tier by weight
        public EliteMonsterData.EliteTier GetRandomEliteTier()
        {
            var weights = new Dictionary<EliteMonsterData.EliteTier, float>
            {
                { EliteMonsterData.EliteTier.Normal, 70.0f },
                { EliteMonsterData.EliteTier.Rare, 20.0f },
                { EliteMonsterData.EliteTier.Epic, 8.0f },
                { EliteMonsterData.EliteTier.Legendary, 2.0f }
            };
            
            return GetWeightedRandom(weights);
        }
        
        private EliteMonsterData.EliteType GetWeightedRandom(Dictionary<EliteMonsterData.EliteType, float> weights)
        {
            float total = 0;
            foreach (var w in weights.Values) total += w;
            
            float random = (float)GD.RandDouble() * total;
            float cumulative = 0;
            
            foreach (var kvp in weights)
            {
                cumulative += kvp.Value;
                if (random <= cumulative) return kvp.Key;
            }
            
            return EliteMonsterData.EliteType.Champion;
        }
        
        private EliteMonsterData.EliteTier GetWeightedRandom(Dictionary<EliteMonsterData.EliteTier, float> weights)
        {
            float total = 0;
            foreach (var w in weights.Values) total += w;
            
            float random = (float)GD.RandDouble() * total;
            float cumulative = 0;
            
            foreach (var kvp in weights)
            {
                cumulative += kvp.Value;
                if (random <= cumulative) return kvp.Key;
            }
            
            return EliteMonsterData.EliteTier.Normal;
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            // EliteMonsterDatabase 是静态配置数据，不需要持久化
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            // EliteMonsterDatabase 是静态配置数据，不需要持久化
        }
    }
    
    public class EliteTypeConfig
    {
        public string Name;
        public string Description;
        public float BaseHealthBonus;
        public float BaseAttackBonus;
        public float BaseDefenseBonus;
        public float BaseSpeedBonus;
        public float DropRateBonus;
        public string ColorHex;
        public List<string> Abilities = new();
    }
    
    public class EliteTierConfig
    {
        public string Name;
        public float StatMultiplier;
        public float SpawnChance;
        public string RarityName;
    }
    
    public class SpawnConditionConfig
    {
        public int MinFloor = 0;
        public int MinPlayerLevel = 0;
        public int MinTimeSeconds = 0;
        public int MinCombo = 0;
        public bool RequireCriticalKill = false;
        public float ChanceBonus = 0;
    }
}
