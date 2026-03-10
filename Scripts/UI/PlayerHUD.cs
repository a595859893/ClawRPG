using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Player HUD - displays player stats, health, mana, experience
    /// </summary>
    public class PlayerHUD : Control
    {
        // Health Bar
        private ProgressBar _healthBar;
        private Label _healthLabel;
        
        // Mana Bar
        private ProgressBar _manaBar;
        private Label _manaLabel;
        
        // Experience Bar
        private ProgressBar _expBar;
        private Label _expLabel;
        
        // Level Display
        private Label _levelLabel;
        
        // Player Reference
        private Player _player;
        
        public override void _Ready()
        {
            SetupUI();
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            
            if (_player != null)
            {
                UpdateHUD();
            }
        }
        
        public override void _Process(double delta)
        {
            if (_player != null)
            {
                UpdateHUD();
            }
        }
        
        private void SetupUI()
        {
            // Create style
            var style = GetStyle();
            
            // Health Bar Container (bottom left)
            var healthContainer = new VBoxContainer();
            healthContainer.SetAnchor(AnchorPresets.BottomLeft);
            healthContainer.Position = new Vector2(20, -80);
            healthContainer.CustomMinimumSize = new Vector2(300, 60);
            AddChild(healthContainer);
            
            _healthLabel = new Label();
            _healthLabel.Text = "HP: 100/100";
            _healthLabel.AddThemeFontSizeOverride("font_size", 18);
            _healthLabel.AddThemeColorOverride("font_color", new Color(1, 0.3, 0.3));
            healthContainer.AddChild(_healthLabel);
            
            _healthBar = new ProgressBar();
            _healthBar.CustomMinimumSize = new Vector2(300, 25);
            _healthBar.ShowPercentage = false;
            healthContainer.AddChild(_healthBar);
            
            // Apply health bar style
            var healthStyle = new StyleBoxFlat();
            healthStyle.BgColor = new Color(0.3, 0, 0);
            healthStyle.BorderWidthBottom = 2;
            healthStyle.BorderColor = new Color(0.8, 0.2, 0.2);
            _healthBar.AddThemeStyleboxOverride("fill", healthStyle);
            
            // Mana Bar Container
            var manaContainer = new VBoxContainer();
            manaContainer.SetAnchor(AnchorPresets.BottomLeft);
            manaContainer.Position = new Vector2(20, -140);
            manaContainer.CustomMinimumSize = new Vector2(300, 60);
            AddChild(manaContainer);
            
            _manaLabel = new Label();
            _manaLabel.Text = "MP: 50/50";
            _manaLabel.AddThemeFontSizeOverride("font_size", 18);
            _manaLabel.AddThemeColorOverride("font_color", new Color(0.3, 0.5, 1));
            manaContainer.AddChild(_manaLabel);
            
            _manaBar = new ProgressBar();
            _manaBar.CustomMinimumSize = new Vector2(300, 20);
            _manaBar.ShowPercentage = false;
            manaContainer.AddChild(_manaBar);
            
            // Apply mana bar style
            var manaStyle = new StyleBoxFlat();
            manaStyle.BgColor = new Color(0, 0, 0.3);
            manaStyle.BorderWidthBottom = 2;
            manaStyle.BorderColor = new Color(0.2, 0.4, 0.8);
            _manaBar.AddThemeStyleboxOverride("fill", manaStyle);
            
            // Experience Bar (top)
            var expContainer = new VBoxContainer();
            expContainer.SetAnchor(AnchorPresets.TopLeft);
            expContainer.Position = new Vector2(20, 20);
            expContainer.CustomMinimumSize = new Vector2(400, 40);
            AddChild(expContainer);
            
            _levelLabel = new Label();
            _levelLabel.Text = "等级 1";
            _levelLabel.AddThemeFontSizeOverride("font_size", 22);
            _levelLabel.AddThemeColorOverride("font_color", new Color(1, 0.85, 0.3));
            expContainer.AddChild(_levelLabel);
            
            _expBar = new ProgressBar();
            _expBar.CustomMinimumSize = new Vector2(400, 15);
            _expBar.ShowPercentage = false;
            expContainer.AddChild(_expBar);
            
            _expLabel = new Label();
            _expLabel.Text = "经验: 0/100";
            _expLabel.AddThemeFontSizeOverride("font_size", 14);
            _expLabel.AddThemeColorOverride("font_color", new Color(0.8, 0.8, 0.8));
            expContainer.AddChild(_expLabel);
            
            // Apply exp bar style
            var expStyle = new StyleBoxFlat();
            expStyle.BgColor = new Color(0.2, 0.2, 0.2);
            expStyle.BorderWidthBottom = 2;
            expStyle.BorderColor = new Color(0.6, 0.6, 0.3);
            _expBar.AddThemeStyleboxOverride("fill", expStyle);
        }
        
        private void UpdateHUD()
        {
            if (_player == null) return;
            
            // Update health
            float healthPercent = (float)_player.CurrentHealth / _player.MaxHealth;
            _healthBar.Value = healthPercent * 100;
            _healthLabel.Text = $"HP: {_player.CurrentHealth}/{_player.MaxHealth}";
            
            // Update mana
            float manaPercent = (float)_player.CurrentMana / _player.MaxMana;
            _manaBar.Value = manaPercent * 100;
            _manaLabel.Text = $"MP: {_player.CurrentMana}/{_player.MaxMana}";
            
            // Update level and experience
            _levelLabel.Text = $"等级 {_player.Level}";
            
            // Calculate exp needed for next level (simple formula: 100 * level)
            int expNeeded = 100 * _player.Level;
            float expPercent = (float)_player.Experience / expNeeded;
            _expBar.Value = expPercent * 100;
            _expLabel.Text = $"经验: {_player.Experience}/{expNeeded}";
        }
        
        private StyleBoxFlat GetStyle()
        {
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1, 0.1, 0.1, 0.8);
            style.BorderWidthLeft = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = new Color(0.3, 0.3, 0.3);
            style.CornerRadiusTopLeft = 5;
            style.CornerRadiusTopRight = 5;
            style.CornerRadiusBottomLeft = 5;
            style.CornerRadiusBottomRight = 5;
            return style;
        }
    }
}
