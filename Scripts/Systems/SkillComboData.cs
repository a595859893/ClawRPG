using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 技能连击类型枚举。
/// </summary>
public enum ComboType
{
    Sequential,    // Skills used in order
    Simultaneous,  // Multiple skills at once
    ChainReaction  // One skill triggers another
}

/// <summary>
/// 技能连击触发条件枚举。
/// </summary>
public enum ComboTrigger
{
    TimeWindow,    // Within time window
    SameElement,   // Same element skills
    DifferentType, // Different skill types
    Any            // Any combination
}

public class ComboBonus
{
    public string BonusId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public float DamageMultiplier { get; set; }
    public float CooldownReduction { get; set; }
    public float Duration { get; set; }
    public int RequiredComboCount { get; set; }
}

public class SkillCombo
{
    public string ComboId { get; set; }
    public string Name { get; set; }
    public ComboType Type { get; set; }
    public ComboTrigger Trigger { get; set; }
    public List<string> SkillIds { get; set; }
    public float TimeWindow { get; set; }
    public ComboBonus Bonus { get; set; }
}

public class ActiveCombo
{
    public string ComboId { get; set; }
    public float StartTime { get; set; }
    public List<string> TriggeredSkills { get; set; }
    public int CurrentStreak { get; set; }
    public bool IsComplete { get; set; }
}

public class PlayerComboData
{
    public Dictionary<string, int> ComboUsageCount { get; set; }
    public Dictionary<string, int> ComboStreakBest { get; set; }
    public float TotalComboDamage { get; set; }
    public int TotalCombosTriggered { get; set; }
    public List<string> DiscoveredCombos { get; set; }
    
    public PlayerComboData()
    {
        ComboUsageCount = new Dictionary<string, int>();
        ComboStreakBest = new Dictionary<string, int>();
        DiscoveredCombos = new List<string>();
    }
}
