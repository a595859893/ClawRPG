using Godot;
using System;
using System.Collections.Generic;

public enum BossType
{
    Normal,       // 普通Boss
    Elite,        // 精英Boss
    World,        // 世界Boss
    Legendary,    // 传说Boss
    Raid,         // 团本Boss
    Dungeon       // 地下城Boss
}

public enum BossPhase
{
    Idle,         // 闲置
    Intro,        // 开场动画
    Active,       // 战斗中
    Enraged,      // 狂暴阶段
    Transition,   // 阶段转换
    Defeated,     // 已被击败
    Escaped       // 逃跑
}

public enum BossSkillType
{
    MeleeAttack,     // 近战攻击
    RangedAttack,    // 远程攻击
    AreaOfEffect,    // 范围攻击
    Summon,          // 召唤小怪
    Debuff,          // 减益效果
    Heal,            // 治疗
    Shield,          // 护盾
    Teleport,        // 传送
    Charge,          // 冲锋
    SpinAttack,      // 旋转攻击
    LaserBeam,       // 激光束
    Projectile,      // 投射物
    Stun,            // 眩晕
    Knockback,       // 击退
    Enrage           // 狂暴
}

public enum AttackPattern
{
    Aggressive,      // 激进攻击
    Defensive,       // 防守反击
    Balanced,        // 平衡模式
    Erratic,         // 不稳定模式
    Phased,          // 阶段模式
    Enraged          // 狂暴模式
}

public enum DifficultyLevel
{
    Easy,        // 简单
    Normal,      // 普通
    Hard,       // 困难
    Nightmare,   // 噩梦
    Legendary    // 传奇
}

// Boss 技能配置
public class BossSkillConfig
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public BossSkillType SkillType { get; set; }
    public float Damage { get; set; }
    public float AreaRadius { get; set; }
    public float Range { get; set; }
    public float Cooldown { get; set; }
    public float CastTime { get; set; }
    public float Duration { get; set; }
    public float KnockbackForce { get; set; }
    public float StunDuration { get; set; }
    public float HealAmount { get; set; }
    public float ShieldAmount { get; set; }
    public List<string> DebuffIds { get; set; } = new List<string>();
    public string SummonMonsterId { get; set; }
    public int SummonCount { get; set; }
    public bool IsEnragedOnly { get; set; }
    public int PhaseRequired { get; set; }
    public float DamageMultiplier { get; set; } = 1.0f;
    public float ExecuteProbability { get; set; } = 1.0f;
}

// Boss 配置
public class BossConfig
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public BossType Type { get; set; }
    public DifficultyLevel Difficulty { get; set; }
    public float MaxHealth { get; set; }
    public float AttackPower { get; set; }
    public float Defense { get; set; }
    public float MoveSpeed { get; set; }
    public float AttackSpeed { get; set; }
    public float CriticalChance { get; set; }
    public float CriticalDamage { get; set; }
    public int Level { get; set; }
    public int PhaseCount { get; set; }
    public float EnrageThreshold { get; set; }
    public float EnrageTimer { get; set; }
    public float RageThreshold { get; set; } = 0.05f; // HP < 5% triggers rage
    public AttackPattern DefaultPattern { get; set; }
    public List<BossSkillConfig> Skills { get; set; } = new List<BossSkillConfig>();
    public List<DropTableEntry> DropTable { get; set; } = new List<DropTableEntry>();
    public float GoldReward { get; set; }
    public float ExpReward { get; set; }
    public int PointReward { get; set; }
    public string TitleReward { get; set; }
    public float RespawnTime { get; set; }
    public bool IsRaidBoss { get; set; }
    public int RequiredPartySize { get; set; }
}

// Boss 阶段配置
public class BossPhaseConfig
{
    public int PhaseNumber { get; set; }
    public string Name { get; set; }
    public float HealthPercentage { get; set; }
    public float DamageMultiplier { get; set; } = 1.0f;
    public float SpeedMultiplier { get; set; } = 1.0f;
    public bool UnlocksNewSkills { get; set; }
    public List<string> NewSkillIds { get; set; } = new List<string>();
    public bool IsTransitionPhase { get; set; }
    public float TransitionDuration { get; set; }
    public string PhaseEffect { get; set; }
}

// 掉落表条目
public class DropTableEntry
{
    public string ItemId { get; set; }
    public float DropChance { get; set; }
    public int MinQuantity { get; set; }
    public int MaxQuantity { get; set; }
    public bool IsGuaranteed { get; set; }
    public float RareBonusChance { get; set; }
}

// 玩家 Boss 战斗记录
public class BossBattleRecord
{
    public string BossId { get; set; }
    public string BossName { get; set; }
    public DateTime BattleStartTime { get; set; }
    public DateTime? BattleEndTime { get; set; }
    public bool IsVictory { get; set; }
    public float TotalDamageDealt { get; set; }
    public float TotalDamageTaken { get; set; }
    public float TotalHealing { get; set; }
    public int EnemiesKilled { get; set; }
    public int TimesKnockedDown { get; set; }
    public int SkillsUsed { get; set; }
    public int BestCombo { get; set; }
    public float SurvivalTime { get; set; }
    public int Ranking { get; set; }
    public List<string> RewardsReceived { get; set; } = new List<string>();
}

// 玩家 Boss 统计数据
public class PlayerBossStats
{
    public int TotalBossesDefeated { get; set; }
    public int WorldBossKills { get; set; }
    public int LegendaryBossKills { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public float TotalSurvivalTime { get; set; }
    public int FirstBloods { get; set; }
    public Dictionary<string, int> BossKillCount { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, float> BestSurvivalTimes { get; set; } = new Dictionary<string, float>();
    public Dictionary<string, float> BestDPS { get; set; } = new Dictionary<string, float>();
    public int CurrentCombo { get; set; }
    public int BestCombo { get; set; }
    public int TotalComboScore { get; set; }
    public Dictionary<string, List<BossBattleRecord>> BattleHistory { get; set; } = new Dictionary<string, List<BossBattleRecord>>();
}

// Boss 战斗实例数据
public class BossBattleInstance
{
    public string InstanceId { get; set; }
    public string BossConfigId { get; set; }
    public BossConfig Config { get; set; }
    public float CurrentHealth { get; set; }
    public int CurrentPhase { get; set; }
    public BossPhase Phase { get; set; }
    public float TimeInCombat { get; set; }
    public float TimeSinceLastAttack { get; set; }
    public float TimeSinceLastSkill { get; set; }
    public AttackPattern CurrentPattern { get; set; }
    public bool IsEnraged { get; set; }
    public bool IsRageTriggered { get; set; } // HP < 5% rage (REQ-127)
    public float EnrageProgress { get; set; }
    public float CurrentDamageMultiplier { get; set; }
    public float CurrentSpeedMultiplier { get; set; }
    public Dictionary<string, float> SkillCooldowns { get; set; } = new Dictionary<string, float>();
    public List<string> ActiveEffects { get; set; } = new List<string>();
    public Vector3 LastTargetPosition { get; set; }
    public int TargetsInCombat { get; set; }
    public Dictionary<string, float> PlayerDamageDealt { get; set; } = new Dictionary<string, float>();
    public Dictionary<string, float> PlayerHealingDone { get; set; } = new Dictionary<string, float>();
    public List<string> SummonedMonsters { get; set; } = new List<string>();
    public bool IsAlive => CurrentHealth > 0;
}
