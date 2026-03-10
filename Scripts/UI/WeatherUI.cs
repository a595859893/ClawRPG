using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 天气显示UI组件
    /// </summary>
    public partial class WeatherUI : Control
    {
        private Label _weatherIconLabel;
        private Label _weatherNameLabel;
        private Label _durationLabel;
        private Label _multiplierLabel;
        private ProgressBar _durationProgressBar;
        private VBoxContainer _effectsContainer;
        private Button _closeButton;
        private Button _changeWeatherButton;
        
        private WeatherSystem _weatherSystem;
        private bool _isVisible = false;
        
        // 效果显示标签
        private Label _damageEffect;
        private Label _defenseEffect;
        private Label _expEffect;
        private Label _dropEffect;
        private Label _visibilityEffect;

        public override void _Ready()
        {
            _weatherSystem = WeatherSystem.Instance;
            
            // 连接天气信号
            _weatherSystem.WeatherChanged += OnWeatherChanged;
            _weatherSystem.WeatherUpdated += OnWeatherUpdated;
            
            SetupUI();
            Visible = false;
        }

        private void SetupUI()
        {
            // 主容器
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            mainContainer.Position = new Vector2(-220, 60);
            mainContainer.CustomMinimumSize = new Vector2(200, 0);
            AddChild(mainContainer);

            // 标题栏
            var titleBar = new HBoxContainer();
            mainContainer.AddChild(titleBar);

            var titleLabel = new Label();
            titleLabel.Text = "  天气状况";
            titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
            titleLabel.AddThemeFontSizeOverride("font_size", 18);
            titleBar.AddChild(titleLabel);

            titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            _closeButton = new Button();
            _closeButton.Text = "×";
            _closeButton.CustomMinimumSize = new Vector2(30, 30);
            _closeButton.Pressed += () => ToggleVisibility();
            titleBar.AddChild(_closeButton);

            // 天气显示面板
            var panel = new PanelContainer();
            panel.Modulate = new Color(1, 1, 1, 0.95f);
            mainContainer.AddChild(panel);

            var contentVBox = new VBoxContainer();
            contentVBox.AddThemeConstantOverride("separation", 10);
            panel.AddChild(contentVBox);

            // 天气图标和名称
            var weatherHeader = new HBoxContainer();
            contentVBox.AddChild(weatherHeader);

            _weatherIconLabel = new Label();
            _weatherIconLabel.Text = "☀️";
            _weatherIconLabel.AddThemeFontSizeOverride("font_size", 32);
            weatherHeader.AddChild(_weatherIconLabel);

            var weatherInfo = new VBoxContainer();
            weatherHeader.AddChild(weatherInfo);

            _weatherNameLabel = new Label();
            _weatherNameLabel.Text = "晴朗";
            _weatherNameLabel.AddThemeFontSizeOverride("font_size", 16);
            _weatherNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
            weatherInfo.AddChild(_weatherNameLabel);

            // 持续时间
            var durationContainer = new HBoxContainer();
            contentVBox.AddChild(durationContainer);

            var durationTitle = new Label();
            durationTitle.Text = "持续: ";
            durationTitle.AddThemeFontSizeOverride("font_size", 14);
            durationContainer.AddChild(durationTitle);

            _durationLabel = new Label();
            _durationLabel.Text = "10:00";
            _durationLabel.AddThemeFontSizeOverride("font_size", 14);
            _durationLabel.Modulate = new Color(1, 1, 0);
            durationContainer.AddChild(_durationLabel);

            // 进度条
            _durationProgressBar = new ProgressBar();
            _durationProgressBar.CustomMinimumSize = new Vector2(180, 12);
            _durationProgressBar.ShowPercentage = false;
            _durationProgressBar.Value = 100;
            contentVBox.AddChild(_durationProgressBar);

            // 效果标题
            var effectsTitle = new Label();
            effectsTitle.Text = "──────── 天气效果 ────────";
            effectsTitle.HorizontalAlignment = HorizontalAlignment.Center;
            effectsTitle.AddThemeFontSizeOverride("font_size", 12);
            contentVBox.AddChild(effectsTitle);

            // 效果容器
            _effectsContainer = new VBoxContainer();
            _effectsContainer.AddThemeConstantOverride("separation", 5);
            contentVBox.AddChild(_effectsContainer);

            // 创建效果标签
            _damageEffect = CreateEffectLabel("伤害: +0%", contentVBox);
            _defenseEffect = CreateEffectLabel("防御: +0%", contentVBox);
            _expEffect = CreateEffectLabel("经验: +0%", contentVBox);
            _dropEffect = CreateEffectLabel("掉落: +0%", contentVBox);
            _visibilityEffect = CreateEffectLabel("视野: 100%", contentVBox);

            // 按钮
            _changeWeatherButton = new Button();
            _changeWeatherButton.Text = "  切换天气  ";
            _changeWeatherButton.Pressed += OnChangeWeatherPressed;
            contentVBox.AddChild(_changeWeatherButton);

            // 自动切换复选框
            var autoCheckBox = new CheckBox();
            autoCheckBox.Text = "自动切换天气";
            autoCheckBox.ButtonPressed = true;
            autoCheckBox.Toggled += OnAutoChangeToggled;
            contentVBox.AddChild(autoCheckBox);

            UpdateWeatherDisplay();
        }

        private Label CreateEffectLabel(string text, VBoxContainer parent)
        {
            var label = new Label();
            label.Text = text;
            label.AddThemeFontSizeOverride("font_size", 13);
            label.HorizontalAlignment = HorizontalAlignment.Center;
            parent.AddChild(label);
            return label;
        }

        private void OnWeatherChanged(WeatherData newWeather, WeatherData oldWeather)
        {
            UpdateWeatherDisplay();
        }

        private void OnWeatherUpdated(WeatherData currentWeather)
        {
            UpdateDuration();
        }

        private void UpdateWeatherDisplay()
        {
            var weather = _weatherSystem.CurrentWeather;
            if (weather == null) return;

            // 更新图标和名称
            _weatherIconLabel.Text = WeatherSystem.GetWeatherIcon(weather.Type);
            _weatherNameLabel.Text = WeatherSystem.GetWeatherName(weather.Type);

            // 更新进度条
            _durationProgressBar.MaxValue = weather.Duration;
            _durationProgressBar.Value = weather.RemainingTime;

            UpdateDuration();

            // 更新效果显示
            UpdateEffects(weather);
        }

        private void UpdateDuration()
        {
            var weather = _weatherSystem.CurrentWeather;
            if (weather == null) return;

            var minutes = (int)(weather.RemainingTime / 60);
            var seconds = (int)(weather.RemainingTime % 60);
            _durationLabel.Text = $"{minutes:D2}:{seconds:D2}";

            // 根据剩余时间改变颜色
            if (weather.RemainingTime < 60)
                _durationLabel.Modulate = new Color(1, 0.3, 0.3);
            else if (weather.RemainingTime < 180)
                _durationLabel.Modulate = new Color(1, 1, 0.3);
            else
                _durationLabel.Modulate = new Color(1, 1, 0);
        }

        private void UpdateEffects(WeatherData weather)
        {
            // 伤害倍率
            var damageText = weather.DamageMultiplier >= 1.0f 
                ? $"伤害: +{(weather.DamageMultiplier - 1) * 100:F0}%" 
                : $"伤害: {(weather.DamageMultiplier - 1) * 100:F0}%";
            _damageEffect.Text = damageText;
            _damageEffect.Modulate = weather.DamageMultiplier >= 1.0f 
                ? new Color(0.3, 1, 0.3) : new Color(1, 0.3, 0.3);

            // 防御倍率
            var defenseText = weather.DefenseMultiplier >= 1.0f 
                ? $"防御: +{(weather.DefenseMultiplier - 1) * 100:F0}%" 
                : $"防御: {(weather.DefenseMultiplier - 1) * 100:F0}%";
            _defenseEffect.Text = defenseText;
            _defenseEffect.Modulate = weather.DefenseMultiplier >= 1.0f 
                ? new Color(0.3, 1, 0.3) : new Color(1, 0.3, 0.3);

            // 经验倍率
            var expText = weather.ExperienceMultiplier >= 1.0f 
                ? $"经验: +{(weather.ExperienceMultiplier - 1) * 100:F0}%" 
                : $"经验: {(weather.ExperienceMultiplier - 1) * 100:F0}%";
            _expEffect.Text = expText;
            _expEffect.Modulate = weather.ExperienceMultiplier >= 1.0f 
                ? new Color(0.3, 1, 0.3) : new Color(1, 0.3, 0.3);

            // 掉落倍率
            var dropText = weather.DropMultiplier >= 1.0f 
                ? $"掉落: +{(weather.DropMultiplier - 1) * 100:F0}%" 
                : $"掉落: {(weather.DropMultiplier - 1) * 100:F0}%";
            _dropEffect.Text = dropText;
            _dropEffect.Modulate = weather.DropMultiplier >= 1.0f 
                ? new Color(0.3, 1, 0.3) : new Color(1, 0.3, 0.3);

            // 视野
            var baseVisibility = 500f;
            var visibilityPercent = (weather.VisibilityRadius / baseVisibility) * 100;
            _visibilityEffect.Text = $"视野: {visibilityPercent:F0}%";
            _visibilityEffect.Modulate = visibilityPercent >= 100 
                ? new Color(0.3, 1, 0.3) : new Color(1, 0.3, 0.3);
        }

        private void OnChangeWeatherPressed()
        {
            _weatherSystem.ChangeToRandomWeather();
        }

        private void OnAutoChangeToggled(bool toggledOn)
        {
            _weatherSystem.AutoChange = toggledOn;
        }

        /// <summary>
        /// 切换可见性
        /// </summary>
        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            Visible = _isVisible;
            
            if (_isVisible)
            {
                UpdateWeatherDisplay();
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // V键切换天气UI
                if (keyEvent.Keycode == Key.V)
                {
                    ToggleVisibility();
                    GetTree().SetInputAsHandled();
                }
            }
        }
    }
}
