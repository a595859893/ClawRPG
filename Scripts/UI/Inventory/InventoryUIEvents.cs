using Godot;
using System;
using ClawRPG.Scripts.Items;

public partial class InventoryUI
{
    private void OnSlotGuiInput(Button slot, InputEvent evt) {
        if (evt is InputEventMouseButton btn && btn.ButtonIndex == MouseButton.Left) {
            if (btn.Pressed) {
                // Start drag after a short delay
                _dragStartTimer = 0.1f;
                _dragSlot = slot;
            } else {
                // Release - normal click
                _dragStartTimer = 0;
                _dragSlot = null;
            }
        }
    }
    
    private void OnSlotPressed(Button slot)
    {
        _selectedSlotIndex = (int)slot.GetMeta("slot_index");
        int itemId = (int)slot.GetMeta("item_id");
        
        if (itemId > 0 && _selectedSlotIndex < _displaySlots.Count)
        {
            var invSlot = _displaySlots[_selectedSlotIndex];
            var item = ItemDatabase.Instance.GetItem(invSlot.ItemId);
            if (item != null)
            {
                string info = $"📦 {item.Name}\n\n";
                info += $"类型: {GetTypeName(item.Type)}\n";
                info += $"品质: {GetQualityName(item.Quality)}\n";
                info += $"价格: {item.Value}\n\n";
                info += $"描述:\n{item.Description}\n\n";
                info += $"数量: x{invSlot.Quantity}";
                
                _itemInfoLabel.Text = info;
            }
        }
        else
        {
            _itemInfoLabel.Text = "空槽位";
        }
    }
    
    private void OnFilterPressed(int filterIndex)
    {
        // Update button styles
        for (int i = 0; i < _filterButtons.Length; i++)
        {
            var style = _filterButtons[i].GetThemeStylebox("normal") as StyleBoxFlat;
            if (style != null)
            {
                style.BgColor = i == filterIndex ? new Color(0.3f, 0.5f, 0.8f) : new Color(0.25f, 0.25f, 0.3f);
            }
        }
        
        _currentFilter = (InventoryFilter)filterIndex;
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetFilter(_currentFilter);
        }
    }
    
    private void OnSortPressed(int sortIndex)
    {
        _currentSort = (InventorySort)sortIndex;
        
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetSort(_currentSort);
        }
    }
    
    private void OnSearchTextChanged(string text)
    {
        if (InventoryManager.Instance != null)
        {
            InventoryManager.Instance.SetSearchQuery(text);
        }
    }
    
    private void OnUseButtonPressed()
    {
        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _displaySlots.Count)
        {
            var invSlot = _displaySlots[_selectedSlotIndex];
            var item = ItemDatabase.Instance.GetItem(invSlot.ItemId);
            
            if (item != null && item is Consumable)
            {
                if (InventoryManager.Instance != null)
                {
                    InventoryManager.Instance.UseItem(invSlot.Index);
                    
                    // Show feedback
                    _itemInfoLabel.Text = $"✅ 使用了 {item.Name}";
                }
            }
            else
            {
                _itemInfoLabel.Text = "❌ 该物品无法使用";
            }
        }
        else
        {
            _itemInfoLabel.Text = "请先选择一个物品";
        }
    }
    
    private void OnDropButtonPressed()
    {
        if (_selectedSlotIndex >= 0 && _selectedSlotIndex < _displaySlots.Count)
        {
            var invSlot = _displaySlots[_selectedSlotIndex];
            
            if (InventoryManager.Instance != null)
            {
                InventoryManager.Instance.RemoveItem(invSlot.ItemId, 1);
                _itemInfoLabel.Text = "✅ 丢弃了一个物品";
            }
        }
        else
        {
            _itemInfoLabel.Text = "请先选择一个物品";
        }
    }
}
