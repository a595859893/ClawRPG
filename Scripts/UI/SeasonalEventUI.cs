// Seasonal Event UI - Display and manage seasonal events
using Godot;
using System;
using System.Collections.Generic;

public class SeasonalEventUI : Control
{
    private PanelContainer _mainPanel;
    private VBoxContainer _eventList;
    private Label _titleLabel;
    private Label _multiplierLabel;
    private Button _closeButton;
    private SeasonalEventData.SeasonalEvent[] _events;

    public override void _Ready()
    {
        _Ready();
        SetupUI();
        LoadEvents();
    }

    private void SetupUI()
    {
        // Main panel
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainPanel);

        var mainMargin = new MarginContainer();
        mainMargin.SetOffsets(20, 20, 20, 20);
        _mainPanel.AddChild(mainMargin);

        var mainVBox = new VBoxContainer();
        mainVBox.AddThemeConstantOverride("separation", 15);
        mainMargin.AddChild(mainVBox);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "季节性活动";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(_titleLabel);

        // Multiplier info
        _multiplierLabel = new Label();
        _multiplierLabel.HorizontalAlignment = HorizontalAlignment.Center;
        mainVBox.AddChild(_multiplierLabel);

        // Event list
        var scrollContainer = new ScrollContainer();
        scrollContainer.CustomMinimumSize = new Vector2(560, 350);
        mainVBox.AddChild(scrollContainer);

        _eventList = new VBoxContainer();
        _eventList.AddThemeConstantOverride("separation", 10);
        scrollContainer.AddChild(_eventList);

        // Close button
        _closeButton = new Button();
        _closeButton.Text = "关闭";
        _closeButton.Pressed += OnClosePressed;
        mainVBox.AddChild(_closeButton);

        // Apply tween animation
        var tween = CreateTween();
        tween.SetParallel(true);
        tween.TweenProperty(_mainPanel, "modulate:a", 0f, 0f);
        tween.TweenProperty(_mainPanel, "scale", new Vector2(0.9f, 0.9f), 0f);
        tween.TweenProperty(_mainPanel, "modulate:a", 1f, 0.3f).SetTrans(Tween.TransitionType.Back).SetEasing(Tween.EasingFunction.EaseOut);
        tween.TweenProperty(_mainPanel, "scale", Vector2.One, 0.3f).SetTrans(Tween.TransitionType.Back).SetEasing(Tween.EasingFunction.EaseOut);
    }

    private void LoadEvents()
    {
        _events = SeasonalEventSystem.Instance.GetActiveEvents();
        
        UpdateMultiplierLabel();
        UpdateEventList();
    }

    private void UpdateMultiplierLabel()
    {
        float goldMult = SeasonalEventSystem.Instance.GetGoldMultiplier();
        float expMult = SeasonalEventSystem.Instance.GetEXPMultiplier();
        float dropMult = SeasonalEventSystem.Instance.GetDropRateMultiplier();

        _multiplierLabel.Text = $"当前加成: 金币 x{goldMult:F1} | 经验 x{expMult:F1} | 掉落 x{dropMult:F1}";
    }

    private void UpdateEventList()
    {
        foreach (Node child in _eventList.GetChildren())
        {
            child.QueueFree();
        }

        if (_events == null || _events.Length == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "当前没有进行中的活动";
            emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _eventList.AddChild(emptyLabel);
            return;
        }

        foreach (var evt in _events)
        {
            var eventPanel = CreateEventPanel(evt);
            _eventList.AddChild(eventPanel);
        }
    }

    private Control CreateEventPanel(SeasonalEventData.SeasonalEvent evt)
    {
        var panel = new PanelContainer();
        panel.CustomMinimumSize = new Vector2(540, 100);

        var margin = new MarginContainer();
        margin.SetOffsets(15, 10, 15, 10);
        panel.AddChild(margin);

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 15);
        margin.AddChild(hbox);

        // Event info
        var infoVBox = new VBoxContainer();
        infoVBox.AddThemeConstantOverride("separation", 5);
        hbox.AddChild(infoVBox);

        // Name with rarity color
        var nameLabel = new Label();
        nameLabel.Text = evt.EventName;
        nameLabel.AddThemeFontSizeOverride("font_size", 18);
        
        // Set rarity color
        Color rarityColor = GetRarityColor(evt.EventRarity);
        nameLabel.Modulate = rarityColor;
        infoVBox.AddChild(nameLabel);

        // Description
        var descLabel = new Label();
        descLabel.Text = evt.Description;
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        descLabel.Modulate = new Color(0.8f, 0.8f, 0.8f);
        infoVBox.AddChild(descLabel);

        // Stats
        var statsLabel = new Label();
        string stats = $"等级要求: {evt.RequiredLevel}";
        if (evt.EntryFee > 0)
            stats += $" | 参赛费: {evt.EntryFee}金币";
        if (evt.MaxEntries > 0)
            stats += $" | 剩余次数: {evt.MaxEntries - SeasonalEventSystem.Instance.GetEventEntries(evt.EventId)}";
        statsLabel.Text = stats;
        statsLabel.AddThemeFontSizeOverride("font_size", 12);
        statsLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        infoVBox.AddChild(statsLabel);

        // Multipliers
        var multLabel = new Label();
        string mults = "";
        if (evt.GoldMultiplier > 1) mults += $"金币x{evt.GoldMultiplier} ";
        if (evt.EXPMultiplier > 1) mults += $"经验x{evt.EXPMultiplier} ";
        if (evt.DropRateMultiplier > 1) mults += $"掉落x{evt.DropRateMultiplier} ";
        if (!string.IsNullOrEmpty(mults))
        {
            multLabel.Text = mults;
            multLabel.AddThemeFontSizeOverride("font_size", 12);
            multLabel.Modulate = new Color(1f, 0.9f, 0.5f);
            infoVBox.AddChild(multLabel);
        }

        // Rewards
        if (evt.RewardItemIds != null && evt.RewardItemIds.Count > 0)
        {
            var rewardLabel = new Label();
            rewardLabel.Text = "奖励: " + string.Join(", ", evt.RewardItemIds);
            rewardLabel.AddThemeFontSizeOverride("font_size", 11);
            rewardLabel.Modulate = new Color(0.6f, 0.9f, 0.6f);
            infoVBox.AddChild(rewardLabel);
        }

        // Participate button
        var buttonVBox = new VBoxContainer();
        buttonVBox.Alignment = BoxContainer.AlignmentMode.Center;
        buttonVBox.AddThemeConstantOverride("separation", 10);
        hbox.AddChild(buttonVBox);

        var participateBtn = new Button();
        participateBtn.Text = "参加";
        participateBtn.CustomMinimumSize = new Vector2(80, 40);
        
        bool canParticipate = SeasonalEventSystem.Instance.CanParticipate(evt.EventId);
        participateBtn.Disabled = !canParticipate;
        
        participateBtn.Pressed += () => OnParticipatePressed(evt.EventId);
        buttonVBox.AddChild(participateBtn);

        return panel;
    }

    private Color GetRarityColor(SeasonalEventData.EventRarity rarity)
    {
        return rarity switch
        {
            SeasonalEventData.EventRarity.Common => new Color(0.7f, 0.7f, 0.7f),
            SeasonalEventData.EventRarity.Uncommon => new Color(0.3f, 0.9f, 0.3f),
            SeasonalEventData.EventRarity.Rare => new Color(0.3f, 0.5f, 1f),
            SeasonalEventData.EventRarity.Epic => new Color(0.6f, 0.3f, 0.9f),
            SeasonalEventData.EventRarity.Legendary => new Color(1f, 0.6f, 0f),
            _ => Colors.White
        };
    }

    private void OnParticipatePressed(string eventId)
    {
        if (SeasonalEventSystem.Instance.Participate(eventId))
        {
            GD.Print($"[SeasonalEventUI] Participated in event: {eventId}");
            LoadEvents(); // Refresh UI
        }
    }

    private void OnClosePressed()
    {
        var tween = CreateTween();
        tween.TweenProperty(_mainPanel, "modulate:a", 0f, 0.2f);
        tween.TweenProperty(_mainPanel, "scale", new Vector2(0.9f, 0.9f), 0.2f);
        tween.TweenCallback(Callable.From(() => QueueFree()));
    }

    public void ToggleVisibility()
    {
        if (Visible)
        {
            OnClosePressed();
        }
        else
        {
            Show();
            LoadEvents();
        }
    }
}
