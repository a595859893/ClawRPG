using Godot;
using System;
using System.Collections.Generic;

public class SeededRunSystem
{
    private static SeededRunSystem _instance;
    public static SeededRunSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new SeededRunSystem();
            return _instance;
        }
    }
    
    private SeededRunData _data;
    private Random _seededRandom;
    private string _currentSeed = "";
    private bool _isInitialized = false;
    
    // Statistics
    public int TotalSeededRuns => _data?.TotalSeededRuns ?? 0;
    public string CurrentSeed => _currentSeed;
    public bool IsSeededModeActive => _data?.IsSeededModeActive ?? false;
    
    public SeededRunSystem()
    {
        _data = new SeededRunData();
    }
    
    public void Initialize()
    {
        if (_isInitialized) return;
        
        LoadData();
        _isInitialized = true;
        GD.Print("[SeededRunSystem] Initialized - Total runs: " + TotalSeededRuns);
    }
    
    private void LoadData()
    {
        // Try to load from save file
        string savePath = "user://seeded_run_data.json";
        if (FileAccess.FileExists(savePath))
        {
            try
            {
                FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Read);
                string jsonString = file.GetAsText();
                file.Close();
                
                // Simple JSON parsing would go here
                // For now, we use default data
                GD.Print("[SeededRunSystem] Data loaded from save");
            }
            catch (Exception e)
            {
                GD.Print("[SeededRunSystem] Failed to load data: " + e.Message);
            }
        }
    }
    
    public void SaveData()
    {
        try
        {
            string savePath = "user://seeded_run_data.json";
            FileAccess file = FileAccess.Open(savePath, FileAccess.ModeFlags.Write);
            
            // Serialize data to JSON
            string jsonString = "{";
            jsonString += "\"TotalSeededRuns\":" + _data.TotalSeededRuns + ",";
            jsonString += "\"LastUsedSeed\":\"" + _data.LastUsedSeed + "\",";
            jsonString += "\"IsSeededModeActive\":" + (_data.IsSeededModeActive ? "true" : "false");
            jsonString += "}";
            
            file.StoreString(jsonString);
            file.Close();
            GD.Print("[SeededRunSystem] Data saved");
        }
        catch (Exception e)
        {
            GD.Print("[SeededRunSystem] Failed to save data: " + e.Message);
        }
    }
    
    public bool StartSeededRun(string seed)
    {
        if (!SeededRunDatabase.Instance.IsValidSeed(seed))
        {
            GD.Print("[SeededRunSystem] Invalid seed: " + seed);
            return false;
        }
        
        // Initialize the seeded random number generator
        int seedHash = GetSeedHash(seed);
        _seededRandom = new Random(seedHash);
        _currentSeed = seed;
        
        // Create or update seed record
        if (!_data.SeedHistory.ContainsKey(seed))
        {
            _data.SeedHistory[seed] = new SeededRunRecord(seed);
        }
        
        _data.LastUsedSeed = seed;
        _data.IsSeededModeActive = true;
        
        GD.Print("[SeededRunSystem] Started seeded run with seed: " + seed);
        return true;
    }
    
    public void EndSeededRun(int floor, int score, float time, int gold, int exp, int enemiesDefeated, int bossesDefeated, bool completed)
    {
        if (!_data.IsSeededModeActive || string.IsNullOrEmpty(_currentSeed))
        {
            GD.Print("[SeededRunSystem] No active seeded run to end");
            return;
        }
        
        var record = _data.SeedHistory[_currentSeed];
        record.RunCount++;
        record.LastPlayed = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
        
        // Update best records
        if (floor > record.BestFloor) record.BestFloor = floor;
        if (score > record.BestScore) record.BestScore = score;
        if (time > 0 && (record.BestTime == 0 || time < record.BestTime)) record.BestTime = time;
        
        record.TotalGold += gold;
        record.TotalExp += exp;
        record.EnemiesDefeated += enemiesDefeated;
        record.BossesDefeated += bossesDefeated;
        
        if (completed && !record.Completed)
        {
            record.Completed = true;
        }
        
        _data.TotalSeededRuns++;
        _data.IsSeededModeActive = false;
        
        // Save data
        SaveData();
        
        GD.Print("[SeededRunSystem] Ended seeded run - Floor: " + floor + ", Score: " + score + ", Seed: " + _currentSeed);
    }
    
    public void CancelSeededRun()
    {
        if (_data.IsSeededModeActive)
        {
            _data.IsSeededModeActive = false;
            GD.Print("[SeededRunSystem] Seeded run cancelled");
        }
    }
    
    // Seeded random number generation
    public int Next()
    {
        return _seededRandom?.Next() ?? 0;
    }
    
    public int Next(int maxValue)
    {
        return _seededRandom?.Next(maxValue) ?? 0;
    }
    
    public int Next(int minValue, int maxValue)
    {
        return _seededRandom?.Next(minValue, maxValue) ?? minValue;
    }
    
    public float NextFloat()
    {
        return (float)(_seededRandom?.NextDouble() ?? 0.0);
    }
    
    public bool NextBool()
    {
        return Next(2) == 1;
    }
    
    // Weighted random selection
    public string WeightedRandom(Dictionary<string, float> weights)
    {
        float total = 0;
        foreach (var weight in weights.Values)
        {
            total += weight;
        }
        
        float randomValue = (float)NextFloat() * total;
        float cumulative = 0;
        
        foreach (var item in weights)
        {
            cumulative += item.Value;
            if (randomValue <= cumulative)
            {
                return item.Key;
            }
        }
        
        // Fallback
        foreach (var item in weights)
        {
            return item.Key;
        }
        
        return "";
    }
    
    // Shuffle array with seed
    public void Shuffle<T>(T[] array)
    {
        int n = array.Length;
        for (int i = n - 1; i > 0; i--)
        {
            int j = Next(i + 1);
            T temp = array[i];
            array[i] = array[j];
            array[j] = temp;
        }
    }
    
    // Get seed hash for random initialization
    private int GetSeedHash(string seed)
    {
        int hash = 0;
        foreach (char c in seed)
        {
            hash = ((hash << 5) - hash) + c;
            hash = hash & hash; // Convert to unsigned
        }
        return Math.Abs(hash);
    }
    
    // Get statistics for a specific seed
    public SeededRunRecord GetSeedStatistics(string seed)
    {
        if (_data.SeedHistory.ContainsKey(seed))
        {
            return _data.SeedHistory[seed];
        }
        return null;
    }
    
    // Get all seed records
    public Dictionary<string, SeededRunRecord> GetAllSeedRecords()
    {
        return _data.SeedHistory;
    }
    
    // Generate a new random seed
    public string GenerateNewSeed()
    {
        return SeededRunDatabase.Instance.GenerateRandomSeed();
    }
    
    // Get preset configuration
    public SeedPreset GetPreset(string presetId)
    {
        return SeededRunDatabase.Instance.GetPreset(presetId);
    }
    
    // Verify seed completion status
    public bool IsSeedCompleted(string seed)
    {
        if (_data.SeedHistory.ContainsKey(seed))
        {
            return _data.SeedHistory[seed].Completed;
        }
        return false;
    }
    
    // Get completion rate
    public float GetCompletionRate()
    {
        if (_data.SeedHistory.Count == 0) return 0f;
        
        int completed = 0;
        foreach (var record in _data.SeedHistory.Values)
        {
            if (record.Completed) completed++;
        }
        
        return (float)completed / _data.SeedHistory.Count;
    }
}
