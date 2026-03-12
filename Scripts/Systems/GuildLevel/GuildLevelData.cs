using Godot;
using System;
using System.Collections.Generic;

public class GuildLevelData
{
    public int GuildId { get; set; }
    public int Level { get; set; } = 1;
    public int Experience { get; set; } = 0;
    public int TotalExperience { get; set; } = 0;
    
    // Stats
    public int TotalMembers { get; set; } = 0;
    public int MaxMembers { get; set; } = 10;
    public int TotalQuestsCompleted { get; set; } = 0;
    public int TotalWarsWon { get; set; } = 0;
    public int TotalWarsLost { get; set; } = 0;
    public int TotalTechnologyResearched { get; set; } = 0;
    
    // Perks unlocked
    public List<string> UnlockedPerks { get; set; } = new List<string>();
    
    // Daily/Weekly stats
    public int DailyContributions { get; set; } = 0;
    public int WeeklyContributions { get; set; } = 0;
    public int DailyQuestsCompleted { get; set; } = 0;
    public int WeeklyQuestsCompleted { get; set; } = 0;
    public long LastDailyReset { get; set; } = 0;
    public long LastWeeklyReset { get; set; } = 0;
}

public class GuildLevelDatabase
{
    // Level requirements: XP needed to reach each level
    private static readonly int[] LevelRequirements = {
        0,      // Level 1
        1000,   // Level 2
        2500,   // Level 3
        5000,   // Level 4
        10000,  // Level 5
        20000,  // Level 6
        35000,  // Level 7
        55000,  // Level 8
        80000,  // Level 9
        120000, // Level 10
        170000, // Level 11
        230000, // Level 12
        300000, // Level 13
        380000, // Level 14
        470000, // Level 15
        570000, // Level 16
        680000, // Level 17
        800000, // Level 18
        950000, // Level 19
        1150000 // Level 20 - Max
    };
    
    public static int GetMaxLevel()
    {
        return LevelRequirements.Length;
    }
    
    public static int GetExperienceForLevel(int level)
    {
        if (level < 1 || level > LevelRequirements.Length)
            return 0;
        return LevelRequirements[level - 1];
    }
    
    // Perk definitions
    public static Dictionary<string, Dictionary<string, object>> GetPerkDefinitions()
    {
        return new Dictionary<string, Dictionary<string, object>>
        {
            // Level 2 Perks
            ["extra_member_5"] = new Dictionary<string, object>
            {
                ["name"] = "Extra Members I",
                ["description"] = "+5 max guild members",
                ["level_required"] = 2,
                ["type"] = "member_bonus",
                ["value"] = 5
            },
            ["gold_bonus_5"] = new Dictionary<string, object>
            {
                ["name"] = "Gold Bonus I",
                ["description"] = "+5% gold from quests",
                ["level_required"] = 2,
                ["type"] = "gold_bonus",
                ["value"] = 0.05
            },
            
            // Level 3 Perks
            ["extra_member_10"] = new Dictionary<string, object>
            {
                ["name"] = "Extra Members II",
                ["description"] = "+10 max guild members",
                ["level_required"] = 3,
                ["type"] = "member_bonus",
                ["value"] = 10
            },
            ["exp_bonus_5"] = new Dictionary<string, object>
            {
                ["name"] = "Experience Bonus I",
                ["description"] = "+5% exp from quests",
                ["level_required"] = 3,
                ["type"] = "exp_bonus",
                ["value"] = 0.05
            },
            
            // Level 4 Perks
            ["quest_discount_10"] = new Dictionary<string, object>
            {
                ["name"] = "Quest Discount I",
                ["description"] = "-10% quest refresh cost",
                ["level_required"] = 4,
                ["type"] = "quest_discount",
                ["value"] = 0.10
            },
            ["war_bonus_10"] = new Dictionary<string, object>
            {
                ["name"] = "War Bonus I",
                ["description"] = "+10% war score",
                ["level_required"] = 4,
                ["type"] = "war_bonus",
                ["value"] = 0.10
            },
            
            // Level 5 Perks
            ["extra_member_15"] = new Dictionary<string, object>
            {
                ["name"] = "Extra Members III",
                ["description"] = "+15 max guild members",
                ["level_required"] = 5,
                ["type"] = "member_bonus",
                ["value"] = 15
            },
            ["gold_bonus_10"] = new Dictionary<string, object>
            {
                ["name"] = "Gold Bonus II",
                ["description"] = "+10% gold from quests",
                ["level_required"] = 5,
                ["type"] = "gold_bonus",
                ["value"] = 0.10
            },
            
            // Level 6 Perks
            ["tech_discount_15"] = new Dictionary<string, object>
            {
                ["name"] = "Technology Discount I",
                ["description"] = "-15% technology research cost",
                ["level_required"] = 6,
                ["type"] = "tech_discount",
                ["value"] = 0.15
            },
            ["bank_discount_10"] = new Dictionary<string, object>
            {
                ["name"] = "Bank Fee Discount I",
                ["description"] = "-10% bank transaction fee",
                ["level_required"] = 6,
                ["type"] = "bank_discount",
                ["value"] = 0.10
            },
            
            // Level 7 Perks
            ["exp_bonus_10"] = new Dictionary<string, object>
            {
                ["name"] = "Experience Bonus II",
                ["description"] = "+10% exp from quests",
                ["level_required"] = 7,
                ["type"] = "exp_bonus",
                ["value"] = 0.10
            },
            ["war_bonus_15"] = new Dictionary<string, object>
            {
                ["name"] = "War Bonus II",
                ["description"] = "+15% war score",
                ["level_required"] = 7,
                ["type"] = "war_bonus",
                ["value"] = 0.15
            },
            
            // Level 8 Perks
            ["extra_member_20"] = new Dictionary<string, object>
            {
                ["name"] = "Extra Members IV",
                ["description"] = "+20 max guild members",
                ["level_required"] = 8,
                ["type"] = "member_bonus",
                ["value"] = 20
            },
            ["loot_bonus_10"] = new Dictionary<string, object>
            {
                ["name"] = "Loot Bonus I",
                ["description"] = "+10% drop rate in guild events",
                ["level_required"] = 8,
                ["type"] = "loot_bonus",
                ["value"] = 0.10
            },
            
            // Level 9 Perks
            ["gold_bonus_15"] = new Dictionary<string, object>
            {
                ["name"] = "Gold Bonus III",
                ["description"] = "+15% gold from quests",
                ["level_required"] = 9,
                ["type"] = "gold_bonus",
                ["value"] = 0.15
            },
            ["quest_discount_20"] = new Dictionary<string, object>
            {
                ["name"] = "Quest Discount II",
                ["description"] = "-20% quest refresh cost",
                ["level_required"] = 9,
                ["type"] = "quest_discount",
                ["value"] = 0.20
            },
            
            // Level 10 Perks (Max Level Perks)
            ["ultimate_member_bonus"] = new Dictionary<string, object>
            {
                ["name"] = "Ultimate Members",
                ["description"] = "+30 max guild members",
                ["level_required"] = 10,
                ["type"] = "member_bonus",
                ["value"] = 30
            },
            ["ultimate_all_bonus"] = new Dictionary<string, object>
            {
                ["name"] = "Ultimate Blessing",
                ["description"] = "+15% all guild bonuses",
                ["level_required"] = 10,
                ["type"] = "all_bonus",
                ["value"] = 0.15
            }
        };
    }
    
    // Get perks available at a specific level
    public static List<string> GetPerksForLevel(int level)
    {
        var perks = new List<string>();
        var definitions = GetPerkDefinitions();
        
        foreach (var perk in definitions)
        {
            if ((int)perk.Value["level_required"] <= level)
            {
                perks.Add(perk.Key);
            }
        }
        
        return perks;
    }
}
