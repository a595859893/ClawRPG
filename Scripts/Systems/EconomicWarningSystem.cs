using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class EconomicWarningSystem : BaseSystem
{
    private EconomicWarningData _data;
    private EconomicWarningDatabase _database;
    private Dictionary<string, float> _indicatorValues = new Dictionary<string, float>();
    private Dictionary<string, float> _lastWarningTimes = new Dictionary<string, float>();
    private bool _isEnabled = true;
    private float _checkInterval = 60f;
    private float _timer = 0f;

    public override void _Ready()
    {
        _database = EconomicWarningDatabase.Instance;
        LoadData();
        UpdateIndicatorValues();
    }
    
    /// <summary>
    /// 系统名称
    /// </summary>
    protected override string SystemName => "EconomicWarning";
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        if (_data != null)
        {
            data["enabled"] = _isEnabled;
            data["check_interval"] = _checkInterval;
        }
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.Contains("enabled"))
        {
            _isEnabled = (bool)data["enabled"];
        }
        
        if (data.Contains("check_interval"))
        {
            _checkInterval = (float)data["check_interval"];
        }
    }
    
    public void LoadData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem != null)
        {
            var loadedData = saveSystem.LoadData<EconomicWarningData>("economic_warning_data");
            if (loadedData != null)
            {
                _data = loadedData;
            }
            else
            {
                _data = new EconomicWarningData();
            }
        }
        else
        {
            _data = new EconomicWarningData();
        }
    }

    public void SaveData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem != null)
        {
            saveSystem.SaveData(_data, "economic_warning_data");
        }
    }

    public override void _Process(double delta)
    {
        if (!_isEnabled) return;

        _timer += delta;
        if (_timer >= _checkInterval)
        {
            _timer = 0f;
            RunEconomicCheck();
        }
    }

    public void UpdateIndicatorValues()
    {
        // Get values from game systems
        var player = GetTree().GetNodesInGroup("player").FirstOrDefault() as Player;
        var auctionSystem = GetNode<AuctionHouseSystem>("/root/AuctionHouseSystem");
        var tradeSystem = GetNode<TradeRouteSystem>("/root/TradeRouteSystem");
        var marketTaxSystem = GetNode<DynamicMarketTaxSystem>("/root/DynamicMarketTaxSystem");

        // Calculate indicator values
        float goldPerPlayer = player != null ? player.Gold : 10000f;
        _indicatorValues["gold_per_player"] = goldPerPlayer;

        // Trade volume (mock - would come from auction/trade systems)
        _indicatorValues["trade_volume"] = auctionSystem != null ? 
            auctionSystem.GetTotalActiveListings() : 100f;

        // Sink ratio (mock - would calculate from actual sinks/sources)
        _indicatorValues["sink_ratio"] = 0.9f;

        // Inflation rate (mock - would calculate from price changes)
        _indicatorValues["inflation_rate"] = 0.02f;

        // Item turnover
        _indicatorValues["item_turnover"] = 0.4f;
    }

    public void RunEconomicCheck()
    {
        UpdateIndicatorValues();

        foreach (var config in _database.WarningConfigs)
        {
            if (ShouldCheckWarning(config))
            {
                CheckWarningCondition(config);
            }
        }

        _data.LastFullCheckTime = Time.GetTicksMsec() / 1000f;
        SaveData();
    }

    private bool ShouldCheckWarning(WarningConfig config)
    {
        if (_lastWarningTimes.TryGetValue(config.WarningId, out float lastTime))
        {
            return (Time.GetTicksMsec() / 1000f) - lastTime >= config.CheckInterval;
        }
        return true;
    }

    private void CheckWarningCondition(WarningConfig config)
    {
        float currentValue = 0f;
        switch (config.WarningId)
        {
            case "inflation_rate_high":
            case "inflation_rate_critical":
                _indicatorValues.TryGetValue("inflation_rate", out currentValue);
                break;
            case "deflation_detected":
                _indicatorValues.TryGetValue("inflation_rate", out currentValue);
                break;
            case "gold_accumulation":
                _indicatorValues.TryGetValue("gold_per_player", out currentValue);
                break;
            case "sink_efficiency_low":
                _indicatorValues.TryGetValue("sink_ratio", out currentValue);
                break;
            case "trade_volume_low":
                _indicatorValues.TryGetValue("trade_volume", out currentValue);
                break;
            default:
                return;
        }

        bool shouldTrigger = false;
        switch (config.WarningType)
        {
            case "Inflation":
                shouldTrigger = currentValue > config.Threshold;
                break;
            case "Deflation":
                shouldTrigger = currentValue < config.Threshold;
                break;
            case "Accumulation":
                shouldTrigger = currentValue > config.Threshold;
                break;
            case "Economy":
                shouldTrigger = currentValue < config.Threshold;
                break;
            case "Trade":
                // For low trade volume, check if it's below threshold percentage
                float healthyMin = _database.IndicatorConfigs
                    .FirstOrDefault(i => i.IndicatorId == "trade_volume")?.HealthyRange.x ?? 100f;
                shouldTrigger = currentValue < healthyMin * (1 - config.Threshold);
                break;
        }

        if (shouldTrigger)
        {
            GenerateWarning(config, currentValue);
        }
        else
        {
            ResolveWarning(config.WarningId);
        }
    }

    private void GenerateWarning(WarningConfig config, float currentValue)
    {
        // Check if warning already exists
        var existingWarning = _data.ActiveWarnings.FirstOrDefault(w => 
            w.WarningId == config.WarningId && w.IsActive);

        if (existingWarning != null) return;

        var warning = new WarningRecord
        {
            WarningId = config.WarningId,
            WarningType = config.WarningType,
            Title = config.Title,
            Description = config.Description,
            Severity = config.Severity,
            Value = currentValue,
            Threshold = config.Threshold,
            Timestamp = Time.GetTicksMsec() / 1000f,
            IsActive = true,
            RecommendedAction = config.RecommendedAction
        };

        _data.ActiveWarnings.Add(warning);
        _data.WarningHistory.Add(warning);
        _data.Statistics.TotalWarningsGenerated++;
        _data.Statistics.WarningsTriggered++;

        if (!_data.Statistics.WarningTypeCounts.ContainsKey(config.WarningType))
            _data.Statistics.WarningTypeCounts[config.WarningType] = 0;
        _data.Statistics.WarningTypeCounts[config.WarningType]++;

        if (config.Severity == WarningSeverity.Critical)
            _data.Statistics.CriticalWarnings++;

        _lastWarningTimes[config.WarningId] = Time.GetTicksMsec() / 1000f;

        GD.Print($"[EconomicWarning] {config.Title} - Value: {currentValue:F2}, Threshold: {config.Threshold:F2}");
    }

    private void ResolveWarning(string warningId)
    {
        var warning = _data.ActiveWarnings.FirstOrDefault(w => w.WarningId == warningId);
        if (warning != null)
        {
            warning.IsActive = false;
            _data.ActiveWarnings.Remove(warning);
            _data.Statistics.WarningsResolved++;
            GD.Print($"[EconomicWarning] Resolved: {warning.Title}");
        }
    }

    public void AcknowledgeWarning(string warningId)
    {
        if (!_data.AcknowledgedWarnings.Contains(warningId))
        {
            _data.AcknowledgedWarnings.Add(warningId);
        }
    }

    public void ClearAcknowledgedWarnings()
    {
        _data.AcknowledgedWarnings.Clear();
    }

    public List<WarningRecord> GetActiveWarnings()
    {
        return _data.ActiveWarnings.Where(w => w.IsActive).ToList();
    }

    public List<WarningRecord> GetAllWarnings()
    {
        return _data.WarningHistory.OrderByDescending(w => w.Timestamp).ToList();
    }

    public List<WarningRecord> GetWarningsByType(string warningType)
    {
        return _data.WarningHistory.Where(w => w.WarningType == warningType)
            .OrderByDescending(w => w.Timestamp).ToList();
    }

    public List<WarningRecord> GetWarningsBySeverity(WarningSeverity severity)
    {
        return _data.WarningHistory.Where(w => w.Severity == severity)
            .OrderByDescending(w => w.Timestamp).ToList();
    }

    public Dictionary<string, float> GetIndicatorValues()
    {
        return new Dictionary<string, float>(_indicatorValues);
    }

    public float GetIndicatorValue(string indicatorId)
    {
        return _indicatorValues.TryGetValue(indicatorId, out float value) ? value : 0f;
    }

    public bool IsIndicatorHealthy(string indicatorId)
    {
        var config = _database.IndicatorConfigs.FirstOrDefault(i => i.IndicatorId == indicatorId);
        if (config == null) return true;

        if (_indicatorValues.TryGetValue(indicatorId, out float value))
        {
            return value >= config.HealthyRange.x && value <= config.HealthyRange.y;
        }
        return true;
    }

    public EconomicStatistics GetStatistics()
    {
        return _data.Statistics;
    }

    public void SetEnabled(bool enabled)
    {
        _isEnabled = enabled;
    }

    public void SetCheckInterval(float interval)
    {
        _checkInterval = Mathf.Max(10f, interval);
    }

    public void ManualCheck()
    {
        RunEconomicCheck();
    }

    public void ClearAllWarnings()
    {
        _data.ActiveWarnings.Clear();
        _data.WarningHistory.Clear();
        _lastWarningTimes.Clear();
        SaveData();
    }

    public void ResetStatistics()
    {
        _data.Statistics = new EconomicStatistics();
        SaveData();
    }
}
