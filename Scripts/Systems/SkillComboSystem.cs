using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 技能Combo系统。管理技能连击的检测、触发和奖励计算。
/// </summary>
public partial class SkillComboSystem : BaseSystem
{
    /// <summary>
    /// 获取系统单例实例。
    /// </summary>
    public static SkillComboSystem Instance { get; private set; }
    
    // Active combo tracking
    private Dictionary<string, ActiveCombo> _activeCombos;
    private List<string> _activeSkillQueue;
    private float _lastSkillTime;
    
    // Current bonuses applied
    private float _currentDamageMultiplier = 1f;
    private float _currentCritBonus = 0f;
    private float _currentCooldownReduction = 0f;
    private float _bonusEndTime;
    
    // Player data
    private PlayerComboData _playerData;
    
    // Signals
    public Signal<string> ComboStarted { get; } = new Signal<string>();
    public Signal<string, int> ComboCompleted { get; } = new Signal<string, int>();
    public Signal<string, float> ComboBonusApplied { get; } = new Signal<string, float>();
    public Signal<string> ComboFailed { get; } = new Signal<string>();
    
    public override void _Ready()
    {
        Instance = this;
        _activeCombos = new Dictionary<string, ActiveCombo>();
        _activeSkillQueue = new List<string>();
        _playerData = new PlayerComboData();
    }
    
    public override void _Process(double delta)
    {
        float currentTime = Time.GetTicksMsec() / 1000f;
        
        // Check combo timeouts
        CheckComboTimeouts(currentTime);
        
        // Update bonus expiration
        if (_bonusEndTime > 0 && currentTime > _bonusEndTime)
        {
            ResetBonuses();
        }
    }
    
    // Record skill usage for combo detection
    public void RecordSkillUse(string skillId)
    {
        float currentTime = Time.GetTicksMsec() / 1000f;
        
        // Add to skill queue
        _activeSkillQueue.Add(skillId);
        _lastSkillTime = currentTime;
        
        // Check for combos
        CheckComboActivation(skillId, currentTime);
        
        // Limit queue size
        if (_activeSkillQueue.Count > 10)
        {
            _activeSkillQueue.RemoveAt(0);
        }
    }
    
    private void CheckComboActivation(string skillId, float currentTime)
    {
        var allCombos = SkillComboDatabase.Instance.GetAllCombos();
        
        foreach (var combo in allCombos)
        {
            if (CanTriggerCombo(combo, skillId, currentTime))
            {
                TriggerCombo(combo, currentTime);
            }
        }
    }
    
    private bool CanTriggerCombo(SkillCombo combo, string newSkillId, float currentTime)
    {
        // Check if skill is part of this combo
        if (!combo.SkillIds.Contains(newSkillId))
            return false;
        
        // Check time window
        float timeSinceLastSkill = currentTime - _lastSkillTime;
        if (timeSinceLastSkill > combo.TimeWindow)
            return false;
        
        // Check trigger conditions
        switch (combo.Trigger)
        {
            case ComboTrigger.SameElement:
                return CheckSameElementCombo(combo);
            case ComboTrigger.DifferentType:
                return CheckDifferentTypeCombo(combo);
            case ComboTrigger.TimeWindow:
            case ComboTrigger.Any:
                return true;
        }
        
        return false;
    }
    
    private bool CheckSameElementCombo(SkillCombo combo)
    {
        // Simplified - in real implementation would check skill elements
        if (_activeSkillQueue.Count < 2) return false;
        
        string lastSkill = _activeSkillQueue[_activeSkillQueue.Count - 1];
        string secondLastSkill = _activeSkillQueue[_activeSkillQueue.Count - 2];
        
        // Check if both skills are in the same combo
        return combo.SkillIds.Contains(lastSkill) && combo.SkillIds.Contains(secondLastSkill);
    }
    
    private bool CheckDifferentTypeCombo(SkillCombo combo)
    {
        // Check if we have skills from different types
        if (_activeSkillQueue.Count < 2) return false;
        
        // Simple check - any two different skills in combo
        var recentSkills = _activeSkillQueue.GetRange(
            Math.Max(0, _activeSkillQueue.Count - 2), 
            Math.Min(2, _activeSkillQueue.Count));
        
        return recentSkills[0] != recentSkills[1] && combo.SkillIds.Contains(recentSkills[0]);
    }
    
    private void TriggerCombo(SkillCombo combo, float currentTime)
    {
        string comboId = combo.ComboId;
        
        // Create or update active combo
        if (!_activeCombos.ContainsKey(comboId))
        {
            _activeCombos[comboId] = new ActiveCombo
            {
                ComboId = comboId,
                StartTime = currentTime,
                TriggeredSkills = new List<string>(),
                CurrentStreak = 0,
                IsComplete = false
            };
            ComboStarted.Emit(comboId);
        }
        
        var activeCombo = _activeCombos[comboId];
        activeCombo.TriggeredSkills.Add(_activeSkillQueue[_activeSkillQueue.Count - 1]);
        activeCombo.CurrentStreak++;
        
        // Check if combo is complete
        if (activeCombo.CurrentStreak >= combo.Bonus.RequiredComboCount)
        {
            CompleteCombo(combo, activeCombo);
        }
        
        // Update player data
        if (!_playerData.ComboUsageCount.ContainsKey(comboId))
            _playerData.ComboUsageCount[comboId] = 0;
        _playerData.ComboUsageCount[comboId]++;
        
        // Track best streak
        if (!_playerData.ComboStreakBest.ContainsKey(comboId) || 
            activeCombo.CurrentStreak > _playerData.ComboStreakBest[comboId])
        {
            _playerData.ComboStreakBest[comboId] = activeCombo.CurrentStreak;
        }
        
        // Discover combo
        if (!_playerData.DiscoveredCombos.Contains(comboId))
        {
            _playerData.DiscoveredCombos.Add(comboId);
        }
    }
    
    private void CompleteCombo(SkillCombo combo, ActiveCombo activeCombo)
    {
        if (activeCombo.IsComplete) return;
        
        activeCombo.IsComplete = true;
        _playerData.TotalCombosTriggered++;
        
        // Apply combo bonus
        ApplyComboBonus(combo.Bonus);
        
        ComboCompleted.Emit(combo.ComboId, activeCombo.CurrentStreak);
        
        // Reset after duration
        activeCombo.CurrentStreak = 0;
        activeCombo.TriggeredSkills.Clear();
    }
    
    private void ApplyComboBonus(ComboBonus bonus)
    {
        float currentTime = Time.GetTicksMsec() / 1000f;
        
        // Apply damage multiplier
        if (bonus.DamageMultiplier > 1f)
        {
            _currentDamageMultiplier = Math.Max(_currentDamageMultiplier, bonus.DamageMultiplier);
        }
        
        // Apply cooldown reduction (simplified)
        _currentCooldownReduction = Math.Max(_currentCooldownReduction, bonus.CooldownReduction);
        
        // Set bonus expiration
        _bonusEndTime = currentTime + bonus.Duration;
        
        ComboBonusApplied.Emit(bonus.Name, bonus.DamageMultiplier);
    }
    
    private void CheckComboTimeouts(float currentTime)
    {
        List<string> expiredCombos = new List<string>();
        
        foreach (var kvp in _activeCombos)
        {
            float timeSinceStart = currentTime - kvp.Value.StartTime;
            var combo = SkillComboDatabase.Instance.GetCombo(kvp.Key);
            
            if (combo != null && timeSinceStart > combo.TimeWindow * 2)
            {
                if (!kvp.Value.IsComplete && kvp.Value.CurrentStreak > 0)
                {
                    ComboFailed.Emit(kvp.Key);
                }
                expiredCombos.Add(kvp.Key);
            }
        }
        
        foreach (string comboId in expiredCombos)
        {
            _activeCombos.Remove(comboId);
        }
    }
    
    private void ResetBonuses()
    {
        _currentDamageMultiplier = 1f;
        _currentCritBonus = 0f;
        _currentCooldownReduction = 0f;
        _bonusEndTime = 0;
    }
    
    // Public getters for combat integration
    public float GetDamageMultiplier() => _currentDamageMultiplier;
    public float GetCritBonus() => _currentCritBonus;
    public float GetCooldownReduction() => _currentCooldownReduction;
    public bool HasActiveBonus() => _bonusEndTime > 0;
    
    // Statistics
    public PlayerComboData GetStatistics() => _playerData;
    
    public int GetTotalCombosTriggered() => _playerData.TotalCombosTriggered;
    public int GetDiscoveredComboCount() => _playerData.DiscoveredCombos.Count;
    public int GetComboUsageCount(string comboId) => 
        _playerData.ComboUsageCount.ContainsKey(comboId) ? _playerData.ComboUsageCount[comboId] : 0;
    
    // Save/Load support
    public Dictionary<string, object> Save()
    {
        return new Dictionary<string, object>
        {
            { "comboUsageCount", _playerData.ComboUsageCount },
            { "comboStreakBest", _playerData.ComboStreakBest },
            { "totalComboDamage", _playerData.TotalComboDamage },
            { "totalCombosTriggered", _playerData.TotalCombosTriggered },
            { "discoveredCombos", _playerData.DiscoveredCombos }
        };
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("comboUsageCount"))
            _playerData.ComboUsageCount = (Dictionary<string, int>)data["comboUsageCount"];
        if (data.ContainsKey("comboStreakBest"))
            _playerData.ComboStreakBest = (Dictionary<string, int>)data["comboStreakBest"];
        if (data.ContainsKey("totalComboDamage"))
            _playerData.TotalComboDamage = Convert.ToSingle(data["totalComboDamage"]);
        if (data.ContainsKey("totalCombosTriggered"))
            _playerData.TotalCombosTriggered = Convert.ToInt32(data["totalCombosTriggered"]);
        if (data.ContainsKey("discoveredCombos"))
            _playerData.DiscoveredCombos = (List<string>)data["discoveredCombos"];
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // Combo usage count
        var usageCount = new Dictionary();
        foreach (var kvp in _playerData.ComboUsageCount)
        {
            usageCount[kvp.Key] = kvp.Value;
        }
        data["combo_usage_count"] = usageCount;
        
        // Combo streak best
        var streakBest = new Dictionary();
        foreach (var kvp in _playerData.ComboStreakBest)
        {
            streakBest[kvp.Key] = kvp.Value;
        }
        data["combo_streak_best"] = streakBest;
        
        data["total_combo_damage"] = _playerData.TotalComboDamage;
        data["total_combos_triggered"] = _playerData.TotalCombosTriggered;
        
        // Discovered combos
        var discoveredCombos = new Array();
        foreach (var combo in _playerData.DiscoveredCombos)
        {
            discoveredCombos.Add(combo);
        }
        data["discovered_combos"] = discoveredCombos;
        
        return data;
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("combo_usage_count"))
        {
            var usageCount = (Dictionary)data["combo_usage_count"];
            _playerData.ComboUsageCount = new Dictionary<string, int>();
            foreach (var kvp in usageCount)
            {
                _playerData.ComboUsageCount[kvp.Key] = (int)kvp.Value;
            }
        }
        
        if (data.Contains("combo_streak_best"))
        {
            var streakBest = (Dictionary)data["combo_streak_best"];
            _playerData.ComboStreakBest = new Dictionary<string, int>();
            foreach (var kvp in streakBest)
            {
                _playerData.ComboStreakBest[kvp.Key] = (int)kvp.Value;
            }
        }
        
        if (data.Contains("total_combo_damage")) _playerData.TotalComboDamage = (float)data["total_combo_damage"];
        if (data.Contains("total_combos_triggered")) _playerData.TotalCombosTriggered = (int)data["total_combos_triggered"];
        
        if (data.Contains("discovered_combos"))
        {
            var discoveredCombos = (Array)data["discovered_combos"];
            _playerData.DiscoveredCombos = new List<string>();
            foreach (string combo in discoveredCombos)
            {
                _playerData.DiscoveredCombos.Add(combo);
            }
        }
    }
}
