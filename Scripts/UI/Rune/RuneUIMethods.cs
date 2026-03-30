using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Systems;

public partial class RuneUI
{
    public override void _Ready()
    {
        _runeSystem = RuneSystem.Instance;
        
        SetupUI();
        ConnectSignals();
        RefreshRuneList();
        RefreshEquippedRunes();
        RefreshStats();
    }
    
    private void SetupUI()
    {
        // Main container
        _mainContainer = new VBoxContainer();
        _mainContainer.SetAnchorPreset(Control.LayoutPreset.FullRect);
        _mainContainer.AddThemeConstantOverride("separation", 10);
        AddChild(_mainContainer);
        
        // Header
        var header = new HBoxContainer();
        _mainContainer.AddChild(header);
        
        var titleLabel = new Label();
        titleLabel.Text = "  ⚔️ 符文系统";
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        header.AddChild(titleLabel);
        
        header.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        var slotsLabel = new Label();
        slotsLabel.Name = "SlotsLabel";
        header.AddChild(slotsLabel);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetVExpand(ExpandMode.Fill);
        _mainContainer.AddChild(_tabContainer);
        
        // Tab 1: My Runes
        var runesTab = new ScrollContainer();
        runesTab.Name = "MyRunes";
        _tabContainer.AddChild(runesTab);
        
        var runesContent = new VBoxContainer();
        runesContent.SetAnchorPreset(Control.LayoutPreset.FullRect);
        runesContent.AddThemeConstantOverride("separation", 10);
        runesTab.AddChild(runesContent);
        
        // Filter bar
        var filterBar = new HBoxContainer();
        filterBar.AddThemeConstantOverride("separation", 10);
        runesContent.AddChild(filterBar);
        
        var typeLabel = new Label();
        typeLabel.Text = "类型:";
        filterBar.AddChild(typeLabel);
        
        _filterTypeButton = new OptionButton();
        _filterTypeButton.AddItem("全部", 0);
        _filterTypeButton.AddItem("攻击", (int)RuneType.Offensive);
        _filterTypeButton.AddItem("防御", (int)RuneType.Defensive);
        _filterTypeButton.AddItem("工具", (int)RuneType.Utility);
        _filterTypeButton.AddItem("特殊", (int)RuneType.Special);
        _filterTypeButton.ItemSelected += OnFilterTypeChanged;
        filterBar.AddChild(_filterTypeButton);
        
        var rarityLabel = new Label();
        rarityLabel.Text = "  稀有度:";
        filterBar.AddChild(rarityLabel);
        
        _filterRarityButton = new OptionButton();
        _filterRarityButton.AddItem("全部", 0);
        _filterRarityButton.AddItem("普通", (int)RuneRarity.Common);
        _filterRarityButton.AddItem("优秀", (int)RuneRarity.Uncommon);
        _filterRarityButton.AddItem("稀有", (int)RuneRarity.Rare);
        _filterRarityButton.AddItem("史诗", (int)RuneRarity.Epic);
        _filterRarityButton.AddItem("传说", (int)RuneRarity.Legendary);
        _filterRarityButton.ItemSelected += OnFilterRarityChanged;
        filterBar.AddChild(_filterRarityButton);
        
        filterBar.AddChild(new Control() { SizeFlagsHorizontal = Control.SizeFlags.Expand });
        
        _runeCountLabel = new Label();
        _runeCountLabel.Text = "符文: 0/0";
        filterBar.AddChild(_runeCountLabel);
        
        // Rune grid
        _runeGrid = new GridContainer();
        _runeGrid.Columns = 4;
        _runeGrid.AddThemeConstantOverride("h_separation", 10);
        _runeGrid.AddThemeConstantOverride("v_separation", 10);
        _runeGrid.SetVExpand(ExpandMode.Fill);
        runesContent.AddChild(_runeGrid);
        
        // Tab 2: Equipped
        var equippedTab = new ScrollContainer();
        equippedTab.Name = "Equipped";
        _tabContainer.AddChild(equippedTab);
        
        _equippedContainer = new VBoxContainer();
        _equippedContainer.SetAnchorPreset(Control.LayoutPreset.FullRect);
        _equippedContainer.AddThemeConstantOverride("separation", 15);
        equippedTab.AddChild(_equippedContainer);
        
        SetupEquippedSlots();
        
        // Tab 3: Stats
        var statsTab = new ScrollContainer();
        statsTab.Name = "Stats";
        _tabContainer.AddChild(statsTab);
        
        _statsContainer = new VBoxContainer();
        _statsContainer.SetAnchorPreset(Control.LayoutPreset.FullRect);
        _statsContainer.AddThemeConstantOverride("separation", 10);
        statsTab.AddChild(_statsContainer);
        
        SetupStatsPanel();
        
        // Details panel (overlay)
        SetupDetailsPanel();
        
        // Update slots label
        UpdateSlotsLabel();
    }
    
    private void SetupEquippedSlots()
    {
        var slotsLabel = new Label();
        slotsLabel.Text = "已装备的符文";
        slotsLabel.AddThemeFontSizeOverride("font_size", 18);
        _equippedContainer.AddChild(slotsLabel);
        
        var slotsGrid = new GridContainer();
        slotsGrid.Columns = 4;
        slotsGrid.AddThemeConstantOverride("h_separation", 10);
        slotsGrid.AddThemeConstantOverride("v_separation", 10);
        _equippedContainer.AddChild(slotsGrid);
        
        foreach (RuneSlotType slot in Enum.GetValues(typeof(RuneSlotType)))
        {
            if (slot == RuneSlotType.Any) continue;
            
            var slotContainer = new VBoxContainer();
            slotContainer.Alignment = BoxContainer.AlignmentMode.Center;
            
            var slotLabel = new Label();
            slotLabel.Text = RuneSystem.GetSlotName(slot);
            slotLabel.HorizontalAlignment = HorizontalAlignment.Center;
            slotContainer.AddChild(slotLabel);
            
            var slotButton = new Button();
            slotButton.CustomMinimumSize = new Vector2(80, 80);
            slotButton.Text = "空";
            slotButton.Pressed += () => OnEquippedSlotPressed(slot);
            _equippedSlots[slot] = slotButton;
            slotContainer.AddChild(slotButton);
            
            slotsGrid.AddChild(slotContainer);
        }
        
        // Unequip button
        var unequipButton = new Button();
        unequipButton.Text = "卸下选中符文";
        unequipButton.Pressed += OnUnequipPressed;
        _equippedContainer.AddChild(unequipButton);
    }
    
    private void SetupStatsPanel()
    {
        var statsTitle = new Label();
        statsTitle.Text = "符文总加成";
        statsTitle.AddThemeFontSizeOverride("font_size", 20);
        _statsContainer.AddChild(statsTitle);
        
        var statsGrid = new GridContainer();
        statsGrid.Columns = 2;
        statsGrid.AddThemeConstantOverride("h_separation", 20);
        statsGrid.AddThemeConstantOverride("v_separation", 8);
        _statsContainer.AddChild(statsGrid);
        
        _totalAttackLabel = new Label();
        _totalAttackLabel.Text = "攻击力: +0";
        statsGrid.AddChild(_totalAttackLabel);
        
        _totalDefenseLabel = new Label();
        _totalDefenseLabel.Text = "防御力: +0";
        statsGrid.AddChild(_totalDefenseLabel);
        
        _totalHealthLabel = new Label();
        _totalHealthLabel.Text = "生命值: +0";
        statsGrid.AddChild(_totalHealthLabel);
        
        _totalSpeedLabel = new Label();
        _totalSpeedLabel.Text = "速度: +0";
        statsGrid.AddChild(_totalSpeedLabel);
        
        _totalCritRateLabel = new Label();
        _totalCritRateLabel.Text = "暴击率: +0%";
        statsGrid.AddChild(_totalCritRateLabel);
        
        _totalCritDamageLabel = new Label();
        _totalCritDamageLabel.Text = "暴击伤害: +0%";
        statsGrid.AddChild(_totalCritDamageLabel);
        
        _totalLifestealLabel = new Label();
        _totalLifestealLabel.Text = "生命偷取: +0%";
        statsGrid.AddChild(_totalLifestealLabel);
        
        _totalDodgeLabel = new Label();
        _totalDodgeLabel.Text = "闪避率: +0";
        statsGrid.AddChild(_totalDodgeLabel);
        
        _totalBlockLabel = new Label();
        _totalBlockLabel.Text = "格挡率: +0";
        statsGrid.AddChild(_totalBlockLabel);
    }
    
    private void SetupDetailsPanel()
    {
        _detailsPanel = new Panel();
        _detailsPanel.Visible = false;
        _detailsPanel.SetAnchorPreset(Control.LayoutPreset.RightWide);
        _detailsPanel.OffsetLeft = -300;
        _detailsPanel.CustomMinimumSize = new Vector2(280, 0);
        AddChild(_detailsPanel);
        
        var detailsContainer = new VBoxContainer();
        detailsContainer.SetAnchorPreset(Control.LayoutPreset.FullRect);
        detailsContainer.AddThemeConstantOverride("separation", 10);
        detailsContainer.AddThemeConstantOverride("margin_left", 10);
        detailsContainer.AddThemeConstantOverride("margin_right", 10);
        detailsContainer.AddThemeConstantOverride("margin_top", 10);
        detailsContainer.AddThemeConstantOverride("margin_bottom", 10);
        _detailsPanel.AddChild(detailsContainer);
        
        _detailsName = new Label();
        _detailsName.AddThemeFontSizeOverride("font_size", 18);
        detailsContainer.AddChild(_detailsName);
        
        _detailsDescription = new Label();
        _detailsDescription.AutowrapMode = TextServer.AutowrapMode.Word;
        detailsContainer.AddChild(_detailsDescription);
        
        var infoContainer = new VBoxContainer();
        infoContainer.AddThemeConstantOverride("separation", 5);
        detailsContainer.AddChild(infoContainer);
        
        _detailsType = new Label();
        infoContainer.AddChild(_detailsType);
        
        _detailsRarity = new Label();
        infoContainer.AddChild(_detailsRarity);
        
        _detailsSlot = new Label();
        infoContainer.AddChild(_detailsSlot);
        
        _detailsLevel = new Label();
        infoContainer.AddChild(_detailsLevel);
        
        var bonusesTitle = new Label();
        bonusesTitle.Text = "属性加成:";
        bonusesTitle.AddThemeFontSizeOverride("font_size", 14);
        detailsContainer.AddChild(bonusesTitle);
        
        _bonusesContainer = new VBoxContainer();
        detailsContainer.AddChild(_bonusesContainer);
        
        _specialEffectLabel = new Label();
        _specialEffectLabel.Visible = false;
        detailsContainer.AddChild(_specialEffectLabel);
        
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        buttonContainer.AddThemeConstantOverride("separation", 10);
        detailsContainer.AddChild(buttonContainer);
        
        var equipButton = new Button();
        equipButton.Text = "装备";
        equipButton.Pressed += OnEquipPressed;
        buttonContainer.AddChild(equipButton);
        
        var closeButton = new Button();
        closeButton.Text = "关闭";
        closeButton.Pressed += () => _detailsPanel.Visible = false;
        buttonContainer.AddChild(closeButton);
    }
    
    private void RefreshRuneList()
    {
        // Clear existing
        foreach (Node child in _runeGrid.GetChildren())
        {
            child.QueueFree();
        }
        
        var allRunes = _runeSystem.GetOwnedRunes();
        var typeFilter = (RuneType)_filterTypeButton.GetSelectedId();
        var rarityFilter = (RuneRarity)_filterRarityButton.GetSelectedId();
        
        var filteredRunes = allRunes;
        
        if (_filterTypeButton.Selected > 0)
        {
            filteredRunes = filteredRunes.FindAll(r => r.Type == typeFilter);
        }
        
        if (_filterRarityButton.Selected > 0)
        {
            filteredRunes = filteredRunes.FindAll(r => r.Rarity == rarityFilter);
        }
        
        _runeCountLabel.Text = $"符文: {filteredRunes.Count}/{allRunes.Count}";
        
        // Create rune cards
        foreach (var rune in filteredRunes)
        {
            var runeCard = CreateRuneCard(rune);
            _runeGrid.AddChild(runeCard);
        }
        
        if (filteredRunes.Count == 0)
        {
            var emptyLabel = new Label();
            emptyLabel.Text = "暂无符文";
            emptyLabel.HorizontalAlignment = HorizontalAlignment.Center;
            _runeGrid.AddChild(emptyLabel);
        }
    }
    
    private Control CreateRuneCard(RuneData rune)
    {
        var card = new PanelContainer();
        card.CustomMinimumSize = new Vector2(100, 100);
        
        var container = new VBoxContainer();
        container.Alignment = BoxContainer.AlignmentMode.Center;
        container.AddThemeConstantOverride("separation", 5);
        card.AddChild(container);
        
        var iconLabel = new Label();
        iconLabel.Text = GetRuneIcon(rune.Type);
        iconLabel.AddThemeFontSizeOverride("font_size", 32);
        iconLabel.HorizontalAlignment = HorizontalAlignment.Center;
        container.AddChild(iconLabel);
        
        var nameLabel = new Label();
        nameLabel.Text = rune.Name;
        nameLabel.HorizontalAlignment = HorizontalAlignment.Center;
        nameLabel.AutowrapMode = TextServer.AutowrapMode.Word;
        nameLabel.CustomMinimumSize = new Vector2(90, 0);
        container.AddChild(nameLabel);
        
        var rarityLabel = new Label();
        rarityLabel.Text = RuneSystem.GetRarityName(rune.Rarity);
        rarityLabel.AddThemeColorOverride("font_color", Color.FromHtml(RuneSystem.GetRarityColor(rune.Rarity)));
        rarityLabel.HorizontalAlignment = HorizontalAlignment.Center;
        rarityLabel.AddThemeFontSizeOverride("font_size", 10);
        container.AddChild(rarityLabel);
        
        // Make clickable
        var button = new Button();
        button.Visible = false;
        button.Pressed += () => OnRuneSelected(rune);
        card.AddChild(button);
        
        // Also connect card input
        card.GuiInput += (InputEvent @event) => {
            if (@event is InputEventMouseButton mouseEvent && mouseEvent.Pressed && mouseEvent.ButtonIndex == MouseButton.Left)
            {
                OnRuneSelected(rune);
            }
        };
        
        return card;
    }
    
    private string GetRuneIcon(RuneType type)
    {
        return type switch
        {
            RuneType.Offensive => "⚔️",
            RuneType.Defensive => "🛡️",
            RuneType.Utility => "⚡",
            RuneType.Special => "✨",
            _ => "💎"
        };
    }
    
    private void ShowRuneDetails(RuneData rune)
    {
        _detailsPanel.Visible = true;
        
        _detailsName.Text = rune.Name;
        _detailsName.AddThemeColorOverride("font_color", Color.FromHtml(RuneSystem.GetRarityColor(rune.Rarity)));
        
        _detailsDescription.Text = rune.Description;
        
        _detailsType.Text = $"类型: {RuneSystem.GetTypeName(rune.Type)}";
        _detailsRarity.Text = $"稀有度: {RuneSystem.GetRarityName(rune.Rarity)}";
        _detailsRarity.AddThemeColorOverride("font_color", Color.FromHtml(RuneSystem.GetRarityColor(rune.Rarity)));
        
        _detailsSlot.Text = $"槽位: {RuneSystem.GetSlotName(rune.SlotType)}";
        _detailsLevel.Text = $"需求等级: {rune.RequiredLevel}";
        
        // Clear and populate bonuses
        foreach (Node child in _bonusesContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        if (rune.AttackBonus > 0) AddBonusLabel($"攻击力 +{rune.AttackBonus}");
        if (rune.DefenseBonus > 0) AddBonusLabel($"防御力 +{rune.DefenseBonus}");
        if (rune.HealthBonus > 0) AddBonusLabel($"生命值 +{rune.HealthBonus}");
        if (rune.SpeedBonus > 0) AddBonusLabel($"速度 +{rune.SpeedBonus}");
        if (rune.CritRateBonus > 0) AddBonusLabel($"暴击率 +{rune.CritRateBonus}%");
        if (rune.CritDamageBonus > 0) AddBonusLabel($"暴击伤害 +{rune.CritDamageBonus}%");
        if (rune.LifeStealBonus > 0) AddBonusLabel($"生命偷取 +{rune.LifeStealBonus}%");
        if (rune.DodgeBonus > 0) AddBonusLabel($"闪避率 +{rune.DodgeBonus}");
        if (rune.BlockBonus > 0) AddBonusLabel($"格挡率 +{rune.BlockBonus}");
        
        // Special effect
        if (!string.IsNullOrEmpty(rune.SpecialEffect))
        {
            _specialEffectLabel.Text = $"特殊效果: {rune.SpecialEffect} (+{rune.SpecialEffectValue}%)";
            _specialEffectLabel.Visible = true;
        }
        else
        {
            _specialEffectLabel.Visible = false;
        }
    }
    
    private void AddBonusLabel(string text)
    {
        var label = new Label();
        label.Text = "• " + text;
        _bonusesContainer.AddChild(label);
    }
    
    private void RefreshEquippedRunes()
    {
        foreach (var kvp in _equippedSlots)
        {
            var slotType = kvp.Key;
            var button = kvp.Value;
            
            var equippedRune = _runeSystem.GetEquippedRune(slotType);
            if (equippedRune != null)
            {
                button.Text = equippedRune.Name;
                button.AddThemeColorOverride("font_color", Color.FromHtml(RuneSystem.GetRarityColor(equippedRune.Rarity)));
            }
            else
            {
                button.Text = "空";
                button.AddThemeColorOverride("font_color", new Color(0.5f, 0.5f, 0.5f));
            }
        }
        
        UpdateSlotsLabel();
    }
    
    private void UpdateSlotsLabel()
    {
        var usedSlots = _runeSystem.GetUsedSlots();
        var totalSlots = _runeSystem.GetTotalSlots();
        
        var header = _mainContainer.GetChild<HBoxContainer>(0);
        var slotsLabel = header.GetNode<Label>("SlotsLabel");
        slotsLabel.Text = $"已装备: {usedSlots}/{totalSlots}";
    }
    
    private void RefreshStats()
    {
        var bonuses = _runeSystem.GetTotalBonuses();
        
        _totalAttackLabel.Text = $"攻击力: +{bonuses["attack"]}";
        _totalDefenseLabel.Text = $"防御力: +{bonuses["defense"]}";
        _totalHealthLabel.Text = $"生命值: +{bonuses["health"]}";
        _totalSpeedLabel.Text = $"速度: +{bonuses["speed"]}";
        _totalCritRateLabel.Text = $"暴击率: +{bonuses["crit_rate"]}%";
        _totalCritDamageLabel.Text = $"暴击伤害: +{bonuses["crit_damage"]}%";
        _totalLifestealLabel.Text = $"生命偷取: +{bonuses["lifesteal"]}%";
        _totalDodgeLabel.Text = $"闪避率: +{bonuses["dodge"]}";
        _totalBlockLabel.Text = $"格挡率: +{bonuses["block"]}";
    }
    
    private void OnEquippedSlotPressed(RuneSlotType slotType)
    {
        // Show unequip option or show available runes for this slot
        var equippedRune = _runeSystem.GetEquippedRune(slotType);
        if (equippedRune != null)
        {
            ShowRuneDetails(equippedRune);
        }
    }
    
    private void OnUnequipPressed()
    {
        if (_selectedRune == null) return;
        
        // Find which slot this rune is in and unequip
        foreach (var kvp in _runeSystem.GetAllEquippedRunes())
        {
            if (kvp.Value == _selectedRune)
            {
                _runeSystem.UnequipRune(kvp.Key);
                RefreshEquippedRunes();
                RefreshStats();
                RefreshRuneList();
                _detailsPanel.Visible = false;
                break;
            }
        }
    }
    
    private void OnEquipPressed()
    {
        if (_selectedRune == null) return;
        
        // Try to equip to appropriate slot
        RuneSlotType targetSlot = _selectedRune.SlotType;
        
        // If Any slot, ask user which slot to equip to
        if (targetSlot == RuneSlotType.Any)
        {
            // For now, just try to find an empty slot
            foreach (RuneSlotType slot in Enum.GetValues(typeof(RuneSlotType)))
            {
                if (slot == RuneSlotType.Any) continue;
                if (_runeSystem.GetEquippedRune(slot) == null)
                {
                    targetSlot = slot;
                    break;
                }
            }
        }
        
        if (_runeSystem.EquipRune(_selectedRune, targetSlot))
        {
            RefreshEquippedRunes();
            RefreshStats();
            RefreshRuneList();
        }
    }
    
    public override void _GuiInput(InputEvent @event)
    {
        if (@event is InputEventKey keyEvent && keyEvent.Pressed)
        {
            if (keyEvent.Keycode == Key.Escape)
            {
                QueueFree();
            }
        }
    }
}
