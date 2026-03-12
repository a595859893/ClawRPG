using Godot;
using System;
using System.Collections.Generic;

public partial class DynamicQuestUI : Control
{
    private TabContainer _tabContainer;
    private VBoxContainer _availableQuestsContainer;
    private VBoxContainer _activeQuestsContainer;
    private VBoxContainer _completedQuestsContainer;
    private Label _goldRewardLabel;
    private Label _expRewardLabel;
    
    private Button _generateButton;
    private Button _closeButton;
    
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        SetupUI();
        VisibilityChanged += OnVisibilityChanged;
        
        // Connect to quest system signals
        if (DynamicQuestSystem.Instance != null)
        {
            DynamicQuestSystem.Instance.QuestGenerated += OnQuestGenerated;
            DynamicQuestSystem.Instance.QuestCompleted += OnQuestCompleted;
            DynamicQuestSystem.Instance.QuestFailed += OnQuestFailed;
        }
    }
    
    private void SetupUI()
    {
        // Main panel
        var panel = new Panel
        {
            Name = "Panel",
            Size = new Vector2(600, 500),
            Position = new Vector2(100, 50)
        };
        AddChild(panel);
        
        // Title
        var titleLabel = new Label
        {
            Text = "Dynamic Quest System",
            HorizontalAlignment = HorizontalAlignment.Center,
            Position = new Vector2(0, 10),
            Size = new Vector2(600, 30)
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        panel.AddChild(titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer
        {
            Position = new Vector2(20, 50),
            Size = new Vector2(560, 380)
        };
        panel.AddChild(_tabContainer);
        
        // Available quests tab
        var availableTab = new Control { Name = "Available" };
        _availableQuestsContainer = new VBoxContainer { Size = new Vector2(540, 340) };
        availableTab.AddChild(_availableQuestsContainer);
        
        var availableLabel = new Label { Text = "Available Quests", Position = new Vector2(10, 10) };
        availableTab.AddChild(availableLabel);
        _tabContainer.AddChild(availableTab);
        
        // Active quests tab
        var activeTab = new Control { Name = "Active" };
        _activeQuestsContainer = new VBoxContainer { Size = new Vector2(540, 340) };
        activeTab.AddChild(_activeQuestsContainer);
        
        var activeLabel = new Label { Text = "Active Quests", Position = new Vector2(10, 10) };
        activeTab.AddChild(activeLabel);
        _tabContainer.AddChild(activeTab);
        
        // Completed quests tab
        var completedTab = new Control { Name = "Completed" };
        _completedQuestsContainer = new VBoxContainer { Size = new Vector2(540, 340) };
        completedTab.AddChild(_completedQuestsContainer);
        
        var completedLabel = new Label { Text = "Completed Quests", Position = new Vector2(10, 10) };
        completedTab.AddChild(completedLabel);
        _tabContainer.AddChild(completedTab);
        
        // Generate button
        _generateButton = new Button
        {
            Text = "Generate New Quests",
            Position = new Vector2(20, 440),
            Size = new Vector2(180, 30)
        };
        _generateButton.Pressed += OnGeneratePressed;
        panel.AddChild(_generateButton);
        
        // Close button
        _closeButton = new Button
        {
            Text = "Close",
            Position = new Vector2(490, 440),
            Size = new Vector2(90, 30)
        };
        _closeButton.Pressed += OnClosePressed;
        panel.AddChild(_closeButton);
        
        // Reward display
        var rewardPanel = new Panel
        {
            Position = new Vector2(210, 440),
            Size = new Vector2(270, 30)
        };
        panel.AddChild(rewardPanel);
        
        _goldRewardLabel = new Label
        {
            Text = "Gold: 0",
            Position = new Vector2(10, 5)
        };
        rewardPanel.AddChild(_goldRewardLabel);
        
        _expRewardLabel = new Label
        {
            Text = "EXP: 0",
            Position = new Vector2(140, 5)
        };
        rewardPanel.AddChild(_expRewardLabel);
        
        Hide();
    }
    
    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            RefreshQuestLists();
        }
    }
    
    private void RefreshQuestLists()
    {
        // Clear existing
        foreach (var child in _availableQuestsContainer.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var child in _activeQuestsContainer.GetChildren())
        {
            child.QueueFree();
        }
        foreach (var child in _completedQuestsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (DynamicQuestSystem.Instance == null) return;
        
        // Populate available quests
        foreach (var quest in DynamicQuestSystem.Instance.GetAvailableQuests())
        {
            _availableQuestsContainer.AddChild(CreateQuestCard(quest));
        }
        
        // Populate active quests
        foreach (var quest in DynamicQuestSystem.Instance.GetActiveQuests())
        {
            _activeQuestsContainer.AddChild(CreateQuestCard(quest, true));
        }
        
        // Populate completed quests
        foreach (var quest in DynamicQuestSystem.Instance.GetCompletedQuests())
        {
            _completedQuestsContainer.AddChild(CreateQuestCard(quest));
        }
    }
    
    private Control CreateQuestCard(DynamicQuestSystem.Quest quest, bool showProgress = false)
    {
        var card = new Panel
        {
            Size = new Vector2(520, 80),
            CustomMinimumSize = new Vector2(520, 80)
        };
        
        var color = GetDifficultyColor(quest.Difficulty);
        var styleBox = new StyleBoxFlat { BgColor = color };
        styleBox.BorderWidthLeft = 3;
        styleBox.BorderColor = color.Darkened(0.3f);
        card.AddThemeStyleboxOverride("panel", styleBox);
        
        // Title
        var titleLabel = new Label
        {
            Text = quest.Title,
            Position = new Vector2(10, 5),
            Size = new Vector2(300, 25)
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 16);
        card.AddChild(titleLabel);
        
        // Description
        var descLabel = new Label
        {
            Text = quest.Description,
            Position = new Vector2(10, 30),
            Size = new Vector2(350, 40),
            AutowrapMode = TextServer.AutowrapWord
        };
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        card.AddChild(descLabel);
        
        // Rewards
        var rewardText = $"Gold: {quest.RewardGold}\nEXP: {quest.RewardExp}";
        var rewardLabel = new Label
        {
            Text = rewardText,
            Position = new Vector2(370, 10),
            Size = new Vector2(140, 40)
        };
        rewardLabel.AddThemeFontSizeOverride("font_size", 12);
        card.AddChild(rewardLabel);
        
        // Progress (for active quests)
        if (showProgress && quest.Status == DynamicQuestSystem.QuestStatus.Active)
        {
            var progressLabel = new Label
            {
                Text = $"Progress: {quest.CurrentProgress}/{quest.TargetCount}",
                Position = new Vector2(10, 60),
                Size = new Vector2(200, 20)
            };
            progressLabel.AddThemeFontSizeOverride("font_size", 11);
            card.AddChild(progressLabel);
            
            if (quest.TimeLimit > 0)
            {
                var timeRemaining = quest.TimeLimit - (float)(DateTime.Now - quest.StartTime).TotalSeconds;
                var timeLabel = new Label
                {
                    Text = $"Time: {timeRemaining:F0}s",
                    Position = new Vector2(220, 60),
                    Size = new Vector2(100, 20)
                };
                timeLabel.AddThemeFontSizeOverride("font_size", 11);
                card.AddChild(timeLabel);
            }
        }
        
        // Start button (for available quests)
        if (quest.Status == DynamicQuestSystem.QuestStatus.Available)
        {
            var startButton = new Button
            {
                Text = "Start",
                Position = new Vector2(420, 25),
                Size = new Vector2(80, 30)
            };
            startButton.Pressed += () => OnStartQuest(quest.Id);
            card.AddChild(startButton);
        }
        
        return card;
    }
    
    private Color GetDifficultyColor(DynamicQuestSystem.QuestDifficulty difficulty)
    {
        return difficulty switch
        {
            DynamicQuestSystem.QuestDifficulty.Easy => new Color(0.2f, 0.8f, 0.2f, 0.3f),
            DynamicQuestSystem.QuestDifficulty.Medium => new Color(0.2f, 0.6f, 0.8f, 0.3f),
            DynamicQuestSystem.QuestDifficulty.Hard => new Color(0.8f, 0.6f, 0.2f, 0.3f),
            DynamicQuestSystem.QuestDifficulty.Epic => new Color(0.6f, 0.2f, 0.8f, 0.3f),
            DynamicQuestSystem.QuestDifficulty.Legendary => new Color(0.8f, 0.2f, 0.2f, 0.3f),
            _ => new Color(0.5f, 0.5f, 0.5f, 0.3f)
        };
    }
    
    private void OnGeneratePressed()
    {
        DynamicQuestSystem.Instance?.GenerateDailyQuests();
        RefreshQuestLists();
    }
    
    private void OnStartQuest(string questId)
    {
        DynamicQuestSystem.Instance?.StartQuest(questId);
        RefreshQuestLists();
    }
    
    private void OnQuestGenerated(DynamicQuestSystem.Quest quest)
    {
        if (Visible) RefreshQuestLists();
    }
    
    private void OnQuestCompleted(DynamicQuestSystem.Quest quest)
    {
        if (Visible) RefreshQuestLists();
    }
    
    private void OnQuestFailed(DynamicQuestSystem.Quest quest)
    {
        if (Visible) RefreshQuestLists();
    }
    
    private void OnClosePressed()
    {
        Hide();
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == KeyCode.KeyQ)
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                RefreshQuestLists();
            }
        }
    }
}
