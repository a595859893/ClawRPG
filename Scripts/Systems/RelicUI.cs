using Godot;
using System;
using System.Collections.Generic;
using Godot.Collections;

namespace ClawRPG.Systems;

/// <summary>
/// 遗物界面
/// </summary>
public class RelicUI : Control
{
    private VBoxContainer _mainContainer;
    private HBoxContainer _headerContainer;
    private HBoxContainer _contentContainer;
    private VBoxContainer _relicListContainer;
    private VBoxContainer _detailContainer;
    private Label _goldLabel;
    private Label _slotLabel;
    
    // 当前选中遗物
    private RelicData _selectedRelic;
    private Button _selectedButton;
    
    // 商店/背包模式
    private bool _isShopMode = false; 
    
    public override void _Ready()
    {
        Visible = false; 
        SetupUI();
        ConnectSignals();
    }
    
    private void SetupUI()
    {
        // 背景面板
        var bg = new Panel();
        bg.SetAnchorsPreset(ControlPreset.FullRect);
        bg.Modulate = new Color(1, 1, 1, 0.95f);
        AddChild(bg);
        
        // 主容器
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorsPreset(ControlPreset.FullRect);
        _mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(_mainContainer);
        
        // 标题栏
        var titleBar = new HBoxContainer();
        _mainContainer.AddChild(titleBar);
        
        var title = new Label();
        title.Text = "  遗物系统 (Relics)";
        title.AddThemeFontSizeOverride("font_size", 24);
        titleBar.AddChild(title);
        
        titleBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        _goldLabel = new Label();
        _goldLabel.Text = "金币: 0";
        _goldLabel.AddThemeFontSizeOverride("font_size", 18);
        titleBar.AddChild(_goldLabel);
        
        var closeBtn = new Button();
        closeBtn.Text = "  关闭  ";
        closeBtn.Pressed += () => Hide();
        titleBar.AddChild(closeBtn);
        
        // 槽位显示
        _slotLabel = new Label();
        _slotLabel.Text = "装备槽: 0/3";
        _slotLabel.AddThemeFontSizeOverride("font_size", 16);
        _mainContainer.AddChild(_slotLabel);
        
        // 模式切换
        var modeContainer = new HBoxContainer();
        modeContainer.AddThemeConstantOverride("separation", 10);
        _mainContainer.AddChild(modeContainer);
        
        var shopBtn = new Button();
        shopBtn.Text = "商店";
        shopBtn.Pressed += () => SetMode(true);
        modeContainer.AddChild(shopBtn);
        
        var inventoryBtn = new Button();
        inventoryBtn.Text = "背包";
        inventoryBtn.Pressed += () => SetMode(false);
        modeContainer.AddChild(inventoryBtn);
        
        modeContainer.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        var unlockBtn = new Button();
        unlockBtn.Text = "解锁槽位 (500g)";
        unlockBtn.Pressed += OnUnlockSlotPressed;
        modeContainer.AddChild(unlockBtn);
        
        // 内容区域
        _contentContainer = new HBoxContainer();
        _contentContainer.SetAnchorsPreset(ControlPreset.FullRect);
        _contentContainer.AddThemeConstantOverride("separation", 20);
        _mainContainer.AddChild(_contentContainer);
        
        // 遗物列表
        var listPanel = new Panel();
        listPanel.CustomMinimumSize = new Vector2(400, 0);
        _contentContainer.AddChild(listPanel);
        
        var listScroll = new ScrollContainer();
        listScroll.SetAnchorsPreset(ControlPreset.FullRect);
        listScroll.AddThemeConstantOverride("h_separation", 5);
        listScroll.AddThemeConstantOverride("v_separation", 5);
        listPanel.AddChild(listScroll);
        
        _relicListContainer = new VBoxContainer();
        _relicListContainer.AddThemeConstantOverride("separation", 5);
        listScroll.AddChild(_relicListContainer);
        
        // 详情面板
        _detailContainer = new VBoxContainer();
        _contentContainer.AddChild(_detailContainer);
        
        RefreshUI();
    }
    
    private void ConnectSignals()
    {
        if (RelicSystem.Instance != null)
        {
            RelicSystem.Instance.RelicPurchased += OnRelicPurchased;
            RelicSystem.Instance.RelicEquipped += OnRelicEquipped;
            RelicSystem.Instance.RelicUnequipped += OnRelicUnequipped;
            RelicSystem.Instance.RelicSlotUnlocked += OnRelicSlotUnlocked;
        }
    }
    
    private void SetMode(bool shopMode)
    {
        _isShopMode = shopMode;
        RefreshUI();
    }
    
    public void RefreshUI()
    {
        // 更新金币显示
        var player = GetTree().CurrentScene.GetNodeOrNull<Player>("Player");
        if (player != null)
        {
            _goldLabel.Text = $"金币: {player.Gold}";
        }
        
        // 更新槽位显示
        if (RelicSystem.Instance != null)
        {
            _slotLabel.Text = $"装备槽: {RelicSystem.Instance.GetCurrentEquippedCount()}/{RelicSystem.Instance.GetMaxRelicSlots()}";
        }
        
        // 刷新遗物列表
        RefreshRelicList();
        RefreshDetailPanel();
    }
    
    private void RefreshRelicList()
    {
        // 清空列表
        foreach (var child in _relicListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        List<RelicData> relics;
        
        if (_isShopMode)
        {
            relics = RelicDatabase.GetShopRelics();
        }
        else
        {
            relics = RelicSystem.Instance.GetOwnedRelics();
        }
        
        // 按稀有度排序
        relics.Sort((a, b) => b.Rarity.CompareTo(a.Rarity));
        
        foreach (var relic in relics)
        {
            var btn = CreateRelicButton(relic);
            _relicListContainer.AddChild(btn);
        }
    }
    
    private Button CreateRelicButton(RelicData relic)
    {
        var btn = new Button();
        btn.CustomMinimumSize = new Vector2(380, 50);
        
        var hbox = new HBoxContainer();
        hbox.AddThemeConstantOverride("separation", 10);
        btn.AddChild(hbox);
        
        // 稀有度颜色
        var color = GetRarityColor(relic.Rarity);
        
        // 遗物图标占位
        var icon = new Label();
        icon.Text = "◆";
        icon.Modulate = color;
        icon.AddThemeFontSizeOverride("font_size", 20);
        hbox.AddChild(icon);
        
        // 遗物名称
        var name = new Label();
        name.Text = relic.Name;
        name.Modulate = color;
        name.AddThemeFontSizeOverride("font_size", 16);
        hbox.AddChild(name);
        
        hbox.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        // 状态
        var status = new Label();
        
        if (_isShopMode)
        {
            status.Text = $"{relic.Price}g";
        }
        else
        {
            var equipped = RelicSystem.Instance.GetEquippedRelics();
            bool isEquipped = equipped.Exists(r => r.Id == relic.Id);
            status.Text = isEquipped ? "[已装备]" : "[未装备]";
        }
        
        status.AddThemeFontSizeOverride("font_size", 14);
        hbox.AddChild(status);
        
        btn.Pressed += () => OnRelicSelected(relic, btn);
        
        return btn;
    }
    
    private void OnRelicSelected(RelicData relic, Button btn)
    {
        _selectedRelic = relic;
        
        // 更新选中状态
        if (_selectedButton != null)
        {
            _selectedButton.Modulate = Colors.White;
        }
        _selectedButton = btn;
        _selectedButton.Modulate = new Color(1, 1, 0.5f);
        
        RefreshDetailPanel();
    }
    
    private void RefreshDetailPanel()
    {
        foreach (var child in _detailContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (_selectedRelic == null)
        {
            var hint = new Label();
            hint.Text = "选择一个遗物查看详情";
            hint.AddThemeFontSizeOverride("font_size", 18);
            _detailContainer.AddChild(hint);
            return;
        }
        
        // 遗物名称
        var name = new Label();
        name.Text = _selectedRelic.Name;
        name.Modulate = GetRarityColor(_selectedRelic.Rarity);
        name.AddThemeFontSizeOverride("font_size", 24);
        _detailContainer.AddChild(name);
        
        // 稀有度和类型
        var typeLabel = new Label();
        typeLabel.Text = $"{GetRarityName(_selectedRelic.Rarity)} · {GetTypeName(_selectedRelic.Type)}";
        typeLabel.AddThemeFontSizeOverride("font_size", 16);
        _detailContainer.AddChild(typeLabel);
        
        // 描述
        var desc = new Label();
        desc.Text = _selectedRelic.Description;
        desc.AddThemeFontSizeOverride("font_size", 16);
        desc.AutowrapMode = TextServer.AutowrapMode.Word;
        _detailContainer.AddChild(desc);
        
        // 属性加成
        if (_selectedRelic.AttributeBonuses.Count > 0)
        {
            var attrTitle = new Label();
            attrTitle.Text = "\n属性加成:";
            attrTitle.AddThemeFontSizeOverride("font_size", 16);
            _detailContainer.AddChild(attrTitle);
            
            foreach (var attr in _selectedRelic.AttributeBonuses)
            {
                var attrLabel = new Label();
                attrLabel.Text = $"  • {GetAttributeName(attr.Key)}: +{(attr.Value * 100):F0}%";
                _detailContainer.AddChild(attrLabel);
            }
        }
        
        // 特殊效果
        if (!string.IsNullOrEmpty(_selectedRelic.SpecialEffect))
        {
            var effectTitle = new Label();
            effectTitle.Text = "\n特殊效果:";
            effectTitle.AddThemeFontSizeOverride("font_size", 16);
            _detailContainer.AddChild(effectTitle);
            
            var effectLabel = new Label();
            effectLabel.Text = $"  • {GetSpecialEffectName(_selectedRelic.SpecialEffect)}";
            _detailContainer.AddChild(effectLabel);
        }
        
        // 操作按钮
        var btnContainer = new HBoxContainer();
        btnContainer.AddThemeConstantOverride("separation", 10);
        _detailContainer.AddChild(btnContainer);
        
        if (_isShopMode)
        {
            // 购买按钮
            var buyBtn = new Button();
            buyBtn.Text = $"购买 ({_selectedRelic.Price}g)";
            buyBtn.Pressed += OnBuyPressed;
            
            bool owned = RelicSystem.Instance.HasRelic(_selectedRelic.Id);
            buyBtn.Disabled = owned;
            
            if (owned)
                buyBtn.Text = "已拥有";
                
            btnContainer.AddChild(buyBtn);
        }
        else
        {
            // 装备/卸下按钮
            var equipped = RelicSystem.Instance.GetEquippedRelics();
            bool isEquipped = equipped.Exists(r => r.Id == _selectedRelic.Id);
            
            if (isEquipped)
            {
                var unequipBtn = new Button();
                unequipBtn.Text = "卸下";
                unequipBtn.Pressed += OnUnequipPressed;
                btnContainer.AddChild(unequipBtn);
            }
            else
            {
                var equipBtn = new Button();
                equipBtn.Text = "装备";
                equipBtn.Pressed += OnEquipPressed;
                
                // 检查是否有空槽位
                if (RelicSystem.Instance.GetCurrentEquippedCount() >= RelicSystem.Instance.GetMaxRelicSlots())
                    equipBtn.Disabled = true;
                    
                btnContainer.AddChild(equipBtn);
            }
        }
    }
    
    private void OnBuyPressed()
    {
        if (_selectedRelic == null) return;
        
        if (RelicSystem.Instance.PurchaseRelic(_selectedRelic.Id))
        {
            RefreshUI();
        }
    }
    
    private void OnEquipPressed()
    {
        if (_selectedRelic == null) return;
        
        if (RelicSystem.Instance.EquipRelic(_selectedRelic.Id))
        {
            RefreshUI();
        }
    }
    
    private void OnUnequipPressed()
    {
        if (_selectedRelic == null) return;
        
        if (RelicSystem.Instance.UnequipRelic(_selectedRelic.Id))
        {
            RefreshUI();
        }
    }
    
    private void OnUnlockSlotPressed()
    {
        int cost = 500;
        
        var player = GetTree().CurrentScene.GetNodeOrNull<Player>("Player");
        if (player != null && player.Gold >= cost)
        {
            if (RelicSystem.Instance.UnlockRelicSlot(cost))
            {
                RefreshUI();
            }
        }
        else
        {
            GD.Print("[RelicUI] Not enough gold to unlock slot");
        }
    }
    
    private void OnRelicPurchased(string relicId)
    {
        RefreshUI();
    }
    
    private void OnRelicEquipped(string relicId)
    {
        RefreshUI();
    }
    
    private void OnRelicUnequipped(string relicId)
    {
        RefreshUI();
    }
    
    private void OnRelicSlotUnlocked(int newSlotCount)
    {
        RefreshUI();
    }
    
    #region 辅助方法
    
    private Color GetRarityColor(RelicRarity rarity)
    {
        return rarity switch
        {
            RelicRarity.Common => Colors.Gray,
            RelicRarity.Uncommon => Colors.Green,
            RelicRarity.Rare => Colors.Blue,
            RelicRarity.Epic => Colors.Magenta,
            RelicRarity.Legendary => new Color(1, 0.5f, 0),
            _ => Colors.White
        };
    }
    
    private string GetRarityName(RelicRarity rarity)
    {
        return rarity switch
        {
            RelicRarity.Common => "普通",
            RelicRarity.Uncommon => "优秀",
            RelicRarity.Rare => "稀有",
            RelicRarity.Epic => "史诗",
            RelicRarity.Legendary => "传说",
            _ => "未知"
        };
    }
    
    private string GetTypeName(RelicType type)
    {
        return type switch
        {
            RelicType.Attack => "攻击型",
            RelicType.Defense => "防御型",
            RelicType.Support => "辅助型",
            RelicType.Special => "特殊型",
            RelicType.Utility => "工具型",
            _ => "未知"
        };
    }
    
    private string GetAttributeName(string attr)
    {
        return attr switch
        {
            "attack" => "攻击力",
            "defense" => "防御力",
            "health" => "生命值",
            "speed" => "速度",
            "crit_rate" => "暴击率",
            "crit_damage" => "暴击伤害",
            "lifesteal" => "生命偷取",
            "dodge" => "闪避率",
            "max_mana" => "最大法力",
            _ => attr
        };
    }
    
    private string GetSpecialEffectName(string effect)
    {
        return effect switch
        {
            "dragon_strike" => "龙息：攻击时有几率造成巨量伤害",
            "phantom_strike" => "幻影打击：额外攻击",
            "regeneration" => "生命回复",
            "thorns" => "荆棘反伤",
            "phoenix_blessing" => "凤凰祝福：抵挡致命伤害",
            "last_stand" => "殊死一搏：低血量无敌",
            "exp_boost" => "经验加成",
            "drop_boost" => "掉落加成",
            "quest_reward_boost" => "任务奖励加成",
            "cooldown_reduction" => "冷却缩减",
            "gold_boost" => "金币加成",
            "daily_blessing" => "每日祝福",
            "soul_gather" => "灵魂收集",
            "teleport" => "传送能力",
            "discount" => "商店折扣",
            "identify" => "物品鉴定",
            _ => effect
        };
    }
    
    #endregion
    
    public override void _Input(InputEvent evt)
    {
        if (evt.IsActionPressed("ui_cancel"))
        {
            Hide();
        }
        
        // R键切换显示
        if (evt.IsActionPressed("ui_relic"))
        {
            if (Visible)
                Hide();
            else
                Show();
        }
    }
    
    public void Toggle()
    {
        if (Visible)
            Hide();
        else
            Show();
    }
}
