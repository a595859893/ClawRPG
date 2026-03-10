using System;
using System.Collections.Generic;
using Godot;
using System.Text;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 对话框UI - 显示NPC对话内容
    /// 增强功能: 打字机效果、对话历史记录、NPC表情变化
    /// </summary>
    public class DialogueUI : Control {
        // UI组件引用
        private Label _speakerNameLabel;
        private RichTextLabel _dialogueText;
        private VBoxContainer _optionsContainer;
        private TextureRect _portrait;
        private TextureButton _continueButton;
        private PanelContainer _mainPanel;
        
        // 增强: 对话历史记录
        private VBoxContainer _historyContainer;
        private ScrollContainer _historyScroll;
        private PanelContainer _historyPanel;
        
        // 增强: 打字机效果
        private string _fullText = "";
        private string _displayedText = "";
        private float _typeTimer = 0f;
        private float _typeSpeed = 0.03f;
        private bool _isTyping = false;
        private bool _skipTypewriter = false;
        
        // 增强: NPC表情
        private Dictionary<string, Color> _emotionColors = new Dictionary<string, Color>();
        
        // 历史记录
        private List<string> _dialogueHistory = new List<string>();
        private int _maxHistoryItems = 10;

        // 预设
        private Color _speakerNameColor = new Color(1f, 0.84f, 0f); // 金色
        private Color _optionHoverColor = new Color(0.2f, 0.2f, 0.2f);
        private Color _optionNormalColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // 按钮资源
        private PackedScene _optionButtonScene;

        public override void _Ready() {
            base._Ready();
            
            // 初始化表情颜色
            InitializeEmotionColors();
            
            SetupUI();
            Visible = false;
            
            // 连接信号
            Quests.DialogueManager.Instance.DialogueStarted.Connect(OnDialogueStarted);
            Quests.DialogueManager.Instance.DialogueEnded.Connect(OnDialogueEnded);
            Quests.DialogueManager.Instance.NodeChanged.Connect(OnNodeChanged);
        }
        
        private void InitializeEmotionColors() {
            _emotionColors["normal"] = new Color(0.5f, 0.5f, 0.5f);    // 灰色
            _emotionColors["happy"] = new Color(1f, 0.8f, 0.2f);       // 黄色
            _emotionColors["angry"] = new Color(1f, 0.3f, 0.3f);       // 红色
            _emotionColors["sad"] = new Color(0.4f, 0.6f, 1f);         // 蓝色
            _emotionColors["surprised"] = new Color(0.8f, 0.5f, 1f);   // 紫色
            _emotionColors["scared"] = new Color(0.6f, 0.8f, 0.6f);     // 淡绿
        }

        private void SetupUI() {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
            _mainPanel.MarginBottom = -50;
            _mainPanel.MarginLeft = 50;
            _mainPanel.MarginRight = -50;
            _mainPanel.CustomMinimumSize = new Vector2(0, 200);
            AddChild(_mainPanel);

            // 背景样式
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0f, 0f, 0f, 0.85f);
            style.BorderWidthBottom = 4;
            style.BorderColor = new Color(0.6f, 0.4f, 0.2f); // 棕色边框
            style.CornerRadiusTopLeft = 10;
            style.CornerRadiusTopRight = 10;
            _mainPanel.AddThemeStyleboxOverride("panel", style);

            // 主容器
            var mainVBox = new VBoxContainer();
            mainVBox.AddThemeConstantOverride("separation", 10);
            _mainPanel.AddChild(mainVBox);

            // 顶部容器（头像+名字+对话内容）
            var topContainer = new HBoxContainer();
            topContainer.AddThemeConstantOverride("separation", 15);
            mainVBox.AddChild(topContainer);

            // 头像
            _portrait = new TextureRect();
            _portrait.CustomMinimumSize = new Vector2(80, 80);
            _portrait.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            topContainer.AddChild(_portrait);

            // 右侧容器
            var rightVBox = new VBoxContainer();
            rightVBox.AddThemeConstantOverride("separation", 5);
            topContainer.AddChild(rightVBox);

            // 说话者名字
            _speakerNameLabel = new Label();
            _speakerNameLabel.AddThemeColorOverride("font_color", _speakerNameColor);
            _speakerNameLabel.AddThemeFontSizeOverride("font_size", 20);
            rightVBox.AddChild(_speakerNameLabel);

            // 对话内容
            _dialogueText = new RichTextLabel();
            _dialogueText.BbcodeEnabled = true;
            _dialogueText.FitContent = true;
            _dialogueText.CustomMinimumSize = new Vector2(400, 60);
            _dialogueText.AddThemeColorOverride("default_color", new Color(1f, 1f, 1f));
            _dialogueText.AddThemeFontSizeOverride("normal_font_size", 16);
            rightVBox.AddChild(_dialogueText);

            // 选项容器
            _optionsContainer = new VBoxContainer();
            _optionsContainer.AddThemeConstantOverride("separation", 8);
            mainVBox.AddChild(_optionsContainer);

            // 继续按钮（用于没有选项时）
            _continueButton = new TextureButton();
            _continueButton.CustomMinimumSize = new Vector2(30, 30);
            _continueButton.Visible = false;
            _continueButton.Pressed += OnContinuePressed;
            
            var continueStyle = new StyleBoxFlat();
            continueStyle.BgColor = new Color(0.3f, 0.3f, 0.3f);
            continueStyle.CornerRadiusBottomLeft = 5;
            continueStyle.CornerRadiusBottomRight = 5;
            continueStyle.CornerRadiusTopLeft = 5;
            continueStyle.CornerRadiusTopRight = 5;
            _continueButton.AddThemeStyleboxOverride("normal", continueStyle);
            
            var hoverStyle = new StyleBoxFlat();
            hoverStyle.BgColor = new Color(0.5f, 0.5f, 0.5f);
            hoverStyle.CornerRadiusBottomLeft = 5;
            hoverStyle.CornerRadiusBottomRight = 5;
            hoverStyle.CornerRadiusTopLeft = 5;
            hoverStyle.CornerRadiusTopRight = 5;
            _continueButton.AddThemeStyleboxOverride("hover", hoverStyle);
            
            mainVBox.AddChild(_continueButton);

            // 增强: 创建历史记录面板
            SetupHistoryPanel();
            
            // 创建选项按钮场景
            CreateOptionButtonScene();
        }
        
        private void SetupHistoryPanel() {
            // 历史记录面板 (在主对话框上方显示)
            _historyPanel = new PanelContainer();
            _historyPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
            _historyPanel.MarginBottom = -260;
            _historyPanel.MarginLeft = 60;
            _historyPanel.MarginRight = -60;
            _historyPanel.CustomMinimumSize = new Vector2(0, 60);
            _historyPanel.Visible = false;
            AddChild(_historyPanel);
            
            var historyStyle = new StyleBoxFlat();
            historyStyle.BgColor = new Color(0f, 0f, 0f, 0.6f);
            historyStyle.CornerRadiusTopLeft = 8;
            historyStyle.CornerRadiusTopRight = 8;
            _historyPanel.AddThemeStyleboxOverride("panel", historyStyle);
            
            _historyScroll = new ScrollContainer();
            _historyScroll.HScrollEnabled = false;
            _historyScroll.VScrollEnabled = true;
            _historyPanel.AddChild(_historyScroll);
            
            _historyContainer = new VBoxContainer();
            _historyContainer.AddThemeConstantOverride("separation", 4);
            _historyScroll.AddChild(_historyContainer);
        }

        private void CreateOptionButtonScene() {
            // 动态创建选项按钮
        }
        
        public override void _Process(float delta) {
            base._Process(delta);
            
            // 打字机效果
            if (_isTyping && !_skipTypewriter) {
                _typeTimer += delta;
                if (_typeTimer >= _typeSpeed) {
                    _typeTimer = 0f;
                    if (_displayedText.Length < _fullText.Length) {
                        _displayedText = _fullText.Substring(0, _displayedText.Length + 1);
                        _dialogueText.Text = _displayedText;
                    } else {
                        _isTyping = false;
                        ShowOptions();
                    }
                }
            }
        }

        private void OnDialogueStarted() {
            Visible = true;
            _dialogueHistory.Clear();
            _historyPanel.Visible = false;
            UpdateDialogueDisplay();
        }

        private void OnDialogueEnded() {
            Visible = false;
            ClearOptions();
            _isTyping = false;
            _fullText = "";
            _displayedText = "";
        }

        private void OnNodeChanged(string nodeId) {
            UpdateDialogueDisplay();
        }

        private void UpdateDialogueDisplay() {
            var manager = Quests.DialogueManager.Instance;
            var currentNode = manager.CurrentNode;

            if (currentNode == null) return;

            // 更新说话者名字
            _speakerNameLabel.Text = currentNode.SpeakerName ?? "";
            
            // 增强: 设置打字机速度
            _typeSpeed = currentNode.TextRevealSpeed > 0 ? currentNode.TextRevealSpeed : 0.03f;
            
            // 更新对话内容 (打字机效果)
            _fullText = currentNode.Text ?? "";
            _displayedText = "";
            _isTyping = true;
            _skipTypewriter = false;
            
            // 添加到历史记录
            AddToHistory(currentNode.SpeakerName, currentNode.Text);

            // 更新头像和表情
            UpdatePortraitWithEmotion(currentNode.SpeakerPortrait, currentNode.Emotion);
            
            // 应用节点动画
            ApplyNodeAnimation(currentNode.Animation);

            // 清空选项,打字机完成后再显示
            ClearOptions();
        }
        
        private void AddToHistory(string speaker, string text) {
            if (string.IsNullOrEmpty(text)) return;
            
            string historyEntry = $"[b]{speaker}:[/b] {text}";
            _dialogueHistory.Add(historyEntry);
            
            // 限制历史记录数量
            while (_dialogueHistory.Count > _maxHistoryItems) {
                _dialogueHistory.RemoveAt(0);
            }
            
            // 更新历史记录显示
            UpdateHistoryDisplay();
        }
        
        private void UpdateHistoryDisplay() {
            // 清除旧的历史记录
            foreach (var child in _historyContainer.GetChildren()) {
                child.QueueFree();
            }
            
            // 显示最近的几条记录
            int startIndex = Math.Max(0, _dialogueHistory.Count - 3);
            for (int i = startIndex; i < _dialogueHistory.Count; i++) {
                var label = new RichTextLabel();
                label.BbcodeEnabled = true;
                label.Text = _dialogueHistory[i];
                label.FitContent = true;
                label.AddThemeColorOverride("default_color", new Color(0.8f, 0.8f, 0.8f, 0.7f));
                label.AddThemeFontSizeOverride("normal_font_size", 12);
                _historyContainer.AddChild(label);
            }
            
            // 显示历史记录面板
            _historyPanel.Visible = _dialogueHistory.Count > 0;
        }
        
        private void ShowOptions() {
            var manager = Quests.DialogueManager.Instance;
            var currentNode = manager.CurrentNode;

            if (currentNode == null) return;

            var availableOptions = manager.GetAvailableOptions();
            
            if (currentNode.IsEndNode || (currentNode.Options.Count == 0 && !string.IsNullOrEmpty(currentNode.NextNodeId))) {
                // 显示继续按钮
                _continueButton.Visible = true;
                _continueButton.Text = "▼ 点击继续";
            } else if (availableOptions.Count > 0) {
                // 显示选项
                _continueButton.Visible = false;
                foreach (var option in availableOptions) {
                    CreateOptionButton(option);
                }
            } else if (!string.IsNullOrEmpty(currentNode.NextNodeId)) {
                // 自动继续
                _continueButton.Visible = true;
                _continueButton.Text = "▼ 点击继续";
            }
        }

        private void UpdatePortraitWithEmotion(string portraitPath, string emotion) {
            // 增强: 根据表情显示不同颜色的头像边框
            if (_portrait != null) {
                var borderColor = _emotionColors.ContainsKey(emotion) 
                    ? _emotionColors[emotion] 
                    : _emotionColors["normal"];
                
                var placeholderStyle = new StyleBoxFlat();
                placeholderStyle.BgColor = new Color(0.3f, 0.3f, 0.3f);
                placeholderStyle.BorderWidthLeft = 4;
                placeholderStyle.BorderWidthRight = 4;
                placeholderStyle.BorderWidthTop = 4;
                placeholderStyle.BorderWidthBottom = 4;
                placeholderStyle.BorderColor = borderColor;
                placeholderStyle.CornerRadiusTopLeft = 40;
                placeholderStyle.CornerRadiusTopRight = 40;
                placeholderStyle.CornerRadiusBottomLeft = 40;
                placeholderStyle.CornerRadiusBottomRight = 40;
                _portrait.AddThemeStyleboxOverride("normal", placeholderStyle);
                
                GD.Print($"[DialogueUI] Emotion changed: {emotion}, border color: {borderColor}");
            }
        }
        
        private void ApplyNodeAnimation(string animation) {
            if (string.IsNullOrEmpty(animation) || animation == "none") return;
            
            switch (animation) {
                case "fade_in":
                    // 淡入效果
                    var tween = CreateTween();
                    _mainPanel.Modulate = new Color(1, 1, 1, 0);
                    tween.TweenProperty(_mainPanel, "modulate:a", 1f, 0.3f);
                    break;
                    
                case "bounce":
                    // 弹跳效果
                    var bounceTween = CreateTween();
                    var originalPos = _mainPanel.Position;
                    bounceTween.TweenProperty(_mainPanel, "position:y", originalPos.y - 20, 0.15f);
                    bounceTween.TweenProperty(_mainPanel, "position:y", originalPos.y, 0.15f);
                    break;
                    
                case "pulse":
                    // 脉冲效果
                    var pulseTween = CreateTween();
                    pulseTween.SetLoops();
                    pulseTween.TweenProperty(_mainPanel, "scale", new Vector2(1.02f, 1.02f), 0.3f);
                    pulseTween.TweenProperty(_mainPanel, "scale", new Vector2(1f, 1f), 0.3f);
                    break;
            }
        }

        private void CreateOptionButton(Quests.DialogueOption option) {
            var button = new Button();
            button.Text = option.Text;
            button.CustomMinimumSize = new Vector2(400, 40);
            button.Align = Button.TextAlign.Left;
            
            // 样式
            var normalStyle = new StyleBoxFlat();
            normalStyle.BgColor = _optionNormalColor;
            normalStyle.BorderWidthBottom = 2;
            normalStyle.BorderColor = new Color(0.5f, 0.3f, 0.1f);
            normalStyle.CornerRadiusTopLeft = 5;
            normalStyle.CornerRadiusTopRight = 5;
            normalStyle.CornerRadiusBottomLeft = 5;
            normalStyle.CornerRadiusBottomRight = 5;
            button.AddThemeStyleboxOverride("normal", normalStyle);

            var hoverStyle = new StyleBoxFlat();
            hoverStyle.BgColor = _optionHoverColor;
            hoverStyle.BorderWidthBottom = 2;
            hoverStyle.BorderColor = new Color(0.8f, 0.5f, 0.2f);
            hoverStyle.CornerRadiusTopLeft = 5;
            hoverStyle.CornerRadiusTopRight = 5;
            hoverStyle.CornerRadiusBottomLeft = 5;
            hoverStyle.CornerRadiusBottomRight = 5;
            button.AddThemeStyleboxOverride("hover", hoverStyle);

            var pressedStyle = new StyleBoxFlat();
            pressedStyle.BgColor = new Color(0.15f, 0.15f, 0.15f);
            pressedStyle.BorderWidthBottom = 2;
            pressedStyle.BorderColor = new Color(1f, 0.7f, 0.3f);
            pressedStyle.CornerRadiusTopLeft = 5;
            pressedStyle.CornerRadiusTopRight = 5;
            pressedStyle.CornerRadiusBottomLeft = 5;
            pressedStyle.CornerRadiusBottomRight = 5;
            button.AddThemeStyleboxOverride("pressed", pressedStyle);

            button.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
            button.AddThemeColorOverride("font_hover_color", new Color(1f, 0.9f, 0.7f));
            button.AddThemeFontSizeOverride("font_size", 16);

            button.Pressed += () => OnOptionSelected(option);
            
            _optionsContainer.AddChild(button);
        }

        private void OnOptionSelected(Quests.DialogueOption option) {
            // 玩家选择选项时,停止打字机效果并立即显示完整文本
            _skipTypewriter = true;
            _isTyping = false;
            _displayedText = _fullText;
            _dialogueText.Text = _fullText;
            
            Quests.DialogueManager.Instance.SelectOption(option);
        }

        private void OnContinuePressed() {
            // 点击继续时,如果正在打字,跳过打字机效果
            if (_isTyping) {
                _skipTypewriter = true;
                _isTyping = false;
                _displayedText = _fullText;
                _dialogueText.Text = _fullText;
                ShowOptions();
            } else {
                Quests.DialogueManager.Instance.Continue();
            }
        }

        private void ClearOptions() {
            foreach (var child in _optionsContainer.GetChildren()) {
                child.QueueFree();
            }
            _continueButton.Visible = false;
        }

        public override void _Input(InputEvent @event) {
            if (!Visible) return;
            
            // 增强: 按Space键跳过打字机效果
            if (@event.IsActionPressed("ui_accept")) {
                if (_isTyping && !_skipTypewriter) {
                    // 跳过打字机效果
                    _skipTypewriter = true;
                    _isTyping = false;
                    _displayedText = _fullText;
                    _dialogueText.Text = _fullText;
                    ShowOptions();
                    return;
                }
                
                // 继续对话
                if (Quests.DialogueManager.Instance.IsInDialogue) {
                    var currentNode = Quests.DialogueManager.Instance.CurrentNode;
                    if (currentNode != null && currentNode.IsEndNode) {
                        Quests.DialogueManager.Instance.EndDialogue();
                    } else if (currentNode != null && (currentNode.Options.Count == 0 || Quests.DialogueManager.Instance.GetAvailableOptions().Count == 0)) {
                        Quests.DialogueManager.Instance.Continue();
                    }
                }
            }
            
            if (@event.IsActionPressed("ui_cancel")) {
                // 按Escape结束对话
                if (Quests.DialogueManager.Instance.IsInDialogue) {
                    Quests.DialogueManager.Instance.EndDialogue();
                }
            }
        }
    }
}
