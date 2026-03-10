using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems.Dialogue;

public partial class DialogueUI : Control {
    [Export] private Color _speakerColor = new Color(1f, 0.9f, 0.6f);
    [Export] private Color _playerChoiceColor = new Color(0.6f, 0.8f, 1f);
    [Export] private Color _disabledChoiceColor = new Color(0.4f, 0.4f, 0.4f);
    
    // UI Elements
    private PanelContainer _mainPanel;
    private VBoxContainer _contentBox;
    private Label _speakerLabel;
    private RichTextLabel _dialogueText;
    private VBoxContainer _choicesContainer;
    private TextureRect _portrait;
    private Label _continueHint;
    
    // State
    private bool _isTyping;
    private float _textRevealTimer;
    private string _fullText;
    private int _revealIndex;
    private List<Button> _choiceButtons = new();
    
    public override void _Ready() {
        SetupUI();
        Visible = false;
        
        // Connect to dialogue system
        if (DialogueSystem.Instance != null) {
            DialogueSystem.Instance.DialogueStarted += OnDialogueStarted;
            DialogueSystem.Instance.DialogueEnded += OnDialogueEnded;
            DialogueSystem.Instance.NodeChanged += OnNodeChanged;
        }
    }
    
    private void SetupUI() {
        // Main panel
        _mainPanel = new PanelContainer {
            AnchorBottom = 0.35f,
            AnchorRight = 1f,
            OffsetTop = -20
        };
        _mainPanel.AddThemeStyleboxOverride("panel", CreatePanelStyle());
        AddChild(_mainPanel);
        
        // Content box
        _contentBox = new VBoxContainer {
            CustomMinimumSize = new Vector2(0, 150)
        };
        _mainPanel.AddChild(_contentBox);
        
        // Portrait
        _portrait = new TextureRect {
            CustomMinimumSize = new Vector2(80, 80),
            ExpandMode = TextureRect.ExpandModeEnumIgnoreSize,
            StretchMode = TextureRect.StretchModeEnumKeepAspectCentered
        };
        
        // Speaker name
        _speakerLabel = new Label {
            CustomMinimumSize = new Vector2(0, 30)
        };
        _speakerLabel.AddThemeColorOverride("font_color", _speakerColor);
        _contentBox.AddChild(_speakerLabel);
        
        // Dialogue text
        _dialogueText = new RichTextLabel {
            FitContent = true,
            CustomMinimumSize = new Vector2(0, 80),
            BbcodeEnabled = true
        };
        _dialogueText.AddThemeColorOverride("default_color", Colors.White);
        _contentBox.AddChild(_dialogueText);
        
        // Continue hint
        _continueHint = new Label {
            Text = "Press SPACE to continue...",
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(0, 20)
        };
        _continueHint.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        _continueHint.Modulate = new Color(1, 1, 1, 0);
        _contentBox.AddChild(_continueHint);
        
        // Choices container
        _choicesContainer = new VBoxContainer {
            CustomMinimumSize = new Vector2(0, 100)
        };
        _choicesContainer.Modulate = new Color(1, 1, 1, 0);
        _contentBox.AddChild(_choicesContainer);
    }
    
    private StyleBoxFlat CreatePanelStyle() {
        var style = new StyleBoxFlat {
            BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f),
            BorderWidthBottom = 4,
            BorderColor = new Color(0.3f, 0.3f, 0.4f),
            CornerRadiusBottomLeft = 8,
            CornerRadiusBottomRight = 8,
            ShadowSize = 8,
            ShadowColor = new Color(0, 0, 0, 0.5f)
        };
        return style;
    }
    
    public override void _Process(double delta) {
        if (_isTyping) {
            _textRevealTimer += (float)delta;
            if (_textRevealTimer >= (_currentNode?.TextSpeed ?? 0.05f)) {
                _textRevealTimer = 0;
                RevealNextCharacter();
            }
        }
    }
    
    public override void _Input(InputEvent e) {
        if (!Visible) return;
        
        if (e.IsActionPressed("ui_accept") || e.IsActionPressed("ui_text_completion_replace")) {
            if (_isTyping) {
                // Complete text immediately
                _isTyping = false;
                _dialogueText.Text = _fullText;
            } else if (!(_currentNode?.HasChoices ?? false)) {
                // Advance dialogue
                DialogueSystem.Instance.AdvanceDialogue();
            }
        }
    }
    
    private DialogueNode _currentNode;
    
    private void OnDialogueStarted(string npcId) {
        Visible = true;
        Modulate = new Color(1, 1, 1, 0);
        
        // Fade in
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1f, 0.3f);
    }
    
    private void OnDialogueEnded() {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.3f);
        tween.TweenCallback(() => Visible = false);
    }
    
    private void OnNodeChanged(DialogueNode node) {
        _currentNode = node;
        
        // Update speaker
        _speakerLabel.Text = node.Speaker;
        
        // Update portrait (placeholder - would load from resources)
        // Update text with typewriter effect
        _fullText = node.Text;
        _revealIndex = 0;
        _dialogueText.Text = "";
        _isTyping = true;
        
        // Update continue hint
        _continueHint.Modulate = new Color(1, 1, 1, 0);
        
        // Update choices
        UpdateChoices();
    }
    
    private void UpdateChoices() {
        // Clear existing choices
        foreach (var btn in _choiceButtons) {
            btn.QueueFree();
        }
        _choiceButtons.Clear();
        
        if (_currentNode?.HasChoices != true) {
            // Show continue hint for linear dialogue
            var tween = CreateTween();
            tween.TweenProperty(_continueHint, "modulate:a", 1f, 0.3f);
            _choicesContainer.Modulate = new Color(1, 1, 1, 0);
            return;
        }
        
        var choices = DialogueSystem.Instance.GetAvailableChoices();
        
        // Hide continue hint, show choices
        _continueHint.Modulate = new Color(1, 1, 1, 0);
        var tween2 = CreateTween();
        tween2.TweenProperty(_choicesContainer, "modulate:a", 1f, 0.3f);
        
        for (int i = 0; i < choices.Count; i++) {
            var choice = choices[i];
            var btn = CreateChoiceButton(choice, i);
            _choicesContainer.AddChild(btn);
            _choiceButtons.Add(btn);
        }
    }
    
    private Button CreateChoiceButton(DialogueChoice choice, int index) {
        var btn = new Button {
            Text = choice.Text,
            CustomMinimumSize = new Vector2(400, 40),
            Alignment = HorizontalAlignment.Left
        };
        
        // Apply styling
        var normalStyle = new StyleBoxFlat {
            BgColor = new Color(0.15f, 0.2f, 0.3f, 0.8f),
            BorderWidthLeft = 3,
            BorderColor = _playerChoiceColor,
            CornerRadiusTopLeft = 4,
            CornerRadiusTopRight = 4,
            CornerRadiusBottomLeft = 4,
            CornerRadiusBottomRight = 4
        };
        btn.AddThemeStyleboxOverride("normal", normalStyle);
        
        var hoverStyle = normalStyle.Duplicate() as StyleBoxFlat;
        hoverStyle.BgColor = new Color(0.2f, 0.3f, 0.4f, 0.9f);
        btn.AddThemeStyleboxOverride("hover", hoverStyle);
        
        var pressedStyle = normalStyle.Duplicate() as StyleBoxFlat;
        pressedStyle.BgColor = new Color(0.1f, 0.15f, 0.25f, 0.95f);
        btn.AddThemeStyleboxOverride("pressed", pressedStyle);
        
        // Check if can select
        bool canSelect = DialogueSystem.Instance.CanSelectChoice(choice);
        if (!canSelect) {
            btn.Modulate = _disabledChoiceColor;
            btn.Disabled = true;
            
            // Add requirement text
            if (!string.IsNullOrEmpty(choice.RequiredItem)) {
                btn.Text += " [Need: " + choice.RequiredItem + "]";
            }
            if (choice.RequiredGold > 0) {
                btn.Text += " [Need: " + choice.RequiredGold + " Gold]";
            }
        } else {
            // Connect signal
            int idx = index; // Capture for closure
            btn.Pressed += () => OnChoiceSelected(idx);
        }
        
        return btn;
    }
    
    private void OnChoiceSelected(int index) {
        DialogueSystem.Instance.MakeChoiceByIndex(index);
    }
    
    private void RevealNextCharacter() {
        if (_revealIndex < _fullText.Length) {
            _dialogueText.Text = _fullText.Substring(0, _revealIndex + 1);
            _revealIndex++;
        } else {
            _isTyping = false;
        }
    }
}
