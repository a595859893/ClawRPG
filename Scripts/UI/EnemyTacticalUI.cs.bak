using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.AI;
using ClawRPG.Scripts.Characters;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Enemy Tactical Info UI - displays enemy AI perception and decisions
    /// Based on Advanced Game UI/UX Design Patterns
    /// </summary>
    public partial class EnemyTacticalUI : Control {
        // UI Components
        private Label _titleLabel;
        private VBoxContainer _enemyList;
        private Label _playerPerceptionLabel;
        
        // Theme colors
        private Color _titleColor = new Color(1f, 0.8f, 0.4f);
        private Color _enemyColor = new Color(0.9f, 0.9f, 0.95f);
        private Color _perceptionColor = new Color(0.6f, 0.8f, 1f);
        private Color _tacticalColor = new Color(1f, 0.7f, 0.5f);
        
        // Enemy tracking
        private List<Enemy> _trackedEnemies = new();
        private Dictionary<Enemy, EnemyTacticalAI> _enemyAI = new();
        private Dictionary<Enemy, Label> _enemyLabels = new();
        
        // Visibility
        private bool _isVisible = false;
        
        public override void _Ready() {
            // Create UI
            SetupUI();
            
            // Connect to game signals
            ConnectSignals();
            
            // Hide by default
            Hide();
        }
        
        /// <summary>
        /// Setup UI components
        /// </summary>
        private void SetupUI() {
            // Main container
            var mainContainer = new HBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 20f,
                OffsetTop = 20f,
                OffsetRight = -20f,
                OffsetBottom = -20f
            };
            AddChild(mainContainer);
            
            // Left panel - Enemy list
            var leftPanel = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill,
                CustomMinimumSize = new Vector2(300, 0)
            };
            mainContainer.AddChild(leftPanel);
            
            // Title
            _titleLabel = new Label {
                Text = "👁️ Enemy Tactical Analysis",
                HorizontalAlignment = HorizontalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 20)
            };
            _titleLabel.Modulate = _titleColor;
            leftPanel.AddChild(_titleLabel);
            
            // Enemy list container with scroll
            var scrollContainer = new ScrollContainer {
                SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill
            };
            leftPanel.AddChild(scrollContainer);
            
            _enemyList = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill
            };
            scrollContainer.AddChild(_enemyList);
            
            // Separator
            var separator = new HSeparator();
            mainContainer.AddChild(separator);
            
            // Right panel - Player perception
            var rightPanel = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill,
                CustomMinimumSize = new Vector2(250, 0)
            };
            mainContainer.AddChild(rightPanel);
            
            var perceptionTitle = new Label {
                Text = "🎯 Player Perception",
                HorizontalAlignment = HorizontalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 18)
            };
            perceptionTitle.Modulate = _perceptionColor;
            rightPanel.AddChild(perceptionTitle);
            
            _playerPerceptionLabel = new Label {
                Text = "Not detected by enemies",
                VerticalAlignment = VerticalAlignment.Top
            };
            _playerPerceptionLabel.Modulate = _enemyColor;
            rightPanel.AddChild(_playerPerceptionLabel);
            
            // Instructions
            var instructions = new Label {
                Text = "\n[H] Toggle This Panel",
                HorizontalAlignment = HorizontalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 14)
            };
            instructions.Modulate = new Color(0.7f, 0.7f, 0.7f);
            rightPanel.AddChild(instructions);
        }
        
        /// <summary>
        /// Connect game signals
        /// </summary>
        private void ConnectSignals() {
            // Enemy spawn/death signals would be connected here
            // For simplicity, we'll refresh in _Process
        }
        
        /// <summary>
        /// Toggle visibility
        /// </summary>
        public void ToggleVisibility() {
            _isVisible = !_isVisible;
            
            if (_isVisible) {
                Show();
                RefreshData();
            } else {
                Hide();
            }
        }
        
        /// <summary>
        /// Refresh all data
        /// </summary>
        public void RefreshData() {
            // Find all enemies
            var enemies = GetTree().GetNodesInGroup("enemies");
            
            // Clear old entries
            foreach (var child in _enemyList.GetChildren()) {
                child.QueueFree();
            }
            _enemyLabels.Clear();
            
            // Track each enemy
            int visibleCount = 0;
            int awareCount = 0;
            
            foreach (var node in enemies) {
                if (node is Enemy enemy && !enemy.IsDead) {
                    AddEnemyEntry(enemy);
                    
                    // Check if player is in detection range
                    var player = enemy.GetTarget();
                    if (player != null) {
                        float dist = enemy.GlobalPosition.DistanceTo(player.GlobalPosition);
                        if (dist < enemy.DetectionRange) {
                            awareCount++;
                        }
                        if (dist < enemy.ChaseRange) {
                            visibleCount++;
                        }
                    }
                }
            }
            
            // Update player perception
            UpdatePlayerPerception(visibleCount, awareCount, enemies.Count);
        }
        
        /// <summary>
        /// Add enemy entry to list
        /// </summary>
        private void AddEnemyEntry(Enemy enemy) {
            var entryContainer = new VBoxContainer {
                CustomMinimumSize = new Vector2(0, 60)
            };
            
            // Enemy name and health
            var nameLabel = new Label {
                Text = $"⚔️ {enemy.EnemyName}",
                AddThemeFontSizeOverride("font_size", 16)
            };
            nameLabel.Modulate = _enemyColor;
            entryContainer.AddChild(nameLabel);
            
            // Health bar
            var healthContainer = new HBoxContainer();
            entryContainer.AddChild(healthContainer);
            
            var healthLabel = new Label {
                Text = $"HP: {enemy.CurrentHealth}/{enemy.MaxHealth}",
                AddThemeFontSizeOverride("font_size", 12)
            };
            healthLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
            healthContainer.AddChild(healthLabel);
            
            // State
            var stateLabel = new Label {
                Text = $"State: Analyzing...",
                AddThemeFontSizeOverride("font_size", 11)
            };
            stateLabel.Modulate = _tacticalColor;
            entryContainer.AddChild(stateLabel);
            
            // Range info
            var rangeLabel = new Label {
                Text = $"Detection: {enemy.DetectionRange}px | Attack: {enemy.AttackRange}px",
                AddThemeFontSizeOverride("font_size", 10)
            };
            rangeLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            entryContainer.AddChild(rangeLabel);
            
            _enemyList.AddChild(entryContainer);
            _enemyLabels[enemy] = stateLabel;
        }
        
        /// <summary>
        /// Update player perception display
        /// </summary>
        private void UpdatePlayerPerception(int visibleCount, int awareCount, int totalEnemies) {
            string perception = "";
            
            if (totalEnemies == 0) {
                perception = "No enemies nearby";
            } else if (awareCount == 0) {
                perception = "✓ Undetected\n\n" +
                           $"• {totalEnemies} enemies in area\n" +
                           $"• Not in detection range";
            } else if (visibleCount == 0) {
                perception = "⚠️ Detected!\n\n" +
                           $"• {awareCount} enemies aware of you\n" +
                           $"• {totalEnemies - awareCount} enemies nearby\n" +
                           $"• They may be searching...";
            } else {
                perception = "❌ In Combat!\n\n" +
                           $"• {visibleCount} enemies see you\n" +
                           $"• {awareCount - visibleCount} aware but not engaged\n" +
                           $"• {totalEnemies - awareCount} enemies nearby";
            }
            
            _playerPerceptionLabel.Text = perception;
        }
        
        public override void _Process(double delta) {
            if (_isVisible && Engine.GetFramesDrawn() % 30 == 0) {
                // Refresh every ~0.5 seconds
                RefreshData();
            }
        }
        
        public override void _Input(InputEvent evt) {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Keycode == Key.H) {
                    ToggleVisibility();
                }
            }
        }
    }
}
