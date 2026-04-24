using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Data
{
    /// <summary>
    /// 锦标赛阶段类型
    /// </summary>
    public enum TournamentStage
    {
        Registration,    // 报名阶段
        GroupStage,      // 小组赛
        QuarterFinals,   // 四分之一决赛
        SemiFinals,      // 半决赛
        Finals,          // 决赛
        Completed        // 已完成
    }

    /// <summary>
    /// 锦标赛赛制类型
    /// </summary>
    public enum TournamentFormat
    {
        SingleElimination,   // 单败淘汰
        DoubleElimination,   // 双败淘汰
        RoundRobin,          // 循环赛
        SwissSystem          // 瑞士制
    }

    /// <summary>
    /// 锦标赛状态
    /// </summary>
    public enum TournamentStatus
    {
        Pending,     // 等待开始
        Active,      // 进行中
        Completed,   // 已完成
        Cancelled    // 已取消
    }

    /// <summary>
    /// 单个参赛选手数据
    /// </summary>
    public class TournamentPlayer
    {
        public string playerId;
        public string playerName;
        public int seedNumber;           // 种子编号
        public int score;                // 当前得分
        public int wins;
        public int losses;
        public int matchesPlayed;
        public bool isEliminated;
        public bool hasLostOnce;         // 双败赛制中使用
        public DateTime registrationTime;
        public List<string> matchHistory = new List<string>();
    }

    /// <summary>
    /// 单场锦标赛比赛
    /// </summary>
    public class TournamentMatch
    {
        public string matchId;
        public int roundNumber;
        public int matchNumber;
        public TournamentStage stage;
        public string player1Id;
        public string player2Id;
        public string winnerId;
        public int player1Score;
        public int player2Score;
        public bool isCompleted;
        public DateTime scheduledTime;
        public DateTime? completedTime;
    }

    /// <summary>
    /// 锦标赛小组（用于循环赛/瑞士制）
    /// </summary>
    public class TournamentGroup
    {
        public string groupId;
        public string groupName;
        public List<string> playerIds = new List<string>();
        public List<TournamentMatch> matches = new List<TournamentMatch>();
    }

    /// <summary>
    /// 完整锦标赛数据
    /// </summary>
    public class Tournament
    {
        public string tournamentId;
        public string tournamentName;
        public string description;
        public TournamentFormat format;
        public TournamentStatus status;
        public TournamentStage currentStage;
        
        public int maxPlayers;
        public int minPlayers;
        public int currentPlayerCount;
        
        public DateTime registrationStart;
        public DateTime registrationEnd;
        public DateTime? startTime;
        public DateTime? endTime;
        
        public int rounds;                    // 预计轮次
        public int currentRound;              // 当前轮次
        
        public List<TournamentPlayer> registeredPlayers = new List<TournamentPlayer>();
        public List<TournamentMatch> matches = new List<TournamentMatch>();
        public List<TournamentGroup> groups = new List<TournamentGroup>();
        
        public int prizePool;                 // 奖金池
        public List<TournamentReward> rewards = new List<TournamentReward>();
        
        public string organizerId;
        public DateTime createdAt;
        public DateTime updatedAt;
    }

    /// <summary>
    /// 锦标赛奖励配置
    /// </summary>
    public class TournamentReward
    {
        public int rankStart;
        public int rankEnd;
        public string rewardType;        // gold/item/title
        public string rewardId;
        public int rewardAmount;
    }

    /// <summary>
    /// 玩家锦标赛记录
    /// </summary>
    public class PlayerTournamentRecord
    {
        public string playerId;
        public string tournamentId;
        public string tournamentName;
        public int finalRank;
        public int score;
        public int wins;
        public int losses;
        public DateTime participatedAt;
    }

    /// <summary>
    /// 玩家锦标赛统计
    /// </summary>
    public class TournamentStatistics
    {
        public string playerId;
        public int totalTournaments;
        public int firstPlace;
        public int secondPlace;
        public int thirdPlace;
        public int top4;
        public int top8;
        public int top16;
        public int totalWins;
        public int totalLosses;
        public int highestRank;
        public int totalPrizeWon;
    }

    /// <summary>
    /// 锦标赛进度数据
    /// </summary>
    public class TournamentProgress
    {
        public string playerId;
        public List<string> participatedTournaments = new List<string>();
        public List<PlayerTournamentRecord> recentRecords = new List<PlayerTournamentRecord>();
        public TournamentStatistics statistics;
    }
}
