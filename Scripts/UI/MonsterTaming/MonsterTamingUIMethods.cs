using Godot;
using Godot.Collections;
using System;

public partial class MonsterTamingUI
{
    public override void _Ready()
    {
        Instance = this;
        SetupUI();
        SetupInput();
        Hide();
    }
    
    private void SetupUI()
    {
        // Main Panel
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(900, 600);
        AddChild(_mainPanel);
        
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        styleBox.CornerRadiusTopLeft = 8;
        styleBox.CornerRadiusTopRight = 8;
        styleBox.CornerRadiusBottomLeft = 8;
        styleBox.CornerRadiusBottomRight = 8;
        _mainPanel.AddThemeStyleboxOverride("panel", styleBox);
        
        // Content Box
        _contentBox = new VBoxContainer();
        _contentBox.SetCustomMinimumSize(new Vector2(880, 580));
        _contentBox.AddThemeConstantOverride("separation", 10);
        _mainPanel.AddChild(_contentBox);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "  🐾 Monster Taming System";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f));
        _contentBox.AddChild(titleLabel);
        
        // Tab Container
        _tabContainer = new TabContainer();
        _tabContainer.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        _tabContainer.SetVExpandFlags(Control.ExpandFlags.ExpandFill);
        _contentBox.AddChild(_tabContainer);
        
        // Setup Wild Monsters Tab
        SetupWildTab();
        
        // Setup Tamed Monsters Tab
        SetupTamedTab();
        
        // Setup Stats Tab
        SetupStatsTab();
        
        // Info Panel (bottom)
        SetupInfoPanel();
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "  ✕ Close  ";
        closeButton.CustomMinimumSize = new Vector2(120, 35);
        closeButton.Pressed += () => ToggleUI();
        
        var buttonBox = new HBoxContainer();
        buttonBox.AddThemeConstantOverride("separation", 10);
        buttonBox.AddChild(closeButton);
        
        var spacer = new Control();
        spacer.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        buttonBox.AddChild(spacer);
        
        var hintLabel = new Label();
        hintLabel.Text = "Press T to toggle";
        hintLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
        buttonBox.AddChild(hintLabel);
        
        _contentBox.AddChild(buttonBox);
    }
    
    private void SetupWildTab()
    {
        var wildTab = new VBoxContainer();
        wildTab.SetName("Wild");
        _tabContainer.AddChild(wildTab);
        
        var headerBox = new HBoxContainer();
        headerBox.AddThemeConstantOverride("separation", 10);
        
        var wildTitle = new Label();
        wildTitle.Text = "Wild Monsters";
        wildTitle.AddThemeFontSizeOverride("font_size", 18);
        wildTitle.AddThemeColorOverride("font_color", new Color(0.9f, 0.8f, 0.5f));
        headerBox.AddChild(wildTitle);
        
        var spacer = new Control();
        spacer.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        headerBox.AddChild(spacer);
        
        _wildCountLabel = new Label();
        _wildCountLabel.Text = "0";
        headerBox.AddChild(_wildCountLabel);
        
        _refreshButton = new Button();
        _refreshButton.Text = "🔄 Refresh";
        _refreshButton.Pressed += OnRefreshPressed;
        headerBox.AddChild(_refreshButton);
        
        wildTab.AddChild(headerBox);
        
        // Wild monsters grid
        _wildScroll = new ScrollContainer();
        _wildScroll.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        _wildScroll.SetVExpandFlags(Control.ExpandFlags.ExpandFill);
        wildTab.AddChild(_wildScroll);
        
        _wildGrid = new GridContainer();
        _wildGrid.Columns = 4;
        _wildGrid.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        _wildGrid.AddThemeConstantOverride("h_separation", 10);
        _wildGrid.AddThemeConstantOverride("v_separation", 10);
        _wildScroll.AddChild(_wildGrid);
    }
    
    private void SetupTamedTab()
    {
        var tamedTab = new VBoxContainer();
        tamedTab.SetName("Tamed");
        _tabContainer.AddChild(tamedTab);
        
        var headerBox = new HBoxContainer();
        headerBox.AddThemeConstantOverride("separation", 10);
        
        var tamedTitle = new Label();
        tamedTitle.Text = "My Monsters";
        tamedTitle.AddThemeFontSizeOverride("font_size", 18);
        tamedTitle.AddThemeColorOverride("font_color", new Color(0.5f, 0.9f, 0.6f));
        headerBox.AddChild(tamedTitle);
        
        var spacer = new Control();
        spacer.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        headerBox.AddChild(spacer);
        
        _tamedCountLabel = new Label();
        _tamedCountLabel.Text = "0";
        headerBox.AddChild(_tamedCountLabel);
        
        tamedTab.AddChild(headerBox);
        
        _tamedScroll = new ScrollContainer();
        _tamedScroll.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        _tamedScroll.SetVExpandFlags(Control.ExpandFlags.ExpandFill);
        tamedTab.AddChild(_tamedScroll);
        
        _tamedGrid = new GridContainer();
        _tamedGrid.Columns = 4;
        _tamedGrid.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        _tamedGrid.AddThemeConstantOverride("h_separation", 10);
        _tamedGrid.AddThemeConstantOverride("v_separation", 10);
        _tamedScroll.AddChild(_tamedGrid);
    }
    
    private void SetupStatsTab()
    {
        var statsTab = new VBoxContainer();
        statsTab.SetName("Stats");
        _tabContainer.AddChild(statsTab);
        
        var statsTitle = new Label();
        statsTitle.Text = "Taming Statistics";
        statsTitle.AddThemeFontSizeOverride("font_size", 18);
        statsTitle.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
        statsTab.AddChild(statsTitle);
        
        _statsBox = new VBoxContainer();
        _statsBox.AddThemeConstantOverride("separation", 15);
        statsTab.AddChild(_statsBox);
        
        _totalAttemptsLabel = new Label();
        _totalAttemptsLabel.Text = "Total Attempts: 0";
        _statsBox.AddChild(_totalAttemptsLabel);
        
        _successRateLabel = new Label();
        _successRateLabel.Text = "Success Rate: 0%";
        _statsBox.AddChild(_successRateLabel);
        
        _legendaryLabel = new Label();
        _legendaryLabel.Text = "Legendary Tames: 0";
        _statsBox.AddChild(_legendaryLabel);
    }
    
    private void SetupInfoPanel()
    {
        _infoPanel = new PanelContainer();
        _infoPanel.SetHExpandFlags(Control.ExpandFlags.ExpandFill);
        _infoPanel.CustomMinimumSize = new Vector2(0, 200);
        _contentBox.AddChild(_infoPanel);
        
        var infoStyle = new StyleBoxFlat();
        infoStyle.BgColor = new Color(0.08f, 0.08f, 0.12f);
        infoStyle.BorderWidthTop = 1;
        infoStyle.BorderColor = new Color(0.25f, 0.25f, 0.35f);
        _infoPanel.AddThemeStyleboxOverride("panel", infoStyle);
        
        var infoBox = new VBoxContainer();
        infoBox.AddThemeConstantOverride("separation", 8);
        _infoPanel.AddChild(infoBox);
        
        // Monster name and rarity
        var nameBox = new HBoxContainer();
        nameBox.AddThemeConstantOverride("separation", 15);
        
        _monsterNameLabel = new Label();
        _monsterNameLabel.Text = "Select a monster";
        _monsterNameLabel.AddThemeFontSizeOverride("font_size", 20);
        _monsterNameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.95f, 0.8f));
        nameBox.AddChild(_monsterNameLabel);
        
        _monsterRarityLabel = new Label();
        _monsterRarityLabel.Text = "";
        _monsterRarityLabel.AddThemeFontSizeOverride("font_size", 16);
        nameBox.AddChild(_monsterRarityLabel);
        
        infoBox.AddChild(nameBox);
        
        // Stats
        _monsterStatsLabel = new Label();
        _monsterStatsLabel.Text = "HP: - | ATK: - | DEF: - | SPD: -";
        _monsterStatsLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
        infoBox.AddChild(_monsterStatsLabel);
        
        // Progress
        _monsterProgressLabel = new Label();
        _monsterProgressLabel.Text = "Tame Progress: 0%";
        infoBox.AddChild(_monsterProgressLabel);
        
        // Method buttons
        var methodBox = new HBoxContainer();
        methodBox.AddThemeConstantOverride("separation", 10);
        
        _methodLabel = new Label();
        _methodLabel.Text = "Taming Method:";
        _methodLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        methodBox.AddChild(_methodLabel);
        
        _feedButton = new Button();
        _feedButton.Text = "🍖 Feed";
        _feedButton.Pressed += () => OnTameMethodPressed(MonsterTamingSystem.TamingMethod.Feed);
        methodBox.AddChild(_feedButton);
        
        _battleButton = new Button();
        _battleButton.Text = "⚔️ Battle";
        _battleButton.Pressed += () => OnTameMethodPressed(MonsterTamingSystem.TamingMethod.Battle);
        methodBox.AddChild(_battleButton);
        
        _playButton = new Button();
        _playButton.Text = "🎾 Play";
        _playButton.Pressed += () => OnTameMethodPressed(MonsterTamingSystem.TamingMethod.Play);
        methodBox.AddChild(_playButton);
        
        _captureButton = new Button();
        _captureButton.Text = "🕸️ Capture (500g)";
        _captureButton.Pressed += () => OnTameMethodPressed(MonsterTamingSystem.TamingMethod.Capture);
        methodBox.AddChild(_captureButton);
        
        infoBox.AddChild(methodBox);
    }
    
    private void SetupInput()
    {
        // Input will be handled by Main.cs
    }
    
    public void ToggleUI()
    {
        if (_isVisible)
        {
            Hide();
        }
        else
        {
            Show();
            RefreshDisplay();
        }
        _isVisible = !_isVisible;
    }
    
    public void RefreshDisplay()
    {
        if (MonsterTamingSystem.Instance == null) return;
        
        // Update counts
        var wildMonsters = MonsterTamingSystem.Instance.GetWildMonsters();
        var tamedMonsters = MonsterTamingSystem.Instance.GetTamedMonsters();
        
        _wildCountLabel.Text = $"({wildMonsters.Count})";
        _tamedCountLabel.Text = $"({tamedMonsters.Count})";
        
        // Update wild grid
        UpdateMonsterGrid(_wildGrid, wildMonsters, true);
        
        // Update tamed grid
        UpdateMonsterGrid(_tamedGrid, tamedMonsters, false);
        
        // Update stats
        var stats = MonsterTamingSystem.Instance.GetTamingStats();
        _totalAttemptsLabel.Text = $"Total Attempts: {stats["total_attempts"]}";
        float rate = (float)stats["success_rate"] * 100f;
        _successRateLabel.Text = $"Success Rate: {rate:F1}%";
        _legendaryLabel.Text = $"Legendary Tames: {stats["legendary_tames"]}";
        
        // Update info panel
        if (_selectedMonster != null)
        {
            UpdateInfoPanel(_selectedMonster);
        }
    }
    
    private void UpdateMonsterGrid(GridContainer grid, Array<TameableMonster> monsters, bool isWild)
    {
        // Clear existing
        foreach (var child in grid.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add monster cards
        foreach (var monster in monsters)
        {
            var card = CreateMonsterCard(monster, isWild);
            grid.AddChild(card);
        }
        
        // Empty state
        if (monsters.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = isWild ? "No wild monsters found" : "No tamed monsters";
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
            grid.AddChild(emptyLabel);
        }
    }
    
    private Control CreateMonsterCard(TameableMonster monster, bool isWild)
    {
        var card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(180, 120);
        
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = GetRarityColor(monster.Rarity) * 0.3f;
        cardStyle.BorderWidthLeft = 2;
        cardStyle.BorderWidthTop = 2;
        cardStyle.BorderWidthRight = 2;
        cardStyle.BorderWidthBottom = 2;
        cardStyle.BorderColor = GetRarityColor(monster.Rarity);
        cardStyle.CornerRadiusTopLeft = 6;
        cardStyle.CornerRadiusTopRight = 6;
        cardStyle.CornerRadiusBottomLeft = 6;
        cardStyle.CornerRadiusBottomRight = 6;
        card.AddThemeStyleboxOverride("panel", cardStyle);
        
        var cardBox = new VBoxContainer();
        cardBox.AddThemeConstantOverride("separation", 5);
        card.AddChild(cardBox);
        
        // Name
        var nameLabel = new Label();
        nameLabel.Text = monster.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        nameLabel.AddThemeColorOverride("font_color", GetRarityColor(monster.Rarity));
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        cardBox.AddChild(nameLabel);
        
        // Level
        var levelLabel = new Label();
        levelLabel.Text = $"Lv.{monster.Level}";
        levelLabel.AddThemeFontSizeOverride("font_size", 12);
        levelLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.8f));
        levelLabel.HorizontalAlignment = HorizontalAlignment.Center;
        cardBox.AddChild(levelLabel);
        
        // Stats preview
        var statsLabel = new Label();
        statsLabel.Text = $"HP:{monster.Health:F0} ATK:{monster.Attack:F0}";
        statsLabel.AddThemeFontSizeOverride("font_size", 11);
        statsLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
        statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        cardBox.AddChild(statsLabel);
        
        // Progress (for wild monsters)
        if (isWild && monster.TameProgress > 0)
        {
            var progressLabel = new Label();
            progressLabel.Text = $"Progress: {monster.TameProgress * 100:F0}%";
            progressLabel.AddThemeFontSizeOverride("font_size", 11);
            progressLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.6f, 0.3f));
            progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
            cardBox.AddChild(progressLabel);
        }
        
        // Select button
        var selectButton = new Button();
        selectButton.Text = isWild ? "Select" : "View";
        selectButton.CustomMinimumSize = new Vector2(80, 25);
        selectButton.Pressed += () => OnMonsterSelected(monster);
        
        var buttonContainer = new HBoxContainer();
        buttonContainer.HorizontalAlignment = HorizontalAlignment.Center;
        buttonContainer.AddChild(selectButton);
        cardBox.AddChild(buttonContainer);
        
        return card;
    }
    
    private Color GetRarityColor(MonsterTamingSystem.MonsterRarity rarity)
    {
        switch (rarity)
        {
            case MonsterTamingSystem.MonsterRarity.Common: return new Color(0.7f, 0.7f, 0.7f);
            case MonsterTamingSystem.MonsterRarity.Uncommon: return new Color(0.3f, 0.8f, 0.3f);
            case MonsterTamingSystem.MonsterRarity.Rare: return new Color(0.3f, 0.5f, 0.9f);
            case MonsterTamingSystem.MonsterRarity.Epic: return new Color(0.6f, 0.3f, 0.8f);
            case MonsterTamingSystem.MonsterRarity.Legendary: return new Color(1f, 0.6f, 0.1f);
            default: return new Color(0.7f, 0.7f, 0.7f);
        }
    }
    
    private void UpdateInfoPanel(TameableMonster monster)
    {
        _monsterNameLabel.Text = monster.Name;
        
        // Rarity
        string rarityText = monster.Rarity.ToString();
        _monsterRarityLabel.Text = $"[{rarityText}]";
        _monsterRarityLabel.AddThemeColorOverride("font_color", GetRarityColor(monster.Rarity));
        
        // Stats
        _monsterStatsLabel.Text = $"HP: {monster.Health:F0} | ATK: {monster.Attack:F0} | DEF: {monster.Defense:F0} | SPD: {monster.Speed:F0}";
        
        // Progress
        if (monster.IsTamed)
        {
            _monsterProgressLabel.Text = "✅ Tamed!";
            _monsterProgressLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 0.5f));
            
            // Disable buttons
            _feedButton.Disabled = true;
            _battleButton.Disabled = true;
            _playButton.Disabled = true;
            _captureButton.Disabled = true;
        }
        else
        {
            _monsterProgressLabel.Text = $"Tame Progress: {monster.TameProgress * 100:F0}% | Attempts: {monster.TameAttempts}";
            _monsterProgressLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.7f, 0.4f));
            
            // Enable buttons
            _feedButton.Disabled = false;
            _battleButton.Disabled = false;
            _playButton.Disabled = false;
            _captureButton.Disabled = false;
        }
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.T || keyEvent.Keycode == Key.Escape)
            {
                ToggleUI();
                GetTree().SetInputAsHandled();
            }
        }
    }
}
