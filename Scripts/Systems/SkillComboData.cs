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
    /// <summary>兼容旧 ComboData.ComboType 枚举，用于 UI 过滤</summary>
    public ComboData.ComboType OldComboType { get; set; }
    /// <summary>稀有度（来自旧 ComboData.Rarity）</summary>
    public ComboData.Rarity Rarity { get; set; }
    /// <summary>描述</summary>
    public string Description { get; set; }
    /// <summary>触发特效名称</summary>
    public string EffectName { get; set; }
    /// <summary>所需连击等级</summary>
    public int RequiredComboLevel { get; set; }
    /// <summary>冷却缩减（来自旧 ComboData.cooldownReduction）</summary>
    public float CooldownReduction { get; set; }
    /// <summary>连击点奖励</summary>
    public int ComboPointReward { get; set; }

    /// <summary>
    /// 转换为旧 ComboData（兼容 ComboUI）
    /// </summary>
    public ComboData ToComboData()
    {
        var cd = new ComboData
        {
            comboId = ComboId,
            comboName = Name,
            description = Description ?? "",
            damageMultiplier = Bonus?.DamageMultiplier ?? 1f,
            cooldownReduction = CooldownReduction,
            comboPointReward = ComboPointReward,
            effectName = EffectName ?? "",
            requiredComboLevel = RequiredComboLevel,
            comboType = OldComboType,
            comboRarity = Rarity
        };
        // 用 skillSequence 字段（Godot Export 字段）存储 SkillIds
        cd.skillSequence = new System.Collections.Generic.List<string>(SkillIds ?? new System.Collections.Generic.List<string>());
        return cd;
    }
}

public class ActiveCombo
{
    public string ComboId { get; set; }
    public float StartTime { get; set; }
    public List<string> TriggeredSkills { get; set; }
    public int CurrentStreak { get; set; }
    public bool IsComplete { get; set; }
    /// <summary>当前步骤（兼容旧 ComboProgress.currentStep）</summary>
    public int CurrentStep { get; set; }
    /// <summary>剩余时间（兼容旧 ComboProgress.timeRemaining）</summary>
    public float TimeRemaining { get; set; }
    /// <summary>是否激活（兼容旧 ComboProgress.isActive）</summary>
    public bool IsActive { get; set; }
    /// <summary>已执行次数（兼容旧 ComboProgress.timesExecuted）</summary>
    public int TimesExecuted { get; set; }
}

public class PlayerComboData
{
    public Dictionary<string, int> ComboUsageCount { get; set; }
    public Dictionary<string, int> ComboStreakBest { get; set; }
    public float TotalComboDamage { get; set; }
    public int TotalCombosTriggered { get; set; }
    public List<string> DiscoveredCombos { get; set; }
    /// <summary>连击点数（兼容旧 ComboSystem）</summary>
    public int ComboPoints { get; set; }
    /// <summary>连击等级（兼容旧 ComboSystem）</summary>
    public int ComboLevel { get; set; }
    /// <summary>当前进行的连击进度 comboId → (currentStep, timeRemaining)</summary>
    public Dictionary<string, (int step, float timeRemaining)> ActiveProgress { get; set; }

    public PlayerComboData()
    {
        ComboUsageCount = new Dictionary<string, int>();
        ComboStreakBest = new Dictionary<string, int>();
        DiscoveredCombos = new List<string>();
        ComboPoints = 0;
        ComboLevel = 1;
        ActiveProgress = new Dictionary<string, (int, float)>();
    }
}
