// Combo Skill UI - 连击技能系统界面
// 连击技能系统用户界面

using Godot;
using System;

#pragma warning disable CS8618 // Non-nullable field is uninitialized

public partial class ComboSkillUI : Control
{
	// 关联的连击系统
	[Export] public ComboSkillSystem ComboSystem { get; set; }

	private GridContainer _comboList = null!;
	private VBoxContainer _detailPanel = null!;
	private HBoxContainer _equippedPanel = null!;
	private string _selectedComboId = "";
	
	private Key _toggleKey = Key.J;

	public override void _Ready()
	{
		// ComboSystem = ComboSkillSystem.GetInstance();
		// Note: 上面的连接需要在外部设置，因为是单例模式
		// if (ComboSystem != null)
		// {
		//     ComboSystem.ComboUnlocked += OnComboUnlocked;
		//     ComboSystem.ComboExecuted += OnComboExecuted;
		//     ComboSystem.CooldownUpdated += OnCooldownUpdated;
		// }
		
		Visible = false;
		SetupUi();
	}

	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == _toggleKey)
			{
				Toggle();
			}
			else if (keyEvent.Keycode == Key.Escape && Visible)
			{
				Visible = false;
			}
		}
	}

	public void Toggle()
	{
		Visible = !Visible;
		if (Visible)
		{
			Refresh();
		}
	}

	public void Refresh()
	{
		if (ComboSystem == null)
		{
			return;
		}
		UpdateComboList();
		UpdateEquippedPanel();
		UpdateDetailPanel();
	}

	// ============ UI构建 ============

	private void SetupUi()
	{
		// 主容器
		var mainVbox = new VBoxContainer();
		mainVbox.SetAnchorsPreset(Control.Preset.FullRect);
		mainVbox.AddThemeConstantOverride("separation", 10);
		AddChild(mainVbox);
		
		// 标题栏
		var titleBar = new HBoxContainer();
		mainVbox.AddChild(titleBar);
		
		var title = new Label();
		title.Text = "连击技能系统";
		title.AddThemeFontSizeOverride("font_size", 24);
		titleBar.AddChild(title);
		
		titleBar.AddChild(new Control());
		
		var closeBtn = new Button();
		closeBtn.Text = "×";
		closeBtn.Pressed += () => Visible = false;
		titleBar.AddChild(closeBtn);
		
		// 装备栏
		var equippedLabel = new Label();
		equippedLabel.Text = "已装备连击 (按快捷键触发)";
		equippedLabel.AddThemeFontSizeOverride("font_size", 16);
		mainVbox.AddChild(equippedLabel);
		
		_equippedPanel = new HBoxContainer();
		_equippedPanel.AddThemeConstantOverride("separation", 10);
		mainVbox.AddChild(_equippedPanel);
		
		// 内容区域
		var contentHbox = new HBoxContainer();
		contentHbox.SetVSizeFlags(Control.SizeFlags.ExpandFill);
		mainVbox.AddChild(contentHbox);
		
		// 左侧：连击列表
		var listPanel = new PanelContainer();
		listPanel.CustomMinimumSize = new Vector2(350, 0);
		contentHbox.AddChild(listPanel);
		
		var listScroll = new ScrollContainer();
		listPanel.AddChild(listScroll);
		
		_comboList = new GridContainer();
		_comboList.Columns = 2;
		_comboList.AddThemeConstantOverride("hseparation", 5);
		_comboList.AddThemeConstantOverride("vseparation", 5);
		listScroll.AddChild(_comboList);
		
		// 右侧：详情面板
		_detailPanel = new VBoxContainer();
		_detailPanel.SetHSizeFlags(Control.SizeFlags.ExpandFill);
		contentHbox.AddChild(_detailPanel);
		SetupDetailPanel();
	}

	private void SetupDetailPanel()
	{
		var title = new Label();
		title.Text = "连击详情";
		title.AddThemeFontSizeOverride("font_size", 18);
		_detailPanel.AddChild(title);
		
		var nameLabel = new Label();
		nameLabel.Name = "name_label";
		nameLabel.AddThemeFontSizeOverride("font_size", 20);
		_detailPanel.AddChild(nameLabel);
		
		var typeLabel = new Label();
		typeLabel.Name = "type_label";
		_detailPanel.AddChild(typeLabel);
		
		var descLabel = new Label();
		descLabel.Name = "desc_label";
		descLabel.AutowrapMode = TextServer.Autowrap.Word;
		_detailPanel.AddChild(descLabel);
		
		var statsLabel = new Label();
		statsLabel.Name = "stats_label";
		_detailPanel.AddChild(statsLabel);
		
		var stepsLabel = new Label();
		stepsLabel.Name = "steps_label";
		stepsLabel.AutowrapMode = TextServer.Autowrap.Word;
		_detailPanel.AddChild(stepsLabel);
		
		var buttonHbox = new HBoxContainer();
		_detailPanel.AddChild(buttonHbox);
		
		var equipBtn = new Button();
		equipBtn.Name = "equip_btn";
		equipBtn.Text = "装备";
		equipBtn.Pressed += OnEquipPressed;
		buttonHbox.AddChild(equipBtn);
		
		var executeBtn = new Button();
		executeBtn.Name = "execute_btn";
		executeBtn.Text = "执行";
		executeBtn.Pressed += OnExecutePressed;
		buttonHbox.AddChild(executeBtn);
	}

	// ============ 数据更新 ============

	private void UpdateComboList()
	{
		// 清除旧项目
		foreach (var child in _comboList.GetChildren())
		{
			child.QueueFree();
		}
		
		var unlocked = ComboSystem.GetUnlockedCombos();
		
		foreach (var comboId in unlocked)
		{
			var combo = ComboSkillDatabase.Instance().GetCombo(comboId);
			if (combo == null)
			{
				continue;
			}
			
			var btn = new Button();
			btn.CustomMinimumSize = new Vector2(160, 50);
			btn.Text = combo.Name;
			btn.TooltipText = $"{combo.Description}\n{GetComboTypeName(combo.ComboType)}";
			
			// 稀有度颜色
			var color = ComboSkillDatabase.Instance().GetRarityColor(combo.Rarity);
			var style = new StyleBoxFlat();
			style.BgColor = color.Darkened(0.7f);
			style.BorderColor = color;
			style.BorderWidthLeft = 2;
			style.BorderWidthTop = 2;
			style.BorderWidthRight = 2;
			style.BorderWidthBottom = 2;
			btn.AddThemeStyleboxOverride("normal", style);
			
			// 选中状态
			if (comboId == _selectedComboId)
			{
				btn.Modulate = Color.Yellow;
			}
			
			// 冷却状态
			if (ComboSystem.IsOnCooldown(comboId))
			{
				btn.Disabled = true;
			}
			
			var comboIdCopy = comboId; // 闭包捕获
			btn.Pressed += () =>
			{
				_selectedComboId = comboIdCopy;
				UpdateComboList();
				UpdateDetailPanel();
			};
			
			_comboList.AddChild(btn);
		}
		
		// 显示未解锁提示
		if (unlocked.Count == 0)
		{
			var emptyLabel = new Label();
			emptyLabel.Text = "暂无解锁的连击技能\n通过升级或完成任务解锁";
			emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
			_comboList.AddChild(emptyLabel);
		}
	}

	private void UpdateEquippedPanel()
	{
		foreach (var child in _equippedPanel.GetChildren())
		{
			child.QueueFree();
		}
		
		var equipped = ComboSystem.GetEquippedCombos();
		
		for (int i = 0; i < 5; i++)
		{
			var slot = new VBoxContainer();
			slot.CustomMinimumSize = new Vector2(80, 80);
			
			var btn = new Button();
			btn.CustomMinimumSize = new Vector2(70, 70);
			btn.Text = "";
			
			if (i < equipped.Count)
			{
				var comboId = equipped[i].ComboId;
				var combo = ComboSkillDatabase.Instance().GetCombo(comboId);
				if (combo != null)
				{
					btn.Text = combo.Name.Substring(0, Math.Min(2, combo.Name.Length));
					btn.TooltipText = $"{combo.Name}\n冷却: {ComboSystem.GetCooldown(comboId):F1}s";
					
					// 冷却显示
					var cooldown = ComboSystem.GetCooldown(comboId);
					if (cooldown > 0)
					{
						var progress = new StyleBoxFlat();
						progress.BgColor = new Color(0, 0, 0, 0.5f);
						var height = 70 * (cooldown / combo.Cooldown);
						progress.ContentMarginTop = 70 - height;
						btn.AddThemeStyleboxOverride("normal", progress);
					}
				}
				
				var index = i;
				btn.Pressed += () => ExecuteComboIndex(index);
			}
			else
			{
				btn.Text = "+";
				btn.Disabled = true;
			}
			
			slot.AddChild(btn);
			
			var keyLabel = new Label();
			keyLabel.Text = $"J+{i + 1}";
			keyLabel.HorizontalAlignment = HorizontalAlignment.Center;
			slot.AddChild(keyLabel);
			
			_equippedPanel.AddChild(slot);
		}
	}

	private void UpdateDetailPanel()
	{
		if (_selectedComboId == "")
		{
			ClearDetailPanel();
			return;
		}
		
		var combo = ComboSkillDatabase.Instance().GetCombo(_selectedComboId);
		if (combo == null)
		{
			ClearDetailPanel();
			return;
		}
		
		var nameLabel = _detailPanel.FindChild("name_label", true, false) as Label;
		var typeLabel = _detailPanel.FindChild("type_label", true, false) as Label;
		var descLabel = _detailPanel.FindChild("desc_label", true, false) as Label;
		var statsLabel = _detailPanel.FindChild("stats_label", true, false) as Label;
		var stepsLabel = _detailPanel.FindChild("steps_label", true, false) as Label;
		var equipBtn = _detailPanel.FindChild("equip_btn", true, false) as Button;
		var executeBtn = _detailPanel.FindChild("execute_btn", true, false) as Button;
		
		if (nameLabel != null)
		{
			nameLabel.Text = combo.Name;
			nameLabel.AddThemeColorOverride("font_color", ComboSkillDatabase.Instance().GetRarityColor(combo.Rarity));
		}
		
		if (typeLabel != null)
		{
			typeLabel.Text = $"类型: {GetComboTypeName(combo.ComboType)} | 稀有度: {ComboSkillDatabase.Instance().GetRarityName(combo.Rarity)}";
		}
		
		if (descLabel != null)
		{
			descLabel.Text = combo.Description;
		}
		
		if (statsLabel != null)
		{
			var cooldown = ComboSystem.GetCooldown(_selectedComboId);
			var isEquipped = ComboSystem.IsEquipped(_selectedComboId);
			var cooldownDisplay = cooldown > 0 ? combo.Cooldown - cooldown : combo.Cooldown;
			statsLabel.Text = $"冷却: {cooldownDisplay:F1}s | 法力: {combo.ManaCost:F0}\n等级需求: {combo.LevelRequired}\n已装备: {(isEquipped ? "是" : "否")}\n执行次数: 0";
		}
		
		if (stepsLabel != null)
		{
			var stepsText = $"连击步骤 ({combo.Steps.Count} 步):\n";
			for (int i = 0; i < combo.Steps.Count; i++)
			{
				var step = combo.Steps[i];
				stepsText += $"{i + 1}. {step.SkillId} - 延迟: {step.Delay:F1}s\n";
				foreach (var effect in step.Effects)
				{
					stepsText += $"   → {effect.Description}\n";
				}
			}
			stepsLabel.Text = stepsText;
		}
		
		if (equipBtn != null)
		{
			equipBtn.Text = ComboSystem.IsEquipped(_selectedComboId) ? "卸下" : "装备";
			equipBtn.Disabled = ComboSystem.IsOnCooldown(_selectedComboId);
		}
		
		if (executeBtn != null)
		{
			executeBtn.Disabled = ComboSystem.IsOnCooldown(_selectedComboId);
		}
	}

	private void ClearDetailPanel()
	{
		var labels = new string[] { "name_label", "type_label", "desc_label", "stats_label", "steps_label" };
		foreach (var labelName in labels)
		{
			var label = _detailPanel.FindChild(labelName, true, false) as Label;
			if (label != null)
			{
				label.Text = "";
			}
		}
	}

	private string GetComboTypeName(ComboType type)
	{
		return type switch
		{
			ComboType.Sequential => "顺序",
			ComboType.Parallel => "并行",
			ComboType.Chain => "链式",
			ComboType.Conditional => "条件",
			_ => "未知"
		};
	}

	// ============ 信号处理 ============

	private void OnComboUnlocked(string comboId)
	{
		Refresh();
	}

	private void OnComboExecuted(string comboId)
	{
		Refresh();
	}

	private void OnCooldownUpdated(string comboId, float remaining)
	{
		Refresh();
	}

	private void OnEquipPressed()
	{
		if (_selectedComboId == "")
		{
			return;
		}
		
		if (ComboSystem.IsEquipped(_selectedComboId))
		{
			ComboSystem.UnequipCombo(_selectedComboId);
		}
		else
		{
			ComboSystem.EquipCombo(_selectedComboId);
		}
		
		Refresh();
	}

	private void OnExecutePressed()
	{
		if (_selectedComboId == "")
		{
			return;
		}
		
		ComboSystem.ExecuteCombo(_selectedComboId);
	}

	private void ExecuteComboIndex(int index)
	{
		var equipped = ComboSystem.GetEquippedCombos();
		if (index < equipped.Count)
		{
			ComboSystem.ExecuteCombo(equipped[index].ComboId);
		}
	}
}
