using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using Game.Systems.Pets;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

public partial class PetEquipmentEnhancementSystem : BaseSystem
{
    public static PetEquipmentEnhancementSystem Instance { get; private set; }

    // Signals
public delegate void EnhancementStarted(string equipmentId, int newTier);
public delegate void EnhancementSucceeded(string equipmentId, int newTier, bool isCritical);
public delegate void EnhancementFailed(string equipmentId, int currentTier);
public delegate void EnhancementDataLoaded();

    // Player enhancement data
    private PetEquipmentEnhancementData.PlayerEnhancementData _playerData = new PetEquipmentEnhancementData.PlayerEnhancementData();

    // Reference to pet equipment system
    private PetEquipmentSystem _petEquipmentSystem;

    public override void _Ready()
    {
        Instance = this;
        _petEquipmentSystem = GetNode<PetEquipmentSystem>("/root/PetEquipmentSystem");
    }

    // Get player data
    public PetEquipmentEnhancementData.PlayerEnhancementData GetPlayerData()
    {
        return _playerData;
    }

    // Get enhancement data for specific equipment
    public PetEquipmentEnhancementData.EquipmentEnhancement GetEquipmentEnhancement(string equipmentId)
    {
        var existing = _playerData.EquipmentList.FirstOrDefault(e => e.EquipmentId == equipmentId);
        if (existing == null)
        {
            existing = new PetEquipmentEnhancementData.EquipmentEnhancement
            {
                EquipmentId = equipmentId,
                CurrentTier = 0,
                Tier = PetEquipmentEnhancementData.EnhancementTier.None,
                SuccessCount = 0,
                FailureCount = 0,
                LastEnhanceTime = DateTime.MinValue
            };
            _playerData.EquipmentList.Add(existing);
        }
        return existing;
    }

    // Check if enhancement is possible
    public bool CanEnhance(string equipmentId, PetEquipmentEnhancementData.EnhancementTier targetTier)
    {
        var equipment = GetEquipmentEnhancement(equipmentId);
        
        // Check tier validity
        if (targetTier <= equipment.Tier)
        {
            GD.Print($"[PetEquipmentEnhancement] Target tier must be higher than current tier");
            return false;
        }

        if (targetTier > PetEquipmentEnhancementData.EnhancementTier.Mythic)
        {
            GD.Print($"[PetEquipmentEnhancement] Cannot enhance beyond Mythic tier");
            return false;
        }

        // Check gold cost
        int cost = PetEquipmentEnhancementDatabase.GetEnhancementCost(targetTier);
        if (Player.Instance.Gold < cost)
        {
            GD.Print($"[PetEquipmentEnhancement] Not enough gold. Need {cost}, have {Player.Instance.Gold}");
            return false;
        }

        // Check materials
        var equipmentType = GetEquipmentType(equipmentId);
        var materials = PetEquipmentEnhancementDatabase.GetMaterialsForEnhancement(equipmentType, targetTier);
        
        foreach (var mat in materials)
        {
            int playerCount = InventoryManager.Instance.GetItemCount(mat.Id);
            if (playerCount < mat.Quantity)
            {
                GD.Print($"[PetEquipmentEnhancement] Not enough materials. Need {mat.Quantity} {mat.Name}, have {playerCount}");
                return false;
            }
        }

        // Check if equipment exists in inventory
        if (!InventoryManager.Instance.HasItem(equipmentId))
        {
            GD.Print($"[PetEquipmentEnhancement] Equipment not found in inventory");
            return false;
        }

        return true;
    }

    // Get equipment type from equipment ID
    private string GetEquipmentType(string equipmentId)
    {
        if (_petEquipmentSystem != null)
        {
            var items = PetEquipmentDatabase.GetAllPetEquipment();
            var item = items.FirstOrDefault(i => i.Id == equipmentId);
            if (item != null)
            {
                return item.Type.ToLower();
            }
        }
        
        // Fallback: infer from ID
        if (equipmentId.ContainsKey("collar")) return "collar";
        if (equipmentId.ContainsKey("harness")) return "harness";
        if (equipmentId.ContainsKey("armor")) return "armor";
        if (equipmentId.ContainsKey("accessory")) return "accessory";
        if (equipmentId.ContainsKey("toy")) return "toy";
        return "accessory";
    }

    // Attempt enhancement
    public PetEquipmentEnhancementData.EnhancementResult TryEnhance(string equipmentId, PetEquipmentEnhancementData.EnhancementTier targetTier)
    {
        if (!CanEnhance(equipmentId, targetTier))
        {
            return PetEquipmentEnhancementData.EnhancementResult.Failure;
        }

        var equipment = GetEquipmentEnhancement(equipmentId);
        float successRate = PetEquipmentEnhancementDatabase.GetSuccessRate(targetTier);
        float criticalRate = PetEquipmentEnhancementDatabase.GetCriticalRate(targetTier);

        // Deduct gold
        int cost = PetEquipmentEnhancementDatabase.GetEnhancementCost(targetTier);
        Player.Instance.Gold -= cost;
        _playerData.TotalGoldSpent += cost;

        // Deduct materials
        var equipmentType = GetEquipmentType(equipmentId);
        var materials = PetEquipmentEnhancementDatabase.GetMaterialsForEnhancement(equipmentType, targetTier);
        
        foreach (var mat in materials)
        {
            InventoryManager.Instance.RemoveItem(mat.Id, mat.Quantity);
        }

        // Roll for result
        float roll = (float)GD.Randd();
        _playerData.TotalEnhancements++;

        EnhancementStarted?.Invoke(equipmentId, (int)targetTier);

        if (roll < successRate)
        {
            // Success
            bool isCritical = roll < criticalRate;
            
            equipment.Tier = targetTier;
            equipment.CurrentTier = (int)targetTier;
            equipment.SuccessCount++;
            equipment.LastEnhanceTime = DateTime.Now;
            
            _playerData.SuccessCount++;
            if (isCritical)
            {
                _playerData.CriticalCount++;
            }

            // Apply enhancement bonus to equipment
            ApplyEnhancementBonus(equipmentId, targetTier, isCritical);

            GD.Print($"[PetEquipmentEnhancement] Enhancement SUCCESS! Equipment: {equipmentId}, Tier: {targetTier}, Critical: {isCritical}");
            EnhancementSucceeded?.Invoke(equipmentId, (int)targetTier, isCritical);
            
            SaveEnhancementData();
            return isCritical ? PetEquipmentEnhancementData.EnhancementResult.CriticalSuccess : PetEquipmentEnhancementData.EnhancementResult.Success;
        }
        else
        {
            // Failure
            equipment.FailureCount++;
            
            _playerData.FailureCount++;
            
            // On failure, tier may decrease
            if (targetTier > PetEquipmentEnhancementData.EnhancementTier.Basic && GD.Randd() < 0.3)
            {
                var newTier = (PetEquipmentEnhancementData.EnhancementTier)((int)targetTier - 1);
                equipment.Tier = newTier;
                equipment.CurrentTier = (int)newTier;
                GD.Print($"[PetEquipmentEnhancement] Enhancement FAILED and degraded! Equipment: {equipmentId}, New Tier: {newTier}");
            }
            else
            {
                GD.Print($"[PetEquipmentEnhancement] Enhancement FAILED! Equipment: {equipmentId}, Tier unchanged");
            }

            EnhancementFailed?.Invoke(equipmentId, equipment.CurrentTier);
            
            SaveEnhancementData();
            return PetEquipmentEnhancementData.EnhancementResult.Failure;
        }
    }

    // Apply enhancement bonus to equipment
    private void ApplyEnhancementBonus(string equipmentId, PetEquipmentEnhancementData.EnhancementTier tier, bool isCritical)
    {
        float multiplier = PetEquipmentEnhancementDatabase.GetBonusMultiplier(tier);
        if (isCritical)
        {
            multiplier *= 1.5f; // Critical gives extra 50%
        }

        // Store the enhancement bonus on the player for later calculation
        // This will be applied when calculating pet equipment bonuses
        if (!Player.Instance.EnhancementBonuses.ContainsKey(equipmentId))
        {
            Player.Instance.EnhancementBonuses[equipmentId] = 1.0f;
        }
        Player.Instance.EnhancementBonuses[equipmentId] = multiplier;
        
        GD.Print($"[PetEquipmentEnhancement] Applied bonus {multiplier}x to {equipmentId}");
    }

    // Get enhancement bonus for equipment
    public float GetEnhancementBonus(string equipmentId)
    {
        if (Player.Instance.EnhancementBonuses.TryGetValue(equipmentId, out var bonus))
        {
            return bonus;
        }
        return 1.0f;
    }

    // Get statistics
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "totalEnhancements", _playerData.TotalEnhancements },
            { "successCount", _playerData.SuccessCount },
            { "criticalCount", _playerData.CriticalCount },
            { "failureCount", _playerData.FailureCount },
            { "totalGoldSpent", _playerData.TotalGoldSpent },
            { "successRate", _playerData.TotalEnhancements > 0 ? (float)_playerData.SuccessCount / _playerData.TotalEnhancements : 0 },
            { "criticalRate", _playerData.TotalEnhancements > 0 ? (float)_playerData.CriticalCount / _playerData.TotalEnhancements : 0 }
        };
    }

    // Save/Load
    public Dictionary<string, object> GetSaveData()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        
        data["totalEnhancements"] = _playerData.TotalEnhancements;
        data["successCount"] = _playerData.SuccessCount;
        data["criticalCount"] = _playerData.CriticalCount;
        data["failureCount"] = _playerData.FailureCount;
        data["totalGoldSpent"] = _playerData.TotalGoldSpent;
        
        List<Dictionary<string, object>> equipmentList = new List<Dictionary<string, object>>();
        foreach (var eq in _playerData.EquipmentList)
        {
            equipmentList.Add(new Dictionary<string, object>
            {
                { "equipmentId", eq.EquipmentId },
                { "currentTier", eq.CurrentTier },
                { "successCount", eq.SuccessCount },
                { "failureCount", eq.FailureCount },
                { "lastEnhanceTime", eq.LastEnhanceTime.ToString("o") }
            });
        }
        data["equipmentList"] = equipmentList;
        
        return data;
    }

    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        _playerData.TotalEnhancements = data.GetValueOrDefault("totalEnhancements", 0);
        _playerData.SuccessCount = data.GetValueOrDefault("successCount", 0);
        _playerData.CriticalCount = data.GetValueOrDefault("criticalCount", 0);
        _playerData.FailureCount = data.GetValueOrDefault("failureCount", 0);
        _playerData.TotalGoldSpent = data.GetValueOrDefault("totalGoldSpent", 0);

        if (data.TryGetValue("equipmentList", out var eqListObj))
        {
            var eqList = eqListObj as List<object>;
            if (eqList != null)
            {
                _playerData.EquipmentList.Clear();
                foreach (var eqObj in eqList)
                {
                    var eqDict = eqObj as Dictionary<string, object>;
                    if (eqDict != null)
                    {
                        var eq = new PetEquipmentEnhancementData.EquipmentEnhancement
                        {
                            EquipmentId = eqDict.GetValueOrDefault("equipmentId", ""),
                            CurrentTier = eqDict.GetValueOrDefault("currentTier", 0),
                            Tier = (PetEquipmentEnhancementData.EnhancementTier)eqDict.GetValueOrDefault("currentTier", 0),
                            SuccessCount = eqDict.GetValueOrDefault("successCount", 0),
                            FailureCount = eqDict.GetValueOrDefault("failureCount", 0)
                        };
                        
                        if (DateTime.TryParse(eqDict.GetValueOrDefault("lastEnhanceTime", "").ToString(), out var lastTime))
                        {
                            eq.LastEnhanceTime = lastTime;
                        }
                        
                        _playerData.EquipmentList.Add(eq);
                        
                        // Restore enhancement bonuses
                        if (eq.CurrentTier > 0)
                        {
                            float bonus = PetEquipmentEnhancementDatabase.GetBonusMultiplier(eq.Tier);
                            if (!Player.Instance.EnhancementBonuses.ContainsKey(eq.EquipmentId))
                            {
                                Player.Instance.EnhancementBonuses[eq.EquipmentId] = bonus;
                            }
                        }
                    }
                }
            }
        }

        GD.Print($"[PetEquipmentEnhancement] Loaded data: {_playerData.TotalEnhancements} enhancements");
        EnhancementDataLoaded?.Invoke();
    }

    private void SaveEnhancementData()
    {
        SaveSystem.SaveGame();
    }

        public override Dictionary<string, object> ExportSaveData() => GetSaveData();
        public override void ImportSaveData(Dictionary<string, object> data) => LoadSaveData(data);
}
