using Godot;
using System;
using System.Collections.Generic;

public class PetBreedingSystem : Node
{
    private static PetBreedingSystem _instance;
    public static PetBreedingSystem Instance => _instance;

    private PetBreedingData _data;
    private PetBreedingDatabase _database;
    private Random _random = new Random();

    // Available pet types for breeding
    private List<string> _availablePetTypes = new List<string>
    {
        "FireDragon", "IceDragon", "ThunderBird", "Phoenix", "Wolf", "Fox", "Bear", "Turtle",
        "Owl", "Eagle", "Cat", "Tiger", "Slime", "Jelly", "Ghost", "Skeleton", "Fish", "Serpent",
        "Butterfly", "Beetle", "Unicorn", "Pegasus", "FireElemental", "WaterElemental", "IceElemental",
        "LightningElemental", "EarthElemental", "WindElemental", "LightElemental", "DarkElemental"
    };

    // Rarity names and colors
    private Dictionary<int, string> _rarityNames = new Dictionary<int, string>
    {
        { 1, "Common" },
        { 2, "Uncommon" },
        { 3, "Rare" },
        { 4, "Epic" },
        { 5, "Legendary" }
    };

    private Dictionary<int, Color> _rarityColors = new Dictionary<int, Color>
    {
        { 1, new Color(0.7f, 0.7f, 0.7f) },    // Gray
        { 2, new Color(0.2f, 0.8f, 0.2f) },    // Green
        { 3, new Color(0.2f, 0.5f, 1.0f) },     // Blue
        { 4, new Color(0.6f, 0.3f, 0.9f) },     // Purple
        { 5, new Color(1.0f, 0.7f, 0.0f) }      // Gold
    };

    public override void _Ready()
    {
        _instance = this;
        _database = new PetBreedingDatabase();
        _data = new PetBreedingData();
        LoadData();
    }

    public PetBreedingData GetData() => _data;
    public PetBreedingDatabase GetDatabase() => _database;
    public List<string> GetAvailablePetTypes() => _availablePetTypes;
    public Dictionary<int, string> GetRarityNames() => _rarityNames;
    public Dictionary<int, Color> GetRarityColors() => _rarityColors;

    /// <summary>
    /// Attempt to breed two pets
    /// </summary>
    public PetBreedingResult Breed(string petType1, string petType2)
    {
        _data.TotalBreeds++;

        // Create key for lookup
        string key1 = $"{petType1}_{petType2}";
        string key2 = $"{petType2}_{petType1}";

        PetBreedConfig config = null;
        if (_database.BreedConfigs.ContainsKey(key1))
            config = _database.BreedConfigs[key1];
        else if (_database.BreedConfigs.ContainsKey(key2))
            config = _database.BreedConfigs[key2];
        else if (petType1 == petType2 && _database.BreedConfigs.ContainsKey("Elemental_Elemental"))
            config = _database.BreedConfigs["Elemental_Elemental"];

        // If no config found, use generic breeding
        if (config == null)
        {
            return GenericBreed(petType1, petType2);
        }

        // Check success
        float successChance = config.BaseSuccessRate;
        if (_random.NextDouble() > successChance)
        {
            SaveData();
            return PetBreedResult.Failure;
        }

        // Determine rarity
        int rarity = RollRarity(config.RarityWeights);
        _data.SuccessfulBreeds++;

        if (rarity == 5)
            _data.LegendaryBreeds++;

        // Record breeding
        var record = new PetBreedingRecord
        {
            Parent1Id = petType1,
            Parent2Id = petType2,
            OffspringId = config.ResultType,
            OffspringType = config.ResultName,
            Rarity = rarity,
            BreedingTime = DateTime.Now,
            WasSuccessful = true
        };
        _data.BreedingHistory.Add(record);

        // Track offspring stats
        if (!_data.OffspringStats.ContainsKey(config.ResultType))
            _data.OffspringStats[config.ResultType] = 0;
        _data.OffspringStats[config.ResultType]++;

        // Unlock breed
        string unlockKey = $"{petType1}+{petType2}";
        _data.UnlockedBreeds[unlockKey] = true;

        SaveData();
        return (PetBreedResult)rarity;
    }

    private PetBreedResult GenericBreed(string petType1, string petType2)
    {
        // Generic breeding for non-defined combinations
        float successChance = 0.4f;
        if (_random.NextDouble() > successChance)
        {
            SaveData();
            return PetBreedResult.Failure;
        }

        _data.SuccessfulBreeds++;

        // Weighted random for rarity
        int rarity = RollRarity(new Dictionary<int, float>
        {
            { 1, 0.50f },
            { 2, 0.30f },
            { 3, 0.15f },
            { 4, 0.04f },
            { 5, 0.01f }
        });

        if (rarity == 5)
            _data.LegendaryBreeds++;

        string offspringType = $"{petType1}_{petType2}_Hybrid";

        var record = new PetBreedingRecord
        {
            Parent1Id = petType1,
            Parent2Id = petType2,
            OffspringId = offspringType,
            OffspringType = "Hybrid",
            Rarity = rarity,
            BreedingTime = DateTime.Now,
            WasSuccessful = true
        };
        _data.BreedingHistory.Add(record);

        string unlockKey = $"{petType1}+{petType2}";
        _data.UnlockedBreeds[unlockKey] = true;

        SaveData();
        return (PetBreedResult)rarity;
    }

    private int RollRarity(Dictionary<int, float> weights)
    {
        float total = weights.Values.Sum();
        float roll = (float)_random.NextDouble() * total;

        float cumulative = 0;
        foreach (var kvp in weights.OrderByDescending(x => x.Key))
        {
            cumulative += kvp.Value;
            if (roll <= cumulative)
                return kvp.Key;
        }

        return 1;
    }

    /// <summary>
    /// Check if a specific breed combination is available
    /// </summary>
    public bool IsBreedUnlocked(string petType1, string petType2)
    {
        string key = $"{petType1}+{petType2}";
        string keyReverse = $"{petType2}+{petType1}";
        return _data.UnlockedBreeds.ContainsKey(key) || _data.UnlockedBreeds.ContainsKey(keyReverse);
    }

    /// <summary>
    /// Get breeding history
    /// </summary>
    public List<PetBreedingRecord> GetBreedingHistory(int count = 20)
    {
        return _data.BreedingHistory.TakeLast(count).Reverse().ToList();
    }

    /// <summary>
    /// Get success rate
    /// </summary>
    public float GetSuccessRate()
    {
        if (_data.TotalBreeds == 0) return 0f;
        return (float)_data.SuccessfulBreeds / _data.TotalBreeds;
    }

    /// <summary>
    /// Get all unlocked breeds
    /// </summary>
    public Dictionary<string, bool> GetUnlockedBreeds()
    {
        return _data.UnlockedBreeds;
    }

    /// <summary>
    /// Clear breeding history
    /// </summary>
    public void ClearHistory()
    {
        _data.BreedingHistory.Clear();
        SaveData();
    }

    /// <summary>
    /// Get breed config for display
    /// </summary>
    public PetBreedConfig GetBreedConfig(string petType1, string petType2)
    {
        string key1 = $"{petType1}_{petType2}";
        string key2 = $"{petType2}_{petType1}";

        if (_database.BreedConfigs.ContainsKey(key1))
            return _database.BreedConfigs[key1];
        if (_database.BreedConfigs.ContainsKey(key2))
            return _database.BreedConfigs[key2];
        if (petType1 == petType2 && _database.BreedConfigs.ContainsKey("Elemental_Elemental"))
            return _database.BreedConfigs["Elemental_Elemental"];

        return null;
    }

    private void LoadData()
    {
        // TODO: Implement load from file
    }

    private void SaveData()
    {
        // TODO: Implement save to file
    }

    public void DebugReset()
    {
        _data = new PetBreedingData();
        SaveData();
    }
}
