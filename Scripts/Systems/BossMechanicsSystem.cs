using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Boss机制系统 - 管理Boss战斗机制
/// 包含Boss阶段切换、技能使用、狂暴机制、召唤小怪等
/// </summary>
public class BossMechanicsSystem : BaseSystem
{
    public static BossMechanicsSystem Instance { get; private set; }
    
    public event Action<BossBattleState> OnPhaseChanged;
    public event Action<BossBattleState, string> OnSkillUsed;
    public event Action<BossBattleState> OnEnrageTriggered;
    public event Action<BossBattleState> OnMinionSpawned;
    public event Action<BossBattleState> OnBossDefeated;
    
    private BossMechanicsDatabase _database;
    private BossMechanicsStats _stats = new BossMechanicsStats();
    private Dictionary<string, BossBattleState> _activeBattles = new Dictionary<string, BossBattleState>();
    
    private Random _random = new Random();
    
    public BossMechanicsStats Stats => _stats;
    
    public override void _Ready()
    {
        Instance = this;
        _database = BossMechanicsDatabase.Instance;
    }
    
    /// <summary>
    /// 系统名称
    /// </summary>
    protected override string SystemName => "BossMechanics";
    
    public void StartBossBattle(string bossId)
    {
        var bossData = _database.GetBoss(bossId);
        if (bossData == null)
        {
            GD.Print($"[BossMechanics] Boss not found: {bossId}");
            return;
        }
        
        var state = new BossBattleState
        {
            BossId = bossId,
            CurrentPhase = 0,
            CurrentHealth = bossData.MaxHealth,
            MaxHealth = bossData.MaxHealth,
            BattleTime = 0f,
            IsEnraged = false,
            ActiveMinionCount = 0
        };
        
        foreach (var skill in bossData.Skills)
        {
            state.SkillCooldowns[skill.SkillId] = 0f;
            state.SkillsUsed[skill.SkillId] = 0;
        }
        
        _activeBattles[bossId] = state;
        
        GD.Print($"[BossMechanics] Started boss battle: {bossData.BossName} (HP: {bossData.MaxHealth})");
    }
    
    public void UpdateBossBattle(string bossId, float delta)
    {
        if (!_activeBattles.ContainsKey(bossId)) return;
        
        var state = _activeBattles[bossId];
        var bossData = _database.GetBoss(bossId);
        if (bossData == null) return;
        
        state.BattleTime += delta;
        
        // Update skill cooldowns
        foreach (var skill in bossData.Skills)
        {
            if (state.SkillCooldowns.ContainsKey(skill.SkillId))
            {
                state.SkillCooldowns[skill.SkillId] = Mathf.Max(0, state.SkillCooldowns[skill.SkillId] - delta);
            }
        }
        
        // Check for phase change
        float healthPercent = state.CurrentHealth / state.MaxHealth;
        for (int i = bossData.Phases.Count - 1; i >= 0; i--)
        {
            var phase = bossData.Phases[i];
            if (healthPercent <= phase.HealthPercentage && state.CurrentPhase < phase.PhaseNumber)
            {
                state.CurrentPhase = phase.PhaseNumber;
                state.PhaseChanged = true;
                _stats.PhasesTriggered++;
                
                OnPhaseChanged?.Invoke(state);
                
                // Spawn minions if specified
                if (phase.SpawnEnemies != null && phase.SpawnEnemies.Count > 0 && bossData.CanSummonMinions)
                {
                    SpawnMinions(bossId, phase.SpawnEnemies, phase.SpawnCount);
                }
                
                GD.Print($"[BossMechanics] Boss {bossData.BossName} entered phase {phase.PhaseNumber}: {phase.PhaseName}");
                break;
            }
        }
        
        // Check for enrage
        if (bossData.HasEnrageMechanic && !state.IsEnraged && state.BattleTime >= bossData.EnrageTime)
        {
            state.IsEnraged = true;
            _stats.EnrageTriggers++;
            
            OnEnrageTriggered?.Invoke(state);
            
            GD.Print($"[BossMechanics] Boss {bossData.BossName} is ENRAGED!");
        }
        
        // Check for minion spawn based on health
        if (bossData.CanSummonMinions && bossData.MinionSpawnHealthPercent > 0)
        {
            if (healthPercent <= bossData.MinionSpawnHealthPercent && state.ActiveMinionCount < bossData.MaxMinionCount)
            {
                if (_random.NextDouble() < 0.01 * delta) // 1% chance per second when in range
                {
                    SpawnMinions(bossId, bossData.MinionTypes, 1);
                }
            }
        }
        
        state.PhaseChanged = false;
    }
    
    public void DamageBoss(string bossId, int damage)
    {
        if (!_activeBattles.ContainsKey(bossId)) return;
        
        var state = _activeBattles[bossId];
        var bossData = _database.GetBoss(bossId);
        
        // Apply weakness multiplier if applicable
        float multiplier = 1.0f;
        // Check if player has weakness element advantage
        
        float actualDamage = damage * multiplier;
        state.CurrentHealth = Mathf.Max(0, state.CurrentHealth - actualDamage);
        state.TotalDamageTaken += (int)actualDamage;
        
        if (state.CurrentHealth <= 0)
        {
            DefeatBoss(bossId);
        }
    }
    
    public void UseSkill(string bossId, string skillId)
    {
        if (!_activeBattles.ContainsKey(bossId)) return;
        
        var state = _activeBattles[bossId];
        var bossData = _database.GetBoss(bossId);
        if (bossData == null) return;
        
        // Find the skill
        BossSkillData skill = null;
        foreach (var s in bossData.Skills)
        {
            if (s.SkillId == skillId)
            {
                skill = s;
                break;
            }
        }
        
        if (skill == null) return;
        
        // Check cooldown
        if (state.SkillCooldowns[skillId] > 0) return;
        
        // Use skill
        state.SkillCooldowns[skillId] = skill.Cooldown;
        state.SkillsUsed[skillId] = state.SkillsUsed.GetValueOrDefault(skillId, 0) + 1;
        
        OnSkillUsed?.Invoke(state, skillId);
        
        GD.Print($"[BossMechanics] Boss {bossData.BossName} used skill: {skill.SkillName}");
    }
    
    public void SpawnMinions(string bossId, string[] minionTypes, int count)
    {
        if (!_activeBattles.ContainsKey(bossId)) return;
        
        var state = _activeBattles[bossId];
        var bossData = _database.GetBoss(bossId);
        
        if (bossData == null || !bossData.CanSummonMinions) return;
        
        int actualCount = Mathf.Min(count, bossData.MaxMinionCount - state.ActiveMinionCount);
        if (actualCount <= 0) return;
        
        state.ActiveMinionCount += actualCount;
        _stats.MinionsSpawned += actualCount;
        
        OnMinionSpawned?.Invoke(state);
        
        GD.Print($"[BossMechanics] Boss {bossData.BossName} summoned {actualCount} minions");
    }
    
    public void MinionDefeated(string bossId)
    {
        if (!_activeBattles.ContainsKey(bossId)) return;
        
        var state = _activeBattles[bossId];
        state.ActiveMinionCount = Mathf.Max(0, state.ActiveMinionCount - 1);
        _stats.MinionsDefeated++;
    }
    
    public void DefeatBoss(string bossId)
    {
        if (!_activeBattles.ContainsKey(bossId)) return;
        
        var state = _activeBattles[bossId];
        var bossData = _database.GetBoss(bossId);
        
        _stats.BossesDefeated++;
        _stats.TotalBattleTime += (int)state.BattleTime;
        _stats.TotalDamageDealt += state.TotalDamageDealt;
        
        if (!_stats.BossKills.ContainsKey(bossId))
            _stats.BossKills[bossId] = 0;
        _stats.BossKills[bossId]++;
        
        if (state.BattleTime < _stats.FastestKillTime)
            _stats.FastestKillTime = (int)state.BattleTime;
        
        // Generate loot
        List<string> loot = GenerateLoot(bossData);
        
        GD.Print($"[BossMechanics] Boss {bossData.BossName} defeated! Loot: {string.Join(", ", loot)}");
        
        OnBossDefeated?.Invoke(state);
        
        // Clean up battle state
        _activeBattles.Remove(bossId);
    }
    
    public void FleeBoss(string bossId)
    {
        if (!_activeBattles.ContainsKey(bossId))
        {
            _stats.BossesFled++;
            _activeBattles.Remove(bossId);
            
            GD.Print($"[BossMechanics] Fled from boss: {bossId}");
        }
    }
    
    private List<string> GenerateLoot(BossMechanicsData bossData)
    {
        List<string> loot = new List<string>();
        
        if (bossData.LootTable == null || bossData.LootTable.Length == 0)
            return loot;
        
        int lootCount = bossData.MinLootCount + _random.Next(bossData.MaxLootCount - bossData.MinLootCount + 1);
        
        for (int i = 0; i < lootCount; i++)
        {
            float roll = (float)_random.NextDouble() * 100f;
            float cumulative = 0f;
            
            for (int j = 0; j < bossData.LootTable.Length && j < bossData.LootWeights.Length; j++)
            {
                cumulative += bossData.LootWeights[j];
                if (roll <= cumulative)
                {
                    loot.Add(bossData.LootTable[j]);
                    break;
                }
            }
        }
        
        return loot;
    }
    
    public BossBattleState GetBattleState(string bossId)
    {
        if (_activeBattles.ContainsKey(bossId))
            return _activeBattles[bossId];
        return null;
    }
    
    public BossPhaseData GetCurrentPhase(string bossId)
    {
        if (!_activeBattles.ContainsKey(bossId)) return null;
        
        var state = _activeBattles[bossId];
        var bossData = _database.GetBoss(bossId);
        
        if (bossData == null || bossData.Phases == null) return null;
        
        int phaseIndex = Mathf.Clamp(state.CurrentPhase - 1, 0, bossData.Phases.Count - 1);
        return bossData.Phases[phaseIndex];
    }
    
    public bool IsBossActive(string bossId)
    {
        return _activeBattles.ContainsKey(bossId);
    }
    
    public List<string> GetAvailableSkills(string bossId)
    {
        List<string> availableSkills = new List<string>();
        
        if (!_activeBattles.ContainsKey(bossId)) return availableSkills;
        
        var state = _activeBattles[bossId];
        var bossData = _database.GetBoss(bossId);
        
        if (bossData == null || bossData.Skills == null) return availableSkills;
        
        foreach (var skill in bossData.Skills)
        {
            if (state.SkillCooldowns[skill.SkillId] <= 0)
            {
                availableSkills.Add(skill.SkillId);
            }
        }
        
        return availableSkills;
    }
    
    public float GetAttackMultiplier(string bossId)
    {
        if (!_activeBattles.ContainsKey(bossId)) return 1.0f;
        
        var state = _activeBattles[bossId];
        var bossData = _database.GetBoss(bossId);
        
        if (bossData == null) return 1.0f;
        
        float multiplier = 1.0f;
        
        // Phase multiplier
        if (state.CurrentPhase > 0 && state.CurrentPhase <= bossData.Phases.Count)
        {
            multiplier *= bossData.Phases[state.CurrentPhase - 1].AttackMultiplier;
        }
        
        // Enrage multiplier
        if (state.IsEnraged && bossData.EnrageTimers.Count > 0)
        {
            multiplier *= bossData.EnrageTimers[0].AttackMultiplier;
        }
        
        return multiplier;
    }
    
    public void SaveData()
    {
        // Save stats to save system
        GD.Print("[BossMechanics] Saving boss mechanics statistics");
    }
    
    public void LoadData()
    {
        // Load stats from save system
        GD.Print("[BossMechanics] Loading boss mechanics statistics");
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        data["defeated_bosses"] = _defeatedBosses;
        data["total_boss_damage"] = _totalBossDamage;
        data["enrage_triggers"] = _enrageTriggers;
        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        if (data.Contains("defeated_bosses")) _defeatedBosses = (int)data["defeated_bosses"];
        if (data.Contains("total_boss_damage")) _totalBossDamage = (int)data["total_boss_damage"];
        if (data.Contains("enrage_triggers")) _enrageTriggers = (int)data["enrage_triggers"];
    }
}
