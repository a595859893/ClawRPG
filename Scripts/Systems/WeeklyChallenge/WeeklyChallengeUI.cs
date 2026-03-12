using Godot;
using System;
using System.Collections.Generic;

public partial class WeeklyChallengeUI : Control
{
    private Control mainContainer;
    private VBoxContainer challengeList;
    private Label titleLabel;
    private Label timerLabel;
    private Label pointsLabel;
    private Label progressLabel;
    private Button claimButton;
    private TabContainer tabContainer;
    
    private Color colorEasy = new Color(0.2f, 0.8f, 0.2f);
    private Color colorMedium = new Color(0.2f, 0.6f, 1.0f);
    private Color colorHard = new Color(0.8f, 0.6f, 0.2f);
    private Color colorEpic = new Color(0.8f, 0.3f, 0.8f);
    
    public override void _Ready()
    {
        Visible = false;
        SetupUI();
        
        // Connect to system signals
        if (WeeklyChallengeSystem.Instance != null)
        {
            WeeklyChallengeSystem.Instance.ChallengeProgressUpdated += OnChallengeProgressUpdated;
            WeeklyChallengeSystem.Instance.ChallengeCompleted += OnChallengeCompleted;
            WeeklyChallengeSystem.Instance.RewardsClaimed += OnRewardsClaimed;
        }
    }
    
    private void SetupUI()
    {
        // Main container
        mainContainer = new Control();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        AddChild(mainContainer);
        
        var bgPanel = new PanelContainer();
        bgPanel.CustomMinimumSize = new Vector2(700, 500);
        mainContainer.AddChild(bgPanel);
        
        var margin = new MarginContainer();
        margin.SetOffsets(20, 20, -20, -20);
        bgPanel.AddChild(margin);
        
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        margin.AddChild(vbox);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = "Weekly Challenges";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(titleLabel);
        
        // Timer and points row
        var infoRow = new HBoxContainer();
        infoRow.Alignment = BoxContainer.Alignment.Center;
        vbox.AddChild(infoRow);
        
        timerLabel = new Label();
        timerLabel.Text = "Time Remaining: 7 days";
        infoRow.AddChild(timerLabel);
        
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(50, 0);
        infoRow.AddChild(spacer);
        
        pointsLabel = new Label();
        pointsLabel.Text = "Points: 0";
        pointsLabel.AddThemeFontSizeOverride("font_size", 18);
        infoRow.AddChild(pointsLabel);
        
        // Progress
        progressLabel = new Label();
        progressLabel.Text = "Completed: 0/0";
        progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(progressLabel);
        
        // Tab container
        tabContainer = new TabContainer();
        tabContainer.CustomMinimumSize = new Vector2(660, 350);
        vbox.AddChild(tabContainer);
        
        // Create tabs for each challenge type
        CreateTab("All", null);
        CreateTab("Combat", ChallengeType.Combat);
        CreateTab("Exploration", ChallengeType.Exploration);
        CreateTab("Collection", ChallengeType.Collection);
        CreateTab("Crafting", ChallengeType.Crafting);
        CreateTab("Social", ChallengeType.Social);
        CreateTab("Economy", ChallengeType.Economy);
        
        // Claim button
        claimButton = new Button();
        claimButton.Text = "Claim Rewards";
        claimButton.CustomMinimumSize = new Vector2(200, 40);
        claimButton.Pressed += OnClaimPressed;
        vbox.AddChild(claimButton);
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close";
        closeButton.Pressed += OnClosePressed;
        vbox.AddChild(closeButton);
        
        // Style
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);
        bgPanel.AddChild(style);
        
        RefreshUI();
    }
    
    private void CreateTab(string name, ChallengeType? type)
    {
        var scroll = new ScrollContainer();
        scroll.Name = name;
        tabContainer.AddChild(scroll);
        
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 8);
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        scroll.AddChild(vbox);
        
        challengeList = vbox;
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt.IsActionPressed("ui_cancel"))
        {
            Visible = false;
        }
    }
    
    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshUI();
        }
    }
    
    private void RefreshUI()
    {
        if (WeeklyChallengeSystem.Instance == null) return;
        
        // Update timer
        var timeRemaining = WeeklyChallengeSystem.Instance.GetTimeRemaining();
        timerLabel.Text = $"Time Remaining: {timeRemaining.Days}d {timeRemaining.Hours}h";
        
        // Update points
        pointsLabel.Text = $"Points: {WeeklyChallengeSystem.Instance.GetTotalPoints()}";
        
        // Update progress
        int completed = WeeklyChallengeSystem.Instance.GetCompletedCount();
        int total = WeeklyChallengeSystem.Instance.GetTotalCount();
        progressLabel.Text = $"Completed: {completed}/{total}";
        
        // Update claim button
        claimButton.Disabled = !WeeklyChallengeSystem.Instance.CanClaimRewards();
        claimButton.Text = WeeklyChallengeSystem.Instance.Data?.RewardsClaimed == true 
            ? "Rewards Claimed" 
            : "Claim Rewards";
        
        // Refresh challenge lists
        RefreshChallengeList("All", null);
        RefreshChallengeList("Combat", ChallengeType.Combat);
        RefreshChallengeList("Exploration", ChallengeType.Exploration);
        RefreshChallengeList("Collection", ChallengeType.Collection);
        RefreshChallengeList("Crafting", ChallengeType.Crafting);
        RefreshChallengeList("Social", ChallengeType.Social);
        RefreshChallengeList("Economy", ChallengeType.Economy);
    }
    
    private void RefreshChallengeList(string tabName, ChallengeType? type)
    {
        var tab = tabContainer.FindChild(tabName, true, false) as Control;
        if (tab == null) return;
        
        var vbox = tab.GetChild(0) as VBoxContainer;
        if (vbox == null) return;
        
        // Clear existing items
        foreach (var child in vbox.GetChildren())
        {
            child.QueueFree();
        }
        
        var challenges = type == null 
            ? WeeklyChallengeSystem.Instance.GetChallenges() 
            : WeeklyChallengeSystem.Instance.GetChallengesByType(type.Value);
        
        foreach (var challenge in challenges.Values)
        {
            var card = CreateChallengeCard(challenge);
            vbox.AddChild(card);
        }
    }
    
    private Control CreateChallengeCard(WeeklyChallenge challenge)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(600, 80);
        
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 15);
        panel.AddChild(hbox);
        
        // Progress section
        var progressBox = new VBoxContainer();
        progressBox.Alignment = BoxContainer.Alignment.Center;
        hbox.AddChild(progressBox);
        
        var progressBar = new ProgressBar();
        progressBar.CustomMinimumSize = new Vector2(100, 20);
        progressBar.MaxValue = challenge.TargetValue;
        progressBar.Value = challenge.CurrentValue;
        progressBar.ShowPercentage = false;
        progressBox.AddChild(progressBar);
        
        var progressText = new Label();
        progressText.Text = $"{challenge.CurrentValue}/{challenge.TargetValue}";
        progressText.HorizontalAlignment = HorizontalAlignment.Center;
        progressBox.AddChild(progressText);
        
        // Info section
        var infoBox = new VBoxContainer();
        infoBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(infoBox);
        
        var nameLabel = new Label();
        nameLabel.Text = challenge.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        infoBox.AddChild(nameLabel);
        
        var descLabel = new Label();
        descLabel.Text = challenge.Description;
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        infoBox.AddChild(descLabel);
        
        // Difficulty and points
        var statsBox = new VBoxContainer();
        statsBox.Alignment = BoxContainer.Alignment.Center;
        hbox.AddChild(statsBox);
        
        var difficultyLabel = new Label();
        difficultyLabel.Text = challenge.Difficulty.ToString();
        difficultyLabel.AddThemeFontSizeOverride("font_size", 14);
        difficultyLabel.Modulate = GetDifficultyColor(challenge.Difficulty);
        statsBox.AddChild(difficultyLabel);
        
        var pointsText = new Label();
        pointsText.Text = $"{challenge.Points} pts";
        pointsText.AddThemeFontSizeOverride("font_size", 14);
        statsBox.AddChild(pointsText);
        
        // Completed indicator
        if (challenge.IsCompleted)
        {
            var checkLabel = new Label();
            checkLabel.Text = "✓";
            checkLabel.AddThemeFontSizeOverride("font_size", 24);
            checkLabel.Modulate = new Color(0.2f, 0.8f, 0.2f);
            hbox.AddChild(checkLabel);
        }
        
        // Style
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.15f, 0.2f);
        style.BorderColor = challenge.IsCompleted ? new Color(0.2f, 0.6f, 0.3f) : new Color(0.3f, 0.3f, 0.4f);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(6);
        panel.AddChild(style);
        
        return panel;
    }
    
    private Color GetDifficultyColor(ChallengeDifficulty difficulty)
    {
        switch (difficulty)
        {
            case ChallengeDifficulty.Easy: return colorEasy;
            case ChallengeDifficulty.Medium: return colorMedium;
            case ChallengeDifficulty.Hard: return colorHard;
            case ChallengeDifficulty.Epic: return colorEpic;
            default: return Color.White;
        }
    }
    
    private void OnChallengeProgressUpdated(string challengeId, int current, int target)
    {
        CallDeferred(nameof(RefreshUI));
    }
    
    private void OnChallengeCompleted(string challengeId)
    {
        CallDeferred(nameof(RefreshUI));
    }
    
    private void OnRewardsClaimed(int gold, int exp)
    {
        CallDeferred(nameof(RefreshUI));
    }
    
    private void OnClaimPressed()
    {
        if (WeeklyChallengeSystem.Instance != null)
        {
            WeeklyChallengeSystem.Instance.ClaimRewards();
        }
    }
    
    private void OnClosePressed()
    {
        Visible = false;
    }
    
    public override void _ExitTree()
    {
        if (WeeklyChallengeSystem.Instance != null)
        {
            WeeklyChallengeSystem.Instance.ChallengeProgressUpdated -= OnChallengeProgressUpdated;
            WeeklyChallengeSystem.Instance.ChallengeCompleted -= OnChallengeCompleted;
            WeeklyChallengeSystem.Instance.RewardsClaimed -= OnRewardsClaimed;
        }
    }
}
