using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Systems
{
    /// <summary>
    /// Core rune system - manages rune collection, equipment, and synthesis
    /// </summary>
    public class RuneSystem
    {
        private PlayerRuneData _playerRuneData;
        
        // Rune slot configurations for equipment
        private readonly Dictionary<RuneSlotType, List<RuneSlot>> _equipmentSlots = new Dictionary<RuneSlotType, List<RuneSlot>>
        {
            { RuneSlotType.Weapon, new List<RuneSlot> { new RuneSlot { SlotType = RuneSlotType.Weapon, SlotIndex = 0, IsUnlocked = true } } },
            { RuneSlotType.Armor, new List<RuneSlot> { new RuneSlot { SlotType = RuneSlotType.Armor, SlotIndex = 0, IsUnlocked = true } } },
            { RuneSlotType.Helmet, new List<RuneSlot> { new RuneSlot { SlotType = RuneSlotType.Helmet, SlotIndex = 0, IsUnlocked = true } } },
            { RuneSlotType.Boots, new List<RuneSlot> { new RuneSlot { SlotType = RuneSlotType.Boots, SlotIndex = 0, IsUnlocked = true } } },
            { RuneSlotType.Gloves, new List<RuneSlot> { new RuneSlot { SlotType = RuneSlotType.Gloves, SlotIndex = 0, IsUnlocked = true } } },
            { RuneSlotType.Accessory, new List<RuneSlot> { new RuneSlot { SlotType = RuneSlotType.Accessory, SlotIndex = 0, IsUnlocked = true } } }
        };
        
        public event Action<string> OnRuneEquipped;
        public event Action<string> OnRuneUnequipped;
        public event Action<string, bool> OnRuneCrafted;
        
        public RuneSystem()
        {
            _playerRuneData = new PlayerRuneData();
        }
        
        public void Initialize(PlayerRuneData saveData)
        {
            if (saveData != null)
            {
                _playerRuneData = saveData;
            }
        }
        
        // ========== Collection Management ==========
        
        public void AddRune(string runeId, int count = 1)
        {
            var rune = RuneDatabase.GetRune(runeId);
            if (rune == null) return;
            
            var existing = _playerRuneData.OwnedRunes.FirstOrDefault(r => r.Id == runeId);
            if (existing != null)
            {
                _playerRuneData.RuneCount[runeId] = _playerRuneData.RuneCount.GetValueOrDefault(runeId, 0) + count;
            }
            else
            {
                _playerRuneData.OwnedRunes.Add(new Rune
                {
                    Id = rune.Id,
                    Name = rune.Name,
                    Description = rune.Description,
                    Type = rune.Type,
                    Rarity = rune.Rarity,
                    DamageBonus = rune.DamageBonus,
                    DefenseBonus = rune.DefenseBonus,
                    HealthBonus = rune.HealthBonus,
                    ManaBonus = rune.ManaBonus,
                    SpeedBonus = rune.SpeedBonus,
                    CritChance = rune.CritChance,
                    CritDamage = rune.CritDamage,
                    LifeSteal = rune.LifeSteal,
                    Regen = rune.Regen,
                    OnHitEffect = rune.OnHitEffect,
                    OnKillEffect = rune.OnKillEffect,
                    OnDamagedEffect = rune.OnDamagedEffect,
                    OnCriticalEffect = rune.OnCriticalEffect,
                    LevelRequired = rune.LevelRequired,
                    Power = rune.Power
                });
                _playerRuneData.RuneCount[runeId] = count;
                _playerRuneData.TotalRunesDiscovered++;
            }
        }
        
        public bool RemoveRune(string runeId, int count = 1)
        {
            if (!_playerRuneData.RuneCount.ContainsKey(runeId) || _playerRuneData.RuneCount[runeId] < count)
                return false;
            
            _playerRuneData.RuneCount[runeId] -= count;
            if (_playerRuneData.RuneCount[runeId] <= 0)
            {
                _playerRuneData.OwnedRunes.RemoveAll(r => r.Id == runeId);
                _playerRuneData.RuneCount.Remove(runeId);
            }
            return true;
        }
        
        public int GetRuneCount(string runeId)
        {
            return _playerRuneData.RuneCount.GetValueOrDefault(runeId, 0);
        }
        
        public List<Rune> GetOwnedRunes()
        {
            return _playerRuneData.OwnedRunes.ToList();
        }
        
        public List<Rune> GetEquippedRunes()
        {
            return _playerRuneData.EquippedRunes.ToList();
        }
        
        // ========== Equipment Management ==========
        
        public bool EquipRune(string runeId, RuneSlotType slotType)
        {
            var rune = _playerRuneData.OwnedRunes.FirstOrDefault(r => r.Id == runeId);
            if (rune == null) return false;
            
            // Check if slot exists
            if (!_equipmentSlots.ContainsKey(slotType)) return false;
            
            // Check if rune already equipped
            var alreadyEquipped = _playerRuneData.EquippedRunes.FirstOrDefault(r => r.Id == runeId);
            if (alreadyEquipped != null)
            {
                _playerRuneData.EquippedRunes.Remove(alreadyEquipped);
            }
            
            // Unequip current rune in slot
            var currentInSlot = _playerRuneData.EquippedRunes.FirstOrDefault(r => 
                _equipmentSlots[slotType].Any(s => s.Rune?.Id == r.Id));
            if (currentInSlot != null)
            {
                _playerRuneData.EquippedRunes.Remove(currentInSlot);
            }
            
            _playerRuneData.EquippedRunes.Add(rune);
            OnRuneEquipped?.Invoke(runeId);
            return true;
        }
        
        public bool UnequipRune(string runeId)
        {
            var rune = _playerRuneData.EquippedRunes.FirstOrDefault(r => r.Id == runeId);
            if (rune == null) return false;
            
            _playerRuneData.EquippedRunes.Remove(rune);
            OnRuneUnequipped?.Invoke(runeId);
            return true;
        }
        
        // ========== Synthesis ==========
        
        public (bool success, Rune result) CraftRune(string resultRuneId)
        {
            // Simplified synthesis - craft higher rarity from 3 lower rarity runes
            var resultRune = RuneDatabase.GetRune(resultRuneId);
            if (resultRune == null) return (false, null);
            
            int rarityLevel = (int)resultRune.Rarity;
            if (rarityLevel <= 0) return (false, null);
            
            // Find ingredients (same type, lower rarity)
            var ingredientRarity = (RuneRarity)(rarityLevel - 1);
            var ingredients = _playerRuneData.OwnedRunes
                .Where(r => r.Type == resultRune.Type && r.Rarity == ingredientRarity)
                .Take(3)
                .ToList();
            
            if (ingredients.Count < 3) return (false, null);
            
            // Check if player has enough
            foreach (var ing in ingredients)
            {
                if (GetRuneCount(ing.Id) < 1) return (false, null);
            }
            
            // Remove ingredients
            foreach (var ing in ingredients)
            {
                RemoveRune(ing.Id, 1);
            }
            
            // Determine success (higher rarity = lower chance)
            int baseSuccess = 80 - (rarityLevel * 10);
            bool success = UnityEngine.Random.Range(0, 100) < baseSuccess;
            
            if (success)
            {
                AddRune(resultRuneId, 1);
                _playerRuneData.TotalRunesCrafted++;
                OnRuneCrafted?.Invoke(resultRuneId, true);
                return (true, resultRune);
            }
            
            OnRuneCrafted?.Invoke(resultRuneId, false);
            return (false, null);
        }
        
        // ========== Stat Calculation ==========
        
        public float GetTotalDamageBonus()
        {
            return _playerRuneData.EquippedRunes.Sum(r => r.DamageBonus);
        }
        
        public float GetTotalDefenseBonus()
        {
            return _playerRuneData.EquippedRunes.Sum(r => r.DefenseBonus);
        }
        
        public float GetTotalHealthBonus()
        {
            return _playerRuneData.EquippedRunes.Sum(r => r.HealthBonus);
        }
        
        public float GetTotalManaBonus()
        {
            return _playerRuneData.EquippedRunes.Sum(r => r.ManaBonus);
        }
        
        public float GetTotalSpeedBonus()
        {
            return _playerRuneData.EquippedRunes.Sum(r => r.SpeedBonus);
        }
        
        public float GetTotalCritChance()
        {
            return _playerRuneData.EquippedRunes.Sum(r => r.CritChance);
        }
        
        public float GetTotalCritDamage()
        {
            return _playerRuneData.EquippedRunes.Sum(r => r.CritDamage);
        }
        
        public float GetTotalLifeSteal()
        {
            return _playerRuneData.EquippedRunes.Sum(r => r.LifeSteal);
        }
        
        public float GetTotalRegen()
        {
            return _playerRuneData.EquippedRunes.Sum(r => r.Regen);
        }
        
        // ========== Effect Triggers ==========
        
        public void OnAttackHit()
        {
            foreach (var rune in _playerRuneData.EquippedRunes.Where(r => r.OnHitEffect))
            {
                // Trigger on-hit effects
            }
        }
        
        public void OnEnemyKilled()
        {
            foreach (var rune in _playerRuneData.EquippedRunes.Where(r => r.OnKillEffect))
            {
                // Trigger on-kill effects
            }
        }
        
        public void OnPlayerDamaged()
        {
            foreach (var rune in _playerRuneData.EquippedRunes.Where(r => r.OnDamagedEffect))
            {
                // Trigger on-damaged effects
            }
        }
        
        public void OnCriticalHit()
        {
            foreach (var rune in _playerRuneData.EquippedRunes.Where(r => r.OnCriticalEffect))
            {
                // Trigger on-critical effects
            }
        }
        
        // ========== Statistics ==========
        
        public RuneStatistics GetStatistics()
        {
            var stats = new RuneStatistics
            {
                TotalRunesOwned = _playerRuneData.OwnedRunes.Sum(r => _playerRuneData.RuneCount.GetValueOrDefault(r.Id, 0)),
                UniqueRunes = _playerRuneData.OwnedRunes.Count,
                TotalCrafted = _playerRuneData.TotalRunesCrafted
            };
            
            foreach (RuneRarity rarity in Enum.GetValues(typeof(RuneRarity)))
            {
                stats.RarityBreakdown[rarity] = _playerRuneData.OwnedRunes.Count(r => r.Rarity == rarity);
            }
            
            foreach (RuneType type in Enum.GetValues(typeof(RuneType)))
            {
                stats.TypeBreakdown[type] = _playerRuneData.OwnedRunes.Count(r => r.Type == type);
            }
            
            return stats;
        }
        
        // ========== Save/Load ==========
        
        public PlayerRuneData ExportSaveData()
        {
            return _playerRuneData;
        }
        
        public void ImportSaveData(PlayerRuneData data)
        {
            if (data != null)
            {
                _playerRuneData = data;
            }
        }
    }
}
