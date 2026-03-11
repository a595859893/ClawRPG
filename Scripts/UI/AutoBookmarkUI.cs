using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Auto bookmark settings UI - allows players to configure auto bookmarking
    /// </summary>
    public partial class AutoBookmarkUI : Control {
        private Control _panel;
        private VBoxContainer _mainContainer;
        private CheckBox _bossCheckBox;
        private CheckBox _shopCheckBox;
        private CheckBox _questCheckBox;
        private CheckBox _waypointCheckBox;
        private Label _titleLabel;
        private Button _closeButton;
        private Button _clearButton;
        
        private bool _isVisible = false; 
        
        public override void _Ready() {
            SetupUI();
            Hide();
        }
        
        private void SetupUI() {
            // Main panel
            _panel = new Control {
                AnchorRight = 1f,
                AnchorBottom = 1f,
                MouseFilter = Control.MouseFilterEnum.Stop
            };
            AddChild(_panel);
            
            // Background panel
            var bgPanel = new PanelContainer {
                AnchorLeft = 0.5f,
                AnchorTop = 0.5f,
                AnchorRight = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -200,
                OffsetTop = -180,
                OffsetRight = 200,
                OffsetBottom = 180,
                CustomMinimumSize = new Vector2(400, 360)
            };
            _panel.AddChild(bgPanel);
            
            var style = new StyleBoxFlat {
                BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
                BorderColor = new Color(0.3f, 0.3f, 0.4f),
                BorderWidthLeft = 2,
                BorderWidthTop = 2,
                BorderWidthRight = 2,
                BorderWidthBottom = 2,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8
            };
            bgPanel.AddThemeStyleboxOverride("panel", style);
            
            // Main container
            _mainContainer = new VBoxContainer {
                AnchorLeft = 0f,
                AnchorTop = 0f,
                AnchorRight = 1f,
                AnchorBottom = 1f,
                OffsetLeft = 20,
                OffsetTop = 20,
                OffsetRight = -20,
                OffsetBottom = -20
            };
            bgPanel.AddChild(_mainContainer);
            
            // Title
            _titleLabel = new Label {
                Text = "⚡ 自动收藏点设置",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 40)
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
            _mainContainer.AddChild(_titleLabel);
            
            // Separator
            var sep1 = new HSeparator { CustomMinimumSize = new Vector2(0, 10) };
            _mainContainer.AddChild(sep1);
            
            // Description
            var descLabel = new Label {
                Text = "自动收藏游戏中的重要位置",
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            _mainContainer.AddChild(descLabel);
            
            // Separator
            var sep2 = new HSeparator { CustomMinimumSize = new Vector2(0, 15) };
            _mainContainer.AddChild(sep2);
            
            // Options
            var optionsContainer = new VBoxContainer {
                CustomMinimumSize = new Vector2(0, 150)
            };
            _mainContainer.AddChild(optionsContainer);
            
            // Boss checkbox
            _bossCheckBox = CreateCheckBox("⚔️ 自动收藏 Boss 位置", true);
            _bossCheckBox.Toggled += OnBossToggled;
            optionsContainer.AddChild(_bossCheckBox);
            
            // Shop checkbox
            _shopCheckBox = CreateCheckBox("🛒 自动收藏商店位置", true);
            _shopCheckBox.Toggled += OnShopToggled;
            optionsContainer.AddChild(_shopCheckBox);
            
            // Quest checkbox
            _questCheckBox = CreateCheckBox("📋 自动收藏任务目标", true);
            _questCheckBox.Toggled += OnQuestToggled;
            optionsContainer.AddChild(_questCheckBox);
            
            // Waypoint checkbox
            _waypointCheckBox = CreateCheckBox("✨ 自动收藏传送点", true);
            _waypointCheckBox.Toggled += OnWaypointToggled;
            optionsContainer.AddChild(_waypointCheckBox);
            
            // Separator
            var sep3 = new HSeparator { CustomMinimumSize = new Vector2(0, 15) };
            _mainContainer.AddChild(sep3);
            
            // Buttons container
            var buttonsContainer = new HBoxContainer {
                Alignment = BoxContainer.AlignmentMode.Center,
                CustomMinimumSize = new Vector2(0, 40)
            };
            _mainContainer.AddChild(buttonsContainer);
            
            // Clear button
            _clearButton = new Button {
                Text = "🗑️ 清除记录",
                CustomMinimumSize = new Vector2(120, 35)
            };
            _clearButton.Pressed += OnClearPressed;
            buttonsContainer.AddChild(_clearButton);
            
            // Spacer
            var spacer = new Control { CustomMinimumSize = new Vector2(20, 0) };
            buttonsContainer.AddChild(spacer);
            
            // Close button
            _closeButton = new Button {
                Text = "关闭",
                CustomMinimumSize = new Vector2(80, 35)
            };
            _closeButton.Pressed += OnClosePressed;
            buttonsContainer.AddChild(_closeButton);
            
            // Add some spacing
            _mainContainer.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
            
            // Load current settings
            LoadSettings();
        }
        
        private CheckBox CreateCheckBox(string text, bool defaultValue) {
            var checkBox = new CheckBox {
                Text = text,
                ButtonPressed = defaultValue,
                CustomMinimumSize = new Vector2(0, 30)
            };
            checkBox.AddThemeFontSizeOverride("font_size", 16);
            return checkBox;
        }
        
        private void LoadSettings() {
            var autoBookmark = AutoBookmarkSystem.Instance;
            if (autoBookmark != null) {
                _bossCheckBox.ButtonPressed = autoBookmark.AutoBookmarkBoss;
                _shopCheckBox.ButtonPressed = autoBookmark.AutoBookmarkShop;
                _questCheckBox.ButtonPressed = autoBookmark.AutoBookmarkQuest;
                _waypointCheckBox.ButtonPressed = autoBookmark.AutoBookmarkWaypoint;
            }
        }
        
        private void OnBossToggled(bool pressed) {
            var autoBookmark = AutoBookmarkSystem.Instance;
            if (autoBookmark != null) {
                autoBookmark.AutoBookmarkBoss = pressed;
            }
        }
        
        private void OnShopToggled(bool pressed) {
            var autoBookmark = AutoBookmarkSystem.Instance;
            if (autoBookmark != null) {
                autoBookmark.AutoBookmarkShop = pressed;
            }
        }
        
        private void OnQuestToggled(bool pressed) {
            var autoBookmark = AutoBookmarkSystem.Instance;
            if (autoBookmark != null) {
                autoBookmark.AutoBookmarkQuest = pressed;
            }
        }
        
        private void OnWaypointToggled(bool pressed) {
            var autoBookmark = AutoBookmarkSystem.Instance;
            if (autoBookmark != null) {
                autoBookmark.AutoBookmarkWaypoint = pressed;
            }
        }
        
        private void OnClearPressed() {
            var autoBookmark = AutoBookmarkSystem.Instance;
            if (autoBookmark != null) {
                autoBookmark.ClearDiscoveredLocations();
                GD.Print("[AutoBookmarkUI] Cleared auto bookmark records");
            }
        }
        
        private void OnClosePressed() {
            Toggle();
        }
        
        public void Toggle() {
            _isVisible = !_isVisible;
            if (_isVisible) {
                Show();
                LoadSettings();
            } else {
                Hide();
            }
        }
        
        public override void _Input(InputEvent e) {
            if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape) {
                if (_isVisible) {
                    Toggle();
                    GetTree().SetInputAsHandled();
                }
            }
        }
    }
}
