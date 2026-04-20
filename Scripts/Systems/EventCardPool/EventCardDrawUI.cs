using Godot;
using System;

namespace ClawRPG.Scripts.Systems.EventCardPool
{
    /// <summary>
    /// 事件卡抽卡UI — 战斗前展示卡牌，接受或重抽
    /// </summary>
    public partial class EventCardDrawUI : CanvasLayer
    {
        // ========== 控件引用 ==========
        private PanelContainer _cardPanel;
        private Label _titleLabel;
        private Label _categoryLabel;
        private Label _rarityLabel;
        private Label _descriptionLabel;
        private Label _effectsLabel;
        private Label _rerollCostLabel;
        private Button _acceptButton;
        private Button _rerollButton;
        private ColorRect _borderRect;
        private TextureRect _categoryIcon;

        private EventCardPoolSystem _system;
        private bool _isVisible = false;

        public override void _Ready()
        {
            _system = EventCardPoolSystem.Instance;
            if (_system == null)
            {
                GD.PrintErr("[EventCardDrawUI] EventCardPoolSystem not found!");
                return;
            }

            _system.OnCardAccepted += OnCardAccepted;

            BuildUI();
            Visible = false;
        }

        private void BuildUI()
        {
            // 主面板
            _cardPanel = new PanelContainer
            {
                AnchorLeft = 0.5f,
                AnchorRight = 0.5f,
                AnchorTop = 0.5f,
                AnchorBottom = 0.5f,
                OffsetLeft = -200,
                OffsetRight = 200,
                OffsetTop = -250,
                OffsetBottom = 250,
                CustomMinimumSize = new Vector2(400, 500)
            };

            var panelStyle = new StyleBoxFlat
            {
                BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
                BorderWidthLeft = 4,
                BorderWidthRight = 4,
                BorderWidthTop = 4,
                BorderWidthBottom = 4,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8
            };
            _cardPanel.AddThemeStyleboxOverride("panel", panelStyle);

            var vbox = new VBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                CustomMinimumSize = new Vector2(380, 480)
            };
            _cardPanel.AddChild(vbox);

            // 类别标签
            _categoryLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            _categoryLabel.Set("custom_fonts/font", CreateDefaultFont(16));
            vbox.AddChild(_categoryLabel);

            // 标题
            _titleLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.Set("custom_fonts/font", CreateDefaultFont(24));
            vbox.AddChild(_titleLabel);

            // 分隔线
            var separator = new HSeparator { CustomMinimumSize = new Vector2(0, 2) };
            vbox.AddChild(separator);

            // 稀有度
            _rarityLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _rarityLabel.Set("custom_fonts/font", CreateDefaultFont(14));
            vbox.AddChild(_rarityLabel);

            // 描述
            _descriptionLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                AutowrapMode = TextServer.AwrapMode.Word,
                CustomMinimumSize = new Vector2(360, 80)
            };
            _descriptionLabel.Set("custom_fonts/font", CreateDefaultFont(14));
            vbox.AddChild(_descriptionLabel);

            // 效果标签
            var effectsTitle = new Label
            {
                Text = "效果",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            effectsTitle.Set("custom_fonts/font", CreateDefaultFont(16));
            vbox.AddChild(effectsTitle);

            _effectsLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Top,
                AutowrapMode = TextServer.AwrapMode.Word,
                CustomMinimumSize = new Vector2(360, 100)
            };
            _effectsLabel.Set("custom_fonts/font", CreateDefaultFont(13));
            vbox.AddChild(_effectsLabel);

            // 按钮区域
            var buttonBox = new HBoxContainer
            {
                Alignment = BoxContainer.AlignmentMode.Center,
                CustomMinimumSize = new Vector2(360, 50)
            };
            vbox.AddChild(buttonBox);

            _acceptButton = new Button
            {
                Text = "接受",
                CustomMinimumSize = new Vector2(160, 45)
            };
            // REQ-151-03: Godot 3→4 Signal migration
            _acceptButton.Pressed += OnAcceptPressed;
            _acceptButton.Set("custom_fonts/font", CreateDefaultFont(16));
            buttonBox.AddChild(_acceptButton);

            var spacer = new Control { CustomMinimumSize = new Vector2(20, 0) };
            buttonBox.AddChild(spacer);

            _rerollButton = new Button
            {
                Text = "重新抽卡",
                CustomMinimumSize = new Vector2(160, 45)
            };
            // REQ-151-03: Godot 3→4 Signal migration
            _rerollButton.Pressed += OnRerollPressed;
            _rerollButton.Set("custom_fonts/font", CreateDefaultFont(16));
            buttonBox.AddChild(_rerollButton);

            // 重抽费用标签
            _rerollCostLabel = new Label
            {
                Text = "",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _rerollCostLabel.Set("custom_fonts/font", CreateDefaultFont(12));
            vbox.AddChild(_rerollCostLabel);

            AddChild(_cardPanel);

            // 边框ColorRect（稀有度颜色）
            _borderRect = new ColorRect
            {
                Color = Colors.White,
                ZIndex = -1
            };
            _cardPanel.AddChild(_borderRect);
            _borderRect.MoveChild(_borderRect, 0);

            // 背景遮罩
            var bg = new ColorRect
            {
                Color = new Color(0, 0, 0, 0.6f),
                ZIndex = -2
            };
            AddChild(bg);
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        }

        private Godot.Font CreateDefaultFont(int size)
        {
            // 尝试加载自定义字体，失败时使用主题默认字体
            try
            {
                var fontData = ResourceLoader.Load<DynamicFontData>("res://Fonts/NotoSansSC/NotoSansSC-Regular.ttf");
                if (fontData != null)
                {
                    var font = new DynamicFont();
                    font.Size = size;
                    font.FontData = fontData;
                    return font;
                }
            }
            catch (Exception ex)
            {
                GD.Print($"[EventCardDrawUI] 字体加载失败: {ex.Message}");
            }
            return null;
        }

        /// <summary>
        /// 显示抽卡界面
        /// </summary>
        public void ShowDrawUI()
        {
            var card = _system.GetCurrentCard();
            if (card == null) return;

            UpdateCardDisplay(card);

            // 动画入场
            var tween = CreateTween();
            tween.SetParallel(true);

            Modulate = new Color(1, 1, 1, 0);
            _cardPanel.Scale = new Vector2(0.8f, 0.8f);

            tween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
            tween.TweenProperty(_cardPanel, "scale", new Vector2(1, 1), 0.3f)
                .SetTransTween(Tween.TransitionType.Back);

            Visible = true;
            _isVisible = true;

            // 播放抽卡音效（如果存在）
            // AudioServer.PlaySFX("event_card_draw");
        }

        /// <summary>
        /// 更新卡牌显示内容
        /// </summary>
        private void UpdateCardDisplay(EventCardConfig card)
        {
            _titleLabel.Text = card.Title;
            _descriptionLabel.Text = card.Description;
            _effectsLabel.Text = card.GetEffectsText();
            _rarityLabel.Text = card.Rarity.ToString().ToUpper();
            _categoryLabel.Text = card.Category.ToString().ToUpper();

            // 稀有度颜色
            var rarityColor = card.GetRarityColor();
            _rarityLabel.Set("custom_colors/font_color", rarityColor);

            // 更新边框颜色
            var panelStyle = (_cardPanel.StyleBox as StyleBoxFlat);
            if (panelStyle != null)
            {
                panelStyle.BorderColor = rarityColor;
            }

            // 重抽费用
            int cost = _system.GetRerollCost();
            if (card.AcceptOption?.RerollCost > 0 || cost > 0)
            {
                int baseCost = card.AcceptOption?.RerollCost > 0 ? card.AcceptOption.RerollCost : 30;
                _rerollCostLabel.Text = $"重新抽卡消耗 {baseCost} 金币（已重抽 {_system.GetType().GetField("_data")?.GetValue(_system)} 次）";
            }
            else
            {
                _rerollCostLabel.Text = "无法重新抽卡";
                _rerollButton.Disabled = true;
            }

            // 根据类别设置标签颜色
            var categoryColor = card.Category switch
            {
                EventCardCategory.Resource => new Color(0.9f, 0.7f, 0.3f),
                EventCardCategory.Ally => new Color(0.3f, 0.7f, 0.9f),
                EventCardCategory.Terrain => new Color(0.5f, 0.8f, 0.4f),
                EventCardCategory.Curse => new Color(0.9f, 0.3f, 0.3f),
                EventCardCategory.Blessing => new Color(0.9f, 0.9f, 0.3f),
                _ => Colors.White
            };
            _categoryLabel.Set("custom_colors/font_color", categoryColor);

            _rerollButton.Disabled = false;
        }

        private void OnAcceptPressed()
        {
            _system.AcceptCard();
            HideDrawUI();
        }

        private void OnRerollPressed()
        {
            int cost = _system.GetRerollCost();
            string newCardId = _system.ReDrawCard(cost, out bool success);

            if (!success)
            {
                // 金币不足，提示
                GD.Print("[EventCardDrawUI] 金币不足，无法重新抽卡");
                return;
            }

            var card = _system.GetCurrentCard();
            if (card != null)
            {
                UpdateCardDisplay(card);
                // 播放换卡动画
                var tween = CreateTween();
                tween.TweenProperty(_cardPanel, "modulate", new Color(1.2f, 1.2f, 1.2f, 1), 0.1f);
                tween.TweenProperty(_cardPanel, "modulate", new Color(1, 1, 1, 1), 0.2f);
            }
        }

        private void OnCardAccepted(string cardId)
        {
            HideDrawUI();
        }

        private void HideDrawUI()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.2f);
            tween.TweenCallback(new Callable(this, nameof(OnHideComplete)));
            _isVisible = false;
        }

        private void OnHideComplete()
        {
            Visible = false;
        }

        public bool IsShowing => _isVisible;
    }
}
