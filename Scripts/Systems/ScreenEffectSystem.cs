using Godot;
using System;
using System.IO;
using System.Text.Json;

namespace ClawRPG.Scripts.Systems;

/// <summary>
/// Core screen effect system for post-processing
/// </summary>
public partial class ScreenEffectSystem : BaseSystem
{
    public static ScreenEffectSystem Instance { get; private set; }
    
    [Export] public bool Enabled { get; set; } = true;
    
    private ScreenEffectData _data = new();
    public ScreenEffectData Data => _data;
    
    private Camera2D _camera;
    private CanvasLayer _effectLayer;
    private ColorRect _flashOverlay;
    private Control _ui;
    
    // Screen effect nodes
    private ColorRect _vignetteOverlay;
    private ColorRect _colorOverlay;
    private Label _debugLabel;
    
    public override void _Ready()
    {
        Instance = this;
        LoadData();
        SetupEffectLayer();
    }
    
    private void SetupEffectLayer()
    {
        // Create canvas layer for screen effects
        _effectLayer = new CanvasLayer
        {
            Name = "ScreenEffectLayer",
            Layer = 100
        };
        AddChild(_effectLayer);
        
        var viewportSize = GetViewportRect().Size;
        
        // Flash overlay
        _flashOverlay = new ColorRect
        {
            Name = "FlashOverlay",
            Color = new Color(1, 1, 1, 0),
            Size = viewportSize,
            AnchorsPreset = Control.LayoutPreset.FullRect
        };
        _effectLayer.AddChild(_flashOverlay);
        
        // Vignette overlay
        _vignetteOverlay = new ColorRect
        {
            Name = "VignetteOverlay",
            Color = new Color(0, 0, 0, 0),
            Size = viewportSize,
            AnchorsPreset = Control.LayoutPreset.FullRect
        };
        _effectLayer.AddChild(_vignetteOverlay);
        
        // Color grading overlay
        _colorOverlay = new ColorRect
        {
            Name = "ColorOverlay",
            Color = new Color(0, 0, 0, 0),
            Size = viewportSize,
            AnchorsPreset = Control.LayoutPreset.FullRect
        };
        _effectLayer.AddChild(_colorOverlay);
        
        // Setup camera
        _camera = GetTree().CurrentScene?.GetNode<Camera2D>("Camera2D");
        if (_camera == null)
        {
            _camera = new Camera2D { Name = "Camera2D" };
            AddChild(_camera);
        }
    }
    
    public override void _Process(double delta)
    {
        if (!Enabled) return;
        
        float dt = (float)delta;
        
        // Update flash effect
        if (_data.FlashTimer > 0)
        {
            _data.FlashTimer -= dt;
            float progress = 1.0f - (_data.FlashTimer / _data.FlashDuration);
            
            // Fade in quickly, fade out slowly
            float intensity;
            if (progress < 0.1f)
                intensity = progress / 0.1f;
            else
                intensity = 1.0f - ((progress - 0.1f) / 0.9f) * 0.8f;
            
            _data.FlashIntensity = intensity * _data.FlashIntensity;
            UpdateFlashEffect();
        }
        
        // Update shake effect
        if (_data.ShakeTimer > 0)
        {
            _data.ShakeTimer -= dt;
            
            // Apply shake trauma decay
            if (_data.ShakeTrauma > 0)
            {
                _data.ShakeTrauma = Mathf.Max(0, _data.ShakeTrauma - dt * _data.ShakeDecay);
                _data.ShakeIntensity = _data.ShakeTrauma * _data.ShakeTrauma * 20.0f;
            }
            
            UpdateShakeEffect();
        }
        else
        {
            _data.ShakeOffset = Vector2.Zero;
            if (_camera != null)
                _camera.Offset = Vector2.Zero;
        }
        
        // Update vignette
        if (_data.VignetteEnabled)
        {
            UpdateVignette();
        }
        
        // Update color grading
        if (_data.ColorGradingEnabled)
        {
            UpdateColorGrading();
        }
    }
    
    #region Flash Effects
    
    /// <summary>
    /// Trigger a screen flash with specified color
    /// </summary>
    public void TriggerFlash(Color color, float intensity = 1.0f, float duration = 0.2f)
    {
        _data.FlashColor = color;
        _data.FlashIntensity = intensity;
        _data.FlashDuration = duration;
        _data.FlashTimer = duration;
        _data.TotalFlashes++;
        
        UpdateFlashEffect();
    }
    
    /// <summary>
    /// Trigger a damage flash (red)
    /// </summary>
    public void TriggerDamageFlash(float intensity = 0.6f, float duration = 0.15f)
    {
        TriggerFlash(new Color(1f, 0f, 0f), intensity, duration);
    }
    
    /// <summary>
    /// Trigger a heal flash (green)
    /// </summary>
    public void TriggerHealFlash(float intensity = 0.4f, float duration = 0.2f)
    {
        TriggerFlash(new Color(0f, 1f, 0f), intensity, duration);
    }
    
    /// <summary>
    /// Trigger a critical flash (yellow)
    /// </summary>
    public void TriggerCritFlash(float intensity = 0.8f, float duration = 0.25f)
    {
        TriggerFlash(new Color(1f, 0.9f, 0.2f), intensity, duration);
    }
    
    /// <summary>
    /// Trigger a magic flash (purple)
    /// </summary>
    public void TriggerMagicFlash(float intensity = 0.5f, float duration = 0.2f)
    {
        TriggerFlash(new Color(0.6f, 0.2f, 1f), intensity, duration);
    }
    
    private void UpdateFlashEffect()
    {
        if (_flashOverlay == null) return;
        
        var color = _data.FlashColor;
        color.A = _data.FlashIntensity;
        _flashOverlay.Color = color;
    }
    
    #endregion
    
    #region Shake Effects
    
    /// <summary>
    /// Trigger a screen shake with trauma-based intensity
    /// </summary>
    public void TriggerShake(float trauma, float duration = 0.5f)
    {
        _data.ShakeTrauma = Mathf.Clamp01(trauma);
        _data.ShakeDuration = duration;
        _data.ShakeTimer = duration;
        _data.ShakeIntensity = trauma * trauma * 20.0f;
        _data.TotalShakes++;
        _data.TotalShakeIntensity += trauma;
        
        UpdateShakeEffect();
    }
    
    /// <summary>
    /// Trigger a light shake
    /// </summary>
    public void TriggerLightShake()
    {
        TriggerShake(0.3f, 0.3f);
    }
    
    /// <summary>
    /// Trigger a medium shake
    /// </summary>
    public void TriggerMediumShake()
    {
        TriggerShake(0.6f, 0.5f);
    }
    
    /// <summary>
    /// Trigger a heavy shake
    /// </summary>
    public void TriggerHeavyShake()
    {
        TriggerShake(1.0f, 0.8f);
    }
    
    /// <summary>
    /// Trigger an explosion shake
    /// </summary>
    public void TriggerExplosionShake()
    {
        TriggerShake(1.5f, 1.0f);
    }
    
    private void UpdateShakeEffect()
    {
        if (_camera == null) return;
        
        // Perlin-like noise for smooth shake
        float time = Time.GetTicksMsec() / 1000.0f;
        float shakeX = (Mathf.Sin(time * 25.0f) * 0.5f + Mathf.Sin(time * 47.0f) * 0.3f + Mathf.Sin(time * 97.0f) * 0.2f);
        float shakeY = (Mathf.Sin(time * 31.0f) * 0.5f + Mathf.Sin(time * 53.0f) * 0.3f + Mathf.Sin(time * 89.0f) * 0.2f);
        
        _data.ShakeOffset = new Vector2(shakeX, shakeY) * _data.ShakeIntensity;
        _camera.Offset = _data.ShakeOffset;
    }
    
    #endregion
    
    #region Post-Processing
    
    private void UpdateVignette()
    {
        if (_vignetteOverlay == null) return;
        
        float intensity = _data.VignetteIntensity;
        float smoothness = _data.VignetteSmoothness;
        
        // Calculate vignette color (darker at edges)
        float vignetteAlpha = intensity * (1.0f - smoothness * 0.5f);
        _vignetteOverlay.Color = new Color(0, 0, 0, vignetteAlpha);
    }
    
    private void UpdateColorGrading()
    {
        if (_colorOverlay == null) return;
        
        // Apply color tint based on temperature
        float temp = _data.Temperature;
        Color tint;
        if (temp > 0)
            tint = new Color(1.0f + temp * 0.1f, 1.0f, 1.0f - temp * 0.1f);
        else
            tint = new Color(1.0f + temp * 0.1f, 1.0f + temp * 0.1f, 1.0f);
        
        tint.A = Mathf.Abs(temp) * 0.15f;
        _colorOverlay.Color = tint;
    }
    
    #endregion
    
    #region Bloom Control
    
    public void SetBloomEnabled(bool enabled)
    {
        _data.BloomEnabled = enabled;
        _data.EnabledEffects["Bloom"] = enabled;
    }
    
    public void SetBloomIntensity(float intensity)
    {
        _data.BloomIntensity = Mathf.Clamp01(intensity);
    }
    
    public void SetBloomThreshold(float threshold)
    {
        _data.BloomThreshold = Mathf.Clamp01(threshold);
    }
    
    #endregion
    
    #region Preset Control
    
    public void ApplyPreset(ScreenEffectDatabase.EffectPreset preset)
    {
        if (!ScreenEffectDatabase.Presets.TryGetValue(preset, out var settings))
            return;
        
        if (settings.TryGetValue("BloomIntensity", out var bi)) _data.BloomIntensity = bi;
        if (settings.TryGetValue("BloomThreshold", out var bt)) _data.BloomThreshold = bt;
        if (settings.TryGetValue("BloomBlur", out var bb)) _data.BloomBlur = bb;
        if (settings.TryGetValue("VignetteIntensity", out var vi)) _data.VignetteIntensity = vi;
        if (settings.TryGetValue("VignetteSmoothness", out var vs)) _data.VignetteSmoothness = vs;
        if (settings.TryGetValue("Saturation", out var sat)) _data.Saturation = sat;
        if (settings.TryGetValue("Contrast", out var con)) _data.Contrast = con;
        if (settings.TryGetValue("Temperature", out var temp)) _data.Temperature = temp;
        if (settings.TryGetValue("ChromaticAberration", out var ca)) _data.ChromaticAberrationAmount = ca;
        if (settings.TryGetValue("FilmGrain", out var fg)) _data.FilmGrainIntensity = fg;
        
        // Update enabled states
        _data.EnabledEffects["Bloom"] = _data.BloomIntensity > 0.1f;
        _data.EnabledEffects["Vignette"] = _data.VignetteIntensity > 0.05f;
        _data.EnabledEffects["ColorGrading"] = _data.Saturation != 1.0f || _data.Contrast != 1.0f;
        _data.EnabledEffects["ChromaticAberration"] = _data.ChromaticAberrationAmount > 0.1f;
        _data.EnabledEffects["FilmGrain"] = _data.FilmGrainIntensity > 0.02f;
    }
    
    #endregion
    
    #region Save/Load
    
    public void LoadData()
    {
        string path = GetSavePath();
        if (!System.IO.File.Exists(path)) return;
        
        try
        {
            string json = System.IO.File.ReadAllText(path);
            var loaded = JsonSerializer.Deserialize<ScreenEffectData>(json);
            if (loaded != null) _data = loaded;
        }
        catch (Exception e)
        {
            GD.PrintErr($"ScreenEffectSystem: Failed to load data: {e.Message}");
        }
    }
    
    public void SaveData()
    {
        try
        {
            string path = GetSavePath();
            var dir = Path.GetDirectoryName(path);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            var options = new JsonSerializerOptions { WriteIndented = true };
            string json = JsonSerializer.Serialize(_data, options);
            System.IO.File.WriteAllText(path, json);
        }
        catch (Exception e)
        {
            GD.PrintErr($"ScreenEffectSystem: Failed to save data: {e.Message}");
        }
    }
    
    private string GetSavePath()
    {
        return Path.Combine(ProjectSettings.GetSetting("application/config/name", "ClawRPG").ToString(), 
            "saves", "screen_effects.json");
    }
    
    #endregion
    
    #region Statistics
    
    public Dictionary<string, Variant> GetStatistics()
    {
        return new Dictionary<string, Variant>
        {
            { "TotalFlashes", _data.TotalFlashes },
            { "TotalShakes", _data.TotalShakes },
            { "AverageShakeIntensity", _data.TotalShakes > 0 ? _data.TotalShakeIntensity / _data.TotalShakes : 0 },
            { "BloomEnabled", _data.BloomEnabled },
            { "VignetteEnabled", _data.VignetteEnabled },
            { "CurrentPreset", "Default" }
        };
    }
    
    #endregion
    
    public override void _ExitTree()
    {
        SaveData();
        Instance = null;
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary<string, object>();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        // No persistent data needed
    }
}
