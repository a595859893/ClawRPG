using Godot;
using System;
using System.Collections.Generic;

public class PetBreedingSystem : BaseSystem
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
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) return;

        // Load unlocked breeds
        if (data.Contains("pet_breeding_unlocked"))
        {
            var unlockedArray = (Godot.Array)data["pet_breeding_unlocked"];
            foreach (string key in unlockedArray)
            {
                _data.UnlockedBreeds[key] = true;
            }
        }

        // Load breeding history
        if (data.Contains("pet_breeding_history"))
        {
            var historyArray = (Godot.Array)data["pet_breeding_history"];
            foreach (Dictionary historyData in historyArray)
            {
                var record = new PetBreedingRecord
                {
                    Parent1Id = (string)historyData["parent1"],
                    Parent2Id = (string)historyData["parent2"],
                    OffspringId = (string)historyData["offspring_id"],
                    OffspringType = (string)historyData["offspring_type"],
                    Rarity = (int)historyData["rarity"],
                    BreedingTime = DateTime.Parse((string)historyData["time"]),
                    WasSuccessful = (bool)historyData["success"]
                };
                _data.BreedingHistory.Add(record);
            }
        }

        // Load stats
        if (data.Contains("pet_breeding_stats"))
        {
            var stats = (Godot.Dictionary)data["pet_breeding_stats"];
            _data.TotalBreeds = (int)stats.Get("total_breeds", 0);
            _data.SuccessfulBreeds = (int)stats.Get("successful_breeds", 0);
            _data.LegendaryBreeds = (int)stats.Get("legendary_breeds", 0);
        }

        // Load offspring stats
        if (data.Contains("pet_breeding_offspring"))
        {
            var offspringData = (Godot.Dictionary)data["pet_breeding_offspring"];
            foreach (string key in offspringData.Keys)
            {
                _data.OffspringStats[key] = (int)offspringData[key];
            }
        }
    }

    private void SaveData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/SaveSystem");
        if (saveSystem == null) return;

        var data = saveSystem.LoadGame();
        if (data == null) data = new Godot.Dictionary();

        // Save unlocked breeds
        var unlockedArray = new Godot.Array();
        foreach (var key in _data.UnlockedBreeds.Keys)
        {
            unlockedArray.Add(key);
        }
        data["pet_breeding_unlocked"] = unlockedArray;

        // Save breeding history (limit to last 50)
        var historyArray = new Godot.Array();
        var recentHistory = _data.BreedingHistory.TakeLast(50).ToList();
        foreach (var record in recentHistory)
        {
            var historyData = new Godot.Dictionary();
            historyData["parent1"] = record.Parent1Id;
            historyData["parent2"] = record.Parent2Id;
            historyData["offspring_id"] = record.OffspringId;
            historyData["offspring_type"] = record.OffspringType;
            historyData["rarity"] = record.Rarity;
            historyData["time"] = record.BreedingTime.ToString("o");
            historyData["success"] = record.WasSuccessful;
            historyArray.Add(historyData);
        }
        data["pet_breeding_history"] = historyArray;

        // Save stats
        var stats = new Godot.Dictionary();
        stats["total_breeds"] = _data.TotalBreeds;
        stats["successful_breeds"] = _data.SuccessfulBreeds;
        stats["legendary_breeds"] = _data.LegendaryBreeds;
        data["pet_breeding_stats"] = stats;

        // Save offspring stats
        var offspringData = new Godot.Dictionary();
        foreach (var kvp in _data.OffspringStats)
        {
            offspringData[kvp.Key] = kvp.Value;
        }
        data["pet_breeding_offspring"] = offspringData;

        saveSystem.SaveGame(data);
    }

    public void DebugReset()
    {
        _data = new PetBreedingData();
        SaveData();
    }

    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        if (_data != null)
        {
            var dataData = _data.ExportSaveData();
            foreach (var kvp in dataData)
            {
                data[kvp.Key] = kvp.Value;
            }
        }
        
        return data;
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null || _data == null) return;
        _data.ImportSaveData(data);
    }
}
