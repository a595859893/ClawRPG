using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public class ArenaTournamentData : Resource
{
    // 锦标赛配置
    public string TournamentName { get; set; } = "Arena Championship";
    public ArenaTournamentType TournamentType { get; set; } = ArenaTournamentType.SingleElimination;
    public ArenaTournamentState State { get; set; } = ArenaTournamentState.Registration;
    
    // 参赛选手
    public List<ArenaTournamentParticipant> Participants { get; set; } = new List<ArenaTournamentParticipant>();
    
    // 比赛记录
    public List<ArenaTournamentMatch> Matches { get; set; } = new List<ArenaTournamentMatch>();
    public List<int> GroupA { get; set; } = new List<int>();
    public List<int> GroupB { get; set; } = new List<int>();
    public List<int> GroupC { get; set; } = new List<int>();
    public List<int> GroupD { get; set; } = new List<int>();
    
    // 当前轮次
    public int CurrentRound { get; set; } = 0;
    public int TotalRounds { get; set; } = 0;
    
    // 锦标赛设置
    public int MaxParticipants { get; set; } = 16;
    public int MinParticipants { get; set; } = 4;
    public int PointsPerWin { get; set; } = 3;
    public int PointsPerDraw { get; set; } = 1;
    public int PointsPerLoss { get; set; } = 0;
    
    // 奖励配置
    public int WinnerReward { get; set; } = 10000;
    public int SecondPlaceReward { get; set; } = 5000;
    public int ThirdPlaceReward { get; set; } = 2500;
    
    // 统计
    public int TotalTournaments { get; set; } = 0;
    public int TournamentsWon { get; set; } = 0;
    public int TournamentsParticipated { get; set; } = 0;
    public int TotalMatchesPlayed { get; set; } = 0;
    public int TotalWins { get; set; } = 0;
    public int TotalLosses { get; set; } = 0;
    public int TotalDraws { get; set; } = 0;
    public int HighestPlacement { get; set; } = 0;
    
    // 历史记录
    public List<ArenaTournamentHistory> History { get; set; } = new List<ArenaTournamentHistory>();
}

public enum ArenaTournamentType
{
    SingleElimination,    // 单败淘汰
    DoubleElimination,    // 双败淘汰
    RoundRobin,           // 循环赛
    Swiss                 // 瑞士制
}

public enum ArenaTournamentState
{
    Registration,    // 报名中
    Seeding,         // 抽签中
    InProgress,      // 进行中
    Completed,       // 已完成
    Cancelled        // 已取消
}

public class ArenaTournamentParticipant
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public int Seed { get; set; } = 0;
    public int Points { get; set; } = 0;
    public int Wins { get; set; } = 0;
    public int Losses { get; set; } = 0;
    public int Draws { get; set; } = 0;
    public int GoalsFor { get; set; } = 0;
    public int GoalsAgainst { get; set; } = 0;
    public int Placement { get; set; } = 0;
    public bool IsEliminated { get; set; } = false;
    public bool IsWinnerBracket { get; set; } = true;
    public int GroupId { get; set; } = 0;
}

public class ArenaTournamentMatch
{
    public int MatchId { get; set; }
    public int Round { get; set; }
    public int Player1Id { get; set; } = -1;
    public int Player2Id { get; set; } = -1;
    public int Player1Score { get; set; } = 0;
    public int Player2Score { get; set; } = 0;
    public int WinnerId { get; set; } = -1;
    public bool IsCompleted { get; set; } = false;
    public bool IsDraw { get; set; } = false;
    public ArenaTournamentMatchState MatchState { get; set; } = ArenaTournamentMatchState.Pending;
}

public enum ArenaTournamentMatchState
{
    Pending,
    Ready,
    InProgress,
    Completed,
    Bye
}

public class ArenaTournamentHistory
{
    public string TournamentName { get; set; } = "";
    public ArenaTournamentType Type { get; set; }
    public int Placement { get; set; }
    public int Participants { get; set; }
    public int Reward { get; set; }
    public long Timestamp { get; set; }
}
