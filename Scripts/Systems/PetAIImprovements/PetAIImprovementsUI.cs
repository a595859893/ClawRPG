// Pet AI Improvements UI
// Display pet AI personality, learning, emotions and stats

using Godot;
using System;

#pragma warning disable CS8618 // Non-nullable field is uninitialized

public partial class PetAIImprovementsUI : Control
{
	// 信号定义
	[Signal]
	public event Action<bool> AiUiToggled;

	// UI Elements
	private PanelContainer _mainPanel = null!;
	private TabContainer _tabContainer = null!;
	private VBoxContainer _personalityPanel = null!;
	private VBoxContainer _behaviorPanel = null!;
	private VBoxContainer _learningPanel = null!;
	private VBoxContainer _emotionPanel = null!;
	private VBoxContainer _statsPanel = null!;

	// Labels
	private Label _aiLevelLabel = null!;
	private Label _personalityLabel = null!;
	private Label _emotionLabel = null!;
	private Label _stateLabel = null!;
	private Label _adaptationLabel = null!;
	private Label _winRateLabel = null!;

	// System reference
	[Export] public PetAIImprovementsSystem? AiSystem { get; set; }

	public override void _Ready()
	{
		SetupUi();
		Visible = false;
	}

	private void SetupUi()
	{
		// Main panel
		_mainPanel = new PanelContainer();
		_mainPanel.AnchorRight = 1.0f;
		_mainPanel.AnchorBottom = 1.0f;
		_mainPanel.OffsetLeft = 200;
		_mainPanel.OffsetTop = 100;
		_mainPanel.OffsetRight = -200;
		_mainPanel.OffsetBottom = -100;
		_mainPanel.SetMeta("ui_type", "pet_ai_improvements");
		AddChild(_mainPanel);
		
		// Style
		var style = new StyleBoxFlat();
		style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
		style.BorderColor = new Color(0.3f, 0.6f, 0.9f, 1.0f);
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(8);
		_mainPanel.AddThemeStyleboxOverride("panel", style);
		
		// VBox container
		var vbox = new VBoxContainer();
		_mainPanel.AddChild(vbox);
		vbox.SetAnchorsPreset(Control.Preset.FullRect);
		vbox.AddThemeConstantOverride("separation", 10);
		
		// Title
		var titleLabel = new Label();
		titleLabel.Text = "🐾 Pet AI Companion";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeFontSizeOverride("font_size", 24);
		vbox.AddChild(titleLabel);
		
		// AI Level display
		_aiLevelLabel = new Label();
		_aiLevelLabel.Text = "AI Level: 1";
		_aiLevelLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_aiLevelLabel.AddThemeFontSizeOverride("font_size", 18);
		vbox.AddChild(_aiLevelLabel);
		
		// Tab container
		_tabContainer = new TabContainer();
		_tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		vbox.AddChild(_tabContainer);
		
		// Create tabs
		SetupPersonalityTab();
		SetupBehaviorTab();
		SetupLearningTab();
		SetupEmotionTab();
		SetupStatsTab();
		
		// Close button
		var closeButton = new Button();
		closeButton.Text = "Close";
		closeButton.Pressed += OnClosePressed;
		vbox.AddChild(closeButton);
	}

	private void SetupPersonalityTab()
	{
		_personalityPanel = new VBoxContainer();
		_personalityPanel.Name = "Personality";
		_tabContainer.AddChild(_personalityPanel);
		
		var title = new Label();
		title.Text = "🐶 Personality Traits";
		title.AddThemeFontSizeOverride("font_size", 18);
		_personalityPanel.AddChild(title);
		
		_personalityLabel = new Label();
		_personalityLabel.Text = "Type: Aggressive";
		_personalityPanel.AddChild(_personalityLabel);
		
		var curiosityLabel = new Label();
		curiosityLabel.Name = "curiosity";
		curiosityLabel.Text = "Curiosity: 50%";
		_personalityPanel.AddChild(curiosityLabel);
		
		var energyLabel = new Label();
		energyLabel.Name = "energy";
		energyLabel.Text = "Energy: 100%";
		_personalityPanel.AddChild(energyLabel);
		
		var loyaltyLabel = new Label();
		loyaltyLabel.Name = "loyalty";
		loyaltyLabel.Text = "Loyalty: 50%";
		_personalityPanel.AddChild(loyaltyLabel);
		
		// Personality type selector
		var selectorLabel = new Label();
		selectorLabel.Text = "\nChange Personality:";
		_personalityPanel.AddChild(selectorLabel);
		
		var typeNames = new string[] { "Aggressive", "Defensive", "Supportive", "Curious", "Lazy" };
		for (int i = 0; i < typeNames.Length; i++)
		{
			var btn = new Button();
			btn.Text = typeNames[i];
			var index = i;
			btn.Pressed += () => OnPersonalitySelected(index);
			_personalityPanel.AddChild(btn);
		}
	}

	private void SetupBehaviorTab()
	{
		_behaviorPanel = new VBoxContainer();
		_behaviorPanel.Name = "Behavior";
		_tabContainer.AddChild(_behaviorPanel);
		
		var title = new Label();
		title.Text = "🎯 Current Behavior";
		title.AddThemeFontSizeOverride("font_size", 18);
		_behaviorPanel.AddChild(title);
		
		_stateLabel = new Label();
		_stateLabel.Text = "State: Idle";
		_behaviorPanel.AddChild(_stateLabel);
		
		var priorityLabel = new Label();
		priorityLabel.Name = "priority";
		priorityLabel.Text = "Priority: 0";
		_behaviorPanel.AddChild(priorityLabel);
		
		var targetLabel = new Label();
		targetLabel.Name = "target";
		targetLabel.Text = "Target: None";
		_behaviorPanel.AddChild(targetLabel);
	}

	private void SetupLearningTab()
	{
		_learningPanel = new VBoxContainer();
		_learningPanel.Name = "Learning";
		_tabContainer.AddChild(_learningPanel);
		
		var title = new Label();
		title.Text = "📚 Learning Progress";
		title.AddThemeFontSizeOverride("font_size", 18);
		_learningPanel.AddChild(title);
		
		_adaptationLabel = new Label();
		_adaptationLabel.Text = "Adaptation: 0%";
		_learningPanel.AddChild(_adaptationLabel);
		
		_winRateLabel = new Label();
		_winRateLabel.Text = "Win Rate: 0%";
		_learningPanel.AddChild(_winRateLabel);
		
		var battlesLabel = new Label();
		battlesLabel.Name = "battles";
		battlesLabel.Text = "Total Battles: 0";
		_learningPanel.AddChild(battlesLabel);
		
		var bestComboLabel = new Label();
		bestComboLabel.Name = "best_combo";
		bestComboLabel.Text = "Best Combo: 0";
		_learningPanel.AddChild(bestComboLabel);
		
		var enemyLabel = new Label();
		enemyLabel.Name = "enemy";
		enemyLabel.Text = "Most Killed: None";
		_learningPanel.AddChild(enemyLabel);
	}

	private void SetupEmotionTab()
	{
		_emotionPanel = new VBoxContainer();
		_emotionPanel.Name = "Emotion";
		_tabContainer.AddChild(_emotionPanel);
		
		var title = new Label();
		title.Text = "😊 Emotional State";
		title.AddThemeFontSizeOverride("font_size", 18);
		_emotionPanel.AddChild(title);
		
		_emotionLabel = new Label();
		_emotionLabel.Text = "Current: Happy";
		_emotionPanel.AddChild(_emotionLabel);
		
		var intensityLabel = new Label();
		intensityLabel.Name = "intensity";
		intensityLabel.Text = "Intensity: 50%";
		_emotionPanel.AddChild(intensityLabel);
		
		var historyLabel = new Label();
		historyLabel.Name = "history";
		historyLabel.Text = "Recent Emotions: None";
		_emotionPanel.AddChild(historyLabel);
	}

	private void SetupStatsTab()
	{
		_statsPanel = new VBoxContainer();
		_statsPanel.Name = "Combat Stats";
		_tabContainer.AddChild(_statsPanel);
		
		var title = new Label();
		title.Text = "⚔️ Combat Statistics";
		title.AddThemeFontSizeOverride("font_size", 18);
		_statsPanel.AddChild(title);
		
		var damageLabel = new Label();
		damageLabel.Name = "damage";
		damageLabel.Text = "Damage Dealt: 0";
		_statsPanel.AddChild(damageLabel);
		
		var preventedLabel = new Label();
		preventedLabel.Name = "prevented";
		preventedLabel.Text = "Damage Prevented: 0";
		_statsPanel.AddChild(preventedLabel);
		
		var healingLabel = new Label();
		healingLabel.Name = "healing";
		healingLabel.Text = "Healing Done: 0";
		_statsPanel.AddChild(healingLabel);
		
		var critLabel = new Label();
		critLabel.Name = "crits";
		critLabel.Text = "Critical Hits: 0";
		_statsPanel.AddChild(critLabel);
		
		var dodgeLabel = new Label();
		dodgeLabel.Name = "dodges";
		dodgeLabel.Text = "Perfect Dodges: 0";
		_statsPanel.AddChild(dodgeLabel);
	}

	public void SetAiSystem(PetAIImprovementsSystem system)
	{
		AiSystem = system;
		UpdateDisplay();
	}

	public void UpdateDisplay()
	{
		if (AiSystem == null)
		{
			return;
		}
		
		// Update personality
		if (AiSystem.Data != null && AiSystem.Data.Personality != null)
		{
			_personalityLabel.Text = "Type: " + AiSystem.GetPersonalityType();
		}
		
		// Update AI level
		_aiLevelLabel.Text = "AI Level: " + AiSystem.GetAiLevel();
		
		// Update behavior state
		_stateLabel.Text = "State: " + AiSystem.GetAiState();
		
		// Update emotion
		_emotionLabel.Text = "Current: " + AiSystem.GetCurrentEmotion();
		
		// Update learning stats
		var learningStats = AiSystem.GetLearningStats();
		_adaptationLabel.Text = "Adaptation: " + Mathf.Round((float)learningStats["adaptation_level"] * 100) + "%";
		_winRateLabel.Text = "Win Rate: " + Mathf.Round((float)learningStats["win_rate"] * 100) + "%";
		
		// Update combat stats
		var combatStats = AiSystem.GetCombatStats();
		var damageLabel = _statsPanel.GetNode("damage");
		if (damageLabel != null)
		{
			(damageLabel as Label)!.Text = "Damage Dealt: " + Mathf.Round((float)combatStats["total_damage_dealt"]);
		}
		var preventedLabel = _statsPanel.GetNode("prevented");
		if (preventedLabel != null)
		{
			(preventedLabel as Label)!.Text = "Damage Prevented: " + Mathf.Round((float)combatStats["total_damage_prevented"]);
		}
		var healingLabel = _statsPanel.GetNode("healing");
		if (healingLabel != null)
		{
			(healingLabel as Label)!.Text = "Healing Done: " + Mathf.Round((float)combatStats["total_healing_done"]);
		}
		var critLabel = _statsPanel.GetNode("crits");
		if (critLabel != null)
		{
			(critLabel as Label)!.Text = "Critical Hits: " + combatStats["critical_hits"];
		}
		var dodgeLabel = _statsPanel.GetNode("dodges");
		if (dodgeLabel != null)
		{
			(dodgeLabel as Label)!.Text = "Perfect Dodges: " + combatStats["perfect_dodges"];
		}
	}

	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("pet_ai_toggle"))
		{
			Toggle();
			GetViewport().SetInputAsHandled();
		}
	}

	public void Toggle()
	{
		Visible = !Visible;
		AiUiToggled?.Invoke(Visible);
		if (Visible && AiSystem != null)
		{
			UpdateDisplay();
		}
	}

	private void OnClosePressed()
	{
		Visible = false;
	}

	private void OnPersonalitySelected(int type)
	{
		if (AiSystem != null)
		{
			AiSystem.SetPersonalityType(type);
			_personalityLabel.Text = "Type: " + AiSystem.GetPersonalityType();
		}
	}
}
