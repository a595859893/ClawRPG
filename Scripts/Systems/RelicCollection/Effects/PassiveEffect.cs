using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 被动效果
/// </summary>
public class PassiveEffect : RelicEffect
{
    public string EffectId { get; set; }
    public float Magnitude { get; set; }
    public bool IsPermanent { get; set; }

    public PassiveEffect()
    {
        EffectType = RelicEffectType.Passive;
        IsPermanent = true;
    }

    public PassiveEffect(string effectId, float magnitude, bool isPermanent = true)
    {
        EffectType = RelicEffectType.Passive;
        EffectId = effectId;
        Magnitude = magnitude;
        IsPermanent = isPermanent;
        Description = GenerateDescription();
    }

    private string GenerateDescription()
    {
        string permanentStr = IsPermanent ? "永久" : $"持续 {Magnitude}s";
        return $"{EffectId}: {permanentStr}";
    }

    public override void Apply(Player player)
    {
        // 根据效果ID应用被动效果
    }

    public override void Remove(Player player)
    {
        // 移除被动效果
    }

    public override Dictionary<string, object> Serialize()
    {
        return new Dictionary<string, object>
        {
            { "effect_type", "Passive" },
            { "effect_id", EffectId },
            { "magnitude", Magnitude },
            { "is_permanent", IsPermanent }
        };
    }

    public override void Deserialize(Dictionary<string, object> data)
    {
        if (data.ContainsKey("effect_id"))
            EffectId = data["effect_id"].ToString();
        if (data.ContainsKey("magnitude"))
            Magnitude = Convert.ToSingle(data["magnitude"]);
        if (data.ContainsKey("is_permanent"))
            IsPermanent = Convert.ToBoolean(data["is_permanent"]);
        Description = GenerateDescription();
    }
}
