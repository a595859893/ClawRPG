using Godot;
using System.Collections.Generic;

/// <summary>
/// 每日仪式数据 - 存储仪式类型配置
/// </summary>
public enum RitualType
{
    MorningPrayer,
    EveningMeditation,
    BlessingOfFire,
    OfferingToWater,
    TributeToEarth,
    WindWhisper,
    LightCeremony,
    ShadowRitual,
    BloodPact,
    SpiritSummon
}

public enum RitualTier
{
    Novice,
    Adept,
    Master,
    Legendary
}

public class RitualData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public RitualType Type { get; set; }
    public RitualTier Tier { get; set; }
    public int GoldCost { get; set; }
    public Dictionary<string, float> AttributeBonuses { get; set; }
    public float Duration { get; set; }
    public int ReputationGain { get; set; }
}
