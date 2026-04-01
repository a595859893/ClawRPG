using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Leaderboard {
    /// <summary>
    /// 排行榜UI系统
    /// </summary>
    public partial class LeaderboardUI : Control {
        // UI组件
        private TabContainer _tabContainer;
        private OptionButton _leaderboardTypeSelector;
        private OptionButton _periodSelector;
        private VBoxContainer _leaderboardList;
        private Label _titleLabel;
        private Label _statsLabel;
        private Button _refreshButton;
        private Button _closeButton;

        // 数据
        private LeaderboardType _currentType = LeaderboardType.PlayerLevel;
        private LeaderboardPeriod _currentPeriod = LeaderboardPeriod.AllTime;
        private List<LeaderboardEntry> _currentEntries = new List<LeaderboardEntry>();

        // 主题颜色
        private Color _goldColor = new Color(1f, 0.84f, 0f);
        private Color _silverColor = new Color(0.75f, 0.75f, 0.75f);
        private Color _bronzeColor = new Color(0.8f, 0.5f, 0.2f);
        private Color _rankUpColor = new Color(0.2f, 0.8f, 0.2f);
        private Color _rankDownColor = new Color(0.8f, 0.2f, 0.2f);

        private LeaderboardSystem _system;
        private LeaderboardDatabase _database;

        public override void _Ready() {
            _system = LeaderboardSystem.Instance;
            _database = GetNode<LeaderboardDatabase>("/root/Main/LeaderboardDatabase");

            SetupUI();
            RefreshLeaderboard();
        }

        private void SetupUI() {
            // 主容器
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorAndMargin(AnchorsPreset.FullRect, 0f);
            mainContainer.AddThemeConstantOverride("separation", 10);
            AddChild(mainContainer);

            // 标题栏
            var titleBar = new HBoxContainer();
            titleBar.AddThemeConstantOverride("separation", 10);
            mainContainer.AddChild(titleBar);

            _titleLabel = new Label();
            _titleLabel.Text = "Leaderboard";
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleBar.AddChild(_titleLabel);

            titleBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            _closeButton = new Button();
            _closeButton.Text = "X";
            _closeButton.RectMinSize = new Vector2(30, 30);
            _closeButton.Pressed += OnClosePressed;
            titleBar.AddChild(_closeButton);

            // 控制栏
            var controlBar = new HBoxContainer();
            controlBar.AddThemeConstantOverride("separation", 10);
            mainContainer.AddChild(controlBar);

            // 类型选择
            var typeLabel = new Label();
            typeLabel.Text = "Type:";
            controlBar.AddChild(typeLabel);

            _leaderboardTypeSelector = new OptionButton();
            PopulateTypeSelector();
            _leaderboardTypeSelector.ItemSelected += OnTypeSelected;
            controlBar.AddChild(_leaderboardTypeSelector);

            // 时间周期选择
            var periodLabel = new Label();
            periodLabel.Text = "Period:";
            controlBar.AddChild(periodLabel);

            _periodSelector = new OptionButton();
            _periodSelector.AddItem("All Time", (int)LeaderboardPeriod.AllTime);
            _periodSelector.AddItem("Monthly", (int)LeaderboardPeriod.Monthly);
            _periodSelector.AddItem("Weekly", (int)LeaderboardPeriod.Weekly);
            _periodSelector.AddItem("Daily", (int)LeaderboardPeriod.Daily);
            _periodSelector.ItemSelected += OnPeriodSelected;
            controlBar.AddChild(_periodSelector);

            controlBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            _refreshButton = new Button();
            _refreshButton.Text = "Refresh";
            _refreshButton.Pressed += OnRefreshPressed;
            controlBar.AddChild(_refreshButton);

            // 统计信息
            _statsLabel = new Label();
            _statsLabel.AddThemeFontSizeOverride("font_size", 14);
            _statsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            mainContainer.AddChild(_statsLabel);

            // 排行榜列表（使用滚动容器）
            var scrollContainer = new ScrollContainer();
            scrollContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
            mainContainer.AddChild(scrollContainer);

            _leaderboardList = new VBoxContainer();
            _leaderboardList.AddThemeConstantOverride("separation", 2);
            scrollContainer.AddChild(_leaderboardList);

            // 设置滚动容器的子节点为列表
            scrollContainer.AddChild(_leaderboardList);
            scrollContainer.SetAnchorAndMargin(AnchorsPreset.FullRect, 0f);
            scrollContainer.MarginTop = 120f;
            scrollContainer.MarginBottom = -40f;
        }

        private void PopulateTypeSelector() {
            _leaderboardTypeSelector.Clear();
            _leaderboardTypeSelector.AddItem("Player Level", (int)LeaderboardType.PlayerLevel);
            _leaderboardTypeSelector.AddItem("Gold", (int)LeaderboardType.Gold);
            _leaderboardTypeSelector.AddItem("Achievements", (int)LeaderboardType.Achievements);
            _leaderboardTypeSelector.AddItem("Arena Wins", (int)LeaderboardType.ArenaWins);
            _leaderboardTypeSelector.AddItem("Dungeon Completed", (int)LeaderboardType.DungeonCompleted);
            _leaderboardTypeSelector.AddItem("Boss Kills", (int)LeaderboardType.BossKills);
            _leaderboardTypeSelector.AddItem("Pet Strength", (int)LeaderboardType.PetStrength);
            _leaderboardTypeSelector.AddItem("Crafting Mastery", (int)LeaderboardType.CraftingMastery);
            _leaderboardTypeSelector.AddItem("Guild Points", (int)LeaderboardType.GuildPoints);
            _leaderboardTypeSelector.AddItem("Cross-Server Rating", (int)LeaderboardType.CrossServerRating);
            _leaderboardTypeSelector.AddItem("Mythic+ Score", (int)LeaderboardType.MythicPlusScore);
            _leaderboardTypeSelector.AddItem("Combo Chain", (int)LeaderboardType.ComboChain);
            _leaderboardTypeSelector.AddItem("Total Damage", (int)LeaderboardType.TotalDamage);
            _leaderboardTypeSelector.AddItem("Total Healing", (int)LeaderboardType.TotalHealing);
        }

        private void RefreshLeaderboard() {
            if (_system == null) return;

            var config = _database?.GetConfig(_currentType);
            if (config != null) {
                _titleLabel.Text = config.DisplayName;
            }

            _currentEntries = _system.GetLeaderboard(_currentType, 0, 100);
            UpdateLeaderboardList();

            var stats = _system.GetStatistics(_currentType);
            if (stats != null) {
                _statsLabel.Text = $"Total Players: {stats.TotalEntries} | Highest Score: {FormatScore(stats.HighestScore)} | Last Update: {stats.LastUpdate:HH:mm:ss}";
            } else {
                _statsLabel.Text = "No data available";
            }
        }

        private void UpdateLeaderboardList() {
            // 清除现有项
            foreach (var child in _leaderboardList.GetChildren()) {
                child.QueueFree();
            }

            // 添加表头
            var header = CreateEntryRow("Rank", "Player", "Score", "Change", true);
            _leaderboardList.AddChild(header);

            // 添加排行榜条目
            foreach (var entry in _currentEntries) {
                var row = CreateEntryRow(
                    entry.Rank.ToString(),
                    entry.PlayerName,
                    FormatScore(entry.Value),
                    GetRankChangeText(entry),
                    false,
                    entry.Rank
                );
                _leaderboardList.AddChild(row);
            }

            if (_currentEntries.Count == 0) {
                var emptyLabel = new Label();
                emptyLabel.Text = "No entries yet";
                emptyLabel.Align = Label.AlignEnum.Center;
                _leaderboardList.AddChild(emptyLabel);
            }
        }

        private Control CreateEntryRow(string rank, string player, string score, string change, bool isHeader, int rankNum = 0) {
            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 10);

            // 排名
            var rankLabel = new Label();
            rankLabel.Text = rank;
            rankLabel.CustomMinimumSize = new Vector2(60, 0);
            rankLabel.Align = Label.AlignEnum.Center;

            if (!isHeader) {
                if (rankNum == 1) rankLabel.AddThemeColorOverride("font_color", _goldColor);
                else if (rankNum == 2) rankLabel.AddThemeColorOverride("font_color", _silverColor);
                else if (rankNum == 3) rankLabel.AddThemeColorOverride("font_color", _bronzeColor);
            }
            container.AddChild(rankLabel);

            // 玩家名
            var playerLabel = new Label();
            playerLabel.Text = player;
            playerLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            container.AddChild(playerLabel);

            // 分数
            var scoreLabel = new Label();
            scoreLabel.Text = score;
            scoreLabel.CustomMinimumSize = new Vector2(120, 0);
            scoreLabel.Align = Label.AlignEnum.Right;
            container.AddChild(scoreLabel);

            // 变化
            var changeLabel = new Label();
            changeLabel.Text = change;
            changeLabel.CustomMinimumSize = new Vector2(80, 0);
            changeLabel.Align = Label.AlignEnum.Center;
            container.AddChild(changeLabel);

            return container;
        }

        private string GetRankChangeText(LeaderboardEntry entry) {
            int change = entry.PreviousRank - entry.Rank;
            if (change > 0) {
                return $"▲{change}";
            } else if (change < 0) {
                return $"▼{Math.Abs(change)}";
            }
            return "-";
        }

        private string FormatScore(long score) {
            if (score >= 1000000000) {
                return $"{score / 1000000000.0:F1}B";
            } else if (score >= 1000000) {
                return $"{score / 1000000.0:F1}M";
            } else if (score >= 1000) {
                return $"{score / 1000.0:F1}K";
            }
            return score.ToString();
        }

        private void OnTypeSelected(int index) {
            _currentType = (LeaderboardType)index;
            RefreshLeaderboard();
        }

        private void OnPeriodSelected(int index) {
            _currentPeriod = (LeaderboardPeriod)index;
            RefreshLeaderboard();
        }

        private void OnRefreshPressed() {
            RefreshLeaderboard();
        }

        private void OnClosePressed() {
            Visible = !Visible;
        }

        public void ToggleVisibility() {
            Visible = !Visible;
            if (Visible) {
                RefreshLeaderboard();
            }
        }
    }
}
