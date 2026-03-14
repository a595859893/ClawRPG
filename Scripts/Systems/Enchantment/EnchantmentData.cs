using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Enchantment
{
    /// <summary>
    /// 附魔类型枚举
    /// </summary>
    public enum EnchantmentType
    {
        Weapon,
        Armor,
        Accessory,
        Universal
    }
    
    /// <summary>
    /// 附魔效果类型
    /// </summary>
    public enum EnchantmentEffect
    {
        Damage,
        CriticalRate,
        CriticalDamage,
        AttackSpeed,
        LifeSteal,
        Defense,
        Health,
        Mana,
        ManaRegen,
        Speed,
        Dodge,
        FireResistance,
        IceResistance,
        LightningResistance,
        PoisonResistance,
        AllAttributes,
        Strength,
        Intelligence,
        Dexterity,
        Vitality,
        Luck
    }
    
    /// <summary>
    /// 附魔等级
    /// </summary>
    public enum EnchantmentTier
    {
        Common,      // 普通
        Uncommon,    // 优秀
        Rare,        // 稀有
        Epic,        // 史诗
        Legendary    // 传说
    }
    
    /// <summary>
    /// 附魔记录
    /// </summary>
    public class EnchantmentRecord
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public EnchantmentType Type { get; set; }
        public EnchantmentEffect PrimaryEffect { get; set; }
        public float PrimaryEffectValue { get; set; }
        public EnchantmentEffect? SecondaryEffect { get; set; }
        public float SecondaryEffectValue { get; set; }
        public EnchantmentTier Tier { get; set; }
        public int RequiredLevel { get; set; }
        public int EnchantmentCost { get; set; }
        public float SuccessRate { get; set; }
        public string Description { get; set; }
        public string IconName { get; set; }
        
        public EnchantmentRecord()
        {
            Id = Guid.NewGuid().ToString();
        }
    }
    
    /// <summary>
    /// 玩家已解锁的附魔
    /// </summary>
    public class UnlockedEnchantment
    {
        public string EnchantmentId { get; set; }
        public DateTime UnlockedAt { get; set; }
        public int UsageCount { get; set; }
        public int SuccessCount { get; set; }
        
        public UnlockedEnchantment()
        {
            UnlockedAt = DateTime.Now;
            UsageCount = 0;
            SuccessCount = 0;
        }
        
        public float GetSuccessRate()
        {
            if (UsageCount == 0) return 0f;
            return (float)SuccessCount / UsageCount * 100f;
        }
    }
    
    /// <summary>
    /// 装备附魔记录
    /// </summary>
    public class EquipmentEnchantment
    {
        public string EquipmentId { get; set; }
        public string EnchantmentId { get; set; }
        public int EnchantmentLevel { get; set; }
        public DateTime EnchantedAt { get; set; }
        public bool IsPermanent { get; set; }
        
        public EquipmentEnchantment()
        {
            EnchantedAt = DateTime.Now;
            EnchantmentLevel = 1;
            IsPermanent = true;
        }
    }
    
    /// <summary>
    /// 附魔会话（用于附魔过程）
    /// </summary>
    public class EnchantmentSession
    {
        public string SessionId { get; set; }
        public string PlayerId { get; set; }
        public string EquipmentId { get; set; }
        public string EnchantmentId { get; set; }
        public int AttemptLevel { get; set; }
        public DateTime StartedAt { get; set; }
        public bool IsCompleted { get; set; }
        public bool WasSuccessful { get; set; }
        
        public EnchantmentSession()
        {
            SessionId = Guid.NewGuid().ToString();
            StartedAt = DateTime.Now;
            IsCompleted = false;
            WasSuccessful = false;
            AttemptLevel = 1;
        }
    }
    
    /// <summary>
    /// 附魔统计
    /// </summary>
    public class EnchantmentStatistics
    {
        public int TotalAttempts { get; set; }
        public int TotalSuccesses { get; set; }
        public int TotalFailures { get; set; }
        public int TotalExpenses { get; set; }
        public Dictionary<string, int> EnchantmentUsageCount { get; set; }
        public int HighestTierUnlocked { get; set; }
        
        public EnchantmentStatistics()
        {
            EnchantmentUsageCount = new Dictionary<string, int>();
            HighestTierUnlocked = 0;
            TotalAttempts = 0;
            TotalSuccesses = 0;
            TotalFailures = 0;
            TotalExpenses = 0;
        }
        
        public float GetOverallSuccessRate()
        {
            if (TotalAttempts == 0) return 0f;
            return (float)TotalSuccesses / TotalAttempts * 100f;
        }
    }
    
    /// <summary>
    /// 附魔进度
    /// </summary>
    public class EnchantmentProgress
    {
        public string PlayerId { get; set; }
        public List<UnlockedEnchantment> UnlockedEnchantments { get; set; }
        public List<EquipmentEnchantment> ActiveEnchantments { get; set; }
        public EnchantmentStatistics Statistics { get; set; }
        public int TotalEnchantmentsPerformed { get; set; }
        public int CurrentFocusPoints { get; set; }
        
        public EnchantmentProgress()
        {
            PlayerId = "";
            UnlockedEnchantments = new List<UnlockedEnchantment>();
            ActiveEnchantments = new List<EquipmentEnchantment>();
            Statistics = new EnchantmentStatistics();
            TotalEnchantmentsPerformed = 0;
            CurrentFocusPoints = 0;
        }
        
        public EnchantmentProgress(string playerId)
        {
            PlayerId = playerId;
            UnlockedEnchantments = new List<UnlockedEnchantment>();
            ActiveEnchantments = new List<EquipmentEnchantment>();
            Statistics = new EnchantmentStatistics();
            TotalEnchantmentsPerformed = 0;
            CurrentFocusPoints = 0;
        }
    }
}
