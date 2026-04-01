using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.GuildQuestBoard {
    /**
     * GuildQuestBoardUI - 公会任务布告栏界面
     * 显示任务列表，接受任务，查看进度
     */
    public partial class GuildQuestBoardUI : Control {
        // 单例
        private static GuildQuestBoardUI _instance;
        public static GuildQuestBoardUI Instance => _instance;
        
        // 节点引用
        private VBoxContainer _mainContainer;
        private HBoxContainer _headerContainer;
        private TabContainer _tabContainer;
        
        // 任务列表
        private GridContainer _questGrid;
        private GridContainer _myQuestGrid;
        private GridContainer _dailyQuestGrid;
        
        // 筛选
        private OptionButton _typeFilter;
        private OptionButton _difficultyFilter;
        
        // 统计面板
        private Label _totalQuestsLabel;
        private Label _completedQuestsLabel;
        private Label _todayPublishedLabel;
        
        // 详细信息
        private Panel _detailPanel;
        private Label _detailTitle;
        private Label _detailDescription;
        private Label _detailType;
        private Label _detailDifficulty;
        private Label _detailProgress;
        private Label _detailReward;
        private Button _acceptButton;
        
        // 当前选中的任务
        private QuestBoardQuest _selectedQuest;
        
        // 刷新计时器
        private float _refreshTimer = 0f;
        private const float REFRESH_INTERVAL = 1.0f;
        
        public override void _Ready() {
            _instance = this;
            SetupUI();
            SetupSignals();
            RefreshQuests();
        }
        
        private void SetupUI() {
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorPreset(ControlPreset.FullRect);
            _mainContainer.AddThemeConstantOverride("separation", 10);
            AddChild(_mainContainer);
            
            // 标题栏
            _headerContainer = new HBoxContainer();
            _headerContainer.AddThemeConstantOverride("separation", 20);
            _mainContainer.AddChild(_headerContainer);
            
            var titleLabel = new Label();
            titleLabel.Text = "📋 公会任务布告栏";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _headerContainer.AddChild(titleLabel);
            
            _headerContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            // 刷新按钮
            var refreshBtn = new Button();
            refreshBtn.Text = "🔄 刷新";
            refreshBtn.Pressed += OnRefreshPressed;
            _headerContainer.AddChild(refreshBtn);
            
            // 关闭按钮
            var closeBtn = new Button();
            closeBtn.Text = "✕ 关闭";
            closeBtn.Pressed += OnClosePressed;
            _headerContainer.AddChild(closeBtn);
            
            // 标签页容器
            _tabContainer = new TabContainer();
            _tabContainer.SetVSizeFlags(Control.SizeFlags.Expand);
            _mainContainer.AddChild(_tabContainer);
            
            // 创建标签页
            CreateAllQuestsTab();
            CreateMyQuestsTab();
            CreateDailyQuestsTab();
            CreateStatsTab();
            
            // 详细信息面板
            CreateDetailPanel();
            
            // 筛选栏
            CreateFilterBar();
        }
        
        private void CreateAllQuestsTab() {
            var scroll = new ScrollContainer();
            scroll.Name = "全部任务";
            _tabContainer.AddChild(scroll);
            
            _questGrid = new GridContainer();
            _questGrid.Columns = 1;
            _questGrid.AddThemeConstantOverride("h_separation", 10);
            _questGrid.AddThemeConstantOverride("v_separation", 10);
            _questGrid.SetAnchorPreset(ControlPreset.FullRect);
            _questGrid.AddThemeConstantOverride("margin_left", 10);
            _questGrid.AddThemeConstantOverride("margin_top", 10);
            _questGrid.AddThemeConstantOverride("margin_right", 10);
            _questGrid.AddThemeConstantOverride("margin_bottom", 10);
            scroll.AddChild(_questGrid);
        }
        
        private void CreateMyQuestsTab() {
            var scroll = new ScrollContainer();
            scroll.Name = "我的任务";
            _tabContainer.AddChild(scroll);
            
            _myQuestGrid = new GridContainer();
            _myQuestGrid.Columns = 1;
            _myQuestGrid.AddThemeConstantOverride("h_separation", 10);
            _myQuestGrid.AddThemeConstantOverride("v_separation", 10);
            _myQuestGrid.SetAnchorPreset(ControlPreset.FullRect);
            _myQuestGrid.AddThemeConstantOverride("margin_left", 10);
            _myQuestGrid.AddThemeConstantOverride("margin_top", 10);
            _myQuestGrid.AddThemeConstantOverride("margin_right", 10);
            _myQuestGrid.AddThemeConstantOverride("margin_bottom", 10);
            scroll.AddChild(_myQuestGrid);
        }
        
        private void CreateDailyQuestsTab() {
            var scroll = new ScrollContainer();
            scroll.Name = "每日任务";
            _tabContainer.AddChild(scroll);
            
            _dailyQuestGrid = new GridContainer();
            _dailyQuestGrid.Columns = 1;
            _dailyQuestGrid.AddThemeConstantOverride("h_separation", 10);
            _dailyQuestGrid.AddThemeConstantOverride("v_separation", 10);
            _dailyQuestGrid.SetAnchorPreset(ControlPreset.FullRect);
            _dailyQuestGrid.AddThemeConstantOverride("margin_left", 10);
            _dailyQuestGrid.AddThemeConstantOverride("margin_top", 10);
            _dailyQuestGrid.AddThemeConstantOverride("margin_right", 10);
            _dailyQuestGrid.AddThemeConstantOverride("margin_bottom", 10);
            scroll.AddChild(_dailyQuestGrid);
        }
        
        private void CreateStatsTab() {
            var statsContainer = new VBoxContainer();
            statsContainer.Name = "统计";
            _tabContainer.AddChild(statsContainer);
            
            // 统计标题
            var statsTitle = new Label();
            statsTitle.Text = "📊 任务统计";
            statsTitle.AddThemeFontSizeOverride("font_size", 20);
            statsContainer.AddChild(statsTitle);
            
            // 统计内容
            var statsScroll = new ScrollContainer();
            statsScroll.SetVSizeFlags(Control.SizeFlags.Expand);
            statsContainer.AddChild(statsScroll);
            
            var statsGrid = new GridContainer();
            statsGrid.Columns = 2;
            statsGrid.AddThemeConstantOverride("h_separation", 20);
            statsGrid.AddThemeConstantOverride("v_separation", 10);
            statsScroll.AddChild(statsGrid);
            
            // 总任务数
            var totalLabel = new Label();
            totalLabel.Text = "总任务数:";
            statsGrid.AddChild(totalLabel);
            
            _totalQuestsLabel = new Label();
            _totalQuestsLabel.Text = "0";
            statsGrid.AddChild(_totalQuestsLabel);
            
            // 已完成任务数
            var completedLabel = new Label();
            completedLabel.Text = "已完成任务:";
            statsGrid.AddChild(completedLabel);
            
            _completedQuestsLabel = new Label();
            _completedQuestsLabel.Text = "0";
            statsGrid.AddChild(_completedQuestsLabel);
            
            // 今日发布
            var todayLabel = new Label();
            todayLabel.Text = "今日发布:";
            statsGrid.AddChild(todayLabel);
            
            _todayPublishedLabel = new Label();
            _todayPublishedLabel.Text = "0";
            statsGrid.AddChild(_todayPublishedLabel);
        }
        
        private void CreateDetailPanel() {
            _detailPanel = new Panel();
            _detailPanel.SetAnchorPreset(ControlPreset.FullRect);
            _detailPanel.Visible = false;
            _mainContainer.AddChild(_detailPanel);
            
            var detailContainer = new VBoxContainer();
            detailContainer.SetAnchorPreset(ControlPreset.FullRect);
            detailContainer.AddThemeConstantOverride("separation", 15);
            detailContainer.AddThemeConstantOverride("margin_left", 20);
            detailContainer.AddThemeConstantOverride("margin_top", 20);
            detailContainer.AddThemeConstantOverride("margin_right", 20);
            detailContainer.AddThemeConstantOverride("margin_bottom", 20);
            _detailPanel.AddChild(detailContainer);
            
            // 标题
            _detailTitle = new Label();
            _detailTitle.AddThemeFontSizeOverride("font_size", 22);
            detailContainer.AddChild(_detailTitle);
            
            // 描述
            _detailDescription = new Label();
            _detailDescription.AutowrapMode = TextServer.AutowrapMode.WordSmart;
            detailContainer.AddChild(_detailDescription);
            
            // 类型和难度
            _detailType = new Label();
            detailContainer.AddChild(_detailType);
            
            _detailDifficulty = new Label();
            detailContainer.AddChild(_detailDifficulty);
            
            // 进度
            _detailProgress = new Label();
            detailContainer.AddChild(_detailProgress);
            
            // 奖励
            _detailReward = new Label();
            _detailReward.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
            detailContainer.AddChild(_detailReward);
            
            // 按钮容器
            var buttonContainer = new HBoxContainer();
            buttonContainer.AddThemeConstantOverride("separation", 10);
            detailContainer.AddChild(buttonContainer);
            
            // 接受按钮
            _acceptButton = new Button();
            _acceptButton.Text = "✅ 接受任务";
            _acceptButton.Pressed += OnAcceptPressed;
            buttonContainer.AddChild(_acceptButton);
            
            // 放弃按钮
            var abandonButton = new Button();
            abandonButton.Text = "❌ 放弃任务";
            abandonButton.Pressed += OnAbandonPressed;
            buttonContainer.AddChild(abandonButton);
            
            // 返回按钮
            var backButton = new Button();
            backButton.Text = "← 返回";
            backButton.Pressed += OnBackPressed;
            buttonContainer.AddChild(backButton);
        }
        
        private void CreateFilterBar() {
            var filterContainer = new HBoxContainer();
            filterContainer.AddThemeConstantOverride("separation", 15);
            _mainContainer.AddChild(filterContainer);
            
            // 类型筛选
            var typeLabel = new Label();
            typeLabel.Text = "类型:";
            filterContainer.AddChild(typeLabel);
            
            _typeFilter = new OptionButton();
            _typeFilter.AddItem("全部", 0);
            _typeFilter.AddItem("战斗", (int)QuestType.Combat);
            _typeFilter.AddItem("采集", (int)QuestType.Gathering);
            _typeFilter.AddItem("制作", (int)QuestType.Crafting);
            _typeFilter.AddItem("送货", (int)QuestType.Delivery);
            _typeFilter.AddItem("救援", (int)QuestType.Rescue);
            _typeFilter.AddItem("狩猎", (int)QuestType.Hunt);
            _typeFilter.AddItem("Boss", (int)QuestType.Boss);
            _typeFilter.AddItem("护送", (int)QuestType.Escort);
            _typeFilter.AddItem("探索", (int)QuestType.Exploration);
            _typeFilter.AddItem("时限", (int)QuestType.Timed);
            _typeFilter.ItemSelected += OnTypeFilterChanged;
            filterContainer.AddChild(_typeFilter);
            
            // 难度筛选
            var diffLabel = new Label();
            diffLabel.Text = "难度:";
            filterContainer.AddChild(diffLabel);
            
            _difficultyFilter = new OptionButton();
            _difficultyFilter.AddItem("全部", 0);
            _difficultyFilter.AddItem("简单", (int)Difficulty.Easy);
            _difficultyFilter.AddItem("普通", (int)Difficulty.Normal);
            _difficultyFilter.AddItem("困难", (int)Difficulty.Hard);
            _difficultyFilter.AddItem("史诗", (int)Difficulty.Epic);
            _difficultyFilter.AddItem("传说", (int)Difficulty.Legendary);
            _difficultyFilter.ItemSelected += OnDifficultyFilterChanged;
            filterContainer.AddChild(_difficultyFilter);
            
            filterContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        }
        
        private void SetupSignals() {
            // 连接系统信号
            if (GuildQuestBoardSystem.Instance != null) {
                // Quest signals would be connected here
            }
        }
        
        public override void _Process(double delta) {
            _refreshTimer += delta;
            if (_refreshTimer >= REFRESH_INTERVAL) {
                _refreshTimer = 0;
                RefreshQuests();
            }
        }
        
        private void RefreshQuests() {
            var system = GuildQuestBoardSystem.Instance;
            if (system == null) return;
            
            // 刷新全部任务
            RefreshQuestList(_questGrid, system.GetAvailableQuests());
            
            // 刷新我的任务（简化版本，使用"玩家"名称）
            RefreshQuestList(_myQuestGrid, system.GetAcceptedQuests("Player"));
            
            // 刷新每日任务
            RefreshQuestList(_dailyQuestGrid, system.GetDailyQuests());
            
            // 刷新统计
            var stats = system.GetStatistics();
            _totalQuestsLabel.Text = stats["total_quests"].ToString();
            _completedQuestsLabel.Text = stats["completed_quests"].ToString();
            _todayPublishedLabel.Text = stats["today_published"].ToString();
        }
        
        private void RefreshQuestList(GridContainer grid, List<QuestBoardQuest> quests) {
            // 清除现有项
            foreach (var child in grid.GetChildren()) {
                child.QueueFree();
            }
            
            // 添加任务卡片
            foreach (var quest in quests) {
                var card = CreateQuestCard(quest);
                grid.AddChild(card);
            }
            
            // 空列表提示
            if (quests.Count == 0) {
                var emptyLabel = new Label();
                emptyLabel.Text = "暂无任务";
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
                grid.AddChild(emptyLabel);
            }
        }
        
        private Control CreateQuestCard(QuestBoardQuest quest) {
            var card = new PanelContainer();
            card.CustomMinimumSize = new Vector2(0, 80);
            
            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 15);
            card.AddChild(container);
            
            // 任务图标
            var iconLabel = new Label();
            iconLabel.Text = GetQuestTypeIcon(quest.QuestType);
            iconLabel.AddThemeFontSizeOverride("font_size", 24);
            container.AddChild(iconLabel);
            
            // 任务信息
            var infoContainer = new VBoxContainer();
            infoContainer.AddThemeConstantOverride("separation", 5);
            container.AddChild(infoContainer);
            
            // 标题
            var titleLabel = new Label();
            titleLabel.Text = quest.Title;
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            infoContainer.AddChild(titleLabel);
            
            // 描述
            var descLabel = new Label();
            descLabel.Text = quest.Description;
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            descLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            infoContainer.AddChild(descLabel);
            
            // 难度和奖励
            var metaLabel = new Label();
            metaLabel.Text = $"{GetDifficultyText(quest.Difficulty)} | 🪙 {quest.RewardGold} | ✨ {quest.RewardExp} | 🏛️ {quest.RewardGuildPoints}";
            metaLabel.AddThemeFontSizeOverride("font_size", 12);
            metaLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            infoContainer.AddChild(metaLabel);
            
            // 进度（如果有）
            if (quest.CurrentProgress > 0) {
                var progressLabel = new Label();
                progressLabel.Text = $"进度: {quest.CurrentProgress}/{quest.RequiredCount}";
                progressLabel.AddThemeFontSizeOverride("font_size", 12);
                progressLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.8f, 1f));
                infoContainer.AddChild(progressLabel);
            }
            
            container.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            // 点击事件
            card.GuiInput += (inputEvent) => {
                if (inputEvent is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left) {
                    ShowQuestDetail(quest);
                }
            };
            
            return card;
        }
        
        private void ShowQuestDetail(QuestBoardQuest quest) {
            _selectedQuest = quest;
            _detailPanel.Visible = true;
            
            _detailTitle.Text = quest.Title;
            _detailDescription.Text = quest.Description;
            _detailType.Text = $"类型: {GetQuestTypeText(quest.QuestType)}";
            _detailDifficulty.Text = $"难度: {GetDifficultyText(quest.Difficulty)}";
            
            if (quest.CurrentProgress > 0) {
                _detailProgress.Text = $"进度: {quest.CurrentProgress}/{quest.RequiredCount} ({(float)quest.CurrentProgress/quest.RequiredCount*100:F1}%)";
            } else {
                _detailProgress.Text = "未开始";
            }
            
            _detailReward.Text = $"奖励: 🪙 {quest.RewardGold} 金币 | ✨ {quest.RewardExp} 经验 | 🏛️ {quest.RewardGuildPoints} 公会点数";
            
            // 更新按钮状态
            _acceptButton.Disabled = quest.IsCompleted;
        }
        
        #region Event Handlers
        
        private void OnRefreshPressed() {
            RefreshQuests();
        }
        
        private void OnClosePressed() {
            Visible = false;
        }
        
        private void OnAcceptPressed() {
            if (_selectedQuest == null) return;
            
            var system = GuildQuestBoardSystem.Instance;
            if (system != null) {
                if (system.AcceptQuest(_selectedQuest.Id, "Player")) {
                    RefreshQuests();
                    _detailPanel.Visible = false;
                }
            }
        }
        
        private void OnAbandonPressed() {
            if (_selectedQuest == null) return;
            
            var system = GuildQuestBoardSystem.Instance;
            if (system != null) {
                if (system.AbandonQuest(_selectedQuest.Id, "Player")) {
                    RefreshQuests();
                    _detailPanel.Visible = false;
                }
            }
        }
        
        private void OnBackPressed() {
            _detailPanel.Visible = false;
            _selectedQuest = null;
        }
        
        private void OnTypeFilterChanged(long index) {
            // 重新刷新列表，应用筛选
            RefreshQuests();
        }
        
        private void OnDifficultyFilterChanged(long index) {
            // 重新刷新列表，应用筛选
            RefreshQuests();
        }
        
        #endregion
        
        #region Helpers
        
        private string GetQuestTypeIcon(QuestType type) {
            return type switch {
                QuestType.Combat => "⚔️",
                QuestType.Gathering => "🌿",
                QuestType.Crafting => "🔨",
                QuestType.Delivery => "📦",
                QuestType.Rescue => "🆘",
                QuestType.Hunt => "🎯",
                QuestType.Boss => "🐉",
                QuestType.Escort => "🛡️",
                QuestType.Exploration => "🗺️",
                QuestType.Timed => "⏰",
                _ => "📋"
            };
        }
        
        private string GetQuestTypeText(QuestType type) {
            return type switch {
                QuestType.Combat => "战斗",
                QuestType.Gathering => "采集",
                QuestType.Crafting => "制作",
                QuestType.Delivery => "送货",
                QuestType.Rescue => "救援",
                QuestType.Hunt => "狩猎",
                QuestType.Boss => "Boss",
                QuestType.Escort => "护送",
                QuestType.Exploration => "探索",
                QuestType.Timed => "时限",
                _ => "其他"
            };
        }
        
        private string GetDifficultyText(Difficulty difficulty) {
            return difficulty switch {
                Difficulty.Easy => "🟢 简单",
                Difficulty.Normal => "🔵 普通",
                Difficulty.Hard => "🟠 困难",
                Difficulty.Epic => "🟣 史诗",
                Difficulty.Legendary => "🟡 传说",
                _ => "⚪ 未知"
            };
        }
        
        #endregion
        
        #region Toggle
        
        public static void Toggle() {
            var ui = GetOrCreate();
            ui.Visible = !ui.Visible;
        }
        
        private static GuildQuestBoardUI GetOrCreate() {
            var root = GetTree().Root;
            
            // 查找现有实例
            foreach (var child in root.GetChildren()) {
                if (child is GuildQuestBoardUI ui) {
                    return ui;
                }
            }
            
            // 创建新实例
            var scene = GD.Load<PackedScene>("res://Scenes/UI/GuildQuestBoardUI.tscn");
            if (scene != null) {
                return scene.Instantiate<GuildQuestBoardUI>();
            }
            
            // 如果场景不存在，创建代码生成的UI
            var newUi = new GuildQuestBoardUI();
            root.AddChild(newUi);
            return newUi;
        }
        
        #endregion
    }
}
