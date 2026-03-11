using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Achievement UI - displays achievement progress and list
    /// </summary>
    public partial class AchievementUI : Control
    {
        [Export] private Color _unlockedColor = new Color(1f, 0.84f, 0f, 1f);
        [Export] private Color _lockedColor = new Color(0.5f, 0.5f, 0.5f, 1f);
        [Export] private Color _progressColor = new Color(0.3f, 0.6f, 1f, 1f);
        
        private PanelContainer _mainPanel;
        private VBoxContainer _contentVBox;
        private Label _titleLabel;
        private Label _progressLabel;
        private ScrollContainer _scrollContainer;
        private GridContainer _achievementGrid;
        private Button _closeButton;
        
        private List<Achievement> _allAchievements;
        private bool _isVisible;
        
        public override void _Ready()
        {
            Visible = false; 
            _isVisible = false; 
            
            SetupUI();
            ConnectSignals();
            
            // Subscribe to achievement events
            AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
            AchievementManager.Instance.OnAchievementProgressUpdated += OnAchievementProgressUpdated;
        }
        
        private void SetupUI()
        {
            // Main panel
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(700, 500);
            AddChild(_mainPanel);
            
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            styleBox.BorderWidthLeft = 3;
            styleBox.BorderWidthRight = 3;
            styleBox.BorderWidthTop = 3;
            styleBox.BorderWidthBottom = 3;
            styleBox.BorderColor = new Color(0.8f, 0.6f, 0.2f, 1f);
            styleBox.CornerRadiusTopLeft = 10;
            styleBox.CornerRadiusTopRight = 10;
            styleBox.CornerRadiusBottomLeft = 10;
            styleBox.CornerRadiusBottomRight = 10;
            _mainPanel.AddThemeStyleboxOverride("panel", styleBox);
            
            // Content VBox
            _contentVBox = new VBoxContainer();
            _contentVBox.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(_contentVBox);
            
            // Title bar
            var titleBar = new HBoxContainer();
            titleBar.AddThemeConstantOverride("separation", 10);
            _contentVBox.AddChild(titleBar);
            
            _titleLabel = new Label();
            _titleLabel.Text = "🏆 成就";
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
            titleBar.AddChild(_titleLabel);
            
            titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            _closeButton.Pressed += ToggleVisibility;
            titleBar.AddChild(_closeButton);
            
            // Progress label
            _progressLabel = new Label();
            _progressLabel.AddThemeFontSizeOverride("font_size", 16);
            _progressLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1f));
            _contentVBox.AddChild(_progressLabel);
            
            // Separator
            var separator = new HSeparator();
            separator.AddThemeColorOverride("separator", new Color(0.5f, 0.4f, 0.2f, 0.5f));
            _contentVBox.AddChild(separator);
            
            // Scroll container
            _scrollContainer = new ScrollContainer();
            _scrollContainer.SetHExpand(true);
            _scrollContainer.SetVExpand(true);
            _scrollContainer.CustomMinimumSize = new Vector2(0, 400);
            _contentVBox.AddChild(_scrollContainer);
            
            // Achievement grid
            _achievementGrid = new GridContainer();
            _achievementGrid.Columns = 2;
            _achievementGrid.AddThemeConstantOverride("h_separation", 10);
            _achievementGrid.AddThemeConstantOverride("v_separation", 10);
            _achievementGrid.SetHExpand(true);
            _scrollContainer.AddChild(_achievementGrid);
            
            RefreshAchievementList();
        }
        
        private void ConnectSignals()
        {
            // Handle input
        }
        
        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                RefreshAchievementList();
                UpdateProgress();
            }
        }
        
        private void RefreshAchievementList()
        {
            // Clear existing items
            foreach (Node child in _achievementGrid.GetChildren())
            {
                child.QueueFree();
            }
            
            _allAchievements = AchievementManager.Instance.GetAllTrackedAchievements();
            
            foreach (var achievement in _allAchievements)
            {
                var item = CreateAchievementItem(achievement);
                _achievementGrid.AddChild(item);
            }
        }
        
        private Control CreateAchievementItem(Achievement achievement)
        {
            var container = new PanelContainer();
            container.SetHExpand(true);
            container.CustomMinimumSize = new Vector2(320, 80);
            
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = achievement.IsUnlocked 
                ? new Color(0.2f, 0.25f, 0.15f, 0.9f) 
                : new Color(0.15f, 0.15f, 0.2f, 0.9f);
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderColor = achievement.IsUnlocked ? _unlockedColor : _lockedColor;
            styleBox.CornerRadiusTopLeft = 8;
            styleBox.CornerRadiusTopRight = 8;
            styleBox.CornerRadiusBottomLeft = 8;
            styleBox.CornerRadiusBottomRight = 8;
            container.AddThemeStyleboxOverride("panel", styleBox);
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 4);
            container.AddChild(vbox);
            
            // Name and icon
            var nameHBox = new HBoxContainer();
            nameHBox.AddThemeConstantOverride("separation", 8);
            vbox.AddChild(nameHBox);
            
            var iconLabel = new Label();
            iconLabel.Text = achievement.IsUnlocked ? "🏆" : "🔒";
            iconLabel.AddThemeFontSizeOverride("font_size", 18);
            nameHBox.AddChild(iconLabel);
            
            var nameLabel = new Label();
            nameLabel.Text = achievement.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.AddThemeColorOverride("font_color", achievement.IsUnlocked ? _unlockedColor : _lockedColor);
            nameLabel.SetHExpand(true);
            nameHBox.AddChild(nameLabel);
            
            // Description
            var descLabel = new Label();
            descLabel.Text = achievement.Description;
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1f));
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            vbox.AddChild(descLabel);
            
            // Progress or reward
            var progressLabel = new Label();
            if (achievement.IsUnlocked)
            {
                progressLabel.Text = $"✅ 已解锁";
                progressLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.8f, 0.3f, 1f));
            }
            else
            {
                progressLabel.Text = $"{achievement.CurrentValue} / {achievement.RequiredValue} ({achievement.Progress * 100:F0}%)";
                progressLabel.AddThemeColorOverride("font_color", _progressColor);
            }
            progressLabel.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(progressLabel);
            
            // Progress bar
            var progressBar = new ProgressBar();
            progressBar.Value = achievement.Progress * 100;
            progressBar.MaxValue = 100;
            progressBar.CustomMinimumSize = new Vector2(0, 8);
            progressBar.ShowPercentage = false; 
            
            var progressStyle = new StyleBoxFlat();
            progressStyle.BgColor = new Color(0.2f, 0.2f, 0.25f, 1f);
            progressStyle.CornerRadiusTopLeft = 4;
            progressStyle.CornerRadiusTopRight = 4;
            progressStyle.CornerRadiusBottomLeft = 4;
            progressStyle.CornerRadiusBottomRight = 4;
            progressBar.AddThemeStyleboxOverride("background", progressStyle);
            
            var fillStyle = new StyleBoxFlat();
            fillStyle.BgColor = achievement.IsUnlocked ? _unlockedColor : _progressColor;
            fillStyle.CornerRadiusTopLeft = 4;
            fillStyle.CornerRadiusTopRight = 4;
            fillStyle.CornerRadiusBottomLeft = 4;
            fillStyle.CornerRadiusBottomRight = 4;
            progressBar.AddThemeStyleboxOverride("fill", fillStyle);
            
            vbox.AddChild(progressBar);
            
            // Rewards
            var rewardLabel = new Label();
            string rewards = "";
            if (achievement.RewardGold > 0) rewards += $"💰{achievement.RewardGold} ";
            if (achievement.RewardExp > 0) rewards += $"✨{achievement.RewardExp}";
            rewardLabel.Text = rewards;
            rewardLabel.AddThemeFontSizeOverride("font_size", 11);
            rewardLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.4f, 1f));
            vbox.AddChild(rewardLabel);
            
            return container;
        }
        
        private void UpdateProgress()
        {
            var stats = AchievementManager.Instance.GetStatistics();
            int unlocked = stats.GetValueOrDefault("unlockedAchievements", 0);
            int total = stats.GetValueOrDefault("totalAchievements", 0);
            float progress = total > 0 ? (float)unlocked / total * 100 : 0;
            
            _progressLabel.Text = $"完成进度: {unlocked}/{total} ({progress:F1}%)";
        }
        
        private void OnAchievementUnlocked(Achievement achievement)
        {
            // Show notification or refresh
            if (Visible)
            {
                RefreshAchievementList();
                UpdateProgress();
            }
        }
        
        private void OnAchievementProgressUpdated(Achievement achievement)
        {
            if (Visible)
            {
                RefreshAchievementList();
            }
        }
        
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.L)
                {
                    ToggleVisibility();
                    GetTree().SetInputAsHandled();
                }
                else if (keyEvent.Keycode == Key.Escape && Visible)
                {
                    ToggleVisibility();
                    GetTree().SetInputAsHandled();
                }
            }
        }
        
        public override void _ExitTree()
        {
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
            AchievementManager.Instance.OnAchievementProgressUpdated -= OnAchievementProgressUpdated;
        }
    }
}
