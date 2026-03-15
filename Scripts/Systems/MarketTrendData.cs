using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 市场趋势数据 - 记录物品价格历史和统计
/// 用于市场分析和价格预测
/// </summary>
public class MarketTrendData : Node
{
    // Price history for each item category
    public Dictionary<string, List<PriceRecord>> PriceHistory = new Dictionary<string, List<PriceRecord>>();
    
    // Current market trends
    public Dictionary<string, MarketTrend> CurrentTrends = new Dictionary<string, MarketTrend>();
    
    // Market predictions
    public Dictionary<string, MarketPrediction> Predictions = new Dictionary<string, MarketPrediction>();
    
    // Global market sentiment (-100 to 100)
    public int MarketSentiment = 0;
    
    // Last update time
    public double LastUpdateTime = 0;
    
    // Statistics
    public int TotalTrendUpdates = 0;
    public int TotalPredictionsMade = 0;
    public int CorrectPredictions = 0;
    
    public override void _Ready()
    {
        LastUpdateTime = Time.GetUnixTimeFromSystem();
    }
}

[System.Serializable]
public class PriceRecord
{
    public double Timestamp;
    public float Price;
    public int Volume; // Trading volume
    
    public PriceRecord(double timestamp, float price, int volume)
    {
        Timestamp = timestamp;
        Price = price;
        Volume = volume;
    }
}

[System.Serializable]
public class MarketTrend
{
    public string Category;
    public TrendDirection Direction; // Rising/Falling/Stable/Volatile
    public float ChangePercent; // Percentage change
    public float Volatility; // How stable the trend is
    public double TrendStrength; // 0-100
    public int Duration; // How long the trend has been active
    
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

[System.Serializable]
public class MarketPrediction
{
    public string Category;
    public TrendDirection PredictedDirection;
    public float PredictedChange;
    public double PredictionTime;
    public bool IsCorrect;
    public double Confidence; // 0-100
    
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

public enum TrendDirection
{
    Rising,
    Falling,
    Stable,
    Volatile
}
