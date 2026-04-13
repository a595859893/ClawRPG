using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;
using ClawRPG.Scripts.Combat;
using ClawRPG.Scripts.UI;

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
/// 
/// REQ-181: Ghost Teaching System
/// Completing the same ghost combo multiple times unlocks teaching mode:
/// - 3 completions: Lv.1 hint (0.5s preview)
/// - 7 completions: Lv.2 hint (0.8s preview)
/// - 15 completions: Lv.3 hint (1.0s preview + combo name)
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

    // === REQ-181: Ghost Teaching System ===
    // comboId → number of times player completed this ghost combo
    private Dictionary<string, int> _ghostCompletionCount = new Dictionary<string, int>();
    // comboId → current teaching mode level (0=none, 1=Lv.1, 2=Lv.2, 3=Lv.3)
    private Dictionary<string, int> _teachingModeLevel = new Dictionary<string, int>();

    // Teaching mode thresholds (REQ-181)
    private const int TeachingLv1Threshold = 3;
    private const int TeachingLv2Threshold = 7;
    private const int TeachingLv3Threshold = 15;

    // Teaching hint durations by level (seconds)
    private static readonly float[] TeachingHintDuration = { 0f, 0.5f, 0.8f, 1.0f };

    // Signals
    /// <summary>REQ-181: Fired when a ghost combo appears.</summary>
    public static Action<AbandonedComboEntry> OnGhostAppeared;
    /// <summary>REQ-181: Fired when teaching mode level upgrades. arg0=comboId, arg1=newLevel.</summary>
    public static Action<string, int> OnTeachingModeUpgraded;
    /// <summary>REQ-181: Fired when a teaching hint should be shown. arg0=comboId, arg1=level, arg2=nextSkillName, arg3=durationSeconds.</summary>
    public static Action<string, int, string, float> OnTeachingHintRequested;

    protected override void Initialize()
    {
        Instance = this;
        
        // Subscribe to combo failure/abandonment signals
        SkillComboSystem.ComboFailed += OnComboFailed;

        // Subscribe to combat start — roll for ghost appearance
        CombatStatusSystem.OnCombatStarted += OnCombatStarted;

        // REQ-181: Subscribe to teaching hint requests
        OnTeachingHintRequested += _OnTeachingHintRequested;

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
        // REQ-202 FIX: SkillComboSystem.ComboFailed only fires on TIMEOUT (not wrong-skill).
        // Read from SkillComboSystem data source, not ComboSystem.
        if (SkillComboSystem.Instance == null) return;

        var progress = SkillComboSystem.Instance.GetPlayerProgress();
        if (progress.TryGetValue(comboId, out var p) && p.IsActive) {
            var allCombos = SkillComboDatabase.Instance.GetAllCombos();
            int totalSteps = 0;
            if (allCombos.TryGetValue(comboId, out var comboData)) {
                totalSteps = comboData?.SkillIds?.Count ?? 0;
            }
            RecordAbandonedCombo(comboId, p.CurrentStep, totalSteps, AbandonmentType.Timeout);
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

        // REQ-175: Always show muscle memory flash (no RNG), after ghost roll
        _SpawnMuscleMemoryFlash();
    }

    /// <summary>
    /// REQ-174: Advance to the next ghost in the queue.
    /// Called when the player completes a ghost combo or presses a key.
    /// REQ-180: Awards 1 conviction point for completing a ghost combo.
    /// REQ-181: Increments ghost completion count and updates teaching mode level.
    /// </summary>
    public void AdvanceGhost()
    {
        if (_activeGhosts.Count == 0) return;

        var completedGhost = _activeGhosts[_currentGhostIndex];
        string completedComboId = completedGhost?.ComboId ?? string.Empty;

        // Remove the completed ghost from history (it was completed, not abandoned)
        _abandonedHistory.Remove(completedGhost);

        // REQ-181: Update teaching mode — increment completion count and check threshold
        if (!string.IsNullOrEmpty(completedComboId))
        {
            _IncrementGhostCompletion(completedComboId);
        }

        // REQ-180: Award conviction point for completing a ghost combo
        GhostConvictionSystem.Instance?.AddConvictionPoint();

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

        // REQ-181: If teaching mode is active for this combo, fire the hint request
        string comboId = ghost?.ComboId ?? string.Empty;
        if (!string.IsNullOrEmpty(comboId))
        {
            int level = GetTeachingModeLevel(comboId);
            if (level > 0)
            {
                string nextSkill = _GetNextSkillNameForCombo(comboId, ghost.AbandonedAtStep);
                float duration = TeachingHintDuration[level];
                OnTeachingHintRequested?.Invoke(comboId, level, nextSkill, duration);
                GD.Print($"[ComboGhostSystem] Teaching hint requested: {comboId} Lv.{level}, skill={nextSkill}, duration={duration}s");
            }
        }
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

    /// <summary>
    /// REQ-182: Check if a ghost is currently active for the given comboId.
    /// Used by ComboIntentDisplay to decide whether to show ghost narrative or rune flash.
    /// </summary>
    public bool ShouldShowGhostForCombo(string comboId)
    {
        if (string.IsNullOrEmpty(comboId)) return false;
        foreach (var ghost in _activeGhosts)
        {
            if (ghost != null && ghost.ComboId == comboId) return true;
        }
        return false;
    }

    /// <summary>
    /// REQ-175: Get the most recently abandoned combo entry.
    /// Returns null if no combo has been abandoned yet.
    /// Used by ComboMuscleMemoryEffect to always show the flash (no RNG).
    /// </summary>
    public AbandonedComboEntry GetLastAbandonedCombo()
    {
        if (_abandonedHistory == null || _abandonedHistory.Count == 0) return null;
        return _abandonedHistory[0];
    }

    // REQ-175: Muscle memory effect instance
    private ComboMuscleMemoryEffect _muscleMemoryEffect;

    // REQ-181: Teaching hint panel instance
    private GhostTeachingHintPanel _teachingHintPanel;

    /// <summary>
    /// REQ-175: Spawn a muscle memory flash for the last abandoned combo.
    /// Called from OnCombatStarted AFTER the ghost roll, ensuring the flash
    /// always shows (no RNG) alongside REQ-174's probabilistic ghost.
    /// </summary>
    private void _SpawnMuscleMemoryFlash()
    {
        var lastAbandoned = GetLastAbandonedCombo();
        if (lastAbandoned == null) return;

        // Spawn the effect as a direct child of root (fullscreen overlay)
        var tree = GetTree();
        if (tree == null) return;

        var root = tree.Root;
        if (root == null) return;

        // Reuse existing effect or create new one
        if (_muscleMemoryEffect != null && IsInstanceValid(_muscleMemoryEffect))
        {
            _muscleMemoryEffect.QueueFree();
        }

        _muscleMemoryEffect = new ComboMuscleMemoryEffect();
        _muscleMemoryEffect._Trigger(lastAbandoned);
        root.AddChild(_muscleMemoryEffect);
    }

    // REQ-181: Handler for teaching hint signal — spawns the hint panel
    private void _OnTeachingHintRequested(string comboId, int level, string skillName, float duration)
    {
        if (level <= 0 || duration <= 0f) return;

        var tree = GetTree();
        if (tree == null) return;
        var root = tree.Root;
        if (root == null) return;

        // Reuse existing panel
        if (_teachingHintPanel != null && IsInstanceValid(_teachingHintPanel))
        {
            _teachingHintPanel.QueueFree();
        }

        _teachingHintPanel = new GhostTeachingHintPanel();
        _teachingHintPanel.ShowHint(comboId, level, skillName, duration);
        root.AddChild(_teachingHintPanel);
    }

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

    // === REQ-181: Teaching Mode Helpers ===

    /// <summary>
    /// REQ-181: Increment the ghost completion count for a combo and check for level-up.
    /// Called from AdvanceGhost() when a ghost combo is completed.
    /// </summary>
    private void _IncrementGhostCompletion(string comboId)
    {
        if (!_ghostCompletionCount.ContainsKey(comboId))
            _ghostCompletionCount[comboId] = 0;

        _ghostCompletionCount[comboId]++;
        int count = _ghostCompletionCount[comboId];

        int oldLevel = GetTeachingModeLevel(comboId);
        int newLevel = _CalculateTeachingLevel(count);

        GD.Print($"[ComboGhostSystem] Ghost completed: {comboId} (x{count}), teaching level: {oldLevel} → {newLevel}");

        if (newLevel > oldLevel)
        {
            _teachingModeLevel[comboId] = newLevel;
            OnTeachingModeUpgraded?.Invoke(comboId, newLevel);
            GD.Print($"[ComboGhostSystem] Teaching mode upgraded! {comboId} → Lv.{newLevel}");
        }
    }

    /// <summary>
    /// REQ-181: Calculate teaching level from completion count.
    /// </summary>
    private int _CalculateTeachingLevel(int completionCount)
    {
        if (completionCount >= TeachingLv3Threshold) return 3;
        if (completionCount >= TeachingLv2Threshold) return 2;
        if (completionCount >= TeachingLv1Threshold) return 1;
        return 0;
    }

    /// <summary>
    /// REQ-181: Get the current teaching mode level for a combo (0–3).
    /// </summary>
    public int GetTeachingModeLevel(string comboId)
    {
        if (_teachingModeLevel.TryGetValue(comboId, out int level))
            return level;
        return 0;
    }

    /// <summary>
    /// REQ-181: Get how many completions until next teaching level.
    /// Returns 0 if at max level.
    /// </summary>
    public int GetCompletionsToNextLevel(string comboId)
    {
        int count = _ghostCompletionCount.TryGetValue(comboId, out var c) ? c : 0;
        int level = GetTeachingModeLevel(comboId);
        if (level >= 3) return 0;
        if (level == 2) return TeachingLv3Threshold - count;
        if (level == 1) return TeachingLv2Threshold - count;
        return TeachingLv1Threshold - count;
    }

    /// <summary>
    /// REQ-181: Get the next skill name for a combo at a given step.
    /// Used for teaching hint display.
    /// </summary>
    private string _GetNextSkillNameForCombo(string comboId, int currentStep)
    {
        // REQ-202 FIX: Use SkillComboDatabase instead of ComboSystem
        try
        {
            if (SkillComboDatabase.Instance != null)
            {
                var allCombos = SkillComboDatabase.Instance.GetAllCombos();
                if (allCombos.TryGetValue(comboId, out var comboData)
                    && comboData?.SkillIds != null
                    && currentStep < comboData.SkillIds.Count)
                {
                    return comboData.SkillIds[currentStep] ?? $"Step {currentStep + 1}";
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[ComboGhostSystem] Failed to get next skill name: {ex.Message}");
        }

        return comboId; // Fallback: show combo ID
    }

    // === REQ-181: Persistence ===

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = base.ExportSaveData();

        // Serialize ghost completion counts
        var counts = new Dictionary<string, object>();
        foreach (var kvp in _ghostCompletionCount)
            counts[kvp.Key] = kvp.Value;

        // Serialize teaching mode levels
        var levels = new Dictionary<string, object>();
        foreach (var kvp in _teachingModeLevel)
            levels[kvp.Key] = kvp.Value;

        data["ghostCompletionCount"] = counts;
        data["teachingModeLevel"] = levels;

        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        base.ImportSaveData(data);
        if (data == null) return;

        if (data.TryGetValue("ghostCompletionCount", out var countsObj) && countsObj is Dictionary<string, object> counts)
        {
            _ghostCompletionCount.Clear();
            foreach (var kvp in counts)
            {
                if (kvp.Value is int i)
                    _ghostCompletionCount[kvp.Key] = i;
            }
        }

        if (data.TryGetValue("teachingModeLevel", out var levelsObj) && levelsObj is Dictionary<string, object> levels)
        {
            _teachingModeLevel.Clear();
            foreach (var kvp in levels)
            {
                if (kvp.Value is int i)
                    _teachingModeLevel[kvp.Key] = i;
            }
        }

        GD.Print($"[ComboGhostSystem] Loaded teaching data: {_ghostCompletionCount.Count} combos tracked, " +
                 $"{_teachingModeLevel.Count} with teaching levels.");
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
