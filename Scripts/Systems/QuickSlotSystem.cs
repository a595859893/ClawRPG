using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Quick slot system for fast item access using number keys
    /// </summary>
    public class QuickSlotSystem : Node {
        public static QuickSlotSystem Instance { get; private set; }
        
        // 9 quick slots (1-9 keys)
        public const int SlotCount = 9;
        
        // Each slot holds an item ID and quantity
        private string[] _slotItemIds = new string[SlotCount];
        private int[] _slotQuantities = new int[SlotCount];
        
        // Signals
        public Action<int, string, int> OnSlotUpdated;
        public Action<int> OnSlotUsed;
        
        public override void _Ready() {
            Instance = this;
        }
        
        public override void _Input(InputEvent @event) {
            // Handle number keys 1-9 for quick slot usage
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo) {
                int slotIndex = -1;
                
                if (keyEvent.Scancode == Godot.KeyList.Key1) slotIndex = 0;
                else if (keyEvent.Scancode == Godot.KeyList.Key2) slotIndex = 1;
                else if (keyEvent.Scancode == Godot.KeyList.Key3) slotIndex = 2;
                else if (keyEvent.Scancode == Godot.KeyList.Key4) slotIndex = 3;
                else if (keyEvent.Scancode == Godot.KeyList.Key5) slotIndex = 4;
                else if (keyEvent.Scancode == Godot.KeyList.Key6) slotIndex = 5;
                else if (keyEvent.Scancode == Godot.KeyList.Key7) slotIndex = 6;
                else if (keyEvent.Scancode == Godot.KeyList.Key8) slotIndex = 7;
                else if (keyEvent.Scancode == Godot.KeyList.Key9) slotIndex = 8;
                
                if (slotIndex >= 0) {
                    UseSlot(slotIndex);
                }
            }
        }
        
        /// <summary>
        /// Assign an item to a quick slot
        /// </summary>
        public void SetSlot(int slotIndex, string itemId, int quantity) {
            if (slotIndex < 0 || slotIndex >= SlotCount) return;
            
            _slotItemIds[slotIndex] = itemId;
            _slotQuantities[slotIndex] = quantity;
            OnSlotUpdated?.Invoke(slotIndex, itemId, quantity);
        }
        
        /// <summary>
        /// Clear a quick slot
        /// </summary>
        public void ClearSlot(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= SlotCount) return;
            
            _slotItemIds[slotIndex] = "";
            _slotQuantities[slotIndex] = 0;
            OnSlotUpdated?.Invoke(slotIndex, "", 0);
        }
        
        /// <summary>
        /// Get the item ID in a slot
        /// </summary>
        public string GetSlotItemId(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= SlotCount) return "";
            return _slotItemIds[slotIndex];
        }
        
        /// <summary>
        /// Get the quantity in a slot
        /// </summary>
        public int GetSlotQuantity(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= SlotCount) return 0;
            return _slotQuantities[slotIndex];
        }
        
        /// <summary>
        /// Use an item in a quick slot
        /// </summary>
        public void UseSlot(int slotIndex) {
            if (slotIndex < 0 || slotIndex >= SlotCount) return;
            if (string.IsNullOrEmpty(_slotItemIds[slotIndex]) || _slotQuantities[slotIndex] <= 0) return;
            
            var item = ItemDatabase.Instance.GetItem(_slotItemIds[slotIndex]);
            if (item == null) return;
            
            // Only consumables can be used from quick slots
            if (item.Type != ItemType.Consumable) {
                return;
            }
            
            var player = GetTree().GetFirstNodeInGroup("Player") as Player;
            if (player == null) return;
            
            // Apply item effect
            bool used = false;
            
            switch (item.ConsumableType) {
                case ConsumableType.HealthPotion:
                    int healAmount = item.Value * 10; // Heal value based on item
                    player.Heal(healAmount);
                    used = true;
                    break;
                    
                case ConsumableType.ManaPotion:
                    // Add mana restoration if Player has Mana property
                    used = true;
                    break;
                    
                case ConsumableType.StrengthPotion:
                    player.ApplyStatusEffect(StatusEffectType.Buff, "strength_boost", 60f);
                    used = true;
                    break;
                    
                case ConsumableType.DefensePotion:
                    player.ApplyStatusEffect(StatusEffectType.Buff, "defense_boost", 60f);
                    used = true;
                    break;
            }
            
            if (used) {
                _slotQuantities[slotIndex]--;
                OnSlotUsed?.Invoke(slotIndex);
                
                // Remove item if quantity is 0
                if (_slotQuantities[slotIndex] <= 0) {
                    ClearSlot(slotIndex);
                } else {
                    OnSlotUpdated?.Invoke(slotIndex, _slotItemIds[slotIndex], _slotQuantities[slotIndex]);
                }
                
                // Remove from inventory
                if (InventoryManager.Instance != null) {
                    InventoryManager.Instance.RemoveItem(_slotItemIds[slotIndex], 1);
                }
                
                // Show feedback
                if (ScreenFlashEffect.Instance != null) {
                    ScreenFlashEffect.Instance.Flash(Color.Green, 0.2f);
                }
                
                if (GameMessageSystem.Instance != null) {
                    GameMessageSystem.Instance.ShowPositive($"使用 {item.Name}");
                }
            }
        }
        
        /// <summary>
        /// Auto-fill quick slots with consumables from inventory
        /// </summary>
        public void AutoFillSlots() {
            if (InventoryManager.Instance == null) return;
            
            var consumables = InventoryManager.Instance.GetItemsByType(ItemType.Consumable);
            int slotIndex = 0;
            
            foreach (var item in consumables) {
                if (slotIndex >= SlotCount) break;
                
                SetSlot(slotIndex, item.Id, item.Quantity);
                slotIndex++;
            }
        }
        
        /// <summary>
        /// Serialize quick slot data for saving
        /// </summary>
        public Dictionary<string, object> Serialize() {
            var data = new Dictionary<string, object>();
            
            for (int i = 0; i < SlotCount; i++) {
                data[$"slot_{i}_item"] = _slotItemIds[i] ?? "";
                data[$"slot_{i}_qty"] = _slotQuantities[i];
            }
            
            return data;
        }
        
        /// <summary>
        /// Deserialize and load quick slot data
        /// </summary>
        public void Deserialize(Dictionary<string, object> data) {
            for (int i = 0; i < SlotCount; i++) {
                string itemId = data.ContainsKey($"slot_{i}_item") ? (string)data[$"slot_{i}_item"] : "";
                int qty = data.ContainsKey($"slot_{i}_qty") ? (int)data[$"slot_{i}_qty"] : 0;
                
                if (!string.IsNullOrEmpty(itemId) && qty > 0) {
                    _slotItemIds[i] = itemId;
                    _slotQuantities[i] = qty;
                } else {
                    _slotItemIds[i] = "";
                    _slotQuantities[i] = 0;
                }
            }
            
            // Notify UI to update
            for (int i = 0; i < SlotCount; i++) {
                OnSlotUpdated?.Invoke(i, _slotItemIds[i], _slotQuantities[i]);
            }
        }
    }
}
