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

        /// <summary>
        /// 导出持久化数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 序列化 Tournaments
            var tournamentsData = new List<Dictionary<string, object>>();
            foreach (var kvp in Tournaments)
            {
                var t = kvp.Value;
                tournamentsData.Add(new Dictionary<string, object>
                {
                    ["tournamentId"] = t.tournamentId,
                    ["tournamentName"] = t.tournamentName,
                    ["description"] = t.description,
                    ["format"] = (int)t.format,
                    ["status"] = (int)t.status,
                    ["currentStage"] = (int)t.currentStage,
                    ["maxPlayers"] = t.maxPlayers,
                    ["minPlayers"] = t.minPlayers,
                    ["currentPlayerCount"] = t.currentPlayerCount,
                    ["registrationStart"] = t.registrationStart.ToString("o"),
                    ["registrationEnd"] = t.registrationEnd.ToString("o"),
                    ["startTime"] = t.startTime?.ToString("o") ?? "",
                    ["endTime"] = t.endTime?.ToString("o") ?? "",
                    ["rounds"] = t.rounds,
                    ["currentRound"] = t.currentRound,
                    ["prizePool"] = t.prizePool,
                    ["organizerId"] = t.organizerId,
                    ["createdAt"] = t.createdAt.ToString("o"),
                    ["updatedAt"] = t.updatedAt.ToString("o")
                });
            }
            data["Tournaments"] = tournamentsData;
            
            // 序列化 ActiveTournaments (通过 ID 引用)
            var activeIds = new List<string>();
            foreach (var t in ActiveTournaments)
            {
                activeIds.Add(t.tournamentId);
            }
            data["ActiveTournamentIds"] = activeIds;
            
            // 序列化 PlayerProgress
            var progressData = new Dictionary<string, Dictionary<string, object>>();
            foreach (var kvp in PlayerProgress)
            {
                var p = kvp.Value;
                progressData[kvp.Key] = new Dictionary<string, object>
                {
                    ["playerId"] = p.playerId,
                    ["participatedTournaments"] = p.participatedTournaments,
                    ["highestRank"] = p.statistics?.highestRank ?? 0,
                    ["totalWins"] = p.statistics?.totalWins ?? 0,
                    ["totalLosses"] = p.statistics?.totalLosses ?? 0
                };
            }
            data["PlayerProgress"] = progressData;
            
            GD.Print($"[ArenaTournamentCoreSystem] 导出 {Tournaments.Count} 个锦标赛数据");
            return data;
        }

        /// <summary>
        /// 导入持久化数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null)
            {
                GD.Print("[ArenaTournamentCoreSystem] 无数据可导入");
                return;
            }
            
            // 导入 Tournaments
            if (data.ContainsKey("Tournaments"))
            {
                var tournamentsData = data["Tournaments"] as List<object>;
                if (tournamentsData != null)
                {
                    foreach (var tObj in tournamentsData)
                    {
                        var tDict = tObj as Dictionary<string, object>;
                        if (tDict == null) continue;
                        
                        var tournament = new Tournament
                        {
                            tournamentId = tDict["tournamentId"] as string ?? "",
                            tournamentName = tDict["tournamentName"] as string ?? "",
                            description = tDict["description"] as string ?? "",
                            format = (TournamentFormat)(int)(tDict["format"] ?? 0),
                            status = (TournamentStatus)(int)(tDict["status"] ?? 0),
                            currentStage = (TournamentStage)(int)(tDict["currentStage"] ?? 0),
                            maxPlayers = (int)(tDict["maxPlayers"] ?? 0),
                            minPlayers = (int)(tDict["minPlayers"] ?? 0),
                            currentPlayerCount = (int)(tDict["currentPlayerCount"] ?? 0),
                            registrationStart = DateTime.Parse(tDict["registrationStart"] as string ?? "2024-01-01"),
                            registrationEnd = DateTime.Parse(tDict["registrationEnd"] as string ?? "2024-01-01"),
                            rounds = (int)(tDict["rounds"] ?? 0),
                            currentRound = (int)(tDict["currentRound"] ?? 0),
                            prizePool = (int)(tDict["prizePool"] ?? 0),
                            organizerId = tDict["organizerId"] as string ?? "",
                            createdAt = DateTime.Parse(tDict["createdAt"] as string ?? "2024-01-01"),
                            updatedAt = DateTime.Parse(tDict["updatedAt"] as string ?? "2024-01-01")
                        };
                        
                        var startTimeStr = tDict["startTime"] as string;
                        if (!string.IsNullOrEmpty(startTimeStr))
                            tournament.startTime = DateTime.Parse(startTimeStr);
                            
                        var endTimeStr = tDict["endTime"] as string;
                        if (!string.IsNullOrEmpty(endTimeStr))
                            tournament.endTime = DateTime.Parse(endTimeStr);
                        
                        Tournaments[tournament.tournamentId] = tournament;
                    }
                }
            }
            
            // 导入 ActiveTournaments
            ActiveTournaments.Clear();
            if (data.ContainsKey("ActiveTournamentIds"))
            {
                var activeIds = data["ActiveTournamentIds"] as List<object>;
                if (activeIds != null)
                {
                    foreach (var id in activeIds)
                    {
                        if (Tournaments.ContainsKey(id as string ?? ""))
                        {
                            ActiveTournaments.Add(Tournaments[id as string ?? ""]);
                        }
                    }
                }
            }
            
            // 导入 PlayerProgress
            if (data.ContainsKey("PlayerProgress"))
            {
                var progressData = data["PlayerProgress"] as Dictionary<string, object>;
                if (progressData != null)
                {
                    foreach (var kvp in progressData)
                    {
                        var pDict = kvp.Value as Dictionary<string, object>;
                        if (pDict == null) continue;
                        
                        var progress = new TournamentProgress
                        {
                            playerId = pDict["playerId"] as string ?? ""
                        };
                        
                        var participated = pDict["participatedTournaments"] as List<object>;
                        if (participated != null)
                        {
                            foreach (var t in participated)
                            {
                                progress.participatedTournaments.Add(t as string ?? "");
                            }
                        }
                        
                        progress.statistics = new TournamentStatistics
                        {
                            playerId = progress.playerId,
                            highestRank = (int)(pDict["highestRank"] ?? 0),
                            totalWins = (int)(pDict["totalWins"] ?? 0),
                            totalLosses = (int)(pDict["totalLosses"] ?? 0)
                        };
                        
                        PlayerProgress[kvp.Key] = progress;
                    }
                }
            }
            
            GD.Print($"[ArenaTournamentCoreSystem] 导入 {Tournaments.Count} 个锦标赛, {PlayerProgress.Count} 个玩家进度");
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

        #region 数据持久化

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();

            // 序列化所有锦标赛
            var tournamentsData = new ArrayList();
            foreach (var kvp in Tournaments)
            {
                var t = kvp.Value;
                var tournamentDict = new Dictionary
                {
                    { "tournamentId", t.tournamentId },
                    { "tournamentName", t.tournamentName },
                    { "description", t.description },
                    { "format", (int)t.format },
                    { "status", (int)t.status },
                    { "currentStage", (int)t.currentStage },
                    { "maxPlayers", t.maxPlayers },
                    { "minPlayers", t.minPlayers },
                    { "currentPlayerCount", t.currentPlayerCount },
                    { "registrationStart", t.registrationStart.ToString("o") },
                    { "registrationEnd", t.registrationEnd.ToString("o") },
                    { "startTime", t.startTime?.ToString("o") },
                    { "endTime", t.endTime?.ToString("o") },
                    { "rounds", t.rounds },
                    { "currentRound", t.currentRound },
                    { "prizePool", t.prizePool },
                    { "entryFee", t.entryFee },
                    { "organizerId", t.organizerId },
                    { "createdAt", t.createdAt.ToString("o") },
                    { "updatedAt", t.updatedAt.ToString("o") }
                };

                // 序列化玩家
                var playersData = new ArrayList();
                foreach (var p in t.registeredPlayers)
                {
                    playersData.Add(new Dictionary
                    {
                        { "playerId", p.playerId },
                        { "playerName", p.playerName },
                        { "seedNumber", p.seedNumber },
                        { "score", p.score },
                        { "wins", p.wins },
                        { "losses", p.losses },
                        { "matchesPlayed", p.matchesPlayed },
                        { "isEliminated", p.isEliminated },
                        { "hasLostOnce", p.hasLostOnce },
                        { "registrationTime", p.registrationTime.ToString("o") },
                        { "matchHistory", new ArrayList(p.matchHistory) }
                    });
                }
                tournamentDict["registeredPlayers"] = playersData;

                // 序列化比赛
                var matchesData = new ArrayList();
                foreach (var m in t.matches)
                {
                    matchesData.Add(new Dictionary
                    {
                        { "matchId", m.matchId },
                        { "roundNumber", m.roundNumber },
                        { "matchNumber", m.matchNumber },
                        { "stage", (int)m.stage },
                        { "player1Id", m.player1Id },
                        { "player2Id", m.player2Id },
                        { "winnerId", m.winnerId },
                        { "player1Score", m.player1Score },
                        { "player2Score", m.player2Score },
                        { "isCompleted", m.isCompleted },
                        { "scheduledTime", m.scheduledTime.ToString("o") },
                        { "completedTime", m.completedTime?.ToString("o") }
                    });
                }
                tournamentDict["matches"] = matchesData;

                // 序列化奖励
                var rewardsData = new ArrayList();
                foreach (var r in t.rewards)
                {
                    rewardsData.Add(new Dictionary
                    {
                        { "rankStart", r.rankStart },
                        { "rankEnd", r.rankEnd },
                        { "rewardType", r.rewardType },
                        { "rewardId", r.rewardId },
                        { "rewardAmount", r.rewardAmount }
                    });
                }
                tournamentDict["rewards"] = rewardsData;

                tournamentsData.Add(tournamentDict);
            }
            data["tournaments"] = tournamentsData;

            // 序列化活动锦标赛索引
            var activeTournamentIds = new ArrayList();
            foreach (var t in ActiveTournaments)
            {
                activeTournamentIds.Add(t.tournamentId);
            }
            data["activeTournamentIds"] = activeTournamentIds;

            // 序列化玩家进度
            var progressData = new ArrayList();
            foreach (var kvp in PlayerProgress)
            {
                var p = kvp.Value;
                var progressDict = new Dictionary
                {
                    { "playerId", p.playerId },
                    { "participatedTournaments", new ArrayList(p.participatedTournaments) }
                };

                // 序列化最近记录
                var recordsData = new ArrayList();
                foreach (var r in p.recentRecords)
                {
                    recordsData.Add(new Dictionary
                    {
                        { "playerId", r.playerId },
                        { "tournamentId", r.tournamentId },
                        { "tournamentName", r.tournamentName },
                        { "finalRank", r.finalRank },
                        { "score", r.score },
                        { "wins", r.wins },
                        { "losses", r.losses },
                        { "participatedAt", r.participatedAt.ToString("o") }
                    });
                }
                progressDict["recentRecords"] = recordsData;

                // 序列化统计
                if (p.statistics != null)
                {
                    progressDict["statistics"] = new Dictionary
                    {
                        { "playerId", p.statistics.playerId },
                        { "totalTournaments", p.statistics.totalTournaments },
                        { "firstPlace", p.statistics.firstPlace },
                        { "secondPlace", p.statistics.secondPlace },
                        { "thirdPlace", p.statistics.thirdPlace },
                        { "top4", p.statistics.top4 },
                        { "top8", p.statistics.top8 },
                        { "top16", p.statistics.top16 },
                        { "totalWins", p.statistics.totalWins },
                        { "totalLosses", p.statistics.totalLosses },
                        { "highestRank", p.statistics.highestRank },
                        { "totalPrizeWon", p.statistics.totalPrizeWon }
                    };
                }

                progressData.Add(progressDict);
            }
            data["playerProgress"] = progressData;

            GD.Print($"[ArenaTournamentCoreSystem] 导出 {Tournaments.Count} 个锦标赛, {PlayerProgress.Count} 个玩家进度");
            return data;
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            // 导入锦标赛
            if (data.Contains("tournaments"))
            {
                var tournamentsData = (ArrayList)data["tournaments"];
                foreach (Dictionary td in tournamentsData)
                {
                    var tournament = new Tournament
                    {
                        tournamentId = td["tournamentId"]?.ToString() ?? "",
                        tournamentName = td["tournamentName"]?.ToString() ?? "",
                        description = td["description"]?.ToString() ?? "",
                        format = (TournamentFormat)(td["format"] as int? ?? 0),
                        status = (TournamentStatus)(td["status"] as int? ?? 0),
                        currentStage = (TournamentStage)(td["currentStage"] as int? ?? 0),
                        maxPlayers = td["maxPlayers"] as int? ?? 0,
                        minPlayers = td["minPlayers"] as int? ?? 0,
                        currentPlayerCount = td["currentPlayerCount"] as int? ?? 0,
                        registrationStart = DateTime.Parse(td["registrationStart"]?.ToString() ?? DateTime.Now.ToString("o")),
                        registrationEnd = DateTime.Parse(td["registrationEnd"]?.ToString() ?? DateTime.Now.AddHours(2).ToString("o")),
                        rounds = td["rounds"] as int? ?? 0,
                        currentRound = td["currentRound"] as int? ?? 0,
                        prizePool = td["prizePool"] as int? ?? 0,
                        entryFee = td["entryFee"] as int? ?? 0,
                        organizerId = td["organizerId"]?.ToString() ?? "",
                        createdAt = DateTime.Parse(td["createdAt"]?.ToString() ?? DateTime.Now.ToString("o")),
                        updatedAt = DateTime.Parse(td["updatedAt"]?.ToString() ?? DateTime.Now.ToString("o"))
                    };

                    if (td["startTime"] != null && !string.IsNullOrEmpty(td["startTime"]?.ToString()))
                        tournament.startTime = DateTime.Parse(td["startTime"]?.ToString());
                    if (td["endTime"] != null && !string.IsNullOrEmpty(td["endTime"]?.ToString()))
                        tournament.endTime = DateTime.Parse(td["endTime"]?.ToString());

                    // 导入玩家
                    if (td.Contains("registeredPlayers"))
                    {
                        foreach (Dictionary pd in (ArrayList)td["registeredPlayers"])
                        {
                            tournament.registeredPlayers.Add(new TournamentPlayer
                            {
                                playerId = pd["playerId"]?.ToString() ?? "",
                                playerName = pd["playerName"]?.ToString() ?? "",
                                seedNumber = pd["seedNumber"] as int? ?? 0,
                                score = pd["score"] as int? ?? 0,
                                wins = pd["wins"] as int? ?? 0,
                                losses = pd["losses"] as int? ?? 0,
                                matchesPlayed = pd["matchesPlayed"] as int? ?? 0,
                                isEliminated = pd["isEliminated"] as bool? ?? false,
                                hasLostOnce = pd["hasLostOnce"] as bool? ?? false,
                                registrationTime = DateTime.Parse(pd["registrationTime"]?.ToString() ?? DateTime.Now.ToString("o")),
                                matchHistory = new List<string>((ArrayList)pd["matchHistory"])
                            });
                        }
                    }

                    // 导入比赛
                    if (td.Contains("matches"))
                    {
                        foreach (Dictionary md in (ArrayList)td["matches"])
                        {
                            var match = new TournamentMatch
                            {
                                matchId = md["matchId"]?.ToString() ?? "",
                                roundNumber = md["roundNumber"] as int? ?? 0,
                                matchNumber = md["matchNumber"] as int? ?? 0,
                                stage = (TournamentStage)(md["stage"] as int? ?? 0),
                                player1Id = md["player1Id"]?.ToString() ?? "",
                                player2Id = md["player2Id"]?.ToString() ?? "",
                                winnerId = md["winnerId"]?.ToString() ?? "",
                                player1Score = md["player1Score"] as int? ?? 0,
                                player2Score = md["player2Score"] as int? ?? 0,
                                isCompleted = md["isCompleted"] as bool? ?? false,
                                scheduledTime = DateTime.Parse(md["scheduledTime"]?.ToString() ?? DateTime.Now.ToString("o"))
                            };
                            if (md["completedTime"] != null && !string.IsNullOrEmpty(md["completedTime"]?.ToString()))
                                match.completedTime = DateTime.Parse(md["completedTime"]?.ToString());
                            tournament.matches.Add(match);
                        }
                    }

                    // 导入奖励
                    if (td.Contains("rewards"))
                    {
                        foreach (Dictionary rd in (ArrayList)td["rewards"])
                        {
                            tournament.rewards.Add(new TournamentReward
                            {
                                rankStart = rd["rankStart"] as int? ?? 0,
                                rankEnd = rd["rankEnd"] as int? ?? 0,
                                rewardType = rd["rewardType"]?.ToString() ?? "",
                                rewardId = rd["rewardId"]?.ToString() ?? "",
                                rewardAmount = rd["rewardAmount"] as int? ?? 0
                            });
                        }
                    }

                    Tournaments[tournament.tournamentId] = tournament;
                }
            }

            // 恢复活动锦标赛
            if (data.Contains("activeTournamentIds"))
            {
                foreach (string tid in (ArrayList)data["activeTournamentIds"])
                {
                    if (Tournaments.TryGetValue(tid, out var tournament))
                    {
                        ActiveTournaments.Add(tournament);
                    }
                }
            }

            // 导入玩家进度
            if (data.Contains("playerProgress"))
            {
                foreach (Dictionary pd in (ArrayList)data["playerProgress"])
                {
                    var progress = new TournamentProgress
                    {
                        playerId = pd["playerId"]?.ToString() ?? "",
                        participatedTournaments = new List<string>((ArrayList)pd["participatedTournaments"])
                    };

                    // 导入最近记录
                    if (pd.Contains("recentRecords"))
                    {
                        foreach (Dictionary rd in (ArrayList)pd["recentRecords"])
                        {
                            progress.recentRecords.Add(new PlayerTournamentRecord
                            {
                                playerId = rd["playerId"]?.ToString() ?? "",
                                tournamentId = rd["tournamentId"]?.ToString() ?? "",
                                tournamentName = rd["tournamentName"]?.ToString() ?? "",
                                finalRank = rd["finalRank"] as int? ?? 0,
                                score = rd["score"] as int? ?? 0,
                                wins = rd["wins"] as int? ?? 0,
                                losses = rd["losses"] as int? ?? 0,
                                participatedAt = DateTime.Parse(rd["participatedAt"]?.ToString() ?? DateTime.Now.ToString("o"))
                            });
                        }
                    }

                    // 导入统计
                    if (pd.Contains("statistics"))
                    {
                        var sd = (Dictionary)pd["statistics"];
                        progress.statistics = new TournamentStatistics
                        {
                            playerId = sd["playerId"]?.ToString() ?? "",
                            totalTournaments = sd["totalTournaments"] as int? ?? 0,
                            firstPlace = sd["firstPlace"] as int? ?? 0,
                            secondPlace = sd["secondPlace"] as int? ?? 0,
                            thirdPlace = sd["thirdPlace"] as int? ?? 0,
                            top4 = sd["top4"] as int? ?? 0,
                            top8 = sd["top8"] as int? ?? 0,
                            top16 = sd["top16"] as int? ?? 0,
                            totalWins = sd["totalWins"] as int? ?? 0,
                            totalLosses = sd["totalLosses"] as int? ?? 0,
                            highestRank = sd["highestRank"] as int? ?? 0,
                            totalPrizeWon = sd["totalPrizeWon"] as int? ?? 0
                        };
                    }

                    PlayerProgress[progress.playerId] = progress;
                }
            }

            GD.Print($"[ArenaTournamentCoreSystem] 导入 {Tournaments.Count} 个锦标赛, {PlayerProgress.Count} 个玩家进度");
        }

        #endregion
    }
}
