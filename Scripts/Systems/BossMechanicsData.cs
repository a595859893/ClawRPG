using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Boss机制数据结构
/// 包含Boss阶段数据、技能数据、属性和状态定义
/// </summary>
public class BossPhaseData
{
    public string PhaseName { get; set; }
    public int PhaseNumber { get; set; }
    public float HealthPercentage { get; set; }
    public float AttackMultiplier { get; set; } = 1.0f;
    public float DefenseMultiplier { get; set; } = 1.0f;
    public float SpeedMultiplier { get; set; } = 1.0f;
    public List<string> NewSkills { get; set; } = new List<string>();
    public List<string> SpawnEnemies { get; set; } = new List<string>();
    public int SpawnCount { get; set; }
    public float PhaseDuration { get; set; }
    public bool IsEnragePhase { get; set; }
    public string PhaseEffect { get; set; }
}

public class BossSkillData
{
    public string SkillName { get; set; }
    public string SkillId { get; set; }
    public float Cooldown { get; set; }
    public float Range { get; set; }
    public float Damage { get; set; }
    public string TargetType { get; set; }
    public float CastTime { get; set; }
    public bool IsInterruptible { get; set; }
    public string EffectType { get; set; }
}

public class BossEnrageData
{
    public string EnrageName { get; set; }
    public float TriggerTime { get; set; }
    public float AttackMultiplier { get; set; } = 1.5f;
    public float SpeedMultiplier { get; set; } = 1.3f;
    public string EnrageEffect { get; set; }
    public string VisualEffect { get; set; }
}

public class BossMechanicsData : Godot.Resource
{
    [Export] public string BossId { get; set; }
    [Export] public string BossName { get; set; }
    [Export] public int BossLevel { get; set; }
    [Export] public float MaxHealth { get; set; }
    [Export] public float Attack { get; set; }
    [Export] public float Defense { get; set; }
    [Export] public float Speed { get; set; }
    
    [Export] public List<BossPhaseData> Phases { get; set; } = new List<BossPhaseData>();
    [Export] public List<BossSkillData> Skills { get; set; } = new List<BossSkillData>();
    [Export] public List<BossEnrageData> EnrageTimers { get; set; } = new List<BossEnrageData>();
    
    [Export] public bool CanSummonMinions { get; set; }
    [Export] public int MaxMinionCount { get; set; }
    [Export] public string[] MinionTypes { get; set; }
    
    [Export] public float MinionSpawnHealthPercent { get; set; } = 0.5f;
    [Export] public bool HasEnrageMechanic { get; set; }
    [Export] public float EnrageTime { get; set; } = 300f;
    
    [Export] public string WeaknessElement { get; set; }
    [Export] public float WeaknessMultiplier { get; set; } = 1.5f;
    
    [Export] public string[] LootTable { get; set; }
    [Export] public float[] LootWeights { get; set; }
    [Export] public int MinLootCount { get; set; } = 1;
    [Export] public int MaxLootCount { get; set; } = 3;
}

public class BossBattleState
{
    public string BossId { get; set; }
    public int CurrentPhase { get; set; }
    public float CurrentHealth { get; set; }
    public float MaxHealth { get; set; }
    public float BattleTime { get; set; }
    public bool IsEnraged { get; set; }
    public int ActiveMinionCount { get; set; }
    public Dictionary<string, float> SkillCooldowns { get; set; } = new Dictionary<string, float>();
    public Dictionary<string, int> SkillsUsed { get; set; } = new Dictionary<string, int>();
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public bool PhaseChanged { get; set; }
}

public class BossMechanicsStats
{
    public int BossesDefeated { get; set; }
    public int BossesFled { get; set; }
    public int PhasesTriggered { get; set; }
    public int MinionsSpawned { get; set; }
    public int MinionsDefeated { get; set; }
    public int EnrageTriggers { get; set; }
    public int TotalBattleTime { get; set; }
    public int TotalDamageDealt { get; set; }
    public Dictionary<string, int> BossKills { get; set; } = new Dictionary<string, int>();
    public int FastestKillTime { get; set; } = int.MaxValue;
}
