using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 公会节日UI - 显示节日活动界面
/// </summary>
public partial class GuildFestivalUI : Control
{
    private GuildFestivalSystem _festivalSystem;
    private TabContainer _tabContainer;
    private VBoxContainer _festivalsList;
    private VBoxContainer _rewardsList;
    private VBoxContainer _historyList;
    private Label _currentBonusLabel;
    private Label _statusLabel;
    private Button _closeButton;
    private bool _isVisible = false;

    public override void _Ready()
    {
        SetupUI();
        
        // 获取系统
        var system = GetNode("/root/Main/GuildFestivalSystem");
        if (system != null)
        {
            _festivalSystem = (GuildFestivalSystem)system;
        }
        
        // 默认隐藏
        Hide();
        
        // 连接信号
        ConnectSignals();
        
        GD.Print("[GuildFestivalUI] Guild Festival UI initialized");
    }

    private void SetupUI()
    {
        // 主容器
        var mainPanel = new PanelContainer
        {
            AnchorRight = 0.6f,
            AnchorBottom = 0.7f,
            RectMinSize = new Vector2(800, 600),
            RectPivotOffset = new Vector2(400, 300)
        };
        mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        AddChild(mainPanel);

        var mainVBox = new VBoxContainer { RectMinSize = new Vector2(780, 580) };
        mainPanel.AddChild(mainVBox);

        // 标题栏
        var titleBar = new HBoxContainer();
        mainVBox.AddChild(titleBar);

        var titleLabel = new Label
        {
            Text = "🎉 公会节日系统",
            RectMinSize = new Vector2(0, 40),
            Align = Label.AlignEnum.Center
        };
        titleLabel.GetStyleBox("normal")?.Set("font_size", 24);
        titleBar.AddChild(titleLabel);

        titleBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        _statusLabel = new Label { Text = "当前无活动", RectMinSize = new Vector2(200, 40) };
        titleBar.AddChild(_statusLabel);

        // 当前加成显示
        _currentBonusLabel = new Label
        {
            Text = "加成: 金币 1.0x | 经验 1.0x",
            RectMinSize = new Vector2(0, 30)
        };
        _currentBonusLabel.GetStyleBox("normal")?.Set("font_size", 16);
        mainVBox.AddChild(_currentBonusLabel);

        // 标签页
        _tabContainer = new TabContainer { SizeFlagsVertical = Control.SizeFlags.Expand };
        mainVBox.AddChild(_tabContainer);

        // 节日列表
        var festivalsTab = new VBoxContainer { Name = "节日" };
        _tabContainer.AddChild(festivalsTab);
        SetupFestivalsTab(festivalsTab);

        // 奖励预览
        var rewardsTab = new VBoxContainer { Name = "奖励" };
        _tabContainer.AddChild(rewardsTab);
        SetupRewardsTab(rewardsTab);

        // 历史记录
        var historyTab = new VBoxContainer { Name = "历史" };
        _tabContainer.AddChild(historyTab);
        SetupHistoryTab(historyTab);

        // 底部按钮
        var buttonBar = new HBoxContainer();
        mainVBox.AddChild(buttonBar);

        buttonBar.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        _closeButton = new Button { Text = "关闭 (ESC)", RectMinSize = new Vector2(120, 40) };
        _closeButton.Pressed += OnClosePressed;
        buttonBar.AddChild(_closeButton);
    }

    private void SetupFestivalsTab(VBoxContainer tab)
    {
        var scroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.Expand };
        tab.AddChild(scroll);

        _festivalsList = new VBoxContainer { RectMinSize = new Vector2(700, 400) };
        scroll.AddChild(_festivalsList);

        // 刷新按钮
        var refreshButton = new Button { Text = "刷新列表", RectMinSize = new Vector2(120, 35) };
        refreshButton.Pressed += RefreshFestivals;
        tab.AddChild(refreshButton);
    }

    private void SetupRewardsTab(VBoxContainer tab)
    {
        var scroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.Expand };
        tab.AddChild(scroll);

        _rewardsList = new VBoxContainer { RectMinSize = new Vector2(700, 400) };
        scroll.AddChild(_rewardsList);

        SetupRewardsList();
    }

    private void SetupHistoryTab(VBoxContainer tab)
    {
        var scroll = new ScrollContainer { SizeFlagsVertical = Control.SizeFlags.Expand };
        tab.AddChild(scroll);

        _historyList = new VBoxContainer { RectMinSize = new Vector2(700, 400) };
        scroll.AddChild(_historyList);

        SetupHistoryList();
    }

    private void SetupRewardsList()
    {
        _rewardsList.ClearChildren();

        var rewards = new[]
        {
            ("春季庆典", "金币加成 50%, 经验加成 30%, 100奖励点"),
            ("丰收祭", "金币加成 80%, 经验加成 20%, 120奖励点"),
            ("仲夏节", "金币加成 40%, 经验加成 50%, 110奖励点"),
            ("中秋节", "金币加成 60%, 经验加成 40%, 130奖励点"),
            ("冬至节", "金币加成 50%, 经验加成 60%, 125奖励点"),
            ("周年庆", "金币加成 100%, 经验加成 100%, 200奖励点"),
            ("英雄纪念日", "金币加成 70%, 经验加成 80%, 150奖励点"),
            ("贸易博览会", "金币加成 150%, 经验加成 10%, 100奖励点"),
            ("龙舟赛", "金币加成 30%, 经验加成 40%, 115奖励点"),
            ("骑士锦标赛", "金币加成 40%, 经验加成 90%, 140奖励点")
        };

        foreach (var (name, desc) in rewards)
        {
            var card = CreateRewardCard(name, desc);
            _rewardsList.AddChild(card);
        }
    }

    private void SetupHistoryList()
    {
        _historyList.ClearChildren();

        var history = new[]
        {
            ("2026-01-01", "春季庆典", "完成", "获得 100 奖励点"),
            ("2025-12-25", "冬至节", "完成", "获得 125 奖励点"),
            ("2025-11-11", "周年庆", "完成", "获得 200 奖励点")
        };

        foreach (var (date, name, status, reward) in history)
        {
            var card = CreateHistoryCard(date, name, status, reward);
            _historyList.AddChild(card);
        }
    }

    private Control CreateRewardCard(string name, string desc)
    {
        var card = new PanelContainer { RectMinSize = new Vector2(700, 60) };
        
        var hbox = new HBoxContainer { RectMinSize = new Vector2(680, 50) };
        card.AddChild(hbox);

        var iconLabel = new Label { Text = "🎊", RectMinSize = new Vector2(50, 50) };
        iconLabel.GetStyleBox("normal")?.Set("font_size", 28);
        hbox.AddChild(iconLabel);

        var infoVBox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.Expand };
        hbox.AddChild(infoVBox);

        var nameLabel = new Label { Text = name, SizeFlagsHorizontal = Control.SizeFlags.Expand };
        nameLabel.GetStyleBox("normal")?.Set("font_size", 18);
        infoVBox.AddChild(nameLabel);

        var descLabel = new Label { Text = desc, SizeFlagsHorizontal = Control.SizeFlags.Expand };
        descLabel.GetStyleBox("normal")?.Set("font_size", 14);
        descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        infoVBox.AddChild(descLabel);

        return card;
    }

    private Control CreateHistoryCard(string date, string name, string status, string reward)
    {
        var card = new PanelContainer { RectMinSize = new Vector2(700, 60) };
        
        var hbox = new HBoxContainer { RectMinSize = new Vector2(680, 50) };
        card.AddChild(hbox);

        var dateLabel = new Label { Text = date, RectMinSize = new Vector2(100, 50) };
        dateLabel.GetStyleBox("normal")?.Set("font_size", 14);
        hbox.AddChild(dateLabel);

        var nameLabel = new Label { Text = name, SizeFlagsHorizontal = Control.SizeFlags.Expand };
        nameLabel.GetStyleBox("normal")?.Set("font_size", 16);
        hbox.AddChild(nameLabel);

        var statusLabel = new Label { Text = status, RectMinSize = new Vector2(80, 50) };
        statusLabel.GetStyleBox("normal")?.Set("font_size", 14);
        statusLabel.Modulate = new Color(0.3f, 0.8f, 0.3f);
        hbox.AddChild(statusLabel);

        var rewardLabel = new Label { Text = reward, RectMinSize = new Vector2(200, 50) };
        rewardLabel.GetStyleBox("normal")?.Set("font_size", 12);
        rewardLabel.Modulate = new Color(0.8f, 0.7f, 0.3f);
        hbox.AddChild(rewardLabel);

        return card;
    }

    private void ConnectSignals()
    {
        // 快捷键
        var input = GetTree().Root.GetViewport().GetInput();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && !keyEvent.Echo)
        {
            if (keyEvent.Scancode == (uint)KeyList.F)
            {
                ToggleVisibility();
            }
        }
    }

    public void ToggleVisibility()
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
            RefreshFestivals();
            RefreshStatus();
        }
    }

    private void RefreshFestivals()
    {
        if (_festivalSystem == null) return;

        _festivalsList.ClearChildren();

        var festivals = _festivalSystem.GetAllFestivals();
        foreach (var kvp in festivals)
        {
            var festival = kvp.Value;
            var card = CreateFestivalCard(festival);
            _festivalsList.AddChild(card);
        }
    }

    private Control CreateFestivalCard(FestivalData festival)
    {
        var stateColor = GetStateColor(festival.State);
        var card = new PanelContainer { RectMinSize = new Vector2(700, 80) };
        
        var hbox = new HBoxContainer { RectMinSize = new Vector2(680, 70) };
        card.AddChild(hbox);

        // 状态指示
        var stateIndicator = new PanelContainer { RectMinSize = new Vector2(10, 70) };
        stateIndicator.SelfModulate = stateColor;
        hbox.AddChild(stateIndicator);

        var infoVBox = new VBoxContainer { SizeFlagsHorizontal = Control.SizeFlags.Expand, RectMinSize = new Vector2(500, 70) };
        hbox.AddChild(infoVBox);

        var nameLabel = new Label { Text = festival.Name, SizeFlagsHorizontal = Control.SizeFlags.Expand };
        nameLabel.GetStyleBox("normal")?.Set("font_size", 18);
        infoVBox.AddChild(nameLabel);

        var descLabel = new Label { Text = festival.Description, SizeFlagsHorizontal = Control.SizeFlags.Expand };
        descLabel.GetStyleBox("normal")?.Set("font_size", 13);
        descLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        infoVBox.AddChild(descLabel);

        var statsLabel = new Label { 
            Text = $"⏱ {festival.Duration / 60}分钟 | 🪙 {festival.BonusGold * 100 - 100}% | ✨ {festival.BonusExp * 100 - 100}% | 🏆 {festival.RewardPoints}",
            SizeFlagsHorizontal = Control.SizeFlags.Expand 
        };
        statsLabel.GetStyleBox("normal")?.Set("font_size", 12);
        statsLabel.Modulate = new Color(0.8f, 0.8f, 0.4f);
        infoVBox.AddChild(statsLabel);

        // 状态按钮
        var buttonContainer = new VBoxContainer { RectMinSize = new Vector2(120, 70) };
        hbox.AddChild(buttonContainer);

        if (festival.State == GuildFestivalSystem.FestivalState.Inactive)
        {
            var startButton = new Button { Text = "开始准备", RectMinSize = new Vector2(110, 30) };
            startButton.Pressed += () => OnStartPressed(festival.Id);
            buttonContainer.AddChild(startButton);
        }
        else if (festival.State == GuildFestivalSystem.FestivalState.Active)
        {
            var joinButton = new Button { Text = "参与节日", RectMinSize = new Vector2(110, 30) };
            joinButton.Modulate = new Color(0.3f, 0.8f, 0.3f);
            buttonContainer.AddChild(joinButton);
        }
        else
        {
            var stateLabel = new Label { Text = GetStateText(festival.State), RectMinSize = new Vector2(110, 30) };
            stateLabel.GetStyleBox("normal")?.Set("font_size", 12);
            stateLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
            buttonContainer.AddChild(stateLabel);
        }

        var statusLabel2 = new Label { Text = $"参与: {festival.ParticipantCount}", RectMinSize = new Vector2(110, 30) };
        statusLabel2.GetStyleBox("normal")?.Set("font_size", 11);
        buttonContainer.AddChild(statusLabel2);

        return card;
    }

    private Color GetStateColor(GuildFestivalSystem.FestivalState state)
    {
        switch (state)
        {
            case GuildFestivalSystem.FestivalState.Inactive:
                return new Color(0.3f, 0.3f, 0.3f);
            case GuildFestivalSystem.FestivalState.Preparation:
                return new Color(0.8f, 0.6f, 0.2f);
            case GuildFestivalSystem.FestivalState.Active:
                return new Color(0.2f, 0.8f, 0.3f);
            case GuildFestivalSystem.FestivalState.Completed:
                return new Color(0.4f, 0.4f, 0.8f);
            default:
                return new Color(0.5f, 0.5f, 0.5f);
        }
    }

    private string GetStateText(GuildFestivalSystem.FestivalState state)
    {
        switch (state)
        {
            case GuildFestivalSystem.FestivalState.Inactive:
                return "未开始";
            case GuildFestivalSystem.FestivalState.Preparation:
                return "准备中";
            case GuildFestivalSystem.FestivalState.Active:
                return "进行中";
            case GuildFestivalSystem.FestivalState.Completed:
                return "已结束";
            default:
                return "未知";
        }
    }

    private void RefreshStatus()
    {
        if (_festivalSystem == null)
        {
            _currentBonusLabel.Text = "加成: 金币 1.0x | 经验 1.0x";
            _statusLabel.Text = "系统未连接";
            return;
        }

        var goldBonus = _festivalSystem.GetCurrentBonusGold();
        var expBonus = _festivalSystem.GetCurrentBonusExp();

        if (goldBonus > 1.0f || expBonus > 1.0f)
        {
            _currentBonusLabel.Text = $"加成: 🪙 金币 {goldBonus:F1}x | ✨ 经验 {expBonus:F1}x";
            _statusLabel.Text = "🎉 节日进行中!";
            _statusLabel.Modulate = new Color(0.3f, 0.9f, 0.4f);
        }
        else
        {
            _currentBonusLabel.Text = "加成: 金币 1.0x | 经验 1.0x";
            _statusLabel.Text = "当前无活动";
            _statusLabel.Modulate = new Color(0.6f, 0.6f, 0.6f);
        }
    }

    private void OnStartPressed(int festivalId)
    {
        if (_festivalSystem != null)
        {
            _festivalSystem.StartFestival(festivalId);
            RefreshFestivals();
            RefreshStatus();
        }
    }

    private void OnClosePressed()
    {
        Hide();
        _isVisible = false;
    }

    public override void _Notification(int what)
    {
        if (what == NotificationPredelete)
        {
            // 清理
        }
    }
}

public static class ControlExtensions
{
    public static void ClearChildren(this Control control)
    {
        foreach (var child in control.GetChildren())
        {
            child.QueueFree();
        }
    }
}
