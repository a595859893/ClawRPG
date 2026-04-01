using Godot;
using System;
using System.Collections.Generic;

public partial class MysteryTreasureUI : Control
{
    private static MysteryTreasureUI _instance;
    public static MysteryTreasureUI Instance => _instance;

    // UI 组件
    private PanelContainer _mainPanel;
    private VBoxContainer _mainVBox;
    private Label _titleLabel;
    private HBoxContainer _statsContainer;
    private Label _totalFoundLabel;
    private Label _totalGoldLabel;
    private Label _totalExpLabel;
    private TabContainer _tabContainer;
    
    // 统计标签页
    private VBoxContainer _statsTab;
    private VBoxContainer _rarityStatsContainer;
    private VBoxContainer _typeStatsContainer;
    
    // 活跃宝藏标签页
    private VBoxContainer _activeTab;
    private ScrollContainer _activeScroll;
    private VBoxContainer _activeList;
    
    // 历史标签页
    private VBoxContainer _historyTab;
    private ScrollContainer _historyScroll;
    private VBoxContainer _historyList;

    private MysteryTreasureSystem _system;
    private MysteryTreasureDatabase _database;
    private bool _isVisible = false; 

    public override void _Ready()
    {
        _instance = this;
        _system = MysteryTreasureSystem.Instance;
        _database = MysteryTreasureDatabase.Instance;
        
        SetupUI();
        Visible = false; 
    }

    private void SetupUI()
    {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(600, 500);
        AddChild(_mainPanel);

        _mainVBox = new VBoxContainer();
        _mainVBox.Setanchorspreset(Control.LayoutPreset.FullRect);
        _mainVBox.AddThemeConstantOverride("separation", 10);
        _mainPanel.AddChild(_mainVBox);

        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "🎁 神秘宝藏";
        _titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _mainVBox.AddChild(_titleLabel);

        // 统计信息栏
        _statsContainer = new HBoxContainer();
        _statsContainer.AddThemeConstantOverride("separation", 30);
        _mainVBox.AddChild(_statsContainer);

        _totalFoundLabel = new Label();
        _totalFoundLabel.Text = "发现: 0";
        _statsContainer.AddChild(_totalFoundLabel);

        _totalGoldLabel = new Label();
        _totalGoldLabel.Text = "金币: 0";
        _statsContainer.AddChild(_totalGoldLabel);

        _totalExpLabel = new Label();
        _totalExpLabel.Text = "经验: 0";
        _statsContainer.AddChild(_totalExpLabel);

        // 标签页容器
        _tabContainer = new TabContainer();
        _tabContainer.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.VERTICAL);
        _mainVBox.AddChild(_tabContainer);

        // ===== 统计标签页 =====
        _statsTab = new VBoxContainer();
        _statsTab.Name = "统计";
        _tabContainer.AddChild(_statsTab);

        _rarityStatsContainer = new VBoxContainer();
        _rarityStatsContainer.AddThemeConstantOverride("separation", 5);
        _statsTab.AddChild(_rarityStatsContainer);

        Label rarityTitle = new Label();
        rarityTitle.Text = "按稀有度统计:";
        rarityTitle.AddThemeFontSizeOverride("font_size", 16);
        _rarityStatsContainer.AddChild(rarityTitle);

        _typeStatsContainer = new VBoxContainer();
        _typeStatsContainer.AddThemeConstantOverride("separation", 5);
        _statsTab.AddChild(_typeStatsContainer);

        Label typeTitle = new Label();
        typeTitle.Text = "按类型统计:";
        typeTitle.AddThemeFontSizeOverride("font_size", 16);
        _typeStatsContainer.AddChild(typeTitle);

        // ===== 活跃宝藏标签页 =====
        _activeTab = new VBoxContainer();
        _activeTab.Name = "活跃宝藏";
        _tabContainer.AddChild(_activeTab);

        _activeScroll = new ScrollContainer();
        _activeScroll.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.VERTICAL);
        _activeTab.AddChild(_activeScroll);

        _activeList = new VBoxContainer();
        _activeList.AddThemeConstantOverride("separation", 5);
        _activeScroll.AddChild(_activeList);

        // ===== 历史标签页 =====
        _historyTab = new VBoxContainer();
        _historyTab.Name = "历史记录";
        _tabContainer.AddChild(_historyTab);

        _historyScroll = new ScrollContainer();
        _historyScroll.SetSizeFlags(Control.SizeFlags.Expand | Control.SizeFlags.Fill, Control.SizeFlags.VERTICAL);
        _historyTab.AddChild(_historyScroll);

        _historyList = new VBoxContainer();
        _historyList.AddThemeConstantOverride("separation", 5);
        _historyScroll.AddChild(_historyList);

        // 刷新按钮
        Button refreshButton = new Button();
        refreshButton.Text = "刷新 (R)";
        refreshButton.Pressed += OnRefreshPressed;
        _mainVBox.AddChild(refreshButton);
    }

    public override void _Process(double delta)
    {
        if (Visible)
        {
            RefreshUI();
        }
    }

    private void RefreshUI()
    {
        if (_system == null) return;

        var playerData = _system.GetPlayerData();
        var stats = _system.GetStatistics();

        // 更新统计信息
        _totalFoundLabel.Text = $"发现: {stats["total_found"]}";
        _totalGoldLabel.Text = $"金币: {stats["total_gold"]}";
        _totalExpLabel.Text = $"经验: {stats["total_exp"]}";

        // 更新稀有度统计
        UpdateRarityStats(stats);

        // 更新类型统计
        UpdateTypeStats(playerData);

        // 更新活跃宝藏列表
        UpdateActiveTreasures();

        // 更新历史记录
        UpdateHistory(playerData);
    }

    private void UpdateRarityStats(Dictionary<string, int> stats)
    {
        // 清空现有内容
        foreach (Node child in _rarityStatsContainer.GetChildren())
        {
            child.QueueFree();
        }

        // 添加稀有度统计
        string[] rarities = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
        string[] rarityNames = { "普通", "优秀", "稀有", "史诗", "传说" };
        
        for (int i = 0; i < rarities.Length; i++)
        {
            string rarityKey = rarities[i].ToLower() + "_count";
            int count = stats.GetValueOrDefault(rarityKey, 0);
            Color color = _database.GetRarityColor((TreasureRarity)i);
            
            Label label = new Label();
            label.Text = $"{rarityNames[i]}: {count}";
            label.Modulate = color;
            _rarityStatsContainer.AddChild(label);
        }
    }

    private void UpdateTypeStats(PlayerMysteryTreasureData playerData)
    {
        // 清空现有内容
        foreach (Node child in _typeStatsContainer.GetChildren())
        {
            child.QueueFree();
        }

        // 添加类型统计
        string[] types = { "Chest", "Hidden", "Ancient", "Monster", "Special" };
        string[] typeNames = { "宝箱", "隐藏", "远古", "怪物掉落", "特殊" };
        
        for (int i = 0; i < types.Length; i++)
        {
            int count = playerData.TypeCount.GetValueOrDefault(types[i], 0);
            
            Label label = new Label();
            label.Text = $"{typeNames[i]}: {count}";
            _typeStatsContainer.AddChild(label);
        }
    }

    private void UpdateActiveTreasures()
    {
        // 清空现有内容
        foreach (Node child in _activeList.GetChildren())
        {
            child.QueueFree();
        }

        var activeTreasures = _system.GetActiveTreasures();
        
        if (activeTreasures.Count == 0)
        {
            Label emptyLabel = new Label();
            emptyLabel.Text = "当前没有活跃的宝藏";
            emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _activeList.AddChild(emptyLabel);
            return;
        }

        foreach (var treasure in activeTreasures)
        {
            var treasureData = _database.GetTreasureById(treasure.TreasureId);
            if (treasureData == null) continue;

            PanelContainer itemPanel = new PanelContainer();
            _activeList.AddChild(itemPanel);

            HBoxContainer itemHBox = new HBoxContainer();
            itemHBox.AddThemeConstantOverride("separation", 10);
            itemPanel.AddChild(itemHBox);

            // 宝藏名称和稀有度
            Label nameLabel = new Label();
            nameLabel.Text = $"📦 {treasureData.TreasureName}";
            nameLabel.Modulate = _database.GetRarityColor(treasureData.Rarity);
            itemHBox.AddChild(nameLabel);

            // 状态
            Label statusLabel = new Label();
            statusLabel.Text = treasure.IsDiscovered ? "🔍 已发现" : "❓ 未发现";
            itemHBox.AddChild(statusLabel);

            // 打开按钮
            Button openButton = new Button();
            openButton.Text = "打开";
            openButton.Pressed += () => OnOpenTreasurePressed(treasure.InstanceId);
            itemHBox.AddChild(openButton);
        }
    }

    private void UpdateHistory(PlayerMysteryTreasureData playerData)
    {
        // 清空现有内容
        foreach (Node child in _historyList.GetChildren())
        {
            child.QueueFree();
        }

        if (playerData.TreasureHistory.Count == 0)
        {
            Label emptyLabel = new Label();
            emptyLabel.Text = "暂无历史记录";
            emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _historyList.AddChild(emptyLabel);
            return;
        }

        // 按数量排序显示
        var sortedHistory = new List<KeyValuePair<string, int>>(playerData.TreasureHistory);
        sortedHistory.Sort((a, b) => b.Value.CompareTo(a.Value));

        foreach (var kvp in sortedHistory)
        {
            var treasureData = _database.GetTreasureById(kvp.Key);
            if (treasureData == null) continue;

            Label label = new Label();
            label.Text = $"📦 {treasureData.TreasureName} x{kvp.Value}";
            label.Modulate = _database.GetRarityColor(treasureData.Rarity);
            _historyList.AddChild(label);
        }
    }

    private void OnOpenTreasurePressed(string instanceId)
    {
        if (_system != null)
        {
            _system.OpenTreasure(instanceId);
        }
    }

    private void OnRefreshPressed()
    {
        RefreshUI();
    }

    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshUI();
        }
    }

    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_mystery_treasure"))
        {
            Toggle();
        }
    }
}
