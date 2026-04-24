using Godot;
using System;
using System.Collections.Generic;

public partial class PetTalentUI : Control
{
    private PetTalentSystem _talentSystem;
    private PetTalentDatabase _database;
    
    // UI Elements
    private Label _titleLabel;
    private Label _pointsLabel;
    private TabContainer _tabContainer;
    private Pet _selectedPet;
    
    // Category panels
    private Dictionary<string, VBoxContainer> _categoryPanels;
    private Dictionary<string, Button> _categoryButtons;
    
    // Details panel
    private PanelContainer _detailsPanel;
    private Label _talentNameLabel;
    private Label _talentDescriptionLabel;
    private Label _talentBonusLabel;
    private Label _talentLevelLabel;
    private Button _allocateButton;
    private Button _resetButton;
    
    private string _selectedTalentId;
    private string _selectedCategoryId = "combat";
    
    public override void _Ready()
    {
        _talentSystem = PetTalentSystem.Instance;
        _database = PetTalentDatabase.Instance;
        
        SetupUI();
        SetupCategoryTabs();
        
        Visible = false;
    }
    
    private void SetupUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorAndMargin(AnchorsPreset.FullRect, MarginPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // Title and points header
        var headerContainer = new HBoxContainer();
        headerContainer.SetAnchorAndMargin(MarginPreset.FullWide, MarginPreset.TopBottom);
        headerContainer.AddThemeConstantOverride("separation", 20);
        mainContainer.AddChild(headerContainer);
        
        _titleLabel = new Label();
        _titleLabel.Text = "🐾 Pet Talent System";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        headerContainer.AddChild(_titleLabel);
        
        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        headerContainer.AddChild(spacer);
        
        _pointsLabel = new Label();
        _pointsLabel.Text = "Available Points: 0";
        _pointsLabel.AddThemeFontSizeOverride("font_size", 18);
        headerContainer.AddChild(_pointsLabel);
        
        // Tab container for categories
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.Expand;
        mainContainer.AddChild(_tabContainer);
        
        _categoryPanels = new Dictionary<string, VBoxContainer>();
        _categoryButtons = new Dictionary<string, Button>();
        
        // Create tab for each category
        foreach (var category in _database.Categories)
        {
            var scrollContainer = new ScrollContainer();
            scrollContainer.Name = category.Name;
            _tabContainer.AddChild(scrollContainer);
            
            var vbox = new VBoxContainer();
            vbox.AddThemeConstantOverride("separation", 10);
            vbox.SetAnchorAndMargin(MarginPreset.FullRect, MarginPreset.FullRect);
            scrollContainer.AddChild(vbox);
            
            _categoryPanels[category.Id] = vbox;
            
            // Add talent buttons to each category
            foreach (var talent in category.Talents)
            {
                CreateTalentButton(talent, vbox);
            }
        }
        
        // Details panel at bottom
        var detailsContainer = new HBoxContainer();
        detailsContainer.SetAnchorAndMargin(MarginPreset.FullWide, MarginPreset.BottomWide);
        detailsContainer.AddThemeConstantOverride("separation", 20);
        mainContainer.AddChild(detailsContainer);
        
        // Talent details
        _detailsPanel = new PanelContainer();
        _detailsPanel.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        detailsContainer.AddChild(_detailsPanel);
        
        var detailsVBox = new VBoxContainer();
        _detailsPanel.AddChild(detailsVBox);
        
        _talentNameLabel = new Label();
        _talentNameLabel.Text = "Select a talent";
        _talentNameLabel.AddThemeFontSizeOverride("font_size", 18);
        detailsVBox.AddChild(_talentNameLabel);
        
        _talentDescriptionLabel = new Label();
        _talentDescriptionLabel.Text = "";
        _talentDescriptionLabel.AutowrapMode = TextServer.AutowrapWord;
        detailsVBox.AddChild(_talentDescriptionLabel);
        
        _talentBonusLabel = new Label();
        _talentBonusLabel.Text = "";
        detailsVBox.AddChild(_talentBonusLabel);
        
        _talentLevelLabel = new Label();
        _talentLevelLabel.Text = "";
        detailsVBox.AddChild(_talentLevelLabel);
        
        // Buttons
        var buttonVBox = new VBoxContainer();
        buttonVBox.AddThemeConstantOverride("separation", 10);
        detailsContainer.AddChild(buttonVBox);
        
        _allocateButton = new Button();
        _allocateButton.Text = "Allocate Point";
        _allocateButton.Pressed += OnAllocatePressed;
        buttonVBox.AddChild(_allocateButton);
        
        _resetButton = new Button();
        _resetButton.Text = "Reset All";
        _resetButton.Pressed += OnResetPressed;
        buttonVBox.AddChild(_resetButton);
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += OnClosePressed;
        buttonVBox.AddChild(closeButton);
    }
    
    private void CreateTalentButton(PetTalent talent, VBoxContainer parent)
    {
        var button = new Button();
        button.Text = $"{talent.Name} (Max {talent.MaxLevel})";
        button.CustomMinimumSize = new Vector2(200, 50);
        button.Pressed += () => OnTalentSelected(talent.Id);
        parent.AddChild(button);
        
        _categoryButtons[talent.Id] = button;
    }
    
    private void SetupCategoryTabs()
    {
        // Select first category by default
        if (_database.Categories.Count > 0)
        {
            _selectedCategoryId = _database.Categories[0].Id;
        }
    }
    
    public void SetSelectedPet(Pet pet)
    {
        _selectedPet = pet;
        RefreshUI();
    }
    
    private void RefreshUI()
    {
        if (_selectedPet == null) return;
        
        var data = _talentSystem.GetOrCreatePetTalentData(_selectedPet.GetInstanceId());
        _pointsLabel.Text = $"Available Points: {data.AvailablePoints}";
        
        // Update talent button states
        foreach (var category in _database.Categories)
        {
            foreach (var talent in category.Talents)
            {
                if (_categoryButtons.ContainsKey(talent.Id))
                {
                    var button = _categoryButtons[talent.Id];
                    int currentLevel = data.UnlockedTalents.ContainsKey(talent.Id) 
                        ? data.UnlockedTalents[talent.Id] 
                        : 0;
                    
                    if (currentLevel >= talent.MaxLevel)
                    {
                        button.Text = $"{talent.Name} [MAX]";
                    }
                    else
                    {
                        button.Text = $"{talent.Name} (Lv {currentLevel}/{talent.MaxLevel})";
                    }
                }
            }
        }
        
        // Update details panel if talent selected
        if (!string.IsNullOrEmpty(_selectedTalentId))
        {
            UpdateTalentDetails(_selectedTalentId);
        }
    }
    
    private void OnTalentSelected(string talentId)
    {
        _selectedTalentId = talentId;
        UpdateTalentDetails(talentId);
    }
    
    private void UpdateTalentDetails(string talentId)
    {
        var talent = _database.GetTalent(talentId);
        if (talent == null || _selectedPet == null) return;
        
        var data = _talentSystem.GetOrCreatePetTalentData(_selectedPet.GetInstanceId());
        int currentLevel = data.UnlockedTalents.ContainsKey(talentId) 
            ? data.UnlockedTalents[talentId] 
            : 0;
        
        _talentNameLabel.Text = talent.Name;
        _talentDescriptionLabel.Text = talent.Description;
        _talentLevelLabel.Text = $"Current Level: {currentLevel}/{talent.MaxLevel} | Points per level: {talent.PointsPerLevel}";
        
        // Build bonus text
        string bonusText = "Bonuses per level:\n";
        if (talent.AttackBonus > 0) bonusText += $"  Attack +{talent.AttackBonus}\n";
        if (talent.DefenseBonus > 0) bonusText += $"  Defense +{talent.DefenseBonus}\n";
        if (talent.HealthBonus > 0) bonusText += $"  Health +{talent.HealthBonus}\n";
        if (talent.SpeedBonus > 0) bonusText += $"  Speed +{talent.SpeedBonus}\n";
        if (talent.CritRateBonus > 0) bonusText += $"  Crit Rate +{talent.CritRateBonus}%\n";
        if (talent.CritDamageBonus > 0) bonusText += $"  Crit Damage +{talent.CritDamageBonus}%\n";
        if (talent.LifeStealBonus > 0) bonusText += $"  Life Steal +{talent.LifeStealBonus}%\n";
        if (talent.DodgeBonus > 0) bonusText += $"  Dodge +{talent.DodgeBonus}%\n";
        
        _talentBonusLabel.Text = bonusText;
        
        // Update allocate button
        if (currentLevel >= talent.MaxLevel)
        {
            _allocateButton.Text = "Max Level";
            _allocateButton.Disabled = true;
        }
        else if (data.AvailablePoints < talent.PointsPerLevel)
        {
            _allocateButton.Text = "Not Enough Points";
            _allocateButton.Disabled = true;
        }
        else
        {
            _allocateButton.Text = $"Allocate ({talent.PointsPerLevel} point(s))";
            _allocateButton.Disabled = false;
        }
    }
    
    private void OnAllocatePressed()
    {
        if (_selectedPet == null || string.IsNullOrEmpty(_selectedTalentId)) return;
        
        if (_talentSystem.AllocateTalent(_selectedPet.GetInstanceId(), _selectedTalentId))
        {
            RefreshUI();
        }
    }
    
    private void OnResetPressed()
    {
        if (_selectedPet == null) return;
        
        if (_talentSystem.ResetTalent(_selectedPet.GetInstanceId()))
        {
            RefreshUI();
        }
    }
    
    private void OnClosePressed()
    {
        Visible = false;
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                Visible = false;
            }
            else if (keyEvent.Keycode == Key.T)
            {
                // Toggle visibility
                Visible = !Visible;
                if (Visible)
                {
                    RefreshUI();
                }
            }
        }
    }
    
    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshUI();
        }
    }
}
