using Godot;
using System;
using System.Collections.Generic;

public class GuildTowerDefenseSystem : BaseSystem
{
    // Tower types
    public enum TowerType { Arrow, Cannon, Ice, Fire, Lightning, Poison, Support, Ultimate }
    
    // Wave state
    public enum WaveState { Preparing, InProgress, Completed, Failed }
    
    // Tower data
    public class Tower
    {
        public TowerType Type;
        public int Level = 1;
        public Vector2 Position;
        public int Damage = 10;
        public float AttackSpeed = 1.0f;
        public float Range = 150f;
        public int Cost = 100;
    }
    
    // Wave data
    public class Wave
    {
        public int WaveNumber;
        public int EnemyCount;
        public float EnemyHealthScale = 1.0f;
        public int RewardGold = 100;
        public int RewardPoints = 10;
    }
    
    // Game state
    private WaveState _waveState = WaveState.Preparing;
    private int _currentWave = 0;
    private int _guildId = -1;
    private int _totalGold = 0;
    private int _totalPoints = 0;
    private int _enemiesDefeated = 0;
    private int _towersBuilt = 0;
    private int _lives = 20;
    private List<Tower> _towers = new List<Tower>();
    private List<Wave> _waves = new List<Wave>();
    
    // Tower database
    private Dictionary<TowerType, Dictionary<int, Dictionary<string, int>> > _towerDatabase;
    
    public override void _Ready()
    {
        InitializeTowerDatabase();
        InitializeWaves();
    }
    
    private void InitializeTowerDatabase()
    {
        _towerDatabase = new Dictionary<TowerType, Dictionary<int, Dictionary<string, int>>>();
        
        // Arrow Tower
        _towerDatabase[TowerType.Arrow] = new Dictionary<int, Dictionary<string, int>>();
        _towerDatabase[TowerType.Arrow][1] = new Dictionary<string, int> { ["damage"] = 10, ["range"] = 150, ["speed"] = 100, ["cost"] = 100 };
        _towerDatabase[TowerType.Arrow][2] = new Dictionary<string, int> { ["damage"] = 18, ["range"] = 170, ["speed"] = 90, ["cost"] = 200 };
        _towerDatabase[TowerType.Arrow][3] = new Dictionary<string, int> { ["damage"] = 30, ["range"] = 200, ["speed"] = 80, ["cost"] = 400 };
        
        // Cannon Tower
        _towerDatabase[TowerType.Cannon] = new Dictionary<int, Dictionary<string, int>>();
        _towerDatabase[TowerType.Cannon][1] = new Dictionary<string, int> { ["damage"] = 25, ["range"] = 120, ["speed"] = 150, ["cost"] = 150 };
        _towerDatabase[TowerType.Cannon][2] = new Dictionary<string, int> { ["damage"] = 45, ["range"] = 140, ["speed"] = 130, ["cost"] = 300 };
        _towerDatabase[TowerType.Cannon][3] = new Dictionary<string, int> { ["damage"] = 75, ["range"] = 160, ["speed"] = 110, ["cost"] = 600 };
        
        // Ice Tower
        _towerDatabase[TowerType.Ice] = new Dictionary<int, Dictionary<string, int>>();
        _towerDatabase[TowerType.Ice][1] = new Dictionary<string, int> { ["damage"] = 5, ["range"] = 130, ["speed"] = 120, ["cost"] = 120 };
        _towerDatabase[TowerType.Ice][2] = new Dictionary<string, int> { ["damage"] = 10, ["range"] = 150, ["speed"] = 100, ["cost"] = 240 };
        _towerDatabase[TowerType.Ice][3] = new Dictionary<string, int> { ["damage"] = 18, ["range"] = 180, ["speed"] = 80, ["cost"] = 480 };
        
        // Fire Tower
        _towerDatabase[TowerType.Fire] = new Dictionary<int, Dictionary<string, int>>();
        _towerDatabase[TowerType.Fire][1] = new Dictionary<string, int> { ["damage"] = 20, ["range"] = 110, ["speed"] = 140, ["cost"] = 180 };
        _towerDatabase[TowerType.Fire][2] = new Dictionary<string, int> { ["damage"] = 36, ["range"] = 130, ["speed"] = 120, ["cost"] = 360 };
        _towerDatabase[TowerType.Fire][3] = new Dictionary<string, int> { ["damage"] = 60, ["range"] = 150, ["speed"] = 100, ["cost"] = 720 };
        
        // Lightning Tower
        _towerDatabase[TowerType.Lightning] = new Dictionary<int, Dictionary<string, int>>();
        _towerDatabase[TowerType.Lightning][1] = new Dictionary<string, int> { ["damage"] = 15, ["range"] = 140, ["speed"] = 110, ["cost"] = 160 };
        _towerDatabase[TowerType.Lightning][2] = new Dictionary<string, int> { ["damage"] = 28, ["range"] = 160, ["speed"] = 95, ["cost"] = 320 };
        _towerDatabase[TowerType.Lightning][3] = new Dictionary<string, int> { ["damage"] = 50, ["range"] = 190, ["speed"] = 80, ["cost"] = 640 };
        
        // Poison Tower
        _towerDatabase[TowerType.Poison] = new Dictionary<int, Dictionary<string, int>>();
        _towerDatabase[TowerType.Poison][1] = new Dictionary<string, int> { ["damage"] = 8, ["range"] = 120, ["speed"] = 100, ["cost"] = 140 };
        _towerDatabase[TowerType.Poison][2] = new Dictionary<string, int> { ["damage"] = 15, ["range"] = 140, ["speed"] = 85, ["cost"] = 280 };
        _towerDatabase[TowerType.Poison][3] = new Dictionary<string, int> { ["damage"] = 28, ["range"] = 170, ["speed"] = 70, ["cost"] = 560 };
        
        // Support Tower
        _towerDatabase[TowerType.Support] = new Dictionary<int, Dictionary<string, int>>();
        _towerDatabase[TowerType.Support][1] = new Dictionary<string, int> { ["damage"] = 0, ["range"] = 180, ["speed"] = 200, ["cost"] = 200 };
        _towerDatabase[TowerType.Support][2] = new Dictionary<string, int> { ["damage"] = 0, ["range"] = 210, ["speed"] = 180, ["cost"] = 400 };
        _towerDatabase[TowerType.Support][3] = new Dictionary<string, int> { ["damage"] = 0, ["range"] = 250, ["speed"] = 150, ["cost"] = 800 };
        
        // Ultimate Tower
        _towerDatabase[TowerType.Ultimate] = new Dictionary<int, Dictionary<string, int>>();
        _towerDatabase[TowerType.Ultimate][1] = new Dictionary<string, int> { ["damage"] = 50, ["range"] = 200, ["speed"] = 80, ["cost"] = 500 };
        _towerDatabase[TowerType.Ultimate][2] = new Dictionary<string, int> { ["damage"] = 90, ["range"] = 230, ["speed"] = 65, ["cost"] = 1000 };
        _towerDatabase[TowerType.Ultimate][3] = new Dictionary<string, int> { ["damage"] = 150, ["range"] = 270, ["speed"] = 50, ["cost"] = 2000 };
    }
    
    private void InitializeWaves()
    {
        _waves.Clear();
        
        // Generate 20 waves with increasing difficulty
        for (int i = 1; i <= 20; i++)
        {
            Wave wave = new Wave
            {
                WaveNumber = i,
                EnemyCount = 5 + i * 2,
                EnemyHealthScale = 1.0f + (i - 1) * 0.15f,
                RewardGold = 100 + i * 50,
                RewardPoints = 10 + i * 5
            };
            _waves.Add(wave);
        }
    }
    
    // Start a new defense session
    public void StartDefense(int guildId)
    {
        _guildId = guildId;
        _currentWave = 0;
        _totalGold = 0;
        _totalPoints = 0;
        _enemiesDefeated = 0;
        _towersBuilt = 0;
        _lives = 20;
        _towers.Clear();
        _waveState = WaveState.Preparing;
        InitializeWaves();
    }
    
    // Build a tower
    public bool BuildTower(TowerType type, Vector2 position)
    {
        if (_waveState != WaveState.Preparing && _waveState != WaveState.InProgress)
            return false;
            
        if (!_towerDatabase.ContainsKey(type))
            return false;
            
        Tower tower = new Tower
        {
            Type = type,
            Level = 1,
            Position = position
        };
        
        // Get tower stats from database
        var stats = _towerDatabase[type][1];
        tower.Damage = stats["damage"];
        tower.Range = stats["range"];
        tower.AttackSpeed = stats["speed"] / 100.0f;
        tower.Cost = stats["cost"];
        
        _towers.Add(tower);
        _towersBuilt++;
        
        return true;
    }
    
    // Upgrade a tower
    public bool UpgradeTower(int towerIndex)
    {
        if (towerIndex < 0 || towerIndex >= _towers.Count)
            return false;
            
        Tower tower = _towers[towerIndex];
        int nextLevel = tower.Level + 1;
        
        if (nextLevel > 3)
            return false;
            
        if (!_towerDatabase.ContainsKey(tower.Type))
            return false;
            
        var stats = _towerDatabase[tower.Type][nextLevel];
        tower.Level = nextLevel;
        tower.Damage = stats["damage"];
        tower.Range = stats["range"];
        tower.AttackSpeed = stats["speed"] / 100.0f;
        
        return true;
    }
    
    // Start next wave
    public bool StartNextWave()
    {
        if (_waveState == WaveState.InProgress)
            return false;
            
        if (_currentWave >= _waves.Count)
            return false;
            
        _currentWave++;
        _waveState = WaveState.InProgress;
        
        return true;
    }
    
    // Record enemy defeat
    public void OnEnemyDefeated(int goldReward, int pointsReward)
    {
        _enemiesDefeated++;
        _totalGold += goldReward;
        _totalPoints += pointsReward;
        
        // Check wave completion
        Wave currentWave = _waves[_currentWave - 1];
        if (_enemiesDefeated >= currentWave.EnemyCount)
        {
            _waveState = WaveState.Completed;
            _totalGold += currentWave.RewardGold;
            _totalPoints += currentWave.RewardPoints;
        }
    }
    
    // Record life lost
    public void OnLifeLost()
    {
        _lives--;
        
        if (_lives <= 0)
        {
            _waveState = WaveState.Failed;
        }
    }
    
    // Get tower info
    public Tower GetTower(int index)
    {
        if (index >= 0 && index < _towers.Count)
            return _towers[index];
        return null;
    }
    
    // Get current wave
    public Wave GetCurrentWave()
    {
        if (_currentWave > 0 && _currentWave <= _waves.Count)
            return _waves[_currentWave - 1];
        return null;
    }
    
    // Get tower count
    public int GetTowerCount() => _towers.Count;
    
    // Get wave state
    public WaveState GetWaveState() => _waveState;
    
    // Get current wave number
    public int GetCurrentWaveNumber() => _currentWave;
    
    // Get lives
    public int GetLives() => _lives;
    
    // Get stats
    public int GetTotalGold() => _totalGold;
    public int GetTotalPoints() => _totalPoints;
    public int GetEnemiesDefeated() => _enemiesDefeated;
    public int GetTowersBuilt() => _towersBuilt;
    
    // Get all towers
    public List<Tower> GetAllTowers() => new List<Tower>(_towers);
    
    // Get tower type name
    public static string GetTowerTypeName(TowerType type)
    {
        switch (type)
        {
            case TowerType.Arrow: return "Arrow Tower";
            case TowerType.Cannon: return "Cannon Tower";
            case TowerType.Ice: return "Ice Tower";
            case TowerType.Fire: return "Fire Tower";
            case TowerType.Lightning: return "Lightning Tower";
            case TowerType.Poison: return "Poison Tower";
            case TowerType.Support: return "Support Tower";
            case TowerType.Ultimate: return "Ultimate Tower";
            default: return "Unknown";
        }
    }
    
    // Get tower cost
    public static int GetTowerCost(TowerType type, int level = 1)
    {
        // Base costs
        int[] baseCosts = { 100, 150, 120, 180, 160, 140, 200, 500 };
        return baseCosts[(int)type] * level;
    }
    
    // Save data
    public Dictionary<string, object> SaveData()
    {
        Dictionary<string, object> data = new Dictionary<string, object>();
        data["guild_id"] = _guildId;
        data["current_wave"] = _currentWave;
        data["total_gold"] = _totalGold;
        data["total_points"] = _totalPoints;
        data["enemies_defeated"] = _enemiesDefeated;
        data["towers_built"] = _towersBuilt;
        data["lives"] = _lives;
        data["wave_state"] = (int)_waveState;
        
        // Save towers
        List<Dictionary<string, object>> towerList = new List<Dictionary<string, object>>();
        foreach (var tower in _towers)
        {
            Dictionary<string, object> towerData = new Dictionary<string, object>();
            towerData["type"] = (int)tower.Type;
            towerData["level"] = tower.Level;
            towerData["pos_x"] = tower.Position.x;
            towerData["pos_y"] = tower.Position.y;
            towerList.Add(towerData);
        }
        data["towers"] = towerList;
        
        return data;
    }
    
    // Load data
    public void LoadData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        _guildId = (int)data.GetValueOrDefault("guild_id", -1);
        _currentWave = (int)data.GetValueOrDefault("current_wave", 0);
        _totalGold = (int)data.GetValueOrDefault("total_gold", 0);
        _totalPoints = (int)data.GetValueOrDefault("total_points", 0);
        _enemiesDefeated = (int)data.GetValueOrDefault("enemies_defeated", 0);
        _towersBuilt = (int)data.GetValueOrDefault("towers_built", 0);
        _lives = (int)data.GetValueOrDefault("lives", 20);
        _waveState = (WaveState)(int)data.GetValueOrDefault("wave_state", 0);
        
        // Load towers
        _towers.Clear();
        var towerList = data.GetValueOrDefault("towers") as List<object>;
        if (towerList != null)
        {
            foreach (Dictionary<string, object> towerData in towerList)
            {
                Tower tower = new Tower
                {
                    Type = (TowerType)(int)towerData["type"],
                    Level = (int)towerData["level"],
                    Position = new Vector2((float)towerData["pos_x"], (float)towerData["pos_y"])
                };
                
                if (_towerDatabase.ContainsKey(tower.Type) && _towerDatabase[tower.Type].ContainsKey(tower.Level))
                {
                    var stats = _towerDatabase[tower.Type][tower.Level];
                    tower.Damage = stats["damage"];
                    tower.Range = stats["range"];
                    tower.AttackSpeed = stats["speed"] / 100.0f;
                    tower.Cost = stats["cost"];
                }
                
                _towers.Add(tower);
            }
        }
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return SaveData();
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        LoadData(new Dictionary<string, object>(data));
    }
}
