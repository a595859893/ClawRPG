using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems;

/// <summary>
/// Game settings system that manages all game configuration options.
/// Implements singleton pattern for global access.
/// </summary>
public partial class GameSettings : BaseSystem
{
    /// <summary>
    /// Gets the singleton instance of GameSettings.
    /// </summary>
    public static GameSettings Instance { get; private set; }

    // Audio settings
    [Export] private float _masterVolume = 1.0f;
    [Export] private float _musicVolume = 0.8f;
    [Export] private float _sfxVolume = 1.0f;
    [Export] private float _voiceVolume = 1.0f;

    // Graphics settings
    [Export] private bool _fullscreen = false; 
    [Export] private bool _vsync = true;
    [Export] private int _qualityLevel = 2; // 0=Low, 1=Medium, 2=High
    [Export] private bool _showFps = false; 
    [Export] private bool _showDamageNumbers = true;
    [Export] private bool _showComboTrails = true; // REQ-130: combo echo trail

    // Game settings
    [Export] private int _difficulty = 1; // 0=Easy, 1=Normal, 2=Hard
    [Export] private bool _autoSave = true;
    [Export] private int _autoSaveInterval = 300; // seconds
    [Export] private bool _showTutorials = true;
    [Export] private bool _showDamageNumbersOnUi = true;
    [Export] private float _uiScale = 1.0f;

    // Accessibility settings
    [Export] private bool _screenShake = true;
    [Export] private bool _hitStop = true;
    [Export] private bool _controllerVibration = true;

    // Key bindings (Dictionary not Godot-exportable; managed via code)
    private Dictionary<string, string> _keyBindings = new Dictionary<string, string>()
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
        Priority = 100; // Early loading
        
        LoadSettings();
    }

    public override void _ExitTree()
    {
        Instance = null;
    }

    #region Audio Control

    /// <summary>
    /// Master volume control (0.0 to 1.0).
    /// </summary>
    public float MasterVolume
    {
        get => _masterVolume;
        set { _masterVolume = Mathf.Clamp(value, 0f, 1f); }
    }

    /// <summary>
    /// Music volume control (0.0 to 1.0).
    /// </summary>
    public float MusicVolume
    {
        get => _musicVolume;
        set { _musicVolume = Mathf.Clamp(value, 0f, 1f); }
    }

    /// <summary>
    /// Sound effects volume control (0.0 to 1.0).
    /// </summary>
    public float SfxVolume
    {
        get => _sfxVolume;
        set { _sfxVolume = Mathf.Clamp(value, 0f, 1f); }
    }

    /// <summary>
    /// Voice volume control (0.0 to 1.0).
    /// </summary>
    public float VoiceVolume
    {
        get => _voiceVolume;
        set { _voiceVolume = Mathf.Clamp(value, 0f, 1f); }
    }

    /// <summary>
    /// Gets the effective volume for a specific audio type considering master volume.
    /// </summary>
    /// <param name="volumeType">The volume level of the specific audio type.</param>
    /// <returns>The effective volume after applying master volume.</returns>
    public float GetEffectiveVolume(float volumeType)
    {
        return volumeType * _masterVolume;
    }

    #endregion

    #region Graphics Settings

    /// <summary>
    /// Fullscreen mode toggle.
    /// </summary>
    public bool Fullscreen
    {
        get => _fullscreen;
        set 
        { 
            _fullscreen = value;
            ApplyDisplaySettings();
        }
    }

    /// <summary>
    /// Vertical sync toggle.
    /// </summary>
    public bool Vsync
    {
        get => _vsync;
        set 
        { 
            _vsync = value;
            ApplyDisplaySettings();
        }
    }

    /// <summary>
    /// Graphics quality level (0=Low, 1=Medium, 2=High).
    /// </summary>
    public int QualityLevel
    {
        get => _qualityLevel;
        set => _qualityLevel = Mathf.Clamp(value, 0, 2);
    }

    /// <summary>
    /// Show FPS counter toggle.
    /// </summary>
    public bool ShowFps
    {
        get => _showFps;
        set => _showFps = value;
    }

    /// <summary>
    /// Show damage numbers toggle.
    /// </summary>
    public bool ShowDamageNumbers
    {
        get => _showDamageNumbers;
        set => _showDamageNumbers = value;
    }

    /// <summary>
    /// Show combo echo trails (REQ-130).
    /// </summary>
    public bool ShowComboTrails
    {
        get => _showComboTrails;
        set
        {
            _showComboTrails = value;
            if (ComboEchoTrailSystem.Instance != null)
                ComboEchoTrailSystem.Instance.SetShowComboTrails(value);
        }
    }

    private void ApplyDisplaySettings()
    {
        var mode = _fullscreen ? Window.Mode.Fullscreen : Window.Mode.Windowed;
        GetWindow().Mode = mode;
        DisplayServer.WindowSetVsyncMode(_vsync ? DisplayServer.VSyncMode.Enabled : DisplayServer.VsyncMode.Disabled);
    }

    #endregion

    #region Game Settings

    /// <summary>
    /// Game difficulty level (0=Easy, 1=Normal, 2=Hard).
    /// </summary>
    public int Difficulty
    {
        get => _difficulty;
        set => _difficulty = Mathf.Clamp(value, 0, 2);
    }

    /// <summary>
    /// Gets the difficulty name as a localized string.
    /// </summary>
    /// <value>Easy, Normal, or Hard based on difficulty setting.</value>
    public string DifficultyName => _difficulty switch
    {
        0 => "简单",
        1 => "普通",
        2 => "困难",
        _ => "普通"
    };

    /// <summary>
    /// Gets the damage multiplier based on difficulty.
    /// </summary>
    /// <value>Multiplier applied to damage calculations.</value>
    public float DifficultyMultiplier => _difficulty switch
    {
        0 => 0.8f,
        1 => 1.0f,
        2 => 1.5f,
        _ => 1.0f
    };

    /// <summary>
    /// Auto-save toggle.
    /// </summary>
    public bool AutoSave
    {
        get => _autoSave;
        set => _autoSave = value;
    }

    /// <summary>
    /// Auto-save interval in seconds.
    /// </summary>
    public int AutoSaveInterval
    {
        get => _autoSaveInterval;
        set => _autoSaveInterval = Mathf.Max(value, 60);
    }

    /// <summary>
    /// Show tutorials toggle.
    /// </summary>
    public bool ShowTutorials
    {
        get => _showTutorials;
        set => _showTutorials = value;
    }

    /// <summary>
    /// UI scale factor (0.5 to 2.0).
    /// </summary>
    public float UiScale
    {
        get => _uiScale;
        set => _uiScale = Mathf.Clamp(value, 0.5f, 2.0f);
    }

    #endregion

    #region Accessibility

    /// <summary>
    /// Screen shake effect toggle.
    /// </summary>
    public bool ScreenShake
    {
        get => _screenShake;
        set => _screenShake = value;
    }

    /// <summary>
    /// Hit stop effect toggle.
    /// </summary>
    public bool HitStop
    {
        get => _hitStop;
        set => _hitStop = value;
    }

    /// <summary>
    /// Controller vibration toggle.
    /// </summary>
    public bool ControllerVibration
    {
        get => _controllerVibration;
        set => _controllerVibration = value;
    }

    #endregion

    #region Key Bindings

    /// <summary>
    /// Dictionary of key bindings (action -> key code).
    /// </summary>
    public Dictionary<string, string> KeyBindings
    {
        get => _keyBindings;
        set => _keyBindings = value;
    }

    /// <summary>
    /// Gets the key binding for a specific action.
    /// </summary>
    /// <param name="action">The action to look up.</param>
    /// <returns>The key code bound to the action, or empty string if not found.</returns>
    public string GetKeyBinding(string action)
    {
        return _keyBindings.ContainsKey(action) ? _keyBindings[action] : "";
    }

    /// <summary>
    /// Sets a key binding for a specific action.
    /// </summary>
    /// <param name="action">The action to bind.</param>
    /// <param name="key">The key code to bind.</param>
    public void SetKeyBinding(string action, string key)
    {
        _keyBindings[action] = key;
    }

    #endregion

    #region Save/Load

    /// <summary>
    /// Saves game settings to the configuration file.
    /// </summary>
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

    /// <summary>
    /// Loads game settings from the configuration file.
    /// </summary>
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

    /// <summary>
    /// Resets all settings to their default values.
    /// </summary>
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

    #region Difficulty Related

    /// <summary>
    /// Gets the damage multiplier based on difficulty setting.
    /// </summary>
    /// <param name="isPlayerDamage">True for player damage, false for enemy damage.</param>
    /// <returns>Damage multiplier based on difficulty.</returns>
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
    /// Gets the experience multiplier based on difficulty.
    /// </summary>
    /// <returns>Experience multiplier (higher for easier difficulties).</returns>
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
    /// Gets the drop rate multiplier based on difficulty.
    /// </summary>
    /// <returns>Drop rate multiplier (higher for easier difficulties).</returns>
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

    #region Persistence

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // Audio settings
        data["master_volume"] = _masterVolume;
        data["music_volume"] = _musicVolume;
        data["sfx_volume"] = _sfxVolume;
        data["voice_volume"] = _voiceVolume;
        
        // Graphics settings
        data["fullscreen"] = _fullscreen;
        data["vsync"] = _vsync;
        data["quality_level"] = _qualityLevel;
        data["show_fps"] = _showFps;
        data["show_damage_numbers"] = _showDamageNumbers;
        
        // Game settings
        data["difficulty"] = _difficulty;
        data["auto_save"] = _autoSave;
        data["auto_save_interval"] = _autoSaveInterval;
        data["show_tutorials"] = _showTutorials;
        data["ui_scale"] = _uiScale;
        
        // Accessibility settings
        data["screen_shake"] = _screenShake;
        data["hit_stop"] = _hitStop;
        data["controller_vibration"] = _controllerVibration;
        
        // Key bindings
        var keyBindingsArray = new Godot.Array();
        foreach (var kvp in _keyBindings)
        {
            var binding = new Dictionary<string, object>();
            binding["action"] = kvp.Key;
            binding["key"] = kvp.Value;
            keyBindingsArray.Add(binding);
        }
        data["key_bindings"] = keyBindingsArray;
        
        return data;
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        // Audio settings
        if (data.Contains("master_volume")) _masterVolume = Convert.ToSingle(data["master_volume"]);
        if (data.Contains("music_volume")) _musicVolume = Convert.ToSingle(data["music_volume"]);
        if (data.Contains("sfx_volume")) _sfxVolume = Convert.ToSingle(data["sfx_volume"]);
        if (data.Contains("voice_volume")) _voiceVolume = Convert.ToSingle(data["voice_volume"]);
        
        // Graphics settings
        if (data.Contains("fullscreen")) _fullscreen = Convert.ToBoolean(data["fullscreen"]);
        if (data.Contains("vsync")) _vsync = Convert.ToBoolean(data["vsync"]);
        if (data.Contains("quality_level")) _qualityLevel = Convert.ToInt32(data["quality_level"]);
        if (data.Contains("show_fps")) _showFps = Convert.ToBoolean(data["show_fps"]);
        if (data.Contains("show_damage_numbers")) _showDamageNumbers = Convert.ToBoolean(data["show_damage_numbers"]);
        
        // Game settings
        if (data.Contains("difficulty")) _difficulty = Convert.ToInt32(data["difficulty"]);
        if (data.Contains("auto_save")) _autoSave = Convert.ToBoolean(data["auto_save"]);
        if (data.Contains("auto_save_interval")) _autoSaveInterval = Convert.ToInt32(data["auto_save_interval"]);
        if (data.Contains("show_tutorials")) _showTutorials = Convert.ToBoolean(data["show_tutorials"]);
        if (data.Contains("ui_scale")) _uiScale = Convert.ToSingle(data["ui_scale"]);
        
        // Accessibility settings
        if (data.Contains("screen_shake")) _screenShake = Convert.ToBoolean(data["screen_shake"]);
        if (data.Contains("hit_stop")) _hitStop = Convert.ToBoolean(data["hit_stop"]);
        if (data.Contains("controller_vibration")) _controllerVibration = Convert.ToBoolean(data["controller_vibration"]);
        
        // Key bindings
        if (data.Contains("key_bindings"))
        {
            var bindings = (Godot.Array)data["key_bindings"];
            foreach (Dictionary binding in bindings)
            {
                string action = (string)binding["action"];
                string key = (string)binding["key"];
                _keyBindings[action] = key;
            }
        }
        
        // Apply display settings after loading
        ApplyDisplaySettings();
    }

    #endregion
}
