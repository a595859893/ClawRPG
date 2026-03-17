using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Partial class - Spawn logic for WorldBossSystem
    /// </summary>
    public partial class WorldBossSystem {
        /// <summary>
        /// Spawner subsystem - handles world boss spawning and lifecycle
        /// </summary>
        public class Spawner {
            private readonly WorldBossSystem _system;
            
            public Spawner(WorldBossSystem system) {
                _system = system;
            }
            
            public void InitializeWorldBosses() {
                // Dragon Apex - Ancient dragon awakened
                _system.Data.AddWorldBoss(new WorldBossData {
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
                });
                
                // Void Titan - Cosmic entity
                _system.Data.AddWorldBoss(new WorldBossData {
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
                });
                
                // Frost Wraith - Ice overlord
                _system.Data.AddWorldBoss(new WorldBossData {
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
                    SpawnConditions = new Dictionary<string, float> { { "server_time", 0.25f } },
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
                });
                
                // Thunder Lord - Storm deity
                _system.Data.AddWorldBoss(new WorldBossData {
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
                });
                
                // Plague Lord - Disease spreader
                _system.Data.AddWorldBoss(new WorldBossData {
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
                });
                
                // Ancient Golem - Earth guardian
                _system.Data.AddWorldBoss(new WorldBossData {
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
                });
                
                // Shadow Assassin - Master of shadows
                _system.Data.AddWorldBoss(new WorldBossData {
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
                });
                
                // Celestial Phoenix - Divine bird
                _system.Data.AddWorldBoss(new WorldBossData {
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
                });
            }
            
            public void TrySpawnWorldBoss() {
                var availableBosses = new List<WorldBossData>();
                
                foreach (var boss in _system.Data.WorldBosses.Values) {
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
                            if (!_system.CheckOnlinePlayerCount(condition.Value)) return false;
                            break;
                        case "world_level":
                            if (!_system.CheckWorldLevel(condition.Value)) return false;
                            break;
                        case "server_time":
                            var random = new Random();
                            if (random.NextDouble() > condition.Value) return false;
                            break;
                        case "weather_thunderstorm":
                            if (!_system.CheckWeatherActive("thunderstorm")) return false;
                            break;
                        case "plague_world_event":
                            if (!_system.CheckWorldEventActive("plague")) return false;
                            break;
                        case "night_time":
                            if (!_system.CheckNightTime()) return false;
                            break;
                        case "day_time":
                            if (!_system.CheckDayTime()) return false;
                            break;
                        case "weather_clear":
                            if (!_system.CheckWeatherClear()) return false;
                            break;
                        case "dungeon_floor":
                            if (!_system.CheckDungeonFloor(condition.Value)) return false;
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
                
                _system.Data.RegisterActiveBoss(bossData.Id, instance);
                
                GD.Print("[WorldBossSystem] ", bossData.SpawnMessage);
                _system.NotifyPlayersInternal(bossData.SpawnMessage);
            }
            
            public void UpdateActiveBosses(float delta) {
                var bossesToRemove = new List<string>();
                
                foreach (var kvp in _system.Data.ActiveBosses) {
                    var boss = kvp.Value;
                    
                    if (boss.Status == WorldBossStatus.Active) {
                        // Check if boss should enter rage mode
                        var healthPercent = (float)boss.CurrentHealth / boss.MaxHealth;
                        if (healthPercent <= 0.25f && boss.Phase < 4) {
                            boss.Phase = 4; // Enrage phase
                            boss.CurrentAttack = (int)(boss.Data.Attack * 1.5f);
                            _system.NotifyPlayersInternal("⚠️ " + boss.Data.Name + " enters ENRAGE mode!");
                        }
                        
                        // Check if boss is defeated
                        if (boss.CurrentHealth <= 0) {
                            DefeatWorldBoss(kvp.Key);
                            bossesToRemove.Add(kvp.Key);
                        }
                    }
                }
                
                foreach (var bossId in bossesToRemove) {
                    _system.Data.RemoveActiveBoss(bossId);
                }
            }
            
            private void DefeatWorldBoss(string bossId) {
                if (!_system.Data.ActiveBosses.TryGetValue(bossId, out var boss)) return;
                
                boss.Status = WorldBossStatus.Defeated;
                _system.Data.RecordBossDefeat(boss.Data.Name);
                
                GD.Print("[WorldBossSystem] ", boss.Data.DefeatMessage);
                _system.NotifyPlayersInternal(boss.Data.DefeatMessage);
                
                // Distribute rewards
                DistributeRewards(boss);
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
                    
                    _system.NotifyPlayersInternal("🎉 Top damage dealer: " + topPlayer + " with " + topDamage + " damage!");
                }
            }
            
            public void CheckWorldBossEvents() {
                // Check for boss-related world events
                foreach (var boss in _system.Data.ActiveBosses.Values) {
                    if (boss.Status == WorldBossStatus.Active) {
                        var elapsed = (DateTime.Now - boss.SpawnTime).TotalMinutes;
                        if (elapsed > 30) {
                            // Boss expires
                            boss.Status = WorldBossStatus.Expired;
                            _system.NotifyPlayersInternal("⏰ " + boss.Data.Name + " has escaped!");
                            _system.Data.RemoveActiveBoss(boss.Data.Id);
                        }
                    }
                }
            }
        }
    }
}
