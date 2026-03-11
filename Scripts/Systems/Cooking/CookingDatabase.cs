using Godot;
using System;
using System.Collections.Generic;

public class CookingDatabase
{
    private static CookingDatabase _instance;
    public static CookingDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new CookingDatabase();
            return _instance;
        }
    }

    public Dictionary<string, CookingRecipe> recipes = new Dictionary<string, CookingRecipe>();
    public Dictionary<FoodType, List<string>> recipesByType = new Dictionary<FoodType, List<string>>();
    public Dictionary<Rarity, List<string>> recipesByRarity = new Dictionary<Rarity, List<string>>();

    public CookingDatabase()
    {
        InitializeRecipes();
    }

    private void InitializeRecipes()
    {
        // Soups (汤类)
        RegisterRecipe(new CookingRecipe
        {
            recipeId = "soup_herb",
            recipeName = "Herb Soup",
            description = "A refreshing herbal soup that restores vitality",
            foodType = FoodType.Soup,
            rarity = Rarity.Common,
            ingredients = new Dictionary<string, int> { { "green_herb", 2 }, { "water", 1 } },
            cookingTime = 30,
            hungerRestored = 20,
            energyRestored = 10,
            statBonuses = new List<StatBonus> { new StatBonus { statName = "health_regen", bonusValue = 2, duration = 5 } }
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "soup_meat",
            recipeName = "Meat Soup",
            description = "Hearty meat soup for warriors",
            foodType = FoodType.Soup,
            rarity = Rarity.Uncommon,
            ingredients = new Dictionary<string, int> { { "meat", 3 }, { "water", 1 }, { "herb", 1 } },
            cookingTime = 45,
            hungerRestored = 35,
            energyRestored = 15,
            statBonuses = new List<StatBonus> { new StatBonus { statName = "attack", bonusValue = 5, duration = 10 } }
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "soup_dragon",
            recipeName = "Dragon Bone Soup",
            description = "Legendary soup made with dragon bones",
            foodType = FoodType.Soup,
            rarity = Rarity.Legendary,
            ingredients = new Dictionary<string, int> { { "dragon_bone", 2 }, { "ginseng", 3 }, { "fire_herb", 2 }, { "premium_broth", 1 } },
            cookingTime = 180,
            hungerRestored = 100,
            energyRestored = 50,
            statBonuses = new List<StatBonus> 
            { 
                new StatBonus { statName = "attack", bonusValue = 20, duration = 30 },
                new StatBonus { statName = "defense", bonusValue = 15, duration = 30 }
            },
            requiredCookingLevel = 5
        });

        // Salads (沙拉)
        RegisterRecipe(new CookingRecipe
        {
            recipeId = "salad_green",
            recipeName = "Green Salad",
            description = "Fresh vegetables with a light dressing",
            foodType = FoodType.Salad,
            rarity = Rarity.Common,
            ingredients = new Dictionary<string, int> { { "lettuce", 2 }, { "tomato", 1 }, { "cucumber", 1 } },
            cookingTime = 20,
            hungerRestored = 15,
            energyRestored = 20,
            statBonuses = new List<StatBonus> { new StatBonus { statName = "dodge", bonusValue = 3, duration = 10 } }
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "salad_fruit",
            recipeName = "Fruit Paradise",
            description = "Exotic fruits from distant lands",
            foodType = FoodType.Salad,
            rarity = Rarity.Rare,
            ingredients = new Dictionary<string, int> { { "apple", 2 }, { "banana", 2 }, { "orange", 1 }, { "honey", 1 } },
            cookingTime = 30,
            hungerRestored = 25,
            energyRestored = 40,
            statBonuses = new List<StatBonus> 
            { 
                new StatBonus { statName = "speed", bonusValue = 10, duration = 15 },
                new StatBonus { statName = "luck", bonusValue = 5, duration = 15 }
            },
            requiredCookingLevel = 3
        });

        // Main Dishes (主菜)
        RegisterRecipe(new CookingRecipe
        {
            recipeId = "dish_roast",
            recipeName = "Roasted Meat",
            description = "Perfectly roasted meat with herbs",
            foodType = FoodType.MainDish,
            rarity = Rarity.Uncommon,
            ingredients = new Dictionary<string, int> { { "meat", 4 }, { "herb", 2 }, { "spice", 1 } },
            cookingTime = 60,
            hungerRestored = 50,
            energyRestored = 25,
            statBonuses = new List<StatBonus> { new StatBonus { statName = "attack", bonusValue = 10, duration = 15 } }
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "dish_fish",
            recipeName = "Grilled Fish",
            description = "Freshly caught fish grilled to perfection",
            foodType = FoodType.MainDish,
            rarity = Rarity.Uncommon,
            ingredients = new Dictionary<string, int> { { "fish", 3 }, { "lemon", 1 }, { "herb", 1 } },
            cookingTime = 45,
            hungerRestored = 45,
            energyRestored = 30,
            statBonuses = new List<StatBonus> { new StatBonus { statName = "magic", bonusValue = 8, duration = 15 } }
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "dish_phoenix",
            recipeName = "Phoenix Roast",
            description = "Legendary dish made with phoenix meat",
            foodType = FoodType.MainDish,
            rarity = Rarity.Legendary,
            ingredients = new Dictionary<string, int> { { "phoenix_feather", 2 }, { "fire_herb", 3 }, { "golden_apple", 2 }, { "premium_spice", 2 } },
            cookingTime = 240,
            hungerRestored = 150,
            energyRestored = 100,
            statBonuses = new List<StatBonus> 
            { 
                new StatBonus { statName = "attack", bonusValue = 30, duration = 60 },
                new StatBonus { statName = "critical_rate", bonusValue = 10, duration = 60 },
                new StatBonus { statName = "lifesteal", bonusValue = 5, duration = 60 }
            },
            requiredCookingLevel = 8
        });

        // Desserts (甜点)
        RegisterRecipe(new CookingRecipe
        {
            recipeId = "dessert_cake",
            recipeName = "Honey Cake",
            description = "Sweet cake made with golden honey",
            foodType = FoodType.Dessert,
            rarity = Rarity.Common,
            ingredients = new Dictionary<string, int> { { "honey", 2 }, { "flour", 2 }, { "egg", 1 } },
            cookingTime = 40,
            hungerRestored = 30,
            energyRestored = 35,
            statBonuses = new List<StatBonus> { new StatBonus { statName = "luck", bonusValue = 5, duration = 20 } }
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "dessert_ice",
            recipeName = "Frost Ice Cream",
            description = "Ice cream frozen with ice magic",
            foodType = FoodType.Dessert,
            rarity = Rarity.Epic,
            ingredients = new Dictionary<string, int> { { "milk", 3 }, { "ice_crystal", 2 }, { "sugar", 2 }, { "fruit", 2 } },
            cookingTime = 50,
            hungerRestored = 20,
            energyRestored = 60,
            statBonuses = new List<StatBonus> 
            { 
                new StatBonus { statName = "defense", bonusValue = 15, duration = 30 },
                new StatBonus { statName = "ice_resist", bonusValue = 20, duration = 30 }
            },
            requiredCookingLevel = 4
        });

        // Drinks (饮品)
        RegisterRecipe(new CookingRecipe
        {
            recipeId = "drink_herb",
            recipeName = "Herbal Tea",
            description = "Refreshing tea with medicinal herbs",
            foodType = FoodType.Drink,
            rarity = Rarity.Common,
            ingredients = new Dictionary<string, int> { { "green_herb", 2 }, { "hot_water", 1 } },
            cookingTime = 15,
            hungerRestored = 5,
            energyRestored = 25,
            statBonuses = new List<StatBonus> { new StatBonus { statName = "health_regen", bonusValue = 5, duration = 10 } }
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "drink_mana",
            recipeName = "Mana Potion",
            description = "Magical beverage that restores mana",
            foodType = FoodType.Drink,
            rarity = Rarity.Rare,
            ingredients = new Dictionary<string, int> { { "mana_herb", 3 }, { "crystal_water", 2 }, { "magic_dust", 1 } },
            cookingTime = 35,
            hungerRestored = 10,
            energyRestored = 50,
            statBonuses = new List<StatBonus> { new StatBonus { statName = "magic", bonusValue = 15, duration = 20 } },
            requiredCookingLevel = 3
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "drink_elixir",
            recipeName = "Elixir of Life",
            description = "Legendary elixir granting eternal vitality",
            foodType = FoodType.Drink,
            rarity = Rarity.Legendary,
            ingredients = new Dictionary<string, int> { { "elixir_herb", 3 }, { "phoenix_feather", 1 }, { "dragon_blood", 2 }, { "crystal_water", 2 } },
            cookingTime = 300,
            hungerRestored = 200,
            energyRestored = 200,
            statBonuses = new List<StatBonus> 
            { 
                new StatBonus { statName = "max_health", bonusValue = 100, duration = 0 },
                new StatBonus { statName = "health_regen", bonusValue = 20, duration = 120 },
                new StatBonus { statName = "all_stats", bonusValue = 10, duration = 60 }
            },
            requiredCookingLevel = 10
        });

        // Snacks (小吃)
        RegisterRecipe(new CookingRecipe
        {
            recipeId = "snack_jerky",
            recipeName = "Dried Meat Jerky",
            description = "Preserved meat for long journeys",
            foodType = FoodType.Snack,
            rarity = Rarity.Common,
            ingredients = new Dictionary<string, int> { { "meat", 3 }, { "salt", 1 } },
            cookingTime = 90,
            hungerRestored = 40,
            energyRestored = 15,
            statBonuses = new List<StatBonus>()
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "snack_seafood",
            recipeName = "Seafood Platter",
            description = "Assorted seafood snacks",
            foodType = FoodType.Snack,
            rarity = Rarity.Rare,
            ingredients = new Dictionary<string, int> { { "fish", 2 }, { "shrimp", 2 }, { "crab", 1 }, { "butter", 1 } },
            cookingTime = 40,
            hungerRestored = 35,
            energyRestored = 30,
            statBonuses = new List<StatBonus> 
            { 
                new StatBonus { statName = "speed", bonusValue = 8, duration = 20 },
                new StatBonus { statName = "attack", bonusValue = 5, duration = 20 }
            },
            requiredCookingLevel = 4
        });

        // Specialty (特色菜)
        RegisterRecipe(new CookingRecipe
        {
            recipeId = "special_wedding",
            recipeName = "Wedding Feast",
            description = "Grand feast for celebrations",
            foodType = FoodType.Specialty,
            rarity = Rarity.Epic,
            ingredients = new Dictionary<string, int> 
            { 
                { "meat", 5 }, { "fish", 3 }, { "vegetables", 4 }, 
                { "honey", 2 }, { "premium_broth", 2 }, { "rare_spice", 2 } 
            },
            cookingTime = 180,
            hungerRestored = 120,
            energyRestored = 80,
            statBonuses = new List<StatBonus> 
            { 
                new StatBonus { statName = "attack", bonusValue = 15, duration = 45 },
                new StatBonus { statName = "defense", bonusValue = 15, duration = 45 },
                new StatBonus { statName = "speed", bonusValue = 10, duration = 45 }
            },
            requiredCookingLevel = 6
        });

        RegisterRecipe(new CookingRecipe
        {
            recipeId = "special_banquet",
            recipeName = "Dragon Banquet",
            description = "Ultimate feast fit for a dragon lord",
            foodType = FoodType.Specialty,
            rarity = Rarity.Legendary,
            ingredients = new Dictionary<string, int> 
            { 
                { "dragon_meat", 3 }, { "phoenix_egg", 2 }, { "ginseng", 3 },
                { "golden_honey", 2 }, { "premium_broth", 3 }, { "legendary_spice", 2 },
                { "ice_crystal", 2 }, { "fire_herb", 2 }
            },
            cookingTime = 360,
            hungerRestored = 200,
            energyRestored = 150,
            statBonuses = new List<StatBonus> 
            { 
                new StatBonus { statName = "all_stats", bonusValue = 25, duration = 120 },
                new StatBonus { statName = "critical_rate", bonusValue = 15, duration = 120 },
                new StatBonus { statName = "critical_damage", bonusValue = 30, duration = 120 },
                new StatBonus { statName = "lifesteal", bonusValue = 10, duration = 120 }
            },
            requiredCookingLevel = 10
        });
    }

    private void RegisterRecipe(CookingRecipe recipe)
    {
        recipes[recipe.recipeId] = recipe;

        if (!recipesByType.ContainsKey(recipe.foodType))
            recipesByType[recipe.foodType] = new List<string>();
        recipesByType[recipe.foodType].Add(recipe.recipeId);

        if (!recipesByRarity.ContainsKey(recipe.rarity))
            recipesByRarity[recipe.rarity] = new List<string>();
        recipesByRarity[recipe.rarity].Add(recipe.recipeId);
    }

    public CookingRecipe GetRecipe(string recipeId)
    {
        return recipes.ContainsKey(recipeId) ? recipes[recipeId] : null;
    }

    public List<CookingRecipe> GetRecipesByType(FoodType type)
    {
        List<CookingRecipe> result = new List<CookingRecipe>();
        if (recipesByType.ContainsKey(type))
        {
            foreach (var id in recipesByType[type])
            {
                result.Add(recipes[id]);
            }
        }
        return result;
    }

    public List<CookingRecipe> GetRecipesByRarity(Rarity rarity)
    {
        List<CookingRecipe> result = new List<CookingRecipe>();
        if (recipesByRarity.ContainsKey(rarity))
        {
            foreach (var id in recipesByRarity[rarity])
            {
                result.Add(recipes[id]);
            }
        }
        return result;
    }

    public List<CookingRecipe> GetAllRecipes()
    {
        return new List<CookingRecipe>(recipes.Values);
    }

    public List<CookingRecipe> GetRecipesByLevel(int cookingLevel)
    {
        List<CookingRecipe> result = new List<CookingRecipe>();
        foreach (var recipe in recipes.Values)
        {
            if (recipe.requiredCookingLevel <= cookingLevel)
            {
                result.Add(recipe);
            }
        }
        return result;
    }
}
