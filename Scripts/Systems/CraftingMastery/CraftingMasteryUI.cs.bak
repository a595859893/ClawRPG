using Godot;
using System;
using System.Collections.Generic;

public partial class CraftingMasteryUI : Control
{
    private VBoxContainer mainContainer;
    private TabContainer tabContainer;
    private Label titleLabel;
    private Label totalMasteryLabel;
    private Label overallTierLabel;

    // Crafting type panels
    private Dictionary<CraftingMasterySystem.CraftingType, VBoxContainer> typePanels = new Dictionary<CraftingMasterySystem.CraftingType, VBoxContainer>();

    public override void _Ready()
    {
        SetupUI();
        LoadMasteryData();
    }

    private void SetupUI()
    {
        // Main container
        mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainContainer);

        // Title
        titleLabel = new Label();
        titleLabel.Text = "⚒️ Crafting Mastery";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 28);
        mainContainer.AddChild(titleLabel);

        // Overall mastery display
        HBoxContainer overallContainer = new HBoxContainer();
        mainContainer.AddChild(overallContainer);

        totalMasteryLabel = new Label();
        totalMasteryLabel.Text = "Total Mastery Level: 0";
        totalMasteryLabel.AddThemeFontSizeOverride("font_size", 20);
        overallContainer.AddChild(totalMasteryLabel);

        overallTierLabel = new Label();
        overallTierLabel.Text = "Tier: Novice";
        overallTierLabel.AddThemeFontSizeOverride("font_size", 20);
        overallContainer.AddChild(overallTierLabel);

        // Tab container for different crafting types
        tabContainer = new TabContainer();
        tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(tabContainer);

        // Create tabs for each crafting type
        foreach (CraftingMasterySystem.CraftingType type in Enum.GetValues(typeof(CraftingMasterySystem.CraftingType)))
        {
            CreateCraftingTypeTab(type);
        }

        // Close button
        Button closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.Pressed += OnClosePressed;
        mainContainer.AddChild(closeButton);
    }

    private void CreateCraftingTypeTab(CraftingMasterySystem.CraftingType type)
    {
        ScrollContainer scroll = new ScrollContainer();
        scroll.Name = type.ToString();
        tabContainer.AddChild(scroll);

        VBoxContainer container = new VBoxContainer();
        container.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        container.AddThemeConstantOverride("separation", 10);
        scroll.AddChild(container);

        // Type title
        Label typeLabel = new Label();
        typeLabel.Text = GetTypeDisplayName(type);
        typeLabel.AddThemeFontSizeOverride("font_size", 24);
        container.AddChild(typeLabel);

        // Mastery info
        VBoxContainer infoContainer = new VBoxContainer();
        container.AddChild(infoContainer);

        // Level
        Label levelLabel = new Label();
        levelLabel.Name = "LevelLabel";
        levelLabel.Text = "Level: 0";
        infoContainer.AddChild(levelLabel);

        // Tier
        Label tierLabel = new Label();
        tierLabel.Name = "TierLabel";
        tierLabel.Text = "Tier: Novice";
        infoContainer.AddChild(tierLabel);

        // Progress bar
        ProgressBar progressBar = new ProgressBar();
        progressBar.Name = "ProgressBar";
        progressBar.MinValue = 0;
        progressBar.MaxValue = 100;
        progressBar.Value = 0;
        container.AddChild(progressBar);

        // Experience
        Label expLabel = new Label();
        expLabel.Name = "ExpLabel";
        expLabel.Text = "Experience: 0 / 100";
        container.AddChild(expLabel);

        // Statistics
        Label statsLabel = new Label();
        statsLabel.Name = "StatsLabel";
        statsLabel.Text = "Total Crafts: 0\nSuccessful: 0 (0%)\nHighest Rarity: Common";
        container.AddChild(statsLabel);

        // Bonuses
        Label bonusesLabel = new Label();
        bonusesLabel.Name = "BonusesLabel";
        bonusesLabel.Text = "\n📈 Bonuses:\nSuccess Rate: +0%\nQuality: +0%\nSpeed: +0%\nCost: -0%\nExperience: +0%";
        container.AddChild(bonusesLabel);

        // Store reference
        typePanels[type] = container;
    }

    private string GetTypeDisplayName(CraftingMasterySystem.CraftingType type)
    {
        switch (type)
        {
            case CraftingMasterySystem.CraftingType.Alchemy: return "⚗️ Alchemy";
            case CraftingMasterySystem.CraftingType.Cooking: return "🍳 Cooking";
            case CraftingMasterySystem.CraftingType.Fishing: return "🎣 Fishing";
            case CraftingMasterySystem.CraftingType.Enchantment: return "✨ Enchantment";
            case CraftingMasterySystem.CraftingType.Smithing: return "🔨 Smithing";
            case CraftingMasterySystem.CraftingType.Tailoring: return "🧵 Tailoring";
            case CraftingMasterySystem.CraftingType.Jeweler: return "💎 Jeweler";
            case CraftingMasterySystem.CraftingType.Herbalism: return "🌿 Herbalism";
            case CraftingMasterySystem.CraftingType.Mining: return "⛏️ Mining";
            case CraftingMasterySystem.CraftingType.Woodcutting: return "🪓 Woodcutting";
            default: return type.ToString();
        }
    }

    private void LoadMasteryData()
    {
        if (CraftingMasterySystem.Instance == null) return;

        // Update overall mastery
        int totalLevel = CraftingMasterySystem.Instance.GetTotalMasteryLevel();
        totalMasteryLabel.Text = $"Total Mastery Level: {totalLevel}";

        CraftingMasterySystem.MasteryTier overallTier = CraftingMasterySystem.Instance.GetOverallMasteryTier();
        overallTierLabel.Text = $"Tier: {overallTier}";

        // Update each type panel
        foreach (var kvp in typePanels)
        {
            CraftingMasterySystem.CraftingType type = kvp.Key;
            VBoxContainer container = kvp.Value;

            CraftingMasterySystem.MasteryData data = CraftingMasterySystem.Instance.GetMasteryData(type);

            // Update level
            Label levelLabel = container.FindChild("LevelLabel", true, false) as Label;
            if (levelLabel != null)
            {
                levelLabel.Text = $"Level: {data.Level} / 100";
            }

            // Update tier
            Label tierLabel = container.FindChild("TierLabel", true, false) as Label;
            if (tierLabel != null)
            {
                tierLabel.Text = $"Tier: {data.GetTier()}";
            }

            // Update progress bar
            ProgressBar progressBar = container.FindChild("ProgressBar", true, false) as ProgressBar;
            if (progressBar != null)
            {
                int expForNext = data.GetExperienceForNextLevel();
                progressBar.MaxValue = expForNext;
                progressBar.Value = data.TotalExperience;
            }

            // Update experience
            Label expLabel = container.FindChild("ExpLabel", true, false) as Label;
            if (expLabel != null)
            {
                int expForNext = data.GetExperienceForNextLevel();
                expLabel.Text = $"Experience: {data.TotalExperience} / {expForNext}";
            }

            // Update statistics
            Label statsLabel = container.FindChild("StatsLabel", true, false) as Label;
            if (statsLabel != null)
            {
                string rarityName = GetRarityName(data.HighestRarityCrafted);
                statsLabel.Text = $"Total Crafts: {data.TotalCrafts}\nSuccessful: {data.SuccessfulCrafts} ({data.GetSuccessRate():F1}%)\nHighest Rarity: {rarityName}";
            }

            // Update bonuses
            Label bonusesLabel = container.FindChild("BonusesLabel", true, false) as Label;
            if (bonusesLabel != null)
            {
                float successBonus = CraftingMasterySystem.Instance.GetMasteryBonus(type, "success_rate");
                float qualityBonus = CraftingMasterySystem.Instance.GetMasteryBonus(type, "quality");
                float speedBonus = CraftingMasterySystem.Instance.GetMasteryBonus(type, "speed");
                float costBonus = CraftingMasterySystem.Instance.GetMasteryBonus(type, "cost");
                float expBonus = CraftingMasterySystem.Instance.GetMasteryBonus(type, "experience");

                bonusesLabel.Text = $"\n📈 Bonuses:\nSuccess Rate: +{successBonus:F1}%\nQuality: +{qualityBonus:F1}%\nSpeed: +{speedBonus:F1}%\nCost: -{costBonus:F1}%\nExperience: +{expBonus:F1}%";
            }
        }
    }

    private string GetRarityName(int rarity)
    {
        switch (rarity)
        {
            case 0: return "Common";
            case 1: return "Uncommon";
            case 2: return "Rare";
            case 3: return "Epic";
            case 4: return "Legendary";
            case 5: return "Mythical";
            default: return "Unknown";
        }
    }

    private void OnClosePressed()
    {
        Visible = false;
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Visible = false;
        }
    }

    public void Refresh()
    {
        LoadMasteryData();
    }
}
