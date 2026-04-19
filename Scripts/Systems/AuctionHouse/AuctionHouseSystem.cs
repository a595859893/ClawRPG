using Godot;
using ClawRPG.Systems.AuctionHouse;
using AuctionItem = ClawRPG.Systems.AuctionHouse.AuctionItem;
using System;
using System.Collections.Generic;

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
        
        LoadAuctionData();
        
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

        // Store a copy in PurchasedItems so GetMyPurchases() still works after listing is removed
        if (!_data.PurchasedItems.ContainsKey(buyerId))
        {
            _data.PurchasedItems[buyerId] = new List<AuctionItem>();
        }
        // Create a copy with purchase metadata
        var purchasedItem = new AuctionItem
        {
            ItemId = listing.ItemId,
            ItemName = listing.ItemName,
            ItemDescription = listing.ItemDescription,
            Quantity = listing.Quantity,
            PricePerUnit = listing.PricePerUnit,
            SellerName = listing.SellerName,
            SellerId = listing.SellerId,
            ListingTime = listing.ListingTime,
            ExpireTime = listing.ExpireTime,
            Rarity = listing.Rarity,
            Category = listing.Category,
            PurchasePrice = totalCost,
            PurchaseTime = (long)(Time.GetUnixTimeFromSystem() * 1000)
        };
        _data.PurchasedItems[buyerId].Add(purchasedItem);

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
    
    // Placeholder: gold is managed by EconomySystem, not AuctionHouseSystem
    public int GetPlayerGold() => 0;

    public bool PlaceBid(int listingId, int amount)
    {
        // Bidding system not yet fully implemented
        GD.Print($"AuctionHouseSystem: PlaceBid called for listing {listingId} with amount {amount} (not implemented)");
        return false;
    }

    public bool BuyNow(int listingId)
    {
        // BuyNow requires player context; stub for compilation
        GD.Print($"AuctionHouseSystem: BuyNow called for listing {listingId} (not implemented)");
        return false;
    }

    public string FormatTimeRemaining(long timestamp)
    {
        long now = (long)(Time.GetUnixTimeFromSystem() * 1000);
        long diff = timestamp - now;
        if (diff <= 0) return "已结束";
        int hours = (int)(diff / 3600000);
        int minutes = (int)((diff % 3600000) / 60000);
        if (hours > 24)
            return $"{(hours / 24)}天 {hours % 24}小时";
        return $"{hours}小时 {minutes}分钟";
    }

    // Search and filter state (stubs for UI calls)
    private string _searchTerm = "";
    private int _rarityFilter = -1;

    public void SetSearchTerm(string term) { _searchTerm = term; }
    public void SetRarityFilter(int rarity) { _rarityFilter = rarity; }

    public Dictionary<string, object> GetPlayerAuctionStats()
    {
        // Compute stats from purchased items
        int totalPurchases = 0;
        int totalSpent = 0;
        if (_data.PurchasedItems.TryGetValue(1, out var purchases))
        {
            totalPurchases = purchases.Count;
            foreach (var p in purchases)
                totalSpent += p.PurchasePrice;
        }
        return new Dictionary<string, object>
        {
            { "TotalSales", _data.TotalSales },
            { "TotalPurchases", totalPurchases },
            { "TotalEarned", 0 },
            { "TotalSpent", totalSpent }
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
        if (_data.PurchasedItems.TryGetValue(playerId, out var purchases))
        {
            return new List<AuctionItem>(purchases);
        }
        return new List<AuctionItem>();
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
        try
        {
            var saveData = GetSaveData();
            var file = new Godot.File();
            if (file.Open("user://auction_house_save.dat", Godot.File.ModeFlags.Write) == Error.Ok)
            {
                file.StoreString(JSON.Print(saveData));
                file.Close();
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AuctionHouseSystem] SaveAuctionData failed: {ex.Message}");
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

        var purchasedItems = new List<Dictionary<string, object>>();
        foreach (var kvp in _data.PurchasedItems)
        {
            var items = new List<Dictionary<string, object>>();
            foreach (var item in kvp.Value)
            {
                items.Add(new Dictionary<string, object>
                {
                    { "itemId", item.ItemId },
                    { "itemName", item.ItemName },
                    { "itemDescription", item.ItemDescription },
                    { "quantity", item.Quantity },
                    { "pricePerUnit", item.PricePerUnit },
                    { "sellerName", item.SellerName },
                    { "sellerId", item.SellerId },
                    { "listingTime", item.ListingTime },
                    { "expireTime", item.ExpireTime },
                    { "rarity", item.Rarity },
                    { "category", item.Category },
                    { "purchasePrice", item.PurchasePrice },
                    { "purchaseTime", item.PurchaseTime }
                });
            }
            purchasedItems.Add(new Dictionary<string, object>
            {
                { "playerId", kvp.Key },
                { "items", items }
            });
        }

        return new Dictionary<string, object>
        {
            { "listings", listings },
            { "purchases", purchases },
            { "purchasedItems", purchasedItems },
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
        
        if (data.ContainsKey("purchases"))
        {
            var purchases = data["purchases"] as List<Dictionary<string, object>>;
            foreach (var purchaseData in purchases)
            {
                int playerId = Convert.ToInt32(purchaseData["playerId"]);
                var listingIds = purchaseData["listingIds"] as List<object>;
                var ids = new List<int>();
                foreach (var lid in listingIds)
                {
                    ids.Add(Convert.ToInt32(lid));
                }
                _data.PurchaseHistory[playerId] = ids;
            }
        }

        if (data.ContainsKey("purchasedItems"))
        {
            var purchasedItems = data["purchasedItems"] as List<Dictionary<string, object>>;
            foreach (var playerData in purchasedItems)
            {
                int playerId = Convert.ToInt32(playerData["playerId"]);
                var itemsList = playerData["items"] as List<Dictionary<string, object>>;
                var items = new List<AuctionItem>();
                foreach (var itemData in itemsList)
                {
                    var item = new AuctionItem
                    {
                        ItemId = itemData["itemId"].ToString(),
                        ItemName = itemData["itemName"].ToString(),
                        ItemDescription = itemData["itemDescription"].ToString(),
                        Quantity = Convert.ToInt32(itemData["quantity"]),
                        PricePerUnit = Convert.ToInt32(itemData["pricePerUnit"]),
                        SellerName = itemData["sellerName"].ToString(),
                        SellerId = Convert.ToInt32(itemData["sellerId"]),
                        ListingTime = Convert.ToInt64(itemData["listingTime"]),
                        ExpireTime = Convert.ToInt64(itemData["expireTime"]),
                        Rarity = itemData["rarity"].ToString(),
                        Category = itemData["category"].ToString(),
                        PurchasePrice = Convert.ToInt32(itemData["purchasePrice"]),
                        PurchaseTime = Convert.ToInt64(itemData["purchaseTime"])
                    };
                    items.Add(item);
                }
                _data.PurchasedItems[playerId] = items;
            }
        }

        GD.Print("AuctionHouseSystem: 拍卖行数据已加载");
    }

    private void LoadAuctionData()
    {
        try
        {
            var file = new Godot.File();
            if (file.FileExists("user://auction_house_save.dat"))
            {
                if (file.Open("user://auction_house_save.dat", Godot.File.ModeFlags.Read) == Error.Ok)
                {
                    var json = file.GetAsText();
                    file.Close();
                    var result = JSON.Parse(json);
                    if (result.Error == Error.Ok && result.Result is Dictionary dict)
                    {
                        LoadSaveData(dict);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            GD.PrintErr($"[AuctionHouseSystem] LoadAuctionData failed: {ex.Message}");
        }
    }
}
