using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Items
{
    /// <summary>
    /// 药水数据类
    /// </summary>
    public class Potion
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public PotionType Type { get; set; }
        public PotionRarity Rarity { get; set; }
        public int Value { get; set; }
        
        // 效果数值
        public float HealthRestore { get; set; }
        public float ManaRestore { get; set; }
        public float HealthRegen { get; set; }
        public float ManaRegen { get; set; }
        public float DamageBoost { get; set; }
        public float DefenseBoost { get; set; }
        public float SpeedBoost { get; set; }
        public float CriticalBoost { get; set; }
        
        // 持续时间（秒）
        public float Duration { get; set; }
        
        // 冷却时间（秒）
        public float Cooldown { get; set; }
        
        // 堆叠上限
        public int MaxStack { get; set; }
        
        public Color GetRarityColor()
        {
            return Rarity switch
            {
                PotionRarity.Common => new Color(0.7f, 0.7f, 0.7f),
                PotionRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),
                PotionRarity.Rare => new Color(0.2f, 0.5f, 1.0f),
                PotionRarity.Epic => new Color(0.6f, 0.3f, 0.9f),
                PotionRarity.Legendary => new Color(1.0f, 0.6f, 0.0f),
                _ => new Color(1f, 1f, 1f)
            };
        }
    }

    public enum PotionType
    {
        Health,
        Mana,
        Stamina,
        Damage,
        Defense,
        Speed,
        Critical,
        Regeneration,
        Antidote,
        Invisibility
    }

    public enum PotionRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }

    /// <summary>
    /// 玩家拥有的药水实例
    /// </summary>
    public class PotionInstance
    {
        public int PotionId { get; set; }
        public int Quantity { get; set; }
        public bool IsAutoUse { get; set; }
        
        public PotionInstance(int potionId, int quantity = 1)
        {
            PotionId = potionId;
            Quantity = quantity;
            IsAutoUse = false; 
        }
    }
}
