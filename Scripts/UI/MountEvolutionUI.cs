using Godot;
using System;
using System.Collections.Generic;

public partial class MountEvolutionUI : Control
{
	private Control _container;
	private VBoxContainer _mainVBox;
	private Label _titleLabel;
	
	// Mount selection
	private OptionButton _mountSelect;
	private Label _mountInfoLabel;
	
	// Evolution info
	private Label _currentEvolutionLabel;
	private Label _nextEvolutionLabel;
	private ProgressBar _evolutionProgress;
	private Label _progressLabel;
	
	// Stats
	private Label _statsLabel;
	
	// Buttons
	private Button _evolveButton;
	private Button _closeButton;
	
	// Evolution list
	private ItemList _evolutionList;
	
	// Statistics
	private Label _statisticsLabel;
	
	private int _selectedMountId = -1;
	
	public override void _Ready()
	{
		Initialize();
	}
	
	private void Initialize()
	{
		// Create main container
		_container = new Control();
		_container.SetAnchorsPreset(Control.LayoutPreset.Center);
		_container.CustomMinimumSize = new Vector2(700, 550);
		AddChild(_container);
		
		// Background panel
		var bgPanel = new PanelContainer();
		bgPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		bgPanel.Modulate = new Color(1, 1, 1, 0.95f);
		_container.AddChild(bgPanel);
		
		var styleBox = new StyleBoxFlat();
		styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.98f);
		styleBox.BorderColor = new Color(0.3f, 0.3f, 0.4f);
		styleBox.SetBorderWidthAll(2);
		styleBox.SetCornerRadiusAll(8);
		bgPanel.AddThemeStyleboxOverride("panel", styleBox);
		
		_mainVBox = new VBoxContainer();
		_mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		_mainVBox.AddThemeConstantOverride("separation", 10);
		bgPanel.AddChild(_mainVBox);
		
		// Title
		_titleLabel = new Label();
		_titleLabel.Text = "  🐎 坐骑进化系统";
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
		_titleLabel.AddThemeFontSizeOverride("font_size", 24);
		_mainVBox.AddChild(_titleLabel);
		
		// Mount selection
		var mountHBox = new HBoxContainer();
		_mainVBox.AddChild(mountHBox);
		
		var mountLabel = new Label();
		mountLabel.Text = "选择坐骑:";
		mountLabel.CustomMinimumSize = new Vector2(100, 0);
		mountHBox.AddChild(mountLabel);
		
		_mountSelect = new OptionButton();
		_mountSelect.CustomMinimumSize = new Vector2(200, 30);
		_mountSelect.ItemSelected += OnMountSelected;
		mountHBox.AddChild(_mountSelect);
		
		_mountInfoLabel = new Label();
		_mountInfoLabel.Text = "";
		mountHBox.AddChild(_mountInfoLabel);
		
		// Evolution info section
		var infoPanel = new PanelContainer();
		infoPanel.CustomMinimumSize = new Vector2(0, 150);
		_mainVBox.AddChild(infoPanel);
		
		var infoStyle = new StyleBoxFlat();
		infoStyle.BgColor = new Color(0.15f, 0.15f, 0.2f);
		infoStyle.SetCornerRadiusAll(5);
		infoPanel.AddThemeStyleboxOverride("panel", infoStyle);
		
		var infoVBox = new VBoxContainer();
		infoPanel.AddChild(infoVBox);
		
		_currentEvolutionLabel = new Label();
		_currentEvolutionLabel.Text = "当前形态: 未选择坐骑";
		_currentEvolutionLabel.AddThemeFontSizeOverride("font_size", 18);
		infoVBox.AddChild(_currentEvolutionLabel);
		
		_nextEvolutionLabel = new Label();
		_nextEvolutionLabel.Text = "下一形态: -";
		infoVBox.AddChild(_nextEvolutionLabel);
		
		var progressContainer = new HBoxContainer();
		infoVBox.AddChild(progressContainer);
		
		var progressLabel = new Label();
		progressLabel.Text = "进化进度:";
		progressContainer.AddChild(progressLabel);
		
		_evolutionProgress = new ProgressBar();
		_evolutionProgress.CustomMinimumSize = new Vector2(300, 20);
		_evolutionProgress.ShowPercentage = false;
		progressContainer.AddChild(_evolutionProgress);
		
		_progressLabel = new Label();
		_progressLabel.Text = "0%";
		progressContainer.AddChild(_progressLabel);
		
		// Evolution list
		var listLabel = new Label();
		listLabel.Text = "进化路线:";
		_mainVBox.AddChild(listLabel);
		
		_evolutionList = new ItemList();
		_evolutionList.CustomMinimumSize = new Vector2(0, 180);
		_evolutionList.ItemSelected += OnEvolutionItemSelected;
		_mainVBox.AddChild(_evolutionList);
		
		// Buttons
		var buttonHBox = new HBoxContainer();
		_mainVBox.AddChild(buttonHBox);
		
		_evolveButton = new Button();
		_evolveButton.Text = "开始进化";
		_evolveButton.CustomMinimumSize = new Vector2(150, 40);
		_evolveButton.Pressed += OnEvolvePressed;
		buttonHBox.AddChild(_evolveButton);
		
		var refreshButton = new Button();
		refreshButton.Text = "刷新坐骑";
		refreshButton.CustomMinimumSize = new Vector2(120, 40);
		refreshButton.Pressed += RefreshMounts;
		buttonHBox.AddChild(refreshButton);
		
		// Statistics
		var statsPanel = new PanelContainer();
		_mainVBox.AddChild(statsPanel);
		
		var statsStyle = new StyleBoxFlat();
		statsStyle.BgColor = new Color(0.12f, 0.12f, 0.18f);
		statsStyle.SetCornerRadiusAll(5);
		statsPanel.AddThemeStyleboxOverride("panel", statsStyle);
		
		_statisticsLabel = new Label();
		_statisticsLabel.Text = "统计: 加载中...";
		_statisticsLabel.AddThemeFontSizeOverride("font_size", 14);
		statsPanel.AddChild(_statisticsLabel);
		
		// Close button
		_closeButton = new Button();
		_closeButton.Text = "关闭 (ESC)";
		_closeButton.CustomMinimumSize = new Vector2(120, 35);
		_closeButton.Alignment = HorizontalAlignment.Center;
		_closeButton.Pressed += OnClosePressed;
		_mainVBox.AddChild(_closeButton);
		
		// Load mounts
		RefreshMounts();
		
		// Animation
		PlayEnterAnimation();
		
		GD.Print("[MountEvolutionUI] Initialized");
	}
	
	private void RefreshMounts()
	{
		_mountSelect.Clear();
		
		// Get mounts from MountManager (placeholder)
		var mounts = GetMounts();
		
		for (int i = 0; i < mounts.Count; i++)
		{
			var mount = mounts[i];
			_mountSelect.AddItem($"坐骑 #{mount.Key}: {mount.Value}", mount.Key);
		}
		
		if (mounts.Count > 0)
		{
			_mountSelect.Selected = 0;
			OnMountSelected(0);
		}
	}
	
	private Dictionary<int, string> GetMounts()
	{
		// Placeholder - would get from MountManager
		var mounts = new Dictionary<int, string>();
		mounts[1] = "火焰战马";
		mounts[2] = "暗影狼";
		mounts[3] = "幼龙";
		return mounts;
	}
	
	private void OnMountSelected(int index)
	{
		if (index < 0) return;
		
		_selectedMountId = _mountSelect.GetItemId(index);
		var mountName = _mountSelect.GetItemText(index);
		
		_mountInfoLabel.Text = $"(ID: {_selectedMountId})";
		
		UpdateEvolutionInfo();
	}
	
	private void UpdateEvolutionInfo()
	{
		if (_selectedMountId < 0)
		{
			_currentEvolutionLabel.Text = "当前形态: 未选择坐骑";
			_nextEvolutionLabel.Text = "下一形态: -";
			_evolutionProgress.Value = 0;
			_progressLabel.Text = "0%";
			return;
		}
		
		var currentConfig = MountEvolutionSystem.Instance.GetEvolutionConfig(_selectedMountId);
		var nextConfig = MountEvolutionSystem.Instance.GetNextEvolutionConfig(_selectedMountId);
		
		if (currentConfig != null)
		{
			_currentEvolutionLabel.Text = $"当前形态: {currentConfig.Name} ({currentConfig.Stage})";
			
			if (nextConfig != null)
			{
				_nextEvolutionLabel.Text = $"下一形态: {nextConfig.Name} - 需要 {nextConfig.RequiredExp} 经验";
				_evolveButton.Text = "查看下一形态";
				_evolveButton.Disabled = false;
			}
			else
			{
				_nextEvolutionLabel.Text = "下一形态: 已达到最高形态!";
				_evolveButton.Text = "已达最高";
				_evolveButton.Disabled = true;
			}
			
			int progress = MountEvolutionSystem.Instance.GetEvolutionProgress(_selectedMountId);
			_evolutionProgress.Value = progress;
			_progressLabel.Text = $"{progress}%";
		}
		else
		{
			_currentEvolutionLabel.Text = "当前形态: 基础形态";
			_nextEvolutionLabel.Text = "下一形态: 需要先选择进化路线";
			_evolutionProgress.Value = 0;
			_progressLabel.Text = "0%";
			_evolveButton.Text = "选择进化";
			_evolveButton.Disabled = false;
		}
		
		UpdateEvolutionList();
		UpdateStatistics();
	}
	
	private void UpdateEvolutionList()
	{
		_evolutionList.Clear();
		
		// Show evolution chain for first mount (placeholder)
		var configs = MountEvolutionDatabase.GetConfigsByChain(MountEvolutionData.EvolutionChain.Dragon);
		
		foreach (var config in configs)
		{
			string text = $"{config.Name} ({config.Stage}) - Lv.{config.RequiredLevel}";
			if (config.RequiredExp > 0)
				text += $", {config.RequiredExp} EXP";
			if (!string.IsNullOrEmpty(config.SkillUnlocked))
				text += $", 技能: {config.SkillUnlocked}";
				
			_evolutionList.AddItem(text);
		}
	}
	
	private void UpdateStatistics()
	{
		var stats = MountEvolutionSystem.Instance.GetEvolutionStatistics();
		
		string statsText = $"统计: 总进化次数: {stats["totalEvolutions"]} | " +
			$"总获得经验: {stats["totalExpGained"]} | " +
			$"进行中: {stats["activeEvolutions"]} | " +
			$"传说: {stats["legendaryEvolutions"]} | " +
			$"史诗: {stats["epicEvolutions"]} | " +
			$"精英: {stats["eliteEvolutions"]}";
		
		_statisticsLabel.Text = statsText;
	}
	
	private void OnEvolutionItemSelected(int index)
	{
		// Show details for selected evolution
	}
	
	private void OnEvolvePressed()
	{
		if (_selectedMountId < 0) return;
		
		var currentConfig = MountEvolutionSystem.Instance.GetEvolutionConfig(_selectedMountId);
		var nextConfig = MountEvolutionSystem.Instance.GetNextEvolutionConfig(_selectedMountId);
		
		if (currentConfig == null)
		{
			// Start new evolution with first config
			var firstConfig = MountEvolutionDatabase.GetConfigsByChain(MountEvolutionData.EvolutionChain.Dragon)[0];
			if (firstConfig != null)
			{
				bool success = MountEvolutionSystem.Instance.TryEvolve(_selectedMountId, firstConfig.Id);
				if (success)
				{
					GD.Print($"[MountEvolutionUI] Started evolution: {firstConfig.Name}");
				}
			}
		}
		else if (nextConfig != null)
		{
			// Check if can evolve to next
			if (MountEvolutionSystem.Instance.CanEvolve(_selectedMountId, nextConfig.Id))
			{
				bool success = MountEvolutionSystem.Instance.TryEvolve(_selectedMountId, nextConfig.Id);
				if (success)
				{
					GD.Print($"[MountEvolutionUI] Evolved to: {nextConfig.Name}");
				}
			}
		}
		
		UpdateEvolutionInfo();
	}
	
	private void OnClosePressed()
	{
		PlayExitAnimation();
	}
	
	private void PlayEnterAnimation()
	{
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.SetTrans(Tween.TransitionType.Back);
		tween.SetEasing(Tween.EasingFunction.EaseOut);
		
		_container.Scale = new Vector2(0.8f, 0.8f);
		_container.Modulate = new Color(1, 1, 1, 0);
		
		tween.TweenProperty(_container, "scale", new Vector2(1f, 1f), 0.3f);
		tween.TweenProperty(_container, "modulate:a", 1f, 0.3f);
	}
	
	private void PlayExitAnimation()
	{
		var tween = CreateTween();
		tween.SetParallel(true);
		tween.SetTrans(Tween.TransitionType.Back);
		tween.SetEasing(Tween.EasingFunction.EaseIn);
		
		tween.TweenProperty(_container, "scale", new Vector2(0.8f, 0.8f), 0.2f);
		tween.TweenProperty(_container, "modulate:a", 0f, 0.2f);
		
		tween.TweenCallback(Callable.From(() => QueueFree()));
	}
	
	public override void _Input(InputEvent event_)
	{
		if (event_ is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.Escape)
			{
				OnClosePressed();
			}
		}
	}
}
