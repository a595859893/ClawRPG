using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.ParallelDimension;

namespace ClawRPG.Scripts.Systems.ParallelDimension {
    
    public static class ParallelDimensionDatabase {
        
        private static Dictionary<int, DimensionEntry> _dimensions;
        private static Dictionary<DimensionType, DimensionRule> _dimensionRules;
        
        public static void Initialize() {
            _dimensions = new Dictionary<int, DimensionEntry>();
            _dimensionRules = new Dictionary<DimensionType, DimensionRule>();
            
            InitializeDimensionRules();
            InitializeDimensions();
        }
        
        private static void InitializeDimensionRules() {
            _dimensionRules[DimensionType.Mirror] = new DimensionRule {
                Description = "Enemies reflect your attacks. Use elemental combos!",
                EnemyMultiplier = 1.2f,
                DropMultiplier = 1.5f,
                ExpMultiplier = 1.3f,
                NoDeathPenalty = false
            };
            
            _dimensionRules[DimensionType.Void] = new DimensionRule {
                Description = "No mana costs. Skills have no cooldowns!",
                EnemyMultiplier = 1.5f,
                DropMultiplier = 2.0f,
                ExpMultiplier = 1.5f,
                NoCooldowns = true,
                InfiniteMana = true
            };
            
            _dimensionRules[DimensionType.Chaos] = new DimensionRule {
                Description = "Random effects every 10 seconds!",
                EnemyMultiplier = 1.3f,
                DropMultiplier = 1.8f,
                ExpMultiplier = 1.4f,
                NoDeathPenalty = true
            };
            
            _dimensionRules[DimensionType.Frozen] = new DimensionRule {
                Description = "Slowed movement. Ice enemies only!",
                EnemyMultiplier = 1.1f,
                DropMultiplier = 1.6f,
                ExpMultiplier = 1.25f,
                AllowedElements = new string[] { "Fire", "Lightning" }
            };
            
            _dimensionRules[DimensionType.Infernal] = new DimensionRule {
                Description = "Fire enemies only. Take burn damage over time!",
                EnemyMultiplier = 1.4f,
                DropMultiplier = 2.0f,
                ExpMultiplier = 1.5f,
                AllowedElements = new string[] { "Water", "Ice" }
            };
            
            _dimensionRules[DimensionType.Ethereal] = new DimensionRule {
                Description = "Ghost enemies. Physical attacks deal reduced damage!",
                EnemyMultiplier = 1.2f,
                DropMultiplier = 1.7f,
                ExpMultiplier = 1.35f,
                AllowedElements = new string[] { "Holy", "Dark" }
            };
            
            _dimensionRules[DimensionType.Dark] = new DimensionRule {
                Description = "Darkness realm. Visibility reduced!",
                EnemyMultiplier = 1.3f,
                DropMultiplier = 1.8f,
                ExpMultiplier = 1.4f,
                AllowedElements = new string[] { "Light" }
            };
            
            _dimensionRules[DimensionType.Light] = new DimensionRule {
                Description = "Light realm. Dark enemies are stronger!",
                EnemyMultiplier = 1.2f,
                DropMultiplier = 1.7f,
                ExpMultiplier = 1.35f,
                AllowedElements = new string[] { "Dark" }
            };
            
            _dimensionRules[DimensionType.Time] = new DimensionRule {
                Description = "Time flows differently. Enemies move at 50% speed!",
                EnemyMultiplier = 1.0f,
                DropMultiplier = 1.5f,
                ExpMultiplier = 1.2f,
                NoDeathPenalty = true
            };
            
            _dimensionRules[DimensionType.Dream] = new DimensionRule {
                Description = "Dreams become reality. Random buffs and debuffs!",
                EnemyMultiplier = 1.25f,
                DropMultiplier = 1.6f,
                ExpMultiplier = 1.3f,
                NoDeathPenalty = true
            };
        }
        
        private static void InitializeDimensions() {
            AddDimension(1, "Mirror Realm", "A dimension where attacks are reflected", DimensionType.Mirror, 1, 0, 10);
            AddDimension(2, "Void Nexus", "A dimension of pure nothingness", DimensionType.Void, 5, 100, 10);
            AddDimension(3, "Chaos Dimension", "A realm of unpredictable chaos", DimensionType.Chaos, 10, 200, 12);
            AddDimension(4, "Frozen Throne", "An icy kingdom frozen in time", DimensionType.Frozen, 15, 300, 10);
            AddDimension(5, "Infernal Pits", "A burning hellscape of fire", DimensionType.Infernal, 20, 500, 12);
            AddDimension(6, "Ethereal Plane", "A ghostly realm between worlds", DimensionType.Ethereal, 25, 750, 10);
            AddDimension(7, "Shadow Domain", "A realm of eternal darkness", DimensionType.Dark, 30, 1000, 15);
            AddDimension(8, "Radiant Heaven", "A shining realm of light", DimensionType.Light, 30, 1000, 15);
            AddDimension(9, "Temporal Rift", "A fracture in time itself", DimensionType.Time, 40, 2000, 20);
            AddDimension(10, "Dream Maze", "A labyrinth of endless dreams", DimensionType.Dream, 35, 1500, 15);
        }
        
        private static void AddDimension(int id, string name, string desc, DimensionType type, int reqLevel, int cost, int maxFloors) {
            var dimension = new DimensionEntry {
                DimensionId = id,
                DimensionName = name,
                Description = desc,
                Type = type,
                State = id == 1 ? DimensionState.Available : DimensionState.Locked,
                RequiredLevel = reqLevel,
                EntryCost = cost,
                MaxFloors = maxFloors,
                CurrentFloor = 1,
                Rules = _dimensionRules.ContainsKey(type) ? _dimensionRules[type] : new DimensionRule()
            };
            
            _dimensions[id] = dimension;
        }
        
        public static DimensionEntry GetDimension(int id) {
            return _dimensions.ContainsKey(id) ? _dimensions[id] : null;
        }
        
        public static List<DimensionEntry> GetAllDimensions() {
            return new List<DimensionEntry>(_dimensions.Values);
        }
        
        public static List<DimensionEntry> GetUnlockedDimensions() {
            var unlocked = new List<DimensionEntry>();
            foreach (var dim in _dimensions.Values) {
                if (dim.State != DimensionState.Locked) {
                    unlocked.Add(dim);
                }
            }
            return unlocked;
        }
        
        public static DimensionRule GetDimensionRules(DimensionType type) {
            return _dimensionRules.ContainsKey(type) ? _dimensionRules[type] : new DimensionRule();
        }
        
        public static List<DimensionReward> GetFloorRewards(int dimensionId, int floor) {
            var rewards = new List<DimensionReward>();
            var baseGold = 100 * dimensionId;
            var baseExp = 50 * dimensionId;
            
            rewards.Add(new DimensionReward {
                Floor = floor,
                GoldReward = baseGold + (floor * 20),
                ExpReward = baseExp + (floor * 10),
                ItemId = "",
                DropChance = 0f
            });
            
            if (floor % 5 == 0) {
                rewards.Add(new DimensionReward {
                    Floor = floor,
                    GoldReward = baseGold * 2,
                    ExpReward = baseExp * 2,
                    ItemId = $"dimension_treasure_{dimensionId}",
                    DropChance = 1.0f
                });
            }
            
            return rewards;
        }
        
        public static bool UnlockDimension(int dimensionId, int playerLevel) {
            if (!_dimensions.ContainsKey(dimensionId)) return false;
            
            var dim = _dimensions[dimensionId];
            if (dim.State != DimensionState.Locked) return false;
            if (playerLevel < dim.RequiredLevel) return false;
            
            dim.State = DimensionState.Available;
            return true;
        }
    }
}
