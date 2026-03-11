using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 宠物繁殖数据结构
    /// </summary>
    public class PetBreedingData
    {
        // 繁殖类型
        public enum BreedingType
        {
            Basic,      // 基础繁殖
            Advanced,   // 高级繁殖
            Legendary   // 传奇繁殖
        }
        
        // 繁殖状态
        public enum BreedingState
        {
            Idle,
            Ready,
            InProgress,
            Completed,
            Cancelled
        }
        
        // 宠物亲本数据
        public class ParentPet
        {
            public string PetId { get; set; }
            public string PetName { get; set; }
            public int Level { get; set; }
            public int Attack { get; set; }
            public int Defense { get; set; }
            public int Health { get; set; }
            public int Speed { get; set; }
            public float CritRate { get; set; }
            public float CritDamage { get; set; }
            public string Rarity { get; set; }
            public string Element { get; set; }
        }
        
        // 繁殖实例
        public class BreedingInstance
        {
            public string InstanceId { get; set; } = Guid.NewGuid().ToString();
            public string Parent1Id { get; set; }
            public string Parent2Id { get; set; }
            public ParentPet Parent1 { get; set; }
            public ParentPet Parent2 { get; set; }
            public BreedingType Type { get; set; }
            public DateTime StartTime { get; set; }
            public int DurationSeconds { get; set; }
            public BreedingState State { get; set; }
            public string OffspringId { get; set; }
            public bool Success { get; set; }
        }
        
        // 玩家繁殖数据
        public class PlayerBreedingData
        {
            public Dictionary<string, BreedingInstance> ActiveBreedings { get; set; } = new Dictionary<string, BreedingInstance>();
            public List<BreedingRecord> History { get; set; } = new List<BreedingRecord>();
            public int TotalBreedings { get; set; }
            public int SuccessfulBreedings { get; set; }
            public int LegendaryBreedings { get; set; }
        }
        
        // 繁殖记录
        public class BreedingRecord
        {
            public string RecordId { get; set; } = Guid.NewGuid().ToString();
            public string Parent1Name { get; set; }
            public string Parent2Name { get; set; }
            public string OffspringName { get; set; }
            public string OffspringRarity { get; set; }
            public DateTime BreedingTime { get; set; }
            public bool Success { get; set; }
            public int GoldCost { get; set; }
        }
    }
}
