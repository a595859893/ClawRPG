using Godot;
using Godot.Collections;
using System;
using System.Collections.Generic;

public partial class RandomDungeonEventDatabase : BaseSystem
{
    public static RandomDungeonEventDatabase Instance { get; private set; }

    protected override void Initialize()
    {
        Instance = this;
        IsInitialized = true;
        GD.Print($"[RandomDungeonEventDatabase] Initialized");
    }

    public override Dictionary ExportSaveData()
    {
        return new Dictionary(); // Configuration only, no runtime state to persist
    }

    public override void ImportSaveData(Dictionary data)
    {
        // No runtime state to restore
    }
    // Event rarity weights by floor
    public Dictionary<string, float> GetRarityWeights(int floor)
    {
        var weights = new Dictionary<string, float>
        {
            { "Common", 50f },
            { "Uncommon", 30f },
            { "Rare", 15f },
            { "Epic", 4f },
            { "Legendary", 1f }
        };
        
        // Scale weights based on floor
        if (floor >= 5)
        {
            weights["Rare"] += 3f;
            weights["Epic"] += 1f;
        }
        if (floor >= 10)
        {
            weights["Epic"] += 2f;
            weights["Legendary"] += 1f;
        }
        if (floor >= 20)
        {
            weights["Legendary"] += 2f;
        }
        
        return weights;
    }
    
    // Category weights for random selection
    public Dictionary<string, float> GetCategoryWeights()
    {
        return new Dictionary<string, float>
        {
            { "Treasure", 20f },
            { "Combat", 15f },
            { "Exploration", 13f },
            { "Trap", 10f },
            { "Hazard", 10f },
            { "Mystery", 10f },
            { "Blessing", 7f },
            { "Curse", 5f },
            { "NPC", 5f },
            { "Reward", 5f }
        };
    }
    
    // Event cooldown periods (seconds)
    public Dictionary<string, float> GetEventCooldowns()
    {
        return new Dictionary<string, float>
        {
            { "ambush", 60f },
            { "surprise_attack", 120f },
            { "reinforcements", 300f },
            { "hidden_chest", 30f },
            { "treasure_room", 600f },
            { "empty_chest", 15f },
            { "healing_fountain", 180f },
            { "blessing_of_light", 300f },
            { "cursed_trap", 90f },
            { "wandering_merchant", 600f }
        };
    }
    
    // Get event color by category
    public Color GetCategoryColor(string category)
    {
        return category switch
        {
            "Combat" => new Color(1f, 0.3f, 0.3f),
            "Treasure" => new Color(1f, 0.8f, 0.2f),
            "Blessing" => new Color(0.3f, 1f, 0.3f),
            "Curse" => new Color(0.6f, 0.2f, 0.8f),
            "Hazard" => new Color(1f, 0.5f, 0.2f),
            "Trap" => new Color(0.8f, 0.3f, 0.3f),
            "Mystery" => new Color(0.6f, 0.4f, 1f),
            "NPC" => new Color(0.4f, 0.8f, 1f),
            "Exploration" => new Color(0.5f, 1f, 0.5f),
            "Reward" => new Color(1f, 0.9f, 0.3f),
            _ => new Color(0.8f, 0.8f, 0.8f)
        };
    }
    
    // Get event rarity color
    public Color GetRarityColor(string rarity)
    {
        return rarity switch
        {
            "Common" => new Color(0.7f, 0.7f, 0.7f),
            "Uncommon" => new Color(0.3f, 0.9f, 0.3f),
            "Rare" => new Color(0.3f, 0.6f, 1f),
            "Epic" => new Color(0.7f, 0.3f, 0.9f),
            "Legendary" => new Color(1f, 0.7f, 0.2f),
            _ => new Color(0.8f, 0.8f, 0.8f)
        };
    }
    
    // Initialize event database
    public Dictionary<string, Dictionary> InitializeEventDatabase()
    {
        var database = new Dictionary<string, Dictionary>();
        
        // Add all events to database
        // This mirrors the event database in RandomDungeonEventSystem
        
        return database;
    }
}
