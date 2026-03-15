using Godot;
using System;

/// <summary>
/// 屏幕特效管理器 - 管理战斗屏幕特效
/// </summary>
public partial class ScreenEffectManager : Node
{
    public static ScreenEffectManager Instance { get; private set; }
    
    [Export] private ShaderMaterial _shockwaveMaterial;
    private ColorRect _fullscreenRect;
    private double _time = 0.0;
    private bool _isActive = false; 
    private double _effectDuration = 0.0;
    private double _currentEffectTime = 0.0;
    
    // Shockwave parameters
    private float _shockwaveStrength = 0.0f;
    private float _shockwaveRadius = 0.5f;
    private float _shockwaveSpeed = 2.0f;
    private Vector2 _shockwaveCenter = new Vector2(0.5f, 0.5f);
    private float _chromaticAmount = 0.0f;
    
    public override void _Ready()
    {
        Instance = this;
        SetupFullscreenEffect();
    }
    
    private void SetupFullscreenEffect()
    {
        _fullscreenRect = new ColorRect();
        _fullscreenRect.SetAnchorPreset(Control.LayoutPreset.FullRect);
        _fullscreenRect.MouseFilter = Control.MouseFilterEnum.Ignore;
        
        // Load shockwave shader
        var shader = GD.Load<Shader>("res://Shaders/shockwave.gdshader");
        _shockwaveMaterial = new ShaderMaterial();
        _shockwaveMaterial.Shader = shader;
        _fullscreenRect.Material = _shockwaveMaterial;
        
        // Add to canvas layer for post-processing
        var canvasLayer = new CanvasLayer();
        canvasLayer.Layer = 100; // Topmost layer
        canvasLayer.AddChild(_fullscreenRect);
        AddChild(canvasLayer);
        
        // Initially hidden
        _fullscreenRect.Visible = false; 
        UpdateShockwaveUniforms();
    }
    
    private void UpdateShockwaveUniforms()
    {
        if (_shockwaveMaterial == null) return;
        
        _shockwaveMaterial.SetShaderParameter("shockwave_strength", _shockwaveStrength);
        _shockwaveMaterial.SetShaderParameter("shockwave_radius", _shockwaveRadius);
        _shockwaveMaterial.SetShaderParameter("shockwave_speed", _shockwaveSpeed);
        _shockwaveMaterial.SetShaderParameter("shockwave_center", new Vector2(_shockwaveCenter.X, _shockwaveCenter.Y));
        _shockwaveMaterial.SetShaderParameter("time", (float)_time);
        _shockwaveMaterial.SetShaderParameter("chromatic_amount", _chromaticAmount);
    }
    
    public override void _Process(double delta)
    {
        _time += delta;
        
        if (_isActive)
        {
            _currentEffectTime += delta;
            
            // Expand radius over time
            _shockwaveRadius += (float)(delta * 0.3f);
            
            // Fade out strength
            _shockwaveStrength = Mathf.Lerp(_shockwaveStrength, 0.0f, (float)(delta * 2.0f));
            
            // Fade chromatic
            _chromaticAmount = Mathf.Lerp(_chromaticAmount, 0.0f, (float)(delta * 1.5f));
            
            UpdateShockwaveUniforms();
            
            // End effect when duration reached
            if (_currentEffectTime >= _effectDuration)
            {
                StopEffect();
            }
        }
    }
    
    /// <summary>
    /// Trigger a shockwave effect at screen center
    /// </summary>
    /// <param name="strength">Effect strength (0-1)</param>
    /// <param name="duration">Effect duration in seconds</param>
    /// <param name="chromatic">Chromatic aberration amount</param>
    public void TriggerShockwave(float strength = 0.5f, double duration = 1.5f, float chromatic = 0.02f)
    {
        _shockwaveStrength = strength;
        _shockwaveRadius = 0.1f;
        _shockwaveSpeed = 2.0f;
        _shockwaveCenter = new Vector2(0.5f, 0.5f);
        _chromaticAmount = chromatic;
        _effectDuration = duration;
        _currentEffectTime = 0.0;
        
        _fullscreenRect.Visible = true;
        _isActive = true;
        
        UpdateShockwaveUniforms();
    }
    
    /// <summary>
    /// Trigger a shockwave effect at a specific screen position
    /// </summary>
    /// <param name="screenPosition">Screen position (0-1 range)</param>
    /// <param name="strength">Effect strength (0-1)</param>
    /// <param name="duration">Effect duration in seconds</param>
    public void TriggerShockwaveAt(Vector2 screenPosition, float strength = 0.5f, double duration = 1.5f)
    {
        _shockwaveStrength = strength;
        _shockwaveRadius = 0.1f;
        _shockwaveSpeed = 2.0f;
        _shockwaveCenter = screenPosition;
        _chromaticAmount = 0.02f;
        _effectDuration = duration;
        _currentEffectTime = 0.0;
        
        _fullscreenRect.Visible = true;
        _isActive = true;
        
        UpdateShockwaveUniforms();
    }
    
    /// <summary>
    /// Trigger a shockwave from a world position (converts to screen position)
    /// </summary>
    /// <param name="worldPosition">World position in game coordinates</param>
    /// <param name="camera">Active camera for conversion</param>
    /// <param name="strength">Effect strength (0-1)</param>
    /// <param name="duration">Effect duration in seconds</param>
    public void TriggerShockwaveFromWorld(Vector2 worldPosition, Camera2D camera, float strength = 0.5f, double duration = 1.5f)
    {
        if (camera == null)
        {
            TriggerShockwave(strength, duration);
            return;
        }
        
        var screenPos = camera.GetScreenCenterPosition();
        var viewportSize = GetViewportRect().Size;
        
        // Convert world position to normalized screen coordinates
        var offset = worldPosition - screenPos;
        var normalizedX = 0.5f + (offset.X / viewportSize.X);
        var normalizedY = 0.5f + (offset.Y / viewportSize.Y);
        
        normalizedX = Mathf.Clamp(normalizedX, 0.0f, 1.0f);
        normalizedY = Mathf.Clamp(normalizedY, 0.0f, 1.0f);
        
        TriggerShockwaveAt(new Vector2(normalizedX, normalizedY), strength, duration);
    }
    
    private void StopEffect()
    {
        _isActive = false; 
        _fullscreenRect.Visible = false; 
        _shockwaveStrength = 0.0f;
        _chromaticAmount = 0.0f;
    }
    
    /// <summary>
    /// Immediately stop all screen effects
    /// </summary>
    public void ClearEffects()
    {
        StopEffect();
        _time = 0.0;
    }
}
