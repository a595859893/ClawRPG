using System;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 合作冒险会话状态
    /// </summary>
    public enum CoopSessionState
    {
        None,
        Forming,      // 组队中
        Starting,     // 即将开始
        InProgress,   // 进行中
        Paused,       // 暂停
        Completed,    // 完成
        Failed,       // 失败
        Cancelled     // 取消
    }

    /// <summary>
    /// 合作冒险类型
    /// </summary>
    public enum CoopAdventureType
    {
        Standard,     // 标准冒险
        Rush,         // 速通模式
        Challenge,    // 挑战模式
        Event         // 活动模式
    }

    /// <summary>
    /// 玩家在合作会话中的状态
    /// </summary>
    public enum CoopPlayerState
    {
        Waiting,      // 等待中
        Ready,        // 已准备
        InDungeon,    // 在地下城中
        Dead,         // 已死亡
        Disconnected  // 断开连接
    }
}
