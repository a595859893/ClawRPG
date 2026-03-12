using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.PetRecycle;

/// <summary>
/// 宠物回收数据库配置
/// </summary>
public class PetRecycleDatabase : Node
{
    // 宠物类型回收配置
    public static Dictionary<string, PetRecycleConfig> PetRecycleConfigs { get; private set; } = new()
    {
        // 狗类宠物
        { "Dog", new PetRecycleConfig 
            { 
                BaseGold = 50, 
                Materials = new List<RecycleMaterial>
                {
                    new() { MaterialId = "pet_fur", Name = "宠物毛皮", AmountMin = 1, AmountMax = 3, Weight = 80 },
                    new() { MaterialId = "bone", Name = "骨头", AmountMin = 1, AmountMax = 2, Weight = 20 }
                }
            } 
        },
        // 猫类宠物
        { "Cat", new PetRecycleConfig 
            { 
                BaseGold = 45, 
                Materials = new List<RecycleMaterial>
                {
                    new() { MaterialId = "pet_fur", Name = "宠物毛皮", AmountMin = 1, AmountMax = 3, Weight = 80 },
                    new() { MaterialId = "whisker", Name = "胡须", AmountMin = 1, AmountMax = 2, Weight = 20 }
                }
            } 
        },
        // 鸟类宠物
        { "Bird", new PetRecycleConfig 
            { 
                BaseGold = 40, 
                Materials = new List<RecycleMaterial>
                {
                    new() { MaterialId = "feather", Name = "羽毛", AmountMin = 2, AmountMax = 5, Weight = 80 },
                    new() { MaterialId = "beak", Name = "喙", AmountMin = 1, AmountMax = 1, Weight = 20 }
                }
            } 
        },
        // 兔子宠物
        { "Rabbit", new PetRecycleConfig 
            { 
                BaseGold = 35, 
                Materials = new List<RecycleMaterial>
                {
                    new() { MaterialId = "pet_fur", Name = "宠物毛皮", AmountMin = 1, AmountMax = 3, Weight = 80 },
                    new() { MaterialId = "carrot", Name = "胡萝卜", AmountMin = 1, AmountMax = 2, Weight = 20 }
                }
            } 
        },
        // 龙类宠物
        { "Dragon", new PetRecycleConfig 
            { 
                BaseGold = 200, 
                Materials = new List<RecycleMaterial>
                {
                    new() { MaterialId = "dragon_scale", Name = "龙鳞", AmountMin = 3, AmountMax = 8, Weight = 50 },
                    new() { MaterialId = "dragon_blood", Name = "龙血", AmountMin = 1, AmountMax = 3, Weight = 30 },
                    new() { MaterialId = "dragon_heart", Name = "龙心", AmountMin = 1, AmountMax = 1, Weight = 20 }
                }
            } 
        },
        // 史莱姆宠物
        { "Slime", new PetRecycleConfig 
            { 
                BaseGold = 25, 
                Materials = new List<RecycleMaterial>
                {
                    new() { MaterialId = "slime_gel", Name = "史莱姆凝胶", AmountMin = 2, AmountMax = 5, Weight = 80 },
                    new() { MaterialId = "slime_core", Name = "史莱姆核心", AmountMin = 1, AmountMax = 1, Weight = 20 }
                }
            } 
        },
        // 骷髅宠物
        { "Skeleton", new PetRecycleConfig 
            { 
                BaseGold = 60, 
                Materials = new List<RecycleMaterial>
                {
                    new() { MaterialId = "bone", Name = "骨头", AmountMin = 3, AmountMax = 6, Weight = 60 },
                    new() { MaterialId = "skull", Name = "骷髅", AmountMin = 1, AmountMax = 1, Weight = 20 },
                    new() { MaterialId = "soul_essence", Name = "灵魂精华", AmountMin = 1, AmountMax = 2, Weight = 20 }
                }
            } 
        },
        // 元素宠物
        { "Elemental", new PetRecycleConfig 
            { 
                BaseGold = 100, 
                Materials = new List<RecycleMaterial>
                {
                    new() { MaterialId = "element_crystal", Name = "元素水晶", AmountMin = 2, AmountMax = 5, Weight = 50 },
                    new() { MaterialId = "essence_fire", Name = "火焰精华", AmountMin = 1, AmountMax = 2, Weight = 20 },
                    new() { MaterialId = "essence_ice", Name = "冰霜精华", AmountMin = 1, AmountMax = 2, Weight = 20 },
                    new() { MaterialId = "essence_lightning", Name = "雷电精华", AmountMin = 1, AmountMax = 2, Weight = 20 }
                }
            } 
        }
    };
    
    // 稀有度加成
    public static Dictionary<string, RarityBonus> RarityBonuses { get; private set; } = new()
    {
        { "Common", new RarityBonus { GoldMultiplier = 1.0f, MaterialBonus = 1 } },
        { "Uncommon", new RarityBonus { GoldMultiplier = 1.5f, MaterialBonus = 2 } },
        { "Rare", new RarityBonus { GoldMultiplier = 2.5f, MaterialBonus = 3 } },
        { "Epic", new RarityBonus { GoldMultiplier = 4.0f, MaterialBonus = 5 } },
        { "Legendary", new RarityBonus { GoldMultiplier = 8.0f, MaterialBonus = 8 } }
    };
    
    // 等级加成
    public static float GetLevelBonus(int level)
    {
        return 1.0f + (level - 1) * 0.05f;
    }
    
    // 获取回收配置
    public static PetRecycleConfig GetConfig(string petType)
    {
        if (PetRecycleConfigs.TryGetValue(petType, out var config))
            return config;
        
        // 默认配置
        return new PetRecycleConfig
        {
            BaseGold = 50,
            Materials = new List<RecycleMaterial>
            {
                new() { MaterialId = "pet_fur", Name = "宠物毛皮", AmountMin = 1, AmountMax = 3, Weight = 80 },
                new() { MaterialId = "bone", Name = "骨头", AmountMin = 1, AmountMax = 2, Weight = 20 }
            }
        };
    }
    
    // 获取稀有度加成
    public static RarityBonus GetRarityBonus(string rarity)
    {
        if (RarityBonuses.TryGetValue(rarity, out var bonus))
            return bonus;
        return RarityBonuses["Common"];
    }
}

/// <summary>
/// 宠物回收配置
/// </summary>
public class PetRecycleConfig
{
    public int BaseGold { get; set; } = 50;
    public List<RecycleMaterial> Materials { get; set; } = new();
}

/// <summary>
/// 回收材料
/// </summary>
public class RecycleMaterial
{
    public string MaterialId { get; set; } = "";
    public string Name { get; set; } = "";
    public int AmountMin { get; set; } = 1;
    public int AmountMax { get; set; } = 1;
    public int Weight { get; set; } = 100;
}

/// <summary>
/// 稀有度加成
/// </summary>
public class RarityBonus
{
    public float GoldMultiplier { get; set; } = 1.0f;
    public int MaterialBonus { get; set; } = 1;
}
