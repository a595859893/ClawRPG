using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetRecycle {
    /// <summary>
    /// 宠物回收数据结构
    /// </summary>
    public class PetRecycleData : BaseSystem
    {
        // 已解锁的宠物类型
        public HashSet<string> UnlockedPetTypes { get; set; } = new HashSet<string>();
        
        // 回收历史记录
        public List<PetRecycleRecord> RecycleHistory { get; set; } = new List<PetRecycleRecord>();
        
        // 统计追踪
        public int TotalRecycled { get; set; } = 0;
        public int CommonRecycled { get; set; } = 0;
        public int UncommonRecycled { get; set; } = 0;
        public int RareRecycled { get; set; } = 0;
        public int EpicRecycled { get; set; } = 0;
        public int LegendaryRecycled { get; set; } = 0;
        public int TotalMaterials { get; set; } = 0;
        public int TotalExperience { get; set; } = 0;
        
        public override void _Ready()
        {
            base._Ready();
            GD.Print("[PetRecycleData] Initialized");
        }
    }
    
    /// <summary>
    /// 宠物回收记录
    /// </summary>
    public class PetRecycleRecord
    {
        public string PetType { get; set; } = "";
        public string PetName { get; set; } = "";
        public string Rarity { get; set; } = "";
        public int Level { get; set; } = 1;
        public List<MaterialReward> Materials { get; set; } = new List<MaterialReward>();
        public int ExperienceGained { get; set; } = 0;
        public int Timestamp { get; set; } = 0;
    }
    
    /// <summary>
    /// 材料奖励
    /// </summary>
    public class MaterialReward
    {
        public string MaterialId { get; set; } = "";
        public string MaterialName { get; set; } = "";
        public int Quantity { get; set; } = 0;
        public int Value { get; set; } = 0;
    }

        public override Dictionary ExportSaveData() => new();
        public override void ImportSaveData(Dictionary data) { }
}
