using Godot;
using ClawRPG.Systems.AuctionHouse;
using AuctionItem = ClawRPG.Systems.AuctionHouse.AuctionItem;
using System;
using System.Collections.Generic;
using ClawRPG.Systems.AuctionHouse;
using AuctionItem = ClawRPG.Systems.AuctionHouse.AuctionItem;

public partial class AuctionHouseSystem : BaseSystem
{
    private AuctionHouseData _data;
    private const int LISTING_FEE_PERCENT = 5;
    private const int MIN_LISTING_PRICE = 10;
    private const int MAX_LISTING_DURATION = 172800000;
    private const int MIN_DURATION = 3600000;
    
    public override void _Ready()
    {
        _data = new AuctionHouseData();
        
        AddToGroup("save");
        AddToGroup("auction_house");
        
        GD.Print("AuctionHouseSystem: 拍卖行系统已初始化");
    }
    
    public Dictionary<string, object> CreateListing(string itemId, string itemName, string itemDescription,
        int quantity, int pricePerUnit, string sellerName, int sellerId, string rarity, string category, int durationMs)
    {
        if (quantity <= 0 || pricePerUnit < MIN_LISTING_PRICE)
        {
            return new Dictionary<string, object> { { "success", false }, { "message", "无效的数量或价格" } };
        }
        
        int totalPrice = quantity * pricePerUnit;
        int listingFee = (int)(totalPrice * LISTING_FEE_PERCENT / 100.0);
        
        if (durationMs < MIN_DURATION || durationMs > MAX_LISTING_DURATION)
        {
            return new Dictionary<string, object> { { "success", false }, { "message", "无效的挂售时长" } };
        }
        
        long currentTime = (long)(Time.GetUnixTimeFromSystem() * 1000);
        
        var listing = new ClawRPG.Systems.AuctionHouse.AuctionItem
        {
            ItemId = itemId,
            ItemName = itemName,
            ItemDescription = itemDescription,
            Quantity = quantity,
            PricePerUnit = pricePerUnit,
            SellerName = sellerName,
            SellerId = sellerId,
            ListingTime = currentTime,
            ExpireTime = currentTime + durationMs,
            Rarity = rarity,
            Category = category
        };
        
        int listingId = _data.NextListingId++;
        _data.ActiveListings[listingId] = listing;
        _data.TotalListings++;
        _data.LastUpdate = currentTime;
        
        SaveAuctionData();
        
        return new Dictionary<string, object> 
        { 
            { "success", true }, 
            { "listingId", listingId },
            { "listingFee", listingFee },
            { "expireTime", listing.ExpireTime },
            { "message", "物品已挂售" }
        };
    }
    
    public Dictionary<string, object> PurchaseItem(int listingId, int buyerId, string buyerName)
    {
        if (!_data.ActiveListings.ContainsKey(listingId))
        {
            return new Dictionary<string, object> { { "success", false }, { "message", "该物品不存在或已下架" } };
        }
        
        var listing = _data.ActiveListings[listingId];
        
        if (listing.SellerId == buyerId)
        {
            return new Dictionary<string, object> { { "success", false }, { "message", "不能购买自己的物品" } };
        }
        
        if ((long)(Time.GetUnixTimeFromSystem() * 1000) > listing.ExpireTime)
        {
            _data.ActiveListings.Remove(listingId);
            return new Dictionary<string, object> { { "success", false }, { "message", "该物品已过期" } };
        }
        
        int totalCost = listing.Quantity * listing.PricePerUnit;
        int sellerReceive = (int)(totalCost * (100 - LISTING_FEE_PERCENT) / 100.0);
        
        _data.ActiveListings.Remove(listingId);
        _data.TotalSales++;
        _data.LastUpdate = (long)(Time.GetUnixTimeFromSystem() * 1000);
        
        if (!_data.PurchaseHistory.ContainsKey(buyerId))
        {
            _data.PurchaseHistory[buyerId] = new List<int>();
        }
        _data.PurchaseHistory[buyerId].Add(listingId);
        
        SaveAuctionData();
        
        return new Dictionary<string, object> 
        { 
            { "success", true }, 
            { "item", listing },
            { "totalCost", totalCost },
            { "sellerReceive", sellerReceive },
            { "listingFee", totalCost - sellerReceive },
            { "message", "购买成功" }
        };
    }
    
    public Dictionary<string, object> CancelListing(int listingId, int playerId)
    {
        if (!_data.ActiveListings.ContainsKey(listingId))
        {
            return new Dictionary<string, object> { { "success", false }, { "message", "该物品不存在或已下架" } };
        }
        
        var listing = _data.ActiveListings[listingId];
        
        if (listing.SellerId != playerId)
        {
            return new Dictionary<string, object> { { "success", false }, { "message", "只能取消自己的挂售" } };
        }
        
        _data.ActiveListings.Remove(listingId);
        _data.LastUpdate = (long)(Time.GetUnixTimeFromSystem() * 1000);
        
        SaveAuctionData();
        
        return new Dictionary<string, object> 
        { 
            { "success", true },
            { "message", "挂售已取消" }
        };
    }
    
    public List<AuctionItem> GetListings(string category = "", string rarity = "", string searchTerm = "", int maxResults = 50)
    {
        var results = new List<AuctionItem>();
        long currentTime = (long)(Time.GetUnixTimeFromSystem() * 1000);
        
        foreach (var kvp in _data.ActiveListings)
        {
            var listing = kvp.Value;
            
            if (currentTime > listing.ExpireTime)
            {
                continue;
            }
            
            if (!string.IsNullOrEmpty(category) && listing.Category != category)
            {
                continue;
            }
            
            if (!string.IsNullOrEmpty(rarity) && listing.Rarity != rarity)
            {
                continue;
            }
            
            if (!string.IsNullOrEmpty(searchTerm))
            {
                if (!listing.ItemName.ToLower().Contains(searchTerm.ToLower()) &&
                    !listing.ItemDescription.ToLower().Contains(searchTerm.ToLower()))
                {
                    continue;
                }
            }
            
            results.Add(listing);
            
            if (results.Count >= maxResults)
            {
                break;
            }
        }
        
        results.Sort((a, b) => a.ListingTime.CompareTo(b.ListingTime));
        
        return results;
    }
    
    public List<AuctionItem> GetMyListings(int playerId)
    {
        var results = new List<AuctionItem>();
        
        foreach (var kvp in _data.ActiveListings)
        {
            if (kvp.Value.SellerId == playerId)
            {
                results.Add(kvp.Value);
            }
        }
        
        return results;
    }
    
    public List<AuctionItem> GetMyPurchases(int playerId)
    {
        var results = new List<AuctionItem>();
        
        if (_data.PurchaseHistory.ContainsKey(playerId))
        {
            foreach (var listingId in _data.PurchaseHistory[playerId])
            {
            }
        }
        
        return results;
    }
    
    public Dictionary<string, int> GetCategories()
    {
        var categories = new Dictionary<string, int>();
        
        foreach (var kvp in _data.ActiveListings)
        {
            var listing = kvp.Value;
            if ((long)(Time.GetUnixTimeFromSystem() * 1000) <= listing.ExpireTime)
            {
                if (categories.ContainsKey(listing.Category))
                {
                    categories[listing.Category]++;
                }
                else
                {
                    categories[listing.Category] = 1;
                }
            }
        }
        
        return categories;
    }
    
    public Dictionary<string, int> GetRarities()
    {
        var rarities = new Dictionary<string, int>();
        
        foreach (var kvp in _data.ActiveListings)
        {
            var listing = kvp.Value;
            if ((long)(Time.GetUnixTimeFromSystem() * 1000) <= listing.ExpireTime)
            {
                if (rarities.ContainsKey(listing.Rarity))
                {
                    rarities[listing.Rarity]++;
                }
                else
                {
                    rarities[listing.Rarity] = 1;
                }
            }
        }
        
        return rarities;
    }
    
    public Dictionary<string, object> GetStatistics()
    {
        int activeCount = 0;
        long currentTime = (long)(Time.GetUnixTimeFromSystem() * 1000);
        
        foreach (var kvp in _data.ActiveListings)
        {
            if (currentTime <= kvp.Value.ExpireTime)
            {
                activeCount++;
            }
        }
        
        return new Dictionary<string, object>
        {
            { "activeListings", activeCount },
            { "totalListings", _data.TotalListings },
            { "totalSales", _data.TotalSales },
            { "lastUpdate", _data.LastUpdate }
        };
    }
    
    public void CleanExpiredListings()
    {
        long currentTime = (long)(Time.GetUnixTimeFromSystem() * 1000);
        var expiredIds = new List<int>();
        
        foreach (var kvp in _data.ActiveListings)
        {
            if (currentTime > kvp.Value.ExpireTime)
            {
                expiredIds.Add(kvp.Key);
            }
        }
        
        foreach (var id in expiredIds)
        {
            _data.ActiveListings.Remove(id);
        }
        
        if (expiredIds.Count > 0)
        {
            _data.LastUpdate = currentTime;
            SaveAuctionData();
            GD.Print($"AuctionHouseSystem: 清理了 {expiredIds.Count} 个过期挂售");
        }
    }
    
    private void SaveAuctionData()
    {
        var saveSystem = GetTree().GetFirstNodeInGroup("save_system");
        if (saveSystem != null)
        {
            // 保存数据
        }
    }
    
    public Dictionary<string, object> GetSaveData()
    {
        var listings = new List<Dictionary<string, object>>();
        
        foreach (var kvp in _data.ActiveListings)
        {
            listings.Add(new Dictionary<string, object>
            {
                { "id", kvp.Key },
                { "itemId", kvp.Value.ItemId },
                { "itemName", kvp.Value.ItemName },
                { "itemDescription", kvp.Value.ItemDescription },
                { "quantity", kvp.Value.Quantity },
                { "pricePerUnit", kvp.Value.PricePerUnit },
                { "sellerName", kvp.Value.SellerName },
                { "sellerId", kvp.Value.SellerId },
                { "listingTime", kvp.Value.ListingTime },
                { "expireTime", kvp.Value.ExpireTime },
                { "rarity", kvp.Value.Rarity },
                { "category", kvp.Value.Category }
            });
        }
        
        var purchases = new List<Dictionary<string, object>>();
        foreach (var kvp in _data.PurchaseHistory)
        {
            purchases.Add(new Dictionary<string, object>
            {
                { "playerId", kvp.Key },
                { "listingIds", kvp.Value }
            });
        }
        
        return new Dictionary<string, object>
        {
            { "listings", listings },
            { "purchases", purchases },
            { "totalListings", _data.TotalListings },
            { "totalSales", _data.TotalSales },
            { "nextListingId", _data.NextListingId }
        };
    }

    // ==================== BaseSystem 持久化接口 ====================
    public override Dictionary<string, object> ExportSaveData()
    {
        return GetSaveData();
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        LoadSaveData(data);
    }
    // ==================== 持久化接口结束 ====================
    
    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("listings"))
        {
            var listings = data["listings"] as List<Dictionary<string, object>>;
            foreach (var listingData in listings)
            {
                var listing = new ClawRPG.Systems.AuctionHouse.AuctionItem
                {
                    ItemId = listingData["itemId"].ToString(),
                    ItemName = listingData["itemName"].ToString(),
                    ItemDescription = listingData["itemDescription"].ToString(),
                    Quantity = Convert.ToInt32(listingData["quantity"]),
                    PricePerUnit = Convert.ToInt32(listingData["pricePerUnit"]),
                    SellerName = listingData["sellerName"].ToString(),
                    SellerId = Convert.ToInt32(listingData["sellerId"]),
                    ListingTime = Convert.ToInt64(listingData["listingTime"]),
                    ExpireTime = Convert.ToInt64(listingData["expireTime"]),
                    Rarity = listingData["rarity"].ToString(),
                    Category = listingData["category"].ToString()
                };
                
                int id = Convert.ToInt32(listingData["id"]);
                _data.ActiveListings[id] = listing;
            }
        }
        
        if (data.ContainsKey("totalListings"))
        {
            _data.TotalListings = Convert.ToInt32(data["totalListings"]);
        }
        
        if (data.ContainsKey("totalSales"))
        {
            _data.TotalSales = Convert.ToInt32(data["totalSales"]);
        }
        
        if (data.ContainsKey("nextListingId"))
        {
            _data.NextListingId = Convert.ToInt32(data["nextListingId"]);
        }
        
        GD.Print("AuctionHouseSystem: 拍卖行数据已加载");
    }
}
