using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Bosses {
    /// <summary>
    /// Boss definition data class
    /// </summary>
    [GodotClass]
    public class BossData : Object
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Title { get; set; }
        public int MaxHealth { get; set; }
        public float MoveSpeed { get; set; }
        public float AttackDamage { get; set; }
        public float AttackRange { get; set; }
        public float AttackCooldown { get; set; }
        public float ChaseRange { get; set; }
        public float DetectionRange { get; set; }
        public int ExperienceReward { get; set; }
        public string[] DropItems { get; set; }
        public int PhaseCount { get; set; }
        public int[] PhaseHealthThresholds { get; set; }
        public float EnrageTime { get; set; }
        public float AbilityCooldown { get; set; }
        public string[] SpecialAbilities { get; set; }
        public string Description { get; set; }
        public string SpritePath { get; set; }
        
        public BossData() { }
        
        public BossData(string id, string name, string title, int health, float speed, float damage)
        {
            Id = id;
            Name = name;
            Title = title;
            MaxHealth = health;
            MoveSpeed = speed;
            AttackDamage = damage;
            AttackRange = 60f;
            AttackCooldown = 1.5f;
            ChaseRange = 250f;
            DetectionRange = 400f;
            ExperienceReward = 500;
            PhaseCount = 3;
            PhaseHealthThresholds = new int[] { 66, 33 };
            EnrageTime = 120f;
            AbilityCooldown = 10f;
        }
    }
    
    /// <summary>
    /// Database of all bosses in the game
    /// </summary>
    public static class BossDatabase
    {
        private static Dictionary<string, BossData> _bosses;
        
        static BossDatabase()
        {
            Initialize();
        }
        
        private static void Initialize()
        {
            _bosses = new Dictionary<string, BossData>();
            
            // Forest Boss - Ancient Treant
            _bosses["treant_king"] = new BossData(
                id: "treant_king",
                name: "TreantKing",
                title: "Ancient Treant",
                health: 2000,
                speed: 60f,
                damage: 35f
            )
            {
                Description = "Guardian of the Whispering Forest",
                PhaseCount = 3,
                PhaseHealthThresholds = new int[] { 66, 33 },
                EnrageTime = 150f,
                AbilityCooldown = 12f,
                SpecialAbilities = new string[] { "ground_slam", "summon_minions", "heal" },
                ExperienceReward = 800,
                DropItems = new string[] { "treant_heart", "ancient_essence" }
            };
            
            // Cave Boss - Crystal Golem
            _bosses["crystal_golem"] = new BossData(
                id: "crystal_golem",
                name: "CrystalGolem",
                title: "Crystal Golem",
                health: 3000,
                speed: 50f,
                damage: 45f
            )
            {
                Description = "Guardian of the Crystal Caverns",
                PhaseCount = 2,
                PhaseHealthThresholds = new int[] { 50 },
                EnrageTime = 180f,
                AbilityCooldown = 8f,
                SpecialAbilities = new string[] { "area_attack", "teleport", "heal" },
                ExperienceReward = 1000,
                DropItems = new string[] { "crystal_shard", "golem_core" }
            };
            
            // Fire Boss - Inferno Dragon
            _bosses["inferno_dragon"] = new BossData(
                id: "inferno_dragon",
                name: "InfernoDragon",
                title: "Inferno Dragon",
                health: 5000,
                speed: 80f,
                damage: 60f
            )
            {
                Description = "The Unholy Flame Incarnate",
                PhaseCount = 3,
                PhaseHealthThresholds = new int[] { 70, 40 },
                EnrageTime = 120f,
                AbilityCooldown = 6f,
                SpecialAbilities = new string[] { "fire_breath", "ground_slam", "teleport", "area_attack" },
                ExperienceReward = 1500,
                DropItems = new string[] { "dragon_heart", "fire_essence", "dragon_scale" }
            };
            
            // Shadow Boss - Dark Assassin
            _bosses["dark_assassin"] = new BossData(
                id: "dark_assassin",
                name: "DarkAssassin",
                title: "Shadow of the Night",
                health: 2500,
                speed: 150f,
                damage: 80f
            )
            {
                Description = "A deadly killer from the Shadow Realm",
                PhaseCount = 3,
                PhaseHealthThresholds = new int[] { 66, 33 },
                EnrageTime = 90f,
                AbilityCooldown = 5f,
                SpecialAbilities = new string[] { "teleport", "area_attack", "summon_minions" },
                ExperienceReward = 1200,
                DropItems = new string[] { "shadow_essence", "assassin_dagger" }
            };
            
            // Ice Boss - Frost Wyrm
            _bosses["frost_wyrm"] = new BossData(
                id: "frost_wyrm",
                name: "FrostWyrm",
                title: "The Eternal Winter",
                health: 4000,
                speed: 70f,
                damage: 50f
            )
            {
                Description = "An ancient dragon of ice and snow",
                PhaseCount = 3,
                PhaseHealthThresholds = new int[] { 65, 30 },
                EnrageTime = 150f,
                AbilityCooldown = 10f,
                SpecialAbilities = new string[] { "fire_breath", "area_attack", "heal" },
                ExperienceReward = 1400,
                DropItems = new string[] { "frost_essence", "ice_scale", "wyrm_heart" }
            };
            
            // Final Boss - Demon Lord
            _bosses["demon_lord"] = new BossData(
                id: "demon_lord",
                name: "DemonLord",
                title: "Lord of the Abyss",
                health: 10000,
                speed: 100f,
                damage: 100f
            )
            {
                Description = "The Ultimate Evil That Threatens All Realms",
                PhaseCount = 4,
                PhaseHealthThresholds = new int[] { 75, 50, 25 },
                EnrageTime = 300f,
                AbilityCooldown = 5f,
                SpecialAbilities = new string[] { "fire_breath", "ground_slam", "teleport", "summon_minions", "area_attack", "heal" },
                ExperienceReward = 5000,
                DropItems = new string[] { "demon_heart", "abyss_essence", "legendary_sword" }
            };
            
            // Mini Bosses
            _bosses["goblin_king"] = new BossData(
                id: "goblin_king",
                name: "GoblinKing",
                title: "Goblin King",
                health: 500,
                speed: 120f,
                damage: 25f
            )
            {
                Description = "Leader of the Goblin Horde",
                PhaseCount = 2,
                PhaseHealthThresholds = new int[] { 50 },
                EnrageTime = 60f,
                AbilityCooldown = 8f,
                SpecialAbilities = new string[] { "summon_minions" },
                ExperienceReward = 200,
                DropItems = new string[] { "goblin_crown", "gold_coin" }
            };
            
            _bosses["orc_chief"] = new BossData(
                id: "orc_chief",
                name: "OrcChief",
                title: "Orc War Chief",
                health: 800,
                speed: 90f,
                damage: 30f
            )
            {
                Description = "Fierce leader of the Orc Clan",
                PhaseCount = 2,
                PhaseHealthThresholds = new int[] { 50 },
                EnrageTime = 90f,
                AbilityCooldown = 6f,
                SpecialAbilities = new string[] { "ground_slam", "heal" },
                ExperienceReward = 300,
                DropItems = new string[] { "orc_trophy", "battle_axe" }
            };
            
            _bosses["skeleton_lord"] = new BossData(
                id: "skeleton_lord",
                name: "SkeletonLord",
                title: "Undead King",
                health: 600,
                speed: 80f,
                damage: 35f
            )
            {
                Description = "Rise from the grave eternal",
                PhaseCount = 2,
                PhaseHealthThresholds = new int[] { 50 },
                EnrageTime = 75f,
                AbilityCooldown = 7f,
                SpecialAbilities = new string[] { "summon_minions", "heal" },
                ExperienceReward = 250,
                DropItems = new string[] { "bone_crown", "soul_gem" }
            };
        }
        
        public static BossData GetBoss(string id)
        {
            if (_bosses.TryGetValue(id, out var boss))
            {
                return boss;
            }
            GD.PrintErr($"Boss not found: {id}");
            return null;
        }
        
        public static List<BossData> GetAllBosses()
        {
            return new List<BossData>(_bosses.Values);
        }
        
        public static List<BossData> GetBossesByDifficulty(string difficulty)
        {
            var result = new List<BossData>();
            foreach (var boss in _bosses.Values)
            {
                if (difficulty == "easy" && boss.ExperienceReward <= 300)
                    result.Add(boss);
                else if (difficulty == "medium" && boss.ExperienceReward > 300 && boss.ExperienceReward <= 1000)
                    result.Add(boss);
                else if (difficulty == "hard" && boss.ExperienceReward > 1000)
                    result.Add(boss);
            }
            return result;
        }
        
        public static bool HasBoss(string id)
        {
            return _bosses.ContainsKey(id);
        }
        
        public static int GetBossCount()
        {
            return _bosses.Count;
        }
    }
}
