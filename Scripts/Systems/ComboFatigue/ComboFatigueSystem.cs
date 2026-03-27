using Godot;
using System;

/// <summary>
/// Combo Fatigue System.
/// Tracks player combo usage patterns and applies adaptation penalties
/// when the same combo is used repeatedly.
///
/// Integration points:
/// - SkillModules.ApplyDamage: apply GetFatigueMultiplier() to damage
/// - SkillComboSystem: call RecordComboUsage() after each combo execution
/// </summary>
public class ComboFatigueSystem : BaseSystem
{
    private static ComboFatigueSystem _instance;
    public static ComboFatigueSystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new ComboFatigueSystem();
            }
            return _instance;
        }
    }
    
    private ComboFatigueData _data;
    
    // Signal emitted when a combo's adaptation level changes
    [Signal]
    public delegate void AdaptationChanged(string comboId, float adaptationLevel, float damageMultiplier);
    
    // Signal emitted when a new combo is used (for UI feedback)
    [Signal]
    public delegate void ComboSwitched(string newComboId, string previousComboId);
    
    public override void _Ready()
    {
        _data = GetNode<ComboFatigueData>("/root/ComboFatigueData");
        if (_data == null)
        {
            GD.PrintErr("ComboFatigueData not found!");
        }
    }
    
    /// <summary>
    /// Record that the player used a combo skill.
    /// Call this after a combo skill successfully deals damage / is executed.
    /// </summary>
    public void RecordComboUsage(string comboId)
    {
        if (_data == null || string.IsNullOrEmpty(comboId)) return;
        
        string previousCombo = _data.GetLastComboId();
        float oldAdaptation = _data.GetAdaptationLevel(comboId);
        
        _data.RecordCombo(comboId);
        
        float newAdaptation = _data.GetAdaptationLevel(comboId);
        float newMultiplier = _data.GetDamageMultiplier(comboId);
        
        // Emit signals for UI updates
        if (oldAdaptation != newAdaptation)
        {
            EmitSignal(nameof(AdaptationChanged), comboId, newAdaptation, newMultiplier);
        }
        
        if (!string.IsNullOrEmpty(previousCombo) && previousCombo != comboId)
        {
            EmitSignal(nameof(ComboSwitched), comboId, previousCombo);
        }
    }
    
    /// <summary>
    /// Get the damage multiplier for a given combo ID.
    /// Returns 1.0 if combo is not tracked (no penalty).
    /// </summary>
    public float GetDamageMultiplier(string comboId)
    {
        if (_data == null) return 1.0f;
        return _data.GetDamageMultiplier(comboId);
    }
    
    /// <summary>
    /// Get the adaptation level (0.0–1.0) for a combo.
    /// </summary>
    public float GetAdaptationLevel(string comboId)
    {
        if (_data == null) return 0f;
        return _data.GetAdaptationLevel(comboId);
    }
    
    /// <summary>
    /// Get the fatigue severity string for UI display.
    /// </summary>
    public string GetFatigueStatus(string comboId)
    {
        float adaptation = GetAdaptationLevel(comboId);
        if (adaptation <= 0f) return "Fresh";
        if (adaptation < 0.2f) return "Slightly Familiar";
        if (adaptation < 0.35f) return "Adapted";
        if (adaptation < 0.45f) return "Highly Adapted";
        return "Fully Adapted";
    }
    
    /// <summary>
    /// Get a summary of current fatigue state for UI.
    /// </summary>
    public (string comboId, float adaptation, float multiplier, string status) GetFatigueInfo(string comboId)
    {
        return (comboId, GetAdaptationLevel(comboId), GetDamageMultiplier(comboId), GetFatigueStatus(comboId));
    }
    
    /// <summary>
    /// Clear all fatigue data (e.g., on new game).
    /// </summary>
    public void Reset()
    {
        if (_data == null) return;
        _data.ComboHistory.Clear();
        _data.ComboAdaptation.Clear();
    }
}
