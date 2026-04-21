using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 每日仪式UI - 显示仪式界面
/// 重构自 REQ-075: 移除对 DailyRitualSystem/DailyRitualDatabase 的直接引用，改为事件驱动解耦
/// </summary>
public partial class DailyRitualUI : Control
{
    // ===== 事件接口（UI → System 通信） =====
    // UI 层通过事件向外部发送操作请求，不直接持有 System/Database 引用

    /// <summary>请求刷新仪式列表（System 收到后调用 UpdateRitualList）</summary>
    public Action OnRefreshRequested;

    /// <summary>请求开始仪式（System 收到后处理，调用 NotifyRitualStarted）</summary>
    public Action<string> OnStartRitualRequested;

    /// <summary>请求清除加成（System 收到后处理）</summary>
    public Action OnClearBonusesRequested;

    // ===== UI组件引用 =====
    private VBoxContainer _mainContainer;
    private HBoxContainer _headerContainer;
    private Label _titleLabel;
    private Label _dailyCountLabel;
    private GridContainer _ritualGrid;
    private Label _statsLabel;
    private Button _closeButton;

    private List<RitualData> _displayedRituals = new List<RitualData>();

    // ===== 纯展示状态（由外部更新） =====
    private List<RitualData> _allRituals = new List<RitualData>();
    private List<string> _unlockedRitualIds = new List<string>();
    private string _currentRitualId = "";
    private int _dailyRitualsRemaining = 3;
    private int _totalPerformed = 0;
    private int _totalGoldSpent = 0;
    private int _totalReputation = 0;

    // ===== 生命周期 =====

    public override void _Ready()
    {
        SetupUI();
        RequestRefresh();
    }

    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsAndOffsetsPreset(Control.LayoutPreset.Center);
        _mainContainer.CustomMinimumSize = new Vector2(700, 500);
        AddChild(_mainContainer);

        // Header
        _headerContainer = new HBoxContainer();
        _headerContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _mainContainer.AddChild(_headerContainer);

        _titleLabel = new Label();
        _titleLabel.Text = "✨ Daily Rituals ✨";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _headerContainer.AddChild(_titleLabel);

        _headerContainer.AddChild(new Control { CustomMinimumSize = new Vector2(50, 0) });

        _dailyCountLabel = new Label();
        _dailyCountLabel.Text = "Rituals Today: 3/3";
        _dailyCountLabel.AddThemeFontSizeOverride("font_size", 16);
        _headerContainer.AddChild(_dailyCountLabel);

        // Separator
        var hs = new HSeparator();
        _mainContainer.AddChild(hs);

        // Ritual Grid
        _ritualGrid = new GridContainer();
        _ritualGrid.Columns = 3;
        _ritualGrid.CustomMinimumSize = new Vector2(650, 300);
        _ritualGrid.AddThemeConstantOverride("separation", 10);
        _mainContainer.AddChild(_ritualGrid);

        // Stats section
        var statsContainer = new HBoxContainer();
        statsContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _mainContainer.AddChild(statsContainer);

        _statsLabel = new Label();
        _statsLabel.Text = "Total Performed: 0 | Gold Spent: 0 | Reputation: 0";
        _statsLabel.AddThemeFontSizeOverride("font_size", 14);
        statsContainer.AddChild(_statsLabel);

        // Bottom buttons
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        _mainContainer.AddChild(buttonContainer);

        var clearButton = new Button();
        clearButton.Text = "Clear Bonuses";
        clearButton.Pressed += OnClearBonusesPressed;
        buttonContainer.AddChild(clearButton);

        buttonContainer.AddChild(new Control { CustomMinimumSize = new Vector2(20, 0) });

        _closeButton = new Button();
        _closeButton.Text = "Close";
        _closeButton.Pressed += OnClosePressed;
        buttonContainer.AddChild(_closeButton);

        // Add styles
        AddStyles();
    }

    private void AddStyles()
    {
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);

        foreach (Node child in _mainContainer.GetChildren())
        {
            if (child is Panel panel)
            {
                panel.AddThemeStyleboxOverride("panel", style);
            }
        }
    }

    // ===== 公开更新接口（由外部/System 调用） =====
    // REQ-075 解耦：UI 不再主动拉取数据，而是等待外部推送

    /// <summary>
    /// 更新仪式列表显示（由 System 调用）
    /// </summary>
    public void UpdateRitualList(List<RitualData> allRituals, List<string> unlockedRitualIds, string currentRitualId)
    {
        _allRituals = allRituals ?? new List<RitualData>();
        _unlockedRitualIds = unlockedRitualIds ?? new List<string>();
        _currentRitualId = currentRitualId ?? "";
        RefreshRitualList();
    }

    /// <summary>
    /// 更新统计数据（由 System 调用）
    /// </summary>
    public void UpdateStats(int totalPerformed, int totalGoldSpent, int totalReputation)
    {
        _totalPerformed = totalPerformed;
        _totalGoldSpent = totalGoldSpent;
        _totalReputation = totalReputation;
        _statsLabel.Text = $"Total Performed: {totalPerformed} | Gold Spent: {totalGoldSpent} | Reputation: {totalReputation}";
    }

    /// <summary>
    /// 更新剩余次数（由 System 调用）
    /// </summary>
    public void UpdateDailyCount(int remaining)
    {
        _dailyRitualsRemaining = remaining;
        _dailyCountLabel.Text = $"Rituals Today: {remaining}/3";
        RefreshRitualList();
    }

    /// <summary>
    /// 通知仪式已开始（由 System 调用）
    /// </summary>
    public void NotifyRitualStarted(string ritualId)
    {
        _currentRitualId = ritualId;
        RefreshRitualList();
    }

    /// <summary>
    /// 通知仪式已完成（由 System 调用）
    /// </summary>
    public void NotifyRitualCompleted(string ritualId)
    {
        _currentRitualId = "";
        RefreshRitualList();
    }

    /// <summary>
    /// 通知仪式已解锁（由 System 调用）
    /// </summary>
    public void NotifyRitualUnlocked(string ritualId)
    {
        if (!_unlockedRitualIds.Contains(ritualId))
            _unlockedRitualIds.Add(ritualId);
        RefreshRitualList();
    }

    // ===== 内部方法 =====

    private void RequestRefresh()
    {
        // 通过事件请求外部提供数据，而不是直接调用 System
        OnRefreshRequested?.Invoke();
    }

    private void RefreshRitualList()
    {
        // Clear existing items
        foreach (Node child in _ritualGrid.GetChildren())
            child.QueueFree();
        _displayedRituals.Clear();

        // Filter and display rituals
        foreach (var ritual in _allRituals)
        {
            // Show novice always, others only if unlocked
            if (ritual.Tier == RitualTier.Novice || _unlockedRitualIds.Contains(ritual.Id))
            {
                _displayedRituals.Add(ritual);
                CreateRitualCard(ritual);
            }
        }
    }

    private void CreateRitualCard(RitualData ritual)
    {
        var cardContainer = new VBoxContainer();
        cardContainer.CustomMinimumSize = new Vector2(200, 180);
        cardContainer.AddThemeConstantOverride("separation", 5);
        _ritualGrid.AddChild(cardContainer);

        // Card background
        var cardPanel = new Panel();
        cardPanel.CustomMinimumSize = new Vector2(200, 180);

        // Color by tier
        var tierColor = GetTierColor(ritual.Tier);
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = new Color(0.15f, 0.15f, 0.2f);
        cardStyle.BorderColor = tierColor;
        cardStyle.SetBorderWidthAll(2);
        cardStyle.SetCornerRadiusAll(6);
        cardPanel.AddThemeStyleboxOverride("panel", cardStyle);
        cardContainer.AddChild(cardPanel);

        // Name
        var nameLabel = new Label();
        nameLabel.Text = ritual.Name;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        cardContainer.AddChild(nameLabel);

        // Tier
        var tierLabel = new Label();
        tierLabel.Text = $"[{ritual.Tier}]";
        tierLabel.AddThemeColorOverride("font_color", tierColor);
        tierLabel.HorizontalAlignment = HorizontalAlignment.Center;
        tierLabel.AddThemeFontSizeOverride("font_size", 12);
        cardContainer.AddChild(tierLabel);

        // Description
        var descLabel = new Label();
        descLabel.Text = ritual.Description;
        descLabel.HorizontalAlignment = HorizontalAlignment.Center;
        descLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        descLabel.CustomMinimumSize = new Vector2(180, 40);
        cardContainer.AddChild(descLabel);

        // Cost and duration
        var infoLabel = new Label();
        infoLabel.Text = $"💰 {ritual.GoldCost} | ⏱️ {ritual.Duration / 60:F0}min";
        infoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        infoLabel.AddThemeFontSizeOverride("font_size", 12);
        cardContainer.AddChild(infoLabel);

        // Bonuses
        var bonusText = "";
        foreach (var bonus in ritual.AttributeBonuses)
        {
            bonusText += $"{bonus.Key}: +{bonus.Value * 100:F0}% ";
        }
        var bonusLabel = new Label();
        bonusLabel.Text = bonusText;
        bonusLabel.HorizontalAlignment = HorizontalAlignment.Center;
        bonusLabel.AddThemeFontSizeOverride("font_size", 11);
        bonusLabel.Modulate = new Color(0.7f, 0.9f, 0.7f);
        cardContainer.AddChild(bonusLabel);

        // Reputation
        var repLabel = new Label();
        repLabel.Text = $"⭐ +{ritual.ReputationGain} Rep";
        repLabel.HorizontalAlignment = HorizontalAlignment.Center;
        repLabel.AddThemeFontSizeOverride("font_size", 11);
        cardContainer.AddChild(repLabel);

        // Start button
        var startButton = new Button();
        startButton.Text = "Perform Ritual";
        startButton.CustomMinimumSize = new Vector2(180, 30);

        // Disable if active or no daily rituals remaining
        bool isCurrentlyActive = _currentRitualId == ritual.Id;
        bool canPerform = !isCurrentlyActive && _dailyRitualsRemaining > 0;
        startButton.Disabled = !canPerform;

        startButton.Pressed += () => OnStartRitualPressed(ritual);
        cardContainer.AddChild(startButton);

        // Show current progress if this is the active ritual
        if (isCurrentlyActive)
        {
            var progressLabel = new Label();
            progressLabel.Text = $"🔮 In Progress...";
            progressLabel.HorizontalAlignment = HorizontalAlignment.Center;
            progressLabel.Modulate = new Color(0.3f, 0.8f, 1f);
            cardContainer.AddChild(progressLabel);

            var progressBar = new ProgressBar();
            progressBar.CustomMinimumSize = new Vector2(180, 20);
            progressBar.ShowPercentage = false;

            // Calculate progress (this would be passed in via UpdateRitualList ideally)
            progressBar.Value = 50; // Placeholder - actual progress should come from System

            var progressStyle = new StyleBoxFlat();
            progressStyle.BgColor = new Color(0.2f, 0.5f, 0.8f);
            progressBar.AddThemeStyleboxOverride("fill", progressStyle);
            cardContainer.AddChild(progressBar);
        }
    }

    private Color GetTierColor(RitualTier tier)
    {
        return tier switch
        {
            RitualTier.Novice => new Color(0.7f, 0.7f, 0.7f),
            RitualTier.Adept => new Color(0.3f, 0.7f, 0.3f),
            RitualTier.Master => new Color(0.5f, 0.5f, 1f),
            RitualTier.Legendary => new Color(1f, 0.7f, 0f),
            _ => new Color(1f, 1f, 1f)
        };
    }

    // ===== 事件处理 =====

    private void OnStartRitualPressed(RitualData ritual)
    {
        // 通过事件请求 System 处理，而不是直接调用 System.Instance
        OnStartRitualRequested?.Invoke(ritual.Id);
    }

    private void OnClearBonusesPressed()
    {
        // 通过事件请求 System 处理
        OnClearBonusesRequested?.Invoke();
    }

    private void OnClosePressed()
    {
        Hide();
        QueueFree();
    }

    // Toggle UI with key press
    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey key && key.Pressed && key.Keycode == Key.R)
        {
            if (IsVisibleInTree())
            {
                OnClosePressed();
            }
        }
    }
}
