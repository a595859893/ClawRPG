using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Pets;

namespace ClawRPG.Scripts.Systems.Pets.VFX {
    /// <summary>
    /// 宠物攻击屏幕边缘爪痕特效
    /// 挂在 CanvasLayer 或 Camera 上，接收 PetAttacked 信号并播放爪痕 VFX
    /// </summary>
    public partial class PetAttackVFX : Node {
        public static PetAttackVFX Instance { get; private set; }

        // 爪痕特效节点（4个方向 + 4个角落）
        private Dictionary<ScreenEdge, ClawMark> _clawMarks = new();
        private Dictionary<ScreenEdge, Timer> _fadeTimers = new();

        // 爪痕纹理（程序化生成）
        private const int CLAW_WIDTH = 8;
        private const int CLAW_LENGTH = 120;
        private const int MARK_COUNT = 3;
        private const float FADE_DURATION = 0.35f;

        // 屏幕边缘枚举
        private enum ScreenEdge { Top, Bottom, Left, Right, TopLeft, TopRight, BottomLeft, BottomRight }

        // 单例
        public static PetAttackVFX GetInstance() => Instance;

        public override void _Ready() {
            Instance = this;
            SetupClawMarks();
            ConnectToPetSignal();
        }

        private void SetupClawMarks() {
            // 创建爪痕节点到8个屏幕边缘位置
            // 这些位置将在 Process 中根据视口大小调整
            Array edges = Enum.GetValues(typeof(ScreenEdge));
            foreach (ScreenEdge edge in edges) {
                var mark = new ClawMark { Edge = edge, Visible = false };
                _clawMarks[edge] = mark;

                var timer = new Timer {
                    OneShot = true,
                    WaitTime = FADE_DURATION
                };
                timer.Timeout += () => HideClawMark(edge);
                _fadeTimers[edge] = timer;
                AddChild(timer);
            }
        }

        private void ConnectToPetSignal() {
            // 延迟连接确保 PetCombatAI 已初始化
            CallDeferred(nameof(ConnectSignalDeferred));
        }

        private void ConnectSignalDeferred() {
            var petAI = PetCombatAI.Instance;
            if (petAI != null) {
                petAI.Connect("PetAttacked", new Callable(this, nameof(OnPetAttacked)), (uint)ConnectFlags.Deferred);
                GD.Print("[PetAttackVFX] Connected to PetCombatAI.PetAttacked");
            } else {
                // 重试
                var timer = new Timer { OneShot = true, WaitTime = 1.0f };
                timer.Timeout += () => ConnectSignalDeferred();
                AddChild(timer);
                timer.Start();
            }
        }

        private void OnPetAttacked(Node2D enemy, int damage) {
            if (enemy == null || !IsInstanceValid(enemy))
                return;

            // 计算攻击来源的屏幕边缘方向
            ScreenEdge edge = CalculateScreenEdge(enemy);
            ShowClawMark(edge);
        }

        private ScreenEdge CalculateScreenEdge(Node2D enemy) {
            var viewport = GetViewport();
            if (viewport == null) return ScreenEdge.Right;

            var camera = viewport.GetCamera2d();
            if (camera == null) return ScreenEdge.Right;

            var screenCenter = viewport.GetVisibleRect().Size / 2f;
            var enemyPos = enemy.GlobalPosition;
            var enemyScreenPos = camera.GetGlobalTransform().AffineInverse().Xform(enemyPos);

            float dx = enemyScreenPos.X - screenCenter.X;
            float dy = enemyScreenPos.Y - screenCenter.Y;

            bool fromLeft = dx < 0;
            bool fromTop = dy < 0;

            float ratio = screenCenter.Y / (Mathf.Abs(dx) + 0.001f);
            float yInfluence = Mathf.Abs(dy) / (Mathf.Abs(dx) + 0.001f);

            // 角落判断
            if (yInfluence > ratio * 0.7f) {
                return fromTop ? (fromLeft ? ScreenEdge.TopLeft : ScreenEdge.TopRight)
                               : (fromLeft ? ScreenEdge.BottomLeft : ScreenEdge.BottomRight);
            }

            // 边缘判断
            return fromLeft ? ScreenEdge.Left : ScreenEdge.Right;
        }

        private void ShowClawMark(ScreenEdge edge) {
            var mark = _clawMarks[edge];
            if (mark == null) return;

            // 重置并显示
            mark.Visible = true;
            mark.Modulate = new Color(1, 1, 1, 1);

            // 重置计时器
            var timer = _fadeTimers[edge];
            if (timer != null && timer.IsStopped()) {
                timer.Start(FADE_DURATION);
            }

            // 开始淡出补间
            var tween = CreateTween();
            tween.TweenProperty(mark, "modulate:a", 0f, FADE_DURATION)
                  .SetTrans(Tween.TransitionType.Quad)
                  .SetEase(Tween.EaseType.Out);
        }

        private void HideClawMark(ScreenEdge edge) {
            var mark = _clawMarks[edge];
            if (mark != null) {
                mark.Visible = false;
            }
        }

        public override void _Process(double delta) {
            // 更新爪痕位置到屏幕边缘
            UpdateClawMarkPositions();
        }

        private void UpdateClawMarkPositions() {
            var viewport = GetViewport();
            if (viewport == null) return;

            var rect = viewport.GetVisibleRect();
            float w = rect.Size.X;
            float h = rect.Size.Y;

            // 根据边缘位置调整
            if (_clawMarks.TryGetValue(ScreenEdge.Left, out var left)) {
                left.Position = new Vector2(20, h / 2);
                left.RotationDegrees = -30;
            }
            if (_clawMarks.TryGetValue(ScreenEdge.Right, out var right)) {
                right.Position = new Vector2(w - 20, h / 2);
                right.RotationDegrees = 30;
            }
            if (_clawMarks.TryGetValue(ScreenEdge.Top, out var top)) {
                top.Position = new Vector2(w / 2, 20);
                top.RotationDegrees = 0;
            }
            if (_clawMarks.TryGetValue(ScreenEdge.Bottom, out var bottom)) {
                bottom.Position = new Vector2(w / 2, h - 20);
                bottom.RotationDegrees = 180;
            }
            if (_clawMarks.TryGetValue(ScreenEdge.TopLeft, out var tl)) {
                tl.Position = new Vector2(20, 20);
                tl.RotationDegrees = -45;
            }
            if (_clawMarks.TryGetValue(ScreenEdge.TopRight, out var tr)) {
                tr.Position = new Vector2(w - 20, 20);
                tr.RotationDegrees = 45;
            }
            if (_clawMarks.TryGetValue(ScreenEdge.BottomLeft, out var bl)) {
                bl.Position = new Vector2(20, h - 20);
                bl.RotationDegrees = -135;
            }
            if (_clawMarks.TryGetValue(ScreenEdge.BottomRight, out var br)) {
                br.Position = new Vector2(w - 20, h - 20);
                br.RotationDegrees = 135;
            }
        }

        /// <summary>
        /// 手动触发一次爪痕特效（供外部调用）
        /// </summary>
        public void TriggerClawMark(Vector2 worldPosition) {
            var viewport = GetViewport();
            if (viewport == null) return;

            var camera = viewport.GetCamera2d();
            if (camera == null) return;

            var screenCenter = viewport.GetVisibleRect().Size / 2f;
            var screenPos = camera.GetGlobalTransform().AffineInverse().Xform(worldPosition);

            float dx = screenPos.X - screenCenter.X;
            float dy = screenPos.Y - screenCenter.Y;
            bool fromLeft = dx < 0;
            bool fromTop = dy < 0;

            float ratio = screenCenter.Y / (Mathf.Abs(dx) + 0.001f);
            float yInfluence = Mathf.Abs(dy) / (Mathf.Abs(dx) + 0.001f);

            ScreenEdge edge;
            if (yInfluence > ratio * 0.7f) {
                edge = fromTop ? (fromLeft ? ScreenEdge.TopLeft : ScreenEdge.TopRight)
                               : (fromLeft ? ScreenEdge.BottomLeft : ScreenEdge.BottomRight);
            } else {
                edge = fromLeft ? ScreenEdge.Left : ScreenEdge.Right;
            }

            ShowClawMark(edge);
        }

        private class ClawMark : Control {
            public PetAttackVFX.ScreenEdge Edge { get; set; }

            public ClawMark() {
                var style = new StyleBoxFlat {
                    BgColor = new Color(0.9f, 0.3f, 0.2f, 0.8f)
                };
                AddThemeStyleboxOverride("panel", style);
                CustomMinimumSize = new Vector2(CLAW_LENGTH, CLAW_WIDTH * 3);
                ZIndex = 2000;
            }
        }
    }
}
