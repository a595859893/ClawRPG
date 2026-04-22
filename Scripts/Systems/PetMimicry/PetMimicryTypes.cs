namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 玩家行为类型 — 由 PetBehaviorLogger 识别并映射为宠物技能
    /// </summary>
    public enum PlayerBehaviorType
    {
        None = 0,

        // 技能使用类
        UseFireSkill,
        UseIceSkill,
        UseElectricSkill,
        UseShadowSkill,
        UseHolySkill,
        UseNatureSkill,

        // 战斗风格类
        AggressiveAttack,
        DefensiveStance,
        FrequentDodge,
        LowHPAggression,
        QuickRetreat,

        // 战术选择类
        FocusElite,
        AvoidCombat,
        TriggerTrap,

        // 探索行为类
        SolvePuzzle,
        CollectLoot,

        // 互动类
        UseHealing,
        PetSynergy,
        SpecialInteraction,
    }

    /// <summary>
    /// 房间环境类型 — 用于房间加成和环境模拟
    /// </summary>
    [System.Flags]
    public enum RoomEnvironmentType
    {
        None = 0,
        TrapDense = 1 << 0,
        Puzzle = 1 << 1,
        Escape = 1 << 2,
        CombatHeavy = 1 << 3,
        LootRich = 1 << 4,
        Boss = 1 << 5,
        Treasure = 1 << 6,
    }

    /// <summary>
    /// 个性触发器类型 — 决定宠物个性分析的维度
    /// </summary>
    public enum PersonalityTriggerType
    {
        None,
        HPRelated,
        EnvironmentSpecialist,
        EventDriven,
        RecentBias,
        Suppressed,
    }

    /// <summary>
    /// 行为印记 — 记录宠物对特定行为的熟悉度
    /// </summary>
    public class BehaviorImprint
    {
        public PlayerBehaviorType BehaviorType { get; set; }
        public RoomEnvironmentType EnvironmentType { get; set; }
        public float Xp { get; set; }
        public int ImprintLevel { get; set; }
        public float DecayTimer { get; set; }
        public DateTime LastRecordedAt { get; set; }
        public int TotalTriggers { get; set; }
        public float Fidelity { get; set; }

        public BehaviorImprint() { }

        public BehaviorImprint(PlayerBehaviorType behavior, RoomEnvironmentType envType)
        {
            BehaviorType = behavior;
            EnvironmentType = envType;
            Xp = 0f;
            ImprintLevel = 0;
            DecayTimer = 0f;
            LastRecordedAt = DateTime.Now;
            TotalTriggers = 0;
            Fidelity = 0.5f;
        }
    }

    /// <summary>
    /// 个性分析结果 — 宠物个性分析算法的输出
    /// </summary>
    public class PersonalityAnalysisResult
    {
        public PlayerBehaviorType DominantBehavior { get; set; }
        public float DominantScore { get; set; }
        public PersonalityTriggerType DominantTriggerType { get; set; }
        public List<PlayerBehaviorType> SecondaryBehaviors { get; set; } = new List<PlayerBehaviorType>();
        public string Description { get; set; } = string.Empty;
        public List<PersonalityTrigger> ActiveTriggers { get; set; } = new List<PersonalityTrigger>();
        public Dictionary<PlayerBehaviorType, float> AllScores { get; set; } = new Dictionary<PlayerBehaviorType, float>();
    }

    /// <summary>
    /// 个性触发器 — 将触发类型与行为关联
    /// </summary>
    public class PersonalityTrigger
    {
        public PersonalityTriggerType TriggerType { get; private set; }
        public PlayerBehaviorType Behavior { get; private set; }
        public float Weight { get; private set; }
        public bool IsActive { get; private set; }
        public string Reason { get; private set; }

        public PersonalityTrigger(PersonalityTriggerType triggerType, PlayerBehaviorType behavior, float weight = 1.0f, bool isActive = true, string reason = null)
        {
            TriggerType = triggerType;
            Behavior = behavior;
            Weight = weight;
            IsActive = isActive;
            Reason = reason ?? string.Empty;
        }
    }
}
