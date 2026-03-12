using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI;

/// <summary>
/// UI control panel for screen effects
/// </summary>
public partial class ScreenEffectUI : Control
{
    private Control _panel;
    private VBoxContainer _mainBox;
    
    // Effect toggles
    private CheckButton _bloomToggle;
    private CheckButton _vignetteToggle;
    private CheckButton _colorGradingToggle;
    private CheckButton _chromaticToggle;
    private CheckButton _filmGrainToggle;
    
    // Sliders
    private HSlider _bloomIntensitySlider;
    private HSlider _bloomThresholdSlider;
    private HSlider _vignetteIntensitySlider;
    private HSlider _saturationSlider;
    private HSlider _contrastSlider;
    private HSlider _temperatureSlider;
    
    // Labels
    private Label _bloomIntensityLabel;
    private Label _bloomThresholdLabel;
    private Label _vignetteIntensityLabel;
    private Label _saturationLabel;
    private Label _contrastLabel;
    private Label _temperatureLabel;
    
    // Preset buttons
    private OptionButton _presetOption;
    
    // Test buttons
    private Button _testFlashButton;
    private Button _testShakeLightButton;
    private Button _testShakeMediumButton;
    private Button _testShakeHeavyButton;
    
    // Stats label
    private Label _statsLabel;
    
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        Visible = false;
        SetupUI();
    }
    
    private void SetupUI()
    {
        // Create main panel
        _panel = new PanelContainer
        {
            Name = "Panel",
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 400,
            OffsetTop = 100,
            OffsetRight = -400,
            OffsetBottom = -100
        };
        AddChild(_panel);
        
        var scroll = new ScrollContainer
        {
            Name = "Scroll",
            AnchorRight = 1f,
            AnchorBottom = 1f
        };
        _panel.AddChild(scroll);
        
        _mainBox = new VBoxContainer
        {
            Name = "MainBox",
            CustomMinimumSize = new Vector2(400, 0)
        };
        scroll.AddChild(_mainBox);
        
        // Title
        var title = new Label
        {
            Text = "🎬 Screen Effects",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 40)
        };
        _mainBox.AddChild(title);
        
        AddSeparator();
        
        // Effect Toggles Section
        var togglesHeader = new Label { Text = "Effect Toggles" };
        _mainBox.AddChild(togglesHeader);
        
        _bloomToggle = CreateToggle("Bloom", true);
        _mainBox.AddChild(_bloomToggle);
        
        _vignetteToggle = CreateToggle("Vignette", true);
        _mainBox.AddChild(_vignetteToggle);
        
        _colorGradingToggle = CreateToggle("Color Grading", true);
        _mainBox.AddChild(_colorGradingToggle);
        
        _chromaticToggle = CreateToggle("Chromatic Aberration", false);
        _mainBox.AddChild(_chromaticToggle);
        
        _filmGrainToggle = CreateToggle("Film Grain", false);
        _mainBox.AddChild(_filmGrainToggle);
        
        AddSeparator();
        
        // Preset Selection
        var presetHeader = new Label { Text = "Preset" };
        _mainBox.AddChild(presetHeader);
        
        _presetOption = new OptionButton
        {
            Name = "PresetOption",
            CustomMinimumSize = new Vector2(0, 30)
        };
        
        var presets = ScreenEffectDatabase.GetPresetNames();
        foreach (var preset in presets)
        {
            _presetOption.AddItem(preset);
        }
        _presetOption.Selected = 0;
        _presetOption.ItemSelected += OnPresetSelected;
        _mainBox.AddChild(_presetOption);
        
        AddSeparator();
        
        // Bloom Settings
        var bloomHeader = new Label { Text = "Bloom Settings" };
        _mainBox.AddChild(bloomHeader);
        
        _bloomIntensitySlider = CreateSlider(0f, 1f, 0.5f, "Intensity");
        _mainBox.AddChild(_bloomIntensitySlider);
        
        _bloomThresholdSlider = CreateSlider(0f, 1f, 0.8f, "Threshold");
        _mainBox.AddChild(_bloomThresholdSlider);
        
        AddSeparator();
        
        // Vignette Settings
        var vignetteHeader = new Label { Text = "Vignette Settings" };
        _mainBox.AddChild(vignetteHeader);
        
        _vignetteIntensitySlider = CreateSlider(0f, 1f, 0.3f, "Intensity");
        _mainBox.AddChild(_vignetteIntensitySlider);
        
        AddSeparator();
        
        // Color Grading Settings
        var colorHeader = new Label { Text = "Color Grading" };
        _mainBox.AddChild(colorHeader);
        
        _saturationSlider = CreateSlider(0f, 2f, 1f, "Saturation");
        _mainBox.AddChild(_saturationSlider);
        
        _contrastSlider = CreateSlider(0f, 2f, 1f, "Contrast");
        _mainBox.AddChild(_contrastSlider);
        
        _temperatureSlider = CreateSlider(-1f, 1f, 0f, "Temperature");
        _mainBox.AddChild(_temperatureSlider);
        
        AddSeparator();
        
        // Test Buttons
        var testHeader = new Label { Text = "Test Effects" };
        _mainBox.AddChild(testHeader);
        
        var testBox = new HBoxContainer { Name = "TestBox" };
        _mainBox.AddChild(testBox);
        
        _testFlashButton = new Button { Text = "Flash", CustomMinimumSize = new Vector2(80, 30) };
        _testFlashButton.Pressed += OnTestFlash;
        testBox.AddChild(_testFlashButton);
        
        _testShakeLightButton = new Button { Text = "Shake L", CustomMinimumSize = new Vector2(80, 30) };
        _testShakeLightButton.Pressed += OnTestShakeLight;
        testBox.AddChild(_testShakeLightButton);
        
        _testShakeMediumButton = new Button { Text = "Shake M", CustomMinimumSize = new Vector2(80, 30) };
        _testShakeMediumButton.Pressed += OnTestShakeMedium;
        testBox.AddChild(_testShakeMediumButton);
        
        _testShakeHeavyButton = new Button { Text = "Shake H", CustomMinimumSize = new Vector2(80, 30) };
        _testShakeHeavyButton.Pressed += OnTestShakeHeavy;
        testBox.AddChild(_testShakeHeavyButton);
        
        AddSeparator();
        
        // Statistics
        var statsHeader = new Label { Text = "Statistics" };
        _mainBox.AddChild(statsHeader);
        
        _statsLabel = new Label
        {
            Name = "StatsLabel",
            Text = "Loading...",
            CustomMinimumSize = new Vector2(0, 60)
        };
        _mainBox.AddChild(_statsLabel);
        
        AddSeparator();
        
        // Close button
        var closeButton = new Button
        {
            Text = "Close (ESC)",
            CustomMinimumSize = new Vector2(0, 40)
        };
        closeButton.Pressed += OnClose;
        _mainBox.AddChild(closeButton);
        
        // Connect slider signals
        ConnectSliders();
        
        // Update stats
        UpdateStats();
    }
    
    private void AddSeparator()
    {
        var sep = new HSeparator { CustomMinimumSize = new Vector2(0, 10) };
        _mainBox.AddChild(sep);
    }
    
    private CheckButton CreateToggle(string text, bool defaultValue)
    {
        var toggle = new CheckButton
        {
            Text = text,
            ButtonPressed = defaultValue,
            CustomMinimumSize = new Vector2(0, 30)
        };
        toggle.Toggled += OnToggleChanged;
        return toggle;
    }
    
    private HSlider CreateSlider(float min, float max, float value, string labelText)
    {
        var container = new VBoxContainer { Name = labelText + "Container" };
        
        var label = new Label { Text = $"{labelText}: {value:F2}" };
        label.Name = labelText + "Label";
        container.AddChild(label);
        
        var slider = new HSlider
        {
            MinValue = min,
            MaxValue = max,
            Value = value,
            CustomMinimumSize = new Vector2(0, 20)
        };
        slider.ValueChanged += OnSliderChanged;
        container.AddChild(slider);
        
        return slider;
    }
    
    private void ConnectSliders()
    {
        _bloomIntensitySlider.ValueChanged += OnBloomIntensityChanged;
        _bloomThresholdSlider.ValueChanged += OnBloomThresholdChanged;
        _vignetteIntensitySlider.ValueChanged += OnVignetteIntensityChanged;
        _saturationSlider.ValueChanged += OnSaturationChanged;
        _contrastSlider.ValueChanged += OnContrastChanged;
        _temperatureSlider.ValueChanged += OnTemperatureChanged;
        
        _bloomToggle.Toggled += OnBloomToggle;
        _vignetteToggle.Toggled += OnVignetteToggle;
        _colorGradingToggle.Toggled += OnColorGradingToggle;
    }
    
    #region Event Handlers
    
    private void OnToggleChanged(bool toggled)
    {
        UpdateEffectSystem();
    }
    
    private void OnSliderChanged(double value)
    {
        UpdateLabels();
        UpdateEffectSystem();
    }
    
    private void OnPresetSelected(int index)
    {
        var presetName = _presetOption.GetItemText(index);
        var preset = ScreenEffectDatabase.GetPreset(presetName);
        
        if (ScreenEffectSystem.Instance != null)
        {
            ScreenEffectSystem.Instance.ApplyPreset(preset);
        }
        
        // Update sliders to match preset
        var data = ScreenEffectSystem.Instance?.Data;
        if (data != null)
        {
            _bloomIntensitySlider.Value = data.BloomIntensity;
            _bloomThresholdSlider.Value = data.BloomThreshold;
            _vignetteIntensitySlider.Value = data.VignetteIntensity;
            _saturationSlider.Value = data.Saturation;
            _contrastSlider.Value = data.Contrast;
            _temperatureSlider.Value = data.Temperature;
        }
        
        UpdateLabels();
    }
    
    private void OnBloomToggle(bool toggled)
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.SetBloomEnabled(toggled);
    }
    
    private void OnVignetteToggle(bool toggled)
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.Data.VignetteEnabled = toggled;
    }
    
    private void OnColorGradingToggle(bool toggled)
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.Data.ColorGradingEnabled = toggled;
    }
    
    private void OnBloomIntensityChanged(double value)
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.SetBloomIntensity((float)value);
        UpdateLabels();
    }
    
    private void OnBloomThresholdChanged(double value)
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.SetBloomThreshold((float)value);
        UpdateLabels();
    }
    
    private void OnVignetteIntensityChanged(double value)
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.Data.VignetteIntensity = (float)value;
        UpdateLabels();
    }
    
    private void OnSaturationChanged(double value)
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.Data.Saturation = (float)value;
        UpdateLabels();
    }
    
    private void OnContrastChanged(double value)
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.Data.Contrast = (float)value;
        UpdateLabels();
    }
    
    private void OnTemperatureChanged(double value)
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.Data.Temperature = (float)value;
        UpdateLabels();
    }
    
    private void OnTestFlash()
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.TriggerFlash(Colors.White, 0.8f, 0.3f);
    }
    
    private void OnTestShakeLight()
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.TriggerLightShake();
    }
    
    private void OnTestShakeMedium()
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.TriggerMediumShake();
    }
    
    private void OnTestShakeHeavy()
    {
        if (ScreenEffectSystem.Instance != null)
            ScreenEffectSystem.Instance.TriggerHeavyShake();
    }
    
    private void OnClose()
    {
        ToggleVisibility(false);
    }
    
    #endregion
    
    private void UpdateLabels()
    {
        if (_bloomIntensityLabel != null)
            _bloomIntensityLabel.Text = $"Intensity: {_bloomIntensitySlider.Value:F2}";
        if (_bloomThresholdLabel != null)
            _bloomThresholdLabel.Text = $"Threshold: {_bloomThresholdSlider.Value:F2}";
        if (_vignetteIntensityLabel != null)
            _vignetteIntensityLabel.Text = $"Intensity: {_vignetteIntensitySlider.Value:F2}";
        if (_saturationLabel != null)
            _saturationLabel.Text = $"Saturation: {_saturationSlider.Value:F2}";
        if (_contrastLabel != null)
            _contrastLabel.Text = $"Contrast: {_contrastSlider.Value:F2}";
        if (_temperatureLabel != null)
            _temperatureLabel.Text = $"Temperature: {_temperatureSlider.Value:F2}";
    }
    
    private void UpdateEffectSystem()
    {
        if (ScreenEffectSystem.Instance == null) return;
        
        var data = ScreenEffectSystem.Instance.Data;
        data.EnabledEffects["Bloom"] = _bloomToggle.ButtonPressed;
        data.EnabledEffects["Vignette"] = _vignetteToggle.ButtonPressed;
        data.EnabledEffects["ColorGrading"] = _colorGradingToggle.ButtonPressed;
        data.EnabledEffects["ChromaticAberration"] = _chromaticToggle.ButtonPressed;
        data.EnabledEffects["FilmGrain"] = _filmGrainToggle.ButtonPressed;
        
        data.BloomEnabled = _bloomToggle.ButtonPressed;
        data.VignetteEnabled = _vignetteToggle.ButtonPressed;
        data.ColorGradingEnabled = _colorGradingToggle.ButtonPressed;
    }
    
    private void UpdateStats()
    {
        if (ScreenEffectSystem.Instance == null)
        {
            _statsLabel.Text = "Screen Effect System not found";
            return;
        }
        
        var stats = ScreenEffectSystem.Instance.GetStatistics();
        _statsLabel.Text = $"Total Flashes: {stats["TotalFlashes"]}\n" +
                          $"Total Shakes: {stats["TotalShakes"]}\n" +
                          $"Avg Shake: {stats["AverageShakeIntensity"]:F2}";
    }
    
    public void ToggleVisibility(bool? force = null)
    {
        _isVisible = force ?? !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            UpdateStats();
            
            // Sync sliders with current data
            var data = ScreenEffectSystem.Instance?.Data;
            if (data != null)
            {
                _bloomIntensitySlider.Value = data.BloomIntensity;
                _bloomThresholdSlider.Value = data.BloomThreshold;
                _vignetteIntensitySlider.Value = data.VignetteIntensity;
                _saturationSlider.Value = data.Saturation;
                _contrastSlider.Value = data.Contrast;
                _temperatureSlider.Value = data.Temperature;
                
                _bloomToggle.ButtonPressed = data.BloomEnabled;
                _vignetteToggle.ButtonPressed = data.VignetteEnabled;
                _colorGradingToggle.ButtonPressed = data.ColorGradingEnabled;
            }
            
            UpdateLabels();
        }
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt.IsActionPressed("ui_cancel"))
        {
            if (Visible)
            {
                ToggleVisibility(false);
                GetTree().SetInputAsHandled();
            }
        }
        
        // Toggle with E key (conflict with Elemental Reaction)
        // Using Ctrl+Shift+E instead
        if (evt.IsActionPressed("ui_accept"))
        {
            // Reserved for future shortcuts
        }
    }
}
