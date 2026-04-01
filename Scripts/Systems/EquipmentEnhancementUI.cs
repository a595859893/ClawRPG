using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 装备强化UI - 强化系统的界面控制
/// 允许玩家选择强化类型并查看强化结果
/// </summary>
public partial class EquipmentEnhancementUI : Control
{
    private Panel _mainPanel;
    private VBoxContainer _mainContainer;
    private Label _titleLabel;
    private HBoxContainer _statsContainer;
    private Label _totalLabel;
    private Label _successLabel;
    private Label _failLabel;
    private VBoxContainer _enhancementTypesContainer;
    private Button _closeButton;
    private Label _goldLabel;

    // Enhancement type buttons
    private Dictionary<EquipmentEnhancementData.EnhancementType, Button> _typeButtons = new Dictionary<EquipmentEnhancementData.EnhancementType, Button>();

    // Current selection
    private EquipmentEnhancementData.EnhancementType _selectedType = EquipmentEnhancementData.EnhancementType.Attack;
    private int _selectedLevel = 1;

    // Result display
    private Label _resultLabel;
    private Label _detailLabel;

    private bool _isVisible = false; 

    public override void _Ready()
    {
        Visible = false; 
        _CreateUI();
        _ConnectSignals();
    }

    private void _CreateUI()
    {
        // Main panel
        _mainPanel = new Panel();
        _mainPanel.SetSize(new Vector2(800, 600));
        _mainPanel.Position = new Vector2(100, 50);
        AddChild(_mainPanel);

        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Godot.Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelStyle.SetCornerRadiusAll(8);
        panelStyle.SetBorderWidthAll(2);
        panelStyle.BorderColor = new Godot.Color(0.3f, 0.3f, 0.4f);
        _mainPanel.AddThemeStyleboxOverride("panel", panelStyle);

        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetSize(new Vector2(780, 580));
        _mainContainer.Position = new Vector2(10, 10);
        _mainContainer.AddThemeConstantOverride("separation", 10);
        _mainPanel.AddChild(_mainContainer);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚒️ Equipment Enhancement";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(_titleLabel);

        // Gold display
        _goldLabel = new Label();
        _goldLabel.Text = "Gold: 0";
        _goldLabel.AddThemeFontSizeOverride("font_size", 18);
        _goldLabel.AddThemeColorOverride("font_color", new Godot.Color(1f, 0.85f, 0.3f));
        _mainContainer.AddChild(_goldLabel);

        // Stats container
        _statsContainer = new HBoxContainer();
        _statsContainer.AddThemeConstantOverride("separation", 20);
        _mainContainer.AddChild(_statsContainer);

        _totalLabel = new Label();
        _totalLabel.Text = "Total: 0";
        _totalLabel.AddThemeFontSizeOverride("font_size", 14);
        _statsContainer.AddChild(_totalLabel);

        _successLabel = new Label();
        _successLabel.Text = "Success: 0";
        _successLabel.AddThemeFontSizeOverride("font_size", 14);
        _successLabel.AddThemeColorOverride("font_color", new Godot.Color(0.3f, 1f, 0.3f));
        _statsContainer.AddChild(_successLabel);

        _failLabel = new Label();
        _failLabel.Text = "Failed: 0";
        _failLabel.AddThemeFontSizeOverride("font_size", 14);
        _failLabel.AddThemeColorOverride("font_color", new Godot.Color(1f, 0.3f, 0.3f));
        _statsContainer.AddChild(_failLabel);

        // Enhancement types container
        _enhancementTypesContainer = new VBoxContainer();
        _enhancementTypesContainer.AddThemeConstantOverride("separation", 5);
        _mainContainer.AddChild(_enhancementTypesContainer);

        // Create buttons for each enhancement type
        var types = Enum.GetValues(typeof(EquipmentEnhancementData.EnhancementType));
        foreach (EquipmentEnhancementData.EnhancementType type in types)
        {
            var button = new Button();
            button.Text = $"  {EquipmentEnhancementDatabase.Instance.GetEnhancementTypeName(type)}";
            button.CustomMinimumSize = new Vector2(760, 40);
            button.Pressed += () => _OnTypeSelected(type);
            _enhancementTypesContainer.AddChild(button);
            _typeButtons[type] = button;
        }

        // Result labels
        _resultLabel = new Label();
        _resultLabel.Text = "";
        _resultLabel.Align = Label.AlignEnum.Center;
        _resultLabel.AddThemeFontSizeOverride("font_size", 20);
        _mainContainer.AddChild(_resultLabel);

        _detailLabel = new Label();
        _detailLabel.Text = "";
        _detailLabel.Align = Label.AlignEnum.Center;
        _detailLabel.AddThemeFontSizeOverride("font_size", 14);
        _mainContainer.AddChild(_detailLabel);

        // Close button
        _closeButton = new Button();
        _closeButton.Text = "Close";
        _closeButton.CustomMinimumSize = new Vector2(100, 40);
        _closeButton.Pressed += _OnClosePressed;
        _mainContainer.AddChild(_closeButton);

        _UpdateUI();
    }

    private void _ConnectSignals()
    {
        if (EquipmentEnhancementSystem.Instance != null)
        {
            EquipmentEnhancementSystem.Instance.OnEnhancementAttempt += _OnEnhancementAttempt;
            EquipmentEnhancementSystem.Instance.OnEnhancementDataChanged += _UpdateUI;
        }
    }

    private void _OnTypeSelected(EquipmentEnhancementData.EnhancementType type)
    {
        _selectedType = type;
        _selectedLevel = 1;
        _UpdateUI();
    }

    private void _OnEnhancementAttempt(EquipmentEnhancementData.EnhancementResult result, int level, EquipmentEnhancementData.EnhancementType type, int bonus)
    {
        string resultText = "";
        Godot.Color resultColor = new Godot.Color(1f, 1f, 1f);

        switch (result)
        {
            case EquipmentEnhancementData.EnhancementResult.CriticalSuccess:
                resultText = $"⭐ CRITICAL SUCCESS! {type} +{level} ⭐";
                resultColor = new Godot.Color(1f, 0.85f, 0.3f);
                break;
            case EquipmentEnhancementData.EnhancementResult.Success:
                resultText = $"✓ Success! {type} +{level}";
                resultColor = new Godot.Color(0.3f, 1f, 0.3f);
                break;
            case EquipmentEnhancementData.EnhancementResult.CriticalFailure:
                resultText = $"💀 CRITICAL FAILURE!";
                resultColor = new Godot.Color(1f, 0.2f, 0.2f);
                break;
            case EquipmentEnhancementData.EnhancementResult.Failure:
                resultText = $"✗ Failed";
                resultColor = new Godot.Color(1f, 0.6f, 0.3f);
                break;
        }

        _resultLabel.Text = resultText;
        _resultLabel.AddThemeColorOverride("font_color", resultColor);

        // Clear result after delay
        var timer = GetTree().CreateTimer(3.0f);
        timer.Timeout += () =>
        {
            _resultLabel.Text = "";
        };
    }

    private void _OnClosePressed()
    {
        ToggleUI();
    }

    private void _UpdateUI()
    {
        if (Player.Instance == null) return;

        // Update gold
        _goldLabel.Text = $"Gold: {Player.Instance.Gold:N0}";

        // Update stats
        var stats = EquipmentEnhancementSystem.Instance.GetStatistics();
        _totalLabel.Text = $"Total: {stats["TotalEnhancements"]}";
        _successLabel.Text = $"Success: {stats["SuccessfulEnhancements"]}";
        _failLabel.Text = $"Failed: {stats["FailedEnhancements"]}";

        // Update button states
        foreach (var kvp in _typeButtons)
        {
            var type = kvp.Key;
            var button = kvp.Value;

            if (type == _selectedType)
            {
                button.AddThemeColorOverride("font_color", EquipmentEnhancementDatabase.Instance.GetEnhancementTypeColor(type));
            }
            else
            {
                button.AddThemeColorOverride("font_color", new Godot.Color(1f, 1f, 1f));
            }
        }

        // Update detail label
        var successRate = EquipmentEnhancementSystem.Instance.GetSuccessRate(_selectedType, _selectedLevel);
        var criticalRate = EquipmentEnhancementSystem.Instance.GetCriticalRate(_selectedType, _selectedLevel);
        var goldCost = EquipmentEnhancementSystem.Instance.GetGoldCost(_selectedType, _selectedLevel);
        var bonus = EquipmentEnhancementSystem.Instance.GetEnhancementBonus(_selectedType, _selectedLevel);

        string detail = $"{EquipmentEnhancementDatabase.Instance.GetEnhancementTypeName(_selectedType)} +{_selectedLevel}\n";
        detail += $"Success Rate: {successRate}% | Critical Rate: {criticalRate}%\n";
        detail += $"Gold Cost: {goldCost:N0} | Bonus: +{bonus * 100:F1}%";

        _detailLabel.Text = detail;
    }

    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;

        if (_isVisible)
        {
            _UpdateUI();
            if (Player.Instance != null)
            {
                Player.Instance.FreezePlayer = true;
            }
        }
        else
        {
            if (Player.Instance != null)
            {
                Player.Instance.FreezePlayer = false; 
            }
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_enhancement"))
        {
            ToggleUI();
            GetTree().SetInputAsHandled();
        }
    }

    public override void _ExitTree()
    {
        if (EquipmentEnhancementSystem.Instance != null)
        {
            EquipmentEnhancementSystem.Instance.OnEnhancementAttempt -= _OnEnhancementAttempt;
            EquipmentEnhancementSystem.Instance.OnEnhancementDataChanged -= _UpdateUI;
        }
    }
}
