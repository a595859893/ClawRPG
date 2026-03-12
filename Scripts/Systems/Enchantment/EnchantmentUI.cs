using Godot;
using System;
using System.Collections.Generic;

public class EnchantmentUI : Control
{
    private PanelContainer _mainPanel;
    private VBoxContainer _contentBox;
    private TabContainer _tabContainer;

    // 标签页
    private ScrollContainer _enchantmentsList;
    private VBoxContainer _enchantmentsVBox;
    private ScrollContainer _appliedList;
    private VBoxContainer _appliedVBox;
    private ScrollContainer _statisticsPanel;
    private VBoxContainer _statisticsVBox;

    // 当前显示的附魔类型筛选
    private EnchantmentData.EnchantmentType? _currentFilter;

    public override void _Ready()
    {
        _currentFilter = null;
        SetupUI();
    }

    private void SetupUI()
    {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_mainPanel);

        var mainStyle = new StyleBoxFlat();
        mainStyle.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        mainStyle.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        mainStyle.SetBorderWidthAll(2);
        mainStyle.SetCornerRadiusAll(8);
        _mainPanel.AddThemeStyleboxOverride("panel", mainStyle);

        // 内容容器
        _contentBox = new VBoxContainer();
        _contentBox.SetCustomMinimumSize(new Vector2(780, 580));
        _contentBox.AddThemeConstantOverride("separation", 10);
        _mainPanel.AddChild(_contentBox);

        // 标题栏
        var titleBar = new HBoxContainer();
        titleBar.AddThemeConstantOverride("separation", 10);
        _contentBox.AddChild(titleBar);

        var titleLabel = new Label();
        titleLabel.Text = "附魔系统 - Enchantment System";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f));
        titleBar.AddChild(titleLabel);

        titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        // 快捷键提示
        var hintLabel = new Label();
        hintLabel.Text = "[E] 切换显示  [ESC] 关闭";
        hintLabel.AddThemeFontSizeOverride("font_size", 14);
        hintLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        titleBar.AddChild(hintLabel);

        // 标签页容器
        _tabContainer = new TabContainer();
        _tabContainer.SetSizeFlagsVertical(Control.SizeFlags.Expand);
        _contentBox.AddChild(_tabContainer);

        // 附魔列表标签页
        _enchantmentsList = new ScrollContainer();
        _enchantmentsList.Name = "附魔库";
        _tabContainer.AddChild(_enchantmentsList);

        _enchantmentsVBox = new VBoxContainer();
        _enchantmentsVBox.SetCustomMinimumSize(new Vector2(740, 500));
        _enchantmentsVBox.AddThemeConstantOverride("separation", 8);
        _enchantmentsList.AddChild(_enchantmentsVBox);

        // 类型筛选按钮
        var filterBar = new HBoxContainer();
        filterBar.AddThemeConstantOverride("separation", 10);
        _enchantmentsVBox.AddChild(filterBar);

        CreateFilterButton(filterBar, "全部", null);
        CreateFilterButton(filterBar, "武器", EnchantmentData.EnchantmentType.Weapon);
        CreateFilterButton(filterBar, "护甲", EnchantmentData.EnchantmentType.Armor);
        CreateFilterButton(filterBar, "饰品", EnchantmentData.EnchantmentType.Accessory);
        CreateFilterButton(filterBar, "头盔", EnchantmentData.EnchantmentType.Helmet);
        CreateFilterButton(filterBar, "鞋子", EnchantmentData.EnchantmentType.Boots);
        CreateFilterButton(filterBar, "手套", EnchantmentData.EnchantmentType.Gloves);

        // 刷新按钮
        var refreshBtn = new Button();
        refreshBtn.Text = "🔄 刷新附魔库";
        refreshBtn.Pressed += OnRefreshEnchantments;
        filterBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        filterBar.AddChild(refreshBtn);

        // 已应用附魔标签页
        _appliedList = new ScrollContainer();
        _appliedList.Name = "已应用";
        _tabContainer.AddChild(_appliedList);

        _appliedVBox = new VBoxContainer();
        _appliedVBox.SetCustomMinimumSize(new Vector2(740, 500));
        _appliedVBox.AddThemeConstantOverride("separation", 8);
        _appliedList.AddChild(_appliedVBox);

        // 统计标签页
        _statisticsPanel = new ScrollContainer();
        _statisticsPanel.Name = "统计";
        _tabContainer.AddChild(_statisticsPanel);

        _statisticsVBox = new VBoxContainer();
        _statisticsVBox.SetCustomMinimumSize(new Vector2(740, 500));
        _statisticsVBox.AddThemeConstantOverride("separation", 10);
        _statisticsPanel.AddChild(_statisticsVBox);

        // 初始加载
        RefreshEnchantmentList();
        RefreshAppliedList();
        RefreshStatistics();

        // 隐藏
        Hide();
    }

    private void CreateFilterButton(HBoxContainer parent, string text, EnchantmentData.EnchantmentType? type)
    {
        var btn = new Button();
        btn.Text = text;
        btn.Pressed += () => OnFilterSelected(type);
        parent.AddChild(btn);
    }

    private void OnFilterSelected(EnchantmentData.EnchantmentType? type)
    {
        _currentFilter = type;
        RefreshEnchantmentList();
    }

    private void OnRefreshEnchantments()
    {
        if (EnchantmentSystem.Instance != null)
        {
            // 假设玩家等级为 30
            EnchantmentSystem.Instance.DiscoverRandomEnchantment(30);
            RefreshEnchantmentList();
            RefreshStatistics();
        }
    }

    private void RefreshEnchantmentList()
    {
        // 清除现有内容（保留筛选栏）
        foreach (var child in _enchantmentsVBox.GetChildren())
        {
            if (child is HBoxContainer && child.GetIndex() == 0) continue;
            child.QueueFree();
        }

        var allEnchantments = EnchantmentDatabase.Instance.GetAllEnchantments();

        foreach (var enchantment in allEnchantments)
        {
            // 筛选
            if (_currentFilter.HasValue && enchantment.Type != _currentFilter.Value)
                continue;

            // 检查是否已解锁
            bool isUnlocked = EnchantmentSystem.Instance != null &&
                             EnchantmentSystem.Instance.IsUnlocked(enchantment.Id);

            if (!isUnlocked)
                continue;

            var card = CreateEnchantmentCard(enchantment);
            _enchantmentsVBox.AddChild(card);
        }
    }

    private Control CreateEnchantmentCard(EnchantmentData enchantment)
    {
        var card = new PanelContainer();
        card.SetCustomMinimumSize(new Vector2(720, 80));

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.15f, 0.2f);
        style.BorderColor = GetRarityColor(enchantment.RarityLevel);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(4);
        card.AddThemeStyleboxOverride("panel", style);

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 15);
        card.AddChild(hbox);

        // 图标占位
        var iconBg = new ColorRect();
        iconBg.Color = GetRarityColor(enchantment.RarityLevel);
        iconBg.SetCustomMinimumSize(new Vector2(60, 60));
        hbox.AddChild(iconBg);

        // 信息
        var infoBox = new VBoxContainer();
        infoBox.AddThemeConstantOverride("separation", 5);
        hbox.AddChild(infoBox);

        var nameLabel = new Label();
        nameLabel.Text = enchantment.Name;
        nameLabel.AddThemeFontSizeOverride("font_size", 18);
        nameLabel.AddThemeColorOverride("font_color", GetRarityColor(enchantment.RarityLevel));
        infoBox.AddChild(nameLabel);

        var descLabel = new Label();
        descLabel.Text = $"{GetTypeName(enchantment.Type)} | {enchantment.RarityLevel} | 需求等级: {enchantment.RequiredLevel}";
        descLabel.AddThemeFontSizeOverride("font_size", 14);
        descLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        infoBox.AddChild(descLabel);

        var propsLabel = new Label();
        var props = new List<string>();
        foreach (var prop in enchantment.Properties)
        {
            props.Add($"{GetPropertyName(prop.Key)} +{prop.Value}");
        }
        propsLabel.Text = string.Join(", ", props);
        propsLabel.AddThemeFontSizeOverride("font_size", 13);
        propsLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 0.6f));
        infoBox.AddChild(propsLabel);

        // 费用和成功率
        var statsBox = new VBoxContainer();
        statsBox.Alignment = BoxContainer.Alignment.End;
        statsBox.AddThemeConstantOverride("separation", 3);
        hbox.AddChild(statsBox);

        var costLabel = new Label();
        int cost = EnchantmentSystem.Instance != null ?
                   EnchantmentSystem.Instance.CalculateEnchantmentCost(enchantment) : enchantment.BaseCost;
        costLabel.Text = $"💰 {cost}";
        costLabel.AddThemeFontSizeOverride("font_size", 16);
        costLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.4f));
        statsBox.AddChild(costLabel);

        var rateLabel = new Label();
        float rate = EnchantmentSystem.Instance != null ?
                    EnchantmentSystem.Instance.CalculateSuccessRate(enchantment) * 100f :
                    enchantment.SuccessRate * 100f;
        rateLabel.Text = $"成功率: {rate:F0}%";
        rateLabel.AddThemeFontSizeOverride("font_size", 13);
        rateLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.8f, 1f));
        statsBox.AddChild(rateLabel);

        return card;
    }

    private void RefreshAppliedList()
    {
        // 清除现有内容
        foreach (var child in _appliedVBox.GetChildren())
        {
            child.QueueFree();
        }

        if (EnchantmentSystem.Instance == null)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无应用的附魔";
            emptyLabel.AddThemeFontSizeOverride("font_size", 18);
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            _appliedVBox.AddChild(emptyLabel);
            return;
        }

        var empty = true;
        var applied = EnchantmentSystem.Instance.SaveData();
        if (applied.ContainsKey("applied_enchantments"))
        {
            var list = (List<object>)applied["applied_enchantments"];
            if (list.Count > 0)
            {
                empty = false;
                foreach (var item in list)
                {
                    var dict = (Dictionary<string, object>)item;
                    var card = CreateAppliedCard(dict);
                    _appliedVBox.AddChild(card);
                }
            }
        }

        if (empty)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无应用的附魔";
            emptyLabel.AddThemeFontSizeOverride("font_size", 18);
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            _appliedVBox.AddChild(emptyLabel);
        }
    }

    private Control CreateAppliedCard(Dictionary<string, object> data)
    {
        string templateId = (string)data["template_id"];
        int level = (int)data["level"];

        var enchantment = EnchantmentDatabase.Instance.GetEnchantment(templateId);
        if (enchantment == null) return new Control();

        var card = new PanelContainer();
        card.SetCustomMinimumSize(new Vector2(720, 70));

        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.15f, 0.15f, 0.2f);
        style.BorderColor = GetRarityColor(enchantment.RarityLevel);
        style.SetBorderWidthAll(2);
        style.SetCornerRadiusAll(4);
        card.AddThemeStyleboxOverride("panel", style);

        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 20);
        card.AddChild(hbox);

        var infoLabel = new Label();
        infoLabel.Text = $"{enchantment.Name} (等级 {level}/{enchantment.MaxLevel})";
        infoLabel.AddThemeFontSizeOverride("font_size", 16);
        infoLabel.AddThemeColorOverride("font_color", GetRarityColor(enchantment.RarityLevel));
        hbox.AddChild(infoLabel);

        hbox.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        // 属性显示
        var propsLabel = new Label();
        var props = new List<string>();
        float levelBonus = 1f + (level - 1) * 0.2f;
        foreach (var prop in enchantment.Properties)
        {
            props.Add($"{GetPropertyName(prop.Key)} +{prop.Value * levelBonus:F1}");
        }
        propsLabel.Text = string.Join(" | ", props);
        propsLabel.AddThemeFontSizeOverride("font_size", 14);
        propsLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 0.6f));
        hbox.AddChild(propsLabel);

        return card;
    }

    private void RefreshStatistics()
    {
        foreach (var child in _statisticsVBox.GetChildren())
        {
            child.QueueFree();
        }

        var statsLabel = new Label();
        statsLabel.Text = "附魔统计";
        statsLabel.AddThemeFontSizeOverride("font_size", 22);
        statsLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.6f));
        _statisticsVBox.AddChild(statsLabel);

        _statisticsVBox.AddChild(new HSeparator());

        if (EnchantmentSystem.Instance != null)
        {
            var stats = EnchantmentSystem.Instance.GetStatistics();

            AddStatRow("已解锁附魔数量", $"{stats["unlocked_count"]}");
            AddStatRow("已应用附魔数量", $"{stats["applied_count"]}");
            AddStatRow("总附魔次数", $"{stats["total_enchantments"]}");
            AddStatRow("成功次数", $"{stats["successful_enchantments"]}");
            AddStatRow("失败次数", $"{stats["failed_enchantments"]}");
            AddStatRow("成功率", $"{stats["success_rate"]:F1}%");
            AddStatRow("总花费金币", $"{stats["total_gold_spent"]}");
            AddStatRow("最常用附魔", $"{stats["most_used_enchantment"]}");
        }
        else
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "附魔系统未初始化";
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
            _statisticsVBox.AddChild(emptyLabel);
        }
    }

    private void AddStatRow(string label, string value)
    {
        var row = new HBoxContainer();
        row.AddThemeConstantOverride("separation", 10);
        _statisticsVBox.AddChild(row);

        var labelNode = new Label();
        labelNode.Text = label + ":";
        labelNode.AddThemeFontSizeOverride("font_size", 16);
        labelNode.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        row.AddChild(labelNode);

        row.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });

        var valueNode = new Label();
        valueNode.Text = value;
        valueNode.AddThemeFontSizeOverride("font_size", 16);
        valueNode.AddThemeColorOverride("font_color", new Color(0.6f, 0.9f, 0.6f));
        row.AddChild(valueNode);
    }

    private Color GetRarityColor(EnchantmentData.Rarity rarity)
    {
        switch (rarity)
        {
            case EnchantmentData.Rarity.Common: return new Color(1f, 1f, 1f);
            case EnchantmentData.Rarity.Uncommon: return new Color(0.12f, 1f, 0f);
            case EnchantmentData.Rarity.Rare: return new Color(0f, 0.44f, 1f);
            case EnchantmentData.Rarity.Epic: return new Color(0.64f, 0.21f, 0.93f);
            case EnchantmentData.Rarity.Legendary: return new Color(1f, 0.5f, 0f);
            default: return new Color(1f, 1f, 1f);
        }
    }

    private string GetTypeName(EnchantmentData.EnchantmentType type)
    {
        switch (type)
        {
            case EnchantmentData.EnchantmentType.Weapon: return "武器";
            case EnchantmentData.EnchantmentType.Armor: return "护甲";
            case EnchantmentData.EnchantmentType.Accessory: return "饰品";
            case EnchantmentData.EnchantmentType.Helmet: return "头盔";
            case EnchantmentData.EnchantmentType.Boots: return "鞋子";
            case EnchantmentData.EnchantmentType.Gloves: return "手套";
            default: return type.ToString();
        }
    }

    private string GetPropertyName(EnchantmentData.PropertyType type)
    {
        switch (type)
        {
            case EnchantmentData.PropertyType.Attack: return "攻击";
            case EnchantmentData.PropertyType.Defense: return "防御";
            case EnchantmentData.PropertyType.Health: return "生命";
            case EnchantmentData.PropertyType.Speed: return "速度";
            case EnchantmentData.PropertyType.Critical: return "暴击";
            case EnchantmentData.PropertyType.Evasion: return "闪避";
            case EnchantmentData.PropertyType.LifeSteal: return "吸血";
            case EnchantmentData.PropertyType.MagicAttack: return "魔攻";
            case EnchantmentData.PropertyType.MagicDefense: return "魔抗";
            case EnchantmentData.PropertyType.FireResistance: return "火抗";
            case EnchantmentData.PropertyType.IceResistance: return "冰抗";
            case EnchantmentData.PropertyType.LightningResistance: return "雷抗";
            default: return type.ToString();
        }
    }

    public override void _Input(InputEvent ev)
    {
        if (ev is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Echo == false)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                Hide();
            }
            else if (keyEvent.Keycode == Key.E)
            {
                if (Visible)
                    Hide();
                else
                {
                    Show();
                    RefreshEnchantmentList();
                    RefreshAppliedList();
                    RefreshStatistics();
                }
            }
        }
    }

    public void Toggle()
    {
        if (Visible)
            Hide();
        else
        {
            Show();
            RefreshEnchantmentList();
            RefreshAppliedList();
            RefreshStatistics();
        }
    }
}
