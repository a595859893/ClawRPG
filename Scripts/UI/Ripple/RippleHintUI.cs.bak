using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Ripple;

namespace ClawRPG.Scripts.UI.Ripple {
    /// <summary>
    /// RippleHintUI - HUD 预兆图标组件
    /// 显示在涟漪达到阈值 70% 时的视觉提示
    /// </summary>
    public partial class RippleHintUI : Control {
        // ========== 导出配置 ==========
        [Export] private Vector2 iconSize = new Vector2(32, 32);
        [Export] private int maxVisibleHints = 3;
        [Export] private float fadeInDuration = 0.3f;
        [Export] private float fadeOutDuration = 0.5f;
        [Export] private float pulseInterval = 2.0f;

        // ========== 节点引用 ==========
        private HBoxContainer _hintContainer;
        private Label _debugLabel;

        // ========== 运行时状态 ==========
        private Dictionary<RippleType, TextureRect> _activeHints = new Dictionary<RippleType, TextureRect>();
        private Dictionary<RippleType, AnimationPlayer> _animators = new Dictionary<RippleType, AnimationPlayer>();
        private List<RippleType> _visibleOrder = new List<RippleType>();
        private bool _isSubscribed = false;

        public override void _Ready() {
            SetupUI();
            SubscribeToSignals();
        }

        public override void _ExitTree() {
            UnsubscribeFromSignals();
        }

        private void SetupUI() {
            // 主容器
            _hintContainer = new HBoxContainer();
            _hintContainer.Alignment = BoxContainer.AlignMode.End;
            AddChild(_hintContainer);

            // 调试标签（开发用）
            _debugLabel = new Label();
            _debugLabel.Text = "Ripple: --";
            _debugLabel.HorizontalAlignment = HorizontalAlignment.Right;
            AddChild(_debugLabel);

            // 初始隐藏
            Visible = false;
        }

        private void SubscribeToSignals() {
            if (_isSubscribed) return;

            if (RippleSystem.Instance != null) {
                RippleSystem.Instance.Connect("RippleHintVisible", this, nameof(OnRippleHintVisible));
                RippleSystem.Instance.Connect("RippleHintHidden", this, nameof(OnRippleHintHidden));
                RippleSystem.Instance.Connect("RippleAdded", this, nameof(OnRippleAdded));
                _isSubscribed = true;
            }
        }

        private void UnsubscribeFromSignals() {
            if (!_isSubscribed) return;

            if (RippleSystem.Instance != null) {
                RippleSystem.Instance.Disconnect("RippleHintVisible", this, nameof(OnRippleHintVisible));
                RippleSystem.Instance.Disconnect("RippleHintHidden", this, nameof(OnRippleHintHidden));
                RippleSystem.Instance.Disconnect("RippleAdded", this, nameof(OnRippleAdded));
                _isSubscribed = false;
            }
        }

        private void OnRippleHintVisible(RippleType type) {
            ShowHint(type);
        }

        private void OnRippleHintHidden(RippleType type) {
            HideHint(type);
        }

        private void OnRippleAdded(RippleType type, int amount, int newTotal) {
            UpdateDebugLabel();
        }

        private void ShowHint(RippleType type) {
            if (_activeHints.ContainsKey(type)) return;
            if (_visibleOrder.Count >= maxVisibleHints) {
                // 移除最老的
                var oldest = _visibleOrder[0];
                HideHint(oldest);
            }

            var hintIcon = CreateHintIcon(type);
            _hintContainer.AddChild(hintIcon);
            _activeHints[type] = hintIcon;
            _visibleOrder.Add(type);

            // 淡入动画
            var anim = new AnimationPlayer();
            hintIcon.AddChild(anim);
            _animators[type] = anim;

            var library = new AnimationLibrary();
            var fadeIn = new Animation();
            fadeIn.Length = fadeInDuration;
            fadeIn.ValueTrackSetEnabled(0, hintIcon, "modulate:a");
            fadeIn.MethodTrackSetEnabled(0, hintIcon, false);
            fadeIn.trackInsertKey(0, 0, Colors.Transparent);
            fadeIn.TrackSetPath(0, hintIcon, "modulate:a");
            fadeIn.KeyframeInsert(0, fadeInDuration, Colors.White);
            library.Add("fade_in", fadeIn);
            anim.AddAnimationLibrary("default", library);
            anim.Play("fade_in");

            Visible = _activeHints.Count > 0;
            UpdateDebugLabel();
        }

        private void HideHint(RippleType type) {
            if (!_activeHints.ContainsKey(type)) return;

            var hintIcon = _activeHints[type];

            // 淡出动画
            if (_animators.ContainsKey(type)) {
                var anim = _animators[type];
                var library = new AnimationLibrary();
                var fadeOut = new Animation();
                fadeOut.Length = fadeOutDuration;
                fadeOut.TrackSetPath(0, hintIcon, "modulate:a");
                fadeOut.KeyframeInsert(0, 0, Colors.White);
                fadeOut.KeyframeInsert(0, fadeOutDuration, Colors.Transparent);
                library.Add("fade_out", fadeOut);
                anim.AddAnimationLibrary("default", library);
                anim.Play("fade_out");
                anim.Connect("animationFinished", this, nameof(OnFadeOutFinished), new Godot.Collections.Array { type });
            } else {
                hintIcon.QueueFree();
            }

            _activeHints.Remove(type);
            _visibleOrder.Remove(type);
            _animators.Remove(type);

            Visible = _activeHints.Count > 0;
            UpdateDebugLabel();
        }

        private void OnFadeOutFinished(String animName, RippleType type) {
            if (_activeHints.ContainsKey(type)) {
                _activeHints[type].QueueFree();
                _activeHints.Remove(type);
            }
            if (_animators.ContainsKey(type)) {
                _animators[type].QueueFree();
                _animators.Remove(type);
            }
        }

        private TextureRect CreateHintIcon(RippleType type) {
            var icon = new TextureRect();
            icon.Expand = true;
            icon.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            icon.CustomMinimumSize = iconSize;

            // 图标颜色根据类型
            Color tint;
            string tooltip;
            switch (type) {
                case RippleType.Loss:
                    tint = new Color(1f, 0.3f, 0.3f, 0.8f);    // 红色
                    tooltip = "连击失败在累积...";
                    break;
                case RippleType.Abandon:
                    tint = new Color(1f, 0.7f, 0.2f, 0.8f);   // 橙色
                    tooltip = "有什么被错过了...";
                    break;
                case RippleType.Sacrifice:
                    tint = new Color(0.8f, 0.4f, 1f, 0.8f);  // 紫色
                    tooltip = "牺牲不会被遗忘...";
                    break;
                case RippleType.Desperation:
                    tint = new Color(0.4f, 0.2f, 0.8f, 0.8f); // 深紫
                    tooltip = "绝境在成形...";
                    break;
                case RippleType.Triumph:
                    tint = new Color(1f, 0.9f, 0.2f, 0.8f);  // 金色
                    tooltip = "传说在酝酿...";
                    break;
                case RippleType.Forget:
                    tint = new Color(0.5f, 0.7f, 1f, 0.8f);   // 蓝色
                    tooltip = "遗忘在积累...";
                    break;
                default:
                    tint = new Color(0.7f, 0.7f, 0.7f, 0.8f);
                    tooltip = "有什么在成形...";
                    break;
            }

            // 用 ColorRect + 图标表示（无外部资源依赖）
            var bg = new ColorRect();
            bg.Color = tint;
            bg.CustomMinimumSize = iconSize;
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            icon.AddChild(bg);

            // 添加问号符号表示预兆
            var label = new Label();
            label.Text = "?";
            label.HorizontalAlignment = HorizontalAlignment.Center;
            label.Valign = Label.VAlign.Center;
            label.AddThemeColorOverride("font_color", Colors.White);
            label.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            bg.AddChild(label);

            icon.HintTooltip = tooltip;
            return icon;
        }

        private void UpdateDebugLabel() {
            if (_debugLabel == null || RippleSystem.Instance == null) return;
            var all = RippleSystem.Instance.GetAllRipplePoints();
            string status = "Ripple: ";
            foreach (var kvp in all) {
                if (kvp.Value > 0) {
                    status += $"{kvp.Key}={kvp.Value} ";
                }
            }
            _debugLabel.Text = string.IsNullOrEmpty(status) ? "Ripple: --" : status;
        }

        /// <summary>
        /// 手动刷新（外部调用）
        /// </summary>
        public void Refresh() {
            UpdateDebugLabel();
        }
    }
}
