using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// 宠物守护系统界面
    /// </summary>
    public partial class PetGuardianUI : Control
    {
        private static PetGuardianUI _instance;
        public static PetGuardianUI Instance => _instance;
        
        // UI 组件
        private PanelContainer _mainPanel;
        private VBoxContainer _content;
        private Label _titleLabel;
        private CheckButton _guardianToggle;
        private Label _statusLabel;
        
        // 统计标签
        private Label _activePetsLabel;
        private Label _enemiesDefeatedLabel;
        
        // 宠物列表
        private ScrollContainer _petListContainer;
        private VBoxContainer _petList;
        
        // 样式
        private Color _activeColor = new Color(0.2f, 0.8f, 0.2f);
        private Color _inactiveColor = new Color(0.6f, 0.6f, 0.6f);
        
        public override void _Ready()
        {
            _instance = this;
            SetupUI();
            GD.Print("宠物守护UI已初始化");
        }
        
        private void SetupUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.CenterRight);
            _mainPanel.Position = new Vector2(-320, 100);
            _mainPanel.CustomMinimumSize = new Vector2(280, 400);
            AddChild(_mainPanel);
            
            // 样式
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            _mainPanel.AddThemeStyleboxOverride("panel", style);
            
            // 内容容器
            _content = new VBoxContainer();
            _content.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _content.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(_content);
            
            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "🐾 宠物守护";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeFontSizeOverride("font_size", 20);
            _content.AddChild(_titleLabel);
            
            // 分隔线
            _content.AddChild(CreateSeparator());
            
            // 守护模式开关
            var toggleContainer = new HBoxContainer();
            _content.AddChild(toggleContainer);
            
            var toggleLabel = new Label();
            toggleLabel.Text = "守护模式: ";
            toggleContainer.AddChild(toggleLabel);
            
            _guardianToggle = new CheckButton();
            _guardianToggle.Text = "关闭";
            _guardianToggle.Toggled += OnGuardianToggle;
            toggleContainer.AddChild(_guardianToggle);
            
            // 状态标签
            _statusLabel = new Label();
            _statusLabel.Text = "点击上方按钮激活守护";
            _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _statusLabel.AddThemeColorOverride("font_color", _inactiveColor);
            _content.AddChild(_statusLabel);
            
            // 分隔线
            _content.AddChild(CreateSeparator());
            
            // 统计区域
            var statsContainer = new VBoxContainer();
            _content.AddChild(statsContainer);
            
            var statsTitle = new Label();
            statsTitle.Text = "📊 守护统计";
            statsTitle.AddThemeFontSizeOverride("font_size", 16);
            statsContainer.AddChild(statsTitle);
            
            _activePetsLabel = new Label();
            _activePetsLabel.Text = "激活宠物: 0";
            statsContainer.AddChild(_activePetsLabel);
            
            _enemiesDefeatedLabel = new Label();
            _enemiesDefeatedLabel.Text = "击败敌人: 0";
            statsContainer.AddChild(_enemiesDefeatedLabel);
            
            // 分隔线
            _content.AddChild(CreateSeparator());
            
            // 说明区域
            var helpContainer = new VBoxContainer();
            _content.AddChild(helpContainer);
            
            var helpTitle = new Label();
            helpTitle.Text = "ℹ️ 说明";
            helpTitle.AddThemeFontSizeOverride("font_size", 16);
            helpContainer.AddChild(helpTitle);
            
            var helpText = new Label();
            helpText.Text = "守护模式下，宠物会在\n玩家周围巡逻，自动攻击\n靠近的敌人";
            helpText.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            helpContainer.AddChild(helpText);
            
            // 分隔线
            _content.AddChild(CreateSeparator());
            
            // 快捷键说明
            var shortcutLabel = new Label();
            shortcutLabel.Text = "快捷键: Ctrl+G 切换显示";
            shortcutLabel.HorizontalAlignment = HorizontalAlignment.Center;
            shortcutLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            shortcutLabel.AddThemeFontSizeOverride("font_size", 12);
            _content.AddChild(shortcutLabel);
            
            // 初始隐藏
            Visible = false;
        }
        
        private Control CreateSeparator()
        {
            var separator = new Control();
            separator.CustomMinimumSize = new Vector2(0, 5);
            return separator;
        }
        
        private void OnGuardianToggle(bool pressed)
        {
            if (PetGuardianSystem.Instance == null)
                return;
                
            if (pressed)
            {
                // 激活守护模式
                PetGuardianSystem.Instance.ActivateGuardianMode("default_pet");
                _guardianToggle.Text = "开启";
                _statusLabel.Text = "守护模式已激活";
                _statusLabel.AddThemeColorOverride("font_color", _activeColor);
            }
            else
            {
                // 停用守护模式
                PetGuardianSystem.Instance.DeactivateAll();
                _guardianToggle.Text = "关闭";
                _statusLabel.Text = "守护模式已停用";
                _statusLabel.AddThemeColorOverride("font_color", _inactiveColor);
            }
            
            UpdateStatistics();
        }
        
        /// <summary>
        /// 更新统计显示
        /// </summary>
        public void UpdateStatistics()
        {
            if (PetGuardianSystem.Instance == null)
                return;
                
            var stats = PetGuardianSystem.Instance.GetStatistics();
            
            _activePetsLabel.Text = $"激活宠物: {stats["active_pets"]}";
            _enemiesDefeatedLabel.Text = $"击败敌人: {stats["total_enemies_defeated"]}";
        }
        
        /// <summary>
        /// 切换显示
        /// </summary>
        public void Toggle()
        {
            Visible = !Visible;
            
            if (Visible)
            {
                UpdateStatistics();
            }
        }
        
        public override void _Input(InputEvent evt)
        {
            // Ctrl+G 切换显示
            if (evt is InputEventKey key && key.Pressed && key.Keycode == Key.G)
            {
                if (Input.IsKeyPressed(Key.Ctrl))
                {
                    Toggle();
                }
            }
            
            // ESC 关闭
            if (evt is InputEventKey keyEscape && keyEscape.Pressed && keyEscape.Keycode == Key.Escape)
            {
                if (Visible)
                {
                    Visible = false;
                }
            }
        }
    }
}
