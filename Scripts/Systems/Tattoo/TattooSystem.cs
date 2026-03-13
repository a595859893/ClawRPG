using Godot;
using System;
using System.Collections.Generic;

public class TattooSystem
{
    private TattooData _data;
    private TattooDatabase _database;
    
    public TattooSystem()
    {
        _database = new TattooDatabase();
        _data = new TattooData();
        
        // Initialize unlocked tattoos
        InitializeDefaultTattoos();
    }
    
    private void InitializeDefaultTattoos()
    {
        // Start with some basic tattoos unlocked
        _data.UnlockedTattoos["tattoo_battle_1"] = true;
        _data.UnlockedTattoos["tattoo_nature_1"] = true;
        _data.UnlockedTattoos["tattoo_deco_1"] = true;
    }
    
    // Check if player has a tattoo unlocked
    public bool IsTattooUnlocked(string tattooId)
    {
        if (_data.UnlockedTattoos.ContainsKey(tattooId))
            return _data.UnlockedTattoos[tattooId];
        return false;
    }
    
    // Unlock a tattoo (purchase)
    public bool UnlockTattoo(string tattooId, int playerGold)
    {
        var tattoo = _database.GetTattoo(tattooId);
        if (tattoo == null)
            return false;
            
        if (IsTattooUnlocked(tattooId))
            return true; // Already unlocked
            
        if (playerGold < tattoo.Cost)
            return false; // Not enough gold
            
        _data.UnlockedTattoos[tattooId] = true;
        _data.TotalGoldSpent += tattoo.Cost;
        
        return true;
    }
    
    // Apply tattoo to a body slot
    public bool ApplyTattoo(string tattooId, string slot)
    {
        if (!IsTattooUnlocked(tattooId))
            return false; // Not unlocked
            
        var tattoo = _database.GetTattoo(tattooId);
        if (tattoo == null)
            return false;
            
        // Check if slot is valid
        if (!_database.BodySlots.ContainsKey(slot))
            return false;
            
        // Check if tattoo is available for this slot
        var slotTattoos = _database.BodySlots[slot];
        if (!slotTattoos.Contains(tattooId))
            return false;
            
        // Remove existing tattoo from slot if any
        if (_data.AppliedTattoos.ContainsKey(slot))
        {
            string oldTattoo = _data.AppliedTattoos[slot];
            if (_data.TattooUsageCount.ContainsKey(oldTattoo))
                _data.TattooUsageCount[oldTattoo]--;
        }
        
        // Apply new tattoo
        _data.AppliedTattoos[slot] = tattooId;
        
        // Track usage
        if (!_data.TattooUsageCount.ContainsKey(tattooId))
            _data.TattooUsageCount[tattooId] = 0;
        _data.TattooUsageCount[tattooId]++;
        
        _data.TotalTattoosApplied++;
        
        // Add to history
        _data.TattooHistory.Add($"{tattooId} -> {slot}");
        if (_data.TattooHistory.Count > 50)
            _data.TattooHistory.RemoveAt(0);
            
        return true;
    }
    
    // Remove tattoo from slot
    public bool RemoveTattoo(string slot)
    {
        if (!_data.AppliedTattoos.ContainsKey(slot))
            return false;
            
        _data.AppliedTattoos.Remove(slot);
        return true;
    }
    
    // Get applied tattoo for slot
    public string GetAppliedTattoo(string slot)
    {
        if (_data.AppliedTattoos.ContainsKey(slot))
            return _data.AppliedTattoos[slot];
        return null;
    }
    
    // Get all applied tattoos
    public Dictionary<string, string> GetAppliedTattoos()
    {
        return new Dictionary<string, string>(_data.AppliedTattoos);
    }
    
    // Calculate total bonuses from all applied tattoos
    public Dictionary<string, float> CalculateTotalBonuses()
    {
        Dictionary<string, float> bonuses = new Dictionary<string, float>
        {
            { "attack", 0f },
            { "defense", 0f },
            { "health", 0f },
            { "speed", 0f },
            { "critical", 0f },
            { "evasion", 0f }
        };
        
        foreach (var slotTattoo in _data.AppliedTattoos)
        {
            var tattoo = _database.GetTattoo(slotTattoo.Value);
            if (tattoo != null)
            {
                bonuses["attack"] += tattoo.AttackBonus;
                bonuses["defense"] += tattoo.DefenseBonus;
                bonuses["health"] += tattoo.HealthBonus;
                bonuses["speed"] += tattoo.SpeedBonus;
                bonuses["critical"] += tattoo.CriticalBonus;
                bonuses["evasion"] += tattoo.EvasionBonus;
            }
        }
        
        return bonuses;
    }
    
    // Get all unlocked tattoos
    public List<string> GetUnlockedTattoos()
    {
        List<string> result = new List<string>();
        foreach (var kvp in _data.UnlockedTattoos)
        {
            if (kvp.Value)
                result.Add(kvp.Key);
        }
        return result;
    }
    
    // Get available slots
    public List<string> GetAvailableSlots()
    {
        return _database.GetAvailableSlots();
    }
    
    // Get tattoo info
    public TattooDatabase.TattooConfig GetTattooInfo(string tattooId)
    {
        return _database.GetTattoo(tattooId);
    }
    
    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        Dictionary<string, int> stats = new Dictionary<string, int>
        {
            { "total_unlocked", _data.UnlockedTattoos.Count },
            { "total_applied", _data.AppliedTattoos.Count },
            { "total_applied_count", _data.TotalTattoosApplied },
            { "total_gold_spent", _data.TotalGoldSpent },
            { "common_count", GetRarityCount(TattooDatabase.TattooRarity.Common) },
            { "uncommon_count", GetRarityCount(TattooDatabase.TattooRarity.Uncommon) },
            { "rare_count", GetRarityCount(TattooDatabase.TattooRarity.Rare) },
            { "epic_count", GetRarityCount(TattooDatabase.TattooRarity.Epic) },
            { "legendary_count", GetRarityCount(TattooDatabase.TattooRarity.Legendary) }
        };
        return stats;
    }
    
    private int GetRarityCount(TattooDatabase.TattooRarity rarity)
    {
        int count = 0;
        foreach (var kvp in _data.UnlockedTattoos)
        {
            if (kvp.Value)
            {
                var tattoo = _database.GetTattoo(kvp.Key);
                if (tattoo != null && tattoo.Rarity == rarity)
                    count++;
            }
        }
        return count;
    }
    
    // Get data for save
    public TattooData GetData()
    {
        return _data;
    }
    
    // Load data from save
    public void LoadData(TattooData data)
    {
        if (data != null)
            _data = data;
    }
}
