using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems;

/// <summary>
/// Database for screen effect presets
/// </summary>
public static class ScreenEffectDatabase
{
    public enum EffectPreset
    {
        Default,
        Cinematic,
        Retro,
        Nightmare,
        Dreamy,
        Intense,
        Subtle,
        Noir,
        Vibrant,
        Muted
    }
    
    public static Dictionary<EffectPreset, Dictionary<string, float>> Presets { get; } = new()
    {
        {
            EffectPreset.Default, new Dictionary<string, float>
            {
                { "BloomIntensity", 0.5f },
                { "BloomThreshold", 0.8f },
                { "BloomBlur", 2.0f },
                { "VignetteIntensity", 0.3f },
                { "VignetteSmoothness", 0.5f },
                { "Saturation", 1.0f },
                { "Contrast", 1.0f },
                { "Temperature", 0.0f },
                { "ChromaticAberration", 0.0f },
                { "FilmGrain", 0.0f }
            }
        },
        {
            EffectPreset.Cinematic, new Dictionary<string, float>
            {
                { "BloomIntensity", 0.7f },
                { "BloomThreshold", 0.85f },
                { "BloomBlur", 3.0f },
                { "VignetteIntensity", 0.5f },
                { "VignetteSmoothness", 0.6f },
                { "Saturation", 0.9f },
                { "Contrast", 1.1f },
                { "Temperature", 0.05f },
                { "ChromaticAberration", 0.3f },
                { "FilmGrain", 0.05f }
            }
        },
        {
            EffectPreset.Retro, new Dictionary<string, float>
            {
                { "BloomIntensity", 0.3f },
                { "BloomThreshold", 0.9f },
                { "BloomBlur", 1.5f },
                { "VignetteIntensity", 0.4f },
                { "VignetteSmoothness", 0.3f },
                { "Saturation", 1.2f },
                { "Contrast", 1.3f },
                { "Temperature", -0.1f },
                { "ChromaticAberration", 0.8f },
                { "FilmGrain", 0.15f }
            }
        },
        {
            EffectPreset.Nightmare, new Dictionary<string, float>
            {
                { "BloomIntensity", 0.8f },
                { "BloomThreshold", 0.7f },
                { "BloomBlur", 4.0f },
                { "VignetteIntensity", 0.7f },
                { "VignetteSmoothness", 0.4f },
                { "Saturation", 0.7f },
                { "Contrast", 1.4f },
                { "Temperature", -0.2f },
                { "ChromaticAberration", 0.6f },
                { "FilmGrain", 0.2f }
            }
        },
        {
            EffectPreset.Dreamy, new Dictionary<string, float>
            {
                { "BloomIntensity", 0.9f },
                { "BloomThreshold", 0.6f },
                { "BloomBlur", 5.0f },
                { "VignetteIntensity", 0.2f },
                { "VignetteSmoothness", 0.8f },
                { "Saturation", 1.1f },
                { "Contrast", 0.95f },
                { "Temperature", 0.1f },
                { "ChromaticAberration", 0.2f },
                { "FilmGrain", 0.1f }
            }
        },
        {
            EffectPreset.Intense, new Dictionary<string, float>
            {
                { "BloomIntensity", 1.0f },
                { "BloomThreshold", 0.75f },
                { "BloomBlur", 3.5f },
                { "VignetteIntensity", 0.6f },
                { "VignetteSmoothness", 0.5f },
                { "Saturation", 1.15f },
                { "Contrast", 1.2f },
                { "Temperature", 0.0f },
                { "ChromaticAberration", 0.4f },
                { "FilmGrain", 0.08f }
            }
        },
        {
            EffectPreset.Subtle, new Dictionary<string, float>
            {
                { "BloomIntensity", 0.2f },
                { "BloomThreshold", 0.9f },
                { "BloomBlur", 1.0f },
                { "VignetteIntensity", 0.15f },
                { "VignetteSmoothness", 0.7f },
                { "Saturation", 1.0f },
                { "Contrast", 1.0f },
                { "Temperature", 0.0f },
                { "ChromaticAberration", 0.0f },
                { "FilmGrain", 0.0f }
            }
        },
        {
            EffectPreset.Noir, new Dictionary<string, float>
            {
                { "BloomIntensity", 0.1f },
                { "BloomThreshold", 0.95f },
                { "BloomBlur", 0.5f },
                { "VignetteIntensity", 0.8f },
                { "VignetteSmoothness", 0.3f },
                { "Saturation", 0.0f },
                { "Contrast", 1.5f },
                { "Temperature", 0.0f },
                { "ChromaticAberration", 0.1f },
                { "FilmGrain", 0.12f }
            }
        },
        {
            EffectPreset.Vibrant, new Dictionary<string, float>
            {
                { "BloomIntensity", 0.8f },
                { "BloomThreshold", 0.7f },
                { "BloomBlur", 2.5f },
                { "VignetteIntensity", 0.25f },
                { "VignetteSmoothness", 0.6f },
                { "Saturation", 1.4f },
                { "Contrast", 1.15f },
                { "Temperature", 0.05f },
                { "ChromaticAberration", 0.15f },
                { "FilmGrain", 0.0f }
            }
        },
        {
            EffectPreset.Muted, new Dictionary<string, float>
            {
                { "BloomIntensity", 0.15f },
                { "BloomThreshold", 0.85f },
                { "BloomBlur", 1.0f },
                { "VignetteIntensity", 0.35f },
                { "VignetteSmoothness", 0.5f },
                { "Saturation", 0.6f },
                { "Contrast", 0.9f },
                { "Temperature", -0.05f },
                { "ChromaticAberration", 0.0f },
                { "FilmGrain", 0.05f }
            }
        }
    };
    
    public static Dictionary<string, Color> EffectColors { get; } = new()
    {
        { "Fire", new Color(1.0f, 0.3f, 0.1f) },
        { "Ice", new Color(0.6f, 0.8f, 1.0f) },
        { "Lightning", new Color(1.0f, 1.0f, 0.4f) },
        { "Poison", new Color(0.4f, 0.8f, 0.2f) },
        { "Holy", new Color(1.0f, 0.9f, 0.5f) },
        { "Dark", new Color(0.3f, 0.1f, 0.4f) },
        { "Physical", new Color(0.7f, 0.5f, 0.3f) },
        { "Heal", new Color(0.3f, 1.0f, 0.5f) },
        { "Crit", new Color(1.0f, 0.8f, 0.0f) },
        { "Miss", new Color(0.5f, 0.5f, 0.5f) }
    };
    
    public static EffectPreset GetPreset(string name)
    {
        if (Enum.TryParse<EffectPreset>(name, true, out var preset))
            return preset;
        return EffectPreset.Default;
    }
    
    public static string[] GetPresetNames() => Enum.GetNames<EffectPreset>();
}
