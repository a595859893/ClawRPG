using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.PetEquipment;

/// <summary>
/// Data structure for pet equipment items
/// </summary>
public class PetEquipmentData
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Type { get; set; } = ""; // Necklace/Collar/Harness/Accessory/Toy/Treat
    public string Rarity { get; set; } = "Common"; // Common/Uncommon/Rare/Epic/Legendary
    public int Level { get; set; } = 1;
    
    // Stat bonuses
    public float AttackBonus { get; set; }
    public float DefenseBonus { get; set; }
    public float HealthBonus { get; set; }
    public float SpeedBonus { get; set; }
    public float CritBonus { get; set; }
    public float DodgeBonus { get; set; }
    public float ExperienceBonus { get; set; }
    
    // Special effects
    public string SpecialEffect { get; set; } = "";
    public float EffectValue { get; set; }
    
    // State
    public bool IsEquipped { get; set; }
    public int SlotIndex { get; set; } = -1;
}

/// <summary>
/// Database configuration for pet equipment
/// </summary>
public class PetEquipmentDatabase
{
    private static readonly Dictionary<int, PetEquipmentData> _equipment = new()
    {
        // Common Equipment
        { 1, new PetEquipmentData { Id = 1, Name = "Basic Collar", Description = "A simple collar for your pet", Type = "Collar", Rarity = "Common", Level = 1, AttackBonus = 2, DefenseBonus = 1 } },
        { 2, new PetEquipmentData { Id = 2, Name = "Leather Harness", Description = "Sturdy leather harness", Type = "Harness", Rarity = "Common", Level = 1, DefenseBonus = 3, HealthBonus = 10 } },
        { 3, new PetEquipmentData { Id = 3, Name = "Wooden Toy", Description = "A simple wooden toy", Type = "Toy", Rarity = "Common", Level = 1, SpeedBonus = 1, ExperienceBonus = 2 } },
        
        // Uncommon Equipment
        { 4, new PetEquipmentData { Id = 4, Name = "Silver Necklace", Description = "A decorative silver necklace", Type = "Necklace", Rarity = "Uncommon", Level = 5, AttackBonus = 5, CritBonus = 2 } },
        { 5, new PetEquipmentData { Id = 5, Name = "Reinforced Harness", Description = "Enhanced leather harness with metal accents", Type = "Harness", Rarity = "Uncommon", Level = 5, DefenseBonus = 8, HealthBonus = 25 } },
        { 6, new PetEquipmentData { Id = 6, Name = "Bouncy Ball", Description = "A fun bouncy ball for play", Type = "Toy", Rarity = "Uncommon", Level = 5, SpeedBonus = 3, ExperienceBonus = 5 } },
        { 7, new PetEquipmentData { Id = 7, Name = "Lucky Charm", Description = "Brings good fortune to your pet", Type = "Accessory", Rarity = "Uncommon", Level = 5, DodgeBonus = 3, ExperienceBonus = 3 } },
        
        // Rare Equipment
        { 8, new PetEquipmentData { Id = 8, Name = "Golden Necklace", Description = "A golden necklace with gems", Type = "Necklace", Rarity = "Rare", Level = 10, AttackBonus = 10, CritBonus = 5, SpeedBonus = 2 } },
        { 9, new PetEquipmentData { Id = 9, Name = "Dragon Scale Harness", Description = "Harness made from dragon scales", Type = "Harness", Rarity = "Rare", Level = 10, DefenseBonus = 15, HealthBonus = 50, FireResist = 10 } },
        { 10, new PetEquipmentData { Id = 10, Name = "Magic Frisbee", Description = "A magical flying disc", Type = "Toy", Rarity = "Rare", Level = 10, SpeedBonus = 5, ExperienceBonus = 10, AttackBonus = 3 } },
        { 11, new PetEquipmentData { Id = 11, Name = "Crystal Accessory", Description = "A crystal that glows with energy", Type = "Accessory", Rarity = "Rare", Level = 10, CritBonus = 3, DodgeBonus = 5, ExperienceBonus = 5 } },
        
        // Epic Equipment
        { 12, new PetEquipmentData { Id = 12, Name = "Amulet of Power", Description = "Ancient amulet imbued with power", Type = "Necklace", Rarity = "Epic", Level = 20, AttackBonus = 20, CritBonus = 10, DefenseBonus = 5 } },
        { 13, new PetEquipmentData { Id = 13, Name = "Titanium Armor", Description = "Lightweight yet powerful armor", Type = "Harness", Rarity = "Epic", Level = 20, DefenseBonus = 25, HealthBonus = 100, SpeedBonus = 3 } },
        { 14, new PetEquipmentData { Id = 14, Name = "Enchanted Plushie", Description = "A magical stuffed companion", Type = "Toy", Rarity = "Epic", Level = 20, ExperienceBonus = 20, HealthBonus = 30, AttackBonus = 5 } },
        { 15, new PetEquipmentData { Id = 15, Name = "Shadow Cloak", Description = "Cloak of shadow energy", Type = "Accessory", Rarity = "Epic", Level = 20, DodgeBonus = 10, CritBonus = 8, AttackBonus = 8 } },
        
        // Legendary Equipment
        { 16, new PetEquipmentData { Id = 16, Name = "Divine Necklace", Description = "Necklace blessed by the gods", Type = "Necklace", Rarity = "Legendary", Level = 30, AttackBonus = 35, CritBonus = 15, SpeedBonus = 5, ExperienceBonus = 10 } },
        { 17, new PetEquipmentData { Id = 17, Name = "Celestial Armor", Description = "Armor forged from starlight", Type = "Harness", Rarity = "Legendary", Level = 30, DefenseBonus = 40, HealthBonus = 200, AllElementResist = 15 } },
        { 18, new PetEquipmentData { Id = 18, Name = "Cosmic Toy", Description = "A toy from beyond the stars", Type = "Toy", Rarity = "Legendary", Level = 30, ExperienceBonus = 35, SpeedBonus = 10, AllStatsBonus = 5 } },
        { 19, new PetEquipmentData { Id = 19, Name = "Eternal Crown", Description = "Crown of infinite power", Type = "Accessory", Rarity = "Legendary", Level = 30, AllStatsBonus = 15, ExperienceBonus = 15, SpecialEffect = "Immortal", EffectValue = 1 } },
        { 20, new PetEquipmentData { Id = 20, Name = "Mythic Collar", Description = "Collar of legends", Type = "Collar", Rarity = "Legendary", Level = 30, AttackBonus = 25, DefenseBonus = 25, HealthBonus = 150, SpeedBonus = 8 } },
    };
    
    public static PetEquipmentData GetEquipment(int id) => _equipment.GetValueOrDefault(id);
    public static List<PetEquipmentData> GetAllEquipment() => new(_equipment.Values);
    public static List<PetEquipmentData> GetByRarity(string rarity)
    {
        var result = new List<PetEquipmentData>();
        foreach (var e in _equipment.Values)
        {
            if (e.Rarity == rarity) result.Add(e);
        }
        return result;
    }
    public static List<PetEquipmentData> GetByType(string type)
    {
        var result = new List<PetEquipmentData>();
        foreach (var e in _equipment.Values)
        {
            if (e.Type == type) result.Add(e);
        }
        return result;
    }
    
    // Additional properties for elemental resists
    public float FireResist { get; set; }
    public float IceResist { get; set; }
    public float LightningResist { get; set; }
    public float AllElementResist { get; set; }
    public float AllStatsBonus { get; set; }
}

/// <summary>
/// Core pet equipment system
/// </summary>
public partial class PetEquipmentSystem : BaseSystem
{
    public static PetEquipmentSystem Instance { get; private set; }
    
    // Player's owned equipment
    public Dictionary<int, PetEquipmentData> OwnedEquipment { get; private set; } = new();
    
    // Equipment slots per pet (6 slots)
    // Collar, Necklace, Harness, Accessory, Toy, Treat
    public Dictionary<int, List<int>> EquippedSlots { get; private set; } = new();
    
    // Statistics
    public int TotalEquipmentOwned { get; private set; }
    public int TotalEquipSlotsUsed { get; private set; }
    public int EquipmentUpgradesPurchased { get; private set; }
    public float TotalExperienceFromEquipment { get; private set; }
    
    // Rarity colors
    private static readonly Dictionary<string, Color> RarityColors = new()
    {
        { "Common", new Color(0.7f, 0.7f, 0.7f) },
        { "Uncommon", new Color(0.2f, 0.8f, 0.2f) },
        { "Rare", new Color(0.2f, 0.5f, 1.0f) },
        { "Epic", new Color(0.6f, 0.3f, 0.8f) },
        { "Legendary", new Color(1.0f, 0.6f, 0.0f) }
    };
    
    public override void _Ready()
    {
        Instance = this;
    }
    
    /// <summary>
    /// Add equipment to player's inventory
    /// </summary>
    public void AddEquipment(int equipmentId)
    {
        var data = PetEquipmentDatabase.GetEquipment(equipmentId);
        if (data == null) return;
        
        var newEquipment = new PetEquipmentData
        {
            Id = data.Id,
            Name = data.Name,
            Description = data.Description,
            Type = data.Type,
            Rarity = data.Rarity,
            Level = data.Level,
            AttackBonus = data.AttackBonus,
            DefenseBonus = data.DefenseBonus,
            HealthBonus = data.HealthBonus,
            SpeedBonus = data.SpeedBonus,
            CritBonus = data.CritBonus,
            DodgeBonus = data.DodgeBonus,
            ExperienceBonus = data.ExperienceBonus,
            SpecialEffect = data.SpecialEffect,
            EffectValue = data.EffectValue,
            IsEquipped = false,
            SlotIndex = -1
        };
        
        OwnedEquipment[equipmentId] = newEquipment;
        TotalEquipmentOwned = OwnedEquipment.Count;
    }
    
    /// <summary>
    /// Equip equipment to a pet
    /// </summary>
    public bool EquipEquipment(int equipmentId, int petId)
    {
        if (!OwnedEquipment.ContainsKey(equipmentId)) return false;
        
        var equipment = OwnedEquipment[equipmentId];
        if (equipment.IsEquipped) return false;
        
        // Get slot type
        string slotType = equipment.Type;
        int slotIndex = GetSlotIndex(slotType);
        
        // Initialize pet slots if needed
        if (!EquippedSlots.ContainsKey(petId))
        {
            EquippedSlots[petId] = new List<int> { -1, -1, -1, -1, -1, -1 };
        }
        
        // Check if slot is occupied
        var slots = EquippedSlots[petId];
        if (slots[slotIndex] != -1)
        {
            // Already equipped something in this slot
            return false;
        }
        
        // Equip it
        slots[slotIndex] = equipmentId;
        equipment.IsEquipped = true;
        equipment.SlotIndex = slotIndex;
        TotalEquipSlotsUsed++;
        
        return true;
    }
    
    /// <summary>
    /// Unequip equipment from a pet
    /// </summary>
    public bool UnequipEquipment(int equipmentId, int petId)
    {
        if (!OwnedEquipment.ContainsKey(equipmentId)) return false;
        
        var equipment = OwnedEquipment[equipmentId];
        if (!equipment.IsEquipped) return false;
        
        if (!EquippedSlots.ContainsKey(petId)) return false;
        
        var slots = EquippedSlots[petId];
        slots[equipment.SlotIndex] = -1;
        equipment.IsEquipped = false;
        equipment.SlotIndex = -1;
        TotalEquipSlotsUsed--;
        
        return true;
    }
    
    /// <summary>
    /// Calculate total bonuses for a pet
    /// </summary>
    public Dictionary<string, float> CalculateBonuses(int petId)
    {
        var bonuses = new Dictionary<string, float>
        {
            { "Attack", 0 }, { "Defense", 0 }, { "Health", 0 },
            { "Speed", 0 }, { "Crit", 0 }, { "Dodge", 0 },
            { "Experience", 0 }, { "FireResist", 0 }, { "IceResist", 0 },
            { "LightningResist", 0 }
        };
        
        if (!EquippedSlots.ContainsKey(petId)) return bonuses;
        
        foreach (var equipmentId in EquippedSlots[petId])
        {
            if (equipmentId == -1 || !OwnedEquipment.ContainsKey(equipmentId)) continue;
            
            var eq = OwnedEquipment[equipmentId];
            bonuses["Attack"] += eq.AttackBonus;
            bonuses["Defense"] += eq.DefenseBonus;
            bonuses["Health"] += eq.HealthBonus;
            bonuses["Speed"] += eq.SpeedBonus;
            bonuses["Crit"] += eq.CritBonus;
            bonuses["Dodge"] += eq.DodgeBonus;
            bonuses["Experience"] += eq.ExperienceBonus;
            
            if (!string.IsNullOrEmpty(eq.SpecialEffect))
            {
                bonuses[eq.SpecialEffect] = eq.EffectValue;
            }
        }
        
        return bonuses;
    }
    
    /// <summary>
    /// Get equipment count by rarity
    /// </summary>
    public Dictionary<string, int> GetRarityDistribution()
    {
        var dist = new Dictionary<string, int>
        {
            { "Common", 0 }, { "Uncommon", 0 }, { "Rare", 0 }, { "Epic", 0 }, { "Legendary", 0 }
        };
        
        foreach (var eq in OwnedEquipment.Values)
        {
            if (dist.ContainsKey(eq.Rarity)) dist[eq.Rarity]++;
        }
        
        return dist;
    }
    
    /// <summary>
    /// Get color for rarity
    /// </summary>
    public static Color GetRarityColor(string rarity)
    {
        return RarityColors.GetValueOrDefault(rarity, Colors.White);
    }
    
    private int GetSlotIndex(string slotType)
    {
        return slotType switch
        {
            "Collar" => 0,
            "Necklace" => 1,
            "Harness" => 2,
            "Accessory" => 3,
            "Toy" => 4,
            "Treat" => 5,
            _ => 0
        };
    }
    
    /// <summary>
    /// Generate random equipment based on rarity weights
    /// </summary>
    public int GenerateRandomEquipment()
    {
        var random = new Random();
        double roll = random.NextDouble();
        
        string rarity;
        if (roll < 0.60) rarity = "Common";
        else if (roll < 0.85) rarity = "Uncommon";
        else if (roll < 0.95) rarity = "Rare";
        else if (roll < 0.99) rarity = "Epic";
        else rarity = "Legendary";
        
        var items = PetEquipmentDatabase.GetByRarity(rarity);
        if (items.Count == 0) return -1;
        
        var selected = items[random.Next(items.Count)];
        return selected.Id;
    }
    
    public void SaveData()
    {
        // Save owned equipment
        // Save equipped slots
    }
    
    public void LoadData()
    {
        // Load owned equipment
        // Load equipped slots
    }
}
