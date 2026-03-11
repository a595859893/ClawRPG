using Godot;
using System;
using System.Collections.Generic;

public enum TitleCategory
{
    Combat,      // 战斗称号
    Gathering,   // 采集称号
    Exploration, // 探索称号
    Social,      // 社交称号
    Special      // 特殊称号
}

public enum TitleRarity
{
    Common,   // 普通
    Rare,     // 稀有
    Epic,     // 史诗
    Legendary // 传说
}

[System.Serializable]
public class TitleDefinition
{
    public string Id;
    public string Name;
    public string Description;
    public TitleCategory Category;
    public TitleRarity Rarity;
    public Dictionary<string, float> AttributeBonuses; // 属性加成
    public string IconName;
    public bool IsSecret; // 是否隐藏名称
    
    public TitleDefinition()
    {
        Id = "";
        Name = "";
        Description = "";
        Category = TitleCategory.Combat;
        Rarity = TitleRarity.Common;
        AttributeBonuses = new Dictionary<string, float>();
        IconName = "";
        IsSecret = false;
    }
}

[System.Serializable]
public class PlayerTitleData
{
    public string TitleId;
    public bool IsUnlocked;
    public bool IsActive;
    public DateTime UnlockTime;
    public int UnlockConditionProgress; // 解锁条件进度
    
    public PlayerTitleData()
    {
        TitleId = "";
        IsUnlocked = false;
        IsActive = false;
        UnlockTime = DateTime.MinValue;
        UnlockConditionProgress = 0;
    }
}

[System.Serializable]
public class PlayerTitleCollection
{
    public Dictionary<string, PlayerTitleData> Titles = new Dictionary<string, PlayerTitleData>();
    public string ActiveTitleId = "";
    
    public PlayerTitleCollection()
    {
        Titles = new Dictionary<string, PlayerTitleData>();
        ActiveTitleId = "";
    }
}
