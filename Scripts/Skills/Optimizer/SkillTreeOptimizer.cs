using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Skills.Optimizer {
    /// <summary>
    /// Skill tree optimization options
    /// </summary>
    public enum OptimizationType {
        FullReset,        // Reset entire skill tree
        BranchReset,      // Reset single branch
        PathSwap,         // Swap between paths
        CostReduction,    // Optimize point costs
        Efficiency        // Optimize for efficiency
    }

    /// <summary>
    /// Skill tree optimization preset
    /// </summary>
    public class SkillTreePreset {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public Dictionary<string, int> SkillPoints { get; set; }
        public OptimizationType Type { get; set; }
    }

    /// <summary>
    /// Skill tree optimization record
    /// </summary>
    public class SkillTreeOptimization {
        public string Id { get; set; }
        public DateTime Timestamp { get; set; }
        public OptimizationType Type { get; set; }
        public int PointsSpent { get; set; }
        public int PointsRefunded { get; set; }
        public Dictionary<string, int> SkillsModified { get; set; }
    }

    /// <summary>
    /// Player's optimization progress
    /// </summary>
    public class SkillTreeOptimizationProgress {
        public int TotalOptimizations { get; set; }
        public int PointsSaved { get; set; }
        public List<SkillTreeOptimization> History { get; set; }
        public List<string> UnlockedPresets { get; set; }
        public Dictionary<string, int> PresetUsageCount { get; set; }
    }

    /// <summary>
    /// Skill tree optimizer system
    /// </summary>
    public class SkillTreeOptimizer : Node {
        public static SkillTreeOptimizer Instance { get; private set; }

        private Dictionary<string, SkillTreePreset> _presets;
        private SkillTreeOptimizationProgress _progress;
        private const string SAVE_KEY = "skill_tree_optimizer";

        public override void _Ready() {
            Instance = this;
            _presets = new Dictionary<string, SkillTreePreset>();
            _progress = new SkillTreeOptimizationProgress {
                History = new List<SkillTreeOptimization>(),
                UnlockedPresets = new List<string>(),
                PresetUsageCount = new Dictionary<string, int>()
            };
            InitializePresets();
        }

        private void InitializePresets() {
            // Combat-focused preset
            _presets["combat_ focus"] = new SkillTreePreset {
                Id = "combat_ focus",
                Name = "Combat Focus",
                Description = "Optimized for maximum combat effectiveness",
                Type = OptimizationType.Efficiency,
                SkillPoints = new Dictionary<string, int> {
                    { "attack", 10 },
                    { "defense", 5 },
                    { "critical", 8 },
                    { "speed", 7 }
                }
            };

            // Tank-focused preset
            _presets["tank_focus"] = new SkillTreePreset {
                Id = "tank_focus",
                Name = "Tank Focus",
                Description = "Optimized for maximum survivability",
                Type = OptimizationType.Efficiency,
                SkillPoints = new Dictionary<string, int> {
                    { "defense", 10 },
                    { "health", 10 },
                    { "resistance", 5 },
                    { "recovery", 5 }
                }
            };

            // Magic-focused preset
            _presets["magic_focus"] = new SkillTreePreset {
                Id = "magic_focus",
                Name = "Magic Focus",
                Description = "Optimized for spellcasting",
                Type = OptimizationType.Efficiency,
                SkillPoints = new Dictionary<string, int> {
                    { "intelligence", 10 },
                    { "mana", 10 },
                    { "spell_power", 8 },
                    { "cooldown", 2 }
                }
            };

            // Hybrid preset
            _presets["hybrid_focus"] = new SkillTreePreset {
                Id = "hybrid_focus",
                Name = "Hybrid Focus",
                Description = "Balanced combat and magic",
                Type = OptimizationType.Efficiency,
                SkillPoints = new Dictionary<string, int> {
                    { "attack", 5 },
                    { "intelligence", 5 },
                    { "defense", 5 },
                    { "speed", 5 }
                }
            };

            // Speed-focused preset
            _presets["speed_focus"] = new SkillTreePreset {
                Id = "speed_focus",
                Name = "Speed Focus",
                Description = "Optimized for fast gameplay",
                Type = OptimizationType.Efficiency,
                SkillPoints = new Dictionary<string, int> {
                    { "speed", 10 },
                    { "evasion", 8 },
                    { "attack_speed", 7 },
                    { "cooldown", 5 }
                }
            };

            // Unlock default presets
            _progress.UnlockedPresets.Add("combat_ focus");
            _progress.UnlockedPresets.Add("tank_focus");
            _progress.UnlockedPresets.Add("magic_focus");
        }

        /// <summary>
        /// Apply a preset to optimize skill tree
        /// </summary>
        public bool ApplyPreset(string presetId, Dictionary<string, int> currentPoints) {
            if (!_presets.ContainsKey(presetId)) return false;
            if (!_progress.UnlockedPresets.Contains(presetId)) return false;

            var preset = _presets[presetId];
            var optimization = new SkillTreeOptimization {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                Type = preset.Type,
                SkillsModified = new Dictionary<string, int>()
            };

            // Calculate points to redistribute
            int totalCurrent = 0;
            int totalPreset = 0;

            foreach (var kvp in currentPoints) {
                totalCurrent += kvp.Value;
            }
            foreach (var kvp in preset.SkillPoints) {
                totalPreset += kvp.Value;
            }

            optimization.PointsSpent = totalPreset;
            optimization.PointsRefunded = totalCurrent;

            // Calculate savings
            int savings = CalculateOptimizationSavings(currentPoints, preset.SkillPoints);
            _progress.PointsSaved += savings;

            // Apply preset points
            foreach (var kvp in preset.SkillPoints) {
                optimization.SkillsModified[kvp.Key] = kvp.Value;
            }

            // Record optimization
            _progress.History.Add(optimization);
            _progress.TotalOptimizations++;

            // Track preset usage
            if (_progress.PresetUsageCount.ContainsKey(presetId)) {
                _progress.PresetUsageCount[presetId]++;
            } else {
                _progress.PresetUsageCount[presetId] = 1;
            }

            // Unlock new presets based on usage
            CheckAndUnlockPresets();

            SaveProgress();
            return true;
        }

        /// <summary>
        /// Reset entire skill tree
        /// </summary>
        public Dictionary<string, int> FullReset(Dictionary<string, int> currentPoints) {
            var optimization = new SkillTreeOptimization {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                Type = OptimizationType.FullReset,
                SkillsModified = new Dictionary<string, int>()
            };

            int refunded = 0;
            foreach (var kvp in currentPoints) {
                refunded += kvp.Value;
                optimization.SkillsModified[kvp.Key] = 0;
            }

            optimization.PointsSpent = 0;
            optimization.PointsRefunded = refunded;
            _progress.History.Add(optimization);
            _progress.TotalOptimizations++;

            SaveProgress();
            return optimization.SkillsModified;
        }

        /// <summary>
        /// Reset a single branch
        /// </summary>
        public Dictionary<string, int> BranchReset(string branch, Dictionary<string, int> currentPoints) {
            var optimization = new SkillTreeOptimization {
                Id = Guid.NewGuid().ToString(),
                Timestamp = DateTime.Now,
                Type = OptimizationType.BranchReset,
                SkillsModified = new Dictionary<string, int>()
            };

            int refunded = 0;
            foreach (var kvp in currentPoints) {
                if (kvp.Key.StartsWith(branch + "_")) {
                    refunded += kvp.Value;
                    optimization.SkillsModified[kvp.Key] = 0;
                }
            }

            optimization.PointsSpent = 0;
            optimization.PointsRefunded = refunded;
            _progress.History.Add(optimization);
            _progress.TotalOptimizations++;

            SaveProgress();
            return optimization.SkillsModified;
        }

        /// <summary>
        /// Calculate optimization savings
        /// </summary>
        private int CalculateOptimizationSavings(Dictionary<string, int> current, Dictionary<string, int> target) {
            int savings = 0;

            foreach (var kvp in target) {
                if (current.ContainsKey(kvp.Key)) {
                    // Calculate cost difference
                    int currentCost = CalculateSkillCost(current[kvp.Key]);
                    int targetCost = CalculateSkillCost(kvp.Value);
                    savings += Math.Max(0, currentCost - targetCost);
                }
            }

            return savings;
        }

        /// <summary>
        /// Calculate skill point cost (progressive)
        /// </summary>
        private int CalculateSkillCost(int points) {
            return points * (points + 1) / 2; // Triangular number
        }

        /// <summary>
        /// Check and unlock new presets
        /// </summary>
        private void CheckAndUnlockPresets() {
            if (_progress.TotalOptimizations >= 5 && !_progress.UnlockedPresets.Contains("hybrid_focus")) {
                _progress.UnlockedPresets.Add("hybrid_focus");
            }
            if (_progress.PointsSaved >= 50 && !_progress.UnlockedPresets.Contains("speed_focus")) {
                _progress.UnlockedPresets.Add("speed_focus");
            }
        }

        /// <summary>
        /// Get available presets
        /// </summary>
        public List<SkillTreePreset> GetAvailablePresets() {
            var result = new List<SkillTreePreset>();
            foreach (var id in _progress.UnlockedPresets) {
                if (_presets.ContainsKey(id)) {
                    result.Add(_presets[id]);
                }
            }
            return result;
        }

        /// <summary>
        /// Get optimization progress
        /// </summary>
        public SkillTreeOptimizationProgress GetProgress() {
            return _progress;
        }

        /// <summary>
        /// Get optimization history
        /// </summary>
        public List<SkillTreeOptimization> GetHistory() {
            return _progress.History;
        }

        /// <summary>
        /// Save progress
        /// </summary>
        private void SaveProgress() {
            // Integration with save system would go here
        }

        /// <summary>
        /// Load progress
        /// </summary>
        public void LoadProgress(SkillTreeOptimizationProgress progress) {
            _progress = progress;
            if (_progress.History == null) {
                _progress.History = new List<SkillTreeOptimization>();
            }
            if (_progress.UnlockedPresets == null) {
                _progress.UnlockedPresets = new List<string>();
            }
            if (_progress.PresetUsageCount == null) {
                _progress.PresetUsageCount = new Dictionary<string, int>();
            }
        }
    }
}
