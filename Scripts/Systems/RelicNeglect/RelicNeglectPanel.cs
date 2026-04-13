using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.RelicNeglect
{
    /// <summary>
    /// 遗物被遗弃感面板 — 显示所有遗物的亲密度状态
    /// 集成到基地 SafeHouseUI 或遗物仓库界面
    /// </summary>
    public partial class RelicNeglectPanel : Control
    {
        private VBoxContainer _mainContainer;
        private ScrollContainer _scrollContainer;
        private Label _titleLabel;
        private Button _closeButton;
        private HBoxContainer _filterBar;
        private bool _isVisible = false;

        // 当前过滤状态 (-1 = 全部, 0-4 = 对应等级)
        private int _filterLevel = -1;

        public override void _Ready()
        {
            SetupPanel();
            Hide();
        }

        private void SetupPanel()
        {
            // 主面板
            var panel = new PanelContainer
            {
                Name = "NeglectPanel",
                CustomMinimumSize = new Vector2(400, 300)
            };

            var style = new StyleBoxFlat
            {
                BgColor = new Color(0.1f, 0.08f, 0.12f, 0.95f),
                BorderColor = new Color(0.3f, 0.2f, 0.4f, 1.0f),
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                BorderWidthBottom = 2,
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8,
                ContentMarginLeft = 12,
                ContentMarginTop = 12,
                ContentMarginRight = 12,
                ContentMarginBottom = 12
            };
            panel.AddThemeStyleboxOverride("panel", style);
            AddChild(panel);

            var vbox = new VBoxContainer { Name = "VBox" };
            panel.AddChild(vbox);

            // 标题栏
            var titleBar = new HBoxContainer();
            vbox.AddChild(titleBar);

            _titleLabel = new Label
            {
                Text = "遗物亲密度",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 16);
            _titleLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 1.0f));
            titleBar.AddChild(_titleLabel);

            var spacer = new Control { SizeFlagsHorizontal = Control.SizeFlagsExpand };
            titleBar.AddChild(spacer);

            _closeButton = new Button { Text = "✕" };
            _closeButton.Pressed += () => HidePanel();
            titleBar.AddChild(_closeButton);

            // 过滤栏
            _filterBar = new HBoxContainer();
            vbox.AddChild(_filterBar);

            var filterLabel = new Label { Text = "筛选:", VerticalAlignment = VerticalAlignment.Center };
            filterLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            _filterBar.AddChild(filterLabel);

            var allBtn = CreateFilterButton("全部", -1);
            _filterBar.AddChild(allBtn);

            var activeBtn = CreateFilterButton("活跃", 0);
            _filterBar.AddChild(activeBtn);

            var neglectedBtn = CreateFilterButton("冷落", 2);
            _filterBar.AddChild(neglectedBtn);

            var sorrowfulBtn = CreateFilterButton("哀伤", 3);
            _filterBar.AddChild(sorrowfulBtn);

            // 滚动区域
            _scrollContainer = new ScrollContainer
            {
                VerticalScrollBarLocked = false,
                HorizontalScrollBarLocked = true
            };
            vbox.AddChild(_scrollContainer);

            _mainContainer = new VBoxContainer { Name = "ItemList" };
            _scrollContainer.AddChild(_mainContainer);
        }

        private Button CreateFilterButton(string text, int level)
        {
            var btn = new Button { Text = text };
            btn.AddThemeFontSizeOverride("font_size", 12);
            btn.CustomMinimumSize = new Vector2(50, 24);
            int capturedLevel = level;
            btn.Pressed += () => SetFilter(capturedLevel);
            return btn;
        }

        private void SetFilter(int level)
        {
            _filterLevel = level;
            Refresh();
        }

        /// <summary>
        /// 显示面板并刷新数据
        /// </summary>
        public void ShowPanel()
        {
            _isVisible = true;
            Visible = true;
            Refresh();

            // 淡入动画
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1f, 0.3f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);

            // 面板居中
            var viewportSize = GetViewportRect().Size;
            GlobalPosition = (viewportSize - CustomMinimumSize) / 2;
        }

        /// <summary>
        /// 隐藏面板
        /// </summary>
        public void HidePanel()
        {
            _isVisible = false;
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0f, 0.2f)
                .SetTrans(Tween.TransitionType.Quad)
                .SetEase(Tween.EaseType.Out);
            tween.TweenCallback(Callable.From(() => Visible = false));
        }

        /// <summary>
        /// 刷新列表
        /// </summary>
        public void Refresh()
        {
            // 清除旧条目
            foreach (var child in _mainContainer.GetChildren())
            {
                child.QueueFree();
            }

            if (RelicNeglectSystem.Instance == null) return;

            var allEntries = RelicNeglectDatabase.GetAllEntries();
            var sortedEntries = new List<RelicNeglectEntry>(allEntries.Values);
            sortedEntries.Sort((a, b) => b.ConsecutiveBattlesUnused.CompareTo(a.ConsecutiveBattlesUnused));

            foreach (var entry in sortedEntries)
            {
                RelicNeglectLevel level = entry.GetVisualLevel();

                // 过滤
                if (_filterLevel >= 0 && (int)level != _filterLevel)
                    continue;

                var row = CreateRelicRow(entry, level);
                _mainContainer.AddChild(row);
            }

            if (GetChildCount() == 0)
            {
                var emptyLabel = new Label
                {
                    Text = "还没有遗物记录",
                    HorizontalAlignment = HorizontalAlignment.Center,
                    VerticalAlignment = VerticalAlignment.Center,
                    CustomMinimumSize = new Vector2(200, 50)
                };
                emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                _mainContainer.AddChild(emptyLabel);
            }
        }

        private Control CreateRelicRow(RelicNeglectEntry entry, RelicNeglectLevel level)
        {
            var hbox = new HBoxContainer
            {
                CustomMinimumSize = new Vector2(0, 32)
            };

            // 状态图标
            var iconLabel = new Label
            {
                Text = GetLevelIcon(level),
                CustomMinimumSize = new Vector2(30, 0)
            };
            iconLabel.AddThemeColorOverride("font_color", GetLevelColor(level));
            hbox.AddChild(iconLabel);

            // 遗物名称
            string relicName = entry.RelicId;
            // 尝试从 RelicSystem 获取真实名称
            try
            {
                if (RelicSystem.Instance != null)
                {
                    var allRelics = RelicSystem.Instance.GetAllRelics();
                    if (allRelics != null)
                    {
                        foreach (var r in allRelics)
                        {
                            if (r.Id == entry.RelicId && !string.IsNullOrEmpty(r.Name))
                            {
                                relicName = r.Name;
                                break;
                            }
                        }
                    }
                }
            }
            catch { /* ignore */ }

            var nameLabel = new Label
            {
                Text = relicName,
                SizeFlagsHorizontal = Control.SizeFlagsExpand,
                VerticalAlignment = VerticalAlignment.Center
            };
            nameLabel.AddThemeColorOverride("font_color", GetLevelColor(level));
            hbox.AddChild(nameLabel);

            // 未使用场次
            var battleLabel = new Label
            {
                Text = $"{entry.ConsecutiveBattlesUnused}场未用",
                CustomMinimumSize = new Vector2(80, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            battleLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            hbox.AddChild(battleLabel);

            // 状态名称
            var levelLabel = new Label
            {
                Text = GetLevelName(level),
                CustomMinimumSize = new Vector2(60, 0),
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            levelLabel.AddThemeColorOverride("font_color", GetLevelColor(level));
            hbox.AddChild(levelLabel);

            return hbox;
        }

        private string GetLevelIcon(RelicNeglectLevel level)
        {
            return level switch
            {
                RelicNeglectLevel.Active => "★",
                RelicNeglectLevel.Wary => "☆",
                RelicNeglectLevel.Neglected => "⚠",
                RelicNeglectLevel.Sorrowful => "💧",
                RelicNeglectLevel.Despairing => "💔",
                _ => "?"
            };
        }

        private string GetLevelName(RelicNeglectLevel level)
        {
            return level switch
            {
                RelicNeglectLevel.Active => "活跃",
                RelicNeglectLevel.Wary => "警觉",
                RelicNeglectLevel.Neglected => "冷落",
                RelicNeglectLevel.Sorrowful => "哀伤",
                RelicNeglectLevel.Despairing => "绝望",
                _ => "未知"
            };
        }

        private Color GetLevelColor(RelicNeglectLevel level)
        {
            return level switch
            {
                RelicNeglectLevel.Active => new Color(0.4f, 1.0f, 0.4f),
                RelicNeglectLevel.Wary => new Color(1.0f, 0.9f, 0.4f),
                RelicNeglectLevel.Neglected => new Color(1.0f, 0.6f, 0.2f),
                RelicNeglectLevel.Sorrowful => new Color(1.0f, 0.3f, 0.3f),
                RelicNeglectLevel.Despairing => new Color(0.7f, 0.3f, 0.8f),
                _ => Colors.White
            };
        }
    }
}
