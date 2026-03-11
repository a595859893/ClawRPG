using Godot;
using System;
using System.Collections.Generic;

namespace Game
{
    /// <summary>
    /// 装备套装数据类型
    /// </summary>
    public class EquipmentSetData
    {
        // 套装类型枚举
        public enum SetType
        {
            Weapon,      // 武器套装
            Armor,       // 护甲套装
            Accessory,  // 饰品套装
            Mixed        // 混合套装
        }

        // 套装稀有度
        public enum SetRarity
        {
            Common,     // 普通
            Uncommon,   // 优秀
            Rare,       // 稀有
            Epic,       // 史诗
            Legendary   // 传说
        }

        /// <summary>
        /// 套装效果定义
        /// </summary>
        public class SetBonus
        {
            public int PieceCount { get; set; }  // 激活所需件数
            public string Description { get; set; }  // 效果描述
            public float AttackBonus { get; set; }   // 攻击加成
            public float DefenseBonus { get; set; }  // 防御加成
            public float HealthBonus { get; set; }   // 生命加成
            public float MagicBonus { get; set; }   // 魔法加成
            public float SpeedBonus { get; set; }   // 速度加成
            public float CritRateBonus { get; set; } // 暴击率加成
            public float CritDamageBonus { get; set; } // 暴击伤害加成
            public float LifeStealBonus { get; set; } // 生命偷取加成
            public float DodgeBonus { get; set; }    // 闪避加成
            public float EXPBonus { get; set; }      // 经验加成
            public float GoldBonus { get; set; }    // 金币加成
        }

        /// <summary>
        /// 套装物品定义
        /// </summary>
        public class SetItem
        {
            public string ItemId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public SetType Type { get; set; }
            public SetRarity Rarity { get; set; }
        }

        /// <summary>
        /// 套装定义
        /// </summary>
        public class EquipmentSet
        {
            public string SetId { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public SetType Type { get; set; }
            public SetRarity Rarity { get; set; }
            public List<SetItem> Items { get; set; } = new List<SetItem>();
            public List<SetBonus> Bonuses { get; set; } = new List<SetBonus>();
        }

        /// <summary>
        /// 玩家套装数据
        /// </summary>
        public class PlayerSetData
        {
            public Dictionary<string, List<string>> OwnedItems { get; set; } = new Dictionary<string, List<string>>();
            public Dictionary<string, int> ActivatedBonuses { get; set; } = new Dictionary<string, int>();
        }

        /// <summary>
        /// 套装统计
        /// </summary>
        public class SetStatistics
        {
            public int TotalSets { get; set; }
            public int CompletedSets { get; set; }
            public int MaxPieceCount { get; set; }
            public Dictionary<string, int> SetPieceCounts { get; set; } = new Dictionary<string, int>();
        }
    }
}
