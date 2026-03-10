using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.UI {
    /// <summary>
    /// Inventory UI - player backpack system
    /// </summary>
    public class InventoryUI : Control
    {
        // UI Elements
        private Panel _mainPanel;
        private GridContainer _itemGrid;
        private Label _titleLabel;
        private Button _closeButton;
        private Label _itemInfoLabel;
        
        // Inventory data
        private Player _player;
        private List<Item> _inventory = new();
        private bool _isVisible = false;
        
        // Grid settings
        private const int SlotsPerRow = 5;
        private const int TotalSlots = 30;
        
        public override void _Ready()
        {
            SetupUI();
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            
            if (_player != null)
            {
                LoadInventory();
            }
            
            Hide();
        }
        
        public override void _Input(InputEvent evt)
        {
            // Toggle with I key
            if (evt is InputEventKey key && key.Pressed && key.Keycode == Key.I)
            {
                ToggleInventory();
            }
        }
        
        private void SetupUI()
        {
            // Main panel
            _mainPanel = new Panel();
            _mainPanel.SetAnchor(AnchorPresets.Center);
            _mainPanel.Position = new Vector2(-300, -250);
            _mainPanel.CustomMinimumSize = new Vector2(600, 500);
            AddChild(_mainPanel);
            
            // Title
            _titleLabel = new Label();
            _titleLabel.Text = "背包";
            _titleLabel.SetAnchor(AnchorPresets.TopLeft);
            _titleLabel.Position = new Vector2(20, 15);
            _titleLabel.AddThemeFontSizeOverride("font_size", 24);
            _titleLabel.AddThemeColorOverride("font_color", new Color(1, 0.85, 0.3));
            _mainPanel.AddChild(_titleLabel);
            
            // Close button
            _closeButton = new Button();
            _closeButton.Text = "X";
            _closeButton.SetAnchor(AnchorPresets.TopRight);
            _closeButton.Position = new Vector2(-40, 10);
            _closeButton.CustomMinimumSize = new Vector2(30, 30);
            _closeButton.Pressed += () => HideInventory();
            _mainPanel.AddChild(_closeButton);
            
            // Item grid
            _itemGrid = new GridContainer();
            _itemGrid.SetAnchor(AnchorPresets.TopLeft);
            _itemGrid.Position = new Vector2(20, 60);
            _itemGrid.CustomMinimumSize = new Vector2(400, 360);
            _itemGrid.Columns = SlotsPerRow;
            _mainPanel.AddChild(_itemGrid);
            
            // Create slot buttons
            for (int i = 0; i < TotalSlots; i++)
            {
                var slot = new Button();
                slot.CustomMinimumSize = new Vector2(70, 70);
                slot.Text = "";
                
                // Style
                var normalStyle = new StyleBoxFlat();
                normalStyle.BgColor = new Color(0.2, 0.2, 0.2);
                normalStyle.BorderWidthBottom = 2;
                normalStyle.BorderColor = new Color(0.4, 0.4, 0.4);
                normalStyle.CornerRadiusTopLeft = 5;
                normalStyle.CornerRadiusTopRight = 5;
                normalStyle.CornerRadiusBottomLeft = 5;
                normalStyle.CornerRadiusBottomRight = 5;
                slot.AddThemeStyleboxOverride("normal", normalStyle);
                
                var hoverStyle = normalStyle.Duplicate() as StyleBoxFlat;
                hoverStyle.BorderColor = new Color(0.6, 0.6, 0.3);
                slot.AddThemeStyleboxOverride("hover", hoverStyle);
                
                slot.SetMeta("slot_index", i);
                slot.Pressed += () => OnSlotPressed(slot);
                
                _itemGrid.AddChild(slot);
            }
            
            // Item info panel
            var infoPanel = new Panel();
            infoPanel.SetAnchor(AnchorPresets.TopRight);
            infoPanel.Position = new Vector2(-170, 60);
            infoPanel.CustomMinimumSize = new Vector2(150, 360);
            _mainPanel.AddChild(infoPanel);
            
            _itemInfoLabel = new Label();
            _itemInfoLabel.SetAnchor(AnchorPresets.FullRect);
            _itemInfoLabel.Position = new Vector2(10, 10);
            _itemInfoLabel.CustomMinimumSize = new Vector2(130, 340);
            _itemInfoLabel.Text = "选择一个物品查看详情";
            _itemInfoLabel.AutowrapMode = TextServer.AutowrapMode.Word;
            _itemInfoLabel.AddThemeFontSizeOverride("font_size", 14);
            infoPanel.AddChild(_itemInfoLabel);
        }
        
        private void LoadInventory()
        {
            // Demo inventory items
            _inventory.Clear();
            
            // Add some starter items
            _inventory.Add(new Weapon { Id = 1, Name = "新手剑", Description = "一把基础的剑", Value = 10, Damage = 5 });
            _inventory.Add(new Consumable { Id = 101, Name = "生命药水", Description = "恢复50点生命值", Value = 20, HealthRestore = 50 });
            _inventory.Add(new Consumable { Id = 102, Name = "魔法药水", Description = "恢复30点魔法值", Value = 20, ManaRestore = 30 });
            _inventory.Add(new Material { Id = 201, Name = "铁矿石", Description = "用于锻造", Value = 5 });
            _inventory.Add(new Material { Id = 202, Name = "木材", Description = "基础材料", Value = 3 });
            
            UpdateGrid();
        }
        
        private void UpdateGrid()
        {
            var slots = _itemGrid.GetChildren();
            
            for (int i = 0; i < slots.Count; i++)
            {
                var slot = slots[i] as Button;
                if (slot == null) continue;
                
                if (i < _inventory.Count && _inventory[i] != null)
                {
                    var item = _inventory[i];
                    slot.Text = item.Name;
                    slot.TooltipText = item.Description;
                }
                else
                {
                    slot.Text = "";
                    slot.TooltipText = "";
                }
            }
        }
        
        private void OnSlotPressed(Button slot)
        {
            int index = (int)slot.GetMeta("slot_index");
            
            if (index < _inventory.Count && _inventory[index] != null)
            {
                var item = _inventory[index];
                _itemInfoLabel.Text = $"名称: {item.Name}\n\n类型: {item.Type}\n\n描述: {item.Description}\n\n价格: {item.Value}";
            }
            else
            {
                _itemInfoLabel.Text = "空槽位";
            }
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
        }
        
        private void HideInventory()
        {
            _isVisible = false;
            _mainPanel.Visible = false;
        }
    }
    
    /// <summary>
    /// Material item type
    /// </summary>
    public class Material : Item
    {
        public Material()
        {
            Type = ItemType.Material;
        }
    }
}
