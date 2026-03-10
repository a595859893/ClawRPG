using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems;

/// <summary>
/// 游戏设置系统 - 管理游戏的各种配置选项
/// 应用单例模式：全局唯一访问点
/// </summary>
public partial class GameSettings : Node
{
    public static GameSettings Instance { get; private set; }

    // 音量设置
    [Export] private float _masterVolume = 1.0f;
    [Export] private float _musicVolume = 0.8f;
    [Export] private float _sfxVolume = 1.0f;
    [Export] private float _voiceVolume = 1.0f;

    // 画面设置
    [Export] private bool _fullscreen = false;
    [Export] private bool _vsync = true;
    [Export] private int _qualityLevel = 2; // 0=低, 1=中, 2=高
    [Export] private bool _showFps = false;
    [Export] private bool _showDamageNumbers = true;

    // 游戏设置
    [Export] private int _difficulty = 1; // 0=简单, 1=普通, 2=困难
    [Export] private bool _autoSave = true;
    [Export] private int _autoSaveInterval = 300; // 秒
    [Export] private bool _showTutorials = true;
    [Export] private bool _showDamageNumbersOnUi = true;
    [Export] private float _uiScale = 1.0f;

    // 辅助功能
    [Export] private bool _screenShake = true;
    [Export] private bool _hitStop = true;
    [Export] private bool _controllerVibration = true;

    // 按键设置
    [Export] private Dictionary<string, string> _keyBindings = new Dictionary<string, string>()
    {
        { "move_up", "Key.W" },
        { "move_down", "Key.S" },
        { "move_left", "Key.A" },
        { "move_right", "Key.D" },
        { "attack", "Key.J" },
        { "block", "Key.K" },
        { "dodge", "Key.L" },
        { "skill1", "Key.1" },
        { "skill2", "Key.2" },
        { "skill3", "Key.3" },
        { "skill4", "Key.4" },
        { "potion", "Key.Q" },
        { "interact", "Key.E" },
        { "inventory", "Key.I" },
        { "map", "Key.M" },
        { "quest", "Key.J" },
        { "pause", "Key.Escape" }
    };

    public override void _Ready()
    {
        Instance = this;
        Name = "GameSettings";
        Priority = 100; // 早期加载
        
        LoadSettings();
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    #region 音量控制

    public float MasterVolume
    {
        get => _masterVolume;
        set { _masterVolume = Mathf.Clamp(value, 0f, 1f); }
    }

    public float MusicVolume
    {
        get => _musicVolume;
        set { _musicVolume = Mathf.Clamp(value, 0f, 1f); }
    }

    public float SfxVolume
    {
        get => _sfxVolume;
        set { _sfxVolume = Mathf.Clamp(value, 0f, 1f); }
    }

    public float VoiceVolume
    {
        get => _voiceVolume;
        set { _voiceVolume = Mathf.Clamp(value, 0f, 1f); }
    }

    /// <summary>
    /// 获取实际音量（考虑主音量）
    /// </summary>
    public float GetEffectiveVolume(float volumeType)
    {
        return volumeType * _masterVolume;
    }

    #endregion

    #region 画面设置

    public bool Fullscreen
    {
        get => _fullscreen;
        set 
        { 
            _fullscreen = value;
            ApplyDisplaySettings();
        }
    }

    public bool Vsync
    {
        get => _vsync;
        set 
        { 
            _vsync = value;
            ApplyDisplaySettings();
        }
    }

    public int QualityLevel
    {
        get => _qualityLevel;
        set => _qualityLevel = Mathf.Clamp(value, 0, 2);
    }

    public bool ShowFps
    {
        get => _showFps;
        set => _showFps = value;
    }

    public bool ShowDamageNumbers
    {
        get => _showDamageNumbers;
        set => _showDamageNumbers = value;
    }

    private void ApplyDisplaySettings()
    {
        var mode = _fullscreen ? Window.Mode.Fullscreen : Window.Mode.Windowed;
        GetWindow().Mode = mode;
        DisplayServer.WindowSetVsyncMode(_vsync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VsyncMode.Disabled);
    }

    #endregion

    #region 游戏设置

    public int Difficulty
    {
        get => _difficulty;
        set => _difficulty = Mathf.Clamp(value, 0, 2);
    }

    public string DifficultyName => _difficulty switch
    {
        0 => "简单",
        1 => "普通",
        2 => "困难",
        _ => "普通"
    };

    public float DifficultyMultiplier => _difficulty switch
    {
        0 => 0.8f,
        1 => 1.0f,
        2 => 1.5f,
        _ => 1.0f
    };

    public bool AutoSave
    {
        get => _autoSave;
        set => _autoSave = value;
    }

    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set => _autoSaveInterval = Mathf.Max(value, 60);
    }

    public bool ShowTutorials
    {
        get => _showTutorials;
        set => _showTutorials = value;
    }

    public float UiScale
    {
        get => _uiScale;
        set => _uiScale = Mathf.Clamp(value, 0.5f, 2.0f);
    }

    #endregion

    #region 辅助功能

    public bool ScreenShake
    {
        get => _screenShake;
        set => _screenShake = value;
    }

    public bool HitStop
    {
        get => _hitStop;
        set => _hitStop = value;
    }

    public bool ControllerVibration
    {
        get => _controllerVibration;
        set => _controllerVibration = value;
    }

    #endregion

    #region 按键设置

    public Dictionary<string, string> KeyBindings
    {
        get => _keyBindings;
        set => _keyBindings = value;
    }

    public string GetKeyBinding(string action)
    {
        return _keyBindings.ContainsKey(action) ? _keyBindings[action] : "";
    }

    public void SetKeyBinding(string action, string key)
    {
        _keyBindings[action] = key;
    }

    #endregion

    #region 存档

    public void SaveSettings()
    {
        var config = new ConfigFile();
        
        // 音量
        config.SetValue("Audio", "master_volume", _masterVolume);
        config.SetValue("Audio", "music_volume", _musicVolume);
        config.SetValue("Audio", "sfx_volume", _sfxVolume);
        config.SetValue("Audio", "voice_volume", _voiceVolume);
        
        // 画面
        config.SetValue("Graphics", "fullscreen", _fullscreen);
        config.SetValue("Graphics", "vsync", _vsync);
        config.SetValue("Graphics", "quality_level", _qualityLevel);
        config.SetValue("Graphics", "show_fps", _showFps);
        config.SetValue("Graphics", "show_damage_numbers", _showDamageNumbers);
        
        // 游戏
        config.SetValue("Game", "difficulty", _difficulty);
        config.SetValue("Game", "auto_save", _autoSave);
        config.SetValue("Game", "auto_save_interval", _autoSaveInterval);
        config.SetValue("Game", "show_tutorials", _showTutorials);
        config.SetValue("Game", "ui_scale", _uiScale);
        
        // 辅助功能
        config.SetValue("Accessibility", "screen_shake", _screenShake);
        config.SetValue("Accessibility", "hit_stop", _hitStop);
        config.SetValue("Accessibility", "controller_vibration", _controllerVibration);
        
        // 按键
        foreach (var kvp in _keyBindings)
        {
            config.SetValue("KeyBindings", kvp.Key, kvp.Value);
        }
        
        config.Save("user://settings.cfg");
        GD.Print("Settings saved");
    }

    public void LoadSettings()
    {
        var config = new ConfigFile();
        var err = config.Load("user://settings.cfg");
        
        if (err != Error.Ok)
        {
            GD.Print("No settings file found, using defaults");
            return;
        }
        
        // 音量
        _masterVolume = config.GetValue("Audio", "master_volume", _masterVolume);
        _musicVolume = config.GetValue("Audio", "music_volume", _musicVolume);
        _sfxVolume = config.GetValue("Audio", "sfx_volume", _sfxVolume);
        _voiceVolume = config.GetValue("Audio", "voice_volume", _voiceVolume);
        
        // 画面
        _fullscreen = (bool)config.GetValue("Graphics", "fullscreen", _fullscreen);
        _vsync = (bool)config.GetValue("Graphics", "vsync", _vsync);
        _qualityLevel = (int)config.GetValue("Graphics", "quality_level", _qualityLevel);
        _showFps = (bool)config.GetValue("Graphics", "show_fps", _showFps);
        _showDamageNumbers = (bool)config.GetValue("Graphics", "show_damage_numbers", _showDamageNumbers);
        
        // 游戏
        _difficulty = (int)config.GetValue("Game", "difficulty", _difficulty);
        _autoSave = (bool)config.GetValue("Game", "auto_save", _autoSave);
        _autoSaveInterval = (int)config.GetValue("Game", "auto_save_interval", _autoSaveInterval);
        _showTutorials = (bool)config.GetValue("Game", "show_tutorials", _showTutorials);
        _uiScale = (float)config.GetValue("Game", "ui_scale", _uiScale);
        
        // 辅助功能
        _screenShake = (bool)config.GetValue("Accessibility", "screen_shake", _screenShake);
        _hitStop = (bool)config.GetValue("Accessibility", "hit_stop", _hitStop);
        _controllerVibration = (bool)config.GetValue("Accessibility", "controller_vibration", _controllerVibration);
        
        // 按键绑定
        foreach (var key in _keyBindings.Keys)
        {
            _keyBindings[key] = (string)config.GetValue("KeyBindings", key, _keyBindings[key]);
        }
        
        // 应用画面设置
        ApplyDisplaySettings();
        
        GD.Print("Settings loaded");
    }

    public void ResetToDefaults()
    {
        _masterVolume = 1.0f;
        _musicVolume = 0.8f;
        _sfxVolume = 1.0f;
        _voiceVolume = 1.0f;
        
        _fullscreen = false;
        _vsync = true;
        _qualityLevel = 2;
        _showFps = false;
        _showDamageNumbers = true;
        
        _difficulty = 1;
        _autoSave = true;
        _autoSaveInterval = 300;
        _showTutorials = true;
        _uiScale = 1.0f;
        
        _screenShake = true;
        _hitStop = true;
        _controllerVibration = true;
        
        _keyBindings = new Dictionary<string, string>()
        {
            { "move_up", "Key.W" },
            { "move_down", "Key.S" },
            { "move_left", "Key.A" },
            { "move_right", "Key.D" },
            { "attack", "Key.J" },
            { "block", "Key.K" },
            { "dodge", "Key.L" },
            { "skill1", "Key.1" },
            { "skill2", "Key.2" },
            { "skill3", "Key.3" },
            { "skill4", "Key.4" },
            { "potion", "Key.Q" },
            { "interact", "Key.E" },
            { "inventory", "Key.I" },
            { "map", "Key.M" },
            { "quest", "Key.J" },
            { "pause", "Key.Escape" }
        };
        
        ApplyDisplaySettings();
        SaveSettings();
        GD.Print("Settings reset to defaults");
    }

    #endregion

    #region 难度相关

    /// <summary>
    /// 获取难度对伤害的加成
    /// </summary>
    public float GetDamageMultiplier(bool isPlayerDamage)
    {
        if (isPlayerDamage)
        {
            // 玩家伤害在困难模式下降低
            return _difficulty switch
            {
                0 => 1.2f,
                1 => 1.0f,
                2 => 0.8f,
                _ => 1.0f
            };
        }
        else
        {
            // 敌人伤害在困难模式下增加
            return _difficulty switch
            {
                0 => 0.7f,
                1 => 1.0f,
                2 => 1.5f,
                _ => 1.0f
            };
        }
    }

    /// <summary>
    /// 获取难度对经验的加成
    /// </summary>
    public float GetExpMultiplier()
    {
        return _difficulty switch
        {
            0 => 1.5f,
            1 => 1.0f,
            2 => 0.7f,
            _ => 1.0f
        };
    }

    /// <summary>
    /// 获取难度对掉落的加成
    /// </summary>
    public float GetDropMultiplier()
    {
        return _difficulty switch
        {
            0 => 1.5f,
            1 => 1.0f,
            2 => 0.8f,
            _ => 1.0f
        };
    }

    #endregion
}
