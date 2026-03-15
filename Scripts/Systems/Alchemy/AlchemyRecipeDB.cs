using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems.Alchemy
{
    /// <summary>
    /// 炼金配方数据库 - 管理所有配方数据
    /// </summary>
    public partial class AlchemyRecipeDB : BaseSystem
    {
        private Dictionary<int, AlchemyRecipe> _recipes = new Dictionary<int, AlchemyRecipe>();
        private int _nextRecipeId = 2001;
        
        public override void _Ready()
        {
            base._Ready();
            InitializeRecipes();
        }
        
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
        }
        
        /// <summary>
        /// 添加配方
        /// </summary>
        private void AddRecipe(AlchemyRecipe recipe)
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
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            // 加载数据
        }
    }
}
