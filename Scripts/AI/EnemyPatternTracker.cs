using Godot;
using System.Collections.Generic;

/// <summary>
/// Per-enemy component that tracks player combo usage patterns and applies countermeasures.
/// Attached to each Enemy instance. Queried by the enemy's behavior tree for countermeasure decisions.
/// </summary>
public partial class EnemyPatternTracker : Node
{
    private Enemy _enemy;
    
    // Pattern state for this enemy
    private Dictionary<string, int> _comboUsageCounts = new Dictionary<string, int>();
    private Dictionary<string, float> _comboLastUsedTick = new Dictionary<string, float>();
    private Dictionary<string, float> _comboCountermeasureStrength = new Dictionary<string, float>();
    
    // Config
    private const int THRESHOLD = 3;
    private const float DECAY_HALF_LIFE_SECONDS = 15f;
    private const float MAX_COUNTERMEASURE_STRENGTH = 1.0f;
    
    // Current active countermeasures
    private float _defenseBuff = 0f;
    private float _evasionChance = 0f;
    private float _counterAttackChance = 0f;
    private string _recognizedComboId = "";
    private bool _isInCounterMode = false;
    
    // Visual feedback
    private bool _visualAlertActive = false;
    
    public override void _Ready()
    {
        _enemy = GetOwner<Enemy>();
        EnemyObserverSystem.Instance.RegisterTracker(this);
    }
    
    public override void _ExitTree()
    {
        if (EnemyObserverSystem.Instance != null)
            EnemyObserverSystem.Instance.UnregisterTracker(this);
    }
    
    public override void _Process(double delta)
    {
        // Decay countermeasures over time
        bool changed = false;
        var keys = new List<string>(_comboCountermeasureStrength.Keys);
        foreach (var comboId in keys)
        {
            float lastTick = _comboLastUsedTick.GetValueOrDefault(comboId, 0f);
            float elapsed = (float)delta; // We track by delta seconds from last use
            // Simple time-based decay handled via OnPlayerComboUsed callback
            _ = elapsed; // Suppress unused warning - decay is tick-based from ObserverSystem
        }
        
        // Decay active countermeasures
        if (_defenseBuff > 0f) { _defenseBuff = Mathf.Max(0f, _defenseBuff - (float)delta * 0.05f); changed = true; }
        if (_evasionChance > 0f) { _evasionChance = Mathf.Max(0f, _evasionChance - (float)delta * 0.03f); changed = true; }
        if (_counterAttackChance > 0f) { _counterAttackChance = Mathf.Max(0f, _counterAttackChance - (float)delta * 0.04f); changed = true; }
        
        if (_defenseBuff <= 0.01f && _evasionChance <= 0.01f && _counterAttackChance <= 0.01f && _isInCounterMode)
        {
            _isInCounterMode = false;
            DeactivateCounterVisuals();
        }
    }
    
    /// <summary>
    /// Called by EnemyObserverSystem when a player combo is completed.
    /// </summary>
    public void OnPlayerComboUsed(string comboId, int usageCount, float currentTick)
    {
        if (!_comboUsageCounts.ContainsKey(comboId))
            _comboUsageCounts[comboId] = 0;
        
        _comboUsageCounts[comboId] = usageCount;
        _comboLastUsedTick[comboId] = currentTick;
        
        if (usageCount >= THRESHOLD)
        {
            // Activate countermeasures
            ActivateCountermeasures(comboId, usageCount, currentTick);
        }
    }
    
    private void ActivateCountermeasures(string comboId, int usageCount, float currentTick)
    {
        // Strength scales with usage count (capped)
        float excess = Mathf.Min(usageCount - THRESHOLD, 5);
        float strength = Mathf.Clamp(0.3f + excess * 0.1f, 0.3f, 0.8f);
        
        _comboCountermeasureStrength[comboId] = strength;
        _defenseBuff = strength * 0.3f;      // up to +30% defense
        _evasionChance = strength * 0.2f;    // up to 20% evasion
        _counterAttackChance = strength * 0.15f; // up to 15% counter attack
        _recognizedComboId = comboId;
        _isInCounterMode = true;
        
        ActivateCounterVisuals();
        
        // Notify enemy AI that we've recognized a pattern
        _enemy.NotifyPatternRecognized(comboId, strength);
    }
    
    private void ActivateCounterVisuals()
    {
        if (_enemy == null) return;
        // Emit a visual signal - the enemy's sprite can listen for this
        // Using a method call for simplicity
        _enemy.SetCounterModeActive(true);
    }
    
    private void DeactivateCounterVisuals()
    {
        if (_enemy == null) return;
        _enemy.SetCounterModeActive(false);
    }
    
    // === Public query API for Behavior Tree ===
    
    public bool IsInCounterMode => _isInCounterMode;
    
    public float GetDefenseBuff() => _defenseBuff;
    
    public float GetEvasionChance() => _evasionChance;
    
    public float GetCounterAttackChance() => _counterAttackChance;
    
    public string GetRecognizedComboId() => _recognizedComboId;
    
    /// <summary>
    /// Returns true if player recently used the given combo more than threshold times.
    /// </summary>
    public bool IsComboRecognized(string comboId)
    {
        return _comboUsageCounts.GetValueOrDefault(comboId, 0) >= THRESHOLD;
    }
    
    /// <summary>
    /// Get all combos that are currently under countermeasure.
    /// </summary>
    public List<string> GetRecognizedCombos()
    {
        var result = new List<string>();
        foreach (var kvp in _comboUsageCounts)
        {
            if (kvp.Value >= THRESHOLD)
                result.Add(kvp.Key);
        }
        return result;
    }
    
    /// <summary>
    /// Called when this enemy is hit by player - check if we should counter
    /// </summary>
    public bool ShouldCounterAttack()
    {
        if (!_isInCounterMode) return false;
        return GD.Randf() < _counterAttackChance;
    }
    
    /// <summary>
    /// Called during damage calculation - applies defense buff
    /// </summary>
    public float ApplyCounterDefense(float baseDamage)
    {
        if (_defenseBuff <= 0f) return baseDamage;
        return baseDamage * (1f - _defenseBuff);
    }
}
