using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 世界事件UI - 显示世界事件通知和详情
/// </summary>
public class WorldEventUI : Control
{
    // UI Elements
    private Label _titleLabel;
    private Label _activeEventsLabel;
    private VBoxContainer _eventListContainer;
    private VBoxContainer _historyContainer;
    private Label _statsLabel;
    private TabContainer _tabContainer;
    private Label _noEventsLabel;

    // Event data display
    private Dictionary<string, EventCard> _eventCards = new Dictionary<string, EventCard>();

    private bool _isVisible = false;
    private int _refreshTimer = 0;

    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        Hide();
        GD.Print("[WorldEventUI] World Event UI initialized");
    }

    private void SetupUI()
    {
        // Main container
        Color bgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        Color borderColor = new Color(0.8f, 0.6f, 0.3f, 1f);

        // Background panel
        Panel background = new Panel();
        background.SetAnchorsPreset(Control.AnchorsPreset.Center);
        background.CustomMinimumSize = new Vector2(800, 600);
        background.RectPosition = new Vector2(-400, -300);
        background.Modulate = bgColor;
        AddChild(background);

        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ 世界事件";
        _titleLabel.RectPosition = new Vector2(0, 10);
        _titleLabel.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.AddColorOverride("font_color", new Color(1f, 0.9f, 0.7f, 1f));
        _titleLabel.RectMinSize = new Vector2(0, 40);
        background.AddChild(_titleLabel);

        // Close button
        Button closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.RectPosition = new Vector2(750, 15);
        closeBtn.RectMinSize = new Vector2(30, 30);
        closeBtn.Connect("pressed", this, nameof(OnClosePressed));
        background.AddChild(closeBtn);

        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.RectPosition = new Vector2(20, 60);
        _tabContainer.RectSize = new Vector2(760, 480);
        background.AddChild(_tabContainer);

        // Active events tab
        Control activeTab = new Control();
        activeTab.Name = "进行中";
        _tabContainer.AddChild(activeTab);

        _activeEventsLabel = new Label();
        _activeEventsLabel.Text = "当前进行中的世界事件:";
        _activeEventsLabel.RectPosition = new Vector2(10, 10);
        _activeEventsLabel.AddColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
        activeTab.AddChild(_activeEventsLabel);

        _eventListContainer = new VBoxContainer();
        _eventListContainer.RectPosition = new Vector2(10, 40);
        _eventListContainer.RectSize = new Vector2(740, 430);
        activeTab.AddChild(_eventListContainer);

        _noEventsLabel = new Label();
        _noEventsLabel.Text = "暂无进行中的事件\n\n世界事件将随机触发\n请留意游戏公告!";
        _noEventsLabel.RectPosition = new Vector2(10, 100);
        _noEventsLabel.Align = Label.AlignEnum.Center;
        _noEventsLabel.AddColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f, 1f));
        activeTab.AddChild(_noEventsLabel);

        // History tab
        Control historyTab = new Control();
        historyTab.Name = "历史";
        _tabContainer.AddChild(historyTab);

        Label historyTitle = new Label();
        historyTitle.Text = "事件历史记录:";
        historyTitle.RectPosition = new Vector2(10, 10);
        historyTitle.AddColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
        historyTab.AddChild(historyTitle);

        _historyContainer = new VBoxContainer();
        _historyContainer.RectPosition = new Vector2(10, 40);
        _historyContainer.RectSize = new Vector2(740, 430);
        historyTab.AddChild(_historyContainer);

        // Stats tab
        Control statsTab = new Control();
        statsTab.Name = "统计";
        _tabContainer.AddChild(statsTab);

        _statsLabel = new Label();
        _statsLabel.RectPosition = new Vector2(20, 20);
        _statsLabel.AddColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
        statsTab.AddChild(_statsLabel);

        // Hint label
        Label hintLabel = new Label();
        hintLabel.Text = "按 E 键切换显示";
        hintLabel.RectPosition = new Vector2(0, 555);
        hintLabel.SetAnchorsPreset(Control.AnchorsPreset.BottomWide);
        hintLabel.Align = Label.AlignEnum.Center;
        hintLabel.AddColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f, 1f));
        background.AddChild(hintLabel);
    }

    private void ConnectSignals()
    {
        if (WorldEventSystem.Instance != null)
        {
            WorldEventSystem.Instance.Connect(nameof(WorldEventSystem.EventAnnounced), this, nameof(OnEventAnnounced));
            WorldEventSystem.Instance.Connect(nameof(WorldEventSystem.EventStarted), this, nameof(OnEventStarted));
            WorldEventSystem.Instance.Connect(nameof(WorldEventSystem.EventCompleted), this, nameof(OnEventCompleted));
            WorldEventSystem.Instance.Connect(nameof(WorldEventSystem.PlayerParticipated), this, nameof(OnPlayerParticipated));
        }
    }

    public override void _Process(float delta)
    {
        _refreshTimer += (int)(delta * 1000);
        if (_refreshTimer >= 500)
        {
            _refreshTimer = 0;
            RefreshEventList();
            UpdateStats();
        }
    }

    private void RefreshEventList()
    {
        // Clear existing cards
        foreach (var card in _eventCards.Values)
        {
            card.QueueFree();
        }
        _eventCards.Clear();

        // Get active events
        var activeEvents = WorldEventSystem.Instance.GetActiveEvents();

        if (activeEvents.Count == 0)
        {
            _noEventsLabel.Visible = true;
            return;
        }

        _noEventsLabel.Visible = false;

        // Create cards for each event
        int index = 0;
        foreach (var kvp in activeEvents)
        {
            WorldEvent evt = kvp.Value;
            EventCard card = new EventCard(evt);
            card.RectPosition = new Vector2(0, index * 160);
            _eventListContainer.AddChild(card);
            _eventCards[kvp.Key] = card;
            index++;
        }
    }

    private void UpdateStats()
    {
        string stats = $"📊 事件统计\n\n";
        stats += $"触发事件总数: {WorldEventSystem.Instance.GetTotalEventsTriggered()}\n";
        stats += $"参与次数: {WorldEventSystem.Instance.GetTotalParticipations()}\n";
        stats += $"获得奖励数: {WorldEventSystem.Instance.GetTotalRewardsClaimed()}\n\n";
        stats += $"事件类型:\n";
        stats += $"  - Festival (节日庆典)\n";
        stats += $"  - Dragon Attack (巨龙袭击)\n";
        stats += $"  - Merchant Caravan (商队)\n";
        stats += $"  - Goblin Raid (哥布林)\n";
        stats += $"  - Bounty Hunt (赏金)\n";
        stats += $"  - Tournament (竞技)\n";
        stats += $"  - Eclipse (日食)\n";
        stats += $"  - Harvest (丰收)\n";
        stats += $"  - Blizzard (暴风雪)\n";
        stats += $"  - Plague (瘟疫)\n";
        stats += $"  - Treasure (宝藏)\n";
        stats += $"  - Ancient (远古)\n";

        _statsLabel.Text = stats;
    }

    private void RefreshHistory()
    {
        foreach (Node child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }

        var history = WorldEventSystem.Instance.GetEventHistory();
        for (int i = history.Count - 1; i >= 0; i--)
        {
            string eventId = history[i];
            WorldEvent evt = WorldEventSystem.Instance.GetEvent(eventId);
            if (evt != null)
            {
                Label historyItem = new Label();
                historyItem.Text = $"• {evt.name}";
                historyItem.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1f));
                _historyContainer.AddChild(historyItem);
            }
        }
    }

    // Event handlers
    private void OnEventAnnounced(string eventId, string eventName, string description)
    {
        RefreshEventList();
    }

    private void OnEventStarted(string eventId)
    {
        RefreshEventList();
    }

    private void OnEventCompleted(string eventId, string eventName)
    {
        RefreshEventList();
        RefreshHistory();
    }

    private void OnPlayerParticipated(string eventId, string eventName)
    {
        RefreshEventList();
    }

    private void OnClosePressed()
    {
        Hide();
        _isVisible = false;
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
            RefreshEventList();
            RefreshHistory();
            UpdateStats();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Scancode == (int)KeyList.E)
            {
                Toggle();
            }
            else if (keyEvent.Scancode == (int)KeyList.Escape && _isVisible)
            {
                Hide();
                _isVisible = false;
            }
        }
    }

    // Event card class
    private class EventCard : Control
    {
        private WorldEvent _event;

        public EventCard(WorldEvent evt)
        {
            _event = evt;
            RectMinSize = new Vector2(720, 150);
            SetupCard();
        }

        private void SetupCard()
        {
            Color cardBg;
            Color typeColor;

            switch (_event.type)
            {
                case WorldEventSystem.EventType.Festival:
                case WorldEventSystem.EventType.HarvestFestival:
                    cardBg = new Color(0.2f, 0.5f, 0.2f, 0.3f);
                    typeColor = new Color(0.3f, 1f, 0.3f, 1f);
                    break;
                case WorldEventSystem.EventType.DragonAttack:
                case WorldEventSystem.EventType.GoblinRaid:
                case WorldEventSystem.EventType.AncientAwakening:
                    cardBg = new Color(0.5f, 0.2f, 0.2f, 0.3f);
                    typeColor = new Color(1f, 0.3f, 0.3f, 1f);
                    break;
                case WorldEventSystem.EventType.MerchantCaravan:
                    cardBg = new Color(0.5f, 0.5f, 0.2f, 0.3f);
                    typeColor = new Color(1f, 1f, 0.3f, 1f);
                    break;
                case WorldEventSystem.EventType.Eclipse:
                    cardBg = new Color(0.3f, 0.2f, 0.5f, 0.3f);
                    typeColor = new Color(0.7f, 0.5f, 1f, 1f);
                    break;
                default:
                    cardBg = new Color(0.2f, 0.3f, 0.5f, 0.3f);
                    typeColor = new Color(0.5f, 0.7f, 1f, 1f);
                    break;
            }

            // Background
            Panel bg = new Panel();
            bg.SetAnchorsPreset(Control.AnchorsPreset.Wide);
            bg.Modulate = cardBg;
            bg.RectMinSize = new Vector2(720, 140);
            AddChild(bg);

            // Event name
            Label nameLabel = new Label();
            nameLabel.Text = $"⚡ {_event.name}";
            nameLabel.RectPosition = new Vector2(15, 10);
            nameLabel.AddColorOverride("font_color", typeColor);
            nameLabel.RectMinSize = new Vector2(0, 30);
            bg.AddChild(nameLabel);

            // Status
            Label statusLabel = new Label();
            string statusText = "";
            Color statusColor = new Color(1f, 1f, 1f, 1f);

            switch (_event.status)
            {
                case WorldEventSystem.EventStatus.Announced:
                    statusText = "即将开始";
                    statusColor = new Color(1f, 0.8f, 0.2f, 1f);
                    break;
                case WorldEventSystem.EventStatus.Active:
                    statusText = "进行中";
                    statusColor = new Color(0.3f, 1f, 0.3f, 1f);
                    break;
                case WorldEventSystem.EventStatus.Concluding:
                    statusText = "即将结束";
                    statusColor = new Color(1f, 0.5f, 0.2f, 1f);
                    break;
            }

            statusLabel.Text = statusText;
            statusLabel.RectPosition = new Vector2(600, 10);
            statusLabel.AddColorOverride("font_color", statusColor);
            bg.AddChild(statusLabel);

            // Description
            Label descLabel = new Label();
            descLabel.Text = _event.description;
            descLabel.RectPosition = new Vector2(15, 45);
            descLabel.AddColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f, 1f));
            descLabel.RectMinSize = new Vector2(0, 40);
            bg.AddChild(descLabel);

            // Time remaining
            Label timeLabel = new Label();
            int minutes = _event.timeRemaining / 60;
            int seconds = _event.timeRemaining % 60;
            timeLabel.Text = $"⏱️ 剩余时间: {minutes}:{seconds:D2}";
            timeLabel.RectPosition = new Vector2(15, 90);
            timeLabel.AddColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f, 1f));
            bg.AddChild(timeLabel);

            // Rewards
            string rewardText = "🎁 奖励: ";
            foreach (string reward in _event.rewards)
            {
                int amount = _event.rewardAmounts.ContainsKey(reward) ? _event.rewardAmounts[reward] : 1;
                rewardText += $"{reward} x{amount} ";
            }
            Label rewardLabel = new Label();
            rewardLabel.Text = rewardText;
            rewardLabel.RectPosition = new Vector2(250, 90);
            rewardLabel.AddColorOverride("font_color", new Color(1f, 0.9f, 0.5f, 1f));
            bg.AddChild(rewardLabel);

            // Participate button
            Button joinBtn = new Button();
            joinBtn.Text = _event.playerParticipated ? "已参与 ✓" : "参与";
            joinBtn.RectPosition = new Vector2(550, 85);
            joinBtn.RectMinSize = new Vector2(100, 30);
            joinBtn.Disabled = _event.playerParticipated;

            if (!_event.playerParticipated)
            {
                joinBtn.Connect("pressed", this, nameof(OnJoinPressed));
            }

            bg.AddChild(joinBtn);

            // Participants
            Label partLabel = new Label();
            partLabel.Text = $"👥 参与人数: {_event.participantCount}";
            partLabel.RectPosition = new Vector2(15, 115);
            partLabel.AddColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f, 1f));
            bg.AddChild(partLabel);
        }

        private void OnJoinPressed()
        {
            WorldEventSystem.Instance.ParticipateInEvent(_event.id);
        }
    }
}
