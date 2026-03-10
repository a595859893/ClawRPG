using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// 对话框UI - 显示NPC对话内容
    /// </summary>
    public class DialogueUI : Control {
        // UI组件引用
        private Label _speakerNameLabel;
        private RichTextLabel _dialogueText;
        private VBoxContainer _optionsContainer;
        private TextureRect _portrait;
        private TextureButton _continueButton;
        private PanelContainer _mainPanel;

        // 预设
        private Color _speakerNameColor = new Color(1f, 0.84f, 0f); // 金色
        private Color _optionHoverColor = new Color(0.2f, 0.2f, 0.2f);
        private Color _optionNormalColor = new Color(0.1f, 0.1f, 0.1f, 0.8f);

        // 按钮资源
        private PackedScene _optionButtonScene;

        public override void _Ready() {
            base._Ready();
            
            SetupUI();
            Visible = false;
            
            // 连接信号
            Quests.DialogueManager.Instance.DialogueStarted.Connect(OnDialogueStarted);
            Quests.DialogueManager.Instance.DialogueEnded.Connect(OnDialogueEnded);
            Quests.DialogueManager.Instance.NodeChanged.Connect(OnNodeChanged);
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

            // 创建选项按钮场景
            CreateOptionButtonScene();
        }

        private void CreateOptionButtonScene() {
            // 动态创建选项按钮
        }

        private void OnDialogueStarted() {
            Visible = true;
            UpdateDialogueDisplay();
        }

        private void OnDialogueEnded() {
            Visible = false;
            ClearOptions();
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

            // 更新对话内容
            _dialogueText.Text = currentNode.Text ?? "";

            // 更新头像（如果需要）
            UpdatePortrait(currentNode.SpeakerPortrait);

            // 显示选项或继续按钮
            ClearOptions();

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

        private void UpdatePortrait(string portraitPath) {
            // TODO: 加载头像纹理
            // 目前使用占位颜色
            if (_portrait != null) {
                var placeholderStyle = new StyleBoxFlat();
                placeholderStyle.BgColor = new Color(0.3f, 0.3f, 0.3f);
                placeholderStyle.CornerRadiusTopLeft = 40;
                placeholderStyle.CornerRadiusTopRight = 40;
                placeholderStyle.CornerRadiusBottomLeft = 40;
                placeholderStyle.CornerRadiusBottomRight = 40;
                _portrait.AddThemeStyleboxOverride("normal", placeholderStyle);
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
            Quests.DialogueManager.Instance.SelectOption(option);
        }

        private void OnContinuePressed() {
            Quests.DialogueManager.Instance.Continue();
        }

        private void ClearOptions() {
            foreach (var child in _optionsContainer.GetChildren()) {
                child.QueueFree();
            }
            _continueButton.Visible = false;
        }

        public override void _Input(InputEvent @event) {
            if (!Visible) return;
            
            if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("ui_cancel")) {
                // 按Enter或Escape跳过/继续
                if (Quests.DialogueManager.Instance.IsInDialogue) {
                    var currentNode = Quests.DialogueManager.Instance.CurrentNode;
                    if (currentNode != null && currentNode.IsEndNode) {
                        Quests.DialogueManager.Instance.EndDialogue();
                    } else if (currentNode != null && (currentNode.Options.Count == 0 || Quests.DialogueManager.Instance.GetAvailableOptions().Count == 0)) {
                        Quests.DialogueManager.Instance.Continue();
                    }
                }
            }
        }
    }
}
