using Godot;
using System;
using System.Collections.Generic;

[GlobalClass]
public partial class RandomDungeonEventUI : Control
{
    private RandomDungeonEventSystem _eventSystem;
    private Label _titleLabel;
    private Label _eventLabel;
    private Label _statsLabel;
    private Button _triggerButton;
    private Button _closeButton;
    private VBoxContainer _mainContainer;
    private TabContainer _tabContainer;
    
    // Current event display
    private RichTextLabel _eventDescription;
    private Label _eventCategory;
    private Label _eventResult;
    
    public override void _Ready()
    {
        SetupUI();
    }
    
    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainContainer.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🎲 Random Dungeon Events";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(_titleLabel);
        
        // Tab Container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _mainContainer.AddChild(_tabContainer);
        
        // Tab 1: Event Trigger
        SetupEventTab();
        
        // Tab 2: Statistics
        SetupStatsTab();
        
        // Tab 3: Event History
        SetupHistoryTab();
        
        // Close button
        _closeButton = new Button();
        _closeButton.Text = "Close (ESC)";
        _closeButton.Pressed += OnClosePressed;
        _mainContainer.AddChild(_closeButton);
    }
    
    private void SetupEventTab()
    {
        var eventTab = new VBoxContainer();
        eventTab.Name = "Event";
        _tabContainer.AddChild(eventTab);
        
        // Event description
        var descLabel = new Label();
        descLabel.Text = "Event Description:";
        descLabel.AddThemeFontSizeOverride("font_size", 16);
        eventTab.AddChild(descLabel);
        
        _eventDescription = new RichTextLabel();
        _eventDescription.BbcodeEnabled = true;
        _eventDescription.CustomMinimumSize = new Vector2(0, 100);
        _eventDescription.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _eventDescription.Text = "[color=gray]No event triggered yet. Click the button below to trigger a random event![/color]";
        eventTab.AddChild(_eventDescription);
        
        // Category display
        var categoryLabel = new Label();
        categoryLabel.Text = "Category:";
        categoryLabel.AddThemeFontSizeOverride("font_size", 14);
        eventTab.AddChild(categoryLabel);
        
        _eventCategory = new Label();
        _eventCategory.Text = "[Waiting...]";
        _eventCategory.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        eventTab.AddChild(_eventCategory);
        
        // Result display
        var resultLabel = new Label();
        resultLabel.Text = "Result:";
        resultLabel.AddThemeFontSizeOverride("font_size", 14);
        eventTab.AddChild(resultLabel);
        
        _eventResult = new Label();
        _eventResult.Text = "-";
        _eventResult.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
        eventTab.AddChild(_eventResult);
        
        // Trigger button
        _triggerButton = new Button();
        _triggerButton.Text = "🎲 Trigger Random Event";
        _triggerButton.CustomMinimumSize = new Vector2(200, 50);
        _triggerButton.Pressed += OnTriggerPressed;
        eventTab.AddChild(_triggerButton);
    }
    
    private void SetupStatsTab()
    {
        var statsTab = new VBoxContainer();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);
        
        _statsLabel = new RichTextLabel();
        _statsLabel.BbcodeEnabled = true;
        _statsLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _statsLabel.Text = "[color=gray]No statistics yet.[/color]";
        statsTab.AddChild(_statsLabel);
    }
    
    private void SetupHistoryTab()
    {
        var historyTab = new VBoxContainer();
        historyTab.Name = "History";
        _tabContainer.AddChild(historyTab);
        
        var historyLabel = new RichTextLabel();
        historyLabel.BbcodeEnabled = true;
        historyLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        historyLabel.Text = "[color=gray]Event history will appear here.[/color]";
        historyTab.AddChild(historyLabel);
    }
    
    public void SetEventSystem(RandomDungeonEventSystem system)
    {
        _eventSystem = system;
    }
    
    private void OnTriggerPressed()
    {
        if (_eventSystem == null) return;
        
        var result = _eventSystem.TriggerRandomEvent();
        
        if (result.ContainsKey("description"))
        {
            string desc = result["description"].ToString();
            _eventDescription.Text = $"[color=yellow]{desc}[/color]";
        }
        
        if (result.ContainsKey("category"))
        {
            string category = result["category"].ToString();
            _eventCategory.Text = category;
            
            // Color based on category
            Color categoryColor = category switch
            {
                "Combat" => new Color(1f, 0.3f, 0.3f),
                "Treasure" => new Color(1f, 0.8f, 0.2f),
                "Blessing" => new Color(0.3f, 1f, 0.3f),
                "Curse" => new Color(0.6f, 0.2f, 0.8f),
                "Hazard" => new Color(1f, 0.5f, 0.2f),
                "Trap" => new Color(0.8f, 0.3f, 0.3f),
                "Mystery" => new Color(0.6f, 0.4f, 1f),
                "NPC" => new Color(0.4f, 0.8f, 1f),
                "Exploration" => new Color(0.5f, 1f, 0.5f),
                "Reward" => new Color(1f, 0.9f, 0.3f),
                _ => new Color(0.8f, 0.8f, 0.8f)
            };
            _eventCategory.AddThemeColorOverride("font_color", categoryColor);
        }
        
        if (result.ContainsKey("message"))
        {
            string message = result["message"].ToString();
            _eventResult.Text = message;
        }
        
        UpdateStatistics();
    }
    
    private void UpdateStatistics()
    {
        if (_eventSystem == null) return;
        
        var stats = _eventSystem.GetStatistics();
        
        string statsText = "[b]Event Statistics[/b]\n\n";
        statsText += $"Total Events: [color=yellow]{stats["total_events"]}[/color]\n";
        statsText += $"Positive Events: [color=green]{stats["positive_events"]}[/color]\n";
        statsText += $"Negative Events: [color=red]{stats["negative_events"]}[/color]\n";
        statsText += $"Neutral Events: [color=gray]{stats["neutral_events"]}[/color]\n\n";
        statsText += $"[b]Rewards[/b]\n";
        statsText += $"Gold Gained: [color=yellow]{stats["gold_gained"]}[/color]\n";
        statsText += $"Gold Lost: [color=red]{stats["gold_lost"]}[/color]\n";
        statsText += $"Experience Gained: [color=cyan]{stats["exp_gained"]}[/color]\n";
        statsText += $"Items Gained: [color=green]{stats["items_gained"]}[/color]\n";
        statsText += $"Items Lost: [color=red]{stats["items_lost"]}[/color]\n";
        
        _statsLabel.Text = statsText;
    }
    
    private void OnClosePressed()
    {
        Visible = false;
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                Visible = false;
            }
        }
    }
}
