using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

// ========== JSON 配置数据结构 ==========

/// <summary>
/// 连击配置条目 - 从 JSON 文件反序列化
/// </summary>
public class ComboConfigEntry
{
    [JsonPropertyName("comboId")] public string ComboId { get; set; }
    [JsonPropertyName("comboName")] public string ComboName { get; set; }
    [JsonPropertyName("description")] public string Description { get; set; }
    [JsonPropertyName("skillSequence")] public List<string> SkillSequence { get; set; }
    [JsonPropertyName("damageMultiplier")] public float DamageMultiplier { get; set; }
    [JsonPropertyName("cooldownReduction")] public float CooldownReduction { get; set; }
    [JsonPropertyName("comboPointReward")] public int ComboPointReward { get; set; }
    [JsonPropertyName("effectName")] public string EffectName { get; set; }
    [JsonPropertyName("requiredComboLevel")] public int RequiredComboLevel { get; set; }
    [JsonPropertyName("comboType")] public string ComboType { get; set; }
    [JsonPropertyName("comboRarity")] public string ComboRarity { get; set; }
}

/// <summary>
/// 连击配置文件格式
/// </summary>
public class ComboConfigFile
{
    [JsonPropertyName("version")] public string Version { get; set; }
    [JsonPropertyName("combos")] public List<ComboConfigEntry> Combos { get; set; }
}

// ========== 数据资源 ==========

/// <summary>
/// 连击数据资源 - 定义一个连击的完整配置
/// </summary>
public class ComboData : Resource
{
    /// <summary>
    /// 连击ID
    /// </summary>
    [Export] public string comboId;
    /// <summary>
    /// 连击名称
    /// </summary>
    [Export] public string comboName;
    /// <summary>
    /// 连击描述
    /// </summary>
    [Export] public string description;
    /// <summary>
    /// 技能序列 - 按顺序需要使用的技能ID列表
    /// </summary>
    [Export] public List<string> skillSequence = new List<string>();
    /// <summary>
    /// 伤害倍率
    /// </summary>
    [Export] public float damageMultiplier = 1.5f;
    /// <summary>
    /// 冷却缩减百分比
    /// </summary>
    [Export] public float cooldownReduction = 0.2f;
    /// <summary>
    /// 连击点奖励
    /// </summary>
    [Export] public int comboPointReward = 10;
    /// <summary>
    /// 特效名称
    /// </summary>
    [Export] public string effectName;
    /// <summary>
    /// 需要的连击等级
    /// </summary>
    [Export] public int requiredComboLevel = 1;
    /// <summary>
    /// 连击类型
    /// </summary>
    [Export] public ComboType comboType;
    /// <summary>
    /// 稀有度
    /// </summary>
    [Export] public Rarity comboRarity;

    /// <summary>
    /// 连击类型枚举
    /// </summary>
    public enum ComboType
    {
        Offensive,
        Defensive,
        Support,
        Utility,
        Special
    }

    /// <summary>
    /// 稀有度枚举
    /// </summary>
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
}

/// <summary>
/// 连击进度 - 追踪玩家当前连击的执行状态
/// </summary>
public class ComboProgress
{
    /// <summary>
    /// 连击ID
    /// </summary>
    public string comboId;
    /// <summary>
    /// 当前完成的步骤数
    /// </summary>
    public int currentStep = 0;
    /// <summary>
    /// 剩余时间（秒）
    /// </summary>
    public float timeRemaining = 0f;
    /// <summary>
    /// 连击是否激活
    /// </summary>
    public bool isActive = false;
    /// <summary>
    /// 已执行次数
    /// </summary>
    public int timesExecuted = 0;
}
