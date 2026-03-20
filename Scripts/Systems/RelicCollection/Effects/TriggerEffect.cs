using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 触发效果
/// </summary>
public class TriggerEffect : RelicEffect
{
    public string TriggerCondition { get; set; }
    public string EffectId { get; set; }
    public float Chance { get; set; }
    public float Cooldown { get; set; }
    public float LastTriggerTime { get; set; }

    public TriggerEffect()
    {
        EffectType = RelicEffectType.Trigger;
        Chance = 1.0f;
        Cooldown = 0f;
        LastTriggerTime = -999f;
    }

    public TriggerEffect(string triggerCondition, string effectId, float chance = 1.0f, float cooldown = 0f)
    {
        EffectType = RelicEffectType.Trigger;
        TriggerCondition = triggerCondition;
        EffectId = effectId;
        Chance = chance;
        Cooldown = cooldown;
        LastTriggerTime = -999f;
        Description = GenerateDescription();
    }

    private string GenerateDescription()
    {
        string cooldownStr = Cooldown > 0 ? $" ({Cooldown}s CD)" : "";
        return $"{TriggerCondition}: {EffectId} ({Chance * 100}%{cooldownStr})";
    }

    public bool CanTrigger(float currentTime)
    {
        if (Cooldown > 0 && currentTime - LastTriggerTime < Cooldown)
            return false;
        return GD.Rand() < Chance;
    }

    public void MarkTriggered(float currentTime)
    {
        LastTriggerTime = currentTime;
    }

    public override void Apply(Player player)
    {
        // 触发效果由事件系统检查
    }

    public override void Remove(Player player)
    {
        LastTriggerTime = -999f;
    }

    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            { "effect_type", "Trigger" },
            { "trigger_condition", TriggerCondition },
            { "effect_id", EffectId },
            { "chance", Chance },
            { "cooldown", Cooldown }
        };
    }

    public override void Deserialize(Dictionary<string, object> data)
    {
        if (data.ContainsKey("trigger_condition"))
            TriggerCondition = data["trigger_condition"].ToString();
        if (data.ContainsKey("effect_id"))
            EffectId = data["effect_id"].ToString();
        if (data.ContainsKey("chance"))
            Chance = Convert.ToSingle(data["chance"]);
        if (data.ContainsKey("cooldown"))
            Cooldown = Convert.ToSingle(data["cooldown"]);
        Description = GenerateDescription();
    }
}
