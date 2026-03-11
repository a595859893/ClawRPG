using Godot;
using Godot.Collections;
using System;

namespace ClawRPG.Scripts.Systems.GemSystem {
    /// <summary>
    /// 宝石合成配方数据库
    /// </summary>
    public static class GemFusionDatabase {
        private static Dictionary<string, GemFusionRecipe> _recipes;
        
        public static void Initialize() {
            _recipes = new Dictionary<string, GemFusionRecipe>();
            LoadFusionRecipes();
        }
        
        private static void LoadFusionRecipes() {
            // 为每种宝石类型和稀有度创建合成配方
            // 从普通合成到优秀，从优秀合成到稀有，等等
            string[] gemTypes = { "ruby", "sapphire", "emerald", "diamond", "topaz", "amethyst", "onyx", "pearl" };
            string[] rarities = { "common", "uncommon", "rare", "epic" };
            int[] gemCounts = { 2, 2, 2, 2 }; // 每个等级需要2个
            int[] goldCosts = { 50, 200, 800, 3200 }; // 金币费用
            float[] successRates = { 0.9f, 0.8f, 0.7f, 0.6f }; // 成功率
            
            for (int i = 0; i < gemTypes.Length; i++) {
                for (int j = 0; j < rarities.Length; j++) {
                    string sourceGemId = $"{gemTypes[i]}_{rarities[j]}";
                    string resultGemId = $"{gemTypes[i]}_{rarities[j + 1]}";
                    string recipeId = $"fusion_{sourceGemId}_to_{resultGemId}";
                    
                    var recipe = new GemFusionRecipe(
                        recipeId,
                        resultGemId,
                        sourceGemId,
                        gemCounts[j],
                        goldCosts[j],
                        successRates[j]
                    );
                    
                    _recipes[recipeId] = recipe;
                }
            }
            
            // 添加传说宝石的特殊合成配方 (需要3个史诗宝石 + 材料)
            for (int i = 0; i < gemTypes.Length; i++) {
                string sourceGemId = $"{gemTypes[i]}_epic";
                string resultGemId = $"{gemTypes[i]}_legendary";
                string recipeId = $"fusion_{sourceGemId}_to_{resultGemId}";
                
                var recipe = new GemFusionRecipe(
                    recipeId,
                    resultGemId,
                    sourceGemId,
                    3, // 需要3个史诗宝石
                    10000, // 10000金币
                    0.5f // 50%成功率
                );
                // 添加特殊材料
                recipe.Materials["magic_crystal"] = 5;
                
                _recipes[recipeId] = recipe;
            }
        }
        
        public static GemFusionRecipe GetRecipe(string recipeId) {
            if (_recipes == null) Initialize();
            return _recipes.TryGetValue(recipeId, out var recipe) ? recipe : null;
        }
        
        public static GemFusionRecipe GetRecipeByGems(string sourceGemId) {
            if (_recipes == null) Initialize();
            
            foreach (var recipe in _recipes.Values) {
                if (recipe.SourceGemId == sourceGemId) {
                    return recipe;
                }
            }
            return null;
        }
        
        public static List<GemFusionRecipe> GetAllRecipes() {
            if (_recipes == null) Initialize();
            return new List<GemFusionRecipe>(_recipes.Values);
        }
        
        public static List<GemFusionRecipe> GetRecipesForGem(string gemId) {
            if (_recipes == null) Initialize();
            
            var result = new List<GemFusionRecipe>();
            foreach (var recipe in _recipes.Values) {
                if (recipe.SourceGemId == gemId) {
                    result.Add(recipe);
                }
            }
            return result;
        }
        
        public static bool CanFuse(string gemId) {
            return GetRecipeByGems(gemId) != null;
        }
    }
}
