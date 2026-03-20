using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 圣物数据
/// </summary>
public class RelicCollectionData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public RelicSlotType SlotType { get; set; }
    public RelicRarity Rarity { get; set; }
    public List<RelicEffect> Effects { get; set; } = new();
    public int Level { get; set; } = 1;
    public int MaxLevel { get; set; } = 5;
    public bool IsOwned { get; set; }

    public RelicCollectionData() { }

    public RelicCollectionData(string id, string name, string description, RelicSlotType slotType, RelicRarity rarity)
    {
        Id = id;
        Name = name;
        Description = description;
        SlotType = slotType;
        Rarity = rarity;
        IsOwned = false;
    }

    public float GetRarityMultiplier()
    {
        return Rarity switch
        {
            RelicRarity.Common => 1.0f,
            RelicRarity.Uncommon => 1.25f,
            RelicRarity.Rare => 1.5f,
            RelicRarity.Epic => 2.0f,
            RelicRarity.Legendary => 3.0f,
            _ => 1.0f
        };
    }
}

/// <summary>
/// 玩家圣物数据
/// </summary>
public class PlayerRelicCollectionData
{
    public HashSet<string> OwnedRelicIds { get; set; } = new();
    public Dictionary<RelicSlotType, string> EquippedRelics { get; set; } = new();
    public Dictionary<string, int> RelicLevels { get; set; } = new();
    public int CollectionScore { get; set; }
    public int TotalRelicsOwned { get; set; }
}
