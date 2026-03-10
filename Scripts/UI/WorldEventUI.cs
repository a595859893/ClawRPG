using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 世界事件UI界面
    /// </summary>
    public class WorldEventUI : Control {
        private Control _container;
        private Label _titleLabel;
        private Label _eventNameLabel;
        private Label _descriptionLabel;
        private Label _timerLabel;
        private Label _multiplierLabel;
        private Label _countdownLabel;
        private ProgressBar _eventProgress;
        private TextureRect _iconDisplay;
        private ColorRect _background;
        private bool _isVisible = false;

        public override void _Ready() {
            SetupUI();
            ConnectSignals();
            
            // 默认隐藏
            Hide();
            
            GD.Print("世界事件UI已加载");
        }

        private void SetupUI() {
            // 背景
            _background = new ColorRect {
                Color = new Color(0, 0, 0, 0.7f),
                Size = new Vector2(400, 200),
                Position = new Vector2(540, 20) // 右上角
            };
            AddChild(_background);

            // 容器
            _container = new Control {
                Size = new Vector2(380, 180),
                Position = new Vector2(10, 10)
            };
            _background.AddChild(_container);

            // 事件图标
            _iconDisplay = new TextureRect {
                Size = new Vector2(40, 40),
                Position = new Vector2(10, 10),
                Text = "🌍"
            };
            _container.AddChild(_iconDisplay);

            // 标题
            _titleLabel = new Label {
                Text = "🌍 世界事件",
                Position = new Vector2(60, 10),
                Size = new Vector2(200, 30)
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 20);
            _container.AddChild(_titleLabel);

            // 事件名称
            _eventNameLabel = new Label {
                Text = "等待事件...",
                Position = new Vector2(10, 50),
                Size = new Vector2(360, 30)
            };
            _eventNameLabel.AddThemeFontSizeOverride("font_size", 18);
            _container.AddChild(_eventNameLabel);

            // 描述
            _descriptionLabel = new Label {
                Text = "",
                Position = new Vector2(10, 80),
                Size = new Vector2(360, 40),
                Autowrap = true
            };
            _container.AddChild(_descriptionLabel);

            // 进度条
            _eventProgress = new ProgressBar {
                Position = new Vector2(10, 120),
                Size = new Vector2(360, 20),
                MaxValue = 100,
                Value = 0
            };
            _eventProgress.AddThemeStyleBoxOverride("fill", CreateProgressStyle());
            _container.AddChild(_eventProgress);

            // 计时器
            _timerLabel = new Label {
                Text = "",
                Position = new Vector2(10, 145),
                Size = new Vector2(180, 25)
            };
            _timerLabel.AddThemeFontSizeOverride("font_size", 16);
            _container.AddChild(_timerLabel);

            // 倍率显示
            _multiplierLabel = new Label {
                Text = "",
                Position = new Vector2(200, 145),
                Size = new Vector2(170, 25),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            _multiplierLabel.AddThemeFontSizeOverride("font_size", 16);
            _container.AddChild(_multiplierLabel);

            // 下次事件倒计时
            _countdownLabel = new Label {
                Text = "",
                Position = new Vector2(10, 170),
                Size = new Vector2(360, 20),
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _countdownLabel.AddThemeFontSizeOverride("font_size", 14);
            _container.AddChild(_countdownLabel);
        }

        private StyleBoxFlat CreateProgressStyle() {
            var style = new StyleBoxFlat {
                BgColor = new Color(0.2f, 0.2f, 0.2f, 1f),
                CornerRadiusTopLeft = 5,
                CornerRadiusTopRight = 5,
                CornerRadiusBottomLeft = 5,
                CornerRadiusBottomRight = 5
            };
            return style;
        }

        private void ConnectSignals() {
            var eventManager = WorldEventManager.Instance;
            if (eventManager != null) {
                eventManager.Connect(nameof(WorldEventManager.EventStarted), this, nameof(OnEventStarted));
                eventManager.Connect(nameof(WorldEventManager.EventEnded), this, nameof(OnEventEnded));
                eventManager.Connect(nameof(WorldEventManager.EventUpdated), this, nameof(OnEventUpdated));
            }
        }

        private void OnEventStarted(WorldEvent evt) {
            Show();
            _isVisible = true;
            UpdateEventDisplay(evt, evt.Duration);
            
            // 设置颜色
            Color eventColor;
            if (Color.TryParse(evt.Color, out eventColor)) {
                _background.Color = new Color(eventColor.R, eventColor.G, eventColor.B, 0.5f);
            }
            
            // 更新倍率显示
            UpdateMultiplierDisplay(evt);
        }

        private void OnEventEnded(WorldEvent evt) {
            _eventNameLabel.Text = "等待下次事件...";
            _descriptionLabel.Text = "";
            _timerLabel.Text = "";
            _multiplierLabel.Text = "";
            _eventProgress.Value = 0;
            _countdownLabel.Text = "下次事件: 准备中...";
            _background.Color = new Color(0, 0, 0, 0.5f);
            
            // 3秒后隐藏
            var timer = GetTree().CreateTimer(3.0f);
            timer.Connect("timeout", this, nameof(HideAfterDelay));
        }

        private void HideAfterDelay() {
            Hide();
            _isVisible = false;
        }

        private void OnEventUpdated(WorldEvent evt, int remainingTime) {
            UpdateEventDisplay(evt, remainingTime);
        }

        private void UpdateEventDisplay(WorldEvent evt, int remainingTime) {
            _eventNameLabel.Text = $"{evt.Icon} {evt.Name} ({evt.GetDifficultyText()})";
            _descriptionLabel.Text = evt.Description;
            
            // 计时器
            int minutes = remainingTime / 60;
            int seconds = remainingTime % 60;
            _timerLabel.Text = $"⏱️ 剩余: {minutes}:{seconds:D2}";
            
            // 进度条
            float progress = (float)remainingTime / evt.Duration * 100;
            _eventProgress.Value = progress;
            
            // 下次事件倒计时（事件进行中时显示）
            if (WorldEventManager.Instance != null) {
                int countdown = WorldEventManager.Instance.NextEventCountdown;
                int cMinutes = countdown / 60;
                int cSeconds = countdown % 60;
                _countdownLabel.Text = $"下次事件: {cMinutes}:{cSeconds:D2}";
            }
        }

        private void UpdateMultiplierDisplay(WorldEvent evt) {
            var multipliers = new List<string>();
            
            if (evt.XPMultiplier > 1.0f) {
                multipliers.Add($"经验 x{evt.XPMultiplier:F1}");
            }
            if (evt.DropMultiplier > 1.0f) {
                multipliers.Add($"掉落 x{evt.DropMultiplier:F1}");
            }
            if (evt.GoldMultiplier > 1.0f) {
                multipliers.Add($"金币 x{evt.GoldMultiplier:F1}");
            }
            
            if (multipliers.Count > 0) {
                _multiplierLabel.Text = string.Join(" | ", multipliers);
            } else {
                _multiplierLabel.Text = "";
            }
        }

        public override void _Process(float delta) {
            if (!_isVisible) return;
            
            // 实时更新倒计时
            var eventManager = WorldEventManager.Instance;
            if (eventManager != null && !eventManager.IsEventActive) {
                int countdown = eventManager.NextEventCountdown;
                int minutes = countdown / 60;
                int seconds = countdown % 60;
                _countdownLabel.Text = $"下次事件: {minutes}:{seconds:D2}";
            }
        }

        public override void _Input(InputEvent evt) {
            // E键切换显示
            if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.E) {
                if (_isVisible) {
                    Hide();
                    _isVisible = false;
                } else {
                    Show();
                    _isVisible = true;
                }
            }
        }
    }
}
