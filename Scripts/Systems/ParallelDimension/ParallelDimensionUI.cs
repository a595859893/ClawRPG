using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.ParallelDimension;

namespace ClawRPG.Scripts.Systems.ParallelDimension {
    
    public partial class ParallelDimensionUI : Control {
        
        private static ParallelDimensionUI _instance;
        public static ParallelDimensionUI Instance => _instance;
        
        private Control _mainPanel;
        private VBoxContainer _dimensionList;
        private Label _titleLabel;
        private Label _scoreLabel;
        private Label _masteredLabel;
        private Button _closeButton;
        private ColorRect _background;
        
        private bool _isVisible = false;
        
        public override void _Ready() {
            _instance = this;
            SetupUI();
            GD.Print("[ParallelDimensionUI] Initialized - Press Ctrl+Shift+P to toggle");
        }
        
        private void SetupUI() {
            _background = new ColorRect {
                Color = new Color(0f, 0f, 0f, 0.7f),
                AnchorsPreset = Control.LayoutPreset.FullRect
            };
            AddChild(_background);
            
            _mainPanel = new Panel {
                AnchorLeft = 0.3f,
                AnchorTop = 0.15f,
                AnchorRight = 0.7f,
                AnchorBottom = 0.85f,
                CustomMinimumSize = new Vector2(600, 500)
            };
            AddChild(_mainPanel);
            
            var vbox = new VBoxContainer {
                AnchorLeft = 0f,
                AnchorTop = 0f,
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 10,
                OffsetTop = 10,
                OffsetRight = -10,
                OffsetBottom = -10
            };
            _mainPanel.AddChild(vbox);
            
            var header = new HBoxContainer {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            vbox.AddChild(header);
            
            _titleLabel = new Label {
                Text = "⚡ Parallel Dimensions ⚡",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            header.AddChild(_titleLabel);
            
            var spacer = new Control {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            header.AddChild(spacer);
            
            _closeButton = new Button {
                Text = "✕",
                CustomMinimumSize = new Vector2(40, 40)
            };
            _closeButton.Pressed += OnClosePressed;
            header.AddChild(_closeButton);
            
            var statsContainer = new HBoxContainer {
                Alignment = BoxContainer.AlignmentMode.Center
            };
            vbox.AddChild(statsContainer);
            
            _scoreLabel = new Label {
                Text = "Total Score: 0"
            };
            _scoreLabel.AddThemeFontSizeOverride("font_size", 18);
            statsContainer.AddChild(_scoreLabel);
            
            var spacer2 = new Control {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                CustomMinimumSize = new Vector2(50, 0)
            };
            statsContainer.AddChild(spacer2);
            
            _masteredLabel = new Label {
                Text = "Mastered: 0/10"
            };
            _masteredLabel.AddThemeFontSizeOverride("font_size", 18);
            statsContainer.AddChild(_masteredLabel);
            
            var separator = new HSeparator();
            vbox.AddChild(separator);
            
            var scrollContainer = new ScrollContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            vbox.AddChild(scrollContainer);
            
            _dimensionList = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            scrollContainer.AddChild(_dimensionList);
            
            RefreshDimensionList();
            
            Visible = false;
        }
        
        public void RefreshDimensionList() {
            foreach (var child in _dimensionList.GetChildren()) {
                child.QueueFree();
            }
            
            var dimensions = ParallelDimensionDatabase.GetAllDimensions();
            
            foreach (var dim in dimensions) {
                var dimCard = CreateDimensionCard(dim);
                _dimensionList.AddChild(dimCard);
            }
            
            var playerData = ParallelDimensionSystem.Instance.GetPlayerData();
            _scoreLabel.Text = $"Total Score: {playerData.TotalDimensionScore}";
            _masteredLabel.Text = $"Mastered: {playerData.DimensionsMastered}/10";
        }
        
        private Control CreateDimensionCard(DimensionEntry dim) {
            var card = new PanelContainer {
                CustomMinimumSize = new Vector2(0, 120),
                Margin = new Margin { Left = 5, Top = 5, Right = 5, Bottom = 5 }
            };
            
            var hbox = new HBoxContainer {
                Margin = new Margin { Left = 10, Top = 10, Right = 10, Bottom = 10 }
            };
            card.AddChild(hbox);
            
            var infoVBox = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            hbox.AddChild(infoVBox);
            
            var nameLabel = new Label {
                Text = $"{dim.DimensionName} (Floor {dim.CurrentFloor}/{dim.MaxFloors})",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 20);
            infoVBox.AddChild(nameLabel);
            
            var descLabel = new Label {
                Text = dim.Description,
                HorizontalAlignment = HorizontalAlignment.Left,
                Autowrap = true
            };
            descLabel.AddThemeFontSizeOverride("font_size", 14);
            infoVBox.AddChild(descLabel);
            
            var statsLabel = new Label {
                Text = $"Best Score: {dim.BestScore} | Times: {dim.TimesCompleted}",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            statsLabel.AddThemeFontSizeOverride("font_size", 14);
            infoVBox.AddChild(statsLabel);
            
            var rulesLabel = new Label {
                Text = dim.Rules?.Description ?? "",
                HorizontalAlignment = HorizontalAlignment.Left,
                Modulate = new Color(1f, 0.8f, 0.4f, 1f)
            };
            rulesLabel.AddThemeFontSizeOverride("font_size", 12);
            infoVBox.AddChild(rulesLabel);
            
            var buttonVBox = new VBoxContainer {
                CustomMinimumSize = new Vector2(150, 0)
            };
            hbox.AddChild(buttonVBox);
            
            if (dim.State == DimensionState.Locked) {
                var lockLabel = new Label {
                    Text = $"🔒 Lv.{dim.RequiredLevel}",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                lockLabel.AddThemeFontSizeOverride("font_size", 18);
                buttonVBox.AddChild(lockLabel);
            } else if (dim.State == DimensionState.InProgress) {
                var inProgressLabel = new Label {
                    Text = "⏳ In Progress",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = new Color(1f, 0.8f, 0.2f, 1f)
                };
                inProgressLabel.AddThemeFontSizeOverride("font_size", 16);
                buttonVBox.AddChild(inProgressLabel);
                
                var continueBtn = new Button {
                    Text = "Continue"
                };
                continueBtn.Pressed += () => OnEnterDimension(dim.DimensionId);
                buttonVBox.AddChild(continueBtn);
                
                var exitBtn = new Button {
                    Text = "Exit"
                };
                exitBtn.Pressed += OnExitDimension;
                buttonVBox.AddChild(exitBtn);
            } else {
                var stateText = dim.State == DimensionState.Mastered ? "✅ Mastered" : "✅ Available";
                var stateLabel = new Label {
                    Text = stateText,
                    HorizontalAlignment = HorizontalAlignment.Center,
                    Modulate = dim.State == DimensionState.Mastered ? new Color(1f, 0.8f, 0.2f, 1f) : new Color(0.4f, 1f, 0.4f, 1f)
                };
                stateLabel.AddThemeFontSizeOverride("font_size", 16);
                buttonVBox.AddChild(stateLabel);
                
                var costLabel = new Label {
                    Text = $"Cost: {dim.EntryCost}g",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                buttonVBox.AddChild(costLabel);
                
                var enterBtn = new Button {
                    Text = "Enter"
                };
                enterBtn.Pressed += () => OnEnterDimension(dim.DimensionId);
                buttonVBox.AddChild(enterBtn);
            }
            
            return card;
        }
        
        private void OnEnterDimension(int dimensionId) {
            var dim = ParallelDimensionDatabase.GetDimension(dimensionId);
            if (dim == null) return;
            
            var playerGold = 10000;
            var playerLevel = 50;
            
            if (ParallelDimensionSystem.Instance.EnterDimension(dimensionId, playerLevel, playerGold)) {
                GD.Print($"[ParallelDimensionUI] Entering dimension {dim.DimensionName}");
                RefreshDimensionList();
            }
        }
        
        private void OnExitDimension() {
            ParallelDimensionSystem.Instance.ExitDimension();
            RefreshDimensionList();
        }
        
        private void OnClosePressed() {
            ToggleUI();
        }
        
        public void ToggleUI() {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible) {
                RefreshDimensionList();
            }
            
            GD.Print($"[ParallelDimensionUI] UI Toggled: {_isVisible}");
        }
        
        public override void _Input(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Keycode == Key.P && keyEvent.CtrlShift) {
                    ToggleUI();
                }
            }
        }
    }
}
