using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Manages loot filtering logic for the inventory and loot drop UI.
    /// Applies filter rules against items and returns filtered results.
    /// Supports AND-logic across dimensions, OR-logic within a dimension.
    /// </summary>
    public partial class InventoryFilterSystem : BaseSystem
    {
        /// <summary>
        /// Gets the singleton instance.
        /// </summary>
        public static InventoryFilterSystem Instance { get; private set; }

        /// <summary>
        /// Current filter state.
        /// </summary>
        public LootFilterData FilterData { get; private set; }

        /// <summary>
        /// Signals that the filter has changed and UI should refresh.
        /// </summary>
        public Action OnFilterChanged;

        /// <summary>
        /// Signals that a new filter preset was applied.
        /// </summary>
        public Action<string> OnPresetApplied;

        /// <summary>
        /// Cached last filtered result to avoid re-filtering on every frame.
        /// </summary>
        private List<ItemData> _cachedFilteredItems = new List<ItemData>();

        /// <summary>
        /// Whether the cache is stale and needs refresh.
        /// </summary>
        private bool _cacheDirty = true;

        public override void _Ready()
        {
            Instance = this;
            base._Ready();
            InitializeFilter();
        }

        /// <summary>
        /// Initializes the filter data, loading from resource or creating defaults.
        /// </summary>
        private void InitializeFilter()
        {
            FilterData = new LootFilterData();
            FilterData.CreateDefaultPresets();
            GD.Print("[InventoryFilterSystem] Initialized with ", FilterData.Presets.Count, " presets");
        }

        /// <summary>
        /// Returns all active filter rules (preset or custom).
        /// </summary>
        private List<FilterRule> GetActiveRules()
        {
            return FilterData.GetActiveRules();
        }

        /// <summary>
        /// Checks if a single item passes a single filter rule.
        /// </summary>
        private bool ItemMatchesRule(ItemData item, FilterRule rule)
        {
            if (!rule.Enabled)
                return true;

            switch (rule.Dimension)
            {
                case FilterDimension.Rarity:
                    return MatchesRarity(item, rule.Value);

                case FilterDimension.Type:
                    return MatchesType(item, rule.Value);

                case FilterDimension.Attribute:
                    return MatchesAttribute(item, rule.Value);

                case FilterDimension.Source:
                    return MatchesSource(item, rule.Value);

                default:
                    return true;
            }
        }

        private bool MatchesRarity(ItemData item, string rarityValue)
        {
            // ItemData uses its own rarity naming; map from LootRarity strings
            string itemRarity = GetItemRarityString(item);
            return itemRarity.Equals(rarityValue, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesType(ItemData item, string typeValue)
        {
            string itemType = GetItemTypeString(item);
            return itemType.Equals(typeValue, StringComparison.OrdinalIgnoreCase);
        }

        private bool MatchesAttribute(ItemData item, string attrValue)
        {
            // Attribute affinity: check item's primary stat or tags
            if (item is EquipmentData equip)
            {
                string primaryAttr = GetPrimaryAttributeString(equip);
                return primaryAttr.Equals(attrValue, StringComparison.OrdinalIgnoreCase);
            }
            return false;
        }

        private bool MatchesSource(ItemData item, string sourceValue)
        {
            // Source tracking would need item to carry source metadata
            // For now, always return true (source filter needs additional item fields)
            return true;
        }

        private string GetItemRarityString(ItemData item)
        {
            // Map ItemData.Rarity (likely enum) to LootFilterData LootRarity strings
            // Assuming ItemData has a Rarity property that returns an enum
            var rarity = item.GetType().GetProperty("Rarity");
            if (rarity != null)
            {
                var value = rarity.GetValue(item)?.ToString() ?? "";
                // Normalize: Common→Common, Uncommon→Uncommon, Rare→Rare, Epic→Epic, Legendary→Legendary
                return value;
            }
            return "Common";
        }

        private string GetItemTypeString(ItemData item)
        {
            // Map to LootDropData.LootType
            if (item is EquipmentData)
                return "Equipment";
            if (item is MaterialData)
                return "Material";
            if (item is CurrencyItemData)
                return "Currency";
            return "Item";
        }

        private string GetPrimaryAttributeString(EquipmentData equip)
        {
            // Heuristic: check which stat is highest to determine affinity
            // Strength→力量型, Intelligence→智力型, Dexterity→敏捷型
            var strProp = equip.GetType().GetProperty("StrengthBonus");
            var intProp = equip.GetType().GetProperty("IntelligenceBonus");
            var dexProp = equip.GetType().GetProperty("DexterityBonus");

            float str = strProp != null ? Convert.ToSingle(strProp.GetValue(equip) ?? 0f) : 0f;
            float intel = intProp != null ? Convert.ToSingle(intProp.GetValue(equip) ?? 0f) : 0f;
            float dex = dexProp != null ? Convert.ToSingle(dexProp.GetValue(equip) ?? 0f) : 0f;

            if (str >= intel && str >= dex)
                return "力量型";
            if (intel >= str && intel >= dex)
                return "智力型";
            if (dex >= str && dex >= intel)
                return "敏捷型";
            return "力量型";
        }

        /// <summary>
        /// Returns true if the item passes all active filter rules (AND across dimensions).
        /// </summary>
        public bool ItemPassesFilter(ItemData item)
        {
            var rules = GetActiveRules();
            foreach (var rule in rules)
            {
                if (!ItemMatchesRule(item, rule))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Applies the current filter to a list of items and returns the filtered list.
        /// </summary>
        public List<ItemData> ApplyFilter(List<ItemData> items)
        {
            var result = new List<ItemData>();
            foreach (var item in items)
            {
                if (ItemPassesFilter(item))
                    result.Add(item);
            }
            _cachedFilteredItems = result;
            _cacheDirty = false;
            return result;
        }

        /// <summary>
        /// Applies a named preset by id.
        /// </summary>
        public void ApplyPreset(string presetId)
        {
            FilterData.ActivePresetId = presetId;
            _cacheDirty = true;
            OnPresetApplied?.Invoke(presetId);
            OnFilterChanged?.Invoke();
            GD.Print($"[InventoryFilterSystem] Applied preset: {presetId}");
        }

        /// <summary>
        /// Switches to custom (non-preset) filter mode.
        /// </summary>
        public void UseCustomRules()
        {
            FilterData.ActivePresetId = null;
            _cacheDirty = true;
            OnFilterChanged?.Invoke();
        }

        /// <summary>
        /// Toggles a custom rule on or off. Creates the rule if it doesn't exist.
        /// </summary>
        public void ToggleCustomRule(FilterDimension dimension, string value, string label, bool enabled)
        {
            var rules = FilterData.CustomRules;

            // Find existing rule
            foreach (var rule in rules)
            {
                if (rule.Dimension == dimension && rule.Value == value)
                {
                    rule.Enabled = enabled;
                    _cacheDirty = true;
                    OnFilterChanged?.Invoke();
                    return;
                }
            }

            // Create new rule
            if (enabled)
            {
                rules.Add(new FilterRule
                {
                    Dimension = dimension,
                    Value = value,
                    Label = label,
                    Enabled = true
                });
                _cacheDirty = true;
                OnFilterChanged?.Invoke();
            }
        }

        /// <summary>
        /// Returns how many items in the total set are hidden by the current filter.
        /// </summary>
        public int GetHiddenCount(List<ItemData> allItems)
        {
            return allItems.Count - ApplyFilter(allItems).Count;
        }

        /// <summary>
        /// Returns all available preset ids and names.
        /// </summary>
        public List<(string Id, string Name)> GetAvailablePresets()
        {
            var result = new List<(string, string)>();
            foreach (var p in FilterData.Presets)
                result.Add((p.Id, p.Name));
            return result;
        }

        /// <summary>
        /// Clears all custom rules.
        /// </summary>
        public void ClearCustomRules()
        {
            FilterData.CustomRules.Clear();
            _cacheDirty = true;
            OnFilterChanged?.Invoke();
        }

        /// <summary>
        /// Marks the filter cache as dirty (call when inventory changes).
        /// </summary>
        public void InvalidateCache()
        {
            _cacheDirty = true;
        }
    }
}
