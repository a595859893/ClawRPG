using System;
using Godot;

namespace ClawRPG.Systems.RelicNeglect
{
    /// <summary>
    /// 遗物被遗弃感叙事触发器 — 监听哀伤状态并显示叙事文字
    /// </summary>
    public partial class RelicNeglectNarrative : Control
    {
        private Label _narrativeLabel;
        private Tween _activeTween;
        private const float DISPLAY_DURATION = 3.0f;

        public override void _Ready()
        {
            SetupNarrativeLabel();
            SubscribeToSignals();
        }

        private void SetupNarrativeLabel()
        {
            _narrativeLabel = new Label
            {
                Name = "NarrativeLabel",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CustomMinimumSize = new Vector2(600, 40),
                Text = "",
                Modulate = new Color(1, 1, 1, 0)
            };
            // 居中显示在屏幕下方
            var viewportSize = GetViewportRect().Size;
            _narrativeLabel.GlobalPosition = new Vector2(
                (viewportSize.X - 600) / 2,
                viewportSize.Y - 150);

            // 样式
            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.05f, 0.03f, 0.08f, 0.9f),
                BorderColor = new Color(0.4f, 0.3f, 0.5f, 0.8f),
                BorderWidthLeft = 1,
                BorderWidthRight = 1,
                BorderWidthTop = 1,
                BorderWidthBottom = 1,
                CornerRadiusTopLeft = 6,
                CornerRadiusTopRight = 6,
                CornerRadiusBottomLeft = 6,
                CornerRadiusBottomRight = 6,
                ContentMarginLeft = 20,
                ContentMarginRight = 20,
                ContentMarginTop = 10,
                ContentMarginBottom = 10
            };
            _narrativeLabel.AddThemeStyleboxOverride("normal", style);
            _narrativeLabel.AddThemeFontSizeOverride("font_size", 14);
            _narrativeLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.75f, 1.0f));

            AddChild(_narrativeLabel);
        }

        private void SubscribeToSignals()
        {
            if (RelicNeglectSystem.Instance != null)
            {
                RelicNeglectSystem.Instance.OnSorrowfulNarrativeTriggered += OnSorrowfulNarrative;
            }
            else
            {
                // 延迟订阅
                var timer = new Godot.Timer { OneShot = true, WaitTime = 1.0f };
                AddChild(timer);
                timer.Timeout += () => {
                    timer.QueueFree();
                    if (RelicNeglectSystem.Instance != null)
                        RelicNeglectSystem.Instance.OnSorrowfulNarrativeTriggered += OnSorrowfulNarrative;
                };
                timer.Start();
            }
        }

        private void OnSorrowfulNarrative(string relicId, string narrativeText)
        {
            DisplayNarrative(narrativeText);

            // 同时添加到 NarrativeLogSystem（如果存在）
            try
            {
                var narrativeLog = GetNodeOrNull<Godot.Node>("/root/NarrativeLogSystem");
                if (narrativeLog != null && narrativeLog.HasMethod("AddEntry"))
                {
                    var entry = new Godot.Collections.Dictionary
                    {
                        ["text"] = narrativeText,
                        ["source"] = "relic_neglect",
                        ["relic_id"] = relicId
                    };
                    narrativeLog.Call("AddEntry", entry);
                }
            }
            catch { /* NarrativeLogSystem 可能不存在 */ }
        }

        private void DisplayNarrative(string text)
        {
            // 停止之前的动画
            _activeTween?.Stop();

            _narrativeLabel.Text = text;
            _narrativeLabel.Modulate = new Color(1, 1, 1, 0);

            _activeTween = CreateTween();
            _activeTween.SetParallel(true);

            // 淡入
            _activeTween.TweenProperty(_narrativeLabel, "modulate:a", 1f, 0.5f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);

            // 保持显示
            _activeTween.TweenInterval(DISPLAY_DURATION);

            // 淡出
            _activeTween.TweenProperty(_narrativeLabel, "modulate:a", 0f, 0.5f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.In);
        }

        public override void _ExitTree()
        {
            if (RelicNeglectSystem.Instance != null)
            {
                RelicNeglectSystem.Instance.OnSorrowfulNarrativeTriggered -= OnSorrowfulNarrative;
            }
        }
    }
}
