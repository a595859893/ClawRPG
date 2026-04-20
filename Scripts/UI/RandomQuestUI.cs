using Godot;
using System;
using System.Collections.Generic;

public partial class RandomQuestUI : Control
{
    private VBoxContainer _mainContainer;
    private TabContainer _tabContainer;
    private Label _titleLabel;
    
    // Active quests tab
    private VBoxContainer _activeTab;
    private VBoxContainer _activeQuestsContainer;
    private Button _generateButton;
    private Button _refreshButton;
    
    // Completed quests tab
    private VBoxContainer _completedTab;
    private VBoxContainer _completedQuestsContainer;
    
    // Statistics tab
    private VBoxContainer _statsTab;
    private Label _totalGeneratedLabel;
    private Label _totalCompletedLabel;
    private Label _completionRateLabel;
    private Label _totalRewardsLabel;
    
    private bool _isVisible = false;
    private RandomQuestSystem _questSystem;
    private int _playerLevel = 25; // Sample level
    
    public override void _Ready()
    {
        _questSystem = RandomQuestSystem.Instance;
        
        SetupUI();
        RefreshData();
    }
    
    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(Control.AnchorsPreset.Center);
        _mainContainer.CustomMinimumSize = new Vector2(700, 550);
        _mainContainer.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(_mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "  📜 Random Quest Generator  📜";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.Modulate = Colors.Gold;
        _titleLabel.AddThemeFontSizeOverride("font_size", 26);
        _mainContainer.AddChild(_titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _mainContainer.AddChild(_tabContainer);
        
        // Active quests tab
        _activeTab = new VBoxContainer();
        _activeTab.Name = "Active";
        _tabContainer.AddChild(_activeTab);
        SetupActiveTab();
        
        // Completed quests tab
        _completedTab = new VBoxContainer();
        _completedTab.Name = "Completed";
        _tabContainer.AddChild(_completedTab);
        SetupCompletedTab();
        
        // Statistics tab
        _statsTab = new VBoxContainer();
        _statsTab.Name = "Statistics";
        _tabContainer.AddChild(_statsTab);
        SetupStatsTab();
        
        // Control buttons
        HBoxContainer buttons = new HBoxContainer();
        _mainContainer.AddChild(buttons);
        
        _generateButton = new Button();
        _generateButton.Text = "  Generate New Quests  ";
        _generateButton.Pressed += OnGeneratePressed;
        buttons.AddChild(_generateButton);
        
        _refreshButton = new Button();
        _refreshButton.Text = "  Refresh  ";
        _refreshButton.Pressed += OnRefreshPressed;
        buttons.AddChild(_refreshButton);
        
        // Close button
        Button closeBtn = new Button();
        closeBtn.Text = "  Close (ESC)  ";
        closeBtn.Pressed += () => ToggleVisibility(false);
        buttons.AddChild(closeBtn);
        
        // Initially hidden
        Visible = false;
    }
    
    private void SetupActiveTab()
    {
        _activeQuestsContainer = new VBoxContainer();
        _activeQuestsContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        
        ScrollContainer scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _activeTab.AddChild(scroll);
        
        scroll.AddChild(_activeQuestsContainer);
    }
    
    private void SetupCompletedTab()
    {
        _completedQuestsContainer = new VBoxContainer();
        _completedQuestsContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        
        ScrollContainer scroll = new ScrollContainer();
        scroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _completedTab.AddChild(scroll);
        
        scroll.AddChild(_completedQuestsContainer);
    }
    
    private void SetupStatsTab()
    {
        _totalGeneratedLabel = new Label();
        _totalGeneratedLabel.Text = "Total Quests Generated: 0";
        _totalGeneratedLabel.AddThemeFontSizeOverride("font_size", 20);
        _statsTab.AddChild(_totalGeneratedLabel);
        
        _totalCompletedLabel = new Label();
        _totalCompletedLabel.Text = "Total Quests Completed: 0";
        _totalCompletedLabel.AddThemeFontSizeOverride("font_size", 20);
        _statsTab.AddChild(_totalCompletedLabel);
        
        _completionRateLabel = new Label();
        _completionRateLabel.Text = "Completion Rate: 0%";
        _completionRateLabel.AddThemeFontSizeOverride("font_size", 20);
        _statsTab.AddChild(_completionRateLabel);
        
        _totalRewardsLabel = new Label();
        _totalRewardsLabel.Text = "Total Gold Earned: 0";
        _totalRewardsLabel.AddThemeFontSizeOverride("font_size", 20);
        _totalRewardsLabel.Modulate = Colors.Gold;
        _statsTab.AddChild(_totalRewardsLabel);
    }
    
    public void RefreshData()
    {
        RefreshActiveQuests();
        RefreshCompletedQuests();
        RefreshStatistics();
    }
    
    private void RefreshActiveQuests()
    {
        foreach (Node child in _activeQuestsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var quests = _questSystem.ActiveQuests;
        if (quests == null || quests.Count == 0)
        {
            Label noQuests = new Label();
            noQuests.Text = "No active quests.\nClick 'Generate New Quests' to get some!";
            noQuests.Modulate = Colors.Gray;
            noQuests.HorizontalAlignment = HorizontalAlignment.Center;
            _activeQuestsContainer.AddChild(noQuests);
            return;
        }
        
        foreach (var quest in quests)
        {
            CreateQuestCard(quest);
        }
    }
    
    private void CreateQuestCard(RandomQuestData.ActiveQuest quest)
    {
        PanelContainer card = new PanelContainer();
        card.Modulate = new Color(1, 1, 1, 0.9f);
        _activeQuestsContainer.AddChild(card);
        
        VBoxContainer content = new VBoxContainer();
        card.AddChild(content);
        
        // Header
        HBoxContainer header = new HBoxContainer();
        content.AddChild(header);
        
        // Difficulty color
        Color diffColor = Colors.Green;
        if (quest.Difficulty == "Medium") diffColor = Colors.Yellow;
        if (quest.Difficulty == "Hard") diffColor = Colors.Orange;
        if (quest.Difficulty == "Epic") diffColor = Colors.Red;
        
        Label typeLabel = new Label();
        typeLabel.Text = $"[{quest.Type}] ";
        typeLabel.Modulate = Colors.Cyan;
        header.AddChild(typeLabel);
        
        Label diffLabel = new Label();
        diffLabel.Text = $"[{quest.Difficulty}] ";
        diffLabel.Modulate = diffColor;
        header.AddChild(diffLabel);
        
        Label nameLabel = new Label();
        nameLabel.Text = quest.Title;
        nameLabel.Modulate = Colors.Gold;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        header.AddChild(nameLabel);
        
        // Description
        Label descLabel = new Label();
        descLabel.Text = quest.Description;
        descLabel.Modulate = Colors.LightGray;
        content.AddChild(descLabel);
        
        // Progress
        HBoxContainer progressContainer = new HBoxContainer();
        content.AddChild(progressContainer);
        
        Label progressLabel = new Label();
        progressLabel.Text = $"Progress: {quest.CurrentAmount}/{quest.RequiredAmount}";
        progressLabel.Modulate = Colors.White;
        progressContainer.AddChild(progressLabel);
        
        // Time remaining
        Label timeLabel = new Label();
        int minutes = quest.TimeLimit / 60;
        int seconds = quest.TimeLimit % 60;
        timeLabel.Text = $"  Time: {minutes}:{seconds:D2}";
        timeLabel.Modulate = quest.TimeLimit < 60 ? Colors.Red : Colors.Yellow;
        progressContainer.AddChild(timeLabel);
        
        // Progress bar
        ProgressBar progressBar = new ProgressBar();
        progressBar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        progressBar.MinValue = 0;
        progressBar.MaxValue = quest.RequiredAmount;
        progressBar.Value = quest.CurrentAmount;
        progressBar.Modulate = new Color(0.2f, 0.8f, 0.2f, 0.8f);
        content.AddChild(progressBar);
        
        // Rewards
        HBoxContainer rewardContainer = new HBoxContainer();
        content.AddChild(rewardContainer);
        
        Label goldLabel = new Label();
        goldLabel.Text = $"💰 {quest.RewardGold} ";
        goldLabel.Modulate = Colors.Gold;
        rewardContainer.AddChild(goldLabel);
        
        Label expLabel = new Label();
        expLabel.Text = $"✨ {quest.RewardExp} EXP";
        expLabel.Modulate = Colors.Cyan;
        rewardContainer.AddChild(expLabel);
        
        // Abandon button
        Button abandonBtn = new Button();
        abandonBtn.Text = "  Abandon  ";
        abandonBtn.Modulate = Colors.Red;
        abandonBtn.Pressed += () => OnAbandonPressed(quest.QuestId);
        content.AddChild(abandonBtn);
    }
    
    private void RefreshCompletedQuests()
    {
        foreach (Node child in _completedQuestsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (_questSystem.Data == null || _questSystem.Data.CompletedQuestIds == null || _questSystem.Data.CompletedQuestIds.Count == 0)
        {
            Label noCompleted = new Label();
            noCompleted.Text = "No completed quests yet.";
            noCompleted.Modulate = Colors.Gray;
            noCompleted.HorizontalAlignment = HorizontalAlignment.Center;
            _completedQuestsContainer.AddChild(noCompleted);
            return;
        }
        
        // Show recent completions
        int count = 0;
        for (int i = _questSystem.Data.CompletedQuestIds.Count - 1; i >= 0 && count < 20; i--)
        {
            var questId = _questSystem.Data.CompletedQuestIds[i];
            var template = RandomQuestDatabase.Instance.GetQuest(questId);
            
            if (template != null)
            {
                Label completedLabel = new Label();
                completedLabel.Text = $"✅ {template.Title} ({template.Difficulty}) - {template.BaseRewardGold}g";
                completedLabel.Modulate = Colors.Green;
                _completedQuestsContainer.AddChild(completedLabel);
                count++;
            }
        }
    }
    
    private void RefreshStatistics()
    {
        if (_questSystem.Data != null)
        {
            _totalGeneratedLabel.Text = $"Total Quests Generated: {_questSystem.Data.TotalQuestsGenerated}";
            _totalCompletedLabel.Text = $"Total Quests Completed: {_questSystem.Data.TotalQuestsCompleted}";
            _completionRateLabel.Text = $"Completion Rate: {_questSystem.GetCompletionRate() * 100:F1}%";
            _totalRewardsLabel.Text = $"Total Gold Earned: {_questSystem.Data.TotalQuestRewards}";
        }
    }
    
    private void OnGeneratePressed()
    {
        _questSystem.RefreshQuests(_playerLevel);
        RefreshData();
    }
    
    private void OnRefreshPressed()
    {
        RefreshData();
    }
    
    private void OnAbandonPressed(string questId)
    {
        _questSystem.AbandonQuest(questId);
        RefreshData();
    }
    
    public void ToggleVisibility(bool? overrideVisibility = null)
    {
        bool newVisibility = overrideVisibility ?? !_isVisible;
        _isVisible = newVisibility;
        Visible = newVisibility;
        
        if (newVisibility)
        {
            RefreshData();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                ToggleVisibility(false);
            }
        }
    }
}
