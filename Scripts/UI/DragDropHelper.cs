using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Drag and drop helper for inventory items to quick slots
    /// </summary>
    public class DragDropHelper : Control {
        public static DragDropHelper Instance { get; private set; }
        
        private Control _dragPreview;
        private bool _isDragging = false;
        private string _draggedItemId = "";
        private int _draggedQuantity = 0;
        private ItemType _draggedItemType;
        
        public event Action<string, int> OnItemDroppedOnQuickSlot;
        
        public override void _Ready() {
            Instance = this;
            
            // Create drag preview (hidden by default)
            _dragPreview = new Control();
            _dragPreview.CustomMinimumSize = new Vector2(50, 50);
            _dragPreview.SetAnchor(AnchorPresets.Center);
            _dragPreview.Modulate = new Color(1, 1, 1, 0.7f);
            _dragPreview.Visible = false;
            AddChild(_dragPreview);
            
            // Create preview background
            var bg = new Panel();
            bg.SetAnchor(AnchorPresets.FullRect);
            bg.Modulate = new Color(0.2f, 0.6f, 1f, 0.8f);
            
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.2f, 0.6f, 1f, 0.6f);
            style.BorderWidthBottom = 2;
            style.BorderWidthLeft = 2;
            style.BorderWidthRight = 2;
            style.BorderWidthTop = 2;
            style.BorderColor = new Color(0.4f, 0.8f, 1f);
            style.CornerRadiusTopLeft = 5;
            style.CornerRadiusTopRight = 5;
            style.CornerRadiusBottomLeft = 5;
            style.CornerRadiusBottomRight = 5;
            bg.AddThemeStyleboxOverride("panel", style);
            
            _dragPreview.AddChild(bg);
        }
        
        public override void _Input(InputEvent @event) {
            if (!_isDragging) return;
            
            if (@event is InputEventMouseMotion motion) {
                // Update drag preview position
                _dragPreview.Position = motion.Position - _dragPreview.CustomMinimumSize / 2;
            }
            else if (@event is InputEventMouseButton btn && !btn.Pressed && btn.ButtonIndex == MouseButton.Left) {
                // Check if dropped on a quick slot
                CheckQuickSlotDrop(btn.Position);
                EndDrag();
            }
        }
        
        /// <summary>
        /// Start dragging an item
        /// </summary>
        public void StartDrag(string itemId, int quantity, ItemType itemType) {
            if (string.IsNullOrEmpty(itemId)) return;
            
            _isDragging = true;
            _draggedItemId = itemId;
            _draggedQuantity = quantity;
            _draggedItemType = itemType;
            
            // Get item name for preview
            var item = ItemDatabase.Instance?.GetItem(itemId);
            string itemName = item?.Name ?? itemId;
            
            // Update preview label
            foreach (var child in _dragPreview.GetChildren()) {
                if (child is Label label) {
                    label.Text = itemName;
                    return;
                }
            }
            
            var label2 = new Label();
            label2.Text = itemName;
            label2.HorizontalAlignment = HorizontalAlignment.Center;
            label2.VerticalAlignment = VerticalAlignment.Center;
            label2.SetAnchor(AnchorPresets.FullRect);
            _dragPreview.AddChild(label2);
            
            _dragPreview.Visible = true;
            GetTree().CurrentInputMode = InputMode.Mouse;
        }
        
        /// <summary>
        /// End dragging
        /// </summary>
        public void EndDrag() {
            _isDragging = false;
            _draggedItemId = "";
            _draggedQuantity = 0;
            _dragPreview.Visible = false;
        }
        
        /// <summary>
        /// Check if dropped on a quick slot
        /// </summary>
        private void CheckQuickSlotDrop(Vector2 dropPosition) {
            var quickSlotBar = GetTree().GetFirstNodeInGroup("QuickSlotBar") as QuickSlotBar;
            if (quickSlotBar == null) return;
            
            // Get quick slot bar position and check bounds
            var slotBarPos = quickSlotBar.GetGlobalPosition();
            var slotBarSize = quickSlotBar.GetRect().Size;
            
            // Check if within bounds
            if (dropPosition.x >= slotBarPos.x && 
                dropPosition.x <= slotBarPos.x + slotBarSize.x &&
                dropPosition.y >= slotBarPos.y && 
                dropPosition.y <= slotBarPos.y + slotBarSize.y) {
                
                // Calculate which slot
                float slotWidth = slotBarSize.x / QuickSlotSystem.SlotCount;
                int slotIndex = (int)((dropPosition.x - slotBarPos.x) / slotWidth);
                
                if (slotIndex >= 0 && slotIndex < QuickSlotSystem.SlotCount) {
                    // Only consumables can be assigned to quick slots
                    if (_draggedItemType == ItemType.Consumable) {
                        OnItemDroppedOnQuickSlot?.Invoke(_draggedItemId, slotIndex);
                        
                        // Show feedback
                        if (GameMessageSystem.Instance != null) {
                            var item = ItemDatabase.Instance?.GetItem(_draggedItemId);
                            GameMessageSystem.Instance.ShowPositive($"已设置快捷键: {item?.Name}");
                        }
                    } else {
                        if (GameMessageSystem.Instance != null) {
                            GameMessageSystem.Instance.ShowWarning("只能将消耗品放入快捷槽");
                        }
                    }
                }
            }
        }
        
        public bool IsDragging => _isDragging;
        public string DraggedItemId => _draggedItemId;
    }
}
