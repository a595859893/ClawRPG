using System;
using System.Collections.Generic;
using Godot;

public class PlayerTalentUI : Control
{
    private Control _mainContainer;
    private VBoxContainer _treePanelContainer;
    private Label _pointsLabel;
    private Label _treeTitleLabel;
    private Label _talentNameLabel;
    private Label _talentDescLabel;
    private Label _talentBonusLabel;
    private Button _unlockButton;
    private OptionButton _treeSelector;
    
    private PlayerTalentData.TalentTree _currentTree = PlayerTalentData.TalentTree.Combat;
    private PlayerTalentData.TalentNode _selectedTalent;
    
    public override void _Ready()
    {
        SetupUI();
        Hide();
    }
    
    private void SetupUI()
    {
        // 主面板
        _panel = new PanelContainer
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            OffsetLeft = 300,
            OffsetRight = -300,
            OffsetTop = 100,
            OffsetBottom = -100
        };
        AddChild(_panel);
        
        var mainVBox = new VBoxContainer { };
        _panel.AddChild(mainVBox);
        
        // 标题
        var titleLabel = new Label
        {
            Text = "  玩家天赋系统  ",
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);
        
        // 顶部控制栏
        var topHBox = new HBoxContainer { };
        mainVBox.AddChild(topHBox);
        
        // 天赋系选择
        var treeLabel = new Label { Text = "天赋系:" };
        topHBox.AddChild(treeLabel);
        
        _treeSelector = new OptionButton();
        foreach (PlayerTalentData.TalentTree tree in Enum.GetValues(typeof(PlayerTalentData.TalentTree)))
        {
            string treeName = tree switch
            {
                PlayerTalentData.TalentTree.Combat => "战斗系",
                PlayerTalentData.TalentTree.Defense => "防御系",
                PlayerTalentData.TalentTree.Support => "辅助系",
                PlayerTalentData.TalentTree.Agility => "敏捷系",
                _ => tree.ToString()
            };
            _treeSelector.AddItem(treeName, (int)tree);
        }
        _treeSelector.Selected = 0;
        _treeSelector.ItemSelected += OnTreeSelected;
        topHBox.AddChild(_treeSelector);
        
        // 天赋点数显示
        _pointsLabel = new Label { Text = "可用点数: 3" };
        _pointsLabel.HorizontalAlignment = HorizontalAlignment.Right;
        topHBox.AddChild(_pointsLabel);
        
        // 天赋列表和详情分割
        var splitContainer = new HSplitContainer { };
        splitContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        splitContainer.SplitOffset = 400;
        mainVBox.AddChild(splitContainer);
        
        // 左侧：天赋树显示
        _treeContainer = new VBoxContainer { };
        _treeContainer.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        splitContainer.AddChild(_treeContainer);
        
        _treeTitleLabel = new Label
        {
            Text = "  战斗系天赋  ",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _treeTitleLabel.AddThemeFontSizeOverride("font_size", 18);
        _treeContainer.AddChild(_treeTitleLabel);
        
        // 渲染天赋列表
        RefreshTalentList();
        
        // 右侧：天赋详情
        var detailPanel = new VBoxContainer { };
        detailPanel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        splitContainer.AddChild(detailPanel);
        
        var detailTitle = new Label
        {
            Text = "  天赋详情  ",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        detailTitle.AddThemeFontSizeOverride("font_size", 18);
        detailPanel.AddChild(detailTitle);
        
        _talentNameLabel = new Label
        {
            Text = "选择一个天赋查看详情",
            HorizontalAlignment = HorizontalAlignment.Center
        };
        _talentNameLabel.AddThemeFontSizeOverride("font_size", 16);
        detailPanel.AddChild(_talentNameLabel);
        
        _talentDescLabel = new Label
        {
            Text = "",
            AutowrapMode = TextServer.AutowrapMode.Word
        };
        _talentDescLabel.CustomMinimumSize = new Vector2(0, 60);
        detailPanel.AddChild(_talentDescLabel);
        
        _talentBonusLabel = new Label
        {
            Text = "",
            Modulate = new Color(1f, 0.9f, 0.5f)
        };
        detailPanel.AddChild(_talentBonusLabel);
        
        _unlockButton = new Button
        {
            Text = "解锁天赋",
            CustomMinimumSize = new Vector2(200, 40)
        };
        _unlockButton.Pressed += OnUnlockPressed;
        detailPanel.AddChild(_unlockButton);
        
        // 关闭按钮
        var closeButton = new Button
        {
            Text = "关闭 (T)"
        };
        closeButton.Pressed += () => Hide();
        mainVBox.AddChild(closeButton);
        
        // 动画效果
        var tween = CreateTween();
        tween.TweenProperty(_panel, "modulate:a", 0f, 0f);
        tween.TweenProperty(_panel, "modulate:a", 1f, 0.3f);
    }
    
    private void OnTreeSelected(int index)
    {
        _currentTree = (PlayerTalentData.TalentTree)index;
        RefreshTalentList();
        UpdateTreeTitle();
    }
    
    private void UpdateTreeTitle()
    {
        string treeName = _currentTree switch
        {
            PlayerTalentData.TalentTree.Combat => "战斗系天赋",
            PlayerTalentData.TalentTree.Defense => "防御系天赋",
            PlayerTalentData.TalentTree.Support => "辅助系天赋",
            PlayerTalentData.TalentTree.Agility => "敏捷系天赋",
            _ => _currentTree.ToString()
        };
        _treeTitleLabel.Text = $"  {treeName}  ";
    }
    
    private void RefreshTalentList()
    {
        // 清除旧的天赋显示
        foreach (Node child in _treeContainer.GetChildren())
        {
            if (child is Label || child is HBoxContainer) continue;
            child.QueueFree();
        }
        
        // 按Tier分组显示
        var talents = PlayerTalentDatabase.Instance.GetTalentsByTree(_currentTree);
        int currentTier = 0;
        
        foreach (var talent in talents)
        {
            if (talent.Tier != currentTier)
            {
                currentTier = talent.Tier;
                var tierLabel = new Label
                {
                    Text = $"=== 第 {currentTier} 层 ===",
                    HorizontalAlignment = HorizontalAlignment.Center
                };
                tierLabel.AddThemeFontSizeOverride("font_size", 14);
                _treeContainer.AddChild(tierLabel);
            }
            
            var talentRow = CreateTalentRow(talent);
            _treeContainer.AddChild(talentRow);
        }
    }
    
    private HBoxContainer CreateTalentRow(PlayerTalentData.TalentNode talent)
    {
        var row = new HBoxContainer { };
        
        bool isUnlocked = PlayerTalentSystem.Instance.PlayerData.UnlockedTalents.Contains(talent.Id);
        bool canUnlock = PlayerTalentSystem.Instance.CanUnlockTalent(talent.Id);
        
        // 天赋名称按钮
        var talentButton = new Button
        {
            Text = talent.Name,
            CustomMinimumSize = new Vector2(180, 30),
            Disabled = !canUnlock && !isUnlocked
        };
        
        // 颜色根据状态
        if (isUnlocked)
            talentButton.Modulate = new Color(0.3f, 1f, 0.3f);  // 绿色 - 已解锁
        else if (canUnlock)
            talentButton.Modulate = new Color(1f, 1f, 0.3f);  // 黄色 - 可解锁
        else
            talentButton.Modulate = new Color(0.5f, 0.5f, 0.5f);  // 灰色 - 未解锁
        
        talentButton.Pressed += () => OnTalentSelected(talent);
        row.AddChild(talentButton);
        
        // 消耗点数
        var costLabel = new Label
        {
            Text = $" ({talent.Cost}点)",
            CustomMinimumSize = new Vector2(50, 0)
        };
        row.AddChild(costLabel);
        
        return row;
    }
    
    private void OnTalentSelected(PlayerTalentData.TalentNode talent)
    {
        _selectedTalent = talent;
        
        _talentNameLabel.Text = talent.Name;
        _talentDescLabel.Text = talent.Description;
        
        // 显示属性加成
        string bonusText = "属性加成:\n";
        foreach (var bonus in talent.Bonuses)
        {
            string bonusName = bonus.Key switch
            {
                "attack_flat" => "攻击力",
                "attack_percent" => "攻击力%",
                "defense_flat" => "防御力",
                "defense_percent" => "防御力%",
                "health_flat" => "生命值",
                "health_percent" => "生命值%",
                "move_speed" => "移动速度",
                "attack_speed" => "攻击速度",
                "crit_rate" => "暴击率",
                "crit_damage" => "暴击伤害",
                "dodge" => "闪避率",
                "lifesteal" => "生命偷取",
                "exp_bonus" => "经验加成",
                "gold_bonus" => "金币加成",
                "drop_rate" => "掉落率",
                "rare_drop" => "稀有掉落",
                "health_regen" => "生命恢复",
                "sell_price" => "出售价格",
                "enhance_success" => "强化成功率",
                _ => bonus.Key
            };
            
            string valueStr = bonus.Value >= 1 ? $"+{bonus.Value:P0}" : $"+{bonus.Value:P1}";
            bonusText += $"• {bonusName}: {valueStr}\n";
        }
        
        // 检查前置
        if (talent.Requires.Count > 0)
        {
            bonusText += $"\n前置: {string.Join(", ", talent.Requires)}\n";
        }
        
        _talentBonusLabel.Text = bonusText;
        
        // 更新按钮状态
        bool isUnlocked = PlayerTalentSystem.Instance.PlayerData.UnlockedTalents.Contains(talent.Id);
        bool canUnlock = PlayerTalentSystem.Instance.CanUnlockTalent(talent.Id);
        
        if (isUnlocked)
        {
            _unlockButton.Text = "已解锁";
            _unlockButton.Disabled = true;
        }
        else if (canUnlock)
        {
            _unlockButton.Text = $"解锁 (消耗{talent.Cost}点)";
            _unlockButton.Disabled = false; 
        }
        else
        {
            _unlockButton.Text = "条件不足";
            _unlockButton.Disabled = true;
        }
    }
    
    private void OnUnlockPressed()
    {
        if (_selectedTalent == null) return;
        
        if (PlayerTalentSystem.Instance.UnlockTalent(_selectedTalent.Id))
        {
            RefreshTalentList();
            OnTalentSelected(_selectedTalent);
            UpdatePointsLabel();
        }
    }
    
    private void UpdatePointsLabel()
    {
        _pointsLabel.Text = $"可用点数: {PlayerTalentSystem.Instance.AvailablePoints}";
    }
    
    public override void _Input(InputEvent e)
    {
        if (e is InputEventKey key && key.Pressed && key.Keycode == Key.T)
        {
            Toggle();
        }
    }
    
    public void Toggle()
    {
        if (Visible)
            Hide();
        else
            Show();
    }
    
    public new void Show()
    {
        Modulate = new Color(1f, 1f, 1f, 0f);
        Visible = true;
        RefreshTalentList();
        UpdatePointsLabel();
        
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 1f, 0.3f);
    }
    
    public new void Hide()
    {
        var tween = CreateTween();
        tween.TweenProperty(this, "modulate:a", 0f, 0.2f);
        tween.TweenCallback(Callable.From(() => Visible = false));
    }
}
