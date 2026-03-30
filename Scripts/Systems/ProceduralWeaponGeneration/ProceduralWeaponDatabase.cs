using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ProceduralWeaponGeneration {
    /// <summary>
    /// Configuration database for procedural weapon generation
    /// </summary>
    public class ProceduralWeaponDatabase : BaseSystem {
        
        // Weapon types available
        public Dictionary<string, WeaponTypeConfig> WeaponTypes { get; set; } = new Dictionary<string, WeaponTypeConfig>();
        
        // Prefix pools by rarity
        public Dictionary<string, List<PrefixConfig>> PrefixPools { get; set; } = new Dictionary<string, List<PrefixConfig>>();
        
        // Suffix pools by rarity
        public Dictionary<string, List<SuffixConfig>> SuffixPools { get; set; } = new Dictionary<string, List<SuffixConfig>>();
        
        // Special effects pools
        public Dictionary<string, List<SpecialEffectConfig>> SpecialEffectPools { get; set; } = new Dictionary<string, List<SpecialEffectConfig>>();
        
        // Rarity configurations
        public Dictionary<string, RarityConfig> Rarities { get; set; } = new Dictionary<string, RarityConfig>();
        
        // Generation costs
        public Dictionary<string, int> GenerationCosts { get; set; } = new Dictionary<string, int>();
        
        public ProceduralWeaponDatabase() {
            InitializeWeaponTypes();
            InitializePrefixes();
            InitializeSuffixes();
            InitializeSpecialEffects();
            InitializeRarities();
            InitializeCosts();
        }
        
        private void InitializeWeaponTypes() {
            // Sword types
            WeaponTypes["Longsword"] = new WeaponTypeConfig {
                Name = "Longsword",
                Category = "Sword",
                BaseAttack = 15,
                BaseDefense = 5,
                BaseSpeed = 8,
                LevelRequirement = 1,
                Weight = 1.0f
            };
            
            WeaponTypes["Dagger"] = new WeaponTypeConfig {
                Name = "Dagger",
                Category = "Sword",
                BaseAttack = 8,
                BaseDefense = 2,
                BaseSpeed = 15,
                LevelRequirement = 1,
                Weight = 0.5f
            };
            
            WeaponTypes["Greatsword"] = new WeaponTypeConfig {
                Name = "Greatsword",
                Category = "Sword",
                BaseAttack = 25,
                BaseDefense = 8,
                BaseSpeed = 4,
                LevelRequirement = 10,
                Weight = 2.0f
            };
            
            // Axe types
            WeaponTypes["Battleaxe"] = new WeaponTypeConfig {
                Name = "Battleaxe",
                Category = "Axe",
                BaseAttack = 22,
                BaseDefense = 6,
                BaseSpeed = 5,
                LevelRequirement = 5,
                Weight = 1.8f
            };
            
            WeaponTypes["Warhammer"] = new WeaponTypeConfig {
                Name = "Warhammer",
                Category = "Axe",
                BaseAttack = 28,
                BaseDefense = 10,
                BaseSpeed = 3,
                LevelRequirement = 15,
                Weight = 2.5f
            };
            
            // Staff types
            WeaponTypes["Quarterstaff"] = new WeaponTypeConfig {
                Name = "Quarterstaff",
                Category = "Staff",
                BaseAttack = 10,
                BaseDefense = 8,
                BaseSpeed = 10,
                LevelRequirement = 1,
                Weight = 1.0f
            };
            
            WeaponTypes["MagicStaff"] = new WeaponTypeConfig {
                Name = "Magic Staff",
                Category = "Staff",
                BaseAttack = 18,
                BaseDefense = 4,
                BaseSpeed = 7,
                LevelRequirement = 8,
                Weight = 1.2f
            };
            
            // Bow types
            WeaponTypes["Shortbow"] = new WeaponTypeConfig {
                Name = "Shortbow",
                Category = "Bow",
                BaseAttack = 12,
                BaseDefense = 2,
                BaseSpeed = 12,
                LevelRequirement = 1,
                Weight = 0.8f
            };
            
            WeaponTypes["Longbow"] = new WeaponTypeConfig {
                Name = "Longbow",
                Category = "Bow",
                BaseAttack = 20,
                BaseDefense = 3,
                BaseSpeed = 9,
                LevelRequirement = 10,
                Weight = 1.5f
            };
            
            // Shield types
            WeaponTypes["WoodenShield"] = new WeaponTypeConfig {
                Name = "Wooden Shield",
                Category = "Shield",
                BaseAttack = 3,
                BaseDefense = 12,
                BaseSpeed = 6,
                LevelRequirement = 1,
                Weight = 1.5f
            };
            
            WeaponTypes["TowerShield"] = new WeaponTypeConfig {
                Name = "Tower Shield",
                Category = "Shield",
                BaseAttack = 5,
                BaseDefense = 20,
                BaseSpeed = 3,
                LevelRequirement = 15,
                Weight = 3.0f
            };
        }
        
        private void InitializePrefixes() {
            // Common prefixes
            PrefixPools["Common"] = new List<PrefixConfig> {
                new PrefixConfig { Name = "Rusty", AttackBonus = 0, DefenseBonus = 0, SpeedBonus = 0, Rarity = "Common" },
                new PrefixConfig { Name = "Worn", AttackBonus = 1, DefenseBonus = 1, SpeedBonus = 0, Rarity = "Common" },
                new PrefixConfig { Name = "Simple", AttackBonus = 2, DefenseBonus = 1, SpeedBonus = 1, Rarity = "Common" },
                new PrefixConfig { Name = "Basic", AttackBonus = 2, DefenseBonus = 2, SpeedBonus = 0, Rarity = "Common" }
            };
            
            // Uncommon prefixes
            PrefixPools["Uncommon"] = new List<PrefixConfig> {
                new PrefixConfig { Name = "Polished", AttackBonus = 4, DefenseBonus = 3, SpeedBonus = 2, Rarity = "Uncommon" },
                new PrefixConfig { Name = "Reinforced", AttackBonus = 3, DefenseBonus = 5, SpeedBonus = 1, Rarity = "Uncommon" },
                new PrefixConfig { Name = "Balanced", AttackBonus = 4, DefenseBonus = 4, SpeedBonus = 3, Rarity = "Uncommon" },
                new PrefixConfig { Name = "Sharp", AttackBonus = 6, DefenseBonus = 2, SpeedBonus = 2, Rarity = "Uncommon" }
            };
            
            // Rare prefixes
            PrefixPools["Rare"] = new List<PrefixConfig> {
                new PrefixConfig { Name = "Enchanted", AttackBonus = 8, DefenseBonus = 6, SpeedBonus = 5, Rarity = "Rare" },
                new PrefixConfig { Name = "Superior", AttackBonus = 10, DefenseBonus = 7, SpeedBonus = 4, Rarity = "Rare" },
                new PrefixConfig { Name = "Masterwork", AttackBonus = 9, DefenseBonus = 8, SpeedBonus = 6, Rarity = "Rare" },
                new PrefixConfig { Name = "Fierce", AttackBonus = 12, DefenseBonus = 5, SpeedBonus = 5, Rarity = "Rare" }
            };
            
            // Epic prefixes
            PrefixPools["Epic"] = new List<PrefixConfig> {
                new PrefixConfig { Name = "Legendary", AttackBonus = 15, DefenseBonus = 12, SpeedBonus = 8, Rarity = "Epic" },
                new PrefixConfig { Name = "Ancient", AttackBonus = 18, DefenseBonus = 14, SpeedBonus = 6, Rarity = "Epic" },
                new PrefixConfig { Name = "Divine", AttackBonus = 20, DefenseBonus = 15, SpeedBonus = 10, Rarity = "Epic" },
                new PrefixConfig { Name = "Cursed", AttackBonus = 25, DefenseBonus = 5, SpeedBonus = 5, Rarity = "Epic" }
            };
            
            // Legendary prefixes
            PrefixPools["Legendary"] = new List<PrefixConfig> {
                new PrefixConfig { Name = "Godslayer", AttackBonus = 35, DefenseBonus = 20, SpeedBonus = 15, Rarity = "Legendary" },
                new PrefixConfig { Name = "Demonheart", AttackBonus = 40, DefenseBonus = 25, SpeedBonus = 10, Rarity = "Legendary" },
                new PrefixConfig { Name = "Worldbreaker", AttackBonus = 45, DefenseBonus = 30, SpeedBonus = 12, Rarity = "Legendary" },
                new PrefixConfig { Name = "Eternity", AttackBonus = 50, DefenseBonus = 35, SpeedBonus = 20, Rarity = "Legendary" }
            };
        }
        
        private void InitializeSuffixes() {
            // Common suffixes
            SuffixPools["Common"] = new List<SuffixConfig> {
                new SuffixConfig { Name = "of the Bear", AttackBonus = 1, DefenseBonus = 2, SpeedBonus = 0, SpecialEffect = "" },
                new SuffixConfig { Name = "of the Wolf", AttackBonus = 2, DefenseBonus = 1, SpeedBonus = 1, SpecialEffect = "" },
                new SuffixConfig { Name = "of the Hawk", AttackBonus = 1, DefenseBonus = 0, SpeedBonus = 3, SpecialEffect = "" }
            };
            
            // Uncommon suffixes
            SuffixPools["Uncommon"] = new List<SuffixConfig> {
                new SuffixConfig { Name = "of the Tiger", AttackBonus = 4, DefenseBonus = 3, SpeedBonus = 2, SpecialEffect = "" },
                new SuffixConfig { Name = "of the Eagle", AttackBonus = 5, DefenseBonus = 2, SpeedBonus = 4, SpecialEffect = "" },
                new SuffixConfig { Name = "of the Panther", AttackBonus = 6, DefenseBonus = 3, SpeedBonus = 3, SpecialEffect = "" }
            };
            
            // Rare suffixes
            SuffixPools["Rare"] = new List<SuffixConfig> {
                new SuffixConfig { Name = "of the Dragon", AttackBonus = 10, DefenseBonus = 8, SpeedBonus = 5, SpecialEffect = "" },
                new SuffixConfig { Name = "of the Phoenix", AttackBonus = 12, DefenseBonus = 6, SpeedBonus = 8, SpecialEffect = "" },
                new SuffixConfig { Name = "of the Storm", AttackBonus = 15, DefenseBonus = 5, SpeedBonus = 10, SpecialEffect = "" }
            };
            
            // Epic suffixes
            SuffixPools["Epic"] = new List<SuffixConfig> {
                new SuffixConfig { Name = "of the Void", AttackBonus = 20, DefenseBonus = 15, SpeedBonus = 12, SpecialEffect = "LifeSteal" },
                new SuffixConfig { Name = "of the Light", AttackBonus = 18, DefenseBonus = 20, SpeedBonus = 10, SpecialEffect = "Holy" },
                new SuffixConfig { Name = "of the Shadow", AttackBonus = 25, DefenseBonus = 12, SpeedBonus = 15, SpecialEffect = "CriticalStrike" }
            };
            
            // Legendary suffixes
            SuffixPools["Legendary"] = new List<SuffixConfig> {
                new SuffixConfig { Name = "of the Cosmos", AttackBonus = 40, DefenseBonus = 35, SpeedBonus = 25, SpecialEffect = "Omnipotent" },
                new SuffixConfig { Name = "of the Infinity", AttackBonus = 50, DefenseBonus = 40, SpeedBonus = 30, SpecialEffect = "Eternal" },
                new SuffixConfig { Name = "of the Universe", AttackBonus = 60, DefenseBonus = 50, SpeedBonus = 35, SpecialEffect = "Worldender" }
            };
        }
        
        private void InitializeSpecialEffects() {
            // Common special effects
            SpecialEffectPools["Common"] = new List<SpecialEffectConfig> {
                new SpecialEffectConfig { Name = "Sharp", Description = "+10% Critical Chance", Trigger = "OnHit", Value = 10 },
                new SpecialEffectConfig { Name = "Durable", Description = "+20% Durability", Trigger = "Passive", Value = 20 }
            };
            
            // Uncommon special effects
            SpecialEffectPools["Uncommon"] = new List<SpecialEffectConfig> {
                new SpecialEffectConfig { Name = "Quick", Description = "+15% Attack Speed", Trigger = "Passive", Value = 15 },
                new SpecialEffectConfig { Name = "Heavy", Description = "+25% Damage", Trigger = "OnHit", Value = 25 },
                new SpecialEffectConfig { Name = "Guard", Description = "+10% Damage Reduction", Trigger = "OnHit", Value = 10 }
            };
            
            // Rare special effects
            SpecialEffectPools["Rare"] = new List<SpecialEffectConfig> {
                new SpecialEffectConfig { Name = "LifeSteal", Description = "+5% Life Steal", Trigger = "OnHit", Value = 5 },
                new SpecialEffectConfig { Name = "Fire", Description = "+20 Fire Damage", Trigger = "OnHit", Value = 20 },
                new SpecialEffectConfig { Name = "Ice", Description = "+20 Ice Damage", Trigger = "OnHit", Value = 20 }
            };
            
            // Epic special effects
            SpecialEffectPools["Epic"] = new List<SpecialEffectConfig> {
                new SpecialEffectConfig { Name = "Explosive", Description = "30% AoE Damage", Trigger = "OnHit", Value = 30 },
                new SpecialEffectConfig { Name = "Poisonous", Description = "10 Damage/second for 5s", Trigger = "OnHit", Value = 10 },
                new SpecialEffectConfig { Name = "Thundering", Description = "50% Stun Chance", Trigger = "OnHit", Value = 50 }
            };
            
            // Legendary special effects
            SpecialEffectPools["Legendary"] = new List<SpecialEffectConfig> {
                new SpecialEffectConfig { Name = "Godly", Description = "+50% All Stats", Trigger = "Passive", Value = 50 },
                new SpecialEffectConfig { Name = "SoulEater", Description = "+15% Life Steal", Trigger = "OnHit", Value = 15 },
                new SpecialEffectConfig { Name = "WorldShatterer", Description = "+100% Damage", Trigger = "OnHit", Value = 100 }
            };
        }
        
        private void InitializeRarities() {
            Rarities["Common"] = new RarityConfig {
                Name = "Common",
                Probability = 0.50f,
                StatMultiplier = 1.0f,
                Color = "#FFFFFF",
                MinEffects = 0,
                MaxEffects = 0
            };
            
            Rarities["Uncommon"] = new RarityConfig {
                Name = "Uncommon",
                Probability = 0.25f,
                StatMultiplier = 1.5f,
                Color = "#00FF00",
                MinEffects = 0,
                MaxEffects = 1
            };
            
            Rarities["Rare"] = new RarityConfig {
                Name = "Rare",
                Probability = 0.15f,
                StatMultiplier = 2.0f,
                Color = "#0080FF",
                MinEffects = 1,
                MaxEffects = 1
            };
            
            Rarities["Epic"] = new RarityConfig {
                Name = "Epic",
                Probability = 0.08f,
                StatMultiplier = 3.0f,
                Color = "#8000FF",
                MinEffects = 1,
                MaxEffects = 2
            };
            
            Rarities["Legendary"] = new RarityConfig {
                Name = "Legendary",
                Probability = 0.02f,
                StatMultiplier = 5.0f,
                Color = "#FF8000",
                MinEffects = 2,
                MaxEffects = 3
            };
        }
        
        private void InitializeCosts() {
            GenerationCosts["Common"] = 50;
            GenerationCosts["Uncommon"] = 100;
            GenerationCosts["Rare"] = 200;
            GenerationCosts["Epic"] = 500;
            GenerationCosts["Legendary"] = 1000;
        }
    }
    
    public class WeaponTypeConfig {
        public string Name { get; set; } = "";
        public string Category { get; set; } = "";
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public int BaseSpeed { get; set; }
        public int LevelRequirement { get; set; }
        public float Weight { get; set; }
    }
    
    public class PrefixConfig {
        public string Name { get; set; } = "";
        public int AttackBonus { get; set; }
        public int DefenseBonus { get; set; }
        public int SpeedBonus { get; set; }
        public string Rarity { get; set; } = "";
    }
    
    public class SuffixConfig {
        public string Name { get; set; } = "";
        public int AttackBonus { get; set; }
        public int DefenseBonus { get; set; }
        public int SpeedBonus { get; set; }
        public string SpecialEffect { get; set; } = "";
    }
    
    public class SpecialEffectConfig {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public string Trigger { get; set; } = "";
        public int Value { get; set; }
    }
    
    public class RarityConfig {
        public string Name { get; set; } = "";
        public float Probability { get; set; }
        public float StatMultiplier { get; set; }
        public string Color { get; set; } = "";
        public int MinEffects { get; set; }
        public int MaxEffects { get; set; }
    }
    
}
