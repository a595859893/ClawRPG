using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 锦标赛查询系统 - 提供各种查询接口
    /// </summary>
    public class ArenaTournamentQueries : BaseSystem
    {
        // 单例
        private static ArenaTournamentQueries _instance;
        public static ArenaTournamentQueries Instance => _instance;

        // 引用核心系统
        private ArenaTournamentCoreSystem _coreSystem;
        
        // 缓存的查询结果
        private Dictionary<string, List<Tournament>> _tournamentCache = new Dictionary<string, List<Tournament>>();
        private DateTime _cacheTimestamp;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromSeconds(30);

        public override void _Ready()
        {
            _instance = this;
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            _coreSystem = ArenaTournamentCoreSystem.Instance;
            GD.Print("[ArenaTournamentQueries] 锦标赛查询系统初始化");
        }

        /// <summary>
        /// 获取核心系统引用（用于内部使用）
        /// </summary>
        public ArenaTournamentCoreSystem GetCoreSystem()
        {
            return _coreSystem;
        }

        #region Tournament Queries

        /// <summary>
        /// 获取所有可报名的锦标赛
        /// </summary>
        public List<Tournament> GetAvailableTournaments()
        {
            return _coreSystem.Tournaments.Values
                .Where(t => t.status == TournamentStatus.Pending && DateTime.Now <= t.registrationEnd)
                .OrderBy(t => t.registrationEnd)
                .ToList();
        }

        /// <summary>
        /// 获取进行中的锦标赛
        /// </summary>
        public List<Tournament> GetActiveTournaments()
        {
            return _coreSystem.ActiveTournaments.ToList();
        }

        /// <summary>
        /// 获取已完成的锦标赛
        /// </summary>
        public List<Tournament> GetCompletedTournaments()
        {
            return _coreSystem.Tournaments.Values
                .Where(t => t.status == TournamentStatus.Completed)
                .OrderByDescending(t => t.endTime)
                .ToList();
        }

        /// <summary>
        /// 获取玩家可报名的锦标赛
        /// </summary>
        public List<Tournament> GetJoinableTournamentsForPlayer(string playerId)
        {
            return _coreSystem.Tournaments.Values
                .Where(t => t.status == TournamentStatus.Pending 
                         && DateTime.Now <= t.registrationEnd
                         && !t.registeredPlayers.Any(p => p.playerId == playerId))
                .OrderBy(t => t.registrationEnd)
                .ToList();
        }

        /// <summary>
        /// 获取玩家已报名的锦标赛
        /// </summary>
        public List<Tournament> GetRegisteredTournaments(string playerId)
        {
            return _coreSystem.Tournaments.Values
                .Where(t => t.registeredPlayers.Any(p => p.playerId == playerId))
                .OrderBy(t => t.registrationEnd)
                .ToList();
        }

        /// <summary>
        /// 获取玩家正在参加的锦标赛
        /// </summary>
        public List<Tournament> GetPlayerActiveTournaments(string playerId)
        {
            return _coreSystem.ActiveTournaments
                .Where(t => t.registeredPlayers.Any(p => p.playerId == playerId))
                .ToList();
        }

        /// <summary>
        /// 根据状态获取锦标赛
        /// </summary>
        public List<Tournament> GetTournamentsByStatus(TournamentStatus status)
        {
            return _coreSystem.Tournaments.Values
                .Where(t => t.status == status)
                .OrderByDescending(t => t.createdAt)
                .ToList();
        }

        /// <summary>
        /// 根据赛制获取锦标赛
        /// </summary>
        public List<Tournament> GetTournamentsByFormat(TournamentFormat format)
        {
            return _coreSystem.Tournaments.Values
                .Where(t => t.format == format)
                .OrderByDescending(t => t.createdAt)
                .ToList();
        }

        #endregion

        #region Single Tournament Queries

        /// <summary>
        /// 获取锦标赛详情
        /// </summary>
        public Tournament GetTournament(string tournamentId)
        {
            return _coreSystem.Tournaments.ContainsKey(tournamentId) 
                ? _coreSystem.Tournaments[tournamentId] 
                : null;
        }

        /// <summary>
        /// 获取锦标赛参赛玩家
        /// </summary>
        public List<TournamentPlayer> GetTournamentPlayers(string tournamentId)
        {
            var tournament = GetTournament(tournamentId);
            return tournament?.registeredPlayers.OrderBy(p => p.seedNumber).ToList();
        }

        /// <summary>
        /// 获取锦标赛比赛列表
        /// </summary>
        public List<TournamentMatch> GetTournamentMatches(string tournamentId)
        {
            var tournament = GetTournament(tournamentId);
            return tournament?.matches.OrderBy(m => m.roundNumber).ThenBy(m => m.matchNumber).ToList();
        }

        /// <summary>
        /// 获取锦标赛当前轮次的比赛
        /// </summary>
        public List<TournamentMatch> GetCurrentRoundMatches(string tournamentId)
        {
            var tournament = GetTournament(tournamentId);
            if (tournament == null) return null;
            
            return tournament.matches
                .Where(m => m.roundNumber == tournament.currentRound)
                .OrderBy(m => m.matchNumber)
                .ToList();
        }

        /// <summary>
        /// 获取锦标赛待进行的比赛
        /// </summary>
        public List<TournamentMatch> GetPendingMatches(string tournamentId)
        {
            var tournament = GetTournament(tournamentId);
            if (tournament == null) return null;
            
            return tournament.matches
                .Where(m => !m.isCompleted)
                .OrderBy(m => m.scheduledTime)
                .ToList();
        }

        /// <summary>
        /// 获取锦标赛已完成的比赛
        /// </summary>
        public List<TournamentMatch> GetCompletedMatches(string tournamentId)
        {
            var tournament = GetTournament(tournamentId);
            if (tournament == null) return null;
            
            return tournament.matches
                .Where(m => m.isCompleted)
                .OrderByDescending(m => m.completedTime)
                .ToList();
        }

        #endregion

        #region Match Queries

        /// <summary>
        /// 获取玩家的下一场比赛
        /// </summary>
        public TournamentMatch GetPlayerNextMatch(string tournamentId, string playerId)
        {
            if (!_coreSystem.Tournaments.ContainsKey(tournamentId))
                return null;
            
            var tournament = _coreSystem.Tournaments[tournamentId];
            return tournament.matches
                .Where(m => !m.isCompleted && 
                           (m.player1Id == playerId || m.player2Id == playerId))
                .OrderBy(m => m.scheduledTime)
                .FirstOrDefault();
        }

        /// <summary>
        /// 获取玩家的所有比赛
        /// </summary>
        public List<TournamentMatch> GetPlayerMatches(string tournamentId, string playerId)
        {
            var tournament = GetTournament(tournamentId);
            if (tournament == null) return null;
            
            return tournament.matches
                .Where(m => m.player1Id == playerId || m.player2Id == playerId)
                .OrderBy(m => m.roundNumber)
                .ThenBy(m => m.matchNumber)
                .ToList();
        }

        /// <summary>
        /// 获取特定轮次的比赛
        /// </summary>
        public List<TournamentMatch> GetMatchesByRound(string tournamentId, int roundNumber)
        {
            var tournament = GetTournament(tournamentId);
            if (tournament == null) return null;
            
            return tournament.matches
                .Where(m => m.roundNumber == roundNumber)
                .OrderBy(m => m.matchNumber)
                .ToList();
        }

        /// <summary>
        /// 获取特定阶段的所有比赛
        /// </summary>
        public List<TournamentMatch> GetMatchesByStage(string tournamentId, TournamentStage stage)
        {
            var tournament = GetTournament(tournamentId);
            if (tournament == null) return null;
            
            return tournament.matches
                .Where(m => m.stage == stage)
                .OrderBy(m => m.roundNumber)
                .ThenBy(m => m.matchNumber)
                .ToList();
        }

        /// <summary>
        /// 获取比赛详情
        /// </summary>
        public TournamentMatch GetMatch(string matchId)
        {
            foreach (var tournament in _coreSystem.Tournaments.Values)
            {
                var match = tournament.matches.FirstOrDefault(m => m.matchId == matchId);
                if (match != null) return match;
            }
            return null;
        }

        #endregion

        #region Player Queries

        /// <summary>
        /// 获取玩家进度
        /// </summary>
        public TournamentProgress GetPlayerProgress(string playerId)
        {
            return _coreSystem.PlayerProgress.ContainsKey(playerId) 
                ? _coreSystem.PlayerProgress[playerId] 
                : null;
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
        /// 获取玩家历史记录
        /// </summary>
        public List<PlayerTournamentRecord> GetPlayerHistory(string playerId)
        {
            var progress = GetPlayerProgress(playerId);
            return progress?.recentRecords ?? new List<PlayerTournamentRecord>();
        }

        /// <summary>
        /// 获取玩家参加的锦标赛数量
        /// </summary>
        public int GetPlayerTournamentCount(string playerId)
        {
            var progress = GetPlayerProgress(playerId);
            return progress?.participatedTournaments.Count ?? 0;
        }

        /// <summary>
        /// 获取玩家排名（基于总成绩）
        /// </summary>
        public int GetPlayerGlobalRank(string playerId)
        {
            var allProgress = _coreSystem.PlayerProgress.Values.ToList();
            
            var rankedPlayers = allProgress
                .OrderByDescending(p => p.statistics?.totalWins ?? 0)
                .ThenByDescending(p => p.statistics?.firstPlace ?? 0)
                .Select((p, index) => new { p.playerId, Rank = index + 1 })
                .FirstOrDefault(x => x.playerId == playerId);
            
            return rankedPlayers?.Rank ?? 0;
        }

        /// <summary>
        /// 检查玩家是否在锦标赛中
        /// </summary>
        public bool IsPlayerInTournament(string tournamentId, string playerId)
        {
            var tournament = GetTournament(tournamentId);
            if (tournament == null) return false;
            
            return tournament.registeredPlayers.Any(p => p.playerId == playerId);
        }

        /// <summary>
        /// 获取玩家在锦标赛中的信息
        /// </summary>
        public TournamentPlayer GetPlayerInTournament(string tournamentId, string playerId)
        {
            var tournament = GetTournament(tournamentId);
            if (tournament == null) return null;
            
            return tournament.registeredPlayers.FirstOrDefault(p => p.playerId == playerId);
        }

        /// <summary>
        /// 获取锦标赛玩家排名
        /// </summary>
        public List<TournamentPlayer> GetTournamentRankings(string tournamentId)
        {
            var tournament = GetTournament(tournamentId);
            if (tournament == null) return new List<TournamentPlayer>();
            
            return tournament.registeredPlayers
                .OrderByDescending(p => p.score)
                .ThenByDescending(p => p.wins)
                .ThenBy(p => p.losses)
                .ToList();
        }

        #endregion

        #region Template Queries

        /// <summary>
        /// 获取所有模板
        /// </summary>
        public List<TournamentTemplate> GetTemplates()
        {
            return ArenaTournamentDatabase.GetAllTemplates();
        }

        /// <summary>
        /// 获取特定模板
        /// </summary>
        public TournamentTemplate GetTemplate(string templateId)
        {
            return ArenaTournamentDatabase.GetTemplate(templateId);
        }

        /// <summary>
        /// 获取适合玩家数量的模板
        /// </summary>
        public List<TournamentTemplate> GetTemplatesForPlayerCount(int playerCount)
        {
            return ArenaTournamentDatabase.GetAllTemplates()
                .Where(t => t.minPlayers <= playerCount && t.maxPlayers >= playerCount)
                .ToList();
        }

        /// <summary>
        /// 获取特定赛制的模板
        /// </summary>
        public List<TournamentTemplate> GetTemplatesByFormat(TournamentFormat format)
        {
            return ArenaTournamentDatabase.GetAllTemplates()
                .Where(t => t.format == format)
                .ToList();
        }

        #endregion

        #region Cache Management

        /// <summary>
        /// 刷新查询缓存
        /// </summary>
        public void RefreshCache()
        {
            _cacheTimestamp = DateTime.Now;
            
            _tournamentCache["available"] = GetAvailableTournaments();
            _tournamentCache["active"] = GetActiveTournaments();
            _tournamentCache["completed"] = GetCompletedTournaments();
        }

        /// <summary>
        /// 获取缓存的可用锦标赛
        /// </summary>
        public List<Tournament> GetCachedAvailableTournaments()
        {
            if (_tournamentCache.ContainsKey("available") && 
                DateTime.Now - _cacheTimestamp < _cacheDuration)
            {
                return _tournamentCache["available"];
            }
            
            RefreshCache();
            return _tournamentCache["available"];
        }

        #endregion

        #region 数据持久化

        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary();
        }

        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
        }

        #endregion
    }
}
