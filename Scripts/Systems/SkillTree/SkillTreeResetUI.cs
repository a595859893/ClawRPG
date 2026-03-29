using Godot;
using System;
using System.Collections.Generic;

public partial class SkillTreeResetUI : Control
{
    private static SkillTreeResetUI _instance;
    public static SkillTreeResetUI Instance => _instance;
    
    // UI Elements
    private Label _titleLabel;
    private Label _statusLabel;
    private Label _pointsLabel;
    private Label _goldLabel;
    private Label _costLabel;
    private Label _infoLabel;
    
    private Button _resetButton;
    private Button _closeButton;
    private Button _statsButton;
    
    private VBoxContainer _statsContainer;
    private VBoxContainer _historyContainer;
    
    private bool _showingStats = true;
    
    public override void _Ready()
    {
        _instance = this;
        
        // Create main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsAndOffsetsPreset(Control.Preset.Center);
        mainContainer.CustomMinimumSize = new Vector2(500, 450);
        mainContainer.AddThemeConstantOverride("separation", 15);
        AddChild(mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🔄 Skill Tree Reset";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);
        
        // Status
        _statusLabel = new Label();
        _statusLabel.Text = "Manage your skill tree resets";
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _statusLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        mainContainer.AddChild(_statusLabel);
        
        // Separator
        mainContainer.AddChild(CreateSeparator());
        
        // Points Info
        var pointsContainer = new HBoxContainer();
        mainContainer.AddChild(pointsContainer);
        
        _pointsLabel = new Label();
        _pointsLabel.Text = "Available Points: 10";
        _pointsLabel.CustomMinimumSize = new Vector2(200, 0);
        pointsContainer.AddChild(_pointsLabel);
        
        _goldLabel = new Label();
        _goldLabel.Text = "Gold: 10000";
        _goldLabel.HorizontalAlignment = HorizontalAlignment.Right;
        pointsContainer.AddChild(_goldLabel);
        
        // Separator
        mainContainer.AddChild(CreateSeparator());
        
        // Reset Cost
        var costContainer = new HBoxContainer();
        mainContainer.AddChild(costContainer);
        
        var costTitleLabel = new Label();
        costTitleLabel.Text = "Reset Cost: ";
        costContainer.AddChild(costTitleLabel);
        
        _costLabel = new Label();
        _costLabel.Text = "FREE (1 remaining)";
        _costLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 0.3f));
        costContainer.AddChild(_costLabel);
        
        // Info
        _infoLabel = new Label();
        _infoLabel.Text = "Refund: 75% of spent points";
        _infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _infoLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
        mainContainer.AddChild(_infoLabel);
        
        // Separator
        mainContainer.AddChild(CreateSeparator());
        
        // Buttons
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.Alignment.Center;
        buttonContainer.CustomMinimumSize = new Vector2(400, 40);
        mainContainer.AddChild(buttonContainer);
        
        _resetButton = new Button();
        _resetButton.Text = "🔄 Reset Skill Tree";
        _resetButton.CustomMinimumSize = new Vector2(180, 40);
        _resetButton.Pressed += OnResetPressed;
        buttonContainer.AddChild(_resetButton);
        
        _statsButton = new Button();
        _statsButton.Text = "📊 Statistics";
        _statsButton.CustomMinimumSize = new Vector2(120, 40);
        _statsButton.Pressed += OnStatsPressed;
        buttonContainer.AddChild(_statsButton);
        
        // Stats/History Container
        _statsContainer = new VBoxContainer();
        _statsContainer.CustomMinimumSize = new Vector2(450, 150);
        mainContainer.AddChild(_statsContainer);
        
        _historyContainer = new VBoxContainer();
        _historyContainer.CustomMinimumSize = new Vector2(450, 150);
        _historyContainer.Visible = false;
        mainContainer.AddChild(_historyContainer);
        
        // Separator
        mainContainer.AddChild(CreateSeparator());
        
        // Close Button
        _closeButton = new Button();
        _closeButton.Text = "❌ Close";
        _closeButton.CustomMinimumSize = new Vector2(100, 35);
        _closeButton.Pressed += OnClosePressed;
        mainContainer.AddChild(_closeButton);
        
        // Initial update
        UpdateUI();
        
        GD.Print("=== Skill Tree Reset UI Ready ===");
    }
    
    private HSeparator CreateSeparator()
    {
        var sep = new HSeparator();
        sep.AddThemeConstantOverride("separation", 10);
        return sep;
    }
    
    private void UpdateUI()
    {
        var system = SkillTreeResetSystem.Instance;
        var data = SkillTreeResetData.Instance;
        
        // Update points and gold
        string pointsText = system.GetStatistics().TotalResets > 0 && data.FreeResetsRemaining == 0 ? "N/A" : "10";
        _pointsLabel.Text = $"Available Points: {pointsText}";
        _goldLabel.Text = $"Gold: 10000";
        
        // Update cost
        int cost = system.GetResetCost();
        if (cost == 0 && data.FreeResetsRemaining > 0)
        {
            _costLabel.Text = $"FREE ({data.FreeResetsRemaining} remaining)";
            _costLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 0.3f));
            _resetButton.Disabled = false;
        }
        else if (cost == 0)
        {
            _costLabel.Text = "Daily Free: Used";
            _costLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.7f, 0.3f));
            _resetButton.Disabled = false;
        }
        else
        {
            _costLabel.Text = $"{cost} Gold";
            _costLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.3f, 0.3f));
            _resetButton.Disabled = cost > 10000; // Not enough gold
        }
        
        // Update stats
        UpdateStatsDisplay();
        UpdateHistoryDisplay();
    }
    
    private void UpdateStatsDisplay()
    {
        _statsContainer.GetChildren().ForEach(c => c.QueueFree());
        
        var stats = SkillTreeResetSystem.Instance.GetStatistics();
        
        // Title
        var title = new Label();
        title.Text = "📊 Reset Statistics";
        title.AddThemeFontSizeOverride("font_size", 16);
        _statsContainer.AddChild(title);
        
        // Stats grid
        var statsText = new Label();
        statsText.Text = $"Total Resets: {stats.TotalResets}\n" +
                        $"Free Used: {stats.FreeResetsUsed}\n" +
                        $"Paid Used: {stats.PaidResetsUsed}\n" +
                        $"Free Remaining: {stats.FreeResetsRemaining}\n" +
                        $"Points Recovered: {stats.TotalPointsRecovered}\n" +
                        $"Gold Spent: {stats.TotalGoldSpent}\n" +
                        $"Max Points Reset: {stats.MaxPointsInSingleReset}\n" +
                        $"Avg Points/Reset: {stats.AveragePointsPerReset}\n" +
                        $"Refund Rate: {stats.RefundPercentage}%";
        statsText.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        _statsContainer.AddChild(statsText);
    }
    
    private void UpdateHistoryDisplay()
    {
        _historyContainer.GetChildren().ForEach(c => c.QueueFree());
        
        var data = SkillTreeResetData.Instance;
        
        // Title
        var title = new Label();
        title.Text = "📜 Reset History";
        title.AddThemeFontSizeOverride("font_size", 16);
        _historyContainer.AddChild(title);
        
        if (data.ResetHistory.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No reset history yet";
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            _historyContainer.AddChild(emptyLabel);
            return;
        }
        
        // Show last 5 resets
        var count = 0;
        for (int i = data.ResetHistory.Count - 1; i >= 0 && count < 5; i--)
        {
            var record = data.ResetHistory[i];
            var recordLabel = new Label();
            recordLabel.Text = $"[{record.ResetType}] {record.PointsReset} pts → +{record.PointsRecovered} pts" +
                             (record.GoldSpent > 0 ? $" ({record.GoldSpent}g)" : "");
            recordLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            _historyContainer.AddChild(recordLabel);
            count++;
        }
    }
    
    private void OnResetPressed()
    {
        var system = SkillTreeResetSystem.Instance;
        var result = system.ResetSkillTree();
        
        if (result.Success)
        {
            GD.Print($"Reset successful! Recovered {result.PointsRecovered} points");
            UpdateUI();
            
            // Show feedback
            _statusLabel.Text = $"✅ Reset! Recovered {result.PointsRecovered} skill points";
            _statusLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 0.3f));
        }
        else
        {
            GD.Print($"Reset failed: {result.ErrorMessage}");
            _statusLabel.Text = $"❌ {result.ErrorMessage}";
            _statusLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.3f, 0.3f));
        }
    }
    
    private void OnStatsPressed()
    {
        _showingStats = !_showingStats;
        _statsContainer.Visible = _showingStats;
        _historyContainer.Visible = !_showingStats;
        
        _statsButton.Text = _showingStats ? "📜 History" : "📊 Statistics";
    }
    
    private void OnClosePressed()
    {
        Visible = false;
    }
    
    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            UpdateUI();
        }
    }
}
