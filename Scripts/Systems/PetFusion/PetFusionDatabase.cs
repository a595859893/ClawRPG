using Godot;
using System;
using System.Collections.Generic;

public class PetFusionDatabase {
    // 宠物融合数据库 - 定义所有融合配方
    
    // 融合类型枚举
    public enum FusionType {
        ElementalCombine,    // 元素融合
        BeastMerge,          // 野兽合并
        MythicalBlend,       // 神话混合
        ShadowFusion,        // 暗影融合
        CelestialMerge,      // 星辰融合
        ChaosCombine         // 混沌融合
    }
    
    // 稀有度权重
    private static readonly Dictionary<string, float> RarityWeights = new Dictionary<string, float> {
        { "Common", 40.0f },
        { "Uncommon", 30.0f },
        { "Rare", 20.0f },
        { "Epic", 8.0f },
        { "Legendary", 2.0f }
    };
    
    // 融合配方 - 定义哪些宠物类型可以融合
    private static readonly Dictionary<string, List<FusionRecipe>> FusionRecipes = new Dictionary<string, List<FusionRecipe>> {
        // 火系融合
        { "Fire", new List<FusionRecipe> {
            new FusionRecipe { ResultType = "Inferno Wolf", Rarity = "Rare", RequiredTypes = new List<string> { "Fire", "Beast" } },
            new FusionRecipe { ResultType = "Phoenix", Rarity = "Legendary", RequiredTypes = new List<string> { "Fire", "Elemental" } },
            new FusionRecipe { ResultType = "Volcanic Dragon", Rarity = "Epic", RequiredTypes = new List<string> { "Fire", "Dragon" } },
        }},
        // 水系融合
        { "Water", new List<FusionRecipe> {
            new FusionRecipe { ResultType = "Sea Serpent", Rarity = "Rare", RequiredTypes = new List<string> { "Water", "Beast" } },
            new FusionRecipe { ResultType = "Tidal Guardian", Rarity = "Epic", RequiredTypes = new List<string> { "Water", "Elemental" } },
            new FusionRecipe { ResultType = "Ocean Dragon", Rarity = "Legendary", RequiredTypes = new List<string> { "Water", "Dragon" } },
        }},
        // 冰系融合
        { "Ice", new List<FusionRecipe> {
            new FusionRecipe { ResultType = "Frost Bear", Rarity = "Rare", RequiredTypes = new List<string> { "Ice", "Beast" } },
            new FusionRecipe { ResultType = "Winter Spirit", Rarity = "Epic", RequiredTypes = new List<string> { "Ice", "Elemental" } },
            new FusionRecipe { ResultType = "Glacial Dragon", Rarity = "Legendary", RequiredTypes = new List<string> { "Ice", "Dragon" } },
        }},
        // 雷电系融合
        { "Lightning", new List<FusionRecipe> {
            new FusionRecipe { ResultType = "Thunder Panther", Rarity = "Rare", RequiredTypes = new List<string> { "Lightning", "Beast" } },
            new FusionRecipe { ResultType = "Storm Elemental", Rarity = "Epic", RequiredTypes = new List<string> { "Lightning", "Elemental" } },
            new FusionRecipe { ResultType = "Thunder Dragon", Rarity = "Legendary", RequiredTypes = new List<string> { "Lightning", "Dragon" } },
        }},
        // 暗影系融合
        { "Shadow", new List<FusionRecipe> {
            new FusionRecipe { ResultType = "Shadow Wolf", Rarity = "Rare", RequiredTypes = new List<string> { "Shadow", "Beast" } },
            new FusionRecipe { ResultType = "Void Walker", Rarity = "Epic", RequiredTypes = new List<string> { "Shadow", "Elemental" } },
            new FusionRecipe { ResultType = "Abyss Dragon", Rarity = "Legendary", RequiredTypes = new List<string> { "Shadow", "Dragon" } },
        }},
        // 光明系融合
        { "Holy", new List<FusionRecipe> {
            new FusionRecipe { ResultType = "Light Lion", Rarity = "Rare", RequiredTypes = new List<string> { "Holy", "Beast" } },
            new FusionRecipe { ResultType = "Celestial Being", Rarity = "Epic", RequiredTypes = new List<string> { "Holy", "Elemental" } },
            new FusionRecipe { ResultType = "Divine Dragon", Rarity = "Legendary", RequiredTypes = new List<string> { "Holy", "Dragon" } },
        }},
        // 普通系融合
        { "Common", new List<FusionRecipe> {
            new FusionRecipe { ResultType = "Mighty Bear", Rarity = "Uncommon", RequiredTypes = new List<string> { "Common", "Beast" } },
            new FusionRecipe { ResultType = "Ancient Golem", Rarity = "Rare", RequiredTypes = new List<string> { "Common", "Elemental" } },
            new FusionRecipe { ResultType = "Elder Dragon", Rarity = "Epic", RequiredTypes = new List<string> { "Common", "Dragon" } },
        }}
    };
    
    // 获取所有可用宠物类型
    public static List<string> GetAllPetTypes() {
        return new List<string> {
            "Fire", "Water", "Ice", "Lightning", "Shadow", "Holy", "Nature",
            "Beast", "Dragon", "Elemental", "Undead", "Slime", "Skeleton", "Common"
        };
    }
    
    // 获取融合配方
    public static List<FusionRecipe> GetRecipesForPetType(string petType) {
        if (FusionRecipes.ContainsKey(petType)) {
            return FusionRecipes[petType];
        }
        return new List<FusionRecipe>();
    }
    
    // 获取随机稀有度
    public static string GetRandomRarity() {
        float total = 0;
        foreach (var weight in RarityWeights.Values) {
            total += weight;
        }
        
        float random = (float)GD.RandDouble() * total;
        float cumulative = 0;
        
        foreach (var kvp in RarityWeights) {
            cumulative += kvp.Value;
            if (random <= cumulative) {
                return kvp.Key;
            }
        }
        
        return "Common";
    }
    
    // 获取稀有度颜色
    public static Color GetRarityColor(string rarity) {
        switch (rarity) {
            case "Common": return new Color(0.7f, 0.7f, 0.7f);
            case "Uncommon": return new Color(0.2f, 0.8f, 0.2f);
            case "Rare": return new Color(0.2f, 0.5f, 1.0f);
            case "Epic": return new Color(0.6f, 0.3f, 0.8f);
            case "Legendary": return new Color(1.0f, 0.6f, 0.0f);
            default: return new Color(1f, 1f, 1f);
        }
    }
    
    // 获取稀有度数值
    public static int GetRarityValue(string rarity) {
        switch (rarity) {
            case "Common": return 1;
            case "Uncommon": return 2;
            case "Rare": return 3;
            case "Epic": return 4;
            case "Legendary": return 5;
            default: return 1;
        }
    }
    
    // 计算融合费用
    public static int CalculateFusionCost(string rarity, int parent1Level, int parent2Level) {
        int baseCost = 100;
        int rarityMultiplier = GetRarityValue(rarity);
        int levelBonus = (parent1Level + parent2Level) * 5;
        
        return baseCost * rarityMultiplier + levelBonus;
    }
    
    // 计算成功率
    public static float CalculateSuccessRate(string rarity) {
        switch (rarity) {
            case "Common": return 0.95f;
            case "Uncommon": return 0.85f;
            case "Rare": return 0.70f;
            case "Epic": return 0.50f;
            case "Legendary": return 0.30f;
            default: return 0.9f;
        }
    }
}

public class FusionRecipe {
    public string ResultType { get; set; } = "";
    public string Rarity { get; set; } = "Common";
    public List<string> RequiredTypes { get; set; } = new List<string>();
}
