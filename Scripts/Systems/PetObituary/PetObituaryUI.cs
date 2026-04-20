using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.PetObituary
{
    /// <summary>
    /// 宠物讣告板 UI — 显示在基地墓碑旁的讣告板（REQ-191）
    /// 纯代码创建，无外部场景依赖
    /// </summary>
    public partial class PetObituaryUI : CanvasLayer
    {
        private VBoxContainer _mainContainer;
        private HBoxContainer _headerBar;
        private Label _titleLabel;
        private Button _closeButton;
        private ScrollContainer _scrollContainer;
        private VBoxContainer _obituaryList;
        private Panel _detailPanel;
        private Label _detailText;
        private Button _detailCloseButton;
        private PetObituarySystem _system;

        private ObituaryEntry _selectedEntry;

        public override void _Ready()
        {
            base._Ready();
            _system = PetObituarySystem.Instance;
            if (_system != null)
            {
                _system.OnObituaryAdded += OnObituaryAdded;
            }
            BuildUI();
            RefreshObituaryList();
        }

        public override void _ExitTree()
        {
            if (_system != null)
            {
                _system.OnObituaryAdded -= OnObituaryAdded;
            }
        }

        private void BuildUI()
        {
            // 主面板
            var panel = new Panel
            {
                Name = "ObituaryBoard",
                Size = new Vector2(320, 420)
            };
            panel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            panel.OffsetLeft = -340;
            panel.OffsetTop = 60;
            panel.OffsetRight = -20;
            panel.OffsetBottom = 480;

            // 背景样式
            var style = panel.GetThemeStylebox("panel", "Panel").Duplicate() as StyleBoxFlat;
            style.BgColor = new Color(0.1f, 0.08f, 0.12f, 0.95f);
            style.BorderColor = new Color(0.4f, 0.3f, 0.5f, 0.6f);
            style.BorderWidthRight = 2;
            style.BorderWidthBottom = 2;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomRight = 8;
            style.CornerRadiusBottomLeft = 8;
            panel.AddThemeStyleboxOverride("panel", style);

            AddChild(panel);

            // 标题栏
            _headerBar = new HBoxContainer();
            _headerBar.Name = "HeaderBar";
            _headerBar.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            _headerBar.OffsetLeft = 0;
            _headerBar.OffsetTop = 0;
            _headerBar.OffsetRight = 300;
            _headerBar.OffsetBottom = 32;
            _headerBar.CustomMinimumSize = new Vector2(300, 32);
            _headerBar.Alignment = HBoxContainer.AlignmentMode.Center;
            panel.AddChild(_headerBar);

            var titleIcon = new Label
            {
                Text = "⚰ ",
                HorizontalAlignment = HorizontalAlignment.Right
            };
            titleIcon.AddThemeColorOverride("font_color", new Color(0.8f, 0.6f, 0.6f));
            _headerBar.AddChild(titleIcon);

            _titleLabel = new Label
            {
                Text = "Obituary Board",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.75f, 0.85f));
            _titleLabel.AddThemeFontSizeOverride("font_size", 14);
            _headerBar.AddChild(_titleLabel);

            var spacer = new Control();
            spacer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _headerBar.AddChild(spacer);

            _closeButton = new Button
            {
                Text = "×",
                Flat = true,
                CustomMinimumSize = new Vector2(28, 28)
            };
            _closeButton.AddThemeColorOverride("font_color", new Color(0.6f, 0.5f, 0.6f));
            _closeButton.Pressed += () => HideBoard();
            _headerBar.AddChild(_closeButton);

            // 分隔线
            var sep = new HSeparator();
            sep.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            sep.OffsetLeft = 10;
            sep.OffsetTop = 36;
            sep.OffsetRight = 310;
            sep.OffsetBottom = 38;
            sep.AddThemeColorOverride("separator", new Color(0.4f, 0.3f, 0.5f, 0.4f));
            panel.AddChild(sep);

            // 滚动列表
            _scrollContainer = new ScrollContainer
            {
                Name = "ScrollContainer",
                SetAnchorsPreset = Control.LayoutPreset.TopLeft
            };
            _scrollContainer.OffsetLeft = 0;
            _scrollContainer.OffsetTop = 42;
            _scrollContainer.OffsetRight = 320;
            _scrollContainer.OffsetBottom = 310;
            _scrollContainer.CustomMinimumSize = new Vector2(320, 268);
            _scrollContainer.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
            panel.AddChild(_scrollContainer);

            _obituaryList = new VBoxContainer
            {
                Name = "ObituaryList",
                CustomMinimumSize = new Vector2(300, 0)
            };
            _obituaryList.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _obituaryList.Alignment = VBoxContainer.AlignmentMode.Center;
            _scrollContainer.AddChild(_obituaryList);

            // 详细面板（初始隐藏）
            _detailPanel = new Panel
            {
                Name = "DetailPanel",
                Visible = false,
                ZIndex = 10
            };
            _detailPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _detailPanel.OffsetLeft = -280;
            _detailPanel.OffsetTop = -200;
            _detailPanel.OffsetRight = 280;
            _detailPanel.OffsetBottom = 200;
            _detailPanel.CustomMinimumSize = new Vector2(560, 400);

            var detailStyle = _detailPanel.GetThemeStylebox("panel", "Panel").Duplicate() as StyleBoxFlat;
            detailStyle.BgColor = new Color(0.08f, 0.06f, 0.10f, 0.98f);
            detailStyle.BorderColor = new Color(0.5f, 0.4f, 0.6f, 0.7f);
            detailStyle.BorderWidthRight = 2;
            detailStyle.BorderWidthBottom = 2;
            detailStyle.CornerRadiusTopRight = 10;
            detailStyle.CornerRadiusBottomRight = 10;
            detailStyle.CornerRadiusBottomLeft = 10;
            detailStyle.CornerRadiusTopLeft = 10;
            _detailPanel.AddThemeStyleboxOverride("panel", detailStyle);

            AddChild(_detailPanel);

            var detailVBox = new VBoxContainer
            {
                Name = "DetailVBox",
                Alignment = VBoxContainer.AlignmentMode.Center
            };
            detailVBox.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            detailVBox.OffsetLeft = 10;
            detailVBox.OffsetTop = 10;
            detailVBox.OffsetRight = 550;
            detailVBox.OffsetBottom = 390;
            _detailPanel.AddChild(detailVBox);

            var detailScroll = new ScrollContainer
            {
                Name = "DetailScroll",
                VerticalScrollMode = ScrollContainer.ScrollMode.Auto
            };
            detailScroll.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            detailScroll.OffsetLeft = 0;
            detailScroll.OffsetTop = 0;
            detailScroll.OffsetRight = 530;
            detailScroll.OffsetBottom = 340;
            detailScroll.CustomMinimumSize = new Vector2(530, 340);
            detailScroll.HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled;
            detailVBox.AddChild(detailScroll);

            var detailContent = new VBoxContainer
            {
                Name = "DetailContent"
            };
            detailContent.CustomMinimumSize = new Vector2(510, 0);
            detailContent.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            detailScroll.AddChild(detailContent);

            _detailText = new Label
            {
                Name = "DetailText",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Top,
                AutowrapMode = TextServer.AutowrapMode.Word,
                CustomMinimumSize = new Vector2(510, 0)
            };
            _detailText.AddThemeColorOverride("font_color", new Color(0.85f, 0.75f, 0.85f));
            _detailText.AddThemeFontSizeOverride("font_size", 13);
            detailContent.AddChild(_detailText);

            var detailBottomBar = new HBoxContainer
            {
                Name = "BottomBar"
            };
            detailBottomBar.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            detailBottomBar.OffsetLeft = 0;
            detailBottomBar.OffsetTop = 345;
            detailBottomBar.OffsetRight = 530;
            detailBottomBar.OffsetBottom = 380;
            detailBottomBar.CustomMinimumSize = new Vector2(530, 35);
            detailVBox.AddChild(detailBottomBar);

            var spacer2 = new Control();
            spacer2.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            detailBottomBar.AddChild(spacer2);

            _detailCloseButton = new Button
            {
                Text = "Close",
                CustomMinimumSize = new Vector2(80, 28)
            };
            _detailCloseButton.Pressed += () => HideDetail();
            detailBottomBar.AddChild(_detailCloseButton);
        }

        private void RefreshObituaryList()
        {
            foreach (var child in _obituaryList.GetChildren())
            {
                child.QueueFree();
            }

            if (_system == null)
            {
                var emptyLabel = new Label
                {
                    Text = "No pets have fallen yet.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.45f, 0.55f));
                emptyLabel.AddThemeFontSizeOverride("font_size", 11);
                emptyLabel.CustomMinimumSize = new Vector2(280, 60);
                _obituaryList.AddChild(emptyLabel);
                return;
            }

            var entries = _system.GetAllObituaries();
            if (entries.Count == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "No pets have fallen yet.",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center
                };
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.45f, 0.55f));
                emptyLabel.AddThemeFontSizeOverride("font_size", 11);
                emptyLabel.CustomMinimumSize = new Vector2(280, 60);
                _obituaryList.AddChild(emptyLabel);
                return;
            }

            foreach (var entry in entries)
            {
                var entryBtn = CreateEntryButton(entry);
                _obituaryList.AddChild(entryBtn);
            }
        }

        private Control CreateEntryButton(ObituaryEntry entry)
        {
            var container = new Panel
            {
                CustomMinimumSize = new Vector2(290, 56),
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };

            var containerStyle = container.GetThemeStylebox("panel", "Panel").Duplicate() as StyleBoxFlat;
            containerStyle.BgColor = new Color(0.15f, 0.12f, 0.18f, 0.8f);
            containerStyle.BorderColor = new Color(0.3f, 0.25f, 0.35f, 0.5f);
            containerStyle.BorderWidthRight = 1;
            containerStyle.BorderWidthBottom = 1;
            containerStyle.CornerRadiusBottomRight = 4;
            containerStyle.CornerRadiusBottomLeft = 4;
            container.AddThemeStyleboxOverride("panel", containerStyle);

            var hbox = new HBoxContainer
            {
                Alignment = HBoxContainer.AlignmentMode.Center
            };
            hbox.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
            hbox.OffsetLeft = 8;
            hbox.OffsetTop = 4;
            hbox.OffsetRight = 282;
            hbox.OffsetBottom = 52;
            hbox.CustomMinimumSize = new Vector2(274, 48);
            container.AddChild(hbox);

            // 宠物名字
            var nameLabel = new Label
            {
                Text = entry.PetName,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
                SizeFlagsHorizontal = Control.SizeFlags.Expand
            };
            nameLabel.AddThemeColorOverride("font_color", entry.GetHexColor());
            nameLabel.AddThemeFontSizeOverride("font_size", 12);
            hbox.AddChild(nameLabel);

            // 日期
            var dateLabel = new Label
            {
                Text = entry.GetDeathDateString(),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            dateLabel.AddThemeColorOverride("font_color", new Color(0.45f, 0.4f, 0.5f));
            dateLabel.AddThemeFontSizeOverride("font_size", 9);
            hbox.AddChild(dateLabel);

            // 点击事件
            container.GuiInput += (inputEvent) =>
            {
                if (inputEvent is InputEventMouseButton btn && btn.Pressed && btn.ButtonIndex == MouseButton.Left)
                {
                    ShowDetail(entry);
                }
            };

            // Hover效果
            container.MouseEntered += () =>
            {
                var hoverStyle = container.GetThemeStylebox("panel", "Panel").Duplicate() as StyleBoxFlat;
                hoverStyle.BgColor = new Color(0.2f, 0.17f, 0.25f, 0.9f);
                hoverStyle.BorderColor = new Color(0.45f, 0.38f, 0.55f, 0.7f);
                container.AddThemeStyleboxOverride("panel", hoverStyle);
            };

            container.MouseExited += () =>
            {
                var normalStyle = container.GetThemeStylebox("panel", "Panel").Duplicate() as StyleBoxFlat;
                normalStyle.BgColor = new Color(0.15f, 0.12f, 0.18f, 0.8f);
                normalStyle.BorderColor = new Color(0.3f, 0.25f, 0.35f, 0.5f);
                container.AddThemeStyleboxOverride("panel", normalStyle);
            };

            return container;
        }

        private void ShowDetail(ObituaryEntry entry)
        {
            _selectedEntry = entry;
            _detailText.Text = entry.ObituaryText;
            _detailPanel.Visible = true;

            // 动画
            var tween = CreateTween();
            tween.TweenProperty(_detailPanel, "modulate:a", 1.0f, 0.3f);
        }

        private void HideDetail()
        {
            var tween = CreateTween();
            tween.TweenProperty(_detailPanel, "modulate:a", 0.0f, 0.2f);
            tween.TweenCallback(Callable.From(() => _detailPanel.Visible = false));
        }

        private void OnObituaryAdded(ObituaryEntry entry)
        {
            // 自动高亮最新条目
            RefreshObituaryList();

            // 提示动画（如果板子可见）
            if (_mainContainer != null)
            {
                var newest = _obituaryList.GetChild(0);
                if (newest != null)
                {
                    var tween = CreateTween();
                    tween.TweenProperty(newest, "modulate:a", 1.0f, 0.1f);
                    tween.TweenProperty(newest, "modulate:a", 0.4f, 0.05f);
                    tween.TweenProperty(newest, "modulate:a", 1.0f, 0.05f);
                    tween.TweenProperty(newest, "modulate:a", 0.4f, 0.05f);
                    tween.TweenProperty(newest, "modulate:a", 1.0f, 0.05f);
                }
            }
        }

        public void ShowBoard()
        {
            Visible = true;
            RefreshObituaryList();
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.2f);
        }

        public void HideBoard()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.2f);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }

        public void ToggleBoard()
        {
            if (Visible)
                HideBoard();
            else
                ShowBoard();
        }
    }

    public static class ObituaryEntryExtensions
    {
        public static Color GetHexColor(this ObituaryEntry entry)
        {
            try
            {
                string hex = entry.PetColor.TrimStart('#');
                if (hex.Length == 6)
                {
                    byte r = System.Convert.ToByte(hex.Substring(0, 2), 16);
                    byte g = System.Convert.ToByte(hex.Substring(2, 2), 16);
                    byte b = System.Convert.ToByte(hex.Substring(4, 2), 16);
                    return new Color(r / 255f, g / 255f, b / 255f);
                }
            }
            catch { }
            return new Color(0.8f, 0.7f, 0.85f);
        }
    }
}
