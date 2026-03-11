using Godot;
using System;
using System.Collections.Generic;

public partial class SkillComboUI : Control
{
    private static SkillComboUI _instance;
    public static SkillComboUI Instance => _instance;
    
    [Export] public bool IsVisible { get; private set; } = false;
    
    // UI Elements
    private PanelContainer _mainPanel;
    private VBoxContainer _contentBox;
    private Label _titleLabel;
    private TabContainer _tabContainer;
    
    // Combo list tab
    private ScrollContainer _comboListScroll;
    private VBoxContainer _comboListContainer;
    
    // Stats tab
    private VBoxContainer _statsContainer;
    private Label _totalCombosLabel;
    private Label _discoveredLabel;
    private Label _bestStreakLabel;
    private Label _activeBonusLabel;
    
    // Active bonus display
    private PanelContainer _bonusDisplay;
    private Label _bonusLabel;
    private ProgressBar _bonusTimer;
    
    public override void _Ready()
    {
        _instance = this;
        SetupUI();
        Hide();
    }
    
    private void SetupUI()
    {
        // Main panel
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.CenterRight);
        _mainPanel.OffsetLeft = -450;
        _mainPanel.OffsetTop = 50;
        _mainPanel.OffsetRight = -20;
        _mainPanel.OffsetBottom = -50;
        _mainPanel.CustomMinimumSize = new Vector2(420, 500);
        AddChild(_mainPanel);
        
        // Style
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);
        _mainPanel.AddThemeStyleboxOverride("panel", style);
        
        // Content box
        _contentBox = new VBoxContainer();
        _contentBox.SetThemeConstant("separation", 10);
        _mainPanel.AddChild(_contentBox);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ Skill Combo System";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _contentBox.AddChild(_titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetTabAlign(TabContainer.TabAlignEnum.Top);
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _contentBox.AddChild(_tabContainer);
        
        // Combo list tab
        var comboListTab = new Control();
        comboListTab.Name = "Combos";
        _tabContainer.AddChild(comboListTab);
        
        _comboListScroll = new ScrollContainer();
        _comboListScroll.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _comboListScroll.VScroll = true;
        comboListTab.AddChild(_comboListScroll);
        
        _comboListContainer = new VBoxContainer();
        _comboListContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _comboListContainer.CustomMinimumSize = new Vector2(380, 350);
        _comboListScroll.AddChild(_comboListContainer);
        
        // Stats tab
        var statsTab = new Control();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);
        
        _statsContainer = new VBoxContainer();
        _statsContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _statsContainer.CustomMinimumSize = new Vector2(380, 350);
        _statsContainer.Position = new Vector2(20, 20);
        _statsContainer.Spacing = 15;
        statsTab.AddChild(_statsContainer);
        
        SetupStatsTab();
        
        // Bonus display panel (floating)
        SetupBonusDisplay();
        
        // Populate combo list
        PopulateComboList();
    }
    
    private void SetupStatsTab()
    {
        _totalCombosLabel = new Label();
        _totalCombosLabel.Text = "Total Combos: 0";
        _totalCombosLabel.AddThemeFontSizeOverride("font_size", 18);
        _statsContainer.AddChild(_totalCombosLabel);
        
        _discoveredLabel = new Label();
        _discoveredLabel.Text = "Discovered: 0 / 0";
        _discoveredLabel.AddThemeFontSizeOverride("font_size", 18);
        _statsContainer.AddChild(_discoveredLabel);
        
        _bestStreakLabel = new Label();
        _bestStreakLabel.Text = "Best Streak: 0";
        _bestStreakLabel.AddThemeFontSizeOverride("font_size", 18);
        _statsContainer.AddChild(_bestStreakLabel);
        
        _activeBonusLabel = new Label();
        _activeBonusLabel.Text = "Active Bonus: None";
        _activeBonusLabel.AddThemeFontSizeOverride("font_size", 18);
        _activeBonusLabel.Modulate = new Color(1f, 0.8f, 0.2f);
        _statsContainer.AddChild(_activeBonusLabel);
    }
    
    private void SetupBonusDisplay()
    {
        _bonusDisplay = new PanelContainer();
        _bonusDisplay.SetAnchorsPreset(Control.LayoutPreset.TopLeft);
        _bonusDisplay.Position = new Vector2(20, 80);
        _bonusDisplay.CustomMinimumSize = new Vector2(300, 50);
        _bonusDisplay.Visible = false;
        
        var bonusStyle = new StyleBoxFlat();
        bonusStyle.BgColor = new Color(0.2f, 0.5f, 0.2f, 0.9f);
        bonusStyle.BorderColor = new Color(0.4f, 0.8f, 0.4f);
        bonusStyle.SetBorderWidthAll(2);
        bonusStyle.SetCornerRadiusAll(5);
        _bonusDisplay.AddThemeStyleboxOverride("panel", bonusStyle);
        
        AddChild(_bonusDisplay);
        
        var bonusBox = new HBoxContainer();
        bonusBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        bonusBox.Alignment = BoxContainer.AlignmentMode.Center;
        _bonusDisplay.AddChild(bonusBox);
        
        _bonusLabel = new Label();
        _bonusLabel.Text = "Combo Active!";
        _bonusLabel.AddThemeFontSizeOverride("font_size", 16);
        bonusBox.AddChild(_bonusLabel);
        
        _bonusTimer = new ProgressBar();
        _bonusTimer.CustomMinimumSize = new Vector2(200, 10);
        _bonusTimer.ShowPercentage = false;
        bonusBox.AddChild(_bonusTimer);
    }
    
    private void PopulateComboList()
    {
        var combos = SkillComboDatabase.Instance.GetAllCombos();
        
        foreach (var combo in combos)
        {
            var comboPanel = CreateComboPanel(combo);
            _comboListContainer.AddChild(comboPanel);
        }
    }
    
    private Control CreateComboPanel(SkillCombo combo)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(360, 80);
        
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.15f, 0.2f);
        style.BorderColor = GetComboRarityColor(combo.Bonus.RequiredComboCount);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(5);
        panel.AddThemeStyleboxOverride("panel", style);
        
        var hbox = new HBoxContainer();
        hbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        hbox.MarginLeft = 10;
        hbox.MarginRight = 10;
        hbox.MarginTop = 5;
        hbox.MarginBottom = 5;
        panel.AddChild(hbox);
        
        // Left side - combo info
        var infoBox = new VBoxContainer();
        infoBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(infoBox);
        
        var nameLabel = new Label();
        nameLabel.Text = $"⚔️ {combo.Name}";
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        nameLabel.Modulate = GetComboRarityColor(combo.Bonus.RequiredComboCount);
        infoBox.AddChild(nameLabel);
        
        var descLabel = new Label();
        descLabel.Text = $"{combo.Type} | {combo.Trigger} | {combo.SkillIds.Count} skills";
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        descLabel.Modulate = new Color(0.7f, 0.7f, 0.8f);
        infoBox.AddChild(descLabel);
        
        var bonusLabel = new Label();
        bonusLabel.Text = $"Bonus: {combo.Bonus.Name} ({combo.Bonus.Description})";
        bonusLabel.AddThemeFontSizeOverride("font_size", 12);
        bonusLabel.Modulate = new Color(0.5f, 0.9f, 0.5f);
        infoBox.AddChild(bonusLabel);
        
        // Right side - usage count
        var countLabel = new Label();
        int usageCount = SkillComboSystem.Instance?.GetComboUsageCount(combo.ComboId) ?? 0;
        countLabel.Text = $"x{usageCount}";
        countLabel.AddThemeFontSizeOverride("font_size", 20);
        countLabel.Modulate = new Color(0.8f, 0.8f, 0.9f);
        hbox.AddChild(countLabel);
        
        return panel;
    }
    
    private Color GetComboRarityColor(int comboCount)
    {
        return comboCount switch
        {
            2 => new Color(0.7f, 0.7f, 0.7f),    // Common - gray
            3 => new Color(0.2f, 0.8f, 0.2f),    // Uncommon - green
            4 => new Color(0.2f, 0.5f, 0.9f),    // Rare - blue
            5 => new Color(0.7f, 0.5f, 0.9f),    // Epic - purple
            _ => new Color(1f, 0.7f, 0.2f)       // Legendary - gold
        };
    }
    
    public override void _Process(double delta)
    {
        if (SkillComboSystem.Instance == null) return;
        
        // Update stats
        var stats = SkillComboSystem.Instance.GetStatistics();
        _totalCombosLabel.Text = $"Total Combos: {stats.TotalCombosTriggered}";
        
        var allCombos = SkillComboDatabase.Instance.GetAllCombos();
        _discoveredLabel.Text = $"Discovered: {stats.DiscoveredCombos.Count} / {allCombos.Count}";
        
        int bestStreak = 0;
        foreach (var kvp in stats.ComboStreakBest)
        {
            bestStreak = Math.Max(bestStreak, kvp.Value);
        }
        _bestStreakLabel.Text = $"Best Streak: {bestStreak}";
        
        // Update active bonus display
        if (SkillComboSystem.Instance.HasActiveBonus())
        {
            _bonusDisplay.Visible = true;
            _bonusTimer.Value = 100f; // Would calculate based on remaining time
            _activeBonusLabel.Text = "Active Bonus: Active!";
            _activeBonusLabel.Modulate = new Color(0.2f, 1f, 0.2f);
        }
        else
        {
            _bonusDisplay.Visible = false;
            _activeBonusLabel.Text = "Active Bonus: None";
            _activeBonusLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
        }
    }
    
    public void Toggle()
    {
        if (IsVisible)
            Hide();
        else
            Show();
    }
    
    public new void Show()
    {
        IsVisible = true;
        Visible = true;
        _mainPanel.Visible = true;
    }
    
    public new void Hide()
    {
        IsVisible = false;
        Visible = false;
        _mainPanel.Visible = false;
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Combo || (keyEvent.Keycode == Key.K && keyEvent.Ctrl))
            {
                Toggle();
            }
            else if (keyEvent.Keycode == Key.Escape && IsVisible)
            {
                Hide();
            }
        }
    }
}
