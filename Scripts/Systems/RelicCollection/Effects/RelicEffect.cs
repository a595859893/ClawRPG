using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems;

/// <summary>
/// 圣物效果类型
/// </summary>
public enum RelicEffectType
{
    StatModifier,  // 属性修改 - 直接修改属性值
    Trigger,       // 触发效果 - 满足条件时触发
    Passive        // 被动效果 - 持续生效
}

/// <summary>
/// 圣物效果基类
/// </summary>
public abstract class RelicEffect
{
    public RelicEffectType EffectType { get; protected set; }
    public string Description { get; set; }

    public abstract void Apply(Player player);
    public abstract void Remove(Player player);
    public abstract Dictionary<string, object> Serialize();
    public abstract void Deserialize(Dictionary<string, object> data);
}
