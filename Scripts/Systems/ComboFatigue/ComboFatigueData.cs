using Godot;
using System;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;

/// <summary>
/// Combo Fatigue persistent data.
/// Tracks recent combo usage and per-combo adaptation levels.
/// </summary>
public partial class ComboFatigueData : BaseSystem
{
    // Recent combo history (combo skill IDs)
    public List<string> ComboHistory { get; set; } = new List<string>();
    
    // adaptation_level per combo_id (0.0 = fresh, 1.0 = fully adapted, max 50% damage penalty)
    public Dictionary<string, float> ComboAdaptation { get; set; } = new Dictionary<string, float>();
    
    // Config: memory window size
    public int MemoryWindow { get; set; } = 10;
    
    // Config: adaptation increase per repeat use
    public float AdaptationIncrement { get; set; } = 0.15f;
    
    // Config: max adaptation level
    public float MaxAdaptation { get; set; } = 0.5f;
    
    // Config: decay factor for other combos when switching
    public float DecayFactor { get; set; } = 0.9f;
    
    public override void _Ready()
    {
        SaveSystem.Instance.RegisterSaveData(this);
    }
    
    /// <summary>
    /// Record that a combo was used. Updates adaptation and decays others.
    /// </summary>
    public void RecordCombo(string comboId)
    {
        if (string.IsNullOrEmpty(comboId)) return;
        
        // Add to history
        ComboHistory.Add(comboId);
        if (ComboHistory.Count > MemoryWindow)
        {
            ComboHistory.RemoveAt(0);
        }
        
        // Increase adaptation for this combo
        float currentAdaptation = ComboAdaptation.GetValueOrDefault(comboId, 0f);
        float newAdaptation = Mathf.Min(MaxAdaptation, currentAdaptation + AdaptationIncrement);
        ComboAdaptation[comboId] = newAdaptation;
        
        // Decay other combos
        foreach (var kvp in ComboAdaptation)
        {
            if (kvp.Key != comboId)
            {
                ComboAdaptation[kvp.Key] = kvp.Value * DecayFactor;
            }
        }
    }
    
    /// <summary>
    /// Get damage multiplier for a combo (1.0 = full damage, 0.5 = 50% damage at max adaptation).
    /// </summary>
    public float GetDamageMultiplier(string comboId)
    {
        float adaptation = ComboAdaptation.GetValueOrDefault(comboId, 0f);
        return 1.0f - adaptation;
    }
    
    /// <summary>
    /// Get adaptation level for a combo (0.0–1.0).
    /// </summary>
    public float GetAdaptationLevel(string comboId)
    {
        return ComboAdaptation.GetValueOrDefault(comboId, 0f);
    }
    
    /// <summary>
    /// Get the most recently used combo ID, or empty string.
    /// </summary>
    public string GetLastComboId()
    {
        return ComboHistory.Count > 0 ? ComboHistory[ComboHistory.Count - 1] : "";
    }
    
    /// <summary>
    /// Export save data.
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        data["comboHistory"] = new Godot.Collections.Array(ComboHistory);
        
        var adaptationList = new Godot.Collections.Array();
        foreach (var kvp in ComboAdaptation)
        {
            adaptationList.Add(new Godot.Collections.Dictionary { ["id"] = kvp.Key, ["level"] = kvp.Value });
        }
        data["comboAdaptation"] = adaptationList;
        
        return data;
    }
    
    /// <summary>
    /// Import save data.
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        ComboHistory.Clear();
        if (data.ContainsKey("comboHistory"))
        {
            foreach (string id in (Godot.Collections.Array)data["comboHistory"])
            {
                ComboHistory.Add(id);
            }
        }
        
        ComboAdaptation.Clear();
        if (data.ContainsKey("comboAdaptation"))
        {
            foreach (Dictionary entry in (Godot.Collections.Array)data["comboAdaptation"])
            {
                string id = (string)entry["id"];
                float level = (float)entry["level"];
                ComboAdaptation[id] = level;
            }
        }
    }
}
