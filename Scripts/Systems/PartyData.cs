using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 队伍数据模块 - 包含所有数据结构定义
/// </summary>
public class PartyData
{
    // 队伍角色
    public enum PartyRole
    {
        Leader,      // 队长
        Tank,        // 坦克
        Healer,      // 治疗
        DamageDealer, // 输出
        Support,     // 辅助
        Scout        // 侦察
    }

    // 队伍Buff类型
    public enum PartyBuffType
    {
        ExperienceBoost,   // 经验加成
        GoldBoost,         // 金币加成
        DamageBoost,       // 伤害加成
        DefenseBoost,      // 防御加成
        HealthRegen,       // 生命恢复
        ManaRegen,         // 法力恢复
        LuckBoost,         // 幸运加成
        DropRateBoost      // 掉落率加成
    }

    // 队伍成员
    public class PartyMember
    {
        public int PlayerId;
        public string PlayerName;
        public Vector2 Position;
        public int Level;
        public int Health;
        public int MaxHealth;
        public PartyRole Role;
        public bool IsOnline;
        public float LastUpdate;
    }

    // 队伍Buff
    public class PartyBuff
    {
        public PartyBuffType Type;
        public float Value;
        public float Duration;
        public float RemainingTime;
        public int ProviderId;
    }

    // 经验分配模式
    public enum ExpDistributionMode
    {
        Equal,          // 平均分配
        BasedOnLevel,   // 按等级分配
        BasedOnDamage,  // 按伤害分配
        BasedOnHealing  // 按治疗分配
    }

    // 队伍配置
    public class PartyConfig
    {
        public bool ShareExp = true;
        public bool ShareLoot = false;
        public bool AutoAccept = false;
        public ExpDistributionMode ExpMode = ExpDistributionMode.Equal;
    }

    // Buff默认配置
    public static Dictionary<PartyBuffType, float> BuffDefaults = new Dictionary<PartyBuffType, float>
    {
        { PartyBuffType.ExperienceBoost, 0.10f },   // 10%
        { PartyBuffType.GoldBoost, 0.10f },         // 10%
        { PartyBuffType.DamageBoost, 0.05f },      // 5%
        { PartyBuffType.DefenseBoost, 0.05f },      // 5%
        { PartyBuffType.HealthRegen, 1.0f },        // 1hp/s
        { PartyBuffType.ManaRegen, 1.0f },          // 1mp/s
        { PartyBuffType.LuckBoost, 0.05f },         // 5%
        { PartyBuffType.DropRateBoost, 0.05f }      // 5%
    };
}
