using Godot;
using System.Collections.Generic;

/// <summary>
/// Global singleton system that tracks player combo usage patterns across all enemies.
/// Enemies query this system via EnemyPatternTracker components to know when to apply countermeasures.
/// </summary>
public partial class EnemyObserverSystem : Node
{
    public static EnemyObserverSystem Instance { get; private set; }
    
    // Global player combo usage history
    // Key: comboId, Value: usage count
    private Dictionary<string, int> _globalComboUsage = new Dictionary<string, int>();
    
    // Last tick each combo was used
    private Dictionary<string, float> _globalComboLastTick = new Dictionary<string, float>();
    
    // Per-combo decay rate (ticks since last use)
    private Dictionary<string, float> _decayAccumulator = new Dictionary<string, float>();
    
    // Registered enemy trackers
    private List<EnemyPatternTracker> _trackers = new List<EnemyPatternTracker>();
    
    // Decay settings
    private const float DECAY_INTERVAL_TICKS = 5f; // Half count every 5 ticks of no use
    private const int MIN_COUNT_TO_TRACK = 1;
    
    // Current global tick (incremented each broadcast)
    private float _currentTick = 0f;
    
    // Track if we've connected to SkillComboSystem
    private bool _connectedToComboSystem = false;
    
    public override void _Ready()
    {
        Instance = this;
        ConnectToComboSystem();
    }
    
    public override void _ExitTree()
    {
        Instance = null;
    }
    
    private void ConnectToComboSystem()
    {
        var scs = SkillComboSystem.Instance;
        if (scs != null)
        {
            scs.ComboCompleted += OnPlayerComboCompleted;
            _connectedToComboSystem = true;
            GD.Print("[EnemyObserverSystem] Connected to SkillComboSystem.ComboCompleted");
        }
        else
        {
            // Retry next frame
            CallDeferred(nameof(ConnectToComboSystem));
        }
    }
    
    private void OnPlayerComboCompleted(string comboId, int streak)
    {
        _currentTick += 1f;
        
        // Update global usage count
        if (!_globalComboUsage.ContainsKey(comboId))
            _globalComboUsage[comboId] = 0;
        
        _globalComboUsage[comboId]++;
        _globalComboLastTick[comboId] = _currentTick;
        
        // Reset decay accumulator for this combo
        _decayAccumulator[comboId] = 0f;
        
        // Notify all registered trackers
        int count = _globalComboUsage[comboId];
        foreach (var tracker in _trackers)
        {
            tracker.OnPlayerComboUsed(comboId, count, _currentTick);
        }
    }
    
    public override void _Process(double delta)
    {
        // Decay combo usage counts over time (when player stops using combos)
        _currentTick += (float)delta;
        
        // Every few seconds, apply decay
        var keys = new List<string>(_globalComboUsage.Keys);
        foreach (var comboId in keys)
        {
            float lastTick = _globalComboLastTick.GetValueOrDefault(comboId, 0f);
            float timeSinceUse = _currentTick - lastTick;
            
            // Accumulate decay
            if (!_decayAccumulator.ContainsKey(comboId))
                _decayAccumulator[comboId] = 0f;
            
            _decayAccumulator[comboId] += (float)delta;
            
            // Every DECAY_INTERVAL_TICKS seconds, halve the count
            if (_decayAccumulator[comboId] >= DECAY_INTERVAL_TICKS)
            {
                _decayAccumulator[comboId] = 0f;
                if (_globalComboUsage[comboId] > MIN_COUNT_TO_TRACK)
                {
                    _globalComboUsage[comboId] = Mathf.Max(MIN_COUNT_TO_TRACK, _globalComboUsage[comboId] / 2);
                    
                    // Notify trackers of decayed count
                    int count = _globalComboUsage[comboId];
                    foreach (var tracker in _trackers)
                    {
                        tracker.OnPlayerComboUsed(comboId, count, _currentTick);
                    }
                    
                    // If below threshold, stop countermeasures
                    if (count < 3)
                    {
                        // Notify trackers that combo is no longer recognized
                        foreach (var tracker in _trackers)
                        {
                            tracker.OnPlayerComboUsed(comboId, count, _currentTick);
                        }
                    }
                }
            }
        }
    }
    
    public void RegisterTracker(EnemyPatternTracker tracker)
    {
        if (!_trackers.Contains(tracker))
            _trackers.Add(tracker);
    }
    
    public void UnregisterTracker(EnemyPatternTracker tracker)
    {
        _trackers.Remove(tracker);
    }
    
    // === Public Query API ===
    
    /// <summary>
    /// Returns how many times player has used this combo globally.
    /// </summary>
    public int GetGlobalComboUsageCount(string comboId)
    {
        return _globalComboUsage.GetValueOrDefault(comboId, 0);
    }
    
    /// <summary>
    /// Returns all combos currently above the threshold.
    /// </summary>
    public List<string> GetRecognizedComboIds()
    {
        var result = new List<string>();
        foreach (var kvp in _globalComboUsage)
        {
            if (kvp.Value >= 3)
                result.Add(kvp.Key);
        }
        return result;
    }
    
    /// <summary>
    /// Returns true if this combo is currently being countered.
    /// </summary>
    public bool IsComboUnderCounter(string comboId)
    {
        return _globalComboUsage.GetValueOrDefault(comboId, 0) >= 3;
    }
}
