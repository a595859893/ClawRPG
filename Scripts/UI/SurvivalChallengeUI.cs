using Godot;
using System;
using System.Collections.Generic;

public class SurvivalChallengeUI : Control
{
    private SurvivalChallengeSystem _system;
    
    // 主面板
    private PanelContainer _mainPanel;
    private VBoxContainer _mainContainer;
    
    // 标签页
    private TabContainer _tabContainer;
    private VBoxContainer _challengeListTab;
    private VBoxContainer _statisticsTab;
    private VBoxContainer _activeTab;
    
    // 挑战列表
    private ScrollContainer _challengeScroll;
    private VBoxContainer _challengeListContainer;
    
    // 统计显示
    private Label _totalKillsLabel;
    private Label _totalGoldLabel;
    private Label _completionsLabel;
    private Label _bestWaveLabel;
    private Label _bestScoreLabel;
    
    // 当前挑战显示
    private Label _currentChallengeLabel;
    private Label _currentWaveLabel;
    private Label _enemiesKilledLabel;
    private Label _elapsedTimeLabel;
    private Label _scoreLabel;
    private ProgressBar _timeProgressBar;
    
    // 开关状态
    private bool _isVisible = false;
    private KeyToggleHandler _toggleHandler;
    
    public override void _Ready()
    {
        _system = SurvivalChallengeSystem.Instance;
        if (_system == null)
        {
            _system = new SurvivalChallengeSystem();
            _system.Initialize();
        }
        
        SetupUI();
        ConnectSignals();
        Hide();
        
        GD.Print("生存挑战UI已初始化");
    }
    
    private void SetupUI()
    {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(500, 450);
        AddChild(_mainPanel);
        
        // 样式
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.BorderColor = new Color(0.4f, 0.3f, 0.2f);
        style.SetBorderWidthAll(3);
        style.SetCornerRadiusAll(12);
        _mainPanel.AddThemeStyleboxOverride("panel", style);
        
        // 主容器
        _mainContainer = new VBoxContainer();
        _mainContainer.AddThemeConstantOverride("separation", 8);
        _mainPanel.AddChild(_mainContainer);
        
        // 标题
        var titleLabel = new Label();
        titleLabel.Text = "🎯 生存挑战";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainContainer.AddChild(titleLabel);
        
        // 标签页容器
        _tabContainer = new TabContainer();
        _tabContainer.CustomMinimumSize = new Vector2(480, 380);
        _mainContainer.AddChild(_tabContainer);
        
        // 创建标签页
        SetupChallengeListTab();
        SetupStatisticsTab();
        SetupActiveTab();
        
        // 快捷键提示
        var hintLabel = new Label();
        hintLabel.Text = "按 X 键关闭";
        hintLabel.HorizontalAlignment = HorizontalAlignment.Center;
        hintLabel.AddThemeFontSizeOverride("font_size", 12);
        hintLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
        _mainContainer.AddChild(hintLabel);
    }
    
    private void SetupChallengeListTab()
    {
        _challengeListTab = new VBoxContainer();
        _challengeListTab.Name = "挑战列表";
        _tabContainer.AddChild(_challengeListTab);
        
        // 说明
        var infoLabel = new Label();
        infoLabel.Text = "选择挑战开始";
        infoLabel.AddThemeFontSizeOverride("font_size", 14);
        _challengeListTab.AddChild(infoLabel);
        
        // 滚动容器
        _challengeScroll = new ScrollContainer();
        _challengeScroll.CustomMinimumSize = new Vector2(460, 320);
        _challengeListTab.AddChild(_challengeScroll);
        
        // 挑战列表
        _challengeListContainer = new VBoxContainer();
        _challengeListContainer.AddThemeConstantOverride("separation", 8);
        _challengeScroll.AddChild(_challengeListContainer);
        
        // 加载挑战列表
        LoadChallengeList();
    }
    
    private void LoadChallengeList()
    {
        var challenges = SurvivalChallengeDatabase.GetAllChallenges();
        
        foreach (var challenge in challenges)
        {
            var challengePanel = CreateChallengePanel(challenge);
            _challengeListContainer.AddChild(challengePanel);
        }
    }
    
    private Control CreateChallengePanel(SurvivalChallengeData.ChallengeConfig config)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(440, 80);
        
        // 样式
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.15f, 0.2f);
        style.BorderColor = SurvivalChallengeDatabase.GetDifficultyColor(config.Difficulty);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(8);
        panel.AddThemeStyleboxOverride("panel", style);
        
        var container = new VBoxContainer();
        container.AddThemeConstantOverride("separation", 4);
        panel.AddChild(container);
        
        // 标题行
        var titleRow = new HBoxContainer();
        container.AddChild(titleRow);
        
        var nameLabel = new Label();
        nameLabel.Text = config.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        titleRow.AddChild(nameLabel);
        
        titleRow.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        var difficultyLabel = new Label();
        difficultyLabel.Text = SurvivalChallengeDatabase.GetDifficultyName(config.Difficulty);
        difficultyLabel.Modulate = SurvivalChallengeDatabase.GetDifficultyColor(config.Difficulty);
        difficultyLabel.AddThemeFontSizeOverride("font_size", 14);
        titleRow.AddChild(difficultyLabel);
        
        // 描述
        var descLabel = new Label();
        descLabel.Text = config.Description;
        descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        container.AddChild(descLabel);
        
        // 信息行
        var infoRow = new HBoxContainer();
        container.AddChild(infoRow);
        
        var typeLabel = new Label();
        typeLabel.Text = $"类型: {SurvivalChallengeDatabase.GetTypeName(config.Type)}";
        typeLabel.AddThemeFontSizeOverride("font_size", 12);
        infoRow.AddChild(typeLabel);
        
        infoRow.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        var levelLabel = new Label();
        levelLabel.Text = $"推荐等级: {config.RecommendedLevel}";
        levelLabel.AddThemeFontSizeOverride("font_size", 12);
        infoRow.AddChild(levelLabel);
        
        var rewardLabel = new Label();
        rewardLabel.Text = $"奖励: {config.BaseGoldReward}金 / {config.BaseExpReward}经验";
        rewardLabel.Modulate = new Color(0.9f, 0.8f, 0.3f);
        rewardLabel.AddThemeFontSizeOverride("font_size", 12);
        container.AddChild(rewardLabel);
        
        // 开始按钮
        var button = new Button();
        button.Text = "开始挑战";
        button.CustomMinimumSize = new Vector2(100, 30);
        button.Pressed += () => OnStartChallenge(config.Id);
        container.AddChild(button);
        
        return panel;
    }
    
    private void OnStartChallenge(string configId)
    {
        if (_system.StartChallenge(configId))
        {
            _tabContainer.CurrentTab = 2; // 切换到活跃挑战标签页
            UpdateActiveChallenge();
        }
    }
    
    private void SetupStatisticsTab()
    {
        _statisticsTab = new VBoxContainer();
        _statisticsTab.Name = "统计";
        _tabContainer.AddChild(_statisticsTab);
        
        var titleLabel = new Label();
        titleLabel.Text = "个人统计";
        titleLabel.AddThemeFontSizeOverride("font_size", 18);
        _statisticsTab.AddChild(titleLabel);
        
        _statisticsTab.AddChild(new HSeparator());
        
        // 总击杀
        var killsContainer = new HBoxContainer();
        _statisticsTab.AddChild(killsContainer);
        killsContainer.AddChild(new Label() { Text = "总击杀数: ", SizeFlagsHorizontal = Control.SizeFlags.Expand });
        _totalKillsLabel = new Label() { Text = "0" };
        killsContainer.AddChild(_totalKillsLabel);
        
        // 总金币
        var goldContainer = new HBoxContainer();
        _statisticsTab.AddChild(goldContainer);
        goldContainer.AddChild(new Label() { Text = "总获得金币: ", SizeFlagsHorizontal = Control.SizeFlags.Expand });
        _totalGoldLabel = new Label() { Text = "0" };
        _totalGoldLabel.Modulate = new Color(0.9f, 0.8f, 0.3f);
        goldContainer.AddChild(_totalGoldLabel);
        
        // 完成次数
        var completionContainer = new HBoxContainer();
        _statisticsTab.AddChild(completionContainer);
        completionContainer.AddChild(new Label() { Text = "完成次数: ", SizeFlagsHorizontal = Control.SizeFlags.Expand });
        _completionsLabel = new Label() { Text = "0" };
        completionContainer.AddChild(_completionsLabel);
        
        _statisticsTab.AddChild(new HSeparator());
        
        // 最佳波次
        var waveContainer = new HBoxContainer();
        _statisticsTab.AddChild(waveContainer);
        waveContainer.AddChild(new Label() { Text = "最佳波次: ", SizeFlagsHorizontal = Control.SizeFlags.Expand });
        _bestWaveLabel = new Label() { Text = "0" };
        _bestWaveLabel.Modulate = new Color(0.3f, 0.8f, 0.3f);
        waveContainer.AddChild(_bestWaveLabel);
        
        // 最高分
        var scoreContainer = new HBoxContainer();
        _statisticsTab.AddChild(scoreContainer);
        scoreContainer.AddChild(new Label() { Text = "最高分: ", SizeFlagsHorizontal = Control.SizeFlags.Expand });
        _bestScoreLabel = new Label() { Text = "0" };
        _bestScoreLabel.Modulate = new Color(0.3f, 0.8f, 0.9f);
        scoreContainer.AddChild(_bestScoreLabel);
        
        UpdateStatistics();
    }
    
    private void SetupActiveTab()
    {
        _activeTab = new VBoxContainer();
        _activeTab.Name = "当前挑战";
        _tabContainer.AddChild(_activeTab);
        
        _currentChallengeLabel = new Label();
        _currentChallengeLabel.Text = "无进行中的挑战";
        _currentChallengeLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _currentChallengeLabel.AddThemeFontSizeOverride("font_size", 20);
        _activeTab.AddChild(_currentChallengeLabel);
        
        _activeTab.AddChild(new HSeparator());
        
        // 波次
        var waveContainer = new HBoxContainer();
        _activeTab.AddChild(waveContainer);
        waveContainer.AddChild(new Label() { Text = "当前波次: ", SizeFlagsHorizontal = Control.SizeFlags.Expand });
        _currentWaveLabel = new Label() { Text = "0" };
        _currentWaveLabel.Modulate = new Color(0.3f, 0.6f, 0.9f);
        waveContainer.AddChild(_currentWaveLabel);
        
        // 击杀数
        var killContainer = new HBoxContainer();
        _activeTab.AddChild(killContainer);
        killContainer.AddChild(new Label() { Text = "击杀数: ", SizeFlagsHorizontal = Control.SizeFlags.Expand });
        _enemiesKilledLabel = new Label() { Text = "0" };
        _enemiesKilledLabel.Modulate = new Color(0.9f, 0.3f, 0.3f);
        killContainer.AddChild(_enemiesKilledLabel);
        
        // 时间
        var timeContainer = new HBoxContainer();
        _activeTab.AddChild(timeContainer);
        timeContainer.AddChild(new Label() { Text = "已用时间: ", SizeFlagsHorizontal = Control.SizeFlags.Expand });
        _elapsedTimeLabel = new Label() { Text = "0:00" };
        timeContainer.AddChild(_elapsedTimeLabel);
        
        // 时间进度条
        _timeProgressBar = new ProgressBar();
        _timeProgressBar.CustomMinimumSize = new Vector2(0, 20);
        _activeTab.AddChild(_timeProgressBar);
        
        _activeTab.AddChild(new HSeparator());
        
        // 得分
        var scoreContainer = new HBoxContainer();
        _activeTab.AddChild(scoreContainer);
        scoreContainer.AddChild(new Label() { Text = "当前得分: ", SizeFlagsHorizontal = Control.SizeFlags.Expand });
        _scoreLabel = new Label() { Text = "0" };
        _scoreLabel.Modulate = new Color(0.9f, 0.8f, 0.3f);
        _scoreLabel.AddThemeFontSizeOverride("font_size", 24);
        scoreContainer.AddChild(_scoreLabel);
        
        // 放弃按钮
        var abandonButton = new Button();
        abandonButton.Text = "放弃挑战";
        abandonButton.CustomMinimumSize = new Vector2(200, 40);
        abandonButton.Pressed += OnAbandonChallenge;
        _activeTab.AddChild(abandonButton);
    }
    
    private void UpdateStatistics()
    {
        var stats = _system.GetStatistics();
        
        _totalKillsLabel.Text = stats["total_kills"].ToString();
        _totalGoldLabel.Text = stats["total_gold"].ToString();
        _completionsLabel.Text = stats["total_completions"].ToString();
        _bestWaveLabel.Text = stats["best_wave"].ToString();
        _bestScoreLabel.Text = stats["best_score"].ToString();
    }
    
    private void UpdateActiveChallenge()
    {
        var challenge = _system.CurrentChallenge;
        
        if (challenge == null)
        {
            _currentChallengeLabel.Text = "无进行中的挑战";
            _currentWaveLabel.Text = "0";
            _enemiesKilledLabel.Text = "0";
            _elapsedTimeLabel.Text = "0:00";
            _scoreLabel.Text = "0";
            _timeProgressBar.Value = 0;
            return;
        }
        
        var config = SurvivalChallengeDatabase.GetChallenge(challenge.ConfigId);
        if (config == null) return;
        
        _currentChallengeLabel.Text = config.Name;
        _currentWaveLabel.Text = challenge.CurrentWave.ToString();
        _enemiesKilledLabel.Text = challenge.EnemiesKilled.ToString();
        
        int minutes = (int)challenge.ElapsedTime / 60;
        int seconds = (int)challenge.ElapsedTime % 60;
        _elapsedTimeLabel.Text = $"{minutes}:{seconds:D2}";
        
        _scoreLabel.Text = challenge.Score.ToString();
        
        // 时间进度条
        if (config.TimeLimit > 0)
        {
            _timeProgressBar.MaxValue = config.TimeLimit;
            _timeProgressBar.Value = challenge.ElapsedTime;
        }
        else
        {
            _timeProgressBar.MaxValue = 100;
            _timeProgressBar.Value = 0;
        }
    }
    
    private void OnAbandonChallenge()
    {
        _system.AbandonChallenge();
    }
    
    private void ConnectSignals()
    {
        // 连接系统信号
    }
    
    public override void _Process(float delta)
    {
        if (_system.IsChallengeActive)
        {
            UpdateActiveChallenge();
        }
    }
    
    public void Toggle()
    {
        if (_isVisible)
        {
            Hide();
            _isVisible = false;
        }
        else
        {
            Show();
            _isVisible = true;
            UpdateStatistics();
        }
    }
}
