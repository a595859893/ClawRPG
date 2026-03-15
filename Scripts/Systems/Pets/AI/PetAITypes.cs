using Godot;
using System;

namespace ClawRPG.Systems.Pets.AI
{
    /// <summary>
    /// 宠物 AI 状态枚举
    /// </summary>
    public enum PetAIState
    {
        Idle,          // 待命
        Following,     // 跟随
        Engaging,      // 接敌
        Attacking,     // 攻击
        Retreating,   // 后撤
        Supporting     // 支援
    }
    
    /// <summary>
    /// 宠物类型枚举
    /// </summary>
    public enum PetType
    {
        Attack,   // 攻击型
        Defense,  // 防御型
        Magic,    // 魔法型
        Support,  // 支援型
        Balanced // 平衡型
    }
}
