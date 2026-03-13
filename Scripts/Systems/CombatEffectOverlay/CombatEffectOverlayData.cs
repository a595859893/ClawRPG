using Godot;
using System;
using System.Collections.Generic;

public class CombatEffectOverlayData : Godot.Object
{
    // Screen overlay effects state
    public bool RedOverlayActive { get; set; }
    public float RedOverlayIntensity { get; set; }
    public bool ScreenFlashActive { get; set; }
    public float ScreenFlashIntensity { get; set; }
    public Color FlashColor { get; set; }
    
    // Camera shake state
    public bool CameraShakeActive { get; set; }
    public float CameraShakeIntensity { get; set; }
    public float CameraShakeDuration { get; set; }
    
    // Slow motion state
    public bool SlowMotionActive { get; set; }
    public float SlowMotionScale { get; set; }
    public float SlowMotionDuration { get; set; }
    
    // Chromatic aberration state
    public bool ChromaticAberrationActive { get; set; }
    public float ChromaticAberrationIntensity { get; set; }
    
    // Vignette state
    public bool VignetteActive { get; set; }
    public float VignetteIntensity { get; set; }
    
    // Active floating texts
    public List<FloatingTextData> ActiveFloatingTexts { get; set; }
    
    // Statistics
    public int TotalScreenFlashes { get; set; }
    public int TotalCameraShakes { get; set; }
    public int TotalSlowMotions { get; set; }
    public int TotalFloatingTexts { get; set; }
    public float TotalShakeIntensity { get; set; }
    
    public CombatEffectOverlayData()
    {
        ActiveFloatingTexts = new List<FloatingTextData>();
        FlashColor = Colors.White;
        Reset();
    }
    
    public void Reset()
    {
        RedOverlayActive = false;
        RedOverlayIntensity = 0f;
        ScreenFlashActive = false;
        ScreenFlashIntensity = 0f;
        FlashColor = Colors.White;
        
        CameraShakeActive = false;
        CameraShakeIntensity = 0f;
        CameraShakeDuration = 0f;
        
        SlowMotionActive = false;
        SlowMotionScale = 1f;
        SlowMotionDuration = 0f;
        
        ChromaticAberrationActive = false;
        ChromaticAberrationIntensity = 0f;
        
        VignetteActive = false;
        VignetteIntensity = 0f;
    }
}

public class FloatingTextData
{
    public string Text { get; set; }
    public Vector2 Position { get; set; }
    public Color Color { get; set; }
    public float Size { get; set; }
    public float Lifetime { get; set; }
    public float Age { get; set; }
    public Vector2 Velocity { get; set; }
    public FloatingTextType Type { get; set; }
    
    public FloatingTextData(string text, Vector2 position, Color color, float size, float lifetime, Vector2 velocity, FloatingTextType type)
    {
        Text = text;
        Position = position;
        Color = color;
        Size = size;
        Lifetime = lifetime;
        Age = 0f;
        Velocity = velocity;
        Type = type;
    }
}

public enum FloatingTextType
{
    Damage,
    CriticalDamage,
    Heal,
    Miss,
    Block,
    Dodge,
    Experience,
    Gold,
    Buff,
    Debuff
}

public enum ScreenEffectType
{
    RedOverlay,
    ScreenFlash,
    CameraShake,
    SlowMotion,
    ChromaticAberration,
    Vignette,
    Pixelate,
    RadialBlur
}
