using Godot;
using System;
using System.Collections.Generic;

public class MilestoneDatabase
{
    private static MilestoneDatabase _instance;
    public static MilestoneDatabase Instance => _instance ?? (_instance = new MilestoneDatabase());
    
    public Dictionary<string, MilestoneConfig> Milestones { get; private set; } = new Dictionary<string, MilestoneConfig>();
    
    public MilestoneDatabase()
    {
        InitializeMilestones();
    }
    
    public class MilestoneConfig
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Tier { get; set; }
        public int RequiredValue { get; set; }
        public Dictionary<string, int> Rewards { get; set; } = new Dictionary<string, int>();
    }
    
    private void InitializeMilestones()
    {
        // Combat Milestones
        AddMilestone("combat_first_kill", "First Blood", "Defeat your first enemy", "Combat", "Bronze", 1, 
            new Dictionary<string, int> { { "gold", 10 }, { "exp", 50 } });
        AddMilestone("combat_10_kills", "Monster Hunter", "Defeat 10 enemies", "Combat", "Bronze", 10, 
            new Dictionary<string, int> { { "gold", 50 }, { "exp", 200 } });
        AddMilestone("combat_50_kills", "Veteran Slayer", "Defeat 50 enemies", "Combat", "Silver", 50, 
            new Dictionary<string, int> { { "gold", 150 }, { "exp", 500 } });
        AddMilestone("combat_100_kills", "Enemy Nemesis", "Defeat 100 enemies", "Combat", "Gold", 100, 
            new Dictionary<string, int> { { "gold", 300 }, { "exp", 1000 } });
        AddMilestone("combat_500_kills", "War Champion", "Defeat 500 enemies", "Combat", "Platinum", 500, 
            new Dictionary<string, int> { { "gold", 1000 }, { "exp", 3000 } });
        AddMilestone("combat_1000_kills", "Legendary Warrior", "Defeat 1000 enemies", "Combat", "Diamond", 1000, 
            new Dictionary<string, int> { { "gold", 2500 }, { "exp", 8000 } });
        AddMilestone("combat_5000_kills", "God of War", "Defeat 5000 enemies", "Combat", "Legendary", 5000, 
            new Dictionary<string, int> { { "gold", 10000 }, { "exp", 25000 } });
            
        // Boss Milestones
        AddMilestone("boss_first", "Boss Breaker", "Defeat your first boss", "Boss", "Bronze", 1, 
            new Dictionary<string, int> { { "gold", 100 }, { "exp", 500 } });
        AddMilestone("boss_5", "Boss Hunter", "Defeat 5 bosses", "Boss", "Silver", 5, 
            new Dictionary<string, int> { { "gold", 300 }, { "exp", 1500 } });
        AddMilestone("boss_25", "Boss Slayer", "Defeat 25 bosses", "Boss", "Gold", 25, 
            new Dictionary<string, int> { { "gold", 1000 }, { "exp", 5000 } });
        AddMilestone("boss_100", "Boss Master", "Defeat 100 bosses", "Boss", "Platinum", 100, 
            new Dictionary<string, int> { { "gold", 5000 }, { "exp", 15000 } });
        AddMilestone("boss_500", "Boss Legend", "Defeat 500 bosses", "Boss", "Legendary", 500, 
            new Dictionary<string, int> { { "gold", 20000 }, { "exp", 50000 } });
            
        // Level Milestones
        AddMilestone("level_5", "Apprentice", "Reach level 5", "Level", "Bronze", 5, 
            new Dictionary<string, int> { { "gold", 50 }, { "exp", 100 } });
        AddMilestone("level_10", "Journeyman", "Reach level 10", "Level", "Silver", 10, 
            new Dictionary<string, int> { { "gold", 100 }, { "exp", 300 } });
        AddMilestone("level_25", "Expert", "Reach level 25", "Level", "Gold", 25, 
            new Dictionary<string, int> { { "gold", 500 }, { "exp", 2000 } });
        AddMilestone("level_50", "Master", "Reach level 50", "Level", "Platinum", 50, 
            new Dictionary<string, int> { { "gold", 2000 }, { "exp", 8000 } });
        AddMilestone("level_100", "Grandmaster", "Reach level 100", "Level", "Diamond", 100, 
            new Dictionary<string, int> { { "gold", 5000 }, { "exp", 20000 } });
        AddMilestone("level_200", "Legendary Hero", "Reach level 200", "Level", "Legendary", 200, 
            new Dictionary<string, int> { { "gold", 15000 }, { "exp", 50000 } });
            
        // Gold Milestones
        AddMilestone("gold_1000", "Wealthy Beginner", "Accumulate 1,000 gold", "Gold", "Bronze", 1000, 
            new Dictionary<string, int> { { "gold", 20 }, { "exp", 50 } });
        AddMilestone("gold_10000", "Merchant", "Accumulate 10,000 gold", "Gold", "Silver", 10000, 
            new Dictionary<string, int> { { "gold", 50 }, { "exp", 150 } });
        AddMilestone("gold_100000", "Tycoon", "Accumulate 100,000 gold", "Gold", "Gold", 100000, 
            new Dictionary<string, int> { { "gold", 100 }, { "exp", 500 } });
        AddMilestone("gold_1000000", "Millionaire", "Accumulate 1,000,000 gold", "Gold", "Platinum", 1000000, 
            new Dictionary<string, int> { { "gold", 500 }, { "exp", 2000 } });
        AddMilestone("gold_10000000", "Dragon Hoarder", "Accumulate 10,000,000 gold", "Gold", "Legendary", 10000000, 
            new Dictionary<string, int> { { "gold", 2000 }, { "exp", 8000 } });
            
        // Dungeon Milestones
        AddMilestone("dungeon_first", "Dungeon Explorer", "Complete your first dungeon", "Dungeon", "Bronze", 1, 
            new Dictionary<string, int> { { "gold", 50 }, { "exp", 200 } });
        AddMilestone("dungeon_10", "Dungeon Delver", "Complete 10 dungeons", "Dungeon", "Silver", 10, 
            new Dictionary<string, int> { { "gold", 200 }, { "exp", 800 } });
        AddMilestone("dungeon_50", "Dungeon Master", "Complete 50 dungeons", "Dungeon", "Gold", 50, 
            new Dictionary<string, int> { { "gold", 800 }, { "exp", 3000 } });
        AddMilestone("dungeon_100", "Dungeon Lord", "Complete 100 dungeons", "Dungeon", "Platinum", 100, 
            new Dictionary<string, int> { { "gold", 2000 }, { "exp", 8000 } });
        AddMilestone("dungeon_500", "Dungeon Legend", "Complete 500 dungeons", "Dungeon", "Legendary", 500, 
            new Dictionary<string, int> { { "gold", 8000 }, { "exp", 25000 } });
            
        // Floor Milestones
        AddMilestone("floor_10", "Floor 10", "Reach floor 10", "Floor", "Bronze", 10, 
            new Dictionary<string, int> { { "gold", 100 }, { "exp", 300 } });
        AddMilestone("floor_25", "Floor 25", "Reach floor 25", "Floor", "Silver", 25, 
            new Dictionary<string, int> { { "gold", 300 }, { "exp", 1000 } });
        AddMilestone("floor_50", "Floor 50", "Reach floor 50", "Floor", "Gold", 50, 
            new Dictionary<string, int> { { "gold", 800 }, { "exp", 3000 } });
        AddMilestone("floor_100", "Floor 100", "Reach floor 100", "Floor", "Platinum", 100, 
            new Dictionary<string, int> { { "gold", 2500 }, { "exp", 10000 } });
        AddMilestone("floor_250", "Floor 250", "Reach floor 250", "Floor", "Diamond", 250, 
            new Dictionary<string, int> { { "gold", 8000 }, { "exp", 25000 } });
        AddMilestone("floor_500", "Floor 500", "Reach floor 500", "Floor", "Legendary", 500, 
            new Dictionary<string, int> { { "gold", 25000 }, { "exp", 75000 } });
            
        // Pet Milestones
        AddMilestone("pet_first", "Pet Companion", "Obtain your first pet", "Pet", "Bronze", 1, 
            new Dictionary<string, int> { { "gold", 30 }, { "exp", 100 } });
        AddMilestone("pet_5", "Pet Collector", "Obtain 5 pets", "Pet", "Silver", 5, 
            new Dictionary<string, int> { { "gold", 150 }, { "exp", 500 } });
        AddMilestone("pet_10", "Pet Master", "Obtain 10 pets", "Pet", "Gold", 10, 
            new Dictionary<string, int> { { "gold", 500 }, { "exp", 2000 } });
        AddMilestone("pet_25", "Pet Legend", "Obtain 25 pets", "Pet", "Platinum", 25, 
            new Dictionary<string, int> { { "gold", 2000 }, { "exp", 8000 } });
        AddMilestone("pet_legendary", "Legendary Pet Owner", "Obtain a legendary pet", "Pet", "Diamond", 1, 
            new Dictionary<string, int> { { "gold", 5000 }, { "exp", 15000 } });
            
        // Achievement Milestones
        AddMilestone("achievement_10", "Achiever", "Unlock 10 achievements", "Achievement", "Bronze", 10, 
            new Dictionary<string, int> { { "gold", 100 }, { "exp", 300 } });
        AddMilestone("achievement_25", "Accomplished", "Unlock 25 achievements", "Achievement", "Silver", 25, 
            new Dictionary<string, int> { { "gold", 300 }, { "exp", 1000 } });
        AddMilestone("achievement_50", "Champion", "Unlock 50 achievements", "Achievement", "Gold", 50, 
            new Dictionary<string, int> { { "gold", 800 }, { "exp", 3000 } });
        AddMilestone("achievement_100", "Paragon", "Unlock 100 achievements", "Achievement", "Platinum", 100, 
            new Dictionary<string, int> { { "gold", 2500 }, { "exp", 10000 } });
            
        // Win Milestones
        AddMilestone("win_first", "First Victory", "Win your first game", "Win", "Bronze", 1, 
            new Dictionary<string, int> { { "gold", 200 }, { "exp", 1000 } });
        AddMilestone("win_10", "Seasoned Victor", "Win 10 games", "Win", "Silver", 10, 
            new Dictionary<string, int> { { "gold", 500 }, { "exp", 3000 } });
        AddMilestone("win_50", "Proven Winner", "Win 50 games", "Win", "Gold", 50, 
            new Dictionary<string, int> { { "gold", 2000 }, { "exp", 10000 } });
        AddMilestone("win_100", "Victorious", "Win 100 games", "Win", "Platinum", 100, 
            new Dictionary<string, int> { { "gold", 5000 }, { "exp", 25000 } });
        AddMilestone("win_500", "Champion of Champions", "Win 500 games", "Win", "Legendary", 500, 
            new Dictionary<string, int> { { "gold", 20000 }, { "exp", 80000 } });
    }
    
    private void AddMilestone(string id, string name, string description, string category, string tier, int requiredValue, Dictionary<string, int> rewards)
    {
        Milestones[id] = new MilestoneConfig
        {
            Id = id,
            Name = name,
            Description = description,
            Category = category,
            Tier = tier,
            RequiredValue = requiredValue,
            Rewards = rewards
        };
    }
    
    public List<MilestoneConfig> GetMilestonesByCategory(string category)
    {
        var result = new List<MilestoneConfig>();
        foreach (var milestone in Milestones.Values)
        {
            if (milestone.Category == category)
                result.Add(milestone);
        }
        return result;
    }
    
    public List<string> GetCategories()
    {
        var categories = new List<string>();
        foreach (var milestone in Milestones.Values)
        {
            if (!categories.Contains(milestone.Category))
                categories.Add(milestone.Category);
        }
        return categories;
    }
}
