using Godot;
using System;
using System.Collections.Generic;

public partial class DungeonExpeditionSystem : BaseSystem
{
    public static DungeonExpeditionSystem Instance { get; private set; }

    // Dungeon Types
    public enum DungeonType
    {
        AncientRuins,      // 远古遗迹
        CrystalCavern,     // 水晶洞窟
        ShadowCrypt,       // 暗影墓穴
        DragonLair,        // 巨龙巢穴
        FrozenFortress,    // 冰霜堡垒
        VolcanicDepths,    // 火山深处
        EnchantedForest,   // 魔法森林
        AbyssalPit,        // 深渊坑道
        HeavenlyTemple,    // 神圣殿堂
        DemonCastle        // 恶魔城堡
    }

    // Difficulty Levels
    public enum Difficulty
    {
        Normal,     // 普通
        Hard,       // 困难
        Nightmare,  // 噩梦
        Hell,       // 地狱
        Inferno     // 炼狱
    }

    // Expedition Status
    public enum ExpeditionStatus
    {
        Available,      // 可用
        InProgress,     // 进行中
        Completed,      // 完成
        Failed          // 失败
    }

    // Dungeon Data
    [System.Serializable]
    public class DungeonData
    {
        public DungeonType Type;
        public string Name;
        public string Description;
        public Difficulty Difficulty;
        public int RecommendedLevel;
        public int FloorCount;          // 楼层数
        public float EnemyScale;        // 敌人缩放
        public float RewardScale;       // 奖励缩放
        public List<string> Enemies;   // 敌人类型
        public List<string> Rewards;   // 奖励类型
    }

    // Expedition Record
    [System.Serializable]
    public class ExpeditionRecord
    {
        public int InstanceId;
        public DungeonType DungeonType;
        public Difficulty Difficulty;
        public ExpeditionStatus Status;
        public int CurrentFloor;
        public int MaxFloor;
        public int EnemiesDefeated;
        public int GoldEarned;
        public int ExpEarned;
        public List<string> ItemsEarned;
        public long StartTime;
        public long EndTime;
    }

    // Player Progress
    [System.Serializable]
    public class PlayerDungeonProgress
    {
        public Dictionary<DungeonType, bool> UnlockedDungeons = new();
        public Dictionary<DungeonType, int> BestFloor = new();
        public Dictionary<DungeonType, int> TotalWins = new();
        public Dictionary<DungeonType, int> TotalAttempts = new();
        public int TotalGoldEarned;
        public int TotalExpEarned;
    }

    // Current expedition
    private ExpeditionRecord _currentExpedition;
    private PlayerDungeonProgress _playerProgress;

    // Dungeon definitions
    private List<DungeonData> _dungeonDefinitions;

    public override void _Ready()
    {
        Instance = this;
        InitializeDungeonDefinitions();
        LoadProgress();
    }

    private void InitializeDungeonDefinitions()
    {
        _dungeonDefinitions = new List<DungeonData>
        {
            new DungeonData
            {
                Type = DungeonType.AncientRuins,
                Name = "Ancient Ruins",
                Description = "Remnants of an ancient civilization, filled with forgotten treasures",
                Difficulty = Difficulty.Normal,
                RecommendedLevel = 1,
                FloorCount = 10,
                EnemyScale = 1.0f,
                RewardScale = 1.0f,
                Enemies = new List<string> { "Skeleton", "Zombie", "Ghost" },
                Rewards = new List<string> { "Gold", "Experience", "Equipment" }
            },
            new DungeonData
            {
                Type = DungeonType.CrystalCavern,
                Name = "Crystal Cavern",
                Description = "A glittering cave system with crystal formations and gem creatures",
                Difficulty = Difficulty.Normal,
                RecommendedLevel = 5,
                FloorCount = 15,
                EnemyScale = 1.2f,
                RewardScale = 1.3f,
                Enemies = new List<string> { "CrystalGolem", "GemSpider", "MineralWraith" },
                Rewards = new List<string> { "Gems", "Gold", "Materials" }
            },
            new DungeonData
            {
                Type = DungeonType.ShadowCrypt,
                Name = "Shadow Crypt",
                Description = "A dark burial ground haunted by restless spirits",
                Difficulty = Difficulty.Hard,
                RecommendedLevel = 15,
                FloorCount = 20,
                EnemyScale = 1.5f,
                RewardScale = 1.6f,
                Enemies = new List<string> { "ShadowKnight", "DeathKnight", "SoulDevourer" },
                Rewards = new List<string> { "DarkArtifacts", "ShadowWeapons", "RareGems" }
            },
            new DungeonData
            {
                Type = DungeonType.DragonLair,
                Name = "Dragon Lair",
                Description = "The巢穴 of powerful dragons, guarding ancient treasures",
                Difficulty = Difficulty.Hard,
                RecommendedLevel = 25,
                FloorCount = 25,
                EnemyScale = 1.8f,
                RewardScale = 2.0f,
                Enemies = new List<string> { "DragonWhelp", "Drake", "ElderDragon" },
                Rewards = new List<string> { "DragonScales", "DragonBlood", "LegendaryEquipment" }
            },
            new DungeonData
            {
                Type = DungeonType.FrozenFortress,
                Name = "Frozen Fortress",
                Description = "An icy fortress defended by frost giants and ice elementals",
                Difficulty = Difficulty.Nightmare,
                RecommendedLevel = 35,
                FloorCount = 30,
                EnemyScale = 2.2f,
                RewardScale = 2.5f,
                Enemies = new List<string> { "FrostGiant", "IceElemental", "WinterWolf" },
                Rewards = new List<string> { "IceCrystals", "FrostWeapons", "FrozenTreasures" }
            },
            new DungeonData
            {
                Type = DungeonType.VolcanicDepths,
                Name = "Volcanic Depths",
                Description = "The burning heart of a volcano, home to fire elementals",
                Difficulty = Difficulty.Nightmare,
                RecommendedLevel = 45,
                FloorCount = 35,
                EnemyScale = 2.6f,
                RewardScale = 3.0f,
                Enemies = new List<string> { "FireElemental", "LavaGolem", "Phoenix" },
                Rewards = new List<string> { "FireGems", "MoltenOre", "VolcanicArtifacts" }
            },
            new DungeonData
            {
                Type = DungeonType.EnchantedForest,
                Name = "Enchanted Forest",
                Description = "A mystical forest filled with magical creatures and fairy secrets",
                Difficulty = Difficulty.Hell,
                RecommendedLevel = 55,
                FloorCount = 40,
                EnemyScale = 3.0f,
                RewardScale = 3.5f,
                Enemies = new List<string> { "TreantGuardian", "ForestSpirit", "UnicornKnight" },
                Rewards = new List<string> { "MagicSeeds", "FairyDust", "NatureArtifacts" }
            },
            new DungeonData
            {
                Type = DungeonType.AbyssalPit,
                Name = "Abyssal Pit",
                Description = "A terrifying descent into the abyss, where darkness reigns",
                Difficulty = Difficulty.Hell,
                RecommendedLevel = 65,
                FloorCount = 50,
                EnemyScale = 3.5f,
                RewardScale = 4.0f,
                Enemies = new List<string> { "AbyssCreature", "VoidWalker", "DarkMatter" },
                Rewards = new List<string> { "VoidEssence", "DarkMatter", "AbyssalWeapons" }
            },
            new DungeonData
            {
                Type = DungeonType.HeavenlyTemple,
                Name = "Heavenly Temple",
                Description = "A sacred temple guarded by celestial beings",
                Difficulty = Difficulty.Inferno,
                RecommendedLevel = 75,
                FloorCount = 60,
                EnemyScale = 4.0f,
                RewardScale = 5.0f,
                Enemies = new List<string> { "CelestialGuard", "Angel", "Seraph" },
                Rewards = new List<string> { "HolyRelics", "DivineWeapons", "CelestialGems" }
            },
            new DungeonData
            {
                Type = DungeonType.DemonCastle,
                Name = "Demon Castle",
                Description = "The fortress of the demon lord, ultimate challenge",
                Difficulty = Difficulty.Inferno,
                RecommendedLevel = 90,
                FloorCount = 100,
                EnemyScale = 5.0f,
                RewardScale = 6.0f,
                Enemies = new List<string> { "DemonSoldier", "DemonGeneral", "DemonLord" },
                Rewards = new List<string> { "DemonSouls", "LegendaryArtifacts", "UltimateWeapons" }
            }
        };
    }

    public void LoadProgress()
    {
        _playerProgress = new PlayerDungeonProgress();
        
        // Unlock first dungeon by default
        _playerProgress.UnlockedDungeons[DungeonType.AncientRuins] = true;
        
        // Try to load from save
        SaveSystem.LoadObject("dungeon_progress", out PlayerDungeonProgress saved);
        if (saved != null)
        {
            _playerProgress = saved;
        }
    }

    public void SaveProgress()
    {
        SaveSystem.SaveObject("dungeon_progress", _playerProgress);
    }

    // Start a new expedition
    public bool StartExpedition(DungeonType type, Difficulty difficulty)
    {
        if (_currentExpedition != null && _currentExpedition.Status == ExpeditionStatus.InProgress)
        {
            GD.Print("Cannot start new expedition while one is in progress");
            return false;
        }

        if (!_playerProgress.UnlockedDungeons.ContainsKey(type) || !_playerProgress.UnlockedDungeons[type])
        {
            GD.Print("Dungeon type not unlocked");
            return false;
        }

        var dungeon = GetDungeonData(type);
        if (dungeon == null) return false;

        _currentExpedition = new ExpeditionRecord
        {
            InstanceId = (int)OS.GetSystemTimeMsecs(),
            DungeonType = type,
            Difficulty = difficulty,
            Status = ExpeditionStatus.InProgress,
            CurrentFloor = 1,
            MaxFloor = dungeon.FloorCount,
            EnemiesDefeated = 0,
            GoldEarned = 0,
            ExpEarned = 0,
            ItemsEarned = new List<string>(),
            StartTime = OS.GetSystemTimeMsecs()
        };

        if (!_playerProgress.TotalAttempts.ContainsKey(type))
            _playerProgress.TotalAttempts[type] = 0;
        _playerProgress.TotalAttempts[type]++;

        SaveProgress();
        
        // Emit signal
        EmitSignal(SignalName.ExpeditionStarted, (int)type, (int)difficulty);
        
        GD.Print($"Expedition started: {dungeon.Name} - {difficulty}");
        return true;
    }

    // Complete current floor
    public void CompleteFloor(int enemiesDefeated, int gold, int exp, List<string> items)
    {
        if (_currentExpedition == null || _currentExpedition.Status != ExpeditionStatus.InProgress)
            return;

        _currentExpedition.CurrentFloor++;
        _currentExpedition.EnemiesDefeated += enemiesDefeated;
        _currentExpedition.GoldEarned += gold;
        _currentExpedition.ExpEarned += exp;
        if (items != null)
            _currentExpedition.ItemsEarned.AddRange(items);

        // Update progress
        _playerProgress.TotalGoldEarned += gold;
        _playerProgress.TotalExpEarned += exp;

        if (_currentExpedition.CurrentFloor >= _currentExpedition.MaxFloor)
        {
            CompleteExpedition(true);
        }
        else
        {
            SaveProgress();
            EmitSignal(SignalName.FloorCompleted, _currentExpedition.CurrentFloor, _currentExpedition.MaxFloor);
        }
    }

    // Complete expedition
    public void CompleteExpedition(bool success)
    {
        if (_currentExpedition == null) return;

        _currentExpedition.Status = success ? ExpeditionStatus.Completed : ExpeditionStatus.Failed;
        _currentExpedition.EndTime = OS.GetSystemTimeMsecs();

        if (success)
        {
            var dungeon = GetDungeonData(_currentExpedition.DungeonType);
            
            // Update best floor
            if (!_playerProgress.BestFloor.ContainsKey(_currentExpedition.DungeonType) ||
                _currentExpedition.MaxFloor > _playerProgress.BestFloor[_currentExpedition.DungeonType])
            {
                _playerProgress.BestFloor[_currentExpedition.DungeonType] = _currentExpedition.MaxFloor;
            }

            // Update wins
            if (!_playerProgress.TotalWins.ContainsKey(_currentExpedition.DungeonType))
                _playerProgress.TotalWins[_currentExpedition.DungeonType] = 0;
            _playerProgress.TotalWins[_currentExpedition.DungeonType]++;

            // Unlock next dungeon if this is the hardest cleared
            UnlockNextDungeon(_currentExpedition.DungeonType);
        }

        SaveProgress();
        
        EmitSignal(SignalName.ExpeditionCompleted, 
            (int)_currentExpedition.DungeonType, 
            success ? 1 : 0,
            _currentExpedition.GoldEarned,
            _currentExpedition.ExpEarned);

        GD.Print($"Expedition completed: {success}, Gold: {_currentExpedition.GoldEarned}, Exp: {_currentExpedition.ExpEarned}");
    }

    // Abandon expedition
    public void AbandonExpedition()
    {
        if (_currentExpedition == null) return;

        _currentExpedition.Status = ExpeditionStatus.Failed;
        _currentExpedition.EndTime = OS.GetSystemTimeMsecs();
        
        SaveProgress();
        EmitSignal(SignalName.ExpeditionAbandoned, (int)_currentExpedition.DungeonType);
        
        GD.Print("Expedition abandoned");
    }

    // Unlock next dungeon
    private void UnlockNextDungeon(DungeonType current)
    {
        var allTypes = Enum.GetValues(typeof(DungeonType));
        int currentIndex = Array.IndexOf(allTypes, current);
        
        if (currentIndex < allTypes.Length - 1)
        {
            DungeonType nextType = (DungeonType)allTypes[currentIndex + 1];
            _playerProgress.UnlockedDungeons[nextType] = true;
        }
    }

    // Get dungeon data
    public DungeonData GetDungeonData(DungeonType type)
    {
        return _dungeonDefinitions.Find(d => d.Type == type);
    }

    // Get all dungeons
    public List<DungeonData> GetAllDungeons()
    {
        return _dungeonDefinitions;
    }

    // Get current expedition
    public ExpeditionRecord GetCurrentExpedition()
    {
        return _currentExpedition;
    }

    // Get player progress
    public PlayerDungeonProgress GetPlayerProgress()
    {
        return _playerProgress;
    }

    // Check if dungeon is unlocked
    public bool IsDungeonUnlocked(DungeonType type)
    {
        return _playerProgress.UnlockedDungeons.ContainsKey(type) && _playerProgress.UnlockedDungeons[type];
    }

    // Calculate rewards for floor
    public (int gold, int exp, List<string> items) CalculateFloorRewards(int floor)
    {
        if (_currentExpedition == null) return (0, 0, new List<string>());

        var dungeon = GetDungeonData(_currentExpedition.DungeonType);
        float scale = dungeon.RewardScale * (1 + floor * 0.1f);

        int gold = (int)(50 * scale * _currentExpedition.Difficulty switch
        {
            Difficulty.Normal => 1.0f,
            Difficulty.Hard => 1.5f,
            Difficulty.Nightmare => 2.0f,
            Difficulty.Hell => 3.0f,
            Difficulty.Inferno => 5.0f,
            _ => 1.0f
        });

        int exp = (int)(100 * scale * (int)_currentExpedition.Difficulty);

        var items = new List<string>();
        
        // Random item drops based on dungeon type
        if (Godot.RandomNumberGenerator.Randf() < 0.3f * scale)
        {
            items.Add(dungeon.Rewards[Godot.RandomNumberGenerator.Randi() % dungeon.Rewards.Count]);
        }

        return (gold, exp, items);
    }

    // Get difficulty name
    public static string GetDifficultyName(Difficulty difficulty)
    {
        return difficulty switch
        {
            Difficulty.Normal => "Normal",
            Difficulty.Hard => "Hard",
            Difficulty.Nightmare => "Nightmare",
            Difficulty.Hell => "Hell",
            Difficulty.Inferno => "Inferno",
            _ => "Unknown"
        };
    }

    // Signals
public delegate void ExpeditionStartedEventHandler(int dungeonType, int difficulty);
public delegate void FloorCompletedEventHandler(int currentFloor, int maxFloor);
public delegate void ExpeditionCompletedEventHandler(int dungeonType, int success, int gold, int exp);
public delegate void ExpeditionAbandonedEventHandler(int dungeonType);

    // ===== 持久化方法 =====

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        if (_currentExpedition != null)
        {
            data["current_expedition_type"] = (int)_currentExpedition.DungeonType;
            data["current_expedition_difficulty"] = (int)_currentExpedition.Difficulty;
            data["current_expedition_status"] = (int)_currentExpedition.Status;
            data["current_expedition_floor"] = _currentExpedition.CurrentFloor;
            data["current_expedition_enemies"] = _currentExpedition.EnemiesDefeated;
            data["current_expedition_start_time"] = _currentExpedition.StartTime.ToString("o");
        }
        
        if (_playerProgress != null)
        {
            data["total_expeditions"] = _playerProgress.TotalExpeditions;
            data["successful_expeditions"] = _playerProgress.SuccessfulExpeditions;
            data["total_gold_earned"] = _playerProgress.TotalGoldEarned;
            data["total_exp_earned"] = _playerProgress.TotalExpEarned;
            data["best_floor"] = _playerProgress.BestFloor;
            data["unlocked_dungeons"] = _playerProgress.UnlockedDungeons;
        }
        
        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        // 恢复当前远征
        if (data.Contains("current_expedition_type"))
        {
            _currentExpedition = new ExpeditionRecord();
            _currentExpedition.DungeonType = (DungeonType)(int)data["current_expedition_type"];
            _currentExpedition.Difficulty = (Difficulty)(int)data["current_expedition_difficulty"];
            _currentExpedition.Status = (ExpeditionStatus)(int)data["current_expedition_status"];
            _currentExpedition.CurrentFloor = (int)(data.GetValueOrDefault("current_expedition_floor", 0));
            _currentExpedition.EnemiesDefeated = (int)(data.GetValueOrDefault("current_expedition_enemies", 0));
            if (data.Contains("current_expedition_start_time"))
                _currentExpedition.StartTime = DateTime.Parse(data["current_expedition_start_time"].ToString());
        }
        
        // 恢复玩家进度
        _playerProgress = new PlayerDungeonProgress();
        _playerProgress.TotalExpeditions = (int)(data.GetValueOrDefault("total_expeditions", 0));
        _playerProgress.SuccessfulExpeditions = (int)(data.GetValueOrDefault("successful_expeditions", 0));
        _playerProgress.TotalGoldEarned = (int)(data.GetValueOrDefault("total_gold_earned", 0));
        _playerProgress.TotalExpEarned = (int)(data.GetValueOrDefault("total_exp_earned", 0));
        _playerProgress.BestFloor = (int)(data.GetValueOrDefault("best_floor", 0));
        
        if (data.Contains("unlocked_dungeons"))
            _playerProgress.UnlockedDungeons = (List<int>)data["unlocked_dungeons"];
    }
}
