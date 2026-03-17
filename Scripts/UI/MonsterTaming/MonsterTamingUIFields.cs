using Godot;
using Godot.Collections;
using System;

public partial class MonsterTamingUI : Control
{
    private static MonsterTamingUI Instance { get; set; }
    
    // UI Elements
    private PanelContainer _mainPanel;
    private VBoxContainer _contentBox;
    private TabContainer _tabContainer;
    
    // Wild Monsters Tab
    private ScrollContainer _wildScroll;
    private GridContainer _wildGrid;
    private Button _refreshButton;
    private Label _wildCountLabel;
    
    // Tamed Monsters Tab
    private ScrollContainer _tamedScroll;
    private GridContainer _tamedGrid;
    private Label _tamedCountLabel;
    
    // Stats Tab
    private VBoxContainer _statsBox;
    private Label _totalAttemptsLabel;
    private Label _successRateLabel;
    private Label _legendaryLabel;
    
    // Info Panel
    private PanelContainer _infoPanel;
    private Label _monsterNameLabel;
    private Label _monsterStatsLabel;
    private Label _monsterRarityLabel;
    private Label _monsterProgressLabel;
    private Button _feedButton;
    private Button _battleButton;
    private Button _playButton;
    private Button _captureButton;
    private Label _methodLabel;
    
    private TameableMonster _selectedMonster;
    private bool _isVisible = false;
}
