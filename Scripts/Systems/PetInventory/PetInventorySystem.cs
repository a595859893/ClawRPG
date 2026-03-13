using Godot;
using System;
using System.Collections.Generic;

public class PetInventorySystem : Node {
    private PetInventoryData _data;
    private PetInventoryDatabase _database;
    
    // Signals
    public signal void ItemAdded(string petId, string itemId, int quantity);
    public signal void ItemRemoved(string petId, string itemId, int quantity);
    public signal void ItemUsed(string petId, string itemId, Dictionary<string, float> effects);
    public signal void InventoryFull(string petId);
    public signal void GoldChanged(string petId, int newAmount);
    
    public override void _Ready() {
        base._Ready();
        InitializeSystem();
    }
    
    private void InitializeSystem() {
        _data = new PetInventoryData();
        _data.Initialize();
        
        _database = new PetInventoryDatabase();
        _database.InitializeDatabase();
        
        GD.Print("[PetInventorySystem] System initialized with " + _data.TotalSlots + " slots per pet");
    }
    
    public bool AddItem(string petId, string itemId, int quantity = 1) {
        if (!_data.PetInventories.ContainsKey(petId)) {
            _data.PetInventories[petId] = new List<PetInventoryItem>();
        }
        
        var inventory = _data.PetInventories[petId];
        
        // Check if item already exists
        foreach (var item in inventory) {
            if (item.ItemId == itemId) {
                item.Quantity += quantity;
                _data.TotalItemsCollected += quantity;
                ItemAdded?.Invoke(petId, itemId, quantity);
                GD.Print("[PetInventory] Added " + quantity + "x " + itemId + " to pet " + petId);
                return true;
            }
        }
        
        // Check inventory capacity
        if (inventory.Count >= _data.TotalSlots) {
            InventoryFull?.Invoke(petId);
            GD.Print("[PetInventory] Inventory full for pet " + petId);
            return false;
        }
        
        // Add new item
        var template = _database.GetItemTemplate(itemId);
        if (template == null) {
            GD.PrintErr("[PetInventory] Item template not found: " + itemId);
            return false;
        }
        
        var newItem = new PetInventoryItem {
            ItemId = itemId,
            ItemName = (string)template["name"],
            Description = (string)template["description"],
            Quantity = quantity,
            Rarity = (string)template["rarity"],
            Category = (string)template["category"],
            Value = (int)template["value"],
            Stats = new Dictionary<string, float>((Dictionary<string, float>)template["stats"]),
            SpecialEffect = (string)template["special_effect"],
            AcquiredAt = DateTime.Now
        };
        
        inventory.Add(newItem);
        _data.TotalItemsCollected += quantity;
        ItemAdded?.Invoke(petId, itemId, quantity);
        GD.Print("[PetInventory] Added new item " + itemId + " to pet " + petId);
        return true;
    }
    
    public bool RemoveItem(string petId, string itemId, int quantity = 1) {
        if (!_data.PetInventories.ContainsKey(petId)) {
            return false;
        }
        
        var inventory = _data.PetInventories[petId];
        
        for (int i = 0; i < inventory.Count; i++) {
            if (inventory[i].ItemId == itemId) {
                if (inventory[i].Quantity < quantity) {
                    GD.PrintErr("[PetInventory] Not enough items: " + inventory[i].Quantity + " < " + quantity);
                    return false;
                }
                
                inventory[i].Quantity -= quantity;
                
                if (inventory[i].Quantity <= 0) {
                    inventory.RemoveAt(i);
                }
                
                ItemRemoved?.Invoke(petId, itemId, quantity);
                GD.Print("[PetInventory] Removed " + quantity + "x " + itemId + " from pet " + petId);
                return true;
            }
        }
        
        return false;
    }
    
    public bool UseItem(string petId, string itemId) {
        if (!RemoveItem(petId, itemId, 1)) {
            return false;
        }
        
        var template = _database.GetItemTemplate(itemId);
        if (template == null) {
            return false;
        }
        
        var stats = (Dictionary<string, float>)template["stats"];
        var effect = (string)template["special_effect"];
        
        // Track usage
        if (!_data.ItemUsageCount.ContainsKey(itemId)) {
            _data.ItemUsageCount[itemId] = 0;
        }
        _data.ItemUsageCount[itemId]++;
        _data.TotalItemsUsed++;
        
        // Deduct gold for consumables
        var category = (string)template["category"];
        if (category == "Consumable" || category == "Special") {
            int value = (int)template["value"];
            AddGold(petId, -value);
            _data.TotalGoldSpent += value;
        }
        
        ItemUsed?.Invoke(petId, itemId, stats);
        GD.Print("[PetInventory] Pet " + petId + " used item " + itemId + ", effect: " + effect);
        return true;
    }
    
    public List<PetInventoryItem> GetInventory(string petId) {
        if (!_data.PetInventories.ContainsKey(petId)) {
            _data.PetInventories[petId] = new List<PetInventoryItem>();
        }
        return _data.PetInventories[petId];
    }
    
    public PetInventoryItem GetItem(string petId, string itemId) {
        var inventory = GetInventory(petId);
        foreach (var item in inventory) {
            if (item.ItemId == itemId) {
                return item;
            }
        }
        return null;
    }
    
    public int GetItemCount(string petId, string itemId) {
        var item = GetItem(petId, itemId);
        return item != null ? item.Quantity : 0;
    }
    
    public bool HasItem(string petId, string itemId) {
        return GetItemCount(petId, itemId) > 0;
    }
    
    public int GetGold(string petId) {
        if (!_data.GoldByPet.ContainsKey(petId)) {
            _data.GoldByPet[petId] = 0;
        }
        return _data.GoldByPet[petId];
    }
    
    public void AddGold(string petId, int amount) {
        if (!_data.GoldByPet.ContainsKey(petId)) {
            _data.GoldByPet[petId] = 0;
        }
        
        _data.GoldByPet[petId] += amount;
        if (_data.GoldByPet[petId] < 0) {
            _data.GoldByPet[petId] = 0;
        }
        
        GoldChanged?.Invoke(petId, _data.GoldByPet[petId]);
        GD.Print("[PetInventory] Pet " + petId + " gold changed to " + _data.GoldByPet[petId]);
    }
    
    public bool SetGold(string petId, int amount) {
        if (amount < 0) return false;
        _data.GoldByPet[petId] = amount;
        GoldChanged?.Invoke(petId, amount);
        return true;
    }
    
    public int GetInventorySize(string petId) {
        return GetInventory(petId).Count;
    }
    
    public int GetMaxSlots() {
        return _data.TotalSlots;
    }
    
    public Dictionary<string, int> GetStatistics() {
        return new Dictionary<string, int> {
            { "total_items_collected", _data.TotalItemsCollected },
            { "total_items_used", _data.TotalItemsUsed },
            { "total_gold_spent", _data.TotalGoldSpent },
            { "unique_pets", _data.PetInventories.Count },
            { "total_item_types", _data.ItemUsageCount.Count }
        };
    }
    
    public Dictionary<string, int> GetItemUsageStats() {
        return new Dictionary<string, int>(_data.ItemUsageCount);
    }
    
    public Dictionary<string, object> GetPetInventorySummary(string petId) {
        var inventory = GetInventory(petId);
        int totalValue = 0;
        Dictionary<string, int> categoryCount = new Dictionary<string, int>();
        Dictionary<string, int> rarityCount = new Dictionary<string, int>();
        
        foreach (var item in inventory) {
            float rarityMultiplier = _database.GetRarityMultiplier(item.Rarity);
            totalValue += (int)(item.Value * rarityMultiplier * item.Quantity);
            
            if (!categoryCount.ContainsKey(item.Category)) {
                categoryCount[item.Category] = 0;
            }
            categoryCount[item.Category] += item.Quantity;
            
            if (!rarityCount.ContainsKey(item.Rarity)) {
                rarityCount[item.Rarity] = 0;
            }
            rarityCount[item.Rarity]++;
        }
        
        return new Dictionary<string, object> {
            { "pet_id", petId },
            { "item_count", inventory.Count },
            { "total_value", totalValue },
            { "gold", GetGold(petId) },
            { "category_count", categoryCount },
            { "rarity_count", rarityCount }
        };
    }
    
    public void ClearInventory(string petId) {
        if (_data.PetInventories.ContainsKey(petId)) {
            _data.PetInventories[petId].Clear();
            GD.Print("[PetInventory] Cleared inventory for pet " + petId);
        }
    }
    
    public Dictionary<string, List<string>> GetCategories() {
        return _database.CategoryItems;
    }
}
