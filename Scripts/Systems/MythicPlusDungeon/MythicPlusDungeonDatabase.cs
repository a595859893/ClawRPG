using System;
using System.Collections.Generic;
using Godot;

public static class MythicPlusDungeonDatabase
{
    private static readonly Dictionary<string, MythicPlusDungeonConfig> _dungeons = new Dictionary<string, MythicPlusDungeonConfig>();
    private static readonly Dictionary<int, MythicPlusReward> _rewards = new Dictionary<int, MythicPlusReward>();
    private static readonly List<MythicPlusAffixGroup> _weeklyAffixes = new List<MythicPlusAffixGroup>();
    private static bool _initialized = false;
    
    public static void Initialize()
    {
        if (_initialized) return;
        
        InitializeDungeons();
        InitializeRewards();
        InitializeWeeklyAffixes();
        
        _initialized = true;
    }
    
    private static void InitializeDungeons()
    {
        // Mythic+ Dungeons with various biomes
        var dungeons = new[]
        {
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_atal_dazar",
                Name = "Atal'Dazar",
                BaseLevel = 1,
                RecommendedItemLevel = 340,
                MinItemLevel = 280,
                BossCount = 4,
                EnemyCount = 45,
                EstimatedTimeMinutes = 25,
                Biome = "Jungle",
                EnemyTypes = new List<string> { "Dinosaur", "Priest", "Guardian", "Troll" },
                Rewards = new Dictionary<string, int> { { "gold", 500 }, { "gear", 3 } }
            },
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_freehold",
                Name = "Freehold",
                BaseLevel = 1,
                RecommendedItemLevel = 345,
                MinItemLevel = 280,
                BossCount = 3,
                EnemyCount = 52,
                EstimatedTimeMinutes = 22,
                Biome = "Pirate",
                EnemyTypes = new List<string> { "Pirate", "Ogre", "Naga", " Beast" },
                Rewards = new Dictionary<string, int> { { "gold", 550 }, { "gear", 3 } }
            },
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_kings_rest",
                Name = "King's Rest",
                BaseLevel = 1,
                RecommendedItemLevel = 350,
                MinItemLevel = 285,
                BossCount = 4,
                EnemyCount = 48,
                EstimatedTimeMinutes = 28,
                Biome = "Desert",
                EnemyTypes = new List<string> { "Skeleton", "Mummy", "Spectral", "Guardian" },
                Rewards = new Dictionary<string, int> { { "gold", 600 }, { "gear", 3 } }
            },
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_siege",
                Name = "Siege of Boralus",
                BaseLevel = 1,
                RecommendedItemLevel = 355,
                MinItemLevel = 290,
                BossCount = 4,
                EnemyCount = 55,
                EstimatedTimeMinutes = 30,
                Biome = "City",
                EnemyTypes = new List<string> { "Soldier", "Ogre", "Demon", "Mech" },
                Rewards = new Dictionary<string, int> { { "gold", 650 }, { "gear", 3 } }
            },
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_temple",
                Name = "Temple of Sethraliss",
                BaseLevel = 1,
                RecommendedItemLevel = 360,
                MinItemLevel = 295,
                BossCount = 3,
                EnemyCount = 42,
                EstimatedTimeMinutes = 24,
                Biome = "Temple",
                EnemyTypes = new List<string> { "Naga", "Elemental", "Mysterious", "Serpent" },
                Rewards = new Dictionary<string, int> { { "gold", 700 }, { "gear", 4 } }
            },
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_underrot",
                Name = "The Underrot",
                BaseLevel = 1,
                RecommendedItemLevel = 365,
                MinItemLevel = 300,
                BossCount = 4,
                EnemyCount = 50,
                EstimatedTimeMinutes = 26,
                Biome = "Underground",
                EnemyTypes = new List<string> { "Rot", "Spitter", "Worm", "Spirit" },
                Rewards = new Dictionary<string, int> { { "gold", 750 }, { "gear", 4 } }
            },
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_tooltip",
                Name = "Tol Dagor",
                BaseLevel = 1,
                RecommendedItemLevel = 370,
                MinItemLevel = 305,
                BossCount = 4,
                EnemyCount = 48,
                EstimatedTimeMinutes = 28,
                Biome = "Prison",
                EnemyTypes = new List<string> { "Guard", "Prisoner", "Overseer", "Mech" },
                Rewards = new Dictionary<string, int> { { "gold", 800 }, { "gear", 4 } }
            },
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_motherlode",
                Name = "The MOTHERLODE!!",
                BaseLevel = 1,
                RecommendedItemLevel = 375,
                MinItemLevel = 310,
                BossCount = 4,
                EnemyCount = 58,
                EstimatedTimeMinutes = 32,
                Biome = "Mine",
                EnemyTypes = new List<string> { "Goblin", "Mech", "Ogre", "Trogg" },
                Rewards = new Dictionary<string, int> { { "gold", 850 }, { "gear", 4 } }
            },
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_shrine",
                Name = "Shrine of the Storm",
                BaseLevel = 1,
                RecommendedItemLevel = 380,
                MinItemLevel = 315,
                BossCount = 4,
                EnemyCount = 44,
                EstimatedTimeMinutes = 30,
                Biome = "Ocean",
                EnemyTypes = new List<string> { "Wraith", "Elemental", "Kraken", "Cultist" },
                Rewards = new Dictionary<string, int> { { "gold", 900 }, { "gear", 4 } }
            },
            new MythicPlusDungeonConfig
            {
                DungeonId = "mythic_boralus",
                Name = "Waycrest Manor",
                BaseLevel = 1,
                RecommendedItemLevel = 385,
                MinItemLevel = 320,
                BossCount = 4,
                EnemyCount = 46,
                EstimatedTimeMinutes = 28,
                Biome = "Mansion",
                EnemyTypes = new List<string> { "Witch", "Soul", "Beast", "Cursed" },
                Rewards = new Dictionary<string, int> { { "gold", 950 }, { "gear", 5 } }
            }
        };
        
        foreach (var dungeon in dungeons)
        {
            _dungeons[dungeon.DungeonId] = dungeon;
        }
    }
    
    private static void InitializeRewards()
    {
        // Level-based rewards
        for (int level = 0; level <= 30; level++)
        {
            var reward = new MythicPlusReward
            {
                Level = level,
                Gold = 200 + (level * 100),
                Experience = 500 + (level * 250),
                ScoreBonus = level * 50,
                Title = GetTitleForLevel(level),
                Items = new List<string>(),
                Unlocks = new List<string>()
            };
            
            // Add gear/loot based on level
            if (level >= 2) reward.Items.Add($"mythic_gear_{Math.Min(level, 20)}_box");
            if (level >= 5) reward.Items.Add("rare_mount_token");
            if (level >= 10) reward.Items.Add($"mythic_weapon_{Math.Min(level, 20)}");
            if (level >= 15) reward.Items.Add("epic_mount_token");
            if (level >= 20) reward.Items.Add($"legendary_equipment_{Math.Min(level, 25)}");
            if (level >= 25) reward.Items.Add("mythic_pet_box");
            
            // Unlocks
            if (level == 3) reward.Unlocks.Add("mythic_plus_weekly_reward_tier2");
            if (level == 5) reward.Unlocks.Add("mythic_plus_keystone");
            if (level == 10) reward.Unlocks.Add("mythic_plus_weekly_reward_tier3");
            if (level == 15) reward.Unlocks.Add("mythic_plus_elite_title");
            if (level == 20) reward.Unlocks.Add("mythic_plus_weekly_reward_tier4");
            if (level == 25) reward.Unlocks.Add("mythic_plus_mythic_mount");
            
            _rewards[level] = reward;
        }
    }
    
    private static string GetTitleForLevel(int level)
    {
        if (level >= 25) return "Mythic+ Grandmaster";
        if (level >= 20) return "Mythic+ Master";
        if (level >= 15) return "Mythic+ Expert";
        if (level >= 10) return "Mythic+ Adept";
        if (level >= 5) return "Mythic+ Challenger";
        if (level >= 2) return "Mythic+ Adventurer";
        return "Mythic+ Initiate";
    }
    
    private static void InitializeWeeklyAffixes()
    {
        // Weekly affix rotations (Season 1-4 example)
        var affixGroups = new[]
        {
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 1, Name = "Fortified + Teeming", Description = "Minions are stronger and more numerous.", Affixes = new List<MythicAffix> { MythicAffix.Fortified, MythicAffix.Teeming } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 2, Name = "Tyrannical + Volcanic", Description = "Bosses are stronger. Beware ground effects.", Affixes = new List<MythicAffix> { MythicAffix.Tyrannical, MythicAffix.Volcanic } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 3, Name = "Fortified + Necrotic", Description = "Minions are stronger. Shields decay over time.", Affixes = new List<MythicAffix> { MythicAffix.Fortified, MythicAffix.Necrotic } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 4, Name = "Tyrannical + Afflicted", Description = "Bosses are stronger. Summons weakened spirits.", Affixes = new List<MythicAffix> { MythicAffix.Tyrannical, MythicAffix.Afflicted } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 5, Name = "Fortified + Spitting Image", Description = "Minions are stronger. Enemies spawn clones.", Affixes = new List<MythicAffix> { MythicAffix.Fortified, MythicAffix.SpittingImage } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 6, Name = "Tyrannical + Grieving Wound", Description = "Bosses are stronger. Healing is reduced.", Affixes = new List<MythicAffix> { MythicAffix.Tyrannical, MythicAffix.GrievingWound } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 7, Name = "Fortified + Explosive", Description = "Minions are stronger. Orb spawns on kills.", Affixes = new List<MythicAffix> { MythicAffix.Fortified, MythicAffix.Explosive } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 8, Name = "Tyrannical + Quaking", Description = "Bosses are stronger. AoE knockbacks periodically.", Affixes = new List<MythicAffix> { MythicAffix.Tyrannical, MythicAffix.Quaking } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 9, Name = "Fortified + Sanguine", Description = "Minions are stronger. Blood pools on death.", Affixes = new List<MythicAffix> { MythicAffix.Fortified, MythicAffix.Sanguine } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 10, Name = "Tyrannical + Bolstering", Description = "Bosses are stronger. Enemies buff on kill.", Affixes = new List<MythicAffix> { MythicAffix.Tyrannical, MythicAffix.Bolstering } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 11, Name = "Fortified + Raging", Description = "Minions are stronger. Enrage at low HP.", Affixes = new List<MythicAffix> { MythicAffix.Fortified, MythicAffix.Raging } },
            new MythicPlusAffixGroup { Season = 1, WeekNumber = 12, Name = "Tyrannical + Bursting", Description = "Bosses are stronger. Deaths cause explosions.", Affixes = new List<MythicAffix> { MythicAffix.Tyrannical, MythicAffix.Bursting } },
            new MythicPlusAffixGroup { Season = 2, WeekNumber = 1, Name = "Fortified + Inspiring", Description = "Minions are stronger. Nearby allies buffed.", Affixes = new List<MythicAffix> { MythicAffix.Fortified, MythicAffix.Inspiring } },
            new MythicPlusAffixGroup { Season = 2, WeekNumber = 2, Name = "Tyrannical + Prideful", Description = "Bosses are stronger. Elite spawns.", Affixes = new List<MythicAffix> { MythicAffix.Tyrannical, MythicAffix.Prideful } },
            new MythicPlusAffixGroup { Season = 2, WeekNumber = 3, Name = "Fortified + Storming", Description = "Minions are stronger. Visibility reduced.", Affixes = new List<MythicAffix> { MythicAffix.Fortified, MythicAffix.Storming } },
            new MythicPlusAffixGroup { Season = 2, WeekNumber = 4, Name = "Tyrannical + Entangling", Description = "Bosses are stronger. Roots nearby players.", Affixes = new List<MythicAffix> { MythicAffix.Tyrannical, MythicAffix.Entangling } }
        };
        
        foreach (var group in affixGroups)
        {
            _weeklyAffixes.Add(group);
        }
    }
    
    public static MythicPlusDungeonConfig GetDungeon(string dungeonId)
    {
        return _dungeons.ContainsKey(dungeonId) ? _dungeons[dungeonId] : null;
    }
    
    public static List<MythicPlusDungeonConfig> GetAllDungeons()
    {
        return new List<MythicPlusDungeonConfig>(_dungeons.Values);
    }
    
    public static MythicPlusReward GetReward(int level)
    {
        return _rewards.ContainsKey(level) ? _rewards[level] : _rewards[0];
    }
    
    public static MythicPlusAffixGroup GetWeeklyAffixes(int season, int weekNumber)
    {
        foreach (var group in _weeklyAffixes)
        {
            if (group.Season == season && group.WeekNumber == weekNumber)
                return group;
        }
        // Default fallback
        return new MythicPlusAffixGroup
        {
            Season = season,
            WeekNumber = weekNumber,
            Name = "Fortified + Teeming",
            Affixes = new List<MythicAffix> { MythicAffix.Fortified, MythicAffix.Teeming }
        };
    }
    
    public static int GetCurrentWeekNumber()
    {
        // Calculate week number from year start
        var now = DateTime.UtcNow;
        var start = new DateTime(now.Year, 1, 1);
        return (int)Math.Ceiling((now - start).TotalDays / 7.0);
    }
    
    public static MythicPlusAffixGroup GetCurrentWeeklyAffixes()
    {
        var now = DateTime.UtcNow;
        return GetWeeklyAffixes(1, GetCurrentWeekNumber() % 12 + 1);
    }
    
    public static int GetTimeLimitForLevel(int level)
    {
        // Base 45 minutes, decreasing by 2 minutes per level, minimum 15 minutes
        return Math.Max(15, 45 - (level * 2));
    }
    
    public static float GetHealthMultiplierForLevel(int level)
    {
        // Health increases by 10% per level
        return 1.0f + (level * 0.1f);
    }
    
    public static float GetDamageMultiplierForLevel(int level)
    {
        // Damage increases by 8% per level
        return 1.0f + (level * 0.08f);
    }
}
