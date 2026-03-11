using Godot;
using System;
using System.Collections.Generic;

public class GuildWarUI : Control
{
    private Label titleLabel;
    private Label statusLabel;
    private VBoxContainer warListContainer;
    private VBoxContainer warDetailsContainer;
    private Button refreshButton;
    private Button closeButton;
    
    private GuildWarManager warManager;
    private int selectedWarId = -1;
    
    public override void _Ready()
    {
        warManager = GuildWarManager.Instance;
        
        SetupUI();
        ConnectSignals();
        RefreshWarList();
    }
    
    private void SetupUI()
    {
        // Main container
        Panel mainPanel = new Panel();
        mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainPanel);
        
        VBoxContainer mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainContainer.AddThemeConstantOverride("separation", 20);
        mainPanel.AddChild(mainContainer);
        
        // Title
        titleLabel = new Label();
        titleLabel.Text = "公会战";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(titleLabel);
        
        // Status
        statusLabel = new Label();
        statusLabel.Text = "";
        statusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        statusLabel.AddThemeColorOverride("font_color", new Color(1, 0.8f, 0));
        mainContainer.AddChild(statusLabel);
        
        // War list and details split
        HBoxContainer splitContainer = new HBoxContainer();
        splitContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        splitContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(splitContainer);
        
        // War list
        warListContainer = new VBoxContainer();
        warListContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        splitContainer.AddChild(warListContainer);
        
        Label listTitle = new Label();
        listTitle.Text = "进行中的战争";
        warListContainer.AddChild(listTitle);
        
        // War details
        warDetailsContainer = new VBoxContainer();
        warDetailsContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        splitContainer.AddChild(warDetailsContainer);
        
        Label detailsTitle = new Label();
        detailsTitle.Text = "战争详情";
        warDetailsContainer.AddChild(detailsTitle);
        
        // Buttons
        HBoxContainer buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        mainContainer.AddChild(buttonContainer);
        
        refreshButton = new Button();
        refreshButton.Text = "刷新";
        refreshButton.Pressed += OnRefreshPressed;
        buttonContainer.AddChild(refreshButton);
        
        closeButton = new Button();
        closeButton.Text = "关闭";
        closeButton.Pressed += OnClosePressed;
        buttonContainer.AddChild(closeButton);
    }
    
    private void ConnectSignals()
    {
        if (warManager != null)
        {
            warManager.Connect(nameof(GuildWarManager.WarStartedSignal), this, nameof(OnWarStarted));
            warManager.Connect(nameof(GuildWarManager.WarEndedSignal), this, nameof(OnWarEnded));
            warManager.Connect(nameof(GuildWarManager.PointsUpdatedSignal), this, nameof(OnPointsUpdated));
        }
    }
    
    private void RefreshWarList()
    {
        // Clear existing items
        foreach (Node child in warListContainer.GetChildren())
        {
            if (child is Button || child is Label)
                child.QueueFree();
        }
        
        if (warManager == null)
        {
            statusLabel.Text = "公会战系统未初始化";
            return;
        }
        
        // Get active wars
        List<GuildWarManager.GuildWarData> activeWars = new List<GuildWarManager.GuildWarData>();
        
        // Add some mock wars for display
        // In real implementation, this would come from GuildWarManager
        
        if (activeWars.Count == 0)
        {
            Label noWarsLabel = new Label();
            noWarsLabel.Text = "暂无进行中的战争";
            noWarsLabel.HorizontalAlignment = HorizontalAlignment.Center;
            warListContainer.AddChild(noWarsLabel);
            statusLabel.Text = "等待公会宣战...";
        }
    }
    
    private void UpdateWarDetails(int warId)
    {
        // Clear existing details
        foreach (Node child in warDetailsContainer.GetChildren())
        {
            if (child is Label || child is HBoxContainer)
                child.QueueFree();
        }
        
        if (warManager == null || warId < 0)
            return;
        
        GuildWarManager.GuildWarData war = warManager.GetWarInfo(warId);
        if (war == null)
        {
            Label noWarLabel = new Label();
            noWarLabel.Text = "选择一场战争查看详情";
            warDetailsContainer.AddChild(noWarLabel);
            return;
        }
        
        // War info
        Label warInfo = new Label();
        warInfo.Text = $"战争 #{war.warId}\n" +
                      $"进攻方: 公会 {war.attackerGuildId}\n" +
                      $"防守方: 公会 {war.defenderGuildId}\n" +
                      $"状态: {war.state}\n" +
                      $"结束时间: {war.endTime:HH:mm}";
        warDetailsContainer.AddChild(warInfo);
        
        // Score
        HBoxContainer scoreContainer = new HBoxContainer();
        warDetailsContainer.AddChild(scoreContainer);
        
        Label attackerScore = new Label();
        attackerScore.Text = $"进攻方得分: {war.attackerPoints}";
        attackerScore.AddThemeColorOverride("font_color", new Color(1, 0.3f, 0.3f));
        scoreContainer.AddChild(attackerScore);
        
        Label vsLabel = new Label();
        vsLabel.Text = "  VS  ";
        vsLabel.AddThemeFontSizeOverride("font_size", 20);
        scoreContainer.AddChild(vsLabel);
        
        Label defenderScore = new Label();
        defenderScore.Text = $"防守方得分: {war.defenderPoints}";
        defenderScore.AddThemeColorOverride("font_color", new Color(0.3f, 0.3f, 1));
        scoreContainer.AddChild(defenderScore);
        
        // Leaderboard
        Label leaderboardTitle = new Label();
        leaderboardTitle.Text = "\n贡献排行榜:";
        leaderboardTitle.AddThemeFontSizeOverride("font_size", 18);
        warDetailsContainer.AddChild(leaderboardTitle);
        
        var leaderboard = warManager.GetWarLeaderboard(warId);
        int rank = 1;
        foreach (var entry in leaderboard)
        {
            Label entryLabel = new Label();
            entryLabel.Text = $"#{rank} 玩家 {entry.Key}: {entry.Value} 贡献点";
            warDetailsContainer.AddChild(entryLabel);
            rank++;
            if (rank > 10) break;
        }
    }
    
    private void OnRefreshPressed()
    {
        RefreshWarList();
        if (selectedWarId >= 0)
            UpdateWarDetails(selectedWarId);
        
        // Process any wars that have ended
        warManager?.ProcessWars();
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
    
    private void OnWarStarted(int warId, int attackerGuildId, int defenderGuildId)
    {
        GD.Print($"Guild War started: {warId}");
        RefreshWarList();
        statusLabel.Text = $"战争 #{warId} 已开始!";
    }
    
    private void OnWarEnded(int warId, int winningGuildId, int attackerPoints, int defenderPoints)
    {
        GD.Print($"Guild War ended: {warId}, Winner: {winningGuildId}");
        RefreshWarList();
        statusLabel.Text = $"战争 #{warId} 已结束! 胜利方: 公会 {winningGuildId}";
    }
    
    private void OnPointsUpdated(int warId, int guildId, int points)
    {
        if (selectedWarId == warId)
            UpdateWarDetails(warId);
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                OnClosePressed();
            }
        }
    }
}
