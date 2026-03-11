using Godot;
using System;
using System.Collections.Generic;

public enum BoonRarity
{
    Common,     // 普通
    Uncommon,   // 优秀
    Rare,       // 稀有
    Epic,       // 史诗
    Legendary   // 传说
}

public enum BoonType
{
    Attack,     // 攻击
    Defense,    // 防御
    Life,       // 生命
    Magic,      // 魔法
    Speed,      // 速度
    Critical,   // 暴击
    Utility,    // utility
    Special     // 特殊
}

[System.Serializable]
public class BoonData
{
    public string Id;
    public string Name;
    public string Description;
    public BoonType Type;
    public BoonRarity Rarity;
    public int AttackBonus;
    public int DefenseBonus;
    public int HealthBonus;
    public int MagicBonus;
    public int SpeedBonus;
    public float CritRateBonus;
    public float CritDamageBonus;
    public float LifestealBonus;
    public float DodgeBonus;
    public int GoldMultiplier;
    public int ExpMultiplier;
    
    public BoonData() { }
    
    public BoonData(string id, string name, string desc, BoonType type, BoonRarity rarity)
    {
        Id = id;
        Name = name;
        Description = desc;
        Type = type;
        Rarity = rarity;
    }
}

[System.Serializable]
public class PlayerBoonData
{
    public List<string> OwnedBoons = new List<string>();
    public List<string> ActiveBoons = new List<string>();
    public int TotalBoonsGained;
    public int TotalGoldEarned;
    public int TotalExpEarned;
}
