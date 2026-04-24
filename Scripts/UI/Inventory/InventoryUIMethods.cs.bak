using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Items;
using InventoryManager = ClawRPG.Scripts.Items.InventoryManager;

namespace ClawRPG.Scripts.UI
{
public partial class InventoryUI
{
    public override void _Input(InputEvent evt)
    {
        // Toggle with I key
        if (evt is InputEventKey key && key.Pressed && key.Keycode == Key.I)
        {
            ToggleInventory();
        }
    }
    
    public override void _Process(double delta) {
        if (_dragStartTimer > 0 && _dragSlot != null) {
            _dragStartTimer -= delta;
            if (_dragStartTimer <= 0) {
                // Start dragging
                var itemId = _dragSlot.GetMeta("item_id") as string;
                var qty = (int)_dragSlot.GetMeta("quantity");
                
                if (!string.IsNullOrEmpty(itemId) && qty > 0) {
                    var item = ItemDatabase.Instance.GetItem(itemId);
                    if (item != null && DragDropHelper.Instance != null) {
                        DragDropHelper.Instance.StartDrag(itemId, qty, item.Type);
                    }
                }
            }
        }
    }
    
    private void SetupUI()
    {
        // Main panel
        _mainPanel = new Panel();
        _mainPanel.SetAnchor(AnchorPresets.Center);
        _mainPanel.Position = new Vector2(-350, -280);
        _mainPanel.CustomMinimumSize = new Vector2(700, 560);
        
        // Panel style
        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelStyle.BorderWidthBottom = 3;
        panelStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        panelStyle.CornerRadiusTopLeft = 10;
        panelStyle.CornerRadiusTopRight = 10;
        panelStyle.CornerRadiusBottomLeft = 10;
        panelStyle.CornerRadiusBottomRight = 10;
        _mainPanel.AddThemeStyleboxOverride("panel", panelStyle);
        
        AddChild(_mainPanel);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🎒 背包";
        _titleLabel.SetAnchor(AnchorPresets.TopLeft);
        _titleLabel.Position = new Vector2(20, 15);
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        _titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.85, 0.3));
        _mainPanel.AddChild(_titleLabel);
        
        // Gold label
        _goldLabel = new Label();
        _goldLabel.Text = "💰 0";
        _goldLabel.SetAnchor(AnchorPresets.TopRight);
        _goldLabel.Position = new Vector2(-180, 18);
        _goldLabel.AddThemeFontSizeOverride("font_size", 20);
        _goldLabel.AddThemeColorOverride("font_color", new Color(1, 0.85, 0.3));
        _mainPanel.AddChild(_goldLabel);
        
        // Close button
        _closeButton = new Button();
        _closeButton.Text = "✕";
        _closeButton.SetAnchor(AnchorPresets.TopRight);
        _closeButton.Position = new Vector2(-40, 10);
        _closeButton.CustomMinimumSize = new Vector2(35, 35);
        _closeButton.Pressed += () => HideInventory();
        
        var closeStyle = new StyleBoxFlat();
        closeStyle.BgColor = new Color(0.6f, 0.2f, 0.2f);
        closeStyle.CornerRadiusTopLeft = 5;
        closeStyle.CornerRadiusTopRight = 5;
        closeStyle.CornerRadiusBottomLeft = 5;
        closeStyle.CornerRadiusBottomRight = 5;
        _closeButton.AddThemeStyleboxOverride("normal", closeStyle);
        _closeButton.AddThemeStyleboxOverride("hover", closeStyle);
        
        _mainPanel.AddChild(_closeButton);
        
        // Search box
        _searchBox = new LineEdit();
        _searchBox.Placeholder = "搜索物品...";
        _searchBox.SetAnchor(AnchorPresets.TopLeft);
        _searchBox.Position = new Vector2(20, 50);
        _searchBox.CustomMinimumSize = new Vector2(150, 30);
        _searchBox.TextChanged += OnSearchTextChanged;
        _mainPanel.AddChild(_searchBox);
        
        // Filter buttons
        SetupFilterButtons();
        
        // Sort buttons
        SetupSortButtons();
        
        // Slots info
        _slotsLabel = new Label();
        _slotsLabel.Text = "0/30 槽位";
        _slotsLabel.SetAnchor(AnchorPresets.TopRight);
        _slotsLabel.Position = new Vector2(-120, 52);
        _slotsLabel.AddThemeFontSizeOverride("font_size", 14);
        _slotsLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        _mainPanel.AddChild(_slotsLabel);
        
        // Item grid
        _itemGrid = new GridContainer();
        _itemGrid.SetAnchor(AnchorPresets.TopLeft);
        _itemGrid.Position = new Vector2(20, 130);
        _itemGrid.CustomMinimumSize = new Vector2(450, 380);
        _itemGrid.Columns = SlotsPerRow;
        _mainPanel.AddChild(_itemGrid);
        
        // Create slot buttons
        for (int i = 0; i < TotalSlots; i++)
        {
            var slot = CreateSlotButton(i);
            _itemGrid.AddChild(slot);
        }
        
        // Item info panel
        var infoPanel = new Panel();
        infoPanel.SetAnchor(AnchorPresets.TopRight);
        infoPanel.Position = new Vector2(-210, 130);
        infoPanel.CustomMinimumSize = new Vector2(190, 380);
        
        var infoStyle = new StyleBoxFlat();
        infoStyle.BgColor = new Color(0.15f, 0.15f, 0.2f);
        infoStyle.BorderWidthBottom = 2;
        infoStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        infoStyle.CornerRadiusTopLeft = 8;
        infoStyle.CornerRadiusTopRight = 8;
        infoStyle.CornerRadiusBottomLeft = 8;
        infoStyle.CornerRadiusBottomRight = 8;
        infoPanel.AddThemeStyleboxOverride("panel", infoStyle);
        
        _mainPanel.AddChild(infoPanel);
        
        _itemInfoLabel = new Label();
        _itemInfoLabel.SetAnchor(AnchorPresets.FullRect);
        _itemInfoLabel.Position = new Vector2(10, 10);
        _itemInfoLabel.CustomMinimumSize = new Vector2(170, 340);
        _itemInfoLabel.Text = "选择一个物品\n查看详情";
        _itemInfoLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _itemInfoLabel.AddThemeFontSizeOverride("font_size", 14);
        _itemInfoLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        infoPanel.AddChild(_itemInfoLabel);
        
        // Quick use button
        var useButton = new Button();
        useButton.Text = "使用物品";
        useButton.SetAnchor(AnchorPresets.BottomLeft);
        useButton.Position = new Vector2(20, -50);
        useButton.CustomMinimumSize = new Vector2(100, 35);
        useButton.Pressed += OnUseButtonPressed;
        
        var useStyle = new StyleBoxFlat();
        useStyle.BgColor = new Color(0.2f, 0.5f, 0.3f);
        useStyle.CornerRadiusTopLeft = 5;
        useStyle.CornerRadiusTopRight = 5;
        useStyle.CornerRadiusBottomLeft = 5;
        useStyle.CornerRadiusBottomRight = 5;
        useButton.AddThemeStyleboxOverride("normal", useStyle);
        
        var useHoverStyle = useStyle.Duplicate() as StyleBoxFlat;
        useHoverStyle.BgColor = new Color(0.3f, 0.6f, 0.4f);
        useButton.AddThemeStyleboxOverride("hover", useHoverStyle);
        
        _mainPanel.AddChild(useButton);
        
        // Drop button
        var dropButton = new Button();
        dropButton.Text = "丢弃物品";
        dropButton.SetAnchor(AnchorPresets.BottomLeft);
        dropButton.Position = new Vector2(130, -50);
        dropButton.CustomMinimumSize = new Vector2(100, 35);
        dropButton.Pressed += OnDropButtonPressed;
        
        var dropStyle = new StyleBoxFlat();
        dropStyle.BgColor = new Color(0.5f, 0.3f, 0.2f);
        dropStyle.CornerRadiusTopLeft = 5;
        dropStyle.CornerRadiusTopRight = 5;
        dropStyle.CornerRadiusBottomLeft = 5;
        dropStyle.CornerRadiusBottomRight = 5;
        dropButton.AddThemeStyleboxOverride("normal", dropStyle);
        
        var dropHoverStyle = dropStyle.Duplicate() as StyleBoxFlat;
        dropHoverStyle.BgColor = new Color(0.6f, 0.4f, 0.3f);
        dropButton.AddThemeStyleboxOverride("hover", dropHoverStyle);
        
        _mainPanel.AddChild(dropButton);
    }
    
    private void SetupFilterButtons()
    {
        string[] filterNames = { "全部", "武器", "防具", "饰品", "消耗品", "材料", "任务" };
        _filterButtons = new Button[filterNames.Length];
        
        for (int i = 0; i < filterNames.Length; i++)
        {
            var btn = new Button();
            btn.Text = filterNames[i];
            btn.Position = new Vector2(180 + i * 58, 50);
            btn.CustomMinimumSize = new Vector2(55, 30);
            
            var style = new StyleBoxFlat();
            style.BgColor = i == 0 ? new Color(0.3f, 0.5f, 0.8f) : new Color(0.25f, 0.25f, 0.3f);
            style.CornerRadiusTopLeft = 4;
            style.CornerRadiusTopRight = 4;
            style.CornerRadiusBottomLeft = 4;
            style.CornerRadiusBottomRight = 4;
            btn.AddThemeStyleboxOverride("normal", style);
            
            var hoverStyle = style.Duplicate() as StyleBoxFlat;
            hoverStyle.BgColor = new Color(0.4f, 0.6f, 0.9f);
            btn.AddThemeStyleboxOverride("hover", hoverStyle);
            
            int filterIndex = i;
            btn.Pressed += () => OnFilterPressed(filterIndex);
            
            _mainPanel.AddChild(btn);
            _filterButtons[i] = btn;
        }
    }
    
    private void SetupSortButtons()
    {
        string[] sortNames = { "默认", "名称", "类型", "价值", "品质" };
        
        var sortLabel = new Label();
        sortLabel.Text = "排序:";
        sortLabel.Position = new Vector2(490, 52);
        sortLabel.AddThemeFontSizeOverride("font_size", 14);
        sortLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        _mainPanel.AddChild(sortLabel);
        
        for (int i = 0; i < sortNames.Length; i++)
        {
            var btn = new Button();
            btn.Text = sortNames[i];
            btn.Position = new Vector2(530 + i * 42, 50);
            btn.CustomMinimumSize = new Vector2(40, 30);
            
            var style = new StyleBoxFlat();
            style.BgColor = i == 0 ? new Color(0.3f, 0.5f, 0.8f) : new Color(0.25f, 0.25f, 0.3f);
            style.CornerRadiusTopLeft = 4;
            style.CornerRadiusTopRight = 4;
            style.CornerRadiusBottomLeft = 4;
            style.CornerRadiusBottomRight = 4;
            btn.AddThemeStyleboxOverride("normal", style);
            
            int sortIndex = i;
            btn.Pressed += () => OnSortPressed(sortIndex);
            
            _mainPanel.AddChild(btn);
        }
    }
    
    private Button CreateSlotButton(int index)
    {
        var slot = new Button();
        slot.CustomMinimumSize = new Vector2(80, 70);
        slot.Text = "";
        
        // Style
        var normalStyle = new StyleBoxFlat();
        normalStyle.BgColor = new Color(0.2f, 0.2f, 0.25f);
        normalStyle.BorderWidthBottom = 2;
        normalStyle.BorderColor = new Color(0.35f, 0.35f, 0.4f);
        normalStyle.CornerRadiusTopLeft = 6;
        normalStyle.CornerRadiusTopRight = 6;
        normalStyle.CornerRadiusBottomLeft = 6;
        normalStyle.CornerRadiusBottomRight = 6;
        slot.AddThemeStyleboxOverride("normal", normalStyle);
        
        var hoverStyle = normalStyle.Duplicate() as StyleBoxFlat;
        hoverStyle.BorderColor = new Color(0.8f, 0.7f, 0.3f);
        slot.AddThemeStyleboxOverride("hover", hoverStyle);
        
        slot.SetMeta("slot_index", index);
        slot.Pressed += () => OnSlotPressed(slot);
        
        // Add drag support using GUI input event
        slot.GuiInput += (evt) => OnSlotGuiInput(slot, evt);
        
        return slot;
    }
    
    private void OnInventoryUpdated()
    {
        RefreshInventory();
    }
    
    private void RefreshInventory()
    {
        if (InventoryManager.Instance != null)
        {
            // Apply search query
            InventoryManager.Instance.SetSearchQuery(_searchBox.Text);
            
            // Get filtered slots
            _displaySlots = InventoryManager.Instance.GetFilteredSlots();
            
            // Update slots label
            int usedSlots = _displaySlots.Count;
            int totalSlots = InventoryManager.Instance.MaxSlots;
            _slotsLabel.Text = $"{usedSlots}/{totalSlots} 槽位";
            
            UpdateGrid();
        }
    }
    
    private void UpdateGrid()
    {
        var slots = _itemGrid.GetChildren();
        
        for (int i = 0; i < slots.Count; i++)
        {
            var slot = slots[i] as Button;
            if (slot == null) continue;
            
            if (i < _displaySlots.Count)
            {
                var invSlot = _displaySlots[i];
                var item = ItemDatabase.Instance.GetItem(invSlot.ItemId);
                if (item != null)
                {
                    // Get quality color
                    int qualityIndex = (int)(item.Quality);
                    qualityIndex = Mathf.Clamp(qualityIndex, 0, _qualityColors.Length - 1);
                    
                    slot.Text = $"{item.Name}\nx{invSlot.Quantity}";
                    slot.TooltipText = item.Description;
                    
                    // Apply quality color to text
                    slot.AddThemeColorOverride("font_color", _qualityColors[qualityIndex]);
                    
                    // Store item id for use/drop
                    slot.SetMeta("item_id", invSlot.ItemId);
                    slot.SetMeta("quantity", invSlot.Quantity);
                }
            }
            else
            {
                slot.Text = "";
                slot.TooltipText = "";
                slot.SetMeta("item_id", 0);
                slot.SetMeta("quantity", 0);
            }
        }
    }
    
    private string GetTypeName(Item.ItemType type)
    {
        return type switch
        {
            Item.ItemType.Weapon => "⚔️ 武器",
            Item.ItemType.Armor => "🛡️ 防具",
            Item.ItemType.Accessory => "💍 饰品",
            Item.ItemType.Consumable => "🧪 消耗品",
            Item.ItemType.Material => "📦 材料",
            Item.ItemType.QuestItem => "📜 任务物品",
            _ => "未知"
        };
    }
    
    private string GetQualityName(ItemQuality quality)
    {
        return quality switch
        {
            ItemQuality.Common => "⬜ 普通",
            ItemQuality.Uncommon => "🟢 优秀",
            ItemQuality.Rare => "🔵 稀有",
            ItemQuality.Epic => "🟣 史诗",
            ItemQuality.Legendary => "🟠 传说",
            _ => "普通"
        };
    }
    
    private void ToggleInventory()
    {
        if (_isVisible)
        {
            HideInventory();
        }
        else
        {
            ShowInventory();
        }
    }
    
    private void ShowInventory()
    {
        _isVisible = true;
        _mainPanel.Visible = true;
        RefreshInventory();
    }
    
    private void HideInventory()
    {
        _isVisible = false; 
        _mainPanel.Visible = false; 
    }
}
}
