using Godot;
using System;
using System.Collections.Generic;

public class DailyQuestUI : Control
{
	private VBoxContainer _questListContainer;
	private Label _titleLabel;
	private Label _statsLabel;
	private Label _dateLabel;
	private Button _closeButton;

	private Color _easyColor = new Color(0.2f, 0.8f, 0.2f);
	private Color _normalColor = new Color(0.2f, 0.6f, 1f);
	private Color _hardColor = new Color(1f, 0.6f, 0f);
	private Color _epicColor = new Color(0.6f, 0.2f, 1f);
	private Color _legendaryColor = new Color(1f, 0.3f, 0.3f);

	public override void _Ready()
	{
		SetupUI();
		RefreshQuestList();
		Visible = false;

		// Connect signals
		if (DailyQuestSystem.Instance != null)
		{
			DailyQuestSystem.Instance.QuestUpdated.Connect(OnQuestUpdated);
			DailyQuestSystem.Instance.QuestCompleted.Connect(OnQuestCompleted);
		}
	}

	private void SetupUI()
	{
		// Main panel
		Panel mainPanel = new Panel();
		mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
		mainPanel.CustomMinimumSize = new Vector2(600, 500);
		AddChild(mainPanel);

		var mainVBox = new VBoxContainer();
		mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		mainVBox.AddThemeConstantOverride("separation", 10);
		mainPanel.AddChild(mainVBox);

		// Title bar
		var titleBar = new HBoxContainer();
		titleBar.AddThemeConstantOverride("separation", 10);
		mainVBox.AddChild(titleBar);

		_titleLabel = new Label();
		_titleLabel.Text = "每日任务";
		_titleLabel.AddThemeFontSizeOverride("font_size", 24);
		titleBar.AddChild(_titleLabel);

		titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

		_dateLabel = new Label();
		_dateLabel.Text = DateTime.Now.ToShortDateString();
		_dateLabel.AddThemeFontSizeOverride("font_size", 16);
		titleBar.AddChild(_dateLabel);

		_closeButton = new Button();
		_closeButton.Text = "X";
		_closeButton.CustomMinimumSize = new Vector2(40, 40);
		_closeButton.Pressed += () => Visible = false;
		titleBar.AddChild(_closeButton);

		// Stats
		_statsLabel = new Label();
		_statsLabel.Text = "获取统计...";
		_statsLabel.AddThemeFontSizeOverride("font_size", 14);
		mainVBox.AddChild(_statsLabel);

		// Scroll container for quest list
		ScrollContainer scrollContainer = new ScrollContainer();
		scrollContainer.SetVerticalExpandFillRatio(1f);
		scrollContainer.SetHorizontalExpandFillRatio(1f);
		mainVBox.AddChild(scrollContainer);

		_questListContainer = new VBoxContainer();
		_questListContainer.AddThemeConstantOverride("separation", 8);
		scrollContainer.AddChild(_questListContainer);

		UpdateStats();
	}

	private void RefreshQuestList()
	{
		// Clear existing
		foreach (Node child in _questListContainer.GetChildren())
		{
			child.QueueFree();
		}

		var quests = DailyQuestSystem.Instance.GetDailyQuests();

		foreach (var quest in quests)
		{
			var questPanel = CreateQuestPanel(quest);
			_questListContainer.AddChild(questPanel);
		}

		UpdateStats();
	}

	private Control CreateQuestPanel(DailyQuestData quest)
	{
		Panel panel = new Panel();
		panel.CustomMinimumSize = new Vector2(560, 80);

		var hBox = new HBoxContainer();
		hBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
		hBox.AddThemeConstantOverride("separation", 10);
		panel.AddChild(hBox);

		// Difficulty indicator
		ColorPanel difficultyIndicator = new ColorPanel();
		difficultyIndicator.CustomMinimumSize = new Vector2(8, 60);
		difficultyIndicator.Color = GetDifficultyColor(quest.Difficulty);
		hBox.AddChild(difficultyIndicator);

		// Quest info
		VBoxContainer infoVBox = new VBoxContainer();
		infoVBox.SetVerticalExpandFillRatio(1f);
		hBox.AddChild(infoVBox);

		// Quest name
		Label nameLabel = new Label();
		nameLabel.Text = quest.QuestName;
		nameLabel.AddThemeFontSizeOverride("font_size", 18);
		nameLabel.AutowrapMode = TextServer.AutowrapMode.Off;
		nameLabel.ClampToViewport = true;
		infoVBox.AddChild(nameLabel);

		// Description
		Label descLabel = new Label();
		descLabel.Text = quest.Description;
		descLabel.AddThemeFontSizeOverride("font_size", 14);
		descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
		infoVBox.AddChild(descLabel);

		// Progress
		Label progressLabel = new Label();
		progressLabel.Text = $"{quest.CurrentCount} / {quest.TargetCount}";
		progressLabel.AddThemeFontSizeOverride("font_size", 14);
		infoVBox.AddChild(progressLabel);

		// Progress bar
		ProgressBar progressBar = new ProgressBar();
		progressBar.CustomMinimumSize = new Vector2(200, 16);
		progressBar.MaxValue = quest.TargetCount;
		progressBar.Value = quest.CurrentCount;
		infoVBox.AddChild(progressBar);

		// Rewards
		VBoxContainer rewardVBox = new VBoxContainer();
		rewardVBox.CustomMinimumSize = new Vector2(120, 0);
		hBox.AddChild(rewardVBox);

		Label rewardLabel = new Label();
		rewardLabel.Text = "奖励:";
		rewardLabel.AddThemeFontSizeOverride("font_size", 12);
		rewardVBox.AddChild(rewardLabel);

		Label goldLabel = new Label();
		goldLabel.Text = $"💰 {quest.GoldReward}";
		goldLabel.AddThemeFontSizeOverride("font_size", 14);
		rewardVBox.AddChild(goldLabel);

		Label expLabel = new Label();
		expLabel.Text = $"✨ {quest.ExpReward}";
		expLabel.AddThemeFontSizeOverride("font_size", 14);
		rewardVBox.AddChild(expLabel);

		// Action button
		Button actionButton = new Button();
		actionButton.CustomMinimumSize = new Vector2(80, 60);

		if (quest.IsClaimed)
		{
			actionButton.Text = "已领取";
			actionButton.Disabled = true;
		}
		else if (quest.IsCompleted)
		{
			actionButton.Text = "领取";
			actionButton.Pressed += () => OnClaimReward(quest);
		}
		else
		{
			actionButton.Text = "进行中";
			actionButton.Disabled = true;
		}

		rewardVBox.AddChild(actionButton);

		// Update progress bar color based on completion
		if (quest.IsCompleted)
		{
			progressBar.Modulate = new Color(0.2f, 0.8f, 0.2f);
		}
		else if (quest.CurrentCount > 0)
		{
			progressBar.Modulate = new Color(0.2f, 0.6f, 1f);
		}

		return panel;
	}

	private Color GetDifficultyColor(DailyQuestData.QuestDifficulty difficulty)
	{
		switch (difficulty)
		{
			case DailyQuestData.QuestDifficulty.Easy: return _easyColor;
			case DailyQuestData.QuestDifficulty.Normal: return _normalColor;
			case DailyQuestData.QuestDifficulty.Hard: return _hardColor;
			case DailyQuestData.QuestDifficulty.Epic: return _epicColor;
			case DailyQuestData.QuestDifficulty.Legendary: return _legendaryColor;
			default: return _normalColor;
		}
	}

	private void OnClaimReward(DailyQuestData quest)
	{
		if (DailyQuestSystem.Instance.ClaimReward(quest))
		{
			RefreshQuestList();
		}
	}

	private void OnQuestUpdated(DailyQuestData quest)
	{
		// Update in place
		RefreshQuestList();
	}

	private void OnQuestCompleted(DailyQuestData quest)
	{
		// Could show notification
		GD.Print("[DailyQuestUI] Quest completed: " + quest.QuestName);
	}

	private void UpdateStats()
	{
		var stats = DailyQuestSystem.Instance.GetStatistics();
		_statsLabel.Text = $"总计完成: {stats["totalCompleted"]} | 已领取: {stats["totalClaimed"]} | 金币: {stats["totalGoldEarned"]} | 经验: {stats["totalExpEarned"]}";
	}

	public void ToggleUI()
	{
		Visible = !Visible;
		if (Visible)
		{
			RefreshQuestList();
		}
	}

	public override void _Input(InputEvent e)
	{
		if (e is InputEventKey keyEvent && keyEvent.Pressed)
		{
			if (keyEvent.Keycode == Key.Q)
			{
				ToggleUI();
			}
		}
	}
}
