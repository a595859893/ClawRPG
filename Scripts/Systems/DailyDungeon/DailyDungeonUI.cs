using Godot;
using System;
using System.Collections.Generic;

public class DailyDungeonUI : Control
{
    private VBoxContainer _mainContainer;
    private ScrollContainer _dungeonList;
    private VBoxContainer _dungeonListContainer;
    private Label _titleLabel;
    private Label _dailyInfoLabel;
    private Label _timerLabel;
    private Label _floorLabel;
    private Button _exitButton;
    private Button _closeButton;

    // Dungeon info panel
    private PanelContainer _infoPanel;
    private Label _infoName;
    private Label _infoDifficulty;
    private Label _infoFloors;
    private Label _infoTime;
    private Label _infoRewards;
    private Label _infoBest;
    private Button _startButton;

    // Combat UI (shown during dungeon)
    private Control _combatUI;
    private Label _combatFloorLabel;
    private Label _combatTimerLabel;
    private Label _combatEnemiesLabel;
    private ProgressBar _timerProgress;

    private DailyDungeonData _selectedDungeon;
    private bool _isVisible = false; 

    public override void _Ready()
    {
        SetupUI();
        Visible = false; 
    }

    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchor(AnchorPreset.Center);
        _mainContainer.SetAnchorPreset(AnchorPreset.Center);
        _mainContainer.Position = new Vector2(-400, -300);
        _mainContainer.Size = new Vector2(800, 600);
        _mainContainer.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(_mainContainer);

        // Background panel
        var bgPanel = new PanelContainer();
        bgPanel.SetMeta("__bg", true);
        
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.98f);
        styleBox.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        styleBox.SetBorderWidthAll(2);
        styleBox.SetCornerRadiusAll(8);
        bgPanel.AddThemeStyleboxOverride("panel", styleBox);
        _mainContainer.AddChild(bgPanel);

        var contentContainer = new VBoxContainer();
        contentContainer.AddThemeConstantOverride("separation", 10);
        bgPanel.AddChild(contentContainer);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "🏰 每日地下城";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 28);
        contentContainer.AddChild(_titleLabel);

        // Daily info
        _dailyInfoLabel = new Label();
        _dailyInfoLabel.Text = "今日挑战: 0/5 | 已完成: 否";
        _dailyInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _dailyInfoLabel.AddThemeFontSizeOverride("font_size", 16);
        contentContainer.AddChild(_dailyInfoLabel);

        // Separator
        var sep1 = new HSeparator();
        contentContainer.AddChild(sep1);

        // Dungeon list
        _dungeonList = new ScrollContainer();
        _dungeonList.Size = new Vector2(760, 350);
        contentContainer.AddChild(_dungeonList);

        _dungeonListContainer = new VBoxContainer();
        _dungeonListContainer.AddThemeConstantOverride("separation", 8);
        _dungeonList.AddChild(_dungeonListContainer);

        // Info panel
        _infoPanel = new PanelContainer();
        _infoPanel.Visible = false; 
        
        var infoStyle = new StyleBoxFlat();
        infoStyle.BgColor = new Color(0.15f, 0.15f, 0.2f);
        infoStyle.SetCornerRadiusAll(4);
        _infoPanel.AddThemeStyleboxOverride("panel", infoStyle);
        contentContainer.AddChild(_infoPanel);

        var infoContainer = new VBoxContainer();
        _infoPanel.AddChild(infoContainer);

        _infoName = new Label();
        _infoName.AddThemeFontSizeOverride("font_size", 20);
        infoContainer.AddChild(_infoName);

        _infoDifficulty = new Label();
        _infoDifficulty.AddThemeFontSizeOverride("font_size", 16);
        infoContainer.AddChild(_infoDifficulty);

        _infoFloors = new Label();
        _infoFloors.AddThemeFontSizeOverride("font_size", 14);
        infoContainer.AddChild(_infoFloors);

        _infoTime = new Label();
        _infoTime.AddThemeFontSizeOverride("font_size", 14);
        infoContainer.AddChild(_infoTime);

        _infoRewards = new Label();
        _infoRewards.AddThemeFontSizeOverride("font_size", 14);
        infoContainer.AddChild(_infoRewards);

        _infoBest = new Label();
        _infoBest.AddThemeFontSizeOverride("font_size", 14);
        infoContainer.AddChild(_infoBest);

        _startButton = new Button();
        _startButton.Text = "开始挑战";
        _startButton.CustomMinimumSize = new Vector2(200, 40);
        _startButton.Pressed += OnStartButtonPressed;
        contentContainer.AddChild(_startButton);

        // Buttons
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        buttonContainer.AddThemeConstantOverride("separation", 20);
        contentContainer.AddChild(buttonContainer);

        _closeButton = new Button();
        _closeButton.Text = "关闭 (ESC)";
        _closeButton.Pressed += OnCloseButtonPressed;
        buttonContainer.AddChild(_closeButton);

        // Combat UI (initially hidden)
        SetupCombatUI();
    }

    private void SetupCombatUI()
    {
        _combatUI = new Control();
        _combatUI.Visible = false; 
        _combatUI.SetAnchor(AnchorPreset.FullRect);
        AddChild(_combatUI);

        // Background
        var combatBg = new PanelContainer();
        combatBg.SetAnchor(AnchorPreset.FullRect);
        
        var combatStyle = new StyleBoxFlat();
        combatStyle.BgColor = new Color(0.05f, 0.05f, 0.1f, 0.95f);
        combatBg.AddThemeStyleboxOverride("panel", combatStyle);
        _combatUI.AddChild(combatBg);

        var combatContainer = new VBoxContainer();
        combatContainer.SetAnchor(AnchorPreset.Center);
        combatContainer.SetAnchorPreset(AnchorPreset.Center);
        combatContainer.Position = new Vector2(-200, -150);
        combatContainer.Size = new Vector2(400, 300);
        combatContainer.AddThemeConstantOverride("separation", 20);
        combatBg.AddChild(combatContainer);

        // Floor label
        _combatFloorLabel = new Label();
        _combatFloorLabel.Text = "第 1 层 / 10 层";
        _combatFloorLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _combatFloorLabel.AddThemeFontSizeOverride("font_size", 32);
        combatContainer.AddChild(_combatFloorLabel);

        // Timer
        _timerLabel = new Label();
        _timerLabel.Text = "剩余时间: 05:00";
        _timerLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _timerLabel.AddThemeFontSizeOverride("font_size", 24);
        combatContainer.AddChild(_timerLabel);

        // Timer progress bar
        _timerProgress = new ProgressBar();
        _timerProgress.CustomMinimumSize = new Vector2(300, 20);
        _timerProgress.MaxValue = 100;
        _timerProgress.Value = 100;
        
        var timerStyle = new StyleBoxFlat();
        timerStyle.BgColor = new Color(0.2f, 0.2f, 0.3f);
        timerStyle.SetCornerRadiusAll(4);
        _timerProgress.AddThemeStyleboxOverride("background", timerStyle);
        
        var timerFill = new StyleBoxFlat();
        timerFill.BgColor = new Color(0.2f, 0.8f, 0.4f);
        timerFill.SetCornerRadiusAll(4);
        _timerProgress.AddThemeStyleboxOverride("fill", timerFill);
        
        combatContainer.AddChild(_timerProgress);

        // Enemies defeated
        _combatEnemiesLabel = new Label();
        _combatEnemiesLabel.Text = "击败敌人: 0 / 3";
        _combatEnemiesLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _combatEnemiesLabel.AddThemeFontSizeOverride("font_size", 20);
        combatContainer.AddChild(_combatEnemiesLabel);

        // Exit button
        _exitButton = new Button();
        _exitButton.Text = "退出地下城";
        _exitButton.CustomMinimumSize = new Vector2(150, 40);
        _exitButton.Pressed += OnExitButtonPressed;
        combatContainer.AddChild(_exitButton);
    }

    public override void _Process(double delta)
    {
        if (!Visible || _combatUI == null)
            return;

        var system = DailyDungeonSystem.Instance;
        if (system == null)
            return;

        // Update in-dungeon UI
        if (system.IsInDungeon())
        {
            _combatUI.Visible = true;
            _mainContainer.Visible = false; 

            var dungeon = system.GetCurrentDungeon();
            var floor = system.GetCurrentFloor();
            var time = system.GetTimeRemaining();

            _combatFloorLabel.Text = $"第 {floor} 层 / {dungeon.TotalFloors} 层";
            
            int minutes = (int)(time / 60);
            int seconds = (int)(time % 60);
            _timerLabel.Text = $"剩余时间: {minutes:D2}:{seconds:D2}";

            // Update progress bar
            float progress = time / dungeon.TimeLimit * 100;
            _timerProgress.Value = progress;

            // Color based on time
            var fill = _timerProgress.GetThemeStylebox("fill") as StyleBoxFlat;
            if (fill != null)
            {
                if (progress < 20)
                    fill.BgColor = new Color(1f, 0.2f, 0.2f);
                else if (progress < 50)
                    fill.BgColor = new Color(1f, 0.6f, 0.2f);
                else
                    fill.BgColor = new Color(0.2f, 0.8f, 0.4f);
            }
        }
        else
        {
            _combatUI.Visible = false; 
            _mainContainer.Visible = true;
        }
    }

    public void Toggle()
    {
        Visible = !Visible;
        _isVisible = Visible;

        if (Visible)
        {
            RefreshDungeonList();
            RefreshDailyInfo();
        }
        else
        {
            _combatUI.Visible = false; 
        }
    }

    private void RefreshDungeonList()
    {
        // Clear existing
        foreach (Node child in _dungeonListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var system = DailyDungeonSystem.Instance;
        if (system == null)
            return;

        var dungeons = system.GetAvailableDungeons();
        
        foreach (var dungeon in dungeons)
        {
            var dungeonButton = CreateDungeonButton(dungeon);
            _dungeonListContainer.AddChild(dungeonButton);
        }
    }

    private Button CreateDungeonButton(DailyDungeonData dungeon)
    {
        var button = new Button();
        button.CustomMinimumSize = new Vector2(740, 60);
        button.Pressed += () => OnDungeonSelected(dungeon);

        var container = new HBoxContainer();
        container.AddThemeConstantOverride("separation", 20);
        button.AddChild(container);

        // Type icon
        var iconLabel = new Label();
        iconLabel.Text = GetDungeonIcon(dungeon.Type);
        iconLabel.AddThemeFontSizeOverride("font_size", 24);
        container.AddChild(iconLabel);

        // Name and info
        var infoContainer = new VBoxContainer();
        infoContainer.SizeFlagsHorizontal = Control.SizeFlags.Expand | Control.SizeFlags.Fill;
        container.AddChild(infoContainer);

        var nameLabel = new Label();
        nameLabel.Text = dungeon.Name;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Left;
        nameLabel.AddThemeFontSizeOverride("font_size", 18);
        infoContainer.AddChild(nameLabel);

        var detailLabel = new Label();
        var difficultyColor = DailyDungeonDatabase.GetDifficultyColor(dungeon.Difficulty);
        detailLabel.Text = $"[color=#{difficultyColor.ToHtml()}]{DailyDungeonDatabase.GetDifficultyName(dungeon.Difficulty)}[/color] | {dungeon.TotalFloors}层 | 等级{dungeon.RecommendLevel} | 奖励: {dungeon.GoldReward}金/{dungeon.ExpReward}经验";
        detailLabel.HorizontalAlignment = HorizontalAlignment.Left;
        detailLabel.AddThemeFontSizeOverride("font_size", 12);
        infoContainer.AddChild(detailLabel);

        // Best record
        var system = DailyDungeonSystem.Instance;
        if (system != null)
        {
            var playerData = system.GetPlayerDungeonData(dungeon.Id);
            if (playerData != null && playerData.BestFloor > 0)
            {
                var bestLabel = new Label();
                bestLabel.Text = $"最佳: 第{playerData.BestFloor}层";
                bestLabel.HorizontalAlignment = HorizontalAlignment.Right;
                bestLabel.AddThemeFontSizeOverride("font_size", 14);
                container.AddChild(bestLabel);
            }
        }

        return button;
    }

    private string GetDungeonIcon(DailyDungeonData.DungeonType type)
    {
        switch (type)
        {
            case DailyDungeonData.DungeonType.AbyssTower: return "🗼";
            case DailyDungeonData.DungeonType.DragonLair: return "🐉";
            case DailyDungeonData.DungeonType.AncientTomb: return "🪦";
            case DailyDungeonData.DungeonType.DemonCastle: return "🏰";
            case DailyDungeonData.DungeonType.SacredGround: return "✨";
            default: return "❓";
        }
    }

    private void RefreshDailyInfo()
    {
        var system = DailyDungeonSystem.Instance;
        if (system == null)
            return;

        int count = system.GetDailyChallengeCount();
        bool completed = system.IsDailyCompleted();

        _dailyInfoLabel.Text = $"今日挑战: {count}/5 | 已完成: {(completed ? "是" : "否")}";
    }

    private void OnDungeonSelected(DailyDungeonData dungeon)
    {
        _selectedDungeon = dungeon;
        _infoPanel.Visible = true;

        var difficultyColor = DailyDungeonDatabase.GetDifficultyColor(dungeon.Difficulty);

        _infoName.Text = dungeon.Name;
        _infoName.Modulate = DailyDungeonDatabase.GetDungeonTypeColor(dungeon.Type);

        _infoDifficulty.Text = $"难度: [color=#{difficultyColor.ToHtml()}]{DailyDungeonDatabase.GetDifficultyName(dungeon.Difficulty)}[/color]";
        
        _infoFloors.Text = $"总层数: {dungeon.TotalFloors} 层";
        _infoTime.Text = $"时间限制: {dungeon.TimeLimit / 60} 分钟";
        _infoRewards.Text = $"奖励: {dungeon.GoldReward} 金币, {dungeon.ExpReward} 经验";

        var system = DailyDungeonSystem.Instance;
        if (system != null)
        {
            var playerData = system.GetPlayerDungeonData(dungeon.Id);
            if (playerData != null && playerData.BestFloor > 0)
            {
                _infoBest.Text = $"历史最佳: 第 {playerData.BestFloor} 层 (通关 {playerData.TimesCompleted} 次)";
            }
            else
            {
                _infoBest.Text = "历史最佳: 尚未挑战";
            }
        }

        // Check if can start
        if (system != null)
        {
            bool canEnter = system.CanEnterDungeon(dungeon.Id);
            _startButton.Disabled = !canEnter;
            _startButton.Text = canEnter ? "开始挑战" : "等级不足";
        }
    }

    private void OnStartButtonPressed()
    {
        if (_selectedDungeon == null)
            return;

        var system = DailyDungeonSystem.Instance;
        if (system == null)
            return;

        if (system.StartDungeon(_selectedDungeon.Id))
        {
            GD.Print("Started dungeon: " + _selectedDungeon.Name);
        }
        else
        {
            GD.PrintErr("Failed to start dungeon");
        }
    }

    private void OnCloseButtonPressed()
    {
        Toggle();
    }

    private void OnExitButtonPressed()
    {
        var system = DailyDungeonSystem.Instance;
        if (system != null)
        {
            system.ExitDungeon();
        }
    }

    public override void _Input(InputEvent ev)
    {
        if (ev.IsActionPressed("ui_cancel") && Visible)
        {
            Toggle();
            GetTree().SetInputAsHandled();
        }
    }
}
