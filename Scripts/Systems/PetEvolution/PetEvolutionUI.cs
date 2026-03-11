using Godot;
using System;
using System.Collections.Generic;

public class PetEvolutionUI : Control
{
	// UI Components
	private Label _titleLabel;
	private HBoxContainer _petContainer;
	private VBoxContainer _detailPanel;
	private Label _petNameLabel;
	private Label _petStageLabel;
	private Label _petTypeLabel;
	private ProgressBar _progressBar;
	private Label _progressLabel;
	private Label _statsLabel;
	private Label _requirementLabel;
	private Button _evolveButton;
	private Label _infoLabel;
	
	// Selected pet index
	private int _selectedPetIndex = 0;
	
	// UI Theme
	private Color _commonColor = new Color(0.7f, 0.7f, 0.7f);
	private Color _uncommonColor = new Color(0.2f, 0.8f, 0.2f);
	private Color _rareColor = new Color(0.2f, 0.5f, 1.0f);
	private Color _epicColor = new Color(0.6f, 0.3f, 0.9f);
	private Color _legendaryColor = new Color(1.0f, 0.6f, 0.0f);
	
	public override void _Ready()
	{
		Visible = false;
		SetProcess(false);
		SetupUI();
	}
	
	private void SetupUI()
	{
		// Main container
		Panel mainPanel = new Panel();
		mainPanel.SetAnchor(AnchorPreset.Center);
		mainPanel.CustomMinimumSize = new Vector2(900, 600);
		mainPanel.Position = new Vector2(-450, -300);
		AddChild(mainPanel);
		
		var mainStyle = new StyleBoxFlat();
		mainStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
		mainStyle.SetCornerRadiusAll(8);
		mainStyle.SetBorderWidthAll(2);
		mainStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f);
		mainPanel.AddThemeStyleboxOverride("panel", mainStyle);
		
		// Title
		_titleLabel = new Label();
		_titleLabel.Text = "  🐾 宠物进化系统";
		_titleLabel.SetAnchor(AnchorPreset.TopWide);
		_titleLabel.OffsetTop = 10;
		_titleLabel.OffsetLeft = 20;
		_titleLabel.OffsetRight = -20;
		_titleLabel.AddThemeFontSizeOverride("font_size", 24);
		_titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f));
		mainPanel.AddChild(_titleLabel);
		
		// Close button
		Button closeBtn = new Button();
		closeBtn.Text = "✕";
		closeBtn.SetAnchor(AnchorPreset.TopRight);
		closeBtn.OffsetLeft = -50;
		closeBtn.OffsetTop = 10;
		closeBtn.OffsetRight = -20;
		closeBtn.OffsetBottom = 40;
		closeBtn.CustomMinimumSize = new Vector2(30, 30);
		closeBtn.Pressed += () => HideUI();
		mainPanel.AddChild(closeBtn);
		
		// Pet list container
		_petContainer = new HBoxContainer();
		_petContainer.SetAnchor(AnchorPreset.TopLeft);
		_petContainer.OffsetTop = 60;
		_petContainer.OffsetLeft = 20;
		_petContainer.OffsetRight = 320;
		_petContainer.OffsetBottom = -20;
		mainPanel.AddChild(_petContainer);
		
		// Create pet list (placeholder - will be populated from PetSystem)
		CreatePetList();
		
		// Detail panel
		_detailPanel = new VBoxContainer();
		_detailPanel.SetAnchor(AnchorPreset.FullRect);
		_detailPanel.OffsetTop = 60;
		_detailPanel.OffsetLeft = 340;
		_detailPanel.OffsetRight = -20;
		_detailPanel.OffsetBottom = -20;
		_detailPanel.AddThemeConstantOverride("separation", 10);
		mainPanel.AddChild(_detailPanel);
		
		// Pet info
		_petNameLabel = new Label();
		_petNameLabel.AddThemeFontSizeOverride("font_size", 28);
		_petNameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.8f));
		_detailPanel.AddChild(_petNameLabel);
		
		_petStageLabel = new Label();
		_petStageLabel.AddThemeFontSizeOverride("font_size", 18);
		_detailPanel.AddChild(_petStageLabel);
		
		_petTypeLabel = new Label();
		_petTypeLabel.AddThemeFontSizeOverride("font_size", 16);
		_detailPanel.AddChild(_petTypeLabel);
		
		// Progress
		_progressLabel = new Label();
		_progressLabel.Text = "进化进度";
		_progressLabel.AddThemeFontSizeOverride("font_size", 14);
		_progressLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
		_detailPanel.AddChild(_progressLabel);
		
		_progressBar = new ProgressBar();
		_progressBar.CustomMinimumSize = new Vector2(0, 20);
		var progressStyle = new StyleBoxFlat();
		progressStyle.BgColor = new Color(0.2f, 0.2f, 0.3f);
		progressStyle.SetCornerRadiusAll(4);
		_progressBar.AddThemeStyleboxOverride("background", progressStyle);
		var progressFill = new StyleBoxFlat();
		progressFill.BgColor = new Color(0.2f, 0.6f, 1.0f);
		progressFill.SetCornerRadiusAll(4);
		_progressBar.AddThemeStyleboxOverride("fill", progressFill);
		_detailPanel.AddChild(_progressBar);
		
		// Stats
		_statsLabel = new Label();
		_statsLabel.AddThemeFontSizeOverride("font_size", 14);
		_statsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
		_detailPanel.AddChild(_statsLabel);
		
		// Requirement
		_requirementLabel = new Label();
		_requirementLabel.AddThemeFontSizeOverride("font_size", 14);
		_requirementLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.6f, 0.4f));
		_detailPanel.AddChild(_requirementLabel);
		
		// Evolve button
		_evolveButton = new Button();
		_evolveButton.Text = "  🔄 进化宠物 ";
		_evolveButton.CustomMinimumSize = new Vector2(200, 45);
		_evolveButton.AddThemeFontSizeOverride("font_size", 18);
		_evolveButton.Pressed += OnEvolvePressed;
		
		var btnNormal = new StyleBoxFlat();
		btnNormal.BgColor = new Color(0.2f, 0.5f, 0.3f);
		btnNormal.SetCornerRadiusAll(6);
		_evolveButton.AddThemeStyleboxOverride("normal", btnNormal);
		
		var btnHover = new StyleBoxFlat();
		btnHover.BgColor = new Color(0.3f, 0.7f, 0.4f);
		btnHover.SetCornerRadiusAll(6);
		_evolveButton.AddThemeStyleboxOverride("hover", btnHover);
		
		_detailPanel.AddChild(_evolveButton);
		
		// Info
		_infoLabel = new Label();
		_infoLabel.Text = "提示：宠物在战斗中获得经验，达到要求后可进化";
		_infoLabel.AddThemeFontSizeOverride("font_size", 12);
		_infoLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
		_detailPanel.AddChild(_infoLabel);
		
		// Populate initial data
		RefreshUI();
	}
	
	private void CreatePetList()
	{
		// Clear existing
		foreach (var child in _petContainer.GetChildren())
		{
			child.QueueFree();
		}
		
		// This would normally get pets from PetSystem
		// For now, create sample pet buttons
		string[] samplePets = { "狼", "熊", "鹰", "狐狸", "龙" };
		
		for (int i = 0; i < samplePets.Length; i++)
		{
			CreatePetButton(i, samplePets[i]);
		}
	}
	
	private void CreatePetButton(int index, string name)
	{
		Button petBtn = new Button();
		petBtn.Text = "  🐾 " + name;
		petBtn.CustomMinimumSize = new Vector2(120, 50);
		petBtn.Pressed += () => OnPetSelected(index);
		
		var btnNormal = new StyleBoxFlat();
		btnNormal.BgColor = new Color(0.2f, 0.2f, 0.25f);
		btnNormal.SetCornerRadiusAll(6);
		btnNormal.SetBorderWidthAll(1);
		btnNormal.BorderColor = new Color(0.3f, 0.3f, 0.35f);
		petBtn.AddThemeStyleboxOverride("normal", btnNormal);
		
		var btnHover = new StyleBoxFlat();
		btnHover.BgColor = new Color(0.3f, 0.3f, 0.4f);
		btnHover.SetCornerRadiusAll(6);
		btnHover.SetBorderWidthAll(2);
		btnHover.BorderColor = new Color(0.5f, 0.5f, 0.6f);
		petBtn.AddThemeStyleboxOverride("hover", btnHover);
		
		var btnPressed = new StyleBoxFlat();
		btnPressed.BgColor = new Color(0.15f, 0.4f, 0.3f);
		btnPressed.SetCornerRadiusAll(6);
		btnPressed.SetBorderWidthAll(2);
		btnPressed.BorderColor = new Color(0.3f, 0.7f, 0.5f);
		petBtn.AddThemeStyleboxOverride("pressed", btnPressed);
		
		_petContainer.AddChild(petBtn);
	}
	
	private void OnPetSelected(int index)
	{
		_selectedPetIndex = index;
		RefreshUI();
	}
	
	private void RefreshUI()
	{
		var stats = PetEvolutionSystem.Instance.GetPetEvolutionStats(_selectedPetIndex);
		
		if (!(bool)stats["exists"])
		{
			// Initialize pet if not exists
			string[] basePets = { "wolf", "bear", "eagle", "fox", "dragon" };
			PetEvolutionSystem.Instance.InitializePet(_selectedPetIndex, basePets[_selectedPetIndex]);
			stats = PetEvolutionSystem.Instance.GetPetEvolutionStats(_selectedPetIndex);
		}
		
		_petNameLabel.Text = (string)stats["display_name"];
		
		// Stage color
		string stage = (string)stats["stage"];
		_petStageLabel.Text = "阶段: " + GetStageName(stage);
		_petStageLabel.AddThemeColorOverride("font_color", GetStageColor(stage));
		
		// Type color
		string type = (string)stats["type"];
		_petTypeLabel.Text = "类型: " + GetTypeName(type);
		_petTypeLabel.AddThemeColorOverride("font_color", GetTypeColor(type));
		
		// Progress
		float progress = (float)stats["progress"];
		_progressBar.Value = progress * 100;
		
		// Stats
		int attack = (int)stats["base_attack"];
		int defense = (int)stats["base_defense"];
		int health = (int)stats["base_health"];
		int speed = (int)stats["base_speed"];
		
		_statsLabel.Text = $"\n📊 基础属性:\n  ⚔️ 攻击力: {attack}\n  🛡️ 防御力: {defense}\n  ❤️ 生命值: {health}\n  💨 速度: {speed}";
		
		// Requirements
		if (stats.ContainsKey("next_evolution"))
		{
			string nextName = (string)stats["next_evolution"];
			int reqExp = (int)stats["required_battle_exp"];
			int reqKills = (int)stats["required_kills"];
			int reqItems = (int)stats["required_items"];
			int curExp = (int)stats["battle_exp"];
			int curKills = (int)stats["total_kills"];
			int curItems = (int)stats["evolution_items"];
			
			_requirementLabel.Text = $"\n📋 进化要求 ({nextName}):\n  ⚔️ 战斗经验: {curExp}/{reqExp}\n  💀 击杀数: {curKills}/{reqKills}\n  💎 进化石: {curItems}/{reqItems}";
			
			_evolveButton.Disabled = !(bool)stats["is_max_stage"];
			if ((bool)stats["is_max_stage"])
			{
				_evolveButton.Text = "  ⭐ 已达最高阶段 ";
			}
			else if (curExp >= reqExp && curKills >= reqKills && curItems >= reqItems)
			{
				_evolveButton.Text = "  🔄 进化宠物 ";
				_evolveButton.Disabled = false;
			}
			else
			{
				_evolveButton.Text = "  ⏳ 尚未满足条件 ";
				_evolveButton.Disabled = true;
			}
		}
		else
		{
			_requirementLabel.Text = "\n⭐ 已达到最高进化阶段！";
			_evolveButton.Text = "  ⭐ 已达最高阶段 ";
			_evolveButton.Disabled = true;
		}
	}
	
	private void OnEvolvePressed()
	{
		// Show evolution type selection dialog
		ShowEvolutionTypeDialog();
	}
	
	private void ShowEvolutionTypeDialog()
	{
		var options = PetEvolutionSystem.Instance.GetAvailableEvolutionOptions(_selectedPetIndex);
		
		if (options.Count == 0)
		{
			GD.Print("[PetEvolutionUI] No evolution options available");
			return;
		}
		
		// For simplicity, auto-evolve to first available type
		var firstOption = options[0];
		if (PetEvolutionSystem.Instance.TryEvolve(_selectedPetIndex, firstOption.Type))
		{
			RefreshUI();
		}
	}
	
	private string GetStageName(string stage)
	{
		switch (stage)
		{
			case "Basic": return "基础";
			case "Advanced": return "进阶";
			case "Elite": return "精英";
			case "Epic": return "史诗";
			case "Legendary": return "传说";
			default: return stage;
		}
	}
	
	private Color GetStageColor(string stage)
	{
		switch (stage)
		{
			case "Basic": return _commonColor;
			case "Advanced": return _uncommonColor;
			case "Elite": return _rareColor;
			case "Epic": return _epicColor;
			case "Legendary": return _legendaryColor;
			default: return Color.White;
		}
	}
	
	private string GetTypeName(string type)
	{
		switch (type)
		{
			case "Fire": return "🔥 火焰";
			case "Ice": return "❄️ 冰霜";
			case "Lightning": return "⚡ 闪电";
			case "Dark": return "🌑 黑暗";
			case "Holy": return "✨ 神圣";
			case "Nature": return "🌿 自然";
			default: return type;
		}
	}
	
	private Color GetTypeColor(string type)
	{
		switch (type)
		{
			case "Fire": return new Color(1f, 0.4f, 0.2f);
			case "Ice": return new Color(0.6f, 0.85f, 1f);
			case "Lightning": return new Color(1f, 1f, 0.3f);
			case "Dark": return new Color(0.4f, 0.3f, 0.5f);
			case "Holy": return new Color(1f, 0.95f, 0.6f);
			case "Nature": return new Color(0.4f, 0.8f, 0.4f);
			default: return Color.White;
		}
	}
	
	public void ShowUI()
	{
		Visible = true;
		SetProcess(true);
		RefreshUI();
	}
	
	public void HideUI()
	{
		Visible = false;
		SetProcess(false);
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			HideUI();
		}
	}
}
