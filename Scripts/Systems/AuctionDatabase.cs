using Godot;
using System;
using System.Collections.Generic;

public class AuctionItem
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
    public AuctionItemStatus Status { get; set; }
}

public enum AuctionItemStatus
{
    Active,
    Sold,
    Expired,
    Cancelled
}

public class PlayerAuctionData
{
    public string PlayerName { get; set; }
    public int TotalSales { get; set; }
    public int TotalPurchases { get; set; }
    public int TotalSpent { get; set; }
    public int TotalEarned { get; set; }
    public List<int> ActiveListings { get; set; } = new List<int>();
    public List<int> WonAuctions { get; set; } = new List<int>();
}

public static class AuctionDatabase
{
    private static readonly Dictionary<int, AuctionItem> _auctions = new Dictionary<int>();
    private static int _nextAuctionId = 1;
    private static readonly Dictionary<string, PlayerAuctionData> _playerData = new Dictionary<string, PlayerAuctionData>();
    
    // Auction duration in hours
    public const int AUCTION_DURATION_HOURS = 24;
    
    // Fee percentages
    public const float LISTING_FEE_PERCENT = 0.02f; // 2% listing fee
    public const float SALE_FEE_PERCENT = 0.05f; // 5% sale fee
    
    // Max listings per player
    public const int MAX_ACTIVE_LISTINGS = 10;
    
    // Price ranges
    public const int MIN_AUCTION_PRICE = 10;
    public const int MAX_AUCTION_PRICE = 999999999;
    
    static AuctionDatabase()
    {
        // Initialize with sample auctions
        InitializeSampleAuctions();
    }
    
    private static void InitializeSampleAuctions()
    {
        // Sample auction items for demonstration
        var sampleAuctions = new[]
        {
            new AuctionItem
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
                Status = AuctionItemStatus.Active
            },
            new AuctionItem
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
                Status = AuctionItemStatus.Active
            },
            new AuctionItem
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
                Status = AuctionItemStatus.Active
            }
        };
        
        foreach (var auction in sampleAuctions)
        {
            _auctions[auction.Id] = auction;
        }
    }
    
    public static int GetNextAuctionId() => _nextAuctionId++;
    
    public static void AddAuction(AuctionItem auction)
    {
        _auctions[auction.Id] = auction;
    }
    
    public static AuctionItem GetAuction(int id)
    {
        return _auctions.ContainsKey(id) ? _auctions[id] : null;
    }
    
    public static List<AuctionItem> GetAllAuctions()
    {
        return new List<AuctionItem>(_auctions.Values);
    }
    
    public static List<AuctionItem> GetActiveAuctions()
    {
        var active = new List<AuctionItem>();
        foreach (var auction in _auctions.Values)
        {
            if (auction.IsActive && auction.Status == AuctionItemStatus.Active && auction.EndTime > DateTime.UtcNow)
            {
                active.Add(auction);
            }
        }
        return active;
    }
    
    public static List<AuctionItem> GetAuctionsByRarity(int rarity)
    {
        var result = new List<AuctionItem>();
        foreach (var auction in GetActiveAuctions())
        {
            if (auction.ItemRarity == rarity)
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
    public static List<AuctionItem> GetAuctionsBySearch(string searchTerm)
    {
        var result = new List<AuctionItem>();
        foreach (var auction in GetActiveAuctions())
        {
            if (auction.ItemName.ToLower().Contains(searchTerm.ToLower()))
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
    public static List<AuctionItem> GetPlayerListings(string playerName)
    {
        var result = new List<AuctionItem>();
        foreach (var auction in _auctions.Values)
        {
            if (auction.SellerName == playerName && auction.IsActive)
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
    public static List<AuctionItem> GetPlayerBids(string playerName)
    {
        var result = new List<AuctionItem>();
        foreach (var auction in GetActiveAuctions())
        {
            if (auction.HighestBidder == playerName)
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
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
    
    public static void UpdatePlayerData(PlayerAuctionData data)
    {
        _playerData[data.PlayerName] = data;
    }
    
    public static void UpdateAuction(AuctionItem auction)
    {
        if (_auctions.ContainsKey(auction.Id))
        {
            _auctions[auction.Id] = auction;
        }
    }
    
    public static void ProcessExpiredAuctions()
    {
        var now = DateTime.UtcNow;
        foreach (var auction in _auctions.Values)
        {
            if (auction.IsActive && auction.Status == AuctionItemStatus.Active && auction.EndTime <= now)
            {
                // Auction ended
                if (!string.IsNullOrEmpty(auction.HighestBidder))
                {
                    // Item sold to highest bidder
                    auction.Status = AuctionItemStatus.Sold;
                    auction.IsActive = false;
                    
                    // Update seller data
                    var sellerData = GetPlayerData(auction.SellerName);
                    int netEarnings = (int)(auction.CurrentBid * (1 - SALE_FEE_PERCENT));
                    sellerData.TotalSales++;
                    sellerData.TotalEarned += netEarnings;
                    UpdatePlayerData(sellerData);
                    
                    // Update buyer data
                    var buyerData = GetPlayerData(auction.HighestBidder);
                    buyerData.TotalPurchases++;
                    buyerData.TotalSpent += auction.CurrentBid;
                    buyerData.WonAuctions.Add(auction.Id);
                    UpdatePlayerData(buyerData);
                }
                else
                {
                    // No bids, auction expired
                    auction.Status = AuctionItemStatus.Expired;
                    auction.IsActive = false;
                }
            }
        }
    }
    
    public static List<AuctionItem> GetExpiredAuctions()
    {
        var result = new List<AuctionItem>();
        foreach (var auction in _auctions.Values)
        {
            if (!auction.IsActive && (auction.Status == AuctionItemStatus.Sold || auction.Status == AuctionItemStatus.Expired))
            {
                result.Add(auction);
            }
        }
        return result;
    }
    
    public static int CalculateListingFee(int price)
    {
        return (int)(price * LISTING_FEE_PERCENT);
    }
    
    public static int CalculateSaleFee(int price)
    {
        return (int)(price * SALE_FEE_PERCENT);
    }
}
