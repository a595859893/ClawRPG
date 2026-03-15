using Godot;
using System;
using System.Collections.Generic;
using ElementalReactionData;

/// <summary>
/// 元素反应数据库 - 配置元素反应效果
/// </summary>
public class ElementalReactionDatabase
{
    private static ElementalReactionDatabase _instance;
    public static ElementalReactionDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new ElementalReactionDatabase();
            return _instance;
        }
    }

    // 元素颜色
    public Dictionary<ElementType, Color> ElementColors = new Dictionary<ElementType, Color>
    {
        { ElementType.Fire, new Color(1f, 0.3f, 0f) },        // 橙红色
        { ElementType.Ice, new Color(0.5f, 0.8f, 1f) },        // 浅蓝色
        { ElementType.Lightning, new Color(1f, 1f, 0.2f) },    // 黄色
        { ElementType.Water, new Color(0.2f, 0.5f, 1f) },       // 蓝色
        { ElementType.Dark, new Color(0.4f, 0.1f, 0.5f) },      // 深紫色
        { ElementType.Holy, new Color(1f, 0.9f, 0.5f) },        // 金白色
        { ElementType.Physical, new Color(0.6f, 0.5f, 0.4f) }, // 棕色
        { ElementType.Nature, new Color(0.3f, 0.8f, 0.2f) },   // 绿色
        { ElementType.Wind, new Color(0.7f, 0.8f, 0.9f) }       // 浅灰色
    };

    // 元素亲和加成 (百分比)
    public Dictionary<ElementType, float> ElementAffinityBonus = new Dictionary<ElementType, float>
    {
        { ElementType.Fire, 0.15f },       // 火亲和: 15% 火属性伤害加成
        { ElementType.Ice, 0.15f },        // 冰亲和: 15% 冰属性伤害加成
        { ElementType.Lightning, 0.12f },  // 雷亲和: 12% 雷属性伤害加成
        { ElementType.Water, 0.12f },      // 水亲和: 12% 水属性伤害加成
        { ElementType.Dark, 0.15f },       // 暗亲和: 15% 暗属性伤害加成
        { ElementType.Holy, 0.15f },       // 光亲和: 15% 光属性伤害加成
        { ElementType.Physical, 0.10f },   // 物理亲和: 10% 物理伤害加成
        { ElementType.Nature, 0.12f },     // 自然亲和: 12% 自然伤害加成
        { ElementType.Wind, 0.10f }        // 风亲和: 10% 风属性伤害加成
    };

    // 反应配置
    public List<ReactionConfig> Reactions = new List<ReactionConfig>
    {
        // Vaporize 蒸发: 火+水 -> 额外伤害
        new ReactionConfig
        {
            Type = ReactionType.Vaporize,
            Element1 = ElementType.Fire,
            Element2 = ElementType.Water,
            DamageMultiplier = 1.5f,
            BaseDamage = 50f,
            ControlDuration = 0f,
            DotDamage = 5f,
            DotDuration = 3f,
            StatModifier = 0f,
            StatusEffect = "Wet",
            IsAOE = false,
            AORadius = 0f,
            Cooldown = 1f
        },

        // Freeze 冻结: 水+冰 -> 冰冻控制
        new ReactionConfig
        {
            Type = ReactionType.Freeze,
            Element1 = ElementType.Water,
            Element2 = ElementType.Ice,
            DamageMultiplier = 1.2f,
            BaseDamage = 30f,
            ControlDuration = 2.5f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = 0f,
            StatusEffect = "Frozen",
            IsAOE = false,
            AORadius = 0f,
            Cooldown = 2f
        },

        // Conduct 导电: 雷+水 -> 连锁伤害
        new ReactionConfig
        {
            Type = ReactionType.Conduct,
            Element1 = ElementType.Lightning,
            Element2 = ElementType.Water,
            DamageMultiplier = 1.8f,
            BaseDamage = 60f,
            ControlDuration = 0.5f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = 0f,
            StatusEffect = "Shocked",
            IsAOE = true,
            AORadius = 150f,
            Cooldown = 1.5f
        },

        // Burn 燃烧: 火+风 -> 持续伤害
        new ReactionConfig
        {
            Type = ReactionType.Burn,
            Element1 = ElementType.Fire,
            Element2 = ElementType.Wind,
            DamageMultiplier = 1.3f,
            BaseDamage = 20f,
            ControlDuration = 0f,
            DotDamage = 15f,
            DotDuration = 5f,
            StatModifier = 0f,
            StatusEffect = "Burning",
            IsAOE = true,
            AORadius = 100f,
            Cooldown = 2f
        },

        // Melt 融化: 火+冰 -> 护甲穿透
        new ReactionConfig
        {
            Type = ReactionType.Melt,
            Element1 = ElementType.Fire,
            Element2 = ElementType.Ice,
            DamageMultiplier = 1.6f,
            BaseDamage = 45f,
            ControlDuration = 0f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = -0.4f,  // 降低40%防御
            StatusEffect = "ArmorMelted",
            IsAOE = false,
            AORadius = 0f,
            Cooldown = 1.5f
        },

        // Shock 震荡: 雷+冰 -> 眩晕
        new ReactionConfig
        {
            Type = ReactionType.Shock,
            Element1 = ElementType.Lightning,
            Element2 = ElementType.Ice,
            DamageMultiplier = 1.4f,
            BaseDamage = 40f,
            ControlDuration = 1.5f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = 0f,
            StatusEffect = "Stunned",
            IsAOE = false,
            AORadius = 0f,
            Cooldown = 2f
        },

        // Corrode 腐蚀: 暗+自然 -> 防御下降
        new ReactionConfig
        {
            Type = ReactionType.Corrode,
            Element1 = ElementType.Dark,
            Element2 = ElementType.Nature,
            DamageMultiplier = 1.3f,
            BaseDamage = 35f,
            ControlDuration = 0f,
            DotDamage = 8f,
            DotDuration = 4f,
            StatModifier = -0.3f,  // 降低30%防御
            StatusEffect = "Corroded",
            IsAOE = false,
            AORadius = 0f,
            Cooldown = 2f
        },

        // Purify 净化: 光+暗 -> 驱散增益
        new ReactionConfig
        {
            Type = ReactionType.Purify,
            Element1 = ElementType.Holy,
            Element2 = ElementType.Dark,
            DamageMultiplier = 1.0f,
            BaseDamage = 25f,
            ControlDuration = 0f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = 0f,
            StatusEffect = "Purified",
            IsAOE = true,
            AORadius = 200f,
            Cooldown = 3f
        },

        // Overload 过载: 雷+雷 -> 爆炸
        new ReactionConfig
        {
            Type = ReactionType.Overload,
            Element1 = ElementType.Lightning,
            Element2 = ElementType.Lightning,
            DamageMultiplier = 2.5f,
            BaseDamage = 100f,
            ControlDuration = 0.3f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = 0f,
            StatusEffect = "Overloaded",
            IsAOE = true,
            AORadius = 180f,
            Cooldown = 3f
        },

        // Superconduct 超导: 冰+雷 -> 移动速度下降
        new ReactionConfig
        {
            Type = ReactionType.Superconduct,
            Element1 = ElementType.Ice,
            Element2 = ElementType.Lightning,
            DamageMultiplier = 1.2f,
            BaseDamage = 25f,
            ControlDuration = 0f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = -0.5f,  // 降低50%速度
            StatusEffect = "Slowed",
            IsAOE = true,
            AORadius = 120f,
            Cooldown = 2f
        },

        // Petrify 石化: 土+暗 -> 石化控制
        new ReactionConfig
        {
            Type = ReactionType.Petrify,
            Element1 = ElementType.Nature,
            Element2 = ElementType.Dark,
            DamageMultiplier = 1.4f,
            BaseDamage = 35f,
            ControlDuration = 3f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = 0f,
            StatusEffect = "Petrified",
            IsAOE = false,
            AORadius = 0f,
            Cooldown = 4f
        },

        // Bloom 绽放: 火+土 -> 范围伤害
        new ReactionConfig
        {
            Type = ReactionType.Bloom,
            Element1 = ElementType.Fire,
            Element2 = ElementType.Nature,
            DamageMultiplier = 1.6f,
            BaseDamage = 55f,
            ControlDuration = 0f,
            DotDamage = 10f,
            DotDuration = 3f,
            StatModifier = 0f,
            StatusEffect = "Blooming",
            IsAOE = true,
            AORadius = 160f,
            Cooldown = 2f
        },

        // Swirl 扩散: 风+任意元素 -> 范围扩散
        new ReactionConfig
        {
            Type = ReactionType.Swirl,
            Element1 = ElementType.Wind,
            Element2 = ElementType.Fire,  // 任意
            DamageMultiplier = 1.3f,
            BaseDamage = 30f,
            ControlDuration = 0f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = 0f,
            StatusEffect = "Swirled",
            IsAOE = true,
            AORadius = 200f,
            Cooldown = 1f
        },

        // Crystallize 结晶: 冰+土 -> 护盾
        new ReactionConfig
        {
            Type = ReactionType.Crystallize,
            Element1 = ElementType.Ice,
            Element2 = ElementType.Nature,
            DamageMultiplier = 0.8f,
            BaseDamage = 15f,
            ControlDuration = 0f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = 0.3f,  // 获得30%护盾
            StatusEffect = "Shielded",
            IsAOE = false,
            AORadius = 0f,
            Cooldown = 3f
        },

        // Fracture 碎裂: 物理+雷 -> 护甲碎裂
        new ReactionConfig
        {
            Type = ReactionType.Fracture,
            Element1 = ElementType.Physical,
            Element2 = ElementType.Lightning,
            DamageMultiplier = 1.5f,
            BaseDamage = 45f,
            ControlDuration = 0f,
            DotDamage = 0f,
            DotDuration = 0f,
            StatModifier = -0.5f,  // 降低50%护甲
            StatusEffect = "ArmorBroken",
            IsAOE = false,
            AORadius = 0f,
            Cooldown = 2f
        },

        // Infusion 灌注: 光+火 -> 神圣伤害
        new ReactionConfig
        {
            Type = ReactionType.Infusion,
            Element1 = ElementType.Holy,
            Element2 = ElementType.Fire,
            DamageMultiplier = 2.0f,
            BaseDamage = 80f,
            ControlDuration = 0f,
            DotDamage = 12f,
            DotDuration = 4f,
            StatModifier = 0f,
            StatusEffect = "HolyBurn",
            IsAOE = false,
            AORadius = 0f,
            Cooldown = 2.5f
        }
    };

    // 获取反应配置
    public ReactionConfig GetReactionConfig(ReactionType type)
    {
        foreach (var reaction in Reactions)
        {
            if (reaction.Type == type)
                return reaction;
        }
        return null;
    }

    // 获取两个元素之间的反应
    public ReactionConfig GetReaction(ElementType elem1, ElementType elem2)
    {
        foreach (var reaction in Reactions)
        {
            if ((reaction.Element1 == elem1 && reaction.Element2 == elem2) ||
                (reaction.Element1 == elem2 && reaction.Element2 == elem1))
            {
                return reaction;
            }
        }
        return null;
    }

    // 获取元素颜色
    public Color GetElementColor(ElementType type)
    {
        if (ElementColors.ContainsKey(type))
            return ElementColors[type];
        return Colors.White;
    }

    // 获取元素亲和加成
    public float GetAffinityBonus(ElementType type)
    {
        if (ElementAffinityBonus.ContainsKey(type))
            return ElementAffinityBonus[type];
        return 0f;
    }
}
