using Godot;
using System;
using System.Collections.Generic;

public class CombatSkillCooldownUI : Control
{
	private CombatSkillCooldownSystem _cooldownSystem;
	
	// Main container
	private PanelContainer _mainPanel;
	private VBoxContainer _mainContainer;
	
	// Title
	private Label _titleLabel;
	
	// Skill list
	private ScrollContainer _skillScroll;
	private VBoxContainer _skillList;
	
	// Statistics
	private Label _statsLabel;
	private Label _readyCountLabel;
	private Label _totalUsedLabel;
	private Label _cooldownTimeLabel;
	
	// Toggle
	private bool _isVisible = false; 
	private KeyToggleHandler _toggleHandler;
	
	// Skill slot containers
	private Dictionary<string, Control> _skillSlots = new Dictionary<string, Control>();
	
	public override void _Ready()
	{
		_cooldownSystem = CombatSkillCooldownSystem.Instance;
		
		SetupUI();
		ConnectSignals();
		Hide();
	}
	
	private void SetupUI()
	{
		// Main panel
		_mainPanel = new PanelContainer();
		_mainPanel.SetAnchor(AnchorPresets.BottomRight);
		_mainPanel.SetOffset(new Vector2(-320, -220), new Vector2(-20, -20));
		_mainPanel.CustomMinimumSize = new Vector2(280, 200);
		AddChild(_mainPanel);
		
		// Style
		var style = new StyleBoxFlat();
		style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
		style.BorderColor = new Color(0.3f, 0.3f, 0.5f, 1.0f);
		style.SetBorderWidthAll(2);
		style.SetCornerRadiusAll(8);
		_mainPanel.AddThemeStyleboxOverride("panel", style);
		
		// Main container
		_mainContainer = new VBoxContainer();
		_mainContainer.AddThemeConstantOverride("separation", 8);
		_mainPanel.AddChild(_mainContainer);
		
		// Title
		_titleLabel = new Label();
		_titleLabel.Text = "⚔ 技能冷却";
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_titleLabel.AddThemeFontSizeOverride("font_size", 18);
		_mainContainer.AddChild(_titleLabel);
		
		// Stats row
		var statsContainer = new HBoxContainer();
		statsContainer.AddThemeConstantOverride("separation", 10);
		_mainContainer.AddChild(statsContainer);
		
		_readyCountLabel = new Label();
		_readyCountLabel.Text = "就绪: 0";
		_readyCountLabel.AddThemeFontSizeOverride("font_size", 14);
		statsContainer.AddChild(_readyCountLabel);
		
		_totalUsedLabel = new Label();
		_totalUsedLabel.Text = "使用: 0";
		_totalUsedLabel.AddThemeFontSizeOverride("font_size", 14);
		statsContainer.AddChild(_totalUsedLabel);
		
		_cooldownTimeLabel = new Label();
		_cooldownTimeLabel.Text = "总冷却: 0s";
		_cooldownTimeLabel.AddThemeFontSizeOverride("font_size", 14);
		statsContainer.AddChild(_cooldownTimeLabel);
		
		// Skill scroll
		_skillScroll = new ScrollContainer();
		_skillScroll.CustomMinimumSize = new Vector2(260, 120);
		_mainContainer.AddChild(_skillScroll);
		
		// Skill list
		_skillList = new VBoxContainer();
		_skillList.AddThemeConstantOverride("separation", 4);
		_skillScroll.AddChild(_skillList);
		
		// Toggle handler
		_toggleHandler = new KeyToggleHandler();
		_toggleHandler.Initialize(this, "Toggle", KeyList.K);
	}
	
	private void ConnectSignals()
	{
		_cooldownSystem.CooldownStarted += OnCooldownStarted;
		_cooldownSystem.CooldownUpdated += OnCooldownUpdated;
		_cooldownSystem.CooldownReady += OnCooldownReady;
	}
	
	private void OnCooldownStarted(string skillId, string skillName, float cooldownTime)
	{
		UpdateSkillSlot(skillId, skillName, cooldownTime, cooldownTime);
		UpdateStatistics();
	}
	
	private void OnCooldownUpdated(string skillId, float remainingTime)
	{
		UpdateSkillProgress(skillId, remainingTime);
	}
	
	private void OnCooldownReady(string skillId, string skillName)
	{
		UpdateSkillReady(skillId);
		UpdateStatistics();
	}
	
	private void UpdateSkillSlot(string skillId, string skillName, float maxCooldown, float currentCooldown)
	{
		Control slot;
		if (_skillSlots.TryGetValue(skillId, out slot))
		{
			// Update existing slot
			UpdateSlotProgress(slot, maxCooldown, currentCooldown);
		}
		else
		{
			// Create new slot
			slot = CreateSkillSlot(skillId, skillName, maxCooldown, currentCooldown);
			_skillList.AddChild(slot);
			_skillSlots[skillId] = slot;
		}
	}
	
	private Control CreateSkillSlot(string skillId, string skillName, float maxCooldown, float currentCooldown)
	{
		var container = new HBoxContainer();
		container.CustomMinimumSize = new Vector2(250, 32);
		
		// Skill name label
		var nameLabel = new Label();
		nameLabel.Text = skillName;
		nameLabel.CustomMinimumSize = new Vector2(100, 0);
		nameLabel.AddThemeFontSizeOverride("font_size", 14);
		container.AddChild(nameLabel);
		
		// Progress bar container
		var progressContainer = new Control();
		progressContainer.CustomMinimumSize = new Vector2(100, 20);
		progressContainer.CustomMinimumSize = new Vector2(120, 20);
		container.AddChild(progressContainer);
		
		// Background
		var bg = new ColorRect();
		bg.Color = new Color(0.2f, 0.2f, 0.3f, 1.0f);
		bg.SetAnchor(AnchorPresets.FullRect);
		progressContainer.AddChild(bg);
		
		// Progress bar
		var progress = new ProgressBar();
		progress.SetAnchor(AnchorPresets.FullRect);
		progress.ShowPercentage = false; 
		progress.MinValue = 0;
		progress.MaxValue = maxCooldown;
		progress.Value = currentCooldown;
		progressContainer.AddChild(progress);
		
		// Store reference for updates
		container.SetMeta("progress", progress);
		container.SetMeta("name_label", nameLabel);
		
		// Time label
		var timeLabel = new Label();
		timeLabel.Text = $"{currentCooldown:F1}s";
		timeLabel.AddThemeFontSizeOverride("font_size", 12);
		timeLabel.CustomMinimumSize = new Vector2(40, 0);
		container.AddChild(timeLabel);
		
		container.SetMeta("time_label", timeLabel);
		
		// Ready indicator
		if (currentCooldown <= 0)
		{
			var readyLabel = new Label();
			readyLabel.Text = "✓";
			readyLabel.AddThemeFontSizeOverride("font_size", 16);
			readyLabel.AddThemeColorOverride("font_color", new Color(0.2f, 1.0f, 0.2f, 1.0f));
			container.AddChild(readyLabel);
			container.SetMeta("ready_label", readyLabel);
		}
		
		return container;
	}
	
	private void UpdateSlotProgress(Control slot, float maxCooldown, float currentCooldown)
	{
		var progress = slot.GetMetaOrDefault<ProgressBar>("progress", null);
		if (progress != null)
		{
			progress.MaxValue = maxCooldown;
			progress.Value = currentCooldown;
			
			// Update color based on cooldown
			var style = progress.GetThemeStylebox("fill") as StyleBoxFlat;
			if (style == null)
			{
				style = new StyleBoxFlat();
				progress.AddThemeStyleboxOverride("fill", style);
			}
			
			float percent = maxCooldown > 0 ? currentCooldown / maxCooldown : 0;
			if (percent > 0.5f)
			{
				style.BgColor = new Color(1.0f, 0.3f, 0.3f, 1.0f); // Red
			}
			else if (percent > 0.25f)
			{
				style.BgColor = new Color(1.0f, 0.7f, 0.2f, 1.0f); // Orange
			}
			else
			{
				style.BgColor = new Color(0.3f, 0.7f, 1.0f, 1.0f); // Blue
			}
		}
		
		var timeLabel = slot.GetMetaOrDefault<Label>("time_label", null);
		if (timeLabel != null)
		{
			timeLabel.Text = $"{currentCooldown:F1}s";
		}
	}
	
	private void UpdateSkillProgress(string skillId, float remainingTime)
	{
		Control slot;
		if (_skillSlots.TryGetValue(skillId, out slot))
		{
			var progress = slot.GetMetaOrDefault<ProgressBar>("progress", null);
			if (progress != null)
			{
				UpdateSlotProgress(slot, progress.MaxValue, remainingTime);
			}
		}
	}
	
	private void UpdateSkillReady(string skillId)
	{
		Control slot;
		if (_skillSlots.TryGetValue(skillId, out slot))
		{
			var timeLabel = slot.GetMetaOrDefault<Label>("time_label", null);
			if (timeLabel != null)
			{
				timeLabel.Text = "就绪";
			}
			
			// Add or update ready label
			var readyLabel = slot.GetMetaOrDefault<Label>("ready_label", null);
			if (readyLabel == null)
			{
				readyLabel = new Label();
				readyLabel.Text = "✓";
				readyLabel.AddThemeFontSizeOverride("font_size", 16);
				readyLabel.AddThemeColorOverride("font_color", new Color(0.2f, 1.0f, 0.2f, 1.0f));
				
				// Add to container
				var container = slot as HBoxContainer;
				if (container != null)
				{
					container.AddChild(readyLabel);
				}
				
				slot.SetMeta("ready_label", readyLabel);
			}
			else
			{
				readyLabel.Visible = true;
			}
			
			// Update progress bar color
			var progress = slot.GetMetaOrDefault<ProgressBar>("progress", null);
			if (progress != null)
			{
				var style = progress.GetThemeStylebox("fill") as StyleBoxFlat;
				if (style == null)
				{
					style = new StyleBoxFlat();
					progress.AddThemeStyleboxOverride("fill", style);
				}
				style.BgColor = new Color(0.2f, 1.0f, 0.2f, 1.0f); // Green
			}
		}
	}
	
	private void UpdateStatistics()
	{
		var stats = _cooldownSystem.GetStatistics();
		int readyCount = (int)stats["readySkills"];
		int totalUsed = (int)stats["totalSkillsUsed"];
		int totalCooldown = (int)stats["totalCooldownTime"];
		
		_readyCountLabel.Text = $"就绪: {readyCount}";
		_totalUsedLabel.Text = $"使用: {totalUsed}";
		_cooldownTimeLabel.Text = $"总冷却: {totalCooldown}s";
	}
	
	public void Toggle()
	{
		_isVisible = !_isVisible;
		
		if (_isVisible)
		{
			Show();
			UpdateAllSkills();
			UpdateStatistics();
		}
		else
		{
			Hide();
		}
	}
	
	private void UpdateAllSkills()
	{
		var cooldowns = _cooldownSystem.GetAllCooldowns();
		foreach (var kvp in cooldowns)
		{
			var cooldown = kvp.Value;
			UpdateSkillSlot(kvp.Key, cooldown.SkillName, cooldown.MaxCooldown, cooldown.CurrentCooldown);
		}
	}
	
	public override void _Process(double delta)
	{
		_cooldownSystem._Process(delta);
	}
	
	public override void _ExitTree()
	{
		_cooldownSystem.CooldownStarted -= OnCooldownStarted;
		_cooldownSystem.CooldownUpdated -= OnCooldownUpdated;
		_cooldownSystem.CooldownReady -= OnCooldownReady;
	}
}
