using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.PetEquipment;

/// <summary>
/// UI for pet equipment system
/// </summary>
public partial class PetEquipmentUI : Control
{
    private VBoxContainer _mainContainer;
    private HBoxContainer _tabsContainer;
    private TabContainer _tabContainer;
    
    // Equipment list
    private GridContainer _equipmentGrid;
    private Label _equipmentCountLabel;
    
    // Equipped slots
    private VBoxContainer _equippedContainer;
    private Label _bonusesLabel;
    
    // Stats
    private VBoxContainer _statsContainer;
    private Label _rarityDistLabel;
    
    // Selected pet
    private int _selectedPetId = 0;
    private PetEquipmentData _selectedEquipment;
    
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        SetupUI();
        Visible = _isVisible;
    }
    
    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchor(AnchorPreset.FullRect);
        _mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(_mainContainer);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "🐾 Pet Equipment";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(titleLabel);
        
        // Tabs
        _tabsContainer = new HBoxContainer();
        _tabsContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _tabsContainer.AddThemeConstantOverride("separation", 10);
        _mainContainer.AddChild(_tabsContainer);
        
        CreateTabButton("Inventory", 0);
        CreateTabButton("Equipped", 1);
        CreateTabButton("Statistics", 2);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _mainContainer.AddChild(_tabContainer);
        
        // Tab 0: Inventory
        var inventoryTab = new ScrollContainer();
        _tabContainer.AddChild(inventoryTab);
        _tabContainer.SetTabTitle(inventoryTab, "Inventory");
        
        var inventoryVBox = new VBoxContainer();
        inventoryVBox.SetAnchor(AnchorPreset.FullRect);
        inventoryVBox.AddThemeConstantOverride("separation", 5);
        inventoryTab.AddChild(inventoryVBox);
        
        _equipmentCountLabel = new Label();
        _equipmentCountLabel.Text = "Equipment: 0";
        inventoryVBox.AddChild(_equipmentCountLabel);
        
        _equipmentGrid = new GridContainer();
        _equipmentGrid.Columns = 4;
        _equipmentGrid.AddThemeConstantOverride("h_separation", 5);
        _equipmentGrid.AddThemeConstantOverride("v_separation", 5);
        inventoryVBox.AddChild(_equipmentGrid);
        
        // Tab 1: Equipped
        _equippedContainer = new VBoxContainer();
        _tabContainer.AddChild(_equippedContainer);
        _tabContainer.SetTabTitle(_equippedContainer, "Equipped");
        
        var petSelectLabel = new Label();
        petSelectLabel.Text = "Select Pet ID:";
        _equippedContainer.AddChild(petSelectLabel);
        
        var petSpinBox = new SpinBox();
        petSpinBox.MinValue = 0;
        petSpinBox.MaxValue = 100;
        petSpinBox.Value = _selectedPetId;
        petSpinBox.ValueChanged += (val) => { _selectedPetId = (int)val; RefreshEquipped(); };
        _equippedContainer.AddChild(petSpinBox);
        
        var slotsLabel = new Label();
        slotsLabel.Text = "Equipped Slots:";
        slotsLabel.AddThemeFontSizeOverride("font_size", 18);
        _equippedContainer.AddChild(slotsLabel);
        
        // Create slot displays
        string[] slotNames = { "Collar", "Necklace", "Harness", "Accessory", "Toy", "Treat" };
        for (int i = 0; i < 6; i++)
        {
            var slotContainer = new HBoxContainer();
            _equippedContainer.AddChild(slotContainer);
            
            var slotLabel = new Label();
            slotLabel.Text = slotNames[i] + ": ";
            slotLabel.CustomMinimumSize = new Vector2(100, 0);
            slotContainer.AddChild(slotLabel);
            
            var slotButton = new Button();
            slotButton.Text = "Empty";
            slotButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
            int slotIdx = i;
            slotButton.Pressed += () => OnSlotPressed(slotIdx);
            slotContainer.AddChild(slotButton);
        }
        
        var bonusesTitle = new Label();
        bonusesTitle.Text = "Active Bonuses:";
        bonusesTitle.AddThemeFontSizeOverride("font_size", 18);
        _equippedContainer.AddChild(bonusesTitle);
        
        _bonusesLabel = new Label();
        _bonusesLabel.Text = "No bonuses active";
        _equippedContainer.AddChild(_bonusesLabel);
        
        // Tab 2: Statistics
        _statsContainer = new VBoxContainer();
        _tabContainer.AddChild(_statsContainer);
        _tabContainer.SetTabTitle(_statsContainer, "Statistics");
        
        var statsTitle = new Label();
        statsTitle.Text = "Equipment Statistics";
        statsTitle.AddThemeFontSizeOverride("font_size", 20);
        _statsContainer.AddChild(statsTitle);
        
        _rarityDistLabel = new Label();
        _rarityDistLabel.Text = "Rarity Distribution:";
        _statsContainer.AddChild(_rarityDistLabel);
        
        var generateButton = new Button();
        generateButton.Text = "🎲 Generate Random Equipment";
        generateButton.Pressed += OnGeneratePressed;
        _mainContainer.AddChild(generateButton);
        
        var hintLabel = new Label();
        hintLabel.Text = "Press P or ESC to close";
        hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        hintLabel.AddThemeFontSizeOverride("font_size", 14);
        _mainContainer.AddChild(hintLabel);
    }
    
    private void CreateTabButton(string text, int tabIndex)
    {
        var button = new Button();
        button.Text = text;
        button.Pressed += () => { _tabContainer.CurrentTab = tabIndex; };
        _tabsContainer.AddChild(button);
    }
    
    private void OnSlotPressed(int slotIndex)
    {
        var system = PetEquipmentSystem.Instance;
        if (system == null) return;
        
        // Check if something is equipped in this slot
        if (EquippedSlots.ContainsKey(_selectedPetId))
        {
            var slots = EquippedSlots[_selectedPetId];
            if (slots[slotIndex] != -1)
            {
                // Unequip
                system.UnequipEquipment(slots[slotIndex], _selectedPetId);
                RefreshEquipped();
            }
        }
    }
    
    private void OnGeneratePressed()
    {
        var system = PetEquipmentSystem.Instance;
        if (system == null) return;
        
        int equipmentId = system.GenerateRandomEquipment();
        if (equipmentId > 0)
        {
            system.AddEquipment(equipmentId);
            RefreshInventory();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if ((keyEvent.Keycode == Key.P && keyEvent.ShiftPressed) || keyEvent.Keycode == Key.Escape)
            {
                ToggleVisibility();
            }
        }
    }
    
    public void ToggleVisibility()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            RefreshAll();
        }
    }
    
    private void RefreshAll()
    {
        RefreshInventory();
        RefreshEquipped();
        RefreshStats();
    }
    
    private void RefreshInventory()
    {
        var system = PetEquipmentSystem.Instance;
        if (system == null) return;
        
        _equipmentCountLabel.Text = $"Equipment: {system.TotalEquipmentOwned}";
        
        // Clear grid
        foreach (var child in _equipmentGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add equipment items
        foreach (var eq in system.OwnedEquipment.Values)
        {
            var button = new Button();
            button.CustomMinimumSize = new Vector2(100, 60);
            
            var color = PetEquipmentSystem.GetRarityColor(eq.Rarity);
            button.Modulate = color;
            
            button.Text = $"{eq.Name}\n{eq.Type}";
            button.TooltipText = $"{eq.Description}\n\nStats: ATK+{eq.AttackBonus} DEF+{eq.DefenseBonus} HP+{eq.HealthBonus}";
            
            int eqId = eq.Id;
            button.Pressed += () => { OnEquipmentClicked(eqId); };
            
            _equipmentGrid.AddChild(button);
        }
    }
    
    private void OnEquipmentClicked(int equipmentId)
    {
        var system = PetEquipmentSystem.Instance;
        if (system == null || !system.OwnedEquipment.ContainsKey(equipmentId)) return;
        
        var eq = system.OwnedEquipment[equipmentId];
        
        if (eq.IsEquipped)
        {
            // Unequip
            system.UnequipEquipment(equipmentId, _selectedPetId);
        }
        else
        {
            // Equip
            system.EquipEquipment(equipmentId, _selectedPetId);
        }
        
        RefreshAll();
    }
    
    private Dictionary<int, List<int>> EquippedSlots => PetEquipmentSystem.Instance?.EquippedSlots;
    
    private void RefreshEquipped()
    {
        var system = PetEquipmentSystem.Instance;
        if (system == null) return;
        
        // Get slot children (starting from index 3, after labels and spinbox)
        var slotButtons = new List<Button>();
        foreach (var child in _equippedContainer.GetChildren())
        {
            if (child is HBoxContainer hbox)
            {
                foreach (var btn in hbox.GetChildren())
                {
                    if (btn is Button b && b.Text != "Empty")
                    {
                        // Check if it's a slot button
                        if (b.SizeFlagsHorizontal == Control.SizeFlags.ExpandFill)
                        {
                            slotButtons.Add(b);
                        }
                    }
                }
            }
        }
        
        // Actually let's just rebuild
        var boxes = new List<HBoxContainer>();
        foreach (var child in _equippedContainer.GetChildren())
        {
            if (child is HBoxContainer hb && hb.GetChildCount() > 1 && hb.GetChild(1) is Button)
            {
                boxes.Add(hb);
            }
        }
        
        string[] slotNames = { "Collar", "Necklace", "Harness", "Accessory", "Toy", "Treat" };
        
        for (int i = 0; i < Math.Min(boxes.Count, 6); i++)
        {
            var hbox = boxes[i];
            var button = hbox.GetChild<Button>(1);
            
            if (EquippedSlots.ContainsKey(_selectedPetId) && EquippedSlots[_selectedPetId][i] != -1)
            {
                int eqId = EquippedSlots[_selectedPetId][i];
                if (system.OwnedEquipment.ContainsKey(eqId))
                {
                    var eq = system.OwnedEquipment[eqId];
                    button.Text = eq.Name;
                    button.Modulate = PetEquipmentSystem.GetRarityColor(eq.Rarity);
                }
            }
            else
            {
                button.Text = "Empty";
                button.Modulate = Colors.Gray;
            }
        }
        
        // Update bonuses
        var bonuses = system.CalculateBonuses(_selectedPetId);
        string bonusText = "Active Bonuses:\n";
        foreach (var kvp in bonuses)
        {
            if (kvp.Value != 0)
            {
                bonusText += $"{kvp.Key}: +{kvp.Value:F1}\n";
            }
        }
        _bonusesLabel.Text = bonusText;
    }
    
    private void RefreshStats()
    {
        var system = PetEquipmentSystem.Instance;
        if (system == null) return;
        
        var rarityDist = system.GetRarityDistribution();
        string statsText = "Rarity Distribution:\n";
        foreach (var kvp in rarityDist)
        {
            var color = PetEquipmentSystem.GetRarityColor(kvp.Key);
            statsText += $"{kvp.Key}: {kvp.Value}\n";
        }
        
        statsText += $"\nTotal Equipment: {system.TotalEquipmentOwned}\n";
        statsText += $"Equipped Slots: {system.TotalEquipSlotsUsed}";
        
        _rarityDistLabel.Text = statsText;
    }
}
