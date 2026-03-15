using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Items;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 炼金系统管理器，负责配方的解锁、制作和玩家炼金数据的管理。
    /// 采用单例模式确保全局唯一实例。
    /// </summary>
    /// <remarks>
    /// 主要功能：
    /// - 配方解锁与管理
    /// - 材料消耗与产物制作
    /// - 炼金经验与等级系统
    /// - 数据持久化
    /// </remarks>
    public class AlchemySystem
    {
        private static AlchemySystem _instance;
        
        /// <summary>
        /// 获取 AlchemySystem 的单例实例。
        /// </summary>
        public static AlchemySystem Instance => _instance ??= new AlchemySystem();

        private PlayerAlchemyData _playerData = new PlayerAlchemyData();
        private bool _isInitialized = false; 

        // 信号系统
        
        /// <summary>
        /// 制作尝试时触发。参数：配方对象，制作是否成功。
        /// </summary>
        public static event Action<AlchemyRecipe, bool> OnCraftAttempt; 
        
        /// <summary>
        /// 炼金等级提升时触发。参数：新的等级。
        /// </summary>
        public static event Action<int> OnLevelUp; 
        
        /// <summary>
        /// 配方解锁时触发。参数：解锁的配方对象。
        /// </summary>
        public static event Action<AlchemyRecipe> OnRecipeUnlocked; 
        
        /// <summary>
        /// 获得材料时触发。参数：材料对象，获得的数量。
        /// </summary>
        public static event Action<AlchemyMaterial, int> OnMaterialObtained; 

        /// <summary>
        /// 获取玩家的炼金数据。
        /// </summary>
        public PlayerAlchemyData PlayerData => _playerData;

        /// <summary>
        /// 初始化炼金系统，解锁基础配方。
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
        /// 解锁指定的炼金配方。
        /// </summary>
        /// <param name="recipeId">要解锁的配方ID。</param>
        /// <returns>解锁成功返回 true，如果配方已存在或不存在返回 false。</returns>
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
        /// 检查指定配方是否已解锁。
        /// </summary>
        /// <param name="recipeId">配方ID。</param>
        /// <returns>已解锁返回 true，否则返回 false。</returns>
        public bool IsRecipeUnlocked(int recipeId)
        {
            return _playerData.UnlockedRecipeIds.Contains(recipeId);
        }

        /// <summary>
        /// 获取所有已解锁的炼金配方列表。
        /// </summary>
        /// <returns>已解锁配方的列表。</returns>
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
        /// 尝试按照指定配方进行制作。
        /// </summary>
        /// <param name="recipeId">配方ID。</param>
        /// <param name="itemId">输出参数，成功时产物的ID。</param>
        /// <param name="quantity">输出参数，成功时产物的数量。</param>
        /// <param name="message">输出参数，操作结果的消息。</param>
        /// <returns>制作成功返回 true，否则返回 false。</returns>
        /// <remarks>
        /// 检查流程：配方是否存在 -> 配方是否解锁 -> 等级是否足够 -> 金币是否足够 -> 材料是否足够
        /// 制作成功后扣除金币和材料，根据成功率为判定结果。
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
        /// 快速制作，自动选择最佳可用配方。
        /// </summary>
        /// <param name="targetItemId">目标产物ID。</param>
        /// <param name="message">输出参数，操作结果的消息。</param>
        /// <returns>制作成功返回 true，否则返回 false。</returns>
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
        /// 检查指定配方是否可以制作。
        /// </summary>
        /// <param name="recipeId">配方ID。</param>
        /// <returns>可以制作返回 true，否则返回 false。</returns>
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
        /// 添加炼金经验值，可触发升级。
        /// </summary>
        /// <param name="amount">要添加的经验值数量。</param>
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
        /// 获得指定的炼金材料，添加到玩家背包。
        /// </summary>
        /// <param name="materialId">材料ID。</param>
        /// <param name="quantity">获得的数量，默认为1。</param>
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
        /// 随机获得指定稀有度的炼金材料。
        /// </summary>
        /// <param name="rarity">目标稀有度。</param>
        public void ObtainRandomMaterial(AlchemyMaterialRarity rarity)
        {
            var material = AlchemyDatabase.Instance.GetRandomMaterialByRarity(rarity);
            if (material != null)
            {
                ObtainMaterial(material.Id, 1);
            }
        }

        /// <summary>
        /// 获取指定配方所需的材料数量。
        /// </summary>
        /// <param name="recipeId">配方ID。</param>
        /// <returns>材料ID到需求数量的字典。</returns>
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
        /// 获取指定配方的缺失材料。
        /// </summary>
        /// <param name="recipeId">配方ID。</param>
        /// <returns>材料ID到缺失数量的字典。</returns>
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
        /// 保存玩家的炼金数据。
        /// </summary>
        /// <returns>包含炼金数据的字典，用于持久化存储。</returns>
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
        /// 加载玩家的炼金数据。
        /// </summary>
        /// <param name="data">包含炼金数据的字典。</param>
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
