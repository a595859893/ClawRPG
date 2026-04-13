using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

namespace ClawRPG.Scripts.Systems.TitleBiography
{
    /// <summary>
    /// 称号传记荣誉墙 UI
    /// 显示已解锁称号的动态传记条目
    /// </summary>
    public partial class TitleBiographyUI : Control
    {
        private VBoxContainer _contentContainer;
        private Label _titleLabel;
        private Label _countLabel;
        private ScrollContainer _scrollContainer;
        private PanelContainer _emptyState;
        private System.Collections.Generic.Dictionary<string, Control> _entryNodes = new System.Collections.Generic.Dictionary<string, Control>();

        // 稀有度颜色
        private readonly Color CommonColor   = new Color(0.7f, 0.7f, 0.7f);
        private readonly Color UncommonColor  = new Color(0.2f, 0.8f, 0.2f);
        private readonly Color RareColor      = new Color(0.2f, 0.5f, 1.0f);
        private readonly Color EpicColor     = new Color(0.6f, 0.3f, 0.9f);
        private readonly Color LegendaryColor = new Color(1.0f, 0.6f, 0.0f);

        public override void _Ready()
        {
            BuildUI();
            SubscribeToSignals();
            RefreshBiographies();
        }

        private void BuildUI()
        {
            // 主容器
            var mainContainer = new VBoxContainer
            {
                AnchorsPreset = (int)Control.LayoutPreset.FullRect,
                CustomMinimumSize = new Vector2(480, 360)
            };
            AddChild(mainContainer);

            // 标题栏
            var headerContainer = new HBoxContainer();
            mainContainer.AddChild(headerContainer);

            _titleLabel = new Label { Text = "🏆 荣誉墙", HorizontalAlignment = HorizontalAlignment.Left };
            headerContainer.AddChild(_titleLabel);

            headerContainer.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFille });

            _countLabel = new Label { Text = "0 个传记", HorizontalAlignment = HorizontalAlignment.Right };
            _countLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 0.8f));
            headerContainer.AddChild(_countLabel);

            // 分隔线
            var separator = new HSeparator();
            mainContainer.AddChild(separator);

            // 空状态提示
            _emptyState = new PanelContainer
            {
                Modulate = new Color(1f, 1f, 1f, 0.6f),
                CustomMinimumSize = new Vector2(0, 120)
            };
            var emptyLabel = new Label
            {
                Text = "还没有传记\n解锁一个称号，开始书写你的传说",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            _emptyState.AddChild(emptyLabel);
            mainContainer.AddChild(_emptyState);

            // 滚动区域
            _scrollContainer = new ScrollContainer
            {
                VerticalScrollMode = ScrollContainer.ScrollMode.Enabled,
                HorizontalScrollMode = ScrollContainer.ScrollMode.Disabled,
                SizeFlagsVertical = SizeFlags.ExpandFille
            };
            mainContainer.AddChild(_scrollContainer);

            _contentContainer = new VBoxContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFille,
                CustomMinimumSize = new Vector2(440, 0)
            };
            _scrollContainer.AddChild(_contentContainer);

            // 关闭按钮
            var closeBtn = new Button { Text = "关闭" };
            closeBtn.Pressed += () => HidePanel();
            mainContainer.AddChild(closeBtn);
        }

        private void SubscribeToSignals()
        {
            var sys = TitleBiographySystem.Instance;
            if (sys != null)
            {
                sys.Connect("BiographyUnlocked", new Callable(this, nameof(OnBiographyUnlocked)), (uint)ConnectFlags.Deferred);
                sys.Connect("BiographyPanelRequested", new Callable(this, nameof(OnPanelRequested)), (uint)ConnectFlags.Deferred);
            }
        }

        private void OnBiographyUnlocked(string titleId, TitleBiographyData biography)
        {
            AddBiographyEntry(biography);
            UpdateCount();
        }

        private void OnPanelRequested()
        {
            ShowPanel();
        }

        /// <summary>
        /// 刷新所有传记（初始化时调用）
        /// </summary>
        public void RefreshBiographies()
        {
            // 清空现有条目
            foreach (var node in _entryNodes.Values)
            {
                node.QueueFree();
            }
            _entryNodes.Clear();

            var sys = TitleBiographySystem.Instance;
            if (sys == null) return;

            var bios = sys.GetUnlockedBiographies();
            foreach (var bio in bios)
            {
                AddBiographyEntry(bio);
            }

            UpdateCount();
        }

        private void AddBiographyEntry(TitleBiographyData bio)
        {
            if (_entryNodes.ContainsKey(bio.TitleId)) return;
            if (_contentContainer == null) return;

            var entry = CreateEntryNode(bio);
            _contentContainer.AddChild(entry);
            _entryNodes[bio.TitleId] = entry;

            // 入场动画
            entry.Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween().SetParallel(true);
            tween.TweenProperty(entry, "modulate:a", 1.0f, 0.4f).SetTrans(Tween.TransitionType.Back).SetEase(Tween.EaseType.Out);

            _emptyState.Visible = false;
        }

        private Control CreateEntryNode(TitleBiographyData bio)
        {
            var rarityColor = GetRarityColor(bio.Rarity);
            var panel = new PanelContainer
            {
                CustomMinimumSize = new Vector2(0, 80),
                Modulate = new Color(1, 1, 1, 0.9f)
            };

            var styleBox = new StyleBoxFlat
            {
                BgColor = new Color(0.1f, 0.1f, 0.15f, 0.9f),
                BorderColor = rarityColor,
                BorderWidthLeft = 3,
                CornerRadiusTopLeft = 4,
                CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4,
                CornerRadiusBottomRight = 4,
                ContentMarginLeft = 12,
                ContentMarginTop = 8,
                ContentMarginRight = 12,
                ContentMarginBottom = 8
            };
            panel.AddThemeStyleboxOverride("panel", styleBox);

            var vbox = new VBoxContainer();
            panel.AddChild(vbox);

            // 标题行
            var titleRow = new HBoxContainer();
            vbox.AddChild(titleRow);

            // 稀有度图标
            var rarityIcon = new Label
            {
                Text = GetRarityEmoji(bio.Rarity),
                VerticalAlignment = VerticalAlignment.Center
            };
            titleRow.AddChild(rarityIcon);

            var titleNameLabel = new Label
            {
                Text = bio.TitleName,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            titleNameLabel.AddThemeColorOverride("font_color", rarityColor);
            titleNameLabel.AddThemeFontSizeOverride("font_size", 15);
            titleRow.AddChild(titleNameLabel);

            titleRow.AddChild(new Control { SizeFlagsHorizontal = SizeFlags.ExpandFille });

            // 日期标签
            var dateLabel = new Label
            {
                Text = bio.UnlockTime != DateTime.MinValue
                    ? bio.UnlockTime.ToString("yyyy-MM-dd")
                    : "",
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = VerticalAlignment.Center
            };
            dateLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            dateLabel.AddThemeFontSizeOverride("font_size", 11);
            titleRow.AddChild(dateLabel);

            // 传记正文
            var bioLabel = new Label
            {
                Text = bio.BiographyText,
                AutowrapMode = TextServer.AutowrapMode.Word,
                HorizontalAlignment = HorizontalAlignment.Left
            };
            bioLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.85f, 0.9f));
            bioLabel.AddThemeFontSizeOverride("font_size", 12);
            vbox.AddChild(bioLabel);

            return panel;
        }

        private Color GetRarityColor(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common"   => CommonColor,
                "uncommon" => UncommonColor,
                "rare"     => RareColor,
                "epic"     => EpicColor,
                "legendary" => LegendaryColor,
                _          => CommonColor
            };
        }

        private string GetRarityEmoji(string rarity)
        {
            return rarity?.ToLower() switch
            {
                "common"   => "⚪",
                "uncommon" => "🟢",
                "rare"     => "🔵",
                "epic"     => "🟣",
                "legendary" => "🟡",
                _          => "⚪"
            };
        }

        private void UpdateCount()
        {
            var count = TitleBiographySystem.Instance?.GetUnlockedCount() ?? 0;
            _countLabel.Text = $"{count} 个传记";
            _emptyState.Visible = count == 0;
        }

        /// <summary>
        /// 显示荣誉墙面板
        /// </summary>
        public void ShowPanel()
        {
            RefreshBiographies();
            Visible = true;

            // 淡入动画
            Modulate = new Color(1, 1, 1, 0);
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 1.0f, 0.3f);
        }

        /// <summary>
        /// 隐藏荣誉墙面板
        /// </summary>
        public void HidePanel()
        {
            var tween = CreateTween();
            tween.TweenProperty(this, "modulate:a", 0.0f, 0.2f).OnCompleted(() => Visible = false);
        }
    }
}
