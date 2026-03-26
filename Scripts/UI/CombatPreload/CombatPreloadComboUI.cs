using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 战斗前Combo预览UI
/// 在战斗开始前展示可用的Combo序列，让玩家选择和确认
/// </summary>
public partial class CombatPreloadComboUI : Control
{
    // 引用
    private CombatPreloadComboSystem _preloadSystem;
    
    // UI元素
    private PanelContainer _mainPanel;
    private Label _titleLabel;
    private Label _subtitleLabel;
    private ScrollContainer _comboScroll;
    private VBoxContainer _comboListContainer;
    private Button _confirmButton;
    private Button _cancelButton;
    private Label _comboLevelLabel;
    
    // 状态
    private string _selectedComboId = null;
    private bool _isVisible = false;
    
    // Combo卡片预制体路径（可选）
    private PackedScene _comboCardScene;
    
    public override void _Ready()
    {
        _preloadSystem = GetNodeOrNull<CombatPreloadComboSystem>("/root/Game/CombatPreloadComboSystem");
        if (_preloadSystem == null)
        {
            GD.PrintErr("[CombatPreloadComboUI] CombatPreloadComboSystem not found!");
            return;
        }
        
        _SetupUI();
        _ConnectSignals();
        
        // 默认隐藏
        Visible = false;
    }

    private void _SetupUI()
    {
        // 主面板
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchor(AnchorPreset.FullRect);
        AddChild(_mainPanel);
        
        // 使用主题样式
        var style = new StyleBoxFlat();
        style.BgColor = new Color(0.1f, 0.1f, 0.15f, 0.95f);
        style.SetBorderWidthAll(2);
        style.SetBorderColor(new Color(0.3f, 0.3f, 0.4f));
        style.SetCornerRadiusAll(8);
        style.SetContentMarginAll(20);
        _mainPanel.AddThemeStyleboxOverride("panel", style);
        
        // 主容器
        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchor(AnchorPreset.FullRect);
        mainVBox.AddThemeConstantOverride("separation", 15);
        _mainPanel.AddChild(mainVBox);
        
        // 标题区
        var headerBox = new HBoxContainer();
        headerBox.AddThemeConstantOverride("separation", 10);
        mainVBox.AddChild(headerBox);
        
        _titleLabel = new Label();
        _titleLabel.Text = "⚔️ 战斗前Combo预览";
        _titleLabel.AddThemeFontSizeOverride("font_size", 24);
        headerBox.AddChild(_titleLabel);
        
        headerBox.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlagsExpandFill });
        
        _comboLevelLabel = new Label();
        _comboLevelLabel.Text = "Combo等级: 1";
        _comboLevelLabel.AddThemeFontSizeOverride("font_size", 16);
        _comboLevelLabel.AddThemeColorOverride("font_color", new Color(0.8f, 0.8f, 0.6f));
        headerBox.AddChild(_comboLevelLabel);
        
        // 副标题
        _subtitleLabel = new Label();
        _subtitleLabel.Text = "选择本场战斗计划使用的Combo，确认后再进入战斗";
        _subtitleLabel.AddThemeFontSizeOverride("font_size", 14);
        _subtitleLabel.AddThemeColorOverride("font_color", new Color(0.7f, 0.7f, 0.7f));
        _subtitleLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        mainVBox.AddChild(_subtitleLabel);
        
        // Combo列表（滚动区域）
        _comboScroll = new ScrollContainer();
        _comboScroll.SetHorizontalScrollMode(ScrollContainer.ScrollMode.Disabled);
        _comboScroll.CustomMinimumSize = new Vector2(0, 350);
        mainVBox.AddChild(_comboScroll);
        
        _comboListContainer = new VBoxContainer();
        _comboListContainer.SetAnchor(AnchorPreset.FullRect);
        _comboListContainer.AddThemeConstantOverride("separation", 10);
        _comboScroll.AddChild(_comboListContainer);
        
        // 按钮区
        var buttonBox = new HBoxContainer();
        buttonBox.AddThemeConstantOverride("separation", 15);
        mainVBox.AddChild(buttonBox);
        
        buttonBox.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlagsExpandFill });
        
        _cancelButton = new Button();
        _cancelButton.Text = "取消";
        _cancelButton.CustomMinimumSize = new Vector2(120, 40);
        _cancelButton.Pressed += _OnCancelPressed;
        buttonBox.AddChild(_cancelButton);
        
        _confirmButton = new Button();
        _confirmButton.Text = "确认并进入战斗";
        _confirmButton.CustomMinimumSize = new Vector2(180, 40);
        _confirmButton.Pressed += _OnConfirmPressed;
        _confirmButton.AddThemeColorOverride("font_color", new Color(0.2f, 0.9f, 0.3f));
        buttonBox.AddChild(_confirmButton);
    }

    private void _ConnectSignals()
    {
        CombatPreloadComboSystem.OnPreloadStateChanged += _OnStateChanged;
        CombatPreloadComboSystem.OnCombosUpdated += _OnCombosUpdated;
        CombatPreloadComboSystem.OnComboConfirmed += _OnComboConfirmed;
    }

    private void _OnStateChanged(CombatPreloadState state)
    {
        switch (state)
        {
            case CombatPreloadState.Showing:
                Visible = true;
                _isVisible = true;
                _RefreshUI();
                break;
            case CombatPreloadState.Hidden:
            case CombatPreloadState.Cancelled:
                Visible = false;
                _isVisible = false;
                _selectedComboId = null;
                break;
            case CombatPreloadState.Confirmed:
                // 确认后隐藏
                Visible = false;
                _isVisible = false;
                break;
        }
    }

    private void _OnCombosUpdated(List<CombatPreloadComboEntry> combos)
    {
        _RebuildComboList(combos);
    }

    private void _OnComboConfirmed(string comboId)
    {
        GD.Print($"[CombatPreloadComboUI] Combo confirmed: {comboId}");
    }

    private void _RefreshUI()
    {
        if (_preloadSystem != null)
        {
            _comboLevelLabel.Text = $"Combo等级: {_preloadSystem.GetPlayerComboLevel()}";
            _RebuildComboList(_preloadSystem.GetAvailableCombos());
        }
    }

    private void _RebuildComboList(List<CombatPreloadComboEntry> combos)
    {
        // 清除现有列表
        foreach (var child in _comboListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        // 按类型分组显示
        CombatPreloadComboType? currentType = null;
        
        foreach (var combo in combos)
        {
            // 类型标题
            if (currentType != combo.ComboType)
            {
                currentType = combo.ComboType;
                var typeLabel = new Label();
                typeLabel.Text = $"── {combo.ComboType} ──";
                typeLabel.AddThemeFontSizeOverride("font_size", 12);
                typeLabel.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
                typeLabel.Align = Label.TextAlign.Center;
                _comboListContainer.AddChild(typeLabel);
            }
            
            // Combo卡片
            var card = _CreateComboCard(combo);
            _comboListContainer.AddChild(card);
        }
        
        if (combos.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "当前没有可用的Combo\n提升Combo等级来解锁更多Combo！";
            emptyLabel.AddThemeFontSizeOverride("font_size", 16);
            emptyLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            emptyLabel.Align = Label.TextAlign.Center;
            _comboListContainer.AddChild(emptyLabel);
        }
    }

    private Control _CreateComboCard(CombatPreloadComboEntry combo)
    {
        var card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(0, 90);
        
        // 卡片背景
        var cardStyle = new StyleBoxFlat();
        cardStyle.BgColor = _GetRarityColor(combo.Rarity) * new Color(0.15f, 0.15f, 0.2f);
        cardStyle.SetBorderWidthAll(1);
        cardStyle.SetBorderColor(_GetRarityColor(combo.Rarity) * new Color(0.5f, 0.5f, 0.5f));
        cardStyle.SetCornerRadiusAll(6);
        cardStyle.SetContentMarginAll(12);
        card.AddThemeStyleboxOverride("panel", cardStyle);
        
        var cardVBox = new VBoxContainer();
        cardVBox.AddThemeConstantOverride("separation", 6);
        card.AddChild(cardVBox);
        
        // 卡片头部：名称 + 稀有度 + 伤害倍率
        var headerBox = new HBoxContainer();
        headerBox.AddThemeConstantOverride("separation", 10);
        cardVBox.AddChild(headerBox);
        
        var nameLabel = new Label();
        nameLabel.Text = combo.ComboName;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        nameLabel.AddThemeColorOverride("font_color", _GetRarityColor(combo.Rarity));
        headerBox.AddChild(nameLabel);
        
        var rarityLabel = new Label();
        rarityLabel.Text = $"[{combo.Rarity}]";
        rarityLabel.AddThemeFontSizeOverride("font_size", 12);
        rarityLabel.AddThemeColorOverride("font_color", _GetRarityColor(combo.Rarity) * new Color(0.8f, 0.8f, 0.8f));
        headerBox.AddChild(rarityLabel);
        
        headerBox.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlagsExpandFill });
        
        var damageLabel = new Label();
        damageLabel.Text = $"伤害: x{combo.DamageMultiplier:F1}";
        damageLabel.AddThemeFontSizeOverride("font_size", 14);
        damageLabel.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0.4f));
        headerBox.AddChild(damageLabel);
        
        // 技能序列
        var sequenceBox = new HBoxContainer();
        cardVBox.AddChild(sequenceBox);
        
        var seqLabel = new Label();
        seqLabel.Text = "序列: ";
        seqLabel.AddThemeFontSizeOverride("font_size", 13);
        seqLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
        sequenceBox.AddChild(seqLabel);
        
        for (int i = 0; i < combo.SkillSequence.Count; i++)
        {
            var skillBtn = new Label();
            skillBtn.Text = $"[{combo.SkillSequence[i]}]";
            skillBtn.AddThemeFontSizeOverride("font_size", 13);
            
            if (i < combo.CurrentProgress)
            {
                skillBtn.AddThemeColorOverride("font_color", new Color(0.3f, 1f, 0.3f));
            }
            else if (i == combo.CurrentProgress)
            {
                skillBtn.AddThemeColorOverride("font_color", new Color(1f, 0.9f, 0.3f));
            }
            else
            {
                skillBtn.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.6f));
            }
            
            sequenceBox.AddChild(skillBtn);
            
            if (i < combo.SkillSequence.Count - 1)
            {
                var arrow = new Label();
                arrow.Text = " → ";
                arrow.AddThemeFontSizeOverride("font_size", 13);
                arrow.AddThemeColorOverride("font_color", new Color(0.4f, 0.4f, 0.5f));
                sequenceBox.AddChild(arrow);
            }
        }
        
        // 描述 + 奖励
        var footerBox = new HBoxContainer();
        cardVBox.AddChild(footerBox);
        
        var descLabel = new Label();
        descLabel.Text = combo.Description;
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        descLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.7f));
        descLabel.SizeFlagsHorizontal = Control.SizeFlagsExpandFill;
        footerBox.AddChild(descLabel);
        
        var rewardLabel = new Label();
        rewardLabel.Text = $"+{combo.ComboPointReward} CP";
        rewardLabel.AddThemeFontSizeOverride("font_size", 12);
        rewardLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.7f, 0.2f));
        footerBox.AddChild(rewardLabel);
        
        // 点击选择
        var selectBtn = new Button();
        selectBtn.CustomMinimumSize = new Vector2(80, 28);
        selectBtn.Text = _selectedComboId == combo.ComboId ? "已选择" : "选择";
        selectBtn.Pressed += () => _OnComboCardSelected(combo.ComboId);
        
        var btnContainer = new HBoxContainer();
        btnContainer.AddChild(new Control { SizeFlagsHorizontal = Control.SizeFlagsExpandFill });
        btnContainer.AddChild(selectBtn);
        cardVBox.AddChild(btnContainer);
        
        return card;
    }

    private Color _GetRarityColor(CombatPreloadComboRarity rarity)
    {
        return rarity switch
        {
            CombatPreloadComboRarity.Common => new Color(0.7f, 0.7f, 0.7f),
            CombatPreloadComboRarity.Uncommon => new Color(0.3f, 0.9f, 0.3f),
            CombatPreloadComboRarity.Rare => new Color(0.3f, 0.5f, 1f),
            CombatPreloadComboRarity.Epic => new Color(0.7f, 0.3f, 1f),
            CombatPreloadComboRarity.Legendary => new Color(1f, 0.6f, 0.1f),
            _ => new Color(0.7f, 0.7f, 0.7f)
        };
    }

    private void _OnComboCardSelected(string comboId)
    {
        _selectedComboId = comboId;
        _preloadSystem.ConfirmCombo(comboId);
        
        // 刷新列表以更新按钮状态
        if (_preloadSystem != null)
        {
            _RebuildComboList(_preloadSystem.GetAvailableCombos());
        }
    }

    private void _OnConfirmPressed()
    {
        if (_preloadSystem != null)
        {
            _preloadSystem.ConfirmAndEnterCombat();
        }
    }

    private void _OnCancelPressed()
    {
        if (_preloadSystem != null)
        {
            _preloadSystem.Cancel();
        }
    }

    public override void _ExitTree()
    {
        if (_preloadSystem != null)
        {
            CombatPreloadComboSystem.OnPreloadStateChanged -= _OnStateChanged;
            CombatPreloadComboSystem.OnCombosUpdated -= _OnCombosUpdated;
            CombatPreloadComboSystem.OnComboConfirmed -= _OnComboConfirmed;
        }
    }
}
