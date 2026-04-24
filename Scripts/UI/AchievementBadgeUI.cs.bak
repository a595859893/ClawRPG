using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.UI {
    public partial class AchievementBadgeUI : Control
    {
        private GridContainer _badgeGrid;
        private Label _titleLabel;
        private Label _statsLabel;
        private int _totalBadges = 0;
        private int _earnedBadges = 0;

        public override void _Ready()
        {
            SetupUI();
            RefreshBadges();
        }

        private void SetupUI()
        {
            var mainPanel = new PanelContainer();
            mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainPanel.CustomMinimumSize = new Vector2(600, 500);
            AddChild(mainPanel);

            var vbox = new VBoxContainer();
            mainPanel.AddChild(vbox);

            // 标题
            _titleLabel = new Label
            {
                Text = "🎖️ 成就徽章",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(_titleLabel);

            // 统计
            _statsLabel = new Label
            {
                Text = "收集进度: 0/0",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _statsLabel.AddThemeFontSizeOverride("font_size", 16);
            vbox.AddChild(_statsLabel);

            // 分割线
            var hsep = new HSeparator();
            vbox.AddChild(hsep);

            // 徽章网格 (4列)
            _badgeGrid = new GridContainer
            {
                Columns = 4,
                HorizontalExpand = true,
                VerticalExpand = true
            };
            _badgeGrid.CustomMinimumSize = new Vector2(560, 350);
            vbox.AddChild(_badgeGrid);

            // 说明
            var infoLabel = new Label
            {
                Text = "铜徽章 → 银徽章 → 金徽章 → 钻石徽章",
                HorizontalAlignment = HorizontalAlignment.Center,
                Modulate = new Color(0.7f, 0.7f, 0.7f)
            };
            vbox.AddChild(infoLabel);
        }

        public void RefreshBadges()
        {
            // 清空现有
            foreach (Node child in _badgeGrid.GetChildren())
                child.QueueFree();

            var badgeSystem = AchievementBadgeSystem.Instance;
            if (badgeSystem == null) return;

            var allBadges = badgeSystem.GetAllBadges();
            _totalBadges = allBadges.Count;
            _earnedBadges = 0;

            // 假设已有成就系统
            var achievementManager = AchievementManager.Instance;
            if (achievementManager != null)
            {
                foreach (var badge in allBadges)
                {
                    bool earned = CheckBadgeEarned(badge.BadgeId, achievementManager);
                    if (earned) _earnedBadges++;
                    CreateBadgeDisplay(badge, earned);
                }
            }
            else
            {
                foreach (var badge in allBadges)
                {
                    CreateBadgeDisplay(badge, false);
                }
            }

            _statsLabel.Text = $"收集进度: {_earnedBadges}/{_totalBadges}";
        }

        private bool CheckBadgeEarned(string badgeId, AchievementManager am)
        {
            // 根据徽章ID检查对应成就是否完成
            return badgeId switch
            {
                "first_blood" => am.HasAchievement("kill_first_enemy"),
                "collector" => am.HasAchievement("collect_10_items"),
                "explorer" => am.HasAchievement("discover_5_regions"),
                "warrior" => am.HasAchievement("kill_100_enemies"),
                "wealthy" => am.HasAchievement("have_10000_gold"),
                "team_player" => am.HasAchievement("complete_10_teams"),
                "boss_slayer" => am.HasAchievement("kill_10_bosses"),
                "master_crafter" => am.HasAchievement("craft_50_equipment"),
                "legend" => am.HasAchievement("reach_max_level"),
                "champion" => am.HasAchievement("complete_game"),
                "perfectionist" => am.HasAchievement("all_achievements"),
                _ => false
            };
        }

        private void CreateBadgeDisplay(AchievementBadge badge, bool earned)
        {
            var container = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(120, 120),
                Alignment = BoxContainer.AlignmentMode.Center
            };

            // 徽章图标
            var iconLabel = new Label
            {
                Text = GetBadgeIcon(badge.IconName),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconLabel.AddThemeFontSizeOverride("font_size", 40);
            
            if (!earned && badge.IsSecret)
            {
                iconLabel.Modulate = new Color(0.3f, 0.3f, 0.3f);
            }
            else if (!earned)
            {
                iconLabel.Modulate = new Color(0.4f, 0.4f, 0.4f);
            }
            else
            {
                iconLabel.Modulate = badge.BadgeColor;
            }
            container.AddChild(iconLabel);

            // 徽章名称
            var nameLabel = new Label
            {
                Text = earned || !badge.IsSecret ? badge.DisplayName : "???",
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 12);
            if (!earned) nameLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            container.AddChild(nameLabel);

            // 边框指示
            if (earned)
            {
                var tierLabel = new Label
                {
                    Text = GetTierText(badge.Tier),
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                tierLabel.AddThemeFontSizeOverride("font_size", 10);
                tierLabel.Modulate = badge.BadgeColor;
                container.AddChild(tierLabel);
            }

            _badgeGrid.AddChild(container);
        }

        private string GetBadgeIcon(string iconName)
        {
            return iconName switch
            {
                "sword" => "⚔️",
                "chest" => "📦",
                "map" => "🗺️",
                "shield" => "🛡️",
                "coin" => "💰",
                "users" => "👥",
                "crown" => "👑",
                "hammer" => "🔨",
                "star" => "⭐",
                "trophy" => "🏆",
                "gem" => "💎",
                _ => "🎖️"
            };
        }

        private string GetTierText(int tier)
        {
            return tier switch
            {
                1 => "🥉",
                2 => "🥈",
                3 => "🥇",
                4 => "💠",
                _ => ""
            };
        }

        public static void Show()
        {
            var ui = new AchievementBadgeUI();
            ui.Name = "AchievementBadgeUI";
            var canvas = UI.Instance?.GetTree()?.Root;
            if (canvas != null)
            {
                canvas.AddChild(ui);
            }
        }
    }
}
