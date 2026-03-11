using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// 坐骑战斗竞技场UI
/// </summary>
public class MountBattleArenaUI : Control
{
    private TabContainer _tabContainer;
    private VBoxContainer _arenaListTab;
    private VBoxContainer _battleTab;
    private VBoxContainer _statisticsTab;
    
    private OptionButton _mountSelect;
    private OptionButton _arenaTypeFilter;
    private OptionButton _difficultyFilter;
    private ItemList _arenaList;
    
    private Label _arenaInfoLabel;
    private Label _waveLabel;
    private ProgressBar _waveProgressBar;
    private Button _startBattleButton;
    private Button _cancelBattleButton;
    
    private Label _statsTotalBattles;
    private Label _statsVictories;
    private Label _statsDefeats;
    private Label _statsWinRate;
    private Label _statsTotalGold;
    private Label _statsTotalExp;
    private Label _statsWavesCleared;
    
    private Color _victoryColor = new Color(0, 1, 0);
    private Color _defeatColor = new Color(1, 0, 0);
    
    public override void _Ready()
    {
        // Create main panel
        var mainPanel = new PanelContainer();
        mainPanel.SetAnchorsPreset(Control.LayoutPreset.Center);
        mainPanel.CustomMinimumSize = new Vector2(800, 600);
        AddChild(mainPanel);
        
        var mainVBox = new VBoxContainer();
        mainPanel.AddChild(mainVBox);
        
        // Title
        var titleLabel = new Label();
        titleLabel.Text = "坐骑战斗竞技场";
        titleLabel.HorizontalAlignment = HorizontalAlignment.Center;
        titleLabel.AddThemeFontSizeOverride("font_size", 24);
        mainVBox.AddChild(titleLabel);
        
        // Close button
        var closeButton = new Button();
        closeButton.Text = "关闭";
        closeButton.Align = Button.AlignMode.Center;
        closeButton.Pressed += () => Hide();
        mainVBox.AddChild(closeButton);
        
        // Tab container
        _tabContainer = new TabContainer();
        _tabContainer.SetVExpand(ExpandMode.Expand);
        mainVBox.AddChild(_tabContainer);
        
        // Arena List Tab
        _arenaListTab = new VBoxContainer();
        _arenaListTab.Name = "竞技场";
        _tabContainer.AddChild(_arenaListTab);
        CreateArenaListTab();
        
        // Battle Tab
        _battleTab = new VBoxContainer();
        _battleTab.Name = "战斗";
        _tabContainer.AddChild(_battleTab);
        CreateBattleTab();
        
        // Statistics Tab
        _statisticsTab = new VBoxContainer();
        _statisticsTab.Name = "统计";
        _tabContainer.AddChild(_statisticsTab);
        CreateStatisticsTab();
        
        // Connect signals
        MountBattleArenaSystem.Instance.OnBattleStarted += OnBattleStarted;
        MountBattleArenaSystem.Instance.OnBattleEnded += OnBattleEnded;
        MountBattleArenaSystem.Instance.OnWaveStarted += OnWaveStarted;
        MountBattleArenaSystem.Instance.OnWaveCompleted += OnWaveCompleted;
        MountBattleArenaSystem.Instance.OnBattleVictory += OnBattleVictory;
        MountBattleArenaSystem.Instance.OnBattleDefeat += OnBattleDefeat;
        
        // Initialize
        RefreshArenaList();
        RefreshMountSelect();
        RefreshStatistics();
    }
    
    private void CreateArenaListTab()
    {
        // Mount selection
        var mountLabel = new Label();
        mountLabel.Text = "选择坐骑:";
        _arenaListTab.AddChild(mountLabel);
        
        _mountSelect = new OptionButton();
        _mountSelect.Selected += OnMountSelected;
        _arenaListTab.AddChild(_mountSelect);
        
        // Filters
        var filterBox = new HBoxContainer();
        _arenaListTab.AddChild(filterBox);
        
        var typeLabel = new Label();
        typeLabel.Text = "类型:";
        filterBox.AddChild(typeLabel);
        
        _arenaTypeFilter = new OptionButton();
        _arenaTypeFilter.AddItem("全部", 0);
        _arenaTypeFilter.AddItem("训练场", (int)MountBattleArenaData.ArenaType.TrainingGround);
        _arenaTypeFilter.AddItem("战斗竞技场", (int)MountBattleArenaData.ArenaType.BattleColosseum);
        _arenaTypeFilter.AddItem("龙之战场", (int)MountBattleArenaData.ArenaType.DragonArena);
        _arenaTypeFilter.AddItem("凤凰巢穴", (int)MountBattleArenaData.ArenaType.PhoenixNest);
        _arenaTypeFilter.AddItem("暗影领域", (int)MountBattleArenaData.ArenaType.ShadowRealm);
        _arenaTypeFilter.AddItem("神圣之地", (int)MountBattleArenaData.ArenaType.SacredGround);
        _arenaTypeFilter.Selected += OnFilterChanged;
        filterBox.AddChild(_arenaTypeFilter);
        
        var diffLabel = new Label();
        diffLabel.Text = "  难度:";
        filterBox.AddChild(diffLabel);
        
        _difficultyFilter = new OptionButton();
        _difficultyFilter.AddItem("全部", 0);
        _difficultyFilter.AddItem("简单", (int)MountBattleArenaData.ArenaDifficulty.Easy);
        _difficultyFilter.AddItem("普通", (int)MountBattleArenaData.ArenaDifficulty.Normal);
        _difficultyFilter.AddItem("困难", (int)MountBattleArenaData.ArenaDifficulty.Hard);
        _difficultyFilter.AddItem("史诗", (int)MountBattleArenaData.ArenaDifficulty.Epic);
        _difficultyFilter.AddItem("传奇", (int)MountBattleArenaData.ArenaDifficulty.Legendary);
        _difficultyFilter.Selected += OnFilterChanged;
        filterBox.AddChild(_difficultyFilter);
        
        // Arena list
        var listLabel = new Label();
        listLabel.Text = "竞技场列表:";
        _arenaListTab.AddChild(listLabel);
        
        _arenaList = new ItemList();
        _arenaList.SetVExpand(ExpandMode.Expand);
        _arenaList.ItemSelected += OnArenaSelected;
        _arenaListTab.AddChild(_arenaList);
        
        // Arena info
        _arenaInfoLabel = new Label();
        _arenaInfoLabel.Text = "选择一个竞技场查看详情";
        _arenaListTab.AddChild(_arenaInfoLabel);
        
        // Start button
        _startBattleButton = new Button();
        _startBattleButton.Text = "开始战斗";
        _startBattleButton.Disabled = true;
        _startBattleButton.Pressed += OnStartBattlePressed;
        _arenaListTab.AddChild(_startBattleButton);
    }
    
    private void CreateBattleTab()
    {
        var battleInfoLabel = new Label();
        battleInfoLabel.Text = "战斗信息";
        battleInfoLabel.HorizontalAlignment = HorizontalAlignment.Center;
        battleInfoLabel.AddThemeFontSizeOverride("font_size", 20);
        _battleTab.AddChild(battleInfoLabel);
        
        _waveLabel = new Label();
        _waveLabel.Text = "波次: -/-";
        _waveLabel.HorizontalAlignment = HorizontalAlignment.Center;
        _battleTab.AddChild(_waveLabel);
        
        _waveProgressBar = new ProgressBar();
        _waveProgressBar.SetVExpand(ExpandMode.Expand);
        _battleTab.AddChild(_waveProgressBar);
        
        _cancelBattleButton = new Button();
        _cancelBattleButton.Text = "退出战斗";
        _cancelBattleButton.Disabled = true;
        _cancelBattleButton.Pressed += OnCancelBattlePressed;
        _battleTab.AddChild(_cancelBattleButton);
    }
    
    private void CreateStatisticsTab()
    {
        var statsGrid = new GridContainer();
        statsGrid.Columns = 2;
        statsGrid.SetVExpand(ExpandMode.Expand);
        _statisticsTab.AddChild(statsGrid);
        
        // Total Battles
        var totalBattlesLabel = new Label();
        totalBattlesLabel.Text = "总战斗次数:";
        statsGrid.AddChild(totalBattlesLabel);
        
        _statsTotalBattles = new Label();
        _statsTotalBattles.Text = "0";
        statsGrid.AddChild(_statsTotalBattles);
        
        // Victories
        var victoriesLabel = new Label();
        victoriesLabel.Text = "胜利次数:";
        statsGrid.AddChild(victoriesLabel);
        
        _statsVictories = new Label();
        _statsVictories.Text = "0";
        statsGrid.AddChild(_statsVictories);
        
        // Defeats
        var defeatsLabel = new Label();
        defeatsLabel.Text = "失败次数:";
        statsGrid.AddChild(defeatsLabel);
        
        _statsDefeats = new Label();
        _statsDefeats.Text = "0";
        statsGrid.AddChild(_statsDefeats);
        
        // Win Rate
        var winRateLabel = new Label();
        winRateLabel.Text = "胜率:";
        statsGrid.AddChild(winRateLabel);
        
        _statsWinRate = new Label();
        _statsWinRate.Text = "0%";
        statsGrid.AddChild(_statsWinRate);
        
        // Total Gold
        var totalGoldLabel = new Label();
        totalGoldLabel.Text = "总获得金币:";
        statsGrid.AddChild(totalGoldLabel);
        
        _statsTotalGold = new Label();
        _statsTotalGold.Text = "0";
        statsGrid.AddChild(_statsTotalGold);
        
        // Total Exp
        var totalExpLabel = new Label();
        totalExpLabel.Text = "总获得经验:";
        statsGrid.AddChild(totalExpLabel);
        
        _statsTotalExp = new Label();
        _statsTotalExp.Text = "0";
        statsGrid.AddChild(_statsTotalExp);
        
        // Waves Cleared
        var wavesClearedLabel = new Label();
        wavesClearedLabel.Text = "总通过波次:";
        statsGrid.AddChild(wavesClearedLabel);
        
        _statsWavesCleared = new Label();
        _statsWavesCleared.Text = "0";
        statsGrid.AddChild(_statsWavesCleared);
    }
    
    private void RefreshMountSelect()
    {
        _mountSelect.Clear();
        
        var mountManager = MountManager.Instance;
        if (mountManager != null)
        {
            var mounts = mountManager.GetMounts();
            int index = 0;
            foreach (var mount in mounts)
            {
                _mountSelect.AddItem($"{mount.GetName()} (Lv.{mount.GetLevel()})", index);
                index++;
            }
        }
        
        if (_mountSelect.ItemCount == 0)
        {
            _mountSelect.AddItem("无可用坐骑", 0);
        }
    }
    
    private void RefreshArenaList()
    {
        _arenaList.Clear();
        
        var typeFilter = (MountBattleArenaData.ArenaType)(_arenaTypeFilter.GetSelectedId() - 1);
        var diffFilter = (MountBattleArenaData.ArenaDifficulty)(_difficultyFilter.GetSelectedId() - 1);
        
        var arenas = MountBattleArenaSystem.Instance.GetAllArenas();
        
        foreach (var arena in arenas)
        {
            bool addArena = true;
            
            // Type filter
            if (_arenaTypeFilter.GetSelectedId() > 0 && arena.Type != typeFilter)
                addArena = false;
            
            // Difficulty filter
            if (_difficultyFilter.GetSelectedId() > 0 && arena.Difficulty != diffFilter)
                addArena = false;
            
            if (addArena)
            {
                string displayName = $"{arena.Name} [{MountBattleArenaDatabase.GetDifficultyName(arena.Difficulty)}]";
                _arenaList.AddItem(displayName);
            }
        }
    }
    
    private void RefreshStatistics()
    {
        var stats = MountBattleArenaSystem.Instance.GetStatistics();
        
        _statsTotalBattles.Text = stats.TotalBattles.ToString();
        _statsVictories.Text = stats.Victories.ToString();
        _statsDefeats.Text = stats.Defeats.ToString();
        _statsWinRate.Text = $"{MountBattleArenaSystem.Instance.GetVictoryRate():F1}%";
        _statsTotalGold.Text = stats.TotalGoldEarned.ToString();
        _statsTotalExp.Text = stats.TotalExpEarned.ToString();
        _statsWavesCleared.Text = stats.TotalWavesCleared.ToString();
    }
    
    private void OnMountSelected(int index)
    {
        RefreshArenaList();
    }
    
    private void OnFilterChanged(int index)
    {
        RefreshArenaList();
    }
    
    private void OnArenaSelected(int index)
    {
        var arenas = MountBattleArenaSystem.Instance.GetAllArenas();
        
        var typeFilter = (MountBattleArenaData.ArenaType)(_arenaTypeFilter.GetSelectedId() - 1);
        var diffFilter = (MountBattleArenaData.ArenaDifficulty)(_difficultyFilter.GetSelectedId() - 1);
        
        int currentIndex = 0;
        MountBattleArenaData.MountArena selectedArena = null;
        
        foreach (var arena in arenas)
        {
            bool addArena = true;
            
            if (_arenaTypeFilter.GetSelectedId() > 0 && arena.Type != typeFilter)
                addArena = false;
            
            if (_difficultyFilter.GetSelectedId() > 0 && arena.Difficulty != diffFilter)
                addArena = false;
            
            if (addArena)
            {
                if (currentIndex == index)
                {
                    selectedArena = arena;
                    break;
                }
                currentIndex++;
            }
        }
        
        if (selectedArena != null)
        {
            _arenaInfoLabel.Text = $"{selectedArena.Name}\n" +
                $"难度: {MountBattleArenaDatabase.GetDifficultyName(selectedArena.Difficulty)}\n" +
                $"推荐等级: {selectedArena.RecommendedLevel}\n" +
                $"波次: {selectedArena.TotalWaves}\n" +
                $"入场费: {selectedArena.EntryFee}金币\n" +
                $"基础奖励: {selectedArena.BaseGoldReward}金币, {selectedArena.BaseExpReward}经验";
            
            _startBattleButton.Disabled = false;
        }
    }
    
    private void OnStartBattlePressed()
    {
        if (_mountSelect.ItemCount == 0 || _mountSelect.Selected < 0)
        {
            GD.PrintErr("[MountBattleArenaUI] No mount selected");
            return;
        }
        
        var mountManager = MountManager.Instance;
        if (mountManager == null) return;
        
        var mounts = mountManager.GetMounts();
        if (_mountSelect.Selected >= mounts.Count) return;
        
        var selectedMount = mounts[_mountSelect.Selected];
        
        var arenas = MountBattleArenaSystem.Instance.GetAllArenas();
        var typeFilter = (MountBattleArenaData.ArenaType)(_arenaTypeFilter.GetSelectedId() - 1);
        var diffFilter = (MountBattleArenaData.ArenaDifficulty)(_difficultyFilter.GetSelectedId() - 1);
        
        int currentIndex = 0;
        MountBattleArenaData.MountArena selectedArena = null;
        
        foreach (var arena in arenas)
        {
            bool addArena = true;
            
            if (_arenaTypeFilter.GetSelectedId() > 0 && arena.Type != typeFilter)
                addArena = false;
            
            if (_difficultyFilter.GetSelectedId() > 0 && arena.Difficulty != diffFilter)
                addArena = false;
            
            if (addArena)
            {
                if (currentIndex == _arenaList.GetSelectedItems()[0])
                {
                    selectedArena = arena;
                    break;
                }
                currentIndex++;
            }
        }
        
        if (selectedArena != null)
        {
            if (MountBattleArenaSystem.Instance.StartBattle(selectedMount.GetId(), selectedArena.Id))
            {
                _tabContainer.CurrentTab = 1; // Switch to battle tab
            }
        }
    }
    
    private void OnCancelBattlePressed()
    {
        MountBattleArenaSystem.Instance.CancelBattle();
    }
    
    private void OnBattleStarted(string arenaId)
    {
        _cancelBattleButton.Disabled = false;
    }
    
    private void OnBattleEnded(string arenaId)
    {
        _cancelBattleButton.Disabled = true;
        _waveLabel.Text = "战斗已结束";
        RefreshStatistics();
    }
    
    private void OnWaveStarted(int currentWave, int totalWaves)
    {
        _waveLabel.Text = $"波次: {currentWave}/{totalWaves}";
        _waveProgressBar.MaxValue = totalWaves;
        _waveProgressBar.Value = currentWave;
    }
    
    private void OnWaveCompleted(int waveNumber)
    {
        GD.Print($"[MountBattleArenaUI] Wave {waveNumber} completed!");
    }
    
    private void OnBattleVictory()
    {
        _waveLabel.Text = "胜利!";
        _waveLabel.Modulate = _victoryColor;
    }
    
    private void OnBattleDefeat()
    {
        _waveLabel.Text = "失败!";
        _waveLabel.Modulate = _defeatColor;
    }
    
    public override void _Input(InputEvent @event)
    {
        if (@event.IsActionPressed("ui_cancel"))
        {
            Hide();
        }
    }
}
