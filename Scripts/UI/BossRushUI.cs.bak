using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.UI.BossRush;

public partial class BossRushUI : Control
{
    private BossRushSystem bossRushSystem;
    private Control mainContainer;
    private Label titleLabel;
    
    // Tab buttons
    private Button rushTabBtn;
    private Button historyTabBtn;
    private Button statsTabBtn;
    
    // Panel components
    private BossRushUIRushPanel rushPanel;
    private BossRushUIHistoryPanel historyPanel;
    private BossRushUIStatsPanel statsPanel;
    
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
        mainContainer.SetAnchorsPreset(FullRect);
        AddChild(mainContainer);
        
        // Background
        var bg = new ColorRect();
        bg.Color = new Color(0, 0, 0, 0.8f);
        bg.SetAnchorsPreset(FullRect);
        mainContainer.AddChild(bg);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = "⚔️ Boss Rush ⚔️";
        titleLabel.SetAnchorsPreset(TopWide);
        titleLabel.AddThemeFontSizeOverride("font_size", 32);
        titleLabel.Position = new Vector2(0, 20);
        mainContainer.AddChild(titleLabel);
        
        // Tab buttons
        SetupTabButtons();
        
        // Initialize panel components
        InitializePanels();
    }
    
    private void SetupTabButtons()
    {
        var tabContainer = new HBoxContainer();
        tabContainer.SetAnchorsPreset(TopWide);
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
    }
    
    private void InitializePanels()
    {
        // Rush Panel
        rushPanel = new BossRushUIRushPanel();
        rushPanel.Initialize(bossRushSystem);
        rushPanel.Setup(mainContainer, new Vector2(0, 120), new Vector2(800, 480));
        rushPanel.OnStartPressed += OnStartPressed;
        rushPanel.OnAdvancePressed += OnAdvancePressed;
        rushPanel.OnQuitPressed += OnQuitPressed;
        rushPanel.OnPausePressed += OnPausePressed;
        
        // History Panel
        historyPanel = new BossRushUIHistoryPanel();
        historyPanel.Initialize(bossRushSystem);
        historyPanel.Setup(mainContainer, new Vector2(0, 120), new Vector2(800, 480));
        
        // Stats Panel
        statsPanel = new BossRushUIStatsPanel();
        statsPanel.Initialize(bossRushSystem);
        statsPanel.Setup(mainContainer, new Vector2(0, 120), new Vector2(800, 480));
    }
    
    private void ConnectSignals()
    {
        rushTabBtn.Pressed += () => SwitchTab(0);
        historyTabBtn.Pressed += () => SwitchTab(1);
        statsTabBtn.Pressed += () => SwitchTab(2);
    }
    
    private void SwitchTab(int tab)
    {
        currentTab = tab;
        
        // Update panel visibility
        rushPanel.Visible = (tab == 0);
        historyPanel.Visible = (tab == 1);
        statsPanel.Visible = (tab == 2);
        
        // Update content
        if (tab == 1)
            historyPanel.UpdateDisplay();
        else if (tab == 2)
            statsPanel.UpdateDisplay();
        
        UpdateUI();
    }
    
    private void UpdateUI()
    {
        if (bossRushSystem == null) return;
        
        rushPanel.UpdateUI();
    }
    
    private void OnStartPressed(string difficulty)
    {
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
    
    public override void _Process(double delta)
    {
        if (bossRushSystem != null && bossRushSystem.IsInRush())
        {
            UpdateUI();
        }
    }
}
