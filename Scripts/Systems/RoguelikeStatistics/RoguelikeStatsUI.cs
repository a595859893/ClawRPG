using Godot;
using System;
using System.Collections.Generic;

public partial class RoguelikeStatsUI : Control
{
    private RoguelikeStatsSystem _system;
    private TabContainer _tabContainer;
    private Label _statsLabel;
    private Label _runsLabel;
    private Label _deathCausesLabel;
    private VBoxContainer _historyContainer;
    
    // Colors
    private Color _victoryColor = new Color(0.2f, 0.8f, 0.2f);
    private Color _defeatColor = new Color(0.8f, 0.2f, 0.2f);
    private Color _goldColor = new Color(1.0f, 0.84f, 0.0f);
    private Color _floorColor = new Color(0.4f, 0.6f, 1.0f);
    
    public override void _Ready()
    {
        _system = new RoguelikeStatsSystem();
        
        // Create main panel
        var panel = new PanelContainer();
        panel.SetAnchorsPreset(Control.Preset.FullRect);
        AddChild(panel);
        
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        
        // Title
        var title = new Label();
        title.Text = "  ⚔️ Roguelike Statistics  ⚔️";
        title.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(title);
        
        // Create tabs
        _tabContainer = new TabContainer();
        _tabContainer.SetVExpandFlags(Control.VExpandFlags.ExpandFill);
        vbox.AddChild(_tabContainer);
        
        // Overview tab
        var overviewTab = new ScrollContainer();
        overviewTab.Name = "Overview";
        _tabContainer.AddChild(overviewTab);
        
        var overviewVBox = new VBoxContainer();
        overviewVBox.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        overviewTab.AddChild(overviewVBox);
        
        _statsLabel = new Label();
        _statsLabel.Name = "StatsLabel";
        overviewVBox.AddChild(_statsLabel);
        
        // History tab
        var historyTab = new ScrollContainer();
        historyTab.Name = "History";
        _tabContainer.AddChild(historyTab);
        
        var historyVBox = new VBoxContainer();
        historyVBox.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        historyTab.AddChild(historyVBox);
        
        _historyContainer = new VBoxContainer();
        historyVBox.AddChild(_historyContainer);
        
        // Death Causes tab
        var deathTab = new ScrollContainer();
        deathTab.Name = "Death Causes";
        _tabContainer.AddChild(deathTab);
        
        var deathVBox = new VBoxContainer();
        deathVBox.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        deathTab.AddChild(deathVBox);
        
        _deathCausesLabel = new Label();
        deathVBox.AddChild(_deathCausesLabel);
        
        // Test buttons
        var buttonBox = new HBoxContainer();
        vbox.AddChild(buttonBox);
        
        var startRunBtn = new Button();
        startRunBtn.Text = "Start Run";
        startRunBtn.Pressed += () => _system.StartRun("Warrior", "Attack");
        buttonBox.AddChild(startRunBtn);
        
        var victoryBtn = new Button();
        victoryBtn.Text = "Victory (Floor 25)";
        victoryBtn.Pressed += () => _system.CompleteRunVictory(25);
        buttonBox.AddChild(victoryBtn);
        
        var deathBtn = new Button();
        deathBtn.Text = "Death (Boss)";
        deathBtn.Pressed += () => _system.CompleteRunDeath(15, "BossDamage");
        buttonBox.AddChild(deathBtn);
        
        var recordBtn = new Button();
        recordBtn.Text = "Record Stats";
        recordBtn.Pressed += RecordTestStats;
        buttonBox.AddChild(recordBtn);
        
        var refreshBtn = new Button();
        refreshBtn.Text = "Refresh";
        refreshBtn.Pressed += RefreshDisplay;
        buttonBox.AddChild(refreshBtn);
        
        // Close button
        var closeBtn = new Button();
        closeBtn.Text = "ESC to Close";
        closeBtn.Pressed += () => Visible = false;
        vbox.AddChild(closeBtn);
        
        RefreshDisplay();
    }
    
    private void RecordTestStats()
    {
        if (!_system.IsRunInProgress()) return;
        
        _system.RecordEnemyKill(false);
        _system.RecordEnemyKill(true);
        _system.RecordDamageDealt(1500);
        _system.RecordDamageTaken(500);
        _system.RecordGoldEarned(250);
        _system.RecordGoldSpent(100);
        _system.RecordItemCollected();
        _system.RecordKeyEvent("Found treasure room!");
    }
    
    private void RefreshDisplay()
    {
        var data = _system.GetData();
        var summary = _system.GetStatistics();
        
        // Overview
        _statsLabel.Text = $@"
=== LIFETIME STATISTICS ===

Total Runs:     {data.TotalRuns}
Wins:           {data.TotalWins} ({summary["WinRate"]})
Deaths:         {data.TotalDeaths}

Highest Floor:  [color=#66A3FF]{data.HighestFloorReached}[/color]
Total Enemies:  {data.TotalEnemiesKilled}
Total Damage:   {data.TotalDamageDealt}
Total Gold:     [color=#FFD700]{data.TotalGoldEarned}[/color]

=== STREAKS ===

Current Win:    {data.CurrentWinStreak}
Best Win:       {data.BestWinStreak}
Current Loss:   {data.CurrentLossStreak}
Best Loss:      {data.BestLossStreak}

=== FAVORITES ===

Class:          {data.MostUsedClass}
Build:          {data.MostUsedBuild}
";
        
        // History
        _historyContainer.QueueFreeChildren();
        var recentRuns = _system.GetRecentRuns(10);
        
        foreach (var run in recentRuns)
        {
            var runLabel = new Label();
            string color = run.Victory ? "#33CC33" : "#CC3333";
            string result = run.Victory ? "VICTORY" : "DEATH";
            string cause = run.Victory ? "" : $" ({run.DeathCause})";
            
            runLabel.Text = $"[color={color}]{result}[/color] - Floor {run.FloorReached} | {run.CharacterClass} ({run.BuildType}) | {run.Duration / 60}:{run.Duration % 60:D2} | {run.EnemiesKilled} kills | {run.GoldEarned}g{cause}";
            _historyContainer.AddChild(runLabel);
        }
        
        // Death causes
        _deathCausesLabel.Text = "\n=== DEATH CAUSES ===\n";
        foreach (var cause in data.DeathCauses)
        {
            _deathCausesLabel.Text += $"{cause.Key}: {cause.Value}\n";
        }
        
        if (data.DeathCauses.Count == 0)
            _deathCausesLabel.Text += "No deaths recorded yet.\n";
    }
    
    public RoguelikeStatsSystem GetSystem() => _system;
    
    public override void _Process(double delta)
    {
        // Could add real-time update here
    }
}
