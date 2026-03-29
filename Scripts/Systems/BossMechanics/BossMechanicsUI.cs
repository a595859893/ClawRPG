using Godot;
using System;
using System.Collections.Generic;

public class BossMechanicsUI : Control
{
    private BossMechanicsSystem _bossSystem;
    
    // UI 组件
    private Label _titleLabel;
    private TabContainer _tabContainer;
    
    // Boss列表标签页
    private Control _bossListTab;
    private ItemList _bossList;
    private Label _bossInfoLabel;
    private Button _startBattleButton;
    
    // 战斗标签页
    private Control _battleTab;
    private ConfidenceFloorHealthBar _bossHealthBar;
    private Label _bossNameLabel;
    private Label _phaseLabel;
    private Label _enrageLabel;
    private VBoxContainer _skillList;
    private Label _combatStatsLabel;
    
    // 统计标签页
    private Control _statsTab;
    private Label _totalStatsLabel;
    private Label _bestRecordsLabel;
    private ItemList _historyList;
    
    // 当前状态
    private string _selectedBossId = "";
    private string _currentBattleId = "";
    private int _currentTab = 0;

    // REQ-156-05: 狂暴模式切换视觉反馈
    private Label _modeLabel;          // "☠️ ENRAGED" 脉冲标签
    private float _pulseTimer = 0f;
    private bool _isModeEnraged = false;
    private float _shakeOffsetX = 0f;
    private Label _attackTypeLabel;     // 当前攻击类型标签

    public override void _Ready()
    {
        _bossSystem = BossMechanicsSystem.Instance;
        
        SetupUI();
        ConnectSignals();
        RefreshBossList();
    }

    private void SetupUI()
    {
        // 主容器
        var mainVBox = new VBoxContainer();
        mainVBox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        mainVBox.AddThemeConstantOverride("separation", 10);
        AddChild(mainVBox);

        // 标题
        _titleLabel = new Label();
        _titleLabel.Text = "=== Boss 战斗系统 ===";
        _titleLabel.Align = Label.AlignEnum.Center;
        mainVBox.AddChild(_titleLabel);

        // 标签页容器
        _tabContainer = new TabContainer();
        _tabContainer.SizeFlagsVertical = Control.SizeFlags.ExpandAndFill;
        mainVBox.AddChild(_tabContainer);

        // 创建标签页
        SetupBossListTab();
        SetupBattleTab();
        SetupStatsTab();

        // 底部说明
        var hintLabel = new Label();
        hintLabel.Text = "[↑/↓] 选择 | [1-3] 切换标签页 | [Enter] 开始战斗 | [ESC] 关闭";
        hintLabel.Align = Label.AlignEnum.Center;
        mainVBox.AddChild(hintLabel);
    }

    private void SetupBossListTab()
    {
        _bossListTab = new Control();
        _bossListTab.Name = "Boss列表";
        _tabContainer.AddChild(_bossListTab);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 10);
        _bossListTab.AddChild(vbox);

        // Boss列表
        var listLabel = new Label();
        listLabel.Text = "可挑战的Boss:";
        vbox.AddChild(listLabel);

        _bossList = new ItemList();
        _bossList.SizeFlagsVertical = Control.SizeFlags.ExpandAndFill;
        _bossList.ItemSelected += OnBossListItemSelected;
        vbox.AddChild(_bossList);

        // Boss信息
        _bossInfoLabel = new Label();
        _bossInfoLabel.Text = "选择一个Boss查看详情";
        vbox.AddChild(_bossInfoLabel);

        // 开始战斗按钮
        _startBattleButton = new Button();
        _startBattleButton.Text = "[Enter] 开始挑战";
        _startBattleButton.Pressed += OnStartBattlePressed;
        vbox.AddChild(_startBattleButton);
    }

    private void SetupBattleTab()
    {
        _battleTab = new Control();
        _battleTab.Name = "战斗";
        _tabContainer.AddChild(_battleTab);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 10);
        _battleTab.AddChild(vbox);

        // Boss血条
        _bossNameLabel = new Label();
        _bossNameLabel.Text = "等待挑战...";
        _bossNameLabel.Align = Label.AlignEnum.Center;
        vbox.AddChild(_bossNameLabel);

        _bossHealthBar = new ConfidenceFloorHealthBar();
        _bossHealthBar.SetThresholds(0.3f, 0.15f); // 30% warning, 15% danger
        vbox.AddChild(_bossHealthBar);

        // 阶段和狂暴状态
        var statusHBox = new HBoxContainer();
        vbox.AddChild(statusHBox);

        _phaseLabel = new Label();
        _phaseLabel.Text = "阶段: -";
        statusHBox.AddChild(_phaseLabel);

        _enrageLabel = new Label();
        _enrageLabel.Text = "狂暴: 未激活";
        statusHBox.AddChild(_enrageLabel);

        // REQ-156-05: 狂暴模式标签（初始隐藏）
        _modeLabel = new Label();
        _modeLabel.Text = "☠️ ENRAGED";
        _modeLabel.Hide();
        vbox.AddChild(_modeLabel);

        // 当前攻击类型标签（REQ-156-05）
        _attackTypeLabel = new Label();
        _attackTypeLabel.Text = "";
        _attackTypeLabel.Hide();
        vbox.AddChild(_attackTypeLabel);

        // 技能列表
        var skillLabel = new Label();
        skillLabel.Text = "Boss技能:";
        vbox.AddChild(skillLabel);

        _skillList = new VBoxContainer();
        _skillList.SizeFlagsVertical = Control.SizeFlags.ExpandAndFill;
        vbox.AddChild(_skillList);

        // 战斗统计
        _combatStatsLabel = new Label();
        _combatStatsLabel.Text = "战斗统计: 等待挑战...";
        vbox.AddChild(_combatStatsLabel);
    }

    private void SetupStatsTab()
    {
        _statsTab = new Control();
        _statsTab.Name = "统计";
        _tabContainer.AddChild(_statsTab);

        var vbox = new VBoxContainer();
        vbox.SetAnchorsPreset(Control.LayoutPreset.FullRect);
        vbox.AddThemeConstantOverride("separation", 10);
        _statsTab.AddChild(vbox);

        // 总统计
        _totalStatsLabel = new Label();
        _totalStatsLabel.Text = "=== 总体统计 ===\n等待数据...";
        vbox.AddChild(_totalStatsLabel);

        // 最佳记录
        _bestRecordsLabel = new Label();
        _bestRecordsLabel.Text = "=== 最佳记录 ===\n等待数据...";
        vbox.AddChild(_bestRecordsLabel);

        // 历史记录
        var historyLabel = new Label();
        historyLabel.Text = "战斗历史:";
        vbox.AddChild(historyLabel);

        _historyList = new ItemList();
        _historyList.SizeFlagsVertical = Control.SizeFlags.ExpandAndFill;
        vbox.AddChild(_historyList);
    }

    private void ConnectSignals()
    {
        if (_bossSystem != null)
        {
            BossMechanicsSystem.BossSpawned += OnBossSpawned;
            BossMechanicsSystem.BossDefeated += OnBossDefeated;
            BossMechanicsSystem.BossPhaseChanged += OnBossPhaseChanged;
            BossMechanicsSystem.BossEnraged += OnBossEnraged;
            BossMechanicsSystem.BossSkillUsed += OnBossSkillUsed;
            BossMechanicsSystem.PlayerComboChanged += OnPlayerComboChanged;
        }

        // REQ-156-05: 订阅模式切换信号
        BossEnrageManager.OnBossModeChanged += OnBossModeChanged;
    }

    private void RefreshBossList()
    {
        _bossList.Clear();
        
        if (_bossSystem == null) return;
        
        var bosses = _bossSystem.GetAllBossConfigs();
        foreach (var kvp in bosses)
        {
            var boss = kvp.Value;
            string displayText = $"{GetBossTypeIcon(boss.Type)} {boss.Name} (Lv.{boss.Level})";
            _bossList.AddItem(displayText);
        }
    }

    private string GetBossTypeIcon(BossType type)
    {
        switch (type)
        {
            case BossType.Normal: return "👹";
            case BossType.Elite: return "💀";
            case BossType.World: return "🐉";
            case BossType.Legendary: return "👑";
            case BossType.Raid: return "🏰";
            case BossType.Dungeon: return "🗝️";
            default: return "❓";
        }
    }

    private void OnBossListItemSelected(int index)
    {
        var bosses = _bossSystem.GetAllBossConfigs();
        int i = 0;
        foreach (var kvp in bosses)
        {
            if (i == index)
            {
                _selectedBossId = kvp.Key;
                UpdateBossInfo(kvp.Value);
                break;
            }
            i++;
        }
    }

    private void UpdateBossInfo(BossConfig boss)
    {
        string info = $@"
=== {boss.Name} ===
类型: {boss.Type}
难度: {boss.Difficulty}
等级: {boss.Level}

生命值: {boss.MaxHealth:N0}
攻击力: {boss.AttackPower}
防御力: {boss.Defense}

技能数量: {boss.Skills.Count}
阶段数: {boss.PhaseCount}
狂暴时间: {boss.EnrageTimer}秒

金币奖励: {boss.GoldReward:N0}
经验奖励: {boss.ExpReward:N0}
积分奖励: {boss.PointReward}

{boss.Description}
";
        _bossInfoLabel.Text = info;
    }

    private void OnStartBattlePressed()
    {
        if (string.IsNullOrEmpty(_selectedBossId)) return;
        
        // 假设玩家ID为"player1"
        _bossSystem.StartBossBattle(_selectedBossId, "player1");
    }

    private void OnBossSpawned(string bossId, string bossName, BossType type)
    {
        _bossNameLabel.Text = $"⚔️ 战斗中: {bossName}";
        _currentBattleId = bossId;
        
        var battles = _bossSystem.GetAllActiveBattles();
        foreach (var battle in battles)
        {
            if (battle.BossConfigId == bossId)
            {
                UpdateBattleUI(battle);
                break;
            }
        }
        
        RefreshStats();
    }

    private void UpdateBattleUI(BossBattleInstance battle)
    {
        _bossHealthBar.SetHealth(battle.CurrentHealth, battle.Config.MaxHealth);
        
        _phaseLabel.Text = $"阶段: {battle.CurrentPhase}/{battle.Config.PhaseCount}";
        
        if (battle.IsEnraged)
        {
            if (battle.IsRageTriggered)
            {
                // HP < 5% rage (REQ-127)
                _enrageLabel.Text = "☠️ 狂暴: HP临界!";
                _enrageLabel.Modulate = new Color(0.8f, 0f, 0f);
            }
            else
            {
                _enrageLabel.Text = "⚠️ 狂暴: 已激活!";
                _enrageLabel.Modulate = new Color(1, 0, 0);
            }
        }
        
        // 更新技能列表
        foreach (var child in _skillList.GetChildren())
        {
            child.QueueFree();
        }
        
        foreach (var skill in battle.Config.Skills)
        {
            var skillLabel = new Label();
            skillLabel.Text = $"• {skill.Name} ({skill.SkillType})";
            _skillList.AddChild(skillLabel);
        }
        
        // 更新统计
        int combo = _bossSystem.GetCombo("player1");
        _combatStatsLabel.Text = $"连击数: {combo}";
    }

    private void OnBossDefeated(string bossId, string bossName, bool isFirstBlood, List<string> rewards)
    {
        string result = isFirstBlood ? "🎉 首杀!" : "✅ 击败!";
        _bossNameLabel.Text = $"{result} {bossName}";
        
        string rewardText = "奖励:\n";
        foreach (var reward in rewards)
        {
            rewardText += reward + "\n";
        }
        
        _combatStatsLabel.Text = rewardText;
        
        RefreshStats();
    }

    private void OnBossPhaseChanged(string bossId, int newPhase)
    {
        _phaseLabel.Text = $"阶段: {newPhase}";
        
        // 更新UI显示
    }

    private void OnBossEnraged(string bossId)
    {
        // 检查是HP-based rage还是time-based enrage
        var battles = _bossSystem.GetAllActiveBattles();
        foreach (var battle in battles)
        {
            if (battle.BossConfigId == bossId)
            {
                if (battle.IsRageTriggered)
                {
                    // HP < 5% rage (REQ-127)
                    _enrageLabel.Text = "☠️ 狂暴: HP临界!";
                    _enrageLabel.Modulate = new Color(0.8f, 0f, 0f);
                }
                else
                {
                    _enrageLabel.Text = "⚠️ 狂暴: 已激活!";
                    _enrageLabel.Modulate = new Color(1, 0, 0);
                }
                break;
            }
        }
    }

    /// <summary>
    /// REQ-156-05: 模式切换视觉反馈
    /// 收到 ModeChanged 信号时触发：
    /// 1. 显示 ENRAGED 标签
    /// 2. 触发屏幕轻微震动
    /// 3. 重置脉冲动画计时器
    /// </summary>
    private void OnBossModeChanged(string battleInstanceId, int oldMode, int newMode)
    {
        if (newMode == 1) // Enraged mode
        {
            _isModeEnraged = true;
            _pulseTimer = 0f;

            // 显示 ENRAGED 标签
            _modeLabel.Show();
            _modeLabel.Modulate = new Color(1f, 0f, 0f, 1f);

            // 攻击类型标签显示狂暴模式
            _attackTypeLabel.Text = "⚡ 狂暴模式: 攻击随机化";
            _attackTypeLabel.Modulate = new Color(1f, 0.3f, 0f, 1f);
            _attackTypeLabel.Show();

            GD.Print("[BossMechanicsUI] ENRAGED mode activated! Starting pulse animation.");
        }
        else // Strategic mode
        {
            _isModeEnraged = false;
            _modeLabel.Hide();
            _attackTypeLabel.Hide();
        }
    }

    /// <summary>
    /// REQ-156-05: 狂暴模式脉冲动画
    /// </summary>
    public override void _Process(float delta)
    {
        if (!_isModeEnraged || _modeLabel == null) return;

        _pulseTimer += delta;

        // 脉冲动画：0.5秒周期，在 0.3~1.0 之间波动
        float pulse = 0.3f + 0.7f * (Mathf.Sin(_pulseTimer * Mathf.Pi * 2f) * 0.5f + 0.5f);
        _modeLabel.modulate = new Color(1f, pulse * 0.3f, pulse * 0.3f, 1f);

        // 轻微震动偏移（每帧微小抖动）
        float shakeX = (Mathf.Sin(_pulseTimer * 30f) * 2f);
        _modeLabel.RectPosition = new Vector2(shakeX, _modeLabel.RectPosition.y);
    }

    private void OnBossSkillUsed(string bossId, string skillId, string skillName)
    {
        // 可选：显示技能使用提示
    }

    private void OnPlayerComboChanged(string playerId, int newCombo)
    {
        if (playerId == "player1")
        {
            _combatStatsLabel.Text = $"连击数: {newCombo}";
        }
    }

    private void RefreshStats()
    {
        if (_bossSystem == null) return;
        
        var stats = _bossSystem.GetPlayerStats();
        
        string totalStats = $@"=== 总体统计 ===
击败Boss总数: {stats.TotalBossesDefeated}
世界Boss击杀: {stats.WorldBossKills}
传说Boss击杀: {stats.LegendaryBossKills}
首杀次数: {stats.FirstBloods}

总伤害: {stats.TotalDamageDealt:N0}
总存活时间: {stats.TotalSurvivalTime:N1}秒
最佳连击: {stats.BestCombo}
";
        _totalStatsLabel.Text = totalStats;
        
        string bestRecords = "=== 最佳记录 ===\n";
        
        foreach (var kvp in stats.BestSurvivalTimes)
        {
            bestRecords += $"生存时间 - {kvp.Key}: {kvp.Value:N1}秒\n";
        }
        
        foreach (var kvp in stats.BestDPS)
        {
            bestRecords += $"DPS - {kvp.Key}: {kvp.Value:N1}\n";
        }
        
        _bestRecordsLabel.Text = bestRecords;
    }

    public override void _Input(InputEvent evt)
    {
        if (evt is InputEventKey keyEvent && keyEvent.Pressed)
        {
            switch (keyEvent.Scancode)
            {
                case KeyList.Up:
                    MoveSelection(-1);
                    break;
                case KeyList.Down:
                    MoveSelection(1);
                    break;
                case KeyList._1:
                    _tabContainer.CurrentTab = 0;
                    break;
                case KeyList._2:
                    _tabContainer.CurrentTab = 1;
                    break;
                case KeyList._3:
                    _tabContainer.CurrentTab = 2;
                    RefreshStats();
                    break;
                case KeyList.Enter:
                    if (_tabContainer.CurrentTab == 0)
                        OnStartBattlePressed();
                    break;
                case KeyList.Escape:
                    Visible = false;
                    break;
            }
        }
    }

    private void MoveSelection(int direction)
    {
        int current = _bossList.GetSelectedItems().Length > 0 ? _bossList.GetSelectedItems()[0] : 0;
        int newIndex = Mathf.Clamp(current + direction, 0, _bossList.GetItemCount() - 1);
        _bossList.Select(newIndex);
        OnBossListItemSelected(newIndex);
    }

    public void ToggleUI()
    {
        Visible = !Visible;
        if (Visible)
        {
            RefreshBossList();
            RefreshStats();
        }
    }
}
