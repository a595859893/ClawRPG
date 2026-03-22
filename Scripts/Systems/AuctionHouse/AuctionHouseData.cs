using Godot;
using System;
using System.Collections.Generic;

public class AuctionHouseData
{
    public Dictionary<int, AuctionItem> ActiveListings = new Dictionary<int, AuctionItem>();
    public Dictionary<int, List<int>> PurchaseHistory = new Dictionary<int, List<int>>();
    public int TotalListings { get; set; }
    public int TotalSales { get; set; }
    public long LastUpdate { get; set; }
    
    public int NextListingId { get; set; } = 1;
    
    // ==================== 持久化接口 ====================
    public Dictionary<string, object> ExportSaveData()
    {
        var listings = new List<Dictionary<string, object>>();
        foreach (var kvp in ActiveListings)
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
        foreach (var kvp in PurchaseHistory)
        {
            purchases.Add(new Dictionary<string, object>
            {
                { "playerId", kvp.Key },
                { "listingIds", kvp.Value }
            });
        }
        
        return new Dictionary<string, object>
        {
            { "activeListings", listings },
            { "purchaseHistory", purchases },
            { "totalListings", TotalListings },
            { "totalSales", TotalSales },
            { "nextListingId", NextListingId }
        };
    }

    public bool ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return false;
        
        if (data.ContainsKey("activeListings"))
        {
            var listings = data["activeListings"] as List<Dictionary<string, object>>;
            foreach (var listingData in listings)
            {
                var listing = new AuctionItem
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
                ActiveListings[id] = listing;
            }
        }
        
        if (data.ContainsKey("purchaseHistory"))
        {
            var purchases = data["purchaseHistory"] as List<Dictionary<string, object>>;
            foreach (var purchaseData in purchases)
            {
                int playerId = Convert.ToInt32(purchaseData["playerId"]);
                var listingIds = purchaseData["listingIds"] as List<object>;
                var ids = new List<int>();
                foreach (var id in listingIds)
                {
                    ids.Add(Convert.ToInt32(id));
                }
                PurchaseHistory[playerId] = ids;
            }
        }
        
        if (data.ContainsKey("totalListings"))
            TotalListings = Convert.ToInt32(data["totalListings"]);
        
        if (data.ContainsKey("totalSales"))
            TotalSales = Convert.ToInt32(data["totalSales"]);
        
        if (data.ContainsKey("nextListingId"))
            NextListingId = Convert.ToInt32(data["nextListingId"]);
        
        return true;
    }
    // ==================== 持久化接口结束 ====================
}
