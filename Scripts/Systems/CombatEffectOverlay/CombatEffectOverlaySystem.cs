using Godot;
using System;
using System.Collections.Generic;

public class CombatEffectOverlaySystem : BaseSystem
{
    // Singleton instance
    private static CombatEffectOverlaySystem _instance;
    public static CombatEffectOverlaySystem Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = GetNode<CombatEffectOverlaySystem>("/root/CombatEffectOverlaySystem");
            }
            return _instance;
        }
    }
    
    // Data
    private CombatEffectOverlayData _data;
    public CombatEffectOverlayData Data => _data;
    
    // Database
    private CombatEffectOverlayDatabase _database;
    
    // Active effect timers
    private float _redOverlayTimer;
    private float _screenFlashTimer;
    private float _chromaticAberrationTimer;
    private float _vignetteTimer;
    private float _cameraShakeTimer;
    private float _slowMotionTimer;
    
    // Camera shake state
    private Vector3 _originalCameraPosition;
    private float _cameraShakeElapsed;
    
    // Time scale for slow motion
    private float _originalTimeScale = 1f;
    private float _targetTimeScale = 1f;
    
    // Random for variation
    private Random _random = new Random();
    
    public override void _Ready()
    {
        _database = CombatEffectOverlayDatabase.Instance;
        _data = new CombatEffectOverlayData();
        
        // Initialize timers
        _redOverlayTimer = 0f;
        _screenFlashTimer = 0f;
        _chromaticAberrationTimer = 0f;
        _vignetteTimer = 0f;
        _cameraShakeTimer = 0f;
        _slowMotionTimer = 0f;
    }
    
    public override void _Process(float delta)
    {
        UpdateScreenEffects(delta);
        UpdateCameraShake(delta);
        UpdateSlowMotion(delta);
        UpdateFloatingTexts(delta);
    }
    
    #region Screen Effects
    
    /// <summary>
    /// Trigger red overlay effect (low health warning)
    /// </summary>
    public void TriggerRedOverlay(float intensity = 0.3f, float duration = 1.0f)
    {
        _data.RedOverlayActive = true;
        _data.RedOverlayIntensity = Mathf.Clamp(intensity, 0f, 0.6f);
        _redOverlayTimer = duration;
    }
    
    /// <summary>
    /// Trigger screen flash effect
    /// </summary>
    public void TriggerScreenFlash(Color? color = null, float intensity = 0.8f, float duration = 0.3f)
    {
        _data.ScreenFlashActive = true;
        _data.ScreenFlashIntensity = Mathf.Clamp(intensity, 0f, 1f);
        _data.FlashColor = color ?? Colors.White;
        _screenFlashTimer = duration;
        _data.TotalScreenFlashes++;
    }
    
    /// <summary>
    /// Trigger chromatic aberration effect
    /// </summary>
    public void TriggerChromaticAberration(float intensity = 3f, float duration = 0.4f)
    {
        _data.ChromaticAberrationActive = true;
        _data.ChromaticAberrationIntensity = Mathf.Clamp(intensity, 0f, 10f);
        _chromaticAberrationTimer = duration;
    }
    
    /// <summary>
    /// Trigger vignette effect
    /// </summary>
    public void TriggerVignette(float intensity = 0.5f, float duration = 0.6f)
    {
        _data.VignetteActive = true;
        _data.VignetteIntensity = Mathf.Clamp(intensity, 0f, 1f);
        _vignetteTimer = duration;
    }
    
    private void UpdateScreenEffects(float delta)
    {
        // Update red overlay
        if (_data.RedOverlayActive)
        {
            _redOverlayTimer -= delta;
            if (_redOverlayTimer <= 0f)
            {
                _data.RedOverlayActive = false;
                _data.RedOverlayIntensity = 0f;
            }
        }
        
        // Update screen flash
        if (_data.ScreenFlashActive)
        {
            _screenFlashTimer -= delta;
            if (_screenFlashTimer <= 0f)
            {
                _data.ScreenFlashActive = false;
                _data.ScreenFlashIntensity = 0f;
            }
        }
        
        // Update chromatic aberration
        if (_data.ChromaticAberrationActive)
        {
            _chromaticAberrationTimer -= delta;
            if (_chromaticAberrationTimer <= 0f)
            {
                _data.ChromaticAberrationActive = false;
                _data.ChromaticAberrationIntensity = 0f;
            }
        }
        
        // Update vignette
        if (_data.VignetteActive)
        {
            _vignetteTimer -= delta;
            if (_vignetteTimer <= 0f)
            {
                _data.VignetteActive = false;
                _data.VignetteIntensity = 0f;
            }
        }
    }
    
    #endregion
    
    #region Camera Shake
    
    /// <summary>
    /// Trigger camera shake effect
    /// </summary>
    public void TriggerCameraShake(string configName = "medium")
    {
        var config = _database.GetCameraShakeConfig(configName);
        
        _data.CameraShakeActive = true;
        _data.CameraShakeIntensity = config.Intensity;
        _data.CameraShakeDuration = config.Duration;
        _cameraShakeTimer = config.Duration;
        _cameraShakeElapsed = 0f;
        
        _data.TotalCameraShakes++;
        _data.TotalShakeIntensity += config.Intensity;
        
        // Store original camera position
        var camera = GetViewport().GetCamera3D();
        if (camera != null)
        {
            _originalCameraPosition = camera.GlobalPosition;
        }
    }
    
    /// <summary>
    /// Trigger camera shake based on damage
    /// </summary>
    public void TriggerDamageShake(float damage, bool isCritical = false)
    {
        string configName;
        
        if (isCritical)
        {
            configName = damage > 100 ? "extreme" : "heavy";
        }
        else if (damage > 50)
        {
            configName = "heavy";
        }
        else if (damage > 20)
        {
            configName = "medium";
        }
        else
        {
            configName = "light";
        }
        
        TriggerCameraShake(configName);
    }
    
    private void UpdateCameraShake(float delta)
    {
        if (!_data.CameraShakeActive)
            return;
        
        _cameraShakeTimer -= delta;
        _cameraShakeElapsed += delta;
        
        if (_cameraShakeTimer <= 0f)
        {
            _data.CameraShakeActive = false;
            _data.CameraShakeIntensity = 0f;
            
            // Reset camera position
            var camera = GetViewport().GetCamera3D();
            if (camera != null)
            {
                camera.GlobalPosition = _originalCameraPosition;
            }
            return;
        }
        
        // Calculate shake intensity with decay
        float decay = 1f - (_cameraShakeElapsed / _data.CameraShakeDuration);
        float currentIntensity = _data.CameraShakeIntensity * decay;
        
        // Apply shake to camera
        var camera3D = GetViewport().GetCamera3D();
        if (camera3D != null)
        {
            float offsetX = (float)(_random.NextDouble() * 2 - 1) * currentIntensity;
            float offsetY = (float)(_random.NextDouble() * 2 - 1) * currentIntensity;
            float offsetZ = (float)(_random.NextDouble() * 2 - 1) * currentIntensity * 0.5f;
            
            camera3D.GlobalPosition = _originalCameraPosition + new Vector3(offsetX, offsetY, offsetZ);
        }
    }
    
    #endregion
    
    #region Slow Motion
    
    /// <summary>
    /// Trigger slow motion effect
    /// </summary>
    public void TriggerSlowMotion(string configName = "normal")
    {
        var config = _database.GetSlowMotionConfig(configName);
        
        _data.SlowMotionActive = true;
        _data.SlowMotionScale = config.Scale;
        _data.SlowMotionDuration = config.Duration;
        _slowMotionTimer = config.Duration;
        _targetTimeScale = config.Scale;
        
        _data.TotalSlowMotions++;
        
        // Apply time scale
        Engine.TimeScale = _targetTimeScale;
    }
    
    /// <summary>
    /// Trigger slow motion based on event
    /// </summary>
    public void TriggerEventSlowMotion(bool isCriticalHit, bool isBoss = false)
    {
        string configName;
        
        if (isBoss && isCriticalHit)
        {
            configName = "dramatic";
        }
        else if (isCriticalHit)
        {
            configName = "normal";
        }
        else
        {
            configName = "quick";
        }
        
        TriggerSlowMotion(configName);
    }
    
    private void UpdateSlowMotion(float delta)
    {
        if (!_data.SlowMotionActive)
            return;
        
        _slowMotionTimer -= delta * _data.SlowMotionScale; // Scale timer by slow motion
        
        if (_slowMotionTimer <= 0f)
        {
            _data.SlowMotionActive = false;
            _data.SlowMotionScale = 1f;
            _targetTimeScale = 1f;
            
            // Restore time scale
            Engine.TimeScale = 1f;
        }
    }
    
    #endregion
    
    #region Floating Text
    
    /// <summary>
    /// Spawn floating text at position
    /// </summary>
    public void SpawnFloatingText(string text, Vector2 screenPosition, FloatingTextType type, float sizeMultiplier = 1f)
    {
        var config = _database.GetFloatingTextConfig(GetConfigNameForType(type));
        var color = _database.GetFloatingTextColor(type);
        
        // Add some randomness to position
        float randomOffsetX = (float)((_random.NextDouble() * 2 - 1) * config.RandomVelocityRange * 0.5f);
        float randomOffsetY = (float)((_random.NextDouble() * 2 - 1) * config.RandomVelocityRange * 0.5f);
        Vector2 velocity = config.Velocity + new Vector2(randomOffsetX, randomOffsetY);
        
        // Calculate size with variation
        float sizeVariation = (float)(_random.NextDouble() * 2 - 1) * (config.MaxSize - config.MinSize) * 0.3f;
        float finalSize = Mathf.Clamp(config.BaseSize * sizeMultiplier + sizeVariation, config.MinSize, config.MaxSize);
        
        var floatingText = new FloatingTextData(
            text,
            screenPosition,
            color,
            finalSize,
            config.Lifetime,
            velocity,
            type
        );
        
        _data.ActiveFloatingTexts.Add(floatingText);
        _data.TotalFloatingTexts++;
    }
    
    /// <summary>
    /// Spawn damage text
    /// </summary>
    public void SpawnDamageText(float damage, Vector2 screenPosition, bool isCritical = false)
    {
        var type = isCritical ? FloatingTextType.CriticalDamage : FloatingTextType.Damage;
        string prefix = isCritical ? "CRIT! " : "";
        SpawnFloatingText(prefix + Mathf.FloorToInt(damage).ToString(), screenPosition, type);
        
        // Trigger camera shake for critical hits
        if (isCritical)
        {
            TriggerDamageShake(damage, true);
            TriggerSlowMotion("quick");
        }
        else if (damage > 30)
        {
            TriggerDamageShake(damage, false);
        }
    }
    
    /// <summary>
    /// Spawn heal text
    /// </summary>
    public void SpawnHealText(float amount, Vector2 screenPosition)
    {
        SpawnFloatingText("+" + Mathf.FloorToInt(amount).ToString(), screenPosition, FloatingTextType.Heal);
    }
    
    /// <summary>
    /// Spawn miss text
    /// </summary>
    public void SpawnMissText(Vector2 screenPosition)
    {
        SpawnFloatingText("MISS", screenPosition, FloatingTextType.Miss);
    }
    
    /// <summary>
    /// Spawn experience text
    /// </summary>
    public void SpawnExperienceText(int amount, Vector2 screenPosition)
    {
        SpawnFloatingText("+" + amount + " XP", screenPosition, FloatingTextType.Experience);
    }
    
    /// <summary>
    /// Spawn gold text
    /// </summary>
    public void SpawnGoldText(int amount, Vector2 screenPosition)
    {
        SpawnFloatingText("+" + amount + " G", screenPosition, FloatingTextType.Gold);
    }
    
    private string GetConfigNameForType(FloatingTextType type)
    {
        switch (type)
        {
            case FloatingTextType.CriticalDamage:
                return "damage_critical";
            case FloatingTextType.Heal:
                return "heal";
            case FloatingTextType.Miss:
            case FloatingTextType.Block:
            case FloatingTextType.Dodge:
                return "miss";
            case FloatingTextType.Experience:
                return "experience";
            case FloatingTextType.Gold:
                return "gold";
            case FloatingTextType.Buff:
            case FloatingTextType.Debuff:
                return "heal";
            default:
                return "damage_small";
        }
    }
    
    private void UpdateFloatingTexts(float delta)
    {
        for (int i = _data.ActiveFloatingTexts.Count - 1; i >= 0; i--)
        {
            var text = _data.ActiveFloatingTexts[i];
            text.Age += delta;
            
            // Update position
            text.Position += text.Velocity * delta;
            
            // Add gravity
            text.Velocity += new Vector2(0, 100f) * delta;
            
            // Remove expired texts
            if (text.Age >= text.Lifetime)
            {
                _data.ActiveFloatingTexts.RemoveAt(i);
            }
        }
    }
    
    #endregion
    
    #region Combat Integration
    
    /// <summary>
    /// Handle damage dealt to enemy
    /// </summary>
    public void OnDamageDealt(float damage, Vector2 screenPosition, bool isCritical, bool isBoss = false)
    {
        SpawnDamageText(damage, screenPosition, isCritical);
        
        // Visual effects based on damage type
        if (isCritical)
        {
            TriggerScreenFlash(new Color(1f, 0.3f, 0.3f), 0.6f, 0.2f);
            TriggerChromaticAberration(5f, 0.3f);
            TriggerVignette(0.4f, 0.3f);
            TriggerSlowMotion("quick");
        }
        else if (damage > 50 || isBoss)
        {
            TriggerScreenFlash(Colors.White, 0.3f, 0.15f);
        }
        
        TriggerDamageShake(damage, isCritical);
    }
    
    /// <summary>
    /// Handle healing
    /// </summary>
    public void OnHeal(float amount, Vector2 screenPosition)
    {
        SpawnHealText(amount, screenPosition);
        TriggerScreenFlash(new Color(0.3f, 1f, 0.5f), 0.3f, 0.15f);
    }
    
    /// <summary>
    /// Handle player taking damage
    /// </summary>
    public void OnPlayerDamaged(float damage, float currentHealth, float maxHealth)
    {
        // Red overlay when low health
        if (currentHealth / maxHealth < 0.25f)
        {
            TriggerRedOverlay(0.5f + (0.1f * (1f - currentHealth / maxHealth)), 0.5f);
        }
        else if (currentHealth / maxHealth < 0.5f)
        {
            TriggerRedOverlay(0.3f, 0.3f);
        }
        
        if (damage > 30)
        {
            TriggerScreenFlash(new Color(1f, 0f, 0f), 0.4f, 0.2f);
            TriggerCameraShake(damage > 50 ? "heavy" : "medium");
        }
    }
    
    /// <summary>
    /// Handle enemy defeated
    /// </summary>
    public void OnEnemyDefeated(Vector2 screenPosition)
    {
        TriggerScreenFlash(new Color(1f, 1f, 0.5f), 0.5f, 0.3f);
        TriggerCameraShake("light");
    }
    
    #endregion
    
    #region Statistics
    
    public int GetTotalScreenFlashes() => _data.TotalScreenFlashes;
    public int GetTotalCameraShakes() => _data.TotalCameraShakes;
    public int GetTotalSlowMotions() => _data.TotalSlowMotions;
    public int GetTotalFloatingTexts() => _data.TotalFloatingTexts;
    public float GetTotalShakeIntensity() => _data.TotalShakeIntensity;
    
    public void ResetStatistics()
    {
        _data.TotalScreenFlashes = 0;
        _data.TotalCameraShakes = 0;
        _data.TotalSlowMotions = 0;
        _data.TotalFloatingTexts = 0;
        _data.TotalShakeIntensity = 0f;
    }
    
    #endregion
    
    #region Save/Load
    
    public Dictionary<string, object> GetSaveData()
    {
        return new Dictionary<string, object>
        {
            { "totalScreenFlashes", _data.TotalScreenFlashes },
            { "totalCameraShakes", _data.TotalCameraShakes },
            { "totalSlowMotions", _data.TotalSlowMotions },
            { "totalFloatingTexts", _data.TotalFloatingTexts },
            { "totalShakeIntensity", _data.TotalShakeIntensity }
        };
    }
    
    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("totalScreenFlashes"))
            _data.TotalScreenFlashes = Convert.ToInt32(data["totalScreenFlashes"]);
        if (data.ContainsKey("totalCameraShakes"))
            _data.TotalCameraShakes = Convert.ToInt32(data["totalCameraShakes"]);
        if (data.ContainsKey("totalSlowMotions"))
            _data.TotalSlowMotions = Convert.ToInt32(data["totalSlowMotions"]);
        if (data.ContainsKey("totalFloatingTexts"))
            _data.TotalFloatingTexts = Convert.ToInt32(data["totalFloatingTexts"]);
        if (data.ContainsKey("totalShakeIntensity"))
            _data.TotalShakeIntensity = Convert.ToSingle(data["totalShakeIntensity"]);
    }
    
    #endregion
}
