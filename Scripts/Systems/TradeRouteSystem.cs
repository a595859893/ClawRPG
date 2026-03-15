using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 贸易路线系统 - 管理世界贸易和商人路线
/// </summary>
public partial class TradeRouteSystem : BaseSystem
{
    private static TradeRouteSystem _instance;
    public static TradeRouteSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new TradeRouteSystem();
            return _instance;
        }
    }

    private TradeRouteData _data;
    private Random _random = new Random();
    private int _tradeCooldown = 30; // seconds between trades

    // Events
    public delegate void RouteUnlockedHandler(string routeId);
    public event RouteUnlockedHandler OnRouteUnlocked;

    public delegate void TradeCompletedHandler(TradeRouteData.TradeRecord record);
    public event TradeCompletedHandler OnTradeCompleted;

    public delegate void MarketUpdateHandler();
    public event MarketUpdateHandler OnMarketUpdate;

    private TradeRouteSystem()
    {
        _data = new TradeRouteData();
        InitializeDatabase();
    }

    public override void _Ready()
    {
        base._Ready();
        Initialize();
    }

    protected override void Initialize()
    {
        IsInitialized = true;
        GD.Print("[TradeRouteSystem] initialized");
    }

    private void InitializeDatabase()
    {
        // Initialize available goods
        foreach (var goods in TradeRouteDatabase.Instance.GoodsTemplates)
        {
            _data.AvailableGoods[goods.GoodsId] = new TradeRouteData.TradeGoods
            {
                GoodsId = goods.GoodsId,
                Name = goods.Name,
                Category = goods.Category,
                BasePrice = goods.BasePrice,
                Volatility = goods.Volatility,
                SourceRegion = goods.SourceRegions[0]
            };
        }

        // Initialize market prices
        RefreshMarket();
    }

    // Route Management
    public void UnlockRoute(string routeId)
    {
        if (_data.UnlockedRoutes.ContainsKey(routeId)) return;

        var template = GetRouteTemplate(routeId);
        if (template == null) return;

        _data.UnlockedRoutes[routeId] = 0;
        OnRouteUnlocked?.Invoke(routeId);
    }

    public bool CanInvest(string routeId)
    {
        if (!_data.UnlockedRoutes.ContainsKey(routeId)) return false;
        
        var template = GetRouteTemplate(routeId);
        if (template == null) return false;

        var route = GetOrCreateRoute(routeId);
        return route.CurrentInvestment < template.BaseInvestment * 10; // Max 10x investment
    }

    public bool InvestInRoute(string routeId, int amount)
    {
        if (!CanInvest(routeId)) return false;

        var route = GetOrCreateRoute(routeId);
        var template = GetRouteTemplate(routeId);
        
        if (Player.Instance.Gold < amount) return false;

        Player.Instance.Gold -= amount;
        route.CurrentInvestment += amount;

        // Level up based on investment
        int newLevel = (route.CurrentInvestment / template.BaseInvestment) + 1;
        if (newLevel > route.Level)
        {
            route.Level = newLevel;
        }

        _data.UnlockedRoutes[routeId] = route.Level;
        return true;
    }

    public TradeRouteData.TradeRoute GetOrCreateRoute(string routeId)
    {
        if (_data.ActiveRoutes.ContainsKey(routeId))
        {
            return _data.ActiveRoutes[routeId];
        }

        var template = GetRouteTemplate(routeId);
        if (template == null) return null;

        var route = new TradeRouteData.TradeRoute
        {
            RouteId = routeId,
            RouteName = template.RouteName,
            StartCity = template.StartCity,
            EndCity = template.EndCity,
            Distance = template.BaseDistance,
            LevelRequirement = template.MinLevel,
            InvestmentCost = template.BaseInvestment,
            CurrentInvestment = 0,
            IsActive = false,
            TotalTrades = 0,
            SuccessfulTrades = 0,
            TotalProfit = 0,
            Level = 1,
            LastTradeTime = DateTime.MinValue
        };

        _data.ActiveRoutes[routeId] = route;
        return route;
    }

    public void ActivateRoute(string routeId)
    {
        var route = GetOrCreateRoute(routeId);
        if (route == null) return;

        if (Player.Instance.Level < route.LevelRequirement)
        {
            GD.Print($"Player level {Player.Instance.Level} does not meet requirement {route.LevelRequirement}");
            return;
        }

        if (route.CurrentInvestment < route.InvestmentCost)
        {
            GD.Print($"Insufficient investment. Required: {route.InvestmentCost}, Current: {route.CurrentInvestment}");
            return;
        }

        route.IsActive = true;
    }

    public void DeactivateRoute(string routeId)
    {
        if (_data.ActiveRoutes.ContainsKey(routeId))
        {
            _data.ActiveRoutes[routeId].IsActive = false;
        }
    }

    // Trading
    public TradeRouteData.TradeRecord ExecuteTrade(string routeId)
    {
        var route = GetOrCreateRoute(routeId);
        if (route == null || !route.IsActive)
        {
            GD.Print("Route is not active");
            return null;
        }

        // Check cooldown
        var timeSinceLastTrade = (DateTime.Now - route.LastTradeTime).TotalSeconds;
        if (timeSinceLastTrade < _tradeCooldown)
        {
            GD.Print($"Trade on cooldown. Wait {(_tradeCooldown - timeSinceLastTrade):F0} more seconds");
            return null;
        }

        var startCity = TradeRouteDatabase.Instance.GetCity(route.StartCity);
        var endCity = TradeRouteDatabase.Instance.GetCity(route.EndCity);

        // Select goods based on source region
        var availableGoods = TradeRouteDatabase.Instance.GetGoodsByRegion(
            TradeRouteDatabase.Instance.GetCity(route.StartCity).Region);

        if (availableGoods.Count == 0) return null;

        var goods = availableGoods[_random.Next(availableGoods.Count)];

        // Calculate prices
        int buyPrice = GetMarketPrice(goods.GoodsId, startCity.Region);
        int sellPrice = GetMarketPrice(goods.GoodsId, endCity.Region);

        // Investment bonus
        float investmentBonus = 1.0f + (route.CurrentInvestment / (float)(route.InvestmentCost * 10));
        int quantity = route.Level * (1 + _random.Next(3)); // 1-3 based on level
        int levelBonus = route.Level * 50;

        int totalCost = buyPrice * quantity;
        int totalRevenue = sellPrice * quantity;
        int profit = (int)((totalRevenue - totalCost) * investmentBonus) + levelBonus;

        // Apply route bonus
        profit = (int)(profit * (1.0f + route.Level * 0.1f));

        // Create record
        var record = new TradeRouteData.TradeRecord
        {
            RecordId = Guid.NewGuid().ToString(),
            RouteName = route.RouteName,
            GoodsName = goods.Name,
            BuyPrice = buyPrice,
            SellPrice = sellPrice,
            Quantity = quantity,
            Profit = profit,
            TradeTime = DateTime.Now
        };

        // Apply results
        Player.Instance.Gold += profit;
        route.TotalTrades++;
        route.SuccessfulTrades++;
        route.TotalProfit += profit;
        route.LastTradeTime = DateTime.Now;

        _data.TradeHistory.Insert(0, record);
        if (_data.TradeHistory.Count > 100) _data.TradeHistory.RemoveAt(_data.TradeHistory.Count - 1);

        OnTradeCompleted?.Invoke(record);
        return record;
    }

    // Market System
    public void RefreshMarket()
    {
        foreach (var goods in TradeRouteDatabase.Instance.GoodsTemplates)
        {
            if (_data.CurrentMarket.ContainsKey(goods.GoodsId))
            {
                var prevPrice = _data.CurrentMarket[goods.GoodsId].CurrentPrice;
                int newPrice = CalculateDynamicPrice(goods);
                int trend = newPrice > prevPrice ? 1 : (newPrice < prevPrice ? -1 : 0);

                _data.CurrentMarket[goods.GoodsId] = new TradeRouteData.MarketData
                {
                    GoodsId = goods.GoodsId,
                    CurrentPrice = newPrice,
                    PreviousPrice = prevPrice,
                    Trend = trend
                };
            }
            else
            {
                int price = CalculateDynamicPrice(goods);
                _data.CurrentMarket[goods.GoodsId] = new TradeRouteData.MarketData
                {
                    GoodsId = goods.GoodsId,
                    CurrentPrice = price,
                    PreviousPrice = price,
                    Trend = 0
                };
            }
        }

        OnMarketUpdate?.Invoke();
    }

    private int CalculateDynamicPrice(TradeRouteDatabase.GoodsTemplate template)
    {
        // Base price with volatility
        float volatility = template.Volatility / 10.0f;
        float randomFactor = 1.0f + ((float)_random.NextDouble() * 2 - 1) * volatility;
        int basePrice = (int)(template.BasePrice * randomFactor);

        // Ensure minimum price
        return Math.Max(10, basePrice);
    }

    public int GetMarketPrice(string goodsId, string region)
    {
        if (_data.CurrentMarket.ContainsKey(goodsId))
        {
            return _data.CurrentMarket[goodsId].CurrentPrice;
        }
        return TradeRouteDatabase.Instance.GetGoods(goodsId)?.BasePrice ?? 100;
    }

    // Getters
    public TradeRouteData GetData() => _data;

    public Dictionary<string, TradeRouteData.TradeRoute> GetActiveRoutes() => _data.ActiveRoutes;

    public List<TradeRouteData.TradeRecord> GetTradeHistory() => _data.TradeHistory;

    public Dictionary<string, TradeRouteData.MarketData> GetMarketData() => _data.CurrentMarket;

    public Dictionary<string, int> GetUnlockedRoutes() => _data.UnlockedRoutes;

    public List<TradeRouteDatabase.RouteTemplate> GetAvailableRouteTemplates()
    {
        return TradeRouteDatabase.Instance.RouteTemplates;
    }

    private TradeRouteDatabase.RouteTemplate GetRouteTemplate(string routeId)
    {
        foreach (var template in TradeRouteDatabase.Instance.RouteTemplates)
        {
            if (template.RouteId == routeId) return template;
        }
        return null;
    }

    public int GetTotalProfit()
    {
        int total = 0;
        foreach (var route in _data.ActiveRoutes.Values)
        {
            total += route.TotalProfit;
        }
        return total;
    }

    public int GetTotalTrades()
    {
        int total = 0;
        foreach (var route in _data.ActiveRoutes.Values)
        {
            total += route.TotalTrades;
        }
        return total;
    }

    public float GetSuccessRate()
    {
        int total = GetTotalTrades();
        if (total == 0) return 0;

        int success = 0;
        foreach (var route in _data.ActiveRoutes.Values)
        {
            success += route.SuccessfulTrades;
        }
        return (float)success / total;
    }

    // Save/Load
    protected override Dictionary ExportSaveData()
    {
        var saveData = new Dictionary();
        
        var routes = new Godot.Collections.Array();
        foreach (var route in _data.ActiveRoutes.Values)
        {
            routes.Add(new Godot.Collections.Dictionary
            {
                { "route_id", route.RouteId },
                { "current_investment", route.CurrentInvestment },
                { "is_active", route.IsActive },
                { "total_trades", route.TotalTrades },
                { "successful_trades", route.SuccessfulTrades },
                { "total_profit", route.TotalProfit },
                { "level", route.Level }
            });
        }
        saveData["active_routes"] = routes;
        
        var unlockedRoutes = new Godot.Collections.Dictionary();
        foreach (var kvp in _data.UnlockedRoutes)
            unlockedRoutes[kvp.Key] = kvp.Value;
        saveData["unlocked_routes"] = unlockedRoutes;
        
        return saveData;
    }

    protected override void ImportSaveData(Dictionary saveData)
    {
        if (saveData == null) return;

        if (saveData.ContainsKey("unlocked_routes"))
        {
            var unlocked = saveData["unlocked_routes"] as Godot.Collections.Dictionary;
            _data.UnlockedRoutes.Clear();
            if (unlocked != null)
            {
                foreach (var kvp in unlocked)
                {
                    _data.UnlockedRoutes[kvp.Key.ToString()] = Convert.ToInt32(kvp.Value);
                }
            }
        }

        if (saveData.ContainsKey("active_routes"))
        {
            var routes = saveData["active_routes"] as Godot.Collections.Array;
            if (routes != null)
            {
                foreach (var routeData in routes)
                {
                    var rd = routeData as Godot.Collections.Dictionary;
                    if (rd == null) continue;
                    
                    var routeId = rd["route_id"].ToString();
                    var route = GetOrCreateRoute(routeId);
                    if (route != null)
                    {
                        route.CurrentInvestment = Convert.ToInt32(rd["current_investment"]);
                        route.IsActive = Convert.ToBoolean(rd["is_active"]);
                        route.TotalTrades = Convert.ToInt32(rd["total_trades"]);
                        route.SuccessfulTrades = Convert.ToInt32(rd["successful_trades"]);
                        route.TotalProfit = Convert.ToInt32(rd["total_profit"]);
                        route.Level = Convert.ToInt32(rd["level"]);
                    }
                }
            }
        }
        
        GD.Print("[TradeRouteSystem] Save data imported");
    }
}
