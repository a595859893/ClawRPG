using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Alchemy
{
    /// <summary>
    /// 炼金配方存储与合成逻辑 - 管理配方数据和合成过程
    /// 整合 AlchemyRecipeDB 和 AlchemyCrafting 的功能
    /// </summary>
    public partial class AlchemyRecipeStore : BaseSystem
    {
        private static AlchemyRecipeStore _instance;
        public static AlchemyRecipeStore Instance => _instance;
        
        // 配方存储
        private Dictionary<int, AlchemyRecipe> _recipes = new Dictionary<int, AlchemyRecipe>();
        private int _nextRecipeId = 2001;
        
        // Signals
        [Signal] public delegate void CraftSuccessEventHandler(int recipeId, int resultItemId, int quantity);
        [Signal] public delegate void CraftFailedEventHandler(int recipeId, string reason);
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            InitializeRecipes();
        }
        
        protected override string SystemName => "AlchemyRecipeStore";
        
        #region Recipe Initialization
        
        /// <summary>
        /// 初始化配方数据
        /// </summary>
        private void InitializeRecipes()
        {
            // 体力药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2001,
                Name = "体力药水",
                Description = "恢复少量体力",
                ResultItemId = 501,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1001, Quantity = 2 }
                },
                GoldCost = 10,
                RequiredAlchemyLevel = 1,
                SuccessRate = 1.0f
            });

            // 法力药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2002,
                Name = "法力药水",
                Description = "恢复少量法力",
                ResultItemId = 511,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1002, Quantity = 2 }
                },
                GoldCost = 10,
                RequiredAlchemyLevel = 1,
                SuccessRate = 1.0f
            });

            // 力量药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2031,
                Name = "力量药水",
                Description = "增加攻击力",
                ResultItemId = 531,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1004, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1011, Quantity = 1 }
                },
                GoldCost = 30,
                RequiredAlchemyLevel = 2,
                SuccessRate = 0.95f
            });

            // 敏捷药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2041,
                Name = "敏捷药水",
                Description = "增加攻击速度",
                ResultItemId = 541,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1003, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1023, Quantity = 1 }
                },
                GoldCost = 30,
                RequiredAlchemyLevel = 2,
                SuccessRate = 0.95f
            });

            // 防御药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2051,
                Name = "防御药水",
                Description = "增加防御力",
                ResultItemId = 551,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1005, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1011, Quantity = 2 }
                },
                GoldCost = 40,
                RequiredAlchemyLevel = 3,
                SuccessRate = 0.90f
            });

            // 速度药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2055,
                Name = "速度药水",
                Description = "增加移动速度",
                ResultItemId = 555,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1031, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1042, Quantity = 1 }
                },
                GoldCost = 50,
                RequiredAlchemyLevel = 3,
                SuccessRate = 0.90f
            });

            // 暴击药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2061,
                Name = "暴击药水",
                Description = "增加暴击率",
                ResultItemId = 561,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1013, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1023, Quantity = 2 }
                },
                GoldCost = 80,
                RequiredAlchemyLevel = 4,
                SuccessRate = 0.90f
            });

            // 生命再生药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2071,
                Name = "生命再生药水",
                Description = "持续恢复生命值",
                ResultItemId = 571,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1006, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1033, Quantity = 1 }
                },
                GoldCost = 40,
                RequiredAlchemyLevel = 3,
                SuccessRate = 0.90f
            });

            // 法力再生药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2072,
                Name = "法力再生药水",
                Description = "持续恢复法力值",
                ResultItemId = 572,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1034, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1032, Quantity = 2 }
                },
                GoldCost = 40,
                RequiredAlchemyLevel = 3,
                SuccessRate = 0.90f
            });

            // 解毒药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2081,
                Name = "解毒药水",
                Description = "清除所有负面状态效果",
                ResultItemId = 581,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1002, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1024, Quantity = 1 }
                },
                GoldCost = 50,
                RequiredAlchemyLevel = 2,
                SuccessRate = 0.95f
            });

            // 隐形药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2091,
                Name = "隐形药水",
                Description = "使敌人无法发现你",
                ResultItemId = 591,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1051, Quantity = 3 },
                    new AlchemyRecipeRequirement { MaterialId = 1034, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1052, Quantity = 1 }
                },
                GoldCost = 150,
                RequiredAlchemyLevel = 8,
                SuccessRate = 0.75f
            });

            // 传说体力药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2101,
                Name = "传说体力药水",
                Description = "恢复全部体力值",
                ResultItemId = 522,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1007, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1045, Quantity = 1 }
                },
                GoldCost = 200,
                RequiredAlchemyLevel = 6,
                SuccessRate = 0.80f
            });
            
            GD.Print($"[AlchemyRecipeStore] Initialized {_recipes.Count} recipes");
        }
        
        #endregion
        
        #region Recipe Management
        
        /// <summary>
        /// 添加配方
        /// </summary>
        public void AddRecipe(AlchemyRecipe recipe)
        {
            if (recipe.Id == 0)
            {
                recipe.Id = _nextRecipeId++;
            }
            _recipes[recipe.Id] = recipe;
        }
        
        /// <summary>
        /// 获取配方
        /// </summary>
        public AlchemyRecipe GetRecipe(int id)
        {
            return _recipes.ContainsKey(id) ? _recipes[id] : null;
        }
        
        /// <summary>
        /// 获取所有配方
        /// </summary>
        public List<AlchemyRecipe> GetAllRecipes()
        {
            return new List<AlchemyRecipe>(_recipes.Values);
        }
        
        /// <summary>
        /// 根据玩家等级获取可用配方
        /// </summary>
        public List<AlchemyRecipe> GetRecipesByLevel(int playerLevel)
        {
            List<AlchemyRecipe> result = new List<AlchemyRecipe>();
            foreach (var recipe in _recipes.Values)
            {
                if (recipe.RequiredAlchemyLevel <= playerLevel)
                    result.Add(recipe);
            }
            return result;
        }
        
        /// <summary>
        /// 根据ID列表获取配方
        /// </summary>
        public List<AlchemyRecipe> GetRecipesByIds(List<int> recipeIds)
        {
            List<AlchemyRecipe> result = new List<AlchemyRecipe>();
            foreach (var id in recipeIds)
            {
                if (_recipes.ContainsKey(id))
                {
                    result.Add(_recipes[id]);
                }
            }
            return result;
        }
        
        #endregion
        
        #region Crafting Logic
        
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
        
        /// <summary>
        /// 尝试合成
        /// </summary>
        public CraftResult TryCraft(int recipeId, Dictionary<int, int> availableMaterials, int playerLevel, int gold)
        {
            var recipe = GetRecipe(recipeId);
            if (recipe == null)
            {
                EmitSignal(SignalName.CraftFailed, recipeId, "未知的配方");
                return new CraftResult
                {
                    Success = false,
                    Message = "未知的配方"
                };
            }
            
            // 检查玩家等级
            if (playerLevel < recipe.RequiredAlchemyLevel)
            {
                var msg = $"需要炼金等级 {recipe.RequiredAlchemyLevel}";
                EmitSignal(SignalName.CraftFailed, recipeId, msg);
                return new CraftResult
                {
                    Success = false,
                    Message = msg
                };
            }
            
            // 检查金币
            if (gold < recipe.GoldCost)
            {
                var msg = $"需要 {recipe.GoldCost} 金币";
                EmitSignal(SignalName.CraftFailed, recipeId, msg);
                return new CraftResult
                {
                    Success = false,
                    Message = msg
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
                    var msg = $"材料不足: {material} x{requirement.Quantity}";
                    EmitSignal(SignalName.CraftFailed, recipeId, msg);
                    return new CraftResult
                    {
                        Success = false,
                        Message = msg
                    };
                }
            }
            
            // 计算成功率
            bool success = CheckSuccess(recipe.SuccessRate);
            
            if (success)
            {
                EmitSignal(SignalName.CraftSuccess, recipeId, recipe.ResultItemId, recipe.ResultQuantity);
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
                EmitSignal(SignalName.CraftFailed, recipeId, "合成失败! 返还部分材料。");
                return new CraftResult
                {
                    Success = false,
                    Message = "合成失败! 返还部分材料。"
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
            int baseExp = recipe.RequiredAlchemyLevel * 10;
            return baseExp;
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
        /// 检查是否可以合成
        /// </summary>
        public bool CanCraft(int recipeId, Dictionary<int, int> availableMaterials, int playerLevel, int gold)
        {
            var recipe = GetRecipe(recipeId);
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
        /// 计算合成所需金币
        /// </summary>
        public int CalculateGoldCost(int recipeId)
        {
            var recipe = GetRecipe(recipeId);
            return recipe != null ? recipe.GoldCost : 0;
        }
        
        /// <summary>
        /// 获取合成所需材料列表
        /// </summary>
        public List<AlchemyRecipeRequirement> GetRequirements(int recipeId)
        {
            var recipe = GetRecipe(recipeId);
            return recipe != null ? recipe.Requirements : new List<AlchemyRecipeRequirement>();
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // 加载数据
        }
        
        #endregion
    }
}
