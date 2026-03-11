using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// Pet affection UI - displays pet relationship status
    /// </summary>
    public class PetAffectionUI : Control {
        private PetAffectionSystem _affectionSystem;
        private PetSystem _petSystem;
        
        private Control _mainPanel;
        private VBoxContainer _petListContainer;
        private Label _titleLabel;
        private Label _totalAffectionLabel;
        private Label _avgLevelLabel;
        private Button _closeButton;
        
        private bool _isVisible = false;
        
        public override void _Ready() {
            _affectionSystem = PetAffectionSystem.Instance;
            _petSystem = PetSystem.Instance;
            
            SetupUI();
            SetupSignals();
            
            Visible = false;
        }
        
        private void SetupUI() {
            // Main panel
            _mainPanel = new Control {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            AddChild(_mainPanel);
            
            // Background
            var bg = new ColorRect {
                Color = new Color(0, 0, 0, 0.7f),
                AnchorRight = 1f,
                AnchorBottom = 1f
            };
            _mainPanel.AddChild(bg);
            
            // Content container
            var content = new VBoxContainer {
                AnchorLeft = 0.3f,
                AnchorTop = 0.15f,
                AnchorRight = 0.7f,
                AnchorBottom = 0.85f,
                GrowHorizontal = GrowDirection.Center,
                GrowVertical = GrowDirection.Center
            };
            _mainPanel.AddChild(content);
            
            // Title
            _titleLabel = new Label {
                Text = "❤️ 宠物好感度",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 50)
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            content.AddChild(_titleLabel);
            
            // Stats row
            var statsRow = new HBoxContainer {
                Alignment = BoxContainer.Alignment.Center
            };
            content.AddChild(statsRow);
            
            _totalAffectionLabel = new Label {
                Text = "总好感度: 0",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _totalAffectionLabel.AddThemeFontSizeOverride("font_size", 18);
            statsRow.AddChild(_totalAffectionLabel);
            
            var spacer = new Control {
                CustomMinimumSize = new Vector2(50, 0)
            };
            statsRow.AddChild(spacer);
            
            _avgLevelLabel = new Label {
                Text = "平均等级: 1.0",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _avgLevelLabel.AddThemeFontSizeOverride("font_size", 18);
            statsRow.AddChild(_avgLevelLabel);
            
            // Separator
            var sep = new HSeparator { CustomMinimumSize = new Vector2(0, 10) };
            content.AddChild(sep);
            
            // Scroll container for pet list
            var scroll = new ScrollContainer {
                SizeFlagsVertical = Control.SizeFlags.ExpandFill
            };
            content.AddChild(scroll);
            
            _petListContainer = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            scroll.AddChild(_petListContainer);
            
            // Close button
            _closeButton = new Button {
                Text = "关闭",
                CustomMinimumSize = new Vector2(200, 40),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _closeButton.AddThemeFontSizeOverride("font_size", 18);
            _closeButton.Pressed += () => ToggleUI();
            content.AddChild(_closeButton);
            
            // Click bg to close
            bg.GuiInput += (InputEvent e) => {
                if (e is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left) {
                    ToggleUI();
                }
            };
        }
        
        private void SetupSignals() {
            if (_affectionSystem != null) {
                _affectionSystem.AffectionChanged += OnAffectionChanged;
                _affectionSystem.AffectionLevelUp += OnAffectionLevelUp;
            }
        }
        
        public void ToggleUI() {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible) {
                RefreshDisplay();
            }
        }
        
        private void RefreshDisplay() {
            // Clear existing
            foreach (Node child in _petListContainer.GetChildren()) {
                child.QueueFree();
            }
            
            // Update stats
            _totalAffectionLabel.Text = $"总好感度: {_affectionSystem.GetTotalAffection()}";
            _avgLevelLabel.Text = $"平均等级: {_affectionSystem.GetAverageAffectionLevel():F1}";
            
            // Get pets
            var pets = _petSystem?.GetOwnedPets();
            if (pets == null || pets.Count == 0) {
                var noPets = new Label {
                    Text = "没有宠物",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    CustomMinimumSize = new Vector2(0, 100)
                };
                noPets.AddThemeFontSizeOverride("font_size", 20);
                _petListContainer.AddChild(noPets);
                return;
            }
            
            // Add pet cards
            foreach (var pet in pets) {
                var card = CreatePetCard(pet);
                _petListContainer.AddChild(card);
            }
        }
        
        private Control CreatePetCard(PetData pet) {
            var card = new VBoxContainer {
                CustomMinimumSize = new Vector2(0, 80)
            };
            
            // Card bg
            var bg = new ColorRect {
                Color = new Color(0.2f, 0.2f, 0.3f, 0.8f),
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            card.AddChild(bg);
            
            var content = new HBoxContainer {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                CustomMinimumSize = new Vector2(0, 70)
            };
            bg.AddChild(content);
            
            // Pet name and level
            var info = new VBoxContainer {
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill,
                Alignment = BoxContainer.Alignment.Center
            };
            content.AddChild(info);
            
            var nameLabel = new Label {
                Text = $"🐾 {pet.Name}",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 20);
            info.AddChild(nameLabel);
            
            string rarityColor = GetRarityColor(pet.Rarity);
            var levelLabel = new Label {
                Text = $"  好感等级: {_affectionSystem.GetAffectionLevel(pet.Id)} ({_affectionSystem.GetAffectionTitle(pet.Id)}) | 好感度: {_affectionSystem.GetAffectionValue(pet.Id)}",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            levelLabel.AddThemeFontSizeOverride("font_size", 16);
            info.AddChild(levelLabel);
            
            // Interaction buttons
            var buttons = new HBoxContainer {
                Alignment = BoxContainer.Alignment.Center
            };
            content.AddChild(buttons);
            
            var feedBtn = new Button {
                Text = "喂食 🍖",
                CustomMinimumSize = new Vector2(80, 30)
            };
            feedBtn.Pressed += () => {
                int gain = _affectionSystem.FeedPet(pet.Id, pet.Rarity);
                ShowGainFeedback(card, $"+{gain} ❤️");
                RefreshDisplay();
            };
            buttons.AddChild(feedBtn);
            
            var playBtn = new Button {
                Text = "玩耍 🎾",
                CustomMinimumSize = new Vector2(80, 30)
            };
            playBtn.Pressed += () => {
                int gain = _affectionSystem.PlayWithPet(pet.Id, pet.Rarity);
                ShowGainFeedback(card, $"+{gain} ❤️");
                RefreshDisplay();
            };
            buttons.AddChild(playBtn);
            
            // Bonus indicator
            var affectionData = _affectionSystem.GetOrCreateAffection(pet.Id, pet.Rarity);
            float bonus = affectionData.GetAffectionBonus();
            if (bonus > 0) {
                var bonusLabel = new Label {
                    Text = $" 属性加成: +{(bonus * 100):F0}%",
                    HorizontalAlignment = HorizontalAlignment.Right
                };
                bonusLabel.AddThemeFontSizeOverride("font_size", 14);
                info.AddChild(bonusLabel);
            }
            
            return card;
        }
        
        private void ShowGainFeedback(Control parent, string text) {
            var label = new Label {
                Text = text,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Position = new Vector2(parent.Size.x / 2 - 30, parent.Size.y / 2)
            };
            label.AddThemeFontSizeOverride("font_size", 24);
            
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(label, "modulate:a", 0f, 1f);
            tween.TweenProperty(label, "position:y", parent.Size.y / 2 - 50, 1f);
            
            parent.AddChild(label);
            tween.TweenCallback(label.QueueFree);
        }
        
        private string GetRarityColor(string rarity) {
            switch (rarity) {
                case "Legendary": return "#FF6B35";
                case "Epic": return "#9B59B6";
                case "Rare": return "#3498DB";
                case "Uncommon": return "#2ECC71";
                default: return "#95A5A6";
            }
        }
        
        private void OnAffectionChanged(string petId, int newAffection, int level) {
            if (_isVisible) {
                RefreshDisplay();
            }
        }
        
        private void OnAffectionLevelUp(string petId, int newLevel) {
            GD.Print($"[PetAffectionUI] Pet {petId} reached level {newLevel}!");
        }
        
        public override void _Input(InputEvent e) {
            if (e is InputEventKey key && key.Pressed) {
                // Ctrl+P to toggle
                if (key.Keycode == Key.P && key.ModifierMask.HasFlag(KeyModifierMask.Shift)) {
                    ToggleUI();
                }
            }
        }
    }
}
