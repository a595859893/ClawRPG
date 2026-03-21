using Godot;
using System;
using System.Collections.Generic;
using GameSystems;

namespace UI
{
    /// <summary>
    /// 商店界面
    /// </summary>
    public partial class ShopUI : Control
    {
        [Export] public bool VisibleOnStart => false;

        // UI 组件
        private VBoxContainer _mainContainer;
        private HBoxContainer _shopListContainer;
        private VBoxContainer _itemListContainer;
        private VBoxContainer _itemDetailContainer;
        private Label _shopNameLabel;
        private Label _goldLabel;
        private Label _shopDescLabel;
        private Button _closeButton;
        private Button _refreshButton;
        private Button _sellTabButton;
        private Button _buyTabButton;

        // 数据
        private ShopData _currentShop;
        private bool _isBuyMode = true;
        private ShopItem _selectedItem;
        private ItemData _selectedItemData;

        // 预制体
        private PackedScene _shopItemRowScene;

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
            _mainContainer.CustomMinimumSize = new Vector2(900, 600);
            AddChild(_mainContainer);

            // 标题栏
            var titleBar = new HBoxContainer();
            _mainContainer.AddChild(titleBar);

            _shopNameLabel = new Label();
            _shopNameLabel.Text = "商店";
            _shopNameLabel.AddThemeFontSizeOverride("font_size", 24);
            titleBar.AddChild(_shopNameLabel);

            titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            _goldLabel = new Label();
            _goldLabel.Text = "金币: 0";
            _goldLabel.AddThemeFontSizeOverride("font_size", 20);
            titleBar.AddChild(_goldLabel);

            _closeButton = new Button();
            _closeButton.Text = "✕";
            _closeButton.TooltipText = "关闭商店 (ESC)";
            _closeButton.Pressed += OnClosePressed;
            titleBar.AddChild(_closeButton);

            // 标签页切换
            var tabContainer = new HBoxContainer();
            _mainContainer.AddChild(tabContainer);

            _buyTabButton = new Button();
            _buyTabButton.Text = "购买";
            _buyTabButton.ToggleMode = true;
            _buyTabButton.ButtonPressed = true;
            _buyTabButton.Pressed += () => SetMode(true);
            tabContainer.AddChild(_buyTabButton);

            _sellTabButton = new Button();
            _sellTabButton.Text = "出售";
            _sellTabButton.ToggleMode = true;
            _sellTabButton.Pressed += () => SetMode(false);
            tabContainer.AddChild(_sellTabButton);

            tabContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

            _refreshButton = new Button();
            _refreshButton.Text = "刷新商店 (100金币)";
            _refreshButton.Pressed += OnRefreshPressed;
            tabContainer.AddChild(_refreshButton);

            // 商店描述
            _shopDescLabel = new Label();
            _shopDescLabel.Text = "";
            _mainContainer.AddChild(_shopDescLabel);

            // 内容区域
            _shopListContainer = new HBoxContainer();
            _shopListContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
            _mainContainer.AddChild(_shopListContainer);

            // 商店列表 (左侧)
            var shopListPanel = new ScrollContainer();
            shopListPanel.CustomMinimumSize = new Vector2(200, 0);
            _shopListContainer.AddChild(shopListPanel);

            var shopListVBox = new VBoxContainer();
            shopListVBox.SizeFlagsVertical = Control.SizeFlags.Expand;
            shopListPanel.AddChild(shopListVBox);

            var shopListLabel = new Label();
            shopListLabel.Text = "商店列表";
            shopListVBox.AddChild(shopListLabel);

            foreach (var shop in ShopSystem.Instance.GetAccessibleShops())
            {
                var shopButton = new Button();
                shopButton.Text = shop.ShopName;
                shopButton.Pressed += () => SelectShop(shop);
                shopListVBox.AddChild(shopButton);
            }

            // 物品列表 (中间)
            _itemListContainer = new VBoxContainer();
            _itemListContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            _shopListContainer.AddChild(_itemListContainer);

            var itemListLabel = new Label();
            itemListLabel.Text = "物品列表";
            _itemListContainer.AddChild(itemListLabel);

            var itemScroll = new ScrollContainer();
            itemScroll.SizeFlagsVertical = Control.SizeFlags.Expand;
            _itemListContainer.AddChild(itemScroll);

            _itemDetailContainer = new VBoxContainer();
            _itemDetailContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            itemScroll.AddChild(_itemDetailContainer);

            // 物品详情 (右侧)
            var detailPanel = new VBoxContainer();
            detailPanel.CustomMinimumSize = new Vector2(250, 0);
            _shopListContainer.AddChild(detailPanel);

            var detailLabel = new Label();
            detailLabel.Text = "物品详情";
            detailPanel.AddChild(detailLabel);

            _itemDetailContainer = new VBoxContainer();
            _itemDetailContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
            detailPanel.AddChild(_itemDetailContainer);
        }

        private void SetupSignals()
        {
            // 连接商店信号
            if (ShopSystem.Instance != null)
            {
                ShopSystem.Instance.PurchaseCompleted += OnPurchaseCompleted;
                ShopSystem.Instance.ItemSold += OnItemSold;
                ShopSystem.Instance.ShopRefreshed += OnShopRefreshed;
            }

            // 连接玩家金币变化信号
            var inventory = InventoryManager.Instance;
            if (inventory != null)
            {
                inventory.GoldChanged += UpdateGoldDisplay;
            }
        }

        private void SelectShop(ShopData shop)
        {
            _currentShop = shop;
            _shopNameLabel.Text = shop.ShopName;
            _shopDescLabel.Text = shop.Description;
            
            if (shop.RefreshCost > 0)
                _refreshButton.Text = $"刷新商店 ({shop.RefreshCost}金币)";
            
            RefreshItemList();
        }

        private void RefreshItemList()
        {
            // 清除现有物品
            foreach (var child in _itemDetailContainer.GetChildren())
            {
                child.QueueFree();
            }

            if (_currentShop == null)
                return;

            if (_isBuyMode)
            {
                // 显示商店物品
                foreach (var item in _currentShop.Items)
                {
                    var itemRow = CreateItemRow(item);
                    _itemDetailContainer.AddChild(itemRow);
                }
            }
            else
            {
                // 显示玩家背包可出售物品
                var inventory = InventoryManager.Instance;
                if (inventory != null)
                {
                    var items = inventory.GetAllItems();
                    foreach (var kvp in items)
                    {
                        var itemRow = CreateSellItemRow(kvp.Key, kvp.Value);
                        _itemDetailContainer.AddChild(itemRow);
                    }
                }
            }
        }

        private Control CreateItemRow(ShopItem item)
        {
            var itemData = ItemDatabase.Instance?.GetItem(item.ItemId);
            if (itemData == null)
                return new Control();

            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 10);

            // 物品图标占位
            var iconPlaceholder = new ColorRect();
            iconPlaceholder.CustomMinimumSize = new Vector2(32, 32);
            iconPlaceholder.Color = new Color(0.3f, 0.3f, 0.3f);
            container.AddChild(iconPlaceholder);

            // 物品信息
            var infoContainer = new VBoxContainer();
            infoContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand;

            var nameLabel = new Label();
            nameLabel.Text = itemData.Name;
            infoContainer.AddChild(nameLabel);

            var priceLabel = new Label();
            int displayPrice = (int)(item.Price * item.Discount);
            string priceText = $"{displayPrice} 金币";
            if (item.Discount < 1.0f)
                priceText += $" ({(int)(item.Discount * 100)}%)";
            priceLabel.Text = priceText;
            priceLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0));
            infoContainer.AddChild(priceLabel);

            var stockLabel = new Label();
            if (item.Stock < 0)
                stockLabel.Text = "库存: 无限";
            else if (item.DailyStock > 0)
                stockLabel.Text = $"库存: {item.Stock} (今日限够: {item.DailyStock})";
            else
                stockLabel.Text = $"库存: {item.Stock}";
            infoContainer.AddChild(stockLabel);

            container.AddChild(infoContainer);

            // 购买按钮
            var buyButton = new Button();
            buyButton.Text = "购买";
            buyButton.Pressed += () => OnBuyItem(item);
            container.AddChild(buyButton);

            return container;
        }

        private Control CreateSellItemRow(string itemId, int quantity)
        {
            var itemData = ItemDatabase.Instance?.GetItem(itemId);
            if (itemData == null)
                return new Control();

            var container = new HBoxContainer();
            container.AddThemeConstantOverride("separation", 10);

            // 物品图标占位
            var iconPlaceholder = new ColorRect();
            iconPlaceholder.CustomMinimumSize = new Vector2(32, 32);
            iconPlaceholder.Color = new Color(0.3f, 0.3f, 0.3f);
            container.AddChild(iconPlaceholder);

            // 物品信息
            var infoContainer = new VBoxContainer();
            infoContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand;

            var nameLabel = new Label();
            nameLabel.Text = $"{itemData.Name} x{quantity}";
            infoContainer.AddChild(nameLabel);

            int sellPrice = (int)(itemData.Value * 0.5f);
            var priceLabel = new Label();
            priceLabel.Text = $"出售价: {sellPrice} 金币/个";
            priceLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            infoContainer.AddChild(priceLabel);

            container.AddChild(infoContainer);

            // 出售按钮
            var sellButton = new Button();
            sellButton.Text = "出售";
            sellButton.Pressed += () => OnSellItem(itemId);
            container.AddChild(sellButton);

            return container;
        }

        private void SetMode(bool buyMode)
        {
            _isBuyMode = buyMode;
            _buyTabButton.ButtonPressed = buyMode;
            _sellTabButton.ButtonPressed = !buyMode;
            RefreshItemList();
        }

        private void OnBuyItem(ShopItem item)
        {
            if (_currentShop == null)
                return;

            ShopSystem.Instance.PurchaseItem(_currentShop.ShopId, item.ItemId, 1);
            RefreshItemList();
            UpdateGoldDisplay();
        }

        private void OnSellItem(string itemId)
        {
            ShopSystem.Instance.SellItem(itemId, 1);
            RefreshItemList();
            UpdateGoldDisplay();
        }

        private void OnRefreshPressed()
        {
            if (_currentShop == null)
                return;

            ShopSystem.Instance.RefreshShop(_currentShop.ShopId);
            RefreshItemList();
            UpdateGoldDisplay();
        }

        private void OnClosePressed()
        {
            Hide();
            
            // 恢复游戏
            var main = GetTree().GetFirstNodeInGroup("main");
            if (main != null)
            {
                main.SetProcessInput(true);
            }
        }

        private void OnPurchaseCompleted(string shopId, string itemId, int quantity, int totalPrice)
        {
            GD.Print($"[ShopUI] 购买完成: {itemId} x{quantity}");
        }

        private void OnItemSold(string itemId, int quantity, int totalPrice)
        {
            GD.Print($"[ShopUI] 出售完成: {itemId} x{quantity}, 获得 {totalPrice}");
        }

        private void OnShopRefreshed(string shopId)
        {
            GD.Print($"[ShopUI] 商店已刷新: {shopId}");
        }

        private void UpdateGoldDisplay()
        {
            var inventory = InventoryManager.Instance;
            if (inventory != null)
            {
                _goldLabel.Text = $"金币: {inventory.Gold}";
            }
        }

        public void Toggle()
        {
            if (Visible)
            {
                Hide();
            }
            else
            {
                Show();
                UpdateGoldDisplay();
                
                // 默认选择第一个可访问的商店
                var shops = ShopSystem.Instance.GetAccessibleShops();
                if (shops.Count > 0 && _currentShop == null)
                {
                    SelectShop(shops[0]);
                }
                else if (_currentShop != null)
                {
                    RefreshItemList();
                }
            }
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel") && Visible)
            {
                OnClosePressed();
                GetTree().SetInputAsHandled();
            }
        }
    }
}
