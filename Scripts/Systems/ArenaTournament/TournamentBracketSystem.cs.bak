using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 锦标赛赛程系统 - 负责比赛对阵生成和赛程管理
    /// </summary>
    public partial class TournamentBracketSystem : BaseSystem
    {
        private static TournamentBracketSystem _instance;
        public static TournamentBracketSystem Instance => _instance;

        // 事件：比赛生成
        public event Action<string, string> OnMatchGenerated;
        // 事件：阶段完成
        public event Action<string, TournamentStage> OnStageCompleted;
        // 事件：比赛完成
        public event Action<string, string> OnMatchCompleted;

        public override void _Ready()
        {
            _instance = this;
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            GD.Print("[TournamentBracketSystem] 赛程系统初始化");
            IsInitialized = true;
        }

        #region Tournament Execution

        /// <summary>
        /// 开始锦标赛 - 生成比赛对阵
        /// </summary>
        public bool StartTournament(string tournamentId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (!core.Tournaments.ContainsKey(tournamentId))
            {
                GD.PrintErr($"[TournamentBracketSystem] 锦标赛不存在: {tournamentId}");
                return false;
            }

            var tournament = core.Tournaments[tournamentId];
            
            if (tournament.currentPlayerCount < tournament.minPlayers)
            {
                GD.PrintErr($"[TournamentBracketSystem] 玩家不足，无法开始 (当前: {tournament.currentPlayerCount}, 最低: {tournament.minPlayers})");
                return false;
            }

            // 生成比赛对阵
            GenerateMatches(tournament);
            
            // 更新锦标赛状态
            tournament.status = TournamentStatus.Active;
            tournament.startTime = DateTime.Now;
            tournament.currentStage = DetermineStartingStage(tournament);
            tournament.currentRound = 1;
            tournament.updatedAt = DateTime.Now;

            core.ActiveTournaments.Add(tournament);
            
            GD.Print($"[TournamentBracketSystem] 锦标赛 {tournament.tournamentName} 开始!");
            
            foreach (var match in tournament.matches)
            {
                OnMatchGenerated?.Invoke(tournamentId, match.matchId);
            }
            
            return true;
        }

        /// <summary>
        /// 生成比赛对阵
        /// </summary>
        public void GenerateMatches(Tournament tournament)
        {
            tournament.matches.Clear();
            tournament.groups.Clear();
            
            switch (tournament.format)
            {
                case TournamentFormat.SingleElimination:
                    GenerateSingleEliminationMatches(tournament);
                    break;
                case TournamentFormat.DoubleElimination:
                    GenerateDoubleEliminationMatches(tournament);
                    break;
                case TournamentFormat.RoundRobin:
                    GenerateRoundRobinMatches(tournament);
                    break;
                case TournamentFormat.SwissSystem:
                    GenerateSwissMatches(tournament, 1);
                    break;
            }
        }

        #region Match Generation

        private void GenerateSingleEliminationMatches(Tournament tournament)
        {
            var players = tournament.registeredPlayers.OrderBy(p => p.seedNumber).ToList();
            int round = 1;
            int matchNum = 1;
            
            for (int i = 0; i < players.Count - 1; i += 2)
            {
                var match = new TournamentMatch
                {
                    matchId = $"{tournament.tournamentId}_R{round}_M{matchNum}",
                    roundNumber = round,
                    matchNumber = matchNum,
                    stage = GetStageForRound(tournament.format, round),
                    player1Id = players[i].playerId,
                    player2Id = players[i + 1].playerId,
                    scheduledTime = DateTime.Now.AddMinutes(matchNum * 5)
                };
                tournament.matches.Add(match);
                matchNum++;
            }

            if (players.Count % 2 == 1)
            {
                var lastPlayer = players[players.Count - 1];
                var match = new TournamentMatch
                {
                    matchId = $"{tournament.tournamentId}_R{round}_M{matchNum}",
                    roundNumber = round,
                    matchNumber = matchNum,
                    stage = GetStageForRound(tournament.format, round),
                    player1Id = lastPlayer.playerId,
                    player2Id = "",
                    winnerId = lastPlayer.playerId,
                    isCompleted = true,
                    scheduledTime = DateTime.Now
                };
                tournament.matches.Add(match);
            }
        }

        private void GenerateDoubleEliminationMatches(Tournament tournament)
        {
            GenerateSingleEliminationMatches(tournament);
            
            foreach (var match in tournament.matches)
            {
                match.stage = TournamentStage.GroupStage;
            }
        }

        private void GenerateRoundRobinMatches(Tournament tournament)
        {
            var players = tournament.registeredPlayers.OrderBy(p => p.seedNumber).ToList();
            int matchNum = 1;
            
            var group = new TournamentGroup
            {
                groupId = $"{tournament.tournamentId}_A",
                groupName = "A组"
            };
            
            for (int i = 0; i < players.Count; i++)
            {
                group.playerIds.Add(players[i].playerId);
                
                for (int j = i + 1; j < players.Count; j++)
                {
                    var match = new TournamentMatch
                    {
                        matchId = $"{tournament.tournamentId}_RR_{matchNum}",
                        roundNumber = j - i,
                        matchNumber = matchNum,
                        stage = TournamentStage.GroupStage,
                        player1Id = players[i].playerId,
                        player2Id = players[j].playerId,
                        scheduledTime = DateTime.Now.AddMinutes(matchNum * 5)
                    };
                    tournament.matches.Add(match);
                    group.matches.Add(match);
                    matchNum++;
                }
            }
            
            tournament.groups.Add(group);
        }

        private void GenerateSwissMatches(Tournament tournament, int round)
        {
            var players = tournament.registeredPlayers
                .Where(p => !p.isEliminated)
                .OrderByDescending(p => p.score)
                .ThenBy(p => p.seedNumber)
                .ToList();
            
            int matchNum = 1;
            
            for (int i = 0; i < players.Count - 1; i += 2)
            {
                var player1 = players[i];
                var player2 = players[i + 1];
                
                var match = new TournamentMatch
                {
                    matchId = $"{tournament.tournamentId}_S{round}_M{matchNum}",
                    roundNumber = round,
                    matchNumber = matchNum,
                    stage = TournamentStage.GroupStage,
                    player1Id = player1.playerId,
                    player2Id = player2.playerId,
                    scheduledTime = DateTime.Now.AddMinutes(matchNum * 5)
                };
                tournament.matches.Add(match);
                matchNum++;
            }
            
            if (players.Count % 2 == 1)
            {
                var lastPlayer = players[players.Count - 1];
                var match = new TournamentMatch
                {
                    matchId = $"{tournament.tournamentId}_S{round}_M{matchNum}",
                    roundNumber = round,
                    matchNumber = matchNum,
                    stage = TournamentStage.GroupStage,
                    player1Id = lastPlayer.playerId,
                    player2Id = "",
                    winnerId = lastPlayer.playerId,
                    isCompleted = true,
                    scheduledTime = DateTime.Now
                };
                tournament.matches.Add(match);
            }
        }

        private TournamentStage GetStageForRound(TournamentFormat format, int round)
        {
            return round switch
            {
                1 => TournamentStage.QuarterFinals,
                2 => TournamentStage.SemiFinals,
                3 => TournamentStage.Finals,
                _ => TournamentStage.GroupStage
            };
        }

        private TournamentStage DetermineStartingStage(Tournament tournament)
        {
            int playerCount = tournament.currentPlayerCount;
            
            if (playerCount >= 8)
                return TournamentStage.QuarterFinals;
            else if (playerCount >= 4)
                return TournamentStage.SemiFinals;
            else
                return TournamentStage.Finals;
        }

        #endregion

        #endregion

        #region Match Processing

        /// <summary>
        /// 报告比赛结果
        /// </summary>
        public bool ReportMatchResult(string matchId, string winnerId, int winnerScore, int loserScore)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            TournamentMatch match = null;
            Tournament tournament = null;
            
            foreach (var t in core.Tournaments.Values)
            {
                var m = t.matches.FirstOrDefault(x => x.matchId == matchId);
                if (m != null)
                {
                    match = m;
                    tournament = t;
                    break;
                }
            }
            
            if (match == null)
            {
                GD.PrintErr($"[TournamentBracketSystem] 比赛不存在: {matchId}");
                return false;
            }
            
            if (match.isCompleted)
            {
                GD.PrintErr("[TournamentBracketSystem] 比赛已完成");
                return false;
            }
            
            if (winnerId != match.player1Id && winnerId != match.player2Id)
            {
                GD.PrintErr("[TournamentBracketSystem] 无效的胜者");
                return false;
            }
            
            string loserId = winnerId == match.player1Id ? match.player2Id : match.player1Id;
            
            // 更新比赛结果
            match.winnerId = winnerId;
            match.isCompleted = true;
            match.completedTime = DateTime.Now;
            
            if (winnerId == match.player1Id)
            {
                match.player1Score = winnerScore;
                match.player2Score = loserScore;
            }
            else
            {
                match.player1Score = loserScore;
                match.player2Score = winnerScore;
            }
            
            // 更新玩家统计
            var winner = tournament.registeredPlayers.FirstOrDefault(p => p.playerId == winnerId);
            var loser = tournament.registeredPlayers.FirstOrDefault(p => p.playerId == loserId);
            
            if (winner != null)
            {
                winner.wins++;
                winner.matchesPlayed++;
                winner.score += 3;
                winner.matchHistory.Add(matchId);
            }
            
            if (loser != null)
            {
                loser.losses++;
                loser.matchesPlayed++;
                loser.matchHistory.Add(matchId);
                
                // 处理淘汰逻辑
                if (tournament.format == TournamentFormat.SingleElimination)
                {
                    loser.isEliminated = true;
                }
                else if (tournament.format == TournamentFormat.DoubleElimination)
                {
                    if (loser.hasLostOnce)
                    {
                        loser.isEliminated = true;
                    }
                    else
                    {
                        loser.hasLostOnce = true;
                    }
                }
            }
            
            GD.Print($"[TournamentBracketSystem] 比赛 {matchId} 完成，胜者: {winnerId}");
            OnMatchCompleted?.Invoke(tournament.tournamentId, matchId);
            
            // 检查阶段完成
            CheckStageCompletion(tournament, match.stage);
            
            return true;
        }

        private void CheckStageCompletion(Tournament tournament, TournamentStage stage)
        {
            var stageMatches = tournament.matches
                .Where(m => m.stage == stage && m.roundNumber == tournament.currentRound)
                .ToList();
            
            if (stageMatches.All(m => m.isCompleted))
            {
                if (stage == TournamentStage.Finals)
                {
                    // 锦标赛完成 - 通知奖励系统
                    var rewardSystem = TournamentRewardSystem.Instance;
                    rewardSystem?.DistributeRewards(tournament);
                    
                    OnStageCompleted?.Invoke(tournament.tournamentId, TournamentStage.Completed);
                }
                else if (tournament.format == TournamentFormat.SwissSystem && 
                         tournament.currentRound < tournament.rounds)
                {
                    GenerateSwissMatches(tournament, tournament.currentRound + 1);
                    tournament.currentRound++;
                }
                else
                {
                    AdvanceToNextStage(tournament);
                }
            }
        }

        private void AdvanceToNextStage(Tournament tournament)
        {
            var currentStage = tournament.currentStage;
            TournamentStage nextStage;
            
            switch (currentStage)
            {
                case TournamentStage.QuarterFinals:
                    nextStage = TournamentStage.SemiFinals;
                    break;
                case TournamentStage.SemiFinals:
                    nextStage = TournamentStage.Finals;
                    break;
                case TournamentStage.Finals:
                    // 通知奖励系统
                    var rewardSystem = TournamentRewardSystem.Instance;
                    rewardSystem?.DistributeRewards(tournament);
                    OnStageCompleted?.Invoke(tournament.tournamentId, TournamentStage.Completed);
                    return;
                default:
                    return;
            }
            
            tournament.currentStage = nextStage;
            tournament.currentRound++;
            
            GenerateNextStageMatches(tournament);
            
            GD.Print($"[TournamentBracketSystem] 锦标赛进入 {nextStage} 阶段");
            OnStageCompleted?.Invoke(tournament.tournamentId, nextStage);
        }

        private void GenerateNextStageMatches(Tournament tournament)
        {
            var prevStage = tournament.currentStage - 1;
            if (prevStage < TournamentStage.QuarterFinals)
                prevStage = TournamentStage.QuarterFinals;
                
            var winners = tournament.matches
                .Where(m => m.stage == prevStage && m.isCompleted)
                .Select(m => m.winnerId)
                .ToList();
            
            int matchNum = 1;
            for (int i = 0; i < winners.Count - 1; i += 2)
            {
                var match = new TournamentMatch
                {
                    matchId = $"{tournament.tournamentId}_S{(int)tournament.currentStage}_M{matchNum}",
                    roundNumber = tournament.currentRound,
                    matchNumber = matchNum,
                    stage = tournament.currentStage,
                    player1Id = winners[i],
                    player2Id = winners[i + 1],
                    scheduledTime = DateTime.Now.AddMinutes(matchNum * 10)
                };
                tournament.matches.Add(match);
                matchNum++;
                
                OnMatchGenerated?.Invoke(tournament.tournamentId, match.matchId);
            }
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 获取锦标赛当前轮次的所有比赛
        /// </summary>
        public List<TournamentMatch> GetCurrentRoundMatches(string tournamentId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (!core.Tournaments.TryGetValue(tournamentId, out var tournament))
                return new List<TournamentMatch>();
            
            return tournament.matches
                .Where(m => m.roundNumber == tournament.currentRound)
                .ToList();
        }

        /// <summary>
        /// 获取玩家下一场比赛
        /// </summary>
        public TournamentMatch GetPlayerNextMatch(string tournamentId, string playerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (!core.Tournaments.TryGetValue(tournamentId, out var tournament))
                return null;
            
            return tournament.matches
                .FirstOrDefault(m => !m.isCompleted && 
                    (m.player1Id == playerId || m.player2Id == playerId));
        }

        /// <summary>
        /// 获取比赛
        /// </summary>
        public TournamentMatch GetMatch(string matchId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            foreach (var t in core.Tournaments.Values)
            {
                var match = t.matches.FirstOrDefault(m => m.matchId == matchId);
                if (match != null)
                    return match;
            }
            
            return null;
        }

        /// <summary>
        /// 获取锦标赛当前排名
        /// </summary>
        public List<TournamentPlayer> GetRankings(string tournamentId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (!core.Tournaments.TryGetValue(tournamentId, out var tournament))
                return new List<TournamentPlayer>();
            
            return tournament.registeredPlayers
                .OrderByDescending(p => p.score)
                .ThenByDescending(p => p.wins)
                .ToList();
        }

        #endregion

        #region 存档支持

        public override Dictionary<string, object> ExportSaveData()
        {
            // 赛程数据由 TournamentPersistenceSystem 统一管理
            // 本系统为无状态协调器，无独立数据需要持久化
            return new Dictionary<string, object>();
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 无自有状态，无需导入
        }

        #endregion
    }
}
