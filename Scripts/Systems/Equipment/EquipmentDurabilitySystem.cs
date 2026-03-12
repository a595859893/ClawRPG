using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Equipment;

public class DurabilityData
{
    public int MaxDurability { get; set; } = 100;
    public int CurrentDurability { get; set; } = 100;
    public float DurabilityLossMultiplier { get; set; } = 1.0f;
    public bool IsBroken => CurrentDurability <= 0;
    public float IntegrityPercent => (float)CurrentDurability / MaxDurability;
    
    // Repair history
    public int RepairCount { get; set; } = 0;
    public int TotalDurabilityRepaired { get; set; } = 0;
    public DateTime LastRepairTime { get; set; }
    
    // Degradation tracking
    public int TimesBroken { get; set; } = 0;
    public int TotalDamageDealt { get; set; } = 0;
    public int TotalDamageReceived { get; set; } = 0;
}

public class DurabilityEffect
{
    public float AttackMultiplier { get; set; } = 1.0f;
    public float DefenseMultiplier { get; set; } = 1.0f;
    public float SpeedMultiplier { get; set; } = 1.0f;
    public bool CanUse { get; set; } = true;
}

public enum DurabilityState
{
    Perfect,      // 100%
    Excellent,    // 75-99%
    Good,         // 50-74%
    Worn,         // 25-49%
    Critical,     // 1-24%
    Broken        // 0%
}

public class EquipmentDurabilitySystem : Node
{
    public static EquipmentDurabilitySystem Instance { get; private set; }
    
    // Durability settings
    private float _globalDurabilityLoss = 1.0f;
    private float _combatDurabilityLoss = 2.0f;
    private float _skillUseDurabilityLoss = 1.5f;
    private float _movementDurabilityLoss = 0.1f;
    
    // Repair settings
    private float _repairCostMultiplier = 1.0f;
    private int _repairItemId = 1001; // Repair kit item ID
    private int _maxRepairPerSession = 5;
    
    // Statistics
    public int TotalItemsDamaged { get; private set; } = 0;
    public int TotalItemsBroken { get; private set; } = 0;
    public int TotalRepairCosts { get; private set; } = 0;
    public int TotalRepairsPerformed { get; private set; } = 0;
    
    private Dictionary<int, DurabilityData> _equipmentDurability = new Dictionary<int, DurabilityData>();
    private Dictionary<int, DurabilityState> _lastState = new Dictionary<int, DurabilityState>();
    
    public override void _Ready()
    {
        Instance = this;
    }
    
    public void Initialize()
    {
        LoadDurabilitySettings();
    }
    
    private void LoadDurabilitySettings()
    {
        // Load from game settings or use defaults
        _globalDurabilityLoss = 1.0f;
        _combatDurabilityLoss = 2.0f;
        _skillUseDurabilityLoss = 1.5f;
        _movementDurabilityLoss = 0.1f;
        _repairCostMultiplier = 1.0f;
    }
    
    // Register equipment with durability tracking
    public void RegisterEquipment(int equipmentId, int maxDurability = 100)
    {
        if (!_equipmentDurability.ContainsKey(equipmentId))
        {
            _equipmentDurability[equipmentId] = new DurabilityData
            {
                MaxDurability = maxDurability,
                CurrentDurability = maxDurability
            };
            _lastState[equipmentId] = DurabilityState.Perfect;
            TotalItemsDamaged++;
        }
    }
    
    // Unregister equipment
    public void UnregisterEquipment(int equipmentId)
    {
        if (_equipmentDurability.ContainsKey(equipmentId))
        {
            _equipmentDurability.Remove(equipmentId);
            _lastState.Remove(equipmentId);
        }
    }
    
    // Apply damage to equipment
    public void ApplyDamage(int equipmentId, int damage)
    {
        if (!_equipmentDurability.TryGetValue(equipmentId, out var data))
            return;
            
        data.CurrentDurability = Math.Max(0, data.CurrentDurability - damage);
        data.TotalDamageReceived += damage;
        
        var newState = GetDurabilityState(equipmentId);
        if (newState != _lastState[equipmentId])
        {
            _lastState[equipmentId] = newState;
            OnDurabilityStateChanged(equipmentId, newState);
        }
        
        if (data.IsBroken && data.TimesBroken == 0)
        {
            data.TimesBroken++;
            TotalItemsBroken++;
        }
    }
    
    // Reduce durability from combat
    public void ReduceFromCombat(int equipmentId, int damageDealt = 0)
    {
        if (!_equipmentDurability.TryGetValue(equipmentId, out var data))
            return;
            
        int loss = (int)(_combatDurabilityLoss * data.DurabilityLossMultiplier);
        data.CurrentDurability = Math.Max(0, data.CurrentDurability - loss);
        data.TotalDamageDealt += damageDealt;
        
        CheckStateChange(equipmentId);
    }
    
    // Reduce durability from skill use
    public void ReduceFromSkillUse(int equipmentId)
    {
        if (!_equipmentDurability.TryGetValue(equipmentId, out var data))
            return;
            
        int loss = (int)(_skillUseDurabilityLoss * data.DurabilityLossMultiplier);
        data.CurrentDurability = Math.Max(0, data.CurrentDurability - loss);
        
        CheckStateChange(equipmentId);
    }
    
    // Reduce durability from movement
    public void ReduceFromMovement(int equipmentId, float delta)
    {
        if (!_equipmentDurability.TryGetValue(equipmentId, out var data))
            return;
            
        int loss = (int)(_movementDurabilityLoss * data.DurabilityLossMultiplier * delta);
        data.CurrentDurability = Math.Max(0, data.CurrentDurability - loss);
        
        CheckStateChange(equipmentId);
    }
    
    private void CheckStateChange(int equipmentId)
    {
        if (!_equipmentDurability.TryGetValue(equipmentId, out var data))
            return;
            
        var newState = GetDurabilityState(equipmentId);
        if (_lastState.TryGetValue(equipmentId, out var last) && newState != last)
        {
            _lastState[equipmentId] = newState;
            OnDurabilityStateChanged(equipmentId, newState);
        }
        
        if (data.IsBroken && data.TimesBroken == 0)
        {
            data.TimesBroken++;
            TotalItemsBroken++;
        }
    }
    
    // Get current durability state
    public DurabilityState GetDurabilityState(int equipmentId)
    {
        if (!_equipmentDurability.TryGetValue(equipmentId, out var data))
            return DurabilityState.Perfect;
            
        if (data.IsBroken)
            return DurabilityState.Broken;
            
        float percent = data.IntegrityPercent;
        
        if (percent >= 0.75f) return DurabilityState.Excellent;
        if (percent >= 0.50f) return DurabilityState.Good;
        if (percent >= 0.25f) return DurabilityState.Worn;
        return DurabilityState.Critical;
    }
    
    // Get durability effect multipliers
    public DurabilityEffect GetDurabilityEffect(int equipmentId)
    {
        var effect = new DurabilityEffect();
        
        if (!_equipmentDurability.TryGetValue(equipmentId, out var data))
            return effect;
            
        float percent = data.IntegrityPercent;
        
        // Apply penalties based on durability
        if (percent >= 0.75f)
        {
            effect.AttackMultiplier = 1.0f;
            effect.DefenseMultiplier = 1.0f;
            effect.SpeedMultiplier = 1.0f;
            effect.CanUse = true;
        }
        else if (percent >= 0.50f)
        {
            effect.AttackMultiplier = 0.95f;
            effect.DefenseMultiplier = 0.95f;
            effect.SpeedMultiplier = 0.95f;
            effect.CanUse = true;
        }
        else if (percent >= 0.25f)
        {
            effect.AttackMultiplier = 0.80f;
            effect.DefenseMultiplier = 0.80f;
            effect.SpeedMultiplier = 0.80f;
            effect.CanUse = true;
        }
        else if (percent > 0)
        {
            effect.AttackMultiplier = 0.50f;
            effect.DefenseMultiplier = 0.50f;
            effect.SpeedMultiplier = 0.50f;
            effect.CanUse = true;
        }
        else
        {
            effect.AttackMultiplier = 0.0f;
            effect.DefenseMultiplier = 0.0f;
            effect.SpeedMultiplier = 0.0f;
            effect.CanUse = false;
        }
        
        return effect;
    }
    
    // Repair equipment
    public bool RepairEquipment(int equipmentId, int amount)
    {
        if (!_equipmentDurability.TryGetValue(equipmentId, out var data))
            return false;
            
        int repairCost = CalculateRepairCost(equipmentId, amount);
        
        // Check if player has enough gold
        // int playerGold = PlayerInventory.GetGold();
        // if (playerGold < repairCost) return false;
        
        int oldDurability = data.CurrentDurability;
        data.CurrentDurability = Math.Min(data.MaxDurability, data.CurrentDurability + amount);
        int actualRepaired = data.CurrentDurability - oldDurability;
        
        data.RepairCount++;
        data.TotalDurabilityRepaired += actualRepaired;
        data.LastRepairTime = DateTime.Now;
        
        TotalRepairCosts += repairCost;
        TotalRepairsPerformed++;
        
        CheckStateChange(equipmentId);
        
        return true;
    }
    
    // Repair all equipment
    public void RepairAllEquipment()
    {
        foreach (var kvp in _equipmentDurability)
        {
            RepairEquipment(kvp.Key, kvp.Value.MaxDurability);
        }
    }
    
    // Calculate repair cost
    public int CalculateRepairCost(int equipmentId, int amount)
    {
        if (!_equipmentDurability.TryGetValue(equipmentId, out var data))
            return 0;
            
        float baseCost = amount * 0.5f; // 0.5 gold per durability
        float conditionMultiplier = 1.0f + (1.0f - data.IntegrityPercent); // More damaged = more expensive
        float repairCountMultiplier = 1.0f + (data.RepairCount * 0.1f); // More repairs = slightly more expensive
        
        return (int)(baseCost * conditionMultiplier * repairCountMultiplier * _repairCostMultiplier);
    }
    
    // Event callbacks
    private void OnDurabilityStateChanged(int equipmentId, DurabilityState newState)
    {
        // Emit signal or call UI system
        // EmitSignal(nameof(DurabilityStateChanged), equipmentId, newState);
        
        // Show notification when equipment breaks
        if (newState == DurabilityState.Broken)
        {
            ShowDurabilityWarning(equipmentId, "装备已损坏!");
        }
        else if (newState == DurabilityState.Critical)
        {
            ShowDurabilityWarning(equipmentId, "装备耐久度临界!");
        }
    }
    
    private void ShowDurabilityWarning(int equipmentId, string message)
    {
        // This would integrate with the notification system
        GD.Print($"[Durability] Equipment {equipmentId}: {message}");
    }
    
    // Get durability data
    public DurabilityData GetDurabilityData(int equipmentId)
    {
        if (_equipmentDurability.TryGetValue(equipmentId, out var data))
            return data;
        return null;
    }
    
    // Get all equipment durability
    public Dictionary<int, DurabilityData> GetAllDurability()
    {
        return new Dictionary<int, DurabilityData>(_equipmentDurability);
    }
    
    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "TotalItemsDamaged", TotalItemsDamaged },
            { "TotalItemsBroken", TotalItemsBroken },
            { "TotalRepairCosts", TotalRepairCosts },
            { "TotalRepairsPerformed", TotalRepairsPerformed },
            { "TrackedEquipment", _equipmentDurability.Count }
        };
    }
    
    // Save durability data
    public Dictionary<string, object> SaveDurabilityData()
    {
        var data = new Dictionary<string, object>();
        
        var equipmentList = new List<Dictionary<string, object>>();
        foreach (var kvp in _equipmentDurability)
        {
            equipmentList.Add(new Dictionary<string, object>
            {
                { "equipmentId", kvp.Key },
                { "maxDurability", kvp.Value.MaxDurability },
                { "currentDurability", kvp.Value.CurrentDurability },
                { "durabilityLossMultiplier", kvp.Value.DurabilityLossMultiplier },
                { "repairCount", kvp.Value.RepairCount },
                { "totalDurabilityRepaired", kvp.Value.TotalDurabilityRepaired },
                { "timesBroken", kvp.Value.TimesBroken },
                { "totalDamageDealt", kvp.Value.TotalDamageDealt },
                { "totalDamageReceived", kvp.Value.TotalDamageReceived }
            });
        }
        
        data["equipment"] = equipmentList;
        data["totalItemsDamaged"] = TotalItemsDamaged;
        data["totalItemsBroken"] = TotalItemsBroken;
        data["totalRepairCosts"] = TotalRepairCosts;
        data["totalRepairsPerformed"] = TotalRepairsPerformed;
        
        return data;
    }
    
    // Load durability data
    public void LoadDurabilityData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        _equipmentDurability.Clear();
        _lastState.Clear();
        
        if (data.TryGetValue("equipment", out var equipmentObj) && equipmentObj is List<object> equipmentList)
        {
            foreach (var item in equipmentList)
            {
                if (item is Dictionary<string, object> eq)
                {
                    int equipmentId = Convert.ToInt32(eq["equipmentId"]);
                    var durabilityData = new DurabilityData
                    {
                        MaxDurability = Convert.ToInt32(eq["maxDurability"]),
                        CurrentDurability = Convert.ToInt32(eq["currentDurability"]),
                        DurabilityLossMultiplier = (float)Convert.ToDouble(eq["durabilityLossMultiplier"]),
                        RepairCount = Convert.ToInt32(eq["repairCount"]),
                        TotalDurabilityRepaired = Convert.ToInt32(eq["totalDurabilityRepaired"]),
                        TimesBroken = Convert.ToInt32(eq["timesBroken"]),
                        TotalDamageDealt = Convert.ToInt32(eq["totalDamageDealt"]),
                        TotalDamageReceived = Convert.ToInt32(eq["totalDamageReceived"])
                    };
                    
                    _equipmentDurability[equipmentId] = durabilityData;
                    _lastState[equipmentId] = GetDurabilityState(equipmentId);
                }
            }
        }
        
        TotalItemsDamaged = Convert.ToInt32(data.GetValueOrDefault("totalItemsDamaged", 0));
        TotalItemsBroken = Convert.ToInt32(data.GetValueOrDefault("totalItemsBroken", 0));
        TotalRepairCosts = Convert.ToInt32(data.GetValueOrDefault("totalRepairCosts", 0));
        TotalRepairsPerformed = Convert.ToInt32(data.GetValueOrDefault("totalRepairsPerformed", 0));
    }
}
