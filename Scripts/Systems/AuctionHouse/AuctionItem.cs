using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.AuctionHouse {
public class AuctionItem
{
    public string ItemId { get; set; }
    public string ItemName { get; set; }
    public string ItemDescription { get; set; }
    public int Quantity { get; set; }
    public int PricePerUnit { get; set; }
    public string SellerName { get; set; }
    public int SellerId { get; set; }
    public long ListingTime { get; set; }
    public long ExpireTime { get; set; }
    public string Rarity { get; set; }
    public string Category { get; set; }
    // UI helper fields (not persisted)
    public int CurrentBid { get; set; }
    public int BuyNowPrice { get; set; }
    // Purchase metadata (populated when item is bought)
    public int PurchasePrice { get; set; }
    public long PurchaseTime { get; set; }
}
}
