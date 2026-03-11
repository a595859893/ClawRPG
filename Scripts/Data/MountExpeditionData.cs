using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 坐骑远征数据类型
    /// </summary>
    public class MountExpeditionData
    {
        /// <summary>
        /// 远征区域
        /// </summary>
        public class ExpeditionZone
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public int RecommendedLevel { get; set; }
            public int DurationMinutes { get; set; }
            public int MountSlots { get; set; }
            public int MinGoldReward { get; set; }
            public int MaxGoldReward { get; set; }
            public int MinExpReward { get; set; }
            public int MaxExpReward { get; set; }
            public float BaseSuccessRate { get; set; }
            public List<string> ItemRewards { get; set; }
            
            public ExpeditionZone()
            {
                ItemRewards = new List<string>();
            }
        }
        
        /// <summary>
        /// 远征结果
        /// </summary>
        public class ExpeditionResult
        {
            public string ZoneId { get; set; }
            public bool Success { get; set; }
            public int GoldReward { get; set; }
            public int ExpReward { get; set; }
            public List<string> ItemRewards { get; set; }
            public string MountId { get; set; }
            public DateTime CompletedAt { get; set; }
            
            public ExpeditionResult()
            {
                ItemRewards = new List<string>();
            }
        }
        
        /// <summary>
        /// 活跃远征
        /// </summary>
        public class ActiveExpedition
        {
            public string ExpeditionId { get; set; }
            public string ZoneId { get; set; }
            public string MountId { get; set; }
            public DateTime StartTime { get; set; }
            public int DurationMinutes { get; set; }
            public bool Completed { get; set; }
            public bool Claimed { get; set; }
        }
        
        /// <summary>
        /// 玩家远征数据
        /// </summary>
        public class PlayerExpeditionData
        {
            public List<ActiveExpedition> ActiveExpeditions { get; set; }
            public List<ExpeditionResult> History { get; set; }
            public int TotalExpeditions { get; set; }
            public int TotalGoldEarned { get; set; }
            public int TotalExpEarned { get; set; }
            public Dictionary<string, int> ZoneCompletions { get; set; }
            
            public PlayerExpeditionData()
            {
                ActiveExpeditions = new List<ActiveExpedition>();
                History = new List<ExpeditionResult>();
                ZoneCompletions = new Dictionary<string, int>();
            }
        }
    }
}
