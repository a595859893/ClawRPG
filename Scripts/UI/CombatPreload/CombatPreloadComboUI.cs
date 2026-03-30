using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems.CombatPreload;

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
    
    // REQ-121: Buyback UI elements
    private Label _countdownLabel;
    private Label _comboPointLabel;
    private HBoxContainer _countdownBox;
    
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
        
        // REQ-121: Combo Point display
        _comboPointLabel = new Label();
        _comboPointLabel.Text = "🔄 Combo Point: 1";
        _comboPointLabel.AddThemeFontSizeOverride("font_size", 16);
        _comboPointLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.7f, 0.2f));
        headerBox.AddChild(_comboPointLabel);
        
        // REQ-121: Countdown box (hidden by default)
        _countdownBox = new HBoxContainer();
        _countdownBox.Visible = false;
        headerBox.AddChild(_countdownBox);
        
        var countdownIcon = new Label();
        countdownIcon.Text = "⏱️ ";
        countdownIcon.AddThemeFontSizeOverride("font_size", 16);
        _countdownBox.AddChild(countdownIcon);
        
        _countdownLabel = new Label();
        _countdownLabel.Text = "3.0s";
        _countdownLabel.AddThemeFontSizeOverride("font_size", 18);
        _countdownLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f));
        _countdownBox.AddChild(_countdownLabel);
        
        // 副标题
        _subtitleLabel = new Label();
        _subtitleLabel.Text = "选择本场战斗计划使用的Combo，确认后有3秒倒计时可更换（消耗Combo Point）";
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
        CombatPreloadComboSystem.OnComboPointChanged += _OnComboPointChanged; // REQ-121
        CombatPreloadComboSystem.OnCountdownTick += _OnCountdownTick; // REQ-121
    }

    private void _OnStateChanged(CombatPreloadState state)
    {
        switch (state)
        {
            case CombatPreloadState.Showing:
                Visible = true;
                _isVisible = true;
                _countdownBox.Visible = false;
                _RefreshUI();
                break;
            case CombatPreloadState.CountingDown: // REQ-121
                _countdownBox.Visible = true;
                _UpdateComboPointDisplay();
                _UpdateConfirmButtonText();
                break;
            case CombatPreloadState.Hidden:
            case CombatPreloadState.Cancelled:
                Visible = false;
                _isVisible = false;
                _selectedComboId = null;
                _countdownBox.Visible = false;
                break;
            case CombatPreloadState.Confirmed:
                // 确认后隐藏
                Visible = false;
                _isVisible = false;
                _countdownBox.Visible = false;
                break;
        }
    }

    // REQ-121: Combo Point changed handler
    private void _OnComboPointChanged(int points)
    {
        _UpdateComboPointDisplay();
        _UpdateConfirmButtonText();
        // 刷新列表以更新按钮状态（禁用/启用）
        if (_preloadSystem != null)
        {
            _RebuildComboList(_preloadSystem.GetAvailableCombos());
        }
    }

    // REQ-121: Countdown tick handler
    private void _OnCountdownTick(float secondsRemaining)
    {
        _countdownLabel.Text = $"{secondsRemaining:F1}s";
        // Flash effect when time is low
        if (secondsRemaining <= 1.5f)
        {
            _countdownLabel.AddThemeColorOverride("font_color", new Color(1f, 0.3f, 0.3f));
        }
        else if (secondsRemaining <= 2.5f)
        {
            _countdownLabel.AddThemeColorOverride("font_color", new Color(1f, 0.8f, 0.2f));
        }
    }

    // REQ-121: Update Combo Point display
    private void _UpdateComboPointDisplay()
    {
        if (_preloadSystem != null)
        {
            int points = _preloadSystem.GetComboPoint();
            _comboPointLabel.Text = $"🔄 Combo Point: {points}";
            // Change color when out of points
            if (points <= 0)
            {
                _comboPointLabel.AddThemeColorOverride("font_color", new Color(0.6f, 0.6f, 0.6f));
            }
            else
            {
                _comboPointLabel.AddThemeColorOverride("font_color", new Color(0.9f, 0.7f, 0.2f));
            }
        }
    }

    // REQ-121: Update confirm button text based on state
    private void _UpdateConfirmButtonText()
    {
        if (_preloadSystem == null) return;
        
        if (_preloadSystem.GetState() == CombatPreloadState.CountingDown)
        {
            int points = _preloadSystem.GetComboPoint();
            if (points <= 0)
            {
                _confirmButton.Text = "等待锁定...";
                _confirmButton.Disabled = true;
            }
            else
            {
                _confirmButton.Text = "立即进入战斗";
            }
        }
        else
        {
            _confirmButton.Text = "确认并进入战斗";
            _confirmButton.Disabled = false;
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
        // REQ-128: 显示有效伤害倍率（已应用疲劳惩罚），与基础倍率不同时标注
        if (Mathf.Abs(combo.EffectiveDamageMultiplier - combo.DamageMultiplier) > 0.01f)
        {
            damageLabel.Text = $"伤害: x{combo.EffectiveDamageMultiplier:F1} (原x{combo.DamageMultiplier:F1})";
            damageLabel.AddThemeFontSizeOverride("font_size", 14);
            damageLabel.AddThemeColorOverride("font_color", new Color(1f, 0.6f, 0.2f)); // 橙色表示有惩罚
        }
        else
        {
            damageLabel.Text = $"伤害: x{combo.DamageMultiplier:F1}";
            damageLabel.AddThemeFontSizeOverride("font_size", 14);
            damageLabel.AddThemeColorOverride("font_color", new Color(1f, 0.4f, 0.4f));
        }
        headerBox.AddChild(damageLabel);
        
        // REQ-128: 疲劳状态标签
        var fatigueLabel = new Label();
        fatigueLabel.Text = $"[疲劳: {combo.FatigueStatus}]";
        fatigueLabel.AddThemeFontSizeOverride("font_size", 13);
        fatigueLabel.AddThemeColorOverride("font_color", combo.FatigueColor);
        headerBox.AddChild(fatigueLabel);
        
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
        // REQ-121: Disable buyback button when Combo Point is 0 during countdown
        bool isCountingDown = _preloadSystem != null && _preloadSystem.GetState() == CombatPreloadState.CountingDown;
        bool noComboPoint = _preloadSystem != null && _preloadSystem.GetComboPoint() <= 0;
        bool isPendingCombo = _preloadSystem != null && _preloadSystem.GetPendingComboId() == combo.ComboId;
        
        var selectBtn = new Button();
        selectBtn.CustomMinimumSize = new Vector2(80, 28);
        if (isPendingCombo && isCountingDown)
        {
            selectBtn.Text = "锁定中";
            selectBtn.Disabled = true;
        }
        else if (isCountingDown && noComboPoint)
        {
            selectBtn.Text = "无Point";
            selectBtn.Disabled = true;
        }
        else if (_selectedComboId == combo.ComboId && !isCountingDown)
        {
            selectBtn.Text = "已选择";
        }
        else
        {
            selectBtn.Text = isCountingDown ? "更换" : "选择";
        }
        
        if (!selectBtn.Disabled)
        {
            selectBtn.Pressed += () => _OnComboCardSelected(combo.ComboId);
        }
        
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
        
        // REQ-121: If already counting down, this is a buyback (costs 1 Combo Point)
        if (_preloadSystem != null && _preloadSystem.GetState() == CombatPreloadState.CountingDown)
        {
            if (_preloadSystem.GetComboPoint() <= 0)
            {
                GD.Print("[CombatPreloadComboUI] No Combo Point remaining for buyback");
                return;
            }
            _preloadSystem.BuybackCombo(comboId);
        }
        else
        {
            _preloadSystem.ConfirmCombo(comboId);
        }
        
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
            CombatPreloadComboSystem.OnComboPointChanged -= _OnComboPointChanged;
            CombatPreloadComboSystem.OnCountdownTick -= _OnCountdownTick;
        }
    }
}
