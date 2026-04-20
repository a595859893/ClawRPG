using Godot;
using System;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 互动类型枚举
    /// </summary>
    public enum InteractionType
    {
        Pet,      // 宠物互动
        Mount     // 坐骑互动
    }

    /// <summary>
    /// 互动动作类型
    /// </summary>
    public enum InteractionAction
    {
        Feed,        // 喂食
        Play,        // 玩耍
        Brush,       // 梳理
        Talk,        // 对话
        Pet,         // 抚摸
        Train,       // 训练
        Rest,        // 休息
        Explore,     // 探索
        Groom,       // 美容
        Massage      // 按摩
    }
}
