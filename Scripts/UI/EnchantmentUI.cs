using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.Enchantment;
using ClawRPG.Scripts.Items;

public partial class EnchantmentUI : Control
{
    private PanelContainer _mainPanel;
    private VBoxContainer _mainVBox;
    private Label _titleLabel;
    private HBoxContainer _tabsContainer;
    private Button _attackTab;
    private Button _defenseTab;
    private Button _magicTab;
    private Button _utilityTab;
    private Button _legendaryTab;
    private ScrollContainer _enchantmentsScroll;
    private VBoxContainer _enchantmentsList;
    private Label _inventoryLabel;
    private GridContainer _inventoryGrid;
    private Label _selectedInfoLabel;
    private Button _enchantButton;
    private Label _goldLabel;

    private EnchantmentType _currentType = EnchantmentType.Attack;
    private EnchantmentData _selectedEnchantment;
    private int _selectedEquipmentSlot = 0; // 0: 武器

    private Color _rarityCommon = new Color(0.7f, 0.7f, 0.7f);
    private Color _rarityUncommon = new Color(0.2f, 0.8f, 0.2f);
    private Color _rarityRare = new Color(0.3f, 0.5f, 1.0f);
    private Color _rarityEpic = new Color(0.6f, 0.3f, 0.9f);
    private Color _rarityLegendary = new Color(1.0f, 0.6f, 0.0f);

    public override void _Ready()
    {
        Visible = false;
        CreateUI();

        // 监听附魔事件
        EnchantmentSystem.Instance.OnEnchantmentResult += OnEnchantmentResult;
    }

    private void CreateUI()
    {
        // 主面板
        _mainPanel = new PanelContainer
        {
            AnchorLeft = 0.5f,
            AnchorTop = 0.5f,
            AnchorRight = 0.5f,
            AnchorBottom = 0.5f,
            OffsetLeft = -400,
            OffsetTop = -300,
            OffsetRight = 400,
            OffsetBottom = 300,
            CustomMinimumSize = new Vector2(800, 600)
        };
        AddChild(_mainPanel);

        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        styleBox.BorderColor = new Color(0.3f, 0.3f, 0.4f);
        styleBox.SetBorderWidthAll(2);
        styleBox.SetCornerRadiusAll(8);
        _mainPanel.AddThemeStyleboxOverride("panel", styleBox);

        _mainVBox = new VBoxContainer { };
        _mainPanel.AddChild(_mainVBox);

        // 标题
        _titleLabel = new Label
        {
            Text = " ✨ 附魔系统",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            CustomMinimumSize = new Vector2(0, 50)
        };
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _titleLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
        _mainVBox.AddChild(_titleLabel);

        // 金币显示
        _goldLabel = new Label
        {
            Text = "金币: 0",
            HorizontalAlignment = HorizontalAlignment.Right,
            CustomMinimumSize = new Vector2(0, 30)
        };
        _goldLabel.AddThemeFontSizeOverride("font_size", 18);
        _goldLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
        _mainVBox.AddChild(_goldLabel);

        // 标签页
        _tabsContainer = new HBoxContainer { };
        _tabsContainer.AddThemeConstantOverride("separation", 10);
        _mainVBox.AddChild(_tabsContainer);

        _attackTab = CreateTabButton("⚔️ 攻击", EnchantmentType.Attack);
        _defenseTab = CreateTabButton("🛡️ 防御", EnchantmentType.Defense);
        _magicTab = CreateTabButton("🔮 魔法", EnchantmentType.Magic);
        _utilityTab = CreateTabButton("✨ 辅助", EnchantmentType.Utility);
        _legendaryTab = CreateTabButton("⭐ 传奇", EnchantmentType.Legendary);

        _tabsContainer.AddChild(_attackTab);
        _tabsContainer.AddChild(_defenseTab);
        _tabsContainer.AddChild(_magicTab);
        _tabsContainer.AddChild(_utilityTab);
        _tabsContainer.AddChild(_legendaryTab);

        UpdateTabColors();

        // 附魔列表
        _enchantmentsScroll = new ScrollContainer { };
        _enchantmentsScroll.CustomMinimumSize = new Vector2(780, 250);
        _mainVBox.AddChild(_enchantmentsScroll);

        _enchantmentsList = new VBoxContainer { };
        _enchantmentsList.AddThemeConstantOverride("separation", 5);
        _enchantmentsScroll.AddChild(_enchantmentsList);

        // 背包显示
        _inventoryLabel = new Label
        {
            Text = "📦 附魔卷轴背包",
            CustomMinimumSize = new Vector2(0, 30)
        };
        _inventoryLabel.AddThemeFontSizeOverride("font_size", 16);
        _inventoryLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.8f));
        _mainVBox.AddChild(_inventoryLabel);

        var inventoryScroll = new ScrollContainer { };
        inventoryScroll.CustomMinimumSize = new Vector2(780, 120);
        _mainVBox.AddChild(inventoryScroll);

        _inventoryGrid = new GridContainer { Columns = 5 };
        _inventoryGrid.AddThemeConstantOverride("h_separation", 10);
        _inventoryGrid.AddThemeConstantOverride("v_separation", 10);
        inventoryScroll.AddChild(_inventoryGrid);

        // 选中信息
        _selectedInfoLabel = new Label
        {
            Text = "选择一个附魔",
            CustomMinimumSize = new Vector2(0, 40),
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _selectedInfoLabel.AddThemeFontSizeOverride("font_size", 14);
        _mainVBox.AddChild(_selectedInfoLabel);

        // 附魔按钮
        _enchantButton = new Button
        {
            Text = "开始附魔",
            CustomMinimumSize = new Vector2(200, 40)
        };
        _enchantButton.Pressed += OnEnchantButtonPressed;
        _mainVBox.AddChild(_enchantButton);

        UpdateEnchantmentList();
        UpdateInventoryDisplay();
        UpdateGoldDisplay();
    }

    private Button CreateTabButton(string text, EnchantmentType type)
    {
        var btn = new Button
        {
            Text = text,
            ToggleMode = true,
            ButtonPressed = _currentType == type
        };
        btn.Pressed += () => OnTabSelected(type);
        return btn;
    }

    private void OnTabSelected(EnchantmentType type)
    {
        _currentType = type;
        UpdateTabColors();
        UpdateEnchantmentList();
    }

    private void UpdateTabColors()
    {
        _attackTab.ButtonPressed = _currentType == EnchantmentType.Attack;
        _defenseTab.ButtonPressed = _currentType == EnchantmentType.Defense;
        _magicTab.ButtonPressed = _currentType == EnchantmentType.Magic;
        _utilityTab.ButtonPressed = _currentType == EnchantmentType.Utility;
        _legendaryTab.ButtonPressed = _currentType == EnchantmentType.Legendary;
    }

    private void UpdateEnchantmentList()
    {
        // 清除现有项
        foreach (var child in _enchantmentsList.GetChildren())
        {
            child.QueueFree();
        }

        var enchantments = EnchantmentDatabase.Instance.GetEnchantmentsByType(_currentType);
        var player = Main.Instance?.GetPlayer();
        int playerLevel = player != null ? player.Level : 1;

        foreach (var enchant in enchantments)
        {
            var item = CreateEnchantmentItem(enchant, playerLevel);
            _enchantmentsList.AddChild(item);
        }
    }

    private Control CreateEnchantmentItem(EnchantmentData enchant, int playerLevel)
    {
        var container = new PanelContainer { };
        container.CustomMinimumSize = new Vector2(760, 60);

        var hbox = new HBoxContainer { };
        hbox.AddThemeConstantOverride("separation", 20);
        container.AddChild(hbox);

        // 稀有度边框颜色
        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.15f, 0.15f, 0.2f);
        styleBox.BorderColor = enchant.GetRarityColor();
        styleBox.SetBorderWidthAll(2);
        styleBox.SetCornerRadiusAll(4);
        container.AddThemeStyleboxOverride("panel", styleBox);

        // 名称
        var nameLabel = new Label
        {
            Text = enchant.Name,
            CustomMinimumSize = new Vector2(120, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        nameLabel.AddThemeColorOverride("font_color", enchant.GetRarityColor());
        hbox.AddChild(nameLabel);

        // 描述
        var descLabel = new Label
        {
            Text = enchant.Description,
            CustomMinimumSize = new Vector2(200, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        descLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        hbox.AddChild(descLabel);

        // 属性
        var attrLabel = new Label
        {
            Text = $"+{enchant.AttributeValue} {GetAttributeName(enchant.Attribute)}",
            CustomMinimumSize = new Vector2(120, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        attrLabel.AddThemeFontSizeOverride("font_size", 14);
        attrLabel.AddThemeColorOverride("font_color", new Color(0.3f, 0.9f, 0.5f));
        hbox.AddChild(attrLabel);

        // 成功率
        var rateLabel = new Label
        {
            Text = $"成功率: {(enchant.SuccessRate * 100):F0}%",
            CustomMinimumSize = new Vector2(100, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        rateLabel.AddThemeFontSizeOverride("font_size", 14);
        rateLabel.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.5f));
        hbox.AddChild(rateLabel);

        // 等级要求
        var levelLabel = new Label
        {
            Text = playerLevel >= enchant.RequiredPlayerLevel ? $"✓ {enchant.RequiredPlayerLevel}级" : $"✗ {enchant.RequiredPlayerLevel}级",
            CustomMinimumSize = new Vector2(80, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        levelLabel.AddThemeFontSizeOverride("font_size", 14);
        levelLabel.AddThemeColorOverride("font_color", playerLevel >= enchant.RequiredPlayerLevel ? new Color(0.3f, 0.9f, 0.5f) : new Color(0.9f, 0.3f, 0.3f));
        hbox.AddChild(levelLabel);

        // 花费
        var costLabel = new Label
        {
            Text = $"💰 {enchant.Cost}",
            CustomMinimumSize = new Vector2(80, 0),
            VerticalAlignment = VerticalAlignment.Center
        };
        costLabel.AddThemeFontSizeOverride("font_size", 14);
        costLabel.AddThemeColorOverride("font_color", new Color(1f, 0.84f, 0f));
        hbox.AddChild(costLabel);

        // 选择按钮
        var selectBtn = new Button
        {
            Text = "选择",
            CustomMinimumSize = new Vector2(60, 30)
        };
        selectBtn.Pressed += () => OnSelectEnchantment(enchant);
        hbox.AddChild(selectBtn);

        return container;
    }

    private void OnSelectEnchantment(EnchantmentData enchant)
    {
        _selectedEnchantment = enchant;
        UpdateSelectedInfo();
    }

    private void UpdateSelectedInfo()
    {
        if (_selectedEnchantment == null)
        {
            _selectedInfoLabel.Text = "选择一个附魔";
            _enchantButton.Disabled = true;
            return;
        }

        var player = Main.Instance?.GetPlayer();
        int playerLevel = player != null ? player.Level : 1;

        int count = EnchantmentSystem.Instance.GetEnchantmentCount(_selectedEnchantment.Id);
        bool canEnchant = playerLevel >= _selectedEnchantment.RequiredPlayerLevel && count > 0;

        _selectedInfoLabel.Text = $"已选择: {_selectedEnchantment.Name} (+{_selectedEnchantment.AttributeValue} {GetAttributeName(_selectedEnchantment.Attribute)}) | 拥有: {count}";
        _enchantButton.Disabled = !canEnchant;
    }

    private void UpdateInventoryDisplay()
    {
        foreach (var child in _inventoryGrid.GetChildren())
        {
            child.QueueFree();
        }

        var inventory = EnchantmentSystem.Instance.GetInventory();
        foreach (var kvp in inventory)
        {
            var enchant = EnchantmentDatabase.Instance.GetEnchantment(kvp.Key);
            if (enchant == null) continue;

            var item = CreateInventoryItem(enchant, kvp.Value);
            _inventoryGrid.AddChild(item);
        }
    }

    private Control CreateInventoryItem(EnchantmentData enchant, int count)
    {
        var container = new PanelContainer { };
        container.CustomMinimumSize = new Vector2(140, 60);

        var vbox = new VBoxContainer { };
        vbox.AddThemeConstantOverride("separation", 2);
        container.AddChild(vbox);

        var styleBox = new StyleBoxFlat();
        styleBox.BgColor = new Color(0.2f, 0.2f, 0.25f);
        styleBox.BorderColor = enchant.GetRarityColor();
        styleBox.SetBorderWidthAll(2);
        styleBox.SetCornerRadiusAll(4);
        container.AddThemeStyleboxOverride("panel", styleBox);

        var nameLabel = new Label
        {
            Text = enchant.Name,
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(130, 25)
        };
        nameLabel.AddThemeFontSizeOverride("font_size", 12);
        nameLabel.AddThemeColorOverride("font_color", enchant.GetRarityColor());
        vbox.AddChild(nameLabel);

        var countLabel = new Label
        {
            Text = $"×{count}",
            HorizontalAlignment = HorizontalAlignment.Center,
            CustomMinimumSize = new Vector2(130, 25)
        };
        countLabel.AddThemeFontSizeOverride("font_size", 14);
        countLabel.AddThemeColorOverride("font_color", new Color(1f, 1f, 1f));
        vbox.AddChild(countLabel);

        return container;
    }

    private void UpdateGoldDisplay()
    {
        var player = Main.Instance?.GetPlayer();
        if (player != null)
        {
            _goldLabel.Text = $"金币: {player.Gold}";
        }
    }

    private void OnEnchantButtonPressed()
    {
        if (_selectedEnchantment == null) return;

        var player = Main.Instance?.GetPlayer();
        if (player == null) return;

        // 检查金币
        if (player.Gold < _selectedEnchantment.Cost)
        {
            GameMessageSystem.Instance?.ShowNegative("金币不足！");
            return;
        }

        // 扣除金币
        player.Gold -= _selectedEnchantment.Cost;
        UpdateGoldDisplay();

        // 执行附魔
        bool success = EnchantmentSystem.Instance.Enchant(
            _selectedEnchantment.Id,
            player.Level,
            _selectedEquipmentSlot
        );

        UpdateInventoryDisplay();
        UpdateSelectedInfo();
    }

    private void OnEnchantmentResult(bool success, string message)
    {
        if (success)
        {
            GameMessageSystem.Instance?.ShowPositive(message);
        }
        else
        {
            GameMessageSystem.Instance?.ShowNegative(message);
        }
    }

    private string GetAttributeName(EnchantmentAttribute attribute)
    {
        return attribute switch
        {
            EnchantmentAttribute.Damage => "伤害",
            EnchantmentAttribute.Defense => "防御",
            EnchantmentAttribute.Health => "生命",
            EnchantmentAttribute.Mana => "法力",
            EnchantmentAttribute.CriticalRate => "暴击率",
            EnchantmentAttribute.CriticalDamage => "暴击伤害",
            EnchantmentAttribute.AttackSpeed => "攻击速度",
            EnchantmentAttribute.MoveSpeed => "移动速度",
            EnchantmentAttribute.FireResistance => "火抗",
            EnchantmentAttribute.IceResistance => "冰抗",
            EnchantmentAttribute.LightningResistance => "雷抗",
            EnchantmentAttribute.PoisonResistance => "毒抗",
            EnchantmentAttribute.AllAttributes => "全属性",
            _ => attribute.ToString()
        };
    }

    public void Toggle()
    {
        Visible = !Visible;
        if (Visible)
        {
            UpdateEnchantmentList();
            UpdateInventoryDisplay();
            UpdateGoldDisplay();
            UpdateSelectedInfo();
        }
    }

    public override void _Input(InputEvent eventArgs)
    {
        if (eventArgs.IsActionPressed("ui_cancel") && Visible)
        {
            Toggle();
        }
    }
}
