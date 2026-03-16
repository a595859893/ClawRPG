using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 锦标赛核心系统 - 管理锦标赛的核心逻辑
    /// </summary>
    public class ArenaTournamentCoreSystem : BaseSystem
    {
        // 单例
        private static ArenaTournamentCoreSystem _instance;
        public static ArenaTournamentCoreSystem Instance => _instance;

        // 锦标赛存储
        public Dictionary<string, Tournament> Tournaments { get; } = new Dictionary<string, Tournament>();
        public List<Tournament> ActiveTournaments { get; } = new List<Tournament>();
        
        // 玩家进度
        public Dictionary<string, TournamentProgress> PlayerProgress { get; } = new Dictionary<string, TournamentProgress>();

        public override void _Ready()
        {
            _instance = this;
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            GD.Print("[ArenaTournamentCoreSystem] 锦标赛核心系统初始化");
            LoadData();
        }

        #region Tournament Creation

        /// <summary>
        /// 从模板创建锦标赛
        /// </summary>
        public Tournament CreateTournamentFromTemplate(string templateId, string organizerId)
        {
            var template = ArenaTournamentDatabase.GetTemplate(templateId);
            if (template == null)
            {
                GD.PrintErr($"[ArenaTournamentCoreSystem] 模板不存在: {templateId}");
                return null;
            }

            var tournament = new Tournament
            {
                tournamentId = GenerateTournamentId(),
                tournamentName = template.name,
                description = template.description,
                format = template.format,
                status = TournamentStatus.Pending,
                currentStage = TournamentStage.Registration,
                maxPlayers = template.maxPlayers,
                minPlayers = template.minPlayers,
                currentPlayerCount = 0,
                registrationStart = DateTime.Now,
                registrationEnd = DateTime.Now.AddSeconds(template.registrationDuration),
                rounds = template.rounds,
                currentRound = 0,
                prizePool = template.prizePool,
                entryFee = template.entryFee,
                organizerId = organizerId,
                createdAt = DateTime.Now,
                updatedAt = DateTime.Now,
                rewards = ArenaTournamentDatabase.GetRewardPool(template.maxPlayers)
            };

            Tournaments[tournament.tournamentId] = tournament;
            
            GD.Print($"[ArenaTournamentCoreSystem] 创建锦标赛: {tournament.tournamentName} ({tournament.tournamentId})");
            return tournament;
        }

        /// <summary>
        /// 创建自定义锦标赛
        /// </summary>
        public Tournament CreateCustomTournament(string name, string description, TournamentFormat format, 
            int maxPlayers, int minPlayers, int prizePool, int entryFee, string organizerId)
        {
            var tournament = new Tournament
            {
                tournamentId = GenerateTournamentId(),
                tournamentName = name,
                description = description,
                format = format,
                status = TournamentStatus.Pending,
                currentStage = TournamentStage.Registration,
                maxPlayers = maxPlayers,
                minPlayers = minPlayers,
                currentPlayerCount = 0,
                registrationStart = DateTime.Now,
                registrationEnd = DateTime.Now.AddHours(2),
                rounds = CalculateRounds(format, maxPlayers),
                currentRound = 0,
                prizePool = prizePool,
                entryFee = entryFee,
                organizerId = organizerId,
                createdAt = DateTime.Now,
                updatedAt = DateTime.Now,
                rewards = ArenaTournamentDatabase.GetRewardPool(maxPlayers)
            };

            Tournaments[tournament.tournamentId] = tournament;
            
            GD.Print($"[ArenaTournamentCoreSystem] 创建自定义锦标赛: {tournament.tournamentName}");
            return tournament;
        }

        #endregion

        #region Player Registration

        /// <summary>
        /// 玩家报名锦标赛
        /// </summary>
        public bool RegisterPlayer(string tournamentId, string playerId, string playerName)
        {
            if (!Tournaments.ContainsKey(tournamentId))
            {
                GD.PrintErr($"[ArenaTournamentCoreSystem] 锦标赛不存在: {tournamentId}");
                return false;
            }

            var tournament = Tournaments[tournamentId];
            
            if (tournament.status != TournamentStatus.Pending)
            {
                GD.PrintErr($"[ArenaTournamentCoreSystem] 锦标赛无法报名: {tournament.status}");
                return false;
            }

            if (DateTime.Now > tournament.registrationEnd)
            {
                GD.PrintErr("[ArenaTournamentCoreSystem] 报名已结束");
                return false;
            }

            if (tournament.currentPlayerCount >= tournament.maxPlayers)
            {
                GD.PrintErr("[ArenaTournamentCoreSystem] 锦标赛已满");
                return false;
            }

            if (tournament.registeredPlayers.Any(p => p.playerId == playerId))
            {
                GD.PrintErr("[ArenaTournamentCoreSystem] 玩家已报名");
                return false;
            }

            var player = new TournamentPlayer
            {
                playerId = playerId,
                playerName = playerName,
                seedNumber = tournament.currentPlayerCount + 1,
                registrationTime = DateTime.Now
            };

            tournament.registeredPlayers.Add(player);
            tournament.currentPlayerCount++;
            tournament.updatedAt = DateTime.Now;
            
            GD.Print($"[ArenaTournamentCoreSystem] 玩家 {playerName} 报名锦标赛 {tournament.tournamentName}");
            return true;
        }

        #endregion

        #region Tournament Execution

        /// <summary>
        /// 开始锦标赛
        /// </summary>
        public bool StartTournament(string tournamentId)
        {
            if (!Tournaments.ContainsKey(tournamentId))
            {
                GD.PrintErr($"[ArenaTournamentCoreSystem] 锦标赛不存在: {tournamentId}");
                return false;
            }

            var tournament = Tournaments[tournamentId];
            
            if (tournament.currentPlayerCount < tournament.minPlayers)
            {
                GD.PrintErr($"[ArenaTournamentCoreSystem] 玩家不足，无法开始 (当前: {tournament.currentPlayerCount}, 最低: {tournament.minPlayers})");
                return false;
            }

            GenerateMatches(tournament);
            
            tournament.status = TournamentStatus.Active;
            tournament.startTime = DateTime.Now;
            tournament.currentStage = TournamentStage.QuarterFinals;
            tournament.currentRound = 1;
            tournament.updatedAt = DateTime.Now;

            ActiveTournaments.Add(tournament);
            
            GD.Print($"[ArenaTournamentCoreSystem] 锦标赛 {tournament.tournamentName} 开始!");
            return true;
        }

        /// <summary>
        /// 生成比赛对阵
        /// </summary>
        public void GenerateMatches(Tournament tournament)
        {
            tournament.matches.Clear();
            
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

        #endregion

        #region Match Processing

        /// <summary>
        /// 报告比赛结果
        /// </summary>
        public bool ReportMatchResult(string matchId, string winnerId, int winnerScore, int loserScore)
        {
            TournamentMatch match = null;
            Tournament tournament = null;
            
            foreach (var t in Tournaments.Values)
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
                GD.PrintErr($"[ArenaTournamentCoreSystem] 比赛不存在: {matchId}");
                return false;
            }
            
            if (match.isCompleted)
            {
                GD.PrintErr("[ArenaTournamentCoreSystem] 比赛已完成");
                return false;
            }
            
            if (winnerId != match.player1Id && winnerId != match.player2Id)
            {
                GD.PrintErr("[ArenaTournamentCoreSystem] 无效的胜者");
                return false;
            }
            
            string loserId = winnerId == match.player1Id ? match.player2Id : match.player1Id;
            
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
            }
            
            if (tournament.format == TournamentFormat.SingleElimination && loser != null)
            {
                loser.isEliminated = true;
            }
            else if (tournament.format == TournamentFormat.DoubleElimination && loser != null)
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
            
            CheckStageCompletion(tournament, match.stage);
            
            GD.Print($"[ArenaTournamentCoreSystem] 比赛 {matchId} 完成，胜者: {winnerId}");
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
                    CompleteTournament(tournament);
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
                    return;
                default:
                    return;
            }
            
            tournament.currentStage = nextStage;
            tournament.currentRound++;
            
            GenerateNextStageMatches(tournament);
        }

        private void GenerateNextStageMatches(Tournament tournament)
        {
            var winners = tournament.matches
                .Where(m => m.stage == tournament.currentStage - 1 && m.isCompleted)
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
            }
        }

        #endregion

        #region Tournament Completion

        /// <summary>
        /// 完成锦标赛
        /// </summary>
        public void CompleteTournament(Tournament tournament)
        {
            tournament.status = TournamentStatus.Completed;
            tournament.currentStage = TournamentStage.Completed;
            tournament.endTime = DateTime.Now;
            
            var rankings = tournament.registeredPlayers
                .OrderByDescending(p => p.score)
                .ThenByDescending(p => p.wins)
                .ToList();
            
            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].score = i + 1;
            }
            
            DistributeRewards(tournament);
            UpdatePlayerRecords(tournament);
            
            ActiveTournaments.Remove(tournament);
            
            GD.Print($"[ArenaTournamentCoreSystem] 锦标赛 {tournament.tournamentName} 结束!");
        }

        private void DistributeRewards(Tournament tournament)
        {
            var rankings = tournament.registeredPlayers
                .OrderByDescending(p => p.score)
                .ThenByDescending(p => p.wins)
                .ToList();
            
            for (int i = 0; i < rankings.Count; i++)
            {
                int rank = i + 1;
                var player = rankings[i];
                
                foreach (var reward in tournament.rewards)
                {
                    if (rank >= reward.rankStart && rank <= reward.rankEnd)
                    {
                        GD.Print($"[ArenaTournamentCoreSystem] 玩家 {player.playerName} 获得排名 {rank} 奖励: {reward.rewardType} x{reward.rewardAmount}");
                    }
                }
            }
        }

        private void UpdatePlayerRecords(Tournament tournament)
        {
            var rankings = tournament.registeredPlayers
                .OrderByDescending(p => p.score)
                .ThenByDescending(p => p.wins)
                .ToList();
            
            foreach (var player in tournament.registeredPlayers)
            {
                int rank = rankings.FindIndex(p => p.playerId == player.playerId) + 1;
                
                if (!PlayerProgress.ContainsKey(player.playerId))
                {
                    PlayerProgress[player.playerId] = new TournamentProgress
                    {
                        playerId = player.playerId,
                        statistics = new TournamentStatistics { playerId = player.playerId }
                    };
                }
                
                var progress = PlayerProgress[player.playerId];
                
                progress.participatedTournaments.Add(tournament.tournamentId);
                
                var record = new PlayerTournamentRecord
                {
                    playerId = player.playerId,
                    tournamentId = tournament.tournamentId,
                    tournamentName = tournament.tournamentName,
                    finalRank = rank,
                    score = player.score,
                    wins = player.wins,
                    losses = player.losses,
                    participatedAt = DateTime.Now
                };
                
                progress.recentRecords.Insert(0, record);
                if (progress.recentRecords.Count > 10)
                {
                    progress.recentRecords.RemoveAt(progress.recentRecords.Count - 1);
                }
                
                var stats = progress.statistics;
                stats.totalTournaments++;
                stats.totalWins += player.wins;
                stats.totalLosses += player.losses;
                
                if (rank == 1) stats.firstPlace++;
                else if (rank == 2) stats.secondPlace++;
                else if (rank == 3) stats.thirdPlace++;
                else if (rank <= 4) stats.top4++;
                else if (rank <= 8) stats.top8++;
                else if (rank <= 16) stats.top16++;
                
                if (stats.highestRank == 0 || rank < stats.highestRank)
                {
                    stats.highestRank = rank;
                }
                
                stats.totalPrizeWon += tournament.prizePool / tournament.currentPlayerCount;
            }
        }

        #endregion

        #region Helpers

        private string GenerateTournamentId()
        {
            return $"T_{DateTime.Now:yyyyMMddHHmmss}_{GD.Randomi(1000, 9999)}";
        }

        private int CalculateRounds(TournamentFormat format, int playerCount)
        {
            var config = ArenaTournamentDatabase.GetFormatConfig(format);
            if (config != null)
            {
                return (int)Math.Ceiling(Math.Log(playerCount, 2));
            }
            return 4;
        }

        private void LoadData()
        {
            GD.Print("[ArenaTournamentCoreSystem] 数据加载完成");
        }

        public void SaveData()
        {
            GD.Print("[ArenaTournamentCoreSystem] 数据保存完成");
        }

        #endregion
    }
}
