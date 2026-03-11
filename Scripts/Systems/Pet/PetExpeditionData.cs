using Godot;
using System;
using System.Collections.Generic;

namespace GameSystems
{
    /// <summary>
    /// 宠物远征数据类型
    /// </summary>
    public class ExpeditionZone
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int RecommendedLevel { get; set; }
        public int DurationMinutes { get; set; }
        public int PetSlotsRequired { get; set; }
        
        // 奖励配置
        public int MinGoldReward { get; set; }
        public int MaxGoldReward { get; set; }
        public int MinExpReward { get; set; }
        public int MaxExpReward { get; set; }
        public List<string> PossibleItems { get; set; } = new List<string>();
        public float ItemDropChance { get; set; }
        
        // 成功条件
        public int RequiredPower { get; set; }
    }
    
    public class ExpeditionResult
    {
        public string ZoneId { get; set; }
        public bool Success { get; set; }
        public int GoldEarned { get; set; }
        public int ExpEarned { get; set; }
        public List<string> ItemsEarned { get; set; } = new List<string>();
        public string PetId { get; set; }
    }
    
    public class ActiveExpedition
    {
        public string ExpeditionId { get; set; }
        public string ZoneId { get; set; }
        public string PetId { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public bool Completed { get; set; }
        public ExpeditionResult Result { get; set; }
    }
    
    public class PlayerExpeditionData
    {
        public List<ActiveExpedition> ActiveExpeditions { get; set; } = new List<ActiveExpedition>();
        public List<ExpeditionResult> History { get; set; } = new List<ExpeditionResult>();
        public int TotalExpeditions { get; set; }
        public int TotalGoldEarned { get; set; }
        public int TotalExpEarned { get; set; }
        public Dictionary<string, int> ZoneCompletions { get; set; } = new Dictionary<string, int>();
    }
}
