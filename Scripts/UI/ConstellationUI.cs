using Godot;
using System;
using System.Collections.Generic;

public class ConstellationUI : Control
{
    private ConstellationSystem _system;
    private ConstellationDatabase _database;
    
    // UI Components
    private TabContainer _tabContainer;
    private VBoxContainer _constellationList;
    private VBoxContainer _activeList;
    private VBoxContainer _statisticsPanel;
    
    // Theme colors
    private Color _commonColor = new Color(0.7f, 0.7f, 0.7f);
    private Color _uncommonColor = new Color(0.2f, 0.8f, 0.2f);
    private Color _rareColor = new Color(0.2f, 0.5f, 1.0f);
    private Color _epicColor = new Color(0.6f, 0.2f, 0.8f);
    private Color _legendaryColor = new Color(1.0f, 0.6f, 0.0f);
    
    // Element colors
    private Color _fireColor = new Color(1.0f, 0.4f, 0.2f);
    private Color _waterColor = new Color(0.2f, 0.6f, 1.0f);
    private Color _earthColor = new Color(0.5f, 0.35f, 0.2f);
    private Color _airColor = new Color(0.6f, 0.9f, 1.0f);
    private Color _lightColor = new Color(1.0f, 0.95f, 0.6f);
    private Color _darkColor = new Color(0.3f, 0.2f, 0.4f);
    
    public override void _Ready()
    {
        _system = GetNode<ConstellationSystem>("/root/ConstellationSystem");
        if (_system == null)
        {
            GD.Print("[ConstellationUI] ConstellationSystem not found!");
            return;
        }
        
        _database = _system.GetDatabase();
        
        SetupUI();
        
        // Handle keyboard input
        SetProcessInput(true);
    }
    
    private void SetupUI()
    {
        // Main panel
        var mainPanel = new PanelContainer();
        mainPanel.SetAnchorsPreset(Control.Preset.Center);
        mainPanel.CustomMinimumSize = new Vector2(900, 600);
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "✦ Constellation System ✦";
        titleLabel.Align = Label.AlignEnum.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetHExpand(ExpandMode.IgnoreSize);
        _tabContainer.SetVExpand(ExpandMode.IgnoreSize);
        _tabContainer.CustomMinimumSize = new Vector2(880, 550);
        mainVBox.AddChild(_tabContainer);
        
        // Tab 1: All Constellations
        var allTab = new ScrollContainer();
        allTab.Name = "All Constellations";
        _tabContainer.AddChild(allTab);
        
        _constellationList = new VBoxContainer();
        _constellationList.SetHExpand(ExpandMode.IgnoreSize);
        allTab.AddChild(_constellationList);
        
        PopulateConstellationList();
        
        // Tab 2: Active
        var activeTab = new ScrollContainer();
        activeTab.Name = "Active";
        _tabContainer.AddChild(activeTab);
        
        _activeList = new VBoxContainer();
        _activeList.SetHExpand(ExpandMode.IgnoreSize);
        activeTab.AddChild(_activeList);
        
        PopulateActiveList();
        
        // Tab 3: Statistics
        var statsTab = new ScrollContainer();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);
        
        _statisticsPanel = new VBoxContainer();
        _statisticsPanel.SetHExpand(ExpandMode.IgnoreSize);
        statsTab.AddChild(_statisticsPanel);
        
        PopulateStatistics();
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Align = Button.AlignEnum.Center;
        closeButton.Pressed += () => QueueFree();
        mainVBox.AddChild(closeButton);
    }
    
    private void PopulateConstellationList()
    {
        var constellations = _database.GetAllConstellations();
        
        foreach (var kvp in constellations)
        {
            var constellation = kvp.Value;
            var card = CreateConstellationCard(constellation);
            _constellationList.AddChild(card);
        }
    }
    
    private Control CreateConstellationCard(ConstellationSystem.Constellation constellation)
    {
        var cardContainer = new HBoxContainer();
        cardContainer.SetHExpand(ExpandMode.IgnoreSize);
        cardContainer.CustomMinimumSize = new Vector2(850, 80);
        
        // Left: Constellation visual
        var visualPanel = new PanelContainer();
        visualPanel.CustomMinimumSize = new Vector2(80, 80);
        cardContainer.AddChild(visualPanel);
        
        var visualLabel = new Label();
        visualLabel.Text = GetConstellationSymbol(constellation.Type);
        visualLabel.AddThemeFontSizeOverride("font_size", 40);
        visualLabel.Align = Label.AlignEnum.Center;
        visualLabel.Valign = Label.VAlign.Center;
        visualPanel.AddChild(visualLabel);
        
        // Apply element color
        var elementColor = GetElementColor(constellation.Type);
        visualPanel.AddThemeStyleboxOverride("panel", CreateFlatStyle(elementColor));
        
        // Middle: Info
        var infoVBox = new VBoxContainer();
        infoVBox.SetVExpand(ExpandMode.IgnoreSize);
        cardContainer.AddChild(infoVBox);
        
        var nameLabel = new Label();
        nameLabel.Text = $"{constellation.Name} ({constellation.Stars} Stars)";
        nameLabel.AddThemeFontSizeOverride("font_size", 18);
        infoVBox.AddChild(nameLabel);
        
        var descLabel = new Label();
        descLabel.Text = constellation.Description;
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        infoVBox.AddChild(descLabel);
        
        var statsLabel = new Label();
        statsLabel.Text = $"ATK: {constellation.AttackBonus:P0} | DEF: {constellation.DefenseBonus:P0} | HP: {constellation.HealthBonus:P0} | SPD: {constellation.SpeedBonus:P0}";
        statsLabel.AddThemeFontSizeOverride("font_size", 12);
        infoVBox.AddChild(statsLabel);
        
        var bonusLabel = new Label();
        bonusLabel.Text = $"CRIT: {constellation.CriticalBonus:P0} | GOLD: {constellation.GoldBonus:P0} | EXP: {constellation.ExpBonus:P0}";
        bonusLabel.AddThemeFontSizeOverride("font_size", 12);
        infoVBox.AddChild(bonusLabel);
        
        // Right: Action button
        var actionPanel = new VBoxContainer();
        actionPanel.Alignment = BoxContainer.AlignmentMode.Center;
        cardContainer.AddChild(actionPanel);
        
        var isUnlocked = _system.IsConstellationUnlocked(constellation.Id);
        
        if (!isUnlocked)
        {
            var unlockButton = new Button();
            unlockButton.Text = $"Unlock\n{constellation.UnlockCost}g";
            unlockButton.Pressed += () => TryUnlockConstellation(constellation.Id, constellation.UnlockCost);
            actionPanel.AddChild(unlockButton);
            
            var levelLabel = new Label();
            levelLabel.Text = $"Lvl {constellation.RequiredLevel}+";
            levelLabel.Align = Label.AlignEnum.Center;
            actionPanel.AddChild(levelLabel);
        }
        else
        {
            var progress = _system.GetConstellationProgress(constellation.Id);
            if (progress != null && progress.ActivatedStars < constellation.Stars)
            {
                var activateButton = new Button();
                activateButton.Text = $"Activate\nStar";
                activateButton.Pressed += () => TryActivateStars(constellation.Id);
                actionPanel.AddChild(activateButton);
            }
            
            var progressLabel = new Label();
            progressLabel.Text = $"Stars: {progress?.ActivatedStars ?? 0}/{constellation.Stars}";
            progressLabel.Align = Label.AlignEnum.Center;
            actionPanel.AddChild(progressLabel);
        }
        
        // Apply rarity color to background
        var bgColor = GetRarityColor(constellation.Rarity);
        cardContainer.AddThemeStyleboxOverride("panel", CreateFlatStyle(bgColor * new Color(1, 1, 1, 0.3f)));
        
        return cardContainer;
    }
    
    private void PopulateActiveList()
    {
        var unlocked = _system.GetUnlockedConstellations();
        
        if (unlocked.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No constellations unlocked yet.";
            emptyLabel.Align = Label.AlignEnum.Center;
            _activeList.AddChild(emptyLabel);
            return;
        }
        
        foreach (var kvp in unlocked)
        {
            var constellation = _database.GetConstellation(kvp.Key);
            if (constellation == null) continue;
            
            var progress = kvp.Value;
            if (!progress.Unlocked) continue;
            
            var card = CreateActiveConstellationCard(constellation, progress);
            _activeList.AddChild(card);
        }
    }
    
    private Control CreateActiveConstellationCard(ConstellationSystem.Constellation constellation, ConstellationSystem.ConstellationProgress progress)
    {
        var cardContainer = new VBoxContainer();
        cardContainer.SetHExpand(ExpandMode.IgnoreSize);
        cardContainer.CustomMinimumSize = new Vector2(850, 100);
        
        var nameLabel = new Label();
        nameLabel.Text = $"✦ {constellation.Name} - {progress.ActivatedStars}/{progress.TotalStars} Stars";
        nameLabel.AddThemeFontSizeOverride("font_size", 20);
        nameLabel.Align = Label.AlignEnum.Center;
        cardContainer.AddChild(nameLabel);
        
        // Progress bar
        var progressBar = new ProgressBar();
        progressBar.CustomMinimumSize = new Vector2(800, 20);
        progressBar.MaxValue = progress.TotalStars;
        progressBar.Value = progress.ActivatedStars;
        progressBar.ShowPercentage = false;
        cardContainer.AddChild(progressBar);
        
        // Stats
        var statsLabel = new Label();
        float activationRatio = (float)progress.ActivatedStars / progress.TotalStars;
        statsLabel.Text = $"Active Bonuses: ATK +{constellation.AttackBonus * activationRatio:P0} | DEF +{constellation.DefenseBonus * activationRatio:P0} | HP +{constellation.HealthBonus * activationRatio:P0}";
        statsLabel.AddThemeFontSizeOverride("font_size", 14);
        statsLabel.Align = Label.AlignEnum.Center;
        cardContainer.AddChild(statsLabel);
        
        var elementColor = GetElementColor(constellation.Type);
        cardContainer.AddThemeStyleboxOverride("panel", CreateFlatStyle(elementColor * new Color(1, 1, 1, 0.2f)));
        
        return cardContainer;
    }
    
    private void PopulateStatistics()
    {
        var stats = _system.GetStatistics();
        
        var statsLabel = new Label();
        statsLabel.Text = $"Total Constellations Unlocked: {stats["total_unlocked"]}\n" +
                         $"Total Stars Activated: {stats["total_stars_activated"]}\n" +
                         $"Gold Spent: {stats["gold_spent"]}\n" +
                         $"Fragments Collected: {stats["fragments_collected"]}\n" +
                         $"Current Fragments: {stats["current_fragments"]}";
        statsLabel.AddThemeFontSizeOverride("font_size", 18);
        statsLabel.Align = Label.AlignEnum.Center;
        _statisticsPanel.AddChild(statsLabel);
        
        // Total bonuses
        var bonuses = _system.GetTotalBonuses();
        var bonusesLabel = new Label();
        bonusesLabel.Text = $"\n[Current Active Bonuses]\n" +
                           $"Attack: +{bonuses["attack"]:P1}\n" +
                           $"Defense: +{bonuses["defense"]:P1}\n" +
                           $"Health: +{bonuses["health"]:P1}\n" +
                           $"Speed: +{bonuses["speed"]:P1}\n" +
                           $"Critical: +{bonuses["critical"]:P1}\n" +
                           $"Evasion: +{bonuses["evasion"]:P1}\n" +
                           $"Gold Bonus: +{bonuses["gold"]:P1}\n" +
                           $"EXP Bonus: +{bonuses["exp"]:P1}";
        bonusesLabel.AddThemeFontSizeOverride("font_size", 16);
        bonusesLabel.Align = Label.AlignEnum.Center;
        _statisticsPanel.AddChild(bonusesLabel);
    }
    
    private void TryUnlockConstellation(string constellationId, int cost)
    {
        // Get player gold from player node
        var player = GetNode("/root/Player");
        if (player != null)
        {
            // Try to unlock
            if (_system.UnlockConstellation(constellationId, 10000, 50)) // Simplified for demo
            {
                RefreshUI();
            }
        }
    }
    
    private void TryActivateStars(string constellationId)
    {
        if (_system.ActivateStars(constellationId, 1, 1000))
        {
            RefreshUI();
        }
    }
    
    private void RefreshUI()
    {
        // Clear and repopulate
        foreach (var child in _constellationList.GetChildren())
            child.QueueFree();
        foreach (var child in _activeList.GetChildren())
            child.QueueFree();
        foreach (var child in _statisticsPanel.GetChildren())
            child.QueueFree();
        
        PopulateConstellationList();
        PopulateActiveList();
        PopulateStatistics();
    }
    
    private string GetConstellationSymbol(ConstellationSystem.ConstellationType type)
    {
        switch (type)
        {
            case ConstellationSystem.ConstellationType.Fire: return "🔥";
            case ConstellationSystem.ConstellationType.Water: return "💧";
            case ConstellationSystem.ConstellationType.Earth: return "🌍";
            case ConstellationSystem.ConstellationType.Air: return "💨";
            case ConstellationSystem.ConstellationType.Light: return "✨";
            case ConstellationSystem.ConstellationType.Dark: return "🌑";
            default: return "⭐";
        }
    }
    
    private Color GetElementColor(ConstellationSystem.ConstellationType type)
    {
        switch (type)
        {
            case ConstellationSystem.ConstellationType.Fire: return _fireColor;
            case ConstellationSystem.ConstellationType.Water: return _waterColor;
            case ConstellationSystem.ConstellationType.Earth: return _earthColor;
            case ConstellationSystem.ConstellationType.Air: return _airColor;
            case ConstellationSystem.ConstellationType.Light: return _lightColor;
            case ConstellationSystem.ConstellationType.Dark: return _darkColor;
            default: return Colors.White;
        }
    }
    
    private Color GetRarityColor(ConstellationSystem.ConstellationRarity rarity)
    {
        switch (rarity)
        {
            case ConstellationSystem.ConstellationRarity.Common: return _commonColor;
            case ConstellationSystem.ConstellationRarity.Uncommon: return _uncommonColor;
            case ConstellationSystem.ConstellationRarity.Rare: return _rareColor;
            case ConstellationSystem.ConstellationRarity.Epic: return _epicColor;
            case ConstellationSystem.ConstellationRarity.Legendary: return _legendaryColor;
            default: return Colors.Gray;
        }
    }
    
    private StyleBoxFlat CreateFlatStyle(Color color)
    {
        var style = new StyleBoxFlat();
        style.BgColor = color;
        style.CornerRadiusTopLeft = 8;
        style.CornerRadiusTopRight = 8;
        style.CornerRadiusBottomLeft = 8;
        style.CornerRadiusBottomRight = 8;
        return style;
    }
    
    public override void _Input(InputEvent event_)
    {
        if (event_ is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                QueueFree();
            }
            else if (keyEvent.Keycode == Key.K)
            {
                // Toggle - handled in Main
            }
        }
    }
}
