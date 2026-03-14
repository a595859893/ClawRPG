using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.SealedDungeon {
    public class SealedDungeonUI : Control {
        private Control _mainContainer;
        private Control _dungeonListPanel;
        private Control _dungeonDetailPanel;
        private Control _statisticsPanel;
        
        private Label _titleLabel;
        private Label _starsLabel;
        private Label _timerLabel;
        private Label _floorLabel;
        private Label _scoreLabel;
        
        private VBoxContainer _zoneList;
        private VBoxContainer _floorList;
        private VBoxContainer _rewardList;
        
        private Button _startButton;
        private Button _completeFloorButton;
        private Button _claimRewardButton;
        private Button _exitButton;
        
        private int _selectedTab = 0;
        
        public override void _Ready() {
            SetupUI();
            ConnectSignals();
            RefreshUI();
            
            GD.Print("[SealedDungeonUI] Sealed Dungeon UI initialized");
        }

        private void SetupUI() {
            // Main container
            _mainContainer = new Control();
            _mainContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
            AddChild(_mainContainer);
            
            // Background panel
            var bgPanel = new PanelContainer {
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                Modulate = new Color(0, 0, 0, 0.85f)
            };
            _mainContainer.AddChild(bgPanel);
            
            var mainVBox = new VBoxContainer {
                AnchorRight = 1.0f,
                AnchorBottom = 1.0f,
                CustomMinimumSize = new Vector2(0, 600)
            };
            bgPanel.AddChild(mainVBox);
            
            // Header
            var headerHBox = new HBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            mainVBox.AddChild(headerHBox);
            
            _titleLabel = new Label {
                Text = "⚔️ Sealed Dungeon ⚔️",
                FontSize = 28,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            headerHBox.AddChild(_titleLabel);
            
            var headerStats = new HBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd };
            headerHBox.AddChild(headerStats);
            
            _starsLabel = new Label {
                Text = "⭐ 0",
                FontSize = 18
            };
            headerStats.AddChild(_starsLabel);
            
            var spacer = new Control {
                CustomMinimumSize = new Vector2(20, 0),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd
            };
            headerStats.AddChild(spacer);
            
            _timerLabel = new Label {
                Text = "⏱️ 00:00",
                FontSize = 18,
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd
            };
            headerStats.AddChild(_timerLabel);
            
            var closeButton = new Button {
                Text = "✕",
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd,
                CustomMinimumSize = new Vector2(40, 0)
            };
            closeButton.Pressed += () => Hide();
            headerStats.AddChild(closeButton);
            
            // Tab buttons
            var tabContainer = new HBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 50)
            };
            mainVBox.AddChild(tabContainer);
            
            var dungeonsTab = CreateTabButton("Dungeons", 0);
            tabContainer.AddChild(dungeonsTab);
            
            var detailTab = CreateTabButton("Details", 1);
            tabContainer.AddChild(detailTab);
            
            var statsTab = CreateTabButton("Statistics", 2);
            tabContainer.AddChild(statsTab);
            
            // Content area
            var contentHBox = new HBoxContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            mainVBox.AddChild(contentHBox);
            
            // Left panel - Zone/Dungeon list
            _dungeonListPanel = new PanelContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(300, 0)
            };
            contentHBox.AddChild(_dungeonListPanel);
            
            var listScroll = new ScrollContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            _dungeonListPanel.AddChild(listScroll);
            
            _zoneList = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            listScroll.AddChild(_zoneList);
            
            // Middle panel - Detail
            _dungeonDetailPanel = new PanelContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(400, 0)
            };
            contentHBox.AddChild(_dungeonDetailPanel);
            
            var detailScroll = new ScrollContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            _dungeonDetailPanel.AddChild(detailScroll);
            
            var detailVBox = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            detailScroll.AddChild(detailVBox);
            
            _floorLabel = new Label {
                Text = "Select a dungeon to view details",
                FontSize = 20,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            detailVBox.AddChild(_floorLabel);
            
            _scoreLabel = new Label {
                Text = "",
                FontSize = 16
            };
            detailVBox.AddChild(_scoreLabel);
            
            _floorList = new VBoxContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            detailVBox.AddChild(_floorList);
            
            // Right panel - Actions & Rewards
            var rightPanel = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(250, 0)
            };
            contentHBox.AddChild(rightPanel);
            
            var actionPanel = new PanelContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            rightPanel.AddChild(actionPanel);
            
            var actionVBox = new VBoxContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            actionPanel.AddChild(actionVBox);
            
            var actionLabel = new Label {
                Text = "Actions",
                FontSize = 18,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            actionVBox.AddChild(actionLabel);
            
            _startButton = new Button {
                Text = "🚀 Start Dungeon",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 50)
            };
            _startButton.Pressed += OnStartButtonPressed;
            actionVBox.AddChild(_startButton);
            
            _completeFloorButton = new Button {
                Text = "✅ Complete Floor",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 50),
                Disabled = true
            };
            _completeFloorButton.Pressed += OnCompleteFloorButtonPressed;
            actionVBox.AddChild(_completeFloorButton);
            
            _claimRewardButton = new Button {
                Text = "🎁 Claim Reward",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 50),
                Disabled = true
            };
            _claimRewardButton.Pressed += OnClaimRewardButtonPressed;
            actionVBox.AddChild(_claimRewardButton);
            
            // Reward list
            var rewardPanel = new PanelContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            rightPanel.AddChild(rewardPanel);
            
            var rewardVBox = new VBoxContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            rewardPanel.AddChild(rewardVBox);
            
            var rewardLabel = new Label {
                Text = "Recent Rewards",
                FontSize = 18,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rewardVBox.AddChild(rewardLabel);
            
            var rewardScroll = new ScrollContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            rewardVBox.AddChild(rewardScroll);
            
            _rewardList = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            rewardScroll.AddChild(_rewardList);
            
            // Exit button
            _exitButton = new Button {
                Text = "Exit Dungeon",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 50),
                Disabled = true
            };
            _exitButton.Pressed += OnExitButtonPressed;
            rightPanel.AddChild(_exitButton);
            
            // Statistics panel (hidden by default)
            _statisticsPanel = new PanelContainer {
                Visible = false,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            mainVBox.AddChild(_statisticsPanel);
            
            var statsScroll = new ScrollContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            _statisticsPanel.AddChild(statsScroll);
            
            var statsVBox = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            statsScroll.AddChild(statsVBox);
            
            CreateStatisticsContent(statsVBox);
        }

        private Button CreateTabButton(string text, int index) {
            var button = new Button {
                Text = text,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(100, 40),
                ToggleMode = true
            };
            button.Pressed += () => OnTabSelected(index);
            return button;
        }

        private void CreateStatisticsContent(VBoxContainer parent) {
            var stats = SealedDungeonSystem.Instance.PlayerData.Statistics;
            
            AddStatRow(parent, "Total Attempts", stats.TotalAttempts.ToString());
            AddStatRow(parent, "Total Completions", stats.TotalCompletions.ToString());
            AddStatRow(parent, "Total Floors Cleared", stats.TotalFloorsCleared.ToString());
            AddStatRow(parent, "Total Gold Earned", stats.TotalGoldEarned.ToString("N0"));
            AddStatRow(parent, "Total XP Earned", stats.TotalExperienceEarned.ToString("N0"));
            AddStatRow(parent, "Longest Streak", stats.LongestStreak.ToString());
            AddStatRow(parent, "Current Streak", stats.CurrentStreak.ToString());
            AddStatRow(parent, "Best Score", stats.BestScore.ToString("N0"));
        }

        private void AddStatRow(VBoxContainer parent, string label, string value) {
            var hbox = new HBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(0, 35)
            };
            
            var labelControl = new Label {
                Text = label + ":",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            hbox.AddChild(labelControl);
            
            var valueControl = new Label {
                Text = value,
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd,
                FontSize = 16
            };
            hbox.AddChild(valueControl);
            
            parent.AddChild(hbox);
        }

        private void ConnectSignals() {
            if (SealedDungeonSystem.Instance == null) return;
            
            SealedDungeonSystem.Instance.DungeonStarted += OnDungeonStarted;
            SealedDungeonSystem.Instance.DungeonCompleted += OnDungeonCompleted;
            SealedDungeonSystem.Instance.DungeonFailed += OnDungeonFailed;
            SealedDungeonSystem.Instance.FloorCompleted += OnFloorCompleted;
            SealedDungeonSystem.Instance.ZoneUnlocked += OnZoneUnlocked;
            SealedDungeonSystem.Instance.RewardClaimed += OnRewardClaimed;
        }

        public void RefreshUI() {
            var playerData = SealedDungeonSystem.Instance.PlayerData;
            
            // Update stars
            int stars = SealedDungeonSystem.Instance.GetTotalStars();
            _starsLabel.Text = $"⭐ {stars}";
            
            // Update zone list
            RefreshZoneList();
            
            // Update button states
            UpdateButtonStates();
            
            // Update timer if in dungeon
            if (SealedDungeonSystem.Instance.IsInDungeon) {
                var dungeon = SealedDungeonSystem.Instance.CurrentDungeon;
                _floorLabel.Text = $"Floor {dungeon.CurrentFloor}/{dungeon.MaxFloors}";
                _scoreLabel.Text = $"Score: {dungeon.CurrentScore}";
            }
        }

        private void RefreshZoneList() {
            foreach (var child in _zoneList.GetChildren()) {
                child.QueueFree();
            }
            
            var playerData = SealedDungeonSystem.Instance.PlayerData;
            var zoneConfigs = SealedDungeonDatabase.Instance.ZoneConfigs;
            
            foreach (var kvp in zoneConfigs) {
                var zone = kvp.Key;
                var config = kvp.Value;
                var isUnlocked = playerData.UnlockedZones.Contains(zone);
                
                var zoneButton = new Button {
                    Text = isUnlocked ? $"🔓 {config.Name}" : $"🔒 {config.Name} (Cost: {config.UnlockCost})",
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                    CustomMinimumSize = new Vector2(0, 45),
                    Disabled = !isUnlocked
                };
                
                var zoneData = SealedDungeonSystem.Instance.GetZoneProgress(zone);
                
                if (zoneData.IsUnlocked) {
                    var infoLabel = new Label {
                        Text = $"  Clears: {zoneData.ClearCount} | Best: {zoneData.BestScore}",
                        FontSize = 12
                    };
                    
                    var container = new VBoxContainer {
                        SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
                    };
                    container.AddChild(zoneButton);
                    container.AddChild(infoLabel);
                    _zoneList.AddChild(container);
                } else {
                    _zoneList.AddChild(zoneButton);
                }
                
                zoneButton.Pressed += () => OnZoneSelected(zone);
            }
        }

        private void UpdateButtonStates() {
            bool inDungeon = SealedDungeonSystem.Instance.IsInDungeon;
            
            _startButton.Disabled = inDungeon;
            _completeFloorButton.Disabled = !inDungeon;
            _exitButton.Disabled = !inDungeon;
            
            var dungeon = SealedDungeonSystem.Instance.CurrentDungeon;
            if (dungeon != null) {
                var floorConfig = SealedDungeonDatabase.Instance.GetFloorConfig(dungeon.CurrentFloor);
                _claimRewardButton.Disabled = !inDungeon || floorConfig == null;
            }
        }

        private void OnTabSelected(int index) {
            _selectedTab = index;
            
            _dungeonListPanel.Visible = index == 0 || index == 1;
            _dungeonDetailPanel.Visible = index == 0 || index == 1;
            _statisticsPanel.Visible = index == 2;
            
            RefreshUI();
        }

        private void OnZoneSelected(DungeonZone zone) {
            var config = SealedDungeonDatabase.Instance.GetZoneConfig(zone);
            if (config == null) return;
            
            var progress = SealedDungeonSystem.Instance.GetZoneProgress(zone);
            
            _floorLabel.Text = $"{config.Name}\n{config.Description}";
            _scoreLabel.Text = $"Difficulty: {config.Difficulty} | Multiplier: {config.ScoreMultiplier}";
            
            // Show floor info
            foreach (var child in _floorList.GetChildren()) {
                child.QueueFree();
            }
            
            var bossFloor = config.BossFloor;
            var floorInfo = new Label {
                Text = $"Boss Floor: {bossFloor}\nSpecial: {config.SpecialMechanic}",
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            _floorList.AddChild(floorInfo);
        }

        private void OnStartButtonPressed() {
            var playerData = SealedDungeonSystem.Instance.PlayerData;
            
            if (playerData.UnlockedZones.Count > 0) {
                var firstZone = playerData.UnlockedZones[0];
                SealedDungeonSystem.Instance.StartDungeon(1);
            }
        }

        private void OnCompleteFloorButtonPressed() {
            SealedDungeonSystem.Instance.CompleteFloor(true);
        }

        private void OnClaimRewardButtonPressed() {
            var reward = SealedDungeonSystem.Instance.ClaimFloorReward();
            if (reward != null) {
                var rewardLabel = new Label {
                    Text = $"+{reward.GoldReward} Gold | +{reward.ExperienceReward} XP",
                    FontSize = 14
                };
                _rewardList.AddChild(rewardLabel);
                
                if (_rewardList.GetChildCount() > 10) {
                    _rewardList.GetChild(0).QueueFree();
                }
            }
        }

        private void OnExitButtonPressed() {
            if (SealedDungeonSystem.Instance.IsInDungeon) {
                SealedDungeonSystem.Instance.CompleteDungeon(false);
            }
        }

        private void OnDungeonStarted(object sender, SealedDungeonData dungeon) {
            RefreshUI();
        }

        private void OnDungeonCompleted(object sender, SealedDungeonData dungeon) {
            GD.Print($"[SealedDungeonUI] Dungeon completed! Score: {dungeon.CurrentScore}");
            RefreshUI();
        }

        private void OnDungeonFailed(object sender, SealedDungeonData dungeon) {
            GD.Print($"[SealedDungeonUI] Dungeon failed at floor {dungeon.CurrentFloor}");
            RefreshUI();
        }

        private void OnFloorCompleted(object sender, (int floor, bool success) result) {
            RefreshUI();
        }

        private void OnZoneUnlocked(object sender, DungeonZone zone) {
            GD.Print($"[SealedDungeonUI] Zone unlocked: {zone}");
            RefreshUI();
        }

        private void OnRewardClaimed(object sender, DungeonReward reward) {
            GD.Print($"[SealedDungeonUI] Reward claimed: {reward.GoldReward} gold, {reward.ExperienceReward} XP");
        }

        public override void _Input(InputEvent @event) {
            if (@event.IsActionPressed("ui_cancel")) {
                Hide();
            }
        }
    }
}
