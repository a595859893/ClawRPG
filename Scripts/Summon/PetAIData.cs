using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// AI行为模式
    /// </summary>
    public enum AIBehaviorPattern
    {
        Aggressive,      // 主动攻击 - 优先攻击最弱敌人
        Defensive,      // 防守 - 保护玩家
        Support,        // 支援 - 治疗和增益
        Guerrilla,      // 游击 - 打了就跑
        Follow,         // 跟随 - 紧跟玩家
        Passive         // 被动 - 仅在受到攻击时反击
    }

    /// <summary>
    /// AI决策类型
    /// </summary>
    public enum AIDecisionType
    {
        Attack,         // 攻击
        Defend,         // 防守
        Heal,           // 治疗
        Buff,           // 增益
        Debuff,         // 减益
        Retreat,        // 撤退
        Follow,         // 跟随
        Explore,        // 探索
        Wait            // 等待
    }

    /// <summary>
    /// 敌人优先级
    /// </summary>
    public enum EnemyPriority
    {
        Lowest,         // 最低
        Low,            // 低
        Normal,         // 普通
        High,           // 高
        Highest         // 最高
    }

    /// <summary>
    /// 宠物AI状态
    /// </summary>
    public enum SummonPetAIState
    {
        Idle,           // 空闲
        Patrolling,     // 巡逻
        Engaging,       // 战斗中
        Supporting,     // 支援中
        Fleeing,       // 撤退中
        Learning       // 学习适应
    }

    /// <summary>
    /// AI行为配置
    /// </summary>
    public class AIBehavior
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AIBehaviorPattern Pattern { get; set; }
        public float AttackRange { get; set; }
        public float RetreatHealthPercent { get; set; }
        public float HealThreshold { get; set; }
        public int DecisionInterval { get; set; }
        public float aggressionLevel { get; set; }
        public List<string> PreferredTargets { get; set; }
        public Dictionary<EnemyPriority, float> PriorityWeights { get; set; }
    }

    /// <summary>
    /// AI决策记录
    /// </summary>
    public class AIDecision
    {
        public AIDecisionType Type { get; set; }
        public string TargetId { get; set; }
        public float Confidence { get; set; }
        public DateTime Timestamp { get; set; }
        public Dictionary<string, float> Reasoning { get; set; }
    }

    /// <summary>
    /// 宠物学习数据
    /// </summary>
    public partial class PetLearningData
    {
        public string SummonId { get; set; }
        public Dictionary<string, int> EnemyTypeKills { get; set; }
        public Dictionary<string, int> DamageTakenByType { get; set; }
        public Dictionary<string, float> AdaptationLevels { get; set; }
        public int SuccessfulDodges { get; set; }
        public int FailedDodges { get; set; }
        public int SmartRetreats { get; set; }
        public int Overextensions { get; set; }
        public float OverallAdaptation { get; set; }
        public DateTime LastLearningUpdate { get; set; }
    }

    /// <summary>
    /// 宠物AI实例数据
    /// </summary>
    public class PetAIInstance
    {
        public string SummonId { get; set; }
        public string BehaviorId { get; set; }
        public PetAIState State { get; set; }
        public AIBehavior CurrentBehavior { get; set; }
        public AIDecision CurrentDecision { get; set; }
        public DateTime LastDecisionTime { get; set; }
        public string CurrentTargetId { get; set; }
        public float PositionX { get; set; }
        public float PositionY { get; set; }
        public float TargetPositionX { get; set; }
        public float TargetPositionY { get; set; }
        public int DecisionsMade { get; set; }
        public int CorrectDecisions { get; set; }
    }

    /// <summary>
    /// 宠物AI玩家数据
    /// </summary>
    public partial class PlayerPetAIData
    {
        public List<PetAIInstance> ActivePetAIs { get; set; }
        public Dictionary<string, PetLearningData> LearningData { get; set; }
        public Dictionary<string, string> BehaviorAssignments { get; set; }
        public int TotalDecisions { get; set; }
        public int SuccessfulDecisions { get; set; }
    }

    /// <summary>
    /// 战斗上下文信息
    /// </summary>
    public class BattleContext
    {
        public List<string> EnemyIds { get; set; }
        public string PlayerId { get; set; }
        public int PlayerHealthPercent { get; set; }
        public int PlayerManaPercent { get; set; }
        public float NearestEnemyDistance { get; set; }
        public string NearestEnemyId { get; set; }
        public int TotalEnemiesRemaining { get; set; }
        public int AlliesInRange { get; set; }
    }

    /// <summary>
    /// 宠物AI统计
    /// </summary>
    public class PetAIStatistics
    {
        public int TotalDecisions { get; set; }
        public int SuccessfulDecisions { get; set; }
        public float SuccessRate { get; set; }
        public Dictionary<AIBehaviorPattern, int> BehaviorUsage { get; set; }
        public Dictionary<AIDecisionType, int> DecisionDistribution { get; set; }
        public int LearningUpdates { get; set; }
        public float AverageAdaptationLevel { get; set; }
    }
}
