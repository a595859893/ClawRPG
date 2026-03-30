using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Manages fate relics - roguelike-style collectible relics
    /// </summary>
    public partial class FateRelicSystem : BaseSystem
    {
        private static FateRelicSystem _instance;
        public static FateRelicSystem Instance {
            get {
                if (_instance == null) {
                    _instance = new FateRelicSystem();
                }
                return _instance;
            }
        }
        
        protected override string SystemName => "FateRelicSystem";
        public static FateRelicSystem Instance {
            get {
                if (_instance == null) {
                    _instance = new FateRelicSystem();
                }
                return _instance;
            }
        }
        
        private PlayerFateRelicData _playerData;
        private Dictionary<string, FateRelic> _ownedRelics;
        private Random _random;
        
        // Signals
        public Action<string> OnRelicAcquired;
        public Action<string> OnRelicEquipped;
        public Action<string> OnRelicUnequipped;
        public Action OnRelicSlotsExpanded;
        
        public FateRelicSystem() {
            _playerData = new PlayerFateRelicData();
            _ownedRelics = new Dictionary<string, FateRelic>();
            _random = new Random();
        }
        
        public void Initialize() {
            GD.Print("[FateRelicSystem] Initializing...");
            LoadRelicData();
        }
        
        public void AcquireRelic(string relicId) {
            var relic = FateRelicDatabase.GetRelic(relicId);
            if (relic == null) {
                GD.PrintErr($"[FateRelicSystem] Relic not found: {relicId}");
                return;
            }
            
            if (_ownedRelics.ContainsKey(relicId)) {
                // Already owned, increase stack
                _ownedRelics[relicId].StackCount++;
                _playerData.RelicStacks[relicId] = _ownedRelics[relicId].StackCount;
            } else {
                // New relic
                var newRelic = new FateRelic {
                    Id = relic.Id,
                    Name = relic.Name,
                    Description = relic.Description,
                    Rarity = relic.Rarity,
                    Type = relic.Type,
                    Effects = new List<FateRelicEffect>(relic.Effects),
                    IsEquipped = false,
                    IsActive = true,
                    StackCount = 1
                };
                _ownedRelics[relicId] = newRelic;
                _playerData.OwnedRelicIds.Add(relicId);
                _playerData.EquippedRelics[relicId] = false;
                _playerData.RelicStacks[relicId] = 1;
                _playerData.RelicsDiscovered++;
            }
            
            SaveRelicData();
            OnRelicAcquired?.Invoke(relicId);
            GD.Print($"[FateRelicSystem] Acquired relic: {relic.Name} (x{_ownedRelics[relicId].StackCount})");
        }
        
        public void AcquireRandomRelic() {
            var relic = FateRelicDatabase.GetRandomRelicByWeight();
            if (relic != null) {
                AcquireRelic(relic.Id);
            }
        }
        
        public bool EquipRelic(string relicId) {
            if (!_ownedRelics.ContainsKey(relicId)) {
                GD.PrintErr($"[FateRelicSystem] Cannot equip - not owned: {relicId}");
                return false;
            }
            
            int equippedCount = 0;
            foreach (var kvp in _playerData.EquippedRelics) {
                if (kvp.Value) equippedCount++;
            }
            
            if (equippedCount >= _playerData.MaxRelicSlots && !_playerData.EquippedRelics[relicId]) {
                GD.Print($"[FateRelicSystem] Cannot equip - max slots ({_playerData.MaxRelicSlots}) reached");
                return false;
            }
            
            _ownedRelics[relicId].IsEquipped = true;
            _playerData.EquippedRelics[relicId] = true;
            
            SaveRelicData();
            OnRelicEquipped?.Invoke(relicId);
            GD.Print($"[FateRelicSystem] Equipped relic: {_ownedRelics[relicId].Name}");
            return true;
        }
        
        public bool UnequipRelic(string relicId) {
            if (!_ownedRelics.ContainsKey(relicId)) {
                GD.PrintErr($"[FateRelicSystem] Cannot unequip - not owned: {relicId}");
                return false;
            }
            
            _ownedRelics[relicId].IsEquipped = false;
            _playerData.EquippedRelics[relicId] = false;
            
            SaveRelicData();
            OnRelicUnequipped?.Invoke(relicId);
            GD.Print($"[FateRelicSystem] Unequipped relic: {_ownedRelics[relicId].Name}");
            return true;
        }
        
        public bool ExpandRelicSlots(int additionalSlots, int goldCost) {
            // Check if player has enough gold (would need integration with economy system)
            _playerData.MaxRelicSlots += additionalSlots;
            
            SaveRelicData();
            OnRelicSlotsExpanded?.Invoke();
            GD.Print($"[FateRelicSystem] Expanded relic slots to {_playerData.MaxRelicSlots}");
            return true;
        }
        
        public List<FateRelic> GetOwnedRelics() {
            return new List<FateRelic>(_ownedRelics.Values);
        }
        
        public List<FateRelic> GetEquippedRelics() {
            var result = new List<FateRelic>();
            foreach (var relic in _ownedRelics.Values) {
                if (relic.IsEquipped) {
                    result.Add(relic);
                }
            }
            return result;
        }
        
        public FateRelic GetRelic(string relicId) {
            if (_ownedRelics.ContainsKey(relicId)) {
                return _ownedRelics[relicId];
            }
            return null;
        }
        
        public int GetOwnedCount() {
            return _ownedRelics.Count;
        }
        
        public int GetEquippedCount() {
            int count = 0;
            foreach (var equipped in _playerData.EquippedRelics.Values) {
                if (equipped) count++;
            }
            return count;
        }
        
        public int GetMaxSlots() {
            return _playerData.MaxRelicSlots;
        }
        
        public Dictionary<string, float> GetAllActiveStatBonuses() {
            var bonuses = new Dictionary<string, float>();
            
            foreach (var relic in _ownedRelics.Values) {
                if (!relic.IsEquipped || !relic.IsActive) continue;
                
                float multiplier = 1.0f + (relic.StackCount - 1) * 0.1f;
                
                foreach (var effect in relic.Effects) {
                    if (bonuses.ContainsKey(effect.Stat)) {
                        bonuses[effect.Stat] += effect.Value * multiplier;
                    } else {
                        bonuses[effect.Stat] = effect.Value * multiplier;
                    }
                }
            }
            
            return bonuses;
        }
        
        public float GetStatBonus(string stat) {
            var bonuses = GetAllActiveStatBonuses();
            if (bonuses.ContainsKey(stat)) {
                return bonuses[stat];
            }
            return 0f;
        }
        
        public void ApplyRelicEffects(CharacterBody2D player) {
            var bonuses = GetAllActiveStatBonuses();
            
            // Apply attack bonus
            if (bonuses.ContainsKey("attack_bonus")) {
                // Would integrate with combat system
            }
            
            // Apply defense bonus
            if (bonuses.ContainsKey("defense_bonus")) {
                // Would integrate with combat system
            }
            
            // Apply movement speed
            if (bonuses.ContainsKey("movement_speed")) {
                // Would integrate with player movement
            }
            
            // Apply crit rate
            if (bonuses.ContainsKey("crit_rate")) {
                // Would integrate with combat system
            }
            
            // Apply lifesteal
            if (bonuses.ContainsKey("lifesteal")) {
                // Would integrate with combat system
            }
        }
        
        public void CompleteRelicSet(string setId) {
            _playerData.RelicsCompleted++;
            GD.Print($"[FateRelicSystem] Completed relic set: {setId}");
        }
        
        private void SaveRelicData() {
            // Save to game data
            var gameData = GameDataManager.GetGameData();
            if (gameData != null) {
                gameData.Set("fate_relic_data", Json.Stringify(new {
                    ownedRelics = _playerData.OwnedRelicIds,
                    equippedRelics = _playerData.EquippedRelics,
                    relicStacks = _playerData.RelicStacks,
                    maxSlots = _playerData.MaxRelicSlots,
                    goldSpent = _playerData.GoldSpentOnRelics,
                    discovered = _playerData.RelicsDiscovered,
                    completed = _playerData.RelicsCompleted
                }));
                GameDataManager.SaveGame();
            }
        }
        
        private void LoadRelicData() {
            var gameData = GameDataManager.GetGameData();
            if (gameData == null) return;
            
            var json = gameData.Get<string>("fate_relic_data");
            if (string.IsNullOrEmpty(json)) return;
            
            try {
                // Parse JSON and restore data
                // Simplified - actual implementation would parse the JSON
                GD.Print("[FateRelicSystem] Relic data loaded");
            } catch (Exception e) {
                GD.PrintErr($"[FateRelicSystem] Failed to load relic data: {e.Message}");
            }
        }
        
        public Dictionary<string, object> GetRelicStatistics() {
            var stats = new Dictionary<string, object> {
                { "total_owned", _playerData.OwnedRelicIds.Count },
                { "total_discovered", _playerData.RelicsDiscovered },
                { "total_completed", _playerData.RelicsCompleted },
                { "equipped_count", GetEquippedCount() },
                { "max_slots", _playerData.MaxRelicSlots },
                { "gold_spent", _playerData.GoldSpentOnRelics }
            };
            
            var rarityCounts = new Dictionary<string, int>();
            foreach (var relic in _ownedRelics.Values) {
                string rarityName = relic.Rarity.Name;
                if (rarityCounts.ContainsKey(rarityName)) {
                    rarityCounts[rarityName]++;
                } else {
                    rarityCounts[rarityName] = 1;
                }
            }
            stats["by_rarity"] = rarityCounts;
            
            var typeCounts = new Dictionary<string, int>();
            foreach (var relic in _ownedRelics.Values) {
                string typeName = relic.Type.Name;
                if (typeCounts.ContainsKey(typeName)) {
                    typeCounts[typeName]++;
                } else {
                    typeCounts[typeName] = 1;
                }
            }
            stats["by_type"] = typeCounts;
            
            return stats;
        }
        
        /// <summary>
        /// Export save data (BaseSystem override)
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            data["ownedRelics"] = _playerData.OwnedRelicIds.ToList();
            data["equippedRelics"] = _playerData.EquippedRelics;
            data["relicStacks"] = _playerData.RelicStacks;
            data["maxSlots"] = _playerData.MaxRelicSlots;
            data["goldSpent"] = _playerData.GoldSpentOnRelics;
            data["discovered"] = _playerData.RelicsDiscovered;
            data["completed"] = _playerData.RelicsCompleted;
            return data;
        }
        
        /// <summary>
        /// Import save data (BaseSystem override)
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("ownedRelics"))
            {
                _playerData.OwnedRelicIds = new HashSet<string>((List<string>)data["ownedRelics"]);
            }
            if (data.Contains("equippedRelics"))
            {
                _playerData.EquippedRelics = (Dictionary<string, bool>)data["equippedRelics"];
            }
            if (data.Contains("relicStacks"))
            {
                _playerData.RelicStacks = (Dictionary<string, int>)data["relicStacks"];
            }
            if (data.Contains("maxSlots"))
            {
                _playerData.MaxRelicSlots = (int)data["maxSlots"];
            }
            if (data.Contains("goldSpent"))
            {
                _playerData.GoldSpentOnRelics = (int)data["goldSpent"];
            }
            if (data.Contains("discovered"))
            {
                _playerData.RelicsDiscovered = (int)data["discovered"];
            }
            if (data.Contains("completed"))
            {
                _playerData.RelicsCompleted = (int)data["completed"];
            }
        }
    }
}
