using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;
using ClawRPG.Scripts.Combat;

/// <summary>
/// Combo Ghost System (REQ-174)
/// 
/// Records abandoned combo sequences and triggers "ghost" echoes in subsequent combats.
/// An abandoned combo has a chance to appear as a visual ghost (desaturated, no damage).
/// 
/// Abandonment scenarios and ghost probabilities:
/// - Timeout: 80%
/// - Wrong skill: 60%
/// - Manual cancel: 40%
/// - Died mid-combo: 30%
/// 
/// Ghost probability scales with how far the combo progressed (abandonedAtStep / totalSteps).
/// </summary>
public partial class ComboGhostSystem : BaseSystem
{
    public new static ComboGhostSystem Instance { get; private set; }

    // REQ-174: Abandoned combo history
    private List<AbandonedComboEntry> _abandonedHistory = new List<AbandonedComboEntry>();
    private const int MaxHistorySize = 3;

    // REQ-174: Currently active ghost (shown in current combat)
    private List<AbandonedComboEntry> _activeGhosts = new List<AbandonedComboEntry>();
    private int _currentGhostIndex = 0;

    // Signal: fired when a ghost combo appears
    public static Action<AbandonedComboEntry> OnGhostAppeared;

    protected override void Initialize()
    {
        Instance = this;
        
        // Subscribe to combo failure/abandonment signals
        ComboSystem.ComboFailed += OnComboFailed;
        
        // Subscribe to combat start — roll for ghost appearance
        CombatStatusSystem.OnCombatStarted += OnCombatStarted;
        
        GD.Print("[ComboGhostSystem] Initialized");
    }

    /// <summary>
    /// REQ-174: Record an abandoned combo and roll for ghost in next combat.
    /// AbandonmentType: Timeout=80%, WrongSkill=60%, ManualCancel=40%, Died=30%
    /// </summary>
    public void RecordAbandonedCombo(string comboId, int abandonedAtStep, int totalSteps, AbandonmentType type)
    {
        if (totalSteps == 0) return;
        
        float progress = (float)abandonedAtStep / totalSteps;
        float baseProbability = GetBaseProbability(type);
        float ghostProbability = baseProbability * progress; // Scale by progress
        
        var entry = new AbandonedComboEntry {
            ComboId = comboId,
            AbandonedAtStep = abandonedAtStep,
            TotalSteps = totalSteps,
            ProgressRatio = progress,
            AbandonmentType = type,
            GhostProbability = ghostProbability,
            AbandonedTimestamp = DateTime.Now
        };

        _abandonedHistory.Insert(0, entry);
        
        // Keep only last MaxHistorySize entries
        while (_abandonedHistory.Count > MaxHistorySize) {
            _abandonedHistory.RemoveAt(_abandonedHistory.Count - 1);
        }
        
        GD.Print($"[ComboGhostSystem] Recorded abandoned combo: {comboId} at {abandonedAtStep}/{totalSteps} " +
                 $"(type={type}, prob={ghostProbability:P0})");
    }

    private void OnComboFailed(string comboId)
    {
        // Get the combo's current progress
        if (ComboSystem.Instance == null) return;
        
        var progress = ComboSystem.Instance.GetPlayerProgress();
        if (progress.TryGetValue(comboId, out var p) && p.isActive) {
            var allCombos = ComboSystem.Instance.GetAllCombos();
            int totalSteps = 0;
            if (allCombos.TryGetValue(comboId, out var comboData)) {
                totalSteps = comboData?.SkillSequence?.Count ?? 0;
            }
            RecordAbandonedCombo(comboId, p.currentStep, totalSteps, AbandonmentType.WrongSkill);
        }
    }

    private void OnCombatStarted()
    {
        // Roll for ghost appearance for each abandoned combo
        _activeGhosts.Clear();
        _currentGhostIndex = 0;
        
        foreach (var entry in _abandonedHistory) {
            if (entry == null) continue;
            
            // Roll the dice
            float roll = (float)GD.Randf(); // 0..1
            if (roll < entry.GhostProbability) {
                _activeGhosts.Add(entry);
                OnGhostAppeared?.Invoke(entry);
                GD.Print($"[ComboGhostSystem] Ghost appeared for {entry.ComboId}! " +
                         $"(rolled {roll:P0} vs prob {entry.GhostProbability:P0})");
            }
        }
        
        if (_activeGhosts.Count > 0) {
            // Show the first ghost
            ShowCurrentGhost();
        }
    }

    /// <summary>
    /// REQ-174: Advance to the next ghost in the queue.
    /// Called when the player completes a ghost combo or presses a key.
    /// </summary>
    public void AdvanceGhost()
    {
        if (_activeGhosts.Count == 0) return;
        
        // Remove the completed ghost from history (it was completed, not abandoned)
        _abandonedHistory.Remove(_activeGhosts[_currentGhostIndex]);
        
        _currentGhostIndex++;
        if (_currentGhostIndex >= _activeGhosts.Count) {
            _activeGhosts.Clear();
            _currentGhostIndex = 0;
        } else {
            ShowCurrentGhost();
        }
    }

    private void ShowCurrentGhost()
    {
        if (_activeGhosts.Count == 0 || _currentGhostIndex >= _activeGhosts.Count) return;
        var ghost = _activeGhosts[_currentGhostIndex];
        
        // Tell the ComboIntentDisplay to show in ghost mode
        // This is handled via signal/callback
        OnGhostAppeared?.Invoke(ghost);
    }

    /// <summary>
    /// Get the current ghost entry for UI display.
    /// </summary>
    public AbandonedComboEntry GetCurrentGhost()
    {
        if (_activeGhosts.Count == 0 || _currentGhostIndex >= _activeGhosts.Count) return null;
        return _activeGhosts[_currentGhostIndex];
    }

    public bool HasActiveGhosts() => _activeGhosts.Count > 0;

    private float GetBaseProbability(AbandonmentType type)
    {
        return type switch {
            AbandonmentType.Timeout => 0.80f,
            AbandonmentType.WrongSkill => 0.60f,
            AbandonmentType.ManualCancel => 0.40f,
            AbandonmentType.Died => 0.30f,
            _ => 0f
        };
    }


}

/// <summary>
/// REQ-174: Entry for a recorded abandoned combo.
/// </summary>
public class AbandonedComboEntry
{
    public string ComboId { get; set; }
    public int AbandonedAtStep { get; set; }
    public int TotalSteps { get; set; }
    public float ProgressRatio { get; set; }
    public AbandonmentType AbandonmentType { get; set; }
    public float GhostProbability { get; set; }
    public DateTime AbandonedTimestamp { get; set; }
}

public enum AbandonmentType
{
    Timeout,
    WrongSkill,
    ManualCancel,
    Died
}
