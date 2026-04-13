using System;
using Godot;

namespace ClawRPG.Systems.PetLegacy
{
    /// <summary>
    /// 宠物遗产标记视觉效果控制器
    /// 挂在 PetLegacyTombstone/PetLegacySoul/PetLegacyBanner tscn 上
    /// </summary>
    public partial class PetLegacyMarker : Node2D
    {
        [Export]
        private int PetId = 0;

        [Export]
        private string MarkerTypeName = "Tombstone";

        private Label _clickLabel;
        private Sprite2D _sprite;
        private AnimationPlayer _animPlayer;
        private bool _isHovered = false;

        public override void _Ready()
        {
            base._Ready();

            _clickLabel = GetNodeOrNull<Label>("ClickLabel");
            _sprite = GetNodeOrNull<Sprite2D>("Sprite");
            _animPlayer = GetNodeOrNull<AnimationPlayer>("AnimationPlayer");

            // 根据类型设置初始显示
            UpdateVisuals();
        }

        private void UpdateVisuals()
        {
            var type = MarkerTypeName.ToLower() switch
            {
                "soul" => LegacyType.Soul,
                "banner" => LegacyType.Banner,
                _ => LegacyType.Tombstone
            };

            if (_clickLabel != null)
            {
                _clickLabel.Text = type switch
                {
                    LegacyType.Soul => "💜",
                    LegacyType.Banner => "⚔️",
                    _ => "🪦"
                };
            }
        }

        public void Initialize(PetLegacyMarkerData data)
        {
            PetId = data.PetId;
            MarkerTypeName = data.MarkerType.ToString();
            UpdateVisuals();
        }

        private void OnArea2DMouseEntered()
        {
            _isHovered = true;
            if (_clickLabel != null)
            {
                _clickLabel.Modulate = new Color(1f, 1f, 0f);  // 高亮
            }
        }

        private void OnArea2DMouseExited()
        {
            _isHovered = false;
            if (_clickLabel != null)
            {
                _clickLabel.Modulate = Colors.White;
            }
        }

        private void OnArea2DInputEvent(Node viewport, InputEvent @event, int shapeIdx)
        {
            if (@event is InputEventMouseButton mouseBtn && mouseBtn.Pressed && mouseBtn.ButtonIndex == MouseButton.Left)
            {
                // 点击标记显示宠物小传
                PetLegacySystem.Instance?.OnMarkerClicked(PetId);
            }
        }

        /// <summary>
        /// 播放灵魂光球漂浮动画
        /// </summary>
        public void PlaySoulFloatAnimation()
        {
            if (_animPlayer != null && _animPlayer.HasAnimation("float"))
            {
                _animPlayer.Play("float");
            }
        }

        /// <summary>
        /// 播放墓碑出现动画
        /// </summary>
        public void PlaySpawnAnimation()
        {
            if (_animPlayer != null && _animPlayer.HasAnimation("spawn"))
            {
                _animPlayer.Play("spawn");
            }
        }
    }
}
