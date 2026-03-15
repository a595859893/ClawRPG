using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ProceduralWeaponGeneration {
    /// <summary>
    /// Data structure for procedural weapon generation system
    /// </summary>
    public class ProceduralWeaponData : BaseSystem {
        
        // Weapon generation history
        public List<WeaponGenerationRecord> GenerationHistory { get; set; } = new List<WeaponGenerationRecord>();
        
        // Statistics tracking
        public int TotalWeaponsGenerated { get; set; } = 0;
        public int LegendaryWeapons { get; set; } = 0;
        public int EpicWeapons { get; set; } = 0;
        public int RareWeapons { get; set; } = 0;
        public int TotalGoldSpent { get; set; } = 0;
        public int TotalMaterialsUsed { get; set; } = 0;
        
        // Unlocked weapon types
        public List<string> UnlockedWeaponTypes { get; set; } = new List<string>();
        
        // Unlocked prefixes
        public List<string> UnlockedPrefixes { get; set; } = new List<string>();
        
        // Unlocked suffixes
        public List<string> UnlockedSuffixes { get; set; } = new List<string>();
        
        // Total generation count per rarity
        public Dictionary<string, int> RarityGenerationCount { get; set; } = new Dictionary<string, int>();
        
        public ProceduralWeaponData() {
            // Initialize rarity counts
            RarityGenerationCount["Common"] = 0;
            RarityGenerationCount["Uncommon"] = 0;
            RarityGenerationCount["Rare"] = 0;
            RarityGenerationCount["Epic"] = 0;
            RarityGenerationCount["Legendary"] = 0;
        }
    }
    
    /// <summary>
    /// Record of a single weapon generation
    /// </summary>
    public class WeaponGenerationRecord {
        public string WeaponName { get; set; } = "";
        public string WeaponType { get; set; } = "";
        public string Rarity { get; set; } = "";
        public int Level { get; set; } = 1;
        public int Attack { get; set; } = 0;
        public int Defense { get; set; } = 0;
        public int Speed { get; set; } = 0;
        public List<string> SpecialEffects { get; set; } = new List<string>();
        public DateTime GenerationTime { get; set; } = DateTime.Now;
        public int GoldCost { get; set; } = 0;
        public bool IsReroll { get; set; } = false;
    }
}
