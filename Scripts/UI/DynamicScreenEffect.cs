using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Dynamic screen effects based on game state
    /// - Vignette when health is low
    /// - Color overlay for damage types
    /// - Screen pulse for combo builds
    /// </summary>
    public partial class DynamicScreenEffect : Control
    {
        private static DynamicScreenEffect _instance;
        public static DynamicScreenEffect Instance => _instance;

        [Export] private ColorRect _vignetteRect;
        [Export] private ColorRect _damageOverlay;
        [Export] private ColorRect _comboPulse;
        
        private Tween _vignetteTween;
        private Tween _overlayTween;
        private Tween _comboTween;
        
        private float _targetVignetteOpacity = 0f;
        private Color _currentOverlayColor = Colors.Transparent;
        
        // Damage type overlay colors
        private readonly Color _fireColor = new Color(1f, 0.3f, 0.2f, 0.3f);
        private readonly Color _iceColor = new Color(0.3f, 0.5f, 1f, 0.3f);
        private readonly Color _lightningColor = new Color(1f, 1f, 0.3f, 0.25f);
        private readonly Color _poisonColor = new Color(0.2f, 0.8f, 0.2f, 0.25f);
        private readonly Color _shadowColor = new Color(0.3f, 0.1f, 0.4f, 0.3f);
        private readonly Color _holyColor = new Color(1f, 0.9f, 0.5f, 0.25f);
        
        public override void _Ready()
        {
            _instance = this;
            SetupEffectRects();
            ConnectSignals();
        }
        
        private void SetupEffectRects()
        {
            // Create vignette effect (dark edges)
            if (_vignetteRect == null)
            {
                _vignetteRect = new ColorRect
                {
                    Name = "Vignette",
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    Color = new Color(0, 0, 0, 0),
                    AnchorRight = 1f,
                    AnchorBottom = 1f
                };
                AddChild(_vignetteRect);
            }
            
            // Create damage type overlay
            if (_damageOverlay == null)
            {
                _damageOverlay = new ColorRect
                {
                    Name = "DamageOverlay",
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    Color = Colors.Transparent,
                    AnchorRight = 1f,
                    AnchorBottom = 1f
                };
                AddChild(_damageOverlay);
            }
            
            // Create combo pulse effect
            if (_comboPulse == null)
            {
                _comboPulse = new ColorRect
                {
                    Name = "ComboPulse",
                    MouseFilter = Control.MouseFilterEnum.Ignore,
                    Color = new Color(1f, 0.8f, 0.2f, 0f),
                    AnchorRight = 1f,
                    AnchorBottom = 1f
                };
                AddChild(_comboPulse);
            }
        }
        
        private void ConnectSignals()
        {
            // Connect to player health changes
            var player = GetTree().GetFirstNodeInGroup("Player") as Node;
            if (player != null)
            {
                CallDeferred(nameof(ConnectPlayerSignals), player);
            }
        }
        
        private void ConnectPlayerSignals(Node player)
        {
            // Monitor player health for vignette effect
        }
        
        public override void _Process(double delta)
        {
            UpdateVignetteEffect();
            UpdateOverlayEffect();
        }
        
        /// <summary>
        /// Update vignette based on player health percentage
        /// </summary>
        private void UpdateVignetteEffect()
        {
            var player = GetTree().GetFirstNodeInGroup("Player") as Player;
            if (player == null || _vignetteRect == null) return;
            
            float healthPercent = (float)player.CurrentHealth / Mathf.Max(1, player.MaxHealth);
            
            // Target opacity: 0% at full health, 70% at 10% health
            if (healthPercent > 0.5f)
            {
                _targetVignetteOpacity = 0f;
            }
            else
            {
                _targetVignetteOpacity = Mathf.Lerp(0f, 0.7f, 1f - (healthPercent * 2f));
            }
            
            // Smooth transition using Tween-like lerp
            float currentOpacity = _vignetteRect.Color.a;
            float newOpacity = Mathf.MoveToward(currentOpacity, _targetVignetteOpacity, 0.05f);
            _vignetteRect.Color = new Color(0, 0, 0, newOpacity);
        }
        
        /// <summary>
        /// Update damage type overlay (fades out over time)
        /// </summary>
        private void UpdateOverlayEffect()
        {
            if (_damageOverlay == null) return;
            
            // Smoothly fade out overlay
            if (_damageOverlay.Color.a > 0)
            {
                float newAlpha = Mathf.MoveToward(_damageOverlay.Color.a, 0f, 0.02f);
                _damageOverlay.Color = new Color(
                    _damageOverlay.Color.R,
                    _damageOverlay.Color.G,
                    _damageOverlay.Color.B,
                    newAlpha
                );
            }
        }
        
        /// <summary>
        /// Show damage type overlay
        /// </summary>
        public void ShowDamageTypeOverlay(string damageType)
        {
            if (_damageOverlay == null) return;
            
            Color targetColor = damageType.ToLower() switch
            {
                "fire" or "burning" => _fireColor,
                "ice" or "frozen" or "cold" => _iceColor,
                "lightning" or "shock" => _lightningColor,
                "poison" or "toxic" => _poisonColor,
                "shadow" or "dark" => _shadowColor,
                "holy" or "light" => _holyColor,
                _ => Colors.Transparent
            };
            
            if (targetColor != Colors.Transparent)
            {
                // Kill existing tween and start new one
                _overlayTween?.Kill();
                _damageOverlay.Color = targetColor;
                
                _overlayTween = CreateTween();
                _overlayTween.TweenProperty(_damageOverlay, "color:a", 0f, 1.5f);
            }
        }
        
        /// <summary>
        /// Trigger combo pulse effect
        /// </summary>
        public void TriggerComboPulse(int comboLevel)
        {
            if (_comboPulse == null || comboLevel < 5) return;
            
            // Intensity based on combo level
            float intensity = Mathf.Clamp(comboLevel / 50f, 0.1f, 0.5f);
            
            // Kill existing tween
            _comboTween?.Kill();
            _comboPulse.Color = new Color(1f, 0.8f, 0.2f, intensity);
            
            // Pulse animation
            _comboTween = CreateTween();
            _comboTween.SetParallel(true);
            _comboTween.TweenProperty(_comboPulse, "color:a", 0f, 0.3f);
            _comboTween.TweenProperty(_comboPulse, "rect_scale", new Vector2(1.1f, 1.1f), 0.15f);
            _comboTween.SetParallel(false);
            _comboTween.TweenProperty(_comboPulse, "rect_scale", Vector2.One, 0.15f);
        }
        
        /// <summary>
        /// Trigger screen shake based on damage
        /// </summary>
        public void TriggerDamageShake(float damageAmount)
        {
            var player = GetTree().GetFirstNodeInGroup("Player") as Player;
            if (player == null) return;
            
            // Calculate shake intensity based on damage
            float intensity = Mathf.Clamp(damageAmount / 100f, 0.5f, 3f);
            
            // Screen shake would be handled by ScreenShake component
            // This is a wrapper to trigger it
            var screenShake = GetTree().GetFirstNodeInGroup("ScreenShake") as Node;
            screenShake?.Call("TriggerShake", intensity);
        }
        
        /// <summary>
        /// Flash screen for important events
        /// </summary>
        public void FlashScreen(Color color, float duration = 0.3f)
        {
            if (_comboPulse == null) return;
            
            _comboTween?.Kill();
            _comboPulse.Color = color;
            
            _comboTween = CreateTween();
            _comboTween.TweenProperty(_comboPulse, "color:a", 0f, duration);
        }
    }
}
