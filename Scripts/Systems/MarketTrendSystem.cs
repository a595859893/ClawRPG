using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// Market trend system that dynamically manages item price trends and predictions.
/// Updates market data periodically, calculates trend directions, and generates market sentiment.
/// </summary>
public class MarketTrendSystem : BaseSystem
{
    /// <summary>
    /// Gets the singleton instance of the MarketTrendSystem.
    /// </summary>
    /// <value>The global instance for market trend operations.</value>
    public static MarketTrendSystem Instance { get; private set; }
    
    private MarketTrendData _data;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    
    // Update interval in seconds (5 minutes)
    private const float UPDATE_INTERVAL = 300f;
    private float _timer = 0f;
    
    // Maximum history records to keep per category
    private const int MAX_HISTORY = 100;
    
    public override void _Ready()
    {
        Instance = this;
        _rng.Randomize();
        
        // Get or create data
        _data = GetNode<MarketTrendData>("/root/MarketTrendData");
        if (_data == null)
        {
            _data = new MarketTrendData();
            _data.Name = "MarketTrendData";
            GetTree().Root.AddChild(_data);
        }
        
        // Initialize categories if empty
        if (_data.PriceHistory.Count == 0)
        {
            InitializeMarket();
        }
        
        GD.Print("[MarketTrendSystem] Initialized with " + _data.PriceHistory.Count + " categories");
    }
    
    private void InitializeMarket()
    {
        foreach (string category in MarketTrendDatabase.ItemCategories)
        {
            // Initialize price history with some data
            List<PriceRecord> history = new List<PriceRecord>();
            float basePrice = MarketTrendDatabase.GetBasePrice(category);
            
            // Generate 30 days of historical data
            for (int i = 30; i >= 0; i--)
            {
                double timestamp = Time.GetUnixTimeFromSystem() - (i * 86400);
                float variance = (float)_rng.RandRange(0.9, 1.1);
                int volume = _rng.RandIntRange(100, 1000);
                history.Add(new PriceRecord(timestamp, basePrice * variance, volume));
            }
            
            _data.PriceHistory[category] = history;
            
            // Initialize trend
            MarketTrend trend = new MarketTrend(category);
            trend.Direction = TrendDirection.Stable;
            trend.ChangePercent = 0;
            _data.CurrentTrends[category] = trend;
            
            // Initialize prediction
            _data.Predictions[category] = new MarketPrediction(category, TrendDirection.Stable, 0, 50);
        }
        
        _data.LastUpdateTime = Time.GetUnixTimeFromSystem();
    }
    
    public override void _Process(float delta)
    {
        _timer += delta;
        
        if (_timer >= UPDATE_INTERVAL)
        {
            _timer = 0f;
            UpdateMarket();
        }
    }
    
    private void UpdateMarket()
    {
        _data.TotalTrendUpdates++;
        
        foreach (string category in MarketTrendDatabase.ItemCategories)
        {
            UpdateCategoryTrend(category);
        }
        
        // Update market sentiment
        UpdateMarketSentiment();
        
        // Make predictions
        MakePredictions();
        
        _data.LastUpdateTime = Time.GetUnixTimeFromSystem();
        
        GD.Print("[MarketTrendSystem] Market updated - " + _data.TotalTrendUpdates + " updates");
    }
    
    private void UpdateCategoryTrend(string category)
    {
        if (!_data.PriceHistory.ContainsKey(category))
            return;
        
        List<PriceRecord> history = _data.PriceHistory[category];
        if (history.Count < 2)
            return;
        
        // Get recent prices
        float recentAvg = history.TakeLast(7).Average(p => p.Price);
        float olderAvg = history.Skip(Math.Max(0, history.Count - 14)).Take(7).Average(p => p.Price);
        
        // Calculate change percentage
        float changePercent = ((recentAvg - olderAvg) / olderAvg) * 100f;
        
        // Calculate volatility
        float volatility = CalculateVolatility(history.TakeLast(14).ToList());
        
        // Determine trend direction
        TrendDirection direction;
        if (Math.Abs(changePercent) < 2f)
        {
            direction = TrendDirection.Stable;
        }
        else if (volatility > MarketTrendDatabase.GetVolatility(category) * 1.5f)
        {
            direction = TrendDirection.Volatile;
        }
        else if (changePercent > 0)
        {
            direction = TrendDirection.Rising;
        }
        else
        {
            direction = TrendDirection.Falling;
        }
        
        // Update trend
        MarketTrend trend = _data.CurrentTrends[category];
        trend.Direction = direction;
        trend.ChangePercent = changePercent;
        trend.Volatility = volatility;
        trend.TrendStrength = Math.Min(100, Math.Abs(changePercent) * 5);
        trend.Duration++;
        
        // Add new price record
        float newPrice = recentAvg * (1 + MarketTrendDatabase.CalculatePriceChange(category, direction, trend.TrendStrength));
        int volume = _rng.RandIntRange(100, 1000);
        
        history.Add(new PriceRecord(Time.GetUnixTimeFromSystem(), newPrice, volume));
        
        // Trim history
        if (history.Count > MAX_HISTORY)
        {
            history.RemoveAt(0);
        }
    }
    
    private float CalculateVolatility(List<PriceRecord> records)
    {
        if (records.Count < 2)
            return 0;
        
        float avg = records.Average(r => r.Price);
        float sumSquares = records.Sum(r => (r.Price - avg) * (r.Price - avg));
        float stdDev = (float)Math.Sqrt(sumSquares / records.Count);
        
        return (avg > 0) ? stdDev / avg : 0;
    }
    
    private void UpdateMarketSentiment()
    {
        // Calculate based on overall trend directions
        int rising = 0;
        int falling = 0;
        
        foreach (var trend in _data.CurrentTrends.Values)
        {
            if (trend.Direction == TrendDirection.Rising)
                rising++;
            else if (trend.Direction == TrendDirection.Falling)
                falling++;
        }
        
        // Update sentiment
        int total = _data.CurrentTrends.Count;
        if (total > 0)
        {
            int newSentiment = ((rising - falling) * 100) / total;
            _data.MarketSentiment = (int)(_data.MarketSentiment * 0.7 + newSentiment * 0.3);
            _data.MarketSentiment = Mathf.Clamp(_data.MarketSentiment, -100, 100);
        }
    }
    
    private void MakePredictions()
    {
        foreach (string category in MarketTrendDatabase.ItemCategories)
        {
            if (!_data.CurrentTrends.ContainsKey(category))
                continue;
            
            MarketTrend trend = _data.CurrentTrends[category];
            
            // Make prediction based on current trend
            TrendDirection predictedDir = trend.Direction;
            float predictedChange = trend.ChangePercent;
            double confidence = Math.Min(95, trend.TrendStrength + 50);
            
            // Add some randomness
            if (_rng.RandFloat() < 0.2f)
            {
                predictedDir = (TrendDirection)_rng.Randi() % 4;
                predictedChange = (float)_rng.RandRange(-10, 10);
                confidence *= 0.7;
            }
            
            _data.Predictions[category] = new MarketPrediction(category, predictedDir, predictedChange, confidence);
            _data.TotalPredictionsMade++;
        }
    }
    
    // Public API
    
    /// <summary>
    /// Gets the current market trend for a specific category.
    /// </summary>
    /// <param name="category">The item category to look up.</param>
    /// <returns>The market trend for the category, or null if not found.</returns>
    public MarketTrend GetTrend(string category)
    {
        if (_data.CurrentTrends.ContainsKey(category))
            return _data.CurrentTrends[category];
        return null;
    }
    
    /// <summary>
    /// Gets the market prediction for a specific category.
    /// </summary>
    /// <param name="category">The item category to look up.</param>
    /// <returns>The market prediction for the category, or null if not found.</returns>
    public MarketPrediction GetPrediction(string category)
    {
        if (_data.Predictions.ContainsKey(category))
            return _data.Predictions[category];
        return null;
    }
    
    /// <summary>
    /// Gets the price history for a specific category.
    /// </summary>
    /// <param name="category">The item category to look up.</param>
    /// <param name="count">Maximum number of records to return (default 30).</param>
    /// <returns>List of price records ordered by timestamp.</returns>
    public List<PriceRecord> GetPriceHistory(string category, int count = 30)
    {
        if (!_data.PriceHistory.ContainsKey(category))
            return new List<PriceRecord>();
        
        return _data.PriceHistory[category].TakeLast(count).ToList();
    }
    
    /// <summary>
    /// Gets the current market sentiment value.
    /// </summary>
    /// <returns>Market sentiment ranging from -100 to 100.</returns>
    public int GetMarketSentiment()
    {
        return _data.MarketSentiment;
    }
    
    /// <summary>
    /// Gets a text description of the current market sentiment.
    /// </summary>
    /// <returns>Localized text describing market sentiment.</returns>
    public string GetSentimentText()
    {
        if (_data.MarketSentiment > 50)
            return "非常乐观";
        else if (_data.MarketSentiment > 20)
            return "乐观";
        else if (_data.MarketSentiment > -20)
            return "中性";
        else if (_data.MarketSentiment > -50)
            return "悲观";
        else
            return "极度悲观";
    }
    
    /// <summary>
    /// Gets all current market trends.
    /// </summary>
    /// <returns>Dictionary of all category trends.</returns>
    public Dictionary<string, MarketTrend> GetAllTrends()
    {
        return new Dictionary<string, MarketTrend>(_data.CurrentTrends);
    }
    
    /// <summary>
    /// Gets the hottest categories based on trend strength.
    /// </summary>
    /// <param name="count">Number of categories to return (default 3).</param>
    /// <returns>List of category names with the strongest trends.</returns>
    public List<string> GetHotCategories(int count = 3)
    {
        return _data.CurrentTrends
            .OrderByDescending(t => t.Value.TrendStrength)
            .Take(count)
            .Select(t => t.Key)
            .ToList();
    }
    
    /// <summary>
    /// Gets potential investment opportunities (categories with low prices likely to rise).
    /// </summary>
    /// <param name="count">Number of opportunities to return (default 3).</param>
    /// <returns>List of category names that may be good investments.</returns>
    public List<string> GetInvestmentOpportunities(int count = 3)
    {
        // Categories with falling prices that are likely to rise
        return _data.CurrentTrends
            .Where(t => t.Value.Direction == TrendDirection.Falling || t.Value.Direction == TrendDirection.Stable)
            .OrderBy(t => t.Value.ChangePercent)
            .Take(count)
            .Select(t => t.Key)
            .ToList();
    }
    
    /// <summary>
    /// Gets market statistics including prediction accuracy.
    /// </summary>
    /// <returns>Dictionary containing various market statistics.</returns>
    public Dictionary<string, float> GetStatistics()
    {
        return new Dictionary<string, float>
        {
            { "TotalUpdates", _data.TotalTrendUpdates },
            { "TotalPredictions", _data.TotalPredictionsMade },
            { "CorrectPredictions", _data.CorrectPredictions },
            { "PredictionAccuracy", _data.TotalPredictionsMade > 0 ? (_data.CorrectPredictions * 100f / _data.TotalPredictionsMade) : 0 },
            { "MarketSentiment", _data.MarketSentiment }
        };
    }
    
    // Save/Load support
    
    /// <summary>
    /// Saves market trend data for persistence.
    /// </summary>
    /// <returns>Dictionary containing serialized market data.</returns>
    public Dictionary<string, object> SaveData()
    {
        return new Dictionary<string, object>
        {
            { "MarketSentiment", _data.MarketSentiment },
            { "TotalTrendUpdates", _data.TotalTrendUpdates },
            { "TotalPredictionsMade", _data.TotalPredictionsMade },
            { "CorrectPredictions", _data.CorrectPredictions },
            { "LastUpdateTime", _data.LastUpdateTime }
        };
    }
    
    /// <summary>
    /// Loads market trend data from persistence.
    /// </summary>
    /// <param name="data">Dictionary containing serialized market data.</param>
    public void LoadData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("MarketSentiment"))
            _data.MarketSentiment = Convert.ToInt32(data["MarketSentiment"]);
        if (data.ContainsKey("TotalTrendUpdates"))
            _data.TotalTrendUpdates = Convert.ToInt32(data["TotalTrendUpdates"]);
        if (data.ContainsKey("TotalPredictionsMade"))
            _data.TotalPredictionsMade = Convert.ToInt32(data["TotalPredictionsMade"]);
        if (data.ContainsKey("CorrectPredictions"))
            _data.CorrectPredictions = Convert.ToInt32(data["CorrectPredictions"]);
        if (data.ContainsKey("LastUpdateTime"))
            _data.LastUpdateTime = Convert.ToDouble(data["LastUpdateTime"]);
    }
    
    #region Data Persistence
    
    public override Dictionary ExportSaveData()
    {
        return new Dictionary(SaveData());
    }
    
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        LoadData(new Dictionary<string, object>(data));
    }
    
    #endregion
}
