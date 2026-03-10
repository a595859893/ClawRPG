using Godot;
using System;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI;

/// <summary>
/// 游戏设置界面
/// </summary>
public partial class SettingsUI : Control
{
    private GameSettings _settings;
    
    // 标签页
    private TabContainer _tabContainer;
    
    // 音量滑块
    private HSlider _masterVolumeSlider;
    private HSlider _musicVolumeSlider;
    private HSlider _sfxVolumeSlider;
    private HSlider _voiceVolumeSlider;
    
    // 画面复选框
    private CheckButton _fullscreenCheck;
    private CheckButton _vsyncCheck;
    private OptionButton _qualityOption;
    private CheckButton _showFpsCheck;
    private CheckButton _showDamageNumbersCheck;
    
    // 游戏设置
    private OptionButton _difficultyOption;
    private CheckButton _autoSaveCheck;
    private SpinBox _autoSaveIntervalSpin;
    private CheckButton _showTutorialsCheck;
    private HSlider _uiScaleSlider;
    
    // 辅助功能
    private CheckButton _screenShakeCheck;
    private CheckButton _hitStopCheck;
    private CheckButton _controllerVibrationCheck;
    
    // 按钮
    private Button _saveButton;
    private Button _resetButton;
    private Button _closeButton;
    
    public override void _Ready()
    {
        _settings = GameSettings.Instance;
        
        SetupUI();
        LoadCurrentSettings();
    }
    
    private void SetupUI()
    {
        // 背景面板
        var bgPanel = new PanelContainer();
        bgPanel.Set AnchorsPreset(Control.LayoutPreset.Center);
        bgPanel.CustomMinimumSize = new Vector2(600, 500);
        
        var bgStyle = new StyleBoxFlat();
        bgStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.98f);
        bgStyle.CornerRadiusTopLeft = 12;
        bgStyle.CornerRadiusTopRight = 12;
        bgStyle.CornerRadiusBottomLeft = 12;
        bgStyle.CornerRadiusBottomRight = 12;
        bgStyle.BorderWidthLeft = 2;
        bgStyle.BorderWidthRight = 2;
        bgStyle.BorderWidthTop = 2;
        bgStyle.BorderWidthBottom = 2;
        bgStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        bgPanel.AddThemeStyleboxOverride("panel", bgStyle);
        AddChild(bgPanel);
        
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        vbox.Set AnchorsPreset(Control.LayoutPreset.Center);
        vbox.Position = new Vector2(-280, -230);
        bgPanel.AddChild(vbox);
        
        // 标题
        var title = new Label();
        title.Text = "⚙️ 游戏设置";
        title.AddThemeFontSizeOverride("font_size", 24);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(title);
        
        // 创建标签页
        _tabContainer = new TabContainer();
        _tabContainer.CustomMinimumSize = new Vector2(560, 380);
        vbox.AddChild(_tabContainer);
        
        // 创建各个标签页
        CreateAudioTab();
        CreateGraphicsTab();
        CreateGameTab();
        CreateAccessibilityTab();
        
        // 按钮行
        var buttonRow = new HBoxContainer();
        buttonRow.Alignment = BoxContainer.AlignmentMode.Center;
        buttonRow.AddThemeConstantOverride("separation", 20);
        vbox.AddChild(buttonRow);
        
        _saveButton = new Button();
        _saveButton.Text = "💾 保存设置";
        _saveButton.Pressed += OnSavePressed;
        buttonRow.AddChild(_saveButton);
        
        _resetButton = new Button();
        _resetButton.Text = "🔄 重置默认";
        _resetButton.Pressed += OnResetPressed;
        buttonRow.AddChild(_resetButton);
        
        _closeButton = new Button();
        _closeButton.Text = "❌ 关闭";
        _closeButton.Pressed += OnClosePressed;
        buttonRow.AddChild(_closeButton);
        
        // ESC 关闭
        GetTree().Root.SizeChanged += OnWindowSizeChanged;
    }
    
    private void CreateAudioTab()
    {
        var audioPanel = new ScrollContainer();
        audioPanel.Name = "音频";
        _tabContainer.AddChild(audioPanel);
        
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 15);
        vbox.Set AnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.Position = new Vector2(10, 10);
        vbox.Size = new Vector2(530, 330);
        audioPanel.AddChild(vbox);
        
        // 主音量
        vbox.AddChild(CreateSliderRow("🔊 主音量", out _masterVolumeSlider, 0, 100, 100));
        
        // 音乐音量
        vbox.AddChild(CreateSliderRow("🎵 音乐音量", out _musicVolumeSlider, 0, 100, 80));
        
        // 音效音量
        vbox.AddChild(CreateSliderRow("🔊 音效音量", out _sfxVolumeSlider, 0, 100, 100));
        
        // 语音音量
        vbox.AddChild(CreateSliderRow("🎤 语音音量", out _voiceVolumeSlider, 0, 100, 100));
    }
    
    private void CreateGraphicsTab()
    {
        var graphicsPanel = new VBoxContainer();
        graphicsPanel.Name = "画面";
        graphicsPanel.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(graphicsPanel);
        
        // 全屏
        _fullscreenCheck = new CheckButton();
        _fullscreenCheck.Text = "🖥️ 全屏模式";
        graphicsPanel.AddChild(_fullscreenCheck);
        
        // 垂直同步
        _vsyncCheck = new CheckButton();
        _vsyncCheck.Text = "🔄 垂直同步";
        _vsyncCheck.ButtonPressed = true;
        graphicsPanel.AddChild(_vsyncCheck);
        
        // 画质
        var qualityRow = new HBoxContainer();
        qualityRow.AddThemeConstantOverride("separation", 10);
        graphicsPanel.AddChild(qualityRow);
        
        var qualityLabel = new Label();
        qualityLabel.Text = "🎮 画质:";
        qualityLabel.CustomMinimumSize = new Vector2(100, 0);
        qualityRow.AddChild(qualityLabel);
        
        _qualityOption = new OptionButton();
        _qualityOption.AddItem("低", 0);
        _qualityOption.AddItem("中", 1);
        _qualityOption.AddItem("高", 2);
        qualityRow.AddChild(_qualityOption);
        
        // 显示FPS
        _showFpsCheck = new CheckButton();
        _showFpsCheck.Text = "📊 显示帧率";
        graphicsPanel.AddChild(_showFpsCheck);
        
        // 显示伤害数字
        _showDamageNumbersCheck = new CheckButton();
        _showDamageNumbersCheck.Text = "💥 显示伤害数字";
        _showDamageNumbersCheck.ButtonPressed = true;
        graphicsPanel.AddChild(_showDamageNumbersCheck);
    }
    
    private void CreateGameTab()
    {
        var gamePanel = new VBoxContainer();
        gamePanel.Name = "游戏";
        gamePanel.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(gamePanel);
        
        // 难度
        var difficultyRow = new HBoxContainer();
        difficultyRow.AddThemeConstantOverride("separation", 10);
        gamePanel.AddChild(difficultyRow);
        
        var difficultyLabel = new Label();
        difficultyLabel.Text = "⚔️ 难度:";
        difficultyLabel.CustomMinimumSize = new Vector2(100, 0);
        difficultyRow.AddChild(difficultyLabel);
        
        _difficultyOption = new OptionButton();
        _difficultyOption.AddItem("简单", 0);
        _difficultyOption.AddItem("普通", 1);
        _difficultyOption.AddItem("困难", 2);
        difficultyRow.AddChild(_difficultyOption);
        
        // 自动保存
        _autoSaveCheck = new CheckButton();
        _autoSaveCheck.Text = "💾 自动保存";
        _autoSaveCheck.ButtonPressed = true;
        gamePanel.AddChild(_autoSaveCheck);
        
        // 自动保存间隔
        var intervalRow = new HBoxContainer();
        intervalRow.AddThemeConstantOverride("separation", 10);
        gamePanel.AddChild(intervalRow);
        
        var intervalLabel = new Label();
        intervalLabel.Text = "⏱️ 自动保存间隔(秒):";
        intervalLabel.CustomMinimumSize = new Vector2(150, 0);
        intervalRow.AddChild(intervalLabel);
        
        _autoSaveIntervalSpin = new SpinBox();
        _autoSaveIntervalSpin.MinValue = 60;
        _autoSaveIntervalSpin.MaxValue = 1800;
        _autoSaveIntervalSpin.Value = 300;
        intervalRow.AddChild(_autoSaveIntervalSpin);
        
        // 显示教程
        _showTutorialsCheck = new CheckButton();
        _showTutorialsCheck.Text = "📖 显示教程";
        _showTutorialsCheck.ButtonPressed = true;
        gamePanel.AddChild(_showTutorialsCheck);
        
        // UI缩放
        vbox.AddChild(CreateSliderRow("📐 UI缩放", out _uiScaleSlider, 50, 200, 100));
    }
    
    private void CreateAccessibilityTab()
    {
        var accessPanel = new VBoxContainer();
        accessPanel.Name = "辅助功能";
        accessPanel.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(accessPanel);
        
        // 屏幕震动
        _screenShakeCheck = new CheckButton();
        _screenShakeCheck.Text = "📳 屏幕震动";
        _screenShakeCheck.ButtonPressed = true;
        accessPanel.AddChild(_screenShakeCheck);
        
        // 顿帧效果
        _hitStopCheck = new CheckButton();
        _hitStopCheck.Text = "⏸️ 打击顿帧效果";
        _hitStopCheck.ButtonPressed = true;
        accessPanel.AddChild(_hitStopCheck);
        
        // 手柄震动
        _controllerVibrationCheck = new CheckButton();
        _controllerVibrationCheck.ButtonPressed = _settings.ControllerVibration;
        _controllerVibrationCheck.ButtonPressed = true;
        accessPanel.AddChild(_controllerVibrationCheck);
        
        // 难度说明
        var difficultyInfo = new Label();
        difficultyInfo.Text = @"
难度说明:
• 简单: 敌人伤害-30%, 经验+50%, 掉落+50%
• 普通: 标准难度
• 困难: 敌人伤害+50%, 经验-30%, 掉落-20%
";
        difficultyInfo.AutowrapMode = TextServer.AutowrapMode.Word;
        accessPanel.AddChild(difficultyInfo);
    }
    
    private Control CreateSliderRow(string labelText, out HSlider slider, float min, float max, float value)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 10);
        
        var label = new Label();
        label.Text = labelText;
        label.CustomMinimumSize = new Vector2(120, 0);
        row.AddChild(label);
        
        slider = new HSlider();
        slider.MinValue = min;
        slider.MaxValue = max;
        slider.Value = value;
        slider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        row.AddChild(slider);
        
        var valueLabel = new Label();
        valueLabel.Text = ((int)value).ToString();
        valueLabel.CustomMinimumSize = new Vector2(40, 0);
        valueLabel.Name = "ValueLabel";
        row.AddChild(valueLabel);
        
        slider.ValueChanged += (val) => valueLabel.Text = ((int)val).ToString();
        
        return row;
    }
    
    private void LoadCurrentSettings()
    {
        if (_settings == null) return;
        
        // 音量
        _masterVolumeSlider.Value = _settings.MasterVolume * 100;
        _musicVolumeSlider.Value = _settings.MusicVolume * 100;
        _sfxVolumeSlider.Value = _settings.SfxVolume * 100;
        _voiceVolumeSlider.Value = _settings.VoiceVolume * 100;
        
        // 画面
        _fullscreenCheck.ButtonPressed = _settings.Fullscreen;
        _vsyncCheck.ButtonPressed = _settings.Vsync;
        _qualityOption.Selected = _settings.QualityLevel;
        _showFpsCheck.ButtonPressed = _settings.ShowFps;
        _showDamageNumbersCheck.ButtonPressed = _settings.ShowDamageNumbers;
        
        // 游戏
        _difficultyOption.Selected = _settings.Difficulty;
        _autoSaveCheck.ButtonPressed = _settings.AutoSave;
        _autoSaveIntervalSpin.Value = _settings.AutoSaveInterval;
        _showTutorialsCheck.ButtonPressed = _settings.ShowTutorials;
        _uiScaleSlider.Value = _settings.UiScale * 100;
        
        // 辅助功能
        _screenShakeCheck.ButtonPressed = _settings.ScreenShake;
        _hitStopCheck.ButtonPressed = _settings.HitStop;
        _controllerVibrationCheck.ButtonPressed = _settings.ControllerVibration;
    }
    
    private void OnSavePressed()
    {
        if (_settings == null) return;
        
        // 音量
        _settings.MasterVolume = (float)(_masterVolumeSlider.Value / 100.0);
        _settings.MusicVolume = (float)(_musicVolumeSlider.Value / 100.0);
        _settings.SfxVolume = (float)(_sfxVolumeSlider.Value / 100.0);
        _settings.VoiceVolume = (float)(_voiceVolumeSlider.Value / 100.0);
        
        // 画面
        _settings.Fullscreen = _fullscreenCheck.ButtonPressed;
        _settings.Vsync = _vsyncCheck.ButtonPressed;
        _settings.QualityLevel = _qualityOption.Selected;
        _settings.ShowFps = _showFpsCheck.ButtonPressed;
        _settings.ShowDamageNumbers = _showDamageNumbersCheck.ButtonPressed;
        
        // 游戏
        _settings.Difficulty = _difficultyOption.Selected;
        _settings.AutoSave = _autoSaveCheck.ButtonPressed;
        _settings.AutoSaveInterval = (int)_autoSaveIntervalSpin.Value;
        _settings.ShowTutorials = _showTutorialsCheck.ButtonPressed;
        _settings.UiScale = (float)(_uiScaleSlider.Value / 100.0);
        
        // 辅助功能
        _settings.ScreenShake = _screenShakeCheck.ButtonPressed;
        _settings.HitStop = _hitStopCheck.ButtonPressed;
        _settings.ControllerVibration = _controllerVibrationCheck.ButtonPressed;
        
        _settings.SaveSettings();
        
        GD.Print("Settings applied and saved");
        QueueFree();
    }
    
    private void OnResetPressed()
    {
        _settings?.ResetToDefaults();
        LoadCurrentSettings();
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    private void OnWindowSizeChanged()
    {
        // 窗口大小改变时保持居中
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey key && key.Pressed && key.Keycode == Key.Escape)
        {
            QueueFree();
            GetTree().Root.SizeChanged -= OnWindowSizeChanged;
        }
    }
}
