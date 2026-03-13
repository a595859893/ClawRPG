using Godot;
using System.Collections.Generic;

public class RuneUI : Control
{
    private TabContainer _tabContainer;
    private VBoxContainer _overviewTab;
    private VBoxContainer _runesTab;
    private VBoxContainer _statisticsTab;
    
    // Overview widgets
    private Label _titleLabel;
    private Label _totalAttributesLabel;
    private GridContainer _equippedRunesGrid;
    
    // Runes widgets
    private OptionButton _slotFilter;
    private OptionButton _typeFilter;
    private GridContainer _runesGrid;
    private Label _runeDetailsLabel;
    private Button _equipButton;
    private Button _enhanceButton;
    
    // Statistics widgets
    private Label _statsLabel;
    
    private string _selectedRuneId = null;
    
    public override void _Ready()
    {
        SetupUI();
        RefreshUI();
    }
    
    private void SetupUI()
    {
        // Main container
        var mainPanel = new PanelContainer();
        mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainPanel.CustomMinimumSize = new Vector2(900, 600);
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ Rune System";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(_titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlags.Fill);
        mainVBox.AddChild(_tabContainer);
        
        // Overview Tab
        _overviewTab = new VBoxContainer();
        _overviewTab.Name = "Overview";
        _tabContainer.AddChild(_overviewTab);
        
        SetupOverviewTab();
        
        // Runes Tab
        _runesTab = new VBoxContainer();
        _runesTab.Name = "Runes";
        _tabContainer.AddChild(_runesTab);
        
        SetupRunesTab();
        
        // Statistics Tab
        _statisticsTab = new VBoxContainer();
        _statisticsTab.Name = "Statistics";
        _tabContainer.AddChild(_statisticsTab);
        
        SetupStatisticsTab();
        
        // Close button
        var closeBtn = new Button();
        closeBtn.Text = "Close (ESC)";
        closeBtn.Pressed += () => Hide();
        mainVBox.AddChild(closeBtn);
    }
    
    private void SetupOverviewTab()
    {
        var scroll = new ScrollContainer();
        scroll.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlags.Fill);
        _overviewTab.AddChild(scroll);
        
        var content = new VBoxContainer();
        scroll.AddChild(content);
        
        // Current attributes
        var attrTitle = new Label();
        attrTitle.Text = "Current Attributes from Runes:";
        attrTitle.AddThemeFontSizeOverride("font_size", 18);
        content.AddChild(attrTitle);
        
        _totalAttributesLabel = new Label();
        _totalAttributesLabel.Text = "No runes equipped";
        content.AddChild(_totalAttributesLabel);
        
        // Equipped runes
        var equippedTitle = new Label();
        equippedTitle.Text = "\nEquipped Runes:";
        equippedTitle.AddThemeFontSizeOverride("font_size", 18);
        content.AddChild(equippedTitle);
        
        _equippedRunesGrid = new GridContainer();
        _equippedRunesGrid.Columns = 5;
        content.AddChild(_equippedRunesGrid);
        
        RefreshEquippedRunes();
    }
    
    private void SetupRunesTab()
    {
        var filterBox = new HBoxContainer();
        _runesTab.AddChild(filterBox);
        
        // Slot filter
        var slotLabel = new Label();
        slotLabel.Text = "Slot: ";
        filterBox.AddChild(slotLabel);
        
        _slotFilter = new OptionButton();
        _slotFilter.AddItem("All Slots", 0);
        _slotFilter.AddItem("Helmet", 1);
        _slotFilter.AddItem("Chest", 2);
        _slotFilter.AddItem("Legs", 3);
        _slotFilter.AddItem("Weapon", 4);
        _slotFilter.AddItem("Accessory", 5);
        _slotFilter.ItemSelected += (index) => RefreshRuneList();
        filterBox.AddChild(_slotFilter);
        
        // Type filter
        var typeLabel = new Label();
        typeLabel.Text = "  Type: ";
        filterBox.AddChild(typeLabel);
        
        _typeFilter = new OptionButton();
        _typeFilter.AddItem("All Types", 0);
        _typeFilter.AddItem("Power", 1);
        _typeFilter.AddItem("Defense", 2);
        _typeFilter.AddItem("Support", 3);
        _typeFilter.AddItem("Special", 4);
        _typeFilter.ItemSelected += (index) => RefreshRuneList();
        filterBox.AddChild(_typeFilter);
        
        // Rune grid
        var scroll = new ScrollContainer();
        scroll.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlags.Fill);
        _runesTab.AddChild(scroll);
        
        _runesGrid = new GridContainer();
        _runesGrid.Columns = 3;
        scroll.AddChild(_runesGrid);
        
        // Details panel
        var detailsPanel = new PanelContainer();
        _runesTab.AddChild(detailsPanel);
        
        _runeDetailsLabel = new Label();
        _runeDetailsLabel.Text = "Select a rune to view details";
        detailsPanel.AddChild(_runeDetailsLabel);
        
        // Action buttons
        var buttonBox = new HBoxContainer();
        _runesTab.AddChild(buttonBox);
        
        _equipButton = new Button();
        _equipButton.Text = "Equip";
        _equipButton.Pressed += OnEquipPressed;
        buttonBox.AddChild(_equipButton);
        
        _enhanceButton = new Button();
        _enhanceButton.Text = "Enhance";
        _enhanceButton.Pressed += OnEnhancePressed;
        buttonBox.AddChild(_enhanceButton);
        
        RefreshRuneList();
    }
    
    private void SetupStatisticsTab()
    {
        var scroll = new ScrollContainer();
        scroll.SetSizeFlags(Control.SizeFlags.ExpandFill, Control.SizeFlags.Fill);
        _statisticsTab.AddChild(scroll);
        
        _statsLabel = new Label();
        _statsLabel.Text = "Loading statistics...";
        scroll.AddChild(_statsLabel);
        
        RefreshStatistics();
    }
    
    public void RefreshUI()
    {
        RefreshEquippedRunes();
        RefreshRuneList();
        RefreshStatistics();
        RefreshAttributes();
    }
    
    private void RefreshAttributes()
    {
        var attrs = RuneSystem.Instance.GetTotalAttributes();
        if (attrs.Count == 0)
        {
            _totalAttributesLabel.Text = "No runes equipped";
            return;
        }
        
        string text = "";
        foreach (var attr in attrs)
        {
            text += $"{attr.Key}: +{attr.Value:F1}\n";
        }
        _totalAttributesLabel.Text = text;
    }
    
    private void RefreshEquippedRunes()
    {
        foreach (var child in _equippedRunesGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        string[] slots = { "Helmet", "Chest", "Legs", "Weapon", "Accessory" };
        var equipped = RuneSystem.Instance.GetEquippedRunes();
        
        foreach (string slot in slots)
        {
            var slotPanel = new PanelContainer();
            slotPanel.CustomMinimumSize = new Vector2(100, 80);
            
            var slotVBox = new VBoxContainer();
            slotPanel.AddChild(slotVBox);
            
            var slotLabel = new Label();
            slotLabel.Text = slot;
            slotLabel.Align = Label.AlignEnum.Center;
            slotLabel.AddThemeFontSizeOverride("font_size", 12);
            slotVBox.AddChild(slotLabel);
            
            if (equipped.ContainsKey(slot))
            {
                string runeId = equipped[slot];
                var rune = RuneDatabase.Instance.GetRune(runeId);
                if (rune != null)
                {
                    var runeLabel = new Label();
                    runeLabel.Text = rune.Name;
                    runeLabel.Align = Label.AlignEnum.Center;
                    runeLabel.AddThemeFontSizeOverride("font_size", 10);
                    slotVBox.AddChild(runeLabel);
                    
                    int level = RuneSystem.Instance.GetRuneLevel(runeId);
                    var levelLabel = new Label();
                    levelLabel.Text = $"Lv.{level}";
                    levelLabel.Align = Label.AlignEnum.Center;
                    slotVBox.AddChild(levelLabel);
                }
            }
            else
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "[Empty]";
                emptyLabel.Align = Label.AlignEnum.Center;
                emptyLabel.AddThemeFontSizeOverride("font_size", 10);
                slotVBox.AddChild(emptyLabel);
            }
            
            _equippedRunesGrid.AddChild(slotPanel);
        }
    }
    
    private void RefreshRuneList()
    {
        foreach (var child in _runesGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        int slotIndex = _slotFilter.Selected;
        int typeIndex = _typeFilter.Selected;
        
        var db = RuneDatabase.Instance;
        var unlocked = RuneSystem.Instance.GetUnlockedRunes();
        
        foreach (var rune in db.Runes.Values)
        {
            // Check filters
            if (slotIndex > 0)
            {
                var runeSlot = (int)rune.Slot;
                if (runeSlot != slotIndex - 1) continue;
            }
            
            if (typeIndex > 0)
            {
                var runeType = (int)rune.Type;
                if (runeType != typeIndex - 1) continue;
            }
            
            if (!unlocked.ContainsKey(rune.Id)) continue;
            
            var runePanel = CreateRunePanel(rune);
            _runesGrid.AddChild(runePanel);
        }
    }
    
    private PanelContainer CreateRunePanel(RuneDatabase.RuneDefinition rune)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(200, 100);
        
        if (_selectedRuneId == rune.Id)
        {
            var style = new StyleBoxFlat();
            style.BgColor = new Color(0.3f, 0.5f, 0.8f, 0.3f);
            style.BorderWidthAll = 2;
            style.BorderColor = new Color(0.5f, 0.7f, 1f);
            panel.AddThemeStyleboxOverride("panel", style);
        }
        
        var vbox = new VBoxContainer();
        panel.AddChild(vbox);
        
        var nameLabel = new Label();
        nameLabel.Text = rune.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        vbox.AddChild(nameLabel);
        
        var typeLabel = new Label();
        typeLabel.Text = $"{rune.Slot} - {rune.Type}";
        typeLabel.AddThemeFontSizeOverride("font_size", 10);
        vbox.AddChild(typeLabel);
        
        int level = RuneSystem.Instance.GetRuneLevel(rune.Id);
        var levelLabel = new Label();
        levelLabel.Text = $"Level: {level}/5";
        levelLabel.AddThemeFontSizeOverride("font_size", 10);
        vbox.AddChild(levelLabel);
        
        // Make clickable
        var button = new Button();
        button.Text = "Select";
        button.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        button.Pressed += () =>
        {
            _selectedRuneId = rune.Id;
            ShowRuneDetails(rune);
            RefreshRuneList();
        };
        vbox.AddChild(button);
        
        return panel;
    }
    
    private void ShowRuneDetails(RuneDatabase.RuneDefinition rune)
    {
        int level = RuneSystem.Instance.GetRuneLevel(rune.Id);
        float levelMult = 1f + (level - 1) * 0.2f;
        
        string text = $"{rune.Name}\n";
        text += $"{rune.Description}\n\n";
        text += $"Slot: {rune.Slot}\n";
        text += $"Type: {rune.Type}\n";
        text += $"Level: {level}/5\n";
        text += $"Required Level: {rune.RequiredLevel}\n\n";
        text += "Attributes:\n";
        
        foreach (var attr in rune.Attributes)
        {
            float value = attr.Value * levelMult;
            text += $"  {attr.Key}: +{value:F1}\n";
        }
        
        if (!string.IsNullOrEmpty(rune.SpecialEffect))
        {
            text += $"\nSpecial: {rune.SpecialEffect}\n";
        }
        
        text += $"\nEnhance Cost: {rune.EnhanceCost * level}g";
        
        _runeDetailsLabel.Text = text;
    }
    
    private void RefreshStatistics()
    {
        var stats = RuneSystem.Instance.GetStatistics();
        
        string text = "📊 Rune Statistics\n\n";
        text += $"Total Runes Unlocked: {stats.TotalRunesUnlocked}\n";
        text += $"Total Runes Equipped: {stats.TotalRunesEquipped}\n";
        text += $"Times Enhanced: {stats.TimesEnhanced}\n";
        text += $"Times Removed: {stats.TimesRemoved}\n\n";
        text += $"Total Gold Spent: {stats.TotalGoldSpent}g\n";
        text += $"Total EXP Gained: {stats.TotalExpGained}";
        
        _statsLabel.Text = text;
    }
    
    private void OnEquipPressed()
    {
        if (string.IsNullOrEmpty(_selectedRuneId)) return;
        
        var rune = RuneDatabase.Instance.GetRune(_selectedRuneId);
        if (rune == null) return;
        
        string slot = rune.Slot.ToString();
        
        // Check if already equipped
        var equipped = RuneSystem.Instance.GetEquippedRunes();
        if (equipped.ContainsKey(slot) && equipped[slot] == _selectedRuneId)
        {
            RuneSystem.Instance.UnequipRune(slot);
        }
        else
        {
            RuneSystem.Instance.EquipRune(slot, _selectedRuneId);
        }
        
        RefreshUI();
    }
    
    private void OnEnhancePressed()
    {
        if (string.IsNullOrEmpty(_selectedRuneId)) return;
        
        var rune = RuneDatabase.Instance.GetRune(_selectedRuneId);
        if (rune == null) return;
        
        int level = RuneSystem.Instance.GetRuneLevel(_selectedRuneId);
        if (level >= 5)
        {
            _runeDetailsLabel.Text = "Rune is already at maximum level!";
            return;
        }
        
        int cost = rune.EnhanceCost * level;
        RuneSystem.Instance.EnhanceRune(_selectedRuneId);
        
        RefreshUI();
        
        // Update details
        ShowRuneDetails(rune);
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            Hide();
        }
    }
}
