using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ChoiceEvents
{
    /// <summary>
    /// Choice event UI - displays choice events and handles user input
    /// </summary>
    public partial class ChoiceEventUI : Control
    {
        private PanelContainer _mainPanel;
        private VBoxContainer _mainVBox;
        
        // Event display
        private Label _titleLabel;
        private Label _descriptionLabel;
        
        // Options container
        private VBoxContainer _optionsContainer;
        private List<Button> _optionButtons;
        
        // Statistics panel
        private PanelContainer _statsPanel;
        private Label _statsLabel;
        
        // Current state
        private bool _isVisible = false;
        private const string KEY_TOGGLE = "ui_choice_event";  // C key
        
        public ChoiceEventUI()
        {
            _optionButtons = new List<Button>();
        }
        
        public override void _Ready()
        {
            // Create main panel
            _mainPanel = new PanelContainer();
            _mainPanel.Name = "ChoiceEventPanel";
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(600, 500);
            AddChild(_mainPanel);
            
            // Create main vertical box
            _mainVBox = new VBoxContainer();
            _mainVBox.Name = "MainVBox";
            _mainVBox.SetHorizontalExpandMode(Control.ExpandMode.ExpandFill);
            _mainVBox.AddThemeConstantOverride("separation", 15);
            _mainPanel.AddChild(_mainVBox);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Name = "TitleLabel";
            _titleLabel.Text = "随机事件选择";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            _mainVBox.AddChild(_titleLabel);
            
            // Description
            _descriptionLabel = new Label();
            _descriptionLabel.Name = "DescriptionLabel";
            _descriptionLabel.Text = "选择你的命运...";
            _descriptionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _descriptionLabel.AddThemeFontSizeOverride("font_size", 18);
            _mainVBox.AddChild(_descriptionLabel);
            
            // Separator
            var hseparator = new HSeparator();
            _mainVBox.AddChild(hseparator);
            
            // Options container with scroll
            var scrollContainer = new ScrollContainer();
            scrollContainer.Name = "OptionsScroll";
            scrollContainer.SetHorizontalExpandMode(Control.ExpandMode.ExpandFill);
            scrollContainer.VerticalScrollMode = ScrollMode.Auto;
            scrollContainer.CustomMinimumSize = new Vector2(0, 300);
            _mainVBox.AddChild(scrollContainer);
            
            _optionsContainer = new VBoxContainer();
            _optionsContainer.Name = "OptionsContainer";
            _optionsContainer.SetHorizontalExpandMode(Control.ExpandMode.ExpandFill);
            _optionsContainer.AddThemeConstantOverride("separation", 10);
            scrollContainer.AddChild(_optionsContainer);
            
            // Separator
            var hseparator2 = new HSeparator();
            _mainVBox.AddChild(hseparator2);
            
            // Stats panel
            _statsPanel = new PanelContainer();
            _statsPanel.Name = "StatsPanel";
            _statsPanel.CustomMinimumSize = new Vector2(0, 80);
            _mainVBox.AddChild(_statsPanel);
            
            var statsVBox = new VBoxContainer();
            _statsPanel.AddChild(statsVBox);
            
            var statsTitle = new Label();
            statsTitle.Text = "选择统计";
            statsTitle.AddThemeFontSizeOverride("font_size", 16);
            statsVBox.AddChild(statsTitle);
            
            _statsLabel = new Label();
            _statsLabel.Name = "StatsLabel";
            _statsLabel.Text = "总事件: 0 | 总选择: 0";
            statsVBox.AddChild(_statsLabel);
            
            // Close button
            var closeButton = new Button();
            closeButton.Text = "关闭 (ESC)";
            closeButton.Pressed += OnCloseButtonPressed;
            _mainVBox.AddChild(closeButton);
            
            // Initial state - hidden
            Visible = false;
            
            // Connect to system signals
            if (ChoiceEventSystem.Instance != null)
            {
                ChoiceEventSystem.Instance.EventStarted += OnEventStarted;
                ChoiceEventSystem.Instance.EventEnded += OnEventEnded;
            }
            
            // Setup input
            SetupInput();
            
            // Update stats
            UpdateStats();
            
            GD.Print("ChoiceEventUI initialized");
        }
        
        private void SetupInput()
        {
            // Input will be handled in _Input method
        }
        
        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Toggle with C key
                if (keyEvent.Keycode == Key.C)
                {
                    ToggleVisibility();
                }
                // Close with Escape
                else if (keyEvent.Keycode == Key.Escape)
                {
                    if (Visible)
                    {
                        Hide();
                    }
                }
                // Number keys 1-9 for quick selection
                else if (keyEvent.Keycode >= Key.Key1 && keyEvent.Keycode <= Key.Key9)
                {
                    int index = keyEvent.Keycode - Key.Key1;
                    if (Visible && index < _optionButtons.Count)
                    {
                        OnOptionSelected(index);
                    }
                }
            }
        }
        
        /// <summary>
        /// Toggle UI visibility
        /// </summary>
        public void ToggleVisibility()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                RefreshDisplay();
            }
        }
        
        /// <summary>
        /// Show the UI
        /// </summary>
        public void Show()
        {
            Visible = true;
            _isVisible = true;
            RefreshDisplay();
            
            // Play tween animation
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_mainPanel, "modulate:a", 1.0, 0.3f);
            tween.TweenProperty(_mainPanel, "scale", Vector2.One, 0.3f).SetTrans(Tween.TransitionType.Back).SetEasing(Tween.EasingFunction.EaseOut);
        }
        
        /// <summary>
        /// Hide the UI
        /// </summary>
        public void Hide()
        {
            // Play tween animation
            var tween = CreateTween();
            tween.TweenProperty(_mainPanel, "modulate:a", 0.0, 0.2f);
            tween.TweenProperty(_mainPanel, "scale", new Vector2(0.95f, 0.95f), 0.2f);
            tween.TweenCallback(Callable.From(() => {
                Visible = false;
                _isVisible = false;
            }));
        }
        
        /// <summary>
        /// Refresh display with current event
        /// </summary>
        private void RefreshDisplay()
        {
            var currentEvent = ChoiceEventSystem.Instance.GetCurrentEvent();
            
            if (currentEvent == null || !currentEvent.IsActive)
            {
                _titleLabel.Text = "随机事件";
                _descriptionLabel.Text = "暂无进行中的事件\n\n按 C 键查看统计";
                ClearOptions();
                return;
            }
            
            _titleLabel.Text = currentEvent.Title;
            _descriptionLabel.Text = currentEvent.Description;
            
            // Clear existing options
            ClearOptions();
            
            // Create option buttons
            for (int i = 0; i < currentEvent.Options.Count; i++)
            {
                var option = currentEvent.Options[i];
                var button = CreateOptionButton(option, i);
                _optionsContainer.AddChild(button);
                _optionButtons.Add(button);
            }
        }
        
        /// <summary>
        /// Create an option button
        /// </summary>
        private Button CreateOptionButton(ChoiceOption option, int index)
        {
            var button = new Button();
            button.Name = $"Option_{index}";
            
            // Format button text
            string rarityColor = GetRarityColor(option.Rarity);
            string text = $"{rarityColor}[{option.Name}]\n{option.Description}";
            
            if (option.IsPermanent)
            {
                text += " [永久]";
            }
            
            button.Text = text;
            button.CustomMinimumSize = new Vector2(500, 60);
            button.Pressed += () => OnOptionSelected(index);
            
            // Add hover animation
            button.MouseEntered += () => {
                var tween = CreateTween();
                tween.TweenProperty(button, "scale", new Vector2(1.02f, 1.02f), 0.1f);
            };
            button.MouseExited += () => {
                var tween = CreateTween();
                tween.TweenProperty(button, "scale", Vector2.One, 0.1f);
            };
            
            return button;
        }
        
        /// <summary>
        /// Get color code for rarity
        /// </summary>
        private string GetRarityColor(ChoiceEventRarity rarity)
        {
            return rarity switch
            {
                ChoiceEventRarity.Common => "[color=#ffffff]",
                ChoiceEventRarity.Uncommon => "[color=#1eff00]",
                ChoiceEventRarity.Rare => "[color=#0070dd]",
                ChoiceEventRarity.Epic => "[color=#a335ee]",
                ChoiceEventRarity.Legendary => "[color=#ff8000]",
                _ => "[color=#ffffff]"
            };
        }
        
        /// <summary>
        /// Clear all option buttons
        /// </summary>
        private void ClearOptions()
        {
            foreach (var button in _optionButtons)
            {
                button.QueueFree();
            }
            _optionButtons.Clear();
        }
        
        /// <summary>
        /// Handle option selection
        /// </summary>
        private void OnOptionSelected(int index)
        {
            if (ChoiceEventSystem.Instance.SelectOption(index))
            {
                // Hide after selection
                Hide();
                
                // Show feedback
                GD.Print($"Option {index} selected");
            }
        }
        
        /// <summary>
        /// Update statistics display
        /// </summary>
        private void UpdateStats()
        {
            var stats = ChoiceEventSystem.Instance.GetStatistics();
            if (stats != null)
            {
                _statsLabel.Text = $"总事件: {stats.TotalEvents} | 总选择: {stats.TotalChoices}";
            }
        }
        
        /// <summary>
        /// Handle event started signal
        /// </summary>
        private void OnEventStarted(ChoiceEventType eventType, string title)
        {
            // Auto-show UI when event starts
            Show();
        }
        
        /// <summary>
        /// Handle event ended signal
        /// </summary>
        private void OnEventEnded(ChoiceEventType eventType, string chosenOption)
        {
            UpdateStats();
        }
        
        /// <summary>
        /// Close button pressed
        /// </summary>
        private void OnCloseButtonPressed()
        {
            Hide();
        }
        
        public override void _ExitTree()
        {
            if (ChoiceEventSystem.Instance != null)
            {
                ChoiceEventSystem.Instance.EventStarted -= OnEventStarted;
                ChoiceEventSystem.Instance.EventEnded -= OnEventEnded;
            }
        }
    }
}
