using Godot;
using System;
using System.Collections.Generic;

public class BossRushData
{
    // Current run state
    public int CurrentStage { get; set; } = 0;
    public int CurrentBossIndex { get; set; } = 0;
    public bool IsInRush { get; set; } = false;
    public int CurrentStreak { get; set; } = 0;
    public int BestStreak { get; set; } = 0;
    
    // Player state at start of rush
    public float StartingHealth { get; set; }
    public float StartingAttack { get; set; }
    public float StartingDefense { get; set; }
    
    // Current state
    public float CurrentHealth { get; set; }
    public int GoldEarned { get; set; }
    public int ExpEarned { get; set; }
    public int BossesDefeated { get; set; }
    
    // Rush history
    public List<BossRushRecord> RushHistory { get; set; } = new List<BossRushRecord>();
    
    // Statistics
    public int TotalRushAttempts { get; set; }
    public int TotalVictories { get; set; }
    public int TotalBossesDefeated { get; set; }
    public int HighestStageReached { get; set; }
    public int TotalGoldEarned { get; set; }
    public int TotalExpEarned { get; set; }
}

public class BossRushRecord
{
    public int Stage { get; set; }
    public int BossesDefeated { get; set; }
    public int GoldEarned { get; set; }
    public int ExpEarned { get; set; }
    public bool Victory { get; set; }
    public long Timestamp { get; set; }
}

public enum BossRushState
{
    NotStarted,
    Preparing,
    InProgress,
    Victory,
    Defeated,
    Paused
}
