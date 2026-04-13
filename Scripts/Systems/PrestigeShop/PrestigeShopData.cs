using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PrestigeShop
{
    /// <summary>
    /// Prestige Shop item category
    /// </summary>
    public enum ShopCategory
    {
        Title,       // Player titles displayed next to name
        PetAura,     // Pet prestige aura effects
        PortalEffect,// Base portal visual effects
        FarewellFx,  // Prestige reset farewell animation
        Other        // Miscellaneous
    }

    /// <summary>
    /// How an item is unlocked
    /// </summary>
    public enum UnlockType
    {
        AutoTier,    // Automatically unlocked when reaching a prestige tier
        Purchase     // Must spend prestige points
    }

    /// <summary>
    /// Shop item definition
    /// </summary>
    public class ShopItem
    {
        public string ItemId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public ShopCategory Category { get; set; }
        public UnlockType UnlockType { get; set; }
        public int Cost { get; set; }           // Points cost (for Purchase type)
        public int RequiredTier { get; set; }    // PrestigeLevel threshold (for AutoTier)
        public string TierName { get; set; }     // "Bronze", "Silver", etc.
        public string IconEmoji { get; set; }    // Emoji icon
    }

    /// <summary>
    /// Player's unlock state for a shop item
    /// </summary>
    public class ShopItemState
    {
        public string ItemId;
        public bool Unlocked;
        public bool Purchased;   // true = bought with points, false = auto tier unlock
        public int PurchasedAtTier; // The tier at which it was unlocked
    }

    /// <summary>
    /// Static database of all prestige shop items
    /// </summary>
    public static class PrestigeShopDatabase
    {
        public static readonly List<ShopItem> AllItems = new List<ShopItem>
        {
            // === TITLES (unlocked by tier) ===
            new ShopItem { ItemId = "title_veteran",      DisplayName = "Veteran",     Description = "A seasoned player who has seen many battles.",        Category = ShopCategory.Title, UnlockType = UnlockType.AutoTier,  RequiredTier = 1,  TierName = "Bronze",    IconEmoji = "⚔️" },
            new ShopItem { ItemId = "title_ascended",     DisplayName = "Ascended",   Description = "One who has transcended mortal limitations.",           Category = ShopCategory.Title, UnlockType = UnlockType.AutoTier,  RequiredTier = 5,  TierName = "Silver",    IconEmoji = "🌟" },
            new ShopItem { ItemId = "title_legend",        DisplayName = "Legend",     Description = "Their name echoes through the ages.",                  Category = ShopCategory.Title, UnlockType = UnlockType.AutoTier,  RequiredTier = 10, TierName = "Gold",      IconEmoji = "🏆" },
            new ShopItem { ItemId = "title_demigod",      DisplayName = "Demigod",    Description = "Walking between mortal and divine.",                   Category = ShopCategory.Title, UnlockType = UnlockType.AutoTier,  RequiredTier = 15, TierName = "Platinum",  IconEmoji = "⚡" },
            new ShopItem { ItemId = "title_eternal",      DisplayName = "Eternal",    Description = "Time cannot touch this one's legacy.",                Category = ShopCategory.Title, UnlockType = UnlockType.AutoTier,  RequiredTier = 20, TierName = "Legendary", IconEmoji = "🌌" },

            // === TITLES (purchased with points) ===
            new ShopItem { ItemId = "title_wanderer",     DisplayName = "Wanderer",   Description = "Always searching, never settling.",                  Category = ShopCategory.Title, UnlockType = UnlockType.Purchase, Cost = 500,  IconEmoji = "🥾" },
            new ShopItem { ItemId = "title_reckoner",     DisplayName = "The Reckoner",Description = "Every debt is tallied, every action weighed.",       Category = ShopCategory.Title, UnlockType = UnlockType.Purchase, Cost = 1000, IconEmoji = "📊" },
            new ShopItem { ItemId = "title_phantom",      DisplayName = "Phantom",    Description = "Neither here nor entirely gone.",                      Category = ShopCategory.Title, UnlockType = UnlockType.Purchase, Cost = 1500, IconEmoji = "👻" },

            // === PET AURAS (purchased with points) ===
            new ShopItem { ItemId = "aura_bronze",        DisplayName = "Bronze Aura",    Description = "A warm bronze glow surrounds your companion.",      Category = ShopCategory.PetAura, UnlockType = UnlockType.AutoTier,  RequiredTier = 1,  TierName = "Bronze",    IconEmoji = "🟤" },
            new ShopItem { ItemId = "aura_silver",        DisplayName = "Silver Aura",    Description = "A shimmering silver light enshrouds your pet.",       Category = ShopCategory.PetAura, UnlockType = UnlockType.AutoTier,  RequiredTier = 5,  TierName = "Silver",    IconEmoji = "🔘" },
            new ShopItem { ItemId = "aura_gold",         DisplayName = "Gold Aura",      Description = "Radiant golden light emanates from your friend.",     Category = ShopCategory.PetAura, UnlockType = UnlockType.AutoTier,  RequiredTier = 10, TierName = "Gold",      IconEmoji = "✨" },
            new ShopItem { ItemId = "aura_platinum",      DisplayName = "Platinum Aura",  Description = "A cool platinum shimmer marks elite status.",         Category = ShopCategory.PetAura, UnlockType = UnlockType.AutoTier,  RequiredTier = 15, TierName = "Platinum",  IconEmoji = "💠" },
            new ShopItem { ItemId = "aura_legendary",    DisplayName = "Legendary Aura", Description = "Mythic particles trail your legendary companion.",   Category = ShopCategory.PetAura, UnlockType = UnlockType.AutoTier,  RequiredTier = 20, TierName = "Legendary", IconEmoji = "🔥" },

            // === PET AURAS (purchased) ===
            new ShopItem { ItemId = "aura_galaxy",        DisplayName = "Galaxy Aura",   Description = "A swirling galaxy orbits your faithful friend.",      Category = ShopCategory.PetAura, UnlockType = UnlockType.Purchase, Cost = 2000, IconEmoji = "🌌" },
            new ShopItem { ItemId = "aura_void",          DisplayName = "Void Aura",     Description = "Darkness pools around your companion's silhouette.",  Category = ShopCategory.PetAura, UnlockType = UnlockType.Purchase, Cost = 3000, IconEmoji = "🕳️" },

            // === PORTAL EFFECTS (purchased with points) ===
            new ShopItem { ItemId = "portal_bronze",      DisplayName = "Bronze Portal",  Description = "The base portal gains a bronze metallic tint.",      Category = ShopCategory.PortalEffect, UnlockType = UnlockType.AutoTier,  RequiredTier = 3,  TierName = "Bronze",    IconEmoji = "🟤" },
            new ShopItem { ItemId = "portal_silver",      DisplayName = "Silver Portal",  Description = "Shimmering silver energy swirls in the portal.",     Category = ShopCategory.PortalEffect, UnlockType = UnlockType.AutoTier,  RequiredTier = 7,  TierName = "Silver",    IconEmoji = "🔘" },
            new ShopItem { ItemId = "portal_gold",       DisplayName = "Gold Portal",    Description = "A royal golden gateway marks your passage.",            Category = ShopCategory.PortalEffect, UnlockType = UnlockType.AutoTier,  RequiredTier = 12, TierName = "Gold",      IconEmoji = "✨" },
            new ShopItem { ItemId = "portal_celestial",  DisplayName = "Celestial Portal",Description = "Divine light pours from this transcendent gate.",    Category = ShopCategory.PortalEffect, UnlockType = UnlockType.Purchase, Cost = 2500, IconEmoji = "🌟" },

            // === FAREWELL FX (purchased with points) ===
            new ShopItem { ItemId = "farewell_sparkle",  DisplayName = "Sparkle Farewell",Description="A gentle cascade of sparkles on prestige reset.",    Category = ShopCategory.FarewellFx, UnlockType = UnlockType.Purchase, Cost = 300,  IconEmoji = "✨" },
            new ShopItem { ItemId = "farewell_firework", DisplayName = "Firework Farewell",Description="Celebrate each reset with a burst of fireworks.",   Category = ShopCategory.FarewellFx, UnlockType = UnlockType.Purchase, Cost = 800,  IconEmoji = "🎆" },
            new ShopItem { ItemId = "farewell_meteor",   DisplayName = "Meteor Farewell", Description="A blazing meteor shower marks your ascension.",       Category = ShopCategory.FarewellFx, UnlockType = UnlockType.Purchase, Cost = 1500, IconEmoji = "☄️" },
        };

        public static List<ShopItem> GetByCategory(ShopCategory category)
        {
            var result = new List<ShopItem>();
            foreach (var item in AllItems)
                if (item.Category == category) result.Add(item);
            return result;
        }

        public static ShopItem GetById(string itemId)
        {
            foreach (var item in AllItems)
                if (item.ItemId == itemId) return item;
            return null;
        }
    }
}
