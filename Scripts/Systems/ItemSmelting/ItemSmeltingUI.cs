using Godot;
using System;
using System.Collections.Generic;

public class ItemSmeltingUI : Control
{
    private ItemSmeltingSystem _system;
    private ItemSmeltingDatabase _database;
    private ItemSmeltingData _data;
    
    // UI Elements
    private Label _titleLabel;
    private TabContainer _tabContainer;
    
    // Smelt tab
    private OptionButton _recipeOption;
    private SpinBox _itemCountSpin;
    private Button _smeltButton;
    private Button _previewButton;
    private Label _costLabel;
    private Label _successRateLabel;
    private ProgressBar _progressBar;
    private Label _statusLabel;
    private RichTextLabel _previewLabel;
    
    // History tab
    private ItemList _historyList;
    
    // Stats tab
    private Label _totalSmeltsLabel;
    private Label _totalItemsLabel;
    private Label _totalMaterialsLabel;
    private Label _goldSpentLabel;
    private Label _avgMaterialsLabel;
    private Button _resetStatsButton;
    
    private int _playerLevel = 1;
    
    public override void _Ready()
    {
        _system = GetNode<ItemSmeltingSystem>("/root/ItemSmeltingSystem");
        _database = GetNode<ItemSmeltingDatabase>("/root/ItemSmeltingDatabase");
        _data = GetNode<ItemSmeltingData>("/root/ItemSmeltingData");
        
        SetupUI();
    }
    
    void SetupUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(AnchorPresets.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚒️ Item Smelting System";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);
        
        // Create tabs
        CreateSmeltTab();
        CreateHistoryTab();
        CreateStatsTab();
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += () => Hide();
        mainContainer.AddChild(closeButton);
    }
    
    void CreateSmeltTab()
    {
        var scroll = new ScrollContainer();
        scroll.Name = "Smelt";
        _tabContainer.AddChild(scroll);
        
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 15);
        container.SetAnchor(AnchorPresets.FullRect);
        container.AddThemeConstantOverride("margin_left", 20);
        container.AddThemeConstantOverride("margin_top", 20);
        container.AddThemeConstantOverride("margin_right", 20);
        container.AddThemeConstantOverride("margin_bottom", 20);
        scroll.AddChild(container);
        
        // Recipe selection
        var recipeLabel = new Label();
        recipeLabel.Text = "Select Recipe:";
        container.AddChild(recipeLabel);
        
        _recipeOption = new OptionButton();
        _recipeOption.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        PopulateRecipes();
        _recipeOption.ItemSelected += (index) => OnRecipeSelected(index);
        container.AddChild(_recipeOption);
        
        // Item count
        var countLabel = new Label();
        countLabel.Text = "Items to Smelt:";
        container.AddChild(countLabel);
        
        _itemCountSpin = new SpinBox();
        _itemCountSpin.MinValue = 1;
        _itemCountSpin.MaxValue = 100;
        _itemCountSpin.Value = 1;
        _itemCountSpin.ValueChanged += (value) => UpdatePreview();
        container.AddChild(_itemCountSpin);
        
        // Cost info
        _costLabel = new Label();
        _costLabel.Text = "Cost: 0 Gold";
        container.AddChild(_costLabel);
        
        _successRateLabel = new Label();
        _successRateLabel.Text = "Success Rate: 0%";
        container.AddChild(_successRateLabel);
        
        // Preview button
        _previewButton = new Button();
        _previewButton.Text = "Preview Materials";
        _previewButton.Pressed += () => UpdatePreview();
        container.AddChild(_previewButton);
        
        // Preview result
        _previewLabel = new RichTextLabel();
        _previewLabel.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _previewLabel.BbcodeEnabled = true;
        _previewLabel.Text = "[color=gray]Click 'Preview Materials' to see what you'll get...[/color]";
        container.AddChild(_previewLabel);
        
        // Smelt button
        _smeltButton = new Button();
        _smeltButton.Text = "🔥 Start Smelting";
        _smeltButton.Pressed += () => StartSmelting();
        _smeltButton.Disabled = true;
        container.AddChild(_smeltButton);
        
        // Progress bar
        _progressBar = new ProgressBar();
        _progressBar.MinValue = 0;
        _progressBar.MaxValue = 1;
        _progressBar.Value = 0;
        _progressBar.Visible = false;
        container.AddChild(_progressBar);
        
        // Status label
        _statusLabel = new Label();
        _statusLabel.Text = "";
        _statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(_statusLabel);
    }
    
    void CreateHistoryTab()
    {
        var container = new VBoxContainer();
        container.Name = "History";
        container.AddThemeConstantOverride("separation", 10);
        container.SetAnchor(AnchorPresets.FullRect);
        container.AddThemeConstantOverride("margin_left", 20);
        container.AddThemeConstantOverride("margin_top", 20);
        container.AddThemeConstantOverride("margin_right", 20);
        container.AddThemeConstantOverride("margin_bottom", 20);
        _tabContainer.AddChild(container);
        
        var historyTitle = new Label();
        historyTitle.Text = "Smelting History";
        historyTitle.AddThemeFontSizeOverride("font_size", 18);
        container.AddChild(historyTitle);
        
        _historyList = new ItemList();
        _historyList.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        container.AddChild(_historyList);
        
        var refreshButton = new Button();
        refreshButton.Text = "Refresh History";
        refreshButton.Pressed += () => RefreshHistory();
        container.AddChild(refreshButton);
    }
    
    void CreateStatsTab()
    {
        var container = new VBoxContainer();
        container.Name = "Statistics";
        container.AddThemeConstantOverride("separation", 15);
        container.SetAnchor(AnchorPresets.FullRect);
        container.AddThemeConstantOverride("margin_left", 20);
        container.AddThemeConstantOverride("margin_top", 20);
        container.AddThemeConstantOverride("margin_right", 20);
        container.AddThemeConstantOverride("margin_bottom", 20);
        _tabContainer.AddChild(container);
        
        var statsTitle = new Label();
        statsTitle.Text = "Smelting Statistics";
        statsTitle.AddThemeFontSizeOverride("font_size", 18);
        container.AddChild(statsTitle);
        
        _totalSmeltsLabel = new Label();
        _totalSmeltsLabel.Text = "Total Smelts: 0";
        container.AddChild(_totalSmeltsLabel);
        
        _totalItemsLabel = new Label();
        _totalItemsLabel.Text = "Items Smelted: 0";
        container.AddChild(_totalItemsLabel);
        
        _totalMaterialsLabel = new Label();
        _totalMaterialsLabel.Text = "Materials Generated: 0";
        container.AddChild(_totalMaterialsLabel);
        
        _goldSpentLabel = new Label();
        _goldSpentLabel.Text = "Gold Spent: 0";
        container.AddChild(_goldSpentLabel);
        
        _avgMaterialsLabel = new Label();
        _avgMaterialsLabel.Text = "Avg Materials/Smelt: 0";
        container.AddChild(_avgMaterialsLabel);
        
        _resetStatsButton = new Button();
        _resetStatsButton.Text = "Reset Statistics";
        _resetStatsButton.Pressed += () => ResetStatistics();
        container.AddChild(_resetStatsButton);
        
        RefreshStats();
    }
    
    void PopulateRecipes()
    {
        _recipeOption.Clear();
        
        var recipes = _database.GetAvailableRecipes(_playerLevel);
        int index = 0;
        foreach (var recipe in recipes)
        {
            _recipeOption.AddItem($"{recipe.Name} (Lv.{recipe.RequiredLevel})", index);
            index++;
        }
        
        if (_recipeOption.ItemCount > 0)
        {
            _recipeOption.Selected = 0;
            OnRecipeSelected(0);
        }
    }
    
    void OnRecipeSelected(int index)
    {
        UpdatePreview();
    }
    
    void UpdatePreview()
    {
        if (_recipeOption.ItemCount == 0) return;
        
        var recipes = _database.GetAvailableRecipes(_playerLevel);
        if (_recipeOption.Selected >= recipes.Count) return;
        
        var recipe = recipes[_recipeOption.Selected];
        int itemCount = (int)_itemCountSpin.Value;
        
        // Update cost
        _costLabel.Text = $"Cost: {recipe.GoldCost * itemCount} Gold";
        
        // Update success rate
        _successRateLabel.Text = $"Success Rate: {recipe.SuccessRate * 100:F1}%";
        
        // Preview materials
        var preview = _system.PreviewSmelting(recipe.Id, itemCount);
        
        string previewText = "[b]Preview:[/b]\n";
        foreach (var material in preview)
        {
            var matInfo = _database.Materials.ContainsKey(material.Key) 
                ? _database.Materials[material.Key] 
                : null;
            string matName = matInfo != null ? matInfo.Name : material.Key;
            previewText += $"• {matName}: [color=yellow]{material.Value}[/color]\n";
        }
        
        _previewLabel.Text = previewText;
        
        _smeltButton.Disabled = false;
    }
    
    void StartSmelting()
    {
        if (_recipeOption.ItemCount == 0) return;
        
        var recipes = _database.GetAvailableRecipes(_playerLevel);
        if (_recipeOption.Selected >= recipes.Count) return;
        
        var recipe = recipes[_recipeOption.Selected];
        int itemCount = (int)_itemCountSpin.Value;
        
        _system.StartSmelting(recipe.Id, itemCount);
        
        _progressBar.Visible = true;
        _statusLabel.Text = "Smelting in progress...";
        _smeltButton.Disabled = true;
    }
    
    public override void _Process(float delta)
    {
        if (_system == null) return;
        
        if (_system.IsSmelting)
        {
            _progressBar.Value = _system.SmeltProgress;
        }
        else if (_progressBar.Visible)
        {
            _progressBar.Visible = false;
            _statusLabel.Text = "Smelting complete!";
            _smeltButton.Disabled = false;
            
            // Refresh history and stats
            RefreshHistory();
            RefreshStats();
        }
    }
    
    void RefreshHistory()
    {
        _historyList.Clear();
        
        var history = _system.GetHistory(20);
        foreach (var record in history)
        {
            var recipe = _database.GetRecipe(record.RecipeId);
            string recipeName = recipe != null ? recipe.Name : record.RecipeId;
            
            string status = record.MaterialsGenerated > 0 ? "✓ Success" : "✗ Failed";
            string timestamp = DateTimeOffset.FromUnixTimeSeconds(record.Timestamp).ToString("MM/dd HH:mm");
            
            _historyList.AddItem($"[{timestamp}] {recipeName} x{record.ItemCount} - {status}");
        }
        
        if (history.Count == 0)
        {
            _historyList.AddItem("No smelting history yet.");
        }
    }
    
    void RefreshStats()
    {
        var stats = _system.GetStatistics();
        
        _totalSmeltsLabel.Text = $"Total Smelts: {stats.TotalSmelts}";
        _totalItemsLabel.Text = $"Items Smelted: {stats.TotalItemsSmelted}";
        _totalMaterialsLabel.Text = $"Materials Generated: {stats.TotalMaterialsGenerated}";
        _goldSpentLabel.Text = $"Gold Spent: {stats.GoldSpent}";
        _avgMaterialsLabel.Text = $"Avg Materials/Smelt: {stats.AverageMaterialsPerSmelt:F1}";
    }
    
    void ResetStatistics()
    {
        if (_data == null) return;
        
        _data.TotalSmelts = 0;
        _data.TotalItemsSmelted = 0;
        _data.TotalMaterialsGenerated = 0;
        _data.GoldSpent = 0;
        _data.RecipeUsageCount.Clear();
        _data.History.Clear();
        _data.SaveData();
        
        RefreshHistory();
        RefreshStats();
        
        GD.Print("Statistics reset!");
    }
    
    public void SetPlayerLevel(int level)
    {
        _playerLevel = level;
        PopulateRecipes();
    }
}

// Required using statements
using Godot;
