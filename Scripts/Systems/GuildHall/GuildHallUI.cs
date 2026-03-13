using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.GuildHall {
    public class GuildHallUI : Control {
        private GuildHallSystem _system;
        private TabContainer _tabContainer;
        
        // UI Elements
        private Label _hallLevelLabel;
        private ProgressBar _experienceBar;
        private Label _goldLabel;
        private Label _visitorsLabel;
        
        // Rooms Tab
        private VBoxContainer _roomsList;
        
        // Decorations Tab
        private VBoxContainer _decorationsList;
        private VBoxContainer _furnitureList;
        
        // Statistics Tab
        private Label _statsLabel;
        
        public override void _Ready() {
            base._Ready();
            
            SetupUI();
            _system = GetNode<GuildHallSystem>("/root/Main/GuildHallSystem");
            
            if (_system == null) {
                GD.PrintErr("[GuildHallUI] System not found!");
                return;
            }
            
            RefreshUI();
        }
        
        private void SetupUI() {
            // Main Container
            var mainPanel = new PanelContainer {
                AnchorRight = 0.8f,
                AnchorBottom = 0.8f,
                AnchorLeft = 0.1f,
                AnchorTop = 0.1f,
                OffsetRight = 400,
                OffsetBottom = 300
            };
            AddChild(mainPanel);
            
            var mainVBox = new VBoxContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            mainPanel.AddChild(mainVBox);
            
            // Header
            var header = new HBoxContainer();
            mainVBox.AddChild(header);
            
            var titleLabel = new Label {
                Text = "Guild Hall",
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            header.AddChild(titleLabel);
            
            var closeButton = new Button {
                Text = "X",
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd
            };
            closeButton.Pressed += () => Hide();
            header.AddChild(closeButton);
            
            // Status Bar
            var statusBar = new HBoxContainer();
            mainVBox.AddChild(statusBar);
            
            _hallLevelLabel = new Label { Text = "Level: 1" };
            statusBar.AddChild(_hallLevelLabel);
            
            statusBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
            
            _goldLabel = new Label { Text = "Gold: 0" };
            statusBar.AddChild(_goldLabel);
            
            statusBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
            
            _visitorsLabel = new Label { Text = "Visitors: 0" };
            statusBar.AddChild(_visitorsLabel);
            
            // Experience Bar
            _experienceBar = new ProgressBar {
                MinValue = 0,
                MaxValue = 100,
                Value = 0,
                SizeFlagsVertical = Control.SizeFlags.ShrinkCenter
            };
            _experienceBar.CustomMinimumSize = new Vector2(0, 20);
            mainVBox.AddChild(_experienceBar);
            
            // Tab Container
            _tabContainer = new TabContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            mainVBox.AddChild(_tabContainer);
            
            // Rooms Tab
            var roomsTab = new Control { Name = "Rooms" };
            _tabContainer.AddChild(roomsTab);
            
            var roomsScroll = new ScrollContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f
            };
            roomsTab.AddChild(roomsScroll);
            
            _roomsList = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            roomsScroll.AddChild(_roomsList);
            
            // Decorations Tab
            var decorationsTab = new Control { Name = "Decorations" };
            _tabContainer.AddChild(decorationsTab);
            
            var decorationsVBox = new VBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f
            };
            decorationsTab.AddChild(decorationsVBox);
            
            var furnitureLabel = new Label { Text = "Placed Furniture:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            furnitureLabel.AddThemeFontSizeOverride("font_size", 18);
            decorationsVBox.AddChild(furnitureLabel);
            
            var furnitureScroll = new ScrollContainer {
                CustomMinimumSize = new Vector2(0, 100),
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            decorationsVBox.AddChild(furnitureScroll);
            
            _furnitureList = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            furnitureScroll.AddChild(_furnitureList);
            
            var decorationLabel = new Label { Text = "Decoration Inventory:", SizeFlagsHorizontal = Control.SizeFlags.ExpandFill };
            decorationLabel.AddThemeFontSizeOverride("font_size", 18);
            decorationsVBox.AddChild(decorationLabel);
            
            var decorationScroll = new ScrollContainer {
                CustomMinimumSize = new Vector2(0, 100),
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            decorationsVBox.AddChild(decorationScroll);
            
            _decorationsList = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            decorationScroll.AddChild(_decorationsList);
            
            // Statistics Tab
            var statsTab = new Control { Name = "Statistics" };
            _tabContainer.AddChild(statsTab);
            
            _statsLabel = new Label {
                Text = "No statistics yet",
                AnchorRight = 1f,
                AnchorBottom = 1f
            };
            statsTab.AddChild(_statsLabel);
            
            // Control Buttons
            var controls = new HBoxContainer();
            mainVBox.AddChild(controls);
            
            var depositBtn = new Button { Text = "Deposit 100 Gold" };
            depositBtn.Pressed += () => _system?.DepositGold(100);
            controls.AddChild(depositBtn);
            
            var expBtn = new Button { Text = "Add 500 XP" };
            expBtn.Pressed += () => _system?.AddExperience(500);
            controls.AddChild(expBtn);
            
            var visitBtn = new Button { Text = "Record Visit" };
            visitBtn.Pressed += () => _system?.RecordVisit("Player1");
            controls.AddChild(visitBtn);
            
            // Test unlock room button
            var unlockBtn = new Button { Text = "Unlock War Room" };
            unlockBtn.Pressed += () => _system?.UnlockRoom("War Room");
            controls.AddChild(unlockBtn);
        }
        
        public void RefreshUI() {
            if (_system == null) return;
            
            // Update status bar
            _hallLevelLabel.Text = $"Level: {_system.GetHallLevel()}";
            _goldLabel.Text = $"Gold: {_system.GetGoldDeposited()}";
            _visitorsLabel.Text = $"Visitors: {_system.GetVisitors().Count}";
            
            // Update experience bar
            _experienceBar.MaxValue = _system.GetRequiredExperience();
            _experienceBar.Value = _system.GetExperience();
            
            // Update rooms list
            RefreshRoomsList();
            
            // Update decorations list
            RefreshDecorationsList();
            
            // Update statistics
            RefreshStatistics();
        }
        
        private void RefreshRoomsList() {
            _roomsList.Clear();
            
            var database = _system.GetDatabase();
            foreach (var roomName in database.Rooms.Keys) {
                var room = database.GetRoom(roomName);
                var isUnlocked = _system.GetUnlockedRooms().Contains(roomName);
                var level = _system.GetRoomLevel(roomName);
                
                var roomPanel = new PanelContainer {
                    CustomMinimumSize = new Vector2(0, 60)
                };
                
                var roomVBox = new VBoxContainer();
                roomPanel.AddChild(roomVBox);
                
                var nameLabel = new Label {
                    Text = $"{roomName} {(isUnlocked ? $"[Lv.{level}]" : "[Locked]")}",
                    SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
                };
                if (!isUnlocked) {
                    nameLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
                }
                roomVBox.AddChild(nameLabel);
                
                var descLabel = new Label {
                    Text = room.Description,
                    Modulate = new Color(0.7f, 0.7f, 0.7f)
                };
                roomVBox.AddChild(descLabel);
                
                if (!isUnlocked) {
                    var costLabel = new Label {
                        Text = $"Cost: {room.GoldCost} Gold, Required Level: {room.RequiredLevel}",
                        Modulate = new Color(0.8f, 0.6f, 0.4f)
                    };
                    roomVBox.AddChild(costLabel);
                }
                
                _roomsList.AddChild(roomPanel);
            }
        }
        
        private void RefreshDecorationsList() {
            _decorationsList.Clear();
            _furnitureList.Clear();
            
            // Furniture placed
            foreach (var furniture in _system.GetFurniture()) {
                var item = new Label { Text = $"📦 {furniture}" };
                _furnitureList.AddChild(item);
            }
            
            // Decoration inventory
            foreach (var decoration in _system.GetDecorationInventory()) {
                var item = new Label { Text = $"🎨 {decoration}" };
                _decorationsList.AddChild(item);
            }
        }
        
        private void RefreshStatistics() {
            var stats = _system.GetStatistics();
            var text = "=== Guild Hall Statistics ===\n\n";
            
            text += $"Hall Level: {_system.GetHallLevel()}\n";
            text += $"Experience: {_system.GetExperience()} / {_system.GetRequiredExperience()}\n";
            text += $"Gold Deposited: {_system.GetGoldDeposited()}\n";
            text += $"Total Visitors: {_system.GetTotalVisits()}\n";
            text += $"Unlocked Rooms: {_system.GetUnlockedRooms().Count}\n";
            text += $"Furniture Placed: {_system.GetFurniture().Count}\n";
            text += $"Decorations Owned: {_system.GetDecorationInventory().Count}\n";
            
            _statsLabel.Text = text;
        }
    }
}
