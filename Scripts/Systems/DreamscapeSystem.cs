using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 梦境系统 - 管理梦境世界和梦境事件
/// </summary>
public partial class DreamscapeSystem : BaseSystem
{
    private static DreamscapeSystem _instance;
    public static DreamscapeSystem Instance => _instance;
    
    public PlayerDreamscapeData PlayerData { get; private set; }
    public bool IsInDreamscape => _currentDreamscape != null;
    
    private DreamscapeEntry _currentDreamscape;
    private DreamscapeLayer _currentLayer;
    private DreamscapeRule _activeRule;
    private int _elapsedTime;
    private bool _layerCompleted;
    private Timer _timer;
    
    // Signals
    public Action<string, int> DreamscapeEntered;
    public Action<string, int, DreamscapeRule> DreamscapeLayerStarted;
    public Action<string, int, int, int, int> DreamscapeLayerCompleted;
    public Action<string, int, int, int> DreamscapeCompleted;
    public Action<string, int> DreamscapeFailed;
    public Action<string> DreamscapeUnlocked;
    public Action<string> DreamscapeMastered;
    
    protected override void Initialize()
    {
        _instance = this;
        PlayerData = new PlayerDreamscapeData();
        _timer = new Timer();
        AddChild(_timer);
        _timer.WaitTime = 1.0f;
        _timer.Timeout += _OnTimerTick;
    }

    public override void _Ready()
    {
        base._Ready();
    }
    
    public void Initialize(PlayerDreamscapeData savedData)
    {
        if (savedData != null)
        {
            PlayerData = savedData;
        }
    }
    
    public bool EnterDreamscape(string dreamscapeId)
    {
        var dreamscape = DreamscapeDatabase.Instance.GetDreamscape(dreamscapeId);
        if (dreamscape == null) return false;
        
        if (dreamscape.State == DreamscapeState.Locked) return false;
        
        _currentDreamscape = dreamscape;
        _currentDreamscape.State = DreamscapeState.InProgress;
        
        if (!PlayerData.Progress.ContainsKey(dreamscapeId))
        {
            PlayerData.Progress[dreamscapeId] = new DreamscapeProgress
            {
                DreamscapeId = dreamscapeId,
                CurrentLayer = 1,
                HighestLayer = 0,
                TotalScore = 0,
                BestScore = 0,
                CompletionCount = 0,
                MasteryCount = 0,
                BestTime = float.MaxValue,
                LastEntered = DateTime.Now,
                IsInDreamscape = true
            };
        }
        else
        {
            PlayerData.Progress[dreamscapeId].IsInDreamscape = true;
            PlayerData.Progress[dreamscapeId].CurrentLayer = 1;
        }
        
        PlayerData.UnlockedDreamscapes[dreamscapeId] = true;
        
        _StartLayer(1);
        DreamscapeEntered(dreamscapeId, 1);
        
        return true;
    }
    
    public bool EnterNextLayer()
    {
        if (_currentDreamscape == null) return false;
        
        var progress = PlayerData.Progress[_currentDreamscape.Id];
        int nextLayer = progress.CurrentLayer + 1;
        
        if (nextLayer > _currentDreamscape.TotalLayers)
        {
            return _CompleteDreamscape();
        }
        
        _StartLayer(nextLayer);
        return true;
    }
    
    private void _StartLayer(int layerNumber)
    {
        var progress = PlayerData.Progress[_currentDreamscape.Id];
        progress.CurrentLayer = layerNumber;
        
        if (layerNumber > progress.HighestLayer)
        {
            progress.HighestLayer = layerNumber;
        }
        
        _currentLayer = DreamscapeDatabase.Instance.GetLayer(_currentDreamscape.Type, layerNumber);
        _activeRule = _currentLayer.SpecialRule;
        _elapsedTime = 0;
        _layerCompleted = false;
        
        progress.CurrentLayerScore = 0;
        progress.CurrentLayerTime = 0;
        
        _timer.Start();
        
        DreamscapeLayerStarted(_currentDreamscape.Id, layerNumber, _activeRule);
    }
    
    private void _OnTimerTick()
    {
        if (_currentDreamscape == null || _layerCompleted) return;
        
        _elapsedTime++;
        var progress = PlayerData.Progress[_currentDreamscape.Id];
        progress.CurrentLayerTime = _elapsedTime;
        
        // Check time limit
        if (_elapsedTime >= _currentLayer.TimeLimit)
        {
            _FailLayer();
        }
    }
    
    public void CompleteLayer(int score, int enemiesKilled)
    {
        if (_currentDreamscape == null || _currentLayer == null || _layerCompleted) return;
        
        _layerCompleted = true;
        _timer.Stop();
        
        var progress = PlayerData.Progress[_currentDreamscape.Id];
        var reward = DreamscapeDatabase.Instance.GetLayerReward(_currentDreamscape.Type, progress.CurrentLayer);
        
        // Calculate score
        int layerScore = _currentLayer.BaseScore + score + (enemiesKilled * 50);
        
        // Time bonus
        int timeBonus = Math.Max(0, (_currentLayer.TimeLimit - _elapsedTime) * 10);
        layerScore += timeBonus;
        
        // Apply multiplier
        layerScore = (int)(layerScore * _currentDreamscape.ScoreMultiplier);
        
        progress.CurrentLayerScore = layerScore;
        progress.TotalScore += layerScore;
        
        int gold = (int)(reward.Gold * _currentDreamscape.DropMultiplier);
        int exp = (int)(reward.Experience * _currentDreamscape.DropMultiplier);
        
        DreamscapeLayerCompleted(_currentDreamscape.Id, progress.CurrentLayer, layerScore, gold, exp);
        
        // Update highest score
        if (progress.TotalScore > progress.BestScore)
        {
            progress.BestScore = progress.TotalScore;
        }
        
        // Update best time
        if (_elapsedTime < progress.BestTime)
        {
            progress.BestTime = _elapsedTime;
        }
    }
    
    private bool _CompleteDreamscape()
    {
        if (_currentDreamscape == null) return false;
        
        _timer.Stop();
        
        var progress = PlayerData.Progress[_currentDreamscape.Id];
        progress.CompletionCount++;
        progress.IsInDreamscape = false;
        
        // Calculate completion rewards
        int completionGold = progress.TotalScore / 10;
        int completionExp = progress.TotalScore / 20;
        
        if (progress.CompletionCount >= 10 && progress.MasteryCount == 0)
        {
            progress.MasteryCount++;
            _currentDreamscape.State = DreamscapeState.Mastered;
            DreamscapeMastered(_currentDreamscape.Id);
        }
        else
        {
            _currentDreamscape.State = DreamscapeState.Completed;
        }
        
        PlayerData.DreamscapesCompleted++;
        PlayerData.TotalLayersCleared += progress.HighestLayer;
        
        // Check for new unlocks
        _CheckForNewUnlocks();
        
        DreamscapeCompleted(_currentDreamscape.Id, progress.TotalScore, completionGold, completionExp);
        
        _currentDreamscape = null;
        _currentLayer = null;
        
        return true;
    }
    
    private void _FailLayer()
    {
        if (_currentDreamscape == null) return;
        
        _timer.Stop();
        
        var progress = PlayerData.Progress[_currentDreamscape.Id];
        int failedLayer = progress.CurrentLayer;
        progress.IsInDreamscape = false;
        
        _currentDreamscape.State = DreamscapeState.Available;
        
        DreamscapeFailed(_currentDreamscape.Id, failedLayer);
        
        _currentDreamscape = null;
        _currentLayer = null;
    }
    
    public void ExitDreamscape()
    {
        if (_currentDreamscape == null) return;
        
        _timer.Stop();
        
        if (PlayerData.Progress.ContainsKey(_currentDreamscape.Id))
        {
            PlayerData.Progress[_currentDreamscape.Id].IsInDreamscape = false;
        }
        
        _currentDreamscape.State = DreamscapeState.Available;
        _currentDreamscape = null;
        _currentLayer = null;
    }
    
    private void _CheckForNewUnlocks()
    {
        // Unlock next dreamscape based on completion
        int completedCount = PlayerData.DreamscapesCompleted;
        
        if (completedCount >= 1)
        {
            DreamscapeDatabase.Instance.UnlockDreamscape(DreamscapeType.Ethereal);
            if (DreamscapeDatabase.Instance.GetDreamscapeByType(DreamscapeType.Ethereal).State == DreamscapeState.Available)
            {
                DreamscapeUnlocked("ethereal");
            }
        }
        
        if (completedCount >= 2)
        {
            DreamscapeDatabase.Instance.UnlockDreamscape(DreamscapeType.Temporal);
            if (DreamscapeDatabase.Instance.GetDreamscapeByType(DreamscapeType.Temporal).State == DreamscapeState.Available)
            {
                DreamscapeUnlocked("temporal");
            }
        }
        
        if (completedCount >= 3)
        {
            DreamscapeDatabase.Instance.UnlockDreamscape(DreamscapeType.Void);
            if (DreamscapeDatabase.Instance.GetDreamscapeByType(DreamscapeType.Void).State == DreamscapeState.Available)
            {
                DreamscapeUnlocked("void");
            }
        }
        
        if (completedCount >= 5)
        {
            DreamscapeDatabase.Instance.UnlockDreamscape(DreamscapeType.Lucid);
            if (DreamscapeDatabase.Instance.GetDreamscapeByType(DreamscapeType.Lucid).State == DreamscapeState.Available)
            {
                DreamscapeUnlocked("lucid");
            }
        }
    }
    
    public void CheckPlayerLevelUnlocks(int playerLevel)
    {
        DreamscapeDatabase.Instance.CheckAndUnlockDreamscapes(playerLevel);
        
        foreach (var ds in DreamscapeDatabase.Instance.Dreamscapes.Values)
        {
            if (ds.State == DreamscapeState.Available && !PlayerData.UnlockedDreamscapes.ContainsKey(ds.Id))
            {
                PlayerData.UnlockedDreamscapes[ds.Id] = true;
                DreamscapeUnlocked(ds.Id);
            }
        }
    }
    
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        data["player_data"] = PlayerData;
        return new Dictionary(data);
    }
    
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("player_data"))
        {
            PlayerData = (PlayerDreamscapeData)data["player_data"];
        }
    }
    
    // Getters
    public DreamscapeEntry GetCurrentDreamscape() => _currentDreamscape;
    public DreamscapeLayer GetCurrentLayer() => _currentLayer;
    public DreamscapeRule GetActiveRule() => _activeRule;
    public int GetElapsedTime() => _elapsedTime;
    public DreamscapeProgress GetProgress(string dreamscapeId) => 
        PlayerData.Progress.ContainsKey(dreamscapeId) ? PlayerData.Progress[dreamscapeId] : null;
    public List<DreamscapeEntry> GetUnlockedDreamscapes() => DreamscapeDatabase.Instance.GetUnlockedDreamscapes();
}
