using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets.AI
{
    /// <summary>
    /// 敌对观察者系统 - 战略批评家（REQ-138）
    /// 职责：独立评估玩家策略，发现偏差时以叙事方式提问而非纠正
    /// 数据来源：PlayerGoalTracker + ObserverWorldModel + TrajectoryPredictor
    /// </summary>
    public class AdversarialObserverState
    {
        // ===== Observer 世界模型 =====
        
        /// <summary>
        /// Observer 状态快照（用于持久化）
        /// </summary>
        public ObserverPersistentState PersistentState { get; set; } = new ObserverPersistentState();
        
        /// <summary>
        /// Observer 当前对战斗局势的评估
        /// </summary>
        public WorldAssessment CurrentAssessment { get; set; } = new WorldAssessment();
        
        /// <summary>
        /// 当前检测到的分歧（如果有）
        /// </summary>
        public DisagreementRecord ActiveDisagreement { get; set; }
        
        /// <summary>
        /// 冷却计时器（防止频繁发声）
        /// </summary>
        public float SilenceCooldown { get; set; } = 0f;
        
        /// <summary>
        /// 是否被玩家禁用
        /// </summary>
        public bool IsDisabled { get; set; } = false;
    }
    
    /// <summary>
    /// Observer 持久化状态（跨 session 保留）
    /// </summary>
    public class ObserverPersistentState
    {
        /// <summary>
        /// Observer 对玩家目标的推断（自然语言描述）
        /// </summary>
        public string PlayerGoalModel { get; set; } = "探索中";
        
        /// <summary>
        /// Observer 对自己预测的置信度（0-1）
        /// </summary>
        public float Confidence { get; set; } = 0.5f;
        
        /// <summary>
        /// 玩家声明的目标（与推断的目标区分）
        /// </summary>
        public string DeclaredGoal { get; set; } = "";
        
        /// <summary>
        /// Observer 历史预测准确度（用于"我可能错了"承认）
        /// </summary>
        public int PredictionSuccessCount { get; set; } = 0;
        public int PredictionFailureCount { get; set; } = 0;
        
        /// <summary>
        /// 已完成的挑战记录（用于去重）
        /// </summary>
        public HashSet<string> IssuedChallengeSignatures { get; set; } = new HashSet<string>();
    }
    
    /// <summary>
    /// 世界局势评估（Observer 独立模型）
    /// </summary>
    public class WorldAssessment
    {
        /// <summary>
        /// 当前敌人数量
        /// </summary>
        public int EnemyCount { get; set; } = 0;
        
        /// <summary>
        /// 玩家血量百分比（0-1）
        /// </summary>
        public float PlayerHealthPercent { get; set; } = 1.0f;
        
        /// <summary>
        /// 玩家附近敌人数量
        /// </summary>
        public int NearbyEnemyCount { get; set; } = 0;
        
        /// <summary>
        /// 玩家当前状态描述
        /// </summary>
        public string PlayerStatus { get; set; } = "正常";
        
        /// <summary>
        /// 环境威胁描述
        /// </summary>
        public string ThreatLevel { get; set; } = "低";
        
        /// <summary>
        /// 玩家当前动作类型
        /// </summary>
        public string CurrentAction { get; set; } = "未知";
        
        /// <summary>
        /// 玩家位置
        /// </summary>
        public Vector2 PlayerPosition { get; set; } = Vector2.Zero;
        
        /// <summary>
        /// 关键敌人位置
        /// </summary>
        public Vector2 KeyEnemyPosition { get; set; } = Vector2.Zero;
    }
    
    /// <summary>
    /// 分歧记录
    /// </summary>
    public class DisagreementRecord
    {
        public int TickId { get; set; }
        public float Timestamp { get; set; }
        
        /// <summary>
        /// 分歧类型
        /// </summary>
        public DisagreementType Type { get; set; }
        
        /// <summary>
        /// 玩家当前/计划动作
        /// </summary>
        public string PlayerAction { get; set; } = "";
        
        /// <summary>
        /// Observer 的预测
        /// </summary>
        public string ObserverPrediction { get; set; } = "";
        
        /// <summary>
        /// 向玩家提问的叙事化问题
        /// </summary>
        public string QuestionPrompt { get; set; } = "";
        
        /// <summary>
        /// Observer 置信度（只在 > threshold 时发声）
        /// </summary>
        public float Confidence { get; set; }
        
        /// <summary>
        /// 是否已向玩家展示
        /// </summary>
        public bool IsDisplayed { get; set; } = false;
    }
    
    /// <summary>
    /// 分歧类型枚举
    /// </summary>
    public enum DisagreementType
    {
        /// <summary>玩家目标漂移（优化方向偏离）</summary>
        GoalDrift,
        
        /// <summary>错过进攻/逃跑机会</summary>
        MissedOpportunity,
        
        /// <summary>忽视威胁</summary>
        ThreatIgnored,
        
        /// <summary>资源与行动不匹配</summary>
        ResourceMismatch
    }
    
    /// <summary>
    /// 玩家动作记录（用于目标推断）
    /// </summary>
    public class PlayerActionRecord
    {
        public int TickId { get; set; }
        public float Timestamp { get; set; }
        public string ActionType { get; set; } = "";   // "attack" / "heal" / "move" / "retreat" / "collect"
        public string ActionTarget { get; set; } = "";  // "enemy_nearby" / "health_pack" / "exit" / ""
        public Vector2 Position { get; set; } = Vector2.Zero;
        public float HealthPercent { get; set; } = 1.0f;
        public float DamageDealt { get; set; } = 0f;
        public float DamageTaken { get; set; } = 0f;
    }
    
    /// <summary>
    /// 玩家目标推断
    /// </summary>
    public class PlayerGoalInference
    {
        /// <summary>
        /// 推断出的目标类型
        /// </summary>
        public string GoalType { get; set; } = "探索";
        
        /// <summary>
        /// 目标描述（自然语言）
        /// </summary>
        public string GoalDescription { get; set; } = "";
        
        /// <summary>
        /// 置信度
        /// </summary>
        public float Confidence { get; set; } = 0.5f;
        
        /// <summary>
        /// 支持此推断的动作数量
        /// </summary>
        public int SupportingActionCount { get; set; } = 0;
    }
    
    /// <summary>
    /// 轨迹预测记录
    /// </summary>
    public class TrajectoryPrediction
    {
        /// <summary>
        /// 基于当前动作预测的未来路线描述
        /// </summary>
        public string PredictedTrajectory { get; set; } = "";
        
        /// <summary>
        /// 预测目的地
        /// </summary>
        public string Destination { get; set; } = "";
        
        /// <summary>
        /// 预测准确度（事后验证）
        /// </summary>
        public float Accuracy { get; set; } = 1.0f;
        
        /// <summary>
        /// 是否已被验证
        /// </summary>
        public bool IsVerified { get; set; } = false;
    }
    
    /// <summary>
    /// Observer 挑战（提问）
    /// </summary>
    public class ObserverChallenge
    {
        public int TickId { get; set; }
        public DisagreementType Type { get; set; }
        public string PlayerAction { get; set; } = "";
        public string ObserverPrediction { get; set; } = "";
        public string QuestionPrompt { get; set; } = "";
        public float Confidence { get; set; }
        public bool WasCorrect { get; set; } = false;  // 事后标记
    }
}
