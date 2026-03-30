using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 锦标赛核心系统 - 协调各子系统
    /// 委托职责给:
    /// - TournamentMatchmakingSystem: 匹配报名
    /// - TournamentBracketSystem: 赛程生成
    /// - TournamentRewardSystem: 奖励发放
    /// - TournamentPersistenceSystem: 持久化
    /// </summary>
    public class ArenaTournamentCoreSystem : BaseSystem
    {
        // 单例
        private static ArenaTournamentCoreSystem _instance;
        public static ArenaTournamentCoreSystem Instance => _instance;

        // 子系统
        public TournamentMatchmakingSystem Matchmaking { get; private set; }
        public TournamentBracketSystem Bracket { get; private set; }
        public TournamentRewardSystem Rewards { get; private set; }
        public TournamentPersistenceSystem Persistence { get; private set; }

        // 锦标赛存储 (委托给子系统管理)
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
            
            // 初始化子系统
            InitializeSubsystems();
            
            // 加载数据
            LoadData();
        }

        private void InitializeSubsystems()
        {
            // 创建子系统节点
            Matchmaking = new TournamentMatchmakingSystem();
            Bracket = new TournamentBracketSystem();
            Rewards = new TournamentRewardSystem();
            Persistence = new TournamentPersistenceSystem();
            
            // 添加为子节点
            AddChild(Matchmaking);
            AddChild(Bracket);
            AddChild(Rewards);
            AddChild(Persistence);
            
            // 订阅子系统事件
            Matchmaking.OnTournamentCreated += OnTournamentCreated;
            Bracket.OnMatchCompleted += OnMatchCompleted;
            Rewards.OnRewardsDistributed += OnRewardsDistributed;
            
            GD.Print("[ArenaTournamentCoreSystem] 子系统初始化完成");
        }

        private void OnTournamentCreated(string tournamentId)
        {
            GD.Print($"[ArenaTournamentCoreSystem] 锦标赛创建事件: {tournamentId}");
        }

        private void OnMatchCompleted(string tournamentId, string matchId)
        {
            GD.Print($"[ArenaTournamentCoreSystem] 比赛完成事件: {tournamentId} - {matchId}");
        }

        private void OnRewardsDistributed(string tournamentId, string playerId, int rank, List<TournamentReward> rewards)
        {
            GD.Print($"[ArenaTournamentCoreSystem] 奖励发放事件: {tournamentId} - {playerId} 排名 {rank}");
        }

        #region Tournament Creation (委托给 Matchmaking)

        /// <summary>
        /// 从模板创建锦标赛
        /// </summary>
        public Tournament CreateTournamentFromTemplate(string templateId, string organizerId)
        {
            return Matchmaking.CreateTournamentFromTemplate(templateId, organizerId);
        }

        /// <summary>
        /// 创建自定义锦标赛
        /// </summary>
        public Tournament CreateCustomTournament(string name, string description, TournamentFormat format, 
            int maxPlayers, int minPlayers, int prizePool, int entryFee, string organizerId)
        {
            return Matchmaking.CreateCustomTournament(name, description, format, maxPlayers, minPlayers, prizePool, entryFee, organizerId);
        }

        #endregion

        #region Player Registration (委托给 Matchmaking)

        /// <summary>
        /// 玩家报名锦标赛
        /// </summary>
        public bool RegisterPlayer(string tournamentId, string playerId, string playerName)
        {
            return Matchmaking.RegisterPlayer(tournamentId, playerId, playerName);
        }

        /// <summary>
        /// 玩家取消报名
        /// </summary>
        public bool UnregisterPlayer(string tournamentId, string playerId)
        {
            return Matchmaking.UnregisterPlayer(tournamentId, playerId);
        }

        #endregion

        #region Tournament Execution (委托给 Bracket)

        /// <summary>
        /// 开始锦标赛
        /// </summary>
        public bool StartTournament(string tournamentId)
        {
            return Bracket.StartTournament(tournamentId);
        }

        /// <summary>
        /// 生成比赛对阵
        /// </summary>
        public void GenerateMatches(Tournament tournament)
        {
            Bracket.GenerateMatches(tournament);
        }

        /// <summary>
        /// 报告比赛结果
        /// </summary>
        public bool ReportMatchResult(string matchId, string winnerId, int winnerScore, int loserScore)
        {
            return Bracket.ReportMatchResult(matchId, winnerId, winnerScore, loserScore);
        }

        #endregion

        #region Reward Distribution (委托给 Rewards)

        /// <summary>
        /// 发放奖励 (供内部/外部调用)
        /// </summary>
        public void DistributeRewards(Tournament tournament)
        {
            Rewards.DistributeRewards(tournament);
        }

        #endregion

        #region Query Methods (便捷方法)

        /// <summary>
        /// 获取玩家锦标赛统计
        /// </summary>
        public TournamentStatistics GetPlayerStatistics(string playerId)
        {
            return Rewards.GetPlayerStatistics(playerId);
        }

        /// <summary>
        /// 获取玩家最近记录
        /// </summary>
        public List<PlayerTournamentRecord> GetRecentRecords(string playerId, int count = 10)
        {
            return Rewards.GetRecentRecords(playerId, count);
        }

        /// <summary>
        /// 获取玩家胜率
        /// </summary>
        public float GetPlayerWinRate(string playerId)
        {
            return Rewards.GetWinRate(playerId);
        }

        /// <summary>
        /// 获取玩家最佳排名
        /// </summary>
        public int GetPlayerBestRank(string playerId)
        {
            return Rewards.GetBestRank(playerId);
        }

        /// <summary>
        /// 获取玩家总奖金
        /// </summary>
        public int GetPlayerTotalPrize(string playerId)
        {
            return Rewards.GetTotalPrizeWon(playerId);
        }

        /// <summary>
        /// 获取玩家榜首次数
        /// </summary>
        public int GetPlayerFirstPlaceCount(string playerId)
        {
            return Rewards.GetFirstPlaceCount(playerId);
        }

        /// <summary>
        /// 获取玩家前三次数
        /// </summary>
        public int GetPlayerTopThreeCount(string playerId)
        {
            return Rewards.GetTopThreeCount(playerId);
        }

        /// <summary>
        /// 获取锦标赛当前轮次的所有比赛
        /// </summary>
        public List<TournamentMatch> GetCurrentRoundMatches(string tournamentId)
        {
            return Bracket.GetCurrentRoundMatches(tournamentId);
        }

        /// <summary>
        /// 获取玩家下一场比赛
        /// </summary>
        public TournamentMatch GetPlayerNextMatch(string tournamentId, string playerId)
        {
            return Bracket.GetPlayerNextMatch(tournamentId, playerId);
        }

        /// <summary>
        /// 获取比赛
        /// </summary>
        public TournamentMatch GetMatch(string matchId)
        {
            return Bracket.GetMatch(matchId);
        }

        /// <summary>
        /// 获取锦标赛当前排名
        /// </summary>
        public List<TournamentPlayer> GetRankings(string tournamentId)
        {
            return Bracket.GetRankings(tournamentId);
        }

        /// <summary>
        /// 获取所有玩家排行榜
        /// </summary>
        public List<(string playerId, TournamentStatistics stats)> GetLeaderboard(int count = 100)
        {
            return Rewards.GetLeaderboard(count);
        }

        /// <summary>
        /// 检查报名是否已满
        /// </summary>
        public bool IsRegistrationFull(string tournamentId)
        {
            return Matchmaking.IsRegistrationFull(tournamentId);
        }

        /// <summary>
        /// 检查是否满足开始条件
        /// </summary>
        public bool CanStartTournament(string tournamentId)
        {
            return Matchmaking.CanStartTournament(tournamentId);
        }

        /// <summary>
        /// 获取锦标赛报名状态
        /// </summary>
        public (int current, int max, int min) GetRegistrationStatus(string tournamentId)
        {
            return Matchmaking.GetRegistrationStatus(tournamentId);
        }

        #endregion

        #region Data Persistence (委托给 Persistence)

        /// <summary>
        /// 导出持久化数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return Persistence.ExportSaveData();
        }

        /// <summary>
        /// 导入持久化数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 先导入数据
            Persistence.ImportSaveData(data);
        }

        private void LoadData()
        {
            Persistence.LoadFromFile();
        }

        /// <summary>
        /// 保存数据
        /// </summary>
        public void SaveData()
        {
            Persistence.SaveToFile();
        }

        #endregion
    }
}
