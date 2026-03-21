using Godot;
using System;
using System.Collections.Generic;

namespace UI
{
    /// <summary>
    /// 拍卖行界面
    /// </summary>
    public partial class AuctionHouseUI : Control
    {
        [Export] public bool VisibleOnStart => false;

        // UI 组件
        private VBoxContainer _mainContainer;
        private HBoxContainer _contentContainer;
        private VBoxContainer _listingsContainer;
        private VBoxContainer _detailContainer;
        private Label _titleLabel;
        private Label _goldLabel;
        private Label _searchLabel;
        private LineEdit _searchEdit;
        private Button _closeButton;
        private Button _myListingsButton;
        private Button _myBidsButton;
        private Button _allListingsButton;
        
        // 筛选按钮
        private HBoxContainer _filterContainer;
        private Button _filterAllButton;
        
        // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
        public event Action OnAuctionListingUpdatedUI;
        private Button _filterCommonButton;
        private Button _filterUncommonButton;
        private Button _filterRareButton;
        private Button _filterEpicButton;
        private Button _filterLegendaryButton;

        // 列表
        private VBoxContainer _auctionListContainer;
        
        // 详情
        private VBoxContainer _detailPanel;
        private Label _itemNameLabel;
        private Label _itemRarityLabel;
        private Label _currentBidLabel;
        private Label _buyNowLabel;
        private Label _sellerLabel;
        private Label _timeRemainingLabel;
        private Label _quantityLabel;
        private Button _bidButton;
        private Button _buyNowButton;
        
        // 统计
        private VBoxContainer _statsContainer;
        private Label _totalSalesLabel;
        private Label _totalPurchasesLabel;
        private Label _totalEarnedLabel;
        private Label _totalSpentLabel;

        // 数据
        private AuctionItem _selectedAuction;
        private string _currentTab = "all"; // all, my_listings, my_bids

        public override void _Ready()
        {
            SetupUI();
            SetupSignals();
            Hide();
        }

        private void SetupUI()
        {
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainContainer.CustomMinimumSize = new Vector2(1000, 650);
            _mainContainer.Position = new Vector2(-500, -325);
            AddChild(_mainContainer);

            // 标题栏
            var titleBar = new HBoxContainer();
            _mainContainer.AddChild(titleBar);

            _titleLabel = new Label();
            _titleLabel.Text = "🏛️ 拍卖行";
            _titleLabel.AddThemeFontSizeOverride("font_size", 28);
            titleBar.AddChild(_titleLabel);

            titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            _goldLabel = new Label();
            _goldLabel.Text = "💰 金币: 0";
            _goldLabel.AddThemeFontSizeOverride("font_size", 20);
            titleBar.AddChild(_goldLabel);

            _closeButton = new Button();
            _closeButton.Text = "✕ 关闭";
            _closeButton.Pressed += OnClosePressed;
            titleBar.AddChild(_closeButton);

            // 标签页按钮
            var tabContainer = new HBoxContainer();
            tabContainer.Alignment = BoxContainer.AlignmentMode.Center;
            _mainContainer.AddChild(tabContainer);

            _allListingsButton = new Button();
            _allListingsButton.Text = "📋 全部拍卖";
            _allListingsButton.Pressed += () => SwitchTab("all");
            tabContainer.AddChild(_allListingsButton);

            _myListingsButton = new Button();
            _myListingsButton.Text = "📦 我的寄售";
            _myListingsButton.Pressed += () => SwitchTab("my_listings");
            tabContainer.AddChild(_myListingsButton);

            _myBidsButton = new Button();
            _myBidsButton.Text = "💰 我的竞拍";
            _myBidsButton.Pressed += () => SwitchTab("my_bids");
            tabContainer.AddChild(_myBidsButton);

            // 搜索和筛选
            var searchContainer = new HBoxContainer();
            _mainContainer.AddChild(searchContainer);

            _searchLabel = new Label();
            _searchLabel.Text = "🔍 搜索: ";
            searchContainer.AddChild(_searchLabel);

            _searchEdit = new LineEdit();
            _searchEdit.CustomMinimumSize = new Vector2(200, 30);
            _searchEdit.TextChanged += OnSearchTextChanged;
            searchContainer.AddChild(_searchEdit);

            searchContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            // 筛选按钮
            _filterContainer = new HBoxContainer();
            _mainContainer.AddChild(_filterContainer);

            var filterLabel = new Label();
            filterLabel.Text = "筛选: ";
            _filterContainer.AddChild(filterLabel);

            _filterAllButton = new Button();
            _filterAllButton.Text = "全部";
            _filterAllButton.Pressed += () => OnFilterPressed(-1);
            _filterContainer.AddChild(_filterAllButton);

            _filterCommonButton = new Button();
            _filterCommonButton.Text = "普通";
            _filterCommonButton.Modulate = new Color(0.62f, 0.62f, 0.62f);
            _filterCommonButton.Pressed += () => OnFilterPressed(0);
            _filterContainer.AddChild(_filterCommonButton);

            _filterUncommonButton = new Button();
            _filterUncommonButton.Text = "优秀";
            _filterUncommonButton.Modulate = new Color(0.3f, 0.69f, 0.31f);
            _filterUncommonButton.Pressed += () => OnFilterPressed(1);
            _filterContainer.AddChild(_filterUncommonButton);

            _filterRareButton = new Button();
            _filterRareButton.Text = "稀有";
            _filterRareButton.Modulate = new Color(0.13f, 0.59f, 0.95f);
            _filterRareButton.Pressed += () => OnFilterPressed(2);
            _filterContainer.AddChild(_filterRareButton);

            _filterEpicButton = new Button();
            _filterEpicButton.Text = "史诗";
            _filterEpicButton.Modulate = new Color(0.61f, 0.15f, 0.69f);
            _filterEpicButton.Pressed += () => OnFilterPressed(3);
            _filterContainer.AddChild(_filterEpicButton);

            _filterLegendaryButton = new Button();
            _filterLegendaryButton.Text = "传说";
            _filterLegendaryButton.Modulate = new Color(1f, 0.6f, 0f);
            _filterLegendaryButton.Pressed += () => OnFilterPressed(4);
            _filterContainer.AddChild(_filterLegendaryButton);

            // 内容区域
            _contentContainer = new HBoxContainer();
            _contentContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
            _mainContainer.AddChild(_contentContainer);

            // 左侧列表
            _listingsContainer = new VBoxContainer();
            _listingsContainer.CustomMinimumSize = new Vector2(500, 0);
            _contentContainer.AddChild(_listingsContainer);

            var listTitle = new Label();
            listTitle.Text = "拍卖列表";
            listTitle.AddThemeFontSizeOverride("font_size", 18);
            _listingsContainer.AddChild(listTitle);

            _auctionListContainer = new VBoxContainer();
            _auctionListContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
            _listingsContainer.AddChild(_auctionListContainer);

            // 右侧详情和统计
            _detailContainer = new VBoxContainer();
            _contentContainer.AddChild(_detailContainer);

            // 详情面板
            _detailPanel = new VBoxContainer();
            _detailContainer.AddChild(_detailPanel);

            var detailTitle = new Label();
            detailTitle.Text = "物品详情";
            detailTitle.AddThemeFontSizeOverride("font_size", 18);
            _detailPanel.AddChild(detailTitle);

            _itemNameLabel = new Label();
            _itemNameLabel.Text = "选择物品查看详情";
            _itemNameLabel.AddThemeFontSizeOverride("font_size", 20);
            _detailPanel.AddChild(_itemNameLabel);

            _itemRarityLabel = new Label();
            _itemRarityLabel.Text = "";
            _detailPanel.AddChild(_itemRarityLabel);

            _quantityLabel = new Label();
            _quantityLabel.Text = "";
            _detailPanel.AddChild(_quantityLabel);

            _currentBidLabel = new Label();
            _currentBidLabel.Text = "";
            _detailPanel.AddChild(_currentBidLabel);

            _buyNowLabel = new Label();
            _buyNowLabel.Text = "";
            _detailPanel.AddChild(_buyNowLabel);

            _sellerLabel = new Label();
            _sellerLabel.Text = "";
            _detailPanel.AddChild(_sellerLabel);

            _timeRemainingLabel = new Label();
            _timeRemainingLabel.Text = "";
            _detailPanel.AddChild(_timeRemainingLabel);

            // 操作按钮
            var buttonContainer = new HBoxContainer();
            _detailPanel.AddChild(buttonContainer);

            _bidButton = new Button();
            _bidButton.Text = "💰 出价";
            _bidButton.CustomMinimumSize = new Vector2(120, 40);
            _bidButton.Pressed += OnBidPressed;
            _bidButton.Disabled = true;
            buttonContainer.AddChild(_bidButton);

            _buyNowButton = new Button();
            _buyNowButton.Text = "⚡ 一口价";
            _buyNowButton.CustomMinimumSize = new Vector2(120, 40);
            _buyNowButton.Pressed += OnBuyNowPressed;
            _buyNowButton.Disabled = true;
            buttonContainer.AddChild(_buyNowButton);

            // 统计面板
            _statsContainer = new VBoxContainer();
            _detailContainer.AddChild(_statsContainer);

            var statsTitle = new Label();
            statsTitle.Text = "我的拍卖统计";
            statsTitle.AddThemeFontSizeOverride("font_size", 18);
            _statsContainer.AddChild(statsTitle);

            _totalSalesLabel = new Label();
            _totalSalesLabel.Text = "出售次数: 0";
            _statsContainer.AddChild(_totalSalesLabel);

            _totalPurchasesLabel = new Label();
            _totalPurchasesLabel.Text = "购买次数: 0";
            _statsContainer.AddChild(_totalPurchasesLabel);

            _totalEarnedLabel = new Label();
            _totalEarnedLabel.Text = "总收入: 0";
            _statsContainer.AddChild(_totalEarnedLabel);

            _totalSpentLabel = new Label();
            _totalSpentLabel.Text = "总支出: 0";
            _statsContainer.AddChild(_totalSpentLabel);
        }

        private void SetupSignals()
        {

        }

        public void Open()
        {
            RefreshList();
            UpdateStats();
            UpdateGoldLabel();
        }

        private void RefreshList()
        {
            // 清空列表
            foreach (var child in _auctionListContainer.GetChildren())
            {
                child.QueueFree();
            }

            // 获取列表
            List<AuctionItem> auctions = null;
            switch (_currentTab)
            {
                case "my_listings":
                    auctions = AuctionHouseSystem.Instance.GetPlayerListings();
                    break;
                case "my_bids":
                    auctions = AuctionHouseSystem.Instance.GetPlayerBids();
                    break;
                default:
                    auctions = AuctionHouseSystem.Instance.GetFilteredListings();
                    break;
            }

            // 添加列表项
            foreach (var auction in auctions)
            {
                var item = CreateAuctionItemRow(auction);
                _auctionListContainer.AddChild(item);
            }

            if (auctions.Count == 0)
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "暂无拍卖物品";
                emptyLabel.Alignment = Label.AlignmentMode.Center;
                _auctionListContainer.AddChild(emptyLabel);
            }
        }

        private Control CreateAuctionItemRow(AuctionItem auction)
        {
            var container = new HBoxContainer();
            container.CustomMinimumSize = new Vector2(0, 50);

            // 稀有度颜色
            var rarityColor = GetRarityColor(auction.ItemRarity);

            // 物品名称
            var nameLabel = new Label();
            nameLabel.Text = $"{auction.ItemName} x{auction.Quantity}";
            nameLabel.Modulate = rarityColor;
            nameLabel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            container.AddChild(nameLabel);

            // 当前价格
            var priceLabel = new Label();
            priceLabel.Text = $"💰 {auction.CurrentBid}";
            container.AddChild(priceLabel);

            // 时间
            var timeLabel = new Label();
            timeLabel.Text = AuctionHouseSystem.Instance.FormatTimeRemaining(auction.EndTime);
            container.AddChild(timeLabel);

            // 点击事件
            var button = new Button();
            button.Text = "查看";
            button.Pressed += () => OnAuctionSelected(auction);
            container.AddChild(button);

            return container;
        }

        private void OnAuctionSelected(AuctionItem auction)
        {
            _selectedAuction = auction;

            _itemNameLabel.Text = auction.ItemName;
            _itemRarityLabel.Text = $"稀有度: {GetRarityName(auction.ItemRarity)}";
            _itemRarityLabel.Modulate = GetRarityColor(auction.ItemRarity);
            _quantityLabel.Text = $"数量: {auction.Quantity}";
            _currentBidLabel.Text = $"当前出价: {auction.CurrentBid} 💰";
            _buyNowLabel.Text = $"一口价: {auction.BuyNowPrice} 💰";
            _sellerLabel.Text = $"卖家: {auction.SellerName}";
            _timeRemainingLabel.Text = $"剩余时间: {AuctionHouseSystem.Instance.FormatTimeRemaining(auction.EndTime)}";

            _bidButton.Disabled = false; 
            _buyNowButton.Disabled = false; 
        }

        private void UpdateStats()
        {
            var stats = AuctionHouseSystem.Instance.GetPlayerAuctionStats();
            if (stats != null)
            {
                _totalSalesLabel.Text = $"出售次数: {stats.TotalSales}";
                _totalPurchasesLabel.Text = $"购买次数: {stats.TotalPurchases}";
                _totalEarnedLabel.Text = $"总收入: {stats.TotalEarned} 💰";
                _totalSpentLabel.Text = $"总支出: {stats.TotalSpent} 💰";
            }
        }

        private void UpdateGoldLabel()
        {
            _goldLabel.Text = $"💰 金币: {AuctionHouseSystem.Instance.GetPlayerGold()}";
        }

        private void SwitchTab(string tab)
        {
            _currentTab = tab;
            RefreshList();
        }

        private void OnSearchTextChanged(string text)
        {
            AuctionHouseSystem.Instance.SetSearchTerm(text);
            RefreshList();
        }

        private void OnFilterPressed(int rarity)
        {
            AuctionHouseSystem.Instance.SetRarityFilter(rarity);
            RefreshList();
        }

        private void OnBidPressed()
        {
            if (_selectedAuction == null) return;
            // 这里可以添加一个输入对话框来输入出价金额
            // 简化版本：直接在当前价格基础上 +10%
            int newBid = (int)(_selectedAuction.CurrentBid * 1.1f);
            if (newBid <= _selectedAuction.CurrentBid)
                newBid = _selectedAuction.CurrentBid + 100;
            
            if (AuctionHouseSystem.Instance.PlaceBid(_selectedAuction.Id, newBid))
            {
                GD.Print($"出价成功: {newBid}");
                RefreshList();
                UpdateGoldLabel();
                UpdateStats();
            }
        }

        private void OnBuyNowPressed()
        {
            if (_selectedAuction == null) return;
            
            if (AuctionHouseSystem.Instance.BuyNow(_selectedAuction.Id))
            {
                GD.Print($"一口价购买成功: {_selectedAuction.ItemName}");
                RefreshList();
                UpdateGoldLabel();
                UpdateStats();
            }
        }

        private void OnClosePressed()
        {
            Hide();
        }

        private Color GetRarityColor(int rarity)
        {
            switch (rarity)
            {
                case 0: return new Color(0.62f, 0.62f, 0.62f); // Common - Gray
                case 1: return new Color(0.3f, 0.69f, 0.31f);  // Uncommon - Green
                case 2: return new Color(0.13f, 0.59f, 0.95f); // Rare - Blue
                case 3: return new Color(0.61f, 0.15f, 0.69f); // Epic - Purple
                case 4: return new Color(1f, 0.6f, 0f);        // Legendary - Orange
                case 5: return new Color(0.96f, 0.26f, 0.21f); // Mythic - Red
                default: return Colors.White;
            }
        }

        private string GetRarityName(int rarity)
        {
            switch (rarity)
            {
                case 0: return "普通";
                case 1: return "优秀";
                case 2: return "稀有";
                case 3: return "史诗";
                case 4: return "传说";
                case 5: return "神话";
                default: return "未知";
            }
        }
    }
}
