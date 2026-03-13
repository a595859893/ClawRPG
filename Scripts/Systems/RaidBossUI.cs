using System;
using Godot;
using System.Collections.Generic;

public class RaidBossUI : Control
{
    private Label titleLabel;
    private Label raidStatusLabel;
    private Label phaseLabel;
    private Label healthLabel;
    private Label timerLabel;
    private Label playerCountLabel;
    private VBoxContainer raidListContainer;
    private VBoxContainer participantListContainer;
    private Label statsLabel;
    private TabContainer tabContainer;
    
    private Button createRaidButton;
    private Button startRaidButton;
    private Button joinRaidButton;
    private Button leaveRaidButton;
    private Button refreshButton;
    
    private OptionButton raidTypeOption;
    private OptionButton roleOption;
    
    // Colors
    private Color successColor = new Color(0.2f, 0.8f, 0.2f);
    private Color warningColor = new Color(1f, 0.6f, 0f);
    private Color dangerColor = new Color(1f, 0.2f, 0.2f);
    private Color phaseColor = new Color(0.3f, 0.6f, 1f);
    
    public override void _Ready()
    {
        SetupUI();
        RefreshRaidList();
    }
    
    private void SetupUI()
    {
        // Main container
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(mainContainer);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = " === RAID BOSS SYSTEM === ";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(titleLabel);
        
        // Tab container
        tabContainer = new TabContainer();
        tabContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.ShrinkEnd);
        mainContainer.AddChild(tabContainer);
        
        // === Raid Tab ===
        var raidTab = new VBoxContainer();
        raidTab.Name = "Raid";
        tabContainer.AddChild(raidTab);
        
        // Raid status panel
        var statusPanel = new PanelContainer();
        statusPanel.AddThemeStyleboxOverride("panel", CreateFlatStyle(new Color(0.15f, 0.15f, 0.2f)));
        raidTab.AddChild(statusPanel);
        
        var statusVBox = new VBoxContainer();
        statusPanel.AddChild(statusVBox);
        
        raidStatusLabel = new Label();
        raidStatusLabel.Text = "No Active Raid";
        raidStatusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statusVBox.AddChild(raidStatusLabel);
        
        var healthHBox = new HBoxContainer();
        healthHBox.Alignment = BoxContainer.Alignment.Center;
        statusVBox.AddChild(healthHBox);
        
        var healthTitleLabel = new Label();
        healthTitleLabel.Text = "Boss HP: ";
        healthHBox.AddChild(healthTitleLabel);
        
        healthLabel = new Label();
        healthLabel.Text = "0 / 0";
        healthHBox.AddChild(healthLabel);
        
        var phaseHBox = new HBoxContainer();
        phaseHBox.Alignment = BoxContainer.Alignment.Center;
        statusVBox.AddChild(phaseHBox);
        
        var phaseTitleLabel = new Label();
        phaseTitleLabel.Text = "Phase: ";
        phaseHBox.AddChild(phaseTitleLabel);
        
        phaseLabel = new Label();
        phaseLabel.Text = "0 / 0";
        phaseHBox.AddChild(phaseLabel);
        
        var timerHBox = new HBoxContainer();
        timerHBox.Alignment = BoxContainer.Alignment.Center;
        statusVBox.AddChild(timerHBox);
        
        var timerTitleLabel = new Label();
        timerTitleLabel.Text = "Enrage: ";
        timerHBox.AddChild(timerTitleLabel);
        
        timerLabel = new Label();
        timerLabel.Text = "0s / 0s";
        timerHBox.AddChild(timerLabel);
        
        playerCountLabel = new Label();
        playerCountLabel.Text = "Players: 0";
        playerCountLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statusVBox.AddChild(playerCountLabel);
        
        // Create raid section
        var createSection = new HBoxContainer();
        createSection.Alignment = BoxContainer.Alignment.Center;
        raidTab.AddChild(createSection);
        
        var raidTypeLabel = new Label();
        raidTypeLabel.Text = "Raid Type: ";
        createSection.AddChild(raidTypeLabel);
        
        raidTypeOption = new OptionButton();
        PopulateRaidTypes();
        createSection.AddChild(raidTypeOption);
        
        var roleLabel = new Label();
        roleLabel.Text = " Role: ";
        createSection.AddChild(roleLabel);
        
        roleOption = new OptionButton();
        PopulateRoles();
        createSection.AddChild(roleOption);
        
        // Action buttons
        var buttonHBox = new HBoxContainer();
        buttonHBox.Alignment = BoxContainer.Alignment.Center;
        raidTab.AddChild(buttonHBox);
        
        createRaidButton = new Button();
        createRaidButton.Text = "Create Raid";
        createRaidButton.Pressed += OnCreateRaidPressed;
        buttonHBox.AddChild(createRaidButton);
        
        joinRaidButton = new Button();
        joinRaidButton.Text = "Join";
        joinRaidButton.Pressed += OnJoinRaidPressed;
        buttonHBox.AddChild(joinRaidButton);
        
        startRaidButton = new Button();
        startRaidButton.Text = "Start";
        startRaidButton.Pressed += OnStartRaidPressed;
        buttonHBox.AddChild(startRaidButton);
        
        leaveRaidButton = new Button();
        leaveRaidButton.Text = "Leave";
        leaveRaidButton.Pressed += OnLeaveRaidPressed;
        buttonHBox.AddChild(leaveRaidButton);
        
        // Participant list
        var participantLabel = new Label();
        participantLabel.Text = "=== Participants ===";
        participantLabel.HorizontalAlignment = HorizontalAlignment.Center;
        raidTab.AddChild(participantLabel);
        
        var participantScroll = new ScrollContainer();
        participantScroll.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.ShrinkEnd);
        participantScroll.CustomMinimumSize = new Vector2(0, 200);
        raidTab.AddChild(participantScroll);
        
        participantListContainer = new VBoxContainer();
        participantListContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.ShrinkEnd);
        participantScroll.AddChild(participantListContainer);
        
        // === Statistics Tab ===
        var statsTab = new VBoxContainer();
        statsTab.Name = "Statistics";
        tabContainer.AddChild(statsTab);
        
        statsLabel = new Label();
        statsLabel.Text = "Loading statistics...";
        statsLabel.VerticalAlignment = VerticalAlignment.Center;
        statsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statsTab.AddChild(statsLabel);
        
        refreshButton = new Button();
        refreshButton.Text = "Refresh";
        refreshButton.Pressed += RefreshStats;
        mainContainer.AddChild(refreshButton);
        
        // Update loop
        SetProcess(true);
    }
    
    private void PopulateRaidTypes()
    {
        raidTypeOption.Clear();
        var raidTypes = Enum.GetValues(typeof(RaidBossType));
        foreach (RaidBossType type in raidTypes)
        {
            raidTypeOption.AddItem(type.ToString(), (int)type);
        }
    }
    
    private void PopulateRoles()
    {
        roleOption.Clear();
        var roles = Enum.GetValues(typeof(RaidRole));
        foreach (RaidRole role in roles)
        {
            roleOption.AddItem(role.ToString(), (int)role);
        }
    }
    
    public override void _Process(float delta)
    {
        // Update raid status
        UpdateRaidStatus();
        
        // Update participants
        UpdateParticipantList();
    }
    
    private void UpdateRaidStatus()
    {
        var raidSystem = RaidBossSystem.Instance;
        var status = raidSystem.GetRaidStatus();
        
        int raidId = (int)status["raidId"];
        
        if (raidId <= 0)
        {
            raidStatusLabel.Text = "No Active Raid";
            healthLabel.Text = "0 / 0";
            phaseLabel.Text = "0 / 0";
            timerLabel.Text = "0s / 0s";
            playerCountLabel.Text = "Players: 0";
            
            createRaidButton.Disabled = false;
            joinRaidButton.Disabled = true;
            startRaidButton.Disabled = true;
            leaveRaidButton.Disabled = true;
        }
        else
        {
            string state = (string)status["state"];
            raidStatusLabel.Text = $"{status["raidName"]} - {state}";
            
            float health = (float)status["bossHealth"];
            float maxHealth = (float)status["bossMaxHealth"];
            healthLabel.Text = $"{(int)health} / {(int)maxHealth}";
            
            // Color based on health
            float healthPercent = maxHealth > 0 ? health / maxHealth : 0;
            if (healthPercent > 0.5f)
                healthLabel.Modulate = successColor;
            else if (healthPercent > 0.25f)
                healthLabel.Modulate = warningColor;
            else
                healthLabel.Modulate = dangerColor;
            
            phaseLabel.Text = $"{status["phase"]} / {status["maxPhases"]}";
            phaseLabel.Modulate = phaseColor;
            
            float timer = (float)status["enrageTimer"];
            float maxTimer = (float)status["maxEnrageTime"];
            timerLabel.Text = $"{(int)timer}s / {(int)maxTimer}s";
            
            // Color based on timer
            if (timer < maxTimer * 0.5f)
                timerLabel.Modulate = successColor;
            else if (timer < maxTimer * 0.75f)
                timerLabel.Modulate = warningColor;
            else
                timerLabel.Modulate = dangerColor;
            
            playerCountLabel.Text = $"Players: {status["playerCount"]}";
            
            createRaidButton.Disabled = true;
            
            bool inRaid = state == "Recruiting" || state == "InProgress";
            joinRaidButton.Disabled = !inRaid;
            startRaidButton.Disabled = state != "Recruiting";
            leaveRaidButton.Disabled = !inRaid;
        }
    }
    
    private void UpdateParticipantList()
    {
        // Clear and rebuild participant list
        foreach (var child in participantListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var raidSystem = RaidBossSystem.Instance;
        var status = raidSystem.GetRaidStatus();
        int raidId = (int)status["raidId"];
        
        if (raidId <= 0)
            return;
        
        // This would need access to participants from RaidBossSystem
        // For now, show placeholder
        var noParticipantsLabel = new Label();
        noParticipantsLabel.Text = "No participants yet";
        noParticipantsLabel.HorizontalAlignment = HorizontalAlignment.Center;
        participantListContainer.AddChild(noParticipantsLabel);
    }
    
    private void RefreshRaidList()
    {
        // Refresh raid list
    }
    
    private void RefreshStats()
    {
        var raidSystem = RaidBossSystem.Instance;
        var stats = raidSystem.GetStatistics();
        
        float winRate = (float)stats["winRate"];
        
        statsLabel.Text = $@"=== RAID STATISTICS ===

Total Raids Joined: {stats["totalRaidsJoined"]}
Total Completed: {stats["totalRaidsCompleted"]}
Total Failed: {stats["totalRaidsFailed"]}
Win Rate: {winRate:F1}%

Total Boss Kills: {stats["totalBossKills"]}
Total Damage Dealt: {stats["totalDamageDealt"]}
Total Healing Done: {stats["totalHealingDone"]}

Best Clear Time: {stats["bestClearTime"]}s";
    }
    
    private void OnCreateRaidPressed()
    {
        var raidType = (RaidBossType)raidTypeOption.GetSelectedId();
        RaidBossSystem.Instance.CreateRaid(raidType, "Player1", "Player1");
        RefreshStats();
    }
    
    private void OnJoinRaidPressed()
    {
        var role = (RaidRole)roleOption.GetSelectedId();
        RaidBossSystem.Instance.JoinRaid("Player1", "Player1", role);
    }
    
    private void OnStartRaidPressed()
    {
        RaidBossSystem.Instance.StartRaid();
    }
    
    private void OnLeaveRaidPressed()
    {
        RaidBossSystem.Instance.LeaveRaid("Player1");
    }
    
    private StyleBoxFlat CreateFlatStyle(Color color)
    {
        var style = new StyleBoxFlat();
        style.BgColor = color;
        style.CornerRadiusTopLeft = 5;
        style.CornerRadiusTopRight = 5;
        style.CornerRadiusBottomLeft = 5;
        style.CornerRadiusBottomRight = 5;
        return style;
    }
}
