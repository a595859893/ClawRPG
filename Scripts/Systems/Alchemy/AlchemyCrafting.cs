using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Alchemy
{
    /// <summary>
    /// 炼金合成逻辑 - 处理物品合成过程
    /// </summary>
    public partial class AlchemyCrafting : BaseSystem
    {
        /// <summary>
        /// 合成结果
        /// </summary>
        public class CraftResult
        {
            public bool Success { get; set; }
            public int ResultItemId { get; set; }
            public int ResultQuantity { get; set; }
            public string Message { get; set; }
            public int ExperienceGained { get; set; }
        }
        
        private AlchemyRecipeDB _recipeDB;
        
        public override void _Ready()
        {
            base._Ready();
            _recipeDB = GetNode<AlchemyRecipeDB>("/root/AlchemyRecipeDB");
            if (_recipeDB == null)
            {
                _recipeDB = new AlchemyRecipeDB();
            }
        }
        
        /// <summary>
        /// 尝试合成
        /// </summary>
        public CraftResult TryCraft(int recipeId, Dictionary<int, int> availableMaterials, int playerLevel, int gold)
        {
            var recipe = _recipeDB.GetRecipe(recipeId);
            if (recipe == null)
            {
                return new CraftResult
                {
                    Success = false,
                    Message = "未知的配方"
                };
            }
            
            // 检查玩家等级
            if (playerLevel < recipe.RequiredAlchemyLevel)
            {
                return new CraftResult
                {
                    Success = false,
                    Message = $"需要炼金等级 {recipe.RequiredAlchemyLevel}"
                };
            }
            
            // 检查金币
            if (gold < recipe.GoldCost)
            {
                return new CraftResult
                {
                    Success = false,
                    Message = $"需要 {recipe.GoldCost} 金币"
                };
            }
            
            // 检查材料
            foreach (var requirement in recipe.Requirements)
            {
                int available = availableMaterials.ContainsKey(requirement.MaterialId) 
                    ? availableMaterials[requirement.MaterialId] 
                    : 0;
                
                if (available < requirement.Quantity)
                {
                    var material = GetMaterialName(requirement.MaterialId);
                    return new CraftResult
                    {
                        Success = false,
                        Message = $"材料不足: {material} x{requirement.Quantity}"
                    };
                }
            }
            
            // 计算成功率
            bool success = CheckSuccess(recipe.SuccessRate);
            
            if (success)
            {
                return new CraftResult
                {
                    Success = true,
                    ResultItemId = recipe.ResultItemId,
                    ResultQuantity = recipe.ResultQuantity,
                    Message = $"成功合成 {recipe.Name}!",
                    ExperienceGained = CalculateExperience(recipe)
                };
            }
            else
            {
                return new CraftResult
                {
                    Success = false,
                    Message = $"合成失败! 返还部分材料。"
                };
            }
        }
        
        /// <summary>
        /// 检查是否成功
        /// </summary>
        private bool CheckSuccess(float successRate)
        {
            var random = new Random();
            return random.NextDouble() < successRate;
        }
        
        /// <summary>
        /// 计算获得经验
        /// </summary>
        private int CalculateExperience(AlchemyRecipe recipe)
        {
            // 基础经验 = 配方等级 * 10
            int baseExp = recipe.RequiredAlchemyLevel * 10;
            
            // 稀有度加成
            float rarityMultiplier = 1.0f;
            
            return (int)(baseExp * rarityMultiplier);
        }
        
        /// <summary>
        /// 获取材料名称
        /// </summary>
        private string GetMaterialName(int materialId)
        {
            // 可以从材料数据库获取
            return $"材料{materialId}";
        }
        
        /// <summary>
        /// 消耗材料
        /// </summary>
        public Dictionary<int, int> ConsumeMaterials(Dictionary<int, int> availableMaterials, List<AlchemyRecipeRequirement> requirements)
        {
            var remaining = new Dictionary<int, int>(availableMaterials);
            
            foreach (var requirement in requirements)
            {
                if (remaining.ContainsKey(requirement.MaterialId))
                {
                    remaining[requirement.MaterialId] -= requirement.Quantity;
                    if (remaining[requirement.MaterialId] <= 0)
                    {
                        remaining.Remove(requirement.MaterialId);
                    }
                }
            }
            
            return remaining;
        }
        
        /// <summary>
        /// 计算合成所需金币
        /// </summary>
        public int CalculateGoldCost(int recipeId)
        {
            var recipe = _recipeDB.GetRecipe(recipeId);
            return recipe != null ? recipe.GoldCost : 0;
        }
        
        /// <summary>
        /// 检查是否可以合成
        /// </summary>
        public bool CanCraft(int recipeId, Dictionary<int, int> availableMaterials, int playerLevel, int gold)
        {
            var recipe = _recipeDB.GetRecipe(recipeId);
            if (recipe == null) return false;
            
            if (playerLevel < recipe.RequiredAlchemyLevel) return false;
            if (gold < recipe.GoldCost) return false;
            
            foreach (var requirement in recipe.Requirements)
            {
                int available = availableMaterials.ContainsKey(requirement.MaterialId) 
                    ? availableMaterials[requirement.MaterialId] 
                    : 0;
                
                if (available < requirement.Quantity) return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// 获取合成所需材料列表
        /// </summary>
        public List<AlchemyRecipeRequirement> GetRequirements(int recipeId)
        {
            var recipe = _recipeDB.GetRecipe(recipeId);
            return recipe != null ? recipe.Requirements : new List<AlchemyRecipeRequirement>();
        }
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 加载数据
        }
    }
}
