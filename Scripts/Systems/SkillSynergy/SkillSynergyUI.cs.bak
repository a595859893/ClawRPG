using Godot;
using System;
using System.Collections.Generic;

public partial class SkillSynergyUI : Control
{
    private Control _mainContainer;
    private VBoxContainer _synergyListContainer;
    private VBoxContainer _activeContainer;
    private VBoxContainer _statsContainer;
    private TabContainer _tabContainer;

    private Label _comboChainLabel;
    private Label _activeCountLabel;

    private bool _isVisible = false;
    private Font _defaultFont;

    public override void _Ready()
    {
        _defaultFont = GD.Load<Font>("res://fonts/default_font.ttf");
        SetupUI();
        Hide();
    }

    private void SetupUI()
    {
        // 主容器
        _mainContainer = new Control();
        _mainContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        AddChild(_mainContainer);

        // 背景面板
        var bgPanel = new Panel();
        bgPanel.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        bgPanel.Modulate = new Color(0, 0, 0, 0.85f);
        _mainContainer.AddChild(bgPanel);

        // 标题栏
        var titleBar = new HBoxContainer();
        titleBar.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
        titleBar.Position = new Vector2(0, 0);
        titleBar.Size = new Vector2(1152, 60);
        _mainContainer.AddChild(titleBar);

        var titleLabel = new Label();
        titleLabel.Text = "⚔️ Skill Synergy System";
        titleLabel.AddThemeFontSizeOverride("font_size", 28);
        titleBar.AddChild(titleLabel);

        titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        // 连击链显示
        _comboChainLabel = new Label();
        _comboChainLabel.Text = "Combo: 0";
        _comboChainLabel.AddThemeFontSizeOverride("font_size", 20);
        titleBar.AddChild(_comboChainLabel);

        // 活跃协同数
        _activeCountLabel = new Label();
        _activeCountLabel.Text = "Active: 0";
        _activeCountLabel.AddThemeFontSizeOverride("font_size", 20);
        titleBar.AddChild(_activeCountLabel);

        // 关闭按钮
        var closeBtn = new Button();
        closeBtn.Text = "✕";
        closeBtn.RectMinSize = new Vector2(50, 40);
        closeBtn.Pressed += () => ToggleVisibility();
        titleBar.AddChild(closeBtn);

        // Tab 容器
        _tabContainer = new TabContainer();
        _tabContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        _tabContainer.Position = new Vector2(20, 80);
        _tabContainer.Size = new Vector2(1112, 580);
        _mainContainer.AddChild(_tabContainer);

        // ============ 协同列表标签页 ============
        var synergyListTab = new ScrollContainer();
        synergyListTab.Name = "All Synergies";
        _tabContainer.AddChild(synergyListTab);

        _synergyListContainer = new VBoxContainer();
        _synergyListContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        _synergyListContainer.Position = new Vector2(10, 10);
        _synergyListContainer.Size = new Vector2(1070, 540);
        synergyListTab.AddChild(_synergyListContainer);

        // ============ 活跃协同标签页 ============
        var activeTab = new ScrollContainer();
        activeTab.Name = "Active";
        _tabContainer.AddChild(activeTab);

        _activeContainer = new VBoxContainer();
        _activeContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        _activeContainer.Position = new Vector2(10, 10);
        _activeContainer.Size = new Vector2(1070, 540);
        activeTab.AddChild(_activeContainer);

        // ============ 统计标签页 ============
        var statsTab = new ScrollContainer();
        statsTab.Name = "Statistics";
        _tabContainer.AddChild(statsTab);

        _statsContainer = new VBoxContainer();
        _statsContainer.SetAnchorsPreset(Control.AnchorsPreset.FullRect);
        _statsContainer.Position = new Vector2(10, 10);
        _statsContainer.Size = new Vector2(1070, 540);
        statsTab.AddChild(_statsContainer);

        // 刷新 UI
        RefreshUI();
    }

    private void RefreshUI()
    {
        RefreshSynergyList();
        RefreshActiveSynergies();
        RefreshStatistics();
        UpdateLabels();
    }

    private void UpdateLabels()
    {
        if (SkillSynergySystem.Instance != null)
        {
            _comboChainLabel.Text = $"Combo: {SkillSynergySystem.Instance.GetCurrentComboChain()}";
            var active = SkillSynergySystem.Instance.GetActiveSynergies();
            _activeCountLabel.Text = $"Active: {active.Count}";
        }
    }

    private void RefreshSynergyList()
    {
        // 清空
        foreach (var child in _synergyListContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (SkillSynergySystem.Instance == null) return;

        var synergies = SkillSynergySystem.Instance.GetAllSynergies();

        foreach (var kvp in synergies)
        {
            var config = kvp.Value;
            var card = CreateSynergyCard(config);
            _synergyListContainer.AddChild(card);
        }
    }

    private Control CreateSynergyCard(SkillSynergyDatabase.SynergyConfig config)
    {
        var card = new PanelContainer();
        card.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
        card.CustomMinimumSize = new Vector2(0, 100);
        card.Modulate = new Color(1, 1, 1, 0.9f);

        var hbox = new HBoxContainer();
        card.AddChild(hbox);

        // 左侧：图标/稀有度指示
        var rarityIndicator = new Panel();
        rarityIndicator.CustomMinimumSize = new Vector2(8, 80);
        var rarityColor = _GetRarityColor(config.Rarity);
        rarityIndicator.Modulate = rarityColor;
        hbox.AddChild(rarityIndicator);

        // 中间：信息
        var infoVBox = new VBoxContainer();
        infoVBox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
        hbox.AddChild(infoVBox);

        // 名称和类型
        var nameRow = new HBoxContainer();
        infoVBox.AddChild(nameRow);

        var nameLabel = new Label();
        nameLabel.Text = config.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 18);
        nameLabel.Modulate = rarityColor;
        nameRow.AddChild(nameLabel);

        nameRow.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        var typeLabel = new Label();
        typeLabel.Text = $"{config.Type} | {config.Rarity}";
        typeLabel.AddThemeFontSizeOverride("font_size", 14);
        nameRow.AddChild(typeLabel);

        // 描述
        var descLabel = new Label();
        descLabel.Text = config.Description;
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        infoVBox.AddChild(descLabel);

        // 所需技能序列
        var skillsLabel = new Label();
        skillsLabel.Text = $"Skills: {string.Join(" → ", config.RequiredSkills)}";
        skillsLabel.AddThemeFontSizeOverride("font_size", 12);
        skillsLabel.Modulate = new Color(0.7f, 0.7f, 0.7f);
        infoVBox.AddChild(skillsLabel);

        // 右侧：效果和冷却
        var effectVBox = new VBoxContainer();
        hbox.AddChild(effectVBox);

        var durationLabel = new Label();
        durationLabel.Text = $"Duration: {config.Duration}s";
        durationLabel.AddThemeFontSizeOverride("font_size", 12);
        effectVBox.AddChild(durationLabel);

        var cooldownLabel = new Label();
        cooldownLabel.Text = $"Cooldown: {config.Cooldown}s";
        cooldownLabel.AddThemeFontSizeOverride("font_size", 12);
        effectVBox.AddChild(cooldownLabel);

        var stacksLabel = new Label();
        stacksLabel.Text = $"Max Stacks: {config.MaxStacks}";
        stacksLabel.AddThemeFontSizeOverride("font_size", 12);
        effectVBox.AddChild(stacksLabel);

        // 解锁进度
        var unlockProgress = SkillSynergySystem.Instance.GetUnlockProgress(config.Id);
        var progressLabel = new Label();
        progressLabel.Text = $"Progress: {unlockProgress}/{config.UnlockRequirement}";
        progressLabel.AddThemeFontSizeOverride("font_size", 12);
        progressLabel.Modulate = unlockProgress >= config.UnlockRequirement ? new Color(0, 1, 0) : new Color(1, 1, 0);
        effectVBox.AddChild(progressLabel);

        // 测试按钮
        var testBtn = new Button();
        testBtn.Text = "Test";
        testBtn.RectMinSize = new Vector2(60, 30);
        testBtn.Pressed += () => SkillSynergySystem.Instance.TestSynergy(config.Id);
        effectVBox.AddChild(testBtn);

        return card;
    }

    private void RefreshActiveSynergies()
    {
        // 清空
        foreach (var child in _activeContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (SkillSynergySystem.Instance == null) return;

        var activeSynergies = SkillSynergySystem.Instance.GetActiveSynergies();

        if (activeSynergies.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "No active synergies";
            emptyLabel.AddThemeFontSizeOverride("font_size", 20);
            emptyLabel.Modulate = new Color(0.5f, 0.5f, 0.5f);
            _activeContainer.AddChild(emptyLabel);
            return;
        }

        foreach (var kvp in activeSynergies)
        {
            var record = kvp.Value;
            var config = SkillSynergySystem.Instance.GetSynergyDetails(kvp.Key);

            var card = new PanelContainer();
            card.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
            card.CustomMinimumSize = new Vector2(0, 80);

            var hbox = new HBoxContainer();
            card.AddChild(hbox);

            // 稀有度条
            var rarityIndicator = new Panel();
            rarityIndicator.CustomMinimumSize = new Vector2(8, 60);
            if (config != null)
                rarityIndicator.Modulate = _GetRarityColor(config.Rarity);
            hbox.AddChild(rarityIndicator);

            // 信息
            var infoVBox = new VBoxContainer();
            infoVBox.SizeFlagsHorizontal = Control.SizeFlags.Expand;
            hbox.AddChild(infoVBox);

            var nameLabel = new Label();
            nameLabel.Text = $"{record.SynergyName} (x{record.TriggerCount}/{record.MaxStacks})";
            nameLabel.AddThemeFontSizeOverride("font_size", 18);
            infoVBox.AddChild(nameLabel);

            // 进度条
            var progressBar = new ProgressBar();
            progressBar.CustomMinimumSize = new Vector2(0, 20);
            progressBar.Value = (record.CurrentDuration / record.Duration) * 100;
            infoVBox.AddChild(progressBar);

            var timeLabel = new Label();
            timeLabel.Text = $"{record.CurrentDuration:F1}s / {record.Duration}s";
            timeLabel.AddThemeFontSizeOverride("font_size", 12);
            infoVBox.AddChild(timeLabel);

            _activeContainer.AddChild(card);
        }
    }

    private void RefreshStatistics()
    {
        // 清空
        foreach (var child in _statsContainer.GetChildren())
        {
            child.QueueFree();
        }

        if (SkillSynergySystem.Instance == null) return;

        var stats = SkillSynergySystem.Instance.GetStatistics();

        // 标题
        var titleLabel = new Label();
        titleLabel.Text = "Synergy Statistics";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _statsContainer.AddChild(titleLabel);

        _statsContainer.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 10) });

        // 统计网格
        var statsGrid = new GridContainer();
        statsGrid.Columns = 2;
        statsGrid.SetAnchorsPreset(Control.AnchorsPreset.TopWide);
        _statsContainer.AddChild(statsGrid);

        AddStatRow(statsGrid, "Total Synergies Triggered", stats.TotalSynergiesTriggered.ToString());
        AddStatRow(statsGrid, "Unique Synergies Discovered", stats.UniqueSynergiesDiscovered.ToString());
        AddStatRow(statsGrid, "Max Combo Chain", stats.MaxComboChain.ToString());
        AddStatRow(statsGrid, "Total Bonus Damage", $"{stats.TotalBonusDamage:F1}%");
        AddStatRow(statsGrid, "Total Bonus Healing", $"{stats.TotalBonusHealing:F1}%");

        _statsContainer.AddChild(new Control() { CustomMinimumSize = new Vector2(0, 20) });

        // 触发历史
        var historyLabel = new Label();
        historyLabel.Text = "Trigger History";
        historyLabel.AddThemeFontSizeOverride("font_size", 20);
        _statsContainer.AddChild(historyLabel);

        var history = SkillSynergySystem.Instance.GetSynergyStats();
        foreach (var kvp in history)
        {
            var config = SkillSynergySystem.Instance.GetSynergyDetails(kvp.Key);
            var name = config != null ? config.Name : kvp.Key;

            var triggerLabel = new Label();
            triggerLabel.Text = $"{name}: {kvp.Value} times";
            triggerLabel.AddThemeFontSizeOverride("font_size", 14);
            _statsContainer.AddChild(triggerLabel);
        }
    }

    private void AddStatRow(GridContainer grid, string label, string value)
    {
        var labelNode = new Label();
        labelNode.Text = label;
        labelNode.AddThemeFontSizeOverride("font_size", 16);
        grid.AddChild(labelNode);

        var valueNode = new Label();
        valueNode.Text = value;
        valueNode.AddThemeFontSizeOverride("font_size", 16);
        valueNode.HorizontalAlignment = HorizontalAlignment.Right;
        grid.AddChild(valueNode);
    }

    private Color _GetRarityColor(SkillSynergyDatabase.SynergyRarity rarity)
    {
        switch (rarity)
        {
            case SkillSynergyDatabase.SynergyRarity.Common:
                return new Color(0.69f, 0.69f, 0.69f);
            case SkillSynergyDatabase.SynergyRarity.Uncommon:
                return new Color(0.12f, 1f, 0f);
            case SkillSynergyDatabase.SynergyRarity.Rare:
                return new Color(0f, 0.44f, 0.87f);
            case SkillSynergyDatabase.SynergyRarity.Epic:
                return new Color(0.64f, 0.21f, 0.93f);
            case SkillSynergyDatabase.SynergyRarity.Legendary:
                return new Color(1f, 0.5f, 0f);
            default:
                return new Color(1f, 1f, 1f);
        }
    }

    public void ToggleVisibility()
    {
        _isVisible = !_isVisible;
        if (_isVisible)
        {
            RefreshUI();
            Show();
        }
        else
        {
            Hide();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // K 键切换显示
            if (keyEvent.Scancode == KeyList.K)
            {
                ToggleVisibility();
            }
            // ESC 关闭
            else if (keyEvent.Scancode == KeyList.Escape && _isVisible)
            {
                Hide();
                _isVisible = false;
            }
        }
    }

    public override void _Process(double delta)
    {
        if (_isVisible && SkillSynergySystem.Instance != null)
        {
            UpdateLabels();
            RefreshActiveSynergies();
        }
    }
}
