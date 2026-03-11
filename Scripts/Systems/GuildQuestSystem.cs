using Godot;
using System;
using System.Collections.Generic;

public partial class GuildQuestSystem : Node
{
    public static GuildQuestSystem Instance { get; private set; }

    [Export] public int MaxActiveQuests = 5;
    [Export] public int QuestRefreshCost = 100;

    private List<GuildQuest> _activeQuests = new List<GuildQuest>();
    private List<GuildQuest> _completedQuests = new List<GuildQuest>();
    private Dictionary<int, GuildQuestProgress> _progressMap = new Dictionary<int, GuildQuestProgress>();

    private List<GuildQuest> _questTemplates = new List<GuildQuest>
    {
        new GuildQuest { Id = 1, Name = "Monster Hunt", Description = "Defeat monsters in the forest", Type = QuestType.Kill, TargetId = 1, TargetCount = 20, Difficulty = 1, GuildPoints = 100, GoldReward = 500 },
        new GuildQuest { Id = 2, Name = "Boss Slayer", Description = "Defeat a powerful boss", Type = QuestType.KillBoss, TargetId = 1, TargetCount = 1, Difficulty = 3, GuildPoints = 300, GoldReward = 1500 },
        new GuildQuest { Id = 3, Name = "Gold Collector", Description = "Earn gold through trading", Type = QuestType.EarnGold, TargetId = 0, TargetCount = 5000, Difficulty = 2, GuildPoints = 200, GoldReward = 1000 },
        new GuildQuest { Id = 4, Name = "Explorer", Description = "Visit different regions", Type = QuestType.VisitRegion, TargetId = 0, TargetCount = 5, Difficulty = 1, GuildPoints = 150, GoldReward = 750 },
        new GuildQuest { Id = 5, Name = "Craftsman", Description = "Craft items", Type = QuestType.Craft, TargetId = 0, TargetCount = 10, Difficulty = 2, GuildPoints = 250, GoldReward = 1200 },
        new GuildQuest { Id = 6, Name = "Dungeon Master", Description = "Complete dungeons", Type = QuestType.CompleteDungeon, TargetId = 0, TargetCount = 3, Difficulty = 3, GuildPoints = 350, GoldReward = 2000 },
        new GuildQuest { Id = 7, Name = "PvP Champion", Description = "Win PvP battles", Type = QuestType.PvPWins, TargetId = 0, TargetCount = 5, Difficulty = 3, GuildPoints = 400, GoldReward = 2500 },
        new GuildQuest { Id = 8, Name = "Mount Trainer", Description = "Train your mounts", Type = QuestType.TrainMount, TargetId = 0, TargetCount = 10, Difficulty = 2, GuildPoints = 180, GoldReward = 900 },
        new GuildQuest { Id = 9, Name = "Pet Tamer", Description = "Battle with pets", Type = QuestType.PetBattles, TargetId = 0, TargetCount = 15, Difficulty = 2, GuildPoints = 220, GoldReward = 1100 },
        new GuildQuest { Id = 10, Name = "Alchemist", Description = "Brew potions", Type = QuestType.Alchemy, TargetId = 0, TargetCount = 20, Difficulty = 2, GuildPoints = 240, GoldReward = 1300 },
        new GuildQuest { Id = 11, Name = "Fisher", Description = "Catch fish", Type = QuestType.Fishing, TargetId = 0, TargetCount = 25, Difficulty = 1, GuildPoints = 120, GoldReward = 600 },
        new GuildQuest { Id = 12, Name = "Treasure Hunter", Description = "Find treasures", Type = QuestType.FindTreasure, TargetId = 0, TargetCount = 8, Difficulty = 2, GuildPoints = 280, GoldReward = 1400 },
        new GuildQuest { Id = 13, Name = "World Event", Description = "Participate in world events", Type = QuestType.WorldEvent, TargetId = 0, TargetCount = 3, Difficulty = 3, GuildPoints = 320, GoldReward = 1800 },
        new GuildQuest { Id = 14, Name = "Daily Grind", Description = "Complete daily quests", Type = QuestType.DailyQuests, TargetId = 0, TargetCount = 5, Difficulty = 1, GuildPoints = 150, GoldReward = 700 },
        new GuildQuest { Id = 15, Name = "Arena Champion", Description = "Win arena battles", Type = QuestType.ArenaWins, TargetId = 0, TargetCount = 10, Difficulty = 3, GuildPoints = 380, GoldReward = 2200 }
    };

    public override void _Ready()
    {
        Instance = this;
        GenerateDailyQuests();
    }

    public void GenerateDailyQuests()
    {
        _activeQuests.Clear();
        var random = new Random();
        var shuffled = new List<GuildQuest>(_questTemplates);
        
        for (int i = shuffled.Count - 1; i > 0; i--)
        {
            int j = random.Next(i + 1);
            (shuffled[i], shuffled[j]) = (shuffled[j], shuffled[i]);
        }

        for (int i = 0; i < Mathf.Min(MaxActiveQuests, shuffled.Count); i++)
        {
            _activeQuests.Add(new GuildQuest
            {
                Id = shuffled[i].Id,
                Name = shuffled[i].Name,
                Description = shuffled[i].Description,
                Type = shuffled[i].Type,
                TargetId = shuffled[i].TargetId,
                TargetCount = shuffled[i].TargetCount,
                Difficulty = shuffled[i].Difficulty,
                GuildPoints = shuffled[i].GuildPoints,
                GoldReward = shuffled[i].GoldReward,
                CurrentProgress = 0,
                IsCompleted = false
            });
        }
    }

    public void RefreshQuests()
    {
        if (Player.Instance.Gold >= QuestRefreshCost)
        {
            Player.Instance.Gold -= QuestRefreshCost;
            GenerateDailyQuests();
        }
    }

    public void UpdateQuestProgress(QuestType type, int targetId, int amount)
    {
        foreach (var quest in _activeQuests)
        {
            if (quest.Type == type && !quest.IsCompleted)
            {
                if (targetId == 0 || quest.TargetId == targetId || quest.TargetId == 0)
                {
                    quest.CurrentProgress = Mathf.Min(quest.CurrentProgress + amount, quest.TargetCount);
                    
                    if (quest.CurrentProgress >= quest.TargetCount)
                    {
                        CompleteQuest(quest);
                    }
                }
            }
        }
    }

    public void CompleteQuest(GuildQuest quest)
    {
        quest.IsCompleted = true;
        _completedQuests.Add(quest);
        
        if (GuildSystem.Instance != null)
        {
            GuildSystem.Instance.AddGuildPoints(quest.GuildPoints);
        }
        
        Player.Instance.Gold += quest.GoldReward;
        
        GD.Print($"[GuildQuest] Quest completed: {quest.Name} - +{quest.GuildPoints} guild points, +{quest.GoldReward} gold");
    }

    public List<GuildQuest> GetActiveQuests() => _activeQuests;
    public List<GuildQuest> GetCompletedQuests() => _completedQuests;

    public Dictionary<string, object> GetQuestStatistics()
    {
        return new Dictionary<string, object>
        {
            { "total_completed", _completedQuests.Count },
            { "total_points", SumQuestPoints() },
            { "total_gold", SumQuestGold() },
            { "by_difficulty", GetQuestsByDifficulty() }
        };
    }

    private int SumQuestPoints()
    {
        int sum = 0;
        foreach (var q in _completedQuests) sum += q.GuildPoints;
        return sum;
    }

    private int SumQuestGold()
    {
        int sum = 0;
        foreach (var q in _completedQuests) sum += q.GoldReward;
        return sum;
    }

    private Dictionary<int, int> GetQuestsByDifficulty()
    {
        var dict = new Dictionary<int, int>();
        foreach (var q in _completedQuests)
        {
            if (!dict.ContainsKey(q.Difficulty)) dict[q.Difficulty] = 0;
            dict[q.Difficulty]++;
        }
        return dict;
    }

    public Dictionary<string, Variant> Save()
    {
        var data = new Dictionary<string, Variant>();
        
        var activeQuestList = new List<Dictionary<string, Variant>>();
        foreach (var q in _activeQuests)
        {
            activeQuestList.Add(new Dictionary<string, Variant>
            {
                { "id", q.Id },
                { "name", q.Name },
                { "description", q.Description },
                { "type", (int)q.Type },
                { "target_id", q.TargetId },
                { "target_count", q.TargetCount },
                { "current_progress", q.CurrentProgress },
                { "difficulty", q.Difficulty },
                { "guild_points", q.GuildPoints },
                { "gold_reward", q.GoldReward },
                { "is_completed", q.IsCompleted }
            });
        }
        data["active_quests"] = activeQuestList;

        var completedQuestList = new List<Dictionary<string, Variant>>();
        foreach (var q in _completedQuests)
        {
            completedQuestList.Add(new Dictionary<string, Variant>
            {
                { "id", q.Id },
                { "name", q.Name },
                { "guild_points", q.GuildPoints },
                { "gold_reward", q.GoldReward }
            });
        }
        data["completed_quests"] = completedQuestList;

        return data;
    }

    public void Load(Dictionary<string, Variant> data)
    {
        if (data.ContainsKey("active_quests"))
        {
            _activeQuests.Clear();
            var questArray = (Godot.Collections.Array)data["active_quests"];
            foreach (Dictionary<string, Variant> q in questArray)
            {
                _activeQuests.Add(new GuildQuest
                {
                    Id = (int)q["id"],
                    Name = (string)q["name"],
                    Description = (string)q["description"],
                    Type = (QuestType)(int)q["type"],
                    TargetId = (int)q["target_id"],
                    TargetCount = (int)q["target_count"],
                    CurrentProgress = (int)q["current_progress"],
                    Difficulty = (int)q["difficulty"],
                    GuildPoints = (int)q["guild_points"],
                    GoldReward = (int)q["gold_reward"],
                    IsCompleted = (bool)q["is_completed"]
                });
            }
        }

        if (data.ContainsKey("completed_quests"))
        {
            _completedQuests.Clear();
            var questArray = (Godot.Collections.Array)data["completed_quests"];
            foreach (Dictionary<string, Variant> q in questArray)
            {
                _completedQuests.Add(new GuildQuest
                {
                    Id = (int)q["id"],
                    Name = (string)q["name"],
                    GuildPoints = (int)q["guild_points"],
                    GoldReward = (int)q["gold_reward"],
                    IsCompleted = true
                });
            }
        }
    }
}

public class GuildQuest
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public QuestType Type { get; set; }
    public int TargetId { get; set; }
    public int TargetCount { get; set; }
    public int CurrentProgress { get; set; }
    public int Difficulty { get; set; }
    public int GuildPoints { get; set; }
    public int GoldReward { get; set; }
    public bool IsCompleted { get; set; }
}

public class GuildQuestProgress
{
    public int QuestId { get; set; }
    public int Progress { get; set; }
    public bool IsCompleted { get; set; }
}

public enum QuestType
{
    Kill,
    KillBoss,
    EarnGold,
    VisitRegion,
    Craft,
    CompleteDungeon,
    PvPWins,
    TrainMount,
    PetBattles,
    Alchemy,
    Fishing,
    FindTreasure,
    WorldEvent,
    DailyQuests,
    ArenaWins
}
