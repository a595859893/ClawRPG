using Godot;
using System;
using System.Collections.Generic;

public class BossRushUI : Control
{
    private BossRushSystem bossRushSystem;
    private Control mainContainer;
    private Label titleLabel;
    private Label statusLabel;
    private Label stageLabel;
    private Label streakLabel;
    private Label healthLabel;
    private ProgressBar healthBar;
    private VBoxContainer rewardsContainer;
    private VBoxContainer historyContainer;
    private VBoxContainer statsContainer;
    
    // Tab buttons
    private Button rushTabBtn;
    private Button historyTabBtn;
    private Button statsTabBtn;
    
    // Rush UI elements
    private Control rushPanel;
    private OptionButton difficultyOption;
    private Button startButton;
    private Button advanceButton;
    private Button quitButton;
    private Button pauseButton;
    
    private int currentTab = 0;
    
    public override void _Ready()
    {
        bossRushSystem = GetNode<BossRushSystem>("/root/Main/BossRushSystem");
        
        SetupUI();
        ConnectSignals();
        UpdateUI();
    }
    
    private void SetupUI()
    {
        // Main container
        mainContainer = new Control();
        mainContainer.SetAnchor(AnchorPreset.FullRect);
        AddChild(mainContainer);
        
        // Background
        var bg = new ColorRect();
        bg.Color = new Color(0, 0, 0, 0.8f);
        bg.SetAnchor(AnchorPreset.FullRect);
        mainContainer.AddChild(bg);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = "⚔️ Boss Rush ⚔️";
        titleLabel.SetAnchor(AnchorPreset.TopWide);
        titleLabel.AddThemeFontSizeOverride("font_size", 32);
        titleLabel.Position = new Vector2(0, 20);
        mainContainer.AddChild(titleLabel);
        
        // Tab buttons
        var tabContainer = new HBoxContainer();
        tabContainer.SetAnchor(AnchorPreset.TopWide);
        tabContainer.Position = new Vector2(20, 70);
        tabContainer.Size = new Vector2(760, 40);
        mainContainer.AddChild(tabContainer);
        
        rushTabBtn = new Button();
        rushTabBtn.Text = "  Rush  ";
        rushTabBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        tabContainer.AddChild(rushTabBtn);
        
        historyTabBtn = new Button();
        historyTabBtn.Text = " History ";
        historyTabBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        tabContainer.AddChild(historyTabBtn);
        
        statsTabBtn = new Button();
        statsTabBtn.Text = " Stats ";
        statsTabBtn.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        tabContainer.AddChild(statsTabBtn);
        
        // Content panels
        SetupRushPanel();
        SetupHistoryPanel();
        SetupStatsPanel();
    }
    
    private void SetupRushPanel()
    {
        rushPanel = new Control();
        rushPanel.SetAnchor(AnchorPreset.FullRect);
        rushPanel.Position = new Vector2(0, 120);
        rushPanel.Size = new Vector2(800, 480);
        mainContainer.AddChild(rushPanel);
        
        // Status label
        statusLabel = new Label();
        statusLabel.Text = "Not Started";
        statusLabel.AddThemeFontSizeOverride("font_size", 24);
        statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statusLabel.SetAnchor(AnchorPreset.TopWide);
        statusLabel.Position = new Vector2(0, 20);
        rushPanel.AddChild(statusLabel);
        
        // Stage info
        stageLabel = new Label();
        stageLabel.Text = "Stage: 1";
        stageLabel.AddThemeFontSizeOverride("font_size", 20);
        stageLabel.SetAnchor(AnchorPreset.TopWide);
        stageLabel.Position = new Vector2(0, 70);
        rushPanel.AddChild(stageLabel);
        
        // Streak info
        streakLabel = new Label();
        streakLabel.Text = "Current Streak: 0 | Best Streak: 0";
        streakLabel.AddThemeFontSizeOverride("font_size", 18);
        streakLabel.SetAnchor(AnchorPreset.TopWide);
        streakLabel.Position = new Vector2(0, 110);
        rushPanel.AddChild(streakLabel);
        
        // Health bar
        var healthTitle = new Label();
        healthTitle.Text = "Health:";
        healthTitle.SetAnchor(AnchorPreset.TopWide);
        healthTitle.Position = new Vector2(200, 160);
        rushPanel.AddChild(healthTitle);
        
        healthBar = new ProgressBar();
        healthBar.SetAnchor(AnchorPreset.TopWide);
        healthBar.Position = new Vector2(200, 190);
        healthBar.Size = new Vector2(400, 30);
        healthBar.MaxValue = 100;
        healthBar.Value = 100;
        healthBar.ShowPercentage = false;
        rushPanel.AddChild(healthBar);
        
        healthLabel = new Label();
        healthLabel.Text = "100%";
        healthLabel.SetAnchor(AnchorPreset.TopWide);
        healthLabel.Position = new Vector2(350, 165);
        rushPanel.AddChild(healthLabel);
        
        // Difficulty selection
        var diffLabel = new Label();
        diffLabel.Text = "Difficulty:";
        diffLabel.Position = new Vector2(250, 260);
        rushPanel.AddChild(diffLabel);
        
        difficultyOption = new OptionButton();
        difficultyOption.Position = new Vector2(350, 255);
        difficultyOption.Size = new Vector2(150, 30);
        difficultyOption.AddItem("Easy");
        difficultyOption.AddItem("Normal");
        difficultyOption.AddItem("Hard");
        difficultyOption.AddItem("Nightmare");
        difficultyOption.AddItem("Legendary");
        difficultyOption.Selected = 1; // Normal default
        rushPanel.AddChild(difficultyOption);
        
        // Action buttons
        startButton = new Button();
        startButton.Text = "  Start Rush  ";
        startButton.Position = new Vector2(250, 320);
        startButton.Size = new Vector2(300, 50);
        rushPanel.AddChild(startButton);
        
        advanceButton = new Button();
        advanceButton.Text = "  Next Boss  ";
        advanceButton.Position = new Vector2(250, 380);
        advanceButton.Size = new Vector2(300, 50);
        advanceButton.Disabled = true;
        rushPanel.AddChild(advanceButton);
        
        quitButton = new Button();
        quitButton.Text = "  Quit Rush  ";
        quitButton.Position = new Vector2(250, 440);
        quitButton.Size = new Vector2(140, 40);
        quitButton.Disabled = true;
        rushPanel.AddChild(quitButton);
        
        pauseButton = new Button();
        pauseButton.Text = "  Pause  ";
        pauseButton.Position = new Vector2(410, 440);
        pauseButton.Size = new Vector2(140, 40);
        pauseButton.Disabled = true;
        rushPanel.AddChild(pauseButton);
        
        // Rewards preview
        var rewardsTitle = new Label();
        rewardsTitle.Text = "Current Rewards:";
        rewardsTitle.Position = new Vector2(600, 160);
        rushPanel.AddChild(rewardsTitle);
        
        rewardsContainer = new VBoxContainer();
        rewardsContainer.Position = new Vector2(600, 200);
        rushPanel.AddChild(rewardsContainer);
        
        UpdateRewardsDisplay();
    }
    
    private void SetupHistoryPanel()
    {
        var historyPanel = new Control();
        historyPanel.SetAnchor(AnchorPreset.FullRect);
        historyPanel.Position = new Vector2(0, 120);
        historyPanel.Size = new Vector2(800, 480);
        historyPanel.Visible = false;
        mainContainer.AddChild(historyPanel);
        
        var title = new Label();
        title.Text = "Rush History";
        title.AddThemeFontSizeOverride("font_size", 24);
        title.Position = new Vector2(20, 20);
        historyPanel.AddChild(title);
        
        historyContainer = new VBoxContainer();
        historyContainer.Position = new Vector2(20, 70);
        historyContainer.Size = new Vector2(760, 390);
        historyPanel.AddChild(historyContainer);
        
        UpdateHistoryDisplay();
    }
    
    private void SetupStatsPanel()
    {
        var statsPanel = new Control();
        statsPanel.SetAnchor(AnchorPreset.FullRect);
        statsPanel.Position = new Vector2(0, 120);
        statsPanel.Size = new Vector2(800, 480);
        statsPanel.Visible = false;
        mainContainer.AddChild(statsPanel);
        
        var title = new Label();
        title.Text = "Statistics";
        title.AddThemeFontSizeOverride("font_size", 24);
        title.Position = new Vector2(20, 20);
        statsPanel.AddChild(title);
        
        statsContainer = new VBoxContainer();
        statsContainer.Position = new Vector2(20, 70);
        statsContainer.Size = new Vector2(760, 390);
        statsPanel.AddChild(statsContainer);
        
        UpdateStatsDisplay();
    }
    
    private void ConnectSignals()
    {
        rushTabBtn.Pressed += () => SwitchTab(0);
        historyTabBtn.Pressed += () => SwitchTab(1);
        statsTabBtn.Pressed += () => SwitchTab(2);
        
        startButton.Pressed += OnStartPressed;
        advanceButton.Pressed += OnAdvancePressed;
        quitButton.Pressed += OnQuitPressed;
        pauseButton.Pressed += OnPausePressed;
    }
    
    private void SwitchTab(int tab)
    {
        currentTab = tab;
        
        rushPanel.Visible = tab == 0;
        // Find and toggle other panels
        for (int i = 0; i < mainContainer.GetChildCount(); i++)
        {
            var child = mainContainer.GetChild(i);
            if (child is Control c && c != rushPanel && c != mainContainer.GetChild(0)) // Skip bg
            {
                c.Visible = (tab > 0 && i == tab);
            }
        }
        
        // Simple visibility toggle
        var panels = mainContainer.GetChildren();
        foreach (var p in panels)
        {
            if (p is Control c && c != rushPanel && c.Name != "ColorRect")
            {
                c.Visible = false;
            }
        }
        
        if (tab == 1) ShowHistory();
        else if (tab == 2) ShowStats();
        else rushPanel.Visible = true;
        
        UpdateUI();
    }
    
    private void ShowHistory()
    {
        // Find or create history panel
        Control historyPanel = null;
        foreach (var child in mainContainer.GetChildren())
        {
            if (child is Control c && c.Name != "ColorRect" && c != rushPanel)
            {
                historyPanel = c;
                break;
            }
        }
        
        if (historyPanel == null)
        {
            historyPanel = new Control();
            historyPanel.SetAnchor(AnchorPreset.FullRect);
            historyPanel.Position = new Vector2(0, 120);
            historyPanel.Size = new Vector2(800, 480);
            mainContainer.AddChild(historyPanel);
            
            var title = new Label();
            title.Text = "Rush History";
            title.AddThemeFontSizeOverride("font_size", 24);
            title.Position = new Vector2(20, 20);
            historyPanel.AddChild(title);
            
            historyContainer = new VBoxContainer();
            historyContainer.Position = new Vector2(20, 70);
            historyContainer.Size = new Vector2(760, 390);
            historyPanel.AddChild(historyContainer);
        }
        
        historyPanel.Visible = true;
        UpdateHistoryDisplay();
    }
    
    private void ShowStats()
    {
        Control statsPanel = null;
        foreach (var child in mainContainer.GetChildren())
        {
            if (child is Control c && c.Name != "ColorRect" && c != rushPanel)
            {
                statsPanel = c;
                break;
            }
        }
        
        if (statsPanel == null || statsPanel == rushPanel)
        {
            statsPanel = new Control();
            statsPanel.SetAnchor(AnchorPreset.FullRect);
            statsPanel.Position = new Vector2(0, 120);
            statsPanel.Size = new Vector2(800, 480);
            mainContainer.AddChild(statsPanel);
            
            var title = new Label();
            title.Text = "Statistics";
            title.AddThemeFontSizeOverride("font_size", 24);
            title.Position = new Vector2(20, 20);
            statsPanel.AddChild(title);
            
            statsContainer = new VBoxContainer();
            statsContainer.Position = new Vector2(20, 70);
            statsContainer.Size = new Vector2(760, 390);
            statsPanel.AddChild(statsContainer);
        }
        
        statsPanel.Visible = true;
        UpdateStatsDisplay();
    }
    
    private void UpdateUI()
    {
        if (bossRushSystem == null) return;
        
        var data = bossRushSystem.GetData();
        var state = bossRushSystem.GetState();
        
        // Update status
        statusLabel.Text = state.ToString();
        
        // Update stage info
        stageLabel.Text = $"Stage: {data.CurrentStage}";
        
        // Update streak
        streakLabel.Text = $"Current Streak: {data.CurrentStreak} | Best Streak: {data.BestStreak}";
        
        // Update health
        float healthPercent = bossRushSystem.GetCurrentHealthPercent() * 100;
        healthBar.Value = healthPercent;
        healthLabel.Text = $"{healthPercent:F0}%";
        
        // Update button states
        bool inRush = bossRushSystem.IsInRush();
        startButton.Disabled = inRush;
        advanceButton.Disabled = !inRush;
        quitButton.Disabled = !inRush;
        pauseButton.Disabled = !inRush;
        
        if (state == BossRushState.Paused)
            pauseButton.Text = "Resume";
        else
            pauseButton.Text = "Pause";
        
        UpdateRewardsDisplay();
    }
    
    private void UpdateRewardsDisplay()
    {
        foreach (var child in rewardsContainer.GetChildren())
            child.QueueFree();
        
        if (bossRushSystem == null) return;
        
        var data = bossRushSystem.GetData();
        
        var goldLabel = new Label();
        goldLabel.Text = $"💰 Gold: {data.GoldEarned}";
        goldLabel.AddThemeFontSizeOverride("font_size", 18);
        rewardsContainer.AddChild(goldLabel);
        
        var expLabel = new Label();
        expLabel.Text = $"✨ Exp: {data.ExpEarned}";
        expLabel.AddThemeFontSizeOverride("font_size", 18);
        rewardsContainer.AddChild(expLabel);
        
        var bossesLabel = new Label();
        bossesLabel.Text = $"👹 Bosses: {data.BossesDefeated}";
        bossesLabel.AddThemeFontSizeOverride("font_size", 18);
        rewardsContainer.AddChild(bossesLabel);
    }
    
    private void UpdateHistoryDisplay()
    {
        foreach (var child in historyContainer.GetChildren())
            child.QueueFree();
        
        if (bossRushSystem == null) return;
        
        var history = bossRushSystem.GetHistory(10);
        
        if (history.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No rush history yet. Start your first boss rush!";
            historyContainer.AddChild(emptyLabel);
            return;
        }
        
        foreach (var record in history)
        {
            var recordPanel = new HBoxContainer();
            
            var resultLabel = new Label();
            resultLabel.Text = record.Victory ? "✅ Victory" : "❌ Defeat";
            resultLabel.CustomMinimumSize = new Vector2(100, 0);
            recordPanel.AddChild(resultLabel);
            
            var stageLabel = new Label();
            stageLabel.Text = $"Stage {record.Stage}";
            stageLabel.CustomMinimumSize = new Vector2(80, 0);
            recordPanel.AddChild(stageLabel);
            
            var bossesLabel = new Label();
            bossesLabel.Text = $"{record.BossesDefeated} bosses";
            bossesLabel.CustomMinimumSize = new Vector2(100, 0);
            recordPanel.AddChild(bossesLabel);
            
            var rewardsLabel = new Label();
            rewardsLabel.Text = $"{record.GoldEarned}g / {record.ExpEarned}exp";
            recordPanel.AddChild(rewardsLabel);
            
            historyContainer.AddChild(recordPanel);
        }
    }
    
    private void UpdateStatsDisplay()
    {
        foreach (var child in statsContainer.GetChildren())
            child.QueueFree();
        
        if (bossRushSystem == null) return;
        
        var stats = bossRushSystem.GetStatistics();
        
        AddStatRow("Total Attempts:", stats["total_attempts"].ToString());
        AddStatRow("Victories:", stats["total_victories"].ToString());
        AddStatRow("Win Rate:", $"{stats["win_rate"]:P1}");
        AddStatRow("Total Bosses Defeated:", stats["total_bosses"].ToString());
        AddStatRow("Highest Stage:", stats["highest_stage"].ToString());
        AddStatRow("Best Streak:", stats["best_streak"].ToString());
        AddStatRow("Total Gold Earned:", stats["total_gold"].ToString());
        AddStatRow("Total Exp Earned:", stats["total_exp"].ToString());
    }
    
    private void AddStatRow(string label, string value)
    {
        var row = new HBoxContainer();
        
        var labelNode = new Label();
        labelNode.Text = label;
        labelNode.CustomMinimumSize = new Vector2(200, 0);
        row.AddChild(labelNode);
        
        var valueNode = new Label();
        valueNode.Text = value;
        valueNode.AddThemeFontSizeOverride("font_size", 20);
        row.AddChild(valueNode);
        
        statsContainer.AddChild(row);
    }
    
    private void OnStartPressed()
    {
        string difficulty = difficultyOption.GetItemText(difficultyOption.Selected);
        if (bossRushSystem.StartRush(difficulty))
        {
            UpdateUI();
        }
    }
    
    private void OnAdvancePressed()
    {
        if (bossRushSystem.IsInRush())
        {
            var boss = bossRushSystem.GetCurrentBoss();
            if (boss != null)
            {
                bossRushSystem.RecordBossDefeat(boss);
                bossRushSystem.AdvanceStage();
                UpdateUI();
            }
        }
    }
    
    private void OnQuitPressed()
    {
        bossRushSystem.QuitRush();
        UpdateUI();
    }
    
    private void OnPausePressed()
    {
        var state = bossRushSystem.GetState();
        if (state == BossRushState.InProgress)
            bossRushSystem.PauseRush();
        else if (state == BossRushState.Paused)
            bossRushSystem.ResumeRush();
        
        UpdateUI();
    }
    
    public override void _Process(float delta)
    {
        if (bossRushSystem != null && bossRushSystem.IsInRush())
        {
            UpdateUI();
        }
    }
}
