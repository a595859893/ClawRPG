using Godot;
using System;
using System.Collections.Generic;

public class SeededRunUI : Control
{
    private SeededRunSystem _system;
    
    // UI Components
    private Label _titleLabel;
    private LineEdit _seedInput;
    private Button _generateSeedButton;
    private Button _startRunButton;
    private Button _cancelRunButton;
    private Label _currentSeedLabel;
    private Label _statusLabel;
    
    // Preset buttons
    private VBoxContainer _presetContainer;
    private Button[] _presetButtons;
    
    // Statistics panel
    private Label _totalRunsLabel;
    private Label _seedsCompletedLabel;
    private Label _completionRateLabel;
    private Label _bestFloorLabel;
    private Label _bestScoreLabel;
    
    // Seed history
    private ItemList _seedHistoryList;
    
    // Tab container
    private TabContainer _tabContainer;
    
    // Colors
    private Color _primaryColor = new Color(0.2f, 0.6f, 1.0f);
    private Color _successColor = new Color(0.2f, 0.8f, 0.4f);
    private Color _warningColor = new Color(1.0f, 0.6f, 0.2f);
    private Color _dangerColor = new Color(1.0f, 0.3f, 0.3f);
    private Color _goldColor = new Color(1.0f, 0.84f, 0.0f);
    
    public override void _Ready()
    {
        _system = SeededRunSystem.Instance;
        _system.Initialize();
        
        SetupUI();
        UpdateStatistics();
        UpdateSeedHistory();
    }
    
    private void SetupUI()
    {
        // Main container
        VBoxContainer mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(Control.LayoutPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🎲 Seeded Run System";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);
        
        // Setup tabs
        SetupStartTab();
        SetupPresetsTab();
        SetupStatisticsTab();
        SetupHistoryTab();
    }
    
    private void SetupStartTab()
    {
        VBoxContainer startTab = new VBoxContainer();
        startTab.Name = "Start";
        startTab.AddThemeConstantOverride("separation", 15);
        _tabContainer.AddChild(startTab);
        
        // Seed input section
        HBoxContainer seedSection = new HBoxContainer();
        seedSection.Alignment = BoxContainer.AlignmentMode.Center;
        startTab.AddChild(seedSection);
        
        Label seedLabel = new Label();
        seedLabel.Text = "Seed: ";
        seedSection.AddChild(seedLabel);
        
        _seedInput = new LineEdit();
        _seedInput.Placeholder = "Enter seed (4-16 characters)";
        _seedInput.CustomMinimumSize = new Vector2(200, 0);
        _seedSection.AddChild(_seedInput);
        
        _generateSeedButton = new Button();
        _generateSeedButton.Text = "🎲 Generate";
        _generateSeedButton.Pressed += OnGenerateSeedPressed;
        seedSection.AddChild(_generateSeedButton);
        
        // Current seed display
        _currentSeedLabel = new Label();
        _currentSeedLabel.Text = "Current Seed: None";
        _currentSeedLabel.Align = Label.AlignEnum.Center;
        _currentSeedLabel.AddThemeColorOverride("font_color", _primaryColor);
        startTab.AddChild(_currentSeedLabel);
        
        // Status
        _statusLabel = new Label();
        _statusLabel.Text = "Seeded Mode: Inactive";
        _statusLabel.Align = Label.AlignEnum.Center;
        startTab.AddChild(_statusLabel);
        
        // Buttons
        HBoxContainer buttonSection = new HBoxContainer();
        buttonSection.Alignment = BoxContainer.AlignmentMode.Center;
        buttonSection.AddThemeConstantOverride("separation", 20);
        startTab.AddChild(buttonSection);
        
        _startRunButton = new Button();
        _startRunButton.Text = "▶ Start Seeded Run";
        _startRunButton.CustomMinimumSize = new Vector2(180, 40);
        _startRunButton.Pressed += OnStartRunPressed;
        buttonSection.AddChild(_startRunButton);
        
        _cancelRunButton = new Button();
        _cancelRunButton.Text = "⏹ Cancel Run";
        _cancelRunButton.CustomMinimumSize = new Vector2(150, 40);
        _cancelRunButton.Pressed += OnCancelRunPressed;
        _cancelRunButton.Disabled = true;
        buttonSection.AddChild(_cancelRunButton);
        
        // Quick actions
        Label quickLabel = new Label();
        quickLabel.Text = "Quick Start:";
        quickLabel.Align = Label.AlignEnum.Center;
        startTab.AddChild(quickLabel);
        
        HBoxContainer quickSection = new HBoxContainer();
        quickSection.Alignment = BoxContainer.AlignmentMode.Center;
        quickSection.AddThemeConstantOverride("separation", 10);
        startTab.AddChild(quickSection);
        
        Button randomSeedButton = new Button();
        randomSeedButton.Text = "🎲 Random Seed";
        randomSeedButton.Pressed += OnRandomSeedPressed;
        quickSection.AddChild(randomSeedButton);
        
        Button continueButton = new Button();
        continueButton.Text = "🔄 Continue Last Seed";
        continueButton.Pressed += OnContinueLastSeedPressed;
        quickSection.AddChild(continueButton);
    }
    
    private void SetupPresetsTab()
    {
        VBoxContainer presetsTab = new VBoxContainer();
        presetsTab.Name = "Presets";
        presetsTab.AddThemeConstantOverride("separation", 10);
        _tabContainer.AddChild(presetsTab);
        
        Label presetTitle = new Label();
        presetTitle.Text = "Seed Presets";
        presetTitle.Align = Label.AlignEnum.Center;
        presetTitle.AddThemeFontSizeOverride("font_size", 18);
        presetsTab.AddChild(presetTitle);
        
        ScrollContainer scrollContainer = new ScrollContainer();
        scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        presetsTab.AddChild(scrollContainer);
        
        _presetContainer = new VBoxContainer();
        _presetContainer.AddThemeConstantOverride("separation", 8);
        scrollContainer.AddChild(_presetContainer);
        
        // Create preset buttons
        var presets = SeededRunDatabase.Instance.SeedPresets;
        foreach (var preset in presets.Values)
        {
            CreatePresetCard(preset);
        }
    }
    
    private void CreatePresetCard(SeedPreset preset)
    {
        VBoxContainer card = new VBoxContainer();
        card.AddThemeConstantOverride("separation", 5);
        _presetContainer.AddChild(card);
        
        HBoxContainer header = new HBoxContainer();
        header.AddThemeConstantOverride("separation", 10);
        card.AddChild(header);
        
        Label nameLabel = new Label();
        nameLabel.Text = "📋 " + preset.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        header.AddChild(nameLabel);
        
        Label difficultyLabel = new Label();
        difficultyLabel.Text = " [" + preset.Difficulty + "]";
        
        // Color based on difficulty
        Color diffColor = _primaryColor;
        switch (preset.Difficulty)
        {
            case "Easy": diffColor = _successColor; break;
            case "Normal": diffColor = _primaryColor; break;
            case "Hard": diffColor = _warningColor; break;
            case "Nightmare":
            case "Legendary": diffColor = _dangerColor; break;
        }
        difficultyLabel.AddThemeColorOverride("font_color", diffColor);
        header.AddChild(difficultyLabel);
        
        Label descLabel = new Label();
        descLabel.Text = preset.Description;
        descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        card.AddChild(descLabel);
        
        // Special rules
        if (preset.SpecialRules.Count > 0)
        {
            Label rulesLabel = new Label();
            rulesLabel.Text = "⚡ " + string.Join(", ", preset.SpecialRules);
            rulesLabel.AddThemeColorOverride("font_color", _goldColor);
            card.AddChild(rulesLabel);
        }
        
        // Start button for this preset
        Button startPresetButton = new Button();
        startPresetButton.Text = "▶ Start with " + preset.Name;
        startPresetButton.Pressed += () => OnPresetStartPressed(preset.Id);
        card.AddChild(startPresetButton);
        
        // Separator
        HSeparator sep = new HSeparator();
        _presetContainer.AddChild(sep);
    }
    
    private void SetupStatisticsTab()
    {
        VBoxContainer statsTab = new VBoxContainer();
        statsTab.Name = "Statistics";
        statsTab.AddThemeConstantOverride("separation", 15);
        _tabContainer.AddChild(statsTab);
        
        // Stats grid
        GridContainer statsGrid = new GridContainer();
        statsGrid.Columns = 2;
        statsGrid.AddThemeConstantOverride("h_separation", 20);
        statsGrid.AddThemeConstantOverride("v_separation", 10);
        statsTab.AddChild(statsGrid);
        
        // Total runs
        _totalRunsLabel = CreateStatRow(statsGrid, "Total Seeded Runs:", "0");
        
        // Seeds completed
        _seedsCompletedLabel = CreateStatRow(statsGrid, "Seeds Completed:", "0");
        
        // Completion rate
        _completionRateLabel = CreateStatRow(statsGrid, "Completion Rate:", "0%");
        
        // Best floor
        _bestFloorLabel = CreateStatRow(statsGrid, "Best Floor:", "0");
        
        // Best score
        _bestScoreLabel = CreateStatRow(statsGrid, "Best Score:", "0");
    }
    
    private Label CreateStatRow(GridContainer parent, string label, string value)
    {
        Label labelWidget = new Label();
        labelWidget.Text = label;
        labelWidget.Align = Label.AlignEnum.Right;
        parent.AddChild(labelWidget);
        
        Label valueWidget = new Label();
        valueWidget.Text = value;
        valueWidget.AddThemeColorOverride("font_color", _primaryColor);
        valueWidget.AddThemeFontSizeOverride("font_size", 18);
        parent.AddChild(valueWidget);
        
        return valueWidget;
    }
    
    private void SetupHistoryTab()
    {
        VBoxContainer historyTab = new VBoxContainer();
        historyTab.Name = "History";
        _tabContainer.AddChild(historyTab);
        
        Label historyTitle = new Label();
        historyTitle.Text = "Seed History";
        historyTitle.Align = Label.AlignEnum.Center;
        historyTitle.AddThemeFontSizeOverride("font_size", 18);
        historyTab.AddChild(historyTitle);
        
        _seedHistoryList = new ItemList();
        _seedHistoryList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        historyTab.AddChild(_seedHistoryList);
        
        // Selected seed info
        _seedHistoryList.ItemSelected += OnSeedHistorySelected;
        
        // Info panel for selected seed
        Label selectedInfo = new Label();
        selectedInfo.Name = "SelectedInfo";
        selectedInfo.Text = "Select a seed to view details";
        historyTab.AddChild(selectedInfo);
    }
    
    private void UpdateStatistics()
    {
        _totalRunsLabel.Text = _system.TotalSeededRuns.ToString();
        
        int completedSeeds = 0;
        int bestFloor = 0;
        int bestScore = 0;
        
        var records = _system.GetAllSeedRecords();
        foreach (var record in records.Values)
        {
            if (record.Completed) completedSeeds++;
            if (record.BestFloor > bestFloor) bestFloor = record.BestFloor;
            if (record.BestScore > bestScore) bestScore = record.BestScore;
        }
        
        _seedsCompletedLabel.Text = completedSeeds.ToString();
        _completionRateLabel.Text = (completedSeeds > 0 ? (completedSeeds * 100 / records.Count) : 0).ToString() + "%";
        _bestFloorLabel.Text = bestFloor.ToString();
        _bestScoreLabel.Text = bestScore.ToString();
    }
    
    private void UpdateSeedHistory()
    {
        _seedHistoryList.Clear();
        
        var records = _system.GetAllSeedRecords();
        foreach (var record in records.Values)
        {
            string displayText = record.Seed;
            if (record.Completed)
            {
                displayText += " ✅";
            }
            displayText += $" (Floor {record.BestFloor}, Runs: {record.RunCount})";
            
            _seedHistoryList.AddItem(displayText);
        }
    }
    
    private void UpdateUI()
    {
        // Update current seed display
        if (_system.IsSeededModeActive)
        {
            _currentSeedLabel.Text = "Current Seed: " + _system.CurrentSeed;
            _statusLabel.Text = "Seeded Mode: Active";
            _statusLabel.AddThemeColorOverride("font_color", _successColor);
            _cancelRunButton.Disabled = false;
        }
        else
        {
            _currentSeedLabel.Text = "Current Seed: None";
            _statusLabel.Text = "Seeded Mode: Inactive";
            _statusLabel.AddThemeColorOverride("font_color", _dangerColor);
            _cancelRunButton.Disabled = true;
        }
        
        UpdateStatistics();
        UpdateSeedHistory();
    }
    
    // Event handlers
    private void OnGenerateSeedPressed()
    {
        string newSeed = _system.GenerateNewSeed();
        _seedInput.Text = newSeed;
    }
    
    private void OnStartRunPressed()
    {
        string seed = _seedInput.Text.Trim();
        if (string.IsNullOrEmpty(seed))
        {
            GD.Print("[SeededRunUI] No seed entered");
            return;
        }
        
        if (_system.StartSeededRun(seed))
        {
            UpdateUI();
        }
    }
    
    private void OnCancelRunPressed()
    {
        _system.CancelSeededRun();
        UpdateUI();
    }
    
    private void OnRandomSeedPressed()
    {
        string seed = _system.GenerateNewSeed();
        _seedInput.Text = seed;
        _system.StartSeededRun(seed);
        UpdateUI();
    }
    
    private void OnContinueLastSeedPressed()
    {
        var data = SeededRunSystem.Instance;
        if (!string.IsNullOrEmpty(data.CurrentSeed))
        {
            _seedInput.Text = data.CurrentSeed;
            data.StartSeededRun(data.CurrentSeed);
            UpdateUI();
        }
    }
    
    private void OnPresetStartPressed(string presetId)
    {
        var preset = _system.GetPreset(presetId);
        if (preset != null)
        {
            // Generate a unique seed for this preset
            string seed = presetId.ToUpper() + _system.GenerateNewSeed().Substring(0, 4);
            _seedInput.Text = seed;
            _system.StartSeededRun(seed);
            UpdateUI();
        }
    }
    
    private void OnSeedHistorySelected(int index)
    {
        var records = _system.GetAllSeedRecords();
        int i = 0;
        foreach (var record in records.Values)
        {
            if (i == index)
            {
                var infoLabel = _seedHistoryList.GetParent().GetParent().GetNode<Label>("SelectedInfo");
                if (infoLabel != null)
                {
                    string info = $"Seed: {record.Seed}\n";
                    info += $"Runs: {record.RunCount}, Completed: {(record.Completed ? "Yes" : "No")}\n";
                    info += $"Best Floor: {record.BestFloor}, Best Score: {record.BestScore}\n";
                    info += $"Total Gold: {record.TotalGold}, Total Exp: {record.TotalExp}\n";
                    info += $"Enemies Defeated: {record.EnemiesDefeated}, Bosses: {record.BossesDefeated}\n";
                    info += $"Last Played: {record.LastPlayed}";
                    infoLabel.Text = info;
                }
                break;
            }
            i++;
        }
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
        }
    }
}
