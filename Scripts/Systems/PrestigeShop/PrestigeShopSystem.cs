using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Systems.PrestigeShop
{
    /// <summary>
    /// Prestige Shop core system - manages items, purchases, and unlocks.
    /// Purely cosmetic meta-progression, zero gameplay balance impact.
    /// </summary>
    public partial class PrestigeShopSystem : BaseSystem
    {
        public static PrestigeShopSystem Instance { get; private set; }

        // Prestige data reference (lazy load to avoid circular deps)
        private PrestigeSystem _prestigeSystem;

        // Unlocked items: itemId -> state
        private Dictionary<string, ShopItemState> _unlockedItems = new Dictionary<string, ShopItemState>();

        // Signal definitions
        [Signal]
        public delegate void ItemUnlockedEventHandler(string itemId, ShopItem item);

        [Signal]
        public delegate void ItemPurchasedEventHandler(string itemId, ShopItem item, int cost);

        [Signal]
        public delegate void TierAutoUnlockedEventHandler(string itemId, int tierLevel, string tierName);

        public override void _Ready()
        {
            Instance = this;
            Initialize();
            GD.Print("=== PrestigeShop System Initialized ===");
        }

        private void Initialize()
        {
            _prestigeSystem = PrestigeSystem.Instance;

            // Initialize all items as locked
            foreach (var item in PrestigeShopDatabase.AllItems)
            {
                if (!_unlockedItems.ContainsKey(item.ItemId))
                {
                    _unlockedItems[item.ItemId] = new ShopItemState
                    {
                        ItemId = item.ItemId,
                        Unlocked = false,
                        Purchased = false,
                        PurchasedAtTier = 0
                    };
                }
            }

            // Sync already-unlocked titles to TitleSystem after save load
            SyncUnlockedTitlesToTitleSystem();
        }

        private void SyncUnlockedTitlesToTitleSystem()
        {
            try
            {
                var titleSystem = TitleSystem.Instance;
                if (titleSystem == null) return;

                foreach (var kvp in _unlockedItems)
                {
                    if (!kvp.Value.Unlocked) continue;
                    var item = PrestigeShopDatabase.GetById(kvp.Key);
                    if (item == null || item.Category != ShopCategory.Title) continue;

                    var rarity = item.UnlockType == UnlockType.AutoTier
                        ? TitleRarity.Legendary
                        : TitleRarity.Epic;
                    titleSystem.RegisterPrestigeTitle(item.ItemId, item.DisplayName, item.Description, rarity);
                }
                GD.Print("[PrestigeShop] Synced unlocked titles to TitleSystem");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PrestigeShop] Failed to sync titles to TitleSystem: {ex.Message}");
            }
        }

        /// <summary>
        /// Called when prestige tier changes. Auto-unlocks tier-based items.
        /// </summary>
        public void OnPrestigeTierChanged(int newTierLevel, string tierName)
        {
            int unlockedCount = 0;
            foreach (var item in PrestigeShopDatabase.AllItems)
            {
                if (item.UnlockType != UnlockType.AutoTier)
                    continue;
                if (item.RequiredTier > newTierLevel)
                    continue;
                if (IsUnlocked(item.ItemId))
                    continue;

                UnlockAutoTierItem(item, newTierLevel);
                unlockedCount++;
            }

            if (unlockedCount > 0)
            {
                EmitSignal(SignalName.TierAutoUnlocked, tierName, newTierLevel);
            }
        }

        private void UnlockAutoTierItem(ShopItem item, int tierLevel)
        {
            var state = _unlockedItems[item.ItemId];
            state.Unlocked = true;
            state.Purchased = false; // Auto-unlock
            state.PurchasedAtTier = tierLevel;
            EmitSignal(SignalName.ItemUnlocked, item.ItemId, item);
            GD.Print($"[PrestigeShop] Auto-unlocked '{item.ItemId}' at tier {tierLevel} ({item.TierName})");

            // Integrate with TitleSystem for Title category items
            if (item.Category == ShopCategory.Title)
            {
                TryRegisterPrestigeTitle(item);
            }
        }

        private void TryRegisterPrestigeTitle(ShopItem item)
        {
            try
            {
                var titleSystem = TitleSystem.Instance;
                if (titleSystem == null) return;

                // Auto-tier titles are legendary-tier, purchase titles are epic-tier
                var rarity = item.UnlockType == UnlockType.AutoTier
                    ? TitleRarity.Legendary
                    : TitleRarity.Epic;

                titleSystem.RegisterPrestigeTitle(item.ItemId, item.DisplayName, item.Description, rarity);
                GD.Print($"[PrestigeShop] Registered prestige title '{item.DisplayName}' in TitleSystem");
            }
            catch (Exception ex)
            {
                GD.PrintErr($"[PrestigeShop] Failed to register prestige title '{item.ItemId}' in TitleSystem: {ex.Message}");
            }
        }

        /// <summary>
        /// Attempt to purchase an item with prestige points.
        /// Returns true if purchase succeeded.
        /// </summary>
        public bool PurchaseItem(string itemId)
        {
            if (_prestigeSystem == null)
                _prestigeSystem = PrestigeSystem.Instance;

            var item = PrestigeShopDatabase.GetById(itemId);
            if (item == null)
            {
                GD.PrintErr($"[PrestigeShop] Purchase failed: item '{itemId}' not found");
                return false;
            }

            if (item.UnlockType != UnlockType.Purchase)
            {
                GD.PrintErr($"[PrestigeShop] Purchase failed: item '{itemId}' is not purchasable");
                return false;
            }

            if (IsUnlocked(itemId))
            {
                GD.Print($"[PrestigeShop] Item '{itemId}' already unlocked");
                return false;
            }

            if (!_prestigeSystem.CanPurchase(item.Cost))
            {
                GD.Print($"[PrestigeShop] Not enough prestige points for '{itemId}' (need {item.Cost}, have {_prestigeSystem.PrestigePoints})");
                return false;
            }

            // Deduct points
            bool deducted = _prestigeSystem.SpendPrestigePoints(item.Cost);
            if (!deducted)
            {
                GD.PrintErr($"[PrestigeShop] Points deduction failed for '{itemId}'");
                return false;
            }

            // Mark as unlocked (purchased)
            var state = _unlockedItems[itemId];
            state.Unlocked = true;
            state.Purchased = true;
            state.PurchasedAtTier = _prestigeSystem.PrestigeLevel;

            EmitSignal(SignalName.ItemPurchased, itemId, item, item.Cost);

            // Integrate with TitleSystem for Title category items
            if (item.Category == ShopCategory.Title)
            {
                TryRegisterPrestigeTitle(item);
            }
            GD.Print($"[PrestigeShop] Purchased '{itemId}' for {item.Cost} points. Remaining: {_prestigeSystem.PrestigePoints}");
            return true;
        }

        /// <summary>
        /// Check if an item is unlocked
        /// </summary>
        public bool IsUnlocked(string itemId)
        {
            if (_unlockedItems.TryGetValue(itemId, out var state))
                return state.Unlocked;
            return false;
        }

        /// <summary>
        /// Check if an item was purchased (vs auto-unlocked)
        /// </summary>
        public bool IsPurchased(string itemId)
        {
            if (_unlockedItems.TryGetValue(itemId, out var state))
                return state.Purchased;
            return false;
        }

        /// <summary>
        /// Get all unlocked items
        /// </summary>
        public List<string> GetUnlockedItemIds()
        {
            var result = new List<string>();
            foreach (var kvp in _unlockedItems)
                if (kvp.Value.Unlocked) result.Add(kvp.Key);
            return result;
        }

        /// <summary>
        /// Get current prestige points (from PrestigeSystem)
        /// </summary>
        public int GetPrestigePoints()
        {
            if (_prestigeSystem == null)
                _prestigeSystem = PrestigeSystem.Instance;
            return _prestigeSystem?.PrestigePoints ?? 0;
        }

        /// <summary>
        /// Get current prestige tier level
        /// </summary>
        public int GetPrestigeLevel()
        {
            if (_prestigeSystem == null)
                _prestigeSystem = PrestigeSystem.Instance;
            return _prestigeSystem?.PrestigeLevel ?? 0;
        }

        /// <summary>
        /// Get current prestige tier name
        /// </summary>
        public string GetPrestigeTierName()
        {
            if (_prestigeSystem == null)
                _prestigeSystem = PrestigeSystem.Instance;
            return _prestigeSystem?.GetPrestigeTierName() ?? "None";
        }

        // ===== Persistence =====

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            var itemsData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var kvp in _unlockedItems)
            {
                itemsData[kvp.Key] = new Dictionary<string, object>
                {
                    ["unlocked"] = kvp.Value.Unlocked,
                    ["purchased"] = kvp.Value.Purchased,
                    ["purchased_at_tier"] = kvp.Value.PurchasedAtTier
                };
            }
            data["unlocked_items"] = itemsData;
            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.ContainsKey("unlocked_items"))
            {
                var itemsData = (Dictionary<string, object>)data["unlocked_items"];
                foreach (var kvp in itemsData)
                {
                    var itemData = (Dictionary<string, object>)kvp.Value;
                    if (_unlockedItems.TryGetValue(kvp.Key, out var state))
                    {
                        state.Unlocked = itemData.ContainsKey("unlocked") && (bool)itemData["unlocked"];
                        state.Purchased = itemData.ContainsKey("purchased") && (bool)itemData["purchased"];
                        state.PurchasedAtTier = itemData.ContainsKey("purchased_at_tier") ? Convert.ToInt32(itemData["purchased_at_tier"]) : 0;
                    }
                }
            }

            // Re-check auto-unlocks after loading tier data (in case tier rose while item was missed)
            if (_prestigeSystem == null)
                _prestigeSystem = PrestigeSystem.Instance;
            int tier = _prestigeSystem?.PrestigeLevel ?? 0;
            foreach (var item in PrestigeShopDatabase.AllItems)
            {
                if (item.UnlockType == UnlockType.AutoTier && !IsUnlocked(item.ItemId) && tier >= item.RequiredTier)
                {
                    UnlockAutoTierItem(item, tier);
                }
            }

            GD.Print($"[PrestigeShop] Data loaded. Unlocked: {GetUnlockedItemIds().Count} items");
        }
    }
}
