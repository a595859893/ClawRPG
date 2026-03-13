using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ArenaTournamentUI : Control
{
    private ArenaTournamentSystem _system;
    private TabContainer _tabContainer;
    private VBoxContainer _overviewTab;
    private VBoxContainer _participantsTab;
    private VBoxContainer _matchesTab;
    private VBoxContainer _statisticsTab;
    
    // 锦标赛控制
    private LineEdit _nameInput;
    private OptionButton _typeSelector;
    private OptionButton _difficultySelector;
    private Button _createButton;
    private Button _startSeedingButton;
    private Button _startTournamentButton;
    
    // 状态显示
    private Label _stateLabel;
    private Label _roundLabel;
    private Label _participantsLabel;
    
    // 列表
    private ItemList _participantsList;
    private ItemList _matchesList;
    private ItemList _rankingsList;
    private ItemList _historyList;
    
    // 统计
    private Label _statsLabel;
    
    public ArenaTournamentUI()
    {
        _system = new ArenaTournamentSystem();
    }
    
    public override void _Ready()
    {
        SetupUI();
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainVBox.AddThemeConstantOverride("separation", 10);
        AddChild(mainVBox);
        
        // 标题
        var title = new Label();
        title.Text = "🏟️ Arena Tournament";
        title.Align = Label.AlignEnum.Center;
        title.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(title);
        
        // 状态栏
        var statusBar = new HBoxContainer();
        mainVBox.AddChild(statusBar);
        
        _stateLabel = new Label();
        _stateLabel.Text = "State: Registration";
        statusBar.AddChild(_stateLabel);
        
        statusBar.AddChild(new Label { Text = "  |  " });
        
        _roundLabel = new Label();
        _roundLabel.Text = "Round: 0/0";
        statusBar.AddChild(_roundLabel);
        
        statusBar.AddChild(new Label { Text = "  |  " });
        
        _participantsLabel = new Label();
        _participantsLabel.Text = "Participants: 0/16";
        statusBar.AddChild(_participantsLabel);
        
        // Tab 容器
        _tabContainer = new TabContainer();
        _tabContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlagsVertical);
        mainVBox.AddChild(_tabContainer);
        
        // Overview Tab
        _overviewTab = new VBoxContainer();
        _overviewTab.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(_overviewTab);
        _tabContainer.SetTabTitle(_overviewTab, "Overview");
        SetupOverviewTab();
        
        // Participants Tab
        _participantsTab = new VBoxContainer();
        _participantsTab.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(_participantsTab);
        _tabContainer.SetTabTitle(_participantsTab, "Participants");
        SetupParticipantsTab();
        
        // Matches Tab
        _matchesTab = new VBoxContainer();
        _matchesTab.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(_matchesTab);
        _tabContainer.SetTabTitle(_matchesTab, "Matches");
        SetupMatchesTab();
        
        // Statistics Tab
        _statisticsTab = new VBoxContainer();
        _statisticsTab.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(_statisticsTab);
        _tabContainer.SetTabTitle(_statisticsTab, "Statistics");
        SetupStatisticsTab();
        
        UpdateUI();
    }
    
    private void SetupOverviewTab()
    {
        // 创建锦标赛区域
        var createSection = new VBoxContainer();
        createSection.AddThemeConstantOverride("separation", 5);
        _overviewTab.AddChild(createSection);
        
        var createLabel = new Label();
        createLabel.Text = "Create Tournament";
        createLabel.AddThemeFontSizeOverride("font_size", 18);
        createSection.AddChild(createLabel);
        
        // 名称输入
        var nameRow = new HBoxContainer();
        createSection.AddChild(nameRow);
        nameRow.AddChild(new Label { Text = "Name: " });
        _nameInput = new LineEdit();
        _nameInput.PlaceholderText = "Tournament Name";
        _nameInput.Text = "Arena Championship";
        _nameInput.CustomMinimumSize = new Vector2(200, 0);
        nameRow.AddChild(_nameInput);
        
        // 类型选择
        var typeRow = new HBoxContainer();
        createSection.AddChild(typeRow);
        typeRow.AddChild(new Label { Text = "Type: " });
        _typeSelector = new OptionButton();
        _typeSelector.AddItem("Single Elimination", (int)ArenaTournamentType.SingleElimination);
        _typeSelector.AddItem("Double Elimination", (int)ArenaTournamentType.DoubleElimination);
        _typeSelector.AddItem("Round Robin", (int)ArenaTournamentType.RoundRobin);
        _typeSelector.AddItem("Swiss System", (int)ArenaTournamentType.Swiss);
        _typeSelector.Select(0);
        typeRow.AddChild(_typeSelector);
        
        // 难度选择
        var diffRow = new HBoxContainer();
        createSection.AddChild(diffRow);
        diffRow.AddChild(new Label { Text = "Difficulty: " });
        _difficultySelector = new OptionButton();
        _difficultySelector.AddItem("Easy", 0);
        _difficultySelector.AddItem("Normal", 1);
        _difficultySelector.AddItem("Hard", 2);
        _difficultySelector.AddItem("Nightmare", 3);
        _difficultySelector.AddItem("Legendary", 4);
        _difficultySelector.Select(1);
        diffRow.AddChild(_difficultySelector);
        
        // 按钮行
        var buttonRow = new HBoxContainer();
        buttonRow.AddThemeConstantOverride("separation", 10);
        _overviewTab.AddChild(buttonRow);
        
        _createButton = new Button();
        _createButton.Text = "Create";
        _createButton.Pressed += OnCreatePressed;
        buttonRow.AddChild(_createButton);
        
        _startSeedingButton = new Button();
        _startSeedingButton.Text = "Start Seeding";
        _startSeedingButton.Pressed += OnStartSeedingPressed;
        _startSeedingButton.Disabled = true;
        buttonRow.AddChild(_startSeedingButton);
        
        _startTournamentButton = new Button();
        _startTournamentButton.Text = "Start Tournament";
        _startTournamentButton.Pressed += OnStartTournamentPressed;
        _startTournamentButton.Disabled = true;
        buttonRow.AddChild(_startTournamentButton);
        
        // 排行榜
        var rankingsLabel = new Label();
        rankingsLabel.Text = "Current Rankings";
        rankingsLabel.AddThemeFontSizeOverride("font_size", 18);
        _overviewTab.AddChild(rankingsLabel);
        
        _rankingsList = new ItemList();
        _rankingsList.CustomMinimumSize = new Vector2(0, 200);
        _rankingsList.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlagsVertical);
        _overviewTab.AddChild(_rankingsList);
    }
    
    private void SetupParticipantsTab()
    {
        // 注册选手区域
        var registerSection = new VBoxContainer();
        registerSection.AddThemeConstantOverride("separation", 5);
        _participantsTab.AddChild(registerSection);
        
        var registerLabel = new Label();
        registerLabel.Text = "Register Participant";
        registerLabel.AddThemeFontSizeOverride("font_size", 16);
        registerSection.AddChild(registerLabel);
        
        var registerButton = new Button();
        registerButton.Text = "Register Current Player";
        registerButton.Pressed += OnRegisterPlayerPressed;
        registerSection.AddChild(registerButton);
        
        // 参赛选手列表
        var listLabel = new Label();
        listLabel.Text = "Registered Participants";
        listLabel.AddThemeFontSizeOverride("font_size", 16);
        _participantsTab.AddChild(listLabel);
        
        _participantsList = new ItemList();
        _participantsList.CustomMinimumSize = new Vector2(0, 300);
        _participantsList.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlagsVertical);
        _participantsTab.AddChild(_participantsList);
    }
    
    private void SetupMatchesTab()
    {
        // 当前比赛
        var matchesLabel = new Label();
        matchesLabel.Text = "Current Matches";
        matchesLabel.AddThemeFontSizeOverride("font_size", 16);
        _matchesTab.AddChild(matchesLabel);
        
        _matchesList = new ItemList();
        _matchesList.CustomMinimumSize = new Vector2(0, 250);
        _matchesList.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlagsVertical);
        _matchesTab.AddChild(_matchesList);
        
        // 完成比赛按钮
        var completeButton = new Button();
        completeButton.Text = "Simulate Match Completion";
        completeButton.Pressed += OnSimulateMatchPressed;
        _matchesTab.AddChild(completeButton);
        
        // 历史记录
        var historyLabel = new Label();
        historyLabel.Text = "Tournament History";
        historyLabel.AddThemeFontSizeOverride("font_size", 16);
        _matchesTab.AddChild(historyLabel);
        
        _historyList = new ItemList();
        _historyList.CustomMinimumSize = new Vector2(0, 150);
        _historyList.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlagsVertical);
        _matchesTab.AddChild(_historyList);
    }
    
    private void SetupStatisticsTab()
    {
        var statsTitle = new Label();
        statsTitle.Text = "Tournament Statistics";
        statsTitle.AddThemeFontSizeOverride("font_size", 20);
        _statisticsTab.AddChild(statsTitle);
        
        _statsLabel = new Label();
        _statsLabel.Text = "No statistics yet";
        _statisticsTab.AddChild(_statsLabel);
        
        var refreshButton = new Button();
        refreshButton.Text = "Refresh Statistics";
        refreshButton.Pressed += OnRefreshStatsPressed;
        _statisticsTab.AddChild(refreshButton);
    }
    
    private void OnCreatePressed()
    {
        string name = _nameInput.Text;
        var type = (ArenaTournamentType)_typeSelector.GetSelectedId();
        
        if (_system.CreateTournament(name, type))
        {
            GD.Print("Tournament created: " + name);
            UpdateUI();
        }
    }
    
    private void OnStartSeedingPressed()
    {
        if (_system.GetData().StartSeeding())
        {
            GD.Print("Seeding started");
            UpdateUI();
        }
    }
    
    private void OnStartTournamentPressed()
    {
        if (_system.GetData().StartTournament())
        {
            GD.Print("Tournament started");
            UpdateUI();
        }
    }
    
    private void OnRegisterPlayerPressed()
    {
        var data = _system.GetData();
        int playerId = 1; // 模拟玩家ID
        string playerName = "Player";
        
        if (_system.RegisterParticipant(playerId, playerName))
        {
            GD.Print("Player registered");
            UpdateUI();
        }
    }
    
    private void OnSimulateMatchPressed()
    {
        var matches = _system.GetCurrentRoundMatches();
        var pendingMatch = matches.FirstOrDefault(m => !m.IsCompleted && m.Player1Id >= 0 && m.Player2Id >= 0);
        
        if (pendingMatch != null)
        {
            var random = new Random();
            int score1 = random.Next(0, 5);
            int score2 = random.Next(0, 5);
            _system.CompleteMatch(pendingMatch.MatchId, score1, score2);
            GD.Print($"Match completed: {score1} - {score2}");
            UpdateUI();
        }
    }
    
    private void OnRefreshStatsPressed()
    {
        var stats = _system.GetStatistics();
        _statsLabel.Text = $"Total Tournaments: {stats["total_tournaments"]}\n" +
            $"Tournaments Won: {stats["tournaments_won"]}\n" +
            $"Matches Played: {stats["total_matches"]}\n" +
            $"Wins: {stats["total_wins"]} | Losses: {stats["total_losses"]} | Draws: {stats["total_draws"]}";
    }
    
    private void UpdateUI()
    {
        var data = _system.GetData();
        
        // 更新状态
        _stateLabel.Text = $"State: {data.State}";
        _roundLabel.Text = $"Round: {data.CurrentRound}/{data.TotalRounds}";
        _participantsLabel.Text = $"Participants: {data.Participants.Count}/{data.MaxParticipants}";
        
        // 更新按钮状态
        _createButton.Disabled = data.State != ArenaTournamentState.Registration && 
                                  data.State != ArenaTournamentState.Completed &&
                                  data.State != ArenaTournamentState.Cancelled;
        _startSeedingButton.Disabled = data.State != ArenaTournamentState.Registration || 
                                        data.Participants.Count < data.MinParticipants;
        _startTournamentButton.Disabled = data.State != ArenaTournamentState.Seeding;
        
        // 更新参赛选手列表
        _participantsList.Clear();
        foreach (var p in data.Participants)
        {
            _participantsList.AddItem($"#{p.Seed} {p.Name} - Points: {p.Points}");
        }
        
        // 更新排行榜
        _rankingsList.Clear();
        var rankings = _system.GetRankings();
        foreach (var p in rankings)
        {
            string status = p.IsEliminated ? " (Eliminated)" : "";
            _rankingsList.AddItem($"#{p.Placement} {p.Name} - {p.Wins}W/{p.Losses}L/{p.Draws}D{status}");
        }
        
        // 更新比赛列表
        _matchesList.Clear();
        var currentMatches = _system.GetCurrentRoundMatches();
        foreach (var m in currentMatches)
        {
            string status = m.IsCompleted ? "✓" : "○";
            string matchInfo = $"Round {m.Round}: Player{m.Player1Id} vs Player{m.Player2Id} ({m.Player1Score}-{m.Player2Score}) {status}";
            _matchesList.AddItem(matchInfo);
        }
        
        // 更新历史
        _historyList.Clear();
        foreach (var h in data.History.Take(10))
        {
            _historyList.AddItem($"#{h.Placement} {h.TournamentName} - {h.Participants} players - {h.Reward}g");
        }
    }
    
    public void Toggle()
    {
        if (Visible)
        {
            Hide();
        }
        else
        {
            Show();
            UpdateUI();
        }
    }
}
