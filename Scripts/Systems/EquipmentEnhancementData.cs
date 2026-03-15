using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 装备强化数据 - 包含强化相关的枚举和数据结构
/// </summary>
public class EquipmentEnhancementData
{
    /// <summary>
    /// 强化类型枚举
    /// </summary>
    public enum EnhancementType
    {
        Attack,       // 攻击强化
        Defense,      // 防御强化
        Health,       // 生命强化
        Magic,        // 魔法强化
        Speed,        // 速度强化
        CriticalRate, // 暴击率强化
        CriticalDamage,// 暴击伤害强化
        LifeSteal,    // 生命偷取强化
        Dodge,        // 闪避强化
        Resilience    // 韧性强化
    }

    /// <summary>
    /// 强化稀有度
    /// </summary>
    public enum EnhancementRarity
    {
        Normal = 0,    // 普通
        Enhanced = 1,  // 强化
        Superior = 2,  // 优秀
        Epic = 3,      // 史诗
        Legendary = 4  // 传说
    }

    /// <summary>
    /// 强化结果枚举
    /// </summary>
    public enum EnhancementResult
    {
        Success,          // 成功
        Failure,          // 失败
        CriticalSuccess,  // 暴击成功
        CriticalFailure   // 暴击失败
    }

    [System.Serializable]
    /// <summary>
    /// 强化配方数据
    /// </summary>
    public class EnhancementRecipe
    {
        public EnhancementType Type;          // 强化类型
        public int Level;                     // 强化等级
        public int SuccessRate;               // 成功率
        public int CriticalRate;              // 暴击率
        public int GoldCost;                  // 金币花费
        public List<int> MaterialIds = new List<int>();      // 材料ID列表
        public List<int> MaterialCounts = new List<int>();   // 材料数量列表
    }

    [System.Serializable]
    /// <summary>
    /// 玩家强化数据
    /// </summary>
    public class PlayerEnhancementData
    {
        public int TotalEnhancements = 0;       // 总强化次数
        public int SuccessfulEnhancements = 0; // 成功次数
        public int FailedEnhancements = 0;     // 失败次数
        public int CriticalSuccesses = 0;     // 暴击成功次数
        public int CriticalFailures = 0;      // 暴击失败次数
        public Dictionary<int, int> EquipmentEnhancementLevels = new Dictionary<int, int>(); // 装备强化等级
    }
}
