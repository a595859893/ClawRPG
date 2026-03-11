using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 事件选择UI
    /// </summary>
    public partial class ChoiceEventUI : Control {
        private static ChoiceEventUI _instance;
        public static ChoiceEventUI Instance {
            get {
                if (_instance == null) {
                    _instance = new ChoiceEventUI();
                }
                return _instance;
            }
        }
        
        // UI 组件
        private Panel _mainPanel;
        private Label _titleLabel;
        private RichTextLabel _descriptionLabel;
        private VBoxContainer _optionsContainer;
        private Button _closeButton;
        private Label _statsLabel;
        
        // 当前事件
        private ChoiceEventData _currentEvent = null;
        
        // 动画
        private Tween _tween;
        
        // 按键绑定
        private bool _isVisible = false;
        
        public ChoiceEventUI() {
            _instance = this;
        }
        
        public void Initialize() {
            // 创建主面板
            _mainPanel = new Panel();
            _mainPanel.SetSize(new Vector2(500, 400));
            _mainPanel.Position = new Vector2(100, 100);
            _mainPanel.Visible = false;
            AddChild(_mainPanel);
            
            // 创建样式
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
            style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
            style.SetBorderWidthAll(2);
            style.SetCornerRadiusAll(8);
            _mainPanel.AddThemeStyleboxOverride("panel", style);
            
            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "事件";
            _titleLabel.Position = new Vector2(20, 15);
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
            _mainPanel.AddChild(_titleLabel);
            
            // 关闭按钮
            _closeButton = new Button();
            _closeButton.Text = "X";
            _closeButton.Position = new Vector2(460, 10);
            _closeButton.Size = new Vector2(30, 30);
            _closeButton.Pressed += OnCloseButtonPressed;
            _mainPanel.AddChild(_closeButton);
            
            // 描述
            _descriptionLabel = new RichTextLabel();
            _descriptionLabel.Position = new Vector2(20, 60);
            _descriptionLabel.Size = new Vector2(460, 100);
            _descriptionLabel.BbcodeEnabled = true;
            _descriptionLabel.AddThemeColorOverride("default_color", new Color(0.9f, 0.9f, 0.9f));
            _mainPanel.AddChild(_descriptionLabel);
            
            // 选项容器
            _optionsContainer = new VBoxContainer();
            _optionsContainer.Position = new Vector2(20, 170);
            _optionsContainer.Size = new Vector2(460, 180);
            _optionsContainer.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(_optionsContainer);
            
            // 统计标签
            _statsLabel = new Label();
            _statsLabel.Text = "";
            _statsLabel.Position = new Vector2(20, 360);
            _statsLabel.AddThemeFontSizeOverride("font_size", 12);
            _statsLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            _mainPanel.AddChild(_statsLabel);
            
            // 连接信号
            if (ChoiceEventSystem.Instance != null) {
                ChoiceEventSystem.Instance.Connect(ChoiceEventSystem.SignalName.EventTriggered, 
                    Callable.From<ChoiceEventData>(OnEventTriggered));
                ChoiceEventSystem.Instance.Connect(ChoiceEventSystem.SignalName.ChoiceMade, 
                    Callable.From<string, string, string>(OnChoiceMade));
            }
            
            UpdateStats();
            GD.Print("[ChoiceEventUI] 事件选择UI已初始化");
        }
        
        /// <summary>
        /// 事件触发回调
        /// </summary>
        private void OnEventTriggered(ChoiceEventData eventData) {
            _currentEvent = eventData;
            ShowEvent(eventData);
        }
        
        /// <summary>
        /// 选择完成回调
        /// </summary>
        private void OnChoiceMade(string eventId, string optionId, string resultText) {
            // 显示结果
            ShowResult(resultText);
            UpdateStats();
            
            // 延迟关闭
            var timer = GetTree().CreateTimer(3.0f);
            timer.Timeout += () => {
                Hide();
            };
        }
        
        /// <summary>
        /// 显示事件
        /// </summary>
        private void ShowEvent(ChoiceEventData eventData) {
            if (eventData == null) return;
            
            _titleLabel.Text = eventData.Title;
            _descriptionLabel.Text = eventData.Description;
            
            // 清除旧选项
            foreach (var child in _optionsContainer.GetChildren()) {
                child.QueueFree();
            }
            
            // 创建新选项
            foreach (var option in eventData.Options) {
                var optionButton = CreateOptionButton(option);
                _optionsContainer.AddChild(optionButton);
            }
            
            Show();
        }
        
        /// <summary>
        /// 创建选项按钮
        /// </summary>
        private Button CreateOptionButton(ChoiceOption option) {
            var button = new Button();
            button.Text = option.Text;
            button.Size = new Vector2(440, 50);
            
            // 样式
            var normalStyle = new StyleBoxFlat();
            normalStyle.BgColor = new Color(0.2f, 0.25f, 0.35f);
            normalStyle.SetCornerRadiusAll(4);
            button.AddThemeStyleboxOverride("normal", normalStyle);
            
            var hoverStyle = new StyleBoxFlat();
            hoverStyle.BgColor = new Color(0.3f, 0.35f, 0.45f);
            hoverStyle.SetCornerRadiusAll(4);
            button.AddThemeStyleboxOverride("hover", hoverStyle);
            
            var pressedStyle = new StyleBoxFlat();
            pressedStyle.BgColor = new Color(0.15f, 0.2f, 0.3f);
            pressedStyle.SetCornerRadiusAll(4);
            button.AddThemeStyleboxOverride("pressed", pressedStyle);
            
            // 文字颜色
            button.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            button.AddThemeColorOverride("font_hover_color", new Color(1f, 0.95f, 0.8f));
            
            // 添加花费信息
            if (option.RequiresGold) {
                button.Text += $" [需要 {option.GoldCost} 金币]";
            }
            
            // 点击事件
            button.Pressed += () => {
                if (ChoiceEventSystem.Instance != null) {
                    ChoiceEventSystem.Instance.MakeChoice(option.OptionId);
                }
            };
            
            return button;
        }
        
        /// <summary>
        /// 显示结果
        /// </summary>
        private void ShowResult(string resultText) {
            _descriptionLabel.Text = resultText;
            
            // 清除选项
            foreach (var child in _optionsContainer.GetChildren()) {
                child.QueueFree();
            }
        }
        
        /// <summary>
        /// 显示UI
        /// </summary>
        public void Show() {
            if (_mainPanel == null) return;
            
            _mainPanel.Visible = true;
            _isVisible = true;
            
            // 淡入动画
            _tween = CreateTween();
            _tween.SetParallel(true);
            _mainPanel.Modulate = new Color(1, 1, 1, 0);
            _tween.TweenProperty(_mainPanel, "modulate:a", 1.0f, 0.3f);
            _tween.TweenProperty(_mainPanel, "scale", new Vector2(0.95f, 0.95f), 0.0f);
            _tween.TweenProperty(_mainPanel, "scale", new Vector2(1.0f, 1.0f), 0.3f).SetTrans(Tween.TransitionType.Back);
        }
        
        /// <summary>
        /// 隐藏UI
        /// </summary>
        public void Hide() {
            if (_mainPanel == null) return;
            
            _isVisible = false;
            _currentEvent = null;
            
            // 淡出动画
            _tween = CreateTween();
            _tween.TweenProperty(_mainPanel, "modulate:a", 0.0f, 0.2f);
            _tween.TweenProperty(_mainPanel, "scale", new Vector2(0.95f, 0.95f), 0.2f);
            _tween.TweenCallback(() => {
                _mainPanel.Visible = false;
                _mainPanel.Scale = new Vector2(1.0f, 1.0f);
            });
        }
        
        /// <summary>
        /// 更新统计
        /// </summary>
        private void UpdateStats() {
            if (ChoiceEventSystem.Instance == null) return;
            
            var stats = ChoiceEventSystem.Instance.GetStatistics();
            _statsLabel.Text = $"总选择次数: {stats["total_choices"]} | 完成事件: {stats["total_events"]} | 获得金币: {stats["total_gold"]} | 获得经验: {stats["total_exp"]}";
        }
        
        /// <summary>
        /// 关闭按钮点击
        /// </summary>
        private void OnCloseButtonPressed() {
            // 跳过当前事件
            if (ChoiceEventSystem.Instance != null) {
                ChoiceEventSystem.Instance.SkipCurrentEvent();
            }
            Hide();
        }
        
        /// <summary>
        /// 切换显示
        /// </summary>
        public void Toggle() {
            if (_isVisible) {
                Hide();
            } else if (_currentEvent != null) {
                Show();
            }
        }
        
        /// <summary>
        /// 检查是否有活跃事件
        /// </summary>
        public bool HasActiveEvent() {
            return _currentEvent != null;
        }
    }
}
