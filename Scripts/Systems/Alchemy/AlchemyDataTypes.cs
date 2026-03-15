using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Alchemy
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
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public AlchemyMaterialType Type { get; set; }
        public AlchemyMaterialRarity Rarity { get; set; }
        public int Value { get; set; } // 金币价值
    }

    // 炼金配方需求
    public class AlchemyRecipeRequirement
    {
        public int MaterialId { get; set; }
        public int Quantity { get; set; }
    }

    // 炼金配方
    public class AlchemyRecipe
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public int ResultItemId { get; set; } // 结果物品ID（药水ID）
        public int ResultQuantity { get; set; } = 1;
        public List<AlchemyRecipeRequirement> Requirements { get; set; } = new List<AlchemyRecipeRequirement>();
        public int GoldCost { get; set; } // 制作费用
        public int RequiredAlchemyLevel { get; set; } // 需要的炼金等级
        public float CraftTime { get; set; } = 1.0f; // 制作时间（秒）
        
        // 成功率
        public float SuccessRate { get; set; } = 1.0f;
    }

    // 玩家炼金数据
    public class PlayerAlchemyData
    {
        public int AlchemyLevel { get; set; } = 1;
        public int CurrentExperience { get; set; }
        
        public int ExperienceToNextLevel => AlchemyLevel * 100;
        
        // 已解锁的配方ID列表
        public List<int> UnlockedRecipeIds { get; set; } = new List<int>();
        
        // 制作统计
        public int TotalCrafted { get; set; }
        public Dictionary<int, int> RecipeUsageCount { get; set; } = new Dictionary<int, int>();
    }
}
