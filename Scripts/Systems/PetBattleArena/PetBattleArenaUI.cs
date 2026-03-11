using Godot;
using System;
using System.Collections.Generic;

public class PetBattleArenaUI : Control
{
    private PetBattleArenaSystem _battleSystem;
    private PetBattleArenaData[] _arenas;
    private int _selectedArenaIndex = 0;
    
    // UI Elements
    private PanelContainer _mainPanel;
    private VBoxContainer _mainVBox;
    private HBoxContainer _header;
    private Label _titleLabel;
    private Button _closeButton;
    
    private TabContainer _tabContainer;
    
    // Arena List Tab
    private Control _arenaListTab;
    private ScrollContainer _arenaScroll;
    private VBoxContainer _arenaListContainer;
    
    // Battle Tab
    private Control _battleTab;
    private Label _arenaNameLabel;
    private Label _waveLabel;
    private ProgressBar _healthBar;
    private Label _healthLabel;
    private Label _timerLabel;
    private Label _statsLabel;
    private Button _surrenderButton;
    private Button _useSkillButton;
    
    // Stats Tab
    private Control _statsTab;
    private Label _totalBattlesLabel;
    private Label _victoriesLabel;
    private Label _defeatsLabel;
    private Label _bestWaveLabel;
    private Label _totalDamageDealtLabel;
    private Label _totalDamageTakenLabel;
    
    private bool _isVisible = false;
    
    public override void _Ready()
    {
        _battleSystem = PetBattleArenaSystem.Instance;
        if (_battleSystem != null)
        {
            _battleSystem.Connect(nameof(PetBattleArenaSystem.BattleStarted), this, nameof(OnBattleStarted));
            _battleSystem.Connect(nameof(PetBattleArenaSystem.BattleEnded), this, nameof(OnBattleEnded));
            _battleSystem.Connect(nameof(PetBattleArenaSystem.WaveStarted), this, nameof(OnWaveStarted));
            _battleSystem.Connect(nameof(PetBattleArenaSystem.PetDamaged), this, nameof(OnPetDamaged));
            _battleSystem.Connect(nameof(PetBattleArenaSystem.BattleStatsUpdated), this, nameof(OnBattleStatsUpdated));
        }
        
        SetupUI();
        Hide();
    }
    
    private void SetupUI()
    {
        // Main Panel
        _mainPanel = new PanelContainer();
        _mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        _mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(_mainPanel);
        
        _mainVBox = new VBoxContainer();
        _mainPanel.AddChild(_mainVBox);
        
        // Header
        _header = new HBoxContainer();
        _mainVBox.AddChild(_header);
        
        _titleLabel = new Label();
        _titleLabel.Text = "宠物战斗竞技场";
        _titleLabel.Align = Label.AlignEnum.Center;
        _titleLabel.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _header.AddChild(_titleLabel);
        
        _closeButton = new Button();
        _closeButton.Text = "X";
        _closeButton.RectMinSize = new Vector2(40, 40);
        _closeButton.Connect("pressed", this, nameof(OnClosePressed));
        _header.AddChild(_closeButton);
        
        // Tab Container
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        _mainVBox.AddChild(_tabContainer);
        
        SetupArenaListTab();
        SetupBattleTab();
        SetupStatsTab();
    }
    
    private void SetupArenaListTab()
    {
        _arenaListTab = new Control();
        _arenaListTab.Name = "竞技场";
        _tabContainer.AddChild(_arenaListTab);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 10);
        _arenaListTab.AddChild(vbox);
        
        var descLabel = new Label();
        descLabel.Text = "选择竞技场开始宠物战斗";
        descLabel.Align = Label.AlignEnum.Center;
        vbox.AddChild(descLabel);
        
        _arenaScroll = new ScrollContainer();
        _arenaScroll.SizeFlagsVertical = Control.SizeFlags.ExpandFill;
        vbox.AddChild(_arenaScroll);
        
        _arenaListContainer = new VBoxContainer();
        _arenaListContainer.AddThemeConstantOverride("separation", 5);
        _arenaScroll.AddChild(_arenaListContainer);
        
        var startButton = new Button();
        startButton.Text = "开始战斗";
        startButton.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        startButton.Connect("pressed", this, nameof(OnStartBattlePressed));
        vbox.AddChild(startButton);
    }
    
    private void SetupBattleTab()
    {
        _battleTab = new Control();
        _battleTab.Name = "战斗";
        _tabContainer.AddChild(_battleTab);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 15);
        _battleTab.AddChild(vbox);
        
        _arenaNameLabel = new Label();
        _arenaNameLabel.Text = "训练场";
        _arenaNameLabel.Align = Label.AlignEnum.Center;
        _arenaNameLabel.AddThemeFontSizeOverride("font_size", 24);
        vbox.AddChild(_arenaNameLabel);
        
        _waveLabel = new Label();
        _waveLabel.Text = "波次: 1/5";
        _waveLabel.Align = Label.AlignEnum.Center;
        vbox.AddChild(_waveLabel);
        
        // Health Bar
        var healthContainer = new HBoxContainer();
        vbox.AddChild(healthContainer);
        
        var healthLabel = new Label();
        healthLabel.Text = "生命值: ";
        healthContainer.AddChild(healthLabel);
        
        _healthBar = new ProgressBar();
        _healthBar.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        _healthBar.MinValue = 0;
        _healthBar.MaxValue = 100;
        _healthBar.Value = 100;
        healthContainer.AddChild(_healthBar);
        
        _healthLabel = new Label();
        _healthLabel.Text = "100/100";
        healthContainer.AddChild(_healthLabel);
        
        _timerLabel = new Label();
        _timerLabel.Text = "战斗时间: 00:00";
        _timerLabel.Align = Label.AlignEnum.Center;
        vbox.AddChild(_timerLabel);
        
        _statsLabel = new Label();
        _statsLabel.Text = "伤害: 0 | 受伤: 0 | 击杀: 0";
        _statsLabel.Align = Label.AlignEnum.Center;
        vbox.AddChild(_statsLabel);
        
        // Buttons
        var buttonContainer = new HBoxContainer();
        buttonContainer.Alignment = BoxContainer.AlignmentMode.Center;
        buttonContainer.AddThemeConstantOverride("separation", 20);
        vbox.AddChild(buttonContainer);
        
        _useSkillButton = new Button();
        _useSkillButton.Text = "使用技能";
        _useSkillButton.RectMinSize = new Vector2(120, 40);
        _useSkillButton.Connect("pressed", this, nameof(OnUseSkillPressed));
        buttonContainer.AddChild(_useSkillButton);
        
        _surrenderButton = new Button();
        _surrenderButton.Text = "投降";
        _surrenderButton.RectMinSize = new Vector2(120, 40);
        _surrenderButton.Connect("pressed", this, nameof(OnSurrenderPressed));
        buttonContainer.AddChild(_surrenderButton);
    }
    
    private void SetupStatsTab()
    {
        _statsTab = new Control();
        _statsTab.Name = "统计";
        _tabContainer.AddChild(_statsTab);
        
        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 10);
        vbox.AddThemeConstantOverride("margin_left", 20);
        vbox.AddThemeConstantOverride("margin_right", 20);
        _statsTab.AddChild(vbox);
        
        var title = new Label();
        title.Text = "战斗统计";
        title.Align = Label.AlignEnum.Center;
        title.AddThemeFontSizeOverride("font_size", 20);
        vbox.AddChild(title);
        
        _totalBattlesLabel = CreateStatLabel("总战斗次数: 0");
        vbox.AddChild(_totalBattlesLabel);
        
        _victoriesLabel = CreateStatLabel("胜利次数: 0");
        vbox.AddChild(_victoriesLabel);
        
        _defeatsLabel = CreateStatLabel("失败次数: 0");
        vbox.AddChild(_defeatsLabel);
        
        _bestWaveLabel = CreateStatLabel("最高波次: 0");
        vbox.AddChild(_bestWaveLabel);
        
        _totalDamageDealtLabel = CreateStatLabel("总伤害: 0");
        vbox.AddChild(_totalDamageDealtLabel);
        
        _totalDamageTakenLabel = CreateStatLabel("总受伤: 0");
        vbox.AddChild(_totalDamageTakenLabel);
    }
    
    private Label CreateStatLabel(string text)
    {
        var label = new Label();
        label.Text = text;
        return label;
    }
    
    public void Toggle()
    {
        if (_isVisible)
        {
            Hide();
        }
        else
        {
            Show();
            RefreshArenaList();
            RefreshStats();
        }
        _isVisible = !_isVisible;
    }
    
    private void RefreshArenaList()
    {
        // Clear existing items
        foreach (Node child in _arenaListContainer.GetChildren())
        {
            child.QueueFree();
        }
        
        Player player = GetNode<Player>("/root/Main/Player");
        int playerLevel = player != null ? player.Level : 1;
        
        _arenas = _battleSystem.GetUnlockedArenas(playerLevel);
        
        for (int i = 0; i < _arenas.Length; i++)
        {
            var arena = _arenas[i];
            var arenaButton = CreateArenaButton(arena, i);
            _arenaListContainer.AddChild(arenaButton);
        }
    }
    
    private Control CreateArenaButton(PetBattleArenaData arena, int index)
    {
        var container = new PanelContainer();
        container.SetMeta("arena_index", index);
        
        var hbox = new HBoxContainer();
        container.AddChild(hbox);
        
        // Arena info
        var infoVBox = new VBoxContainer();
        infoVBox.SizeFlagsHorizontal = Control.SizeFlags.ExpandFill;
        hbox.AddChild(infoVBox);
        
        var nameLabel = new Label();
        nameLabel.Text = arena.ArenaName;
        nameLabel.AddThemeFontSizeOverride("font_size", 16);
        infoVBox.AddChild(nameLabel);
        
        var descLabel = new Label();
        descLabel.Text = $"{arena.Description} | 推荐等级: {arena.RecommendedLevel} | 波次: {arena.TotalWaves}";
        descLabel.AddThemeFontSizeOverride("font_size", 12);
        infoVBox.AddChild(descLabel);
        
        var rewardLabel = new Label();
        rewardLabel.Text = $"奖励: {arena.RewardGold}金币 {arena.RewardExp}经验";
        rewardLabel.AddThemeFontSizeOverride("font_size", 11);
        infoVBox.AddChild(rewardLabel);
        
        // Best wave
        int bestWave = _battleSystem.GetBestWave(arena.ArenaId);
        bool completed = _battleSystem.IsArenaCompleted(arena.ArenaId);
        
        var statusLabel = new Label();
        statusLabel.Text = completed ? "✓ 已完成" : (bestWave > 0 ? $"最佳: {bestWave}" : "未挑战");
        statusLabel.AddThemeColorOverride("font_color", completed ? Colors.Green : Colors.Yellow);
        hbox.AddChild(statusLabel);
        
        // Select on click
        var selectButton = new Button();
        selectButton.Text = index == _selectedArenaIndex ? "已选择" : "选择";
        selectButton.Connect("pressed", this, nameof(OnArenaSelected), new Godot.Collections.Array { index });
        hbox.AddChild(selectButton);
        
        return container;
    }
    
    private void RefreshStats()
    {
        var data = _battleSystem.PlayerData;
        
        _totalBattlesLabel.Text = $"总战斗次数: {data.TotalBattles}";
        _victoriesLabel.Text = $"胜利次数: {data.Victories}";
        _defeatsLabel.Text = $"失败次数: {data.Defeats}";
        _bestWaveLabel.Text = $"最高波次: {data.BestWave}";
        _totalDamageDealtLabel.Text = $"总伤害: {data.TotalDamageDealt}";
        _totalDamageTakenLabel.Text = $"总受伤: {data.TotalDamageTaken}";
    }
    
    private void OnArenaSelected(int index)
    {
        _selectedArenaIndex = index;
        RefreshArenaList();
    }
    
    private void OnStartBattlePressed()
    {
        if (_arenas == null || _selectedArenaIndex >= _arenas.Length)
            return;
        
        var arena = _arenas[_selectedArenaIndex];
        
        // Create a basic pet instance for battle
        Player player = GetNode<Player>("/root/Main/Player");
        if (player == null) return;
        
        // Get first available pet or create default
        var pet = CreateDefaultPetBattleInstance(player);
        
        if (_battleSystem.StartBattle(arena.ArenaId, pet))
        {
            _tabContainer.CurrentTab = 1; // Switch to battle tab
        }
    }
    
    private PetBattleInstance CreateDefaultPetBattleInstance(Player player)
    {
        // Get pet data from pet system if available
        var petSystem = PetSystem.Instance;
        
        var pet = new PetBattleInstance
        {
            PetId = "player_pet",
            MaxHealth = 100 + player.Level * 10,
            CurrentHealth = 100 + player.Level * 10,
            Attack = 10 + player.Level * 2,
            Defense = 5 + player.Level,
            Speed = 10 + player.Level,
            Level = player.Level,
            Experience = 0,
            EquippedSkills = new string[] { "basic_attack", "heal" }
        };
        
        return pet;
    }
    
    private void OnBattleStarted(string arenaId)
    {
        var arena = PetBattleArenaDatabase.GetArena(arenaId);
        if (arena != null)
        {
            _arenaNameLabel.Text = arena.ArenaName;
            UpdateHealthBar();
        }
    }
    
    private void OnBattleEnded(bool victory, int wavesCleared, int damageDealt)
    {
        string result = victory ? "胜利!" : "失败...";
        string message = victory 
            ? $"恭喜!你完成了{wavesCleared}波战斗,造成了{damageDealt}点伤害!"
            : $"战斗失败...你完成了{wavesCleared}波战斗,造成了{damageDealt}点伤害。";
        
        GD.Print(message);
        
        // Return to arena list after a delay
        GetTree().CreateTimer(3.0f).Connect("timeout", this, nameof(OnReturnToList));
    }
    
    private void OnReturnToList()
    {
        _tabContainer.CurrentTab = 0;
        RefreshArenaList();
        RefreshStats();
    }
    
    private void OnWaveStarted(int waveNumber, int totalWaves)
    {
        _waveLabel.Text = $"波次: {waveNumber}/{totalWaves}";
    }
    
    private void OnPetDamaged(int currentHealth, int maxHealth)
    {
        UpdateHealthBar();
    }
    
    private void OnBattleStatsUpdated(int damageDealt, int damageTaken, int enemiesDefeated)
    {
        int minutes = (int)(_battleSystem.BattleTimer / 60);
        int seconds = (int)(_battleSystem.BattleTimer % 60);
        _timerLabel.Text = $"战斗时间: {minutes:D2}:{seconds:D2}";
        
        _statsLabel.Text = $"伤害: {damageDealt} | 受伤: {damageTaken} | 击杀: {enemiesDefeated}";
    }
    
    private void UpdateHealthBar()
    {
        _healthBar.MaxValue = _battleSystem.PlayerMaxHealth;
        _healthBar.Value = _battleSystem.PlayerCurrentHealth;
        _healthLabel.Text = $"{_battleSystem.PlayerCurrentHealth}/{_battleSystem.PlayerMaxHealth}";
    }
    
    private void OnUseSkillPressed()
    {
        _battleSystem.UseSkill("basic_skill");
    }
    
    private void OnSurrenderPressed()
    {
        _battleSystem.Surrender();
    }
    
    private void OnClosePressed()
    {
        if (_battleSystem.CurrentState == PetBattleArenaSystem.BattleState.BattleActive)
        {
            _battleSystem.Surrender();
        }
        Toggle();
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel") && _isVisible)
        {
            Toggle();
        }
    }
}
