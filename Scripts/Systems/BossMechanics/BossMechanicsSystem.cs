using Godot;
using System;
using System.Collections.Generic;

public class BossMechanicsSystem : BaseSystem
{
    public static BossMechanicsSystem Instance { get; private set; }

    private Dictionary<string, BossBattleInstance> _activeBossBattles = new Dictionary<string, BossBattleInstance>();
    private Dictionary<string, BossBattleRecord> _playerBattleRecords = new Dictionary<string, BossBattleRecord>();
    private PlayerBossStats _playerStats = new PlayerBossStats();
    private Random _random = new Random();

    // 子系统
    private BossPhaseManager _phaseManager;
    private BossAI _ai;
    private BossAbilityDatabase _abilityDb;

    // Boss状态管理
    private Dictionary<string, float> _bossHealthBars = new Dictionary<string, float>();
    private Dictionary<string, int> _comboCounters = new Dictionary<string, int>();
    private Dictionary<string, DateTime> _lastAttackTimes = new Dictionary<string, DateTime>();

    // 信号事件
    public static signal BossSpawned(string bossId, string bossName, BossType type);
    public static signal BossDefeated(string bossId, string bossName, bool isFirstBlood, List<string> rewards);
    public static signal BossEscaped(string bossId, string bossName);
    public static signal BossPhaseChanged(string bossId, int newPhase);
    public static signal BossEnraged(string bossId);
    public static signal BossSkillUsed(string bossId, string skillId, string skillName);
    public static signal PlayerComboChanged(string playerId, int newCombo);
    public static signal BattleRecordUpdated(string playerId, BossBattleRecord record);

    public override void _Ready()
    {
        Instance = this;
        BossMechanicsDatabase.Initialize();
        
        // 初始化子系统
        _phaseManager = new BossPhaseManager(this);
        _ai = new BossAI(this);
        _abilityDb = new BossAbilityDatabase(this);
        
        // 订阅子系统事件
        SubscribeToSubsystems();
        
        LoadPlayerStats();
    }

    private void SubscribeToSubsystems()
    {
        // 转发子系统事件
        _phaseManager.OnPhaseTransition += (state, phase) => 
        {
            BossPhaseChanged?.Emit(state.InstanceId, phase.PhaseNumber);
        };
    }

    public override void _Process(float delta)
    {
        UpdateBossBattles(delta);
    }

    private void UpdateBossBattles(float delta)
    {
        foreach (var battle in _activeBossBattles.Values)
        {
            if (!battle.IsAlive) continue;

            battle.TimeInCombat += delta;
            battle.TimeSinceLastAttack += delta;
            battle.TimeSinceLastSkill += delta;

            // 更新技能冷却
            foreach (var skillCooldown in battle.SkillCooldowns)
            {
                battle.SkillCooldowns[skillCooldown.Key] = Mathf.Max(0, skillCooldown.Value - delta);
            }

            // 检查狂暴 - 委托给阶段管理器
            if (battle.Config.EnrageTimer > 0 && battle.TimeInCombat >= battle.Config.EnrageTimer && !battle.IsEnraged)
            {
                TriggerEnrage(battle);
            }

            // 检查阶段转换 - 委托给阶段管理器
            float healthPercent = battle.CurrentHealth / battle.Config.MaxHealth;
            int targetPhase = _phaseManager.GetPhaseFromHealth(healthPercent, battle.Config.PhaseCount);
            if (targetPhase > battle.CurrentPhase)
            {
                _phaseManager.TransitionToPhase(battle, targetPhase);
            }

            // AI决策 - 委托给AI系统
            _ai.Update(battle, delta);
        }
    }

    private void TriggerEnrage(BossBattleInstance battle)
    {
        battle.IsEnraged = true;
        battle.EnrageProgress = 1.0f;
        battle.CurrentDamageMultiplier *= 2.0f;
        battle.CurrentSpeedMultiplier *= 1.5f;
        
        // 使用狂暴技能
        var enrageSkill = _abilityDb.FindSkill(battle, "boss_enrage");
        if (enrageSkill != null)
        {
            BossSkillUsed?.Emit(battle.InstanceId, enrageSkill.Id, enrageSkill.Name);
        }
        
        BossEnraged?.Emit(battle.InstanceId);
    }

    /// <summary>
    /// 开始Boss战斗
    /// </summary>
    public void StartBossBattle(string bossId, string playerId)
    {
        var bossData = BossMechanicsDatabase.Instance.GetBoss(bossId);
        if (bossData == null)
        {
            GD.PrintErr($"Boss not found: {bossId}");
            return;
        }

        var bossConfig = BossMechanicsDatabase.Instance.GetBossConfig(bossId);
        
        var battle = new BossBattleInstance
        {
            InstanceId = Guid.NewGuid().ToString(),
            BossId = bossId,
            PlayerId = playerId,
            Config = bossConfig,
            CurrentHealth = bossConfig.MaxHealth,
            MaxHealth = bossConfig.MaxHealth,
            CurrentPhase = 1,
            IsAlive = true,
            TimeInCombat = 0,
            TimeSinceLastAttack = 0,
            TimeSinceLastSkill = 0,
            SkillCooldowns = new Dictionary<string, float>()
        };

        // 初始化技能冷却
        foreach (var skill in bossConfig.Skills)
        {
            battle.SkillCooldowns[skill.Id] = 0;
        }

        _activeBossBattles[battle.InstanceId] = battle;
        _bossHealthBars[battle.InstanceId] = 1.0f;
        _comboCounters[playerId] = 0;
        _lastAttackTimes[playerId] = DateTime.Now;
        
        // 初始化阶段管理器
        _phaseManager.InitializeBattle(battle);
        
        // 初始化AI
        _ai.InitializeBattle(battle);

        BossSpawned?.Emit(bossId, bossData.DisplayName, bossData.Type);

        GD.Print($"Boss battle started: {bossData.DisplayName}");
    }

    /// <summary>
    /// 对Boss造成伤害
    /// </summary>
    public void DealDamageToBoss(string instanceId, string playerId, float damage)
    {
        if (!_activeBossBattles.TryGetValue(instanceId, out var battle))
            return;

        // 更新连击
        UpdateCombo(playerId);

        // 计算伤害
        float actualDamage = damage * battle.CurrentDamageMultiplier;
        battle.CurrentHealth -= actualDamage;
        battle.TotalDamageDealt += actualDamage;
        _playerStats.TotalDamageDealt += actualDamage;

        // 更新血条
        _bossHealthBars[instanceId] = battle.CurrentHealth / battle.MaxHealth;

        // 检查是否击败
        if (battle.CurrentHealth <= 0)
        {
            battle.IsAlive = false;
            battle.CurrentHealth = 0;
            DefeatBoss(instanceId, playerId);
        }
    }

    /// <summary>
    /// 击败Boss
    /// </summary>
    private void DefeatBoss(string instanceId, string playerId)
    {
        var battle = _activeBossBattles[instanceId];
        var bossData = BossMechanicsDatabase.Instance.GetBoss(battle.BossId);
        
        bool isFirstBlood = !_playerBattleRecords.ContainsKey(playerId) || 
            !_playerBattleRecords[playerId].DefeatedBosses.Contains(battle.BossId);
        
        // 发放奖励
        var rewards = new List<string>();
        if (bossData.RewardItems != null)
        {
            rewards.AddRange(bossData.RewardItems);
        }
        
        // 更新统计
        _playerStats.TotalBossesDefeated++;
        _playerStats.TotalDamageDealt += battle.TotalDamageDealt;
        
        if (battle.Config.Type == BossType.WorldBoss)
            _playerStats.WorldBossKills++;
        if (battle.Config.Type == BossType.Legendary)
            _playerStats.LegendaryBossKills++;
        
        if (isFirstBlood)
            _playerStats.FirstBloods++;
        
        // 记录
        if (!_playerBattleRecords.ContainsKey(playerId))
        {
            _playerBattleRecords[playerId] = new BossBattleRecord();
        }
        
        var record = new BossBattleRecord
        {
            BossId = battle.BossId,
            InstanceId = battle.InstanceId,
            DamageDealt = battle.TotalDamageDealt,
            SurvivalTime = battle.TimeInCombat,
            IsFirstBlood = isFirstBlood,
            Timestamp = DateTime.Now
        };
        
        _playerBattleRecords[playerId].DefeatedBosses.Add(battle.BossId);
        _playerBattleRecords[playerId].Records.Add(record);
        
        BattleRecordUpdated?.Emit(playerId, record);
        BossDefeated?.Emit(battle.BossId, bossData?.DisplayName ?? battle.BossId, isFirstBlood, rewards);

        // 清理
        _activeBossBattles.Remove(instanceId);
        _bossHealthBars.Remove(instanceId);
    }

    /// <summary>
    /// 更新连击
    /// </summary>
    private void UpdateCombo(string playerId)
    {
        var now = DateTime.Now;
        if (_lastAttackTimes.TryGetValue(playerId, out var lastTime))
        {
            var gap = (now - lastTime).TotalSeconds;
            if (gap < 3.0)
            {
                _comboCounters[playerId]++;
            }
            else
            {
                _comboCounters[playerId] = 1;
            }
        }
        else
        {
            _comboCounters[playerId] = 1;
        }
        
        _lastAttackTimes[playerId] = now;
        
        var combo = _comboCounters[playerId];
        if (combo > _playerStats.BestCombo)
            _playerStats.BestCombo = combo;
        
        _playerStats.TotalComboScore += combo;
        
        PlayerComboChanged?.Emit(playerId, combo);
    }

    /// <summary>
    /// 获取Boss战斗实例
    /// </summary>
    public BossBattleInstance GetBattleInstance(string instanceId)
    {
        return _activeBossBattles.GetValueOrDefault(instanceId);
    }

    /// <summary>
    /// 获取玩家战斗记录
    /// </summary>
    public BossBattleRecord GetPlayerRecord(string playerId)
    {
        return _playerBattleRecords.GetValueOrDefault(playerId);
    }

    /// <summary>
    /// 获取玩家统计
    /// </summary>
    public PlayerBossStats GetPlayerStats()
    {
        return _playerStats;
    }

    /// <summary>
    /// 获取Boss血条百分比
    /// </summary>
    public float GetBossHealthPercent(string instanceId)
    {
        return _bossHealthBars.GetValueOrDefault(instanceId, 0);
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // 委托给子系统
        if (_phaseManager != null)
            data["phaseManager"] = _phaseManager.ExportSaveData();
        if (_ai != null)
            data["ai"] = _ai.ExportSaveData();
        
        // 导出玩家统计数据
        data["totalBossesDefeated"] = _playerStats.TotalBossesDefeated;
        data["worldBossKills"] = _playerStats.WorldBossKills;
        data["legendaryBossKills"] = _playerStats.LegendaryBossKills;
        data["totalDamageDealt"] = _playerStats.TotalDamageDealt;
        data["totalDamageTaken"] = _playerStats.TotalDamageTaken;
        data["totalSurvivalTime"] = _playerStats.TotalSurvivalTime;
        data["firstBloods"] = _playerStats.FirstBloods;
        data["bestCombo"] = _playerStats.BestCombo;
        data["totalComboScore"] = _playerStats.TotalComboScore;
        
        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // 委托给子系统
        if (_phaseManager != null && data.Contains("phaseManager"))
            _phaseManager.ImportSaveData(data["phaseManager"] as Dictionary);
        if (_ai != null && data.Contains("ai"))
            _ai.ImportSaveData(data["ai"] as Dictionary);
        
        // 导入玩家统计数据
        _playerStats.TotalBossesDefeated = data.GetValueOrDefault("totalBossesDefeated", 0);
        _playerStats.WorldBossKills = data.GetValueOrDefault("worldBossKills", 0);
        _playerStats.LegendaryBossKills = data.GetValueOrDefault("legendaryBossKills", 0);
        _playerStats.TotalDamageDealt = data.GetValueOrDefault("totalDamageDealt", 0);
        _playerStats.TotalDamageTaken = data.GetValueOrDefault("totalDamageTaken", 0);
        _playerStats.TotalSurvivalTime = data.GetValueOrDefault("totalSurvivalTime", 0.0f);
        _playerStats.FirstBloods = data.GetValueOrDefault("firstBloods", 0);
        _playerStats.BestCombo = data.GetValueOrDefault("bestCombo", 0);
        _playerStats.TotalComboScore = data.GetValueOrDefault("totalComboScore", 0);
    }

    private void LoadPlayerStats()
    {
    }

    private void SavePlayerStats()
    {
    }
}
