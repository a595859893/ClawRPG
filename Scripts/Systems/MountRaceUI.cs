using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 坐骑竞速界面 - 管理坐骑竞速活动的UI显示
/// </summary>
public partial class MountRaceUI : Control
{
	private Control _mainPanel;
	private VBoxContainer _raceListContainer;
	private Label _titleLabel;
	private Label _statsLabel;
	private Button _closeButton;

	private MountRaceData.MountRace _selectedRace;
	private bool _isRacing = false; 

	public override void _Ready()
	{
		Visible = false; 
	 SetupUI();
	}

	private void SetupUI()
	{
		// Main panel
		_mainPanel = new PanelContainer();
		_mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
		_mainPanel.CustomMinimumSize = new Vector2(800, 600);
		AddChild(_mainPanel);

		var mainVBox = new VBoxContainer();
		_mainPanel.AddChild(mainVBox);

		// Title
		_titleLabel = new Label();
		_titleLabel.Text = "🐎 坐骑竞赛";
		_titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_titleLabel.AddThemeFontSizeOverride("font_size", 24);
		mainVBox.AddChild(_titleLabel);

		// Stats
		_statsLabel = new Label();
		_statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
		mainVBox.AddChild(_statsLabel);

		// Race list (scrollable)
		var scrollContainer = new ScrollContainer();
		scrollContainer.CustomMinimumSize = new Vector2(760, 400);
		mainVBox.AddChild(scrollContainer);

		_raceListContainer = new VBoxContainer();
		_raceListContainer.CustomMinimumSize = new Vector2(740, 0);
		scrollContainer.AddChild(_raceListContainer);

		// Close button
		_closeButton = new Button();
		_closeButton.Text = "关闭 (ESC)";
		_closeButton.CustomMinimumSize = new Vector2(200, 40);
		_closeButton.Pressed += OnClosePressed;
		
		var buttonContainer = new HBoxContainer();
		buttonContainer.HorizontalAlignment = HorizontalAlignment.Center;
		buttonContainer.AddChild(_closeButton);
		mainVBox.AddChild(buttonContainer);

		RefreshRaceList();
	}

	private void RefreshRaceList()
	{
		// Clear existing
		foreach (Node child in _raceListContainer.GetChildren())
		{
			child.QueueFree();
		}

		var races = MountRaceDatabase.Instance.GetAllRaces();

		foreach (var race in races)
		{
			var raceCard = CreateRaceCard(race);
			_raceListContainer.AddChild(raceCard);
		}

		UpdateStats();
	}

	private Control CreateRaceCard(MountRaceData.MountRace race)
	{
		var card = new PanelContainer();
		card.CustomMinimumSize = new Vector2(720, 80);
		card.AddThemeStyleboxOverride("panel", CreateCardStyle());

		var hbox = new HBoxContainer();
		card.AddChild(hbox);

		// Race info
		var infoVBox = new VBoxContainer();
		infoVBox.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
		hbox.AddChild(infoVBox);

		// Name and difficulty
		var nameLabel = new Label();
		nameLabel.Text = $"{race.Name}  [难度: {GetDifficultyStars(race.Difficulty)}]";
		nameLabel.AddThemeFontSizeOverride("font_size", 18);
		infoVBox.AddChild(nameLabel);

		var descLabel = new Label();
		descLabel.Text = race.Description;
		descLabel.AddThemeFontSizeOverride("font_size", 14);
		descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
		infoVBox.AddChild(descLabel);

		var statsLabel = new Label();
		statsLabel.Text = $"距离: {race.Distance}m | 报名费: {race.EntryFee}金 | 奖励: {race.RewardGold}金/{race.RewardExp}经验";
		statsLabel.AddThemeFontSizeOverride("font_size", 12);
		infoVBox.AddChild(statsLabel);

		// Start button
		var startButton = new Button();
		startButton.Text = "开始";
		startButton.CustomMinimumSize = new Vector2(100, 40);
		startButton.Pressed += () => OnStartRacePressed(race);
		hbox.AddChild(startButton);

		// Check if player has mount
		var player = GetTree().Root.GetNode<Player>("Player");
		if (player == null || string.IsNullOrEmpty(player.CurrentMountId))
		{
			startButton.Disabled = true;
			startButton.Text = "需要坐骑";
		}
		else if (player.Gold < race.EntryFee)
		{
			startButton.Disabled = true;
			startButton.Text = "金币不足";
		}

		return card;
	}

	private string GetDifficultyStars(int difficulty)
	{
		string stars = "";
		for (int i = 0; i < difficulty; i++)
			stars += "★";
		for (int i = difficulty; i < 5; i++)
			stars += "☆";
		return stars;
	}

	private StyleBoxFlat CreateCardStyle()
	{
		var style = new StyleBoxFlat();
		style.BgColor = new Color(0.15f, 0.15f, 0.2f, 0.9f);
		style.BorderWidthLeft = 2;
		style.BorderWidthRight = 2;
		style.BorderWidthTop = 2;
		style.BorderWidthBottom = 2;
		style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
		style.CornerRadiusTopLeft = 8;
		style.CornerRadiusTopRight = 8;
		style.CornerRadiusBottomLeft = 8;
		style.CornerRadiusBottomRight = 8;
		return style;
	}

	private void UpdateStats()
	{
		var progress = MountRaceSystem.Instance.GetProgress();
		_statsLabel.Text = $"总参赛: {progress.TotalRaces} | 🥇{progress.FirstPlaces} 🥈{progress.SecondPlaces} 🥉{progress.ThirdPlaces} | 总收益: {progress.TotalEarnings}金";
	}

	private void OnStartRacePressed(MountRaceData.MountRace race)
	{
		if (MountRaceSystem.Instance.StartRace(race.Id))
		{
			Hide();
			ShowRaceUI();
		}
	}

	private Control _raceUIPanel;
	private Label _raceInfoLabel;
	private ProgressBar _positionBar;
	private Label _timerLabel;
	private Label _checkpointLabel;

	private void ShowRaceUI()
	{
		_isRacing = true;
		
		_raceUIPanel = new Control();
		_raceUIPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		GetTree().Root.AddChild(_raceUIPanel);

		var panel = new PanelContainer();
		panel.SetAnchorsPreset(Control.LayoutPreset.CenterBottom);
		panel.OffsetTop = -200;
		panel.CustomMinimumSize = new Vector2(600, 150);
		_raceUIPanel.AddChild(panel);

		var vbox = new VBoxContainer();
		panel.AddChild(vbox);

		var race = MountRaceSystem.Instance.GetCurrentRace();
		_raceInfoLabel = new Label();
		_raceInfoLabel.Text = $"🏁 {race.Name}";
		_raceInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
		_raceInfoLabel.AddThemeFontSizeOverride("font_size", 20);
		vbox.AddChild(_raceInfoLabel);

		// Position bar
		_positionBar = new ProgressBar();
		_positionBar.CustomMinimumSize = new Vector2(560, 30);
		_positionBar.MaxValue = 100;
		_positionBar.Value = 0;
		_positionBar.ShowPercentage = false; 
		vbox.AddChild(_positionBar);

		var posLabel = new Label();
		posLabel.Text = "起点                              终点";
		posLabel.HorizontalAlignment = HorizontalAlignment.Center;
		vbox.AddChild(posLabel);

		// Timer and checkpoint
		var hbox = new HBoxContainer();
		hbox.HorizontalAlignment = HorizontalAlignment.Center;
		vbox.AddChild(hbox);

		_timerLabel = new Label();
		_timerLabel.Text = "时间: 0.0s";
		_timerLabel.AddThemeFontSizeOverride("font_size", 18);
		hbox.AddChild(_timerLabel);

		var spacer = new Control();
		spacer.CustomMinimumSize = new Vector2(50, 0);
		hbox.AddChild(spacer);

		_checkpointLabel = new Label();
		_checkpointLabel.Text = "检查点: 0/0";
		_checkpointLabel.AddThemeFontSizeOverride("font_size", 18);
		hbox.AddChild(_checkpointLabel);

		// Cancel button
		var cancelButton = new Button();
		cancelButton.Text = "退出竞赛 (ESC)";
		cancelButton.Pressed += OnExitRacePressed;
		vbox.AddChild(cancelButton);

		// Connect signals
		MountRaceSystem.Instance.RacePositionUpdate += () => OnRaceUpdate();
		MountRaceSystem.Instance.RaceFinished += () => OnRaceFinished();
	}

	private void OnRaceUpdate()
	{
		if (!_isRacing)
			return;

		var playerRacer = MountRaceSystem.Instance.GetPlayerRacer();
		var race = MountRaceSystem.Instance.GetCurrentRace();
		
		if (playerRacer != null && race != null)
		{
			float progress = (playerRacer.CurrentPosition / race.Distance) * 100f;
			_positionBar.Value = progress;
			_timerLabel.Text = $"时间: {playerRacer.ElapsedTime:F1}s";
			_checkpointLabel.Text = $"检查点: {playerRacer.CurrentCheckpoint}/{race.Checkpoints.Count - 1}";
		}
	}

	private void OnRaceFinished()
	{
		if (!_isRacing)
			return;

		_isRacing = false; 

		var playerRacer = MountRaceSystem.Instance.GetPlayerRacer();
		var race = MountRaceSystem.Instance.GetCurrentRace();
		
		if (playerRacer != null && race != null)
		{
			int place = MountRaceSystem.Instance.GetPlayerPosition();
			int totalRacers = MountRaceSystem.Instance.GetRacers().Count;
			
			// Show result dialog
			var resultPanel = new PanelContainer();
			resultPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
			resultPanel.CustomMinimumSize = new Vector2(400, 200);
			GetTree().Root.AddChild(resultPanel);

			var vbox = new VBoxContainer();
			resultPanel.AddChild(vbox);

			var resultLabel = new Label();
			if (place == 1)
				resultLabel.Text = "🏆 冠军!";
			else if (place == 2)
				resultLabel.Text = "🥈 第二名!";
			else if (place == 3)
				resultLabel.Text = "🥉 第三名!";
			else
				resultLabel.Text = $"第 {place} 名";
			
			resultLabel.HorizontalAlignment = HorizontalAlignment.Center;
			resultLabel.AddThemeFontSizeOverride("font_size", 28);
			vbox.AddChild(resultLabel);

			var timeLabel = new Label();
			timeLabel.Text = $"用时: {playerRacer.ElapsedTime:F1}s";
			timeLabel.HorizontalAlignment = HorizontalAlignment.Center;
			vbox.AddChild(timeLabel);

			var rewardLabel = new Label();
			int reward = 0;
			if (place == 1) reward = race.RewardGold;
			else if (place == 2) reward = (int)(race.RewardGold * 0.6f);
			else if (place == 3) reward = (int)(race.RewardGold * 0.3f);
			else reward = (int)(race.RewardGold * 0.1f);
			rewardLabel.Text = $"奖励: {reward}金 / {race.RewardExp}经验";
			rewardLabel.HorizontalAlignment = HorizontalAlignment.Center;
			vbox.AddChild(rewardLabel);

			var okButton = new Button();
			okButton.Text = "确定";
			okButton.Pressed += () =>
			{
				resultPanel.QueueFree();
				if (_raceUIPanel != null)
					_raceUIPanel.QueueFree();
			};
			vbox.AddChild(okButton);
		}

		// Disconnect signals
		MountRaceSystem.Instance.RacePositionUpdate -= () => OnRaceUpdate();
		MountRaceSystem.Instance.RaceFinished -= () => OnRaceFinished();
	}

	private void OnExitRacePressed()
	{
		MountRaceSystem.Instance.CancelRace();
		_isRacing = false; 
		
		if (_raceUIPanel != null)
			_raceUIPanel.QueueFree();
	}

	private void OnClosePressed()
	{
		Hide();
	}

	public override void _Input(InputEvent e)
	{
		if (e is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.Escape)
			{
				if (_isRacing)
					OnExitRacePressed();
				else
					Hide();
			}
		}
	}

	public void Show()
	{
		Visible = true;
		RefreshRaceList();
	}

	public void Hide()
	{
		Visible = false; 
	}

	public void Toggle()
	{
		if (Visible)
			Hide();
		else
			Show();
	}
}
