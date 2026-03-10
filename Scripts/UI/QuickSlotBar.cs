using Godot;
using System;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Quick slot bar UI - displays 9 quick access slots at the bottom of the screen
    /// </summary>
    public class QuickSlotBar : Control {
        private HBoxContainer _slotContainer;
        private Label _titleLabel;
        private QuickSlotItem[] _slotItems = new QuickSlotItem[QuickSlotSystem.SlotCount];
        
        public override void _Ready() {
            // Add to QuickSlotBar group for drag-drop detection
            AddToGroup("QuickSlotBar");
            
            // Create main container
            var mainContainer = new VBoxContainer();
            mainContainer.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
            mainContainer.Position = new Vector2(0, -80);
            mainContainer.Size = new Vector2(1152, 80);
            mainContainer.Alignment = BoxContainer.AlignmentMode.Center;
            AddChild(mainContainer);
            
            // Title label
            _titleLabel = new Label();
            _titleLabel.Text = "快速槽 [1-9]";
            _titleLabel.Align = Label.AlignEnum.Center;
            _titleLabel.AddColorOverride("font_color", new Color(1, 0.84f, 0)); // Gold color
            mainContainer.AddChild(_titleLabel);
            
            // Slot container
            _slotContainer = new HBoxContainer();
            _slotContainer.Alignment = BoxContainer.AlignmentMode.Center;
            _slotContainer.Spacing = 8;
            mainContainer.AddChild(_slotContainer);
            
            // Create 9 slot items
            for (int i = 0; i < QuickSlotSystem.SlotCount; i++) {
                var slot = new QuickSlotItem(i);
                _slotContainer.AddChild(slot);
                _slotItems[i] = slot;
            }
            
            // Subscribe to slot updates
            if (QuickSlotSystem.Instance != null) {
                QuickSlotSystem.Instance.OnSlotUpdated += OnSlotUpdated;
                QuickSlotSystem.Instance.OnSlotUsed += OnSlotUsed;
            }
            
            // Connect to inventory updates to auto-refresh
            if (InventoryManager.Instance != null) {
                InventoryManager.Instance.OnInventoryUpdated += OnInventoryUpdated;
            }
        }
        
        private void OnSlotUpdated(int slotIndex, string itemId, int quantity) {
            if (slotIndex >= 0 && slotIndex < QuickSlotSystem.SlotCount) {
                _slotItems[slotIndex].UpdateSlot(itemId, quantity);
            }
        }
        
        private void OnSlotUsed(int slotIndex) {
            if (slotIndex >= 0 && slotIndex < QuickSlotSystem.SlotCount) {
                _slotItems[slotIndex].PlayUseAnimation();
            }
        }
        
        private void OnInventoryUpdated() {
            // Auto-refresh slots when inventory changes
            QuickSlotSystem.Instance?.AutoFillSlots();
        }
        
        public override void _ExitTree() {
            if (QuickSlotSystem.Instance != null) {
                QuickSlotSystem.Instance.OnSlotUpdated -= OnSlotUpdated;
                QuickSlotSystem.Instance.OnSlotUsed -= OnSlotUsed;
            }
            
            if (InventoryManager.Instance != null) {
                InventoryManager.Instance.OnInventoryUpdated -= OnInventoryUpdated;
            }
        }
    }
    
    /// <summary>
    /// Individual quick slot item display
    /// </summary>
    public class QuickSlotItem : Control {
        private int _slotIndex;
        private Panel _slotPanel;
        private Label _keyLabel;
        private Label _itemNameLabel;
        private Label _quantityLabel;
        private TextureRect _iconRect;
        private Color _slotColor = new Color(0.2f, 0.2f, 0.2f, 0.8f);
        private Color _emptyColor = new Color(0.1f, 0.1f, 0.1f, 0.5f);
        
        public QuickSlotItem(int slotIndex) {
            _slotIndex = slotIndex;
            CustomMinimumSize = new Vector2(100, 60);
            
            // Slot panel background
            _slotPanel = new Panel();
            _slotPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
            _slotPanel.Modulate = _emptyColor;
            AddChild(_slotPanel);
            
            // Key number label (top-left)
            _keyLabel = new Label();
            _keyLabel.Text = (slotIndex + 1).ToString();
            _keyLabel.Position = new Vector2(5, 2);
            _keyLabel.AddColorOverride("font_color", new Color(1, 1, 1, 0.7f));
            _keyLabel.AddColorOverride("font_color_shadow", new Color(0, 0, 0, 0.8f));
            AddChild(_keyLabel);
            
            // Item icon (center)
            _iconRect = new TextureRect();
            _iconRect.SetAnchorsPreset(Control.LayoutPreset.Center);
            _iconRect.Position = new Vector2(-16, 8);
            _iconRect.Size = new Vector2(32, 32);
            _iconRect.StretchMode = TextureRect.StretchModeEnum.KeepAspectCentered;
            AddChild(_iconRect);
            
            // Item name label (bottom)
            _itemNameLabel = new Label();
            _itemNameLabel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
            _itemNameLabel.Position = new Vector2(5, -18);
            _itemNameLabel.Size = new Vector2(90, 16);
            _itemNameLabel.Align = Label.AlignEnum.Center;
            _itemNameLabel.AddColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
            _itemNameLabel.AddColorOverride("font_color_shadow", new Color(0, 0, 0, 0.8f));
            AddChild(_itemNameLabel);
            
            // Quantity label (bottom-right)
            _quantityLabel = new Label();
            _quantityLabel.SetAnchorsPreset(Control.LayoutPreset.BottomRight);
            _quantityLabel.Position = new Vector2(-25, -16);
            _quantityLabel.Size = new Vector2(20, 14);
            _quantityLabel.Align = Label.AlignEnum.Right;
            _quantityLabel.AddColorOverride("font_color", new Color(1, 1, 1));
            _quantityLabel.AddColorOverride("font_color_shadow", new Color(0, 0, 0, 0.8f));
            AddChild(_quantityLabel);
            
            // Style the panel
            var style = new StyleBoxFlat();
            style.BgColor = _emptyColor;
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.BorderWidthBottom = 2;
            style.BorderColor = new Color(0.3f, 0.3f, 0.3f, 0.5f);
            style.CornerRadiusTopLeft = 4;
            style.CornerRadiusTopRight = 4;
            style.CornerRadiusBottomLeft = 4;
            style.CornerRadiusBottomRight = 4;
            _slotPanel.AddThemeStyleboxOverride("panel", style);
        }
        
        public void UpdateSlot(string itemId, int quantity) {
            if (string.IsNullOrEmpty(itemId) || quantity <= 0) {
                // Empty slot
                _itemNameLabel.Text = "";
                _quantityLabel.Text = "";
                _iconRect.Texture = null;
                _slotPanel.Modulate = _emptyColor;
                return;
            }
            
            var item = ItemDatabase.Instance?.GetItem(itemId);
            if (item == null) {
                _itemNameLabel.Text = itemId;
                _quantityLabel.Text = quantity.ToString();
                return;
            }
            
            // Update display
            _itemNameLabel.Text = item.Name;
            _quantityLabel.Text = quantity > 1 ? quantity.ToString() : "";
            
            // Set color based on item quality
            Color qualityColor = GetQualityColor(item.Quality);
            _slotPanel.Modulate = new Color(qualityColor.R * 0.3f, qualityColor.G * 0.3f, qualityColor.B * 0.3f, 0.9f);
            
            // Update border color
            var style = _slotPanel.GetThemeStylebox("panel") as StyleBoxFlat;
            if (style != null) {
                style.BorderColor = qualityColor;
            }
            
            // Load item icon if available (use placeholder colors for now)
            UpdateIcon(item);
        }
        
        private void UpdateIcon(Item item) {
            // For now, create a colored placeholder based on item type
            // In a real game, you'd load actual item icons
            _iconRect.Modulate = GetItemTypeColor(item.Type);
        }
        
        private Color GetQualityColor(ItemQuality quality) {
            switch (quality) {
                case ItemQuality.Common: return new Color(0.7f, 0.7f, 0.7f);
                case ItemQuality.Uncommon: return new Color(0.2f, 0.8f, 0.2f);
                case ItemQuality.Rare: return new Color(0.2f, 0.5f, 1f);
                case ItemQuality.Epic: return new Color(0.6f, 0.3f, 0.9f);
                case ItemQuality.Legendary: return new Color(1f, 0.6f, 0f);
                default: return new Color(0.7f, 0.7f, 0.7f);
            }
        }
        
        private Color GetItemTypeColor(ItemType type) {
            switch (type) {
                case ItemType.Consumable: return new Color(1f, 0.3f, 0.3f);
                case ItemType.Weapon: return new Color(1f, 0.8f, 0.2f);
                case ItemType.Armor: return new Color(0.3f, 0.5f, 1f);
                case ItemType.Accessory: return new Color(0.8f, 0.3f, 0.8f);
                case ItemType.Material: return new Color(0.5f, 0.5f, 0.5f);
                case ItemType.Quest: return new Color(1f, 1f, 0.2f);
                default: return new Color(1f, 1f, 1f);
            }
        }
        
        public void PlayUseAnimation() {
            // Flash animation when item is used
            var tween = CreateTween();
            tween.TweenProperty(_slotPanel, "modulate:a", 0.3f, 0.1f);
            tween.TweenProperty(_slotPanel, "modulate:a", 1f, 0.1f);
        }
    }
}
