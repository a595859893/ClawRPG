using Godot;
using System;
using System.Collections.Generic;

public class DailyLoginBonusData : Resource
{
    // 登录记录
    public Dictionary<string, object> LastLoginDate { get; set; } = new Dictionary<string, object>
    {
        { "year", 0 },
        { "month", 0 },
        { "day", 0 }
    };
    
    // 连续登录数据
    public int CurrentStreak { get; set; } = 0;
    public int BestStreak { get; set; } = 0;
    public int TotalLogins { get; set; } = 0;
    public int TotalDaysClaimed { get; set; } = 0;
    
    // 本月登录记录
    public List<int> MonthlyLoginDays { get; set; } = new List<int>();
    
    // 累计登录天数（用于月度大奖）
    public int CumulativeLoginDays { get; set; } = 0;
    
    // 统计数据
    public int TotalGoldReceived { get; set; } = 0;
    public int TotalExpReceived { get; set; } = 0;
    public int TotalDiamondsReceived { get; set; } = 0;
    public int ClaimedBonusCount { get; set; } = 0;
    
    // 历史记录
    public List<Dictionary<string, object>> LoginHistory { get; set; } = new List<Dictionary<string, object>>();
}
