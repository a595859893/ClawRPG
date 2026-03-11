using Godot;
using System;
using System.Collections.Generic;

public class BattlePassUI : Control
{
    private Control mainPanel;
    private VBoxContainer seasonInfo;
    private ProgressBar xpProgress;
    private Label levelLabel;
    private Label xpLabel;
    private ScrollContainer rewardsContainer;
    private VBoxContainer rewardsVBox;
    private ScrollContainer challengesContainer;
    private VBoxContainer challengesVBox;
    private Button premiumButton;
    private Label premiumStatusLabel;
    private Button closeButton;
    
    private bool isPremium = false;
    
    public override void _Ready()
    {
        SetupUI();
        LoadData();
        Refresh();
    }
    
    private void SetupUI()
    {
        // Main panel
        mainPanel = new Control();
        mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainPanel);
        
        var bg = new ColorRect();
        bg.Color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        bg.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainPanel.AddChild(bg);
        
        // Title
        var title = new Label();
        title.Text = "BATTLE PASS";
        title.AddThemeFontSizeOverride("font_size", 32);
        title.HorizontalAlignment = HorizontalAlignment.Center;
        title.Position = new Vector2(0, 20);
        title.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        mainPanel.AddChild(title);
        
        // Season info
        seasonInfo = new VBoxContainer();
        seasonInfo.Position = new Vector2(50, 80);
        seasonInfo.CustomMinimumSize = new Vector2(700, 60);
        mainPanel.AddChild(seasonInfo);
        
        var seasonName = new Label();
        seasonName.Text = $"Season: {BattlePassManager.Instance.GetSeasonName()}";
        seasonName.AddThemeFontSizeOverride("font_size", 24);
        seasonInfo.AddChild(seasonName);
        
        var daysRemaining = new Label();
        daysRemaining.Text = $"Days Remaining: {BattlePassManager.Instance.GetDaysRemaining()}";
        daysRemaining.AddThemeFontSizeOverride("font_size", 18);
        seasonInfo.AddChild(daysRemaining);
        
        // Level and XP
        var levelContainer = new HBoxContainer();
        levelContainer.Position = new Vector2(50, 160);
        levelContainer.CustomMinimumSize = new Vector2(700, 40);
        mainPanel.AddChild(levelContainer);
        
        levelLabel = new Label();
        levelLabel.Text = $"Level {BattlePassManager.Instance.GetCurrentLevel()}";
        levelLabel.AddThemeFontSizeOverride("font_size", 24);
        levelContainer.AddChild(levelLabel);
        
        xpProgress = new ProgressBar();
        xpProgress.CustomMinimumSize = new Vector2(500, 30);
        xpProgress.CustomMinimumSize = new Vector2(0, 30);
        xpProgress.MinValue = 0;
        xpProgress.MaxValue = BattlePassManager.Instance.GetXPToNextLevel();
        xpProgress.Value = BattlePassManager.Instance.GetCurrentXP();
        levelContainer.AddChild(xpProgress);
        
        xpLabel = new Label();
        xpLabel.Text = $"{BattlePassManager.Instance.GetCurrentXP()}/{BattlePassManager.Instance.GetXPToNextLevel()} XP";
        levelContainer.AddChild(xpLabel);
        
        // Premium status
        premiumStatusLabel = new Label();
        premiumStatusLabel.Position = new Vector2(50, 220);
        premiumStatusLabel.Text = "Free Pass";
        premiumStatusLabel.AddThemeFontSizeOverride("font_size", 20);
        mainPanel.AddChild(premiumStatusLabel);
        
        // Premium button
        premiumButton = new Button();
        premiumButton.Text = "Upgrade to Premium - 980 Diamonds";
        premiumButton.Position = new Vector2(200, 215);
        premiumButton.CustomMinimumSize = new Vector2(250, 35);
        premiumButton.Pressed += OnPremiumPressed;
        mainPanel.AddChild(premiumButton);
        
        // Rewards section
        var rewardsTitle = new Label();
        rewardsTitle.Text = "REWARDS";
        rewardsTitle.AddThemeFontSizeOverride("font_size", 22);
        rewardsTitle.Position = new Vector2(50, 270);
        mainPanel.AddChild(rewardsTitle);
        
        // Free rewards button
        var freeBtn = new Button();
        freeBtn.Text = "Free Rewards";
        freeBtn.Position = new Vector2(50, 310);
        freeBtn.CustomMinimumSize = new Vector2(120, 30);
        freeBtn.Pressed += () => ShowRewards(false);
        mainPanel.AddChild(freeBtn);
        
        // Premium rewards button
        var premBtn = new Button();
        premBtn.Text = "Premium Rewards";
        premBtn.Position = new Vector2(180, 310);
        premBtn.CustomMinimumSize = new Vector2(150, 30);
        premBtn.Pressed += () => ShowRewards(true);
        mainPanel.AddChild(premBtn);
        
        rewardsContainer = new ScrollContainer();
        rewardsContainer.Position = new Vector2(50, 350);
        rewardsContainer.CustomMinimumSize = new Vector2(350, 200);
        mainPanel.AddChild(rewardsContainer);
        
        rewardsVBox = new VBoxContainer();
        rewardsVBox.CustomMinimumSize = new Vector2(350, 0);
        rewardsContainer.AddChild(rewardsVBox);
        
        // Challenges section
        var challengesTitle = new Label();
        challengesTitle.Text = "CHALLENGES";
        challengesTitle.AddThemeFontSizeOverride("font_size", 22);
        challengesTitle.Position = new Vector2(420, 270);
        mainPanel.AddChild(challengesTitle);
        
        challengesContainer = new ScrollContainer();
        challengesContainer.Position = new Vector2(420, 310);
        challengesContainer.CustomMinimumSize = new Vector2(350, 240);
        mainPanel.AddChild(challengesContainer);
        
        challengesVBox = new VBoxContainer();
        challengesVBox.CustomMinimumSize = new Vector2(350, 0);
        challengesContainer.AddChild(challengesVBox);
        
        // Close button
        closeButton = new Button();
        closeButton.Text = "Close (B)";
        closeButton.Position = new Vector2(650, 560);
        closeButton.CustomMinimumSize = new Vector2(120, 40);
        closeButton.Pressed += OnClosePressed;
        mainPanel.AddChild(closeButton);
        
        // Input
        SetProcessInput(true);
    }
    
    private void LoadData()
    {
        isPremium = BattlePassManager.Instance.HasPremiumPass();
    }
    
    private void Refresh()
    {
        levelLabel.Text = $"Level {BattlePassManager.Instance.GetCurrentLevel()}";
        xpProgress.MaxValue = BattlePassManager.Instance.GetXPToNextLevel();
        xpProgress.Value = BattlePassManager.Instance.GetCurrentXP();
        xpLabel.Text = $"{BattlePassManager.Instance.GetCurrentXP()}/{BattlePassManager.Instance.GetXPToNextLevel()} XP";
        
        premiumStatusLabel.Text = isPremium ? "Premium Pass Active" : "Free Pass";
        premiumButton.Visible = !isPremium;
        
        ShowRewards(false);
        ShowChallenges();
    }
    
    private void ShowRewards(bool premium)
    {
        foreach (var child in rewardsVBox.GetChildren())
        {
            child.QueueFree();
        }
        
        var rewards = premium ? BattlePassManager.Instance.GetPremiumRewards() : BattlePassManager.Instance.GetFreeRewards();
        
        foreach (var reward in rewards)
        {
            var rewardPanel = new HBoxContainer();
            rewardPanel.CustomMinimumSize = new Vector2(330, 40);
            rewardsVBox.AddChild(rewardPanel);
            
            var levelLabel = new Label();
            levelLabel.Text = $"Lv.{reward.Level}";
            levelLabel.CustomMinimumSize = new Vector2(50, 0);
            rewardPanel.AddChild(levelLabel);
            
            var nameLabel = new Label();
            nameLabel.Text = $"{reward.Amount}x {reward.Type}";
            nameLabel.CustomMinimumSize = new Vector2(200, 0);
            rewardPanel.AddChild(nameLabel);
            
            var claimBtn = new Button();
            claimBtn.Text = "Claim";
            claimBtn.CustomMinimumSize = new Vector2(80, 30);
            claimBtn.Pressed += () => OnClaimReward(reward.Level, premium);
            rewardPanel.AddChild(claimBtn);
        }
    }
    
    private void ShowChallenges()
    {
        foreach (var child in challengesVBox.GetChildren())
        {
            child.QueueFree();
        }
        
        var challenges = BattlePassManager.Instance.GetChallenges();
        
        foreach (var challenge in challenges)
        {
            var challengePanel = new VBoxContainer();
            challengePanel.CustomMinimumSize = new Vector2(330, 60);
            challengesVBox.AddChild(challengePanel);
            
            var nameLabel = new Label();
            nameLabel.Text = challenge.Name;
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            challengePanel.AddChild(nameLabel);
            
            var descLabel = new Label();
            descLabel.Text = $"{challenge.Description} ({challenge.Progress}/{challenge.Target})";
            challengePanel.AddChild(descLabel);
            
            var progressBar = new ProgressBar();
            progressBar.MinValue = 0;
            progressBar.MaxValue = challenge.Target;
            progressBar.Value = challenge.Progress;
            progressBar.CustomMinimumSize = new Vector2(0, 20);
            challengePanel.AddChild(progressBar);
        }
    }
    
    private void OnClaimReward(int level, bool premium)
    {
        if (premium && !isPremium)
        {
            GD.Print("Need premium pass to claim premium rewards!");
            return;
        }
        
        BattlePassManager.Instance.ClaimReward(level, premium);
        Refresh();
    }
    
    private void OnPremiumPressed()
    {
        // In real implementation, would trigger IAP
        GD.Print("Opening premium pass purchase...");
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey key && key.Pressed && key.Keycode == Key.B)
        {
            OnClosePressed();
        }
    }
}
