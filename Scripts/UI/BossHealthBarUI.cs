using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.BossMechanics;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Boss血条UI系统 - 在Boss战斗时显示Boss血量
    /// </summary>
    public partial class BossHealthBarUI : Control
    {
        // UI Components
        private PanelContainer _container;
        private ProgressBar _healthBar;
        private Label _bossNameLabel;
        private Label _healthLabel;
        private Label _phaseLabel;
        private TextureRect _bossIcon;

        // Enrage UI Components
        private ProgressBar _enrageBar;
        private Label _enrageLabel;
        private Label _enrageWarningLabel;

        // REQ-160: Intent display components
        private HBoxContainer _intentContainer;
        private Label _intentLabel;
        private TextureRect _intentIcon;
        private Label _intentDamageLabel;
        private BossIntentData _currentIntent;
        
        // State
        private Node2D _currentBoss;
        private bool _isVisible = false; 
        private float _displayTimer = 0f;
        private float _fadeTimer = 0f;
        private bool _wasEnraged = false; 
        private int _lastEnrageSecond = -1;
        private const float AutoHideTime = 3f;
        private const float FadeTime = 0.5f;
        
        // Colors
        private Color _healthColorNormal = new Color(0.2f, 0.8f, 0.2f);
        private Color _healthColorWarning = new Color(0.9f, 0.7f, 0.1f);
        private Color _healthColorCritical = new Color(0.9f, 0.2f, 0.2f);
        private Color _enrageColorWarning = new Color(1f, 0.3f, 0f, 0.8f);
        private Color _enrageColorActive = new Color(1f, 0f, 0f, 1f);

        // Confidence floor (REQ-119)
        private const float ConfidenceFloorWarning = 0.30f;  // 30%: orange warning
        private const float ConfidenceFloorCritical = 0.15f; // 15%: red danger
        private bool _isPulsing = false;

        // REQ-160: Intent type colors
        private readonly Color _intentDamage = new Color(0.95f, 0.2f, 0.2f);   // Red
        private readonly Color _intentBuff = new Color(0.95f, 0.85f, 0.2f);   // Yellow
        private readonly Color _intentDebuff = new Color(0.7f, 0.2f, 0.9f);   // Purple
        private readonly Color _intentDefend = new Color(0.2f, 0.5f, 0.95f);  // Blue
        private readonly Color _intentSpecial = new Color(0.4f, 0.1f, 0.5f);  // Dark purple
        private readonly Color _intentEnraged = new Color(1f, 0.3f, 0f);      // Orange-red for enrage
        
        public override void _Ready()
        {
            SetupUI();
            Visible = false; 
        }
        
        private void SetupUI()
        {
            // Main container
            _container = new PanelContainer();
            _container.SetAnchorsPreset(Control.LayoutPreset.CenterTop);
            _container.Position = new Vector2(-200, 20);
            _container.Size = new Vector2(400, 80);
            AddChild(_container);
            
            // StyleBox for container
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.1f, 0.1f, 0.1f, 0.9f);
            panelStyle.BorderWidthLeft = 2;
            panelStyle.BorderWidthRight = 2;
            panelStyle.BorderWidthTop = 2;
            panelStyle.BorderWidthBottom = 2;
            panelStyle.BorderColor = new Color(0.6f, 0.4f, 0.1f, 1f);
            panelStyle.CornerRadiusTopLeft = 8;
            panelStyle.CornerRadiusTopRight = 8;
            panelStyle.CornerRadiusBottomLeft = 8;
            panelStyle.CornerRadiusBottomRight = 8;
            _container.AddThemeStyleboxOverride("panel", panelStyle);
            
            // VBoxContainer for content
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 5);
            _container.AddChild(vbox);
            
            // Boss name and icon row
            var nameHBox = new HBoxContainer();
            nameHBox.Alignment = BoxContainer.AlignmentMode.Center;
            vbox.AddChild(nameHBox);
            
            // Boss icon placeholder
            _bossIcon = new TextureRect();
            _bossIcon.CustomMinimumSize = new Vector2(32, 32);
            _bossIcon.Modulate = new Color(1f, 0.8f, 0.4f);
            nameHBox.AddChild(_bossIcon);
            
            // Boss name
            _bossNameLabel = new Label();
            _bossNameLabel.Text = "Boss Name";
            _bossNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _bossNameLabel.AddThemeFontSizeOverride("font_size", 18);
            _bossNameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.7f));
            nameHBox.AddChild(_bossNameLabel);

            // REQ-160: Intent display row — hidden until boss selects an ability
            _intentContainer = new HBoxContainer();
            _intentContainer.Alignment = BoxContainer.AlignmentMode.Center;
            _intentContainer.HorizontalAlignment = HorizontalAlignment.Center;
            _intentContainer.Visible = false;
            _intentContainer.CustomMinimumSize = new Vector2(360, 28);
            vbox.AddChild(_intentContainer);

            _intentIcon = new TextureRect();
            _intentIcon.CustomMinimumSize = new Vector2(20, 20);
            _intentIcon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            _intentContainer.AddChild(_intentIcon);

            _intentLabel = new Label();
            _intentLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _intentLabel.AddThemeFontSizeOverride("font_size", 13);
            _intentLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            _intentContainer.AddChild(_intentLabel);

            _intentDamageLabel = new Label();
            _intentDamageLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _intentDamageLabel.AddThemeFontSizeOverride("font_size", 12);
            _intentDamageLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.6f));
            _intentContainer.AddChild(_intentDamageLabel);

            // Health bar
            _healthBar = new ProgressBar();
            _healthBar.CustomMinimumSize = new Vector2(360, 24);
            _healthBar.MinValue = 0;
            _healthBar.MaxValue = 100;
            _healthBar.Value = 100;
            _healthBar.ShowPercentage = false; 
            
            // Health bar style
            var healthBg = new StyleBoxFlat();
            healthBg.BgColor = new Color(0.2f, 0.2f, 0.2f, 1f);
            healthBg.CornerRadiusTopLeft = 4;
            healthBg.CornerRadiusTopRight = 4;
            healthBg.CornerRadiusBottomLeft = 4;
            healthBg.CornerRadiusBottomRight = 4;
            _healthBar.AddThemeStyleboxOverride("background", healthBg);
            
            var healthFill = new StyleBoxFlat();
            healthFill.BgColor = _healthColorNormal;
            healthFill.CornerRadiusTopLeft = 4;
            healthFill.CornerRadiusTopRight = 4;
            healthFill.CornerRadiusBottomLeft = 4;
            healthFill.CornerRadiusBottomRight = 4;
            _healthBar.AddThemeStyleboxOverride("fill", healthFill);
            
            vbox.AddChild(_healthBar);
            
            // Health label (current/max)
            _healthLabel = new Label();
            _healthLabel.Text = "100 / 100";
            _healthLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _healthLabel.AddThemeFontSizeOverride("font_size", 14);
            _healthLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            vbox.AddChild(_healthLabel);
            
            // Phase label
            _phaseLabel = new Label();
            _phaseLabel.Text = "";
            _phaseLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _phaseLabel.AddThemeFontSizeOverride("font_size", 12);
            _phaseLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
            vbox.AddChild(_phaseLabel);
            
            // Enrage warning label (shows when boss is about to enrage)
            _enrageWarningLabel = new Label();
            _enrageWarningLabel.Text = "";
            _enrageWarningLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _enrageWarningLabel.AddThemeFontSizeOverride("font_size", 11);
            _enrageWarningLabel.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0f));
            vbox.AddChild(_enrageWarningLabel);
            
            // Enrage progress bar
            _enrageBar = new ProgressBar();
            _enrageBar.CustomMinimumSize = new Vector2(360, 8);
            _enrageBar.MinValue = 0;
            _enrageBar.MaxValue = 100;
            _enrageBar.Value = 0;
            _enrageBar.ShowPercentage = false; 
            _enrageBar.Visible = false; 
            
            var enrageBg = new StyleBoxFlat();
            enrageBg.BgColor = new Color(0.15f, 0.15f, 0.15f, 1f);
            enrageBg.CornerRadiusTopLeft = 3;
            enrageBg.CornerRadiusTopRight = 3;
            enrageBg.CornerRadiusBottomLeft = 3;
            enrageBg.CornerRadiusBottomRight = 3;
            _enrageBar.AddThemeStyleboxOverride("background", enrageBg);
            
            var enrageFill = new StyleBoxFlat();
            enrageFill.BgColor = _enrageColorWarning;
            enrageFill.CornerRadiusTopLeft = 3;
            enrageFill.CornerRadiusTopRight = 3;
            enrageFill.CornerRadiusBottomLeft = 3;
            enrageFill.CornerRadiusBottomRight = 3;
            _enrageBar.AddThemeStyleboxOverride("fill", enrageFill);
            
            vbox.AddChild(_enrageBar);
            
            // Enrage label
            _enrageLabel = new Label();
            _enrageLabel.Text = "";
            _enrageLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _enrageLabel.AddThemeFontSizeOverride("font_size", 11);
            _enrageLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0f));
            _enrageLabel.Visible = false; 
            vbox.AddChild(_enrageLabel);
        }
        
        public override void _Process(double delta)
        {
            if (_currentBoss == null || !IsInstanceValid(_currentBoss))
            {
                if (_isVisible && _displayTimer <= 0)
                {
                    StartFadeOut();
                }
                return;
            }
            
            // Update health display
            UpdateBossHealth();
            
            // Handle timers
            if (_displayTimer > 0)
            {
                _displayTimer -= delta;
            }
            
            // Handle fade
            if (_fadeTimer > 0)
            {
                _fadeTimer -= delta;
                float alpha = Mathf.Clamp(_fadeTimer / FadeTime, 0f, 1f);
                _container.Modulate = new Color(1f, 1f, 1f, alpha);
                
                if (_fadeTimer <= 0)
                {
                    _isVisible = false;
                    Visible = false;
                }
            }

            // Danger pulse (REQ-119) — combine with fade alpha
            if (_isPulsing)
            {
                PulseTimer += delta;
                float pulse = Mathf.Sin(PulseTimer * Mathf.Pi * 2f / PulseInterval) * 0.3f + 0.7f;
                float alpha = _fadeTimer > 0 ? Mathf.Clamp(_fadeTimer / FadeTime, 0f, 1f) : 1f;
                _container.Modulate = new Color(pulse, pulse * 0.5f, pulse * 0.5f, alpha);
            }
        }
        
        private void UpdateBossHealth()
        {
            // Get boss health from Boss script
            var boss = _currentBoss as Scripts.Boss;
            if (boss == null) return;
            
            float currentHealth = boss.CurrentHealth;
            float maxHealth = boss.MaxHealth;
            
            // Update progress bar
            _healthBar.MaxValue = maxHealth;
            _healthBar.Value = currentHealth;
            
            // Update health text
            _healthLabel.Text = $"{(int)currentHealth} / {(int)maxHealth}";
            
            // Update health bar color based on percentage (REQ-119: confidence floor)
            float healthPercent = currentHealth / maxHealth;
            StyleBoxFlat fillStyle = _healthBar.GetThemeStylebox("fill") as StyleBoxFlat;
            if (fillStyle != null)
            {
                Color targetColor;
                bool shouldPulse = false;
                if (healthPercent > ConfidenceFloorWarning)
                {
                    targetColor = _healthColorNormal;
                }
                else if (healthPercent > ConfidenceFloorCritical)
                {
                    targetColor = _healthColorWarning;
                }
                else
                {
                    targetColor = _healthColorCritical;
                    shouldPulse = true;
                }

                // Smooth transition via Tween
                if (fillStyle.BgColor != targetColor)
                {
                    Tween tween = CreateTween();
                    tween.TweenProperty(fillStyle, "bg_color", targetColor, 0.3f);
                }

                // Danger pulse effect (red pulsing when <= 15%)
                UpdateDangerPulse(shouldPulse);
            }
            
            // Update phase if boss has phases
            if (boss.GetCurrentPhase() > 1)
            {
                _phaseLabel.Text = $"Phase {boss.GetCurrentPhase()}";
            }
            else
            {
                _phaseLabel.Text = "";
            }
            
            // Update enrage display
            UpdateEnrageDisplay(boss, delta);
        }
        
        private void UpdateDangerPulse(bool shouldPulse)
        {
            if (shouldPulse && !_isPulsing)
            {
                _isPulsing = true;
                // Start pulsing animation
                PulseTimer = 0f;
            }
            else if (!shouldPulse && _isPulsing)
            {
                _isPulsing = false;
                // Restore normal appearance
                _container.Modulate = new Color(1f, 1f, 1f, 1f);
            }
        }

        private float PulseTimer = 0f;
        private const float PulseInterval = 0.5f;

        private void UpdateEnrageDisplay(Scripts.Boss boss, float delta)
        {
            bool isEnraged = boss.IsEnraged();
            float enrageTimeRemaining = boss.GetEnrageTimeRemaining();
            float enrageTime = boss.EnrageTime;
            
            // Check if enrage state changed
            if (isEnraged != _wasEnraged)
            {
                _wasEnraged = isEnraged;
                if (isEnraged)
                {
                    // Boss just became enraged - trigger visual effect
                    OnBossEnraged();
                }
            }
            
            if (isEnraged)
            {
                // Boss is enraged - show enrage active state
                _enrageBar.Visible = true;
                _enrageLabel.Visible = true;
                _enrageBar.Value = 100;
                _enrageLabel.Text = "⚠️ ENRAGED! ⚠️";
                _enrageLabel.AddThemeColorOverride("font_color", _enrageColorActive);
                _enrageWarningLabel.Text = "";
                
                // Pulsing effect for enrage
                float pulse = Mathf.Sin(Time.GetTicksMsec() * 0.01f) * 0.3f + 0.7f;
                _enrageBar.Modulate = new Color(1f, pulse * 0.3f, pulse * 0.3f, 1f);
            }
            else if (enrageTimeRemaining > 0 && enrageTime > 0)
            {
                // Show enrage countdown when boss health is low (< 30%) or enrage is imminent (< 30s)
                float healthPercent = boss.CurrentHealth / boss.MaxHealth;
                float enragePercentRemaining = (enrageTimeRemaining / enrageTime) * 100f;
                
                if (healthPercent < 0.3f || enrageTimeRemaining < 30f)
                {
                    _enrageBar.Visible = true;
                    _enrageBar.Value = enragePercentRemaining;
                    
                    // Warning color when enrage is close
                    StyleBoxFlat enrageFill = _enrageBar.GetThemeStylebox("fill") as StyleBoxFlat;
                    if (enrageFill != null)
                    {
                        if (enrageTimeRemaining < 10f)
                        {
                            enrageFill.BgColor = _enrageColorActive;
                        }
                        else
                        {
                            enrageFill.BgColor = _enrageColorWarning;
                        }
                    }
                    
                    // Show warning text
                    if (enrageTimeRemaining < 10f)
                    {
                        _enrageWarningLabel.Text = "⚡ ENRAGE IMMINENT! ⚡";
                        _enrageLabel.Text = $"Enrage: {(int)enrageTimeRemaining}s";
                        
                        // Warning effect when enrage is imminent - light shake every second
                        int currentSecond = (int)enrageTimeRemaining;
                        if (currentSecond != _lastEnrageSecond)
                        {
                            _lastEnrageSecond = currentSecond;
                            var cameraEffect = GetNodeOrNull<CameraEffectSystem>("/root/Main/CameraEffectSystem");
                            if (cameraEffect != null)
                            {
                                cameraEffect.TriggerLightShake();
                            }
                        }
                    }
                    else
                    {
                        _enrageWarningLabel.Text = "";
                        _enrageLabel.Text = $"Enrage: {(int)enrageTimeRemaining}s";
                        _lastEnrageSecond = -1; // Reset when not in warning zone
                    }
                    
                    _enrageLabel.Visible = true;
                    _enrageBar.Modulate = new Color(1f, 1f, 1f, 1f);
                }
                else
                {
                    // Hide enrage display
                    _enrageBar.Visible = false; 
                    _enrageLabel.Visible = false; 
                    _enrageWarningLabel.Text = "";
                }
            }
            else
            {
                // No enrage system or already enraged
                _enrageBar.Visible = false; 
                _enrageLabel.Visible = false; 
                _enrageWarningLabel.Text = "";
            }
        }
        
        private void OnBossEnraged()
        {
            // Visual feedback when boss becomes enraged
            GD.Print("Boss has become ENRAGED!");
            
            // Trigger screen shake effect - violent shake for enrage
            var cameraEffect = GetNodeOrNull<CameraEffectSystem>("/root/Main/CameraEffectSystem");
            if (cameraEffect != null)
            {
                cameraEffect.TriggerViolentShake();
            }
            
            // Trigger screen flash effect - red flash for danger
            var screenFlash = GetNodeOrNull<ScreenFlashEffect>("/root/Main/ScreenFlashEffect");
            if (screenFlash != null)
            {
                screenFlash.FlashCustomColor(new Color(1f, 0.2f, 0f, 0.6f), 0.5f);
            }
            
            // Trigger vignette effect for dramatic impact
            if (cameraEffect != null)
            {
                cameraEffect.SetVignette(0.5f);
                // Create a timer to fade out vignette (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
                var timer = GetTree().CreateTimer(2.0f);
                timer.Timeout += () => {
                    if (cameraEffect != null)
                    {
                        cameraEffect.SetVignette(0f);
                    }
                };
            }
        }
        
        /// <summary>
        /// Show boss health bar for a specific boss
        /// </summary>
        public void ShowBossHealth(Node2D boss, string bossName = "")
        {
            _currentBoss = boss;
            
            if (boss is Scripts.Boss bossScript)
            {
                _bossNameLabel.Text = string.IsNullOrEmpty(bossName) ? bossScript.BossName : bossName;
            }
            else
            {
                _bossNameLabel.Text = string.IsNullOrEmpty(bossName) ? "Unknown Boss" : bossName;
            }
            
            // Reset fade
            _fadeTimer = 0f;
            _container.Modulate = new Color(1f, 1f, 1f, 1f);

            // Subscribe to boss events
            UnsubscribeFromBossEvents(); // clean up any previous subscription
            SubscribeToBossEvents();

            // Show
            Visible = true;
            _isVisible = true;
            _displayTimer = AutoHideTime;

            // Update immediately
            UpdateBossHealth();
        }

        /// <summary>
        /// REQ-160: Subscribe to boss intent events
        /// </summary>
        private void SubscribeToBossEvents()
        {
            if (_currentBoss is Scripts.Boss boss) {
                boss.OnIntentSelected += OnBossIntentSelected;
            }
        }

        /// <summary>
        /// REQ-160: Unsubscribe from boss intent events
        /// </summary>
        private void UnsubscribeFromBossEvents()
        {
            if (_currentBoss is Scripts.Boss boss) {
                boss.OnIntentSelected -= OnBossIntentSelected;
            }
            _currentIntent = null;
            _intentContainer.Visible = false;
        }

        /// <summary>
        /// REQ-160: Handler for BossDecisionMaker's intent selection signal.
        /// Shows the intent UI with the ability name, type icon, and damage range.
        /// </summary>
        private void OnBossIntentSelected(BossIntentData intent)
        {
            _currentIntent = intent;
            UpdateIntentDisplay(intent);

            // Keep health bar visible while boss is preparing an attack
            _displayTimer = AutoHideTime;
        }

        /// <summary>
        /// REQ-160: Refresh the intent display based on current BossIntentData.
        /// </summary>
        private void UpdateIntentDisplay(BossIntentData intent)
        {
            if (intent == null) {
                _intentContainer.Visible = false;
                return;
            }

            // Pick color based on intent type
            Color intentColor = intent.IntentType switch {
                BossIntentType.Damage => _intentDamage,
                BossIntentType.Buff => _intentBuff,
                BossIntentType.Debuff => _intentDebuff,
                BossIntentType.Defend => _intentDefend,
                BossIntentType.Special => _intentSpecial,
                _ => Colors.White
            };

            // Enraged state makes intent glow orange
            if (intent.IsEnraged) {
                intentColor = _intentEnraged;
            }

            // Build the icon emoji (Godot Label supports basic emoji as text on most fonts)
            string icon = intent.IntentType switch {
                BossIntentType.Damage => intent.IsAoE ? "💥" : "⚔️",
                BossIntentType.Buff => intent.AbilityName?.ToLowerInvariant().Contains("heal") == true ? "💚" : "🛡️",
                BossIntentType.Debuff => "☠️",
                BossIntentType.Defend => "🏃",
                BossIntentType.Special => intent.AbilityName?.ToLowerInvariant().Contains("summon") == true ? "👹" : "⚡",
                _ => "❓"
            };

            _intentIcon.Modulate = intentColor;
            _intentLabel.Text = $"  {icon} {intent.AbilityName}";
            _intentLabel.AddThemeColorOverride("font_color", intentColor);

            string damageText = intent.GetDisplayString();
            _intentDamageLabel.Text = intent.IntentType == BossIntentType.Damage
                ? $"  → {damageText}"
                : "";
            _intentDamageLabel.AddThemeColorOverride("font_color", intentColor);

            _intentContainer.Visible = true;
        }

        /// <summary>
        /// Hide boss health bar
        /// </summary>
        public void HideBossHealth()
        {
            UnsubscribeFromBossEvents();
            _currentBoss = null;
            StartFadeOut();
        }
        
        private void StartFadeOut()
        {
            _displayTimer = 0f;
            _fadeTimer = FadeTime;
            _isPulsing = false;
            PulseTimer = 0f;
        }
        
        /// <summary>
        /// Check if currently showing a boss
        /// </summary>
        public bool IsShowingBoss()
        {
            return _isVisible && _currentBoss != null && IsInstanceValid(_currentBoss);
        }
        
        /// <summary>
        /// Get current boss instance
        /// </summary>
        public Node2D GetCurrentBoss()
        {
            return _currentBoss;
        }
    }
}
