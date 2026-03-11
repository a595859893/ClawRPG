using Godot;
using System;
using System.Collections.Generic;
using ClawrRPG.Scripts.Systems;
using ClawrRPG.Scripts.UI;

namespace ClawrRPG.Scripts.UI {
    /// <summary>
    /// 公告板系统 - 整合显示世界事件、每日挑战和成就进度的综合面板
    /// 应用数据驱动设计模式：从 insights.md 学习
    /// </summary>
    public partial class BulletinBoardUI : Control
    {
        // 节点引用
        private PanelContainer mainPanel;
        private VBoxContainer contentBox;
        private TabContainer tabContainer;
        
        // 世界事件标签
        private VBoxContainer worldEventTab;
        private Label currentEventLabel;
        private ProgressBar eventProgressBar;
        private Label eventTimerLabel;
        private Label eventMultiplierLabel;
        
        // 每日挑战标签
        private VBoxContainer dailyChallengeTab;
        private VBoxContainer challengeList;
        private Label challengeRefreshLabel;
        
        // 成就进度标签
        private VBoxContainer achievementTab;
        private VBoxContainer achievementProgressList;
        private Label totalAchievementsLabel;
        
        // 信号系统
        public static event Action OnBulletinBoardOpened;
        public static event Action OnBulletinBoardClosed;
        
        // 数据驱动：世界事件数据
        private string currentEventName = "无";
        private float eventTimeRemaining = 0;
        private float eventProgress = 0;
        private float xpMultiplier = 1.0f;
        private float dropMultiplier = 1.0f;
        private float goldMultiplier = 1.0f;
        
        // 数据驱动：每日挑战数据
        private List<ChallengeDisplayData> activeChallenges = new List<ChallengeDisplayData>();
        
        // 数据驱动：成就进度数据
        private List<AchievementProgressData> achievementProgress = new List<AchievementProgressData>();
        
        // 单例模式
        public static BulletinBoardUI Instance { get; private set; }
        
        public override void _Ready()
        {
            Instance = this;
            SetupUI();
            ConnectSignals();
        }
        
        private void SetupUI()
        {
            // 主面板
            mainPanel = new PanelContainer();
            mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainPanel.CustomMinimumSize = new Vector2(600, 500);
            AddChild(mainPanel);
            
            // 样式
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            mainPanel.AddThemeStyleboxOverride("panel", style);
            
            // 内容容器
            contentBox = new VBoxContainer();
            contentBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            contentBox.AddThemeConstantOverride("separation", 10);
            mainPanel.AddChild(contentBox);
            
            // 标题栏
            var titleBar = new HBoxContainer();
            titleBar.AddThemeConstantOverride("separation", 10);
            contentBox.AddChild(titleBar);
            
            var titleLabel = new Label();
            titleLabel.Text = " 📋 公告板 ";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
            titleBar.AddChild(titleLabel);
            
            titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            var closeBtn = new Button();
            closeBtn.Text = "✕";
            closeBtn.TooltipText = "关闭 (B键)";
            closeBtn.Pressed += () => HideBulletinBoard();
            titleBar.AddChild(closeBtn);
            
            // 标签容器
            tabContainer = new TabContainer();
            tabContainer.SetSizeFlagsVertical(Control.SizeFlags.Expand);
            contentBox.AddChild(tabContainer);
            
            // 创建标签页
            SetupWorldEventTab();
            SetupDailyChallengeTab();
            SetupAchievementTab();
            
            // 初始隐藏
            HideBulletinBoard();
        }
        
        private void SetupWorldEventTab()
        {
            worldEventTab = new VBoxContainer();
            worldEventTab.AddThemeConstantOverride("separation", 15);
            tabContainer.AddChild(worldEventTab);
            tabContainer.SetTabTitle(0, "🌍 世界事件");
            
            // 当前事件标题
            var eventTitle = new Label();
            eventTitle.Text = "当前活动事件";
            eventTitle.AddThemeFontSizeOverride("font_size", 18);
            eventTitle.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
            worldEventTab.AddChild(eventTitle);
            
            // 当前事件名称
            currentEventLabel = new Label();
            currentEventLabel.Text = currentEventName;
            currentEventLabel.AddThemeFontSizeOverride("font_size", 22);
            currentEventLabel.AddThemeColorOverride("font_color", new Color(1f, 0.7f, 0.3f));
            worldEventTab.AddChild(currentEventLabel);
            
            // 事件进度条
            eventProgressBar = new ProgressBar();
            eventProgressBar.CustomMinimumSize = new Vector2(0, 30);
            eventProgressBar.Value = 0;
            eventProgressBar.MaxValue = 100;
            
            var progressStyle = new StyleBoxFlat();
            progressStyle.BgColor = new Color(0.2f, 0.2f, 0.3f);
            progressStyle.SetCornerRadiusAll(4);
            eventProgressBar.AddThemeStyleboxOverride("background", progressStyle);
            
            var fillStyle = new StyleBoxFlat();
            fillStyle.BgColor = new Color(0.3f, 0.6f, 1f);
            fillStyle.SetCornerRadiusAll(4);
            eventProgressBar.AddThemeStyleboxOverride("fill", fillStyle);
            
            worldEventTab.AddChild(eventProgressBar);
            
            // 事件计时器
            eventTimerLabel = new Label();
            eventTimerLabel.Text = "剩余时间: --:--";
            eventTimerLabel.AddThemeFontSizeOverride("font_size", 16);
            worldEventTab.AddChild(eventTimerLabel);
            
            // 倍率信息
            var multiplierTitle = new Label();
            multiplierTitle.Text = "活动倍率加成";
            multiplierTitle.AddThemeFontSizeOverride("font_size", 16);
            multiplierTitle.AddThemeColorOverride("font_color", new Color(0.7f, 0.9f, 0.7f));
            worldEventTab.AddChild(multiplierTitle);
            
            eventMultiplierLabel = new Label();
            eventMultiplierLabel.Text = "经验: x1.0 | 掉落: x1.0 | 金币: x1.0";
            eventMultiplierLabel.AddThemeFontSizeOverride("font_size", 14);
            worldEventTab.AddChild(eventMultiplierLabel);
            
            // 提示
            var tipLabel = new Label();
            tipLabel.Text = "按 E 键查看详细世界事件";
            tipLabel.AddThemeFontSizeOverride("font_size", 12);
            tipLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
            worldEventTab.AddChild(tipLabel);
        }
        
        private void SetupDailyChallengeTab()
        {
            dailyChallengeTab = new VBoxContainer();
            dailyChallengeTab.AddThemeConstantOverride("separation", 10);
            tabContainer.AddChild(dailyChallengeTab);
            tabContainer.SetTabTitle(1, "📅 每日挑战");
            
            // 刷新时间
            challengeRefreshLabel = new Label();
            challengeRefreshLabel.Text = "刷新时间: 剩余 --:--";
            challengeRefreshLabel.AddThemeFontSizeOverride("font_size", 14);
            challengeRefreshLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
            dailyChallengeTab.AddChild(challengeRefreshLabel);
            
            // 挑战列表
            var listTitle = new Label();
            listTitle.Text = "当前挑战";
            listTitle.AddThemeFontSizeOverride("font_size", 16);
            dailyChallengeTab.AddChild(listTitle);
            
            challengeList = new VBoxContainer();
            challengeList.AddThemeConstantOverride("separation", 8);
            dailyChallengeTab.AddChild(challengeList);
        }
        
        private void SetupAchievementTab()
        {
            achievementTab = new VBoxContainer();
            achievementTab.AddThemeConstantOverride("separation", 10);
            tabContainer.AddChild(achievementTab);
            tabContainer.SetTabTitle(2, "🏆 成就进度");
            
            // 总成就统计
            totalAchievementsLabel = new Label();
            totalAchievementsLabel.Text = "成就进度: 0 / 0";
            totalAchievementsLabel.AddThemeFontSizeOverride("font_size", 16);
            totalAchievementsLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
            achievementTab.AddChild(totalAchievementsLabel);
            
            // 成就进度列表
            achievementProgressList = new VBoxContainer();
            achievementProgressList.AddThemeConstantOverride("separation", 5);
            achievementTab.AddChild(achievementProgressList);
        }
        
        private void ConnectSignals()
        {
            // 连接世界事件系统信号
            if (WorldEventSystem.Instance != null)
            {
                WorldEventSystem.Instance.EventStarted += OnEventStarted;
                WorldEventSystem.Instance.EventEnded += OnEventEnded;
                WorldEventSystem.Instance.EventUpdated += OnEventUpdated;
            }
            
            // 连接每日挑战系统信号
            if (DailyChallengeManager.Instance != null)
            {
                DailyChallengeManager.Instance.ChallengeCompleted += OnChallengeCompleted;
                DailyChallengeManager.Instance.ChallengeUpdated += OnChallengeUpdated;
            }
            
            // 连接成就系统信号
            if (AchievementManager.Instance != null)
            {
                AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
                AchievementManager.Instance.OnAchievementProgressUpdated += OnAchievementProgressUpdated;
            }
        }
        
        public override void _Process(double delta)
        {
            // 更新事件计时器
            if (eventTimeRemaining > 0)
            {
                eventTimeRemaining -= (float)delta;
                UpdateEventTimerDisplay();
            }
        }
        
        public override void _Input(InputEvent e)
        {
            if (e.IsActionPressed("bulletin_board"))
            {
                ToggleBulletinBoard();
            }
        }
        
        public void ToggleBulletinBoard()
        {
            if (Visible)
            {
                HideBulletinBoard();
            }
            else
            {
                ShowBulletinBoard();
            }
        }
        
        public void ShowBulletinBoard()
        {
            Visible = true;
            RefreshAllData();
            OnBulletinBoardOpened?.Invoke();
        }
        
        public void HideBulletinBoard()
        {
            Visible = false; 
            OnBulletinBoardClosed?.Invoke();
        }
        
        private void RefreshAllData()
        {
            RefreshWorldEventData();
            RefreshDailyChallengeData();
            RefreshAchievementData();
        }
        
        // 世界事件数据更新
        private void RefreshWorldEventData()
        {
            if (WorldEventSystem.Instance == null) return;
            
            var currentEvent = WorldEventSystem.Instance.GetCurrentEvent();
            if (currentEvent != null)
            {
                currentEventName = currentEvent.Name;
                eventTimeRemaining = WorldEventSystem.Instance.GetTimeRemaining();
                xpMultiplier = WorldEventSystem.Instance.GetXPMultiplier();
                dropMultiplier = WorldEventSystem.Instance.GetDropMultiplier();
                goldMultiplier = WorldEventSystem.Instance.GetGoldMultiplier();
                
                currentEventLabel.Text = currentEventName;
                eventMultiplierLabel.Text = $"经验: x{xpMultiplier:F1} | 掉落: x{dropMultiplier:F1} | 金币: x{goldMultiplier:F1}";
            }
            else
            {
                currentEventName = "无活动事件";
                currentEventLabel.Text = currentEventName;
                eventMultiplierLabel.Text = "经验: x1.0 | 掉落: x1.0 | 金币: x1.0";
            }
        }
        
        private void UpdateEventTimerDisplay()
        {
            int minutes = (int)(eventTimeRemaining / 60);
            int seconds = (int)(eventTimeRemaining % 60);
            eventTimerLabel.Text = $"剩余时间: {minutes:D2}:{seconds:D2}";
        }
        
        // 每日挑战数据更新
        private void RefreshDailyChallengeData()
        {
            // 清空现有列表
            foreach (var child in challengeList.GetChildren())
            {
                child.QueueFree();
            }
            
            // 添加刷新时间
            if (DailyChallengeManager.Instance != null)
            {
                var timeUntilRefresh = DailyChallengeManager.Instance.GetTimeUntilRefresh();
                int hours = (int)(timeUntilRefresh / 3600);
                int minutes = (int)((timeUntilRefresh % 3600) / 60);
                challengeRefreshLabel.Text = $"刷新时间: {hours:D2}:{minutes:D2}";
            }
            
            // 显示挑战数据（示例）
            var sampleChallenge = CreateChallengeItem("击杀10只哥布林", 7, 10, true);
            challengeList.AddChild(sampleChallenge);
            
            var sampleChallenge2 = CreateChallengeItem("收集5个素材", 3, 5, false);
            challengeList.AddChild(sampleChallenge2);
            
            var sampleChallenge3 = CreateChallengeItem("造成1000点伤害", 850, 1000, false);
            challengeList.AddChild(sampleChallenge3);
        }
        
        private Control CreateChallengeItem(string name, int current, int target, bool completed)
        {
            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 10);
            
            // 状态图标
            var statusIcon = new Label();
            statusIcon.Text = completed ? "✅" : "⬜";
            statusIcon.AddThemeFontSizeOverride("font_size", 16);
            container.AddChild(statusIcon);
            
            // 挑战名称
            var nameLabel = new Label();
            nameLabel.Text = name;
            nameLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            container.AddChild(nameLabel);
            
            // 进度
            var progressLabel = new Label();
            progressLabel.Text = $"{current} / {target}";
            progressLabel.AddThemeFontSizeOverride("font_size", 14);
            progressLabel.AddThemeColorOverride("font_color", completed ? new Color(0.5f, 1f, 0.5f) : new Color(1f, 1f, 0.7f));
            container.AddChild(progressLabel);
            
            return container;
        }
        
        // 成就数据更新
        private void RefreshAchievementData()
        {
            // 清空现有列表
            foreach (var child in achievementProgressList.GetChildren())
            {
                child.QueueFree();
            }
            
            // 显示示例成就进度（示例）
            var sampleAchievements = new[]
            {
                ("初出茅庐 (击杀10只怪物)", 10, 10, true),
                ("战士 (击杀100只怪物)", 67, 100, false),
                ("金币大亨 (拥有10000金币)", 5000, 10000, false),
                ("探索者 (发现3个区域)", 2, 3, false)
            };
            
            int unlocked = 0;
            int total = sampleAchievements.Length;
            
            foreach (var (name, current, target, completed) in sampleAchievements)
            {
                if (completed) unlocked++;
                
                var item = CreateAchievementItem(name, current, target, completed);
                achievementProgressList.AddChild(item);
            }
            
            totalAchievementsLabel.Text = $"成就进度: {unlocked} / {total}";
        }
        
        private Control CreateAchievementItem(string name, int current, int target, bool completed)
        {
            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 2);
            
            // 名称和状态
            var header = new HBoxContainer();
            
            var statusIcon = new Label();
            statusIcon.Text = completed ? "🏅" : "🔒";
            statusIcon.AddThemeFontSizeOverride("font_size", 14);
            header.AddChild(statusIcon);
            
            var nameLabel = new Label();
            nameLabel.Text = name;
            nameLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            nameLabel.AddThemeFontSizeOverride("font_size", 13);
            if (completed)
            {
                nameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
            }
            header.AddChild(nameLabel);
            
            container.AddChild(header);
            
            // 进度条
            var progressBar = new ProgressBar();
            progressBar.CustomMinimumSize = new Vector2(0, 8);
            progressBar.Value = current;
            progressBar.MaxValue = target;
            
            var bgStyle = new StyleBoxFlat();
            bgStyle.BgColor = new Color(0.2f, 0.2f, 0.25f);
            bgStyle.SetCornerRadiusAll(3);
            progressBar.AddThemeStyleboxOverride("background", bgStyle);
            
            var fillStyle = new StyleBoxFlat();
            fillStyle.BgColor = completed ? new Color(1f, 0.8f, 0.2f) : new Color(0.3f, 0.5f, 0.8f);
            fillStyle.SetCornerRadiusAll(3);
            progressBar.AddThemeStyleboxOverride("fill", fillStyle);
            
            container.AddChild(progressBar);
            
            return container;
        }
        
        // 信号处理
        private void OnEventStarted(string eventName)
        {
            RefreshWorldEventData();
        }
        
        private void OnEventEnded()
        {
            RefreshWorldEventData();
        }
        
        private void OnEventUpdated()
        {
            RefreshWorldEventData();
        }
        
        private void OnChallengeCompleted(string challengeName)
        {
            RefreshDailyChallengeData();
        }
        
        private void OnChallengeUpdated()
        {
            RefreshDailyChallengeData();
        }
        
        private void OnAchievementUnlocked(string achievementName)
        {
            RefreshAchievementData();
        }
        
        private void OnAchievementProgressUpdated()
        {
            RefreshAchievementData();
        }
        
        // 数据类
        private class ChallengeDisplayData
        {
            public string Name;
            public int CurrentProgress;
            public int TargetProgress;
            public bool IsCompleted;
            public int RewardGold;
            public int RewardXP;
        }
        
        private class AchievementProgressData
        {
            public string Name;
            public int CurrentProgress;
            public int TargetProgress;
            public bool IsUnlocked;
            public int RewardGold;
            public int RewardXP;
        }
    }
}
