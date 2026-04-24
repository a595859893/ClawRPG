using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物决策记录数据结构 - 记录每个决策周期的详细信息（REQ-137）
    /// 用于回放、调试和叙事系统集成
    /// </summary>
    public class PetDecisionRecord
    {
        /// <summary>全局决策 Tick ID（与 PetDecisionSystem.NextDecisionTick() 同步）</summary>
        public int TickId { get; set; }

        /// <summary>决策时间戳（秒）</summary>
        public float Timestamp { get; set; }

        /// <summary>决策类型</summary>
        public DecisionType Type { get; set; }

        /// <summary>宠物当前状态</summary>
        public PetDecisionSystem.PetAIState StateBefore { get; set; }

        /// <summary>决策后状态</summary>
        public PetDecisionSystem.PetAIState StateAfter { get; set; }

        /// <summary>目标敌人（若有）</summary>
        public string TargetName { get; set; }

        /// <summary>目标距离</summary>
        public float TargetDistance { get; set; }

        /// <summary>决策理由（可读字符串）</summary>
        public string Reason { get; set; }

        /// <summary>执行结果</summary>
        public DecisionOutcome Outcome { get; set; }

        /// <summary>决策置信度</summary>
        public float Confidence { get; set; }

        /// <summary>
        /// 决策类型枚举
        /// </summary>
        public enum DecisionType
        {
            StateTransition,  // 状态切换
            TargetSelection,   // 目标选择
            BehaviorExecution // 行为执行
        }

        /// <summary>
        /// 决策结果枚举
        /// </summary>
        public enum DecisionOutcome
        {
            Unknown,     // 未知
            Success,     // 成功
            Failure,     // 失败
            Cancelled   // 被取消
        }

        /// <summary>
        /// 工厂方法：创建状态切换记录
        /// </summary>
        public static PetDecisionRecord CreateStateTransition(int tickId, float timestamp,
            PetDecisionSystem.PetAIState before, PetDecisionSystem.PetAIState after, string reason)
        {
            return new PetDecisionRecord
            {
                TickId = tickId,
                Timestamp = timestamp,
                Type = DecisionType.StateTransition,
                StateBefore = before,
                StateAfter = after,
                Reason = reason,
                Outcome = DecisionOutcome.Unknown,
                Confidence = 1.0f
            };
        }

        /// <summary>
        /// 工厂方法：创建目标选择记录
        /// </summary>
        public static PetDecisionRecord CreateTargetSelection(int tickId, float timestamp,
            Node2D target, float distance, string reason)
        {
            return new PetDecisionRecord
            {
                TickId = tickId,
                Timestamp = timestamp,
                Type = DecisionType.TargetSelection,
                StateBefore = PetDecisionSystem.PetAIState.Idle,
                StateAfter = PetDecisionSystem.PetAIState.Engaging,
                TargetName = target?.Name ?? "null",
                TargetDistance = distance,
                Reason = reason,
                Outcome = DecisionOutcome.Unknown,
                Confidence = 0.85f
            };
        }

        /// <summary>
        /// 工厂方法：创建行为执行记录
        /// </summary>
        public static PetDecisionRecord CreateBehaviorExecution(int tickId, float timestamp,
            PetDecisionSystem.PetAIState state, string reason)
        {
            return new PetDecisionRecord
            {
                TickId = tickId,
                Timestamp = timestamp,
                Type = DecisionType.BehaviorExecution,
                StateBefore = state,
                StateAfter = state,
                Reason = reason,
                Outcome = DecisionOutcome.Unknown,
                Confidence = 0.9f
            };
        }

        /// <summary>
        /// 获取可读字符串（用于 UI 和日志）
        /// </summary>
        public string ToReadableString()
        {
            string outcomeIcon = Outcome switch
            {
                DecisionOutcome.Success => "✓",
                DecisionOutcome.Failure => "✗",
                DecisionOutcome.Cancelled => "○",
                _ => "?"
            };

            string typeStr = Type switch
            {
                DecisionType.StateTransition => $"[{TickId}] 状态切换 {StateBefore}→{StateAfter}",
                DecisionType.TargetSelection => $"[{TickId}] 目标选择: {TargetName} ({TargetDistance:F0}px)",
                DecisionType.BehaviorExecution => $"[{TickId}] 行为执行: {StateAfter}",
                _ => $"[{TickId}] 未知决策"
            };

            return $"{outcomeIcon} {typeStr} | {Reason}";
        }

        /// <summary>
        /// 序列化（用于持久化）
        /// </summary>
        public Dictionary ToDictionary()
        {
            return new Dictionary
            {
                { "tickId", TickId },
                { "timestamp", Timestamp },
                { "type", (int)Type },
                { "stateBefore", (int)StateBefore },
                { "stateAfter", (int)StateAfter },
                { "targetName", TargetName ?? "" },
                { "targetDistance", TargetDistance },
                { "reason", Reason ?? "" },
                { "outcome", (int)Outcome },
                { "confidence", Confidence }
            };
        }

        /// <summary>
        /// 反序列化（用于加载）
        /// </summary>
        public static PetDecisionRecord FromDictionary(Dictionary data)
        {
            if (data == null) return null;

            return new PetDecisionRecord
            {
                TickId = data.ContainsKey("tickId") ? Convert.ToInt32(data["tickId"]) : 0,
                Timestamp = data.ContainsKey("timestamp") ? Convert.ToSingle(data["timestamp"]) : 0f,
                Type = data.ContainsKey("type") ? (DecisionType)(int)data["type"] : DecisionType.StateTransition,
                StateBefore = data.ContainsKey("stateBefore") ? (PetDecisionSystem.PetAIState)(int)data["stateBefore"] : PetDecisionSystem.PetAIState.Idle,
                StateAfter = data.ContainsKey("stateAfter") ? (PetDecisionSystem.PetAIState)(int)data["stateAfter"] : PetDecisionSystem.PetAIState.Idle,
                TargetName = data.ContainsKey("targetName") ? data["targetName"]?.ToString() : "",
                TargetDistance = data.ContainsKey("targetDistance") ? Convert.ToSingle(data["targetDistance"]) : 0f,
                Reason = data.ContainsKey("reason") ? data["reason"]?.ToString() : "",
                Outcome = data.ContainsKey("outcome") ? (DecisionOutcome)(int)data["outcome"] : DecisionOutcome.Unknown,
                Confidence = data.ContainsKey("confidence") ? Convert.ToSingle(data["confidence"]) : 0f
            };
        }
    }
}
