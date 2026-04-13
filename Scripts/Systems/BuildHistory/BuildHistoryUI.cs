using System.Collections.Generic;
using Godot;

namespace ClawRPG.Systems.BuildHistory
{
    /// <summary>
    /// Build 历史 UI — 基地/安全屋的「历史回顾」面板
    /// 纯叙事展示，不影响游戏数值
    /// </summary>
    public partial class BuildHistoryUI : Control
    {
        // UI 结构
        [Export]
        private bool _enabled = true;

        [Export]
        private NodePath _historyListPath;
        [Export]
        private NodePath _summaryLabelPath;
        [Export]
        private NodePath _statsLabelPath;
        [Export]
        private NodePath _closeButtonPath;
        [Export]
        private NodePath _filterHighlightPath;
        [Export]
        private NodePath _filterLowlightPath;
        [Export]
        private NodePath _filterAllPath;

        private VBoxContainer _historyList;
        private Label _summaryLabel;
        private Label _statsLabel;
        private Button _closeButton;
        private Button _filterHighlightBtn;
        private Button _filterLowlightBtn;
        private Button _filterAllBtn;

        private enum FilterMode { All, Highlights, Lowlights }
        private FilterMode _currentFilter = FilterMode.All;

        // 当前选中的 entry
        private BuildHistoryEntry _selectedEntry;

        public override void _Ready()
        {
            base._Ready();

            // 获取 UI 节点（如果场景中有预设）
            _historyList = GetNodeOrNull<VBoxContainer>(_historyListPath);
            _summaryLabel = GetNodeOrNull<Label>(_summaryLabelPath);
            _statsLabel = GetNodeOrNull<Label>(_statsLabelPath);
            _closeButton = GetNodeOrNull<Button>(_closeButtonPath);
            _filterHighlightBtn = GetNodeOrNull<Button>(_filterHighlightPath);
            _filterLowlightBtn = GetNodeOrNull<Button>(_filterLowlightPath);
            _filterAllBtn = GetNodeOrNull<Button>(_filterAllPath);

            // 如果场景中没有预设节点，创建程序化 UI
            if (_historyList == null)
                CreateProgrammaticUI();

            // 订阅事件
            if (BuildHistorySystem.Instance != null)
            {
                BuildHistorySystem.Instance.OnHistoryEntryCreated += OnHistoryEntryCreated;
            }

            // 默认隐藏
            Visible = false;
        }

        public override void _ExitTree()
        {
            base._ExitTree();
            if (BuildHistorySystem.Instance != null)
            {
                BuildHistorySystem.Instance.OnHistoryEntryCreated -= OnHistoryEntryCreated;
            }
        }

        #region UI Creation

        private void CreateProgrammaticUI()
        {
            // 主容器
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(mainContainer);

            // 顶部栏：标题 + 关闭按钮
            var headerBar = new HBoxContainer();
            headerBar.Alignment = BoxContainer.AlignmentMode.Center;
            headerBar.CustomMinimumSize = new Vector2(0, 50);
            mainContainer.AddChild(headerBar);

            var titleLabel = new Label();
            titleLabel.Text = "📜 历史回顾";
            titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            titleLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            headerBar.AddChild(titleLabel);

            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.CustomMinimumSize = new Vector2(40, 40);
            _closeButton.Pressed += () => HidePanel();
            headerBar.AddChild(_closeButton);

            // 统计栏
            _statsLabel = new Label();
            _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _statsLabel.AddThemeFontSizeOverride("font_size", 14);
            _statsLabel.Modulate = new Color(0.7f, 0.7f, 0.6f);
            mainContainer.AddChild(_statsLabel);

            // 过滤器栏
            var filterBar = new HBoxContainer();
            filterBar.Alignment = BoxContainer.AlignmentMode.Center;
            filterBar.CustomMinimumSize = new Vector2(0, 40);
            mainContainer.AddChild(filterBar);

            _filterAllBtn = CreateFilterButton("全部", true);
            _filterAllBtn.Pressed += () => SetFilter(FilterMode.All);
            filterBar.AddChild(_filterAllBtn);

            _filterHighlightBtn = CreateFilterButton("高光", false);
            _filterHighlightBtn.Pressed += () => SetFilter(FilterMode.Highlights);
            filterBar.AddChild(_filterHighlightBtn);

            _filterLowlightBtn = CreateFilterButton("低谷", false);
            _filterLowlightBtn.Pressed += () => SetFilter(FilterMode.Lowlights);
            filterBar.AddChild(_filterLowlightBtn);

            // 分隔线
            var sep = new HSeparator();
            mainContainer.AddChild(sep);

            // 叙事总结区
            _summaryLabel = new Label();
            _summaryLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _summaryLabel.CustomMinimumSize = new Vector2(0, 80);
            _summaryLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _summaryLabel.Modulate = new Color(0.9f, 0.85f, 0.7f);
            mainContainer.AddChild(_summaryLabel);

            // 历史列表（可滚动）
            var scroll = new ScrollContainer();
            scroll.VerticalScrollBarEditingEnabled = true;
            scroll.CustomMinimumSize = new Vector2(0, 300);
            mainContainer.AddChild(scroll);

            _historyList = new VBoxContainer();
            _historyList.CustomMinimumSize = new Vector2(0, 280);
            scroll.AddChild(_historyList);

            // 刷新显示
            RefreshStats();
            RefreshHistoryList();
        }

        private Button CreateFilterButton(string text, bool active)
        {
            var btn = new Button();
            btn.Text = text;
            btn.CustomMinimumSize = new Vector2(80, 36);
            UpdateFilterButtonStyle(btn, active);
            return btn;
        }

        private void UpdateFilterButtonStyle(Button btn, bool active)
        {
            if (active)
            {
                btn.AddThemeColorOverride("font_color", new Color(1.0f, 0.9f, 0.6f));
                btn.AddThemeColorOverride("font_hover_color", new Color(1.0f, 0.9f, 0.6f));
            }
            else
            {
                btn.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                btn.AddThemeColorOverride("font_hover_color", new Color(0.7f, 0.7f, 0.7f));
            }
        }

        private void SetFilter(FilterMode mode)
        {
            _currentFilter = mode;
            UpdateFilterButtonStyle(_filterAllBtn, mode == FilterMode.All);
            UpdateFilterButtonStyle(_filterHighlightBtn, mode == FilterMode.Highlights);
            UpdateFilterButtonStyle(_filterLowlightBtn, mode == FilterMode.Lowlights);
            RefreshHistoryList();
        }

        #endregion

        #region Refresh

        private void RefreshStats()
        {
            if (BuildHistorySystem.Instance == null)
                return;

            int total = BuildHistorySystem.Instance.GetAllHistory().Count;
            int maxCombo = BuildHistorySystem.Instance.GetAllTimeMaxCombo();
            int bestStreak = BuildHistorySystem.Instance.GetAllTimeBestWinStreak();

            if (_statsLabel != null)
            {
                _statsLabel.Text = $"总局数: {total}  |  历史最高连击: {maxCombo}  |  最佳连胜: {bestStreak}";
            }
        }

        private void RefreshHistoryList()
        {
            if (_historyList == null || BuildHistorySystem.Instance == null)
                return;

            // 清除现有项
            foreach (var child in _historyList.GetChildren())
            {
                child.QueueFree();
            }

            var entries = BuildHistorySystem.Instance.GetRecentHistory(10);

            // 逆序（最新在前）
            for (int i = entries.Count - 1; i >= 0; i--)
            {
                var entry = entries[i];
                var item = CreateHistoryItem(entry);
                _historyList.AddChild(item);
            }

            if (entries.Count == 0)
            {
                var empty = new Label();
                empty.Text = "还没有历史记录，去经历你的第一次轮回吧。";
                empty.HorizontalAlignment = HorizontalAlignment.Center;
                empty.Modulate = new Color(0.5f, 0.5f, 0.5f);
                _historyList.AddChild(empty);
            }
        }

        private Control CreateHistoryItem(BuildHistoryEntry entry)
        {
            var container = new Panel();
            container.CustomMinimumSize = new Vector2(0, 90);

            var styleBox = new Godot.StyleBoxFlat();
            styleBox.BgColor = new Color(0.12f, 0.12f, 0.18f, 0.9f);
            styleBox.CornerRadiusTopLeft = 8;
            styleBox.CornerRadiusTopRight = 8;
            styleBox.CornerRadiusBottomLeft = 8;
            styleBox.CornerRadiusBottomRight = 8;
            styleBox.BorderWidthLeft = 1;
            styleBox.BorderWidthTop = 1;
            styleBox.BorderWidthRight = 1;
            styleBox.BorderWidthBottom = 1;
            styleBox.BorderColor = entry.Victory
                ? new Color(0.3f, 0.6f, 0.3f, 0.6f)   // 胜：绿色
                : new Color(0.6f, 0.3f, 0.3f, 0.6f); // 败：红色
            container.AddThemeStyleboxOverride("panel", styleBox);

            var innerVBox = new VBoxContainer();
            innerVBox.MarginLeft = 12;
            innerVBox.MarginTop = 8;
            innerVBox.MarginRight = -12;
            innerVBox.MarginBottom = -8;
            container.AddChild(innerVBox);

            // 标题行
            var headerRow = new HBoxContainer();
            innerVBox.AddChild(headerRow);

            var runLabel = new Label();
            runLabel.Text = $"#{entry.RunIndex}  " + (entry.Victory ? "⚔️ 胜利" : "💀 失败");
            runLabel.HorizontalAlignment = HorizontalAlignment.Left;
            runLabel.AddThemeFontSizeOverride("font_size", 16);
            runLabel.Modulate = entry.Victory
                ? new Color(0.6f, 1.0f, 0.6f)
                : new Color(1.0f, 0.6f, 0.6f);
            headerRow.AddChild(runLabel);

            var statsMiniLabel = new Label();
            statsMiniLabel.Text = $"  击杀:{entry.TotalEnemiesDefeated}  连击:{entry.MaxComboAchieved}";
            statsMiniLabel.HorizontalAlignment = HorizontalAlignment.Right;
            statsMiniLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            headerRow.AddChild(statsMiniLabel);

            // 分隔线
            var sep = new HSeparator();
            sep.MarginTop = 4;
            sep.MarginBottom = 4;
            innerVBox.AddChild(sep);

            // 根据过滤器显示内容
            var contentLabel = new Label();
            contentLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            contentLabel.CustomMinimumSize = new Vector2(0, 50);
            contentLabel.Modulate = new Color(0.85f, 0.8f, 0.7f);

            var lines = new List<string>();

            switch (_currentFilter)
            {
                case FilterMode.Highlights:
                    if (entry.HighlightMoments.Count > 0)
                    {
                        foreach (var h in entry.HighlightMoments)
                            lines.Add($"✨ {h.NarrativeText}");
                    }
                    else
                    {
                        lines.Add("(本局无高光时刻)");
                    }
                    break;

                case FilterMode.Lowlights:
                    if (entry.LowlightMoments.Count > 0)
                    {
                        foreach (var l in entry.LowlightMoments)
                            lines.Add($"💔 {l.NarrativeText}");
                    }
                    else
                    {
                        lines.Add("(本局无低谷时刻)");
                    }
                    break;

                default: // All
                    var summary = BuildHistorySystem.Instance != null
                        ? BuildHistorySystem.Instance.GetRunSummaryNarrative(entry)
                        : "";
                    lines.Add(summary);
                    break;
            }

            contentLabel.Text = string.Join("\n", lines);
            contentLabel.HorizontalAlignment = HorizontalAlignment.Center;
            innerVBox.AddChild(contentLabel);

            // 点击展开详情
            container.GuiInput += (inputEvent) =>
            {
                if (inputEvent is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
                {
                    OnEntryClicked(entry);
                }
            };

            return container;
        }

        private void OnEntryClicked(BuildHistoryEntry entry)
        {
            _selectedEntry = entry;

            if (_summaryLabel != null)
            {
                var fullText = BuildHistorySystem.Instance != null
                    ? BuildHistorySystem.Instance.GetRunSummaryNarrative(entry)
                    : "";

                // 添加详细高光/低谷
                if (entry.HighlightMoments.Count > 0)
                {
                    fullText += "\n\n✨ 高光时刻：";
                    foreach (var h in entry.HighlightMoments)
                        fullText += $"\n  • {h.NarrativeText}";
                }
                if (entry.LowlightMoments.Count > 0)
                {
                    fullText += "\n\n💔 低谷时刻：";
                    foreach (var l in entry.LowlightMoments)
                        fullText += $"\n  • {l.NarrativeText}";
                }

                _summaryLabel.Text = fullText;
            }
        }

        private void OnHistoryEntryCreated(BuildHistoryEntry entry)
        {
            RefreshStats();
            RefreshHistoryList();
        }

        #endregion

        #region Show/Hide

        /// <summary>
        /// 显示历史回顾面板
        /// </summary>
        public void ShowPanel()
        {
            Visible = true;
            RefreshStats();
            RefreshHistoryList();

            if (_summaryLabel != null)
                _summaryLabel.Text = "选择一个历史记录查看详情";
        }

        /// <summary>
        /// 隐藏历史回顾面板
        /// </summary>
        public void HidePanel()
        {
            Visible = false;
        }

        /// <summary>
        /// 切换显示状态
        /// </summary>
        public void TogglePanel()
        {
            if (Visible)
                HidePanel();
            else
                ShowPanel();
        }

        #endregion
    }
}
