using System;
using System.Collections.Generic;
using Godot;

public partial class MythicPlusDungeonUI : Control
{
    private static MythicPlusDungeonUI _instance;
    public static MythicPlusDungeonUI Instance => _instance;
    
    // UI Components
    private TabContainer _tabContainer;
    private VBoxContainer _runContainer;
    private VBoxContainer _historyContainer;
    private VBoxContainer _leaderboardContainer;
    private VBoxContainer _statsContainer;
    
    // Run UI
    private OptionButton _dungeonSelect;
    private SpinBox _levelSpin;
    private Button _startButton;
    private Label _currentRunInfo;
    private Label _timerLabel;
    private Label _affixLabel;
    private Label _enemyKillLabel;
    private Label _bossKillLabel;
    private Button _completeButton;
    private Button _failButton;
    
    // Stats
    private Label _bestLevelLabel;
    private Label _totalRunsLabel;
    private Label _completionRateLabel;
    private Label _highestScoreLabel;
    private Label _avgTimeLabel;
    private Label _weeklyAffixesLabel;
    
    private bool _isVisible = false;
    private int _timerSeconds = 0;
    
    public override void _Ready()
    {
        _instance = this;
        SetupUI();
        Hide();
    }
    
    private void SetupUI()
    {
        // Main Panel
        var mainPanel = new PanelContainer
        {
            Name = "MainPanel",
            AnchorRight = Vector2.One,
            AnchorBottom = Vector2.One,
            Margin = new Margin(50, 50, 50, 50)
        };
        AddChild(mainPanel);
        
        var mainStyle = new StyleBoxFlat();
        mainStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        mainStyle.BorderColor = new Color(0.8f, 0.6f, 0.2f, 1f);
        mainStyle.SetBorderWidthAll(3);
        mainStyle.SetCornerRadiusAll(8);
        mainPanel.AddThemeStyleboxOverride("panel", mainStyle);
        
        // Title
        var titleLabel = new Label
        {
            Text = "⚔️ Mythic+ Dungeon System",
            AnchorRight = Vector2.One,
            AnchorBottom = Vector2.One,
            Margin = new Margin(20, 15, 20, 0),
            Align = Label.AlignEnum.Center,
            Valign = Label.VAlign.Top
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 28);
        titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f, 1f));
        mainPanel.AddChild(titleLabel);
        
        // Tab Container
        _tabContainer = new TabContainer
        {
            AnchorRight = Vector2.One,
            AnchorBottom = Vector2.One,
            Margin = new Margin(20, 60, 20, 20)
        };
        mainPanel.AddChild(_tabContainer);
        
        // Setup tabs
        SetupRunTab();
        SetupHistoryTab();
        SetupLeaderboardTab();
        SetupStatsTab();
        
        // Close button
        var closeButton = new Button
        {
            Text = "✕ Close",
            AnchorLeft = 1f,
            AnchorRight = 1f,
            AnchorTop = 0f,
            AnchorBottom = 0f,
            Margin = new Margin(-120, -40, -20, -10),
            RectMinSize = new Vector2(100, 30)
        };
        closeButton.Pressed += OnCloseButtonPressed;
        mainPanel.AddChild(closeButton);
    }
    
    private void SetupRunTab()
    {
        _runContainer = new VBoxContainer();
        _runContainer.Name = "Run";
        _tabContainer.AddChild(_runContainer);
        
        var scroll = new ScrollContainer();
        scroll.Name = "Scroll";
        _runContainer.AddChild(scroll);
        
        var content = new VBoxContainer();
        content.Name = "Content";
        content.RectMinSize = new Vector2(400, 500);
        scroll.AddChild(content);
        
        // Dungeon Selection
        var dungeonLabel = new Label { Text = "🏰 Select Dungeon:" };
        dungeonLabel.AddThemeFontSizeOverride("font_size", 18);
        content.AddChild(dungeonLabel);
        
        _dungeonSelect = new OptionButton();
        _dungeonSelect.RectMinSize = new Vector2(300, 40);
        
        var dungeons = MythicPlusDungeonDatabase.GetAllDungeons();
        foreach (var dungeon in dungeons)
        {
            _dungeonSelect.AddItem($"{dungeon.Name} (ilvl {dungeon.RecommendedItemLevel})", dungeons.IndexOf(dungeon));
        }
        content.AddChild(_dungeonSelect);
        
        // Level Selection
        var levelLabel = new Label { Text = "📊 Mythic Level:" };
        levelLabel.AddThemeFontSizeOverride("font_size", 18);
        content.AddChild(levelLabel);
        
        _levelSpin = new SpinBox();
        _levelSpin.RectMinSize = new Vector2(200, 40);
        _levelSpin.MinValue = 0;
        _levelSpin.MaxValue = 30;
        _levelSpin.Value = 0;
        _levelSpin.Step = 1;
        content.AddChild(_levelSpin);
        
        // Start Button
        _startButton = new Button
        {
            Text = "⚔️ Start Mythic+ Run",
            RectMinSize = new Vector2(300, 50)
        };
        _startButton.AddThemeFontSizeOverride("font_size", 22);
        _startButton.Pressed += OnStartButtonPressed;
        content.AddChild(_startButton);
        
        // Separator
        content.AddChild(new Control { RectMinSize = new Vector2(0, 20) });
        
        // Current Run Info
        var currentLabel = new Label { Text = "📋 Current Run:" };
        currentLabel.AddThemeFontSizeOverride("font_size", 20);
        content.AddChild(currentLabel);
        
        _currentRunInfo = new Label { Text = "No active run", Align = Label.AlignEnum.Center };
        _currentRunInfo.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1f));
        content.AddChild(_currentRunInfo);
        
        _timerLabel = new Label { Text = "⏱️ Time: 00:00" };
        _timerLabel.AddThemeFontSizeOverride("font_size", 24);
        _timerLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.3f, 1f));
        content.AddChild(_timerLabel);
        
        _affixLabel = new Label { Text = "✨ Affixes: None" };
        _affixLabel.AddThemeFontSizeOverride("font_size", 16);
        _affixLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f, 1f));
        content.AddChild(_affixLabel);
        
        _enemyKillLabel = new Label { Text = "💀 Enemies: 0" };
        content.AddChild(_enemyKillLabel);
        
        _bossLabel = new Label { Text = "👹 Bosses: 0" };
        content.AddChild(_bossLabel);
        
        // Separator
        content.AddChild(new Control { RectMinSize = new Vector2(0, 20) });
        
        // Complete/Fail Buttons
        var buttonContainer = new HBoxContainer();
        content.AddChild(buttonContainer);
        
        _completeButton = new Button
        {
            Text = "✅ Complete Run",
            RectMinSize = new Vector2(150, 40),
            Disabled = true
        };
        _completeButton.Pressed += OnCompleteButtonPressed;
        buttonContainer.AddChild(_completeButton);
        
        _failButton = new Button
        {
            Text = "❌ Fail Run",
            RectMinSize = new Vector2(150, 40),
            Disabled = true
        };
        _failButton.Pressed += OnFailButtonPressed;
        buttonContainer.AddChild(_failButton);
    }
    
    private Label _bossLabel;
    
    private void SetupHistoryTab()
    {
        _historyContainer = new VBoxContainer();
        _historyContainer.Name = "History";
        _tabContainer.AddChild(_historyContainer);
        
        var scroll = new ScrollContainer();
        _historyContainer.AddChild(scroll);
        
        var content = new VBoxContainer();
        content.Name = "Content";
        content.RectMinSize = new Vector2(400, 500);
        scroll.AddChild(content);
        
        var title = new Label { Text = "📜 Recent Runs", Align = Label.AlignEnum.Center };
        title.AddThemeFontSizeOverride("font_size", 22);
        content.AddChild(title);
        
        var separator = new HSeparator();
        content.AddChild(separator);
        
        RefreshHistoryList();
    }
    
    private void SetupLeaderboardTab()
    {
        _leaderboardContainer = new VBoxContainer();
        _leaderboardContainer.Name = "Leaderboard";
        _tabContainer.AddChild(_leaderboardContainer);
        
        var scroll = new ScrollContainer();
        _leaderboardContainer.AddChild(scroll);
        
        var content = new VBoxContainer();
        content.Name = "Content";
        content.RectMinSize = new Vector2(400, 500);
        scroll.AddChild(content);
        
        var title = new Label { Text = "🏆 Leaderboard", Align = Label.AlignEnum.Center };
        title.AddThemeFontSizeOverride("font_size", 22);
        content.AddChild(title);
        
        var separator = new HSeparator();
        content.AddChild(separator);
        
        RefreshLeaderboardList();
    }
    
    private void SetupStatsTab()
    {
        _statsContainer = new VBoxContainer();
        _statsContainer.Name = "Stats";
        _tabContainer.AddChild(_statsContainer);
        
        var scroll = new ScrollContainer();
        _statsContainer.AddChild(scroll);
        
        var content = new VBoxContainer();
        content.Name = "Content";
        content.RectMinSize = new Vector2(400, 500);
        scroll.AddChild(content);
        
        var title = new Label { Text = "📊 Your Statistics", Align = Label.AlignEnum.Center };
        title.AddThemeFontSizeOverride("font_size", 22);
        content.AddChild(title);
        
        var separator = new HSeparator();
        content.AddChild(separator);
        
        _bestLevelLabel = new Label { Text = "Best Level: 0" };
        content.AddChild(_bestLevelLabel);
        
        _totalRunsLabel = new Label { Text = "Total Runs: 0" };
        content.AddChild(_totalRunsLabel);
        
        _completionRateLabel = new Label { Text = "Completion Rate: 0%" };
        content.AddChild(_completionRateLabel);
        
        _highestScoreLabel = new Label { Text = "Highest Score: 0" };
        content.AddChild(_highestScoreLabel);
        
        _avgTimeLabel = new Label { Text = "Average Time: 0s" };
        content.AddChild(_avgTimeLabel);
        
        var separator2 = new HSeparator();
        content.AddChild(separator2);
        
        _weeklyAffixesLabel = new Label { Text = "Weekly Affixes: Loading..." };
        _weeklyAffixesLabel.AddThemeColorOverride("font_color", new Color(1f, 0.7f, 0.3f, 1f));
        content.AddChild(_weeklyAffixesLabel);
        
        RefreshStats();
    }
    
    private void RefreshHistoryList()
    {
        // Clear existing items (except title and separator)
        foreach (Node child in _historyContainer.GetChild<ScrollContainer>()?.GetChild<VBoxContainer>())
        {
            if (child is Label || child is HSeparator) continue;
            child.QueueFree();
        }
        
        var history = MythicPlusDungeonSystem.Instance.GetRecentRuns(20);
        var container = _historyContainer.GetChild<ScrollContainer>()?.GetChild<VBoxContainer>();
        if (container == null) return;
        
        foreach (var run in history)
        {
            var color = run.Completed ? new Color(0.3f, 0.9f, 0.3f) : new Color(0.9f, 0.3f, 0.3f);
            var status = run.Completed ? "✅" : "❌";
            
            var runLabel = new Label
            {
                Text = $"{status} Level {run.DungeonLevel} | Score: {run.Score} | Time: {run.CompletedTimeSeconds}s | Kills: {run.EnemiesKilled} | Deaths: {run.Deaths}",
                RectMinSize = new Vector2(0, 35)
            };
            runLabel.AddThemeColorOverride("font_color", color);
            container.AddChild(runLabel);
        }
    }
    
    private void RefreshLeaderboardList()
    {
        var container = _leaderboardContainer.GetChild<ScrollContainer>()?.GetChild<VBoxContainer>();
        if (container == null) return;
        
        // Clear existing
        foreach (Node child in container)
        {
            if (child is Label || child is HSeparator) continue;
            child.QueueFree();
        }
        
        var leaderboard = MythicPlusDungeonSystem.Instance.GetLeaderboard(20);
        
        int rank = 1;
        foreach (var entry in leaderboard)
        {
            var medal = rank == 1 ? "🥇" : rank == 2 ? "🥈" : rank == 3 ? "🥉" : "  ";
            
            var entryLabel = new Label
            {
                Text = $"{medal} #{rank} {entry.PlayerName} | Level {entry.Level} | Score: {entry.Score} | Time: {entry.TimeSeconds}s",
                RectMinSize = new Vector2(0, 35)
            };
            
            if (rank == 1) entryLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f, 1f));
            else if (rank == 2) entryLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1f));
            else if (rank == 3) entryLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.5f, 0.3f, 1f));
            
            container.AddChild(entryLabel);
            rank++;
        }
    }
    
    private void RefreshStats()
    {
        var stats = MythicPlusDungeonSystem.Instance.GetDetailedStats();
        
        _bestLevelLabel.Text = $"Best Level: {stats["best_level"]}";
        _totalRunsLabel.Text = $"Total Runs: {stats["total_runs"]} (Completed: {stats["completed_runs"]}, Failed: {stats["failed_runs"]})";
        _completionRateLabel.Text = $"Completion Rate: {stats["completion_rate"]}%";
        _highestScoreLabel.Text = $"Highest Score: {stats["highest_score"]}";
        _avgTimeLabel.Text = $"Average Time: {stats["average_time"]}s";
        
        var affixes = MythicPlusDungeonSystem.Instance.GetCurrentAffixes();
        _weeklyAffixesLabel.Text = $"Weekly Affixes: {affixes.Name}";
    }
    
    private void UpdateCurrentRunUI()
    {
        var run = MythicPlusDungeonSystem.Instance.GetCurrentRun();
        
        if (run == null || run.Completed || run.Failed)
        {
            _currentRunInfo.Text = "No active run";
            _timerLabel.Text = "⏱️ Time: 00:00";
            _affixLabel.Text = "✨ Affixes: None";
            _enemyKillLabel.Text = "💀 Enemies: 0";
            _bossLabel.Text = "👹 Bosses: 0";
            _timerSeconds = 0;
            _startButton.Disabled = false;
            _completeButton.Disabled = true;
            _failButton.Disabled = true;
            return;
        }
        
        _startButton.Disabled = true;
        _completeButton.Disabled = false;
        _failButton.Disabled = false;
        
        _currentRunInfo.Text = $"Level {run.DungeonLevel} Mythic+";
        _enemyKillLabel.Text = $"💀 Enemies: {run.EnemiesKilled}";
        _bossLabel.Text = $"👹 Bosses: {run.BossesDefeated}";
        
        // Update timer
        if (run.StartTime != null)
        {
            var elapsed = (int)(DateTime.UtcNow - run.StartTime).TotalSeconds;
            var minutes = elapsed / 60;
            var seconds = elapsed % 60;
            _timerLabel.Text = $"⏱️ Time: {minutes:D2}:{seconds:D2}";
        }
        
        // Update affixes
        if (run.ActiveAffixes.Count > 0)
        {
            var affixNames = string.Join(", ", run.ActiveAffixes);
            _affixLabel.Text = $"✨ Affixes: {affixNames}";
        }
    }
    
    #region Signal Handlers
    
    private void OnStartButtonPressed()
    {
        var dungeonIndex = _dungeonSelect.Selected;
        var level = (int)_levelSpin.Value;
        
        if (dungeonIndex < 0)
        {
            GD.Warning("[MythicPlusUI] Please select a dungeon");
            return;
        }
        
        var dungeons = MythicPlusDungeonDatabase.GetAllDungeons();
        if (dungeonIndex >= dungeons.Count)
        {
            GD.Warning("[MythicPlusUI] Invalid dungeon selection");
            return;
        }
        
        var dungeon = dungeons[dungeonIndex];
        
        // Check item level requirement (simplified)
        var player = PlayerData.Instance;
        if (player != null && player.GetStat("item_level") < dungeon.MinItemLevel)
        {
            GD.Warning($"[MythicPlusUI] Item level too low. Required: {dungeon.MinItemLevel}");
            return;
        }
        
        MythicPlusDungeonSystem.Instance.StartRun(dungeon.DungeonId, level);
        UpdateCurrentRunUI();
        
        GD.Print($"[MythicPlusUI] Started run - {dungeon.Name} Level {level}");
    }
    
    private void OnCompleteButtonPressed()
    {
        var result = MythicPlusDungeonSystem.Instance.CompleteRun(true);
        
        if (result != null)
        {
            GD.Print($"[MythicPlusUI] Run completed! Score: {result.Score}, Gold: {result.RewardGold}, XP: {result.RewardExp}");
            
            // Show reward message
            var rewardMsg = $"🎉 Run Complete!\nScore: {result.Score}\nGold: {result.RewardGold}\nXP: {result.RewardExp}";
            
            if (result.RewardItems.Count > 0)
            {
                rewardMsg += "\nItems: " + string.Join(", ", result.RewardItems);
            }
            
            GD.Print(rewardMsg);
        }
        
        UpdateCurrentRunUI();
        RefreshHistoryList();
        RefreshStats();
    }
    
    private void OnFailButtonPressed()
    {
        var result = MythicPlusDungeonSystem.Instance.CompleteRun(false);
        
        if (result != null)
        {
            GD.Print($"[MythicPlusUI] Run failed - Level {result.DungeonLevel}, Kills: {result.EnemiesKilled}");
        }
        
        UpdateCurrentRunUI();
        RefreshHistoryList();
        RefreshStats();
    }
    
    private void OnCloseButtonPressed()
    {
        Toggle();
    }
    
    #endregion
    
    public void Toggle()
    {
        _isVisible = !_isVisible;
        
        if (_isVisible)
        {
            Show();
            RefreshStats();
            RefreshHistoryList();
            RefreshLeaderboardList();
        }
        else
        {
            Hide();
        }
    }
    
    public override void _Process(double delta)
    {
        if (_isVisible && _tabContainer?.GetCurrentTab() == 0)
        {
            UpdateCurrentRunUI();
        }
    }
}
