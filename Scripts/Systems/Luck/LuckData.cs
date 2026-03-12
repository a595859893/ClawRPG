using Godot;
using System;
using System.Collections.Generic;

public class LuckData
{
    // 基础幸运值
    public int BaseLuck { get; set; } = 50;
    
    // 临时增益/减益
    public List<LuckModifier> ActiveModifiers { get; set; } = new List<LuckModifier>();
    
    // 幸运历史记录
    public List<LuckEvent> History { get; set; } = new List<LuckEvent>();
    
    // 统计
    public int TotalLuckyRolls { get; set; } = 0;
    public int CriticalLuckRolls { get; set; } = 0;
    public int FailedLuckRolls { get; set; } = 0;
    public int TotalLuckBonus { get; set; } = 0;
}

public class LuckModifier
{
    public string Name { get; set; } = "";
    public int Value { get; set; } = 0;
    public string Source { get; set; } = "";  // "item", "buff", "curse", "zone"
    public int Duration { get; set; } = 0;    // 剩余持续时间(秒)，0表示永久
    public DateTime AppliedAt { get; set; } = DateTime.Now;
}

public class LuckEvent
{
    public string Type { get; set; } = "";  // "roll", "bonus", "critical", "fail"
    public int Value { get; set; } = 0;
    public int Result { get; set; } = 0;
    public string Source { get; set; } = "";
    public DateTime Timestamp { get; set; } = DateTime.Now;
}

// 幸运结果枚举
public enum LuckResult
{
    CriticalFailure,  // 大失败
    Failure,          // 失败
    LowSuccess,       // 小成功
    Success,          // 成功
    HighSuccess,      // 大成功
    CriticalSuccess   // 暴击大成功
}
