using Godot;
using System;
using System.Collections.Generic;

public partial class RandomNameUI : Control
{
    private RandomNameSystem _system;
    private Label _titleLabel;
    private Label _nameLabel;
    private OptionButton _styleOption;
    private OptionButton _genderOption;
    private Button _generateButton;
    private Button _fantasyButton;
    private Button _generateMultipleButton;
    private Label _statsLabel;
    private VBoxContainer _historyContainer;
    
    // Colors
    private Color _titleColor = new Color(1f, 0.9f, 0.5f);
    private Color _buttonColor = new Color(0.3f, 0.5f, 0.8f);
    private Color _bgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    
    public override void _Ready()
    {
        _system = new RandomNameSystem();
        _system._Ready();
        
        SetupUI();
    }

    private void SetupUI()
    {
        // Background panel
        var panel = new PanelContainer();
        panel.RectMinSize = new Vector2(400, 500);
        panel.AnchorLeft = 0.5f;
        panel.AnchorTop = 0.5f;
        panel.AnchorRight = 0.5f;
        panel.AnchorBottom = 0.5f;
        panel.OffsetLeft = -200;
        panel.OffsetTop = -250;
        panel.OffsetRight = 200;
        panel.OffsetBottom = 250;
        
        var style = new StyleBoxFlat();
        style.BgColor = _bgColor;
        style.BorderWidthLeft = 2;
        style.BorderWidthRight = 2;
        style.BorderWidthTop = 2;
        style.BorderWidthBottom = 2;
        style.BorderColor = new Color(0.5f, 0.4f, 0.3f);
        style.CornerRadiusTopLeft = 10;
        style.CornerRadiusTopRight = 10;
        style.CornerRadiusBottomLeft = 10;
        style.CornerRadiusBottomRight = 10;
        panel.AddStyleboxOverride("panel", style);
        
        AddChild(panel);
        
        var mainVBox = new VBoxContainer();
        mainVBox.AddConstantOverride("separation", 15);
        panel.AddChild(mainVBox);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🎲 Random Name Generator";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddColorOverride("font_color", _titleColor);
        _titleLabel.RectMinSize = new Vector2(0, 40);
        mainVBox.AddChild(_titleLabel);
        
        // Style selection
        var styleLabel = new Label();
        styleLabel.Text = "Name Style:";
        styleLabel.AddColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        mainVBox.AddChild(styleLabel);
        
        _styleOption = new OptionButton();
        _styleOption.AddItem("Random", 0);
        _styleOption.AddItem("Western", 1);
        _styleOption.AddItem("Nordic", 2);
        _styleOption.AddItem("Eastern", 3);
        _styleOption.AddItem("Fantasy", 4);
        _styleOption.AddItem("Ancient", 5);
        _styleOption.Select(0);
        mainVBox.AddChild(_styleOption);
        
        // Gender selection
        var genderLabel = new Label();
        genderLabel.Text = "Gender:";
        genderLabel.AddColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        mainVBox.AddChild(genderLabel);
        
        _genderOption = new OptionButton();
        _genderOption.AddItem("Any", 0);
        _genderOption.AddItem("Male", 1);
        _genderOption.AddItem("Female", 2);
        _genderOption.Select(0);
        mainVBox.AddChild(_genderOption);
        
        // Generated name display
        _nameLabel = new Label();
        _nameLabel.Text = "Press Generate to create a name";
        _nameLabel.Align = Label.AlignEnum.Center;
        _nameLabel.AddColorOverride("font_color", new Color(1f, 1f, 1f));
        _nameLabel.RectMinSize = new Vector2(0, 50);
        mainVBox.AddChild(_nameLabel);
        
        // Generate button
        _generateButton = new Button();
        _generateButton.Text = "Generate Name";
        _generateButton.RectMinSize = new Vector2(0, 40);
        _generateButton.Pressed += _OnGeneratePressed;
        mainVBox.AddChild(_generateButton);
        
        // Fantasy name button
        _fantasyButton = new Button();
        _fantasyButton.Text = "Generate Fantasy Name";
        _fantasyButton.RectMinSize = new Vector2(0, 35);
        _fantasyButton.Pressed += _OnFantasyPressed;
        mainVBox.AddChild(_fantasyButton);
        
        // Generate multiple button
        _generateMultipleButton = new Button();
        _generateMultipleButton.Text = "Generate 5 Names";
        _generateMultipleButton.RectMinSize = new Vector2(0, 35);
        _generateMultipleButton.Pressed += _OnGenerateMultiplePressed;
        mainVBox.AddChild(_generateMultipleButton);
        
        // Stats
        var statsTitle = new Label();
        statsTitle.Text = "Statistics:";
        statsTitle.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        mainVBox.AddChild(statsTitle);
        
        _statsLabel = new Label();
        _statsLabel.Text = "Total Generated: 0";
        _statsLabel.AddColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        _statsLabel.RectMinSize = new Vector2(0, 60);
        mainVBox.AddChild(_statsLabel);
        
        // History
        var historyTitle = new Label();
        historyTitle.Text = "Recent Names:";
        historyTitle.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        mainVBox.AddChild(historyTitle);
        
        _historyContainer = new VBoxContainer();
        _historyContainer.RectMinSize = new Vector2(0, 100);
        mainVBox.AddChild(_historyContainer);
    }

    private void _OnGeneratePressed()
    {
        var style = (RandomNameDatabase.NameStyle)_styleOption.Selected;
        var gender = (RandomNameDatabase.NameGender)_genderOption.Selected;
        
        string name = _system.GenerateName(style, gender);
        _nameLabel.Text = name;
        
        UpdateStats();
        UpdateHistory();
    }

    private void _OnFantasyPressed()
    {
        string name = _system.GenerateFantasyName();
        _nameLabel.Text = name;
        
        UpdateStats();
        UpdateHistory();
    }

    private void _OnGenerateMultiplePressed()
    {
        var style = (RandomNameDatabase.NameStyle)_styleOption.Selected;
        string[] names = _system.GenerateMultipleNames(5, style);
        
        _nameLabel.Text = string.Join("\n", names);
        
        UpdateStats();
        UpdateHistory();
    }

    private void UpdateStats()
    {
        var stats = _system.GetStatistics();
        string statsText = "Total Generated: " + stats["TotalGenerated"] + "\n";
        
        foreach (var kvp in stats)
        {
            if (kvp.Key.StartsWith("Culture_"))
            {
                string culture = kvp.Key.Substring(8);
                statsText += culture + ": " + kvp.Value + "\n";
            }
        }
        
        _statsLabel.Text = statsText;
    }

    private void UpdateHistory()
    {
        // Clear existing
        foreach (Node child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add recent names
        string[] recentNames = _system.GetRecentNames(5);
        foreach (string name in recentNames)
        {
            var label = new Label();
            label.Text = "• " + name;
            label.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            _historyContainer.AddChild(label);
        }
    }

    public void Toggle()
    {
        if (Visible)
            Hide();
        else
            Show();
    }
}
