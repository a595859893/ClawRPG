using Godot;
using System;
using System.Collections.Generic;

public partial class CookingSystem : BaseSystem
{
    public static CookingSystem Instance { get; private set; }

    private CookingData _cookingData = new CookingData();
    private ActiveCooking _currentCooking = null;
    private bool _isCooking = false;

    // Experience needed per level
    private int[] _expPerLevel = { 0, 100, 250, 500, 800, 1200, 1700, 2300, 3000, 4000, 5500 };

    // Signals
    [Signal] public delegate void RecipeLearnedEventHandler(string recipeId, string recipeName);
    [Signal] public delegate void CookingStartedEventHandler(string recipeId);
    [Signal] public delegate void CookingProgressEventHandler(float progress);
    [Signal] public delegate void CookingCompletedEventHandler(string recipeId, bool success);
    [Signal] public delegate void LevelUpEventHandler(int newLevel, int exp);
    [Signal] public delegate void BuffAppliedEventHandler(string statName, float value, int duration);

    public override void _Ready()
    {
        Instance = this;
        InitializeCooking();
    }

    public void InitializeCooking()
    {
        // Initialize with some basic recipes
        if (_cookingData.knownRecipes.Count == 0)
        {
            LearnRecipe("soup_herb");
            LearnRecipe("salad_green");
            LearnRecipe("dish_roast");
            LearnRecipe("dessert_cake");
            LearnRecipe("drink_herb");
            LearnRecipe("snack_jerky");
        }
    }

    public void LearnRecipe(string recipeId)
    {
        var recipe = CookingDatabase.Instance.GetRecipe(recipeId);
        if (recipe == null) return;

        if (!_cookingData.knownRecipes.ContainsKey(recipeId))
        {
            _cookingData.knownRecipes[recipeId] = _cookingData.cookingLevel;
            EmitSignal(SignalName.RecipeLearned, recipeId, recipe.recipeName);
        }
    }

    public bool CanCook(string recipeId)
    {
        var recipe = CookingDatabase.Instance.GetRecipe(recipeId);
        if (recipe == null) return false;

        // Check if recipe is known
        if (!_cookingData.knownRecipes.ContainsKey(recipeId)) return false;

        // Check cooking level
        if (recipe.requiredCookingLevel > _cookingData.cookingLevel) return false;

        // Check ingredients
        var inventory = InventoryManager.Instance;
        foreach (var ingredient in recipe.ingredients)
        {
            if (!inventory.HasItem(ingredient.Key, ingredient.Value))
            {
                return false;
            }
        }

        return true;
    }

    public bool StartCooking(string recipeId)
    {
        if (_isCooking) return false;

        var recipe = CookingDatabase.Instance.GetRecipe(recipeId);
        if (recipe == null) return false;

        if (!CanCook(recipeId)) return false;

        // Consume ingredients
        var inventory = InventoryManager.Instance;
        foreach (var ingredient in recipe.ingredients)
        {
            inventory.RemoveItem(ingredient.Key, ingredient.Value);
        }

        // Start cooking
        _currentCooking = new ActiveCooking
        {
            recipeId = recipeId,
            progress = 0,
            totalTime = recipe.cookingTime,
            isComplete = false,
            isBurning = false
        };
        _isCooking = true;

        EmitSignal(SignalName.CookingStarted, recipeId);
        return true;
    }

    public void UpdateCooking(float delta)
    {
        if (!_isCooking || _currentCooking == null) return;

        _currentCooking.progress += delta;
        float progressPercent = _currentCooking.progress / _currentCooking.totalTime;
        EmitSignal(SignalName.CookingProgress, progressPercent);

        if (_currentCooking.progress >= _currentCooking.totalTime)
        {
            CompleteCooking();
        }
    }

    private void CompleteCooking()
    {
        if (_currentCooking == null) return;

        _currentCooking.isComplete = true;
        _isCooking = false;

        var recipe = CookingDatabase.Instance.GetRecipe(_currentCooking.recipeId);
        bool success = false;

        // Base success rate based on recipe rarity
        float successRate = 0.95f;
        switch (recipe.rarity)
        {
            case Rarity.Common: successRate = 0.95f; break;
            case Rarity.Uncommon: successRate = 0.90f; break;
            case Rarity.Rare: successRate = 0.80f; break;
            case Rarity.Epic: successRate = 0.70f; break;
            case Rarity.Legendary: successRate = 0.50f; break;
        }

        // Adjust for cooking level
        int levelDiff = _cookingData.cookingLevel - recipe.requiredCookingLevel;
        successRate += levelDiff * 0.05f;
        successRate = Mathf.Clamp(successRate, 0.3f, 0.99f);

        success = GD.Randf() < successRate;

        _cookingData.totalDishesCooked++;

        if (success)
        {
            _cookingData.successfulCooks++;
            
            // Add cooked dish to inventory
            string dishName = recipe.recipeId;
            int stackSize = 1;
            InventoryManager.Instance.AddItem(dishName, stackSize);

            // Track cooked dishes
            if (!_cookingData.cookedDishes.ContainsKey(dishName))
                _cookingData.cookedDishes[dishName] = 0;
            _cookingData.cookedDishes[dishName]++;

            // Award experience
            int expGained = GetRecipeExp(recipe);
            AddExperience(expGained);

            // Apply stat bonuses
            ApplyStatBonuses(recipe.statBonuses);
        }
        else
        {
            _cookingData.failedCooks++;
        }

        EmitSignal(SignalName.CookingCompleted, _currentCooking.recipeId, success);
        _currentCooking = null;
    }

    private int GetRecipeExp(CookingRecipe recipe)
    {
        int baseExp = 10;
        switch (recipe.rarity)
        {
            case Rarity.Common: baseExp = 10; break;
            case Rarity.Uncommon: baseExp = 25; break;
            case Rarity.Rare: baseExp = 50; break;
            case Rarity.Epic: baseExp = 100; break;
            case Rarity.Legendary: baseExp = 200; break;
        }
        return baseExp * (recipe.requiredCookingLevel + 1);
    }

    private void AddExperience(int exp)
    {
        _cookingData.cookingExp += exp;
        
        // Check for level up
        int maxLevel = _expPerLevel.Length;
        while (_cookingData.cookingLevel < maxLevel && 
               _cookingData.cookingExp >= _expPerLevel[_cookingData.cookingLevel])
        {
            _cookingData.cookingLevel++;
            EmitSignal(SignalName.LevelUp, _cookingData.cookingLevel, _cookingData.cookingExp);
        }
    }

    private void ApplyStatBonuses(List<StatBonus> bonuses)
    {
        foreach (var bonus in bonuses)
        {
            if (bonus.duration > 0)
            {
                // Temporary buff
                ApplyBuff(bonus.statName, bonus.bonusValue, bonus.duration);
            }
            else
            {
                // Permanent stat boost
                ApplyPermanentBuff(bonus.statName, bonus.bonusValue);
            }
        }
    }

    private void ApplyBuff(string statName, float value, int durationMinutes)
    {
        EmitSignal(SignalName.BuffApplied, statName, value, durationMinutes);
        
        // Apply to player stats (simplified - would need PlayerStats integration)
        var player = GetTree().CurrentScene.FindChild("Player", true, false) as CharacterBody2D;
        if (player != null)
        {
            // Add temporary stat modifier
            // This would need actual implementation with PlayerStats system
        }
    }

    private void ApplyPermanentBuff(string statName, float value)
    {
        // Apply permanent stat boost
        EmitSignal(SignalName.BuffApplied, statName, value, 0);
    }

    public void CancelCooking()
    {
        if (!_isCooking) return;
        
        // Return some ingredients (50%)
        if (_currentCooking != null)
        {
            var recipe = CookingDatabase.Instance.GetRecipe(_currentCooking.recipeId);
            if (recipe != null)
            {
                var inventory = InventoryManager.Instance;
                foreach (var ingredient in recipe.ingredients)
                {
                    int returnAmount = (int)Mathf.Ceil(ingredient.Value * 0.5f);
                    if (returnAmount > 0)
                    {
                        inventory.AddItem(ingredient.Key, returnAmount);
                    }
                }
            }
        }

        _isCooking = false;
        _currentCooking = null;
    }

    public void AddFavoriteRecipe(string recipeId)
    {
        if (!_cookingData.favoriteRecipes.Contains(recipeId))
        {
            _cookingData.favoriteRecipes.Add(recipeId);
        }
    }

    public void RemoveFavoriteRecipe(string recipeId)
    {
        _cookingData.favoriteRecipes.Remove(recipeId);
    }

    // Getters
    public CookingData GetCookingData() => _cookingData;
    public int GetCookingLevel() => _cookingData.cookingLevel;
    public int GetCookingExp() => _cookingData.cookingExp;
    public int GetExpForNextLevel()
    {
        int level = Mathf.Min(_cookingData.cookingLevel + 1, _expPerLevel.Length - 1);
        return _expPerLevel[level];
    }
    public bool IsCooking() => _isCooking;
    public ActiveCooking GetCurrentCooking() => _currentCooking;
    public Dictionary<string, int> GetKnownRecipes() => _cookingData.knownRecipes;
    public Dictionary<string, int> GetCookedDishes() => _cookingData.cookedDishes;
    public List<string> GetFavoriteRecipes() => _cookingData.favoriteRecipes;

    public Dictionary<string, int> GetStatistics()
    {
        return new Dictionary<string, int>
        {
            { "total_cooked", _cookingData.totalDishesCooked },
            { "successful", _cookingData.successfulCooks },
            { "failed", _cookingData.failedCooks },
            { "success_rate", _cookingData.totalDishesCooked > 0 ? 
                (int)((float)_cookingData.successfulCooks / _cookingData.totalDishesCooked * 100) : 0 }
        };
    }

    // Save/Load
    public Dictionary<string, Variant> SaveData()
    {
        return new Dictionary<string, Variant>
        {
            { "known_recipes", _cookingData.knownRecipes },
            { "cooking_level", _cookingData.cookingLevel },
            { "cooking_exp", _cookingData.cookingExp },
            { "total_dishes_cooked", _cookingData.totalDishesCooked },
            { "successful_cooks", _cookingData.successfulCooks },
            { "failed_cooks", _cookingData.failedCooks },
            { "cooked_dishes", _cookingData.cookedDishes },
            { "favorite_recipes", _cookingData.favoriteRecipes }
        };
    }

    public void LoadData(Dictionary<string, Variant> data)
    {
        if (data == null) return;

        if (data.ContainsKey("known_recipes"))
            _cookingData.knownRecipes = (Dictionary<string, int>)data["known_recipes"];
        if (data.ContainsKey("cooking_level"))
            _cookingData.cookingLevel = (int)data["cooking_level"];
        if (data.ContainsKey("cooking_exp"))
            _cookingData.cookingExp = (int)data["cooking_exp"];
        if (data.ContainsKey("total_dishes_cooked"))
            _cookingData.totalDishesCooked = (int)data["total_dishes_cooked"];
        if (data.ContainsKey("successful_cooks"))
            _cookingData.successfulCooks = (int)data["successful_cooks"];
        if (data.ContainsKey("failed_cooks"))
            _cookingData.failedCooks = (int)data["failed_cooks"];
        if (data.ContainsKey("cooked_dishes"))
            _cookingData.cookedDishes = (Dictionary<string, int>)data["cooked_dishes"];
        if (data.ContainsKey("favorite_recipes"))
            _cookingData.favoriteRecipes = (List<string>)data["favorite_recipes"];
    }

    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return SaveData();
    }

    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.Contains("known_recipes"))
            _cookingData.knownRecipes = (Dictionary<string, int>)data["known_recipes"];
        if (data.Contains("cooking_level"))
            _cookingData.cookingLevel = (int)data["cooking_level"];
        if (data.Contains("cooking_exp"))
            _cookingData.cookingExp = (int)data["cooking_exp"];
        if (data.Contains("total_dishes_cooked"))
            _cookingData.totalDishesCooked = (int)data["total_dishes_cooked"];
        if (data.Contains("successful_cooks"))
            _cookingData.successfulCooks = (int)data["successful_cooks"];
        if (data.Contains("failed_cooks"))
            _cookingData.failedCooks = (int)data["failed_cooks"];
        if (data.Contains("cooked_dishes"))
            _cookingData.cookedDishes = (Dictionary<string, int>)data["cooked_dishes"];
        if (data.Contains("favorite_recipes"))
            _cookingData.favoriteRecipes = (List<string>)data["favorite_recipes"];
    }
}
