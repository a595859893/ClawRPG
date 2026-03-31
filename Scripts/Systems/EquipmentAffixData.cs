using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Equipment affix types
    /// </summary>
    public enum AffixType
    {
        Prefix,   // 前缀: 攻击/防御/生命/速度
        Suffix    // 后缀: 暴击率/暴击伤害/生命偷取/闪避/韧性
    }

    /// <summary>
    /// Single affix definition
    /// </summary>
    public class EquipmentAffix
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AffixType Type { get; set; }
        public ItemQuality MinQuality { get; set; }
        public float AttributeValue { get; set; }
        public string AttributeName { get; set; }  // attack/defense/health/speed/crit_rate/crit_damage/lifesteal/dodge/resistance
        public float Weight { get; set; } = 1.0f;  // 抽取权重
    }

    /// <summary>
    /// Equipment with affixes
    /// </summary>
    public class EquipmentAffixData
    {
        public int ItemId { get; set; }
        public List<EquipmentAffix> Affixes { get; set; } = new List<EquipmentAffix>();
        public float TotalScore { get; set; }  // 装备评分
    }

    /// <summary>
    /// Player's equipment affix data
    /// </summary>
    public class PlayerAffixData
    {
        public Dictionary<int, EquipmentAffixData> EquipmentAffixes { get; set; } = new Dictionary<int, EquipmentAffixData>();
    }
}
