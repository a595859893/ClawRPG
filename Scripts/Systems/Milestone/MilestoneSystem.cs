using Godot;
using System;
using System.Collections.Generic;

public class MilestoneSystem
{
    private static MilestoneSystem _instance;
    public static MilestoneSystem Instance => _instance ?? (_instance = new MilestoneSystem());
    
    private MilestoneData _data = new MilestoneData();
    private MilestoneDatabase _database = MilestoneDatabase.Instance;
    
    public event Action<string> OnMilestoneUnlocked;
    
    public MilestoneSystem()
    {
        LoadData();
    }
    
    public void LoadData()
    {
        // TODO: Load from file
    }
    
    public void SaveData()
    {
        // TODO: Save to file
    }
    
    public void UpdateProgress(string milestoneId, int newValue)
    {
        if (!_database.Milestones.ContainsKey(milestoneId))
            return;
            
        var config = _database.Milestones[milestoneId];
        
        if (!_data.Milestones.ContainsKey(milestoneId))
        {
            _data.Milestones[milestoneId] = new MilestoneData.MilestoneEntry
            {
                Id = milestoneId,
                Name = config.Name,
                Description = config.Description,
                Category = config.Category,
                Tier = ParseTier(config.Tier),
                RequiredValue = config.RequiredValue,
                CurrentValue = 0,
                Unlocked = false,
                Rewards = new List<string>()
            };
        }
        
        var entry = _data.Milestones[milestoneId];
        entry.CurrentValue = newValue;
        
        if (!entry.Unlocked && newValue >= config.RequiredValue)
        {
            UnlockMilestone(milestoneId);
        }
        
        SaveData();
    }
    
    public void IncrementProgress(string milestoneId)
    {
        if (!_database.Milestones.ContainsKey(milestoneId))
            return;
            
        int currentValue = 0;
        if (_data.Milestones.ContainsKey(milestoneId))
            currentValue = _data.Milestones[milestoneId].CurrentValue;
            
        UpdateProgress(milestoneId, currentValue + 1);
    }
    
    private void UnlockMilestone(string milestoneId)
    {
        if (!_data.Milestones.ContainsKey(milestoneId))
            return;
            
        var entry = _data.Milestones[milestoneId];
        entry.Unlocked = true;
        entry.UnlockTime = DateTime.Now;
        
        var config = _database.Milestones[milestoneId];
        foreach (var reward in config.Rewards)
        {
            entry.Rewards.Add($"{reward.Key}: {reward.Value}");
        }
        
        UpdateStatistics(entry);
        
        OnMilestoneUnlocked?.Invoke(milestoneId);
    }
    
    private void UpdateStatistics(MilestoneData.MilestoneEntry entry)
    {
        _data.Statistics.TotalMilestones++;
        _data.Statistics.UnlockedMilestones++;
        
        switch (entry.Tier)
        {
            case MilestoneData.MilestoneTier.Bronze:
                _data.Statistics.BronzeMilestones++;
                break;
            case MilestoneData.MilestoneTier.Silver:
                _data.Statistics.SilverMilestones++;
                break;
            case MilestoneData.MilestoneTier.Gold:
                _data.Statistics.GoldMilestones++;
                break;
            case MilestoneData.MilestoneTier.Platinum:
                _data.Statistics.PlatinumMilestones++;
                break;
            case MilestoneData.MilestoneTier.Diamond:
                _data.Statistics.DiamondMilestones++;
                break;
            case MilestoneData.MilestoneTier.Legendary:
                _data.Statistics.LegendaryMilestones++;
                break;
        }
        
        var config = _database.Milestones[entry.Id];
        if (config.Rewards.ContainsKey("gold"))
            _data.Statistics.TotalGoldEarned += config.Rewards["gold"];
        if (config.Rewards.ContainsKey("exp"))
            _data.Statistics.TotalExpEarned += config.Rewards["exp"];
    }
    
    private MilestoneData.MilestoneTier ParseTier(string tier)
    {
        switch (tier)
        {
            case "Bronze": return MilestoneData.MilestoneTier.Bronze;
            case "Silver": return MilestoneData.MilestoneTier.Silver;
            case "Gold": return MilestoneData.MilestoneTier.Gold;
            case "Platinum": return MilestoneData.MilestoneTier.Platinum;
            case "Diamond": return MilestoneData.MilestoneTier.Diamond;
            case "Legendary": return MilestoneData.MilestoneTier.Legendary;
            default: return MilestoneData.MilestoneTier.Bronze;
        }
    }
    
    public bool IsMilestoneUnlocked(string milestoneId)
    {
        if (!_data.Milestones.ContainsKey(milestoneId))
            return false;
        return _data.Milestones[milestoneId].Unlocked;
    }
    
    public int GetMilestoneProgress(string milestoneId)
    {
        if (!_data.Milestones.ContainsKey(milestoneId))
            return 0;
        return _data.Milestones[milestoneId].CurrentValue;
    }
    
    public int GetMilestoneRequired(string milestoneId)
    {
        if (!_database.Milestones.ContainsKey(milestoneId))
            return 0;
        return _database.Milestones[milestoneId].RequiredValue;
    }
    
    public List<MilestoneData.MilestoneEntry> GetAllMilestones()
    {
        var result = new List<MilestoneData.MilestoneEntry>();
        foreach (var entry in _data.Milestones.Values)
        {
            result.Add(entry);
        }
        return result;
    }
    
    public List<MilestoneData.MilestoneEntry> GetMilestonesByCategory(string category)
    {
        var result = new List<MilestoneData.MilestoneEntry>();
        foreach (var entry in _data.Milestones.Values)
        {
            if (entry.Category == category)
                result.Add(entry);
        }
        return result;
    }
    
    public List<MilestoneData.MilestoneEntry> GetUnlockedMilestones()
    {
        var result = new List<MilestoneData.MilestoneEntry>();
        foreach (var entry in _data.Milestones.Values)
        {
            if (entry.Unlocked)
                result.Add(entry);
        }
        return result;
    }
    
    public List<MilestoneData.MilestoneEntry> GetLockedMilestones()
    {
        var result = new List<MilestoneData.MilestoneEntry>();
        foreach (var entry in _data.Milestones.Values)
        {
            if (!entry.Unlocked)
                result.Add(entry);
        }
        return result;
    }
    
    public MilestoneData.MilestoneStatistics GetStatistics()
    {
        return _data.Statistics;
    }
    
    public List<string> GetCategories()
    {
        return _database.GetCategories();
    }
    
    // Convenience methods for common milestone updates
    public void OnEnemyDefeated() => IncrementProgress("combat_first_kill");
    public void OnBossDefeated() => IncrementProgress("boss_first");
    public void OnDungeonCompleted() => IncrementProgress("dungeon_first");
    public void OnPetObtained() => IncrementProgress("pet_first");
    public void OnGameWon() => IncrementProgress("win_first");
    public void OnAchievementUnlocked() => IncrementProgress("achievement_10");
    
    public void UpdateFloorProgress(int floor)
    {
        UpdateProgress("floor_10", floor);
        UpdateProgress("floor_25", floor);
        UpdateProgress("floor_50", floor);
        UpdateProgress("floor_100", floor);
        UpdateProgress("floor_250", floor);
        UpdateProgress("floor_500", floor);
    }
    
    public void UpdateLevelProgress(int level)
    {
        UpdateProgress("level_5", level);
        UpdateProgress("level_10", level);
        UpdateProgress("level_25", level);
        UpdateProgress("level_50", level);
        UpdateProgress("level_100", level);
        UpdateProgress("level_200", level);
    }
    
    public void UpdateGoldProgress(int gold)
    {
        UpdateProgress("gold_1000", gold);
        UpdateProgress("gold_10000", gold);
        UpdateProgress("gold_100000", gold);
        UpdateProgress("gold_1000000", gold);
        UpdateProgress("gold_10000000", gold);
    }
}
