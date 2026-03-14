using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Systems.CraftingMastery;

namespace ClawRPG.Systems
{
    /// <summary>
    /// Core system for crafting mastery
    /// </summary>
    public class CraftingMasterySystem : Node
    {
        private static CraftingMasterySystem _instance;
        public static CraftingMasterySystem Instance => _instance;
        
        private CraftingMasteryData _playerData;
        private CraftingMasteryDatabase _database;
        private CraftingProgress _currentProgress;
        private Random _random = new Random();
        
        // Signals
        public static string CraftingCompletedSignal => "crafting_completed";
        public static string MasteryLevelUpSignal => "mastery_level_up";
        public static string RecipeUnlockedSignal => "recipe_unlocked";
        
        public override void _Ready()
        {
            _instance = this;
            _database = CraftingMasteryDatabase.Instance;
            _playerData = new CraftingMasteryData();
            
            // Initialize all categories
            foreach (CraftingCategory cat in Enum.GetValues(typeof(CraftingCategory)))
            {
                if (!_playerData.CategoryMasteries.ContainsKey(cat))
                {
                    _playerData.CategoryMasteries[cat] = new CategoryMastery
                    {
                        Category = cat,
                        Level = 1,
                        Experience = 0
                    };
                }
            }
            
            GD.Print("[CraftingMasterySystem] Initialized successfully");
        }
        
        /// <summary>
        /// Start crafting a recipe
        /// </summary>
        public bool StartCrafting(string recipeId)
        {
            var recipe = _database.GetRecipe(recipeId);
            if (recipe == null)
            {
                GD.PrintErr($"[CraftingMasterySystem] Recipe not found: {recipeId}");
                return false;
            }
            
            var categoryMastery = GetCategoryMastery(recipe.Category);
            if (categoryMastery == null)
            {
                GD.PrintErr($"[CraftingMasterySystem] Category mastery not found: {recipe.Category}");
                return false;
            }
            
            // Check requirements
            // Note: player level check would require player stats system
            if (categoryMastery.Level < recipe.RequiredMasteryLevel)
            {
                GD.PrintErr($"[CraftingMasterySystem] Insufficient mastery level. Required: {recipe.RequiredMasteryLevel}, Current: {categoryMastery.Level}");
                return false;
            }
            
            // Start crafting progress
            _currentProgress = new CraftingProgress
            {
                RecipeId = recipeId,
                Category = recipe.Category,
                StartTime = DateTime.Now,
                ProgressPercent = 0,
                IsComplete = false
            };
            
            // Simulate crafting time based on difficulty
            int craftTimeMs = 500 + (int)recipe.Difficulty * 200;
            
            // Process crafting after delay
            CallDeferred(nameof(CompleteCrafting), recipeId);
            
            GD.Print($"[CraftingMasterySystem] Started crafting: {recipe.Name}");
            return true;
        }
        
        /// <summary>
        /// Complete the crafting process
        /// </summary>
        private void CompleteCrafting(string recipeId)
        {
            if (_currentProgress == null || _currentProgress.RecipeId != recipeId)
                return;
                
            var recipe = _database.GetRecipe(recipeId);
            var categoryMastery = GetCategoryMastery(recipe.Category);
            var categoryConfig = _database.GetCategoryConfig(recipe.Category);
            var masteryBonus = _database.GetMasteryBonus(categoryMastery.Level);
            
            // Calculate success chance with mastery bonuses
            float successChance = recipe.SuccessRate;
            successChance += masteryBonus.SuccessRateBonus;
            successChance += categoryMastery.Level * categoryConfig.LevelMultiplier;
            successChance = Mathf.Clamp(successChance, 0.1f, 1.0f);
            
            // Determine success
            bool success = _random.NextDouble() < successChance;
            
            // Determine masterpiece
            bool isMasterpiece = success && _random.NextDouble() < recipe.MasterpieceChance;
            
            // Calculate experience
            int expGained = recipe.ExperienceReward;
            if (isMasterpiece)
            {
                expGained += recipe.MasterpieceBonusExp;
            }
            expGained = (int)(expGained * (1.0f + masteryBonus.ExpBonus));
            
            // Update progress
            _currentProgress.Success = success;
            _currentProgress.IsMasterpiece = isMasterpiece;
            _currentProgress.IsComplete = true;
            _currentProgress.ProgressPercent = 100;
            
            // Update player data
            _playerData.TotalCrafts++;
            categoryMastery.TotalCrafts++;
            
            if (success)
            {
                _playerData.SuccessfulCrafts++;
                categoryMastery.SuccessfulCrafts++;
                categoryMastery.CurrentStreak++;
                
                if (categoryMastery.CurrentStreak > categoryMastery.BestStreak)
                {
                    categoryMastery.BestStreak = categoryMastery.CurrentStreak;
                }
                
                // Add experience
                int oldLevel = categoryMastery.Level;
                categoryMastery.Experience += expGained;
                
                // Check for level up
                while (categoryMastery.Level < 6 && categoryMastery.Experience >= _database.GetExperienceForLevel(categoryMastery.Level + 1))
                {
                    categoryMastery.Level++;
                }
                
                if (categoryMastery.Level > oldLevel)
                {
                    GD.Print($"[CraftingMasterySystem] {recipe.Category} mastery leveled up to {categoryMastery.Level}!");
                    // Emit level up signal
                }
                
                // Check for recipe unlocks
                CheckRecipeUnlocks(categoryMastery);
            }
            else
            {
                categoryMastery.CurrentStreak = 0;
                _playerData.FailedCrafts++;
            }
            
            if (isMasterpiece)
            {
                _playerData.MasterpieceCrafts++;
            }
            
            // Record session
            _playerData.RecentSessions.Add(new CraftingSession
            {
                RecipeId = recipeId,
                Category = recipe.Category,
                Timestamp = DateTime.Now,
                Success = success,
                IsMasterpiece = isMasterpiece,
                ExperienceGained = success ? expGained : 0
            });
            
            // Keep only last 50 sessions
            if (_playerData.RecentSessions.Count > 50)
            {
                _playerData.RecentSessions.RemoveAt(0);
            }
            
            _playerData.LastCraftTime = DateTime.Now;
            
            // Add to unlocked recipes if not already
            if (success && !_playerData.UnlockedRecipes.Contains(recipeId))
            {
                _playerData.UnlockedRecipes.Add(recipeId);
            }
            
            // Update recipe usage count
            if (!categoryMastery.RecipeUsageCount.ContainsKey(recipeId))
            {
                categoryMastery.RecipeUsageCount[recipeId] = 0;
            }
            categoryMastery.RecipeUsageCount[recipeId]++;
            
            GD.Print($"[CraftingMasterySystem] Crafting completed: {recipe.Name}, Success: {success}, Masterpiece: {isMasterpiece}, Exp: {expGained}");
        }
        
        /// <summary>
        /// Check and unlock new recipes based on mastery level
        /// </summary>
        private void CheckRecipeUnlocks(CategoryMastery categoryMastery)
        {
            var availableRecipes = _database.GetAvailableRecipes(categoryMastery.Category, 1, categoryMastery.Level);
            var masteryBonus = _database.GetMasteryBonus(categoryMastery.Level);
            
            foreach (var recipe in availableRecipes)
            {
                if (!_playerData.UnlockedRecipes.Contains(recipe.Id) && !categoryMastery.UnlockedRecipes.Contains(recipe.Id))
                {
                    // Unlock based on mastery bonus
                    if (categoryMastery.Level >= recipe.RequiredMasteryLevel)
                    {
                        categoryMastery.UnlockedRecipes.Add(recipe.Id);
                        GD.Print($"[CraftingMasterySystem] New recipe unlocked: {recipe.Name}");
                    }
                }
            }
        }
        
        /// <summary>
        /// Get category mastery data
        /// </summary>
        public CategoryMastery GetCategoryMastery(CraftingCategory category)
        {
            return _playerData.CategoryMasteries.ContainsKey(category) ? _playerData.CategoryMasteries[category] : null;
        }
        
        /// <summary>
        /// Get overall crafting statistics
        /// </summary>
        public CraftingStatistics GetStatistics()
        {
            var stats = new CraftingStatistics
            {
                TotalCrafts = _playerData.TotalCrafts,
                SuccessfulCrafts = _playerData.SuccessfulCrafts,
                FailedCrafts = _playerData.FailedCrafts,
                MasterpieceCrafts = _playerData.MasterpieceCrafts,
                AverageSuccessRate = _playerData.TotalCrafts > 0 ? (float)_playerData.SuccessfulCrafts / _playerData.TotalCrafts : 0f
            };
            
            // Find best streak
            int bestStreak = 0;
            foreach (var mastery in _playerData.CategoryMasteries.Values)
            {
                if (mastery.BestStreak > bestStreak)
                    bestStreak = mastery.BestStreak;
            }
            stats.BestStreak = bestStreak;
            
            // Category data
            foreach (var mastery in _playerData.CategoryMasteries.Values)
            {
                stats.CategoryCraftCounts[mastery.Category] = mastery.TotalCrafts;
                stats.CategoryMasteryLevels[mastery.Category] = mastery.Level;
            }
            
            // Most used recipe
            string mostUsed = "";
            int mostUsedCount = 0;
            foreach (var mastery in _playerData.CategoryMasteries.Values)
            {
                foreach (var usage in mastery.RecipeUsageCount)
                {
                    if (usage.Value > mostUsedCount)
                    {
                        mostUsed = usage.Key;
                        mostUsedCount = usage.Value;
                    }
                }
            }
            stats.MostUsedRecipe = mostUsed;
            stats.MostUsedRecipeCount = mostUsedCount;
            
            return stats;
        }
        
        /// <summary>
        /// Get current crafting progress
        /// </summary>
        public CraftingProgress GetCurrentProgress()
        {
            return _currentProgress;
        }
        
        /// <summary>
        /// Get recipes for a specific category
        /// </summary>
        public List<CraftingRecipe> GetRecipesForCategory(CraftingCategory category)
        {
            return _database.GetRecipesByCategory(category);
        }
        
        /// <summary>
        /// Get available recipes for a category based on player level and mastery
        /// </summary>
        public List<CraftingRecipe> GetAvailableRecipes(CraftingCategory category, int playerLevel)
        {
            var mastery = GetCategoryMastery(category);
            int masteryLevel = mastery?.Level ?? 1;
            return _database.GetAvailableRecipes(category, playerLevel, masteryLevel);
        }
        
        /// <summary>
        /// Get mastery level name
        /// </summary>
        public string GetMasteryLevelName(int level)
        {
            switch (level)
            {
                case 1: return "新手 (Novice)";
                case 2: return "学徒 (Apprentice)";
                case 3: return "熟练工 (Journeyman)";
                case 4: return "专家 (Expert)";
                case 5: return "大师 (Master)";
                case 6: return "宗师 (GrandMaster)";
                default: return "未知";
            }
        }
        
        /// <summary>
        /// Get experience progress for next level
        /// </summary>
        public (int current, int required, float percent) GetExperienceProgress(CraftingCategory category)
        {
            var mastery = GetCategoryMastery(category);
            if (mastery == null || mastery.Level >= 6)
                return (0, 0, 1.0f);
            
            int currentExp = mastery.Experience;
            int nextLevelExp = _database.GetExperienceForLevel(mastery.Level + 1);
            int currentLevelExp = _database.GetExperienceForLevel(mastery.Level);
            
            int expNeeded = nextLevelExp - currentLevelExp;
            int expProgress = currentExp - currentLevelExp;
            
            float percent = (float)expProgress / expNeeded;
            
            return (expProgress, expNeeded, percent);
        }
        
        /// <summary>
        /// Get player data for save/load
        /// </summary>
        public CraftingMasteryData GetPlayerData()
        {
            return _playerData;
        }
        
        /// <summary>
        /// Load player data from save
        /// </summary>
        public void LoadPlayerData(CraftingMasteryData data)
        {
            _playerData = data;
            
            // Ensure all categories exist
            foreach (CraftingCategory cat in Enum.GetValues(typeof(CraftingCategory)))
            {
                if (!_playerData.CategoryMasteries.ContainsKey(cat))
                {
                    _playerData.CategoryMasteries[cat] = new CategoryMastery
                    {
                        Category = cat,
                        Level = 1,
                        Experience = 0
                    };
                }
            }
            
            GD.Print("[CraftingMasterySystem] Player data loaded");
        }
        
        /// <summary>
        /// Get total unlocked recipes count
        /// </summary>
        public int GetUnlockedRecipesCount()
        {
            return _playerData.UnlockedRecipes.Count;
        }
        
        /// <summary>
        /// Get database instance
        /// </summary>
        public CraftingMasteryDatabase GetDatabase()
        {
            return _database;
        }
    }
}
