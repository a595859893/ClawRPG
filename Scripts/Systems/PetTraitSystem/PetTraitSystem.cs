using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Core system for managing pet traits.
    /// </summary>
    public class PetTraitSystem : Node
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
            // TODO: Implement save to file
        }
        
        private void SaveData()
        {
            // TODO: Implement load from file
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
    }
}
