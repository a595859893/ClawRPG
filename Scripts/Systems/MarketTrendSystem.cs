using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Market trend data container that tracks item price history and statistics.
/// Used for market analysis and price prediction systems.
/// </summary>
public class MarketTrendSystem : BaseSystem
{
    /// <summary>
    /// Price history for each item category, keyed by category name.
    /// Each list contains PriceRecord entries ordered by timestamp.
    /// </summary>
    public Dictionary<string, List<PriceRecord>> PriceHistory = new Dictionary<string, List<PriceRecord>>();
    
    /// <summary>
    /// Current market trends for each category, tracking direction and strength.
    /// </summary>
    public Dictionary<string, MarketTrend> CurrentTrends = new Dictionary<string, MarketTrend>();
    
    /// <summary>
    /// Market predictions for each category based on trend analysis.
    /// </summary>
    public Dictionary<string, MarketPrediction> Predictions = new Dictionary<string, MarketPrediction>();
    
    /// <summary>
    /// Global market sentiment indicator ranging from -100 (extremely bearish) to 100 (extremely bullish).
    /// </summary>
    public int MarketSentiment = 0;
    
    /// <summary>
    /// Unix timestamp of the last market data update.
    /// </summary>
    public double LastUpdateTime = 0;
    
    /// <summary>
    /// Total number of market trend updates performed.
    /// </summary>
    public int TotalTrendUpdates = 0;
    
    /// <summary>
    /// Total number of predictions made by the system.
    /// </summary>
    public int TotalPredictionsMade = 0;
    
    /// <summary>
    /// Number of predictions that proved to be correct.
    /// Used for measuring prediction accuracy.
    /// </summary>
    public int CorrectPredictions = 0;
    
    public override void _Ready()
    {
        LastUpdateTime = Time.GetUnixTimeFromSystem();
    }

    /// <summary>
    /// Export save data for persistence.
    /// Persists market summary stats and current trends. Price history and predictions are not persisted
    /// as they are regenerated dynamically based on market activity.
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();

        // Persist global market stats
        data["market_sentiment"] = MarketSentiment;
        data["total_trend_updates"] = TotalTrendUpdates;
        data["total_predictions_made"] = TotalPredictionsMade;
        data["correct_predictions"] = CorrectPredictions;
        data["last_update_time"] = LastUpdateTime;

        // Persist current trends (small dict, meaningful accumulated state)
        List<object> trendsData = new List<object>();
        foreach (var kvp in CurrentTrends)
        {
            trendsData.Add(new Dictionary<string, object>
            {
                { "category", kvp.Key },
                { "direction", (int)kvp.Value.Direction },
                { "change_percent", kvp.Value.ChangePercent },
                { "volatility", kvp.Value.Volatility },
                { "trend_strength", kvp.Value.TrendStrength },
                { "duration", kvp.Value.Duration }
            });
        }
        data["current_trends"] = trendsData;

        return new Dictionary(data);
    }

    /// <summary>
    /// Import save data from persistence.
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;

        if (data.Contains("market_sentiment"))
            MarketSentiment = (int)data["market_sentiment"];
        if (data.Contains("total_trend_updates"))
            TotalTrendUpdates = (int)data["total_trend_updates"];
        if (data.Contains("total_predictions_made"))
            TotalPredictionsMade = (int)data["total_predictions_made"];
        if (data.Contains("correct_predictions"))
            CorrectPredictions = (int)data["correct_predictions"];
        if (data.Contains("last_update_time"))
            LastUpdateTime = (double)data["last_update_time"];

        // Restore current trends
        if (data.Contains("current_trends"))
        {
            CurrentTrends.Clear();
            var trendsData = (Godot.Collections.Array)data["current_trends"];
            foreach (var t in trendsData)
            {
                var tdict = (Dictionary)t;
                string category = (string)tdict["category"];
                var trend = new MarketTrend(category)
                {
                    Direction = (TrendDirection)(int)tdict["direction"],
                    ChangePercent = (float)tdict["change_percent"],
                    Volatility = (float)tdict["volatility"],
                    TrendStrength = (double)tdict["trend_strength"],
                    Duration = (int)tdict["duration"]
                };
                CurrentTrends[category] = trend;
            }
        }
    }
}

/// <summary>
/// Represents a single price record at a specific point in time.
/// </summary>
[System.Serializable]
public class PriceRecord
{
    public double Timestamp;
    public float Price;
    public int Volume;

    public PriceRecord(double timestamp, float price, int volume)
    {
        Timestamp = timestamp;
        Price = price;
        Volume = volume;
    }
}

/// <summary>
/// Represents the current market trend for a specific category.
/// </summary>
[System.Serializable]
public class MarketTrend
{
    public string Category;
    public TrendDirection Direction;
    public float ChangePercent;
    public float Volatility;
    public double TrendStrength;
    public int Duration;

    public MarketTrend(string category)
    {
        Category = category;
        Direction = TrendDirection.Stable;
        ChangePercent = 0;
        Volatility = 0;
        TrendStrength = 0;
        Duration = 0;
    }
}

/// <summary>
/// Represents a market prediction for a specific category.
/// </summary>
[System.Serializable]
public class MarketPrediction
{
    public string Category;
    public TrendDirection PredictedDirection;
    public float PredictedChange;
    public double PredictionTime;
    public bool IsCorrect;
    public double Confidence;

    public MarketPrediction(string category, TrendDirection direction, float change, double confidence)
    {
        Category = category;
        PredictedDirection = direction;
        PredictedChange = change;
        PredictionTime = Time.GetUnixTimeFromSystem();
        IsCorrect = false;
        Confidence = confidence;
    }
}

/// <summary>
/// Defines the possible directions for market trends.
/// </summary>
public enum TrendDirection
{
    Rising,
    Falling,
    Stable,
    Volatile
}
