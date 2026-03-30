using Godot;
using System;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// MainMenuUI - 主菜单界面管理
    /// 处理主菜单的显示/隐藏、按钮交互
    /// </summary>
    public partial class MainMenuUI : BaseUI
    {
        public static new MainMenuUI Instance { get; protected set; }

        // 场景引用
        private Main _main;

        // UI 节点引用 (从 CanvasLayer/MainMenuUI 加载)
        private Button _startGameButton;
        private Button _settingsButton;
        private Button _exitButton;

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            LoadNodes();
        }

        private void LoadNodes()
        {
            // 优先从 CanvasLayer 查找
            var canvasLayer = GetTree()?.CurrentScene?.GetNodeOrNull<CanvasLayer>("CanvasLayer");
            if (canvasLayer != null)
            {
                var node = canvasLayer.GetNodeOrNull<Control>("MainMenuUI");
                if (node != null)
                {
                    _startGameButton = node.GetNodeOrNull<Button>("VBox/StartGameButton");
                    _settingsButton = node.GetNodeOrNull<Button>("VBox/SettingsButton");
                    _exitButton = node.GetNodeOrNull<Button>("VBox/ExitButton");
                    return;
                }
            }

            // 降级: 尝试从自身节点树查找
            _startGameButton = GetNodeOrNull<Button>("VBox/StartGameButton");
            _settingsButton = GetNodeOrNull<Button>("VBox/SettingsButton");
            _exitButton = GetNodeOrNull<Button>("VBox/ExitButton");
        }

        public void Initialize(Main main)
        {
            _main = main;
            ConnectButtons();
        }

        private void ConnectButtons()
        {
            if (_startGameButton != null)
                _startGameButton.Pressed += OnStartGamePressed;

            if (_settingsButton != null)
                _settingsButton.Pressed += OnSettingsPressed;

            if (_exitButton != null)
                _exitButton.Pressed += OnExitPressed;
        }

        private void OnStartGamePressed()
        {
            GD.Print("[MainMenuUI] Start Game pressed");
            // 通知 Main 开始新游戏
            _main?.StartNewGame();
        }

        private void OnSettingsPressed()
        {
            GD.Print("[MainMenuUI] Settings pressed");
            // 打开设置界面
            _main?.GetNodeOrNull<Control>("CanvasLayer/SettingsUI")?.Show();
        }

        private void OnExitPressed()
        {
            GD.Print("[MainMenuUI] Exit pressed");
            GetTree().Quit();
        }

        protected override void OnShow()
        {
            GD.Print("[MainMenuUI] Main menu shown");
        }

        protected override void OnHide()
        {
            GD.Print("[MainMenuUI] Main menu hidden");
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                ["UIName"] = UIName
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            base.ImportSaveData(data);
        }

        public override void _ExitTree()
        {
            if (_startGameButton != null)
                _startGameButton.Pressed -= OnStartGamePressed;
            if (_settingsButton != null)
                _settingsButton.Pressed -= OnSettingsPressed;
            if (_exitButton != null)
                _exitButton.Pressed -= OnExitPressed;

            Instance = null;
        }
    }
}
