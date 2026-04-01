using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Systems.Narrative
{
    /// <summary>
    /// 叙事日志 UI - 菜单界面，允许玩家查看已收集/未收集的叙事碎片
    ///
    /// 布局：
    /// - 左侧：房间类型列表（Library / BossRoom / Merchant / ...）
    /// - 右侧：该类型的碎片列表（已收集亮色，未收集暗色+???）
    /// - 底部：总体进度条
    /// - 顶部：主题标签过滤按钮
    /// </summary>
    public partial class NarrativeLogUI : Control
    {
        /// <summary>UI 元素</summary>
        private HBoxContainer _mainContainer;
        private VBoxContainer _roomTypeList;      // 左侧：房间类型
        private VBoxContainer _fragmentList;      // 右侧：碎片列表
        private Label _progressLabel;              // 底部：进度
        private ProgressBar _progressBar;          // 底部：进度条
        private HBoxContainer _themeFilterBar;     // 顶部：主题过滤器
        private Label _roomTypeTitle;              // 当前房间类型标题
        private Button _closeButton;

        /// <summary>当前选中的房间类型</summary>
        private string _selectedRoomType = null;

        /// <summary>当前主题过滤器（null = 全部）</summary>
        private string _activeThemeFilter = null;

        /// <summary>所有碎片数据</summary>
        private List<NarrativeFragment> _allFragments = new List<NarrativeFragment>();

        /// <summary>已收集的碎片ID</summary>
        private HashSet<string> _collected = new HashSet<string>();

        public override void _Ready()
        {
            base._Ready();

            // 全屏暗色背景
            var bg = new ColorRect { Color = new Color(0, 0, 0, 0.85f) };
            bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(bg);

            // 主容器（水平分割：左侧房间列表 + 右侧碎片详情）
            _mainContainer = new HBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainContainer.CustomMinimumSize = new Vector2(1000, 600);
            _mainContainer.Alignment = BoxContainer.AlignmentMode.Center;
            AddChild(_mainContainer);

            // 背景面板
            var bgPanel = new Panel();
            bgPanel.CustomMinimumSize = new Vector2(1000, 600);
            bgPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainContainer.AddChild(bgPanel);

            var innerContainer = new HBoxContainer { CustomMinimumSize = new Vector2(960, 560) };
            innerContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            bgPanel.AddChild(innerContainer);

            // 左侧：房间类型列表（窄）
            var leftPanel = new VBoxContainer { CustomMinimumSize = new Vector2(200, 0) };
            leftPanel.AddThemeConstantOverride("separation", 8);
            innerContainer.AddChild(leftPanel);

            var leftTitle = new Label { Text = "叙事日志", CustomMinimumSize = new Vector2(0, 40) };
            leftTitle.AddThemeFontSizeOverride("font_size", 20);
            leftPanel.AddChild(leftTitle);

            // 主题过滤器（横向滚动）
            _themeFilterBar = new HBoxContainer { CustomMinimumSize = new Vector2(0, 36) };
            _themeFilterBar.AddThemeConstantOverride("separation", 6);
            leftPanel.AddChild(_themeFilterBar);

            var allBtn = MakeThemeButton("全部", null);
            leftPanel.AddChild(allBtn);

            _roomTypeList = new VBoxContainer { VerticalFirstFrame = true };
            _roomTypeList.AddThemeConstantOverride("separation", 4);
            leftPanel.AddChild(new ScrollContainer {
                VerticalScrollBarPolicy = ScrollContainer.ScrollBarPolicy.AsNeeded,
                HorizontalScrollBarPolicy = ScrollContainer.ScrollBarPolicy.Off,
                CustomMinimumSize = new Vector2(0, 400)
            }.Also(c => c.AddChild(_roomTypeList)));

            // 分隔线
            innerContainer.AddChild(new VSeparator());

            // 右侧：碎片列表（宽）
            var rightPanel = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill };
            rightPanel.AddThemeConstantOverride("separation", 8);
            innerContainer.AddChild(rightPanel);

            // 顶部标题栏
            var topBar = new HBoxContainer { CustomMinimumSize = new Vector2(0, 40) };
            _roomTypeTitle = new Label { Text = "选择一个房间类型", HorizontalAlignment = Godot.HorizontalAlignment.Left };
            _roomTypeTitle.AddThemeFontSizeOverride("font_size", 18);
            topBar.AddChild(_roomTypeTitle);
            topBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.ShrinkBegin });
            _closeButton = new Button { Text = "关闭", SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd };
            _closeButton.Pressed += () => HideMenu();
            topBar.AddChild(_closeButton);
            rightPanel.AddChild(topBar);

            // 碎片列表（可滚动）
            var scroll = new ScrollContainer {
                VerticalScrollBarPolicy = ScrollContainer.ScrollBarPolicy.AsNeeded,
                HorizontalScrollBarPolicy = ScrollContainer.ScrollBarPolicy.Off,
                SizeFlagsVertical = Control.SizeFlags.Expand | Control.SizeFlags.Fill
            };
            _fragmentList = new VBoxContainer();
            _fragmentList.AddThemeConstantOverride("separation", 8);
            scroll.AddChild(_fragmentList);
            rightPanel.AddChild(scroll);

            // 底部进度
            var bottomBar = new HBoxContainer { CustomMinimumSize = new Vector2(0, 36) };
            _progressLabel = new Label { Text = "收集进度: 0 / 0" };
            bottomBar.AddChild(_progressLabel);
            bottomBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            _progressBar = new ProgressBar {
                MinValue = 0,
                MaxValue = 100,
                Value = 0,
                CustomMinimumSize = new Vector2(200, 20),
                SizeFlagsHorizontal = Control.SizeFlags.ShrinkEnd
            };
            _progressBar.Step = 1;
            bottomBar.AddChild(_progressBar);
            rightPanel.AddChild(bottomBar);

            // 订阅系统信号
            if (NarrativeLogSystem.Instance != null)
            {
                NarrativeLogSystem.Instance.Connect("FragmentCollected", this, nameof(_OnFragmentCollected));
            }

            // ESC 关闭
            SetProcessInput(true);

            // 加载数据
            RefreshData();
        }

        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey key && key.Pressed && key.Scancode == (uint)KeyList.Escape)
            {
                HideMenu();
            }
        }

        /// <summary>
        /// 打开菜单
        /// </summary>
        public void ShowMenu()
        {
            RefreshData();
            Visible = true;
            GetTree().Paused = true;
        }

        /// <summary>
        /// 隐藏菜单
        /// </summary>
        public void HideMenu()
        {
            Visible = false;
            GetTree().Paused = false;
        }

        /// <summary>
        /// 刷新数据和 UI
        /// </summary>
        private void RefreshData()
        {
            if (NarrativeLogSystem.Instance == null)
                return;

            _allFragments = NarrativeLogSystem.Instance.GetAllFragments();
            _collected = new HashSet<string>(_allFragments
                .Where(f => NarrativeLogSystem.Instance.GetCollectionProgress().collected > 0 ||
                           NarrativeLogSystem.Instance.GetCollectedFragments().Any(cf => cf.FragmentId == f.FragmentId))
                .Select(f => f.FragmentId));

            // 从系统获取真实已收集
            var collectedFrags = NarrativeLogSystem.Instance.GetCollectedFragments();
            _collected = new HashSet<string>(collectedFrags.Select(f => f.FragmentId));

            RefreshThemeFilter();
            RefreshRoomTypeList();
            RefreshFragmentList();
            RefreshProgress();
        }

        /// <summary>
        /// 刷新主题过滤器按钮
        /// </summary>
        private void RefreshThemeFilter()
        {
            foreach (Node child in _themeFilterBar.GetChildren())
                child.QueueFree();

            var allThemes = _allFragments
                .Where(f => !string.IsNullOrEmpty(f.Theme))
                .Select(f => f.Theme)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            MakeThemeButton("全部", null);
            foreach (var theme in allThemes)
                MakeThemeButton(theme, theme);
        }

        private Button MakeThemeButton(string label, string theme)
        {
            var btn = new Button { Text = label, TogglePressed = true, ButtonPressed = (_activeThemeFilter == theme) };
            btn.AddThemeFontSizeOverride("font_size", 12);
            btn.CustomMinimumSize = new Vector2(0, 28);
            btn.Pressed += () => {
                _activeThemeFilter = theme;
                RefreshThemeFilter();
                RefreshFragmentList();
            };
            _themeFilterBar.AddChild(btn);
            return btn;
        }

        /// <summary>
        /// 刷新左侧房间类型列表
        /// </summary>
        private void RefreshRoomTypeList()
        {
            foreach (Node child in _roomTypeList.GetChildren())
                child.QueueFree();

            var roomTypes = _allFragments.Select(f => f.RoomType).Distinct().OrderBy(rt => rt).ToList();

            foreach (var roomType in roomTypes)
            {
                var fragOfType = _allFragments.Where(f => f.RoomType == roomType).ToList();
                int collected = fragOfType.Count(f => _collected.Contains(f.FragmentId));
                int total = fragOfType.Count;

                var btn = new Button
                {
                    Text = $"{roomType}  ({collected}/{total})",
                    Alignment = ButtonAlignment.Left,
                    TogglePressed = (_selectedRoomType == roomType),
                    CustomMinimumSize = new Vector2(180, 36)
                };
                btn.AddThemeFontSizeOverride("font_size", 13);
                var rt = roomType; // capture
                btn.Pressed += () => SelectRoomType(rt);
                _roomTypeList.AddChild(btn);
            }
        }

        /// <summary>
        /// 选择房间类型
        /// </summary>
        private void SelectRoomType(string roomType)
        {
            _selectedRoomType = roomType;
            _roomTypeTitle.Text = roomType;
            RefreshRoomTypeList();
            RefreshFragmentList();
        }

        /// <summary>
        /// 刷新右侧碎片列表
        /// </summary>
        private void RefreshFragmentList()
        {
            foreach (Node child in _fragmentList.GetChildren())
                child.QueueFree();

            IEnumerable<NarrativeFragment> frags;
            if (!string.IsNullOrEmpty(_selectedRoomType))
                frags = _allFragments.Where(f => f.RoomType == _selectedRoomType);
            else
                frags = _allFragments;

            if (!string.IsNullOrEmpty(_activeThemeFilter))
                frags = frags.Where(f => f.Theme == _activeThemeFilter);

            frags = frags.OrderBy(f => f.RoomType).ThenBy(f => f.FloorRange).ThenBy(f => f.FragmentId);

            foreach (var frag in frags)
            {
                bool isCollected = _collected.Contains(frag.FragmentId);
                var item = new PanelContainer { CustomMinimumSize = new Vector2(0, 80) };
                item.AddThemeStyleboxOverride("panel", MakeFragmentPanelStyle(isCollected));

                var hbox = new HBoxContainer { CustomMinimumSize = new Vector2(0, 80) };
                hbox.AddThemeConstantOverride("separation", 12);

                // 左侧：房间+楼层+主题标签
                var leftInfo = new VBoxContainer { CustomMinimumSize = new Vector2(120, 0), Alignment = VBoxContainer.AlignmentMode.Top };
                leftInfo.AddThemeConstantOverride("separation", 4);

                var roomLabel = new Label { Text = frag.RoomType, HorizontalAlignment = Godot.HorizontalAlignment.Left };
                roomLabel.AddThemeFontSizeOverride("font_size", 11);
                leftInfo.AddChild(roomLabel);

                var floorLabel = new Label { Text = "Floor " + frag.FloorRange, HorizontalAlignment = Godot.HorizontalAlignment.Left };
                floorLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
                floorLabel.AddThemeFontSizeOverride("font_size", 10);
                leftInfo.AddChild(floorLabel);

                if (!string.IsNullOrEmpty(frag.Theme))
                {
                    var themeLabel = new Label { Text = "#" + frag.Theme, HorizontalAlignment = Godot.HorizontalAlignment.Left };
                    themeLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.7f, 1.0f));
                    themeLabel.AddThemeFontSizeOverride("font_size", 10);
                    leftInfo.AddChild(themeLabel);
                }

                hbox.AddChild(leftInfo);

                // 分隔线
                hbox.AddChild(new VSeparator());

                // 右侧：叙事文本
                var textVBox = new VBoxContainer { Alignment = VBoxContainer.AlignmentMode.Center, SizeFlagsHorizontal = Control.SizeFlags.Expand };
                textVBox.AddThemeConstantOverride("separation", 4);

                if (isCollected)
                {
                    var titleLbl = new Label { Text = FragTitle(frag.FragmentId), HorizontalAlignment = Godot.HorizontalAlignment.Left };
                    titleLbl.AddThemeFontSizeOverride("font_size", 13);
                    titleLbl.AddThemeColorOverride("font_color", new Color(1.0f, 0.9f, 0.5f));
                    textVBox.AddChild(titleLbl);

                    var narrativeLbl = new Label
                    {
                        Text = frag.NarrativeText,
                        HorizontalAlignment = Godot.HorizontalAlignment.Left,
                        AutowrapMode = TextServer.AutowrapMode.WordSmart
                    };
                    narrativeLbl.AddThemeFontSizeOverride("font_size", 12);
                    textVBox.AddChild(narrativeLbl);
                }
                else
                {
                    var unknownLbl = new Label
                    {
                        Text = "???\n\n尚未发现这个碎片。探索更多的房间也许能找到它……",
                        HorizontalAlignment = Godot.HorizontalAlignment.Left,
                        AutowrapMode = TextServer.AutowrapMode.WordSmart
                    };
                    unknownLbl.AddThemeFontSizeOverride("font_size", 12);
                    unknownLbl.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.4f));
                    textVBox.AddChild(unknownLbl);
                }

                hbox.AddChild(textVBox);

                // 收集状态图标
                var statusLbl = new Label
                {
                    Text = isCollected ? "✓" : "○",
                    CustomMinimumSize = new Vector2(30, 0),
                    HorizontalAlignment = Godot.HorizontalAlignment.Center,
                    VerticalAlignment = Godot.VerticalAlignment.Center
                };
                statusLbl.AddThemeFontSizeOverride("font_size", 18);
                statusLbl.AddThemeColorOverride("font_color", isCollected ? new Color(0.4f, 1.0f, 0.4f) : new Color(0.3f, 0.3f, 0.3f));
                hbox.AddChild(statusLbl);

                item.AddChild(hbox);
                _fragmentList.AddChild(item);
            }

            if (!frags.Any())
            {
                var emptyLbl = new Label
                {
                    Text = "没有找到匹配的碎片",
                    HorizontalAlignment = Godot.HorizontalAlignment.Center,
                    AutowrapMode = TextServer.AutowrapMode.Word
                };
                emptyLbl.AddThemeFontSizeOverride("font_size", 14);
                emptyLbl.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
                _fragmentList.AddChild(emptyLbl);
            }
        }

        private string FragTitle(string fragmentId)
        {
            // 从 FragmentId 提取标题，如 "library_burn_01" → "燃烧的图书馆 #01"
            var parts = fragmentId.Split('_');
            if (parts.Length >= 2)
            {
                string type = parts[0];
                string mood = parts[1];
                string num = parts.Length >= 3 ? " #" + parts[2] : "";
                return char.ToUpper(type[0]) + type.Substring(1) + " / " + char.ToUpper(mood[0]) + mood.Substring(1) + num;
            }
            return fragmentId;
        }

        private StyleBoxFlat MakeFragmentPanelStyle(bool isCollected)
        {
            var style = new StyleBoxFlat
            {
                BgColor = isCollected ? new Color(0.15f, 0.18f, 0.12f) : new Color(0.1f, 0.1f, 0.1f),
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderWidthTop = 2,
                BorderWidthBottom = 2,
                BorderColor = isCollected ? new Color(0.3f, 0.5f, 0.2f) : new Color(0.2f, 0.2f, 0.2f),
                CornerRadiusTopLeft = 4,
                CornerRadiusTopRight = 4,
                CornerRadiusBottomLeft = 4,
                CornerRadiusBottomRight = 4,
                ContentMarginLeft = 12,
                ContentMarginRight = 12,
                ContentMarginTop = 8,
                ContentMarginBottom = 8
            };
            return style;
        }

        /// <summary>
        /// 刷新底部进度条
        /// </summary>
        private void RefreshProgress()
        {
            if (NarrativeLogSystem.Instance == null)
                return;

            var (collected, total) = NarrativeLogSystem.Instance.GetCollectionProgress();
            _progressLabel.Text = $"收集进度: {collected} / {total}";
            if (total > 0)
                _progressBar.MaxValue = total;
            _progressBar.Value = collected;
        }

        private void _OnFragmentCollected(string fragmentId, string roomType)
        {
            RefreshData();
        }
    }

    // C# 扩展方法
    internal static class ControlExtensions
    {
        internal static T Also<T>(this T node, Action<T> action) where T : Node
        {
            action(node);
            return node;
        }
    }
}
