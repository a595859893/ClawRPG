using Godot;
using System;
using System.Collections.Generic;

public class RoguelikeStatsSystem
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
}
