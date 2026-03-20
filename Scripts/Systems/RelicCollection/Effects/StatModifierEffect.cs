using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 属性修改效果
/// </summary>
public class StatModifierEffect : RelicEffect
{
    public string StatName { get; set; }
    public float Value { get; set; }
    public bool IsPercentage { get; set; }

    public StatModifierEffect()
    {
        EffectType = RelicEffectType.StatModifier;
    }

    public StatModifierEffect(string statName, float value, bool isPercentage = false)
    {
        EffectType = RelicEffectType.StatModifier;
        StatName = statName;
        Value = value;
        IsPercentage = isPercentage;
        Description = GenerateDescription();
    }

    private string GenerateDescription()
    {
        string valueStr = IsPercentage ? $"{Value * 100}%" : Value.ToString("F1");
        return $"{StatName} {valueStr}";
    }

    public override void Apply(Player player)
    {
        switch (StatName.ToLower())
        {
            case "attack":
                player.RelicAttackBonus += IsPercentage ? player.Attack * Value : Value;
                break;
            case "defense":
                player.RelicDefenseBonus += IsPercentage ? player.Defense * Value : Value;
                break;
            case "health":
                player.RelicHealthBonus += IsPercentage ? player.MaxHealth * Value : Value;
                break;
            case "speed":
                player.RelicSpeedBonus += IsPercentage ? player.Speed * Value : Value;
                break;
            case "crit_rate":
                player.RelicCritRateBonus += Value;
                break;
            case "crit_damage":
                player.RelicCritDamageBonus += Value;
                break;
            case "lifesteal":
                player.RelicLifestealBonus += Value;
                break;
            case "dodge":
                player.RelicDodgeBonus += Value;
                break;
            case "max_mana":
                player.MaxMana += IsPercentage ? player.MaxMana * Value : Value;
                break;
            case "mana_regen":
                player.ManaRegen += IsPercentage ? player.ManaRegen * Value : Value;
                break;
        }
    }

    public override void Remove(Player player)
    {
        switch (StatName.ToLower())
        {
            case "attack":
                player.RelicAttackBonus -= IsPercentage ? player.Attack * Value : Value;
                break;
            case "defense":
                player.RelicDefenseBonus -= IsPercentage ? player.Defense * Value : Value;
                break;
            case "health":
                player.RelicHealthBonus -= IsPercentage ? player.MaxHealth * Value : Value;
                break;
            case "speed":
                player.RelicSpeedBonus -= IsPercentage ? player.Speed * Value : Value;
                break;
            case "crit_rate":
                player.RelicCritRateBonus -= Value;
                break;
            case "crit_damage":
                player.RelicCritDamageBonus -= Value;
                break;
            case "lifesteal":
                player.RelicLifestealBonus -= Value;
                break;
            case "dodge":
                player.RelicDodgeBonus -= Value;
                break;
            case "max_mana":
                player.MaxMana -= IsPercentage ? player.MaxMana * Value : Value;
                break;
            case "mana_regen":
                player.ManaRegen -= IsPercentage ? player.ManaRegen * Value : Value;
                break;
        }
    }

    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            { "effect_type", "StatModifier" },
            { "stat_name", StatName },
            { "value", Value },
            { "is_percentage", IsPercentage }
        };
    }

    public override void Deserialize(Dictionary<string, object> data)
    {
        if (data.ContainsKey("stat_name"))
            StatName = data["stat_name"].ToString();
        if (data.ContainsKey("value"))
            Value = Convert.ToSingle(data["value"]);
        if (data.ContainsKey("is_percentage"))
            IsPercentage = Convert.ToBoolean(data["is_percentage"]);
        Description = GenerateDescription();
    }
}
