using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Hotkey Help UI - displays control hints
    /// </summary>
    public class HotkeyHelpUI : Control
    {
        private VBoxContainer _container;
        private bool _isVisible = true;
        
        public override void _Ready()
        {
            SetupUI();
        }
        
        public override void _Input(InputEvent evt)
        {
            // Toggle with H key
            if (evt is InputEventKey key && key.Pressed && key.Keycode == Key.H)
            {
                ToggleVisibility();
            }
        }
        
        private void SetupUI()
        {
            // Main container - bottom right corner
            _container = new VBoxContainer();
            _container.SetAnchor(AnchorPresets.BottomRight);
            _container.Position = new Vector2(-220, -200);
            _container.CustomMinimumSize = new Vector2(200, 180);
            AddChild(_container);
            
            // Title
            var title = new Label();
            title.Text = "操作说明";
            title.AddThemeFontSizeOverride("font_size", 16);
            title.AddThemeColorOverride("font_color", new Color(1, 0.85, 0.3));
            _container.AddChild(title);
            
            // Movement
            AddHotkey("移动", "WASD / 方向键");
            AddHotkey("攻击", "J / 鼠标左键");
            AddHotkey("格挡", "K / 鼠标右键");
            AddHotkey("闪避", "L / Shift");
            AddHotkey("技能", "1-6 数字键");
            AddHotkey("背包", "I");
            AddHotkey("技能栏", "K");
            AddHotkey("任务", "Q");
            AddHotkey("宠物", "P");
            AddHotkey("成就", "L");
            AddHotkey("合成", "C");
            AddHotkey("世界地图", "M");
            AddHotkey("统计", "Z");
            AddHotkey("任务指引", "G");
            AddHotkey("暂停", "ESC");
            
            // Toggle hint
            var toggleHint = new Label();
            toggleHint.Text = "(按 H 切换显示)";
            toggleHint.AddThemeFontSizeOverride("font_size", 12);
            toggleHint.AddThemeColorOverride("font_color", new Color(0.6, 0.6, 0.6));
            _container.AddChild(toggleHint);
        }
        
        private void AddHotkey(string action, string key)
        {
            var hbox = new HBoxContainer();
            hbox.CustomMinimumSize = new Vector2(0, 20);
            _container.AddChild(hbox);
            
            var actionLabel = new Label();
            actionLabel.Text = action + ": ";
            actionLabel.AddThemeFontSizeOverride("font_size", 12);
            actionLabel.AddThemeColorOverride("font_color", new Color(0.9, 0.9, 0.9));
            actionLabel.CustomMinimumSize = new Vector2(60, 0);
            hbox.AddChild(actionLabel);
            
            var keyLabel = new Label();
            keyLabel.Text = key;
            keyLabel.AddThemeFontSizeOverride("font_size", 12);
            keyLabel.AddThemeColorOverride("font_color", new Color(0.5, 0.8, 1));
            hbox.AddChild(keyLabel);
        }
        
        private void ToggleVisibility()
        {
            _isVisible = !_isVisible;
            _container.Visible = _isVisible;
        }
    }
}
