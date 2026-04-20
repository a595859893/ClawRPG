using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Systems.ComboReplay
{
    /// <summary>
    /// Combo 回放 UI（REQ-114-03 + REQ-114-04）
    /// 显示回放时间线、操作记录列表、播放控制
    /// </summary>
    public partial class ComboReplayUI : BaseUI
    {
        public static ComboReplayUI Instance { get; private set; }

        // 主面板
        private PanelContainer _mainPanel;
        private VBoxContainer _mainVBox;

        // 顶部标题栏
        private HBoxContainer _titleBar;
        private Label _titleLabel;
        private Button _closeButton;

        // 回放信息区
        private HBoxContainer _infoBar;
        private Label _sceneLabel;
        private Label _resultLabel;
        private Label _durationLabel;
        private Label _seedLabel;

        // 时间轴
        private HBoxContainer _timelineContainer;
        private Label _currentTimeLabel;
        private HSlider _timelineSlider;
        private Label _totalTimeLabel;

        // 播放控制条
        private HBoxContainer _controlsBar;
        private Button _playPauseButton;
        private Button _rewindButton;
        private Button _fastForwardButton;
        private Label _speedLabel;
        private Button _speedButton;

        // 操作历史列表
        private ScrollContainer _historyScroll;
        private VBoxContainer _historyContainer;
        private Label _noActionsLabel;

        // Combo 记录列表
        private ScrollContainer _comboScroll;
        private VBoxContainer _comboContainer;
        private Label _noCombosLabel;

        // 当前高亮的操作
        private int _highlightedActionIndex = -1;
        private readonly Color _highlightColor = new Color(1f, 0.85f, 0.2f, 0.15f);
        private readonly Color _normalColor = new Color(0, 0, 0, 0);

        // 当前回放
        private ComboReplayData _currentReplay;

        // 每帧刷新（时间轴）
        private int _frameSkip = 0;

        public override void _Ready()
        {
            Instance = this;
            InitializeUI();
            base._Ready();

            // 订阅播放器信号
            ComboReplayPlayer.OnActionReached += OnActionReached;
            ComboReplayPlayer.OnComboReached += OnComboReached;
            ComboReplayPlayer.OnReplayFinished += OnReplayFinished;
            ComboReplayPlayer.OnTimelineUpdated += OnTimelineUpdated;

            // 初始隐藏
            Hide();
        }

        protected override void InitializeUI()
        {
            // 创建主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_mainPanel);

            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.97f);
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
            _mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _mainPanel.AddChild(_mainVBox);

            // 标题栏
            _titleBar = new HBoxContainer();
            _mainVBox.AddChild(_titleBar);

            _titleLabel = new Label();
            _titleLabel.Text = "⚔️ Combo 回放";
            _titleLabel.AddThemeFontSizeOverride("font_size", 18);
            _titleBar.AddChild(_titleLabel);

            _titleBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });

            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.Pressed += Hide;
            _titleBar.AddChild(_closeButton);

            // 信息栏
            _infoBar = new HBoxContainer();
            _mainVBox.AddChild(_infoBar);

            _sceneLabel = new Label();
            _sceneLabel.Text = "场景: -";
            _sceneLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            _sceneLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _infoBar.AddChild(_sceneLabel);

            _resultLabel = new Label();
            _resultLabel.Text = "结果: -";
            _resultLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            _resultLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _infoBar.AddChild(_resultLabel);

            _durationLabel = new Label();
            _durationLabel.Text = "时长: -";
            _durationLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
            _durationLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _infoBar.AddChild(_durationLabel);

            _seedLabel = new Label();
            _seedLabel.Text = "Seed: -";
            _seedLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
            _seedLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _infoBar.AddChild(_seedLabel);

            // 时间轴
            _timelineContainer = new HBoxContainer();
            _timelineContainer.CustomMinimumSize = new Vector2(0, 32);
            _mainVBox.AddChild(_timelineContainer);

            _currentTimeLabel = new Label();
            _currentTimeLabel.Text = "0.0s";
            _currentTimeLabel.CustomMinimumSize = new Vector2(60, 0);
            _currentTimeLabel.AddThemeFontSizeOverride("font_size", 13);
            _timelineContainer.AddChild(_currentTimeLabel);

            _timelineSlider = new HSlider();
            _timelineSlider.MinValue = 0;
            _timelineSlider.Step = 0.1;
            _timelineSlider.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _timelineSlider.ValueChanged += OnSliderValueChanged;
            _timelineContainer.AddChild(_timelineSlider);

            _totalTimeLabel = new Label();
            _totalTimeLabel.Text = "0.0s";
            _totalTimeLabel.CustomMinimumSize = new Vector2(60, 0);
            _totalTimeLabel.AddThemeFontSizeOverride("font_size", 13);
            _timelineContainer.AddChild(_totalTimeLabel);

            // 播放控制条
            _controlsBar = new HBoxContainer();
            _mainVBox.AddChild(_controlsBar);

            _rewindButton = new Button();
            _rewindButton.Text = "⏮";
            _rewindButton.Pressed += OnRewindPressed;
            _controlsBar.AddChild(_rewindButton);

            _playPauseButton = new Button();
            _playPauseButton.Text = "▶";
            _playPauseButton.CustomMinimumSize = new Vector2(60, 0);
            _playPauseButton.Pressed += OnPlayPausePressed;
            _controlsBar.AddChild(_playPauseButton);

            _fastForwardButton = new Button();
            _fastForwardButton.Text = "⏭";
            _fastForwardButton.Pressed += OnFastForwardPressed;
            _controlsBar.AddChild(_fastForwardButton);

            _controlsBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });

            _speedLabel = new Label();
            _speedLabel.Text = "速度:";
            _speedLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
            _controlsBar.AddChild(_speedLabel);

            _speedButton = new Button();
            _speedButton.Text = "1x";
            _speedButton.Pressed += OnSpeedPressed;
            _controlsBar.AddChild(_speedButton);

            // 历史记录区（操作列表）
            _historyScroll = new ScrollContainer();
            _historyScroll.CustomMinimumSize = new Vector2(0, 200);
            _historyScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
            _mainVBox.AddChild(_historyScroll);

            _historyContainer = new VBoxContainer();
            _historyContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            _historyScroll.AddChild(_historyContainer);

            _noActionsLabel = new Label();
            _noActionsLabel.Text = "暂无操作记录";
            _noActionsLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.5f));
            _noActionsLabel.Align = Label.LabelEnum.Center;
            _historyContainer.AddChild(_noActionsLabel);

            GD.Print("[ComboReplayUI] Initialized");
        }

        public override void _Process(double delta)
        {
            if (!IsVisible)
                return;

            // 每 3 帧更新一次（减少开销）
            _frameSkip++;
            if (_frameSkip < 3)
                return;
            _frameSkip = 0;
        }

        /// <summary>
        /// 显示指定回放
        /// </summary>
        public void ShowReplay(ComboReplayData replay)
        {
            if (replay == null)
                return;

            _currentReplay = replay;
            _highlightedActionIndex = -1;

            // 更新信息栏
            _sceneLabel.Text = $"场景: {replay.Metadata.SceneName}";
            _resultLabel.Text = $"结果: {(replay.Metadata.Result == "victory" ? "胜利 ✓" : "失败 ✗")}";
            _resultLabel.AddThemeColorOverride("font_color", replay.Metadata.Result == "victory"
                ? new Color(0.2f, 0.9f, 0.4f)
                : new Color(0.9f, 0.3f, 0.2f));
            _durationLabel.Text = $"时长: {replay.DurationSeconds:F1f}s";
            _seedLabel.Text = $"Seed: {replay.Seed}";

            // 更新时间轴
            _timelineSlider.MaxValue = replay.DurationSeconds;
            _timelineSlider.Value = 0;
            _currentTimeLabel.Text = "0.0s";
            _totalTimeLabel.Text = $"{replay.DurationSeconds:F1f}s";

            // 清空并重建操作列表
            foreach (var child in _historyContainer.GetChildren())
                child.QueueFree();
            _historyContainer.AddChild(_noActionsLabel);
            _noActionsLabel.Visible = replay.Actions.Count == 0;

            if (replay.Actions.Count > 0)
            {
                _noActionsLabel.Visible = false;
                // 预建操作行（前20个）
                int count = Mathf.Min(replay.Actions.Count, 20);
                for (int i = 0; i < count; i++)
                {
                    AddActionRow(replay.Actions[i], i);
                }
                if (replay.Actions.Count > 20)
                {
                    var moreLabel = new Label();
                    moreLabel.Text = $"... 还有 {replay.Actions.Count - 20} 条记录";
                    moreLabel.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.5f));
                    _historyContainer.AddChild(moreLabel);
                }
            }

            // 更新播放按钮状态
            UpdatePlayPauseButton();

            // 开始播放
            ComboReplayPlayer.Instance.LoadAndPlay(replay);
            Show();
        }

        private void AddActionRow(PlayerActionRecord action, int index)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(0, 26);
            panel.Name = $"action_{index}";

            var boxStyle = new StyleBoxFlat();
            boxStyle.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.8f);
            boxStyle.CornerRadiusTopLeft = 3;
            boxStyle.CornerRadiusTopRight = 3;
            boxStyle.CornerRadiusBottomLeft = 3;
            boxStyle.CornerRadiusBottomRight = 3;
            panel.AddThemeStyleboxOverride("panel", boxStyle);

            var hbox = new HBoxContainer();
            panel.AddChild(hbox);

            var timeLabel = new Label();
            timeLabel.Text = $"[{action.Time:F1f}s]";
            timeLabel.CustomMinimumSize = new Vector2(55, 0);
            timeLabel.AddThemeFontSizeOverride("font_size", 12);
            timeLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
            hbox.AddChild(timeLabel);

            var typeLabel = new Label();
            typeLabel.Text = GetActionTypeName(action.Type);
            typeLabel.CustomMinimumSize = new Vector2(90, 0);
            typeLabel.AddThemeFontSizeOverride("font_size", 12);
            hbox.AddChild(typeLabel);

            var detailLabel = new Label();
            detailLabel.Text = GetActionDetail(action);
            detailLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            detailLabel.AddThemeFontSizeOverride("font_size", 12);
            hbox.AddChild(detailLabel);

            // 插入到 _noActionsLabel 之前
            int insertIndex = _historyContainer.GetChildCount();
            _historyContainer.AddChild(panel);
            panel.GetParent()?.MoveChild(panel, insertIndex - 1);
        }

        private string GetActionTypeName(PlayerActionType type)
        {
            switch (type)
            {
                case PlayerActionType.SkillUse: return "🎯 技能";
                case PlayerActionType.ComboCompleted: return "🔥 Combo";
                case PlayerActionType.Movement: return "🏃 移动";
                case PlayerActionType.ItemUsed: return "📦 道具";
                case PlayerActionType.Dodge: return "💨 闪避";
                default: return "❓ 未知";
            }
        }

        private string GetActionDetail(PlayerActionRecord action)
        {
            switch (action.Type)
            {
                case PlayerActionType.SkillUse:
                    return $"使用技能: {action.SkillId}";
                case PlayerActionType.ComboCompleted:
                    return $"Combo完成: {action.SkillId}";
                case PlayerActionType.Movement:
                    return $"移动到 ({action.PlayerPosX:F0}, {action.PlayerPosY:F0})";
                case PlayerActionType.ItemUsed:
                    return $"使用道具: {action.SkillId}";
                case PlayerActionType.Dodge:
                    return "闪避";
                default:
                    return "";
            }
        }

        private void OnActionReached(PlayerActionRecord action)
        {
            // 高亮对应行（滚动到可见区域）
            // 简化处理：只更新当前时间标签
        }

        private void OnComboReached(ComboRecord combo)
        {
            // Combo 触发时可以在历史区插入高亮条目
        }

        private void OnReplayFinished()
        {
            _playPauseButton.Text = "↺";
            _currentTimeLabel.Text = $"{_currentReplay?.DurationSeconds:F1f}s";
        }

        private void OnTimelineUpdated(float elapsed, float total)
        {
            if (_timelineSlider != null && Math.Abs(_timelineSlider.Value - elapsed) > 0.5f)
            {
                _timelineSlider.SetValueNoSignal(elapsed);
            }
            _currentTimeLabel.Text = $"{elapsed:F1f}s";
        }

        private void OnSliderValueChanged(double value)
        {
            ComboReplayPlayer.Instance?.SeekTo((float)value);
        }

        private void OnPlayPausePressed()
        {
            var player = ComboReplayPlayer.Instance;
            if (player == null)
                return;

            if (player.IsPlaying())
            {
                player.Pause();
            }
            else
            {
                player.Play();
            }
            UpdatePlayPauseButton();
        }

        private void OnRewindPressed()
        {
            ComboReplayPlayer.Instance?.SeekTo(0f);
        }

        private void OnFastForwardPressed()
        {
            var player = ComboReplayPlayer.Instance;
            if (player == null)
                return;
            player.SeekTo(player.GetTotalDuration());
        }

        private void OnSpeedPressed()
        {
            var player = ComboReplayPlayer.Instance;
            if (player == null)
                return;

            float[] speeds = { 0.5f, 1f, 1.5f, 2f, 3f };
            float current = player.GetPlaybackSpeed();
            float next = 1f;
            for (int i = 0; i < speeds.Length; i++)
            {
                if (Math.Abs(speeds[i] - current) < 0.01f)
                {
                    next = speeds[(i + 1) % speeds.Length];
                    break;
                }
            }
            player.SetPlaybackSpeed(next);
            _speedButton.Text = $"{next}x";
        }

        private void UpdatePlayPauseButton()
        {
            var player = ComboReplayPlayer.Instance;
            if (player != null && player.IsPlaying())
                _playPauseButton.Text = "⏸";
            else
                _playPauseButton.Text = "▶";
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
            ComboReplayPlayer.Instance?.Stop();
        }

        public override void _ExitTree()
        {
            ComboReplayPlayer.OnActionReached -= OnActionReached;
            ComboReplayPlayer.OnComboReached -= OnComboReached;
            ComboReplayPlayer.OnReplayFinished -= OnReplayFinished;
            ComboReplayPlayer.OnTimelineUpdated -= OnTimelineUpdated;
        }
    }
}
