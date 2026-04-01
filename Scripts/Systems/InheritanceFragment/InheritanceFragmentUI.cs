using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.InheritanceFragment
{
    /// <summary>
    /// 传承碎片 UI - 在新 run 开始时显示已解锁的碎片提示
    /// </summary>
    public partial class InheritanceFragmentUI : Control
    {
        private VBoxContainer _fragmentList;
        private Label _titleLabel;
        private Label _hintLabel;
        private Timer _hideTimer;
        private AnimationPlayer _animPlayer;
        private bool _isVisible;

        // 碎片提示队列
        private Queue<InheritanceFragment> _pendingFragments = new Queue<InheritanceFragment>();
        private float _displayDuration = 4.0f;  // 每个提示显示时间

        public override void _Ready()
        {
            base._Ready();

            // 初始化 UI 结构
            InitializeUI();

            // 默认隐藏
            Visible = false;

            // 订阅碎片解锁事件
            if (InheritanceFragmentSystem.Instance != null)
            {
                InheritanceFragmentSystem.Instance.OnFragmentUnlocked += HandleFragmentUnlocked;
            }
        }

        private void InitializeUI()
        {
            // 主容器
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            mainContainer.Alignment = BoxContainer.AlignmentMode.Center;
            AddChild(mainContainer);

            // 背景面板
            var bgPanel = new Panel();
            bgPanel.CustomMinimumSize = new Vector2(500, 200);
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            styleBox.CornerRadiusTopLeft = 12;
            styleBox.CornerRadiusTopRight = 12;
            styleBox.CornerRadiusBottomLeft = 12;
            styleBox.CornerRadiusBottomRight = 12;
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderColor = new Color(0.6f, 0.4f, 0.8f, 0.8f);  // 紫色边框
            bgPanel.AddThemeStyleboxOverride("panel", styleBox);
            mainContainer.AddChild(bgPanel);

            var contentContainer = new VBoxContainer();
            contentContainer.CustomMinimumSize = new Vector2(480, 180);
            contentContainer.Alignment = BoxContainer.AlignmentMode.Center;
            contentContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            contentContainer.MarginLeft = 10;
            contentContainer.MarginTop = 10;
            contentContainer.MarginRight = -10;
            contentContainer.MarginBottom = -10;
            bgPanel.AddChild(contentContainer);

            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "传承记忆";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 20);
            var titleColor = new Color(0.9f, 0.7f, 1.0f);  // 淡紫色
            _titleLabel.AddThemeColorOverride("font_color", titleColor);
            contentContainer.AddChild(_titleLabel);

            // 分隔线
            var separator = new HSeparator();
            separator.MarginTop = 5;
            separator.MarginBottom = 5;
            contentContainer.AddChild(separator);

            // 碎片列表
            _fragmentList = new VBoxContainer();
            _fragmentList.Alignment = BoxContainer.AlignmentMode.Center;
            contentContainer.AddChild(_fragmentList);

            // 提示标签
            _hintLabel = new Label();
            _hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _hintLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _hintLabel.CustomMinimumSize = new Vector2(460, 60);
            contentContainer.AddChild(_hintLabel);

            // 隐藏计时器
            _hideTimer = new Timer();
            _hideTimer.WaitTime = _displayDuration;
            _hideTimer.OneShot = true;
            _hideTimer.Timeout += OnHideTimerTimeout;
            AddChild(_hideTimer);

            // 动画播放器
            _animPlayer = new AnimationPlayer();
            AddChild(_animPlayer);
        }

        /// <summary>
        /// 显示新 run 开始的碎片提示
        /// </summary>
        public void ShowFragmentsForNewRun()
        {
            var system = InheritanceFragmentSystem.Instance;
            if (system == null || !system.HasUnseenFragments())
                return;

            var fragments = system.GetUnlockedFragments();
            if (fragments.Count == 0)
                return;

            // 清空并填充队列
            _pendingFragments.Clear();
            foreach (var fragment in fragments)
            {
                _pendingFragments.Enqueue(fragment);
            }

            // 显示第一个
            ShowNextFragment();
        }

        /// <summary>
        /// 显示下一个碎片提示
        /// </summary>
        private void ShowNextFragment()
        {
            if (_pendingFragments.Count == 0)
            {
                HideFragments();
                return;
            }

            var fragment = _pendingFragments.Dequeue();

            // 清空列表
            foreach (var child in _fragmentList.GetChildren())
            {
                child.QueueFree();
            }

            // 添加碎片名称
            var fragmentName = new Label();
            fragmentName.Text = fragment.DisplayName;
            fragmentName.HorizontalAlignment = HorizontalAlignment.Center;
            fragmentName.AddThemeFontSizeOverride("font_size", 16);
            var fragmentColor = new Color(1.0f, 0.9f, 0.5f);  // 金色
            fragmentName.AddThemeColorOverride("font_color", fragmentColor);
            _fragmentList.AddChild(fragmentName);

            // 显示模糊提示
            _hintLabel.Text = fragment.VagueHint;
            _hintLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.9f));

            // 显示面板
            Visible = true;
            _isVisible = true;

            // 播放淡入动画
            PlayFadeIn();

            // 开始计时
            _hideTimer.Start(_displayDuration);
        }

        /// <summary>
        /// 处理碎片解锁事件
        /// </summary>
        private void HandleFragmentUnlocked(string fragmentId, InheritanceFragment fragment)
        {
            // 将新解锁的碎片添加到显示队列
            _pendingFragments.Enqueue(fragment);

            // 如果当前没有显示任何内容，立即显示
            if (!_isVisible)
            {
                _hideTimer.Stop();
                ShowNextFragment();
            }
        }

        private void OnHideTimerTimeout()
        {
            // 隐藏当前，显示下一个
            PlayFadeOut();
        }

        private void HideFragments()
        {
            Visible = false;
            _isVisible = false;
            _hideTimer.Stop();
        }

        private void PlayFadeIn()
        {
            // 简单淡入 - 使用 Tween
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
            Modulate = new Color(1, 1, 1, 0);  // 从透明开始
            tween.Play();
        }

        private void PlayFadeOut()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.3f);
            tween.Finished += () =>
            {
                // 显示下一个或隐藏
                if (_pendingFragments.Count > 0)
                {
                    ShowNextFragment();
                }
                else
                {
                    HideFragments();
                }
            };
            tween.Play();
        }

        /// <summary>
        /// 手动关闭碎片显示
        /// </summary>
        public void Dismiss()
        {
            _hideTimer.Stop();
            _pendingFragments.Clear();
            PlayFadeOut();
        }

        public override void _Input(InputEvent evt)
        {
            if (!_isVisible)
                return;

            // 按任意键或点击关闭
            if (evt is InputEventKey keyEvt && keyEvt.Pressed)
            {
                Dismiss();
            }
            else if (evt is InputEventMouseButton mouseEvt && mouseEvt.Pressed)
            {
                Dismiss();
            }
        }
    }
}
