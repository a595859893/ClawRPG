using Godot;
/// <summary>
/// 元素反应用户界面。
/// </summary>
using System;
using System.Collections.Generic;
using static ElementalReactionData;

public class ElementalReactionUI : Control
{
    private PanelContainer _mainPanel;
    private VBoxContainer _content;
    private TabContainer _tabContainer;

    // 反应图鉴标签页
    private ScrollContainer _reactionBookTab;
    private GridContainer _reactionGrid;

    // 当前状态标签页
    private ScrollContainer _currentStatusTab;
    private VBoxContainer _statusContent;

    // 统计标签页
    private ScrollContainer _statsTab;
    private VBoxContainer _statsContent;

    private bool _isVisible = false;
    private const string TOGGLE_KEY = "elemental_reaction";

    public override void _Ready()
    {
        SetupUI();

        // 初始隐藏
        Hide();

        GD.Print("[ElementalReactionUI] Initialized - Press E to toggle");
    }

    public override void _Process(double delta)
    {
        // 检查快捷键
        if (Input.IsActionJustPressed("elemental_reaction_toggle"))
        {
            Toggle();
        }
    }

    private void SetupUI()
    {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_mainPanel);

        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        styleBox.BorderWidthLeft = 2;
        styleBox.BorderWidthRight = 2;
        styleBox.BorderWidthTop = 2;
        styleBox.BorderWidthBottom = 2;
        styleBox.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        styleBox.CornerRadiusTopLeft = 8;
        styleBox.CornerRadiusTopRight = 8;
        styleBox.CornerRadiusBottomLeft = 8;
        styleBox.CornerRadiusBottomRight = 8;
        _mainPanel.AddThemeStyleboxOverride("panel", styleBox);

        // 标题栏
        var titleBar = new HBoxContainer();
        _mainPanel.AddChild(titleBar);

        var title = new Label();
        title.Text = "  ⚡ Elemental Reactions  ⚡";
        title.AddThemeFontSizeOverride("font_size", 20);
        title.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
        titleBar.AddChild(title);

        var spacer = new Control();
        spacer.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        titleBar.AddChild(spacer);

        var closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.TooltipText = "Close (ESC)";
        closeBtn.Pressed += OnClosePressed;
        titleBar.AddChild(closeBtn);

        // 内容区域
        _content = new VBoxContainer();
        _content.SetSeparation(10);
        _mainPanel.AddChild(_content);

        // 标签页容器
        _tabContainer = new TabContainer();
        _tabContainer.SetSizeFlagsVertical(Control.SizeFlags.Expand);
        _content.AddChild(_tabContainer);

        // 创建三个标签页
        CreateReactionBookTab();
        CreateCurrentStatusTab();
        CreateStatsTab();
    }

    private void CreateReactionBookTab()
    {
        _reactionBookTab = new ScrollContainer();
        _reactionBookTab.Name = "ReactionBook";
        _tabContainer.AddChild(_reactionBookTab);

        var tabLabel = new Label();
        tabLabel.Text = "📖 Reaction Book";
        _tabContainer.SetTabTitle(_reactionBookTab, "📖 Reaction Book");

        _reactionGrid = new GridContainer();
        _reactionGrid.Columns = 3;
        _reactionGrid.SetSeparation(10);
        _reactionGrid.AddThemeConstantOverride("separation", 10);
        _reactionBookTab.AddChild(_reactionGrid);

        // 添加所有反应卡片
        var reactions = ElementalReactionDatabase.Instance.Reactions;
        foreach (var reaction in reactions)
        {
            var card = CreateReactionCard(reaction);
            _reactionGrid.AddChild(card);
        }
    }

    private Control CreateReactionCard(ReactionConfig config)
    {
        var card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(240, 140);

        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.15f, 0.15f, 0.2f);
        styleBox.BorderWidthLeft = 1;
        styleBox.BorderWidthRight = 1;
        styleBox.BorderWidthTop = 1;
        styleBox.BorderWidthBottom = 1;
        styleBox.BorderColor = ElementalReactionDatabase.Instance.GetElementColor(config.Element1).Blend(
            ElementalReactionDatabase.Instance.GetElementColor(config.Element2), 0.5f);
        styleBox.CornerRadiusTopLeft = 6;
        styleBox.CornerRadiusTopRight = 6;
        styleBox.CornerRadiusBottomLeft = 6;
        styleBox.CornerRadiusBottomRight = 6;
        card.AddThemeStyleboxOverride("panel", styleBox);

        var vbox = new VBoxContainer();
        vbox.SetSeparation(5);
        card.AddChild(vbox);

        // 反应名称
        var nameLabel = new Label();
        nameLabel.Text = config.Type.ToString();
        nameLabel.AddThemeFontSizeOverride("font_size", 14);
        nameLabel.AddThemeColorOverride("font_color", new Color(1f, 0.85f, 0.4f));
        vbox.AddChild(nameLabel);

        // 元素组合
        var elementRow = new HBoxContainer();
        vbox.AddChild(elementRow);

        var elem1Color = ElementalReactionDatabase.Instance.GetElementColor(config.Element1);
        var elem1Label = new Label();
        elem1Label.Text = $"● {config.Element1}";
        elem1Label.AddThemeColorOverride("font_color", elem1Color);
        elementRow.AddChild(elem1Label);

        var plusLabel = new Label();
        plusLabel.Text = " + ";
        plusLabel.AddThemeColorOverride("font_color", Colors.Gray);
        elementRow.AddChild(plusLabel);

        var elem2Color = ElementalReactionDatabase.Instance.GetElementColor(config.Element2);
        var elem2Label = new Label();
        elem2Label.Text = $"● {config.Element2}";
        elem2Label.AddThemeColorOverride("font_color", elem2Color);
        elementRow.AddChild(elem2Label);

        // 效果描述
        var effectLabel = new Label();
        string effectText = $"DMG: {config.BaseDamage * config.DamageMultiplier:F0}";
        if (config.ControlDuration > 0)
            effectText += $"\nStun: {config.ControlDuration}s";
        if (config.DotDamage > 0)
            effectText += $"\nDoT: {config.DotDamage}/s × {config.DotDuration}s";
        if (config.StatModifier != 0)
            effectText += $"\nStat: {(config.StatModifier > 0 ? "+" : "")}{config.StatModifier * 100:F0}%";
        if (config.IsAOE)
            effectText += $"\nAOE: {config.AORadius:F0}";

        effectLabel.Text = effectText;
        effectLabel.AddThemeFontSizeOverride("font_size", 11);
        effectLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.85f));
        vbox.AddChild(effectLabel);

        return card;
    }

    private void CreateCurrentStatusTab()
    {
        _currentStatusTab = new ScrollContainer();
        _currentStatusTab.Name = "CurrentStatus";
        _tabContainer.AddChild(_currentStatusTab);

        var tabLabel = new Label();
        tabLabel.Text = "🎯 Current Status";
        _tabContainer.SetTabTitle(_currentStatusTab, "🎯 Current Status");

        _statusContent = new VBoxContainer();
        _statusContent.SetSeparation(10);
        _currentStatusTab.AddChild(_statusContent);

        // 玩家元素亲和
        var affinitySection = CreateSection("✨ Elemental Affinity");
        _statusContent.AddChild(affinitySection);

        var playerState = ElementalReactionSystem.Instance.PlayerState;
        if (playerState.ElementalAffinity != null && playerState.ElementalAffinity.Count > 0)
        {
            foreach (var kvp in playerState.ElementalAffinity)
            {
                var label = new Label();
                label.Text = $"  {kvp.Key}: +{kvp.Value * 100:F1}%";
                label.AddThemeColorOverride("font_color", ElementalReactionDatabase.Instance.GetElementColor(kvp.Key));
                affinitySection.AddChild(label);
            }
        }
        else
        {
            var label = new Label();
            label.Text = "  No affinity bonus active";
            label.AddThemeColorOverride("font_color", Colors.Gray);
            affinitySection.AddChild(label);
        }

        // 说明
        var infoSection = CreateSection("💡 How It Works");
        _statusContent.AddChild(infoSection);

        var infoLabel = new Label();
        infoLabel.Text = "  • Apply elements to enemies\n" +
                        "  • When 2+ elements combine, reactions trigger\n" +
                        "  • Build element affinity for bonus damage\n" +
                        "  • AOE reactions affect nearby enemies";
        infoLabel.AddThemeFontSizeOverride("font_size", 12);
        infoLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.75f));
        infoSection.AddChild(infoLabel);
    }

    private void CreateStatsTab()
    {
        _statsTab = new ScrollContainer();
        _statsTab.Name = "Stats";
        _tabContainer.AddChild(_statsTab);

        var tabLabel = new Label();
        tabLabel.Text = "📊 Statistics";
        _tabContainer.SetTabTitle(_statsTab, "📊 Statistics");

        _statsContent = new VBoxContainer();
        _statsContent.SetSeparation(10);
        _statsTab.AddChild(_statsContent);

        RefreshStats();
    }

    private void RefreshStats()
    {
        _statsContent.GetChildren().ForEach(c => c.QueueFree());

        var stats = ElementalReactionSystem.Instance.GetReactionStats();

        // 总计
        var totalSection = CreateSection("📈 Total Statistics");
        _statsContent.AddChild(totalSection);

        var totalReactions = new Label();
        totalReactions.Text = $"  Total Reactions: {stats["TotalReactions"]}";
        totalReactions.AddThemeFontSizeOverride("font_size", 16);
        totalReactions.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
        totalSection.AddChild(totalReactions);

        var totalDamage = new Label();
        totalDamage.Text = $"  Total Damage: {stats["TotalDamage"]:F0}";
        totalDamage.AddThemeFontSizeOverride("font_size", 14);
        totalDamage.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.3f));
        totalSection.AddChild(totalDamage);

        // 按类型统计
        var breakdownSection = CreateSection("📋 Reaction Breakdown");
        _statsContent.AddChild(breakdownSection);

        int count = 0;
        foreach (var kvp in stats)
        {
            if (kvp.Key == "TotalReactions" || kvp.Key == "TotalDamage")
                continue;

            count++;
            var label = new Label();
            label.Text = $"  {kvp.Key}: {kvp.Value}";
            label.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.9f));
            breakdownSection.AddChild(label);
        }

        if (count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "  No reactions yet";
            emptyLabel.AddThemeColorOverride("font_color", Colors.Gray);
            breakdownSection.AddChild(emptyLabel);
        }

        // 重置按钮
        var resetBtn = new Button();
        resetBtn.Text = "🔄 Reset Statistics";
        resetBtn.Pressed += OnResetPressed;
        _statsContent.AddChild(resetBtn);
    }

    private Control CreateSection(string title)
    {
        var section = new VBoxContainer();

        var titleLabel = new Label();
        titleLabel.Text = title;
        titleLabel.AddThemeFontSizeOverride("font_size", 14);
        titleLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
        section.AddChild(titleLabel);

        var separator = new HSeparator();
        separator.AddThemeColorOverride("separator", new Color(0.3f, 0.3f, 0.4f));
        section.AddChild(separator);

        return section;
    }

    private void OnTogglePressed()
    {
        Toggle();
    }

    public void Toggle()
    {
        if (_isVisible)
            HideUI();
        else
            ShowUI();
    }

    public void ShowUI()
    {
        base.Show();
        _isVisible = true;
        RefreshStats();
    }

    public void HideUI()
    {
        base.Hide();
        _isVisible = false;
    }

    private void OnClosePressed()
    {
        Hide();
    }

    private void OnResetPressed()
    {
        ElementalReactionSystem.Instance.ResetStats();
        RefreshStats();
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == KeyCode.Escape)
        {
            if (_isVisible)
            {
                Hide();
                GetTree().SetInputAsHandled();
            }
        }
    }
}
