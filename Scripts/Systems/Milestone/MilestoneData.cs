using Godot;
using System;
using System.Collections.Generic;

public class MilestoneData
{
    public Dictionary<string, MilestoneEntry> Milestones { get; set; } = new Dictionary<string, MilestoneEntry>();
    public Dictionary<string, int> CategoryProgress { get; set; } = new Dictionary<string, int>();
    public MilestoneStatistics Statistics { get; set; } = new MilestoneStatistics();
    
    public class MilestoneEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public MilestoneTier Tier { get; set; }
        public int RequiredValue { get; set; }
        public int CurrentValue { get; set; }
        public bool Unlocked { get; set; }
        public DateTime? UnlockTime { get; set; }
        public List<string> Rewards { get; set; } = new List<string>();
    }
    
    public enum MilestoneTier
    {
        Bronze,
        Silver,
        Gold,
        Platinum,
        Diamond,
        Legendary
    }
    
    public class MilestoneStatistics
    {
        public int TotalMilestones { get; set; }
        public int UnlockedMilestones { get; set; }
        public int BronzeMilestones { get; set; }
        public int SilverMilestones { get; set; }
        public int GoldMilestones { get; set; }
        public int PlatinumMilestones { get; set; }
        public int DiamondMilestones { get; set; }
        public int LegendaryMilestones { get; set; }
        public int TotalRewardsClaimed { get; set; }
        public int TotalGoldEarned { get; set; }
        public int TotalExpEarned { get; set; }
    }
}
