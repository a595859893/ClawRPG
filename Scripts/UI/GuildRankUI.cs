using Godot;
using System;
using System.Collections.Generic;

public partial class GuildRankUI : Control
{
	private GuildRankSystem rankSystem;
	private Label titleLabel;
	private Label seasonLabel;
	private Button leaderboardTab;
	private Button myRankTab;
	private Button seasonTab;
	private VBoxContainer contentContainer;
	private ScrollContainer leaderboardScroll;
	private VBoxContainer myRankContainer;
	private VBoxContainer seasonContainer;
	private Label currentRankLabel;
	private Label pointsLabel;
	private Label tierLabel;
	private Label streakLabel;
	private Label statsLabel;
	
	private int selectedTab = 0;
	private Color goldColor = new Color(1, 0.84f, 0);
	private Color silverColor = new Color(0.75f, 0.75f, 0.75f);
	private Color bronzeColor = new Color(0.8f, 0.5f, 0.2f);
	private Color platinumColor = new Color(0.9f, 0.9f, 0.95f);
	private Color diamondColor = new Color(0.65f, 0.95f, 1f);
	private Color masterColor = new Color(0.62f, 0.3f, 0.93f);
	private Color gmColor = new Color(1f, 0.42f, 0.42f);
	private Color championColor = new Color(1f, 0.08f, 0.58f);
	private Color legendColor = new Color(1f, 0.27f, 0f);
	private Color supremeColor = new Color(1f, 0f, 0f);
	
	public override void _Ready()
	{
		// Find the rank system
		rankSystem = GetNode<GuildRankSystem>("/root/Main/GuildRankSystem");
		if (rankSystem == null)
		{
			GD.PrintErr("GuildRankSystem not found!");
			return;
		}
		
		CreateUI();
		GD.Print("🏆 Guild Rank UI initialized");
	}
	
	private void CreateUI()
	{
		// Main container
		var mainContainer = new VBoxContainer();
		mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
		mainContainer.Position = new Vector2(400, 150);
		mainContainer.CustomMinimumSize = new Vector2(500, 500);
		AddChild(mainContainer);
		
		// Title
		titleLabel = new Label();
		titleLabel.Text = "🏆 Guild Rank System";
		titleLabel.Align = Label.AlignEnum.Center;
		titleLabel.AddThemeFontSizeOverride("font_size", 28);
		mainContainer.AddChild(titleLabel);
		
		// Season info
		seasonLabel = new Label();
		seasonLabel.Text = "Season 1";
		seasonLabel.Align = Label.AlignEnum.Center;
		seasonLabel.AddThemeFontSizeOverride("font_size", 16);
		mainContainer.AddChild(seasonLabel);
		
		// Tab buttons
		var tabContainer = new HBoxContainer();
		tabContainer.Alignment = BoxContainer.AlignmentMode.Center;
		mainContainer.AddChild(tabContainer);
		
		leaderboardTab = new Button();
		leaderboardTab.Text = "Leaderboard";
		leaderboardTab.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		leaderboardTab.Pressed += () => OnTabPressed(0);
		tabContainer.AddChild(leaderboardTab);
		
		myRankTab = new Button();
		myRankTab.Text = "My Rank";
		myRankTab.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		myRankTab.Pressed += () => OnTabPressed(1);
		tabContainer.AddChild(myRankTab);
		
		seasonTab = new Button();
		seasonTab.Text = "Season";
		seasonTab.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		seasonTab.Pressed += () => OnTabPressed(2);
		tabContainer.AddChild(seasonTab);
		
		// Content container
		contentContainer = new VBoxContainer();
		contentContainer.CustomMinimumSize = new Vector2(480, 380);
		mainContainer.AddChild(contentContainer);
		
		// Create tab contents
		CreateLeaderboardTab();
		CreateMyRankTab();
		CreateSeasonTab();
		
		// Show initial tab
		ShowTab(0);
		
		// Close button
		var closeButton = new Button();
		closeButton.Text = "Close (ESC)";
		closeButton.Align = Button.TextAlign.Center;
		closeButton.Pressed += OnClosePressed;
		mainContainer.AddChild(closeButton);
	}
	
	private void CreateLeaderboardTab()
	{
		leaderboardScroll = new ScrollContainer();
		leaderboardScroll.Visible = false;
		contentContainer.AddChild(leaderboardScroll);
		
		var listContainer = new VBoxContainer();
		leaderboardScroll.AddChild(listContainer);
		
		var header = new Label();
		header.Text = "🏅 Top Players";
		header.AddThemeFontSizeOverride("font_size", 20);
		header.Align = Label.AlignEnum.Center;
		listContainer.AddChild(header);
		
		// Sample data for display
		string[] sampleNames = { "DragonSlayer", "ShadowKnight", "PhoenixRider", "StormBreaker", "NightHawk" };
		int[] samplePoints = { 2500, 2100, 1800, 1500, 1200 };
		int[] sampleTiers = { 5, 4, 4, 3, 3 };
		
		for (int i = 0; i < sampleNames.Length; i++)
		{
			var playerPanel = CreatePlayerCard(i + 1, sampleNames[i], samplePoints[i], sampleTiers[i]);
			listContainer.AddChild(playerPanel);
		}
	}
	
	private Control CreatePlayerCard(int rank, string name, int points, int tier)
	{
		var panel = new PanelContainer();
		panel.CustomMinimumSize = new Vector2(450, 50);
		
		var hbox = new HBoxContainer();
		panel.AddChild(hbox);
		
		// Rank number
		var rankLabel = new Label();
		rankLabel.Text = "#" + rank;
		rankLabel.CustomMinimumSize = new Vector2(40, 0);
		rankLabel.AddThemeFontSizeOverride("font_size", 18);
		if (rank == 1) rankLabel.Modulate = goldColor;
		else if (rank == 2) rankLabel.Modulate = silverColor;
		else if (rank == 3) rankLabel.Modulate = bronzeColor;
		hbox.AddChild(rankLabel);
		
		// Player name
		var nameLabel = new Label();
		nameLabel.Text = name;
		nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
		nameLabel.AddThemeFontSizeOverride("font_size", 16);
		hbox.AddChild(nameLabel);
		
		// Tier
		var tierLabel = new Label();
		tierLabel.Text = rankSystem.GetTierName(tier);
		tierLabel.Modulate = GetTierColor(tier);
		tierLabel.AddThemeFontSizeOverride("font_size", 14);
		hbox.AddChild(tierLabel);
		
		// Points
		var pointsLabel = new Label();
		pointsLabel.Text = points + " pts";
		pointsLabel.AddThemeFontSizeOverride("font_size", 14);
		hbox.AddChild(pointsLabel);
		
		return panel;
	}
	
	private void CreateMyRankTab()
	{
		myRankContainer = new VBoxContainer();
		myRankContainer.Visible = false;
		contentContainer.AddChild(myRankContainer);
		
		var header = new Label();
		header.Text = "📊 Your Rank";
		header.AddThemeFontSizeOverride("font_size", 20);
		header.Align = Label.AlignEnum.Center;
		myRankContainer.AddChild(header);
		
		// Current rank display
		currentRankLabel = new Label();
		currentRankLabel.Text = "🥉 Bronze";
		currentRankLabel.Align = Label.AlignEnum.Center;
		currentRankLabel.AddThemeFontSizeOverride("font_size", 32);
		myRankContainer.AddChild(currentRankLabel);
		
		// Points
		pointsLabel = new Label();
		pointsLabel.Text = "Points: 100";
		pointsLabel.Align = Label.AlignEnum.Center;
		pointsLabel.AddThemeFontSizeOverride("font_size", 18);
		myRankContainer.AddChild(pointsLabel);
		
		// Progress to next tier
		var progressBar = new ProgressBar();
		progressBar.CustomMinimumSize = new Vector2(400, 20);
		progressBar.Value = 30;
		progressBar.MaxValue = 100;
		myRankContainer.AddChild(progressBar);
		
		var progressLabel = new Label();
		progressLabel.Text = "Next Tier: Silver (100/250)";
		progressLabel.Align = Label.AlignEnum.Center;
		myRankContainer.AddChild(progressLabel);
		
		// Stats
		var statsPanel = new VBoxContainer();
		myRankContainer.AddChild(statsPanel);
		
		tierLabel = new Label();
		tierLabel.Text = "Tier: Bronze I";
		statsPanel.AddChild(tierLabel);
		
		streakLabel = new Label();
		streakLabel.Text = "Current Streak: 0";
		statsPanel.AddChild(streakLabel);
		
		statsLabel = new Label();
		statsLabel.Text = "Wins: 0 | Losses: 0 | Win Rate: 0%";
		statsPanel.AddChild(statsLabel);
	}
	
	private void CreateSeasonTab()
	{
		seasonContainer = new VBoxContainer();
		seasonContainer.Visible = false;
		contentContainer.AddChild(seasonContainer);
		
		var header = new Label();
		header.Text = "📅 Season Statistics";
		header.AddThemeFontSizeOverride("font_size", 20);
		header.Align = Label.AlignEnum.Center;
		seasonContainer.AddChild(header);
		
		var stats = rankSystem.GetSeasonStats();
		
		var seasonInfo = new Label();
		seasonInfo.Text = $"Season: {stats["season"]}\nStarted: {stats["startDate"]}";
		seasonInfo.Align = Label.AlignEnum.Center;
		seasonContainer.AddChild(seasonInfo);
		
		var totalInfo = new Label();
		totalInfo.Text = $"Total Matches: {(int)stats["totalWins"] + (int)stats["totalLosses"]}\nWins: {stats["totalWins"]} | Losses: {stats["totalLosses"]}";
		totalInfo.Align = Label.AlignEnum.Center;
		seasonContainer.AddChild(totalInfo);
		
		var winRateLabel = new Label();
		winRateLabel.Text = $"Win Rate: {stats["winRate"]:F1}%";
		winRateLabel.Align = Label.AlignEnum.Center;
		winRateLabel.AddThemeFontSizeOverride("font_size", 24);
		seasonContainer.AddChild(winRateLabel);
		
		// Start new season button
		var newSeasonButton = new Button();
		newSeasonButton.Text = "Start New Season";
		newSeasonButton.Pressed += OnNewSeasonPressed;
		seasonContainer.AddChild(newSeasonButton);
	}
	
	private void OnTabPressed(int tabIndex)
	{
		ShowTab(tabIndex);
	}
	
	private void ShowTab(int tabIndex)
	{
		selectedTab = tabIndex;
		
		if (leaderboardScroll != null)
			leaderboardScroll.Visible = (tabIndex == 0);
		if (myRankContainer != null)
			myRankContainer.Visible = (tabIndex == 1);
		if (seasonContainer != null)
			seasonContainer.Visible = (tabIndex == 2);
		
		leaderboardTab.Modulate = (tabIndex == 0) ? goldColor : Color.White;
		myRankTab.Modulate = (tabIndex == 1) ? goldColor : Color.White;
		seasonTab.Modulate = (tabIndex == 2) ? goldColor : Color.White;
	}
	
	private void OnNewSeasonPressed()
	{
		rankSystem.StartNewSeason();
		seasonLabel.Text = "Season " + rankSystem.GetSeasonStats()["season"];
		GD.Print("🔄 New season started!");
	}
	
	private void OnClosePressed()
	{
		QueueFree();
	}
	
	private Color GetTierColor(int tierLevel)
	{
		switch (tierLevel)
		{
			case 1: return bronzeColor;
			case 2: return silverColor;
			case 3: return goldColor;
			case 4: return platinumColor;
			case 5: return diamondColor;
			case 6: return masterColor;
			case 7: return gmColor;
			case 8: return championColor;
			case 9: return legendColor;
			case 10: return supremeColor;
			default: return bronzeColor;
		}
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
		{
			OnClosePressed();
		}
	}
}
