using Godot;
using System;
using System.Collections.Generic;

public partial class TitleCollectionUI : Control
{
    private Button _closeButton;
    private TabContainer _tabContainer;
    
    // Overview tab
    private Label _totalCollectedLabel;
    private Label _progressLabel;
    private ProgressBar _progressBar;
    private Label _currentTitleLabel;
    private OptionButton _displayTitleSelector;
    
    // Collection tab
    private OptionButton _categoryFilter;
    private OptionButton _rarityFilter;
    private VBoxContainer _titleListContainer;
    private ScrollContainer _titleScroll;
    
    // Statistics tab
    private Label _statsTotalLabel;
    private Label _statsLegendaryLabel;
    private Label _statsEpicLabel;
    private Label _statsRareLabel;
    private Label _statsProgressLabel;
    
    private string _selectedCategory = "All";
    private string _selectedRarity = "All";
    
    public override void _Ready()
    {
        SetupUI();
        PopulateDisplayTitleSelector();
        RefreshUI();
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainPanel = new Panel();
        mainPanel.SetSize(new Vector2(800, 600));
        mainPanel.Position = new Vector2(100, 50);
        mainPanel.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0);
        mainVBox.AddThemeConstantOverride("separation", 10);
        mainPanel.AddChild(mainVBox);
        
        // 标题栏
        var titleBar = new HBoxContainer();
        titleBar.AddThemeConstantOverride("separation", 10);
        mainVBox.AddChild(titleBar);
        
        var titleLabel = new Label();
        titleLabel.Text = "🎖️ Title Collection";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleBar.AddChild(titleLabel);
        
        titleBar.AddChild(new Control()); // Spacer
        
        _closeButton = new Button();
        _closeButton.Text = "✕";
        _closeButton.RectMinSize = new Vector2(40, 40);
        _closeButton.Pressed += OnClosePressed;
        titleBar.AddChild(_closeButton);
        
        // Tab Container
        _tabContainer = new TabContainer();
        _tabContainer;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        mainVBox.AddChild(_tabContainer);
        
        // 创建标签页
        CreateOverviewTab();
        CreateCollectionTab();
        CreateStatisticsTab();
    }
    
    private void CreateOverviewTab()
    {
        var tab = new ScrollContainer();
        tab.Name = "Overview";
        _tabContainer.AddChild(tab);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 10);
        vbox.AddThemeConstantOverride("separation", 20);
        tab.AddChild(vbox);
        
        // 收集进度
        var progressSection = new VBoxContainer();
        vbox.AddChild(progressSection);
        
        var progressTitle = new Label();
        progressTitle.Text = "Collection Progress";
        progressTitle.AddThemeFontSizeOverride("font_size", 18);
        progressSection.AddChild(progressTitle);
        
        _progressBar = new ProgressBar();
        _progressBar.RectMinSize = new Vector2(0, 30);
        _progressBar.PercentVisible = true;
        progressSection.AddChild(_progressBar);
        
        _progressLabel = new Label();
        _progressLabel.Text = "0 / 0 (0%)";
        progressSection.AddChild(_progressLabel);
        
        _totalCollectedLabel = new Label();
        _totalCollectedLabel.Text = "Total Titles Collected: 0";
        _totalCollectedLabel.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(_totalCollectedLabel);
        
        // 当前显示标题
        var displaySection = new VBoxContainer();
        vbox.AddChild(displaySection);
        
        var displayTitle = new Label();
        displayTitle.Text = "Display Title";
        displayTitle.AddThemeFontSizeOverride("font_size", 18);
        displaySection.AddChild(displayTitle);
        
        var displayDesc = new Label();
        displayDesc.Text = "Select a title to display next to your name";
        displayDesc.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        displaySection.AddChild(displayDesc);
        
        _displayTitleSelector = new OptionButton();
        _displayTitleSelector.ItemSelected += OnDisplayTitleSelected;
        displaySection.AddChild(_displayTitleSelector);
        
        _currentTitleLabel = new Label();
        _currentTitleLabel.Text = "Current: None";
        _currentTitleLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.8f, 1.0f));
        vbox.AddChild(_currentTitleLabel);
    }
    
    private void CreateCollectionTab()
    {
        var tab = new VBoxContainer();
        tab.Name = "Collection";
        _tabContainer.AddChild(tab);
        
        // 筛选栏
        var filterBar = new HBoxContainer();
        filterBar.AddThemeConstantOverride("separation", 10);
        tab.AddChild(filterBar);
        
        var categoryLabel = new Label();
        categoryLabel.Text = "Category:";
        filterBar.AddChild(categoryLabel);
        
        _categoryFilter = new OptionButton();
        _categoryFilter.ItemSelected += OnCategorySelected;
        filterBar.AddChild(_categoryFilter);
        
        var rarityLabel = new Label();
        rarityLabel.Text = "  Rarity:";
        filterBar.AddChild(rarityLabel);
        
        _rarityFilter = new OptionButton();
        _rarityFilter.ItemSelected += OnRaritySelected;
        filterBar.AddChild(_rarityFilter);
        
        // 标题列表
        _titleScroll = new ScrollContainer();
        _titleScroll;
        _titleScroll.SizeFlagsVertical = Control.SizeFlags.Expand;
        tab.AddChild(_titleScroll);
        
        _titleListContainer = new VBoxContainer();
        _titleListContainer.AddThemeConstantOverride("separation", 5);
        _titleScroll.AddChild(_titleListContainer);
        
        // 填充筛选选项
        PopulateFilters();
    }
    
    private void CreateStatisticsTab()
    {
        var tab = new ScrollContainer();
        tab.Name = "Statistics";
        _tabContainer.AddChild(tab);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 20);
        vbox.AddThemeConstantOverride("separation", 15);
        tab.AddChild(vbox);
        
        var title = new Label();
        title.Text = "📊 Collection Statistics";
        title.AddThemeFontSizeOverride("font_size", 22);
        vbox.AddChild(title);
        
        var separator = new HSeparator();
        vbox.AddChild(separator);
        
        // 统计数据
        _statsTotalLabel = CreateStatLabel("Total Collected: 0 / 0");
        vbox.AddChild(_statsTotalLabel);
        
        _statsLegendaryLabel = CreateStatLabel("Legendary: 0");
        vbox.AddChild(_statsLegendaryLabel);
        
        _statsEpicLabel = CreateStatLabel("Epic: 0");
        vbox.AddChild(_statsEpicLabel);
        
        _statsRareLabel = CreateStatLabel("Rare: 0");
        vbox.AddChild(_statsRareLabel);
        
        var spacer = new Control();
        spacer.RectMinSize = new Vector2(0, 20);
        vbox.AddChild(spacer);
        
        var progressTitle = new Label();
        progressTitle.Text = "Progress by Rarity";
        progressTitle.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(progressTitle);
        
        _statsProgressLabel = new Label();
        _statsProgressLabel.Text = "Common: 0 | Uncommon: 0 | Rare: 0 | Epic: 0 | Legendary: 0";
        vbox.AddChild(_statsProgressLabel);
    }
    
    private Label CreateStatLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        label.AddThemeFontSizeOverride("font_size", 16);
        return label;
    }
    
    private void PopulateFilters()
    {
        // Category filter
        _categoryFilter.Clear();
        _categoryFilter.AddItem("All Categories", 0);
        string[] categories = TitleCollectionDatabase.GetCategories();
        for (int i = 0; i < categories.Length; i++)
        {
            _categoryFilter.AddItem(categories[i], i + 1);
        }
        
        // Rarity filter
        _rarityFilter.Clear();
        _rarityFilter.AddItem("All Rarities", 0);
        string[] rarities = TitleCollectionDatabase.GetRarities();
        for (int i = 0; i < rarities.Length; i++)
        {
            _rarityFilter.AddItem(rarities[i], i + 1);
        }
    }
    
    private void PopulateDisplayTitleSelector()
    {
        _displayTitleSelector.Clear();
        _displayTitleSelector.AddItem("None (No Display Title)", 0);
        
        var system = TitleCollectionSystem.Instance;
        if (system == null) return;
        
        var collected = system.GetCollectedTitleIds();
        int index = 1;
        foreach (string titleId in collected)
        {
            var config = TitleCollectionDatabase.GetTitleById(titleId);
            if (config != null)
            {
                string displayName = $"[{config["rarity"]}] {config["name"]}";
                _displayTitleSelector.AddItem(displayName, index);
                index++;
            }
        }
        
        // Select current
        string currentTitle = system.GetDisplayTitle();
        if (currentTitle == "")
        {
            _displayTitleSelector.Select(0);
        }
        else
        {
            // Find and select current title
            for (int i = 0; i < _displayTitleSelector.GetItemCount(); i++)
            {
                // Note: This is simplified, in production you'd map IDs properly
            }
        }
    }
    
    private void RefreshUI()
    {
        var system = TitleCollectionSystem.Instance;
        if (system == null) return;
        
        // Overview refresh
        var stats = system.GetStatistics();
        int total = (int)stats["total_collected"];
        int available = (int)stats["total_available"];
        float progress = (float)stats["progress"];
        
        _totalCollectedLabel.Text = $"Total Titles Collected: {total} / {available}";
        _progressBar.Value = progress * 100;
        _progressLabel.Text = $"{total} / {available} ({progress * 100:F1}%)";
        
        string currentTitle = system.GetDisplayTitle();
        if (currentTitle == "")
        {
            _currentTitleLabel.Text = "Current: None";
        }
        else
        {
            var config = system.GetDisplayTitleConfig();
            if (config != null)
            {
                _currentTitleLabel.Text = $"Current: [{config["rarity"]}] {config["name"]}";
            }
        }
        
        // Statistics refresh
        _statsTotalLabel.Text = $"Total Collected: {total} / {available}";
        _statsLegendaryLabel.Text = $"Legendary: {stats["legendary"]}";
        _statsEpicLabel.Text = $"Epic: {stats["epic"]}";
        _statsRareLabel.Text = $"Rare: {stats["rare"]}";
        
        // Refresh title list
        RefreshTitleList();
    }
    
    private void RefreshTitleList()
    {
        // Clear existing
        foreach (Node child in _titleListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var system = TitleCollectionSystem.Instance;
        if (system == null) return;
        
        var allTitles = TitleCollectionDatabase.GetAllTitles();
        
        foreach (var titleConfig in allTitles)
        {
            string titleId = (string)titleConfig["id"];
            string category = (string)titleConfig["category"];
            string rarity = (string)titleConfig["rarity"];
            
            // Apply filters
            if (_selectedCategory != "All" && category != _selectedCategory)
                continue;
            if (_selectedRarity != "All" && rarity != _selectedRarity)
                continue;
            
            bool collected = system.HasTitle(titleId);
            
            // Create title card
            var card = CreateTitleCard(titleConfig, collected);
            _titleListContainer.AddChild(card);
        }
    }
    
    private Control CreateTitleCard(Dictionary config, bool collected)
    {
        var panel = new PanelContainer();
        panel.Modulate = collected ? new Color(1, 1, 1, 1) : new Color(0.5f, 0.5f, 0.5f, 0.3f);
        
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);
        panel.AddChild(hbox);
        
        // Status icon
        var statusLabel = new Label();
        statusLabel.Text = collected ? "✅" : "🔒";
        statusLabel.AddThemeFontSizeOverride("font_size", 18);
        hbox.AddChild(statusLabel);
        
        // Title info
        var infoVbox = new VBoxContainer();
        hbox.AddChild(infoVbox);
        
        var nameLabel = new Label();
        string rarity = (string)config["rarity"];
        nameLabel.Text = $"[{rarity}] {config["name"]}";
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        nameLabel.AddThemeColorOverride("font_color", TitleCollectionDatabase.GetRarityColor(rarity));
        infoVbox.AddChild(nameLabel);
        
        var reqLabel = new Label();
        reqLabel.Text = config["requirement"].ToString();
        reqLabel.AddThemeFontSizeOverride("font_size", 12);
        reqLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        infoVbox.AddChild(reqLabel);
        
        return panel;
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    private void OnDisplayTitleSelected(int index)
    {
        var system = TitleCollectionSystem.Instance;
        if (system == null) return;
        
        if (index == 0)
        {
            system.SetDisplayTitle("");
        }
        else
        {
            var collected = system.GetCollectedTitleIds();
            if (index - 1 < collected.Count)
            {
                system.SetDisplayTitle((string)collected[index - 1]);
            }
        }
        
        RefreshUI();
    }
    
    private void OnCategorySelected(int index)
    {
        if (index == 0)
            _selectedCategory = "All";
        else
            _selectedCategory = TitleCollectionDatabase.GetCategories()[index - 1];
        
        RefreshTitleList();
    }
    
    private void OnRaritySelected(int index)
    {
        if (index == 0)
            _selectedRarity = "All";
        else
            _selectedRarity = TitleCollectionDatabase.GetRarities()[index - 1];
        
        RefreshTitleList();
    }
    
    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            QueueFree();
        }
    }
}
