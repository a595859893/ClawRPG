using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.AI.Flocking
{
    /// <summary>
    /// Enemy Flocking UI - 控制面板
    /// 显示群体行为参数和统计信息
    /// </summary>
    public partial class EnemyFlockingUI : Control
    {
        private EnemyFlockingSystem _flockingSystem;
        private bool _isVisible = false;
        
        // UI 组件
        private Label _titleLabel;
        private Label _statsLabel;
        private HSlider _separationSlider;
        private HSlider _alignmentSlider;
        private HSlider _cohesionSlider;
        private HSlider _perceptionSlider;
        private HSlider _speedSlider;
        private Button _toggleButton;
        private Button _closeButton;
        
        public override void _Ready()
        {
            _flockingSystem = EnemyFlockingSystem.Instance;
            SetupUI();
            
            // 初始隐藏
            Visible = false;
            
            // 注册快捷键
            InputManager.Instance.RegisterAction("toggle_flocking_ui", new[] { Key.F }, ToggleVisibility);
        }
        
        private void SetupUI()
        {
            // 标题
            _titleLabel = new Label
            {
                Text = "Enemy Flocking System",
                Position = new Vector2(20, 20),
                AddThemeFontSizeOverride("font_size", 24)
            };
            AddChild(_titleLabel);
            
            // 统计信息
            _statsLabel = new Label
            {
                Position = new Vector2(20, 60),
                AddThemeFontSizeOverride("font_size", 16)
            };
            AddChild(_statsLabel);
            
            // 分离权重滑块
            CreateSlider("Separation Weight", 20, 140, _separationSlider = new HSlider
            {
                MinValue = 0,
                MaxValue = 5,
                Step = 0.1f,
                Value = 1.5f,
                Position = new Vector2(20, 200),
                Size = new Vector2(200, 20)
            });
            
            // 对齐权重滑块
            CreateSlider("Alignment Weight", 20, 260, _alignmentSlider = new HSlider
            {
                MinValue = 0,
                MaxValue = 5,
                Step = 0.1f,
                Value = 1.0f,
                Position = new Vector2(20, 260),
                Size = new Vector2(200, 20)
            });
            
            // 凝聚权重滑块
            CreateSlider("Cohesion Weight", 20, 320, _cohesionSlider = new HSlider
            {
                MinValue = 0,
                MaxValue = 5,
                Step = 0.1f,
                Value = 1.0f,
                Position = new Vector2(20, 320),
                Size = new Vector2(200, 20)
            });
            
            // 感知半径滑块
            CreateSlider("Perception Radius", 20, 380, _perceptionSlider = new HSlider
            {
                MinValue = 20,
                MaxValue = 300,
                Step = 10,
                Value = 100,
                Position = new Vector2(20, 380),
                Size = new Vector2(200, 20)
            });
            
            // 最大速度滑块
            CreateSlider("Max Speed", 20, 440, _speedSlider = new HSlider
            {
                MinValue = 10,
                MaxValue = 150,
                Step = 5,
                Value = 50,
                Position = new Vector2(20, 440),
                Size = new Vector2(200, 20)
            });
            
            // 切换按钮
            _toggleButton = new Button
            {
                Text = "Apply Settings",
                Position = new Vector2(20, 500),
                Size = new Vector2(150, 40)
            };
            _toggleButton.Pressed += OnApplyPressed;
            AddChild(_toggleButton);
            
            // 关闭按钮
            _closeButton = new Button
            {
                Text = "Close (ESC)",
                Position = new Vector2(180, 500),
                Size = new Vector2(120, 40)
            };
            _closeButton.Pressed += Hide;
            AddChild(_closeButton);
            
            // 连接滑块信号
            _separationSlider.ValueChanged += OnParameterChanged;
            _alignmentSlider.ValueChanged += OnParameterChanged;
            _cohesionSlider.ValueChanged += OnParameterChanged;
            _perceptionSlider.ValueChanged += OnParameterChanged;
            _speedSlider.ValueChanged += OnParameterChanged;
        }
        
        private void CreateSlider(string labelText, float x, float y, HSlider slider)
        {
            Label label = new Label
            {
                Text = labelText,
                Position = new Vector2(x, y - 30)
            };
            AddChild(label);
            AddChild(slider);
        }
        
        private void OnParameterChanged(double value)
        {
            UpdateStats();
        }
        
        private void OnApplyPressed()
        {
            if (_flockingSystem != null)
            {
                _flockingSystem.SetFlockingParameters(
                    (float)_separationSlider.Value,
                    (float)_alignmentSlider.Value,
                    (float)_cohesionSlider.Value
                );
            }
            UpdateStats();
        }
        
        private void UpdateStats()
        {
            if (_flockingSystem == null) return;
            
            var stats = _flockingSystem.GetStatistics();
            
            _statsLabel.Text = $"Statistics:\n" +
                $"Active Flocks: {stats["active_flocks"]}\n" +
                $"Total Members: {stats["total_flock_members"]}\n" +
                $"Flock Updates: {stats["flock_updates"]}\n\n" +
                $"Parameters:\n" +
                $"Separation: {_separationSlider.Value:F1}\n" +
                $"Alignment: {_alignmentSlider.Value:F1}\n" +
                $"Cohesion: {_cohesionSlider.Value:F1}\n" +
                $"Perception: {_perceptionSlider.Value:F0}\n" +
                $"Max Speed: {_speedSlider.Value:F0}";
        }
        
        public void ToggleVisibility()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                UpdateStats();
            }
        }
        
        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
            {
                if (Visible)
                {
                    Hide();
                }
            }
        }
        
        public override void _Notification(int what)
        {
            if (what == NotificationVisibilityChanged)
            {
                _isVisible = Visible;
            }
        }
    }
}
