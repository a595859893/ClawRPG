using System;
using System.Collections.Generic;
using BossMechanicData;

public class BossMechanicDatabase
{
    private static BossMechanicDatabase _instance;
    public static BossMechanicDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new BossMechanicDatabase();
            return _instance;
        }
    }

    public Dictionary<string, BossConfiguration> BossConfigs { get; private set; }

    private BossMechanicDatabase()
    {
        BossConfigs = new Dictionary<string, BossConfiguration>();
        InitializeBosses();
    }

    private void InitializeBosses()
    {
        // === 区域 Boss 配置 ===
        
        // 1. 草地区域 Boss - 森林巨熊
        AddBoss(new BossConfiguration
        {
            BossId = "forest_bear",
            BossName = "森林巨熊",
            RegionId = "forest",
            RecommendedLevel = 5,
            BaseHealth = 500f,
            BaseAttack = 25f,
            BaseDefense = 10f,
            BaseSpeed = 2f,
            EnrageTimer = 120f,
            EnrageDamageMultiplier = 1.5f,
            MaxSummons = 0,
            LootBonusMultiplier = 1.0f,
            AggressionRadius = 15f,
            RetreatThreshold = 0.3f,
            HealPercent = 0f,
            Phases = new List<BossPhase>
            {
                new BossPhase
                {
                    PhaseId = "phase1_normal",
                    PhaseName = "正常",
                    PhaseType = BossPhaseType.Normal,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 1.0f,
                    AttackMultiplier = 1.0f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.0f
                },
                new BossPhase
                {
                    PhaseId = "phase2_enraged",
                    PhaseName = "狂暴",
                    PhaseType = BossPhaseType.Enraged,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.5f,
                    AttackMultiplier = 1.5f,
                    DefenseMultiplier = 0.8f,
                    SpeedMultiplier = 1.3f,
                    VisualEffect = "rage_aura",
                    EnableShield = false
                }
            },
            Skills = new List<BossSkill>
            {
                new BossSkill
                {
                    SkillId = "bear_swipe",
                    SkillName = "拍击",
                    Description = "挥爪攻击前方敌人",
                    Type = SkillType.MeleeAttack,
                    BaseDamage = 30f,
                    Cooldown = 3f,
                    Range = 5f,
                    CastTime = 0.5f,
                    Priority = 1.0f
                },
                new BossSkill
                {
                    SkillId = "bear_roar",
                    SkillName = "咆哮",
                    Description = "震耳欲聋的咆哮，降低敌人防御",
                    Type = SkillType.Debuff,
                    BaseDamage = 10f,
                    Cooldown = 15f,
                    Range = 10f,
                    CastTime = 1f,
                    Buffs = new string[] { "defense_down" },
                    Priority = 0.7f
                }
            }
        });

        // 2. 湖泊区域 Boss - 深渊蛟龙
        AddBoss(new BossConfiguration
        {
            BossId = "lake_dragon",
            BossName = "深渊蛟龙",
            RegionId = "lake",
            RecommendedLevel = 15,
            BaseHealth = 2000f,
            BaseAttack = 50f,
            BaseDefense = 20f,
            BaseSpeed = 3f,
            EnrageTimer = 90f,
            EnrageDamageMultiplier = 1.8f,
            MaxSummons = 3,
            LootBonusMultiplier = 1.5f,
            AggressionRadius = 20f,
            RetreatThreshold = 0.25f,
            HealPercent = 0.05f,
            Phases = new List<BossPhase>
            {
                new BossPhase
                {
                    PhaseId = "phase1_normal",
                    PhaseName = "正常",
                    PhaseType = BossPhaseType.Normal,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 1.0f,
                    AttackMultiplier = 1.0f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.0f
                },
                new BossPhase
                {
                    PhaseId = "phase2_enraged",
                    PhaseName = "愤怒",
                    PhaseType = BossPhaseType.Enraged,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.66f,
                    AttackMultiplier = 1.4f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.2f,
                    VisualEffect = "water_aura"
                },
                new BossPhase
                {
                    PhaseId = "phase3_frenzy",
                    PhaseName = " frenzy",
                    PhaseType = BossPhaseType.Frenzy,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.33f,
                    AttackMultiplier = 2.0f,
                    DefenseMultiplier = 0.7f,
                    SpeedMultiplier = 1.5f,
                    VisualEffect = "water_torrent",
                    EnableShield = true,
                    ShieldDuration = 3f
                }
            },
            Skills = new List<BossSkill>
            {
                new BossSkill
                {
                    SkillId = "dragon_bite",
                    SkillName = "龙咬",
                    Description = "尖锐的龙牙撕咬",
                    Type = SkillType.MeleeAttack,
                    BaseDamage = 60f,
                    Cooldown = 2f,
                    Range = 6f,
                    CastTime = 0.3f,
                    Priority = 1.0f
                },
                new BossSkill
                {
                    SkillId = "dragon_flood",
                    SkillName = "洪水",
                    Description = "召唤洪水攻击范围内敌人",
                    Type = SkillType.AreaOfEffect,
                    BaseDamage = 80f,
                    Cooldown = 10f,
                    Range = 15f,
                    AreaRadius = 8f,
                    CastTime = 2f,
                    Priority = 0.8f
                },
                new BossSkill
                {
                    SkillId = "dragon_summon",
                    SkillName = "召唤水元素",
                    Description = "召唤水元素助战",
                    Type = SkillType.Summon,
                    BaseDamage = 0f,
                    Cooldown = 30f,
                    Range = 0f,
                    CastTime = 3f,
                    Priority = 0.5f
                },
                new BossSkill
                {
                    SkillId = "dragon_heal",
                    SkillName = "水之治愈",
                    Description = "汲取水元素恢复生命",
                    Type = SkillType.Buff,
                    BaseDamage = -50f,
                    Cooldown = 20f,
                    Range = 0f,
                    CastTime = 1f,
                    Priority = 0.3f
                }
            }
        });

        // 3. 山脉区域 Boss - 泰坦巨人
        AddBoss(new BossConfiguration
        {
            BossId = "mountain_titan",
            BossName = "泰坦巨人",
            RegionId = "mountain",
            RecommendedLevel = 25,
            BaseHealth = 5000f,
            BaseAttack = 80f,
            BaseDefense = 40f,
            BaseSpeed = 1.5f,
            EnrageTimer = 180f,
            EnrageDamageMultiplier = 2.0f,
            MaxSummons = 0,
            LootBonusMultiplier = 2.0f,
            AggressionRadius = 25f,
            RetreatThreshold = 0.2f,
            HealPercent = 0f,
            Phases = new List<BossPhase>
            {
                new BossPhase
                {
                    PhaseId = "phase1_normal",
                    PhaseName = "正常",
                    PhaseType = BossPhaseType.Normal,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 1.0f,
                    AttackMultiplier = 1.0f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.0f
                },
                new BossPhase
                {
                    PhaseId = "phase2_enraged",
                    PhaseName = "岩石装甲",
                    PhaseType = BossPhaseType.Enraged,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.7f,
                    AttackMultiplier = 1.3f,
                    DefenseMultiplier = 1.5f,
                    SpeedMultiplier = 0.8f,
                    VisualEffect = "rock_armor",
                    EnableShield = true,
                    ShieldDuration = 5f
                },
                new BossPhase
                {
                    PhaseId = "phase3_finalstand",
                    PhaseName = "最终姿态",
                    PhaseType = BossPhaseType.FinalStand,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.3f,
                    AttackMultiplier = 2.5f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.2f,
                    VisualEffect = "earthquake"
                }
            },
            Skills = new List<BossSkill>
            {
                new BossSkill
                {
                    SkillId = "titan_punch",
                    SkillName = "重拳",
                    Description = "毁天灭地的重拳",
                    Type = SkillType.MeleeAttack,
                    BaseDamage = 120f,
                    Cooldown = 4f,
                    Range = 8f,
                    CastTime = 1f,
                    Priority = 1.0f
                },
                new BossSkill
                {
                    SkillId = "titan_stomp",
                    SkillName = "践踏",
                    Description = "震碎大地的践踏",
                    Type = SkillType.AreaOfEffect,
                    BaseDamage = 150f,
                    Cooldown = 12f,
                    Range = 20f,
                    AreaRadius = 12f,
                    CastTime = 2f,
                    Priority = 0.9f
                },
                new BossSkill
                {
                    SkillId = "titan_rock_throw",
                    SkillName = "岩石投掷",
                    Description = "投掷巨大岩石",
                    Type = SkillType.RangedAttack,
                    BaseDamage = 100f,
                    Cooldown = 6f,
                    Range = 30f,
                    CastTime = 1.5f,
                    Priority = 0.7f
                },
                new BossSkill
                {
                    SkillId = "titan_charge",
                    SkillName = "冲锋",
                    Description = "全速冲锋撞击",
                    Type = SkillType.Charge,
                    BaseDamage = 200f,
                    Cooldown = 20f,
                    Range = 25f,
                    CastTime = 1f,
                    Priority = 0.6f
                }
            }
        });

        // 4. 火山区域 Boss - 炎魔领主
        AddBoss(new BossConfiguration
        {
            BossId = "volcano_lord",
            BossName = "炎魔领主",
            RegionId = "volcano",
            RecommendedLevel = 35,
            BaseHealth = 8000f,
            BaseAttack = 120f,
            BaseDefense = 30f,
            BaseSpeed = 2.5f,
            EnrageTimer = 150f,
            EnrageDamageMultiplier = 2.2f,
            MaxSummons = 5,
            LootBonusMultiplier = 2.5f,
            AggressionRadius = 25f,
            RetreatThreshold = 0.2f,
            HealPercent = 0.1f,
            Phases = new List<BossPhase>
            {
                new BossPhase
                {
                    PhaseId = "phase1_normal",
                    PhaseName = "正常",
                    PhaseType = BossPhaseType.Normal,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 1.0f,
                    AttackMultiplier = 1.0f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.0f
                },
                new BossPhase
                {
                    PhaseId = "phase2_transformation",
                    PhaseName = "火焰形态",
                    PhaseType = BossPhaseType.Transformation,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.75f,
                    AttackMultiplier = 1.5f,
                    DefenseMultiplier = 0.8f,
                    SpeedMultiplier = 1.4f,
                    VisualEffect = "fire_transformation"
                },
                new BossPhase
                {
                    PhaseId = "phase3_enraged",
                    PhaseName = "末日火焰",
                    PhaseType = BossPhaseType.Enraged,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.5f,
                    AttackMultiplier = 2.0f,
                    DefenseMultiplier = 1.2f,
                    SpeedMultiplier = 1.0f,
                    VisualEffect = "inferno",
                    EnableShield = true,
                    ShieldDuration = 4f
                },
                new BossPhase
                {
                    PhaseId = "phase4_finalstand",
                    PhaseName = "最终毁灭",
                    PhaseType = BossPhaseType.FinalStand,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.2f,
                    AttackMultiplier = 3.0f,
                    DefenseMultiplier = 0.5f,
                    SpeedMultiplier = 1.8f,
                    VisualEffect = "apocalypse"
                }
            },
            Skills = new List<BossSkill>
            {
                new BossSkill
                {
                    SkillId = "lord_fireball",
                    SkillName = "火球术",
                    Description = "发射巨型火球",
                    Type = SkillType.RangedAttack,
                    BaseDamage = 150f,
                    Cooldown = 4f,
                    Range = 25f,
                    CastTime = 1.5f,
                    Priority = 1.0f
                },
                new BossSkill
                {
                    SkillId = "lord_meteor",
                    SkillName = "流星火雨",
                    Description = "召唤流星坠落",
                    Type = SkillType.AreaOfEffect,
                    BaseDamage = 200f,
                    Cooldown = 15f,
                    Range = 30f,
                    AreaRadius = 15f,
                    CastTime = 3f,
                    Priority = 0.9f
                },
                new BossSkill
                {
                    SkillId = "lord_inferno",
                    SkillName = "炼狱",
                    Description = "持续燃烧范围内敌人",
                    Type = SkillType.AreaOfEffect,
                    BaseDamage = 50f,
                    Cooldown = 25f,
                    Range = 20f,
                    AreaRadius = 18f,
                    CastTime = 2f,
                    Priority = 0.7f
                },
                new BossSkill
                {
                    SkillId = "lord_summon",
                    SkillName = "召唤炎魔",
                    Description = "召唤小火魔助战",
                    Type = SkillType.Summon,
                    BaseDamage = 0f,
                    Cooldown = 20f,
                    Range = 0f,
                    CastTime = 2f,
                    Priority = 0.5f
                },
                new BossSkill
                {
                    SkillId = "lord_self_destruct",
                    SkillName = "自爆",
                    Description = "牺牲自我造成巨大伤害",
                    Type = SkillType.SelfDestruct,
                    BaseDamage = 500f,
                    Cooldown = 60f,
                    Range = 25f,
                    AreaRadius = 20f,
                    CastTime = 3f,
                    Priority = 0.2f
                }
            }
        });

        // 5. 冰霜区域 Boss - 冰霜巨龙
        AddBoss(new BossConfiguration
        {
            BossId = "ice_dragon",
            BossName = "冰霜巨龙",
            RegionId = "ice",
            RecommendedLevel = 40,
            BaseHealth = 12000f,
            BaseAttack = 150f,
            BaseDefense = 50f,
            BaseSpeed = 3f,
            EnrageTimer = 180f,
            EnrageDamageMultiplier = 2.5f,
            MaxSummons = 4,
            LootBonusMultiplier = 3.0f,
            AggressionRadius = 30f,
            RetreatThreshold = 0.15f,
            HealPercent = 0.08f,
            Phases = new List<BossPhase>
            {
                new BossPhase
                {
                    PhaseId = "phase1_normal",
                    PhaseName = "正常",
                    PhaseType = BossPhaseType.Normal,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 1.0f,
                    AttackMultiplier = 1.0f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.0f
                },
                new BossPhase
                {
                    PhaseId = "phase2_frozen_throne",
                    PhaseName = "冰封王座",
                    PhaseType = BossPhaseType.Transformation,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.6f,
                    AttackMultiplier = 1.3f,
                    DefenseMultiplier = 1.8f,
                    SpeedMultiplier = 0.5f,
                    VisualEffect = "ice_throne",
                    EnableShield = true,
                    ShieldDuration = 8f
                },
                new BossPhase
                {
                    PhaseId = "phase3_frenzy",
                    PhaseName = "绝对零度",
                    PhaseType = BossPhaseType.Frenzy,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.3f,
                    AttackMultiplier = 2.5f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.5f,
                    VisualEffect = "absolute_zero"
                }
            },
            Skills = new List<BossSkill>
            {
                new BossSkill
                {
                    SkillId = "ice_frost_breath",
                    SkillName = "冰霜吐息",
                    Description = "冰霜巨龙吐息",
                    Type = SkillType.Beam,
                    BaseDamage = 180f,
                    Cooldown = 8f,
                    Range = 20f,
                    CastTime = 2f,
                    Priority = 1.0f
                },
                new BossSkill
                {
                    SkillId = "ice_blizzard",
                    SkillName = "冰风暴",
                    Description = "召唤冰风暴攻击",
                    Type = SkillType.AreaOfEffect,
                    BaseDamage = 150f,
                    Cooldown = 12f,
                    Range = 25f,
                    AreaRadius = 15f,
                    CastTime = 3f,
                    Priority = 0.9f
                },
                new BossSkill
                {
                    SkillId = "ice_freeze",
                    SkillName = "冰冻",
                    Description = "冰冻敌人",
                    Type = SkillType.Debuff,
                    BaseDamage = 50f,
                    Cooldown = 15f,
                    Range = 15f,
                    CastTime = 1f,
                    Buffs = new string[] { "frozen", "cold" },
                    Priority = 0.7f
                },
                new BossSkill
                {
                    SkillId = "ice_summon",
                    SkillName = "召唤冰元素",
                    Description = "召唤冰元素守卫",
                    Type = SkillType.Summon,
                    BaseDamage = 0f,
                    Cooldown = 25f,
                    Range = 0f,
                    CastTime = 2f,
                    Priority = 0.4f
                },
                new BossSkill
                {
                    SkillId = "ice_teleport",
                    SkillName = "闪现",
                    Description = "瞬间移动位置",
                    Type = SkillType.Teleport,
                    BaseDamage = 0f,
                    Cooldown = 10f,
                    Range = 30f,
                    CastTime = 0.5f,
                    Priority = 0.6f
                }
            }
        });

        // 6. 暗影区域 Boss - 暗影君主
        AddBoss(new BossConfiguration
        {
            BossId = "shadow_lord",
            BossName = "暗影君主",
            RegionId = "shadow",
            RecommendedLevel = 45,
            BaseHealth = 15000f,
            BaseAttack = 180f,
            BaseDefense = 40f,
            BaseSpeed = 4f,
            EnrageTimer = 200f,
            EnrageDamageMultiplier = 2.8f,
            MaxSummons = 6,
            LootBonusMultiplier = 3.5f,
            AggressionRadius = 30f,
            RetreatThreshold = 0.1f,
            HealPercent = 0.12f,
            Phases = new List<BossPhase>
            {
                new BossPhase
                {
                    PhaseId = "phase1_normal",
                    PhaseName = "正常",
                    PhaseType = BossPhaseType.Normal,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 1.0f,
                    AttackMultiplier = 1.0f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.0f
                },
                new BossPhase
                {
                    PhaseId = "phase2_shadow_realm",
                    PhaseName = "暗影领域",
                    PhaseType = BossPhaseType.Transformation,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.66f,
                    AttackMultiplier = 1.5f,
                    DefenseMultiplier = 1.3f,
                    SpeedMultiplier = 1.2f,
                    VisualEffect = "shadow_realm",
                    EnableShield = true,
                    ShieldDuration = 5f
                },
                new BossPhase
                {
                    PhaseId = "phase3_dark_ascension",
                    PhaseName = "黑暗升华",
                    PhaseType = BossPhaseType.Enraged,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.33f,
                    AttackMultiplier = 2.2f,
                    DefenseMultiplier = 0.8f,
                    SpeedMultiplier = 1.6f,
                    VisualEffect = "dark_energy"
                },
                new BossPhase
                {
                    PhaseId = "phase4_void",
                    PhaseName = "虚空降临",
                    PhaseType = BossPhaseType.FinalStand,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.1f,
                    AttackMultiplier = 3.5f,
                    DefenseMultiplier = 1.5f,
                    SpeedMultiplier = 2.0f,
                    VisualEffect = "void_portal"
                }
            },
            Skills = new List<BossSkill>
            {
                new BossSkill
                {
                    SkillId = "shadow_scythe",
                    SkillName = "暗影镰刀",
                    Description = "挥舞暗影镰刀",
                    Type = SkillType.MeleeAttack,
                    BaseDamage = 200f,
                    Cooldown = 3f,
                    Range = 10f,
                    CastTime = 0.8f,
                    Priority = 1.0f
                },
                new BossSkill
                {
                    SkillId = "shadow_darkness",
                    SkillName = "暗影笼罩",
                    Description = "使区域陷入黑暗",
                    Type = SkillType.Debuff,
                    BaseDamage = 100f,
                    Cooldown = 18f,
                    Range = 25f,
                    AreaRadius = 15f,
                    CastTime = 2f,
                    Buffs = new string[] { "blind", "silence" },
                    Priority = 0.8f
                },
                new BossSkill
                {
                    SkillId = "shadow_soul_drain",
                    SkillName = "灵魂汲取",
                    Description = "汲取敌人灵魂恢复生命",
                    Type = SkillType.Buff,
                    BaseDamage = -200f,
                    Cooldown = 20f,
                    Range = 15f,
                    CastTime = 1.5f,
                    Priority = 0.6f
                },
                new BossSkill
                {
                    SkillId = "shadow_portal",
                    SkillName = "暗影传送",
                    Description = "在暗影中穿梭",
                    Type = SkillType.Teleport,
                    BaseDamage = 0f,
                    Cooldown = 8f,
                    Range = 35f,
                    CastTime = 0.3f,
                    Priority = 0.7f
                },
                new BossSkill
                {
                    SkillId = "shadow_mass_summon",
                    SkillName = "暗影大军",
                    Description = "召唤大量暗影生物",
                    Type = SkillType.Summon,
                    BaseDamage = 0f,
                    Cooldown = 45f,
                    Range = 0f,
                    CastTime = 4f,
                    Priority = 0.4f
                },
                new BossSkill
                {
                    SkillId = "shadow_death_beam",
                    SkillName = "死亡光线",
                    Description = "汇聚暗影能量的死亡光线",
                    Type = SkillType.Beam,
                    BaseDamage = 300f,
                    Cooldown = 30f,
                    Range = 40f,
                    CastTime = 4f,
                    Priority = 0.9f
                }
            }
        });

        // 7. 龙之巢穴 Boss - 远古巨龙
        AddBoss(new BossConfiguration
        {
            BossId = "ancient_dragon",
            BossName = "远古巨龙",
            RegionId = "dragon_lair",
            RecommendedLevel = 50,
            BaseHealth = 25000f,
            BaseAttack = 250f,
            BaseDefense = 80f,
            BaseSpeed = 3.5f,
            EnrageTimer = 240f,
            EnrageDamageMultiplier = 3.0f,
            MaxSummons = 8,
            LootBonusMultiplier = 5.0f,
            AggressionRadius = 40f,
            RetreatThreshold = 0.1f,
            HealPercent = 0.15f,
            Phases = new List<BossPhase>
            {
                new BossPhase
                {
                    PhaseId = "phase1_normal",
                    PhaseName = "正常",
                    PhaseType = BossPhaseType.Normal,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 1.0f,
                    AttackMultiplier = 1.0f,
                    DefenseMultiplier = 1.0f,
                    SpeedMultiplier = 1.0f
                },
                new BossPhase
                {
                    PhaseId = "phase2_dragon_form",
                    PhaseName = "巨龙形态",
                    PhaseType = BossPhaseType.Transformation,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.8f,
                    AttackMultiplier = 1.8f,
                    DefenseMultiplier = 1.5f,
                    SpeedMultiplier = 1.3f,
                    VisualEffect = "dragon_morph"
                },
                new BossPhase
                {
                    PhaseId = "phase3_elemental_fury",
                    PhaseName = "元素愤怒",
                    PhaseType = BossPhaseType.Enraged,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.5f,
                    AttackMultiplier = 2.5f,
                    DefenseMultiplier = 1.2f,
                    SpeedMultiplier = 1.5f,
                    VisualEffect = "elemental_fury"
                },
                new BossPhase
                {
                    PhaseId = "phase4_apocalyptic",
                    PhaseName = "末日降临",
                    PhaseType = BossPhaseType.FinalStand,
                    TriggerType = PhaseTriggerType.HealthPercent,
                    HealthThreshold = 0.2f,
                    AttackMultiplier = 4.0f,
                    DefenseMultiplier = 2.0f,
                    SpeedMultiplier = 2.0f,
                    VisualEffect = "apocalyptic_fire"
                }
            },
            Skills = new List<BossSkill>
            {
                new BossSkill
                {
                    SkillId = "dragon_fire_breath",
                    SkillName = "龙息",
                    Description = "毁灭性的龙息",
                    Type = SkillType.Beam,
                    BaseDamage = 350f,
                    Cooldown = 10f,
                    Range = 30f,
                    CastTime = 3f,
                    Priority = 1.0f
                },
                new BossSkill
                {
                    SkillId = "dragon_elemental_blast",
                    SkillName = "元素爆发",
                    Description = "释放所有元素能量",
                    Type = SkillType.AreaOfEffect,
                    BaseDamage = 400f,
                    Cooldown = 25f,
                    Range = 40f,
                    AreaRadius = 25f,
                    CastTime = 4f,
                    Priority = 1.0f
                },
                new BossSkill
                {
                    SkillId = "dragon_tail_sweep",
                    SkillName = "扫尾",
                    Description = "巨龙摆尾",
                    Type = SkillType.AreaOfEffect,
                    BaseDamage = 250f,
                    Cooldown = 8f,
                    Range = 20f,
                    AreaRadius = 18f,
                    CastTime = 1f,
                    Priority = 0.9f
                },
                new BossSkill
                {
                    SkillId = "dragon_mass_summon",
                    SkillName = "召唤龙群",
                    Description = "召唤大量亚龙助战",
                    Type = SkillType.Summon,
                    BaseDamage = 0f,
                    Cooldown = 60f,
                    Range = 0f,
                    CastTime = 5f,
                    Priority = 0.5f
                },
                new BossSkill
                {
                    SkillId = "dragon_teleport",
                    SkillName = "空间转移",
                    Description = "瞬间移动到任意位置",
                    Type = SkillType.Teleport,
                    BaseDamage = 0f,
                    Cooldown = 15f,
                    Range = 50f,
                    CastTime = 0.5f,
                    Priority = 0.7f
                },
                new BossSkill
                {
                    SkillId = "dragon_divine_wrath",
                    SkillName = "神圣愤怒",
                    Description = "最终技能 - 神圣愤怒",
                    Type = SkillType.AreaOfEffect,
                    BaseDamage = 800f,
                    Cooldown = 120f,
                    Range = 50f,
                    AreaRadius = 30f,
                    CastTime = 5f,
                    Priority = 1.0f
                }
            }
        });
    }

    public void AddBoss(BossConfiguration config)
    {
        BossConfigs[config.BossId] = config;
    }

    public BossConfiguration GetBossConfig(string bossId)
    {
        if (BossConfigs.ContainsKey(bossId))
            return BossConfigs[bossId];
        return null;
    }

    public List<BossConfiguration> GetAllBosses()
    {
        return new List<BossConfiguration>(BossConfigs.Values);
    }

    public List<BossConfiguration> GetBossesByRegion(string regionId)
    {
        List<BossConfiguration> result = new List<BossConfiguration>();
        foreach (var boss in BossConfigs.Values)
        {
            if (boss.RegionId == regionId)
                result.Add(boss);
        }
        return result;
    }
}
