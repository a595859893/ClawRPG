using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Event Chain UI - 事件连锁界面
    /// </summary>
    public partial class EventChainUI : Control {
        private static EventChainUI Instance { get; set; }

        private bool isVisible = false;
        private TabContainer tabContainer;
        private Label titleLabel;
        private VBoxContainer activeChainsContainer;
        private VBoxContainer completedChainsContainer;
        private VBoxContainer statsContainer;

        // 统计显示
        private Label statsStartedLabel;
        private Label statsCompletedLabel;
        private Label statsFailedLabel;
        private Label statsGoldLabel;
        private Label statsExpLabel;

        public override void _Ready() {
            Instance = this;
            SetupUI();
            
            // 订阅 EventChainSystem 信号
            if (EventChainSystem.Instance != null) {
                EventChainSystem.Instance.Connect("ChainCompleted", this, "_OnChainCompleted");
                EventChainSystem.Instance.Connect("ChainStarted", this, "_OnChainStarted");
                EventChainSystem.Instance.Connect("ChainAdvanced", this, "_OnChainAdvanced");
            }
            
            Hide();
        }

        private void SetupUI() {
            // 主容器
            var mainPanel = new PanelContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One,
                OffsetLeft = 100,
                OffsetTop = 50,
                OffsetRight = -100,
                OffsetBottom = -50
            };
            AddChild(mainPanel);

            var mainVBox = new VBoxContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            mainPanel.AddChild(mainVBox);

            // 标题栏
            var titleBar = new HBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            mainVBox.AddChild(titleBar);

            titleLabel = new Label {
                Text = "事件连锁系统",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                Alignment = Alignment.Center
            };
            titleBar.AddChild(titleLabel);

            var closeButton = new Button {
                Text = "X",
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd
            };
            closeButton.Pressed += () => ToggleVisibility();
            titleBar.AddChild(closeButton);

            // TabContainer
            tabContainer = new TabContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            mainVBox.AddChild(tabContainer);

            // 活跃连锁标签页
            var activeTab = new Control {
                Name = "ActiveChains"
            };
            tabContainer.AddChild(activeTab);
            tabContainer.SetTabTitle(0, "当前进行");

            var activeScroll = new ScrollContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One
            };
            activeTab.AddChild(activeScroll);

            activeChainsContainer = new VBoxContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One
            };
            activeScroll.AddChild(activeChainsContainer);

            // 完成连锁标签页
            var completedTab = new Control {
                Name = "CompletedChains"
            };
            tabContainer.AddChild(completedTab);
            tabContainer.SetTabTitle(1, "已完成");

            var completedScroll = new ScrollContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One
            };
            completedTab.AddChild(completedScroll);

            completedChainsContainer = new VBoxContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One
            };
            completedScroll.AddChild(completedChainsContainer);

            // 统计标签页
            var statsTab = new Control {
                Name = "Statistics"
            };
            tabContainer.AddChild(statsTab);
            tabContainer.SetTabTitle(2, "统计");

            var statsVBox = new VBoxContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One,
                OffsetLeft = 20,
                OffsetTop = 20
            };
            statsTab.AddChild(statsVBox);

            statsStartedLabel = new Label { Text = "开始总数: 0" };
            statsVBox.AddChild(statsStartedLabel);

            statsCompletedLabel = new Label { Text = "完成数: 0" };
            statsVBox.AddChild(statsCompletedLabel);

            statsFailedLabel = new Label { Text = "失败数: 0" };
            statsVBox.AddChild(statsFailedLabel);

            var spacer1 = new Control { CustomMinimumSize = new Vector2(0, 20) };
            statsVBox.AddChild(spacer1);

            statsGoldLabel = new Label { Text = "获得金币: 0" };
            statsVBox.AddChild(statsGoldLabel);

            statsExpLabel = new Label { Text = "获得经验: 0" };
            statsVBox.AddChild(statsExpLabel);

            // 添加快捷键说明
            var hintLabel = new Label {
                Text = "按 L 键切换显示",
                Alignment = Alignment.Center,
                CustomMinimumSize = new Vector2(0, 30)
            };
            mainVBox.AddChild(hintLabel);
        }

        public override void _Process(double delta) {
            // 可选：更新UI显示
        }

        public void ToggleVisibility() {
            if (isVisible) {
                Hide();
                isVisible = false;
            } else {
                Show();
                RefreshDisplay();
                isVisible = true;
            }
        }

        private void RefreshDisplay() {
            if (EventChainSystem.Instance == null) return;

            // 更新活跃连锁
            RefreshActiveChains();

            // 更新已完成连锁
            RefreshCompletedChains();

            // 更新统计
            RefreshStats();
        }

        private void RefreshActiveChains() {
            // 清除现有内容
            foreach (var child in activeChainsContainer.GetChildren()) {
                child.QueueFree();
            }

            var activeChains = EventChainSystem.Instance.GetActiveChains();

            if (activeChains.Count == 0) {
                var emptyLabel = new Label {
                    Text = "暂无进行中的事件连锁",
                    Alignment = Alignment.Center
                };
                activeChainsContainer.AddChild(emptyLabel);
                return;
            }

            foreach (var chain in activeChains) {
                var chainData = EventChainSystem.Instance.GetChain(chain.chainId);
                if (chainData == null) continue;

                var chainPanel = CreateChainCard(chainData, chain.progress);
                activeChainsContainer.AddChild(chainPanel);
            }
        }

        private void RefreshCompletedChains() {
            // 清除现有内容
            foreach (var child in completedChainsContainer.GetChildren()) {
                child.QueueFree();
            }

            // 这里可以显示已完成的事件链历史
            // 简化版本显示为空
            var emptyLabel = new Label {
                Text = "已完成连锁将在此显示",
                Alignment = Alignment.Center
            };
            completedChainsContainer.AddChild(emptyLabel);
        }

        private void RefreshStats() {
            var stats = EventChainSystem.Instance.GetStatistics();

            statsStartedLabel.Text = $"开始总数: {stats["total_chains_started"]}";
            statsCompletedLabel.Text = $"完成数: {stats["total_chains_completed"]}";
            statsFailedLabel.Text = $"失败数: {stats["total_chains_failed"]}";
            statsGoldLabel.Text = $"获得金币: {stats["total_gold_earned"]}";
            statsExpLabel.Text = $"获得经验: {stats["total_exp_earned"]}";
        }

        private Control CreateChainCard(EventChainData data, float progress) {
            var panel = new PanelContainer {
                CustomMinimumSize = new Vector2(0, 80),
                MarginLeft = 10,
                MarginRight = 10,
                MarginTop = 5,
                MarginBottom = 5
            };

            var vbox = new VBoxContainer {
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            panel.AddChild(vbox);

            // 名称和类别
            var header = new HBoxContainer();
            vbox.AddChild(header);

            var nameLabel = new Label {
                Text = data.chainName,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            header.AddChild(nameLabel);

            var categoryLabel = new Label {
                Text = GetCategoryIcon(data.category),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd
            };
            header.AddChild(categoryLabel);

            // 描述
            var descLabel = new Label {
                Text = data.description,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            vbox.AddChild(descLabel);

            // 进度条
            var progressBar = new ProgressBar {
                Value = progress * 100,
                MaxValue = 100,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            vbox.AddChild(progressBar);

            return panel;
        }

        private string GetCategoryIcon(EventChainCategory category) {
            switch (category) {
                case EventChainCategory.Adventure: return "⚔️";
                case EventChainCategory.Combat: return "💥";
                case EventChainCategory.Mystery: return "🔮";
                case EventChainCategory.Romance: return "💕";
                case EventChainCategory.Tragedy: return "😢";
                case EventChainCategory.Comedy: return "😂";
                case EventChainCategory.Legend: return "🐉";
                default: return "📜";
            }
        }

        // ========== 链事件通知 ==========
        
        private void _OnChainCompleted(string chainName, int goldBonus, int expBonus) {
            ShowRewardNotification(chainName, goldBonus, expBonus);
        }
        
        private void _OnChainStarted(string chainId, string chainName) {
            ShowChainNotification($"事件链开始: {chainName}", Colors.LightGreen);
        }
        
        private void _OnChainAdvanced(string chainId, string chainName, int currentStage, int totalStages) {
            ShowChainNotification($"{chainName} 推进到第 {currentStage}/{totalStages} 阶段", Colors.LightBlue);
        }
        
        /// <summary>
        /// 显示奖励结算通知（居中大弹窗）
        /// </summary>
        private void ShowRewardNotification(string chainName, int goldBonus, int expBonus) {
            var popup = new PanelContainer {
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -200,
                OffsetTop = -120,
                OffsetRight = 200,
                OffsetBottom = 120
            };
            popup.Modulate = new Color(1, 1, 1, 0);
            AddChild(popup);
            
            var bg = new ColorRect {
                Color = new Color(0.1f, 0.1f, 0.2f, 0.95f),
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One
            };
            popup.AddChild(bg);
            
            var border = new PanelContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One,
                CustomMinimumSize = new Vector2(2, 2)
            };
            var style = new StyleBoxFlat { BgColor = new Color(0.9f, 0.7f, 0.2f, 1f), BorderWidthLeft = 3, BorderWidthTop = 3, BorderWidthRight = 3, BorderWidthBottom = 3 };
            border.AddStyleboxOverride("panel", style);
            popup.AddChild(border);
            
            var vbox = new VBoxContainer {
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One,
                OffsetLeft = 15,
                OffsetTop = 15,
                OffsetRight = -15,
                OffsetBottom = -15
            };
            border.AddChild(vbox);
            
            var title = new Label {
                Text = "🎉 事件链完成!",
                Alignment = Alignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            title.AddThemeColorOverride("font_color", new Color(0.9f, 0.7f, 0.2f));
            vbox.AddChild(title);
            
            var chainLabel = new Label {
                Text = chainName,
                Alignment = Alignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            chainLabel.AddThemeColorOverride("font_color", Colors.White);
            vbox.AddChild(chainLabel);
            
            var spacer = new Control { CustomMinimumSize = new Vector2(0, 10) };
            vbox.AddChild(spacer);
            
            var rewardBox = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            vbox.AddChild(rewardBox);
            
            var goldLabel = new Label {
                Text = $"💰 {goldBonus}",
                Alignment = Alignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            goldLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.2f));
            rewardBox.AddChild(goldLabel);
            
            var expLabel = new Label {
                Text = $"✨ {expBonus}",
                Alignment = Alignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            expLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.8f, 1f));
            rewardBox.AddChild(expLabel);
            
            // 淡入动画
            var tween = CreateTween();
            tween.TweenProperty(popup, "modulate:a", 1f, 0.3f);
            tween.TweenInterval(2.5f);
            tween.TweenProperty(popup, "modulate:a", 0f, 0.3f);
            tween.TweenCallback(popup, "queue_free");
        }
        
        /// <summary>
        /// 显示简短链通知（HUD风格）
        /// </summary>
        private void ShowChainNotification(string message, Color color) {
            var notif = new PanelContainer {
                AnchorLeft = 0.5f,
                AnchorTop = 0.05f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.05f,
                OffsetLeft = -200,
                OffsetTop = 0,
                OffsetRight = 200,
                OffsetBottom = 40
            };
            notif.Modulate = new Color(1, 1, 1, 0);
            AddChild(notif);
            
            var bg = new ColorRect {
                Color = new Color(0.05f, 0.05f, 0.1f, 0.85f),
                AnchorRight = Vector2.One,
                AnchorBottom = Vector2.One
            };
            notif.AddChild(bg);
            
            var border = new PanelContainer { AnchorRight = Vector2.One, AnchorBottom = Vector2.One };
            var borderStyle = new StyleBoxFlat { BgColor = new Color(0, 0, 0, 0), BorderWidthLeft = 2, BorderWidthRight = 2, BorderWidthTop = 2, BorderWidthBottom = 2, BorderColor = color };
            border.AddStyleboxOverride("panel", borderStyle);
            notif.AddChild(border);
            
            var label = new Label {
                Text = message,
                Alignment = Alignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            label.AddThemeColorOverride("font_color", color);
            border.AddChild(label);
            
            var tween = CreateTween();
            tween.TweenProperty(notif, "modulate:a", 1f, 0.2f);
            tween.TweenInterval(2.0f);
            tween.TweenProperty(notif, "modulate:a", 0f, 0.3f);
            tween.TweenCallback(notif, "queue_free");
        }

        public override void _Input(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Scancode == KeyList.L) {
                    ToggleVisibility();
                }
            }
        }
    }
}
