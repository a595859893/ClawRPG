using Godot;
using System;
using System.Collections.Generic;

public class BattleStatisticsUI : Control
{
    private BattleStatisticsSystem _battleStats;
    private Control _mainPanel;
    private TabContainer _tabContainer;
    private Label _winRateLabel;
    private Label _totalBattlesLabel;
    private Label _damageDealtLabel;
    private Label _damageTakenLabel;
    private Label _enemiesKilledLabel;
    private Label _skillAccuracyLabel;
    private Label _mostKilledLabel;
    private Label _dominantElementLabel;
    private Label _sessionStatsLabel;
    private VBoxContainer _recentBattlesContainer;
    private VBoxContainer _elementalDamageContainer;
    
    private bool _visible = false;
    
    public override void _Ready()
    {
        _battleStats = GetNode<BattleStatisticsSystem>("/root/BattleStatisticsSystem");
        
        SetupUI();
        Hide();
    }
    
    private void SetupUI()
    {
        // Main Panel
        _mainPanel = new Control();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_mainPanel);
        
        // Background Panel
        Panel backgroundPanel = new Panel();
        backgroundPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        backgroundPanel.Modulate = new Color(0, 0, 0, 0.85f);
        _mainPanel.AddChild(backgroundPanel);
        
        // Title
        Label title = new Label();
        title.Text = "⚔️ Battle Statistics";
        title.AddThemeFontSizeOverride("font_size", 28);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.Position = new Vector2(0, 20);
        title.Size = new Vector2(800, 50);
        _mainPanel.AddChild(title);
        
        // Close button
        Button closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.Position = new Vector2(750, 20);
        closeBtn.Size = new Vector2(40, 40);
        closeBtn.Pressed += () => ToggleVisibility();
        _mainPanel.AddChild(closeBtn);
        
        // Tab Container
        _tabContainer = new TabContainer();
        _tabContainer.Position = new Vector2(50, 80);
        _tabContainer.Size = new Vector2(700, 480);
        _mainPanel.AddChild(_tabContainer);
        
        // Overview Tab
        Control overviewTab = CreateOverviewTab();
        _tabContainer.AddChild(overviewTab);
        _tabContainer.SetTabTitle(0, "Overview");
        
        // Session Tab
        Control sessionTab = CreateSessionTab();
        _tabContainer.AddChild(sessionTab);
        _tabContainer.SetTabTitle(1, "Session");
        
        // Elemental Tab
        Control elementalTab = CreateElementalTab();
        _tabContainer.AddChild(elementalTab);
        _tabContainer.SetTabTitle(2, "Elemental");
        
        // Recent Battles Tab
        Control recentTab = CreateRecentTab();
        _tabContainer.AddChild(recentTab);
        _tabContainer.SetTabTitle(3, "Recent");
        
        // Reset Button
        Button resetBtn = new Button();
        resetBtn.Text = "Reset Statistics";
        resetBtn.Position = new Vector2(50, 570);
        resetBtn.Size = new Vector2(150, 30);
        resetBtn.Pressed += OnResetPressed;
        _mainPanel.AddChild(resetBtn);
    }
    
    private Control CreateOverviewTab()
    {
        Control tab = new Control();
        
        // Win Rate
        _winRateLabel = CreateStatLabel("Win Rate: 0%", 50, 30);
        tab.AddChild(_winRateLabel);
        
        // Total Battles
        _totalBattlesLabel = CreateStatLabel("Total Battles: 0", 50, 70);
        tab.AddChild(_totalBattlesLabel);
        
        // Damage Dealt
        _damageDealtLabel = CreateStatLabel("Total Damage Dealt: 0", 50, 110);
        tab.AddChild(_damageDealtLabel);
        
        // Damage Taken
        _damageTakenLabel = CreateStatLabel("Total Damage Taken: 0", 50, 150);
        tab.AddChild(_damageDealtLabel);
        
        // Enemies Killed
        _enemiesKilledLabel = CreateStatLabel("Enemies Killed: 0", 50, 190);
        tab.AddChild(_enemiesKilledLabel);
        
        // Bosses Killed
        Label bossesLabel = CreateStatLabel("Bosses Killed: 0", 50, 230);
        tab.AddChild(bossesLabel);
        
        // Elite Killed
        Label eliteLabel = CreateStatLabel("Elite Killed: 0", 50, 270);
        tab.AddChild(eliteLabel);
        
        // Skill Accuracy
        _skillAccuracyLabel = CreateStatLabel("Skill Accuracy: 0%", 50, 310);
        tab.AddChild(_skillAccuracyLabel);
        
        // Most Killed Enemy
        _mostKilledLabel = CreateStatLabel("Most Killed: None", 50, 350);
        tab.AddChild(_mostKilledLabel);
        
        // Dominant Element
        _dominantElementLabel = CreateStatLabel("Dominant Element: Physical", 50, 390);
        tab.AddChild(_dominantElementLabel);
        
        // Average Damage
        Label avgDamageLabel = CreateStatLabel("Avg Damage/Battle: 0", 400, 30);
        tab.AddChild(avgDamageLabel);
        
        // Average Duration
        Label avgDurationLabel = CreateStatLabel("Avg Battle Duration: 0s", 400, 70);
        tab.AddChild(avgDurationLabel);
        
        // Critical Damage
        Label critDamageLabel = CreateStatLabel("Critical Damage: 0", 400, 110);
        tab.AddChild(critDamageLabel);
        
        // Healing
        Label healingLabel = CreateStatLabel("Total Healing: 0", 400, 150);
        tab.AddChild(healingLabel);
        
        // Total Battle Time
        Label totalTimeLabel = CreateStatLabel("Total Battle Time: 0m", 400, 190);
        tab.AddChild(totalTimeLabel);
        
        // Skills Hit/Missed
        Label skillsUsedLabel = CreateStatLabel("Skills Used: 0 (Hit: 0, Missed: 0)", 400, 230);
        tab.AddChild(skillsUsedLabel);
        
        return tab;
    }
    
    private Control CreateSessionTab()
    {
        Control tab = new Control();
        
        _sessionStatsLabel = new Label();
        _sessionStatsLabel.Position = new Vector2(50, 30);
        _sessionStatsLabel.Size = new Vector2(600, 400);
        _sessionStatsLabel.Text = "Session Statistics\n\nBattles: 0\nVictories: 0\nWin Rate: 0%";
        tab.AddChild(_sessionStatsLabel);
        
        return tab;
    }
    
    private Control CreateElementalTab()
    {
        Control tab = new Control();
        
        _elementalDamageContainer = new VBoxContainer();
        _elementalDamageContainer.Position = new Vector2(50, 30);
        _elementalDamageContainer.Size = new Vector2(600, 400);
        tab.AddChild(_elementalDamageContainer);
        
        return tab;
    }
    
    private Control CreateRecentTab()
    {
        Control tab = new Control();
        
        _recentBattlesContainer = new VBoxContainer();
        _recentBattlesContainer.Position = new Vector2(50, 30);
        _recentBattlesContainer.Size = new Vector2(600, 400);
        tab.AddChild(_recentBattlesContainer);
        
        return tab;
    }
    
    private Label CreateStatLabel(string text, float x, float y)
    {
        Label label = new Label();
        label.Text = text;
        label.Position = new Vector2(x, y);
        label.Size = new Vector2(300, 30);
        return label;
    }
    
    public override void _Process(double delta)
    {
        if (_visible)
        {
            UpdateStats();
        }
    }
    
    private void UpdateStats()
    {
        var stats = _battleStats.GetStats();
        
        // Overview tab
        _winRateLabel.Text = $"Win Rate: {_battleStats.GetWinRate():F1}%";
        _totalBattlesLabel.Text = $"Total Battles: {stats.TotalBattles}";
        _damageDealtLabel.Text = $"Total Damage Dealt: {stats.TotalDamageDealt:N0}";
        _damageTakenLabel.Text = $"Total Damage Taken: {stats.TotalDamageTaken:N0}";
        _enemiesKilledLabel.Text = $"Enemies Killed: {stats.TotalEnemiesKilled:N0}";
        _skillAccuracyLabel.Text = $"Skill Accuracy: {_battleStats.GetSkillAccuracy():F1}%";
        _mostKilledLabel.Text = $"Most Killed: {_battleStats.GetMostKilledEnemy()}";
        _dominantElementLabel.Text = $"Dominant Element: {_battleStats.GetDominantElement()}";
        
        // Session tab
        float sessionWinRate = stats.SessionBattles > 0 ? (float)stats.SessionVictories / stats.SessionBattles * 100f : 0f;
        _sessionStatsLabel.Text = $"Session Statistics\n\nBattles: {stats.SessionBattles}\nVictories: {stats.SessionVictories}\nDefeats: {stats.SessionBattles - stats.SessionVictories}\nWin Rate: {sessionWinRate:F1}%\n\nSession Start: {stats.SessionStart:HH:mm}";
        
        // Elemental tab
        UpdateElementalTab(stats);
        
        // Recent tab
        UpdateRecentTab(stats);
    }
    
    private void UpdateElementalTab(BattleStatsData stats)
    {
        // Clear existing
        foreach (Node child in _elementalDamageContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add elemental stats
        AddElementalStat("Physical", stats.PhysicalDamage, Colors.Brown);
        AddElementalStat("Fire", stats.FireDamage, Colors.OrangeRed);
        AddElementalStat("Ice", stats.IceDamage, Colors.LightBlue);
        AddElementalStat("Lightning", stats.LightningDamage, Colors.Yellow);
        AddElementalStat("Dark", stats.DarkDamage, Colors.Purple);
        AddElementalStat("Holy", stats.HolyDamage, Colors.Gold);
    }
    
    private void AddElementalStat(string element, int damage, Color color)
    {
        HBoxContainer row = new HBoxContainer();
        row.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        
        Label elementLabel = new Label();
        elementLabel.Text = element + ":";
        elementLabel.Modulate = color;
        elementLabel.CustomMinimumSize = new Vector2(100, 0);
        row.AddChild(elementLabel);
        
        Label damageLabel = new Label();
        damageLabel.Text = damage.ToString("N0");
        row.AddChild(damageLabel);
        
        _elementalDamageContainer.AddChild(row);
    }
    
    private void UpdateRecentTab(BattleStatsData stats)
    {
        // Clear existing
        foreach (Node child in _recentBattlesContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (stats.RecentBattles.Count == 0)
        {
            Label noDataLabel = new Label();
            noDataLabel.Text = "No recent battles";
            _recentBattlesContainer.AddChild(noDataLabel);
            return;
        }
        
        foreach (var battle in stats.RecentBattles)
        {
            HBoxContainer row = new HBoxContainer();
            
            Label resultLabel = new Label();
            resultLabel.Text = battle.Victory ? "✓" : "✗";
            resultLabel.Modulate = battle.Victory ? Colors.Green : Colors.Red;
            resultLabel.CustomMinimumSize = new Vector2(30, 0);
            row.AddChild(resultLabel);
            
            Label statsLabel = new Label();
            statsLabel.Text = $"{battle.Timestamp:HH:mm} | {battle.DamageDealt:N0} dmg | {battle.EnemiesKilled} kills | {battle.Duration:F1}s";
            row.AddChild(statsLabel);
            
            _recentBattlesContainer.AddChild(row);
        }
    }
    
    private void OnResetPressed()
    {
        _battleStats.ResetStats();
        UpdateStats();
    }
    
    public void ToggleVisibility()
    {
        if (_visible)
        {
            Hide();
            _visible = false;
        }
        else
        {
            Show();
            _visible = true;
            UpdateStats();
        }
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.U)
        {
            ToggleVisibility();
        }
        else if (e is InputEventKey escapeEvent && escapeEvent.Pressed && escapeEvent.Keycode == Key.Escape && _visible)
        {
            ToggleVisibility();
        }
    }
}
