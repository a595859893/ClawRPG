using Godot;
using System;
using System.Collections.Generic;

public partial class DungeonExpeditionUI : Control
{
    private VBoxContainer _mainContainer;
    private TabContainer _tabContainer;
    
    // Dungeon list tab
    private ScrollContainer _dungeonListScroll;
    private VBoxContainer _dungeonListContainer;
    
    // Current expedition tab
    private Label _expeditionTitle;
    private Label _expeditionStatus;
    private ProgressBar _floorProgress;
    private Label _floorLabel;
    private Label _enemiesLabel;
    private Label _goldLabel;
    private Label _expLabel;
    private Button _completeFloorButton;
    private Button _abandonButton;
    
    // Stats tab
    private Label _totalGoldLabel;
    private Label _totalExpLabel;
    private Label _totalWinsLabel;
    private Label _totalAttemptsLabel;
    private ScrollContainer _dungeonStatsContainer;
    private VBoxContainer _dungeonStatsList;

    private bool _isVisible = false;

    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        Hide();
    }

    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchor(AnchorPreset.FullRect);
        _mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(_mainContainer);

        // Title
        Label title = new Label();
        title.Text = "🏰 Dungeon Expedition";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(title);

        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _mainContainer.AddChild(_tabContainer);

        // Create tabs
        CreateDungeonListTab();
        CreateExpeditionTab();
        CreateStatsTab();

        // Close button
        Button closeButton = new Button();
        closeButton.Text = "  Close (ESC)  ";
        closeButton.Alignment = HorizontalAlignment.Center;
        closeButton.Pressed += () => ToggleVisibility();
        _mainContainer.AddChild(closeButton);
    }

    private void CreateDungeonListTab()
    {
        _dungeonListScroll = new ScrollContainer();
        _dungeonListScroll.Name = "Dungeons";
        _tabContainer.AddChild(_dungeonListScroll);

        _dungeonListContainer = new VBoxContainer();
        _dungeonListContainer.AddThemeConstantOverride("separation", 10);
        _dungeonListScroll.AddChild(_dungeonListContainer);

        RefreshDungeonList();
    }

    private void CreateExpeditionTab()
    {
        VBoxContainer expeditionContainer = new VBoxContainer();
        expeditionContainer.Name = "Current";
        expeditionContainer.AddThemeConstantOverride("separation", 15);
        _tabContainer.AddChild(expeditionContainer);

        // Expedition title
        _expeditionTitle = new Label();
        _expeditionTitle.Text = "No Active Expedition";
        _expeditionTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _expeditionTitle.AddThemeFontSizeOverride("font_size", 20);
        expeditionContainer.AddChild(_expeditionTitle);

        // Status
        _expeditionStatus = new Label();
        _expeditionStatus.Text = "Start a new expedition from the Dungeons tab";
        _expeditionStatus.HorizontalAlignment = HorizontalAlignment.Center;
        expeditionContainer.AddChild(_expeditionStatus);

        // Floor progress
        HBoxContainer progressContainer = new HBoxContainer();
        progressContainer.Alignment = BoxContainer.AlignmentMode.Center;
        expeditionContainer.AddChild(progressContainer);

        Label floorTitle = new Label();
        floorTitle.Text = "Floor: ";
        progressContainer.AddChild(floorTitle);

        _floorProgress = new ProgressBar();
        _floorProgress.CustomMinimumSize = new Vector2(200, 20);
        _floorProgress.ShowPercentage = false;
        progressContainer.AddChild(_floorProgress);

        _floorLabel = new Label();
        _floorLabel.Text = " 0/0";
        progressContainer.AddChild(_floorLabel);

        // Stats
        _enemiesLabel = new Label();
        _enemiesLabel.Text = "Enemies Defeated: 0";
        expeditionContainer.AddChild(_enemiesLabel);

        _goldLabel = new Label();
        _goldLabel.Text = "Gold Earned: 0";
        expeditionContainer.AddChild(_goldLabel);

        _expLabel = new Label();
        _expLabel.Text = "Experience Earned: 0";
        expeditionContainer.AddChild(_expLabel);

        // Buttons
        HBoxContainer buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        buttonContainer.AddThemeConstantOverride("separation", 20);
        expeditionContainer.AddChild(buttonContainer);

        _completeFloorButton = new Button();
        _completeFloorButton.Text = "Complete Floor (+Rewards)";
        _completeFloorButton.Pressed += OnCompleteFloorPressed;
        buttonContainer.AddChild(_completeFloorButton);

        _abandonButton = new Button();
        _abandonButton.Text = "Abandon Expedition";
        _abandonButton.Pressed += OnAbandonPressed;
        buttonContainer.AddChild(_abandonButton);

        UpdateExpeditionUI();
    }

    private void CreateStatsTab()
    {
        VBoxContainer statsContainer = new VBoxContainer();
        statsContainer.Name = "Statistics";
        statsContainer.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(statsContainer);

        // Summary
        HBoxContainer summaryContainer = new HBoxContainer();
        summaryContainer.Alignment = BoxContainer.AlignmentMode.Center;
        statsContainer.AddChild(summaryContainer);

        _totalGoldLabel = new Label();
        _totalGoldLabel.Text = "Total Gold: 0";
        summaryContainer.AddChild(_totalGoldLabel);

        Label spacer = new Label();
        spacer.CustomMinimumSize = new Vector2(40, 0);
        summaryContainer.AddChild(spacer);

        _totalExpLabel = new Label();
        _totalExpLabel.Text = "Total Exp: 0";
        summaryContainer.AddChild(_totalExpLabel);

        HBoxContainer winsContainer = new HBoxContainer();
        winsContainer.Alignment = BoxContainer.AlignmentMode.Center;
        statsContainer.AddChild(winsContainer);

        _totalWinsLabel = new Label();
        _totalWinsLabel.Text = "Total Wins: 0";
        winsContainer.AddChild(_totalWinsLabel);

        Label spacer2 = new Label();
        spacer2.CustomMinimumSize = new Vector2(40, 0);
        winsContainer.AddChild(spacer2);

        _totalAttemptsLabel = new Label();
        _totalAttemptsLabel.Text = "Total Attempts: 0";
        winsContainer.AddChild(_totalAttemptsLabel);

        // Dungeon stats list
        Label dungeonStatsTitle = new Label();
        dungeonStatsTitle.Text = "Dungeon Progress";
        dungeonStatsTitle.AddThemeFontSizeOverride("font_size", 18);
        statsContainer.AddChild(dungeonStatsTitle);

        _dungeonStatsContainer = new ScrollContainer();
        _dungeonStatsContainer.CustomMinimumSize = new Vector2(0, 300);
        statsContainer.AddChild(_dungeonStatsContainer);

        _dungeonStatsList = new VBoxContainer();
        _dungeonStatsList.AddThemeConstantOverride("separation", 5);
        _dungeonStatsContainer.AddChild(_dungeonStatsList);

        RefreshStatsUI();
    }

    private void ConnectSignals()
    {
        if (DungeonExpeditionSystem.Instance != null)
        {
            DungeonExpeditionSystem.Instance.ExpeditionStarted += OnExpeditionStarted;
            DungeonExpeditionSystem.Instance.FloorCompleted += OnFloorCompleted;
            DungeonExpeditionSystem.Instance.ExpeditionCompleted += OnExpeditionCompleted;
            DungeonExpeditionSystem.Instance.ExpeditionAbandoned += OnExpeditionAbandoned;
        }
    }

    private void RefreshDungeonList()
    {
        // Clear existing
        foreach (Node child in _dungeonListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var dungeons = DungeonExpeditionSystem.Instance.GetAllDungeons();
        var progress = DungeonExpeditionSystem.Instance.GetPlayerProgress();

        foreach (var dungeon in dungeons)
        {
            bool unlocked = DungeonExpeditionSystem.Instance.IsDungeonUnlocked(dungeon.Type);
            
            // Create dungeon card
            PanelContainer card = new PanelContainer();
            card.CustomMinimumSize = new Vector2(0, 80);
            
            HBoxContainer cardContent = new HBoxContainer();
            cardContent.AddThemeConstantOverride("separation", 15);
            cardContent.Alignment = BoxContainer.AlignmentMode.Center;
            card.AddChild(cardContent);

            // Icon
            Label icon = new Label();
            icon.Text = GetDungeonIcon(dungeon.Type);
            icon.AddThemeFontSizeOverride("font_size", 32);
            cardContent.AddChild(icon);

            // Info
            VBoxContainer info = new VBoxContainer();
            info.Alignment = BoxContainer.AlignmentMode.Center;
            cardContent.AddChild(info);

            Label nameLabel = new Label();
            nameLabel.Text = dungeon.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.Modulate = unlocked ? Colors.White : Colors.Gray;
            info.AddChild(nameLabel);

            Label descLabel = new Label();
            descLabel.Text = dungeon.Description;
            descLabel.Modulate = unlocked ? Colors.LightGray : Colors.Gray;
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            info.AddChild(descLabel);

            Label levelLabel = new Label();
            levelLabel.Text = $"Recommended Level: {dungeon.RecommendedLevel} | Floors: {dungeon.FloorCount}";
            levelLabel.Modulate = unlocked ? Colors.Yellow : Colors.Gray;
            levelLabel.AddThemeFontSizeOverride("font_size", 12);
            info.AddChild(levelLabel);

            // Progress
            if (progress.BestFloor.ContainsKey(dungeon.Type))
            {
                Label bestLabel = new Label();
                bestLabel.Text = $"Best: Floor {progress.BestFloor[dungeon.Type]}";
                bestLabel.Modulate = Colors.Green;
                bestLabel.AddThemeFontSizeOverride("font_size", 12);
                info.AddChild(bestLabel);
            }

            // Action button
            if (unlocked)
            {
                Button startButton = new Button();
                startButton.Text = "Start";
                startButton.Pressed += () => OnStartDungeonPressed(dungeon.Type);
                cardContent.AddChild(startButton);
            }
            else
            {
                Label lockedLabel = new Label();
                lockedLabel.Text = "🔒 Locked";
                lockedLabel.Modulate = Colors.Red;
                cardContent.AddChild(lockedLabel);
            }

            _dungeonListContainer.AddChild(card);
        }
    }

    private void RefreshStatsUI()
    {
        var progress = DungeonExpeditionSystem.Instance.GetPlayerProgress();
        
        _totalGoldLabel.Text = $"Total Gold: {progress.TotalGoldEarned:N0}";
        _totalExpLabel.Text = $"Total Exp: {progress.TotalExpEarned:N0}";
        
        int totalWins = 0;
        int totalAttempts = 0;
        foreach (var kvp in progress.TotalWins)
            totalWins += kvp.Value;
        foreach (var kvp in progress.TotalAttempts)
            totalAttempts += kvp.Value;
        
        _totalWinsLabel.Text = $"Total Wins: {totalWins}";
        _totalAttemptsLabel.Text = $"Total Attempts: {totalAttempts}";

        // Clear and refresh dungeon stats
        foreach (Node child in _dungeonStatsList.GetChildren())
        {
            child.QueueFree();
        }

        var dungeons = DungeonExpeditionSystem.Instance.GetAllDungeons();
        foreach (var dungeon in dungeons)
        {
            bool unlocked = DungeonExpeditionSystem.Instance.IsDungeonUnlocked(dungeon.Type);
            int wins = progress.TotalWins.ContainsKey(dungeon.Type) ? progress.TotalWins[dungeon.Type] : 0;
            int attempts = progress.TotalAttempts.ContainsKey(dungeon.Type) ? progress.TotalAttempts[dungeon.Type] : 0;
            int best = progress.BestFloor.ContainsKey(dungeon.Type) ? progress.BestFloor[dungeon.Type] : 0;

            HBoxContainer statRow = new HBoxContainer();
            statRow.AddThemeConstantOverride("separation", 10);

            Label nameLabel = new Label();
            nameLabel.Text = dungeon.Name;
            nameLabel.CustomMinimumSize = new Vector2(150, 0);
            nameLabel.Modulate = unlocked ? Colors.White : Colors.Gray;
            statRow.AddChild(nameLabel);

            Label winsLabel = new Label();
            winsLabel.Text = $"Wins: {wins}";
            winsLabel.CustomMinimumSize = new Vector2(80, 0);
            statRow.AddChild(winsLabel);

            Label attemptsLabel = new Label();
            attemptsLabel.Text = $"Attempts: {attempts}";
            attemptsLabel.CustomMinimumSize = new Vector2(100, 0);
            statRow.AddChild(attemptsLabel);

            Label bestLabel = new Label();
            bestLabel.Text = $"Best: {best}/{dungeon.FloorCount}";
            bestLabel.Modulate = Colors.Yellow;
            statRow.AddChild(bestLabel);

            _dungeonStatsList.AddChild(statRow);
        }
    }

    private void UpdateExpeditionUI()
    {
        var expedition = DungeonExpeditionSystem.Instance.GetCurrentExpedition();
        
        if (expedition == null || expedition.Status != DungeonExpeditionSystem.ExpeditionStatus.InProgress)
        {
            _expeditionTitle.Text = "No Active Expedition";
            _expeditionStatus.Text = "Start a new expedition from the Dungeons tab";
            _floorProgress.Visible = false;
            _floorLabel.Text = "";
            _enemiesLabel.Text = "Enemies Defeated: 0";
            _goldLabel.Text = "Gold Earned: 0";
            _expLabel.Text = "Experience Earned: 0";
            _completeFloorButton.Disabled = true;
            _abandonButton.Disabled = true;
            return;
        }

        var dungeon = DungeonExpeditionSystem.Instance.GetDungeonData(expedition.DungeonType);
        
        _expeditionTitle.Text = dungeon.Name;
        _expeditionStatus.Text = $"{DungeonExpeditionSystem.GetDifficultyName(expedition.Difficulty)} - In Progress";
        _floorProgress.Visible = true;
        _floorProgress.MaxValue = expedition.MaxFloor;
        _floorProgress.Value = expedition.CurrentFloor;
        _floorLabel.Text = $" {expedition.CurrentFloor}/{expedition.MaxFloor}";
        _enemiesLabel.Text = $"Enemies Defeated: {expedition.EnemiesDefeated}";
        _goldLabel.Text = $"Gold Earned: {expedition.GoldEarned:N0}";
        _expLabel.Text = $"Experience Earned: {expedition.ExpEarned:N0}";
        _completeFloorButton.Disabled = false;
        _abandonButton.Disabled = false;
    }

    private string GetDungeonIcon(DungeonExpeditionSystem.DungeonType type)
    {
        return type switch
        {
            DungeonExpeditionSystem.DungeonType.AncientRuins => "🏛️",
            DungeonExpeditionSystem.DungeonType.CrystalCavern => "💎",
            DungeonExpeditionSystem.DungeonType.ShadowCrypt => "⚰️",
            DungeonExpeditionSystem.DungeonType.DragonLair => "🐉",
            DungeonExpeditionSystem.DungeonType.FrozenFortress => "❄️",
            DungeonExpeditionSystem.DungeonType.VolcanicDepths => "🌋",
            DungeonExpeditionSystem.DungeonType.EnchantedForest => "🌲",
            DungeonExpeditionSystem.DungeonType.AbyssalPit => "🕳️",
            DungeonExpeditionSystem.DungeonType.HeavenlyTemple => "⛩️",
            DungeonExpeditionSystem.DungeonType.DemonCastle => "👹",
            _ => "🏰"
        };
    }

    private void OnStartDungeonPressed(DungeonExpeditionSystem.DungeonType type)
    {
        // Show difficulty selection
        var difficultyDialog = new AcceptDialog();
        difficultyDialog.Title = "Select Difficulty";
        
        VBoxContainer dialogContent = new VBoxContainer();
        
        Label infoLabel = new Label();
        infoLabel.Text = "Select difficulty for this expedition:";
        dialogContent.AddChild(infoLabel);
        
        // Create difficulty buttons
        foreach (DungeonExpeditionSystem.Difficulty difficulty in Enum.GetValues(typeof(DungeonExpeditionSystem.Difficulty)))
        {
            Button diffButton = new Button();
            diffButton.Text = DungeonExpeditionSystem.GetDifficultyName(difficulty);
            diffButton.Pressed += () => {
                if (DungeonExpeditionSystem.Instance.StartExpedition(type, difficulty))
                {
                    _tabContainer.CurrentTab = 1; // Switch to current tab
                    RefreshDungeonList();
                    RefreshStatsUI();
                }
                difficultyDialog.Hide();
            };
            dialogContent.AddChild(diffButton);
        }
        
        difficultyDialog.AddChild(dialogContent);
        AddChild(difficultyDialog);
        difficultyDialog.PopupCentered(new Vector2(300, 250));
    }

    private void OnCompleteFloorPressed()
    {
        var expedition = DungeonExpeditionSystem.Instance.GetCurrentExpedition();
        if (expedition == null) return;

        var (gold, exp, items) = DungeonExpeditionSystem.Instance.CalculateFloorRewards(expedition.CurrentFloor);
        
        // Add rewards
        if (Player.Instance != null)
        {
            Player.Instance.AddGold(gold);
            Player.Instance.AddExperience(exp);
        }
        
        DungeonExpeditionSystem.Instance.CompleteFloor(
            Godot.RandomNumberGenerator.Randi() % 5 + 3,
            gold,
            exp,
            items
        );

        RefreshStatsUI();
    }

    private void OnAbandonPressed()
    {
        DungeonExpeditionSystem.Instance.AbandonExpedition();
        RefreshDungeonList();
        RefreshStatsUI();
    }

    private void OnExpeditionStarted(int dungeonType, int difficulty)
    {
        UpdateExpeditionUI();
    }

    private void OnFloorCompleted(int currentFloor, int maxFloor)
    {
        UpdateExpeditionUI();
    }

    private void OnExpeditionCompleted(int dungeonType, int success, int gold, int exp)
    {
        UpdateExpeditionUI();
        RefreshDungeonList();
        RefreshStatsUI();
        
        // Show completion message
        if (success == 1)
        {
            GD.Print($"Expedition completed! Gold: {gold}, Exp: {exp}");
        }
    }

    private void OnExpeditionAbandoned(int dungeonType)
    {
        UpdateExpeditionUI();
        RefreshDungeonList();
        RefreshStatsUI();
    }

    public void ToggleVisibility()
    {
        if (_isVisible)
        {
            Hide();
            _isVisible = false;
        }
        else
        {
            Show();
            RefreshDungeonList();
            RefreshStatsUI();
            UpdateExpeditionUI();
            _isVisible = true;
        }
    }

    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            if (_isVisible)
            {
                ToggleVisibility();
                GetViewport().SetInputAsHandled();
            }
        }
        
        // D key toggle
        if (evt is InputEventKey keyEvent2 && keyEvent2.Pressed && keyEvent2.Keycode == Key.D)
        {
            if (keyEvent2.Ctrl && !keyEvent2.Shift && !keyEvent2.Alt)
            {
                ToggleVisibility();
                GetViewport().SetInputAsHandled();
            }
        }
    }

    public override void _Process(double delta)
    {
        // Auto-refresh every second if expedition is active
        if (_isVisible)
        {
            var expedition = DungeonExpeditionSystem.Instance.GetCurrentExpedition();
            if (expedition != null && expedition.Status == DungeonExpeditionSystem.ExpeditionStatus.InProgress)
            {
                UpdateExpeditionUI();
            }
        }
    }
}
