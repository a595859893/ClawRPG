using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 装备套装系统 - 管理装备套装的收集和激活
/// 支持多种套装，每种套装有2件和4件激活效果
/// </summary>
public class EquipmentSetSystem : Node
{
    /// <summary>
    /// 获取系统单例实例
    /// </summary>
    // Singleton instance
    public static EquipmentSetSystem Instance { get; private set; }

    // Set data storage
    public Dictionary<string, EquipmentSet> Sets { get; private set; }
    
    // Player's unlocked sets
    public HashSet<string> UnlockedSets { get; private set; }
    
    // Currently equipped set bonuses
    public Dictionary<string, int> EquippedPieces { get; private set; }

    // Signals
    public static string SetUnlockedSignal => "set_unlocked";
    public static string SetBonusActivatedSignal => "set_bonus_activated";

    public override void _Ready()
    {
        Instance = this;
        Sets = new Dictionary<string, EquipmentSet>();
        UnlockedSets = new HashSet<string>();
        EquippedPieces = new Dictionary<string, int>();
        
        InitializeSets();
        
        // Connect to player equipment changes
        // This would ideally connect to an equipment system signal
    }

    private void InitializeSets()
    {
        // Warrior Sets - Heavy armor focus
        AddSet(new EquipmentSet(
            id: "iron_warrior",
            name: "Iron Warrior",
            description: "Standard issue armor for frontline soldiers",
            pieces: new Dictionary<string, string> {
                { "helmet", "Iron Helmet" },
                { "chest", "Iron Chestplate" },
                { "legs", "Iron Leggings" },
                { "weapon", "Iron Sword" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+10% Defense" },
                { 4, "+20% Attack, +10% Defense" }
            },
            rarity: Rarity.Common
        ));

        AddSet(new EquipmentSet(
            id: "dragon_slayer",
            name: "Dragon Slayer",
            description: "Armor forged to hunt the mightiest of beasts",
            pieces: new Dictionary<string, string> {
                { "helmet", "Dragon Helm" },
                { "chest", "Dragon Chestplate" },
                { "legs", "Dragon Leggings" },
                { "weapon", "Dragon Slayer Greatsword" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+15% Attack against Bosses" },
                { 4, "+30% Attack against Bosses, +20% Fire Resistance" }
            },
            rarity: Rarity.Epic
        ));

        // Mage Sets - Magic focus
        AddSet(new EquipmentSet(
            id: "arcane_mage",
            name: "Arcane Mage",
            description: "Robes imbued with ancient magical knowledge",
            pieces: new Dictionary<string, string> {
                { "helmet", "Arcane Hat" },
                { "chest", "Arcane Robe" },
                { "legs", "Arcane Pants" },
                { "weapon", "Arcane Staff" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+15% Magic Damage" },
                { 4, "+30% Magic Damage, +20% Mana Regen" }
            },
            rarity: Rarity.Rare
        ));

        AddSet(new EquipmentSet(
            id: "frost_wizard",
            name: "Frost Wizard",
            description: "Frozen artifacts from the eternal winter realm",
            pieces: new Dictionary<string, string> {
                { "helmet", "Frost Crown" },
                { "chest", "Frost Robe" },
                { "legs", "Frost Greaves" },
                { "weapon", "Frost Staff" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+20% Ice Damage" },
                { 4, "+40% Ice Damage, Enemies frozen take +10% damage" }
            },
            rarity: Rarity.Epic
        ));

        // Ranger Sets - Speed and crit focus
        AddSet(new EquipmentSet(
            id: "shadow_assassin",
            name: "Shadow Assassin",
            description: "Gear of the silent night hunters",
            pieces: new Dictionary<string, string> {
                { "helmet", "Shadow Hood" },
                { "chest", "Shadow Vest" },
                { "legs", "Shadow Pants" },
                { "weapon", "Shadow Daggers" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+15% Critical Chance" },
                { 4, "+30% Critical Chance, +20% Attack Speed" }
            },
            rarity: Rarity.Rare
        ));

        // Healer Sets - Support focus
        AddSet(new EquipmentSet(
            id: "holy_priest",
            name: "Holy Priest",
            description: "Sacred armor blessed by the divine light",
            pieces: new Dictionary<string, string> {
                { "helmet", "Holy Circlet" },
                { "chest", "Holy Vestment" },
                { "legs", "Holy Sandals" },
                { "weapon", "Holy Staff" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+20% Healing Received" },
                { 4, "+40% Healing Received, +25% Holy Damage" }
            },
            rarity: Rarity.Rare
        ));

        // Elemental Sets
        AddSet(new EquipmentSet(
            id: "inferno",
            name: "Inferno",
            description: "Burning armor from the heart of a volcano",
            pieces: new Dictionary<string, string> {
                { "helmet", "Inferno Helm" },
                { "chest", "Inferno Plate" },
                { "legs", "Inferno Greaves" },
                { "weapon", "Inferno Blade" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+25% Fire Damage" },
                { 4, "+50% Fire Damage, Enemies burn for extra damage" }
            },
            rarity: Rarity.Epic
        ));

        AddSet(new EquipmentSet(
            id: "thunder_lord",
            name: "Thunder Lord",
            description: "Storm-infused armor crackling with lightning",
            pieces: new Dictionary<string, string> {
                { "helmet", "Thunder Crown" },
                { "chest", "Thunder Plate" },
                { "legs", "Thunder Greaves" },
                { "weapon", "Thunder Hammer" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+25% Lightning Damage" },
                { 4, "+50% Lightning Damage, Chance to stun enemies" }
            },
            rarity: Rarity.Epic
        ));

        // Legendary Sets
        AddSet(new EquipmentSet(
            id: "chaos_king",
            name: "Chaos King",
            description: "Corrupted armor that bends reality itself",
            pieces: new Dictionary<string, string> {
                { "helmet", "Chaos Crown" },
                { "chest", "Chaos Armor" },
                { "legs", "Chaos Greaves" },
                { "weapon", "Chaos Reaper" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+20% All Damage" },
                { 4, "+40% All Damage, +15% Critical Chance" }
            },
            rarity: Rarity.Legendary
        ));

        AddSet(new EquipmentSet(
            id: "divine_guardian",
            name: "Divine Guardian",
            description: "Heavenly armor blessed by the gods themselves",
            pieces: new Dictionary<string, string> {
                { "helmet", "Divine Helm" },
                { "chest", "Divine Plate" },
                { "legs", "Divine Greaves" },
                { "weapon", "Divine Shield" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+30% Defense, +20% Health" },
                { 4, "+50% Defense, +40% Health, Reflect 10% damage" }
            },
            rarity: Rarity.Legendary
        ));

        // Beginner Sets
        AddSet(new EquipmentSet(
            id: "adventurer",
            name: "Adventurer",
            description: "Basic gear for those starting their journey",
            pieces: new Dictionary<string, string> {
                { "helmet", "Adventurer Hat" },
                { "chest", "Adventurer Vest" },
                { "legs", "Adventurer Pants" },
                { "weapon", "Adventurer Sword" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+5% All Stats" },
                { 4, "+10% All Stats, +5% Movement Speed" }
            },
            rarity: Rarity.Common
        ));

        // Nature Sets
        AddSet(new EquipmentSet(
            id: "druid",
            name: "Druid of the Grove",
            description: "Natural armor infused with forest magic",
            pieces: new Dictionary<string, string> {
                { "helmet", "Druid Hood" },
                { "chest", "Druid Tunic" },
                { "legs", "Druid Pants" },
                { "weapon", "Druid Staff" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+20% Nature Damage, +10% Health Regen" },
                { 4, "+40% Nature Damage, +20% Health Regen" }
            },
            rarity: Rarity.Rare
        ));

        // Poison Sets
        AddSet(new EquipmentSet(
            id: "plague_bringer",
            name: "Plague Bringer",
            description: "Toxic armor of the alchemist's craft",
            pieces: new Dictionary<string, string> {
                { "helmet", "Plague Mask" },
                { "chest", "Plague Coat" },
                { "legs", "Plague Greaves" },
                { "weapon", "Plague Dagger" }
            },
            bonuses: new Dictionary<int, string> {
                { 2, "+25% Poison Damage" },
                { 4, "+50% Poison Damage, Enemies take poison damage over time" }
            },
            rarity: Rarity.Epic
        ));
    }

    private void AddSet(EquipmentSet set)
    {
        Sets[set.Id] = set;
    }

    // Get all sets
    public List<EquipmentSet> GetAllSets()
    {
        return new List<EquipmentSet>(Sets.Values);
    }

    // Get sets by rarity
    public List<EquipmentSet> GetSetsByRarity(Rarity rarity)
    {
        List<EquipmentSet> result = new List<EquipmentSet>();
        foreach (var set in Sets.Values)
        {
            if (set.Rarity == rarity)
                result.Add(set);
        }
        return result;
    }

    // Check if player has a piece from a set
    public void OnEquipmentChanged(string slot, string itemId)
    {
        // This would be called when player equips/unequips items
        // Check which sets the item belongs to
        foreach (var set in Sets.Values)
        {
            foreach (var piece in set.Pieces.Values)
            {
                if (piece == itemId)
                {
                    UpdateSetProgress(set.Id);
                    break;
                }
            }
        }
    }

    // Update equipped piece count for a set
    private void UpdateSetProgress(string setId)
    {
        if (!Sets.ContainsKey(setId))
            return;

        var set = Sets[setId];
        int equippedCount = 0;

        // This would check actual player inventory
        // For now, we'll track it conceptually
        if (EquippedPieces.ContainsKey(setId))
            equippedCount = EquippedPieces[setId];
        else
            EquippedPieces[setId] = 0;

        // Check for set bonus activation
        int newBonusLevel = 0;
        if (equippedCount >= 4)
            newBonusLevel = 4;
        else if (equippedCount >= 2)
            newBonusLevel = 2;

        if (newBonusLevel > 0 && set.Bonuses.ContainsKey(newBonusLevel))
        {
            // Emit bonus activated signal
            EmitSignal(SetBonusActivatedSignal, setId, newBonusLevel, set.Bonuses[newBonusLevel]);
        }
    }

    // Unlock a set (for tracking/achievements)
    public void UnlockSet(string setId)
    {
        if (!UnlockedSets.Contains(setId))
        {
            UnlockedSets.Add(setId);
            if (Sets.ContainsKey(setId))
            {
                EmitSignal(SetUnlockedSignal, setId, Sets[setId].Name);
            }
        }
    }

    // Get set by ID
    public EquipmentSet GetSet(string setId)
    {
        return Sets.ContainsKey(setId) ? Sets[setId] : null;
    }

    // Calculate set bonus for a specific set
    public string GetActiveBonus(string setId)
    {
        if (!Sets.ContainsKey(setId) || !EquippedPieces.ContainsKey(setId))
            return "";

        int count = EquippedPieces[setId];
        if (count >= 4 && Sets[setId].Bonuses.ContainsKey(4))
            return Sets[setId].Bonuses[4];
        if (count >= 2 && Sets[setId].Bonuses.ContainsKey(2))
            return Sets[setId].Bonuses[2];

        return "";
    }

    // Save/Load support
    public Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            { "unlocked_sets", new List<string>(UnlockedSets) },
            { "equipped_pieces", EquippedPieces }
        };
    }

    public void Deserialize(Dictionary<string, object> data)
    {
        if (data.ContainsKey("unlocked_sets"))
        {
            UnlockedSets = new HashSet<string>((List<string>)data["unlocked_sets"]);
        }
        if (data.ContainsKey("equipped_pieces"))
        {
            EquippedPieces = new Dictionary<string, int>((Dictionary<string, object>)data["equipped_pieces"]);
        }
    }
}

// Equipment Set data class
public class EquipmentSet
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public Dictionary<string, string> Pieces { get; set; } // slot -> item name
    public Dictionary<int, string> Bonuses { get; set; } // piece count -> bonus description
    public Rarity Rarity { get; set; }

    public EquipmentSet(string id, string name, string description, 
        Dictionary<string, string> pieces, Dictionary<int, string> bonuses, Rarity rarity)
    {
        Id = id;
        Name = name;
        Description = description;
        Pieces = pieces;
        Bonuses = bonuses;
        Rarity = rarity;
    }
}

// Rarity enum
public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}
