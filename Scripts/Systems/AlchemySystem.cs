using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Alchemy system manager - handles recipe unlocking, crafting, and player alchemy data management.
    /// Uses singleton pattern to ensure global uniqueness.
    /// </summary>
    /// <remarks>
    /// Main features:
    /// - Recipe unlocking and management
    /// - Material consumption and item crafting
    /// - Alchemy experience and leveling system
    /// - Data persistence
    /// </remarks>
    public class AlchemySystem : BaseSystem
    {
        private static AlchemySystem _instance;
        
        /// <summary>
        /// Gets the singleton instance of AlchemySystem.
        /// </summary>
        public static AlchemySystem Instance => _instance ??= new AlchemySystem();

        private PlayerAlchemyData _playerData = new PlayerAlchemyData();
        private bool _isInitialized = false; 

        // Event system
        
        /// <summary>
        /// Fired when a craft attempt is made. Parameters: recipe object, success status.
        /// </summary>
        public static event Action<AlchemyRecipe, bool> OnCraftAttempt; 
        
        /// <summary>
        /// Fired when alchemy level increases. Parameters: new level.
        /// </summary>
        public static event Action<int> OnLevelUp; 
        
        /// <summary>
        /// Fired when a recipe is unlocked. Parameters: unlocked recipe object.
        /// </summary>
        public static event Action<AlchemyRecipe> OnRecipeUnlocked; 
        
        /// <summary>
        /// Fired when materials are obtained. Parameters: material object, quantity obtained.
        /// </summary>
        public static event Action<AlchemyMaterial, int> OnMaterialObtained; 

        /// <summary>
        /// Gets the player's alchemy data.
        /// </summary>
        public PlayerAlchemyData PlayerData => _playerData;

        /// <summary>
        /// Initialize the alchemy system and unlock basic recipes.
        /// </summary>
        public void Initialize()
        {
            if (_isInitialized) return;
            
            // 初始解锁一些基础配方
            UnlockRecipe(2001); // 小生命药水
            UnlockRecipe(2011); // 小法力药水
            UnlockRecipe(2051); // 速度药水
            UnlockRecipe(2081); // 解毒药水
            
            _isInitialized = true;
            GD.Print("[AlchemySystem] Initialized - Starting Alchemy Level: " + _playerData.AlchemyLevel);
        }

        /// <summary>
        /// Unlock the specified alchemy recipe.
        /// </summary>
        /// <param name="recipeId">ID of the recipe to unlock.</param>
        /// <returns>Returns true if unlocked successfully, false if already exists or doesn't exist.</returns>
        public bool UnlockRecipe(int recipeId)
        {
            if (_playerData.UnlockedRecipeIds.Contains(recipeId))
                return false;

            var recipe = AlchemyDatabase.Instance.GetRecipe(recipeId);
            if (recipe == null) return false;

            _playerData.UnlockedRecipeIds.Add(recipeId);
            OnRecipeUnlocked?.Invoke(recipe);
            GD.Print("[AlchemySystem] Unlocked Recipe: " + recipe.Name);
            return true;
        }

        /// <summary>
        /// Check if the specified recipe is unlocked.
        /// </summary>
        /// <param name="recipeId">Recipe ID.</param>
        /// <returns>Returns true if unlocked, otherwise false.</returns>
        public bool IsRecipeUnlocked(int recipeId)
        {
            return _playerData.UnlockedRecipeIds.Contains(recipeId);
        }

        /// <summary>
        /// Get list of all unlocked alchemy recipes.
        /// </summary>
        /// <returns>List of unlocked recipes.</returns>
        public List<AlchemyRecipe> GetUnlockedRecipes()
        {
            List<AlchemyRecipe> unlocked = new List<AlchemyRecipe>();
            foreach (var recipeId in _playerData.UnlockedRecipeIds)
            {
                var recipe = AlchemyDatabase.Instance.GetRecipe(recipeId);
                if (recipe != null)
                    unlocked.Add(recipe);
            }
            return unlocked;
        }

        /// <summary>
        /// Attempt to craft using the specified recipe.
        /// </summary>
        /// <param name="recipeId">Recipe ID.</param>
        <param name="itemId">Output parameter, ID of the crafted item on success.</param>
        /// <param name="quantity">Output parameter, quantity of the crafted item on success.</param>
        /// <param name="message">Output parameter, result message of the operation.</param>
        /// <returns>Returns true if craft succeeded, otherwise false.</returns>
        /// <remarks>
        /// Check process: recipe exists -> recipe unlocked -> level sufficient -> gold sufficient -> materials sufficient
        /// After success, deduct gold and materials, determine result based on success rate.
        /// </remarks>
        public bool TryCraft(int recipeId, out int itemId, out int quantity, out string message)
        {
            itemId = 0;
            quantity = 0;
            message = "";

            var recipe = AlchemyDatabase.Instance.GetRecipe(recipeId);
            if (recipe == null)
            {
                message = "配方不存在";
                return false;
            }

            if (!_playerData.UnlockedRecipeIds.Contains(recipeId))
            {
                message = "配方未解锁";
                return false;
            }

            if (_playerData.AlchemyLevel < recipe.RequiredAlchemyLevel)
            {
                message = $"需要炼金等级 {recipe.RequiredAlchemyLevel}";
                return false;
            }

            // 检查金币
            var inventory = PlayerInventory.Instance;
            if (inventory.Gold < recipe.GoldCost)
            {
                message = $"金币不足 (需要 {recipe.GoldCost})";
                return false;
            }

            // 检查材料
            foreach (var req in recipe.Requirements)
            {
                int playerCount = inventory.GetItemCount(req.MaterialId);
                if (playerCount < req.Quantity)
                {
                    var material = AlchemyDatabase.Instance.GetMaterial(req.MaterialId);
                    message = $"材料不足: {material?.Name ?? "未知材料"} (需要 {req.Quantity}, 有 {playerCount})";
                    return false;
                }
            }

            // 扣除金币
            inventory.RemoveGold(recipe.GoldCost);

            // 扣除材料
            foreach (var req in recipe.Requirements)
            {
                inventory.RemoveItem(req.MaterialId, req.Quantity);
            }

            // 随机成功判定
            bool success = new Random().NextDouble() < recipe.SuccessRate;

            _playerData.TotalCrafted++;
            if (!_playerData.RecipeUsageCount.ContainsKey(recipeId))
                _playerData.RecipeUsageCount[recipeId] = 0;
            _playerData.RecipeUsageCount[recipeId]++;

            if (success)
            {
                itemId = recipe.ResultItemId;
                quantity = recipe.ResultQuantity;
                
                // 添加产物到背包
                inventory.AddItem(itemId, quantity);

                // 获得经验
                AddExperience(recipe.RequiredAlchemyLevel * 20);

                message = $"成功制作: {recipe.Name} x{quantity}";
                GD.Print($"[AlchemySystem] Crafted: {recipe.Name}");
            }
            else
            {
                message = $"制作失败: {recipe.Name}";
                GD.Print($"[AlchemySystem] Craft Failed: {recipe.Name}");
            }

            OnCraftAttempt?.Invoke(recipe, success);
            return true;
        }

        /// <summary>
        /// Quick craft - automatically select best available recipe.
        /// </summary>
        /// <param name="targetItemId">Target item ID to craft.</param>
        /// <param name="message">Output parameter, result message of the operation.</param>
        /// <returns>Returns true if craft succeeded, otherwise false.</returns>
        public bool QuickCraft(int targetItemId, out string message)
        {
            var recipes = GetUnlockedRecipes();
            
            foreach (var recipe in recipes)
            {
                if (recipe.ResultItemId == targetItemId)
                {
                    return TryCraft(recipe.Id, out _, out _, out message);
                }
            }

            message = "没有可用的配方";
            return false;
        }

        /// <summary>
        /// Check if the specified recipe can be crafted.
        /// </summary>
        /// <param name="recipeId">Recipe ID.</param>
        /// <returns>Returns true if can craft, otherwise false.</returns>
        public bool CanCraft(int recipeId)
        {
            var recipe = AlchemyDatabase.Instance.GetRecipe(recipeId);
            if (recipe == null) return false;

            if (!_playerData.UnlockedRecipeIds.Contains(recipeId)) return false;
            if (_playerData.AlchemyLevel < recipe.RequiredAlchemyLevel) return false;

            var inventory = PlayerInventory.Instance;
            if (inventory.Gold < recipe.GoldCost) return false;

            foreach (var req in recipe.Requirements)
            {
                if (inventory.GetItemCount(req.MaterialId) < req.Quantity)
                    return false;
            }

            return true;
        }

        /// <summary>
        /// Add alchemy experience, may trigger level up.
        /// </summary>
        /// <param name="amount">Amount of experience to add.</param>
        public void AddExperience(int amount)
        {
            _playerData.CurrentExperience += amount;
            
            while (_playerData.CurrentExperience >= _playerData.ExperienceToNextLevel)
            {
                _playerData.CurrentExperience -= _playerData.ExperienceToNextLevel;
                _playerData.AlchemyLevel++;
                GD.Print($"[AlchemySystem] Level Up! New Level: {_playerData.AlchemyLevel}");
                OnLevelUp?.Invoke(_playerData.AlchemyLevel);
            }
        }

        /// <summary>
        /// Obtain specified alchemy materials, added to player inventory.
        /// </summary>
        /// <param name="materialId">Material ID.</param>
        /// <param name="quantity">Quantity to obtain, default is 1.</param>
        public void ObtainMaterial(int materialId, int quantity = 1)
        {
            var material = AlchemyDatabase.Instance.GetMaterial(materialId);
            if (material == null) return;

            var inventory = PlayerInventory.Instance;
            
            // 添加为普通物品
            var item = new Item
            {
                Id = materialId,
                Name = material.Name,
                Description = material.Description,
                Type = ItemType.Material,
                Rarity = (ItemRarity)(int)material.Rarity,
                Value = material.Value,
                Quantity = quantity,
                MaxStack = 99
            };

            inventory.AddItem(item);
            OnMaterialObtained?.Invoke(material, quantity);
            
            GD.Print($"[AlchemySystem] Obtained Material: {material.Name} x{quantity}");
        }

        /// <summary>
        /// Randomly obtain alchemy materials of specified rarity.
        /// </summary>
        /// <param name="rarity">Target rarity.</param>
        public void ObtainRandomMaterial(AlchemyMaterialRarity rarity)
        {
            var material = AlchemyDatabase.Instance.GetRandomMaterialByRarity(rarity);
            if (material != null)
            {
                ObtainMaterial(material.Id, 1);
            }
        }

        /// <summary>
        /// Get required materials for the specified recipe.
        /// </summary>
        /// <param name="recipeId">Recipe ID.</param>
        /// <returns>Dictionary of material ID to required quantity.</returns>
        public Dictionary<int, int> GetRequiredMaterials(int recipeId)
        {
            var requirements = new Dictionary<int, int>();
            var recipe = AlchemyDatabase.Instance.GetRecipe(recipeId);
            
            if (recipe != null)
            {
                foreach (var req in recipe.Requirements)
                {
                    requirements[req.MaterialId] = req.Quantity;
                }
            }
            
            return requirements;
        }

        /// <summary>
        /// Get missing materials for the specified recipe.
        /// </summary>
        /// <param name="recipeId">Recipe ID.</param>
        /// <returns>Dictionary of material ID to missing quantity.</returns>
        public Dictionary<int, int> GetMissingMaterials(int recipeId)
        {
            var missing = new Dictionary<int, int>();
            var recipe = AlchemyDatabase.Instance.GetRecipe(recipeId);
            var inventory = PlayerInventory.Instance;

            if (recipe != null)
            {
                foreach (var req in recipe.Requirements)
                {
                    int playerCount = inventory.GetItemCount(req.MaterialId);
                    if (playerCount < req.Quantity)
                    {
                        missing[req.MaterialId] = req.Quantity - playerCount;
                    }
                }
            }

            return missing;
        }

        /// <summary>
        /// Save player's alchemy data.
        /// </summary>
        /// <returns>Dictionary containing alchemy data for persistence.</returns>
        public Dictionary<string, object> SaveData()
        {
            return new Dictionary<string, object>
            {
                { "alchemy_level", _playerData.AlchemyLevel },
                { "current_experience", _playerData.CurrentExperience },
                { "unlocked_recipe_ids", _playerData.UnlockedRecipeIds },
                { "total_crafted", _playerData.TotalCrafted },
                { "recipe_usage_count", _playerData.RecipeUsageCount }
            };
        }

        /// <summary>
        /// Load player's alchemy data.
        /// </summary>
        /// <param name="data">Dictionary containing alchemy data.</param>
        public void LoadData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.ContainsKey("alchemy_level"))
                _playerData.AlchemyLevel = Convert.ToInt32(data["alchemy_level"]);
            if (data.ContainsKey("current_experience"))
                _playerData.CurrentExperience = Convert.ToInt32(data["current_experience"]);
            if (data.ContainsKey("unlocked_recipe_ids"))
                _playerData.UnlockedRecipeIds = new List<int>((List<object>)data["unlocked_recipe_ids"]).ConvertAll(x => Convert.ToInt32(x));
            if (data.ContainsKey("total_crafted"))
                _playerData.TotalCrafted = Convert.ToInt32(data["total_crafted"]);
            if (data.ContainsKey("recipe_usage_count"))
            {
                var dict = (Dictionary<object, object>)data["recipe_usage_count"];
                _playerData.RecipeUsageCount = new Dictionary<int, int>();
                foreach (var kvp in dict)
                {
                    _playerData.RecipeUsageCount[Convert.ToInt32(kvp.Key)] = Convert.ToInt32(kvp.Value);
                }
            }

            GD.Print("[AlchemySystem] Data Loaded - Level: " + _playerData.AlchemyLevel);
        }

        /// <summary>
        /// Export save data for persistence
        /// </summary>
        public Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary<string, object>
            {
                { "alchemy_level", _playerData.AlchemyLevel },
                { "current_experience", _playerData.CurrentExperience },
                { "unlocked_recipe_ids", _playerData.UnlockedRecipeIds },
                { "total_crafted", _playerData.TotalCrafted },
                { "recipe_usage_count", _playerData.RecipeUsageCount }
            };
        }

        /// <summary>
        /// Import save data from persistence
        /// </summary>
        public void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.ContainsKey("alchemy_level"))
                _playerData.AlchemyLevel = Convert.ToInt32(data["alchemy_level"]);
            if (data.ContainsKey("current_experience"))
                _playerData.CurrentExperience = Convert.ToInt32(data["current_experience"]);
            if (data.ContainsKey("unlocked_recipe_ids"))
                _playerData.UnlockedRecipeIds = new List<int>((List<object>)data["unlocked_recipe_ids"]).ConvertAll(x => Convert.ToInt32(x));
            if (data.ContainsKey("total_crafted"))
                _playerData.TotalCrafted = Convert.ToInt32(data["total_crafted"]);
            if (data.ContainsKey("recipe_usage_count"))
            {
                var dict = (Dictionary<object, object>)data["recipe_usage_count"];
                _playerData.RecipeUsageCount = new Dictionary<int, int>();
                foreach (var kvp in dict)
                {
                    _playerData.RecipeUsageCount[Convert.ToInt32(kvp.Key)] = Convert.ToInt32(kvp.Value);
                }
            }
        }
    }
}
