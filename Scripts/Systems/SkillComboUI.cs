using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 技能Combo用户界面。显示和管理技能Combo面板。
/// </summary>
public partial class SkillComboUI : Control
{
    /// <summary>
    /// 获取界面单例实例。
    /// </summary>
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
    
    // === REQ-159: Combo Forget Visualization ===
    // Filter bar
    private HBoxContainer _forgetFilterBar;
    private Button _filterAllBtn;
    private Button _filterProficientBtn;
    private Button _filterRustyBtn;
    private Button _filterForgottenBtn;
    
    /// <summary>当前过滤状态，null = 显示全部</summary>
    private ComboForgetLevel? _currentFilter = null;
    
    /// <summary>所有 combo 面板，按 comboId 索引，用于快速刷新</summary>
    private Dictionary<string, PanelContainer> _comboPanels = new Dictionary<string, PanelContainer>();
    
    /// <summary>每个 combo 面板的遗忘指示器 Label，按 comboId 索引</summary>
    private Dictionary<string, Label> _forgetIndicators = new Dictionary<string, Label>();
    
    /// <summary>每个 combo 面板的遗忘提示 Tooltip Label，按 comboId 索引</summary>
    private Dictionary<string, Label> _forgetTooltips = new Dictionary<string, Label>();
    
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
        
        // === REQ-159: 遗忘等级过滤栏 ===
        SetupForgetFilterBar();
        _contentBox.AddChild(_forgetFilterBar);
        
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
    
    // ========== REQ-159: Combo Forget Visualization ==========
    
    private void SetupForgetFilterBar()
    {
        _forgetFilterBar = new HBoxContainer();
        _forgetFilterBar.Alignment = BoxContainer.AlignmentMode.Center;
        _forgetFilterBar.CustomMinimumSize = new Vector2(0, 32);
        
        var filterLabel = new Label();
        filterLabel.Text = "熟练度: ";
        filterLabel.AddThemeFontSizeOverride("font_size", 13);
        filterLabel.Modulate = new Color(0.6f, 0.6f, 0.7f);
        _forgetFilterBar.AddChild(filterLabel);
        
        _filterAllBtn = MakeFilterButton("全部", null);
        _filterProficientBtn = MakeFilterButton("熟练", ComboForgetLevel.Proficient);
        _filterRustyBtn = MakeFilterButton("生疏", ComboForgetLevel.Rusty);
        _filterForgottenBtn = MakeFilterButton("遗忘", ComboForgetLevel.Forgotten);
        
        _forgetFilterBar.AddChild(_filterAllBtn);
        _forgetFilterBar.AddChild(_filterProficientBtn);
        _forgetFilterBar.AddChild(_filterRustyBtn);
        _forgetFilterBar.AddChild(_filterForgottenBtn);
        
        UpdateFilterButtonStyles();
    }
    
    private Button MakeFilterButton(string text, ComboForgetLevel? level)
    {
        var btn = new Button();
        btn.Text = text;
        btn.AddThemeFontSizeOverride("font_size", 12);
        btn.CustomMinimumSize = new Vector2(60, 28);
        
        if (level.HasValue)
        {
            var levelColor = GetForgetLevelColor(level.Value);
            var styleNormal = new StyleBoxFlat();
            styleNormal.BgColor = new Color(0.2f, 0.2f, 0.25f);
            styleNormal.BorderColor = new Color(0.4f, 0.4f, 0.45f);
            styleNormal.SetBorderWidthAll(1);
            styleNormal.SetCornerRadiusAll(4);
            btn.AddThemeStyleboxOverride("normal", styleNormal);
            
            var stylePressed = new StyleBoxFlat();
            stylePressed.BgColor = new Color(levelColor.R * 0.4f, levelColor.G * 0.4f, levelColor.B * 0.4f);
            stylePressed.BorderColor = levelColor;
            stylePressed.SetBorderWidthAll(1);
            stylePressed.SetCornerRadiusAll(4);
            btn.AddThemeStyleboxOverride("pressed", stylePressed);
        }
        
        btn.Pressed += () => OnFilterSelected(level);
        return btn;
    }
    
    private void OnFilterSelected(ComboForgetLevel? level)
    {
        _currentFilter = level;
        UpdateFilterButtonStyles();
        RepopulateComboList();
    }
    
    private void UpdateFilterButtonStyles()
    {
        SetFilterActive(_filterAllBtn, _currentFilter == null);
        SetFilterActive(_filterProficientBtn, _currentFilter == ComboForgetLevel.Proficient);
        SetFilterActive(_filterRustyBtn, _currentFilter == ComboForgetLevel.Rusty);
        SetFilterActive(_filterForgottenBtn, _currentFilter == ComboForgetLevel.Forgotten);
    }
    
    private void SetFilterActive(Button btn, bool active)
    {
        if (btn == null) return;
        var normalStyle = (StyleBoxFlat)btn.GetThemeStylebox("normal");
        if (active)
        {
            // 高亮：背景变亮 + 边框变成对应颜色
            normalStyle.BgColor = new Color(0.3f, 0.3f, 0.35f);
        }
        else
        {
            normalStyle.BgColor = new Color(0.2f, 0.2f, 0.25f);
        }
    }
    
    private Color GetForgetLevelColor(ComboForgetLevel level)
    {
        return level switch
        {
            ComboForgetLevel.Proficient => new Color(0.2f, 0.9f, 0.3f),
            ComboForgetLevel.Rusty => new Color(0.95f, 0.75f, 0.2f),
            ComboForgetLevel.Forgotten => new Color(0.9f, 0.2f, 0.2f),
            _ => Colors.White
        };
    }
    
    private string GetForgetLevelText(ComboForgetLevel level)
    {
        return level switch
        {
            ComboForgetLevel.Proficient => "✓",
            ComboForgetLevel.Rusty => "~",
            ComboForgetLevel.Forgotten => "?",
            _ => ""
        };
    }
    
    /// <summary>
    /// 刷新指定 combo 的遗忘指示器（信号回调）
    /// </summary>
    public void RefreshForgetIndicator(string comboId)
    {
        if (!_forgetIndicators.TryGetValue(comboId, out var indicator)) return;
        if (!_comboPanels.TryGetValue(comboId, out var panel)) return;
        
        var (level, color, icon, successRate, games) = GetForgetDisplayInfo(comboId);
        
        indicator.Text = icon;
        indicator.Modulate = color;
        
        // 更新面板边框颜色（叠加稀有度）
        var panelStyle = (StyleBoxFlat)panel.GetThemeStylebox("panel");
        // 保留原有稀有度边框色，叠加一个半透明遗忘色
        panel.Modulate = color;
        
        // 更新 tooltip
        if (_forgetTooltips.TryGetValue(comboId, out var tooltip))
        {
            string tooltipText = level switch
            {
                ComboForgetLevel.Proficient => $"熟练 | 成功率: {successRate:P0}",
                ComboForgetLevel.Rusty => $"生疏 | {games}局未用 | 成功率: {successRate:P0}",
                ComboForgetLevel.Forgotten => $"遗忘 | {games}局未用 | 成功率: {successRate:P0}",
                _ => ""
            };
            tooltip.Text = tooltipText;
        }
    }
    
    private (ComboForgetLevel level, Color color, string icon, float successRate, int games) GetForgetDisplayInfo(string comboId)
    {
        var level = ComboForgetUI.Instance?.GetForgetLevel(comboId) ?? ComboForgetLevel.Proficient;
        var color = GetForgetLevelColor(level);
        var icon = GetForgetLevelText(level);
        var successRate = ComboForgetUI.Instance?.GetEffectiveSuccessRate(comboId) ?? 1.0f;
        int games = 0;
        if (Framework.ComboForgetSystem.Instance != null)
        {
            var info = Framework.ComboForgetSystem.Instance.GetForgetInfo(comboId);
            games = info.games;
        }
        return (level, color, icon, successRate, games);
    }
    
    /// <summary>
    /// 过滤并重新显示 combo 列表
    /// </summary>
    private void RepopulateComboList()
    {
        // 隐藏不符合过滤条件的面板
        foreach (var kvp in _comboPanels)
        {
            var comboId = kvp.Key;
            var panel = kvp.Value;
            
            if (_currentFilter == null)
            {
                panel.Visible = true;
            }
            else
            {
                var level = ComboForgetUI.Instance?.GetForgetLevel(comboId) ?? ComboForgetLevel.Proficient;
                panel.Visible = (level == _currentFilter.Value);
            }
        }
    }
    
    // ========== End REQ-159 ==========
    
    private void PopulateComboList()
    {
        // 清空旧面板字典（UI children 由 Godot 管理）
        _comboPanels.Clear();
        _forgetIndicators.Clear();
        _forgetTooltips.Clear();
        
        // 清空现有 UI children
        foreach (var child in _comboListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
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
        panel.CustomMinimumSize = new Vector2(360, 95);  // 稍微加高以容纳遗忘提示
        
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.15f, 0.2f);
        style.BorderColor = GetComboRarityColor(combo.Bonus.RequiredComboCount);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(5);
        panel.AddThemeStyleboxOverride("panel", style);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.MarginLeft = 10;
        vbox.MarginRight = 10;
        vbox.MarginTop = 5;
        vbox.MarginBottom = 5;
        panel.AddChild(vbox);
        
        // Top row: forget indicator + combo name + usage count
        var topRow = new HBoxContainer();
        topRow.Alignment = BoxContainer.AlignmentMode.Center;
        vbox.AddChild(topRow);
        
        // === REQ-159: 遗忘等级指示器 ===
        var forgetBadge = new Label();
        var (initLevel, initColor, initIcon, initRate, initGames) = GetForgetDisplayInfo(combo.ComboId);
        forgetBadge.Text = initIcon;
        forgetBadge.Modulate = initColor;
        forgetBadge.AddThemeFontSizeOverride("font_size", 14);
        forgetBadge.CustomMinimumSize = new Vector2(20, 20);
        topRow.AddChild(forgetBadge);
        _forgetIndicators[combo.ComboId] = forgetBadge;
        
        var spacer = new Control();
        spacer.CustomMinimumSize = new Vector2(6, 0);
        topRow.AddChild(spacer);
        
        // Combo name
        var nameLabel = new Label();
        nameLabel.Text = $"⚔️ {combo.Name}";
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        nameLabel.Modulate = GetComboRarityColor(combo.Bonus.RequiredComboCount);
        nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        topRow.AddChild(nameLabel);
        
        // Right side - usage count
        var countLabel = new Label();
        int usageCount = SkillComboSystem.Instance?.GetComboUsageCount(combo.ComboId) ?? 0;
        countLabel.Text = $"x{usageCount}";
        countLabel.AddThemeFontSizeOverride("font_size", 20);
        countLabel.Modulate = new Color(0.8f, 0.8f, 0.9f);
        topRow.AddChild(countLabel);
        
        // Middle: desc + bonus
        var descBox = new HBoxContainer();
        vbox.AddChild(descBox);
        
        var descLabel = new Label();
        descLabel.Text = $"{combo.Type} | {combo.Trigger} | {combo.SkillIds.Count} skills";
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        descLabel.Modulate = new Color(0.7f, 0.7f, 0.8f);
        descBox.AddChild(descLabel);
        
        descBox.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.ExpandFill });
        
        var bonusLabel = new Label();
        bonusLabel.Text = $"Bonus: {combo.Bonus.Name} ({combo.Bonus.Description})";
        bonusLabel.AddThemeFontSizeOverride("font_size", 12);
        bonusLabel.Modulate = new Color(0.5f, 0.9f, 0.5f);
        descBox.AddChild(bonusLabel);
        
        // === REQ-159: 遗忘提示行 ===
        var tooltipRow = new HBoxContainer();
        tooltipRow.Alignment = BoxContainer.AlignmentMode.End;
        vbox.AddChild(tooltipRow);
        
        var forgetTooltip = new Label();
        string initTooltip = initLevel switch
        {
            ComboForgetLevel.Proficient => $"熟练 | 成功率: {initRate:P0}",
            ComboForgetLevel.Rusty => $"生疏 | {initGames}局未用 | 成功率: {initRate:P0}",
            ComboForgetLevel.Forgotten => $"遗忘 | {initGames}局未用 | 成功率: {initRate:P0}",
            _ => ""
        };
        forgetTooltip.Text = initTooltip;
        forgetTooltip.AddThemeFontSizeOverride("font_size", 11);
        forgetTooltip.Modulate = new Color(0.55f, 0.55f, 0.65f);
        tooltipRow.AddChild(forgetTooltip);
        _forgetTooltips[combo.ComboId] = forgetTooltip;
        
        // 注册到字典
        _comboPanels[combo.ComboId] = panel;
        
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
