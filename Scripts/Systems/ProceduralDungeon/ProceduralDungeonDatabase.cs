using Godot;
using System;
using System.Collections.Generic;

public class ProceduralDungeonDatabase
{
    private static ProceduralDungeonDatabase instance;
    public static ProceduralDungeonDatabase Instance
    {
        get
        {
            if (instance == null) instance = new ProceduralDungeonDatabase();
            return instance;
        }
    }
    
    // Room type weights by difficulty
    public Dictionary<DungeonDifficulty, Dictionary<RoomType, float>> RoomTypeWeights = new Dictionary<DungeonDifficulty, Dictionary<RoomType, float>>();
    
    // Shape characteristics
    public Dictionary<DungeonShape, int> ShapeMinRooms = new Dictionary<DungeonShape, int>();
    public Dictionary<DungeonShape, int> ShapeMaxRooms = new Dictionary<DungeonShape, int>();
    
    // Difficulty modifiers
    public Dictionary<DungeonDifficulty, float> DifficultyEnemyMultiplier = new Dictionary<DungeonDifficulty, float>();
    public Dictionary<DungeonDifficulty, float> DifficultyRewardMultiplier = new Dictionary<DungeonDifficulty, float>();
    
    // Room templates
    public Dictionary<RoomType, Dictionary<string, object>> RoomTemplates = new Dictionary<RoomType, Dictionary<string, object>>();
    
    public ProceduralDungeonDatabase()
    {
        InitializeRoomWeights();
        InitializeShapeConfig();
        InitializeDifficultyModifiers();
        InitializeRoomTemplates();
    }
    
    private void InitializeRoomWeights()
    {
        // Easy difficulty
        RoomTypeWeights[DungeonDifficulty.Easy] = new Dictionary<RoomType, float>
        {
            { RoomType.Combat, 0.35f },
            { RoomType.Treasure, 0.20f },
            { RoomType.Rest, 0.15f },
            { RoomType.Shop, 0.10f },
            { RoomType.Event, 0.10f },
            { RoomType.Empty, 0.10f }
        };
        
        // Normal difficulty
        RoomTypeWeights[DungeonDifficulty.Normal] = new Dictionary<RoomType, float>
        {
            { RoomType.Combat, 0.40f },
            { RoomType.Treasure, 0.15f },
            { RoomType.Rest, 0.12f },
            { RoomType.Shop, 0.08f },
            { RoomType.Event, 0.10f },
            { RoomType.Trap, 0.08f },
            { RoomType.Puzzle, 0.07f }
        };
        
        // Hard difficulty
        RoomTypeWeights[DungeonDifficulty.Hard] = new Dictionary<RoomType, float>
        {
            { RoomType.Combat, 0.45f },
            { RoomType.MiniBoss, 0.08f },
            { RoomType.Treasure, 0.12f },
            { RoomType.Rest, 0.08f },
            { RoomType.Shop, 0.05f },
            { RoomType.Event, 0.08f },
            { RoomType.Trap, 0.07f },
            { RoomType.Puzzle, 0.07f }
        };
        
        // Nightmare difficulty
        RoomTypeWeights[DungeonDifficulty.Nightmare] = new Dictionary<RoomType, float>
        {
            { RoomType.Combat, 0.40f },
            { RoomType.MiniBoss, 0.12f },
            { RoomType.Treasure, 0.10f },
            { RoomType.Rest, 0.05f },
            { RoomType.Event, 0.10f },
            { RoomType.Trap, 0.10f },
            { RoomType.Puzzle, 0.08f },
            { RoomType.Secret, 0.05f }
        };
        
        // Legendary difficulty
        RoomTypeWeights[DungeonDifficulty.Legendary] = new Dictionary<RoomType, float>
        {
            { RoomType.Combat, 0.35f },
            { RoomType.MiniBoss, 0.15f },
            { RoomType.Treasure, 0.08f },
            { RoomType.Event, 0.12f },
            { RoomType.Trap, 0.12f },
            { RoomType.Puzzle, 0.10f },
            { RoomType.Secret, 0.08f }
        };
    }
    
    private void InitializeShapeConfig()
    {
        ShapeMinRooms[DungeonShape.Linear] = 5;
        ShapeMaxRooms[DungeonShape.Linear] = 8;
        
        ShapeMinRooms[DungeonShape.Branching] = 8;
        ShapeMaxRooms[DungeonShape.Branching] = 15;
        
        ShapeMinRooms[DungeonShape.Circular] = 10;
        ShapeMaxRooms[DungeonShape.Circular] = 18;
        
        ShapeMinRooms[DungeonShape.HubAndSpoke] = 12;
        ShapeMaxRooms[DungeonShape.HubAndSpoke] = 20;
        
        ShapeMinRooms[DungeonShape.Maze] = 15;
        ShapeMaxRooms[DungeonShape.Maze] = 25;
    }
    
    private void InitializeDifficultyModifiers()
    {
        DifficultyEnemyMultiplier[DungeonDifficulty.Easy] = 0.8f;
        DifficultyEnemyMultiplier[DungeonDifficulty.Normal] = 1.0f;
        DifficultyEnemyMultiplier[DungeonDifficulty.Hard] = 1.5f;
        DifficultyEnemyMultiplier[DungeonDifficulty.Nightmare] = 2.0f;
        DifficultyEnemyMultiplier[DungeonDifficulty.Legendary] = 3.0f;
        
        DifficultyRewardMultiplier[DungeonDifficulty.Easy] = 0.8f;
        DifficultyRewardMultiplier[DungeonDifficulty.Normal] = 1.0f;
        DifficultyRewardMultiplier[DungeonDifficulty.Hard] = 1.5f;
        DifficultyRewardMultiplier[DungeonDifficulty.Nightmare] = 2.5f;
        DifficultyRewardMultiplier[DungeonDifficulty.Legendary] = 4.0f;
    }
    
    private void InitializeRoomTemplates()
    {
        // Combat room templates
        RoomTemplates[RoomType.Combat] = new Dictionary<string, object>
        {
            { "enemyCount", new int[] { 3, 5, 8 } },
            { "enemyTypes", new string[] { "Slime", "Goblin", "Skeleton" } },
            { "hasElite", false }
        };
        
        // Treasure room templates
        RoomTemplates[RoomType.Treasure] = new Dictionary<string, object>
        {
            { "chestCount", new int[] { 1, 2, 3 } },
            { "guarded", true },
            { "trapChance", 0.3f }
        };
        
        // Boss room templates
        RoomTemplates[RoomType.Boss] = new Dictionary<string, object>
        {
            { "bossType", "Dragon" },
            { "minionCount", 3 },
            { "phaseCount", 3 }
        };
        
        // Shop room templates
        RoomTemplates[RoomType.Shop] = new Dictionary<string, object>
        {
            { "itemCount", 6 },
            { "restockAvailable", true },
            { "discount", 0.9f }
        };
        
        // Rest room templates
        RoomTemplates[RoomType.Rest] = new Dictionary<string, object>
        {
            { "healAmount", 0.5f },
            { "canSave", true },
            { "hasMerchant", false }
        };
    }
    
    public RoomType GetRandomRoomType(DungeonDifficulty difficulty)
    {
        var weights = RoomTypeWeights[difficulty];
        float totalWeight = 0;
        foreach (var w in weights) totalWeight += w.Value;
        
        float randomValue = (float)GD.RandD() * totalWeight;
        float cumulative = 0;
        
        foreach (var kvp in weights)
        {
            cumulative += kvp.Value;
            if (randomValue <= cumulative)
                return kvp.Key;
        }
        
        return RoomType.Combat;
    }
    
    public int GetRoomCountForShape(DungeonShape shape)
    {
        int min = ShapeMinRooms[shape];
        int max = ShapeMaxRooms[shape];
        return GD.RandI() % (max - min + 1) + min;
    }
}
