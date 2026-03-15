using Godot;
using System;
using System.Collections.Generic;

public class PetInventoryUI : Control {
    private PetInventorySystem _system;
    private PetInventoryDatabase _database;
    
    // UI Elements
    private Label _titleLabel;
    private Label _petNameLabel;
    private Label _goldLabel;
    private Label _slotsLabel;
    private ItemGrid _itemGrid;
    private ItemDetailPanel _detailPanel;
    private TabContainer _tabContainer;
    
    // Filters
    private OptionButton _categoryFilter;
    private OptionButton _rarityFilter;
    private LineEdit _searchEdit;
    
    private string _currentPetId = "default_pet";
    private string _selectedItemId = "";
    
    public override void _Ready() {
        base._Ready();
        SetupUI();
        InitializeSystem();
    }
    
    private void SetupUI() {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.WideFullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // Title bar
        var titleBar = new HBoxContainer();
        mainContainer.AddChild(titleBar);
        
        _titleLabel = new Label();
        _titleLabel.Text = "Pet Inventory";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleBar.AddChild(_titleLabel);
        
        titleBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        // Pet selector
        var petLabel = new Label();
        petLabel.Text = "Pet: ";
        titleBar.AddChild(petLabel);
        
        var petSelector = new OptionButton();
        petSelector.AddItem("Default Pet", 0);
        petSelector.Select(0);
        petSelector.ItemSelected += OnPetSelected;
        titleBar.AddChild(petSelector);
        
        // Info bar
        var infoBar = new HBoxContainer();
        mainContainer.AddChild(infoBar);
        
        _petNameLabel = new Label();
        _petNameLabel.Text = "Pet: Default Pet";
        infoBar.AddChild(_petNameLabel);
        
        infoBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        _goldLabel = new Label();
        _goldLabel.Text = "Gold: 0";
        _goldLabel.AddThemeColorOverride("font_color", new Color(1, 0.84f, 0));
        infoBar.AddChild(_goldLabel);
        
        infoBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        _slotsLabel = new Label();
        _slotsLabel.Text = "Slots: 0/50";
        infoBar.AddChild(_slotsLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        mainContainer.AddChild(_tabContainer);
        
        // Inventory tab
        var inventoryTab = new VBoxContainer();
        inventoryTab.Name = "Inventory";
        _tabContainer.AddChild(inventoryTab);
        
        // Filters
        var filterBar = new HBoxContainer();
        filterBar.AddThemeConstantOverride("separation", 10);
        inventoryTab.AddChild(filterBar);
        
        var searchLabel = new Label();
        searchLabel.Text = "Search:";
        filterBar.AddChild(searchLabel);
        
        _searchEdit = new LineEdit();
        _searchEdit.PlaceholderText = "Search items...";
        _searchEdit.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        _searchEdit.TextChanged += OnSearchChanged;
        filterBar.AddChild(_searchEdit);
        
        var categoryLabel = new Label();
        categoryLabel.Text = "Category:";
        filterBar.AddChild(categoryLabel);
        
        _categoryFilter = new OptionButton();
        _categoryFilter.AddItem("All", 0);
        _categoryFilter.AddItem("Consumable", 1);
        _categoryFilter.AddItem("Equipment", 2);
        _categoryFilter.AddItem("Material", 3);
        _categoryFilter.AddItem("Special", 4);
        _categoryFilter.Select(0);
        _categoryFilter.ItemSelected += OnCategoryChanged;
        filterBar.AddChild(_categoryFilter);
        
        var rarityLabel = new Label();
        rarityLabel.Text = "Rarity:";
        filterBar.AddChild(rarityLabel);
        
        _rarityFilter = new OptionButton();
        _rarityFilter.AddItem("All", 0);
        _rarityFilter.AddItem("Common", 1);
        _rarityFilter.AddItem("Uncommon", 2);
        _rarityFilter.AddItem("Rare", 3);
        _rarityFilter.AddItem("Epic", 4);
        _rarityFilter.AddItem("Legendary", 5);
        _rarityFilter.Select(0);
        _rarityFilter.ItemSelected += OnRarityChanged;
        filterBar.AddChild(_rarityFilter);
        
        // Item grid container
        var gridContainer = new HBoxContainer();
        gridContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        inventoryTab.AddChild(gridContainer);
        
        // Item grid (scrollable)
        var scrollContainer = new ScrollContainer();
        scrollContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        scrollContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        gridContainer.AddChild(scrollContainer);
        
        _itemGrid = new ItemGrid();
        _itemGrid.CustomMinimumSize = new Vector2(400, 300);
        _itemGrid.ItemSelected += OnItemSelected;
        scrollContainer.AddChild(_itemGrid);
        
        // Detail panel
        _detailPanel = new ItemDetailPanel();
        _detailPanel.CustomMinimumSize = new Vector2(250, 0);
        _detailPanel.ItemUseRequested += OnItemUseRequested;
        _detailPanel.ItemDropRequested += OnItemDropRequested;
        gridContainer.AddChild(_detailPanel);
        
        // Statistics tab
        var statsTab = new VBoxContainer();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);
        
        var statsLabel = new Label();
        statsLabel.Text = "Inventory Statistics";
        statsLabel.AddThemeFontSizeOverride("font_size", 18);
        statsLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.8f, 1));
        statsTab.AddChild(statsLabel);
        
        var statsScroll = new ScrollContainer();
        statsScroll.SizeFlagsVertical = Control.SizeFlags.Expand;
        statsTab.AddChild(statsScroll);
        
        var statsVBox = new VBoxContainer();
        statsVBox.AddThemeConstantOverride("separation", 5);
        statsScroll.AddChild(statsVBox);
        
        // Test buttons
        var testBar = new HBoxContainer();
        testBar.AddThemeConstantOverride("separation", 10);
        mainContainer.AddChild(testBar);
        
        var testAddBtn = new Button();
        testAddBtn.Text = "Add Random Item";
        testAddBtn.Pressed += OnAddRandomItem;
        testBar.AddChild(testAddBtn);
        
        var testGoldBtn = new Button();
        testGoldBtn.Text = "Add 1000 Gold";
        testGoldBtn.Pressed += OnAddGold;
        testBar.AddChild(testGoldBtn);
        
        var testClearBtn = new Button();
        testClearBtn.Text = "Clear Inventory";
        testClearBtn.Pressed += OnClearInventory;
        testBar.AddChild(testClearBtn);
        
        var closeBtn = new Button();
        closeBtn.Text = "Close (ESC)";
        closeBtn.Pressed += () => Hide();
        testBar.AddChild(closeBtn);
    }
    
    private void InitializeSystem() {
        _system = new PetInventorySystem();
        _system.Name = "PetInventorySystem";
        GetTree().CurrentScene.AddChild(_system);
        
        _database = new PetInventoryDatabase();
        _database.Name = "PetInventoryDatabase";
        GetTree().CurrentScene.AddChild(_database);
        
        _system.ItemAdded += OnItemAdded;
        _system.ItemRemoved += OnItemRemoved;
        _system.ItemUsed += OnItemUsed;
        _system.GoldChanged += OnGoldChanged;
        
        RefreshInventory();
    }
    
    private void RefreshInventory() {
        var items = _system.GetInventory(_currentPetId);
        _itemGrid.SetItems(items);
        
        int gold = _system.GetGold(_currentPetId);
        int slotCount = _system.GetInventorySize(_currentPetId);
        int maxSlots = _system.GetMaxSlots();
        
        _goldLabel.Text = "Gold: " + gold;
        _slotsLabel.Text = "Slots: " + slotCount + "/" + maxSlots;
        
        var summary = _system.GetPetInventorySummary(_currentPetId);
        // Update detail panel if item selected
        if (!string.IsNullOrEmpty(_selectedItemId)) {
            var item = _system.GetItem(_currentPetId, _selectedItemId);
            if (item != null) {
                _detailPanel.SetItem(item, _database);
            }
        }
    }
    
    private void OnPetSelected(long index) {
        _currentPetId = "pet_" + index;
        _petNameLabel.Text = "Pet: " + (_currentPetId == "pet_0" ? "Default Pet" : "Pet " + index);
        _selectedItemId = "";
        RefreshInventory();
    }
    
    private void OnItemSelected(string itemId) {
        _selectedItemId = itemId;
        var item = _system.GetItem(_currentPetId, itemId);
        if (item != null) {
            _detailPanel.SetItem(item, _database);
        }
    }
    
    private void OnSearchChanged(string text) {
        _itemGrid.SetSearchFilter(text);
    }
    
    private void OnCategoryChanged(long index) {
        string[] categories = { "All", "Consumable", "Equipment", "Material", "Special" };
        _itemGrid.SetCategoryFilter(categories[index]);
    }
    
    private void OnRarityChanged(long index) {
        string[] rarities = { "All", "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        _itemGrid.SetRarityFilter(rarities[index]);
    }
    
    private void OnItemAdded(string petId, string itemId, int quantity) {
        GD.Print("[PetInventoryUI] Item added: " + itemId + " x" + quantity);
        RefreshInventory();
    }
    
    private void OnItemRemoved(string petId, string itemId, int quantity) {
        GD.Print("[PetInventoryUI] Item removed: " + itemId + " x" + quantity);
        RefreshInventory();
    }
    
    private void OnItemUsed(string petId, string itemId, Dictionary<string, float> effects) {
        GD.Print("[PetInventoryUI] Item used: " + itemId + " with effects: " + effects.Count);
        RefreshInventory();
    }
    
    private void OnGoldChanged(string petId, int newAmount) {
        _goldLabel.Text = "Gold: " + newAmount;
    }
    
    private void OnItemUseRequested(string itemId) {
        if (_system.UseItem(_currentPetId, itemId)) {
            GD.Print("[PetInventoryUI] Used item: " + itemId);
        }
    }
    
    private void OnItemDropRequested(string itemId) {
        if (_system.RemoveItem(_currentPetId, itemId, 1)) {
            GD.Print("[PetInventoryUI] Dropped item: " + itemId);
        }
    }
    
    private void OnAddRandomItem() {
        string[] itemIds = { "health_potion", "pet_food_basic", "pet_collar_common", "pet_ticket", "luck_charm" };
        Random rand = new Random();
        string randomItem = itemIds[rand.Next(itemIds.Length)];
        _system.AddItem(_currentPetId, randomItem, rand.Next(1, 4));
    }
    
    private void OnAddGold() {
        _system.AddGold(_currentPetId, 1000);
    }
    
    private void OnClearInventory() {
        _system.ClearInventory(_currentPetId);
        _selectedItemId = "";
        RefreshInventory();
    }
    
    public override void _Input(InputEvent @event) {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed) {
            if (keyEvent.Keycode == Key.Escape) {
                Hide();
            }
        }
    }
}

// Item Grid Control
public class ItemGrid : GridContainer {
    private List<PetInventoryItem> _allItems = new List<PetInventoryItem>();
    private List<PetInventoryItem> _filteredItems = new List<PetInventoryItem>();
    private string _searchFilter = "";
    private string _categoryFilter = "All";
    private string _rarityFilter = "All";
    
    public signal ItemSelected(string itemId);
    
    public ItemGrid() {
        Columns = 5;
        AddThemeConstantOverride("h_separation", 5);
        AddThemeConstantOverride("v_separation", 5);
    }
    
    public void SetItems(List<PetInventoryItem> items) {
        _allItems = items;
        ApplyFilters();
    }
    
    public void SetSearchFilter(string text) {
        _searchFilter = text.ToLower();
        ApplyFilters();
    }
    
    public void SetCategoryFilter(string category) {
        _categoryFilter = category;
        ApplyFilters();
    }
    
    public void SetRarityFilter(string rarity) {
        _rarityFilter = rarity;
        ApplyFilters();
    }
    
    private void ApplyFilters() {
        _filteredItems.Clear();
        
        foreach (var item in _allItems) {
            bool match = true;
            
            // Search filter
            if (!string.IsNullOrEmpty(_searchFilter)) {
                if (!item.ItemName.ToLower().Contains(_searchFilter) && 
                    !item.Description.ToLower().Contains(_searchFilter)) {
                    match = false;
                }
            }
            
            // Category filter
            if (_categoryFilter != "All" && item.Category != _categoryFilter) {
                match = false;
            }
            
            // Rarity filter
            if (_rarityFilter != "All" && item.Rarity != _rarityFilter) {
                match = false;
            }
            
            if (match) {
                _filteredItems.Add(item);
            }
        }
        
        RefreshDisplay();
    }
    
    private void RefreshDisplay() {
        // Clear existing children
        foreach (Node child in GetChildren()) {
            child.QueueFree();
        }
        
        // Create item buttons
        foreach (var item in _filteredItems) {
            var btn = new Button();
            btn.CustomMinimumSize = new Vector2(60, 60);
            btn.Text = item.Quantity > 1 ? item.ItemName + "\n(x" + item.Quantity + ")" : item.ItemName;
            btn.TooltipText = item.Description + "\n\nCategory: " + item.Category + "\nRarity: " + item.Rarity + "\nValue: " + item.Value;
            
            // Color by rarity
            Color rarityColor = Colors.White;
            switch (item.Rarity) {
                case "Uncommon": rarityColor = Colors.Green; break;
                case "Rare": rarityColor = Colors.Blue; break;
                case "Epic": rarityColor = new Color(0.63f, 0.21f, 0.93f); break;
                case "Legendary": rarityColor = new Color(1f, 0.5f, 0); break;
            }
            btn.Modulate = rarityColor;
            
            btn.Pressed += () => ItemSelected?.Invoke(item.ItemId);
            AddChild(btn);
        }
        
        // Empty state
        if (_filteredItems.Count == 0) {
            var emptyLabel = new Label();
            emptyLabel.Text = "No items found";
            emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            AddChild(emptyLabel);
        }
    }
}

// Item Detail Panel
public class ItemDetailPanel : VBoxContainer {
    private PetInventoryItem _currentItem;
    private PetInventoryDatabase _database;
    
    private Label _nameLabel;
    private Label _descriptionLabel;
    private Label _categoryLabel;
    private Label _rarityLabel;
    private Label _quantityLabel;
    private Label _valueLabel;
    private Label _statsLabel;
    private Button _useButton;
    private Button _dropButton;
    
    public signal ItemUseRequested(string itemId);
    public signal ItemDropRequested(string itemId);
    
    public ItemDetailPanel() {
        AddThemeConstantOverride("separation", 10);
        
        _nameLabel = new Label();
        _nameLabel.AddThemeFontSizeOverride("font_size", 18);
        AddChild(_nameLabel);
        
        _descriptionLabel = new Label();
        _descriptionLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        AddChild(_descriptionLabel);
        
        AddChild(new HSeparator());
        
        _categoryLabel = new Label();
        AddChild(_categoryLabel);
        
        _rarityLabel = new Label();
        AddChild(_rarityLabel);
        
        _quantityLabel = new Label();
        AddChild(_quantityLabel);
        
        _valueLabel = new Label();
        AddChild(_valueLabel);
        
        AddChild(new HSeparator());
        
        _statsLabel = new Label();
        _statsLabel.AutowrapMode = TextServer.AutowrapMode.WordSmart;
        AddChild(_statsLabel);
        
        AddChild(new Control { SizeFlagsVertical = Control.SizeFlags.Expand });
        
        _useButton = new Button();
        _useButton.Text = "Use Item";
        _useButton.Pressed += OnUsePressed;
        AddChild(_useButton);
        
        _dropButton = new Button();
        _dropButton.Text = "Drop Item";
        _dropButton.Pressed += OnDropPressed;
        AddChild(_dropButton);
        
        ClearDisplay();
    }
    
    public void SetItem(PetInventoryItem item, PetInventoryDatabase database) {
        _currentItem = item;
        _database = database;
        
        _nameLabel.Text = item.ItemName;
        _descriptionLabel.Text = item.Description;
        _categoryLabel.Text = "Category: " + item.Category;
        _rarityLabel.Text = "Rarity: " + item.Rarity;
        _quantityLabel.Text = "Quantity: " + item.Quantity;
        
        float rarityMultiplier = database.GetRarityMultiplier(item.Rarity);
        int totalValue = (int)(item.Value * rarityMultiplier * item.Quantity);
        _valueLabel.Text = "Value: " + totalValue + " Gold";
        
        // Stats
        string statsText = "Stats:\n";
        if (item.Stats.Count > 0) {
            foreach (var stat in item.Stats) {
                statsText += "  " + stat.Key + ": " + stat.Value + "\n";
            }
        } else {
            statsText += "  None";
        }
        if (!string.IsNullOrEmpty(item.SpecialEffect)) {
            statsText += "\nSpecial Effect: " + item.SpecialEffect;
        }
        _statsLabel.Text = statsText;
        
        // Color by rarity
        Color rarityColor = Colors.White;
        switch (item.Rarity) {
            case "Uncommon": rarityColor = Colors.Green; break;
            case "Rare": rarityColor = Colors.Blue; break;
            case "Epic": rarityColor = new Color(0.63f, 0.21f, 0.93f); break;
            case "Legendary": rarityColor = new Color(1f, 0.5f, 0); break;
        }
        _nameLabel.Modulate = rarityColor;
        
        // Enable/disable buttons based on category
        bool isUsable = item.Category == "Consumable" || item.Category == "Special";
        _useButton.Disabled = !isUsable;
    }
    
    public void ClearDisplay() {
        _nameLabel.Text = "Select an item";
        _descriptionLabel.Text = "";
        _categoryLabel.Text = "";
        _rarityLabel.Text = "";
        _quantityLabel.Text = "";
        _valueLabel.Text = "";
        _statsLabel.Text = "";
        _useButton.Disabled = true;
        _dropButton.Disabled = true;
    }
    
    private void OnUsePressed() {
        if (_currentItem != null) {
            ItemUseRequested?.Invoke(_currentItem.ItemId);
        }
    }
    
    private void OnDropPressed() {
        if (_currentItem != null) {
            ItemDropRequested?.Invoke(_currentItem.ItemId);
        }
    }
}
