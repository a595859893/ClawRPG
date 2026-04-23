using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 拍卖品数据 - 定义拍卖物品的数据结构
/// </summary>
public class AuctionRecord
{
    public int Id { get; set; }
    public string SellerName { get; set; }
    public int ItemId { get; set; }
    public string ItemName { get; set; }
    public int ItemRarity { get; set; }
    public int Quantity { get; set; }
    public int CurrentBid { get; set; }
    public int BuyNowPrice { get; set; }
    public string HighestBidder { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool IsActive { get; set; }
    public AuctionListingStatus Status { get; set; }
}

/// <summary>
/// 拍卖品状态枚举
/// </summary>
public enum AuctionListingStatus
{
    Active,     // 活跃
    Sold,       // 已售出
    Expired,    // 已过期
    Cancelled   // 已取消
}

/// <summary>
/// 玩家拍卖数据
/// </summary>
public class PlayerAuctionData
{
    public string PlayerName { get; set; }
    public int TotalSales { get; set; }         // 总售出次数
    public int TotalPurchases { get; set; }      // 总购买次数
    public int TotalSpent { get; set; }         // 总花费
    public int TotalEarned { get; set; }         // 总收入
    public List<int> ActiveListings { get; set; } = new List<int>();  // 活跃挂单
    public List<int> WonAuctions { get; set; } = new List<int>();     // 赢得的拍卖
}

/// <summary>
/// 拍卖数据库 - 管理所有拍卖品和玩家数据
/// </summary>
public static class AuctionDatabase
{
    private static readonly Dictionary<int, AuctionRecord> _auctions = new Dictionary<int, AuctionRecord>();
    private static int _nextAuctionId = 1;
    private static readonly Dictionary<string, PlayerAuctionData> _playerData = new Dictionary<string, PlayerAuctionData>();
    
    // 拍卖持续时间（小时）
    public const int AUCTION_DURATION_HOURS = 24;
    
    // 费用百分比
    public const float LISTING_FEE_PERCENT = 0.02f; // 2% 挂单费
    public const float SALE_FEE_PERCENT = 0.05f;    // 5% 销售费
    
    // 每个玩家最大挂单数
    public const int MAX_ACTIVE_LISTINGS = 10;
    
    // 价格范围
    public const int MIN_AUCTION_PRICE = 10;
    public const int MAX_AUCTION_PRICE = 999999999;
    
    static AuctionDatabase()
    {
        // 初始化示例拍卖品
        InitializeSampleAuctions();
    }
    
    /// <summary>
    /// 初始化示例拍卖品数据
    /// </summary>
    private static void InitializeSampleAuctions()
    {
        // 示例拍卖物品
        var sampleAuctions = new[]
        {
            new AuctionRecord
            {
                Id = _nextAuctionId++,
                SellerName = "Merchant_Alice",
                ItemId = 1001,
                ItemName = " legendary Sword",
                ItemRarity = 4,
                Quantity = 1,
                CurrentBid = 5000,
                BuyNowPrice = 15000,
                HighestBidder = "",
                StartTime = DateTime.UtcNow.AddHours(-12),
                EndTime = DateTime.UtcNow.AddHours(12),
                IsActive = true,
                Status = AuctionListingStatus.Active
            },
            new AuctionRecord
            {
                Id = _nextAuctionId++,
                SellerName = "Merchant_Bob",
                ItemId = 2001,
                ItemName = "Rare Armor",
                ItemRarity = 3,
                Quantity = 1,
                CurrentBid = 2000,
                BuyNowPrice = 8000,
                HighestBidder = "",
                StartTime = DateTime.UtcNow.AddHours(-6),
                EndTime = DateTime.UtcNow.AddHours(18),
                IsActive = true,
                Status = AuctionListingStatus.Active
            },
            new AuctionRecord
            {
                Id = _nextAuctionId++,
                SellerName = "Merchant_Clara",
                ItemId = 3001,
                ItemName = "Health Potion x10",
                ItemRarity = 1,
                Quantity = 10,
                CurrentBid = 100,
                BuyNowPrice = 500,
                HighestBidder = "",
                StartTime = DateTime.UtcNow.AddHours(-2),
                EndTime = DateTime.UtcNow.AddHours(22),
                IsActive = true,
                Status = AuctionListingStatus.Active
            }
        };
        
        foreach (var auction in sampleAuctions)
        {
            _auctions[auction.Id] = auction;
        }
    }
    
    /// <summary>
    /// 获取下一个拍卖ID
    /// </summary>
    public static int GetNextAuctionId() => _nextAuctionId++;
    
    /// <summary>
    /// 添加拍卖品
    /// </summary>
    public static void AddAuction( AuctionRecord auction)
    {
        _auctions[auction.Id] = auction;
    }
    
    /// <summary>
    /// 获取指定ID的拍卖品
    /// </summary>
    public static  AuctionRecord GetAuction(int id)
    {
        return _auctions.ContainsKey(id) ? _auctions[id] : null;
    }
    
    /// <summary>
    /// 获取所有拍卖品
    /// </summary>
    public static List<AuctionRecord> GetAllAuctions()
    {
        return new List<AuctionRecord>(_auctions.Values);
    }
    
    /// <summary>
    /// 获取所有活跃的拍卖品
    /// </summary>
    public static List<AuctionRecord> GetActiveAuctions()
    {
        var active = new List<AuctionRecord>();
        foreach (var auction in _auctions.Values)
        {
            if (auction.IsActive && auction.Status == AuctionListingStatus.Active && auction.EndTime > DateTime.UtcNow)
            {
                active.Add(auction);
            }
        }
        return active;
    }
    
    /// <summary>
    /// 按稀有度获取拍卖品
    /// </summary>
    public static List<AuctionRecord> GetAuctionsByRarity(int rarity)
    {
        var result = new List<AuctionRecord>();
        foreach (var auction in GetActiveAuctions())
        {
            if (auction.ItemRarity == rarity)
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
    /// <summary>
    /// 按搜索词获取拍卖品
    /// </summary>
    public static List<AuctionRecord> GetAuctionsBySearch(string searchTerm)
    {
        var result = new List<AuctionRecord>();
        foreach (var auction in GetActiveAuctions())
        {
            if (auction.ItemName.ToLower().Contains(searchTerm.ToLower()))
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
    /// <summary>
    /// 获取玩家的挂单
    /// </summary>
    public static List<AuctionRecord> GetPlayerListings(string playerName)
    {
        var result = new List<AuctionRecord>();
        foreach (var auction in _auctions.Values)
        {
            if (auction.SellerName == playerName && auction.IsActive)
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
    /// <summary>
    /// 获取玩家的竞拍
    /// </summary>
    public static List<AuctionRecord> GetPlayerBids(string playerName)
    {
        var result = new List<AuctionRecord>();
        foreach (var auction in GetActiveAuctions())
        {
            if (auction.HighestBidder == playerName)
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
    /// <summary>
    /// 获取玩家拍卖数据
    /// </summary>
    public static PlayerAuctionData GetPlayerData(string playerName)
    {
        if (!_playerData.ContainsKey(playerName))
        {
            _playerData[playerName] = new PlayerAuctionData
            {
                PlayerName = playerName,
                TotalSales = 0,
                TotalPurchases = 0,
                TotalSpent = 0,
                TotalEarned = 0
            };
        }
        return _playerData[playerName];
    }
    
    /// <summary>
    /// 更新玩家数据
    /// </summary>
    public static void UpdatePlayerData(PlayerAuctionData data)
    {
        _playerData[data.PlayerName] = data;
    }
    
    /// <summary>
    /// 更新拍卖品信息
    /// </summary>
    public static void UpdateAuction( AuctionRecord auction)
    {
        if (_auctions.ContainsKey(auction.Id))
        {
            _auctions[auction.Id] = auction;
        }
    }
    
    /// <summary>
    /// 处理过期的拍卖品
    /// </summary>
    public static void ProcessExpiredAuctions()
    {
        var now = DateTime.UtcNow;
        foreach (var auction in _auctions.Values)
        {
            if (auction.IsActive && auction.Status == AuctionListingStatus.Active && auction.EndTime <= now)
            {
                // 拍卖结束
                if (!string.IsNullOrEmpty(auction.HighestBidder))
                {
                    // 物品出售给出价最高者
                    auction.Status = AuctionListingStatus.Sold;
                    auction.IsActive = false; 
                    
                    // 更新卖家数据
                    var sellerData = GetPlayerData(auction.SellerName);
                    int netEarnings = (int)(auction.CurrentBid * (1 - SALE_FEE_PERCENT));
                    sellerData.TotalSales++;
                    sellerData.TotalEarned += netEarnings;
                    UpdatePlayerData(sellerData);
                    
                    // 更新买家数据
                    var buyerData = GetPlayerData(auction.HighestBidder);
                    buyerData.TotalPurchases++;
                    buyerData.TotalSpent += auction.CurrentBid;
                    buyerData.WonAuctions.Add(auction.Id);
                    UpdatePlayerData(buyerData);
                }
                else
                {
                    // 无人出价，拍卖过期
                    auction.Status = AuctionListingStatus.Expired;
                    auction.IsActive = false; 
                }
            }
        }
    }
    
    /// <summary>
    /// 获取已过期的拍卖品
    /// </summary>
    public static List<AuctionRecord> GetExpiredAuctions()
    {
        var result = new List<AuctionRecord>();
        foreach (var auction in _auctions.Values)
        {
            if (!auction.IsActive && (auction.Status == AuctionListingStatus.Sold || auction.Status == AuctionListingStatus.Expired))
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
    /// <summary>
    /// 计算挂单费用
    /// </summary>
    public static int CalculateListingFee(int price)
    {
        return (int)(price * LISTING_FEE_PERCENT);
    }
    
    /// <summary>
    /// 计算销售费用
    /// </summary>
    public static int CalculateSaleFee(int price)
    {
        return (int)(price * SALE_FEE_PERCENT);
    }
}
