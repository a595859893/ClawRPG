using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 随机挑战UI系统 - 管理挑战界面显示
/// 包含挑战列表、详细信息、刷新功能等
/// </summary>
public partial class ProceduralChallengeUI : Control
{
    private VBoxContainer _mainContainer;
    private VBoxContainer _challengeList;
    private Label _titleLabel;
    private Label _statsLabel;
    private Button _refreshButton;
    private Dictionary<string, Control> _challengePanels = new Dictionary<string, Control>();

    private Color _commonColor = new Color(0.62f, 0.62f, 0.62f);
    private Color _uncommonColor = new Color(0.3f, 0.69f, 0.31f);
    private Color _rareColor = new Color(0.13f, 0.59f, 0.95f);
    private Color _epicColor = new Color(0.61f, 0.15f, 0.69f);
    private Color _legendaryColor = new Color(1f, 0.6f, 0f);

    public override void _Ready()
    {
        Visible = false; 
        SetupUI();
        
        // Connect to challenge system signals
        if (ProceduralChallengeSystem.Instance != null)
        {
            ProceduralChallengeSystem.Instance.ChallengeStarted += OnChallengeStarted;
            ProceduralChallengeSystem.Instance.ChallengeUpdated += OnChallengeUpdated;
            ProceduralChallengeSystem.Instance.ChallengeCompleted += OnChallengeCompleted;
            ProceduralChallengeSystem.Instance.ChallengeFailed += OnChallengeFailed;
        }
    }

    private void SetupUI()
    {
        // Main panel
        PanelContainer mainPanel = new PanelContainer();
        mainPanel.SetAnchor(AnchorsPreset.Center);
        mainPanel.SetOffset(-300, -250, 300, 250);
        mainPanel.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(mainPanel);

        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchor(AnchorsPreset.FullRect);
        mainPanel.AddChild(_mainContainer);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ Procedural Challenges";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(_titleLabel);

        // Stats
        _statsLabel = new Label();
        _statsLabel.Text = "Total Completed: 0 | Gold Earned: 0 | Exp Earned: 0";
        _statsLabel.Align = Label.AlignEnum.Center;
        _statsLabel.AddThemeFontSizeOverride("font_size", 14);
        _mainContainer.AddChild(_statsLabel);

        // Scroll container for challenges
        ScrollContainer scrollContainer = new ScrollContainer();
        scrollContainer.SetAnchor(AnchorsPreset.FullRect);
        scrollContainer.SetOffset(new Vector2(0, 80), new Vector2(0, -50));
        _mainContainer.AddChild(scrollContainer);

        _challengeList = new VBoxContainer();
        _challengeList.SetAnchor(AnchorsPreset.FullRect);
        scrollContainer.AddChild(_challengeList);

        // Refresh button
        _refreshButton = new Button();
        _refreshButton.Text = "🔄 Refresh Challenges";
        _refreshButton.SetOffset(100, 450, 500, 500);
        _refreshButton.Pressed += OnRefreshPressed;
        _mainContainer.AddChild(_refreshButton);

        // Close button
        Button closeButton = new Button();
        closeButton.Text = "✕ Close";
        closeButton.Pressed += OnClosePressed;
        _mainContainer.AddChild(closeButton);
    }

    public override void _Process(double delta)
    {
        if (Visible)
        {
            RefreshChallengeList();
            UpdateStats();
        }
    }

    private void RefreshChallengeList()
    {
        // Clear existing panels
        foreach (var panel in _challengePanels.Values)
        {
            panel.QueueFree();
        }
        _challengePanels.Clear();

        if (ProceduralChallengeSystem.Instance == null) return;

        var challenges = ProceduralChallengeSystem.Instance.GetActiveChallenges();
        
        foreach (var challenge in challenges)
        {
            var panel = CreateChallengePanel(challenge);
            _challengeList.AddChild(panel);
            _challengePanels[challenge.InstanceId] = panel;
        }
    }

    private Control CreateChallengePanel(ProceduralChallengeData.ActiveChallenge challenge)
    {
        var template = ProceduralChallengeDatabase.GetTemplate(challenge.TemplateId);
        if (template == null) return new Control();

        PanelContainer panel = new PanelContainer();
        panel.SetCustomMinimumSize(new Vector2(0, 80));
        
        // Set panel color based on rarity
        StyleBoxFlat style = new StyleBoxFlat();
        style.BgColor = GetRarityColor(challenge.Rarity) * new Color(1, 1, 1, 0.3f);
        style.BorderWidthLeft = 3;
        style.BorderWidthRight = 3;
        style.BorderWidthTop = 3;
        style.BorderWidthBottom = 3;
        style.BorderColor = GetRarityColor(challenge.Rarity);
        style.CornerRadiusTopLeft = 5;
        style.CornerRadiusTopRight = 5;
        style.CornerRadiusBottomLeft = 5;
        style.CornerRadiusBottomRight = 5;
        panel.AddThemeStyleboxOverride("panel", style);

        HBoxContainer hbox = new HBoxContainer();
        panel.AddChild(hbox);

        // Info section
        VBoxContainer info = new VBoxContainer();
        hbox.AddChild(info);

        // Name and rarity
        Label nameLabel = new Label();
        nameLabel.Text = $"{template.Name} [{challenge.Rarity}]";
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        nameLabel.Modulate = GetRarityColor(challenge.Rarity);
        info.AddChild(nameLabel);

        // Description
        Label descLabel = new Label();
        string desc = template.Description
            .Replace("{count}", challenge.TargetProgress.ToString())
            .Replace("{time}", (challenge.TimeLimit / 60).ToString() + " min");
        descLabel.Text = desc;
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        info.AddChild(descLabel);

        // Progress bar
        ProgressBar progressBar = new ProgressBar();
        progressBar.MinValue = 0;
        progressBar.MaxValue = challenge.TargetProgress;
        progressBar.Value = challenge.CurrentProgress;
        progressBar.CustomMinimumSize = new Vector2(200, 20);
        info.AddChild(progressBar);

        // Progress label
        Label progressLabel = new Label();
        progressLabel.Text = $"{challenge.CurrentProgress} / {challenge.TargetProgress}";
        progressLabel.Align = Label.AlignEnum.Center;
        progressBar.AddChild(progressLabel);

        // Time remaining (if applicable)
        if (challenge.TimeLimit > 0)
        {
            Label timeLabel = new Label();
            int minutes = challenge.TimeRemaining / 60;
            int seconds = challenge.TimeRemaining % 60;
            timeLabel.Text = $"⏱️ {minutes}:{seconds:D2}";
            timeLabel.AddThemeFontSizeOverride("font_size", 14);
            info.AddChild(timeLabel);
        }

        // Rewards section
        VBoxContainer rewards = new VBoxContainer();
        hbox.AddChild(rewards);

        Label goldLabel = new Label();
        goldLabel.Text = $"💰 {challenge.GoldReward}";
        goldLabel.Modulate = new Color(1f, 0.84f, 0f);
        rewards.AddChild(goldLabel);

        Label expLabel = new Label();
        expLabel.Text = $"✨ {challenge.ExpReward}";
        expLabel.Modulate = new Color(0.4f, 0.8f, 1f);
        rewards.AddChild(expLabel);

        // Action button
        Button actionButton = new Button();
        actionButton.CustomMinimumSize = new Vector2(100, 40);
        
        switch (challenge.Status)
        {
            case ProceduralChallengeData.ChallengeStatus.Available:
                actionButton.Text = "Start";
                actionButton.Pressed += () => OnStartPressed(challenge.InstanceId);
                break;
            case ProceduralChallengeData.ChallengeStatus.InProgress:
                actionButton.Text = "In Progress";
                actionButton.Disabled = true;
                break;
            case ProceduralChallengeData.ChallengeStatus.Completed:
                actionButton.Text = "Completed";
                actionButton.Disabled = true;
                break;
            case ProceduralChallengeData.ChallengeStatus.Failed:
                actionButton.Text = "Failed";
                actionButton.Disabled = true;
                break;
            default:
                actionButton.Text = "Unknown";
                actionButton.Disabled = true;
                break;
        }
        
        rewards.AddChild(actionButton);

        return panel;
    }

    private Color GetRarityColor(ProceduralChallengeData.ChallengeRarity rarity)
    {
        switch (rarity)
        {
            case ProceduralChallengeData.ChallengeRarity.Common: return _commonColor;
            case ProceduralChallengeData.ChallengeRarity.Uncommon: return _uncommonColor;
            case ProceduralChallengeData.ChallengeRarity.Rare: return _rareColor;
            case ProceduralChallengeData.ChallengeRarity.Epic: return _epicColor;
            case ProceduralChallengeData.ChallengeRarity.Legendary: return _legendaryColor;
            default: return Colors.White;
        }
    }

    private void UpdateStats()
    {
        if (ProceduralChallengeSystem.Instance == null) return;

        var stats = ProceduralChallengeSystem.Instance.GetStatistics();
        _statsLabel.Text = $"Total Completed: {stats["total_completed"]} | Gold Earned: {stats["total_gold"]} | Exp Earned: {stats["total_exp"]}";
    }

    private void OnStartPressed(string instanceId)
    {
        if (ProceduralChallengeSystem.Instance != null)
        {
            ProceduralChallengeSystem.Instance.StartChallenge(instanceId);
        }
    }

    private void OnRefreshPressed()
    {
        if (ProceduralChallengeSystem.Instance != null)
        {
            ProceduralChallengeSystem.Instance.RefreshChallenges();
        }
    }

    private void OnClosePressed()
    {
        Visible = false; 
    }

    private void OnChallengeStarted(string instanceId)
    {
        RefreshChallengeList();
    }

    private void OnChallengeUpdated(string instanceId, int current, int target)
    {
        RefreshChallengeList();
    }

    private void OnChallengeCompleted(string instanceId, int gold, int exp)
    {
        // Show completion notification
        RefreshChallengeList();
    }

    private void OnChallengeFailed(string instanceId)
    {
        RefreshChallengeList();
    }

    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            if (ProceduralChallengeSystem.Instance != null)
            {
                ProceduralChallengeSystem.Instance.RefreshChallenges();
            }
            RefreshChallengeList();
        }
    }
}
