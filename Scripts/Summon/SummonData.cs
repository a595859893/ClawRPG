using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// 召唤物类型
    /// </summary>
    public enum SummonType
    {
        Elemental,      // 元素生物
        Spirit,         // 灵魂
        Construct,      // 构造体
        Beast,          // 野兽
        Celestial,      // 天界生物
        Demon,          // 恶魔
        Undead,         // 不死族
        Divine          // 神性生物
    }

    /// <summary>
    /// 召唤物稀有度
    /// </summary>
    public enum SummonRarity
    {
        Common,         // 普通
        Uncommon,       // 优秀
        Rare,           // 稀有
        Epic,           // 史诗
        Legendary,      // 传说
        Mythic          // 神化
    }

    /// <summary>
    /// 召唤物状态
    /// </summary>
    public enum SummonState
    {
        Available,      // 可用
        Active,         // 战斗中
        Cooldown,       // 冷却中
        Dismissed       // 已解散
    }

    /// <summary>
    /// 召唤物属性
    /// </summary>
    public class SummonStats
    {
        public int Health { get; set; }
        public int Attack { get; set; }
        public int Defense { get; set; }
        public int Magic { get; set; }
        public int Speed { get; set; }
        public float CriticalRate { get; set; }
        public float CriticalDamage { get; set; }
        public float DodgeRate { get; set; }
        public float BlockRate { get; set; }
        public int LifeSteal { get; set; }
        public int MagicReflect { get; set; }
    }

    /// <summary>
    /// 召唤物技能
    /// </summary>
    public class SummonSkill
    {
        public string SkillId { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int Cooldown { get; set; }
        public int ManaCost { get; set; }
        public float DamageMultiplier { get; set; }
        public string Effect { get; set; }
    }

    /// <summary>
    /// 召唤物配置
    /// </summary>
    public class Summon
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public SummonType Type { get; set; }
        public SummonRarity Rarity { get; set; }
        public SummonStats BaseStats { get; set; }
        public List<SummonSkill> Skills { get; set; }
        public int LevelRequirement { get; set; }
        public int ManaCost { get; set; }
        public int Duration { get; set; }
        public float AttackSpeed { get; set; }
        public string Icon { get; set; }
    }

    /// <summary>
    /// 玩家已解锁的召唤物
    /// </summary>
    public class UnlockedSummon
    {
        public string SummonId { get; set; }
        public DateTime UnlockTime { get; set; }
        public int UseCount { get; set; }
        public int TotalDamage { get; set; }
        public int TotalKills { get; set; }
    }

    /// <summary>
    /// 活跃召唤物实例
    /// </summary>
    public class ActiveSummon
    {
        public string SummonId { get; set; }
        public SummonState State { get; set; }
        public int CurrentHealth { get; set; }
        public int Level { get; set; }
        public int Experience { get; set; }
        public DateTime ActiveTime { get; set; }
        public DateTime CooldownEnd { get; set; }
    }

    /// <summary>
    /// 玩家召唤数据
    /// </summary>
    public class PlayerSummonData
    {
        public List<UnlockedSummon> UnlockedSummons { get; set; }
        public List<ActiveSummon> ActiveSummons { get; set; }
        public int MaxActiveSummons { get; set; }
        public int TotalSummons { get; set; }
        public int TotalDamageDealt { get; set; }
    }

    /// <summary>
    /// 召唤会话记录
    /// </summary>
    public class SummonSession
    {
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<string> SummonIds { get; set; }
        public int DamageDealt { get; set; }
        public int EnemiesKilled { get; set; }
    }

    /// <summary>
    /// 召唤统计数据
    /// </summary>
    public class SummonStatistics
    {
        public int TotalSummons { get; set; }
        public int TotalDamageDealt { get; set; }
        public int TotalKills { get; set; }
        public TimeSpan TotalActiveTime { get; set; }
        public Dictionary<SummonType, int> SummonsByType { get; set; }
        public Dictionary<SummonRarity, int> SummonsByRarity { get; set; }
        public string MostUsedSummonId { get; set; }
        public int HighestDamage { get; set; }
    }
}
