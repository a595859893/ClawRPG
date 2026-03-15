using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Core system for procedural name generation
    /// </summary>
    public class ProceduralNameSystem {
        private ProceduralNameData _data;
        private Random _random;
        private int? _seed;
        
        public ProceduralNameSystem() {
            _data = new ProceduralNameData();
            _random = new Random();
        }
        
        public ProceduralNameSystem(int seed) {
            _data = new ProceduralNameData();
            _seed = seed;
            _random = new Random(seed);
        }
        
        /// <summary>
        /// Generate a random item name
        /// </summary>
        public string GenerateName(string type = "", string rarity = "", string style = "", int? seed = null) {
            var rng = seed.HasValue ? new Random(seed.Value) : _random;
            
            // Use defaults if not specified
            if (string.IsNullOrEmpty(type)) {
                type = ProceduralNameDatabase.GetAllTypes()[rng.Next(ProceduralNameDatabase.GetAllTypes().Length)];
            }
            if (string.IsNullOrEmpty(rarity)) {
                rarity = GetRandomRarity(rng);
            }
            if (string.IsNullOrEmpty(style)) {
                style = ProceduralNameDatabase.GetAllStyles()[rng.Next(ProceduralNameDatabase.GetAllStyles().Length)];
            }
            
            // Build name components
            string prefix = GetPrefix(rarity, rng);
            string middle = "";
            string suffix = GetSuffix(rarity, type, rng);
            
            // 50% chance to use middle part
            if (rng.Next(2) == 0) {
                middle = ProceduralNameDatabase.MiddleParts[rng.Next(ProceduralNameDatabase.MiddleParts.Length)];
            }
            
            // Build final name
            string name;
            if (rng.Next(3) == 0) {
                // Format: "Prefix Suffix"
                name = $"{prefix} {suffix}";
            } else if (!string.IsNullOrEmpty(middle)) {
                // Format: "Prefix Middle Suffix"
                name = $"{prefix} {middle} {suffix}";
            } else {
                // Format: "Prefix of the Suffix"
                name = $"{prefix} of the {suffix}";
            }
            
            // Record generation
            RecordGeneration(name, type, rarity, style);
            
            return name;
        }
        
        /// <summary>
        /// Get prefix based on rarity
        /// </summary>
        private string GetPrefix(string rarity, Random rng) {
            var prefixes = ProceduralNameDatabase.GetPrefixesByRarity(rarity);
            return prefixes[rng.Next(prefixes.Length)];
        }
        
        /// <summary>
        /// Get suffix based on rarity and item type
        /// </summary>
        private string GetSuffix(string rarity, string type, Random rng) {
            // 70% chance to use type-specific suffix
            if (rng.Next(10) < 7 && ProceduralNameDatabase.TypeSuffixes.ContainsKey(type)) {
                var typeSuffixes = ProceduralNameDatabase.TypeSuffixes[type];
                return typeSuffixes[rng.Next(typeSuffixes.Length)];
            }
            
            // Otherwise use rarity-based suffix
            var suffixes = ProceduralNameDatabase.GetSuffixesByRarity(rarity);
            return suffixes[rng.Next(suffixes.Length)];
        }
        
        /// <summary>
        /// Get random rarity based on weighted probability
        /// </summary>
        private string GetRandomRarity(Random rng) {
            int roll = rng.Next(100);
            if (roll < 50) return "Common";      // 50%
            if (roll < 75) return "Uncommon";    // 25%
            if (roll < 90) return "Rare";        // 15%
            if (roll < 98) return "Epic";        // 8%
            return "Legendary";                   // 2%
        }
        
        /// <summary>
        /// Record a name generation
        /// </summary>
        private void RecordGeneration(string name, string type, string rarity, string style) {
            var record = new NameRecord {
                Name = name,
                Type = type,
                Rarity = rarity,
                Style = style,
                Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
            };
            
            _data.GenerationHistory.Insert(0, record);
            
            // Keep only last 50 records
            if (_data.GenerationHistory.Count > 50) {
                _data.GenerationHistory.RemoveAt(_data.GenerationHistory.Count - 1);
            }
            
            // Update statistics
            _data.TotalGenerated++;
            
            if (_data.TypeUsageCount.ContainsKey(type)) {
                _data.TypeUsageCount[type]++;
            }
            
            if (_data.RarityUsageCount.ContainsKey(rarity)) {
                _data.RarityUsageCount[rarity]++;
            }
            
            if (_data.StyleUsageCount.ContainsKey(style)) {
                _data.StyleUsageCount[style]++;
            }
            
            // Update style-specific counts
            switch (style) {
                case "Fantasy": _data.FantasyStyleCount++; break;
                case "Modern": _data.ModernStyleCount++; break;
                case "Mythical": _data.MythicalStyleCount++; break;
                case "Ancient": _data.AncientStyleCount++; break;
            }
        }
        
        /// <summary>
        /// Generate multiple names at once
        /// </summary>
        public List<string> GenerateMultiple(int count, string type = "", string rarity = "", string style = "") {
            var names = new List<string>();
            for (int i = 0; i < count; i++) {
                names.Add(GenerateName(type, rarity, style));
            }
            return names;
        }
        
        /// <summary>
        /// Get data for UI
        /// </summary>
        public ProceduralNameData GetData() {
            return _data;
        }
        
        /// <summary>
        /// Get generation history
        /// </summary>
        public List<NameRecord> GetHistory() {
            return _data.GenerationHistory;
        }
        
        /// <summary>
        /// Get statistics
        /// </summary>
        public Dictionary<string, int> GetStatistics() {
            return new Dictionary<string, int> {
                { "TotalGenerated", _data.TotalGenerated },
                { "FantasyStyle", _data.FantasyStyleCount },
                { "ModernStyle", _data.ModernStyleCount },
                { "MythicalStyle", _data.MythicalStyleCount },
                { "AncientStyle", _data.AncientStyleCount }
            };
        }
        
        /// <summary>
        /// Load data from save
        /// </summary>
        public void LoadData(ProceduralNameData data) {
            _data = data ?? new ProceduralNameData();
        }
        
        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStatistics() {
            _data = new ProceduralNameData();
        }
    }
}
