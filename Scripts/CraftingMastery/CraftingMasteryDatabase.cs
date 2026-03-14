using System;
using System.Collections.Generic;
using ClawRPG.Systems.CraftingMastery;

namespace ClawRPG.Systems.CraftingMastery
{
    /// <summary>
    /// Database for crafting mastery configuration
    /// </summary>
    public class CraftingMasteryDatabase
    {
        private static CraftingMasteryDatabase _instance;
        public static CraftingMasteryDatabase Instance => _instance ?? (_instance = new CraftingMasteryDatabase());
        
        // Experience thresholds for each mastery level
        private readonly Dictionary<int, int> _levelThresholds = new Dictionary<int, int>
        {
            { 1, 0 },      // Novice
            { 2, 100 },    // Apprentice
            { 3, 300 },    // Journeyman
            { 4, 600 },    // Expert
            { 5, 1000 },   // Master
            { 6, 1500 }   // GrandMaster
        };
        
        // Category mastery configurations
        private readonly Dictionary<CraftingCategory, CategoryConfig> _categoryConfigs;
        
        // Recipe database
        private readonly Dictionary<string, CraftingRecipe> _recipes;
        
        // Mastery bonuses per level
        private readonly Dictionary<int, MasteryBonus> _masteryBonuses;
        
        public CraftingMasteryDatabase()
        {
            _categoryConfigs = InitializeCategoryConfigs();
            _recipes = InitializeRecipes();
            _masteryBonuses = InitializeMasteryBonuses();
        }
        
        private Dictionary<CraftingCategory, CategoryConfig> InitializeCategoryConfigs()
        {
            return new Dictionary<CraftingCategory, CategoryConfig>
            {
                { CraftingCategory.Blacksmithing, new CategoryConfig
                    {
                        Category = CraftingCategory.Blacksmithing,
                        DisplayName = "锻造",
                        Description = "武器和护甲制作",
                        Icon = "⚔️",
                        BaseSuccessRate = 0.85f,
                        LevelMultiplier = 0.02f
                    }
                },
                { CraftingCategory.Alchemy, new CategoryConfig
                    {
                        Category = CraftingCategory.Alchemy,
                        DisplayName = "炼金",
                        Description = "药水和解毒剂制作",
                        Icon = "🧪",
                        BaseSuccessRate = 0.80f,
                        LevelMultiplier = 0.02f
                    }
                },
                { CraftingCategory.Enchanting, new CategoryConfig
                    {
                        Category = CraftingCategory.Enchanting,
                        DisplayName = "附魔",
                        Description = "装备附魔制作",
                        Icon = "✨",
                        BaseSuccessRate = 0.75f,
                        LevelMultiplier = 0.025f
                    }
                },
                { CraftingCategory.Cooking, new CategoryConfig
                    {
                        Category = CraftingCategory.Cooking,
                        DisplayName = "烹饪",
                        Description = "食物和饮料制作",
                        Icon = "🍖",
                        BaseSuccessRate = 0.90f,
                        LevelMultiplier = 0.015f
                    }
                },
                { CraftingCategory.Leatherworking, new CategoryConfig
                    {
                        Category = CraftingCategory.Leatherworking,
                        DisplayName = "制皮",
                        Description = "皮甲和皮革物品制作",
                        Icon = "🛡️",
                        BaseSuccessRate = 0.82f,
                        LevelMultiplier = 0.02f
                    }
                },
                { CraftingCategory.Tailoring, new CategoryConfig
                    {
                        Category = CraftingCategory.Tailoring,
                        DisplayName = "裁缝",
                        Description = "布甲和布料物品制作",
                        Icon = "🧵",
                        BaseSuccessRate = 0.83f,
                        LevelMultiplier = 0.02f
                    }
                },
                { CraftingCategory.Jewelcrafting, new CategoryConfig
                    {
                        Category = CraftingCategory.Jewelcrafting,
                        DisplayName = "珠宝",
                        Description = "珠宝和宝石镶嵌制作",
                        Icon = "💎",
                        BaseSuccessRate = 0.70f,
                        LevelMultiplier = 0.03f
                    }
                },
                { CraftingCategory.Inscription, new CategoryConfig
                    {
                        Category = CraftingCategory.Inscription,
                        DisplayName = "铭文",
                        Description = "卷轴和符文制作",
                        Icon = "📜",
                        BaseSuccessRate = 0.78f,
                        LevelMultiplier = 0.025f
                    }
                }
            };
        }
        
        private Dictionary<string, CraftingRecipe> InitializeRecipes()
        {
            var recipes = new Dictionary<string, CraftingRecipe>();
            int recipeId = 1;
            
            // Blacksmithing Recipes
            AddRecipe(recipes, ref recipeId, "BS_SWORD_01", "铁剑", CraftingCategory.Blacksmithing, RecipeDifficulty.Simple, 1, 1,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "IRON_INGOT", ItemName = "铁锭", Quantity = 3 } }, 10, 5, 0.95f);
            AddRecipe(recipes, ref recipeId, "BS_SWORD_02", "钢剑", CraftingCategory.Blacksmithing, RecipeDifficulty.Common, 5, 2,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "STEEL_INGOT", ItemName = "钢锭", Quantity = 5 }, new RecipeComponent { ItemId = "LEATHER", ItemName = "皮革", Quantity = 1 } }, 25, 12, 0.90f);
            AddRecipe(recipes, ref recipeId, "BS_SWORD_03", "秘银剑", CraftingCategory.Blacksmithing, RecipeDifficulty.Uncommon, 10, 3,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "MITHRIL_INGOT", ItemName = "秘银锭", Quantity = 8 }, new RecipeComponent { ItemId = "GEM_RUBY", ItemName = "红宝石", Quantity = 1 } }, 50, 25, 0.85f);
            AddRecipe(recipes, ref recipeId, "BS_SWORD_04", "龙鳞剑", CraftingCategory.Blacksmithing, RecipeDifficulty.Rare, 20, 4,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "DRAGON_SCALE", ItemName = "龙鳞", Quantity = 5 }, new RecipeComponent { ItemId = "OBSIDIAN", ItemName = "黑曜石", Quantity = 3 } }, 100, 50, 0.75f);
            AddRecipe(recipes, ref recipeId, "BS_SWORD_05", "传奇泰坦之剑", CraftingCategory.Blacksmithing, RecipeDifficulty.Legendary, 30, 6,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "TITANIUM_INGOT", ItemName = "钛金锭", Quantity = 20 }, new RecipeComponent { ItemId = "DRAGON_HEART", ItemName = "龙心", Quantity = 1 }, new RecipeComponent { ItemId = "STAR_METAL", ItemName = "星金", Quantity = 10 } }, 300, 150, 0.50f);
            
            // Armor Recipes
            AddRecipe(recipes, ref recipeId, "BS_HELM_01", "铁头盔", CraftingCategory.Blacksmithing, RecipeDifficulty.Simple, 1, 1,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "IRON_INGOT", ItemName = "铁锭", Quantity = 4 } }, 12, 6, 0.95f);
            AddRecipe(recipes, ref recipeId, "BS_CHEST_01", "钢胸甲", CraftingCategory.Blacksmithing, RecipeDifficulty.Common, 5, 2,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "STEEL_INGOT", ItemName = "钢锭", Quantity = 8 }, new RecipeComponent { ItemId = "LEATHER", ItemName = "皮革", Quantity = 2 } }, 30, 15, 0.88f);
            
            // Alchemy Recipes
            AddRecipe(recipes, ref recipeId, "ALCH_POT_HP_01", "初级生命药水", CraftingCategory.Alchemy, RecipeDifficulty.Simple, 1, 1,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "HERB_HEAL", ItemName = "治疗草", Quantity = 2 }, new RecipeComponent { ItemId = "WATER", ItemName = "水", Quantity = 1 } }, 8, 4, 0.92f);
            AddRecipe(recipes, ref recipeId, "ALCH_POT_HP_02", "中级生命药水", CraftingCategory.Alchemy, RecipeDifficulty.Common, 5, 2,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "HERB_HEAL", ItemName = "治疗草", Quantity = 5 }, new RecipeComponent { ItemId = "CRYSTAL_MANA", ItemName = "魔法水晶", Quantity = 1 } }, 20, 10, 0.85f);
            AddRecipe(recipes, ref recipeId, "ALCH_POT_HP_03", "高级生命药水", CraftingCategory.Alchemy, RecipeDifficulty.Uncommon, 15, 3,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "HERB_FLAME", ItemName = "烈焰草", Quantity = 5 }, new RecipeComponent { ItemId = "GEM_EMERALD", ItemName = "祖母绿", Quantity = 1 } }, 45, 22, 0.78f);
            AddRecipe(recipes, ref recipeId, "ALCH_POT_SPEED_01", "速度药水", CraftingCategory.Alchemy, RecipeDifficulty.Common, 5, 2,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "HERB_SWIFT", ItemName = "风速草", Quantity = 4 }, new RecipeComponent { ItemId = "WIND_ESSENCE", ItemName = "风之精华", Quantity = 1 } }, 22, 11, 0.82f);
            AddRecipe(recipes, ref recipeId, "ALCH_ELIXIR_MASTER", "大师药剂", CraftingCategory.Alchemy, RecipeDifficulty.Legendary, 30, 6,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "HERB_RARE", ItemName = "稀有药草", Quantity = 10 }, new RecipeComponent { ItemId = "DRAGON_BLOOD", ItemName = "龙血", Quantity = 2 }, new RecipeComponent { ItemId = "MOON_DUST", ItemName = "月尘", Quantity = 5 } }, 250, 125, 0.45f);
            
            // Enchanting Recipes
            AddRecipe(recipes, ref recipeId, "ENCH_SCROLL_01", "力量强化卷轴", CraftingCategory.Enchanting, RecipeDifficulty.Common, 5, 2,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "PAPER", ItemName = "纸张", Quantity = 3 }, new RecipeComponent { ItemId = "ESSENCE_POWER", ItemName = "力量精华", Quantity = 2 } }, 25, 12, 0.80f);
            AddRecipe(recipes, ref recipeId, "ENCH_SCROLL_02", "敏捷强化卷轴", CraftingCategory.Enchanting, RecipeDifficulty.Common, 5, 2,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "PAPER", ItemName = "纸张", Quantity = 3 }, new RecipeComponent { ItemId = "ESSENCE_AGILITY", ItemName = "敏捷精华", Quantity = 2 } }, 25, 12, 0.80f);
            AddRecipe(recipes, ref recipeId, "ENCH_GEM_01", "力量宝石", CraftingCategory.Enchanting, RecipeDifficulty.Rare, 20, 4,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "GEM_RUBY", ItemName = "红宝石", Quantity = 3 }, new RecipeComponent { ItemId = "ESSENCE_POWER", ItemName = "力量精华", Quantity = 5 } }, 80, 40, 0.65f);
            
            // Cooking Recipes
            AddRecipe(recipes, ref recipeId, "COOK_FOOD_01", "烤肉", CraftingCategory.Cooking, RecipeDifficulty.Simple, 1, 1,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "MEAT", ItemName = "肉", Quantity = 2 }, new RecipeComponent { ItemId = "SPICE", ItemName = "香料", Quantity = 1 } }, 5, 2, 0.95f);
            AddRecipe(recipes, ref recipeId, "COOK_FOOD_02", "治疗料理", CraftingCategory.Cooking, RecipeDifficulty.Common, 5, 2,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "MEAT", ItemName = "肉", Quantity = 3 }, new RecipeComponent { ItemId = "HERB_HEAL", ItemName = "治疗草", Quantity = 2 } }, 15, 7, 0.90f);
            AddRecipe(recipes, ref recipeId, "COOK_FOOD_03", "战斗套餐", CraftingCategory.Cooking, RecipeDifficulty.Uncommon, 15, 3,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "MEAT_PREMIUM", ItemName = "优质肉", Quantity = 5 }, new RecipeComponent { ItemId = "HERB_RARE", ItemName = "稀有药草", Quantity = 3 }, new RecipeComponent { ItemId = "WINE", ItemName = "葡萄酒", Quantity = 1 } }, 40, 20, 0.82f);
            
            // Leatherworking Recipes
            AddRecipe(recipes, ref recipeId, "LEATHER_ARMOR_01", "皮甲", CraftingCategory.Leatherworking, RecipeDifficulty.Simple, 1, 1,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "LEATHER", ItemName = "皮革", Quantity = 4 } }, 8, 4, 0.92f);
            AddRecipe(recipes, ref recipeId, "LEATHER_ARMOR_02", "强化皮甲", CraftingCategory.Leatherworking, RecipeDifficulty.Common, 8, 2,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "LEATHER_THICK", ItemName = "厚皮革", Quantity = 6 }, new RecipeComponent { ItemId = "IRON_INGOT", ItemName = "铁锭", Quantity = 2 } }, 22, 11, 0.85f);
            
            // Tailoring Recipes
            AddRecipe(recipes, ref recipeId, "TAILOR_CLOTH_01", "布衣", CraftingCategory.Tailoring, RecipeDifficulty.Simple, 1, 1,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "CLOTH", ItemName = "布料", Quantity = 5 } }, 6, 3, 0.94f);
            AddRecipe(recipes, ref recipeId, "TAILOR_CLOTH_02", "魔法长袍", CraftingCategory.Tailoring, RecipeDifficulty.Uncommon, 15, 3,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "CLOTH_FINE", ItemName = "优质布料", Quantity = 8 }, new RecipeComponent { ItemId = "THREAD_MAGIC", ItemName = "魔法丝线", Quantity = 3 } }, 45, 22, 0.78f);
            
            // Jewelcrafting Recipes
            AddRecipe(recipes, ref recipeId, "JEWEL_RING_01", "力量戒指", CraftingCategory.Jewelcrafting, RecipeDifficulty.Common, 10, 3,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "GEM_RUBY", ItemName = "红宝石", Quantity = 2 }, new RecipeComponent { ItemId = "GOLD_INGOT", ItemName = "金锭", Quantity = 3 } }, 35, 17, 0.72f);
            AddRecipe(recipes, ref recipeId, "JEWEL_AMULET_01", "生命护符", CraftingCategory.Jewelcrafting, RecipeDifficulty.Rare, 20, 4,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "GEM_EMERALD", ItemName = "祖母绿", Quantity = 3 }, new RecipeComponent { ItemId = "SILVER_INGOT", ItemName = "银锭", Quantity = 5 }, new RecipeComponent { ItemId = "ESSENCE_LIFE", ItemName = "生命精华", Quantity = 3 } }, 90, 45, 0.60f);
            
            // Inscription Recipes
            AddRecipe(recipes, ref recipeId, "INSCRIBE_SCROLL_01", "火球术卷轴", CraftingCategory.Inscription, RecipeDifficulty.Common, 8, 2,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "PAPER", ItemName = "纸张", Quantity = 4 }, new RecipeComponent { ItemId = "INK_FIRE", ItemName = "火焰墨水", Quantity = 2 } }, 20, 10, 0.82f);
            AddRecipe(recipes, ref recipeId, "INSCRIBE_SCROLL_02", "冰霜护盾卷轴", CraftingCategory.Inscription, RecipeDifficulty.Uncommon, 15, 3,
                new List<RecipeComponent> { new RecipeComponent { ItemId = "PAPER_PREMIUM", ItemName = "优质纸张", Quantity = 5 }, new RecipeComponent { ItemId = "INK_ICE", ItemName = "冰霜墨水", Quantity = 3 }, new RecipeComponent { ItemId = "CRYSTAL_ICE", ItemName = "冰晶", Quantity = 2 } }, 50, 25, 0.72f);
            
            return recipes;
        }
        
        private void AddRecipe(Dictionary<string, CraftingRecipe> recipes, ref int id, string recipeId, string name, 
            CraftingCategory category, RecipeDifficulty difficulty, int reqLevel, int reqMastery,
            List<RecipeComponent> components, int exp, int masterpieceExp, float successRate)
        {
            recipes[recipeId] = new CraftingRecipe
            {
                Id = recipeId,
                Name = name,
                Category = category,
                Difficulty = difficulty,
                RequiredLevel = reqLevel,
                RequiredMasteryLevel = reqMastery,
                Components = components,
                ExperienceReward = exp,
                MasterpieceBonusExp = masterpieceExp,
                SuccessRate = successRate,
                MasterpieceChance = 0.05f + (int)difficulty * 0.02f
            };
        }
        
        private Dictionary<int, MasteryBonus> InitializeMasteryBonuses()
        {
            return new Dictionary<int, MasteryBonus>
            {
                { 1, new MasteryBonus { SuccessRateBonus = 0.0f, ExpBonus = 0.0f, CostReduction = 0.0f, UnlockCount = 0 } },
                { 2, new MasteryBonus { SuccessRateBonus = 0.02f, ExpBonus = 0.05f, CostReduction = 0.02f, UnlockCount = 2 } },
                { 3, new MasteryBonus { SuccessRateBonus = 0.05f, ExpBonus = 0.10f, CostReduction = 0.05f, UnlockCount = 5 } },
                { 4, new MasteryBonus { SuccessRateBonus = 0.08f, ExpBonus = 0.15f, CostReduction = 0.08f, UnlockCount = 8 } },
                { 5, new MasteryBonus { SuccessRateBonus = 0.12f, ExpBonus = 0.20f, CostReduction = 0.12f, UnlockCount = 12 } },
                { 6, new MasteryBonus { SuccessRateBonus = 0.18f, ExpBonus = 0.30f, CostReduction = 0.18f, UnlockCount = 20 } }
            };
        }
        
        public int GetExperienceForLevel(int level)
        {
            return _levelThresholds.ContainsKey(level) ? _levelThresholds[level] : 0;
        }
        
        public CategoryConfig GetCategoryConfig(CraftingCategory category)
        {
            return _categoryConfigs.ContainsKey(category) ? _categoryConfigs[category] : null;
        }
        
        public CraftingRecipe GetRecipe(string recipeId)
        {
            return _recipes.ContainsKey(recipeId) ? _recipes[recipeId] : null;
        }
        
        public List<CraftingRecipe> GetRecipesByCategory(CraftingCategory category)
        {
            var result = new List<CraftingRecipe>();
            foreach (var recipe in _recipes.Values)
            {
                if (recipe.Category == category)
                    result.Add(recipe);
            }
            return result;
        }
        
        public List<CraftingRecipe> GetAvailableRecipes(CraftingCategory category, int playerLevel, int masteryLevel)
        {
            var result = new List<CraftingRecipe>();
            foreach (var recipe in _recipes.Values)
            {
                if (recipe.Category == category && playerLevel >= recipe.RequiredLevel && masteryLevel >= recipe.RequiredMasteryLevel)
                    result.Add(recipe);
            }
            return result;
        }
        
        public MasteryBonus GetMasteryBonus(int level)
        {
            return _masteryBonuses.ContainsKey(level) ? _masteryBonuses[level] : null;
        }
        
        public int GetTotalRecipesCount() => _recipes.Count;
        
        public Dictionary<CraftingCategory, List<CraftingRecipe>> GetAllRecipesByCategory()
        {
            var result = new Dictionary<CraftingCategory, List<CraftingRecipe>>();
            foreach (CraftingCategory cat in Enum.GetValues(typeof(CraftingCategory)))
            {
                result[cat] = GetRecipesByCategory(cat);
            }
            return result;
        }
    }
    
    /// <summary>
    /// Category configuration
    /// </summary>
    public class CategoryConfig
    {
        public CraftingCategory Category { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public string Icon { get; set; }
        public float BaseSuccessRate { get; set; }
        public float LevelMultiplier { get; set; }
    }
    
    /// <summary>
    /// Mastery bonus configuration
    /// </summary>
    public class MasteryBonus
    {
        public float SuccessRateBonus { get; set; }
        public float ExpBonus { get; set; }
        public float CostReduction { get; set; }
        public int UnlockCount { get; set; }
    }
}
