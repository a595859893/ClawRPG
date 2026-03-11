using System;
using System.Collections.Generic;
using Godot;

public partial class MomentumUI : Control
{
    private PanelContainer _mainPanel;
    private VBoxContainer _contentBox;
    private Label _titleLabel;
    private GridContainer _momentumGrid;
    private Label _statsLabel;
    private bool _isVisible = false;
    
    // Colors
    private Color _neutralColor = new Color(0.5f, 0.5f, 0.5f);
    private Color _buildingColor = new Color(0.6f, 0.6f, 0.2f);
    private Color _chargedColor = new Color(0.2f, 0.6f, 0.2f);
    private Color _overchargedColor = new Color(0.8f, 0.2f, 0.8f);
    private Color _fadingColor = new Color(0.6f, 0.3f, 0.1f);
    
    public override void _Ready()
    {
        _CreateUI();
        Visible = false;
        
        // Connect to MomentumSystem signals
        if (MomentumSystem.Instance != null)
        {
            MomentumSystem.Instance.MomentumChanged += _OnMomentumChanged;
            MomentumSystem.Instance.MomentumOvercharged += _OnMomentumOvercharged;
        }
    }
    
    private void _CreateUI()
    {
        // Main Panel
        _mainPanel = new PanelContainer();
        _mainPanel.AnchorPreset = ControlPreset.TopRight;
        _mainPanel.OffsetLeft = -320;
        _mainPanel.OffsetTop = 60;
        _mainPanel.OffsetRight = -20;
        _mainPanel.OffsetBottom = -20;
        _mainPanel.CustomMinimumSize = new Vector2(300, 400);
        AddChild(_mainPanel);
        
        // StyleBox
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        styleBox.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        styleBox.SetBorderWidthAll(2);
        styleBox.SetCornerRadiusAll(8);
        _mainPanel.AddThemeStyleboxOverride("panel", styleBox);
        
        // Content Box
        _contentBox = new VBoxContainer();
        _contentBox.AddThemeConstantOverride("separation", 10);
        _mainPanel.AddChild(_contentBox);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚡ Momentum";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 20);
        _contentBox.AddChild(_titleLabel);
        
        // Separator
        var separator = new HSeparator();
        _contentBox.AddChild(separator);
        
        // Momentum Grid
        _momentumGrid = new GridContainer();
        _momentumGrid.Columns = 2;
        _momentumGrid.AddThemeConstantOverride("h_separation", 10);
        _momentumGrid.AddThemeConstantOverride("v_separation", 8);
        _contentBox.AddChild(_momentumGrid);
        
        // Create momentum displays
        _CreateMomentumDisplays();
        
        // Separator
        var separator2 = new HSeparator();
        _contentBox.AddChild(separator2);
        
        // Stats
        _statsLabel = new Label();
        _statsLabel.Text = "Statistics";
        _statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statsLabel.AddThemeFontSizeOverride("font_size", 16);
        _contentBox.AddChild(_statsLabel);
        
        var statsValueLabel = new Label();
        statsValueLabel.Name = "StatsValue";
        _contentBox.AddChild(statsValueLabel);
        
        // Hint
        var hintLabel = new Label();
        hintLabel.Text = "Press M to toggle";
        hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        hintLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        hintLabel.AddThemeFontSizeOverride("font_size", 12);
        _contentBox.AddChild(hintLabel);
    }
    
    private void _CreateMomentumDisplays()
    {
        var types = Enum.GetValues(typeof(MomentumData.MomentumType));
        foreach (MomentumData.MomentumType type in types)
        {
            // Type label
            var typeLabel = new Label();
            typeLabel.Name = type.ToString() + "_Label";
            typeLabel.Text = _GetMomentumName(type);
            typeLabel.HorizontalAlignment = HorizontalAlignment.Left;
            _momentumGrid.AddChild(typeLabel);
            
            // Progress bar container
            var progressContainer = new VBoxContainer();
            progressContainer.Name = type.ToString() + "_Container";
            
            var progressBar = new ProgressBar();
            progressBar.Name = type.ToString() + "_Progress";
            progressBar.CustomMinimumSize = new Vector2(150, 20);
            progressBar.ShowPercentage = false;
            progressContainer.AddChild(progressBar);
            
            var stateLabel = new Label();
            stateLabel.Name = type.ToString() + "_State";
            stateLabel.HorizontalAlignment = HorizontalAlignment.Right;
            stateLabel.AddThemeFontSizeOverride("font_size", 11);
            progressContainer.AddChild(stateLabel);
            
            _momentumGrid.AddChild(progressContainer);
        }
    }
    
    private string _GetMomentumName(MomentumData.MomentumType type)
    {
        switch (type)
        {
            case MomentumData.MomentumType.Attack: return "⚔️ Attack";
            case MomentumData.MomentumType.Defense: return "🛡️ Defense";
            case MomentumData.MomentumType.Speed: return "⚡ Speed";
            case MomentumData.MomentumType.Luck: return "🍀 Luck";
            case MomentumData.MomentumType.Critical: return "💥 Critical";
            default: return type.ToString();
        }
    }
    
    public override void _Process(double delta)
    {
        if (!Visible) return;
        
        _UpdateMomentumDisplays();
    }
    
    private void _UpdateMomentumDisplays()
    {
        if (MomentumSystem.Instance == null) return;
        
        var momenta = MomentumSystem.Instance.GetAllMomenta();
        
        foreach (var kvp in momenta)
        {
            var type = kvp.Key;
            var momentum = kvp.Value;
            
            var progressBar = _momentumGrid.FindChild(type.ToString() + "_Progress", true, false) as ProgressBar;
            var stateLabel = _momentumGrid.FindChild(type.ToString() + "_State", true, false) as Label;
            
            if (progressBar != null)
            {
                progressBar.MaxValue = momentum.MaxCharge;
                progressBar.Value = momentum.Charge;
                
                // Color based on state
                var styleBox = progressBar.GetThemeStylebox("fill") as StyleBoxFlat;
                if (styleBox == null)
                {
                    styleBox = new StyleBoxFlat();
                    progressBar.AddThemeStyleboxOverride("fill", styleBox);
                }
                
                styleBox.BgColor = _GetStateColor(momentum.State);
            }
            
            if (stateLabel != null)
            {
                stateLabel.Text = $"{_GetStateName(momentum.State)} (Lv.{momentum.Level})";
                stateLabel.AddThemeColorOverride("font_color", _GetStateColor(momentum.State));
            }
        }
        
        // Update stats
        var statsLabel = _contentBox.FindChild("StatsValue", true, false) as Label;
        if (statsLabel != null)
        {
            var stats = MomentumSystem.Instance.GetStatistics();
            statsLabel.Text = $"Total: {stats.TotalMomentumGained}\nMax Level: {stats.MaxMomentumReached}\nOvercharge: {stats.OverchargeCount}\nLost: {stats.MomentumLostToDecay}";
        }
    }
    
    private Color _GetStateColor(MomentumData.MomentumState state)
    {
        switch (state)
        {
            case MomentumData.MomentumState.Neutral: return _neutralColor;
            case MomentumData.MomentumState.Building: return _buildingColor;
            case MomentumData.MomentumState.Charged: return _chargedColor;
            case MomentumData.MomentumState.Overcharged: return _overchargedColor;
            case MomentumData.MomentumState.Fading: return _fadingColor;
            default: return _neutralColor;
        }
    }
    
    private string _GetStateName(MomentumData.MomentumState state)
    {
        switch (state)
        {
            case MomentumData.MomentumState.Neutral: return "Neutral";
            case MomentumData.MomentumState.Building: return "Building";
            case MomentumData.MomentumState.Charged: return "Charged";
            case MomentumData.MomentumState.Overcharged: return "OVERCHARGED";
            case MomentumData.MomentumState.Fading: return "Fading";
            default: return "Unknown";
        }
    }
    
    private void _OnMomentumChanged(MomentumData.MomentumType type, MomentumData.MomentumState state, int level)
    {
        // Visual feedback could be added here
    }
    
    private void _OnMomentumOvercharged(MomentumData.MomentumType type)
    {
        // Could add screen effect or sound here
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.M)
            {
                ToggleVisibility();
            }
        }
    }
    
    public void ToggleVisibility()
    {
        Visible = !Visible;
        if (Visible)
        {
            _UpdateMomentumDisplays();
        }
    }
}
