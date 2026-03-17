using Godot;
using System;
using System.Collections.Generic;

public partial class MilestoneSystem : BaseSystem
{
    private static MilestoneSystem _instance;
    public static MilestoneSystem Instance => _instance ?? (_instance = new MilestoneSystem());
    
    protected override string SystemName => "MilestoneSystem";
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
        var saveSystem = SaveSystem.Instance;
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null || data.Count == 0) return;

        // Load milestone data
        if (data.Contains("milestones"))
        {
            var milestoneData = (Godot.Dictionary)data["milestones"];
            
            // Load milestone entries
            if (milestoneData.Contains("entries"))
            {
                var entriesArray = (Godot.Array)milestoneData["entries"];
                foreach (Godot.Dictionary entryDict in entriesArray)
                {
                    var entry = new MilestoneData.MilestoneEntry
                    {
                        Id = (string)entryDict["id"],
                        Name = (string)entryDict["name"],
                        Description = (string)entryDict["description"],
                        Category = (string)entryDict["category"],
                        Tier = (MilestoneData.MilestoneTier)(int)entryDict["tier"],
                        RequiredValue = (int)entryDict["required_value"],
                        CurrentValue = (int)entryDict["current_value"],
                        Unlocked = (bool)entryDict["unlocked"]
                    };
                    
                    if (entryDict.Contains("unlock_time") && entryDict["unlock_time"] != null)
                        entry.UnlockTime = DateTime.Parse((string)entryDict["unlock_time"]);
                    
                    if (entryDict.Contains("rewards"))
                    {
                        var rewardsArray = (Godot.Array)entryDict["rewards"];
                        entry.Rewards = new List<string>();
                        foreach (string reward in rewardsArray)
                            entry.Rewards.Add(reward);
                    }
                    
                    _data.Milestones[entry.Id] = entry;
                }
            }
            
            // Load category progress
            if (milestoneData.Contains("category_progress"))
            {
                var catProgressDict = (Godot.Dictionary)milestoneData["category_progress"];
                foreach (var key in catProgressDict.Keys)
                {
                    _data.CategoryProgress[(string)key] = (int)catProgressDict[key];
                }
            }
            
            // Load statistics
            if (milestoneData.Contains("statistics"))
            {
                var statsDict = (Godot.Dictionary)milestoneData["statistics"];
                _data.Statistics.TotalMilestones = statsDict.Contains("total_milestones") ? (int)statsDict["total_milestones"] : 0;
                _data.Statistics.UnlockedMilestones = statsDict.Contains("unlocked_milestones") ? (int)statsDict["unlocked_milestones"] : 0;
                _data.Statistics.BronzeMilestones = statsDict.Contains("bronze_milestones") ? (int)statsDict["bronze_milestones"] : 0;
                _data.Statistics.SilverMilestones = statsDict.Contains("silver_milestones") ? (int)statsDict["silver_milestones"] : 0;
                _data.Statistics.GoldMilestones = statsDict.Contains("gold_milestones") ? (int)statsDict["gold_milestones"] : 0;
                _data.Statistics.PlatinumMilestones = statsDict.Contains("platinum_milestones") ? (int)statsDict["platinum_milestones"] : 0;
                _data.Statistics.DiamondMilestones = statsDict.Contains("diamond_milestones") ? (int)statsDict["diamond_milestones"] : 0;
                _data.Statistics.LegendaryMilestones = statsDict.Contains("legendary_milestones") ? (int)statsDict["legendary_milestones"] : 0;
                _data.Statistics.TotalRewardsClaimed = statsDict.Contains("total_rewards_claimed") ? (int)statsDict["total_rewards_claimed"] : 0;
                _data.Statistics.TotalGoldEarned = statsDict.Contains("total_gold_earned") ? (int)statsDict["total_gold_earned"] : 0;
                _data.Statistics.TotalExpEarned = statsDict.Contains("total_exp_earned") ? (int)statsDict["total_exp_earned"] : 0;
            }
        }
    }
    
    public void SaveData()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Dictionary();

        // Save milestone data
        var milestoneData = new Godot.Dictionary();
        
        // Save milestone entries
        var entriesArray = new Godot.Array();
        foreach (var kvp in _data.Milestones)
        {
            var entry = kvp.Value;
            var entryDict = new Godot.Dictionary();
            entryDict["id"] = entry.Id;
            entryDict["name"] = entry.Name;
            entryDict["description"] = entry.Description;
            entryDict["category"] = entry.Category;
            entryDict["tier"] = (int)entry.Tier;
            entryDict["required_value"] = entry.RequiredValue;
            entryDict["current_value"] = entry.CurrentValue;
            entryDict["unlocked"] = entry.Unlocked;
            entryDict["unlock_time"] = entry.UnlockTime?.ToString("o") ?? "";
            
            var rewardsArray = new Godot.Array();
            foreach (string reward in entry.Rewards)
                rewardsArray.Add(reward);
            entryDict["rewards"] = rewardsArray;
            
            entriesArray.Add(entryDict);
        }
        milestoneData["entries"] = entriesArray;
        
        // Save category progress
        var catProgressDict = new Godot.Dictionary();
        foreach (var kvp in _data.CategoryProgress)
            catProgressDict[kvp.Key] = kvp.Value;
        milestoneData["category_progress"] = catProgressDict;
        
        // Save statistics
        var statsDict = new Godot.Dictionary();
        statsDict["total_milestones"] = _data.Statistics.TotalMilestones;
        statsDict["unlocked_milestones"] = _data.Statistics.UnlockedMilestones;
        statsDict["bronze_milestones"] = _data.Statistics.BronzeMilestones;
        statsDict["silver_milestones"] = _data.Statistics.SilverMilestones;
        statsDict["gold_milestones"] = _data.Statistics.GoldMilestones;
        statsDict["platinum_milestones"] = _data.Statistics.PlatinumMilestones;
        statsDict["diamond_milestones"] = _data.Statistics.DiamondMilestones;
        statsDict["legendary_milestones"] = _data.Statistics.LegendaryMilestones;
        statsDict["total_rewards_claimed"] = _data.Statistics.TotalRewardsClaimed;
        statsDict["total_gold_earned"] = _data.Statistics.TotalGoldEarned;
        statsDict["total_exp_earned"] = _data.Statistics.TotalExpEarned;
        milestoneData["statistics"] = statsDict;

        data["milestones"] = milestoneData;
        saveSystem.SaveGame(data);
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
    
    /// <summary>
    /// Export save data (BaseSystem override)
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        data["milestones"] = _data.Milestones;
        data["categoryProgress"] = _data.CategoryProgress;
        data["statistics"] = _data.Statistics;
        return data;
    }
    
    /// <summary>
    /// Import save data (BaseSystem override)
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("milestones"))
        {
            _data.Milestones = (Dictionary<string, MilestoneData.MilestoneEntry>)data["milestones"];
        }
        if (data.Contains("categoryProgress"))
        {
            _data.CategoryProgress = (Dictionary<string, int>)data["categoryProgress"];
        }
        if (data.Contains("statistics"))
        {
            _data.Statistics = (MilestoneData.MilestoneStatistics)data["statistics"];
        }
    }
}
