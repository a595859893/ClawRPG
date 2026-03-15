// ============================================
// Enchantment System - 附魔系统
// ============================================

using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.EnchantmentSystem
{
    // ============================================
    // Data Structures - 数据结构
    // ============================================
    
    public enum EnchantmentType
    {
        Weapon,      // 武器附魔
        Armor,      // 护甲附魔
        Accessory,  // 饰品附魔
        Universal   // 通用附魔
    }
    
    public enum EnchantmentRarity
    {
        Common = 1,      // 普通
        Uncommon = 2,   // 优秀
        Rare = 3,       // 稀有
        Epic = 4,       // 史诗
        Legendary = 5   // 传说
    }
    
    public enum EnchantmentEffect
    {
        // 攻击类
        Damage,           // 攻击力+
        CriticalChance,    // 暴击率+
        CriticalDamage,    // 暴击伤害+
        AttackSpeed,      // 攻击速度+
        
        // 防御类
        Defense,           // 防御力+
        MaxHealth,         // 生命值+
        HealthRegen,       // 生命恢复+
        FireResistance,   // 火焰抗性
        IceResistance,     // 冰霜抗性
        LightningResistance, // 雷电抗性
        PoisonResistance,  // 毒抗性
        
        // 资源类
        MaxMana,           // 法力值+
        ManaRegen,         // 法力恢复+
        
        // 速度类
        MovementSpeed,     // 移动速度+
        CooldownReduction, // 冷却缩减
        
        // 特殊类
        LifeSteal,         // 生命偷取
        Dodge,             // 闪避率+
        GoldBonus,         // 金币加成
        ExperienceBonus,   // 经验加成
        ItemDropBonus,     // 物品掉落加成
    }
    
    public class EnchantmentConfig
    {
        public string Id;
        public string Name;
        public string Description;
        public EnchantmentType Type;
        public EnchantmentRarity Rarity;
        public EnchantmentEffect Effect;
        public float EffectValue;
        public int LevelRequirement;
        public int GoldCost;
        public float SuccessRate;
    }
    
    public class PlayerEnchantmentData
    {
        public Dictionary<string, UnlockedEnchantment> UnlockedEnchantments = new Dictionary<string, UnlockedEnchantment>();
        public int TotalEnchantmentsPerformed;
        public int SuccessfulEnchantments;
        public int FailedEnchantments;
    }
    
    public class UnlockedEnchantment
    {
        public string EnchantmentId;
        public DateTime UnlockedAt;
    }
    
    public class EquipmentEnchantment
    {
        public string EquipmentId;
        public string EnchantmentId;
        public DateTime AppliedAt;
    }
    
    // ============================================
    // Configuration Database - 配置数据库
    // ============================================
    
    public static class EnchantmentDatabase
    {
        private static List<EnchantmentConfig> _enchantments;
        
        public static List<EnchantmentConfig> GetAllEnchantments()
        {
            if (_enchantments == null)
            {
                InitializeEnchantments();
            }
            return _enchantments;
        }
        
        private static void InitializeEnchantments()
        {
            _enchantments = new List<EnchantmentConfig>
            {
                // ============================================
                // Weapon Enchantments - 武器附魔
                // ============================================
                
                // Common - 普通
                new EnchantmentConfig { Id = "weapon_damage_1", Name = "锋利", Description = "攻击力+5%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.Damage, EffectValue = 5, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                new EnchantmentConfig { Id = "weapon_crit_1", Name = "锐眼", Description = "暴击率+2%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.CriticalChance, EffectValue = 2, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                new EnchantmentConfig { Id = "weapon_attack_speed_1", Name = "迅捷", Description = "攻击速度+3%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.AttackSpeed, EffectValue = 3, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                
                // Uncommon - 优秀
                new EnchantmentConfig { Id = "weapon_damage_2", Name = "锋利 II", Description = "攻击力+10%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.Damage, EffectValue = 10, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                new EnchantmentConfig { Id = "weapon_crit_2", Name = "锐眼 II", Description = "暴击率+4%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.CriticalChance, EffectValue = 4, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                new EnchantmentConfig { Id = "weapon_crit_damage_1", Name = "重击", Description = "暴击伤害+10%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.CriticalDamage, EffectValue = 10, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                new EnchantmentConfig { Id = "weapon_lifesteal_1", Name = "吸血", Description = "生命偷取+3%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.LifeSteal, EffectValue = 3, LevelRequirement = 15, GoldCost = 300, SuccessRate = 0.75f },
                
                // Rare - 稀有
                new EnchantmentConfig { Id = "weapon_damage_3", Name = "锋利 III", Description = "攻击力+15%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.Damage, EffectValue = 15, LevelRequirement = 20, GoldCost = 500, SuccessRate = 0.7f },
                new EnchantmentConfig { Id = "weapon_crit_3", Name = "锐眼 III", Description = "暴击率+6%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.CriticalChance, EffectValue = 6, LevelRequirement = 20, GoldCost = 500, SuccessRate = 0.7f },
                new EnchantmentConfig { Id = "weapon_crit_damage_2", Name = "重击 II", Description = "暴击伤害+20%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.CriticalDamage, EffectValue = 20, LevelRequirement = 20, GoldCost = 500, SuccessRate = 0.7f },
                new EnchantmentConfig { Id = "weapon_lifesteal_2", Name = "吸血 II", Description = "生命偷取+5%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.LifeSteal, EffectValue = 5, LevelRequirement = 25, GoldCost = 600, SuccessRate = 0.65f },
                new EnchantmentConfig { Id = "weapon_cooldown_1", Name = "急速", Description = "冷却缩减+5%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.CooldownReduction, EffectValue = 5, LevelRequirement = 25, GoldCost = 600, SuccessRate = 0.65f },
                
                // Epic - 史诗
                new EnchantmentConfig { Id = "weapon_damage_4", Name = "锋利 IV", Description = "攻击力+25%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Epic, EnchantmentEffect = EnchantmentEffect.Damage, EffectValue = 25, LevelRequirement = 35, GoldCost = 1000, SuccessRate = 0.55f },
                new EnchantmentConfig { Id = "weapon_crit_4", Name = "锐眼 IV", Description = "暴击率+10%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Epic, EnchantmentEffect = EnchantmentEffect.CriticalChance, EffectValue = 10, LevelRequirement = 35, GoldCost = 1000, SuccessRate = 0.55f },
                new EnchantmentConfig { Id = "weapon_crit_damage_3", Name = "重击 III", Description = "暴击伤害+35%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Epic, EnchantmentEffect = EnchantmentEffect.CriticalDamage, EffectValue = 35, LevelRequirement = 35, GoldCost = 1000, SuccessRate = 0.55f },
                new EnchantmentConfig { Id = "weapon_lifesteal_3", Name = "吸血 III", Description = "生命偷取+8%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Epic, EnchantmentEffect = EnchantmentEffect.LifeSteal, EffectValue = 8, LevelRequirement = 40, GoldCost = 1200, SuccessRate = 0.5f },
                
                // Legendary - 传说
                new EnchantmentConfig { Id = "weapon_damage_5", Name = "锋利 V", Description = "攻击力+40%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Legendary, EnchantmentEffect = EnchantmentEffect.Damage, EffectValue = 40, LevelRequirement = 50, GoldCost = 2500, SuccessRate = 0.4f },
                new EnchantmentConfig { Id = "weapon_crit_5", Name = "锐眼 V", Description = "暴击率+15%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Legendary, EnchantmentEffect = EnchantmentEffect.CriticalChance, EffectValue = 15, LevelRequirement = 50, GoldCost = 2500, SuccessRate = 0.4f },
                new EnchantmentConfig { Id = "weapon_crit_damage_4", Name = "重击 IV", Description = "暴击伤害+50%", EnchantmentType = EnchantmentType.Weapon, EnchantmentRarity = EnchantmentRarity.Legendary, EnchantmentEffect = EnchantmentEffect.CriticalDamage, EffectValue = 50, LevelRequirement = 50, GoldCost = 2500, SuccessRate = 0.4f },
                
                // ============================================
                // Armor Enchantments - 护甲附魔
                // ============================================
                
                // Common - 普通
                new EnchantmentConfig { Id = "armor_defense_1", Name = "坚固", Description = "防御力+5%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.Defense, EffectValue = 5, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                new EnchantmentConfig { Id = "armor_health_1", Name = "生命", Description = "生命值+5%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.MaxHealth, EffectValue = 5, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                new EnchantmentConfig { Id = "armor_dodge_1", Name = "闪避", Description = "闪避率+2%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.Dodge, EffectValue = 2, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                
                // Uncommon - 优秀
                new EnchantmentConfig { Id = "armor_defense_2", Name = "坚固 II", Description = "防御力+10%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.Defense, EffectValue = 10, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                new EnchantmentConfig { Id = "armor_health_2", Name = "生命 II", Description = "生命值+10%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.MaxHealth, EffectValue = 10, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                new EnchantmentConfig { Id = "armor_regen_1", Name = "回复", Description = "生命恢复+3%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.HealthRegen, EffectValue = 3, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                new EnchantmentConfig { Id = "armor_fire_res_1", Name = "抗火", Description = "火焰抗性+5%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.FireResistance, EffectValue = 5, LevelRequirement = 15, GoldCost = 300, SuccessRate = 0.75f },
                
                // Rare - 稀有
                new EnchantmentConfig { Id = "armor_defense_3", Name = "坚固 III", Description = "防御力+15%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.Defense, EffectValue = 15, LevelRequirement = 20, GoldCost = 500, SuccessRate = 0.7f },
                new EnchantmentConfig { Id = "armor_health_3", Name = "生命 III", Description = "生命值+15%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.MaxHealth, EffectValue = 15, LevelRequirement = 20, GoldCost = 500, SuccessRate = 0.7f },
                new EnchantmentConfig { Id = "armor_regen_2", Name = "回复 II", Description = "生命恢复+5%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.HealthRegen, EffectValue = 5, LevelRequirement = 20, GoldCost = 500, SuccessRate = 0.7f },
                new EnchantmentConfig { Id = "armor_dodge_2", Name = "闪避 II", Description = "闪避率+4%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.Dodge, EffectValue = 4, LevelRequirement = 25, GoldCost = 600, SuccessRate = 0.65f },
                new EnchantmentConfig { Id = "armor_ice_res_1", Name = "抗冰", Description = "冰霜抗性+5%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.IceResistance, EffectValue = 5, LevelRequirement = 25, GoldCost = 600, SuccessRate = 0.65f },
                
                // Epic - 史诗
                new EnchantmentConfig { Id = "armor_defense_4", Name = "坚固 IV", Description = "防御力+25%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Epic, EnchantmentEffect = EnchantmentEffect.Defense, EffectValue = 25, LevelRequirement = 35, GoldCost = 1000, SuccessRate = 0.55f },
                new EnchantmentConfig { Id = "armor_health_4", Name = "生命 IV", Description = "生命值+25%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Epic, EnchantmentEffect = EnchantmentEffect.MaxHealth, EffectValue = 25, LevelRequirement = 35, GoldCost = 1000, SuccessRate = 0.55f },
                
                // Legendary - 传说
                new EnchantmentConfig { Id = "armor_defense_5", Name = "坚固 V", Description = "防御力+40%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Legendary, EnchantmentEffect = EnchantmentEffect.Defense, EffectValue = 40, LevelRequirement = 50, GoldCost = 2500, SuccessRate = 0.4f },
                new EnchantmentConfig { Id = "armor_health_5", Name = "生命 V", Description = "生命值+40%", EnchantmentType = EnchantmentType.Armor, EnchantmentRarity = EnchantmentRarity.Legendary, EnchantmentEffect = EnchantmentEffect.MaxHealth, EffectValue = 40, LevelRequirement = 50, GoldCost = 2500, SuccessRate = 0.4f },
                
                // ============================================
                // Accessory Enchantments - 饰品附魔
                // ============================================
                
                // Common - 普通
                new EnchantmentConfig { Id = "acc_mana_1", Name = "法力", Description = "法力值+5%", EnchantmentType = EnchantmentType.Accessory, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.MaxMana, EffectValue = 5, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                new EnchantmentConfig { Id = "acc_mana_regen_1", Name = "回蓝", Description = "法力恢复+3%", EnchantmentType = EnchantmentType.Accessory, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.ManaRegen, EffectValue = 3, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                new EnchantmentConfig { Id = "acc_speed_1", Name = "加速", Description = "移动速度+3%", EnchantmentType = EnchantmentType.Accessory, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.MovementSpeed, EffectValue = 3, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                
                // Uncommon - 优秀
                new EnchantmentConfig { Id = "acc_mana_2", Name = "法力 II", Description = "法力值+10%", EnchantmentType = EnchantmentType.Accessory, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.MaxMana, EffectValue = 10, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                new EnchantmentConfig { Id = "acc_mana_regen_2", Name = "回蓝 II", Description = "法力恢复+5%", EnchantmentType = EnchantmentType.Accessory, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.ManaRegen, EffectValue = 5, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                
                // Rare - 稀有
                new EnchantmentConfig { Id = "acc_mana_3", Name = "法力 III", Description = "法力值+15%", EnchantmentType = EnchantmentType.Accessory, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.MaxMana, EffectValue = 15, LevelRequirement = 20, GoldCost = 500, SuccessRate = 0.7f },
                new EnchantmentConfig { Id = "acc_cooldown_1", Name = "减cd", Description = "冷却缩减+5%", EnchantmentType = EnchantmentType.Accessory, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.CooldownReduction, EffectValue = 5, LevelRequirement = 20, GoldCost = 500, SuccessRate = 0.7f },
                
                // Epic - 史诗
                new EnchantmentConfig { Id = "acc_mana_4", Name = "法力 IV", Description = "法力值+25%", EnchantmentType = EnchantmentType.Accessory, EnchantmentRarity = EnchantmentRarity.Epic, EnchantmentEffect = EnchantmentEffect.MaxMana, EffectValue = 25, LevelRequirement = 35, GoldCost = 1000, SuccessRate = 0.55f },
                
                // Legendary - 传说
                new EnchantmentConfig { Id = "acc_mana_5", Name = "法力 V", Description = "法力值+40%", EnchantmentType = EnchantmentType.Accessory, EnchantmentRarity = EnchantmentRarity.Legendary, EnchantmentEffect = EnchantmentEffect.MaxMana, EffectValue = 40, LevelRequirement = 50, GoldCost = 2500, SuccessRate = 0.4f },
                
                // ============================================
                // Universal Enchantments - 通用附魔
                // ============================================
                
                // Common - 普通
                new EnchantmentConfig { Id = "uni_gold_1", Name = "招财", Description = "金币加成+5%", EnchantmentType = EnchantmentType.Universal, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.GoldBonus, EffectValue = 5, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                new EnchantmentConfig { Id = "uni_exp_1", Name = "经验", Description = "经验加成+5%", EnchantmentType = EnchantmentType.Universal, EnchantmentRarity = EnchantmentRarity.Common, EnchantmentEffect = EnchantmentEffect.ExperienceBonus, EffectValue = 5, LevelRequirement = 1, GoldCost = 100, SuccessRate = 0.9f },
                
                // Uncommon - 优秀
                new EnchantmentConfig { Id = "uni_gold_2", Name = "招财 II", Description = "金币加成+10%", EnchantmentType = EnchantmentType.Universal, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.GoldBonus, EffectValue = 10, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                new EnchantmentConfig { Id = "uni_exp_2", Name = "经验 II", Description = "经验加成+10%", EnchantmentType = EnchantmentType.Universal, EnchantmentRarity = EnchantmentRarity.Uncommon, EnchantmentEffect = EnchantmentEffect.ExperienceBonus, EffectValue = 10, LevelRequirement = 10, GoldCost = 250, SuccessRate = 0.8f },
                
                // Rare - 稀有
                new EnchantmentConfig { Id = "uni_drop_1", Name = "掉落", Description = "物品掉落+10%", EnchantmentType = EnchantmentType.Universal, EnchantmentRarity = EnchantmentRarity.Rare, EnchantmentEffect = EnchantmentEffect.ItemDropBonus, EffectValue = 10, LevelRequirement = 20, GoldCost = 500, SuccessRate = 0.7f },
                
                // Epic - 史诗
                new EnchantmentConfig { Id = "uni_gold_3", Name = "招财 III", Description = "金币加成+20%", EnchantmentType = EnchantmentType.Universal, EnchantmentRarity = EnchantmentRarity.Epic, EnchantmentEffect = EnchantmentEffect.GoldBonus, EffectValue = 20, LevelRequirement = 35, GoldCost = 1000, SuccessRate = 0.55f },
                new EnchantmentConfig { Id = "uni_exp_3", Name = "经验 III", Description = "经验加成+20%", EnchantmentType = EnchantmentType.Universal, EnchantmentRarity = EnchantmentRarity.Epic, EnchantmentEffect = EnchantmentEffect.ExperienceBonus, EffectValue = 20, LevelRequirement = 35, GoldCost = 1000, SuccessRate = 0.55f },
                
                // Legendary - 传说
                new EnchantmentConfig { Id = "uni_all_1", Name = "全能", Description = "全属性+5%", EnchantmentType = EnchantmentType.Universal, EnchantmentRarity = EnchantmentRarity.Legendary, EnchantmentEffect = EnchantmentEffect.Damage, EffectValue = 5, LevelRequirement = 50, GoldCost = 5000, SuccessRate = 0.25f },
            };
        }
        
        public static List<EnchantmentConfig> GetEnchantmentsByType(EnchantmentType type)
        {
            return GetAllEnchantments().Where(e => e.Type == type).ToList();
        }
        
        public static List<EnchantmentConfig> GetEnchantmentsByRarity(EnchantmentRarity rarity)
        {
            return GetAllEnchantments().Where(e => e.Rarity == rarity).ToList();
        }
        
        public static EnchantmentConfig GetEnchantmentById(string id)
        {
            return GetAllEnchantments().FirstOrDefault(e => e.Id == id);
        }
        
        public static Dictionary<EnchantmentRarity, string> RarityNames = new Dictionary<EnchantmentRarity, string>
        {
            { EnchantmentRarity.Common, "普通" },
            { EnchantmentRarity.Uncommon, "优秀" },
            { EnchantmentRarity.Rare, "稀有" },
            { EnchantmentRarity.Epic, "史诗" },
            { EnchantmentRarity.Legendary, "传说" }
        };
        
        public static Dictionary<EnchantmentType, string> TypeNames = new Dictionary<EnchantmentType, string>
        {
            { EnchantmentType.Weapon, "武器" },
            { EnchantmentType.Armor, "护甲" },
            { EnchantmentType.Accessory, "饰品" },
            { EnchantmentType.Universal, "通用" }
        };
    }
    
    // ============================================
    // Core System - 核心系统
    // ============================================
    
    public class EnchantmentSystem
    {
        private static EnchantmentSystem _instance;
        public static EnchantmentSystem Instance => _instance ?? (_instance = new EnchantmentSystem());
        
        private PlayerEnchantmentData _playerData;
        
        public void Initialize()
        {
            _playerData = new PlayerEnchantmentData();
            
            // 解锁所有基础附魔
            var basicEnchantments = new[]
            {
                "weapon_damage_1", "weapon_crit_1", "weapon_attack_speed_1",
                "armor_defense_1", "armor_health_1", "armor_dodge_1",
                "acc_mana_1", "acc_mana_regen_1", "acc_speed_1",
                "uni_gold_1", "uni_exp_1"
            };
            
            foreach (var id in basicEnchantments)
            {
                if (!_playerData.UnlockedEnchantments.ContainsKey(id))
                {
                    _playerData.UnlockedEnchantments[id] = new UnlockedEnchantment
                    {
                        EnchantmentId = id,
                        UnlockedAt = DateTime.Now
                    };
                }
            }
        }
        
        public bool UnlockEnchantment(string enchantmentId)
        {
            if (_playerData.UnlockedEnchantments.ContainsKey(enchantmentId))
                return false;
                
            var config = EnchantmentDatabase.GetEnchantmentById(enchantmentId);
            if (config == null)
                return false;
                
            _playerData.UnlockedEnchantments[enchantmentId] = new UnlockedEnchantment
            {
                EnchantmentId = enchantmentId,
                UnlockedAt = DateTime.Now
            };
            
            return true;
        }
        
        public bool IsEnchantmentUnlocked(string enchantmentId)
        {
            return _playerData.UnlockedEnchantments.ContainsKey(enchantmentId);
        }
        
        public bool PerformEnchantment(string equipmentId, string enchantmentId)
        {
            var config = EnchantmentDatabase.GetEnchantmentById(enchantmentId);
            if (config == null)
                return false;
                
            // 检查是否已解锁
            if (!IsEnchantmentUnlocked(enchantmentId))
                return false;
                
            // 检查金币
            // if (player.Gold < config.GoldCost) return false;
            
            // 随机成功判定
            var random = new Random();
            var roll = random.NextDouble();
            var success = roll < config.SuccessRate;
            
            _playerData.TotalEnchantmentsPerformed++;
            
            if (success)
            {
                _playerData.SuccessfulEnchantments++;
                // 应用附魔到装备
                // equipment.ApplyEnchantment(enchantmentId);
                return true;
            }
            else
            {
                _playerData.FailedEnchantments++;
                return false;
            }
        }
        
        public PlayerEnchantmentData GetPlayerData()
        {
            return _playerData;
        }
        
        public List<EnchantmentConfig> GetUnlockedEnchantments()
        {
            var result = new List<EnchantmentConfig>();
            foreach (var unlocked in _playerData.UnlockedEnchantments.Values)
            {
                var config = EnchantmentDatabase.GetEnchantmentById(unlocked.EnchantmentId);
                if (config != null)
                    result.Add(config);
            }
            return result;
        }
        
        public Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            data["unlockedEnchantments"] = _playerData.UnlockedEnchantments;
            data["totalEnchantments"] = _playerData.TotalEnchantmentsPerformed;
            data["successfulEnchantments"] = _playerData.SuccessfulEnchantments;
            data["failedEnchantments"] = _playerData.FailedEnchantments;
            return data;
        }
        
        public void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("unlockedEnchantments"))
            {
                _playerData.UnlockedEnchantments = (Dictionary<string, UnlockedEnchantment>)data["unlockedEnchantments"];
            }
            if (data.ContainsKey("totalEnchantments"))
            {
                _playerData.TotalEnchantmentsPerformed = Convert.ToInt32(data["totalEnchantments"]);
            }
            if (data.ContainsKey("successfulEnchantments"))
            {
                _playerData.SuccessfulEnchantments = Convert.ToInt32(data["successfulEnchantments"]);
            }
            if (data.ContainsKey("failedEnchantments"))
            {
                _playerData.FailedEnchantments = Convert.ToInt32(data["failedEnchantments"]);
            }
        }
    }
}
