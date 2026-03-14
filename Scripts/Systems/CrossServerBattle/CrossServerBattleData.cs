using Godot;
using System;
using System.Collections.Generic;

public enum CrossServerMatchType
{
    OneVOne,
    ThreeVThree,
    FiveVFive,
    TenVTen,
    BossRush,
    TeamDeathmatch
}

public enum CrossServerServerLevel
{
    Bronze,
    Silver,
    Gold,
    Platinum,
    Diamond
}

public enum CrossServerMatchState
{
    Registration,
    Matching,
    InProgress,
    Completed,
    Cancelled
}

public enum CrossServerPlayerResult
{
    None,
    Win,
    Loss,
    Draw
}

[System.Serializable]
public class ServerInfo
{
    public string ServerId { get; set; } = "";
    public string ServerName { get; set; } = "";
    public int PlayerCount { get; set; }
    public int AverageLevel { get; set; }
    public int ServerRank { get; set; }
    public CrossServerServerLevel ServerLevel { get; set; }
    public long LastUpdateTime { get; set; }
}

[System.Serializable]
public class CrossServerBattleRecord
{
    public string PlayerId { get; set; } = "";
    public string PlayerName { get; set; } = "";
    public string ServerId { get; set; } = "";
    public int PersonalRank { get; set; }
    public int TotalPoints { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Draws { get; set; }
    public int CurrentStreak { get; set; }
    public int BestStreak { get; set; }
    public long LastMatchTime { get; set; }
    public int SeasonNumber { get; set; }
}

[System.Serializable]
public class CrossServerMatch
{
    public string MatchId { get; set; } = "";
    public CrossServerMatchType MatchType { get; set; }
    public CrossServerMatchState State { get; set; }
    public List<string> Team1Players { get; set; } = new List<string>();
    public List<string> Team2Players { get; set; } = new List<string>();
    public int Team1Score { get; set; }
    public int Team2Score { get; set; }
    public string Team1ServerId { get; set; } = "";
    public string Team2ServerId { get; set; } = "";
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public int Duration { get; set; }
}

[System.Serializable]
public class CrossServerSeason
{
    public int SeasonNumber { get; set; }
    public long StartTime { get; set; }
    public long EndTime { get; set; }
    public bool IsActive { get; set; }
    public int RewardPoints { get; set; }
}

[System.Serializable]
public class CrossServerBattleData
{
    public List<CrossServerBattleRecord> PlayerRecords { get; set; } = new List<CrossServerBattleRecord>();
    public List<ServerInfo> RegisteredServers { get; set; } = new List<ServerInfo>();
    public List<CrossServerMatch> ActiveMatches { get; set; } = new List<CrossServerMatch>();
    public List<CrossServerMatch> MatchHistory { get; set; } = new List<CrossServerMatch>();
    public CrossServerSeason CurrentSeason { get; set; } = new CrossServerSeason();
    
    // Statistics
    public int TotalMatches { get; set; }
    public int TotalWins { get; set; }
    public int TotalLosses { get; set; }
    public int TotalDraws { get; set; }
    public int BestRanking { get; set; }
    public int HighestPoints { get; set; }
    public int BestStreak { get; set; }
    public long TotalPlayTime { get; set; }
}
