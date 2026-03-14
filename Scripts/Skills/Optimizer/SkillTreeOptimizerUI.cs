using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Skills.Optimizer;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Skill Tree Optimizer UI
    /// </summary>
    public class SkillTreeOptimizerUI : Control {
        private VBoxContainer _mainContainer;
        private HBoxContainer _buttonContainer;
        private VBoxContainer _presetContainer;
        private VBoxContainer _statsContainer;
        private Label _titleLabel;
        private Label _statsLabel;
        private Button _fullResetButton;
        private Button _closeButton;
        private bool _isVisible = false;

        public override void _Ready() {
            Visible = false;
            SetupUI();
        }

        private void SetupUI() {
            // Main panel
            var panel = new PanelContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                CustomMinimumSize = new Vector2(600, 500)
            };
            AddChild(panel);

            var mainMargin = new MarginContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                MarginLeft = 50,
                MarginTop = 50,
                MarginRight = -50,
                MarginBottom = -50
            };
            panel.AddChild(mainMargin);

            _mainContainer = new VBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f
            };
            mainMargin.AddChild(_mainContainer);

            // Title
            _titleLabel = new Label {
                Text = "⚡ Skill Tree Optimizer",
                Align = Label.AlignEnum.Center,
                CustomMinimumSize = new Vector2(0, 50)
            };
            _mainContainer.AddChild(_titleLabel);

            // Separator
            var hsep1 = new HSeparator();
            _mainContainer.AddChild(hsep1);

            // Stats container
            _statsContainer = new VBoxContainer {
                CustomMinimumSize = new Vector2(0, 80)
            };
            _mainContainer.AddChild(_statsContainer);

            _statsLabel = new Label {
                Text = "Optimization Statistics\nTotal Optimizations: 0\nPoints Saved: 0\nPresets Unlocked: 3",
                Align = Label.AlignEnum.Center
            };
            _statsContainer.AddChild(_statsLabel);

            // Separator
            var hsep2 = new HSeparator();
            _mainContainer.AddChild(hsep2);

            // Presets section
            var presetLabel = new Label {
                Text = "Available Presets",
                Align = Label.AlignEnum.Center
            };
            _mainContainer.AddChild(presetLabel);

            _presetContainer = new VBoxContainer {
                CustomMinimumSize = new Vector2(0, 200)
            };
            _mainContainer.AddChild(_presetContainer);

            // Button container
            _buttonContainer = new HBoxContainer {
                Alignment = BoxContainer.AlignMode.Center,
                CustomMinimumSize = new Vector2(0, 50)
            };
            _mainContainer.AddChild(_buttonContainer);

            // Full Reset button
            _fullResetButton = new Button {
                Text = "Full Reset",
                CustomMinimumSize = new Vector2(150, 40)
            };
            _fullResetButton.Connect("pressed", this, nameof(_OnFullResetPressed));
            _buttonContainer.AddChild(_fullResetButton);

            // Spacing
            var spacer = new Control {
                CustomMinimumSize = new Vector2(20, 0)
            };
            _buttonContainer.AddChild(spacer);

            // Close button
            _closeButton = new Button {
                Text = "Close",
                CustomMinimumSize = new Vector2(150, 40)
            };
            _closeButton.Connect("pressed", this, nameof(_OnClosePressed));
            _buttonContainer.AddChild(_closeButton);

            // Load presets
            RefreshPresets();
            RefreshStats();
        }

        private void RefreshPresets() {
            // Clear existing preset buttons
            foreach (var child in _presetContainer.GetChildren()) {
                child.QueueFree();
            }

            var optimizer = SkillTreeOptimizer.Instance;
            if (optimizer == null) return;

            var presets = optimizer.GetAvailablePresets();
            foreach (var preset in presets) {
                var presetButton = CreatePresetButton(preset);
                _presetContainer.AddChild(presetButton);
            }
        }

        private Button CreatePresetButton(SkillTreePreset preset) {
            var button = new Button {
                Text = $"{preset.Name}: {preset.Description}",
                CustomMinimumSize = new Vector2(0, 45),
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            button.Connect("pressed", this, nameof(_OnPresetPressed), new Godot.Collections.Array { preset.Id });
            return button;
        }

        private void RefreshStats() {
            var optimizer = SkillTreeOptimizer.Instance;
            if (optimizer == null) return;

            var progress = optimizer.GetProgress();
            _statsLabel.Text = $"Optimization Statistics\n" +
                $"Total Optimizations: {progress.TotalOptimizations}\n" +
                $"Points Saved: {progress.PointsSaved}\n" +
                $"Presets Unlocked: {progress.UnlockedPresets.Count}";
        }

        private void _OnPresetPressed(string presetId) {
            var optimizer = SkillTreeOptimizer.Instance;
            if (optimizer == null) return;

            // Get current skill points (would integrate with SkillMasterySystem)
            var currentPoints = new Dictionary<string, int> {
                { "attack", 5 },
                { "defense", 3 },
                { "critical", 4 }
            };

            if (optimizer.ApplyPreset(presetId, currentPoints)) {
                GD.Print($"Applied preset: {presetId}");
                RefreshStats();
                RefreshPresets();
            }
        }

        private void _OnFullResetPressed() {
            var optimizer = SkillTreeOptimizer.Instance;
            if (optimizer == null) return;

            var currentPoints = new Dictionary<string, int> {
                { "attack", 5 },
                { "defense", 3 },
                { "critical", 4 }
            };

            var result = optimizer.FullReset(currentPoints);
            GD.Print("Full skill tree reset performed");
            RefreshStats();
        }

        private void _OnClosePressed() {
            ToggleUI();
        }

        public void ToggleUI() {
            _isVisible = !_isVisible;
            Visible = _isVisible;

            if (_isVisible) {
                RefreshPresets();
                RefreshStats();
            }
        }

        public override void _Input(InputEvent @event) {
            if (@event.IsActionPressed("ui_cancel") && _isVisible) {
                ToggleUI();
            }
        }
    }
}
