using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Elite Monster UI - Display elite monster information and statistics
    /// </summary>
    public partial class EliteMonsterUI : Control
    {
        private static EliteMonsterUI _instance;
        public static EliteMonsterUI Instance => _instance;
        
        // UI Elements
        private Label _titleLabel;
        private Label _activeCountLabel;
        private VBoxContainer _eliteListContainer;
        private VBoxContainer _statisticsContainer;
        private TabContainer _tabContainer;
        
        // Visibility
        private bool _isVisible = false;
        
        public override void _Ready()
        {
            _instance = this;
            SetupUI();
            Visible = false;
        }
        
        private void SetupUI()
        {
            // Main panel
            var panel = new PanelContainer
            {
                Name = "MainPanel",
                AnchorsPreset = LayoutPreset.Center,
                OffsetLeft = -400,
                OffsetRight = 400,
                OffsetTop = -300,
                OffsetBottom = 300
            };
            AddChild(panel);
            
            var vbox = new VBoxContainer
            {
                OffsetLeft = 10,
                OffsetRight = -10,
                OffsetTop = 10,
                OffsetBottom = -10
            };
            panel.AddChild(vbox);
            
            // Title
            _titleLabel = new Label
            {
                Text = "🏆 Elite Monster System",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            vbox.AddChild(_titleLabel);
            
            // Tab container
            _tabContainer = new TabContainer
            {
                SizeFlagsVertical = SizeFlags.ExpandFill
            };
            vbox.AddChild(_tabContainer);
            
            // Active Elite tab
            var activeTab = new VBoxContainer { Name = "ActiveElite" };
            _tabContainer.AddChild(activeTab);
            
            var activeTitle = new Label { Text = "Active Elite Monsters" };
            activeTitle.AddThemeFontSizeOverride("font_size", 18);
            activeTab.AddChild(activeTitle);
            
            _activeCountLabel = new Label { Text = "Current: 0" };
            activeTab.AddChild(_activeCountLabel);
            
            _eliteListContainer = new VBoxContainer { Name = "EliteList" };
            activeTab.AddChild(_eliteListContainer);
            
            // Statistics tab
            _statisticsContainer = new VBoxContainer { Name = "Statistics" };
            _tabContainer.AddChild(_statisticsContainer);
            
            var statsTitle = new Label { Text = "Statistics" };
            statsTitle.AddThemeFontSizeOverride("font_size", 18);
            _statisticsContainer.AddChild(statsTitle);
            
            // Spawn Info tab
            var spawnTab = new VBoxContainer { Name = "SpawnInfo" };
            _tabContainer.AddChild(spawnTab);
            
            var spawnTitle = new Label { Text = "Spawn Information" };
            spawnTitle.AddThemeFontSizeOverride("font_size", 18);
            spawnTab.AddChild(spawnTitle);
            
            var baseChanceLabel = new Label { Text = "Base Spawn Chance: 8%" };
            spawnTab.AddChild(baseChanceLabel);
            
            var floorBonusLabel = new Label { Text = "Floor Bonus: +2% (Floor 5+), +3% (Floor 10+), +5% (Floor 20+)" };
            spawnTab.AddChild(floorBonusLabel);
            
            var comboBonusLabel = new Label { Text = "Combo Bonus: +2% (Combo 5+), +5% (Combo 10+)" };
            spawnTab.AddChild(comboBonusLabel);
            
            var timeBonusLabel = new Label { Text = "Time Bonus: +3% (10+ minutes)" };
            spawnTab.AddChild(timeBonusLabel);
            
            // Close button
            var closeButton = new Button
            {
                Text = "Close (ESC)",
                CustomMinimumSize = new Vector2(0, 40)
            };
            closeButton.Pressed += () => ToggleVisibility();
            vbox.AddChild(closeButton);
            
            // Test spawn button
            var testButton = new Button
            {
                Text = "Test Spawn Elite",
                CustomMinimumSize = new Vector2(0, 40)
            };
            testButton.Pressed += OnTestSpawnPressed;
            vbox.AddChild(testButton);
        }
        
        public override void _Process(double delta)
        {
            if (!_isVisible) return;
            
            UpdateActiveEliteList();
            UpdateStatistics();
        }
        
        private void UpdateActiveEliteList()
        {
            // Clear existing
            foreach (var child in _eliteListContainer.GetChildren())
            {
                child.QueueFree();
            }
            
            // Get system instance
            var system = EliteMonsterSystem.Instance;
            if (system == null) return;
            
            // Update count
            int count = 0;
            foreach (var kvp in system.GetStatistics())
            {
                if (kvp.Key == "TotalEliteSpawns")
                {
                    count = kvp.Value;
                    break;
                }
            }
            _activeCountLabel.Text = $"Total Spawned: {count}";
            
            // Note: Full implementation would iterate over active elite monsters
            // For now, show placeholder
            var placeholder = new Label { Text = "Elite monsters appear in combat encounters" };
            _eliteListContainer.AddChild(placeholder);
        }
        
        private void UpdateStatistics()
        {
            // Clear existing
            foreach (var child in _statisticsContainer.GetChildren())
            {
                if (child is Label) child.QueueFree();
            }
            
            var system = EliteMonsterSystem.Instance;
            if (system == null) return;
            
            var stats = system.GetStatistics();
            
            // Title
            var title = new Label { Text = "All-Time Statistics" };
            title.AddThemeFontSizeOverride("font_size", 16);
            _statisticsContainer.AddChild(title);
            
            // Add stat labels
            AddStatLabel($"Total Elite Spawns: {stats.GetValueOrDefault("TotalEliteSpawns", 0)}");
            AddStatLabel($"Elite Monsters Defeated: {stats.GetValueOrDefault("EliteMonstersDefeated", 0)}");
            AddStatLabel("");
            
            // Elite types
            AddStatLabel("Elite Types:");
            AddStatLabel($"  Champions: {stats.GetValueOrDefault("Champions", 0)}");
            AddStatLabel($"  Bosses: {stats.GetValueOrDefault("Bosses", 0)}");
            AddStatLabel($"  Rogues: {stats.GetValueOrDefault("Rogues", 0)}");
            AddStatLabel($"  Tanks: {stats.GetValueOrDefault("Tanks", 0)}");
            AddStatLabel($"  Mages: {stats.GetValueOrDefault("Mages", 0)}");
            AddStatLabel($"  Assassins: {stats.GetValueOrDefault("Assassins", 0)}");
            AddStatLabel($"  Healers: {stats.GetValueOrDefault("Healers", 0)}");
            AddStatLabel($"  Brutes: {stats.GetValueOrDefault("Brutes", 0)}");
            AddStatLabel($"  Swifts: {stats.GetValueOrDefault("Swifts", 0)}");
            AddStatLabel($"  Ancients: {stats.GetValueOrDefault("Ancients", 0)}");
            AddStatLabel("");
            
            // Tier breakdown
            AddStatLabel("Tier Breakdown:");
            AddStatLabel($"  Normal: {stats.GetValueOrDefault("NormalTier", 0)}");
            AddStatLabel($"  Rare: {stats.GetValueOrDefault("RareTier", 0)}");
            AddStatLabel($"  Epic: {stats.GetValueOrDefault("EpicTier", 0)}");
            AddStatLabel($"  Legendary: {stats.GetValueOrDefault("LegendaryTier", 0)}");
            AddStatLabel("");
            
            // Bonus stats
            AddStatLabel($"Total Gold Bonus: {stats.GetValueOrDefault("TotalGoldBonus", 0)}");
            AddStatLabel($"Total Exp Bonus: {stats.GetValueOrDefault("TotalExpBonus", 0)}");
        }
        
        private void AddStatLabel(string text)
        {
            var label = new Label { Text = text };
            _statisticsContainer.AddChild(label);
        }
        
        private void OnTestSpawnPressed()
        {
            var system = EliteMonsterSystem.Instance;
            if (system != null)
            {
                // Force spawn with 100% chance
                system.SetSpawnChanceOverride(1.0f);
                GD.Print("[EliteMonsterUI] Test spawn enabled - next monster check will spawn elite");
                
                // Reset after a moment
                CallDeferred(() => system.SetSpawnChanceOverride(null));
            }
        }
        
        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                UpdateActiveEliteList();
                UpdateStatistics();
            }
        }
        
        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                if (_isVisible)
                {
                    ToggleVisibility();
                    GetViewport().SetInputAsHandled();
                }
            }
        }
    }
}
