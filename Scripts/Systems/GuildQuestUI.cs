using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 公会任务UI - 显示公会任务界面
/// 重构自 REQ-075: 移除对 GuildQuestSystem 的直接引用，改为事件驱动解耦
/// </summary>
public partial class GuildQuestUI : Control
{
    // ===== 事件接口（UI → System 通信） =====
    // UI 层通过事件向外部发送操作请求，不直接持有 System 引用
    
    /// <summary>请求刷新任务列表（外部/System 收到后调用 UpdateQuestList）</summary>
    public Action OnRefreshRequested;
    
    /// <summary>请求接受任务（外部/System 收到后处理）</summary>
    public Action<string> OnQuestAccepted;
    
    /// <summary>请求完成任务（外部/System 收到后处理）</summary>
    public Action<string> OnQuestCompleted;

    // ===== UI Elements =====
    private VBoxContainer _questList;
    private Label _titleLabel;
    private Label _statsLabel;
    private Button _refreshButton;
    private Button _closeButton;

    // Current state (for tracking which quest panel corresponds to which quest)
    private string _currentQuestId = "";

    // ===== 生命周期 =====
    
    public override void _Ready()
    {
        Visible = false;
        SetupUI();
        // 不再直接持有 System 引用
        // 初始化数据通过 OnRefreshRequested 事件请求
    }

    // ===== 公开更新接口（System → UI 通信） =====
    // REQ-075 解耦：UI 不再主动拉取数据，而是等待外部推送
    
    /// <summary>
    /// 更新任务列表显示（由外部/System 调用）
    /// </summary>
    public void UpdateQuestList(List<GuildQuest> quests)
    {
        // 清除旧列表
        foreach (Node child in _questList.GetChildren())
        {
            child.QueueFree();
        }
        
        if (quests == null || quests.Count == 0)
        {
            var emptyLabel = new Label { Text = "No active quests." };
            emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _questList.AddChild(emptyLabel);
            return;
        }
        
        foreach (var quest in quests)
        {
            var questPanel = CreateQuestPanel(quest);
            _questList.AddChild(questPanel);
        }
    }
    
    /// <summary>
    /// 更新统计数据显示（由外部/System 调用）
    /// </summary>
    public void UpdateStatistics(int totalCompleted, int totalPoints, int totalGold)
    {
        _statsLabel.Text = $"[b]Total Completed:[/b] {totalCompleted}  " +
                          $"[b]Total Guild Points:[/b] {totalPoints}  " +
                          $"[b]Total Gold Earned:[/b] {totalGold}";
    }

    // ===== UI 设置 =====
    
    private void SetupUI()
    {
        var bg = new Panel
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            Modulate = new Color(1, 1, 1, 0.9f)
        };
        AddChild(bg);

        var mainContainer = new VBoxContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 100,
            OffsetTop = 50,
            OffsetRight = -100,
            OffsetBottom = -50
        };
        bg.AddChild(mainContainer);

        _titleLabel = new Label
        {
            Text = "🏰 Guild Quests",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 32);
        mainContainer.AddChild(_titleLabel);

        var separator = new HSeparator();
        mainContainer.AddChild(separator);

        var buttonContainer = new HBoxContainer();
        mainContainer.AddChild(buttonContainer);

        _refreshButton = new Button
        {
            Text = "🔄 Refresh Quests (100g)",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _refreshButton.Pressed += OnRefreshPressed;
        buttonContainer.AddChild(_refreshButton);

        _closeButton = new Button
        {
            Text = "✕ Close",
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        _closeButton.Pressed += OnClosePressed;
        buttonContainer.AddChild(_closeButton);

        var scroll = new ScrollContainer
        {
            VerticalScrollBarExclusive = false
        };
        scroll.SizeFlagsVertical = SizeFlags.ExpandFill;
        mainContainer.AddChild(scroll);

        _questList = new VBoxContainer();
        _questList.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        _questList.SizeFlagsVertical = SizeFlags.ExpandFill;
        scroll.AddChild(_questList);

        _statsLabel = new Label
        {
            Text = "",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _statsLabel.AddThemeFontSizeOverride("font_size", 18);
        mainContainer.AddChild(_statsLabel);
    }

    // ===== 事件处理（转发到外部） =====
    
    /// <summary>
    /// 切换 UI 可见性（由外部或快捷键调用）
    /// </summary>
    public void ToggleUI()
    {
        Visible = !Visible;
        if (Visible)
        {
            // 可见时请求刷新数据
            OnRefreshRequested?.Invoke();
        }
    }

    private void OnRefreshPressed()
    {
        // 通过事件请求刷新，而不是直接调用 System
        OnRefreshRequested?.Invoke();
    }

    private void OnQuestAcceptedAction(string questId)
    {
        OnQuestAccepted?.Invoke(questId);
    }

    private void OnQuestCompletedAction(string questId)
    {
        OnQuestCompleted?.Invoke(questId);
    }

    private void OnClosePressed()
    {
        Visible = false;
    }

    // ===== 私有辅助方法 =====
    
    private Control CreateQuestPanel(GuildQuest quest)
    {
        var panel = new PanelContainer();
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        
        var hbox = new HBoxContainer();
        panel.AddChild(hbox);

        var difficultyColor = GetDifficultyColor(quest.Difficulty);
        var progressPercent = quest.TargetCount > 0 
            ? (float)quest.CurrentProgress / quest.TargetCount * 100 
            : 0f;
        
        var infoLabel = new RichTextLabel
        {
            FitContent = true,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        
        string statusIcon = quest.IsCompleted ? "✅" : "⬜";
        string progressText = $"{quest.CurrentProgress}/{quest.TargetCount}";
        
        infoLabel.Text = $"[color={difficultyColor}][b]{quest.Name}[/b][/color] {statusIcon}\n" +
                        $"[color=#AAAAAA]{quest.Description}[/color]\n" +
                        $"Progress: [color=#FFD700]{progressText}[/color] ({progressPercent:F0}%)\n" +
                        $"[color=#00FFFF]Guild Points: +{quest.GuildPoints}[/color] | [color=#FFD700]Gold: +{quest.GoldReward}[/color]";
        
        hbox.AddChild(infoLabel);

        var progressBar = new ProgressBar
        {
            SizeFlagsVertical = SizeFlags.ShrinkCenter,
            MinValue = 0,
            MaxValue = quest.TargetCount > 0 ? quest.TargetCount : 1,
            Value = quest.CurrentProgress,
            ShowPercentage = false
        };
        progressBar.CustomMinimumSize = new Vector2(0, 20);
        
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.2f, 0.2f, 0.2f);
        progressBar.AddThemeStyleboxOverride("background", styleBox);
        
        var fillStyle = new StyleBoxFlat();
        fillStyle.BgColor = quest.IsCompleted ? new Color(0, 1, 0) : new Color(0, 0.7f, 1);
        progressBar.AddThemeStyleboxOverride("fill", fillStyle);
        
        hbox.AddChild(progressBar);

        return panel;
    }

    private string GetDifficultyColor(int difficulty)
    {
        return difficulty switch
        {
            1 => "#90EE90",
            2 => "#FFD700",
            3 => "#FF6347",
            4 => "#FF00FF",
            5 => "#FF0000",
            _ => "#FFFFFF"
        };
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Visible = false;
        }
    }
}
