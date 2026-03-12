using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 宠物探险数据类
    /// </summary>
    [Serializable]
    public class PetExpeditionData
    {
        public List<PlayerExpeditionData> PlayerData { get; set; } = new List<PlayerExpeditionData>();
        
        [Serializable]
        public class PlayerExpeditionData
        {
            public int TotalExpeditions { get; set; }
            public int SuccessfulExpeditions { get; set; }
            public int FailedExpeditions { get; set; }
            public int GoldEarned { get; set; }
            public int ExperienceGained { get; set; }
            public List<string> ItemsEarned { get; set; } = new List<string>();
            public int HighestRarityFound { get; set; } // 0=None, 1=Common, ..., 5=Legendary
        }
    }
    
    /// <summary>
    /// 探险类型
    /// </summary>
    public enum ExpeditionType
    {
        Forest,         // 森林探险 - 容易，低奖励
        Mountain,       // 山脉探险 - 中等难度
        Desert,         // 沙漠探险 - 中等难度
        Ocean,          // 海洋探险 - 较难
        Volcano,        // 火山探险 - 困难
        IcePeak,        // 冰峰探险 - 困难
        AncientRuins,   // 远古遗迹 - 非常困难
        DragonLair,     // 巨龙巢穴 - 极难
        ShadowRealm,    // 暗影领域 - 史诗难度
        CelestialRealm  // 天界领域 - 传说难度
    }
    
    /// <summary>
    /// 探险区域配置
    /// </summary>
    public class ExpeditionConfig
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public int DurationMinutes { get; set; }
        public int MinLevel { get; set; }
        public float SuccessRate { get; set; }
        public int[] GoldReward { get; set; } // min, max
        public int[] ExpReward { get; set; } // min, max
        public string[] ItemPool { get; set; }
        public float[] RarityWeights { get; set; } // Common, Uncommon, Rare, Epic, Legendary
    }
    
    /// <summary>
    /// 活跃的探险
    /// </summary>
    [Serializable]
    public class ActiveExpedition
    {
        public string PetId { get; set; }
        public string PetName { get; set; }
        public ExpeditionType Type { get; set; }
        public DateTime StartTime { get; set; }
        public int DurationMinutes { get; set; }
        public bool Completed { get; set; }
        public bool Success { get; set; }
        public int GoldReward { get; set; }
        public int ExpReward { get; set; }
        public string ItemReward { get; set; }
    }
}
