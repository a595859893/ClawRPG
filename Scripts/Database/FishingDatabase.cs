using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Database;

/// <summary>
/// 钓鱼数据库
/// </summary>
public class FishingDatabase
{
    private static FishingDatabase _instance;
    public static FishingDatabase Instance => _instance ??= new FishingDatabase();
    
    // 鱼竿数据
    public Dictionary<string, FishingRodData> FishingRods { get; private set; } = new Dictionary<string, FishingRodData>();
    
    // 鱼类数据
    public Dictionary<string, FishingData> Fish { get; private set; } = new Dictionary<string, FishingData>();
    
    // 按稀有度分类
    public Dictionary<ItemRarity, List<string>> FishByRarity { get; private set; } = new Dictionary<ItemRarity, List<string>>();
    
    // 按钓鱼等级分类
    public Dictionary<int, List<string>> FishByLevel { get; private set; } = new Dictionary<int, List<string>>();
    
    private FishingDatabase()
    {
        InitializeFishingRods();
        InitializeFish();
    }
    
    private void InitializeFishingRods()
    {
        // 新手鱼竿
        FishingRods["rod_wooden"] = new FishingRodData
        {
            Id = "rod_wooden",
            Name = "木质鱼竿",
            Description = "最简单的鱼竿，适合初学者",
            Durability = 50,
            CatchBonus = 1.0f,
            SpeedBonus = 1.0f,
            LuckBonus = 1.0f,
            RequiredLevel = 1,
            DurabilityPerCast = 1,
            Price = 50
        };
        
        // 进阶鱼竿
        FishingRods["rod_bamboo"] = new FishingRodData
        {
            Id = "rod_bamboo",
            Name = "竹制鱼竿",
            Description = "轻便耐用的鱼竿",
            Durability = 100,
            CatchBonus = 1.2f,
            SpeedBonus = 1.1f,
            LuckBonus = 1.1f,
            RequiredLevel = 10,
            DurabilityPerCast = 1,
            Price = 200
        };
        
        // 精良鱼竿
        FishingRods["rod_iron"] = new FishingRodData
        {
            Id = "rod_iron",
            Name = "铁质鱼竿",
            Description = "坚固耐用的金属鱼竿",
            Durability = 200,
            CatchBonus = 1.4f,
            SpeedBonus = 1.2f,
            LuckBonus = 1.2f,
            RequiredLevel = 25,
            DurabilityPerCast = 1,
            Price = 500
        };
        
        // 稀有鱼竿
        FishingRods["rod_golden"] = new FishingRodData
        {
            Id = "rod_golden",
            Name = "黄金鱼竿",
            Description = "传说中的黄金鱼竿",
            Durability = 500,
            CatchBonus = 1.6f,
            SpeedBonus = 1.4f,
            LuckBonus = 1.4f,
            RequiredLevel = 40,
            DurabilityPerCast = 1,
            Price = 2000
        };
        
        // 史诗鱼竿
        FishingRods["rod_diamond"] = new FishingRodData
        {
            Id = "rod_diamond",
            Name = "钻石鱼竿",
            Description = "最顶级的钓鱼工具",
            Durability = 1000,
            CatchBonus = 2.0f,
            SpeedBonus = 1.6f,
            LuckBonus = 1.6f,
            RequiredLevel = 60,
            DurabilityPerCast = 1,
            Price = 10000
        };
    }
    
    private void InitializeFish()
    {
        // 普通鱼 (白色)
        AddFish(new FishingData
        {
            Id = "fish_small",
            Name = "小鱼",
            Description = "最常见的淡水鱼",
            ItemId = "fish_small",
            MinQuantity = 1,
            MaxQuantity = 3,
            DropChance = 0.35f,
            RequiredFishingLevel = 1,
            RequiredMinLuck = 0,
            Rarity = ItemRarity.Common,
            ExperienceReward = 5
        });
        
        AddFish(new FishingData
        {
            Id = "fish_medium",
            Name = "中等鱼",
            Description = "常见的食用鱼",
            ItemId = "fish_medium",
            MinQuantity = 1,
            MaxQuantity = 2,
            DropChance = 0.25f,
            RequiredFishingLevel = 5,
            RequiredMinLuck = 0,
            Rarity = ItemRarity.Common,
            ExperienceReward = 10
        });
        
        AddFish(new FishingData
        {
            Id = "fish_large",
            Name = "大鱼",
            Description = "体型较大的鱼",
            ItemId = "fish_large",
            MinQuantity = 1,
            MaxQuantity = 2,
            DropChance = 0.15f,
            RequiredFishingLevel = 10,
            RequiredMinLuck = 10,
            Rarity = ItemRarity.Common,
            ExperienceReward = 15
        });
        
        // 优秀鱼 (绿色)
        AddFish(new FishingData
        {
            Id = "fish_golden",
            Name = "金鱼",
            Description = "金色的锦鲤",
            ItemId = "fish_golden",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.12f,
            RequiredFishingLevel = 15,
            RequiredMinLuck = 20,
            Rarity = ItemRarity.Uncommon,
            ExperienceReward = 25
        });
        
        AddFish(new FishingData
        {
            Id = "fish_silver",
            Name = "银鱼",
            Description = "银光闪闪的鱼",
            ItemId = "fish_silver",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.10f,
            RequiredFishingLevel = 20,
            RequiredMinLuck = 25,
            Rarity = ItemRarity.Uncommon,
            ExperienceReward = 30
        });
        
        AddFish(new FishingData
        {
            Id = "fish_rainbow",
            Name = "彩虹鱼",
            Description = "鳞片呈现彩虹光泽",
            ItemId = "fish_rainbow",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.08f,
            RequiredFishingLevel = 25,
            RequiredMinLuck = 30,
            Rarity = ItemRarity.Uncommon,
            ExperienceReward = 40
        });
        
        // 稀有鱼 (蓝色)
        AddFish(new FishingData
        {
            Id = "fish_magic",
            Name = "魔法鱼",
           Description = "蕴含微弱魔力的鱼",
            ItemId = "fish_magic",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.06f,
            RequiredFishingLevel = 30,
            RequiredMinLuck = 40,
            Rarity = ItemRarity.Rare,
            ExperienceReward = 60
        });
        
        AddFish(new FishingData
        {
            Id = "fish_crystal",
            Name = "水晶鱼",
            Description = "身体透明如水晶",
            ItemId = "fish_crystal",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.05f,
            RequiredFishingLevel = 35,
            RequiredMinLuck = 45,
            Rarity = ItemRarity.Rare,
            ExperienceReward = 75
        });
        
        AddFish(new FishingData
        {
            Id = "fish_glowing",
            Name = "发光鱼",
            Description = "在黑暗中会发光",
            ItemId = "fish_glowing",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.04f,
            RequiredFishingLevel = 40,
            RequiredMinLuck = 50,
            Rarity = ItemRarity.Rare,
            ExperienceReward = 90
        });
        
        // 史诗鱼 (紫色)
        AddFish(new FishingData
        {
            Id = "fish_ancient",
            Name = "远古鱼",
            Description = "从远古时代存活至今",
            ItemId = "fish_ancient",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.03f,
            RequiredFishingLevel = 45,
            RequiredMinLuck = 60,
            Rarity = ItemRarity.Epic,
            ExperienceReward = 150
        });
        
        AddFish(new FishingData
        {
            Id = "fish_spirit",
            Name = "灵魂鱼",
            Description = "据说能看到灵魂",
            ItemId = "fish_spirit",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.02f,
            RequiredFishingLevel = 50,
            RequiredMinLuck = 70,
            Rarity = ItemRarity.Epic,
            ExperienceReward = 200
        });
        
        AddFish(new FishingData
        {
            Id = "fish_dragon",
            Name = "龙鱼",
            Description = "具有龙的血统",
            ItemId = "fish_dragon",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.015f,
            RequiredFishingLevel = 55,
            RequiredMinLuck = 80,
            Rarity = ItemRarity.Epic,
            ExperienceReward = 300
        });
        
        // 传说鱼 (橙色)
        AddFish(new FishingData
        {
            Id = "fish_legendary",
            Name = "传说之鱼",
            Description = "传说中的终极鱼类",
            ItemId = "fish_legendary",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.005f,
            RequiredFishingLevel = 60,
            RequiredMinLuck = 95,
            Rarity = ItemRarity.Legendary,
            ExperienceReward = 500
        });
        
        AddFish(new FishingData
        {
            Id = "fish_phantom",
            Name = "幻影鱼",
            Description = "只在月圆之夜出现",
            ItemId = "fish_phantom",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.003f,
            RequiredFishingLevel = 65,
            RequiredMinLuck = 100,
            Rarity = ItemRarity.Legendary,
            ExperienceReward = 750
        });
        
        AddFish(new FishingData
        {
            Id = "fish_eternal",
            Name = "永恒鱼",
            Description = "永远不会死亡的鱼",
            ItemId = "fish_eternal",
            MinQuantity = 1,
            MaxQuantity = 1,
            DropChance = 0.001f,
            RequiredFishingLevel = 70,
            RequiredMinLuck = 120,
            Rarity = ItemRarity.Legendary,
            ExperienceReward = 1000
        });
    }
    
    private void AddFish(FishingData fish)
    {
        Fish[fish.Id] = fish;
        
        // 按稀有度分类
        if (!FishByRarity.ContainsKey(fish.Rarity))
            FishByRarity[fish.Rarity] = new List<string>();
        FishByRarity[fish.Rarity].Add(fish.Id);
        
        // 按等级分类
        int level = fish.RequiredFishingLevel;
        if (!FishByLevel.ContainsKey(level))
            FishByLevel[level] = new List<string>();
        FishByLevel[level].Add(fish.Id);
    }
    
    /// <summary>
    /// 获取所有可用的鱼（根据等级和幸运）
    /// </summary>
    public List<FishingData> GetAvailableFish(int fishingLevel, float playerLuck)
    {
        var available = new List<FishingData>();
        
        foreach (var fish in Fish.Values)
        {
            if (fish.RequiredFishingLevel <= fishingLevel && 
                fish.RequiredMinLuck <= playerLuck)
            {
                available.Add(fish);
            }
        }
        
        return available;
    }
    
    /// <summary>
    /// 随机选择一条鱼（考虑概率）
    /// </summary>
    public FishingData RollFish(int fishingLevel, float playerLuck)
    {
        var available = GetAvailableFish(fishingLevel, playerLuck);
        if (available.Count == 0) return null;
        
        float totalChance = 0;
        foreach (var fish in available)
        {
            totalChance += fish.DropChance;
        }
        
        float roll = (float)GD.RandDouble() * totalChance;
        float current = 0;
        
        foreach (var fish in available)
        {
            current += fish.DropChance;
            if (roll <= current)
            {
                return fish;
            }
        }
        
        return available[0];
    }
    
    /// <summary>
    /// 获取鱼竿数据
    /// </summary>
    public FishingRodData GetFishingRod(string rodId)
    {
        return FishingRods.TryGetValue(rodId, out var rod) ? rod : null;
    }
    
    /// <summary>
    /// 获取所有鱼竿
    /// </summary>
    public List<FishingRodData> GetAllFishingRods()
    {
        return new List<FishingRodData>(FishingRods.Values);
    }
}
