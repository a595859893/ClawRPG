using Godot;
using System;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 自动药水设置界面 - 允许玩家配置自动使用药水的行为
    /// </summary>
    public partial class AutoPotionUI : Control
    {
        private Label _titleLabel;
        private CheckBox _autoHealthCheck;
        private CheckBox _autoManaCheck;
        private CheckBox _autoBuffCheck;
        private HSlider _healthThresholdSlider;
        private HSlider _manaThresholdSlider;
        private Label _healthThresholdValue;
        private Label _manaThresholdValue;
        private Label _healthDescLabel;
        private Label _manaDescLabel;
        
        private bool _isVisible = false;

        public override void _Ready()
        {
            // 创建UI元素
            CreateUI();
            
            // 初始状态隐藏
            Hide();
        }

        private void CreateUI()
        {
            // 主容器
            var mainPanel = new PanelContainer
            {
                AnchorLeft = 0.5f,
                AnchorRight = 0.5f,
                AnchorTop = 0.3f,
                AnchorBottom = 0.7f,
                OffsetLeft = -200,
                OffsetRight = 200,
                OffsetTop = -150,
                OffsetBottom = 150
            };
            AddChild(mainPanel);

            var mainVBox = new VBoxContainer
            {
                CustomMinimumSize = new Vector2(400, 300)
            };
            mainPanel.AddChild(mainVBox);

            // 标题
            _titleLabel = new Label
            {
                Text = "⚗️ 自动药水设置",
                HorizontalAlignment = HorizontalAlignment.Center,
                CustomMinimumSize = new Vector2(0, 40)
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            mainVBox.AddChild(_titleLabel);

            // 分隔线
            var separator = new HSeparator();
            mainVBox.AddChild(separator);

            // 自动使用生命药水
            _autoHealthCheck = new CheckBox
            {
                Text = "自动使用生命药水",
                ButtonPressed = AutoPotionSystem.Instance?.AutoUseHealthPotion ?? true
            };
            _autoHealthCheck.Toggled += OnAutoHealthToggled;
            mainVBox.AddChild(_autoHealthCheck);

            // 生命药水阈值
            var healthBox = new HBoxContainer();
            mainVBox.AddChild(healthBox);

            _healthDescLabel = new Label
            {
                Text = "生命阈值:",
                CustomMinimumSize = new Vector2(100, 0)
            };
            healthBox.AddChild(_healthDescLabel);

            _healthThresholdSlider = new HSlider
            {
                MinValue = 5,
                MaxValue = 95,
                Value = AutoPotionSystem.Instance?.HealthPotionThreshold ?? 30,
                CustomMinimumSize = new Vector2(200, 0)
            };
            _healthThresholdSlider.ValueChanged += OnHealthThresholdChanged;
            healthBox.AddChild(_healthThresholdSlider);

            _healthThresholdValue = new Label
            {
                Text = $"{_healthThresholdSlider.Value}%",
                CustomMinimumSize = new Vector2(50, 0)
            };
            healthBox.AddChild(_healthThresholdValue);

            // 自动使用魔法药水
            _autoManaCheck = new CheckBox
            {
                Text = "自动使用魔法药水",
                ButtonPressed = AutoPotionSystem.Instance?.AutoUseManaPotion ?? true
            };
            _autoManaCheck.Toggled += OnAutoManaToggled;
            mainVBox.AddChild(_autoManaCheck);

            // 魔法药水阈值
            var manaBox = new HBoxContainer();
            mainVBox.AddChild(manaBox);

            _manaDescLabel = new Label
            {
                Text = "魔法阈值:",
                CustomMinimumSize = new Vector2(100, 0)
            };
            manaBox.AddChild(_manaDescLabel);

            _manaThresholdSlider = new HSlider
            {
                MinValue = 5,
                MaxValue = 95,
                Value = AutoPotionSystem.Instance?.ManaPotionThreshold ?? 30,
                CustomMinimumSize = new Vector2(200, 0)
            };
            _manaThresholdSlider.ValueChanged += OnManaThresholdChanged;
            manaBox.AddChild(_manaThresholdSlider);

            _manaThresholdValue = new Label
            {
                Text = $"{_manaThresholdSlider.Value}%",
                CustomMinimumSize = new Vector2(50, 0)
            };
            manaBox.AddChild(_manaThresholdValue);

            // 自动使用增益药水
            _autoBuffCheck = new CheckBox
            {
                Text = "自动使用增益药水 (30秒冷却)",
                ButtonPressed = AutoPotionSystem.Instance?.AutoUseBuffPotions ?? false
            };
            _autoBuffCheck.Toggled += OnAutoBuffToggled;
            mainVBox.AddChild(_autoBuffCheck);

            // 说明文字
            var infoLabel = new Label
            {
                Text = "当背包中有药水时，系统会根据阈值自动使用",
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            infoLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            mainVBox.AddChild(infoLabel);

            // 关闭按钮
            var closeButton = new Button
            {
                Text = "关闭 (X)",
                CustomMinimumSize = new Vector2(0, 40)
            };
            closeButton.Pressed += ToggleVisibility;
            mainVBox.AddChild(closeButton);
        }

        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                // X键切换显示
                if (keyEvent.Keycode == Key.X)
                {
                    var main = GetTree().GetFirstNodeInGroup("Main");
                    if (main != null)
                    {
                        // 检查是否按住Shift
                        if (Input.IsKeyPressed(Key.Shift))
                        {
                            ToggleVisibility();
                        }
                    }
                }
            }
        }

        public void ToggleVisibility()
        {
            if (_isVisible)
            {
                Hide();
                _isVisible = false;
            }
            else
            {
                Show();
                _isVisible = true;
                RefreshValues();
            }
        }

        private void RefreshValues()
        {
            if (AutoPotionSystem.Instance == null) return;

            _autoHealthCheck.ButtonPressed = AutoPotionSystem.Instance.AutoUseHealthPotion;
            _autoManaCheck.ButtonPressed = AutoPotionSystem.Instance.AutoUseManaPotion;
            _autoBuffCheck.ButtonPressed = AutoPotionSystem.Instance.AutoUseBuffPotions;
            _healthThresholdSlider.Value = AutoPotionSystem.Instance.HealthPotionThreshold;
            _manaThresholdSlider.Value = AutoPotionSystem.Instance.ManaPotionThreshold;
            _healthThresholdValue.Text = $"{_healthThresholdSlider.Value}%";
            _manaThresholdValue.Text = $"{_manaThresholdSlider.Value}%";
        }

        private void OnAutoHealthToggled(bool toggled)
        {
            AutoPotionSystem.Instance?.SetAutoHealthPotion(toggled);
        }

        private void OnAutoManaToggled(bool toggled)
        {
            AutoPotionSystem.Instance?.SetAutoManaPotion(toggled);
        }

        private void OnAutoBuffToggled(bool toggled)
        {
            AutoPotionSystem.Instance?.SetAutoBuffPotions(toggled);
        }

        private void OnHealthThresholdChanged(double value)
        {
            _healthThresholdValue.Text = $"{value}%";
            AutoPotionSystem.Instance?.SetHealthThreshold((int)value);
        }

        private void OnManaThresholdChanged(double value)
        {
            _manaThresholdValue.Text = $"{value}%";
            AutoPotionSystem.Instance?.SetManaThreshold((int)value);
        }
    }
}
