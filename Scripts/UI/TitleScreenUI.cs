using Godot;
using System;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 标题画面UI - 游戏启动时的初始界面
    /// 提供开始游戏、加载存档、设置、退出等功能
    /// </summary>
    public partial class TitleScreenUI : Control
    {
        // UI 组件
        private ColorRect _background;
        private Label _titleLabel;
        private Label _subtitleLabel;
        private VBoxContainer _menuContainer;
        private Button _newGameButton;
        private Button _continueButton;
        private Button _settingsButton;
        private Button _quitButton;
        
        // 动画
        private float _animationTime = 0f;
        private bool _isVisible = false;
        
        // 设置界面引用
        private Control _settingsUI;
        
        public override void _Ready()
        {
            SetupUI();
            Visible = false;
        }
        
        private void SetupUI()
        {
            // 背景遮罩
            _background = new ColorRect
            {
                Color = new Color(0.05f, 0.05f, 0.1f, 0.95f),
                AnchorsPreset = Control.LayoutPreset.FullRect
            };
            AddChild(_background);
            
            // 标题标签
            _titleLabel = new Label
            {
                Text = "🦖 ClawRPG",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 72)
            };
            _titleLabel.Position = new Vector2(0, 150);
            _titleLabel.Size = new Vector2(1280, 120);
            AddChild(_titleLabel);
            
            // 副标题
            _subtitleLabel = new Label
            {
                Text = "- 冒险之旅 -",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                AddThemeFontSizeOverride("font_size", 28)
            };
            _subtitleLabel.Position = new Vector2(0, 260);
            _subtitleLabel.Size = new Vector2(1280, 50);
            AddChild(_subtitleLabel);
            
            // 菜单容器
            _menuContainer = new VBoxContainer
            {
                Position = new Vector2(540, 380),
                Size = new Vector2(200, 250)
            };
            _menuContainer.AddThemeConstantOverride("separation", 20);
            AddChild(_menuContainer);
            
            // 新建游戏按钮
            _newGameButton = CreateMenuButton("🆕 新游戏");
            _newGameButton.Pressed += OnNewGamePressed;
            _menuContainer.AddChild(_newGameButton);
            
            // 继续游戏按钮
            _continueButton = CreateMenuButton("📂 继续游戏");
            _continueButton.Pressed += OnContinuePressed;
            _menuContainer.AddChild(_continueButton);
            
            // 设置按钮
            _settingsButton = CreateMenuButton("⚙️ 设置");
            _settingsButton.Pressed += OnSettingsPressed;
            _menuContainer.AddChild(_settingsButton);
            
            // 退出按钮
            _quitButton = CreateMenuButton("❌ 退出");
            _quitButton.Pressed += OnQuitPressed;
            _menuContainer.AddChild(_quitButton);
            
            // 版本信息
            var versionLabel = new Label
            {
                Text = "v1.0.0 | Made with ❤️",
                HorizontalAlignment = HorizontalAlignment.Right,
                AddThemeFontSizeOverride("font_size", 14)
            };
            versionLabel.Position = new Vector2(1050, 680);
            versionLabel.Size = new Vector2(200, 30);
            AddChild(versionLabel);
            
            // 检查存档状态
            UpdateContinueButton();
        }
        
        private Button CreateMenuButton(string text)
        {
            var button = new Button
            {
                Text = text,
                CustomMinimumSize = new Vector2(200, 50)
            };
            button.AddThemeFontSizeOverride("font_size", 22);
            
            // 设置按钮样式
            var normalStyle = new StyleBoxFlat
            {
                BgColor = new Color(0.2f, 0.2f, 0.3f, 0.8f),
                BorderWidthBottom = 2,
                BorderWidthTop = 2,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderColor = new Color(0.4f, 0.4f, 0.6f, 1f),
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8
            };
            
            var hoverStyle = new StyleBoxFlat
            {
                BgColor = new Color(0.3f, 0.3f, 0.5f, 0.9f),
                BorderWidthBottom = 2,
                BorderWidthTop = 2,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderColor = new Color(0.6f, 0.6f, 0.9f, 1f),
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8
            };
            
            var pressedStyle = new StyleBoxFlat
            {
                BgColor = new Color(0.4f, 0.4f, 0.6f, 1f),
                BorderWidthBottom = 2,
                BorderWidthTop = 2,
                BorderWidthLeft = 2,
                BorderWidthRight = 2,
                BorderColor = new Color(0.8f, 0.8f, 1f, 1f),
                CornerRadiusTopLeft = 8,
                CornerRadiusTopRight = 8,
                CornerRadiusBottomLeft = 8,
                CornerRadiusBottomRight = 8
            };
            
            button.AddThemeStyleboxOverride("normal", normalStyle);
            button.AddThemeStyleboxOverride("hover", hoverStyle);
            button.AddThemeStyleboxOverride("pressed", pressedStyle);
            
            return button;
        }
        
        private void UpdateContinueButton()
        {
            // 检查是否有存档可以继续
            bool hasSave = CheckSaveExists();
            _continueButton.Disabled = !hasSave;
            _continueButton.Modulate = hasSave ? new Color(1, 1, 1, 1) : new Color(1, 1, 1, 0.4f);
        }
        
        private bool CheckSaveExists()
        {
            // 检查存档文件是否存在
            string savePath = OS.GetUserDataDir() + "/savegame_0.json";
            return FileAccess.FileExists(savePath);
        }
        
        public void ShowTitleScreen()
        {
            Visible = true;
            _isVisible = true;
            _animationTime = 0f;
            UpdateContinueButton();
            
            // 获取主节点并暂停游戏
            var main = GetTree().Root.GetNode<Main>("Main");
            if (main != null)
            {
                main.SetGameState(Main.GameState.TitleScreen);
            }
            
            GetTree().Paused = true;
        }
        
        public void HideTitleScreen()
        {
            _isVisible = false;
            Visible = false;
            GetTree().Paused = false;
        }
        
        private void OnNewGamePressed()
        {
            // 开始新游戏
            var main = GetTree().Root.GetNode<Main>("Main");
            if (main != null)
            {
                main.StartNewGame();
            }
            HideTitleScreen();
        }
        
        private void OnContinuePressed()
        {
            // 继续游戏（加载存档）
            var main = GetTree().Root.GetNode<Main>("Main");
            if (main != null)
            {
                main.LoadGame(0); // 加载第一个存档槽
            }
            HideTitleScreen();
        }
        
        private void OnSettingsPressed()
        {
            // 打开设置界面
            var main = GetTree().Root.GetNode<Main>("Main");
            if (main != null)
            {
                main.ToggleSettings();
            }
        }
        
        private void OnQuitPressed()
        {
            // 退出游戏
            GetTree().Quit();
        }
        
        public override void _Process(double delta)
        {
            if (!_isVisible) return;
            
            _animationTime += (float)delta;
            
            // 标题动画 - 轻微上下浮动
            float titleY = 150 + Mathf.Sin(_animationTime * 2f) * 5f;
            _titleLabel.Position = new Vector2(0, titleY);
            
            // 副标题淡入淡出
            float subtitleAlpha = 0.7f + Mathf.Sin(_animationTime * 1.5f) * 0.3f;
            _subtitleLabel.Modulate = new Color(1, 1, 1, subtitleAlpha);
            
            // 按钮发光效果
            float glow = 0.8f + Mathf.Sin(_animationTime * 3f) * 0.2f;
            foreach (var child in _menuContainer.GetChildren())
            {
                if (child is Button btn && !btn.Disabled)
                {
                    btn.Modulate = new Color(glow, glow, glow, 1);
                }
            }
        }
        
        // 处理键盘输入
        public override void _Input(InputEvent @event)
        {
            if (@event is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.Escape)
                {
                    // ESC 键返回或退出
                    if (Visible)
                    {
                        HideTitleScreen();
                    }
                }
            }
        }
    }
}
