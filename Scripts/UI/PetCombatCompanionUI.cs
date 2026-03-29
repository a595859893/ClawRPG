using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Pets;
using ClawRPG.Scripts.Systems.Pets.AI;
using ClawRPG.Scripts.Systems.PetMimicry;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Pet Combat Companion UI - displays pet combat coordination stats
    /// </summary>
    public partial class PetCombatCompanionUI : Control
    {
        private PetCombatCompanionSystem _companionSystem;
        
        private TabContainer _tabContainer;
        private ComboBox _petSelector;
        private Label _syncLabel;
        private ProgressBar _syncBar;
        private Label _comboLabel;
        private Label _roleLabel;
        private Label _statsLabel;
        private Label _learningLabel;
        
        // Tactical tab controls
        private Label _tacticalModeLabel;
        private Label _petHealthLabel;
        private ProgressBar _petHealthBar;
        private Label _playerHealthLabel;
        private ProgressBar _playerHealthBar;
        private Label _decisionLogLabel;
        private Button _btnFollow;
        private Button _btnProtect;
        private Button _btnAttack;

        // Synergy tracker controls
        private Label _synergyCounterLabel;
        private PanelContainer _synergyBurstPanel;

        // Decision Review tab controls - REQ-137
        private ScrollContainer _decisionScroll;
        private VBoxContainer _decisionList;
        private Label _decisionEmptyLabel;
        private Button _btnRefreshDecision;

        // Observer tab controls - REQ-138
        private Label _observerGoalLabel;
        private Label _observerTrajectoryLabel;
        private Label _observerWorldLabel;
        private Label _observerConfidenceLabel;
        private VBoxContainer _observerInfoBox;
        private Button _btnToggleObserver;
        private GuardianPetNarrativeModule _narrativeModule;

        // Personality tab controls - REQ-142-06
        private VBoxContainer _personalityTab;
        private Label _personalityCardLabel;
        private Label _personalityTypeLabel;
        private Label _personalityDescLabel;
        private VBoxContainer _imprintListContainer;

        // Performance tab controls - REQ-148
        private VBoxContainer _performanceTab;
        private Label _performanceSummaryLabel;
        private Label _performanceTimeLabel;
        private Label _performanceHpLabel;
        private Label _performanceWinRateLabel;
        private Label _performanceSampleLabel;

        private string _selectedPetId = "";

        public override void _Ready()
        {
            _companionSystem = PetCombatCompanionSystem.Instance;
            
            if (_companionSystem == null)
            {
                GD.PushWarning("PetCombatCompanionSystem not found!");
                return;
            }

            SetupUI();
            ConnectSignals();
            RefreshPetList();
            RefreshTacticalUI();
        }

        private void SetupUI()
        {
            // Main panel
            var mainPanel = new PanelContainer
            {
                Name = "MainPanel",
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 20f,
                OffsetTop = 20f,
                OffsetRight = -20f,
                OffsetBottom = -20f
            };
            AddChild(mainPanel);

            var vbox = new VBoxContainer
            {
                OffsetLeft = 10f,
                OffsetTop = 10f,
                OffsetRight = -10f,
                OffsetBottom = -10f
            };
            mainPanel.AddChild(vbox);

            // Title
            var title = new Label
            {
                Text = "宠物战斗伴随系统",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            title.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(title);

            // Pet selector
            var selectorHBox = new HBoxContainer();
            vbox.AddChild(selectorHBox);

            var selectorLabel = new Label { Text = "选择宠物: " };
            selectorHBox.AddChild(selectorLabel);

            _petSelector = new ComboBox();
            _petSelector.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _petSelector.ItemSelected += OnPetSelected;
            selectorHBox.AddChild(_petSelector);

            // Tab container
            _tabContainer = new TabContainer
            {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            vbox.AddChild(_tabContainer);

            // Overview tab
            var overviewTab = new VBoxContainer { Name = "Overview" };
            _tabContainer.AddChild(overviewTab);
            
            _syncLabel = new Label { Text = "同步率: 50%" };
            _syncLabel.AddThemeFontSizeOverride("font_size", 18);
            overviewTab.AddChild(_syncLabel);

            _syncBar = new ProgressBar
            {
                MinValue = 0,
                MaxValue = 100,
                Value = 50,
                SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
            };
            _syncBar.CustomMinimumSize = new Vector2(0, 30);
            overviewTab.AddChild(_syncBar);

            _comboLabel = new Label { Text = "连击链: 0" };
            _comboLabel.AddThemeFontSizeOverride("font_size", 18);
            overviewTab.AddChild(_comboLabel);

            _roleLabel = new Label { Text = "当前角色: Attacker" };
            _roleLabel.AddThemeFontSizeOverride("font_size", 16);
            overviewTab.AddChild(_roleLabel);

            // Role buttons
            var roleHBox = new HBoxContainer();
            overviewTab.AddChild(roleHBox);

            string[] roles = { "Attacker", "Support", "Tank", "Scout" };
            foreach (var role in roles)
            {
                var btn = new Button { Text = role };
                btn.Pressed += () => OnRoleButtonPressed(role);
                roleHBox.AddChild(btn);
            }

            // Statistics tab
            var statsTab = new ScrollContainer { Name = "Statistics" };
            _tabContainer.AddChild(statsTab);
            
            _statsLabel = new Label
            {
                Text = "统计数据",
                VerticalAlignment = VerticalAlignment.Top
            };
            _statsLabel.Position = new Vector2(10, 10);
            statsTab.AddChild(_statsLabel);

            // Learning tab
            var learningTab = new ScrollContainer { Name = "Learning" };
            _tabContainer.AddChild(learningTab);
            
            _learningLabel = new Label
            {
                Text = "学习数据",
                VerticalAlignment = VerticalAlignment.Top
            };
            _learningLabel.Position = new Vector2(10, 10);
            learningTab.AddChild(_learningLabel);

            // Tactical tab
            SetupTacticalTab();
            SetupDecisionTab();
            SetupPersonalityTab();
            SetupObserverTab();
            SetupPerformanceTab();
        }

        /// <summary>
        /// Setup the Tactical tab - REQ-112-03 & REQ-112-04
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

        private void OnRefreshDecisionPressed()
        {
            RefreshDecisionTab();
        }

        private void OnReplayFinished()
        {
            // Refresh the decision list when replay finishes (battle ends)
            RefreshDecisionTab();
        }

        private void OnDecisionRecorded(PetDecisionRecord record)
        {
            // Optionally update in real-time, but for now just refresh on battle end
        }

        private void RefreshDecisionTab()
        {
            // Clear existing entries
            foreach (var child in _decisionList.GetChildren())
            {
                child.QueueFree();
            }

            var replaySystem = PetReplayTraceSystem.Instance;
            if (replaySystem == null)
            {
                _decisionList.AddChild(new Label { Text = "PetReplayTraceSystem 不可用" });
                return;
            }

            var records = replaySystem.GetCurrentBattleRecords();
            if (records.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "暂无决策记录\n请先进行一场战斗",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                _decisionList.AddChild(emptyLabel);
                return;
            }

            // Add each decision as a card
            foreach (var record in records)
            {
                AddDecisionCard(record);
            }
        }

        private void AddDecisionCard(PetDecisionRecord record)
        {
            var card = new PanelContainer();
            var cardStyle = new StyleBoxFlat
            {
                BgColor = GetDecisionColor(record.Outcome).WithAlpha(0.15f)
            };
            cardStyle.SetBorderWidthAll(1);
            cardStyle.BorderColor = GetDecisionColor(record.Outcome).WithAlpha(0.4f);
            cardStyle.SetCornerRadiusAll(4);
            card.AddThemeStyleboxOverride("panel", cardStyle);
            _decisionList.AddChild(card);

            var cardVBox = new VBoxContainer();
            card.AddChild(cardVBox);

            // Header: tick + type + outcome icon
            var headerHBox = new HBoxContainer();
            cardVBox.AddChild(headerHBox);

            var tickLabel = new Label
            {
                Text = $"[Tick {record.TickId}]",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            tickLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
            tickLabel.AddThemeFontSizeOverride("font_size", 14);
            headerHBox.AddChild(tickLabel);

            var typeLabel = new Label
            {
                Text = GetDecisionTypeLabel(record.Type),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            typeLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
            typeLabel.AddThemeFontSizeOverride("font_size", 13);
            headerHBox.AddChild(typeLabel);

            var outcomeLabel = new Label
            {
                Text = GetOutcomeIcon(record.Outcome),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            outcomeLabel.AddThemeColorOverride("font_color", GetDecisionColor(record.Outcome));
            headerHBox.AddChild(outcomeLabel);

            // State transition info
            if (record.Type == PetDecisionRecord.DecisionType.StateTransition)
            {
                var stateLabel = new Label
                {
                    Text = $"{record.StateBefore} → {record.StateAfter}",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                stateLabel.AddThemeFontSizeOverride("font_size", 13);
                stateLabel.AddThemeColorOverride("font_color", new Color(0.75f, 0.75f, 0.75f));
                cardVBox.AddChild(stateLabel);
            }

            // Target info
            if (!string.IsNullOrEmpty(record.TargetName) && record.TargetName != "null")
            {
                var targetLabel = new Label
                {
                    Text = $"目标: {record.TargetName} ({record.TargetDistance:F0}px)",
                    HorizontalAlignment = HorizontalAlignment.Left
                };
                targetLabel.AddThemeFontSizeOverride("font_size", 13);
                cardVBox.AddChild(targetLabel);
            }

            // Reason
            if (!string.IsNullOrEmpty(record.Reason))
            {
                var reasonLabel = new Label
                {
                    Text = record.Reason,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    AutowrapMode = TextServer.AutowrapMode.WordSmart
                };
                reasonLabel.AddThemeFontSizeOverride("font_size", 12);
                reasonLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.65f, 0.65f));
                cardVBox.AddChild(reasonLabel);
            }
        }

        private static string GetDecisionTypeLabel(PetDecisionRecord.DecisionType type)
        {
            return type switch
            {
                PetDecisionRecord.DecisionType.StateTransition => "🔄 状态切换",
                PetDecisionRecord.DecisionType.TargetSelection => "🎯 目标选择",
                PetDecisionRecord.DecisionType.BehaviorExecution => "⚡ 行为执行",
                _ => "❓ 未知"
            };
        }

        private static string GetOutcomeIcon(PetDecisionRecord.DecisionOutcome outcome)
        {
            return outcome switch
            {
                PetDecisionRecord.DecisionOutcome.Success => "✅",
                PetDecisionRecord.DecisionOutcome.Failure => "❌",
                PetDecisionRecord.DecisionOutcome.Cancelled => "⭕",
                _ => "⚪"
            };
        }

        private static Color GetDecisionColor(PetDecisionRecord.DecisionOutcome outcome)
        {
            return outcome switch
            {
                PetDecisionRecord.DecisionOutcome.Success => new Color(0.3f, 0.9f, 0.3f),
                PetDecisionRecord.DecisionOutcome.Failure => new Color(0.9f, 0.3f, 0.2f),
                PetDecisionRecord.DecisionOutcome.Cancelled => new Color(0.9f, 0.7f, 0.2f),
                _ => new Color(0.6f, 0.6f, 0.6f)
            };
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
                SizeFlagsVertical = Control.SizeFlags.ShinkBegin
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

        private void OnToggleObserverPressed()
        {
            var observerSystem = AdversarialObserverSystem.Instance;
            if (observerSystem == null) return;

            var state = observerSystem.GetObserverState();
            bool newDisabled = !state.IsDisabled;
            observerSystem.SetEnabled(!newDisabled);

            _btnToggleObserver.Text = newDisabled ? "▶️ 开启" : "🛑 关闭";
            _observerConfidenceLabel.Text = newDisabled ? "已关闭" : "观测中";
        }

        private void OnObserverConfidenceChanged(float confidence)
        {
            RefreshObserverTab();
        }

        private void RefreshObserverTab()
        {
            var observerSystem = AdversarialObserverSystem.Instance;
            if (observerSystem == null || _observerWorldLabel == null) return;

            var assessment = observerSystem.GetCurrentAssessment();
            var goalInference = observerSystem.GetCurrentGoalInference();
            var state = observerSystem.GetObserverState();

            // Update world label
            if (_observerWorldLabel != null && _narrativeModule != null)
            {
                _observerWorldLabel.Text = _narrativeModule.DescribeWorldAssessment(assessment);
            }

            // Update goal label
            if (_observerGoalLabel != null && _narrativeModule != null)
            {
                _observerGoalLabel.Text = _narrativeModule.DescribeGoalInference(goalInference);
            }

            // Update confidence
            if (_observerConfidenceLabel != null)
            {
                float conf = state.PersistentState.Confidence;
                string confStr = conf > 0.75f ? "◆◆◆ 高" : (conf > 0.5f ? "◆◆ 中" : (conf > 0.25f ? "◆ 低" : "◇ 迷茫"));
                _observerConfidenceLabel.Text = $"{confStr} ({conf:P0})";
            }
        }

        private void ConnectSignals()
        {
            if (_companionSystem != null)
            {
                _companionSystem.ComboChainChanged += OnComboChainChanged;
                _companionSystem.RoleChanged += OnRoleChanged;
                _companionSystem.SyncLevelChanged += OnSyncLevelChanged;
                _companionSystem.ComboExecuted += OnComboExecuted;
                _companionSystem.LearningUpdated += OnLearningUpdated;
            }
        }

        private void RefreshPetList()
        {
            _petSelector.Clear();
            
            if (_companionSystem != null)
            {
                var stats = _companionSystem.GetStatistics();
                int petCount = stats.ContainsKey("pet_count") ? (int)stats["pet_count"] : 0;
                
                _petSelector.AddItem("全部宠物", 0);
                
                // Add individual pets if any
                // This would need to be populated from actual pet data
            }
        }

        private void OnPetSelected(long index)
        {
            if (index == 0)
            {
                _selectedPetId = "";
            }
            else
            {
                _selectedPetId = _petSelector.GetItemText((int)index);
            }
            RefreshUI();
        }

        private void OnRoleButtonPressed(string role)
        {
            if (!string.IsNullOrEmpty(_selectedPetId) && _companionSystem != null)
            {
                _companionSystem.SetPetRole(_selectedPetId, role);
            }
        }

        /// <summary>
        /// Handle tactical mode button press - REQ-112-03
        /// </summary>
        private void OnTacticalModePressed(PetTacticalAI.PetTacticalMode mode)
        {
            var tacticalAI = PetTacticalAI.Instance;
            if (tacticalAI != null)
            {
                tacticalAI.SetTacticalMode(mode);
                AppendDecisionLog($"[玩家] 切换至 {GetModeName(mode)}");
            }
            else
            {
                GD.PushWarning("[PetCombatCompanionUI] PetTacticalAI.Instance is null");
            }
        }

        /// <summary>
        /// Handle tactical mode changes from PetTacticalAI
        /// </summary>
        private void OnPetTacticalModeChanged(PetTacticalAI.PetTacticalMode oldMode, PetTacticalAI.PetTacticalMode newMode)
        {
            _tacticalModeLabel.Text = $"战术模式: {GetModeName(newMode)}";
            UpdateModeButtonHighlight(newMode);
        }

        /// <summary>
        /// Handle tactical decision events - REQ-112-04 Readable Failure
        /// </summary>
        private void OnTacticalDecision(string reason)
        {
            AppendDecisionLog(reason);
        }

        /// <summary>
        /// Append a line to the decision log
        /// </summary>
        private void AppendDecisionLog(string line)
        {
            if (_decisionLogLabel == null) return;
            
            string existing = _decisionLogLabel.Text;
            if (existing == "等待决策..." || existing == "")
            {
                _decisionLogLabel.Text = line;
            }
            else
            {
                // Keep last 5 lines
                string[] lines = existing.Split('\n');
                if (lines.Length >= 5)
                {
                    var trimmed = new List<string>(lines);
                    while (trimmed.Count >= 5) trimmed.RemoveAt(0);
                    _decisionLogLabel.Text = string.Join("\n", trimmed) + "\n" + line;
                }
                else
                {
                    _decisionLogLabel.Text = existing + "\n" + line;
                }
            }
        }

        /// <summary>
        /// Update tactical UI state
        /// </summary>
        private void RefreshTacticalUI()
        {
            var tacticalAI = PetTacticalAI.Instance;
            if (tacticalAI != null)
            {
                var mode = tacticalAI.GetCurrentMode();
                _tacticalModeLabel.Text = $"战术模式: {GetModeName(mode)}";
                UpdateModeButtonHighlight(mode);
            }
        }

        /// <summary>
        /// Highlight the active mode button
        /// </summary>
        private void UpdateModeButtonHighlight(PetTacticalAI.PetTacticalMode activeMode)
        {
            _btnFollow.ButtonDisabled = activeMode != PetTacticalAI.PetTacticalMode.Follow;
            _btnProtect.ButtonDisabled = activeMode != PetTacticalAI.PetTacticalMode.Protect;
            _btnAttack.ButtonDisabled = activeMode != PetTacticalAI.PetTacticalMode.Attack;
        }

        private string GetModeName(PetTacticalAI.PetTacticalMode mode)
        {
            return mode switch
            {
                PetTacticalAI.PetTacticalMode.Follow => "跟随",
                PetTacticalAI.PetTacticalMode.Protect => "保护",
                PetTacticalAI.PetTacticalMode.Attack => "进攻",
                _ => "未知"
            };
        }

        private void OnComboChainChanged(string petId, int chain)
        {
            _comboLabel.Text = $"连击链: {chain}";
        }

        private void OnRoleChanged(string petId, string role)
        {
            if (petId == _selectedPetId)
            {
                _roleLabel.Text = $"当前角色: {role}";
            }
        }

        private void OnSyncLevelChanged(string petId, float level)
        {
            if (petId == _selectedPetId || string.IsNullOrEmpty(_selectedPetId))
            {
                int percentage = (int)(level * 100);
                _syncLabel.Text = $"同步率: {percentage}%";
                _syncBar.Value = percentage;
            }
        }

        private void OnComboExecuted(string petId, ComboType type, float damage)
        {
            GD.Print($"Combo executed: {type} for {damage} damage");
        }

        private void OnLearningUpdated(string petId, string updateType)
        {
            RefreshLearningTab();
        }

        private void RefreshUI()
        {
            if (_companionSystem == null || string.IsNullOrEmpty(_selectedPetId))
            {
                // Show overall stats
                var stats = _companionSystem.GetStatistics();
                _syncLabel.Text = "同步率: -";
                _syncBar.Value = 0;
                _comboLabel.Text = $"总连击数: {stats["total_combos"]}";
                _roleLabel.Text = "宠物数量: " + stats["pet_count"];
            }
            else
            {
                // Show pet-specific stats
                var stats = _companionSystem.GetPetStatistics(_selectedPetId);
                
                if (stats.ContainsKey("sync_level"))
                {
                    float sync = (float)stats["sync_level"];
                    int percentage = (int)(sync * 100);
                    _syncLabel.Text = $"同步率: {percentage}%";
                    _syncBar.Value = percentage;
                }
                
                if (stats.ContainsKey("current_combo_chain"))
                {
                    _comboLabel.Text = $"连击链: {stats["current_combo_chain"]}";
                }
                
                if (stats.ContainsKey("role"))
                {
                    _roleLabel.Text = $"当前角色: {stats["role"]}";
                }
            }

            RefreshStatsTab();
            RefreshLearningTab();
            RefreshPersonalityTab();
            RefreshPerformanceTab();
        }

        private void RefreshStatsTab()
        {
            if (_companionSystem == null) return;

            string text = "=== 战斗统计 ===\n\n";
            
            var overallStats = _companionSystem.GetStatistics();
            text += $"总连击次数: {overallStats["total_combos"]}\n";
            text += $"总连击伤害: {overallStats["total_combo_damage"]:F1}\n";
            text += $"最高连击链: {overallStats["highest_combo_chain"]}\n";
            text += $"激活宠物数: {overallStats["pet_count"]}\n";

            _statsLabel.Text = text;
        }

        private void RefreshLearningTab()
        {
            if (_companionSystem == null || string.IsNullOrEmpty(_selectedPetId)) return;

            string text = "=== 学习数据 ===\n\n";
            
            var learning = _companionSystem.GetLearningReport(_selectedPetId);
            
            foreach (var kvp in learning)
            {
                text += $"{kvp.Key}: {kvp.Value}\n";
            }

            _learningLabel.Text = text;
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

        private void RefreshPersonalityTab()
        {
            if (_personalityTab == null) return;

            var mimicryData = PetMimicryData.Instance;
            if (mimicryData == null) return;

            // Determine personality card type
            var dominant = mimicryData.GetDominantBehavior();
            string typeName;
            string description;

            if (dominant.HasValue)
            {
                typeName = GetPersonalityTypeName(dominant.Value);
                description = GetPersonalityDescription(dominant.Value);
            }
            else
            {
                typeName = "尚未形成个性";
                description = "宠物正在观察你的行为...";
            }

            _personalityTypeLabel.Text = typeName;
            _personalityDescLabel.Text = description;

            // Rebuild imprint list
            foreach (var child in _imprintListContainer.GetChildren())
            {
                child.QueueFree();
            }

            var ranking = mimicryData.GetBehaviorRanking();
            if (ranking.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "暂无印记记录",
                    Modulate = new Color(0.5f, 0.5f, 0.5f)
                };
                _imprintListContainer.AddChild(emptyLabel);
                return;
            }

            foreach (var (behavior, level) in ranking)
            {
                var row = CreateImprintRow(behavior, level);
                _imprintListContainer.AddChild(row);
            }
        }

        private string GetPersonalityTypeName(PlayerBehaviorType behavior)
        {
            return behavior switch
            {
                PlayerBehaviorType.UseFireSkill => "🔥 火焰使者",
                PlayerBehaviorType.UseIceSkill => "❄️ 冰霜使者",
                PlayerBehaviorType.UseElectricSkill => "⚡ 雷电使者",
                PlayerBehaviorType.UseShadowSkill => "🌙 暗影使者",
                PlayerBehaviorType.UseHolySkill => "✨ 神圣使者",
                PlayerBehaviorType.UseNatureSkill => "🌿 自然使者",
                PlayerBehaviorType.FrequentDodge => "💨 闪避大师",
                PlayerBehaviorType.AggressiveAttack => "⚔️ 战斗狂人",
                PlayerBehaviorType.DefensiveStance => "🛡️ 守护者",
                PlayerBehaviorType.LowHPAggression => "💀 背水一战",
                PlayerBehaviorType.QuickRetreat => "🏃 撤退专家",
                PlayerBehaviorType.FocusElite => "🎯 精英猎手",
                PlayerBehaviorType.AvoidCombat => "🔍 规避战士",
                PlayerBehaviorType.TriggerTrap => "⚙️ 陷阱触发者",
                PlayerBehaviorType.SolvePuzzle => "🧩 解谜专家",
                PlayerBehaviorType.CollectLoot => "💰 收藏家",
                PlayerBehaviorType.UseHealing => "💚 治愈师",
                PlayerBehaviorType.PetSynergy => "🐾 协战伙伴",
                PlayerBehaviorType.SpecialInteraction => "🌟 特殊互动者",
                _ => "❓ 未知性格"
            };
        }

        private string GetPersonalityDescription(PlayerBehaviorType behavior)
        {
            return behavior switch
            {
                PlayerBehaviorType.UseFireSkill => "你的火系法术给宠物留下了灼烧的印象",
                PlayerBehaviorType.UseIceSkill => "你的冰系控制让宠物学会了冰霜护体",
                PlayerBehaviorType.UseElectricSkill => "你的闪电战术被宠物记在心里",
                PlayerBehaviorType.UseShadowSkill => "你的暗系能力让宠物学会了潜行",
                PlayerBehaviorType.UseHolySkill => "你的神圣力量启发了宠物",
                PlayerBehaviorType.UseNatureSkill => "你对自然的亲近感染了宠物",
                PlayerBehaviorType.FrequentDodge => "你灵活的走位是宠物的教材",
                PlayerBehaviorType.AggressiveAttack => "你的激进打法激励了宠物",
                PlayerBehaviorType.DefensiveStance => "宠物从你身上学到了防守",
                PlayerBehaviorType.LowHPAggression => "你在低血量时的勇敢震撼了宠物",
                PlayerBehaviorType.QuickRetreat => "你的战术撤退被宠物效仿",
                PlayerBehaviorType.FocusElite => "你优先击杀精英的策略被宠物观察",
                PlayerBehaviorType.AvoidCombat => "你规避战斗的方式影响了宠物",
                PlayerBehaviorType.TriggerTrap => "你触发陷阱的行为被宠物记住",
                PlayerBehaviorType.SolvePuzzle => "你解谜的能力启发了宠物",
                PlayerBehaviorType.CollectLoot => "你收集战利品的习惯传染给了宠物",
                PlayerBehaviorType.UseHealing => "你的治疗本能被宠物继承",
                PlayerBehaviorType.PetSynergy => "你经常与宠物协同作战，宠物更加信任你",
                PlayerBehaviorType.SpecialInteraction => "你的特殊互动方式被宠物铭记",
                _ => "宠物正在形成独特个性"
            };
        }

        private Control CreateImprintRow(PlayerBehaviorType behavior, int level)
        {
            var hbox = new HBoxContainer();
            hbox.CustomMinimumSize = new Vector2(380, 28);

            var nameLabel = new Label
            {
                Text = GetBehaviorDisplayName(behavior),
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 12);
            hbox.AddChild(nameLabel);

            // Level stars
            var starsLabel = new Label
            {
                Text = new string('★', level) + new string('☆', 5 - level),
                VerticalAlignment = VerticalAlignment.Center,
                Modulate = GetLevelColor(level)
            };
            starsLabel.AddThemeFontSizeOverride("font_size", 12);
            hbox.AddChild(starsLabel);

            return hbox;
        }

        private string GetBehaviorDisplayName(PlayerBehaviorType behavior)
        {
            return behavior switch
            {
                PlayerBehaviorType.UseFireSkill => "🔥 火系",
                PlayerBehaviorType.UseIceSkill => "❄️ 冰系",
                PlayerBehaviorType.UseElectricSkill => "⚡ 电系",
                PlayerBehaviorType.UseShadowSkill => "🌙 暗系",
                PlayerBehaviorType.UseHolySkill => "✨ 神圣",
                PlayerBehaviorType.UseNatureSkill => "🌿 自然",
                PlayerBehaviorType.FrequentDodge => "💨 闪避",
                PlayerBehaviorType.AggressiveAttack => "⚔️ 激进攻击",
                PlayerBehaviorType.DefensiveStance => "🛡️ 防守姿态",
                PlayerBehaviorType.LowHPAggression => "💀 背水一战",
                PlayerBehaviorType.QuickRetreat => "🏃 快速撤退",
                PlayerBehaviorType.FocusElite => "🎯 精英猎手",
                PlayerBehaviorType.AvoidCombat => "🔍 规避战斗",
                PlayerBehaviorType.TriggerTrap => "⚙️ 触发陷阱",
                PlayerBehaviorType.SolvePuzzle => "🧩 解谜",
                PlayerBehaviorType.CollectLoot => "💰 收集战利品",
                PlayerBehaviorType.UseHealing => "💚 治疗",
                PlayerBehaviorType.PetSynergy => "🐾 协战",
                PlayerBehaviorType.SpecialInteraction => "🌟 特殊互动",
                _ => behavior.ToString()
            };
        }

        private Color GetLevelColor(int level)
        {
            return level switch
            {
                0 => new Color(0.4f, 0.4f, 0.4f),
                1 => new Color(0.6f, 0.9f, 0.6f),
                2 => new Color(0.9f, 0.9f, 0.4f),
                3 => new Color(1f, 0.7f, 0.3f),
                4 => new Color(1f, 0.5f, 0.2f),
                5 => new Color(1f, 0.3f, 0.3f),
                _ => Colors.White
            };
        }

        public override void _Process(double delta)
        {
            // Update health bars from PetTacticalAI state
            RefreshHealthDisplay();
        }

        /// <summary>
        /// Refresh health display from PetTacticalAI
        /// </summary>
        private void RefreshHealthDisplay()
        {
            var tacticalAI = PetTacticalAI.Instance;
            if (tacticalAI == null) return;

            // Pet health
            float petHP = tacticalAI.GetPetHealthPercent();
            int petPercent = (int)(petHP * 100);
            _petHealthLabel.Text = $"宠物: {petPercent}%";
            _petHealthBar.Value = petPercent;

            // Player health
            float playerHP = tacticalAI.GetPlayerHealthPercent();
            int playerPercent = (int)(playerHP * 100);
            _playerHealthLabel.Text = $"玩家: {playerPercent}%";
            _playerHealthBar.Value = playerPercent;
        }

        /// <summary>
        /// 更新协同攻击计数器 UI（由 PetSynergyTracker 调用）
        /// </summary>
        public void UpdateSynergyCounter(int count, int threshold, bool active, float remaining) {
            if (_synergyCounterLabel == null) return;

            if (active) {
                _synergyCounterLabel.Text = $"⚡ 协同激活！剩余 {remaining:F0}s (+10%)";
                _synergyCounterLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.1f));
            } else {
                _synergyCounterLabel.Text = $"协同攻击: {count}/{threshold}";
                if (count >= threshold - 1) {
                    _synergyCounterLabel.AddThemeColorOverride("font_color", new Color(1f, 0.5f, 0.2f));
                } else {
                    _synergyCounterLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.3f));
                }
            }
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

        private void RefreshPerformanceTab()
        {
            if (_performanceTab == null || PetPerformanceData.Instance == null)
                return;

            var perfData = PetPerformanceData.Instance;
            var comparison = perfData.GetComparison();

            int petCount = perfData.GetPetAssistedCount();
            int soloCount = perfData.GetSoloCount();

            if (!comparison.HasEnoughData)
            {
                _performanceSummaryLabel.Text = "数据收集中...\n\n使用宠物参与战斗，我会记录通关数据。\n收集足够的样本后，我会展示宠物对我战斗表现的帮助。";
                _performanceSummaryLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
                _performanceTimeLabel.Text = "⏱ 平均时间: 待收集";
                _performanceHpLabel.Text = "❤️ 平均HP损耗: 待收集";
                _performanceWinRateLabel.Text = "🏆 胜率对比: 待收集";
                _performanceSampleLabel.Text = $"样本数: {petCount}宠物 / {soloCount}独战 (需要各5个样本)";
                return;
            }

            // 有足够数据，显示对比
            _performanceSummaryLabel.Modulate = new Color(1f, 0.9f, 0.5f);

            string timeStr = comparison.TimeSavedPerRoom >= 0
                ? $"宠物帮我平均节省 {comparison.TimeSavedPerRoom:F1}秒/房间"
                : $"宠物参战时平均多花 {-comparison.TimeSavedPerRoom:F1}秒/房间";

            string hpStr = comparison.HpSavedPerRoom >= 0
                ? $"宠物帮我平均节省 {comparison.HpSavedPerRoom}HP/房间"
                : $"宠物参战时多损耗 {-comparison.HpSavedPerRoom}HP/房间";

            string winStr = $"宠物模式胜率 {comparison.WinRatePetAssisted:P0} vs 独战胜率 {comparison.WinRateSolo:P0}";

            _performanceSummaryLabel.Text = "=== 宠物价值报告 ===\n\n" +
                $"📊 基于 {petCount}次宠物参战 vs {soloCount}次独战数据";

            _performanceTimeLabel.Text = $"⏱ {timeStr}";
            _performanceTimeLabel.Modulate = comparison.TimeSavedPerRoom >= 0
                ? new Color(0.5f, 1f, 0.5f)
                : new Color(1f, 0.5f, 0.5f);

            _performanceHpLabel.Text = $"❤️ {hpStr}";
            _performanceHpLabel.Modulate = comparison.HpSavedPerRoom >= 0
                ? new Color(0.5f, 1f, 0.5f)
                : new Color(1f, 0.5f, 0.5f);

            _performanceWinRateLabel.Text = $"🏆 {winStr}";
            _performanceSampleLabel.Text = $"样本数: {petCount}宠物 / {soloCount}独战 ✓";
            _performanceSampleLabel.Modulate = new Color(0.5f, 1f, 0.5f);
        }

        /// <summary>
        /// 显示协同增益爆发特效（由 PetSynergyTracker 调用）
        /// </summary>
        public void ShowSynergyBurst() {
            if (_synergyBurstPanel == null) return;

            _synergyBurstPanel.Visible = true;

            // 2秒后自动隐藏
            var timer = new Timer { OneShot = true, WaitTime = 2f };
            timer.Timeout += () => {
                if (_synergyBurstPanel != null) {
                    _synergyBurstPanel.Visible = false;
                }
                timer.QueueFree();
            };
            AddChild(timer);
            timer.Start();

            // 淡出动画
            var tween = CreateTween();
            tween.TweenInterval(1.5f);
            tween.TweenProperty(_synergyBurstPanel, "modulate:a", 0f, 0.5f);
        }

        public override void _Notification(int what)
        {
            if (what == NotificationExitTree)
            {
                if (_companionSystem != null)
                {
                    _companionSystem.ComboChainChanged -= OnComboChainChanged;
                    _companionSystem.RoleChanged -= OnRoleChanged;
                    _companionSystem.SyncLevelChanged -= OnSyncLevelChanged;
                    _companionSystem.ComboExecuted -= OnComboExecuted;
                    _companionSystem.LearningUpdated -= OnLearningUpdated;
                }

                var tacticalAI = PetTacticalAI.Instance;
                if (tacticalAI != null)
                {
                    tacticalAI.OnTacticalModeChanged -= OnPetTacticalModeChanged;
                    tacticalAI.OnTacticalDecision -= OnTacticalDecision;
                }
            }
        }
    }
}
