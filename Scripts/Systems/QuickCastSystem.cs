using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 快速施法槽项目数据。
/// </summary>
public class QuickCastItem
{
    /// <summary>物品ID</summary>
    public string ItemId { get; set; }

    /// <summary>物品名称</summary>
    public string ItemName { get; set; }

    /// <summary>槽位索引</summary>
    public int SlotIndex { get; set; }

    /// <summary>是否已分配物品</summary>
    public bool IsAssigned { get; set; }

    /// <summary>物品数量</summary>
    public int Quantity { get; set; }

    /// <summary>剩余冷却时间（秒）</summary>
    public float CooldownRemaining { get; set; }

    /// <summary>冷却总时间（秒）</summary>
    public float CooldownTime { get; set; }
    
    public QuickCastItem()
    {
        ItemId = "";
        ItemName = "";
        SlotIndex = -1;
        IsAssigned = false;
        Quantity = 0;
        CooldownRemaining = 0f;
        CooldownTime = 0f;
    }
}

/// <summary>
/// 快速施法系统。允许玩家通过数字键快速使用物品和技能。
/// </summary>
public partial class QuickCastSystem : BaseSystem
{
    /// <summary>
    /// 获取系统单例实例。
    /// </summary>
    public static QuickCastSystem Instance { get; private set; }
    
    [Export] public int MaxSlots = 9;
    [Export] public float GlobalCooldown = 0.5f;
    
    private List<QuickCastItem> _quickSlots = new List<QuickCastItem>();
    private float _globalCooldownRemaining = 0f;
    private Player _player;
    private RandomNumberGenerator _rng = new RandomNumberGenerator();
    
    // Statistics
    private int _totalCasts = 0;
    private int _successfulCasts = 0;
    private Dictionary<string, int> _itemUsageCount = new Dictionary<string, int>();
    
    public override void _Ready()
    {
        Instance = this;
        InitializeSlots();
        
        // Get player reference
        _player = GetTree().GetFirstNodeInGroup("Player") as Player;
        
        // Load saved data
        LoadQuickCastData();
    }
    
    private void InitializeSlots()
    {
        for (int i = 0; i < MaxSlots; i++)
        {
            _quickSlots.Add(new QuickCastItem { SlotIndex = i });
        }
    }
    
    public override void _Process(float delta)
    {
        // Update global cooldown
        if (_globalCooldownRemaining > 0)
        {
            _globalCooldownRemaining -= delta;
            if (_globalCooldownRemaining < 0) _globalCooldownRemaining = 0;
        }
        
        // Update individual slot cooldowns
        foreach (var slot in _quickSlots)
        {
            if (slot.CooldownRemaining > 0)
            {
                slot.CooldownRemaining -= delta;
                if (slot.CooldownRemaining < 0) slot.CooldownRemaining = 0;
            }
        }
    }
    
    public override void _Input(InputEvent eventArgs)
    {
        if (eventArgs is InputEventKey keyEvent && keyEvent.Pressed)
        {
            int slotIndex = -1;
            
            // Map number keys 1-9 to slots 0-8
            if (keyEvent.Keycode == Key.Key1) slotIndex = 0;
            else if (keyEvent.Keycode == Key.Key2) slotIndex = 1;
            else if (keyEvent.Keycode == Key.Key3) slotIndex = 2;
            else if (keyEvent.Keycode == Key.Key4) slotIndex = 3;
            else if (keyEvent.Keycode == Key.Key5) slotIndex = 4;
            else if (keyEvent.Keycode == Key.Key6) slotIndex = 5;
            else if (keyEvent.Keycode == Key.Key7) slotIndex = 6;
            else if (keyEvent.Keycode == Key.Key8) slotIndex = 7;
            else if (keyEvent.Keycode == Key.Key9) slotIndex = 8;
            
            if (slotIndex >= 0 && slotIndex < MaxSlots)
            {
                UseQuickCastSlot(slotIndex);
            }
        }
    }
    
    public void AssignItemToSlot(int slotIndex, string itemId)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return;
        
        var slot = _quickSlots[slotIndex];
        
        // Get item data from ItemManager
        var itemData = ItemManager.Instance?.GetItemData(itemId);
        if (itemData == null) return;
        
        slot.ItemId = itemId;
        slot.ItemName = itemData.Name;
        slot.IsAssigned = true;
        
        // Set cooldown based on item type
        slot.CooldownTime = GetItemCooldown(itemId);
        
        SaveQuickCastData();
    }
    
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return;
        
        var slot = _quickSlots[slotIndex];
        slot.ItemId = "";
        slot.ItemName = "";
        slot.IsAssigned = false;
        slot.Quantity = 0;
        slot.CooldownRemaining = 0f;
        
        SaveQuickCastData();
    }
    
    public void UseQuickCastSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MaxSlots) return;
        
        var slot = _quickSlots[slotIndex];
        
        // Check if slot has item assigned
        if (!slot.IsAssigned || string.IsNullOrEmpty(slot.ItemId))
        {
            return;
        }
        
        // Check global cooldown
        if (_globalCooldownRemaining > 0)
        {
            return;
        }
        
        // Check slot cooldown
        if (slot.CooldownRemaining > 0)
        {
            return;
        }
        
        // Check if player has the item
        if (_player == null) return;
        
        var inventory = _player.Inventory;
        if (inventory == null) return;
        
        // Find item in inventory
        int itemIndex = -1;
        for (int i = 0; i < inventory.Count; i++)
        {
            if (inventory[i].Id == slot.ItemId)
            {
                itemIndex = i;
                break;
            }
        }
        
        if (itemIndex < 0)
        {
            // Item not in inventory
            return;
        }
        
        var inventorySlot = inventory[itemIndex];
        if (inventorySlot.Quantity <= 0)
        {
            return;
        }
        
        // Use the item
        bool success = UseItem(slot.ItemId, inventorySlot.Quantity);
        
        if (success)
        {
            // Remove one item
            inventory[itemIndex].Quantity -= 1;
            if (inventory[itemIndex].Quantity <= 0)
            {
                inventory.RemoveAt(itemIndex);
            }
            
            // Set cooldowns
            _globalCooldownRemaining = GlobalCooldown;
            slot.CooldownRemaining = slot.CooldownTime;
            
            // Update statistics
            _totalCasts++;
            _successfulCasts++;
            
            if (_itemUsageCount.ContainsKey(slot.ItemId))
                _itemUsageCount[slot.ItemId]++;
            else
                _itemUsageCount[slot.ItemId] = 1;
            
            // Play sound effect
            SoundEffectManager.Instance?.PlayEffect("item_use");
            
            // Visual feedback
            ShowUseEffect(slotIndex);
            
            SaveQuickCastData();
        }
    }
    
    private bool UseItem(string itemId, int quantity)
    {
        if (_player == null) return false;
        
        var itemData = ItemManager.Instance?.GetItemData(itemId);
        if (itemData == null) return false;
        
        // Apply item effects based on type
        switch (itemData.Type)
        {
            case ItemType.Potion:
            case ItemType.HealthPotion:
            case ItemType.ManaPotion:
                // Health/Mana potions are handled by Player
                return true;
                
            case ItemType.Buff:
                // Apply buff effect
                ApplyBuffEffect(itemId);
                return true;
                
            case ItemType.Scroll:
                // Apply scroll effect
                ApplyScrollEffect(itemId);
                return true;
                
            case ItemType.Food:
                // Apply food effect
                ApplyFoodEffect(itemId);
                return true;
                
            default:
                return true;
        }
    }
    
    private void ApplyBuffEffect(string itemId)
    {
        // Get buff type from item
        var buffType = GetBuffTypeFromItem(itemId);
        var duration = GetBuffDurationFromItem(itemId);
        var value = GetBuffValueFromItem(itemId);
        
        if (_player != null && buffType != BuffType.None)
        {
            _player.ApplyBuff(buffType, value, duration);
        }
    }
    
    private void ApplyScrollEffect(string itemId)
    {
        // Handle scroll effects (teleport, summoning, etc.)
        // Simplified: just consume the scroll
    }
    
    private void ApplyFoodEffect(string itemId)
    {
        // Apply food hunger restoration
        var hungerRestore = GetFoodValueFromItem(itemId);
        if (_player != null && hungerRestore > 0)
        {
            // Player hunger restoration
        }
    }
    
    private float GetItemCooldown(string itemId)
    {
        var itemData = ItemManager.Instance?.GetItemData(itemId);
        if (itemData == null) return 1.0f;
        
        switch (itemData.Type)
        {
            case ItemType.Potion:
            case ItemType.HealthPotion:
            case ItemType.ManaPotion:
                return 0.5f;
            case ItemType.Buff:
                return 2.0f;
            case ItemType.Scroll:
                return 1.5f;
            case ItemType.Food:
                return 0.3f;
            default:
                return 1.0f;
        }
    }
    
    private BuffType GetBuffTypeFromItem(string itemId)
    {
        // Map item to buff type
        if (itemId.Contains("attack") || itemId.Contains("strength"))
            return BuffType.Attack;
        if (itemId.Contains("defense") || itemId.Contains("armor"))
            return BuffType.Defense;
        if (itemId.Contains("speed") || itemId.Contains("haste"))
            return BuffType.Speed;
        if (itemId.Contains("health") || itemId.Contains("vitality"))
            return BuffType.Health;
        return BuffType.None;
    }
    
    private float GetBuffDurationFromItem(string itemId)
    {
        // Return duration in seconds
        return 60f; // Default 1 minute
    }
    
    private float GetBuffValueFromItem(string itemId)
    {
        // Return buff value
        return 10f; // Default value
    }
    
    private float GetFoodValueFromItem(string itemId)
    {
        return 20f; // Default hunger restore
    }
    
    private void ShowUseEffect(int slotIndex)
    {
        // Create visual feedback for quick cast
        // This could be a particle effect or UI animation
    }
    
    // Auto-assign items to quick slots
    public void AutoAssignPotions()
    {
        if (_player?.Inventory == null) return;
        
        int slotIndex = 0;
        foreach (var slot in _player.Inventory)
        {
            if (slotIndex >= MaxSlots) break;
            
            var itemData = ItemManager.Instance?.GetItemData(slot.Id);
            if (itemData != null && (itemData.Type == ItemType.Potion || 
                itemData.Type == ItemType.HealthPotion || 
                itemData.Type == ItemType.ManaPotion))
            {
                AssignItemToSlot(slotIndex, slot.Id);
                slotIndex++;
                
                if (slotIndex >= MaxSlots) break;
            }
        }
    }
    
    // Getters
    public List<QuickCastItem> GetQuickSlots() => _quickSlots;
    public QuickCastItem GetSlot(int index) => index >= 0 && index < MaxSlots ? _quickSlots[index] : null;
    public float GetGlobalCooldownRemaining() => _globalCooldownRemaining;
    public bool IsGlobalCooldownActive() => _globalCooldownRemaining > 0;
    
    // Statistics
    public int GetTotalCasts() => _totalCasts;
    public int GetSuccessfulCasts() => _successfulCasts;
    public float GetSuccessRate() => _totalCasts > 0 ? (float)_successfulCasts / _totalCasts : 0f;
    public Dictionary<string, int> GetItemUsageCount() => _itemUsageCount;
    public string GetMostUsedItem()
    {
        string mostUsed = "";
        int maxCount = 0;
        foreach (var kvp in _itemUsageCount)
        {
            if (kvp.Value > maxCount)
            {
                maxCount = kvp.Value;
                mostUsed = kvp.Key;
            }
        }
        return mostUsed;
    }
    
    // Save/Load
    public Dictionary<string, object> SaveData()
    {
        var data = new Dictionary<string, object>();
        
        var slots = new List<Dictionary<string, object>>();
        foreach (var slot in _quickSlots)
        {
            slots.Add(new Dictionary<string, object>
            {
                { "item_id", slot.ItemId },
                { "item_name", slot.ItemName },
                { "is_assigned", slot.IsAssigned },
                { "cooldown_time", slot.CooldownTime }
            });
        }
        
        data["slots"] = slots;
        data["total_casts"] = _totalCasts;
        data["successful_casts"] = _successfulCasts;
        data["item_usage_count"] = _itemUsageCount;
        
        return data;
    }
    
    public void LoadData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("slots"))
        {
            var slots = data["slots"] as List<Dictionary<string, object>>;
            if (slots != null)
            {
                for (int i = 0; i < Mathf.Min(slots.Count, MaxSlots); i++)
                {
                    var slotData = slots[i];
                    var slot = _quickSlots[i];
                    
                    if (slotData.ContainsKey("item_id"))
                        slot.ItemId = slotData["item_id"].ToString();
                    if (slotData.ContainsKey("item_name"))
                        slot.ItemName = slotData["item_name"].ToString();
                    if (slotData.ContainsKey("is_assigned"))
                        slot.IsAssigned = (bool)slotData["is_assigned"];
                    if (slotData.ContainsKey("cooldown_time"))
                        slot.CooldownTime = (float)Convert.ToDouble(slotData["cooldown_time"]);
                }
            }
        }
        
        if (data.ContainsKey("total_casts"))
            _totalCasts = Convert.ToInt32(data["total_casts"]);
        if (data.ContainsKey("successful_casts"))
            _successfulCasts = Convert.ToInt32(data["successful_casts"]);
        if (data.ContainsKey("item_usage_count"))
            _itemUsageCount = new Dictionary<string, int>(
                (System.Collections.Generic.IDictionary<string, int>)data["item_usage_count"]
            );
    }
    
    private void SaveQuickCastData()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem != null)
        {
            var data = SaveData();
            saveSystem.SaveGameData("quick_cast", data);
        }
    }
    
    private void LoadQuickCastData()
    {
        var saveSystem = SaveSystem.Instance;
        if (saveSystem != null)
        {
            var data = saveSystem.LoadGameData("quick_cast");
            LoadData(data);
        }
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        var slots = new Array();
        foreach (var slot in _quickSlots)
        {
            var slotData = new Dictionary
            {
                { "item_id", slot.ItemId },
                { "item_name", slot.ItemName },
                { "is_assigned", slot.IsAssigned },
                { "cooldown_time", slot.CooldownTime }
            };
            slots.Add(slotData);
        }
        
        data["slots"] = slots;
        data["total_casts"] = _totalCasts;
        data["successful_casts"] = _successfulCasts;
        
        // Item usage count
        var usageCount = new Dictionary();
        foreach (var kvp in _itemUsageCount)
        {
            usageCount[kvp.Key] = kvp.Value;
        }
        data["item_usage_count"] = usageCount;
        
        return data;
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("slots"))
        {
            var slots = (Array)data["slots"];
            for (int i = 0; i < Mathf.Min(slots.Count, MaxSlots); i++)
            {
                var slotData = (Dictionary)slots[i];
                var slot = _quickSlots[i];
                
                if (slotData.Contains("item_id"))
                    slot.ItemId = slotData["item_id"].ToString();
                if (slotData.Contains("item_name"))
                    slot.ItemName = slotData["item_name"].ToString();
                if (slotData.Contains("is_assigned"))
                    slot.IsAssigned = (bool)slotData["is_assigned"];
                if (slotData.Contains("cooldown_time"))
                    slot.CooldownTime = (float)slotData["cooldown_time"];
            }
        }
        
        if (data.Contains("total_casts"))
            _totalCasts = (int)data["total_casts"];
        if (data.Contains("successful_casts"))
            _successfulCasts = (int)data["successful_casts"];
        if (data.Contains("item_usage_count"))
        {
            var usageCount = (Dictionary)data["item_usage_count"];
            _itemUsageCount = new Dictionary<string, int>();
            foreach (var kvp in usageCount)
            {
                _itemUsageCount[kvp.Key] = (int)kvp.Value;
            }
        }
    }
}
