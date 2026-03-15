using Godot;
using System;
using System.Collections.Generic;

public class CombatRatingUI : Control
{
	private CombatRatingSystem combatRatingSystem;
	
	// UI Elements
	private Label titleLabel;
	private Label currentScoreLabel;
	private Label currentStarsLabel;
	private Label gradeLabel;
	private ColorRect gradeColorRect;
	
	private TabContainer mainTabContainer;
	
	// Overview tab
	private Label totalBattlesLabel;
	private Label totalScoreLabel;
	private Label highestScoreLabel;
	private Label highestGradeLabel;
	private Label averageGradeLabel;
	private Label sssCountLabel;
	private Label noDamageCountLabel;
	private Label sessionBattlesLabel;
	private Label sessionScoreLabel;
	
	// History tab
	private ItemList historyList;
	
	// Test tab
	private Button testBattleButton;
	private Label testResultLabel;
	
	public override void _Ready()
	{
		// Get system
		combatRatingSystem = GetNode<CombatRatingSystem>("/root/Main/CombatRatingSystem");
		if (combatRatingSystem == null)
		{
			combatRatingSystem = new CombatRatingSystem();
			GetTree().Root.AddChild(combatRatingSystem);
		}
		
		SetupUI();
		ConnectSignals();
		UpdateDisplay();
	}
	
	private void SetupUI()
	{
		// Main container
		PanelContainer mainPanel = new PanelContainer();
		mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
		mainPanel.CustomMinimumSize = new Vector2(600, 500);
		AddChild(mainPanel);
		
		VBoxContainer mainVBox = new VBoxContainer();
		mainPanel.AddChild(mainVBox);
		
		// Title
		titleLabel = new Label();
		titleLabel.Text = "⚔️ Combat Rating System";
		titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
		titleLabel.AddThemeFontSizeOverride("font_size", 24);
		mainVBox.AddChild(titleLabel);
		
		// Current battle info
		PanelContainer currentPanel = new PanelContainer();
		currentPanel.AddThemeStyleboxOverride("panel", new StyleBoxFlat());
		mainVBox.AddChild(currentPanel);
		
		HBoxContainer currentHBox = new HBoxContainer();
		currentPanel.AddChild(currentHBox);
		
		// Score display
		VBoxContainer scoreBox = new VBoxContainer();
		currentHBox.AddChild(scoreBox);
		
		currentScoreLabel = new Label();
		currentScoreLabel.Text = "Score: 0";
		currentScoreLabel.AddThemeFontSizeOverride("font_size", 20);
		scoreBox.AddChild(currentScoreLabel);
		
		currentStarsLabel = new Label();
		currentStarsLabel.Text = "Stars: ★☆☆☆☆";
		currentStarsLabel.AddThemeFontSizeOverride("font_size", 18);
		scoreBox.AddChild(currentStarsLabel);
		
		// Grade display
		VBoxContainer gradeBox = new VBoxContainer();
		currentHBox.AddChild(gradeBox);
		
		gradeLabel = new Label();
		gradeLabel.Text = "Grade: -";
		gradeLabel.HorizontalAlignment = HorizontalAlignment.Center;
		gradeLabel.AddThemeFontSizeOverride("font_size", 18);
		gradeBox.AddChild(gradeLabel);
		
		gradeColorRect = new ColorRect();
		gradeColorRect.CustomMinimumSize = new Vector2(100, 30);
		gradeColorRect.Color = Colors.Gray;
		gradeBox.AddChild(gradeColorRect);
		
		// Tab container
		mainTabContainer = new TabContainer();
		mainTabContainer.CustomMinimumSize = new Vector2(580, 350);
		mainVBox.AddChild(mainTabContainer);
		
		// Overview tab
		Control overviewTab = new Control();
		overviewTab.Name = "Overview";
		mainTabContainer.AddChild(overviewTab);
		SetupOverviewTab(overviewTab);
		
		// History tab
		Control historyTab = new Control();
		historyTab.Name = "History";
		mainTabContainer.AddChild(historyTab);
		SetupHistoryTab(historyTab);
		
		// Test tab
		Control testTab = new Control();
		testTab.Name = "Test";
		mainTabContainer.AddChild(testTab);
		SetupTestTab(testTab);
		
		// Close button
		Button closeButton = new Button();
		closeButton.Text = "Close (ESC)";
		closeButton.Pressed += () => Hide();
		mainVBox.AddChild(closeButton);
	}
	
	private void SetupOverviewTab(Control tab)
	{
		VBoxContainer vbox = new VBoxContainer();
		vbox.SetAnchorsPreset(Control.LayoutPreset.Center);
		vbox.CustomMinimumSize = new Vector2(500, 300);
		tab.AddChild(vbox);
		
		// Statistics title
		Label statsTitle = new Label();
		statsTitle.Text = "📊 Statistics";
		statsTitle.HorizontalAlignment = HorizontalAlignment.Center;
		statsTitle.AddThemeFontSizeOverride("font_size", 20);
		vbox.AddChild(statsTitle);
		
		vbox.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 10) });
		
		// Grid for stats
		GridContainer grid = new GridContainer();
		grid.Columns = 2;
		grid.CustomMinimumSize = new Vector2(500, 250);
		vbox.AddChild(grid);
		
		// Total battles
		Label totalBattlesTitle = new Label();
		totalBattlesTitle.Text = "Total Battles:";
		grid.AddChild(totalBattlesTitle);
		
		totalBattlesLabel = new Label();
		totalBattlesLabel.Text = "0";
		totalBattlesLabel.HorizontalAlignment = HorizontalAlignment.Right;
		grid.AddChild(totalBattlesLabel);
		
		// Total score
		Label totalScoreTitle = new Label();
		totalScoreTitle.Text = "Total Score:";
		grid.AddChild(totalScoreTitle);
		
		totalScoreLabel = new Label();
		totalScoreLabel.Text = "0";
		totalScoreLabel.HorizontalAlignment = HorizontalAlignment.Right;
		grid.AddChild(totalScoreLabel);
		
		// Highest score
		Label highestScoreTitle = new Label();
		highestScoreTitle.Text = "Highest Score:";
		grid.AddChild(highestScoreTitle);
		
		highestScoreLabel = new Label();
		highestScoreLabel.Text = "0";
		highestScoreLabel.HorizontalAlignment = HorizontalAlignment.Right;
		grid.AddChild(highestScoreLabel);
		
		// Highest grade
		Label highestGradeTitle = new Label();
		highestGradeTitle.Text = "Highest Grade:";
		grid.AddChild(highestGradeTitle);
		
		highestGradeLabel = new Label();
		highestGradeLabel.Text = "-";
		highestGradeLabel.HorizontalAlignment = HorizontalAlignment.Right;
		grid.AddChild(highestGradeLabel);
		
		// Average grade
		Label averageGradeTitle = new Label();
		averageGradeTitle.Text = "Average Grade:";
		grid.AddChild(averageGradeTitle);
		
		averageGradeLabel = new Label();
		averageGradeLabel.Text = "-";
		averageGradeLabel.HorizontalAlignment = HorizontalAlignment.Right;
		grid.AddChild(averageGradeLabel);
		
		// SSS count
		Label sssCountTitle = new Label();
		sssCountTitle.Text = "SSS Count:";
		grid.AddChild(sssCountTitle);
		
		sssCountLabel = new Label();
		sssCountLabel.Text = "0";
		sssCountLabel.HorizontalAlignment = HorizontalAlignment.Right;
		grid.AddChild(sssCountLabel);
		
		// No damage count
		Label noDamageCountTitle = new Label();
		noDamageCountTitle.Text = "No Damage Battles:";
		grid.AddChild(noDamageCountTitle);
		
		noDamageCountLabel = new Label();
		noDamageCountLabel.Text = "0";
		noDamageCountLabel.HorizontalAlignment = HorizontalAlignment.Right;
		grid.AddChild(noDamageCountLabel);
		
		vbox.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 20) });
		
		// Session stats
		Label sessionTitle = new Label();
		sessionTitle.Text = "🎮 This Session";
		sessionTitle.HorizontalAlignment = HorizontalAlignment.Center;
		sessionTitle.AddThemeFontSizeOverride("font_size", 16);
		vbox.AddChild(sessionTitle);
		
		HBoxContainer sessionBox = new HBoxContainer();
		vbox.AddChild(sessionBox);
		
		VBoxContainer sessionLeft = new VBoxContainer();
		sessionBox.AddChild(sessionLeft);
		
		Label sessionBattlesTitle = new Label();
		sessionBattlesTitle.Text = "Battles:";
		sessionLeft.AddChild(sessionBattlesTitle);
		
		sessionBattlesLabel = new Label();
		sessionBattlesLabel.Text = "0";
		sessionLeft.AddChild(sessionBattlesLabel);
		
		VBoxContainer sessionRight = new VBoxContainer();
		sessionBox.AddChild(sessionRight);
		
		Label sessionScoreTitle = new Label();
		sessionScoreTitle.Text = "Score:";
		sessionRight.AddChild(sessionScoreTitle);
		
		sessionScoreLabel = new Label();
		sessionScoreLabel.Text = "0";
		sessionRight.AddChild(sessionScoreLabel);
	}
	
	private void SetupHistoryTab(Control tab)
	{
		VBoxContainer vbox = new VBoxContainer();
		vbox.SetAnchorsPreset(Control.LayoutPreset.Center);
		vbox.CustomMinimumSize = new Vector2(550, 320);
		tab.AddChild(vbox);
		
		Label historyTitle = new Label();
		historyTitle.Text = "📜 Battle History";
		historyTitle.HorizontalAlignment = HorizontalAlignment.Center;
		historyTitle.AddThemeFontSizeOverride("font_size", 18);
		vbox.AddChild(historyTitle);
		
		historyList = new ItemList();
		historyList.CustomMinimumSize = new Vector2(530, 250);
		vbox.AddChild(historyList);
		
		HBoxContainer buttonBox = new HBoxContainer();
		vbox.AddChild(buttonBox);
		
		Button clearButton = new Button();
		clearButton.Text = "Clear History";
		clearButton.Pressed += () => 
		{
			combatRatingSystem.ClearStatistics();
			UpdateDisplay();
		};
		buttonBox.AddChild(clearButton);
		
		Button refreshButton = new Button();
		refreshButton.Text = "Refresh";
		refreshButton.Pressed += () => UpdateHistoryList();
		buttonBox.AddChild(refreshButton);
	}
	
	private void SetupTestTab(Control tab)
	{
		VBoxContainer vbox = new VBoxContainer();
		vbox.SetAnchorsPreset(Control.LayoutPreset.Center);
		vbox.CustomMinimumSize = new Vector2(500, 300);
		tab.AddChild(vbox);
		
		Label testTitle = new Label();
		testTitle.Text = "🧪 Test Battle";
		testTitle.HorizontalAlignment = HorizontalAlignment.Center;
		testTitle.AddThemeFontSizeOverride("font_size", 18);
		vbox.AddChild(testTitle);
		
		vbox.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 20) });
		
		testBattleButton = new Button();
		testBattleButton.Text = "Start Test Battle";
		testBattleButton.CustomMinimumSize = new Vector2(200, 50);
		testBattleButton.Pressed += OnTestBattlePressed;
		vbox.AddChild(testBattleButton);
		
		vbox.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 20) });
		
		testResultLabel = new Label();
		testResultLabel.Text = "Click button to start a test battle";
		testResultLabel.HorizontalAlignment = HorizontalAlignment.Center;
		testResultLabel.AutowrapMode = TextServer.AutowrapMode.Word;
		vbox.AddChild(testResultLabel);
		
		vbox.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 20) });
		
		// Info
		Label infoLabel = new Label();
		infoLabel.Text = "This will simulate a battle with random stats\nand show the rating result.";
		infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
		infoLabel.AutowrapMode = TextServer.AutowrapMode.Word;
		vbox.AddChild(infoLabel);
	}
	
	private void ConnectSignals()
	{
		// Connect ESC to close
	}
	
	private void _Input(InputEvent @event)
	{
		if (@event.IsActionPressed("ui_cancel"))
		{
			Hide();
		}
	}
	
	private void OnTestBattlePressed()
	{
		if (testBattleButton.Text == "Start Test Battle")
		{
			// Start battle
			combatRatingSystem.StartBattle();
			testBattleButton.Text = "End Battle";
			testResultLabel.Text = "Battle in progress...";
			
			// Simulate some combat
			SimulateCombat();
		}
		else
		{
			// End battle
			var record = combatRatingSystem.EndBattle();
			testBattleButton.Text = "Start Test Battle";
			
			if (record != null)
			{
				string gradeName = combatRatingSystem.GetGradeName(record.grade);
				Color gradeColor = combatRatingSystem.GetGradeColor(record.grade);
				
				testResultLabel.Text = $"Grade: {gradeName}\n" +
					$"Score: {record.score}\n" +
					$"Stars: {new String('★', record.stars)}{new String('☆', 5 - record.stars)}\n" +
					$"Time: {record.timeTaken:F1}s\n" +
					$"Damage: {record.damageDealt} dealt / {record.damageTaken} taken\n" +
					$"Enemies: {record.enemiesDefeated}\n" +
					$"Gold Reward: {record.goldReward}\n" +
					$"EXP Reward: {record.expReward}";
				
				gradeColorRect.Color = gradeColor;
				gradeLabel.Text = "Grade: " + gradeName;
			}
			
			UpdateDisplay();
		}
	}
	
	private void SimulateCombat()
	{
		Random rand = new Random();
		
		// Simulate 10 hits
		for (int i = 0; i < 10; i++)
		{
			int damage = rand.Next(50, 150);
			bool isCrit = rand.Next(100) < 30;
			combatRatingSystem.RecordDamageDealt(damage, isCrit);
			combatRatingSystem.RecordComboHit();
		}
		
		// Simulate 5 enemies defeated
		for (int i = 0; i < 5; i++)
		{
			bool isElite = rand.Next(100) < 20;
			bool isBoss = rand.Next(100) < 5;
			combatRatingSystem.RecordEnemyDefeated(isElite, isBoss);
		}
		
		// Sometimes take damage
		if (rand.Next(100) < 70)
		{
			int damage = rand.Next(10, 50);
			combatRatingSystem.RecordDamageTaken(damage);
		}
		
		// Sometimes perfect dodges
		int dodges = rand.Next(0, 4);
		for (int i = 0; i < dodges; i++)
		{
			combatRatingSystem.RecordPerfectDodge();
		}
		
		// Update display
		UpdateCurrentBattleDisplay();
	}
	
	private void UpdateCurrentBattleDisplay()
	{
		if (combatRatingSystem == null) return;
		
		currentScoreLabel.Text = $"Score: {combatRatingSystem.GetCurrentScore()}";
		
		int stars = combatRatingSystem.GetCurrentStars();
		currentStarsLabel.Text = $"Stars: {new String('★', stars)}{new String('☆', 5 - stars)}";
	}
	
	private void UpdateDisplay()
	{
		if (combatRatingSystem == null) return;
		
		var stats = combatRatingSystem.GetStatistics();
		
		// Overview tab
		totalBattlesLabel.Text = stats.totalBattles.ToString();
		totalScoreLabel.Text = stats.totalScore.ToString();
		highestScoreLabel.Text = stats.highestScore.ToString();
		highestGradeLabel.Text = combatRatingSystem.GetGradeName(stats.highestGrade);
		averageGradeLabel.Text = ((int)stats.averageGrade).ToString();
		sssCountLabel.Text = stats.sssCount.ToString();
		noDamageCountLabel.Text = stats.noDamageCount.ToString();
		sessionBattlesLabel.Text = stats.sessionBattles.ToString();
		sessionScoreLabel.Text = stats.sessionScore.ToString();
		
		// History
		UpdateHistoryList();
	}
	
	private void UpdateHistoryList()
	{
		if (combatRatingSystem == null || historyList == null) return;
		
		historyList.Clear();
		
		var stats = combatRatingSystem.GetStatistics();
		var history = stats.ratingHistory;
		
		// Show last 20 battles (reversed)
		int start = Math.Max(0, history.Count - 20);
		for (int i = history.Count - 1; i >= start; i--)
		{
			var record = history[i];
			string gradeName = combatRatingSystem.GetGradeName(record.grade);
			Color gradeColor = combatRatingSystem.GetGradeColor(record.grade);
			
			string text = $"#{record.battleId} | {gradeName} | Score: {record.score} | " +
				$"{new String('★', record.stars)}{new String('☆', 5 - record.stars)} | " +
				$"{record.timeTaken:F1}s | {record.enemiesDefeated} enemies";
			
			int index = historyList.AddItem(text);
			
			// Color based on grade
			var color = new Color(gradeColor.R, gradeColor.G, gradeColor.B, 0.7f);
			historyList.SetItemCustomFgColor(index, color);
		}
	}
	
	public void Toggle()
	{
		if (Visible)
			Hide();
		else
		{
			Show();
			UpdateDisplay();
		}
	}
}
