using Godot;
using System;
using System.Collections.Generic;

public partial class AchievementUI : Control
{
    private VBoxContainer _mainContainer;
    private HBoxContainer _headerContainer;
    private TabContainer _tabContainer;
    private Label _titleLabel;
    private Label _progressLabel;

    // Achievement containers by category
    private ScrollContainer _combatContainer;
    private ScrollContainer _explorationContainer;
    private ScrollContainer _collectionContainer;
    private ScrollContainer _socialContainer;
    private ScrollContainer _economyContainer;
    private ScrollContainer _progressionContainer;
    private ScrollContainer _specialContainer;

    // Stats panel
    private VBoxContainer _statsContainer;
    private Label _totalKillsLabel;
    private Label _bossKillsLabel;
    private Label _pvpWinsLabel;
    private Label _goldLabel;
    private Label _playTimeLabel;

    private bool _isVisible = false;
    private AchievementData.AchievementCategory _currentCategory = AchievementData.AchievementCategory.Combat;

    public override void _Ready()
    {
        SetupUI();
        Visible = false;
    }

    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainContainer.CustomMinimumSize = new Vector2(900, 600);
        AddChild(_mainContainer);

        // Background panel
        var bgPanel = new Panel();
        bgPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        bgPanel.Modulate = new Color(0, 0, 0, 0.85f);
        _mainContainer.AddChild(bgPanel);

        // Header
        _headerContainer = new HBoxContainer();
        _headerContainer.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        _headerContainer.MarginTop = 10;
        _headerContainer.MarginLeft = 10;
        _headerContainer.MarginRight = 10;
        _mainContainer.AddChild(_headerContainer);

        _titleLabel = new Label();
        _titleLabel.Text = "🏆 Achievements";
        _titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.84f, 0));
        _titleLabel.RectMinSize = new Vector2(0, 40);
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Left;
        _titleLabel.AddFontOverride("font_size", 24);
        _headerContainer.AddChild(_titleLabel);

        _progressLabel = new Label();
        _progressLabel.Text = "0 / 0 Unlocked";
        _progressLabel.AddThemeColorOverride("font_color", Colors.White);
        _progressLabel.HorizontalAlignment = HorizontalAlignment.Right;
        _headerContainer.AddChild(_progressLabel);

        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _tabContainer.MarginTop = 60;
        _tabContainer.MarginLeft = 10;
        _tabContainer.MarginRight = 10;
        _tabContainer.MarginBottom = 60;
        _mainContainer.AddChild(_tabContainer);

        // Create tabs
        CreateTab("Combat", AchievementData.AchievementCategory.Combat);
        CreateTab("Exploration", AchievementData.AchievementCategory.Exploration);
        CreateTab("Collection", AchievementData.AchievementCategory.Collection);
        CreateTab("Social", AchievementData.AchievementCategory.Social);
        CreateTab("Economy", AchievementData.AchievementCategory.Economy);
        CreateTab("Progression", AchievementData.AchievementCategory.Progression);
        CreateTab("Special", AchievementData.AchievementCategory.Special);

        // Stats panel at bottom
        var statsPanel = new HBoxContainer();
        statsPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        statsPanel.MarginBottom = 10;
        statsPanel.MarginLeft = 10;
        statsPanel.MarginRight = 10;
        statsPanel.Alignment = BoxContainer.AlignMode.Center;
        _mainContainer.AddChild(statsPanel);

        _totalKillsLabel = CreateStatLabel("Kills: 0");
        statsPanel.AddChild(_totalKillsLabel);

        var sep1 = new Label();
        sep1.Text = "  |  ";
        sep1.AddThemeColorOverride("font_color", Colors.Gray);
        statsPanel.AddChild(sep1);

        _bossKillsLabel = CreateStatLabel("Bosses: 0");
        statsPanel.AddChild(_bossKillsLabel);

        var sep2 = new Label();
        sep2.Text = "  |  ";
        sep2.AddThemeColorOverride("font_color", Colors.Gray);
        statsPanel.AddChild(sep2);

        _pvpWinsLabel = CreateStatLabel("PvP Wins: 0");
        statsPanel.AddChild(_pvpWinsLabel);

        var sep3 = new Label();
        sep3.Text = "  |  ";
        sep3.AddThemeColorOverride("font_color", Colors.Gray);
        statsPanel.AddChild(sep3);

        _goldLabel = CreateStatLabel("Gold: 0");
        statsPanel.AddChild(_goldLabel);

        var sep4 = new Label();
        sep4.Text = "  |  ";
        sep4.AddThemeColorOverride("font_color", Colors.Gray);
        statsPanel.AddChild(sep4);

        _playTimeLabel = CreateStatLabel("Playtime: 0h");
        statsPanel.AddChild(_playTimeLabel);

        // Bottom hint
        var hintLabel = new Label();
        hintLabel.Text = "Press H to toggle  |  ESC to close";
        hintLabel.AddThemeColorOverride("font_color", Colors.Gray);
        hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        hintLabel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        hintLabel.MarginBottom = 10;
        _mainContainer.AddChild(hintLabel);
    }

    private void CreateTab(string tabName, AchievementData.AchievementCategory category)
    {
        var scroll = new ScrollContainer();
        scroll.Name = tabName;
        _tabContainer.AddChild(scroll);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.MarginLeft = 5;
        vbox.MarginTop = 5;
        vbox.MarginRight = 5;
        vbox.MarginBottom = 5;
        scroll.AddChild(vbox);

        // Store reference
        switch (category)
        {
            case AchievementData.AchievementCategory.Combat:
                _combatContainer = scroll;
                break;
            case AchievementData.AchievementCategory.Exploration:
                _explorationContainer = scroll;
                break;
            case AchievementData.AchievementCategory.Collection:
                _collectionContainer = scroll;
                break;
            case AchievementData.AchievementCategory.Social:
                _socialContainer = scroll;
                break;
            case AchievementData.AchievementCategory.Economy:
                _economyContainer = scroll;
                break;
            case AchievementData.AchievementCategory.Progression:
                _progressionContainer = scroll;
                break;
            case AchievementData.AchievementCategory.Special:
                _specialContainer = scroll;
                break;
        }

        RefreshCategory(category);
    }

    private Label CreateStatLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        label.AddThemeColorOverride("font_color", Colors.White);
        label.HorizontalAlignment = HorizontalAlignment.Center;
        return label;
    }

    public void RefreshCategory(AchievementData.AchievementCategory category)
    {
        ScrollContainer targetScroll = null;
        switch (category)
        {
            case AchievementData.AchievementCategory.Combat: targetScroll = _combatContainer; break;
            case AchievementData.AchievementCategory.Exploration: targetScroll = _explorationContainer; break;
            case AchievementData.AchievementCategory.Collection: targetScroll = _collectionContainer; break;
            case AchievementData.AchievementCategory.Social: targetScroll = _socialContainer; break;
            case AchievementData.AchievementCategory.Economy: targetScroll = _economyContainer; break;
            case AchievementData.AchievementCategory.Progression: targetScroll = _progressionContainer; break;
            case AchievementData.AchievementCategory.Special: targetScroll = _specialContainer; break;
        }

        if (targetScroll == null) return;

        // Clear existing
        foreach (Node child in targetScroll.GetChildren())
        {
            child.QueueFree();
        }

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.MarginLeft = 5;
        vbox.MarginTop = 5;
        vbox.MarginRight = 5;
        vbox.MarginBottom = 5;
        targetScroll.AddChild(vbox);

        var achievementSystem = AchievementSystem.Instance;
        if (achievementSystem == null) return;

        var achievements = achievementSystem.GetAchievementsByCategory(category);
        foreach (var achievement in achievements)
        {
            var card = CreateAchievementCard(achievement);
            vbox.AddChild(card);
        }

        UpdateProgressLabel();
    }

    private Control CreateAchievementCard(AchievementData.Achievement achievement)
    {
        var card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(0, 80);
        card.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        card.MarginLeft = 5;
        card.MarginRight = 5;
        card.MarginBottom = 5;

        var hbox = new HBoxContainer();
        hbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        hbox.MarginLeft = 10;
        hbox.MarginTop = 8;
        hbox.MarginRight = 10;
        hbox.MarginBottom = 8;
        card.AddChild(hbox);

        // Icon
        var iconLabel = new Label();
        iconLabel.Text = achievement.isUnlocked ? "🏆" : "🔒";
        iconLabel.AddFontOverride("font_size", 28);
        hbox.AddChild(iconLabel);

        // Info
        var infoVbox = new VBoxContainer();
        infoVbox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(infoVbox);

        var nameLabel = new Label();
        nameLabel.Text = achievement.name;
        nameLabel.AddThemeColorOverride("font_color", GetRarityColor(achievement.rarity));
        nameLabel.AddFontOverride("font_size", 16);
        infoVbox.AddChild(nameLabel);

        var descLabel = new Label();
        descLabel.Text = achievement.description;
        descLabel.AddThemeColorOverride("font_color", achievement.isUnlocked ? Colors.LightGreen : Colors.Gray);
        descLabel.Autowrap = true;
        infoVbox.AddChild(descLabel);

        // Progress
        var progressVbox = new VBoxContainer();
        progressVbox.Alignment = BoxContainer.AlignMode.End;
        progressVbox.CustomMinimumSize = new Vector2(150, 0);
        hbox.AddChild(progressVbox);

        var progressText = new Label();
        progressText.HorizontalAlignment = HorizontalAlignment.Right;
        if (achievement.isUnlocked)
        {
            progressText.Text = "✓ COMPLETED";
            progressText.AddThemeColorOverride("font_color", Colors.Green);
        }
        else
        {
            progressText.Text = $"{achievement.currentProgress} / {achievement.requirement}";
            progressText.AddThemeColorOverride("font_color", Colors.White);
        }
        progressVbox.AddChild(progressText);

        // Progress bar
        var progressBar = new ProgressBar();
        progressBar.CustomMinimumSize = new Vector2(150, 10);
        progressBar.Value = (float)achievement.currentProgress / achievement.requirement * 100;
        progressBar.ShowPercentage = false;
        progressBar.Modulate = achievement.isUnlocked ? Colors.Green : new Color(0.3f, 0.3f, 0.3f);
        progressVbox.AddChild(progressBar);

        // Reward
        var rewardLabel = new Label();
        rewardLabel.Text = $"💰 {achievement.rewardGold} | ✨ {achievement.rewardExp}";
        rewardLabel.AddThemeColorOverride("font_color", Colors.Yellow);
        rewardLabel.HorizontalAlignment = HorizontalAlignment.Right;
        progressVbox.AddChild(rewardLabel);

        return card;
    }

    private Color GetRarityColor(AchievementData.AchievementRarity rarity)
    {
        switch (rarity)
        {
            case AchievementData.AchievementRarity.Common: return Colors.Gray;
            case AchievementData.AchievementRarity.Uncommon: return Colors.Green;
            case AchievementData.AchievementRarity.Rare: return Colors.Blue;
            case AchievementData.AchievementRarity.Epic: return new Color(0.6f, 0.2f, 0.8f);
            case AchievementData.AchievementRarity.Legendary: return new Color(1f, 0.6f, 0);
            default: return Colors.White;
        }
    }

    private void UpdateProgressLabel()
    {
        var achievementSystem = AchievementSystem.Instance;
        if (achievementSystem == null) return;

        int unlocked = achievementSystem.GetUnlockedCount();
        int total = achievementSystem.GetTotalAchievementCount();
        _progressLabel.Text = $"{unlocked} / {total} Unlocked";

        // Update stats
        _totalKillsLabel.Text = $"Kills: {achievementSystem.GetTotalKills()}";
        _bossKillsLabel.Text = $"Bosses: {achievementSystem.GetBossKills()}";
        _pvpWinsLabel.Text = $"PvP: {achievementSystem.GetPvpWins()}";
        _goldLabel.Text = $"Gold: {achievementSystem.GetGoldAccumulated():N0}";
        _playTimeLabel.Text = $"Playtime: {achievementSystem.GetPlayTimeHours():F1}h";
    }

    public void Refresh()
    {
        UpdateProgressLabel();
        RefreshCategory(_currentCategory);
    }

    public override void _Input(InputEvent ev)
    {
        if (ev.IsActionPressed("ui_cancel") || ev.IsActionPressed("ui_toggle_achievements"))
        {
            Toggle();
        }
    }

    public void Toggle()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;

        if (_isVisible)
        {
            Refresh();
            UpdateProgressLabel();
        }
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            // Cleanup if needed
        }
        base.Dispose(disposing);
    }
}
