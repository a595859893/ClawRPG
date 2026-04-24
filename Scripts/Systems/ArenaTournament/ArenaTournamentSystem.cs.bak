using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 竞技场锦标赛系统 - 管理所有锦标赛活动（整合层）
    /// </summary>
    public partial class ArenaTournamentSystem : BaseSystem
    {
        // 单例
        private static ArenaTournamentSystem _instance;
        public static ArenaTournamentSystem Instance => _instance;

        // 子系统引用
        private ArenaTournamentCoreSystem _coreSystem;
        private ArenaTournamentQueries _queriesSystem;
        
        // 信号
        public Action<Tournament> tournament_created;
        public Action<string, string> player_registered;
        public Action<Tournament> tournament_started;
        public Action<TournamentMatch> match_started;
        public Action<TournamentMatch> match_completed;
        public Action<Tournament, TournamentStage> stage_completed;
        public Action<Tournament> tournament_completed;

        public override void _Ready()
        {
            _instance = this;
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            GD.Print("[ArenaTournamentSystem] 锦标赛系统初始化");
            
            // 确保子系统已初始化
            _coreSystem = ArenaTournamentCoreSystem.Instance;
            _queriesSystem = ArenaTournamentQueries.Instance;
            
            // 订阅核心系统事件用于转发信号
            SubscribeToCoreEvents();
            
            LoadData();
        }

        private void SubscribeToCoreEvents()
        {
            // 核心系统的事件订阅可以在这里添加
            // 目前使用直接调用方式，信号由本系统发出
        }

        #region Tournament Management (代理到核心系统)

        /// <summary>
        /// 从模板创建锦标赛
        /// </summary>
        public Tournament CreateTournamentFromTemplate(string templateId, string organizerId)
        {
            var tournament = _coreSystem.CreateTournamentFromTemplate(templateId, organizerId);
            if (tournament != null)
            {
                tournament_created?.Emit(tournament);
            }
            return tournament;
        }

        /// <summary>
        /// 创建自定义锦标赛
        /// </summary>
        public Tournament CreateCustomTournament(string name, string description, TournamentFormat format, 
            int maxPlayers, int minPlayers, int prizePool, int entryFee, string organizerId)
        {
            var tournament = _coreSystem.CreateCustomTournament(name, description, format, 
                maxPlayers, minPlayers, prizePool, entryFee, organizerId);
            if (tournament != null)
            {
                tournament_created?.Emit(tournament);
            }
            return tournament;
        }

        /// <summary>
        /// 玩家报名锦标赛
        /// </summary>
        public bool RegisterPlayer(string tournamentId, string playerId, string playerName)
        {
            var result = _coreSystem.RegisterPlayer(tournamentId, playerId, playerName);
            if (result)
            {
                player_registered?.Emit(tournamentId, playerId);
            }
            return result;
        }

        /// <summary>
        /// 开始锦标赛
        /// </summary>
        public bool StartTournament(string tournamentId)
        {
            var result = _coreSystem.StartTournament(tournamentId);
            if (result)
            {
                var tournament = _coreSystem.Tournaments[tournamentId];
                tournament_started?.Emit(tournament);
                
                // 发出第一场比赛开始的信号
                var firstMatch = _queriesSystem.GetCurrentRoundMatches(tournamentId)?.FirstOrDefault();
                if (firstMatch != null)
                {
                    match_started?.Emit(firstMatch);
                }
            }
            return result;
        }

        /// <summary>
        /// 报告比赛结果
        /// </summary>
        public bool ReportMatchResult(string matchId, string winnerId, int winnerScore, int loserScore)
        {
            var result = _coreSystem.ReportMatchResult(matchId, winnerId, winnerScore, loserScore);
            if (result)
            {
                var match = _queriesSystem.GetMatch(matchId);
                if (match != null)
                {
                    match_completed?.Emit(match);
                    
                    // 检查是否完成锦标赛
                    var tournament = _coreSystem.Tournaments.Values.FirstOrDefault(t => t.matches.Any(m => m.matchId == matchId));
                    if (tournament != null && tournament.status == TournamentStatus.Completed)
                    {
                        tournament_completed?.Emit(tournament);
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// 生成比赛对阵
        /// </summary>
        public void GenerateMatches(string tournamentId)
        {
            var tournament = _coreSystem.Tournaments[tournamentId];
            if (tournament != null)
            {
                _coreSystem.GenerateMatches(tournament);
            }
        }

        #endregion

        #region Queries (代理到查询系统)

        /// <summary>
        /// 获取所有可报名的锦标赛
        /// </summary>
        public List<Tournament> GetAvailableTournaments()
        {
            return _queriesSystem.GetAvailableTournaments();
        }

        /// <summary>
        /// 获取进行中的锦标赛
        /// </summary>
        public List<Tournament> GetActiveTournaments()
        {
            return _queriesSystem.GetActiveTournaments();
        }

        /// <summary>
        /// 获取已完成的锦标赛
        /// </summary>
        public List<Tournament> GetCompletedTournaments()
        {
            return _queriesSystem.GetCompletedTournaments();
        }

        /// <summary>
        /// 获取玩家可报名的锦标赛
        /// </summary>
        public List<Tournament> GetJoinableTournamentsForPlayer(string playerId)
        {
            return _queriesSystem.GetJoinableTournamentsForPlayer(playerId);
        }

        /// <summary>
        /// 获取玩家已报名的锦标赛
        /// </summary>
        public List<Tournament> GetRegisteredTournaments(string playerId)
        {
            return _queriesSystem.GetRegisteredTournaments(playerId);
        }

        /// <summary>
        /// 获取玩家正在参加的锦标赛
        /// </summary>
        public List<Tournament> GetPlayerActiveTournaments(string playerId)
        {
            return _queriesSystem.GetPlayerActiveTournaments(playerId);
        }

        /// <summary>
        /// 获取锦标赛详情
        /// </summary>
        public Tournament GetTournament(string tournamentId)
        {
            return _queriesSystem.GetTournament(tournamentId);
        }

        /// <summary>
        /// 获取锦标赛参赛玩家
        /// </summary>
        public List<TournamentPlayer> GetTournamentPlayers(string tournamentId)
        {
            return _queriesSystem.GetTournamentPlayers(tournamentId);
        }

        /// <summary>
        /// 获取锦标赛比赛列表
        /// </summary>
        public List<TournamentMatch> GetTournamentMatches(string tournamentId)
        {
            return _queriesSystem.GetTournamentMatches(tournamentId);
        }

        /// <summary>
        /// 获取锦标赛当前轮次的比赛
        /// </summary>
        public List<TournamentMatch> GetCurrentRoundMatches(string tournamentId)
        {
            return _queriesSystem.GetCurrentRoundMatches(tournamentId);
        }

        /// <summary>
        /// 获取玩家的下一场比赛
        /// </summary>
        public TournamentMatch GetPlayerNextMatch(string tournamentId, string playerId)
        {
            return _queriesSystem.GetPlayerNextMatch(tournamentId, playerId);
        }

        /// <summary>
        /// 获取玩家的所有比赛
        /// </summary>
        public List<TournamentMatch> GetPlayerMatches(string tournamentId, string playerId)
        {
            return _queriesSystem.GetPlayerMatches(tournamentId, playerId);
        }

        /// <summary>
        /// 获取比赛详情
        /// </summary>
        public TournamentMatch GetMatch(string matchId)
        {
            return _queriesSystem.GetMatch(matchId);
        }

        /// <summary>
        /// 获取玩家进度
        /// </summary>
        public TournamentProgress GetPlayerProgress(string playerId)
        {
            return _queriesSystem.GetPlayerProgress(playerId);
        }

        /// <summary>
        /// 获取玩家统计
        /// </summary>
        public TournamentStatistics GetPlayerStatistics(string playerId)
        {
            return _queriesSystem.GetPlayerStatistics(playerId);
        }

        /// <summary>
        /// 获取玩家历史记录
        /// </summary>
        public List<PlayerTournamentRecord> GetPlayerHistory(string playerId)
        {
            return _queriesSystem.GetPlayerHistory(playerId);
        }

        /// <summary>
        /// 获取玩家参加的锦标赛数量
        /// </summary>
        public int GetPlayerTournamentCount(string playerId)
        {
            return _queriesSystem.GetPlayerTournamentCount(playerId);
        }

        /// <summary>
        /// 获取玩家排名
        /// </summary>
        public int GetPlayerGlobalRank(string playerId)
        {
            return _queriesSystem.GetPlayerGlobalRank(playerId);
        }

        /// <summary>
        /// 检查玩家是否在锦标赛中
        /// </summary>
        public bool IsPlayerInTournament(string tournamentId, string playerId)
        {
            return _queriesSystem.IsPlayerInTournament(tournamentId, playerId);
        }

        /// <summary>
        /// 获取玩家在锦标赛中的信息
        /// </summary>
        public TournamentPlayer GetPlayerInTournament(string tournamentId, string playerId)
        {
            return _queriesSystem.GetPlayerInTournament(tournamentId, playerId);
        }

        /// <summary>
        /// 获取锦标赛玩家排名
        /// </summary>
        public List<TournamentPlayer> GetTournamentRankings(string tournamentId)
        {
            return _queriesSystem.GetTournamentRankings(tournamentId);
        }

        /// <summary>
        /// 获取所有模板
        /// </summary>
        public List<TournamentTemplate> GetTemplates()
        {
            return _queriesSystem.GetTemplates();
        }

        /// <summary>
        /// 获取特定模板
        /// </summary>
        public TournamentTemplate GetTemplate(string templateId)
        {
            return _queriesSystem.GetTemplate(templateId);
        }

        /// <summary>
        /// 获取适合玩家数量的模板
        /// </summary>
        public List<TournamentTemplate> GetTemplatesForPlayerCount(int playerCount)
        {
            return _queriesSystem.GetTemplatesForPlayerCount(playerCount);
        }

        /// <summary>
        /// 获取特定赛制的模板
        /// </summary>
        public List<TournamentTemplate> GetTemplatesByFormat(TournamentFormat format)
        {
            return _queriesSystem.GetTemplatesByFormat(format);
        }

        #endregion

        #region Data Management

        private void LoadData()
        {
            GD.Print("[ArenaTournamentSystem] 数据加载完成");
        }

        public void SaveData()
        {
            _coreSystem.SaveData();
            GD.Print("[ArenaTournamentSystem] 数据保存完成");
        }

        #endregion

        #region BaseSystem 持久化接口

        public override Dictionary<string, object> ExportSaveData()
        {
            var tournamentsData = new List<Dictionary<string, object>>();
            foreach (var kvp in _coreSystem.Tournaments)
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
            foreach (var kvp in _coreSystem.PlayerProgress)
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
                    _coreSystem.Tournaments[tournament.tournamentId] = tournament;
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
                    _coreSystem.PlayerProgress[progress.playerId] = progress;
                }
            }

            GD.Print("[ArenaTournamentSystem] 数据已加载");
        }

        #endregion
    }
}
