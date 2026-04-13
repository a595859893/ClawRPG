using Godot;
using System;
using System.Collections.Generic;

public partial class RandomQuestData : Resource
{
    [Export] public List<ActiveQuest> ActiveQuests { get; set; } = new List<ActiveQuest>();
    [Export] public List<string> CompletedQuestIds { get; set; } = new List<string>();
    [Export] public List<string> FailedQuestIds { get; set; } = new List<string>();
    [Export] public Dictionary<string, int> QuestCompletionCount { get; set; } = new Dictionary<string, int>();
    [Export] public int TotalQuestsGenerated { get; set; }
    [Export] public int TotalQuestsCompleted { get; set; }
    [Export] public int TotalQuestsFailed { get; set; }
    [Export] public int TotalQuestRewards { get; set; } // Total gold earned
    
    public class ActiveQuest
    {
        public string QuestId { get; set; }
        public string Title { get; set; }
        public string Description { get; set; }
        public string Type { get; set; } // Combat/Collection/Exploration/Delivery/Escort
        public string Difficulty { get; set; } // Easy/Medium/Hard/Epic
        public int RequiredAmount { get; set; }
        public int CurrentAmount { get; set; }
        public int TimeLimit { get; set; } // Seconds remaining
        public int RewardGold { get; set; }
        public int RewardExp { get; set; }
        public string TargetId { get; set; } // Enemy/item/NPC ID
        public DateTime StartTime { get; set; }
    }
}
