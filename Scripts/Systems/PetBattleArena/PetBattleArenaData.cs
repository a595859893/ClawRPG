using Godot;
using System;
using System.Collections.Generic;

public class PetBattleArenaData : Resource
{
    public enum ArenaType
    {
        TrainingGround,
        BattleColosseum,
        DragonArena,
        PhoenixNest,
        ShadowRealm,
        SacredGround
    }

    public enum ArenaDifficulty
    {
        Easy,
        Normal,
        Hard,
        Epic,
        Legendary
    }

    public string ArenaId { get; set; }
    public string ArenaName { get; set; }
    public string Description { get; set; }
    public ArenaType Type { get; set; }
    public ArenaDifficulty Difficulty { get; set; }
    public int RecommendedLevel { get; set; }
    public int TotalWaves { get; set; }
    public int RewardGold { get; set; }
    public int RewardExp { get; set; }
    public string[] RewardItems { get; set; }
    public int UnlockLevel { get; set; }
}

public class PetBattleInstance
{
    public string PetId { get; set; }
    public int CurrentHealth { get; set; }
    public int MaxHealth { get; set; }
    public int Attack { get; set; }
    public int Defense { get; set; }
    public int Speed { get; set; }
    public int Level { get; set; }
    public int Experience { get; set; }
    public string[] EquippedSkills { get; set; }
}

public class EnemyWave
{
    public int WaveNumber { get; set; }
    public string EnemyId { get; set; }
    public int EnemyCount { get; set; }
    public int EnemyLevel { get; set; }
    public int EnemyHealth { get; set; }
    public int EnemyAttack { get; set; }
    public int EnemyDefense { get; set; }
    public float SpawnInterval { get; set; }
}

public class PlayerPetBattleData
{
    public int TotalBattles { get; set; }
    public int Victories { get; set; }
    public int Defeats { get; set; }
    public int BestWave { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalDamageTaken { get; set; }
    public int EnemiesDefeated { get; set; }
    public List<string> UnlockedArenas { get; set; } = new List<string>();
    public Dictionary<string, int> ArenaBestWaves { get; set; } = new Dictionary<string, int>();
    public Dictionary<string, bool> ArenaCompleted { get; set; } = new Dictionary<string, bool>();
}

public static class PetBattleArenaDatabase
{
    private static Dictionary<string, PetBattleArenaData> _arenas = new Dictionary<string, PetBattleArenaData>();
    private static Dictionary<string, EnemyWave[]> _waves = new Dictionary<string, EnemyWave[]>();
    
    public static void Initialize()
    {
        CreateArenas();
        CreateWaves();
    }
    
    private static void CreateArenas()
    {
        // Training Ground - Easy
        var training = new PetBattleArenaData
        {
            ArenaId = "training_ground",
            ArenaName = "训练场",
            Description = "新手宠物的训练场地，敌人较弱",
            Type = PetBattleArenaData.ArenaType.TrainingGround,
            Difficulty = PetBattleArenaData.ArenaDifficulty.Easy,
            RecommendedLevel = 1,
            TotalWaves = 5,
            RewardGold = 100,
            RewardExp = 50,
            RewardItems = new string[] { "pet_food_small" },
            UnlockLevel = 1
        };
        _arenas[training.ArenaId] = training;
        
        // Battle Colosseum - Normal
        var colosseum = new PetBattleArenaData
        {
            ArenaId = "battle_colosseum",
            ArenaName = "战斗竞技场",
            Description = "标准的宠物战斗竞技场",
            Type = PetBattleArenaData.ArenaType.BattleColosseum,
            Difficulty = PetBattleArenaData.ArenaDifficulty.Normal,
            RecommendedLevel = 10,
            TotalWaves = 8,
            RewardGold = 300,
            RewardExp = 150,
            RewardItems = new string[] { "pet_food_medium", "pet_collar_uncommon" },
            UnlockLevel = 5
        };
        _arenas[colosseum.ArenaId] = colosseum;
        
        // Dragon Arena - Hard
        var dragonArena = new PetBattleArenaData
        {
            ArenaId = "dragon_arena",
            ArenaName = "龙之战场",
            Description = "面对强大龙系敌人的战场",
            Type = PetBattleArenaData.ArenaType.DragonArena,
            Difficulty = PetBattleArenaData.ArenaDifficulty.Hard,
            RecommendedLevel = 25,
            TotalWaves = 10,
            RewardGold = 800,
            RewardExp = 400,
            RewardItems = new string[] { "pet_food_large", "dragon_scale", "pet_armor_rare" },
            UnlockLevel = 15
        };
        _arenas[dragonArena.ArenaId] = dragonArena;
        
        // Phoenix Nest - Epic
        var phoenixNest = new PetBattleArenaData
        {
            ArenaId = "phoenix_nest",
            ArenaName = "凤凰巢穴",
            Description = "浴火重生的试炼之地",
            Type = PetBattleArenaData.ArenaType.PhoenixNest,
            Difficulty = PetBattleArenaData.ArenaDifficulty.Epic,
            RecommendedLevel = 40,
            TotalWaves = 12,
            RewardGold = 2000,
            RewardExp = 1000,
            RewardItems = new string[] { "phoenix_feather", "pet_accessory_epic" },
            UnlockLevel = 30
        };
        _arenas[phoenixNest.ArenaId] = phoenixNest;
        
        // Shadow Realm - Epic
        var shadowRealm = new PetBattleArenaData
        {
            ArenaId = "shadow_realm",
            ArenaName = "暗影领域",
            Description = "暗影生物的领地",
            Type = PetBattleArenaData.ArenaType.ShadowRealm,
            Difficulty = PetBattleArenaData.ArenaDifficulty.Epic,
            RecommendedLevel = 45,
            TotalWaves = 12,
            RewardGold = 2500,
            RewardExp = 1200,
            RewardItems = new string[] { "shadow_essence", "pet_toy_epic" },
            UnlockLevel = 35
        };
        _arenas[shadowRealm.ArenaId] = shadowRealm;
        
        // Sacred Ground - Legendary
        var sacredGround = new PetBattleArenaData
        {
            ArenaId = "sacred_ground",
            ArenaName = "神圣之地",
            Description = "传说中宠物战斗的最高殿堂",
            Type = PetBattleArenaData.ArenaType.SacredGround,
            Difficulty = PetBattleArenaData.ArenaDifficulty.Legendary,
            RecommendedLevel = 60,
            TotalWaves = 15,
            RewardGold = 10000,
            RewardExp = 5000,
            RewardItems = new string[] { "sacred_orb", "legendary_pet_food" },
            UnlockLevel = 50
        };
        _arenas[sacredGround.ArenaId] = sacredGround;
    }
    
    private static void CreateWaves()
    {
        // Training Ground waves
        _waves["training_ground"] = new EnemyWave[]
        {
            new EnemyWave { WaveNumber = 1, EnemyId = "slime", EnemyCount = 3, EnemyLevel = 1, EnemyHealth = 30, EnemyAttack = 5, EnemyDefense = 0, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 2, EnemyId = "slime", EnemyCount = 4, EnemyLevel = 2, EnemyHealth = 40, EnemyAttack = 8, EnemyDefense = 1, SpawnInterval = 1.8f },
            new EnemyWave { WaveNumber = 3, EnemyId = "wolf_pup", EnemyCount = 2, EnemyLevel = 3, EnemyHealth = 60, EnemyAttack = 12, EnemyDefense = 2, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 4, EnemyId = "wolf_pup", EnemyCount = 3, EnemyLevel = 4, EnemyHealth = 80, EnemyAttack = 15, EnemyDefense = 3, SpawnInterval = 2.2f },
            new EnemyWave { WaveNumber = 5, EnemyId = "elder_wolf", EnemyCount = 1, EnemyLevel = 5, EnemyHealth = 200, EnemyAttack = 25, EnemyDefense = 5, SpawnInterval = 0 }
        };
        
        // Battle Colosseum waves
        _waves["battle_colosseum"] = new EnemyWave[]
        {
            new EnemyWave { WaveNumber = 1, EnemyId = "goblin", EnemyCount = 4, EnemyLevel = 8, EnemyHealth = 80, EnemyAttack = 15, EnemyDefense = 5, SpawnInterval = 1.8f },
            new EnemyWave { WaveNumber = 2, EnemyId = "goblin", EnemyCount = 5, EnemyLevel = 9, EnemyHealth = 100, EnemyAttack = 18, EnemyDefense = 6, SpawnInterval = 1.6f },
            new EnemyWave { WaveNumber = 3, EnemyId = "orc", EnemyCount = 2, EnemyLevel = 10, EnemyHealth = 150, EnemyAttack = 25, EnemyDefense = 10, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 4, EnemyId = "orc", EnemyCount = 3, EnemyLevel = 11, EnemyHealth = 180, EnemyAttack = 30, EnemyDefense = 12, SpawnInterval = 2.2f },
            new EnemyWave { WaveNumber = 5, EnemyId = "troll", EnemyCount = 1, EnemyLevel = 12, EnemyHealth = 300, EnemyAttack = 40, EnemyDefense = 15, SpawnInterval = 0 },
            new EnemyWave { WaveNumber = 6, EnemyId = "orc_warrior", EnemyCount = 3, EnemyLevel = 13, EnemyHealth = 200, EnemyAttack = 35, EnemyDefense = 18, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 7, EnemyId = "troll", EnemyCount = 2, EnemyLevel = 14, EnemyHealth = 350, EnemyAttack = 45, EnemyDefense = 20, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 8, EnemyId = "colosseum_champion", EnemyCount = 1, EnemyLevel = 15, EnemyHealth = 600, EnemyAttack = 60, EnemyDefense = 25, SpawnInterval = 0 }
        };
        
        // Dragon Arena waves
        _waves["dragon_arena"] = new EnemyWave[]
        {
            new EnemyWave { WaveNumber = 1, EnemyId = "wyvern", EnemyCount = 2, EnemyLevel = 20, EnemyHealth = 200, EnemyAttack = 35, EnemyDefense = 15, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 2, EnemyId = "wyvern", EnemyCount = 3, EnemyLevel = 21, EnemyHealth = 250, EnemyAttack = 40, EnemyDefense = 18, SpawnInterval = 2.2f },
            new EnemyWave { WaveNumber = 3, EnemyId = "fire_dragon", EnemyCount = 1, EnemyLevel = 22, EnemyHealth = 400, EnemyAttack = 50, EnemyDefense = 20, SpawnInterval = 0 },
            new EnemyWave { WaveNumber = 4, EnemyId = "drake", EnemyCount = 3, EnemyLevel = 23, EnemyHealth = 300, EnemyAttack = 45, EnemyDefense = 22, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 5, EnemyId = "fire_dragon", EnemyCount = 2, EnemyLevel = 24, EnemyHealth = 450, EnemyAttack = 55, EnemyDefense = 25, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 6, ElderDragon(25) },
            new EnemyWave { WaveNumber = 7, ElderDragon(26) },
            new EnemyWave { WaveNumber = 8, ElderDragon(27) },
            new EnemyWave { WaveNumber = 9, ElderDragon(28) },
            new EnemyWave { WaveNumber = 10, DragonKing(30) }
        };
        
        // Phoenix Nest waves
        _waves["phoenix_nest"] = new EnemyWave[]
        {
            new EnemyWave { WaveNumber = 1, EnemyId = "flame_sprite", EnemyCount = 4, EnemyLevel = 35, EnemyHealth = 150, EnemyAttack = 40, EnemyDefense = 10, SpawnInterval = 1.5f },
            new EnemyWave { WaveNumber = 2, EnemyId = "fire_elemental", EnemyCount = 2, EnemyLevel = 36, EnemyHealth = 300, EnemyAttack = 50, EnemyDefense = 20, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 3, EnemyId = "phoenix_chick", EnemyCount = 2, EnemyLevel = 37, EnemyHealth = 400, EnemyAttack = 55, EnemyDefense = 25, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 4, EnemyId = "fire_elemental", EnemyCount = 3, EnemyLevel = 38, EnemyHealth = 350, EnemyAttack = 60, EnemyDefense = 22, SpawnInterval = 2.2f },
            new EnemyWave { WaveNumber = 5, EnemyId = "phoenix", EnemyCount = 1, EnemyLevel = 39, EnemyHealth = 800, EnemyAttack = 80, EnemyDefense = 30, SpawnInterval = 0 },
            new EnemyWave { WaveNumber = 6, EnemyId = "flame_sprite", EnemyCount = 6, EnemyLevel = 40, EnemyHealth = 200, EnemyAttack = 50, EnemyDefense = 15, SpawnInterval = 1.2f },
            new EnemyWave { WaveNumber = 7, EnemyId = "fire_elemental", EnemyCount = 4, EnemyLevel = 41, EnemyHealth = 400, EnemyAttack = 70, EnemyDefense = 25, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 8, EnemyId = "phoenix_chick", EnemyCount = 3, EnemyLevel = 42, EnemyHealth = 500, EnemyAttack = 75, EnemyDefense = 30, SpawnInterval = 2.2f },
            new EnemyWave { WaveNumber = 9, EnemyId = "phoenix", EnemyCount = 2, EnemyLevel = 43, EnemyHealth = 900, EnemyAttack = 90, EnemyDefense = 35, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 10, EnemyId = "inferno_phoenix", EnemyCount = 1, EnemyLevel = 45, EnemyHealth = 1500, EnemyAttack = 120, EnemyDefense = 45, SpawnInterval = 0 },
            new EnemyWave { WaveNumber = 11, EnemyId = "phoenix", EnemyCount = 2, EnemyLevel = 46, EnemyHealth = 1000, EnemyAttack = 100, EnemyDefense = 40, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 12, EnemyId = "inferno_phoenix", EnemyCount = 1, EnemyLevel = 48, EnemyHealth = 2000, EnemyAttack = 150, EnemyDefense = 50, SpawnInterval = 0 }
        };
        
        // Shadow Realm waves
        _waves["shadow_realm"] = new EnemyWave[]
        {
            new EnemyWave { WaveNumber = 1, EnemyId = "shadow_spirit", EnemyCount = 4, EnemyLevel = 40, EnemyHealth = 180, EnemyAttack = 45, EnemyDefense = 12, SpawnInterval = 1.8f },
            new EnemyWave { WaveNumber = 2, EnemyId = "dark_wolf", EnemyCount = 3, EnemyLevel = 41, EnemyHealth = 280, EnemyAttack = 55, EnemyDefense = 18, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 3, EnemyId = "shadow_knight", EnemyCount = 2, EnemyLevel = 42, EnemyHealth = 400, EnemyAttack = 65, EnemyDefense = 25, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 4, EnemyId = "wraith", EnemyCount = 2, EnemyLevel = 43, EnemyHealth = 350, EnemyAttack = 70, EnemyDefense = 20, SpawnInterval = 2.2f },
            new EnemyWave { WaveNumber = 5, EnemyId = "shadow_lord", EnemyCount = 1, EnemyLevel = 44, EnemyHealth = 800, EnemyAttack = 85, EnemyDefense = 35, SpawnInterval = 0 },
            new EnemyWave { WaveNumber = 6, EnemyId = "shadow_spirit", EnemyCount = 6, EnemyLevel = 45, EnemyHealth = 220, EnemyAttack = 55, EnemyDefense = 15, SpawnInterval = 1.5f },
            new EnemyWave { WaveNumber = 7, EnemyId = "dark_wolf", EnemyCount = 4, EnemyLevel = 46, EnemyHealth = 320, EnemyAttack = 65, EnemyDefense = 20, SpawnInterval = 1.8f },
            new EnemyWave { WaveNumber = 8, EnemyId = "shadow_knight", EnemyCount = 3, EnemyLevel = 47, EnemyHealth = 500, EnemyAttack = 80, EnemyDefense = 30, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 9, EnemyId = "wraith_king", EnemyCount = 1, EnemyLevel = 48, EnemyHealth = 1000, EnemyAttack = 100, EnemyDefense = 40, SpawnInterval = 0 },
            new EnemyWave { WaveNumber = 10, EnemyId = "shadow_lord", EnemyCount = 2, EnemyLevel = 49, EnemyHealth = 900, EnemyAttack = 95, EnemyDefense = 38, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 11, EnemyId = "wraith_king", EnemyCount = 2, EnemyLevel = 50, EnemyHealth = 1200, EnemyAttack = 110, EnemyDefense = 45, SpawnInterval = 2.2f },
            new EnemyWave { WaveNumber = 12, EnemyId = "lord_of_shadows", EnemyCount = 1, EnemyLevel = 55, EnemyHealth = 2500, EnemyAttack = 150, EnemyDefense = 55, SpawnInterval = 0 }
        };
        
        // Sacred Ground waves
        _waves["sacred_ground"] = new EnemyWave[]
        {
            new EnemyWave { WaveNumber = 1, EnemyId = "holy_knight", EnemyCount = 3, EnemyLevel = 50, EnemyHealth = 400, EnemyAttack = 70, EnemyDefense = 30, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 2, EnemyId = "sacred_guardian", EnemyCount = 2, EnemyLevel = 52, EnemyHealth = 500, EnemyAttack = 80, EnemyDefense = 35, SpawnInterval = 2.2f },
            new EnemyWave { WaveNumber = 3, EnemyId = "light_angel", EnemyCount = 2, EnemyLevel = 54, EnemyHealth = 600, EnemyAttack = 90, EnemyDefense = 40, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 4, EnemyId = "divine_beast", EnemyCount = 2, EnemyLevel = 56, EnemyHealth = 700, EnemyAttack = 100, EnemyDefense = 45, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 5, EnemyId = "archangel", EnemyCount = 1, EnemyLevel = 58, EnemyHealth = 1200, EnemyAttack = 120, EnemyDefense = 50, SpawnInterval = 0 },
            new EnemyWave { WaveNumber = 6, EnemyId = "holy_knight", EnemyCount = 5, EnemyLevel = 60, EnemyHealth = 500, EnemyAttack = 85, EnemyDefense = 35, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 7, EnemyId = "sacred_guardian", EnemyCount = 4, EnemyLevel = 62, EnemyHealth = 600, EnemyAttack = 95, EnemyDefense = 40, SpawnInterval = 1.8f },
            new EnemyWave { WaveNumber = 8, EnemyId = "light_angel", EnemyCount = 3, EnemyLevel = 64, EnemyHealth = 800, EnemyAttack = 110, EnemyDefense = 45, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 9, EnemyId = "divine_beast", EnemyCount = 3, EnemyLevel = 66, EnemyHealth = 1000, EnemyAttack = 130, EnemyDefense = 50, SpawnInterval = 2.2f },
            new EnemyWave { WaveNumber = 10, EnemyId = "archangel", EnemyCount = 2, EnemyLevel = 68, EnemyHealth = 1500, EnemyAttack = 150, EnemyDefense = 55, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 11, EnemyId = "celestial_dragon", EnemyCount = 1, EnemyLevel = 70, EnemyHealth = 2000, EnemyAttack = 180, EnemyDefense = 60, SpawnInterval = 0 },
            new EnemyWave { WaveNumber = 12, EnemyId = "archangel", EnemyCount = 3, EnemyLevel = 72, EnemyHealth = 1800, EnemyAttack = 170, EnemyDefense = 60, SpawnInterval = 2.0f },
            new EnemyWave { WaveNumber = 13, EnemyId = "divine_beast", EnemyCount = 4, EnemyLevel = 74, EnemyHealth = 1500, EnemyAttack = 160, EnemyDefense = 55, SpawnInterval = 1.8f },
            new EnemyWave { WaveNumber = 14, EnemyId = "celestial_dragon", EnemyCount = 2, EnemyLevel = 78, EnemyHealth = 2500, EnemyAttack = 200, EnemyDefense = 70, SpawnInterval = 2.5f },
            new EnemyWave { WaveNumber = 15, EnemyId = "god_of_war", EnemyCount = 1, EnemyLevel = 80, EnemyHealth = 5000, EnemyAttack = 300, EnemyDefense = 100, SpawnInterval = 0 }
        };
    }
    
    private static EnemyWave ElderDragon(int level)
    {
        return new EnemyWave { WaveNumber = 0, EnemyId = "elder_dragon", EnemyCount = 1, EnemyLevel = level, EnemyHealth = 600 + (level - 25) * 100, EnemyAttack = 70 + (level - 25) * 10, EnemyDefense = 30 + (level - 25) * 5, SpawnInterval = 0 };
    }
    
    private static EnemyWave DragonKing(int level)
    {
        return new EnemyWave { WaveNumber = 10, EnemyId = "dragon_king", EnemyCount = 1, EnemyLevel = level, EnemyHealth = 1500, EnemyAttack = 150, EnemyDefense = 50, SpawnInterval = 0 };
    }
    
    public static PetBattleArenaData[] GetAllArenas()
    {
        var list = new List<PetBattleArenaData>(_arenas.Values);
        list.Sort((a, b) => a.UnlockLevel.CompareTo(b.UnlockLevel));
        return list.ToArray();
    }
    
    public static PetBattleArenaData GetArena(string arenaId)
    {
        return _arenas.ContainsKey(arenaId) ? _arenas[arenaId] : null;
    }
    
    public static EnemyWave[] GetWaves(string arenaId)
    {
        return _waves.ContainsKey(arenaId) ? _waves[arenaId] : null;
    }
    
    public static bool IsUnlocked(PetBattleArenaData arena, int playerLevel)
    {
        return playerLevel >= arena.UnlockLevel;
    }
}
