using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 教程UI - 显示教程提示和高亮
    /// </summary>
    public partial class TutorialUI : Control {
        private static TutorialUI _instance;
        public static TutorialUI Instance {
            get {
                if (_instance == null) {
                    _instance = new TutorialUI();
                }
                return _instance;
            }
        }

        // 教程面板
        private PanelContainer _panel;
        private VBoxContainer _content;
        private Label _titleLabel;
        private Label _descriptionLabel;
        private Label _targetLabel;
        private ProgressBar _progressBar;
        private Button _skipButton;
        private Button _nextButton;

        // 高亮效果
        private ColorRect _highlightOverlay;
        private ColorRect _highlightCircle;
        private Label _actionLabel;
        private Label _stepCounter;  // 教程步骤计数器

        // 当前教程
        private TutorialStep _currentStep;
        private float _remainingTime;
        private bool _isActive;
        private List<TutorialStep> _pendingSteps = new List<TutorialStep>();
        private int _completedSteps = 0;
        private int _totalSteps = 0;
        
        // 动画
        private Tween _pulseTween;
        
        // 样式
        private Color _panelBgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        private Color _titleColor = new Color(1f, 0.84f, 0f, 1f);
        private Color _descriptionColor = new Color(0.9f, 0.9f, 0.9f, 1f);
        private Color _targetColor = new Color(0.4f, 0.8f, 1f, 1f);
        private Color _highlightColor = new Color(1f, 0.9f, 0.3f, 0.4f);
        private Color _successColor = new Color(0.3f, 0.9f, 0.5f, 1f);
        
        public bool IsActive => _isActive;
        public TutorialStep CurrentStep => _currentStep;

        public TutorialUI() {
            _instance = this;
            Name = "TutorialUI";
        }

        public override void _Ready() {
            // 确保在场景树中
            if (GetParent() == null) {
                return;
            }
            
            SetupUI();
            ConnectSignals();
        }

        private void SetupUI() {
            // 主面板
            _panel = new PanelContainer();
            _panel.AnchorLeft = 0.5f;
            _panel.AnchorRight = 0.5f;
            _panel.OffsetTop = 100;
            _panel.OffsetBottom = 280;
            _panel.OffsetLeft = -250;
            _panel.OffsetRight = 250;
            _panel.Modulate = new Color(1f, 1f, 1f, 0f);
            
            var style = new StyleBoxFlat();
            style.BgColor = _panelBgColor;
            style.BorderWidthLeft = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = _titleColor;
            style.CornerRadiusTopLeft = 8;
            style.CornerRadiusTopRight = 8;
            style.CornerRadiusBottomLeft = 8;
            style.CornerRadiusBottomRight = 8;
            _panel.AddThemeStyleboxOverride("panel", style);
            
            // 内容
            _content = new VBoxContainer();
            _content.AddThemeConstantOverride("separation", 10);
            
            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "教程";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _titleLabel.AddThemeColorOverride("font_color", _titleColor);
            _titleLabel.AddThemeFontSizeOverride("font_size", 20);
            
            // 描述
            _descriptionLabel = new Label();
            _descriptionLabel.Text = "";
            _descriptionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _descriptionLabel.Autowrap = true;
            _descriptionLabel.AddThemeColorOverride("font_color", _descriptionColor);
            _descriptionLabel.AddThemeFontSizeOverride("font_size", 14);
            
            // 目标操作
            _targetLabel = new Label();
            _targetLabel.Text = "";
            _targetLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _targetLabel.AddThemeColorOverride("font_color", _targetColor);
            _targetLabel.AddThemeFontSizeOverride("font_size", 16);
            
            // 进度条
            _progressBar = new ProgressBar();
            _progressBar.MinValue = 0;
            _progressBar.MaxValue = 1;
            _progressBar.Value = 1;
            _progressBar.CustomMinimumSize = new Vector2(400, 8);
            
            // 按钮容器
            var buttonContainer = new HBoxContainer();
            buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
            buttonContainer.AddThemeConstantOverride("separation", 20);
            
            // 跳过按钮
            _skipButton = new Button();
            _skipButton.Text = "跳过";
            _skipButton.CustomMinimumSize = new Vector2(80, 32);
            
            // 下一步按钮
            _nextButton = new Button();
            _nextButton.Text = "知道了";
            _nextButton.CustomMinimumSize = new Vector2(100, 32);
            
            buttonContainer.AddChild(_skipButton);
            buttonContainer.AddChild(_nextButton);
            
            _content.AddChild(_titleLabel);
            _content.AddChild(_descriptionLabel);
            _content.AddChild(_targetLabel);
            _content.AddChild(_progressBar);
            _content.AddChild(buttonContainer);
            
            _panel.AddChild(_content);
            AddChild(_panel);
            
            // 高亮覆盖层
            _highlightOverlay = new ColorRect();
            _highlightOverlay.Color = new Color(0f, 0f, 0f, 0.5f);
            _highlightOverlay.Visible = false; 
            AddChild(_highlightOverlay);
            
            // 高亮圆圈
            _highlightCircle = new ColorRect();
            _highlightCircle.Color = _highlightColor;
            _highlightCircle.CustomMinimumSize = new Vector2(60, 60);
            _highlightCircle.Visible = false; 
            AddChild(_highlightCircle);
            
            // 操作提示
            _actionLabel = new Label();
            _actionLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _actionLabel.AddThemeColorOverride("font_color", Colors.White);
            _actionLabel.AddThemeFontSizeOverride("font_size", 18);
            _actionLabel.Visible = false; 
            AddChild(_actionLabel);
            
            // 步骤计数器
            _stepCounter = new Label();
            _stepCounter.Text = "1/1";
            _stepCounter.HorizontalAlignment = HorizontalAlignment.Right;
            _stepCounter.AddThemeColorOverride("font_color", _targetColor);
            _stepCounter.AddThemeFontSizeOverride("font_size", 12);
            _stepCounter.Position = new Vector2(420, 15);
            _stepCounter.Visible = false; 
            _panel.AddChild(_stepCounter);
            
            // 初始隐藏
            Visible = false; 
            
            // 计算总步骤数
            _totalSteps = TutorialDatabase.Instance.GetAllSteps().Count;
        }

        private void ConnectSignals() {
            if (_skipButton != null) {
                _skipButton.Pressed += OnSkipPressed;
            }
            if (_nextButton != null) {
                _nextButton.Pressed += OnNextPressed;
            }
        }

        /// <summary>
        /// 开始教程步骤
        /// </summary>
        public void StartTutorial(TutorialStep step) {
            if (step == null || _isActive) return;
            
            _currentStep = step;
            _isActive = true;
            _remainingTime = step.Duration;
            
            // 更新UI
            _titleLabel.Text = step.Title;
            _descriptionLabel.Text = step.Description;
            
            // 更新步骤计数器
            _completedSteps = 0;
            foreach (var s in TutorialDatabase.Instance.GetAllSteps()) {
                if (s.IsCompleted) _completedSteps++;
            }
            _stepCounter.Text = $"{_completedSteps + 1}/{_totalSteps}";
            _stepCounter.Visible = true;
            
            // 目标操作
            if (step.TargetType != TutorialTargetType.None && !string.IsNullOrEmpty(step.TargetAction)) {
                _targetLabel.Text = $"请按: {step.TargetAction}";
                _targetLabel.Visible = true;
            } else {
                _targetLabel.Visible = false; 
            }
            
            // 进度条
            if (step.Duration > 0) {
                _progressBar.Visible = true;
                _progressBar.MaxValue = step.Duration;
                _progressBar.Value = step.Duration;
            } else {
                _progressBar.Visible = false; 
            }
            
            // 按钮
            _skipButton.Visible = step.CanSkip;
            _nextButton.Text = step.Duration > 0 ? "继续" : "知道了";
            
            // 高亮
            if (step.TargetType != TutorialTargetType.None) {
                ShowHighlight(step);
            } else {
                _highlightOverlay.Visible = false; 
                _highlightCircle.Visible = false; 
                _actionLabel.Visible = false; 
            }
            
            // 显示
            Visible = true;
            
            // 淡入动画 + 缩放效果
            var tween = CreateTween();
            _panel.Scale = new Vector2(0.8f, 0.8f);
            tween.SetParallel(true);
            tween.TweenProperty(_panel, "modulate:a", 1f, 0.3f);
            tween.TweenProperty(_panel, "scale", new Vector2(1f, 1f), 0.3f).SetTrans(Tween.TransitionType.Back);
            
            GD.Print($"[TutorialUI] Started tutorial: {step.StepId} ({_completedSteps + 1}/{_totalSteps})");
        }

        private void ShowHighlight(TutorialStep step) {
            // 高亮位置
            if (step.HighlightPosition != Vector2.Zero) {
                _highlightCircle.Position = step.HighlightPosition - _highlightCircle.CustomMinimumSize / 2;
                _highlightCircle.Visible = true;
                _actionLabel.Position = step.HighlightPosition + new Vector2(0, 40);
                _actionLabel.Text = step.TargetAction;
                _actionLabel.Visible = true;
                
                // 动画效果
                var tween = CreateTween();
                tween.SetLoops(-1);
                tween.TweenProperty(_highlightCircle, "modulate:a", 0.8f, 0.5f);
                tween.TweenProperty(_highlightCircle, "modulate:a", 0.4f, 0.5f);
            } else {
                _highlightCircle.Visible = false; 
                _actionLabel.Visible = false; 
            }
            
            _highlightOverlay.Visible = true;
        }

        /// <summary>
        /// 结束当前教程
        /// </summary>
        public void EndTutorial() {
            if (!_isActive) return;
            
            _isActive = false; 
            
            if (_currentStep != null) {
                _currentStep.IsCompleted = true;
            }
            
            // 淡出动画 + 缩放效果
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.TweenProperty(_panel, "modulate:a", 0f, 0.3f);
            tween.TweenProperty(_panel, "scale", new Vector2(1.1f, 1.1f), 0.3f);
            tween.TweenCallback(Callable.From(() => {
                Visible = false; 
                _panel.Scale = new Vector2(1f, 1f);
                _highlightOverlay.Visible = false; 
                _highlightCircle.Visible = false; 
                _actionLabel.Visible = false; 
                _stepCounter.Visible = false; 
                
                // 检查是否全部完成
                if (IsAllTutorialsCompleted()) {
                    ShowCompletionMessage();
                }
            }));
            
            // 检查下一个教程
            if (_pendingSteps.Count > 0) {
                var nextStep = _pendingSteps[0];
                _pendingSteps.RemoveAt(0);
                CallDeferred("StartTutorial", nextStep);
            }
            
            GD.Print($"[TutorialUI] Ended tutorial: {_currentStep?.StepId}");
        }
        
        /// <summary>
        /// 显示完成消息
        /// </summary>
        private void ShowCompletionMessage() {
            var completionStep = new TutorialStep {
                StepId = "completion",
                Title = "🎉 恭喜完成所有教程！",
                Description = "你已经掌握了游戏的基础操作。祝你冒险愉快！",
                Duration = 5f,
                CanSkip = true
            };
            
            CallDeferred("StartTutorial", completionStep);
        }

        /// <summary>
        /// 触发教程事件
        /// </summary>
        public void TriggerTutorial(TutorialTrigger trigger) {
            var steps = TutorialDatabase.Instance.GetStepsByTrigger(trigger);
            if (steps.Count > 0) {
                StartTutorial(steps[0]);
                // 添加剩余的到队列
                for (int i = 1; i < steps.Count; i++) {
                    _pendingSteps.Add(steps[i]);
                }
            }
        }

        /// <summary>
        /// 手动开始教程
        /// </summary>
        public void StartTutorialById(string stepId) {
            var step = TutorialDatabase.Instance.GetStep(stepId);
            if (step != null && !step.IsCompleted) {
                StartTutorial(step);
            }
        }

        /// <summary>
        /// 检查是否已完成所有教程
        /// </summary>
        public bool IsAllTutorialsCompleted() {
            return TutorialDatabase.Instance.GetIncompleteSteps().Count == 0;
        }

        /// <summary>
        /// 重置所有教程进度
        /// </summary>
        public void ResetAllTutorials() {
            foreach (var step in TutorialDatabase.Instance.GetAllSteps()) {
                step.IsCompleted = false; 
            }
            _pendingSteps.Clear();
            EndTutorial();
        }

        public override void _Process(double delta) {
            if (!_isActive || _currentStep == null) return;
            
            // 计时器
            if (_currentStep.Duration > 0) {
                _remainingTime -= delta;
                _progressBar.Value = _remainingTime;
                
                if (_remainingTime <= 0) {
                    EndTutorial();
                }
            }
        }

        private void OnSkipPressed() {
            EndTutorial();
        }

        private void OnNextPressed() {
            EndTutorial();
        }

        /// <summary>
        /// 显示教程提示（快捷方式）
        /// </summary>
        public static void ShowTutorial(string title, string description, float duration = 3f) {
            var step = new TutorialStep {
                StepId = "temp",
                Title = title,
                Description = description,
                Duration = duration,
                CanSkip = true
            };
            
            Instance.StartTutorial(step);
        }

        /// <summary>
        /// 显示操作提示
        /// </summary>
        public static void ShowActionHint(string action, string hint) {
            Instance._actionLabel.Text = action;
            Instance._actionLabel.Visible = true;
            
            // 延迟隐藏
            Instance.CallDeferred(nameof(HideActionHint), 2.0f);
        }

        private void HideActionHint(float delay) {
            var timer = GetTree().CreateTimer(delay);
            timer.Timeout += () => {
                if (_actionLabel != null) {
                    _actionLabel.Visible = false; 
                }
            };
        }
    }
}
