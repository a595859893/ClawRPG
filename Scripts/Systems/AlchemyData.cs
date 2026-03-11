using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 炼金数据类型 - 材料、配方、产物
    /// </summary>
    
    // 炼金材料类型
    public enum AlchemyMaterialType
    {
        Herb,       // 草药
        Mineral,    // 矿物
        MonsterPart, // 怪物素材
        Fish,       // 鱼类（钓鱼系统）
        Crystal,    // 水晶
        Special     // 特殊材料
    }

    // 炼金材料稀有度
    public enum AlchemyMaterialRarity
    {
        Common,     // 普通
        Uncommon,   // 优秀
        Rare,       // 稀有
        Epic,       // 史诗
        Legendary   // 传说
    }

    // 炼金材料
    public class AlchemyMaterial
    {
        public int Id;
        public string Name;
        public string Description;
        public AlchemyMaterialType Type;
        public AlchemyMaterialRarity Rarity;
        public int Value; // 金币价值
    }

    // 炼金配方需求
    public class AlchemyRecipeRequirement
    {
        public int MaterialId;
        public int Quantity;
    }

    // 炼金配方
    public class AlchemyRecipe
    {
        public int Id;
        public string Name;
        public string Description;
        public int ResultItemId; // 结果物品ID（药水ID）
        public int ResultQuantity = 1;
        public List<AlchemyRecipeRequirement> Requirements = new List<AlchemyRecipeRequirement>();
        public int GoldCost; // 制作费用
        public int RequiredAlchemyLevel; // 需要的炼金等级
        public float CraftTime = 1.0f; // 制作时间（秒）
        
        // 成功率
        public float SuccessRate = 1.0f;
    }

    // 玩家炼金数据
    public class PlayerAlchemyData
    {
        public int AlchemyLevel = 1;
        public int CurrentExperience = 0;
        public int ExperienceToNextLevel => AlchemyLevel * 100;
        
        // 已解锁的配方ID列表
        public List<int> UnlockedRecipeIds = new List<int>();
        
        // 制作统计
        public int TotalCrafted = 0;
        public Dictionary<int, int> RecipeUsageCount = new Dictionary<int, int>();
    }
}
