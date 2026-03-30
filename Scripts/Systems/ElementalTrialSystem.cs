using Godot;
/// <summary>
/// 元素试炼系统。
/// </summary>
using System;
using System.Collections.Generic;

/// <summary>
/// 元素试炼系统 - 管理元素试炼关卡
/// </summary>
public class ElementalTrialSystem : BaseSystem
{
    private static ElementalTrialSystem _instance;
    public static ElementalTrialSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new ElementalTrialSystem();
            return _instance;
        }
    }

    [Signal]
    public delegate void TrialStartedDelegate(string trialId, int wave, int timeRemaining);
    [Signal]
    public delegate void TrialCompletedDelegate(string trialId, int wave, int timeRemaining);
    [Signal]
    public delegate void TrialFailedDelegate(string trialId, int wave, string reason);
    [Signal]
    public delegate void WaveCompletedDelegate(int currentWave, int totalWaves);
    [Signal]
    public delegate void TrialUnlockedDelegate(string trialId);

    private ElementalTrialDatabase _database;
    private Dictionary<string, PlayerTrialProgress> _playerProgress;
    private string _currentTrialId;
    private int _currentWave;
    private float _timeRemaining;
    private bool _isTrialActive;
    private Timer _trialTimer;
    
    // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
    public event Action OnTrialTimerTickUI;

    public ElementalTrialSystem()
    {
        _database = ElementalTrialDatabase.Instance;
        _playerProgress = new Dictionary<string, PlayerTrialProgress>();
    }

    public void Initialize()
    {
        _trialTimer = new Timer();
        _trialTimer.WaitTime = 1.0f;
        // REQ-058-11: migrated from Godot 3 .Connect() to C# event +=
        _trialTimer.Timeout += OnTrialTimerTick;
    }

    private void OnTrialTimerTick()
    {
        // REQ-058-11: Invoke new event
        OnTrialTimerTickUI?.Invoke();
        if (!_isTrialActive) return;

        _timeRemaining -= 1.0f;
        
        if (_timeRemaining <= 0)
        {
            FailTrial("时间耗尽！");
        }
    }

    public bool StartTrial(string trialId)
    {
        var trial = _database.GetTrial(trialId);
        if (trial == null || !trial.IsUnlocked)
        {
            GD.PrintErr($"Trial {trialId} not found or not unlocked");
            return false;
        }

        if (_isTrialActive)
        {
            GD.PrintErr("A trial is already active");
            return false;
        }

        _currentTrialId = trialId;
        _currentWave = 1;
        _timeRemaining = trial.TimeLimit;
        _isTrialActive = true;

        // Initialize player progress if needed
        string playerId = "local_player"; // Single player mode
        if (!_playerProgress.ContainsKey(playerId))
        {
            _playerProgress[playerId] = new PlayerTrialProgress();
        }

        // Spawn first wave enemies
        SpawnWave(trial);

        EmitSignal(nameof(TrialStartedDelegate), trialId, _currentWave, (int)_timeRemaining);
        
        if (_trialTimer != null)
        {
            _trialTimer.Start();
        }

        return true;
    }

    private void SpawnWave(ElementalTrialData trial)
    {
        // This would integrate with EnemySpawner
        // For now, we track the wave number
        GD.Print($"Spawning wave {_currentWave} with {trial.EnemyIds.Count} enemy types");
    }

    public void OnWaveCleared()
    {
        var trial = _database.GetTrial(_currentTrialId);
        if (trial == null) return;

        EmitSignal(nameof(WaveCompletedDelegate), _currentWave, trial.WaveCount);
        
        if (_currentWave >= trial.WaveCount)
        {
            CompleteTrial();
        }
        else
        {
            _currentWave++;
            SpawnWave(trial);
        }
    }

    public void OnPlayerDefeated()
    {
        FailTrial("玩家被击败！");
    }

    private void CompleteTrial()
    {
        _isTrialActive = false; 
        if (_trialTimer != null)
        {
            _trialTimer.Stop();
        }

        var trial = _database.GetTrial(_currentTrialId);
        if (trial == null) return;

        // Update progress
        string playerId = "local_player";
        if (_playerProgress.ContainsKey(playerId))
        {
            var progress = _playerProgress[playerId];
            progress.CompletedTrials.Add(_currentTrialId);
            
            if (progress.BestWaves.ContainsKey(_currentTrialId))
            {
                if (_currentWave > progress.BestWaves[_currentTrialId])
                {
                    progress.BestWaves[_currentTrialId] = _currentWave;
                }
            }
            else
            {
                progress.BestWaves[_currentTrialId] = _currentWave;
            }
        }

        // Grant rewards
        var player = GetPlayer();
        if (player != null)
        {
            player.AddGold(trial.GoldReward);
            player.AddExp(trial.ExpReward);
            
            foreach (var itemId in trial.ItemRewards)
            {
                // Add item to player inventory
                GD.Print($"Granting reward: {itemId}");
            }
        }

        // Unlock next trial
        UnlockNextTrial(trial);

        // Mark as completed
        trial.IsCompleted = true;
        trial.BestWave = _currentWave;

        EmitSignal(nameof(TrialCompletedDelegate), _currentTrialId, _currentWave, (int)_timeRemaining);
        _currentTrialId = "";
    }

    private void FailTrial(string reason)
    {
        _isTrialActive = false; 
        if (_trialTimer != null)
        {
            _trialTimer.Stop();
        }

        string failedTrialId = _currentTrialId;
        
        // Record progress
        string playerId = "local_player";
        if (_playerProgress.ContainsKey(playerId))
        {
            var progress = _playerProgress[playerId];
            if (progress.BestWaves.ContainsKey(failedTrialId))
            {
                progress.BestWaves[failedTrialId] = Math.Max(progress.BestWaves[failedTrialId], _currentWave);
            }
            else
            {
                progress.BestWaves[failedTrialId] = _currentWave;
            }
        }

        EmitSignal(nameof(TrialFailedDelegate), failedTrialId, _currentWave, reason);
        _currentTrialId = "";
    }

    private void UnlockNextTrial(ElementalTrialData completedTrial)
    {
        var allTrials = _database.GetAllTrials();
        int currentIndex = allTrials.FindIndex(t => t.TrialId == completedTrial.TrialId);
        
        if (currentIndex >= 0 && currentIndex < allTrials.Count - 1)
        {
            var nextTrial = allTrials[currentIndex + 1];
            if (!nextTrial.IsUnlocked)
            {
                nextTrial.IsUnlocked = true;
                EmitSignal(nameof(TrialUnlockedDelegate), nextTrial.TrialId);
            }
        }
    }

    private Node GetPlayer()
    {
        var main = GetTree().CurrentScene;
        if (main != null)
        {
            return main.GetNodeOrNull("Player");
        }
        return null;
    }

    private Node GetTree()
    {
        return Engine.GetMainLoop();
    }

    public bool IsTrialActive()
    {
        return _isTrialActive;
    }

    public int GetCurrentWave()
    {
        return _currentWave;
    }

    public float GetTimeRemaining()
    {
        return _timeRemaining;
    }

    public string GetCurrentTrialId()
    {
        return _currentTrialId;
    }

    public List<ElementalTrialData> GetAllTrials()
    {
        return _database.GetAllTrials();
    }

    public List<ElementalTrialData> GetUnlockedTrials()
    {
        return _database.GetUnlockedTrials();
    }

    public ElementalTrialData GetTrial(string trialId)
    {
        return _database.GetTrial(trialId);
    }

    public void UnlockTrial(string trialId)
    {
        _database.UnlockTrial(trialId);
        var trial = _database.GetTrial(trialId);
        if (trial != null)
        {
            EmitSignal(nameof(TrialUnlockedDelegate), trialId);
        }
    }

    public Dictionary<string, object> Save()
    {
        var data = new Dictionary<string, object>();
        
        var trialData = new Dictionary<string, Dictionary<string, object>>();
        foreach (var trial in _database.GetAllTrials())
        {
            trialData[trial.TrialId] = new Dictionary<string, object>
            {
                { "isUnlocked", trial.IsUnlocked },
                { "isCompleted", trial.IsCompleted },
                { "bestWave", trial.BestWave }
            };
        }
        data["trials"] = trialData;

        var progressData = new Dictionary<string, object>();
        foreach (var kvp in _playerProgress)
        {
            progressData[kvp.Key] = kvp.Value.Save();
        }
        data["playerProgress"] = progressData;

        return data;
    }

    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.ContainsKey("trials"))
        {
            var trialData = data["trials"] as Dictionary<string, object>;
            foreach (var kvp in trialData)
            {
                var trial = _database.GetTrial(kvp.Key);
                if (trial != null)
                {
                    var trialSave = kvp.Value as Dictionary<string, object>;
                    if (trialSave != null)
                    {
                        trial.IsUnlocked = (bool)trialSave.Get("isUnlocked", false);
                        trial.IsCompleted = (bool)trialSave.Get("isCompleted", false);
                        trial.BestWave = (int)trialSave.Get("bestWave", 0);
                    }
                }
            }
        }

        if (data.ContainsKey("playerProgress"))
        {
            var progressData = data["playerProgress"] as Dictionary<string, object>;
            foreach (var kvp in progressData)
            {
                var progress = new PlayerTrialProgress();
                progress.Load(kvp.Value as Dictionary<string, object>);
                _playerProgress[kvp.Key] = progress;
            }
        }
    }
}

public class PlayerTrialProgress
{
    public List<string> CompletedTrials { get; set; }
    public Dictionary<string, int> BestWaves { get; set; }

    public PlayerTrialProgress()
    {
        CompletedTrials = new List<string>();
        BestWaves = new Dictionary<string, int>();
    }

    public Dictionary<string, object> Save()
    {
        return new Dictionary<string, object>
        {
            { "completedTrials", CompletedTrials },
            { "bestWaves", BestWaves }
        };
    }

    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.ContainsKey("completedTrials"))
        {
            CompletedTrials = new List<string>((System.Collections.IEnumerable)data["completedTrials"]);
        }

        if (data.ContainsKey("bestWaves"))
        {
            var wavesData = data["bestWaves"] as Dictionary<string, object>;
            foreach (var kvp in wavesData)
            {
                BestWaves[kvp.Key] = Convert.ToInt32(kvp.Value);
            }
        }
    }

    public override Dictionary ExportSaveData()
    {
        return new Dictionary
        {
            { "completedTrials", CompletedTrials },
            { "bestWaves", BestWaves }
        };
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        if (data.ContainsKey("completedTrials"))
        {
            CompletedTrials = new List<string>((System.Collections.IEnumerable)data["completedTrials"]);
        }

        if (data.ContainsKey("bestWaves"))
        {
            var wavesData = data["bestWaves"] as Dictionary<string, object>;
            foreach (var kvp in wavesData)
            {
                BestWaves[kvp.Key] = Convert.ToInt32(kvp.Value);
            }
        }
    }
}
