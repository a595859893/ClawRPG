using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.AuctionHouse
{
public class AuctionHouseData
{
    public Dictionary<int, AuctionItem> ActiveListings = new Dictionary<int, AuctionItem>();
    public Dictionary<int, List<int>> PurchaseHistory = new Dictionary<int, List<int>>();
    // PurchasedItems: playerId -> list of AuctionItem copies (preserved after original listing is removed)
    public Dictionary<int, List<AuctionItem>> PurchasedItems = new Dictionary<int, List<AuctionItem>>();
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

        var purchasedItems = new List<Dictionary<string, object>>();
        foreach (var kvp in PurchasedItems)
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
            { "activeListings", listings },
            { "purchaseHistory", purchases },
            { "purchasedItems", purchasedItems },
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
                PurchasedItems[playerId] = items;
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
}
