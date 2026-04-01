using Godot;
using System;
using System.Collections.Generic;

public partial class RoguelikeStatsSystem : BaseSystem
{
    private RoguelikeStatsData _data;
    private int _currentRunId = 0;
    private DateTime _runStartTime;
    private bool _runInProgress = false;
    
    // Current run tracking
    private int _currentEnemiesKilled = 0;
    private int _currentBossesKilled = 0;
    private int _currentDamageDealt = 0;
    private int _currentDamageTaken = 0;
    private int _currentGoldEarned = 0;
    private int _currentGoldSpent = 0;
    private int _currentItemsCollected = 0;
    private int _currentPetsObtained = 0;
    private List<string> _currentKeyEvents = new List<string>();
    private string _currentClass = "Warrior";
    private string _currentBuild = "Balanced";
    
    public RoguelikeStatsSystem()
    {
        _data = new RoguelikeStatsData();
    }
    
    // Start a new run
    public void StartRun(string characterClass, string buildType)
    {
        _runInProgress = true;
        _runStartTime = DateTime.Now;
        _currentRunId++;
        _currentEnemiesKilled = 0;
        _currentBossesKilled = 0;
        _currentDamageDealt = 0;
        _currentDamageTaken = 0;
        _currentGoldEarned = 0;
        _currentGoldSpent = 0;
        _currentItemsCollected = 0;
        _currentPetsObtained = 0;
        _currentKeyEvents.Clear();
        _currentClass = characterClass;
        _currentBuild = buildType;
        
        GD.Print($"[RoguelikeStats] Started run #{_currentRunId} as {characterClass} ({buildType})");
    }
    
    // End a run (victory)
    public void CompleteRunVictory(int floorReached)
    {
        if (!_runInProgress) return;
        
        var record = CreateRunRecord(floorReached, true, "");
        _data.RunHistory.Add(record);
        
        // Update statistics
        _data.TotalRuns++;
        _data.TotalWins++;
        _data.CurrentWinStreak++;
        if (_data.CurrentWinStreak > _data.BestWinStreak)
            _data.BestWinStreak = _data.CurrentWinStreak;
        _data.CurrentLossStreak = 0;
        
        // Update lifetime stats
        UpdateLifetimeStats(record);
        
        // Update most used class
        UpdateMostUsedClass(_currentClass);
        UpdateMostUsedBuild(_currentBuild);
        
        _runInProgress = false;
        GD.Print($"[RoguelikeStats] Run #{_currentRunId} completed! Reached floor {floorReached}");
    }
    
    // End a run (death)
    public void CompleteRunDeath(int floorReached, string deathCause)
    {
        if (!_runInProgress) return;
        
        var record = CreateRunRecord(floorReached, false, deathCause);
        _data.RunHistory.Add(record);
        
        // Update statistics
        _data.TotalRuns++;
        _data.TotalDeaths++;
        _data.CurrentLossStreak++;
        if (_data.CurrentLossStreak > _data.BestLossStreak)
            _data.BestLossStreak = _data.CurrentLossStreak;
        _data.CurrentWinStreak = 0;
        
        // Track death cause
        if (_data.DeathCauses.ContainsKey(deathCause))
            _data.DeathCauses[deathCause]++;
        else
            _data.DeathCauses[deathCause] = 1;
        
        // Update lifetime stats
        UpdateLifetimeStats(record);
        
        // Update most used class
        UpdateMostUsedClass(_currentClass);
        UpdateMostUsedBuild(_currentBuild);
        
        _runInProgress = false;
        GD.Print($"[RoguelikeStats] Run #{_currentRunId} ended in death at floor {floorReached}. Cause: {deathCause}");
    }
    
    private RoguelikeRunRecord CreateRunRecord(int floorReached, bool victory, string deathCause)
    {
        var duration = (int)(DateTime.Now - _runStartTime).TotalSeconds;
        int dps = duration > 0 ? _currentDamageDealt / duration : 0;
        
        return new RoguelikeRunRecord
        {
            RunId = _currentRunId,
            StartTime = _runStartTime,
            EndTime = DateTime.Now,
            Duration = duration,
            Victory = victory,
            FloorReached = floorReached,
            CharacterClass = _currentClass,
            BuildType = _currentBuild,
            EnemiesKilled = _currentEnemiesKilled,
            BossesKilled = _currentBossesKilled,
            DamageDealt = _currentDamageDealt,
            DamageTaken = _currentDamageTaken,
            GoldEarned = _currentGoldEarned,
            GoldSpent = _currentGoldSpent,
            ItemsCollected = _currentItemsCollected,
            PetsObtained = _currentPetsObtained,
            DamagePerSecond = dps,
            DeathCause = deathCause,
            KeyEvents = new List<string>(_currentKeyEvents)
        };
    }
    
    private void UpdateLifetimeStats(RoguelikeRunRecord record)
    {
        if (record.FloorReached > _data.HighestFloorReached)
            _data.HighestFloorReached = record.FloorReached;
        
        _data.TotalEnemiesKilled += record.EnemiesKilled;
        _data.TotalDamageDealt += record.DamageDealt;
        _data.TotalDamageTaken += record.DamageTaken;
        _data.TotalGoldEarned += record.GoldEarned;
        _data.TotalGoldSpent += record.GoldSpent;
        _data.TotalItemsCollected += record.ItemsCollected;
        _data.TotalPetsObtained += record.PetsObtained;
    }
    
    private void UpdateMostUsedClass(string characterClass)
    {
        if (characterClass == _data.MostUsedClass)
        {
            _data.MostUsedClassCount++;
        }
        else if (_data.MostUsedClassCount == 0 || _currentEnemiesKilled > _data.MostUsedClassCount)
        {
            _data.MostUsedClass = characterClass;
            _data.MostUsedClassCount = 1;
        }
    }
    
    private void UpdateMostUsedBuild(string buildType)
    {
        if (buildType == _data.MostUsedBuild)
        {
            _data.MostUsedBuildCount++;
        }
        else if (_data.MostUsedBuildCount == 0 || _currentEnemiesKilled > _data.MostUsedBuildCount)
        {
            _data.MostUsedBuild = buildType;
            _data.MostUsedBuildCount = 1;
        }
    }
    
    // Track during run
    public void RecordEnemyKill(bool isBoss = false)
    {
        if (!_runInProgress) return;
        _currentEnemiesKilled++;
        if (isBoss) _currentBossesKilled++;
    }
    
    public void RecordDamageDealt(int damage)
    {
        if (!_runInProgress) return;
        _currentDamageDealt += damage;
    }
    
    public void RecordDamageTaken(int damage)
    {
        if (!_runInProgress) return;
        _currentDamageTaken += damage;
    }
    
    public void RecordGoldEarned(int gold)
    {
        if (!_runInProgress) return;
        _currentGoldEarned += gold;
    }
    
    public void RecordGoldSpent(int gold)
    {
        if (!_runInProgress) return;
        _currentGoldSpent += gold;
    }
    
    public void RecordItemCollected()
    {
        if (!_runInProgress) return;
        _currentItemsCollected++;
    }
    
    public void RecordPetObtained()
    {
        if (!_runInProgress) return;
        _currentPetsObtained++;
    }
    
    public void RecordKeyEvent(string eventName)
    {
        if (!_runInProgress) return;
        _currentKeyEvents.Add($"{DateTime.Now:HH:mm:ss} - {eventName}");
    }
    
    // Getters
    public RoguelikeStatsData GetData() => _data;
    public bool IsRunInProgress() => _runInProgress;
    public int GetCurrentRunId() => _currentRunId;
    
    // Get recent runs
    public List<RoguelikeRunRecord> GetRecentRuns(int count = 10)
    {
        var result = new List<RoguelikeRunRecord>();
        int start = Math.Max(0, _data.RunHistory.Count - count);
        for (int i = start; i < _data.RunHistory.Count; i++)
        {
            result.Add(_data.RunHistory[i]);
        }
        return result;
    }
    
    // Get statistics summary
    public Dictionary<string, string> GetStatisticsSummary()
    {
        float winRate = _data.TotalRuns > 0 ? (float)_data.TotalWins / _data.TotalRuns * 100 : 0;
        float avgFloor = _data.TotalRuns > 0 ? (float)_data.HighestFloorReached / _data.TotalRuns : 0;
        
        return new Dictionary<string, string>
        {
            { "TotalRuns", _data.TotalRuns.ToString() },
            { "Wins", _data.TotalWins.ToString() },
            { "Deaths", _data.TotalDeaths.ToString() },
            { "WinRate", $"{winRate:F1}%" },
            { "HighestFloor", _data.HighestFloorReached.ToString() },
            { "TotalEnemies", _data.TotalEnemiesKilled.ToString() },
            { "TotalGold", _data.TotalGoldEarned.ToString() },
            { "CurrentWinStreak", _data.CurrentWinStreak.ToString() },
            { "BestWinStreak", _data.BestWinStreak.ToString() },
            { "FavoriteClass", _data.MostUsedClass },
            { "FavoriteBuild", _data.MostUsedBuild }
        };
    }
    
    // Save/Load (to be implemented)
    public void SaveData() { }
    public void LoadData() { }

    #region Save System
    
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Godot.Dictionary();
        
        // 保存总统计数据
        data["total_runs"] = _data.TotalRuns;
        data["total_wins"] = _data.TotalWins;
        data["total_deaths"] = _data.TotalDeaths;
        data["highest_floor_reached"] = _data.HighestFloorReached;
        data["total_enemies_killed"] = _data.TotalEnemiesKilled;
        data["total_damage_dealt"] = _data.TotalDamageDealt;
        data["total_damage_taken"] = _data.TotalDamageTaken;
        data["total_gold_earned"] = _data.TotalGoldEarned;
        data["total_gold_spent"] = _data.TotalGoldSpent;
        data["total_items_collected"] = _data.TotalItemsCollected;
        data["total_pets_obtained"] = _data.TotalPetsObtained;
        
        // 保存连胜/连败记录
        data["current_win_streak"] = _data.CurrentWinStreak;
        data["best_win_streak"] = _data.BestWinStreak;
        data["current_loss_streak"] = _data.CurrentLossStreak;
        data["best_loss_streak"] = _data.BestLossStreak;
        
        // 保存最喜欢的职业和构建
        data["most_used_class"] = _data.MostUsedClass;
        data["most_used_class_count"] = _data.MostUsedClassCount;
        data["most_used_build"] = _data.MostUsedBuild;
        data["most_used_build_count"] = _data.MostUsedBuildCount;
        
        // 保存死亡原因统计
        var deathCauses = new Godot.Dictionary();
        foreach (var kvp in _data.DeathCauses)
        {
            deathCauses[kvp.Key] = kvp.Value;
        }
        data["death_causes"] = deathCauses;
        
        // 保存历史记录（最近50场）
        var runHistory = new Godot.Array();
        int start = Math.Max(0, _data.RunHistory.Count - 50);
        for (int i = start; i < _data.RunHistory.Count; i++)
        {
            var record = _data.RunHistory[i];
            var recordData = new Godot.Dictionary();
            recordData["run_id"] = record.RunId;
            recordData["victory"] = record.Victory;
            recordData["floor_reached"] = record.FloorReached;
            recordData["character_class"] = record.CharacterClass;
            recordData["build_type"] = record.BuildType;
            recordData["enemies_killed"] = record.EnemiesKilled;
            recordData["bosses_killed"] = record.BossesKilled;
            recordData["damage_dealt"] = record.DamageDealt;
            recordData["damage_taken"] = record.DamageTaken;
            recordData["gold_earned"] = record.GoldEarned;
            recordData["gold_spent"] = record.GoldSpent;
            recordData["duration"] = record.Duration;
            recordData["death_cause"] = record.DeathCause;
            runHistory.Add(recordData);
        }
        data["run_history"] = runHistory;
        
        return data;
    }
    
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 加载总统计数据
        if (data.Contains("total_runs")) _data.TotalRuns = (int)data["total_runs"];
        if (data.Contains("total_wins")) _data.TotalWins = (int)data["total_wins"];
        if (data.Contains("total_deaths")) _data.TotalDeaths = (int)data["total_deaths"];
        if (data.Contains("highest_floor_reached")) _data.HighestFloorReached = (int)data["highest_floor_reached"];
        if (data.Contains("total_enemies_killed")) _data.TotalEnemiesKilled = (int)data["total_enemies_killed"];
        if (data.Contains("total_damage_dealt")) _data.TotalDamageDealt = (int)data["total_damage_dealt"];
        if (data.Contains("total_damage_taken")) _data.TotalDamageTaken = (int)data["total_damage_taken"];
        if (data.Contains("total_gold_earned")) _data.TotalGoldEarned = (int)data["total_gold_earned"];
        if (data.Contains("total_gold_spent")) _data.TotalGoldSpent = (int)data["total_gold_spent"];
        if (data.Contains("total_items_collected")) _data.TotalItemsCollected = (int)data["total_items_collected"];
        if (data.Contains("total_pets_obtained")) _data.TotalPetsObtained = (int)data["total_pets_obtained"];
        
        // 加载连胜/连败记录
        if (data.Contains("current_win_streak")) _data.CurrentWinStreak = (int)data["current_win_streak"];
        if (data.Contains("best_win_streak")) _data.BestWinStreak = (int)data["best_win_streak"];
        if (data.Contains("current_loss_streak")) _data.CurrentLossStreak = (int)data["current_loss_streak"];
        if (data.Contains("best_loss_streak")) _data.BestLossStreak = (int)data["best_loss_streak"];
        
        // 加载最喜欢的职业和构建
        if (data.Contains("most_used_class")) _data.MostUsedClass = (string)data["most_used_class"];
        if (data.Contains("most_used_class_count")) _data.MostUsedClassCount = (int)data["most_used_class_count"];
        if (data.Contains("most_used_build")) _data.MostUsedBuild = (string)data["most_used_build"];
        if (data.Contains("most_used_build_count")) _data.MostUsedBuildCount = (int)data["most_used_build_count"];
        
        // 加载死亡原因统计
        if (data.Contains("death_causes"))
        {
            _data.DeathCauses.Clear();
            var deathCauses = (Godot.Dictionary)data["death_causes"];
            foreach (string cause in deathCauses.Keys)
            {
                _data.DeathCauses[cause] = (int)deathCauses[cause];
            }
        }
        
        // 加载历史记录
        if (data.Contains("run_history"))
        {
            _data.RunHistory.Clear();
            var runHistory = (Godot.Array)data["run_history"];
            foreach (var recordData in runHistory)
            {
                var rd = (Godot.Dictionary)recordData;
                var record = new RoguelikeRunRecord
                {
                    RunId = (int)rd["run_id"],
                    Victory = (bool)rd["victory"],
                    FloorReached = (int)rd["floor_reached"],
                    CharacterClass = (string)rd["character_class"],
                    BuildType = (string)rd["build_type"],
                    EnemiesKilled = (int)rd["enemies_killed"],
                    BossesKilled = (int)rd["bosses_killed"],
                    DamageDealt = (int)rd["damage_dealt"],
                    DamageTaken = (int)rd["damage_taken"],
                    GoldEarned = (int)rd["gold_earned"],
                    GoldSpent = (int)rd["gold_spent"],
                    Duration = (int)rd["duration"],
                    DeathCause = (string)rd["death_cause"]
                };
                _data.RunHistory.Add(record);
            }
        }
        
        GD.Print($"[RoguelikeStats] Loaded: {_data.TotalRuns} runs, {_data.TotalWins} wins, {_data.HighestFloorReached} best floor");
    }
    
    #endregion
}
