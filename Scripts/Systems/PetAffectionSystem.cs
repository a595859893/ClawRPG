using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Manages pet affection system - tracks and increases player-pet relationship
    /// </summary>
    public class PetAffectionSystem {
        public static PetAffectionSystem Instance { get; private set; }
        
        private PlayerAffectionData _playerAffectionData = new PlayerAffectionData();
        private Dictionary<string, PetAffectionData> _petAffection => _playerAffectionData.PetAffection;
        
        // Affection gain values
        private const int FEED_AFFECTION = 50;
        private const int PLAY_AFFECTION = 30;
        private const int BATTLE_WIN_AFFECTION = 100;
        private const int BATTLE_PARTICIPATION_AFFECTION = 20;
        private const int DAILY_INTERACTION_BONUS = 25;
        
        // Affection multipliers
        private const float RARE_PET_MULTIPLIER = 1.5f;
        private const float EPIC_PET_MULTIPLIER = 2.0f;
        private const float LEGENDARY_PET_MULTIPLIER = 3.0f;
        
        public signal AffectionChanged(string petId, int newAffection, int level);
        public signal AffectionLevelUp(string petId, int newLevel);
        
        public PetAffectionSystem() {
            Instance = this;
        }
        
        public void Initialize() {
            GD.Print("[PetAffectionSystem] Initialized");
        }
        
        /// <summary>
        /// Get or create affection data for a pet
        /// </summary>
        public PetAffectionData GetOrCreateAffection(string petId, string rarity = "Common") {
            if (!_petAffection.ContainsKey(petId)) {
                _petAffection[petId] = new PetAffectionData {
                    PetId = petId,
                    CurrentAffection = 0,
                    TotalInteractionCount = 0,
                    FeedCount = 0,
                    PlayCount = 0,
                    BattleCount = 0,
                    LastInteractionTime = 0
                };
            }
            return _petAffection[petId];
        }
        
        /// <summary>
        /// Feed a pet to increase affection
        /// </summary>
        public int FeedPet(string petId, string rarity = "Common") {
            var affection = GetOrCreateAffection(petId, rarity);
            int oldLevel = affection.GetAffectionLevelNumber();
            
            float multiplier = GetRarityMultiplier(rarity);
            int gain = (int)(FEED_AFFECTION * multiplier);
            
            affection.CurrentAffection += gain;
            affection.FeedCount++;
            affection.TotalInteractionCount++;
            affection.LastInteractionTime = OS.GetUnixTime();
            
            int newLevel = affection.GetAffectionLevelNumber();
            
            AffectionChanged?.Call(petId, affection.CurrentAffection, newLevel);
            
            if (newLevel > oldLevel) {
                AffectionLevelUp?.Call(petId, newLevel);
                GD.Print($"[PetAffection] {petId} leveled up to {affection.GetAffectionTitle()}!");
            }
            
            return gain;
        }
        
        /// <summary>
        /// Play with pet to increase affection
        /// </summary>
        public int PlayWithPet(string petId, string rarity = "Common") {
            var affection = GetOrCreateAffection(petId, rarity);
            int oldLevel = affection.GetAffectionLevelNumber();
            
            float multiplier = GetRarityMultiplier(rarity);
            int gain = (int)(PLAY_AFFECTION * multiplier);
            
            affection.CurrentAffection += gain;
            affection.PlayCount++;
            affection.TotalInteractionCount++;
            affection.LastInteractionTime = OS.GetUnixTime();
            
            int newLevel = affection.GetAffectionLevelNumber();
            
            AffectionChanged?.Call(petId, affection.CurrentAffection, newLevel);
            
            if (newLevel > oldLevel) {
                AffectionLevelUp?.Call(petId, newLevel);
            }
            
            return gain;
        }
        
        /// <summary>
        /// Battle participation increases affection
        /// </summary>
        public int OnBattleEnd(string petId, string rarity = "Common", bool won = true) {
            var affection = GetOrCreateAffection(petId, rarity);
            int oldLevel = affection.GetAffectionLevelNumber();
            
            float multiplier = GetRarityMultiplier(rarity);
            int baseGain = won ? BATTLE_WIN_AFFECTION : BATTLE_PARTICIPATION_AFFECTION;
            int gain = (int)(baseGain * multiplier);
            
            affection.CurrentAffection += gain;
            affection.BattleCount++;
            affection.TotalInteractionCount++;
            affection.LastInteractionTime = OS.GetUnixTime();
            
            int newLevel = affection.GetAffectionLevelNumber();
            
            AffectionChanged?.Call(petId, affection.CurrentAffection, newLevel);
            
            if (newLevel > oldLevel) {
                AffectionLevelUp?.Call(petId, newLevel);
            }
            
            return gain;
        }
        
        /// <summary>
        /// Daily interaction bonus
        /// </summary>
        public int GetDailyBonus(string petId) {
            var affection = GetOrCreateAffection(petId);
            long currentTime = OS.GetUnixTime();
            long timeSinceLastInteraction = currentTime - affection.LastInteractionTime;
            
            // More than 24 hours since last interaction
            if (timeSinceLastInteraction > 86400) {
                return DAILY_INTERACTION_BONUS;
            }
            return 0;
        }
        
        /// <summary>
        /// Get affection bonus for pet stats
        /// </summary>
        public float GetAffectionStatBonus(string petId) {
            if (!_petAffection.ContainsKey(petId)) return 0f;
            return _petAffection[petId].GetAffectionBonus();
        }
        
        /// <summary>
        /// Get current affection level
        /// </summary>
        public int GetAffectionLevel(string petId) {
            if (!_petAffection.ContainsKey(petId)) return 1;
            return _petAffection[petId].GetAffectionLevelNumber();
        }
        
        /// <summary>
        /// Get current affection value
        /// </summary>
        public int GetAffectionValue(string petId) {
            if (!_petAffection.ContainsKey(petId)) return 0;
            return _petAffection[petId].CurrentAffection;
        }
        
        /// <summary>
        /// Get affection title
        /// </summary>
        public string GetAffectionTitle(string petId) {
            if (!_petAffection.ContainsKey(petId)) return "Stranger";
            return _petAffection[petId].GetAffectionTitle();
        }
        
        /// <summary>
        /// Get all pet affection data
        /// </summary>
        public Dictionary<string, PetAffectionData> GetAllAffectionData() {
            return _petAffection;
        }
        
        /// <summary>
        /// Get total affection across all pets
        /// </summary>
        public int GetTotalAffection() {
            int total = 0;
            foreach (var kvp in _petAffection) {
                total += kvp.Value.CurrentAffection;
            }
            return total;
        }
        
        /// <summary>
        /// Get average affection level
        /// </summary>
        public float GetAverageAffectionLevel() {
            if (_petAffection.Count == 0) return 1f;
            float total = 0;
            foreach (var kvp in _petAffection) {
                total += kvp.Value.GetAffectionLevelNumber();
            }
            return total / _petAffection.Count;
        }
        
        private float GetRarityMultiplier(string rarity) {
            switch (rarity) {
                case "Legendary": return LEGENDARY_PET_MULTIPLIER;
                case "Epic": return EPIC_PET_MULTIPLIER;
                case "Rare": return RARE_PET_MULTIPLIER;
                default: return 1.0f;
            }
        }
        
        /// <summary>
        /// Save affection data
        /// </summary>
        public Dictionary<string, object> Save() {
            var data = new Dictionary<string, object>();
            var petData = new List<Dictionary<string, object>>();
            
            foreach (var kvp in _petAffection) {
                petData.Add(new Dictionary<string, object> {
                    { "petId", kvp.Value.PetId },
                    { "currentAffection", kvp.Value.CurrentAffection },
                    { "totalInteractionCount", kvp.Value.TotalInteractionCount },
                    { "feedCount", kvp.Value.FeedCount },
                    { "playCount", kvp.Value.PlayCount },
                    { "battleCount", kvp.Value.BattleCount },
                    { "lastInteractionTime", kvp.Value.LastInteractionTime }
                });
            }
            
            data["petAffection"] = petData;
            return data;
        }
        
        /// <summary>
        /// Load affection data
        /// </summary>
        public void Load(Dictionary<string, object> data) {
            if (data == null) return;
            
            if (data.ContainsKey("petAffection")) {
                var petDataList = (Godot.Collections.Array)data["petAffection"];
                foreach (Dictionary<string, object> petData in petDataList) {
                    var affection = new PetAffectionData {
                        PetId = (string)petData["petId"],
                        CurrentAffection = (int)petData["currentAffection"],
                        TotalInteractionCount = (int)petData["totalInteractionCount"],
                        FeedCount = (int)petData["feedCount"],
                        PlayCount = (int)petData["playCount"],
                        BattleCount = (int)petData["battleCount"],
                        LastInteractionTime = (int)petData["lastInteractionTime"]
                    };
                    _petAffection[affection.PetId] = affection;
                }
            }
            
            GD.Print($"[PetAffectionSystem] Loaded {_petAffection.Count} pet affection records");
        }
    }
}
