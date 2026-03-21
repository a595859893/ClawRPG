using Godot;
using System;
using System.Collections.Generic;

public class WeatherUI : Control
{
    private Label title_label;
    private Label weather_type_label;
    private Label intensity_label;
    private Label time_label;
    private Label effects_label;
    private Label stats_label;
    private VBoxContainer weather_list;
    private Button close_button;
    private CheckButton enable_check;
    private HBoxContainer weather_buttons;
    
    private Color[] rarity_colors = {
        new Color(0.7f, 0.7f, 0.7f),  // Light - Gray
        new Color(0.4f, 0.8f, 0.4f),  // Moderate - Green
        new Color(0.4f, 0.6f, 1.0f),  // Heavy - Blue
        new Color(1.0f, 0.4f, 0.4f)   // Severe - Red
    };
    
    // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
    public event Action<WeatherType, WeatherType> OnWeatherChangedUI;
    public event Action<WeatherIntensity> OnIntensityChangedUI;
    
    public override void _Ready()
    {
        CreateUI();
        ConnectSignals();
        RefreshDisplay();
    }
    
    private void CreateUI()
    {
        // Main container
        var main_container = new VBoxContainer();
        main_container.SetAnchorsPreset(Control.LayoutPreset.Center);
        main_container.CustomMinimumSize = new Vector2(500, 600);
        main_container.Position = new Vector2(-250, -300);
        AddChild(main_container);
        
        // Title
        title_label = new Label();
        title_label.Text = "  🌤️  天气系统  🌤️";
        title_label.HorizontalAlignment = HorizontalAlignment.Center;
        title_label.AddThemeFontSizeOverride("font_size", 24);
        main_container.AddChild(title_label);
        
        // Enable check
        enable_check = new CheckButton();
        enable_check.Text = "启用天气效果";
        enable_check.ButtonPressed = WeatherSystem.Instance.IsWeatherEnabled();
        main_container.AddChild(enable_check);
        
        // Current weather display
        var current_frame = new PanelContainer();
        current_frame.CustomMinimumSize = new Vector2(480, 150);
        main_container.AddChild(current_frame);
        
        var current_vbox = new VBoxContainer();
        current_frame.AddChild(current_vbox);
        
        var current_title = new Label();
        current_title.Text = "当前天气";
        current_title.AddThemeFontSizeOverride("font_size", 18);
        current_vbox.AddChild(current_title);
        
        weather_type_label = new Label();
        weather_type_label.Text = "类型: -";
        current_vbox.AddChild(weather_type_label);
        
        intensity_label = new Label();
        intensity_label.Text = "强度: -";
        current_vbox.AddChild(intensity_label);
        
        time_label = new Label();
        time_label.Text = "剩余时间: -";
        current_vbox.AddChild(time_label);
        
        effects_label = new Label();
        effects_label.Text = "效果: -";
        current_vbox.AddChild(effects_label);
        
        // Quick change buttons
        var button_label = new Label();
        button_label.Text = "切换天气:";
        main_container.AddChild(button_label);
        
        weather_buttons = new HBoxContainer();
        weather_buttons.Alignment = BoxContainer.AlignMode.Center;
        main_container.AddChild(weather_buttons);
        
        string[] weather_names = {"晴朗", "多云", "雨", "雪", "雷暴", "雾", "沙尘", "冰雹", "暴风雪", "风暴"};
        for (int i = 0; i < 10; i++)
        {
            var btn = new Button();
            btn.Text = weather_names[i];
            btn.CustomMinimumSize = new Vector2(45, 30);
            btn.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
            weather_buttons.AddChild(btn);
            
            int weather_index = i;
            btn.Pressed += () => OnWeatherButtonPressed(weather_index);
        }
        
        // Statistics
        var stats_frame = new PanelContainer();
        stats_frame.CustomMinimumSize = new Vector2(480, 150);
        main_container.AddChild(stats_frame);
        
        var stats_vbox = new VBoxContainer();
        stats_frame.AddChild(stats_vbox);
        
        var stats_title = new Label();
        stats_title.Text = "天气统计";
        stats_title.AddThemeFontSizeOverride("font_size", 18);
        stats_vbox.AddChild(stats_title);
        
        stats_label = new Label();
        stats_label.Text = "加载中...";
        stats_vbox.AddChild(stats_label);
        
        // Close button
        close_button = new Button();
        close_button.Text = "关闭 (ESC)";
        close_button.CustomMinimumSize = new Vector2(200, 40);
        main_container.AddChild(close_button);
    }
    
    private void ConnectSignals()
    {
        enable_check.Toggled += OnEnableToggled;
        close_button.Pressed += OnClosePressed;
        
        if (WeatherSystem.Instance != null)
        {
            // REQ-058-11: migrated from Godot 3 .Connect() to C# event +=
            WeatherSystem.Instance.WeatherChanged += OnWeatherChanged; // NEW
            WeatherSystem.Instance.Connect(nameof(WeatherSystem.WeatherChanged), this, nameof(OnWeatherChanged)); // TODO: Remove after migration
            WeatherSystem.Instance.WeatherIntensityChanged += OnIntensityChanged; // NEW
            WeatherSystem.Instance.Connect(nameof(WeatherSystem.WeatherIntensityChanged), this, nameof(OnIntensityChanged)); // TODO: Remove after migration
        }
    }
    
    private void RefreshDisplay()
    {
        if (WeatherSystem.Instance == null) return;
        
        // Update current weather
        WeatherType current_type = WeatherSystem.Instance.GetCurrentWeatherType();
        WeatherIntensity current_intensity = WeatherSystem.Instance.GetCurrentIntensity();
        
        weather_type_label.Text = $"类型: {GetWeatherName(current_type)}";
        intensity_label.Text = $"强度: {GetIntensityName(current_intensity)}";
        
        float remaining = WeatherSystem.Instance.GetRemainingTime();
        int minutes = (int)(remaining / 60);
        int seconds = (int)(remaining % 60);
        time_label.Text = $"剩余时间: {minutes}:{seconds:D2}";
        
        // Update effects
        float vis = WeatherSystem.Instance.GetVisibilityReduction();
        float move = WeatherSystem.Instance.GetMovementSpeedModifier();
        float atk = WeatherSystem.Instance.GetAttackSpeedModifier();
        float def = WeatherSystem.Instance.GetDefenseModifier();
        float drop = WeatherSystem.Instance.GetDropRateModifier();
        
        effects_label.Text = $"效果:\n" +
            $"  视野: {(vis * 100):F0}%\n" +
            $"  移速: {(move * 100):F0}%\n" +
            $"  攻击: {(atk * 100):F0}%\n" +
            $"  防御: {(def * 100):F0}%\n" +
            $"  掉落: {(drop * 100):F0}%";
        
        // Update stats
        var stats = WeatherSystem.Instance.GetStatistics();
        stats_label.Text = $"总天气变化: {stats["total_changes"]}\n" +
            $"解锁天气: {stats["unlocked_types"]}\n" +
            $"最喜爱天气: {stats["favorite_weather"]} ({stats["favorite_time"]:F1}秒)";
    }
    
    public override void _Process(float delta)
    {
        RefreshDisplay();
    }
    
    private void OnWeatherButtonPressed(int index)
    {
        if (WeatherSystem.Instance == null) return;
        
        WeatherType[] types = (WeatherType[])Enum.GetValues(typeof(WeatherType));
        if (index < types.Length)
        {
            WeatherIntensity[] intensities = (WeatherIntensity[])Enum.GetValues(typeof(WeatherIntensity));
            WeatherIntensity random_intensity = intensities[GD.Randi() % intensities.Length];
            
            WeatherSystem.Instance.ForceWeatherChange(types[index], random_intensity, 120);
        }
    }
    
    private void OnEnableToggled(bool pressed)
    {
        if (WeatherSystem.Instance != null)
        {
            WeatherSystem.Instance.SetWeatherEnabled(pressed);
        }
    }
    
    private void OnClosePressed()
    {
        Visible = false;
    }
    
    private void OnWeatherChanged(WeatherType old_type, WeatherType new_type)
    {
        // REQ-058-11: Invoke new event
        OnWeatherChangedUI?.Invoke(old_type, new_type);
        RefreshDisplay();
    }
    
    private void OnIntensityChanged(WeatherIntensity intensity)
    {
        // REQ-058-11: Invoke new event
        OnIntensityChangedUI?.Invoke(intensity);
        RefreshDisplay();
    }
    
    private string GetWeatherName(WeatherType type)
    {
        switch (type)
        {
            case WeatherType.Clear: return "晴朗";
            case WeatherType.Cloudy: return "多云";
            case WeatherType.Rain: return "雨";
            case WeatherType.Snow: return "雪";
            case WeatherType.Thunderstorm: return "雷暴";
            case WeatherType.Fog: return "雾";
            case WeatherType.Sandstorm: return "沙尘暴";
            case WeatherType.Hail: return "冰雹";
            case WeatherType.Blizzard: return "暴风雪";
            case WeatherType.Storm: return "风暴";
            default: return "未知";
        }
    }
    
    private string GetIntensityName(WeatherIntensity intensity)
    {
        switch (intensity)
        {
            case WeatherIntensity.Light: return "轻微";
            case WeatherIntensity.Moderate: return "中等";
            case WeatherIntensity.Heavy: return "强烈";
            case WeatherIntensity.Severe: return "严重";
            default: return "未知";
        }
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey key_event && key_event.Pressed && key_event.Keycode == Key.Escape)
        {
            Visible = false;
        }
    }
}
