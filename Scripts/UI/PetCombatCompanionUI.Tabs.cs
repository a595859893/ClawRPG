using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Pets;
using ClawRPG.Scripts.Systems.Pets.AI;
using ClawRPG.Scripts.Systems.PetMimicry;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// PetCombatCompanionUI - Tabs partial class
    /// Contains all SetupXxxTab() methods
    /// </summary>
    public partial class PetCombatCompanionUI
    {
        /// <summary>
        /// Setup the Tactical tab - REQ-112-03 &amp; REQ-112-04
        /// </summary>
        private void SetupTacticalTab()
        {
            var tacticalTab = new VBoxContainer { Name = "Tactical" };
            _tabContainer.AddChild(tacticalTab);

            // Mode display
            _tacticalModeLabel = new Label
            {
                Text = "战术模式: Follow",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _tacticalModeLabel.AddThemeFontSizeOverride("font_size", 20);
            tacticalTab.AddChild(_tacticalModeLabel);

            // Mode buttons
            var modeHBox = new HBoxContainer();
            tacticalTab.AddChild(modeHBox);

            _btnFollow = new Button { Text = "跟随" };
            _btnFollow.Pressed += () => OnTacticalModePressed(PetTacticalAI.PetTacticalMode.Follow);
            modeHBox.AddChild(_btnFollow);

            _btnProtect = new Button { Text = "保护" };
            _btnProtect.Pressed += () => OnTacticalModePressed(PetTacticalAI.PetTacticalMode.Protect);
            modeHBox.AddChild(_btnProtect);

            _btnAttack = new Button { Text = "进攻" };
            _btnAttack.Pressed += () => OnTacticalModePressed(PetTacticalAI.PetTacticalMode.Attack);
            modeHBox.AddChild(_btnAttack);

            // Health status section
            var healthTitle = new Label { Text = "--- 生命状态 ---" };
            healthTitle.AddThemeFontSizeOverride("font_size", 14);
            tacticalTab.AddChild(healthTitle);

            // Pet health
            var petHealthHBox = new HBoxContainer();
            tacticalTab.AddChild(petHealthHBox);

            _petHealthLabel = new Label { Text = "宠物: 100%" };
            _petHealthLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
            petHealthHBox.AddChild(_petHealthLabel);

            _petHealthBar = new ProgressBar
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 100,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _petHealthBar.CustomMinimumSize = new Vector2(0, 20);
            petHealthHBox.AddChild(_petHealthBar);

            // Player health
            var playerHealthHBox = new HBoxContainer();
            tacticalTab.AddChild(playerHealthHBox);

            _playerHealthLabel = new Label { Text = "玩家: 100%" };
            _playerHealthLabel.SizeFlagsHorizontal = Control.SizeFlags.ShrinkBegin;
            playerHealthHBox.AddChild(_playerHealthLabel);

            _playerHealthBar = new ProgressBar
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 100,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _playerHealthBar.CustomMinimumSize = new Vector2(0, 20);
            playerHealthHBox.AddChild(_playerHealthBar);

            // Decision log section
            var logTitle = new Label { Text = "--- 决策日志 (Readable Failure) ---" };
            logTitle.AddThemeFontSizeOverride("font_size", 14);
            tacticalTab.AddChild(logTitle);

            var logScroll = new ScrollContainer
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            tacticalTab.AddChild(logScroll);

            _decisionLogLabel = new Label
            {
                Text = "等待决策...",
                VerticalAlignment = VerticalAlignment.Top,
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            _decisionLogLabel.CustomMinimumSize = new Vector2(0, 100);
            logScroll.AddChild(_decisionLogLabel);

            // Synergy tracker section - REQ-132
            var synergyTitle = new Label { Text = "--- 宠物协同 (REQ-132) ---" };
            synergyTitle.AddThemeFontSizeOverride("font_size", 14);
            tacticalTab.AddChild(synergyTitle);

            _synergyCounterLabel = new Label
            {
                Text = "协同攻击: 0/5",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _synergyCounterLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
            tacticalTab.AddChild(_synergyCounterLabel);

            // Synergy burst panel (hidden by default)
            _synergyBurstPanel = new PanelContainer {
                Visible = false,
                ZIndex = 3000
            };
            var burstStyle = new StyleBoxFlat {
                BgColor = new Color(1f, 0.85f, 0.1f, 0.9f),
                BorderColor = new Color(1f, 0.7f, 0.1f)
            };
            burstStyle.SetBorderWidthAll(3);
            burstStyle.SetCornerRadiusAll(8);
            _synergyBurstPanel.AddThemeStyleboxOverride("panel", burstStyle);
            tacticalTab.AddChild(_synergyBurstPanel);

            var burstLabel = new Label {
                Text = "⚡ 宠物协同！+10% 伤害！",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            burstLabel.AddThemeFontSizeOverride("font_size", 18);
            burstLabel.AddThemeColorOverride("font_color", new Color(0.2f, 0.15f, 0f));
            _synergyBurstPanel.AddChild(burstLabel);

            // Connect PetTacticalAI signals if available
            var tacticalAI = PetTacticalAI.Instance;
            if (tacticalAI != null)
            {
                tacticalAI.OnTacticalModeChanged += OnPetTacticalModeChanged;
                tacticalAI.OnTacticalDecision += OnTacticalDecision;
            }
        }

        /// <summary>
        /// Setup the Decision Review tab - REQ-137
        /// Shows a timeline of pet AI decisions from the last battle
        /// </summary>
        private void SetupDecisionTab()
        {
            var decisionTab = new VBoxContainer { Name = "Decision" };
            _tabContainer.AddChild(decisionTab);

            // Header with title and refresh button
            var headerHBox = new HBoxContainer();
            decisionTab.AddChild(headerHBox);

            var decisionTitle = new Label
            {
                Text = "宠物决策回顾",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            decisionTitle.AddThemeFontSizeOverride("font_size", 20);
            headerHBox.AddChild(decisionTitle);

            _btnRefreshDecision = new Button { Text = "🔄 刷新" };
            _btnRefreshDecision.Pressed += OnRefreshDecisionPressed;
            headerHBox.AddChild(_btnRefreshDecision);

            // Stats summary
            var statsHBox = new HBoxContainer();
            decisionTab.AddChild(statsHBox);

            var replaySystem = PetReplayTraceSystem.Instance;
            if (replaySystem != null)
            {
                var (total, success, failure, rate) = replaySystem.GetStatistics();
                var statsText = $"总决策: {total}  |  成功: {success}  |  失败: {failure}  |  成功率: {rate:P0}";
                var statsLabel = new Label { Text = statsText };
                statsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
                statsHBox.AddChild(statsLabel);
            }

            // Decision list (scrollable)
            _decisionScroll = new ScrollContainer
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            decisionTab.AddChild(_decisionScroll);

            _decisionList = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            _decisionScroll.AddChild(_decisionList);

            _decisionEmptyLabel = new Label
            {
                Text = "暂无决策记录\n请先进行一场战斗",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _decisionEmptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            _decisionList.AddChild(_decisionEmptyLabel);

            // Subscribe to PetReplayTraceSystem signals
            if (replaySystem != null)
            {
                replaySystem.OnReplayFinished += OnReplayFinished;
                replaySystem.OnDecisionRecorded += OnDecisionRecorded;
            }
        }

        /// <summary>
        /// Setup the Observer tab - REQ-138
        /// Shows AdversarialObserver's view of the player's strategy
        /// </summary>
        private void SetupObserverTab()
        {
            var observerTab = new VBoxContainer { Name = "Observer" };
            _tabContainer.AddChild(observerTab);

            // Header
            var headerHBox = new HBoxContainer();
            observerTab.AddChild(headerHBox);

            var observerTitle = new Label
            {
                Text = "🧐 战略观察者",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            observerTitle.AddThemeFontSizeOverride("font_size", 20);
            headerHBox.AddChild(observerTitle);

            headerHBox.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });

            _btnToggleObserver = new Button { Text = "🛑 关闭" };
            _btnToggleObserver.Pressed += OnToggleObserverPressed;
            headerHBox.AddChild(_btnToggleObserver);

            // Observer enable status
            var observerSystem = AdversarialObserverSystem.Instance;
            bool isEnabled = observerSystem == null || !observerSystem.GetObserverState().IsDisabled;

            // Confidence indicator
            var confidenceHBox = new HBoxContainer();
            observerTab.AddChild(confidenceHBox);

            var confidenceTitle = new Label { Text = "置信度: " };
            confidenceHBox.AddChild(confidenceTitle);

            _observerConfidenceLabel = new Label
            {
                Text = isEnabled ? "观测中" : "已关闭",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            _observerConfidenceLabel.AddThemeFontSizeOverride("font_size", 16);
            confidenceHBox.AddChild(_observerConfidenceLabel);

            // Separator
            var sep1 = new HSeparator();
            observerTab.AddChild(sep1);

            // World Assessment section
            var worldTitle = new Label
            {
                Text = "【我的视野】",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            worldTitle.AddThemeColorOverride("font_color", new Color(0.7f, 0.85f, 1f));
            observerTab.AddChild(worldTitle);

            _observerWorldLabel = new Label
            {
                Text = "正在观测...",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsVertical = Control.SizeFlags.ShinkBegin
            };
            _observerWorldLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.7f));
            observerTab.AddChild(_observerWorldLabel);

            // Separator
            var sep2 = new HSeparator();
            observerTab.AddChild(sep2);

            // Player Goal Inference section
            var goalTitle = new Label
            {
                Text = "【你的目标（我猜的）】",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            goalTitle.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.7f));
            observerTab.AddChild(goalTitle);

            _observerGoalLabel = new Label
            {
                Text = "还未看清你的意图...",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsVertical = Control.SizeFlags.ShinkBegin
            };
            _observerGoalLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.8f, 0.7f));
            observerTab.AddChild(_observerGoalLabel);

            // Separator
            var sep3 = new HSeparator();
            observerTab.AddChild(sep3);

            // Trajectory Prediction section
            var trajTitle = new Label
            {
                Text = "【我的预测】",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            trajTitle.AddThemeColorOverride("font_color", new Color(0.7f, 1f, 0.85f));
            observerTab.AddChild(trajTitle);

            _observerTrajectoryLabel = new Label
            {
                Text = "还没有足够的数据...",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsVertical = Control.SizeFlags.ShrinkBegin
            };
            _observerTrajectoryLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.9f, 0.8f));
            observerTab.AddChild(_observerTrajectoryLabel);

            // Initialize narrative module
            _narrativeModule = new GuardianPetNarrativeModule();

            // Subscribe to Observer signals
            if (observerSystem != null)
            {
                observerSystem.OnConfidenceChanged += OnObserverConfidenceChanged;
            }

            // Initial refresh
            RefreshObserverTab();
        }

        // ── Personality Tab (REQ-142-06) ───────────────────────────────────
        private void SetupPersonalityTab()
        {
            _personalityTab = new ScrollContainer { Name = "Personality" };
            _tabContainer.AddChild(_personalityTab);

            var container = new VBoxContainer();
            container.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _personalityTab.AddChild(container);

            // Personality Card Header
            _personalityCardLabel = new Label
            {
                Text = "个性卡",
                VerticalAlignment = VerticalAlignment.Top
            };
            _personalityCardLabel.AddThemeFontSizeOverride("font_size", 18);
            _personalityCardLabel.Position = new Vector2(10, 10);
            container.AddChild(_personalityCardLabel);

            _personalityTypeLabel = new Label
            {
                Text = "尚未形成个性",
                VerticalAlignment = VerticalAlignment.Top
            };
            _personalityTypeLabel.AddThemeFontSizeOverride("font_size", 24);
            _personalityTypeLabel.Modulate = new Color(1f, 0.85f, 0.4f);
            _personalityTypeLabel.Position = new Vector2(10, 40);
            container.AddChild(_personalityTypeLabel);

            _personalityDescLabel = new Label
            {
                Text = "宠物正在观察你的行为...",
                VerticalAlignment = VerticalAlignment.Top,
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            _personalityDescLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            _personalityDescLabel.Position = new Vector2(10, 72);
            _personalityDescLabel.Size = new Vector2(380, 60);
            container.AddChild(_personalityDescLabel);

            var separator = new HSeparator();
            separator.Position = new Vector2(10, 135);
            container.AddChild(separator);

            var imprintHeader = new Label
            {
                Text = "行为印记",
                VerticalAlignment = VerticalAlignment.Top
            };
            imprintHeader.AddThemeFontSizeOverride("font_size", 14);
            imprintHeader.Modulate = new Color(0.8f, 0.8f, 0.8f);
            imprintHeader.Position = new Vector2(10, 145);
            container.AddChild(imprintHeader);

            _imprintListContainer = new VBoxContainer();
            _imprintListContainer.Position = new Vector2(10, 175);
            container.AddChild(_imprintListContainer);
        }

        // ── Performance Tab (REQ-148) ────────────────────────────────────────
        private void SetupPerformanceTab()
        {
            _performanceTab = new VBoxContainer { Name = "Performance" };
            _tabContainer.AddChild(_performanceTab);

            // Header
            var headerHBox = new HBoxContainer();
            _performanceTab.AddChild(headerHBox);

            var perfTitle = new Label
            {
                Text = "🐾 我的价值",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            perfTitle.AddThemeFontSizeOverride("font_size", 20);
            headerHBox.AddChild(perfTitle);

            headerHBox.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });

            var sep1 = new HSeparator();
            _performanceTab.AddChild(sep1);

            // Summary section
            _performanceSummaryLabel = new Label
            {
                Text = "数据收集中...",
                AutowrapMode = TextServer.AutowrapMode.WordSmart,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            _performanceSummaryLabel.AddThemeFontSizeOverride("font_size", 14);
            _performanceTab.AddChild(_performanceSummaryLabel);

            var sep2 = new HSeparator();
            _performanceTab.AddChild(sep2);

            // Time comparison
            _performanceTimeLabel = new Label
            {
                Text = "⏱ 平均时间: -",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            _performanceTimeLabel.AddThemeFontSizeOverride("font_size", 14);
            _performanceTab.AddChild(_performanceTimeLabel);

            // HP comparison
            _performanceHpLabel = new Label
            {
                Text = "❤️ 平均HP损耗: -",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            _performanceHpLabel.AddThemeFontSizeOverride("font_size", 14);
            _performanceTab.AddChild(_performanceHpLabel);

            // Win rate
            _performanceWinRateLabel = new Label
            {
                Text = "🏆 胜率对比: -",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            _performanceWinRateLabel.AddThemeFontSizeOverride("font_size", 14);
            _performanceTab.AddChild(_performanceWinRateLabel);

            var sep3 = new HSeparator();
            _performanceTab.AddChild(sep3);

            // Sample count
            _performanceSampleLabel = new Label
            {
                Text = "样本数: 0 (需要5个样本才能显示对比)",
                AutowrapMode = TextServer.AutowrapMode.WordSmart
            };
            _performanceSampleLabel.AddThemeFontSizeOverride("font_size", 12);
            _performanceSampleLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            _performanceTab.AddChild(_performanceSampleLabel);
        }
    }
}
