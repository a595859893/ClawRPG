using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// Rune UI - Interface for rune management
/// </summary>
public partial class RuneUI : Control
{
    private RuneSystem _runeSystem;
    private VBoxContainer _mainContainer;
    
    // Rune list
    private GridContainer _runeGrid;
    private OptionButton _filterTypeButton;
    private Label _runeCountLabel;
    
    // Equipped runes panel
    private VBoxContainer _equippedContainer;
    private Dictionary<RuneSlotType, Button> _equippedSlots = new Dictionary<RuneSlotType, Button>();
    
    // Details panel
    private Panel _detailsPanel;
    private Label _detailsName;
    private Label _detailsDescription;
    private Label _detailsType;
    private Label _detailsSlot;
    private Label _detailsLevel;
    private VBoxContainer _bonusesContainer;
    private Label _specialEffectLabel;
    
    // Stats panel
    private VBoxContainer _statsContainer;
    private Label _totalAttackLabel;
    private Label _totalDefenseLabel;
    private Label _totalHealthLabel;
    private Label _totalSpeedLabel;
    private Label _totalCritRateLabel;
    private Label _totalCritDamageLabel;
    private Label _totalLifestealLabel;
    private Label _totalDodgeLabel;
    private Label _totalBlockLabel;
    
    private RuneData _selectedRune;
    
    public override void _Ready()
    {
        _runeSystem = RuneSystem.Instance;
        
        SetupUI();
        ConnectSignals();
        RefreshRuneList();
        RefreshEquippedRunes();
        RefreshStats();
    }
}
