namespace ClawRPG.Systems.CombatTension;

/// <summary>
/// 战斗紧张度等级枚举
/// </summary>
public enum TensionLevel
{
    Calm = 0,      // 0.0 - 0.2
    Rising = 1,    // 0.2 - 0.4
    Intense = 2,   // 0.4 - 0.6
    Critical = 3,  // 0.6 - 0.8
    Enraged = 4    // 0.8 - 1.0
}

/// <summary>
/// 紧张度参数配置
/// </summary>
public class TensionParams
{
    public float ComboWeight = 0.4f;
    public float PetHpWeight = 0.3f;
    public float BossChargeWeight = 0.3f;
    public int ComboRisingThreshold = 5;
    public int ComboIntenseThreshold = 10;
    public int ComboCriticalThreshold = 15;
}

/// <summary>
/// 紧张度状态
/// </summary>
public class TensionState
{
    public TensionLevel CurrentLevel = TensionLevel.Calm;
    public float NormalizedValue = 0.0f;
    public bool Transitioning = false;
}

/// <summary>
/// 颜色预设
/// </summary>
public class TensionColorPreset
{
    public Godot.Color CalmColor = new Godot.Color(0.1f, 0.3f, 0.6f, 0.0f);      // 冷蓝透明
    public Godot.Color RisingColor = new Godot.Color(0.8f, 0.4f, 0.1f, 0.0f);   // 暖橙透明
    public Godot.Color IntenseColor = new Godot.Color(0.9f, 0.2f, 0.1f, 0.0f);   // 橙红透明
    public Godot.Color CriticalColor = new Godot.Color(0.8f, 0.05f, 0.05f, 0.0f);// 深红透明
    public Godot.Color EnragedColor = new Godot.Color(0.6f, 0.0f, 0.0f, 0.0f);  // 血红透明
}

/// <summary>
/// 粒子预设
/// </summary>
public class TensionParticlePreset
{
    public float CalmSpeed = 20.0f;
    public float RisingSpeed = 60.0f;
    public float IntenseSpeed = 120.0f;
    public float CriticalSpeed = 200.0f;
    public float EnragedSpeed = 300.0f;
}

/// <summary>
/// BGM 层预设
/// </summary>
public class TensionBgmPreset
{
    public int Layer0Index = 0;  // 安静背景
    public int Layer1Index = 1;  // 紧张弦乐
    public int Layer2Index = 2;  // 狂暴鼓点
}

/// <summary>
/// 战斗紧张度数据库（静态配置）
/// </summary>
public static class CombatTensionDatabase
{
    public static readonly TensionParams DefaultParams = new TensionParams
    {
        ComboWeight = 0.4f,
        PetHpWeight = 0.3f,
        BossChargeWeight = 0.3f,
        ComboRisingThreshold = 5,
        ComboIntenseThreshold = 10,
        ComboCriticalThreshold = 15
    };

    public static readonly TensionColorPreset ColorPresets = new TensionColorPreset();
    public static readonly TensionParticlePreset ParticlePresets = new TensionParticlePreset();
    public static readonly TensionBgmPreset BgmPresets = new TensionBgmPreset();

    public static TensionLevel GetTensionLevel(float normalizedValue)
    {
        if (normalizedValue < 0.2f) return TensionLevel.Calm;
        if (normalizedValue < 0.4f) return TensionLevel.Rising;
        if (normalizedValue < 0.6f) return TensionLevel.Intense;
        if (normalizedValue < 0.8f) return TensionLevel.Critical;
        return TensionLevel.Enraged;
    }

    public static Godot.Color GetTensionColor(TensionLevel level)
    {
        return level switch
        {
            TensionLevel.Calm => ColorPresets.CalmColor,
            TensionLevel.Rising => ColorPresets.RisingColor,
            TensionLevel.Intense => ColorPresets.IntenseColor,
            TensionLevel.Critical => ColorPresets.CriticalColor,
            TensionLevel.Enraged => ColorPresets.EnragedColor,
            _ => ColorPresets.CalmColor
        };
    }

    public static float GetParticleSpeed(TensionLevel level)
    {
        return level switch
        {
            TensionLevel.Calm => ParticlePresets.CalmSpeed,
            TensionLevel.Rising => ParticlePresets.RisingSpeed,
            TensionLevel.Intense => ParticlePresets.IntenseSpeed,
            TensionLevel.Critical => ParticlePresets.CriticalSpeed,
            TensionLevel.Enraged => ParticlePresets.EnragedSpeed,
            _ => ParticlePresets.CalmSpeed
        };
    }

    public static int GetBgmLayer(TensionLevel level)
    {
        return level switch
        {
            TensionLevel.Calm => BgmPresets.Layer0Index,
            TensionLevel.Rising => BgmPresets.Layer0Index,
            TensionLevel.Intense => BgmPresets.Layer1Index,
            TensionLevel.Critical => BgmPresets.Layer1Index,
            TensionLevel.Enraged => BgmPresets.Layer2Index,
            _ => BgmPresets.Layer0Index
        };
    }
}
