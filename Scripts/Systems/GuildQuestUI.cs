using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 公会任务UI - 显示公会任务界面
/// </summary>
public partial class GuildQuestUI : Control
{
    private VBoxContainer _questList;
    private Label _titleLabel;
    private Label _statsLabel;
    private Button _refreshButton;
    private Button _closeButton;

    private GuildQuestSystem _questSystem;

    public override void _Ready()
    {
        Visible = false;
        _questSystem = GuildQuestSystem.Instance;

        SetupUI();
        SetupKeybinds();
    }

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

    private void SetupKeybinds()
    {
        if (KeybindingSystem.Instance != null)
        {
            KeybindingSystem.Instance.BindAction("guild_quest", ToggleUI);
        }
    }

    public void ToggleUI()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshQuestList();
        }
    }

    private void RefreshQuestList()
    {
        foreach (Node child in _questList.GetChildren())
        {
            child.QueueFree();
        }

        var activeQuests = _questSystem.GetActiveQuests();
        
        foreach (var quest in activeQuests)
        {
            var questPanel = CreateQuestPanel(quest);
            _questList.AddChild(questPanel);
        }

        UpdateStats();
    }

    private Control CreateQuestPanel(GuildQuest quest)
    {
        var panel = new PanelContainer();
        panel.SizeFlagsHorizontal = SizeFlags.ExpandFill;
        
        var hbox = new HBoxContainer();
        panel.AddChild(hbox);

        var difficultyColor = GetDifficultyColor(quest.Difficulty);
        var statusColor = quest.IsCompleted ? "[color=#00FF00]" : "[color=#FFFFFF]";
        var progressPercent = (float)quest.CurrentProgress / quest.TargetCount * 100;
        
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
            MaxValue = quest.TargetCount,
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

    private void UpdateStats()
    {
        var stats = _questSystem.GetQuestStatistics();
        
        _statsLabel.Text = $"[b]Total Completed:[/b] {stats["total_completed"]}  " +
                          $"[b]Total Guild Points:[/b] {stats["total_points"]}  " +
                          $"[b]Total Gold Earned:[/b] {stats["total_gold"]}";
    }

    private void OnRefreshPressed()
    {
        _questSystem.RefreshQuests();
        RefreshQuestList();
    }

    private void OnClosePressed()
    {
        Visible = false;
    }

    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.Escape)
        {
            Visible = false;
        }
    }
}
