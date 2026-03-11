using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetFoster
{
    /// <summary>
    /// 宠物寄养数据
    /// </summary>
    public enum FosterType
    {
        Rest,       // 休息 - 恢复饱食度
        Training,   // 训练 - 获得经验
        Gathering,  // 采集 - 获得材料
        Play,       // 玩耍 - 提升好感度
        Guard       // 守护 - 获得金币
    }
    
    public enum FosterStatus
    {
        Available,  // 可用
        Fostering,  // 寄养中
        Completed   // 完成待领取
    }
    
    /// <summary>
    /// 寄养配置
    /// </summary>
    public class FosterConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public FosterType Type { get; set; }
        public int Duration { get; set; }  // 寄养时长(秒)
        public int Cost { get; set; }     // 寄养费用(金币)
        public int MinPetLevel { get; set; }
        
        // 奖励配置
        public int ExpReward { get; set; }
        public int GoldReward { get; set; }
        public int AffectionReward { get; set; }
        public List<string> MaterialRewards { get; set; }  // 材料奖励ID列表
        public float MaterialDropChance { get; set; }
    }
    
    /// <summary>
    /// 玩家寄养数据
    /// </summary>
    public class PlayerFosterData
    {
        public Dictionary<string, ActiveFoster> ActiveFosters { get; set; }  // petId -> ActiveFoster
        public List<FosterRecord> History { get; set; }  // 历史记录
        public int TotalFosters { get; set; }
        public int TotalExpGained { get; set; }
        public int TotalGoldEarned { get; set; }
        public int TotalMaterialsGained { get; set; }
    }
    
    /// <summary>
    /// 活跃寄养
    /// </summary>
    public class ActiveFoster
    {
        public string PetId { get; set; }
        public string ConfigId { get; set; }
        public FosterType Type { get; set; }
        public long StartTime { get; set; }
        public int Duration { get; set; }
        public FosterStatus Status { get; set; }
        
        // 预计奖励
        public int ExpReward { get; set; }
        public int GoldReward { get; set; }
        public int AffectionReward { get; set; }
    }
    
    /// <summary>
    /// 寄养记录
    /// </summary>
    public class FosterRecord
    {
        public string PetId { get; set; }
        public string PetName { get; set; }
        public FosterType Type { get; set; }
        public long CompletedTime { get; set; }
        public int ExpGained { get; set; }
        public int GoldEarned { get; set; }
        public int AffectionGained { get; set; }
        public List<string> MaterialsGained { get; set; }
    }
}
