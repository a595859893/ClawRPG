using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
using ClawRPG.Scripts.Systems.Pets;
using ClawRPG.Scripts.Systems.Pets.AI;
using ClawRPG.Scripts.Systems.PetMimicry;
using ClawRPG.Systems.PetFormation;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Pet Combat Companion UI - displays pet combat coordination stats
    /// Split into partial classes:
    ///   - PetCombatCompanionUI.cs       : Class declaration, fields, SetupUI
    ///   - PetCombatCompanionUI.Tabs.cs  : SetupXxxTab() methods
    ///   - PetCombatCompanionUI.Handlers.cs : Event handlers and refresh methods
    /// </summary>
    public partial class PetCombatCompanionUI : Control
    {
        // ── Core References ────────────────────────────────────────────────
        private PetCombatCompanionSystem _companionSystem;

        // ── Main UI Controls ─────────────────────────────────────────────
        private TabContainer _tabContainer;
        private ComboBox _petSelector;
        private Label _syncLabel;
        private ProgressBar _syncBar;
        private Label _comboLabel;
        private Label _roleLabel;
        private Label _statsLabel;
        private Label _learningLabel;

        // ── Formation UI (REQ-176-04) ────────────────────────────────────
        private CheckButton _btnFormation;
        private PetFormationUI _formationUI;

        // REQ-178: Social Memory Panel
        private PetSocialMemoryPanel _socialMemoryPanel;

        // REQ-179: Combo Fatigue Panel
        private ComboFatiguePanel _fatiguePanel;

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

        // ════════════════════════════════════════════════════════════════
        // Lifecycle
        // ════════════════════════════════════════════════════════════════

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

        // ════════════════════════════════════════════════════════════════
        // UI Setup
        // ════════════════════════════════════════════════════════════════

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

            // Overview tab (inline - simple static content)
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

            // REQ-179: Combo Fatigue Panel - show fatigue stars in overview
            _fatiguePanel = new ComboFatiguePanel
            {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            overviewTab.AddChild(_fatiguePanel);

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

            // Statistics tab (inline - simple static content)
            var statsTab = new ScrollContainer { Name = "Statistics" };
            _tabContainer.AddChild(statsTab);

            _statsLabel = new Label
            {
                Text = "统计数据",
                VerticalAlignment = VerticalAlignment.Top
            };
            _statsLabel.Position = new Vector2(10, 10);
            statsTab.AddChild(_statsLabel);

            // Learning tab (inline - simple static content)
            var learningTab = new ScrollContainer { Name = "Learning" };
            _tabContainer.AddChild(learningTab);

            _learningLabel = new Label
            {
                Text = "学习数据",
                VerticalAlignment = VerticalAlignment.Top
            };
            _learningLabel.Position = new Vector2(10, 10);
            learningTab.AddChild(_learningLabel);

            // Complex tabs are split into partial class
            SetupTacticalTab();
            SetupDecisionTab();
            SetupPersonalityTab();
            SetupObserverTab();
            SetupPerformanceTab();
            // REQ-178: Social Memory tab
            SetupSocialMemoryTab();

            // ── REQ-176-04: Formation Toggle Button ────────────────────────
            var formationHBox = new HBoxContainer();
            vbox.AddChild(formationHBox);

            _btnFormation = new CheckButton
            {
                Text = "🐾 战术阵型",
                ButtonPressed = false,
                tooltip_text = "显示/隐藏战术阵型设置"
            };
            _btnFormation.Toggled += OnFormationToggled;
            formationHBox.AddChild(_btnFormation);

            // Instantiate PetFormationUI as a floating overlay
            _formationUI = new PetFormationUI
            {
                Name = "PetFormationUI_Overlay"
            };
            // Position it centered at top of screen
            _formationUI.Position = new Vector2I(0, 60);
            _formationUI.AnchorLeft = 0.5f;
            _formationUI.AnchorRight = 0.5f;
            _formationUI.OffsetLeft = -200;
            _formationUI.OffsetRight = 200;
            AddChild(_formationUI);
            // Start hidden
            _formationUI.Modulate = new Color(1f, 1f, 1f, 0f);
            _formationUI.Hide();
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

        // ── REQ-176-04: Formation Toggle ──────────────────────────────────

        private void OnFormationToggled(bool buttonPressed)
        {
            if (_formationUI == null) return;

            if (buttonPressed)
            {
                _formationUI.ShowFormationUI();
            }
            else
            {
                _formationUI.HideFormationUI();
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
            // REQ-179: Update fatigue panel for selected pet
            if (_fatiguePanel != null)
            {
                _fatiguePanel.SetPetId(_selectedPetId);
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
    }
}
