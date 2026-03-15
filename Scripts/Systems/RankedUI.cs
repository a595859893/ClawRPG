using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 排位UI - 显示排位赛界面
/// </summary>
public partial class RankedUI : Control
{
    private Control container;
    private Label titleLabel;
    private Label rankLabel;
    private Label pointsLabel;
    private Label winsLabel;
    private Label lossesLabel;
    private Label streakLabel;
    private Label winRateLabel;
    private Label seasonLabel;
    private ProgressBar seasonProgress;
    private VBoxContainer matchHistoryContainer;
    private VBoxContainer leaderboardContainer;
    private Button closeButton;

    private bool isVisible = false;

    public override void _Ready()
    {
        SetupUI();
        Hide();
    }

    private void SetupUI()
    {
        // Main container
        container = new Control();
        container.SetAnchorsPreset(Control.LayoutPreset.Center);
        container.CustomMinimumSize = new Vector2(600, 500);
        AddChild(container);

        // Background panel
        var bg = new PanelContainer();
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        bg.Modulate = new Color(0, 0, 0, 0.85f);
        container.AddChild(bg);

        var margin = new MarginContainer();
        margin.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        margin.AddThemeConstantOverride("margin_left", 20);
        margin.AddThemeConstantOverride("margin_right", 20);
        margin.AddThemeConstantOverride("margin_top", 20);
        margin.AddThemeConstantOverride("margin_bottom", 20);
        bg.AddChild(margin);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 10);
        margin.AddChild(vbox);

        // Title
        titleLabel = new Label();
        titleLabel.Text = "🏆 PvP Ranked System";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(titleLabel);

        // Rank display
        var rankBox = new HBoxContainer();
        rankBox.Alignment = BoxContainer.Alignment.Center;
        vbox.AddChild(rankBox);

        rankLabel = new Label();
        rankLabel.Text = "Bronze IV";
        rankLabel.AddThemeFontSizeOverride("font_size", 32);
        rankBox.AddChild(rankLabel);

        // Points
        pointsLabel = new Label();
        pointsLabel.Text = "0 Points";
        pointsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(pointsLabel);

        // Stats grid
        var statsGrid = new GridContainer();
        statsGrid.Columns = 3;
        vbox.AddChild(statsGrid);

        winsLabel = new Label();
        winsLabel.Text = "Wins: 0";
        winsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statsGrid.AddChild(winsLabel);

        lossesLabel = new Label();
        lossesLabel.Text = "Losses: 0";
        lossesLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statsGrid.AddChild(lossesLabel);

        streakLabel = new Label();
        streakLabel.Text = "Streak: 0";
        streakLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statsGrid.AddChild(streakLabel);

        winRateLabel = new Label();
        winRateLabel.Text = "Win Rate: 0%";
        winRateLabel.HorizontalAlignment = HorizontalAlignment.Center;
        winRateLabel.CustomMinimumSize = new Vector2(200, 0);
        statsGrid.AddChild(winRateLabel);

        // Season info
        seasonLabel = new Label();
        seasonLabel.Text = "Season: 2026-03";
        seasonLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(seasonLabel);

        seasonProgress = new ProgressBar();
        seasonProgress.CustomMinimumSize = new Vector2(0, 20);
        seasonProgress.Value = 50;
        vbox.AddChild(seasonProgress);

        // Tab container for match history and leaderboard
        var tabContainer = new TabContainer();
        tabContainer.SetVExpand(true);
        vbox.AddChild(tabContainer);

        // Match history tab
        matchHistoryContainer = new VBoxContainer();
        matchHistoryContainer.Name = "Match History";
        tabContainer.AddChild(matchHistoryContainer);

        var historyTitle = new Label();
        historyTitle.Text = "Recent Matches";
        historyTitle.AddThemeFontSizeOverride("font_size", 18);
        matchHistoryContainer.AddChild(historyTitle);

        // Leaderboard tab
        leaderboardContainer = new VBoxContainer();
        leaderboardContainer.Name = "Leaderboard";
        tabContainer.AddChild(leaderboardContainer);

        var leaderboardTitle = new Label();
        leaderboardTitle.Text = "Top Players";
        leaderboardTitle.AddThemeFontSizeOverride("font_size", 18);
        leaderboardContainer.AddChild(leaderboardTitle);

        // Close button
        closeButton = new Button();
        closeButton.Text = "Close";
        closeButton.CustomMinimumSize = new Vector2(100, 40);
        closeButton.Pressed += () => Toggle();
        vbox.AddChild(closeButton);

        // Update display
        RefreshDisplay();
    }

    public void RefreshDisplay()
    {
        if (RankedSystem.Instance == null) return;

        var stats = RankedSystem.Instance.GetPlayerStats();
        
        rankLabel.Text = $"{stats["tier"]} {stats["division"]}";
        pointsLabel.Text = $"{stats["points"]} Points";
        
        // Set rank color based on tier
        string tierStr = stats["tier"].ToString();
        Color tierColor = new Color(0.8f, 0.5f, 0.3f); // Bronze default
        switch (tierStr)
        {
            case "Silver": tierColor = new Color(0.7f, 0.7f, 0.8f); break;
            case "Gold": tierColor = new Color(1f, 0.85f, 0.3f); break;
            case "Diamond": tierColor = new Color(0.4f, 0.9f, 1f); break;
            case "Master": tierColor = new Color(0.9f, 0.3f, 0.9f); break;
            case "GrandMaster": tierColor = new Color(1f, 0.3f, 0.3f); break;
        }
        rankLabel.Modulate = tierColor;
        
        winsLabel.Text = $"Wins: {stats["wins"]}";
        lossesLabel.Text = $"Losses: {stats["losses"]}";
        streakLabel.Text = $"🔥 Streak: {stats["currentStreak"]}";
        winRateLabel.Text = $"Win Rate: {stats["winRate"]:F1}%";
        
        seasonLabel.Text = RankedSystem.Instance.GetSeasonInfo();
        seasonProgress.Value = RankedSystem.Instance.GetSeasonProgress() * 100;

        // Refresh match history
        RefreshMatchHistory();
        
        // Refresh leaderboard
        RefreshLeaderboard();
    }

    private void RefreshMatchHistory()
    {
        // Clear existing
        foreach (Node child in matchHistoryContainer.GetChildren())
        {
            if (child is Label || child is HBoxContainer) child.QueueFree();
        }

        var matches = RankedSystem.Instance.GetMatchHistory(10);
        
        foreach (var match in matches)
        {
            var hbox = new HBoxContainer();
            matchHistoryContainer.AddChild(hbox);
            
            var resultLabel = new Label();
            resultLabel.Text = match.playerWon ? "✅ WIN" : "❌ LOSS";
            resultLabel.Modulate = match.playerWon ? new Color(0.3f, 1f, 0.3f) : new Color(1f, 0.3f, 0.3f);
            hbox.AddChild(resultLabel);
            
            var pointsLabel = new Label();
            pointsLabel.Text = $" {(match.pointsChange >= 0 ? "+" : "")}{match.pointsChange} pts";
            hbox.AddChild(pointsLabel);
            
            var opponentLabel = new Label();
            opponentLabel.Text = $" vs {match.opponentName}";
            hbox.AddChild(opponentLabel);
            
            var timeLabel = new Label();
            timeLabel.Text = $" {match.matchTime:HH:mm}";
            timeLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
            hbox.AddChild(timeLabel);
        }
    }

    private void RefreshLeaderboard()
    {
        // Clear existing
        foreach (Node child in leaderboardContainer.GetChildren())
        {
            if (child is Label || child is HBoxContainer) child.QueueFree();
        }

        var leaderboard = RankedSystem.Instance.GetLeaderboard(10);
        
        int rank = 1;
        foreach (var kvp in leaderboard)
        {
            var hbox = new HBoxContainer();
            leaderboardContainer.AddChild(hbox);
            
            var rankLabel = new Label();
            rankLabel.Text = $"#{rank}";
            rankLabel.CustomMinimumSize = new Vector2(40, 0);
            hbox.AddChild(rankLabel);
            
            var nameLabel = new Label();
            nameLabel.Text = kvp.Key;
            hbox.AddChild(nameLabel);
            
            var pointsLabel = new Label();
            pointsLabel.Text = $" {kvp.Value} pts";
            pointsLabel.Modulate = new Color(1f, 0.85f, 0.3f);
            hbox.AddChild(pointsLabel);
            
            rank++;
        }
    }

    public void Toggle()
    {
        if (isVisible)
        {
            Hide();
            isVisible = false;
        }
        else
        {
            Show();
            RefreshDisplay();
            isVisible = true;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            if (isVisible)
            {
                Toggle();
            }
        }
        
        // Toggle with K key (conflicting with combat stance, using Shift+K
        if (@event is InputEventKey keyEvent2 && keyEvent2.Pressed && keyEvent2.Keycode == Key.K)
        {
            if (Input.IsKeyPressed(Key.Shift))
            {
                Toggle();
            }
        }
    }
}
