using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Event Chain UI - 事件连锁界面
    /// </summary>
    public class EventChainUI : Control {
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

        public override void _Process(float delta) {
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

        public override void _Input(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Scancode == KeyList.L) {
                    ToggleVisibility();
                }
            }
        }
    }
}
