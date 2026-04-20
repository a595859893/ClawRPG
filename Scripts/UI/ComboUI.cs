using Godot;
using System;
using System.Collections.Generic;

public partial class ComboUI : Control
{
    private SkillComboSystem _comboSystem;
    
    // UI Elements
    private Label _titleLabel;
    private Label _pointsLabel;
    private Label _levelLabel;
    private ProgressBar _levelProgressBar;
    private GridContainer _comboGrid;
    private Label _progressLabel;
    private Label _helpLabel;
    
    // Combo display elements
    private Dictionary<string, Control> _comboCards = new Dictionary<string, Control>();
    
    // State
    private bool _isVisible = false;
    private ComboData.ComboType _currentFilter = ComboData.ComboType.Offensive;
    
    public override void _Ready()
    {
        _comboSystem = SkillComboSystem.Instance;
        if (_comboSystem == null)
        {
            GD.PrintErr("[ComboUI] SkillComboSystem not found!");
            return;
        }
        
        _SetupUI();
        _ConnectSignals();
        
        // Initialize hidden
        Visible = false;
    }
    
    private void _SetupUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // Title bar
        var titleBar = new HBoxContainer();
        mainContainer.AddChild(titleBar);
        
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ Skill Combo System";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleBar.AddChild(_titleLabel);
        
        titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand }); // Spacer
        
        // Points and Level display
        var statsContainer = new HBoxContainer();
        statsContainer.AddThemeConstantOverride("separation", 20);
        mainContainer.AddChild(statsContainer);
        
        _pointsLabel = new Label();
        _pointsLabel.Text = "Combo Points: 0";
        _pointsLabel.AddThemeFontSizeOverride("font_size", 18);
        statsContainer.AddChild(_pointsLabel);
        
        _levelLabel = new Label();
        _levelLabel.Text = "Combo Level: 1";
        _levelLabel.AddThemeFontSizeOverride("font_size", 18);
        statsContainer.AddChild(_levelLabel);
        
        // Level progress bar
        _levelProgressBar = new ProgressBar();
        _levelProgressBar.MinValue = 0;
        _levelProgressBar.MaxValue = 100;
        _levelProgressBar.Value = 0;
        _levelProgressBar.CustomMinimumSize = new Vector2(0, 20);
        mainContainer.AddChild(_levelProgressBar);
        
        // Filter buttons
        var filterContainer = new HBoxContainer();
        filterContainer.AddThemeConstantOverride("separation", 10);
        mainContainer.AddChild(filterContainer);
        
        _CreateFilterButton(filterContainer, "Offensive", ComboData.ComboType.Offensive);
        _CreateFilterButton(filterContainer, "Defensive", ComboData.ComboType.Defensive);
        _CreateFilterButton(filterContainer, "Support", ComboData.ComboType.Support);
        _CreateFilterButton(filterContainer, "Utility", ComboData.ComboType.Utility);
        _CreateFilterButton(filterContainer, "Special", ComboData.ComboType.Special);
        _CreateFilterButton(filterContainer, "All", (ComboData.ComboType)99); // Special value for all
        
        // Combo grid
        _comboGrid = new GridContainer();
        _comboGrid.Columns = 3;
        _comboGrid.AddThemeConstantOverride("h_separation", 10);
        _comboGrid.AddThemeConstantOverride("v_separation", 10);
        mainContainer.AddChild(_comboGrid);
        
        // Progress info
        _progressLabel = new Label();
        _progressLabel.Text = "";
        _progressLabel.AddThemeFontSizeOverride("font_size", 14);
        mainContainer.AddChild(_progressLabel);
        
        // Help text
        _helpLabel = new Label();
        _helpLabel.Text = "Use skills in sequence to trigger combos! | Press [C] to toggle";
        _helpLabel.AddThemeFontSizeOverride("font_size", 12);
        _helpLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        mainContainer.AddChild(_helpLabel);
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += () => ToggleVisibility(false);
        mainContainer.AddChild(closeButton);
        
        _RefreshComboDisplay();
    }
    
    private void _CreateFilterButton(Container parent, string text, ComboData.ComboType type)
    {
        var button = new Button();
        button.Text = text;
        button.Pressed += () => 
        {
            _currentFilter = type;
            _RefreshComboDisplay();
        };
        parent.AddChild(button);
    }
    
    private void _ConnectSignals()
    {
        if (_comboSystem != null)
        {
            SkillComboSystem.ComboPointsChanged += OnComboPointsChanged;
            SkillComboSystem.ComboLevelChanged += OnComboLevelChanged;
            SkillComboSystem.ComboProgressUpdated += OnComboProgressUpdated;
        }
    }
    
    private void _RefreshComboDisplay()
    {
        // Clear existing cards
        foreach (var child in _comboGrid.GetChildren())
        {
            child.QueueFree();
        }
        _comboCards.Clear();
        
        if (_comboSystem == null) return;
        
        var combos = _currentFilter == (ComboData.ComboType)99
            ? _comboSystem.GetUnlockedCombos()
            : _comboSystem.GetCombosByType(_currentFilter);

        foreach (var combo in combos)
        {
            var card = _CreateComboCard(combo);
            _comboGrid.AddChild(card);
            _comboCards[combo.ComboId] = card;
        }
        
        // Update stats display
        _UpdateStats();
    }
    
    private Control _CreateComboCard(SkillCombo combo)
    {
        var card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(250, 150);
        
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 5);
        card.AddChild(vbox);
        
        // Name and rarity
        var nameLabel = new Label();
        nameLabel.Text = combo.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        
        // Color by rarity
        Color rarityColor = _GetRarityColor(combo.Rarity);
        nameLabel.AddThemeColorOverride("font_color", rarityColor);
        vbox.AddChild(nameLabel);
        
        // Description
        var descLabel = new Label();
        descLabel.Text = combo.Description ?? "";
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        vbox.AddChild(descLabel);
        
        // Skill sequence
        var seqLabel = new Label();
        seqLabel.Text = "Sequence: " + string.Join(" → ", combo.SkillIds ?? new System.Collections.Generic.List<string>());
        seqLabel.AddThemeFontSizeOverride("font_size", 11);
        seqLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        vbox.AddChild(seqLabel);
        
        // Stats
        var statsLabel = new Label();
        statsLabel.Text = $"DMG: {combo.Bonus?.DamageMultiplier ?? 1f}x | Points: +{combo.ComboPointReward}";
        statsLabel.AddThemeFontSizeOverride("font_size", 11);
        statsLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.6f));
        vbox.AddChild(statsLabel);
        
        // Execution count
        var progress = _comboSystem.GetPlayerProgress();
        if (progress.TryGetValue(combo.ComboId, out var prog) && prog.TimesExecuted > 0)
        {
            var execLabel = new Label();
            execLabel.Text = $"Executed: {prog.TimesExecuted}x";
            execLabel.AddThemeFontSizeOverride("font_size", 10);
            execLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 0.6f));
            vbox.AddChild(execLabel);
        }
        
        // Type badge
        var typeLabel = new Label();
        typeLabel.Text = combo.OldComboType.ToString();
        typeLabel.AddThemeFontSizeOverride("font_size", 10);
        typeLabel.AddThemeColorOverride("font_color", _GetTypeColor(combo.OldComboType));
        vbox.AddChild(typeLabel);
        
        return card;
    }
    
    private Color _GetRarityColor(ComboData.Rarity rarity)
    {
        return rarity switch
        {
            ComboData.Rarity.Common => new Color(0.7f, 0.7f, 0.7f),
            ComboData.Rarity.Uncommon => new Color(0.4f, 0.9f, 0.4f),
            ComboData.Rarity.Rare => new Color(0.4f, 0.6f, 1.0f),
            ComboData.Rarity.Epic => new Color(0.7f, 0.4f, 1.0f),
            ComboData.Rarity.Legendary => new Color(1.0f, 0.7f, 0.0f),
            _ => Colors.White
        };
    }
    
    private Color _GetTypeColor(ComboData.ComboType type)
    {
        return type switch
        {
            ComboData.ComboType.Offensive => new Color(1.0f, 0.3f, 0.3f),
            ComboData.ComboType.Defensive => new Color(0.3f, 0.3f, 1.0f),
            ComboData.ComboType.Support => new Color(0.3f, 1.0f, 0.3f),
            ComboData.ComboType.Utility => new Color(0.7f, 0.7f, 0.3f),
            ComboData.ComboType.Special => new Color(1.0f, 0.5f, 1.0f),
            _ => Colors.White
        };
    }
    
    private void _UpdateStats()
    {
        if (_comboSystem == null) return;
        
        _pointsLabel.Text = $"Combo Points: {_comboSystem.GetComboPoints()}";
        _levelLabel.Text = $"Combo Level: {_comboSystem.GetComboLevel()}";
        
        int currentLevel = _comboSystem.GetComboLevel();
        int pointsForLevel = currentLevel * 50;
        int pointsInLevel = _comboSystem.GetComboPoints() % pointsForLevel;
        _levelProgressBar.MaxValue = pointsForLevel;
        _levelProgressBar.Value = pointsInLevel;
    }
    
    private void OnComboPointsChanged(int newPoints)
    {
        _pointsLabel.Text = $"Combo Points: {newPoints}";
        _UpdateStats();
    }
    
    private void OnComboLevelChanged(int newLevel)
    {
        _levelLabel.Text = $"Combo Level: {newLevel}";
        _RefreshComboDisplay(); // Refresh to show newly unlocked combos
    }
    
    private void OnComboProgressUpdated(string comboId, int currentStep, float timeRemaining)
    {
        var progress = _comboSystem.GetPlayerProgress();
        if (progress.TryGetValue(comboId, out var prog))
        {
            var combo = _comboSystem.GetAllCombos()[comboId];
            int totalSteps = combo.SkillIds?.Count ?? 0;
            _progressLabel.Text = $"▶ {combo.Name}: Step {currentStep}/{totalSteps} ({timeRemaining:F1}s)";
        }
    }
    
    public void ToggleVisibility(bool? force = null)
    {
        bool newState = force ?? !Visible;
        Visible = newState;
        _isVisible = newState;
        
        if (_isVisible)
        {
            _RefreshComboDisplay();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.C)
            {
                ToggleVisibility();
            }
            else if (keyEvent.Keycode == Key.Escape && _isVisible)
            {
                ToggleVisibility(false);
            }
        }
    }
    
    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            if (_comboSystem != null)
            {
                SkillComboSystem.ComboPointsChanged -= OnComboPointsChanged;
                SkillComboSystem.ComboLevelChanged -= OnComboLevelChanged;
                SkillComboSystem.ComboProgressUpdated -= OnComboProgressUpdated;
            }
        }
    }
}
