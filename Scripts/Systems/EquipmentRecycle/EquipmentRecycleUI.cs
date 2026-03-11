using Godot;
using System;
using System.Collections.Generic;
using System.Text.Json;

namespace ClawRPG.Scripts.Systems.EquipmentRecycle
{
    /// <summary>
    /// 装备回收系统UI
    /// 按 R 键切换显示
    /// </summary>
    public partial class EquipmentRecycleUI : Control
    {
        private bool _isVisible = false;
        private List<Dictionary<string, object>> _selectedItems = new List<Dictionary<string, object>>();
        private int _currentPage = 0;
        private int _itemsPerPage = 10;

        // UI组件
        private PanelContainer _mainPanel;
        private VBoxContainer _mainVBox;
        private Label _titleLabel;
        private ScrollContainer _itemsScroll;
        private VBoxContainer _itemsContainer;
        private Label _goldLabel;
        private Label _materialsLabel;
        private Button _recycleButton;
        private Button _statsButton;
        private Button _closeButton;
        private Label _pageLabel;

        public override void _Ready()
        {
            Visible = false;
            CreateUI();
            LoadData();
        }

        private void CreateUI()
        {
            // 主面板
            _mainPanel = new PanelContainer();
            _mainPanel.CustomMinimumSize = new Vector2(500, 400);
            _mainPanel.AnchorLeft = 0.5f;
            _mainPanel.AnchorTop = 0.5f;
            _mainPanel.AnchorRight = 0.5f;
            _mainPanel.AnchorBottom = 0.5f;
            _mainPanel.OffsetLeft = -250;
            _mainPanel.OffsetTop = -200;
            _mainPanel.OffsetRight = 250;
            _mainPanel.OffsetBottom = 200;
            AddChild(_mainPanel);

            // 主VBox
            _mainVBox = new VBoxContainer();
            _mainPanel.AddChild(_mainVBox);

            // 标题
            _titleLabel = new Label();
            _titleLabel.Text = "装备回收系统";
            _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _mainVBox.AddChild(_titleLabel);

            // 物品列表滚动
            _itemsScroll = new ScrollContainer();
            _itemsScroll.CustomMinimumSize = new Vector2(480, 200);
            _mainVBox.AddChild(_itemsScroll);

            _itemsContainer = new VBoxContainer();
            _itemsContainer.CustomMinimumSize = new Vector2(460, 0);
            _itemsScroll.AddChild(_itemsContainer);

            // 预览面板
            var previewPanel = new VBoxContainer();
            _mainVBox.AddChild(previewPanel);

            _goldLabel = new Label();
            _goldLabel.Text = "回收获得: 0 金币";
            previewPanel.AddChild(_goldLabel);

            _materialsLabel = new Label();
            _materialsLabel.Text = "材料: 无";
            previewPanel.AddChild(_materialsLabel);

            // 按钮容器
            var buttonContainer = new HBoxContainer();
            _mainVBox.AddChild(buttonContainer);

            _recycleButton = new Button();
            _recycleButton.Text = "回收选中";
            _recycleButton.CustomMinimumSize = new Vector2(120, 40);
            _recycleButton.Pressed += OnRecyclePressed;
            buttonContainer.AddChild(_recycleButton);

            _statsButton = new Button();
            _statsButton.Text = "查看统计";
            _statsButton.CustomMinimumSize = new Vector2(120, 40);
            _statsButton.Pressed += OnStatsPressed;
            buttonContainer.AddChild(_statsButton);

            _closeButton = new Button();
            _closeButton.Text = "关闭";
            _closeButton.CustomMinimumSize = new Vector2(80, 40);
            _closeButton.Pressed += OnClosePressed;
            buttonContainer.AddChild(_closeButton);

            // 页码标签
            _pageLabel = new Label();
            _pageLabel.Text = "第 1/1 页";
            _pageLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _mainVBox.AddChild(_pageLabel);
        }

        private void LoadData()
        {
            EquipmentRecycleSystem.Instance?.LoadStats();
        }

        public override void _Input(InputEvent @event)
        {
            if (@event.IsActionPressed("ui_cancel"))
            {
                HideUI();
                GetTree().SetInputAsHandled();
            }
        }

        public override void _Process(double delta)
        {
            if (_isVisible)
            {
                // 检查关闭
                if (Input.IsActionJustPressed("ui_cancel"))
                {
                    HideUI();
                }
            }
        }

        public void ShowUI()
        {
            Visible = true;
            _isVisible = true;
            RefreshItems();
            PlayAppearAnimation();
        }

        public void HideUI()
        {
            _isVisible = false;
            Visible = false;
        }

        public void ToggleUI()
        {
            if (_isVisible)
            {
                HideUI();
            }
            else
            {
                ShowUI();
            }
        }

        private void RefreshItems()
        {
            // 获取可回收的装备
            var recyclables = GetRecyclableItems();

            // 分页
            int startIdx = _currentPage * _itemsPerPage;
            int endIdx = Math.Min(startIdx + _itemsPerPage, recyclables.Count);

            // 清空
            foreach (Node child in _itemsContainer.GetChildren())
            {
                child.QueueFree();
            }

            // 添加物品
            for (int i = startIdx; i < endIdx && i < recyclables.Count; i++)
            {
                var itemRow = CreateItemRow(recyclables[i]);
                _itemsContainer.AddChild(itemRow);
            }

            // 更新页码
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)recyclables.Count / _itemsPerPage));
            _pageLabel.Text = $"第 {_currentPage + 1}/{totalPages} 页 (共 {recyclables.Count} 件)";

            // 更新预览
            UpdatePreview();
        }

        private List<Dictionary<string, object>> GetRecyclableItems()
        {
            var items = new List<Dictionary<string, object>>();

            // 从背包获取装备
            if (InventoryManager.Inventory != null)
            {
                foreach (var slot in InventoryManager.Inventory)
                {
                    if (slot?.ItemData != null)
                    {
                        var itemType = slot.ItemData.Get("type", "");
                        if (itemType is string typeStr && (typeStr == "Weapon" || typeStr == "Armor" || typeStr == "Accessory" || typeStr == "Mount" || typeStr == "Pet"))
                        {
                            items.Add(slot.ItemData);
                        }
                    }
                }
            }

            return items;
        }

        private Control CreateItemRow(Dictionary<string, object> item)
        {
            var row = new HBoxContainer();
            row.CustomMinimumSize = new Vector2(400, 40);

            // 检查选中
            bool isSelected = _selectedItems.Exists(x => x.Get("id") == item.Get("id"));

            // 物品名称
            var nameLabel = new Label();
            nameLabel.Text = item.Get("name", "Unknown").ToString();
            nameLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            row.AddChild(nameLabel);

            // 稀有度
            var rarityLabel = new Label();
            rarityLabel.Text = item.Get("rarity", "Common").ToString();
            rarityLabel.CustomMinimumSize = new Vector2(80, 0);
            row.AddChild(rarityLabel);

            // 增强等级
            var enhanceLabel = new Label();
            int enhance = item.ContainsKey("enhancement_level") ? Convert.ToInt32(item["enhancement_level"]) : 0;
            enhanceLabel.Text = enhance > 0 ? $"+{enhance}" : "";
            enhanceLabel.CustomMinimumSize = new Vector2(50, 0);
            row.AddChild(enhanceLabel);

            // 回收预览
            int previewGold = 0;
            if (EquipmentRecycleSystem.Instance != null)
            {
                var preview = EquipmentRecycleSystem.Instance.GetRecyclePreview(item);
                previewGold = preview.Gold;
            }

            var previewLabel = new Label();
            previewLabel.Text = $"{previewGold} 金币";
            previewLabel.CustomMinimumSize = new Vector2(80, 0);
            row.AddChild(previewLabel);

            // 点击选择
            row.GuiInput += (InputEvent @event) =>
            {
                if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
                {
                    ToggleItemSelection(item);
                }
            };

            // 选中样式
            if (isSelected)
            {
                row.Modulate = new Color(1f, 0.8f, 0.8f, 1f);
            }

            return row;
        }

        private void ToggleItemSelection(Dictionary<string, object> item)
        {
            int idx = _selectedItems.FindIndex(x => x.Get("id") == item.Get("id"));
            if (idx >= 0)
            {
                _selectedItems.RemoveAt(idx);
            }
            else
            {
                _selectedItems.Add(item);
            }
            RefreshItems();
        }

        private void UpdatePreview()
        {
            int totalGold = 0;
            var totalMaterials = new Dictionary<string, int>();

            foreach (var item in _selectedItems)
            {
                if (EquipmentRecycleSystem.Instance != null)
                {
                    var preview = EquipmentRecycleSystem.Instance.GetRecyclePreview(item);
                    totalGold += preview.Gold;
                    foreach (string mat in preview.Materials)
                    {
                        if (!totalMaterials.ContainsKey(mat))
                            totalMaterials[mat] = 0;
                        totalMaterials[mat]++;
                    }
                }
            }

            _goldLabel.Text = $"回收获得: {totalGold} 金币";

            string materialsText = "材料: ";
            if (totalMaterials.Count > 0)
            {
                int count = 0;
                foreach (var kvp in totalMaterials)
                {
                    if (count++ < 5)
                        materialsText += $"{kvp.Key} x{kvp.Value} ";
                }
            }
            else
            {
                materialsText += "无";
            }
            _materialsLabel.Text = materialsText;

            _recycleButton.Disabled = _selectedItems.Count == 0;
        }

        private void OnRecyclePressed()
        {
            if (_selectedItems.Count == 0) return;

            if (EquipmentRecycleSystem.Instance != null)
            {
                var result = EquipmentRecycleSystem.Instance.BatchRecycle(_selectedItems);
                if (result.Success)
                {
                    ShowNotification($"成功回收 {result.Count} 件装备，获得 {result.TotalGold} 金币");
                    _selectedItems.Clear();
                    RefreshItems();
                    // 刷新背包
                    InventoryManager.InventoryUI?.Refresh();
                }
            }
        }

        private void OnStatsPressed()
        {
            if (EquipmentRecycleSystem.Instance != null)
            {
                var stats = EquipmentRecycleSystem.Instance.GetStats();
                string msg = $"回收统计:\n总回收: {stats.TotalRecycled} 件\n总金币: {stats.TotalGoldEarned}\n最常回收: {(string.IsNullOrEmpty(stats.FavoriteRarity) ? "无" : stats.FavoriteRarity)}";
                ShowNotification(msg);
            }
        }

        private void OnClosePressed()
        {
            HideUI();
        }

        private void PlayAppearAnimation()
        {
            var tween = CreateTween();
            _mainPanel.Scale = new Vector2(0.8f, 0.8f);
            _mainPanel.Modulate = new Color(1f, 1f, 1f, 0f);
            tween.SetParallel(true);
            tween.TweenProperty(_mainPanel, "scale", new Vector2(1f, 1f), 0.3f).SetEase(Tween.EaseType.Out).SetTrans(Tween.TransitionType.Back);
            tween.TweenProperty(_mainPanel, "modulate:a", 1f, 0.3f);
        }

        private void ShowNotification(string text)
        {
            GD.Print($"[装备回收] {text}");
        }
    }
}
