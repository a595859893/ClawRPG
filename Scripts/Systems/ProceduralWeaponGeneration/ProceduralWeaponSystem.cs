using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ProceduralWeaponGeneration {
    /// <summary>
    /// Core procedural weapon generation system
    /// </summary>
    public partial class ProceduralWeaponSystem : BaseSystem {
        
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
            var saveSystem = SaveSystem.Instance;
            if (saveSystem == null) return;

            var data = saveSystem.LoadGame();
            if (data == null) data = new Godot.Collections.Dictionary();

            // Save generation history (limit to last 50)
            var historyArray = new Godot.Array();
            var recentHistory = _data.GenerationHistory.TakeLast(50).ToList();
            foreach (var record in recentHistory)
            {
                var recordData = new Godot.Collections.Dictionary();
                recordData["weapon_name"] = record.WeaponName;
                recordData["weapon_type"] = record.WeaponType;
                recordData["rarity"] = record.Rarity;
                recordData["level"] = record.Level;
                recordData["attack"] = record.Attack;
                recordData["defense"] = record.Defense;
                recordData["speed"] = record.Speed;
                
                var effectsArray = new Godot.Array();
                foreach (var effect in record.SpecialEffects)
                {
                    effectsArray.Add(effect);
                }
                recordData["special_effects"] = effectsArray;
                
                if (record.GenerationTime != default(DateTime))
                    recordData["generation_time"] = record.GenerationTime.ToString("o");
                recordData["gold_cost"] = record.GoldCost;
                recordData["is_reroll"] = record.IsReroll;
                historyArray.Add(recordData);
            }
            data["procedural_weapon_history"] = historyArray;

            // Save statistics
            data["procedural_weapon_total"] = _data.TotalWeaponsGenerated;
            data["procedural_weapon_legendary"] = _data.LegendaryWeapons;
            data["procedural_weapon_epic"] = _data.EpicWeapons;
            data["procedural_weapon_rare"] = _data.RareWeapons;
            data["procedural_weapon_gold_spent"] = _data.TotalGoldSpent;
            data["procedural_weapon_materials_used"] = _data.TotalMaterialsUsed;

            // Save unlocked types
            var unlockedTypesArray = new Godot.Array();
            foreach (var type in _data.UnlockedWeaponTypes)
            {
                unlockedTypesArray.Add(type);
            }
            data["procedural_weapon_unlocked_types"] = unlockedTypesArray;

            // Save unlocked prefixes
            var unlockedPrefixesArray = new Godot.Array();
            foreach (var prefix in _data.UnlockedPrefixes)
            {
                unlockedPrefixesArray.Add(prefix);
            }
            data["procedural_weapon_unlocked_prefixes"] = unlockedPrefixesArray;

            // Save unlocked suffixes
            var unlockedSuffixesArray = new Godot.Array();
            foreach (var suffix in _data.UnlockedSuffixes)
            {
                unlockedSuffixesArray.Add(suffix);
            }
            data["procedural_weapon_unlocked_suffixes"] = unlockedSuffixesArray;

            // Save rarity distribution
            var rarityDistDict = new Godot.Collections.Dictionary();
            foreach (var kvp in _data.RarityGenerationCount)
            {
                rarityDistDict[kvp.Key] = kvp.Value;
            }
            data["procedural_weapon_rarity_dist"] = rarityDistDict;

            saveSystem.SaveGame(data);
            GD.Print("[ProceduralWeaponSystem] Data saved successfully");
        }
        
        /// <summary>
        /// Load data from file
        /// </summary>
        public void LoadData() {
            var saveSystem = SaveSystem.Instance;
            if (saveSystem == null) return;

            var data = saveSystem.LoadGame();
            if (data == null) return;

            // Load generation history
            if (data.ContainsKey("procedural_weapon_history")) {
                var historyArray = (Godot.Array)data["procedural_weapon_history"];
                _data.GenerationHistory.Clear();
                foreach (Godot.Collections.Dictionary recordData in historyArray)
                {
                    var record = new WeaponGenerationRecord();
                    record.WeaponName = recordData.ContainsKey("weapon_name") ? (string)recordData["weapon_name"] : "";
                    record.WeaponType = recordData.ContainsKey("weapon_type") ? (string)recordData["weapon_type"] : "";
                    record.Rarity = recordData.ContainsKey("rarity") ? (string)recordData["rarity"] : "";
                    record.Level = recordData.ContainsKey("level") ? (int)recordData["level"] : 1;
                    record.Attack = recordData.ContainsKey("attack") ? (int)recordData["attack"] : 0;
                    record.Defense = recordData.ContainsKey("defense") ? (int)recordData["defense"] : 0;
                    record.Speed = recordData.ContainsKey("speed") ? (int)recordData["speed"] : 0;
                    
                    if (recordData.ContainsKey("special_effects")) {
                        var effectsArray = (Godot.Array)recordData["special_effects"];
                        foreach (string effect in effectsArray)
                        {
                            record.SpecialEffects.Add(effect);
                        }
                    }
                    
                    if (recordData.ContainsKey("generation_time") && recordData["generation_time"] != null) {
                        if (DateTime.TryParse((string)recordData["generation_time"], out var genTime)) {
                            record.GenerationTime = genTime;
                        }
                    }
                    record.GoldCost = recordData.ContainsKey("gold_cost") ? (int)recordData["gold_cost"] : 0;
                    record.IsReroll = recordData.ContainsKey("is_reroll") && (bool)recordData["is_reroll"];
                    _data.GenerationHistory.Add(record);
                }
            }

            // Load statistics
            if (data.ContainsKey("procedural_weapon_total"))
                _data.TotalWeaponsGenerated = (int)data["procedural_weapon_total"];
            if (data.ContainsKey("procedural_weapon_legendary"))
                _data.LegendaryWeapons = (int)data["procedural_weapon_legendary"];
            if (data.ContainsKey("procedural_weapon_epic"))
                _data.EpicWeapons = (int)data["procedural_weapon_epic"];
            if (data.ContainsKey("procedural_weapon_rare"))
                _data.RareWeapons = (int)data["procedural_weapon_rare"];
            if (data.ContainsKey("procedural_weapon_gold_spent"))
                _data.TotalGoldSpent = (int)data["procedural_weapon_gold_spent"];
            if (data.ContainsKey("procedural_weapon_materials_used"))
                _data.TotalMaterialsUsed = (int)data["procedural_weapon_materials_used"];

            // Load unlocked types
            if (data.ContainsKey("procedural_weapon_unlocked_types")) {
                var typesArray = (Godot.Array)data["procedural_weapon_unlocked_types"];
                _data.UnlockedWeaponTypes.Clear();
                foreach (string type in typesArray)
                {
                    _data.UnlockedWeaponTypes.Add(type);
                }
            }

            // Load unlocked prefixes
            if (data.ContainsKey("procedural_weapon_unlocked_prefixes")) {
                var prefixesArray = (Godot.Array)data["procedural_weapon_unlocked_prefixes"];
                _data.UnlockedPrefixes.Clear();
                foreach (string prefix in prefixesArray)
                {
                    _data.UnlockedPrefixes.Add(prefix);
                }
            }

            // Load unlocked suffixes
            if (data.ContainsKey("procedural_weapon_unlocked_suffixes")) {
                var suffixesArray = (Godot.Array)data["procedural_weapon_unlocked_suffixes"];
                _data.UnlockedSuffixes.Clear();
                foreach (string suffix in suffixesArray)
                {
                    _data.UnlockedSuffixes.Add(suffix);
                }
            }

            // Load rarity distribution
            if (data.ContainsKey("procedural_weapon_rarity_dist")) {
                var rarityDistDict = (Godot.Collections.Dictionary)data["procedural_weapon_rarity_dist"];
                foreach (var key in rarityDistDict.Keys)
                {
                    _data.RarityGenerationCount[(string)key] = (int)rarityDistDict[key];
                }
            }

            GD.Print("[ProceduralWeaponSystem] Data loaded successfully");
        }
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
}
