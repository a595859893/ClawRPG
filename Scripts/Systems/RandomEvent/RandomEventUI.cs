using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.RandomEvent;
using EventRarity = ClawRPG.Scripts.Systems.RandomEvent.EventRarity;

/// <summary>
/// UI for displaying and interacting with random events
/// </summary>
public partial class RandomEventUI : Control
{
    private Label _titleLabel;
    private Label _descriptionLabel;
    private Label _eventTypeLabel;
    private Label _rarityLabel;
    private Label _rewardsLabel;
    private Button _dismissButton;
    private Button _triggerButton;
    private VBoxContainer _eventListContainer;
    private VBoxContainer _statsContainer;
    private TabContainer _tabContainer;
    
    // Event display
    private Panel _currentEventPanel;
    private Label _currentEventName;
    private Label _currentEventDesc;
    private Label _currentEventTimer;
    
    // Stats display
    private Label _eventsEncounteredLabel;
    private Label _positiveEventsLabel;
    private Label _negativeEventsLabel;
    private Label _totalGoldGainedLabel;
    private Label _totalGoldLostLabel;
    private Label _totalExperienceLabel;
    
    private bool _isVisible = false;
    private RandomEventSystem _eventSystem;
    
    public override void _Ready()
    {
        _eventSystem = GetNode<RandomEventSystem>("/root/RandomEventSystem");
        
        SetupUI();
        ConnectSignals();
        Hide();
    }
    
    private void SetupUI()
    {
        // Main container
        var mainContainer = new HBoxContainer
        {
            AnchorRight = Vector2.One,
            AnchorBottom = Vector2.One,
            OffsetLeft = 50,
            OffsetTop = 50,
            OffsetRight = -50,
            OffsetBottom = -50
        };
        AddChild(mainContainer);
        
        // Left panel - Current Event & Actions
        var leftPanel = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill,
            CustomMinimumSize = new Vector2(300, 0)
        };
        mainContainer.AddChild(leftPanel);
        
        // Title
        _titleLabel = new Label
        {
            Text = "Random Events",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 40)
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        leftPanel.AddChild(_titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer
        {
            SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill
        };
        leftPanel.AddChild(_tabContainer);
        
        // Current Event Tab
        var currentEventTab = new VBoxContainer();
        _tabContainer.AddChild(currentEventTab);
        _tabContainer.SetTabTitle(0, "Current Event");
        
        _currentEventName = new Label
        {
            Text = "No active event",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 30)
        };
        _currentEventName.AddThemeFontSizeOverride("font_size", 18);
        currentEventTab.AddChild(_currentEventName);
        
        _currentEventDesc = new Label
        {
            Text = "Waiting for random events...",
            HorizontalAlignment = HorizontalAlignment.Center,
            AutowrapMode = TextServer.WordWrap
        };
        currentEventTab.AddChild(_currentEventDesc);
        
        _currentEventTimer = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 20)
        };
        currentEventTab.AddChild(_currentEventTimer);
        
        var buttonContainer = new HBoxContainer
        {
            CustomMinimumSize = new Vector2(0, 40)
        };
        currentEventTab.AddChild(buttonContainer);
        
        _triggerButton = new Button
        {
            Text = "Trigger Event",
            SizeFlagsHorizontal = SizeFlags.Expand
        };
        _triggerButton.Pressed += OnTriggerPressed;
        buttonContainer.AddChild(_triggerButton);
        
        _dismissButton = new Button
        {
            Text = "Dismiss",
            SizeFlagsHorizontal = SizeFlags.Expand,
            Disabled = true
        };
        _dismissButton.Pressed += OnDismissPressed;
        buttonContainer.AddChild(_dismissButton);
        
        // Stats Tab
        var statsTab = new VBoxContainer();
        _tabContainer.AddChild(statsTab);
        _tabContainer.SetTabTitle(1, "Statistics");
        
        SetupStatsPanel(statsTab);
        
        // Right panel - Event List
        var rightPanel = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill
        };
        mainContainer.AddChild(rightPanel);
        
        var listTitle = new Label
        {
            Text = "All Events",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 30)
        };
        listTitle.AddThemeFontSizeOverride("font_size", 18);
        rightPanel.AddChild(listTitle);
        
        var scrollContainer = new ScrollContainer
        {
            SizeFlagsVertical = SizeFlags.Expand | SizeFlags.Fill
        };
        rightPanel.AddChild(scrollContainer);
        
        _eventListContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.Expand
        };
        scrollContainer.AddChild(_eventListContainer);
        
        RefreshEventList();
    }
    
    private void SetupStatsPanel(VBoxContainer parent)
    {
        _eventsEncounteredLabel = new Label { Text = "Events Encountered: 0" };
        parent.AddChild(_eventsEncounteredLabel);
        
        _positiveEventsLabel = new Label { Text = "Positive Events: 0" };
        parent.AddChild(_positiveEventsLabel);
        
        _negativeEventsLabel = new Label { Text = "Negative Events: 0" };
        parent.AddChild(_negativeEventsLabel);
        
        var spacer = new Control { CustomMinimumSize = new Vector2(0, 20) };
        parent.AddChild(spacer);
        
        _totalGoldGainedLabel = new Label { Text = "Total Gold Gained: 0" };
        parent.AddChild(_totalGoldGainedLabel);
        
        _totalGoldLostLabel = new Label { Text = "Total Gold Lost: 0" };
        parent.AddChild(_totalGoldLostLabel);
        
        _totalExperienceLabel = new Label { Text = "Total Experience: 0" };
        parent.AddChild(_totalExperienceLabel);
    }
    
    private void ConnectSignals()
    {
        if (_eventSystem != null)
        {
            _eventSystem.EventTriggered += OnEventTriggered;
            _eventSystem.EventEnded += OnEventEnded;
        }
    }
    
    private void OnEventTriggered(RandomEventData evt)
    {
        _currentEventName.Text = evt.eventName;
        _currentEventDesc.Text = evt.description;
        
        var rarityText = evt.rarity.ToString();
        var typeText = evt.eventType.ToString();
        
        if (evt.isPositive)
            _currentEventDesc.Text += $"\n\n[Positive] {typeText}";
        else if (evt.isNegative)
            _currentEventDesc.Text += $"\n\n[Negative] {typeText}";
        else
            _currentEventDesc.Text += $"\n\n[Neutral] {typeText}";
        
        _currentEventDesc.Text += $"\nRarity: {rarityText}";
        
        _dismissButton.Disabled = false;
        RefreshStats();
    }
    
    private void OnEventEnded(RandomEventData evt)
    {
        _currentEventName.Text = "No active event";
        _currentEventDesc.Text = "Waiting for random events...";
        _currentEventTimer.Text = "";
        _dismissButton.Disabled = true;
    }
    
    private void OnTriggerPressed()
    {
        _eventSystem?.TriggerRandomEvent();
    }
    
    private void OnDismissPressed()
    {
        _eventSystem?.DismissCurrentEvent();
    }
    
    private void RefreshEventList()
    {
        // Clear existing
        foreach (var child in _eventListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (_eventSystem == null) return;
        
        var events = _eventSystem.GetAllEvents();
        foreach (var kvp in events)
        {
            var evt = kvp.Value;
            var eventItem = new HBoxContainer();
            
            var nameLabel = new Label
            {
                Text = evt.eventName,
                SizeFlagsHorizontal = SizeFlags.Expand
            };
            eventItem.AddChild(nameLabel);
            
            var rarityLabel = new Label
            {
                Text = evt.rarity.ToString(),
                CustomMinimumSize = new Vector2(80, 0)
            };
            
            // Color code by rarity
            Color rarityColor;
            switch (evt.rarity)
            {
                case EventRarity.Common:
                    rarityColor = Colors.White;
                    break;
                case EventRarity.Uncommon:
                    rarityColor = Colors.Green;
                    break;
                case EventRarity.Rare:
                    rarityColor = Colors.Blue;
                    break;
                case EventRarity.Legendary:
                    rarityColor = Colors.Gold;
                    break;
                default:
                    rarityColor = Colors.White;
                    break;
            }
            rarityLabel.Modulate = rarityColor;
            eventItem.AddChild(rarityLabel);
            
            var typeLabel = new Label
            {
                Text = evt.isPositive ? "+" : (evt.isNegative ? "-" : "o"),
                CustomMinimumSize = new Vector2(30, 0)
            };
            typeLabel.Modulate = evt.isPositive ? Colors.Green : (evt.isNegative ? Colors.Red : Colors.Gray);
            eventItem.AddChild(typeLabel);
            
            _eventListContainer.AddChild(eventItem);
        }
    }
    
    private void RefreshStats()
    {
        if (_eventSystem == null) return;
        
        var stats = _eventSystem.GetStats();
        
        _eventsEncounteredLabel.Text = $"Events Encountered: {stats.eventsEncountered}";
        _positiveEventsLabel.Text = $"Positive Events: {stats.positiveEvents}";
        _negativeEventsLabel.Text = $"Negative Events: {stats.negativeEvents}";
        _totalGoldGainedLabel.Text = $"Total Gold Gained: {stats.totalGoldGained}";
        _totalGoldLostLabel.Text = $"Total Gold Lost: {stats.totalGoldLost}";
        _totalExperienceLabel.Text = $"Total Experience: {stats.totalExperienceGained}";
    }
    
    public override void _Process(double delta)
    {
        // Update current event timer display
        if (_eventSystem?.GetCurrentEvent() != null)
        {
            var currentEvent = _eventSystem.GetCurrentEvent();
            if (currentEvent.effectDuration > 0)
            {
                var elapsed = (DateTime.Now - DateTime.Now).TotalSeconds; // Placeholder
                var remaining = Math.Max(0, currentEvent.effectDuration - elapsed);
                _currentEventTimer.Text = $"Duration: {remaining:F0}s";
            }
        }
        
        // Periodic stats refresh
        if (Engine.GetProcessFrames() % 60 == 0)
        {
            RefreshStats();
        }
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // E key to toggle UI
            if (keyEvent.Keycode == Key.E)
            {
                ToggleVisibility();
            }
        }
    }
    
    public void ToggleVisibility()
    {
        if (_isVisible)
        {
            Hide();
            _isVisible = false;
        }
        else
        {
            Show();
            _isVisible = true;
            RefreshEventList();
            RefreshStats();
        }
    }
}
