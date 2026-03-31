using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.UI;

public class CombatHUDEnhancementUI : Control
{
	private CombatHUDEnhancementSystem _combatSystem;
	
	// Main container
	private PanelContainer _mainPanel;
	private VBoxContainer _mainContainer;
	
	// Stats display
	private Label _titleLabel;
	private Label _combatTimeLabel;
	private Label _dpsLabel;
	private Label _damageDealtLabel;
	private Label _damageTakenLabel;
	private Label _healingLabel;
	private Label _enemiesKilledLabel;
	private Label _criticalHitsLabel;
	private Label _comboLabel;
	private Label _dodgeLabel;
	private Label _blockLabel;
	
	// Rating display
	private Label _ratingLabel;
	private ProgressBar _efficiencyBar;
	private ProgressBar _survivalBar;
	private ProgressBar _skillBar;
	private ProgressBar _paceBar;
	
	// Toggle
	private bool _isVisible = false; 
	private KeyToggleHandler _toggleHandler;
	
	public override void _Ready()
	{
		_combatSystem = CombatHUDEnhancementSystem.Instance;
		if (_combatSystem == null)
		{
			_combatSystem = new CombatHUDEnhancementSystem();
		}
		
		SetupUI();
		ConnectSignals();
		Hide();
	}
	
	private void SetupUI()
	{
		// Main panel
		_mainPanel = new PanelContainer();
		_mainPanel.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
		_mainPanel.Position = new Vector2(-320, -280);
		_mainPanel.CustomMinimumSize = new Vector2(300, 260);
		_mainPanel.Modulate = new Color(1, 1, 1, 0.9f);
		AddChild(_mainPanel);
		
		// Style
		var style = new StyleBoxFlat();
		style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
		style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(8);
		_mainPanel.AddThemeStyleboxOverride("panel", style);
		
		// Main container
		_mainContainer = new VBoxContainer();
		_mainContainer.AddThemeConstantOverride("separation", 4);
		_mainPanel.AddChild(_mainContainer);
		
		// Title
		_titleLabel = new Label();
		_titleLabel.Text = "⚔️ Combat Stats";
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_titleLabel.AddThemeFontSizeOverride("font_size", 18);
		_mainContainer.AddChild(_titleLabel);
		
		// Add separator
		AddHSeparator(_mainContainer);
		
		// Combat time and DPS row
		var topRow = new HBoxContainer();
		_mainContainer.AddChild(topRow);
		
		_combatTimeLabel = new Label();
		_combatTimeLabel.Text = "Time: 00:00";
		_combatTimeLabel.AddThemeFontSizeOverride("font_size", 14);
		topRow.AddChild(_combatTimeLabel);
		
		topRow.AddChild(new Control { CustomMinimumSize = new Vector2(20, 0) });
		
		_dpsLabel = new Label();
		_dpsLabel.Text = "DPS: 0";
		_dpsLabel.AddThemeFontSizeOverride("font_size", 14);
		topRow.AddChild(_dpsLabel);
		
		// Add stats grid
		var statsGrid = new GridContainer();
		statsGrid.Columns = 2;
		_mainContainer.AddChild(statsGrid);
		
		// Row 1
		_damageDealtLabel = CreateStatLabel("Damage Dealt: 0", statsGrid);
		_damageTakenLabel = CreateStatLabel("Damage Taken: 0", statsGrid);
		
		// Row 2
		_healingLabel = CreateStatLabel("Healing: 0", statsGrid);
		_enemiesKilledLabel = CreateStatLabel("Enemies: 0", statsGrid);
		
		// Row 3
		_criticalHitsLabel = CreateStatLabel("Crits: 0", statsGrid);
		_comboLabel = CreateStatLabel("Combo: 0", statsGrid);
		
		// Row 4
		_dodgeLabel = CreateStatLabel("Dodge: 0", statsGrid);
		_blockLabel = CreateStatLabel("Block: 0", statsGrid);
		
		// Add separator
		AddHSeparator(_mainContainer);
		
		// Rating section
		var ratingContainer = new VBoxContainer();
		_mainContainer.AddChild(ratingContainer);
		
		_ratingLabel = new Label();
		_ratingLabel.Text = "Rating: C";
		_ratingLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_ratingLabel.AddThemeFontSizeOverride("font_size", 20);
		ratingContainer.AddChild(_ratingLabel);
		
		// Efficiency bar
		var effLabel = new Label();
		effLabel.Text = "Damage Efficiency";
		effLabel.AddThemeFontSizeOverride("font_size", 11);
		ratingContainer.AddChild(effLabel);
		
		_efficiencyBar = CreateProgressBar(ratingContainer);
		
		// Survival bar
		var survLabel = new Label();
		survLabel.Text = "Survival";
		survLabel.AddThemeFontSizeOverride("font_size", 11);
		ratingContainer.AddChild(survLabel);
		
		_survivalBar = CreateProgressBar(ratingContainer);
		
		// Skill usage bar
		var skillLabel = new Label();
		skillLabel.Text = "Skill Usage";
		skillLabel.AddThemeFontSizeOverride("font_size", 11);
		ratingContainer.AddChild(skillLabel);
		
		_skillBar = CreateProgressBar(ratingContainer);
		
		// Combat pace bar
		var paceLabel = new Label();
		paceLabel.Text = "Combat Pace";
		paceLabel.AddThemeFontSizeOverride("font_size", 11);
		ratingContainer.AddChild(paceLabel);
		
		_paceBar = CreateProgressBar(ratingContainer);
		
		// Add key hint
		var hintLabel = new Label();
		hintLabel.Text = "Press [ to toggle";
		hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
		hintLabel.AddThemeFontSizeOverride("font_size", 10);
		hintLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
		_mainContainer.AddChild(hintLabel);
	}
	
	private Label CreateStatLabel(string text, GridContainer parent)
	{
		var label = new Label();
		label.Text = text;
		label.AddThemeFontSizeOverride("font_size", 12);
		parent.AddChild(label);
		return label;
	}
	
	private ProgressBar CreateProgressBar(Container parent)
	{
		var bar = new ProgressBar();
		bar.CustomMinimumSize = new Vector2(0, 12);
		bar.MaxValue = 100;
		bar.Value = 50;
		
		var style = new StyleBoxFlat();
		style.BgColor = new Color(0.2f, 0.2f, 0.3f);
		style.SetCornerRadiusAll(3);
		bar.AddThemeStyleboxOverride("fill", style);
		
		parent.AddChild(bar);
		return bar;
	}
	
	private void AddHSeparator(Container parent)
	{
		var sep = new HSeparator();
		sep.AddThemeConstantOverride("separation", 4);
		parent.AddChild(sep);
	}
	
	private void ConnectSignals()
	{
		if (_combatSystem != null)
		{
			_combatSystem.CombatEnded += OnCombatEnded;
			_combatSystem.ComboChanged += OnComboChanged;
			_combatSystem.MilestoneReached += OnMilestoneReached;
		}
	}
	
	public override void _Process(double delta)
	{
		if (!_isVisible || _combatSystem == null) return;
		
		var stats = _combatSystem.GetCurrentStats();
		
		// Update time
		int totalSeconds = (int)stats.CombatDuration;
		int minutes = totalSeconds / 60;
		int seconds = totalSeconds % 60;
		_combatTimeLabel.Text = $"Time: {minutes:D2}:{seconds:D2}";
		
		// Update DPS
		_dpsLabel.Text = $"DPS: {stats.DamagePerSecond:F1}";
		
		// Update stats
		_damageDealtLabel.Text = $"Damage: {stats.TotalDamageDealt}";
		_damageTakenLabel.Text = $"Taken: {stats.TotalDamageTaken}";
		_healingLabel.Text = $"Healing: {stats.TotalHealingDone}";
		_enemiesKilledLabel.Text = $"Enemies: {stats.EnemiesKilled}";
		_criticalHitsLabel.Text = $"Crits: {stats.CriticalHits}";
		_comboLabel.Text = $"Combo: {stats.CurrentCombo} ({stats.MaxCombo})";
		_dodgeLabel.Text = $"Dodge: {stats.DodgeCount}";
		_blockLabel.Text = $"Block: {stats.BlockCount}";
		
		// Update combo color
		int combo = _combatSystem.GetCurrentCombo();
		if (combo >= 50) _comboLabel.Modulate = new Color(1f, 0.3f, 0.3f);
		else if (combo >= 25) _comboLabel.Modulate = new Color(1f, 0.7f, 0.3f);
		else if (combo >= 10) _comboLabel.Modulate = new Color(1f, 1f, 0.3f);
		else _comboLabel.Modulate = Colors.White;
	}
	
	private void OnCombatEnded(CombatHUDEnhancementData.CombatRating rating)
	{
		// Update rating display
		_ratingLabel.Text = $"Rating: {rating.Grade}";
		
		// Color based on grade
		switch (rating.Grade)
		{
			case "S": _ratingLabel.Modulate = new Color(1f, 0.84f, 0f); break; // Gold
			case "A": _ratingLabel.Modulate = new Color(0.3f, 1f, 0.3f); break; // Green
			case "B": _ratingLabel.Modulate = new Color(0.3f, 0.7f, 1f); break; // Blue
			case "C": _ratingLabel.Modulate = new Color(1f, 0.7f, 0.3f); break; // Orange
			default: _ratingLabel.Modulate = new Color(1f, 0.3f, 0.3f); break; // Red
		}
		
		// Update progress bars
		_efficiencyBar.Value = Math.Min(rating.DamageEfficiency * 20, 100);
		_survivalBar.Value = rating.SurvivalRate * 100;
		_skillBar.Value = Math.Min(rating.SkillUsage * 33, 100);
		_paceBar.Value = Math.Min(rating.CombatPace * 10, 100);
	}
	
	private void OnComboChanged(int newCombo)
	{
		// Combo change is handled in _Process
	}
	
	private void OnMilestoneReached(string milestone)
	{
		// Could show notification here
		GD.Print($"Combat milestone reached: {milestone}");
	}
	
	public void Toggle()
	{
		if (_isVisible)
		{
			Hide();
			_isVisible = false; 
		}
		else
		{
			Show();
			_isVisible = true;
			// Start new session when UI is opened
			if (_combatSystem != null && !_combatSystem.IsInCombat())
			{
				_combatSystem.StartNewSession();
			}
		}
	}
	
	public override void _Input(InputEvent evt)
	{
		if (evt is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.Bracketleft || keyEvent.Keycode == Key.Bracketright)
			{
				Toggle();
			}
		}
	}
}
