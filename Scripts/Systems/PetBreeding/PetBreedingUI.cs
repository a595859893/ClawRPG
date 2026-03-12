using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class PetBreedingUI : Control
{
    private PetBreedingSystem _breedingSystem;
    private VBoxContainer _mainContainer;
    private TabContainer _tabContainer;

    // Pet selection
    private OptionButton _parent1Selector;
    private OptionButton _parent2Selector;
    private Label _selectedParent1Label;
    private Label _selectedParent2Label;
    private Button _breedButton;
    private Label _resultLabel;

    // Stats
    private Label _totalBreedsLabel;
    private Label _successRateLabel;
    private Label _legendaryCountLabel;

    // History
    private VBoxContainer _historyContainer;

    // Breed info
    private Label _breedInfoLabel;

    private Color _rarityCommon = new Color(0.7f, 0.7f, 0.7f);
    private Color _rarityUncommon = new Color(0.2f, 0.8f, 0.2f);
    private Color _rarityRare = new Color(0.2f, 0.5f, 1.0f);
    private Color _rarityEpic = new Color(0.6f, 0.3f, 0.9f);
    private Color _rarityLegendary = new Color(1.0f, 0.7f, 0.0f);

    public override void _Ready()
    {
        _breedingSystem = GetNode<PetBreedingSystem>("/root/PetBreedingSystem");
        if (_breedingSystem == null)
        {
            GD.PrintErr("PetBreedingSystem not found!");
            return;
        }

        SetupUI();
        ConnectSignals();
        RefreshUI();
    }

    private void SetupUI()
    {
        // Main panel
        var panel = new PanelContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            MarginLeft = 50,
            MarginTop = 50,
            MarginRight = -50,
            MarginBottom = -50
        };
        AddChild(panel);

        var panelStyle = new StyleBoxFlat();
        panelStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        panelStyle.CornerRadiusTopLeft = 10;
        panelStyle.CornerRadiusTopRight = 10;
        panelStyle.CornerRadiusBottomLeft = 10;
        panelStyle.CornerRadiusBottomRight = 10;
        panel.AddStyleboxOverride("panel", panelStyle);

        _mainContainer = new VBoxContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            CustomMinimumSize = new Vector2(600, 500)
        };
        panel.AddChild(_mainContainer);

        // Title
        var titleLabel = new Label
        {
            Text = "🐾 Pet Breeding System",
            Align = Label.AlignEnum.Center,
            CustomMinimumSize = new Vector2(0, 50)
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(titleLabel);

        // Tab container
        _tabContainer = new TabContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandAndFill,
            SizeFlagsVertical = SizeFlags.ExpandAndFill
        };
        _mainContainer.AddChild(_tabContainer);

        // Create tabs
        CreateBreedingTab();
        CreateHistoryTab();
        CreateStatisticsTab();

        // Close button
        var closeButton = new Button
        {
            Text = "Close (ESC)",
            CustomMinimumSize = new Vector2(120, 40)
        };
        closeButton.Pressed += () => Hide();
        _mainContainer.AddChild(closeButton);
    }

    private void CreateBreedingTab()
    {
        var tab = new VBoxContainer();
        tab.Name = "Breeding";
        _tabContainer.AddChild(tab);

        var infoLabel = new Label
        {
            Text = "Select two pets to breed",
            Align = Label.AlignEnum.Center
        };
        tab.AddChild(infoLabel);

        // Parent selection
        var selectionBox = new HBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandAndFill,
            CustomMinimumSize = new Vector2(0, 150)
        };
        tab.AddChild(selectionBox);

        // Parent 1
        var parent1Box = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandAndFill,
            SizeFlagsVertical = SizeFlags.ExpandAndFill
        };
        selectionBox.AddChild(parent1Box);

        var parent1Title = new Label { Text = "Parent 1", Align = Label.AlignEnum.Center };
        parent1Box.AddChild(parent1Title);

        _parent1Selector = new OptionButton
        {
            SizeFlagsHorizontal = SizeFlags.ExpandAndFill
        };
        parent1Box.AddChild(_parent1Selector);

        _selectedParent1Label = new Label { Text = "", Align = Label.AlignEnum.Center };
        parent1Box.AddChild(_selectedParent1Label);

        // VS label
        var vsLabel = new Label
        {
            Text = "×",
            CustomMinimumSize = new Vector2(50, 0),
            Align = Label.AlignEnum.Center
        };
        vsLabel.AddThemeFontSizeOverride("font_size", 32);
        selectionBox.AddChild(vsLabel);

        // Parent 2
        var parent2Box = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandAndFill,
            SizeFlagsVertical = SizeFlags.ExpandAndFill
        };
        selectionBox.AddChild(parent2Box);

        var parent2Title = new Label { Text = "Parent 2", Align = Label.AlignEnum.Center };
        parent2Box.AddChild(parent2Title);

        _parent2Selector = new OptionButton
        {
            SizeFlagsHorizontal = SizeFlags.ExpandAndFill
        };
        parent2Box.AddChild(_parent2Selector);

        _selectedParent2Label = new Label { Text = "", Align = Label.AlignEnum.Center };
        parent2Box.AddChild(_selectedParent2Label);

        // Breed info
        _breedInfoLabel = new Label
        {
            Text = "",
            Align = Label.AlignEnum.Center,
            CustomMinimumSize = new Vector2(0, 60)
        };
        tab.AddChild(_breedInfoLabel);

        // Breed button
        _breedButton = new Button
        {
            Text = "🔄 Breed Pets",
            CustomMinimumSize = new Vector2(200, 50)
        };
        _breedButton.Pressed += OnBreedPressed;
        tab.AddChild(_breedButton);

        // Result
        _resultLabel = new Label
        {
            Text = "",
            Align = Label.AlignEnum.Center,
            CustomMinimumSize = new Vector2(0, 80)
        };
        _resultLabel.AddThemeFontSizeOverride("font_size", 20);
        tab.AddChild(_resultLabel);

        // Populate selectors
        PopulatePetSelectors();
    }

    private void CreateHistoryTab()
    {
        var tab = new ScrollContainer();
        tab.Name = "History";
        _tabContainer.AddChild(tab);

        _historyContainer = new VBoxContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandAndFill,
            SizeFlagsVertical = SizeFlags.ExpandAndFill
        };
        tab.AddChild(_historyContainer);

        RefreshHistory();
    }

    private void CreateStatisticsTab()
    {
        var tab = new VBoxContainer();
        tab.Name = "Statistics";
        _tabContainer.AddChild(tab);

        var statsTitle = new Label
        {
            Text = "📊 Breeding Statistics",
            Align = Label.AlignEnum.Center
        };
        statsTitle.AddThemeFontSizeOverride("font_size", 20);
        tab.AddChild(statsTitle);

        _totalBreedsLabel = new Label { Text = "Total Breeds: 0" };
        tab.AddChild(_totalBreedsLabel);

        _successRateLabel = new Label { Text = "Success Rate: 0%" };
        tab.AddChild(_successRateLabel);

        _legendaryCountLabel = new Label { Text = "Legendary Offspring: 0" };
        tab.AddChild(_legendaryCountLabel);

        tab.AddChild(new HSeparator());

        var clearButton = new Button { Text = "Clear History" };
        clearButton.Pressed += () =>
        {
            _breedingSystem.ClearHistory();
            RefreshUI();
        };
        tab.AddChild(clearButton);

        RefreshStatistics();
    }

    private void PopulatePetSelectors()
    {
        var petTypes = _breedingSystem.GetAvailablePetTypes();

        _parent1Selector.Clear();
        _parent2Selector.Clear();

        foreach (var pet in petTypes)
        {
            _parent1Selector.AddItem(pet);
            _parent2Selector.AddItem(pet);
        }

        if (petTypes.Count > 1)
            _parent2Selector.Select(1);
    }

    private void ConnectSignals()
    {
        _parent1Selector.ItemSelected += (index) => UpdateBreedInfo();
        _parent2Selector.ItemSelected += (index) => UpdateBreedInfo();
    }

    private void UpdateBreedInfo()
    {
        string pet1 = _parent1Selector.GetItemText(_parent1Selector.Selected);
        string pet2 = _parent2Selector.GetItemText(_parent2Selector.Selected);

        _selectedParent1Label.Text = pet1;
        _selectedParent2Label.Text = pet2;

        var config = _breedingSystem.GetBreedConfig(pet1, pet2);
        if (config != null)
        {
            _breedInfoLabel.Text = $"{config.ResultName}\n{config.Description}\nSuccess Rate: {config.BaseSuccessRate * 100}%";
        }
        else
        {
            _breedInfoLabel.Text = "Generic Hybrid\nCustom combination\nSuccess Rate: 40%";
        }
    }

    private void OnBreedPressed()
    {
        string pet1 = _parent1Selector.GetItemText(_parent1Selector.Selected);
        string pet2 = _parent2Selector.GetItemText(_parent2Selector.Selected);

        if (pet1 == pet2)
        {
            _resultLabel.Text = "⚠️ Please select different pets!";
            _resultLabel.Modulate = new Color(1f, 0.5f, 0.5f);
            return;
        }

        var result = _breedingSystem.Breed(pet1, pet2);

        switch (result)
        {
            case PetBreedResult.Failure:
                _resultLabel.Text = "💔 Breeding Failed!\nThe pets were incompatible.";
                _resultLabel.Modulate = _rarityCommon;
                break;
            case PetBreedResult.Common:
                _resultLabel.Text = "✅ Success! Common Offspring\nThe breeding was successful.";
                _resultLabel.Modulate = _rarityCommon;
                break;
            case PetBreedResult.Uncommon:
                _resultLabel.Text = "✨ Success! Uncommon Offspring\nA rare find!";
                _resultLabel.Modulate = _rarityUncommon;
                break;
            case PetBreedResult.Rare:
                _resultLabel.Text = "🌟 Success! Rare Offspring\nAn exceptional companion!";
                _resultLabel.Modulate = _rarityRare;
                break;
            case PetBreedResult.Epic:
                _resultLabel.Text = "💎 Success! Epic Offspring\nA magnificent creature!";
                _resultLabel.Modulate = _rarityEpic;
                break;
            case PetBreedResult.Legendary:
                _resultLabel.Text = "👑 LEGENDARY Offspring! 👑\nA truly mythical companion!";
                _resultLabel.Modulate = _rarityLegendary;
                break;
        }

        RefreshUI();
    }

    private void RefreshUI()
    {
        PopulatePetSelectors();
        UpdateBreedInfo();
        RefreshHistory();
        RefreshStatistics();
    }

    private void RefreshHistory()
    {
        foreach (var child in _historyContainer.GetChildren())
            child.QueueFree();

        var history = _breedingSystem.GetBreedingHistory(20);

        if (history.Count == 0)
        {
            var emptyLabel = new Label { Text = "No breeding history yet" };
            _historyContainer.AddChild(emptyLabel);
            return;
        }

        foreach (var record in history)
        {
            var recordBox = new HBoxContainer
            {
                CustomMinimumSize = new Vector2(0, 40)
            };

            var color = GetRarityColor(record.Rarity);
            var rarityName = _breedingSystem.GetRarityNames()[record.Rarity];

            var label = new Label
            {
                Text = $"{record.Parent1Id} + {record.Parent2Id} → {record.OffspringType} [{rarityName}]",
                Modulate = color
            };
            recordBox.AddChild(label);

            _historyContainer.AddChild(recordBox);
        }
    }

    private void RefreshStatistics()
    {
        var data = _breedingSystem.GetData();
        _totalBreedsLabel.Text = $"Total Breeds: {data.TotalBreeds}";
        _successRateLabel.Text = $"Success Rate: {_breedingSystem.GetSuccessRate() * 100:F1}%";
        _legendaryCountLabel.Text = $"Legendary Offspring: {data.LegendaryBreeds}";
    }

    private Color GetRarityColor(int rarity)
    {
        switch (rarity)
        {
            case 1: return _rarityCommon;
            case 2: return _rarityUncommon;
            case 3: return _rarityRare;
            case 4: return _rarityEpic;
            case 5: return _rarityLegendary;
            default: return _rarityCommon;
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            Hide();
        }
    }

    private bool _isVisible = false;
    public void Toggle()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        if (_isVisible)
        {
            RefreshUI();
        }
    }
}
