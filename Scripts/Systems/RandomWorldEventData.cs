// RandomWorldEventData.cs - 随机世界事件数据结构
using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// 随机世界事件类型
    /// </summary>
    public enum WorldEventType {
        ResourceSpawn,      // 资源刷新
        EnemyInvasion,     // 敌人入侵
        TreasureChest,     // 宝箱出现
        MerchantArrival,   // 商人到来
        WeatherChange,     // 天气变化
        LuckyMoment,       // 幸运时刻
        CurseEvent,        // 诅咒事件
        BlessingEvent,     // 祝福事件
        HiddenChest,       // 隐藏宝箱
        RARE_DragonAttack  // 稀有：巨龙袭击
    }

    /// <summary>
    /// 事件稀有度
    /// </summary>
    public enum EventRarity {
        Common = 1,     // 普通
        Uncommon = 2,   // 优秀
        Rare = 3,       // 稀有
        Epic = 4,       // 史诗
        Legendary = 5   // 传说
    }

    /// <summary>
    /// 世界事件数据
    /// </summary>
    [GlobalClass]
    public partial class WorldEventData : Resource {
        [Export] public string EventId { get; set; }
        [Export] public string EventName { get; set; }
        [Export] public string Description { get; set; }
        [Export] public WorldEventType EventType { get; set; }
        [Export] public EventRarity Rarity { get; set; }
        
        // 事件效果
        [Export] public int GoldReward { get; set; }
        [Export] public int ExpReward { get; set; }
        // [Export] removed: List<string> not Godot-exportable
        public List<string> ItemRewards { get; set; } = new List<string>();
        
        // 触发条件
        [Export] public int MinPlayerLevel { get; set; } = 1;
        [Export] public float TriggerChance { get; set; } = 0.1f; // 10%基础概率
        [Export] public int CooldownMinutes { get; set; } = 60; // 冷却时间
        
        // 持续时间
        [Export] public int DurationSeconds { get; set; } = 300; // 5分钟
        
        // 视觉/音效
        [Export] public string ParticleEffect { get; set; }
        [Export] public string SoundEffect { get; set; }
    }

    /// <summary>
    /// 玩家世界事件数据
    /// </summary>
    public class PlayerWorldEventData {
        public Dictionary<string, int> EventTriggerCount = new Dictionary<string, int>(); // 事件触发次数
        public Dictionary<string, DateTime> LastEventTime = new Dictionary<string, DateTime>(); // 上次触发时间
        public List<string> ActiveEvents = new List<string>(); // 当前活跃事件
        public int TotalEventsTriggered { get; set; } // 总触发次数
        public int LegendaryEventsWitnessed { get; set; } // 见证的传说事件
    }

    /// <summary>
    /// 世界事件配置
    /// </summary>
    public class WorldEventConfig {
        public WorldEventData Event { get; set; }
        public Vector2 WorldPosition { get; set; }
        public DateTime TriggerTime { get; set; }
        public DateTime ExpireTime { get; set; }
        public bool IsActive { get; set; }
    }
}
