using Godot;
using System;
using System.Collections.Generic;

public class TradeRouteData
{
    // 贸易路线数据结构
    public class TradeRoute
    {
        public string RouteId { get; set; }
        public string RouteName { get; set; }
        public string StartCity { get; set; }
        public string EndCity { get; set; }
        public int Distance { get; set; } // 距离（影响时间）
        public int LevelRequirement { get; set; }
        public int InvestmentCost { get; set; } // 投资成本
        public int CurrentInvestment { get; set; }
        public bool IsActive { get; set; }
        public int TotalTrades { get; set; }
        public int SuccessfulTrades { get; set; }
        public int TotalProfit { get; set; }
        public int Level { get; set; }
        public DateTime LastTradeTime { get; set; }
    }

    // 贸易商品
    public class TradeGoods
    {
        public string GoodsId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; } // Material/Food/Weapon/Armor/Magic/Art
        public int BasePrice { get; set; }
        public int Volatility { get; set; } // 价格波动 1-10
        public string SourceRegion { get; set; }
        public string TargetRegion { get; set; }
    }

    // 贸易记录
    public class TradeRecord
    {
        public string RecordId { get; set; }
        public string RouteName { get; set; }
        public string GoodsName { get; set; }
        public int BuyPrice { get; set; }
        public int SellPrice { get; set; }
        public int Quantity { get; set; }
        public int Profit { get; set; }
        public DateTime TradeTime { get; set; }
    }

    // 当前市场数据
    public class MarketData
    {
        public string GoodsId { get; set; }
        public int CurrentPrice { get; set; }
        public int PreviousPrice { get; set; }
        public int Trend { get; set; } // -1 down, 0 stable, 1 up
    }

    public Dictionary<string, TradeRoute> ActiveRoutes { get; set; } = new Dictionary<string, TradeRoute>();
    public Dictionary<string, TradeGoods> AvailableGoods { get; set; } = new Dictionary<string, TradeGoods>();
    public List<TradeRecord> TradeHistory { get; set; } = new List<TradeRecord>();
    public Dictionary<string, MarketData> CurrentMarket { get; set; } = new Dictionary<string, MarketData>();
    public Dictionary<string, int> UnlockedRoutes { get; set; } = new Dictionary<string, int>();
}
