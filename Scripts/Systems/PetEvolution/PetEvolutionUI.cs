using System;
using System.Collections.Generic;
using Godot;

public partial class PetEvolutionUI : Control
{
    private Panel _mainPanel;
    private VBoxContainer _mainContainer;
    private TabContainer _tabContainer;
    
    // Overview tab
    private VBoxContainer _overviewTab;
    private Label _titleLabel;
    private Label _statsLabel;
    private PetEvolutionList _petList;
    
    // Evolution tab
    private VBoxContainer _evolutionTab;
    private PetEvolutionDetails _details;
    
    // Statistics tab
    private VBoxContainer _statisticsTab;
    private Label _statisticsLabel;
    
    private bool _isVisible = false;
    private int _selectedPetId = -1;

    public override void _Ready()
    {
        _mainPanel = new Panel();
        _mainPanel.SetAnchorsPreset(ControlPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_mainPanel);
        _mainPanel.Visible = false;

        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(ControlPreset.FullRect);
        _mainContainer.AddThemeConstantOverride("separation", 10);
        _mainPanel.AddChild(_mainContainer);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🐾 Pet Evolution System";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(_titleLabel);

        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.Vertical);
        _mainContainer.AddChild(_tabContainer);

        // Create tabs
        CreateOverviewTab();
        CreateEvolutionTab();
        CreateStatisticsTab();

        // Close button
        Button closeBtn = new Button();
        closeBtn.Text = "Close (ESC)";
        closeBtn.Pressed += () => ToggleVisibility(false);
        _mainContainer.AddChild(closeBtn);

        // Initialize system
        PetEvolutionSystem.Instance.Initialize();
        UpdateDisplay();
    }

    private void CreateOverviewTab()
    {
        _overviewTab = new VBoxContainer();
        _overviewTab.Name = "Overview";
        _tabContainer.AddChild(_overviewTab);

        Label overviewTitle = new Label();
        overviewTitle.Text = "Your Pets Evolution Status";
        overviewTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _overviewTab.AddChild(overviewTitle);

        // Pet list
        _petList = new PetEvolutionList();
        _petList.ItemSelected += OnPetSelected;
        _overviewTab.AddChild(_petList);
    }

    private void CreateEvolutionTab()
    {
        _evolutionTab = new VBoxContainer();
        _evolutionTab.Name = "Evolution";
        _tabContainer.AddChild(_evolutionTab);

        Label evolutionTitle = new Label();
        evolutionTitle.Text = "Evolution Details";
        evolutionTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _evolutionTab.AddChild(evolutionTitle);

        _details = new PetEvolutionDetails();
        _evolutionTab.AddChild(_details);
    }

    private void CreateStatisticsTab()
    {
        _statisticsTab = new VBoxContainer();
        _statisticsTab.Name = "Statistics";
        _tabContainer.AddChild(_statisticsTab);

        Label statsTitle = new Label();
        statsTitle.Text = "Evolution Statistics";
        statsTitle.HorizontalAlignment = HorizontalAlignment.Center;
        _statisticsTab.AddChild(statsTitle);

        _statisticsLabel = new Label();
        _statisticsTab.AddChild(_statisticsLabel);
    }

    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                ToggleVisibility(false);
            }
        }
    }

    public void ToggleVisibility(bool? force = null)
    {
        if (force.HasValue)
            _isVisible = force.Value;
        else
            _isVisible = !_isVisible;

        _mainPanel.Visible = _isVisible;
        
        if (_isVisible)
        {
            UpdateDisplay();
        }
    }

    private void OnPetSelected(long id)
    {
        _selectedPetId = (int)id;
        UpdateDetails();
    }

    private void UpdateDisplay()
    {
        UpdatePetList();
        UpdateStatistics();
    }

    private void UpdatePetList()
    {
        if (_petList == null) return;
        
        var records = PetEvolutionSystem.Instance.GetAllEvolutionRecords();
        
        // Clear existing items
        foreach (var child in _petList.GetChildren())
        {
            child.QueueFree();
        }

        // Add pets
        var petTypes = new[] { "Dog", "Cat", "Bird", "Rabbit", "Dragon", "Slime", "Skeleton", "Elemental" };
        
        int index = 0;
        foreach (var petType in petTypes)
        {
            for (int i = 1; i <= 3; i++) // Simulate 3 pets per type
            {
                int petId = index * 10 + i;
                var progress = PetEvolutionSystem.Instance.GetEvolutionProgress(petId, petType);
                
                var item = new ListBoxItem();
                item.Text = $"{petType} #{i}: {progress.CurrentForm}";
                item.SetMetadata(0, petId);
                _petList.AddChild(item);
                
                index++;
            }
        }
    }

    private void UpdateDetails()
    {
        if (_details == null || _selectedPetId < 0) return;
        
        // Find pet type from ID
        string[] petTypes = { "Dog", "Cat", "Bird", "Rabbit", "Dragon", "Slime", "Skeleton", "Elemental" };
        int typeIndex = _selectedPetId / 10;
        string petType = petTypes[Mathf.Min(typeIndex, petTypes.Length - 1)];
        
        var progress = PetEvolutionSystem.Instance.GetEvolutionProgress(_selectedPetId, petType);
        _details.UpdateDetails(progress, petType);
    }

    private void UpdateStatistics()
    {
        if (_statisticsLabel == null) return;
        
        var stats = PetEvolutionSystem.Instance.GetStatistics();
        _statisticsLabel.Text = $"Total Evolutions: {stats["total_evolutions"]}\n" +
            $"Legendary: {stats["legendary_evolutions"]}\n" +
            $"Epic: {stats["epic_evolutions"]}\n" +
            $"Rare: {stats["rare_evolutions"]}\n" +
            $"Total Points Spent: {stats["total_points"]}\n" +
            $"History Entries: {stats["history_count"]}";
    }

    public void AddTestPoints(int petId, string petType, int points)
    {
        PetEvolutionSystem.Instance.AddEvolutionPoints(petId, petType, points);
        UpdateDisplay();
    }

    public void TriggerEvolution(int petId, string petType)
    {
        var result = PetEvolutionSystem.Instance.EvolvePet(petId, petType);
        
        if (result.Success)
        {
            GD.Print(result.Message);
            UpdateDisplay();
        }
    }
}

public class PetEvolutionList : ItemList
{
    public PetEvolutionList()
    {
        CustomMinimumSize = new Vector2(0, 300);
    }
}

public class PetEvolutionDetails : VBoxContainer
{
    private Label _currentFormLabel;
    private Label _nextFormLabel;
    private ProgressBar _progressBar;
    private Label _pointsLabel;
    private Button _evolveButton;
    private Label _chainLabel;

    public PetEvolutionDetails()
    {
        CustomMinimumSize = new Vector2(0, 300);
        
        _currentFormLabel = new Label();
        _currentFormLabel.Text = "Current Form: None";
        AddChild(_currentFormLabel);
        
        _nextFormLabel = new Label();
        _nextFormLabel.Text = "Next Form: None";
        AddChild(_nextFormLabel);
        
        _progressBar = new ProgressBar();
        _progressBar.CustomMinimumSize = new Vector2(0, 30);
        AddChild(_progressBar);
        
        _pointsLabel = new Label();
        _pointsLabel.Text = "0 / 0 Evolution Points";
        AddChild(_pointsLabel);
        
        _evolveButton = new Button();
        _evolveButton.Text = "Evolve!";
        _evolveButton.Pressed += OnEvolvePressed;
        AddChild(_evolveButton);
        
        _chainLabel = new Label();
        _chainLabel.Text = "\nEvolution Chain:\n";
        AddChild(_chainLabel);
    }

    public void UpdateDetails(PetEvolutionSystem.EvolutionProgress progress, string petType)
    {
        _currentFormLabel.Text = $"Current Form: {progress.CurrentForm}";
        _nextFormLabel.Text = progress.IsMaxEvolution 
            ? "Maximum Evolution Reached!" 
            : $"Next Form: {progress.NextForm} ({progress.NextRarity})";
        
        _progressBar.MaxValue = progress.RequiredPoints > 0 ? progress.RequiredPoints : 1;
        _progressBar.Value = progress.CurrentPoints;
        
        _pointsLabel.Text = $"{progress.CurrentPoints} / {progress.RequiredPoints} Evolution Points";
        _evolveButton.Disabled = !progress.CanEvolve;
        
        // Show evolution chain
        var chain = PetEvolutionSystem.Instance.GetEvolutionChain(petType);
        string chainText = "\nEvolution Chain:\n";
        foreach (var form in chain)
        {
            chainText += $"• {form.FormName} (Tier {form.Tier}, {form.Rarity}) - {form.RequiredPoints} pts\n";
        }
        _chainLabel.Text = chainText;
    }

    private void OnEvolvePressed()
    {
        // This will be called from parent UI
    }
}
