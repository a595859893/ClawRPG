using Godot;
using System;
using System.Collections.Generic;

public class PetBattleArenaSystem : BaseSystem
{
    public static PetBattleArenaSystem Instance { get; private set; }
    
    public enum BattleState
    {
        Idle,
        Preparing,
        BattleActive,
        WaveComplete,
        BattleVictory,
        BattleDefeat,
        Paused
    }
    
    private BattleState _currentState = BattleState.Idle;
    private PetBattleArenaData _currentArena;
    private PetBattleInstance _playerPet;
    private int _currentWave = 0;
    private int _enemiesRemaining = 0;
    private int _playerCurrentHealth;
    private int _playerMaxHealth;
    private float _battleTimer = 0f;
    private float _waveDelayTimer = 0f;
    private bool _waveInProgress = false; 
    
    // Battle statistics
    private int _totalDamageDealt = 0;
    private int _totalDamageTaken = 0;
    private int _enemiesDefeated = 0;
    private int _highestCombo = 0;
    private int _currentCombo = 0;
    private float _lastAttackTime = 0f;
    
    // Player data
    private PlayerPetBattleData _playerData = new PlayerPetBattleData();
    
    // Signals
    [Signal]
    public delegate void BattleStarted(string arenaId);
    
    [Signal]
    public delegate void BattleEnded(bool victory, int wavesCleared, int damageDealt);
    
    [Signal]
    public delegate void WaveStarted(int waveNumber, int totalWaves);
    
    [Signal]
    public delegate void WaveCompleted(int waveNumber);
    
    [Signal]
    public delegate void PetDamaged(int currentHealth, int maxHealth);
    
    [Signal]
    public delegate void EnemyDefeated(string enemyId);
    
    [Signal]
    public delegate void BattleStatsUpdated(int damageDealt, int damageTaken, int enemiesDefeated);
    
    public override void _Ready()
    {
        Instance = this;
        PetBattleArenaDatabase.Initialize();
        LoadData();
    }
    
    public BattleState CurrentState => _currentState;
    public PetBattleArenaData CurrentArena => _currentArena;
    public int CurrentWave => _currentWave;
    public int PlayerCurrentHealth => _playerCurrentHealth;
    public int PlayerMaxHealth => _playerMaxHealth;
    public float BattleTimer => _battleTimer;
    public PlayerPetBattleData PlayerData => _playerData;
    
    public bool StartBattle(string arenaId, PetBattleInstance pet)
    {
        if (_currentState != BattleState.Idle && _currentState != BattleState.BattleVictory && _currentState != BattleState.BattleDefeat)
        {
            GD.Print("Cannot start battle - current state: " + _currentState);
            return false;
        }
        
        var arena = PetBattleArenaDatabase.GetArena(arenaId);
        if (arena == null)
        {
            GD.Print("Arena not found: " + arenaId);
            return false;
        }
        
        _currentArena = arena;
        _playerPet = pet;
        _currentWave = 0;
        _playerMaxHealth = pet.MaxHealth;
        _playerCurrentHealth = pet.CurrentHealth;
        _battleTimer = 0f;
        _waveDelayTimer = 0f;
        _waveInProgress = false; 
        
        // Reset stats
        _totalDamageDealt = 0;
        _totalDamageTaken = 0;
        _enemiesDefeated = 0;
        _highestCombo = 0;
        _currentCombo = 0;
        
        _currentState = BattleState.Preparing;
        
        // Unlock arena if needed
        if (!_playerData.UnlockedArenas.Contains(arenaId))
        {
            _playerData.UnlockedArenas.Add(arenaId);
            SaveData();
        }
        
        EmitSignal(nameof(BattleStarted), arenaId);
        
        // Start first wave after brief delay
        _waveDelayTimer = 1.0f;
        _currentState = BattleState.BattleActive;
        
        return true;
    }
    
    public override void _Process(float delta)
    {
        if (_currentState != BattleState.BattleActive)
            return;
        
        _battleTimer += delta;
        
        // Handle wave delay
        if (_waveDelayTimer > 0)
        {
            _waveDelayTimer -= delta;
            if (_waveDelayTimer <= 0)
            {
                StartNextWave();
            }
            return;
        }
        
        // Check if wave is complete
        if (_waveInProgress && _enemiesRemaining <= 0)
        {
            OnWaveComplete();
        }
    }
    
    private void StartNextWave()
    {
        _currentWave++;
        
        if (_currentWave > _currentArena.TotalWaves)
        {
            OnBattleVictory();
            return;
        }
        
        var waves = PetBattleArenaDatabase.GetWaves(_currentArena.ArenaId);
        if (waves == null || _currentWave > waves.Length)
        {
            OnBattleVictory();
            return;
        }
        
        var wave = waves[_currentWave - 1];
        _enemiesRemaining = wave.EnemyCount;
        _waveInProgress = true;
        
        EmitSignal(nameof(WaveStarted), _currentWave, _currentArena.TotalWaves);
        GD.Print($"Wave {_currentWave}/{_currentArena.TotalWaves} started - {wave.EnemyCount} {wave.EnemyId}");
    }
    
    private void OnWaveComplete()
    {
        _waveInProgress = false; 
        
        if (_currentWave >= _currentArena.TotalWaves)
        {
            OnBattleVictory();
            return;
        }
        
        EmitSignal(nameof(WaveCompleted), _currentWave);
        
        // Setup next wave delay
        _waveDelayTimer = 2.0f;
        
        // Update best wave
        if (_currentWave > _playerData.BestWave)
        {
            _playerData.BestWave = _currentWave;
        }
        
        string arenaId = _currentArena.ArenaId;
        if (_playerData.ArenaBestWaves.ContainsKey(arenaId))
        {
            if (_currentWave > _playerData.ArenaBestWaves[arenaId])
            {
                _playerData.ArenaBestWaves[arenaId] = _currentWave;
            }
        }
        else
        {
            _playerData.ArenaBestWaves[arenaId] = _currentWave;
        }
        
        SaveData();
    }
    
    private void OnBattleVictory()
    {
        _currentState = BattleState.BattleVictory;
        
        _playerData.Victories++;
        _playerData.TotalBattles++;
        
        // Mark arena as completed
        if (!_playerData.ArenaCompleted.ContainsKey(_currentArena.ArenaId))
        {
            _playerData.ArenaCompleted[_currentArena.ArenaId] = true;
        }
        
        // Grant rewards
        GrantRewards();
        
        SaveData();
        
        GD.Print($"Battle Victory! Cleared {_currentWave} waves");
        EmitSignal(nameof(BattleEnded), true, _currentWave, _totalDamageDealt);
    }
    
    public void OnBattleDefeat()
    {
        _currentState = BattleState.BattleDefeat;
        _playerData.Defeats++;
        _playerData.TotalBattles++;
        
        SaveData();
        
        GD.Print($"Battle Defeat! Reached wave {_currentWave}");
        EmitSignal(nameof(BattleEnded), false, _currentWave, _totalDamageDealt);
    }
    
    private void GrantRewards()
    {
        if (_currentArena == null) return;
        
        // Gold reward
        int goldReward = _currentArena.RewardGold;
        Player player = GetNodeOrNull<Player>("/root/Main/Player");
        if (player != null)
        {
            player.AddGold(goldReward);
        }
        
        // Experience reward
        int expReward = _currentArena.RewardExp;
        if (_playerPet != null)
        {
            _playerPet.Experience += expReward;
        }
        
        // Item rewards
        if (_currentArena.RewardItems != null)
        {
            InventoryManager inventory = InventoryManager.Instance;
            if (inventory != null)
            {
                foreach (string itemId in _currentArena.RewardItems)
                {
                    inventory.AddItem(itemId, 1);
                }
            }
        }
        
        GD.Print($"Rewards granted: {goldReward} gold, {expReward} exp");
    }
    
    // Combat methods
    public void PetAttack(int damage)
    {
        if (_currentState != BattleState.BattleActive || !_waveInProgress)
            return;
        
        // Apply combo
        float currentTime = _battleTimer;
        if (currentTime - _lastAttackTime < 1.5f)
        {
            _currentCombo++;
            if (_currentCombo > _highestCombo)
                _highestCombo = _currentCombo;
        }
        else
        {
            _currentCombo = 1;
        }
        _lastAttackTime = currentTime;
        
        // Calculate actual damage
        int actualDamage = Mathf.Max(1, damage);
        _totalDamageDealt += actualDamage;
        
        // Enemy takes damage
        _enemiesRemaining--;
        
        EmitSignal(nameof(EnemyDefeated), "enemy");
        EmitSignal(nameof(BattleStatsUpdated), _totalDamageDealt, _totalDamageTaken, _enemiesDefeated);
    }
    
    public void PetTakeDamage(int damage)
    {
        if (_currentState != BattleState.BattleActive)
            return;
        
        // Apply defense
        int defense = _playerPet != null ? _playerPet.Defense : 0;
        int actualDamage = Mathf.Max(1, damage - defense / 2);
        
        _playerCurrentHealth -= actualDamage;
        _totalDamageTaken += actualDamage;
        
        // Reset combo on hit
        _currentCombo = 0;
        
        EmitSignal(nameof(PetDamaged), _playerCurrentHealth, _playerMaxHealth);
        EmitSignal(nameof(BattleStatsUpdated), _totalDamageDealt, _totalDamageTaken, _enemiesDefeated);
        
        if (_playerCurrentHealth <= 0)
        {
            _playerCurrentHealth = 0;
            OnBattleDefeat();
        }
    }
    
    public void UseSkill(string skillId)
    {
        if (_currentState != BattleState.BattleActive)
            return;
        
        // Skill logic based on skill ID
        GD.Print($"Pet using skill: {skillId}");
    }
    
    public void HealPet(int amount)
    {
        if (_currentState != BattleState.BattleActive)
            return;
        
        _playerCurrentHealth = Mathf.Min(_playerMaxHealth, _playerCurrentHealth + amount);
        EmitSignal(nameof(PetDamaged), _playerCurrentHealth, _playerMaxHealth);
    }
    
    public void PauseBattle()
    {
        if (_currentState == BattleState.BattleActive)
        {
            _currentState = BattleState.Paused;
        }
    }
    
    public void ResumeBattle()
    {
        if (_currentState == BattleState.Paused)
        {
            _currentState = BattleState.BattleActive;
        }
    }
    
    public void Surrender()
    {
        if (_currentState == BattleState.BattleActive)
        {
            OnBattleDefeat();
        }
    }
    
    public void ExitBattle()
    {
        _currentState = BattleState.Idle;
        _currentArena = null;
        _playerPet = null;
    }
    
    public PetBattleArenaData[] GetUnlockedArenas(int playerLevel)
    {
        var allArenas = PetBattleArenaDatabase.GetAllArenas();
        var unlocked = new List<PetBattleArenaData>();
        
        foreach (var arena in allArenas)
        {
            if (PetBattleArenaDatabase.IsUnlocked(arena, playerLevel))
            {
                unlocked.Add(arena);
            }
        }
        
        return unlocked.ToArray();
    }
    
    public int GetBestWave(string arenaId)
    {
        return _playerData.ArenaBestWaves.ContainsKey(arenaId) ? _playerData.ArenaBestWaves[arenaId] : 0;
    }
    
    public bool IsArenaCompleted(string arenaId)
    {
        return _playerData.ArenaCompleted.ContainsKey(arenaId) && _playerData.ArenaCompleted[arenaId];
    }
    
    // Save/Load
    public Dictionary<string, object> Save()
    {
        var data = new Dictionary<string, object>();
        data["total_battles"] = _playerData.TotalBattles;
        data["victories"] = _playerData.Victories;
        data["defeats"] = _playerData.Defeats;
        data["best_wave"] = _playerData.BestWave;
        data["total_damage_dealt"] = _playerData.TotalDamageDealt;
        data["total_damage_taken"] = _playerData.TotalDamageTaken;
        data["enemies_defeated"] = _playerData.EnemiesDefeated;
        data["unlocked_arenas"] = _playerData.UnlockedArenas;
        data["arena_best_waves"] = _playerData.ArenaBestWaves;
        data["arena_completed"] = _playerData.ArenaCompleted;
        return data;
    }
    
    public void Load(Dictionary<string, object> data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("total_battles")) _playerData.TotalBattles = (int)data["total_battles"];
        if (data.ContainsKey("victories")) _playerData.Victories = (int)data["victories"];
        if (data.ContainsKey("defeats")) _playerData.Defeats = (int)data["defeats"];
        if (data.ContainsKey("best_wave")) _playerData.BestWave = (int)data["best_wave"];
        if (data.ContainsKey("total_damage_dealt")) _playerData.TotalDamageDealt = (int)data["total_damage_dealt"];
        if (data.ContainsKey("total_damage_taken")) _playerData.TotalDamageTaken = (int)data["total_damage_taken"];
        if (data.ContainsKey("enemies_defeated")) _playerData.EnemiesDefeated = (int)data["enemies_defeated"];
        
        if (data.ContainsKey("unlocked_arenas"))
        {
            _playerData.UnlockedArenas = new List<string>((string[])data["unlocked_arenas"]);
        }
        
        if (data.ContainsKey("arena_best_waves"))
        {
            var dict = (Dictionary<string, object>)data["arena_best_waves"];
            _playerData.ArenaBestWaves = new Dictionary<string, int>();
            foreach (var kvp in dict)
            {
                _playerData.ArenaBestWaves[kvp.Key] = (int)kvp.Value;
            }
        }
        
        if (data.ContainsKey("arena_completed"))
        {
            var dict = (Dictionary<string, object>)data["arena_completed"];
            _playerData.ArenaCompleted = new Dictionary<string, bool>();
            foreach (var kvp in dict)
            {
                _playerData.ArenaCompleted[kvp.Key] = (bool)kvp.Value;
            }
        }
    }
    
    private void SaveData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/Main/SaveSystem");
        if (saveSystem != null)
        {
            var data = Save();
            saveSystem.SaveGameData("pet_battle_arena", data);
        }
    }
    
    private void LoadData()
    {
        var saveSystem = GetNode<SaveSystem>("/root/Main/SaveSystem");
        if (saveSystem != null)
        {
            var data = saveSystem.LoadGameData("pet_battle_arena");
            Load(data);
        }
    }

        public override Dictionary ExportSaveData() => new();
        public override void ImportSaveData(Dictionary data) { }
}
