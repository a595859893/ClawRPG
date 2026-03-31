using Godot;
using System;

namespace ClawRPG.Scripts.Systems.Pets.VFX
{
    /// <summary>
    /// 宠物默契通知 UI — 显示配合动画触发提示
    /// REQ-163: 纯视觉系统，不影响数值/伤害计算
    /// </summary>
    public partial class PetSynergyNotificationUI : CanvasLayer
    {
        /// <summary>显示在屏幕中央的配合提示标签</summary>
        private Label _synergyLabel;

        /// <summary>当前配置</summary>
        [Export] private float _displayDuration = 1.5f;
        [Export] private float _fadeOutDuration = 0.4f;
        [Export] private bool _enabled = true;

        private bool _isShowing = false;
        private Timer _connectTimer;

        public override void _Ready()
        {
            SetupUI();
            ConnectToTrigger();
        }

        private void SetupUI()
        {
            _synergyLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Modulate = new Color(1f, 0.9f, 0.6f, 0f)
            };

            var screenSize = GetViewport().GetVisibleRect().Size;
            _synergyLabel.Position = new Vector2(screenSize.X / 2f - 200f, screenSize.Y * 0.35f);
            _synergyLabel.Size = new Vector2(400f, 60f);
            _synergyLabel.AddThemeFontSizeOverride("font_size", 28);

            AddChild(_synergyLabel);
        }

        private void ConnectToTrigger()
        {
            if (PetSynergySkillTrigger.Instance != null)
            {
                PetSynergySkillTrigger.Instance.SynergyAnimTriggered += OnSynergyTriggered;
            }
            else
            {
                var timer = new Timer { OneShot = true, WaitTime = 1.0f };
                timer.Timeout += () => ConnectToTrigger();
                AddChild(timer);
                timer.Start();
                // Stop and free previous retry timer to prevent node leak
                if (_connectTimer != null && _connectTimer.IsValid())
                {
                    _connectTimer.Stop();
                    _connectTimer.QueueFree();
                }
                _connectTimer = timer;
            }
        }

        private void OnSynergyTriggered(int attackerId, int buddyId, int friendshipLevel, string animName)
        {
            if (!_enabled) return;
            ShowNotification(friendshipLevel, animName);
        }

        private void ShowNotification(int friendshipLevel, string animName)
        {
            // 正在显示则跳过（防止动画重叠）
            if (_isShowing) return;
            _isShowing = true;

            string tierText = friendshipLevel >= 16 ? "💫 最高默契！" :
                              friendshipLevel >= 6 ? "✨ 默契配合" :
                              "🐾 友好互动";

            _synergyLabel.Text = $"{tierText}";
            _synergyLabel.Modulate = new Color(1f, 0.9f, 0.6f, 1f);
            _synergyLabel.Scale = new Vector2(0.8f, 0.8f);

            // 缩放进入
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_synergyLabel, "scale", new Vector2(1.15f, 1.15f), 0.12f).From(new Vector2(0.8f, 0.8f));
            tween.TweenProperty(_synergyLabel, "modulate:a", 1f, 0.1f).From(0f);

            // 停留后淡出
            tween.TweenInterval(_displayDuration);
            tween.TweenProperty(_synergyLabel, "modulate:a", 0f, _fadeOutDuration);
            tween.TweenCallback(Callable.From(() => _isShowing = false));
        }

        /// <summary>
        /// 运行时启用/禁用
        /// </summary>
        public void SetEnabled(bool enabled) => _enabled = enabled;
    }
}
