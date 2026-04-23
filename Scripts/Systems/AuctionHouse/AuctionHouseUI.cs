using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Systems.AuctionHouse;

public partial class AuctionHouseUI : Control
{
    private VBoxContainer _mainContainer;
    private HBoxContainer _categoryContainer;
    private GridContainer _listingsGrid;
    private Label _titleLabel;
    private Label _statsLabel;
    private LineEdit _searchInput;
    private OptionButton _categoryFilter;
    private OptionButton _rarityFilter;
    private Button _searchButton;
    private Button _refreshButton;
    private Button _createListingButton;
    private Button _myListingsButton;
    private TabContainer _tabContainer;
    
    private PanelContainer _detailsPanel;
    private Label _detailsTitle;
    private Label _detailsInfo;
    private Label _detailsPrice;
    private Button _buyButton;
    private Button _cancelButton;
    
    private AuctionHouseSystem _auctionSystem;
    private string _currentTab = "browse";
    private int _selectedListingId = -1;
    
    private string[] _categories = { "全部", "武器", "防具", "饰品", "消耗品", "材料", "宠物", "坐骑", "其他" };
    private string[] _rarities = { "全部", "普通", "优秀", "稀有", "史诗", "传说" };
    
    public override void _Ready()
    {
        _auctionSystem = GetNode<AuctionHouseSystem>("/root/Main/AuctionHouseSystem");
        
        SetupUI();
        ConnectSignals();
        RefreshListings();
        
        GD.Print("AuctionHouseUI: 拍卖行界面已加载");
    }
    
    private void SetupUI()
    {
        var bg = new TextureRect();
        bg.Texture = GD.Load<Texture2D>("res://assets/ui/panel_bg.png");
        bg.Modulate = new Color(1, 1, 1, 0.9f);
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        AddChild(bg);
        
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _mainContainer.OffsetLeft = 50;
        _mainContainer.OffsetTop = 50;
        _mainContainer.OffsetRight = -50;
        _mainContainer.OffsetBottom = -50;
        _mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(_mainContainer);
        
        var titleBar = new HBoxContainer();
        _mainContainer.AddChild(titleBar);
        
        _titleLabel = new Label();
        _titleLabel.Text = "🏪 拍卖行";
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        titleBar.AddChild(_titleLabel);
        
        titleBar.AddChild(new Control());
        ((Control)titleBar.GetChild(titleBar.GetChildCount() - 1)).SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        
        var closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.TooltipText = "关闭 (ESC)";
        closeBtn.Pressed += () => Hide();
        titleBar.AddChild(closeBtn);
        
        var filterBar = new HBoxContainer();
        _mainContainer.AddChild(filterBar);
        
        var searchLabel = new Label();
        searchLabel.Text = "搜索: ";
        filterBar.AddChild(searchLabel);
        
        _searchInput = new LineEdit();
        _searchInput.PlaceholderText = "搜索物品...";
        _searchInput.CustomMinimumSize = new Vector2(200, 0);
        filterBar.AddChild(_searchInput);
        
        var catLabel = new Label();
        catLabel.Text = "  分类: ";
        filterBar.AddChild(catLabel);
        
        _categoryFilter = new OptionButton();
        foreach (var cat in _categories)
        {
            _categoryFilter.AddItem(cat);
        }
        _categoryFilter.Selected = 0;
        filterBar.AddChild(_categoryFilter);
        
        var rareLabel = new Label();
        rareLabel.Text = "  稀有度: ";
        filterBar.AddChild(rareLabel);
        
        _rarityFilter = new OptionButton();
        foreach (var rare in _rarities)
        {
            _rarityFilter.AddItem(rare);
        }
        _rarityFilter.Selected = 0;
        filterBar.AddChild(_rarityFilter);
        
        _searchButton = new Button();
        _searchButton.Text = "搜索";
        _searchButton.Pressed += OnSearchPressed;
        filterBar.AddChild(_searchButton);
        
        _refreshButton = new Button();
        _refreshButton.Text = "刷新";
        _refreshButton.Pressed += RefreshListings;
        filterBar.AddChild(_refreshButton);
        
        var actionBar = new HBoxContainer();
        _mainContainer.AddChild(actionBar);
        
        var browseTab = new Button();
        browseTab.Text = "浏览市场";
        browseTab.Pressed += () => SwitchTab("browse");
        actionBar.AddChild(browseTab);
        
        _myListingsButton = new Button();
        _myListingsButton.Text = "我的挂售";
        _myListingsButton.Pressed += () => SwitchTab("my_listings");
        actionBar.AddChild(_myListingsButton);
        
        _createListingButton = new Button();
        _createListingButton.Text = "发布挂售";
        _createListingButton.Pressed += () => SwitchTab("create");
        actionBar.AddChild(_createListingButton);
        
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _mainContainer.AddChild(_tabContainer);
        
        var browseTabControl = new Control();
        browseTabControl.Name = "Browse";
        _tabContainer.AddChild(browseTabControl);
        SetupBrowseTab(browseTabControl);
        
        var myListingsTabControl = new Control();
        myListingsTabControl.Name = "MyListings";
        _tabContainer.AddChild(myListingsTabControl);
        SetupMyListingsTab(myListingsTabControl);
        
        var createTabControl = new Control();
        createTabControl.Name = "Create";
        _tabContainer.AddChild(createTabControl);
        SetupCreateTab(createTabControl);
        
        var statsBar = new HBoxContainer();
        _mainContainer.AddChild(statsBar);
        
        _statsLabel = new Label();
        _statsLabel.Text = "正在加载统计数据...";
        statsBar.AddChild(_statsLabel);
    }
    
    private void SetupBrowseTab(Control parent)
    {
        var hbox = new HBoxContainer();
        hbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        parent.AddChild(hbox);
        
        var scroll = new ScrollContainer();
        scroll.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(scroll);
        
        _listingsGrid = new GridContainer();
        _listingsGrid.Columns = 4;
        _listingsGrid.AddThemeConstantOverride("h_separation", 10);
        _listingsGrid.AddThemeConstantOverride("v_separation", 10);
        scroll.AddChild(_listingsGrid);
        
        _detailsPanel = new PanelContainer();
        _detailsPanel.CustomMinimumSize = new Vector2(300, 0);
        hbox.AddChild(_detailsPanel);
        
        var detailsVBox = new VBoxContainer();
        _detailsPanel.AddChild(detailsVBox);
        
        _detailsTitle = new Label();
        _detailsTitle.Text = "请选择一个物品";
        _detailsTitle.AddThemeFontSizeOverride("font_size", 18);
        detailsVBox.AddChild(_detailsTitle);
        
        _detailsInfo = new Label();
        _detailsInfo.Text = "";
        _detailsInfo.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        detailsVBox.AddChild(_detailsInfo);
        
        _detailsPrice = new Label();
        _detailsPrice.Text = "";
        detailsVBox.AddChild(_detailsPrice);
        
        detailsVBox.AddChild(new Control());
        ((Control)detailsVBox.GetChild(detailsVBox.GetChildCount() - 1)).SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        
        _buyButton = new Button();
        _buyButton.Text = "购买";
        _buyButton.Disabled = true;
        _buyButton.Pressed += OnBuyPressed;
        detailsVBox.AddChild(_buyButton);
        
        _cancelButton = new Button();
        _cancelButton.Text = "取消挂售";
        _cancelButton.Disabled = true;
        _cancelButton.Pressed += OnCancelPressed;
        detailsVBox.AddChild(_cancelButton);
    }
    
    private void SetupMyListingsTab(Control parent)
    {
        var label = new Label();
        label.Text = "我的挂售列表";
        label.SetAnchorsPreset(Control.LayoutPreset.Center);
        parent.AddChild(label);
    }
    
    private void SetupCreateTab(Control parent)
    {
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.OffsetLeft = 20;
        vbox.OffsetTop = 20;
        parent.AddChild(vbox);
        
        var title = new Label();
        title.Text = "发布新挂售";
        title.AddThemeFontSizeOverride("font_size", 22);
        vbox.AddChild(title);
        
        vbox.AddChild(new Control());
        ((Control)vbox.GetChild(vbox.GetChildCount() - 1)).SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        
        var hint = new Label();
        hint.Text = "挂售功能开发中...\n请在浏览市场时选择物品进行购买";
        hint.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vbox.AddChild(hint);
    }
    
    private void ConnectSignals()
    {
        _searchInput.TextSubmitted += (text) => OnSearchPressed();
    }
    
    private void OnSearchPressed()
    {
        RefreshListings();
    }
    
    private void RefreshListings()
    {
        foreach (var child in _listingsGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        string category = _categoryFilter.GetItemText(_categoryFilter.Selected);
        string rarity = _rarityFilter.GetItemText(_rarityFilter.Selected);
        string search = _searchInput.Text;
        
        string categoryFilter = category == "全部" ? "" : category;
        string rarityFilter = rarity == "全部" ? "" : rarity;
        
        var listings = _auctionSystem.GetListings(categoryFilter, rarityFilter, search, 50);
        
        foreach (var listing in listings)
        {
            var itemCard = CreateListingCard(listing);
            _listingsGrid.AddChild(itemCard);
        }
        
        if (listings.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无挂售物品";
            _listingsGrid.AddChild(emptyLabel);
        }
        
        UpdateStatistics();
    }
    
    private Control CreateListingCard(AuctionItem listing)
    {
        var card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(180, 120);
        
        var vbox = new VBoxContainer();
        card.AddChild(vbox);
        
        var nameLabel = new Label();
        nameLabel.Text = listing.ItemName;
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        nameLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        vbox.AddChild(nameLabel);
        
        var rarityLabel = new Label();
        rarityLabel.Text = $"稀有度: {listing.Rarity}";
        vbox.AddChild(rarityLabel);
        
        var quantityLabel = new Label();
        quantityLabel.Text = $"数量: {listing.Quantity}";
        vbox.AddChild(quantityLabel);
        
        var priceLabel = new Label();
        priceLabel.Text = $"💰 {listing.PricePerUnit} x {listing.Quantity}";
        priceLabel.AddThemeFontSizeOverride("font_size", 16);
        vbox.AddChild(priceLabel);
        
        var sellerLabel = new Label();
        sellerLabel.Text = $"卖家: {listing.SellerName}";
        sellerLabel.AddThemeFontSizeOverride("font_size", 10);
        vbox.AddChild(sellerLabel);
        
        var selectBtn = new Button();
        selectBtn.Text = "查看";
        selectBtn.Pressed += () => SelectListing(listing);
        vbox.AddChild(selectBtn);
        
        return card;
    }
    
    private void SelectListing(AuctionItem listing)
    {
        _selectedListingId = 0;
        
        foreach (var kvp in _auctionSystem.GetListings())
        {
            if (kvp.ItemName == listing.ItemName && kvp.SellerName == listing.SellerName)
            {
                _selectedListingId = kvp.GetHashCode();
                break;
            }
        }
        
        _detailsTitle.Text = listing.ItemName;
        _detailsInfo.Text = $"{listing.ItemDescription}\n\n分类: {listing.Category}\n稀有度: {listing.Rarity}\n数量: {listing.Quantity}\n卖家: {listing.SellerName}";
        _detailsPrice.Text = $"总价: {listing.PricePerUnit * listing.Quantity} 金币";
        
        _buyButton.Disabled = false;
        
        if (_currentTab == "my_listings")
        {
            _cancelButton.Disabled = false;
        }
    }
    
    private void OnBuyPressed()
    {
        if (_selectedListingId < 0) return;
        
        var result = _auctionSystem.PurchaseItem(_selectedListingId, 1, "Player");
        
        if (Convert.ToBoolean(result["success"]))
        {
            GD.Print("AuctionHouseUI: 购买成功");
            RefreshListings();
        }
        else
        {
            GD.Print($"AuctionHouseUI: 购买失败 - {result["message"]}");
        }
    }
    
    private void OnCancelPressed()
    {
        if (_selectedListingId < 0) return;
        
        var result = _auctionSystem.CancelListing(_selectedListingId, 1);
        
        if (Convert.ToBoolean(result["success"]))
        {
            GD.Print("AuctionHouseUI: 取消成功");
            RefreshListings();
        }
    }
    
    private void SwitchTab(string tab)
    {
        _currentTab = tab;
        
        if (tab == "browse")
        {
            _tabContainer.CurrentTab = 0;
            RefreshListings();
        }
        else if (tab == "my_listings")
        {
            _tabContainer.CurrentTab = 1;
        }
        else if (tab == "create")
        {
            _tabContainer.CurrentTab = 2;
        }
    }
    
    private void UpdateStatistics()
    {
        var stats = _auctionSystem.GetStatistics();
        
        _statsLabel.Text = $"活跃挂售: {stats["activeListings"]} | 总挂售次数: {stats["totalListings"]} | 总成交量: {stats["totalSales"]}";
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
        }
    }
}
