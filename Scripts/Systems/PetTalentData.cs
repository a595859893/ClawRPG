using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物天赋数据 - 定义宠物天赋的类型、稀有度和效果
/// </summary>
public class PetTalentData : Godot.Object
{
    public enum TalentType
    {
        Attack,      // 攻击型
        Defense,     // 防御型
        Support,     // 辅助型
        Special,     // 特殊型
        Utility      // 工具型
    }

    public enum TalentRarity
    {
        Common,      // 普通
        Uncommon,    // 优秀
        Rare,        // 稀有
        Epic,        // 史诗
        Legendary    // 传说
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public TalentType Type { get; set; }
    public TalentRarity Rarity { get; set; }
    public float BonusValue { get; set; }
    public string AffectedStat { get; set; }

    public PetTalentData() { }

    public PetTalentData(string id, string name, string desc, TalentType type, TalentRarity rarity, float bonus, string stat)
    {
        Id = id;
        Name = name;
        Description = desc;
        Type = type;
        Rarity = rarity;
        BonusValue = bonus;
        AffectedStat = stat;
    }
}

public class PetTalent : Godot.Object
{
    public string TalentId { get; set; }
    public int Level { get; set; }
    public bool IsUnlocked { get; set; }

    public PetTalent() 
    {
        Level = 1;
        IsUnlocked = false; 
    }

    public PetTalent(string talentId, int level = 1)
    {
        TalentId = talentId;
        Level = level;
        IsUnlocked = true;
    }
}

public class PlayerPetTalentData : Godot.Object
{
    public Dictionary<string, List<PetTalent>> PetTalents { get; set; }
    public Dictionary<string, int> TalentPoints { get; set; }

    public PlayerPetTalentData()
    {
        PetTalents = new Dictionary<string, List<PetTalent>>();
        TalentPoints = new Dictionary<string, int>();
    }
}
