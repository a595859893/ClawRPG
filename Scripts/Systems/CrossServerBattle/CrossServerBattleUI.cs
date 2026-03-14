using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class CrossServerBattleUI : Control
{
    private CrossServerBattleSystem _system;
    private TabContainer _tabContainer;
    private Label _titleLabel;
    
    // Overview Tab
    private VBoxContainer _overviewTab;
    private Label _currentSeasonLabel;
    private Label _playerRankLabel;
    private Label _totalPointsLabel;
    private Label _winsLabel;
    private Label _lossesLabel;
    private Label _drawsLabel;
    private Label _winRateLabel;
    private Label _currentStreakLabel;
    private Label _bestStreakLabel;
    private Label _serverInfoLabel;
    private Button _startMatchButton;
    private OptionButton _matchTypeOption;
    
    // Rankings Tab
    private VBoxContainer _rankingsTab;
    private Tree _playerRankingTree;
    private Tree _serverRankingTree;
    private OptionButton _rankingTypeOption;
    
    // Matches Tab
    private VBoxContainer _matchesTab;
    private VBoxContainer _activeMatchesContainer;
    private VBoxContainer _matchHistoryContainer;
    private Button _refreshMatchesButton;
    
    // Statistics Tab
    private VBoxContainer _statisticsTab;
    private Label _totalMatchesLabel;
    private Label _totalWinsLabel;
    private Label _totalLossesLabel;
    private Label _totalDrawsLabel;
    private Label _bestRankLabel;
    private Label _highestPointsLabel;
    private Label _bestStreakStatLabel;
    private Label _activeServersLabel;
    private Label _registeredPlayersLabel;

    public override void _Ready()
    {
        _system = GetNode<CrossServerBattleSystem>("/root/Main/CrossServerBattleSystem");
        if (_system == null)
        {
            GD.PrintErr("[CrossServerBattleUI] System not found!");
            return;
        }

        SetupUI();
        SetupTabs();
        RefreshData();
        
        GD.Print("[CrossServerBattleUI] Initialized");
    }

    private void SetupUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0);
        mainContainer.MarginLeft = 50;
        mainContainer.MarginTop = 50;
        mainContainer.MarginRight = -50;
        mainContainer.MarginBottom = -50;
        AddChild(mainContainer);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ Cross-Server Battle System";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        mainContainer.AddChild(_titleLabel);

        // Tab Container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);

        // Close button
        var closeButton = new Button();
        closeButton.Text = "  Close (ESC)  ";
        closeButton.Align = Button.AlignMode.Center;
        closeButton.Connect("pressed", this, nameof(OnCloseButtonPressed));
        mainContainer.AddChild(closeButton);
    }

    private void SetupTabs()
    {
        SetupOverviewTab();
        SetupRankingsTab();
        SetupMatchesTab();
        SetupStatisticsTab();
    }

    private void SetupOverviewTab()
    {
        _overviewTab = new VBoxContainer();
        _overviewTab.Name = "Overview";
        _tabContainer.AddChild(_overviewTab);

        // Season Info
        _currentSeasonLabel = new Label();
        _currentSeasonLabel.Text = "Current Season: 1";
        _currentSeasonLabel.AddThemeFontSizeOverride("font_size", 20);
        _overviewTab.AddChild(_currentSeasonLabel);

        AddSeparator(_overviewTab);

        // Player Stats
        var statsLabel = new Label();
        statsLabel.Text = "Your Statistics";
        statsLabel.AddThemeFontSizeOverride("font_size", 18);
        _overviewTab.AddChild(statsLabel);

        _playerRankLabel = new Label();
        _playerRankLabel.Text = "Personal Rank: --";
        _overviewTab.AddChild(_playerRankLabel);

        _totalPointsLabel = new Label();
        _totalPointsLabel.Text = "Total Points: 1000";
        _overviewTab.AddChild(_totalPointsLabel);

        _winsLabel = new Label();
        _winsLabel.Text = "Wins: 0";
        _overviewTab.AddChild(_winsLabel);

        _lossesLabel = new Label();
        _lossesLabel.Text = "Losses: 0";
        _overviewTab.AddChild(_lossesLabel);

        _drawsLabel = new Label();
        _drawsLabel.Text = "Draws: 0";
        _overviewTab.AddChild(_drawsLabel);

        _winRateLabel = new Label();
        _winRateLabel.Text = "Win Rate: 0%";
        _overviewTab.AddChild(_winRateLabel);

        _currentStreakLabel = new Label();
        _currentStreakLabel.Text = "Current Streak: 0";
        _overviewTab.AddChild(_currentStreakLabel);

        _bestStreakLabel = new Label();
        _bestStreakLabel.Text = "Best Streak: 0";
        _overviewTab.AddChild(_bestStreakLabel);

        AddSeparator(_overviewTab);

        // Server Info
        var serverLabel = new Label();
        serverLabel.Text = "Server Information";
        serverLabel.AddThemeFontSizeOverride("font_size", 18);
        _overviewTab.AddChild(serverLabel);

        _serverInfoLabel = new Label();
        _serverInfoLabel.Text = "Server: Not Connected";
        _overviewTab.AddChild(_serverInfoLabel);

        AddSeparator(_overviewTab);

        // Match Type Selection
        var matchTypeLabel = new Label();
        matchTypeLabel.Text = "Select Match Type:";
        _overviewTab.AddChild(matchTypeLabel);

        _matchTypeOption = new OptionButton();
        var matchTypes = Enum.GetValues(typeof(CrossServerMatchType));
        foreach (var type in matchTypes)
        {
            _matchTypeOption.AddItem(type.ToString());
        }
        _overviewTab.AddChild(_matchTypeOption);

        // Start Match Button
        _startMatchButton = new Button();
        _startMatchButton.Text = "  Find Match  ";
        _startMatchButton.Connect("pressed", this, nameof(OnStartMatchPressed));
        _overviewTab.AddChild(_startMatchButton);
    }

    private void SetupRankingsTab()
    {
        _rankingsTab = new VBoxContainer();
        _rankingsTab.Name = "Rankings";
        _tabContainer.AddChild(_rankingsTab);

        // Ranking Type Selection
        var typeLabel = new Label();
        typeLabel.Text = "Ranking Type:";
        _rankingsTab.AddChild(typeLabel);

        _rankingTypeOption = new OptionButton();
        _rankingTypeOption.AddItem("Player Rankings");
        _rankingTypeOption.AddItem("Server Rankings");
        _rankingTypeOption.Connect("item_selected", this, nameof(OnRankingTypeSelected));
        _rankingsTab.AddChild(_rankingTypeOption);

        AddSeparator(_rankingsTab);

        // Player Rankings Tree
        _playerRankingTree = new Tree();
        _playerRankingTree.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _rankingsTab.AddChild(_playerRankingTree);

        var playerRoot = _playerRankingTree.CreateItem();
        playerRoot.SetText(0, "Player Rankings");
        
        var rankColumn = _playerRankingTree.CreateColumn();
        _playerRankingTree.SetColumnTitle(rankColumn, "Rank");
        _playerRankingTree.SetColumnTitleEnabled(rankColumn, true);
        
        var nameColumn = _playerRankingTree.CreateColumn();
        _playerRankingTree.SetColumnTitle(nameColumn, "Player");
        _playerRankingTree.SetColumnTitleEnabled(nameColumn, true);
        
        var pointsColumn = _playerRankingTree.CreateColumn();
        _playerRankingTree.SetColumnTitle(pointsColumn, "Points");
        _playerRankingTree.SetColumnTitleEnabled(pointsColumn, true);
        
        var winsColumn = _playerRankingTree.CreateColumn();
        _playerRankingTree.SetColumnTitle(winsColumn, "Wins");
        _playerRankingTree.SetColumnTitleEnabled(winsColumn, true);

        // Server Rankings Tree (initially hidden)
        _serverRankingTree = new Tree();
        _serverRankingTree.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _serverRankingTree.Visible = false;
        _rankingsTab.AddChild(_serverRankingTree);

        var serverRoot = _serverRankingTree.CreateItem();
        serverRoot.SetText(0, "Server Rankings");
        
        var serverRankCol = _serverRankingTree.CreateColumn();
        _serverRankingTree.SetColumnTitle(serverRankCol, "Rank");
        _serverRankingTree.SetColumnTitleEnabled(serverRankCol, true);
        
        var serverNameCol = _serverRankingTree.CreateColumn();
        _serverRankingTree.SetColumnTitle(serverNameCol, "Server");
        _serverRankingTree.SetColumnTitleEnabled(serverNameCol, true);
        
        var serverLevelCol = _serverRankingTree.CreateColumn();
        _serverRankingTree.SetColumnTitle(serverLevelCol, "Level");
        _serverRankingTree.SetColumnTitleEnabled(serverLevelCol, true);
        
        var serverPlayersCol = _serverRankingTree.CreateColumn();
        _serverRankingTree.SetColumnTitle(serverPlayersCol, "Players");
        _serverRankingTree.SetColumnTitleEnabled(serverPlayersCol, true);
    }

    private void SetupMatchesTab()
    {
        _matchesTab = new VBoxContainer();
        _matchesTab.Name = "Matches";
        _tabContainer.AddChild(_matchesTab);

        // Refresh Button
        _refreshMatchesButton = new Button();
        _refreshMatchesButton.Text = "  Refresh  ";
        _refreshMatchesButton.Connect("pressed", this, nameof(OnRefreshMatchesPressed));
        _matchesTab.AddChild(_refreshMatchesButton);

        AddSeparator(_matchesTab);

        // Active Matches
        var activeLabel = new Label();
        activeLabel.Text = "Active Matches";
        activeLabel.AddThemeFontSizeOverride("font_size", 18);
        _matchesTab.AddChild(activeLabel);

        _activeMatchesContainer = new VBoxContainer();
        _matchesTab.AddChild(_activeMatchesContainer);

        AddSeparator(_matchesTab);

        // Match History
        var historyLabel = new Label();
        historyLabel.Text = "Match History";
        historyLabel.AddThemeFontSizeOverride("font_size", 18);
        _matchesTab.AddChild(historyLabel);

        _matchHistoryContainer = new VBoxContainer();
        _matchesTab.AddChild(_matchHistoryContainer);
    }

    private void SetupStatisticsTab()
    {
        _statisticsTab = new VBoxContainer();
        _statisticsTab.Name = "Statistics";
        _tabContainer.AddChild(_statisticsTab);

        var titleLabel = new Label();
        titleLabel.Text = "Overall Statistics";
        titleLabel.AddThemeFontSizeOverride("font_size", 20);
        _statisticsTab.AddChild(titleLabel);

        AddSeparator(_statisticsTab);

        _totalMatchesLabel = new Label();
        _totalMatchesLabel.Text = "Total Matches: 0";
        _statisticsTab.AddChild(_totalMatchesLabel);

        _totalWinsLabel = new Label();
        _totalWinsLabel.Text = "Total Wins: 0";
        _statisticsTab.AddChild(_totalWinsLabel);

        _totalLossesLabel = new Label();
        _totalLossesLabel.Text = "Total Losses: 0";
        _statisticsTab.AddChild(_totalLossesLabel);

        _totalDrawsLabel = new Label();
        _totalDrawsLabel.Text = "Total Draws: 0";
        _statisticsTab.AddChild(_totalDrawsLabel);

        AddSeparator(_statisticsTab);

        _bestRankLabel = new Label();
        _bestRankLabel.Text = "Best Ranking: --";
        _statisticsTab.AddChild(_bestRankLabel);

        _highestPointsLabel = new Label();
        _highestPointsLabel.Text = "Highest Points: 0";
        _statisticsTab.AddChild(_highestPointsLabel);

        _bestStreakStatLabel = new Label();
        _bestStreakStatLabel.Text = "Best Streak: 0";
        _statisticsTab.AddChild(_bestStreakStatLabel);

        AddSeparator(_statisticsTab);

        _activeServersLabel = new Label();
        _activeServersLabel.Text = "Active Servers: 0";
        _statisticsTab.AddChild(_activeServersLabel);

        _registeredPlayersLabel = new Label();
        _registeredPlayersLabel.Text = "Registered Players: 0";
        _statisticsTab.AddChild(_registeredPlayersLabel);
    }

    private void AddSeparator(VBoxContainer container)
    {
        var separator = new HSeparator();
        container.AddChild(separator);
    }

    private void RefreshData()
    {
        if (_system == null) return;

        // Update Overview Tab
        var season = _system.Data.CurrentSeason;
        if (season != null)
        {
            _currentSeasonLabel.Text = $"Current Season: {season.SeasonNumber}";
        }

        // Get player record (using placeholder ID for demo)
        var playerRecord = _system.GetPlayerRecord("player_1");
        if (playerRecord != null)
        {
            _playerRankLabel.Text = $"Personal Rank: #{playerRecord.PersonalRank}";
            _totalPointsLabel.Text = $"Total Points: {playerRecord.TotalPoints}";
            _winsLabel.Text = $"Wins: {playerRecord.Wins}";
            _lossesLabel.Text = $"Losses: {playerRecord.Losses}";
            _drawsLabel.Text = $"Draws: {playerRecord.Draws}";
            
            int totalGames = playerRecord.Wins + playerRecord.Losses + playerRecord.Draws;
            float winRate = totalGames > 0 ? (float)playerRecord.Wins / totalGames * 100 : 0;
            _winRateLabel.Text = $"Win Rate: {winRate:F1}%";
            
            _currentStreakLabel.Text = $"Current Streak: {playerRecord.CurrentStreak}";
            _bestStreakLabel.Text = $"Best Streak: {playerRecord.BestStreak}";
        }
        else
        {
            _playerRankLabel.Text = "Personal Rank: --";
            _totalPointsLabel.Text = "Total Points: 1000";
            _winsLabel.Text = "Wins: 0";
            _lossesLabel.Text = "Losses: 0";
            _drawsLabel.Text = "Draws: 0";
            _winRateLabel.Text = "Win Rate: 0%";
            _currentStreakLabel.Text = "Current Streak: 0";
            _bestStreakLabel.Text = "Best Streak: 0";
        }

        // Server Info
        var serverInfo = _system.GetServerInfo("server_1");
        if (serverInfo != null)
        {
            _serverInfoLabel.Text = $"Server: {serverInfo.ServerName} (Rank #{serverInfo.ServerRank})";
        }
        else
        {
            _serverInfoLabel.Text = "Server: No Server Connected";
        }

        // Update Rankings
        UpdateRankingTrees();

        // Update Statistics
        var stats = _system.GetStatistics();
        _totalMatchesLabel.Text = $"Total Matches: {stats["TotalMatches"]}";
        _totalWinsLabel.Text = $"Total Wins: {stats["TotalWins"]}";
        _totalLossesLabel.Text = $"Total Losses: {stats["TotalLosses"]}";
        _totalDrawsLabel.Text = $"Total Draws: {stats["TotalDraws"]}";
        _bestRankLabel.Text = $"Best Ranking: #{stats["BestRanking"]}";
        _highestPointsLabel.Text = $"Highest Points: {stats["HighestPoints"]}";
        _bestStreakStatLabel.Text = $"Best Streak: {stats["BestStreak"]}";
        _activeServersLabel.Text = $"Active Servers: {stats["ActiveServers"]}";
        _registeredPlayersLabel.Text = $"Registered Players: {stats["RegisteredPlayers"]}";
    }

    private void UpdateRankingTrees()
    {
        if (_system == null) return;

        // Clear existing items
        _playerRankingTree.Clear();
        var playerRoot = _playerRankingTree.CreateItem();
        playerRoot.SetText(0, "Player Rankings");

        // Add player rankings
        var topPlayers = _system.GetTopPlayers(20);
        foreach (var player in topPlayers)
        {
            var item = _playerRankingTree.CreateItem(playerRoot);
            item.SetText(0, $"#{player.PersonalRank}");
            item.SetText(1, player.PlayerName);
            item.SetText(2, player.TotalPoints.ToString());
            item.SetText(3, player.Wins.ToString());
        }

        // Clear server rankings
        _serverRankingTree.Clear();
        var serverRoot = _serverRankingTree.CreateItem();
        serverRoot.SetText(0, "Server Rankings");

        // Add server rankings
        var topServers = _system.GetTopServers(20);
        foreach (var server in topServers)
        {
            var item = _serverRankingTree.CreateItem(serverRoot);
            item.SetText(0, $"#{server.ServerRank}");
            item.SetText(1, server.ServerName);
            item.SetText(2, server.ServerLevel.ToString());
            item.SetText(3, server.PlayerCount.ToString());
        }
    }

    private void OnRankingTypeSelected(int index)
    {
        _playerRankingTree.Visible = (index == 0);
        _serverRankingTree.Visible = (index == 1);
    }

    private void OnStartMatchPressed()
    {
        if (_system == null) return;

        var matchType = (CrossServerMatchType)_matchTypeOption.Selected;
        
        // Find matching players
        var matchingPlayers = _system.FindMatchingPlayers("player_1", matchType);
        
        if (matchingPlayers.Count >= CrossServerBattleDatabase.Instance.GetMatchTypeConfig(matchType).TeamSize - 1)
        {
            var team1 = new List<string> { "player_1" };
            var team2 = matchingPlayers;
            
            var match = _system.CreateMatch(matchType, team1, team2, "server_1", "server_2");
            
            GD.Print($"[CrossServerBattleUI] Match started: {match.MatchId}");
            RefreshData();
        }
        else
        {
            GD.Print("[CrossServerBattleUI] Not enough players to start match");
        }
    }

    private void OnRefreshMatchesPressed()
    {
        RefreshData();
    }

    private void OnCloseButtonPressed()
    {
        Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            Visible = false;
        }
    }

    public void ToggleVisibility()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshData();
        }
    }
}
