using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// World Boss System - Global boss events that require player cooperation
    /// </summary>
    [GlobalClass]
    public partial class WorldBossSystem : Node {
        
        // Singleton instance
        public static WorldBossSystem Instance { get; private set; }
        
        // World boss data
        private Dictionary<string, WorldBossData> worldBosses = new();
        private Dictionary<string, WorldBossInstance> activeBosses = new();
        
        // Event tracking
        private int totalWorldBossEvents = 0;
        private int successfulDefeats = 0;
        private int totalDamageDealt = 0;
        private int totalPlayers参与 = 0;
        private List<string> bossHistory = new();
        
        // Timers
        private float spawnTimer = 0f;
        private float eventCheckTimer = 0f;
        
        // Configuration
        private float spawnInterval = 600f; // 10 minutes
        private int maxActiveBosses = 3;
        private float globalDamageMultiplier = 1.0f;
        
        public override void _Ready() {
            Instance = this;
            InitializeWorldBosses();
            GD.Print("[WorldBossSystem] Initialized with ", worldBosses.Count, " world bosses");
        }
        
        public override void _Process(float delta) {
            spawnTimer += delta;
            eventCheckTimer += delta;
            
            // Spawn new world boss periodically
            if (spawnTimer >= spawnInterval && activeBosses.Count < maxActiveBosses) {
                TrySpawnWorldBoss();
                spawnTimer = 0f;
            }
            
            // Check boss events
            if (eventCheckTimer >= 5f) {
                CheckWorldBossEvents();
                eventCheckTimer = 0f;
            }
            
            // Update active bosses
            UpdateActiveBosses(delta);
        }
        
        private void InitializeWorldBosses() {
            // Dragon Apex - Ancient dragon awakened
            worldBosses["dragon_apex"] = new WorldBossData {
                Id = "dragon_apex",
                Name = "Dragon Apex",
                Description = "An ancient dragon awakened from eternal slumber",
                Type = WorldBossType.Elite,
                Difficulty = 5,
                MaxHealth = 1000000,
                Attack = 5000,
                Defense = 2000,
                Speed = 100,
                Abilities = new List<string> { "FireBreath", "TailSweep", "WingStorm", "Roar" },
                Element = ElementType.Fire,
                Weakness = ElementType.Ice,
                SpawnConditions = new Dictionary<string, float> { { "player_count", 10 } },
                Rewards = new WorldBossRewards {
                    GoldMin = 50000,
                    GoldMax = 100000,
                    ExperienceMin = 50000,
                    ExperienceMax = 100000,
                    DropRateBonus = 0.5f,
                    UniqueDrops = new List<string> { "DragonScale", "DragonHeart", "ApexFang" }
                },
                SpawnMessage = "⚠️ World Boss Appeared: Dragon Apex has awakened!",
                DefeatMessage = "🏆 Victory! Dragon Apex has been defeated!"
            };
            
            // Void Titan - Cosmic entity
            worldBosses["void_titan"] = new WorldBossData {
                Id = "void_titan",
                Name = "Void Titan",
                Description = "A cosmic entity from the void dimension",
                Type = WorldBossType.Cosmic,
                Difficulty = 5,
                MaxHealth = 1500000,
                Attack = 6000,
                Defense = 1500,
                Speed = 80,
                Abilities = new List<string> { "VoidBlast", "GravityWell", "DarkMatter", "CosmicRay" },
                Element = ElementType.Dark,
                Weakness = ElementType.Holy,
                SpawnConditions = new Dictionary<string, float> { { "world_level", 50 } },
                Rewards = new WorldBossRewards {
                    GoldMin = 75000,
                    GoldMax = 150000,
                    ExperienceMin = 75000,
                    ExperienceMax = 150000,
                    DropRateBonus = 0.6f,
                    UniqueDrops = new List<string> { "VoidEssence", "CosmicShard", "TitanCore" }
                },
                SpawnMessage = "🌌 World Boss Appeared: Void Titan emerges from the void!",
                DefeatMessage = "🏆 Victory! Void Titan has been banished!"
            };
            
            // Frost Wraith - Ice overlord
            worldBosses["frost_wraith"] = new WorldBossData {
                Id = "frost_wraith",
                Name = "Frost Wraith",
                Description = "An ice overlord from the frozen north",
                Type = WorldBossType.Elite,
                Difficulty = 4,
                MaxHealth = 750000,
                Attack = 4000,
                Defense = 1800,
                Speed = 120,
                Abilities = new List<string> { "IceSpear", "Blizzard", "FrozenGround", "FrostNova" },
                Element = ElementType.Ice,
                Weakness = ElementType.Fire,
                SpawnConditions = new Dictionary<string, float> { { "server_time", 0.25f } }, // 25% chance
                Rewards = new WorldBossRewards {
                    GoldMin = 40000,
                    GoldMax = 80000,
                    ExperienceMin = 40000,
                    ExperienceMax = 80000,
                    DropRateBonus = 0.4f,
                    UniqueDrops = new List<string> { "FrostCore", "IceCrown", "WraithSoul" }
                },
                SpawnMessage = "❄️ World Boss Appeared: Frost Wraith freezes the realm!",
                DefeatMessage = "🏆 Victory! Frost Wraith has melted away!"
            };
            
            // Thunder Lord - Storm deity
            worldBosses["thunder_lord"] = new WorldBossData {
                Id = "thunder_lord",
                Name = "Thunder Lord",
                Description = "A deity of lightning and storms",
                Type = WorldBossType.Divine,
                Difficulty = 5,
                MaxHealth = 1200000,
                Attack = 7000,
                Defense = 1200,
                Speed = 150,
                Abilities = new List<string> { "ThunderStrike", "StormSurge", "LightningChain", "ThunderClap" },
                Element = ElementType.Lightning,
                Weakness = ElementType.Earth,
                SpawnConditions = new Dictionary<string, float> { { "weather_thunderstorm", 1.0f } },
                Rewards = new WorldBossRewards {
                    GoldMin = 60000,
                    GoldMax = 120000,
                    ExperienceMin = 60000,
                    ExperienceMax = 120000,
                    DropRateBonus = 0.55f,
                    UniqueDrops = new List<string> { "ThunderOrb", "StormCrown", "LightningHeart" }
                },
                SpawnMessage = "⚡ World Boss Appeared: Thunder Lord brings the storm!",
                DefeatMessage = "🏆 Victory! Thunder Lord has been silenced!"
            };
            
            // Plague Lord - Disease spreader
            worldBosses["plague_lord"] = new WorldBossData {
                Id = "plague_lord",
                Name = "Plague Lord",
                Description = "A corrupted entity spreading corruption",
                Type = WorldBossType.Corrupted,
                Difficulty = 4,
                MaxHealth = 800000,
                Attack = 3500,
                Defense = 2000,
                Speed = 90,
                Abilities = new List<string> { "PlagueCloud", "ToxicBurst", "Infection", "DeathFog" },
                Element = ElementType.Poison,
                Weakness = ElementType.Holy,
                SpawnConditions = new Dictionary<string, float> { { "plague_world_event", 1.0f } },
                Rewards = new WorldBossRewards {
                    GoldMin = 45000,
                    GoldMax = 90000,
                    ExperienceMin = 45000,
                    ExperienceMax = 90000,
                    DropRateBonus = 0.45f,
                    UniqueDrops = new List<string> { "PlagueCore", "ToxicTail", "CorruptedHeart" }
                },
                SpawnMessage = "☠️ World Boss Appeared: Plague Lord spreads corruption!",
                DefeatMessage = "🏆 Victory! Plague Lord has been purged!"
            };
            
            // Ancient Golem - Earth guardian
            worldBosses["ancient_golem"] = new WorldBossData {
                Id = "ancient_golem",
                Name = "Ancient Golem",
                Description = "A massive construct from ancient times",
                Type = WorldBossType.Construct,
                Difficulty = 3,
                MaxHealth = 500000,
                Attack = 3000,
                Defense = 3000,
                Speed = 50,
                Abilities = new List<string> { "RockSmash", "GroundPound", "Avalanche", "StoneSkin" },
                Element = ElementType.Earth,
                Weakness = ElementType.Lightning,
                SpawnConditions = new Dictionary<string, float> { { "dungeon_floor", 50 } },
                Rewards = new WorldBossRewards {
                    GoldMin = 30000,
                    GoldMax = 60000,
                    ExperienceMin = 30000,
                    ExperienceMax = 60000,
                    DropRateBonus = 0.35f,
                    UniqueDrops = new List<string> { "GolemCore", "AncientRelic", "EarthHeart" }
                },
                SpawnMessage = "🗿 World Boss Appeared: Ancient Golem awakens!",
                DefeatMessage = "🏆 Victory! Ancient Golem returns to dormancy!"
            };
            
            // Shadow Assassin - Master of shadows
            worldBosses["shadow_assassin"] = new WorldBossData {
                Id = "shadow_assassin",
                Name = "Shadow Assassin",
                Description = "A legendary assassin from the shadow realm",
                Type = WorldBossType.Assassin,
                Difficulty = 4,
                MaxHealth = 400000,
                Attack = 8000,
                Defense = 800,
                Speed = 200,
                Abilities = new List<string> { "ShadowStrike", "DeathMark", "SmokeBomb", "Assassinate" },
                Element = ElementType.Dark,
                Weakness = ElementType.Light,
                SpawnConditions = new Dictionary<string, float> { { "night_time", 1.0f } },
                Rewards = new WorldBossRewards {
                    GoldMin = 55000,
                    GoldMax = 110000,
                    ExperienceMin = 55000,
                    ExperienceMax = 110000,
                    DropRateBonus = 0.5f,
                    UniqueDrops = new List<string> { "ShadowCloak", "AssassinMark", "VoidDagger" }
                },
                SpawnMessage = "🗡️ World Boss Appeared: Shadow Assassin stalks the realm!",
                DefeatMessage = "🏆 Victory! Shadow Assassin has been neutralized!"
            };
            
            // Celestial Phoenix - Divine bird
            worldBosses["celestial_phoenix"] = new WorldBossData {
                Id = "celestial_phoenix",
                Name = "Celestial Phoenix",
                Description = "A divine bird of light and fire",
                Type = WorldBossType.Divine,
                Difficulty = 5,
                MaxHealth = 900000,
                Attack = 5500,
                Defense = 1600,
                Speed = 130,
                Abilities = new List<string> { "PhoenixBlast", "DivineWrath", "Rebirth", "SolarFlare" },
                Element = ElementType.Holy,
                Weakness = ElementType.Dark,
                SpawnConditions = new Dictionary<string, float> { { "day_time", 1.0f }, { "weather_clear", 1.0f } },
                Rewards = new WorldBossRewards {
                    GoldMin = 80000,
                    GoldMax = 160000,
                    ExperienceMin = 80000,
                    ExperienceMax = 160000,
                    DropRateBonus = 0.65f,
                    UniqueDrops = new List<string> { "PhoenixFeather", "DivineEgg", "SolarCrown" }
                },
                SpawnMessage = "🔥 World Boss Appeared: Celestial Phoenix descends from the heavens!",
                DefeatMessage = "🏆 Victory! Celestial Phoenix rises anew!"
            };
        }
        
        private void TrySpawnWorldBoss() {
            var availableBosses = new List<WorldBossData>();
            
            foreach (var boss in worldBosses.Values) {
                if (CanSpawnBoss(boss)) {
                    availableBosses.Add(boss);
                }
            }
            
            if (availableBosses.Count > 0) {
                var random = new Random();
                var bossToSpawn = availableBosses[random.Next(availableBosses.Count)];
                SpawnWorldBoss(bossToSpawn);
            }
        }
        
        private bool CanSpawnBoss(WorldBossData bossData) {
            // Check spawn conditions
            foreach (var condition in bossData.SpawnConditions) {
                switch (condition.Key) {
                    case "player_count":
                        if (GetOnlinePlayerCount() < condition.Value) return false;
                        break;
                    case "world_level":
                        if (GetWorldLevel() < condition.Value) return false;
                        break;
                    case "server_time":
                        var random = new Random();
                        if (random.NextDouble() > condition.Value) return false;
                        break;
                    case "weather_thunderstorm":
                        if (!IsWeatherActive("thunderstorm")) return false;
                        break;
                    case "plague_world_event":
                        if (!IsWorldEventActive("plague")) return false;
                        break;
                    case "night_time":
                        if (!IsNightTime()) return false;
                        break;
                    case "day_time":
                        if (!IsDayTime()) return false;
                        break;
                    case "weather_clear":
                        if (!IsWeatherClear()) return false;
                        break;
                    case "dungeon_floor":
                        if (GetHighestDungeonFloor() < condition.Value) return false;
                        break;
                }
            }
            
            return true;
        }
        
        private void SpawnWorldBoss(WorldBossData bossData) {
            var instance = new WorldBossInstance {
                Data = bossData,
                CurrentHealth = bossData.MaxHealth,
                MaxHealth = bossData.MaxHealth,
                Phase = 1,
                SpawnTime = DateTime.Now,
                DamageContributors = new Dictionary<string, int>(),
                Status = WorldBossStatus.Active
            };
            
            activeBosses[bossData.Id] = instance;
            totalWorldBossEvents++;
            
            GD.Print("[WorldBossSystem] ", bossData.SpawnMessage);
            NotifyPlayers(bossData.SpawnMessage);
        }
        
        private void UpdateActiveBosses(float delta) {
            var bossesToRemove = new List<string>();
            
            foreach (var kvp in activeBosses) {
                var boss = kvp.Value;
                
                if (boss.Status == WorldBossStatus.Active) {
                    // Check if boss should enter rage mode
                    var healthPercent = (float)boss.CurrentHealth / boss.MaxHealth;
                    if (healthPercent <= 0.25f && boss.Phase < 4) {
                        boss.Phase = 4; // Enrage phase
                        boss.CurrentAttack = (int)(boss.Data.Attack * 1.5f);
                        NotifyPlayers("⚠️ " + boss.Data.Name + " enters ENRAGE mode!");
                    }
                    
                    // Check if boss is defeated
                    if (boss.CurrentHealth <= 0) {
                        DefeatWorldBoss(kvp.Key);
                        bossesToRemove.Add(kvp.Key);
                    }
                }
            }
            
            foreach (var bossId in bossesToRemove) {
                activeBosses.Remove(bossId);
            }
        }
        
        private void DefeatWorldBoss(string bossId) {
            if (!activeBosses.TryGetValue(bossId, out var boss)) return;
            
            boss.Status = WorldBossStatus.Defeated;
            successfulDefeats++;
            
            GD.Print("[WorldBossSystem] ", boss.Data.DefeatMessage);
            NotifyPlayers(boss.Data.DefeatMessage);
            
            // Distribute rewards
            DistributeRewards(boss);
            
            // Record history
            bossHistory.Add(boss.Data.Name + " - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm"));
            if (bossHistory.Count > 50) bossHistory.RemoveAt(0);
        }
        
        private void DistributeRewards(WorldBossInstance boss) {
            var random = new Random();
            var totalDamage = 0;
            foreach (var damage in boss.DamageContributors.Values) {
                totalDamage += damage;
            }
            
            if (totalDamage == 0) return;
            
            foreach (var kvp in boss.DamageContributors) {
                var playerDamage = kvp.Value;
                var damageShare = (float)playerDamage / totalDamage;
                
                // Gold reward
                var goldReward = (int)(GD.RandRange(boss.Data.Rewards.GoldMin, boss.Data.Rewards.GoldMax) * damageShare);
                // Experience reward
                var expReward = (int)(GD.RandRange(boss.Data.Rewards.ExperienceMin, boss.Data.Rewards.ExperienceMax) * damageShare);
                
                GD.Print("[WorldBossSystem] Player ", kvp.Key, " receives ", goldReward, " gold and ", expReward, " exp");
            }
            
            // Bonus for top damage dealer
            if (boss.DamageContributors.Count > 0) {
                var topPlayer = "";
                var topDamage = 0;
                foreach (var kvp in boss.DamageContributors) {
                    if (kvp.Value > topDamage) {
                        topDamage = kvp.Value;
                        topPlayer = kvp.Key;
                    }
                }
                
                NotifyPlayers("🎉 Top damage dealer: " + topPlayer + " with " + topDamage + " damage!");
            }
        }
        
        private void CheckWorldBossEvents() {
            // Check for boss-related world events
            foreach (var boss in activeBosses.Values) {
                if (boss.Status == WorldBossStatus.Active) {
                    var elapsed = (DateTime.Now - boss.SpawnTime).TotalMinutes;
                    if (elapsed > 30) {
                        // Boss expires
                        boss.Status = WorldBossStatus.Expired;
                        NotifyPlayers("⏰ " + boss.Data.Name + " has escaped!");
                        activeBosses.Remove(boss.Data.Id);
                    }
                }
            }
        }
        
        // Public methods
        public void DealDamageToBoss(string bossId, string playerId, int damage) {
            if (!activeBosses.TryGetValue(bossId, out var boss)) return;
            
            damage = (int)(damage * globalDamageMultiplier);
            boss.CurrentHealth -= damage;
            
            if (!boss.DamageContributors.ContainsKey(playerId)) {
                boss.DamageContributors[playerId] = 0;
            }
            boss.DamageContributors[playerId] += damage;
            
            totalDamageDealt += damage;
            totalPlayers参与++;
        }
        
        public Dictionary<string, WorldBossInstance> GetActiveBosses() => new Dictionary<string, WorldBossInstance>(activeBosses);
        
        public Dictionary<string, WorldBossData> GetAllBosses() => new Dictionary<string, WorldBossData>(worldBosses);
        
        public WorldBossStatistics GetStatistics() {
            return new WorldBossStatistics {
                TotalEvents = totalWorldBossEvents,
                SuccessfulDefeats = successfulDefeats,
                TotalDamageDealt = totalDamageDealt,
                TotalPlayers参与 = totalPlayers参与,
                SuccessRate = totalWorldBossEvents > 0 ? (float)successfulDefeats / totalWorldBossEvents : 0,
                BossHistory = new List<string>(bossHistory)
            };
        }
        
        public void SetSpawnInterval(float interval) => spawnInterval = interval;
        
        public void SetGlobalDamageMultiplier(float multiplier) => globalDamageMultiplier = multiplier;
        
        // Helper methods (mock implementations)
        private int GetOnlinePlayerCount() => 1;
        private int GetWorldLevel() => 50;
        private bool IsWeatherActive(string weather) => false;
        private bool IsWorldEventActive(string eventName) => false;
        private bool IsNightTime() => true;
        private bool IsDayTime() => false;
        private bool IsWeatherClear() => true;
        private int GetHighestDungeonFloor() => 50;
        
        private void NotifyPlayers(string message) {
            GD.Print("[WorldBossSystem] " + message);
        }
    }
    
    // Data classes
    public enum WorldBossType { Elite, Cosmic, Divine, Corrupted, Construct, Assassin }
    public enum WorldBossStatus { Active, Defeated, Expired }
    public enum ElementType { Fire, Ice, Lightning, Dark, Holy, Earth, Poison, Physical }
    
    public class WorldBossData {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public WorldBossType Type { get; set; }
        public int Difficulty { get; set; }
        public int MaxHealth { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Speed { get; set; }
        public List<string> Abilities { get; set; }
        public ElementType Element { get; set; }
        public ElementType Weakness { get; set; }
        public Dictionary<string, float> SpawnConditions { get; set; }
        public WorldBossRewards Rewards { get; set; }
        public string SpawnMessage { get; set; }
        public string DefeatMessage { get; set; }
    }
    
    public class WorldBossRewards {
        public int GoldMin { get; set; }
        public int GoldMax { get; set; }
        public int ExperienceMin { get; set; }
        public int ExperienceMax { get; set; }
        public float DropRateBonus { get; set; }
        public List<string> UniqueDrops { get; set; }
    }
    
    public class WorldBossInstance {
        public WorldBossData Data { get; set; }
        public long CurrentHealth { get; set; }
        public long MaxHealth { get; set; }
        public int Phase { get; set; }
        public int CurrentAttack { get; set; }
        public DateTime SpawnTime { get; set; }
        public Dictionary<string, int> DamageContributors { get; set; }
        public WorldBossStatus Status { get; set; }
    }
    
    public class WorldBossStatistics {
        public int TotalEvents { get; set; }
        public int SuccessfulDefeats { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalPlayers参与 { get; set; }
        public float SuccessRate { get; set; }
        public List<string> BossHistory { get; set; }
    }
}
