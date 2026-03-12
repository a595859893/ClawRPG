using Godot;
using System;
using System.Collections.Generic;

public class EconomicWarningDatabase
{
    private static EconomicWarningDatabase _instance;
    public static EconomicWarningDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new EconomicWarningDatabase();
            return _instance;
        }
    }

    public List<WarningConfig> WarningConfigs { get; private set; } = new List<WarningConfig>();
    public List<EconomicIndicatorConfig> IndicatorConfigs { get; private set; } = new List<EconomicIndicatorConfig>();

    public EconomicWarningDatabase()
    {
        InitializeWarningConfigs();
        InitializeIndicatorConfigs();
    }

    private void InitializeWarningConfigs()
    {
        // Inflation Warnings
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "inflation_rate_high",
            WarningType = "Inflation",
            Title = "High Inflation Rate",
            Description = "Currency inflation is above normal levels. Gold is losing purchasing power.",
            Severity = WarningSeverity.Warning,
            Threshold = 0.15f,
            RecommendedAction = "Consider increasing sinks: higher repair costs, more expensive crafting, or event taxes.",
            CheckInterval = 300f
        });

        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "inflation_rate_critical",
            WarningType = "Inflation",
            Title = "Critical Inflation",
            Description = "Currency inflation is at critical levels. Economy may collapse soon.",
            Severity = WarningSeverity.Critical,
            Threshold = 0.30f,
            RecommendedAction = "URGENT: Implement emergency sinks, reduce rewards, or introduce new expensive items.",
            CheckInterval = 60f
        });

        // Deflation Warnings
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "deflation_detected",
            WarningType = "Deflation",
            Title = "Deflation Detected",
            Description = "Currency value is increasing. Players may hoard gold instead of spending.",
            Severity = WarningSeverity.Warning,
            Threshold = -0.10f,
            RecommendedAction = "Consider reducing taxes, increasing rewards, or introducing time-limited offers.",
            CheckInterval = 300f
        });

        // Gold Accumulation Warnings
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "gold_accumulation",
            WarningType = "Accumulation",
            Title = "Gold Accumulation",
            Description = "Average player gold is increasing rapidly without corresponding spending.",
            Severity = WarningSeverity.Warning,
            Threshold = 50000f,
            RecommendedAction = "Add more sinks: housing upgrades, cosmetic items, or prestige systems.",
            CheckInterval = 600f
        });

        // Market Volatility Warnings
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "market_volatility_high",
            WarningType = "Market",
            Title = "High Market Volatility",
            Description = "Item prices are fluctuating wildly. Market is unstable.",
            Severity = WarningSeverity.Warning,
            Threshold = 0.40f,
            RecommendedAction = "Review recent price changes and consider stabilizing market interventions.",
            CheckInterval = 300f
        });

        // Item Price Warnings
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "item_price_spike",
            WarningType = "ItemPrice",
            Title = "Item Price Spike",
            Description = "Specific item prices have increased dramatically.",
            Severity = WarningSeverity.Info,
            Threshold = 0.50f,
            RecommendedAction = "Monitor for potential manipulation or supply issues.",
            CheckInterval = 180f
        });

        // Trade Volume Warnings
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "trade_volume_low",
            WarningType = "Trade",
            Title = "Low Trade Volume",
            Description = "Market trading has decreased significantly. Economy may be slowing.",
            Severity = WarningSeverity.Info,
            Threshold = 0.30f,
            RecommendedAction = "Consider introducing new items or limited-time events to stimulate trading.",
            CheckInterval = 600f
        });

        // Income/Spend Ratio Warnings
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "income_spend_imbalance",
            WarningType = "Balance",
            Title = "Income/Spend Imbalance",
            Description = "More gold entering economy than leaving. Risk of inflation.",
            Severity = WarningSeverity.Warning,
            Threshold = 1.5f,
            RecommendedAction = "Review sink mechanics and consider adding more gold-consuming features.",
            CheckInterval = 300f
        });

        // Legendary Item Drop Rate
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "legendary_drop_too_high",
            WarningType = "Loot",
            Title = "Legendary Drop Rate Too High",
            Description = "Legendary items are too common. May devalue rare items.",
            Severity = WarningSeverity.Warning,
            Threshold = 0.05f,
            RecommendedAction = "Consider reducing drop rates or adding more rare variants.",
            CheckInterval = 600f
        });

        // Player Wealth Disparity
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "wealth_disparity",
            WarningType = "Balance",
            Title = "Wealth Disparity",
            Description = "Gap between richest and poorest players is growing.",
            Severity = WarningSeverity.Info,
            Threshold = 10f,
            RecommendedAction = "Consider catch-up mechanics or progressive reward systems.",
            CheckInterval = 600f
        });

        // Sink Efficiency
        WarningConfigs.Add(new WarningConfig
        {
            WarningId = "sink_efficiency_low",
            WarningType = "Economy",
            Title = "Low Sink Efficiency",
            Description = "Gold sinks are not removing enough currency from economy.",
            Severity = WarningSeverity.Warning,
            Threshold = 0.5f,
            RecommendedAction = "Add or enhance gold sinks: crafting, repair, trading taxes, etc.",
            CheckInterval = 300f
        });
    }

    private void InitializeIndicatorConfigs()
    {
        // Economic indicators to monitor
        IndicatorConfigs.Add(new EconomicIndicatorConfig
        {
            IndicatorId = "inflation_rate",
            Name = "Inflation Rate",
            Description = "Rate of currency value decrease over time",
            MinValue = -0.5f,
            MaxValue = 0.5f,
            HealthyRange = new Vector2(-0.05f, 0.05f)
        });

        IndicatorConfigs.Add(new EconomicIndicatorConfig
        {
            IndicatorId = "gold_per_player",
            Name = "Average Gold Per Player",
            Description = "Mean gold held by active players",
            MinValue = 0,
            MaxValue = 1000000,
            HealthyRange = new Vector2(5000, 50000)
        });

        IndicatorConfigs.Add(new EconomicIndicatorConfig
        {
            IndicatorId = "trade_volume",
            Name = "Trade Volume",
            Description = "Total items traded in market per hour",
            MinValue = 0,
            MaxValue = 10000,
            HealthyRange = new Vector2(100, 1000)
        });

        IndicatorConfigs.Add(new EconomicIndicatorConfig
        {
            IndicatorId = "sink_ratio",
            Name = "Sink Ratio",
            Description = "Ratio of gold sinks to gold sources",
            MinValue = 0,
            MaxValue = 2f,
            HealthyRange = new Vector2(0.8f, 1.2f)
        });

        IndicatorConfigs.Add(new EconomicIndicatorConfig
        {
            IndicatorId = "item_turnover",
            Name = "Item Turnover Rate",
            Description = "How quickly items change hands in market",
            MinValue = 0,
            MaxValue = 1f,
            HealthyRange = new Vector2(0.2f, 0.8f)
        });
    }

    public WarningConfig GetWarningConfig(string warningId)
    {
        return WarningConfigs.FirstOrDefault(w => w.WarningId == warningId);
    }

    public List<WarningConfig> GetWarningConfigsByType(string warningType)
    {
        return WarningConfigs.Where(w => w.WarningType == warningType).ToList();
    }

    public List<WarningConfig> GetWarningConfigsBySeverity(WarningSeverity severity)
    {
        return WarningConfigs.Where(w => w.Severity == severity).ToList();
    }
}

public class WarningConfig
{
    public string WarningId { get; set; }
    public string WarningType { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public WarningSeverity Severity { get; set; }
    public float Threshold { get; set; }
    public string RecommendedAction { get; set; }
    public float CheckInterval { get; set; } = 300f;
}

public class EconomicIndicatorConfig
{
    public string IndicatorId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public float MinValue { get; set; }
    public float MaxValue { get; set; }
    public Vector2 HealthyRange { get; set; }
}
