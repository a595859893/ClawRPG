using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;
using TitleRecommendationData = ClawRPG.Scripts.Systems.TitleRecommendation.TitleRecommendationData;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// REQ-200-04/05/06: "下一个目标" 推荐面板
    /// 显示进度 >= 80% 的未解锁称号列表,订阅 TitleUnlocked 信号自动刷新.
    /// 集成到 HUD 右下角.
    /// </summary>
    public partial class TitleRecommendationUI : Control
    {
        private static TitleRecommendationUI _instance;
        public static TitleRecommendationUI Instance => _instance;

        // 稀有度颜色 (与 TitleNotification/TitleBiographyUI 保持一致)
        private readonly Color CommonColor   = new Color(0.7f, 0.7f, 0.7f);
        private readonly Color UncommonColor  = new Color(0.2f, 0.8f, 0.2f);
        private readonly Color RareColor      = new Color(0.2f, 0.5f, 1.0f);
        private readonly Color EpicColor      = new Color(0.6f, 0.3f, 0.9f);
        private readonly Color LegendaryColor = new Color(1.0f, 0.6f, 0.0f);

        private VBoxContainer _listContainer;
        private Label _emptyLabel;
        private Label _headerLabel;
        private PanelContainer _panel;
        private const int MaxRecommendations = 5;
        private const float MinProgress = 0.80f;

        public TitleRecommendationUI()
        {
            _instance = this;
            Name = "TitleRecommendationUI";
        }

        public override void _Ready()
        {
            SetAnchorsAndMarginsPreset(Control.Preset.FullRect);
            BuildPanel();
            SubscribeToSignals();
            RefreshRecommendations();
        }

        public override void _ExitTree()
        {
            UnsubscribeFromSignals();
        }

        private void BuildPanel()
        {
            // 主面板
            _panel = new PanelContainer();
            _panel.SetAnchorsPreset(Control.Preset.BottomRight);
            _panel.Position = new Vector2(-340, -260);
            _panel.CustomMinimumSize = new Vector2(320, 240);
            AddChild(_panel);

            // 背景样式
            var styleBox = new StyleBoxFlat();
            styleBox.BgColor = new Color(0.05f, 0.05f, 0.1f, 0.88f);
            styleBox.BorderWidthLeft = 2;
            styleBox.BorderWidthTop = 2;
            styleBox.BorderWidthRight = 2;
            styleBox.BorderWidthBottom = 2;
            styleBox.BorderColor = new Color(0.3f, 0.3f, 0.5f, 0.6f);
            styleBox.CornerRadiusTopLeft = 8;
            styleBox.CornerRadiusTopRight = 8;
            styleBox.CornerRadiusBottomLeft = 8;
            styleBox.CornerRadiusBottomRight = 8;
            styleBox.ContentMarginLeft = 12;
            styleBox.ContentMarginTop = 10;
            styleBox.ContentMarginRight = 12;
            styleBox.ContentMarginBottom = 10;
            _panel.AddThemeStyleboxOverride("panel", styleBox);

            // 主容器
            VBoxContainer mainVBox = new VBoxContainer();
            mainVBox.AddThemeConstantOverride("separation", 8);
            _panel.AddChild(mainVBox);

            // 标题行
            HBoxContainer headerHBox = new HBoxContainer();
            mainVBox.AddChild(headerHBox);

            _headerLabel = new Label
            {
                Text = "🏅 下一个目标",
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center
            };
            _headerLabel.AddThemeFontSizeOverride("font_size", 15);
            _headerLabel.AddThemeColorOverride("font_color", new Color(0.95f, 0.85f, 0.55f));
            headerHBox.AddChild(_headerLabel);

            // 关闭按钮 (X)
            Button closeBtn = new Button
            {
                Text = "✕",
                Flat = true
            };
            closeBtn.AddThemeFontSizeOverride("font_size", 12);
            closeBtn.Pressed += () => Hide();
            headerHBox.AddChild(closeBtn);
            headerHBox.AddThemeConstantOverride("separation", 4);

            // 分隔线
            HSeparator sep = new HSeparator();
            mainVBox.AddChild(sep);

            // 列表容器
            _listContainer = new VBoxContainer();
            _listContainer.AddThemeConstantOverride("separation", 6);
            mainVBox.AddChild(_listContainer);

            // 空状态标签
            _emptyLabel = new Label
            {
                Text = "最高进度: --%",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Visible = false
            };
            _emptyLabel.AddThemeFontSizeOverride("font_size", 13);
            _emptyLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.65f));
            mainVBox.AddChild(_emptyLabel);
        }

        private Color GetRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 0: return CommonColor;
                case 1: return UncommonColor;
                case 2: return RareColor;
                case 3: return EpicColor;
                case 4: return LegendaryColor;
                default: return CommonColor;
            }
        }

        private string GetRarityName(int rarity)
        {
            switch (rarity)
            {
                case 0: return "普通";
                case 1: return "优秀";
                case 2: return "稀有";
                case 3: return "史诗";
                case 4: return "传说";
                default: return "";
            }
        }

        /// <summary>
        /// REQ-200-04/06: 刷新推荐列表. 被 TitleUnlocked 信号和初始加载调用.
        /// </summary>
        public void RefreshRecommendations()
        {
            if (!IsInstanceValid(_listContainer)) return;

            // 清除旧列表
            foreach (var child in _listContainer.GetChildren())
            {
                child.QueueFree();
            }

            var titleSystem = TitleSystem.Instance;
            if (titleSystem == null)
            {
                _emptyLabel.Text = "称号系统加载中...";
                _emptyLabel.Visible = true;
                return;
            }

            var recommendations = titleSystem.GetRecommendedTitles(MinProgress, MaxRecommendations);

            if (recommendations.Count == 0)
            {
                float highest = titleSystem.GetHighestLockedProgress();
                int pct = (int)Mathf.Round(highest * 100f);
                _emptyLabel.Text = $"最高进度: {pct}%";
                _emptyLabel.Visible = true;
                return;
            }

            _emptyLabel.Visible = false;

            foreach (var rec in recommendations)
            {
                CreateRecommendationRow(rec);
            }
        }

        private void CreateRecommendationRow(TitleRecommendationData rec)
        {
            Color rarityColor = GetRarityColor(rec.Rarity);

            // 行容器
            HBoxContainer row = new HBoxContainer();
            row.AddThemeConstantOverride("separation", 8);
            _listContainer.AddChild(row);

            // 稀有度色块 (左侧标记条)
            Panel colorBar = new Panel
            {
                CustomMinimumSize = new Vector2(4, 32)
            };
            var barStyle = new StyleBoxFlat
            {
                BgColor = rarityColor,
                CornerRadiusTopLeft = 2,
                CornerRadiusTopRight = 2,
                CornerRadiusBottomLeft = 2,
                CornerRadiusBottomRight = 2
            };
            colorBar.AddThemeStyleboxOverride("panel", barStyle);
            row.AddChild(colorBar);

            // 右侧内容
            VBoxContainer content = new VBoxContainer();
            content.AddThemeConstantOverride("separation", 2);
            content.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            row.AddChild(content);

            // 标题名称 + 稀有度
            HBoxContainer titleRow = new HBoxContainer();
            content.AddChild(titleRow);

            Label nameLabel = new Label
            {
                Text = rec.TitleName,
                HorizontalAlignment = HorizontalAlignment.Left,
                SizeFlagsHorizontal = Control.SizeFlags.ExpandFill
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 13);
            nameLabel.AddThemeColorOverride("font_color", rarityColor);
            titleRow.AddChild(nameLabel);

            Label rarityLabel = new Label
            {
                Text = GetRarityName(rec.Rarity),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            rarityLabel.AddThemeFontSizeOverride("font_size", 11);
            rarityLabel.AddThemeColorOverride("font_color", rarityColor);
            rarityLabel.Set("autowrap_mode", TextServer.AutowrapMode.Off);
            titleRow.AddChild(rarityLabel);

            // 进度条
            ProgressBar progressBar = new ProgressBar
            {
                MinValue = 0.0,
                MaxValue = 1.0,
                Value = rec.Progress,
                CustomMinimumSize = new Vector2(0, 10)
            };
            progressBar.SetAnchorsPreset(Control.Preset.FullRect);
            progressBar.PercentVisible = false;
            content.AddChild(progressBar);

            // 进度文字
            Label progressLabel = new Label
            {
                Text = rec.GetProgressLabel(),
                HorizontalAlignment = HorizontalAlignment.Right
            };
            progressLabel.AddThemeFontSizeOverride("font_size", 11);
            progressLabel.AddThemeColorOverride("font_color", new Color(0.65f, 0.65f, 0.7f));
            content.AddChild(progressLabel);
        }

        private void SubscribeToSignals()
        {
            var ts = TitleSystem.Instance;
            if (ts != null)
            {
                // Connect via Godot signal (same pattern as TitleBiographySystem)
                if (ts.HasSignal(SignalName.TitleUnlocked))
                {
                    ts.Connect(SignalName.TitleUnlocked, new Callable(this, MethodName.OnTitleUnlocked), (uint)ConnectFlags.Deferred);
                }
                // Also refresh when title progress is updated
                if (ts.HasSignal(SignalName.TitleProgressUpdated))
                {
                    ts.Connect(SignalName.TitleProgressUpdated, new Callable(this, MethodName.OnTitleProgressUpdated), (uint)ConnectFlags.Deferred);
                }
            }
        }

        private void UnsubscribeFromSignals()
        {
            var ts = TitleSystem.Instance;
            if (ts != null)
            {
                if (ts.HasSignal(SignalName.TitleUnlocked) && ts.IsConnected(SignalName.TitleUnlocked, new Callable(this, MethodName.OnTitleUnlocked)))
                {
                    ts.Disconnect(SignalName.TitleUnlocked, new Callable(this, MethodName.OnTitleUnlocked));
                }
                if (ts.HasSignal(SignalName.TitleProgressUpdated) && ts.IsConnected(SignalName.TitleProgressUpdated, new Callable(this, MethodName.OnTitleProgressUpdated)))
                {
                    ts.Disconnect(SignalName.TitleProgressUpdated, new Callable(this, MethodName.OnTitleProgressUpdated));
                }
            }
        }

        private void OnTitleUnlocked(string playerId, TitleData titleData)
        {
            // Refresh the recommendation list when a title is unlocked
            RefreshRecommendations();
            // TitleNotification fires its own notification via its separate subscription
        }

        private void OnTitleProgressUpdated(string titleId, int current, int required)
        {
            RefreshRecommendations();
        }

        /// <summary>
        /// 供外部调用的显示方法 (REQ-200-05 HUD 集成).
        /// </summary>
        public void ShowPanel()
        {
            Visible = true;
            RefreshRecommendations();
        }

        public void HidePanel()
        {
            Visible = false;
        }
    }
}
