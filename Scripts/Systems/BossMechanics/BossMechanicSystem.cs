using System;
using System.Collections.Generic;
using BossMechanicData;

public class BossMechanicSystem
{
    private static BossMechanicSystem _instance;
    public static BossMechanicSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new BossMechanicSystem();
            return _instance;
        }
    }

    // 当前战斗状态
    public bool IsInBossBattle { get; private set; }
    public string CurrentBossId { get; private set; }
    public int CurrentPhaseIndex { get; private set; }
    public float BattleStartTime { get; private set; }
    public float CurrentBossHealth { get; private set; }
    public float MaxBossHealth { get; private set; }
    
    // 战斗统计
    private float _totalDamageDealt;
    private int _skillUses;
    private int _dodges;
    private int _crits;
    private int _maxCombo;
    private int _currentCombo;
    
    // 玩家数据
    private PlayerBossStats _playerStats;
    
    // 信号系统
    public Action<string, int> OnPhaseChanged;
    public Action<string, float> OnHealthChanged;
    public Action<string> OnBossDefeated;
    public Action<string> OnBossEnraged;
    public Action<string, string> OnSkillUsed;
    public Action<string, string> OnBattleStarted;
    public Action<float> OnBattleEnded;

    private BossMechanicSystem()
    {
        _playerStats = new PlayerBossStats
        {
            BossStats = new Dictionary<string, BossBattleStats>()
        };
    }

    public void Initialize()
    {
        BossMechanicDatabase.Instance.InitializeBosses();
    }

    #region 战斗控制

    /// <summary>
    /// 开始 Boss 战斗
    /// </summary>
    public bool StartBossBattle(string bossId)
    {
        var config = BossMechanicDatabase.Instance.GetBossConfig(bossId);
        if (config == null) return false;

        IsInBossBattle = true;
        CurrentBossId = bossId;
        CurrentPhaseIndex = 0;
        BattleStartTime = UnityEngine.Time.time;
        MaxBossHealth = config.BaseHealth;
        CurrentBossHealth = MaxBossHealth;
        
        _totalDamageDealt = 0;
        _skillUses = 0;
        _dodges = 0;
        _crits = 0;
        _maxCombo = 0;
        _currentCombo = 0;
        
        OnBattleStarted?.Invoke(bossId, CurrentPhaseIndex);
        
        // 通知 DynamicDifficultySystem 如果存在
        var dda = DynamicDifficultySystem.Instance;
        if (dda != null)
        {
            dda.RecordBossBattleStart(bossId);
        }
        
        return true;
    }

    /// <summary>
    /// 对 Boss 造成伤害
    /// </summary>
    public void DealDamageToBoss(float damage, bool isCritical = false)
    {
        if (!IsInBossBattle) return;

        var config = BossMechanicDatabase.Instance.GetBossConfig(CurrentBossId);
        if (config == null) return;

        // 获取当前阶段属性
        var phase = GetCurrentPhase();
        float actualDamage = damage * GetPhaseAttackMultiplier(phase);
        
        // 应用防御
        actualDamage = Math.Max(1, actualDamage - config.BaseDefense * GetPhaseDefenseMultiplier(phase));
        
        CurrentBossHealth -= actualDamage;
        _totalDamageDealt += actualDamage;
        
        if (isCritical)
        {
            _crits++;
            _currentCombo++;
            if (_currentCombo > _maxCombo) _maxCombo = _currentCombo;
        }
        
        // 检查阶段转换
        CheckPhaseTransition();
        
        // 通知血量变化
        OnHealthChanged?.Invoke(CurrentBossId, CurrentBossHealth / MaxBossHealth);
        
        // 检查 Boss 是否死亡
        if (CurrentBossHealth <= 0)
        {
            EndBossBattle(true);
        }
    }

    /// <summary>
    /// Boss 攻击玩家
    /// </summary>
    public float OnBossAttack(float baseDamage)
    {
        if (!IsInBossBattle) return 0;

        var config = BossMechanicDatabase.Instance.GetBossConfig(CurrentBossId);
        if (config == null) return 0;

        var phase = GetCurrentPhase();
        float damage = baseDamage * GetPhaseAttackMultiplier(phase);
        
        return damage;
    }

    /// <summary>
    /// 玩家闪避成功
    /// </summary>
    public void OnPlayerDodge()
    {
        if (!IsInBossBattle) return;
        _dodges++;
        _currentCombo = 0;
    }

    /// <summary>
    /// 玩家使用技能
    /// </summary>
    public void OnSkillUsedByPlayer(string skillId)
    {
        if (!IsInBossBattle) return;
        _skillUses++;
        OnSkillUsed?.Invoke(CurrentBossId, skillId);
    }

    /// <summary>
    /// 结束 Boss 战斗
    /// </summary>
    public void EndBossBattle(bool victory)
    {
        if (!IsInBossBattle) return;

        float battleTime = UnityEngine.Time.time - BattleStartTime;
        
        // 更新玩家统计
        UpdatePlayerStats(victory, battleTime);
        
        // 通知 DynamicDifficultySystem
        var dda = DynamicDifficultySystem.Instance;
        if (dda != null)
        {
            dda.RecordBossBattleEnd(victory, battleTime, _totalDamageDealt);
        }
        
        OnBattleEnded?.Invoke(battleTime);
        
        if (victory)
        {
            OnBossDefeated?.Invoke(CurrentBossId);
            
            // 计算战斗评价
            string rating = CalculateBattleRating();
            
            // 发放奖励
            GrantBattleRewards(victory, rating);
        }
        
        // 重置战斗状态
        IsInBossBattle = false; 
        CurrentBossId = null;
        CurrentPhaseIndex = 0;
    }

    #endregion

    #region 阶段管理

    private void CheckPhaseTransition()
    {
        if (!IsInBossBattle) return;

        var config = BossMechanicDatabase.Instance.GetBossConfig(CurrentBossId);
        if (config == null || config.Phases == null || config.Phases.Count == 0) return;

        float healthPercent = CurrentBossHealth / MaxBossHealth;
        
        for (int i = CurrentPhaseIndex + 1; i < config.Phases.Count; i++)
        {
            var phase = config.Phases[i];
            
            bool shouldTransition = false; 
            
            switch (phase.TriggerType)
            {
                case PhaseTriggerType.HealthPercent:
                    shouldTransition = healthPercent <= phase.HealthThreshold;
                    break;
                case PhaseTriggerType.TimeElapsed:
                    float elapsed = UnityEngine.Time.time - BattleStartTime;
                    shouldTransition = elapsed >= phase.TimeThreshold;
                    break;
                case PhaseTriggerType.DamageDealt:
                    shouldTransition = _totalDamageDealt >= phase.DamageThreshold;
                    break;
            }
            
            if (shouldTransition)
            {
                CurrentPhaseIndex = i;
                OnPhaseChanged?.Invoke(CurrentBossId, i);
                
                // 狂暴通知
                if (phase.PhaseType == BossPhaseType.Enraged || 
                    phase.PhaseType == BossPhaseType.Frenzy)
                {
                    OnBossEnraged?.Invoke(CurrentBossId);
                }
            }
        }
    }

    public BossPhase GetCurrentPhase()
    {
        if (!IsInBossBattle) return null;
        
        var config = BossMechanicDatabase.Instance.GetBossConfig(CurrentBossId);
        if (config == null || config.Phases == null) return null;
        
        if (CurrentPhaseIndex >= 0 && CurrentPhaseIndex < config.Phases.Count)
            return config.Phases[CurrentPhaseIndex];
        
        return null;
    }

    public float GetPhaseAttackMultiplier(BossPhase phase)
    {
        return phase != null ? phase.AttackMultiplier : 1.0f;
    }

    public float GetPhaseDefenseMultiplier(BossPhase phase)
    {
        return phase != null ? phase.DefenseMultiplier : 1.0f;
    }

    #endregion

    #region 技能管理

    public BossSkill GetRandomSkill()
    {
        if (!IsInBossBattle) return null;
        
        var config = BossMechanicDatabase.Instance.GetBossConfig(CurrentBossId);
        if (config == null || config.Skills == null || config.Skills.Count == 0) return null;

        // 根据技能优先级随机选择
        float totalPriority = 0;
        foreach (var skill in config.Skills)
        {
            totalPriority += skill.Priority;
        }

        float random = UnityEngine.Random.Range(0, totalPriority);
        float cumulative = 0;
        
        foreach (var skill in config.Skills)
        {
            cumulative += skill.Priority;
            if (random <= cumulative)
                return skill;
        }

        return config.Skills[0];
    }

    public List<BossSkill> GetAvailableSkills()
    {
        if (!IsInBossBattle) return new List<BossSkill>();
        
        var config = BossMechanicDatabase.Instance.GetBossConfig(CurrentBossId);
        if (config == null || config.Skills == null) return new List<BossSkill>();
        
        return config.Skills;
    }

    #endregion

    #region 统计与评价

    private void UpdatePlayerStats(bool victory, float battleTime)
    {
        if (CurrentBossId == null) return;

        // 获取或创建该 Boss 的统计
        if (!_playerStats.BossStats.ContainsKey(CurrentBossId))
        {
            _playerStats.BossStats[CurrentBossId] = new BossBattleStats
            {
                BossId = CurrentBossId
            };
        }

        var stats = _playerStats.BossStats[CurrentBossId];
        
        stats.TotalBattles++;
        if (victory)
        {
            stats.Victories++;
            _playerStats.ConsecutiveWins++;
            _playerStats.ConsecutiveLosses = 0;
               {
            stats.Defeats++;
 }
        else
            _playerStats.ConsecutiveLosses++;
            _playerStats.ConsecutiveWins = 0;
        }

        stats.TotalDamageDealt += (int)_totalDamageDealt;
        
        // 更新时间统计
        if (victory && (stats.BestTime == 0 || battleTime < stats.BestTime))
            stats.BestTime = battleTime;
        
        stats.AverageTime = (stats.AverageTime * (stats.TotalBattles - 1) + battleTime) / stats.TotalBattles;

        // 阶段统计
        if (CurrentPhaseIndex >= 1) stats.TimesReachedPhase2++;
        if (CurrentPhaseIndex >= 2) stats.TimesReachedPhase3++;
        if (CurrentPhaseIndex >= 3) stats.TimesReachedPhase4++;

        // 总体统计
        _playerStats.TotalBossBattles++;
        if (victory) _playerStats.TotalBossesDefeated++;
        _playerStats.TotalBattleTime += battleTime;
        _playerStats.TotalDamageDealt += (int)_totalDamageDealt;

        // 最佳连击
        if (_maxCombo > stats.BestCombo) stats.BestCombo = _maxCombo;
    }

    public string CalculateBattleRating()
    {
        if (!IsInBossBattle) return "N/A";

        float healthPercent = CurrentBossHealth / MaxBossHealth;
        float battleTime = UnityEngine.Time.time - BattleStartTime;
        var config = BossMechanicDatabase.Instance.GetBossConfig(CurrentBossId);
        
        int score = 0;
        
        // 无伤加成
        if (_dodges > 0) score += 10;
        
        // 暴击加成
        score += _crits * 5;
        
        // 速度加成
        float expectedTime = config.BaseHealth / (config.BaseAttack * 2);
        if (battleTime < expectedTime * 0.5f) score += 30;
        else if (battleTime < expectedTime) score += 20;
        else if (battleTime < expectedTime * 1.5f) score += 10;
        
        // 阶段加成
        score += CurrentPhaseIndex * 15;
        
        // 剩余血量加成
        if (healthPercent > 0.5f) score += 20;
        else if (healthPercent > 0.25f) score += 10;

        // 技能使用效率
        if (_skillUses > 0)
        {
            float dps = _totalDamageDealt / battleTime;
            float skillDps = (_totalDamageDealt / _skillUses);
            if (skillDps > config.BaseAttack * 2) score += 15;
        }

        // 评级
        if (score >= 90) return "S";
        if (score >= 70) return "A";
        if (score >= 50) return "B";
        if (score >= 30) return "C";
        return "D";
    }

    private void GrantBattleRewards(bool victory, string rating)
    {
        if (CurrentBossId == null) return;

        var config = BossMechanicDatabase.Instance.GetBossConfig(CurrentBossId);
        if (config == null) return;

        // 基础奖励
        int baseGold = (int)(config.BaseHealth * 0.1f);
        int baseExp = (int)(config.BaseHealth * 0.15f);

        // 评级加成
        float ratingMultiplier = 1.0f;
        switch (rating)
        {
            case "S": ratingMultiplier = 2.0f; break;
            case "A": ratingMultiplier = 1.5f; break;
            case "B": ratingMultiplier = 1.2f; break;
            case "C": ratingMultiplier = 1.0f; break;
            case "D": ratingMultiplier = 0.8f; break;
        }

        // 阶段加成
        float phaseMultiplier = 1.0f + (CurrentPhaseIndex * 0.2f);

        // 计算最终奖励
        int totalGold = (int)(baseGold * ratingMultiplier * phaseMultiplier * config.LootBonusMultiplier);
        int totalExp = (int)(baseExp * ratingMultiplier * phaseMultiplier * config.LootBonusMultiplier);

        // 发放奖励
        var player = Player.Instance;
        if (player != null)
        {
            player.AddGold(totalGold);
            player.AddExp(totalExp);
            
            // 通知 UI
            NotificationManager.Instance?.ShowNotification(
                $"Boss 战斗胜利!\n评级: {rating}\n金币: +{totalGold}\n经验: +{totalExp}",
                NotificationManager.NotificationType.Reward
            );
        }

        // 检查是否有稀有掉落
        CheckRareDrops();
    }

    private void CheckRareDrops()
    {
        if (CurrentBossId == null) return;

        var config = BossMechanicDatabase.Instance.GetBossConfig(CurrentBossId);
        if (config == null) return;

        // 使用战利品系统检查掉落
        var lootSystem = LootDropSystem.Instance;
        if (lootSystem != null)
        {
            // Boss 掉落
            var drop = lootSystem.RollLoot("boss_drop");
            if (drop != null)
            {
                InventoryManager.Instance.AddItem(drop);
                NotificationManager.Instance?.ShowNotification(
                    $"稀有掉落: {drop.Name}!",
                    NotificationManager.NotificationType.Loot
                );
            }
        }
    }

    #endregion

    #region 玩家统计查询

    public PlayerBossStats GetPlayerStats()
    {
        return _playerStats;
    }

    public BossBattleStats GetBossStats(string bossId)
    {
        if (_playerStats.BossStats.ContainsKey(bossId))
            return _playerStats.BossStats[bossId];
        return null;
    }

    public int GetTotalBossesDefeated()
    {
        return _playerStats.TotalBossesDefeated;
    }

    public float GetAverageBattleTime()
    {
        if (_playerStats.TotalBossBattles == 0) return 0;
        return _playerStats.TotalBattleTime / _playerStats.TotalBossBattles;
    }

    public int GetWinStreak()
    {
        return _playerStats.ConsecutiveWins;
    }

    #endregion

    #region 存档支持

    public Dictionary<string, object> GetSaveData()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        
        // 玩家 Boss 统计
        List<Dictionary<string, object>> bossStatsList = new List<Dictionary<string, object>>();
        foreach (var kvp in _playerStats.BossStats)
        {
            bossStatsList.Add(new Dictionary<string, object>
            {
                { "boss_id", kvp.Key },
                { "total_battles", kvp.Value.TotalBattles },
                { "victories", kvp.Value.Victories },
                { "defeats", kvp.Value.Defeats },
                { "best_time", kvp.Value.BestTime },
                { "average_time", kvp.Value.AverageTime },
                { "total_damage", kvp.Value.TotalDamageDealt },
                { "best_combo", kvp.Value.BestCombo }
            });
        }
        
        data["boss_stats"] = bossStatsList;
        data["total_bosses_defeated"] = _playerStats.TotalBossesDefeated;
        data["total_boss_battles"] = _playerStats.TotalBossBattles;
        data["consecutive_wins"] = _playerStats.ConsecutiveWins;
        data["consecutive_losses"] = _playerStats.ConsecutiveLosses;
        
        return data;
    }

    public void LoadSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        if (data.ContainsKey("boss_stats"))
        {
            var bossStatsList = data["boss_stats"] as List<object>;
            if (bossStatsList != null)
            {
                foreach (var statsData in bossStatsList)
                {
                    var dict = statsData as Dictionary<string, object>;
                    if (dict != null && dict.ContainsKey("boss_id"))
                    {
                        string bossId = dict["boss_id"].ToString();
                        var stats = new BossBattleStats
                        {
                            BossId = bossId,
                            TotalBattles = Convert.ToInt32(dict["total_battles"]),
                            Victories = Convert.ToInt32(dict["victories"]),
                            Defeats = Convert.ToInt32(dict["defeats"]),
                            BestTime = Convert.ToSingle(dict["best_time"]),
                            AverageTime = Convert.ToSingle(dict["average_time"]),
                            TotalDamageDealt = Convert.ToInt32(dict["total_damage"]),
                            BestCombo = Convert.ToInt32(dict["best_combo"])
                        };
                        _playerStats.BossStats[bossId] = stats;
                    }
                }
            }
        }

        if (data.ContainsKey("total_bosses_defeated"))
            _playerStats.TotalBossesDefeated = Convert.ToInt32(data["total_bosses_defeated"]);
        if (data.ContainsKey("total_boss_battles"))
            _playerStats.TotalBossBattles = Convert.ToInt32(data["total_boss_battles"]);
        if (data.ContainsKey("consecutive_wins"))
            _playerStats.ConsecutiveWins = Convert.ToInt32(data["consecutive_wins"]);
        if (data.ContainsKey("consecutive_losses"))
            _playerStats.ConsecutiveLosses = Convert.ToInt32(data["consecutive_losses"]);
    }

    #endregion
}
