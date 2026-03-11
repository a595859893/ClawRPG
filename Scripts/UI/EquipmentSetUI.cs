using Godot;
using System;
using System.Collections.Generic;
using Game.EquipmentSetDataSpace;

namespace Game
{
    /// <summary>
    /// 装备套装UI
    /// </summary>
    public class EquipmentSetUI : Control
    {
        private VBoxContainer _mainContainer;
        private ScrollContainer _scrollContainer;
        private GridContainer _setGrid;
        
        // 筛选按钮
        private HBoxContainer _filterContainer;
        private Button _btnAll;
        private Button _btnWeapon;
        private Button _btnArmor;
        private Button _btnAccessory;
        
        // 统计面板
        private Label _statsLabel;
        
        // 详情面板
        private Panel _detailPanel;
        private Label _detailName;
        private Label _detailRarity;
        private Label _detailDescription;
        private Label _detailProgress;
        private VBoxContainer _itemListContainer;
        private VBoxContainer _bonusListContainer;
        
        private SetType? _currentFilter = null;
        private EquipmentSet _selectedSet = null;

        public override void _Ready()
        {
            _CreateUI();
            _ConnectSignals();
            RefreshSets();
        }

        private void _CreateUI()
        {
            // 主容器
            _mainContainer = new VBoxContainer();
            _mainContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
            _mainContainer.AddThemeConstantOverride("separation", 10);
            AddChild(_mainContainer);

            // 标题
            var titleLabel = new Label();
            titleLabel.Text = "  装备套装";
            titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _mainContainer.AddChild(titleLabel);

            // 筛选按钮
            _filterContainer = new HBoxContainer();
            _filterContainer.AddThemeConstantOverride("separation", 10);
            _mainContainer.AddChild(_filterContainer);

            _btnAll = _CreateFilterButton("全部", null);
            _btnWeapon = _CreateFilterButton("武器", SetType.Weapon);
            _btnArmor = _CreateFilterButton("护甲", SetType.Armor);
            _btnAccessory = _CreateFilterButton("饰品", SetType.Accessory);

            // 统计标签
            _statsLabel = new Label();
            _statsLabel.Text = "收集统计: ";
            _mainContainer.AddChild(_statsLabel);

            // 滚动容器
            _scrollContainer = new ScrollContainer();
            _scrollContainer.SetHExpand(true);
            _scrollContainer.SetVExpand(true);
            _mainContainer.AddChild(_scrollContainer);

            // 网格容器
            _setGrid = new GridContainer();
            _setGrid.Columns = 2;
            _setGrid.AddThemeConstantOverride("h_separation", 10);
            _setGrid.AddThemeConstantOverride("v_separation", 10);
            _scrollContainer.AddChild(_setGrid);

            // 详情面板
            _CreateDetailPanel();
            
            // 默认隐藏
            Visible = false; 
        }

        private Button _CreateFilterButton(string text, SetType? filter)
        {
            var btn = new Button();
            btn.Text = text;
            btn.Pressed += () => _OnFilterPressed(filter);
            _filterContainer.AddChild(btn);
            return btn;
        }

        private void _CreateDetailPanel()
        {
            _detailPanel = new Panel();
            _detailPanel.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
            _detailPanel.Visible = false; 
            AddChild(_detailPanel);

            var detailContainer = new VBoxContainer();
            detailContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
            detailContainer.AddThemeConstantOverride("separation", 10);
            detailContainer.AddThemeConstantOverride("margin_left", 20);
            detailContainer.AddThemeConstantOverride("margin_top", 20);
            detailContainer.AddThemeConstantOverride("margin_right", -20);
            detailContainer.AddThemeConstantOverride("margin_bottom", -20);
            _detailPanel.AddChild(detailContainer);

            // 关闭按钮
            var closeBtn = new Button();
            closeBtn.Text = "关闭";
            closeBtn.Align = Button.AlignMode.Center;
            closeBtn.Pressed += () => _detailPanel.Visible = false; 
            
            var headerContainer = new HBoxContainer();
            headerContainer.AddChild(closeBtn);
            headerContainer.AddChild(new Control() { HExpand = true }); // Spacer
            detailContainer.AddChild(headerContainer);

            // 套装名称
            _detailName = new Label();
            _detailName.AddThemeFontSizeOverride("font_size", 20);
            detailContainer.AddChild(_detailName);

            // 稀有度
            _detailRarity = new Label();
            _detailRarity.AddThemeFontSizeOverride("font_size", 16);
            detailContainer.AddChild(_detailRarity);

            // 描述
            _detailDescription = new Label();
            _detailDescription.AutowrapMode = TextServer.AutowrapMode.Word;
            detailContainer.AddChild(_detailDescription);

            // 进度
            _detailProgress = new Label();
            _detailProgress.Text = "收集进度: ";
            detailContainer.AddChild(_detailProgress);

            // 套装物品标题
            var itemTitle = new Label();
            itemTitle.Text = "套装物品:";
            itemTitle.AddThemeFontSizeOverride("font_size", 16);
            detailContainer.AddChild(itemTitle);

            // 物品列表
            _itemListContainer = new VBoxContainer();
            detailContainer.AddChild(_itemListContainer);

            // 套装效果标题
            var bonusTitle = new Label();
            bonusTitle.Text = "套装效果:";
            bonusTitle.AddThemeFontSizeOverride("font_size", 16);
            detailContainer.AddChild(bonusTitle);

            // 效果列表
            _bonusListContainer = new VBoxContainer();
            detailContainer.AddChild(_bonusListContainer);
        }

        private void _ConnectSignals()
        {
            // 按键处理
        }

        public void RefreshSets()
        {
            // 清除现有
            foreach (var child in _setGrid.GetChildren())
            {
                child.QueueFree();
            }

            var db = EquipmentSetDatabase.Instance;
            List<EquipmentSet> sets;

            if (_currentFilter.HasValue)
            {
                sets = db.GetSetsByType(_currentFilter.Value);
            }
            else
            {
                sets = db.GetAllSets();
            }

            // 统计
            var stats = EquipmentSetSystem.Instance.GetStatistics();
            _statsLabel.Text = $"收集统计: 套装 {stats.CompletedSets}/{stats.TotalSets}";

            foreach (var set in sets)
            {
                var panel = _CreateSetPanel(set);
                _setGrid.AddChild(panel);
            }
        }

        private Control _CreateSetPanel(EquipmentSet set)
        {
            var panel = new PanelContainer();
            panel.CustomMinimumSize = new Vector2(350, 120);

            var container = new VBoxContainer();
            container.AddThemeConstantOverride("separation", 5);
            panel.AddChild(container);

            // 名称和稀有度
            var nameLabel = new Label();
            nameLabel.Text = set.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            
            // 稀有度颜色
            Color rarityColor = _GetRarityColor(set.Rarity);
            nameLabel.Modulate = rarityColor;
            container.AddChild(nameLabel);

            // 描述
            var descLabel = new Label();
            descLabel.Text = set.Description;
            descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            descLabel.AddThemeFontSizeOverride("font_size", 14);
            container.AddChild(descLabel);

            // 进度
            int owned = EquipmentSetSystem.Instance.GetSetPieceCount(set.SetId);
            int total = set.Items.Count;
            var progressLabel = new Label();
            progressLabel.Text = $"已收集: {owned}/{total} 件";
            
            // 完成颜色
            if (owned >= total)
            {
                progressLabel.Modulate = new Color(0.2f, 0.8f, 0.2f); // 绿色
            }
            else if (owned > 0)
            {
                progressLabel.Modulate = new Color(1f, 0.8f, 0.2f); // 黄色
            }
            container.AddChild(progressLabel);

            // 进度条
            var progressBar = new ProgressBar();
            progressBar.MaxValue = total;
            progressBar.Value = owned;
            progressBar.CustomMinimumSize = new Vector2(0, 10);
            container.AddChild(progressBar);

            // 查看按钮
            var viewBtn = new Button();
            viewBtn.Text = "查看详情";
            viewBtn.Pressed += () => _ShowSetDetail(set);
            container.AddChild(viewBtn);

            return panel;
        }

        private void _ShowSetDetail(EquipmentSet set)
        {
            _selectedSet = set;
            _detailPanel.Visible = true;

            _detailName.Text = set.Name;
            _detailName.Modulate = _GetRarityColor(set.Rarity);

            _detailRarity.Text = "稀有度: " + _GetRarityName(set.Rarity);
            _detailRarity.Modulate = _GetRarityColor(set.Rarity);

            _detailDescription.Text = set.Description;

            int owned = EquipmentSetSystem.Instance.GetSetPieceCount(set.SetId);
            int total = set.Items.Count;
            _detailProgress.Text = $"收集进度: {owned}/{total}";

            // 物品列表
            foreach (var child in _itemListContainer.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var item in set.Items)
            {
                var itemLabel = new Label();
                bool hasItem = EquipmentSetSystem.Instance.HasSetItem(set.SetId, item.ItemId);
                
                if (hasItem)
                {
                    itemLabel.Text = $"✓ {item.Name}";
                    itemLabel.Modulate = new Color(0.2f, 0.8f, 0.2f);
                }
                else
                {
                    itemLabel.Text = $"✗ {item.Name}";
                    itemLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
                }
                
                _itemListContainer.AddChild(itemLabel);
            }

            // 效果列表
            foreach (var child in _bonusListContainer.GetChildren())
            {
                child.QueueFree();
            }

            foreach (var bonus in set.Bonuses)
            {
                var bonusLabel = new Label();
                bool isActive = owned >= bonus.PieceCount;
                
                if (isActive)
                {
                    bonusLabel.Text = $"✓ {bonus.PieceCount}件: {bonus.Description}";
                    bonusLabel.Modulate = new Color(0.2f, 0.8f, 0.2f);
                }
                else
                {
                    bonusLabel.Text = $"✗ {bonus.PieceCount}件: {bonus.Description}";
                    bonusLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
                }
                
                _bonusListContainer.AddChild(bonusLabel);
            }
        }

        private void _OnFilterPressed(SetType? filter)
        {
            _currentFilter = filter;
            RefreshSets();
        }

        private Color _GetRarityColor(SetRarity rarity)
        {
            switch (rarity)
            {
                case SetRarity.Common:
                    return new Color(0.7f, 0.7f, 0.7f);
                case SetRarity.Uncommon:
                    return new Color(0.2f, 0.8f, 0.2f);
                case SetRarity.Rare:
                    return new Color(0.2f, 0.5f, 1f);
                case SetRarity.Epic:
                    return new Color(0.6f, 0.2f, 0.8f);
                case SetRarity.Legendary:
                    return new Color(1f, 0.6f, 0f);
                default:
                    return Colors.White;
            }
        }

        private string _GetRarityName(SetRarity rarity)
        {
            switch (rarity)
            {
                case SetRarity.Common: return "普通";
                case SetRarity.Uncommon: return "优秀";
                case SetRarity.Rare: return "稀有";
                case SetRarity.Epic: return "史诗";
                case SetRarity.Legendary: return "传说";
                default: return "未知";
            }
        }

        public void Toggle()
        {
            Visible = !Visible;
            if (Visible)
            {
                RefreshSets();
            }
        }
    }
}
