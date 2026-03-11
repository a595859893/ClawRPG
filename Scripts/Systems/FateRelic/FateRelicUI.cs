using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.UI {
    /// <summary>
    /// Fate Relic UI - displays and manages player's relic collection
    /// </summary>
    public class FateRelicUI : Control {
        private FateRelicSystem _relicSystem;
        
        // UI Elements
        private Label _titleLabel;
        private HBoxContainer _mainContainer;
        private VBoxContainer _relicListContainer;
        private VBoxContainer _detailPanel;
        private Label _detailName;
        private Label _detailDescription;
        private Label _detailRarity;
        private Label _detailType;
        private Label _detailEffects;
        private Label _statsLabel;
        private Button _equipButton;
        private Button _unequipButton;
        private Button _expandSlotsButton;
        private Button _closeButton;
        
        private string _selectedRelicId;
        private bool _isVisible;
        
        public override void _Ready() {
            _relicSystem = FateRelicSystem.Instance;
            _relicSystem.Initialize();
            
            SetupUI();
            _isVisible = false;
            Visible = false;
            
            // Connect signals
            _relicSystem.OnRelicAcquired += OnRelicAcquired;
            _relicSystem.OnRelicEquipped += OnRelicEquipped;
            _relicSystem.OnRelicUnequipped += OnRelicUnequipped;
            
            GD.Print("[FateRelicUI] Initialized");
        }
        
        private void SetupUI() {
            // Main container
            _mainContainer = new HBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 100,
                OffsetTop = 50,
                OffsetRight = -100,
                OffsetBottom = -50
            };
            AddChild(_mainContainer);
            
            // Left panel - Relic list
            var leftPanel = new PanelContainer {
                CustomMinimumSize = new Vector2(400, 0),
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            _mainContainer.AddChild(leftPanel);
            
            var leftVBox = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            leftPanel.AddChild(leftVBox);
            
            // Title
            _titleLabel = new Label {
                Text = "🎭 Fate Relics",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            leftVBox.AddChild(_titleLabel);
            
            // Stats
            _statsLabel = new Label {
                Text = "Relics: 0/0 | Slots: 3",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            leftVBox.AddChild(_statsLabel);
            
            // Scroll container for relic list
            var scrollContainer = new ScrollContainer {
                SizeFlagsVertical = Control.SizeFlags.Expand
            };
            leftVBox.AddChild(scrollContainer);
            
            _relicListContainer = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            scrollContainer.AddChild(_relicListContainer);
            
            // Buttons
            var buttonContainer = new HBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            leftVBox.AddChild(buttonContainer);
            
            _expandSlotsButton = new Button {
                Text = "Expand Slots (500g)",
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            _expandSlotsButton.Pressed += OnExpandSlotsPressed;
            buttonContainer.AddChild(_expandSlotsButton);
            
            _closeButton = new Button {
                Text = "Close",
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            _closeButton.Pressed += OnClosePressed;
            buttonContainer.AddChild(_closeButton);
            
            // Right panel - Detail view
            var rightPanel = new PanelContainer {
                CustomMinimumSize = new Vector2(350, 0),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd
            };
            _mainContainer.AddChild(rightPanel);
            
            _detailPanel = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            rightPanel.AddChild(_detailPanel);
            
            // Detail content
            _detailName = new Label {
                Text = "Select a relic",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _detailName.AddThemeFontSizeOverride("font_size", 20);
            _detailPanel.AddChild(_detailName);
            
            _detailRarity = new Label {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _detailPanel.AddChild(_detailRarity);
            
            _detailType = new Label {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _detailPanel.AddChild(_detailType);
            
            var spacer1 = new Control {
                CustomMinimumSize = new Vector2(0, 20)
            };
            _detailPanel.AddChild(spacer1);
            
            _detailDescription = new Label {
                Text = "",
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            _detailPanel.AddChild(_detailDescription);
            
            var spacer2 = new Control {
                CustomMinimumSize = new Vector2(0, 20)
            };
            _detailPanel.AddChild(spacer2);
            
            _detailEffects = new Label {
                Text = "",
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            _detailPanel.AddChild(_detailEffects);
            
            // Equip/Unequip buttons
            var actionContainer = new HBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            _detailPanel.AddChild(actionContainer);
            
            _equipButton = new Button {
                Text = "Equip",
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            _equipButton.Pressed += OnEquipPressed;
            actionContainer.AddChild(_equipButton);
            
            _unequipButton = new Button {
                Text = "Unequip",
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            _unequipButton.Pressed += OnUnequipPressed;
            actionContainer.AddChild(_unequipButton);
            
            RefreshRelicList();
        }
        
        private void RefreshRelicList() {
            // Clear existing items
            foreach (var child in _relicListContainer.GetChildren()) {
                child.QueueFree();
            }
            
            var relics = _relicSystem.GetOwnedRelics();
            
            // Sort by rarity (legendary first)
            relics.Sort((a, b) => {
                int rarityCompare = Array.IndexOf(RelicRarity.All, b.Rarity).CompareTo(Array.IndexOf(RelicRarity.All, a.Rarity));
                if (rarityCompare != 0) return rarityCompare;
                return string.Compare(a.Name, b.Name);
            });
            
            foreach (var relic in relics) {
                var item = CreateRelicItem(relic);
                _relicListContainer.AddChild(item);
            }
            
            // Update stats
            int owned = _relicSystem.GetOwnedCount();
            int equipped = _relicSystem.GetEquippedCount();
            int maxSlots = _relicSystem.GetMaxSlots();
            _statsLabel.Text = $"Relics: {owned} | Equipped: {equipped}/{maxSlots}";
        }
        
        private Control CreateRelicItem(FateRelic relic) {
            var container = new Button {
                Text = $"{(relic.IsEquipped ? "✓ " : "  ")}{relic.Name} (x{relic.StackCount})",
                TextAlign = TextAlign.Left,
                CustomMinimumSize = new Vector2(0, 40)
            };
            
            // Set color based on rarity
            Color rarityColor;
            switch (relic.Rarity.Name) {
                case "Common": rarityColor = new Color(0.62f, 0.62f, 0.62f); break;
                case "Uncommon": rarityColor = new Color(0.30f, 0.69f, 0.31f); break;
                case "Rare": rarityColor = new Color(0.13f, 0.59f, 0.95f); break;
                case "Epic": rarityColor = new Color(0.61f, 0.15f, 0.69f); break;
                case "Legendary": rarityColor = new Color(1.0f, 0.60f, 0f); break;
                default: rarityColor = Colors.White;
            }
            container.Modulate = rarityColor;
            
            container.Pressed += () => OnRelicSelected(relic.Id);
            
            return container;
        }
        
        private void OnRelicSelected(string relicId) {
            _selectedRelicId = relicId;
            var relic = _relicSystem.GetRelic(relicId);
            
            if (relic == null) return;
            
            _detailName.Text = relic.Name;
            _detailRarity.Text = $"[{relic.Rarity.Name}]";
            _detailType.Text = $"Type: {relic.Type.Name}";
            _detailDescription.Text = relic.Description;
            
            // Build effects text
            var effectsText = "Effects:\n";
            foreach (var effect in relic.Effects) {
                string sign = effect.Value >= 0 ? "+" : "";
                effectsText += $"• {sign}{effect.Value * 100:F0}% {effect.Stat.Replace("_", " ")}\n";
            }
            if (relic.StackCount > 1) {
                effectsText += $"\nStacks: x{relic.StackCount}";
            }
            _detailEffects.Text = effectsText;
            
            // Update button visibility
            _equipButton.Visible = !relic.IsEquipped;
            _unequipButton.Visible = relic.IsEquipped;
        }
        
        private void OnEquipPressed() {
            if (!string.IsNullOrEmpty(_selectedRelicId)) {
                _relicSystem.EquipRelic(_selectedRelicId);
            }
        }
        
        private void OnUnequipPressed() {
            if (!string.IsNullOrEmpty(_selectedRelicId)) {
                _relicSystem.UnequipRelic(_selectedRelicId);
            }
        }
        
        private void OnExpandSlotsPressed() {
            // Expand slots for gold
            _relicSystem.ExpandRelicSlots(1, 500);
            RefreshRelicList();
        }
        
        private void OnClosePressed() {
            ToggleUI();
        }
        
        private void OnRelicAcquired(string relicId) {
            RefreshRelicList();
        }
        
        private void OnRelicEquipped(string relicId) {
            RefreshRelicList();
            if (_selectedRelicId == relicId) {
                OnRelicSelected(relicId);
            }
        }
        
        private void OnRelicUnequipped(string relicId) {
            RefreshRelicList();
            if (_selectedRelicId == relicId) {
                OnRelicSelected(relicId);
            }
        }
        
        public void ToggleUI() {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible) {
                RefreshRelicList();
            }
        }
        
        public override void _Input(InputEvent e) {
            if (e is InputEventKey keyEvent && keyEvent.Pressed) {
                if (keyEvent.Keycode == Key.R) {
                    ToggleUI();
                }
            }
        }
    }
}
