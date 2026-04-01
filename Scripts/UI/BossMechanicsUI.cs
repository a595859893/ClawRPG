using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.BossMechanics;

namespace ClawRPG.UI {
public partial class BossMechanicsUI : Control
{
    private Label _titleLabel;
    private TabContainer _tabContainer;
    private VBoxContainer _bossListTab;
    private VBoxContainer _activeBattleTab;
    private VBoxContainer _statsTab;
    private VBoxContainer _loreTab;
    
    private ScrollContainer _bossScroll;
    private VBoxContainer _bossContainer;
    
    private Label _activeBossLabel;
    private ProgressBar _healthBar;
    private Label _phaseLabel;
    private Label _timerLabel;
    private Label _enrageLabel;
    private Label _rageLabel;
    private Label _minionLabel;
    private VBoxContainer _skillContainer;
    private VBoxContainer _timerContainer;
    
    private Label _statsLabel;
    
    private BossMechanicsSystem _system;
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        _system = GetNode<BossMechanicsSystem>("/root/BossMechanicsSystem");
        if (_system == null)
        {
            GD.Print("[BossMechanicsUI] System not found, creating...");
            _system = new BossMechanicsSystem();
            AddChild(_system);
        }
        
        SetupUI();
        SetupShortcuts();
        
        // Connect to system events
        _system.OnPhaseChanged += OnPhaseChanged;
        _system.OnEnrageTriggered += OnEnrageTriggered;
        _system.OnBossDefeated += OnBossDefeated;
        
        Visible = false;
    }
    
    private void SetupUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(AnchorPreset.FullRect);
        mainContainer.Margin = new Color32(20, 20, 20, 20);
        AddChild(mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ Boss Mechanics System";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);
        
        // Boss List Tab
        _bossListTab = new VBoxContainer();
        _bossListTab.Name = "Boss List";
        _tabContainer.AddChild(_bossListTab);
        
        var bossListLabel = new Label();
        bossListLabel.Text = "Available Bosses";
        bossListLabel.AddThemeFontSizeOverride("font_size", 18);
        _bossListTab.AddChild(bossListLabel);
        
        _bossScroll = new ScrollContainer();
        _bossScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _bossListTab.AddChild(_bossScroll);
        
        _bossContainer = new VBoxContainer();
        _bossScroll.AddChild(_bossContainer);
        
        PopulateBossList();
        
        // Active Battle Tab
        _activeBattleTab = new VBoxContainer();
        _activeBattleTab.Name = "Active Battle";
        _tabContainer.AddChild(_activeBattleTab);
        
        _activeBossLabel = new Label();
        _activeBossLabel.Text = "No active boss battle";
        _activeBossLabel.AddThemeFontSizeOverride("font_size", 20);
        _activeBattleTab.AddChild(_activeBossLabel);
        
        _healthBar = new ProgressBar();
        _healthBar.MinValue = 0;
        _healthBar.MaxValue = 100;
        _healthBar.Value = 0;
        _healthBar.CustomMinimumSize = new Vector2(0, 30);
        _activeBattleTab.AddChild(_healthBar);
        
        var infoContainer = new HBoxContainer();
        _activeBattleTab.AddChild(infoContainer);
        
        _phaseLabel = new Label();
        _phaseLabel.Text = "Phase: -";
        infoContainer.AddChild(_phaseLabel);
        
        _timerLabel = new Label();
        _timerLabel.Text = "Time: 0s";
        _activeBattleTab.AddChild(_timerLabel);
        
        _enrageLabel = new Label();
        _enrageLabel.Text = "⚠️ Enrage: Not triggered";
        _enrageLabel.Modulate = new Color(1, 0.3, 0.3);
        _activeBattleTab.AddChild(_enrageLabel);
        
        // REQ-127: RAGE mode indicator
        _rageLabel = new Label();
        _rageLabel.Text = "☠️ RAGE MODE";
        _rageLabel.Modulate = new Color(1, 0, 0);
        _rageLabel.AddThemeFontSizeOverride("font_size", 28);
        _rageLabel.Hide();
        _activeBattleTab.AddChild(_rageLabel);
        
        _minionLabel = new Label();
        _minionLabel.Text = "Minions: 0";
        _activeBattleTab.AddChild(_minionLabel);
        
        var skillLabel = new Label();
        skillLabel.Text = "Skills:";
        skillLabel.AddThemeFontSizeOverride("font_size", 16);
        _activeBattleTab.AddChild(skillLabel);
        
        _skillContainer = new VBoxContainer();
        _activeBattleTab.AddChild(_skillContainer);
        
        // Stats Tab
        _statsTab = new VBoxContainer();
        _statsTab.Name = "Statistics";
        _tabContainer.AddChild(_statsTab);
        
        var statsTitle = new Label();
        statsTitle.Text = "Boss Battle Statistics";
        statsTitle.AddThemeFontSizeOverride("font_size", 18);
        _statsTab.AddChild(statsTitle);
        
        _statsLabel = new Label();
        _statsLabel.Text = "Loading...";
        _statsTab.AddChild(_statsLabel);
        
        // Lore Tab
        _loreTab = new VBoxContainer();
        _loreTab.Name = "Boss Lore";
        _tabContainer.AddChild(_loreTab);
        
        var loreTitle = new Label();
        loreTitle.Text = "Boss Lore & Strategy";
        loreTitle.AddThemeFontSizeOverride("font_size", 18);
        _loreTab.AddChild(loreTitle);
        
        var loreContent = new RichTextLabel();
        loreContent.BbcodeEnabled = true;
        loreContent.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        loreContent.Text = "[b]Dragon Lord[/b]\n" +
            "- Weakness: Ice (2x damage)\n" +
            "- Strategy: Interrupt fire breath, dodge wing slash\n" +
            "- Phase 2: Kill summoned dragons quickly\n\n" +
            "[b]Shadow Assassin[/b]\n" +
            "- Weakness: Holy (1.8x damage)\n" +
            "- Strategy: Use AoE to hit clones\n" +
            "- Phase 3: High burst damage, stay mobile\n\n" +
            "[b]Ancient Golem[/b]\n" +
            "- Weakness: Lightning (1.8x damage)\n" +
            "- Strategy: Lightning attacks break shield\n" +
            "- Phase 2: Spread out to avoid earthquake\n\n" +
            "[b]Frost Wyrm[/b]\n" +
            "- Weakness: Fire (2x damage)\n" +
            "- Strategy: Fire attacks prevent freezing\n" +
            "- Watch for absolute zero ability\n\n" +
            "[b]Demon King[/b]\n" +
            "- Weakness: Holy (2.5x damage)\n" +
            "- Strategy: Kill minions first\n" +
            "- Phase 4: Very dangerous, use all cooldowns";
        _loreTab.AddChild(loreContent);
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += () => ToggleUI();
        mainContainer.AddChild(closeButton);
    }
    
    private void PopulateBossList()
    {
        // Clear existing
        foreach (var child in _bossContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var database = BossMechanicsDatabase.Instance;
        var bossIds = database.GetAllBossIds();
        
        foreach (var bossId in bossIds)
        {
            var bossData = database.GetBoss(bossId);
            if (bossData == null) continue;
            
            var bossCard = new PanelContainer();
            bossCard.CustomMinimumSize = new Vector2(0, 80);
            
            var cardContainer = new VBoxContainer();
            bossCard.AddChild(cardContainer);
            
            var nameLabel = new Label();
            nameLabel.Text = $"♟ {bossData.BossName} (Lv.{bossData.BossLevel})";
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            cardContainer.AddChild(nameLabel);
            
            var statsLabel = new Label();
            statsLabel.Text = $"HP: {bossData.MaxHealth:N0} | ATK: {bossData.Attack} | DEF: {bossData.Defense}";
            cardContainer.AddChild(statsLabel);
            
            var phaseLabel = new Label();
            phaseLabel.Text = $"Phases: {bossData.Phases.Count} | Enrage: {bossData.EnrageTime}s | Minions: {bossData.MaxMinionCount}";
            cardContainer.AddChild(phaseLabel);
            
            var weaknessLabel = new Label();
            weaknessLabel.Text = $"Weakness: {bossData.WeaknessElement} ({bossData.WeaknessMultiplier}x)";
            weaknessLabel.Modulate = new Color(1, 0.5, 0.5);
            cardContainer.AddChild(weaknessLabel);
            
            var startButton = new Button();
            startButton.Text = "Start Battle";
            startButton.Pressed += () => _system.StartBossBattle(bossId);
            cardContainer.AddChild(startButton);
            
            _bossContainer.AddChild(bossCard);
        }
    }
    
    private void SetupShortcuts()
    {
        // Handle keyboard input in _Input
    }
    
    public override void _Input(InputEvent eventEvent)
    {
        if (eventEvent is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.B && !keyEvent.Echo)
            {
                ToggleUI();
            }
            else if (keyEvent.Keycode == Key.Escape && _isVisible)
            {
                ToggleUI();
            }
        }
    }
    
    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            UpdateUI();
        }
    }
    
    private void UpdateUI()
    {
        UpdateBossList();
        UpdateActiveBattle();
        UpdateStats();
    }
    
    private void UpdateBossList()
    {
        PopulateBossList();
    }
    
    private void UpdateActiveBattle()
    {
        var database = BossMechanicsDatabase.Instance;
        var bossIds = database.GetAllBossIds();
        
        bool hasActive = false;
        foreach (var bossId in bossIds)
        {
            if (_system.IsBossActive(bossId))
            {
                hasActive = true;
                var bossData = database.GetBoss(bossId);
                var state = _system.GetBattleState(bossId);
                
                _activeBossLabel.Text = $"⚔️ {bossData.BossName}";
                
                float healthPercent = (state.CurrentHealth / state.MaxHealth) * 100;
                _healthBar.MaxValue = 100;
                _healthBar.Value = healthPercent;
                
                var phase = _system.GetCurrentPhase(bossId);
                _phaseLabel.Text = phase != null ? $"Phase: {phase.PhaseNumber} - {phase.PhaseName}" : "Phase: 1";
                
                _timerLabel.Text = $"Time: {state.BattleTime:F1}s";
                
                _enrageLabel.Text = state.IsEnraged ? "⚠️⚠️ ENRAGED! ⚠️⚠️" : $"Enrage in: {Mathf.Max(0, bossData.EnrageTime - state.BattleTime):F0}s";
                _enrageLabel.Modulate = state.IsEnraged ? new Color(1, 0, 0) : new Color(1, 0.7, 0.3);
                
                // REQ-127: Show RAGE mode indicator
                bool isRageTriggered = state.IsRageTriggered;
                _rageLabel.Visible = isRageTriggered;
                if (isRageTriggered)
                {
                    GD.Print($"[BossMechanicsUI] BOSS {bossData.BossName} entered RAGE MODE!");
                }
                
                _minionLabel.Text = $"Minions: {state.ActiveMinionCount}";
                
                // Update skills
                foreach (var child in _skillContainer.GetChildren())
                {
                    child.QueueFree();
                }
                
                var skills = _system.GetAvailableSkills(bossId);
                foreach (var skillId in skills)
                {
                    var skillLabel = new Label();
                    skillLabel.Text = $"✓ {skillId} ready";
                    skillLabel.Modulate = new Color(0.3, 1, 0.3);
                    _skillContainer.AddChild(skillLabel);
                }
                
                break;
            }
        }
        
        if (!hasActive)
        {
            _activeBossLabel.Text = "No active boss battle";
            _healthBar.Value = 0;
            _phaseLabel.Text = "Phase: -";
            _timerLabel.Text = "Time: 0s";
            _enrageLabel.Text = "⚠️ Enrage: Not triggered";
            _enrageLabel.Modulate = new Color(1, 0.3, 0.3);
            _minionLabel.Text = "Minions: 0";
            
            foreach (var child in _skillContainer.GetChildren())
            {
                child.QueueFree();
            }
        }
    }
    
    private void UpdateStats()
    {
        var stats = _system.Stats;
        
        string statsText = $"[b]Boss Battle Statistics[/b]\n\n";
        statsText += $"Bosses Defeated: {stats.BossesDefeated}\n";
        statsText += $"Bosses Fled: {stats.BossesFled}\n";
        statsText += $"Phases Triggered: {stats.PhasesTriggered}\n";
        statsText += $"Minions Spawned: {stats.MinionsSpawned}\n";
        statsText += $"Minions Defeated: {stats.MinionsDefeated}\n";
        statsText += $"Enrage Triggers: {stats.EnrageTriggers}\n";
        statsText += $"Total Battle Time: {stats.TotalBattleTime}s\n";
        statsText += $"Total Damage Dealt: {stats.TotalDamageDealt:N0}\n";
        statsText += $"Fastest Kill: {stats.FastestKillTime}s\n";
        
        if (stats.BossKills.Count > 0)
        {
            statsText += "\n[b]Boss Kills:[/b]\n";
            foreach (var kvp in stats.BossKills)
            {
                var bossData = BossMechanicsDatabase.Instance.GetBoss(kvp.Key);
                string name = bossData != null ? bossData.BossName : kvp.Key;
                statsText += $"  {name}: {kvp.Value}\n";
            }
        }
        
        _statsLabel.Text = statsText;
    }
    
    private void OnPhaseChanged(BossBattleState state)
    {
        GD.Print($"[BossMechanicsUI] Phase changed: {state.CurrentPhase}");
        UpdateUI();
    }
    
    private void OnEnrageTriggered(BossBattleState state)
    {
        GD.Print($"[BossMechanicsUI] Boss enrage triggered!");
        UpdateUI();
    }
    
    private void OnBossDefeated(BossBattleState state)
    {
        GD.Print($"[BossMechanicsUI] Boss defeated!");
        UpdateUI();
    }
    
    public override void _Process(double delta)
    {
        if (!_isVisible) return;
        
        // Update active battle
        var database = BossMechanicsDatabase.Instance;
        var bossIds = database.GetAllBossIds();
        
        foreach (var bossId in bossIds)
        {
            if (_system.IsBossActive(bossId))
            {
                _system.UpdateBossBattle(bossId, (float)delta);
            }
        }
        
        // Refresh UI periodically
        if (Engine.GetFramesDrawn() % 30 == 0)
        {
            UpdateActiveBattle();
        }
    }
}
}
