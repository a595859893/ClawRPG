using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

/// <summary>
/// 封印之塔管理器 - 管理封印之塔爬塔玩法
/// </summary>
public partial class SealedTowerManager : BaseSystem
{
    public static SealedTowerManager Instance { get; private set; }
    
    // Tower state
    private int _currentFloor = 1;
    private int _maxFloorReached = 1;
    private int _totalRuns = 0;
    private int _wins = 0;
    private bool _isInTower = false;
    private bool _isPaused = false;
    
    // Current run data
    private int _currentFloorEnemiesDefeated = 0;
    private int _totalEnemiesDefeated = 0;
    private int _goldEarned = 0;
    private int _expEarned = 0;
    private List<string> _acquiredBoons = new List<string>();
    private List<string> _activeCurses = new List<string>();
    private int _currentHealth = 0;
    private int _maxHealth = 100;
    
    // Tower configuration
    private int _enemiesPerFloor = 5;
    private float _enemyScaling = 1.1f; // 10% more HP/damage per floor
    private float _rewardScaling = 1.15f; // 15% more rewards per floor
    
    // Tower floors
    private List<TowerFloor> _floors = new List<TowerFloor>();
    
    // Signals
public delegate void FloorChanged(int floor);
public delegate void RunStarted();
public delegate void RunEnded(bool victory, int floorsCleared, int enemiesDefeated);
public delegate void BoonAcquired(string boonId);
public delegate void CurseAcquired(string curseId);
    
    public override void _Ready()
    {
        Instance = this;
        InitializeTower();
    }
    
    private void InitializeTower()
    {
        // Create predefined tower floors
        for (int i = 1; i <= 100; i++)
        {
            var floor = new TowerFloor
            {
                FloorNumber = i,
                Name = GetFloorName(i),
                Description = GetFloorDescription(i),
                EnemyCount = Math.Min(3 + (i / 10), 15),
                EnemyMultiplier = Mathf.Pow(_enemyScaling, i - 1),
                GoldReward = (int)(100 * Mathf.Pow(_rewardScaling, i - 1)),
                ExpReward = (int)(50 * Mathf.Pow(_rewardScaling, i - 1)),
                Rarity = GetFloorRarity(i)
            };
            _floors.Add(floor);
        }
    }
    
    private string GetFloorName(int floor)
    {
        if (floor <= 10) return "Ground Floor";
        if (floor <= 20) return "Dark Cellar";
        if (floor <= 30) return "Forgotten Hall";
        if (floor <= 40) return "Cursed Corridor";
        if (floor <= 50) return "Phantom Chamber";
        if (floor <= 60) return "Blood Arena";
        if (floor <= 70) return "Demon's Lair";
        if (floor <= 80) return "Dragon's Nest";
        if (floor <= 90) return "Celestial Hall";
        return "Sealed Throne";
    }
    
    private string GetFloorDescription(int floor)
    {
        string[] descriptions = {
            "The entrance to the ancient sealed tower.",
            "Shadows dance on the cold stone walls.",
            "The air grows thick with ancient magic.",
            "Cursed energy flows through this corridor.",
            "Ghostly whispers echo in the darkness.",
            "Blood stains mark the arena floor.",
            "Demon seals flicker with dark energy.",
            "Dragon scales cover every surface.",
            "Divine light shines from above.",
            "The final seal awaits."
        };
        int index = Math.Min(floor / 10, descriptions.Length - 1);
        return descriptions[index];
    }
    
    private string GetFloorRarity(int floor)
    {
        if (floor <= 20) return "Common";
        if (floor <= 40) return "Uncommon";
        if (floor <= 60) return "Rare";
        if (floor <= 80) return "Epic";
        return "Legendary";
    }
    
    public void StartRun()
    {
        if (_isInTower) return;
        
        _isInTower = true;
        _currentFloor = 1;
        _currentFloorEnemiesDefeated = 0;
        _totalEnemiesDefeated = 0;
        _goldEarned = 0;
        _expEarned = 0;
        _acquiredBoons.Clear();
        _activeCurses.Clear();
        
        // Get player's current health
        if (Player.Instance != null)
        {
            _maxHealth = Player.Instance.MaxHealth;
            _currentHealth = _maxHealth;
        }
        
        EmitSignal(nameof(RunStarted));
        GD.Print($"[SealedTower] Started run at floor {_currentFloor}");
    }
    
    public void ExitTower(bool victory)
    {
        if (!_isInTower) return;
        
        _isInTower = false;
        _totalRuns++;
        
        if (victory)
        {
            _wins++;
            if (_currentFloor > _maxFloorReached)
            {
                _maxFloorReached = _currentFloor;
            }
        }
        
        EmitSignal(nameof(RunEnded), victory, _currentFloor, _totalEnemiesDefeated);
        GD.Print($"[SealedTower] Run ended - Victory: {victory}, Floors: {_currentFloor}, Enemies: {_totalEnemiesDefeated}");
    }
    
    public void EnemyDefeated()
    {
        if (!_isInTower) return;
        
        _currentFloorEnemiesDefeated++;
        _totalEnemiesDefeated++;
        
        var currentFloorData = GetCurrentFloorData();
        if (currentFloorData != null)
        {
            _goldEarned += currentFloorData.GoldReward / currentFloorData.EnemyCount;
            _expEarned += currentFloorData.ExpReward / currentFloorData.EnemyCount;
        }
        
        // Check if floor is complete
        if (_currentFloorEnemiesDefeated >= GetCurrentFloorData()?.EnemyCount)
        {
            AdvanceToNextFloor();
        }
    }
    
    private void AdvanceToNextFloor()
    {
        int oldFloor = _currentFloor;
        _currentFloor++;
        _currentFloorEnemiesDefeated = 0;
        
        // Random event on floor advance
        TriggerRandomFloorEvent();
        
        EmitSignal(nameof(FloorChanged), _currentFloor);
        GD.Print($"[SealedTower] Advanced to floor {_currentFloor}");
    }
    
    private void TriggerRandomFloorEvent()
    {
        var random = new Random();
        int eventType = random.Next(10);
        
        switch (eventType)
        {
            case 0: // Boon
            case 1:
                GrantRandomBoon();
                break;
            case 2: // Curse
                GrantRandomCurse();
                break;
            case 3: // Treasure
            case 4:
                _goldEarned += 50 * _currentFloor;
                break;
            case 5: // Rest site
                HealPlayer(Math.Max(10, _maxHealth / 4));
                break;
            // 6-9: No event
        }
    }
    
    private void GrantRandomBoon()
    {
        string[] boons = { "attack_boost", "defense_boost", "speed_boost", "health_boost", "crit_boost", "lifesteal_boost" };
        var random = new Random();
        string boon = boons[random.Next(boons.Length)];
        
        if (!_acquiredBoons.Contains(boon))
        {
            _acquiredBoons.Add(boon);
            EmitSignal(nameof(BoonAcquired), boon);
            GD.Print($"[SealedTower] Acquired boon: {boon}");
        }
    }
    
    private void GrantRandomCurse()
    {
        string[] curses = { "weakness", "fragile", "slow", "vulnerable", "cursed" };
        var random = new Random();
        string curse = curses[random.Next(curses.Length)];
        
        if (!_activeCurses.Contains(curse))
        {
            _activeCurses.Add(curse);
            EmitSignal(nameof(CurseAcquired), curse);
            GD.Print($"[SealedTower] Acquired curse: {curse}");
        }
    }
    
    public void HealPlayer(int amount)
    {
        _currentHealth = Math.Min(_currentHealth + amount, _maxHealth);
    }
    
    public void TakeDamage(int damage)
    {
        // Apply curse effects
        int actualDamage = damage;
        if (_activeCurses.ContainsKey("vulnerable"))
        {
            actualDamage = (int)(actualDamage * 1.25);
        }
        
        _currentHealth -= actualDamage;
        
        if (_currentHealth <= 0)
        {
            ExitTower(false);
        }
    }
    
    public TowerFloor GetCurrentFloorData()
    {
        if (_currentFloor > 0 && _currentFloor <= _floors.Count)
        {
            return _floors[_currentFloor - 1];
        }
        return null;
    }
    
    public Dictionary<string, int> GetTowerStats()
    {
        return new Dictionary<string, int>
        {
            { "total_runs", _totalRuns },
            { "wins", _wins },
            { "max_floor", _maxFloorReached },
            { "current_floor", _currentFloor },
            { "enemies_defeated", _totalEnemiesDefeated },
            { "gold_earned", _goldEarned },
            { "exp_earned", _expEarned },
            { "boons_count", _acquiredBoons.Count },
            { "curses_count", _activeCurses.Count }
        };
    }
    
    // Getters
    public bool IsInTower => _isInTower;
    public int CurrentFloor => _currentFloor;
    public int MaxFloorReached => _maxFloorReached;
    public int CurrentHealth => _currentHealth;
    public int MaxHealth => _maxHealth;
    public List<string> AcquiredBoons => new List<string>(_acquiredBoons);
    public List<string> ActiveCurses => new List<string>(_activeCurses);
    public float GetFloorEnemyMultiplier() => GetCurrentFloorData()?.EnemyMultiplier ?? 1.0f;
    
    // Save/Load
    public Dictionary<string, object> SaveData()
    {
        return new Dictionary<string, object>
        {
            { "max_floor_reached", _maxFloorReached },
            { "total_runs", _totalRuns },
            { "wins", _wins }
        };
    }
    
    public void LoadData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("max_floor_reached"))
            _maxFloorReached = (int)data["max_floor_reached"];
        if (data.ContainsKey("total_runs"))
            _totalRuns = (int)data["total_runs"];
        if (data.ContainsKey("wins"))
            _wins = (int)data["wins"];
    }
    
    /// <summary>
    /// Export save data for persistence
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        return new Dictionary
        {
            { "current_floor", _currentFloor },
            { "max_floor_reached", _maxFloorReached },
            { "total_runs", _totalRuns },
            { "wins", _wins },
            { "is_in_tower", _isInTower }
        };
    }
    
    /// <summary>
    /// Import save data from persistence
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("current_floor")) _currentFloor = (int)data["current_floor"];
        if (data.ContainsKey("max_floor_reached")) _maxFloorReached = (int)data["max_floor_reached"];
        if (data.ContainsKey("total_runs")) _totalRuns = (int)data["total_runs"];
        if (data.ContainsKey("wins")) _wins = (int)data["wins"];
        if (data.ContainsKey("is_in_tower")) _isInTower = (bool)data["is_in_tower"];
    }
}

public class TowerFloor
{
    public int FloorNumber { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int EnemyCount { get; set; }
    public float EnemyMultiplier { get; set; }
    public int GoldReward { get; set; }
    public int ExpReward { get; set; }
    public string Rarity { get; set; }
}
