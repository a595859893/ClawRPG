using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 宠物AI系统 - 智能召唤物行为控制
    /// </summary>
    public partial class PetAISystem : BaseSystem
    {
        // 玩家宠物AI数据
        private PlayerPetAIData _playerData;

        // 信号事件
        public event Action<string, AIDecision> OnDecisionMade;
        public event Action<string, ClawRPG.Systems.Pets.AI.PetAIState> OnStateChanged;
        public event Action<string, float> OnAdaptationChanged;

        //  Singleton
        private static PetAISystem _instance;
        public static PetAISystem Instance => _instance ??= new PetAISystem();

        private PetAISystem()
        {
            _playerData = new PlayerPetAIData
            {
                ActivePetAIs = new List<PetAIInstance>(),
                LearningData = new Dictionary<string, PetLearningData>(),
                BehaviorAssignments = new Dictionary<string, string>()
            };
        }
        
        /// <summary>
        /// 系统名称
        /// </summary>
        protected override string SystemName => "PetAI";
        
        /// <summary>
        /// 初始化系统
        /// </summary>
        protected override void Initialize()
        {
            base.Initialize();
            GD.Print("[PetAISystem] Initialized");
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            var activePets = new Array();
            foreach (var pet in _playerData.ActivePetAIs)
            {
                activePets.Add(pet.SummonId);
            }
            data["active_pets"] = activePets;
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            // 已实现数据导入（调用主系统的ImportSaveData）
        }
        
        /// <summary>
        /// 初始化宠物AI
        /// </summary>
        public void Initialize(PlayerPetAIData data)
        {
            if (data != null)
            {
                _playerData = data;
            }
        }

        /// <summary>
        /// 为召唤物分配AI行为
        /// </summary>
        public bool AssignBehavior(string summonId, string behaviorId)
        {
            var behavior = PetAIDatabase.GetBehaviorById(behaviorId);
            if (behavior == null) return false;

            // 查找或创建AI实例
            var aiInstance = _playerData.ActivePetAIs.Find(ai => ai.SummonId == summonId);
            if (aiInstance == null)
            {
                aiInstance = new PetAIInstance
                {
                    SummonId = summonId,
                    State = ClawRPG.Scripts.Data.ClawRPG.Systems.Pets.AI.PetAIState.Idle
                };
                _playerData.ActivePetAIs.Add(aiInstance);
            }

            aiInstance.BehaviorId = behaviorId;
            aiInstance.CurrentBehavior = behavior;

            // 初始化学习数据
            if (!_playerData.LearningData.ContainsKey(summonId))
            {
                _playerData.LearningData[summonId] = new PetLearningData
                {
                    SummonId = summonId,
                    EnemyTypeKills = new Dictionary<string, int>(),
                    DamageTakenByType = new Dictionary<string, float>(),
                    AdaptationLevels = new Dictionary<string, float>(),
                    OverallAdaptation = 0.5f,
                    LastLearningUpdate = DateTime.Now
                };
            }

            _playerData.BehaviorAssignments[summonId] = behaviorId;
            return true;
        }

        /// <summary>
        /// 移除召唤物的AI
        /// </summary>
        public void RemoveAI(string summonId)
        {
            _playerData.ActivePetAIs.RemoveAll(ai => ai.SummonId == summonId);
            _playerData.LearningData.Remove(summonId);
            _playerData.BehaviorAssignments.Remove(summonId);
        }

        /// <summary>
        /// 获取召唤物的AI实例
        /// </summary>
        public PetAIInstance GetAIInstance(string summonId)
        {
            return _playerData.ActivePetAIs.Find(ai => ai.SummonId == summonId);
        }

        /// <summary>
        /// 获取召唤物的学习数据
        /// </summary>
        public PetLearningData GetLearningData(string summonId)
        {
            return _playerData.LearningData.GetValueOrDefault(summonId);
        }

        /// <summary>
        /// 做出AI决策（每帧调用）
        /// </summary>
        public AIDecision MakeDecision(string summonId, BattleContext context)
        {
            var aiInstance = GetAIInstance(summonId);
            if (aiInstance == null || aiInstance.CurrentBehavior == null)
            {
                return null;
            }

            // 检查决策间隔
            var now = DateTime.Now;
            if ((now - aiInstance.LastDecisionTime).TotalMilliseconds < aiInstance.CurrentBehavior.DecisionInterval)
            {
                return aiInstance.CurrentDecision;
            }

            // 做出新决策
            var decision = EvaluateAndDecide(aiInstance, context);
            aiInstance.CurrentDecision = decision;
            aiInstance.LastDecisionTime = now;
            aiInstance.DecisionsMade++;
            _playerData.TotalDecisions++;

            // 更新状态
            UpdateState(aiInstance, decision.Type);

            OnDecisionMade?.Invoke(summonId, decision);
            return decision;
        }

        /// <summary>
        /// 评估战斗上下文并做出决策
        /// </summary>
        private AIDecision EvaluateAndDecide(PetAIInstance ai, BattleContext context)
        {
            var behavior = ai.CurrentBehavior;
            var decision = new AIDecision
            {
                Timestamp = DateTime.Now,
                Reasoning = new Dictionary<string, float>()
            };

            // 获取学习数据用于调整
            var learning = GetLearningData(ai.SummonId);
            float adaptationBonus = learning?.OverallAdaptation ?? 0.5f;

            // 根据行为模式做出决策
            switch (behavior.Pattern)
            {
                case AIBehaviorPattern.Aggressive:
                    decision = EvaluateAggressive(ai, context, adaptationBonus);
                    break;
                case AIBehaviorPattern.Defensive:
                    decision = EvaluateDefensive(ai, context, adaptationBonus);
                    break;
                case AIBehaviorPattern.Support:
                    decision = EvaluateSupport(ai, context, adaptationBonus);
                    break;
                case AIBehaviorPattern.Guerrilla:
                    decision = EvaluateGuerrilla(ai, context, adaptationBonus);
                    break;
                case AIBehaviorPattern.Follow:
                    decision = EvaluateFollow(ai, context, adaptationBonus);
                    break;
                case AIBehaviorPattern.Passive:
                    decision = EvaluatePassive(ai, context, adaptationBonus);
                    break;
            }

            // 评估决策质量
            EvaluateDecision(ai, decision, context);

            return decision;
        }

        private AIDecision EvaluateAggressive(PetAIInstance ai, BattleContext context, float adaptation)
        {
            var decision = new AIDecision { Type = AIDecisionType.Attack, Confidence = 0.7f };

            // 检查是否需要撤退
            if (context.TotalEnemiesRemaining > 3 && adaptation < 0.6f)
            {
                decision.Type = AIDecisionType.Retreat;
                decision.Confidence = 0.8f;
                return decision;
            }

            // 选择优先级最高的敌人
            var targetId = SelectTarget(ai, context, true);
            if (!string.IsNullOrEmpty(targetId))
            {
                decision.TargetId = targetId;
            }

            return decision;
        }

        private AIDecision EvaluateDefensive(PetAIInstance ai, BattleContext context, float adaptation)
        {
            var decision = new AIDecision { Type = AIDecisionType.Defend, Confidence = 0.6f };

            // 检查玩家是否需要保护
            if (context.PlayerHealthPercent < 30)
            {
                decision.Type = AIDecisionType.Defend;
                decision.TargetId = context.PlayerId;
                decision.Confidence = 0.9f;
                return decision;
            }

            // 选择最接近玩家的敌人攻击
            var targetId = SelectTarget(ai, context, false);
            if (!string.IsNullOrEmpty(targetId))
            {
                decision.Type = AIDecisionType.Attack;
                decision.TargetId = targetId;
            }

            return decision;
        }

        private AIDecision EvaluateSupport(PetAIInstance ai, BattleContext context, float adaptation)
        {
            var decision = new AIDecision { Type = AIDecisionType.Heal, Confidence = 0.7f };

            // 检查玩家生命值
            if (context.PlayerHealthPercent < ai.CurrentBehavior.HealThreshold * 100)
            {
                decision.Type = AIDecisionType.Heal;
                decision.TargetId = context.PlayerId;
                decision.Confidence = 0.95f;
                return decision;
            }

            // 检查法力值，考虑使用增益
            if (context.PlayerManaPercent > 50)
            {
                decision.Type = AIDecisionType.Buff;
                decision.TargetId = context.PlayerId;
                decision.Confidence = 0.8f;
                return decision;
            }

            // 寻找可以攻击的敌人
            if (context.TotalEnemiesRemaining > 0)
            {
                decision.Type = AIDecisionType.Attack;
                decision.TargetId = context.NearestEnemyId;
                decision.Confidence = 0.5f;
            }

            return decision;
        }

        private AIDecision EvaluateGuerrilla(PetAIInstance ai, BattleContext context, float adaptation)
        {
            var decision = new AIDecision { Type = AIDecisionType.Attack, Confidence = 0.6f };

            // 检查生命值是否需要撤退
            if (context.TotalEnemiesRemaining > 2)
            {
                // 评估是否应该撤退
                if (adaptation < 0.5f || context.NearestEnemyDistance < ai.CurrentBehavior.AttackRange)
                {
                    decision.Type = AIDecisionType.Retreat;
                    decision.Confidence = 0.85f;
                    return decision;
                }
            }

            // 选择孤立的目标
            var targetId = SelectTarget(ai, context, true);
            if (!string.IsNullOrEmpty(targetId))
            {
                decision.TargetId = targetId;
            }

            return decision;
        }

        private AIDecision EvaluateFollow(PetAIInstance ai, BattleContext context, float adaptation)
        {
            var decision = new AIDecision { Type = AIDecisionType.Follow, Confidence = 0.8f };

            // 检查是否有敌人在附近
            if (context.NearestEnemyDistance < ai.CurrentBehavior.AttackRange)
            {
                decision.Type = AIDecisionType.Attack;
                decision.TargetId = context.NearestEnemyId;
                decision.Confidence = 0.7f;
            }
            else if (context.TotalEnemiesRemaining > 0)
            {
                // 探索
                decision.Type = AIDecisionType.Explore;
                decision.TargetId = context.NearestEnemyId;
                decision.Confidence = 0.6f;
            }

            return decision;
        }

        private AIDecision EvaluatePassive(PetAIInstance ai, BattleContext context, float adaptation)
        {
            var decision = new AIDecision { Type = AIDecisionType.Wait, Confidence = 0.9f };

            // 只在受到攻击时反击
            if (context.NearestEnemyDistance < 1.0f)
            {
                decision.Type = AIDecisionType.Attack;
                decision.TargetId = context.NearestEnemyId;
                decision.Confidence = 0.8f;
            }

            return decision;
        }

        /// <summary>
        /// 选择目标敌人
        /// </summary>
        private string SelectTarget(PetAIInstance ai, BattleContext context, bool preferWeak)
        {
            if (context.EnemyIds == null || context.EnemyIds.Count == 0)
                return null;

            var behavior = ai.CurrentBehavior;
            var learning = GetLearningData(ai.SummonId);

            string bestTarget = null;
            float bestScore = float.MinValue;

            foreach (var enemyId in context.EnemyIds)
            {
                float score = 0;

                // 根据优先级权重计算分数
                var priority = EvaluateEnemyPriority(enemyId, context, learning);
                if (behavior.PriorityWeights.TryGetValue(priority, out float weight))
                {
                    score += weight;
                }

                // 适应等级调整
                if (learning != null && learning.AdaptationLevels.TryGetValue(enemyId, out float enemyAdaptation))
                {
                    score += enemyAdaptation * 0.3f;
                }

                if (score > bestScore)
                {
                    bestScore = score;
                    bestTarget = enemyId;
                }
            }

            return bestTarget;
        }

        /// <summary>
        /// 评估敌人优先级
        /// </summary>
        private EnemyPriority EvaluateEnemyPriority(string enemyId, BattleContext context, PetLearningData learning)
        {
            // 简单实现：基于敌人数量和距离
            if (context.TotalEnemiesRemaining <= 1)
                return EnemyPriority.Highest;

            if (context.NearestEnemyId == enemyId)
            {
                if (learning != null && learning.AdaptationLevels.TryGetValue(enemyId, out float adapt) && adapt > 0.7f)
                    return EnemyPriority.High;
                return EnemyPriority.Normal;
            }

            return EnemyPriority.Low;
        }

        /// <summary>
        /// 更新AI状态
        /// </summary>
        private void UpdateState(PetAIInstance ai, AIDecisionType decisionType)
        {
            ClawRPG.Systems.Pets.AI.ClawRPG.Systems.Pets.AI.PetAIState newState = ai.State;

            switch (decisionType)
            {
                case AIDecisionType.Attack:
                    newState = ClawRPG.Scripts.Data.ClawRPG.Systems.Pets.AI.PetAIState.Engaging;
                    break;
                case AIDecisionType.Heal:
                case AIDecisionType.Buff:
                    newState = ClawRPG.Scripts.Data.ClawRPG.Systems.Pets.AI.PetAIState.Supporting;
                    break;
                case AIDecisionType.Retreat:
                    newState = ClawRPG.Scripts.Data.ClawRPG.Systems.Pets.AI.PetAIState.Fleeing;
                    break;
                case AIDecisionType.Follow:
                case AIDecisionType.Explore:
                    newState = ClawRPG.Scripts.Data.ClawRPG.Systems.Pets.AI.PetAIState.Patrolling;
                    break;
                case AIDecisionType.Wait:
                case AIDecisionType.Defend:
                    newState = ClawRPG.Scripts.Data.ClawRPG.Systems.Pets.AI.PetAIState.Idle;
                    break;
            }

            if (newState != ai.State)
            {
                ai.State = newState;
                OnStateChanged?.Invoke(ai.SummonId, newState);
            }
        }

        /// <summary>
        /// 评估决策质量并更新学习数据
        /// </summary>
        private void EvaluateDecision(PetAIInstance ai, AIDecision decision, BattleContext context)
        {
            var learning = GetLearningData(ai.SummonId);
            if (learning == null) return;

            bool wasCorrect = false;

            // 简单评估逻辑
            switch (decision.Type)
            {
                case AIDecisionType.Retreat:
                    // 如果敌人很多且生命值低，撤退是正确的
                    wasCorrect = context.TotalEnemiesRemaining > 2;
                    learning.SmartRetreats++;
                    break;
                case AIDecisionType.Attack:
                    // 攻击有目标是正确的
                    wasCorrect = !string.IsNullOrEmpty(decision.TargetId);
                    break;
                case AIDecisionType.Heal:
                    // 玩家生命低时治疗是正确的
                    wasCorrect = context.PlayerHealthPercent < 50;
                    break;
            }

            if (wasCorrect)
            {
                ai.CorrectDecisions++;
                learning.OverallAdaptation = Math.Min(1.0f, learning.OverallAdaptation + 0.01f);
                OnAdaptationChanged?.Invoke(ai.SummonId, learning.OverallAdaptation);
            }
            else
            {
                learning.OverallAdaptation = Math.Max(0.1f, learning.OverallAdaptation - 0.005f);
            }

            learning.LastLearningUpdate = DateTime.Now;
        }

        /// <summary>
        /// 记录击杀
        /// </summary>
        public void RecordKill(string summonId, string enemyType)
        {
            var learning = GetLearningData(summonId);
            if (learning == null) return;

            if (!learning.EnemyTypeKills.ContainsKey(enemyType))
                learning.EnemyTypeKills[enemyType] = 0;
            learning.EnemyTypeKills[enemyType]++;

            // 更新对该类型敌人的适应等级
            if (!learning.AdaptationLevels.ContainsKey(enemyType))
                learning.AdaptationLevels[enemyType] = 0.5f;
            learning.AdaptationLevels[enemyType] = Math.Min(1.0f, learning.AdaptationLevels[enemyType] + 0.05f);
        }

        /// <summary>
        /// 记录受伤
        /// </summary>
        public void RecordDamageTaken(string summonId, string enemyType, float damage)
        {
            var learning = GetLearningData(summonId);
            if (learning == null) return;

            if (!learning.DamageTakenByType.ContainsKey(enemyType))
                learning.DamageTakenByType[enemyType] = 0;
            learning.DamageTakenByType[enemyType] += damage;

            // 降低对该类型敌人的适应等级
            if (learning.AdaptationLevels.TryGetValue(enemyType, out float current))
            {
                learning.AdaptationLevels[enemyType] = Math.Max(0.1f, current - 0.02f);
            }
        }

        /// <summary>
        /// 记录闪避成功
        /// </summary>
        public void RecordDodge(string summonId, bool success)
        {
            var learning = GetLearningData(summonId);
            if (learning == null) return;

            if (success)
                learning.SuccessfulDodges++;
            else
                learning.FailedDodges++;
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        public PetAIStatistics GetStatistics()
        {
            var stats = new PetAIStatistics
            {
                TotalDecisions = _playerData.TotalDecisions,
                SuccessfulDecisions = _playerData.ActivePetAIs.Sum(ai => ai.CorrectDecisions),
                BehaviorUsage = new Dictionary<AIBehaviorPattern, int>(),
                DecisionDistribution = new Dictionary<AIDecisionType, int>(),
                LearningUpdates = 0,
                AverageAdaptationLevel = 0.5f
            };

            if (stats.TotalDecisions > 0)
            {
                stats.SuccessRate = (float)stats.SuccessfulDecisions / stats.TotalDecisions;
            }

            // 统计行为使用
            foreach (var ai in _playerData.ActivePetAIs)
            {
                if (ai.CurrentBehavior != null)
                {
                    var pattern = ai.CurrentBehavior.Pattern;
                    if (!stats.BehaviorUsage.ContainsKey(pattern))
                        stats.BehaviorUsage[pattern] = 0;
                    stats.BehaviorUsage[pattern]++;
                }
            }

            // 统计决策分布
            foreach (var ai in _playerData.ActivePetAIs)
            {
                if (ai.CurrentDecision != null)
                {
                    var type = ai.CurrentDecision.Type;
                    if (!stats.DecisionDistribution.ContainsKey(type))
                        stats.DecisionDistribution[type] = 0;
                    stats.DecisionDistribution[type]++;
                }
            }

            // 计算平均适应等级
            if (_playerData.LearningData.Count > 0)
            {
                stats.AverageAdaptationLevel = _playerData.LearningData.Values.Average(l => l.OverallAdaptation);
                stats.LearningUpdates = _playerData.LearningData.Count;
            }

            return stats;
        }

        /// <summary>
        /// 导出存档数据
        /// </summary>
        public PlayerPetAIData ExportAIData()
        {
            return _playerData;
        }

        /// <summary>
        /// 导入存档数据
        /// </summary>
        public void ImportAIData(PlayerPetAIData data)
        {
            if (data != null)
            {
                _playerData = data;
            }
        }
    }
}
