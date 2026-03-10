using Godot;
using System;
using System.Collections.Generic;

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
        
        // State
        private Node2D _currentBoss;
        private bool _isVisible = false;
        private float _displayTimer = 0f;
        private float _fadeTimer = 0f;
        private const float AutoHideTime = 3f;
        private const float FadeTime = 0.5f;
        
        // Colors
        private Color _healthColorNormal = new Color(0.2f, 0.8f, 0.2f);
        private Color _healthColorWarning = new Color(0.9f, 0.7f, 0.1f);
        private Color _healthColorCritical = new Color(0.9f, 0.2f, 0.2f);
        
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
        }
        
        public override void _Process(float delta)
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
            
            // Update health bar color based on percentage
            float healthPercent = currentHealth / maxHealth;
            StyleBoxFlat fillStyle = _healthBar.GetThemeStylebox("fill") as StyleBoxFlat;
            if (fillStyle != null)
            {
                if (healthPercent > 0.5f)
                {
                    fillStyle.BgColor = _healthColorNormal;
                }
                else if (healthPercent > 0.25f)
                {
                    fillStyle.BgColor = _healthColorWarning;
                }
                else
                {
                    fillStyle.BgColor = _healthColorCritical;
                }
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
            
            // Show
            Visible = true;
            _isVisible = true;
            _displayTimer = AutoHideTime;
            
            // Update immediately
            UpdateBossHealth();
        }
        
        /// <summary>
        /// Hide boss health bar
        /// </summary>
        public void HideBossHealth()
        {
            _currentBoss = null;
            StartFadeOut();
        }
        
        private void StartFadeOut()
        {
            _displayTimer = 0f;
            _fadeTimer = FadeTime;
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
