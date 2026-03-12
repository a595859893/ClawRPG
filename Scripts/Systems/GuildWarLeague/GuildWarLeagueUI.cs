using Godot;
using System;
using System.Collections.Generic;

public class GuildWarLeagueUI : Control {
    
    private GuildWarLeagueSystem _system;
    private TabContainer _tabContainer;
    private Label _seasonLabel;
    private Label _daysLabel;
    
    // Current tab
    private int _currentTab = 0;
    
    public override void _Ready() {
        base._Ready();
        
        // Get reference to system
        _system = GetNode<GuildWarLeagueSystem>("/root/Main/GuildWarLeagueSystem");
        
        SetupUI();
    }
    
    private void SetupUI() {
        // Main panel
        var mainPanel = new PanelContainer();
        mainPanel.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Wide);
        mainPanel.OffsetLeft = 50;
        mainPanel.OffsetRight = -50;
        mainPanel.OffsetTop = 50;
        mainPanel.OffsetBottom = -50;
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // Header
        var header = new HBoxContainer();
        mainVBox.AddChild(header);
        
        var titleLabel = new Label();
        titleLabel.Text = "🏆 Guild War League";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        header.AddChild(titleLabel);
        
        header.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        _seasonLabel = new Label();
        _seasonLabel.Text = "Season 1";
        header.AddChild(_seasonLabel);
        
        _daysLabel = new Label();
        _daysLabel.Text = "30 days remaining";
        header.AddChild(_daysLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        mainVBox.AddChild(_tabContainer);
        
        // Create tabs
        CreateStandingsTab();
        CreateMatchesTab();
        CreateRewardsTab();
        CreateStatisticsTab();
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += () => Hide();
        mainVBox.AddChild(closeButton);
        
        // Update display
        UpdateDisplay();
    }
    
    private void CreateStandingsTab() {
        var scroll = new ScrollContainer();
        scroll.Name = "Standings";
        _tabContainer.AddChild(scroll);
        
        var vbox = new VBoxContainer();
        vbox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        vbox.SizeFlagsVertical = Control.SizeFlags.Expand;
        scroll.AddChild(vbox);
        
        // Division filter
        var filterLabel = new Label();
        filterLabel.Text = "Division Standings";
        filterLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(filterLabel);
        
        var divisions = new string[] { "Legendary", "Diamond", "Platinum", "Gold", "Silver", "Bronze" };
        foreach (var div in divisions) {
            var divLabel = new Label();
            divLabel.Text = $"=== {div} ===";
            divLabel.AddThemeColorOverride("font_color", GetDivisionColor(div));
            vbox.AddChild(divLabel);
            
            var guilds = _system.GetDivisionGuilds(div);
            var sorted = new List<KeyValuePair<string, GuildWarLeagueRecord>>(guilds);
            sorted.Sort((a, b) => a.Value.Rank.CompareTo(b.Value.Rank));
            
            foreach (var guild in sorted) {
                var guildLabel = new Label();
                guildLabel.Text = $"#{guild.Value.Rank} {guild.Value.GuildName} - {guild.Value.Points} pts ({guild.Value.Wins}W-{guild.Value.Losses}L-{guild.Value.Draws}D)";
                vbox.AddChild(guildLabel);
            }
        }
    }
    
    private void CreateMatchesTab() {
        var scroll = new ScrollContainer();
        scroll.Name = "Matches";
        _tabContainer.AddChild(scroll);
        
        var vbox = new VBoxContainer();
        vbox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        vbox.SizeFlagsVertical = Control.SizeFlags.Expand;
        scroll.AddChild(vbox);
        
        var titleLabel = new Label();
        titleLabel.Text = "Season Matches";
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(titleLabel);
        
        // Show recent matches
        var matches = _system.GetTopGuilds(100);
        foreach (var guild in matches.Values) {
            var guildMatches = _system.GetGuildMatches(guild.GuildId);
            foreach (var match in guildMatches) {
                var matchLabel = new Label();
                string result = match.MatchState == "Completed" 
                    ? $"{match.GuildAName} {match.GuildAScore} - {match.GuildBScore} {match.GuildBName}"
                    : $"{match.GuildAName} vs {match.GuildBName} ({match.MatchState})";
                matchLabel.Text = result;
                vbox.AddChild(matchLabel);
            }
        }
    }
    
    private void CreateRewardsTab() {
        var scroll = new ScrollContainer();
        scroll.Name = "Rewards";
        _tabContainer.AddChild(scroll);
        
        var vbox = new VBoxContainer();
        vbox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        vbox.SizeFlagsVertical = Control.SizeFlags.Expand;
        scroll.AddChild(vbox);
        
        var titleLabel = new Label();
        titleLabel.Text = "Season Rewards by Division";
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(titleLabel);
        
        var divisions = new string[] { "Bronze", "Silver", "Gold", "Platinum", "Diamond", "Legendary" };
        foreach (var div in divisions) {
            var reward = _system.GetSeasonReward(div);
            if (reward == null) continue;
            
            var rewardBox = new VBoxContainer();
            vbox.AddChild(rewardBox);
            
            var divLabel = new Label();
            divLabel.Text = $"=== {div} ===";
            divLabel.AddThemeColorOverride("font_color", GetDivisionColor(div));
            rewardBox.AddChild(divLabel);
            
            var goldLabel = new Label();
            goldLabel.Text = $"Gold: {reward.GoldReward}";
            rewardBox.AddChild(goldLabel);
            
            var repLabel = new Label();
            repLabel.Text = $"Reputation: {reward.ReputationReward}";
            rewardBox.AddChild(repLabel);
            
            var itemLabel = new Label();
            itemLabel.Text = $"Items: {string.Join(", ", reward.ItemRewardPool)}";
            rewardBox.AddChild(itemLabel);
        }
    }
    
    private void CreateStatisticsTab() {
        var vbox = new VBoxContainer();
        vbox.Name = "Statistics";
        _tabContainer.AddChild(vbox);
        
        var titleLabel = new Label();
        titleLabel.Text = "League Statistics";
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(titleLabel);
        
        var stats = _system.GetStatistics();
        
        var matchesLabel = new Label();
        matchesLabel.Text = $"Total Matches: {stats.TotalMatchesPlayed}";
        vbox.AddChild(matchesLabel);
        
        var guildsLabel = new Label();
        guildsLabel.Text = $"Total Guilds: {stats.TotalGuildsParticipated}";
        vbox.AddChild(guildsLabel);
        
        var seasonsLabel = new Label();
        seasonsLabel.Text = $"Total Seasons: {stats.TotalSeasons}";
        vbox.AddChild(seasonsLabel);
        
        var winStreakLabel = new Label();
        winStreakLabel.Text = $"Longest Win Streak: {stats.LongestWinStreak}";
        vbox.AddChild(winStreakLabel);
        
        var highPointsLabel = new Label();
        highPointsLabel.Text = $"Highest Points: {stats.HighestPoints}";
        vbox.AddChild(highPointsLabel);
        
        // Generate random matches button
        var generateButton = new Button();
        generateButton.Text = "Generate Test Matches";
        generateButton.Pressed += () => {
            _system.GenerateRandomMatches(10);
            UpdateDisplay();
        };
        vbox.AddChild(generateButton);
    }
    
    private void UpdateDisplay() {
        _seasonLabel.Text = $"Season {_system.GetCurrentSeason()}";
        int daysLeft = _system.GetSeasonDaysRemaining();
        _daysLabel.Text = $"{daysLeft} days remaining";
    }
    
    private Color GetDivisionColor(string division) {
        return division switch {
            "Legendary" => new Color(1f, 0.84f, 0f), // Gold
            "Diamond" => new Color(0.3f, 0.8f, 1f), // Cyan
            "Platinum" => new Color(0.75f, 0.75f, 0.75f), // Silver
            "Gold" => new Color(1f, 0.84f, 0f), // Gold
            "Silver" => new Color(0.7f, 0.7f, 0.7f), // Silver
            "Bronze" => new Color(0.8f, 0.5f, 0.2f), // Brown
            _ => Colors.White
        };
    }
    
    public override void _Input(InputEvent @event) {
        if (@event.IsActionPressed("ui_cancel")) {
            Hide();
        }
    }
}
