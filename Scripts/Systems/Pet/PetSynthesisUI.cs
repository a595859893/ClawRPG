using Godot;
using System;
using System.Collections.Generic;

public class PetSynthesisUI : Control
{
    private static PetSynthesisUI _instance;
    public static PetSynthesisUI Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new PetSynthesisUI();
            }
            return _instance;
        }
    }
    
    // UI Elements
    private Panel _mainPanel;
    private VBoxContainer _mainContainer;
    
    // Tabs
    private TabContainer _tabContainer;
    private Control _synthesisTab;
    private Control _historyTab;
    private Control _statisticsTab;
    
    // Synthesis tab
    private OptionButton _pet1Selector;
    private OptionButton _pet2Selector;
    private Label _recipeLabel;
    private Label _costLabel;
    private Label _successRateLabel;
    private Button _synthesizeButton;
    private Label _resultLabel;
    
    // History tab
    private ItemList _historyList;
    
    // Statistics tab
    private Label _totalSynthesesLabel;
    private Label _successfulSynthesesLabel;
    private Label _legendarySynthesesLabel;
    private Label _successRateStatLabel;
    private Label _totalGoldSpentLabel;
    
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        _instance = this;
        SetupUI();
        
        // Connect signals
        PetSynthesisSystem.Instance.Connect(PetSynthesisSystem.SignalSynthesisStarted, this, "_on_synthesis_started");
        PetSynthesisSystem.Instance.Connect(PetSynthesisSystem.SignalSynthesisCompleted, this, "_on_synthesis_completed");
        PetSynthesisSystem.Instance.Connect(PetSynthesisSystem.SignalSynthesisFailed, this, "_on_synthesis_failed");
    }
    
    private void SetupUI()
    {
        // Main panel
        _mainPanel = new Panel();
        _mainPanel.SetSize(new Vector2(600, 500));
        _mainPanel.RectPosition = new Vector2(100, 50);
        AddChild(_mainPanel);
        
        // Title bar
        var titleLabel = new Label();
        titleLabel.Text = "🐾 Pet Synthesis System";
        titleLabel.RectPosition = new Vector2(20, 10);
        titleLabel.AddColorOverride("font_color", new Color(1f, 0.84f, 0f));
        _mainPanel.AddChild(titleLabel);
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "X";
        closeButton.RectPosition = new Vector2(550, 10);
        closeButton.RectSize = new Vector2(30, 30);
        closeButton.Connect("pressed", this, "_on_close_pressed");
        _mainPanel.AddChild(closeButton);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.RectPosition = new Vector2(20, 50);
        _tabContainer.RectSize = new Vector2(560, 420);
        _mainPanel.AddChild(_tabContainer);
        
        SetupSynthesisTab();
        SetupHistoryTab();
        SetupStatisticsTab();
        
        _mainPanel.Visible = false;
    }
    
    private void SetupSynthesisTab()
    {
        _synthesisTab = new Control();
        _synthesisTab.Name = "Synthesis";
        _tabContainer.AddChild(_synthesisTab);
        
        var container = new VBoxContainer();
        container.RectPosition = new Vector2(20, 20);
        container.RectSize = new Vector2(520, 350);
        container.AddConstantOverride("separation", 15);
        _synthesisTab.AddChild(container);
        
        // Pet 1 selector
        var pet1Label = new Label();
        pet1Label.Text = "Select Pet 1:";
        container.AddChild(pet1Label);
        
        _pet1Selector = new OptionButton();
        _pet1Selector.RectSize = new Vector2(300, 30);
        _pet1Selector.Connect("item_selected", this, "_on_pet1_selected");
        container.AddChild(_pet1Selector);
        
        // Pet 2 selector
        var pet2Label = new Label();
        pet2Label.Text = "Select Pet 2:";
        container.AddChild(pet2Label);
        
        _pet2Selector = new OptionButton();
        _pet2Selector.RectSize = new Vector2(300, 30);
        _pet2Selector.Connect("item_selected", this, "_on_pet2_selected");
        container.AddChild(_pet2Selector);
        
        // Recipe info
        _recipeLabel = new Label();
        _recipeLabel.Text = "Recipe: -";
        container.AddChild(_recipeLabel);
        
        _costLabel = new Label();
        _costLabel.Text = "Cost: -";
        container.AddChild(_costLabel);
        
        _successRateLabel = new Label();
        _successRateLabel.Text = "Success Rate: -";
        container.AddChild(_successRateLabel);
        
        // Synthesize button
        _synthesizeButton = new Button();
        _synthesizeButton.Text = "🔮 Synthesize";
        _synthesizeButton.RectSize = new Vector2(200, 40);
        _synthesizeButton.Connect("pressed", this, "_on_synthesize_pressed");
        container.AddChild(_synthesizeButton);
        
        // Result label
        _resultLabel = new Label();
        _resultLabel.Text = "";
        _resultLabel.AddColorOverride("font_color", new Color(1f, 1f, 0f));
        container.AddChild(_resultLabel);
        
        // Help text
        var helpLabel = new Label();
        helpLabel.Text = "\nTips:\n- Combining pets may result in failure\n- Higher rarity = better stats\n- Some recipes require level prerequisites";
        helpLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        container.AddChild(helpLabel);
    }
    
    private void SetupHistoryTab()
    {
        _historyTab = new Control();
        _historyTab.Name = "History";
        _tabContainer.AddChild(_historyTab);
        
        _historyList = new ItemList();
        _historyList.RectPosition = new Vector2(20, 20);
        _historyList.RectSize = new Vector2(520, 350);
        _historyTab.AddChild(_historyList);
        
        // Refresh button
        var refreshButton = new Button();
        refreshButton.Text = "🔄 Refresh";
        refreshButton.RectPosition = new Vector2(420, 380);
        refreshButton.RectSize = new Vector2(120, 30);
        refreshButton.Connect("pressed", this, "_on_refresh_history_pressed");
        _historyTab.AddChild(refreshButton);
    }
    
    private void SetupStatisticsTab()
    {
        _statisticsTab = new Control();
        _statisticsTab.Name = "Statistics";
        _tabContainer.AddChild(_statisticsTab);
        
        var container = new VBoxContainer();
        container.RectPosition = new Vector2(30, 30);
        container.RectSize = new Vector2(500, 350);
        container.AddConstantOverride("separation", 20);
        _statisticsTab.AddChild(container);
        
        // Title
        var title = new Label();
        title.Text = "📊 Synthesis Statistics";
        title.AddColorOverride("font_color", new Color(1f, 0.84f, 0f));
        container.AddChild(title);
        
        _totalSynthesesLabel = new Label();
        _totalSynthesesLabel.Text = "Total Syntheses: 0";
        container.AddChild(_totalSynthesesLabel);
        
        _successfulSynthesesLabel = new Label();
        _successfulSynthesesLabel.Text = "Successful: 0";
        container.AddChild(_successfulSynthesesLabel);
        
        _legendarySynthesesLabel = new Label();
        _legendarySynthesesLabel.Text = "Legendary Results: 0";
        _legendarySynthesesLabel.AddColorOverride("font_color", new Color(1f, 0.84f, 0f));
        container.AddChild(_legendarySynthesesLabel);
        
        _successRateStatLabel = new Label();
        _successRateStatLabel.Text = "Success Rate: 0%";
        container.AddChild(_successRateStatLabel);
        
        _totalGoldSpentLabel = new Label();
        _totalGoldSpentLabel.Text = "Total Gold Spent: 0";
        container.AddChild(_totalGoldSpentLabel);
        
        // Update statistics
        UpdateStatistics();
    }
    
    public void Toggle()
    {
        _isVisible = !_isVisible;
        _mainPanel.Visible = _isVisible;
        
        if (_isVisible)
        {
            RefreshPetSelectors();
            UpdateStatistics();
            RefreshHistory();
        }
    }
    
    private void RefreshPetSelectors()
    {
        _pet1Selector.Clear();
        _pet2Selector.Clear();
        
        // Get all pets from PetManager
        // Note: This assumes PetManager has a method to get all pets
        // For now, add placeholder options
        
        _pet1Selector.AddItem("Select Pet 1...", 0);
        _pet2Selector.AddItem("Select Pet 2...", 0);
        
        // Update recipe info
        UpdateRecipeInfo();
    }
    
    private void UpdateRecipeInfo()
    {
        // This would check if selected pets have a recipe
        _recipeLabel.Text = "Recipe: Check available combinations";
        _costLabel.Text = "Cost: 500 Gold (base)";
        _successRateLabel.Text = "Success Rate: 50% (base)";
    }
    
    private void UpdateStatistics()
    {
        var system = PetSynthesisSystem.Instance;
        
        _totalSynthesesLabel.Text = $"Total Syntheses: {system.GetTotalSyntheses()}";
        _successfulSynthesesLabel.Text = $"Successful: {system.GetSuccessfulSyntheses()}";
        _legendarySynthesesLabel.Text = $"Legendary Results: {system.GetLegendarySyntheses()}";
        
        float rate = system.GetSuccessRate() * 100f;
        _successRateStatLabel.Text = $"Success Rate: {rate:F1}%";
        
        var data = system.GetData();
        _totalGoldSpentLabel.Text = $"Total Gold Spent: {data.TotalGoldSpent:N0}";
    }
    
    private void RefreshHistory()
    {
        _historyList.Clear();
        
        var history = PetSynthesisSystem.Instance.GetAllSynthesisHistory();
        
        foreach (var record in history)
        {
            string text = $"{record.ResultPetType} ({record.ResultRarity})";
            if (!record.WasSuccessful)
                text = "❌ Failed Synthesis";
            
            text += $" - {record.GoldCost} Gold";
            
            _historyList.AddItem(text);
        }
        
        if (history.Count == 0)
        {
            _historyList.AddItem("No synthesis history yet");
        }
    }
    
    #region Signal Handlers
    
    private void _on_close_pressed()
    {
        Toggle();
    }
    
    private void _on_pet1_selected(int index)
    {
        UpdateRecipeInfo();
    }
    
    private void _on_pet2_selected(int index)
    {
        UpdateRecipeInfo();
    }
    
    private void _on_synthesize_pressed()
    {
        int pet1Index = _pet1Selector.Selected;
        int pet2Index = _pet2Selector.Selected;
        
        if (pet1Index <= 0 || pet2Index <= 0)
        {
            _resultLabel.Text = "⚠️ Please select both pets!";
            _resultLabel.AddColorOverride("font_color", new Color(1f, 0.5f, 0.5f));
            return;
        }
        
        // Note: In actual implementation, we'd map these indices to pet IDs
        _resultLabel.Text = "🔮 Synthesizing...";
        _resultLabel.AddColorOverride("font_color", new Color(0.5f, 0.8f, 1f));
        
        // Perform synthesis (placeholder)
        // PetSynthesisSystem.Instance.StartSynthesis(pet1Id, pet2Id);
    }
    
    private void _on_synthesis_started(int pet1Id, int pet2Id)
    {
        _resultLabel.Text = "🔮 Synthesizing...";
        _resultLabel.AddColorOverride("font_color", new Color(0.5f, 0.8f, 1f));
    }
    
    private void _on_synthesis_completed(int resultPetId, string rarity)
    {
        string message = $"✨ Success! Created {rarity} pet!";
        
        switch (rarity)
        {
            case "Legendary":
                _resultLabel.AddColorOverride("font_color", new Color(1f, 0.84f, 0f));
                break;
            case "Epic":
                _resultLabel.AddColorOverride("font_color", new Color(0.6f, 0.2f, 1f));
                break;
            case "Rare":
                _resultLabel.AddColorOverride("font_color", new Color(0.2f, 0.6f, 1f));
                break;
            default:
                _resultLabel.AddColorOverride("font_color", new Color(0.5f, 1f, 0.5f));
                break;
        }
        
        _resultLabel.Text = message;
        
        UpdateStatistics();
        RefreshHistory();
    }
    
    private void _on_synthesis_failed()
    {
        _resultLabel.Text = "💥 Synthesis Failed! Pets were lost.";
        _resultLabel.AddColorOverride("font_color", new Color(1f, 0.3f, 0.3f));
        
        UpdateStatistics();
    }
    
    private void _on_refresh_history_pressed()
    {
        RefreshHistory();
    }
    
    #endregion
    
    public override void _Input(InputEvent eventItem)
    {
        if (eventItem.IsActionPressed("ui_cancel") && _isVisible)
        {
            Toggle();
        }
    }
}
