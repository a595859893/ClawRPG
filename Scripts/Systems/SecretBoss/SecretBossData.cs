using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Secret Boss System - 隐藏Boss系统
    /// 只有满足特定条件时才会出现的特殊Boss
    /// </summary>
    public class SecretBossData {
        public string BossId { get; set; }
        public string BossName { get; set; }
        public string Description { get; set; }
        
        // Boss类型
        public SecretBossType Type { get; set; }
        
        // 稀有度
        public Rarity Rarity { get; set; }
        
        // 出现条件
        public SecretBossCondition Condition { get; set; }
        
        // 基础属性
        public int BaseHealth { get; set; }
        public int BaseAttack { get; set; }
        public int BaseDefense { get; set; }
        public float AttackSpeed { get; set; }
        public float MoveSpeed { get; set; }
        
        // 特殊能力
        public List<string> SpecialAbilities { get; set; } = new List<string>();
        
        // 掉落物品
        public List<SecretBossDrop> Drops { get; set; } = new List<SecretBossDrop>();
        
        // 出现提示消息
        public string SpawnMessage { get; set; }
        
        // 是否已被发现/击败
        public bool IsDiscovered { get; set; }
        public bool IsDefeated { get; set; }
        public int DefeatCount { get; set; }
        
        // 图标
        public string IconPath { get; set; }
        
        // 背景故事
        public string Lore { get; set; }
    }
    
    public enum SecretBossType {
        Shadow,      // 暗影型
        Temporal,    // 时间型
        Chaos,       // 混沌型
        Ancient,     // 远古型
        Celestial,   // 星辰型
        Abyssal,     // 深渊型
        Phantom,     // 幻影型
        Divine       // 神性型
    }
    
    public enum Rarity {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary
    }
    
    public class SecretBossCondition {
        // 条件类型
        public ConditionType Type { get; set; }
        
        // 数值要求
        public int RequiredValue { get; set; }
        
        // 位置要求 (区域ID)
        public string RequiredArea { get; set; }
        
        // 时间要求 (小时)
        public int? RequiredHourStart { get; set; }
        public int? RequiredHourEnd { get; set; }
        
        // 天气要求
        public WeatherType? RequiredWeather { get; set; }
        
        // 装备要求 (物品ID列表)
        public List<string> RequiredItems { get; set; } = new List<string>();
        
        // 前置Boss (需要先击败)
        public string RequiredBossDefeated { get; set; }
        
        // 玩家等级要求
        public int? RequiredPlayerLevel { get; set; }
        
        // 击杀数量要求
        public string? RequiredKillCount { get; set; }
        public int RequiredKillAmount { get; set; }
        
        // 幸运值要求
        public int? RequiredLuck { get; set; }
        
        // 世界状态要求
        public string? RequiredWorldFlag { get; set; }
    }
    
    public enum ConditionType {
        TimeOfDay,       // 特定时间
        Weather,         // 特定天气
        KillCount,       // 击杀数量
        PlayerLevel,     // 玩家等级
        Location,       // 特定位置
        Equipment,       // 装备特定物品
        QuestComplete,  // 完成特定任务
        WorldFlag,      // 世界状态
        Luck,           // 幸运值
        BossDefeated,   // 击败特定Boss
        MoonPhase,      // 月相
        ComboCount      // 连击数
    }
    
    public enum WeatherType {
        Clear,
        Cloudy,
        Rain,
        Snow,
        Thunderstorm,
        Fog,
        Sandstorm,
        Hail,
        Blizzard,
        Storm
    }
    
    public class SecretBossDrop {
        public string ItemId { get; set; }
        public string ItemName { get; set; }
        public int MinQuantity { get; set; }
        public int MaxQuantity { get; set; }
        public float DropRate { get; set; }
        public bool IsGuaranteed { get; set; }
    }
    
    public class SecretBossSpawnInfo {
        public string BossId { get; set; }
        public Vector3 Position { get; set; }
        public float SpawnTime { get; set; }
        public float Duration { get; set; }
        public bool IsActive { get; set; }
    }
}
