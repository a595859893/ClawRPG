using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// InventoryUI - 背包UI管理
    /// 处理物品显示、拖拽、装备、背包分页等功能
    /// 支持战利品过滤器（REQ-102）
    /// </summary>
    public partial class InventoryUI : BaseUI
    {
        public static new InventoryUI Instance { get; protected set; }

        // 场景引用
        private Main _main;
        private Player _player;

        // 背包数据
        private List<ItemData> _allItems = new List<ItemData>();   // 全量物品（过滤前）
        private List<ItemData> _items = new List<ItemData>();      // 当前页显示物品（过滤后）
        private int _selectedSlot = -1;
        private int _currentPage = 0;
        private const int ITEMS_PER_PAGE = 24;

        // UI 节点
        private GridContainer _itemGrid;
        private Label _pageLabel;
        private Button _prevPageButton;
        private Button _nextPageButton;
        private Label _goldLabel;
        private Control _itemTooltip;
        private Control _equipmentPanel;

        // 过滤器相关 (REQ-102)
        private InventoryFilterSystem _filterSystem;
        private HBoxContainer _filterBar;
        private Label _hiddenCountLabel;
        private Button _clearFilterButton;
        private readonly Dictionary<string, Button> _presetButtons = new Dictionary<string, Button>();

        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            LoadNodes();
            SetupUI();
            SetupFilterSystem();
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.InventoryUpdated += OnInventoryUpdated;
            }
            RefreshInventory();
            Hide();
        }

        private void LoadNodes()
        {
            var canvasLayer = GetTree()?.CurrentScene?.GetNodeOrNull<CanvasLayer>("CanvasLayer");
            if (canvasLayer != null)
            {
                var inv = canvasLayer.GetNodeOrNull<Control>("InventoryUI");
                if (inv != null)
                {
                    _itemGrid = inv.GetNodeOrNull<GridContainer>("VBox/ItemGrid");
                    _pageLabel = inv.GetNodeOrNull<Label>("VBox/PageLabel");
                    _prevPageButton = inv.GetNodeOrNull<Button>("VBox/PrevPageButton");
                    _nextPageButton = inv.GetNodeOrNull<Button>("VBox/NextPageButton");
                    _goldLabel = inv.GetNodeOrNull<Label>("VBox/GoldLabel");
                    _itemTooltip = inv.GetNodeOrNull<Control>("ItemTooltip");
                    _equipmentPanel = inv.GetNodeOrNull<Control>("EquipmentPanel");
                    
                    // REQ-102: Filter bar nodes
                    _filterBar = inv.GetNodeOrNull<HBoxContainer>("VBox/FilterBar");
                    _hiddenCountLabel = inv.GetNodeOrNull<Label>("VBox/FilterBar/HiddenCountLabel");
                    _clearFilterButton = inv.GetNodeOrNull<Button>("VBox/FilterBar/ClearFilterButton");
                }
            }

            // 降级查找
            if (_itemGrid == null)
                _itemGrid = GetNodeOrNull<GridContainer>("VBox/ItemGrid");
            if (_pageLabel == null)
                _pageLabel = GetNodeOrNull<Label>("VBox/PageLabel");
            if (_prevPageButton == null)
                _prevPageButton = GetNodeOrNull<Button>("VBox/PrevPageButton");
            if (_nextPageButton == null)
                _nextPageButton = GetNodeOrNull<Button>("VBox/NextPageButton");
            if (_goldLabel == null)
                _goldLabel = GetNodeOrNull<Label>("VBox/GoldLabel");
            if (_itemTooltip == null)
                _itemTooltip = GetNodeOrNull<Control>("ItemTooltip");
            if (_equipmentPanel == null)
                _equipmentPanel = GetNodeOrNull<Control>("EquipmentPanel");

            ConnectButtons();
            ConnectFilterButtons();
        }

        private void ConnectButtons()
        {
            if (_prevPageButton != null)
                _prevPageButton.Pressed += OnPrevPage;
            if (_nextPageButton != null)
                _nextPageButton.Pressed += OnNextPage;
        }

        // ===== REQ-102: Loot Filter =====

        private void SetupFilterSystem()
        {
            if (InventoryFilterSystem.Instance != null)
            {
                _filterSystem = InventoryFilterSystem.Instance;
                _filterSystem.OnFilterChanged += OnFilterChanged;
                _filterSystem.OnPresetApplied += OnPresetApplied;
                BuildFilterBarPresets();
            }

            if (_clearFilterButton != null)
                _clearFilterButton.Pressed += OnClearFilter;

            // F key toggles rarity filter
        }

        private void BuildFilterBarPresets()
        {
            if (_filterBar == null || _filterSystem == null) return;

            // Remove existing preset buttons (keep system buttons like ClearFilter)
            foreach (var btn in _presetButtons.Values)
            {
                btn.QueueFree();
            }
            _presetButtons.Clear();

            var presets = _filterSystem.GetAvailablePresets();
            foreach (var (id, name) in presets)
            {
                var btn = new Button
                {
                    Text = name,
                    ToggleMode = true,
                    Pressed = (_filterSystem.FilterData.ActivePresetId == id)
                };
                btn.Pressed += () => OnPresetButtonPressed(id);
                _filterBar.AddChild(btn);
                _presetButtons[id] = btn;
            }
        }

        private void OnPresetButtonPressed(string presetId)
        {
            _filterSystem.ApplyPreset(presetId);
        }

        private void OnPresetApplied(string presetId)
        {
            // Update button states
            foreach (var kvp in _presetButtons)
            {
                if (kvp.Key != null)
                    kvp.Value.Pressed = (kvp.Key == presetId);
            }
        }

        private void OnFilterChanged()
        {
            _currentPage = 0;  // Reset to first page when filter changes
            if (_filterSystem != null)
                _filterSystem.InvalidateCache();
            Refresh();
        }

        private void OnClearFilter()
        {
            if (_filterSystem != null)
            {
                _filterSystem.UseCustomRules();
                _filterSystem.ClearCustomRules();
            }
            foreach (var btn in _presetButtons.Values)
                btn.Pressed = false;
        }

        private void ConnectFilterButtons()
        {
            if (_clearFilterButton != null)
                _clearFilterButton.Pressed += OnClearFilter;
        }

        public void Initialize(Main main)
        {
            _main = main;
            _player = GetTree()?.GetFirstNodeInGroup("player") as Player;
            LoadInventoryData();
        }

        private void LoadInventoryData()
        {
            if (_player != null && _player.HasMethod("GetInventory"))
            {
                _allItems = (List<ItemData>)_player.Get("GetInventory").DynamicInvoke();
            }
            // Apply filter if system exists
            if (_filterSystem != null)
            {
                _items = _filterSystem.ApplyFilter(_allItems);
            }
            else
            {
                _items = new List<ItemData>(_allItems);
            }
            Refresh();
        }

        protected override void OnShow()
        {
            GD.Print("[InventoryUI] Inventory opened");
            LoadInventoryData();
        }

        protected override void OnHide()
        {
            GD.Print("[InventoryUI] Inventory closed");
            _selectedSlot = -1;
            HideTooltip();
        }

        protected override void OnRefresh()
        {
            UpdateItemGrid();
            UpdatePageControls();
            UpdateGoldDisplay();
            UpdateFilterHiddenCount();
        }

        private void UpdateFilterHiddenCount()
        {
            if (_hiddenCountLabel == null || _filterSystem == null) return;

            int hidden = _filterSystem.GetHiddenCount(_allItems);
            if (hidden > 0)
            {
                _hiddenCountLabel.Text = $"还有 {hidden} 件物品被隐藏";
                _hiddenCountLabel.Visible = true;
            }
            else
            {
                _hiddenCountLabel.Visible = false;
            }
        }

        private void UpdateItemGrid()
        {
            if (_itemGrid == null) return;

            // 清除现有物品
            foreach (Node child in _itemGrid.GetChildren())
            {
                child.QueueFree();
            }

            // 计算分页
            int startIndex = _currentPage * ITEMS_PER_PAGE;
            int endIndex = Math.Min(startIndex + ITEMS_PER_PAGE, _items.Count);

            // 填充当前页物品
            for (int i = startIndex; i < endIndex; i++)
            {
                var slot = CreateItemSlot(_items[i], i);
                _itemGrid.AddChild(slot);
            }
        }

        private Control CreateItemSlot(ItemData item, int index)
        {
            var slot = new TextureRect();
            slot.CustomMinimumSize = new Vector2(48, 48);

            if (item != null && item.Icon != null)
            {
                slot.Texture = item.Icon;
            }

            if (item != null)
            {
                slot.GuiInput += (ev) => OnItemSlotInput(item, ev);
            }

            // 选中高亮
            if (index == _selectedSlot)
            {
                var highlight = new ColorRect
                {
                    Color = new Color(1f, 1f, 0f, 0.3f),
                    ExpandToLength = true
                };
                slot.AddChild(highlight);
            }

            return slot;
        }

        private void OnItemSlotInput(ItemData item, InputEvent evt)
        {
            if (evt is InputEventMouseButton mb && mb.Pressed && mb.ButtonIndex == MouseButton.Left)
            {
                OnItemClicked(item);
            }
        }

        private void OnItemClicked(ItemData item)
        {
            if (item == null) return;
            GD.Print($"[InventoryUI] Item clicked: {item.Name}");
            ShowTooltip(item);
        }

        private void ShowTooltip(ItemData item)
        {
            if (_itemTooltip == null) return;
            _itemTooltip.Visible = true;

            var nameLabel = _itemTooltip.GetNodeOrNull<Label>("NameLabel");
            var descLabel = _itemTooltip.GetNodeOrNull<Label>("DescLabel");
            var statsLabel = _itemTooltip.GetNodeOrNull<Label>("StatsLabel");

            if (nameLabel != null) nameLabel.Text = item.Name;
            if (descLabel != null) descLabel.Text = item.Description;
            if (statsLabel != null && item.HasMethod("GetStatsString"))
                statsLabel.Text = item.GetStatsString();
        }

        private void HideTooltip()
        {
            if (_itemTooltip != null)
                _itemTooltip.Visible = false;
        }

        private void UpdatePageControls()
        {
            if (_pageLabel == null) return;

            int totalPages = Math.Max(1, (int)Math.Ceiling((double)_items.Count / ITEMS_PER_PAGE));
            _pageLabel.Text = $"Page {_currentPage + 1}/{totalPages}";

            if (_prevPageButton != null)
                _prevPageButton.Disabled = _currentPage <= 0;
            if (_nextPageButton != null)
                _nextPageButton.Disabled = _currentPage >= totalPages - 1;
        }

        private void UpdateGoldDisplay()
        {
            if (_goldLabel == null || _player == null) return;

            if (_player.HasMethod("GetGold"))
            {
                _goldLabel.Text = $"Gold: {_player.GetGold()}";
            }
        }

        private void OnPrevPage()
        {
            if (_currentPage > 0)
            {
                _currentPage--;
                Refresh();
            }
        }

        private void OnNextPage()
        {
            int totalPages = Math.Max(1, (int)Math.Ceiling((double)_items.Count / ITEMS_PER_PAGE));
            if (_currentPage < totalPages - 1)
            {
                _currentPage++;
                Refresh();
            }
        }

        public void AddItem(ItemData item)
        {
            _allItems.Add(item);
            // Re-apply filter
            if (_filterSystem != null)
                _items = _filterSystem.ApplyFilter(_allItems);
            else
                _items = new List<ItemData>(_allItems);
            Refresh();
        }

        public void RemoveItem(int slotIndex)
        {
            if (slotIndex >= 0 && slotIndex < _allItems.Count)
            {
                _allItems.RemoveAt(slotIndex);
                // Re-apply filter
                if (_filterSystem != null)
                    _items = _filterSystem.ApplyFilter(_allItems);
                else
                    _items = new List<ItemData>(_allItems);
                Refresh();
            }
        }

        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                ["UIName"] = UIName,
                ["CurrentPage"] = _currentPage,
                ["ItemCount"] = _items.Count
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            base.ImportSaveData(data);
            if (data.Contains("CurrentPage"))
                _currentPage = Convert.ToInt32(data["CurrentPage"]);
            Refresh();
        }

        public override void _ExitTree()
        {
            if (_prevPageButton != null)
                _prevPageButton.Pressed -= OnPrevPage;
            if (_nextPageButton != null)
                _nextPageButton.Pressed -= OnNextPage;
            if (_clearFilterButton != null)
                _clearFilterButton.Pressed -= OnClearFilter;
            if (_filterSystem != null)
            {
                _filterSystem.OnFilterChanged -= OnFilterChanged;
                _filterSystem.OnPresetApplied -= OnPresetApplied;
            }
            Instance = null;
        }

        public override void _Input(InputEvent evt)
        {
            // REQ-102: F key toggles rarity filter cycling
            if (Visible && evt is InputEventKey key && key.Pressed && key.Keycode == Key.F)
            {
                CycleRarityFilter();
            }
        }

        private int _rarityFilterIndex = -1;  // -1 = no filter

        private void CycleRarityFilter()
        {
            if (_filterSystem == null) return;

            string[] rarityOrder = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
            _rarityFilterIndex++;

            if (_rarityFilterIndex >= rarityOrder.Length)
            {
                // Turn off filter
                _rarityFilterIndex = -1;
                _filterSystem.UseCustomRules();
                _filterSystem.ClearCustomRules();
                GD.Print("[InventoryUI] Rarity filter cleared");
            }
            else
            {
                string rarity = rarityOrder[_rarityFilterIndex];
                _filterSystem.UseCustomRules();
                _filterSystem.ClearCustomRules();
                _filterSystem.ToggleCustomRule(FilterDimension.Rarity, rarity, rarity, true);
                GD.Print($"[InventoryUI] Rarity filter set to: {rarity}");
            }
        }

        /// <summary>
        /// Reloads inventory from player and re-applies filter.
        /// </summary>
        public void RefreshInventory()
        {
            LoadInventoryData();
        }
    }

    /// <summary>
    /// 物品数据结构 (占位，实际项目中应在独立文件中定义)
    /// </summary>
    public class ItemData
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public Texture2D Icon { get; set; }
        public int Quantity { get; set; } = 1;

        public string GetStatsString() => "";
    }
}
