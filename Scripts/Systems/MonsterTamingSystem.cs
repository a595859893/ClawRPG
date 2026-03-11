using Godot;
using Godot.Collections;
using System;

public partial class MonsterTamingSystem : Node
{
    public static MonsterTamingSystem Instance { get; private set; }
    
    // Tameable monster types
    public enum MonsterType
    {
        Wolf, Bear, Eagle, Fox, Tiger, Lion,
        Dragon, Phoenix, Griffin, Unicorn,
        Snake, Spider, Scorpion, Bat,
        Skeleton, Ghost, Slime, Golem
    }
    
    // Monster rarity
    public enum MonsterRarity
    {
        Common, Uncommon, Rare, Epic, Legendary
    }
    
    // Taming state
    public enum TamingState
    {
        Idle, Attempting, Success, Failed
    }
    
    // Monster data structure
    public class TameableMonster
    {
        public MonsterType Type { get; set; }
        public MonsterRarity Rarity { get; set; }
        public string Name { get; set; }
        public int Level { get; set; }
        public float Health { get; set; }
        public float Attack { get; set; }
        public float Defense { get; set; }
        public float Speed { get; set; }
        public float TameProgress { get; set; }
        public TamingState State { get; set; }
        public int TameAttempts { get; set; }
        public DateTime LastAttemptTime { get; set; }
        public bool IsTamed { get; set; }
        public string TamedBy { get; set; }
    }
    
    // Taming method
    public enum TamingMethod
    {
        Feed, Battle, Play, Trade, Capture
    }
    
    // Configuration
    private Dictionary<MonsterType, Dictionary<MonsterRarity, Dictionary>> _monsterData;
    private Dictionary<TamingMethod, float> _methodEfficiency;
    private Array<TameableMonster> _wildMonsters;
    private Array<TameableMonster> _tamedMonsters;
    
    // Stats
    private int _totalTameAttempts;
    private int _successfulTames;
    private int _legendaryTames;
    
    public override void _Ready()
    {
        Instance = this;
        _wildMonsters = new Array<TameableMonster>();
        _tamedMonsters = new Array<TameableMonster>();
        InitializeData();
        GenerateWildMonsters();
    }
    
    private void InitializeData()
    {
        _monsterData = new Dictionary<MonsterType, Dictionary<MonsterRarity, Dictionary>>();
        
        // Initialize monster data for each type
        var types = new Array<MonsterType>((MonsterType[])Enum.GetValues(typeof(MonsterType)));
        var rarities = new Array<MonsterRarity>((MonsterRarity[])Enum.GetValues(typeof(MonsterRarity)));
        
        foreach (var type in types)
        {
            _monsterData[type] = new Dictionary<MonsterRarity, Dictionary>();
            
            foreach (var rarity in rarities)
            {
                var baseStats = GetBaseStats(type);
                var multiplier = GetRarityMultiplier(rarity);
                
                var data = new Dictionary
                {
                    { "name", GetMonsterName(type, rarity) },
                    { "base_health", (float)baseStats["health"] * multiplier },
                    { "base_attack", (float)baseStats["attack"] * multiplier },
                    { "base_defense", (float)baseStats["defense"] * multiplier },
                    { "base_speed", (float)baseStats["speed"] * multiplier },
                    { "tame_difficulty", GetTameDifficulty(type, rarity) },
                    { "required_level", GetRequiredLevel(rarity) },
                    { "gold_cost", GetGoldCost(rarity) }
                };
                
                _monsterData[type][rarity] = data;
            }
        }
        
        // Method efficiency
        _methodEfficiency = new Dictionary<TamingMethod, float>
        {
            { TamingMethod.Feed, 0.25f },
            { TamingMethod.Battle, 0.35f },
            { TamingMethod.Play, 0.20f },
            { TamingMethod.Trade, 0.40f },
            { TamingMethod.Capture, 0.50f }
        };
    }
    
    private Dictionary GetBaseStats(MonsterType type)
    {
        switch (type)
        {
            case MonsterType.Wolf: return new Dictionary { { "health", 100f }, { "attack", 15f }, { "defense", 8f }, { "speed", 12f } };
            case MonsterType.Bear: return new Dictionary { { "health", 180f }, { "attack", 20f }, { "defense", 15f }, { "speed", 6f } };
            case MonsterType.Eagle: return new Dictionary { { "health", 80f }, { "attack", 18f }, { "defense", 6f }, { "speed", 18f } };
            case MonsterType.Fox: return new Dictionary { { "health", 90f }, { "attack", 16f }, { "defense", 7f }, { "speed", 15f } };
            case MonsterType.Tiger: return new Dictionary { { "health", 150f }, { "attack", 22f }, { "defense", 10f }, { "speed", 14f } };
            case MonsterType.Lion: return new Dictionary { { "health", 170f }, { "attack", 24f }, { "defense", 12f }, { "speed", 13f } };
            case MonsterType.Dragon: return new Dictionary { { "health", 300f }, { "attack", 35f }, { "defense", 25f }, { "speed", 15f } };
            case MonsterType.Phoenix: return new Dictionary { { "health", 250f }, { "attack", 32f }, { "defense", 20f }, { "speed", 20f } };
            case MonsterType.Griffin: return new Dictionary { { "health", 280f }, { "attack", 33f }, { "defense", 22f }, { "speed", 18f } };
            case MonsterType.Unicorn: return new Dictionary { { "health", 220f }, { "attack", 28f }, { "defense", 18f }, { "speed", 22f } };
            case MonsterType.Snake: return new Dictionary { { "health", 70f }, { "attack", 14f }, { "defense", 5f }, { "speed", 16f } };
            case MonsterType.Spider: return new Dictionary { { "health", 60f }, { "attack", 12f }, { "defense", 4f }, { "speed", 14f } };
            case MonsterType.Scorpion: return new Dictionary { { "health", 85f }, { "attack", 17f }, { "defense", 9f }, { "speed", 11f } };
            case MonsterType.Bat: return new Dictionary { { "health", 50f }, { "attack", 10f }, { "defense", 3f }, { "speed", 17f } };
            case MonsterType.Skeleton: return new Dictionary { { "health", 95f }, { "attack", 13f }, { "defense", 11f }, { "speed", 9f } };
            case MonsterType.Ghost: return new Dictionary { { "health", 75f }, { "attack", 19f }, { "defense", 4f }, { "speed", 21f } };
            case MonsterType.Slime: return new Dictionary { { "health", 120f }, { "attack", 8f }, { "defense", 12f }, { "speed", 7f } };
            case MonsterType.Golem: return new Dictionary { { "health", 200f }, { "attack", 11f }, { "defense", 28f }, { "speed", 4f } };
            default: return new Dictionary { { "health", 100f }, { "attack", 10f }, { "defense", 10f }, { "speed", 10f } };
        }
    }
    
    private float GetRarityMultiplier(MonsterRarity rarity)
    {
        switch (rarity)
        {
            case MonsterRarity.Common: return 1.0f;
            case MonsterRarity.Uncommon: return 1.3f;
            case MonsterRarity.Rare: return 1.7f;
            case MonsterRarity.Epic: return 2.2f;
            case MonsterRarity.Legendary: return 3.0f;
            default: return 1.0f;
        }
    }
    
    private float GetTameDifficulty(MonsterType type, MonsterRarity rarity)
    {
        float baseDifficulty = 0.3f;
        float rarityBonus = (float)rarity * 0.15f;
        return baseDifficulty + rarityBonus;
    }
    
    private int GetRequiredLevel(MonsterRarity rarity)
    {
        switch (rarity)
        {
            case MonsterRarity.Common: return 1;
            case MonsterRarity.Uncommon: return 5;
            case MonsterRarity.Rare: return 10;
            case MonsterRarity.Epic: return 20;
            case MonsterRarity.Legendary: return 35;
            default: return 1;
        }
    }
    
    private int GetGoldCost(MonsterRarity rarity)
    {
        switch (rarity)
        {
            case MonsterRarity.Common: return 100;
            case MonsterRarity.Uncommon: return 300;
            case MonsterRarity.Rare: return 800;
            case MonsterRarity.Epic: return 2000;
            case MonsterRarity.Legendary: return 5000;
            default: return 100;
        }
    }
    
    private string GetMonsterName(MonsterType type, MonsterRarity rarity)
    {
        string rarityPrefix = "";
        switch (rarity)
        {
            case MonsterRarity.Common: rarityPrefix = ""; break;
            case MonsterRarity.Uncommon: rarityPrefix = "Elite "; break;
            case MonsterRarity.Rare: rarityPrefix = "Rare "; break;
            case MonsterRarity.Epic: rarityPrefix = "Epic "; break;
            case MonsterRarity.Legendary: rarityPrefix = "Ancient "; break;
        }
        
        string typeName = type.ToString().Replace("_", " ");
        return rarityPrefix + typeName;
    }
    
    private void GenerateWildMonsters()
    {
        var random = new RandomNumberGenerator();
        random.Randomize();
        
        int monsterCount = random.RandiRange(5, 15);
        
        var types = new Array<MonsterType>((MonsterType[])Enum.GetValues(typeof(MonsterType)));
        var rarities = new Array<MonsterRarity>((MonsterRarity[])Enum.GetValues(typeof(MonsterRarity)));
        
        for (int i = 0; i < monsterCount; i++)
        {
            var type = types[random.RandiRange(0, types.Count - 1)];
            
            // Weight rarities
            float roll = random.Randf();
            MonsterRarity rarity;
            if (roll < 0.50f) rarity = MonsterRarity.Common;
            else if (roll < 0.80f) rarity = MonsterRarity.Uncommon;
            else if (roll < 0.93f) rarity = MonsterRarity.Rare;
            else if (roll < 0.98f) rarity = MonsterRarity.Epic;
            else rarity = MonsterRarity.Legendary;
            
            var monster = CreateMonster(type, rarity, random);
            _wildMonsters.Add(monster);
        }
    }
    
    private TameableMonster CreateMonster(MonsterType type, MonsterRarity rarity, RandomNumberGenerator random)
    {
        var data = _monsterData[type][rarity];
        var baseStats = GetBaseStats(type);
        var multiplier = GetRarityMultiplier(rarity);
        
        return new TameableMonster
        {
            Type = type,
            Rarity = rarity,
            Name = (string)data["name"],
            Level = random.RandiRange(1, 30),
            Health = (float)data["base_health"] * (0.8f + random.Randf() * 0.4f),
            Attack = (float)data["base_attack"] * (0.8f + random.Randf() * 0.4f),
            Defense = (float)data["base_defense"] * (0.8f + random.Randf() * 0.4f),
            Speed = (float)data["base_speed"] * (0.8f + random.Randf() * 0.4f),
            TameProgress = 0f,
            State = TamingState.Idle,
            TameAttempts = 0,
            LastAttemptTime = DateTime.MinValue,
            IsTamed = false,
            TamedBy = ""
        };
    }
    
    public bool AttemptTame(TameableMonster monster, TamingMethod method, string playerId, int playerLevel, int playerGold)
    {
        if (monster.IsTamed)
        {
            GD.Print("Monster is already tamed!");
            return false;
        }
        
        var data = _monsterData[monster.Type][monster.Rarity];
        int requiredLevel = (int)data["required_level"];
        int goldCost = (int)data["gold_cost"];
        
        if (playerLevel < requiredLevel)
        {
            GD.Print($"Player level {playerLevel} is too low. Required: {requiredLevel}");
            return false;
        }
        
        if (playerGold < goldCost)
        {
            GD.Print($"Not enough gold. Required: {goldCost}, Have: {playerGold}");
            return false;
        }
        
        float methodEfficiency = _methodEfficiency[method];
        float difficulty = GetTameDifficulty(monster.Type, monster.Rarity);
        float successChance = methodEfficiency * (1.0f - difficulty) * (1.0f - monster.TameProgress);
        
        // Bonus for repeated attempts
        successChance += monster.TameAttempts * 0.05f;
        
        // Cap success rate
        successChance = Mathf.Clamp(successChance, 0.05f, 0.85f);
        
        var random = new RandomNumberGenerator();
        random.Randomize();
        
        _totalTameAttempts++;
        monster.TameAttempts++;
        monster.LastAttemptTime = DateTime.Now;
        
        if (random.Randf() < successChance)
        {
            // Success!
            monster.IsTamed = true;
            monster.TamedBy = playerId;
            monster.State = TamingState.Success;
            _successfulTames++;
            
            if (monster.Rarity == MonsterRarity.Legendary)
            {
                _legendaryTames++;
            }
            
            // Move from wild to tamed
            _wildMonsters.Remove(monster);
            _tamedMonsters.Add(monster);
            
            GD.Print($"Successfully tamed {monster.Name}!");
            return true;
        }
        else
        {
            // Failed - increase progress
            monster.TameProgress = Mathf.Clamp(monster.TameProgress + 0.1f, 0f, 0.9f);
            monster.State = TamingState.Failed;
            
            GD.Print($"Failed to tame {monster.Name}. Progress: {monster.TameProgress * 100}%");
            return false;
        }
    }
    
    public void ReleaseMonster(TameableMonster monster)
    {
        if (!monster.IsTamed) return;
        
        monster.IsTamed = false;
        monster.TamedBy = "";
        monster.TameProgress = 0f;
        monster.TameAttempts = 0;
        
        _tamedMonsters.Remove(monster);
        
        var random = new RandomNumberGenerator();
        random.Randomize();
        _wildMonsters.Add(CreateMonster(monster.Type, monster.Rarity, random));
    }
    
    public Array<TameableMonster> GetWildMonsters() => _wildMonsters;
    public Array<TameableMonster> GetTamedMonsters() => _tamedMonsters;
    
    public Dictionary GetTamingStats()
    {
        return new Dictionary
        {
            { "total_attempts", _totalTameAttempts },
            { "successful_tames", _successfulTames },
            { "legendary_tames", _legendaryTames },
            { "success_rate", _totalTameAttempts > 0 ? (float)_successfulTames / _totalTameAttempts : 0f },
            { "wild_count", _wildMonsters.Count },
            { "tamed_count", _tamedMonsters.Count }
        };
    }
    
    public void RefreshWildMonsters()
    {
        _wildMonsters.Clear();
        GenerateWildMonsters();
    }
    
    public Dictionary SaveData()
    {
        var data = new Dictionary
        {
            { "total_tame_attempts", _totalTameAttempts },
            { "successful_tames", _successfulTames },
            { "legendary_tames", _legendaryTames }
        };
        
        var tamedList = new Array<Dictionary>();
        foreach (var monster in _tamedMonsters)
        {
            tamedList.Add(new Dictionary
            {
                { "type", (int)monster.Type },
                { "rarity", (int)monster.Rarity },
                { "name", monster.Name },
                { "level", monster.Level },
                { "health", monster.Health },
                { "attack", monster.Attack },
                { "defense", monster.Defense },
                { "speed", monster.Speed }
            });
        }
        data["tamed_monsters"] = tamedList;
        
        return data;
    }
    
    public void LoadData(Dictionary data)
    {
        if (data.Contains("total_tame_attempts"))
            _totalTameAttempts = (int)data["total_tame_attempts"];
        if (data.Contains("successful_tames"))
            _successfulTames = (int)data["successful_tames"];
        if (data.Contains("legendary_tames"))
            _legendaryTames = (int)data["legendary_tames"];
        
        // Load tamed monsters
        if (data.Contains("tamed_monsters"))
        {
            var tamedList = (Array)data["tamed_monsters"];
            foreach (Dictionary monsterData in tamedList)
            {
                var monster = new TameableMonster
                {
                    Type = (MonsterType)(int)monsterData["type"],
                    Rarity = (MonsterRarity)(int)monsterData["rarity"],
                    Name = (string)monsterData["name"],
                    Level = (int)monsterData["level"],
                    Health = (float)monsterData["health"],
                    Attack = (float)monsterData["attack"],
                    Defense = (float)monsterData["defense"],
                    Speed = (float)monsterData["speed"],
                    IsTamed = true
                };
                _tamedMonsters.Add(monster);
            }
        }
    }
}
