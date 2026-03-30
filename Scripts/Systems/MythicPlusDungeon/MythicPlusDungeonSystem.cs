using System;
using System.Collections.Generic;
using Godot;

public class MythicPlusDungeonSystem : BaseSystem
{
    private static MythicPlusDungeonSystem _instance;
    public static MythicPlusDungeonSystem Instance => _instance;
    
    private MythicPlusProgress _playerProgress;
    private MythicPlusRun _currentRun;
    private List<MythicPlusRun> _runHistory;
    private List<MythicPlusLeaderboard> _leaderboard;
    private MythicPlusAffixGroup _currentAffixes;
    private Random _random;
    
    // Signals
    public static string RunStartedSignal => "mythic_run_started";
    public static string RunCompletedSignal => "mythic_run_completed";
    public static string RunFailedSignal => "mythic_run_failed";
    public static string BossDefeatedSignal => "mythic_boss_defeated";
    public static string AffixTriggeredSignal => "mythic_affix_triggered";
    public static string LevelCompletedSignal => "mythic_level_completed";
    public static string ScoreUpdatedSignal => "mythic_score_updated";
    
    public override void _Ready()
    {
        _instance = this;
        _random = new Random();
        _runHistory = new List<MythicPlusRun>();
        _leaderboard = new List<MythicPlusLeaderboard>();
        
        MythicPlusDungeonDatabase.Initialize();
        _currentAffixes = MythicPlusDungeonDatabase.GetCurrentWeeklyAffixes();
        
        LoadProgress();
        
        GD.Print("[MythicPlusDungeonSystem] Initialized - Current Week Affixes: " + _currentAffixes.Name);
    }
    
    public void LoadProgress()
    {
        // Load player progress from save system
        _playerProgress = SaveSystem.Load<MythicPlusProgress>("mythic_plus_progress");
        if (_playerProgress == null)
        {
            _playerProgress = new MythicPlusProgress
            {
                BestLevel = 0,
                TotalRuns = 0,
                CompletedRuns = 0,
                FailedRuns = 0,
                HighestScore = 0,
                ConsecutiveCompletions = 0
            };
        }
    }
    
    public void SaveProgress()
    {
        SaveSystem.Save(_playerProgress, "mythic_plus_progress");
    }
    
    #region Run Management
    
    public MythicPlusRun StartRun(string dungeonId, int level)
    {
        if (_currentRun != null && !_currentRun.Completed && !_currentRun.Failed)
        {
            GD.Warning("[MythicPlusDungeon] Cannot start new run while one is in progress");
            return null;
        }
        
        var dungeon = MythicPlusDungeonDatabase.GetDungeon(dungeonId);
        if (dungeon == null)
        {
            GD.Warning("[MythicPlusDungeon] Unknown dungeon: " + dungeonId);
            return null;
        }
        
        _currentRun = new MythicPlusRun
        {
            RunId = _runHistory.Count + 1,
            DungeonLevel = level,
            StartTime = DateTime.UtcNow,
            Difficulty = GetDifficultyForLevel(level),
            ActiveAffixes = new List<MythicAffix>(_currentAffixes.Affixes)
        };
        
        // Add weekly affixes
        if (level >= 4 && _currentAffixes.Affixes.Count >= 1)
            _currentRun.ActiveAffixes.Add(_currentAffixes.Affixes[0]);
        if (level >= 7 && _currentAffixes.Affixes.Count >= 2)
            _currentRun.ActiveAffixes.Add(_currentAffixes.Affixes[1]);
        
        _playerProgress.TotalRuns++;
        _playerProgress.LastRunTime = DateTime.UtcNow;
        
        SaveProgress();
        
        EventSignal.emit_signal(RunStartedSignal, _currentRun);
        
        GD.Print($"[MythicPlusDungeon] Started run - Dungeon: {dungeon.Name}, Level: {level}, Affixes: {_currentRun.ActiveAffixes.Count}");
        
        return _currentRun;
    }
    
    public MythicPlusRun CompleteRun(bool success)
    {
        if (_currentRun == null || _currentRun.Completed || _currentRun.Failed)
        {
            GD.Warning("[MythicPlusDungeon] No active run to complete");
            return null;
        }
        
        _currentRun.EndTime = DateTime.UtcNow;
        _currentRun.CompletedTimeSeconds = (int)(_currentRun.EndTime.Value - _currentRun.StartTime).TotalSeconds;
        _currentRun.Completed = success;
        _currentRun.Failed = !success;
        
        // Calculate rewards
        if (success)
        {
            var reward = MythicPlusDungeonDatabase.GetReward(_currentRun.DungeonLevel);
            _currentRun.RewardGold = reward.Gold;
            _currentRun.RewardExp = reward.Experience;
            _currentRun.RewardItems = new List<string>(reward.Items);
            
            // Update progress
            _playerProgress.CompletedRuns++;
            _playerProgress.ConsecutiveCompletions++;
            
            if (_currentRun.DungeonLevel > _playerProgress.BestLevel)
            {
                _playerProgress.BestLevel = _currentRun.DungeonLevel;
            }
            
            // Track level completion
            if (!_playerProgress.LevelCompletionCount.ContainsKey(_currentRun.DungeonLevel))
                _playerProgress.LevelCompletionCount[_currentRun.DungeonLevel] = 0;
            _playerProgress.LevelCompletionCount[_currentRun.DungeonLevel]++;
            
            // Track best time
            if (!_playerProgress.LevelBestTime.ContainsKey(_currentRun.DungeonLevel) ||
                _currentRun.CompletedTimeSeconds < _playerProgress.LevelBestTime[_currentRun.DungeonLevel])
            {
                _playerProgress.LevelBestTime[_currentRun.DungeonLevel] = _currentRun.CompletedTimeSeconds;
            }
            
            _currentRun.Score = _currentRun.CalculateScore();
            
            if (_currentRun.Score > _playerProgress.HighestScore)
            {
                _playerProgress.HighestScore = _currentRun.Score;
            }
            
            EventSignal.emit_signal(RunCompletedSignal, _currentRun);
        }
        else
        {
            _playerProgress.FailedRuns++;
            _playerProgress.ConsecutiveCompletions = 0;
            EventSignal.emit_signal(RunFailedSignal, _currentRun);
        }
        
        _playerProgress.TotalTimePlayed += _currentRun.CompletedTimeSeconds;
        
        _runHistory.Add(_currentRun);
        
        // Add to leaderboard if successful
        if (success)
        {
            AddToLeaderboard(_currentRun);
        }
        
        SaveProgress();
        
        var result = _currentRun;
        _currentRun = null;
        
        return result;
    }
    
    public void RecordEnemyKilled()
    {
        if (_currentRun != null && !_currentRun.Completed && !_currentRun.Failed)
        {
            _currentRun.EnemiesKilled++;
            _playerProgress.TotalEnemiesKilled++;
        }
    }
    
    public void RecordBossDefeated()
    {
        if (_currentRun != null && !_currentRun.Completed && !_currentRun.Failed)
        {
            _currentRun.BossesDefeated++;
            EventSignal.emit_signal(BossDefeatedSignal, _currentRun.DungeonLevel, _currentRun.BossesDefeated);
        }
    }
    
    public void RecordDeath()
    {
        if (_currentRun != null && !_currentRun.Completed && !_currentRun.Failed)
        {
            _currentRun.Deaths++;
            _playerProgress.TotalDeaths++;
        }
    }
    
    public void TriggerAffix(string affixName)
    {
        EventSignal.emit_signal(AffixTriggeredSignal, affixName);
    }
    
    #endregion
    
    #region Queries
    
    public MythicPlusRun GetCurrentRun() => _currentRun;
    
    public MythicPlusProgress GetPlayerProgress() => _playerProgress;
    
    public List<MythicPlusRun> GetRunHistory() => new List<MythicPlusRun>(_runHistory);
    
    public List<MythicPlusRun> GetRecentRuns(int count = 10)
    {
        var result = new List<MythicPlusRun>();
        var sorted = new List<MythicPlusRun>(_runHistory);
        sorted.Sort((a, b) => b.StartTime.CompareTo(a.StartTime));
        
        for (int i = 0; i < Math.Min(count, sorted.Count); i++)
        {
            result.Add(sorted[i]);
        }
        
        return result;
    }
    
    public List<MythicPlusLeaderboard> GetLeaderboard(int count = 100, bool weekly = false)
    {
        var sorted = new List<MythicPlusLeaderboard>(_leaderboard);
        sorted.Sort((a, b) => b.Score.CompareTo(a.Score));
        
        var result = new List<MythicPlusLeaderboard>();
        for (int i = 0; i < Math.Min(count, sorted.Count); i++)
        {
            if (weekly && sorted[i].IsWeekly || !weekly)
                result.Add(sorted[i]);
        }
        
        return result;
    }
    
    public MythicPlusAffixGroup GetCurrentAffixes() => _currentAffixes;
    
    public List<MythicPlusDungeonConfig> GetAvailableDungeons()
    {
        return MythicPlusDungeonDatabase.GetAllDungeons();
    }
    
    public MythicPlusDungeonConfig GetDungeon(string dungeonId)
    {
        return MythicPlusDungeonDatabase.GetDungeon(dungeonId);
    }
    
    public int GetTimeLimit(int level)
    {
        return MythicPlusDungeonDatabase.GetTimeLimitForLevel(level);
    }
    
    public float GetHealthMultiplier(int level)
    {
        return MythicPlusDungeonDatabase.GetHealthMultiplierForLevel(level);
    }
    
    public float GetDamageMultiplier(int level)
    {
        return MythicPlusDungeonDatabase.GetDamageMultiplierForLevel(level);
    }
    
    public int GetCompletionRate()
    {
        if (_playerProgress.TotalRuns == 0) return 0;
        return (_playerProgress.CompletedRuns * 100) / _playerProgress.TotalRuns;
    }
    
    public double GetAverageTime()
    {
        if (_playerProgress.TotalRuns == 0) return 0;
        return (double)_playerProgress.TotalTimePlayed / _playerProgress.TotalRuns;
    }
    
    #endregion
    
    #region Private Methods
    
    private MythicPlusDifficulty GetDifficultyForLevel(int level)
    {
        return level switch
        {
            0 => MythicPlusDifficulty.Mythic0,
            2 => MythicPlusDifficulty.Mythic2,
            5 => MythicPlusDifficulty.Mythic5,
            10 => MythicPlusDifficulty.Mythic10,
            15 => MythicPlusDifficulty.Mythic15,
            20 => MythicPlusDifficulty.Mythic20,
            _ => MythicPlusDifficulty.MythicPlus
        };
    }
    
    private void AddToLeaderboard(MythicPlusRun run)
    {
        var entry = new MythicPlusLeaderboard
        {
            PlayerId = 1, // Current player
            PlayerName = PlayerData.Instance?.PlayerName ?? "Player",
            Level = run.DungeonLevel,
            Score = run.Score,
            TimeSeconds = run.CompletedTimeSeconds,
            CompletionTime = run.EndTime.Value,
            IsWeekly = false
        };
        
        // Add to both weekly and all-time
        _leaderboard.Add(entry);
        
        var weeklyEntry = new MythicPlusLeaderboard
        {
            PlayerId = 1,
            PlayerName = entry.PlayerName,
            Level = run.DungeonLevel,
            Score = run.Score,
            TimeSeconds = run.CompletedTimeSeconds,
            CompletionTime = run.EndTime.Value,
            IsWeekly = true
        };
        _leaderboard.Add(weeklyEntry);
        
        // Sort by score
        _leaderboard.Sort((a, b) => b.Score.CompareTo(a.Score));
    }
    
    #endregion
    
    #region Statistics
    
    public Dictionary<string, object> GetDetailedStats()
    {
        return new Dictionary<string, object>
        {
            { "best_level", _playerProgress.BestLevel },
            { "total_runs", _playerProgress.TotalRuns },
            { "completed_runs", _playerProgress.CompletedRuns },
            { "failed_runs", _playerProgress.FailedRuns },
            { "completion_rate", GetCompletionRate() },
            { "highest_score", _playerProgress.HighestScore },
            { "average_time", GetAverageTime() },
            { "total_time", _playerProgress.TotalTimePlayed },
            { "total_enemies_killed", _playerProgress.TotalEnemiesKilled },
            { "total_deaths", _playerProgress.TotalDeaths },
            { "consecutive_completions", _playerProgress.ConsecutiveCompletions },
            { "current_affixes", _currentAffixes.Name }
        };
    }
    
    #endregion
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 玩家进度
        if (_playerProgress != null)
        {
            data["best_level"] = _playerProgress.BestLevel;
            data["total_runs"] = _playerProgress.TotalRuns;
            data["completed_runs"] = _playerProgress.CompletedRuns;
            data["failed_runs"] = _playerProgress.FailedRuns;
            data["highest_score"] = _playerProgress.HighestScore;
            data["consecutive_completions"] = _playerProgress.ConsecutiveCompletions;
            data["total_time_played"] = _playerProgress.TotalTimePlayed;
            data["total_enemies_killed"] = _playerProgress.TotalEnemiesKilled;
            data["total_deaths"] = _playerProgress.TotalDeaths;
            
            // 等级完成次数
            data["level_completion_count"] = new Dictionary(_playerProgress.LevelCompletionCount);
            
            // 等级最佳时间
            data["level_best_time"] = new Dictionary(_playerProgress.LevelBestTime);
        }
        
        // 跑图历史
        var historyList = new Array();
        foreach (var run in _runHistory)
        {
            var runData = new Dictionary
            {
                { "run_id", run.RunId },
                { "dungeon_level", run.DungeonLevel },
                { "completed", run.Completed },
                { "failed", run.Failed },
                { "score", run.Score },
                { "completed_time_seconds", run.CompletedTimeSeconds },
                { "enemies_killed", run.EnemiesKilled },
                { "bosses_defeated", run.BossesDefeated },
                { "deaths", run.Deaths },
                { "start_time", run.StartTime.ToString("o") }
            };
            historyList.Add(runData);
        }
        data["run_history"] = historyList;
        
        // 排行榜
        var leaderboardList = new Array();
        foreach (var entry in _leaderboard)
        {
            var entryData = new Dictionary
            {
                { "player_id", entry.PlayerId },
                { "player_name", entry.PlayerName },
                { "level", entry.Level },
                { "score", entry.Score },
                { "time_seconds", entry.TimeSeconds },
                { "is_weekly", entry.IsWeekly }
            };
            leaderboardList.Add(entryData);
        }
        data["leaderboard"] = leaderboardList;
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 玩家进度
        if (_playerProgress == null)
        {
            _playerProgress = new MythicPlusProgress();
        }
        
        _playerProgress.BestLevel = (int)data.GetValueOrDefault("best_level", 0);
        _playerProgress.TotalRuns = (int)data.GetValueOrDefault("total_runs", 0);
        _playerProgress.CompletedRuns = (int)data.GetValueOrDefault("completed_runs", 0);
        _playerProgress.FailedRuns = (int)data.GetValueOrDefault("failed_runs", 0);
        _playerProgress.HighestScore = (int)data.GetValueOrDefault("highest_score", 0);
        _playerProgress.ConsecutiveCompletions = (int)data.GetValueOrDefault("consecutive_completions", 0);
        _playerProgress.TotalTimePlayed = (int)data.GetValueOrDefault("total_time_played", 0);
        _playerProgress.TotalEnemiesKilled = (int)data.GetValueOrDefault("total_enemies_killed", 0);
        _playerProgress.TotalDeaths = (int)data.GetValueOrDefault("total_deaths", 0);
        
        // 等级完成次数
        if (data.Contains("level_completion_count"))
        {
            var completionDict = (Dictionary)data["level_completion_count"];
            _playerProgress.LevelCompletionCount = new Dictionary<int, int>();
            foreach (var kvp in completionDict)
            {
                _playerProgress.LevelCompletionCount[int.Parse(kvp.Key.ToString())] = (int)kvp.Value;
            }
        }
        
        // 等级最佳时间
        if (data.Contains("level_best_time"))
        {
            var bestTimeDict = (Dictionary)data["level_best_time"];
            _playerProgress.LevelBestTime = new Dictionary<int, int>();
            foreach (var kvp in bestTimeDict)
            {
                _playerProgress.LevelBestTime[int.Parse(kvp.Key.ToString())] = (int)kvp.Value;
            }
        }
        
        // 跑图历史
        _runHistory = new List<MythicPlusRun>();
        if (data.Contains("run_history"))
        {
            var historyArray = (Array)data["run_history"];
            foreach (Dictionary runData in historyArray)
            {
                var run = new MythicPlusRun
                {
                    RunId = (int)runData["run_id"],
                    DungeonLevel = (int)runData["dungeon_level"],
                    Completed = (bool)runData["completed"],
                    Failed = (bool)runData["failed"],
                    Score = (int)runData["score"],
                    CompletedTimeSeconds = (int)runData["completed_time_seconds"],
                    EnemiesKilled = (int)runData["enemies_killed"],
                    BossesDefeated = (int)runData["bosses_defeated"],
                    Deaths = (int)runData["deaths"]
                };
                if (runData.Contains("start_time") && DateTime.TryParse(runData["start_time"].ToString(), out var startTime))
                {
                    run.StartTime = startTime;
                }
                _runHistory.Add(run);
            }
        }
        
        // 排行榜
        _leaderboard = new List<MythicPlusLeaderboard>();
        if (data.Contains("leaderboard"))
        {
            var leaderboardArray = (Array)data["leaderboard"];
            foreach (Dictionary entryData in leaderboardArray)
            {
                var entry = new MythicPlusLeaderboard
                {
                    PlayerId = (int)entryData["player_id"],
                    PlayerName = (string)entryData["player_name"],
                    Level = (int)entryData["level"],
                    Score = (int)entryData["score"],
                    TimeSeconds = (int)entryData["time_seconds"],
                    IsWeekly = (bool)entryData["is_weekly"]
                };
                _leaderboard.Add(entry);
            }
        }
    }
}
