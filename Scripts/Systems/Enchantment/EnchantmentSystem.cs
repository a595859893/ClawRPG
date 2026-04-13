using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;

namespace ClawRPG.Systems.Enchantment;

public partial class EnchantmentSystem : BaseSystem
{
    public static new EnchantmentSystem Instance { get; private set; }

    /// <summary>
    /// Signal emitted when an enchantment attempt completes.
    /// </summary>
    [Signal]
    public delegate void EnchantmentResultEventHandler(bool success, string message);

    // Inventory: enchantment ID -> count
    private Dictionary<string, int> _inventory = new();

    // Equipment slot index -> list of enchantment IDs applied to that slot
    private Dictionary<int, List<string>> _equipmentEnchantments = new();

    public override void _Ready()
    {
        base._Ready();
        Instance = this;

        // Initialize inventory with starter enchantments
        _inventory["enchant_fire_strike"] = 3;
        _inventory["enchant_ice_strike"] = 3;
        _inventory["enchant_steel_skin"] = 3;
        _inventory["enchant_mana_flow"] = 2;
        _inventory["enchant_swift"] = 2;
    }

    /// <summary>
    /// Attempts to enchant the specified equipment slot.
    /// Returns true if the enchantment succeeded.
    /// </summary>
    public bool Enchant(string enchantmentId, int playerLevel, int equipmentSlot)
    {
        var record = EnchantmentDatabase.Instance.GetEnchantment(enchantmentId);
        if (record == null)
        {
            EmitSignal(SignalName.EnchantmentResult, false, "未知附魔类型");
            return false;
        }

        // Check player level requirement
        if (playerLevel < record.RequiredPlayerLevel)
        {
            EmitSignal(SignalName.EnchantmentResult, false, $"需要 {record.RequiredPlayerLevel} 级才能使用此附魔");
            return false;
        }

        // Check inventory
        if (!_inventory.TryGetValue(enchantmentId, out int count) || count <= 0)
        {
            EmitSignal(SignalName.EnchantmentResult, false, "没有可用的附魔卷轴");
            return false;
        }

        // Consume one enchantment from inventory
        _inventory[enchantmentId]--;

        // Roll for success
        float roll = (float)GD.RandRange(0.0, 1.0);
        if (roll <= record.SuccessRate)
        {
            // Success — apply enchantment to slot
            if (!_equipmentEnchantments.ContainsKey(equipmentSlot))
                _equipmentEnchantments[equipmentSlot] = new List<string>();
            _equipmentEnchantments[equipmentSlot].Add(enchantmentId);

            EmitSignal(SignalName.EnchantmentResult, true, $"附魔成功！{record.Name} 已应用到装备");
            return true;
        }
        else
        {
            // Failure — enchantment is consumed but not applied
            EmitSignal(SignalName.EnchantmentResult, false, $"附魔失败！{record.Name} 消失了");
            return false;
        }
    }

    /// <summary>
    /// Returns the number of available enchantments of the given type.
    /// </summary>
    public int GetEnchantmentCount(string enchantmentId)
    {
        _inventory.TryGetValue(enchantmentId, out int count);
        return count;
    }

    /// <summary>
    /// Returns the full enchantment inventory (ID -> count).
    /// </summary>
    public Dictionary<string, int> GetInventory()
    {
        return new Dictionary<string, int>(_inventory);
    }

    /// <summary>
    /// Returns the list of enchantment IDs applied to the given equipment slot.
    /// </summary>
    public List<string> GetEquipmentEnchantments(int equipmentSlot)
    {
        if (_equipmentEnchantments.TryGetValue(equipmentSlot, out var list))
            return new List<string>(list);
        return new List<string>();
    }

    /// <summary>
    /// Adds enchantment scrolls to the inventory (e.g., from loot).
    /// </summary>
    public void AddToInventory(string enchantmentId, int count = 1)
    {
        if (!_inventory.ContainsKey(enchantmentId))
            _inventory[enchantmentId] = 0;
        _inventory[enchantmentId] += count;
    }

    /// <summary>
    /// Clears all enchantments from an equipment slot.
    /// </summary>
    public void ClearEquipmentSlot(int slot)
    {
        _equipmentEnchantments.Remove(slot);
    }

        public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();

        // Serialize inventory
        var inventoryData = new Dictionary<string, int>();
        foreach (var kvp in _inventory)
            inventoryData[kvp.Key] = kvp.Value;
        data["inventory"] = inventoryData;

        // Serialize equipment enchantments
        var equipData = new Dictionary<int, List<string>>();
        foreach (var kvp in _equipmentEnchantments)
            equipData[kvp.Key] = kvp.Value;
        data["equipmentEnchantments"] = equipData;

        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data.TryGetValue("inventory", out var invObj) && invObj is Dictionary<string, object> invDict)
        {
            _inventory.Clear();
            foreach (var kvp in invDict)
            {
                if (kvp.Value is int intVal)
                    _inventory[kvp.Key] = intVal;
            }
        }

        if (data.TryGetValue("equipmentEnchantments", out var eqObj) && eqObj is Dictionary<string, object> eqDict)
        {
            _equipmentEnchantments.Clear();
            foreach (var kvp in eqDict)
            {
                int slot = int.Parse(kvp.Key);
                if (kvp.Value is List<object> list)
                {
                    var strList = new List<string>();
                    foreach (var item in list)
                        if (item is string s)
                            strList.Add(s);
                    _equipmentEnchantments[slot] = strList;
                }
            }
        }
    }
}
