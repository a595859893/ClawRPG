using Godot;
using System;
using System.Collections.Generic;

// 神秘宝藏数据类型
public class MysteryTreasureData
{
    public string TreasureId { get; set; }
    public string TreasureName { get; set; }
    public string Description { get; set; }
    public TreasureRarity Rarity { get; set; }
    public TreasureType Type { get; set; }
    public int MinGold { get; set; }
    public int MaxGold { get; set; }
    public List<string> ItemIds { get; set; }
    public List<int> ItemCounts { get; set; }
    public int ExpReward { get; set; }
    public float SpawnChance { get; set; }
}

public enum TreasureRarity
{
    Common,      // 普通
    Uncommon,    // 优秀
    Rare,        // 稀有
    Epic,        // 史诗
    Legendary    // 传说
}

public enum TreasureType
{
    Chest,       // 宝箱
    Hidden,      // 隐藏
    Ancient,     // 远古
    Monster,     // 怪物掉落
    Special      // 特殊
}

// 玩家宝藏数据
public class PlayerMysteryTreasureData
{
    public int TotalFound { get; set; }
    public Dictionary<string, int> RarityCount { get; set; }
    public Dictionary<string, int> TypeCount { get; set; }
    public int TotalGoldEarned { get; set; }
    public int TotalExpEarned { get; set; }
    public List<string> DiscoveredTreasureIds { get; set; }
    public Dictionary<string, int> TreasureHistory { get; set; } // treasureId -> count
}

// 宝藏实例数据
public class TreasureInstance
{
    public string InstanceId { get; set; }
    public string TreasureId { get; set; }
    public Vector2 Position { get; set; }
    public bool IsOpened { get; set; }
    public bool IsDiscovered { get; set; }
    public float SpawnTime { get; set; }
}
