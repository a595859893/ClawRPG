using Godot;
using System;
using System.Collections.Generic;

public class EnchantmentData
{
    // 附魔类型
    public enum EnchantmentType
    {
        Weapon,
        Armor,
        Accessory,
        Helmet,
        Boots,
        Gloves
    }

    // 稀有度
    public enum Rarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    // 附魔属性类型
    public enum PropertyType
    {
        Attack,
        Defense,
        Health,
        Speed,
        Critical,
        Evasion,
        LifeSteal,
        MagicAttack,
        MagicDefense,
        FireResistance,
        IceResistance,
        LightningResistance
    }

    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public EnchantmentType Type { get; set; }
    public Rarity RarityLevel { get; set; }
    public Dictionary<PropertyType, float> Properties { get; set; }
    public int RequiredLevel { get; set; }
    public int MaxLevel { get; set; }
    public int BaseCost { get; set; }
    public float SuccessRate { get; set; }
    public string IconName { get; set; }

    public EnchantmentData()
    {
        Properties = new Dictionary<PropertyType, float>();
    }

    public float GetPropertyValue(PropertyType type)
    {
        return Properties.ContainsKey(type) ? Properties[type] : 0f;
    }

    public float GetTotalPropertyBonus()
    {
        float total = 0f;
        foreach (var prop in Properties.Values)
        {
            total += prop;
        }
        return total;
    }
}

public class EnchantmentInstance
{
    public string Id { get; set; }
    public string TemplateId { get; set; }
    public int CurrentLevel { get; set; }
    public int Experience { get; set; }
    public bool IsActive { get; set; }
    public DateTime AppliedTime { get; set; }

    public EnchantmentInstance()
    {
        Id = Guid.NewGuid().ToString();
        CurrentLevel = 1;
        Experience = 0;
        IsActive = true;
        AppliedTime = DateTime.Now;
    }
}

// 附魔应用记录
public class EnchantmentRecord
{
    public string EquipmentId { get; set; }
    public string EnchantmentId { get; set; }
    public DateTime AppliedTime { get; set; }
    public int TotalAttempts { get; set; }
    public int SuccessfulAttempts { get; set; }

    public EnchantmentRecord()
    {
        TotalAttempts = 0;
        SuccessfulAttempts = 0;
    }

    public float GetSuccessRate()
    {
        return TotalAttempts > 0 ? (float)SuccessfulAttempts / TotalAttempts * 100f : 0f;
    }
}
