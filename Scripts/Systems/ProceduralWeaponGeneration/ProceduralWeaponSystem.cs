using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ProceduralWeaponGeneration {
    /// <summary>
    /// Core procedural weapon generation system
    /// </summary>
    public class ProceduralWeaponSystem : Node {
        
        private ProceduralWeaponData _data;
        private ProceduralWeaponDatabase _database;
        
        // Random number generator with seed support
        private Random _random;
        private int? _seed;
        
        public ProceduralWeaponSystem() {
            _random = new Random();
        }
        
        public void Initialize(ProceduralWeaponData data, ProceduralWeaponDatabase database) {
            _data = data;
            _database = database;
            GD.Print("[ProceduralWeaponSystem] Initialized");
        }
        
        /// <summary>
        /// Set seed for reproducible generation
        /// </summary>
        public void SetSeed(int seed) {
            _seed = seed;
            _random = new Random(seed);
        }
        
        /// <summary>
        /// Generate a random weapon
        /// </summary>
        public WeaponGenerationRecord GenerateWeapon(string weaponType = "", string rarity = "") {
            // Determine weapon type if not specified
            if (string.IsNullOrEmpty(weaponType)) {
                weaponType = GetRandomWeaponType();
            }
            
            // Determine rarity if not specified
            if (string.IsNullOrEmpty(rarity)) {
                rarity = RollRarity();
            }
            
            // Get rarity config
            var rarityConfig = _database.Rarities[rarity];
            
            // Get weapon type config
            var weaponConfig = _database.WeaponTypes[weaponType];
            
            // Roll for prefix (higher rarity = higher chance)
            PrefixConfig prefix = null;
            if (_random.NextFloat() < GetPrefixChance(rarity)) {
                prefix = GetRandomPrefix(rarity);
            }
            
            // Roll for suffix (higher rarity = higher chance)
            SuffixConfig suffix = null;
            if (_random.NextFloat() < GetSuffixChance(rarity)) {
                suffix = GetRandomSuffix(rarity);
            }
            
            // Calculate base stats with rarity multiplier
            float multiplier = rarityConfig.StatMultiplier;
            int attack = (int)(weaponConfig.BaseAttack * multiplier);
            int defense = (int)(weaponConfig.BaseDefense * multiplier);
            int speed = (int)(weaponConfig.BaseSpeed * multiplier);
            
            // Apply prefix bonuses
            if (prefix != null) {
                attack += prefix.AttackBonus;
                defense += prefix.DefenseBonus;
                speed += prefix.SpeedBonus;
            }
            
            // Apply suffix bonuses
            if (suffix != null) {
                attack += suffix.AttackBonus;
                defense += suffix.DefenseBonus;
                speed += suffix.SpeedBonus;
            }
            
            // Add variance (±10%)
            attack = AddVariance(attack);
            defense = AddVariance(defense);
            speed = AddVariance(speed);
            
            // Generate special effects
            List<string> specialEffects = GenerateSpecialEffects(rarity, rarityConfig);
            
            // Build weapon name
            string weaponName = BuildWeaponName(weaponType, prefix, suffix);
            
            // Create generation record
            var record = new WeaponGenerationRecord {
                WeaponName = weaponName,
                WeaponType = weaponType,
                Rarity = rarity,
                Level = weaponConfig.LevelRequirement,
                Attack = attack,
                Defense = defense,
                Speed = speed,
                SpecialEffects = specialEffects,
                GenerationTime = DateTime.Now,
                GoldCost = _database.GenerationCosts[rarity],
                IsReroll = false
            };
            
            // Update statistics
            UpdateStatistics(record);
            
            return record;
        }
        
        /// <summary>
        /// Reroll an existing weapon (generates new stats, keeps type)
        /// </summary>
        public WeaponGenerationRecord RerollWeapon(WeaponGenerationRecord original) {
            var newWeapon = GenerateWeapon(original.WeaponType, "");
            newWeapon.IsReroll = true;
            newWeapon.GoldCost = (int)(original.GoldCost * 0.5f); // 50% cost for reroll
            return newWeapon;
        }
        
        /// <summary>
        /// Get random weapon type based on level availability
        /// </summary>
        private string GetRandomWeaponType() {
            var availableTypes = new List<string>();
            foreach (var kvp in _database.WeaponTypes) {
                availableTypes.Add(kvp.Key);
            }
            return availableTypes[_random.Next(availableTypes.Count)];
        }
        
        /// <summary>
        /// Roll for rarity based on probability
        /// </summary>
        private string RollRarity() {
            float roll = _random.NextFloat();
            float cumulative = 0f;
            
            foreach (var kvp in _database.Rarities) {
                cumulative += kvp.Value.Probability;
                if (roll < cumulative) {
                    return kvp.Key;
                }
            }
            
            return "Common"; // Default
        }
        
        /// <summary>
        /// Get random prefix based on rarity
        /// </summary>
        private PrefixConfig GetRandomPrefix(string rarity) {
            var prefixes = _database.PrefixPools[rarity];
            return prefixes[_random.Next(prefixes.Count)];
        }
        
        /// <summary>
        /// Get random suffix based on rarity
        /// </summary>
        private SuffixConfig GetRandomSuffix(string rarity) {
            var suffixes = _database.SuffixPools[rarity];
            return suffixes[_random.Next(suffixes.Count)];
        }
        
        /// <summary>
        /// Get prefix chance based on rarity
        /// </summary>
        private float GetPrefixChance(string rarity) {
            switch (rarity) {
                case "Common": return 0.1f;
                case "Uncommon": return 0.3f;
                case "Rare": return 0.6f;
                case "Epic": return 0.9f;
                case "Legendary": return 1.0f;
                default: return 0.1f;
            }
        }
        
        /// <summary>
        /// Get suffix chance based on rarity
        /// </summary>
        private float GetSuffixChance(string rarity) {
            switch (rarity) {
                case "Common": return 0.0f;
                case "Uncommon": return 0.1f;
                case "Rare": return 0.3f;
                case "Epic": return 0.6f;
                case "Legendary": return 0.9f;
                default: return 0.0f;
            }
        }
        
        /// <summary>
        /// Generate special effects based on rarity
        /// </summary>
        private List<string> GenerateSpecialEffects(string rarity, RarityConfig config) {
            var effects = new List<string>();
            
            int effectCount = _random.Next(config.MinEffects, config.MaxEffects + 1);
            if (effectCount == 0) return effects;
            
            var availableEffects = _database.SpecialEffectPools[rarity];
            
            // Shuffle and pick
            for (int i = 0; i < Math.Min(effectCount, availableEffects.Count); i++) {
                int index = _random.Next(availableEffects.Count);
                effects.Add(availableEffects[index].Name);
            }
            
            return effects;
        }
        
        /// <summary>
        /// Build weapon name from components
        /// </summary>
        private string BuildWeaponName(string weaponType, PrefixConfig prefix, SuffixConfig suffix) {
            string name = "";
            
            if (prefix != null) {
                name += prefix.Name + " ";
            }
            
            name += weaponType;
            
            if (suffix != null) {
                name += " " + suffix.Name;
            }
            
            return name;
        }
        
        /// <summary>
        /// Add ±10% variance to a value
        /// </summary>
        private int AddVariance(int value) {
            float variance = (float)(_random.NextDouble() * 0.2 - 0.1); // -10% to +10%
            return (int)(value * (1 + variance));
        }
        
        /// <summary>
        /// Update generation statistics
        /// </summary>
        private void UpdateStatistics(WeaponGenerationRecord record) {
            _data.TotalWeaponsGenerated++;
            _data.TotalGoldSpent += record.GoldCost;
            
            // Update rarity counts
            if (_data.RarityGenerationCount.ContainsKey(record.Rarity)) {
                _data.RarityGenerationCount[record.Rarity]++;
            }
            
            // Update special rarity counts
            switch (record.Rarity) {
                case "Legendary": _data.LegendaryWeapons++; break;
                case "Epic": _data.EpicWeapons++; break;
                case "Rare": _data.RareWeapons++; break;
            }
            
            // Add to history (keep last 50)
            _data.GenerationHistory.Insert(0, record);
            if (_data.GenerationHistory.Count > 50) {
                _data.GenerationHistory.RemoveAt(_data.GenerationHistory.Count - 1);
            }
        }
        
        /// <summary>
        /// Get generation cost for rarity
        /// </summary>
        public int GetGenerationCost(string rarity) {
            return _database.GenerationCosts.ContainsKey(rarity) ? _database.GenerationCosts[rarity] : 50;
        }
        
        /// <summary>
        /// Get reroll cost for a weapon
        /// </summary>
        public int GetRerollCost(WeaponGenerationRecord weapon) {
            return (int)(GetGenerationCost(weapon.Rarity) * 0.5f);
        }
        
        /// <summary>
        /// Get weapon type config
        /// </summary>
        public WeaponTypeConfig GetWeaponTypeConfig(string weaponType) {
            return _database.WeaponTypes.ContainsKey(weaponType) ? _database.WeaponTypes[weaponType] : null;
        }
        
        /// <summary>
        /// Get rarity config
        /// </summary>
        public RarityConfig GetRarityConfig(string rarity) {
            return _database.Rarities.ContainsKey(rarity) ? _database.Rarities[rarity] : null;
        }
        
        /// <summary>
        /// Get all available weapon types
        /// </summary>
        public List<string> GetAvailableWeaponTypes() {
            return new List<string>(_database.WeaponTypes.Keys);
        }
        
        /// <summary>
        /// Get statistics summary
        /// </summary>
        public Dictionary<string, object> GetStatistics() {
            return new Dictionary<string, object> {
                { "TotalWeaponsGenerated", _data.TotalWeaponsGenerated },
                { "LegendaryWeapons", _data.LegendaryWeapons },
                { "EpicWeapons", _data.EpicWeapons },
                { "RareWeapons", _data.RareWeapons },
                { "TotalGoldSpent", _data.TotalGoldSpent },
                { "TotalMaterialsUsed", _data.TotalMaterialsUsed },
                { "RarityDistribution", _data.RarityGenerationCount }
            };
        }
        
        /// <summary>
        /// Save data to file
        /// </summary>
        public void SaveData() {
            // TODO: Implement save to file
            GD.Print("[ProceduralWeaponSystem] Save not implemented");
        }
        
        /// <summary>
        /// Load data from file
        /// </summary>
        public void LoadData() {
            // TODO: Implement load from file
            GD.Print("[ProceduralWeaponSystem] Load not implemented");
        }
    }
}
