using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Fishing
{
    /// <summary>
    /// 钓鱼系统数据类型定义
    /// </summary>
    
    // 钓鱼地点类型
    public enum FishingLocationType
    {
        River,        // 河流
        Lake,         // 湖泊
        Ocean,        // 海洋
        Swamp,        // 沼泽
        Waterfall,    // 瀑布
        Underground,  // 地下泉水
        Volcanic,     // 火山温泉
        Mystical      // 神秘水域
    }
    
    // 鱼类类型
    public enum FishType
    {
        Common,     // 普通
        Uncommon,   // 优秀
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary,  // 传说
        Mythic      // 神化
    }
    
    // 鱼类分类
    public enum FishCategory
    {
        Freshwater,  // 淡水鱼
        Saltwater,   // 海水鱼
        Crustacean,  // 甲壳类
        Reptile,     // 爬行类
        Mythical,    // 神话生物
        Special      // 特殊
    }
    
    // 钓鱼状态
    public enum FishingState
    {
        Idle,        // 空闲
        Casting,     // 抛竿中
        Waiting,     // 等待中
        Biting,      // 咬钩中
        Reeling,     // 收线中
        Caught,      // 钓获
        Escaped      // 逃脱
    }
    
    // 鱼竿类型
    public enum RodType
    {
        Bamboo,          // 竹竿
        Fiberglass,      // 玻璃钢
        Carbon,          // 碳纤维
        Master,          // 大师级
        Legendary,       // 传奇
        Mythic           // 神器
    }
    
    // 鱼饵类型
    public enum BaitType
    {
        Worm,           // 蚯蚓
        Insect,         // 昆虫
        Fish,           // 小鱼
        Fruit,          // 水果
        Special,        // 特殊
        Lure,           // 拟饵
        Fly             // 飞蝇
    }
    
    // 钓鱼环境效果
    public enum FishingEnvironmentEffect
    {
        None,
        Clear,
        Cloudy,
        Rain,
        Storm,
        Night,
        GoldenHour,
        BloodMoon,
        Festival
    }
    
    // 单条鱼的数据
    public class FishData
    {
        public string ID { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public FishType Rarity { get; set; }
        public FishCategory Category { get; set; }
        public int BaseValue { get; set; }           // 基础价值
        public int ExperienceReward { get; set; }     // 经验奖励
        public int MinWeight { get; set; }           // 最小重量(克)
        public int MaxWeight { get; set; }           // 最大重量(克)
        public List<FishingLocationType> Locations { get; set; }  // 出现地点
        public bool IsSeasonal { get; set; }         // 季节限定
        public string Season { get; set; }           // 限定季节
        public bool IsTimeLimited { get; set; }       // 时间限定
        public string TimeOfDay { get; set; }        // 限定时间
    }
    
    // 钓鱼记录
    public class FishingRecord
    {
        public string FishID { get; set; }
        public DateTime CaughtAt { get; set; }
        public string Location { get; set; }
        public int Weight { get; set; }              // 重量(克)
        public bool IsNewRecord { get; set; }        // 是否新纪录
        public int AttemptNumber { get; set; }        // 第几次尝试
    }
    
    // 钓鱼会话数据
    public class FishingSession
    {
        public DateTime StartTime { get; set; }
        public FishingLocationType Location { get; set; }
        public RodType Rod { get; set; }
        public BaitType Bait { get; set; }
        public int TotalAttempts { get; set; }
        public int SuccessfulCatches { get; set; }
        public int TotalValue { get; set; }
        public int TotalExperience { get; set; }
        public List<FishingRecord> Records { get; set; }
        public FishingState CurrentState { get; set; }
        public float CurrentProgress { get; set; }    // 当前进度(0-1)
        public FishData CurrentFish { get; set; }    // 当前咬钩的鱼
    }
    
    // 玩家钓鱼数据
    public class PlayerFishingData
    {
        public int TotalCatches { get; set; }        // 总钓获数
        public int TotalAttempts { get; set; }        // 总尝试次数
        public int TotalValue { get; set; }          // 总价值
        public int TotalExperience { get; set; }     // 总经验
        public int CurrentLevel { get; set; }        // 当前等级
        public int CurrentXP { get; set; }           // 当前经验
        public Dictionary<string, int> FishCaught { get; set; }  // 各鱼类钓获次数
        public Dictionary<FishingLocationType, int> LocationCatches { get; set; }  // 各地点钓获数
        public List<FishingRecord> RecentCatches { get; set; }   // 最近钓获
        public Dictionary<string, int> WeightRecords { get; set; }  // 各鱼类重量纪录
        public RodType EquippedRod { get; set; }    // 装备的鱼竿
        public BaitType PreferredBait { get; set; }   // 首选鱼饵
        public Dictionary<string, bool> UnlockedFish { get; set; }  // 解锁的鱼
        public int BiggestCatchWeight { get; set; }  // 最大钓获重量
        public string BiggestCatchFish { get; set; } // 最大钓获鱼类
        public int PerfectCatches { get; set; }     // 完美钓获次数
    }
    
    // 钓鱼统计
    public class FishingStatistics
    {
        public int TotalPlayTime { get; set; }       // 总钓鱼时间(分钟)
        public int TotalCatches { get; set; }       // 总钓获数
        public int TotalAttempts { get; set; }       // 总尝试次数
        public float SuccessRate { get; set; }        // 成功率
        public int TotalValue { get; set; }          // 总价值
        public int TotalExperience { get; set; }     // 总经验
        public int LongestStreak { get; set; }        // 最长连钓
        public int CurrentStreak { get; set; }       // 当前连钓
        public Dictionary<FishType, int> RarityCatches { get; set; }  // 各稀有度钓获
        public Dictionary<FishingLocationType, int> LocationStats { get; set; }  // 地点统计
        public string RarestFishCaught { get; set; }  // 钓获的最稀有鱼
        public int UniqueSpecies { get; set; }        // 独特物种数
    }
    
    // 钓鱼环境状态
    public class FishingEnvironmentState
    {
        public FishingEnvironmentEffect Effect { get; set; }
        public float CatchBonus { get; set; }        // 钓获加成
        public float RareBonus { get; set; }         // 稀有度加成
        public float ValueBonus { get; set; }         // 价值加成
        public string ActiveEvent { get; set; }      // 活跃事件
    }
}
