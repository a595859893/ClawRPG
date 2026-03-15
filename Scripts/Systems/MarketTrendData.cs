using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Market trend data container that tracks item price history and statistics.
/// Used for market analysis and price prediction systems.
/// </summary>
public class MarketTrendData : BaseSystem
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
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}

/// <summary>
/// Represents a single price record at a specific point in time.
/// </summary>
[System.Serializable]
public class PriceRecord
{
    /// <summary>
    /// Unix timestamp when this price was recorded.
    /// </summary>
    public double Timestamp;
    
    /// <summary>
    /// The recorded price value.
    /// </summary>
    public float Price;
    
    /// <summary>
    /// Trading volume at this price point.
    /// </summary>
    public int Volume;
    
    /// <summary>
    /// Creates a new price record with the specified values.
    /// </summary>
    /// <param name="timestamp">Unix timestamp of the record.</param>
    /// <param name="price">The price value.</param>
    /// <param name="volume">Trading volume at this price.</param>
    public PriceRecord(double timestamp, float price, int volume)
    {
        Timestamp = timestamp;
        Price = price;
        Volume = volume;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}

/// <summary>
/// Represents the current market trend for a specific category.
/// </summary>
[System.Serializable]
public class MarketTrend
{
    /// <summary>
    /// The item category this trend represents.
    /// </summary>
    public string Category;
    
    /// <summary>
    /// Current direction of the trend (Rising, Falling, Stable, or Volatile).
    /// </summary>
    public TrendDirection Direction;
    
    /// <summary>
    /// Percentage change in price over the tracking period.
    /// </summary>
    public float ChangePercent;
    
    /// <summary>
    /// Volatility indicator showing how unstable the price is.
    /// </summary>
    public float Volatility;
    
    /// <summary>
    /// Strength of the trend from 0 to 100.
    /// </summary>
    public double TrendStrength;
    
    /// <summary>
    /// Duration in update cycles that this trend has been active.
    /// </summary>
    public int Duration;
    
    /// <summary>
    /// Creates a new market trend for the specified category.
    /// </summary>
    /// <param name="category">The item category to track.</param>
    public MarketTrend(string category)
    {
        Category = category;
        Direction = TrendDirection.Stable;
        ChangePercent = 0;
        Volatility = 0;
        TrendStrength = 0;
        Duration = 0;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}

/// <summary>
/// Represents a market prediction for a specific category.
/// </summary>
[System.Serializable]
public class MarketPrediction
{
    /// <summary>
    /// The item category this prediction is for.
    /// </summary>
    public string Category;
    
    /// <summary>
    /// Predicted direction of price movement.
    /// </summary>
    public TrendDirection PredictedDirection;
    
    /// <summary>
    /// Predicted percentage change in price.
    /// </summary>
    public float PredictedChange;
    
    /// <summary>
    /// Unix timestamp when this prediction was made.
    /// </summary>
    public double PredictionTime;
    
    /// <summary>
    /// Whether the prediction was correct (updated after the prediction period).
    /// </summary>
    public bool IsCorrect;
    
    /// <summary>
    /// Confidence level of the prediction from 0 to 100.
    /// </summary>
    public double Confidence;
    
    /// <summary>
    /// Creates a new market prediction.
    /// </summary>
    /// <param name="category">The item category being predicted.</param>
    /// <param name="direction">Predicted trend direction.</param>
    /// <param name="change">Predicted percentage change.</param>
    /// <param name="confidence">Confidence level from 0 to 100.</param>
    public MarketPrediction(string category, TrendDirection direction, float change, double confidence)
    {
        Category = category;
        PredictedDirection = direction;
        PredictedChange = change;
        PredictionTime = Time.GetUnixTimeFromSystem();
        IsCorrect = false;
        Confidence = confidence;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}

/// <summary>
/// Defines the possible directions for market trends.
/// </summary>
public enum TrendDirection
{
    /// <summary>
    /// Price is trending upward.
    /// </summary>
    Rising,
    
    /// <summary>
    /// Price is trending downward.
    /// </summary>
    Falling,
    
    /// <summary>
    /// Price is relatively stable with minimal change.
    /// </summary>
    Stable,
    
    /// <summary>
    /// Price is highly volatile with significant fluctuations.
    /// </summary>
    Volatile

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        // No persistent data needed
    }
}
