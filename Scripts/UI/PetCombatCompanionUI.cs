using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Pets;
using ClawRPG.Scripts.Systems.Pets.AI;

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
            SetupObserverTab();
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

        private void ConnectSignals
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
