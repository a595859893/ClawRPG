using Godot;
using System;
using System.Collections.Generic;

public class RandomQuestDatabase
{
    private static RandomQuestDatabase _instance;
    public static RandomQuestDatabase Instance => _instance ??= new RandomQuestDatabase();
    
    public Dictionary<string, QuestTemplate> QuestTemplates { get; set; } = new Dictionary<string, QuestTemplate>();
    
    public class QuestTemplate
    {
        public string Id { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; }
        public string Difficulty { get; set; }
        public int RequiredAmount { get; set; }
        public int TimeLimit { get; set; }
        public int BaseRewardGold { get; set; }
        public int BaseRewardExp { get; set; }
        public string TargetId { get; set; }
        public float DifficultyMultiplier { get; set; }
    }
    
    public RandomQuestDatabase()
    {
        InitializeQuests();
    }
    
    private void InitializeQuests()
    {
        // Combat Quests - Easy
        AddQuest(new QuestTemplate
        {
            Id = "slime_defeat_5",
            Title = "Slime Hunter",
            Description = "Defeat 5 slimes in the meadow",
            Type = "Combat",
            Difficulty = "Easy",
            RequiredAmount = 5,
            TimeLimit = 300,
            BaseRewardGold = 50,
            BaseRewardExp = 30,
            TargetId = "slime",
            DifficultyMultiplier = 1.0f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "goblin_defeat_8",
            Title = "Goblin Clearer",
            Description = "Defeat 8 goblins in the forest",
            Type = "Combat",
            Difficulty = "Easy",
            RequiredAmount = 8,
            TimeLimit = 360,
            BaseRewardGold = 70,
            BaseRewardExp = 40,
            TargetId = "goblin",
            DifficultyMultiplier = 1.0f
        });
        
        // Combat Quests - Medium
        AddQuest(new QuestTemplate
        {
            Id = "wolf_pack_10",
            Title = "Wolf Pack Exterminator",
            Description = "Defeat 10 wolves in the northern woods",
            Type = "Combat",
            Difficulty = "Medium",
            RequiredAmount = 10,
            TimeLimit = 420,
            BaseRewardGold = 150,
            BaseRewardExp = 80,
            TargetId = "wolf",
            DifficultyMultiplier = 1.5f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "skeleton_army_12",
            Title = "Skeleton Exorcist",
            Description = "Defeat 12 skeletons in the crypt",
            Type = "Combat",
            Difficulty = "Medium",
            RequiredAmount = 12,
            TimeLimit = 480,
            BaseRewardGold = 180,
            BaseRewardExp = 100,
            TargetId = "skeleton",
            DifficultyMultiplier = 1.5f
        });
        
        // Combat Quests - Hard
        AddQuest(new QuestTemplate
        {
            Id = "orc_chief_5",
            Title = "Orc Warlord Slayer",
            Description = "Defeat 5 orc chiefs in the stronghold",
            Type = "Combat",
            Difficulty = "Hard",
            RequiredAmount = 5,
            TimeLimit = 600,
            BaseRewardGold = 350,
            BaseRewardExp = 200,
            TargetId = "orc_chief",
            DifficultyMultiplier = 2.0f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "troll_hunter_8",
            Title = "Troll Hunter",
            Description = "Defeat 8 trolls in the mountain caves",
            Type = "Combat",
            Difficulty = "Hard",
            RequiredAmount = 8,
            TimeLimit = 720,
            BaseRewardGold = 400,
            BaseRewardExp = 250,
            TargetId = "troll",
            DifficultyMultiplier = 2.0f
        });
        
        // Combat Quests - Epic
        AddQuest(new QuestTemplate
        {
            Id = "dragon_slayer_3",
            Title = "Dragon Slayer",
            Description = "Defeat 3 dragons in the volcanic region",
            Type = "Combat",
            Difficulty = "Epic",
            RequiredAmount = 3,
            TimeLimit = 900,
            BaseRewardGold = 800,
            BaseRewardExp = 500,
            TargetId = "dragon",
            DifficultyMultiplier = 3.0f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "demon_lord_1",
            Title = "Demon Lord Banisher",
            Description = "Defeat the Demon Lord in the underworld",
            Type = "Combat",
            Difficulty = "Epic",
            RequiredAmount = 1,
            TimeLimit = 1200,
            BaseRewardGold = 1500,
            BaseRewardExp = 1000,
            TargetId = "demon_lord",
            DifficultyMultiplier = 4.0f
        });
        
        // Collection Quests - Easy
        AddQuest(new QuestTemplate
        {
            Id = "herb_gather_10",
            Title = "Herbalist",
            Description = "Gather 10 healing herbs",
            Type = "Collection",
            Difficulty = "Easy",
            RequiredAmount = 10,
            TimeLimit = 240,
            BaseRewardGold = 40,
            BaseRewardExp = 25,
            TargetId = "healing_herb",
            DifficultyMultiplier = 1.0f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "ore_collect_15",
            Title = "Miner",
            Description = "Collect 15 iron ore pieces",
            Type = "Collection",
            Difficulty = "Easy",
            RequiredAmount = 15,
            TimeLimit = 300,
            BaseRewardGold = 60,
            BaseRewardExp = 35,
            TargetId = "iron_ore",
            DifficultyMultiplier = 1.0f
        });
        
        // Collection Quests - Medium
        AddQuest(new QuestTemplate
        {
            Id = "herb_master_25",
            Title = "Master Herbalist",
            Description = "Gather 25 rare herbs",
            Type = "Collection",
            Difficulty = "Medium",
            RequiredAmount = 25,
            TimeLimit = 480,
            BaseRewardGold = 160,
            BaseRewardExp = 90,
            TargetId = "rare_herb",
            DifficultyMultiplier = 1.5f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "gem_finder_8",
            Title = "Gem Hunter",
            Description = "Find 8 precious gems",
            Type = "Collection",
            Difficulty = "Medium",
            RequiredAmount = 8,
            TimeLimit = 540,
            BaseRewardGold = 200,
            BaseRewardExp = 120,
            TargetId = "precious_gem",
            DifficultyMultiplier = 1.5f
        });
        
        // Collection Quests - Hard
        AddQuest(new QuestTemplate
        {
            Id = "ancient_artifact_3",
            Title = "Artifact Collector",
            Description = "Find 3 ancient artifacts",
            Type = "Collection",
            Difficulty = "Hard",
            RequiredAmount = 3,
            TimeLimit = 720,
            BaseRewardGold = 450,
            BaseRewardExp = 300,
            TargetId = "ancient_artifact",
            DifficultyMultiplier = 2.0f
        });
        
        // Exploration Quests - Easy
        AddQuest(new QuestTemplate
        {
            Id = "cave_explore_1",
            Title = "Cave Explorer",
            Description = "Explore the hidden cave",
            Type = "Exploration",
            Difficulty = "Easy",
            RequiredAmount = 1,
            TimeLimit = 180,
            BaseRewardGold = 30,
            BaseRewardExp = 20,
            TargetId = "hidden_cave",
            DifficultyMultiplier = 1.0f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "tower_climb_1",
            Title = "Tower Ascender",
            Description = "Climb to the top of the old tower",
            Type = "Exploration",
            Difficulty = "Easy",
            RequiredAmount = 1,
            TimeLimit = 240,
            BaseRewardGold = 45,
            BaseRewardExp = 30,
            TargetId = "old_tower",
            DifficultyMultiplier = 1.0f
        });
        
        // Exploration Quests - Medium
        AddQuest(new QuestTemplate
        {
            Id = "dungeon_clear_3",
            Title = "Dungeon Delver",
            Description = "Clear 3 floors of the ancient dungeon",
            Type = "Exploration",
            Difficulty = "Medium",
            RequiredAmount = 3,
            TimeLimit = 600,
            BaseRewardGold = 220,
            BaseRewardExp = 150,
            TargetId = "ancient_dungeon",
            DifficultyMultiplier = 1.5f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "temple_discover_1",
            Title = "Temple Discoverer",
            Description = "Discover the lost temple in the jungle",
            Type = "Exploration",
            Difficulty = "Medium",
            RequiredAmount = 1,
            TimeLimit = 480,
            BaseRewardGold = 180,
            BaseRewardExp = 110,
            TargetId = "lost_temple",
            DifficultyMultiplier = 1.5f
        });
        
        // Exploration Quests - Hard
        AddQuest(new QuestTemplate
        {
            Id = "abyss_explore_5",
            Title = "Abyss Explorer",
            Description = "Explore 5 floors of the abyss",
            Type = "Exploration",
            Difficulty = "Hard",
            RequiredAmount = 5,
            TimeLimit = 900,
            BaseRewardGold = 500,
            BaseRewardExp = 350,
            TargetId = "the_abyss",
            DifficultyMultiplier = 2.0f
        });
        
        // Delivery Quests - Easy
        AddQuest(new QuestTemplate
        {
            Id = "letter_deliver_1",
            Title = "Message Courier",
            Description = "Deliver the letter to the village elder",
            Type = "Delivery",
            Difficulty = "Easy",
            RequiredAmount = 1,
            TimeLimit = 300,
            BaseRewardGold = 35,
            BaseRewardExp = 20,
            TargetId = "village_elder",
            DifficultyMultiplier = 1.0f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "package_deliver_1",
            Title = "Package Delivery",
            Description = "Deliver the package to the merchant",
            Type = "Delivery",
            Difficulty = "Easy",
            RequiredAmount = 1,
            TimeLimit = 360,
            BaseRewardGold = 45,
            BaseRewardExp = 25,
            TargetId = "merchant",
            DifficultyMultiplier = 1.0f
        });
        
        // Delivery Quests - Medium
        AddQuest(new QuestTemplate
        {
            Id = "royal_scroll_1",
            Title = "Royal Messenger",
            Description = "Deliver the royal scroll to the king",
            Type = "Delivery",
            Difficulty = "Medium",
            RequiredAmount = 1,
            TimeLimit = 480,
            BaseRewardGold = 200,
            BaseRewardExp = 120,
            TargetId = "king",
            DifficultyMultiplier = 1.5f
        });
        
        // Escort Quests - Medium
        AddQuest(new QuestTemplate
        {
            Id = "merchant_escort_1",
            Title = "Merchant Guard",
            Description = "Escort the merchant safely to town",
            Type = "Escort",
            Difficulty = "Medium",
            RequiredAmount = 1,
            TimeLimit = 540,
            BaseRewardGold = 170,
            BaseRewardExp = 100,
            TargetId = "merchant_caravan",
            DifficultyMultiplier = 1.5f
        });
        
        AddQuest(new QuestTemplate
        {
            Id = "noble_escort_1",
            Title = "Noble Protector",
            Description = "Escort the noble to the castle",
            Type = "Escort",
            Difficulty = "Medium",
            RequiredAmount = 1,
            TimeLimit = 600,
            BaseRewardGold = 220,
            BaseRewardExp = 130,
            TargetId = "noble_carriage",
            DifficultyMultiplier = 1.5f
        });
        
        // Escort Quests - Hard
        AddQuest(new QuestTemplate
        {
            Id = "princess_rescue_1",
            Title = "Princess Saver",
            Description = "Rescue the princess from the tower",
            Type = "Escort",
            Difficulty = "Hard",
            RequiredAmount = 1,
            TimeLimit = 720,
            BaseRewardGold = 400,
            BaseRewardExp = 250,
            TargetId = "princess",
            DifficultyMultiplier = 2.0f
        });
    }
    
    private void AddQuest(QuestTemplate quest)
    {
        QuestTemplates[quest.Id] = quest;
    }
    
    public List<QuestTemplate> GetQuestsByType(string type)
    {
        List<QuestTemplate> result = new List<QuestTemplate>();
        foreach (var quest in QuestTemplates.Values)
        {
            if (quest.Type == type)
                result.Add(quest);
        }
        return result;
    }
    
    public List<QuestTemplate> GetQuestsByDifficulty(string difficulty)
    {
        List<QuestTemplate> result = new List<QuestTemplate>();
        foreach (var quest in QuestTemplates.Values)
        {
            if (quest.Difficulty == difficulty)
                result.Add(quest);
        }
        return result;
    }
    
    public List<QuestTemplate> GetRandomQuests(int count, int playerLevel)
    {
        List<QuestTemplate> available = new List<QuestTemplate>();
        
        foreach (var quest in QuestTemplates.Values)
        {
            // Filter by player level
            if (playerLevel < 5 && (quest.Difficulty == "Hard" || quest.Difficulty == "Epic"))
                continue;
            if (playerLevel < 10 && quest.Difficulty == "Epic")
                continue;
            
            available.Add(quest);
        }
        
        // Shuffle and return
        List<QuestTemplate> result = new List<QuestTemplate>();
        var random = new Random();
        
        for (int i = 0; i < Math.Min(count, available.Count); i++)
        {
            int index = random.Next(available.Count);
            result.Add(available[index]);
            available.RemoveAt(index);
        }
        
        return result;
    }
    
    public QuestTemplate GetQuest(string questId)
    {
        return QuestTemplates.ContainsKey(questId) ? QuestTemplates[questId] : null;
    }
}
