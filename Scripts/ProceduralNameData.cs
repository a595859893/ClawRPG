using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Data structure for procedural name generation system
    /// </summary>
    public class ProceduralNameData {
        public List<NameRecord> GenerationHistory { get; set; } = new List<NameRecord>();
        public Dictionary<string, int> TypeUsageCount { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> RarityUsageCount { get; set; } = new Dictionary<string, int>();
        public Dictionary<string, int> StyleUsageCount { get; set; } = new Dictionary<string, int>();
        
        // Statistics
        public int TotalGenerated { get; set; } = 0;
        public int FantasyStyleCount { get; set; } = 0;
        public int ModernStyleCount { get; set; } = 0;
        public int MythicalStyleCount { get; set; } = 0;
        public int AncientStyleCount { get; set; } = 0;
        
        public ProceduralNameData() {
            // Initialize default counts
            TypeUsageCount["Weapon"] = 0;
            TypeUsageCount["Armor"] = 0;
            TypeUsageCount["Accessory"] = 0;
            TypeUsageCount["Potion"] = 0;
            TypeUsageCount["Scroll"] = 0;
            TypeUsageCount["Pet"] = 0;
            TypeUsageCount["Mount"] = 0;
            TypeUsageCount["Material"] = 0;
            
            RarityUsageCount["Common"] = 0;
            RarityUsageCount["Uncommon"] = 0;
            RarityUsageCount["Rare"] = 0;
            RarityUsageCount["Epic"] = 0;
            RarityUsageCount["Legendary"] = 0;
            
            StyleUsageCount["Fantasy"] = 0;
            StyleUsageCount["Modern"] = 0;
            StyleUsageCount["Mythical"] = 0;
            StyleUsageCount["Ancient"] = 0;
        }
    }
    
    public class NameRecord {
        public string Name { get; set; } = "";
        public string Type { get; set; } = "";
        public string Rarity { get; set; } = "";
        public string Style { get; set; } = "";
        public long Timestamp { get; set; } = 0;
    }
}
