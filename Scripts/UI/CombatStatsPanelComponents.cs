using Godot;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// UI Components factory for CombatStatsPanel
    /// Handles creation and styling of UI elements
    /// </summary>
    public static class CombatStatsPanelComponents
    {
        /// <summary>
        /// Create the main panel container with styling
        /// </summary>
        public static PanelContainer CreateMainPanel()
        {
            var mainPanel = new PanelContainer
            {
                Name = "MainPanel",
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 0,
                OffsetTop = 0,
                OffsetRight = 0,
                OffsetBottom = 0
            };
            
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
            mainPanel.AddThemeStyleBoxOverride("panel", panelStyle);
            
            return mainPanel;
        }
        
        /// <summary>
        /// Create the stats container with theme
        /// </summary>
        public static VBoxContainer CreateStatsContainer()
        {
            var statsContainer = new VBoxContainer
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
            return statsContainer;
        }
        
        /// <summary>
        /// Create the title label
        /// </summary>
        public static Label CreateTitleLabel()
        {
            var titleLabel = new Label
            {
                Text = "⚔️ 战斗统计",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 18);
            titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f, 1f));
            return titleLabel;
        }
        
        /// <summary>
        /// Create a stat row with label and value
        /// </summary>
        public static Label CreateStatRow(VBoxContainer container, string label, string value, Color valueColor)
        {
            var rowContainer = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                CustomMinimumHeight = 24
            };
            container.AddChild(rowContainer);
            
            var labelControl = new Label
            {
                Text = label + ":",
                HorizontalAlignment = HorizontalAlignment.Left,
                SizeFlagsHorizontal = SizeFlags.Expand
            };
            labelControl.AddThemeFontSizeOverride("font_size", 13);
            labelControl.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1f));
            rowContainer.AddChild(labelControl);
            
            var valueControl = new Label
            {
                Text = value,
                HorizontalAlignment = HorizontalAlignment.Right,
                SizeFlagsHorizontal = SizeFlags.ShrinkEnd
            };
            valueControl.AddThemeFontSizeOverride("font_size", 14);
            valueControl.AddThemeColorOverride("font_color", valueColor);
            rowContainer.AddChild(valueControl);
            
            return valueControl;
        }
        
        /// <summary>
        /// Create a separator
        /// </summary>
        public static void CreateSeparator(VBoxContainer container)
        {
            var separator = new HSeparator
            {
                Modulate = new Color(0.4f, 0.3f, 0.2f, 0.5f),
                CustomMinimumHeight = 1
            };
            container.AddChild(separator);
        }
        
        /// <summary>
        /// Create the rating panel (shown after combat)
        /// </summary>
        public static (PanelContainer panel, Label ratingLabel, Label detailLabel) CreateRatingPanel(Control owner)
        {
            var ratingPanel = new PanelContainer
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
            ratingPanel.AddThemeStyleBoxOverride("panel", ratingStyle);
            
            var ratingContainer = new VBoxContainer
            {
                Name = "RatingContainer",
                OffsetLeft = 15,
                OffsetTop = 15,
                OffsetRight = -15,
                OffsetBottom = -15
            };
            ratingPanel.AddChild(ratingContainer);
            
            var ratingLabel = new Label
            {
                Text = "S",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            ratingLabel.AddThemeFontSizeOverride("font_size", 48);
            ratingLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
            ratingContainer.AddChild(ratingLabel);
            
            var detailLabel = new Label
            {
                Text = "完美表现！",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            detailLabel.AddThemeFontSizeOverride("font_size", 14);
            detailLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
            ratingContainer.AddChild(detailLabel);
            
            return (ratingPanel, ratingLabel, detailLabel);
        }
        
        /// <summary>
        /// Create a theme for the stats container
        /// </summary>
        private static Theme CreateTheme()
        {
            var theme = new Theme();
            theme.SetFontSize("font_size", 14);
            return theme;
        }
    }
}
