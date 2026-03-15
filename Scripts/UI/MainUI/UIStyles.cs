using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.UI
{
    /// <summary>
    /// UI 样式 - 管理 UI 主题和样式
    /// </summary>
    public partial class UIStyles : BaseSystem
    {
        /// <summary>
        /// 主题类型
        /// </summary>
        public enum ThemeType
        {
            Default,
            Dark,
            Light,
            Fantasy,
            SciFi
        }
        
        /// <summary>
        /// 颜色方案
        /// </summary>
        public class ColorScheme
        {
            public Color PrimaryColor { get; set; } = new Color(0.2f, 0.6f, 1.0f);
            public Color SecondaryColor { get; set; } = new Color(0.4f, 0.4f, 0.4f);
            public Color AccentColor { get; set; } = new Color(1.0f, 0.8f, 0.0f);
            public Color BackgroundColor { get; set; } = new Color(0.1f, 0.1f, 0.1f);
            public Color TextColor { get; set; } = new Color(1.0f, 1.0f, 1.0f);
            public Color SuccessColor { get; set; } = new Color(0.2f, 0.8f, 0.2f);
            public Color WarningColor { get; set; } = new Color(1.0f, 0.6f, 0.0f);
            public Color ErrorColor { get; set; } = new Color(1.0f, 0.2f, 0.2f);
        }
        
        private ThemeType _currentTheme = ThemeType.Default;
        private ColorScheme _currentColorScheme = new ColorScheme();
        private Dictionary<ThemeType, ColorScheme> _themeColors = new Dictionary<ThemeType, ColorScheme>();
        
        public override void _Ready()
        {
            base._Ready();
            InitializeThemes();
        }
        
        /// <summary>
        /// 初始化主题
        /// </summary>
        private void InitializeThemes()
        {
            // 默认主题
            _themeColors[ThemeType.Default] = new ColorScheme
            {
                PrimaryColor = new Color(0.2f, 0.6f, 1.0f),
                SecondaryColor = new Color(0.4f, 0.4f, 0.4f),
                AccentColor = new Color(1.0f, 0.8f, 0.0f),
                BackgroundColor = new Color(0.1f, 0.1f, 0.1f),
                TextColor = new Color(1.0f, 1.0f, 1.0f)
            };
            
            // 深色主题
            _themeColors[ThemeType.Dark] = new ColorScheme
            {
                PrimaryColor = new Color(0.3f, 0.5f, 0.9f),
                SecondaryColor = new Color(0.2f, 0.2f, 0.3f),
                AccentColor = new Color(0.9f, 0.7f, 0.1f),
                BackgroundColor = new Color(0.05f, 0.05f, 0.08f),
                TextColor = new Color(0.9f, 0.9f, 0.9f)
            };
            
            // 浅色主题
            _themeColors[ThemeType.Light] = new ColorScheme
            {
                PrimaryColor = new Color(0.0f, 0.4f, 0.8f),
                SecondaryColor = new Color(0.7f, 0.7f, 0.7f),
                AccentColor = new Color(0.8f, 0.6f, 0.0f),
                BackgroundColor = new Color(0.95f, 0.95f, 0.95f),
                TextColor = new Color(0.1f, 0.1f, 0.1f)
            };
            
            // 奇幻主题
            _themeColors[ThemeType.Fantasy] = new ColorScheme
            {
                PrimaryColor = new Color(0.5f, 0.3f, 0.7f),
                SecondaryColor = new Color(0.3f, 0.2f, 0.4f),
                AccentColor = new Color(1.0f, 0.84f, 0.0f),
                BackgroundColor = new Color(0.15f, 0.1f, 0.2f),
                TextColor = new Color(0.95f, 0.9f, 0.8f)
            };
            
            // 科幻主题
            _themeColors[ThemeType.SciFi] = new ColorScheme
            {
                PrimaryColor = new Color(0.0f, 0.8f, 1.0f),
                SecondaryColor = new Color(0.0f, 0.3f, 0.4f),
                AccentColor = new Color(1.0f, 0.2f, 0.5f),
                BackgroundColor = new Color(0.0f, 0.05f, 0.1f),
                TextColor = new Color(0.8f, 1.0f, 1.0f)
            };
            
            _currentColorScheme = _themeColors[ThemeType.Default];
        }
        
        /// <summary>
        /// 设置主题
        /// </summary>
        public void SetTheme(ThemeType theme)
        {
            _currentTheme = theme;
            if (_themeColors.ContainsKey(theme))
            {
                _currentColorScheme = _themeColors[theme];
                GD.Print($"[UIStyles] Theme changed to: {theme}");
            }
        }
        
        /// <summary>
        /// 获取当前主题
        /// </summary>
        public ThemeType GetCurrentTheme()
        {
            return _currentTheme;
        }
        
        /// <summary>
        /// 获取当前颜色方案
        /// </summary>
        public ColorScheme GetColorScheme()
        {
            return _currentColorScheme;
        }
        
        /// <summary>
        /// 获取主色
        /// </summary>
        public Color GetPrimaryColor()
        {
            return _currentColorScheme.PrimaryColor;
        }
        
        /// <summary>
        /// 获取次色
        /// </summary>
        public Color GetSecondaryColor()
        {
            return _currentColorScheme.SecondaryColor;
        }
        
        /// <summary>
        /// 获取强调色
        /// </summary>
        public Color GetAccentColor()
        {
            return _currentColorScheme.AccentColor;
        }
        
        /// <summary>
        /// 获取背景色
        /// </summary>
        public Color GetBackgroundColor()
        {
            return _currentColorScheme.BackgroundColor;
        }
        
        /// <summary>
        /// 获取文本色
        /// </summary>
        public Color GetTextColor()
        {
            return _currentColorScheme.TextColor;
        }
        
        /// <summary>
        /// 获取成功色
        /// </summary>
        public Color GetSuccessColor()
        {
            return _currentColorScheme.SuccessColor;
        }
        
        /// <summary>
        /// 获取警告色
        /// </summary>
        public Color GetWarningColor()
        {
            return _currentColorScheme.WarningColor;
        }
        
        /// <summary>
        /// 获取错误色
        /// </summary>
        public Color GetErrorColor()
        {
            return _currentColorScheme.ErrorColor;
        }
        
        /// <summary>
        /// 自定义颜色
        /// </summary>
        public void SetCustomColor(string colorType, Color color)
        {
            switch (colorType.ToLower())
            {
                case "primary":
                    _currentColorScheme.PrimaryColor = color;
                    break;
                case "secondary":
                    _currentColorScheme.SecondaryColor = color;
                    break;
                case "accent":
                    _currentColorScheme.AccentColor = color;
                    break;
                case "background":
                    _currentColorScheme.BackgroundColor = color;
                    break;
                case "text":
                    _currentColorScheme.TextColor = color;
                    break;
            }
        }
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            data["theme"] = (int)_currentTheme;
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("theme"))
            {
                SetTheme((ThemeType)(int)data["theme"]);
            }
        }
    }
}
