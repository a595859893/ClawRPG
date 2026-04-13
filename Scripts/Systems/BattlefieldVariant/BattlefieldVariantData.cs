using Godot;
using System;

/// <summary>
/// 战场变体类型枚举（REQ-115）
/// </summary>
public enum BattlefieldVariantType
{
    None = 0,
    /// <summary>焦土 — 地面持续灼烧，静止叠伤</summary>
    ScorchedEarth = 1,
    /// <summary>破碎地面 — 近战落空概率，弹道偏移</summary>
    BrokenGround = 2,
    /// <summary>静电空气 — 技能链式反应</summary>
    StaticAir = 3
}

/// <summary>
/// 战场变体配置数据（Resources/.tres 文件）
/// </summary>
public partial class BattlefieldVariantConfig : Resource
{
    [Export] public BattlefieldVariantType VariantType = BattlefieldVariantType.None;
    [Export] public string DisplayName = "";
    [Export] public string Description = "";
    [Export] public Color IconColor = Colors.White;
    [Export] public float DamagePerTick = 2f;
    [Export] public float TickInterval = 1f;
    [Export] public float StackingPenaltyPerTick = 1f;
    [Export] public float MissChance = 0.15f;
    [Export] public float ProjectileDeviationChance = 0.10f;
    [Export] public float ChainReactionChance = 0.20f;
    [Export] public float ChainRadius = 80f;
}

/// <summary>
/// 战场变体运行时数据
/// </summary>
public class BattlefieldVariantRuntimeData
{
    public BattlefieldVariantType ActiveVariant { get; set; } = BattlefieldVariantType.None;
    public bool IsActive { get; set; } = false;
    public float ElapsedTime { get; set; } = 0f;
    public float EffectIntensity { get; set; } = 1.0f;
    public float LastEffectTime { get; set; } = 0f;
    public float StationaryTime { get; set; } = 0f; // for ScorchedEarth
    public Vector2 LastPlayerPosition { get; set; } = Vector2.Zero; // for ScorchedEarth
    public int ChainReactionCount { get; set; } = 0; // for StaticAir
    public string ToJson()
    {
        return $"{{\"activeVariant\":{(int)ActiveVariant},\"isActive\":{IsActive},\"elapsedTime\":{ElapsedTime:F2},\"effectIntensity\":{EffectIntensity:F2}}}";
    }
}
