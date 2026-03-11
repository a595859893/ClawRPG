using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Secret achievements that are discovered through gameplay
    /// </summary>
    public partial class SecretAchievementData : Resource
    {
        public string AchievementId { get; set; }
        public string DisplayName { get; set; }
        public string Description { get; set; }
        public SecretAchievementCategory Category { get; set; }
        public SecretAchievementRarity Rarity { get; set; }
        public int DiscoveryCondition { get; set; } // Times to trigger before discovery
        public int GoldReward { get; set; }
        public int ExpReward { get; set; }
        public List<string> Tags { get; set; } = new();
    }

    public enum SecretAchievementCategory
    {
        Combat,
        Exploration,
        Collection,
        Social,
        Challenge,
        Lucky,
        Hidden
    }

    public enum SecretAchievementRarity
    {
        Common,
        Uncommon,
        Rare,
        Epic,
        Legendary,
        Mythic
    }

    public partial class PlayerSecretAchievementData : RefCounted
    {
        public string AchievementId { get; set; }
        public bool IsDiscovered { get; set; }
        public int Progress { get; set; }
        public DateTime DiscoveredAt { get; set; }
    }

    public partial class SecretAchievementDatabase
    {
        private static Dictionary<string, SecretAchievementData> _achievements = new()
        {
            // Combat - Rare achievements
            {"kill_100_bosses", new SecretAchievementData {
                AchievementId = "kill_100_bosses",
                DisplayName = "?????",
                Description = "Defeat 100 bosses. What lies beyond the final battle?",
                Category = SecretAchievementCategory.Combat,
                Rarity = SecretAchievementRarity.Rare,
                DiscoveryCondition = 100,
                GoldReward = 5000,
                ExpReward = 2500,
                Tags = new List<string> { "boss", "combat", "endgame" }
            }},
            {"no_damage_run", new SecretAchievementData {
                AchievementId = "no_damage_run",
                DisplayName = "??????",
                Description = "Complete a dungeon without taking any damage",
                Category = SecretAchievementCategory.Challenge,
                Rarity = SecretAchievementRarity.Epic,
                DiscoveryCondition = 1,
                GoldReward = 3000,
                ExpReward = 1500,
                Tags = new List<string> { "dungeon", "skill", "perfect" }
            }},
            {"combo_100", new SecretAchievementData {
                AchievementId = "combo_100",
                DisplayName = "???????",
                Description = "Reach 100 hit combo",
                Category = SecretAchievementCategory.Combat,
                Rarity = SecretAchievementRarity.Rare,
                DiscoveryCondition = 100,
                GoldReward = 2000,
                ExpReward = 1000,
                Tags = new List<string> { "combat", "combo", "skill" }
            }},
            
            // Exploration - Lucky discoveries
            {"find_all_treasures", new SecretAchievementData {
                AchievementId = "find_all_treasures",
                DisplayName = "??????",
                Description = "Discover every type of mystery treasure",
                Category = SecretAchievementCategory.Exploration,
                Rarity = SecretAchievementRarity.Epic,
                DiscoveryCondition = 20,
                GoldReward = 4000,
                ExpReward = 2000,
                Tags = new List<string> { "treasure", "exploration", "collection" }
            }},
            {"visit_all_regions", new SecretAchievementData {
                AchievementId = "visit_all_regions",
                DisplayName = "??????",
                Description = "Visit every region in the world",
                Category = SecretAchievementCategory.Exploration,
                Rarity = SecretAchievementRarity.Rare,
                DiscoveryCondition = 10,
                GoldReward = 2500,
                ExpReward = 1200,
                Tags = new List<string> { "exploration", "world", "travel" }
            }},
            
            // Collection
            {"collect_all_pets", new SecretAchievementData {
                AchievementId = "collect_all_pets",
                DisplayName = "??????",
                Description = "Obtain every type of pet",
                Category = SecretAchievementCategory.Collection,
                Rarity = SecretAchievementRarity.Legendary,
                DiscoveryCondition = 20,
                GoldReward = 8000,
                ExpReward = 4000,
                Tags = new List<string> { "pets", "collection", "completionist" }
            }},
            {"max_all_mounts", new SecretAchievementData {
                AchievementId = "max_all_mounts",
                DisplayName = "??????",
                Description = "Max out every mount's level",
                Category = SecretAchievementCategory.Collection,
                Rarity = SecretAchievementRarity.Legendary,
                DiscoveryCondition = 50,
                GoldReward = 10000,
                ExpReward = 5000,
                Tags = new List<string> { "mounts", "collection", "dedication" }
            }},
            
            // Lucky moments
            {"critical_rain", new SecretAchievementData {
                AchievementId = "critical_rain",
                DisplayName = "???????",
                Description = "Land 50 critical hits in a single battle",
                Category = SecretAchievementCategory.Lucky,
                Rarity = SecretAchievementRarity.Uncommon,
                DiscoveryCondition = 50,
                GoldReward = 1500,
                ExpReward = 750,
                Tags = new List<string> { "combat", "crit", "luck" }
            }},
            {"legendary_drop", new SecretAchievementData {
                AchievementId = "legendary_drop",
                DisplayName = "???????",
                Description = "Get a legendary item from a common enemy",
                Category = SecretAchievementCategory.Lucky,
                Rarity = SecretAchievementRarity.Mythic,
                DiscoveryCondition = 1,
                GoldReward = 20000,
                ExpReward = 10000,
                Tags = new List<string> { "luck", "drop", "legendary" }
            }},
            
            // Challenge
            {"solo_elite_dungeon", new SecretAchievementData {
                AchievementId = "solo_elite_dungeon",
                DisplayName = "??????",
                Description = "Complete an elite dungeon solo",
                Category = SecretAchievementCategory.Challenge,
                Rarity = SecretAchievementRarity.Epic,
                DiscoveryCondition = 1,
                GoldReward = 5000,
                ExpReward = 2500,
                Tags = new List<string> { "dungeon", "solo", "challenge" }
            }},
            {"speed_run", new SecretAchievementData {
                AchievementId = "speed_run",
                DisplayName = "??????",
                Description = "Complete a dungeon in under 3 minutes",
                Category = SecretAchievementCategory.Challenge,
                Rarity = SecretAchievementRarity.Epic,
                DiscoveryCondition = 1,
                GoldReward = 3500,
                ExpReward = 1800,
                Tags = new List<string> { "speed", "dungeon", "challenge" }
            }},
            
            // Social
            {"guild_founder", new SecretAchievementData {
                AchievementId = "guild_founder",
                DisplayName = "???????",
                Description = "Create your own guild",
                Category = SecretAchievementCategory.Social,
                Rarity = SecretAchievementRarity.Rare,
                DiscoveryCondition = 1,
                GoldReward = 3000,
                ExpReward = 1500,
                Tags = new List<string> { "guild", "social", "leader" }
            }},
            {"trade_master", new SecretAchievementData {
                AchievementId = "trade_master",
                DisplayName = "??????",
                Description = "Complete 100 trades with other players",
                Category = SecretAchievementCategory.Social,
                Rarity = SecretAchievementRarity.Rare,
                DiscoveryCondition = 100,
                GoldReward = 4000,
                ExpReward = 2000,
                Tags = new List<string> { "trade", "social", "economy" }
            }},
            
            // Hidden mysteries
            {"the_chosen_one", new SecretAchievementData {
                AchievementId = "the_chosen_one",
                DisplayName = "?????????",
                Description = "Reach level 50",
                Category = SecretAchievementCategory.Hidden,
                Rarity = SecretAchievementRarity.Mythic,
                DiscoveryCondition = 50,
                GoldReward = 25000,
                ExpReward = 15000,
                Tags = new List<string> { "level", "progression", "mystery" }
            }},
            {"millionaire", new SecretAchievementData {
                AchievementId = "millionaire",
                DisplayName = "??????",
                Description = "Accumulate 1,000,000 gold",
                Category = SecretAchievementCategory.Hidden,
                Rarity = SecretAchievementRarity.Legendary,
                DiscoveryCondition = 1000000,
                GoldReward = 0,
                ExpReward = 5000,
                Tags = new List<string> { "gold", "wealth", "economy" }
            }},
        };

        public static SecretAchievementData GetAchievement(string id)
        {
            return _achievements.ContainsKey(id) ? _achievements[id] : null;
        }

        public static List<SecretAchievementData> GetAllAchievements()
        {
            return new List<SecretAchievementData>(_achievements.Values);
        }

        public static List<SecretAchievementData> GetAchievementsByCategory(SecretAchievementCategory category)
        {
            List<SecretAchievementData> result = new();
            foreach (var achievement in _achievements.Values)
            {
                if (achievement.Category == category)
                    result.Add(achievement);
            }
            return result;
        }

        public static List<SecretAchievementData> GetAchievementsByRarity(SecretAchievementRarity rarity)
        {
            List<SecretAchievementData> result = new();
            foreach (var achievement in _achievements.Values)
            {
                if (achievement.Rarity == rarity)
                    result.Add(achievement);
            }
            return result;
        }

        public static int GetTotalCount() => _achievements.Count;
    }
}
