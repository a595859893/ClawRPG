using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 炼金系统管理器 - 单例模式
    /// </summary>
    public class AlchemySystem
    {
        private static AlchemySystem _instance;
        public static AlchemySystem Instance => _instance ??= new AlchemySystem();

        private PlayerAlchemyData _playerData = new PlayerAlchemyData();
        private bool _isInitialized = false; 

        // 信号系统
        public static event Action<AlchemyRecipe, bool> OnCraftAttempt; // 制作结果
        public static event Action<int> OnLevelUp; // 升级
        public static event Action<AlchemyRecipe> OnRecipeUnlocked; // 解锁配方
        public static event Action<AlchemyMaterial, int> OnMaterialObtained; // 获得材料

        /// <summary>
        /// 获取玩家炼金数据
        /// </summary>
        public PlayerAlchemyData PlayerData => _playerData;

        /// <summary>
        /// 初始化炼金系统，解锁基础配方
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

        // 解锁配方
        /// <summary>
        /// 解锁指定的炼金配方
        /// </summary>
        /// <param name="recipeId">配方ID</param>
        /// <returns>是否成功解锁</returns>
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

        // 检查是否已解锁配方
        /// <summary>
        /// 检查指定配方是否已解锁
        /// </summary>
        /// <param name="recipeId">配方ID</param>
        /// <returns>是否已解锁</returns>
        public bool IsRecipeUnlocked(int recipeId)
        {
            return _playerData.UnlockedRecipeIds.Contains(recipeId);
        }

        // 获取已解锁的配方列表
        /// <summary>
        /// 获取玩家已解锁的所有炼金配方列表
        /// </summary>
        /// <returns>已解锁的配方列表</returns>
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

        // 尝试制作
        /// <summary>
        /// 尝试制作指定配方
        /// </summary>
        /// <param name="recipeId">配方ID</param>
        /// <param name="itemId">输出：制作产物的物品ID</param>
        /// <param name="quantity">输出：制作产物的数量</param>
        /// <param name="message">输出：结果消息</param>
        /// <returns>是否成功制作</returns>
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

        // 快速制作（自动选择最佳配方）
        /// <summary>
        /// 快速制作指定目标物品，自动选择可用配方
        /// </summary>
        /// <param name="targetItemId">目标物品ID</param>
        /// <param name="message">输出：结果消息</param>
        /// <returns>是否成功制作</returns>
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

        // 检查是否能制作指定配方
        /// <summary>
        /// 检查指定配方是否可以制作（材料是否足够）
        /// </summary>
        /// <param name="recipeId">配方ID</param>
        /// <returns>是否能够制作</returns>
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

        // 添加经验
        /// <summary>
        /// 添加炼金经验值，处理升级逻辑
        /// </summary>
        /// <param name="amount">经验值数量</param>
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

        // 获得材料（采集/掉落）
        /// <summary>
        /// 获得炼金材料，添加到玩家背包
        /// </summary>
        /// <param name="materialId">材料ID</param>
        /// <param name="quantity">数量，默认为1</param>
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

        // 随机获得材料（基于稀有度权重）
        /// <summary>
        /// 随机获得指定稀有度的炼金材料
        /// </summary>
        /// <param name="rarity">目标稀有度</param>
        public void ObtainRandomMaterial(AlchemyMaterialRarity rarity)
        {
            var material = AlchemyDatabase.Instance.GetRandomMaterialByRarity(rarity);
            if (material != null)
            {
                ObtainMaterial(material.Id, 1);
            }
        }

        // 获取制作所需的材料数量
        /// <summary>
        /// 获取指定配方所需的材料数量
        /// </summary>
        /// <param name="recipeId">配方ID</param>
        /// <returns>材料ID到数量的字典</returns>
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

        // 检查材料是否足够
        /// <summary>
        /// 获取指定配方缺少的材料数量
        /// </summary>
        /// <param name="recipeId">配方ID</param>
        /// <returns>缺少的材料ID到数量的字典</returns>
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

        // 保存数据
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

        // 加载数据
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
    }
}
