using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using Framework;

namespace ClawRPG.Scripts.Leaderboard {
    /// <summary>
    /// 排行榜核心系统
    /// </summary>
    public partial class LeaderboardSystem : BaseSystem {
        private static LeaderboardSystem _instance;
        public static LeaderboardSystem Instance => _instance;

        // 排行榜数据存储
        private Dictionary<LeaderboardType, LeaderboardData> _leaderboards = 
            new Dictionary<LeaderboardType, LeaderboardData>();

        // 玩家数据
        private Dictionary<string, PlayerLeaderboardData> _playerData = 
            new Dictionary<string, PlayerLeaderboardData>();

        // 排行榜变化历史
        private List<LeaderboardChange> _changeHistory = new List<LeaderboardChange>();

        // 刷新计时器
        private Dictionary<LeaderboardType, float> _updateTimers = 
            new Dictionary<LeaderboardType, float>();

        // 数据库引用
        private LeaderboardDatabase _database;

        // 信号
public delegate void LeaderboardUpdated(LeaderboardType type);
public delegate void RankChanged(string playerId, LeaderboardType type, int oldRank, int newRank);
public delegate void RewardClaimed(string playerId, LeaderboardType type, int rank, RankReward reward);

        public override void _Ready() {
            _instance = this;
            _database = GetNode<LeaderboardDatabase>("/root/Main/LeaderboardDatabase");
            if (_database == null) {
                _database = new LeaderboardDatabase();
                AddChild(_database);
            }
            InitializeLeaderboards();
        }

        public override void _Process(double delta) {
            UpdateTimers(delta);
        }

        private void InitializeLeaderboards() {
            foreach (LeaderboardType type in Enum.GetValues(typeof(LeaderboardType))) {
                _leaderboards[type] = new LeaderboardData {
                    Type = type,
                    Period = LeaderboardPeriod.AllTime,
                    Entries = new List<LeaderboardEntry>(),
                    LastReset = DateTime.Now
                };
                _updateTimers[type] = 0f;
            }
        }

        private void UpdateTimers(float delta) {
            foreach (var kvp in _updateTimers.ToList()) {
                var config = _database.GetConfig(kvp.Key);
                if (config != null) {
                    _updateTimers[kvp.Key] += delta;
                    if (_updateTimers[kvp.Key] >= config.UpdateFrequency) {
                        _updateTimers[kvp.Key] = 0f;
                        // 触发更新（可以在这里添加服务器同步逻辑）
                    }
                }
            }
        }

        /// <summary>
        /// 更新玩家分数
        /// </summary>
        public void UpdateScore(string playerId, string playerName, LeaderboardType type, long score) {
            if (!_leaderboards.ContainsKey(type)) return;

            var leaderboard = _leaderboards[type];
            var entry = leaderboard.Entries.FirstOrDefault(e => e.PlayerId == playerId);

            int oldRank = entry?.Rank ?? 0;
            long previousValue = entry?.Value ?? 0;

            if (entry == null) {
                entry = new LeaderboardEntry {
                    PlayerId = playerId,
                    PlayerName = playerName,
                    Value = score,
                    LastUpdated = DateTime.Now
                };
                leaderboard.Entries.Add(entry);
            } else {
                entry.Value = score;
                entry.LastUpdated = DateTime.Now;
                entry.PlayerName = playerName;
            }

            // 重新排序
            SortLeaderboard(type);

            // 重新计算排名
            UpdateRanks(type);

            // 获取新排名
            entry = leaderboard.Entries.FirstOrDefault(e => e.PlayerId == playerId);
            int newRank = entry?.Rank ?? 0;

            // 记录变化
            if (oldRank != newRank) {
                RecordChange(playerId, playerName, type, oldRank, newRank, previousValue, score);
                EmitSignal(nameof(RankChanged), playerId, type, oldRank, newRank);
            }

            // 更新玩家数据
            UpdatePlayerData(playerId, playerName, type, score, newRank, oldRank);

            leaderboard.IsDirty = true;
            EmitSignal(nameof(LeaderboardUpdated), type);
        }

        /// <summary>
        /// 增加玩家分数
        /// </summary>
        public void AddScore(string playerId, string playerName, LeaderboardType type, long amount) {
            long currentScore = GetPlayerScore(playerId, type);
            UpdateScore(playerId, playerName, type, currentScore + amount);
        }

        /// <summary>
        /// 获取玩家分数
        /// </summary>
        public long GetPlayerScore(string playerId, LeaderboardType type) {
            if (_playerData.ContainsKey(playerId) && _playerData[playerId].Scores.ContainsKey(type)) {
                return _playerData[playerId].Scores[type];
            }
            return 0;
        }

        /// <summary>
        /// 获取玩家排名
        /// </summary>
        public int GetPlayerRank(string playerId, LeaderboardType type) {
            if (_playerData.ContainsKey(playerId) && _playerData[playerId].Ranks.ContainsKey(type)) {
                return _playerData[playerId].Ranks[type];
            }
            return 0;
        }

        /// <summary>
        /// 获取排行榜条目
        /// </summary>
        public List<LeaderboardEntry> GetLeaderboard(LeaderboardType type, int offset = 0, int limit = 100) {
            if (!_leaderboards.ContainsKey(type)) return new List<LeaderboardEntry>();

            var entries = _leaderboards[type].Entries;
            return entries.Skip(offset).Take(limit).ToList();
        }

        /// <summary>
        /// 获取排行榜前N名
        /// </summary>
        public List<LeaderboardEntry> GetTopEntries(LeaderboardType type, int count = 10) {
            return GetLeaderboard(type, 0, count);
        }

        /// <summary>
        /// 获取玩家周围的排名
        /// </summary>
        public List<LeaderboardEntry> GetEntriesAroundPlayer(string playerId, LeaderboardType type, int range = 5) {
            if (!_leaderboards.ContainsKey(type)) return new List<LeaderboardEntry>();

            var leaderboard = _leaderboards[type];
            var playerEntry = leaderboard.Entries.FirstOrDefault(e => e.PlayerId == playerId);
            if (playerEntry == null) return new List<LeaderboardEntry>();

            int playerIndex = leaderboard.Entries.IndexOf(playerEntry);
            int startIndex = Math.Max(0, playerIndex - range);
            int endIndex = Math.Min(leaderboard.Entries.Count - 1, playerIndex + range);

            var result = new List<LeaderboardEntry>();
            for (int i = startIndex; i <= endIndex; i++) {
                result.Add(leaderboard.Entries[i]);
            }
            return result;
        }

        /// <summary>
        /// 获取排行榜统计
        /// </summary>
        public LeaderboardStatistics GetStatistics(LeaderboardType type) {
            if (!_leaderboards.ContainsKey(type)) return null;

            var entries = _leaderboards[type].Entries;
            if (entries.Count == 0) return null;

            var topEntry = entries.First();
            return new LeaderboardStatistics {
                TotalEntries = entries.Count,
                HighestScore = topEntry.Value,
                TopPlayerId = topEntry.PlayerId,
                TopPlayerName = topEntry.PlayerName,
                LastUpdate = entries.Max(e => e.LastUpdate)
            };
        }

        /// <summary>
        /// 获取排名变化
        /// </summary>
        public List<LeaderboardChange> GetRecentChanges(LeaderboardType type, int count = 10) {
            return _changeHistory
                .Where(c => c.Type == type)
                .OrderByDescending(c => c.Timestamp)
                .Take(count)
                .ToList();
        }

        /// <summary>
        /// 领取排名奖励
        /// </summary>
        public RankReward ClaimReward(string playerId, LeaderboardType type) {
            int rank = GetPlayerRank(playerId, type);
            if (rank == 0) return null;

            var reward = _database.GetRewardForRank(type, rank);
            if (reward == null) return null;

            // 发放奖励（金币、经验、称号）
            EmitSignal(nameof(RewardClaimed), playerId, type, rank, reward);
            return reward;
        }

        /// <summary>
        /// 重置排行榜
        /// </summary>
        public void ResetLeaderboard(LeaderboardType type) {
            if (!_leaderboards.ContainsKey(type)) return;

            _leaderboards[type].Entries.Clear();
            _leaderboards[type].LastReset = DateTime.Now;
            _leaderboards[type].IsDirty = true;

            // 清除相关玩家数据
            foreach (var player in _playerData.Values) {
                if (player.Scores.ContainsKey(type)) player.Scores.Remove(type);
                if (player.Ranks.ContainsKey(type)) player.Ranks.Remove(type);
                if (player.PreviousRanks.ContainsKey(type)) player.PreviousRanks.Remove(type);
                if (player.LastUpdateTimes.ContainsKey(type)) player.LastUpdateTimes.Remove(type);
            }

            EmitSignal(nameof(LeaderboardUpdated), type);
        }

        /// <summary>
        /// 移除玩家数据
        /// </summary>
        public void RemovePlayer(string playerId) {
            foreach (var leaderboard in _leaderboards.Values) {
                var entry = leaderboard.Entries.FirstOrDefault(e => e.PlayerId == playerId);
                if (entry != null) {
                    leaderboard.Entries.Remove(entry);
                    leaderboard.IsDirty = true;
                }
            }

            if (_playerData.ContainsKey(playerId)) {
                _playerData.Remove(playerId);
            }
        }

        private void SortLeaderboard(LeaderboardType type) {
            if (!_leaderboards.ContainsKey(type)) return;

            var config = _database.GetConfig(type);
            var entries = _leaderboards[type].Entries;

            if (config != null && config.SortDescending) {
                entries.Sort((a, b) => b.Value.CompareTo(a.Value));
            } else {
                entries.Sort((a, b) => a.Value.CompareTo(b.Value));
            }
        }

        private void UpdateRanks(LeaderboardType type) {
            if (!_leaderboards.ContainsKey(type)) return;

            var entries = _leaderboards[type].Entries;
            for (int i = 0; i < entries.Count; i++) {
                entries[i].PreviousRank = entries[i].Rank;
                entries[i].Rank = i + 1;
            }
        }

        private void UpdatePlayerData(string playerId, string playerName, LeaderboardType type, long score, int rank, int previousRank) {
            if (!_playerData.ContainsKey(playerId)) {
                _playerData[playerId] = new PlayerLeaderboardData {
                    PlayerId = playerId,
                    PlayerName = playerName
                };
            }

            var player = _playerData[playerId];
            player.PlayerName = playerName;
            player.Scores[type] = score;
            player.Ranks[type] = rank;
            player.PreviousRanks[type] = previousRank;
            player.LastUpdateTimes[type] = DateTime.Now;
        }

        private void RecordChange(string playerId, string playerName, LeaderboardType type, 
            int oldRank, int newRank, long previousValue, long newValue) {
            var change = new LeaderboardChange {
                PlayerId = playerId,
                PlayerName = playerName,
                Type = type,
                PreviousRank = oldRank,
                NewRank = newRank,
                PreviousValue = previousValue,
                NewValue = newValue,
                Timestamp = DateTime.Now
            };
            _changeHistory.Insert(0, change);

            // 保留最近1000条记录
            if (_changeHistory.Count > 1000) {
                _changeHistory.RemoveAt(_changeHistory.Count - 1);
            }
        }

        /// <summary>
        /// 导出存档数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData() {
            var data = new Dictionary<string, object>();
            var leaderboardData = new Dictionary<string, object>();

            foreach (var kvp in _leaderboards) {
                var entries = new List<Dictionary<string, object>>();
                foreach (var entry in kvp.Value.Entries) {
                    entries.Add(new Dictionary<string, object> {
                        ["player_id"] = entry.PlayerId,
                        ["player_name"] = entry.PlayerName,
                        ["rank"] = entry.Rank,
                        ["value"] = entry.Value,
                        ["previous_rank"] = entry.PreviousRank,
                        ["last_updated"] = entry.LastUpdated.ToBinary()
                    });
                }
                leaderboardData[kvp.Key.ToString()] = entries;
            }

            data["leaderboards"] = leaderboardData;
            data["change_history_count"] = _changeHistory.Count;
            return data;
        }

        /// <summary>
        /// 导入存档数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data) {
            if (!data.ContainsKey("leaderboards")) return;

            var leaderboardData = data["leaderboards"] as Dictionary<string, object>;
            foreach (var kvp in leaderboardData) {
                if (Enum.TryParse<LeaderboardType>(kvp.Key, out var type)) {
                    var entries = kvp.Value as List<object>;
                    if (entries != null && _leaderboards.ContainsKey(type)) {
                        _leaderboards[type].Entries.Clear();
                        foreach (var entryData in entries) {
                            var entryDict = entryData as Dictionary<string, object>;
                            if (entryDict != null) {
                                var entry = new LeaderboardEntry {
                                    PlayerId = entryDict["player_id"] as string,
                                    PlayerName = entryDict["player_name"] as string,
                                    Rank = Convert.ToInt32(entryDict["rank"]),
                                    Value = Convert.ToInt64(entryDict["value"]),
                                    PreviousRank = Convert.ToInt32(entryDict["previous_rank"]),
                                    LastUpdated = DateTime.FromBinary(Convert.ToInt64(entryDict["last_updated"]))
                                };
                                _leaderboards[type].Entries.Add(entry);
                            }
                        }
                    }
                }
            }
        }
    }
}
