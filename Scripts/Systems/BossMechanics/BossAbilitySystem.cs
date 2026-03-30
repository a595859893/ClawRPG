using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Boss技能系统 - 负责Boss技能/机制（伤害、治疗、护盾、召唤等）
/// </summary>
public class BossAbilitySystem : BaseSystem
{
    public static BossAbilitySystem Instance { get; private set; }

    // 信号 - 技能相关 (C# Action 委托)
    public static Action<string, string, string> BossSkillInitiated;
    public static Action<string, string, string> BossSkillExecuted;
    public static Action<string, string> BossSkillCompleted;
    public static Action<string, float> BossHealed;
    public static Action<string, float> BossShielded;
    public static Action<string, List<string>> MonstersSummoned;

    private Random _random = new Random();

    // ========== 系统内部状态 (用于 SaveData) ==========
    // 当前关联的 BossBattleInstance 引用
    private BossBattleInstance _currentBattle;
    // 激活技能列表 (技能ID)
    private List<string> _activatedSkills = new List<string>();
    // 召唤的小怪ID列表
    private List<string> _summonedMonsters = new List<string>();

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// 获取或设置当前关联的 BossBattleInstance
    /// </summary>
    public BossBattleInstance CurrentBattle
    {
        get => _currentBattle;
        set => _currentBattle = value;
    }

    /// <summary>
    /// 初始化技能系统
    /// </summary>
    public void InitializeAbilities(BossBattleInstance battle)
    {
        _currentBattle = battle;
        battle.SkillCooldowns.Clear();
        battle.ActiveEffects.Clear();
        _activatedSkills.Clear();
        _summonedMonsters.Clear();

        // 初始化技能冷却
        if (battle.Config.Skills != null)
        {
            foreach (var skill in battle.Config.Skills)
            {
                battle.SkillCooldowns[skill.Id] = _random.Next(0, (int)skill.Cooldown);
            }
        }
    }

    /// <summary>
    /// 更新技能冷却（每帧调用）
    /// </summary>
    public void UpdateAbilities(BossBattleInstance battle, float delta)
    {
        if (!battle.IsAlive) return;

        battle.TimeSinceLastSkill += delta;

        // 更新技能冷却
        foreach (var skillCooldown in battle.SkillCooldowns)
        {
            battle.SkillCooldowns[skillCooldown.Key] = Mathf.Max(0, skillCooldown.Value - delta);
        }
    }

    /// <summary>
    /// 选择可用技能
    /// </summary>
    public BossSkillConfig SelectSkill(BossBattleInstance battle)
    {
        if (battle.Config.Skills == null || battle.Config.Skills.Count == 0)
            return null;

        List<BossSkillConfig> availableSkills = new List<BossSkillConfig>();
        
        foreach (var skill in battle.Config.Skills)
        {
            // 检查冷却
            if (battle.SkillCooldowns.ContainsKey(skill.Id) && battle.SkillCooldowns[skill.Id] > 0)
                continue;
                
            // 检查狂暴状态限制
            if (skill.IsEnragedOnly && !battle.IsEnraged)
                continue;
                
            // 检查阶段要求
            if (skill.PhaseRequired > battle.CurrentPhase)
                continue;
                
            // 检查执行概率
            if (_random.NextDouble() > skill.ExecuteProbability)
                continue;
                
            availableSkills.Add(skill);
        }
        
        if (availableSkills.Count == 0)
            return null;
            
        return availableSkills[_random.Next(availableSkills.Count)];
    }

    /// <summary>
    /// 执行Boss技能
    /// </summary>
    public void ExecuteSkill(BossBattleInstance battle, BossSkillConfig skill)
    {
        if (skill == null || battle == null) return;

        // 设置冷却
        battle.SkillCooldowns[skill.Id] = skill.Cooldown;
        battle.TimeSinceLastSkill = 0;

        // 跟踪激活的技能
        if (!_activatedSkills.Contains(skill.Id))
        {
            _activatedSkills.Add(skill.Id);
        }

        // 发出技能开始信号
        BossSkillInitiated?.Invoke(battle.InstanceId, skill.Id, skill.Name);

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
                
            case BossSkillType.Charge:
                PerformCharge(battle, skill);
                break;
                
            case BossSkillType.SpinAttack:
                ApplySpinAttack(battle, skill);
                break;
                
            case BossSkillType.LaserBeam:
                ApplyLaserBeam(battle, skill);
                break;
                
            case BossSkillType.Enrage:
                ApplyEnrageEffect(battle, skill);
                break;
        }

        // 发出技能执行完成信号
        BossSkillExecuted?.Invoke(battle.InstanceId, skill.Id, skill.Name);
        BossSkillCompleted?.Invoke(battle.InstanceId, skill.Id);
    }

    /// <summary>
    /// 应用直接伤害
    /// </summary>
    public float ApplyDirectDamage(BossBattleInstance battle, BossSkillConfig skill)
    {
        float damage = skill.Damage * battle.CurrentDamageMultiplier;
        
        if (_random.NextDouble() < battle.Config.CriticalChance)
        {
            damage *= battle.Config.CriticalDamage;
        }
        
        // 返回实际伤害值，供外部使用
        return damage;
    }

    /// <summary>
    /// 应用范围伤害
    /// </summary>
    public float ApplyAreaDamage(BossBattleInstance battle, BossSkillConfig skill)
    {
        float damage = skill.Damage * battle.CurrentDamageMultiplier;
        
        if (_random.NextDouble() < battle.Config.CriticalChance)
        {
            damage *= battle.Config.CriticalDamage;
        }
        
        // 范围信息可以通过技能配置的AreaRadius获取
        return damage;
    }

    /// <summary>
    /// 召唤怪物
    /// </summary>
    public void SummonMonsters(BossBattleInstance battle, BossSkillConfig skill)
    {
        if (string.IsNullOrEmpty(skill.SummonMonsterId)) return;

        List<string> summonedIds = new List<string>();

        for (int i = 0; i < skill.SummonCount; i++)
        {
            string summonId = $"{skill.SummonMonsterId}_{battle.InstanceId}_{i}";
            battle.SummonedMonsters.Add(summonId);
            // 同步到系统内部状态
            if (!_summonedMonsters.Contains(summonId))
            {
                _summonedMonsters.Add(summonId);
            }
            summonedIds.Add(summonId);
        }

        MonstersSummoned?.Invoke(battle.InstanceId, summonedIds);
    }

    /// <summary>
    /// 自我治疗
    /// </summary>
    public void ApplySelfHeal(BossBattleInstance battle, BossSkillConfig skill)
    {
        float healAmount = skill.HealAmount;
        float oldHealth = battle.CurrentHealth;
        
        battle.CurrentHealth = Mathf.Min(battle.Config.MaxHealth, battle.CurrentHealth + healAmount);
        
        float actualHeal = battle.CurrentHealth - oldHealth;
        if (actualHeal > 0)
        {
            BossHealed?.Invoke(battle.InstanceId, actualHeal);
        }
    }

    /// <summary>
    /// 应用护盾
    /// </summary>
    public void ApplyShield(BossBattleInstance battle, BossSkillConfig skill)
    {
        string shieldEffect = $"shield_{skill.ShieldAmount}";
        
        // 移除旧护盾，添加新护盾
        battle.ActiveEffects.RemoveAll(e => e.StartsWith("shield_"));
        battle.ActiveEffects.Add(shieldEffect);
        
        BossShielded?.Invoke(battle.InstanceId, skill.ShieldAmount);
    }

    /// <summary>
    /// 应用Debuff
    /// </summary>
    public void ApplyDebuff(BossBattleInstance battle, BossSkillConfig skill)
    {
        if (skill.DebuffIds == null) return;
        
        foreach (var debuffId in skill.DebuffIds)
        {
            battle.ActiveEffects.Add(debuffId);
        }
    }

    /// <summary>
    /// 传送
    /// </summary>
    public void PerformTeleport(BossBattleInstance battle)
    {
        battle.ActiveEffects.Add("teleporting");
        // 实际的传送逻辑由其他系统处理
    }

    /// <summary>
    /// 眩晕
    /// </summary>
    public void ApplyStun(BossBattleInstance battle, BossSkillConfig skill)
    {
        battle.ActiveEffects.Add($"stun_{skill.StunDuration}");
    }

    /// <summary>
    /// 击退
    /// </summary>
    public void ApplyKnockback(BossBattleInstance battle, BossSkillConfig skill)
    {
        battle.ActiveEffects.Add($"knockback_{skill.KnockbackForce}");
    }

    /// <summary>
    /// 冲锋
    /// </summary>
    public void PerformCharge(BossBattleInstance battle, BossSkillConfig skill)
    {
        battle.ActiveEffects.Add("charging");
    }

    /// <summary>
    /// 旋转攻击
    /// </summary>
    public void ApplySpinAttack(BossBattleInstance battle, BossSkillConfig skill)
    {
        float damage = skill.Damage * battle.CurrentDamageMultiplier;
        battle.ActiveEffects.Add("spin_attack");
    }

    /// <summary>
    /// 激光束
    /// </summary>
    public void ApplyLaserBeam(BossBattleInstance battle, BossSkillConfig skill)
    {
        battle.ActiveEffects.Add("laser_beam");
    }

    /// <summary>
    /// 狂暴效果
    /// </summary>
    public void ApplyEnrageEffect(BossBattleInstance battle, BossSkillConfig skill)
    {
        // 狂暴效果由PhaseSystem处理
        if (!battle.IsEnraged)
        {
            BossPhaseSystem.Instance.TriggerEnrage(battle);
        }
    }

    /// <summary>
    /// 检查技能是否可用
    /// </summary>
    public bool IsSkillReady(BossBattleInstance battle, string skillId)
    {
        if (!battle.SkillCooldowns.ContainsKey(skillId))
            return false;
            
        return battle.SkillCooldowns[skillId] <= 0;
    }

    /// <summary>
    /// 获取技能剩余冷却时间
    /// </summary>
    public float GetSkillCooldown(BossBattleInstance battle, string skillId)
    {
        if (!battle.SkillCooldowns.ContainsKey(skillId))
            return 0;
            
        return battle.SkillCooldowns[skillId];
    }

    /// <summary>
    /// 查找技能配置
    /// </summary>
    public BossSkillConfig FindSkill(BossBattleInstance battle, string skillId)
    {
        if (battle.Config.Skills == null) return null;
        
        foreach (var skill in battle.Config.Skills)
        {
            if (skill.Id == skillId)
                return skill;
        }
        return null;
    }

    /// <summary>
    /// 应用伤害减免（护盾）
    /// </summary>
    public float ApplyShieldReduction(BossBattleInstance battle, float damage)
    {
        float remainingDamage = damage;
        
        for (int i = battle.ActiveEffects.Count - 1; i >= 0; i--)
        {
            if (battle.ActiveEffects[i].StartsWith("shield_"))
            {
                float shieldAmount = float.Parse(battle.ActiveEffects[i].Split('_')[1]);
                if (shieldAmount >= remainingDamage)
                {
                    battle.ActiveEffects[i] = $"shield_{shieldAmount - remainingDamage}";
                    remainingDamage = 0;
                }
                else
                {
                    remainingDamage -= shieldAmount;
                    battle.ActiveEffects.RemoveAt(i);
                }
                break;
            }
        }
        
        return remainingDamage;
    }

    /// <summary>
    /// 导出技能系统数据 (Override基类无参方法)
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();

        // 导出激活的技能列表
        var activatedArray = new Godot.Collections.Array();
        foreach (var skillId in _activatedSkills)
        {
            activatedArray.Add(skillId);
        }
        data["activatedSkills"] = activatedArray;

        // 导出召唤的小怪
        var summonArray = new Godot.Collections.Array();
        foreach (var monsterId in _summonedMonsters)
        {
            summonArray.Add(monsterId);
        }
        data["summonedMonsters"] = summonArray;

        // 导出技能冷却 (从当前关联的battle)
        if (_currentBattle != null)
        {
            var cooldownArray = new Godot.Collections.Array();
            foreach (var kvp in _currentBattle.SkillCooldowns)
            {
                cooldownArray.Add(new Godot.Collections.Array { kvp.Key, kvp.Value });
            }
            data["skillCooldowns"] = cooldownArray;
        }

        return data;
    }

    /// <summary>
    /// 导入技能系统数据 (Override基类无参方法)
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        // 导入激活的技能列表
        if (data.Contains("activatedSkills"))
        {
            _activatedSkills.Clear();
            var activatedArray = (Godot.Collections.Array)data["activatedSkills"];
            foreach (string skillId in activatedArray)
            {
                _activatedSkills.Add(skillId);
            }
        }

        // 导入召唤的小怪
        if (data.Contains("summonedMonsters"))
        {
            _summonedMonsters.Clear();
            var summonArray = (Godot.Collections.Array)data["summonedMonsters"];
            foreach (string monsterId in summonArray)
            {
                _summonedMonsters.Add(monsterId);
            }
        }

        // 导入技能冷却 (到当前关联的battle)
        if (_currentBattle != null && data.Contains("skillCooldowns"))
        {
            _currentBattle.SkillCooldowns.Clear();
            var cooldownArray = (Godot.Collections.Array)data["skillCooldowns"];
            foreach (Godot.Collections.Array entry in cooldownArray)
            {
                _currentBattle.SkillCooldowns[(string)entry[0]] = (float)entry[1];
            }
        }
    }

    /*
    /// <summary>
    /// 导出技能系统数据 (旧方法，保留以兼容)
    /// </summary>
    public Dictionary ExportSaveData(BossBattleInstance battle)
    {
        var data = new Dictionary<string, object>();

        if (battle != null)
        {
            // 导出技能冷却
            var cooldownArray = new Godot.Collections.Array();
            foreach (var kvp in battle.SkillCooldowns)
            {
                cooldownArray.Add(new Godot.Collections.Array { kvp.Key, kvp.Value });
            }
            data["skillCooldowns"] = cooldownArray;

            // 导出召唤的小怪
            var summonArray = new Godot.Collections.Array();
            foreach (var monsterId in battle.SummonedMonsters)
            {
                summonArray.Add(monsterId);
            }
            data["summonedMonsters"] = summonArray;
        }

        return data;
    }

    /// <summary>
    /// 导入技能系统数据 (旧方法，保留以兼容)
    /// </summary>
    public void ImportSaveData(BossBattleInstance battle, Dictionary data)
    {
        if (battle == null || data == null) return;

        // 导入技能冷却
        if (data.Contains("skillCooldowns"))
        {
            battle.SkillCooldowns.Clear();
            var cooldownArray = (Godot.Collections.Array)data["skillCooldowns"];
            foreach (Godot.Collections.Array entry in cooldownArray)
            {
                battle.SkillCooldowns[(string)entry[0]] = (float)entry[1];
            }
        }

        // 导入召唤的小怪
        if (data.Contains("summonedMonsters"))
        {
            battle.SummonedMonsters.Clear();
            var summonArray = (Godot.Collections.Array)data["summonedMonsters"];
            foreach (string monsterId in summonArray)
            {
                battle.SummonedMonsters.Add(monsterId);
            }
        }
    }
    */
}
