using Godot;
using System;
using System.Collections.Generic;

public partial class BossMechanicsSystem : BaseSystem
{
    public static BossMechanicsSystem Instance { get; private set; }

    private Dictionary<string, BossBattleInstance> _activeBossBattles = new Dictionary<string, BossBattleInstance>();
    private Dictionary<string, BossBattleRecord> _playerBattleRecords = new Dictionary<string, BossBattleRecord>();
    private PlayerBossStats _playerStats = new PlayerBossStats();
    private Random _random = new Random();

    // 玩家连击管理（保留在主系统）
    private Dictionary<string, int> _comboCounters = new Dictionary<string, int>();
    private Dictionary<string, DateTime> _lastAttackTimes = new Dictionary<string, DateTime>();

    // 信号事件（C# Action 委托）
    public static Action<string, string, BossType> BossSpawned;
    public static Action<string, string, bool, List<string>> BossDefeated;
    public static Action<string, string> BossEscaped;
    public static Action<string, int> BossPhaseChanged;
    public static Action<string> BossEnraged;
    public static Action<string, string, string> BossSkillUsed;
    public static Action<string, int> PlayerComboChanged;
    public static Action<string, BossBattleRecord> BattleRecordUpdated;

    // 子系统实例
    private BossPhaseSystem _phaseSystem;
    private BossAbilitySystem _abilitySystem;
    private BossPatternSystem _patternSystem;

    public override void _Ready()
    {
        Instance = this;
        BossMechanicsDatabase.Initialize();
        
        // 初始化子系统
        _phaseSystem = new BossPhaseSystem();
        _abilitySystem = new BossAbilitySystem();
        _patternSystem = new BossPatternSystem();
        
        // 将子系统添加到场景树
        AddChild(_phaseSystem);
        AddChild(_abilitySystem);
        AddChild(_patternSystem);
        
        // 连接子系统信号
        ConnectSubsystemSignals();

        // 连接Rage触发信号 (REQ-127)
        BossPhaseSystem.Instance.BossRageTriggered += _OnBossRageTriggered;
        
        LoadPlayerStats();
    }

    private void ConnectSubsystemSignals()
    {
        // 连接PhaseSystem信号
        BossPhaseSystem.Instance.BossPhaseChanged += _OnPhaseChanged;
        BossPhaseSystem.Instance.BossEnraged += _OnBossEnraged;
        
        // 连接AbilitySystem信号
        BossAbilitySystem.Instance.BossSkillExecuted += _OnSkillExecuted;
        
        // 连接PatternSystem信号
        BossPatternSystem.Instance.BossPatternChanged += _OnPatternChanged;
    }

    private void _OnPhaseChanged(string instanceId, int oldPhase, int newPhase)
    {
        // 转发阶段变化信号（保持兼容）
        if (_activeBossBattles.ContainsKey(instanceId))
        {
            BossPhaseChanged?.Invoke(_activeBossBattles[instanceId].BossConfigId, newPhase);
        }
    }

    private void _OnBossEnraged(string instanceId)
    {
        // 转发狂暴信号
        if (_activeBossBattles.ContainsKey(instanceId))
        {
            BossEnraged?.Invoke(_activeBossBattles[instanceId].BossConfigId);
        }
    }

    private void _OnBossRageTriggered(string instanceId)
    {
        // 转发HP-based狂暴信号 (REQ-127)
        if (_activeBossBattles.ContainsKey(instanceId))
        {
            BossEnraged?.Invoke(_activeBossBattles[instanceId].BossConfigId);
        }
    }

    private void _OnSkillExecuted(string instanceId, string skillId, string skillName)
    {
        // 转发技能使用信号
        if (_activeBossBattles.ContainsKey(instanceId))
        {
            BossSkillUsed?.Invoke(_activeBossBattles[instanceId].BossConfigId, skillId, skillName);
        }
    }

    private void _OnPatternChanged(string instanceId, AttackPattern oldPattern, AttackPattern newPattern)
    {
        // 模式变化信号（可用于UI显示等）
    }

    public override void _Process(double delta)
    {
        UpdateBossBattles(delta);
    }

    private void UpdateBossBattles(float delta)
    {
        foreach (var battle in _activeBossBattles.Values)
        {
            if (!battle.IsAlive) continue;

            // 委托给子系统处理
            _phaseSystem.UpdatePhase(battle, delta);
            _abilitySystem.UpdateAbilities(battle, delta);
            _patternSystem.UpdatePattern(battle, delta);

            // 检查阶段转换完成
            CheckPhaseTransitionComplete(battle);
        }
    }

    private void CheckPhaseTransitionComplete(BossBattleInstance battle)
    {
        if (battle.Phase == BossPhase.Transition)
        {
            // 检查是否已经过了转换时间
            // 这里简化处理，实际可以根据转换开始时间判断
            if (battle.TimeInCombat > 2.0f) // 假设转换需要2秒
            {
                _phaseSystem.CompletePhaseTransition(battle);
            }
        }
    }

    // 公开API - 战斗管理
    public void StartBossBattle(string bossId, string playerId)
    {
        var config = BossMechanicsDatabase.GetBossConfig(bossId);
        if (config == null) return;

        string instanceId = Guid.NewGuid().ToString();
        var battle = new BossBattleInstance
        {
            InstanceId = instanceId,
            BossConfigId = bossId,
            Config = config,
            CurrentHealth = config.MaxHealth,
            TimeInCombat = 0,
            TimeSinceLastAttack = 0,
            TimeSinceLastSkill = 2.0f,
            TargetsInCombat = 1
        };

        // 初始化子系统
        _phaseSystem.InitializePhase(battle);
        _abilitySystem.InitializeAbilities(battle);
        _patternSystem.InitializePattern(battle);

        // 初始化玩家伤害记录
        battle.PlayerDamageDealt[playerId] = 0;
        battle.PlayerHealingDone[playerId] = 0;

        _activeBossBattles[instanceId] = battle;
        _playerBattleRecords[playerId] = new BossBattleRecord
        {
            BossId = bossId,
            BossName = config.Name,
            BattleStartTime = DateTime.Now,
            RewardsReceived = new List<string>()
        };

        BossSpawned?.Invoke(bossId, config.Name, config.Type);
    }

    public void DealDamageToBoss(string instanceId, string playerId, float damage)
    {
        if (!_activeBossBattles.ContainsKey(instanceId)) return;
        
        var battle = _activeBossBattles[instanceId];
        
        // 应用护盾减免
        float actualDamage = _abilitySystem.ApplyShieldReduction(battle, damage);
        actualDamage *= battle.CurrentDamageMultiplier;
        
        battle.CurrentHealth -= actualDamage;
        
        if (battle.PlayerDamageDealt.ContainsKey(playerId))
            battle.PlayerDamageDealt[playerId] += actualDamage;
        
        if (_playerBattleRecords.ContainsKey(playerId))
            _playerBattleRecords[playerId].TotalDamageDealt += actualDamage;

        // 更新连击
        UpdateCombo(playerId);

        // 根据血量更新模式
        _patternSystem.UpdatePatternByHealth(battle);

        // 检查Boss死亡
        if (!battle.IsAlive)
        {
            CompleteBossBattle(instanceId, playerId);
        }
    }

    private void UpdateCombo(string playerId)
    {
        if (!_comboCounters.ContainsKey(playerId))
            _comboCounters[playerId] = 0;
            
        var lastAttack = _lastAttackTimes.ContainsKey(playerId) ? _lastAttackTimes[playerId] : DateTime.MinValue;
        
        if ((DateTime.Now - lastAttack).TotalSeconds < 3.0)
        {
            _comboCounters[playerId]++;
        }
        else
        {
            _comboCounters[playerId] = 1;
        }
        
        _lastAttackTimes[playerId] = DateTime.Now;
        
        // 更新统计
        if (_comboCounters[playerId] > _playerStats.BestCombo)
            _playerStats.BestCombo = _comboCounters[playerId];
            
        PlayerComboChanged?.Invoke(playerId, _comboCounters[playerId]);
    }

    private void CompleteBossBattle(string instanceId, string playerId)
    {
        if (!_activeBossBattles.ContainsKey(instanceId)) return;
        
        var battle = _activeBossBattles[instanceId];
        battle.Phase = BossPhase.Defeated;
        
        // 生成掉落
        List<string> rewards = GenerateRewards(battle);
        
        // 检查首杀
        bool isFirstBlood = !_playerStats.BossKillCount.ContainsKey(battle.BossConfigId);
        
        // 更新统计
        _playerStats.TotalBossesDefeated++;
        
        if (battle.Config.Type == BossType.World)
            _playerStats.WorldBossKills++;
        if (battle.Config.Type == BossType.Legendary)
            _playerStats.LegendaryBossKills++;
            
        if (isFirstBlood)
        {
            _playerStats.FirstBloods++;
            rewards.Add($"首杀奖励: {battle.Config.GoldReward * 0.5f} 金币");
        }
        
        // 更新击杀计数
        if (_playerStats.BossKillCount.ContainsKey(battle.BossConfigId))
            _playerStats.BossKillCount[battle.BossConfigId]++;
        else
            _playerStats.BossKillCount[battle.BossConfigId] = 1;
        
        // 记录战斗
        if (_playerBattleRecords.ContainsKey(playerId))
        {
            var record = _playerBattleRecords[playerId];
            record.IsVictory = true;
            record.BattleEndTime = DateTime.Now;
            record.RewardsReceived = rewards;
            
            // 更新最佳生存时间
            string bossKey = battle.BossConfigId;
            float survivalTime = (float)(record.BattleEndTime.Value - record.BattleStartTime).TotalSeconds;
            
            if (!_playerStats.BestSurvivalTimes.ContainsKey(bossKey) || 
                survivalTime > _playerStats.BestSurvivalTimes[bossKey])
            {
                _playerStats.BestSurvivalTimes[bossKey] = survivalTime;
            }
            
            // 更新最佳DPS
            float dps = record.TotalDamageDealt / survivalTime;
            if (!_playerStats.BestDPS.ContainsKey(bossKey) || dps > _playerStats.BestDPS[bossKey])
            {
                _playerStats.BestDPS[bossKey] = dps;
            }
            
            // 添加到历史记录
            if (!_playerStats.BattleHistory.ContainsKey(bossKey))
                _playerStats.BattleHistory[bossKey] = new List<BossBattleRecord>();
            _playerStats.BattleHistory[bossKey].Add(record);
            
            BattleRecordUpdated?.Invoke(playerId, record);
        }
        
        // 发放奖励
        foreach (var reward in rewards)
        {
            // 实际奖励发放逻辑
        }
        
        // 更新总计
        _playerStats.TotalDamageDealt += (int)battle.PlayerDamageDealt.GetValueOrDefault(playerId, 0);
        _playerStats.TotalSurvivalTime += (float)(DateTime.Now - _playerBattleRecords[playerId].BattleStartTime).TotalSeconds;
        
        // 发出信号
        BossDefeated?.Invoke(battle.BossConfigId, battle.Config.Name, isFirstBlood, rewards);
        
        // 清理
        _activeBossBattles.Remove(instanceId);
        
        SavePlayerStats();
    }

    private List<string> GenerateRewards(BossBattleInstance battle)
    {
        List<string> rewards = new List<string>();
        
        // 基础奖励
        rewards.Add($"金币: {battle.Config.GoldReward}");
        rewards.Add($"经验: {battle.Config.ExpReward}");
        if (battle.Config.PointReward > 0)
            rewards.Add($"积分: {battle.Config.PointReward}");
            
        // 掉落
        foreach (var drop in battle.Config.DropTable)
        {
            float roll = (float)_random.NextDouble();
            float adjustedChance = drop.DropChance;
            
            // 稀有加成
            if (drop.RareBonusChance > 0)
                adjustedChance += drop.RareBonusChance;
                
            if (roll <= adjustedChance || drop.IsGuaranteed)
            {
                int quantity = _random.Next(drop.MinQuantity, drop.MaxQuantity + 1);
                rewards.Add($"{drop.ItemId} x{quantity}");
            }
        }
        
        // 称号奖励
        if (!string.IsNullOrEmpty(battle.Config.TitleReward))
        {
            rewards.Add($"称号: {battle.Config.TitleReward}");
        }
        
        return rewards;
    }

    // 公开API - 查询方法
    public BossBattleInstance GetActiveBattle(string instanceId)
    {
        return _activeBossBattles.ContainsKey(instanceId) ? _activeBossBattles[instanceId] : null;
    }

    public List<BossBattleInstance> GetAllActiveBattles()
    {
        return new List<BossBattleInstance>(_activeBossBattles.Values);
    }

    public PlayerBossStats GetPlayerStats()
    {
        return _playerStats;
    }

    public int GetCombo(string playerId)
    {
        return _comboCounters.ContainsKey(playerId) ? _comboCounters[playerId] : 0;
    }

    public Dictionary<string, BossConfig> GetAllBossConfigs()
    {
        return BossMechanicsDatabase.GetAllBossConfigs();
    }

    public List<BossConfig> GetBossConfigsByType(BossType type)
    {
        return BossMechanicsDatabase.GetBossConfigsByType(type);
    }

    public List<BossConfig> GetBossConfigsByDifficulty(DifficultyLevel difficulty)
    {
        return BossMechanicsDatabase.GetBossConfigsByDifficulty(difficulty);
    }

    // 公开API - 子系统访问（保持向后兼容）
    public BossPhaseSystem GetPhaseSystem() => _phaseSystem;
    public BossAbilitySystem GetAbilitySystem() => _abilitySystem;
    public BossPatternSystem GetPatternSystem() => _patternSystem;

    // 存档支持
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        // 导出统计数据
        data["totalBossesDefeated"] = _playerStats.TotalBossesDefeated;
        data["worldBossKills"] = _playerStats.WorldBossKills;
        data["legendaryBossKills"] = _playerStats.LegendaryBossKills;
        data["totalDamageDealt"] = _playerStats.TotalDamageDealt;
        data["totalDamageTaken"] = _playerStats.TotalDamageTaken;
        data["totalSurvivalTime"] = _playerStats.TotalSurvivalTime;
        data["firstBloods"] = _playerStats.FirstBloods;
        data["bestCombo"] = _playerStats.BestCombo;
        data["totalComboScore"] = _playerStats.TotalComboScore;
        
        // 导出击杀计数
        var killCountArray = new Godot.Collections.Array();
        foreach (var kvp in _playerStats.BossKillCount)
        {
            killCountArray.Add(new Godot.Collections.Array { kvp.Key, kvp.Value });
        }
        data["bossKillCount"] = killCountArray;
        
        // 导出最佳生存时间
        var survivalArray = new Godot.Collections.Array();
        foreach (var kvp in _playerStats.BestSurvivalTimes)
        {
            survivalArray.Add(new Godot.Collections.Array { kvp.Key, kvp.Value });
        }
        data["bestSurvivalTimes"] = survivalArray;
        
        // 导出最佳DPS
        var dpsArray = new Godot.Collections.Array();
        foreach (var kvp in _playerStats.BestDPS)
        {
            dpsArray.Add(new Godot.Collections.Array { kvp.Key, kvp.Value });
        }
        data["bestDPS"] = dpsArray;
        
        // 导出活跃战斗数据（各子系统）
        var battlesData = new Godot.Collections.Array();
        foreach (var battle in _activeBossBattles)
        {
            var battleData = new Dictionary<string, object>();
            battleData["instanceId"] = battle.Key;
            
            // 委托给子系统导出
            battleData["phaseData"] = _phaseSystem.ExportSaveData(battle.Value);
            battleData["abilityData"] = _abilitySystem.ExportSaveData(battle.Value);
            battleData["patternData"] = _patternSystem.ExportSaveData(battle.Value);
            
            battlesData.Add(battleData);
        }
        data["activeBattles"] = battlesData;
        
        return data;
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        _playerStats.TotalBossesDefeated = data.GetValueOrDefault("totalBossesDefeated", 0);
        _playerStats.WorldBossKills = data.GetValueOrDefault("worldBossKills", 0);
        _playerStats.LegendaryBossKills = data.GetValueOrDefault("legendaryBossKills", 0);
        _playerStats.TotalDamageDealt = data.GetValueOrDefault("totalDamageDealt", 0);
        _playerStats.TotalDamageTaken = data.GetValueOrDefault("totalDamageTaken", 0);
        _playerStats.TotalSurvivalTime = data.GetValueOrDefault("totalSurvivalTime", 0.0f);
        _playerStats.FirstBloods = data.GetValueOrDefault("firstBloods", 0);
        _playerStats.BestCombo = data.GetValueOrDefault("bestCombo", 0);
        _playerStats.TotalComboScore = data.GetValueOrDefault("totalComboScore", 0);
        
        // 导入击杀计数
        if (data.Contains("bossKillCount"))
        {
            _playerStats.BossKillCount.Clear();
            var killCountArray = (Godot.Collections.Array)data["bossKillCount"];
            foreach (Godot.Collections.Array entry in killCountArray)
            {
                _playerStats.BossKillCount[(string)entry[0]] = (int)entry[1];
            }
        }
        
        // 导入最佳生存时间
        if (data.Contains("bestSurvivalTimes"))
        {
            _playerStats.BestSurvivalTimes.Clear();
            var survivalArray = (Godot.Collections.Array)data["bestSurvivalTimes"];
            foreach (Godot.Collections.Array entry in survivalArray)
            {
                _playerStats.BestSurvivalTimes[(string)entry[0]] = (float)entry[1];
            }
        }
        
        // 导入最佳DPS
        if (data.Contains("bestDPS"))
        {
            _playerStats.BestDPS.Clear();
            var dpsArray = (Godot.Collections.Array)data["bestDPS"];
            foreach (Godot.Collections.Array entry in dpsArray)
            {
                _playerStats.BestDPS[(string)entry[0]] = (float)entry[1];
            }
        }
        
        // 导入活跃战斗数据
        if (data.Contains("activeBattles") && _phaseSystem != null && _abilitySystem != null && _patternSystem != null)
        {
            var battlesData = (Godot.Collections.Array)data["activeBattles"];
            foreach (Dictionary battleData in battlesData)
            {
                string instanceId = (string)battleData["instanceId"];
                
                if (_activeBossBattles.ContainsKey(instanceId))
                {
                    var battle = _activeBossBattles[instanceId];
                    
                    if (battleData.Contains("phaseData"))
                        _phaseSystem.ImportSaveData(battle, (Dictionary)battleData["phaseData"]);
                    
                    if (battleData.Contains("abilityData"))
                        _abilitySystem.ImportSaveData(battle, (Dictionary)battleData["abilityData"]);
                    
                    if (battleData.Contains("patternData"))
                        _patternSystem.ImportSaveData(battle, (Dictionary)battleData["patternData"]);
                }
            }
        }
    }

    private void LoadPlayerStats()
    {
        // 从存档加载玩家数据
    }

    private void SavePlayerStats()
    {
        // 保存玩家数据
    }
}
