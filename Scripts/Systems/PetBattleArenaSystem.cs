using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 宠物战斗竞技场系统 - 宠物对战比赛管理
/// 支持排位赛、练习赛等多种模式
/// </summary>
public class PetBattleArenaSystem : BaseSystem
{
    // Singleton
    public static PetBattleArenaSystem Instance { get; private set; }

    // Battle state
    private enum BattleState { Idle, Preparation, InProgress, Completed }
    private BattleState _state = BattleState.Idle;
    
    // Arena configuration
    private int _arenaLevel = 1;
    private int _maxArenaLevel = 10;
    private int _unlockedArenas = 1;
    
    // Battle tracking
    private int _totalBattles = 0;
    private int _wins = 0;
    private int _losses = 0;
    private int _currentStreak = 0;
    private int _bestStreak = 0;
    private int _rankingPoints = 0;
    private int _rank = 500;
    
    // Current battle
    private PetInstance _playerPet;
    private PetInstance _enemyPet;
    private int _playerScore = 0;
    private int _enemyScore = 0;
    private int _roundsPlayed = 0;
    private int _maxRounds = 5;
    
    // Rewards
    private int _goldReward = 0;
    private int _expReward = 0;
    
    // Arena types
    public enum ArenaType
    {
        TrainingGround,
        SilverArena,
        GoldArena,
        PlatinumArena,
        DiamondArena,
        ChampionArena,
        LegendArena,
        MythicArena,
        DivineArena,
        CelestialArena
    }
    
    // Pet battle stats
    public class PetBattleStats
    {
        public int TotalBattles;
        public int Wins;
        public int Losses;
        public int BestStreak;
        public int RankingPoints;
        public int Rank;
        public int HighestRank;
        public Dictionary<string, int> FavoritePets = new Dictionary<string, int>();
    }
    
    private PetBattleStats _battleStats = new PetBattleStats();
    
    // Signal for battle events
    [Signal]
    public delegate void BattleStarted(PetInstance playerPet, PetInstance enemyPet);
    
    [Signal]
    public delegate void BattleRoundComplete(int round, int playerScore, int enemyScore);
    
    [Signal]
    public delegate void BattleCompleted(bool victory, int goldReward, int expReward);
    
    [Signal]
    public delegate void RankUpdated(int newRank);
    
    public override void _Ready()
    {
        Instance = this;
    }
    
    public override void _Process(float delta)
    {
        if (_state == BattleState.InProgress)
        {
            // Auto-resolve battle after delay
        }
    }
    
    // Initialize system
    public void Initialize()
    {
        LoadData();
    }
    
    // Get arena info
    public Dictionary<string, object> GetArenaInfo()
    {
        return new Dictionary<string, object>
        {
            { "arenaLevel", _arenaLevel },
            { "unlockedArenas", _unlockedArenas },
            { "totalBattles", _totalBattles },
            { "wins", _wins },
            { "losses", _losses },
            { "currentStreak", _currentStreak },
            { "bestStreak", _bestStreak },
            { "rankingPoints", _rankingPoints },
            { "rank", _rank },
            { "winRate", _totalBattles > 0 ? (float)_wins / _totalBattles * 100 : 0 }
        };
    }
    
    // Get current battle status
    public Dictionary<string, object> GetCurrentBattle()
    {
        return new Dictionary<string, object>
        {
            { "state", _state.ToString() },
            { "playerPet", _playerPet != null ? _playerPet.Name : "None" },
            { "enemyPet", _enemyPet != null ? _enemyPet.Name : "None" },
            { "playerScore", _playerScore },
            { "enemyScore", _enemyScore },
            { "roundsPlayed", _roundsPlayed },
            { "maxRounds", _maxRounds }
        };
    }
    
    // Start a battle
    public bool StartBattle(PetInstance playerPet, ArenaType arenaType)
    {
        if (_state != BattleState.Idle)
            return false;
            
        if (playerPet == null)
            return false;
            
        // Check arena unlock
        int arenaIndex = (int)arenaType;
        if (arenaIndex >= _unlockedArenas)
            return false;
            
        _playerPet = playerPet;
        _enemyPet = GenerateEnemyPet(arenaType);
        _playerScore = 0;
        _enemyScore = 0;
        _roundsPlayed = 0;
        
        // Calculate rewards based on arena
        CalculateRewards(arenaType);
        
        _state = BattleState.Preparation;
        EmitSignal(nameof(BattleStarted), _playerPet, _enemyPet);
        
        return true;
    }
    
    // Generate enemy pet
    private PetInstance GenerateEnemyPet(ArenaType arenaType)
    {
        var enemy = new PetInstance();
        
        // Scale enemy based on arena
        int basePower = 50 + (int)arenaType * 30;
        int powerVariance = (int)(basePower * 0.2f);
        int actualPower = basePower + GD.RandRange(-powerVariance, powerVariance);
        
        // Set enemy stats
        enemy.Name = GetRandomEnemyName(arenaType);
        enemy.Attack = actualPower * 2;
        enemy.Defense = actualPower;
        enemy.Health = actualPower * 10;
        enemy.Speed = actualPower / 2;
        
        // Set enemy type
        string[] petTypes = { "Wolf", "Bear", "Eagle", "Fox", "Tiger", "Lion", "Dragon", "Phoenix" };
        enemy.PetType = petTypes[GD.RandI() % petTypes.Length];
        
        enemy.Level = _arenaLevel * 5 + GD.RandI() % 5;
        
        return enemy;
    }
    
    // Get random enemy name
    private string GetRandomEnemyName(ArenaType arenaType)
    {
        string[] prefixes = { "Fierce", "Mighty", "Swift", "Ancient", "Shadow", "Thunder", "Frost", "Infernal" };
        string[] names = { "Beast", "Champion", "Warrior", "Guardian", "Hunter", "Striker", "Raider", "Conqueror" };
        
        string prefix = prefixes[GD.RandI() % prefixes.Length];
        string name = names[GD.RandI() % names.Length];
        
        return $"{prefix} {name}";
    }
    
    // Calculate rewards
    private void CalculateRewards(ArenaType arenaType)
    {
        int baseGold = 50 + (int)arenaType * 25;
        int baseExp = 30 + (int)arenaType * 20;
        
        // Streak bonus
        float streakMultiplier = 1.0f + (_currentStreak * 0.1f);
        
        _goldReward = (int)(baseGold * streakMultiplier);
        _expReward = (int)(baseExp * streakMultiplier);
    }
    
    // Play round
    public void PlayRound()
    {
        if (_state != BattleState.Preparation && _state != BattleState.InProgress)
            return;
            
        if (_roundsPlayed >= _maxRounds)
        {
            EndBattle();
            return;
        }
        
        _state = BattleState.InProgress;
        _roundsPlayed++;
        
        // Calculate round outcome
        int playerRoll = GD.RandI() % 100 + _playerPet.Speed + (_playerPet.Attack / 10);
        int enemyRoll = GD.RandI() % 100 + _enemyPet.Speed + (_enemyPet.Attack / 10);
        
        // Critical hit chance
        bool playerCrit = GD.RandI() % 100 < 10;
        bool enemyCrit = GD.RandI() % 100 < 10;
        
        int playerDamage = (_playerPet.Attack - _enemyPet.Defense / 2);
        int enemyDamage = (_enemyPet.Attack - _playerPet.Defense / 2);
        
        if (playerCrit) playerDamage = (int)(playerDamage * 1.5f);
        if (enemyCrit) enemyDamage = (int)(enemyDamage * 1.5f);
        
        if (playerDamage < 1) playerDamage = 1;
        if (enemyDamage < 1) enemyDamage = 1;
        
        // Determine winner
        if (playerRoll > enemyRoll)
        {
            _playerScore++;
            _enemyPet.Health -= playerDamage;
        }
        else
        {
            _enemyScore++;
            _playerPet.Health -= enemyDamage;
        }
        
        EmitSignal(nameof(BattleRoundComplete), _roundsPlayed, _playerScore, _enemyScore);
        
        // Check for early victory (best of 3)
        if (_playerScore >= 3 || _enemyScore >= 3)
        {
            EndBattle();
        }
        else if (_roundsPlayed >= _maxRounds)
        {
            EndBattle();
        }
    }
    
    // Auto-play entire battle
    public void AutoPlayBattle()
    {
        if (_state == BattleState.Idle)
            return;
            
        while (_state == BattleState.Preparation || _state == BattleState.InProgress)
        {
            if (_playerScore >= 3 || _enemyScore >= 3 || _roundsPlayed >= _maxRounds)
            {
                EndBattle();
                break;
            }
            PlayRound();
        }
    }
    
    // End battle
    private void EndBattle()
    {
        _state = BattleState.Completed;
        
        bool victory = _playerScore > _enemyScore;
        
        if (victory)
        {
            _wins++;
            _currentStreak++;
            if (_currentStreak > _bestStreak)
                _bestStreak = _currentStreak;
                
            // Ranking points
            int pointsGain = 25 + (_arenaLevel * 5);
            if (_currentStreak > 3)
                pointsGain += _currentStreak * 2;
                
            _rankingPoints += pointsGain;
            
            // Update rank
            int newRank = Math.Max(1, _rank - (pointsGain / 10));
            if (newRank < _rank)
            {
                _rank = newRank;
                EmitSignal(nameof(RankUpdated), _rank);
            }
        }
        else
        {
            _losses++;
            _currentStreak = 0;
            
            // Lose points
            int pointsLoss = 10 + (_arenaLevel * 2);
            _rankingPoints = Math.Max(0, _rankingPoints - pointsLoss);
            _rank = Math.Min(1000, _rank + (pointsLoss / 10));
        }
        
        _totalBattles++;
        
        // Track favorite pet
        if (_playerPet != null)
        {
            string petName = _playerPet.Name;
            if (!_battleStats.FavoritePets.ContainsKey(petName))
                _battleStats.FavoritePets[petName] = 0;
            _battleStats.FavoritePets[petName]++;
        }
        
        // Update stats
        _battleStats.TotalBattles = _totalBattles;
        _battleStats.Wins = _wins;
        _battleStats.Losses = _losses;
        _battleStats.BestStreak = _bestStreak;
        _battleStats.RankingPoints = _rankingPoints;
        _battleStats.Rank = _rank;
        
        SaveData();
        EmitSignal(nameof(BattleCompleted), victory, _goldReward, _expReward);
        
        _state = BattleState.Idle;
    }
    
    // Unlock arena
    public bool UnlockArena(int arenaIndex)
    {
        if (arenaIndex <= _unlockedArenas)
            return false;
            
        int cost = arenaIndex * 1000;
        
        // Check if player has enough gold (need to integrate with economy system)
        _unlockedArenas = Math.Min(arenaIndex + 1, _maxArenaLevel);
        
        SaveData();
        return true;
    }
    
    // Upgrade arena
    public bool UpgradeArena()
    {
        if (_arenaLevel >= _maxArenaLevel)
            return false;
            
        _arenaLevel++;
        SaveData();
        return true;
    }
    
    // Get battle stats
    public PetBattleStats GetBattleStats()
    {
        return _battleStats;
    }
    
    // Get arena types info
    public List<Dictionary<string, object>> GetArenaTypes()
    {
        var arenas = new List<Dictionary<string, object>>();
        
        string[] arenaNames = {
            "Training Ground", "Silver Arena", "Gold Arena", "Platinum Arena",
            "Diamond Arena", "Champion Arena", "Legend Arena", "Mythic Arena",
            "Divine Arena", "Celestial Arena"
        };
        
        int[] arenaCosts = { 0, 500, 1500, 4000, 8000, 15000, 30000, 50000, 80000, 120000 };
        
        for (int i = 0; i < 10; i++)
        {
            arenas.Add(new Dictionary<string, object>
            {
                { "name", arenaNames[i] },
                { "index", i },
                { "unlocked", i < _unlockedArenas },
                { "cost", arenaCosts[i] },
                { "difficulty", i + 1 }
            });
        }
        
        return arenas;
    }
    
    // Save data
    public void SaveData()
    {
        // Save to game data
        if (GameData.Instance != null)
        {
            GameData.Instance.SaveData("pet_battle_arena", new Dictionary<string, object>
            {
                { "arenaLevel", _arenaLevel },
                { "unlockedArenas", _unlockedArenas },
                { "totalBattles", _totalBattles },
                { "wins", _wins },
                { "losses", _losses },
                { "currentStreak", _currentStreak },
                { "bestStreak", _bestStreak },
                { "rankingPoints", _rankingPoints },
                { "rank", _rank }
            });
        }
    }
    
    // Load data
    public void LoadData()
    {
        if (GameData.Instance != null)
        {
            var data = GameData.Instance.GetData("pet_battle_arena") as Dictionary<string, object>;
            if (data != null)
            {
                _arenaLevel = data.ContainsKey("arenaLevel") ? (int)data["arenaLevel"] : 1;
                _unlockedArenas = data.ContainsKey("unlockedArenas") ? (int)data["unlockedArenas"] : 1;
                _totalBattles = data.ContainsKey("totalBattles") ? (int)data["totalBattles"] : 0;
                _wins = data.ContainsKey("wins") ? (int)data["wins"] : 0;
                _losses = data.ContainsKey("losses") ? (int)data["losses"] : 0;
                _currentStreak = data.ContainsKey("currentStreak") ? (int)data["currentStreak"] : 0;
                _bestStreak = data.ContainsKey("bestStreak") ? (int)data["bestStreak"] : 0;
                _rankingPoints = data.ContainsKey("rankingPoints") ? (int)data["rankingPoints"] : 0;
                _rank = data.ContainsKey("rank") ? (int)data["rank"] : 500;
            }
        }
    }
    
    // Reset battle
    public void ResetBattle()
    {
        _state = BattleState.Idle;
        _playerPet = null;
        _enemyPet = null;
        _playerScore = 0;
        _enemyScore = 0;
        _roundsPlayed = 0;
    }
}
