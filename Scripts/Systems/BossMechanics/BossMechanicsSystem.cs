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
        LoadPlayerStats();
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

            // 检查狂暴
            if (battle.Config.EnrageTimer > 0 && battle.TimeInCombat >= battle.Config.EnrageTimer && !battle.IsEnraged)
            {
                TriggerEnrage(battle);
            }

            // 检查阶段转换
            float healthPercent = battle.CurrentHealth / battle.Config.MaxHealth;
            int targetPhase = GetPhaseFromHealth(healthPercent, battle.Config.PhaseCount);
            if (targetPhase > battle.CurrentPhase)
            {
                TransitionToPhase(battle, targetPhase);
            }

            // AI决策
            MakeBossDecision(battle, delta);
        }
    }

    private int GetPhaseFromHealth(float healthPercent, int totalPhases)
    {
        float phaseThreshold = 1.0f / totalPhases;
        for (int i = totalPhases - 1; i >= 0; i--)
        {
            if (healthPercent <= (i + 1) * phaseThreshold)
                return totalPhases - i;
        }
        return 1;
    }

    private void TriggerEnrage(BossBattleInstance battle)
    {
        battle.IsEnraged = true;
        battle.EnrageProgress = 1.0f;
        battle.CurrentDamageMultiplier *= 2.0f;
        battle.CurrentSpeedMultiplier *= 1.5f;
        
        // 使用狂暴技能
        var enrageSkill = FindSkill(battle, "boss_enrage");
        if (enrageSkill != null)
        {
            BossSkillUsed?.Emit(battle.InstanceId, enrageSkill.Id, enrageSkill.Name);
        }
        
        BossEnraged?.Emit(battle.InstanceId);
    }

    private void TransitionToPhase(BossBattleInstance battle, int newPhase)
    {
        battle.CurrentPhase = newPhase;
        battle.Phase = BossPhase.Transition;
        
        // 应用阶段加成
        float phaseMultiplier = 1.0f + (newPhase - 1) * 0.25f;
        battle.CurrentDamageMultiplier *= phaseMultiplier;
        
        BossPhaseChanged?.Emit(battle.InstanceId, newPhase);
        
        // 延迟恢复战斗状态
        GetTree().CreateTimer(2.0f).Connect("timeout", this, nameof(OnPhaseTransitionComplete), new Godot.Collections.Array { battle.InstanceId });
    }

    private void OnPhaseTransitionComplete(string instanceId)
    {
        if (_activeBossBattles.ContainsKey(instanceId))
        {
            _activeBossBattles[instanceId].Phase = BossPhase.Active;
        }
    }

    private void MakeBossDecision(BossBattleInstance battle, float delta)
    {
        // 基础攻击
        float attackInterval = 1.0f / battle.Config.AttackSpeed;
        if (battle.TimeSinceLastAttack >= attackInterval)
        {
            PerformBasicAttack(battle);
            battle.TimeSinceLastAttack = 0;
        }

        // 技能决策
        if (battle.TimeSinceLastSkill >= 3.0f)
        {
            var skill = SelectSkill(battle);
            if (skill != null)
            {
                ExecuteSkill(battle, skill);
                battle.TimeSinceLastSkill = 0;
            }
        }
    }

    private void PerformBasicAttack(BossBattleInstance battle)
    {
        float damage = battle.Config.AttackPower * battle.CurrentDamageMultiplier;
        
        // 添加暴击
        if (_random.NextDouble() < battle.Config.CriticalChance)
        {
            damage *= battle.Config.CriticalDamage;
        }

        // 应用到所有玩家
        foreach (var playerDamage in battle.PlayerDamageDealt.Keys)
        {
            // 实际战斗系统会处理伤害
        }

        battle.LastTargetPosition = Vector3.Zero; // 更新目标位置
    }

    private BossSkillConfig SelectSkill(BossBattleInstance battle)
    {
        List<BossSkillConfig> availableSkills = new List<BossSkillConfig>();
        
        foreach (var skill in battle.Config.Skills)
        {
            // 检查冷却
            if (battle.SkillCooldowns.ContainsKey(skill.Id) && battle.SkillCooldowns[skill.Id] > 0)
                continue;
                
            // 检查狂暴状态
            if (skill.IsEnragedOnly && !battle.IsEnraged)
                continue;
                
            // 检查阶段
            if (skill.PhaseRequired > battle.CurrentPhase)
                continue;
                
            // 检查概率
            if (_random.NextDouble() > skill.ExecuteProbability)
                continue;
                
            availableSkills.Add(skill);
        }
        
        if (availableSkills.Count == 0)
            return null;
            
        return availableSkills[_random.Next(availableSkills.Count)];
    }

    private void ExecuteSkill(BossBattleInstance battle, BossSkillConfig skill)
    {
        battle.SkillCooldowns[skill.Id] = skill.Cooldown;
        
        // 执行技能效果
        switch (skill.SkillType)
        {
            case BossSkillType.MeleeAttack:
            case BossSkillType.RangedAttack:
            case BossSkillType.Projectile:
                ApplyDirectDamage(battle, skill);
                break;
                
            case BossSkillType.AreaOfEffect:
                ApplyAreaDamage(battle, skill);
                break;
                
            case BossSkillType.Summon:
                SummonMonsters(battle, skill);
                break;
                
            case BossSkillType.Heal:
                ApplySelfHeal(battle, skill);
                break;
                
            case BossSkillType.Shield:
                ApplyShield(battle, skill);
                break;
                
            case BossSkillType.Debuff:
                ApplyDebuff(battle, skill);
                break;
                
            case BossSkillType.Teleport:
                PerformTeleport(battle);
                break;
                
            case BossSkillType.Stun:
                ApplyStun(battle, skill);
                break;
                
            case BossSkillType.Knockback:
                ApplyKnockback(battle, skill);
                break;
        }
        
        BossSkillUsed?.Emit(battle.InstanceId, skill.Id, skill.Name);
    }

    private void ApplyDirectDamage(BossBattleInstance battle, BossSkillConfig skill)
    {
        float damage = skill.Damage * battle.CurrentDamageMultiplier;
        
        if (_random.NextDouble() < battle.Config.CriticalChance)
        {
            damage *= battle.Config.CriticalDamage;
        }
    }

    private void ApplyAreaDamage(BossBattleInstance battle, BossSkillConfig skill)
    {
        float damage = skill.Damage * battle.CurrentDamageMultiplier;
        
        // 范围伤害逻辑
        if (_random.NextDouble() < battle.Config.CriticalChance)
        {
            damage *= battle.Config.CriticalDamage;
        }
    }

    private void SummonMonsters(BossBattleInstance battle, BossSkillConfig skill)
    {
        if (!string.IsNullOrEmpty(skill.SummonMonsterId))
        {
            for (int i = 0; i < skill.SummonCount; i++)
            {
                battle.SummonedMonsters.Add(skill.SummonMonsterId);
            }
        }
    }

    private void ApplySelfHeal(BossBattleInstance battle, BossSkillConfig skill)
    {
        float healAmount = skill.HealAmount;
        battle.CurrentHealth = Mathf.Min(battle.Config.MaxHealth, battle.CurrentHealth + healAmount);
    }

    private void ApplyShield(BossBattleInstance battle, BossSkillConfig skill)
    {
        battle.ActiveEffects.Add($"shield_{skill.ShieldAmount}");
    }

    private void ApplyDebuff(BossBattleInstance battle, BossSkillConfig skill)
    {
        foreach (var debuffId in skill.DebuffIds)
        {
            battle.ActiveEffects.Add(debuffId);
        }
    }

    private void PerformTeleport(BossBattleInstance battle)
    {
        battle.ActiveEffects.Add("teleporting");
    }

    private void ApplyStun(BossBattleInstance battle, BossSkillConfig skill)
    {
        battle.ActiveEffects.Add($"stun_{skill.StunDuration}");
    }

    private void ApplyKnockback(BossBattleInstance battle, BossSkillConfig skill)
    {
        battle.ActiveEffects.Add($"knockback_{skill.KnockbackForce}");
    }

    private BossSkillConfig FindSkill(BossBattleInstance battle, string skillId)
    {
        foreach (var skill in battle.Config.Skills)
        {
            if (skill.Id == skillId)
                return skill;
        }
        return null;
    }

    // 公开API
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
            CurrentPhase = 1,
            Phase = BossPhase.Active,
            TimeInCombat = 0,
            TimeSinceLastAttack = 0,
            TimeSinceLastSkill = 2.0f,
            CurrentPattern = config.DefaultPattern,
            IsEnraged = false,
            EnrageProgress = 0,
            CurrentDamageMultiplier = 1.0f,
            CurrentSpeedMultiplier = 1.0f,
            TargetsInCombat = 1
        };

        // 初始化技能冷却
        foreach (var skill in config.Skills)
        {
            battle.SkillCooldowns[skill.Id] = _random.Next(0, (int)skill.Cooldown);
        }

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

        BossSpawned?.Emit(bossId, config.Name, config.Type);
    }

    public void DealDamageToBoss(string instanceId, string playerId, float damage)
    {
        if (!_activeBossBattles.ContainsKey(instanceId)) return;
        
        var battle = _activeBossBattles[instanceId];
        float actualDamage = damage * battle.CurrentDamageMultiplier;
        
        // 检查护盾
        float shieldAmount = 0;
        for (int i = battle.ActiveEffects.Count - 1; i >= 0; i--)
        {
            if (battle.ActiveEffects[i].StartsWith("shield_"))
            {
                shieldAmount = float.Parse(battle.ActiveEffects[i].Split('_')[1]);
                if (shieldAmount >= actualDamage)
                {
                    battle.ActiveEffects[i] = $"shield_{shieldAmount - actualDamage}";
                    actualDamage = 0;
                }
                else
                {
                    actualDamage -= shieldAmount;
                    battle.ActiveEffects.RemoveAt(i);
                }
                break;
            }
        }

        battle.CurrentHealth -= actualDamage;
        
        if (battle.PlayerDamageDealt.ContainsKey(playerId))
            battle.PlayerDamageDealt[playerId] += actualDamage;
        
        if (_playerBattleRecords.ContainsKey(playerId))
            _playerBattleRecords[playerId].TotalDamageDealt += actualDamage;

        // 更新连击
        UpdateCombo(playerId);

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
            
        PlayerComboChanged?.Emit(playerId, _comboCounters[playerId]);
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
            
            BattleRecordUpdated?.Emit(playerId, record);
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
        BossDefeated?.Emit(battle.BossConfigId, battle.Config.Name, isFirstBlood, rewards);
        
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

    // 存档支持
    public Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
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
        
        return data;
    }

    public void ImportSaveData(Dictionary data)
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
