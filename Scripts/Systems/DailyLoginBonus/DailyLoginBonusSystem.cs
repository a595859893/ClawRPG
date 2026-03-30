using Godot;
using System;
using System.Collections.Generic;

public class DailyLoginBonusSystem : BaseSystem
{
    private DailyLoginBonusData _data;
    private DailyLoginBonusDatabase _database;
    
    // 事件信号
    
    public override void _Ready()
    {
        _database = new DailyLoginBonusDatabase();
        LoadData();
    }
    
    // 加载数据
    private void LoadData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem != null)
        {
            var gameData = saveSystem.LoadGame();
            if (gameData.Contains("dailyLoginBonus"))
            {
                var bonusData = (Godot.Dictionary)gameData["dailyLoginBonus"];
                _data = new DailyLoginBonusData();
                
                if (bonusData.Contains("currentStreak"))
                    _data.CurrentStreak = Convert.ToInt32(bonusData["currentStreak"]);
                if (bonusData.Contains("bestStreak"))
                    _data.BestStreak = Convert.ToInt32(bonusData["bestStreak"]);
                if (bonusData.Contains("totalLogins"))
                    _data.TotalLogins = Convert.ToInt32(bonusData["totalLogins"]);
                if (bonusData.Contains("totalDaysClaimed"))
                    _data.TotalDaysClaimed = Convert.ToInt32(bonusData["totalDaysClaimed"]);
                if (bonusData.Contains("cumulativeLoginDays"))
                    _data.CumulativeLoginDays = Convert.ToInt32(bonusData["cumulativeLoginDays"]);
                if (bonusData.Contains("totalGoldReceived"))
                    _data.TotalGoldReceived = Convert.ToInt32(bonusData["totalGoldReceived"]);
                if (bonusData.Contains("totalExpReceived"))
                    _data.TotalExpReceived = Convert.ToInt32(bonusData["totalExpReceived"]);
                if (bonusData.Contains("totalDiamondsReceived"))
                    _data.TotalDiamondsReceived = Convert.ToInt32(bonusData["totalDiamondsReceived"]);
                if (bonusData.Contains("claimedBonusCount"))
                    _data.ClaimedBonusCount = Convert.ToInt32(bonusData["claimedBonusCount"]);
                
                // 解析最后登录日期
                if (bonusData.Contains("lastLoginDate"))
                {
                    var lastLogin = (Godot.Dictionary)bonusData["lastLoginDate"];
                    _data.LastLoginDate = new Dictionary<string, object>
                    {
                        { "year", lastLogin.Contains("year") ? Convert.ToInt32(lastLogin["year"]) : 0 },
                        { "month", lastLogin.Contains("month") ? Convert.ToInt32(lastLogin["month"]) : 0 },
                        { "day", lastLogin.Contains("day") ? Convert.ToInt32(lastLogin["day"]) : 0 }
                    };
                }
                
                // 解析本月登录天数
                if (bonusData.Contains("monthlyLoginDays"))
                {
                    var monthlyDays = (Godot.Array)bonusData["monthlyLoginDays"];
                    _data.MonthlyLoginDays = new List<int>();
                    foreach (var d in monthlyDays)
                    {
                        _data.MonthlyLoginDays.Add(Convert.ToInt32(d));
                    }
                }
            }
            else
            {
                _data = new DailyLoginBonusData();
            }
        }
        else
        {
            _data = new DailyLoginBonusData();
        }
    }
    
    // 保存数据
    public void SaveData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem != null)
        {
            var gameData = saveSystem.LoadGame();
            
            var bonusData = new Godot.Dictionary
            {
                { "currentStreak", _data.CurrentStreak },
                { "bestStreak", _data.BestStreak },
                { "totalLogins", _data.TotalLogins },
                { "totalDaysClaimed", _data.TotalDaysClaimed },
                { "cumulativeLoginDays", _data.CumulativeLoginDays },
                { "totalGoldReceived", _data.TotalGoldReceived },
                { "totalExpReceived", _data.TotalExpReceived },
                { "totalDiamondsReceived", _data.TotalDiamondsReceived },
                { "claimedBonusCount", _data.ClaimedBonusCount },
                { "lastLoginDate", new Godot.Dictionary
                    {
                        { "year", _data.LastLoginDate["year"] },
                        { "month", _data.LastLoginDate["month"] },
                        { "day", _data.LastLoginDate["day"] }
                    }
                }
            };
            
            // 转换本月登录天数
            var monthlyDaysArray = new Godot.Array();
            foreach (var day in _data.MonthlyLoginDays)
            {
                monthlyDaysArray.Add(day);
            }
            bonusData["monthlyLoginDays"] = monthlyDaysArray;
            
            gameData["dailyLoginBonus"] = bonusData;
            saveSystem.SaveGame(gameData);
        }
    }
    
    // 检查今日是否已领取
    public bool IsTodayClaimed()
    {
        var now = DateTime.Now;
        
        // 检查本月登录天数
        if (_data.MonthlyLoginDays.Contains(now.Day))
        {
            return true;
        }
        
        return false;
    }
    
    // 检查是否可以领取今日奖励
    public bool CanClaimToday()
    {
        return !IsTodayClaimed();
    }
    
    // 领取今日奖励
    public Dictionary<string, object> ClaimDailyBonus()
    {
        var now = DateTime.Now;
        
        // 如果今日已领取，返回空
        if (IsTodayClaimed())
        {
            return new Dictionary<string, object>();
        }
        
        // 计算连续登录
        int lastYear = Convert.ToInt32(_data.LastLoginDate["year"]);
        int lastMonth = Convert.ToInt32(_data.LastLoginDate["month"]);
        int lastDay = Convert.ToInt32(_data.LastLoginDate["day"]);
        
        var lastLoginDate = new DateTime(lastYear, lastMonth, lastDay);
        var today = new DateTime(now.Year, now.Month, now.Day);
        var daysDiff = (today - lastLoginDate).Days;
        
        // 更新连续登录
        if (daysDiff == 1)
        {
            // 连续登录
            _data.CurrentStreak++;
        }
        else if (daysDiff > 1)
        {
            // 中断连续登录
            _data.CurrentStreak = 1;
        }
        else if (daysDiff == 0)
        {
            // 同一天，不改变连续数
        }
        else
        {
            // 首次登录或更长时间
            _data.CurrentStreak = 1;
        }
        
        // 更新最佳连续
        if (_data.CurrentStreak > _data.BestStreak)
        {
            _data.BestStreak = _data.CurrentStreak;
            EmitSignal(nameof(StreakUpdated), _data.CurrentStreak, _data.BestStreak);
        }
        
        // 更新统计
        _data.TotalLogins++;
        _data.TotalDaysClaimed++;
        _data.CumulativeLoginDays++;
        
        // 记录本月登录
        if (!_data.MonthlyLoginDays.Contains(now.Day))
        {
            _data.MonthlyLoginDays.Add(now.Day);
        }
        
        // 检查是否需要重置本月登录（新月）
        if (lastMonth != now.Month || lastYear != now.Year)
        {
            _data.MonthlyLoginDays.Clear();
            _data.MonthlyLoginDays.Add(now.Day);
        }
        
        // 计算奖励
        int dayInCycle = ((_data.TotalDaysClaimed - 1) % 7) + 1;
        var rewardConfig = _database.GetDailyReward(dayInCycle);
        
        float streakMultiplier = _database.GetStreakMultiplier(_data.CurrentStreak);
        float baseMultiplier = Convert.ToSingle(rewardConfig["multiplier"]);
        float totalMultiplier = baseMultiplier * streakMultiplier;
        
        int gold = (int)(Convert.ToInt32(rewardConfig["gold"]) * totalMultiplier);
        int exp = (int)(Convert.ToInt32(rewardConfig["exp"]) * totalMultiplier);
        int diamonds = (int)(Convert.ToInt32(rewardConfig["diamonds"]) * totalMultiplier);
        
        // 累计奖励统计
        _data.TotalGoldReceived += gold;
        _data.TotalExpReceived += exp;
        _data.TotalDiamondsReceived += diamonds;
        _data.ClaimedBonusCount++;
        
        // 记录最后登录日期
        _data.LastLoginDate = new Dictionary<string, object>
        {
            { "year", now.Year },
            { "month", now.Month },
            { "day", now.Day }
        };
        
        // 检查月度大奖
        bool monthlyBonus = false;
        if (_database.IsMonthlyBonusDay(_data.CumulativeLoginDays))
        {
            var monthlyConfig = _database.MonthlyBonus;
            gold += Convert.ToInt32(monthlyConfig["gold"]);
            exp += Convert.ToInt32(monthlyConfig["exp"]);
            diamonds += Convert.ToInt32(monthlyConfig["diamonds"]);
            monthlyBonus = true;
            
            _data.TotalGoldReceived += Convert.ToInt32(monthlyConfig["gold"]);
            _data.TotalExpReceived += Convert.ToInt32(monthlyConfig["exp"]);
            _data.TotalDiamondsReceived += Convert.ToInt32(monthlyConfig["diamonds"]);
            
            EmitSignal(nameof(MonthlyBonusClaimed), _data.CumulativeLoginDays);
        }
        
        // 发送奖励
        var playerStats = GetNode("/root/PlayerStats");
        if (playerStats != null)
        {
            playerStats.Call("AddGold", gold);
            playerStats.Call("AddExperience", exp);
            // diamonds 需要通过实际系统添加
        }
        
        // 记录历史
        var historyEntry = new Dictionary<string, object>
        {
            { "date", now.ToString("yyyy-MM-dd") },
            { "day", dayInCycle },
            { "streak", _data.CurrentStreak },
            { "gold", gold },
            { "exp", exp },
            { "diamonds", diamonds },
            { "monthlyBonus", monthlyBonus }
        };
        
        _data.LoginHistory.Insert(0, historyEntry);
        if (_data.LoginHistory.Count > 30)
        {
            _data.LoginHistory.RemoveAt(_data.LoginHistory.Count - 1);
        }
        
        // 保存数据
        SaveData();
        
        // 发送信号
        EmitSignal(nameof(BonusClaimed), dayInCycle, gold, exp, diamonds);
        
        // 返回奖励结果
        return new Dictionary<string, object>
        {
            { "day", dayInCycle },
            { "streak", _data.CurrentStreak },
            { "gold", gold },
            { "exp", exp },
            { "diamonds", diamonds },
            { "multiplier", totalMultiplier },
            { "monthlyBonus", monthlyBonus }
        };
    }
    
    // 获取当前登录信息
    public Dictionary<string, object> GetLoginInfo()
    {
        var now = DateTime.Now;
        int dayInCycle = ((_data.TotalDaysClaimed) % 7) + 1;
        
        return new Dictionary<string, object>
        {
            { "canClaim", CanClaimToday() },
            { "currentStreak", _data.CurrentStreak },
            { "bestStreak", _data.BestStreak },
            { "totalLogins", _data.TotalLogins },
            { "totalDaysClaimed", _data.TotalDaysClaimed },
            { "cumulativeLoginDays", _data.CumulativeLoginDays },
            { "dayInCycle", dayInCycle },
            { "isTodayClaimed", IsTodayClaimed() },
            { "today", $"{now.Year}-{now.Month:D2}-{now.Day:D2}" }
        };
    }
    
    // 获取统计数据
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            { "totalGoldReceived", _data.TotalGoldReceived },
            { "totalExpReceived", _data.TotalExpReceived },
            { "totalDiamondsReceived", _data.TotalDiamondsReceived },
            { "claimedBonusCount", _data.ClaimedBonusCount },
            { "currentStreak", _data.CurrentStreak },
            { "bestStreak", _data.BestStreak },
            { "totalLogins", _data.TotalLogins },
            { "cumulativeLoginDays", _data.CumulativeLoginDays }
        };
    }
    
    // 获取本周奖励预览
    public List<Dictionary<string, object>> GetWeeklyPreview()
    {
        var preview = new List<Dictionary<string, object>>();
        
        for (int day = 1; day <= 7; day++)
        {
            var reward = _database.GetDailyReward(day);
            bool claimed = _data.MonthlyLoginDays.Contains(day);
            
            preview.Add(new Dictionary<string, object>
            {
                { "day", day },
                { "gold", reward["gold"] },
                { "exp", reward["exp"] },
                { "diamonds", reward["diamonds"] },
                { "multiplier", reward["multiplier"] },
                { "claimed", claimed }
            });
        }
        
        return preview;
    }
    
    // 获取历史记录
    public List<Dictionary<string, object>> GetHistory()
    {
        return _data.LoginHistory;
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 登录数据
        data["current_streak"] = _data.CurrentStreak;
        data["best_streak"] = _data.BestStreak;
        data["total_logins"] = _data.TotalLogins;
        data["total_days_claimed"] = _data.TotalDaysClaimed;
        data["cumulative_login_days"] = _data.CumulativeLoginDays;
        data["total_gold_received"] = _data.TotalGoldReceived;
        data["total_exp_received"] = _data.TotalExpReceived;
        data["total_diamonds_received"] = _data.TotalDiamondsReceived;
        data["claimed_bonus_count"] = _data.ClaimedBonusCount;
        
        // 最后登录日期
        data["last_login_year"] = (int)_data.LastLoginDate.Get("year", 0);
        data["last_login_month"] = (int)_data.LastLoginDate.Get("month", 0);
        data["last_login_day"] = (int)_data.LastLoginDate.Get("day", 0);
        
        // 本月登录天数
        var monthlyDaysArray = new Array();
        foreach (var day in _data.MonthlyLoginDays)
        {
            monthlyDaysArray.Add(day);
        }
        data["monthly_login_days"] = monthlyDaysArray;
        
        return data;
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("current_streak")) _data.CurrentStreak = (int)data["current_streak"];
        if (data.Contains("best_streak")) _data.BestStreak = (int)data["best_streak"];
        if (data.Contains("total_logins")) _data.TotalLogins = (int)data["total_logins"];
        if (data.Contains("total_days_claimed")) _data.TotalDaysClaimed = (int)data["total_days_claimed"];
        if (data.Contains("cumulative_login_days")) _data.CumulativeLoginDays = (int)data["cumulative_login_days"];
        if (data.Contains("total_gold_received")) _data.TotalGoldReceived = (int)data["total_gold_received"];
        if (data.Contains("total_exp_received")) _data.TotalExpReceived = (int)data["total_exp_received"];
        if (data.Contains("total_diamonds_received")) _data.TotalDiamondsReceived = (int)data["total_diamonds_received"];
        if (data.Contains("claimed_bonus_count")) _data.ClaimedBonusCount = (int)data["claimed_bonus_count"];
        
        // 最后登录日期
        _data.LastLoginDate["year"] = data.Contains("last_login_year") ? (int)data["last_login_year"] : 0;
        _data.LastLoginDate["month"] = data.Contains("last_login_month") ? (int)data["last_login_month"] : 0;
        _data.LastLoginDate["day"] = data.Contains("last_login_day") ? (int)data["last_login_day"] : 0;
        
        // 本月登录天数
        _data.MonthlyLoginDays.Clear();
        if (data.Contains("monthly_login_days"))
        {
            var monthlyDaysArray = (Array)data["monthly_login_days"];
            foreach (var day in monthlyDaysArray)
            {
                _data.MonthlyLoginDays.Add((int)day);
            }
        }
    }
}
