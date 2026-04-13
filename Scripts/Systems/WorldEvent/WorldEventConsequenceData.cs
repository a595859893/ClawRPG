// WorldEventConsequenceData.cs
// REQ-197: WorldEvent因果事件链 — 数据结构
// 读取 WorldEventSystem.EventHistory，驱动后续事件因果

using System;
using System.Collections.Generic;

namespace ClawRPG.Core.Systems
{
    /// <summary>
    /// 事件因果类型
    /// </summary>
    public enum ConsequenceType
    {
        Grudge,     // 失败 → 怨念（难度叠加）
        Mark,       // 成功 → 印记（SafeHouse视觉）
        Debt        // 跳过 → 债务（低等级追债）
    }

    /// <summary>
    /// 事件结果（玩家如何处理该事件）
    /// </summary>
    public enum EventOutcome
    {
        None,       // 未处理（事件过期）
        Success,    // 成功完成
        Failed,     // 失败
        Skipped     // 跳过/忽略
    }

    /// <summary>
    /// 单个事件的因果状态
    /// </summary>
    [Serializable]
    public class EventConsequenceState
    {
        /// <summary>事件类型</summary>
        public WorldEventType EventType;

        /// <summary>怨念等级（失败次数）</summary>
        public int GrudgeLevel;

        /// <summary>印记计数（成功次数）</summary>
        public int MarkCount;

        /// <summary>债务计数（跳过次数）</summary>
        public int DebtCount;

        /// <summary>最近一次结果</summary>
        public EventOutcome LastOutcome;

        /// <summary>最近一次处理时间戳</summary>
        public long LastOutcomeTimestamp;

        /// <summary>怨念叙事是否已升级（5+次失败触发强化叙事）</summary>
        public bool IsGrudgeEscalated;

        /// <summary>债务是否已触发过</summary>
        public bool DebtTriggered;
    }

    /// <summary>
    /// 债务记录（用于低等级追债检测）
    /// </summary>
    [Serializable]
    public class DebtRecord
    {
        /// <summary>事件类型</summary>
        public WorldEventType EventType;

        /// <summary>欠债时玩家等级</summary>
        public int PlayerLevelAtDebt;

        /// <summary>债务类型</summary>
        public string DebtCategory;

        /// <summary>债务产生时间戳</summary>
        public long DebtTimestamp;

        /// <summary>债务是否已结算</summary>
        public bool IsResolved;
    }

    /// <summary>
    /// 印记记录（用于 SafeHouse 视觉层）
    /// </summary>
    [Serializable]
    public class MarkRecord
    {
        /// <summary>事件类型</summary>
        public WorldEventType EventType;

        /// <summary>印记强度（可叠加）</summary>
        public int Intensity;

        /// <summary>首次获得时间戳</summary>
        public long FirstEarnedTimestamp;

        /// <summary>最近获得时间戳</summary>
        public long LastEarnedTimestamp;
    }

    /// <summary>
    /// 完整因果状态快照（用于 ExportSaveData）
    /// </summary>
    [Serializable]
    public class WorldEventConsequenceSaveData
    {
        /// <summary>每个事件类型的因果状态</summary>
        public Dictionary<string, EventConsequenceState> ConsequenceStates;

        /// <summary>活跃债务记录</summary>
        public List<DebtRecord> ActiveDebts;

        /// <summary>活跃印记记录</summary>
        public List<MarkRecord> ActiveMarks;
    }
}
