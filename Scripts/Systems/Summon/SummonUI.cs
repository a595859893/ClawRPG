using Godot;
using System;
using System.Collections.Generic;

public partial class SummonUI : Control
{
	private SummonSystem summonSystem;
	private VBoxContainer mainContainer;
	private HBoxContainer buttonContainer;
	private GridContainer summonGrid;
	private Label titleLabel;
	private Label statsLabel;
	private TabContainer tabContainer;
	
	private int selectedSummonId = -1;
	
	public override void _Ready()
	{
		summonSystem = GetNode<SummonSystem>("/root/SummonSystem");
		SetupUI();
		Visible = false;
	}
	
	private void SetupUI()
	{
		// Main container
		mainContainer = new VBoxContainer();
		mainContainer.SetAnchorsPreset(Control.LayoutPreset.Wide);
		mainContainer.AddThemeConstantOverride("separation", 10);
		AddChild(mainContainer);
		
		// Title
		titleLabel = new Label();
		titleLabel.Text = "  召唤兽系统  ";
		titleLabel.AddThemeFontSizeOverride("font_size", 24);
		mainContainer.AddChild(titleLabel);
		
		// Tab container
		tabContainer = new TabContainer();
		tabContainer.SetSize(new Vector2(800, 500));
		mainContainer.AddChild(tabContainer);
		
		// Create tabs
		CreateAllSummonsTab();
		CreateMySummonsTab();
		CreateStatisticsTab();
		
		// Button container
		buttonContainer = new HBoxContainer();
		buttonContainer.AddThemeConstantOverride("separation", 10);
		mainContainer.AddChild(buttonContainer);
		
		var closeButton = new Button();
		closeButton.Text = "关闭 (ESC)";
		closeButton.Pressed += OnClosePressed;
		buttonContainer.AddChild(closeButton);
		
		var unlockAllButton = new Button();
		unlockAllButton.Text = "解锁全部召唤兽";
		unlockAllButton.Pressed += OnUnlockAllPressed;
		buttonContainer.AddChild(unlockAllButton);
	}
	
	private void CreateAllSummonsTab()
	{
		var scrollContainer = new ScrollContainer();
		scrollContainer.Name = "全部召唤兽";
		tabContainer.AddChild(scrollContainer);
		
		var vbox = new VBoxContainer();
		vbox.SetSize(new Vector2(760, 460));
		scrollContainer.AddChild(vbox);
		
		summonGrid = new GridContainer();
		summonGrid.Columns = 4;
		summonGrid.AddThemeConstantOverride("h_separation", 10);
		summonGrid.AddThemeConstantOverride("v_separation", 10);
		vbox.AddChild(summonGrid);
		
		RefreshSummonGrid();
	}
	
	private void CreateMySummonsTab()
	{
		var scrollContainer = new ScrollContainer();
		scrollContainer.Name = "我的召唤兽";
		tabContainer.AddChild(scrollContainer);
		
		var vbox = new VBoxContainer();
		vbox.SetSize(new Vector2(760, 460));
		scrollContainer.AddChild(vbox);
		
		var myGrid = new GridContainer();
		myGrid.Columns = 4;
		myGrid.AddThemeConstantOverride("h_separation", 10);
		myGrid.AddThemeConstantOverride("v_separation", 10);
		vbox.AddChild(myGrid);
		
		RefreshMySummonGrid(myGrid);
	}
	
	private void CreateStatisticsTab()
	{
		var vbox = new VBoxContainer();
		vbox.Name = "统计";
		vbox.AddThemeConstantOverride("separation", 15);
		tabContainer.AddChild(vbox);
		
		var statsTitle = new Label();
		statsTitle.Text = "召唤兽统计";
		statsTitle.AddThemeFontSizeOverride("font_size", 20);
		vbox.AddChild(statsTitle);
		
		statsLabel = new Label();
		vbox.AddChild(statsLabel);
		
		RefreshStatistics();
	}
	
	private void RefreshSummonGrid()
	{
		// Clear existing
		foreach (var child in summonGrid.GetChildren())
		{
			child.QueueFree();
		}
		
		var allSummons = summonSystem.GetAllSummons();
		foreach (var summon in allSummons)
		{
			var panel = CreateSummonCard(summon);
			summonGrid.AddChild(panel);
		}
	}
	
	private void RefreshMySummonGrid(GridContainer grid)
	{
		// Clear existing
		foreach (var child in grid.GetChildren())
		{
			child.QueueFree();
		}
		
		var mySummons = summonSystem.GetUnlockedSummons();
		if (mySummons.Count == 0)
		{
			var emptyLabel = new Label();
			emptyLabel.Text = "尚未解锁任何召唤兽";
			emptyLabel.AddThemeFontSizeOverride("font_size", 18);
			grid.AddChild(emptyLabel);
		}
		else
		{
			foreach (var summon in mySummons)
			{
				var panel = CreateSummonCard(summon);
				grid.AddChild(panel);
			}
		}
	}
	
	private Control CreateSummonCard(SummonData summon)
	{
		var panel = new PanelContainer();
		panel.CustomMinimumSize = new Vector2(180, 200);
		
		var vbox = new VBoxContainer();
		vbox.AddThemeConstantOverride("separation", 5);
		panel.AddChild(vbox);
		
		// Name
		var nameLabel = new Label();
		nameLabel.Text = summon.Name;
		nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
		nameLabel.AddThemeFontSizeOverride("font_size", 16);
		vbox.AddChild(nameLabel);
		
		// Type and Rarity
		var typeLabel = new Label();
		typeLabel.Text = $"{summon.Type} | {summon.Rarity}";
		typeLabel.HorizontalAlignment = HorizontalAlignment.Center;
		typeLabel.AddThemeFontSizeOverride("font_size", 12);
		vbox.AddChild(typeLabel);
		
		// Stats
		var statsText = $"攻击: {summon.BaseAttack}\n防御: {summon.BaseDefense}\n生命: {summon.BaseHealth}\n攻速: {summon.AttackSpeed:F1}\n法力: {summon.ManaCost}";
		var statsLabel = new Label();
		statsLabel.Text = statsText;
		statsLabel.AddThemeFontSizeOverride("font_size", 12);
		vbox.AddChild(statsLabel);
		
		// Unlock status
		var isUnlocked = summonSystem.IsSummonUnlocked(summon.SummonId);
		var statusLabel = new Label();
		statusLabel.Text = isUnlocked ? "已解锁" : "未解锁";
		statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
		statusLabel.Modulate = isUnlocked ? new Color(0, 1, 0) : new Color(1, 0, 0);
		vbox.AddChild(statusLabel);
		
		// Select button
		var selectButton = new Button();
		selectButton.Text = isUnlocked ? "选择" : "查看";
		selectButton.Pressed += () => OnSummonSelected(summon.SummonId);
		vbox.AddChild(selectButton);
		
		return panel;
	}
	
	private void RefreshStatistics()
	{
		var stats = summonSystem.GetSummonStatistics();
		var text = $"总召唤兽数量: {stats["total"]}\n" +
		           $"已解锁数量: {stats["unlocked"]}\n\n" +
		           $"普通: {stats["common"]}\n" +
		           $"优秀: {stats["uncommon"]}\n" +
		           $"稀有: {stats["rare"]}\n" +
		           $"史诗: {stats["epic"]}\n" +
		           $"传说: {stats["legendary"]}";
		statsLabel.Text = text;
	}
	
	private void OnSummonSelected(int summonId)
	{
		selectedSummonId = summonId;
		var summon = summonSystem.GetSummonData(summonId);
		if (summon != null)
		{
			summonSystem.SetActiveSummon(summonId);
			GD.Print($"选择了召唤兽: {summon.Name}");
		}
	}
	
	private void OnClosePressed()
	{
		Visible = false;
	}
	
	private void OnUnlockAllPressed()
	{
		summonSystem.UnlockAllSummons();
		RefreshSummonGrid();
		RefreshStatistics();
		GD.Print("已解锁全部召唤兽");
	}
	
	public override void _Input(InputEvent @event)
	{
		if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == KeyList.Escape)
		{
			Visible = false;
		}
	}
	
	public void Toggle()
	{
		Visible = !Visible;
		if (Visible)
		{
			RefreshSummonGrid();
			RefreshStatistics();
		}
	}
}
