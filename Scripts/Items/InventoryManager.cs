using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework;

namespace ClawRPG.Scripts.Items
{
    /// <summary>
    /// Inventory filter types
    /// </summary>
    public enum InventoryFilter
    {
        All,
        Weapon,
        Armor,
        Accessory,
        Consumable,
        Material,
        QuestItem
    }

    /// <summary>
    /// Inventory sort types
    /// </summary>
    public enum InventorySort
    {
        None,
        Name,
        Type,
        Value,
        Quality
    }

    /// <summary>
    /// Enhanced Inventory Manager with filtering, sorting and search
    /// </summary>
    public class InventoryManager : BaseSystem
    {
        public static InventoryManager Instance { get; private set; }

        [Signal]
        public signal_void InventoryUpdated;

        [Signal]
        public signal_void ItemUsed;

        [Export]
        public int MaxSlots { get; set; } = 30;

        private List<InventorySlot> _slots = new();
        private InventoryFilter _currentFilter = InventoryFilter.All;
        private InventorySort _currentSort = InventorySort.None;
        private string _searchQuery = "";

        public override void _Ready()
        {
            Instance = this;
            InitializeSlots();
        }

        private void InitializeSlots()
        {
            _slots.Clear();
            for (int i = 0; i < MaxSlots; i++)
            {
                _slots.Add(new InventorySlot { Index = i });
            }
        }

        /// <summary>
        /// Add item to inventory
        /// </summary>
        public bool AddItem(int itemId, int quantity = 1)
        {
            var item = ItemDatabase.Instance.GetItem(itemId);
            if (item == null) return false;

            // Try to stack with existing items
            if (item.MaxStack > 1)
            {
                foreach (var slot in _slots)
                {
                    if (slot.ItemId == itemId && slot.Quantity < item.MaxStack)
                    {
                        int space = item.MaxStack - slot.Quantity;
                        int toAdd = Math.Min(space, quantity);
                        slot.Quantity += toAdd;
                        quantity -= toAdd;
                        if (quantity <= 0)
                        {
                            EmitSignal(SignalName.InventoryUpdated);
                            return true;
                        }
                    }
                }
            }

            // Add to empty slot
            foreach (var slot in _slots)
            {
                if (slot.IsEmpty)
                {
                    slot.ItemId = itemId;
                    slot.Quantity = Math.Min(quantity, item.MaxStack);
                    quantity = 0;
                    if (quantity <= 0) break;
                }
            }

            EmitSignal(SignalName.InventoryUpdated);
            return quantity == 0;
        }

        /// <summary>
        /// Remove item from inventory
        /// </summary>
        public bool RemoveItem(int itemId, int quantity = 1)
        {
            foreach (var slot in _slots)
            {
                if (slot.ItemId == itemId)
                {
                    if (slot.Quantity >= quantity)
                    {
                        slot.Quantity -= quantity;
                        if (slot.Quantity <= 0)
                        {
                            slot.ItemId = 0;
                            slot.Quantity = 0;
                        }
                        EmitSignal(SignalName.InventoryUpdated);
                        return true;
                    }
                    else
                    {
                        quantity -= slot.Quantity;
                        slot.ItemId = 0;
                        slot.Quantity = 0;
                    }
                }
            }
            EmitSignal(SignalName.InventoryUpdated);
            return false;
        }

        /// <summary>
        /// Get filtered and sorted inventory slots
        /// </summary>
        public List<InventorySlot> GetFilteredSlots()
        {
            var filtered = _slots.Where(s => !s.IsEmpty).ToList();

            // Apply filter
            if (_currentFilter != InventoryFilter.All)
            {
                filtered = filtered.Where(s =>
                {
                    var item = ItemDatabase.Instance.GetItem(s.ItemId);
                    if (item == null) return false;
                    return item.Type == (Item.ItemType)_currentFilter;
                }).ToList();
            }

            // Apply search
            if (!string.IsNullOrWhiteSpace(_searchQuery))
            {
                filtered = filtered.Where(s =>
                {
                    var item = ItemDatabase.Instance.GetItem(s.ItemId);
                    if (item == null) return false;
                    return item.Name.Contains(_searchQuery, StringComparison.OrdinalIgnoreCase);
                }).ToList();
            }

            // Apply sort
            filtered = _currentSort switch
            {
                InventorySort.Name => filtered.OrderBy(s => ItemDatabase.Instance.GetItem(s.ItemId)?.Name ?? "").ToList(),
                InventorySort.Type => filtered.OrderBy(s => ItemDatabase.Instance.GetItem(s.ItemId)?.Type).ToList(),
                InventorySort.Value => filtered.OrderBy(s => ItemDatabase.Instance.GetItem(s.ItemId)?.Value ?? 0).ToList(),
                InventorySort.Quality => filtered.OrderBy(s => (int)(ItemDatabase.Instance.GetItem(s.ItemId)?.Quality ?? ItemQuality.Common)).ToList(),
                _ => filtered
            };

            return filtered;
        }

        /// <summary>
        /// Set inventory filter
        /// </summary>
        public void SetFilter(InventoryFilter filter)
        {
            _currentFilter = filter;
            EmitSignal(SignalName.InventoryUpdated);
        }

        /// <summary>
        /// Set inventory sort
        /// </summary>
        public void SetSort(InventorySort sort)
        {
            _currentSort = sort;
            EmitSignal(SignalName.InventoryUpdated);
        }

        /// <summary>
        /// Set search query
        /// </summary>
        public void SetSearchQuery(string query)
        {
            _searchQuery = query;
            EmitSignal(SignalName.InventoryUpdated);
        }

        /// <summary>
        /// Use item from slot
        /// </summary>
        public bool UseItem(int slotIndex)
        {
            if (slotIndex < 0 || slotIndex >= _slots.Count) return false;
            var slot = _slots[slotIndex];
            if (slot.IsEmpty) return false;

            var item = ItemDatabase.Instance.GetItem(slot.ItemId);
            if (item == null) return false;

            // Only consumables can be used
            if (item is Consumable consumable)
            {
                var player = GetTree().GetFirstNodeInGroup("player") as Player;
                if (player != null)
                {
                    if (consumable.HealthRestore > 0)
                        player.Heal(consumable.HealthRestore);
                    if (consumable.ManaRestore > 0)
                        player.RestoreMana(consumable.ManaRestore);
                    if (consumable.StaminaRestore > 0)
                        player.RestoreStamina(consumable.StaminaRestore);
                }

                RemoveItem(slot.ItemId, 1);
                EmitSignal(SignalName.ItemUsed);
                return true;
            }

            return false;
        }

        /// <summary>
        /// Get all slots (unfiltered)
        /// </summary>
        public List<InventorySlot> GetAllSlots() => new(_slots);

        /// <summary>
        /// Get slot at index
        /// </summary>
        public InventorySlot GetSlot(int index) => index >= 0 && index < _slots.Count ? _slots[index] : null;

        /// <summary>
        /// Get total item count
        /// </summary>
        public int GetTotalItemCount() => _slots.Sum(s => s.Quantity);

        /// <summary>
        /// Check if inventory is full
        /// </summary>
        public bool IsFull() => _slots.All(s => !s.IsEmpty);

        /// <summary>
        /// Clear all items
        /// </summary>
        public void Clear()
        {
            foreach (var slot in _slots)
            {
                slot.ItemId = 0;
                slot.Quantity = 0;
            }
            EmitSignal(SignalName.InventoryUpdated);
        }

        /// <summary>
        /// Get item count by id
        /// </summary>
        public int GetItemCount(int itemId) => _slots.Where(s => s.ItemId == itemId).Sum(s => s.Quantity);
    }

    /// <summary>
    /// Inventory slot data
    /// </summary>
    public class InventorySlot
    {
        public int Index { get; set; }
        public int ItemId { get; set; }
        public int Quantity { get; set; }

        public bool IsEmpty => ItemId == 0 || Quantity <= 0;
    }
}
