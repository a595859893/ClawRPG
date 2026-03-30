using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 种子跑系统 - 管理种子跑模式的数据和统计
/// </summary>
public class SeededRunSystem : BaseSystem
{
    private static SeededRunSystem _instance;
    public static SeededRunSystem Instance
    {
        get
        {
            if (_instance == null) _instance = GetTree().Root.GetNode<SeededRunSystem>("SeededRunSystem");
            return _instance;
        }
    }
    
    private SeededRunData _data;
    private Random _seededRandom;
    private string _currentSeed = "";
    
    // Statistics
    public int TotalSeededRuns => _data?.TotalSeededRuns ?? 0;
    public string CurrentSeed => _currentSeed;
    public bool IsSeededModeActive => _data?.IsSeededModeActive ?? false;
    
    public override void _Ready()
    {
        base._Ready();
        // 确保节点在树中
        if (GetParent() == null)
        {
            GD.PrintErr("[SeededRunSystem] Warning: Not added to scene tree!");
        }
    }
    
    protected override void Initialize()
    {
        _data = new SeededRunData();
        _isInitialized = true;
        GD.Print("[SeededRunSystem] Initialized - Total runs: " + TotalSeededRuns);
    }
    
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();
        
        data["total_seeded_runs"] = _data.TotalSeededRuns;
        data["last_used_seed"] = _data.LastUsedSeed ?? "";
        data["is_seeded_mode_active"] = _data.IsSeededModeActive;
        
        // 序列化种子历史
        var seedHistory = new Array();
        if (_data.SeedHistory != null)
        {
            foreach (var kvp in _data.SeedHistory)
            {
                var record = new Dictionary<string, object>();
                record["seed"] = kvp.Key;
                record["run_count"] = kvp.Value.RunCount;
                record["best_floor"] = kvp.Value.BestFloor;
                record["best_score"] = kvp.Value.BestScore;
                record["best_time"] = kvp.Value.BestTime;
                record["total_gold"] = kvp.Value.TotalGold;
                record["total_exp"] = kvp.Value.TotalExp;
                record["enemies_defeated"] = kvp.Value.EnemiesDefeated;
                record["bosses_defeated"] = kvp.Value.BossesDefeated;
                record["completed"] = kvp.Value.Completed;
                record["last_played"] = kvp.Value.LastPlayed ?? "";
                seedHistory.Add(record);
            }
        }
        data["seed_history"] = seedHistory;
        
        return data;
    }
    
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null)
        {
            GD.Print("[SeededRunSystem] No save data to import");
            return;
        }
        
        try
        {
            _data = new SeededRunData();
            
            if (data.Contains("total_seeded_runs"))
                _data.TotalSeededRuns = Convert.ToInt32(data["total_seeded_runs"]);
            if (data.Contains("last_used_seed"))
                _data.LastUsedSeed = data["last_used_seed"]?.ToString() ?? "";
            if (data.Contains("is_seeded_mode_active"))
                _data.IsSeededModeActive = Convert.ToBoolean(data["is_seeded_mode_active"]);
            
            // 反序列化种子历史
            if (data.Contains("seed_history"))
            {
                var seedHistory = data["seed_history"] as Array;
                if (seedHistory != null)
                {
                    _data.SeedHistory = new Dictionary<string, SeededRunRecord>();
                    foreach (Dictionary record in seedHistory)
                    {
                        string seed = record["seed"]?.ToString() ?? "";
                        if (!string.IsNullOrEmpty(seed))
                        {
                            var runRecord = new SeededRunRecord(seed);
                            if (record.Contains("run_count"))
                                runRecord.RunCount = Convert.ToInt32(record["run_count"]);
                            if (record.Contains("best_floor"))
                                runRecord.BestFloor = Convert.ToInt32(record["best_floor"]);
                            if (record.Contains("best_score"))
                                runRecord.BestScore = Convert.ToInt32(record["best_score"]);
                            if (record.Contains("best_time"))
                                runRecord.BestTime = Convert.ToSingle(record["best_time"]);
                            if (record.Contains("total_gold"))
                                runRecord.TotalGold = Convert.ToInt32(record["total_gold"]);
                            if (record.Contains("total_exp"))
                                runRecord.TotalExp = Convert.ToInt32(record["total_exp"]);
                            if (record.Contains("enemies_defeated"))
                                runRecord.EnemiesDefeated = Convert.ToInt32(record["enemies_defeated"]);
                            if (record.Contains("bosses_defeated"))
                                runRecord.BossesDefeated = Convert.ToInt32(record["bosses_defeated"]);
                            if (record.Contains("completed"))
                                runRecord.Completed = Convert.ToBoolean(record["completed"]);
                            if (record.Contains("last_played"))
                                runRecord.LastPlayed = record["last_played"]?.ToString() ?? "";
                            
                            _data.SeedHistory[seed] = runRecord;
                        }
                    }
                }
            }
            
            GD.Print("[SeededRunSystem] Data imported - Total runs: " + _data.TotalSeededRuns);
        }
        catch (Exception e)
        {
            GD.PrintErr("[SeededRunSystem] Failed to import save data: " + e.Message);
            _data = new SeededRunData();
        }
    }
    
    public bool StartSeededRun(string seed)
    {
        if (_data == null) _data = new SeededRunData();
        
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
        if (_data == null) _data = new SeededRunData();
        
        if (!_data.IsSeededModeActive || string.IsNullOrEmpty(_currentSeed))
        {
            GD.Print("[SeededRunSystem] No active seeded run to end");
            return;
        }
        
        if (!_data.SeedHistory.ContainsKey(_currentSeed))
        {
            _data.SeedHistory[_currentSeed] = new SeededRunRecord(_currentSeed);
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
        
        GD.Print("[SeededRunSystem] Ended seeded run - Floor: " + floor + ", Score: " + score + ", Seed: " + _currentSeed);
    }
    
    public void CancelSeededRun()
    {
        if (_data == null) _data = new SeededRunData();
        
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
        if (_data?.SeedHistory?.ContainsKey(seed) == true)
        {
            return _data.SeedHistory[seed];
        }
        return null;
    }
    
    // Get all seed records
    public Dictionary<string, SeededRunRecord> GetAllSeedRecords()
    {
        return _data?.SeedHistory ?? new Dictionary<string, SeededRunRecord>();
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
        if (_data?.SeedHistory?.ContainsKey(seed) == true)
        {
            return _data.SeedHistory[seed].Completed;
        }
        return false;
    }
    
    // Get completion rate
    public float GetCompletionRate()
    {
        if (_data?.SeedHistory == null || _data.SeedHistory.Count == 0) return 0f;
        
        int completed = 0;
        foreach (var record in _data.SeedHistory.Values)
        {
            if (record.Completed) completed++;
        }
        
        return (float)completed / _data.SeedHistory.Count;
    }
}
