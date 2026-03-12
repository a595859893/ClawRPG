using Godot;
using System;
using System.Collections.Generic;

public class DailyRitualUI : Control
{
    private VBoxContainer _mainContainer;
    private HBoxContainer _headerContainer;
    private Label _titleLabel;
    private Label _dailyCountLabel;
    private GridContainer _ritualGrid;
    private Label _statsLabel;
    private Button _closeButton;

    // Ritual item scene reference
    private PackedScene _ritualItemScene;

    private List<RitualData> _displayedRituals = new List<RitualData>();

    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        RefreshRitualList();
        UpdateStats();
    }

    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorAndMargin(AnchorPreset.CenterCenter, 0.5f);
        _mainContainer.CustomMinimumSize = new Vector2(700, 500);
        AddChild(_mainContainer);

        // Background panel
        var panel = new Panel();
        panel.SetAnchorAndMargin(AnchorPreset.FullRect, 0);
        panel.Modulate = new Color(1, 1, 1, 0.95f);
        _mainContainer.AddChild(panel);

        // Header
        _headerContainer = new HBoxContainer();
        _headerContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _mainContainer.AddChild(_headerContainer);

        _titleLabel = new Label();
        _titleLabel.Text = "✨ Daily Rituals ✨";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _headerContainer.AddChild(_titleLabel);

        _headerContainer.AddChild(new Control { CustomMinimumSize = new Vector2(50, 0) });

        _dailyCountLabel = new Label();
        _dailyCountLabel.Text = "Rituals Today: 3/3";
        _dailyCountLabel.AddThemeFontSizeOverride("font_size", 16);
        _headerContainer.AddChild(_dailyCountLabel);

        // Separator
        var hs = new HSeparator();
        _mainContainer.AddChild(hs);

        // Ritual Grid
        _ritualGrid = new GridContainer();
        _ritualGrid.Columns = 3;
        _ritualGrid.CustomMinimumSize = new Vector2(650, 300);
        _ritualGrid.AddThemeConstantOverride("separation", 10);
        _mainContainer.AddChild(_ritualGrid);

        // Stats section
        var statsContainer = new HBoxContainer();
        statsContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _mainContainer.AddChild(statsContainer);

        _statsLabel = new Label();
        _statsLabel.Text = "Total Performed: 0 | Gold Spent: 0 | Reputation: 0";
        _statsLabel.AddThemeFontSizeOverride("font_size", 14);
        statsContainer.AddChild(_statsLabel);

        // Bottom buttons
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _mainContainer.AddChild(buttonContainer);

        var clearButton = new Button();
        clearButton.Text = "Clear Bonuses";
        clearButton.Pressed += OnClearBonusesPressed;
        buttonContainer.AddChild(clearButton);

        buttonContainer.AddChild(new Control { CustomMinimumSize = new Vector2(20, 0) });

        _closeButton = new Button();
        _closeButton.Text = "Close";
        _closeButton.Pressed += OnClosePressed;
        buttonContainer.AddChild(_closeButton);

        // Add styles
        AddStyles();
    }

    private void AddStyles()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);
        
        foreach (var child in _mainContainer.Children)
        {
            if (child is Panel panel)
            {
                panel.AddThemeStyleboxOverride("panel", style);
            }
        }
    }

    private void ConnectSignals()
    {
        if (DailyRitualSystem.Instance != null)
        {
            DailyRitualSystem.Instance.Connect(nameof(DailyRitualSystem.RitualCompleted), this, nameof(OnRitualCompleted));
            DailyRitualSystem.Instance.Connect(nameof(DailyRitualSystem.RitualUnlocked), this, nameof(OnRitualUnlocked));
        }
    }

    private void RefreshRitualList()
    {
        // Clear existing items
        foreach (var child in _ritualGrid.GetChildren())
            child.QueueFree();
        _displayedRituals.Clear();

        // Get all rituals and filter by unlock status
        var allRituals = DailyRitualDatabase.Instance.GetAllRituals();
        
        foreach (var ritual in allRituals)
        {
            // Show novice always, others only if unlocked
            if (ritual.Tier == RitualTier.Novice || 
                DailyRitualSystem.Instance.UnlockedRitualIds.Contains(ritual.Id))
            {
                _displayedRituals.Add(ritual);
                CreateRitualCard(ritual);
            }
        }

        // Update daily count
        UpdateDailyCount();
    }

    private void CreateRitualCard(RitualData ritual)
    {
        var cardContainer = new VBoxContainer();
        cardContainer.CustomMinimumSize = new Vector2(200, 180);
        cardContainer.AddThemeConstantOverride("separation", 5);
        _ritualGrid.AddChild(cardContainer);

        // Card background
        var cardPanel = new Panel();
        cardPanel.CustomMinimumSize = new Vector2(200, 180);
        
        // Color by tier
        var tierColor = GetTierColor(ritual.Tier);
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = new Color(0.15f, 0.15f, 0.2f);
        cardStyle.BorderColor = tierColor;
        cardStyle.SetBorderWidthAll(2);
        cardStyle.SetCornerRadiusAll(6);
        cardPanel.AddThemeStyleboxOverride("panel", cardStyle);
        cardContainer.AddChild(cardPanel);

        // Name
        var nameLabel = new Label();
        nameLabel.Text = ritual.Name;
        nameLabel.Align = Label.AlignEnum.Center;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        cardContainer.AddChild(nameLabel);

        // Tier
        var tierLabel = new Label();
        tierLabel.Text = $"[{ritual.Tier}]";
        tierLabel.AddThemeColorOverride("font_color", tierColor);
        tierLabel.Align = Label.AlignEnum.Center;
        tierLabel.AddThemeFontSizeOverride("font_size", 12);
        cardContainer.AddChild(tierLabel);

        // Description
        var descLabel = new Label();
        descLabel.Text = ritual.Description;
        descLabel.Align = Label.AlignEnum.Center;
        descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        descLabel.CustomMinimumSize = new Vector2(180, 40);
        cardContainer.AddChild(descLabel);

        // Cost and duration
        var infoLabel = new Label();
        infoLabel.Text = $"💰 {ritual.GoldCost} | ⏱️ {ritual.Duration / 60:F0}min";
        infoLabel.Align = Label.AlignEnum.Center;
        infoLabel.AddThemeFontSizeOverride("font_size", 12);
        cardContainer.AddChild(infoLabel);

        // Bonuses
        var bonusText = "";
        foreach (var bonus in ritual.AttributeBonuses)
        {
            bonusText += $"{bonus.Key}: +{bonus.Value * 100:F0}% ";
        }
        var bonusLabel = new Label();
        bonusLabel.Text = bonusText;
        bonusLabel.Align = Label.AlignEnum.Center;
        bonusLabel.AddThemeFontSizeOverride("font_size", 11);
        bonusLabel.Modulate = new Color(0.7f, 0.9f, 0.7f);
        cardContainer.AddChild(bonusLabel);

        // Reputation
        var repLabel = new Label();
        repLabel.Text = $"⭐ +{ritual.ReputationGain} Rep";
        repLabel.Align = Label.AlignEnum.Center;
        repLabel.AddThemeFontSizeOverride("font_size", 11);
        cardContainer.AddChild(repLabel);

        // Start button
        var startButton = new Button();
        startButton.Text = "Perform Ritual";
        startButton.CustomMinimumSize = new Vector2(180, 30);
        
        // Disable if active or no daily rituals remaining
        bool canPerform = !DailyRitualSystem.Instance.IsRitualActive && 
                         DailyRitualSystem.Instance.GetDailyRitualsRemaining() > 0;
        startButton.Disabled = !canPerform;
        
        startButton.Pressed += () => OnStartRitualPressed(ritual);
        cardContainer.AddChild(startButton);

        // Show current progress if this is the active ritual
        if (DailyRitualSystem.Instance.CurrentRitualId == ritual.Id)
        {
            var progressLabel = new Label();
            progressLabel.Text = $"🔮 In Progress...";
            progressLabel.Align = Label.AlignEnum.Center;
            progressLabel.Modulate = new Color(0.3f, 0.8f, 1f);
            cardContainer.AddChild(progressLabel);

            var progressBar = new ProgressBar();
            progressBar.CustomMinimumSize = new Vector2(180, 20);
            progressBar.PercentVisible = false;
            
            float progress = DailyRitualSystem.Instance.RitualProgress / ritual.Duration;
            progressBar.Value = progress * 100;
            
            var progressStyle = new StyleBoxFlat();
            progressStyle.BgColor = new Color(0.2f, 0.5f, 0.8f);
            progressBar.AddThemeStyleboxOverride("fill", progressStyle);
            cardContainer.AddChild(progressBar);
        }
    }

    private Color GetTierColor(RitualTier tier)
    {
        return tier switch
        {
            RitualTier.Novice => new Color(0.7f, 0.7f, 0.7f),
            RitualTier.Adept => new Color(0.3f, 0.7f, 0.3f),
            RitualTier.Master => new Color(0.5f, 0.5f, 1f),
            RitualTier.Legendary => new Color(1f, 0.7f, 0f),
            _ => new Color(1f, 1f, 1f)
        };
    }

    private void UpdateDailyCount()
    {
        int remaining = DailyRitualSystem.Instance.GetDailyRitualsRemaining();
        _dailyCountLabel.Text = $"Rituals Today: {remaining}/3";
        
        // Refresh the list to update button states
        RefreshRitualList();
    }

    private void UpdateStats()
    {
        _statsLabel.Text = $"Total Performed: {DailyRitualSystem.Instance.TotalRitualsPerformed} | " +
                          $"Gold Spent: {DailyRitualSystem.Instance.TotalGoldSpent} | " +
                          $"Reputation: {DailyRitualSystem.Instance.TotalReputationGained}";
    }

    private void OnStartRitualPressed(RitualData ritual)
    {
        var player = GetTree().CurrentScene?.GetNode<Player>("Player");
        if (player == null)
        {
            GD.Print("Player not found");
            return;
        }

        if (player.gold < ritual.GoldCost)
        {
            GD.Print("Not enough gold");
            return;
        }

        // Deduct gold
        player.gold -= ritual.GoldCost;

        // Start ritual
        if (DailyRitualSystem.Instance.StartRitual(ritual.Id, player.gold))
        {
            RefreshRitualList();
        }
    }

    private void OnClearBonusesPressed()
    {
        DailyRitualSystem.Instance.ClearBonuses();
    }

    private void OnClosePressed()
    {
        Hide();
        QueueFree();
    }

    private void OnRitualCompleted(string ritualId)
    {
        UpdateDailyCount();
        UpdateStats();
        RefreshRitualList();
    }

    private void OnRitualUnlocked(string ritualId)
    {
        RefreshRitualList();
    }

    // Toggle UI with key press
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.R)
        {
            if (IsVisibleInTree())
            {
                OnClosePressed();
            }
        }
    }
}
