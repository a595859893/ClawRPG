using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Combo Fatigue System (REQ-120 + REQ-179)
/// 
/// Tracks combo usage and calculates fatigue levels per pet.
/// When a player repeatedly uses the same combo, the pet becomes "fatigued"
/// and the combo becomes less effective (up to 50% damage reduction).
/// 
/// Integration:
/// - SkillComboSystem.CompleteCombo() calls RecordComboUsage()
/// - UI queries GetFatigueLevel(petId) to display ★★☆ stars
/// 
/// Fatigue levels: 0-33% → ★☆☆ | 34-66% → ★★☆ | 67-100% → ★★★
/// </summary>
public partial class ComboFatigueSystem : BaseSystem
{
    private static ComboFatigueSystem _instance;
    public static new ComboFatigueSystem Instance { get { return _instance; } set { _instance = value; } }

    // Per-pet combo usage history (circular buffer of recent combo IDs)
    // Key: petId, Value: list of recently used combo IDs
    private Dictionary<string, List<string>> _petComboHistory;

    // Per-pet combo adaptation levels (0.0 = fresh, 1.0 = max fatigue)
    // Key: petId, Value: dictionary of comboId → adaptation level
    private Dictionary<string, Dictionary<string, float>> _petComboAdaptation;

    // Configuration
    private const int HistoryWindowSize = 10;       // How many recent combos to track
    private const float AdaptationPerUse = 0.15f;  // Adaptation increase per repeated use
    private const float DecayRate = 0.90f;         // Decay multiplier when switching combos
    private const float MaxAdaptation = 1.0f;       // Cap at 100% adaptation
    private const float MinDamageMultiplier = 0.5f; // Maximum damage reduction (50% at full fatigue)

    // Fatigue level thresholds (for UI stars)
    private const float FatigueLevel1Threshold = 0.33f; // ★★☆
    private const float FatigueLevel2Threshold = 0.66f; // ★★★

    // Signal: fired when a pet's fatigue level changes
    public static Action<string, float> OnFatigueChanged; // petId, newFatigueLevel

    public override void _Ready()
    {
        Instance = this;
        _petComboHistory = new Dictionary<string, List<string>>();
        _petComboAdaptation = new Dictionary<string, Dictionary<string, float>>();
        GD.Print("[ComboFatigueSystem] Initialized");
    }

    /// <summary>
    /// REQ-120: Record a combo usage for a pet.
    /// Called by SkillComboSystem.CompleteCombo() when a combo finishes.
    /// </summary>
    public void RecordComboUsage(string comboId, string petId = "")
    {
        // Default to active pet if no petId provided
        if (string.IsNullOrEmpty(petId))
        {
            petId = GetActivePetId();
        }

        if (string.IsNullOrEmpty(petId))
            return;

        // Ensure structures exist
        if (!_petComboHistory.ContainsKey(petId))
            _petComboHistory[petId] = new List<string>();
        if (!_petComboAdaptation.ContainsKey(petId))
            _petComboAdaptation[petId] = new Dictionary<string, float>();

        var history = _petComboHistory[petId];
        var adaptation = _petComboAdaptation[petId];

        // Add to history
        history.Add(comboId);

        // Trim history to window size
        while (history.Count > HistoryWindowSize)
            history.RemoveAt(0);

        // Check if this is a repeat of the most recent combo
        if (history.Count >= 2 && history[history.Count - 2] == comboId)
        {
            // Same as previous - increase adaptation
            if (!adaptation.ContainsKey(comboId))
                adaptation[comboId] = 0f;

            adaptation[comboId] = Math.Min(MaxAdaptation, adaptation[comboId] + AdaptationPerUse);
        }
        else
        {
            // Different combo - decay other adaptations
            foreach (var key in adaptation.Keys)
            {
                if (key != comboId)
                {
                    adaptation[key] *= DecayRate;
                    if (adaptation[key] < 0.01f)
                        adaptation.Remove(key);
                }
            }

            // Start tracking new combo if not already
            if (!adaptation.ContainsKey(comboId))
                adaptation[comboId] = 0f;
        }

        float currentFatigue = GetFatigueLevelInternal(petId, comboId);
        OnFatigueChanged?.Invoke(petId, currentFatigue);

        GD.Print($"[ComboFatigueSystem] Recorded combo '{comboId}' for pet '{petId}', fatigue: {currentFatigue:P0}");
    }

    /// <summary>
    /// REQ-179: Get the current fatigue level for a pet's active combo (0.0 - 1.0).
    /// Returns 0.0 if no fatigue data exists.
    /// </summary>
    public float GetFatigueLevel(string petId = "")
    {
        if (string.IsNullOrEmpty(petId))
            petId = GetActivePetId();

        if (string.IsNullOrEmpty(petId))
            return 0f;

        if (!_petComboHistory.ContainsKey(petId) || _petComboHistory[petId].Count == 0)
            return 0f;

        // Get most recent combo
        string lastCombo = _petComboHistory[petId][_petComboHistory[petId].Count - 1];
        return GetFatigueLevelInternal(petId, lastCombo);
    }

    /// <summary>
    /// Internal: get adaptation level for a specific pet+combo.
    /// </summary>
    private float GetFatigueLevelInternal(string petId, string comboId)
    {
        if (!_petComboAdaptation.ContainsKey(petId))
            return 0f;

        return _petComboAdaptation[petId].TryGetValue(comboId, out float level) ? level : 0f;
    }

    /// <summary>
    /// REQ-179: Get fatigue stars as a display string (★☆☆ / ★★☆ / ★★★).
    /// </summary>
    public string GetFatigueStars(string petId = "")
    {
        float fatigue = GetFatigueLevel(petId);
        return FatigueToStars(fatigue);
    }

    /// <summary>
    /// Convert a fatigue value (0.0-1.0) to star string.
    /// 0-33%: ★☆☆ | 34-66%: ★★☆ | 67-100%: ★★★
    /// </summary>
    public string FatigueToStars(float fatigue)
    {
        if (fatigue < FatigueLevel1Threshold)
            return "★☆☆";
        if (fatigue < FatigueLevel2Threshold)
            return "★★☆";
        return "★★★";
    }

    /// <summary>
    /// REQ-120: Get the damage multiplier for a specific combo.
    /// Returns 1.0 at 0% fatigue, 0.5 (MinDamageMultiplier) at 100% fatigue.
    /// </summary>
    public float GetDamageMultiplier(string comboId, string petId = "")
    {
        float fatigue = GetFatigueLevel(petId);
        return 1.0f - (fatigue * (1.0f - MinDamageMultiplier));
    }

    /// <summary>
    /// Check if a combo is at high fatigue (★★☆ or ★★★).
    /// Used for UI warning highlighting.
    /// </summary>
    public bool IsHighFatigue(string petId = "")
    {
        return GetFatigueLevel(petId) >= FatigueLevel1Threshold;
    }

    private string GetActivePetId()
    {
        // Try to get from PetCombatCompanionSystem singleton
        try
        {
            // Godot 4 singleton access
            var petSystem = Engine.GetSingleton("PetCombatCompanionSystem");
            if (petSystem != null)
            {
                var getIdMethod = petSystem.GetType().GetMethod("GetActivePetId");
                if (getIdMethod != null)
                {
                    var result = getIdMethod.Invoke(petSystem, null);
                    if (result is string id && !string.IsNullOrEmpty(id))
                        return id;
                }
            }
        }
        catch
        {
            // Singleton not available, try direct type lookup
        }

        // Fallback: use PetCombatCompanionSystem.Instance directly if available
        try
        {
            var companionType = Type.GetType("PetCombatCompanionSystem, ClawRPG");
            if (companionType != null)
            {
                var instanceProp = companionType.GetProperty("Instance");
                if (instanceProp != null)
                {
                    var instance = instanceProp.GetValue(null) as Godot.Node;
                    if (instance != null)
                    {
                        var getIdMethod = instance.GetType().GetMethod("GetActivePetId");
                        if (getIdMethod != null)
                        {
                            var result = getIdMethod.Invoke(instance, null);
                            if (result is string id && !string.IsNullOrEmpty(id))
                                return id;
                        }
                    }
                }
            }
        }
        catch
        {
            // Fallback failed
        }

        return "";
    }

    // ── Persistence ──────────────────────────────────────────────────────

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();

        // Export history
        var historyData = new Dictionary<string, List<string>>();
        foreach (var kvp in _petComboHistory)
        {
            historyData[kvp.Key] = kvp.Value;
        }
        data["history"] = historyData;

        // Export adaptation levels
        var adaptationData = new Dictionary<string, Dictionary<string, float>>();
        foreach (var petKvp in _petComboAdaptation)
        {
            adaptationData[petKvp.Key] = petKvp.Value;
        }
        data["adaptation"] = adaptationData;

        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.TryGetValue("history", out var historyObj) && historyObj is Dictionary<string, List<string>> historyData)
        {
            _petComboHistory = historyData;
        }

        if (data.TryGetValue("adaptation", out var adaptObj) && adaptObj is Dictionary<string, Dictionary<string, float>> adaptationData)
        {
            _petComboAdaptation = adaptationData;
        }
    }
}
