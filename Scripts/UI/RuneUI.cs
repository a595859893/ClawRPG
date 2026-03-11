using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;
using ClawRPG.Scripts.Systems;

public class RuneUI : Control
{
    private PanelContainer mainPanel;
    private VBoxContainer mainVBox;
    private TabContainer tabContainer;
    
    // Inventory tab
    private GridContainer inventoryGrid;
    private Label inventoryLabel;
    
    // Equipment tab
    private GridContainer equipmentGrid;
    private Label equipmentLabel;
    
    // Sets tab
    private VBoxContainer setsVBox;
    private Label setsLabel;
    
    // Selected rune info
    private PanelContainer infoPanel;
    private Label runeNameLabel;
    private Label runeTypeLabel;
    private Label runeRarityLabel;
    private Label runeDescriptionLabel;
    private Label runeAttributesLabel;
    private Label runeSetLabel;
    private Button equipButton;
    private Button unequipButton;
    private Button sellButton;
    
    private Rune selectedRune;
    private int selectedSlotIndex = -1;

    public override void _Ready()
    {
        Visible = false; 
        SetupUI();
        
        // Connect signals
        if (RuneManager.Instance != null)
        {
            RuneManager.Instance.OnRunesUpdated += RefreshInventory;
            RuneManager.Instance.OnRuneEquipped += RefreshEquipment;
        }
        
        RefreshAll();
    }

    private void SetupUI()
    {
        // Main Panel
        mainPanel = new PanelContainer();
        mainPanel.AnchorRight = 1f;
        mainPanel.AnchorBottom = 1f;
        mainPanel.SetAnchorsPreserveMargin(true);
        mainPanel.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(mainPanel);

        // Main VBox
        mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreserveMargin(true);
        mainVBox.AnchorRight = 1f;
        mainVBox.AnchorBottom = 1f;
        mainVBox.AddThemeConstantOverride("separation", 10);
        mainPanel.AddChild(mainVBox);

        // Title
        var titleLabel = new Label();
        titleLabel.Text = "🔮 Rune System";
        titleLabel.Align = Label.AlignEnum.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);

        // Tab Container
        tabContainer = new TabContainer();
        tabContainer.SetAnchorsPreserveMargin(true);
        tabContainer.AnchorRight = 1f;
        tabContainer.AnchorBottom = 0.85f;
        mainVBox.AddChild(tabContainer);

        // Inventory Tab
        SetupInventoryTab();

        // Equipment Tab
        SetupEquipmentTab();

        // Sets Tab
        SetupSetsTab();

        // Info Panel
        SetupInfoPanel();

        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (R)";
        closeButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        closeButton.Pressed += () => ToggleUI();
        mainVBox.AddChild(closeButton);
    }

    private void SetupInventoryTab()
    {
        var inventoryTab = new Control();
        inventoryTab.Name = "Inventory";
        tabContainer.AddChild(inventoryTab);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreserveMargin(true);
        vbox.AnchorRight = 1f;
        vbox.AnchorBottom = 1f;
        vbox.AddThemeConstantOverride("separation", 10);
        inventoryTab.AddChild(vbox);

        inventoryLabel = new Label();
        inventoryLabel.Text = "Your Runes";
        inventoryLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(inventoryLabel);

        var scrollContainer = new ScrollContainer();
        scrollContainer.SetAnchorsPreserveMargin(true);
        scrollContainer.AnchorRight = 1f;
        scrollContainer.AnchorBottom = 1f;
        scrollContainer.VscrollEnabled = true;
        vbox.AddChild(scrollContainer);

        inventoryGrid = new GridContainer();
        inventoryGrid.Columns = 5;
        inventoryGrid.AddThemeConstantOverride("h_separation", 10);
        inventoryGrid.AddThemeConstantOverride("v_separation", 10);
        inventoryGrid.SetAnchorsPreserveMargin(true);
        inventoryGrid.AnchorRight = 1f;
        scrollContainer.AddChild(inventoryGrid);
    }

    private void SetupEquipmentTab()
    {
        var equipmentTab = new Control();
        equipmentTab.Name = "Equipment";
        tabContainer.AddChild(equipmentTab);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreserveMargin(true);
        vbox.AnchorRight = 1f;
        vbox.AnchorBottom = 1f;
        vbox.AddThemeConstantOverride("separation", 10);
        equipmentTab.AddChild(vbox);

        equipmentLabel = new Label();
        equipmentLabel.Text = "Equipped Runes";
        equipmentLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(equipmentLabel);

        var scrollContainer = new ScrollContainer();
        scrollContainer.SetAnchorsPreserveMargin(true);
        scrollContainer.AnchorRight = 1f;
        scrollContainer.AnchorBottom = 1f;
        scrollContainer.VscrollEnabled = true;
        vbox.AddChild(scrollContainer);

        equipmentGrid = new GridContainer();
        equipmentGrid.Columns = 2;
        equipmentGrid.AddThemeConstantOverride("h_separation", 10);
        equipmentGrid.AddThemeConstantOverride("v_separation", 10);
        equipmentGrid.SetAnchorsPreserveMargin(true);
        equipmentGrid.AnchorRight = 1f;
        scrollContainer.AddChild(equipmentGrid);
    }

    private void SetupSetsTab()
    {
        var setsTab = new Control();
        setsTab.Name = "Sets";
        tabContainer.AddChild(setsTab);

        var scrollContainer = new ScrollContainer();
        scrollContainer.SetAnchorsPreserveMargin(true);
        scrollContainer.AnchorRight = 1f;
        scrollContainer.AnchorBottom = 1f;
        scrollContainer.VscrollEnabled = true;
        setsTab.AddChild(scrollContainer);

        setsVBox = new VBoxContainer();
        setsVBox.SetAnchorsPreserveMargin(true);
        setsVBox.AnchorRight = 1f;
        setsVBox.AnchorBottom = 1f;
        setsVBox.AddThemeConstantOverride("separation", 10);
        scrollContainer.AddChild(setsVBox);

        setsLabel = new Label();
        setsLabel.Text = "Active Set Bonuses";
        setsLabel.AddThemeFontSizeOverride("font_size", 18);
        setsVBox.AddChild(setsLabel);
    }

    private void SetupInfoPanel()
    {
        var infoTab = new Control();
        infoTab.Name = "Info";
        tabContainer.AddChild(infoTab);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreserveMargin(true);
        vbox.AnchorRight = 1f;
        vbox.AnchorBottom = 1f;
        vbox.AddThemeConstantOverride("separation", 10);
        vbox.MarginLeft = 20;
        vbox.MarginTop = 20;
        vbox.MarginRight = -20;
        vbox.MarginBottom = -20;
        infoTab.AddChild(vbox);

        infoPanel = new PanelContainer();
        infoPanel.SetAnchorsPreserveMargin(true);
        infoPanel.AnchorRight = 1f;
        infoPanel.AnchorBottom = 1f;
        vbox.AddChild(infoPanel);

        var infoVBox = new VBoxContainer();
        infoVBox.AddThemeConstantOverride("separation", 10);
        infoPanel.AddChild(infoVBox);

        runeNameLabel = new Label();
        runeNameLabel.Text = "Select a rune";
        runeNameLabel.AddThemeFontSizeOverride("font_size", 20);
        infoVBox.AddChild(runeNameLabel);

        runeTypeLabel = new Label();
        runeTypeLabel.Text = "";
        infoVBox.AddChild(runeTypeLabel);

        runeRarityLabel = new Label();
        runeRarityLabel.Text = "";
        infoVBox.AddChild(runeRarityLabel);

        runeDescriptionLabel = new Label();
        runeDescriptionLabel.Text = "";
        runeDescriptionLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        infoVBox.AddChild(runeDescriptionLabel);

        runeAttributesLabel = new Label();
        runeAttributesLabel.Text = "";
        infoVBox.AddChild(runeAttributesLabel);

        runeSetLabel = new Label();
        runeSetLabel.Text = "";
        infoVBox.AddChild(runeSetLabel);

        // Buttons
        var buttonHBox = new HBoxContainer();
        buttonHBox.AddThemeConstantOverride("separation", 10);
        infoVBox.AddChild(buttonHBox);

        equipButton = new Button();
        equipButton.Text = "Equip";
        equipButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        equipButton.Pressed += OnEquipPressed;
        buttonHBox.AddChild(equipButton);

        unequipButton = new Button();
        unequipButton.Text = "Unequip";
        unequipButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        unequipButton.Pressed += OnUnequipPressed;
        buttonHBox.AddChild(unequipButton);

        sellButton = new Button();
        sellButton.Text = "Sell";
        sellButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        sellButton.Pressed += OnSellPressed;
        buttonHBox.AddChild(sellButton);

        UpdateInfoPanel(null);
    }

    public void ToggleUI()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshAll();
            InputManager.AddActionListener("ui_cancel", OnClosePressed);
            InputManager.AddActionListener("ui_rune", OnClosePressed);
        }
        else
        {
            InputManager.RemoveActionListener("ui_cancel", OnClosePressed);
            InputManager.RemoveActionListener("ui_rune", OnClosePressed);
        }
    }

    private void OnClosePressed()
    {
        ToggleUI();
    }

    private void RefreshAll()
    {
        RefreshInventory();
        RefreshEquipment();
        RefreshSets();
    }

    private void RefreshInventory()
    {
        // Clear existing children
        foreach (var child in inventoryGrid.GetChildren())
            child.QueueFree();

        var runes = RuneManager.Instance.GetOwnedRunes();
        foreach (var rune in runes)
        {
            var button = CreateRuneButton(rune);
            inventoryGrid.AddChild(button);
        }

        inventoryLabel.Text = $"Your Runes ({runes.Count})";
    }

    private void RefreshEquipment()
    {
        // Clear existing children
        foreach (var child in equipmentGrid.GetChildren())
            child.QueueFree();

        for (int i = 0; i < 5; i++)
        {
            var slotPanel = new VBoxContainer();
            
            var slotLabel = new Label();
            slotLabel.Text = $"Slot {i + 1}";
            if (RuneManager.Instance.IsSlotUnlocked(i))
            {
                slotLabel.Text += " ✓";
            }
            else
            {
                slotLabel.Text += $" (Cost: {RuneManager.Instance.GetSlotUnlockCost(i)}g)";
                slotLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
            }
            slotLabel.Align = Label.AlignEnum.Center;
            slotPanel.AddChild(slotLabel);

            var equippedRune = RuneManager.Instance.GetEquippedRune(i);
            if (equippedRune != null)
            {
                var button = CreateRuneButton(equippedRune, i);
                slotPanel.AddChild(button);
            }
            else
            {
                var emptyLabel = new Label();
                emptyLabel.Text = "[Empty]";
                emptyLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
                slotPanel.AddChild(emptyLabel);

                // Add click to equip
                var selectButton = new Button();
                selectButton.Text = "Select";
                selectButton.Pressed += () => OnSlotSelected(i);
                slotPanel.AddChild(selectButton);
            }

            // Add unlock button if not unlocked
            if (!RuneManager.Instance.IsSlotUnlocked(i))
            {
                var unlockButton = new Button();
                unlockButton.Text = "Unlock";
                unlockButton.Pressed += () => OnUnlockSlot(i);
                slotPanel.AddChild(unlockButton);
            }

            equipmentGrid.AddChild(slotPanel);
        }

        var equipped = RuneManager.Instance.GetAllEquippedRunes();
        equipmentLabel.Text = $"Equipped Runes ({equipped.Count}/5)";
    }

    private void RefreshSets()
    {
        // Clear existing children (except label)
        foreach (var child in setsVBox.GetChildren())
        {
            if (child != setsLabel)
                child.QueueFree();
        }

        var equipped = RuneManager.Instance.GetAllEquippedRunes();
        
        // Count set pieces
        var setCounts = new Dictionary<RuneSet, int>();
        foreach (var rune in equipped)
        {
            if (rune.Set != RuneSet.None)
            {
                if (!setCounts.ContainsKey(rune.Set))
                    setCounts[rune.Set] = 0;
                setCounts[rune.Set]++;
            }
        }
        
        if (setCounts.Count == 0)
        {
            var noSetsLabel = new Label();
            noSetsLabel.Text = "No set bonuses active";
            noSetsLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
            setsVBox.AddChild(noSetsLabel);
            return;
        }

        foreach (var setCount in setCounts)
        {
            var setName = setCount.Key.ToString();
            var count = setCount.Value;
            
            var setPanel = new PanelContainer();
            setPanel.CustomMinimumSize = new Vector2(0, 60);
            setsVBox.AddChild(setPanel);

            var hbox = new HBoxContainer();
            hbox.AddThemeConstantOverride("separation", 20);
            setPanel.AddChild(hbox);

            var nameLabel = new Label();
            nameLabel.Text = setName + " Set";
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            hbox.AddChild(nameLabel);

            var countLabel = new Label();
            countLabel.Text = $"{count} pieces";
            countLabel.Modulate = new Color(0, 1, 0);
            hbox.AddChild(countLabel);
        }
    }

    private Button CreateRuneButton(Rune rune, int slotIndex = -1)
    {
        var button = new Button();
        button.Text = rune.Name;
        button.CustomMinimumSize = new Vector2(100, 40);
        
        // Color based on rarity
        var color = GetRarityColor(rune.Rarity);
        button.Modulate = color;
        
        button.Pressed += () => OnRuneSelected(rune, slotIndex);
        
        return button;
    }

    private Color GetRarityColor(RuneRarity rarity)
    {
        switch (rarity)
        {
            case RuneRarity.Common: return new Color(1, 1, 1);
            case RuneRarity.Uncommon: return new Color(0.1f, 1, 0);
            case RuneRarity.Rare: return new Color(0, 0.44f, 1);
            case RuneRarity.Epic: return new Color(0.64f, 0.21f, 0.93f);
            case RuneRarity.Legendary: return new Color(1, 0.5f, 0);
            default: return new Color(1, 1, 1);
        }
    }

    private void OnRuneSelected(Rune rune, int slotIndex)
    {
        selectedRune = rune;
        selectedSlotIndex = slotIndex;
        UpdateInfoPanel(rune);
    }

    private void OnSlotSelected(int slotIndex)
    {
        selectedSlotIndex = slotIndex;
        // Show inventory to select a rune for this slot
        tabContainer.CurrentTab = 0; // Switch to inventory tab
    }

    private void OnUnlockSlot(int slotIndex)
    {
        var player = GameManager.GetPlayer();
        if (player == null) return;

        int cost = RuneManager.Instance.GetSlotUnlockCost(slotIndex);
        if (player.Gold >= cost)
        {
            if (RuneManager.Instance.UnlockSlot(slotIndex, player.Gold))
            {
                player.AddGold(-cost);
                RefreshEquipment();
            }
        }
        else
        {
            GD.Print("Not enough gold to unlock slot!");
        }
    }

    private void UpdateInfoPanel(Rune rune)
    {
        if (rune == null)
        {
            runeNameLabel.Text = "Select a rune";
            runeTypeLabel.Text = "";
            runeRarityLabel.Text = "";
            runeDescriptionLabel.Text = "";
            runeAttributesLabel.Text = "";
            runeSetLabel.Text = "";
            equipButton.Disabled = true;
            unequipButton.Disabled = true;
            sellButton.Disabled = true;
            return;
        }

        runeNameLabel.Text = rune.Name;
        runeNameLabel.Modulate = GetRarityColor(rune.Rarity);
        
        runeTypeLabel.Text = $"Type: {rune.Type}";
        runeRarityLabel.Text = $"Rarity: {rune.Rarity}";
        runeDescriptionLabel.Text = rune.Description;

        var attrs = new System.Text.StringBuilder();
        attrs.Append("Attributes:\n");
        foreach (var attr in rune.Attributes)
        {
            attrs.Append($"  {attr.Key}: +{attr.Value}\n");
        }
        runeAttributesLabel.Text = attrs.ToString();

        runeSetLabel.Text = rune.Set != RuneSet.None ? $"Set: {rune.Set}" : "No set";

        // Check if rune is equipped
        bool isEquipped = false; 
        for (int i = 0; i < 5; i++)
        {
            var equipped = RuneManager.Instance.GetEquippedRune(i);
            if (equipped != null && equipped.Id == rune.Id)
            {
                isEquipped = true;
                break;
            }
        }

        equipButton.Disabled = selectedSlotIndex < 0 || isEquipped;
        unequipButton.Disabled = !isEquipped;
        sellButton.Disabled = isEquipped;
    }

    private void OnEquipPressed()
    {
        if (selectedRune == null || selectedSlotIndex < 0)
            return;

        RuneManager.Instance.EquipRune(selectedSlotIndex, selectedRune);
        RefreshAll();
    }

    private void OnUnequipPressed()
    {
        if (selectedRune == null)
            return;

        // Find the slot and unequip
        for (int i = 0; i < 5; i++)
        {
            var equipped = RuneManager.Instance.GetEquippedRune(i);
            if (equipped != null && equipped.Id == selectedRune.Id)
            {
                RuneManager.Instance.UnequipRune(i);
                break;
            }
        }
        RefreshAll();
    }

    private void OnSellPressed()
    {
        if (selectedRune == null)
            return;

        var player = GameManager.GetPlayer();
        if (player == null) return;

        // Check if rune is equipped
        bool isEquipped = false; 
        for (int i = 0; i < 5; i++)
        {
            var equipped = RuneManager.Instance.GetEquippedRune(i);
            if (equipped != null && equipped.Id == selectedRune.Id)
            {
                isEquipped = true;
                break;
            }
        }

        if (isEquipped)
        {
            GD.Print("[RuneUI] Cannot sell equipped rune");
            return;
        }

        // Sell the rune
        if (RuneManager.Instance.RemoveRune(selectedRune))
        {
            player.AddGold(selectedRune.Price);
            GD.Print($"[RuneUI] Sold {selectedRune.Name} for {selectedRune.Price} gold");
            RefreshAll();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_rune") || @event.IsActionPressed("ui_cancel"))
        {
            if (Visible)
            {
                ToggleUI();
                GetTree().SetInputAsHandled();
            }
        }
    }
}
