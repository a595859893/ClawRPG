using Godot;
using System;
using System.Collections.Generic;

public partial class EnemyScalingUI : Control
{
    private Label _titleLabel;
    private TabContainer _tabContainer;
    private VBoxContainer _overviewTab;
    private VBoxContainer _enemiesTab;
    private VBoxContainer _statisticsTab;

    private Label _currentLevelLabel;
    private Label _floorLabel;
    private Label _killsLabel;
    private Label _timeLabel;
    private Label _waveLabel;

    private OptionButton _enemyTypeOption;
    private OptionButton _difficultyOption;
    private Label _scaledStatsLabel;

    private Label _totalCalculationsLabel;
    private Button _resetButton;

    private EnemyScalingSystem _system;
    private EnemyScalingDatabase _database;

    public override void _Ready()
    {
        _system = EnemyScalingSystem.Instance;
        _database = EnemyScalingDatabase.Instance;
        _system.Initialize();

        SetupUI();
    }

    private void SetupUI()
    {
        // 主容器
        var mainContainer = new VBoxContainer();
        mainContainer.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainContainer.CustomMinimumSize = new Vector2(600, 500);
        AddChild(mainContainer);

        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ Enemy Scaling System";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainContainer.AddChild(_titleLabel);

        // TabContainer
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        mainContainer.AddChild(_tabContainer);

        // 概览标签页
        _overviewTab = new VBoxContainer();
        _overviewTab.Name = "Overview";
        _tabContainer.AddChild(_overviewTab);
        SetupOverviewTab();

        // 敌人配置标签页
        _enemiesTab = new VBoxContainer();
        _enemiesTab.Name = "Enemies";
        _tabContainer.AddChild(_enemiesTab);
        SetupEnemiesTab();

        // 统计标签页
        _statisticsTab = new VBoxContainer();
        _statisticsTab.Name = "Statistics";
        _tabContainer.AddChild(_statisticsTab);
        SetupStatisticsTab();

        // 关闭按钮
        var closeButton = new Button();
        closeButton.Text = "Close (ESC)";
        closeButton.SizeFlagsHorizontal = Control.SizeFlags.ShrinkCenter;
        closeButton.Pressed += OnClosePressed;
        mainContainer.AddChild(closeButton);
    }

    private void SetupOverviewTab()
    {
        var grid = new GridContainer();
        grid.Columns = 2;
        grid.AddThemeConstantOverride("separation", 10);
        _overviewTab.AddChild(grid);

        // 当前等级
        grid.AddChild(CreateStatLabel("Player Level:"));
        _currentLevelLabel = CreateValueLabel("1");
        grid.AddChild(_currentLevelLabel);

        // 当前楼层
        grid.AddChild(CreateStatLabel("Current Floor:"));
        _floorLabel = CreateValueLabel("1");
        grid.AddChild(_floorLabel);

        // 击杀数
        grid.AddChild(CreateStatLabel("Enemies Defeated:"));
        _killsLabel = CreateValueLabel("0");
        grid.AddChild(_killsLabel);

        // 游戏时间
        grid.AddChild(CreateStatLabel("Play Time:"));
        _timeLabel = CreateValueLabel("0 min");
        grid.AddChild(_timeLabel);

        // 当前波次
        grid.AddChild(CreateStatLabel("Current Wave:"));
        _waveLabel = CreateValueLabel("1");
        grid.AddChild(_waveLabel);

        // 更新按钮
        var updateButton = new Button();
        updateButton.Text = "Update Progress";
        updateButton.Pressed += OnUpdateProgressPressed;
        _overviewTab.AddChild(updateButton);

        // 刷新显示
        RefreshOverview();
    }

    private void SetupEnemiesTab()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        _enemiesTab.AddChild(vbox);

        // 敌人类型选择
        var hbox1 = new HBoxContainer();
        vbox.AddChild(hbox1);

        var enemyLabel = new Label();
        enemyLabel.Text = "Enemy Type: ";
        hbox1.AddChild(enemyLabel);

        _enemyTypeOption = new OptionButton();
        var enemyTypes = _database.GetAllEnemyTypes();
        foreach (var type in enemyTypes)
        {
            _enemyTypeOption.AddItem(type);
        }
        _enemyTypeOption.ItemSelected += OnEnemyTypeSelected;
        hbox1.AddChild(_enemyTypeOption);

        // 难度选择
        var hbox2 = new HBoxContainer();
        vbox.AddChild(hbox2);

        var difficultyLabel = new Label();
        difficultyLabel.Text = "Difficulty: ";
        hbox2.AddChild(difficultyLabel);

        _difficultyOption = new OptionButton();
        var difficulties = _database.GetAllDifficulties();
        foreach (var diff in difficulties)
        {
            _difficultyOption.AddItem(diff);
        }
        _difficultyOption.Selected = 1; // Default to Normal
        _difficultyOption.ItemSelected += OnDifficultySelected;
        hbox2.AddChild(_difficultyOption);

        // 缩放等级输入
        var hbox3 = new HBoxContainer();
        vbox.AddChild(hbox3);

        var levelLabel = new Label();
        levelLabel.Text = "Scaling Level: ";
        hbox3.AddChild(levelLabel);

        var levelSpin = new SpinBox();
        levelSpin.MinValue = 1;
        levelSpin.MaxValue = 100;
        levelSpin.Value = 1;
        levelSpin.Name = "LevelSpin";
        hbox3.AddChild(levelSpin);

        // 计算按钮
        var calcButton = new Button();
        calcButton.Text = "Calculate Scaled Stats";
        calcButton.Pressed += OnCalculatePressed;
        vbox.AddChild(calcButton);

        // 结果显示
        var resultScroll = new ScrollContainer();
        resultScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(resultScroll);

        _scaledStatsLabel = new Label();
        _scaledStatsLabel.Text = "Select enemy type and difficulty, then click Calculate";
        _scaledStatsLabel.Autowrap = true;
        resultScroll.AddChild(_scaledStatsLabel);
    }

    private void SetupStatisticsTab()
    {
        var vbox = new VBoxContainer();
        vbox.AddThemeConstantOverride("separation", 10);
        _statisticsTab.AddChild(vbox);

        // 总计算次数
        var hbox1 = new HBoxContainer();
        vbox.AddChild(hbox1);
        hbox1.AddChild(CreateStatLabel("Total Scaling Calculations:"));
        _totalCalculationsLabel = CreateValueLabel("0");
        hbox1.AddChild(_totalCalculationsLabel);

        // 重置按钮
        _resetButton = new Button();
        _resetButton.Text = "Reset Progress";
        _resetButton.Pressed += OnResetPressed;
        vbox.AddChild(_resetButton);

        RefreshStatistics();
    }

    private Label CreateStatLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        label.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        return label;
    }

    private Label CreateValueLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        label.HorizontalAlignment = HorizontalAlignment.Right;
        return label;
    }

    private void RefreshOverview()
    {
        var stats = _system.GetStatistics();
        _currentLevelLabel.Text = stats["CurrentPlayerLevel"].ToString();
        _floorLabel.Text = stats["CurrentFloor"].ToString();
        _killsLabel.Text = stats["EnemiesDefeated"].ToString();
        _timeLabel.Text = $"{stats["PlayTimeMinutes"]} min";
        _waveLabel.Text = stats["CurrentWave"].ToString();
    }

    private void RefreshStatistics()
    {
        var stats = _system.GetStatistics();
        _totalCalculationsLabel.Text = stats["TotalScalingCalculations"].ToString();
    }

    private void OnUpdateProgressPressed()
    {
        // 模拟更新玩家进度
        var random = new Random();
        int playerLevel = random.Next(1, 50);
        int floor = random.Next(1, 30);
        int kills = random.Next(0, 500);
        float time = random.Next(0, 300);

        _system.UpdatePlayerProgress(playerLevel, floor, kills, 0, time);
        RefreshOverview();
    }

    private void OnEnemyTypeSelected(int index)
    {
    }

    private void OnDifficultySelected(int index)
    {
    }

    private void OnCalculatePressed()
    {
        string enemyType = _enemyTypeOption.GetItemText(_enemyTypeOption.Selected);
        string difficulty = _difficultyOption.GetItemText(_difficultyOption.Selected);

        var levelSpin = GetNode<SpinBox>("../Enemies/LevelSpin");
        if (levelSpin == null)
        {
            levelSpin = GetNode<VBoxContainer>(_tabContainer.GetPath()).GetChild<SpinBox>(2);
        }

        int level = (int)levelSpin.Value;

        var stats = _system.GetScaledEnemyStats(enemyType, level, difficulty);

        string result = $"=== {enemyType} (Level {level}, {difficulty}) ===\n\n";
        result += $"Health: {stats["Health"]:F0}\n";
        result += $"Attack: {stats["Attack"]:F1}\n";
        result += $"Defense: {stats["Defense"]:F1}\n";
        result += $"Speed: {stats["Speed"]:F1}\n";
        result += $"Experience: {stats["Experience"]:F1}\n";
        result += $"Drop Rate: {stats["DropRate"]:F2}x";

        _scaledStatsLabel.Text = result;
        RefreshStatistics();
    }

    private void OnResetPressed()
    {
        _system.ResetProgress();
        RefreshOverview();
        RefreshStatistics();
    }

    private void OnClosePressed()
    {
        QueueFree();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Scancode == KeyList.Escape)
        {
            QueueFree();
        }
    }
}
