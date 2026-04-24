using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Core system for managing pet traits.
    /// </summary>
    public partial class PetTraitSystem : BaseSystem
    {
        public static PetTraitSystem Instance { get; private set; }
        
        private PetTraitData _data = new PetTraitData();
        private Random _random = new Random();
        
        // Statistics
        private int _totalTraitsUnlocked = 0;
        private int _totalTraitsActivated = 0;
        
        public override void _Ready()
        {
            Instance = this;
            PetTraitDatabase.Initialize();
            LoadData();
        }
        
        #region Data Management
        
        private void LoadData()
        {
            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
            if (saveSystem == null) return;

            var data = saveSystem.LoadGame();
            if (data == null) return;

            // Load unlocked traits
            if (data.ContainsKey("pet_trait_unlocked"))
            {
                var unlockedArray = (Godot.Array)data["pet_trait_unlocked"];
                foreach (string traitId in unlockedArray)
                {
                    _data.UnlockedTraits[traitId] = true;
                }
            }

            // Load trait levels
            if (data.ContainsKey("pet_trait_levels"))
            {
                var levelsData = (Godot.Collections.Dictionary)data["pet_trait_levels"];
                foreach (string key in levelsData.Keys)
                {
                    _data.TraitLevels[key] = (int)levelsData[key];
                }
            }

            // Load active traits
            if (data.ContainsKey("pet_trait_active"))
            {
                var activeArray = (Godot.Array)data["pet_trait_active"];
                foreach (string traitId in activeArray)
                {
                    if (!_data.ActiveTraits.Contains(traitId))
                        _data.ActiveTraits.Add(traitId);
                }
            }

            // Load stats
            if (data.ContainsKey("pet_trait_stats"))
            {
                var stats = (Godot.Collections.Dictionary)data["pet_trait_stats"];
                _totalTraitsUnlocked = (int)stats.Get("unlocked", 0);
                _totalTraitsActivated = (int)stats.Get("activated", 0);
            }
        }
        
        private void SaveData()
        {
            var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
            if (saveSystem == null) return;

            var data = saveSystem.LoadGame();
            if (data == null) data = new Godot.Collections.Dictionary();

            // Save unlocked traits
            var unlockedArray = new Godot.Array();
            foreach (var key in _data.UnlockedTraits.Keys)
            {
                unlockedArray.Add(key);
            }
            data["pet_trait_unlocked"] = unlockedArray;

            // Save trait levels
            var levelsData = new Godot.Collections.Dictionary();
            foreach (var kvp in _data.TraitLevels)
            {
                levelsData[kvp.Key] = kvp.Value;
            }
            data["pet_trait_levels"] = levelsData;

            // Save active traits
            var activeArray = new Godot.Array();
            foreach (var traitId in _data.ActiveTraits)
            {
                activeArray.Add(traitId);
            }
            data["pet_trait_active"] = activeArray;

            // Save stats
            var stats = new Godot.Collections.Dictionary();
            stats["unlocked"] = _totalTraitsUnlocked;
            stats["activated"] = _totalTraitsActivated;
            data["pet_trait_stats"] = stats;

            saveSystem.SaveGame(data);
        }
        
        public void ResetData()
        {
            _data = new PetTraitData();
            _totalTraitsUnlocked = 0;
            _totalTraitsActivated = 0;
            SaveData();
        }
        
        #endregion
        
        #region Trait Unlocking
        
        public bool UnlockTrait(string traitId)
        {
            var trait = PetTraitDatabase.GetTrait(traitId);
            if (trait == null)
            {
                GD.PrintErr($"PetTraitSystem: Trait {traitId} not found");
                return false;
            }
            
            if (_data.UnlockedTraits.ContainsKey(traitId) && _data.UnlockedTraits[traitId])
            {
                GD.Print($"PetTraitSystem: Trait {traitId} already unlocked");
                return false;
            }
            
            _data.UnlockedTraits[traitId] = true;
            _data.TraitLevels[traitId] = 1;
            _totalTraitsUnlocked++;
            
            GD.Print($"PetTraitSystem: Unlocked trait {trait.Name}");
            SaveData();
            return true;
        }
        
        public bool CanUnlockTrait(string traitId, int petLevel, string petType = "")
        {
            var trait = PetTraitDatabase.GetTrait(traitId);
            if (trait == null) return false;
            
            // Check if already unlocked
            if (_data.UnlockedTraits.ContainsKey(traitId) && _data.UnlockedTraits[traitId])
                return false;
            
            // Check level requirement
            if (petLevel < trait.MinPetLevel)
                return false;
            
            // Check pet type requirement
            if (!string.IsNullOrEmpty(trait.RequiredPetType) && trait.RequiredPetType != petType)
                return false;
            
            return true;
        }
        
        public List<PetTrait> GetUnlockableTraits(int petLevel, string petType = "")
        {
            List<PetTrait> result = new List<PetTrait>();
            var allTraits = PetTraitDatabase.GetUnlockableTraits(petLevel, petType);
            
            foreach (var trait in allTraits)
            {
                if (CanUnlockTrait(trait.Id, petLevel, petType))
                    result.Add(trait);
            }
            
            return result;
        }
        
        #endregion
        
        #region Trait Activation
        
        public bool ActivateTrait(string traitId)
        {
            var trait = PetTraitDatabase.GetTrait(traitId);
            if (trait == null)
            {
                GD.PrintErr($"PetTraitSystem: Trait {traitId} not found");
                return false;
            }
            
            if (!_data.UnlockedTraits.ContainsKey(traitId) || !_data.UnlockedTraits[traitId])
            {
                GD.Print($"PetTraitSystem: Cannot activate locked trait {traitId}");
                return false;
            }
            
            if (_data.ActiveTraits.Contains(traitId))
            {
                GD.Print($"PetTraitSystem: Trait {traitId} already active");
                return false;
            }
            
            _data.ActiveTraits.Add(traitId);
            _totalTraitsActivated++;
            
            GD.Print($"PetTraitSystem: Activated trait {trait.Name}");
            SaveData();
            return true;
        }
        
        public bool DeactivateTrait(string traitId)
        {
            if (!_data.ActiveTraits.Contains(traitId))
                return false;
            
            _data.ActiveTraits.Remove(traitId);
            _totalTraitsActivated--;
            
            GD.Print($"PetTraitSystem: Deactivated trait {traitId}");
            SaveData();
            return true;
        }
        
        public void ToggleTrait(string traitId)
        {
            if (_data.ActiveTraits.Contains(traitId))
                DeactivateTrait(traitId);
            else
                ActivateTrait(traitId);
        }
        
        #endregion
        
        #region Bonus Calculation
        
        public float GetAttackBonus()
        {
            float bonus = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    bonus += trait.AttackBonus;
            }
            return bonus;
        }
        
        public float GetDefenseBonus()
        {
            float bonus = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    bonus += trait.DefenseBonus;
            }
            return bonus;
        }
        
        public float GetHealthBonus()
        {
            float bonus = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    bonus += trait.HealthBonus;
            }
            return bonus;
        }
        
        public float GetSpeedBonus()
        {
            float bonus = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    bonus += trait.SpeedBonus;
            }
            return bonus;
        }
        
        public float GetCriticalBonus()
        {
            float bonus = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    bonus += trait.CriticalBonus;
            }
            return bonus;
        }
        
        public float GetEvasionBonus()
        {
            float bonus = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    bonus += trait.EvasionBonus;
            }
            return bonus;
        }
        
        public float GetExpBonus()
        {
            float bonus = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    bonus += trait.ExpBonus;
            }
            return bonus;
        }
        
        public float GetGoldBonus()
        {
            float bonus = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    bonus += trait.GoldBonus;
            }
            return bonus;
        }
        
        public float GetDropRateBonus()
        {
            float bonus = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    bonus += trait.DropRateBonus;
            }
            return bonus;
        }
        
        #endregion
        
        #region Special Effects
        
        public bool ShouldDoubleGold()
        {
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null && trait.DoubleGoldChance)
                {
                    if (_random.NextDouble() < 0.10) // 10% chance
                        return true;
                }
            }
            return false;
        }
        
        public bool ShouldDoubleExp()
        {
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null && trait.DoubleExpChance)
                {
                    if (_random.NextDouble() < 0.10) // 10% chance
                        return true;
                }
            }
            return false;
        }
        
        public bool ShouldRareDrop()
        {
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null && trait.RareDropChance)
                {
                    if (_random.NextDouble() < 0.05) // 5% chance
                        return true;
                }
            }
            return false;
        }
        
        public bool ShouldSteal()
        {
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null && trait.StealChance)
                {
                    if (_random.NextDouble() < 0.05) // 5% chance
                        return true;
                }
            }
            return false;
        }
        
        public float GetLifestealPercent()
        {
            float percent = 0f;
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null && trait.RegenHealth)
                    percent += 0.05f; // 5% lifesteal
            }
            return percent;
        }
        
        #endregion
        
        #region Queries
        
        public bool IsTraitUnlocked(string traitId)
        {
            return _data.UnlockedTraits.ContainsKey(traitId) && _data.UnlockedTraits[traitId];
        }
        
        public bool IsTraitActive(string traitId)
        {
            return _data.ActiveTraits.Contains(traitId);
        }
        
        public List<PetTrait> GetUnlockedTraits()
        {
            List<PetTrait> result = new List<PetTrait>();
            foreach (var kvp in _data.UnlockedTraits)
            {
                if (kvp.Value)
                {
                    var trait = PetTraitDatabase.GetTrait(kvp.Key);
                    if (trait != null)
                        result.Add(trait);
                }
            }
            return result;
        }
        
        public List<PetTrait> GetActiveTraits()
        {
            List<PetTrait> result = new List<PetTrait>();
            foreach (var traitId in _data.ActiveTraits)
            {
                var trait = PetTraitDatabase.GetTrait(traitId);
                if (trait != null)
                    result.Add(trait);
            }
            return result;
        }
        
        public int GetUnlockedCount() => _totalTraitsUnlocked;
        public int GetActiveCount() => _totalTraitsActivated;
        public int GetTotalTraitCount() => PetTraitDatabase.GetTotalTraitCount();
        
        public Dictionary<string, bool> GetUnlockedTraitsDict() => _data.UnlockedTraits;
        public List<string> GetActiveTraitsList() => _data.ActiveTraits;
        
        #endregion
        
        #region Random Unlock
        
        public PetTrait RandomUnlock(int petLevel, string petType = "")
        {
            var unlockable = GetUnlockableTraits(petLevel, petType);
            if (unlockable.Count == 0)
                return null;
            
            // Weight by rarity
            var weighted = new List<PetTrait>();
            foreach (var trait in unlockable)
            {
                int weight = GetRarityWeight(trait.Rarity);
                for (int i = 0; i < weight; i++)
                    weighted.Add(trait);
            }
            
            var selected = weighted[_random.Next(weighted.Count)];
            UnlockTrait(selected.Id);
            return selected;
        }
        
        private int GetRarityWeight(TraitRarity rarity)
        {
            switch (rarity)
            {
                case TraitRarity.Common: return 10;
                case TraitRarity.Uncommon: return 7;
                case TraitRarity.Rare: return 4;
                case TraitRarity.Epic: return 2;
                case TraitRarity.Legendary: return 1;
                default: return 1;
            }
        }

        #endregion

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Godot.Collections.Dictionary
            {
                ["unlockedTraits"] = _data.UnlockedTraits,
                ["traitLevels"] = _data.TraitLevels,
                ["activeTraits"] = _data.ActiveTraits,
                ["totalTraitsUnlocked"] = _totalTraitsUnlocked,
                ["totalTraitsActivated"] = _totalTraitsActivated
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.TryGetValue("unlockedTraits", out var unlockedObj) && unlockedObj is Godot.Collections.Dictionary unlocked)
            {
                _data.UnlockedTraits.Clear();
                foreach (string traitId in unlocked.Keys)
                {
                    _data.UnlockedTraits[traitId] = (bool)unlocked[traitId];
                }
            }

            if (data.TryGetValue("traitLevels", out var levelsObj) && levelsObj is Godot.Collections.Dictionary levels)
            {
                _data.TraitLevels.Clear();
                foreach (string key in levels.Keys)
                {
                    _data.TraitLevels[key] = Convert.ToInt32(levels[key]);
                }
            }

            if (data.TryGetValue("activeTraits", out var activeObj) && activeObj is Godot.Collections.Array activeArray)
            {
                _data.ActiveTraits.Clear();
                foreach (string traitId in activeArray)
                {
                    if (!_data.ActiveTraits.Contains(traitId))
                        _data.ActiveTraits.Add(traitId);
                }
            }

            if (data.TryGetValue("totalTraitsUnlocked", out var unlockedCountObj))
            {
                _totalTraitsUnlocked = Convert.ToInt32(unlockedCountObj);
            }

            if (data.TryGetValue("totalTraitsActivated", out var activatedCountObj))
            {
                _totalTraitsActivated = Convert.ToInt32(activatedCountObj);
            }

            SaveData();
        }
    }

}
