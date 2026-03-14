using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.SealedDungeon {
    public class SealedDungeonDatabase {
        private static SealedDungeonDatabase _instance;
        public static SealedDungeonDatabase Instance {
            get {
                if (_instance == null) _instance = new SealedDungeonDatabase();
                return _instance;
            }
        }

        public Dictionary<int, DungeonConfig> DungeonConfigs { get; private set; }
        public Dictionary<DungeonZone, ZoneConfig> ZoneConfigs { get; private set; }
        public Dictionary<int, FloorConfig> FloorConfigs { get; private set; }
        public List<DungeonReward> RewardTable { get; private set; }

        public SealedDungeonDatabase() {
            InitializeDungeonConfigs();
            InitializeZoneConfigs();
            InitializeFloorConfigs();
            InitializeRewardTable();
        }

        private void InitializeDungeonConfigs() {
            DungeonConfigs = new Dictionary<int, DungeonConfig>();
            
            DungeonConfigs[1] = new DungeonConfig {
                DungeonId = 1,
                Name = "Ancient Seal",
                Description = "An ancient dungeon sealed away for millennia. Each floor holds secrets waiting to be discovered.",
                RecommendedLevel = 1,
                Difficulty = 1,
                MaxFloors = 10,
                UnlockRequirement = "None",
                Zone = DungeonZone.Entrance
            };

            DungeonConfigs[2] = new DungeonConfig {
                DungeonId = 2,
                Name = "Whispering Depths",
                Description = "Shadows move in the corners of your vision. Voices call from the darkness.",
                RecommendedLevel = 10,
                Difficulty = 2,
                MaxFloors = 15,
                UnlockRequirement = "Complete Ancient Seal",
                Zone = DungeonZone.WhisperingCorridor
            };

            DungeonConfigs[3] = new DungeonConfig {
                DungeonId = 3,
                Name = "Forgotten Tombs",
                Description = "Treasures of a lost civilization await those brave enough to claim them.",
                RecommendedLevel = 20,
                Difficulty = 3,
                MaxFloors = 20,
                UnlockRequirement = "Complete Whispering Depths",
                Zone = DungeonZone.ForgottenChamber
            };
        }

        private void InitializeZoneConfigs() {
            ZoneConfigs = new Dictionary<DungeonZone, ZoneConfig>();
            
            ZoneConfigs[DungeonZone.Entrance] = new ZoneConfig {
                Zone = DungeonZone.Entrance,
                Name = "The Entrance",
                Description = "The gateway to the sealed dungeon. Ancient runes glow faintly on the walls.",
                Difficulty = 1,
                UnlockCost = 0,
                EnemyMultiplier = 1.0f,
                ScoreMultiplier = 1.0f,
                BossFloor = 5,
                SpecialMechanic = "None"
            };

            ZoneConfigs[DungeonZone.WhisperingCorridor] = new ZoneConfig {
                Zone = DungeonZone.WhisperingCorridor,
                Name = "Whispering Corridor",
                Description = "Dark corridors where voices can be heard. Shadows may attack without warning.",
                Difficulty = 2,
                UnlockCost = 1000,
                EnemyMultiplier = 1.2f,
                ScoreMultiplier = 1.2f,
                BossFloor = 7,
                SpecialMechanic = "Stealth"
            };

            ZoneConfigs[DungeonZone.ForgottenChamber] = new ZoneConfig {
                Zone = DungeonZone.ForgottenChamber,
                Name = "Forgotten Chamber",
                Description = "Ancient chambers filled with traps and puzzles. Every step could be your last.",
                Difficulty = 3,
                UnlockCost = 2500,
                EnemyMultiplier = 1.4f,
                ScoreMultiplier = 1.4f,
                BossFloor = 10,
                SpecialMechanic = "Trap"
            };

            ZoneConfigs[DungeonZone.ShadowRealm] = new ZoneConfig {
                Zone = DungeonZone.ShadowRealm,
                Name = "Shadow Realm",
                Description = "A dimension between light and darkness. Reality bends in strange ways.",
                Difficulty = 4,
                UnlockCost = 5000,
                EnemyMultiplier = 1.6f,
                ScoreMultiplier = 1.6f,
                BossFloor = 12,
                SpecialMechanic = "PhaseShift"
            };

            ZoneConfigs[DungeonZone.AncientVault] = new ZoneConfig {
                Zone = DungeonZone.AncientVault,
                Name = "Ancient Vault",
                Description = "Where legends store their most prized possessions. Heavy defenses protect the treasures within.",
                Difficulty = 5,
                UnlockCost = 8000,
                EnemyMultiplier = 1.8f,
                ScoreMultiplier = 1.8f,
                BossFloor = 15,
                SpecialMechanic = "Guardian"
            };

            ZoneConfigs[DungeonZone.CrystalCavern] = new ZoneConfig {
                Zone = DungeonZone.CrystalCavern,
                Name = "Crystal Cavern",
                Description = "Crystalline formations amplify magic. Both yours and the enemies'.",
                Difficulty = 6,
                UnlockCost = 12000,
                EnemyMultiplier = 2.0f,
                ScoreMultiplier = 2.0f,
                BossFloor = 18,
                SpecialMechanic = "ManaSurge"
            };

            ZoneConfigs[DungeonZone.DragonLair] = new ZoneConfig {
                Zone = DungeonZone.DragonLair,
                Name = "Dragon's Lair",
                Description = "The ancient wyrm sleeps here. Tremors warn of its slumber.",
                Difficulty = 7,
                UnlockCost = 18000,
                EnemyMultiplier = 2.2f,
                ScoreMultiplier = 2.2f,
                BossFloor = 20,
                SpecialMechanic = "DragonBreath"
            };

            ZoneConfigs[DungeonZone.VoidPortal] = new ZoneConfig {
                Zone = DungeonZone.VoidPortal,
                Name = "Void Portal",
                Description = "A tear in reality itself. Strange entities lurk beyond.",
                Difficulty = 8,
                UnlockCost = 25000,
                EnemyMultiplier = 2.5f,
                ScoreMultiplier = 2.5f,
                BossFloor = 25,
                SpecialMechanic = "Void corruption"
            };

            ZoneConfigs[DungeonZone.CelestialGarden] = new ZoneConfig {
                Zone = DungeonZone.CelestialGarden,
                Name = "Celestial Garden",
                Description = "A garden that exists beyond the mortal realm. Beautiful but deadly.",
                Difficulty = 9,
                UnlockCost = 35000,
                EnemyMultiplier = 2.8f,
                ScoreMultiplier = 2.8f,
                BossFloor = 30,
                SpecialMechanic = "DivineWrath"
            };

            ZoneConfigs[DungeonZone.EternalThrone] = new ZoneConfig {
                Zone = DungeonZone.EternalThrone,
                Name = "Eternal Throne",
                Description = "The seat of the dungeon's true master. Final challenge awaits.",
                Difficulty = 10,
                UnlockCost = 50000,
                EnemyMultiplier = 3.0f,
                ScoreMultiplier = 3.0f,
                BossFloor = 50,
                SpecialMechanic = "TimeWarp"
            };
        }

        private void InitializeFloorConfigs() {
            FloorConfigs = new Dictionary<int, FloorConfig>();
            
            for (int floor = 1; floor <= 50; floor++) {
                FloorConfigs[floor] = new FloorConfig {
                    FloorNumber = floor,
                    EnemyCount = 3 + (floor / 5),
                    EnemyLevel = 1 + (floor / 3),
                    IsBossFloor = (floor % 5 == 0),
                    TimeLimit = Math.Max(300, 600 - (floor * 5)),
                    ScoreRequirement = floor * 100,
                    GoldMultiplier = 1.0f + (floor * 0.1f),
                    ExperienceMultiplier = 1.0f + (floor * 0.15f)
                };
            }
        }

        private void InitializeRewardTable() {
            RewardTable = new List<DungeonReward>();
            
            for (int floor = 1; floor <= 50; floor++) {
                bool isBoss = floor % 5 == 0;
                RewardTable.Add(new DungeonReward {
                    FloorNumber = floor,
                    GoldReward = isBoss ? floor * 500 : floor * 100,
                    ExperienceReward = isBoss ? floor * 200 : floor * 50,
                    ItemRewards = GenerateItemRewards(floor, isBoss),
                    IsBossFloor = isBoss
                });
            }
        }

        private List<string> GenerateItemRewards(int floor, bool isBoss) {
            var items = new List<string>();
            int itemCount = isBoss ? 3 : 1;
            
            for (int i = 0; i < itemCount; i++) {
                if (floor >= 40) {
                    items.Add($"LegendaryArtifact_{floor}");
                } else if (floor >= 30) {
                    items.Add($"EpicGear_{floor}");
                } else if (floor >= 20) {
                    items.Add($"RareEquipment_{floor}");
                } else if (floor >= 10) {
                    items.Add($"UncommonItem_{floor}");
                } else {
                    items.Add($"CommonLoot_{floor}");
                }
            }
            
            return items;
        }

        public DungeonConfig GetDungeonConfig(int dungeonId) {
            return DungeonConfigs.ContainsKey(dungeonId) ? DungeonConfigs[dungeonId] : null;
        }

        public ZoneConfig GetZoneConfig(DungeonZone zone) {
            return ZoneConfigs.ContainsKey(zone) ? ZoneConfigs[zone] : null;
        }

        public FloorConfig GetFloorConfig(int floor) {
            return FloorConfigs.ContainsKey(floor) ? FloorConfigs[floor] : null;
        }

        public DungeonReward GetReward(int floor) {
            return RewardTable.Find(r => r.FloorNumber == floor);
        }
    }

    public class DungeonConfig {
        public int DungeonId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RecommendedLevel { get; set; }
        public int Difficulty { get; set; }
        public int MaxFloors { get; set; }
        public string UnlockRequirement { get; set; }
        public DungeonZone Zone { get; set; }
    }

    public class ZoneConfig {
        public DungeonZone Zone { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Difficulty { get; set; }
        public int UnlockCost { get; set; }
        public float EnemyMultiplier { get; set; }
        public float ScoreMultiplier { get; set; }
        public int BossFloor { get; set; }
        public string SpecialMechanic { get; set; }
    }

    public class FloorConfig {
        public int FloorNumber { get; set; }
        public int EnemyCount { get; set; }
        public int EnemyLevel { get; set; }
        public bool IsBossFloor { get; set; }
        public int TimeLimit { get; set; }
        public int ScoreRequirement { get; set; }
        public float GoldMultiplier { get; set; }
        public float ExperienceMultiplier { get; set; }
    }
}
