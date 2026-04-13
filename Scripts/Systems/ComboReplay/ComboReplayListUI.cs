using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Systems.ComboReplay
{
    /// <summary>
    /// Combo 回放列表 UI（REQ-114-04）
    /// 显示本地保存的回放列表，点击播放
    /// </summary>
    public partial class ComboReplayListUI : BaseUI
    {
        public static ComboReplayListUI Instance { get; private set; }

        private PanelContainer _mainPanel;
        private VBoxContainer _mainVBox;

        // 标题栏
        private HBoxContainer _titleBar;
        private Label _titleLabel;
        private Button _closeButton;

        // 回放列表
        private ScrollContainer _listScroll;
        private VBoxContainer _listContainer;
        private Label _emptyLabel;

        // 回调：选中回放后打开 ComboReplayUI 播放
        public static Action<ComboReplayData> OnReplaySelected;

        public override void _Ready()
        {
            Instance = this;
            InitializeUI();
            base._Ready();
            Hide();
        }

        protected override void InitializeUI()
        {
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_mainPanel);

            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.08f, 0.08f, 0.12f, 0.98f);
            style.CornerRadiusTopLeft = 8;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomLeft = 8;
            style.CornerRadiusBottomRight = 8;
            style.ContentMarginLeft = 16;
            style.ContentMarginTop = 12;
            style.ContentMarginRight = 16;
            style.ContentMarginBottom = 12;
            style.border_width_left = 1;
            style.border_width_right = 1;
            style.border_width_top = 1;
            style.border_width_bottom = 1;
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f, 1f);
            _mainPanel.AddThemeStyleboxOverride("panel", style);

            _mainVBox = new VBoxContainer();
            _mainPanel.AddChild(_mainVBox);

            // 标题栏
            _titleBar = new HBoxContainer();
            _mainVBox.AddChild(_titleBar);

            _titleLabel = new Label();
            _titleLabel.Text = "📜 战斗回放列表";
            _titleLabel.AddThemeFontSizeOverride("font_size", 18);
            _titleBar.AddChild(_titleLabel);

            _titleBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlagsExpandFill });

            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.Pressed += Hide;
            _titleBar.AddChild(_closeButton);

            // 列表区
            _listScroll = new ScrollContainer();
            _listScroll.CustomMinimumSize = new Vector2(0, 400);
            _listScroll.SizeFlagsVertical = Control.SizeFlagsExpandFill;
            _mainVBox.AddChild(_listScroll);

            _listContainer = new VBoxContainer();
            _listContainer.SizeFlagsHorizontal = Control.SizeFlagsExpandFill;
            _listScroll.AddChild(_listContainer);

            _emptyLabel = new Label();
            _emptyLabel.Text = "暂无保存的回放";
            _emptyLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.5f));
            _emptyLabel.Align = Label.LabelEnum.Center;
            _listContainer.AddChild(_emptyLabel);

            GD.Print("[ComboReplayListUI] Initialized");
        }

        /// <summary>
        /// 刷新并显示列表
        /// </summary>
        public void RefreshList()
        {
            // 清空列表
            foreach (var child in _listContainer.GetChildren())
                child.QueueFree();

            var persistence = ComboReplayPersistence.Instance;
            if (persistence == null)
            {
                _emptyLabel.Text = "持久化系统未初始化";
                _listContainer.AddChild(_emptyLabel);
                return;
            }

            var replays = persistence.GetReplayList();

            if (replays.Count == 0)
            {
                _emptyLabel.Text = "暂无保存的回放";
                _listContainer.AddChild(_emptyLabel);
                Show();
                return;
            }

            // 添加回放条目
            foreach (var info in replays)
            {
                var row = CreateReplayRow(info);
                _listContainer.AddChild(row);
            }

            Show();
        }

        private Control CreateReplayRow(ComboReplayPersistence.ReplayFileInfo info)
        {
            var container = new PanelContainer();
            container.CustomMinimumSize = new Vector2(0, 64);

            var boxStyle = new StyleBoxFlat();
            boxStyle.BgColor = new Color(0.12f, 0.12f, 0.18f, 0.9f);
            boxStyle.CornerRadiusTopLeft = 6;
            boxStyle.CornerRadiusTopRight = 6;
            boxStyle.CornerRadiusBottomLeft = 6;
            boxStyle.CornerRadiusBottomRight = 6;
            boxStyle.ContentMarginLeft = 12;
            boxStyle.ContentMarginRight = 12;
            boxStyle.ContentMarginTop = 8;
            boxStyle.ContentMarginBottom = 8;
            container.AddThemeStyleboxOverride("panel", boxStyle);

            var hbox = new HBoxContainer();
            container.AddChild(hbox);

            // 结果图标
            var resultIcon = new Label();
            resultIcon.Text = info.Result == "victory" ? "✓" : "✗";
            resultIcon.AddThemeColorOverride("font_color",
                info.Result == "victory" ? new Color(0.2f, 0.9f, 0.4f) : new Color(0.9f, 0.3f, 0.2f));
            resultIcon.AddThemeFontSizeOverride("font_size", 20);
            resultIcon.CustomMinimumSize = new Vector2(30, 0);
            hbox.AddChild(resultIcon);

            // 中间信息
            var infoVBox = new VBoxContainer();
            infoVBox.SizeFlagsHorizontal = Control.SizeFlagsExpandFill;
            hbox.AddChild(infoVBox);

            var nameLabel = new Label();
            nameLabel.Text = $"{info.SceneName}  •  {info.CreatedAt:MM-dd HH:mm}";
            nameLabel.AddThemeFontSizeOverride("font_size", 14);
            nameLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 1f));
            infoVBox.AddChild(nameLabel);

            var statsLabel = new Label();
            statsLabel.Text = $"{info.ActionCount} 操作  •  {info.ComboCount} Combos  •  {info.DurationSeconds:F1f}s  •  Seed {info.Seed}";
            statsLabel.AddThemeFontSizeOverride("font_size", 12);
            statsLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
            infoVBox.AddChild(statsLabel);

            // 播放按钮
            var playBtn = new Button();
            playBtn.Text = "▶ 播放";
            playBtn.Pressed += () => OnPlayPressed(info.FileName);
            hbox.AddChild(playBtn);

            // 删除按钮
            var delBtn = new Button();
            delBtn.Text = "🗑";
            delBtn.Pressed += () => OnDeletePressed(info.FileName);
            hbox.AddChild(delBtn);

            return container;
        }

        private void OnPlayPressed(string fileName)
        {
            var persistence = ComboReplayPersistence.Instance;
            if (persistence == null)
                return;

            var replay = persistence.LoadReplay(fileName);
            if (replay != null)
            {
                Hide();
                OnReplaySelected?.Invoke(replay);
            }
        }

        private void OnDeletePressed(string fileName)
        {
            ComboReplayPersistence.Instance?.DeleteReplay(fileName);
            RefreshList(); // 刷新列表
        }

        public override void Show()
        {
            Visible = true;
            IsVisible = true;
        }

        public override void Hide()
        {
            Visible = false;
            IsVisible = false;
        }
    }
}
