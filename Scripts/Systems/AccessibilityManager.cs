using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems {
    /// <summary>
    /// 无障碍设置系统 - 辅助功能配置
    /// 包含：颜色盲模式、高对比度、UI缩放、文字大小、字幕系统等
    /// </summary>
    public class AccessibilityManager : BaseSystem
    {
        public static AccessibilityManager Instance { get; private set; }

        // 颜色盲模式类型
        public enum ColorBlindMode {
            None = 0,
            RedGreen = 1,      // 红绿色盲
            BlueYellow = 2     // 蓝黄色盲
        }

        // UI缩放级别
        public enum UIScaleLevel {
            Small = 80,
            Normal = 100,
            Large = 120,
            ExtraLarge = 140
        }

        // 文字大小级别
        public enum TextSizeLevel {
            Small = 14,
            Normal = 16,
            Large = 20,
            ExtraLarge = 24
        }

        // 辅助功能设置
        private ColorBlindMode _colorBlindMode = ColorBlindMode.None;
        private bool _highContrastMode = false;
        private UIScaleLevel _uiScale = UIScaleLevel.Normal;
        private TextSizeLevel _textSize = TextSizeLevel.Normal;
        private bool _subtitlesEnabled = true;
        private bool _soundVisualization = true;
        private bool _simplifiedControls = false;
        private bool _autoPotionEnabled = true;
        private bool _damageNumbersEnabled = true;
        private float _uiVolume = 1.0f;
        private float _musicVolume = 1.0f;
        private float _sfxVolume = 1.0f;

        // 事件信号
        [Signal]
        public delegate void AccessibilitySettingsChanged();
        
        [Signal]
        public delegate void ColorBlindModeChanged(ColorBlindMode mode);
        
        [Signal]
        public delegate void UIScaleChanged(UIScaleLevel scale);
        
        [Signal]
        public delegate void TextSizeChanged(TextSizeLevel size);

        public override void _Ready()
        {
            Instance = this;
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "Accessibility";

        // 属性访问器
        public ColorBlindMode ColorBlind {
            get => _colorBlindMode;
            set {
                _colorBlindMode = value;
                EmitSignal(nameof(ColorBlindModeChanged), value);
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public bool HighContrast {
            get => _highContrastMode;
            set {
                _highContrastMode = value;
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public UIScaleLevel UIScale {
            get => _uiScale;
            set {
                _uiScale = value;
                EmitSignal(nameof(UIScaleChanged), value);
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public TextSizeLevel TextSize {
            get => _textSize;
            set {
                _textSize = value;
                EmitSignal(nameof(TextSizeChanged), value);
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public bool SubtitlesEnabled {
            get => _subtitlesEnabled;
            set {
                _subtitlesEnabled = value;
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public bool SoundVisualization {
            get => _soundVisualization;
            set {
                _soundVisualization = value;
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public bool SimplifiedControls {
            get => _simplifiedControls;
            set {
                _simplifiedControls = value;
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public bool AutoPotionEnabled {
            get => _autoPotionEnabled;
            set {
                _autoPotionEnabled = value;
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public bool DamageNumbersEnabled {
            get => _damageNumbersEnabled;
            set {
                _damageNumbersEnabled = value;
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public float UIVolume {
            get => _uiVolume;
            set {
                _uiVolume = Mathf.Clamp(value, 0f, 1f);
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public float MusicVolume {
            get => _musicVolume;
            set {
                _musicVolume = Mathf.Clamp(value, 0f, 1f);
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        public float SFXVolume {
            get => _sfxVolume;
            set {
                _sfxVolume = Mathf.Clamp(value, 0f, 1f);
                EmitSignal(nameof(AccessibilitySettingsChanged));
            }
        }

        /// <summary>
        /// 获取当前UI缩放因子
        /// </summary>
        public float GetUIScaleFactor() {
            return (float)_uiScale / 100f;
        }

        /// <summary>
        /// 获取当前文字大小
        /// </summary>
        public int GetTextSize() {
            return (int)_textSize;
        }

        /// <summary>
        /// 获取颜色盲模式对应的颜色调整
        /// </summary>
        public Color GetAdjustedColor(Color original) {
            switch (_colorBlindMode) {
                case ColorBlindMode.RedGreen:
                    // 红绿色盲友好 - 增强蓝黄色调
                    return new Color(
                        original.r * 0.7f + 0.1f,
                        original.g * 0.7f + 0.1f,
                        original.b * 1.3f
                    );
                case ColorBlindMode.BlueYellow:
                    // 蓝黄色盲友好 - 增强红绿色调
                    return new Color(
                        original.r * 1.3f,
                        original.g * 1.3f,
                        original.b * 0.7f + 0.1f
                    );
                default:
                    return original;
            }
        }

        /// <summary>
        /// 获取高对比度颜色
        /// </summary>
        public Color GetHighContrastColor(Color normal, Color highContrast) {
            return _highContrastMode ? highContrast : normal;
        }

        /// <summary>
        /// 重置所有设置为默认值
        /// </summary>
        public void ResetToDefaults()
        {
            _colorBlindMode = ColorBlindMode.None;
            _highContrastMode = false;
            _uiScale = UIScaleLevel.Normal;
            _textSize = TextSizeLevel.Normal;
            _subtitlesEnabled = true;
            _soundVisualization = true;
            _simplifiedControls = false;
            _autoPotionEnabled = true;
            _damageNumbersEnabled = true;
            _uiVolume = 1.0f;
            _musicVolume = 1.0f;
            _sfxVolume = 1.0f;
            
            EmitSignal(nameof(AccessibilitySettingsChanged));
        }

        /// <summary>
        /// 导出保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return Serialize();
        }

        /// <summary>
        /// 导入保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            Deserialize(new Dictionary<string, object>(data));
        }

        /// <summary>
        /// 序列化设置数据
        /// </summary>
        public Dictionary<string, object> Serialize()
        {
            return new Dictionary<string, object>
            {
                ["colorBlindMode"] = (int)_colorBlindMode,
                ["highContrastMode"] = _highContrastMode,
                ["uiScale"] = (int)_uiScale,
                ["textSize"] = (int)_textSize,
                ["subtitlesEnabled"] = _subtitlesEnabled,
                ["soundVisualization"] = _soundVisualization,
                ["simplifiedControls"] = _simplifiedControls,
                ["autoPotionEnabled"] = _autoPotionEnabled,
                ["damageNumbersEnabled"] = _damageNumbersEnabled,
                ["uiVolume"] = _uiVolume,
                ["musicVolume"] = _musicVolume,
                ["sfxVolume"] = _sfxVolume
            };
        }

        /// <summary>
        /// 反序列化设置数据
        /// </summary>
        public void Deserialize(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.ContainsKey("colorBlindMode"))
                _colorBlindMode = (ColorBlindMode)(int)data["colorBlindMode"];
            if (data.ContainsKey("highContrastMode"))
                _highContrastMode = (bool)data["highContrastMode"];
            if (data.ContainsKey("uiScale"))
                _uiScale = (UIScaleLevel)(int)data["uiScale"];
            if (data.ContainsKey("textSize"))
                _textSize = (TextSizeLevel)(int)data["textSize"];
            if (data.ContainsKey("subtitlesEnabled"))
                _subtitlesEnabled = (bool)data["subtitlesEnabled"];
            if (data.ContainsKey("soundVisualization"))
                _soundVisualization = (bool)data["soundVisualization"];
            if (data.ContainsKey("simplifiedControls"))
                _simplifiedControls = (bool)data["simplifiedControls"];
            if (data.ContainsKey("autoPotionEnabled"))
                _autoPotionEnabled = (bool)data["autoPotionEnabled"];
            if (data.ContainsKey("damageNumbersEnabled"))
                _damageNumbersEnabled = (bool)data["damageNumbersEnabled"];
            if (data.ContainsKey("uiVolume"))
                _uiVolume = (float)(double)data["uiVolume"];
            if (data.ContainsKey("musicVolume"))
                _musicVolume = (float)(double)data["musicVolume"];
            if (data.ContainsKey("sfxVolume"))
                _sfxVolume = (float)(double)data["sfxVolume"];
        }
    }
}
