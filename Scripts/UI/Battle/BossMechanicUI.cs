using System;
using System.Collections.Generic;
using Godot;
using Godot.Collections;

public partial class BossMechanicUI : Control
{
    private Control _container;
    private Label _titleLabel;
    private TabContainer _tabContainer;
    
    // Boss 列表标签页
    private Control _bossListTab;
    private VBoxContainer _bossListContainer;
    
    // 统计标签页
    private Control _statsTab;
    private VBoxContainer _statsContainer;
    private Label _totalDefeatedLabel;
    private Label _totalBattlesLabel;
    private Label _winRateLabel;
    private Label _avgTimeLabel;
    private Label _winStreakLabel;
    
    // 当前战斗标签页
    private Control _battleTab;
    private Label _currentBossLabel;
    private ProgressBar _healthBar;
    private Label _healthLabel;
    private Label _phaseLabel;
    private Label _timerLabel;
    private Label _damageLabel;
    private Label _ratingLabel;
    
    private bool _isVisible = false; 
    private float _battleTimer = 0;

    public override void _Ready()
    {
        _Ready();
        SetupUI();
        ConnectSignals();
    }

    private void SetupUI()
    {
        // 主容器
        _container = new Control();
        _container.Name = "BossMechanicContainer";
        _container.SetAnchorsPreset(Control.LayoutPreset.Center);
        _container.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_container);
        _container.Visible = false; 

        // 背景面板
        Panel background = new Panel();
        background.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.SetCornerRadiusAll(8);
        style.SetBorderWidthAll(2);
        style.BorderColor = new Color(0.8f, 0.6f, 0.2f, 1f);
        background.AddThemeStyleboxOverride("panel", style);
        _container.AddChild(background);

        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "Boss 战斗系统";
        _titleLabel.SetAnchorsPreset(Control.LayoutPreset.TopWide);
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
        _titleLabel.Position = new Vector2(0, 10);
        _container.AddChild(_titleLabel);

        // 关闭按钮
        Button closeBtn = new Button();
        closeBtn.Text = "×";
        closeBtn.Position = new Vector2(760, 10);
        closeBtn.Size = new Vector2(30, 30);
        closeBtn.Pressed += () => ToggleUI();
        _container.AddChild(closeBtn);

        // 标签页容器
        _tabContainer = new TabContainer();
        _tabContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _tabContainer.Position = new Vector2(20, 60);
        _tabContainer.Size = new Vector2(760, 520);
        _container.AddChild(_tabContainer);

        // === Boss 列表标签页 ===
        _bossListTab = new Control();
        _bossListTab.Name = "Boss列表";
        _tabContainer.AddChild(_bossListTab);

        _bossListContainer = new VBoxContainer();
        _bossListContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _bossListContainer.AddThemeConstantOverride("separation", 10);
        _bossListTab.AddChild(_bossListContainer);

        RefreshBossList();

        // === 战斗统计标签页 ===
        _statsTab = new Control();
        _statsTab.Name = "战斗统计";
        _tabContainer.AddChild(_statsTab);

        _statsContainer = new VBoxContainer();
        _statsContainer.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        _statsContainer.AddThemeConstantOverride("separation", 15);
        _statsContainer.Position = new Vector2(20, 20);
        _statsContainer.Size = new Vector2(720, 480);
        _statsTab.AddChild(_statsContainer);

        // 统计标题
        Label statsTitle = new Label();
        statsTitle.Text = "玩家 Boss 战斗统计";
        statsTitle.AddThemeFontSizeOverride("font_size", 22);
        statsTitle.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
        _statsContainer.AddChild(statsTitle);

        // 详细统计
        CreateStatLabel("总击败 Boss 数: ", out _totalDefeatedLabel);
        CreateStatLabel("总战斗次数: ", out _totalBattlesLabel);
        CreateStatLabel("胜率: ", out _winRateLabel);
        CreateStatLabel("平均战斗时间: ", out _avgTimeLabel);
        CreateStatLabel("当前连胜: ", out _winStreakLabel);

        RefreshStats();

        // === 当前战斗标签页 ===
        _battleTab = new Control();
        _battleTab.Name = "当前战斗";
        _tabContainer.AddChild(_battleTab);

        SetupBattleTab();

        // 应用 Tween 动画
        ApplyTweenAnimations();
    }

    private void CreateStatLabel(string prefix, out Label label)
    {
        HBoxContainer hbox = new HBoxContainer();
        _statsContainer.AddChild(hbox);

        Label prefixLabel = new Label();
        prefixLabel.Text = prefix;
        prefixLabel.AddThemeFontSizeOverride("font_size", 18);
        hbox.AddChild(prefixLabel);

        label = new Label();
        label.Text = "0";
        label.AddThemeFontSizeOverride("font_size", 18);
        label.AddThemeColorOverride("font_color", new Color(0.2f, 1f, 0.2f, 1f));
        hbox.AddChild(label);
    }

    private void SetupBattleTab()
    {
        VBoxContainer vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 20);
        vbox.Position = new Vector2(20, 20);
        vbox.Size = new Vector2(720, 480);
        _battleTab.AddChild(vbox);

        // 当前 Boss 名称
        Label bossTitle = new Label();
        bossTitle.Text = "当前 Boss: 无";
        bossTitle.AddThemeFontSizeOverride("font_size", 24);
        bossTitle.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0.4f, 1f));
        _currentBossLabel = bossTitle;
        vbox.AddChild(bossTitle);

        // Boss 血条
        _healthBar = new ProgressBar();
        _healthBar.CustomMinimumSize = new Vector2(0, 30);
        _healthBar.Value = 100;
        _healthBar.ShowPercentage = false; 
        
        var healthStyle = new StyleBoxFlat();
        healthStyle.BgColor = new Color(0.3f, 0.1f, 0.1f, 1f);
        healthStyle.SetCornerRadiusAll(4);
        _healthBar.AddThemeStyleboxOverride("background", healthStyle);
        
        var fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = new Color(1f, 0.2f, 0.2f, 1f);
        fillStyle.SetCornerRadiusAll(4);
        _healthBar.AddThemeStyleboxOverride("fill", fillStyle);
        
        vbox.AddChild(_healthBar);

        // 血量标签
        _healthLabel = new Label();
        _healthLabel.Text = "HP: 0 / 0";
        _healthLabel.HorizontalAlignment = HorizontalAlignment.Center;
        vbox.AddChild(_healthLabel);

        // 当前阶段
        _phaseLabel = new Label();
        _phaseLabel.Text = "当前阶段: -";
        _phaseLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(_phaseLabel);

        // 战斗计时器
        _timerLabel = new Label();
        _timerLabel.Text = "战斗时间: 00:00";
        _timerLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(_timerLabel);

        // 伤害统计
        _damageLabel = new Label();
        _damageLabel.Text = "总伤害: 0";
        _damageLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(_damageLabel);

        // 战斗评价
        _ratingLabel = new Label();
        _ratingLabel.Text = "当前评价: -";
        _ratingLabel.AddThemeFontSizeOverride("font_size", 20);
        _ratingLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
        vbox.AddChild(_ratingLabel);
    }

    private void ApplyTweenAnimations()
    {
        var tween = CreateTween();
        tween.SetParallel(true);
        
        tween.TweenProperty(_container, "modulate:a", 0f, 0f);
        tween.TweenProperty(_container, "scale", new Vector2(0.9f, 0.9f), 0f);
        
        _container.Modulate = new Color(1, 1, 1, 0);
        _container.Scale = new Vector2(0.9f, 0.9f);
    }

    private void ConnectSignals()
    {
        var bossSystem = BossMechanicSystem.Instance;
        if (bossSystem != null)
        {
            bossSystem.OnBattleStarted += OnBattleStarted;
            bossSystem.OnPhaseChanged += OnPhaseChanged;
            bossSystem.OnHealthChanged += OnHealthChanged;
            bossSystem.OnBattleEnded += OnBattleEnded;
        }
    }

    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        _container.Visible = _isVisible;

        if (_isVisible)
        {
            // 打开动画
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.SetTrans(Tween.TransitionType.Back);
            tween.SetEasing(Tween.EasingFunction.EaseOut);
            
            tween.TweenProperty(_container, "modulate:a", 1f, 0.3f);
            tween.TweenProperty(_container, "scale", new Vector2(1f, 1f), 0.3f);
            
            RefreshBossList();
            RefreshStats();
            RefreshBattleStatus();
        }
        else
        {
            // 关闭动画
            var tween = CreateTween();
            tween.SetParallel(true);
            tween.SetTrans(Tween.TransitionType.Back);
            tween.SetEasing(Tween.EasingFunction.EaseIn);
            
            tween.TweenProperty(_container, "modulate:a", 0f, 0.2f);
            tween.TweenProperty(_container, "scale", new Vector2(0.9f, 0.9f), 0.2f);
            
            tween.TweenCallback(() => _container.Visible = false);
        }
    }

    private void RefreshBossList()
    {
        // 清除旧内容
        foreach (Node child in _bossListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var bosses = BossMechanicDatabase.Instance.GetAllBosses();
        
        // 表头
        HBoxContainer header = new HBoxContainer();
        _bossListContainer.AddChild(header);
        
        AddHeaderLabel(header, "Boss名称", 150);
        AddHeaderLabel(header, "区域", 100);
        AddHeaderLabel(header, "等级", 60);
        AddHeaderLabel(header, "生命值", 100);
        AddHeaderLabel(header, "状态", 100);

        // Boss 列表
        foreach (var boss in bosses)
        {
            HBoxContainer row = new HBoxContainer();
            _bossListContainer.AddChild(row);

            var stats = BossMechanicSystem.Instance.GetBossStats(boss.BossId);
            bool defeated = stats != null && stats.Victories > 0;

            AddValueLabel(row, boss.BossName, 150, defeated ? new Color(0.2f, 1f, 0.2f) : new Color(0.8f, 0.8f, 0.8f));
            AddValueLabel(row, boss.RegionId, 100);
            AddValueLabel(row, boss.RecommendedLevel.ToString(), 60);
            AddValueLabel(row, boss.BaseHealth.ToString("N0"), 100);
            
            string status = defeated ? "已击败" : "未挑战";
            AddValueLabel(row, status, 100, defeated ? new Color(0.2f, 1f, 0.2f) : new Color(1f, 0.6f, 0.2f));
        }
    }

    private void AddHeaderLabel(HBoxContainer parent, string text, float width)
    {
        Label label = new Label();
        label.Text = text;
        label.CustomMinimumSize = new Vector2(width, 0);
        label.AddThemeFontSizeOverride("font_size", 16);
        label.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f, 1f));
        parent.AddChild(label);
    }

    private void AddValueLabel(HBoxContainer parent, string text, float width, Color? color = null)
    {
        Label label = new Label();
        label.Text = text;
        label.CustomMinimumSize = new Vector2(width, 0);
        label.AddThemeFontSizeOverride("font_size", 14);
        if (color.HasValue)
            label.AddThemeColorOverride("font_color", color.Value);
        parent.AddChild(label);
    }

    private void RefreshStats()
    {
        var stats = BossMechanicSystem.Instance.GetPlayerStats();
        
        _totalDefeatedLabel.Text = stats.TotalBossesDefeated.ToString();
        _totalBattlesLabel.Text = stats.TotalBossBattles.ToString();
        
        float winRate = stats.TotalBossBattles > 0 ? 
            (float)stats.TotalBossesDefeated / stats.TotalBossBattles * 100 : 0;
        _winRateLabel.Text = $"{winRate:F1}%";
        
        float avgTime = BossMechanicSystem.Instance.GetAverageBattleTime();
        _avgTimeLabel.Text = $"{avgTime:F1}秒";
        
        _winStreakLabel.Text = stats.ConsecutiveWins.ToString();
    }

    private void RefreshBattleStatus()
    {
        if (!BossMechanicSystem.Instance.IsInBossBattle)
        {
            _currentBossLabel.Text = "当前 Boss: 无";
            _healthBar.Value = 0;
            _healthLabel.Text = "HP: 0 / 0";
            _phaseLabel.Text = "当前阶段: -";
            _timerLabel.Text = "战斗时间: 00:00";
            _damageLabel.Text = "总伤害: 0";
            _ratingLabel.Text = "当前评价: -";
            return;
        }

        var system = BossMechanicSystem.Instance;
        var config = BossMechanicDatabase.Instance.GetBossConfig(system.CurrentBossId);
        
        if (config != null)
        {
            _currentBossLabel.Text = $"当前 Boss: {config.BossName}";
            
            float healthPercent = system.CurrentBossHealth / system.MaxBossHealth * 100;
            _healthBar.Value = healthPercent;
            _healthLabel.Text = $"HP: {system.CurrentBossHealth:N0} / {system.MaxBossHealth:N0}";
            
            var phase = system.GetCurrentPhase();
            _phaseLabel.Text = $"当前阶段: {phase?.PhaseName ?? "未知"}";
        }

        _timerLabel.Text = $"战斗时间: {FormatTime(_battleTimer)}";
        _ratingLabel.Text = $"当前评价: {system.CalculateBattleRating()}";
    }

    private string FormatTime(float seconds)
    {
        int mins = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{mins:D2}:{secs:D2}";
    }

    #region 信号处理

    private void OnBattleStarted(string bossId, int phaseIndex)
    {
        var config = BossMechanicDatabase.Instance.GetBossConfig(bossId);
        if (config != null)
        {
            _currentBossLabel.Text = $"当前 Boss: {config.BossName}";
            _phaseLabel.Text = $"当前阶段: {config.Phases[phaseIndex].PhaseName}";
            _healthBar.Value = 100;
            _healthLabel.Text = $"HP: {config.BaseHealth:N0} / {config.BaseHealth:N0}";
            _battleTimer = 0;
        }
    }

    private void OnPhaseChanged(string bossId, int phaseIndex)
    {
        var config = BossMechanicDatabase.Instance.GetBossConfig(bossId);
        if (config != null && phaseIndex < config.Phases.Count)
        {
            _phaseLabel.Text = $"当前阶段: {config.Phases[phaseIndex].PhaseName}";
        }
    }

    private void OnHealthChanged(string bossId, float healthPercent)
    {
        _healthBar.Value = healthPercent * 100;
    }

    private void OnBattleEnded(float battleTime)
    {
        RefreshBossList();
        RefreshStats();
    }

    #endregion

    public override void _Process(double delta)
    {
        if (BossMechanicSystem.Instance.IsInBossBattle)
        {
            _battleTimer += (float)delta;
            _timerLabel.Text = $"战斗时间: {FormatTime(_battleTimer)}";
            
            var system = BossMechanicSystem.Instance;
            float healthPercent = system.CurrentBossHealth / system.MaxBossHealth * 100;
            _healthBar.Value = healthPercent;
            _healthLabel.Text = $"HP: {system.CurrentBossHealth:N0} / {system.MaxBossHealth:N0}";
            
            _ratingLabel.Text = $"当前评价: {system.CalculateBattleRating()}";
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.B)
            {
                ToggleUI();
            }
        }
    }
}
