using Godot;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// UI Components for CombatStatsPanel
    /// Handles creation and styling of all UI elements
    /// </summary>
    public class CombatStatsPanelComponents
    {
        private readonly Control _owner;
        
        // Rating panel references (exposed to owner)
        public PanelContainer RatingPanel { get; private set; }
        public Label RatingLabel { get; private set; }
        public Label RatingDetailLabel { get; private set; }
        
        public CombatStatsPanelComponents(Control owner)
        {
            _owner = owner;
        }
        
        /// <summary>
        /// Setup the main panel and stats container
        /// </summary>
        public void SetupMainPanel(out PanelContainer mainPanel, out VBoxContainer statsContainer)
        {
            // Main panel
            mainPanel = new PanelContainer
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
            
            // Stats container
            statsContainer = new VBoxContainer
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
            mainPanel.AddChild(statsContainer);
            
            // Title
            var titleLabel = new Label
            {
                Text = "⚔️ 战斗统计",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 18);
            titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f, 1f));
            statsContainer.AddChild(titleLabel);
            
            // Separator
            AddSeparator(statsContainer);
        }
        
        /// <summary>
        /// Setup the rating panel (shown after combat)
        /// </summary>
        public void SetupRatingPanel()
        {
            RatingPanel = new PanelContainer
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
            RatingPanel.AddThemeStyleBoxOverride("panel", ratingStyle);
            
            var ratingContainer = new VBoxContainer
            {
                Name = "RatingContainer",
                OffsetLeft = 15,
                OffsetTop = 15,
                OffsetRight = -15,
                OffsetBottom = -15
            };
            RatingPanel.AddChild(ratingContainer);
            
            RatingLabel = new Label
            {
                Text = "S",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            RatingLabel.AddThemeFontSizeOverride("font_size", 48);
            RatingLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
            ratingContainer.AddChild(RatingLabel);
            
            RatingDetailLabel = new Label
            {
                Text = "完美表现！",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            RatingDetailLabel.AddThemeFontSizeOverride("font_size", 14);
            RatingDetailLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
            ratingContainer.AddChild(RatingDetailLabel);
            
            // Add as overlay to owner
            _owner.AddChild(RatingPanel);
        }
        
        /// <summary>
        /// Create a theme for the stats container
        /// </summary>
        private Theme CreateTheme()
        {
            var theme = new Theme();
            theme.SetFontSize("font_size", 14);
            return theme;
        }
        
        /// <summary>
        /// Add a stat row with label and value
        /// </summary>
        public Label AddStatRow(VBoxContainer container, string label, string value, Color valueColor)
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
        /// Add a separator between sections
        /// </summary>
        public void AddSeparator(VBoxContainer container)
        {
            var separator = new HSeparator
            {
                Modulate = new Color(0.4f, 0.3f, 0.2f, 0.5f),
                CustomMinimumHeight = 1
            };
            container.AddChild(separator);
        }
    }
}
