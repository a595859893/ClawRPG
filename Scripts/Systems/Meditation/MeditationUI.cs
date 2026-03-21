using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Meditation
{
    /// <summary>
    /// Meditation UI - Interface for meditation system
    /// </summary>
    public class MeditationUI : Control
    {
        // UI Components
        private PanelContainer _mainPanel;
        private VBoxContainer _contentBox;
        private Label _titleLabel;
        private ProgressBar _focusProgress;
        private Label _focusLabel;
        private GridContainer _meditationGrid;
        private Label _statusLabel;
        private Button _closeButton;
        
        // Meditation buttons
        private Dictionary<MeditationType, Button> _meditationButtons = new Dictionary<MeditationType, Button>();
        
        // Current session timer
        private float _sessionTimer = 0f;
        private MeditationSession _currentSession;
        private bool _isMeditating = false;
        
        public override void _Ready()
        {
            SetupUI();
            ConnectSignals();
            RefreshUI();
        }
        
        private void SetupUI()
        {
            // Main panel
            _mainPanel = new PanelContainer();
            _mainPanel.Name = "MeditationPanel";
            _mainPanel.AnchorLeft = 0.5f;
            _mainPanel.AnchorTop = 0.5f;
            _mainPanel.AnchorRight = 0.5f;
            _mainPanel.AnchorBottom = 0.5f;
            _mainPanel.OffsetLeft = -300;
            _mainPanel.OffsetTop = -250;
            _mainPanel.OffsetRight = 300;
            _mainPanel.OffsetBottom = 250;
            _mainPanel.RectMinSize = new Vector2(600, 500);
            AddChild(_mainPanel);
            
            // Style
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.5f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            _mainPanel.AddStyleboxOverride("panel", style);
            
            // Content box
            _contentBox = new VBoxContainer();
            _contentBox.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
            _contentBox.MarginLeft = 15;
            _contentBox.MarginTop = 15;
            _contentBox.MarginRight = -15;
            _contentBox.MarginBottom = -15;
            _mainPanel.AddChild(_contentBox);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "🧘 Meditation";
            _titleLabel.Align = Label.AlignEnum.Center;
            _titleLabel.AnchorRight = 1f;
            _titleLabel.RectMinSize = new Vector2(0, 40);
            _contentBox.AddChild(_titleLabel);
            
            // Focus progress
            var focusContainer = new HBoxContainer();
            focusContainer.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
            focusContainer.MarginBottom = 5;
            _contentBox.AddChild(focusContainer);
            
            var focusLabelTitle = new Label();
            focusLabelTitle.Text = "Focus: ";
            focusLabelTitle.RectMinSize = new Vector2(80, 0);
            focusContainer.AddChild(focusLabelTitle);
            
            _focusProgress = new ProgressBar();
            _focusProgress.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _focusProgress.MinValue = 0;
            _focusProgress.MaxValue = 100;
            _focusProgress.ShowPercentage = false;
            _focusProgress.RectMinSize = new Vector2(200, 20);
            focusContainer.AddChild(_focusProgress);
            
            _focusLabel = new Label();
            _focusLabel.Text = "0/100";
            _focusLabel.RectMinSize = new Vector2(80, 0);
            focusContainer.AddChild(_focusLabel);
            
            // Meditation type grid
            _meditationGrid = new GridContainer();
            _meditationGrid.Columns = 2;
            _meditationGrid.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _contentBox.AddChild(_meditationGrid);
            
            // Create meditation buttons
            CreateMeditationButtons();
            
            // Status label
            _statusLabel = new Label();
            _statusLabel.Align = Label.AlignEnum.Center;
            _statusLabel.Text = "Select a meditation type to begin";
            _statusLabel.RectMinSize = new Vector2(0, 60);
            _contentBox.AddChild(_statusLabel);
            
            // Close button
            _closeButton = new Button();
            _closeButton.Text = "Close";
            _closeButton.RectMinSize = new Vector2(120, 40);
            _contentBox.AddChild(_closeButton);
        }
        
        private void CreateMeditationButtons()
        {
            foreach (MeditationType type in Enum.GetValues(typeof(MeditationType)))
            {
                var button = new Button();
                button.Name = type.ToString() + "Button";
                button.RectMinSize = new Vector2(250, 60);
                
                var config = MeditationDatabase.Instance.GetTypeConfig(type);
                if (config != null)
                {
                    button.Text = $"{config.DisplayName}\n{config.Description}";
                    button.HintTooltip = $"Duration: {config.MinDuration}-{config.MaxDuration}s\nCooldown: {config.Cooldown}s";
                }
                
                button.Pressed += () => _OnMeditationButtonPressed(type);
                _meditationButtons[type] = button;
                _meditationGrid.AddChild(button);
            }
        }
        
        private void ConnectSignals()
        {
            _closeButton.Pressed += _OnClosePressed;
            
            if (MeditationSystem.Instance?.signals != null)
            {
                MeditationSystem.Instance.signals.MeditationStarted += _OnMeditationStarted;
                MeditationSystem.Instance.signals.MeditationCompleted += _OnMeditationCompleted;
                MeditationSystem.Instance.signals.BuffApplied += _OnBuffApplied;
                MeditationSystem.Instance.signals.FocusGained += _OnFocusGained;
                MeditationSystem.Instance.signals.AbilityUnlocked += _OnAbilityUnlocked;
            }
        }
        
        private void RefreshUI()
        {
            // Get player ID (would normally come from game state)
            string playerId = "player1"; // Placeholder
            
            var progress = MeditationSystem.Instance.GetProgress(playerId);
            if (progress != null)
            {
                _focusProgress.Value = progress.CurrentFocus;
                _focusProgress.MaxValue = progress.MaxFocus;
                _focusLabel.Text = $"{progress.CurrentFocus}/{progress.MaxFocus}";
            }
            
            // Update button states
            foreach (var kvp in _meditationButtons)
            {
                var type = kvp.Key;
                var button = kvp.Value;
                
                bool unlocked = MeditationSystem.Instance.GetUnlockedTypes(playerId).Contains(type);
                bool onCooldown = MeditationSystem.Instance.IsOnCooldown(playerId, type);
                bool canMeditate = MeditationSystem.Instance.CanMeditate(playerId, type, 60); // Default 60s
                
                button.Disabled = !unlocked || onCooldown || !canMeditate || _isMeditating;
                
                if (!unlocked)
                {
                    var config = MeditationDatabase.Instance.GetTypeConfig(type);
                    if (config != null && MeditationDatabase.Instance.FocusToUnlock.ContainsKey(type.ToString()))
                    {
                        int required = MeditationDatabase.Instance.FocusToUnlock[type.ToString()];
                        button.Text = $"🔒 {config.DisplayName}\nRequires {required} Focus";
                    }
                }
                else if (onCooldown)
                {
                    int remaining = MeditationSystem.Instance.GetCooldownRemaining(playerId, type);
                    button.Text = $"⏳ {type} ({remaining}s)";
                }
            }
        }
        
        private void _OnMeditationButtonPressed(MeditationType type)
        {
            string playerId = "player1"; // Placeholder
            
            int duration = 60; // Default duration
            
            if (MeditationSystem.Instance.CanMeditate(playerId, type, duration))
            {
                MeditationSystem.Instance.StartMeditation(playerId, type, duration);
                _statusLabel.Text = $"Meditating: {type}...";
                _isMeditating = true;
            }
            else
            {
                _statusLabel.Text = "Cannot start meditation. Check cooldown or unlock status.";
            }
        }
        
        private void _OnClosePressed()
        {
            if (_isMeditating)
            {
                string playerId = "player1";
                MeditationSystem.Instance.CancelMeditation(playerId);
            }
            QueueFree();
        }
        
        private void _OnMeditationStarted(string playerId, MeditationType type)
        {
            _statusLabel.Text = $"Started: {type}";
            RefreshUI();
        }
        
        private void _OnMeditationCompleted(string playerId, MeditationType type, List<string> benefits)
        {
            _isMeditating = false;
            string benefitText = benefits.Count > 0 ? string.Join(", ", benefits) : "None";
            _statusLabel.Text = $"Completed {type}!\nBenefits: {benefitText}";
            RefreshUI();
        }
        
        private void _OnBuffApplied(string playerId, MeditationType type, string stat, float value)
        {
            GD.Print($"[MeditationUI] Buff applied: {stat} + {value}");
        }
        
        private void _OnFocusGained(string playerId, int focusAmount)
        {
            RefreshUI();
        }
        
        private void _OnAbilityUnlocked(string playerId, string abilityId)
        {
            _statusLabel.Text = $"New meditation unlocked: {abilityId}!";
            RefreshUI();
        }
        
        public override void _Process(float delta)
        {
            if (_isMeditating && _currentSession != null)
            {
                _sessionTimer += delta;
                
                var session = MeditationSystem.Instance.GetCurrentSession("player1");
                if (session != null)
                {
                    int elapsed = (int)_sessionTimer;
                    int remaining = session.Duration - elapsed;
                    
                    if (remaining <= 0)
                    {
                        // Session complete
                        MeditationSystem.Instance.CompleteMeditation("player1");
                        _sessionTimer = 0;
                    }
                    else
                    {
                        _statusLabel.Text = $"Meditating... {remaining}s remaining";
                    }
                }
            }
        }
        
        public override void _ExitTree()
        {
            if (MeditationSystem.Instance?.signals != null)
            {
                MeditationSystem.Instance.signals.MeditationStarted -= _OnMeditationStarted;
                MeditationSystem.Instance.signals.MeditationCompleted -= _OnMeditationCompleted;
                MeditationSystem.Instance.signals.BuffApplied -= _OnBuffApplied;
                MeditationSystem.Instance.signals.FocusGained -= _OnFocusGained;
                MeditationSystem.Instance.signals.AbilityUnlocked -= _OnAbilityUnlocked;
            }
        }
    }
}
