using Godot;
using System;
using System.Collections.Generic;

public class SeededRunDatabase
{
    private static SeededRunDatabase _instance;
    public static SeededRunDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new SeededRunDatabase();
            return _instance;
        }
    }
    
    // Seed preset configurations
    public Dictionary<string, SeedPreset> SeedPresets { get; private set; } = new Dictionary<string, SeedPreset>();
    
    // Random number generator for seed validation
    private Random _random = new Random();
    
    public SeededRunDatabase()
    {
        InitializeSeedPresets();
    }
    
    private void InitializeSeedPresets()
    {
        // Tutorial seed - designed for new players
        SeedPresets["tutorial_001"] = new SeedPreset
        {
            Id = "tutorial_001",
            Name = "Tutorial Run",
            Description = "A beginner-friendly seed with easier enemies",
            Difficulty = "Easy",
            RecommendedLevel = 1,
            SpecialRules = new List<string> { "Reduced enemy damage", "More healing items", "Tutorial hints" },
            FloorMultiplier = 0.5f,
            EnemyDamageMultiplier = 0.7f,
            DropRateMultiplier = 1.5f,
            GoldMultiplier = 1.2f
        };
        
        // Challenge seeds
        SeedPresets["challenge_speedrun"] = new SeedPreset
        {
            Id = "challenge_speedrun",
            Name = "Speed Run Challenge",
            Description = "Complete the run as fast as possible",
            Difficulty = "Normal",
            RecommendedLevel = 10,
            SpecialRules = new List<string> { "Timer starts immediately", "No pausing", "Bosses spawn earlier" },
            FloorMultiplier = 1.2f,
            EnemyDamageMultiplier = 1.0f,
            DropRateMultiplier = 0.8f,
            GoldMultiplier = 1.0f
        };
        
        SeedPresets["challenge_no_heal"] = new SeedPreset
        {
            Id = "challenge_no_heal",
            Name = "No Healing Challenge",
            Description = "No natural healing allowed",
            Difficulty = "Hard",
            RecommendedLevel = 20,
            SpecialRules = new List<string> { "No passive healing", "No healing items in shops", "Potions are 50% effective" },
            FloorMultiplier = 1.0f,
            EnemyDamageMultiplier = 1.3f,
            DropRateMultiplier = 1.2f,
            GoldMultiplier = 1.5f
        };
        
        SeedPresets["challenge_ironman"] = new SeedPreset
        {
            Id = "challenge_ironman",
            Name = "Ironman Mode",
            Description = "No saves, no retries, one life",
            Difficulty = "Nightmare",
            RecommendedLevel = 30,
            SpecialRules = new List<string> { "No save points", "No retreat", "Permadeath" },
            FloorMultiplier = 1.5f,
            EnemyDamageMultiplier = 1.5f,
            DropRateMultiplier = 2.0f,
            GoldMultiplier = 2.0f
        };
        
        // Special seeds
        SeedPresets["special_lucky"] = new SeedPreset
        {
            Id = "special_lucky",
            Name = "Lucky Seed",
            Description = "Increased drop rates and rare item chances",
            Difficulty = "Easy",
            RecommendedLevel = 1,
            SpecialRules = new List<string> { "2x drop rate", "Rare items more common", "Golden chests guaranteed" },
            FloorMultiplier = 1.0f,
            EnemyDamageMultiplier = 0.8f,
            DropRateMultiplier = 2.0f,
            GoldMultiplier = 1.5f
        };
        
        SeedPresets["special_endless"] = new SeedPreset
        {
            Id = "special_endless",
            Name = "Endless Mode",
            Description = "No floor limit, infinite scaling",
            Difficulty = "Legendary",
            RecommendedLevel = 40,
            SpecialRules = new List<string> { "No floor cap", "Infinite scaling", "Bosses every 5 floors" },
            FloorMultiplier = 999f,
            EnemyDamageMultiplier = 2.0f,
            DropRateMultiplier = 3.0f,
            GoldMultiplier = 3.0f
        };
        
        // Competitive seeds
        SeedPresets["competitive_ranked"] = new SeedPreset
        {
            Id = "competitive_ranked",
            Name = "Ranked Seed",
            Description = "Official ranked competitive seed",
            Difficulty = "Normal",
            RecommendedLevel = 15,
            SpecialRules = new List<string> { "Leaderboard tracked", "Standardized rewards", "Fair competitive format" },
            FloorMultiplier = 1.0f,
            EnemyDamageMultiplier = 1.0f,
            DropRateMultiplier = 1.0f,
            GoldMultiplier = 1.0f
        };
        
        SeedPresets["competitive_tournament"] = new SeedPreset
        {
            Id = "competitive_tournament",
            Name = "Tournament Seed",
            Description = "For tournament play only",
            Difficulty = "Hard",
            RecommendedLevel = 25,
            SpecialRules = new List<string> { "Fixed time limit", "No custom builds", "Standardized starting items" },
            FloorMultiplier = 1.0f,
            EnemyDamageMultiplier = 1.2f,
            DropRateMultiplier = 1.0f,
            GoldMultiplier = 1.0f
        };
    }
    
    public string GenerateRandomSeed()
    {
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
        char[] seedChars = new char[8];
        for (int i = 0; i < 8; i++)
        {
            seedChars[i] = chars[_random.Next(chars.Length)];
        }
        return new string(seedChars);
    }
    
    public bool IsValidSeed(string seed)
    {
        if (string.IsNullOrEmpty(seed)) return false;
        if (seed.Length < 4 || seed.Length > 16) return false;
        
        foreach (char c in seed)
        {
            if (!char.IsLetterOrDigit(c)) return false;
        }
        return true;
    }
    
    public SeedPreset GetPreset(string presetId)
    {
        if (SeedPresets.ContainsKey(presetId))
            return SeedPresets[presetId];
        return null;
    }
}

public class SeedPreset
{
    public string Id { get; set; } = "";
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Difficulty { get; set; } = "Normal";
    public int RecommendedLevel { get; set; } = 1;
    public List<string> SpecialRules { get; set; } = new List<string>();
    public float FloorMultiplier { get; set; } = 1.0f;
    public float EnemyDamageMultiplier { get; set; } = 1.0f;
    public float DropRateMultiplier { get; set; } = 1.0f;
    public float GoldMultiplier { get; set; } = 1.0f;
}
