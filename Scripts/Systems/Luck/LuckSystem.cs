using Godot;
using System;
using System.Collections.Generic;

public partial class LuckSystem : BaseSystem
{
    private static LuckSystem _instance;
    public static LuckSystem Instance => _instance;
    
    protected override string SystemName => "LuckSystem";
    
    private LuckData _data;
    private Random _random = new Random();
    
    // 事件信号
    public static Action<LuckResult, int, int> OnLuckRoll;
    public static Action<string, int> OnModifierAdded;
    public static Action<string> OnModifierRemoved;
    public static Action<int> OnLuckChanged;
    
    public LuckSystem()
    {
        _instance = this;
        _data = new LuckData();
        LuckDatabase.Initialize();
    }
    
    public void Initialize()
    {
        LoadData();
    }
    
    // 获取当前总幸运值
    public int GetCurrentLuck()
    {
        int total = _data.BaseLuck;
        
        // 加上所有活跃修饰器
        foreach (var mod in _data.ActiveModifiers)
        {
            total += mod.Value;
        }
        
        return Mathf.Clamp(total, 0, 100);
    }
    
    // 获取基础幸运值
    public int GetBaseLuck() => _data.BaseLuck;
    
    // 设置基础幸运值
    public void SetBaseLuck(int value)
    {
        _data.BaseLuck = Mathf.Clamp(value, 0, 100);
        OnLuckChanged?.Invoke(GetCurrentLuck());
    }
    
    // 添加幸运修饰器
    public void AddModifier(string name, int value, string source, int duration = 0)
    {
        var modifier = new LuckModifier
        {
            Name = name,
            Value = value,
            Source = source,
            Duration = duration,
            AppliedAt = DateTime.Now
        };
        
        _data.ActiveModifiers.Add(modifier);
        _data.TotalLuckBonus += value;
        
        OnModifierAdded?.Invoke(name, value);
        OnLuckChanged?.Invoke(GetCurrentLuck());
        
        SaveData();
    }
    
    // 移除幸运修饰器
    public void RemoveModifier(string source)
    {
        var toRemove = _data.ActiveModifiers.FindAll(m => m.Source == source);
        foreach (var mod in toRemove)
        {
            _data.ActiveModifiers.Remove(mod);
            OnModifierRemoved?.Invoke(mod.Name);
        }
        
        if (toRemove.Count > 0)
        {
            OnLuckChanged?.Invoke(GetCurrentLuck());
            SaveData();
        }
    }
    
    // 获取活跃修饰器列表
    public List<LuckModifier> GetActiveModifiers() => new List<LuckModifier>(_data.ActiveModifiers);
    
    // 执行幸运判定
    public LuckResult Roll(int difficulty = 50)
    {
        int currentLuck = GetCurrentLuck();
        int roll = _random.Next(1, 101);  // 1-100
        
        // 幸运影响投骰结果
        int modifiedRoll = roll + (currentLuck - 50) / 5;  // 每5点幸运调整1点
        
        LuckResult result;
        
        if (modifiedRoll <= 5)
            result = LuckResult.CriticalFailure;
        else if (modifiedRoll <= 20)
            result = LuckResult.Failure;
        else if (modifiedRoll <= 40)
            result = LuckResult.LowSuccess;
        else if (modifiedRoll <= 60)
            result = LuckResult.Success;
        else if (modifiedRoll <= 80)
            result = LuckResult.HighSuccess;
        else
            result = LuckResult.CriticalSuccess;
        
        // 记录事件
        _data.TotalLuckyRolls++;
        if (result == LuckResult.CriticalSuccess || result == LuckResult.CriticalFailure)
            _data.CriticalLuckRolls++;
        if (result == LuckResult.Failure || result == LuckResult.CriticalFailure)
            _data.FailedLuckRolls++;
        
        var luckEvent = new LuckEvent
        {
            Type = "roll",
            Value = roll,
            Result = modifiedRoll,
            Source = $"difficulty:{difficulty}"
        };
        _data.History.Add(luckEvent);
        
        // 限制历史记录数量
        if (_data.History.Count > 100)
            _data.History.RemoveAt(0);
        
        OnLuckRoll?.Invoke(result, roll, modifiedRoll);
        SaveData();
        
        return result;
    }
    
    // 简单幸运判定（返回是否成功）
    public bool SimpleRoll(int successChance)
    {
        int roll = _random.Next(1, 101);
        int currentLuck = GetCurrentLuck();
        
        // 幸运调整成功概率
        int adjustedChance = successChance + (currentLuck - 50) / 4;
        adjustedChance = Mathf.Clamp(adjustedChance, 5, 95);
        
        bool success = roll <= adjustedChance;
        
        // 记录
        _data.TotalLuckyRolls++;
        if (!success)
            _data.FailedLuckRolls++;
        
        var luckEvent = new LuckEvent
        {
            Type = "roll",
            Value = roll,
            Result = adjustedChance,
            Source = $"chance:{successChance}%"
        };
        _data.History.Add(luckEvent);
        
        if (_data.History.Count > 100)
            _data.History.RemoveAt(0);
        
        SaveData();
        
        return success;
    }
    
    // 计算掉落率加成
    public float GetDropRateBonus()
    {
        int luck = GetCurrentLuck();
        // 50点幸运 = 1.0x, 100点 = 1.5x, 0点 = 0.5x
        return 0.5f + (luck / 100f) * 1.0f;
    }
    
    // 计算暴击率加成
    public float GetCriticalRateBonus()
    {
        int luck = GetCurrentLuck();
        // 每10点幸运增加1%暴击率（基础5%）
        return 0.05f + (luck / 1000f);
    }
    
    // 获取稀有度加权（用于随机选择）
    public int GetRarityWeight(int baseWeight)
    {
        int luck = GetCurrentLuck();
        // 幸运增加权重
        float multiplier = 1.0f + (luck - 50) / 100f;
        return (int)(baseWeight * multiplier);
    }
    
    // 更新临时修饰器持续时间
    public void Update(float delta)
    {
        List<LuckModifier> expired = new List<LuckModifier>();
        
        foreach (var mod in _data.ActiveModifiers)
        {
            if (mod.Duration > 0)
            {
                TimeSpan elapsed = DateTime.Now - mod.AppliedAt;
                if (elapsed.TotalSeconds >= mod.Duration)
                {
                    expired.Add(mod);
                }
            }
        }
        
        foreach (var mod in expired)
        {
            _data.ActiveModifiers.Remove(mod);
            OnModifierRemoved?.Invoke(mod.Name);
        }
        
        if (expired.Count > 0)
        {
            OnLuckChanged?.Invoke(GetCurrentLuck());
            SaveData();
        }
    }
    
    // 获取统计信息
    public int GetTotalRolls() => _data.TotalLuckyRolls;
    public int GetCriticalRolls() => _data.CriticalLuckRolls;
    public int GetFailedRolls() => _data.FailedLuckRolls;
    public float GetSuccessRate()
    {
        if (_data.TotalLuckyRolls == 0) return 0;
        return (float)(_data.TotalLuckyRolls - _data.FailedLuckRolls) / _data.TotalLuckyRolls;
    }
    
    // 获取历史记录
    public List<LuckEvent> GetHistory(int count = 10)
    {
        int start = Math.Max(0, _data.History.Count - count);
        return _data.History.GetRange(start, _data.History.Count - start);
    }
    
    // 区域效果应用
    public void ApplyZoneEffect(string zoneId)
    {
        var zone = LuckDatabase.GetZone(zoneId);
        if (zone != null)
        {
            AddModifier($"区域:{zone.Name}", zone.LuckModifier, $"zone:{zoneId}", 0);
        }
    }
    
    // 天气效果应用
    public void ApplyWeatherEffect(string weatherId)
    {
        var weather = LuckDatabase.GetWeather(weatherId);
        if (weather != null)
        {
            AddModifier($"天气:{weather.Name}", weather.LuckModifier, $"weather:{weatherId}", 0);
        }
    }
    
    // 清理区域/天气效果
    public void ClearZoneWeatherEffects()
    {
        var toRemove = _data.ActiveModifiers.FindAll(m => 
            m.Source.StartsWith("zone:") || m.Source.StartsWith("weather:"));
        
        foreach (var mod in toRemove)
        {
            _data.ActiveModifiers.Remove(mod);
            OnModifierRemoved?.Invoke(mod.Name);
        }
        
        if (toRemove.Count > 0)
        {
            OnLuckChanged?.Invoke(GetCurrentLuck());
            SaveData();
        }
    }
    
    // 存档
    public void SaveData()
    {
        // JSON 序列化
        var json = JsonSerializer.Serialize(_data);
        // 这里应该是实际的文件写入操作
        // File.WriteAllText(...)
    }
    
    // 读档
    public void LoadData()
    {
        // JSON 反序列化
        // var json = File.ReadAllText(...);
        // _data = JsonSerializer.Deserialize<LuckData>(json);
    }
    
    /// <summary>
    /// Export save data (BaseSystem override)
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        data["baseLuck"] = _data.BaseLuck;
        data["activeModifiers"] = _data.ActiveModifiers;
        data["totalLuckyRolls"] = _data.TotalLuckyRolls;
        data["criticalLuckRolls"] = _data.CriticalLuckRolls;
        data["failedLuckRolls"] = _data.FailedLuckRolls;
        data["history"] = _data.History;
        return data;
    }
    
    /// <summary>
    /// Import save data (BaseSystem override)
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("baseLuck"))
        {
            _data.BaseLuck = (int)data["baseLuck"];
        }
        if (data.Contains("activeModifiers"))
        {
            _data.ActiveModifiers = (List<LuckModifier>)data["activeModifiers"];
        }
        if (data.Contains("totalLuckyRolls"))
        {
            _data.TotalLuckyRolls = (int)data["totalLuckyRolls"];
        }
        if (data.Contains("criticalLuckRolls"))
        {
            _data.CriticalLuckRolls = (int)data["criticalLuckRolls"];
        }
        if (data.Contains("failedLuckRolls"))
        {
            _data.FailedLuckRolls = (int)data["failedLuckRolls"];
        }
        if (data.Contains("history"))
        {
            _data.History = (List<LuckEvent>)data["history"];
        }
    }
}
