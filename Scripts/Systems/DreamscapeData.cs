using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 梦境数据 - 存储梦境类型和配置
/// </summary>
public enum DreamscapeType
{
    Nightmare,   // 噩梦 - 高难度敌人
    Ethereal,    // 以太 - 漂浮战斗
    Void,        // 虚空 - 无尽虚空
    Temporal,    // 时间 - 时间减缓
    Lucid        // 清醒 - 自由建造
}

public enum DreamscapeState
{
    Locked,      // 锁定
    Available,   // 可用
    InProgress,  // 进行中
    Completed,   // 完成
    Mastered     // 精通
}

public enum DreamscapeRule
{
    None,                // 无特殊规则
    FloatGravity,        // 漂浮重力
    TimeSlowdown,       // 时间减缓
    NoCooldown,         // 无冷却
    DoubleDamage,        // 双倍伤害
    InfiniteMana,        // 无限法力
    OneHitKill,         // 一击必杀
    RandomElements,      // 随机元素
    GravityReversal,    // 重力反转
    NoDeathPenalty      // 无死亡惩罚
}

public class DreamscapeLayer
{
    public int LayerNumber { get; set; }
    public string EnemyType { get; set; }
    public int EnemyCount { get; set; }
    public DreamscapeRule SpecialRule { get; set; }
    public int TimeLimit { get; set; }  // 秒
    public int BaseScore { get; set; }
    public int BaseGold { get; set; }
    public int BaseExperience { get; set; }
    public bool IsBossLayer { get; set; }
    public string BossType { get; set; }
}

public class DreamscapeEntry
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public DreamscapeType Type { get; set; }
    public DreamscapeState State { get; set; }
    public int TotalLayers { get; set; }
    public int RequiredPlayerLevel { get; set; }
    public int EntryCost { get; set; }
    public DreamscapeRule DefaultRule { get; set; }
    public float EnemyMultiplier { get; set; }
    public float ScoreMultiplier { get; set; }
    public float DropMultiplier { get; set; }
}

public class DreamscapeProgress
{
    public string DreamscapeId { get; set; }
    public int CurrentLayer { get; set; }
    public int HighestLayer { get; set; }
    public int TotalScore { get; set; }
    public int BestScore { get; set; }
    public int CompletionCount { get; set; }
    public int MasteryCount { get; set; }
    public float BestTime { get; set; }
    public DateTime LastEntered { get; set; }
    public bool IsInDreamscape { get; set; }
    public int CurrentLayerScore { get; set; }
    public int CurrentLayerTime { get; set; }
}

public class DreamscapeReward
{
    public int Gold { get; set; }
    public int Experience { get; set; }
    public List<string> Items { get; set; }
    public float DropRateBonus { get; set; }
    public int BonusScore { get; set; }
}

public class PlayerDreamscapeData
{
    public Dictionary<string, DreamscapeProgress> Progress { get; set; }
    public Dictionary<string, bool> UnlockedDreamscapes { get; set; }
    public int TotalScore { get; set; }
    public int DreamscapesCompleted { get; set; }
    public int DreamscapesMastered { get; set; }
    public int TotalLayersCleared { get; set; }
    
    public PlayerDreamscapeData()
    {
        Progress = new Dictionary<string, DreamscapeProgress>();
        UnlockedDreamscapes = new Dictionary<string, bool>();
    }
}
