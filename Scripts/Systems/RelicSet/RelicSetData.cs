using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 遗物套装数据
    /// </summary>
    public class RelicSetData
    {
        public class RelicSet
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int PieceCount { get; set; }  // 套装件数
            public List<string> RelicIds { get; set; }  // 包含的遗物ID列表
            public Dictionary<string, float> SetBonuses { get; set; }  // 套装加成: 件数 -> 加成属性
            public string Icon { get; set; }
        }

        // 玩家已解锁的遗物套装
        public List<string> UnlockedSetIds { get; set; } = new List<string>();
        
        // 玩家已装备的遗物ID列表
        public List<string> EquippedRelicIds { get; set; } = new List<string>();
        
        // 套装收集统计
        public Dictionary<string, int> SetCompletionCounts { get; set; } = new Dictionary<string, int>();
    }
}
