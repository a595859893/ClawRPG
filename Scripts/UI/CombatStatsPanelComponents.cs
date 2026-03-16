using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Combat Stats Panel Components - UI element creation
    /// Handles all UI element creation and styling
    /// </summary>
    public partial class CombatStatsPanel : Control
    {
        // Stats labels
        private Label _damageDealtLabel;
        private Label _damageTakenLabel;
        private Label _killsLabel;
        private Label _combatTimeLabel;
        private Label _dodgesLabel;
        private Label _blocksLabel;
        private Label _critsLabel;
        private Label _comboLabel;
        
        // Rating components
        private PanelContainer _ratingPanel;
        private Label _ratingLabel;
        private Label _ratingDetailLabel;
        
        // UI containers
        private PanelContainer _mainPanel;
        private VBoxContainer _statsContainer;
        
        // Animation
        private Tween _pulseTween;
        
        #region UI Setup
        
        private void SetupUI()
        {
            Name = "CombatStatsPanel";
            AnchorRight = 0f;
            AnchorBottom = 0f;
            OffsetLeft = 20;
            OffsetTop = 300;
            OffsetRight = 220;
            OffsetBottom = 550;
            
            // Main panel
            _mainPanel = new PanelContainer
            {
                Name = "MainPanel",
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 0,
                OffsetTop = 0,
                OffsetRight = 0,
                OffsetBottom = 0
            };
            AddChild(_mainPanel);
            
            // Style
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f);
            panelStyle.CornerRadiusTopLeft = 8;
            panelStyle.CornerRadiusTopRight = 8;
            panelStyle.CornerRadiusBottomLeft = 8;
            panelStyle.CornerRadiusBottomRight = 8;
            panelStyle.BorderWidthLeft = 2;
            panelStyle.BorderWidthTop = 2;
            panelStyle.BorderWidthRight = 2;
            panelStyle.BorderWidthBottom = 2;
            panelStyle.BorderColor = new Color(0.4f, 0.3f, 0.2f, 0.8f);
            _mainPanel.AddThemeStyleBoxOverride("panel", panelStyle);
            
            // Stats container
            _statsContainer = new VBoxContainer
            {
                Name = "StatsContainer",
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10,
                Theme = CreateTheme()
            };
            _mainPanel.AddChild(_statsContainer);
            
            // Title
            var titleLabel = new Label
            {
                Text = "⚔️ 战斗统计",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 18);
            titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f, 1f));
            _statsContainer.AddChild(titleLabel);
            
            // Separator
            AddSeparator();
            
            // Create stat rows
            _damageDealtLabel = AddStatRow("造成伤害", "0", new Color(1f, 0.4f, 0.4f, 1f));
            _damageTakenLabel = AddStatRow("受到伤害", "0", new Color(0.4f, 0.6f, 1f, 1f));
            _killsLabel = AddStatRow("击杀敌人", "0", new Color(0.4f, 1f, 0.5f, 1f));
            _combatTimeLabel = AddStatRow("战斗时间", "0:00", new Color(1f, 0.9f, 0.5f, 1f));
            _dodgesLabel = AddStatRow("闪避次数", "0", new Color(0.5f, 0.8f, 1f, 1f));
            _blocksLabel = AddStatRow("格挡次数", "0", new Color(0.8f, 0.6f, 1f, 1f));
            _critsLabel = AddStatRow("暴击次数", "0", new Color(1f, 0.5f, 0.8f, 1f));
            _comboLabel = AddStatRow("最高连击", "0", new Color(1f, 0.85f, 0.2f, 1f));
            
            // Rating panel (initially hidden)
            SetupRatingPanel();
        }
        
        private void SetupRatingPanel()
        {
            _ratingPanel = new PanelContainer
            {
                Name = "RatingPanel",
                Visible = false,
                OffsetLeft = -10,
                OffsetTop = -10,
                OffsetRight = 10,
                OffsetBottom = 10
            };
            
            var ratingStyle = new StyleBoxFlat();
            ratingStyle.BgColor = new Color(0.15f, 0.12f, 0.1f, 0.95f);
            ratingStyle.CornerRadiusTopLeft = 10;
            ratingStyle.CornerRadiusTopRight = 10;
            ratingStyle.CornerRadiusBottomLeft = 10;
            ratingStyle.CornerRadiusBottomRight = 10;
            ratingStyle.BorderWidthLeft = 3;
            ratingStyle.BorderWidthTop = 3;
            ratingStyle.BorderWidthRight = 3;
            ratingStyle.BorderWidthBottom = 3;
            _ratingPanel.AddThemeStyleBoxOverride("panel", ratingStyle);
            
            var ratingContainer = new VBoxContainer
            {
                Name = "RatingContainer",
                OffsetLeft = 15,
                OffsetTop = 15,
                OffsetRight = -15,
                OffsetBottom = -15
            };
            _ratingPanel.AddChild(ratingContainer);
            
            _ratingLabel = new Label
            {
                Text = "S",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _ratingLabel.AddThemeFontSizeOverride("font_size", 48);
            _ratingLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
            ratingContainer.AddChild(_ratingLabel);
            
            _ratingDetailLabel = new Label
            {
                Text = "完美表现！",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _ratingDetailLabel.AddThemeFontSizeOverride("font_size", 14);
            _ratingDetailLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
            ratingContainer.AddChild(_ratingDetailLabel);
            
            // Add as overlay
            AddChild(_ratingPanel);
        }
        
        private Theme CreateTheme()
        {
            var theme = new Theme();
            theme.SetFontSize("font_size", 14);
            return theme;
        }
        
        private Label AddStatRow(string label, string value, Color valueColor)
        {
            var container = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                CustomMinimumHeight = 24
            };
            _statsContainer.AddChild(container);
            
            var labelControl = new Label
            {
                Text = label + ":",
                HorizontalAlignment = HorizontalAlignment.Left,
                SizeFlagsHorizontal = SizeFlags.Expand
            };
            labelControl.AddThemeFontSizeOverride("font_size", 13);
            labelControl.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1f));
            container.AddChild(labelControl);
            
            var valueControl = new Label
            {
                Text = value,
                HorizontalAlignment = HorizontalAlignment.Right,
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd
            };
            valueControl.AddThemeFontSizeOverride("font_size", 14);
            valueControl.AddThemeColorOverride("font_color", valueColor);
            container.AddChild(valueControl);
            
            return valueControl;
        }
        
        private void AddSeparator()
        {
            var separator = new HSeparator
            {
                Modulate = new Color(0.4f, 0.3f, 0.2f, 0.5f),
                CustomMinimumHeight = 1
            };
            _statsContainer.AddChild(separator);
        }
        
        #endregion
    }
}
