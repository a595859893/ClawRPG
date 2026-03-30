using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 竞技场赛季系统 - 处理玩家数据、排名、统计
    /// </summary>
    public partial class ArenaSeasonSystem : BaseSystem
    {
        private static ArenaSeasonSystem _instance;
        public static ArenaSeasonSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ArenaSeasonSystem();
                return _instance;
            }
            private set { _instance = value; }
        }

        protected override string SystemName => "ArenaSeasonSystem";

        // 信号系统 - 赛季相关
        public event Action<int, ArenaColosseumData.PlayerColosseumData> OnPlayerStatsUpdated;
        public event Action<int, int> OnRatingChanged; // playerId, newRating

        // 数据
        private Dictionary<int, ArenaColosseumData.PlayerColosseumData> _playerData;
        private int _currentSeasonId = 1;
        private DateTime _seasonStartDate;
        private DateTime _seasonEndDate;

        public ArenaSeasonSystem()
        {
            _playerData = new Dictionary<int, ArenaColosseumData.PlayerColosseumData>();
            _seasonStartDate = DateTime.Now;
            _seasonEndDate = _seasonStartDate.AddMonths(1); // 默认赛季一个月
        }

        protected override void Initialize()
        {
            base.Initialize();
            GD.Print("[ArenaSeasonSystem] Initialized");
        }

        #region 公开接口

        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public ArenaColosseumData.PlayerColosseumData GetPlayerData(int playerId)
        {
            if (!_playerData.ContainsKey(playerId))
            {
                _playerData[playerId] = CreateDefaultPlayerData(playerId);
            }
            return _playerData[playerId];
        }

        /// <summary>
        /// 获取所有玩家数据（用于排名）
        /// </summary>
        public List<ArenaColosseumData.PlayerColosseumData> GetAllPlayerData()
        {
            return new List<ArenaColosseumData.PlayerColosseumData>(_playerData.Values);
        }

        /// <summary>
        /// 更新玩家统计
        /// </summary>
        public void UpdatePlayerStats(int playerId, bool isWinner, int damage, int kills, int prize)
        {
            var data = GetPlayerData(playerId);
            data.TotalMatches++;
            data.TotalEntryFees += prize;

            if (isWinner)
            {
                data.Wins++;
                data.CurrentStreak++;
                data.HighestStreak = Math.Max(data.HighestStreak, data.CurrentStreak);
                data.TotalPrizeEarned += prize;
            }
            else
            {
                data.Losses++;
                data.CurrentStreak = 0;
            }

            data.HighestDamage = Math.Max(data.HighestDamage, damage);
            data.TotalKills += kills;

            // 更新rating
            int oldRating = data.Rating;
            int ratingChange = isWinner ? 25 : -15;
            data.Rating = Math.Max(100, data.Rating + ratingChange);

            OnPlayerStatsUpdated?.Invoke(playerId, data);
            if (oldRating != data.Rating)
            {
                OnRatingChanged?.Invoke(playerId, data.Rating);
            }
        }

        /// <summary>
        /// 获取排名
        /// </summary>
        public int GetRank(int playerId)
        {
            var sortedPlayers = GetRankings();
            for (int i = 0; i < sortedPlayers.Count; i++)
            {
                if (sortedPlayers[i].PlayerId == playerId)
                    return i + 1;
            }
            return -1;
        }

        /// <summary>
        /// 获取排行榜
        /// </summary>
        public List<ArenaColosseumData.PlayerColosseumData> GetRankings(int limit = 100)
        {
            var list = new List<ArenaColosseumData.PlayerColosseumData>(_playerData.Values);
            list.Sort((a, b) => b.Rating.CompareTo(a.Rating));
            return list.GetRange(0, Math.Min(limit, list.Count));
        }

        /// <summary>
        /// 获取玩家段位
        /// </summary>
        public string GetRankTier(int playerId)
        {
            var data = GetPlayerData(playerId);
            return GetRankTierByRating(data.Rating);
        }

        /// <summary>
        /// 根据Rating获取段位名称
        /// </summary>
        public string GetRankTierByRating(int rating)
        {
            if (rating >= 2500) return "王者";
            if (rating >= 2000) return "大师";
            if (rating >= 1600) return "钻石";
            if (rating >= 1300) return "铂金";
            if (rating >= 1000) return "黄金";
            if (rating >= 700) return "白银";
            return "青铜";
        }

        /// <summary>
        /// 获取当前赛季信息
        /// </summary>
        public Dictionary<string, object> GetSeasonInfo()
        {
            return new Dictionary<string, object>
            {
                { "seasonId", _currentSeasonId },
                { "startDate", _seasonStartDate },
                { "endDate", _seasonEndDate },
                { "daysRemaining", (_seasonEndDate - DateTime.Now).Days },
                { "totalPlayers", _playerData.Count }
            };
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            int totalMatches = 0;
            int totalWins = 0;
            int totalLosses = 0;
            int totalKills = 0;

            foreach (var data in _playerData.Values)
            {
                totalMatches += data.TotalMatches;
                totalWins += data.Wins;
                totalLosses += data.Losses;
                totalKills += data.TotalKills;
            }

            return new Dictionary<string, object>
            {
                { "totalPlayers", _playerData.Count },
                { "totalMatches", totalMatches },
                { "totalWins", totalWins },
                { "totalLosses", totalLosses },
                { "totalKills", totalKills },
                { "averageRating", _playerData.Count > 0 ? GetAverageRating() : 0 }
            };
        }

        /// <summary>
        /// 添加比赛记录
        /// </summary>
        public void AddColosseumRecord(int playerId, ArenaColosseumData.ColosseumRecord record)
        {
            var data = GetPlayerData(playerId);
            data.History.Add(record);

            // 保持历史记录不超过50条
            if (data.History.Count > 50)
                data.History.RemoveAt(0);
        }

        /// <summary>
        /// 获取玩家历史记录
        /// </summary>
        public List<ArenaColosseumData.ColosseumRecord> GetPlayerHistory(int playerId)
        {
            var data = GetPlayerData(playerId);
            return new List<ArenaColosseumData.ColosseumRecord>(data.History);
        }

        #endregion

        #region 内部方法

        private ArenaColosseumData.PlayerColosseumData CreateDefaultPlayerData(int playerId)
        {
            return new ArenaColosseumData.PlayerColosseumData
            {
                PlayerId = playerId,
                TotalMatches = 0,
                Wins = 0,
                Losses = 0,
                TotalPrizeEarned = 0,
                TotalEntryFees = 0,
                HighestStreak = 0,
                CurrentStreak = 0,
                HighestDamage = 0,
                TotalKills = 0,
                Rating = 1000,
                History = new List<ArenaColosseumData.ColosseumRecord>()
            };
        }

        private int GetAverageRating()
        {
            if (_playerData.Count == 0) return 0;
            int total = 0;
            foreach (var data in _playerData.Values)
            {
                total += data.Rating;
            }
            return total / _playerData.Count;
        }

        #endregion

        #region 存档支持

        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();

            var playerDataList = new List<Dictionary<string, object>>();
            foreach (var pd in _playerData)
            {
                var historyList = new List<Dictionary<string, object>>();
                foreach (var record in pd.Value.History)
                {
                    historyList.Add(new Dictionary<string, object>
                    {
                        { "colosseumId", record.ColosseumId },
                        { "type", (int)record.Type },
                        { "isWinner", record.IsWinner },
                        { "damageDealt", record.DamageDealt },
                        { "kills", record.Kills },
                        { "prizeEarned", record.PrizeEarned },
                        { "timestamp", record.Timestamp.ToString("o") }
                    });
                }

                playerDataList.Add(new Dictionary<string, object>
                {
                    { "playerId", pd.Key },
                    { "totalMatches", pd.Value.TotalMatches },
                    { "wins", pd.Value.Wins },
                    { "losses", pd.Value.Losses },
                    { "totalPrizeEarned", pd.Value.TotalPrizeEarned },
                    { "totalEntryFees", pd.Value.TotalEntryFees },
                    { "highestStreak", pd.Value.HighestStreak },
                    { "currentStreak", pd.Value.CurrentStreak },
                    { "highestDamage", pd.Value.HighestDamage },
                    { "totalKills", pd.Value.TotalKills },
                    { "rating", pd.Value.Rating },
                    { "history", historyList }
                });
            }

            data["playerData"] = playerDataList;
            data["currentSeasonId"] = _currentSeasonId;
            data["seasonStartDate"] = _seasonStartDate.ToString("o");
            data["seasonEndDate"] = _seasonEndDate.ToString("o");

            return data;
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            if (data.Contains("currentSeasonId"))
                _currentSeasonId = Convert.ToInt32(data["currentSeasonId"]);

            if (data.Contains("seasonStartDate"))
                _seasonStartDate = DateTime.Parse(data["seasonStartDate"].ToString());

            if (data.Contains("seasonEndDate"))
                _seasonEndDate = DateTime.Parse(data["seasonEndDate"].ToString());

            if (!data.Contains("playerData")) return;

            var playerDataList = data["playerData"] as List<object>;
            if (playerDataList == null) return;

            foreach (var pdData in playerDataList)
            {
                var pd = pdData as Dictionary<string, object>;
                if (pd == null) continue;

                int playerId = Convert.ToInt32(pd["playerId"]);
                var historyList = new List<ArenaColosseumData.ColosseumRecord>();

                if (pd.Contains("history"))
                {
                    var historyData = pd["history"] as List<object>;
                    if (historyData != null)
                    {
                        foreach (var hData in historyData)
                        {
                            var h = hData as Dictionary<string, object>;
                            if (h != null)
                            {
                                historyList.Add(new ArenaColosseumData.ColosseumRecord
                                {
                                    ColosseumId = Convert.ToInt32(h["colosseumId"]),
                                    Type = (ArenaColosseumData.ColosseumType)Convert.ToInt32(h["type"]),
                                    IsWinner = Convert.ToBoolean(h["isWinner"]),
                                    DamageDealt = Convert.ToInt32(h["damageDealt"]),
                                    Kills = Convert.ToInt32(h["kills"]),
                                    PrizeEarned = Convert.ToInt32(h["prizeEarned"]),
                                    Timestamp = DateTime.Parse(h["timestamp"].ToString())
                                });
                            }
                        }
                    }
                }

                var playerColosseumData = new ArenaColosseumData.PlayerColosseumData
                {
                    PlayerId = playerId,
                    TotalMatches = Convert.ToInt32(pd["totalMatches"]),
                    Wins = Convert.ToInt32(pd["wins"]),
                    Losses = Convert.ToInt32(pd["losses"]),
                    TotalPrizeEarned = Convert.ToInt32(pd["totalPrizeEarned"]),
                    TotalEntryFees = Convert.ToInt32(pd["totalEntryFees"]),
                    HighestStreak = Convert.ToInt32(pd["highestStreak"]),
                    CurrentStreak = Convert.ToInt32(pd["currentStreak"]),
                    HighestDamage = Convert.ToInt32(pd["highestDamage"]),
                    TotalKills = Convert.ToInt32(pd["totalKills"]),
                    Rating = Convert.ToInt32(pd["rating"]),
                    History = historyList
                };

                _playerData[playerId] = playerColosseumData;
            }
        }

        #endregion
    }
}
