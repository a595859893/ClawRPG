using Godot;
using System;
using System.Collections.Generic;

public class DailyLoginBonusDatabase : BaseSystem
{
    // 每日奖励配置（7天循环）
    public List<Dictionary<string, object>> DailyRewards { get; private set; } = new List<Dictionary<string, object>>
    {
        // Day 1
        new Dictionary<string, object>
        {
            { "day", 1 },
            { "gold", 100 },
            { "exp", 50 },
            { "diamonds", 0 },
            { "items", new List<string>() },
            { "multiplier", 1.0f }
        },
        // Day 2
        new Dictionary<string, object>
        {
            { "day", 2 },
            { "gold", 150 },
            { "exp", 75 },
            { "diamonds", 0 },
            { "items", new List<string>() },
            { "multiplier", 1.0f }
        },
        // Day 3
        new Dictionary<string, object>
        {
            { "day", 3 },
            { "gold", 200 },
            { "exp", 100 },
            { "diamonds", 5 },
            { "items", new List<string>() },
            { "multiplier", 1.0f }
        },
        // Day 4
        new Dictionary<string, object>
        {
            { "day", 4 },
            { "gold", 250 },
            { "exp", 125 },
            { "diamonds", 0 },
            { "items", new List<string>() },
            { "multiplier", 1.0f }
        },
        // Day 5
        new Dictionary<string, object>
        {
            { "day", 5 },
            { "gold", 300 },
            { "exp", 150 },
            { "diamonds", 10 },
            { "items", new List<string>() },
            { "multiplier", 1.0f }
        },
        // Day 6
        new Dictionary<string, object>
        {
            { "day", 6 },
            { "gold", 400 },
            { "exp", 200 },
            { "diamonds", 0 },
            { "items", new List<string>() },
            { "multiplier", 1.0f }
        },
        // Day 7 (Big reward)
        new Dictionary<string, object>
        {
            { "day", 7 },
            { "gold", 1000 },
            { "exp", 500 },
            { "diamonds", 50 },
            { "items", new List<string>() { "LegendaryChest" } },
            { "multiplier", 2.0f }
        }
    };
    
    // 连续登录加成
    public Dictionary<int, float> StreakMultipliers { get; private set; } = new Dictionary<int, float>
    {
        { 7, 1.0f },    // 1 week: 1.0x
        { 14, 1.25f },  // 2 weeks: 1.25x
        { 21, 1.5f },   // 3 weeks: 1.5x
        { 30, 2.0f },   // 1 month: 2.0x
        { 60, 2.5f },   // 2 months: 2.5x
        { 90, 3.0f },   // 3 months: 3.0f
        { 180, 4.0f },  // 6 months: 4.0x
        { 365, 5.0f }   // 1 year: 5.0x
    };
    
    // 月度大奖（每30天）
    public Dictionary<string, object> MonthlyBonus { get; private set; } = new Dictionary<string, object>
    {
        { "gold", 5000 },
        { "exp", 2500 },
        { "diamonds", 200 },
        { "items", new List<string>() { "LegendaryWeaponRandom", "EpicAccessoryRandom" } }
    };
    
    // 获取当前天的奖励配置
    public Dictionary<string, object> GetDailyReward(int day)
    {
        int index = (day - 1) % 7;
        return DailyRewards[index];
    }
    
    // 获取连续登录加成
    public float GetStreakMultiplier(int streak)
    {
        float multiplier = 1.0f;
        foreach (var kvp in StreakMultipliers)
        {
            if (streak >= kvp.Key)
            {
                multiplier = kvp.Value;
            }
        }
        return multiplier;
    }
    
    // 检查是否是月度大奖日
    public bool IsMonthlyBonusDay(int cumulativeDays)
    {
        return cumulativeDays > 0 && cumulativeDays % 30 == 0;
    }
}
