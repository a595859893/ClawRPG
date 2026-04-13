using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 模仿技能触发类型 — 定义技能在什么条件下激活
    /// </summary>
    public enum MimicryTriggerType
    {
        /// <summary>无触发条件（手动或默认）</summary>
        None,
        /// <summary>宠物HP低于阈值时触发</summary>
        HpBelowThreshold,
        /// <summary>主人受伤时触发（反击类）</summary>
        OnOwnerDamaged,
        /// <summary>敌人在范围内时触发</summary>
        OnEnemyNearby,
        /// <summary>主人攻击时触发（协同类）</summary>
        OnOwnerAttacking,
        /// <summary>冷却结束自动触发</summary>
        CooldownBased,
        /// <summary>玩家手动激活</summary>
        ManualToggle,
        /// <summary>进入特定环境类型时触发</summary>
        OnEnvironmentMatch
    }

    /// <summary>
    /// 技能触发配置 — 定义单个技能的触发条件、优先级和互斥组
    /// </summary>
    public struct MimicryTriggerConfig
    {
        /// <summary>触发类型</summary>
        public MimicryTriggerType Trigger { get; set; }

        /// <summary>HP阈值（HpBelowThreshold时使用，0.0-1.0）</summary>
        public float Threshold { get; set; }

        /// <summary>敌人距离阈值（OnEnemyNearby时使用，像素）</summary>
        public float Range { get; set; }

        /// <summary>激活优先级（越高越优先，0-100）</summary>
        public int Priority { get; set; }

        /// <summary>互斥组名（同组技能不能同时激活）</summary>
        public string MutexGroup { get; set; }

        /// <summary>该技能对应的环境类型（OnEnvironmentMatch时使用）</summary>
        public RoomEnvironmentType EnvironmentType { get; set; }

        public static MimicryTriggerConfig Default => new MimicryTriggerConfig
        {
            Trigger = MimicryTriggerType.CooldownBased,
            Threshold = 0f,
            Range = 200f,
            Priority = 50,
            MutexGroup = null,
            EnvironmentType = RoomEnvironmentType.None
        };
    }

    /// <summary>
    /// 模仿技能类型 — 宠物根据印记习得的技能效果
    /// </summary>
    public enum MimicrySkillType
    {
        /// <summary>火焰吐息 — 火系伤害</summary>
        FireBreath,
        /// <summary>冰霜吐息 — 冰系减速</summary>
        FrostBreath,
        /// <summary>电弧打击 — 电系AOE</summary>
        ElectricArc,
        /// <summary>暗影撕裂 — 暗系穿透</summary>
        ShadowTear,
        /// <summary>神圣制裁 — 神圣系爆发</summary>
        HolySmite,
        /// <summary>自然缠绕 — 藤蔓控制</summary>
        NatureBind,
        /// <summary>突进拉扯 — 位移冲击</summary>
        DashStrike,
        /// <summary>狂暴连击 — 高频攻击</summary>
        FrenzyStrike,
        /// <summary>铁壁防御 — 临时护盾</summary>
        IronBulwark,
        /// <summary>背水一战 — 低血量狂暴</summary>
        LastStand,
        /// <summary>掩护撤退 — 加速+减伤</summary>
        Rearguard,
        /// <summary>精英杀手 — 对精英特攻</summary>
        EliteSlayer,
        /// <summary>闪避精通 — 闪避率提升</summary>
        DodgeMaster,
        /// <summary>陷阱感知 — 陷阱预警</summary>
        TrapSense,
        /// <summary>谜题之眼 — 提示系统</summary>
        PuzzleInsight,
        /// <summary>掠夺本能 — 战利品加成</summary>
        LootInstinct,
        /// <summary>治疗之光 — 恢复宠物HP</summary>
        HealingLight,
        /// <summary>协同獠牙 — 协同攻击强化</summary>
        SynergyFangs,
        /// <summary>特殊形态 — 特殊互动触发</summary>
        SpecialMorph
    }

    /// <summary>
    /// 模仿技能定义 — 将 PlayerBehaviorType 映射为宠物的技能效果
    /// </summary>
    public class MimicrySkillDefinition
    {
        public MimicrySkillType SkillType { get; set; }
        public PlayerBehaviorType SourceBehavior { get; set; }
        public string SkillName { get; set; }
        public string Description { get; set; }
        public float BaseDamage { get; set; }
        public float DamagePerLevel { get; set; }  // 每级增加伤害
        public float BaseDuration { get; set; }
        public float DurationPerLevel { get; set; }
        public float CooldownSeconds { get; set; }
        public float CooldownReductionPerLevel { get; set; }
        public int MaxCharges { get; set; }         // 0 = 无限制（普通技能）
        public float ChargeRestoreTime { get; set; } // 每次充能时间

        // ── REQ-146: Skill Activator 触发配置 ──────────────────────────────
        /// <summary>技能触发条件配置</summary>
        public MimicryTriggerConfig TriggerConfig { get; set; }

        /// <summary>
        /// 根据印记等级计算实际伤害
        /// </summary>
        public float GetDamage(int imprintLevel)
        {
            return BaseDamage + DamagePerLevel * imprintLevel;
        }

        /// <summary>
        /// 根据印记等级计算持续时间
        /// </summary>
        public float GetDuration(int imprintLevel)
        {
            return BaseDuration + DurationPerLevel * imprintLevel;
        }

        /// <summary>
        /// 根据印记等级计算冷却时间
        /// </summary>
        public float GetCooldown(int imprintLevel)
        {
            float cd = CooldownSeconds - CooldownReductionPerLevel * imprintLevel;
            return Mathf.Max(cd, 1f); // 最低1秒冷却
        }
    }

    /// <summary>
    /// 模仿技能实例 — 宠物实际使用的技能对象
    /// </summary>
    public class MimicrySkillInstance
    {
        public MimicrySkillDefinition Definition { get; set; }
        public int ImprintLevel { get; set; }
        public float CurrentCooldown { get; set; }
        public int CurrentCharges { get; set; }
        public float ChargeAccumulator { get; set; }

        public float Damage => Definition.GetDamage(ImprintLevel);
        public float Duration => Definition.GetDuration(ImprintLevel);
        public float Cooldown => Definition.GetCooldown(ImprintLevel);

        public bool IsReady => CurrentCooldown <= 0f && (Definition.MaxCharges == 0 || CurrentCharges > 0);

        public void Tick(float delta)
        {
            if (CurrentCooldown > 0f)
                CurrentCooldown -= delta;

            if (Definition.MaxCharges > 0)
            {
                ChargeAccumulator += delta;
                if (ChargeAccumulator >= Definition.ChargeRestoreTime)
                {
                    ChargeAccumulator -= Definition.ChargeRestoreTime;
                    CurrentCharges = Mathf.Min(CurrentCharges + 1, Definition.MaxCharges);
                }
            }
        }

        public bool TryUse()
        {
            if (!IsReady) return false;

            if (Definition.MaxCharges > 0)
            {
                CurrentCharges--;
                ChargeAccumulator = 0f;
            }
            else
            {
                CurrentCooldown = Cooldown;
            }
            return true;
        }
    }

    /// <summary>
    /// 行为 → 技能映射数据库 — 全局单例
    /// 
    /// 职责：
    /// 1. 管理所有 (PlayerBehaviorType → MimicrySkillDefinition) 映射
    /// 2. 提供技能查询接口
    /// 3. 生成技能实例
    /// </summary>
    public partial class MimicryDatabase : Node
    {
        public static MimicryDatabase Instance { get; private set; }

        /// <summary>
        /// 所有技能定义（按技能类型索引）
        /// </summary>
        private Dictionary<MimicrySkillType, MimicrySkillDefinition> _skillsByType = new Dictionary<MimicrySkillType, MimicrySkillDefinition>();

        /// <summary>
        /// 所有技能定义（按来源行为索引）
        /// </summary>
        private Dictionary<PlayerBehaviorType, MimicrySkillDefinition> _skillsByBehavior = new Dictionary<PlayerBehaviorType, MimicrySkillDefinition>();

        public override void _Ready()
        {
            Instance = this;
            BuildDatabase();
            GD.Print($"[MimicryDatabase] Initialized with {_skillsByType.Count} skill definitions");
        }

        /// <summary>
        /// 构建行为 → 技能映射表
        /// </summary>
        private void BuildDatabase()
        {
            var definitions = new List<MimicrySkillDefinition>
            {
                // ── 元素系 ──────────────────────────────────────────────────
                // ── 元素系 ──────────────────────────────────────────────────
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.FireBreath,
                    SourceBehavior = PlayerBehaviorType.UseFireSkill,
                    SkillName = "火焰吐息",
                    Description = "喷吐火焰，对敌人造成火系伤害",
                    BaseDamage = 15f,
                    DamagePerLevel = 5f,
                    BaseDuration = 0f,
                    DurationPerLevel = 0f,
                    CooldownSeconds = 8f,
                    CooldownReductionPerLevel = 0.5f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnemyNearby,
                        Threshold = 0f,
                        Range = 250f,
                        Priority = 60,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.FrostBreath,
                    SourceBehavior = PlayerBehaviorType.UseIceSkill,
                    SkillName = "冰霜吐息",
                    Description = "喷吐冰霜，减速敌人并造成冰系伤害",
                    BaseDamage = 12f,
                    DamagePerLevel = 4f,
                    BaseDuration = 2f,
                    DurationPerLevel = 0.3f,
                    CooldownSeconds = 8f,
                    CooldownReductionPerLevel = 0.5f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnemyNearby,
                        Threshold = 0f,
                        Range = 250f,
                        Priority = 60,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.ElectricArc,
                    SourceBehavior = PlayerBehaviorType.UseElectricSkill,
                    SkillName = "电弧打击",
                    Description = "释放电弧，AOE伤害并有小概率眩晕",
                    BaseDamage = 18f,
                    DamagePerLevel = 6f,
                    BaseDuration = 0f,
                    DurationPerLevel = 0f,
                    CooldownSeconds = 10f,
                    CooldownReductionPerLevel = 0.6f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnemyNearby,
                        Threshold = 0f,
                        Range = 180f,
                        Priority = 70,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.ShadowTear,
                    SourceBehavior = PlayerBehaviorType.UseShadowSkill,
                    SkillName = "暗影撕裂",
                    Description = "暗影穿刺，无视部分护甲",
                    BaseDamage = 20f,
                    DamagePerLevel = 7f,
                    BaseDuration = 0f,
                    DurationPerLevel = 0f,
                    CooldownSeconds = 9f,
                    CooldownReductionPerLevel = 0.5f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnemyNearby,
                        Threshold = 0f,
                        Range = 200f,
                        Priority = 65,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.HolySmite,
                    SourceBehavior = PlayerBehaviorType.UseHolySkill,
                    SkillName = "神圣制裁",
                    Description = "神圣之光，对邪恶敌人造成额外伤害",
                    BaseDamage = 22f,
                    DamagePerLevel = 8f,
                    BaseDuration = 0f,
                    DurationPerLevel = 0f,
                    CooldownSeconds = 12f,
                    CooldownReductionPerLevel = 0.7f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnemyNearby,
                        Threshold = 0f,
                        Range = 220f,
                        Priority = 65,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.NatureBind,
                    SourceBehavior = PlayerBehaviorType.UseNatureSkill,
                    SkillName = "自然缠绕",
                    Description = "藤蔓束缚，短暂定身敌人",
                    BaseDamage = 10f,
                    DamagePerLevel = 3f,
                    BaseDuration = 1.5f,
                    DurationPerLevel = 0.2f,
                    CooldownSeconds = 9f,
                    CooldownReductionPerLevel = 0.5f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnemyNearby,
                        Threshold = 0f,
                        Range = 200f,
                        Priority = 60,
                        MutexGroup = null
                    }
                },

                // ── 战术系 ──────────────────────────────────────────────────
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.DashStrike,
                    SourceBehavior = PlayerBehaviorType.FrequentDodge,
                    SkillName = "突进拉扯",
                    Description = "快速位移至敌人身后并造成伤害",
                    BaseDamage = 14f,
                    DamagePerLevel = 4f,
                    BaseDuration = 0f,
                    DurationPerLevel = 0f,
                    CooldownSeconds = 6f,
                    CooldownReductionPerLevel = 0.4f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnemyNearby,
                        Threshold = 0f,
                        Range = 150f,
                        Priority = 75,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.FrenzyStrike,
                    SourceBehavior = PlayerBehaviorType.AggressiveAttack,
                    SkillName = "狂暴连击",
                    Description = "短时间内心智疯狂攻击",
                    BaseDamage = 8f,
                    DamagePerLevel = 2.5f,
                    BaseDuration = 3f,
                    DurationPerLevel = 0.5f,
                    CooldownSeconds = 14f,
                    CooldownReductionPerLevel = 0.8f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnOwnerAttacking,
                        Threshold = 0f,
                        Range = 0f,
                        Priority = 55,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.IronBulwark,
                    SourceBehavior = PlayerBehaviorType.DefensiveStance,
                    SkillName = "铁壁防御",
                    Description = "为宠物自身或玩家提供临时护盾",
                    BaseDamage = 0f,
                    DamagePerLevel = 0f,
                    BaseDuration = 4f,
                    DurationPerLevel = 0.5f,
                    CooldownSeconds = 12f,
                    CooldownReductionPerLevel = 0.6f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnOwnerDamaged,
                        Threshold = 0f,
                        Range = 0f,
                        Priority = 70,
                        MutexGroup = "defense"
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.LastStand,
                    SourceBehavior = PlayerBehaviorType.LowHPAggression,
                    SkillName = "背水一战",
                    Description = "HP低于30%时自动触发，伤害大幅提升",
                    BaseDamage = 30f,
                    DamagePerLevel = 10f,
                    BaseDuration = 0f,
                    DurationPerLevel = 0f,
                    CooldownSeconds = 20f,
                    CooldownReductionPerLevel = 1.2f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.HpBelowThreshold,
                        Threshold = 0.3f,
                        Range = 0f,
                        Priority = 90,
                        MutexGroup = "offense"
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.Rearguard,
                    SourceBehavior = PlayerBehaviorType.QuickRetreat,
                    SkillName = "掩护撤退",
                    Description = "提升主人移速并提供短暂减伤",
                    BaseDamage = 0f,
                    DamagePerLevel = 0f,
                    BaseDuration = 3f,
                    DurationPerLevel = 0.3f,
                    CooldownSeconds = 15f,
                    CooldownReductionPerLevel = 0.8f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnOwnerAttacking,
                        Threshold = 0f,
                        Range = 0f,
                        Priority = 65,
                        MutexGroup = "defense"
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.EliteSlayer,
                    SourceBehavior = PlayerBehaviorType.FocusElite,
                    SkillName = "精英杀手",
                    Description = "对精英/Boss敌人造成额外伤害",
                    BaseDamage = 25f,
                    DamagePerLevel = 9f,
                    BaseDuration = 0f,
                    DurationPerLevel = 0f,
                    CooldownSeconds = 10f,
                    CooldownReductionPerLevel = 0.6f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnemyNearby,
                        Threshold = 0f,
                        Range = 300f,
                        Priority = 80,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.DodgeMaster,
                    SourceBehavior = PlayerBehaviorType.FrequentDodge,
                    SkillName = "闪避精通",
                    Description = "被动提升宠物闪避率（激活时）",
                    BaseDamage = 0f,
                    DamagePerLevel = 0f,
                    BaseDuration = 5f,
                    DurationPerLevel = 0.5f,
                    CooldownSeconds = 18f,
                    CooldownReductionPerLevel = 1f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.CooldownBased,
                        Threshold = 0f,
                        Range = 0f,
                        Priority = 50,
                        MutexGroup = null
                    }
                },

                // ── 感知系 ──────────────────────────────────────────────────
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.TrapSense,
                    SourceBehavior = PlayerBehaviorType.TriggerTrap,
                    SkillName = "陷阱感知",
                    Description = "显示附近陷阱位置并短暂无敌",
                    BaseDamage = 0f,
                    DamagePerLevel = 0f,
                    BaseDuration = 2f,
                    DurationPerLevel = 0.2f,
                    CooldownSeconds = 20f,
                    CooldownReductionPerLevel = 1f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnvironmentMatch,
                        Threshold = 0f,
                        Range = 0f,
                        Priority = 55,
                        MutexGroup = null,
                        EnvironmentType = RoomEnvironmentType.TrapDense
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.PuzzleInsight,
                    SourceBehavior = PlayerBehaviorType.SolvePuzzle,
                    SkillName = "谜题之眼",
                    Description = "显示谜题提示（持续可见）",
                    BaseDamage = 0f,
                    DamagePerLevel = 0f,
                    BaseDuration = 10f,
                    DurationPerLevel = 2f,
                    CooldownSeconds = 30f,
                    CooldownReductionPerLevel = 2f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnEnvironmentMatch,
                        Threshold = 0f,
                        Range = 0f,
                        Priority = 55,
                        MutexGroup = null,
                        EnvironmentType = RoomEnvironmentType.Puzzle
                    }
                },

                // ── 增益系 ──────────────────────────────────────────────────
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.LootInstinct,
                    SourceBehavior = PlayerBehaviorType.CollectLoot,
                    SkillName = "掠夺本能",
                    Description = "开启后短时间内击败敌人额外掉落",
                    BaseDamage = 0f,
                    DamagePerLevel = 0f,
                    BaseDuration = 15f,
                    DurationPerLevel = 2f,
                    CooldownSeconds = 45f,
                    CooldownReductionPerLevel = 3f,
                    MaxCharges = 1,
                    ChargeRestoreTime = 0f,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.CooldownBased,
                        Threshold = 0f,
                        Range = 0f,
                        Priority = 40,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.HealingLight,
                    SourceBehavior = PlayerBehaviorType.UseHealing,
                    SkillName = "治疗之光",
                    Description = "为宠物恢复生命值",
                    BaseDamage = -20f,  // 负值表示治疗
                    DamagePerLevel = -5f,
                    BaseDuration = 0f,
                    DurationPerLevel = 0f,
                    CooldownSeconds = 15f,
                    CooldownReductionPerLevel = 0.8f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.HpBelowThreshold,
                        Threshold = 0.5f,
                        Range = 0f,
                        Priority = 75,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.SynergyFangs,
                    SourceBehavior = PlayerBehaviorType.PetSynergy,
                    SkillName = "协同獠牙",
                    Description = "下一次协同攻击伤害提升",
                    BaseDamage = 15f,
                    DamagePerLevel = 5f,
                    BaseDuration = 0f,
                    DurationPerLevel = 0f,
                    CooldownSeconds = 12f,
                    CooldownReductionPerLevel = 0.6f,
                    MaxCharges = 0,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.OnOwnerAttacking,
                        Threshold = 0f,
                        Range = 0f,
                        Priority = 70,
                        MutexGroup = null
                    }
                },
                new MimicrySkillDefinition
                {
                    SkillType = MimicrySkillType.SpecialMorph,
                    SourceBehavior = PlayerBehaviorType.SpecialInteraction,
                    SkillName = "特殊形态",
                    Description = "宠物临时变形，获得特殊能力",
                    BaseDamage = 0f,
                    DamagePerLevel = 0f,
                    BaseDuration = 8f,
                    DurationPerLevel = 1f,
                    CooldownSeconds = 60f,
                    CooldownReductionPerLevel = 4f,
                    MaxCharges = 1,
                    ChargeRestoreTime = 0f,
                    TriggerConfig = new MimicryTriggerConfig
                    {
                        Trigger = MimicryTriggerType.ManualToggle,
                        Threshold = 0f,
                        Range = 0f,
                        Priority = 30,
                        MutexGroup = null
                    }
                }
            };

            foreach (var def in definitions)
            {
                _skillsByType[def.SkillType] = def;
                _skillsByBehavior[def.SourceBehavior] = def;
            }
        }

        // ── Public API ─────────────────────────────────────────────────────

        /// <summary>
        /// 根据行为类型获取技能定义（若无映射返回null）
        /// </summary>
        public MimicrySkillDefinition GetSkillForBehavior(PlayerBehaviorType behavior)
        {
            return _skillsByBehavior.TryGetValue(behavior, out var def) ? def : null;
        }

        /// <summary>
        /// 根据技能类型获取技能定义
        /// </summary>
        public MimicrySkillDefinition GetSkill(MimicrySkillType skillType)
        {
            return _skillsByType.TryGetValue(skillType, out var def) ? def : null;
        }

        /// <summary>
        /// 获取宠物所有已解锁的技能（行为印记等级>0）
        /// </summary>
        public List<MimicrySkillInstance> GetUnlockedSkills(PetMimicryData mimicryData)
        {
            var result = new List<MimicrySkillInstance>();
            if (mimicryData == null) return result;

            foreach (var kvp in _skillsByBehavior)
            {
                int level = mimicryData.GetHighestLevel(kvp.Key);
                if (level > 0)
                {
                    result.Add(new MimicrySkillInstance
                    {
                        Definition = kvp.Value,
                        ImprintLevel = level,
                        CurrentCooldown = 0f,
                        CurrentCharges = kvp.Value.MaxCharges,
                        ChargeAccumulator = 0f
                    });
                }
            }
            return result;
        }

        /// <summary>
        /// 获取指定环境下的最佳技能（模仿优先级）
        /// </summary>
        public MimicrySkillDefinition GetBestSkillForEnvironment(RoomEnvironmentType envType, PetMimicryData mimicryData)
        {
            MimicrySkillDefinition best = null;
            int bestLevel = 0;

            var imprints = mimicryData.GetImprintsForEnvironment(envType);
            foreach (var imprint in imprints)
            {
                if (imprint.ImprintLevel > 0)
                {
                    var def = GetSkillForBehavior(imprint.BehaviorType);
                    if (def != null && imprint.ImprintLevel > bestLevel)
                    {
                        bestLevel = imprint.ImprintLevel;
                        best = def;
                    }
                }
            }
            return best;
        }

        /// <summary>
        /// 获取技能显示信息（名称+描述+等级）
        /// </summary>
        public (string Name, string Description, string EffectText) GetSkillDisplayInfo(MimicrySkillType skillType, int imprintLevel)
        {
            var def = GetSkill(skillType);
            if (def == null) return ("未知", "无描述", "");

            float dmg = def.GetDamage(imprintLevel);
            float dur = def.GetDuration(imprintLevel);
            float cd = def.GetCooldown(imprintLevel);

            string effectText = dmg != 0f
                ? $"伤害: {dmg:F0} | 冷却: {cd:F1}s"
                : dur > 0f
                    ? $"持续: {dur:F1}s | 冷却: {cd:F1}s"
                    : $"冷却: {cd:F1}s";

            return (def.SkillName, def.Description, effectText);
        }

        /// <summary>
        /// 获取所有技能类型列表
        /// </summary>
        public IEnumerable<MimicrySkillType> GetAllSkillTypes()
        {
            return _skillsByType.Keys;
        }
    }
}
