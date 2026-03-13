using Godot;
using System;
using System.Collections.Generic;

public class SeededRunData : Resource
{
    [Export] public int TotalSeededRuns { get; set; } = 0;
    [Export] public Dictionary<string, SeededRunRecord> SeedHistory { get; set; } = new Dictionary<string, SeededRunRecord>();
    [Export] public string LastUsedSeed { get; set; } = "";
    [Export] public bool IsSeededModeActive { get; set; } = false;
    
    public SeededRunData()
    {
        SeedHistory = new Dictionary<string, SeededRunRecord>();
    }
}

public class SeededRunRecord
{
    public string Seed { get; set; } = "";
    public int RunCount { get; set; } = 0;
    public int BestFloor { get; set; } = 0;
    public int BestScore { get; set; } = 0;
    public float BestTime { get; set; } = 0f;
    public int TotalGold { get; set; } = 0;
    public int TotalExp { get; set; } = 0;
    public int EnemiesDefeated { get; set; } = 0;
    public int BossesDefeated { get; set; } = 0;
    public bool Completed { get; set; } = false;
    public string LastPlayed { get; set; } = "";
    
    public SeededRunRecord() {}
    
    public SeededRunRecord(string seed)
    {
        Seed = seed;
    }
}
