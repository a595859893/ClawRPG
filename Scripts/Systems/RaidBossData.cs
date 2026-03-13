using System;
using System.Collections.Generic;

public class RaidBossData
{
    // Raid Instance Data
    public int CurrentRaidId { get; set; }
    public string CurrentRaidName { get; set; }
    public RaidBossType CurrentRaidType { get; set; }
    public int CurrentPhase { get; set; }
    public int MaxPhases { get; set; }
    
    // Player Participation
    public List<RaidParticipant> Participants { get; set; }
    public List<string> JoinedPlayerIds { get; set; }
    public string LeaderId { get; set; }
    
    // Raid State
    public RaidState CurrentState { get; set; }
    public float BossHealth { get; set; }
    public float BossMaxHealth { get; set; }
    public float EnrageTimer { get; set; }
    public float MaxEnrageTime { get; set; }
    
    // Rewards
    public bool RewardsClaimed { get; set; }
    public int TotalGoldReward { get; set; }
    public int TotalExpReward { get; set; }
    public List<string> LootItems { get; set; }
    
    // Statistics
    public int TotalRaidsJoined { get; set; }
    public int TotalRaidsCompleted { get; set; }
    public int TotalRaidsFailed { get; set; }
    public int TotalBossKills { get; set; }
    public int TotalDamageDealt { get; set; }
    public int TotalHealingDone { get; set; }
    public int BestClearTime { get; set; }
    
    // History
    public List<RaidHistoryRecord> History { get; set; }
    
    public RaidBossData()
    {
        Participants = new List<RaidParticipant>();
        JoinedPlayerIds = new List<string>();
        LootItems = new List<string>();
        History = new List<RaidHistoryRecord>();
        CurrentState = RaidState.NotStarted;
        CurrentPhase = 1;
    }
}

public enum RaidBossType
{
    DragonLair,
    DemonCastle,
    AncientTemple,
    VoidRift,
    FrozenCitadel,
    ShadowRealm,
    CelestialPalace,
    AbyssalPit
}

public enum RaidState
{
    NotStarted,
    Recruiting,
    InProgress,
    PhaseComplete,
    Victory,
    Failed,
    Abandoned
}

public class RaidParticipant
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public RaidRole Role { get; set; }
    public int DamageDealt { get; set; }
    public int HealingDone { get; set; }
    public int Deaths { get; set; }
    public bool IsAlive { get; set; }
    public float ContributionPercent { get; set; }
}

public enum RaidRole
{
    Tank,
    Healer,
    Damage,
    Support
}

public class RaidHistoryRecord
{
    public string RaidName { get; set; }
    public DateTime Timestamp { get; set; }
    public bool Victory { get; set; }
    public int ClearTime { get; set; }
    public int DamageDealt { get; set; }
    public int GoldReward { get; set; }
    public int ExpReward { get; set; }
}
