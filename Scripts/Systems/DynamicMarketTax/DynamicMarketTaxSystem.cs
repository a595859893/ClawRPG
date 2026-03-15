using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 动态市场税率系统 - 根据市场热度自动调整税率
/// 基于 Game Economy Design 学习成果
/// </summary>
public class MarketTaxData : BaseSystem
{
    // 基础税率配置
    public float BaseTaxRate { get; set; } = 5.0f;
    
    // 当前动态税率
    public float CurrentDynamicTaxRate { get; set; } = 5.0f;
    
    // 市场热度指标 (0-100)
    public float MarketActivity { get; set; } = 50.0f;
    
    // 税收历史记录
    public List<TaxRecord> TaxHistory { get; set; } = new List<TaxRecord>();
    
    // 统计
    public int TotalTransactions { get; set; }
    public long TotalTaxCollected { get; set; }
    public long TotalVolume { get; set; }
    public float AverageTransactionValue { get; set; }
    public float PeakVolume { get; set; }
    public long LastTaxUpdate { get; set; }
    
    // 市场趋势
    public string MarketTrend { get; set; } = "Stable";
    public int ConsecutiveHighActivity { get; set; }
    public int ConsecutiveLowActivity { get; set; }
    
    public override void _Ready()
    {
        LastTaxUpdate = OS.GetSystemTimeMsecs();
    }
}

public class TaxRecord
{
    public long Timestamp { get; set; }
    public int TransactionCount { get; set; }
    public long Volume { get; set; }
    public long TaxCollected { get; set; }
    public float TaxRate { get; set; }
    public float MarketActivity { get; set; }
}

public class DynamicMarketTaxSystem : BaseSystem
{
    private MarketTaxData _data;
    
    // 税率配置
    private const float MIN_TAX_RATE = 3.0f;      // 最低税率 3%
    private const float MAX_TAX_RATE = 15.0f;     // 最高税率 15%
    private const float BASE_TAX_RATE = 5.0f;     // 基础税率 5%
    
    // 市场热度阈值
    private const float HIGH_ACTIVITY_THRESHOLD = 70.0f;
    private const float LOW_ACTIVITY_THRESHOLD = 30.0f;
    
    // 税率调整参数
    private const float TAX_ADJUSTMENT_PER_POINT = 0.1f;  // 每点热度调整 0.1%
    private const int TAX_UPDATE_INTERVAL = 60000;        // 每分钟更新一次
    
    // 趋势判断
    private const int TREND_WINDOW_SIZE = 5;
    private List<float> _activityHistory = new List<float>();
    
    public override void _Ready()
    {
        _data = new MarketTaxData();
        AddChild(_data);
        
        AddToGroup("save");
        AddToGroup("dynamic_market_tax");
        
        GD.Print("DynamicMarketTaxSystem: 动态市场税率系统已初始化");
        GD.Print($"  基础税率: {BASE_TAX_RATE}%");
        GD.Print($"  税率范围: {MIN_TAX_RATE}% - {MAX_TAX_RATE}%");
    }
    
    /// <summary>
    /// 记录一笔交易
    /// </summary>
    public void RecordTransaction(int transactionCount, long volume)
    {
        if (volume <= 0) return;
        
        long currentTime = OS.GetSystemTimeMsecs();
        
        // 更新统计
        _data.TotalTransactions += transactionCount;
        _data.TotalVolume += volume;
        
        // 计算税收
        long tax = (long)(volume * _data.CurrentDynamicTaxRate / 100.0);
        _data.TotalTaxCollected += tax;
        
        // 更新平均交易额
        if (_data.TotalTransactions > 0)
        {
            _data.AverageTransactionValue = (float)_data.TotalVolume / _data.TotalTransactions;
        }
        
        // 更新峰值交易量
        if (volume > _data.PeakVolume)
        {
            _data.PeakVolume = volume;
        }
        
        // 记录到历史
        var record = new TaxRecord
        {
            Timestamp = currentTime,
            TransactionCount = transactionCount,
            Volume = volume,
            TaxCollected = tax,
            TaxRate = _data.CurrentDynamicTaxRate,
            MarketActivity = _data.MarketActivity
        };
        _data.TaxHistory.Add(record);
        
        // 限制历史记录大小
        if (_data.TaxHistory.Count > 1000)
        {
            _data.TaxHistory.RemoveAt(0);
        }
        
        // 更新市场热度
        UpdateMarketActivity(transactionCount, volume);
        
        // 检查是否需要更新税率
        if (currentTime - _data.LastTaxUpdate > TAX_UPDATE_INTERVAL)
        {
            UpdateDynamicTaxRate();
            _data.LastTaxUpdate = currentTime;
        }
        
        SaveTaxData();
    }
    
    /// <summary>
    /// 更新市场热度
    /// </summary>
    private void UpdateMarketActivity(int transactionCount, long volume)
    {
        // 基于交易数量和交易量计算热度
        float activityFromTransactions = Mathf.Clamp(transactionCount * 2.0f, 0, 30);
        float activityFromVolume = Mathf.Clamp(volume / 10000.0f, 0, 40);
        
        // 随机波动
        float randomFluctuation = GD.Randf() * 10 - 5;
        
        // 目标热度
        float targetActivity = 50.0f + activityFromTransactions + activityFromVolume + randomFluctuation;
        targetActivity = Mathf.Clamp(targetActivity, 0, 100);
        
        // 平滑过渡
        _data.MarketActivity = Mathf.Lerp(_data.MarketActivity, targetActivity, 0.3f);
        
        // 记录到活动历史
        _activityHistory.Add(_data.MarketActivity);
        if (_activityHistory.Count > TREND_WINDOW_SIZE)
        {
            _activityHistory.RemoveAt(0);
        }
        
        // 更新趋势
        UpdateMarketTrend();
    }
    
    /// <summary>
    /// 更新市场趋势
    /// </summary>
    private void UpdateMarketTrend()
    {
        if (_activityHistory.Count < TREND_WINDOW_SIZE)
        {
            _data.MarketTrend = "Stable";
            return;
        }
        
        float sum = 0;
        for (int i = 1; i < _activityHistory.Count; i++)
        {
            sum += _activityHistory[i] - _activityHistory[i - 1];
        }
        float avgChange = sum / (_activityHistory.Count - 1);
        
        if (avgChange > 2.0f)
        {
            _data.MarketTrend = "Rising";
            _data.ConsecutiveHighActivity++;
            _data.ConsecutiveLowActivity = 0;
        }
        else if (avgChange < -2.0f)
        {
            _data.MarketTrend = "Falling";
            _data.ConsecutiveLowActivity++;
            _data.ConsecutiveHighActivity = 0;
        }
        else
        {
            _data.MarketTrend = "Stable";
            _data.ConsecutiveHighActivity = 0;
            _data.ConsecutiveLowActivity = 0;
        }
    }
    
    /// <summary>
    /// 更新动态税率
    /// </summary>
    private void UpdateDynamicTaxRate()
    {
        // 基于市场热度的税率调整
        float activityOffset = _data.MarketActivity - 50.0f;  // -50 to +50
        float taxAdjustment = activityOffset * TAX_ADJUSTMENT_PER_POINT;
        
        // 基础税率 + 调整
        float newTaxRate = BASE_TAX_RATE + taxAdjustment;
        newTaxRate = Mathf.Clamp(newTaxRate, MIN_TAX_RATE, MAX_TAX_RATE);
        
        // 趋势加成/减成
        if (_data.MarketTrend == "Rising" && _data.ConsecutiveHighActivity >= 3)
        {
            // 市场火热，提高税率抑制过度交易
            newTaxRate += 1.0f;
        }
        else if (_data.MarketTrend == "Falling" && _data.ConsecutiveLowActivity >= 3)
        {
            // 市场冷清，降低税率刺激交易
            newTaxRate -= 1.0f;
        }
        
        newTaxRate = Mathf.Clamp(newTaxRate, MIN_TAX_RATE, MAX_TAX_RATE);
        
        _data.CurrentDynamicTaxRate = newTaxRate;
        
        GD.Print($"DynamicMarketTaxSystem: 税率更新 - {_data.CurrentDynamicTaxRate}% (热度: {_data.MarketActivity:F1}%, 趋势: {_data.MarketTrend})");
    }
    
    /// <summary>
    /// 获取当前税率
    /// </summary>
    public float GetCurrentTaxRate()
    {
        return _data.CurrentDynamicTaxRate;
    }
    
    /// <summary>
    /// 获取市场热度
    /// </summary>
    public float GetMarketActivity()
    {
        return _data.MarketActivity;
    }
    
    /// <summary>
    /// 获取市场趋势
    /// </summary>
    public string GetMarketTrend()
    {
        return _data.MarketTrend;
    }
    
    /// <summary>
    /// 获取税收统计
    /// </summary>
    public Dictionary<string, object> GetTaxStatistics()
    {
        return new Dictionary<string, object>
        {
            { "totalTransactions", _data.TotalTransactions },
            { "totalTaxCollected", _data.TotalTaxCollected },
            { "totalVolume", _data.TotalVolume },
            { "averageTransactionValue", _data.AverageTransactionValue },
            { "peakVolume", _data.PeakVolume },
            { "currentTaxRate", _data.CurrentDynamicTaxRate },
            { "marketActivity", _data.MarketActivity },
            { "marketTrend", _data.MarketTrend },
            { "baseTaxRate", BASE_TAX_RATE },
            { "minTaxRate", MIN_TAX_RATE },
            { "maxTaxRate", MAX_TAX_RATE }
        };
    }
    
    /// <summary>
    /// 获取税率状态描述
    /// </summary>
    public string GetTaxStatusDescription()
    {
        string status;
        if (_data.CurrentDynamicTaxRate <= MIN_TAX_RATE + 1.0f)
            status = "低税率 - 刺激市场";
        else if (_data.CurrentDynamicTaxRate >= MAX_TAX_RATE - 1.0f)
            status = "高税率 - 抑制过热";
        else if (_data.MarketActivity > HIGH_ACTIVITY_THRESHOLD)
            status = "市场活跃 - 税率上调";
        else if (_data.MarketActivity < LOW_ACTIVITY_THRESHOLD)
            status = "市场冷清 - 税率下调";
        else
            status = "市场稳定 - 正常税率";
        
        return $"{status} (税率: {_data.CurrentDynamicTaxRate:F1}%, 热度: {_data.MarketActivity:F1}%)";
    }
    
    /// <summary>
    /// 模拟交易用于测试
    /// </summary>
    public void SimulateTransaction(int transactionCount = 1, long volume = 1000)
    {
        RecordTransaction(transactionCount, volume);
    }
    
    private void SaveTaxData()
    {
        // 保存到文件
        string savePath = "user://dynamic_market_tax.save";
        using (var file = new File())
        {
            if (file.Open(savePath, File.ModeFlags.Write))
            {
                var saveData = new Dictionary<string, object>
                {
                    { "baseTaxRate", _data.BaseTaxRate },
                    { "currentDynamicTaxRate", _data.CurrentDynamicTaxRate },
                    { "marketActivity", _data.MarketActivity },
                    { "totalTransactions", _data.TotalTransactions },
                    { "totalTaxCollected", _data.TotalTaxCollected },
                    { "totalVolume", _data.TotalVolume },
                    { "averageTransactionValue", _data.AverageTransactionValue },
                    { "peakVolume", _data.PeakVolume },
                    { "lastTaxUpdate", _data.LastTaxUpdate },
                    { "marketTrend", _data.MarketTrend },
                    { "consecutiveHighActivity", _data.ConsecutiveHighActivity },
                    { "consecutiveLowActivity", _data.ConsecutiveLowActivity }
                };
                
                string json = JSON.Print(saveData);
                file.StoreString(json);
                file.Close();
            }
        }
    }
    
    private void LoadTaxData()
    {
        string savePath = "user://dynamic_market_tax.save";
        using (var file = new File())
        {
            if (file.FileExists(savePath))
            {
                if (file.Open(savePath, File.ModeFlags.Read))
                {
                    string json = file.GetAsText();
                    file.Close();
                    
                    var result = JSON.Parse(json);
                    if (result.Error == Error.Ok)
                    {
                        var saveData = result.Result as Dictionary<string, object>;
                        if (saveData != null)
                        {
                            _data.BaseTaxRate = (float)saveData.Get("baseTaxRate", 5.0);
                            _data.CurrentDynamicTaxRate = (float)saveData.Get("currentDynamicTaxRate", 5.0);
                            _data.MarketActivity = (float)saveData.Get("marketActivity", 50.0);
                            _data.TotalTransactions = (int)saveData.Get("totalTransactions", 0);
                            _data.TotalTaxCollected = (long)saveData.Get("totalTaxCollected", 0L);
                            _data.TotalVolume = (long)saveData.Get("totalVolume", 0L);
                            _data.AverageTransactionValue = (float)saveData.Get("averageTransactionValue", 0.0);
                            _data.PeakVolume = (float)saveData.Get("peakVolume", 0.0);
                            _data.LastTaxUpdate = (long)saveData.Get("lastTaxUpdate", OS.GetSystemTimeMsecs());
                            _data.MarketTrend = (string)saveData.Get("marketTrend", "Stable");
                            _data.ConsecutiveHighActivity = (int)saveData.Get("consecutiveHighActivity", 0);
                            _data.ConsecutiveLowActivity = (int)saveData.Get("consecutiveLowActivity", 0);
                            
                            GD.Print("DynamicMarketTaxSystem: 税收数据已加载");
                        }
                    }
                }
            }
        }
    }
    
    /// <summary>
    /// 重置统计数据
    /// </summary>
    public void ResetStatistics()
    {
        _data.TotalTransactions = 0;
        _data.TotalTaxCollected = 0;
        _data.TotalVolume = 0;
        _data.AverageTransactionValue = 0;
        _data.PeakVolume = 0;
        _data.TaxHistory.Clear();
        _activityHistory.Clear();
        
        GD.Print("DynamicMarketTaxSystem: 统计数据已重置");
        SaveTaxData();
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 税率数据
        data["base_tax_rate"] = _data.BaseTaxRate;
        data["current_dynamic_tax_rate"] = _data.CurrentDynamicTaxRate;
        data["market_activity"] = _data.MarketActivity;
        
        // 统计
        data["total_transactions"] = _data.TotalTransactions;
        data["total_tax_collected"] = _data.TotalTaxCollected;
        data["total_volume"] = _data.TotalVolume;
        data["average_transaction_value"] = _data.AverageTransactionValue;
        data["peak_volume"] = _data.PeakVolume;
        
        // 市场趋势
        data["market_trend"] = _data.MarketTrend;
        data["consecutive_high_activity"] = _data.ConsecutiveHighActivity;
        data["consecutive_low_activity"] = _data.ConsecutiveLowActivity;
        
        // 活动历史
        var activityHistoryArray = new Array();
        foreach (var activity in _activityHistory)
        {
            activityHistoryArray.Add(activity);
        }
        data["activity_history"] = activityHistoryArray;
        
        return data;
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("base_tax_rate")) _data.BaseTaxRate = (float)data["base_tax_rate"];
        if (data.Contains("current_dynamic_tax_rate")) _data.CurrentDynamicTaxRate = (float)data["current_dynamic_tax_rate"];
        if (data.Contains("market_activity")) _data.MarketActivity = (float)data["market_activity"];
        
        if (data.Contains("total_transactions")) _data.TotalTransactions = (int)data["total_transactions"];
        if (data.Contains("total_tax_collected")) _data.TotalTaxCollected = Convert.ToInt64(data["total_tax_collected"]);
        if (data.Contains("total_volume")) _data.TotalVolume = Convert.ToInt64(data["total_volume"]);
        if (data.Contains("average_transaction_value")) _data.AverageTransactionValue = (float)data["average_transaction_value"];
        if (data.Contains("peak_volume")) _data.PeakVolume = (float)data["peak_volume"];
        
        if (data.Contains("market_trend")) _data.MarketTrend = (string)data["market_trend"];
        if (data.Contains("consecutive_high_activity")) _data.ConsecutiveHighActivity = (int)data["consecutive_high_activity"];
        if (data.Contains("consecutive_low_activity")) _data.ConsecutiveLowActivity = (int)data["consecutive_low_activity"];
        
        // 活动历史
        _activityHistory.Clear();
        if (data.Contains("activity_history"))
        {
            var activityHistoryArray = (Array)data["activity_history"];
            foreach (var activity in activityHistoryArray)
            {
                _activityHistory.Add((float)activity);
            }
        }
    }
}
