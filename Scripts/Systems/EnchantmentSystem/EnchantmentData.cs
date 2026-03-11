using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 附魔类型
    /// </summary>
    public enum EnchantmentType {
        Weapon,      // 武器附魔
        Armor,       // 防具附魔
        Accessory,   // 饰品附魔
        Universal    // 通用附魔
    }

    /// <summary>
    /// 附魔属性类型
    /// </summary>
    public enum EnchantmentAttribute {
        Damage,          // 伤害
        AttackSpeed,    // 攻击速度
        CriticalRate,   // 暴击率
        CriticalDamage, // 暴击伤害
        Defense,        // 防御
        Health,         // 生命
        Mana,           // 法力
        HealthRegen,    // 生命恢复
        ManaRegen,      // 法力恢复
        MoveSpeed,      // 移动速度
        FireResistance, // 火焰抗性
        IceResistance,  // 冰霜抗性
        ThunderResistance, // 雷电抗性
        DarkResistance, // 暗影抗性
        LightResistance // 光明抗性
    }

    /// <summary>
    /// 附魔稀有度
    /// </summary>
    public enum EnchantmentRarity {
        Common = 1,     // 普通 (白色)
        Uncommon = 2,  // 优秀 (绿色)
        Rare = 3,      // 稀有 (蓝色)
        Epic = 4,      // 史诗 (紫色)
        Legendary = 5  // 传说 (橙色)
    }

    /// <summary>
    /// 附魔数据
    /// </summary>
    [System.Serializable]
    public class EnchantmentData {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public EnchantmentType Type { get; set; }
        public EnchantmentRarity Rarity { get; set; }
        public Dictionary<EnchantmentAttribute, float> Attributes { get; set; }  // 附魔属性
        public int RequiredLevel { get; set; }  // 需求等级
        public int GoldCost { get; set; }       // 金币成本
        public string IconName { get; set; }
        public List<string> Tags { get; set; }  // 标签

        public EnchantmentData() {
            Attributes = new Dictionary<EnchantmentAttribute, float>();
            Tags = new List<string>();
        }

        /// <summary>
        /// 获取附魔颜色
        /// </summary>
        public Color GetRarityColor() {
            switch (Rarity) {
                case EnchantmentRarity.Common: return new Color(1f, 1f, 1f);
                case EnchantmentRarity.Uncommon: return new Color(0.2f, 1f, 0.2f);
                case EnchantmentRarity.Rare: return new Color(0.3f, 0.5f, 1f);
                case EnchantmentRarity.Epic: return new Color(0.6f, 0.2f, 0.8f);
                case EnchantmentRarity.Legendary: return new Color(1f, 0.6f, 0f);
                default: return new Color(1f, 1f, 1f);
            }
        }

        /// <summary>
        /// 获取稀有度名称
        /// </summary>
        public string GetRarityName() {
            switch (Rarity) {
                case EnchantmentRarity.Common: return "普通";
                case EnchantmentRarity.Uncommon: return "优秀";
                case EnchantmentRarity.Rare: return "稀有";
                case EnchantmentRarity.Epic: return "史诗";
                case EnchantmentRarity.Legendary: return "传说";
                default: return "未知";
            }
        }
    }

    /// <summary>
    /// 装备附魔槽位
    /// </summary>
    [System.Serializable]
    public class EquipmentEnchantmentSlot {
        public int SlotIndex { get; set; }
        public EnchantmentData Enchantment { get; set; }
        public bool IsUnlocked { get; set; }

        public EquipmentEnchantmentSlot() {
            IsUnlocked = false; 
        }
    }

    /// <summary>
    /// 附魔结果
    /// </summary>
    public class EnchantResult {
        public bool Success { get; set; }
        public EnchantmentData Enchantment { get; set; }
        public string Message { get; set; }
        public int NewItemLevel { get; set; }

        public static EnchantResult Fail(string message) {
            return new EnchantResult { Success = false, Message = message };
        }

        public static EnchantResult Success(EnchantmentData enchantment, string message = "") {
            return new EnchantResult { Success = true, Enchantment = enchantment, Message = message };
        }
    }
}
