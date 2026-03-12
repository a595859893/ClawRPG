using Godot;
using System;
using System.Collections.Generic;

public class MarketTrendDatabase
{
    // Item categories to track
    public static readonly string[] ItemCategories = new string[]
    {
        "Weapon",
        "Armor",
        "Accessory",
        "Consumable",
        "Material",
        "Pet",
        "Mount",
        "Artifact",
        "Rune",
        "Enchantment"
    };
    
    // Base prices for each category
    public static Dictionary<string, float> BasePrices = new Dictionary<string, float>
    {
        { "Weapon", 500f },
        { "Armor", 400f },
        { "Accessory", 300f },
        { "Consumable", 50f },
        { "Material", 25f },
        { "Pet", 1000f },
        { "Mount", 2000f },
        { "Artifact", 1500f },
        { "Rune", 200f },
        { "Enchantment", 350f }
    };
    
    // Price volatility by category
    public static Dictionary<string, float> Volatility = new Dictionary<string, float>
    {
        { "Weapon", 0.15f },
        { "Armor", 0.12f },
        { "Accessory", 0.18f },
        { "Consumable", 0.25f },
        { "Material", 0.30f },
        { "Pet", 0.20f },
        { "Mount", 0.10f },
        { "Artifact", 0.22f },
        { "Rune", 0.28f },
        { "Enchantment", 0.16f }
    };
    
    // Trend change factors
    public static float[] TrendChangeFactors = new float[] { -0.15f, -0.10f, -0.05f, 0f, 0.05f, 0.10f, 0.15f };
    
    // Market events that affect trends
    public static Dictionary<string, string[]> CategoryEvents = new Dictionary<string, string[]>
    {
        { "Weapon", new string[] { "War", "Peace", "MonsterOutbreak" } },
        { "Armor", new string[] { "War", "ColdWave", "Peace" } },
        { "Consumable", new string[] { "Plague", "Festival", "Normal" } },
        { "Material", new string[] { "Drought", "Boom", "Normal" } },
        { "Pet", new string[] { "PetEvent", "Normal" } }
    };
    
    // Get random base price with some variance
    public static float GetBasePrice(string category)
    {
        if (!BasePrices.ContainsKey(category))
            return 100f;
        
        float basePrice = BasePrices[category];
        float variance = (float)GD.RandRange(0.8, 1.2);
        return basePrice * variance;
    }
    
    // Get volatility for category
    public static float GetVolatility(string category)
    {
        if (!Volatility.ContainsKey(category))
            return 0.2f;
        return Volatility[category];
    }
    
    // Calculate price change based on trend
    public static float CalculatePriceChange(string category, TrendDirection direction, float trendStrength)
    {
        float baseChange = 0f;
        float volatility = GetVolatility(category);
        
        switch (direction)
        {
            case TrendDirection.Rising:
                baseChange = volatility * (trendStrength / 100f);
                break;
            case TrendDirection.Falling:
                baseChange = -volatility * (trendStrength / 100f);
                break;
            case TrendDirection.Volatile:
                baseChange = (float)GD.RandRange(-volatility, volatility);
                break;
            case TrendDirection.Stable:
            default:
                baseChange = (float)GD.RandRange(-0.02f, 0.02f);
                break;
        }
        
        return baseChange;
    }
}
