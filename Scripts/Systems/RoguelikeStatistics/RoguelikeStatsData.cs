using Godot;
using System;
using System.Collections.Generic;

public class RoguelikeStatsData
{
    // Run history
    public List<RoguelikeRunRecord> RunHistory = new List<RoguelikeRunRecord>();
    
    // Lifetime statistics
    public int TotalRuns = 0;
    public int TotalWins = 0;
    public int TotalDeaths = 0;
    public int HighestFloorReached = 0;
    public int TotalEnemiesKilled = 0;
    public int TotalDamageDealt = 0;
    public int TotalDamageTaken = 0;
    public int TotalGoldEarned = 0;
    public int TotalGoldSpent = 0;
    public int TotalItemsCollected = 0;
    public int TotalPetsObtained = 0;
    
    // Streak tracking
    public int CurrentWinStreak = 0;
    public int BestWinStreak = 0;
    public int CurrentLossStreak = 0;
    public int BestLossStreak = 0;
    
    // Death causes tracking
    public Dictionary<string, int> DeathCauses = new Dictionary<string, int>();
    
    // Favorite builds
    public string MostUsedClass = "";
    public int MostUsedClassCount = 0;
    public string MostUsedBuild = "";
    public int MostUsedBuildCount = 0;
}

public class RoguelikeRunRecord
{
    public int RunId;
    public DateTime StartTime;
    public DateTime EndTime;
    public int Duration; // in seconds
    public bool Victory;
    public int FloorReached;
    public string CharacterClass;
    public string BuildType;
    public int EnemiesKilled;
    public int BossesKilled;
    public int DamageDealt;
    public int DamageTaken;
    public int GoldEarned;
    public int GoldSpent;
    public int ItemsCollected;
    public int PetsObtained;
    public int DamagePerSecond;
    public string DeathCause;
    public List<string> KeyEvents = new List<string>();
}
