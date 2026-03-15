using Godot;
using System;
using System.Collections.Generic;

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

            _petSelector = new ComboBox()
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
            var roleHBox = new HBoxContainer()
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
            }
        }
    }
}
