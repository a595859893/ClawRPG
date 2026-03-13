using Godot;
using System;
using System.Collections.Generic;

public class CombatEffectOverlayDatabase : Godot.Object
{
    // Singleton instance
    private static CombatEffectOverlayDatabase _instance;
    public static CombatEffectOverlayDatabase Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new CombatEffectOverlayDatabase();
            }
            return _instance;
        }
    }
    
    // Screen flash configurations
    public Dictionary<ScreenEffectType, ScreenFlashConfig> ScreenFlashConfigs { get; private set; }
    public Dictionary<string, FloatingTextConfig> FloatingTextConfigs { get; private set; }
    public Dictionary<string, CameraShakeConfig> CameraShakeConfigs { get; private set; }
    public Dictionary<string, SlowMotionConfig> SlowMotionConfigs { get; private set; }
    
    // Floating text colors by type
    public Dictionary<FloatingTextType, Color> FloatingTextColors { get; private set; }
    
    public CombatEffectOverlayDatabase()
    {
        InitializeScreenFlashConfigs();
        InitializeFloatingTextConfigs();
        InitializeCameraShakeConfigs();
        InitializeSlowMotionConfigs();
        InitializeFloatingTextColors();
    }
    
    private void InitializeScreenFlashConfigs()
    {
        ScreenFlashConfigs = new Dictionary<ScreenEffectType, ScreenFlashConfig>();
        
        ScreenFlashConfigs[ScreenEffectType.RedOverlay] = new ScreenFlashConfig
        {
            MinIntensity = 0f,
            MaxIntensity = 0.6f,
            DefaultIntensity = 0.3f,
            FadeInDuration = 0.1f,
            FadeOutDuration = 0.5f,
            Color = new Color(1f, 0f, 0f, 0.3f)
        };
        
        ScreenFlashConfigs[ScreenEffectType.ScreenFlash] = new ScreenFlashConfig
        {
            MinIntensity = 0f,
            MaxIntensity = 1f,
            DefaultIntensity = 0.8f,
            FadeInDuration = 0.05f,
            FadeOutDuration = 0.3f,
            Color = Colors.White
        };
        
        ScreenFlashConfigs[ScreenEffectType.ChromaticAberration] = new ScreenFlashConfig
        {
            MinIntensity = 0f,
            MaxIntensity = 10f,
            DefaultIntensity = 3f,
            FadeInDuration = 0.1f,
            FadeOutDuration = 0.4f,
            Color = Colors.White
        };
        
        ScreenFlashConfigs[ScreenEffectType.Vignette] = new ScreenFlashConfig
        {
            MinIntensity = 0f,
            MaxIntensity = 1f,
            DefaultIntensity = 0.5f,
            FadeInDuration = 0.2f,
            FadeOutDuration = 0.6f,
            Color = Colors.Black
        };
    }
    
    private void InitializeFloatingTextConfigs()
    {
        FloatingTextConfigs = new Dictionary<string, FloatingTextConfig>();
        
        // Damage text configs
        FloatingTextConfigs["damage_small"] = new FloatingTextConfig
        {
            BaseSize = 24f,
            MinSize = 20f,
            MaxSize = 32f,
            Lifetime = 1.0f,
            Velocity = new Vector2(0, -50f),
            RandomVelocityRange = 20f
        };
        
        FloatingTextConfigs["damage_critical"] = new FloatingTextConfig
        {
            BaseSize = 48f,
            MinSize = 40f,
            MaxSize = 64f,
            Lifetime = 1.5f,
            Velocity = new Vector2(0, -80f),
            RandomVelocityRange = 30f
        };
        
        FloatingTextConfigs["heal"] = new FloatingTextConfig
        {
            BaseSize = 32f,
            MinSize = 28f,
            MaxSize = 40f,
            Lifetime = 1.2f,
            Velocity = new Vector2(0, -60f),
            RandomVelocityRange = 15f
        };
        
        FloatingTextConfigs["miss"] = new FloatingTextConfig
        {
            BaseSize = 28f,
            MinSize = 24f,
            MaxSize = 36f,
            Lifetime = 0.8f,
            Velocity = new Vector2(0, -40f),
            RandomVelocityRange = 10f
        };
        
        FloatingTextConfigs["experience"] = new FloatingTextConfig
        {
            BaseSize = 22f,
            MinSize = 18f,
            MaxSize = 28f,
            Lifetime = 1.5f,
            Velocity = new Vector2(0, -45f),
            RandomVelocityRange = 15f
        };
        
        FloatingTextConfigs["gold"] = new FloatingTextConfig
        {
            BaseSize = 26f,
            MinSize = 22f,
            MaxSize = 34f,
            Lifetime = 1.3f,
            Velocity = new Vector2(0, -55f),
            RandomVelocityRange = 20f
        };
    }
    
    private void InitializeCameraShakeConfigs()
    {
        CameraShakeConfigs = new Dictionary<string, CameraShakeConfig>();
        
        CameraShakeConfigs["light"] = new CameraShakeConfig
        {
            Intensity = 5f,
            Duration = 0.2f,
            Frequency = 20f,
            DecreaseFactor = 2f
        };
        
        CameraShakeConfigs["medium"] = new CameraShakeConfig
        {
            Intensity = 10f,
            Duration = 0.4f,
            Frequency = 25f,
            DecreaseFactor = 1.5f
        };
        
        CameraShakeConfigs["heavy"] = new CameraShakeConfig
        {
            Intensity = 20f,
            Duration = 0.6f,
            Frequency = 30f,
            DecreaseFactor = 1.2f
        };
        
        CameraShakeConfigs["extreme"] = new CameraShakeConfig
        {
            Intensity = 40f,
            Duration = 1.0f,
            Frequency = 40f,
            DecreaseFactor = 1.0f
        };
        
        CameraShakeConfigs["critical"] = new CameraShakeConfig
        {
            Intensity = 60f,
            Duration = 1.5f,
            Frequency = 50f,
            DecreaseFactor = 0.8f
        };
    }
    
    private void InitializeSlowMotionConfigs()
    {
        SlowMotionConfigs = new Dictionary<string, SlowMotionConfig>();
        
        SlowMotionConfigs["quick"] = new SlowMotionConfig
        {
            Scale = 0.3f,
            Duration = 0.15f,
            FadeInDuration = 0.05f,
            FadeOutDuration = 0.1f
        };
        
        SlowMotionConfigs["normal"] = new SlowMotionConfig
        {
            Scale = 0.2f,
            Duration = 0.3f,
            FadeInDuration = 0.1f,
            FadeOutDuration = 0.2f
        };
        
        SlowMotionConfigs["dramatic"] = new SlowMotionConfig
        {
            Scale = 0.1f,
            Duration = 0.5f,
            FadeInDuration = 0.15f,
            FadeOutDuration = 0.35f
        };
    }
    
    private void InitializeFloatingTextColors()
    {
        FloatingTextColors = new Dictionary<FloatingTextType, Color>();
        
        FloatingTextColors[FloatingTextType.Damage] = new Color(1f, 0.9f, 0.7f);      // Light yellow
        FloatingTextColors[FloatingTextType.CriticalDamage] = new Color(1f, 0.3f, 0.3f); // Red
        FloatingTextColors[FloatingTextType.Heal] = new Color(0.3f, 1f, 0.5f);        // Green
        FloatingTextColors[FloatingTextType.Miss] = new Color(0.7f, 0.7f, 0.7f);      // Gray
        FloatingTextColors[FloatingTextType.Block] = new Color(0.5f, 0.5f, 1f);        // Blue
        FloatingTextColors[FloatingTextType.Dodge] = new Color(0.6f, 0.8f, 1f);        // Light blue
        FloatingTextColors[FloatingTextType.Experience] = new Color(0.9f, 0.6f, 1f);   // Purple
        FloatingTextColors[FloatingTextType.Gold] = new Color(1f, 0.85f, 0.3f);        // Gold
        FloatingTextColors[FloatingTextType.Buff] = new Color(0.4f, 1f, 0.8f);        // Cyan
        FloatingTextColors[FloatingTextType.Debuff] = new Color(0.8f, 0.3f, 0.8f);    // Magenta
    }
    
    public Color GetFloatingTextColor(FloatingTextType type)
    {
        if (FloatingTextColors.ContainsKey(type))
            return FloatingTextColors[type];
        return Colors.White;
    }
    
    public FloatingTextConfig GetFloatingTextConfig(string configName)
    {
        if (FloatingTextConfigs.ContainsKey(configName))
            return FloatingTextConfigs[configName];
        return FloatingTextConfigs["damage_small"];
    }
    
    public CameraShakeConfig GetCameraShakeConfig(string configName)
    {
        if (CameraShakeConfigs.ContainsKey(configName))
            return CameraShakeConfigs[configName];
        return CameraShakeConfigs["medium"];
    }
    
    public SlowMotionConfig GetSlowMotionConfig(string configName)
    {
        if (SlowMotionConfigs.ContainsKey(configName))
            return SlowMotionConfigs[configName];
        return SlowMotionConfigs["normal"];
    }
}

public class ScreenFlashConfig
{
    public float MinIntensity { get; set; }
    public float MaxIntensity { get; set; }
    public float DefaultIntensity { get; set; }
    public float FadeInDuration { get; set; }
    public float FadeOutDuration { get; set; }
    public Color Color { get; set; }
}

public class FloatingTextConfig
{
    public float BaseSize { get; set; }
    public float MinSize { get; set; }
    public float MaxSize { get; set; }
    public float Lifetime { get; set; }
    public Vector2 Velocity { get; set; }
    public float RandomVelocityRange { get; set; }
}

public class CameraShakeConfig
{
    public float Intensity { get; set; }
    public float Duration { get; set; }
    public float Frequency { get; set; }
    public float DecreaseFactor { get; set; }
}

public class SlowMotionConfig
{
    public float Scale { get; set; }
    public float Duration { get; set; }
    public float FadeInDuration { get; set; }
    public float FadeOutDuration { get; set; }
}
