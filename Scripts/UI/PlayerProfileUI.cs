using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 玩家档案界面 - 显示玩家游戏统计和进度
/// </summary>
public class PlayerProfileUI : Control
{
    private Label _titleLabel;
    private TabContainer _tabContainer;
    
    // 总览标签页
    private VBoxContainer _overviewTab;
    private Label _playerNameLabel;
    private Label _levelLabel;
    private Label _playTimeLabel;
    private Label _playTimeValueLabel;
    private Label _firstPlayLabel;
    private Label _lastPlayLabel;
    
    // 战斗标签页
    private VBoxContainer _combatTab;
    private Label _killsLabel;
    private Label _bossKillsLabel;
    private Label _damageDealtLabel;
    private Label _damageTakenLabel;
    private Label _healingLabel;
    private Label _criticalHitsLabel;
    private Label _maxComboLabel;
    private Label _deathsLabel;
    private Label _kdaLabel;
    
    // 经济标签页
    private VBoxContainer _economyTab;
    private Label _goldEarnedLabel;
    private Label _goldSpentLabel;
    private Label _netGoldLabel;
    private Label _itemsCollectedLabel;
    private Label _itemsCraftedLabel;
    
    // 探索标签页
    private VBoxContainer _explorationTab;
    private Label _regionsLabel;
    private Label _dungeonsLabel;
    private Label _questsLabel;
    private Label _secretsLabel;
    
    // 社交标签页
    private VBoxContainer _socialTab;
    private Label _tradesLabel;
    private Label _pvpWinsLabel;
    private Label _pvpLossesLabel;
    private Label _pvpWinRateLabel;
    private Label _partiesLabel;
    
    // 成就标签页
    private VBoxContainer _achievementsTab;
    private Label _achievementsUnlockedLabel;
    private Label _achievementPointsLabel;
    
    // 会话统计
    private VBoxContainer _sessionTab;
    private Label _sessionTimeLabel;
    private Label _sessionKillsLabel;
    private Label _sessionDamageLabel;
    private Label _sessionGoldLabel;
    private Label _sessionDPSLabel;
    
    private bool _isVisible = false;
    private PlayerProfileSystem _profileSystem;
    
    public override void _Ready()
    {
        Visible = false;
        _profileSystem = PlayerProfileSystem.Instance;
        
        SetupUI();
        
        // 连接信号
        if (_profileSystem != null)
        {
            _profileSystem.OnProfileUpdated += UpdateDisplay;
        }
    }
    
    public override void _ExitTree()
    {
        if (_profileSystem != null)
        {
            _profileSystem.OnProfileUpdated -= UpdateDisplay;
        }
    }
    
    private void SetupUI()
    {
        // 背景面板
        var bgPanel = new Panel
        {
            AnchorRight = 1f,
            AnchorBottom = 1f,
            Color = new Color(0, 0, 0, 0.7f)
        };
        AddChild(bgPanel);
        
        // 标题
        _titleLabel = new Label
        {
            Text = "📊 玩家档案",
            AnchorLeft = 0.5f,
            AnchorRight = 0.5f,
            AnchorTop = 0.02f,
            AnchorBottom = 0.08f,
            Align = Label.AlignEnum.Center,
            FontSize = 28
        };
        AddChild(_titleLabel);
        
        // 标签容器
        _tabContainer = new TabContainer
        {
            AnchorLeft = 0.05f,
            AnchorRight = 0.95f,
            AnchorTop = 0.1f,
            AnchorBottom = 0.9f,
            TabAlign = TabContainer.TabAlignEnum.Left
        };
        AddChild(_tabContainer);
        
        // 创建各标签页
        CreateOverviewTab();
        CreateCombatTab();
        CreateEconomyTab();
        CreateExplorationTab();
        CreateSocialTab();
        CreateAchievementsTab();
        CreateSessionTab();
        
        // 关闭按钮
        var closeBtn = new Button
        {
            Text = "✕ 关闭",
            AnchorLeft = 0.4f,
            AnchorRight = 0.6f,
            AnchorTop = 0.92f,
            AnchorBottom = 0.98f
        };
        closeBtn.Pressed += () => ToggleVisibility();
        AddChild(closeBtn);
    }
    
    private void CreateOverviewTab()
    {
        _overviewTab = new VBoxContainer();
        _overviewTab.Name = "总览";
        _tabContainer.AddChild(_overviewTab);
        
        var grid = new GridContainer { Columns = 2 };
        _overviewTab.AddChild(grid);
        
        _playerNameLabel = CreateStatRow(grid, "玩家名称:", "Player");
        _levelLabel = CreateStatRow(grid, "当前等级:", "1");
        _playTimeValueLabel = CreateStatRow(grid, "总游戏时间:", "0h 0m");
        _firstPlayLabel = CreateStatRow(grid, "首次游戏:", "-");
        _lastPlayLabel = CreateStatRow(grid, "最后游戏:", "-");
    }
    
    private void CreateCombatTab()
    {
        _combatTab = new VBoxContainer();
        _combatTab.Name = "战斗";
        _tabContainer.AddChild(_combatTab);
        
        var grid = new GridContainer { Columns = 2 };
        _combatTab.AddChild(grid);
        
        _killsLabel = CreateStatRow(grid, "总击杀:", "0");
        _bossKillsLabel = CreateStatRow(grid, "Boss击杀:", "0");
        _damageDealtLabel = CreateStatRow(grid, "总伤害:", "0");
        _damageTakenLabel = CreateStatRow(grid, "承受伤害:", "0");
        _healingLabel = CreateStatRow(grid, "总治疗:", "0");
        _criticalHitsLabel = CreateStatRow(grid, "暴击次数:", "0");
        _maxComboLabel = CreateStatRow(grid, "最高连击:", "0");
        _deathsLabel = CreateStatRow(grid, "死亡次数:", "0");
        _kdaLabel = CreateStatRow(grid, "KDA:", "0");
    }
    
    private void CreateEconomyTab()
    {
        _economyTab = new VBoxContainer();
        _economyTab.Name = "经济";
        _tabContainer.AddChild(_economyTab);
        
        var grid = new GridContainer { Columns = 2 };
        _economyTab.AddChild(grid);
        
        _goldEarnedLabel = CreateStatRow(grid, "总收入:", "0");
        _goldSpentLabel = CreateStatRow(grid, "总支出:", "0");
        _netGoldLabel = CreateStatRow(grid, "净收益:", "0");
        _itemsCollectedLabel = CreateStatRow(grid, "收集物品:", "0");
        _itemsCraftedLabel = CreateStatRow(grid, "制作物品:", "0");
    }
    
    private void CreateExplorationTab()
    {
        _explorationTab = new VBoxContainer();
        _explorationTab.Name = "探索";
        _tabContainer.AddChild(_explorationTab);
        
        var grid = new GridContainer { Columns = 2 };
        _explorationTab.AddChild(grid);
        
        _regionsLabel = CreateStatRow(grid, "发现区域:", "0");
        _dungeonsLabel = CreateStatRow(grid, "通关副本:", "0");
        _questsLabel = CreateStatRow(grid, "完成任务:", "0");
        _secretsLabel = CreateStatRow(grid, "发现秘密:", "0");
    }
    
    private void CreateSocialTab()
    {
        _socialTab = new VBoxContainer();
        _socialTab.Name = "社交";
        _tabContainer.AddChild(_socialTab);
        
        var grid = new GridContainer { Columns = 2 };
        _socialTab.AddChild(grid);
        
        _tradesLabel = CreateStatRow(grid, "交易次数:", "0");
        _pvpWinsLabel = CreateStatRow(grid, "PVP胜利:", "0");
        _pvpLossesLabel = CreateStatRow(grid, "PVP失败:", "0");
        _pvpWinRateLabel = CreateStatRow(grid, "PVP胜率:", "0%");
        _partiesLabel = CreateStatRow(grid, "组队次数:", "0");
    }
    
    private void CreateAchievementsTab()
    {
        _achievementsTab = new VBoxContainer();
        _achievementsTab.Name = "成就";
        _tabContainer.AddChild(_achievementsTab);
        
        var grid = new GridContainer { Columns = 2 };
        _achievementsTab.AddChild(grid);
        
        _achievementsUnlockedLabel = CreateStatRow(grid, "解锁成就:", "0");
        _achievementPointsLabel = CreateStatRow(grid, "成就点数:", "0");
    }
    
    private void CreateSessionTab()
    {
        _sessionTab = new VBoxContainer();
        _sessionTab.Name = "本次会话";
        _tabContainer.AddChild(_sessionTab);
        
        var grid = new GridContainer { Columns = 2 };
        _sessionTab.AddChild(grid);
        
        _sessionTimeLabel = CreateStatRow(grid, "游戏时间:", "0m");
        _sessionKillsLabel = CreateStatRow(grid, "击杀:", "0");
        _sessionDamageLabel = CreateStatRow(grid, "伤害:", "0");
        _sessionGoldLabel = CreateStatRow(grid, "获得金币:", "0");
        _sessionDPSLabel = CreateStatRow(grid, "DPS:", "0");
    }
    
    private Label CreateStatRow(GridContainer grid, string label, string value)
    {
        var labelControl = new Label
        {
            Text = label,
            HorizontalAlignment = Label.HAlignEnum.Left
        };
        grid.AddChild(labelControl);
        
        var valueLabel = new Label
        {
            Text = value,
            HorizontalAlignment = Label.HAlignEnum.Right,
            Modulate = new Color(1, 0.9, 0.5)
        };
        grid.AddChild(valueLabel);
        
        return valueLabel;
    }
    
    public void ToggleVisibility()
    {
        _isVisible = !_isVisible;
        Visible = _isVisible;
        
        if (_isVisible)
        {
            UpdateDisplay();
        }
    }
    
    private void UpdateDisplay()
    {
        if (_profileSystem == null) return;
        
        var profile = _profileSystem.Profile;
        
        // 总览
        _playerNameLabel.Text = profile.PlayerName;
        _levelLabel.Text = profile.CurrentLevel.ToString();
        _playTimeValueLabel.Text = _profileSystem.GetPlayTimeFormatted();
        _firstPlayLabel.Text = profile.FirstPlayDate.ToString("yyyy-MM-dd");
        _lastPlayLabel.Text = profile.LastPlayDate.ToString("yyyy-MM-dd HH:mm");
        
        // 战斗
        _killsLabel.Text = profile.TotalKills.ToString("N0");
        _bossKillsLabel.Text = profile.BossKills.ToString("N0");
        _damageDealtLabel.Text = profile.TotalDamageDealt.ToString("N0");
        _damageTakenLabel.Text = profile.TotalDamageTaken.ToString("N0");
        _healingLabel.Text = profile.TotalHealingDone.ToString("N0");
        _criticalHitsLabel.Text = profile.CriticalHits.ToString("N0");
        _maxComboLabel.Text = profile.MaxCombo.ToString("N0");
        _deathsLabel.Text = profile.Deaths.ToString("N0");
        _kdaLabel.Text = _profileSystem.GetKDA().ToString("F2");
        
        // 经济
        _goldEarnedLabel.Text = profile.TotalGoldEarned.ToString("N0");
        _goldSpentLabel.Text = profile.TotalGoldSpent.ToString("N0");
        _netGoldLabel.Text = (profile.TotalGoldEarned - profile.TotalGoldSpent).ToString("N0");
        _itemsCollectedLabel.Text = profile.ItemsCollected.ToString("N0");
        _itemsCraftedLabel.Text = profile.ItemsCrafted.ToString("N0");
        
        // 探索
        _regionsLabel.Text = profile.RegionsDiscovered.ToString();
        _dungeonsLabel.Text = profile.DungeonsCompleted.ToString();
        _questsLabel.Text = profile.QuestsCompleted.ToString();
        _secretsLabel.Text = profile.SecretsFound.ToString();
        
        // 社交
        _tradesLabel.Text = profile.TradesCompleted.ToString();
        _pvpWinsLabel.Text = profile.PvPWins.ToString();
        _pvpLossesLabel.Text = profile.PvPLosses.ToString();
        _pvpWinRateLabel.Text = _profileSystem.GetWinRate().ToString("F1") + "%";
        _partiesLabel.Text = profile.PartiesJoined.ToString();
        
        // 成就
        _achievementsUnlockedLabel.Text = profile.AchievementsUnlocked.ToString();
        _achievementPointsLabel.Text = profile.TotalAchievementPoints.ToString();
        
        // 会话统计
        int sessionTime = _profileSystem.GetSessionPlayTime();
        _sessionTimeLabel.Text = $"{sessionTime / 60}m {sessionTime % 60}s";
        _sessionKillsLabel.Text = _profileSystem.GetSessionKills().ToString("N0");
        _sessionDamageLabel.Text = _profileSystem.GetSessionDamageDealt().ToString("N0");
        _sessionGoldLabel.Text = _profileSystem.GetSessionGoldEarned().ToString("N0");
        _sessionDPSLabel.Text = _profileSystem.GetDPS();
    }
    
    public override void _Input(InputEvent event_)
    {
        if (event_ is InputEventKey keyEvent && keyEvent.Pressed)
        {
            // P键切换显示
            if (keyEvent.Keycode == Key.P)
            {
                var action = keyEvent.Strength;
                if (action > 0.5f)
                {
                    ToggleVisibility();
                }
            }
        }
    }
}
