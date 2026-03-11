using Godot;
using System;
using System.Collections.Generic;

namespace Game.Scripts.Systems.EquipmentReforging
{
    /// <summary>
    /// 装备洗练数据类型定义
    /// </summary>
    public enum ReforgeType
    {
        Basic,      // 基础属性重置
        Advanced,   // 高级洗练(改变稀有度)
        Legendary   // 传奇洗练(完全重铸)
    }

    public enum ReforgeRarity
    {
        Common,     // 普通
        Uncommon,  // 优秀
        Rare,      // 稀有
        Epic,      // 史诗
        Legendary  // 传说
    }

    /// <summary>
    /// 洗练属性定义
    /// </summary>
    public class ReforgeAttribute
    {
        public string Name { get; set; }
        public float MinValue { get; set; }
        public float MaxValue { get; set; }
        public float Weight { get; set; }  // 权重
    }

    /// <summary>
    /// 洗练配方
    /// </summary>
    public class ReforgeRecipe
    {
        public ReforgeType Type { get; set; }
        public ReforgeRarity Rarity { get; set; }
        public int GoldCost { get; set; }
        public float SuccessRate { get; set; }
        public List<string> RequiredMaterials { get; set; }
        public Dictionary<string, int> MaterialCosts { get; set; }
        public List<ReforgeAttribute> AvailableAttributes { get; set; }
    }

    /// <summary>
    /// 玩家装备洗练数据
    /// </summary>
    public class PlayerReforgeData
    {
        public int TotalReforges { get; set; }
        public int SuccessfulReforges { get; set; }
        public int FailedReforges { get; set; }
        public Dictionary<string, int> ReforgeHistoryByType { get; set; }  // 装备ID -> 洗练次数
        public Dictionary<string, List<Dictionary<string, float>>> ReforgeAttributesHistory { get; set; }  // 装备ID -> 历史属性
    }

    /// <summary>
    /// 装备洗练槽位数据
    /// </summary>
    public class EquipmentReforgeSlot
    {
        public string EquipmentId { get; set; }
        public ReforgeType ReforgeType { get; set; }
        public ReforgeRarity OriginalRarity { get; set; }
        public ReforgeRarity TargetRarity { get; set; }
        public Dictionary<string, float> ReforgedAttributes { get; set; }
        public DateTime ReforgeTime { get; set; }
    }
}
