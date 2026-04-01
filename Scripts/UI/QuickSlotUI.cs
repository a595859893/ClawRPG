using Godot;
using System;

public partial class QuickSlotUI : Control
{
    private HBoxContainer _slotContainer;
    private QuickSlotItem[] _slotItems = new QuickSlotItem[8];
    
    // Styling
    private Color normalColor = new Color(0.2f, 0.2f, 0.2f, 0.9f);
    private Color hoverColor = new Color(0.3f, 0.3f, 0.3f, 0.95f);
    private Color emptyColor = new Color(0.15f, 0.15f, 0.15f, 0.8f);
    private Color potionColor = new Color(0.2f, 0.6f, 1f, 0.9f);
    private Color foodColor = new Color(1f, 0.7f, 0.3f, 0.9f);
    private Color scrollColor = new Color(0.8f, 0.6f, 1f, 0.9f);
    
    public override void _Ready()
    {
        _CreateUI();
        _ConnectSignals();
    }
    
    private void _CreateUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        mainContainer.Position = new Vector2(0, 550);
        mainContainer.Size = new Vector2(1152, 120);
        mainContainer.GrowHorizontal = Control.GrowDirection.Both;
        mainContainer.Alignment = BoxContainer.AlignmentMode.Center;
        AddChild(mainContainer);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "[1-8] 快捷物品栏";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 16);
        mainContainer.AddChild(titleLabel);
        
        // Slot container
        _slotContainer = new HBoxContainer();
        _slotContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _slotContainer.GrowHorizontal = Control.GrowDirection.Both;
        _slotContainer.Spacing = 8;
        mainContainer.AddChild(_slotContainer);
        
        // Create 8 slots
        for (int i = 0; i < 8; i++)
        {
            var slotItem = new QuickSlotItem();
            slotItem.SlotIndex = i;
            slotItem.CustomMinimumSize = new Vector2(80, 80);
            
            _slotContainer.AddChild(slotItem);
            _slotItems[i] = slotItem;
            
            // Create slot visual
            _CreateSlotVisual(slotItem, i);
        }
        
        // Initial update
        _UpdateAllSlots();
        
        // Hide by default
        Visible = false; 
    }
    
    private void _CreateSlotVisual(QuickSlotItem slot, int index)
    {
        // Background panel
        var bgPanel = new Panel();
        bgPanel.CustomMinimumSize = new Vector2(80, 80);
        bgPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        slot.AddChild(bgPanel);
        
        // Style the panel
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = normalColor;
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.BorderColor = new Color(0.4f, 0.4f, 0.4f, 1f);
        styleBox.CornerRadiusTopLeft = 8;
        styleBox.CornerRadiusTopRight = 8;
        styleBox.CornerRadiusBottomLeft = 8;
        styleBox.CornerRadiusBottomRight = 8;
        bgPanel.AddThemeStyleboxOverride("panel", styleBox);
        
        // Hotkey label
        var keyLabel = new Label();
        keyLabel.Text = (index + 1).ToString();
        keyLabel.HorizontalAlignment = HorizontalAlignment.Center;
        keyLabel.VerticalAlignment = VerticalAlignment.Center;
        keyLabel.Position = new Vector2(0, 0);
        keyLabel.Size = new Vector2(80, 80);
        keyLabel.AddThemeFontSizeOverride("font_size", 20);
        keyLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f, 0.8f));
        bgPanel.AddChild(keyLabel);
        
        slot.KeyLabel = keyLabel;
        slot.BackgroundPanel = bgPanel;
        slot.StyleBox = styleBox;
        
        // Icon (Item icon will be added dynamically)
        var icon = new TextureRect();
        icon.SetAnchorsPreset(Control.LayoutPreset.Center);
        icon.Position = new Vector2(15, 15);
        icon.Size = new Vector2(50, 50);
        icon.StretchMode = TextureRect.StretchMode.KeepAspectCentered;
        icon.Modulate = new Color(1, 1, 1, 0.9f);
        bgPanel.AddChild(icon);
        
        slot.IconTexture = icon;
        
        // Count label
        var countLabel = new Label();
        countLabel.HorizontalAlignment = HorizontalAlignment.Right;
        countLabel.VerticalAlignment = VerticalAlignment.Bottom;
        countLabel.Position = new Vector2(50, 55);
        countLabel.AddThemeFontSizeOverride("font_size", 14);
        countLabel.AddThemeColorOverride("font_color", new Color(1, 1, 1, 1));
        bgPanel.AddChild(countLabel);
        
        slot.CountLabel = countLabel;
        
        // Item name label
        var nameLabel = new Label();
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.VerticalAlignment = VerticalAlignment.Bottom;
        nameLabel.Position = new Vector2(0, -15);
        nameLabel.Size = new Vector2(80, 20);
        nameLabel.AddThemeFontSizeOverride("font_size", 10);
        nameLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
        bgPanel.AddChild(nameLabel);
        
        slot.NameLabel = nameLabel;
    }
    
    private void _ConnectSignals()
    {
        // Connect to quick slot system
        if (QuickSlotSystem.Instance != null)
        {
            QuickSlotSystem.Instance.SlotUpdated += _OnSlotUpdated;
            QuickSlotSystem.Instance.SlotUsed += _OnSlotUsed;
        }
    }
    
    public override void _Input(InputEvent evt)
    {
        // Handle number key presses for quick slot activation
        if (evt is InputEventKey keyEvent && keyEvent.Pressed)
        {
            int slotIndex = -1;
            
            if (keyEvent.Keycode == Key.Key1) slotIndex = 0;
            else if (keyEvent.Keycode == Key.Key2) slotIndex = 1;
            else if (keyEvent.Keycode == Key.Key3) slotIndex = 2;
            else if (keyEvent.Keycode == Key.Key4) slotIndex = 3;
            else if (keyEvent.Keycode == Key.Key5) slotIndex = 4;
            else if (keyEvent.Keycode == Key.Key6) slotIndex = 5;
            else if (keyEvent.Keycode == Key.Key7) slotIndex = 6;
            else if (keyEvent.Keycode == Key.Key8) slotIndex = 7;
            
            if (slotIndex >= 0)
            {
                _ActivateSlot(slotIndex);
                GetViewport().SetInputAsHandled();
            }
        }
        
        // Toggle visibility with Tab
        if (evt is InputEventKey tabEvent && tabEvent.Pressed && tabEvent.Keycode == Key.Tab)
        {
            Visible = !Visible;
            GetViewport().SetInputAsHandled();
        }
    }
    
    private void _ActivateSlot(int slotIndex)
    {
        if (QuickSlotSystem.Instance != null)
        {
            QuickSlotSystem.Instance.UseSlot(slotIndex);
        }
    }
    
    private void _OnSlotUpdated(int slotIndex, QuickSlotData data)
    {
        if (slotIndex < 0 || slotIndex >= 8) return;
        
        var slot = _slotItems[slotIndex];
        var item = ItemDatabase.GetItem(data.ItemId);
        
        if (data.IsEmpty || item == null)
        {
            // Empty slot
            slot.StyleBox.BgColor = emptyColor;
            slot.IconTexture.Modulate = new Color(1, 1, 1, 0);
            slot.CountLabel.Text = "";
            slot.NameLabel.Text = "";
            return;
        }
        
        // Update slot with item data
        slot.StyleBox.BgColor = _GetSlotColor(data.SlotType);
        slot.CountLabel.Text = data.ItemCount > 1 ? data.ItemCount.ToString() : "";
        slot.NameLabel.Text = _TruncateName(item.Name, 6);
        
        // Try to load icon (would need actual texture in real implementation)
        // For now, use placeholder
        slot.IconTexture.Modulate = new Color(1, 1, 1, 0.9f);
    }
    
    private void _OnSlotUsed(int slotIndex, QuickSlotData data)
    {
        // Visual feedback when slot is used
        if (slotIndex >= 0 && slotIndex < 8)
        {
            var slot = _slotItems[slotIndex];
            
            // Flash effect
            var tween = CreateTween();
            tween.TweenProperty(slot.BackgroundPanel, "modulate", new Color(1.5f, 1.5f, 1.5f, 1f), 0.1f);
            tween.TweenProperty(slot.BackgroundPanel, "modulate", new Color(1f, 1f, 1f, 1f), 0.2f);
        }
    }
    
    private void _UpdateAllSlots()
    {
        if (QuickSlotSystem.Instance == null) return;
        
        var slots = QuickSlotSystem.Instance.GetAllSlots();
        for (int i = 0; i < 8; i++)
        {
            _OnSlotUpdated(i, slots[i]);
        }
    }
    
    private Color _GetSlotColor(QuickSlotType slotType)
    {
        switch (slotType)
        {
            case QuickSlotType.Potion:
                return potionColor;
            case QuickSlotType.Food:
                return foodColor;
            case QuickSlotType.Scroll:
                return scrollColor;
            default:
                return normalColor;
        }
    }
    
    private string _TruncateName(string name, int maxLength)
    {
        if (string.IsNullOrEmpty(name)) return "";
        if (name.Length <= maxLength) return name;
        return name.Substring(0, maxLength - 1) + "…";
    }
    
    public void Show() => Visible = true;
    public void Hide() => Visible = false; 
    public void Toggle() => Visible = !Visible;
}

// Helper class for slot items
public class QuickSlotItem : VBoxContainer
{
    public int SlotIndex { get; set; }
    public Label KeyLabel { get; set; }
    public Label CountLabel { get; set; }
    public Label NameLabel { get; set; }
    public TextureRect IconTexture { get; set; }
    public Panel BackgroundPanel { get; set; }
    public StyleBoxFlat StyleBox { get; set; }
}
