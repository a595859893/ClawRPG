using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems;

/// <summary>
/// Screen effect data for post-processing effects
/// </summary>
public class ScreenEffectData
{
    public bool BloomEnabled { get; set; } = true;
    public float BloomIntensity { get; set; } = 0.5f;
    public float BloomThreshold { get; set; } = 0.8f;
    public float BloomBlur { get; set; } = 2.0f;
    
    public bool VignetteEnabled { get; set; } = true;
    public float VignetteIntensity { get; set; } = 0.3f;
    public float VignetteSmoothness { get; set; } = 0.5f;
    
    public bool ColorGradingEnabled { get; set; } = true;
    public float Saturation { get; set; } = 1.0f;
    public float Contrast { get; set; } = 1.0f;
    public float Temperature { get; set; } = 0.0f;
    
    public bool ChromaticAberrationEnabled { get; set; } = false;
    public float ChromaticAberrationAmount { get; set; } = 0.5f;
    
    public bool FilmGrainEnabled { get; set; } = false;
    public float FilmGrainIntensity { get; set; } = 0.1f;
    
    // Screen flash state
    public Color FlashColor { get; set; } = Colors.White;
    public float FlashIntensity { get; set; } = 0.0f;
    public float FlashDuration { get; set; } = 0.2f;
    public float FlashTimer { get; set; } = 0.0f;
    
    // Screen shake state
    public Vector2 ShakeOffset { get; set; } = Vector2.Zero;
    public float ShakeIntensity { get; set; } = 0.0f;
    public float ShakeDuration { get; set; } = 0.0f;
    public float ShakeTimer { get; set; } = 0.0f;
    
    // Screen shake trauma for decay
    public float ShakeTrauma { get; set; } = 0.0f;
    public float ShakeDecay { get; set; } = 2.0f;
    
    // Statistics
    public int TotalFlashes { get; set; } = 0;
    public int TotalShakes { get; set; } = 0;
    public float TotalShakeIntensity { get; set; } = 0.0f;
    
    public Dictionary<string, bool> EnabledEffects { get; set; } = new()
    {
        { "Bloom", true },
        { "Vignette", true },
        { "ColorGrading", true },
        { "ChromaticAberration", false },
        { "FilmGrain", false }
    };
}
