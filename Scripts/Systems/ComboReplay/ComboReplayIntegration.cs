using Godot;
using System;
using ClawRPG.Scripts.Framework;

namespace ClawRPG.Scripts.Systems.ComboReplay
{
    /// <summary>
    /// Combo 回放 UI 集成（REQ-114-04）
    /// 职责：战斗结束后显示"查看回放"按钮，点击后打开回放列表，选择后进入回放播放
    /// </summary>
    public class ComboReplayIntegration : Node
    {
        private static ComboReplayIntegration _instance;
        public static ComboReplayIntegration Instance => _instance ??= new ComboReplayIntegration();

        // 通知面板（战斗结束后显示）
        private PanelContainer _notificationPanel;
        private Button _viewReplayButton;
        private Label _replayInfoLabel;
        private Timer _autoHideTimer;

        // 当前回放数据（最近一次战斗）
        private ComboReplayData _latestReplay;

        public override void _Ready()
        {
            _instance = this;

            // 订阅战斗结束信号
            var combatHUD = CombatHUDEnhancementSystem.Instance;
            if (combatHUD != null)
            {
                combatHUD.CombatEnded += OnCombatEnded;
            }

            // 订阅录制完成信号（最新回放）
            ComboReplayRecorder.OnReplayRecorded += OnReplayRecorded;

            // 订阅 ComboReplayListUI 的选择回调
            ComboReplayListUI.OnReplaySelected += OnReplaySelected;

            // 初始创建通知面板（但不显示）
            BuildNotificationPanel();

            GD.Print("[ComboReplayIntegration] Initialized");
        }

        public override void _ExitTree()
        {
            var combatHUD = CombatHUDEnhancementSystem.Instance;
            if (combatHUD != null)
            {
                combatHUD.CombatEnded -= OnCombatEnded;
            }
            ComboReplayRecorder.OnReplayRecorded -= OnReplayRecorded;
            ComboReplayListUI.OnReplaySelected -= OnReplaySelected;
        }

        /// <summary>
        /// 构建通知面板
        /// </summary>
        private void BuildNotificationPanel()
        {
            _notificationPanel = new PanelContainer();
            _notificationPanel.SetAnchorsPreset(Control.LayoutPreset.TopRight);
            _notificationPanel.Position = new Vector2(-260, 10);
            _notificationPanel.CustomMinimumSize = new Vector2(240, 80);
            _notificationPanel.Visible = false;

            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.08f, 0.1f, 0.18f, 0.96f);
            style.CornerRadiusTopLeft = 8;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomLeft = 8;
            style.CornerRadiusBottomRight = 8;
            style.border_width_left = 1;
            style.border_width_right = 1;
            style.border_width_top = 1;
            style.border_width_bottom = 1;
            style.BorderColor = new Color(0.4f, 0.35f, 0.1f, 1f);
            style.ContentMarginLeft = 12;
            style.ContentMarginRight = 12;
            style.ContentMarginTop = 8;
            style.ContentMarginBottom = 8;
            _notificationPanel.AddThemeStyleboxOverride("panel", style);

            var root = GetTree().Root;
            if (root != null)
            {
                root.AddChild(_notificationPanel);
            }

            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 4);
            _notificationPanel.AddChild(vbox);

            _replayInfoLabel = new Label();
            _replayInfoLabel.Text = "⚔️ 战斗回放已保存";
            _replayInfoLabel.AddThemeFontSizeOverride("font_size", 13);
            _replayInfoLabel.AddThemeColorOverride("font_color", new Color(0.85f, 0.8f, 0.6f));
            vbox.AddChild(_replayInfoLabel);

            _viewReplayButton = new Button();
            _viewReplayButton.Text = "📜 查看回放列表";
            _viewReplayButton.Pressed += OnViewReplayPressed;
            vbox.AddChild(_viewReplayButton);

            // 自动隐藏计时器
            _autoHideTimer = new Timer();
            _autoHideTimer.WaitTime = 8f;
            _autoHideTimer.OneShot = true;
            _autoHideTimer.Timeout += () =>
            {
                HideNotification();
            };
            if (root != null)
            {
                root.AddChild(_autoHideTimer);
            }
        }

        /// <summary>
        /// 战斗结束回调
        /// </summary>
        private void OnCombatEnded(CombatHUDEnhancementData.CombatRating rating)
        {
            // 延迟显示，等战斗结束动画
            _autoHideTimer?.Start();
            ShowNotification();
        }

        /// <summary>
        /// 录制完成回调
        /// </summary>
        private void OnReplayRecorded(ComboReplayData replay)
        {
            _latestReplay = replay;
            if (replay != null)
            {
                _replayInfoLabel.Text = $"⚔️ {replay.Metadata.SceneName} - {replay.Combos.Count} Combos, {replay.Actions.Count} 操作";
            }
        }

        /// <summary>
        /// 显示通知
        /// </summary>
        private void ShowNotification()
        {
            if (_notificationPanel != null)
            {
                _notificationPanel.Visible = true;
            }
        }

        /// <summary>
        /// 隐藏通知
        /// </summary>
        private void HideNotification()
        {
            if (_notificationPanel != null)
            {
                _notificationPanel.Visible = false;
            }
        }

        /// <summary>
        /// 点击查看回放列表
        /// </summary>
        private void OnViewReplayPressed()
        {
            _autoHideTimer?.Stop();
            HideNotification();

            // 打开回放列表 UI
            if (ComboReplayListUI.Instance != null)
            {
                ComboReplayListUI.Instance.RefreshList();
            }
        }

        /// <summary>
        /// 回放列表选中后开始播放
        /// </summary>
        private void OnReplaySelected(ComboReplayData replay)
        {
            // 打开回放播放器 UI
            if (ComboReplayUI.Instance != null)
            {
                ComboReplayUI.Instance.ShowReplay(replay);
            }
        }

        /// <summary>
        /// 获取最近一次回放
        /// </summary>
        public ComboReplayData GetLatestReplay() => _latestReplay;
    }
}
