using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 宠物AI行为数据库
    /// </summary>
    public static class PetAIDatabase
    {
        // 预配置的AI行为
        public static List<AIBehavior> Behaviors { get; private set; }

        static PetAIDatabase()
        {
            InitializeBehaviors();
        }

        private static void InitializeBehaviors()
        {
            Behaviors = new List<AIBehavior>
            {
                // 主动攻击型
                new AIBehavior
                {
                    Id = "ai_aggressive_1",
                    Name = "狂战士",
                    Description = "优先攻击最弱的敌人，激进进攻",
                    Pattern = AIBehaviorPattern.Aggressive,
                    AttackRange = 3.0f,
                    RetreatHealthPercent = 0.1f,
                    HealThreshold = 0.3f,
                    DecisionInterval = 500,
                    aggressionLevel = 0.9f,
                    PreferredTargets = new List<string> { "low_health", "caster" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.8f },
                        { EnemyPriority.Low, 0.6f },
                        { EnemyPriority.Normal, 0.4f },
                        { EnemyPriority.High, 0.2f },
                        { EnemyPriority.Highest, 0.1f }
                    }
                },
                new AIBehavior
                {
                    Id = "ai_aggressive_2",
                    Name = "战斗机",
                    Description = "优先攻击高威胁目标，保持进攻",
                    Pattern = AIBehaviorPattern.Aggressive,
                    AttackRange = 2.5f,
                    RetreatHealthPercent = 0.15f,
                    HealThreshold = 0.25f,
                    DecisionInterval = 600,
                    aggressionLevel = 0.8f,
                    PreferredTargets = new List<string> { "boss", "elite" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.1f },
                        { EnemyPriority.Low, 0.2f },
                        { EnemyPriority.Normal, 0.3f },
                        { EnemyPriority.High, 0.6f },
                        { EnemyPriority.Highest, 0.9f }
                    }
                },

                // 防守型
                new AIBehavior
                {
                    Id = "ai_defensive_1",
                    Name = "守卫者",
                    Description = "保护玩家，优先攻击靠近玩家的敌人",
                    Pattern = AIBehaviorPattern.Defensive,
                    AttackRange = 4.0f,
                    RetreatHealthPercent = 0.25f,
                    HealThreshold = 0.5f,
                    DecisionInterval = 400,
                    aggressionLevel = 0.4f,
                    PreferredTargets = new List<string> { "approaching_player" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.3f },
                        { EnemyPriority.Low, 0.4f },
                        { EnemyPriority.Normal, 0.5f },
                        { EnemyPriority.High, 0.7f },
                        { EnemyPriority.Highest, 0.9f }
                    }
                },
                new AIBehavior
                {
                    Id = "ai_defensive_2",
                    Name = "盾卫",
                    Description = "保持防守姿态，只在必要时反击",
                    Pattern = AIBehaviorPattern.Defensive,
                    AttackRange = 2.0f,
                    RetreatHealthPercent = 0.35f,
                    HealThreshold = 0.6f,
                    DecisionInterval = 800,
                    aggressionLevel = 0.3f,
                    PreferredTargets = new List<string> { "attacking" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.4f },
                        { EnemyPriority.Low, 0.5f },
                        { EnemyPriority.Normal, 0.6f },
                        { EnemyPriority.High, 0.7f },
                        { EnemyPriority.Highest, 0.8f }
                    }
                },

                // 支援型
                new AIBehavior
                {
                    Id = "ai_support_1",
                    Name = "治疗者",
                    Description = "优先治疗和增益玩家",
                    Pattern = AIBehaviorPattern.Support,
                    AttackRange = 5.0f,
                    RetreatHealthPercent = 0.2f,
                    HealThreshold = 0.7f,
                    DecisionInterval = 300,
                    aggressionLevel = 0.3f,
                    PreferredTargets = new List<string> { "injured_ally" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.5f },
                        { EnemyPriority.Low, 0.5f },
                        { EnemyPriority.Normal, 0.5f },
                        { EnemyPriority.High, 0.5f },
                        { EnemyPriority.Highest, 0.5f }
                    }
                },
                new AIBehavior
                {
                    Id = "ai_support_2",
                    Name = "辅助者",
                    Description = "提供持续支援和buff",
                    Pattern = AIBehaviorPattern.Support,
                    AttackRange = 4.5f,
                    RetreatHealthPercent = 0.25f,
                    HealThreshold = 0.65f,
                    DecisionInterval = 350,
                    aggressionLevel = 0.35f,
                    PreferredTargets = new List<string> { "injured_ally", "buff_candidate" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.5f },
                        { EnemyPriority.Low, 0.5f },
                        { EnemyPriority.Normal, 0.5f },
                        { EnemyPriority.High, 0.5f },
                        { EnemyPriority.Highest, 0.5f }
                    }
                },

                // 游击型
                new AIBehavior
                {
                    Id = "ai_guerrilla_1",
                    Name = "游侠",
                    Description = "打了就跑，保持移动",
                    Pattern = AIBehaviorPattern.Guerrilla,
                    AttackRange = 5.0f,
                    RetreatHealthPercent = 0.4f,
                    HealThreshold = 0.45f,
                    DecisionInterval = 450,
                    aggressionLevel = 0.6f,
                    PreferredTargets = new List<string> { "isolated", "low_health" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.6f },
                        { EnemyPriority.Low, 0.5f },
                        { EnemyPriority.Normal, 0.4f },
                        { EnemyPriority.High, 0.3f },
                        { EnemyPriority.Highest, 0.2f }
                    }
                },
                new AIBehavior
                {
                    Id = "ai_guerrilla_2",
                    Name = "刺客",
                    Description = "快速袭击高价值目标后撤退",
                    Pattern = AIBehaviorPattern.Guerrilla,
                    AttackRange = 6.0f,
                    RetreatHealthPercent = 0.35f,
                    HealThreshold = 0.4f,
                    DecisionInterval = 500,
                    aggressionLevel = 0.7f,
                    PreferredTargets = new List<string> { "caster", "healer" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.2f },
                        { EnemyPriority.Low, 0.3f },
                        { EnemyPriority.Normal, 0.4f },
                        { EnemyPriority.High, 0.7f },
                        { EnemyPriority.Highest, 0.8f }
                    }
                },

                // 跟随型
                new AIBehavior
                {
                    Id = "ai_follow_1",
                    Name = "伴侣",
                    Description = "紧跟玩家，提供即时支援",
                    Pattern = AIBehaviorPattern.Follow,
                    AttackRange = 3.0f,
                    RetreatHealthPercent = 0.3f,
                    HealThreshold = 0.5f,
                    DecisionInterval = 600,
                    aggressionLevel = 0.5f,
                    PreferredTargets = new List<string> { "approaching_player" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.4f },
                        { EnemyPriority.Low, 0.5f },
                        { EnemyPriority.Normal, 0.6f },
                        { EnemyPriority.High, 0.7f },
                        { EnemyPriority.Highest, 0.8f }
                    }
                },

                // 被动型
                new AIBehavior
                {
                    Id = "ai_passive_1",
                    Name = "宠物",
                    Description = "温顺的宠物，只在受到攻击时反击",
                    Pattern = AIBehaviorPattern.Passive,
                    AttackRange = 1.5f,
                    RetreatHealthPercent = 0.5f,
                    HealThreshold = 0.4f,
                    DecisionInterval = 1000,
                    aggressionLevel = 0.1f,
                    PreferredTargets = new List<string> { "attacking_me" },
                    PriorityWeights = new Dictionary<EnemyPriority, float>
                    {
                        { EnemyPriority.Lowest, 0.5f },
                        { EnemyPriority.Low, 0.5f },
                        { EnemyPriority.Normal, 0.5f },
                        { EnemyPriority.High, 0.5f },
                        { EnemyPriority.Highest, 0.5f }
                    }
                }
            };
        }

        /// <summary>
        /// 获取所有行为
        /// </summary>
        public static List<AIBehavior> GetAllBehaviors()
        {
            return new List<AIBehavior>(Behaviors);
        }

        /// <summary>
        /// 根据ID获取行为
        /// </summary>
        public static AIBehavior GetBehaviorById(string id)
        {
            return Behaviors.Find(b => b.Id == id);
        }

        /// <summary>
        /// 根据模式获取行为
        /// </summary>
        public static List<AIBehavior> GetBehaviorsByPattern(AIBehaviorPattern pattern)
        {
            return Behaviors.FindAll(b => b.Pattern == pattern);
        }

        /// <summary>
        /// 获取推荐行为（基于召唤物类型）
        /// </summary>
        public static List<AIBehavior> GetRecommendedBehaviors(string summonType)
        {
            var recommendations = new List<AIBehavior>();
            
            switch (summonType.ToLower())
            {
                case "elemental":
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Aggressive));
                    break;
                case "spirit":
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Support));
                    break;
                case "construct":
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Defensive));
                    break;
                case "beast":
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Guerrilla));
                    break;
                case "celestial":
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Support));
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Defensive));
                    break;
                case "demon":
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Aggressive));
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Guerrilla));
                    break;
                case "undead":
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Aggressive));
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Passive));
                    break;
                case "divine":
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Support));
                    recommendations.AddRange(GetBehaviorsByPattern(AIBehaviorPattern.Follow));
                    break;
                default:
                    recommendations.AddRange(Behaviors);
                    break;
            }
            
            return recommendations;
        }
    }
}
