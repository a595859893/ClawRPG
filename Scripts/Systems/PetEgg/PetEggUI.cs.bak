using Godot;
using System;
using System.Collections.Generic;

public partial class PetEggUI : Control
{
    private VBoxContainer mainContainer;
    private HBoxContainer eggListContainer;
    private VBoxContainer eggInfoContainer;
    private Label titleLabel;
    private Label statsLabel;
    
    // Egg display
    private GridContainer eggGrid;
    private Button selectedEggButton;
    private string selectedEggId = "";
    
    // Info panel
    private Label eggNameLabel;
    private Label eggDescriptionLabel;
    private Label eggRarityLabel;
    private Label hatchTimeLabel;
    private Label goldCostLabel;
    private Label progressLabel;
    private ProgressBar hatchProgressBar;
    private Button hatchButton;
    private Button hatchNowButton;
    
    // Statistics
    private Label totalHatchedLabel;
    private Label totalGoldSpentLabel;
    
    private bool isVisible = false;
    
    public override void _Ready()
    {
        SetupUI();
        VisibilityChanged += OnVisibilityChanged;
    }
    
    private void SetupUI()
    {
        // Main container
        mainContainer = new VBoxContainer();
        mainContainer.SetAnchor(AnchorPresets.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = " 🐣 Pet Egg Hatching";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(titleLabel);
        
        // Stats row
        HBoxContainer statsRow = new HBoxContainer();
        statsRow.AddThemeConstantOverride("separation", 20);
        mainContainer.AddChild(statsRow);
        
        totalHatchedLabel = new Label();
        totalHatchedLabel.Text = "Total Hatched: 0";
        statsRow.AddChild(totalHatchedLabel);
        
        totalGoldSpentLabel = new Label();
        totalGoldSpentLabel.Text = "Gold Spent: 0";
        statsRow.AddChild(totalGoldSpentLabel);
        
        // Main content area
        HBoxContainer contentArea = new HBoxContainer();
        contentArea.AddThemeConstantOverride("separation", 20);
        mainContainer.AddChild(contentArea);
        
        // Left - Egg list
        VBoxContainer leftPanel = new VBoxContainer();
        leftPanel.CustomMinimumSize = new Vector2(300, 0);
        contentArea.AddChild(leftPanel);
        
        Label eggListTitle = new Label();
        eggListTitle.Text = "Your Eggs";
        eggListTitle.AddThemeFontSizeOverride("font_size", 18);
        leftPanel.AddChild(eggListTitle);
        
        ScrollContainer eggScroll = new ScrollContainer();
        eggScroll.CustomMinimumSize = new Vector2(280, 350);
        leftPanel.AddChild(eggScroll);
        
        eggGrid = new GridContainer();
        eggGrid.Columns = 2;
        eggGrid.AddThemeConstantOverride("h_separation", 10);
        eggGrid.AddThemeConstantOverride("v_separation", 10);
        eggScroll.AddChild(eggGrid);
        
        // Right - Egg info
        VBoxContainer rightPanel = new VBoxContainer();
        rightPanel.CustomMinimumSize = new Vector2(300, 0);
        contentArea.AddChild(rightPanel);
        
        Label infoTitle = new Label();
        infoTitle.Text = "Egg Details";
        infoTitle.AddThemeFontSizeOverride("font_size", 18);
        rightPanel.AddChild(infoTitle);
        
        eggInfoContainer = new VBoxContainer();
        eggInfoContainer.AddThemeConstantOverride("separation", 5);
        rightPanel.AddChild(eggInfoContainer);
        
        eggNameLabel = new Label();
        eggNameLabel.Text = "Select an egg";
        eggNameLabel.AddThemeFontSizeOverride("font_size", 16);
        eggInfoContainer.AddChild(eggNameLabel);
        
        eggDescriptionLabel = new Label();
        eggDescriptionLabel.Text = "";
        eggDescriptionLabel.AutowrapMode = TextServer.AwrapMode.Word;
        eggInfoContainer.AddChild(eggDescriptionLabel);
        
        eggRarityLabel = new Label();
        eggRarityLabel.Text = "";
        eggInfoContainer.AddChild(eggRarityLabel);
        
        hatchTimeLabel = new Label();
        hatchTimeLabel.Text = "";
        eggInfoContainer.AddChild(hatchTimeLabel);
        
        goldCostLabel = new Label();
        goldCostLabel.Text = "";
        eggInfoContainer.AddChild(goldCostLabel);
        
        // Progress section
        Label progressTitle = new Label();
        progressTitle.Text = "Hatching Progress:";
        progressTitle.AddThemeFontSizeOverride("font_size", 14);
        eggInfoContainer.AddChild(progressTitle);
        
        hatchProgressBar = new ProgressBar();
        hatchProgressBar.CustomMinimumSize = new Vector2(0, 20);
        hatchProgressBar.PercentVisible = true;
        eggInfoContainer.AddChild(hatchProgressBar);
        
        progressLabel = new Label();
        progressLabel.Text = "";
        progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
        eggInfoContainer.AddChild(progressLabel);
        
        // Buttons
        HBoxContainer buttonRow = new HBoxContainer();
        buttonRow.AddThemeConstantOverride("separation", 10);
        eggInfoContainer.AddChild(buttonRow);
        
        hatchButton = new Button();
        hatchButton.Text = "Start Hatching";
        hatchButton.CustomMinimumSize = new Vector2(130, 40);
        hatchButton.Pressed += OnHatchButtonPressed;
        buttonRow.AddChild(hatchButton);
        
        hatchNowButton = new Button();
        hatchNowButton.Text = "Hatch Now!";
        hatchNowButton.CustomMinimumSize = new Vector2(130, 40);
        hatchNowButton.Pressed += OnHatchNowButtonPressed;
        buttonRow.AddChild(hatchNowButton);
        
        // Close button
        Button closeButton = new Button();
        closeButton.Text = "Close (H)";
        closeButton.CustomMinimumSize = new Vector2(0, 40);
        closeButton.Pressed += OnCloseButtonPressed;
        mainContainer.AddChild(closeButton);
        
        // Initial state
        UpdateEggList();
        UpdateStats();
        UpdateEggInfo();
    }
    
    private void OnVisibilityChanged()
    {
        if (Visible)
        {
            UpdateEggList();
            UpdateStats();
            UpdateEggInfo();
        }
    }
    
    private void UpdateEggList()
    {
        // Clear existing
        foreach (Node child in eggGrid.GetChildren())
            child.QueueFree();
        
        if (PetEggSystem.Instance == null) return;
        
        var eggs = PetEggSystem.Instance.GetOwnedEggs();
        
        if (eggs.Count == 0)
        {
            Label noEggsLabel = new Label();
            noEggsLabel.Text = "No eggs in inventory";
            noEggsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            eggGrid.AddChild(noEggsLabel);
            return;
        }
        
        foreach (var kvp in eggs)
        {
            var egg = kvp.Value;
            var eggData = PetEggDatabase.GetEgg(egg.eggId);
            if (eggData == null) continue;
            
            Button eggButton = new Button();
            eggButton.CustomMinimumSize = new Vector2(120, 80);
            eggButton.Text = eggData.eggName + "\n";
            
            if (egg.isHatched)
            {
                eggButton.Text += "[Hatched]";
            }
            else if (egg.isHatching)
            {
                float progress = PetEggSystem.Instance.GetHatchProgress(kvp.Key);
                eggButton.Text += $"{(int)(progress * 100)}%";
            }
            else
            {
                eggButton.Text += PetEggData.RarityNames[eggData.rarity];
            }
            
            // Color based on rarity
            Color rarityColor = GetRarityColor(eggData.rarity);
            eggButton.Modulate = rarityColor;
            
            eggButton.Pressed += () => OnEggSelected(kvp.Key);
            
            eggGrid.AddChild(eggButton);
        }
    }
    
    private void UpdateStats()
    {
        if (PetEggSystem.Instance == null) return;
        
        totalHatchedLabel.Text = $"Total Hatched: {PetEggSystem.Instance.GetTotalEggsHatched()}";
        totalGoldSpentLabel.Text = $"Gold Spent: {PetEggSystem.Instance.GetTotalGoldSpent()}";
    }
    
    private void UpdateEggInfo()
    {
        if (selectedEggId == "" || PetEggSystem.Instance == null)
        {
            eggNameLabel.Text = "Select an egg";
            eggDescriptionLabel.Text = "";
            eggRarityLabel.Text = "";
            hatchTimeLabel.Text = "";
            goldCostLabel.Text = "";
            progressLabel.Text = "";
            hatchProgressBar.Value = 0;
            hatchButton.Disabled = true;
            hatchNowButton.Disabled = true;
            return;
        }
        
        var egg = PetEggSystem.Instance.GetEgg(selectedEggId);
        if (egg == null)
        {
            selectedEggId = "";
            UpdateEggInfo();
            return;
        }
        
        var eggData = PetEggDatabase.GetEgg(egg.eggId);
        if (eggData == null) return;
        
        eggNameLabel.Text = eggData.eggName;
        eggNameLabel.Modulate = GetRarityColor(eggData.rarity);
        
        eggDescriptionLabel.Text = eggData.description;
        
        eggRarityLabel.Text = $"Rarity: {PetEggData.RarityNames[eggData.rarity]}";
        eggRarityLabel.Modulate = GetRarityColor(eggData.rarity);
        
        int hatchTime = eggData.hatchTimeSeconds;
        string timeStr = "";
        if (hatchTime >= 3600)
            timeStr = $"{hatchTime / 3600}h {(hatchTime % 3600) / 60}m";
        else if (hatchTime >= 60)
            timeStr = $"{hatchTime / 60}m {hatchTime % 60}s";
        else
            timeStr = $"{hatchTime}s";
        
        hatchTimeLabel.Text = $"Hatch Time: {timeStr}";
        goldCostLabel.Text = $"Cost: {eggData.goldCost} Gold";
        
        if (egg.isHatched)
        {
            progressLabel.Text = "✅ Hatched!";
            hatchProgressBar.Value = 100;
            hatchButton.Disabled = true;
            hatchNowButton.Disabled = true;
        }
        else if (egg.isHatching)
        {
            float progress = PetEggSystem.Instance.GetHatchProgress(selectedEggId);
            int remaining = PetEggSystem.Instance.GetRemainingHatchTime(selectedEggId);
            
            hatchProgressBar.Value = progress * 100;
            
            if (remaining > 0)
            {
                string remainingStr = "";
                if (remaining >= 3600)
                    remainingStr = $"{remaining / 3600}h {(remaining % 3600) / 60}m";
                else if (remaining >= 60)
                    remainingStr = $"{remaining / 60}m {remaining % 60}s";
                else
                    remainingStr = $"{remaining}s";
                
                progressLabel.Text = $"Hatching... {remainingStr} remaining";
                hatchButton.Disabled = true;
                hatchNowButton.Disabled = true;
            }
            else
            {
                progressLabel.Text = "Ready to hatch!";
                hatchButton.Disabled = true;
                hatchNowButton.Disabled = false;
            }
        }
        else
        {
            progressLabel.Text = "Not started";
            hatchProgressBar.Value = 0;
            hatchButton.Disabled = Player.Instance.Gold < eggData.goldCost;
            hatchNowButton.Disabled = true;
        }
    }
    
    private void OnEggSelected(string uniqueId)
    {
        selectedEggId = uniqueId;
        UpdateEggInfo();
    }
    
    private void OnHatchButtonPressed()
    {
        if (selectedEggId == "" || PetEggSystem.Instance == null) return;
        
        var egg = PetEggSystem.Instance.GetEgg(selectedEggId);
        if (egg == null || egg.isHatching || egg.isHatched) return;
        
        var eggData = PetEggDatabase.GetEgg(egg.eggId);
        if (eggData == null) return;
        
        if (Player.Instance.Gold < eggData.goldCost)
        {
            ShowMessage("Not enough gold!");
            return;
        }
        
        if (PetEggSystem.Instance.StartHatching(selectedEggId))
        {
            UpdateEggList();
            UpdateEggInfo();
            ShowMessage("Hatching started!");
        }
    }
    
    private void OnHatchNowButtonPressed()
    {
        if (selectedEggId == "" || PetEggSystem.Instance == null) return;
        
        if (!PetEggSystem.Instance.IsEggReadyToHatch(selectedEggId))
        {
            ShowMessage("Egg is not ready yet!");
            return;
        }
        
        int? petId = PetEggSystem.Instance.HatchEgg(selectedEggId);
        if (petId.HasValue)
        {
            var eggData = PetEggDatabase.GetEgg(PetEggSystem.Instance.GetEgg(selectedEggId)?.eggId ?? "");
            string petTypeName = eggData?.petType ?? "unknown";
            ShowMessage($"🎉 Hatched a {petTypeName} pet! (ID: {petId.Value})");
            
            selectedEggId = "";
            UpdateEggList();
            UpdateStats();
            UpdateEggInfo();
        }
    }
    
    private void OnCloseButtonPressed()
    {
        Hide();
    }
    
    private Color GetRarityColor(int rarity)
    {
        switch (rarity)
        {
            case 1: return new Color(0.616f, 0.616f, 0.616f); // Common - gray
            case 2: return new Color(0.118f, 1f, 0f); // Uncommon - green
            case 3: return new Color(0f, 0.439f, 0.867f); // Rare - blue
            case 4: return new Color(0.639f, 0.208f, 0.933f); // Epic - purple
            case 5: return new Color(1f, 0.502f, 0f); // Legendary - orange
            default: return Colors.White;
        }
    }
    
    private void ShowMessage(string message)
    {
        // Simple message display - in real implementation would show a popup
        GD.Print(message);
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.H)
        {
            if (Visible)
                Hide();
            else
                Show();
        }
    }
    
    public void Show()
    {
        Visible = true;
        UpdateEggList();
        UpdateStats();
        UpdateEggInfo();
    }
}
