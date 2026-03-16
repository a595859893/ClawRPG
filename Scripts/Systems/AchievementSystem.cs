using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

/// <summary>
/// Achievement system - manages player achievements, tracks statistics, and handles unlocking
/// </summary>
public class AchievementSystem : BaseSystem
{
    public static AchievementSystem Instance { get; private set; }

    private Dictionary<string, AchievementData.Achievement> _achievements = new Dictionary<string, AchievementData.Achievement>();
    private Dictionary<AchievementData.AchievementCategory, List<string>> _categoryAchievements = new Dictionary<AchievementData.AchievementCategory, List<string>>();

    // Stats tracking
    private int _totalKills;
    private int _bossKills;
    private int _pvpWins;
    private int _zonesDiscovered;
    private int _sealedTowerFloor;
    private int _petsCollected;
    private int _mountsCollected;
    private int _equipmentCollected;
    private int _friendsMade;
    private int _goldAccumulated;
    private int _goldSpent;
    private int _skillPointsSpent;
    private int _itemsCrafted;
    private int _loginStreak;
    private float _playTimeHours;
    
    // Quick Mode Stats
    private int _quickModeWins;
    private int _quickModePlays;
    private int _quickModeStreak;
    private int _quickModeSpeedRuns;
    private int _quickModePerfectRuns;

    // Signals
    [Signal] public delegate void AchievementUnlocked(AchievementData.Achievement achievement);
    [Signal] public delegate void ProgressUpdated(string achievementId, int current, int required);
    [Signal] public delegate void CategoryCompleted(AchievementData.AchievementCategory category);

    public override void _Ready()
    {
        Instance = this;
        InitializeAchievements();
        LoadData();
    }
    
    /// <summary>
    /// System name for identification
    /// </summary>
    protected override string SystemName => "Achievement";

    private void InitializeAchievements()
    {
        // Initialize category mapping
        foreach (AchievementData.AchievementCategory cat in Enum.GetValues(typeof(AchievementData.AchievementCategory)))
        {
            _categoryAchievements[cat] = new List<string>();
        }

        // Load from database
        var dbAchievements = AchievementDatabase.GetAllAchievements();
        foreach (var achievement in dbAchievements)
        {
            _achievements[achievement.id] = achievement;
            _categoryAchievements[achievement.category].Add(achievement.id);
        }
    }

    /// <summary>
    /// Load achievement data from save system
    /// </summary>
    public void LoadData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) return;

        // Load achievement progress
        if (data.Contains("achievements"))
        {
            var achievementsArray = (Godot.Array)data["achievements"];
            foreach (Dictionary achievementData in achievementsArray)
            {
                string id = (string)achievementData["id"];
                if (_achievements.ContainsKey(id))
                {
                    _achievements[id].currentProgress = (int)achievementData["progress"];
                    _achievements[id].isUnlocked = (bool)achievementData["unlocked"];
                    if (achievementData.Contains("unlocked_at"))
                    {
                        _achievements[id].unlockedAt = DateTime.Parse((string)achievementData["unlocked_at"]);
                    }
                }
            }
        }

        // Load stats
        if (data.Contains("achievement_stats"))
        {
            var stats = (Godot.Dictionary)data["achievement_stats"];
            _totalKills = (int)stats.Get("total_kills", 0);
            _bossKills = (int)stats.Get("boss_kills", 0);
            _pvpWins = (int)stats.Get("pvp_wins", 0);
            _zonesDiscovered = (int)stats.Get("zones_discovered", 0);
            _sealedTowerFloor = (int)stats.Get("sealed_tower_floor", 0);
            _petsCollected = (int)stats.Get("pets_collected", 0);
            _mountsCollected = (int)stats.Get("mounts_collected", 0);
            _equipmentCollected = (int)stats.Get("equipment_collected", 0);
            _friendsMade = (int)stats.Get("friends_made", 0);
            _goldAccumulated = (int)stats.Get("gold_accumulated", 0);
            _goldSpent = (int)stats.Get("gold_spent", 0);
            _skillPointsSpent = (int)stats.Get("skill_points_spent", 0);
            _itemsCrafted = (int)stats.Get("items_crafted", 0);
            _loginStreak = (int)stats.Get("login_streak", 0);
            _playTimeHours = (float)stats.Get("playtime_hours", 0.0);
        }
    }

    /// <summary>
    /// Save achievement data to save system
    /// </summary>
    public void SaveData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Dictionary();

        // Save achievement progress
        var achievementsArray = new Godot.Array();
        foreach (var achievement in _achievements.Values)
        {
            var achievementData = new Godot.Dictionary();
            achievementData["id"] = achievement.id;
            achievementData["progress"] = achievement.currentProgress;
            achievementData["unlocked"] = achievement.isUnlocked;
            if (achievement.unlockedAt.HasValue)
            {
                achievementData["unlocked_at"] = achievement.unlockedAt.Value.ToString("o");
            }
            achievementsArray.Add(achievementData);
        }
        data["achievements"] = achievementsArray;

        // Save stats
        var stats = new Godot.Dictionary();
        stats["total_kills"] = _totalKills;
        stats["boss_kills"] = _bossKills;
        stats["pvp_wins"] = _pvpWins;
        stats["zones_discovered"] = _zonesDiscovered;
        stats["sealed_tower_floor"] = _sealedTowerFloor;
        stats["pets_collected"] = _petsCollected;
        stats["mounts_collected"] = _mountsCollected;
        stats["equipment_collected"] = _equipmentCollected;
        stats["friends_made"] = _friendsMade;
        stats["gold_accumulated"] = _goldAccumulated;
        stats["gold_spent"] = _goldSpent;
        stats["skill_points_spent"] = _skillPointsSpent;
        stats["items_crafted"] = _itemsCrafted;
        stats["login_streak"] = _loginStreak;
        stats["playtime_hours"] = _playTimeHours;
        data["achievement_stats"] = stats;

        saveSystem.SaveGame(data);
    }

    // Progress tracking methods

    /// <summary>
    /// Record a kill event
    /// </summary>
    /// <param name="isBoss">Whether the killed entity is a boss</param>
    public void AddKill(bool isBoss = false)
    {
        _totalKills++;
        if (isBoss) _bossKills++;

        UpdateAchievementProgress("combat_kills_100", _totalKills);
        UpdateAchievementProgress("combat_kills_500", _totalKills);
        UpdateAchievementProgress("combat_kills_1000", _totalKills);
        UpdateAchievementProgress("combat_kills_5000", _totalKills);
        UpdateAchievementProgress("combat_kills_10000", _totalKills);

        if (isBoss)
        {
            UpdateAchievementProgress("boss_kills_10", _bossKills);
            UpdateAchievementProgress("boss_kills_50", _bossKills);
            UpdateAchievementProgress("boss_kills_100", _bossKills);
        }

        SaveData();
    }

    /// <summary>
    /// Record a PVP victory
    /// </summary>
    public void AddPvpWin()
    {
        _pvpWins++;
        UpdateAchievementProgress("pvp_wins_10", _pvpWins);
        UpdateAchievementProgress("pvp_wins_50", _pvpWins);
        UpdateAchievementProgress("pvp_wins_100", _pvpWins);
        UpdateAchievementProgress("pvp_wins_500", _pvpWins);
        SaveData();
    }

    /// <summary>
    /// Record discovery of a new zone
    /// </summary>
    /// <param name="zoneCount">Total number of zones discovered</param>
    public void DiscoverZone(int zoneCount)
    {
        _zonesDiscovered = zoneCount;
        UpdateAchievementProgress("explore_zones_5", _zonesDiscovered);
        UpdateAchievementProgress("explore_zones_10", _zonesDiscovered);
        UpdateAchievementProgress("explore_zones_20", _zonesDiscovered);
        UpdateAchievementProgress("explore_zones_all", _zonesDiscovered);
        SaveData();
    }

    /// <summary>
    /// Update sealed tower progress
    /// </summary>
    /// <param name="floor">Current floor reached</param>
    public void UpdateSealedTower(int floor)
    {
        _sealedTowerFloor = floor;
        UpdateAchievementProgress("sealed_tower_10", _sealedTowerFloor);
        UpdateAchievementProgress("sealed_tower_50", _sealedTowerFloor);
        UpdateAchievementProgress("sealed_tower_100", _sealedTowerFloor);
        SaveData();
    }

    /// <summary>
    /// Add a pet to collection
    /// </summary>
    public void AddPet()
    {
        _petsCollected++;
        UpdateAchievementProgress("pets_5", _petsCollected);
        UpdateAchievementProgress("pets_10", _petsCollected);
        UpdateAchievementProgress("pets_all", _petsCollected);
        SaveData();
    }

    /// <summary>
    /// Add a mount to collection
    /// </summary>
    public void AddMount()
    {
        _mountsCollected++;
        UpdateAchievementProgress("mounts_3", _mountsCollected);
        UpdateAchievementProgress("mounts_8", _mountsCollected);
        UpdateAchievementProgress("mounts_all", _mountsCollected);
        SaveData();
    }

    /// <summary>
    /// Add equipment to collection
    /// </summary>
    /// <param name="count">Number of equipment to add</param>
    public void AddEquipment(int count = 1)
    {
        _equipmentCollected += count;
        UpdateAchievementProgress("equipment_50", _equipmentCollected);
        UpdateAchievementProgress("equipment_200", _equipmentCollected);
        UpdateAchievementProgress("equipment_500", _equipmentCollected);
        SaveData();
    }

    /// <summary>
    /// Add a friend
    /// </summary>
    public void AddFriend()
    {
        _friendsMade++;
        UpdateAchievementProgress("friends_10", _friendsMade);
        UpdateAchievementProgress("friends_50", _friendsMade);
        SaveData();
    }

    /// <summary>
    /// Update accumulated gold amount
    /// </summary>
    /// <param name="currentGold">Current gold amount</param>
    public void UpdateGold(int currentGold)
    {
        if (currentGold > _goldAccumulated)
        {
            _goldAccumulated = currentGold;
            UpdateAchievementProgress("gold_10000", _goldAccumulated);
            UpdateAchievementProgress("gold_100000", _goldAccumulated);
            UpdateAchievementProgress("gold_1000000", _goldAccumulated);
            UpdateAchievementProgress("gold_10000000", _goldAccumulated);
            SaveData();
        }
    }

    /// <summary>
    /// Add gold spent amount
    /// </summary>
    /// <param name="amount">Amount of gold spent</param>
    public void AddGoldSpent(int amount)
    {
        _goldSpent += amount;
        UpdateAchievementProgress("spend_50000", _goldSpent);
        UpdateAchievementProgress("spend_500000", _goldSpent);
        SaveData();
    }

    /// <summary>
    /// Add skill points spent
    /// </summary>
    /// <param name="points">Number of skill points spent</param>
    public void AddSkillPointsSpent(int points)
    {
        _skillPointsSpent += points;
        UpdateAchievementProgress("skill_points_50", _skillPointsSpent);
        UpdateAchievementProgress("skill_points_200", _skillPointsSpent);
        SaveData();
    }

    /// <summary>
    /// Add a crafted item to the count
    /// </summary>
    public void AddCraftedItem()
    {
        _itemsCrafted++;
        UpdateAchievementProgress("craft_10", _itemsCrafted);
        UpdateAchievementProgress("craft_100", _itemsCrafted);
        UpdateAchievementProgress("craft_500", _itemsCrafted);
        SaveData();
    }

    /// <summary>
    /// Update login streak count
    /// </summary>
    /// <param name="streak">Number of consecutive days logged in</param>
    public void UpdateLoginStreak(int streak)
    {
        _loginStreak = streak;
        UpdateAchievementProgress("login_7", _loginStreak);
        UpdateAchievementProgress("login_30", _loginStreak);
        SaveData();
    }

    /// <summary>
    /// Update total play time
    /// </summary>
    /// <param name="hours">Total hours played</param>
    public void UpdatePlayTime(float hours)
    {
        _playTimeHours = hours;
        UpdateAchievementProgress("playtime_1h", (int)_playTimeHours);
        UpdateAchievementProgress("playtime_10h", (int)_playTimeHours);
        UpdateAchievementProgress("playtime_50h", (int)_playTimeHours);
        UpdateAchievementProgress("playtime_100h", (int)_playTimeHours);
        SaveData();
    }

    /// <summary>
    /// Update player level
    /// </summary>
    /// <param name="level">Current player level</param>
    public void UpdateLevel(int level)
    {
        UpdateAchievementProgress("level_10", level);
        UpdateAchievementProgress("level_50", level);
        UpdateAchievementProgress("level_100", level);
        UpdateAchievementProgress("level_200", level);
        SaveData();
    }

    /// <summary>
    /// Set guild joined status
    /// </summary>
    /// <param name="isLeader">Whether player is the guild leader</param>
    public void SetGuildJoined(bool isLeader = false)
    {
        UpdateAchievementProgress("guild_join", 1);
        if (isLeader)
        {
            UpdateAchievementProgress("guild_leader", 1);
        }
        SaveData();
    }

    /// <summary>
    /// Set first battle completed
    /// </summary>
    public void SetFirstBattle()
    {
        UpdateAchievementProgress("first_blood", 1);
        SaveData();
    }

    // ============ Quick Mode Achievement Tracking ============
    
    /// <summary>
    /// Track quick mode win count
    /// </summary>
    /// <param name="wins">Total wins in quick mode</param>
    public void TrackQuickModeWin(int wins)
    {
        _quickModeWins = wins;
        
        // Update all quick mode win achievements
        UpdateAchievementProgress("quick_first_win", _quickModeWins);
        UpdateAchievementProgress("quick_wins_10", _quickModeWins);
        UpdateAchievementProgress("quick_wins_50", _quickModeWins);
        UpdateAchievementProgress("quick_wins_100", _quickModeWins);
        UpdateAchievementProgress("quick_wins_500", _quickModeWins);
        
        SaveData();
    }
    
    /// <summary>
    /// Track quick mode play count
    /// </summary>
    /// <param name="plays">Total plays in quick mode</param>
    public void TrackQuickModePlay(int plays)
    {
        _quickModePlays = plays;
        
        UpdateAchievementProgress("quick_plays_50", _quickModePlays);
        UpdateAchievementProgress("quick_plays_200", _quickModePlays);
        
        SaveData();
    }
    
    /// <summary>
    /// Track quick mode win streak
    /// </summary>
    /// <param name="streak">Current consecutive win streak</param>
    public void TrackQuickModeStreak(int streak)
    {
        _quickModeStreak = streak;
        
        UpdateAchievementProgress("quick_streak_5", _quickModeStreak);
        UpdateAchievementProgress("quick_streak_10", _quickModeStreak);
        UpdateAchievementProgress("quick_streak_25", _quickModeStreak);
        
        SaveData();
    }
    
    /// <summary>
    /// Track quick mode speed run completions
    /// </summary>
    /// <param name="speedRuns">Number of speed runs completed</param>
    public void TrackQuickModeSpeedRun(int speedRuns)
    {
        _quickModeSpeedRuns = speedRuns;
        
        UpdateAchievementProgress("quick_speed_5", _quickModeSpeedRuns);
        UpdateAchievementProgress("quick_speed_10", _quickModeSpeedRuns);
        UpdateAchievementProgress("quick_speed_25", _quickModeSpeedRuns);
        
        SaveData();
    }
    
    /// <summary>
    /// Track quick mode perfect run completions
    /// </summary>
    /// <param name="perfectRuns">Number of perfect runs (no damage taken)</param>
    public void TrackQuickModePerfectRun(int perfectRuns)
    {
        _quickModePerfectRuns = perfectRuns;
        
        UpdateAchievementProgress("quick_perfect_3", _quickModePerfectRuns);
        UpdateAchievementProgress("quick_perfect_10", _quickModePerfectRuns);
        
        SaveData();
    }

    private void UpdateAchievementProgress(string achievementId, int value)
    {
        if (!_achievements.ContainsKey(achievementId)) return;

        var achievement = _achievements[achievementId];
        if (achievement.isUnlocked) return;

        achievement.currentProgress = Mathf.Min(value, achievement.requirement);
        EmitSignal(nameof(ProgressUpdated), achievementId, achievement.currentProgress, achievement.requirement);

        if (achievement.currentProgress >= achievement.requirement)
        {
            UnlockAchievement(achievement);
        }
    }

    private void UnlockAchievement(AchievementData.Achievement achievement)
    {
        achievement.isUnlocked = true;
        achievement.unlockedAt = DateTime.Now;

        // Grant rewards
        var player = GetNode<Player>("/root/Player");
        if (player != null)
        {
            player.AddGold(achievement.rewardGold);
            player.AddExp(achievement.rewardExp);
        }

        EmitSignal(nameof(AchievementUnlocked), achievement);
        GD.Print($"Achievement Unlocked: {achievement.name}! Reward: {achievement.rewardGold} gold, {achievement.rewardExp} exp");

        // Check if category is completed
        CheckCategoryCompletion(achievement.category);
    }

    private void CheckCategoryCompletion(AchievementData.AchievementCategory category)
    {
        var categoryIds = _categoryAchievements[category];
        bool allComplete = true;

        foreach (var id in categoryIds)
        {
            if (!_achievements[id].isUnlocked)
            {
                allComplete = false;
                break;
            }
        }

        if (allComplete)
        {
            EmitSignal(nameof(CategoryCompleted), category);
        }
    }

    // Public getters

    /// <summary>
    /// Get all achievements
    /// </summary>
    /// <returns>Dictionary of all achievements</returns>
    public Dictionary<string, AchievementData.Achievement> GetAllAchievements()
    {
        return _achievements;
    }

    /// <summary>
    /// Get achievements by category
    /// </summary>
    /// <param name="category">Achievement category</param>
    /// <returns>List of achievements in the category</returns>
    public List<AchievementData.Achievement> GetAchievementsByCategory(AchievementData.AchievementCategory category)
    {
        var result = new List<AchievementData.Achievement>();
        if (_categoryAchievements.ContainsKey(category))
        {
            foreach (var id in _categoryAchievements[category])
            {
                result.Add(_achievements[id]);
            }
        }
        return result;
    }

    /// <summary>
    /// Get list of unlocked achievements
    /// </summary>
    /// <returns>List of unlocked achievements</returns>
    public List<AchievementData.Achievement> GetUnlockedAchievements()
    {
        var result = new List<AchievementData.Achievement>();
        foreach (var achievement in _achievements.Values)
        {
            if (achievement.isUnlocked)
            {
                result.Add(achievement);
            }
        }
        return result;
    }

    /// <summary>
    /// Get count of unlocked achievements
    /// </summary>
    /// <returns>Number of unlocked achievements</returns>
    public int GetUnlockedCount()
    {
        int count = 0;
        foreach (var achievement in _achievements.Values)
        {
            if (achievement.isUnlocked) count++;
        }
        return count;
    }

    /// <summary>
    /// Get total number of achievements
    /// </summary>
    /// <returns>Total achievement count</returns>
    public int GetTotalAchievementCount()
    {
        return _achievements.Count;
    }

    /// <summary>
    /// Get total gold rewards from unlocked achievements
    /// </summary>
    /// <returns>Total gold rewards</returns>
    public int GetTotalRewardGold()
    {
        int total = 0;
        foreach (var achievement in _achievements.Values)
        {
            if (achievement.isUnlocked)
            {
                total += achievement.rewardGold;
            }
        }
        return total;
    }

    /// <summary>
    /// Get total experience rewards from unlocked achievements
    /// </summary>
    /// <returns>Total experience rewards</returns>
    public int GetTotalRewardExp()
    {
        int total = 0;
        foreach (var achievement in _achievements.Values)
        {
            if (achievement.isUnlocked)
            {
                total += achievement.rewardExp;
            }
        }
        return total;
    }

    // Stats getters
    public int GetTotalKills() => _totalKills;
    public int GetBossKills() => _bossKills;
    public int GetPvpWins() => _pvpWins;
    public int GetZonesDiscovered() => _zonesDiscovered;
    public int GetSealedTowerFloor() => _sealedTowerFloor;
    public int GetPetsCollected() => _petsCollected;
    public int GetMountsCollected() => _mountsCollected;
    public int GetEquipmentCollected() => _equipmentCollected;
    public int GetFriendsMade() => _friendsMade;
    public int GetGoldAccumulated() => _goldAccumulated;
    public int GetGoldSpent() => _goldSpent;
    public int GetSkillPointsSpent() => _skillPointsSpent;
    public int GetItemsCrafted() => _itemsCrafted;
    public int GetLoginStreak() => _loginStreak;
    public float GetPlayTimeHours() => _playTimeHours;

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 统计数据
        data["total_kills"] = _totalKills;
        data["boss_kills"] = _bossKills;
        data["pvp_wins"] = _pvpWins;
        data["zones_discovered"] = _zonesDiscovered;
        data["sealed_tower_floor"] = _sealedTowerFloor;
        data["pets_collected"] = _petsCollected;
        data["mounts_collected"] = _mountsCollected;
        data["equipment_collected"] = _equipmentCollected;
        data["friends_made"] = _friendsMade;
        data["gold_accumulated"] = _goldAccumulated;
        data["gold_spent"] = _goldSpent;
        data["skill_points_spent"] = _skillPointsSpent;
        data["items_crafted"] = _itemsCrafted;
        data["login_streak"] = _loginStreak;
        data["play_time_hours"] = _playTimeHours;
        
        // Quick Mode 统计数据
        data["quick_mode_wins"] = _quickModeWins;
        data["quick_mode_plays"] = _quickModePlays;
        data["quick_mode_streak"] = _quickModeStreak;
        data["quick_mode_speed_runs"] = _quickModeSpeedRuns;
        data["quick_mode_perfect_runs"] = _quickModePerfectRuns;
        
        // 已解锁成就
        var unlocked = new Array();
        foreach (var kvp in _achievements)
        {
            if (kvp.Value.isUnlocked)
            {
                unlocked.Add(kvp.Key);
            }
        }
        data["unlocked_achievements"] = unlocked;
        
        return data;
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("total_kills")) _totalKills = (int)data["total_kills"];
        if (data.Contains("boss_kills")) _bossKills = (int)data["boss_kills"];
        if (data.Contains("pvp_wins")) _pvpWins = (int)data["pvp_wins"];
        if (data.Contains("zones_discovered")) _zonesDiscovered = (int)data["zones_discovered"];
        if (data.Contains("sealed_tower_floor")) _sealedTowerFloor = (int)data["sealed_tower_floor"];
        if (data.Contains("pets_collected")) _petsCollected = (int)data["pets_collected"];
        if (data.Contains("mounts_collected")) _mountsCollected = (int)data["mounts_collected"];
        if (data.Contains("equipment_collected")) _equipmentCollected = (int)data["equipment_collected"];
        if (data.Contains("friends_made")) _friendsMade = (int)data["friends_made"];
        if (data.Contains("gold_accumulated")) _goldAccumulated = (int)data["gold_accumulated"];
        if (data.Contains("gold_spent")) _goldSpent = (int)data["gold_spent"];
        if (data.Contains("skill_points_spent")) _skillPointsSpent = (int)data["skill_points_spent"];
        if (data.Contains("items_crafted")) _itemsCrafted = (int)data["items_crafted"];
        if (data.Contains("login_streak")) _loginStreak = (int)data["login_streak"];
        if (data.Contains("play_time_hours")) _playTimeHours = (float)data["play_time_hours"];
        
        // Quick Mode 统计数据
        if (data.Contains("quick_mode_wins")) _quickModeWins = (int)data["quick_mode_wins"];
        if (data.Contains("quick_mode_plays")) _quickModePlays = (int)data["quick_mode_plays"];
        if (data.Contains("quick_mode_streak")) _quickModeStreak = (int)data["quick_mode_streak"];
        if (data.Contains("quick_mode_speed_runs")) _quickModeSpeedRuns = (int)data["quick_mode_speed_runs"];
        if (data.Contains("quick_mode_perfect_runs")) _quickModePerfectRuns = (int)data["quick_mode_perfect_runs"];
    }
}
