using Godot;
using System;
using System.Collections.Generic;

public class StreakUI : Control
{
    private StreakSystem _streakSystem;
    private Label _titleLabel;
    private VBoxContainer _streakContainer;
    private Button _closeButton;
    
    private Dictionary<StreakType, StreakRecord> _streakRecords;
    
    public override void _Ready()
    {
        _streakSystem = GetNode<StreakSystem>("/root/StreakSystem");
        
        SetupUI();
        RefreshDisplay();
    }
    
    private void SetupUI()
    {
        // Main panel
        PanelContainer mainPanel = new PanelContainer();
        mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainPanel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(mainPanel);
        
        VBoxContainer mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🎯 Streak System";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(_titleLabel);
        
        // Close button
        _closeButton = new Button();
        _closeButton.Text = "✕ Close";
        _closeButton.AlignMode = Button.TextAlign.Center;
        _closeButton.CustomMinimumSize = new Vector2(100, 30);
        _closeButton.Connect("pressed", this, nameof(OnClosePressed));
        mainVBox.AddChild(_closeButton);
        
        // Separator
        mainVBox.AddChild(new HSeparator());
        
        // Streak container (scrollable)
        ScrollContainer scrollContainer = new ScrollContainer();
        scrollContainer.SetHorizontalStretchExpand();
        scrollContainer.SetVerticalStretchExpand();
        mainVBox.AddChild(scrollContainer);
        
        _streakContainer = new VBoxContainer();
        scrollContainer.AddChild(_streakContainer);
        
        // Statistics section
        mainVBox.AddChild(new HSeparator());
        
        Label statsLabel = new Label();
        statsLabel.Text = "📊 Statistics";
        statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(statsLabel);
        
        HBoxContainer statsBox = new HBoxContainer();
        mainVBox.AddChild(statsBox);
        
        Label totalGoldLabel = new Label();
        totalGoldLabel.Text = $"💰 Gold: {_streakSystem.GetTotalGoldFromStreaks()}";
        statsBox.AddChild(totalGoldLabel);
        
        Label totalExpLabel = new Label();
        totalExpLabel.Text = $"✨ Exp: {_streakSystem.GetTotalExpFromStreaks()}";
        statsBox.AddChild(totalExpLabel);
        
        Label rewardsLabel = new Label();
        rewardsLabel.Text = $"🎁 Claims: {_streakSystem.GetTotalRewardsClaimed()}";
        statsBox.AddChild(rewardsLabel);
        
        Label freezeLabel = new Label();
        freezeLabel.Text = $"❄️ Freezes: {_streakSystem.GetStreakFreezeTokens()}";
        statsBox.AddChild(freezeLabel);
    }
    
    private void RefreshDisplay()
    {
        // Clear existing
        foreach (Node child in _streakContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        _streakRecords = _streakSystem.GetAllStreaks();
        
        // Create streak cards
        foreach (var kvp in _streakRecords)
        {
            StreakType type = kvp.Key;
            StreakRecord record = kvp.Value;
            
            CreateStreakCard(type, record);
        }
    }
    
    private void CreateStreakCard(StreakType type, StreakRecord record)
    {
        PanelContainer card = new PanelContainer();
        card.SetHorizontalExpand();
        card.CustomMinimumSize = new Vector2(0, 80);
        _streakContainer.AddChild(card);
        
        HBoxContainer cardContent = new HBoxContainer();
        card.AddChild(cardContent);
        
        // Icon and name
        VBoxContainer infoBox = new VBoxContainer();
        cardContent.AddChild(infoBox);
        
        Label typeLabel = new Label();
        typeLabel.Text = GetStreakTypeName(type);
        typeLabel.AddThemeFontSizeOverride("font_size", 18);
        infoBox.AddChild(typeLabel);
        
        Label currentLabel = new Label();
        currentLabel.Text = $"🔥 Current: {record.CurrentStreak} days";
        currentLabel.AddThemeFontSizeOverride("font_size", 14);
        infoBox.AddChild(currentLabel);
        
        Label bestLabel = new Label();
        bestLabel.Text = $"⭐ Best: {record.BestStreak} days";
        bestLabel.AddThemeFontSizeOverride("font_size", 12);
        infoBox.AddChild(bestLabel);
        
        Label totalLabel = new Label();
        totalLabel.Text = $"📅 Total: {record.TotalDays} days";
        totalLabel.AddThemeFontSizeOverride("font_size", 12);
        infoBox.AddChild(totalLabel);
        
        // Progress bar
        VBoxContainer progressBox = new VBoxContainer();
        progressBox.SetHorizontalExpand();
        cardContent.AddChild(progressBox);
        
        ProgressBar progressBar = new ProgressBar();
        progressBar.SetHorizontalExpand();
        progressBar.MinValue = 0;
        progressBar.MaxValue = 30;
        progressBar.Value = record.CurrentStreak;
        progressBox.AddChild(progressBar);
        
        Label progressLabel = new Label();
        progressLabel.Text = $"{record.CurrentStreak} / 30 days";
        progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
        progressBox.AddChild(progressLabel);
        
        // Claim button
        VBoxContainer buttonBox = new VBoxContainer();
        cardContent.AddChild(buttonBox);
        
        Button claimButton = new Button();
        claimButton.Text = record.ClaimedToday ? "✅ Claimed" : "🎁 Claim";
        claimButton.Disabled = record.ClaimedToday || record.CurrentStreak == 0;
        claimButton.Connect("pressed", this, nameof(OnClaimPressed), new Godot.Collections.Array { type });
        buttonBox.AddChild(claimButton);
        
        // Streak freeze button
        Button freezeButton = new Button();
        freezeButton.Text = "❄️ Freeze";
        freezeButton.Disabled = !_streakSystem.CanUseStreakFreeze();
        freezeButton.Connect("pressed", this, nameof(OnFreezePressed), new Godot.Collections.Array { type });
        buttonBox.AddChild(freezeButton);
        
        // Status indicator
        Label statusLabel = new Label();
        if (record.CurrentStreak == 0)
        {
            statusLabel.Text = "⚠️ Broken";
            statusLabel.Modulate = new Color(1, 0.3, 0.3);
        }
        else if (record.ClaimedToday)
        {
            statusLabel.Text = "✓ Active";
            statusLabel.Modulate = new Color(0.3, 1, 0.3);
        }
        else
        {
            statusLabel.Text = "⏳ Pending";
            statusLabel.Modulate = new Color(1, 0.8, 0.3);
        }
        buttonBox.AddChild(statusLabel);
    }
    
    private string GetStreakTypeName(StreakType type) => type switch
    {
        StreakType.Login => "📅 Daily Login",
        StreakType.Battle => "⚔️ Battle",
        StreakType.Quest => "📜 Quests",
        StreakType.Dungeon => "🏰 Dungeons",
        StreakType.PetInteraction => "🐾 Pet Interaction",
        _ => "Unknown"
    };
    
    private void OnClaimPressed(StreakType type)
    {
        var reward = _streakSystem.ClaimReward(type);
        GD.Print($"Claimed reward for {type}: {reward.Gold} gold, {reward.Exp} exp");
        RefreshDisplay();
    }
    
    private void OnFreezePressed(StreakType type)
    {
        if (_streakSystem.UseStreakFreeze(type))
        {
            GD.Print($"Used streak freeze for {type}");
            RefreshDisplay();
        }
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            OnClosePressed();
        }
    }
}
