using Godot;
using System;
using System.Collections.Generic;

public class DreamscapeUI : Control
{
    private static DreamscapeUI _instance;
    public static DreamscapeUI Instance => _instance;
    
    private Button _closeButton;
    private VBoxContainer _mainContainer;
    private HBoxContainer _dreamscapeList;
    private PanelContainer _detailPanel;
    private Label _titleLabel;
    private Label _descriptionLabel;
    private Label _progressLabel;
    private Label _layersLabel;
    private Label _ruleLabel;
    private Label _rewardLabel;
    private Button _enterButton;
    private Button _exitButton;
    private Label _currentLayerLabel;
    private Label _timerLabel;
    private Label _scoreLabel;
    private Label _ruleEffectLabel;
    
    private DreamscapeEntry _selectedDreamscape;
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        _instance = this;
        _CreateUI();
        Visible = false;
    }
    
    private void _CreateUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(Control.AnchorsPreset.Center);
        _mainContainer.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "Dreamscape System";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddFontOverride("font", GD.Load<DynamicFont>("res://Fonts/LargeFont.ttf"));
        _mainContainer.AddChild(_titleLabel);
        
        // Close button row
        var buttonRow = new HBoxContainer();
        buttonRow.Alignment = BoxContainer.AlignMode.End;
        _mainContainer.AddChild(buttonRow);
        
        _exitButton = new Button();
        _exitButton.Text = "Exit Dreamscape";
        _exitButton.Pressed += _OnExitPressed;
        buttonRow.AddChild(_exitButton);
        
        _closeButton = new Button();
        _closeButton.Text = "X";
        _closeButton.Pressed += _OnClosePressed;
        buttonRow.AddChild(_closeButton);
        
        // Current layer info (shown when in dreamscape)
        _currentLayerLabel = new Label();
        _currentLayerLabel.Text = "";
        _currentLayerLabel.Align = Label.AlignEnum.Center;
        _currentLayerLabel.Visible = false;
        _mainContainer.AddChild(_currentLayerLabel);
        
        _timerLabel = new Label();
        _timerLabel.Text = "";
        _timerLabel.Align = Label.AlignEnum.Center;
        _timerLabel.Visible = false;
        _mainContainer.AddChild(_timerLabel);
        
        _scoreLabel = new Label();
        _scoreLabel.Text = "";
        _scoreLabel.Align = Label.AlignEnum.Center;
        _scoreLabel.Visible = false;
        _mainContainer.AddChild(_scoreLabel);
        
        _ruleEffectLabel = new Label();
        _ruleEffectLabel.Text = "";
        _ruleEffectLabel.Align = Label.AlignEnum.Center;
        _ruleEffectLabel.Visible = false;
        _mainContainer.AddChild(_ruleEffectLabel);
        
        // Dreamscape list
        _dreamscapeList = new HBoxContainer();
        _dreamscapeList.Alignment = BoxContainer.AlignMode.Center;
        _dreamscapeList.CustomMinimumSize = new Vector2(0, 200);
        _mainContainer.AddChild(_dreamscapeList);
        
        // Detail panel
        _detailPanel = new PanelContainer();
        _detailPanel.CustomMinimumSize = new Vector2(0, 200);
        _mainContainer.AddChild(_detailPanel);
        
        var detailVBox = new VBoxContainer();
        _detailPanel.AddChild(detailVBox);
        
        _descriptionLabel = new Label();
        _descriptionLabel.Text = "Select a dreamscape";
        _descriptionLabel.Autowrap = true;
        detailVBox.AddChild(_descriptionLabel);
        
        _progressLabel = new Label();
        _progressLabel.Text = "";
        detailVBox.AddChild(_progressLabel);
        
        _layersLabel = new Label();
        _layersLabel.Text = "";
        detailVBox.AddChild(_layersLabel);
        
        _ruleLabel = new Label();
        _ruleLabel.Text = "";
        detailVBox.AddChild(_ruleLabel);
        
        _rewardLabel = new Label();
        _rewardLabel.Text = "";
        detailVBox.AddChild(_rewardLabel);
        
        // Enter button
        _enterButton = new Button();
        _enterButton.Text = "Enter Dreamscape";
        _enterButton.Pressed += _OnEnterPressed;
        _enterButton.Disabled = true;
        _mainContainer.AddChild(_enterButton);
        
        _RefreshDreamscapeList();
    }
    
    private void _RefreshDreamscapeList()
    {
        foreach (Node child in _dreamscapeList.GetChildren())
        {
            child.QueueFree();
        }
        
        var dreamscapes = DreamscapeSystem.Instance.GetUnlockedDreamscapes();
        
        foreach (var ds in dreamscapes)
        {
            var card = _CreateDreamscapeCard(ds);
            _dreamscapeList.AddChild(card);
        }
    }
    
    private Control _CreateDreamscapeCard(DreamscapeEntry ds)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(150, 180);
        
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        
        var nameLabel = new Label();
        nameLabel.Text = ds.Name;
        nameLabel.Align = Label.AlignEnum.Center;
        nameLabel.AddFontOverride("font", GD.Load<DynamicFont>("res://Fonts/BoldFont.ttf"));
        vbox.AddChild(nameLabel);
        
        var typeLabel = new Label();
        typeLabel.Text = ds.Type.ToString();
        typeLabel.Align = Label.AlignEnum.Center;
        typeLabel.AddColorOverride("font_color", _GetTypeColor(ds.Type));
        vbox.AddChild(typeLabel);
        
        var layersInfo = new Label();
        layersInfo.Text = $"{ds.TotalLayers} Layers";
        layersInfo.Align = Label.AlignEnum.Center;
        vbox.AddChild(layersInfo);
        
        var stateLabel = new Label();
        stateLabel.Text = ds.State.ToString();
        stateLabel.Align = Label.AlignEnum.Center;
        stateLabel.AddColorOverride("font_color", _GetStateColor(ds.State));
        vbox.AddChild(stateLabel);
        
        var selectButton = new Button();
        selectButton.Text = "Select";
        selectButton.Pressed += () => _OnDreamscapeSelected(ds.Id);
        vbox.AddChild(selectButton);
        
        return panel;
    }
    
    private Color _GetTypeColor(DreamscapeType type)
    {
        switch (type)
        {
            case DreamscapeType.Nightmare: return new Color(0.5f, 0, 0.5f);  // Purple
            case DreamscapeType.Ethereal: return new Color(0, 0.8f, 0.8f);    // Cyan
            case DreamscapeType.Void: return new Color(0.2f, 0, 0.2f);        // Dark
            case DreamscapeType.Temporal: return new Color(0.8f, 0.8f, 0);    // Gold
            case DreamscapeType.Lucid: return new Color(0, 1f, 0.5f);        // Green
            default: return Color.White;
        }
    }
    
    private Color _GetStateColor(DreamscapeState state)
    {
        switch (state)
        {
            case DreamscapeState.Locked: return Color.Gray;
            case DreamscapeState.Available: return Color.Green;
            case DreamscapeState.InProgress: return Color.Yellow;
            case DreamscapeState.Completed: return Color.Blue;
            case DreamscapeState.Mastered: return new Color(1f, 0.5f, 0);      // Orange
            default: return Color.White;
        }
    }
    
    private void _OnDreamscapeSelected(string dreamscapeId)
    {
        _selectedDreamscape = DreamscapeDatabase.Instance.GetDreamscape(dreamscapeId);
        if (_selectedDreamscape == null) return;
        
        _titleLabel.Text = _selectedDreamscape.Name;
        _descriptionLabel.Text = _selectedDreamscape.Description;
        
        var progress = DreamscapeSystem.Instance.GetProgress(dreamscapeId);
        if (progress != null)
        {
            _progressLabel.Text = $"Progress: Layer {progress.CurrentLayer}/{_selectedDreamscape.TotalLayers}\n" +
                                 $"Best Score: {progress.BestScore}\n" +
                                 $"Completed: {progress.CompletionCount}x\n" +
                                 $"Mastered: {progress.MasteryCount}x";
        }
        else
        {
            _progressLabel.Text = "Not started";
        }
        
        _layersLabel.Text = $"Total Layers: {_selectedDreamscape.TotalLayers}";
        _ruleLabel.Text = $"Special Rule: {_selectedDreamscape.DefaultRule}";
        _rewardLabel.Text = $"Score Multiplier: {_selectedDreamscape.ScoreMultiplier}x\n" +
                           $"Drop Multiplier: {_selectedDreamscape.DropMultiplier}x";
        
        _enterButton.Disabled = _selectedDreamscape.State == DreamscapeState.Locked;
    }
    
    private void _OnEnterPressed()
    {
        if (_selectedDreamscape == null) return;
        
        if (DreamscapeSystem.Instance.EnterDreamscape(_selectedDreamscape.Id))
        {
            _UpdateInDreamscapeUI();
        }
    }
    
    private void _OnExitPressed()
    {
        DreamscapeSystem.Instance.ExitDreamscape();
        _UpdateMainUI();
    }
    
    private void _OnClosePressed()
    {
        ToggleUI();
    }
    
    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            _RefreshDreamscapeList();
            
            if (DreamscapeSystem.Instance.IsInDreamscape)
            {
                _UpdateInDreamscapeUI();
            }
            else
            {
                _UpdateMainUI();
            }
        }
    }
    
    private void _UpdateInDreamscapeUI()
    {
        _dreamscapeList.Visible = false;
        _detailPanel.Visible = false;
        _enterButton.Visible = false;
        
        _currentLayerLabel.Visible = true;
        _timerLabel.Visible = true;
        _scoreLabel.Visible = true;
        _ruleEffectLabel.Visible = true;
        _exitButton.Visible = true;
        
        _UpdateDreamscapeInfo();
    }
    
    private void _UpdateMainUI()
    {
        _dreamscapeList.Visible = true;
        _detailPanel.Visible = true;
        _enterButton.Visible = true;
        
        _currentLayerLabel.Visible = false;
        _timerLabel.Visible = false;
        _scoreLabel.Visible = false;
        _ruleEffectLabel.Visible = false;
        
        _titleLabel.Text = "Dreamscape System";
        _RefreshDreamscapeList();
    }
    
    public void _UpdateDreamscapeInfo()
    {
        var dreamscape = DreamscapeSystem.Instance.GetCurrentDreamscape();
        var layer = DreamscapeSystem.Instance.GetCurrentLayer();
        var progress = dreamscape != null ? DreamscapeSystem.Instance.GetProgress(dreamscape.Id) : null;
        
        if (dreamscape == null || layer == null || progress == null) return;
        
        _titleLabel.Text = dreamscape.Name;
        
        _currentLayerLabel.Text = $"Layer {progress.CurrentLayer}/{dreamscape.TotalLayers}" +
                                  (layer.IsBossLayer ? " - BOSS" : "");
        
        _timerLabel.Text = $"Time: {_elapsedTime} / {layer.TimeLimit}s";
        
        _scoreLabel.Text = $"Score: {progress.TotalScore} (Layer: {progress.CurrentLayerScore})";
        
        _ruleEffectLabel.Text = $"Active Rule: {DreamscapeSystem.Instance.GetActiveRule()}\n" +
                               _GetRuleDescription(DreamscapeSystem.Instance.GetActiveRule());
    }
    
    private int _elapsedTime = 0;
    
    public void UpdateTimer(int elapsed, int limit)
    {
        _elapsedTime = elapsed;
        _timerLabel.Text = $"Time: {elapsed} / {limit}s";
    }
    
    private string _GetRuleDescription(DreamscapeRule rule)
    {
        switch (rule)
        {
            case DreamscapeRule.None: return "No special effects";
            case DreamscapeRule.FloatGravity: return "Enemies float in zero gravity";
            case DreamscapeRule.TimeSlowdown: return "Time moves 50% slower";
            case DreamscapeRule.NoCooldown: return "Skills have no cooldown";
            case DreamscapeRule.DoubleDamage: return "All damage doubled";
            case DreamscapeRule.InfiniteMana: return "Unlimited mana";
            case DreamscapeRule.OneHitKill: return "One hit kills everything";
            case DreamscapeRule.RandomElements: return "Random elemental effects";
            case DreamscapeRule.GravityReversal: return "Gravity is reversed";
            case DreamscapeRule.NoDeathPenalty: return "No penalty for dying";
            default: return "";
        }
    }
    
    public override void _Process(float delta)
    {
        if (DreamscapeSystem.Instance.IsInDreamscape && Visible)
        {
            _UpdateDreamscapeInfo();
        }
    }
}
