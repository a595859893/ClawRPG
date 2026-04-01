using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Pet Emotion UI - displays pet emotional states
    /// </summary>
    public partial class PetEmotionUI : Control
    {
        private PetEmotionSystem _emotionSystem;
        private Control _mainPanel;
        private Label _titleLabel;
        private VBoxContainer _petListContainer;
        private Button _closeButton;
        private Button _refreshButton;
        
        private bool _isVisible = false;
        private string _selectedPetId = "";

        public override void _Ready()
        {
            _emotionSystem = PetEmotionSystem.Instance;
            SetupUI();
            Visible = false;
        }

        private void SetupUI()
        {
            // Main panel
            _mainPanel = new Control();
            _mainPanel.SetAnchorsPreset(Control.AnchorsPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(600, 500);
            AddChild(_mainPanel);

            // Background panel
            var bgPanel = new PanelContainer();
            bgPanel.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.CornerRadiusTopLeft = 10;
            style.CornerRadiusTopRight = 10;
            style.CornerRadiusBottomLeft = 10;
            style.CornerRadiusBottomRight = 10;
            style.BorderWidthLeft = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            bgPanel.AddThemeStyleboxOverride("panel", style);
            _mainPanel.AddChild(bgPanel);

            var mainVBox = new VBoxContainer();
            mainVBox.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
            mainVBox.AddThemeConstantOverride("separation", 10);
            bgPanel.AddChild(mainVBox);

            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "🐾 Pet Emotion System";
            _titleLabel.Align = Label.AlignEnum.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainVBox.AddChild(_titleLabel);

            // Tab container for different views
            var tabContainer = new TabContainer();
            tabContainer.SetHExpand(true);
            tabContainer.SetVExpand(true);
            mainVBox.AddChild(tabContainer);

            // Tab 1: All Pets Overview
            var overviewTab = new ScrollContainer();
            overviewTab.Name = "Overview";
            tabContainer.AddChild(overviewTab);

            _petListContainer = new VBoxContainer();
            _petListContainer.SetHExpand(true);
            _petListContainer.AddThemeConstantOverride("separation", 5);
            overviewTab.AddChild(_petListContainer);

            // Tab 2: Statistics
            var statsTab = new VBoxContainer();
            statsTab.Name = "Statistics";
            tabContainer.AddChild(statsTab);

            var statsLabel = new Label();
            statsLabel.Text = "Emotion Statistics";
            statsLabel.AddThemeFontSizeOverride("font_size", 18);
            statsLabel.Align = Label.AlignEnum.Center;
            statsTab.AddChild(statsLabel);

            var statsScroll = new ScrollContainer();
            statsScroll.SetVExpand(true);
            statsTab.AddChild(statsScroll);

            var statsContainer = new VBoxContainer();
            statsContainer.SetHExpand(true);
            statsScroll.AddChild(statsContainer);

            // Tab 3: Emotion Guide
            var guideTab = new ScrollContainer();
            guideTab.Name = "Guide";
            tabContainer.AddChild(guideTab);

            var guideContainer = new VBoxContainer();
            guideContainer.SetHExpand(true);
            guideTab.AddChild(guideContainer);

            PopulateEmotionGuide(guideContainer);

            // Button row
            var buttonRow = new HBoxContainer();
            buttonRow.Alignment = BoxContainer.AlignMode.Center;
            buttonRow.AddThemeConstantOverride("separation", 10);
            mainVBox.AddChild(buttonRow);

            _refreshButton = new Button();
            _refreshButton.Text = "🔄 Refresh";
            _refreshButton.Pressed += OnRefreshPressed;
            buttonRow.AddChild(_refreshButton);

            var resetButton = new Button();
            resetButton.Text = "🗑️ Reset All";
            resetButton.Pressed += OnResetPressed;
            buttonRow.AddChild(resetButton);

            _closeButton = new Button();
            _closeButton.Text = "❌ Close (ESC)";
            _closeButton.Pressed += OnClosePressed;
            buttonRow.AddChild(_closeButton);

            // Initial refresh
            RefreshPetList();
            RefreshStatistics();
        }

        private void PopulateEmotionGuide(VBoxContainer container)
        {
            var emotions = Database.PetEmotionDatabase.Emotions;
            
            foreach (var emotion in emotions.Values)
            {
                var emotionPanel = new HBoxContainer();
                container.AddChild(emotionPanel);

                var emojiLabel = new Label();
                emojiLabel.Text = emotion.Emoji + " ";
                emojiLabel.AddThemeFontSizeOverride("font_size", 20);
                emotionPanel.AddChild(emojiLabel);

                var infoVBox = new VBoxContainer();
                infoVBox.SetHExpand(true);
                emotionPanel.AddChild(infoVBox);

                var nameLabel = new Label();
                nameLabel.Text = $"{emotion.Name} ({emotion.Category})";
                nameLabel.AddThemeFontSizeOverride("font_size", 16);
                nameLabel.Modulate = emotion.DisplayColor;
                infoVBox.AddChild(nameLabel);

                var descLabel = new Label();
                descLabel.Text = emotion.Description;
                descLabel.AddThemeFontSizeOverride("font_size", 12);
                infoVBox.AddChild(descLabel);

                var modifierLabel = new Label();
                var modifiers = new List<string>();
                foreach (var mod in emotion.StatModifiers)
                {
                    modifiers.Add($"{mod.Key}: {mod.Value:F2}x");
                }
                modifierLabel.Text = "Modifiers: " + string.Join(", ", modifiers);
                modifierLabel.AddThemeFontSizeOverride("font_size", 11);
                infoVBox.AddChild(modifierLabel);
            }
        }

        private void RefreshPetList()
        {
            foreach (var child in _petListContainer.GetChildren())
            {
                child.QueueFree();
            }

            var allPets = _emotionSystem.GetAllPetEmotions();
            
            if (allPets.Count == 0)
            {
                var noPetsLabel = new Label();
                noPetsLabel.Text = "No pets with emotions yet.\nInteract with pets to see their emotions!";
                noPetsLabel.Align = Label.AlignEnum.Center;
                noPetsLabel.AddThemeFontSizeOverride("font_size", 16);
                _petListContainer.AddChild(noPetsLabel);
                return;
            }

            foreach (var pet in allPets)
            {
                var petCard = CreatePetEmotionCard(pet.Key, pet.Value);
                _petListContainer.AddChild(petCard);
            }
        }

        private Control CreatePetEmotionCard(string petId, Data.PetEmotionData emotionData)
        {
            var card = new PanelContainer();
            card.SetHExpand(true);
            
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.15f, 0.15f, 0.2f);
            style.CornerRadiusTopLeft = 5;
            style.CornerRadiusTopRight = 5;
            style.CornerRadiusBottomLeft = 5;
            style.CornerRadiusBottomRight = 5;
            card.AddThemeStyleboxOverride("panel", style);

            var cardHBox = new HBoxContainer();
            cardHBox.AddThemeConstantOverride("separation", 15);
            card.AddChild(cardHBox);

            // Pet ID
            var petLabel = new Label();
            petLabel.Text = $"🐾 {petId}";
            petLabel.AddThemeFontSizeOverride("font_size", 16);
            petLabel.SetVExpand(true);
            petLabel.VerticalAlignment = Label.VAlign.Center;
            cardHBox.AddChild(petLabel);

            // Emotion info
            var emotionInfo = new VBoxContainer();
            emotionInfo.SetHExpand(true);
            cardHBox.AddChild(emotionInfo);

            var emotionConfig = Database.PetEmotionDatabase.GetEmotion(emotionData.DominantEmotion);
            
            var emotionLabel = new Label();
            emotionLabel.Text = $"{emotionConfig.Emoji} {emotionConfig.Name} ({emotionData.CurrentIntensity})";
            emotionLabel.Modulate = emotionConfig.DisplayColor;
            emotionLabel.AddThemeFontSizeOverride("font_size", 14);
            emotionInfo.AddChild(emotionLabel);

            // Emotion bars
            var barsContainer = new HBoxContainer();
            barsContainer.AddThemeConstantOverride("separation", 3);
            emotionInfo.AddChild(barsContainer);

            foreach (var emotion in emotionData.CurrentEmotions)
            {
                var emotionBar = new ProgressBar();
                emotionBar.CustomMinimumSize = new Vector2(40, 8);
                emotionBar.Value = emotion.Value * 100;
                emotionBar.MaxValue = 100;
                
                var barStyle = new StyleBoxFlat();
                var config = Database.PetEmotionDatabase.GetEmotion(emotion.Key);
                barStyle.BgColor = config.DisplayColor;
                barStyle.CornerRadiusTopLeft = 2;
                barStyle.CornerRadiusTopRight = 2;
                barStyle.CornerRadiusBottomLeft = 2;
                barStyle.CornerRadiusBottomRight = 2;
                emotionBar.AddThemeStyleboxOverride("fill", barStyle);
                
                barsContainer.AddChild(emotionBar);
            }

            // Stat modifiers
            var modifiers = _emotionSystem.GetStatModifiers(petId);
            var modifierLabel = new Label();
            var modifierTexts = new List<string>();
            foreach (var mod in modifiers)
            {
                if (mod.Value != 1.0f)
                {
                    modifierTexts.Add($"{mod.Key}: {(mod.Value - 1) * 100:+0;-0}%");
                }
            }
            modifierLabel.Text = modifierTexts.Count > 0 ? "📊 " + string.Join(", ", modifierTexts) : "📊 Neutral";
            modifierLabel.AddThemeFontSizeOverride("font_size", 11);
            emotionInfo.AddChild(modifierLabel);

            return card;
        }

        private void RefreshStatistics()
        {
            // This would populate the statistics tab
            // Implementation similar to RefreshPetList
        }

        private void OnRefreshPressed()
        {
            RefreshPetList();
            RefreshStatistics();
            GD.Print("[PetEmotionUI] Refreshed");
        }

        private void OnResetPressed()
        {
            _emotionSystem.ResetAll();
            RefreshPetList();
            RefreshStatistics();
        }

        private void OnClosePressed()
        {
            ToggleVisibility();
        }

        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                RefreshPetList();
                RefreshStatistics();
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                if (Visible)
                {
                    ToggleVisibility();
                }
            }
        }

        public bool IsVisible()
        {
            return _isVisible;
        }
    }
}
