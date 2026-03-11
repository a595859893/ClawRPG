using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Secret Achievement UI - displays hidden achievements with progress
    /// </summary>
    public partial class SecretAchievementUI : Control
    {
        private Control _mainPanel;
        private VBoxContainer _contentContainer;
        private Label _titleLabel;
        private Label _statsLabel;
        private GridContainer _achievementGrid;
        private Button _closeButton;
        
        // Achievement item scene reference
        private PackedScene _achievementItemScene;
        
        // Category filter
        private OptionButton _categoryFilter;
        private OptionButton _rarityFilter;
        
        // Current filter
        private SecretAchievementCategory? _currentCategoryFilter = null;
        private SecretAchievementRarity? _currentRarityFilter = null;
        
        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
            RefreshAchievements();
        }

        private void SetupUI()
        {
            // Main panel
            _mainPanel = new Control();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(800, 600);
            AddChild(_mainPanel);
            
            // Background panel
            Panel backgroundPanel = new Panel();
            backgroundPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            backgroundPanel.Modulate = new Color(0, 0, 0, 0.85f);
            _mainPanel.AddChild(backgroundPanel);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "Secret Achievements";
            _titleLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.VerticalAlignment = VerticalAlignment.Center;
            _titleLabel.Position = new Vector2(0, 20);
            _titleLabel.CustomMinimumSize = new Vector2(0, 50);
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            _mainPanel.AddChild(_titleLabel);
            
            // Close button
            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.Position = new Vector2(750, 20);
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            _mainPanel.AddChild(_closeButton);
            
            // Stats label
            _statsLabel = new Label();
            _statsLabel.Text = "Discovered: 0 / 0";
            _statsLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
            _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _statsLabel.Position = new Vector2(0, 70);
            _statsLabel.AddThemeFontSizeOverride("font_size", 18);
            _mainPanel.AddChild(_statsLabel);
            
            // Filter container
            HBoxContainer filterContainer = new HBoxContainer();
            filterContainer.Position = new Vector2(50, 110);
            filterContainer.CustomMinimumSize = new Vector2(700, 40);
            _mainPanel.AddChild(filterContainer);
            
            // Category filter
            Label categoryLabel = new Label();
            categoryLabel.Text = "Category:";
            categoryLabel.VerticalAlignment = VerticalAlignment.Center;
            filterContainer.AddChild(categoryLabel);
            
            _categoryFilter = new OptionButton();
            _categoryFilter.CustomMinimumSize = new Vector2(150, 30);
            _categoryFilter.AddItem("All Categories", 0);
            _categoryFilter.AddItem("Combat", (int)SecretAchievementCategory.Combat + 1);
            _categoryFilter.AddItem("Exploration", (int)SecretAchievementCategory.Exploration + 1);
            _categoryFilter.AddItem("Collection", (int)SecretAchievementCategory.Collection + 1);
            _categoryFilter.AddItem("Social", (int)SecretAchievementCategory.Social + 1);
            _categoryFilter.AddItem("Challenge", (int)SecretAchievementCategory.Challenge + 1);
            _categoryFilter.AddItem("Lucky", (int)SecretAchievementCategory.Lucky + 1);
            _categoryFilter.AddItem("Hidden", (int)SecretAchievementCategory.Hidden + 1);
            filterContainer.AddChild(_categoryFilter);
            
            // Spacer
            Control spacer = new Control();
            spacer.CustomMinimumSize = new Vector2(50, 0);
            filterContainer.AddChild(spacer);
            
            // Rarity filter
            Label rarityLabel = new Label();
            rarityLabel.Text = "Rarity:";
            rarityLabel.VerticalAlignment = VerticalAlignment.Center;
            filterContainer.AddChild(rarityLabel);
            
            _rarityFilter = new OptionButton();
            _rarityFilter.CustomMinimumSize = new Vector2(150, 30);
            _rarityFilter.AddItem("All Rarities", 0);
            _rarityFilter.AddItem("Common", (int)SecretAchievementRarity.Common + 1);
            _rarityFilter.AddItem("Uncommon", (int)SecretAchievementRarity.Uncommon + 1);
            _rarityFilter.AddItem("Rare", (int)SecretAchievementRarity.Rare + 1);
            _rarityFilter.AddItem("Epic", (int)SecretAchievementRarity.Epic + 1);
            _rarityFilter.AddItem("Legendary", (int)SecretAchievementRarity.Legendary + 1);
            _rarityFilter.AddItem("Mythic", (int)SecretAchievementRarity.Mythic + 1);
            filterContainer.AddChild(_rarityFilter);
            
            // Scroll container for achievements
            ScrollContainer scrollContainer = new ScrollContainer();
            scrollContainer.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
            scrollContainer.Position = new Vector2(30, 160);
            scrollContainer.CustomMinimumSize = new Vector2(740, 400);
            _mainPanel.AddChild(scrollContainer);
            
            // Achievement grid
            _achievementGrid = new GridContainer();
            _achievementGrid.Columns = 2;
            _achievementGrid.AddThemeConstantOverride("separation", 10);
            scrollContainer.AddChild(_achievementGrid);
            
            // Apply animations
            ApplyAnimations();
        }

        private void ApplyAnimations()
        {
            // Panel fade in animation
            Tween tween = CreateTween();
            tween.SetParallel(true);
            
            foreach (Node child in _mainPanel.GetChildren())
            {
                if (child is Control control)
                {
                    control.Modulate = new Color(1, 1, 1, 0);
                    tween.TweenProperty(control, "modulate:a", 1.0, 0.3f);
                }
            }
            
            // Scale animation
            _mainPanel.Scale = new Vector2(0.9f, 0.9f);
            tween.TweenProperty(_mainPanel, "scale", new Vector2(1.0f, 1.0f), 0.3f)
                .SetTrans(Tween.TransitionType.Back)
                .SetEasing(Tween.EasingFunction.EaseOut);
        }

        private void ConnectSignals()
        {
            _closeButton.Pressed += OnClosePressed;
            _categoryFilter.ItemSelected += OnCategoryFilterChanged;
            _rarityFilter.ItemSelected += OnRarityFilterChanged;
            
            // Connect to system signals
            if (SecretAchievementSystem.Instance != null)
            {
                SecretAchievementSystem.Instance.OnAchievementDiscovered += OnAchievementDiscovered;
            }
        }

        private void RefreshAchievements()
        {
            // Clear existing items
            foreach (Node child in _achievementGrid.GetChildren())
            {
                child.QueueFree();
            }
            
            var allAchievements = SecretAchievementDatabase.GetAllAchievements();
            int totalDiscovered = 0;
            int totalCount = allAchievements.Count;
            
            foreach (var achievement in allAchievements)
            {
                // Apply filters
                if (_currentCategoryFilter.HasValue && achievement.Category != _currentCategoryFilter.Value)
                    continue;
                if (_currentRarityFilter.HasValue && achievement.Rarity != _currentRarityFilter.Value)
                    continue;
                
                // Get player data
                bool isDiscovered = SecretAchievementSystem.Instance.IsDiscovered(achievement.AchievementId);
                if (isDiscovered) totalDiscovered++;
                
                int progress = SecretAchievementSystem.Instance.GetProgress(achievement.AchievementId);
                float progressPercent = SecretAchievementSystem.Instance.GetDiscoveryProgress(achievement.AchievementId);
                
                // Create achievement item
                Control item = CreateAchievementItem(achievement, isDiscovered, progress, progressPercent);
                _achievementGrid.AddChild(item);
            }
            
            // Update stats
            _statsLabel.Text = $"Discovered: {totalDiscovered} / {totalCount} ({SecretAchievementSystem.Instance.GetTotalDiscoveryPercentage()*100:F1}%)";
        }

        private Control CreateAchievementItem(SecretAchievementData achievement, bool isDiscovered, int progress, float progressPercent)
        {
            Control itemContainer = new Control();
            itemContainer.CustomMinimumSize = new Vector2(350, 100);
            
            // Background panel
            Panel itemPanel = new Panel();
            itemPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            itemPanel.Modulate = GetRarityColor(achievement.Rarity, isDiscovered);
            itemContainer.AddChild(itemPanel);
            
            // Name label
            Label nameLabel = new Label();
            nameLabel.Text = isDiscovered ? achievement.DisplayName : "???????";
            nameLabel.Position = new Vector2(10, 10);
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            itemContainer.AddChild(nameLabel);
            
            // Description
            Label descLabel = new Label();
            descLabel.Text = isDiscovered ? achievement.Description : "??? ??????? ???????";
            descLabel.Position = new Vector2(10, 35);
            descLabel.CustomMinimumSize = new Vector2(330, 30);
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f, 1);
            itemContainer.AddChild(descLabel);
            
            // Progress bar (for undiscovered)
            if (!isDiscovered && progress > 0)
            {
                ProgressBar progressBar = new ProgressBar();
                progressBar.Position = new Vector2(10, 70);
                progressBar.CustomMinimumSize = new Vector2(330, 15);
                progressBar.Value = progressPercent * 100;
                progressBar.ShowPercentage = false; 
                progressBar.Modulate = new Color(0.3f, 0.3f, 0.3f, 0.8f);
                itemContainer.AddChild(progressBar);
                
                Label progressLabel = new Label();
                progressLabel.Text = $"{progress} / {achievement.DiscoveryCondition}";
                progressLabel.Position = new Vector2(10, 85);
                progressLabel.AddThemeFontSizeOverride("font_size", 10);
                progressLabel.Modulate = new Color(0.5f, 0.5f, 0.5f, 1);
                itemContainer.AddChild(progressLabel);
            }
            
            // Rewards (for discovered)
            if (isDiscovered && (achievement.GoldReward > 0 || achievement.ExpReward > 0))
            {
                Label rewardLabel = new Label();
                string rewards = "";
                if (achievement.GoldReward > 0) rewards += $"💰 {achievement.GoldReward} ";
                if (achievement.ExpReward > 0) rewards += $"✨ {achievement.ExpReward}";
                rewardLabel.Text = rewards;
                rewardLabel.Position = new Vector2(10, 75);
                rewardLabel.AddThemeFontSizeOverride("font_size", 12);
                rewardLabel.Modulate = new Color(1f, 0.85f, 0.3f, 1);
                itemContainer.AddChild(rewardLabel);
            }
            
            // Category badge
            Label categoryBadge = new Label();
            categoryBadge.Text = achievement.Category.ToString();
            categoryBadge.Position = new Vector2(280, 10);
            categoryBadge.AddThemeFontSizeOverride("font_size", 10);
            categoryBadge.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.5f);
            itemContainer.AddChild(categoryBadge);
            
            // Hover effect
            itemPanel.MouseEntered += () => {
                Tween tween = CreateTween();
                tween.TweenProperty(itemPanel, "modulate", itemPanel.Modulate * new Color(1, 1, 1, 1.2f), 0.1f);
            };
            itemPanel.MouseExited += () => {
                Tween tween = CreateTween();
                tween.TweenProperty(itemPanel, "modulate", GetRarityColor(achievement.Rarity, isDiscovered), 0.1f);
            };
            
            return itemContainer;
        }

        private Color GetRarityColor(SecretAchievementRarity rarity, bool isDiscovered)
        {
            if (!isDiscovered)
                return new Color(0.2f, 0.2f, 0.2f, 0.8f);
                
            return rarity switch
            {
                SecretAchievementRarity.Common => new Color(0.6f, 0.6f, 0.6f, 0.9f),
                SecretAchievementRarity.Uncommon => new Color(0.3f, 0.7f, 0.3f, 0.9f),
                SecretAchievementRarity.Rare => new Color(0.3f, 0.5f, 0.9f, 0.9f),
                SecretAchievementRarity.Epic => new Color(0.6f, 0.3f, 0.9f, 0.9f),
                SecretAchievementRarity.Legendary => new Color(1f, 0.6f, 0.2f, 0.9f),
                SecretAchievementRarity.Mythic => new Color(1f, 0.3f, 0.5f, 0.9f),
                _ => new Color(0.5f, 0.5f, 0.5f, 0.9f)
            };
        }

        private void OnClosePressed()
        {
            // Fade out animation
            Tween tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.2f);
            tween.TweenCallback(Callable.From(() => QueueFree()));
        }

        private void OnCategoryFilterChanged(long index)
        {
            if (index == 0)
                _currentCategoryFilter = null;
            else
                _currentCategoryFilter = (SecretAchievementCategory)(index - 1);
            
            RefreshAchievements();
        }

        private void OnRarityFilterChanged(long index)
        {
            if (index == 0)
                _currentRarityFilter = null;
            else
                _currentRarityFilter = (SecretAchievementRarity)(index - 1);
            
            RefreshAchievements();
        }

        private void OnAchievementDiscovered(string achievementId, int goldReward, int expReward)
        {
            RefreshAchievements();
            
            // Show discovery notification
            var achievement = SecretAchievementDatabase.GetAchievement(achievementId);
            if (achievement != null)
            {
                GD.Print($"[SecretAchievement] Discovered: {achievement.DisplayName} - {achievement.Description}");
            }
        }

        public static void Show()
        {
            var ui = new SecretAchievementUI();
            var main = (CanvasItem)Engine.GetMainLoop();
            if (main is Node mainNode)
            {
                mainNode.AddChild(ui);
            }
        }
    }
}
