using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Multiplayer Leaderboard UI
    /// Displays player rankings in various categories
    /// </summary>
    public partial class MultiplayerLeaderboardUI : Control
    {
        private static MultiplayerLeaderboardUI _instance;
        public static MultiplayerLeaderboardUI Instance => _instance;

        // UI elements
        private PanelContainer _mainPanel;
        private VBoxContainer _contentContainer;
        private OptionButton _categorySelector;
        private ScrollContainer _leaderboardScroll;
        private VBoxContainer _entriesContainer;
        private Label _titleLabel;
        
        // Stats
        private HBoxContainer _playerStatsContainer;
        private Label _yourRankLabel;
        private Label _yourValueLabel;
        
        // Category
        private MultiplayerLeaderboard.LeaderboardCategory _currentCategory = 
            MultiplayerLeaderboard.LeaderboardCategory.Kills;

        // Colors
        private readonly Color GoldColor = new Color(1f, 0.84f, 0f, 1f);
        private readonly Color SilverColor = new Color(0.75f, 0.75f, 0.75f, 1f);
        private readonly Color BronzeColor = new Color(0.8f, 0.5f, 0.2f, 1f);
        private readonly Color PlayerHighlightColor = new Color(0.3f, 0.6f, 1f, 1f);

        public override void _Ready()
        {
            _instance = this;
            SetupUI();
            ConnectSignals();
            Hide();
        }

        private void SetupUI()
        {
            // Main panel
            _mainPanel = new PanelContainer
            {
                Name = "LeaderboardPanel",
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -250,
                OffsetTop = -200,
                OffsetRight = 250,
                OffsetBottom = 200,
                GrowHorizontal = Control.GrowDirection.Center,
                GrowVertical = Control.GrowDirection.Center
            };
            AddChild(_mainPanel);

            // Style
            var panelStyle = new StyleBoxFlat();
            panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            panelStyle.CornerRadiusTopLeft = 12;
            panelStyle.CornerRadiusTopRight = 12;
            panelStyle.CornerRadiusBottomLeft = 12;
            panelStyle.CornerRadiusBottomRight = 12;
            panelStyle.BorderWidthLeft = 2;
            panelStyle.BorderWidthTop = 2;
            panelStyle.BorderWidthRight = 2;
            panelStyle.BorderWidthBottom = 2;
            panelStyle.BorderColor = new Color(0.3f, 0.5f, 0.8f, 0.8f);
            _mainPanel.AddThemeStyleBoxOverride("panel", panelStyle);

            // Content container
            _contentContainer = new VBoxContainer
            {
                Name = "ContentContainer",
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 15,
                OffsetTop = 15,
                OffsetRight = -15,
                OffsetBottom = -15
            };
            _mainPanel.AddChild(_contentContainer);

            // Title
            _titleLabel = new Label
            {
                Text = "🏆 排行榜",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", GoldColor);
            _contentContainer.AddChild(_titleLabel);

            // Category selector
            _categorySelector = new OptionButton
            {
                Name = "CategorySelector",
                CustomMinimumHeight = 35
            };
            PopulateCategorySelector();
            _categorySelector.ItemSelected += OnCategorySelected;
            _contentContainer.AddChild(_categorySelector);

            // Separator
            AddHSeparator();

            // Player stats row
            _playerStatsContainer = new HBoxContainer
            {
                Name = "PlayerStats",
                Alignment = BoxContainer.AlignmentMode.Center
            };
            _contentContainer.AddChild(_playerStatsContainer);

            _yourRankLabel = new Label
            {
                Text = "你的排名: --",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _yourRankLabel.AddThemeFontSizeOverride("font_size", 14);
            _yourRankLabel.AddThemeColorOverride("font_color", PlayerHighlightColor);
            _playerStatsContainer.AddChild(_yourRankLabel);

            var spacer = new Control
            {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            _playerStatsContainer.AddChild(spacer);

            _yourValueLabel = new Label
            {
                Text = "数值: 0",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            _yourValueLabel.AddThemeFontSizeOverride("font_size", 14);
            _yourValueLabel.AddThemeColorOverride("font_color", Colors.White);
            _playerStatsContainer.AddChild(_yourValueLabel);

            // Separator
            AddHSeparator();

            // Leaderboard scroll
            _leaderboardScroll = new ScrollContainer
            {
                Name = "LeaderboardScroll",
                VerticalScrollMode = ScrollContainer.ScrollMode.Enabled,
                CustomMinimumHeight = 250
            };
            _contentContainer.AddChild(_leaderboardScroll);

            // Entries container
            _entriesContainer = new VBoxContainer
            {
                Name = "EntriesContainer"
            };
            _leaderboardScroll.AddChild(_entriesContainer);

            // Initial load
            RefreshLeaderboard();
        }

        private void PopulateCategorySelector()
        {
            _categorySelector.Clear();
            
            foreach (MultiplayerLeaderboard.LeaderboardCategory category 
                in Enum.GetValues(typeof(MultiplayerLeaderboard.LeaderboardCategory)))
            {
                _categorySelector.AddItem(
                    MultiplayerLeaderboard.GetCategoryName(category),
                    (int)category
                );
            }
        }

        private void AddHSeparator()
        {
            var separator = new HSeparator
            {
                Modulate = new Color(0.4f, 0.5f, 0.7f, 0.5f),
                CustomMinimumHeight = 2
            };
            _contentContainer.AddChild(separator);
        }

        private void ConnectSignals()
        {
            if (MultiplayerLeaderboard.Instance != null)
            {
                MultiplayerLeaderboard.Instance.OnLeaderboardUpdated += OnLeaderboardUpdated;
            }
        }

        private void OnLeaderboardUpdated(MultiplayerLeaderboard.LeaderboardCategory category)
        {
            if (category == _currentCategory)
            {
                RefreshLeaderboard();
            }
        }

        private void OnCategorySelected(long index)
        {
            _currentCategory = (MultiplayerLeaderboard.LeaderboardCategory)index;
            RefreshLeaderboard();
        }

        private void RefreshLeaderboard()
        {
            // Clear existing entries
            foreach (Node child in _entriesContainer.GetChildren())
            {
                child.QueueFree();
            }

            // Get entries
            var entries = MultiplayerLeaderboard.Instance.GetLeaderboard(_currentCategory, 10);
            
            // Get player info
            var playerRank = MultiplayerLeaderboard.Instance.GetPlayerRank(_currentCategory);
            var playerValue = MultiplayerLeaderboard.Instance.GetPlayerValue(_currentCategory);

            // Update stats
            _yourRankLabel.Text = playerRank > 0 ? $"你的排名: #{playerRank}" : "你的排名: --";
            _yourValueLabel.Text = $"数值: {playerValue:N0}";

            // Add entries
            if (entries.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "暂无数据",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                emptyLabel.AddThemeFontSizeOverride("font_size", 16);
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f, 1f));
                _entriesContainer.AddChild(emptyLabel);
                return;
            }

            int displayIndex = 0;
            foreach (var entry in entries)
            {
                displayIndex++;
                var entryContainer = CreateEntryRow(entry, displayIndex, playerRank > 0 && entry.Rank == playerRank);
                _entriesContainer.AddChild(entryContainer);
            }
        }

        private Control CreateEntryRow(MultiplayerLeaderboard.LeaderboardEntry entry, int displayIndex, bool isPlayer)
        {
            var container = new HBoxContainer
            {
                CustomMinimumHeight = 32,
                Alignment = BoxContainer.AlignmentMode.Center
            };
            
            if (isPlayer)
            {
                var highlightStyle = new StyleBoxFlat();
                highlightStyle.BgColor = new Color(0.2f, 0.4f, 0.7f, 0.3f);
                highlightStyle.CornerRadiusTopLeft = 4;
                highlightStyle.CornerRadiusTopRight = 4;
                highlightStyle.CornerRadiusBottomLeft = 4;
                highlightStyle.CornerRadiusBottomRight = 4;
                
                var panel = new PanelContainer
                {
                    MouseFilter = Control.MouseFilterEnum.Ignore
                };
                panel.AddThemeStyleBoxOverride("panel", highlightStyle);
                container.AddChild(panel);
                
                // Use container for children
                var innerContainer = container;
                container = panel;
                container.AddChild(innerContainer);
            }

            // Rank
            var rankLabel = new Label
            {
                Text = $"#{entry.Rank}",
                CustomMinimumWidth = 45,
                HorizontalAlignment = HorizontalAlignment.Center
            };
            rankLabel.AddThemeFontSizeOverride("font_size", 14);
            
            // Rank color
            Color rankColor;
            switch (entry.Rank)
            {
                case 1:
                    rankColor = GoldColor;
                    break;
                case 2:
                    rankColor = SilverColor;
                    break;
                case 3:
                    rankColor = BronzeColor;
                    break;
                default:
                    rankColor = Colors.White;
                    break;
            }
            rankLabel.AddThemeColorOverride("font_color", rankColor);
            container.AddChild(rankLabel);

            // Player name
            var nameLabel = new Label
            {
                Text = entry.PlayerName,
                SizeFlagsHorizontal = Control.SizeFlags.Expand,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            nameLabel.AddThemeColorOverride("font_color", isPlayer ? PlayerHighlightColor : Colors.LightGray);
            container.AddChild(nameLabel);

            // Value
            var valueLabel = new Label
            {
                Text = entry.Value.ToString("N0"),
                CustomMinimumWidth = 80,
                HorizontalAlignment = HorizontalAlignment.Right
            };
            valueLabel.AddThemeFontSizeOverride("font_size", 14);
            valueLabel.AddThemeColorOverride("font_color", Colors.Yellow);
            container.AddChild(valueLabel);

            return container;
        }

        public void Toggle()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                RefreshLeaderboard();
                PlayAppearAnimation();
            }
        }

        private void PlayAppearAnimation()
        {
            _mainPanel.Modulate = new Color(1f, 1f, 1f, 0f);
            _mainPanel.Scale = new Vector2(0.9f, 0.9f);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_mainPanel, "modulate:a", 1f, 0.25f);
            tween.TweenProperty(_mainPanel, "scale", new Vector2(1f, 1f), 0.25f).SetTrans(Tween.TransitionType.Back).SetEasing(Tween.EasingFunction.EaseOut);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
            {
                if (keyEvent.Keycode == Key.L)
                {
                    Toggle();
                }
                else if (keyEvent.Keycode == Key.Escape && Visible)
                {
                    Hide();
                }
            }
        }
    }
}
