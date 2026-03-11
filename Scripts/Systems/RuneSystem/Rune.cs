using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 符文类型枚举
    /// </summary>
    public enum RuneType {
        Attack,      // 攻击符文
        Defense,     // 防御符文
        Magic,       // 魔法符文
        Utility,     // 辅助符文
        Legendary    // 传奇符文
    }

    /// <summary>
    /// 符文稀有度
    /// </summary>
    public enum RuneRarity {
        Common,      // 普通
        Uncommon,    // 优秀
        Rare,        // 稀有
        Epic,        // 史诗
        Legendary    // 传奇
    }

    /// <summary>
    /// 符文属性类型
    /// </summary>
    public enum RuneAttribute {
        Damage,           // 伤害+
        Defense,          // 防御+
        MaxHealth,        // 最大生命+
        MaxMana,          // 最大法力+
        CritChance,       // 暴击率+
        CritDamage,       // 暴击伤害+
        AttackSpeed,      // 攻击速度+
        MoveSpeed,        // 移动速度+
        HealthRegen,      // 生命恢复+
        ManaRegen,        // 法力恢复+
        FireResistance,   // 火焰抗性+
        IceResistance,    // 冰霜抗性+
        DarkResistance    // 暗影抗性+
    }

    /// <summary>
    /// 符文集合类型
    /// </summary>
    public enum RuneSet {
        None,
        Attack,         // 攻击套装
        Defense,        // 防御套装
        Life,           // 生命套装
        Magic,          // 魔法套装
        Speed,          // 速度套装
        Critical,       // 暴击套装
        Balance,        // 均衡套装
        Dragon,         // 龙之套装
        Phoenix,        // 凤凰套装
        Shadow          // 暗影套装
    }

    /// <summary>
    /// 符文数据类
    /// </summary>
    public class Rune {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public RuneType Type { get; set; }
        public RuneRarity Rarity { get; set; }
        public RuneSet Set { get; set; }  // 符文所属套装
        public Dictionary<RuneAttribute, float> Attributes { get; set; }
        public int LevelRequired { get; set; }
        public int Price { get; set; }
        
        // 唯一被动效果（如果有）
        public string UniquePassive { get; set; }
        
        // 图标路径
        public string IconPath { get; set; }

        public Rune() {
            Attributes = new Dictionary<RuneAttribute, float>();
            Set = RuneSet.None;
        }

        /// <summary>
        /// 获取符文颜色
        /// </summary>
        public Color GetRarityColor() {
            return Rarity switch {
                RuneRarity.Common => new Color(0.7f, 0.7f, 0.7f),      // 灰色
                RuneRarity.Uncommon => new Color(0.2f, 0.8f, 0.2f),    // 绿色
                RuneRarity.Rare => new Color(0.3f, 0.5f, 1.0f),       // 蓝色
                RuneRarity.Epic => new Color(0.6f, 0.3f, 0.9f),        // 紫色
                RuneRarity.Legendary => new Color(1.0f, 0.6f, 0.0f),  // 橙色
                _ => Colors.White
            };
        }

        /// <summary>
        /// 获取类型颜色
        /// </summary>
        public Color GetTypeColor() {
            return Type switch {
                RuneType.Attack => new Color(1.0f, 0.3f, 0.3f),    // 红色
                RuneType.Defense => new Color(0.3f, 0.6f, 1.0f),   // 蓝色
                RuneType.Magic => new Color(0.8f, 0.4f, 1.0f),    // 紫色
                RuneType.Utility => new Color(0.4f, 0.9f, 0.4f),   // 绿色
                RuneType.Legendary => new Color(1.0f, 0.8f, 0.0f),  // 金色
                _ => Colors.White
            };
        }
    }

    /// <summary>
    /// 装备符文槽位
    /// </summary>
    public class EquipmentRuneSlot {
        public int SlotIndex { get; set; }
        public bool IsUnlocked { get; set; }
        public Rune EquippedRune { get; set; }
        public int UnlockCost { get; set; }

        public EquipmentRuneSlot(int index) {
            SlotIndex = index;
            IsUnlocked = index == 0; // 第一个槽位默认解锁
            UnlockCost = index switch {
                0 => 0,
                1 => 500,
                2 => 1500,
                3 => 4000,
                4 => 10000,
                _ => 25000
            };
        }
    }
}
