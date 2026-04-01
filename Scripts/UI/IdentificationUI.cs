using Godot;
using System;
using System.Collections.Generic;

public partial class IdentificationUI : Control
{
    private Label _titleLabel;
    private Label _statsLabel;
    private VBoxContainer _methodContainer;
    private Button _identifyButton;
    private Button _closeButton;
    private OptionButton _methodOption;
    private Label _costLabel;
    private Label _resultLabel;
    private ColorRect _bg;
    
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        SetupUI();
        Hide();
    }
    
    private void SetupUI()
    {
        // Background
        _bg = new ColorRect();
        _bg.Color = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        _bg.SetAnchorPreset(Control.LayoutPreset.Center);
        _bg.CustomMinimumSize = new Vector2(500, 450);
        AddChild(_bg);
        
        // Title
        _titleLabel = new Label();
        _titleLabel.Text = "⚗️ 装备鉴定系统";
        _titleLabel.SetAnchorPreset(Control.LayoutPreset.TopWide);
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.Position = new Vector2(0, 10);
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        _bg.AddChild(_titleLabel);
        
        // Stats Label
        _statsLabel = new Label();
        _statsLabel.Text = "统计信息";
        _statsLabel.SetAnchorPreset(Control.LayoutPreset.TopWide);
        _statsLabel.Position = new Vector2(0, 50);
        _statsLabel.Align = Label.AlignEnum.Center;
        _statsLabel.AddThemeFontSizeOverride("font_size", 16);
        _bg.AddChild(_statsLabel);
        
        // Method Selection
        var methodLabel = new Label();
        methodLabel.Text = "选择鉴定方法:";
        methodLabel.Position = new Vector2(30, 130);
        _bg.AddChild(methodLabel);
        
        _methodOption = new OptionButton();
        _methodOption.Position = new Vector2(30, 160);
        _methodOption.CustomMinimumSize = new Vector2(200, 40);
        
        _methodOption.AddItem("免费鉴定 (1-2属性)", 0);
        _methodOption.AddItem("标准鉴定 (100金, 2-3属性)", 1);
        _methodOption.AddItem("高级鉴定 (500金, 3-4属性)", 2);
        _methodOption.AddItem("高级鉴定 (2000金, 4-5属性)", 3);
        
        _methodOption.ItemSelected += OnMethodChanged;
        _bg.AddChild(_methodOption);
        
        // Cost Label
        _costLabel = new Label();
        _costLabel.Text = "费用: 0 金币";
        _costLabel.Position = new Vector2(250, 160);
        _costLabel.AddThemeFontSizeOverride("font_size", 16);
        _bg.AddChild(_costLabel);
        
        // Identify Button
        _identifyButton = new Button();
        _identifyButton.Text = "🔮 开始鉴定";
        _identifyButton.Position = new Vector2(30, 220);
        _identifyButton.CustomMinimumSize = new Vector2(200, 50);
        _identifyButton.Pressed += OnIdentifyPressed;
        _bg.AddChild(_identifyButton);
        
        // Result Label
        _resultLabel = new Label();
        _resultLabel.Text = "鉴定结果将显示在这里...";
        _resultLabel.Position = new Vector2(30, 290);
        _resultLabel.CustomMinimumSize = new Vector2(440, 100);
        _resultLabel.Align = Label.AlignEnum.Center;
        _resultLabel.AddThemeFontSizeOverride("font_size", 14);
        _bg.AddChild(_resultLabel);
        
        // Close Button
        _closeButton = new Button();
        _closeButton.Text = "关闭";
        _closeButton.Position = new Vector2(350, 390);
        _closeButton.CustomMinimumSize = new Vector2(120, 40);
        _closeButton.Pressed += OnClosePressed;
        _bg.AddChild(_closeButton);
        
        UpdateStats();
    }
    
    private void OnMethodChanged(int index)
    {
        int cost = 0;
        switch (index)
        {
            case 0: cost = 0; break;
            case 1: cost = 100; break;
            case 2: cost = 500; break;
            case 3: cost = 2000; break;
        }
        _costLabel.Text = $"费用: {cost} 金币";
    }
    
    private void OnIdentifyPressed()
    {
        var identification = IdentificationSystem.GetInstance();
        if (identification == null)
        {
            _resultLabel.Text = "错误: 鉴定系统未初始化";
            return;
        }
        
        int index = _methodOption.Selected;
        var method = (IdentificationSystem.IdentificationMethod)index;
        int cost = IdentificationSystem.GetIdentificationCost(method);
        
        // 检查金币
        if (Player.Instance != null && Player.Instance.gold < cost)
        {
            _resultLabel.Text = $"金币不足! 需要 {cost} 金币";
            return;
        }
        
        // 扣除金币
        if (cost > 0 && Player.Instance != null)
        {
            Player.Instance.gold -= cost;
        }
        
        // 随机装备稀有度
        string[] rarities = {"Common", "Uncommon", "Rare", "Epic", "Legendary", "Mythical"};
        string rarity = rarities[(int)(GD.Randf() * rarities.Length)];
        
        // 执行鉴定
        var attributes = identification.IdentifyEquipment(rarity, method);
        
        // 显示结果
        string resultText = $"[color=#FFD700]稀有度: {rarity}[/color]\n\n";
        resultText += "鉴定属性:\n";
        
        foreach (var attr in attributes)
        {
            string attrName = GetAttributeDisplayName(attr.Key);
            string color = GetAttributeColor(attr.Key);
            resultText += $"• [color={color}]{attrName}: +{attr.Value}[/color]\n";
        }
        
        _resultLabel.Text = resultText;
        UpdateStats();
    }
    
    private string GetAttributeDisplayName(string attr)
    {
        switch (attr)
        {
            case "attack": return "攻击力";
            case "defense": return "防御力";
            case "health": return "生命值";
            case "magic": return "魔法值";
            case "speed": return "速度";
            case "crit_rate": return "暴击率";
            case "crit_damage": return "暴击伤害";
            case "lifesteal": return "生命偷取";
            case "dodge": return "闪避";
            case "fire_resist": return "火焰抗性";
            case "ice_resist": return "冰霜抗性";
            case "lightning_resist": return "雷电抗性";
            case "dark_resist": return "暗影抗性";
            case "holy_resist": return "神圣抗性";
            case "exp_bonus": return "经验加成";
            case "gold_bonus": return "金币加成";
            case "drop_bonus": return "掉落加成";
            case "regen": return "生命恢复";
            default: return attr;
        }
    }
    
    private string GetAttributeColor(string attr)
    {
        switch (attr)
        {
            case "attack": return "#FF6B6B";
            case "defense": return "#4ECDC4";
            case "health": return "#95E1D3";
            case "magic": return "#A8E6CF";
            case "speed": return "#FFEAA7";
            case "crit_rate": return "#FD79A8";
            case "crit_damage": return "#E84393";
            case "lifesteal": return "#D63031";
            case "dodge": return "#74B9FF";
            case "fire_resist": return "#FF7675";
            case "ice_resist": return "#81ECEC";
            case "lightning_resist": return "#FDCB6E";
            case "dark_resist": return "#2D3436";
            case "holy_resist": return "#FFEAA7";
            case "exp_bonus": return "#00CEC9";
            case "gold_bonus": return "#FDCB6E";
            case "drop_bonus": return "#6C5CE7";
            case "regen": return "#00B894";
            default: return "#FFFFFF";
        }
    }
    
    private void UpdateStats()
    {
        var identification = IdentificationSystem.GetInstance();
        if (identification == null) return;
        
        var stats = identification.GetStatistics();
        
        string[] rarityNames = {"无", "普通", "优秀", "稀有", "史诗", "传说", "神话"};
        int highestRarity = Convert.ToInt32(stats["highest_rarity"]);
        string rarityName = highestRarity < rarityNames.Length ? rarityNames[highestRarity] : "神话";
        
        _statsLabel.Text = $"总鉴定次数: {stats["total_identifications"]} | 最高稀有度: {rarityName}";
    }
    
    private void OnClosePressed()
    {
        ToggleUI();
    }
    
    public void ToggleUI()
    {
        _isVisible = !_isVisible;
        
        if (_isVisible)
        {
            Show();
            UpdateStats();
        }
        else
        {
            Hide();
        }
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel") && _isVisible)
        {
            ToggleUI();
        }
    }
}

