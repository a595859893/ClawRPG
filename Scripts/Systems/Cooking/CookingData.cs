using Godot;
using System;
using System.Collections.Generic;

public class CookingRecipe : Resource
{
    [Export] public string recipeId;
    [Export] public string recipeName;
    [Export] public string description;
    [Export] public FoodType foodType;
    [Export] public Rarity rarity;
    [Export] public Dictionary<string, int> ingredients = new Dictionary<string, int>();
    [Export] public int cookingTime; // seconds
    [Export] public int hungerRestored;
    [Export] public int energyRestored;
    [Export] public List<StatBonus> statBonuses = new List<StatBonus>();
    [Export] public List<string> requiredTools = new List<string>();
    [Export] public int requiredCookingLevel;
}

public class StatBonus : Resource
{
    [Export] public string statName;
    [Export] public float bonusValue;
    [Export] public int duration; // minutes, 0 = permanent
}

public enum FoodType
{
    Soup,
    Salad,
    MainDish,
    Dessert,
    Drink,
    Snack,
    Specialty
}

public enum Rarity
{
    Common,
    Uncommon,
    Rare,
    Epic,
    Legendary
}

public class CookingData
{
    public Dictionary<string, int> knownRecipes = new Dictionary<string, int>();
    public int cookingLevel = 1;
    public int cookingExp = 0;
    public int totalDishesCooked = 0;
    public int successfulCooks = 0;
    public int failedCooks = 0;
    public Dictionary<string, int> cookedDishes = new Dictionary<string, int>();
    public List<string> favoriteRecipes = new List<string>();
}

public class ActiveCooking
{
    public string recipeId;
    public float progress;
    public float totalTime;
    public bool isComplete;
    public bool isBurning;
}
