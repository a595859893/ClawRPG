using Godot;
using System;
using System.Collections.Generic;

public class EquipmentSetUI : Control
{
    // UI Components
    private Label titleLabel;
    private HBoxContainer filterContainer;
    private GridContainer setGrid;
    private VBoxContainer detailPanel;
    private Label setNameLabel;
    private Label setDescriptionLabel;
    private Label setRarityLabel;
    private Label piecesLabel;
    private Label bonusLabel;
    private Label statsLabel;
    private CheckButton unlockedFilterCheck;
    
    // Data
    private List<EquipmentSet> allSets;
    private EquipmentSet selectedSet;
    private string currentFilter = "All";
    private bool showUnlockedOnly = false;

    // Colors for rarity
    private Color commonColor = new Color(0.7f, 0.7f, 0.7f);
    private Color uncommonColor = new Color(0.3f, 0.9f, 0.3f);
    private Color rareColor = new Color(0.3f, 0.5f, 1.0f);
    private Color epicColor = new Color(0.6f, 0.3f, 0.9f);
    private Color legendaryColor = new Color(1.0f, 0.6f, 0.0f);

    public override void _Ready()
    {
        // Create UI
        SetupUI();
        
        // Load data
        if (EquipmentSetSystem.Instance != null)
        {
            allSets = EquipmentSetSystem.Instance.GetAllSets();
            RefreshSetGrid();
        }
        
        // Connect input
        ConnectInput();
    }

    private void SetupUI()
    {
        // Main container
        VBoxContainer mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(AnchorPresets.FullRect);
        mainContainer.Margin = new Margin(20, 20, 20, 20);
        mainContainer.AddChild(this);
        AddChild(mainContainer);

        // Title
        titleLabel = new Label();
        titleLabel.Text = "⚔️ Equipment Sets";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(titleLabel);

        // Filter container
        HBoxContainer filterRow = new HBoxContainer();
        filterRow.Alignment = BoxContainer.AlignmentMode.Center;
        filterRow.Margin = new Margin(0, 10, 0, 10);
        mainContainer.AddChild(filterRow);

        // Filter buttons
        string[] filters = { "All", "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        foreach (string filter in filters)
        {
            Button btn = new Button();
            btn.Text = filter;
            btn.Margin = new Margin(5, 0, 5, 0);
            btn.Pressed += () => OnFilterPressed(filter);
            filterRow.AddChild(btn);
        }

        // Unlocked filter
        unlockedFilterCheck = new CheckButton();
        unlockedFilterCheck.Text = "Show Unlocked Only";
        unlockedFilterCheck.Margin = new Margin(10, 0, 0, 0);
        unlockedFilterCheck.Toggled += OnUnlockedFilterToggled;
        filterRow.AddChild(unlockedFilterCheck);

        // Content container with detail panel
        HBoxContainer contentContainer = new HBoxContainer();
        contentContainer.SetAnchor(AnchorPresets.FullRect);
        contentContainer.Margin = new Margin(0, 60, 0, 0);
        mainContainer.AddChild(contentContainer);

        // Set grid (left side)
        setGrid = new GridContainer();
        setGrid.Columns = 3;
        setGrid.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        contentContainer.AddChild(setGrid);

        // Detail panel (right side)
        detailPanel = new VBoxContainer();
        detailPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        detailPanel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        detailPanel.Margin = new Margin(20, 0, 0, 0);
        contentContainer.AddChild(detailPanel);

        // Set details
        setNameLabel = new Label();
        setNameLabel.AddThemeFontSizeOverride("font_size", 20);
        detailPanel.AddChild(setNameLabel);

        setDescriptionLabel = new Label();
        setDescriptionLabel.Autowrap = true;
        setDescriptionLabel.Margin = new Margin(0, 10, 0, 10);
        detailPanel.AddChild(setDescriptionLabel);

        setRarityLabel = new Label();
        setRarityLabel.Margin = new Margin(0, 5, 0, 5);
        detailPanel.AddChild(setRarityLabel);

        // Pieces section
        Label piecesTitle = new Label();
        piecesTitle.Text = "📦 Set Pieces:";
        piecesTitle.AddThemeFontSizeOverride("font_size", 16);
        piecesTitle.Margin = new Margin(0, 15, 0, 5);
        detailPanel.AddChild(piecesTitle);

        piecesLabel = new Label();
        piecesLabel.Autowrap = true;
        detailPanel.AddChild(piecesLabel);

        // Bonus section
        Label bonusTitle = new Label();
        bonusTitle.Text = "✨ Set Bonuses:";
        bonusTitle.AddThemeFontSizeOverride("font_size", 16);
        bonusTitle.Margin = new Margin(0, 15, 0, 5);
        detailPanel.AddChild(bonusTitle);

        bonusLabel = new Label();
        bonusLabel.Autowrap = true;
        detailPanel.AddChild(bonusLabel);

        // Stats
        statsLabel = new Label();
        statsLabel.Margin = new Margin(0, 20, 0, 10);
        detailPanel.AddChild(statsLabel);

        // Instructions
        Label instructionLabel = new Label();
        instructionLabel.Text = "Press E to toggle • ESC to close";
        instructionLabel.AddThemeFontSizeOverride("font_size", 12);
        instructionLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        mainContainer.AddChild(instructionLabel);
    }

    private void RefreshSetGrid()
    {
        // Clear existing
        foreach (Node child in setGrid.GetChildren())
        {
            child.QueueFree();
        }

        // Filter sets
        List<EquipmentSet> filteredSets = new List<EquipmentSet>();
        foreach (var set in allSets)
        {
            bool matchesFilter = currentFilter == "All" || set.Rarity.ToString() == currentFilter;
            bool matchesUnlocked = !showUnlockedOnly || EquipmentSetSystem.Instance.UnlockedSets.Contains(set.Id);
            
            if (matchesFilter && matchesUnlocked)
                filteredSets.Add(set);
        }

        // Create cards
        foreach (var set in filteredSets)
        {
            Control card = CreateSetCard(set);
            setGrid.AddChild(card);
        }
    }

    private Control CreateSetCard(EquipmentSet set)
    {
        // Card container
        PanelContainer card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(180, 120);
        
        // Get rarity color
        Color rarityColor = GetRarityColor(set.Rarity);
        
        // Card content
        VBoxContainer content = new VBoxContainer();
        content.Alignment = BoxContainer.AlignmentMode.Center;
        card.AddChild(content);

        // Set icon (based on rarity)
        Label iconLabel = new Label();
        iconLabel.Text = GetRarityIcon(set.Rarity);
        iconLabel.AddThemeFontSizeOverride("font_size", 32);
        content.AddChild(iconLabel);

        // Set name
        Label nameLabel = new Label();
        nameLabel.Text = set.Name;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        nameLabel.Modulate = rarityColor;
        content.AddChild(nameLabel);

        // Piece count
        Label pieceLabel = new Label();
        pieceLabel.Text = $"{set.Pieces.Count} pieces";
        pieceLabel.HorizontalAlignment = HorizontalAlignment.Center;
        pieceLabel.AddThemeFontSizeOverride("font_size", 12);
        pieceLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        content.AddChild(pieceLabel);

        // Unlocked indicator
        if (EquipmentSetSystem.Instance.UnlockedSets.Contains(set.Id))
        {
            Label unlockedLabel = new Label();
            unlockedLabel.Text = "✓";
            unlockedLabel.HorizontalAlignment = HorizontalAlignment.Center;
            unlockedLabel.Modulate = new Color(0.3f, 0.9f, 0.3f);
            content.AddChild(unlockedLabel);
        }

        // Click to select
        card.GuiInput += (InputEvent @event) => 
        {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                SelectSet(set);
            }
        };

        return card;
    }

    private void SelectSet(EquipmentSet set)
    {
        selectedSet = set;
        UpdateDetailPanel();
    }

    private void UpdateDetailPanel()
    {
        if (selectedSet == null)
            return;

        // Set name
        setNameLabel.Text = selectedSet.Name;
        setNameLabel.Modulate = GetRarityColor(selectedSet.Rarity);

        // Description
        setDescriptionLabel.Text = selectedSet.Description;

        // Rarity
        setRarityLabel.Text = $"Rarity: {selectedSet.Rarity}";
        setRarityLabel.Modulate = GetRarityColor(selectedSet.Rarity);

        // Pieces
        string piecesText = "";
        foreach (var piece in selectedSet.Pieces)
        {
            piecesText += $"• {piece.Value} ({piece.Key})\n";
        }
        piecesLabel.Text = piecesText;

        // Bonuses
        string bonusText = "";
        foreach (var bonus in selectedSet.Bonuses)
        {
            string pieces = bonus.Key == 2 ? "2 pieces" : "4 pieces";
            bonusText += $"[{pieces}] {bonus.Value}\n";
        }
        bonusLabel.Text = bonusText;

        // Stats
        int unlockedCount = EquipmentSetSystem.Instance.UnlockedSets.Contains(selectedSet.Id) ? 1 : 0;
        statsLabel.Text = $"Total Sets: {allSets.Count}\nUnlocked: {EquipmentSetSystem.Instance.UnlockedSets.Count}";
    }

    private Color GetRarityColor(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return commonColor;
            case Rarity.Uncommon: return uncommonColor;
            case Rarity.Rare: return rareColor;
            case Rarity.Epic: return epicColor;
            case Rarity.Legendary: return legendaryColor;
            default: return commonColor;
        }
    }

    private string GetRarityIcon(Rarity rarity)
    {
        switch (rarity)
        {
            case Rarity.Common: return "⬜";
            case Rarity.Uncommon: return "🟩";
            case Rarity.Rare: return "🟦";
            case Rarity.Epic: return "🟪";
            case Rarity.Legendary: return "🟧";
            default: return "⬜";
        }
    }

    private void OnFilterPressed(string filter)
    {
        currentFilter = filter;
        RefreshSetGrid();
    }

    private void OnUnlockedFilterToggled(bool toggled)
    {
        showUnlockedOnly = toggled;
        RefreshSetGrid();
    }

    private void ConnectInput()
    {
        // Input handling would be done via parent or signal
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel") || @event.IsActionPressed("ui_close"))
        {
            Hide();
        }
        else if (@event.IsActionPressed("ui_accept") || @event.IsActionPressed("toggle_equipment_sets"))
        {
            // Toggle visibility
            if (Visible)
                Hide();
            else
                Show();
        }
    }

    public void Toggle()
    {
        if (Visible)
            Hide();
        else
        {
            Show();
            RefreshSetGrid();
        }
    }
}
