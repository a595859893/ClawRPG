using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Boss模式系统 - 负责Boss攻击模式、决策逻辑、AI行为
/// </summary>
public class BossPatternSystem : BaseSystem
{
    public static BossPatternSystem Instance { get; private set; }

    // 信号 - 模式相关
    public static Action<string, AttackPattern, AttackPattern> BossPatternChanged;
    public static Action<string, Vector3> BossAttackInitiated;
    public static Action<string, string, string> BossTargetChanged;

    private Random _random = new Random();

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// 初始化Boss模式
    /// </summary>
    public void InitializePattern(BossBattleInstance battle)
    {
        battle.CurrentPattern = battle.Config.DefaultPattern;
        battle.TimeSinceLastAttack = 0;
    }

    /// <summary>
    /// 更新Boss行为（每帧调用）
    /// </summary>
    public void UpdatePattern(BossBattleInstance battle, float delta)
    {
        if (!battle.IsAlive) return;

        // 检查是否眩晕/无法行动
        if (IsBossStunned(battle)) return;

        // 更新攻击间隔
        battle.TimeSinceLastAttack += delta;

        // 执行攻击决策
        MakeAttackDecision(battle, delta);
    }

    /// <summary>
    /// 检查Boss是否眩晕
    /// </summary>
    private bool IsBossStunned(BossBattleInstance battle)
    {
        foreach (var effect in battle.ActiveEffects)
        {
            if (effect.StartsWith("stun_"))
                return true;
        }
        return false;
    }

    /// <summary>
    /// 做出攻击决策
    /// </summary>
    public void MakeAttackDecision(BossBattleInstance battle, float delta)
    {
        // 根据当前模式决定行为
        switch (battle.CurrentPattern)
        {
            case AttackPattern.Aggressive:
                UpdateAggressivePattern(battle, delta);
                break;
                
            case AttackPattern.Defensive:
                UpdateDefensivePattern(battle, delta);
                break;
                
            case AttackPattern.Balanced:
                UpdateBalancedPattern(battle, delta);
                break;
                
            case AttackPattern.Erratic:
                UpdateErraticPattern(battle, delta);
                break;
                
            case AttackPattern.Phased:
                UpdatePhasedPattern(battle, delta);
                break;
                
            case AttackPattern.Enraged:
                UpdateEnragedPattern(battle, delta);
                break;
        }
    }

    /// <summary>
    /// 激进攻击模式 - 快速攻击，优先输出
    /// </summary>
    private void UpdateAggressivePattern(BossBattleInstance battle, float delta)
    {
        float attackInterval = 1.0f / (battle.Config.AttackSpeed * battle.CurrentSpeedMultiplier * 1.5f);
        
        if (battle.TimeSinceLastAttack >= attackInterval)
        {
            PerformBasicAttack(battle);
            battle.TimeSinceLastAttack = 0;
        }
        
        // 激进模式更频繁使用技能
        if (battle.TimeSinceLastSkill >= 2.0f)
        {
            TryUseSkill(battle);
        }
    }

    /// <summary>
    /// 防守反击模式 - 谨慎攻击，优先防御
    /// </summary>
    private void UpdateDefensivePattern(BossBattleInstance battle, float delta)
    {
        float attackInterval = 1.0f / (battle.Config.AttackSpeed * battle.CurrentSpeedMultiplier * 0.7f);
        
        if (battle.TimeSinceLastAttack >= attackInterval)
        {
            PerformBasicAttack(battle);
            battle.TimeSinceLastAttack = 0;
        }
        
        // 防守模式优先使用防御性技能
        if (battle.TimeSinceLastSkill >= 4.0f)
        {
            TryUseDefensiveSkill(battle);
        }
    }

    /// <summary>
    /// 平衡模式 - 攻守兼备
    /// </summary>
    private void UpdateBalancedPattern(BossBattleInstance battle, float delta)
    {
        float attackInterval = 1.0f / (battle.Config.AttackSpeed * battle.CurrentSpeedMultiplier);
        
        if (battle.TimeSinceLastAttack >= attackInterval)
        {
            PerformBasicAttack(battle);
            battle.TimeSinceLastAttack = 0;
        }
        
        // 平衡模式按正常频率使用技能
        if (battle.TimeSinceLastSkill >= 3.0f)
        {
            TryUseSkill(battle);
        }
    }

    /// <summary>
    /// 不稳定模式 - 随机性强
    /// </summary>
    private void UpdateErraticPattern(BossBattleInstance battle, float delta)
    {
        // 随机改变攻击间隔
        float randomFactor = (float)_random.NextDouble() * 2.0f;
        float attackInterval = randomFactor / (battle.Config.AttackSpeed * battle.CurrentSpeedMultiplier);
        
        if (battle.TimeSinceLastAttack >= attackInterval)
        {
            PerformBasicAttack(battle);
            battle.TimeSinceLastAttack = 0;
        }
        
        // 不稳定模式技能使用更随机
        if (battle.TimeSinceLastSkill >= (float)_random.NextDouble() * 5.0f)
        {
            TryUseSkill(battle);
        }
        
        // 随机切换模式
        if (_random.NextDouble() < 0.01f)
        {
            SwitchToRandomPattern(battle);
        }
    }

    /// <summary>
    /// 阶段模式 - 根据阶段改变行为
    /// </summary>
    private void UpdatePhasedPattern(BossBattleInstance battle, float delta)
    {
        float baseInterval = 1.0f / (battle.Config.AttackSpeed * battle.CurrentSpeedMultiplier);
        
        // 阶段越高，攻击越快
        float phaseMultiplier = 1.0f + (battle.CurrentPhase - 1) * 0.2f;
        float attackInterval = baseInterval / phaseMultiplier;
        
        if (battle.TimeSinceLastAttack >= attackInterval)
        {
            PerformBasicAttack(battle);
            battle.TimeSinceLastAttack = 0;
        }
        
        // 每个阶段可能有不同的技能组合
        if (battle.TimeSinceLastSkill >= 3.0f)
        {
            TryUsePhaseSkill(battle);
        }
    }

    /// <summary>
    /// 狂暴模式 - 全方位增强
    /// </summary>
    private void UpdateEnragedPattern(BossBattleInstance battle, float delta)
    {
        // 狂暴模式下攻击大幅加速
        float attackInterval = 1.0f / (battle.Config.AttackSpeed * battle.CurrentSpeedMultiplier * 2.0f);
        
        if (battle.TimeSinceLastAttack >= attackInterval)
        {
            PerformBasicAttack(battle);
            battle.TimeSinceLastAttack = 0;
        }
        
        // 狂暴模式技能使用更频繁
        if (battle.TimeSinceLastSkill >= 1.5f)
        {
            TryUseSkill(battle);
        }
        
        // 狂暴模式可能随机切换到其他模式
        if (_random.NextDouble() < 0.05f)
        {
            battle.CurrentPattern = AttackPattern.Aggressive;
            BossPatternChanged?.Invoke(battle.InstanceId, AttackPattern.Enraged, AttackPattern.Aggressive);
        }
    }

    /// <summary>
    /// 执行普通攻击
    /// </summary>
    public void PerformBasicAttack(BossBattleInstance battle)
    {
        float damage = battle.Config.AttackPower * battle.CurrentDamageMultiplier;
        
        // 暴击判定
        if (_random.NextDouble() < battle.Config.CriticalChance)
        {
            damage *= battle.Config.CriticalDamage;
        }
        
        // 更新目标位置（这里为0，实际由定位系统更新）
        battle.LastTargetPosition = Vector3.Zero;
        
        // 发出攻击信号
        BossAttackInitiated?.Invoke(battle.InstanceId, battle.LastTargetPosition);
    }

    /// <summary>
    /// 尝试使用技能
    /// </summary>
    private void TryUseSkill(BossBattleInstance battle)
    {
        var skill = BossAbilitySystem.Instance.SelectSkill(battle);
        if (skill != null)
        {
            BossAbilitySystem.Instance.ExecuteSkill(battle, skill);
            battle.TimeSinceLastSkill = 0;
        }
    }

    /// <summary>
    /// 尝试使用防御性技能
    /// </summary>
    private void TryUseDefensiveSkill(BossBattleInstance battle)
    {
        // 优先选择防御性技能
        BossSkillConfig defensiveSkill = null;
        
        if (battle.Config.Skills != null)
        {
            foreach (var skill in battle.Config.Skills)
            {
                if (skill.SkillType == BossSkillType.Shield || 
                    skill.SkillType == BossSkillType.Heal)
                {
                    if (BossAbilitySystem.Instance.IsSkillReady(battle, skill.Id))
                    {
                        defensiveSkill = skill;
                        break;
                    }
                }
            }
        }
        
        if (defensiveSkill != null)
        {
            BossAbilitySystem.Instance.ExecuteSkill(battle, defensiveSkill);
        }
        else
        {
            // 如果没有防御技能，则使用普通技能
            TryUseSkill(battle);
        }
        
        battle.TimeSinceLastSkill = 0;
    }

    /// <summary>
    /// 尝试使用当前阶段对应的技能
    /// </summary>
    private void TryUsePhaseSkill(BossBattleInstance battle)
    {
        BossSkillConfig phaseSkill = null;
        
        if (battle.Config.Skills != null)
        {
            foreach (var skill in battle.Config.Skills)
            {
                if (skill.PhaseRequired == battle.CurrentPhase)
                {
                    if (BossAbilitySystem.Instance.IsSkillReady(battle, skill.Id))
                    {
                        phaseSkill = skill;
                        break;
                    }
                }
            }
        }
        
        if (phaseSkill != null)
        {
            BossAbilitySystem.Instance.ExecuteSkill(battle, phaseSkill);
        }
        else
        {
            TryUseSkill(battle);
        }
        
        battle.TimeSinceLastSkill = 0;
    }

    /// <summary>
    /// 切换到随机模式
    /// </summary>
    private void SwitchToRandomPattern(BossBattleInstance battle)
    {
        AttackPattern[] patterns = { AttackPattern.Aggressive, AttackPattern.Defensive, AttackPattern.Balanced };
        AttackPattern newPattern = patterns[_random.Next(patterns.Length)];
        
        if (newPattern != battle.CurrentPattern)
        {
            AttackPattern oldPattern = battle.CurrentPattern;
            battle.CurrentPattern = newPattern;
            BossPatternChanged?.Invoke(battle.InstanceId, oldPattern, newPattern);
        }
    }

    /// <summary>
    /// 切换攻击模式
    /// </summary>
    public void SwitchPattern(BossBattleInstance battle, AttackPattern newPattern)
    {
        if (battle.CurrentPattern != newPattern)
        {
            AttackPattern oldPattern = battle.CurrentPattern;
            battle.CurrentPattern = newPattern;
            BossPatternChanged?.Invoke(battle.InstanceId, oldPattern, newPattern);
        }
    }

    /// <summary>
    /// 根据血量百分比切换模式
    /// </summary>
    public void UpdatePatternByHealth(BossBattleInstance battle)
    {
        float healthPercent = battle.CurrentHealth / battle.Config.MaxHealth;
        
        // 狂暴模式
        if (battle.IsEnraged && battle.CurrentPattern != AttackPattern.Enraged)
        {
            SwitchPattern(battle, AttackPattern.Enraged);
            return;
        }
        
        // 根据血量切换模式
        if (healthPercent > 0.7f)
        {
            // 高血量：激进
            SwitchPattern(battle, AttackPattern.Aggressive);
        }
        else if (healthPercent > 0.3f)
        {
            // 中血量：平衡
            SwitchPattern(battle, AttackPattern.Balanced);
        }
        else
        {
            // 低血量：根据Boss类型决定
            if (battle.Config.Type == BossType.Elite || battle.Config.Type == BossType.Legendary)
            {
                SwitchPattern(battle, AttackPattern.Erratic);
            }
            else
            {
                SwitchPattern(battle, AttackPattern.Aggressive);
            }
        }
    }

    /// <summary>
    /// 评估当前情况并做出决策
    /// </summary>
    public AttackPattern EvaluateAndSelectPattern(BossBattleInstance battle)
    {
        float healthPercent = battle.CurrentHealth / battle.Config.MaxHealth;
        
        // 狂暴状态优先
        if (battle.IsEnraged)
            return AttackPattern.Enraged;
        
        // 阶段模式优先
        if (battle.Config.DefaultPattern == AttackPattern.Phased)
            return AttackPattern.Phased;
        
        // 根据血量和玩家数量决定
        if (battle.TargetsInCombat > 2)
        {
            // 多个目标：偏向防守
            return AttackPattern.Defensive;
        }
        
        if (healthPercent < 0.25f)
        {
            // 危急时刻：不稳定
            return AttackPattern.Erratic;
        }
        
        return battle.Config.DefaultPattern;
    }

    /// <summary>
    /// 获取模式的描述
    /// </summary>
    public string GetPatternDescription(AttackPattern pattern)
    {
        switch (pattern)
        {
            case AttackPattern.Aggressive:
                return "激进攻击：快速攻击，优先输出";
            case AttackPattern.Defensive:
                return "防守反击：谨慎攻击，优先防御";
            case AttackPattern.Balanced:
                return "平衡模式：攻守兼备";
            case AttackPattern.Erratic:
                return "不稳定模式：行为随机";
            case AttackPattern.Phased:
                return "阶段模式：根据阶段改变行为";
            case AttackPattern.Enraged:
                return "狂暴模式：全方位增强";
            default:
                return "未知模式";
        }
    }

    /// <summary>
    /// 导出模式系统数据
    /// </summary>
    public Dictionary ExportSaveData(BossBattleInstance battle)
    {
        var data = new Dictionary();
        
        if (battle != null)
        {
            data["currentPattern"] = (int)battle.CurrentPattern;
            data["timeSinceLastAttack"] = battle.TimeSinceLastAttack;
            data["timeSinceLastSkill"] = battle.TimeSinceLastSkill;
        }
        
        return data;
    }

    /// <summary>
    /// 导入模式系统数据
    /// </summary>
    public void ImportSaveData(BossBattleInstance battle, Dictionary data)
    {
        if (battle == null || data == null) return;
        
        battle.CurrentPattern = (AttackPattern)data.GetValueOrDefault("currentPattern", (int)AttackPattern.Balanced);
        battle.TimeSinceLastAttack = data.GetValueOrDefault("timeSinceLastAttack", 0f);
        battle.TimeSinceLastSkill = data.GetValueOrDefault("timeSinceLastSkill", 0f);
    }
}
