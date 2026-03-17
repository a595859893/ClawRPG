using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

namespace ClawRPG.Scripts.Crafting {
    /// <summary>
    /// Crafting recipe - defines how to craft an item
    /// </summary>
    public class CraftingRecipe
    {
        public int Id { get; set; }
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public int ResultItemId { get; set; }
        public int ResultQuantity { get; set; } = 1;
        
        // Required materials: ItemId -> Quantity
        public Dictionary<int, int> Materials { get; set; } = new();
        
        // Crafting requirements
        public int RequiredLevel { get; set; } = 1;
        public string RequiredSkill { get; set; } = "";
        public int RequiredSkillLevel { get; set; } = 0;
        
        // Crafting station
        public string StationType { get; set; } = "any"; // "forge", "alchemy", "enchant", "any"
    }
    
    /// <summary>
    /// Crafting recipe database
    /// </summary>
    public class RecipeDatabase
    {
        private static RecipeDatabase _instance;
        public static RecipeDatabase Instance => _instance ??= new RecipeDatabase();
        
        private Dictionary<int, CraftingRecipe> _recipes = new();
        
        public RecipeDatabase()
        {
            Initialize();
        }
        
        private void Initialize()
        {
            // Weapon crafting recipes
            AddRecipe(new CraftingRecipe {
                Id = 1,
                Name = "铁剑",
                Description = "锻造一把铁剑",
                ResultItemId = 2,
                ResultQuantity = 1,
                Materials = { {301, 5}, {308, 3} },
                RequiredLevel = 1,
                StationType = "forge"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 2,
                Name = "钢剑",
                Description = "锻造一把钢剑",
                ResultItemId = 3,
                ResultQuantity = 1,
                Materials = { {301, 10}, {308, 5}, {309, 3} },
                RequiredLevel = 5,
                StationType = "forge"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 3,
                Name = "银剑",
                Description = "锻造一把银剑",
                ResultItemId = 4,
                ResultQuantity = 1,
                Materials = { {302, 3}, {304, 5}, {308, 10} },
                RequiredLevel = 10,
                StationType = "forge"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 4,
                Name = "火焰之剑",
                Description = "附魔火焰属性的剑",
                ResultItemId = 5,
                ResultQuantity = 1,
                Materials = { {303, 2}, {304, 10}, {4, 1} },
                RequiredLevel = 15,
                StationType = "enchant"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 5,
                Name = "冰霜之剑",
                Description = "附魔冰霜属性的剑",
                ResultItemId = 6,
                ResultQuantity = 1,
                Materials = { {303, 2}, {305, 10}, {4, 1} },
                RequiredLevel = 15,
                StationType = "enchant"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 6,
                Name = "雷神之锤",
                Description = "锻造雷神之锤",
                ResultItemId = 7,
                ResultQuantity = 1,
                Materials = { {302, 5}, {304, 15}, {308, 20} },
                RequiredLevel = 20,
                StationType = "forge"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 7,
                Name = "传奇之刃",
                Description = "传说中的终极武器",
                ResultItemId = 8,
                ResultQuantity = 1,
                Materials = { {302, 10}, {303, 5}, {304, 20}, {305, 20} },
                RequiredLevel = 30,
                StationType = "forge"
            });
            
            // Armor crafting recipes
            AddRecipe(new CraftingRecipe {
                Id = 101,
                Name = "皮甲",
                Description = "制作皮甲",
                ResultItemId = 102,
                ResultQuantity = 1,
                Materials = { {307, 5} },
                RequiredLevel = 1,
                StationType = "forge"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 102,
                Name = "锁甲",
                Description = "制作锁甲",
                ResultItemId = 103,
                ResultQuantity = 1,
                Materials = { {308, 10}, {309, 5} },
                RequiredLevel = 5,
                StationType = "forge"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 103,
                Name = "铁甲",
                Description = "制作铁甲",
                ResultItemId = 104,
                ResultQuantity = 1,
                Materials = { {308, 20}, {301, 10} },
                RequiredLevel = 10,
                StationType = "forge"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 104,
                Name = "龙鳞甲",
                Description = "用龙鳞制作护甲",
                ResultItemId = 105,
                ResultQuantity = 1,
                Materials = { {302, 5}, {308, 30}, {301, 20} },
                RequiredLevel = 20,
                StationType = "forge"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 105,
                Name = "金甲",
                Description = "制作华丽金甲",
                ResultItemId = 106,
                ResultQuantity = 1,
                Materials = { {302, 10}, {305, 20}, {308, 50} },
                RequiredLevel = 25,
                StationType = "forge"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 106,
                Name = "神话战甲",
                Description = "神话级护甲",
                ResultItemId = 107,
                ResultQuantity = 1,
                Materials = { {302, 15}, {303, 10}, {304, 30}, {305, 30} },
                RequiredLevel = 35,
                StationType = "forge"
            });
            
            // Consumable crafting recipes
            AddRecipe(new CraftingRecipe {
                Id = 201,
                Name = "小生命药水",
                Description = "制作小生命药水",
                ResultItemId = 201,
                ResultQuantity = 3,
                Materials = { {301, 2} },
                RequiredLevel = 1,
                StationType = "alchemy"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 202,
                Name = "中生命药水",
                Description = "制作中生命药水",
                ResultItemId = 202,
                ResultQuantity = 2,
                Materials = { {301, 5}, {309, 2} },
                RequiredLevel = 5,
                StationType = "alchemy"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 203,
                Name = "大生命药水",
                Description = "制作大生命药水",
                ResultItemId = 203,
                ResultQuantity = 1,
                Materials = { {301, 10}, {302, 2}, {309, 5} },
                RequiredLevel = 15,
                StationType = "alchemy"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 204,
                Name = "法力药水",
                Description = "制作法力药水",
                ResultItemId = 204,
                ResultQuantity = 3,
                Materials = { {305, 2} },
                RequiredLevel = 3,
                StationType = "alchemy"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 205,
                Name = "力量药水",
                Description = "临时提升攻击力",
                ResultItemId = 207,
                ResultQuantity = 1,
                Materials = { {304, 3}, {301, 5} },
                RequiredLevel = 10,
                StationType = "alchemy"
            });
            
            AddRecipe(new CraftingRecipe {
                Id = 206,
                Name = "防御药水",
                Description = "临时提升防御力",
                ResultItemId = 208,
                ResultQuantity = 1,
                Materials = { {305, 3}, {301, 5} },
                RequiredLevel = 10,
                StationType = "alchemy"
            });
        }
        
        private void AddRecipe(CraftingRecipe recipe)
        {
            _recipes[recipe.Id] = recipe;
        }
        
        public CraftingRecipe GetRecipe(int id)
        {
            return _recipes.ContainsKey(id) ? _recipes[id] : null;
        }
        
        public List<CraftingRecipe> GetAllRecipes()
        {
            return new List<CraftingRecipe>(_recipes.Values);
        }
        
        public List<CraftingRecipe> GetRecipesByStation(string stationType)
        {
            var result = new List<CraftingRecipe>();
            foreach (var recipe in _recipes.Values)
            {
                if (recipe.StationType == stationType || recipe.StationType == "any")
                    result.Add(recipe);
            }
            return result;
        }
        
        public List<CraftingRecipe> GetRecipesByLevel(int playerLevel)
        {
            var result = new List<CraftingRecipe>();
            foreach (var recipe in _recipes.Values)
            {
                if (recipe.RequiredLevel <= playerLevel)
                    result.Add(recipe);
            }
            return result;
        }
    }
    
    /// <summary>
    /// Crafting manager - handles player crafting operations
    /// <summary>
    /// Crafting system - handles player crafting operations
    /// </summary>
    public class CraftingSystem : BaseSystem
    {
        private static CraftingSystem _instance;
        public static new CraftingSystem Instance
        {
            get => _instance;
            private set => _instance = value;
        }
        
        // Event for crafting success
        public static event Action<CraftingRecipe, int> OnCraftingSuccess;
        public static event Action<string> OnCraftingFailed;
        
        private RecipeDatabase _recipeDatabase;
        
        public override void _Ready()
        {
            base._Ready();
            Instance = this;
            _recipeDatabase = RecipeDatabase.Instance;
            LoadData();
        }
        
        protected override void Initialize()
        {
            GD.Print("[CraftingSystem] Initialized");
        }
        
        /// <summary>
        /// Export save data
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary();
        }
        
        /// <summary>
        /// Import save data
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
        }
        
        /// <summary>
        /// Check if player can craft a recipe
        /// </summary>
        public bool CanCraft(ClawRPG.Scripts.Items.Inventory inventory, int recipeId, int playerLevel = 1)
        {
            var recipe = _recipeDatabase.GetRecipe(recipeId);
            if (recipe == null) return false;
            
            // Check player level
            if (recipe.RequiredLevel > playerLevel)
            {
                OnCraftingFailed?.Invoke($"需要等级 {recipe.RequiredLevel} 才能制作 {recipe.Name}");
                return false;
            }
            
            // Check materials
            foreach (var material in recipe.Materials)
            {
                int requiredQuantity = material.Value;
                int availableQuantity = GetMaterialQuantity(inventory, material.Key);
                
                if (availableQuantity < requiredQuantity)
                {
                    var item = ClawRPG.Scripts.Items.ItemDatabase.Instance.GetItem(material.Key);
                    OnCraftingFailed?.Invoke($"材料不足: {item?.Name ?? "Unknown"} (需要 {requiredQuantity}, 有 {availableQuantity})");
                    return false;
                }
            }
            
            // Check inventory space
            var resultItem = ClawRPG.Scripts.Items.ItemDatabase.Instance.GetItem(recipe.ResultItemId);
            if (resultItem == null) return false;
            
            int freeSlots = GetFreeInventorySlots(inventory);
            bool canStack = CanStackItem(inventory, recipe.ResultItemId, recipe.ResultQuantity);
            
            if (!canStack && freeSlots < 1)
            {
                OnCraftingFailed?.Invoke("背包空间不足");
                return false;
            }
            
            return true;
        }
        
        /// <summary>
        /// Attempt to craft an item
        /// </summary>
        public bool Craft(ClawRPG.Scripts.Items.Inventory inventory, int recipeId, int playerLevel = 1)
        {
            if (!CanCraft(inventory, recipeId, playerLevel))
                return false;
            
            var recipe = _recipeDatabase.GetRecipe(recipeId);
            
            // Consume materials
            foreach (var material in recipe.Materials)
            {
                RemoveMaterial(inventory, material.Key, material.Value);
            }
            
            // Add result item
            inventory.AddItem(recipe.ResultItemId, recipe.ResultQuantity);
            
            // Track achievement progress
            AchievementManager.Instance.TrackCraft();
            
            // Trigger success event
            OnCraftingSuccess?.Invoke(recipe, recipe.ResultQuantity);
            
            GD.Print($"[Crafting] 成功制作: {recipe.Name} x{recipe.ResultQuantity}");
            
            return true;
        }
        
        /// <summary>
        /// Get all available recipes for player's level
        /// </summary>
        public List<CraftingRecipe> GetAvailableRecipes(int playerLevel)
        {
            return _recipeDatabase.GetRecipesByLevel(playerLevel);
        }
        
        /// <summary>
        /// Get crafting progress for a recipe (0.0 - 1.0)
        /// </summary>
        public float GetCraftingProgress(ClawRPG.Scripts.Items.Inventory inventory, int recipeId)
        {
            var recipe = _recipeDatabase.GetRecipe(recipeId);
            if (recipe == null) return 0f;
            
            float minProgress = 1f;
            
            foreach (var material in recipe.Materials)
            {
                int required = material.Value;
                int available = GetMaterialQuantity(inventory, material.Key);
                
                float progress = (float)available / required;
                minProgress = Math.Min(minProgress, progress);
            }
            
            return Math.Min(minProgress, 1f);
        }
        
        private int GetMaterialQuantity(ClawRPG.Scripts.Items.Inventory inventory, int itemId)
        {
            // This would need to access inventory's internal data
            // For now, return 0 - integration with inventory needed
            return 0;
        }
        
        private void RemoveMaterial(ClawRPG.Scripts.Items.Inventory inventory, int itemId, int quantity)
        {
            // Integration with inventory system
            // This would remove items from inventory
        }
        
        private int GetFreeInventorySlots(ClawRPG.Scripts.Items.Inventory inventory)
        {
            // This would check inventory for free slots
            return 5; // Placeholder
        }
        
        private bool CanStackItem(ClawRPG.Scripts.Items.Inventory inventory, int itemId, int quantity)
        {
            // This would check if item can be stacked
            return false; // Placeholder
        }
    }
}
