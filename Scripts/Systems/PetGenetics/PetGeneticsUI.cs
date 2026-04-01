using Godot;
using System;
using System.Collections.Generic;

public partial class PetGeneticsUI : Control
{
    private PetGeneticsSystem _system;
    private TabContainer _tabContainer;
    private VBoxContainer _overviewTab;
    private VBoxContainer _genesTab;
    private VBoxContainer _statisticsTab;
    
    // Pet selection
    private OptionButton _petSelector;
    private Label _selectedPetLabel;
    
    // Gene display
    private VBoxContainer _geneList;
    private OptionButton _geneTypeSelector;
    
    // Statistics labels
    private Label _totalModLabel;
    private Label _legendaryLabel;
    private Label _epicLabel;
    private Label _rareLabel;
    
    private string _currentPetId = "pet_1";
    
    public override void _Ready()
    {
        _system = new PetGeneticsSystem();
        GetTree().Root.AddChild(_system);
        
        SetupUI();
        RefreshData();
    }
    
    private void SetupUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainContainer);
        
        // Title
        var title = new Label();
        title.Text = "Pet Genetics System";
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(title);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);
        
        // Overview tab
        _overviewTab = new VBoxContainer();
        _tabContainer.AddChild(_overviewTab);
        _tabContainer.SetTabTitle(_overviewTab, "Overview");
        SetupOverviewTab();
        
        // Genes tab
        _genesTab = new VBoxContainer();
        _tabContainer.AddChild(_genesTab);
        _tabContainer.SetTabTitle(_genesTab, "Genes");
        SetupGenesTab();
        
        // Statistics tab
        _statisticsTab = new VBoxContainer();
        _tabContainer.AddChild(_statisticsTab);
        _tabContainer.SetTabTitle(_statisticsTab, "Statistics");
        SetupStatisticsTab();
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += () => QueueFree();
        mainContainer.AddChild(closeButton);
    }
    
    private void SetupOverviewTab()
    {
        // Pet selector
        var petLabel = new Label();
        petLabel.Text = "Select Pet:";
        _overviewTab.AddChild(petLabel);
        
        _petSelector = new OptionButton();
        _petSelector.AddItem("Pet 1 (Default)", 0);
        _petSelector.AddItem("Pet 2", 1);
        _petSelector.AddItem("Pet 3", 2);
        _petSelector.AddItem("Pet 4", 3);
        _petSelector.AddItem("Pet 5", 4);
        _petSelector.ItemSelected += OnPetSelected;
        _overviewTab.AddChild(_petSelector);
        
        // Current genes
        var genesLabel = new Label();
        genesLabel.Text = "Current Genes:";
        _overviewTab.AddChild(genesLabel);
        
        _geneList = new VBoxContainer();
        _overviewTab.AddChild(_geneList);
        
        // Action buttons
        var addGeneButton = new Button();
        addGeneButton.Text = "Add Random Gene";
        addGeneButton.Pressed += OnAddGenePressed;
        _overviewTab.AddChild(addGeneButton);
        
        var addPhysicalButton = new Button();
        addPhysicalButton.Text = "Add Physical Gene";
        addPhysicalButton.Pressed += () => OnAddGeneTypePressed("Physical");
        _overviewTab.AddChild(addPhysicalButton);
        
        var addMagicalButton = new Button();
        addMagicalButton.Text = "Add Magical Gene";
        addMagicalButton.Pressed += () => OnAddGeneTypePressed("Magical");
        _overviewTab.AddChild(addMagicalButton);
    }
    
    private void SetupGenesTab()
    {
        // Gene type selector
        var typeLabel = new Label();
        typeLabel.Text = "Gene Type Filter:";
        _genesTab.AddChild(typeLabel);
        
        _geneTypeSelector = new OptionButton();
        _geneTypeSelector.AddItem("All Types", 0);
        _geneTypeSelector.AddItem("Physical", 1);
        _geneTypeSelector.AddItem("Magical", 2);
        _geneTypeSelector.AddItem("Support", 3);
        _geneTypeSelector.AddItem("Utility", 4);
        _genesTab.AddChild(_geneTypeSelector);
        
        // Unlocked templates
        var unlockedLabel = new Label();
        unlockedLabel.Text = "Unlocked Gene Templates:";
        _genesTab.AddChild(unlockedLabel);
        
        var templatesContainer = new VBoxContainer();
        _genesTab.AddChild(templatesContainer);
        
        // Generate button
        var generateButton = new Button();
        generateButton.Text = "Generate New Gene";
        generateButton.Pressed += OnAddGenePressed;
        _genesTab.AddChild(generateButton);
    }
    
    private void SetupStatisticsTab()
    {
        _totalModLabel = new Label();
        _totalModLabel.Text = "Total Modifications: 0";
        _statisticsTab.AddChild(_totalModLabel);
        
        _legendaryLabel = new Label();
        _legendaryLabel.Text = "Legendary Genes: 0";
        _legendaryLabel.Modulate = new Color(1, 0.84f, 0); // Gold
        _statisticsTab.AddChild(_legendaryLabel);
        
        _epicLabel = new Label();
        _epicLabel.Text = "Epic Genes: 0";
        _epicLabel.Modulate = new Color(0.64f, 0.08f, 0.64f); // Purple
        _statisticsTab.AddChild(_epicLabel);
        
        _rareLabel = new Label();
        _rareLabel.Text = "Rare Genes: 0";
        _rareLabel.Modulate = new Color(0.08f, 0.44f, 0.88f); // Blue
        _statisticsTab.AddChild(_rareLabel);
        
        // History
        var historyLabel = new Label();
        historyLabel.Text = "Modification History:";
        _statisticsTab.AddChild(historyLabel);
        
        var historyContainer = new VBoxContainer();
        _statisticsTab.AddChild(historyContainer);
        
        // Reset button
        var resetButton = new Button();
        resetButton.Text = "Reset Statistics";
        resetButton.Pressed += OnResetPressed;
        _statisticsTab.AddChild(resetButton);
    }
    
    private void OnPetSelected(long index)
    {
        _currentPetId = "pet_" + (index + 1);
        RefreshData();
    }
    
    private void OnAddGenePressed()
    {
        if (_system.AddGeneToPet(_currentPetId))
        {
            RefreshData();
        }
    }
    
    private void OnAddGeneTypePressed(string geneType)
    {
        if (_system.AddGeneToPet(_currentPetId, geneType))
        {
            RefreshData();
        }
    }
    
    private void OnResetPressed()
    {
        // Reset would require modifying the system
        RefreshData();
    }
    
    private void RefreshData()
    {
        // Clear gene list
        foreach (Node child in _geneList.GetChildren())
        {
            child.QueueFree();
        }
        
        // Display current genes
        var genes = _system.GetPetGenes(_currentPetId);
        foreach (var gene in genes)
        {
            var genePanel = new PanelContainer();
            var geneLabel = new Label();
            
            string rarityColor = GetRarityColor(gene.Rarity);
            geneLabel.Text = $"[{gene.Rarity}] {gene.GeneName} ({gene.GeneType})\n" +
                $"STR: {gene.StrengthBonus:F2} VIT: {gene.VitalityBonus:F2} AGI: {gene.AgilityBonus:F2}\n" +
                $"INT: {gene.IntelligenceBonus:F2} LUCK: {gene.LuckBonus:F2}" +
                (string.IsNullOrEmpty(gene.SpecialEffect) ? "" : $"\nEffect: {gene.SpecialEffect}");
            
            geneLabel.Modulate = ColorFromHex(rarityColor);
            genePanel.AddChild(geneLabel);
            _geneList.AddChild(genePanel);
        }
        
        // Update statistics
        var stats = _system.GetStatistics();
        _totalModLabel.Text = $"Total Modifications: {stats["TotalModifications"]}";
        _legendaryLabel.Text = $"Legendary Genes: {stats["LegendaryGenes"]}";
        _epicLabel.Text = $"Epic Genes: {stats["EpicGenes"]}";
        _rareLabel.Text = $"Rare Genes: {stats["RareGenes"]}";
    }
    
    private string GetRarityColor(string rarity)
    {
        switch (rarity)
        {
            case "Legendary": return "#FFD700";
            case "Epic": return "#A020F0";
            case "Rare": return "#4169E1";
            case "Uncommon": return "#32CD32";
            default: return "#FFFFFF";
        }
    }
    
    private Color ColorFromHex(string hex)
    {
        var color = new Color(1, 1, 1);
        if (hex.Length == 7)
        {
            color.R = Convert.ToByte(hex.Substring(1, 2), 16) / 255.0f;
            color.G = Convert.ToByte(hex.Substring(3, 2), 16) / 255.0f;
            color.B = Convert.ToByte(hex.Substring(5, 2), 16) / 255.0f;
        }
        return color;
    }
    
    public override void _Input(InputEvent ev)
    {
        if (ev.IsActionPressed("ui_cancel"))
        {
            QueueFree();
        }
    }
}
