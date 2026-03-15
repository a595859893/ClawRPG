// Pet AI Improvements UI
// Display pet AI personality, learning, emotions and stats

using Godot;
using System;

#pragma warning disable CS8618 // Non-nullable field is uninitialized

public class PetAIImprovementsUI : Control
{
	// Signal
	public const string AiUiToggled = "ai_ui_toggled";
	
	// UI Elements
	private PanelContainer mainPanel;
	private TabContainer tabContainer;
	private VBoxContainer personalityPanel;
	private VBoxContainer behaviorPanel;
	private VBoxContainer learningPanel;
	private VBoxContainer emotionPanel;
	private VBoxContainer statsPanel;
	
	// Labels
	private Label aiLevelLabel;
	private Label personalityLabel;
	private Label emotionLabel;
	private Label stateLabel;
	private Label adaptationLabel;
	private Label winRateLabel;
	
	// System reference
	private PetAIImprovementsSystem aiSystem = null;
	
	public override void _Ready()
	{
		SetupUI();
		Visible = false;
	}
	
	private void SetupUI()
	{
		// Main panel
		mainPanel = new PanelContainer();
		mainPanel.AnchorRight = 1.0f;
		mainPanel.AnchorBottom = 1.0f;
		mainPanel.OffsetLeft = 200;
		mainPanel.OffsetTop = 100;
		mainPanel.OffsetRight = -200;
		mainPanel.OffsetBottom = -100;
		mainPanel.SetMeta("ui_type", "pet_ai_improvements");
		AddChild(mainPanel);
		
		// Style
		var style = new StyleBoxFlat();
		style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
		style.BorderColor = new Color(0.3f, 0.6f, 0.9f, 1.0f);
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(8);
		mainPanel.AddThemeStyleboxOverride("panel", style);
		
		// VBox container
		var vbox = new VBoxContainer();
		mainPanel.AddChild(vbox);
		vbox.SetAnchorsPreset(Control.Preset.FullRect);
		vbox.AddThemeConstantOverride("separation", 10);
		
		// Title
		var titleLabel = new Label();
		titleLabel.Text = "🐾 Pet AI Companion";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeFontSizeOverride("font_size", 24);
		vbox.AddChild(titleLabel);
		
		// AI Level display
		aiLevelLabel = new Label();
		aiLevelLabel.Text = "AI Level: 1";
		aiLevelLabel.HorizontalAlignment = HorizontalAlignment.Center;
		aiLevelLabel.AddThemeFontSizeOverride("font_size", 18);
		vbox.AddChild(aiLevelLabel);
		
		// Tab container
		tabContainer = new TabContainer();
		tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
		vbox.AddChild(tabContainer);
		
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
		personalityPanel = new VBoxContainer();
		personalityPanel.Name = "Personality";
		tabContainer.AddChild(personalityPanel);
		
		var title = new Label();
		title.Text = "🐶 Personality Traits";
		title.AddThemeFontSizeOverride("font_size", 18);
		personalityPanel.AddChild(title);
		
		personalityLabel = new Label();
		personalityLabel.Text = "Type: Aggressive";
		personalityPanel.AddChild(personalityLabel);
		
		var curiosityLabel = new Label();
		curiosityLabel.Name = "curiosity";
		curiosityLabel.Text = "Curiosity: 50%";
		personalityPanel.AddChild(curiosityLabel);
		
		var energyLabel = new Label();
		energyLabel.Name = "energy";
		energyLabel.Text = "Energy: 100%";
		personalityPanel.AddChild(energyLabel);
		
		var loyaltyLabel = new Label();
		loyaltyLabel.Name = "loyalty";
		loyaltyLabel.Text = "Loyalty: 50%";
		personalityPanel.AddChild(loyaltyLabel);
		
		// Personality type selector
		var selectorLabel = new Label();
		selectorLabel.Text = "\nChange Personality:";
		personalityPanel.AddChild(selectorLabel);
		
		var typeNames = new string[] { "Aggressive", "Defensive", "Supportive", "Curious", "Lazy" };
		for (int i = 0; i < typeNames.Length; i++)
		{
			int index = i;
			var btn = new Button();
			btn.Text = typeNames[i];
			btn.Pressed += () => OnPersonalitySelected(index);
			personalityPanel.AddChild(btn);
		}
	}
	
	private void SetupBehaviorTab()
	{
		behaviorPanel = new VBoxContainer();
		behaviorPanel.Name = "Behavior";
		tabContainer.AddChild(behaviorPanel);
		
		var title = new Label();
		title.Text = "🎯 Current Behavior";
		title.AddThemeFontSizeOverride("font_size", 18);
		behaviorPanel.AddChild(title);
		
		stateLabel = new Label();
		stateLabel.Text = "State: Idle";
		behaviorPanel.AddChild(stateLabel);
		
		var priorityLabel = new Label();
		priorityLabel.Name = "priority";
		priorityLabel.Text = "Priority: 0";
		behaviorPanel.AddChild(priorityLabel);
		
		var targetLabel = new Label();
		targetLabel.Name = "target";
		targetLabel.Text = "Target: None";
		behaviorPanel.AddChild(targetLabel);
	}
	
	private void SetupLearningTab()
	{
		learningPanel = new VBoxContainer();
		learningPanel.Name = "Learning";
		tabContainer.AddChild(learningPanel);
		
		var title = new Label();
		title.Text = "📚 Learning Progress";
		title.AddThemeFontSizeOverride("font_size", 18);
		learningPanel.AddChild(title);
		
		adaptationLabel = new Label();
		adaptationLabel.Text = "Adaptation: 0%";
		learningPanel.AddChild(adaptationLabel);
		
		winRateLabel = new Label();
		winRateLabel.Text = "Win Rate: 0%";
		learningPanel.AddChild(winRateLabel);
		
		var battlesLabel = new Label();
		battlesLabel.Name = "battles";
		battlesLabel.Text = "Total Battles: 0";
		learningPanel.AddChild(battlesLabel);
		
		var bestComboLabel = new Label();
		bestComboLabel.Name = "best_combo";
		bestComboLabel.Text = "Best Combo: 0";
		learningPanel.AddChild(bestComboLabel);
		
		var enemyLabel = new Label();
		enemyLabel.Name = "enemy";
		enemyLabel.Text = "Most Killed: None";
		learningPanel.AddChild(enemyLabel);
	}
	
	private void SetupEmotionTab()
	{
		emotionPanel = new VBoxContainer();
		emotionPanel.Name = "Emotion";
		tabContainer.AddChild(emotionPanel);
		
		var title = new Label();
		title.Text = "😊 Emotional State";
		title.AddThemeFontSizeOverride("font_size", 18);
		emotionPanel.AddChild(title);
		
		emotionLabel = new Label();
		emotionLabel.Text = "Current: Happy";
		emotionPanel.AddChild(emotionLabel);
		
		var intensityLabel = new Label();
		intensityLabel.Name = "intensity";
		intensityLabel.Text = "Intensity: 50%";
		emotionPanel.AddChild(intensityLabel);
		
		var historyLabel = new Label();
		historyLabel.Name = "history";
		historyLabel.Text = "Recent Emotions: None";
		emotionPanel.AddChild(historyLabel);
	}
	
	private void SetupStatsTab()
	{
		statsPanel = new VBoxContainer();
		statsPanel.Name = "Combat Stats";
		tabContainer.AddChild(statsPanel);
		
		var title = new Label();
		title.Text = "⚔️ Combat Statistics";
		title.AddThemeFontSizeOverride("font_size", 18);
		statsPanel.AddChild(title);
		
		var damageLabel = new Label();
		damageLabel.Name = "damage";
		damageLabel.Text = "Damage Dealt: 0";
		statsPanel.AddChild(damageLabel);
		
		var preventedLabel = new Label();
		preventedLabel.Name = "prevented";
		preventedLabel.Text = "Damage Prevented: 0";
		statsPanel.AddChild(preventedLabel);
		
		var healingLabel = new Label();
		healingLabel.Name = "healing";
		healingLabel.Text = "Healing Done: 0";
		statsPanel.AddChild(healingLabel);
		
		var critLabel = new Label();
		critLabel.Name = "crits";
		critLabel.Text = "Critical Hits: 0";
		statsPanel.AddChild(critLabel);
		
		var dodgeLabel = new Label();
		dodgeLabel.Name = "dodges";
		dodgeLabel.Text = "Perfect Dodges: 0";
		statsPanel.AddChild(dodgeLabel);
	}
	
	public void SetAiSystem(PetAIImprovementsSystem system)
	{
		aiSystem = system;
		UpdateDisplay();
	}
	
	public void UpdateDisplay()
	{
		if (aiSystem == null)
		{
			return;
		}
		
		// Update personality
		if (aiSystem.Data != null && aiSystem.Data.Personality != null)
		{
			personalityLabel.Text = "Type: " + aiSystem.GetPersonalityType();
		}
		
		// Update AI level
		aiLevelLabel.Text = "AI Level: " + aiSystem.GetAiLevel().ToString();
		
		// Update behavior state
		stateLabel.Text = "State: " + aiSystem.GetAiState();
		
		// Update emotion
		emotionLabel.Text = "Current: " + aiSystem.GetCurrentEmotion();
		
		// Update learning stats
		var learningStats = aiSystem.GetLearningStats();
		adaptationLabel.Text = $"Adaptation: {Mathf.Round((float)learningStats["adaptation_level"] * 100)}%";
		winRateLabel.Text = $"Win Rate: {Mathf.Round((float)learningStats["win_rate"] * 100)}%";
		
		// Update combat stats
		var combatStats = aiSystem.GetCombatStats();
		var damageLabel = statsPanel.GetNode("damage") as Label;
		if (damageLabel != null)
		{
			damageLabel.Text = $"Damage Dealt: {Mathf.Round((float)combatStats["total_damage_dealt"])}";
		}
		var preventedLabel = statsPanel.GetNode("prevented") as Label;
		if (preventedLabel != null)
		{
			preventedLabel.Text = $"Damage Prevented: {Mathf.Round((float)combatStats["total_damage_prevented"])}";
		}
		var healingLabel = statsPanel.GetNode("healing") as Label;
		if (healingLabel != null)
		{
			healingLabel.Text = $"Healing Done: {Mathf.Round((float)combatStats["total_healing_done"])}";
		}
		var critLabel = statsPanel.GetNode("crits") as Label;
		if (critLabel != null)
		{
			critLabel.Text = $"Critical Hits: {combatStats["critical_hits"]}";
		}
		var dodgeLabel = statsPanel.GetNode("dodges") as Label;
		if (dodgeLabel != null)
		{
			dodgeLabel.Text = $"Perfect Dodges: {combatStats["perfect_dodges"]}";
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
		EmitSignal(AiUiToggled, Visible);
		if (Visible && aiSystem != null)
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
		if (aiSystem != null)
		{
			aiSystem.SetPersonalityType(type);
			personalityLabel.Text = "Type: " + aiSystem.GetPersonalityType();
		}
	}
}
