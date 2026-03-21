using Godot;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Visual combo display with animations
    /// </summary>
    public partial class ComboDisplayUI : Control
    {
        private static ComboDisplayUI _instance;
        public static ComboDisplayUI Instance => _instance;
        
        [Export] private Label _comboLabel;
        [Export] private Label _multiplierLabel;
        [Export] private ProgressBar _comboProgressBar;
        [Export] private TextureRect _comboIcon;
        
        private Tween _scaleTween;
        private Tween _fadeTween;
        private bool _isVisible = false; 
        
        public override void _Ready()
        {
            _instance = this;
            AddToGroup("ComboDisplay");
            SetupVisuals();
            ConnectSignals();
            Hide();
        }
        
        private void SetupVisuals()
        {
            // Create container
            Name = "ComboDisplayUI";
            AnchorRight = 0f;
            AnchorBottom = 0f;
            OffsetLeft = 20;
            OffsetTop = 150;
            OffsetRight = 220;
            OffsetBottom = 280;
            
            // Combo label
            if (_comboLabel == null)
            {
                _comboLabel = new Label
                {
                    Name = "ComboLabel",
                    Text = "0",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    OffsetLeft = 0,
                    OffsetTop = 0,
                    OffsetRight = 200,
                    OffsetBottom = 80
                };
                AddChild(_comboLabel);
            }
            
            // Multiplier label
            if (_multiplierLabel == null)
            {
                _multiplierLabel = new Label
                {
                    Name = "Multiplier",
                    Text = "x1.0",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    OffsetLeft = 0,
                    OffsetTop = 70,
                    OffsetRight = 200,
                    OffsetBottom = 110
                };
                AddChild(_multiplierLabel);
            }
            
            // Progress bar
            if (_comboProgressBar == null)
            {
                _comboProgressBar = new ProgressBar
                {
                    Name = "ComboProgress",
                    MinValue = 0,
                    MaxValue = 100,
                    Value = 0,
                    ShowPercentage = false,
                    OffsetLeft = 0,
                    OffsetTop = 115,
                    OffsetRight = 200,
                    OffsetBottom = 130
                };
                AddChild(_comboProgressBar);
            }
            
            // Style the labels
            ApplyLabelStyle();
        }
        
        private void ApplyLabelStyle()
        {
            if (_comboLabel != null)
            {
                _comboLabel.AddThemeFontSizeOverride("font_size", 48);
                _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f, 1f));
                _comboLabel.AddThemeColorOverride("font_shadow_color", new Color(0, 0, 0, 0.8f));
                _comboLabel.AddThemeConstantOverride("shadow_offset_x", 2);
                _comboLabel.AddThemeConstantOverride("shadow_offset_y", 2);
            }
            
            if (_multiplierLabel != null)
            {
                _multiplierLabel.AddThemeFontSizeOverride("font_size", 20);
                _multiplierLabel.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.1f, 1f));
            }
            
            if (_comboProgressBar != null)
            {
                _comboProgressBar.AddThemeStyleBoxOverride("fill", CreateProgressStyle());
            }
        }
        
        private StyleBoxFlat CreateProgressStyle()
        {
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            style.CornerRadiusTopLeft = 4;
            style.CornerRadiusTopRight = 4;
            style.CornerRadiusBottomLeft = 4;
            style.CornerRadiusBottomRight = 4;
            return style;
        }
        
        private void ConnectSignals()
        {
            var comboSystem = GetTree().GetFirstNodeInGroup("ComboSystem");
            if (comboSystem != null)
            {
                comboSystem.OnComboChanged += OnComboChanged;
                comboSystem.OnComboMilestone += OnComboMilestone;
                comboSystem.OnComboBroken += OnComboBroken;
            }
        }
        
        private void OnComboChanged(int newCombo, int maxCombo)
        {
            if (newCombo == 0)
            {
                HideCombo();
                return;
            }
            
            if (!_isVisible)
            {
                Show();
                _isVisible = true;
            }
            
            // Update labels
            _comboLabel.Text = newCombo.ToString();
            float multiplier = 1f + (newCombo * 0.1f);
            _multiplierLabel.Text = $"x{multiplier:F1}";
            
            // Update progress (time until decay)
            float decayTime = 3.0f; // Should match ComboSystem
            _comboProgressBar.MaxValue = decayTime;
            _comboProgressBar.Value = decayTime;
            
            // Color based on combo level
            Color comboColor = GetComboColor(newCombo);
            _comboLabel.AddThemeColorOverride("font_color", comboColor);
            
            // Scale animation
            PlayScaleAnimation();
        }
        
        private void OnComboMilestone(int comboLevel, int goldReward, int expReward)
        {
            // Big celebration animation for milestones
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_comboLabel, "scale", new Vector2(1.5f, 1.5f), 0.1f);
            tween.TweenProperty(_comboLabel, "modulate", new Color(1f, 1f, 0.5f, 1f), 0.1f);
            tween.SetParallel(false);
            tween.TweenProperty(_comboLabel, "scale", Vector2.One, 0.3f);
            tween.TweenProperty(_comboLabel, "modulate", Colors.White, 0.3f);
            
            // Flash effect
            _comboLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 0.8f, 1f));
        }
        
        private void OnComboBroken()
        {
            // Fade out animation
            _fadeTween?.Kill();
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(this, "modulate:a", 0f, 0.5f);
            _fadeTween.TweenCallback(Callable.From(Hide));
        }
        
        private void HideCombo()
        {
            _fadeTween?.Kill();
            _fadeTween = CreateTween();
            _fadeTween.TweenProperty(this, "modulate:a", 0f, 0.3f);
            _fadeTween.TweenCallback(Callable.From(() => {
                Hide();
                _isVisible = false; 
            }));
        }
        
        private void PlayScaleAnimation()
        {
            _scaleTween?.Kill();
            _scaleTween = CreateTween();
            _scaleTween.TweenProperty(this, "scale", new Vector2(1.1f, 1.1f), 0.05f);
            _scaleTween.TweenProperty(this, "scale", Vector2.One, 0.15f);
        }
        
        private Color GetComboColor(int combo)
        {
            if (combo >= 75) return new Color(1f, 0.3f, 0.1f, 1f); // Red - Epic
            if (combo >= 50) return new Color(0.9f, 0.4f, 0.9f, 1f); // Purple - Rare
            if (combo >= 25) return new Color(0.2f, 0.7f, 1f, 1f);   // Blue - Uncommon
            if (combo >= 10) return new Color(0.3f, 0.9f, 0.4f, 1f); // Green - Common
            return new Color(1f, 0.85f, 0.2f, 1f); // Gold - Default
        }
        
        public override void _Process(double delta)
        {
            // Update progress bar (combo decay timer)
            var comboSystem = GetTree().GetFirstNodeInGroup("ComboSystem") as ComboSystem;
            if (comboSystem != null && comboSystem.CurrentCombo > 0 && _comboProgressBar != null)
            {
                // This is a simple approximation
                _comboProgressBar.Value = Mathf.Max(0, _comboProgressBar.Value - (float)delta);
            }
        }
    }
}
