using Godot;
using System;
using System.Collections.Generic;

public partial class PetRecycleUI : Control
{
    private Control _mainPanel;
    private TabContainer _tabContainer;
    private Label _titleLabel;
    
    // 统计标签页
    private Label _totalRecycledLabel;
    private Label _totalGoldLabel;
    private Label _totalMaterialsLabel;
    private Label _rarePetsLabel;
    private Label _epicPetsLabel;
    private Label _legendaryPetsLabel;
    
    // 历史标签页
    private VBoxContainer _historyContainer;
    
    // 预览标签页
    private OptionButton _petTypeSelector;
    private OptionButton _raritySelector;
    private SpinBox _levelSpinBox;
    private Button _previewButton;
    private Button _recycleButton;
    private Label _previewGoldLabel;
    private VBoxContainer _previewMaterialsContainer;
    
    // 宠物类型列表
    private string[] _petTypes = { "Dog", "Cat", "Bird", "Rabbit", "Dragon", "Slime", "Skeleton", "Elemental" };
    private string[] _rarities = { "Common", "Uncommon", "Rare", "Epic", "Legendary" };
    
    public override void _Ready()
    {
        SetupUI();
        ConnectSignals();
        RefreshStatistics();
        
        GD.Print("[PetRecycleUI] Initialized");
    }
    
    private void SetupUI()
    {
        // 主面板
        _mainPanel = new Control
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            MouseFilter = Control.MouseFilterEnum.Stop
        };
        AddChild(_mainPanel);
        
        // 背景
        var bgPanel = new Panel
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            Modulate = new Color(1, 1, 1, 0.95f)
        };
        _mainPanel.AddChild(bgPanel);
        
        // 标题栏
        var titleBar = new HBoxContainer
        {
            AnchorRight = 1f,
            OffsetTop = 10,
            OffsetRight = -10
        };
        _mainPanel.AddChild(titleBar);
        
        _titleLabel = new Label
        {
            Text = "  🐾 宠物回收系统",
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 40)
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        titleBar.AddChild(_titleLabel);
        
        // 关闭按钮
        var closeButton = new Button
        {
            Text = "✕",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        closeButton.Pressed += () => Hide();
        titleBar.AddChild(closeButton);
        
        // 标签页容器
        _tabContainer = new TabContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetTop = 60,
            OffsetRight = -20,
            OffsetBottom = -20
        };
        _mainPanel.AddChild(_tabContainer);
        
        // 创建标签页
        CreateStatisticsTab();
        CreateHistoryTab();
        CreateRecycleTab();
    }
    
    private void CreateStatisticsTab()
    {
        var tab = new Control();
        tab.Name = "统计";
        _tabContainer.AddChild(tab);
        
        var vbox = new VBoxContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 20,
            OffsetTop = 20,
            OffsetRight = -20,
            OffsetBottom = -20
        };
        tab.AddChild(vbox);
        
        // 标题
        var title = new Label
        {
            Text = "📊 回收统计",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(title);
        
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // 统计项
        _totalRecycledLabel = CreateStatLabel("Total Recycled:", "0", vbox);
        _totalGoldLabel = CreateStatLabel("Total Gold Earned:", "0", vbox);
        _totalMaterialsLabel = CreateStatLabel("Total Materials:", "0", vbox);
        _rarePetsLabel = CreateStatLabel("Rare Pets:", "0", vbox);
        _epicPetsLabel = CreateStatLabel("Epic Pets:", "0", vbox);
        _legendaryPetsLabel = CreateStatLabel("Legendary Pets:", "0", vbox);
        
        // 重置按钮
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 30) });
        
        var resetButton = new Button
        {
            Text = "重置统计",
            CustomMinimumSize = new Vector2(200, 40)
        };
        resetButton.Pressed += OnResetPressed;
        vbox.AddChild(resetButton);
    }
    
    private void CreateHistoryTab()
    {
        var tab = new Control();
        tab.Name = "历史";
        _tabContainer.AddChild(tab);
        
        var scroll = new ScrollContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 20,
            OffsetTop = 20,
            OffsetRight = -20,
            OffsetBottom = -20
        };
        tab.AddChild(scroll);
        
        _historyContainer = new VBoxContainer
        {
            CustomMinimumSize = new Vector2(400, 0)
        };
        scroll.AddChild(_historyContainer);
        
        RefreshHistory();
    }
    
    private void CreateRecycleTab()
    {
        var tab = new Control();
        tab.Name = "回收";
        _tabContainer.AddChild(tab);
        
        var vbox = new VBoxContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 20,
            OffsetTop = 20,
            OffsetRight = -20,
            OffsetBottom = -20
        };
        tab.AddChild(vbox);
        
        // 标题
        var title = new Label
        {
            Text = "🔄 宠物回收",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        title.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(title);
        
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // 宠物类型选择
        var typeLabel = new Label { Text = "宠物类型:" };
        vbox.AddChild(typeLabel);
        
        _petTypeSelector = new OptionButton;
        foreach (var type in _petTypes)
        {
            _petTypeSelector.AddItem(type);
        }
        vbox.AddChild(_petTypeSelector);
        
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
        
        // 稀有度选择
        var rarityLabel = new Label { Text = "稀有度:" };
        vbox.AddChild(rarityLabel);
        
        _raritySelector = new OptionButton;
        foreach (var rarity in _rarities)
        {
            _raritySelector.AddItem(rarity);
        }
        vbox.AddChild(_raritySelector);
        
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 10) });
        
        // 等级
        var levelLabel = new Label { Text = "等级:" };
        vbox.AddChild(levelLabel);
        
        _levelSpinBox = new SpinBox
        {
            MinValue = 1,
            MaxValue = 100,
            Value = 1
        };
        vbox.AddChild(_levelSpinBox);
        
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // 预览按钮
        _previewButton = new Button
        {
            Text = "预览回收收益",
            CustomMinimumSize = new Vector2(200, 40)
        };
        _previewButton.Pressed += OnPreviewPressed;
        vbox.AddChild(_previewButton);
        
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // 预览结果
        var previewTitle = new Label { Text = "预览结果:", HorizontalAlignment = HorizontalAlignment.Center };
        vbox.AddChild(previewTitle);
        
        _previewGoldLabel = new Label
        {
            Text = "金币: 0",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _previewGoldLabel.AddThemeFontSizeOverride("font_size", 18);
        vbox.AddChild(_previewGoldLabel);
        
        _previewMaterialsContainer = new VBoxContainer();
        vbox.AddChild(_previewMaterialsContainer);
        
        vbox.AddChild(new Control { CustomMinimumSize = new Vector2(0, 20) });
        
        // 回收按钮
        _recycleButton = new Button
        {
            Text = "确认回收",
            CustomMinimumSize = new Vector2(200, 50)
        };
        _recycleButton.Pressed += OnRecyclePressed;
        vbox.AddChild(_recycleButton);
    }
    
    private Label CreateStatLabel(string labelText, string valueText, VBoxContainer parent)
    {
        var hbox = new HBoxContainer();
        parent.AddChild(hbox);
        
        var label = new Label
        {
            Text = labelText,
            CustomMinimumSize = new Vector2(200, 0)
        };
        hbox.AddChild(label);
        
        var value = new Label
        {
            Text = valueText,
            HorizontalAlignment = HorizontalAlignment.Right
        };
        value.AddThemeFontSizeOverride("font_size", 18);
        hbox.AddChild(value);
        
        return value;
    }
    
    private void ConnectSignals()
    {
        if (PetRecycleSystem.Instance != null)
        {
            PetRecycleSystem.Instance.RecycleCompleted += OnRecycleCompleted;
            PetRecycleSystem.Instance.StatisticsUpdated += OnStatisticsUpdated;
        }
    }
    
    private void RefreshStatistics()
    {
        if (PetRecycleSystem.Instance == null) return;
        
        var stats = PetRecycleSystem.Instance.GetStatistics();
        
        _totalRecycledLabel.Text = stats.TotalRecycled.ToString();
        _totalGoldLabel.Text = stats.TotalGoldEarned.ToString("N0");
        _totalMaterialsLabel.Text = stats.TotalMaterialsEarned.ToString();
        _rarePetsLabel.Text = stats.RarePetsRecycled.ToString();
        _epicPetsLabel.Text = stats.EpicPetsRecycled.ToString();
        _legendaryPetsLabel.Text = stats.LegendaryPetsRecycled.ToString();
    }
    
    private void RefreshHistory()
    {
        // 清除旧内容
        foreach (var child in _historyContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (PetRecycleSystem.Instance == null) return;
        
        var history = PetRecycleSystem.Instance.GetHistory(20);
        
        foreach (var record in history)
        {
            var hbox = new HBoxContainer();
            _historyContainer.AddChild(hbox);
            
            var label = new Label
            {
                Text = $"{record.PetName} ({record.Rarity}) - {record.GoldEarned} Gold - {record.MaterialsEarned.Count} Materials"
            };
            hbox.AddChild(label);
        }
        
        if (history.Count == 0)
        {
            var emptyLabel = new Label
            {
                Text = "暂无回收记录",
                HorizontalAlignment = HorizontalAlignment.Center
            };
            _historyContainer.AddChild(emptyLabel);
        }
    }
    
    private void OnPreviewPressed()
    {
        if (PetRecycleSystem.Instance == null) return;
        
        string petType = _petTypes[_petTypeSelector.Selected];
        string rarity = _rarities[_raritySelector.Selected];
        int level = (int)_levelSpinBox.Value;
        
        var preview = PetRecycleSystem.Instance.GetRecyclePreview(petType, rarity, level);
        
        _previewGoldLabel.Text = $"金币: {preview["gold"]}";
        
        // 显示材料
        foreach (var child in _previewMaterialsContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        var materials = preview["materials"] as List<Dictionary<string, object>>;
        foreach (var mat in materials)
        {
            var label = new Label
            {
                Text = $"  • {mat["name"]} x{mat["amount"]}"
            };
            _previewMaterialsContainer.AddChild(label);
        }
    }
    
    private void OnRecyclePressed()
    {
        if (PetRecycleSystem.Instance == null) return;
        
        string petType = _petTypes[_petTypeSelector.Selected];
        string rarity = _rarities[_raritySelector.Selected];
        int level = (int)_levelSpinBox.Value;
        
        // 生成虚拟宠物ID
        string petId = $"recycle_{DateTimeOffset.UtcNow.ToUnixTimeSeconds()}";
        string petName = $"{petType}_{rarity}";
        
        var record = PetRecycleSystem.Instance.RecyclePet(petId, petName, petType, rarity, level);
        
        GD.Print($"[PetRecycleUI] Recycled: {record.PetName}");
    }
    
    private void OnRecycleCompleted(RecycleRecord record)
    {
        RefreshHistory();
    }
    
    private void OnStatisticsUpdated(PetRecycleData data)
    {
        RefreshStatistics();
    }
    
    private void OnResetPressed()
    {
        if (PetRecycleSystem.Instance == null) return;
        
        PetRecycleSystem.Instance.ResetStatistics();
        RefreshHistory();
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                Hide();
            }
        }
    }
}
