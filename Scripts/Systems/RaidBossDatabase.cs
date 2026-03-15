using System;
using System.Collections.Generic;

/// <summary>
/// 团本数据库 - 存储和管理团本配置数据
/// 包含Boss配置、角色需求、掉落表等
/// </summary>
public class RaidBossDatabase
{
    // Raid Boss Configurations
    public static Dictionary<RaidBossType, RaidConfig> RaidConfigs { get; private set; }
    
    // Role Distributions
    public static Dictionary<RaidRole, float> RoleRequirements { get; private set; }
    
    // Phase Thresholds
    public static Dictionary<int, float> PhaseHealthThresholds { get; private set; }
    
    // Loot Tables
    public static Dictionary<RaidBossType, List<LootEntry>> LootTables { get; private set; }
    
    static RaidBossDatabase()
    {
        InitializeRaidConfigs();
        InitializeRoleRequirements();
        InitializePhaseThresholds();
        InitializeLootTables();
    }
    
    private static void InitializeRaidConfigs()
    {
        RaidConfigs = new Dictionary<RaidBossType, RaidConfig>
        {
            { RaidBossType.DragonLair, new RaidConfig {
                Name = "Dragon's Lair",
                Description = "Face the ancient dragon and its minions",
                MinPlayers = 4,
                MaxPlayers = 8,
                RecommendedLevel = 30,
                BossHealth = 500000f,
                MaxPhases = 4,
                EnrageTime = 600f,
                Rewards = new RaidRewards { Gold = 10000, Exp = 50000 }
            }},
            { RaidBossType.DemonCastle, new RaidConfig {
                Name = "Demon Castle",
                Description = "Conquer the demon lord's fortress",
                MinPlayers = 4,
                MaxPlayers = 8,
                RecommendedLevel = 35,
                BossHealth = 750000f,
                MaxPhases = 5,
                EnrageTime = 720f,
                Rewards = new RaidRewards { Gold = 15000, Exp = 75000 }
            }},
            { RaidBossType.AncientTemple, new RaidConfig {
                Name = "Ancient Temple",
                Description = "Explore the ruins and defeat the guardian",
                MinPlayers = 4,
                MaxPlayers = 8,
                RecommendedLevel = 25,
                BossHealth = 350000f,
                MaxPhases = 3,
                EnrageTime = 480f,
                Rewards = new RaidRewards { Gold = 8000, Exp = 40000 }
            }},
            { RaidBossType.VoidRift, new RaidConfig {
                Name = "Void Rift",
                Description = "Close the rift before entities escape",
                MinPlayers = 5,
                MaxPlayers = 8,
                RecommendedLevel = 40,
                BossHealth = 1000000f,
                MaxPhases = 5,
                EnrageTime = 900f,
                Rewards = new RaidRewards { Gold = 20000, Exp = 100000 }
            }},
            { RaidBossType.FrozenCitadel, new RaidConfig {
                Name = "Frozen Citadel",
                Description = "Defeat the ice king and his frost warriors",
                MinPlayers = 4,
                MaxPlayers = 8,
                RecommendedLevel = 32,
                BossHealth = 600000f,
                MaxPhases = 4,
                EnrageTime = 660f,
                Rewards = new RaidRewards { Gold = 12000, Exp = 60000 }
            }},
            { RaidBossType.ShadowRealm, new RaidConfig {
                Name = "Shadow Realm",
                Description = "Navigate the darkness and defeat shadow lords",
                MinPlayers = 5,
                MaxPlayers = 8,
                RecommendedLevel = 38,
                BossHealth = 850000f,
                MaxPhases = 5,
                EnrageTime = 840f,
                Rewards = new RaidRewards { Gold = 18000, Exp = 90000 }
            }},
            { RaidBossType.CelestialPalace, new RaidConfig {
                Name = "Celestial Palace",
                Description = "Ascend to the heavens and challenge the divine",
                MinPlayers = 6,
                MaxPlayers = 8,
                RecommendedLevel = 45,
                BossHealth = 1200000f,
                MaxPhases = 6,
                EnrageTime = 1080f,
                Rewards = new RaidRewards { Gold = 25000, Exp = 125000 }
            }},
            { RaidBossType.AbyssalPit, new RaidConfig {
                Name = "Abyssal Pit",
                Description = "Descend into the abyss and face the unknown",
                MinPlayers = 6,
                MaxPlayers = 8,
                RecommendedLevel = 42,
                BossHealth = 1000000f,
                MaxPhases = 5,
                EnrageTime = 960f,
                Rewards = new RaidRewards { Gold = 22000, Exp = 110000 }
            }}
        };
    }
    
    private static void InitializeRoleRequirements()
    {
        RoleRequirements = new Dictionary<RaidRole, float>
        {
            { RaidRole.Tank, 1f },      // 1 tank required
            { RaidRole.Healer, 2f },    // 2 healers recommended
            { RaidRole.Damage, 3f },    // 3 DPS
            { RaidRole.Support, 1f }     // 1 support
        };
    }
    
    private static void InitializePhaseThresholds()
    {
        PhaseHealthThresholds = new Dictionary<int, float>
        {
            { 1, 1.0f },   // Phase 1: 100% HP
            { 2, 0.75f },  // Phase 2: 75% HP
            { 3, 0.5f },   // Phase 3: 50% HP
            { 4, 0.25f },  // Phase 4: 25% HP
            { 5, 0.1f },   // Phase 5: 10% HP
            { 6, 0.05f }   // Phase 6: 5% HP (Final)
        };
    }
    
    private static void InitializeLootTables()
    {
        LootTables = new Dictionary<RaidBossType, List<LootEntry>>();
        
        // Dragon Lair Loot
        LootTables[RaidBossType.DragonLair] = new List<LootEntry>
        {
            new LootEntry { ItemId = "dragon_scale", Rarity = "Epic", DropRate = 0.3f },
            new LootEntry { ItemId = "fire_breath_weapon", Rarity = "Legendary", DropRate = 0.1f },
            new LootEntry { ItemId = "dragon_heart", Rarity = "Epic", DropRate = 0.25f },
            new LootEntry { ItemId = "ancient_dragon_bone", Rarity = "Rare", DropRate = 0.5f }
        };
        
        // Demon Castle Loot
        LootTables[RaidBossType.DemonCastle] = new List<LootEntry>
        {
            new LootEntry { ItemId = "demon_horn", Rarity = "Epic", DropRate = 0.3f },
            new LootEntry { ItemId = "hellfire_armor", Rarity = "Legendary", DropRate = 0.1f },
            new LootEntry { ItemId = "soul_ shard", Rarity = "Epic", DropRate = 0.25f },
            new LootEntry { ItemId = "dark_essence", Rarity = "Rare", DropRate = 0.5f }
        };
        
        // Add default loot for other raids
        LootTables[RaidBossType.AncientTemple] = new List<LootEntry>
        {
            new LootEntry { ItemId = "ancient_relic", Rarity = "Epic", DropRate = 0.3f },
            new LootEntry { ItemId = "temple_guardian_blade", Rarity = "Legendary", DropRate = 0.1f },
            new LootEntry { ItemId = "sacred_gem", Rarity = "Rare", DropRate = 0.5f }
        };
        
        LootTables[RaidBossType.VoidRift] = new List<LootEntry>
        {
            new LootEntry { ItemId = "void_crystal", Rarity = "Epic", DropRate = 0.3f },
            new LootEntry { ItemId = "reality_tear_weapon", Rarity = "Legendary", DropRate = 0.1f },
            new LootEntry { ItemId = "dark_matter", Rarity = "Epic", DropRate = 0.25f }
        };
        
        LootTables[RaidBossType.FrozenCitadel] = new List<LootEntry>
        {
            new LootEntry { ItemId = "frost_essence", Rarity = "Epic", DropRate = 0.3f },
            new LootEntry { ItemId = "ice_crown", Rarity = "Legendary", DropRate = 0.1f },
            new LootEntry { ItemId = "winter_blade", Rarity = "Rare", DropRate = 0.5f }
        };
        
        LootTables[RaidBossType.ShadowRealm] = new List<LootEntry>
        {
            new LootEntry { ItemId = "shadow_essence", Rarity = "Epic", DropRate = 0.3f },
            new LootEntry { ItemId = "dark_reaper", Rarity = "Legendary", DropRate = 0.1f },
            new LootEntry { ItemId = "nightshade", Rarity = "Rare", DropRate = 0.5f }
        };
        
        LootTables[RaidBossType.CelestialPalace] = new List<LootEntry>
        {
            new LootEntry { ItemId = "divine_orb", Rarity = "Legendary", DropRate = 0.2f },
            new LootEntry { ItemId = "heavenly_weapon", Rarity = "Legendary", DropRate = 0.15f },
            new LootEntry { ItemId = "celestial_blessing", Rarity = "Epic", DropRate = 0.4f }
        };
        
        LootTables[RaidBossType.AbyssalPit] = new List<LootEntry>
        {
            new LootEntry { ItemId = "abyss_heart", Rarity = "Legendary", DropRate = 0.15f },
            new LootEntry { ItemId = "void_armor", Rarity = "Legendary", DropRate = 0.1f },
            new LootEntry { ItemId = "corrupted_artifact", Rarity = "Epic", DropRate = 0.35f }
        };
    }
}

public class RaidConfig
{
    public string Name { get; set; }
    public string Description { get; set; }
    public int MinPlayers { get; set; }
    public int MaxPlayers { get; set; }
    public int RecommendedLevel { get; set; }
    public float BossHealth { get; set; }
    public int MaxPhases { get; set; }
    public float EnrageTime { get; set; }
    public RaidRewards Rewards { get; set; }
}

public class RaidRewards
{
    public int Gold { get; set; }
    public int Exp { get; set; }
}

public class LootEntry
{
    public string ItemId { get; set; }
    public string Rarity { get; set; }
    public float DropRate { get; set; }
}
