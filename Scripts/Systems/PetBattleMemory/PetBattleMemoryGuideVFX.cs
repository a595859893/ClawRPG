using Godot;
using System;

namespace ClawRPG.Systems.PetBattleMemory
{
    /// <summary>
    /// 宠物战斗记忆引导视觉特效（REQ-190）
    /// 在宠物头顶显示引导技能的淡金色图标/标签
    /// </summary>
    public partial class PetBattleMemoryGuideVFX : Node2D
    {
        // 组件引用
        private Label _skillLabel;
        private Control _iconContainer;
        private ColorRect _backgroundRect;

        // 状态
        private float _displayDuration = 2f;
        private string _currentSkillId = "";
        private string _currentComboId = "";

        // 动画状态
        private bool _isAnimating = false;

        public override void _Ready()
        {
            SetupUI();
            Visible = false;
        }

        private void SetupUI()
        {
            // 背景容器（圆角矩形，降级为简单矩形）
            _backgroundRect = new ColorRect
            {
                Size = new Vector2(120, 40),
                Color = new Color(0.1f, 0.08f, 0.15f, 0.85f), // 深紫色半透明
                AnchorsPreset = Control.LayoutPreset.Center,
                OffsetLeft = -60,
                OffsetRight = 60,
                OffsetTop = -20,
                OffsetBottom = 20
            };
            AddChild(_backgroundRect);

            // 技能名称标签
            _skillLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                CustomMinimumSize = new Vector2(100, 30),
                Position = new Vector2(-50, -15),
                Size = new Vector2(100, 30),
                Modulate = new Color(1f, 0.85f, 0.4f, 1f) // 淡金色
            };
            AddChild(_skillLabel);

            // 引导图标符号（在标签上方）
            var iconLabel = new Label
            {
                Text = "⇢",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Position = new Vector2(-50, -38),
                Size = new Vector2(100, 20),
                Modulate = new Color(1f, 0.7f, 0.3f, 0.9f) // 琥珀色
            };
            AddChild(iconLabel);
        }

        /// <summary>
        /// 显示引导 VFX
        /// </summary>
        /// <param name="skillId">引导的技能 ID</param>
        /// <param name="comboId">关联的 combo ID</param>
        /// <param name="duration">显示时长（秒）</param>
        public void ShowGuide(string skillId, string comboId, float duration = 2f)
        {
            _currentSkillId = skillId ?? "";
            _currentComboId = comboId ?? "";
            _displayDuration = duration;

            // 设置标签文本
            string displayName = GetSkillDisplayName(_currentSkillId);
            _skillLabel.Text = displayName;

            // 显示节点
            Visible = true;
            Modulate = new Color(1f, 1f, 1f, 1f);

            // 播放进入动画
            PlayEnterAnimation();

            // 设置自动消失
            var timer = GetTree()?.CreateTimer(_displayDuration, false);
            if (timer != null)
            {
                timer.Timeout += OnDisplayTimeout;
            }
        }

        private void PlayEnterAnimation()
        {
            if (_isAnimating) return;
            _isAnimating = true;

            // 缩放弹入动画
            Scale = new Vector2(0.3f, 0.3f);
            var tween = CreateTween();
            tween.SetTrans(Tween.TransitionType.Back);
            tween.TweenProperty(this, "scale", new Vector2(1f, 1f), 0.3f);

            // 淡入
            tween.Parallel().TweenProperty(this, "modulate:a", 1f, 0.2f).From(0f);

            tween.TweenCallback(Callable.From(() => _isAnimating = false));
        }

        private void PlayExitAnimation()
        {
            _isAnimating = true;
            var tween = CreateTween();
            tween.SetTrans(Tween.TransitionType.Quad);

            // 向上飘动 + 淡出
            tween.TweenProperty(this, "position", Position + new Vector2(0, -20), 0.4f);
            tween.Parallel().TweenProperty(this, "modulate:a", 0f, 0.4f);

            tween.TweenCallback(Callable.From(() =>
            {
                _isAnimating = false;
                QueueFree();
            }));
        }

        private void OnDisplayTimeout()
        {
            PlayExitAnimation();
        }

        private string GetSkillDisplayName(string skillId)
        {
            if (string.IsNullOrEmpty(skillId))
                return "???";

            // 尝试从 SkillDatabase 获取显示名
            var skillDb = GetNodeOrNull<Godot.Node>("/root/SkillDatabase");
            if (skillDb != null && skillDb.HasMethod("GetSkillName"))
            {
                var name = skillDb.Call("GetSkillName", skillId);
                if (name != null && !name.AsString().Empty())
                    return name.AsString();
            }

            // 降级：格式化 skillId
            return skillId.Replace("_", " ").Capitalize();
        }

        public override void _Process(double delta)
        {
            // 可选：轻微漂浮动画
            if (Visible && !_isAnimating)
            {
                float bob = Mathf.Sin((float)delta * 3f) * 2f;
                // 仅在没有其他动画时应用
            }
        }
    }
}
