using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Items;

namespace ClawRPG.Scripts.UI
{
    /// <summary>
    /// Enhanced Inventory UI with filtering, sorting and search
    /// </summary>
    public partial class InventoryUI : Control
    {
        // UI Elements
        private Panel _mainPanel;
        private GridContainer _itemGrid;
        private Label _titleLabel;
        private Button _closeButton;
        private Label _itemInfoLabel;
        private LineEdit _searchBox;
        
        // Filter buttons
        private Button[] _filterButtons;
        private Label _goldLabel;
        private Label _slotsLabel;
        
        // Inventory data
        private Player _player;
        private List<InventorySlot> _displaySlots = new();
        private bool _isVisible = false; 
        
        // Grid settings
        private const int SlotsPerRow = 5;
        private const int TotalSlots = 30;
        
        // Current filter and sort
        private InventoryFilter _currentFilter = InventoryFilter.All;
        private InventorySort _currentSort = InventorySort.None;
        
        // Quality colors
        private readonly Color[] _qualityColors = new Color[]
        {
            new Color(0.7f, 0.7f, 0.7f),   // Common - Gray
            new Color(0.2f, 0.8f, 0.2f),   // Uncommon - Green
            new Color(0.3f, 0.5f, 1.0f),   // Rare - Blue
            new Color(0.6f, 0.3f, 0.9f),   // Epic - Purple
            new Color(1.0f, 0.6f, 0.0f)    // Legendary - Orange
        };
        
        // Drag state
        private float _dragStartTimer = 0;
        private Button _dragSlot = null;
        
        // Selected slot
        private int _selectedSlotIndex = -1;
    }
}
