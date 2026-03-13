using System;
using System.Collections.Generic;
using System.Linq;
using Godot;
using ClawRPG.Scripts.Systems.GuildTournamentBracket;

namespace ClawRPG.Scripts.Systems.GuildTournamentBracket {
    /// <summary>
    /// 公会锦标赛赛程系统
    /// </summary>
    public class GuildTournamentBracketSystem {
        // 单例
        private static GuildTournamentBracketSystem _instance;
        public static GuildTournamentBracketSystem Instance {
            get {
                if (_instance == null) {
                    _instance = new GuildTournamentBracketSystem();
                }
                return _instance;
            }
        }
        
        // 当前锦标赛数据
        private GuildTournamentBracketData _currentTournament;
        
        // 构造函数
        public GuildTournamentBracketSystem() {
            _instance = this;
            _currentTournament = new GuildTournamentBracketData();
        }
        
        /// <summary>
        /// 创建新锦标赛
        /// </summary>
        public void CreateTournament(string tournamentId, GuildTournamentBracketDatabase.TournamentFormat format) {
            _currentTournament = new GuildTournamentBracketData();
            _currentTournament.TournamentId = tournamentId;
            _currentTournament.CurrentPhase = TournamentPhase.Registration;
            _currentTournament.TotalRounds = 0;
            GD.Print($"[GuildTournamentBracket] Created new tournament: {tournamentId}");
        }
        
        /// <summary>
        /// 注册公会
        /// </summary>
        public bool RegisterGuild(string guildId, string guildName) {
            if (_currentTournament.CurrentPhase != TournamentPhase.Registration) {
                GD.Print($"[GuildTournamentBracket] Cannot register: tournament not in registration phase");
                return false;
            }
            
            if (_currentTournament.ParticipatingGuilds.Contains(guildId)) {
                GD.Print($"[GuildTournamentBracket] Guild {guildName} already registered");
                return false;
            }
            
            var config = GuildTournamentBracketDatabase.GetFormatConfig(GuildTournamentBracketDatabase.TournamentFormat.SingleElimination);
            if (_currentTournament.ParticipatingGuilds.Count >= config.MaxTeams) {
                GD.Print($"[GuildTournamentBracket] Tournament is full");
                return false;
            }
            
            _currentTournament.ParticipatingGuilds.Add(guildId);
            GD.Print($"[GuildTournamentBracket] Guild {guildName} registered successfully");
            return true;
        }
        
        /// <summary>
        /// 开始抽签
        /// </summary>
        public void StartSeeding() {
            if (_currentTournament.CurrentPhase != TournamentPhase.Registration) {
                GD.Print($"[GuildTournamentBracket] Cannot start seeding: not in registration phase");
                return;
            }
            
            if (_currentTournament.ParticipatingGuilds.Count < 4) {
                GD.Print($"[GuildTournamentBracket] Not enough guilds to start tournament");
                return;
            }
            
            _currentTournament.CurrentPhase = TournamentPhase.Seeding;
            GenerateBracket();
            GD.Print($"[GuildTournamentBracket] Seeding completed, generated {_currentTournament.Matches.Count} matches");
        }
        
        /// <summary>
        /// 生成对阵表
        /// </summary>
        private void GenerateBracket() {
            var guilds = _currentTournament.ParticipatingGuilds.ToList();
            ShuffleList(guilds);
            
            // 种子排位
            var seededGuilds = new List<string>();
            for (int i = 0; i < guilds.Count; i++) {
                seededGuilds.Add(guilds[i]);
            }
            
            // 计算轮次
            int teamCount = seededGuilds.Count;
            int rounds = (int)Math.Ceiling(Math.Log2(teamCount));
            _currentTournament.TotalRounds = rounds;
            _currentTournament.CurrentRound = 1;
            
            // 生成第一轮对阵
            int firstRoundMatches = teamCount / 2;
            for (int i = 0; i < firstRoundMatches; i++) {
                var match = new BracketMatch {
                    Round = 1,
                    MatchNumber = i + 1,
                    Guild1Id = seededGuilds[i * 2],
                    Guild2Id = seededGuilds[i * 2 + 1],
                    Status = MatchStatus.Pending
                };
                _currentTournament.Matches.Add(match);
            }
            
            // 生成后续轮次的空位
            int matchId = firstRoundMatches;
            for (int round = 2; round <= rounds; round++) {
                int matchesInRound = firstRoundMatches / (round - 1);
                for (int i = 0; i < matchesInRound; i++) {
                    var match = new BracketMatch {
                        Round = round,
                        MatchNumber = i + 1,
                        Guild1Id = "",
                        Guild2Id = "",
                        Status = MatchStatus.Pending
                    };
                    _currentTournament.Matches.Add(match);
                }
            }
            
            _currentTournament.CurrentPhase = TournamentPhase.InProgress;
        }
        
        /// <summary>
        /// 开始比赛
        /// </summary>
        public void StartMatch(string matchId) {
            var match = _currentTournament.Matches.FirstOrDefault(m => m.MatchId == matchId);
            if (match == null) {
                GD.Print($"[GuildTournamentBracket] Match not found: {matchId}");
                return;
            }
            
            if (string.IsNullOrEmpty(match.Guild1Id) || string.IsNullOrEmpty(match.Guild2Id)) {
                GD.Print($"[GuildTournamentBracket] Cannot start match: both guilds must be set");
                return;
            }
            
            match.Status = MatchStatus.InProgress;
            match.StartTime = DateTime.Now;
            GD.Print($"[GuildTournamentBracket] Match started: {match.Guild1Id} vs {match.Guild2Id}");
        }
        
        /// <summary>
        /// 结束比赛
        /// </summary>
        public void CompleteMatch(string matchId, string winnerId, int winnerScore, int loserScore) {
            var match = _currentTournament.Matches.FirstOrDefault(m => m.MatchId == matchId);
            if (match == null) {
                GD.Print($"[GuildTournamentBracket] Match not found: {matchId}");
                return;
            }
            
            match.Status = MatchStatus.Completed;
            match.WinnerId = winnerId;
            match.Guild1Score = winnerId == match.Guild1Id ? winnerScore : loserScore;
            match.Guild2Score = winnerId == match.Guild2Id ? winnerScore : loserScore;
            match.EndTime = DateTime.Now;
            
            // 记录结果
            var result = new MatchResult {
                MatchId = matchId,
                WinnerId = winnerId,
                LoserId = winnerId == match.Guild1Id ? match.Guild2Id : match.Guild1Id,
                WinnerScore = winnerScore,
                LoserScore = loserScore,
                CompletedAt = DateTime.Now
            };
            _currentTournament.Results.Add(result);
            
            // 更新统计
            if (!_currentTournament.Statistics.GuildWins.ContainsKey(winnerId)) {
                _currentTournament.Statistics.GuildWins[winnerId] = 0;
            }
            _currentTournament.Statistics.GuildWins[winnerId]++;
            
            var loserId = winnerId == match.Guild1Id ? match.Guild2Id : match.Guild1Id;
            if (!_currentTournament.Statistics.GuildLosses.ContainsKey(loserId)) {
                _currentTournament.Statistics.GuildLosses[loserId] = 0;
            }
            _currentTournament.Statistics.GuildLosses[loserId]++;
            
            _currentTournament.Statistics.CompletedMatches++;
            
            // 推进到下一轮
            AdvanceToNextRound(match);
            
            GD.Print($"[GuildTournamentBracket] Match completed: {winnerId} wins with score {winnerScore}-{loserScore}");
        }
        
        /// <summary>
        /// 推进到下一轮
        /// </summary>
        private void AdvanceToNextRound(BracketMatch completedMatch) {
            int currentRound = completedMatch.Round;
            
            // 检查是否还有未完成的当前轮次比赛
            var currentRoundMatches = _currentTournament.Matches
                .Where(m => m.Round == currentRound && m.Status != MatchStatus.Completed)
                .ToList();
            
            if (currentRoundMatches.Count > 0) {
                return; // 当前轮次还有比赛
            }
            
            // 准备下一轮
            if (currentRound >= _currentTournament.TotalRounds) {
                // 锦标赛结束
                _currentTournament.CurrentPhase = TournamentPhase.Completed;
                var finalWinner = _currentTournament.Matches
                    .FirstOrDefault(m => m.Round == currentRound);
                if (finalWinner != null) {
                    _currentTournament.ChampionGuildId = finalWinner.WinnerId;
                }
                GD.Print($"[GuildTournamentBracket] Tournament completed! Champion: {_currentTournament.ChampionGuildId}");
                return;
            }
            
            // 生成下一轮对阵
            int nextRound = currentRound + 1;
            var nextRoundMatches = _currentTournament.Matches
                .Where(m => m.Round == nextRound)
                .OrderBy(m => m.MatchNumber)
                .ToList();
            
            var currentRoundWinners = _currentTournament.Matches
                .Where(m => m.Round == currentRound && !string.IsNullOrEmpty(m.WinnerId))
                .OrderBy(m => m.MatchNumber)
                .ToList();
            
            // 为下一轮比赛分配获胜者
            for (int i = 0; i < nextRoundMatches.Count && i * 2 < currentRoundWinners.Count; i++) {
                nextRoundMatches[i].Guild1Id = currentRoundWinners[i * 2].WinnerId;
                nextRoundMatches[i].Guild2Id = currentRoundWinners[i * 2 + 1].WinnerId;
                nextRoundMatches[i].Status = MatchStatus.Ready;
            }
            
            _currentTournament.CurrentRound = nextRound;
            
            // 更新阶段
            switch (nextRound) {
                case 2:
                    _currentTournament.CurrentPhase = TournamentPhase.QuarterFinals;
                    break;
                case 3:
                    _currentTournament.CurrentPhase = TournamentPhase.SemiFinals;
                    break;
                case 4:
                    _currentTournament.CurrentPhase = TournamentPhase.Finals;
                    break;
            }
            
            GD.Print($"[GuildTournamentBracket] Advanced to round {nextRound}");
        }
        
        /// <summary>
        /// 获取当前锦标赛数据
        /// </summary>
        public GuildTournamentBracketData GetTournamentData() {
            return _currentTournament;
        }
        
        /// <summary>
        /// 获取当前轮次比赛
        /// </summary>
        public List<BracketMatch> GetCurrentRoundMatches() {
            return _currentTournament.Matches
                .Where(m => m.Round == _currentTournament.CurrentRound)
                .ToList();
        }
        
        /// <summary>
        /// 获取所有比赛
        /// </summary>
        public List<BracketMatch> GetAllMatches() {
            return _currentTournament.Matches;
        }
        
        /// <summary>
        /// 获取公会排名
        /// </summary>
        public List<GuildRanking> GetGuildRankings() {
            var rankings = new List<GuildRanking>();
            
            foreach (var guildId in _currentTournament.ParticipatingGuilds) {
                int wins = _currentTournament.Statistics.GuildWins.ContainsKey(guildId) 
                    ? _currentTournament.Statistics.GuildWins[guildId] 
                    : 0;
                int losses = _currentTournament.Statistics.GuildLosses.ContainsKey(guildId) 
                    ? _currentTournament.Statistics.GuildLosses[guildId] 
                    : 0;
                
                rankings.Add(new GuildRanking {
                    GuildId = guildId,
                    Wins = wins,
                    Losses = losses,
                    Points = wins * 3
                });
            }
            
            return rankings.OrderByDescending(r => r.Points).ThenByDescending(r => r.Wins).ToList();
        }
        
        /// <summary>
        /// 随机打乱列表
        /// </summary>
        private void ShuffleList<T>(List<T> list) {
            var random = new Random();
            for (int i = list.Count - 1; i > 0; i--) {
                int j = random.Next(i + 1);
                var temp = list[i];
                list[i] = list[j];
                list[j] = temp;
            }
        }
        
        /// <summary>
        /// 获取统计信息
        /// </summary>
        public Dictionary<string, object> GetStatistics() {
            return new Dictionary<string, object> {
                { "TotalMatches", _currentTournament.Statistics.TotalMatches },
                { "CompletedMatches", _currentTournament.Statistics.CompletedMatches },
                { "CurrentRound", _currentTournament.CurrentRound },
                { "TotalRounds", _currentTournament.TotalRounds },
                { "ParticipatingGuilds", _currentTournament.ParticipatingGuilds.Count },
                { "Champion", _currentTournament.ChampionGuildId }
            };
        }
        
        /// <summary>
        /// 重置锦标赛
        /// </summary>
        public void ResetTournament() {
            _currentTournament = new GuildTournamentBracketData();
            GD.Print("[GuildTournamentBracket] Tournament reset");
        }
    }
    
    /// <summary>
    /// 公会排名
    /// </summary>
    public class GuildRanking {
        public string GuildId { get; set; }
        public int Wins { get; set; }
        public int Losses { get; set; }
        public int Points { get; set; }
    }
}
