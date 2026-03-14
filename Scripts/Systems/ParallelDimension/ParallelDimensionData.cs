using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ParallelDimension {
    
    public enum DimensionType {
        Mirror,
        Void,
        Chaos,
        Frozen,
        Infernal,
        Ethereal,
        Dark,
        Light,
        Time,
        Dream
    }
    
    public enum DimensionState {
        Locked,
        Available,
        InProgress,
        Completed,
        Mastered
    }
    
    public class DimensionRule {
        public string Description { get; set; }
        public float EnemyMultiplier { get; set; } = 1.0f;
        public float DropMultiplier { get; set; } = 1.0f;
        public float ExpMultiplier { get; set; } = 1.0f;
        public bool NoDeathPenalty { get; set; } = false;
        public bool GravityReversed { get; set; } = false;
        public bool NoCooldowns { get; set; } = false;
        public bool InfiniteMana { get; set; } = false;
        public string[] AllowedElements { get; set; } = null;
        public string[] ForbiddenElements { get; set; } = null;
    }
    
    public class DimensionEntry {
        public int DimensionId { get; set; }
        public string DimensionName { get; set; }
        public string Description { get; set; }
        public DimensionType Type { get; set; }
        public DimensionState State { get; set; } = DimensionState.Locked;
        public int RequiredLevel { get; set; }
        public int EntryCost { get; set; }
        public int MaxFloors { get; set; } = 10;
        public int CurrentFloor { get; set; } = 1;
        public int BestScore { get; set; }
        public int BestTime { get; set; }
        public int TimesCompleted { get; set; }
        public DateTime LastEntered { get; set; }
        public DimensionRule Rules { get; set; }
    }
    
    public class PlayerDimensionData {
        public int PlayerId { get; set; }
        public List<DimensionEntry> UnlockedDimensions { get; set; } = new List<DimensionEntry>();
        public int TotalDimensionScore { get; set; }
        public int DimensionsMastered { get; set; }
        public Dictionary<int, int> DimensionHighScores { get; set; } = new Dictionary<int, int>();
        public Dictionary<int, int> DimensionCompletions { get; set; } = new Dictionary<int, int>();
    }
    
    public class DimensionReward {
        public int Floor { get; set; }
        public int GoldReward { get; set; }
        public int ExpReward { get; set; }
        public string ItemId { get; set; }
        public float DropChance { get; set; }
    }
}
