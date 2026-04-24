using Godot;
using System;
using System.Collections.Generic;
using SaveSystem = ClawRPG.Scripts.Framework.SaveSystem;
using ClawRPG.Scripts.Framework;
using ClawRPG.Scripts.UI;
using ClawRPG.Scripts.Items;

/// <summary>
/// 快速栏数据 - 定义一个快捷栏槽位的配置
/// </summary>
public class QuickSlotData
{
    public string ItemId { get; set; } = "";
    public int ItemCount { get; set; } = 0;
    public QuickSlotType SlotType { get; set; } = QuickSlotType.Item;
    public int Hotkey { get; set; } = -1; // 1-8 for number keys
    
    public bool IsEmpty => string.IsNullOrEmpty(ItemId) || ItemCount <= 0;
}

/// <summary>
/// 快速栏类型枚举
/// </summary>
public enum QuickSlotType
{
    Item,       // 消耗品
    Scroll,     // 卷轴
    Potion,     // 药水
    Food,       // 食物
    Mount       // 坐骑
}

/// <summary>
/// 快速栏系统 - 管理玩家的快捷栏槽位
/// 支持 1-8 数字键快速使用物品
/// </summary>
public partial class QuickSlotSystem : BaseSystem
{
    public static QuickSlotSystem Instance { get; private set; }
    
    // 8 quick slots (1-8 keys)
    private QuickSlotData[] _slots = new QuickSlotData[8];
    private int _selectedSlot = 0;
    
    // Signals
public delegate void SlotUpdated(int slotIndex, QuickSlotData data);
public delegate void SlotUsed(int slotIndex, QuickSlotData data);
public delegate void SlotEmpty(int slotIndex);
    
    public override void _Ready()
    {
        Instance = this;
        
        // Initialize slots
        for (int i = 0; i < 8; i++)
        {
            _slots[i] = new QuickSlotData();
            _slots[i].Hotkey = i + 1;
        }
        
        LoadQuickSlots();
    }
    
    #region Public Methods
    
    /// <summary>
    /// Add item to quick slot (first available or specific slot)
    /// </summary>
    public bool AddToQuickSlot(string itemId, int count = 1, int targetSlot = -1)
    {
        var item = ItemDatabase.GetItem(itemId);
        if (item == null) return false;
        
        int slotIndex = targetSlot;
        
        // If no specific slot, find first available
        if (slotIndex < 0)
        {
            // Try to stack with existing
            for (int i = 0; i < 8; i++)
            {
                if (_slots[i].ItemId == itemId && _slots[i].ItemCount < 99)
                {
                    slotIndex = i;
                    break;
                }
            }
            
            // Find empty slot
            if (slotIndex < 0)
            {
                for (int i = 0; i < 8; i++)
                {
                    if (_slots[i].IsEmpty)
                    {
                        slotIndex = i;
                        break;
                    }
                }
            }
        }
        
        // No available slot
        if (slotIndex < 0 || slotIndex >= 8) return false;
        
        // Add to slot
        _slots[slotIndex].ItemId = itemId;
        _slots[slotIndex].ItemCount += count;
        _slots[slotIndex].SlotType = GetSlotType(item);
        
        SlotUpdated(slotIndex, _slots[slotIndex]);
        SaveQuickSlots();
        
        return true;
    }
    
    /// <summary>
    /// Use item in quick slot
    /// </summary>
    public bool UseSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 8) return false;
        
        var slot = _slots[slotIndex];
        if (slot.IsEmpty)
        {
            SlotEmpty(slotIndex);
            return false;
        }
        
        // Use the item
        bool success = UseItem(slot.ItemId);
        
        if (success)
        {
            slot.ItemCount--;
            
            if (slot.ItemCount <= 0)
            {
                slot.ItemId = "";
                slot.ItemCount = 0;
            }
            
            SlotUsed(slotIndex, slot);
            SlotUpdated(slotIndex, slot);
            SaveQuickSlots();
        }
        
        return success;
    }
    
    /// <summary>
    /// Get slot data
    /// </summary>
    public QuickSlotData GetSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 8) return null;
        return _slots[slotIndex];
    }
    
    /// <summary>
    /// Get all slots
    /// </summary>
    public QuickSlotData[] GetAllSlots() => _slots;
    
    /// <summary>
    /// Clear a slot
    /// </summary>
    public void ClearSlot(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= 8) return;
        
        _slots[slotIndex] = new QuickSlotData();
        _slots[slotIndex].Hotkey = slotIndex + 1;
        
        SlotUpdated(slotIndex, _slots[slotIndex]);
        SaveQuickSlots();
    }
    
    /// <summary>
    /// Swap two slots
    /// </summary>
    public void SwapSlots(int fromSlot, int toSlot)
    {
        if (fromSlot < 0 || fromSlot >= 8 || toSlot < 0 || toSlot >= 8) return;
        if (fromSlot == toSlot) return;
        
        var temp = _slots[fromSlot];
        _slots[fromSlot] = _slots[toSlot];
        _slots[toSlot] = temp;
        
        // Update hotkeys
        _slots[fromSlot].Hotkey = fromSlot + 1;
        _slots[toSlot].Hotkey = toSlot + 1;
        
        SlotUpdated(fromSlot, _slots[fromSlot]);
        SlotUpdated(toSlot, _slots[toSlot]);
        SaveQuickSlots();
    }
    
    #endregion
    
    #region Private Methods
    
    private QuickSlotType GetSlotType(Item item)
    {
        if (item == null) return QuickSlotType.Item;
        
        // Determine slot type based on item properties
        if (item is Potion || item.Name.Contains("药水") || item.Name.Contains("药水"))
            return QuickSlotType.Potion;
        if (item is Food || item.Name.Contains("食物") || item.Name.Contains("肉"))
            return QuickSlotType.Food;
        if (item.Name.Contains("卷轴") || item.Name.Contains("传送"))
            return QuickSlotType.Scroll;
        
        return QuickSlotType.Item;
    }
    
    private bool UseItem(string itemId)
    {
        var item = ItemDatabase.GetItem(itemId);
        if (item == null) return false;
        
        var player = GetTree().CurrentScene?.GetNode("Player") as Node;
        if (player == null) return false;
        
        // Use item based on type
        bool used = false; 
        
        switch (GetSlotType(item))
        {
            case QuickSlotType.Potion:
                used = UsePotion(item);
                break;
            case QuickSlotType.Food:
                used = UseFood(item);
                break;
            case QuickSlotType.Scroll:
                used = UseScroll(item);
                break;
            default:
                used = UseConsumable(item);
                break;
        }
        
        // Play use sound
        if (used && SoundEffectSystem.Instance != null)
        {
            if (item is Potion || GetSlotType(item) == QuickSlotType.Potion)
                SoundEffectSystem.Instance.PlayPotionUse();
            else if (item is Food || GetSlotType(item) == QuickSlotType.Food)
                SoundEffectSystem.Instance.PlayEat();
            else
                SoundEffectSystem.Instance.PlayItemPickup();
        }
        
        return used;
    }
    
    private bool UsePotion(Item item)
    {
        var playerEntity = GetTree().CurrentScene?.GetNode("Player") as Node;
        if (playerEntity == null) return false;
        
        // Try to heal or restore mana based on item properties
        int healAmount = 0;
        if (item is Potion potion)
        {
            // Potion healing
            healAmount = potion.HealAmount;
        }
        else if (item is Food food)
        {
            healAmount = food.HealthRestore;
        }
        
        // Default healing if no specific value
        if (healAmount <= 0) healAmount = 50;
        
        if (playerEntity.HasMethod("Heal"))
            playerEntity.Call("Heal", healAmount);
        
        return true;
    }
    
    private bool UseFood(Item item)
    {
        var playerEntity = GetTree().CurrentScene?.GetNode("Player") as Node;
        if (playerEntity == null) return false;
        
        int healAmount = 0;
        if (item is Food food)
        {
            healAmount = food.HealthRestore;
        }
        
        if (healAmount <= 0) healAmount = 30;
        
        if (playerEntity.HasMethod("Heal"))
            playerEntity.Call("Heal", healAmount);
        
        return true;
    }
    
    private bool UseScroll(Item item)
    {
        var player = GetTree().CurrentScene?.GetNode("Player") as Node2D;
        if (player == null) return false;
        
        // Handle different scroll types
        if (item.Name.Contains("传送") || item.Name.Contains("home"))
        {
            // Teleport to home/waypoint
            if (player.HasMethod("TeleportToHome"))
                player.Call("TeleportToHome");
        }
        
        return true;
    }
    
    private bool UseConsumable(Item item)
    {
        var playerEntity = GetTree().CurrentScene?.GetNode("Player") as Node;
        if (playerEntity == null) return false;
        
        // Generic consumable - just use it
        if (item is Potion potion)
        {
            if (playerEntity.HasMethod("Heal"))
                playerEntity.Call("Heal", potion.HealAmount > 0 ? potion.HealAmount : 50);
        }
        
        return true;
    }
    
    #endregion
    
    #region Save/Load
    
    private void SaveQuickSlots()
    {
        var saveData = new Godot.Collections.Dictionary();
        
        for (int i = 0; i < 8; i++)
        {
            var slotData = new Godot.Collections.Dictionary();
            slotData["item_id"] = _slots[i].ItemId;
            slotData["item_count"] = _slots[i].ItemCount;
            slotData["slot_type"] = (int)_slots[i].SlotType;
            saveData["slot_" + i] = slotData;
        }
        
        SaveSystem.Save("quick_slots", saveData);
    }
    
    private void LoadQuickSlots()
    {
        var saveData = SaveSystem.Load("quick_slots") as Godot.Collections.Dictionary;
        if (saveData == null) return;
        
        for (int i = 0; i < 8; i++)
        {
            var key = "slot_" + i;
            if (saveData.Contains(key))
            {
                var slotData = saveData[key] as Godot.Collections.Dictionary;
                if (slotData != null)
                {
                    _slots[i].ItemId = slotData.ContainsKey("item_id") ? (string)slotData["item_id"] : "";
                    _slots[i].ItemCount = slotData.ContainsKey("item_count") ? (int)slotData["item_count"] : 0;
                    _slots[i].SlotType = slotData.ContainsKey("slot_type") ? (QuickSlotType)(int)slotData["slot_type"] : QuickSlotType.Item;
                }
            }
        }
    }

    // ===== 持久化方法 =====

    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 快捷栏数据
        var slotsData = new List<Dictionary>();
        for (int i = 0; i < 8; i++)
        {
            var slotDict = new Dictionary<string, object>();
            slotDict["item_id"] = _slots[i].ItemId;
            slotDict["item_count"] = _slots[i].ItemCount;
            slotDict["slot_type"] = (int)_slots[i].SlotType;
            slotsData.Add(slotDict);
        }
        data["slots"] = slotsData;
        
        // 当前选中槽位
        data["selected_slot"] = _selectedSlot;
        
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // 加载快捷栏数据
        if (data.ContainsKey("slots"))
        {
            var slotsData = (Array)data["slots"];
            for (int i = 0; i < Math.Min(8, slotsData.Count); i++)
            {
                var slotDict = (Dictionary)slotsData[i];
                _slots[i].ItemId = slotDict.ContainsKey("item_id") ? slotDict["item_id"].ToString() : "";
                _slots[i].ItemCount = slotDict.ContainsKey("item_count") ? (int)slotDict["item_count"] : 0;
                _slots[i].SlotType = slotDict.ContainsKey("slot_type") ? (QuickSlotType)(int)slotDict["slot_type"] : QuickSlotType.Item;
            }
        }
        
        // 加载选中槽位
        if (data.ContainsKey("selected_slot"))
            _selectedSlot = (int)data["selected_slot"];
    }
    
    #endregion
}
