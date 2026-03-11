using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Crafting;

/// <summary>
/// 钓鱼数据结构
/// </summary>
public class FishingData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // 钓到的物品
    public string ItemId { get; set; }
    public int MinQuantity { get; set; } = 1;
    public int MaxQuantity { get; set; } = 1;
    
    // 掉落概率
    public float DropChance { get; set; } = 1.0f;
    
    // 钓鱼要求
    public int RequiredFishingLevel { get; set; } = 1;
    public float RequiredMinLuck { get; set; } = 0;
    
    // 稀有度
    public ItemRarity Rarity { get; set; } = ItemRarity.Common;
    
    // 经验值
    public int ExperienceReward { get; set; } = 10;
}

/// <summary>
/// 钓鱼状态
/// </summary>
public enum FishingState
{
    Idle,           // 空闲
    Casting,        // 抛竿中
    Waiting,        // 等待中
    Biting,         // 鱼咬钩
    Reeling,        // 收线中
    Caught,         // 钓到了
    Missed          // 错过了
}

/// <summary>
/// 鱼竿数据
/// </summary>
public class FishingRodData
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    
    // 属性
    public int Durability { get; set; } = 100;
    public float CatchBonus { get; set; } = 1.0f;      // 捕获加成
    public float SpeedBonus { get; set; } = 1.0f;      // 收线速度加成
    public float LuckBonus { get; set; } = 1.0f;       // 幸运加成
    public int RequiredLevel { get; set; } = 1;
    
    // 耐久度消耗
    public int DurabilityPerCast { get; set; } = 1;
    
    // 价格
    public int Price { get; set; } = 100;
}

/// <summary>
/// 钓鱼技能数据
/// </summary>
public class FishingSkillData
{
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int ExperienceToNextLevel { get; set; } = 100;
    
    // 技能加成
    public float CatchBonus { get; set; } = 1.0f;
    public float LuckBonus { get; set; } = 1.0f;
    public float SpeedBonus { get; set; } = 1.0f;
    
    // 已解锁的鱼
    public List<string> UnlockedFish { get; set; } = new List<string>();
}
