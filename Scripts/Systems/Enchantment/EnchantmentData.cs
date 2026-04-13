using Godot;
using System;

namespace ClawRPG.Systems.Enchantment;

public enum EnchantmentType
{
    Attack,
    Defense,
    Magic,
    Utility,
    Legendary,
    Weapon = Attack,
    Armor = Defense,
    Accessory = Magic,
    Universal = Utility
}

public enum EnchantmentAttribute
{
    Damage,
    Defense,
    Health,
    Mana,
    CriticalRate,
    CriticalDamage,
    AttackSpeed,
    MoveSpeed,
    FireResistance,
    IceResistance,
    LightningResistance,
    PoisonResistance,
    AllAttributes
}

// Alias for EnchantmentUI compatibility
public enum EnchantmentEffect
{
    Damage = EnchantmentAttribute.Damage,
    Defense = EnchantmentAttribute.Defense,
    Health = EnchantmentAttribute.Health,
    Mana = EnchantmentAttribute.Mana,
    CriticalRate = EnchantmentAttribute.CriticalRate,
    CriticalDamage = EnchantmentAttribute.CriticalDamage,
    AttackSpeed = EnchantmentAttribute.AttackSpeed,
    Speed = EnchantmentAttribute.MoveSpeed,
    LifeSteal = 100,  // Not in EnchantmentAttribute
    Dodge = 101,       // Not in EnchantmentAttribute
    FireResistance = EnchantmentAttribute.FireResistance,
    IceResistance = EnchantmentAttribute.IceResistance,
    LightningResistance = EnchantmentAttribute.LightningResistance,
    PoisonResistance = EnchantmentAttribute.PoisonResistance,
    AllAttributes = EnchantmentAttribute.AllAttributes,
    ManaRegen = 102   // Not in EnchantmentAttribute
}

public enum EnchantmentTier
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public class EnchantmentRecord
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public EnchantmentType Type { get; set; }
    public EnchantmentTier Tier { get; set; }
    public EnchantmentAttribute Attribute { get; set; }
    public float AttributeValue { get; set; }
    public float SuccessRate { get; set; }
    public int RequiredPlayerLevel { get; set; }
    public int Cost { get; set; }

    public Color GetRarityColor()
    {
        return Tier switch
        {
            EnchantmentTier.Common => new Color(0.7f, 0.7f, 0.7f),
            EnchantmentTier.Uncommon => new Color(0.2f, 0.8f, 0.2f),
            EnchantmentTier.Rare => new Color(0.3f, 0.5f, 1.0f),
            EnchantmentTier.Epic => new Color(0.6f, 0.3f, 0.9f),
            EnchantmentTier.Legendary => new Color(1.0f, 0.6f, 0.0f),
            _ => new Color(0.7f, 0.7f, 0.7f)
        };
    }
}

public class EnchantmentSaveData
{
    public Dictionary<string, int> Inventory { get; set; } = new();
    public Dictionary<int, List<string>> EquipmentEnchantments { get; set; } = new();
}
