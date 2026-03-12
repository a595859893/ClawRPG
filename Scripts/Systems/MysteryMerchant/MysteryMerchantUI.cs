using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 神秘商店 UI
    /// </summary>
    public partial class MysteryMerchantUI : Control
    {
        private Control _mainPanel;
        private VBoxContainer _mainContainer;
        private Label _titleLabel;
        private Button _closeButton;
        
        // 商店列表
        private ScrollContainer _merchantListContainer;
        private VBoxContainer _merchantList;
        
        // 当前商店详情
        private PanelContainer _merchantDetailPanel;
        private Label _merchantNameLabel;
        private Label _merchantDescLabel;
        private Label _merchantTimerLabel;
        private ScrollContainer _itemListContainer;
        private VBoxContainer _itemList;
        
        // 统计面板
        private PanelContainer _statsPanel;
        private Label _statsLabel;
        
        // 刷新按钮
        private Button _refreshButton;
        
        // 当前选中的商店
        private string _selectedMerchantId = null;
        
        // 更新计时器
        private float _updateTimer = 0f;
        
        // 显示状态
        private bool _isVisible = false;
        
        public override void _Ready()
        {
            SetupUI();
            Hide();
        }
        
        public override void _Process(float delta)
        {
            if (!_isVisible) return;
            
            _updateTimer += delta;
            if (_updateTimer >= 1f)
            {
                UpdateMerchantList();
                UpdateMerchantTimer();
                _updateTimer = 0f;
            }
        }
        
        private void SetupUI()
        {
            // 主面板
            _mainPanel = new Control();
            _mainPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            AddChild(_mainPanel);
            
            // 背景
            var bg = new ColorRect
            {
                Color = new Color(0, 0, 0, 0.7f),
                SetAnchorsPreset(Control.LayoutPreset.FullRect)
            };
            _mainPanel.AddChild(bg);
            
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
            _mainContainer.CustomMinimumSize = new Vector2(900, 600);
            _mainContainer.Position = new Vector2(-450, -300);
            _mainPanel.AddChild(_mainContainer);
            
            // 标题栏
            var titleBar = new HBoxContainer();
            _mainContainer.AddChild(titleBar);
            
            _titleLabel = new Label
            {
                Text = "神秘商店",
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            titleBar.AddChild(_titleLabel);
            
            titleBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
            
            _closeButton = new Button
            {
                Text = "X",
                CustomMinimumSize = new Vector2(40, 40)
            };
            _closeButton.Pressed += () => ToggleVisibility();
            titleBar.AddChild(_closeButton);
            
            // 内容区域
            var contentArea = new HBoxContainer();
            contentArea.SetVExpandMode(Control.ContainerSizingMode.ExpandFill);
            _mainContainer.AddChild(contentArea);
            
            // 左侧：商店列表
            var leftPanel = new PanelContainer();
            leftPanel.CustomMinimumSize = new Vector2(250, 0);
            contentArea.AddChild(leftPanel);
            
            _merchantListContainer = new ScrollContainer();
            leftPanel.AddChild(_merchantListContainer);
            
            _merchantList = new VBoxContainer();
            _merchantListContainer.AddChild(_merchantList);
            
            // 中间：商店详情
            _merchantDetailPanel = new PanelContainer();
            _merchantDetailPanel.SetHExpandMode(Control.ContainerSizingMode.ExpandFill);
            contentArea.AddChild(_merchantDetailPanel);
            
            var detailVBox = new VBoxContainer();
            _merchantDetailPanel.AddChild(detailVBox);
            
            _merchantNameLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _merchantNameLabel.AddThemeFontSizeOverride("font_size", 20);
            detailVBox.AddChild(_merchantNameLabel);
            
            _merchantDescLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            detailVBox.AddChild(_merchantDescLabel);
            
            _merchantTimerLabel = new Label
            {
                HorizontalAlignment = HorizontalAlignment.Center,
                Text = "剩余时间: --"
            };
            detailVBox.AddChild(_merchantTimerLabel);
            
            _itemListContainer = new ScrollContainer();
            _itemListContainer.SetVExpandMode(Control.ContainerSizingMode.ExpandFill);
            detailVBox.AddChild(_itemListContainer);
            
            _itemList = new VBoxContainer();
            _itemListContainer.AddChild(_itemList);
            
            // 刷新按钮
            _refreshButton = new Button
            {
                Text = "刷新商店 (50 金币)",
                CustomMinimumSize = new Vector2(0, 50)
            };
            _refreshButton.Pressed += OnRefreshPressed;
            detailVBox.AddChild(_refreshButton);
            
            // 右侧：统计面板
            _statsPanel = new PanelContainer();
            _statsPanel.CustomMinimumSize = new Vector2(200, 0);
            contentArea.AddChild(_statsPanel);
            
            _statsLabel = new Label
            {
                Text = "统计信息\n\n访问次数: 0\n购买次数: 0\n消费总额: 0\n发现隐藏: 0",
                VerticalAlignment = VerticalAlignment.Top
            };
            _statsPanel.AddChild(_statsLabel);
        }
        
        // 切换显示
        public void ToggleVisibility()
        {
            if (_isVisible)
            {
                Hide();
                _isVisible = false;
            }
            else
            {
                Show();
                _isVisible = true;
                RefreshAll();
            }
        }
        
        // 刷新所有
        private void RefreshAll()
        {
            UpdateMerchantList();
            UpdateStats();
        }
        
        // 更新商店列表
        private void UpdateMerchantList()
        {
            // 清除旧列表
            foreach (Node child in _itemList.GetChildren())
            {
                child.QueueFree();
            }
            
            if (MysteryMerchantSystem.Instance == null) return;
            
            var merchants = MysteryMerchantSystem.Instance.GetActiveMerchants();
            
            // 更新左侧商店列表
            foreach (Node child in _itemList.GetChildren())
            {
                child.QueueFree();
            }
            
            foreach (var merchant in merchants)
            {
                var btn = new Button
                {
                    Text = $"{merchant.MerchantName} ({merchant.Items.Count}件商品)",
                    CustomMinimumSize = new Vector2(0, 50),
                    TextHorizontalAlignment = HorizontalAlignment.Left
                };
                btn.Pressed += () => SelectMerchant(merchant.MerchantId);
                _merchantList.AddChild(btn);
            }
        }
        
        // 选择商店
        private void SelectMerchant(string merchantId)
        {
            _selectedMerchantId = merchantId;
            
            if (MysteryMerchantSystem.Instance == null) return;
            
            var merchant = MysteryMerchantSystem.Instance.GetMerchantById(merchantId);
            if (merchant == null) return;
            
            // 访问商店
            MysteryMerchantSystem.Instance.VisitMerchant(merchantId);
            
            // 更新详情面板
            _merchantNameLabel.Text = merchant.MerchantName;
            _merchantDescLabel.Text = merchant.Description;
            
            // 更新刷新按钮
            _refreshButton.Text = $"刷新商店 ({merchant.RefreshCost} 金币)";
            
            // 更新商品列表
            UpdateItemList(merchant);
        }
        
        // 更新商品列表
        private void UpdateItemList(MysteryMerchant merchant)
        {
            foreach (Node child in _itemList.GetChildren())
            {
                child.QueueFree();
            }
            
            foreach (var item in merchant.Items)
            {
                var itemPanel = new PanelContainer();
                itemPanel.CustomMinimumSize = new Vector2(0, 80);
                _itemList.AddChild(itemPanel);
                
                var itemHBox = new HBoxContainer();
                itemPanel.AddChild(itemHBox);
                
                // 商品名称和稀有度颜色
                var rarityColor = Color.FromHtml(MysteryMerchantDatabase.GetRarityColor(item.Rarity));
                var nameLabel = new Label
                {
                    Text = item.ItemName,
                    CustomMinimumSize = new Vector2(150, 0)
                };
                nameLabel.Modulate = rarityColor;
                itemHBox.AddChild(nameLabel);
                
                // 描述
                var descLabel = new Label
                {
                    Text = item.Description,
                    SizeFlagsHorizontal = Control.SizeFlags.Expand,
                    AutowrapMode = TextServer.AutowrapMode.Word
                };
                itemHBox.AddChild(descLabel);
                
                // 价格
                var priceLabel = new Label
                {
                    Text = $"{item.Price} 金币",
                    CustomMinimumSize = new Vector2(80, 0)
                };
                itemHBox.AddChild(priceLabel);
                
                // 库存
                var stockLabel = new Label
                {
                    Text = item.Stock > 0 ? $"x{item.Stock}" : "售罄",
                    CustomMinimumSize = new Vector2(50, 0)
                };
                if (item.Stock <= 0)
                    stockLabel.Modulate = Colors.Gray;
                itemHBox.AddChild(stockLabel);
                
                // 购买按钮
                var buyBtn = new Button
                {
                    Text = "购买",
                    CustomMinimumSize = new Vector2(60, 0),
                    Disabled = item.Stock <= 0
                };
                int itemIndex = merchant.Items.IndexOf(item);
                buyBtn.Pressed += () => OnBuyItem(merchant.MerchantId, itemIndex);
                itemHBox.AddChild(buyBtn);
                
                // 隐藏商品标记
                if (item.IsSecret)
                {
                    var secretLabel = new Label
                    {
                        Text = " [隐藏]",
                        Modulate = Colors.Gold
                    };
                    itemHBox.AddChild(secretLabel);
                }
                
                // 限时标记
                if (item.IsLimited)
                {
                    var limitedLabel = new Label
                    {
                        Text = " [限时]",
                        Modulate = Colors.Orange
                    };
                    itemHBox.AddChild(limitedLabel);
                }
            }
        }
        
        // 更新商店计时器
        private void UpdateMerchantTimer()
        {
            if (_selectedMerchantId == null || MysteryMerchantSystem.Instance == null) return;
            
            var merchant = MysteryMerchantSystem.Instance.GetMerchantById(_selectedMerchantId);
            if (merchant == null) return;
            
            int seconds = (int)Math.Max(0, merchant.RemainingTime);
            int minutes = seconds / 60;
            int secs = seconds % 60;
            _merchantTimerLabel.Text = $"剩余时间: {minutes}:{secs:D2}";
            
            // 商店过期
            if (merchant.RemainingTime <= 0)
            {
                _selectedMerchantId = null;
                _merchantNameLabel.Text = "商店已关闭";
                _merchantDescLabel.Text = "";
                _merchantTimerLabel.Text = "";
            }
        }
        
        // 购买商品
        private void OnBuyItem(string merchantId, int itemIndex)
        {
            if (MysteryMerchantSystem.Instance == null) return;
            
            bool success = MysteryMerchantSystem.Instance.PurchaseItem(merchantId, itemIndex);
            
            if (success)
            {
                // 刷新显示
                var merchant = MysteryMerchantSystem.Instance.GetMerchantById(merchantId);
                if (merchant != null)
                {
                    UpdateItemList(merchant);
                }
                UpdateStats();
                
                // 播放音效或显示反馈
                GD.Print("购买成功！");
            }
            else
            {
                GD.Print("购买失败！");
            }
        }
        
        // 刷新商店
        private void OnRefreshPressed()
        {
            if (_selectedMerchantId == null || MysteryMerchantSystem.Instance == null) return;
            
            bool success = MysteryMerchantSystem.Instance.RefreshMerchant(_selectedMerchantId);
            
            if (success)
            {
                var merchant = MysteryMerchantSystem.Instance.GetMerchantById(_selectedMerchantId);
                if (merchant != null)
                {
                    UpdateItemList(merchant);
                }
                UpdateStats();
                GD.Print("刷新成功！");
            }
            else
            {
                GD.Print("刷新失败，金币不足！");
            }
        }
        
        // 更新统计
        private void UpdateStats()
        {
            if (MysteryMerchantSystem.Instance == null) return;
            
            var stats = MysteryMerchantSystem.Instance.GetStatistics();
            
            _statsLabel.Text = $"统计信息\n\n" +
                $"访问次数: {stats["totalVisits"]}\n" +
                $"购买次数: {stats["totalPurchases"]}\n" +
                $"消费总额: {stats["totalGoldSpent"]} 金币\n" +
                $"发现隐藏: {stats["secretItemsFound"]}\n" +
                $"幸运购买: {stats["luckyPurchases"]}\n" +
                $"解锁商店: {stats["unlockedMerchantTypes"]}";
        }
        
        // 输入处理
        public override void _Input(InputEvent evt)
        {
            if (evt is InputEventKey keyEvent && keyEvent.Pressed)
            {
                if (keyEvent.Keycode == Key.M || (keyEvent.Keycode == Key.Shift && keyEvent.Keycode == Key.M))
                {
                    ToggleVisibility();
                }
                else if (keyEvent.Keycode == Key.Escape && _isVisible)
                {
                    ToggleVisibility();
                }
            }
        }
    }
}
