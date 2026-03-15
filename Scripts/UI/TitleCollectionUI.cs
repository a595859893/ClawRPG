using Godot;
using System;
using System.Collections.Generic;

public partial class TitleCollectionUI : Control
{
    public static TitleCollectionUI Instance { get; private set; }
    
    // UI Elements
    private VBoxContainer _mainContainer;
    private TabContainer _tabContainer;
    
    // Collection tab
    private GridContainer _titleGrid;
    private Label _completionLabel;
    private Label _totalUnlockedLabel;
    private Button _unequipButton;
    
    // Statistics tab
    private Label _statsTotalLabel;
    private Label _statsCategoryLabel;
    private Label _statsRarityLabel;
    private Label _statsRecentLabel;
    
    // Filter controls
    private OptionButton _categoryFilter;
    private OptionButton _rarityFilter;
    private CheckBox _showHiddenCheck;
    
    // Current filter
    private TitleCollectionData.TitleCategory? _currentCategoryFilter = null;
    private TitleCollectionData.TitleRarity? _currentRarityFilter = null;
    private bool _showHidden = false;
    
    // Title buttons cache
    private Dictionary<string, Button> _titleButtons = new();
    
    public override void _Ready()
    {
        Instance = this;
        SetupUI();
        ConnectSignals();
        RefreshUI();
    }
    
    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorPreset(ControlPreset.FullRect);
        AddChild(_mainContainer);
        
        // Header
        var header = new HBoxContainer();
        _mainContainer.AddChild(header);
        
        var titleLabel = new Label();
        titleLabel.Text = "🏆 Title Collection";
        titleLabel.SizeFlagsHorizontal = ControlSize.Expand | ControlSize.ShrinkCenter;
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        header.AddChild(titleLabel);
        
        var closeButton = new Button();
        closeButton.Text = "✕";
        closeButton.Pressed += () => Hide();
        header.AddChild(closeButton);
        
        // Filter bar
        var filterBar = new HBoxContainer();
        _mainContainer.AddChild(filterBar);
        
        var categoryLabel = new Label();
        categoryLabel.Text = "Category:";
        filterBar.AddChild(categoryLabel);
        
        _categoryFilter = new OptionButton();
        _categoryFilter.AddItem("All Categories", 0);
        _categoryFilter.AddItem("Combat", 1);
        _categoryFilter.AddItem("Exploration", 2);
        _categoryFilter.AddItem("Crafting", 3);
        _categoryFilter.AddItem("Social", 4);
        _categoryFilter.AddItem("Achievement", 5);
        _categoryFilter.AddItem("Seasonal", 6);
        _categoryFilter.AddItem("Special", 7);
        _categoryFilter.AddItem("Hidden", 8);
        _categoryFilter.ItemSelected += OnCategoryFilterChanged;
        filterBar.AddChild(_categoryFilter);
        
        var rarityLabel = new Label();
        rarityLabel.Text = "  Rarity:";
        filterBar.AddChild(rarityLabel);
        
        _rarityFilter = new OptionButton();
        _rarityFilter.AddItem("All Rarities", 0);
        _rarityFilter.AddItem("Common", 1);
        _rarityFilter.AddItem("Uncommon", 2);
        _rarityFilter.AddItem("Rare", 3);
        _rarityFilter.AddItem("Epic", 4);
        _rarityFilter.AddItem("Legendary", 5);
        _rarityFilter.AddItem("Mythic", 6);
        _rarityFilter.ItemSelected += OnRarityFilterChanged;
        filterBar.AddChild(_rarityFilter);
        
        _showHiddenCheck = new CheckBox();
        _showHiddenCheck.Text = "Show Hidden";
        _showHiddenCheck.Toggled += OnShowHiddenToggled;
        filterBar.AddChild(_showHiddenCheck);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = ControlSize.Expand | ControlSize.Fill;
        _mainContainer.AddChild(_tabContainer);
        
        // Collection tab
        var collectionTab = new ScrollContainer();
        collectionTab.Name = "Collection";
        _tabContainer.AddChild(collectionTab);
        
        _titleGrid = new GridContainer();
        _titleGrid.Columns = 4;
        _titleGrid.SizeFlagsHorizontal = ControlSize.Expand | ControlSize.ShrinkCenter;
        _titleGrid.SizeFlagsVertical = ControlSize.Expand | ControlSize.Fill;
        _titleGrid.CustomMinimumSize = new Vector2(800, 400);
        collectionTab.AddChild(_titleGrid);
        
        // Stats bar
        var statsBar = new HBoxContainer();
        _mainContainer.AddChild(statsBar);
        
        _completionLabel = new Label();
        _completionLabel.Text = "Completion: 0%";
        statsBar.AddChild(_completionLabel);
        
        var spacer = new Control();
        spacer.SizeFlagsHorizontal = ControlSize.Expand;
        statsBar.AddChild(spacer);
        
        _totalUnlockedLabel = new Label();
        _totalUnlockedLabel.Text = "0 / 0 Titles Unlocked";
        statsBar.AddChild(_totalUnlockedLabel);
        
        _unequipButton = new Button();
        _unequipButton.Text = "Unequip Title";
        _unequipButton.Pressed += OnUnequipPressed;
        statsBar.AddChild(_unequipButton);
        
        // Statistics tab
        var statsTab = new VBoxContainer();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);
        
        _statsTotalLabel = new Label();
        _statsTotalLabel.Text = "Total Unlocked: 0";
        statsTab.AddChild(_statsTotalLabel);
        
        _statsCategoryLabel = new Label();
        _statsCategoryLabel.Text = "By Category:";
        statsTab.AddChild(_statsCategoryLabel);
        
        _statsRarityLabel = new Label();
        _statsRarityLabel.Text = "By Rarity:";
        statsTab.AddChild(_statsRarityLabel);
        
        _statsRecentLabel = new Label();
        _statsRecentLabel.Text = "Most Recent: None";
        statsTab.AddChild(_statsRecentLabel);
        
        // Initially hide
        Hide();
    }
    
    private void ConnectSignals()
    {
        if (TitleCollectionSystem.Instance != null)
        {
            TitleCollectionSystem.Instance.Connect(TitleCollectionSystem.SignalName.TitleUnlocked, 
                new Callable(this, MethodName.OnTitleUnlocked));
            TitleCollectionSystem.Instance.Connect(TitleCollectionSystem.SignalName.TitleEquipped, 
                new Callable(this, MethodName.OnTitleEquipped));
            TitleCollectionSystem.Instance.Connect(TitleCollectionSystem.SignalName.TitleUnequipped, 
                new Callable(this, MethodName.OnTitleUnequipped));
        }
    }
    
    public void RefreshUI()
    {
        // Clear existing buttons
        foreach (var child in _titleGrid.GetChildren())
        {
            child.QueueFree();
        }
        _titleButtons.Clear();
        
        // Get all titles
        var allTitles = TitleCollectionDatabase.Instance.GetAllTitles();
        
        foreach (var kvp in allTitles)
        {
            var title = kvp.Value;
            
            // Apply filters
            if (_currentCategoryFilter != null && title.Category != _currentCategoryFilter)
                continue;
                
            if (_currentRarityFilter != null && title.Rarity != _currentRarityFilter)
                continue;
                
            if (!title.IsHidden && !_showHidden && title.Category == TitleCollectionData.TitleCategory.Hidden)
                continue;
            
            // Create title button
            var button = CreateTitleButton(title);
            _titleGrid.AddChild(button);
            _titleButtons[title.Id] = button;
        }
        
        // Update stats
        UpdateStats();
    }
    
    private Button CreateTitleButton(TitleCollectionData.Title title)
    {
        var button = new Button();
        button.CustomMinimumSize = new Vector2(180, 60);
        
        var isUnlocked = TitleCollectionSystem.Instance.IsTitleUnlocked(title.Id);
        
        // Set appearance based on unlock status
        if (!isUnlocked)
        {
            button.Modulate = new Color(0.5f, 0.5f, 0.5f, 0.7f);
            button.Disabled = true;
        }
        
        // Set text
        var titleName = title.IsHidden && !isUnlocked ? "???" : title.Name;
        var rarityColor = GetRarityColor(title.Rarity);
        
        var text = $"[color=#{rarityColor.ToHex()}]{titleName}[/color]\n";
        text += title.Description;
        
        button.Text = text;
        button.Pressed += () => OnTitlePressed(title);
        
        // Set tooltip
        button.TooltipText = $"{title.Name}\n{title.Description}\nCategory: {title.Category}\nRarity: {title.Rarity}";
        
        return button;
    }
    
    private void UpdateStats()
    {
        var system = TitleCollectionSystem.Instance;
        
        // Update completion
        var completion = system.GetCompletionPercentage();
        _completionLabel.Text = $"Completion: {completion:F1}%";
        
        // Update total
        var unlocked = system.GetTotalUnlocked();
        var total = system.GetTotalAvailable();
        _totalUnlockedLabel.Text = $"{unlocked} / {total} Titles Unlocked";
        
        // Update statistics tab
        var stats = system.GetStatistics();
        _statsTotalLabel.Text = $"Total Unlocked: {stats.TotalUnlocked}";
        
        // Category breakdown
        var categoryText = "By Category:\n";
        foreach (TitleCollectionData.TitleCategory cat in Enum.GetValues(typeof(TitleCollectionData.TitleCategory)))
        {
            var count = TitleCollectionDatabase.Instance.GetTitleCountByCategory(cat);
            categoryText += $"  {cat}: {count}\n";
        }
        _statsCategoryLabel.Text = categoryText;
        
        // Rarity breakdown
        var rarityText = "By Rarity:\n";
        foreach (TitleCollectionData.TitleRarity rar in Enum.GetValues(typeof(TitleCollectionData.TitleRarity)))
        {
            var titles = TitleCollectionDatabase.Instance.GetTitlesByRarity(rar);
            var unlockedCount = 0;
            foreach (var t in titles)
            {
                if (system.IsTitleUnlocked(t.Id))
                    unlockedCount++;
            }
            rarityText += $"  {rar}: {unlockedCount}/{titles.Count}\n";
        }
        _statsRarityLabel.Text = rarityText;
        
        // Recent title
        _statsRecentLabel.Text = string.IsNullOrEmpty(stats.MostRecentTitle) 
            ? "Most Recent: None" 
            : $"Most Recent: {stats.MostRecentTitle}";
    }
    
    private Color GetRarityColor(TitleCollectionData.TitleRarity rarity)
    {
        return rarity switch
        {
            TitleCollectionData.TitleRarity.Common => new Color(1f, 1f, 1f),
            TitleCollectionData.TitleRarity.Uncommon => new Color(0.6f, 1f, 0.6f),
            TitleCollectionData.TitleRarity.Rare => new Color(0.4f, 0.7f, 1f),
            TitleCollectionData.TitleRarity.Epic => new Color(0.8f, 0.4f, 1f),
            TitleCollectionData.TitleRarity.Legendary => new Color(1f, 0.7f, 0.3f),
            TitleCollectionData.TitleRarity.Mythic => new Color(1f, 0.4f, 0.4f),
            _ => new Color(1f, 1f, 1f)
        };
    }
    
    #region Signal Handlers
    
    private void OnTitlePressed(TitleCollectionData.Title title)
    {
        var isUnlocked = TitleCollectionSystem.Instance.IsTitleUnlocked(title.Id);
        
        if (isUnlocked)
        {
            var activeId = TitleCollectionSystem.Instance.GetActiveTitleId();
            if (activeId == title.Id)
            {
                // Already equipped, unequip
                TitleCollectionSystem.Instance.UnequipTitle();
            }
            else
            {
                // Equip
                TitleCollectionSystem.Instance.EquipTitle(title.Id);
            }
        }
    }
    
    private void OnCategoryFilterChanged(long index)
    {
        if (index == 0)
            _currentCategoryFilter = null;
        else
            _currentCategoryFilter = (TitleCollectionData.TitleCategory)(index - 1);
        
        RefreshUI();
    }
    
    private void OnRarityFilterChanged(long index)
    {
        if (index == 0)
            _currentRarityFilter = null;
        else
            _currentRarityFilter = (TitleCollectionData.TitleRarity)(index - 1);
        
        RefreshUI();
    }
    
    private void OnShowHiddenToggled(bool toggled)
    {
        _showHidden = toggled;
        RefreshUI();
    }
    
    private void OnUnequipPressed()
    {
        TitleCollectionSystem.Instance.UnequipTitle();
    }
    
    private void OnTitleUnlocked(string titleId, TitleCollectionData.Title title)
    {
        RefreshUI();
    }
    
    private void OnTitleEquipped(string titleId)
    {
        RefreshUI();
    }
    
    private void OnTitleUnequipped()
    {
        RefreshUI();
    }
    
    #endregion
    
    #region Toggle
    
    public void Toggle()
    {
        if (Visible)
            Hide();
        else
        {
            Show();
            RefreshUI();
        }
    }
    
    #endregion
}
