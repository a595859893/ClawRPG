using System;
using System.Collections.Generic;

public class BossMechanicData
{
    // Boss 阶段类型
    public enum BossPhaseType
    {
        Normal,
        Enraged,
        Frenzy,
        FinalStand,
        Transformation
    }

    // 阶段转换触发条件
    public enum PhaseTriggerType
    {
        HealthPercent,    // 血量百分比触发
        TimeElapsed,      // 时间经过触发
        DamageDealt,      // 累计伤害触发
        Custom            // 自定义条件
    }

    // 技能类型
    public enum SkillType
    {
        MeleeAttack,
        RangedAttack,
        AreaOfEffect,
        Summon,
        Buff,
        Debuff,
        Teleport,
        Charge,
        Beam,
        SelfDestruct
    }

    // Boss 阶段配置
    public class BossPhase
    {
        public string PhaseId { get; set; }
        public string PhaseName { get; set; }
        public BossPhaseType PhaseType { get; set; }
        public float HealthThreshold { get; set; }  // 触发此阶段的血量百分比
        public float TimeThreshold { get; set; }     // 触发此阶段的时间(秒)
        public float DamageThreshold { get; set; }   // 触发此阶段的累计伤害
        public PhaseTriggerType TriggerType { get; set; }
        
        // 属性修改
        public float AttackMultiplier { get; set; } = 1.0f;
        public float DefenseMultiplier { get; set; } = 1.0f;
        public float SpeedMultiplier { get; set; } = 1.0f;
        public float HealthMultiplier { get; set; } = 1.0f;
        
        // 特效
        public string VisualEffect { get; set; }
        public string AudioEffect { get; set; }
        public bool EnableShield { get; set; }
        public float ShieldDuration { get; set; }
    }

    // Boss 技能配置
    public class BossSkill
    {
        public string SkillId { get; set; }
        public string SkillName { get; set; }
        public string Description { get; set; }
        public SkillType Type { get; set; }
        public float BaseDamage { get; set; }
        public float Cooldown { get; set; }
        public float Range { get; set; }
        public float CastTime { get; set; }
        public float AreaRadius { get; set; }
        public string[] Buffs { get; set; }
        public int EnergyCost { get; set; }
        public float Priority { get; set; }  // 技能优先级
    }

    // Boss 配置
    public class BossConfiguration
    {
        public string BossId { get; set; }
        public string BossName { get; set; }
        public string RegionId { get; set; }
        public int RecommendedLevel { get; set; }
        
        // 基础属性
        public float BaseHealth { get; set; }
        public float BaseAttack { get; set; }
        public float BaseDefense { get; set; }
        public float BaseSpeed { get; set; }
        
        // 阶段配置
        public List<BossPhase> Phases { get; set; }
        
        // 技能配置
        public List<BossSkill> Skills { get; set; }
        
        // 战斗参数
        public float EnrageTimer { get; set; }      // 狂暴时间(秒)
        public float EnrageDamageMultiplier { get; set; }  // 狂暴后伤害倍数
        public int MaxSummons { get; set; }         // 最大召唤数量
        public float LootBonusMultiplier { get; set; }   // 战利品奖励倍数
        
        // AI 参数
        public float AggressionRadius { get; set; }
        public float RetreatThreshold { get; set; }  // 撤退血量阈值
        public float HealPercent { get; set; }       // 战斗中回复百分比
    }

    // 玩家战斗统计
    public class BossBattleStats
    {
        public string BossId { get; set; }
        public int TotalBattles { get; set; }
        public int Victories { get; set; }
        public int Defeats { get; set; }
        public float BestTime { get; set; }         // 最短击杀时间
        public float AverageTime { get; set; }      // 平均击杀时间
        public int TotalDamageDealt { get; set; }
        public int TotalDamageTaken { get; set; }
        public int TimesReachedPhase2 { get; set; }
        public int TimesReachedPhase3 { get; set; }
        public int TimesReachedPhase4 { get; set; }
        public int BestCombo { get; set; }
        public int MaxSimultaneousKills { get; set; }
    }

    // 玩家总体 Boss 统计
    public class PlayerBossStats
    {
        public int TotalBossesDefeated { get; set; }
        public int TotalBossBattles { get; set; }
        public Dictionary<string, BossBattleStats> BossStats { get; set; }
        public int ConsecutiveWins { get; set; }
        public int ConsecutiveLosses { get; set; }
        public float TotalBattleTime { get; set; }
        public int TotalDamageDealt { get; set; }
    }
}
