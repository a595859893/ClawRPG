using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物成就系统
/// 管理宠物成就的解锁、进度追踪和奖励发放
/// </summary>
public partial class PetAchievementSystem : BaseSystem
{
    private static PetAchievementSystem _instance;
    public static PetAchievementSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                // Try to get from scene tree first (Godot way)
                var nodes = GetTree().GetNodesInGroup("systems");
                foreach (var node in nodes)
                {
                    if (node is PetAchievementSystem system)
                    {
                        _instance = system;
                        break;
                    }
                }
                
                // Fallback: create new instance (shouldn't happen in normal use)
                if (_instance == null)
                {
                    GD.PrintErr("[PetAchievementSystem] Instance not found in scene tree!");
                }
            }
            return _instance;
        }
    }
    
    public PetAchievementData Data { get; private set; }
    public PetAchievementDatabase Database { get; private set; }
    
    // Pet tracking counters (would be connected to actual game data)
    private Dictionary<string, int> _battleWins = new Dictionary<string, int>();
    private Dictionary<string, int> _enemyKills = new Dictionary<string, int>();
    private Dictionary<string, int> _bossKills = new Dictionary<string, int>();
    private Dictionary<string, int> _locationsVisited = new Dictionary<string, int>();
    private Dictionary<string, int> _floorReached = new Dictionary<string, int>();
    private Dictionary<string, int> _goldEarned = new Dictionary<string, int>();
    private Dictionary<string, int> _petLevel = new Dictionary<string, int>();
    private Dictionary<string, int> _itemsCollected = new Dictionary<string, int>();
    private Dictionary<string, int> _criticalHits = new Dictionary<string, int>();
    private Dictionary<string, int> _maxCombo = new Dictionary<string, int>();
    private Dictionary<string, int> _perfectBattles = new Dictionary<string, int>();
    private Dictionary<string, int> _survivalWins = new Dictionary<string, int>();
    private Dictionary<string, int> _noDamageWins = new Dictionary<string, int>();
    
    public event Action<string, PetAchievementData.Achievement> OnAchievementUnlocked;
    
    /// <summary>
    /// 初始化系统
    /// </summary>
    protected override void Initialize()
    {
        base.Initialize();
        
        Data = new PetAchievementData();
        Database = new PetAchievementDatabase();
        InitializeAchievements();
        
        AddToGroup("systems");
        GD.Print($"[PetAchievementSystem] Initialized");
    }
    
    private void InitializeAchievements()
    {
        // Initialize global achievements from database
        foreach (var achievementDef in Database.AllAchievements)
        {
            var achievement = new PetAchievementData.Achievement
            {
                Id = achievementDef.Id,
                Name = achievementDef.Name,
                Description = achievementDef.Description,
                Type = achievementDef.Type,
                Rarity = achievementDef.Rarity,
                RequiredValue = achievementDef.RequiredValue,
                CurrentValue = 0,
                IsUnlocked = false,
                UnlockedAt = null
            };
            Data.GlobalAchievements.Add(achievement);
            Data.TotalAchievements++;
        }
    }
    
    // Initialize achievements for a specific pet
    public void InitializePetAchievements(string petId)
    {
        if (!Data.PetAchievements.ContainsKey(petId))
        {
            Data.PetAchievements[petId] = new List<PetAchievementData.Achievement>();
            
            foreach (var achievementDef in Database.AllAchievements)
            {
                var achievement = new PetAchievementData.Achievement
                {
                    Id = achievementDef.Id,
                    Name = achievementDef.Name,
                    Description = achievementDef.Description,
                    Type = achievementDef.Type,
                    Rarity = achievementDef.Rarity,
                    RequiredValue = achievementDef.RequiredValue,
                    CurrentValue = 0,
                    IsUnlocked = false,
                    UnlockedAt = null
                };
                Data.PetAchievements[petId].Add(achievement);
                Data.TotalAchievements++;
            }
        }
        
        // Initialize counters
        if (!_battleWins.ContainsKey(petId)) _battleWins[petId] = 0;
        if (!_enemyKills.ContainsKey(petId)) _enemyKills[petId] = 0;
        if (!_bossKills.ContainsKey(petId)) _bossKills[petId] = 0;
        if (!_locationsVisited.ContainsKey(petId)) _locationsVisited[petId] = 0;
        if (!_floorReached.ContainsKey(petId)) _floorReached[petId] = 0;
        if (!_goldEarned.ContainsKey(petId)) _goldEarned[petId] = 0;
        if (!_petLevel.ContainsKey(petId)) _petLevel[petId] = 1;
        if (!_itemsCollected.ContainsKey(petId)) _itemsCollected[petId] = 0;
        if (!_criticalHits.ContainsKey(petId)) _criticalHits[petId] = 0;
        if (!_maxCombo.ContainsKey(petId)) _maxCombo[petId] = 0;
        if (!_perfectBattles.ContainsKey(petId)) _perfectBattles[petId] = 0;
        if (!_survivalWins.ContainsKey(petId)) _survivalWins[petId] = 0;
        if (!_noDamageWins.ContainsKey(petId)) _noDamageWins[petId] = 0;
    }
    
    // Update achievement progress
    public void UpdateProgress(string petId, string achievementId, int newValue)
    {
        InitializePetAchievements(petId);
        
        var achievements = Data.PetAchievements[petId];
        foreach (var achievement in achievements)
        {
            if (achievement.Id == achievementId && !achievement.IsUnlocked)
            {
                achievement.CurrentValue = newValue;
                
                if (achievement.CurrentValue >= achievement.RequiredValue)
                {
                    UnlockAchievement(petId, achievement);
                }
                break;
            }
        }
    }
    
    // Increment achievement progress
    public void IncrementProgress(string petId, string achievementId, int amount = 1)
    {
        InitializePetAchievements(petId);
        
        var achievements = Data.PetAchievements[petId];
        foreach (var achievement in achievements)
        {
            if (achievement.Id == achievementId && !achievement.IsUnlocked)
            {
                achievement.CurrentValue += amount;
                
                if (achievement.CurrentValue >= achievement.RequiredValue)
                {
                    UnlockAchievement(petId, achievement);
                }
                break;
            }
        }
    }
    
    // Unlock achievement
    private void UnlockAchievement(string petId, PetAchievementData.Achievement achievement)
    {
        achievement.IsUnlocked = true;
        achievement.UnlockedAt = DateTime.Now;
        
        Data.TotalAchievementsUnlocked++;
        if (Data.RarityBreakdown.ContainsKey(achievement.Rarity))
        {
            Data.RarityBreakdown[achievement.Rarity]++;
        }
        
        // Get reward
        var def = Database.GetAchievement(achievement.Id);
        if (def != null)
        {
            Data.TotalGoldEarned += def.GoldReward;
            // Gold would be added to player's wallet in actual implementation
        }
        
        OnAchievementUnlocked?.Invoke(petId, achievement);
    }
    
    // Convenience methods for common achievement updates
    public void RecordBattleVictory(string petId)
    {
        InitializePetAchievements(petId);
        
        int wins = ++_battleWins[petId];
        UpdateProgress(petId, "battle_first_victory", 1);
        UpdateProgress(petId, "battle_10_victories", wins);
        UpdateProgress(petId, "battle_50_victories", wins);
        UpdateProgress(petId, "battle_100_victories", wins);
        UpdateProgress(petId, "battle_500_victories", wins);
        UpdateProgress(petId, "battle_1000_victories", wins);
    }
    
    public void RecordEnemyKill(string petId)
    {
        InitializePetAchievements(petId);
        
        int kills = ++_enemyKills[petId];
        UpdateProgress(petId, "kills_10_enemies", kills);
        UpdateProgress(petId, "kills_50_enemies", kills);
        UpdateProgress(petId, "kills_100_enemies", kills);
        UpdateProgress(petId, "kills_500_enemies", kills);
    }
    
    public void RecordBossKill(string petId)
    {
        InitializePetAchievements(petId);
        
        int kills = ++_bossKills[petId];
        UpdateProgress(petId, "boss_first_kill", 1);
        UpdateProgress(petId, "boss_5_kills", kills);
        UpdateProgress(petId, "boss_25_kills", kills);
    }
    
    public void RecordLocationVisit(string petId)
    {
        InitializePetAchievements(petId);
        
        int locations = ++_locationsVisited[petId];
        UpdateProgress(petId, "explore_first_location", 1);
        UpdateProgress(petId, "explore_5_locations", locations);
        UpdateProgress(petId, "explore_10_locations", locations);
        UpdateProgress(petId, "explore_20_locations", locations);
    }
    
    public void RecordFloorReached(string petId, int floor)
    {
        InitializePetAchievements(petId);
        
        if (floor > _floorReached[petId])
        {
            _floorReached[petId] = floor;
            UpdateProgress(petId, "floor_5_reached", floor);
            UpdateProgress(petId, "floor_10_reached", floor);
            UpdateProgress(petId, "floor_25_reached", floor);
            UpdateProgress(petId, "floor_50_reached", floor);
        }
    }
    
    public void RecordGoldEarned(string petId, int gold)
    {
        InitializePetAchievements(petId);
        
        _goldEarned[petId] += gold;
        UpdateProgress(petId, "gold_100_earned", _goldEarned[petId]);
        UpdateProgress(petId, "gold_1000_earned", _goldEarned[petId]);
        UpdateProgress(petId, "gold_10000_earned", _goldEarned[petId]);
        UpdateProgress(petId, "gold_100000_earned", _goldEarned[petId]);
    }
    
    public void RecordLevelUp(string petId, int level)
    {
        InitializePetAchievements(petId);
        
        _petLevel[petId] = level;
        UpdateProgress(petId, "level_5_reached", level);
        UpdateProgress(petId, "level_10_reached", level);
        UpdateProgress(petId, "level_25_reached", level);
        UpdateProgress(petId, "level_50_reached", level);
        UpdateProgress(petId, "level_100_reached", level);
    }
    
    public void RecordItemCollected(string petId)
    {
        InitializePetAchievements(petId);
        
        int items = ++_itemsCollected[petId];
        UpdateProgress(petId, "collect_first_item", 1);
        UpdateProgress(petId, "collect_10_items", items);
        UpdateProgress(petId, "collect_50_items", items);
        UpdateProgress(petId, "collect_100_items", items);
    }
    
    public void RecordCriticalHit(string petId)
    {
        InitializePetAchievements(petId);
        
        int crits = ++_criticalHits[petId];
        UpdateProgress(petId, "crit_10_times", crits);
        UpdateProgress(petId, "crit_100_times", crits);
        UpdateProgress(petId, "crit_500_times", crits);
    }
    
    public void RecordCombo(string petId, int combo)
    {
        InitializePetAchievements(petId);
        
        if (combo > _maxCombo[petId])
        {
            _maxCombo[petId] = combo;
            UpdateProgress(petId, "combo_5", combo);
            UpdateProgress(petId, "combo_10", combo);
            UpdateProgress(petId, "combo_25", combo);
        }
    }
    
    public void RecordPerfectBattle(string petId)
    {
        InitializePetAchievements(petId);
        
        UpdateProgress(petId, "perfect_battle", 1);
    }
    
    public void RecordSurvivalWin(string petId)
    {
        InitializePetAchievements(petId);
        
        UpdateProgress(petId, "survival_expert", 1);
    }
    
    public void RecordNoDamageWin(string petId)
    {
        InitializePetAchievements(petId);
        
        int wins = ++_noDamageWins[petId];
        UpdateProgress(petId, "no_damage_run", wins);
    }
    
    public void RecordEvolution(string petId, int evolutionCount, bool isLegendary)
    {
        InitializePetAchievements(petId);
        
        UpdateProgress(petId, "evolve_first", evolutionCount);
        UpdateProgress(petId, "evolve_3_times", evolutionCount);
        
        if (isLegendary)
        {
            UpdateProgress(petId, "evolve_legendary", 1);
        }
    }
    
    // Get achievements for a pet
    public List<PetAchievementData.Achievement> GetPetAchievements(string petId)
    {
        InitializePetAchievements(petId);
        return Data.PetAchievements[petId];
    }
    
    // Get unlocked achievements
    public List<PetAchievementData.Achievement> GetUnlockedAchievements(string petId)
    {
        InitializePetAchievements(petId);
        
        var result = new List<PetAchievementData.Achievement>();
        foreach (var achievement in Data.PetAchievements[petId])
        {
            if (achievement.IsUnlocked)
                result.Add(achievement);
        }
        return result;
    }
    
    // Get achievement progress percentage
    public float GetProgressPercentage(string petId)
    {
        InitializePetAchievements(petId);
        
        int total = Data.PetAchievements[petId].Count;
        if (total == 0) return 0;
        
        int unlocked = 0;
        foreach (var achievement in Data.PetAchievements[petId])
        {
            if (achievement.IsUnlocked) unlocked++;
        }
        
        return (float)unlocked / total * 100f;
    }
    
    // Get statistics
    public PetAchievementData GetStatistics()
    {
        return Data;
    }
    
    // Check if achievement is unlocked
    public bool IsAchievementUnlocked(string petId, string achievementId)
    {
        InitializePetAchievements(petId);
        
        foreach (var achievement in Data.PetAchievements[petId])
        {
            if (achievement.Id == achievementId)
                return achievement.IsUnlocked;
        }
        return false;
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // Export pet achievements
        var petAchievementsData = new Dictionary<string, Dictionary[]>();
        foreach (var kvp in Data.PetAchievements)
        {
            var achievementsList = new List<Dictionary>();
            foreach (var achievement in kvp.Value)
            {
                achievementsList.Add(AchievementToDict(achievement));
            }
            petAchievementsData[kvp.Key] = achievementsList.ToArray();
        }
        data["pet_achievements"] = petAchievementsData;
        
        // Export global achievements
        var globalAchievementsList = new List<Dictionary>();
        foreach (var achievement in Data.GlobalAchievements)
        {
            globalAchievementsList.Add(AchievementToDict(achievement));
        }
        data["global_achievements"] = globalAchievementsList.ToArray();
        
        // Export statistics
        data["total_unlocked"] = Data.TotalAchievementsUnlocked;
        data["total_achievements"] = Data.TotalAchievements;
        data["total_rewards_claimed"] = Data.TotalRewardsClaimed;
        data["total_gold_earned"] = Data.TotalGoldEarned;
        
        // Export rarity breakdown
        var rarityBreakdown = new Dictionary<string, int>();
        foreach (var kvp in Data.RarityBreakdown)
        {
            rarityBreakdown[kvp.Key.ToString()] = kvp.Value;
        }
        data["rarity_breakdown"] = rarityBreakdown;
        
        // Export tracking counters
        data["battle_wins"] = ConvertDictToArray(_battleWins);
        data["enemy_kills"] = ConvertDictToArray(_enemyKills);
        data["boss_kills"] = ConvertDictToArray(_bossKills);
        data["locations_visited"] = ConvertDictToArray(_locationsVisited);
        data["floor_reached"] = ConvertDictToArray(_floorReached);
        data["gold_earned"] = ConvertDictToArray(_goldEarned);
        data["pet_level"] = ConvertDictToArray(_petLevel);
        data["items_collected"] = ConvertDictToArray(_itemsCollected);
        data["critical_hits"] = ConvertDictToArray(_criticalHits);
        data["max_combo"] = ConvertDictToArray(_maxCombo);
        data["perfect_battles"] = ConvertDictToArray(_perfectBattles);
        data["survival_wins"] = ConvertDictToArray(_survivalWins);
        data["no_damage_wins"] = ConvertDictToArray(_noDamageWins);
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // Import pet achievements
        if (data.Contains("pet_achievements") && data["pet_achievements"] is Dictionary petAchievementsData)
        {
            Data.PetAchievements = new Dictionary<string, List<PetAchievementData.Achievement>>();
            foreach (var kvp in petAchievementsData)
            {
                var achievementsList = new List<PetAchievementData.Achievement>();
                if (kvp.Value is Dictionary[] achievementsArray)
                {
                    foreach (var achievementDict in achievementsArray)
                    {
                        achievementsList.Add(DictToAchievement(achievementDict));
                    }
                }
                Data.PetAchievements[kvp.Key.ToString()] = achievementsList;
            }
        }
        
        // Import global achievements
        if (data.Contains("global_achievements") && data["global_achievements"] is Dictionary[] globalArray)
        {
            Data.GlobalAchievements = new List<PetAchievementData.Achievement>();
            foreach (var achievementDict in globalArray)
            {
                Data.GlobalAchievements.Add(DictToAchievement(achievementDict));
            }
        }
        
        // Import statistics
        if (data.Contains("total_unlocked"))
            Data.TotalAchievementsUnlocked = Convert.ToInt32(data["total_unlocked"]);
        if (data.Contains("total_achievements"))
            Data.TotalAchievements = Convert.ToInt32(data["total_achievements"]);
        if (data.Contains("total_rewards_claimed"))
            Data.TotalRewardsClaimed = Convert.ToInt32(data["total_rewards_claimed"]);
        if (data.Contains("total_gold_earned"))
            Data.TotalGoldEarned = Convert.ToInt32(data["total_gold_earned"]);
        
        // Import rarity breakdown
        if (data.Contains("rarity_breakdown") && data["rarity_breakdown"] is Dictionary rarityDict)
        {
            Data.RarityBreakdown = new Dictionary<PetAchievementData.AchievementRarity, int>();
            foreach (PetAchievementData.AchievementRarity rarity in Enum.GetValues(typeof(PetAchievementData.AchievementRarity)))
            {
                if (rarityDict.ContainsKey(rarity.ToString()))
                    Data.RarityBreakdown[rarity] = Convert.ToInt32(rarityDict[rarity.ToString()]);
                else
                    Data.RarityBreakdown[rarity] = 0;
            }
        }
        
        // Import tracking counters
        _battleWins = ConvertArrayToDict(data, "battle_wins");
        _enemyKills = ConvertArrayToDict(data, "enemy_kills");
        _bossKills = ConvertArrayToDict(data, "boss_kills");
        _locationsVisited = ConvertArrayToDict(data, "locations_visited");
        _floorReached = ConvertArrayToDict(data, "floor_reached");
        _goldEarned = ConvertArrayToDict(data, "gold_earned");
        _petLevel = ConvertArrayToDict(data, "pet_level");
        _itemsCollected = ConvertArrayToDict(data, "items_collected");
        _criticalHits = ConvertArrayToDict(data, "critical_hits");
        _maxCombo = ConvertArrayToDict(data, "max_combo");
        _perfectBattles = ConvertArrayToDict(data, "perfect_battles");
        _survivalWins = ConvertArrayToDict(data, "survival_wins");
        _noDamageWins = ConvertArrayToDict(data, "no_damage_wins");
        
        GD.Print($"[PetAchievementSystem] Loaded save data");
    }
    
    /// <summary>
    /// 重置系统数据
    /// </summary>
    public override void Reset()
    {
        base.Reset();
        
        Data = new PetAchievementData();
        _battleWins.Clear();
        _enemyKills.Clear();
        _bossKills.Clear();
        _locationsVisited.Clear();
        _floorReached.Clear();
        _goldEarned.Clear();
        _petLevel.Clear();
        _itemsCollected.Clear();
        _criticalHits.Clear();
        _maxCombo.Clear();
        _perfectBattles.Clear();
        _survivalWins.Clear();
        _noDamageWins.Clear();
        
        InitializeAchievements();
    }
    
    // Helper: Convert achievement to dictionary
    private Dictionary AchievementToDict(PetAchievementData.Achievement achievement)
    {
        var dict = new Dictionary<string, object>();
        dict["id"] = achievement.Id;
        dict["name"] = achievement.Name;
        dict["description"] = achievement.Description;
        dict["type"] = achievement.Type.ToString();
        dict["rarity"] = achievement.Rarity.ToString();
        dict["required_value"] = achievement.RequiredValue;
        dict["current_value"] = achievement.CurrentValue;
        dict["is_unlocked"] = achievement.IsUnlocked;
        dict["unlocked_at"] = achievement.UnlockedAt?.ToString("o") ?? "";
        return dict;
    }
    
    // Helper: Convert dictionary to achievement
    private PetAchievementData.Achievement DictToAchievement(Dictionary dict)
    {
        var achievement = new PetAchievementData.Achievement();
        achievement.Id = dict.Contains("id") ? dict["id"].ToString() : "";
        achievement.Name = dict.Contains("name") ? dict["name"].ToString() : "";
        achievement.Description = dict.Contains("description") ? dict["description"].ToString() : "";
        
        if (dict.Contains("type") && Enum.TryParse<PetAchievementData.AchievementType>(dict["type"].ToString(), out var type))
            achievement.Type = type;
        if (dict.Contains("rarity") && Enum.TryParse<PetAchievementData.AchievementRarity>(dict["rarity"].ToString(), out var rarity))
            achievement.Rarity = rarity;
        
        achievement.RequiredValue = dict.Contains("required_value") ? Convert.ToInt32(dict["required_value"]) : 0;
        achievement.CurrentValue = dict.Contains("current_value") ? Convert.ToInt32(dict["current_value"]) : 0;
        achievement.IsUnlocked = dict.Contains("is_unlocked") && Convert.ToBoolean(dict["is_unlocked"]);
        
        if (dict.Contains("unlocked_at") && !string.IsNullOrEmpty(dict["unlocked_at"].ToString()))
            achievement.UnlockedAt = DateTime.Parse(dict["unlocked_at"].ToString());
        
        return achievement;
    }
    
    // Helper: Convert Dictionary<string, int> to Godot Dictionary array
    private Godot.Collections.Array ConvertDictToArray(Dictionary<string, int> dict)
    {
        var array = new Godot.Collections.Array();
        foreach (var kvp in dict)
        {
            var item = new Dictionary<string, object>();
            item["key"] = kvp.Key;
            item["value"] = kvp.Value;
            array.Add(item);
        }
        return array;
    }
    
    // Helper: Convert Godot Dictionary array to Dictionary<string, int>
    private Dictionary<string, int> ConvertArrayToDict(Dictionary data, string key)
    {
        var result = new Dictionary<string, int>();
        if (data.Contains(key) && data[key] is Godot.Collections.Array array)
        {
            foreach (var item in array)
            {
                if (item is Dictionary dict && dict.Contains("key") && dict.Contains("value"))
                {
                    result[dict["key"].ToString()] = Convert.ToInt32(dict["value"]);
                }
            }
        }
        return result;
    }
}
