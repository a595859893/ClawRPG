using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Combat
{
    public partial class DamageNumberUI : Control
    {
        private DamageNumberSystem _system;
        
        private Label _titleLabel;
        private CheckBox _enableCheck;
        private VBoxContainer _settingsContainer;
        private Label _statsLabel;
        
        // Settings controls
        private HSlider _fontSizeSlider;
        private HSlider _criticalFontSizeSlider;
        private HSlider _floatSpeedSlider;
        private HSlider _floatDurationSlider;
        private HSlider _fadeStartSlider;
        private CheckBox _shakeCheck;
        private HSlider _shakeAmountSlider;
        
        private bool _isVisible = false;

        public override void _Ready()
        {
            _system = GetNode<DamageNumberSystem>("/root/Main/DamageNumberSystem");
            
            SetupUI();
            
            // Connect input
            Input.ActionPressed += OnActionPressed;
            
            // Hide by default
            Hide();
        }

        private void SetupUI()
        {
            // Main panel
            var panel = new PanelContainer();
            panel.SetAnchorsPreset(Control.LayoutPreset.Center);
            AddChild(panel);
            
            var mainVBox = new VBoxContainer();
            mainVBox.Size = new Vector2(400, 500);
            panel.AddChild(mainVBox);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "🎯 Damage Number Settings";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            mainVBox.AddChild(_titleLabel);
            
            // Enable check
            _enableCheck = new CheckBox();
            _enableCheck.Text = "Enable Damage Numbers";
            _enableCheck.ButtonPressed = true;
            _enableCheck.Toggled += OnEnableToggled;
            mainVBox.AddChild(_enableCheck);
            
            // Settings container
            _settingsContainer = new VBoxContainer();
            mainVBox.AddChild(_settingsContainer);
            
            // Font Size
            _settingsContainer.AddChild(CreateSliderRow("Font Size", 12, 48, 24, out _fontSizeSlider, OnFontSizeChanged));
            
            // Critical Font Size
            _settingsContainer.AddChild(CreateSliderRow("Critical Size", 18, 64, 36, out _criticalFontSizeSlider, OnCriticalFontSizeChanged));
            
            // Float Speed
            _settingsContainer.AddChild(CreateSliderRow("Float Speed", 20, 150, 50, out _floatSpeedSlider, OnFloatSpeedChanged));
            
            // Float Duration
            _settingsContainer.AddChild(CreateSliderRow("Float Duration", 0.5f, 3.0f, 1.0f, out _floatDurationSlider, OnFloatDurationChanged));
            
            // Fade Start
            _settingsContainer.AddChild(CreateSliderRow("Fade Start", 0.3f, 1.5f, 0.7f, out _fadeStartSlider, OnFadeStartChanged));
            
            // Shake Enable
            _shakeCheck = new CheckBox();
            _shakeCheck.Text = "Enable Shake";
            _shakeCheck.ButtonPressed = true;
            _shakeCheck.Toggled += OnShakeToggled;
            _settingsContainer.AddChild(_shakeCheck);
            
            // Shake Amount
            _settingsContainer.AddChild(CreateSliderRow("Shake Amount", 1, 10, 3, out _shakeAmountSlider, OnShakeAmountChanged));
            
            // Test buttons
            var testHBox = new HBoxContainer();
            mainVBox.AddChild(testHBox);
            
            var testDamageBtn = new Button();
            testDamageBtn.Text = "Test Damage";
            testDamageBtn.Pressed += () => _system?.ShowDamage(GetViewport().GetMousePosition(), Random.Shared.Next(50, 200), false, false, false);
            testHBox.AddChild(testDamageBtn);
            
            var testCritBtn = new Button();
            testCritBtn.Text = "Test Crit";
            testCritBtn.Pressed += () => _system?.ShowDamage(GetViewport().GetMousePosition(), Random.Shared.Next(100, 500), true, false, false);
            testHBox.AddChild(testCritBtn);
            
            var testHealBtn = new Button();
            testHealBtn.Text = "Test Heal";
            testHealBtn.Pressed += () => _system?.ShowDamage(GetViewport().GetMousePosition(), Random.Shared.Next(30, 100), false, true, false);
            testHBox.AddChild(testHealBtn);
            
            var testMissBtn = new Button();
            testMissBtn.Text = "Test Miss";
            testMissBtn.Pressed += () => _system?.ShowDamage(GetViewport().GetMousePosition(), 0, false, false, true);
            testHBox.AddChild(testMissBtn);
            
            // Statistics
            var statsTitle = new Label();
            statsTitle.Text = "📊 Statistics";
            mainVBox.AddChild(statsTitle);
            
            _statsLabel = new Label();
            _statsLabel.Text = "No data yet";
            mainVBox.AddChild(_statsLabel);
            
            // Close button
            var closeBtn = new Button();
            closeBtn.Text = "Close (ESC)";
            closeBtn.Pressed += () => ToggleVisibility(false);
            mainVBox.AddChild(closeBtn);
        }

        private HBoxContainer CreateSliderRow(string labelText, float min, float max, float defaultValue, out HSlider slider, Action<float> onChanged)
        {
            var hbox = new HBoxContainer();
            
            var label = new Label();
            label.Text = labelText;
            label.CustomMinimumSize = new Vector2(120, 0);
            hbox.AddChild(label);
            
            slider = new HSlider();
            slider.MinValue = min;
            slider.MaxValue = max;
            slider.Value = defaultValue;
            slider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            slider.ValueChanged += (value) => onChanged?.Invoke(value);
            hbox.AddChild(slider);
            
            var valueLabel = new Label();
            valueLabel.Text = defaultValue.ToString("F1");
            valueLabel.CustomMinimumSize = new Vector2(40, 0);
            slider.ValueChanged += (value) => valueLabel.Text = value.ToString("F1");
            hbox.AddChild(valueLabel);
            
            return hbox;
        }

        private void OnEnableToggled(bool toggledOn)
        {
            _settingsContainer.Visible = toggledOn;
            if (_system?.Data != null)
            {
                _system.Data.FontSize = _fontSizeSlider?.Value ?? 24f;
            }
        }

        private void OnFontSizeChanged(float value)
        {
            if (_system?.Data != null)
            {
                _system.Data.FontSize = value;
            }
        }

        private void OnCriticalFontSizeChanged(float value)
        {
            if (_system?.Data != null)
            {
                _system.Data.CriticalFontSize = value;
            }
        }

        private void OnFloatSpeedChanged(float value)
        {
            if (_system?.Data != null)
            {
                _system.Data.FloatSpeed = value;
            }
        }

        private void OnFloatDurationChanged(float value)
        {
            if (_system?.Data != null)
            {
                _system.Data.FloatDuration = value;
            }
        }

        private void OnFadeStartChanged(float value)
        {
            if (_system?.Data != null)
            {
                _system.Data.FadeStartTime = value;
            }
        }

        private void OnShakeToggled(bool toggledOn)
        {
            if (_system?.Data != null)
            {
                _system.Data.EnableShake = toggledOn;
            }
            _shakeAmountSlider.Editable = toggledOn;
        }

        private void OnShakeAmountChanged(float value)
        {
            if (_system?.Data != null)
            {
                _system.Data.ShakeAmount = value;
            }
        }

        private void OnActionPressed(StringName action)
        {
            if (action == "ui_accept" || action == "damage_number_toggle")
            {
                ToggleVisibility(!_isVisible);
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Escape && _isVisible)
                {
                    ToggleVisibility(false);
                }
                else if (keyEvent.Keycode == Key.D && Input.IsKeyPressed(Key.Ctrl))
                {
                    ToggleVisibility(!_isVisible);
                }
            }
        }

        public void ToggleVisibility(bool show)
        {
            _isVisible = show;
            
            if (show)
            {
                UpdateStatistics();
                Show();
            }
            else
            {
                Hide();
            }
        }

        private void UpdateStatistics()
        {
            if (_system == null)
            {
                _statsLabel.Text = "System not found";
                return;
            }
            
            var stats = _system.GetStatistics();
            _statsLabel.Text = $"Total Numbers: {stats["totalDamageNumbers"]}\n" +
                              $"Critical Hits: {stats["criticalHits"]}\n" +
                              $"Total Damage: {stats["totalDamage"]}\n" +
                              $"Total Healing: {stats["totalHealing"]}\n" +
                              $"Active: {stats["activeCount"]}";
        }

        public override void _Process(double delta)
        {
            if (_isVisible && _system != null)
            {
                UpdateStatistics();
            }
        }
    }
}
