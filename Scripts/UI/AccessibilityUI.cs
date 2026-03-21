using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems;

namespace ClawRPG.UI {
    /// <summary>
    /// 无障碍设置界面 - Accessibility Settings UI
    /// Ctrl+Shift+A 键切换显示
    /// </summary>
    public class AccessibilityUI : Control
    {
        private static AccessibilityUI _instance;
        public static AccessibilityUI Instance => _instance;

        private bool _isVisible = false;
        private PanelContainer _mainPanel;
        private VBoxContainer _contentBox;
        
        // 设置项
        private OptionButton _colorBlindOption;
        private CheckButton _highContrastCheck;
        private OptionButton _uiScaleOption;
        private OptionButton _textSizeOption;
        private CheckButton _subtitlesCheck;
        
        // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
        public event Action OnAccessibilitySettingsChangedUI;
        private CheckButton _soundVizCheck;
        private CheckButton _simplifiedCheck;
        private CheckButton _autoPotionCheck;
        private CheckButton _damageNumbersCheck;
        private HSlider _uiVolumeSlider;
        private HSlider _musicVolumeSlider;
        private HSlider _sfxVolumeSlider;
        private Label _uiVolumeLabel;
        private Label _musicVolumeLabel;
        private Label _sfxVolumeLabel;

        public override void _Ready()
        {
            _instance = this;
            SetupUI();
            Hide();
            
            // 连接到AccessibilityManager信号 (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
            if (AccessibilityManager.Instance != null)
            {
                AccessibilityManager.Instance.AccessibilitySettingsChanged += OnSettingsChanged; // NEW
                AccessibilityManager.Instance.Connect(nameof(AccessibilityManager.AccessibilitySettingsChanged), Callable.From(OnSettingsChanged)); // TODO: Remove after migration
            }
        }

        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainPanel.CustomMinimumSize = new Vector2(500, 600);
            AddChild(_mainPanel);

            // 样式
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            _mainPanel.AddThemeStyleboxOverride("panel", style);

            // 滚动容器
            var scroll = new ScrollContainer();
            scroll.SetHorizontalStretchMode(Control.StretchMode.Expand);
            _mainPanel.AddChild(scroll);

            // 内容容器
            _contentBox = new VBoxContainer();
            _contentBox.SetHorizontalStretchMode(Control.StretchMode.Expand);
            _contentBox.AddThemeConstantOverride("separation", 10);
            scroll.AddChild(_contentBox);

            // 标题
            var title = new Label();
            title.Text = "♿ 无障碍设置 / Accessibility";
            title.HorizontalAlignment = HorizontalAlignment.Center;
            title.AddThemeFontSizeOverride("font_size", 24);
            title.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.5f));
            _contentBox.AddChild(title);

            AddSeparator();

            // 视觉无障碍部分
            AddSectionTitle("视觉无障碍 / Visual Accessibility");
            
            // 颜色盲模式
            AddSettingRow("颜色盲模式:", CreateColorBlindOption());
            _contentBox.AddChild(CreateDescription("红绿色盲/蓝黄色盲模式"));

            // 高对比度
            AddSettingRow("高对比度模式:", CreateHighContrastCheck());
            _contentBox.AddChild(CreateDescription("增强UI对比度"));

            // UI缩放
            AddSettingRow("界面缩放:", CreateUIScaleOption());
            _contentBox.AddChild(CreateDescription("调整界面整体大小"));

            // 文字大小
            AddSettingRow("文字大小:", CreateTextSizeOption());
            _contentBox.AddChild(CreateDescription("调整文字显示大小"));

            AddSeparator();

            // 听觉无障碍部分
            AddSectionTitle("听觉无障碍 / Auditory Accessibility");

            // 字幕
            AddSettingRow("显示字幕:", CreateSubtitlesCheck());
            _contentBox.AddChild(CreateDescription("显示对话和音效字幕"));

            // 声音可视化
            AddSettingRow("声音可视化:", CreateSoundVizCheck());
            _contentBox.AddChild(CreateDescription("重要音效提供视觉替代"));

            // 音量控制
            AddVolumeControl("UI音量:", CreateUIVolumeSlider(), out _uiVolumeSlider, out _uiVolumeLabel);
            AddVolumeControl("音乐音量:", CreateMusicVolumeSlider(), out _musicVolumeSlider, out _musicVolumeLabel);
            AddVolumeControl("音效音量:", CreateSFXVolumeSlider(), out _sfxVolumeSlider, out _sfxVolumeLabel);

            AddSeparator();

            // 辅助功能部分
            AddSectionTitle("辅助功能 / Assistance");

            // 简化操作
            AddSettingRow("简化操作模式:", CreateSimplifiedCheck());
            _contentBox.AddChild(CreateDescription("简化控制和交互方式"));

            // 自动药水
            AddSettingRow("自动药水:", CreateAutoPotionCheck());
            _contentBox.AddChild(CreateDescription("血量低时自动使用药水"));

            // 伤害数字
            AddSettingRow("显示伤害数字:", CreateDamageNumbersCheck());
            _contentBox.AddChild(CreateDescription("战斗伤害数值显示"));

            AddSeparator();

            // 按钮区域
            var buttonBox = new HBoxContainer();
            buttonBox.Alignment = BoxContainer.AlignmentMode.Center;
            buttonBox.AddThemeConstantOverride("separation", 20);
            _contentBox.AddChild(buttonBox);

            var resetBtn = CreateButton("重置默认");
            resetBtn.Pressed += OnResetPressed;
            buttonBox.AddChild(resetBtn);

            var closeBtn = CreateButton("关闭");
            closeBtn.Pressed += () => ToggleVisibility();
            buttonBox.AddChild(closeBtn);

            AddSeparator();
        }

        private void AddSectionTitle(string text)
        {
            var label = new Label();
            label.Text = text;
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.AddThemeFontSizeOverride("font_size", 18);
            label.AddThemeColorOverride("font_color", new Color(0.7f, 0.85f, 1f));
            _contentBox.AddChild(label);
        }

        private void AddSeparator()
        {
            var sep = new HSeparator();
            sep.AddThemeConstantOverride("separation", 10);
            _contentBox.AddChild(sep);
        }

        private void AddSettingRow(string labelText, Control control)
        {
            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 10);
            
            var label = new Label();
            label.Text = labelText;
            label.CustomMinimumSize = new Vector2(140, 0);
            label.HorizontalAlignment = HorizontalAlignment.Right;
            hbox.AddChild(label);
            
            control.SetHorizontalExpand(true);
            hbox.AddChild(control);
            
            _contentBox.AddChild(hbox);
        }

        private void AddVolumeControl(string labelText, HSlider slider, out HSlider outSlider, out Label outLabel)
        {
            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 10);
            
            var label = new Label();
            label.Text = labelText;
            label.CustomMinimumSize = new Vector2(140, 0);
            label.HorizontalAlignment = HorizontalAlignment.Right;
            hbox.AddChild(label);
            
            slider.SetHorizontalExpand(true);
            hbox.AddChild(slider);
            
            outLabel = new Label();
            outLabel.Text = "100%";
            outLabel.CustomMinimumSize = new Vector2(50, 0);
            hbox.AddChild(outLabel);
            
            outSlider = slider;
            _contentBox.AddChild(hbox);
        }

        private Label CreateDescription(string text)
        {
            var label = new Label();
            label.Text = "  ↳ " + text;
            label.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
            label.AddThemeFontSizeOverride("font_size", 12);
            return label;
        }

        private OptionButton CreateColorBlindOption()
        {
            _colorBlindOption = new OptionButton();
            _colorBlindOption.AddItem("关闭", 0);
            _colorBlindOption.AddItem("红绿色盲", 1);
            _colorBlindOption.AddItem("蓝黄色盲", 2);
            _colorBlindOption.Selected = (int)AccessibilityManager.Instance.ColorBlind;
            _colorBlindOption.ItemSelected += (index) => {
                AccessibilityManager.Instance.ColorBlind = (AccessibilityManager.ColorBlindMode)index;
            };
            return _colorBlindOption;
        }

        private CheckButton CreateHighContrastCheck()
        {
            _highContrastCheck = new CheckButton();
            _highContrastCheck.ButtonPressed = AccessibilityManager.Instance.HighContrast;
            _highContrastCheck.Toggled += (pressed) => {
                AccessibilityManager.Instance.HighContrast = pressed;
            };
            return _highContrastCheck;
        }

        private OptionButton CreateUIScaleOption()
        {
            _uiScaleOption = new OptionButton();
            _uiScaleOption.AddItem("小 (80%)", 0);
            _uiScaleOption.AddItem("正常 (100%)", 1);
            _uiScaleOption.AddItem("大 (120%)", 2);
            _uiScaleOption.AddItem("特大 (140%)", 3);
            _uiScaleOption.Selected = (int)AccessibilityManager.Instance.UIScale;
            _uiScaleOption.ItemSelected += (index) => {
                AccessibilityManager.Instance.UIScale = (AccessibilityManager.UIScaleLevel)index;
            };
            return _uiScaleOption;
        }

        private OptionButton CreateTextSizeOption()
        {
            _textSizeOption = new OptionButton();
            _textSizeOption.AddItem("小", 0);
            _textSizeOption.AddItem("正常", 1);
            _textSizeOption.AddItem("大", 2);
            _textSizeOption.AddItem("特大", 3);
            _textSizeOption.Selected = (int)AccessibilityManager.Instance.TextSize;
            _textSizeOption.ItemSelected += (index) => {
                AccessibilityManager.Instance.TextSize = (AccessibilityManager.TextSizeLevel)index;
            };
            return _textSizeOption;
        }

        private CheckButton CreateSubtitlesCheck()
        {
            _subtitlesCheck = new CheckButton();
            _subtitlesCheck.ButtonPressed = AccessibilityManager.Instance.SubtitlesEnabled;
            _subtitlesCheck.Toggled += (pressed) => {
                AccessibilityManager.Instance.SubtitlesEnabled = pressed;
            };
            return _subtitlesCheck;
        }

        private CheckButton CreateSoundVizCheck()
        {
            _soundVizCheck = new CheckButton();
            _soundVizCheck.ButtonPressed = AccessibilityManager.Instance.SoundVisualization;
            _soundVizCheck.Toggled += (pressed) => {
                AccessibilityManager.Instance.SoundVisualization = pressed;
            };
            return _soundVizCheck;
        }

        private CheckButton CreateSimplifiedCheck()
        {
            _simplifiedCheck = new CheckButton();
            _simplifiedCheck.ButtonPressed = AccessibilityManager.Instance.SimplifiedControls;
            _simplifiedCheck.Toggled += (pressed) => {
                AccessibilityManager.Instance.SimplifiedControls = pressed;
            };
            return _simplifiedCheck;
        }

        private CheckButton CreateAutoPotionCheck()
        {
            _autoPotionCheck = new CheckButton();
            _autoPotionCheck.ButtonPressed = AccessibilityManager.Instance.AutoPotionEnabled;
            _autoPotionCheck.Toggled += (pressed) => {
                AccessibilityManager.Instance.AutoPotionEnabled = pressed;
            };
            return _autoPotionCheck;
        }

        private CheckButton CreateDamageNumbersCheck()
        {
            _damageNumbersCheck = new CheckButton();
            _damageNumbersCheck.ButtonPressed = AccessibilityManager.Instance.DamageNumbersEnabled;
            _damageNumbersCheck.Toggled += (pressed) => {
                AccessibilityManager.Instance.DamageNumbersEnabled = pressed;
            };
            return _damageNumbersCheck;
        }

        private HSlider CreateUIVolumeSlider()
        {
            _uiVolumeSlider = new HSlider();
            _uiVolumeSlider.MinValue = 0;
            _uiVolumeSlider.MaxValue = 100;
            _uiVolumeSlider.Value = AccessibilityManager.Instance.UIVolume * 100;
            _uiVolumeSlider.ValueChanged += (value) => {
                AccessibilityManager.Instance.UIVolume = value / 100f;
                if (_uiVolumeLabel != null)
                    _uiVolumeLabel.Text = (int)value + "%";
            };
            return _uiVolumeSlider;
        }

        private HSlider CreateMusicVolumeSlider()
        {
            _musicVolumeSlider = new HSlider();
            _musicVolumeSlider.MinValue = 0;
            _musicVolumeSlider.MaxValue = 100;
            _musicVolumeSlider.Value = AccessibilityManager.Instance.MusicVolume * 100;
            _musicVolumeSlider.ValueChanged += (value) => {
                AccessibilityManager.Instance.MusicVolume = value / 100f;
                if (_musicVolumeLabel != null)
                    _musicVolumeLabel.Text = (int)value + "%";
            };
            return _musicVolumeSlider;
        }

        private HSlider CreateSFXVolumeSlider()
        {
            _sfxVolumeSlider = new HSlider();
            _sfxVolumeSlider.MinValue = 0;
            _sfxVolumeSlider.MaxValue = 100;
            _sfxVolumeSlider.Value = AccessibilityManager.Instance.SFXVolume * 100;
            _sfxVolumeSlider.ValueChanged += (value) => {
                AccessibilityManager.Instance.SFXVolume = value / 100f;
                if (_sfxVolumeLabel != null)
                    _sfxVolumeLabel.Text = (int)value + "%";
            };
            return _sfxVolumeSlider;
        }

        private Button CreateButton(string text)
        {
            var btn = new Button();
            btn.Text = text;
            btn.CustomMinimumSize = new Vector2(120, 40);
            
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.2f, 0.4f, 0.6f);
            style.SetCornerRadiusAll(4);
            btn.AddThemeStyleboxOverride("normal", style);
            
            var hoverStyle = style.Duplicate() as StyleBoxFlat;
            hoverStyle.BgColor = new Color(0.3f, 0.5f, 0.7f);
            btn.AddThemeStyleboxOverride("hover", hoverStyle);
            
            return btn;
        }

        private void OnSettingsChanged()
        {
            // REQ-058-11: Invoke new event
            OnAccessibilitySettingsChangedUI?.Invoke();
            // 更新UI显示
            _colorBlindOption.Selected = (int)AccessibilityManager.Instance.ColorBlind;
            _highContrastCheck.ButtonPressed = AccessibilityManager.Instance.HighContrast;
            _uiScaleOption.Selected = (int)AccessibilityManager.Instance.UIScale;
            _textSizeOption.Selected = (int)AccessibilityManager.Instance.TextSize;
            _subtitlesCheck.ButtonPressed = AccessibilityManager.Instance.SubtitlesEnabled;
            _soundVizCheck.ButtonPressed = AccessibilityManager.Instance.SoundVisualization;
            _simplifiedCheck.ButtonPressed = AccessibilityManager.Instance.SimplifiedControls;
            _autoPotionCheck.ButtonPressed = AccessibilityManager.Instance.AutoPotionEnabled;
            _damageNumbersCheck.ButtonPressed = AccessibilityManager.Instance.DamageNumbersEnabled;
            
            if (_uiVolumeLabel != null)
                _uiVolumeLabel.Text = (int)(AccessibilityManager.Instance.UIVolume * 100) + "%";
            if (_musicVolumeLabel != null)
                _musicVolumeLabel.Text = (int)(AccessibilityManager.Instance.MusicVolume * 100) + "%";
            if (_sfxVolumeLabel != null)
                _sfxVolumeLabel.Text = (int)(AccessibilityManager.Instance.SFXVolume * 100) + "%";
        }

        private void OnResetPressed()
        {
            AccessibilityManager.Instance.ResetToDefaults();
        }

        public void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            if (_isVisible)
                Show();
            else
                Hide();
        }

        public override void _Input(InputEvent e)
        {
            if (e is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // Ctrl+Shift+A 切换显示
                if (keyEvent.Keycode == Key.A && keyEvent.Ctrl && keyEvent.Shift)
                {
                    ToggleVisibility();
                    GetTree().SetInputAsHandled();
                }
            }
        }
    }
}
