using Godot;
using System;
using System.Collections.Generic;

public partial class WeeklyChallengeData : Resource
{
    public int WeekNumber { get; set; }
    public int Year { get; set; }
    public Dictionary<string, WeeklyChallenge> Challenges { get; set; } = new Dictionary<string, WeeklyChallenge>();
    public int TotalPoints { get; set; }
    public int CompletedChallenges { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public bool RewardsClaimed { get; set; }
}

public class WeeklyChallenge
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public ChallengeType Type { get; set; }
    public ChallengeDifficulty Difficulty { get; set; }
    public int TargetValue { get; set; }
    public int CurrentValue { get; set; }
    public int Points { get; set; }
    public bool IsCompleted { get; set; }
    public int RewardGold { get; set; }
    public int RewardExp { get; set; }
}

public enum ChallengeType
{
    Combat,
    Exploration,
    Collection,
    Crafting,
    Social,
    Economy
}

public enum ChallengeDifficulty
{
    Easy,
    Medium,
    Hard,
    Epic
}

public class WeeklyChallengeDatabase
{
    private static readonly Dictionary<string, List<WeeklyChallengeTemplate>> Templates = new Dictionary<string, List<WeeklyChallengeTemplate>>
    {
        ["Combat"] = new List<WeeklyChallengeTemplate>
        {
            new WeeklyChallengeTemplate { Id = "kill_enemies", Name = "Monster Slayer", Description = "Defeat monsters in combat", TargetValue = 100, Points = 10, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "kill_bosses", Name = "Boss Hunter", Description = "Defeat boss enemies", TargetValue = 10, Points = 25, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "pvp_wins", Name = "Arena Champion", Description = "Win PvP battles", TargetValue = 20, Points = 30, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "damage_dealt", Name = "Damage Dealer", Description = "Deal total damage", TargetValue = 50000, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "critical_hits", Name = "Critical Strike", Description = "Land critical hits", TargetValue = 50, Points = 15, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "perfect_dodge", Name = "Shadow Dancer", Description = "Perfectly dodge attacks", TargetValue = 30, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "survive_combat", Name = "Survivor", Description = "Win battles while low health", TargetValue = 10, Points = 25, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "elemental_kills", Name = "Elemental Master", Description = "Kill enemies with elements", TargetValue = 100, Points = 30, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "combo_kills", Name = "Combo King", Description = "Kill enemies with combos", TargetValue = 50, Points = 35, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "epic_boss", Name = "Legendary Hunter", Description = "Defeat legendary bosses", TargetValue = 3, Points = 50, Difficulty = ChallengeDifficulty.Epic }
        },
        ["Exploration"] = new List<WeeklyChallengeTemplate>
        {
            new WeeklyChallengeTemplate { Id = "explore_areas", Name = "Explorer", Description = "Discover new areas", TargetValue = 10, Points = 15, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "complete_quests", Name = "Quest Hero", Description = "Complete quests", TargetValue = 20, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "tower_floors", Name = "Tower Climber", Description = "Reach floor in Sealed Tower", TargetValue = 20, Points = 30, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "dungeon_clear", Name = "Dungeon Master", Description = "Clear dungeon floors", TargetValue = 30, Points = 25, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "realm_visit", Name = "Dream Walker", Description = "Visit dream realms", TargetValue = 15, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "treasure_find", Name = "Treasure Hunter", Description = "Find treasures", TargetValue = 20, Points = 25, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "world_event", Name = "Event Attendee", Description = "Participate in world events", TargetValue = 5, Points = 30, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "secret_discovery", Name = "Secret Seeker", Description = "Discover secrets", TargetValue = 10, Points = 40, Difficulty = ChallengeDifficulty.Epic }
        },
        ["Collection"] = new List<WeeklyChallengeTemplate>
        {
            new WeeklyChallengeTemplate { Id = "collect_items", Name = "Collector", Description = "Collect items", TargetValue = 50, Points = 10, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "collect_pets", Name = "Pet Collector", Description = "Add new pets", TargetValue = 5, Points = 25, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "collect_mounts", Name = "Mount Collector", Description = "Obtain new mounts", TargetValue = 3, Points = 25, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "unlock_artifacts", Name = "Artifact Hunter", Description = "Unlock artifacts", TargetValue = 5, Points = 30, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "complete_sets", Name = "Set Collector", Description = "Complete equipment sets", TargetValue = 2, Points = 35, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "collect_runes", Name = "Rune Collector", Description = "Collect runes", TargetValue = 10, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "unlock_titles", Name = "Title Hunter", Description = "Unlock new titles", TargetValue = 5, Points = 25, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "rare_collection", Name = "Rare Collector", Description = "Collect rare items", TargetValue = 10, Points = 40, Difficulty = ChallengeDifficulty.Epic }
        },
        ["Crafting"] = new List<WeeklyChallengeTemplate>
        {
            new WeeklyChallengeTemplate { Id = "craft_items", Name = "Crafter", Description = "Craft items", TargetValue = 30, Points = 15, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "mastery_level", Name = "Master Crafter", Description = "Reach mastery level", TargetValue = 5, Points = 30, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "enchant_items", Name = "Enchanter", Description = "Enchant equipment", TargetValue = 20, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "cook_food", Name = "Chef", Description = "Cook food", TargetValue = 30, Points = 15, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "alchemy_potions", Name = "Alchemist", Description = "Brew potions", TargetValue = 20, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "gear_enhance", Name = "Enhancer", Description = "Enhance equipment", TargetValue = 15, Points = 25, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "legendary_craft", Name = "Legendary Artisan", Description = "Craft legendary items", TargetValue = 3, Points = 50, Difficulty = ChallengeDifficulty.Epic }
        },
        ["Social"] = new List<WeeklyChallengeTemplate>
        {
            new WeeklyChallengeTemplate { Id = "guild_tasks", Name = "Guild Worker", Description = "Complete guild tasks", TargetValue = 20, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "friend_added", Name = "Social Butterfly", Description = "Add friends", TargetValue = 10, Points = 15, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "trade_items", Name = "Trader", Description = "Trade with players", TargetValue = 15, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "guild_donate", Name = "Generous Member", Description = "Donate to guild", TargetValue = 5000, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "help_newbies", Name = "Mentor", Description = "Help new players", TargetValue = 10, Points = 30, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "tournament_join", Name = "Competitor", Description = "Join tournaments", TargetValue = 5, Points = 25, Difficulty = ChallengeDifficulty.Medium }
        },
        ["Economy"] = new List<WeeklyChallengeTemplate>
        {
            new WeeklyChallengeTemplate { Id = "earn_gold", Name = "Wealthy", Description = "Earn gold", TargetValue = 10000, Points = 10, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "spend_gold", Name = "Big Spender", Description = "Spend gold", TargetValue = 10000, Points = 10, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "trade_volume", Name = "Merchant", Description = "High trade volume", TargetValue = 50000, Points = 25, Difficulty = ChallengeDifficulty.Hard },
            new WeeklyChallengeTemplate { Id = "investment", Name = "Investor", Description = "Invest in guild bank", TargetValue = 10000, Points = 20, Difficulty = ChallengeDifficulty.Medium },
            new WeeklyChallengeTemplate { Id = "shop_purchases", Name = "Customer", Description = "Make shop purchases", TargetValue = 30, Points = 15, Difficulty = ChallengeDifficulty.Easy },
            new WeeklyChallengeTemplate { Id = "auction_trades", Name = "Auction Master", Description = "Complete auction trades", TargetValue = 10, Points = 25, Difficulty = ChallengeDifficulty.Medium }
        }
    };

    public static List<WeeklyChallenge> GenerateWeeklyChallenges()
    {
        var random = new Random();
        var challenges = new List<WeeklyChallenge>();
        
        // Generate 6-8 random challenges
        int challengeCount = random.Next(6, 9);
        
        // Ensure at least one from Combat and Exploration
        var combatTemplate = Templates["Combat"][random.Next(Templates["Combat"].Count)];
        challenges.Add(CreateChallenge(combatTemplate));
        challengeCount--;
        
        var explorationTemplate = Templates["Exploration"][random.Next(Templates["Exploration"].Count)];
        challenges.Add(CreateChallenge(explorationTemplate));
        challengeCount--;
        
        // Fill remaining slots with random challenges
        var categories = new List<string> { "Combat", "Exploration", "Collection", "Crafting", "Social", "Economy" };
        
        for (int i = 0; i < challengeCount; i++)
        {
            var category = categories[random.Next(categories.Count)];
            var template = Templates[category][random.Next(Templates[category].Count)];
            
            // Avoid duplicates
            if (!challenges.Exists(c => c.Id == template.Id))
            {
                challenges.Add(CreateChallenge(template));
            }
        }
        
        return challenges;
    }
    
    private static WeeklyChallenge CreateChallenge(WeeklyChallengeTemplate template)
    {
        int baseGold = 500;
        int baseExp = 200;
        
        // Scale rewards by difficulty
        switch (template.Difficulty)
        {
            case ChallengeDifficulty.Easy:
                baseGold *= 1;
                baseExp *= 1;
                break;
            case ChallengeDifficulty.Medium:
                baseGold *= 2;
                baseExp *= 2;
                break;
            case ChallengeDifficulty.Hard:
                baseGold *= 3;
                baseExp *= 3;
                break;
            case ChallengeDifficulty.Epic:
                baseGold *= 5;
                baseExp *= 5;
                break;
        }
        
        return new WeeklyChallenge
        {
            Id = template.Id,
            Name = template.Name,
            Description = template.Description,
            Type = GetChallengeType(template.Id),
            Difficulty = template.Difficulty,
            TargetValue = template.TargetValue,
            CurrentValue = 0,
            Points = template.Points,
            IsCompleted = false,
            RewardGold = baseGold + template.Points * 50,
            RewardExp = baseExp + template.Points * 20
        };
    }
    
    private static ChallengeType GetChallengeType(string id)
    {
        if (id.Contains("kill") || id.Contains("damage") || id.Contains("pvp") || id.Contains("combo") || id.Contains("dodge") || id.Contains("survive"))
            return ChallengeType.Combat;
        if (id.Contains("explore") || id.Contains("quest") || id.Contains("tower") || id.Contains("dungeon") || id.Contains("realm") || id.Contains("treasure") || id.Contains("event") || id.Contains("secret"))
            return ChallengeType.Exploration;
        if (id.Contains("collect") || id.Contains("unlock") || id.Contains("complete_") || id.Contains("set") || id.Contains("rare"))
            return ChallengeType.Collection;
        if (id.Contains("craft") || id.Contains("mastery") || id.Contains("enchant") || id.Contains("cook") || id.Contains("alchemy") || id.Contains("enhance") || id.Contains("legendary"))
            return ChallengeType.Crafting;
        if (id.Contains("guild") || id.Contains("friend") || id.Contains("trade") || id.Contains("help") || id.Contains("tournament"))
            return ChallengeType.Social;
        return ChallengeType.Economy;
    }
}

public class WeeklyChallengeTemplate
{
    public string Id { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public int TargetValue { get; set; }
    public int Points { get; set; }
    public ChallengeDifficulty Difficulty { get; set; }
}
