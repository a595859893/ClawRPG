using Godot;
using System;
using System.Collections.Generic;

public partial class DreamRealmUI : Control
{
    private DreamRealmSystem _dreamSystem;

    // UI 组件
    private Label _titleLabel;
    private Label _realmLabel;
    private Label _levelLabel;
    private Label _timeLabel;
    private Label _enemiesLabel;
    private Label _treasuresLabel;
    private Label _powerLabel;

    private VBoxContainer _realmListContainer;
    private VBoxContainer _buffListContainer;
    private Button _enterButton;
    private Button _exitButton;
    private Button _closeButton;
    private TabContainer _tabContainer;

    // 预设
    private Color _commonColor = new Color(0.7f, 0.7f, 0.7f);
    private Color _uncommonColor = new Color(0.2f, 0.8f, 0.2f);
    private Color _rareColor = new Color(0.2f, 0.5f, 1.0f);
    private Color _epicColor = new Color(0.6f, 0.3f, 0.9f);
    private Color _legendaryColor = new Color(1.0f, 0.7f, 0.0f);

    private Color _panelBgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
    private Color _borderColor = new Color(0.3f, 0.3f, 0.5f);
    private Color _titleColor = new Color(0.9f, 0.7f, 1.0f);

    public override void _Ready()
    {
        _dreamSystem = DreamRealmSystem.Instance;
        SetupUI();
        ConnectSignals();
        Visible = false;
    }

    private void SetupUI()
    {
        // 主容器
        var mainContainer = new VBoxContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 50,
            OffsetTop = 50,
            OffsetRight = -50,
            OffsetBottom = -50
        };
        AddChild(mainContainer);

        // 标题
        _titleLabel = new Label
        {
            Text = "🌙 Dream Realm - 梦境领域",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 60)
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 32);
        _titleLabel.AddThemeColorOverride("font_color", _titleColor);
        mainContainer.AddChild(_titleLabel);

        // TabContainer
        _tabContainer = new TabContainer
        {
            SizeFlagsHorizontal = SizeFlags.ExpandFill,
            SizeFlagsVertical = SizeFlags.ExpandFill
        };
        mainContainer.AddChild(_tabContainer);

        // 创建标签页
        CreateOverviewTab();
        CreateRealmsTab();
        CreateBuffsTab();
        CreateStatsTab();

        // 按钮行
        var buttonContainer = new HBoxContainer
        {
            Alignment = BoxContainer.Alignment.Center,
            CustomMinimumSize = new Vector2(0, 60)
        };
        mainContainer.AddChild(buttonContainer);

        _enterButton = new Button
        {
            Text = "  🌙 Enter Dream ",
            CustomMinimumSize = new Vector2(180, 45)
        };
        _enterButton.Pressed += OnEnterPressed;
        buttonContainer.AddChild(_enterButton);

        _exitButton = new Button
        {
            Text = "  💫 Exit Dream ",
            CustomMinimumSize = new Vector2(180, 45),
            Disabled = true
        };
        _exitButton.Pressed += OnExitPressed;
        buttonContainer.AddChild(_exitButton);

        _closeButton = new Button
        {
            Text = "  ❌ Close ",
            CustomMinimumSize = new Vector2(120, 45)
        };
        _closeButton.Pressed += OnClosePressed;
        buttonContainer.AddChild(_closeButton);
    }

    private void CreateOverviewTab()
    {
        var tab = new ScrollContainer
        {
            Name = "Overview"
        };
        _tabContainer.AddChild(tab);

        var container = new VBoxContainer
        {
            OffsetLeft = 20,
            OffsetTop = 20,
            OffsetRight = -20,
            OffsetBottom = -20
        };
        tab.AddChild(container);

        // 当前状态
        var statusPanel = CreateStatPanel("Current Status", _panelBgColor);
        container.AddChild(statusPanel);

        var statusGrid = new GridContainer { Columns = 2 };
        statusPanel.AddChild(statusGrid);

        _realmLabel = CreateStatLabel("Realm: ");
        statusGrid.AddChild(CreateStatRow("Realm:", _realmLabel));

        _levelLabel = CreateStatLabel("Level: ");
        statusGrid.AddChild(CreateStatRow("Level:", _levelLabel));

        _timeLabel = CreateStatLabel("Time: ");
        statusGrid.AddChild(CreateStatRow("Time:", _timeLabel));

        _enemiesLabel = CreateStatLabel("Enemies: ");
        statusGrid.AddChild(CreateStatRow("Enemies:", _enemiesLabel));

        _treasuresLabel = CreateStatLabel("Treasures: ");
        statusGrid.AddChild(CreateStatRow("Treasures:", _treasuresLabel));

        _powerLabel = CreateStatLabel("Power: ");
        statusGrid.AddChild(CreateStatRow("Power:", _powerLabel));

        // 提示
        var hintLabel = new Label
        {
            Text = "\n📖 Tips:\n• Defeat enemies to gain power\n• Find treasures for rare rewards\n• Stay longer for bonus multipliers",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        hintLabel.AddThemeFontSizeOverride("font_size", 14);
        hintLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        container.AddChild(hintLabel);
    }

    private void CreateRealmsTab()
    {
        var tab = new ScrollContainer
        {
            Name = "Realms"
        };
        _tabContainer.AddChild(tab);

        _realmListContainer = new VBoxContainer
        {
            OffsetLeft = 20,
            OffsetTop = 20,
            OffsetRight = -20,
            OffsetBottom = -20
        };
        tab.AddChild(_realmListContainer);

        RefreshRealmList();
    }

    private void CreateBuffsTab()
    {
        var tab = new ScrollContainer
        {
            Name = "Buffs"
        };
        _tabContainer.AddChild(tab);

        _buffListContainer = new VBoxContainer
        {
            OffsetLeft = 20,
            OffsetTop = 20,
            OffsetRight = -20,
            OffsetBottom = -20
        };
        tab.AddChild(_buffListContainer);

        RefreshBuffList();
    }

    private void CreateStatsTab()
    {
        var tab = new ScrollContainer
        {
            Name = "Stats"
        };
        _tabContainer.AddChild(tab);

        var container = new VBoxContainer
        {
            OffsetLeft = 20,
            OffsetTop = 20,
            OffsetRight = -20,
            OffsetBottom = -20
        };
        tab.AddChild(container);

        var title = new Label
        {
            Text = "📊 Realm Statistics",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 24);
        title.AddThemeColorOverride("font_color", _titleColor);
        container.AddChild(title);

        // 统计信息将动态生成
    }

    private Panel CreateStatPanel(string title, Color bgColor)
    {
        var panel = new Panel
        {
            CustomMinimumSize = new Vector2(0, 100)
        };
        panel.AddThemeStyleBoxOverride("panel", CreateStyleBox(bgColor, _borderColor));

        var label = new Label
        {
            Text = title,
            VerticalAlignment = VerticalAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            OffsetTop = 10
        };
        label.AddThemeFontSizeOverride("font_size", 20);
        label.AddThemeColorOverride("font_color", _titleColor);
        panel.AddChild(label);

        return panel;
    }

    private StyleBoxFlat CreateStyleBox(Color bgColor, Color borderColor)
    {
        var style = new StyleBoxFlat
        {
            BgColor = bgColor,
            BorderWidthLeft = 2,
            BorderWidthTop = 2,
            BorderWidthRight = 2,
            BorderWidthBottom = 2,
            BorderColor = borderColor,
            CornerRadiusTopLeft = 10,
            CornerRadiusTopRight = 10,
            CornerRadiusBottomLeft = 10,
            CornerRadiusBottomRight = 10
        };
        return style;
    }

    private Label CreateStatLabel(string prefix)
    {
        var label = new Label
        {
            Text = prefix,
            HorizontalAlignment = HorizontalAlignment.Left
        };
        label.AddThemeFontSizeOverride("font_size", 16);
        label.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
        return label;
    }

    private HBoxContainer CreateStatRow(string labelText, Label valueLabel)
    {
        var container = new HBoxContainer { CustomMinimumSize = new Vector2(0, 35) };

        var label = new Label
        {
            Text = labelText,
            HorizontalAlignment = HorizontalAlignment.Left,
            SizeFlagsHorizontal = SizeFlags.ExpandFill
        };
        label.AddThemeFontSizeOverride("font_size", 16);
        label.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        container.AddChild(label);

        valueLabel.HorizontalAlignment = HorizontalAlignment.Right;
        container.AddChild(valueLabel);

        return container;
    }

    private void ConnectSignals()
    {
        if (_dreamSystem != null)
        {
            _dreamSystem.DreamEntered += OnDreamEntered;
            _dreamSystem.DreamExited += OnDreamExited;
            _dreamSystem.LevelUp += OnLevelUp;
            _dreamSystem.TreasureFound += OnTreasureFound;
        }
    }

    public override void _Process(double delta)
    {
        if (Visible && _dreamSystem != null)
        {
            UpdateDisplay();
        }
    }

    private void UpdateDisplay()
    {
        if (_dreamSystem == null) return;

        // 更新状态显示
        _realmLabel.Text = _dreamSystem.IsInDream ?
            _dreamSystem.GetRealmInfo(_dreamSystem.CurrentRealm)["name"].ToString() :
            "Not in Dream";

        _levelLabel.Text = $"Lv.{_dreamSystem.DreamLevel}";
        _timeLabel.Text = FormatTime(_dreamSystem.TimeInDream);
        _enemiesLabel.Text = _dreamSystem.EnemiesDefeated.ToString();
        _treasuresLabel.Text = _dreamSystem.TreasuresFound.ToString();
        _powerLabel.Text = $"{_dreamSystem.DreamPowerMultiplier:F2}x";

        // 更新按钮状态
        _enterButton.Disabled = _dreamSystem.IsInDream;
        _exitButton.Disabled = !_dreamSystem.IsInDream;
    }

    private void RefreshRealmList()
    {
        if (_realmListContainer == null || _dreamSystem == null) return;

        foreach (var child in _realmListContainer.GetChildren())
        {
            child.QueueFree();
        }

        foreach (var realm in Enum.GetValues(typeof(DreamRealmSystem.RealmType)))
        {
            var realmType = (DreamRealmSystem.RealmType)realm;
            var info = _dreamSystem.GetRealmInfo(realmType);
            bool unlocked = (bool)info["unlocked"];

            var realmPanel = new Panel
            {
                CustomMinimumSize = new Vector2(0, 80)
            };
            realmPanel.AddThemeStyleBoxOverride("panel", CreateStyleBox(
                unlocked ? new Color(0.15f, 0.15f, 0.2f, 0.95f) : new Color(0.1f, 0.1f, 0.12f, 0.95f),
                unlocked ? _borderColor : new Color(0.2f, 0.2f, 0.2f)));

            var container = new HBoxContainer { OffsetLeft = 15, OffsetTop = 10, OffsetRight = -15, OffsetBottom = -10 };
            realmPanel.AddChild(container);

            // 图标
            var iconLabel = new Label
            {
                Text = unlocked ? "🌙" : "🔒",
                CustomMinimumSize = new Vector2(40, 0),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center
            };
            iconLabel.AddThemeFontSizeOverride("font_size", 24);
            container.AddChild(iconLabel);

            // 信息
            var infoContainer = new VBoxContainer
            {
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            container.AddChild(infoContainer);

            var nameLabel = new Label
            {
                Text = info["name"].ToString(),
                HorizontalAlignment = HorizontalAlignment.Left
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            nameLabel.AddThemeColorOverride("font_color", unlocked ? _titleColor : new Color(0.4f, 0.4f, 0.4f));
            infoContainer.AddChild(nameLabel);

            var descLabel = new Label
            {
                Text = info["description"].ToString(),
                HorizontalAlignment = HorizontalAlignment.Left,
                AutowrapMode = TextServer.AutowrapMode.Word
            };
            descLabel.AddThemeFontSizeOverride("font_size", 12);
            descLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            infoContainer.AddChild(descLabel);

            var statsLabel = new Label
            {
                Text = $"Visits: {info["timesVisited"]} | Difficulty: {info["difficulty"]}",
                HorizontalAlignment = HorizontalAlignment.Left
            };
            statsLabel.AddThemeFontSizeOverride("font_size", 12);
            statsLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            infoContainer.AddChild(statsLabel);

            // 进入按钮
            if (unlocked)
            {
                var enterRealmBtn = new Button
                {
                    Text = "Enter",
                    CustomMinimumSize = new Vector2(80, 35)
                };
                enterRealmBtn.Pressed += () => OnEnterRealmPressed(realmType);
                container.AddChild(enterRealmBtn);
            }

            _realmListContainer.AddChild(realmPanel);
        }
    }

    private void RefreshBuffList()
    {
        if (_buffListContainer == null || _dreamSystem == null) return;

        foreach (var child in _buffListContainer.GetChildren())
        {
            child.QueueFree();
        }

        var buffs = new[] { "attack", "defense", "speed", "luck", "experience" };
        var buffNames = new Dictionary<string, string>
        {
            { "attack", "⚔️ Attack" },
            { "defense", "🛡️ Defense" },
            { "speed", "⚡ Speed" },
            { "luck", "🍀 Luck" },
            { "experience", "✨ Experience" }
        };

        foreach (var buff in buffs)
        {
            float value = _dreamSystem.GetDreamBuff(buff);
            var buffPanel = new Panel
            {
                CustomMinimumSize = new Vector2(0, 50)
            };
            buffPanel.AddThemeStyleBoxOverride("panel", CreateStyleBox(_panelBgColor, _borderColor));

            var container = new HBoxContainer { OffsetLeft = 15, OffsetTop = 5, OffsetRight = -15, OffsetBottom = -5 };
            buffPanel.AddChild(container);

            var nameLabel = new Label
            {
                Text = buffNames[buff],
                SizeFlagsHorizontal = SizeFlags.ExpandFill
            };
            nameLabel.AddThemeFontSizeOverride("font_size", 16);
            nameLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.9f, 0.9f));
            container.AddChild(nameLabel);

            var valueLabel = new Label
            {
                Text = $"{value:F2}x"
            };
            valueLabel.AddThemeFontSizeOverride("font_size", 18);
            valueLabel.AddThemeColorOverride("font_color", value > 1.0f ? _legendaryColor : new Color(0.7f, 0.7f, 0.7f));
            container.AddChild(valueLabel);

            _buffListContainer.AddChild(buffPanel);
        }
    }

    private string FormatTime(float seconds)
    {
        int mins = (int)(seconds / 60);
        int secs = (int)(seconds % 60);
        return $"{mins:D2}:{secs:D2}";
    }

    private void OnEnterPressed()
    {
        if (_dreamSystem != null && !_dreamSystem.IsInDream)
        {
            _dreamSystem.EnterDream(DreamRealmSystem.RealmType.NightmareForest);
        }
    }

    private void OnEnterRealmPressed(DreamRealmSystem.RealmType realm)
    {
        if (_dreamSystem != null && !_dreamSystem.IsInDream)
        {
            _dreamSystem.EnterDream(realm);
        }
    }

    private void OnExitPressed()
    {
        if (_dreamSystem != null && _dreamSystem.IsInDream)
        {
            _dreamSystem.ExitDream();
        }
    }

    private void OnClosePressed()
    {
        Visible = false;
    }

    private void OnDreamEntered(DreamRealmSystem.RealmType realm)
    {
        UpdateDisplay();
        RefreshRealmList();
    }

    private void OnDreamExited()
    {
        UpdateDisplay();
        RefreshRealmList();
    }

    private void OnLevelUp(int newLevel)
    {
        UpdateDisplay();
    }

    private void OnTreasureFound(DreamRealmSystem.DreamTreasure treasure)
    {
        UpdateDisplay();
        RefreshBuffList();
    }

    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            UpdateDisplay();
            RefreshRealmList();
            RefreshBuffList();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.D)
        {
            if (Visible)
            {
                Toggle();
            }
        }
    }
}
