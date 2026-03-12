using Godot;
using System;
using System.Collections.Generic;

public class ConstellationSystem : Node
{
    private ConstellationData _data;
    private ConstellationDatabase _database;
    
    // Singleton instance
    public static ConstellationSystem Instance { get; private set; }
    
    public override void _Ready()
    {
        Instance = this;
        
        _data = new ConstellationData();
        _data._Ready();
        
        _database = new ConstellationDatabase();
        _database._Ready();
        
        LoadConstellationData();
    }
    
    private void LoadConstellationData()
    {
        // Try to load from save
        var saveSystem = GetNode("/root/SaveSystem");
        if (saveSystem != null)
        {
            // Load constellation data if save exists
        }
    }
    
    // Unlock a constellation
    public bool UnlockConstellation(string constellationId, int playerGold, int playerLevel)
    {
        var constellation = _database.GetConstellation(constellationId);
        if (constellation == null)
        {
            GD.Print($"[ConstellationSystem] Constellation not found: {constellationId}");
            return false;
        }
        
        // Check if already unlocked
        if (_data.UnlockedConstellations.ContainsKey(constellationId) && 
            _data.UnlockedConstellations[constellationId].Unlocked)
        {
            GD.Print($"[ConstellationSystem] Constellation already unlocked: {constellationId}");
            return false;
        }
        
        // Check level requirement
        if (playerLevel < constellation.RequiredLevel)
        {
            GD.Print($"[ConstellationSystem] Player level {playerLevel} too low, required: {constellation.RequiredLevel}");
            return false;
        }
        
        // Check gold
        if (playerGold < constellation.UnlockCost)
        {
            GD.Print($"[ConstellationSystem] Not enough gold: {playerGold}, required: {constellation.UnlockCost}");
            return false;
        }
        
        // Unlock the constellation
        var progress = new ConstellationData.ConstellationProgress
        {
            ConstellationId = constellationId,
            Unlocked = true,
            ActivatedStars = 0,
            TotalStars = constellation.Stars,
            UnlockTime = DateTime.Now
        };
        
        _data.UnlockedConstellations[constellationId] = progress;
        _data.TotalConstellationsUnlocked++;
        _data.GoldSpentOnConstellations += constellation.UnlockCost;
        
        // Deduct gold (assume caller handles this)
        SaveConstellationData();
        
        GD.Print($"[ConstellationSystem] Unlocked constellation: {constellation.Name} for {constellation.UnlockCost} gold");
        return true;
    }
    
    // Activate stars in a constellation
    public bool ActivateStars(string constellationId, int starsToActivate, int playerGold)
    {
        if (!_data.UnlockedConstellations.ContainsKey(constellationId))
        {
            GD.Print($"[ConstellationSystem] Constellation not unlocked: {constellationId}");
            return false;
        }
        
        var progress = _data.UnlockedConstellations[constellationId];
        var constellation = _database.GetConstellation(constellationId);
        
        if (constellation == null)
            return false;
        
        int maxStars = constellation.Stars;
        int currentStars = progress.ActivatedStars;
        int availableStars = maxStars - currentStars;
        
        if (availableStars <= 0)
        {
            GD.Print($"[ConstellationSystem] All stars already activated for: {constellationId}");
            return false;
        }
        
        int starsToAdd = Math.Min(starsToActivate, availableStars);
        int costPerStar = 50 + (currentStars * 10); // Cost increases with each star
        int totalCost = costPerStar * starsToAdd;
        
        if (playerGold < totalCost)
        {
            GD.Print($"[ConstellationSystem] Not enough gold for stars: {playerGold}, required: {totalCost}");
            return false;
        }
        
        progress.ActivatedStars += starsToAdd;
        _data.UsedActivationPoints += starsToAdd;
        _data.TotalStarsActivated += starsToAdd;
        _data.GoldSpentOnConstellations += totalCost;
        
        SaveConstellationData();
        
        GD.Print($"[ConstellationSystem] Activated {starsToAdd} stars in {constellation.Name}, total: {progress.ActivatedStars}/{maxStars}");
        return true;
    }
    
    // Get total bonuses from all activated constellations
    public Dictionary<string, float> GetTotalBonuses()
    {
        Dictionary<string, float> bonuses = new Dictionary<string, float>
        {
            { "attack", 0f },
            { "defense", 0f },
            { "health", 0f },
            { "speed", 0f },
            { "critical", 0f },
            { "evasion", 0f },
            { "gold", 0f },
            { "exp", 0f }
        };
        
        foreach (var kvp in _data.UnlockedConstellations)
        {
            var progress = kvp.Value;
            if (!progress.Unlocked || progress.ActivatedStars <= 0)
                continue;
            
            var constellation = _database.GetConstellation(kvp.Key);
            if (constellation == null)
                continue;
            
            float activationRatio = (float)progress.ActivatedStars / progress.TotalStars;
            
            bonuses["attack"] += constellation.AttackBonus * activationRatio;
            bonuses["defense"] += constellation.DefenseBonus * activationRatio;
            bonuses["health"] += constellation.HealthBonus * activationRatio;
            bonuses["speed"] += constellation.SpeedBonus * activationRatio;
            bonuses["critical"] += constellation.CriticalBonus * activationRatio;
            bonuses["evasion"] += constellation.EvasionBonus * activationRatio;
            bonuses["gold"] += constellation.GoldBonus * activationRatio;
            bonuses["exp"] += constellation.ExpBonus * activationRatio;
        }
        
        return bonuses;
    }
    
    // Check if constellation is unlocked
    public bool IsConstellationUnlocked(string constellationId)
    {
        if (_data.UnlockedConstellations.ContainsKey(constellationId))
            return _data.UnlockedConstellations[constellationId].Unlocked;
        return false;
    }
    
    // Get constellation progress
    public ConstellationData.ConstellationProgress GetConstellationProgress(string constellationId)
    {
        if (_data.UnlockedConstellations.ContainsKey(constellationId))
            return _data.UnlockedConstellations[constellationId];
        return null;
    }
    
    // Get all unlocked constellations
    public Dictionary<string, ConstellationData.ConstellationProgress> GetUnlockedConstellations()
    {
        return new Dictionary<string, ConstellationData.ConstellationProgress>(_data.UnlockedConstellations);
    }
    
    // Add constellation fragments (currency for constellation system)
    public void AddFragments(int amount)
    {
        _data.ConstellationFragments += amount;
        _data.FragmentsCollected += amount;
        SaveConstellationData();
    }
    
    // Spend fragments to unlock constellation (alternative to gold)
    public bool UnlockWithFragments(string constellationId, int playerLevel)
    {
        var constellation = _database.GetConstellation(constellationId);
        if (constellation == null)
            return false;
        
        int fragmentCost = constellation.UnlockCost / 2; // Fragments are more valuable
        
        if (_data.ConstellationFragments < fragmentCost)
        {
            GD.Print($"[ConstellationSystem] Not enough fragments: {_data.ConstellationFragments}, required: {fragmentCost}");
            return false;
        }
        
        if (playerLevel < constellation.RequiredLevel)
        {
            GD.Print($"[ConstellationSystem] Player level too low: {playerLevel}, required: {constellation.RequiredLevel}");
            return false;
        }
        
        _data.ConstellationFragments -= fragmentCost;
        
        return UnlockConstellation(constellationId, 0, playerLevel);
    }
    
    // Get statistics
    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "total_unlocked", _data.TotalConstellationsUnlocked },
            { "total_stars_activated", _data.TotalStarsActivated },
            { "gold_spent", _data.GoldSpentOnConstellations },
            { "fragments_collected", _data.FragmentsCollected },
            { "current_fragments", _data.ConstellationFragments }
        };
    }
    
    // Save data
    private void SaveConstellationData()
    {
        var saveSystem = GetNode("/root/SaveSystem");
        if (saveSystem != null)
        {
            // Save constellation data
        }
    }
    
    // Get database reference
    public ConstellationDatabase GetDatabase()
    {
        return _database;
    }
    
    // Get data reference
    public ConstellationData GetData()
    {
        return _data;
    }
}
