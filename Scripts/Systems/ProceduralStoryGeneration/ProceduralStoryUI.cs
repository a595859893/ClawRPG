using Godot;
using System;
using System.Collections.Generic;

public partial class ProceduralStoryUI : Control
{
    private ProceduralStorySystem _system;
    private ProceduralStoryDatabase _database;
    
    // UI Components
    private TabContainer _tabContainer;
    private VBoxContainer _activeStoriesContainer;
    private VBoxContainer _historyContainer;
    private VBoxContainer _statsContainer;
    private Label _titleLabel;
    
    // Colors
    private Color _heroColor = new Color(0.2f, 0.6f, 1.0f);
    private Color _tragedyColor = new Color(0.8f, 0.2f, 0.2f);
    private Color _romanceColor = new Color(1.0f, 0.4f, 0.6f);
    private Color _adventureColor = new Color(0.2f, 0.8f, 0.4f);
    private Color _mysteryColor = new Color(0.5f, 0.3f, 0.8f);
    private Color _legendColor = new Color(1.0f, 0.84f, 0.0f);
    
    public override void _Ready()
    {
        _system = GetNode<ProceduralStorySystem>("/root/ProceduralStorySystem");
        _database = GetNode<ProceduralStoryDatabase>("/root/ProceduralStoryDatabase");
        
        SetupUI();
    }
    
    private void SetupUI()
    {
        // Main panel
        var panel = new PanelContainer();
        panel.SetAnchorsPreset(Control.LayoutPreset.Center);
        panel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(panel);
        
        var mainVBox = new VBoxContainer();
        panel.AddChild(mainVBox);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "Procedural Story System";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddColorOverride("font_color", _legendColor);
        mainVBox.AddChild(_titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetHExpand(ExpandEnum.Fill);
        _tabContainer.SetVExpand(ExpandEnum.Fill);
        _tabContainer.CustomMinimumSize = new Vector2(750, 500);
        mainVBox.AddChild(_tabContainer);
        
        // Active Stories tab
        var activeTab = new ScrollContainer();
        activeTab.Name = "Active Stories";
        _tabContainer.AddChild(activeTab);
        
        _activeStoriesContainer = new VBoxContainer();
        _activeStoriesContainer.SetHExpand(ExpandEnum.Fill);
        activeTab.AddChild(_activeStoriesContainer);
        
        // Add story button
        var addButton = new Button();
        addButton.Text = "Start New Story";
        addButton.Pressed += _OnStartStoryPressed;
        _activeStoriesContainer.AddChild(addButton);
        
        var separator = new HSeparator();
        _activeStoriesContainer.AddChild(separator);
        
        // History tab
        var historyTab = new ScrollContainer();
        historyTab.Name = "History";
        _tabContainer.AddChild(historyTab);
        
        _historyContainer = new VBoxContainer();
        _historyContainer.SetHExpand(ExpandEnum.Fill);
        historyTab.AddChild(_historyContainer);
        
        // Statistics tab
        var statsTab = new ScrollContainer();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);
        
        _statsContainer = new VBoxContainer();
        _statsContainer.SetHExpand(ExpandEnum.Fill);
        statsTab.AddChild(_statsContainer);
        
        // Refresh button
        var refreshButton = new Button();
        refreshButton.Text = "Refresh";
        refreshButton.Pressed += _OnRefreshPressed;
        mainVBox.AddChild(refreshButton);
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += _OnClosePressed;
        mainVBox.AddChild(closeButton);
        
        // Initial refresh
        RefreshAll();
    }
    
    private void RefreshAll()
    {
        RefreshActiveStories();
        RefreshHistory();
        RefreshStatistics();
    }
    
    private void RefreshActiveStories()
    {
        // Clear existing (except first two items)
        while (_activeStoriesContainer.GetChildCount() > 2)
        {
            var child = _activeStoriesContainer.GetChild(2);
            _activeStoriesContainer.RemoveChild(child);
            child.QueueFree();
        }
        
        var stories = _system.GetActiveStories();
        
        if (stories.Count == 0)
        {
            var noStoriesLabel = new Label();
            noStoriesLabel.Text = "No active stories. Start one!";
            noStoriesLabel.AddColorOverride("font_color", Colors.Gray);
            _activeStoriesContainer.AddChild(noStoriesLabel);
            return;
        }
        
        foreach (var kvp in stories)
        {
            var story = kvp.Value;
            var storyPanel = CreateStoryCard(story);
            _activeStoriesContainer.AddChild(storyPanel);
        }
    }
    
    private Control CreateStoryCard(ProceduralStoryData.ActiveStory story)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(700, 120);
        
        var hbox = new HBoxContainer();
        panel.AddChild(hbox);
        
        // Story info
        var infoVBox = new VBoxContainer();
        infoVBox.SetVExpand(ExpandEnum.Fill);
        hbox.AddChild(infoVBox);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = story.StoryName;
        titleLabel.AddColorOverride("font_color", _legendColor);
        titleLabel.FontSettings = new Label.LabelSettings
        {
            FontSize = 18
        };
        infoVBox.AddChild(titleLabel);
        
        // Progress
        var progressLabel = new Label();
        progressLabel.Text = $"Chapter {story.CurrentChapter}/{story.TotalChapters} | Progress: {story.Progress}%";
        infoVBox.AddChild(progressLabel);
        
        // Tension
        var tensionLabel = new Label();
        tensionLabel.Text = $"Tension: {story.Tension}/100";
        infoVBox.AddChild(tensionLabel);
        
        // State
        var stateLabel = new Label();
        stateLabel.Text = $"State: {story.State}";
        infoVBox.AddChild(stateLabel);
        
        // Buttons
        var buttonVBox = new VBoxContainer();
        buttonVBox.SetVAlign(VerticalAlignment.Center);
        hbox.AddChild(buttonVBox);
        
        // Continue button
        var continueButton = new Button();
        continueButton.Text = "Continue";
        continueButton.Pressed += () => _OnContinueStoryPressed(story.StoryId);
        buttonVBox.AddChild(continueButton);
        
        // Pause button
        var pauseButton = new Button();
        pauseButton.Text = story.State == ProceduralStoryData.StoryState.Paused ? "Resume" : "Pause";
        pauseButton.Pressed += () => _OnPauseStoryPressed(story.StoryId);
        buttonVBox.AddChild(pauseButton);
        
        // Fail button
        var failButton = new Button();
        failButton.Text = "Give Up";
        failButton.Pressed += () => _OnFailStoryPressed(story.StoryId);
        buttonVBox.AddChild(failButton);
        
        return panel;
    }
    
    private void RefreshHistory()
    {
        // Clear existing
        foreach (var child in _historyContainer.GetChildren())
        {
            _historyContainer.RemoveChild(child);
            child.QueueFree();
        }
        
        // This would need to access the data from system
        var noHistoryLabel = new Label();
        noHistoryLabel.Text = "Story history will appear here.";
        noHistoryLabel.AddColorOverride("font_color", Colors.Gray);
        _historyContainer.AddChild(noHistoryLabel);
    }
    
    private void RefreshStatistics()
    {
        // Clear existing
        foreach (var child in _statsContainer.GetChildren())
        {
            _statsContainer.RemoveChild(child);
            child.QueueFree();
        }
        
        var stats = _system.GetStatistics();
        
        foreach (var kvp in stats)
        {
            var statLabel = new Label();
            statLabel.Text = $"{kvp.Key}: {kvp.Value}";
            _statsContainer.AddChild(statLabel);
        }
        
        // Reset button
        var resetButton = new Button();
        resetButton.Text = "Reset Statistics";
        resetButton.Pressed += _OnResetStatsPressed;
        _statsContainer.AddChild(resetButton);
    }
    
    private void _OnStartStoryPressed()
    {
        _system.StartStory();
        RefreshAll();
    }
    
    private void _OnContinueStoryPressed(string storyId)
    {
        // Show choices dialog (simplified)
        var story = _system.GetStory(storyId);
        if (story == null) return;
        
        // Get current chapter choices (simplified)
        var choices = new string[] { "Option A", "Option B", "Option C" };
        
        // For now, just advance with random choice
        if (choices.Length > 0)
        {
            var randomChoice = choices[GD.Randi() % choices.Length];
            _system.MakeChoice(storyId, randomChoice);
        }
        
        RefreshAll();
    }
    
    private void _OnPauseStoryPressed(string storyId)
    {
        var story = _system.GetStory(storyId);
        if (story == null) return;
        
        if (story.State == ProceduralStoryData.StoryState.Paused)
            _system.ResumeStory(storyId);
        else
            _system.PauseStory(storyId);
        
        RefreshAll();
    }
    
    private void _OnFailStoryPressed(string storyId)
    {
        _system.FailStory(storyId);
        RefreshAll();
    }
    
    private void _OnResetStatsPressed()
    {
        _system.ResetStatistics();
        RefreshAll();
    }
    
    private void _OnRefreshPressed()
    {
        RefreshAll();
    }
    
    private void _OnClosePressed()
    {
        QueueFree();
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            QueueFree();
        }
    }
}
