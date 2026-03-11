using Godot;
using System;
using System.Collections.Generic;

public class PrestigeUI : Control
{
    private Control mainPanel;
    private VBoxContainer infoContainer;
    private Label prestigeLevelLabel;
    private Label prestigePointsLabel;
    private Label prestigeTierLabel;
    private Label timesPrestigedLabel;
    private ProgressBar prestigeProgress;
    private VBoxContainer bonusesContainer;
    private VBoxContainer rewardsContainer;
    private Button prestigeButton;
    private Label prestigeButtonLabel;
    private Label statusLabel;
    private Button closeButton;
    
    private PrestigeSystem prestigeSystem;
    private int playerLevel;
    
    public override void _Ready()
    {
        prestigeSystem = PrestigeSystem.Instance;
        SetupUI();
        Refresh();
    }
    
    private void SetupUI()
    {
        // Main panel
        mainPanel = new Control();
        mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainPanel.CustomMinimumSize = new Vector2(700, 550);
        AddChild(mainPanel);
        
        var bg = new ColorRect();
        bg.Color = new Color(0.08f, 0.08f, 0.12f, 0.98f);
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainPanel.AddChild(bg);
        
        // Title
        var title = new Label();
        title.Text = "PRESTIGE SYSTEM";
        title.AddThemeFontSizeOverride("font_size", 36);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.Position = new Vector2(0, 15);
        title.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        mainPanel.AddChild(title);
        
        // Close button
        closeButton = new Button();
        closeButton.Text = "X";
        closeButton.Position = new Vector2(660, 10);
        closeButton.CustomMinimumSize = new Vector2(30, 30);
        closeButton.Pressed += () => Hide();
        mainPanel.AddChild(closeButton);
        
        // Info container
        infoContainer = new VBoxContainer();
        infoContainer.Position = new Vector2(30, 70);
        infoContainer.CustomMinimumSize = new Vector2(640, 120);
        mainPanel.AddChild(infoContainer);
        
        // Prestige Level
        prestigeLevelLabel = new Label();
        prestigeLevelLabel.Text = "Prestige Level: 0";
        prestigeLevelLabel.AddThemeFontSizeOverride("font_size", 28);
        infoContainer.AddChild(prestigeLevelLabel);
        
        // Prestige Points
        prestigePointsLabel = new Label();
        prestigePointsLabel.Text = "Prestige Points: 0";
        prestigePointsLabel.AddThemeFontSizeOverride("font_size", 22);
        infoContainer.AddChild(prestigePointsLabel);
        
        // Prestige Tier
        prestigeTierLabel = new Label();
        prestigeTierLabel.Text = "Tier: None";
        prestigeTierLabel.AddThemeFontSizeOverride("font_size", 20);
        infoContainer.AddChild(prestigeTierLabel);
        
        // Times Prestiged
        timesPrestigedLabel = new Label();
        timesPrestigedLabel.Text = "Times Prestiged: 0";
        timesPrestigedLabel.AddThemeFontSizeOverride("font_size", 18);
        infoContainer.AddChild(timesPrestigedLabel);
        
        // Progress bar
        var progressLabel = new Label();
        progressLabel.Text = "Prestige Progress";
        progressLabel.AddThemeFontSizeOverride("font_size", 16);
        progressLabel.Position = new Vector2(30, 200);
        mainPanel.AddChild(progressLabel);
        
        prestigeProgress = new ProgressBar();
        prestigeProgress.Position = new Vector2(30, 225);
        prestigeProgress.CustomMinimumSize = new Vector2(640, 25);
        prestigeProgress.MaxValue = 1.0;
        prestigeProgress.Value = 0;
        mainPanel.AddChild(prestigeProgress);
        
        // Bonuses section
        var bonusesTitle = new Label();
        bonusesTitle.Text = "ACTIVE BONUSES";
        bonusesTitle.AddThemeFontSizeOverride("font_size", 22);
        bonusesTitle.Position = new Vector2(30, 270);
        mainPanel.AddChild(bonusesTitle);
        
        bonusesContainer = new VBoxContainer();
        bonusesContainer.Position = new Vector2(30, 300);
        bonusesContainer.CustomMinimumSize = new Vector2(300, 150);
        mainPanel.AddChild(bonusesContainer);
        
        // Rewards section
        var rewardsTitle = new Label();
        rewardsTitle.Text = "PRESTIGE REWARDS";
        rewardsTitle.AddThemeFontSizeOverride("font_size", 22);
        rewardsTitle.Position = new Vector2(370, 270);
        mainPanel.AddChild(rewardsTitle);
        
        rewardsContainer = new VBoxContainer();
        rewardsContainer.Position = new Vector2(370, 300);
        rewardsContainer.CustomMinimumSize = new Vector2(300, 150);
        mainPanel.AddChild(rewardsContainer);
        
        // Status label
        statusLabel = new Label();
        statusLabel.Text = "";
        statusLabel.AddThemeFontSizeOverride("font_size", 16);
        statusLabel.Position = new Vector2(30, 460);
        statusLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainPanel.AddChild(statusLabel);
        
        // Prestige button
        prestigeButton = new Button();
        prestigeButton.Text = "";
        prestigeButton.Position = new Vector2(200, 490);
        prestigeButton.CustomMinimumSize = new Vector2(300, 50);
        prestigeButton.Pressed += OnPrestigePressed;
        mainPanel.AddChild(prestigeButton);
        
        prestigeButtonLabel = new Label();
        prestigeButtonLabel.Text = "PRESTIGE";
        prestigeButtonLabel.AddThemeFontSizeOverride("font_size", 24);
        prestigeButtonLabel.HorizontalAlignment = HorizontalAlignment.Center;
        prestigeButtonLabel.Position = new Vector2(0, 10);
        prestigeButtonLabel.SetAnchorsPreset(Control.LayoutPreset.Center);
        prestigeButton.AddChild(prestigeButtonLabel);
    }
    
    private void Refresh()
    {
        if (prestigeSystem == null)
            return;
        
        // Get player level from Main
        var main = GetTree().CurrentScene as ClawRPG.Scripts.Main;
        if (main != null && main.GetPlayer() != null)
        {
            playerLevel = main.GetPlayer().Level;
        }
        else
        {
            playerLevel = 100; // Default for testing
        }
        
        // Update labels
        prestigeLevelLabel.Text = $"Prestige Level: {prestigeSystem.PrestigeLevel}";
        prestigePointsLabel.Text = $"Prestige Points: {prestigeSystem.PrestigePoints}";
        prestigeTierLabel.Text = $"Tier: {prestigeSystem.GetPrestigeTierName()}";
        timesPrestigedLabel.Text = $"Times Prestiged: {prestigeSystem.TimesPrestiged}";
        
        // Update progress
        prestigeProgress.Value = prestigeSystem.GetPrestigeProgress();
        
        // Update bonuses
        UpdateBonuses();
        
        // Update rewards preview
        UpdateRewards();
        
        // Update button state
        UpdateButtonState();
    }
    
    private void UpdateBonuses()
    {
        // Clear existing
        foreach (var child in bonusesContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // Add bonus labels
        var expBonus = new Label();
        expBonus.Text = $"EXP Multiplier: {prestigeSystem.GetExperienceMultiplier():P0}";
        expBonus.AddThemeFontSizeOverride("font_size", 16);
        bonusesContainer.AddChild(expBonus);
        
        var goldBonus = new Label();
        goldBonus.Text = $"Gold Multiplier: {prestigeSystem.GetGoldMultiplier():P0}";
        goldBonus.AddThemeFontSizeOverride("font_size", 16);
        bonusesContainer.AddChild(goldBonus);
        
        var attrBonus = new Label();
        attrBonus.Text = $"Attribute Bonus: +{prestigeSystem.GetAttributeBonus()}";
        attrBonus.AddThemeFontSizeOverride("font_size", 16);
        bonusesContainer.AddChild(attrBonus);
        
        var tierColor = new Label();
        tierColor.Text = $"Tier Color: {prestigeSystem.GetPrestigeTierColor()}";
        tierColor.AddThemeFontSizeOverride("font_size", 16);
        bonusesContainer.AddChild(tierColor);
    }
    
    private void UpdateRewards()
    {
        // Clear existing
        foreach (var child in rewardsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (prestigeSystem.PrestigeLevel >= PrestigeSystem.MAX_PRESTIGE_LEVEL)
        {
            var maxLabel = new Label();
            maxLabel.Text = "MAX PRESTIGE REACHED!";
            maxLabel.AddThemeFontSizeOverride("font_size", 18);
            rewardsContainer.AddChild(maxLabel);
            return;
        }
        
        // Show next prestige rewards
        var nextPoints = prestigeSystem.CalculatePrestigePointsReward();
        var nextReward = new Label();
        nextReward.Text = $"Next Prestige: +{nextPoints} Points";
        nextReward.AddThemeFontSizeOverride("font_size", 16);
        rewardsContainer.AddChild(nextReward);
        
        var requiredLevel = prestigeSystem.GetRequiredLevelForPrestige();
        if (requiredLevel > 0)
        {
            var levelReq = new Label();
            levelReq.Text = $"Required Level: {requiredLevel}";
            levelReq.AddThemeFontSizeOverride("font_size", 16);
            rewardsContainer.AddChild(levelReq);
        }
        
        var totalEarned = new Label();
        totalEarned.Text = $"Total Earned: {prestigeSystem.TotalPrestigePointsEarned}";
        totalEarned.AddThemeFontSizeOverride("font_size", 16);
        rewardsContainer.AddChild(totalEarned);
    }
    
    private void UpdateButtonState()
    {
        if (prestigeSystem.PrestigeLevel >= PrestigeSystem.MAX_PRESTIGE_LEVEL)
        {
            prestigeButton.Disabled = true;
            prestigeButtonLabel.Text = "MAX LEVEL";
            statusLabel.Text = "You have reached maximum prestige level!";
            return;
        }
        
        bool canPrestige = prestigeSystem.CanPrestige(playerLevel);
        prestigeButton.Disabled = !canPrestige;
        
        if (canPrestige)
        {
            prestigeButtonLabel.Text = "PRESTIGE NOW!";
            statusLabel.Text = $"Ready to prestige! You will earn {prestigeSystem.CalculatePrestigePointsReward()} Prestige Points.";
        }
        else
        {
            prestigeButtonLabel.Text = "CANNOT PRESTIGE";
            int required = PrestigeSystem.REQUIRED_LEVEL;
            statusLabel.Text = $"Reach level {required} to prestige. Current: {playerLevel}";
        }
    }
    
    private void OnPrestigePressed()
    {
        if (prestigeSystem == null)
            return;
        
        if (!prestigeSystem.CanPrestige(playerLevel))
        {
            statusLabel.Text = "Cannot prestige yet!";
            return;
        }
        
        // Perform prestige
        bool success = prestigeSystem.PerformPrestige(playerLevel, 0, 0);
        
        if (success)
        {
            statusLabel.Text = $"Prestige successful! Now at level {prestigeSystem.PrestigeLevel}!";
            Refresh();
            
            // Notify player
            GD.Print("Prestige completed! Level: " + prestigeSystem.PrestigeLevel);
        }
        else
        {
            statusLabel.Text = "Prestige failed!";
        }
    }
    
    public void Show()
    {
        Visible = true;
        Refresh();
    }
    
    public void Hide()
    {
        Visible = false;
    }
    
    private void LoadData()
    {
        // Data is automatically loaded from save system
    }
}
