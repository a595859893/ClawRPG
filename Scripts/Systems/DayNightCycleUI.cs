using Godot;
using System;
using Godot.Collections;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Day/Night Cycle UI - Displays current time and phase information
    /// </summary>
    public class DayNightCycleUI : Control {
        private DayNightCycleSystem _dayNightSystem;
        
        // UI Elements
        private Label _timeLabel;
        private Label _phaseLabel;
        private ProgressBar _timeProgressBar;
        private Label _bonusLabel;
        private Button _speedButton;
        private PanelContainer _mainPanel;
        
        // State
        private bool _isVisible = false;
        private float[] _speedOptions = { 0f, 1f, 2f, 5f, 10f };
        private int _currentSpeedIndex = 1;
        
        public override void _Ready() {
            base._Ready();
            SetupUI();
            SetupKeybinds();
        }
        
        private void SetupUI() {
            // Main panel
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _mainPanel.Position = new Vector2(-220, 20);
            _mainPanel.CustomMinimumSize = new Vector2(200, 180);
            AddChild(_mainPanel);
            
            var mainVBox = new VBoxContainer();
            mainVBox.AddThemeConstantOverride("separation", 8);
            _mainPanel.AddChild(mainVBox);
            
            // Title
            var titleLabel = new Label();
            titleLabel.Text = "☀️ 日夜循环";
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.AddThemeFontSizeOverride("font_size", 16);
            mainVBox.AddChild(titleLabel);
            
            // Separator
            var hsep1 = new HSeparator();
            mainVBox.AddChild(hsep1);
            
            // Time display
            _timeLabel = new Label();
            _timeLabel.Text = "12:00";
            _timeLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _timeLabel.AddThemeFontSizeOverride("font_size", 28);
            mainVBox.AddChild(_timeLabel);
            
            // Phase label
            _phaseLabel = new Label();
            _phaseLabel.Text = "白天";
            _phaseLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _phaseLabel.AddThemeFontSizeOverride("font_size", 14);
            mainVBox.AddChild(_phaseLabel);
            
            // Time progress bar
            _timeProgressBar = new ProgressBar();
            _timeProgressBar.CustomMinimumSize = new Vector2(180, 20);
            _timeProgressBar.MinValue = 0;
            _timeProgressBar.MaxValue = 24;
            _timeProgressBar.ShowPercentage = false;
            mainVBox.AddChild(_timeProgressBar);
            
            // Separator
            var hsep2 = new HSeparator();
            mainVBox.AddChild(hsep2);
            
            // Bonus label
            _bonusLabel = new Label();
            _bonusLabel.Text = "经验: 1.0x\n金币: 1.0x\n掉落: 1.0x";
            _bonusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _bonusLabel.AddThemeFontSizeOverride("font_size", 12);
            mainVBox.AddChild(_bonusLabel);
            
            // Speed control button
            _speedButton = new Button();
            _speedButton.Text = "速度: 1x [N]";
            _speedButton.Pressed += OnSpeedButtonPressed;
            mainVBox.AddChild(_speedButton);
            
            // Initially hidden
            _mainPanel.Visible = false;
        }
        
        private void SetupKeybinds() {
            // N key to toggle
        }
        
        public void Initialize(DayNightCycleSystem system) {
            _dayNightSystem = system;
            _dayNightSystem.OnTimeUpdated += OnTimeUpdated;
            _dayNightSystem.OnPhaseChanged += OnPhaseChanged;
            UpdateDisplay();
        }
        
        private void OnTimeUpdated(float time) {
            UpdateDisplay();
        }
        
        private void OnPhaseChanged(DayNightCycleSystem.TimePhase phase) {
            UpdateDisplay();
        }
        
        private void UpdateDisplay() {
            if (_dayNightSystem == null) return;
            
            _timeLabel.Text = _dayNightSystem.GetTimeString();
            _phaseLabel.Text = _dayNightSystem.GetPhaseName();
            _timeProgressBar.Value = _dayNightSystem.GetCurrentTime();
            
            // Update bonus display
            string bonusText = $"经验: {_dayNightSystem.ExperienceMultiplier:F1f}x\n" +
                             $"金币: {_dayNightSystem.GoldMultiplier:F1f}x\n" +
                             $"掉落: {_dayNightSystem.DropRateMultiplier:F1f}x";
            _bonusLabel.Text = bonusText;
            
            // Update phase color based on current phase
            UpdatePhaseColors(_dayNightSystem.GetCurrentPhase());
        }
        
        private void UpdatePhaseColors(DayNightCycleSystem.TimePhase phase) {
            Color phaseColor = phase switch {
                DayNightCycleSystem.TimePhase.Dawn => new Color(1f, 0.8f, 0.6f),
                DayNightCycleSystem.TimePhase.Day => new Color(1f, 1f, 0.8f),
                DayNightCycleSystem.TimePhase.Dusk => new Color(1f, 0.6f, 0.4f),
                DayNightCycleSystem.TimePhase.Night => new Color(0.5f, 0.5f, 0.7f),
                _ => Colors.White
            };
            
            _phaseLabel.Modulate = phaseColor;
        }
        
        public void Toggle() {
            _isVisible = !_isVisible;
            _mainPanel.Visible = _isVisible;
        }
        
        public void Show() {
            _isVisible = true;
            _mainPanel.Visible = true;
        }
        
        public void Hide() {
            _isVisible = false;
            _mainPanel.Visible = false;
        }
        
        private void OnSpeedButtonPressed() {
            _currentSpeedIndex = (_currentSpeedIndex + 1) % _speedOptions.Length;
            float speed = _speedOptions[_currentSpeedIndex];
            
            if (_dayNightSystem != null) {
                _dayNightSystem.SetTimeScale(speed);
            }
            
            string speedText = speed == 0 ? "暂停" : $"{speed:F0}x";
            _speedButton.Text = $"速度: {speedText} [N]";
        }
        
        public override void _Input(InputEvent @event) {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
                // N key to toggle time UI
                if (keyEvent.Keycode == Key.N) {
                    Toggle();
                }
                // Escape to close
                else if (keyEvent.Keycode == Key.Escape && _isVisible) {
                    Hide();
                }
            }
        }
        
        public override void _ExitTree() {
            base._ExitTree();
            if (_dayNightSystem != null) {
                _dayNightSystem.OnTimeUpdated -= OnTimeUpdated;
                _dayNightSystem.OnPhaseChanged -= OnPhaseChanged;
            }
        }
    }
}
