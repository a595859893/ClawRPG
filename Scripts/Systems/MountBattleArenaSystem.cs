using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// 坐骑战斗竞技场系统
/// </summary>
public partial class MountBattleArenaSystem : BaseSystem
{
    private static MountBattleArenaSystem _instance;
    public static MountBattleArenaSystem Instance
    {
        get
        {
            if (_instance == null)
                _instance = new MountBattleArenaSystem();
            return _instance;
        }
    }
    
    // 信号系统 (C# events, Godot 4 compatible)
    public static event Action<string> OnBattleStarted;
    public static event Action<string> OnBattleEnded;
    public static event Action<string, bool> OnBattleCompleted; // arenaId, victory
    public static event Action<int, int> OnWaveStarted; // currentWave, totalWaves
    public static event Action<int> OnWaveCompleted; // waveNumber
    public static event Action OnBattleVictory;
    public static event Action OnBattleDefeat;
    
    private MountBattleArenaData.MountBattleInstance _currentBattle;
    private MountBattleArenaData.PlayerMountArenaData _playerData;
    private List<MountBattleArenaData.MountArena> _arenas;
    private Random _random = new Random();
    
    public MountBattleArenaData.MountBattleInstance CurrentBattle => _currentBattle;
    public MountBattleArenaData.PlayerMountArenaData PlayerData => _playerData;
    public bool IsInBattle => _currentBattle != null && _currentBattle.State == MountBattleArenaData.BattleState.InProgress;
    
    public MountBattleArenaSystem()
    {
        _arenas = MountBattleArenaDatabase.GetAllArenas();
        _playerData = new MountBattleArenaData.PlayerMountArenaData();
    }

    public override void _Ready()
    {
        base._Ready();
        Initialize();
    }

    protected override void Initialize()
    {
        _arenas = MountBattleArenaDatabase.GetAllArenas();
        IsInitialized = true;
        GD.Print("[MountBattleArenaSystem] initialized");
    }
    
    public List<MountBattleArenaData.MountArena> GetAllArenas()
    {
        return _arenas;
    }
    
    public MountBattleArenaData.MountArena GetArena(string arenaId)
    {
        return MountBattleArenaDatabase.GetArena(arenaId);
    }
    
    public bool CanStartBattle(string mountId, string arenaId)
    {
        var mount = GetMountById(mountId);
        if (mount == null)
        {
            GD.PrintErr($"[MountBattleArena] Mount not found: {mountId}");
            return false;
        }
        
        var arena = GetArena(arenaId);
        if (arena == null)
        {
            GD.PrintErr($"[MountBattleArena] Arena not found: {arenaId}");
            return false;
        }
        
        // Check mount level
        if (mount.GetLevel() < arena.RecommendedLevel)
        {
            GD.PrintErr($"[MountBattleArena] Mount level too low: {mount.GetLevel()} < {arena.RecommendedLevel}");
            return false;
        }
        
        // Check entry fee
        var player = Main.Instance.GetPlayer();
        if (player != null && player.Gold < arena.EntryFee)
        {
            GD.PrintErr($"[MountBattleArena] Insufficient gold: {player.Gold} < {arena.EntryFee}");
            return false;
        }
        
        // Check if already in battle
        if (IsInBattle)
        {
            GD.PrintErr("[MountBattleArena] Already in battle");
            return false;
        }
        
        return true;
    }
    
    public bool StartBattle(string mountId, string arenaId)
    {
        if (!CanStartBattle(mountId, arenaId))
            return false;
        
        var mount = GetMountById(mountId);
        var arena = GetArena(arenaId);
        
        // Deduct entry fee
        var player = Main.Instance.GetPlayer();
        if (player != null && arena.EntryFee > 0)
        {
            player.Gold -= arena.EntryFee;
        }
        
        // Create battle instance
        _currentBattle = new MountBattleArenaData.MountBattleInstance
        {
            MountId = mountId,
            ArenaId = arenaId,
            CurrentWave = 1,
            EnemiesDefeated = 0,
            TotalDamageDealt = 0,
            TotalDamageTaken = 0,
            SkillsUsed = 0,
            State = MountBattleArenaData.BattleState.InProgress,
            StartTime = DateTime.Now
        };
        
        // Update player data
        _playerData.TotalBattles++;
        if (!_playerData.BattleCount.ContainsKey(arenaId))
            _playerData.BattleCount[arenaId] = 0;
        _playerData.BattleCount[arenaId]++;
        
        OnBattleStarted.Emit(arenaId);
        OnWaveStarted.Emit(_currentBattle.CurrentWave, arena.TotalWaves);
        
        GD.Print($"[MountBattleArena] Battle started: {mountId} in {arenaId}");
        return true;
    }
    
    public void OnEnemyDefeated(int damageDealt)
    {
        if (_currentBattle == null || _currentBattle.State != MountBattleArenaData.BattleState.InProgress)
            return;
        
        _currentBattle.EnemiesDefeated++;
        _currentBattle.TotalDamageDealt += damageDealt;
        
        var arena = GetArena(_currentBattle.ArenaId);
        if (arena == null) return;
        
        int enemiesThisWave = arena.EnemiesPerWave;
        
        // Check if wave is complete
        if (_currentBattle.EnemiesDefeated >= _currentBattle.CurrentWave * enemiesThisWave)
        {
            OnWaveCompleted.Emit(_currentBattle.CurrentWave);
            
            if (_currentBattle.CurrentWave >= arena.TotalWaves)
            {
                // Victory!
                CompleteBattle(true);
            }
            else
            {
                // Next wave
                _currentBattle.CurrentWave++;
                OnWaveStarted.Emit(_currentBattle.CurrentWave, arena.TotalWaves);
            }
        }
    }
    
    public void OnMountDamaged(int damage)
    {
        if (_currentBattle == null || _currentBattle.State != MountBattleArenaData.BattleState.InProgress)
            return;
        
        _currentBattle.TotalDamageTaken += damage;
        
        var mount = GetMountById(_currentBattle.MountId);
        if (mount != null)
        {
            // Check if mount is defeated (simplified - in real implementation would check mount health)
            if (_currentBattle.TotalDamageTaken >= mount.GetMaxHealth() * 0.8f)
            {
                CompleteBattle(false);
            }
        }
    }
    
    public void OnSkillUsed()
    {
        if (_currentBattle == null || _currentBattle.State != MountBattleArenaData.BattleState.InProgress)
            return;
        
        _currentBattle.SkillsUsed++;
    }
    
    public void CompleteBattle(bool victory)
    {
        if (_currentBattle == null) return;
        
        var arena = GetArena(_currentBattle.ArenaId);
        
        if (victory)
        {
            _currentBattle.State = MountBattleArenaData.BattleState.Victory;
            _playerData.Victories++;
            _playerData.TotalWavesCleared += _currentBattle.CurrentWave;
            
            // Calculate rewards
            int goldReward = CalculateGoldReward(arena);
            int expReward = CalculateExpReward(arena);
            
            var player = Main.Instance.GetPlayer();
            if (player != null)
            {
                player.Gold += goldReward;
                player.AddExp(expReward);
            }
            
            _playerData.TotalGoldEarned += goldReward;
            _playerData.TotalExpEarned += expReward;
            
            // Update best waves
            if (!_playerData.BestWaves.ContainsKey(_currentBattle.ArenaId) ||
                _currentBattle.CurrentWave > _playerData.BestWaves[_currentBattle.ArenaId])
            {
                _playerData.BestWaves[_currentBattle.ArenaId] = _currentBattle.CurrentWave;
            }
            
            OnBattleVictory.Emit();
            GD.Print($"[MountBattleArena] Victory! Gold: {goldReward}, Exp: {expReward}");
        }
        else
        {
            _currentBattle.State = MountBattleArenaData.BattleState.Defeated;
            _playerData.Defeats++;
            
            // Consolation reward
            int goldReward = arena.BaseGoldReward / 5;
            var player = Main.Instance.GetPlayer();
            if (player != null)
            {
                player.Gold += goldReward;
            }
            
            _playerData.TotalGoldEarned += goldReward;
            
            OnBattleDefeat.Emit();
            GD.Print($"[MountBattleArena] Defeat. Consolation: {goldReward}");
        }
        
        OnBattleCompleted.Emit(_currentBattle.ArenaId, victory);
        OnBattleEnded.Emit(_currentBattle.ArenaId);
        
        _currentBattle = null;
        
        // Auto save
        SaveSystem.Instance.AutoSave();
    }
    
    public void CancelBattle()
    {
        if (_currentBattle == null) return;
        
        _currentBattle = null;
        OnBattleEnded.Emit("");
        
        GD.Print("[MountBattleArena] Battle cancelled");
    }
    
    private int CalculateGoldReward(MountBattleArenaData.MountArena arena)
    {
        float multiplier = 1.0f;
        
        // Wave completion bonus
        if (_currentBattle != null)
        {
            multiplier += (_currentBattle.CurrentWave - 1) * 0.2f;
        }
        
        // Damage dealt bonus
        if (_currentBattle != null && _currentBattle.TotalDamageDealt > 0)
        {
            multiplier += (_currentBattle.TotalDamageDealt / 1000.0f) * 0.1f;
        }
        
        return (int)(arena.BaseGoldReward * multiplier);
    }
    
    private int CalculateExpReward(MountBattleArenaData.MountArena arena)
    {
        float multiplier = 1.0f;
        
        if (_currentBattle != null)
        {
            multiplier += (_currentBattle.CurrentWave - 1) * 0.2f;
            
            // Skill usage bonus
            if (_currentBattle.SkillsUsed > 0)
            {
                multiplier += _currentBattle.SkillsUsed * 0.05f;
            }
        }
        
        return (int)(arena.BaseExpReward * multiplier);
    }
    
    private Mount GetMountById(string mountId)
    {
        var mountManager = MountManager.Instance;
        if (mountManager == null) return null;
        
        var mounts = mountManager.GetMounts();
        foreach (var mount in mounts)
        {
            if (mount.GetId() == mountId)
                return mount;
        }
        return null;
    }
    
    protected override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        // Save player data
        data["total_battles"] = _playerData.TotalBattles;
        data["victories"] = _playerData.Victories;
        data["defeats"] = _playerData.Defeats;
        data["total_waves_cleared"] = _playerData.TotalWavesCleared;
        data["total_gold_earned"] = _playerData.TotalGoldEarned;
        data["total_exp_earned"] = _playerData.TotalExpEarned;
        
        // Save best waves
        var bestWavesList = new Godot.Collections.Array();
        foreach (var kvp in _playerData.BestWaves)
        {
            bestWavesList.Add(new Godot.Collections.Dictionary
            {
                { "arena_id", kvp.Key },
                { "waves", kvp.Value }
            });
        }
        data["best_waves"] = bestWavesList;
        
        // Save battle counts
        var battleCountList = new Godot.Collections.Array();
        foreach (var kvp in _playerData.BattleCount)
        {
            battleCountList.Add(new Godot.Collections.Dictionary
            {
                { "arena_id", kvp.Key },
                { "count", kvp.Value }
            });
        }
        data["battle_counts"] = battleCountList;
        
        return data;
    }
    
    protected override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        if (data.ContainsKey("total_battles"))
            _playerData.TotalBattles = Convert.ToInt32(data["total_battles"]);
        if (data.ContainsKey("victories"))
            _playerData.Victories = Convert.ToInt32(data["victories"]);
        if (data.ContainsKey("defeats"))
            _playerData.Defeats = Convert.ToInt32(data["defeats"]);
        if (data.ContainsKey("total_waves_cleared"))
            _playerData.TotalWavesCleared = Convert.ToInt32(data["total_waves_cleared"]);
        if (data.ContainsKey("total_gold_earned"))
            _playerData.TotalGoldEarned = Convert.ToInt32(data["total_gold_earned"]);
        if (data.ContainsKey("total_exp_earned"))
            _playerData.TotalExpEarned = Convert.ToInt32(data["total_exp_earned"]);
        
        // Load best waves
        if (data.ContainsKey("best_waves"))
        {
            var bestWavesList = data["best_waves"] as Godot.Collections.Array;
            if (bestWavesList != null)
            {
                foreach (var item in bestWavesList)
                {
                    var dict = item as Godot.Collections.Dictionary;
                    if (dict != null && dict.ContainsKey("arena_id") && dict.ContainsKey("waves"))
                    {
                        string arenaId = dict["arena_id"].ToString();
                        int waves = Convert.ToInt32(dict["waves"]);
                        _playerData.BestWaves[arenaId] = waves;
                    }
                }
            }
        }
        
        // Load battle counts
        if (data.ContainsKey("battle_counts"))
        {
            var battleCountList = data["battle_counts"] as Godot.Collections.Array;
            if (battleCountList != null)
            {
                foreach (var item in battleCountList)
                {
                    var dict = item as Godot.Collections.Dictionary;
                    if (dict != null && dict.ContainsKey("arena_id") && dict.ContainsKey("count"))
                    {
                        string arenaId = dict["arena_id"].ToString();
                        int count = Convert.ToInt32(dict["count"]);
                        _playerData.BattleCount[arenaId] = count;
                    }
                }
            }
        }
    }
    
    public MountBattleArenaData.PlayerMountArenaData GetStatistics()
    {
        return _playerData;
    }
    
    public float GetVictoryRate()
    {
        if (_playerData.TotalBattles == 0) return 0f;
        return (float)_playerData.Victories / _playerData.TotalBattles * 100f;
    }
}
