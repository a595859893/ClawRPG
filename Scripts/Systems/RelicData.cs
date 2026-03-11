using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 遗物数据结构
/// </summary>
public class RelicData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public RelicType Type { get; set; }
    public RelicRarity Rarity { get; set; }
    public int Tier { get; set; } // 1-3
    public Dictionary<string, float> AttributeBonuses { get; set; } = new();
    public string SpecialEffect { get; set; } // 特殊效果标识
    public int Price { get; set; }
    
    public RelicData()
    {
    }
    
    public RelicData(string id, string name, string description, RelicType type, RelicRarity rarity, int tier, int price)
    {
        Id = id;
        Name = name;
        Description = description;
        Type = type;
        Rarity = rarity;
        Tier = tier;
        Price = price;
    }
}

public enum RelicType
{
    Attack,      // 攻击型
    Defense,     // 防御型
    Support,     // 辅助型
    Special,     // 特殊型
    Utility      // 工具型
}

public enum RelicRarity
{
    Common,      // 普通
    Uncommon,    // 优秀
    Rare,        // 稀有
    Epic,        // 史诗
    Legendary    // 传说
}

/// <summary>
/// 玩家遗物数据
/// </summary>
public class PlayerRelicData
{
    public List<string> OwnedRelicIds { get; set; } = new();
    public List<string> EquippedRelicIds { get; set; } = new();
    public int MaxRelicSlots { get; set; } = 3;
}
