using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 卡牌收藏界面控制器
/// 负责显示和管理玩家的卡牌收藏、卡包购买以及统计信息
/// </summary>
public partial class CardCollectionUI : Control
{
    private CardCollectionSystem _system;
    private CardCollectionDatabase _database;
    private CardCollectionData _data;
    
    // UI Elements
    private TabContainer _tabContainer;
    private VBoxContainer _collectionTab;
    private VBoxContainer _packsTab;
    private VBoxContainer _statisticsTab;
    
    // Collection tab elements
    private OptionButton _categoryFilter;
    private OptionButton _rarityFilter;
    private GridContainer _cardGrid;
    private Label _collectionProgress;
    private Label _totalCardsLabel;
    
    // Packs tab elements
    private GridContainer _packGrid;
    private Label _goldLabel;
    
    // Statistics tab elements
    private Label _statsLabel;
    private Label _rarityDistLabel;
    private Label _categoryDistLabel;
    
    // Card display
    private Panel _cardPreview;
    private Label _cardNameLabel;
    private Label _cardInfoLabel;
    private Label _cardDescLabel;
    private TextureRect _cardRarityIndicator;
    
    private string _selectedCategory = "All";
    private string _selectedRarity = "All";
    
    /// <summary>
    /// 节点准备就绪时调用
    /// 初始化系统引用并加载UI数据
    /// </summary>
    public override void _Ready()
    {
        _system = GetNode<CardCollectionSystem>("/root/CardCollectionSystem");
        _database = GetNode<CardCollectionDatabase>("/root/CardCollectionDatabase");
        _data = GetNode<CardCollectionData>("/root/CardCollectionData");
        
        SetupUI();
        RefreshCollection();
        RefreshPacks();
        RefreshStatistics();
    }
    
    /// <summary>
    /// 设置界面布局和控件
    /// 包括三个标签页：收藏、卡包、统计
    /// </summary>
    private void SetupUI()
    {
        // Main container
        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0f);
        AddChild(mainVBox);
        
        // Title
        var title = new Label();
        title.Text = "Card Collection";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(title);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0f);
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainVBox.AddChild(_tabContainer);
        
        // Collection tab
        _collectionTab = new VBoxContainer();
        _collectionTab.Name = "Collection";
        _tabContainer.AddChild(_collectionTab);
        
        // Filters
        var filterHBox = new HBoxContainer();
        _collectionTab.AddChild(filterHBox);
        
        var categoryLabel = new Label();
        categoryLabel.Text = "Category:";
        filterHBox.AddChild(categoryLabel);
        
        _categoryFilter = new OptionButton();
        _categoryFilter.AddItem("All", 0);
        _categoryFilter.AddItem("Attack", 1);
        _categoryFilter.AddItem("Skill", 2);
        _categoryFilter.AddItem("Power", 3);
        _categoryFilter.AddItem("Defense", 4);
        _categoryFilter.AddItem("Special", 5);
        _categoryFilter.ItemSelected += OnCategorySelected;
        filterHBox.AddChild(_categoryFilter);
        
        var rarityLabel = new Label();
        rarityLabel.Text = "Rarity:";
        rarityLabel.MarginLeft = 20;
        filterHBox.AddChild(rarityLabel);
        
        _rarityFilter = new OptionButton();
        _rarityFilter.AddItem("All", 0);
        _rarityFilter.AddItem("Common", 1);
        _rarityFilter.AddItem("Uncommon", 2);
        _rarityFilter.AddItem("Rare", 3);
        _rarityFilter.AddItem("Epic", 4);
        _rarityFilter.AddItem("Legendary", 5);
        _rarityFilter.ItemSelected += OnRaritySelected;
        filterHBox.AddChild(_rarityFilter);
        
        // Progress label
        _collectionProgress = new Label();
        _collectionProgress.HorizontalAlignment = HorizontalAlignment.Center;
        _collectionTab.AddChild(_collectionProgress);
        
        // Total cards label
        _totalCardsLabel = new Label();
        _totalCardsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _collectionTab.AddChild(_totalCardsLabel);
        
        // Card grid (scrollable)
        var scrollContainer = new ScrollContainer();
        scrollContainer.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0f);
        scrollContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _collectionTab.AddChild(scrollContainer);
        
        _cardGrid = new GridContainer();
        _cardGrid.Columns = 5;
        _cardGrid.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _cardGrid.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        scrollContainer.AddChild(_cardGrid);
        
        // Card preview panel
        _cardPreview = new Panel();
        _cardPreview.SetAnchorAndMargin(Control.LayoutPreset.BottomLeft, 0f);
        _cardPreview.CustomMinimumSize = new Vector2(300, 150);
        _collectionTab.AddChild(_cardPreview);
        
        var previewVBox = new VBoxContainer();
        _cardPreview.AddChild(previewVBox);
        
        _cardNameLabel = new Label();
        _cardNameLabel.Text = "Select a card";
        _cardNameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        previewVBox.AddChild(_cardNameLabel);
        
        _cardInfoLabel = new Label();
        _cardInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        previewVBox.AddChild(_cardInfoLabel);
        
        _cardDescLabel = new Label();
        _cardDescLabel.Text = "";
        _cardDescLabel.Autowrap = true;
        previewVBox.AddChild(_cardDescLabel);
        
        // Packs tab
        _packsTab = new VBoxContainer();
        _packsTab.Name = "Packs";
        _tabContainer.AddChild(_packsTab);
        
        _goldLabel = new Label();
        _goldLabel.Text = "Gold: 0";
        _goldLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _packsTab.AddChild(_goldLabel);
        
        var packScroll = new ScrollContainer();
        packScroll.SetAnchorAndMargin(Control.LayoutPreset.FullRect, 0f);
        packScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _packsTab.AddChild(packScroll);
        
        _packGrid = new GridContainer();
        _packGrid.Columns = 3;
        _packGrid.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        packScroll.AddChild(_packGrid);
        
        // Statistics tab
        _statisticsTab = new VBoxContainer();
        _statisticsTab.Name = "Statistics";
        _tabContainer.AddChild(_statisticsTab);
        
        _statsLabel = new Label();
        _statsLabel.Autowrap = true;
        _statisticsTab.AddChild(_statsLabel);
        
        _rarityDistLabel = new Label();
        _rarityDistLabel.MarginTop = 20;
        _rarityDistLabel.Autowrap = true;
        _statisticsTab.AddChild(_rarityDistLabel);
        
        _categoryDistLabel = new Label();
        _categoryDistLabel.MarginTop = 20;
        _categoryDistLabel.Autowrap = true;
        _statisticsTab.AddChild(_categoryDistLabel);
    }
    
    /// <summary>
    /// 分类筛选选项改变时的回调
    /// </summary>
    /// <param name="index">选中的分类索引</param>
    private void OnCategorySelected(int index)
    {
        string[] categories = { "All", "Attack", "Skill", "Power", "Defense", "Special" };
        _selectedCategory = categories[index];
        RefreshCollection();
    }
    
    /// <summary>
    /// 稀有度筛选选项改变时的回调
    /// </summary>
    /// <param name="index">选中的稀有度索引</param>
    private void OnRaritySelected(int index)
    {
        string[] rarities = { "All", "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        _selectedRarity = rarities[index];
        RefreshCollection();
    }
    
    /// <summary>
    /// 刷新收藏界面显示
    /// 根据当前筛选条件显示已拥有的卡牌
    /// </summary>
    private void RefreshCollection()
    {
        // Clear grid
        foreach (Node child in _cardGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        int totalCards = _database.Cards.Count;
        float progress = _system.GetCollectionProgress();
        
        _collectionProgress.Text = $"Collection Progress: {progress:F1}% ({_data.TotalUniqueCards}/{totalCards})";
        _totalCardsLabel.Text = $"Total Cards Obtained: {_data.TotalCardsObtained} | Duplicates: {_data.TotalDuplicates}";
        
        // Get filtered cards
        foreach (var kvp in _data.OwnedCards)
        {
            var card = _database.GetCard(kvp.Key);
            if (card == null) continue;
            
            // Apply filters
            if (_selectedCategory != "All" && card.Category != _selectedCategory)
                continue;
            if (_selectedRarity != "All" && card.Rarity != _selectedRarity)
                continue;
            
            // Create card button
            var cardBtn = CreateCardButton(kvp.Key, kvp.Value);
            _cardGrid.AddChild(cardBtn);
        }
    }
    
    /// <summary>
    /// 创建卡牌显示按钮
    /// </summary>
    /// <param name="cardId">卡牌ID</param>
    /// <param name="count">拥有数量</param>
    /// <returns>创建的卡牌按钮</returns>
    private Button CreateCardButton(string cardId, int count)
    {
        var card = _database.GetCard(cardId);
        var btn = new Button();
        btn.CustomMinimumSize = new Vector2(100, 120);
        btn.Text = $"{card.Name}\nx{count}";
        
        // Color based on rarity
        var color = _database.GetRarityColor(card.Rarity);
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = color;
        styleBox.SetCornerRadiusAll(8);
        styleBox.SetBorderWidthAll(2);
        styleBox.BorderColor = color.Darkened(0.3f);
        btn.AddThemeStyleboxOverride("normal", styleBox);
        
        // Highlight favorites
        if (_system.IsFavorite(cardId))
        {
            btn.Text = $"★ {card.Name}\nx{count}";
        }
        
        btn.Pressed += () => ShowCardPreview(cardId);
        
        return btn;
    }
    
    /// <summary>
    /// 显示卡牌预览信息
    /// </summary>
    /// <param name="cardId">要预览的卡牌ID</param>
    private void ShowCardPreview(string cardId)
    {
        var card = _database.GetCard(cardId);
        var count = _system.GetCardCount(cardId);
        
        _cardNameLabel.Text = card.Name;
        _cardInfoLabel.Text = $"{card.Category} | {card.Rarity} | Cost: {card.Cost} | Damage: {card.BaseDamage}\nOwned: {count}";
        _cardDescLabel.Text = card.Description;
        
        // Favorite button
        var favBtn = new Button();
        favBtn.Text = _system.IsFavorite(cardId) ? "★ Unfavorite" : "☆ Favorite";
        favBtn.MarginTop = 120;
        _cardPreview.AddChild(favBtn);
        
        favBtn.Pressed -= () => _system.ToggleFavorite(cardId);
        favBtn.Pressed += () => 
        {
            _system.ToggleFavorite(cardId);
            RefreshCollection();
            ShowCardPreview(cardId);
        };
    }
    
    /// <summary>
    /// 刷新卡包界面显示
    /// </summary>
    private void RefreshPacks()
    {
        // Clear pack grid
        foreach (Node child in _packGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add pack buttons
        foreach (var kvp in _database.Packs)
        {
            var pack = kvp.Value;
            var packBtn = CreatePackButton(pack);
            _packGrid.AddChild(packBtn);
        }
    }
    
    /// <summary>
    /// 创建卡包购买按钮
    /// </summary>
    /// <param name="pack">卡包数据</param>
    /// <returns>创建的卡包按钮</returns>
    private Button CreatePackButton(CardPack pack)
    {
        var btn = new Button();
        btn.CustomMinimumSize = new Vector2(150, 100);
        btn.Text = $"{pack.Name}\n{pack.Price} Gold";
        
        btn.Pressed += () => 
        {
            var obtained = _system.OpenPack(pack.Id);
            GD.Print($"Opened {pack.Name}: {string.Join(", ", obtained)}");
            RefreshCollection();
            RefreshPacks();
            RefreshStatistics();
        };
        
        return btn;
    }
    
    /// <summary>
    /// 刷新统计数据界面显示
    /// </summary>
    private void RefreshStatistics()
    {
        var stats = _system.GetStatistics();
        
        _statsLabel.Text = $"Statistics:\n" +
            $"Total Unique Cards: {stats["TotalUniqueCards"]}\n" +
            $"Total Cards Obtained: {stats["TotalCardsObtained"]}\n" +
            $"Total Duplicates: {stats["TotalDuplicates"]}\n" +
            $"Packs Opened: {stats["PacksOpened"]}\n" +
            $"Gold Spent: {stats["TotalGoldSpent"]}\n" +
            $"Favorite Cards: {stats["FavoriteCount"]}\n" +
            $"Deck Buildable: {stats["DeckBuildableCount"]}";
        
        var rarityDist = _system.GetRarityDistribution();
        _rarityDistLabel.Text = "Rarity Distribution:\n" +
            $"Common: {rarityDist["Common"]}\n" +
            $"Uncommon: {rarityDist["Uncommon"]}\n" +
            $"Rare: {rarityDist["Rare"]}\n" +
            $"Epic: {rarityDist["Epic"]}\n" +
            $"Legendary: {rarityDist["Legendary"]}";
        
        var categoryDist = _system.GetCategoryDistribution();
        _categoryDistLabel.Text = "Category Distribution:\n" +
            $"Attack: {categoryDist["Attack"]}\n" +
            $"Skill: {categoryDist["Skill"]}\n" +
            $"Power: {categoryDist["Power"]}\n" +
            $"Defense: {categoryDist["Defense"]}\n" +
            $"Special: {categoryDist["Special"]}";
    }
}
