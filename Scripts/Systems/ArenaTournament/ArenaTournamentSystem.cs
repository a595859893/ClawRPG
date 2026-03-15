using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 竞技场锦标赛系统 - 管理所有锦标赛活动
    /// </summary>
    public class ArenaTournamentSystem : BaseSystem
    {
        // 单例
        private static ArenaTournamentSystem _instance;
        public static ArenaTournamentSystem Instance => _instance;

        // 锦标赛存储
        private Dictionary<string, Tournament> _tournaments = new Dictionary<string, Tournament>();
        private List<Tournament> _activeTournaments = new List<Tournament>();
        
        // 玩家进度
        private Dictionary<string, TournamentProgress> _playerProgress = new Dictionary<string, TournamentProgress>();
        
        // 信号
        public signal tournament_created(Tournament tournament);
        public signal player_registered(string tournament_id, string player_id);
        public signal tournament_started(Tournament tournament);
        public signal match_started(TournamentMatch match);
        public signal match_completed(TournamentMatch match);
        public signal stage_completed(Tournament tournament, TournamentStage stage);
        public signal tournament_completed(Tournament tournament);

        public override void _Ready()
        {
            _instance = this;
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            GD.Print("[ArenaTournamentSystem] 锦标赛系统初始化");
            LoadData();
        }

        #region Tournament Management

        /// <summary>
        /// 从模板创建锦标赛
        /// </summary>
        public Tournament CreateTournamentFromTemplate(string templateId, string organizerId)
        {
            var template = ArenaTournamentDatabase.GetTemplate(templateId);
            if (template == null)
            {
                GD.PrintErr($"[ArenaTournamentSystem] 模板不存在: {templateId}");
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

            _tournaments[tournament.tournamentId] = tournament;
            tournament_created?.Emit(tournament);
            
            GD.Print($"[ArenaTournamentSystem] 创建锦标赛: {tournament.tournamentName} ({tournament.tournamentId})");
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
                registrationEnd = DateTime.Now.AddHours(2), // 默认2小时报名
                rounds = CalculateRounds(format, maxPlayers),
                currentRound = 0,
                prizePool = prizePool,
                entryFee = entryFee,
                organizerId = organizerId,
                createdAt = DateTime.Now,
                updatedAt = DateTime.Now,
                rewards = ArenaTournamentDatabase.GetRewardPool(maxPlayers)
            };

            _tournaments[tournament.tournamentId] = tournament;
            tournament_created?.Emit(tournament);
            
            GD.Print($"[ArenaTournamentSystem] 创建自定义锦标赛: {tournament.tournamentName}");
            return tournament;
        }

        /// <summary>
        /// 玩家报名锦标赛
        /// </summary>
        public bool RegisterPlayer(string tournamentId, string playerId, string playerName)
        {
            if (!_tournaments.ContainsKey(tournamentId))
            {
                GD.PrintErr($"[ArenaTournamentSystem] 锦标赛不存在: {tournamentId}");
                return false;
            }

            var tournament = _tournaments[tournamentId];
            
            if (tournament.status != TournamentStatus.Pending)
            {
                GD.PrintErr($"[ArenaTournamentSystem] 锦标赛无法报名: {tournament.status}");
                return false;
            }

            if (DateTime.Now > tournament.registrationEnd)
            {
                GD.PrintErr("[ArenaTournamentSystem] 报名已结束");
                return false;
            }

            if (tournament.currentPlayerCount >= tournament.maxPlayers)
            {
                GD.PrintErr("[ArenaTournamentSystem] 锦标赛已满");
                return false;
            }

            // 检查是否已报名
            if (tournament.registeredPlayers.Any(p => p.playerId == playerId))
            {
                GD.PrintErr("[ArenaTournamentSystem] 玩家已报名");
                return false;
            }

            // 创建玩家数据
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

            player_registered?.Emit(tournamentId, playerId);
            
            GD.Print($"[ArenaTournamentSystem] 玩家 {playerName} 报名锦标赛 {tournament.tournamentName}");
            return true;
        }

        /// <summary>
        /// 开始锦标赛
        /// </summary>
        public bool StartTournament(string tournamentId)
        {
            if (!_tournaments.ContainsKey(tournamentId))
            {
                GD.PrintErr($"[ArenaTournamentSystem] 锦标赛不存在: {tournamentId}");
                return false;
            }

            var tournament = _tournaments[tournamentId];
            
            if (tournament.currentPlayerCount < tournament.minPlayers)
            {
                GD.PrintErr($"[ArenaTournamentSystem] 玩家不足，无法开始 (当前: {tournament.currentPlayerCount}, 最低: {tournament.minPlayers})");
                return false;
            }

            // 根据赛制生成比赛
            GenerateMatches(tournament);
            
            tournament.status = TournamentStatus.Active;
            tournament.startTime = DateTime.Now;
            tournament.currentStage = TournamentStage.QuarterFinals; // 从淘汰赛开始
            tournament.currentRound = 1;
            tournament.updatedAt = DateTime.Now;

            _activeTournaments.Add(tournament);
            tournament_started?.Emit(tournament);
            
            GD.Print($"[ArenaTournamentSystem] 锦标赛 {tournament.tournamentName} 开始!");
            return true;
        }

        /// <summary>
        /// 生成比赛对阵
        /// </summary>
        private void GenerateMatches(Tournament tournament)
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
            
            // 洗牌或按种子生成对阵
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

            // 如果玩家数为奇数，有玩家轮空
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
                    player2Id = "", // 轮空
                    winnerId = lastPlayer.playerId,
                    isCompleted = true,
                    scheduledTime = DateTime.Now
                };
                tournament.matches.Add(match);
            }
        }

        private void GenerateDoubleEliminationMatches(Tournament tournament)
        {
            // 简化实现：先生成单败，然后在胜者组基础上生成败者组
            GenerateSingleEliminationMatches(tournament);
            
            // 标记为双败赛制
            foreach (var match in tournament.matches)
            {
                match.stage = TournamentStage.GroupStage; // 胜者组第一轮
            }
        }

        private void GenerateRoundRobinMatches(Tournament tournament)
        {
            var players = tournament.registeredPlayers.OrderBy(p => p.seedNumber).ToList();
            int matchNum = 1;
            
            // 创建小组
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
                // 避免重复对战
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
            
            // 轮空处理
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

        /// <summary>
        /// 报告比赛结果
        /// </summary>
        public bool ReportMatchResult(string matchId, string winnerId, int winnerScore, int loserScore)
        {
            TournamentMatch match = null;
            Tournament tournament = null;
            
            foreach (var t in _tournaments.Values)
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
                GD.PrintErr($"[ArenaTournamentSystem] 比赛不存在: {matchId}");
                return false;
            }
            
            if (match.isCompleted)
            {
                GD.PrintErr("[ArenaTournamentSystem] 比赛已完成");
                return false;
            }
            
            // 验证胜者是参赛者
            if (winnerId != match.player1Id && winnerId != match.player2Id)
            {
                GD.PrintErr("[ArenaTournamentSystem] 无效的胜者");
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
                winner.score += 3; // 胜3分
                winner.matchHistory.Add(matchId);
            }
            
            if (loser != null)
            {
                loser.losses++;
                loser.matchesPlayed++;
                loser.matchHistory.Add(matchId);
            }
            
            // 检查是否需要淘汰
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
            
            // 检查阶段完成
            CheckStageCompletion(tournament, match.stage);
            
            match_completed?.Emit(match);
            
            GD.Print($"[ArenaTournamentSystem] 比赛 {matchId} 完成，胜者: {winnerId}");
            return true;
        }

        private void CheckStageCompletion(Tournament tournament, TournamentStage stage)
        {
            var stageMatches = tournament.matches
                .Where(m => m.stage == stage && m.roundNumber == tournament.currentRound)
                .ToList();
            
            if (stageMatches.All(m => m.isCompleted))
            {
                stage_completed?.Emit(tournament, stage);
                
                // 检查是否需要进入下一阶段
                if (stage == TournamentStage.Finals)
                {
                    CompleteTournament(tournament);
                }
                else if (tournament.format == TournamentFormat.SwissSystem && 
                         tournament.currentRound < tournament.rounds)
                {
                    // 瑞士制进入下一轮
                    GenerateSwissMatches(tournament, tournament.currentRound + 1);
                    tournament.currentRound++;
                }
                else
                {
                    // 淘汰赛进入下一阶段
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
                    return; // 决赛不需要进入下一阶段
                default:
                    return;
            }
            
            tournament.currentStage = nextStage;
            tournament.currentRound++;
            
            // 生成下一阶段比赛
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

        /// <summary>
        /// 完成锦标赛
        /// </summary>
        private void CompleteTournament(Tournament tournament)
        {
            tournament.status = TournamentStatus.Completed;
            tournament.currentStage = TournamentStage.Completed;
            tournament.endTime = DateTime.Now;
            
            // 计算排名
            var rankings = tournament.registeredPlayers
                .OrderByDescending(p => p.score)
                .ThenByDescending(p => p.wins)
                .ToList();
            
            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].score = i + 1; // 临时用作排名
            }
            
            // 发放奖励
            DistributeRewards(tournament);
            
            // 更新玩家记录
            foreach (var player in tournament.registeredPlayers)
            {
                UpdatePlayerRecord(tournament, player);
            }
            
            _activeTournaments.Remove(tournament);
            tournament_completed?.Emit(tournament);
            
            GD.Print($"[ArenaTournamentSystem] 锦标赛 {tournament.tournamentName} 结束!");
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
                        // 发放奖励 (实际实现需要与经济系统集成)
                        GD.Print($"[ArenaTournamentSystem] 玩家 {player.playerName} 获得排名 {rank} 奖励: {reward.rewardType} x{reward.rewardAmount}");
                    }
                }
            }
        }

        private void UpdatePlayerRecord(Tournament tournament, TournamentPlayer player)
        {
            var rankings = tournament.registeredPlayers
                .OrderByDescending(p => p.score)
                .ThenByDescending(p => p.wins)
                .ToList();
            
            int rank = rankings.FindIndex(p => p.playerId == player.playerId) + 1;
            
            if (!_playerProgress.ContainsKey(player.playerId))
            {
                _playerProgress[player.playerId] = new TournamentProgress
                {
                    playerId = player.playerId,
                    statistics = new TournamentStatistics { playerId = player.playerId }
                };
            }
            
            var progress = _playerProgress[player.playerId];
            
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
            
            // 更新统计
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

        #endregion

        #region Queries

        /// <summary>
        /// 获取所有可报名的锦标赛
        /// </summary>
        public List<Tournament> GetAvailableTournaments()
        {
            return _tournaments.Values
                .Where(t => t.status == TournamentStatus.Pending && DateTime.Now <= t.registrationEnd)
                .OrderBy(t => t.registrationEnd)
                .ToList();
        }

        /// <summary>
        /// 获取进行中的锦标赛
        /// </summary>
        public List<Tournament> GetActiveTournaments()
        {
            return _activeTournaments.ToList();
        }

        /// <summary>
        /// 获取锦标赛详情
        /// </summary>
        public Tournament GetTournament(string tournamentId)
        {
            return _tournaments.ContainsKey(tournamentId) ? _tournaments[tournamentId] : null;
        }

        /// <summary>
        /// 获取玩家的下一场比赛
        /// </summary>
        public TournamentMatch GetPlayerNextMatch(string tournamentId, string playerId)
        {
            if (!_tournaments.ContainsKey(tournamentId))
                return null;
            
            var tournament = _tournaments[tournamentId];
            return tournament.matches
                .Where(m => !m.isCompleted && 
                           (m.player1Id == playerId || m.player2Id == playerId))
                .OrderBy(m => m.scheduledTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// 获取玩家进度
        /// </summary>
        public TournamentProgress GetPlayerProgress(string playerId)
        {
            return _playerProgress.ContainsKey(playerId) ? _playerProgress[playerId] : null;
        }

        /// <summary>
        /// 获取玩家统计
        /// </summary>
        public TournamentStatistics GetPlayerStatistics(string playerId)
        {
            var progress = GetPlayerProgress(playerId);
            return progress?.statistics;
        }

        /// <summary>
        /// 获取所有模板
        /// </summary>
        public List<TournamentTemplate> GetTemplates()
        {
            return ArenaTournamentDatabase.GetAllTemplates();
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
            // 实际实现需要从存档加载数据
            GD.Print("[ArenaTournamentSystem] 数据加载完成");
        }

        public void SaveData()
        {
            // 实际实现需要保存数据到存档
            GD.Print("[ArenaTournamentSystem] 数据保存完成");
        }

        #endregion

        #region BaseSystem 持久化接口

        public override Dictionary<string, object> ExportSaveData()
        {
            var tournamentsData = new List<Dictionary<string, object>>();
            foreach (var kvp in _tournaments)
            {
                tournamentsData.Add(new Dictionary<string, object>
                {
                    { "tournamentId", kvp.Value.tournamentId },
                    { "tournamentName", kvp.Value.tournamentName },
                    { "description", kvp.Value.description },
                    { "format", (int)kvp.Value.format },
                    { "status", (int)kvp.Value.status },
                    { "currentStage", (int)kvp.Value.currentStage },
                    { "maxPlayers", kvp.Value.maxPlayers },
                    { "minPlayers", kvp.Value.minPlayers },
                    { "currentPlayerCount", kvp.Value.currentPlayerCount },
                    { "currentRound", kvp.Value.currentRound },
                    { "prizePool", kvp.Value.prizePool },
                    { "entryFee", kvp.Value.entryFee }
                });
            }

            var progressData = new List<Dictionary<string, object>>();
            foreach (var kvp in _playerProgress)
            {
                progressData.Add(new Dictionary<string, object>
                {
                    { "playerId", kvp.Key },
                    { "tournamentsJoined", kvp.Value.tournamentsJoined },
                    { "wins", kvp.Value.wins },
                    { "losses", kvp.Value.losses },
                    { "totalPoints", kvp.Value.totalPoints }
                });
            }

            return new Dictionary<string, object>
            {
                { "tournaments", tournamentsData },
                { "playerProgress", progressData }
            };
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data.ContainsKey("tournaments"))
            {
                var tournamentsData = data["tournaments"] as List<Dictionary<string, object>>;
                foreach (var tData in tournamentsData)
                {
                    var tournament = new Tournament
                    {
                        tournamentId = tData["tournamentId"].ToString(),
                        tournamentName = tData["tournamentName"].ToString(),
                        description = tData["description"].ToString(),
                        format = (TournamentFormat)Convert.ToInt32(tData["format"]),
                        status = (TournamentStatus)Convert.ToInt32(tData["status"]),
                        currentStage = (TournamentStage)Convert.ToInt32(tData["currentStage"]),
                        maxPlayers = Convert.ToInt32(tData["maxPlayers"]),
                        minPlayers = Convert.ToInt32(tData["minPlayers"]),
                        currentPlayerCount = Convert.ToInt32(tData["currentPlayerCount"]),
                        currentRound = Convert.ToInt32(tData["currentRound"]),
                        prizePool = Convert.ToInt32(tData["prizePool"]),
                        entryFee = Convert.ToInt32(tData["entryFee"])
                    };
                    _tournaments[tournament.tournamentId] = tournament;
                }
            }

            if (data.ContainsKey("playerProgress"))
            {
                var progressData = data["playerProgress"] as List<Dictionary<string, object>>;
                foreach (var pData in progressData)
                {
                    var progress = new TournamentProgress
                    {
                        playerId = pData["playerId"].ToString(),
                        tournamentsJoined = Convert.ToInt32(pData["tournamentsJoined"]),
                        wins = Convert.ToInt32(pData["wins"]),
                        losses = Convert.ToInt32(pData["losses"]),
                        totalPoints = Convert.ToInt32(pData["totalPoints"])
                    };
                    _playerProgress[progress.playerId] = progress;
                }
            }

            GD.Print("[ArenaTournamentSystem] 数据已加载");
        }

        #endregion
    }
}
