using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 拍卖行系统 - 管理玩家间的物品拍卖
/// 支持物品挂售、出价、一口价购买等功能
/// </summary>
public class AuctionHouseSystem : BaseSystem
{
    /// <summary>
    /// 获取系统单例实例
    /// </summary>
    public static AuctionHouseSystem Instance { get; private set; }
    
    [Signal]
    [Signal]
    public delegate void auction_listing_updated();
    
    [Signal]
    [Signal]
    public delegate void bid_placed(string bidder, int auction_id, int amount);
    
    [Signal]
    [Signal]
    public delegate void auction_won(string winner, int auction_id, int amount);
    
    [Signal]
    [Signal]
    public delegate void auction_ended(int auction_id, string status);
    
    private Player _player;
    private List<AuctionItem> _currentListings = new List<AuctionItem>();
    private List<AuctionItem> _playerListings = new List<AuctionItem>();
    private List<AuctionItem> _playerBids = new List<AuctionItem>();
    private int _selectedListingIndex = -1;
    private string _searchTerm = "";
    private int _filterRarity = -1; // -1 = all
    
    public override void _Ready()
    {
        Instance = this;
        AddToGroup("AuctionHouseSystem");
    }
    
    /// <summary>
    /// 系统名称
    /// </summary>
    protected override string SystemName => "AuctionHouse";
    
    /// <summary>
    /// 初始化系统
    /// </summary>
    public void Initialize(Player player)
    {
        _player = player;
        RefreshListings();
    }
    
    /// <summary>
    /// 刷新拍卖品列表
    /// </summary>
    public void RefreshListings()
    {
        AuctionDatabase.ProcessExpiredAuctions();
        _currentListings = AuctionDatabase.GetActiveAuctions();
        
        if (_player != null)
        {
            _playerListings = AuctionDatabase.GetPlayerListings(_player.PlayerName);
            _playerBids = AuctionDatabase.GetPlayerBids(_player.PlayerName);
        }
        
        EmitSignal("auction_listing_updated");
    }
    
    /// <summary>
    /// 获取过滤后的拍卖品列表
    /// </summary>
    public List<AuctionItem> GetFilteredListings()
    {
        var listings = new List<AuctionItem>();
        
        foreach (var auction in _currentListings)
        {
            // Apply search filter
            if (!string.IsNullOrEmpty(_searchTerm))
            {
                if (!auction.ItemName.ToLower().Contains(_searchTerm.ToLower()))
                    continue;
            }
            
            // Apply rarity filter
            if (_filterRarity >= 0 && auction.ItemRarity != _filterRarity)
                continue;
            
            listings.Add(auction);
        }
        
        return listings;
    }
    
    /// <summary>
    /// 设置搜索关键词
    /// </summary>
    public void SetSearchTerm(string term)
    {
        _searchTerm = term;
        EmitSignal("auction_listing_updated");
    }
    
    /// <summary>
    /// 设置稀有度过滤
    /// </summary>
    public void SetRarityFilter(int rarity)
    {
        _filterRarity = rarity;
        EmitSignal("auction_listing_updated");
    }
    
    public void SetSelectedListing(int index)
    {
        _selectedListingIndex = index;
    }
    
    public AuctionItem GetSelectedListing()
    {
        var listings = GetFilteredListings();
        if (_selectedListingIndex >= 0 && _selectedListingIndex < listings.Count)
        {
            return listings[_selectedListingIndex];
        }
        return null;
    }
    
    public List<AuctionItem> GetCurrentListings() => _currentListings;
    public List<AuctionItem> GetPlayerListings() => _playerListings;
    public List<AuctionItem> GetPlayerBids() => _playerBids;
    
    public bool CanListItem()
    {
        if (_player == null) return false;
        var playerData = AuctionDatabase.GetPlayerData(_player.PlayerName);
        return playerData.ActiveListings.Count < AuctionDatabase.MAX_ACTIVE_LISTINGS;
    }
    
    public int GetListingFee(int price)
    {
        return AuctionDatabase.CalculateListingFee(price);
    }
    
    public bool ListItem(int itemId, string itemName, int itemRarity, int quantity, int startingBid, int buyNowPrice)
    {
        if (_player == null) return false;
        
        var playerData = AuctionDatabase.GetPlayerData(_player.PlayerName);
        
        // Check max listings
        if (playerData.ActiveListings.Count >= AuctionDatabase.MAX_ACTIVE_LISTINGS)
        {
            GD.Print($"AuctionHouse: Max listings reached ({AuctionDatabase.MAX_ACTIVE_LISTINGS})");
            return false;
        }
        
        // Check price limits
        if (startingBid < AuctionDatabase.MIN_AUCTION_PRICE || startingBid > AuctionDatabase.MAX_AUCTION_PRICE)
        {
            GD.Print($"AuctionHouse: Starting bid must be between {AuctionDatabase.MIN_AUCTION_PRICE} and {AuctionDatabase.MAX_AUCTION_PRICE}");
            return false;
        }
        
        if (buyNowPrice < startingBid || buyNowPrice > AuctionDatabase.MAX_AUCTION_PRICE)
        {
            GD.Print($"AuctionHouse: Buy now price must be >= starting bid and <= {AuctionDatabase.MAX_AUCTION_PRICE}");
            return false;
        }
        
        // Check listing fee
        int listingFee = GetListingFee(startingBid);
        if (_player.Gold < listingFee)
        {
            GD.Print($"AuctionHouse: Not enough gold for listing fee ({listingFee})");
            return false;
        }
        
        // Deduct listing fee
        _player.Gold -= listingFee;
        
        // Create auction
        var auction = new AuctionItem
        {
            Id = AuctionDatabase.GetNextAuctionId(),
            SellerName = _player.PlayerName,
            ItemId = itemId,
            ItemName = itemName,
            ItemRarity = itemRarity,
            Quantity = quantity,
            CurrentBid = startingBid,
            BuyNowPrice = buyNowPrice,
            HighestBidder = "",
            StartTime = DateTime.UtcNow,
            EndTime = DateTime.UtcNow.AddHours(AuctionDatabase.AUCTION_DURATION_HOURS),
            IsActive = true,
            Status = AuctionItemStatus.Active
        };
        
        AuctionDatabase.AddAuction(auction);
        playerData.ActiveListings.Add(auction.Id);
        AuctionDatabase.UpdatePlayerData(playerData);
        
        RefreshListings();
        
        GD.Print($"AuctionHouse: Listed {itemName} for {startingBid} gold (fee: {listingFee})");
        return true;
    }
    
    public bool PlaceBid(int auctionId, int bidAmount)
    {
        if (_player == null) return false;
        
        var auction = AuctionDatabase.GetAuction(auctionId);
        if (auction == null || !auction.IsActive)
        {
            GD.Print("AuctionHouse: Auction not found or inactive");
            return false;
        }
        
        if (auction.SellerName == _player.PlayerName)
        {
            GD.Print("AuctionHouse: Cannot bid on your own auction");
            return false;
        }
        
        if (DateTime.UtcNow >= auction.EndTime)
        {
            GD.Print("AuctionHouse: Auction has ended");
            return false;
        }
        
        if (bidAmount <= auction.CurrentBid)
        {
            GD.Print($"AuctionHouse: Bid must be higher than current bid ({auction.CurrentBid})");
            return false;
        }
        
        if (_player.Gold < bidAmount)
        {
            GD.Print("AuctionHouse: Not enough gold");
            return false;
        }
        
        // Refund previous highest bidder
        if (!string.IsNullOrEmpty(auction.HighestBidder))
        {
            // In a real game, we'd add gold back to the previous bidder
            // For simplicity, we just track the current bid
        }
        
        // Deduct bid amount
        _player.Gold -= bidAmount;
        
        // Update auction
        auction.HighestBidder = _player.PlayerName;
        auction.CurrentBid = bidAmount;
        AuctionDatabase.UpdateAuction(auction);
        
        EmitSignal("bid_placed", _player.PlayerName, auctionId, bidAmount);
        
        RefreshListings();
        
        GD.Print($"AuctionHouse: Placed bid of {bidAmount} on {auction.ItemName}");
        return true;
    }
    
    public bool BuyNow(int auctionId)
    {
        if (_player == null) return false;
        
        var auction = AuctionDatabase.GetAuction(auctionId);
        if (auction == null || !auction.IsActive)
        {
            GD.Print("AuctionHouse: Auction not found or inactive");
            return false;
        }
        
        if (auction.SellerName == _player.PlayerName)
        {
            GD.Print("AuctionHouse: Cannot buy your own item");
            return false;
        }
        
        if (DateTime.UtcNow >= auction.EndTime)
        {
            GD.Print("AuctionHouse: Auction has ended");
            return false;
        }
        
        if (_player.Gold < auction.BuyNowPrice)
        {
            GD.Print($"AuctionHouse: Not enough gold (need {auction.BuyNowPrice})");
            return false;
        }
        
        // Deduct gold
        _player.Gold -= auction.BuyNowPrice;
        
        // Process sale
        auction.Status = AuctionItemStatus.Sold;
        auction.IsActive = false; 
        auction.HighestBidder = _player.PlayerName;
        
        // Update seller data
        var sellerData = AuctionDatabase.GetPlayerData(auction.SellerName);
        int netEarnings = (int)(auction.BuyNowPrice * (1 - AuctionDatabase.SALE_FEE_PERCENT));
        sellerData.TotalSales++;
        sellerData.TotalEarned += netEarnings;
        sellerData.ActiveListings.Remove(auction.Id);
        AuctionDatabase.UpdatePlayerData(sellerData);
        
        // Update buyer data
        var buyerData = AuctionDatabase.GetPlayerData(_player.PlayerName);
        buyerData.TotalPurchases++;
        buyerData.TotalSpent += auction.BuyNowPrice;
        buyerData.WonAuctions.Add(auction.Id);
        AuctionDatabase.UpdatePlayerData(buyerData);
        
        AuctionDatabase.UpdateAuction(auction);
        
        EmitSignal("auction_won", _player.PlayerName, auctionId, auction.BuyNowPrice);
        
        // Add item to buyer inventory (simplified - would need actual item system integration)
        GD.Print($"AuctionHouse: Bought {auction.ItemName} for {auction.BuyNowPrice} gold");
        
        RefreshListings();
        
        return true;
    }
    
    public bool CancelListing(int auctionId)
    {
        if (_player == null) return false;
        
        var auction = AuctionDatabase.GetAuction(auctionId);
        if (auction == null)
        {
            GD.Print("AuctionHouse: Auction not found");
            return false;
        }
        
        if (auction.SellerName != _player.PlayerName)
        {
            GD.Print("AuctionHouse: Not your auction");
            return false;
        }
        
        if (!auction.IsActive)
        {
            GD.Print("AuctionHouse: Auction is not active");
            return false;
        }
        
        if (!string.IsNullOrEmpty(auction.HighestBidder))
        {
            GD.Print("AuctionHouse: Cannot cancel auction with active bids");
            return false;
        }
        
        // Cancel auction
        auction.Status = AuctionItemStatus.Cancelled;
        auction.IsActive = false; 
        
        // Update player data
        var playerData = AuctionDatabase.GetPlayerData(_player.PlayerName);
        playerData.ActiveListings.Remove(auction.Id);
        AuctionDatabase.UpdatePlayerData(playerData);
        
        AuctionDatabase.UpdateAuction(auction);
        
        EmitSignal("auction_ended", auctionId, "Cancelled");
        
        RefreshListings();
        
        GD.Print($"AuctionHouse: Cancelled listing for {auction.ItemName}");
        return true;
    }
    
    public PlayerAuctionData GetPlayerAuctionStats()
    {
        if (_player == null) return null;
        return AuctionDatabase.GetPlayerData(_player.PlayerName);
    }
    
    public int GetPlayerGold()
    {
        return _player != null ? _player.Gold : 0;
    }
    
    public Dictionary<string, object> GetSaveData()
    {
        var data = new Dictionary<string, object>();
        data["player_auction_data"] = GetPlayerAuctionStats();
        return data;
    }
    
    public void LoadSaveData(Dictionary<string, object> data)
    {
        // Player auction data is tracked by player name in the database
        // No need to restore from save in this implementation
    }
    
    public string FormatTimeRemaining(DateTime endTime)
    {
        var remaining = endTime - DateTime.UtcNow;
        if (remaining.TotalSeconds <= 0)
            return "Ended";
        
        if (remaining.TotalHours >= 24)
            return $"{(int)remaining.TotalDays}d {remaining.Hours}h";
        else if (remaining.TotalMinutes >= 60)
            return $"{(int)remaining.TotalHours}h {remaining.Minutes}m";
        else
            return $"{remaining.Minutes}m {remaining.Seconds}s";
    }
    
    public string GetRarityColor(int rarity)
    {
        switch (rarity)
        {
            case 0: return "#9E9E9E"; // Common - Gray
            case 1: return "#4CAF50";  // Uncommon - Green
            case 2: return "#2196F3";  // Rare - Blue
            case 3: return "#9C27B0"; // Epic - Purple
            case 4: return "#FF9800"; // Legendary - Orange
            case 5: return "#F44336"; // Mythic - Red
            default: return "#FFFFFF";
        }
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        data["search_term"] = _searchTerm;
        data["filter_rarity"] = _filterRarity;
        
        // 玩家拍卖品
        var playerListings = new Array();
        foreach (var item in _playerListings)
        {
            playerListings.Add(item.ItemId);
        }
        data["player_listings"] = playerListings;
        
        // 玩家竞拍
        var playerBids = new Array();
        foreach (var item in _playerBids)
        {
            playerBids.Add(item.ItemId);
        }
        data["player_bids"] = playerBids;
        
        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        if (data.Contains("search_term")) _searchTerm = (string)data["search_term"];
        if (data.Contains("filter_rarity")) _filterRarity = (int)data["filter_rarity"];
    }
}
