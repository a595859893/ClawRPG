using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.GuildTournamentBracket {
    /// <summary>
    /// 公会锦标赛赛程系统数据
    /// </summary>
    [Serializable]
    public class GuildTournamentBracketData {
        // 锦标赛ID
        public string TournamentId { get; set; }
        
        // 当前阶段
        public TournamentPhase CurrentPhase { get; set; }
        
        // 参赛公会列表
        public List<string> ParticipatingGuilds { get; set; }
        
        // 对阵表
        public List<BracketMatch> Matches { get; set; }
        
        // 当前轮次
        public int CurrentRound { get; set; }
        
        // 总轮次数
        public int TotalRounds { get; set; }
        
        // 比赛结果
        public List<MatchResult> Results { get; set; }
        
        // 冠军公会ID
        public string ChampionGuildId { get; set; }
        
        // 统计
        public TournamentStatistics Statistics { get; set; }
        
        public GuildTournamentBracketData() {
            ParticipatingGuilds = new List<string>();
            Matches = new List<BracketMatch>();
            Results = new List<MatchResult>();
            CurrentRound = 0;
            TotalRounds = 0;
            ChampionGuildId = "";
            Statistics = new TournamentStatistics();
        }
    }
    
    /// <summary>
    /// 锦标赛阶段
    /// </summary>
    public enum TournamentPhase {
        Registration,    // 报名中
        Seeding,        // 抽签中
        InProgress,     // 进行中
        QuarterFinals,  // 四分之一决赛
        SemiFinals,     // 半决赛
        Finals,         // 决赛
        Completed,      // 已完成
        Cancelled       // 已取消
    }
    
    /// <summary>
    /// 对阵比赛
    /// </summary>
    [Serializable]
    public class BracketMatch {
        public string MatchId { get; set; }
        public int Round { get; set; }
        public int MatchNumber { get; set; }
        public string Guild1Id { get; set; }
        public string Guild2Id { get; set; }
        public string WinnerId { get; set; }
        public MatchStatus Status { get; set; }
        public int Guild1Score { get; set; }
        public int Guild2Score { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        
        public BracketMatch() {
            MatchId = Guid.NewGuid().ToString();
            Status = MatchStatus.Pending;
            Guild1Score = 0;
            Guild2Score = 0;
        }
    }
    
    /// <summary>
    /// 比赛状态
    /// </summary>
    public enum MatchStatus {
        Pending,    // 待开始
        Ready,      // 准备中
        InProgress, // 进行中
        Completed,  // 已完成
        Cancelled   // 已取消
    }
    
    /// <summary>
    /// 比赛结果
    /// </summary>
    [Serializable]
    public class MatchResult {
        public string MatchId { get; set; }
        public string WinnerId { get; set; }
        public string LoserId { get; set; }
        public int WinnerScore { get; set; }
        public int LoserScore { get; set; }
        public DateTime CompletedAt { get; set; }
    }
    
    /// <summary>
    /// 锦标赛统计
    /// </summary>
    [Serializable]
    public class TournamentStatistics {
        public int TotalMatches { get; set; }
        public int CompletedMatches { get; set; }
        public int TotalKills { get; set; }
        public int TotalDeaths { get; set; }
        public int TotalDamage { get; set; }
        public int TotalHealing { get; set; }
        public Dictionary<string, int> GuildWins { get; set; }
        public Dictionary<string, int> GuildLosses { get; set; }
        
        public TournamentStatistics() {
            GuildWins = new Dictionary<string, int>();
            GuildLosses = new Dictionary<string, int>();
        }
    }
}
