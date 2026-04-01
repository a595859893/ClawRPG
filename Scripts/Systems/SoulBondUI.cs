using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 灵魂绑定用户界面。显示和管理灵魂绑定面板。
/// </summary>
public partial class SoulBondUI : Control
{
    private TabContainer _tabContainer;
    private VBoxContainer _bondListContainer;
    private VBoxContainer _historyContainer;
    private VBoxContainer _statsContainer;
    private Label _titleLabel;

    private Dictionary<string, SoulBondData> _displayedBonds;

    public override void _Ready()
    {
        _displayedBonds = new Dictionary<string, SoulBondData>();
        SetupUI();
        PopulateData();
    }

    private void SetupUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(AnchorPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "  Soul Bond System  ";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);

        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetAnchor(AnchorPreset.FullRect);
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);

        // Bonds tab
        var bondsTab = new ScrollContainer();
        bondsTab.Name = "Bonds";
        _tabContainer.AddChild(bondsTab);

        _bondListContainer = new VBoxContainer();
        _bondListContainer.SetAnchor(AnchorPreset.FullRect);
        _bondListContainer.AddThemeConstantOverride("separation", 10);
        bondsTab.AddChild(_bondListContainer);

        // History tab
        var historyTab = new ScrollContainer();
        historyTab.Name = "History";
        _tabContainer.AddChild(historyTab);

        _historyContainer = new VBoxContainer();
        _historyContainer.SetAnchor(AnchorPreset.FullRect);
        _historyContainer.AddThemeConstantOverride("separation", 5);
        historyTab.AddChild(_historyContainer);

        // Stats tab
        var statsTab = new ScrollContainer();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);

        _statsContainer = new VBoxContainer();
        _statsContainer.SetAnchor(AnchorPreset.FullRect);
        _statsContainer.AddThemeConstantOverride("separation", 10);
        statsTab.AddChild(_statsContainer);

        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Align = Button.AlignMode.Center;
        closeButton.Pressed += () => Hide();
        mainContainer.AddChild(closeButton);
    }

    private void PopulateData()
    {
        PopulateBondList();
        PopulateHistory();
        PopulateStatistics();
    }

    private void PopulateBondList()
    {
        foreach (var child in _bondListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var bonds = SoulBondSystem.Instance.ActiveBonds;

        if (bonds.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No bonds formed yet. Equip an item or pet to begin bonding.";
            _bondListContainer.AddChild(emptyLabel);
            return;
        }

        foreach (var kvp in bonds)
        {
            var bond = kvp.Value;
            var bondCard = CreateBondCard(bond);
            _bondListContainer.AddChild(bondCard);
        }
    }

    private Control CreateBondCard(SoulBondData bond)
    {
        var card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(0, 120);

        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 5);
        card.AddChild(container);

        // Header
        var header = new HBoxContainer();
        container.AddChild(header);

        var nameLabel = new Label();
        nameLabel.Text = $"📿 {bond.ItemOrPetId}";
        nameLabel.AddThemeFontSizeOverride("font_size", 18);
        header.AddChild(nameLabel);

        header.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        var typeLabel = new Label();
        typeLabel.Text = $"Type: {bond.BondType}";
        header.AddChild(typeLabel);

        // Level
        var levelLabel = new Label();
        levelLabel.Text = $"Level: {bond.CurrentLevel} | Points: {bond.TotalBondPoints}";
        container.AddChild(levelLabel);

        // Progress bar
        var progressBar = new ProgressBar();
        progressBar.MaxValue = bond.BondPointsToNextLevel > 0 ? bond.BondPointsToNextLevel : 1;
        progressBar.Value = bond.TotalBondPoints;
        progressBar.CustomMinimumSize = new Vector2(0, 20);
        container.AddChild(progressBar);

        // Abilities
        var abilitiesLabel = new Label();
        abilitiesLabel.Text = $"Unlocked Abilities: {string.Join(", ", bond.UnlockedAbilities)}";
        abilitiesLabel.AddThemeFontSizeOverride("font_size", 12);
        container.AddChild(abilitiesLabel);

        // Interact button
        var interactButton = new Button();
        interactButton.Text = "Bond (+10 Points)";
        interactButton.Pressed += () => OnInteractPressed(bond.ItemOrPetId);
        container.AddChild(interactButton);

        return card;
    }

    private void OnInteractPressed(string itemOrPetId)
    {
        SoulBondSystem.Instance.InteractWithBond(itemOrPetId, 10);
        PopulateData();
    }

    private void PopulateHistory()
    {
        foreach (var child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }

        var history = SoulBondSystem.Instance.BondHistory;

        if (history.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No bond history yet.";
            _historyContainer.AddChild(emptyLabel);
            return;
        }

        foreach (var record in history)
        {
            var recordLabel = new Label();
            recordLabel.Text = $"[{record.Timestamp:HH:mm:ss}] {record.ItemOrPetId}: {record.PreviousLevel} → {record.NewLevel}";
            _historyContainer.AddChild(recordLabel);
        }
    }

    private void PopulateStatistics()
    {
        foreach (var child in _statsContainer.GetChildren())
        {
            child.QueueFree();
        }

        var stats = SoulBondSystem.Instance.GetStatistics();

        foreach (var kvp in stats)
        {
            var statLabel = new Label();
            statLabel.Text = $"{kvp.Key}: {kvp.Value}";
            statLabel.AddThemeFontSizeOverride("font_size", 16);
            _statsContainer.AddChild(statLabel);
        }

        // Reset button
        var resetButton = new Button();
        resetButton.Text = "Reset All Bonds";
        resetButton.Pressed += OnResetPressed;
        _statsContainer.AddChild(resetButton);
    }

    private void OnResetPressed()
    {
        SoulBondSystem.Instance.ResetAllBonds();
        PopulateData();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Hide();
        }
    }

    public void Refresh()
    {
        PopulateData();
    }
}
