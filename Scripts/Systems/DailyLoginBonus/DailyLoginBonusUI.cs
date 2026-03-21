using Godot;
using System;
using System.Collections.Generic;

public class DailyLoginBonusUI : Control
{
    private DailyLoginBonusSystem _system;
    private Label _titleLabel;
    private Label _streakLabel;
    private Label _statusLabel;
    private Button _claimButton;
    private Button _closeButton;
    private VBoxContainer _weeklyContainer;
    private VBoxContainer _historyContainer;
    private Label _statsLabel;
    
    // Tab containers
    private TabContainer _tabContainer;
    private Control _dailyTab;
    private Control _historyTab;
    
    // REQ-058-11: Migrated from Godot 3 .Connect() to C# event
    public event Action OnClaimPressedEvent;
    private Control _statsTab;
    
    public override void _Ready()
    {
        _system = GetNode<DailyLoginBonusSystem>("/root/DailyLoginBonusSystem");
        
        SetupUI();
        RefreshUI();
    }
    
    private void SetupUI()
    {
        // 主容器
        var mainPanel = new PanelContainer
        {
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            AnchorTop = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -300,
            OffsetRight = 300,
            OffsetTop = -250,
            OffsetBottom = 250
        };
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // 标题栏
        var titleBar = new HBoxContainer();
        mainVBox.AddChild(titleBar);
        
        _titleLabel = new Label
        {
            Text = "每日登录奖励",
            Alignment = Alignment.Center
        };
        _titleLabel.Set("custom_colors/font_color", new Color(1, 0.84f, 0));
        titleBar.AddChild(_closeButton);
        
        // 状态标签
        _streakLabel = new Label
        {
            Text = "连续登录: 0 天",
            Alignment = Alignment.Center
        };
        mainVBox.AddChild(_streakLabel);
        
        _statusLabel = new Label
        {
            Text = "今日奖励: 未领取",
            Alignment = Alignment.Center
        };
        mainVBox.AddChild(_statusLabel);
        
        // 标签容器
        _tabContainer = new TabContainer
        {
            SizeFlagsHorizontal = SizeFlags.Expand | SizeFlags.Fill
        };
        mainVBox.AddChild(_tabContainer);
        
        // 每日标签
        _dailyTab = new Control();
        _dailyTab.SetAnchor(Control.LayoutPreset.FullRect);
        _tabContainer.AddChild(_dailyTab);
        _tabContainer.SetTabTitle(_dailyTab, "每日奖励");
        
        SetupDailyTab(_dailyTab);
        
        // 历史标签
        _historyTab = new Control();
        _historyTab.SetAnchor(Control.LayoutPreset.FullRect);
        _tabContainer.AddChild(_historyTab);
        _tabContainer.SetTabTitle(_historyTab, "历史记录");
        
        SetupHistoryTab(_historyTab);
        
        // 统计标签
        _statsTab = new Control();
        _statsTab.SetAnchor(Control.LayoutPreset.FullRect);
        _tabContainer.AddChild(_statsTab);
        _tabContainer.SetTabTitle(_statsTab, "统计数据");
        
        SetupStatsTab(_statsTab);
        
        // 领取按钮
        _claimButton = new Button
        {
            Text = "领取今日奖励",
            SizeFlagsHorizontal = SizeFlags.Expand
        };
        _claimButton.Set("custom_colors/font_color", new Color(1, 1, 1));
        _claimButton.Set("custom_colors/font_hover_color", new Color(1, 1, 0));
        // REQ-058-11: migrated from Godot 3 .Connect() to C# event +=
        _claimButton.Pressed += OnClaimPressed;
        mainVBox.AddChild(_claimButton);
    }
    
    private void SetupDailyTab(Control tab)
    {
        var scroll = new ScrollContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetRight = -10,
            OffsetBottom = -10
        };
        tab.AddChild(scroll);
        
        _weeklyContainer = new VBoxContainer();
        scroll.AddChild(_weeklyContainer);
        
        // 标题
        var header = new Label
        {
            Text = "本周奖励预览",
            Alignment = Alignment.Center
        };
        header.Set("custom_colors/font_color", new Color(1, 0.84f, 0));
        _weeklyContainer.AddChild(header);
    }
    
    private void SetupHistoryTab(Control tab)
    {
        var scroll = new ScrollContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetRight = -10,
            OffsetBottom = -10
        };
        tab.AddChild(scroll);
        
        _historyContainer = new VBoxContainer();
        scroll.AddChild(_historyContainer);
    }
    
    private void SetupStatsTab(Control tab)
    {
        var vbox = new VBoxContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetRight = -10,
            OffsetBottom = -10
        };
        tab.AddChild(vbox);
        
        _statsLabel = new Label
        {
            Text = "统计数据",
            Alignment = Alignment.Center
        };
        vbox.AddChild(_statsLabel);
    }
    
    private void RefreshUI()
    {
        if (_system == null) return;
        
        var info = _system.GetLoginInfo();
        
        // 更新连续登录显示
        int streak = Convert.ToInt32(info["currentStreak"]);
        _streakLabel.Text = $"连续登录: {streak} 天 (最佳: {_system.GetStatistics()["bestStreak"]} 天)";
        
        // 更新状态
        bool canClaim = Convert.ToBoolean(info["canClaim"]);
        if (canClaim)
        {
            _statusLabel.Text = "今日奖励: 可领取！";
            _statusLabel.Set("custom_colors/font_color", new Color(0, 1, 0));
            _claimButton.Disabled = false;
            _claimButton.Text = "领取今日奖励";
        }
        else
        {
            _statusLabel.Text = "今日奖励: 已领取 (明天再来！)";
            _statusLabel.Set("custom_colors/font_color", new Color(0.5f, 0.5f, 0.5f));
            _claimButton.Disabled = true;
            _claimButton.Text = "已领取";
        }
        
        // 更新每周预览
        RefreshWeeklyPreview();
        
        // 更新历史记录
        RefreshHistory();
        
        // 更新统计
        RefreshStats();
    }
    
    private void RefreshWeeklyPreview()
    {
        // 清除旧内容
        foreach (Node child in _weeklyContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var preview = _system.GetWeeklyPreview();
        
        foreach (var day in preview)
        {
            int dayNum = Convert.ToInt32(day["day"]);
            int gold = Convert.ToInt32(day["gold"]);
            int exp = Convert.ToInt32(day["exp"]);
            int diamonds = Convert.ToInt32(day["diamonds"]);
            bool claimed = Convert.ToBoolean(day["claimed"]);
            
            var dayPanel = new PanelContainer
            {
                CustomMinimumHeight = 40
            };
            _weeklyContainer.AddChild(dayPanel);
            
            var dayHBox = new HBoxContainer();
            dayPanel.AddChild(dayHBox);
            
            // 日期标签
            var dayLabel = new Label
            {
                Text = $"第 {dayNum} 天:",
                SizeFlagsHorizontal = SizeFlags.Expand,
                Alignment = Alignment.Left
            };
            if (claimed)
            {
                dayLabel.Set("custom_colors/font_color", new Color(0.5f, 0.5f, 0.5f));
            }
            dayHBox.AddChild(dayLabel);
            
            // 奖励标签
            string rewards = $"{gold}金";
            if (exp > 0) rewards += $", {exp}经验";
            if (diamonds > 0) rewards += $", {diamonds}钻石";
            
            if (dayNum == 7)
            {
                rewards += " [大奖!]";
            }
            
            var rewardLabel = new Label
            {
                Text = rewards,
                SizeFlagsHorizontal = SizeFlags.Expand,
                Alignment = Alignment.Center
            };
            
            if (claimed)
            {
                rewardLabel.Set("custom_colors/font_color", new Color(0.5f, 0.5f, 0.5f));
            }
            else if (dayNum == 7)
            {
                rewardLabel.Set("custom_colors/font_color", new Color(1, 0.84f, 0));
            }
            else
            {
                rewardLabel.Set("custom_colors/font_color", new Color(0, 1, 0));
            }
            
            dayHBox.AddChild(rewardLabel);
            
            // 状态标签
            var statusLabel = new Label
            {
                Text = claimed ? "[已领取]" : "[未领取]",
                SizeFlagsHorizontal = SizeFlags.Expand,
                Alignment = Alignment.Right
            };
            if (claimed)
            {
                statusLabel.Set("custom_colors/font_color", new Color(0, 1, 0));
            }
            else
            {
                statusLabel.Set("custom_colors/font_color", new Color(1, 0.5f, 0));
            }
            dayHBox.AddChild(statusLabel);
        }
    }
    
    private void RefreshHistory()
    {
        // 清除旧内容
        foreach (Node child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var history = _system.GetHistory();
        
        if (history.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "暂无登录记录",
                Alignment = Alignment.Center
            };
            _historyContainer.AddChild(emptyLabel);
            return;
        }
        
        foreach (var entry in history)
        {
            string date = entry["date"].ToString();
            int day = Convert.ToInt32(entry["day"]);
            int streak = Convert.ToInt32(entry["streak"]);
            int gold = Convert.ToInt32(entry["gold"]);
            int exp = Convert.ToInt32(entry["exp"]);
            bool monthly = Convert.ToBoolean(entry["monthlyBonus"]);
            
            var entryPanel = new PanelContainer
            {
                CustomMinimumHeight = 35
            };
            _historyContainer.AddChild(entryPanel);
            
            var entryLabel = new Label
            {
                Text = $"{date} | 第{day}天 | 连续{streak}天 | +{gold}金 {exp}经验" + (monthly ? " [月度大奖]" : ""),
                Alignment = Alignment.Center
            };
            entryPanel.AddChild(entryLabel);
        }
    }
    
    private void RefreshStats()
    {
        var stats = _system.GetStatistics();
        
        string statsText = "📊 累计统计\n\n";
        statsText += $"总登录次数: {stats["totalLogins"]}\n";
        statsText += $"累计登录天数: {stats["cumulativeLoginDays"]}\n";
        statsText += $"最佳连续登录: {stats["bestStreak"]} 天\n\n";
        statsText += $"累计获得金币: {stats["totalGoldReceived"]}\n";
        statsText += $"累计获得经验: {stats["totalExpReceived"]}\n";
        statsText += $"累计获得钻石: {stats["totalDiamondsReceived"]}\n";
        statsText += $"\n已领取奖励次数: {stats["claimedBonusCount"]}";
        
        _statsLabel.Text = statsText;
    }
    
    private void OnClaimPressed()
    {
        // REQ-058-11: Invoke new event
        OnClaimPressedEvent?.Invoke();
        if (_system == null) return;
        
        var result = _system.ClaimDailyBonus();
        
        if (result.Count > 0)
        {
            int gold = Convert.ToInt32(result["gold"]);
            int exp = Convert.ToInt32(result["exp"]);
            int diamonds = Convert.ToInt32(result["diamonds"]);
            int day = Convert.ToInt32(result["day"]);
            bool monthly = Convert.ToBoolean(result["monthlyBonus"]);
            
            string message = $"🎉 奖励已发放！\n\n";
            message += $"第 {day} 天奖励:\n";
            message += $"+ {gold} 金币\n";
            message += $"+ {exp} 经验\n";
            if (diamonds > 0) message += $"+ {diamonds} 钻石\n";
            if (monthly) message += "\n⭐ 月度大奖已触发！";
            
            // 显示结果（这里可以用弹窗，简化处理用Label更新）
            GD.Print(message);
        }
        
        RefreshUI();
    }
    
    private void OnClosePressed()
    {
        QueueFree();
    }
}
