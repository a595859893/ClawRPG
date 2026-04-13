using System;
using Godot;
using Godot.Collections;

namespace ClawRPG.Systems.RelicNeglect
{
    /// <summary>
    /// 遗物被遗弃感叠加层 — 挂载在遗物图标节点旁
    /// 显示灰尘/裂纹等视觉状态
    /// </summary>
    public partial class RelicNeglectOverlay : Control
    {
        /// <summary>关联的遗物ID</summary>
        [Export]
        public string RelicId { get; set; } = "";

        private TextureRect _dustOverlay;
        private TextureRect _crackOverlay;
        private Label _levelLabel;
        private RelicNeglectLevel _currentLevel = RelicNeglectLevel.Active;

        // 叠加层资源路径（相对于 res:// 或用户自定义）
        private const string DUST_TEXTURE = "res://assets/relic_neglect/dust.png";
        private const string CRACK_LIGHT_TEXTURE = "res://assets/relic_neglect/crack_light.png";
        private const string CRACK_HEAVY_TEXTURE = "res://assets/relic_neglect/crack_heavy.png";
        private const string GLOOM_TEXTURE = "res://assets/relic_neglect/gloom.png";
        private const string SHATTER_TEXTURE = "res://assets/relic_neglect/shatter.png";

        public override void _Ready()
        {
            SetupOverlay();
            UpdateVisibility();
        }

        private void SetupOverlay()
        {
            // 创建叠加层容器
            var container = new HBoxContainer
            {
                Name = "NeglectOverlay",
                Alignment = BoxContainer.AlignMode.End,
                VerticalCustomMinimumSize = 0
            };
            AddChild(container);

            // 灰尘叠加层
            _dustOverlay = new TextureRect
            {
                Name = "DustOverlay",
                ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
                StretchMode = TextureRect.StretchModeEnum.KeepSize,
                CustomMinimumSize = new Vector2(16, 16),
                Modulate = new Color(1, 1, 1, 0.6f),
                Visible = false
            };
            container.AddChild(_dustOverlay);

            // 裂纹叠加层
            _crackOverlay = new TextureRect
            {
                Name = "CrackOverlay",
                ExpandMode = TextureRect.ExpandModeEnum.FitWidth,
                StretchMode = TextureRect.StretchModeEnum.KeepSize,
                CustomMinimumSize = new Vector2(20, 20),
                Modulate = new Color(1, 1, 1, 0.8f),
                Visible = false
            };
            container.AddChild(_crackOverlay);

            // 等级标签
            _levelLabel = new Label
            {
                Name = "LevelLabel",
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Top,
                Modulate = new Color(0.7f, 0.5f, 0.3f, 0.9f)
            };
            container.AddChild(_levelLabel);
        }

        /// <summary>
        /// 更新叠加层显示状态
        /// </summary>
        public void UpdateOverlay(RelicNeglectLevel level)
        {
            if (level == _currentLevel) return;
            _currentLevel = level;

            // 加载纹理（如果资源存在的话）
            // 注意：这些纹理资源需要美术提供，这里做防御性处理
            try
            {
                _dustOverlay.Texture = ResourceLoader.Exists(DUST_TEXTURE)
                    ? GD.Load<Texture2D>(DUST_TEXTURE) : null;
                _crackOverlay.Texture = ResourceLoader.Exists(CRACK_HEAVY_TEXTURE)
                    ? GD.Load<Texture2D>(CRACK_HEAVY_TEXTURE) : null;
            }
            catch
            {
                // 资源加载失败，使用程序化方式绘制
            }

            // 根据等级显示/隐藏叠加层
            bool showDust = level >= RelicNeglectLevel.Wary;
            bool showCrack = level >= RelicNeglectLevel.Neglected;

            _dustOverlay.Visible = showDust;
            _crackOverlay.Visible = showCrack;

            // 根据等级着色
            switch (level)
            {
                case RelicNeglectLevel.Active:
                    Modulate = Colors.White;
                    _levelLabel.Text = "";
                    break;
                case RelicNeglectLevel.Wary:
                    Modulate = new Color(1.0f, 0.9f, 0.7f, 0.8f);
                    _levelLabel.Text = "☆";
                    _levelLabel.Modulate = new Color(1.0f, 0.8f, 0.2f);
                    break;
                case RelicNeglectLevel.Neglected:
                    Modulate = new Color(1.0f, 0.8f, 0.5f, 0.85f);
                    _levelLabel.Text = "☆☆";
                    _levelLabel.Modulate = new Color(1.0f, 0.6f, 0.1f);
                    break;
                case RelicNeglectLevel.Sorrowful:
                    Modulate = new Color(0.9f, 0.6f, 0.6f, 0.9f);
                    _levelLabel.Text = "☆☆☆";
                    _levelLabel.Modulate = new Color(0.9f, 0.3f, 0.3f);
                    break;
                case RelicNeglectLevel.Despairing:
                    Modulate = new Color(0.7f, 0.4f, 0.7f, 1.0f);
                    _levelLabel.Text = "💔";
                    _levelLabel.Modulate = new Color(0.6f, 0.2f, 0.6f);
                    break;
            }

            // 如果状态好转（被激活），播放消散动画
            if (level == RelicNeglectLevel.Active)
            {
                PlayFadeOutAnimation();
            }
        }

        /// <summary>
        /// 遗物被重新激活时的消散动画（灰尘消失）
        /// </summary>
        private void PlayFadeOutAnimation()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0f, 0.3f)
                .FromCurrent()
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
            tween.TweenCallback(Callable.From(() => {
                UpdateVisibility();
                Modulate = Colors.White;
            }));
        }

        /// <summary>
        /// 状态升级时的裂纹加深动画
        /// </summary>
        private void PlayCrackDeepenAnimation()
        {
            var tween = CreateTween();
            tween.TweenProperty(_crackOverlay, "modulate:a", 1.0f, 0.5f)
                .FromCurrent()
                .SetTrans(Tween.TransitionType.Elastic)
                .SetEase(Tween.EaseType.Out);
        }

        private void UpdateVisibility()
        {
            bool showDust = _currentLevel >= RelicNeglectLevel.Wary;
            bool showCrack = _currentLevel >= RelicNeglectLevel.Neglected;
            _dustOverlay.Visible = showDust;
            _crackOverlay.Visible = showCrack;
            _levelLabel.Visible = _currentLevel > RelicNeglectLevel.Active;
        }
    }
}
