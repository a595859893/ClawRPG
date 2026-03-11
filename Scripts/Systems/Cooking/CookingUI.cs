using Godot;
using System;
using System.Collections.Generic;

public partial class CookingUI : Control
{
    private Control _mainPanel;
    private VBoxContainer _recipeList;
    private Label _cookingLevelLabel;
    private Label _expLabel;
    private ProgressBar _expProgressBar;
    private Label _statusLabel;
    private ProgressBar _cookingProgressBar;
    private Button _cancelButton;
    private Label _statisticsLabel;

    private bool _isVisible = false;
    private string _selectedRecipeId = "";

    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        Hide();
    }

    private void SetupUI()
    {
        // Main Panel
        _mainPanel = new Control();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_mainPanel);

        // Background Panel
        var bgPanel = new Panel();
        bgPanel.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        bgPanel.Modulate = new Color(0, 0, 0, 0.8f);
        _mainPanel.AddChild(bgPanel);

        // Title
        var titleLabel = new Label();
        titleLabel.Text = "🍳 Cooking System";
        titleLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.VerticalAlignment = VerticalAlignment.Center;
        titleLabel.Position = new Vector2(0, 20);
        titleLabel.AddThemeFontSizeOverride("font_size", 28);
        _mainPanel.AddChild(titleLabel);

        // Cooking Level Section
        var levelPanel = new HBoxContainer();
        levelPanel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        levelPanel.Position = new Vector2(20, 70);
        levelPanel.CustomMinimumSize = new Vector2(760, 40);
        _mainPanel.AddChild(levelPanel);

        _cookingLevelLabel = new Label();
        _cookingLevelLabel.Text = "Cooking Level: 1";
        _cookingLevelLabel.AddThemeFontSizeOverride("font_size", 18);
        levelPanel.AddChild(_cookingLevelLabel);

        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        levelPanel.AddChild(spacer);

        _expLabel = new Label();
        _expLabel.Text = "EXP: 0 / 100";
        _expLabel.AddThemeFontSizeOverride("font_size", 16);
        levelPanel.AddChild(_expLabel);

        // EXP Progress Bar
        _expProgressBar = new ProgressBar();
        _expProgressBar.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        _expProgressBar.Position = new Vector2(20, 115);
        _expProgressBar.CustomMinimumSize = new Vector2(760, 20);
        _expProgressBar.MaxValue = 100;
        _expProgressBar.Value = 0;
        _mainPanel.AddChild(_expProgressBar);

        // Recipe List (ScrollContainer)
        var scrollContainer = new ScrollContainer();
        scrollContainer.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        scrollContainer.Position = new Vector2(20, 145);
        scrollContainer.CustomMinimumSize = new Vector2(380, 350);
        _mainPanel.AddChild(scrollContainer);

        _recipeList = new VBoxContainer();
        _recipeList.CustomMinimumSize = new Vector2(360, 0);
        scrollContainer.AddChild(_recipeList);

        // Status Section
        var statusPanel = new VBoxContainer();
        statusPanel.SetAnchorsPreset(Control.LayoutPreset.RightWide);
        statusPanel.Position = new Vector2(420, 145);
        statusPanel.CustomMinimumSize = new Vector2(360, 200);
        _mainPanel.AddChild(statusPanel);

        var statusTitle = new Label();
        statusTitle.Text = "📋 Recipe Details";
        statusTitle.AddThemeFontSizeOverride("font_size", 18);
        statusPanel.AddChild(statusTitle);

        _statusLabel = new Label();
        _statusLabel.Text = "Select a recipe";
        _statusLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        statusPanel.AddChild(_statusLabel);

        // Cooking Progress Section
        var progressPanel = new VBoxContainer();
        progressPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        progressPanel.Position = new Vector2(20, 360);
        progressPanel.CustomMinimumSize = new Vector2(760, 60);
        _mainPanel.AddChild(progressPanel);

        _cookingProgressBar = new ProgressBar();
        _cookingProgressBar.CustomMinimumSize = new Vector2(760, 30);
        _cookingProgressBar.MaxValue = 100;
        _cookingProgressBar.Value = 0;
        progressPanel.AddChild(_cookingProgressBar);

        var progressLabel = new Label();
        progressLabel.Text = "Cooking Progress: Idle";
        progressLabel.Name = "ProgressLabel";
        progressPanel.AddChild(progressLabel);

        // Buttons
        var buttonPanel = new HBoxContainer();
        buttonPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        buttonPanel.Position = new Vector2(20, 430);
        buttonPanel.CustomMinimumSize = new Vector2(760, 40);
        _mainPanel.AddChild(buttonPanel);

        var cookButton = new Button();
        cookButton.Text = "🍳 Start Cooking";
        cookButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        cookButton.Pressed += OnCookButtonPressed;
        buttonPanel.AddChild(cookButton);

        _cancelButton = new Button();
        _cancelButton.Text = "❌ Cancel";
        _cancelButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _cancelButton.Pressed += OnCancelButtonPressed;
        _cancelButton.Disabled = true;
        buttonPanel.AddChild(_cancelButton);

        // Statistics Section
        var statsPanel = new VBoxContainer();
        statsPanel.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        statsPanel.Position = new Vector2(20, 480);
        statsPanel.CustomMinimumSize = new Vector2(760, 80);
        _mainPanel.AddChild(statsPanel);

        var statsTitle = new Label();
        statsTitle.Text = "📊 Statistics";
        statsTitle.AddThemeFontSizeOverride("font_size", 16);
        statsPanel.AddChild(statsTitle);

        _statisticsLabel = new Label();
        _statisticsLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        _statisticsLabel.Text = "Total: 0 | Success: 0 | Failed: 0 | Rate: 0%";
        statsPanel.AddChild(_statisticsLabel);

        // Close Button
        var closeButton = new Button();
        closeButton.Text = "❌ Close (ESC)";
        closeButton.SetAnchorsPreset(Control.LayoutPreset.BottomWide);
        closeButton.Position = new Vector2(20, 560);
        closeButton.CustomMinimumSize = new Vector2(760, 35);
        closeButton.Pressed += OnCloseButtonPressed;
        _mainPanel.AddChild(closeButton);
    }

    private void ConnectSignals()
    {
        if (CookingSystem.Instance != null)
        {
            CookingSystem.Instance.Connect(CookingSystem.SignalName.CookingProgress, 
                Callable.From<float>(OnCookingProgress));
            CookingSystem.Instance.Connect(CookingSystem.SignalName.CookingCompleted, 
                Callable.From<string, bool>(OnCookingCompleted));
            CookingSystem.Instance.Connect(CookingSystem.SignalName.LevelUp, 
                Callable.From<int, int>(OnLevelUp));
            CookingSystem.Instance.Connect(CookingSystem.SignalName.RecipeLearned, 
                Callable.From<string, string>(OnRecipeLearned));
        }
    }

    public void Toggle()
    {
        if (_isVisible)
            Hide();
        else
            Show();
    }

    public override void _Notification(int what)
    {
        if (what == NotificationVisibilityChanged)
        {
            _isVisible = Visible;
            if (_isVisible)
            {
                RefreshUI();
            }
        }
    }

    private void RefreshUI()
    {
        if (CookingSystem.Instance == null) return;

        // Update level and exp
        _cookingLevelLabel.Text = $"🍳 Cooking Level: {CookingSystem.Instance.GetCookingLevel()}";
        
        int currentExp = CookingSystem.Instance.GetCookingExp();
        int nextExp = CookingSystem.Instance.GetExpForNextLevel();
        _expLabel.Text = $"EXP: {currentExp} / {nextExp}";
        
        float expPercent = nextExp > 0 ? (float)currentExp / nextExp * 100 : 0;
        _expProgressBar.Value = expPercent;

        // Update recipe list
        UpdateRecipeList();

        // Update statistics
        var stats = CookingSystem.Instance.GetStatistics();
        _statisticsLabel.Text = $"Total: {stats["total_cooked"]} | Success: {stats["successful"]} | Failed: {stats["failed"]} | Rate: {stats["success_rate"]}%";

        // Update cooking status
        if (CookingSystem.Instance.IsCooking())
        {
            var cooking = CookingSystem.Instance.GetCurrentCooking();
            if (cooking != null)
            {
                var recipe = CookingDatabase.Instance.GetRecipe(cooking.recipeId);
                if (recipe != null)
                {
                    _cancelButton.Disabled = false;
                }
            }
        }
        else
        {
            _cancelButton.Disabled = true;
            _cookingProgressBar.Value = 0;
            var progressLabel = _mainPanel.FindChild("ProgressLabel", true, false) as Label;
            if (progressLabel != null)
                progressLabel.Text = "Cooking Progress: Idle";
        }
    }

    private void UpdateRecipeList()
    {
        // Clear existing items
        foreach (var child in _recipeList.GetChildren())
        {
            child.QueueFree();
        }

        var knownRecipes = CookingSystem.Instance.GetKnownRecipes();
        var cookingLevel = CookingSystem.Instance.GetCookingLevel();

        foreach (var recipeId in knownRecipes.Keys)
        {
            var recipe = CookingDatabase.Instance.GetRecipe(recipeId);
            if (recipe == null) continue;

            var recipeButton = CreateRecipeButton(recipe, cookingLevel);
            _recipeList.AddChild(recipeButton);
        }
    }

    private Button CreateRecipeButton(CookingRecipe recipe, int cookingLevel)
    {
        var button = new Button();
        button.CustomMinimumSize = new Vector2(360, 60);

        // Check if available
        bool canCook = CookingSystem.Instance.CanCook(recipe.recipeId);
        bool levelOk = recipe.requiredCookingLevel <= cookingLevel;

        // Color based on rarity
        Color rarityColor = Colors.White;
        switch (recipe.rarity)
        {
            case Rarity.Common: rarityColor = Colors.Gray; break;
            case Rarity.Uncommon: rarityColor = Colors.Green; break;
            case Rarity.Rare: rarityColor = Colors.Blue; break;
            case Rarity.Epic: rarityColor = new Color(0.6f, 0.3f, 0.8f); break;
            case Rarity.Legendary: rarityColor = new Color(1f, 0.6f, 0f); break;
        }

        string statusIcon = canCook ? "✅" : "❌";
        string levelText = recipe.requiredCookingLevel > 0 ? $" [Lv.{recipe.requiredCookingLevel}]" : "";
        
        button.Text = $"{statusIcon} {recipe.recipeName} ({recipe.foodType}){levelText}";
        button.Modulate = canCook ? rarityColor : new Color(0.5f, 0.5f, 0.5f);
        
        if (canCook)
        {
            button.Pressed += () => OnRecipeSelected(recipe.recipeId);
        }

        return button;
    }

    private void OnRecipeSelected(string recipeId)
    {
        _selectedRecipeId = recipeId;
        var recipe = CookingDatabase.Instance.GetRecipe(recipeId);
        if (recipe == null) return;

        // Build recipe details text
        string details = $"📝 {recipe.recipeName}\n";
        details += $"⭐ Rarity: {recipe.rarity}\n";
        details += $"🍲 Type: {recipe.foodType}\n\n";
        
        details += "📦 Ingredients:\n";
        foreach (var ingredient in recipe.ingredients)
        {
            bool hasItem = InventoryManager.Instance.HasItem(ingredient.Key, ingredient.Value);
            string check = hasItem ? "✅" : "❌";
            details += $"  {check} {ingredient.Key}: {ingredient.Value}\n";
        }
        
        details += $"\n⏱️ Cooking Time: {recipe.cookingTime}s\n";
        details += $"🍖 Hunger: +{recipe.hungerRestored}\n";
        details += $"⚡ Energy: +{recipe.energyRestored}\n";
        
        if (recipe.statBonuses.Count > 0)
        {
            details += "\n✨ Stat Bonuses:\n";
            foreach (var bonus in recipe.statBonuses)
            {
                string duration = bonus.duration > 0 ? $" ({bonus.duration}min)" : " (Permanent)";
                details += $"  • {bonus.statName}: +{bonus.bonusValue}{duration}\n";
            }
        }

        _statusLabel.Text = details;
    }

    private void OnCookButtonPressed()
    {
        if (string.IsNullOrEmpty(_selectedRecipeId)) return;

        if (CookingSystem.Instance.StartCooking(_selectedRecipeId))
        {
            var cooking = CookingSystem.Instance.GetCurrentCooking();
            if (cooking != null)
            {
                _cancelButton.Disabled = false;
                var progressLabel = _mainPanel.FindChild("ProgressLabel", true, false) as Label;
                if (progressLabel != null)
                    progressLabel.Text = "Cooking Progress: Cooking...";
            }
        }
    }

    private void OnCancelButtonPressed()
    {
        CookingSystem.Instance.CancelCooking();
        _cancelButton.Disabled = true;
        _cookingProgressBar.Value = 0;
        var progressLabel = _mainPanel.FindChild("ProgressLabel", true, false) as Label;
        if (progressLabel != null)
            progressLabel.Text = "Cooking Progress: Cancelled";
    }

    private void OnCloseButtonPressed()
    {
        Hide();
    }

    private void OnCookingProgress(float progress)
    {
        _cookingProgressBar.Value = progress * 100;
    }

    private void OnCookingCompleted(string recipeId, bool success)
    {
        var progressLabel = _mainPanel.FindChild("ProgressLabel", true, false) as Label;
        if (progressLabel != null)
        {
            progressLabel.Text = success ? 
                $"Cooking Progress: ✅ Success! {CookingDatabase.Instance.GetRecipe(recipeId).recipeName}" :
                "Cooking Progress: ❌ Failed!";
        }

        _cancelButton.Disabled = true;
        RefreshUI();
    }

    private void OnLevelUp(int newLevel, int exp)
    {
        RefreshUI();
    }

    private void OnRecipeLearned(string recipeId, string recipeName)
    {
        RefreshUI();
    }

    public override void _Input(InputEvent e)
    {
        if (e.IsActionPressed("ui_cancel") && _isVisible)
        {
            Hide();
        }
    }
}
