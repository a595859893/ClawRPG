using System;
using System.Collections.Generic;
using System.Linq;
using ClawRPG.Scripts.Data;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 锦标赛奖励系统 - 负责奖励发放和玩家记录更新
    /// </summary>
    public partial class TournamentRewardSystem : BaseSystem
    {
        private static TournamentRewardSystem _instance;
        public static TournamentRewardSystem Instance => _instance;

        // 事件：奖励发放
        public event Action<string, string, int, List<TournamentReward>> OnRewardsDistributed;
        // 事件：锦标赛完成
        public event Action<string, List<TournamentPlayer>> OnTournamentCompleted;

        public override void _Ready()
        {
            _instance = this;
            InitializeSystem();
        }

        private void InitializeSystem()
        {
            GD.Print("[TournamentRewardSystem] 奖励系统初始化");
            IsInitialized = true;
        }

        #region Reward Distribution

        /// <summary>
        /// 发放锦标赛奖励
        /// </summary>
        public void DistributeRewards(Tournament tournament)
        {
            if (tournament == null)
            {
                GD.PrintErr("[TournamentRewardSystem] 锦标赛不存在");
                return;
            }

            var core = ArenaTournamentCoreSystem.Instance;
            
            // 计算排名
            var rankings = tournament.registeredPlayers
                .OrderByDescending(p => p.score)
                .ThenByDescending(p => p.wins)
                .ToList();
            
            // 更新玩家最终排名分数
            for (int i = 0; i < rankings.Count; i++)
            {
                rankings[i].score = i + 1;
            }
            
            // 发放奖励
            for (int i = 0; i < rankings.Count; i++)
            {
                int rank = i + 1;
                var player = rankings[i];
                var playerRewards = new List<TournamentReward>();
                
                foreach (var reward in tournament.rewards)
                {
                    if (rank >= reward.rankStart && rank <= reward.rankEnd)
                    {
                        // 发放奖励 (这里可以扩展为实际的奖励发放逻辑)
                        GD.Print($"[TournamentRewardSystem] 玩家 {player.playerName} (排名 {rank}) 获得奖励: {reward.rewardType} x{reward.rewardAmount}");
                        playerRewards.Add(reward);
                    }
                }
                
                if (playerRewards.Count > 0)
                {
                    OnRewardsDistributed?.Invoke(tournament.tournamentId, player.playerId, rank, playerRewards);
                }
            }
            
            // 更新玩家记录
            UpdatePlayerRecords(tournament, rankings);
            
            // 完成锦标赛
            CompleteTournament(tournament);
            
            GD.Print($"[TournamentRewardSystem] 锦标赛 {tournament.tournamentName} 奖励发放完成");
        }

        private void CompleteTournament(Tournament tournament)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            tournament.status = TournamentStatus.Completed;
            tournament.currentStage = TournamentStage.Completed;
            tournament.endTime = DateTime.Now;
            
            // 从活动列表中移除
            core.ActiveTournaments.Remove(tournament);
            
            var rankings = tournament.registeredPlayers
                .OrderByDescending(p => p.score)
                .ThenByDescending(p => p.wins)
                .ToList();
            
            GD.Print($"[TournamentRewardSystem] 锦标赛 {tournament.tournamentName} 结束!");
            OnTournamentCompleted?.Invoke(tournament.tournamentId, rankings);
        }

        private void UpdatePlayerRecords(Tournament tournament, List<TournamentPlayer> rankings)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            foreach (var player in tournament.registeredPlayers)
            {
                int rank = rankings.FindIndex(p => p.playerId == player.playerId) + 1;
                
                // 获取或创建玩家进度
                if (!core.PlayerProgress.ContainsKey(player.playerId))
                {
                    core.PlayerProgress[player.playerId] = new TournamentProgress
                    {
                        playerId = player.playerId,
                        statistics = new TournamentStatistics { playerId = player.playerId }
                    };
                }
                
                var progress = core.PlayerProgress[player.playerId];
                
                // 添加参与的锦标赛
                progress.participatedTournaments.Add(tournament.tournamentId);
                
                // 创建记录
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
                UpdateStatistics(progress, player, rank, tournament);
            }
        }

        private void UpdateStatistics(TournamentProgress progress, TournamentPlayer player, int rank, Tournament tournament)
        {
            var stats = progress.statistics;
            
            stats.totalTournaments++;
            stats.totalWins += player.wins;
            stats.totalLosses += player.losses;
            
            // 更新排名统计
            if (rank == 1) stats.firstPlace++;
            else if (rank == 2) stats.secondPlace++;
            else if (rank == 3) stats.thirdPlace++;
            else if (rank <= 4) stats.top4++;
            else if (rank <= 8) stats.top8++;
            else if (rank <= 16) stats.top16++;
            
            // 更新最高排名
            if (stats.highestRank == 0 || rank < stats.highestRank)
            {
                stats.highestRank = rank;
            }
            
            // 更新奖金
            stats.totalPrizeWon += tournament.prizePool / tournament.currentPlayerCount;
        }

        #endregion

        #region Query Methods

        /// <summary>
        /// 获取玩家锦标赛统计
        /// </summary>
        public TournamentStatistics GetPlayerStatistics(string playerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (core.PlayerProgress.TryGetValue(playerId, out var progress))
            {
                return progress.statistics;
            }
            
            return null;
        }

        /// <summary>
        /// 获取玩家最近记录
        /// </summary>
        public List<PlayerTournamentRecord> GetRecentRecords(string playerId, int count = 10)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (core.PlayerProgress.TryGetValue(playerId, out var progress))
            {
                return progress.recentRecords.Take(count).ToList();
            }
            
            return new List<PlayerTournamentRecord>();
        }

        /// <summary>
        /// 获取玩家总参赛次数
        /// </summary>
        public int GetTotalTournaments(string playerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (core.PlayerProgress.TryGetValue(playerId, out var progress))
            {
                return progress.statistics?.totalTournaments ?? 0;
            }
            
            return 0;
        }

        /// <summary>
        /// 获取玩家最佳排名
        /// </summary>
        public int GetBestRank(string playerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (core.PlayerProgress.TryGetValue(playerId, out var progress))
            {
                return progress.statistics?.highestRank ?? 0;
            }
            
            return 0;
        }

        /// <summary>
        /// 获取玩家胜率
        /// </summary>
        public float GetWinRate(string playerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (core.PlayerProgress.TryGetValue(playerId, out var progress))
            {
                var stats = progress.statistics;
                if (stats == null || stats.totalWins + stats.totalLosses == 0)
                    return 0f;
                
                return (float)stats.totalWins / (stats.totalWins + stats.totalLosses);
            }
            
            return 0f;
        }

        /// <summary>
        /// 获取玩家榜首次数
        /// </summary>
        public int GetFirstPlaceCount(string playerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (core.PlayerProgress.TryGetValue(playerId, out var progress))
            {
                return progress.statistics?.firstPlace ?? 0;
            }
            
            return 0;
        }

        /// <summary>
        /// 获取玩家前三次数
        /// </summary>
        public int GetTopThreeCount(string playerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (core.PlayerProgress.TryGetValue(playerId, out var progress))
            {
                var stats = progress.statistics;
                if (stats == null) return 0;
                
                return stats.firstPlace + stats.secondPlace + stats.thirdPlace;
            }
            
            return 0;
        }

        /// <summary>
        /// 获取玩家总奖金
        /// </summary>
        public int GetTotalPrizeWon(string playerId)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            if (core.PlayerProgress.TryGetValue(playerId, out var progress))
            {
                return progress.statistics?.totalPrizeWon ?? 0;
            }
            
            return 0;
        }

        /// <summary>
        /// 获取所有玩家排行榜 (按总成绩)
        /// </summary>
        public List<(string playerId, TournamentStatistics stats)> GetLeaderboard(int count = 100)
        {
            var core = ArenaTournamentCoreSystem.Instance;
            
            return core.PlayerProgress
                .Select(kvp => (kvp.Key, kvp.Value.statistics))
                .Where(x => x.stats != null)
                .OrderByDescending(x => x.stats.firstPlace * 100 + x.stats.secondPlace * 50 + x.stats.thirdPlace)
                .ThenByDescending(x => x.stats.totalWins)
                .Take(count)
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
