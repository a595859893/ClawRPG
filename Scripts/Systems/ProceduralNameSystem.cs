using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Core system for procedural name generation
    /// </summary>
    public class ProceduralNameSystem : BaseSystem
    {
        private static ProceduralNameSystem _instance;
        public static ProceduralNameSystem Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = GetNode<ProceduralNameSystem>("/root/ProceduralNameSystem");
                    if (_instance == null)
                    {
                        var node = new ProceduralNameSystem();
                        node.Name = "ProceduralNameSystem";
                        Engine.GetMainLoop().Root.AddChild(node);
                    }
                }
                return _instance;
            }
        }
        
        private ProceduralNameData _data;
        private Random _random;
        private int? _seed;
        
        protected override void Initialize()
        {
            base.Initialize();
            
            _data = new ProceduralNameData();
            _random = new Random();
            
            // 注册到保存系统
            SaveSystem.Instance?.Register(this);
            
            GD.Print("[ProceduralNameSystem] Initialized");
        }
        
        /// <summary>
        /// Generate a random item name
        /// </summary>
        public string GenerateName(string type = "", string rarity = "", string style = "", int? seed = null) {
            var rng = seed.HasValue ? new Random(seed.Value) : _random;
            
            // Use defaults if not specified
            if (string.IsNullOrEmpty(type)) {
                var types = ProceduralNameDatabase.GetAllTypes();
                if (types.Length > 0)
                    type = types[rng.Next(types.Length)];
            }
            if (string.IsNullOrEmpty(rarity)) {
                rarity = GetRandomRarity(rng);
            }
            if (string.IsNullOrEmpty(style)) {
                var styles = ProceduralNameDatabase.GetAllStyles();
                if (styles.Length > 0)
                    style = styles[rng.Next(styles.Length)];
            }
            
            // Build name components
            string prefix = GetPrefix(rarity, rng);
            string middle = "";
            string suffix = GetSuffix(rarity, type, rng);
            
            // 50% chance to use middle part
            if (rng.Next(2) == 0) {
                var middles = ProceduralNameDatabase.MiddleParts;
                if (middles.Length > 0)
                    middle = middles[rng.Next(middles.Length)];
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
            return prefixes.Length > 0 ? prefixes[rng.Next(prefixes.Length)] : "Mysterious";
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
            return suffixes.Length > 0 ? suffixes[rng.Next(suffixes.Length)] : "Unknown";
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
            } else {
                _data.TypeUsageCount[type] = 1;
            }
            
            if (_data.RarityUsageCount.ContainsKey(rarity)) {
                _data.RarityUsageCount[rarity]++;
            } else {
                _data.RarityUsageCount[rarity] = 1;
            }
            
            if (_data.StyleUsageCount.ContainsKey(style)) {
                _data.StyleUsageCount[style]++;
            } else {
                _data.StyleUsageCount[style] = 1;
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
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            data["total_generated"] = _data.TotalGenerated;
            data["fantasy_style_count"] = _data.FantasyStyleCount;
            data["modern_style_count"] = _data.ModernStyleCount;
            data["mythical_style_count"] = _data.MythicalStyleCount;
            data["ancient_style_count"] = _data.AncientStyleCount;
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            _data.TotalGenerated = data.Contains("total_generated") ? (int)data["total_generated"] : 0;
            _data.FantasyStyleCount = data.Contains("fantasy_style_count") ? (int)data["fantasy_style_count"] : 0;
            _data.ModernStyleCount = data.Contains("modern_style_count") ? (int)data["modern_style_count"] : 0;
            _data.MythicalStyleCount = data.Contains("mythical_style_count") ? (int)data["mythical_style_count"] : 0;
            _data.AncientStyleCount = data.Contains("ancient_style_count") ? (int)data["ancient_style_count"] : 0;
        }
        
        /// <summary>
        /// 获取系统ID
        /// </summary>
        public override string GetId()
        {
            return "ProceduralNameSystem";
        }
        
        /// <summary>
        /// Reset statistics
        /// </summary>
        public void ResetStatistics() {
            _data = new ProceduralNameData();
        }
    }
}
