using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 炼金配方数据库 - 数据驱动设计
    /// </summary>
    public class AlchemyDatabase
    {
        private static AlchemyDatabase _instance;
        public static AlchemyDatabase Instance => _instance ??= new AlchemyDatabase();

        private Dictionary<int, AlchemyMaterial> _materials = new Dictionary<int, AlchemyMaterial>();
        private Dictionary<int, AlchemyRecipe> _recipes = new Dictionary<int, AlchemyRecipe>();
        private int _nextMaterialId = 1001;
        private int _nextRecipeId = 2001;

        public AlchemyDatabase()
        {
            InitializeMaterials();
            InitializeRecipes();
        }

        private void InitializeMaterials()
        {
            // 草药类 (1001-1015)
            AddMaterial(new AlchemyMaterial
            {
                Id = 1001,
                Name = "红叶草",
                Description = "常见的红色草药",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Common,
                Value = 5
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1002,
                Name = "蓝莲花",
                Description = "具有魔法能量的莲花",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 15
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1003,
                Name = "银叶草",
                Description = "月光照耀的银色草药",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 30
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1004,
                Name = "火焰花",
                Description = "生长在火山附近的炽热花朵",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 45
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1005,
                Name = "冰晶草",
                Description = "极寒之地生长的晶体草",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 45
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1006,
                Name = "月光草",
                Description = "只在满月时绽放的奇迹草药",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 100
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1007,
                Name = "龙心草",
                Description = "传说中巨龙栖息地的神圣草药",
                Type = AlchemyMaterialType.Herb,
                Rarity = AlchemyMaterialRarity.Legendary,
                Value = 500
            });

            // 矿物类 (1011-1018)
            AddMaterial(new AlchemyMaterial
            {
                Id = 1011,
                Name = "铜矿石",
                Description = "常见的金属矿石",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Common,
                Value = 3
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1012,
                Name = "银矿石",
                Description = "稀有的银色金属",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 20
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1013,
                Name = "金矿石",
                Description = "珍贵的金色金属",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 50
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1014,
                Name = "秘银",
                Description = "传说中的魔法金属",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 200
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1015,
                Name = "龙晶",
                Description = "蕴含巨龙力量的晶体",
                Type = AlchemyMaterialType.Mineral,
                Rarity = AlchemyMaterialRarity.Legendary,
                Value = 800
            });

            // 怪物素材类 (1021-1030)
            AddMaterial(new AlchemyMaterial
            {
                Id = 1021,
                Name = "史莱姆凝胶",
                Description = "史莱姆的凝胶状物质",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Common,
                Value = 8
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1022,
                Name = "哥布林耳朵",
                Description = "哥布林的战利品",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Common,
                Value = 10
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1023,
                Name = "狼牙",
                Description = "野狼的锋利獠牙",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 25
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1024,
                Name = "骷髅碎片",
                Description = "亡灵生物的遗骸",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 20
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1025,
                Name = "龙鳞",
                Description = "巨龙的鳞片",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 300
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1026,
                Name = "凤凰羽毛",
                Description = "浴火凤凰的羽毛",
                Type = AlchemyMaterialType.MonsterPart,
                Rarity = AlchemyMaterialRarity.Legendary,
                Value = 1000
            });

            // 水晶类 (1031-1040)
            AddMaterial(new AlchemyMaterial
            {
                Id = 1031,
                Name = "红水晶",
                Description = "蕴含火焰能量的水晶",
                Type = AlchemyMaterialType.Crystal,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 30
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1032,
                Name = "蓝水晶",
                Description = "蕴含冰霜能量的水晶",
                Type = AlchemyMaterialType.Crystal,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 30
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1033,
                Name = "绿水晶",
                Description = "蕴含自然能量的水晶",
                Type = AlchemyMaterialType.Crystal,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 30
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1034,
                Name = "紫水晶",
                Description = "蕴含暗影能量的水晶",
                Type = AlchemyMaterialType.Crystal,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 60
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1035,
                Name = "彩虹水晶",
                Description = "蕴含多元能量的稀有水晶",
                Type = AlchemyMaterialType.Epic,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 250
            });

            // 鱼类 (1041-1050) - 钓鱼系统联动
            AddMaterial(new AlchemyMaterial
            {
                Id = 1041,
                Name = "淡水鱼",
                Description = "常见的淡水鱼",
                Type = AlchemyMaterialType.Fish,
                Rarity = AlchemyMaterialRarity.Common,
                Value = 8
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1042,
                Name = "银鱼",
                Description = "鳞片闪着银光的鱼",
                Type = AlchemyMaterialType.Fish,
                Rarity = AlchemyMaterialRarity.Uncommon,
                Value = 20
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1043,
                Name = "金鱼",
                Description = "珍贵的金色鲤鱼",
                Type = AlchemyMaterialType.Fish,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 50
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1044,
                Name = "龙鳞鱼",
                Description = "传说中具有龙之血脉的鱼",
                Type = AlchemyMaterialType.Fish,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 150
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1045,
                Name = "海神鱼",
                Description = "深海传说中的神鱼",
                Type = AlchemyMaterialType.Fish,
                Rarity = AlchemyMaterialRarity.Legendary,
                Value = 600
            });

            // 特殊材料 (1051-1060)
            AddMaterial(new AlchemyMaterial
            {
                Id = 1051,
                Name = "星尘",
                Description = "夜空中的闪烁粉尘",
                Type = AlchemyMaterialType.Special,
                Rarity = AlchemyMaterialRarity.Rare,
                Value = 80
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1052,
                Name = "精灵之泪",
                Description = "森林精灵流下的泪滴",
                Type = AlchemyMaterialType.Special,
                Rarity = AlchemyMaterialRarity.Epic,
                Value = 200
            });

            AddMaterial(new AlchemyMaterial
            {
                Id = 1053,
                Name = "世界树枝叶",
                Description = "世界树的神圣枝叶",
                Type = AlchemyMaterialType.Special,
                Rarity = AlchemyMaterialRarity.Legendary,
                Value = 800
            });
        }

        private void InitializeRecipes()
        {
            // 基础生命药水配方
            AddRecipe(new AlchemyRecipe
            {
                Id = 2001,
                Name = "小生命药水",
                Description = "恢复少量生命值",
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

            // 中生命药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2002,
                Name = "中生命药水",
                Description = "恢复中等生命值",
                ResultItemId = 502,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1002, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1001, Quantity = 1 }
                },
                GoldCost = 25,
                RequiredAlchemyLevel = 2,
                SuccessRate = 0.95f
            });

            // 大生命药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2003,
                Name = "大生命药水",
                Description = "恢复大量生命值",
                ResultItemId = 503,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1003, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1002, Quantity = 2 }
                },
                GoldCost = 50,
                RequiredAlchemyLevel = 4,
                SuccessRate = 0.90f
            });

            // 超级生命药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2004,
                Name = "超级生命药水",
                Description = "恢复大量生命值并持续恢复",
                ResultItemId = 504,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1006, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1003, Quantity = 3 },
                    new AlchemyRecipeRequirement { MaterialId = 1051, Quantity = 2 }
                },
                GoldCost = 150,
                RequiredAlchemyLevel = 6,
                SuccessRate = 0.85f
            });

            // 传说生命药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2005,
                Name = "传说生命药水",
                Description = "恢复全部生命值",
                ResultItemId = 505,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1007, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1053, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1026, Quantity = 1 }
                },
                GoldCost = 500,
                RequiredAlchemyLevel = 10,
                SuccessRate = 0.70f
            });

            // 小法力药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2011,
                Name = "小法力药水",
                Description = "恢复少量法力值",
                ResultItemId = 511,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1032, Quantity = 2 }
                },
                GoldCost = 10,
                RequiredAlchemyLevel = 1,
                SuccessRate = 1.0f
            });

            // 中法力药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2012,
                Name = "中法力药水",
                Description = "恢复中等法力值",
                ResultItemId = 512,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1032, Quantity = 3 },
                    new AlchemyRecipeRequirement { MaterialId = 1031, Quantity = 1 }
                },
                GoldCost = 25,
                RequiredAlchemyLevel = 3,
                SuccessRate = 0.95f
            });

            // 大法力药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2013,
                Name = "大法力药水",
                Description = "恢复大量法力值",
                ResultItemId = 513,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1034, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1032, Quantity = 3 }
                },
                GoldCost = 50,
                RequiredAlchemyLevel = 5,
                SuccessRate = 0.90f
            });

            // 超级法力药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2014,
                Name = "超级法力药水",
                Description = "恢复大量法力值并持续恢复",
                ResultItemId = 514,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1035, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1034, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1052, Quantity = 1 }
                },
                GoldCost = 150,
                RequiredAlchemyLevel = 7,
                SuccessRate = 0.85f
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
                    new AlchemyRecipeRequirement { MaterialId = 1023, Quantity = 1 }
                },
                GoldCost = 30,
                RequiredAlchemyLevel = 2,
                SuccessRate = 0.95f
            });

            // 强效力量药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2032,
                Name = "强效力量药水",
                Description = "大幅增加攻击力",
                ResultItemId = 532,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1025, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1004, Quantity = 3 },
                    new AlchemyRecipeRequirement { MaterialId = 1031, Quantity = 2 }
                },
                GoldCost = 80,
                RequiredAlchemyLevel = 5,
                SuccessRate = 0.85f
            });

            // 传说力量药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2033,
                Name = "传说力量药水",
                Description = "大幅增加攻击力并提高暴击率",
                ResultItemId = 533,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1015, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1025, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1053, Quantity = 1 }
                },
                GoldCost = 400,
                RequiredAlchemyLevel = 10,
                SuccessRate = 0.65f
            });

            // 防御药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2041,
                Name = "防御药水",
                Description = "增加防御力",
                ResultItemId = 541,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1005, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1012, Quantity = 1 }
                },
                GoldCost = 30,
                RequiredAlchemyLevel = 2,
                SuccessRate = 0.95f
            });

            // 强效防御药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2042,
                Name = "强效防御药水",
                Description = "大幅增加防御力",
                ResultItemId = 542,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1014, Quantity = 1 },
                    new AlchemyRecipeRequirement { MaterialId = 1005, Quantity = 3 },
                    new AlchemyRecipeRequirement { MaterialId = 1033, Quantity = 2 }
                },
                GoldCost = 80,
                RequiredAlchemyLevel = 5,
                SuccessRate = 0.85f
            });

            // 速度药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2051,
                Name = "速度药水",
                Description = "增加移动速度",
                ResultItemId = 551,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1033, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1021, Quantity = 2 }
                },
                GoldCost = 25,
                RequiredAlchemyLevel = 1,
                SuccessRate = 0.95f
            });

            // 超级速度药水
            AddRecipe(new AlchemyRecipe
            {
                Id = 2052,
                Name = "超级速度药水",
                Description = "大幅增加移动速度",
                ResultItemId = 552,
                ResultQuantity = 1,
                Requirements = new List<AlchemyRecipeRequirement>
                {
                    new AlchemyRecipeRequirement { MaterialId = 1051, Quantity = 2 },
                    new AlchemyRecipeRequirement { MaterialId = 1033, Quantity = 3 }
                },
                GoldCost = 60,
                RequiredAlchemyLevel = 4,
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

        private void AddMaterial(AlchemyMaterial material)
        {
            _materials[material.Id] = material;
        }

        private void AddRecipe(AlchemyRecipe recipe)
        {
            _recipes[recipe.Id] = recipe;
        }

        // 材料获取
        public AlchemyMaterial GetMaterial(int id)
        {
            return _materials.ContainsKey(id) ? _materials[id] : null;
        }

        public List<AlchemyMaterial> GetAllMaterials()
        {
            return new List<AlchemyMaterial>(_materials.Values);
        }

        public List<AlchemyMaterial> GetMaterialsByType(AlchemyMaterialType type)
        {
            List<AlchemyMaterial> result = new List<AlchemyMaterial>();
            foreach (var material in _materials.Values)
            {
                if (material.Type == type)
                    result.Add(material);
            }
            return result;
        }

        public List<AlchemyMaterial> GetMaterialsByRarity(AlchemyMaterialRarity rarity)
        {
            List<AlchemyMaterial> result = new List<AlchemyMaterial>();
            foreach (var material in _materials.Values)
            {
                if (material.Rarity == rarity)
                    result.Add(material);
            }
            return result;
        }

        // 配方获取
        public AlchemyRecipe GetRecipe(int id)
        {
            return _recipes.ContainsKey(id) ? _recipes[id] : null;
        }

        public List<AlchemyRecipe> GetAllRecipes()
        {
            return new List<AlchemyRecipe>(_recipes.Values);
        }

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

        // 生成随机材料掉落（用于采集/战斗掉落）
        public AlchemyMaterial GetRandomMaterialByRarity(AlchemyMaterialRarity rarity)
        {
            var materials = GetMaterialsByRarity(rarity);
            if (materials.Count == 0) return null;
            
            var random = new Random();
            return materials[random.Next(materials.Count)];
        }

        public AlchemyMaterial GetRandomMaterial()
        {
            var random = new Random();
            var materialList = new List<AlchemyMaterial>(_materials.Values);
            return materialList[random.Next(materialList.Count)];
        }
    }
}
