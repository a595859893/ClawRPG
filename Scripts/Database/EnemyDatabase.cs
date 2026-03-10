using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Database {
    /// <summary>
    /// Enemy type definition for data-driven enemy spawning
    /// </summary>
    [Serializable]
    public class EnemyType {
        public string Id;
        public string Name;
        public string Description;
        
        // Combat stats
        public int MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackRange;
        public float AttackCooldown;
        public float ChaseRange;
        public float DetectionRange;
        
        // Combat stats
        public float CriticalChance = 0.05f;
        public float CriticalDamage = 1.5f;
        
        // Rewards
        public int ExperienceReward;
        public int GoldReward;
        
        // Visual
        public string SpritePath;
        public Color SpriteModulate = Colors.White;
        
        // Loot table (itemId -> dropChance)
        public Dictionary<string, float> DropTable = new();
        
        // AI behavior
        public bool CanChase = true;
        public bool CanAttack = true;
        public bool IsAggressive = true;
        
        // Status effect vulnerability
        public Dictionary<string, float> StatusEffectVulnerability = new();
        
        public EnemyType() {
            Id = "";
            Name = "Unknown";
            Description = "";
        }
        
        public EnemyType(string id, string name, int hp, float speed, float damage) {
            Id = id;
            Name = name;
            MaxHealth = hp;
            MoveSpeed = speed;
            AttackDamage = damage;
        }
    }
    
    /// <summary>
    /// Database of all enemy types in the game
    /// </summary>
    public class EnemyDatabase {
        private static EnemyDatabase _instance;
        public static EnemyDatabase Instance {
            get {
                if (_instance == null) {
                    _instance = new EnemyDatabase();
                    _instance.LoadEnemies();
                }
                return _instance;
            }
        }
        
        private Dictionary<string, EnemyType> _enemies = new();
        
        public void LoadEnemies() {
            // === FOREST ENEMIES ===
            _enemies["goblin"] = new EnemyType("goblin", "Goblin", 30, 80f, 8f) {
                Description = "A small, mischievous creature",
                AttackRange = 40f,
                AttackCooldown = 1.2f,
                ChaseRange = 150f,
                DetectionRange = 250f,
                ExperienceReward = 10,
                GoldReward = 5,
                DropTable = new Dictionary<string, float> {
                    { "goblin_ear", 0.3f },
                    { "monster_essence", 0.1f }
                },
                SpriteModulate = new Color(0.4f, 0.6f, 0.3f)
            };
            
            _enemies["wolf"] = new EnemyType("wolf", "Forest Wolf", 45, 120f, 12f) {
                Description = "A fierce predator from the forest",
                AttackRange = 35f,
                AttackCooldown = 0.8f,
                ChaseRange = 200f,
                DetectionRange = 300f,
                ExperienceReward = 15,
                GoldReward = 8,
                DropTable = new Dictionary<string, float> {
                    { "monster_essence", 0.2f },
                    { "wolf_pelt", 0.4f }
                },
                SpriteModulate = new Color(0.5f, 0.5f, 0.5f)
            };
            
            _enemies["slime"] = new EnemyType("slime", "Forest Slime", 25, 40f, 5f) {
                Description = "A gelatinous blob",
                AttackRange = 30f,
                AttackCooldown = 2.0f,
                ChaseRange = 100f,
                DetectionRange = 150f,
                ExperienceReward = 5,
                GoldReward = 2,
                DropTable = new Dictionary<string, float> {
                    { "slime_gel", 0.5f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 1.5f },  // Fire deals more damage
                    { "ice", 0.5f }   // Ice deals less damage
                },
                SpriteModulate = new Color(0.3f, 0.8f, 0.3f)
            };
            
            _enemies["spider"] = new EnemyType("spider", "Giant Spider", 40, 100f, 10f) {
                Description = "A poisonous arachnid",
                AttackRange = 45f,
                AttackCooldown = 1.0f,
                ChaseRange = 180f,
                DetectionRange = 280f,
                ExperienceReward = 12,
                GoldReward = 6,
                DropTable = new Dictionary<string, float> {
                    { "spider_silk", 0.4f },
                    { "monster_essence", 0.15f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 1.3f }
                },
                SpriteModulate = new Color(0.2f, 0.1f, 0.2f)
            };
            
            // === CAVE ENEMIES ===
            _enemies["bat"] = new EnemyType("bat", "Cave Bat", 20, 150f, 6f) {
                Description = "A nocturnal flying creature",
                AttackRange = 25f,
                AttackCooldown = 0.6f,
                ChaseRange = 120f,
                DetectionRange = 200f,
                ExperienceReward = 8,
                GoldReward = 3,
                DropTable = new Dictionary<string, float> {
                    { "monster_essence", 0.1f }
                },
                SpriteModulate = new Color(0.3f, 0.2f, 0.3f)
            };
            
            _enemies["skeleton"] = new EnemyType("skeleton", "Skeleton Warrior", 55, 70f, 15f) {
                Description = "An undead warrior",
                AttackRange = 50f,
                AttackCooldown = 1.5f,
                ChaseRange = 160f,
                DetectionRange = 260f,
                ExperienceReward = 20,
                GoldReward = 12,
                DropTable = new Dictionary<string, float> {
                    { "skeleton_bone", 0.5f },
                    { "ancient_coin", 0.2f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "holy", 1.5f },
                    { "fire", 1.2f }
                },
                SpriteModulate = new Color(0.9f, 0.9f, 0.85f)
            };
            
            _enemies["cave_spider"] = new EnemyType("cave_spider", "Cave Spider", 50, 90f, 14f) {
                Description = "A blind spider from the depths",
                AttackRange = 50f,
                AttackCooldown = 1.2f,
                ChaseRange = 150f,
                DetectionRange = 220f,
                ExperienceReward = 18,
                GoldReward = 10,
                DropTable = new Dictionary<string, float> {
                    { "spider_silk", 0.5f },
                    { "dark_crystal", 0.15f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 1.4f }
                },
                SpriteModulate = new Color(0.15f, 0.08f, 0.15f)
            };
            
            _enemies["rock_golem"] = new EnemyType("rock_golem", "Rock Golem", 100, 40f, 20f) {
                Description = "A stone construct animated with magic",
                AttackRange = 60f,
                AttackCooldown = 2.0f,
                ChaseRange = 100f,
                DetectionRange = 180f,
                ExperienceReward = 30,
                GoldReward = 20,
                DropTable = new Dictionary<string, float> {
                    { "golem_core", 0.3f },
                    { "ancient_coin", 0.1f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "ice", 1.3f },
                    { "lightning", 1.4f }
                },
                SpriteModulate = new Color(0.5f, 0.45f, 0.4f)
            };
            
            // === FIRE DUNGEON ENEMIES ===
            _enemies["fire_elemental"] = new EnemyType("fire_elemental", "Fire Elemental", 60, 110f, 18f) {
                Description = "A creature made of pure flame",
                AttackRange = 40f,
                AttackCooldown = 0.9f,
                ChaseRange = 170f,
                DetectionRange = 270f,
                ExperienceReward = 25,
                GoldReward = 15,
                DropTable = new Dictionary<string, float> {
                    { "fire_essence", 0.4f },
                    { "phoenix_feather", 0.2f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "ice", 2.0f },
                    { "water", 1.5f }
                },
                SpriteModulate = new Color(1f, 0.4f, 0.1f)
            };
            
            _enemies["magma_golem"] = new EnemyType("magma_golem", "Magma Golem", 120, 35f, 25f) {
                Description = "A golem forged from molten rock",
                AttackRange = 70f,
                AttackCooldown = 2.5f,
                ChaseRange = 90f,
                DetectionRange = 160f,
                ExperienceReward = 40,
                GoldReward = 25,
                DropTable = new Dictionary<string, float> {
                    { "magma_core", 0.35f },
                    { "fire_essence", 0.25f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "ice", 2.0f },
                    { "water", 1.8f }
                },
                SpriteModulate = new Color(0.7f, 0.2f, 0.1f)
            };
            
            _enemies["fire_imp"] = new EnemyType("fire_imp", "Fire Imp", 35, 140f, 12f) {
                Description = "A mischievous fire demon",
                AttackRange = 35f,
                AttackCooldown = 0.7f,
                ChaseRange = 200f,
                DetectionRange = 320f,
                ExperienceReward = 22,
                GoldReward = 14,
                DropTable = new Dictionary<string, float> {
                    { "fire_essence", 0.5f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "ice", 1.8f }
                },
                SpriteModulate = new Color(1f, 0.3f, 0f)
            };
            
            // === ICE DUNGEON ENEMIES ===
            _enemies["ice_elemental"] = new EnemyType("ice_elemental", "Ice Elemental", 55, 100f, 16f) {
                Description = "A creature formed from glacial ice",
                AttackRange = 45f,
                AttackCooldown = 1.0f,
                ChaseRange = 160f,
                DetectionRange = 260f,
                ExperienceReward = 24,
                GoldReward = 14,
                DropTable = new Dictionary<string, float> {
                    { "ice_crystal", 0.4f },
                    { "frost_essence", 0.2f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 2.0f },
                    { "lightning", 1.3f }
                },
                SpriteModulate = new Color(0.6f, 0.8f, 1f)
            };
            
            _enemies["frost_wolf"] = new EnemyType("frost_wolf", "Frost Wolf", 50, 130f, 14f) {
                Description = "A wolf made of magical ice",
                AttackRange = 40f,
                AttackCooldown = 0.85f,
                ChaseRange = 190f,
                DetectionRange = 290f,
                ExperienceReward = 20,
                GoldReward = 12,
                DropTable = new Dictionary<string, float> {
                    { "frost_essence", 0.3f },
                    { "wolf_pelt", 0.3f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 1.6f }
                },
                SpriteModulate = new Color(0.7f, 0.85f, 1f)
            };
            
            _enemies["ice_skeleton"] = new EnemyType("ice_skeleton", "Ice Skeleton", 60, 65f, 16f) {
                Description = "An undead frozen in ice",
                AttackRange = 50f,
                AttackCooldown = 1.4f,
                ChaseRange = 140f,
                DetectionRange = 240f,
                ExperienceReward = 22,
                GoldReward = 13,
                DropTable = new Dictionary<string, float> {
                    { "ice_crystal", 0.35f },
                    { "skeleton_bone", 0.3f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 1.8f }
                },
                SpriteModulate = new Color(0.75f, 0.9f, 1f)
            };
            
            // === SHADOW DUNGEON ENEMIES ===
            _enemies["shadow_spirit"] = new EnemyType("shadow_spirit", "Shadow Spirit", 30, 160f, 10f) {
                Description = "A wisp of pure darkness",
                AttackRange = 30f,
                AttackCooldown = 0.5f,
                ChaseRange = 180f,
                DetectionRange = 300f,
                ExperienceReward = 18,
                GoldReward = 10,
                DropTable = new Dictionary<string, float> {
                    { "shadow_essence", 0.4f },
                    { "dark_crystal", 0.2f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "holy", 1.8f },
                    { "fire", 1.3f }
                },
                SpriteModulate = new Color(0.2f, 0.1f, 0.3f)
            };
            
            _enemies["dark_knight"] = new EnemyType("dark_knight", "Dark Knight", 80, 80f, 22f) {
                Description = "A fallen warrior of darkness",
                AttackRange = 55f,
                AttackCooldown = 1.3f,
                ChaseRange = 150f,
                DetectionRange = 250f,
                ExperienceReward = 35,
                GoldReward = 22,
                DropTable = new Dictionary<string, float> {
                    { "dark_crystal", 0.3f },
                    { "ancient_coin", 0.25f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "holy", 1.5f },
                    { "fire", 1.2f }
                },
                SpriteModulate = new Color(0.25f, 0.2f, 0.3f)
            };
            
            _enemies["wraith"] = new EnemyType("wraith", "Wraith", 45, 120f, 18f) {
                Description = "A ghostly apparition",
                AttackRange = 40f,
                AttackCooldown = 0.8f,
                ChaseRange = 170f,
                DetectionRange = 280f,
                ExperienceReward = 28,
                GoldReward = 16,
                DropTable = new Dictionary<string, float> {
                    { "shadow_essence", 0.35f },
                    { "ghost_essence", 0.25f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "holy", 2.0f },
                    { "fire", 1.4f }
                },
                SpriteModulate = new Color(0.3f, 0.4f, 0.5f)
            };
            
            // === FOREST ELITE ENEMIES ===
            _enemies["forest_bear"] = new EnemyType("forest_bear", "Forest Bear", 80, 90f, 22f) {
                Description = "A powerful bear of the forest",
                AttackRange = 55f,
                AttackCooldown = 1.8f,
                ChaseRange = 180f,
                DetectionRange = 280f,
                ExperienceReward = 28,
                GoldReward = 18,
                DropTable = new Dictionary<string, float> {
                    { "bear_pelt", 0.5f },
                    { "monster_essence", 0.2f }
                },
                SpriteModulate = new Color(0.45f, 0.3f, 0.2f)
            };
            
            _enemies["mystic_deer"] = new EnemyType("mystic_deer", "Mystic Deer", 35, 130f, 8f) {
                Description = "A magical deer with antlers glowing with nature's power",
                AttackRange = 30f,
                AttackCooldown = 0.9f,
                ChaseRange = 160f,
                DetectionRange = 260f,
                ExperienceReward = 15,
                GoldReward = 10,
                DropTable = new Dictionary<string, float> {
                    { "antler", 0.4f },
                    { "nature_essence", 0.25f }
                },
                SpriteModulate = new Color(0.85f, 0.75f, 0.6f)
            };
            
            _enemies["poison_mushroom"] = new EnemyType("poison_mushroom", "Poison Mushroom", 30, 35f, 14f) {
                Description = "A sentient fungus that releases toxic spores",
                AttackRange = 45f,
                AttackCooldown = 1.5f,
                ChaseRange = 80f,
                DetectionRange = 150f,
                ExperienceReward = 14,
                GoldReward = 8,
                DropTable = new Dictionary<string, float> {
                    { "mushroom", 0.6f },
                    { "poison_essence", 0.2f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 1.4f }
                },
                SpriteModulate = new Color(0.6f, 0.2f, 0.7f)
            };
            
            // === CAVE ELITE ENEMIES ===
            _enemies["giant_centipede"] = new EnemyType("giant_centipede", "Giant Centipede", 45, 110f, 12f) {
                Description = "A massive multi-legged predator",
                AttackRange = 40f,
                AttackCooldown = 0.7f,
                ChaseRange = 150f,
                DetectionRange = 240f,
                ExperienceReward = 20,
                GoldReward = 12,
                DropTable = new Dictionary<string, float> {
                    { "centipede_leg", 0.4f },
                    { "monster_essence", 0.15f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 1.3f }
                },
                SpriteModulate = new Color(0.5f, 0.3f, 0.2f)
            };
            
            _enemies["cave_troll"] = new EnemyType("cave_troll", "Cave Troll", 150, 50f, 28f) {
                Description = "A brutish troll from the deep caves",
                AttackRange = 70f,
                AttackCooldown = 2.2f,
                ChaseRange = 120f,
                DetectionRange = 200f,
                ExperienceReward = 50,
                GoldReward = 35,
                DropTable = new Dictionary<string, float> {
                    { "troll_flesh", 0.4f },
                    { "golem_core", 0.2f }
                },
                SpriteModulate = new Color(0.35f, 0.45f, 0.35f)
            };
            
            // === FIRE DUNGEON ELITE ENEMIES ===
            _enemies["lava_eel"] = new EnemyType("lava_eel", "Lava Eel", 55, 125f, 16f) {
                Description = "A serpentine creature that swims through molten rock",
                AttackRange = 40f,
                AttackCooldown = 0.75f,
                ChaseRange = 180f,
                DetectionRange = 290f,
                ExperienceReward = 26,
                GoldReward = 16,
                DropTable = new Dictionary<string, float> {
                    { "lava_scale", 0.45f },
                    { "fire_essence", 0.3f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "ice", 2.2f },
                    { "water", 1.6f }
                },
                SpriteModulate = new Color(1f, 0.35f, 0.05f)
            };
            
            _enemies["fire_phoenix"] = new EnemyType("fire_phoenix", "Fire Phoenix", 70, 145f, 20f) {
                Description = "A majestic bird reborn in flames",
                AttackRange = 50f,
                AttackCooldown = 1.0f,
                ChaseRange = 220f,
                DetectionRange = 350f,
                ExperienceReward = 45,
                GoldReward = 30,
                DropTable = new Dictionary<string, float> {
                    { "phoenix_feather", 0.6f },
                    { "fire_essence", 0.4f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "ice", 2.5f }
                },
                SpriteModulate = new Color(1f, 0.6f, 0.1f)
            };
            
            // === ICE DUNGEON ELITE ENEMIES ===
            _enemies["ice_yeti"] = new EnemyType("ice_yeti", "Ice Yeti", 95, 85f, 24f) {
                Description = "A towering ape of glacial ice",
                AttackRange = 65f,
                AttackCooldown = 2.0f,
                ChaseRange = 160f,
                DetectionRange = 270f,
                ExperienceReward = 38,
                GoldReward = 24,
                DropTable = new Dictionary<string, float> {
                    { "yeti_fur", 0.5f },
                    { "ice_crystal", 0.35f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 2.0f },
                    { "lightning", 1.4f }
                },
                SpriteModulate = new Color(0.9f, 0.95f, 1f)
            };
            
            _enemies["frost_golem"] = new EnemyType("frost_golem", "Frost Golem", 110, 45f, 26f) {
                Description = "A construct of eternal ice",
                AttackRange = 65f,
                AttackCooldown = 2.3f,
                ChaseRange = 100f,
                DetectionRange = 180f,
                ExperienceReward = 42,
                GoldReward = 28,
                DropTable = new Dictionary<string, float> {
                    { "frost_core", 0.45f },
                    { "ice_crystal", 0.3f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "fire", 2.2f },
                    { "lightning", 1.5f }
                },
                SpriteModulate = new Color(0.7f, 0.9f, 1f)
            };
            
            // === SHADOW DUNGEON ELITE ENEMIES ===
            _enemies["vampire"] = new EnemyType("vampire", "Vampire Lord", 75, 115f, 26f) {
                Description = "An undead noble that feeds on the living",
                AttackRange = 45f,
                AttackCooldown = 0.9f,
                ChaseRange = 200f,
                DetectionRange = 320f,
                ExperienceReward = 40,
                GoldReward = 28,
                DropTable = new Dictionary<string, float> {
                    { "vampire_fang", 0.5f },
                    { "blood_essence", 0.35f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "holy", 2.0f },
                    { "fire", 1.5f }
                },
                SpriteModulate = new Color(0.4f, 0.1f, 0.15f)
            };
            
            _enemies["banshee"] = new EnemyType("banshee", "Banshee", 40, 135f, 20f) {
                Description = "A wailing spirit of death",
                AttackRange = 35f,
                AttackCooldown = 0.6f,
                ChaseRange = 190f,
                DetectionRange = 310f,
                ExperienceReward = 35,
                GoldReward = 22,
                DropTable = new Dictionary<string, float> {
                    { "ghost_essence", 0.5f },
                    { "shadow_essence", 0.3f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "holy", 2.2f },
                    { "fire", 1.6f }
                },
                SpriteModulate = new Color(0.25f, 0.3f, 0.5f)
            };
            
            // === DRAGON LAIR ENEMIES ===
            _enemies["dragon_scion"] = new EnemyType("dragon_scion", "Dragon Scion", 200, 100f, 45f) {
                Description = "A young dragon beginning its rise to power",
                AttackRange = 80f,
                AttackCooldown = 2.5f,
                ChaseRange = 220f,
                DetectionRange = 400f,
                ExperienceReward = 120,
                GoldReward = 80,
                DropTable = new Dictionary<string, float> {
                    { "dragon_scale", 0.6f },
                    { "dragon_blood", 0.4f },
                    { "ancient_coin", 0.3f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "ice", 1.5f }
                },
                SpriteModulate = new Color(0.8f, 0.25f, 0.15f)
            };
            
            _enemies["drake"] = new EnemyType("drake", "Drake", 180, 110f, 38f) {
                Description = "A lesser dragon with powerful breath",
                AttackRange = 75f,
                AttackCooldown = 2.2f,
                ChaseRange = 200f,
                DetectionRange = 380f,
                ExperienceReward = 100,
                GoldReward = 65,
                DropTable = new Dictionary<string, float> {
                    { "dragon_scale", 0.5f },
                    { "fire_essence", 0.35f }
                },
                SpriteModulate = new Color(0.3f, 0.5f, 0.3f)
            };
            
            // === HOLY TEMPLE ENEMIES ===
            _enemies["holy_sentinel"] = new EnemyType("holy_sentinel", "Holy Sentinel", 90, 75f, 20f) {
                Description = "An ancient guardian of the sacred temple",
                AttackRange = 55f,
                AttackCooldown = 1.4f,
                ChaseRange = 150f,
                DetectionRange = 260f,
                ExperienceReward = 32,
                GoldReward = 20,
                DropTable = new Dictionary<string, float> {
                    { "holy_crystal", 0.4f },
                    { "ancient_coin", 0.2f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "shadow", 1.5f },
                    { "dark", 1.5f }
                },
                SpriteModulate = new Color(1f, 0.9f, 0.5f)
            };
            
            _enemies["divine_angel"] = new EnemyType("divine_angel", "Divine Angel", 60, 140f, 16f) {
                Description = "A celestial being of pure light",
                AttackRange = 40f,
                AttackCooldown = 0.8f,
                ChaseRange = 200f,
                DetectionRange = 340f,
                ExperienceReward = 50,
                GoldReward = 35,
                DropTable = new Dictionary<string, float> {
                    { "holy_feather", 0.5f },
                    { "holy_crystal", 0.35f }
                },
                StatusEffectVulnerability = new Dictionary<string, float> {
                    { "shadow", 2.0f },
                    { "dark", 2.0f }
                },
                SpriteModulate = new Color(1f, 0.95f, 0.8f)
            };
            
            // === BOSSES (already in BossDatabase, but adding references) ===
            _enemies["ancient_treant"] = new EnemyType("ancient_treant", "Ancient Treant", 2000, 60f, 40f) {
                Description = "The guardian of the forest",
                AttackRange = 80f,
                AttackCooldown = 3.0f,
                ChaseRange = 200f,
                DetectionRange = 400f,
                ExperienceReward = 500,
                GoldReward = 200,
                DropTable = new Dictionary<string, float> {
                    { "treant_heart", 1.0f },
                    { "ancient_coin", 0.5f }
                },
                SpriteModulate = new Color(0.2f, 0.5f, 0.2f)
            };
            
            _enemies["demon_lord"] = new EnemyType("demon_lord", "Demon Lord", 10000, 70f, 80f) {
                Description = "The ruler of the demon realm",
                AttackRange = 100f,
                AttackCooldown = 2.0f,
                ChaseRange = 250f,
                DetectionRange = 500f,
                ExperienceReward = 5000,
                GoldReward = 1000,
                DropTable = new Dictionary<string, float> {
                    { "demon_crown", 1.0f },
                    { "demon_heart", 1.0f },
                    { "ancient_coin", 1.0f }
                },
                SpriteModulate = new Color(0.6f, 0.1f, 0.1f)
            };
            
            GD.Print($"[EnemyDatabase] Loaded {_enemies.Count} enemy types");
        }
        
        public EnemyType GetEnemy(string id) {
            if (_enemies.TryGetValue(id, out var enemy)) {
                return enemy;
            }
            GD.Warning($"Enemy type '{id}' not found in database");
            return null;
        }
        
        public List<EnemyType> GetAllEnemies() {
            return new List<EnemyType>(_enemies.Values);
        }
        
        public List<EnemyType> GetEnemiesByRegion(string region) {
            // This would be expanded with region tags
            return GetAllEnemies();
        }
        
        public List<EnemyType> GetEnemiesForLevel(int playerLevel) {
            var result = new List<EnemyType>();
            foreach (var enemy in _enemies.Values) {
                // Return enemies appropriate for player level
                if (enemy.MaxHealth <= playerLevel * 50 + 100) {
                    result.Add(enemy);
                }
            }
            return result;
        }
        
        public bool HasEnemy(string id) {
            return _enemies.ContainsKey(id);
        }
        
        public int GetEnemyCount() {
            return _enemies.Count;
        }
    }
}
