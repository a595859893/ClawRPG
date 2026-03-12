using Godot;
using System;
using System.Collections.Generic;

public class ElementalReactionData
{
    // 元素类型
    public enum ElementType
    {
        Fire,      // 火
        Ice,       // 冰
        Lightning, // 雷
        Water,     // 水
        Dark,      // 暗
        Holy,      // 光
        Physical,  // 物理
        Nature,    // 自然
        Wind       // 风
    }

    // 反应类型
    public enum ReactionType
    {
        Vaporize,      // 蒸发: 火+水 -> 额外伤害
        Freeze,        // 冻结: 水+冰 -> 冰冻控制
        Conduct,       // 导电: 雷+水 -> 连锁伤害
        Burn,          // 燃烧: 火+风 -> 持续伤害
        Melt,          // 融化: 火+冰 -> 护甲穿透
        Shock,         // 震荡: 雷+冰 -> 眩晕
        Corrode,       // 腐蚀: 暗+自然 -> 防御下降
        Purify,        // 净化: 光+暗 -> 驱散增益
        Overload,      // 过载: 雷+雷 -> 爆炸
        Superconduct,  // 超导: 冰+雷 -> 移动速度下降
        Petrify,       // 石化: 土+暗 -> 石化控制
        Bloom,         // 绽放: 火+土 -> 范围伤害
        Swirl,         // 扩散: 风+任意元素 -> 范围扩散
        Crystallize,   // 结晶: 冰+土 -> 护盾
        Fracture,      // 碎裂: 物理+雷 -> 护甲碎裂
        Infusion       // 灌注: 光+火 -> 神圣伤害
    }

    // 元素状态
    public class ElementStatus
    {
        public ElementType Type { get; set; }
        public float Intensity { get; set; }  // 强度 0-100
        public float Duration { get; set; }   // 持续时间(秒)
        public float AccumulatedDamage { get; set; }  // 累积伤害
        public Node Reference { get; set; }   // 引用节点
    }

    // 反应结果
    public class ReactionResult
    {
        public ReactionType Type { get; set; }
        public ElementType Element1 { get; set; }
        public ElementType Element2 { get; set; }
        public float Damage { get; set; }
        public float ControlDuration { get; set; }  // 控制时长
        public float DotDamage { get; set; }        // 持续伤害
        public float DotDuration { get; set; }     // 持续时长
        public float StatModifier { get; set; }     // 属性修正
        public string StatusEffect { get; set; }    // 状态效果
        public bool IsAOE { get; set; }            // 是否范围伤害
    }

    // 反应配置
    public class ReactionConfig
    {
        public ReactionType Type { get; set; }
        public ElementType Element1 { get; set; }
        public ElementType Element2 { get; set; }
        public float DamageMultiplier { get; set; }  // 伤害倍率
        public float BaseDamage { get; set; }       // 基础伤害
        public float ControlDuration { get; set; }
        public float DotDamage { get; set; }
        public float DotDuration { get; set; }
        public float StatModifier { get; set; }
        public string StatusEffect { get; set; }
        public bool IsAOE { get; set; }
        public float AORadius { get; set; }        // 范围半径
        public float Cooldown { get; set; }        // 反应冷却
    }

    // 玩家元素状态
    public class PlayerElementalState
    {
        public Dictionary<ElementType, float> ElementalGauge { get; set; } = new Dictionary<ElementType, float>();
        public Dictionary<ElementType, float> ElementalAffinity { get; set; } = new Dictionary<ElementType, float>();  // 元素亲和加成
        public List<ElementStatus> ActiveElements { get; set; } = new List<ElementStatus>();
        public Dictionary<ReactionType, int> ReactionsTriggered { get; set; } = new Dictionary<ReactionType, int>();
        public float TotalReactionDamage { get; set; }
    }

    // 敌人元素状态
    public class EnemyElementalState
    {
        public NodeId Node { get; set; }
        public Dictionary<ElementType, float> AppliedElements { get; set; } = new Dictionary<ElementType, float>();
        public Dictionary<ReactionType, int> ReactionsSuffered { get; set; } = new Dictionary<ReactionType, int>();
        public float TotalDamageTaken { get; set; }
    }
}
