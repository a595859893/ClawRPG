using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 角斗场系统 - 玩家实时对战
    /// </summary>
    public class ArenaColosseumSystem
    {
        private static ArenaColosseumSystem _instance;
        public static ArenaColosseumSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ArenaColosseumSystem();
                return _instance;
            }
            private set { _instance = value; }
        }

        // 信号系统
        public event Action<ArenaColosseumData.ActiveColosseum> OnColosseumStarted;
        public event Action<ArenaColosseumData.ActiveColosseum> OnColosseumEnded;
        public event Action<ArenaColosseumData.ActiveColosseum, ArenaColosseumData.Participant> OnPlayerJoined;
        public event Action<ArenaColosseumData.ActiveColosseum, ArenaColosseumData.Participant> OnPlayerLeft;
        public event Action<ArenaColosseumData.ActiveColosseum, int, int> OnDamageDealt;
        public event Action<ArenaColosseumData.ActiveColosseum, int> OnPlayerEliminated;
        public event Action<ArenaColosseumData.ActiveColosseum, ArenaColosseumData.Participant> OnMatchFound;
        public event Action<ArenaColosseumData.ActiveColosseum> OnCountdownStarted;
        public event Action<ArenaColosseumData.ActiveColosseum> OnCountdownEnded;

        // 数据
        private List<ArenaColosseumData.Colosseum> _colosseums;
        private List<ArenaColosseumData.ActiveColosseum> _activeColosseums;
        private Dictionary<int, ArenaColosseumData.PlayerColosseumData> _playerData;
        private int _nextInstanceId = 1;

        public ArenaColosseumSystem()
        {
            _colosseums = ArenaColosseumDatabase.GetDefaultColosseums();
            _activeColosseums = new List<ArenaColosseumData.ActiveColosseum>();
            _playerData = new Dictionary<int, ArenaColosseumData.PlayerColosseumData>();
        }

        public void Initialize()
        {
            GD.Print("[ArenaColosseumSystem] Initialized");
        }

        #region 公开接口

        /// <summary>
        /// 加入角斗场
        /// </summary>
        public bool JoinColosseum(int playerId, int colosseumId, string playerName, int level, 
            int health, int damage, int wins, int losses)
        {
            var colosseum = GetColosseum(colosseumId);
            if (colosseum == null)
            {
                GD.PrintErr($"[ArenaColosseumSystem] Colosseum {colosseumId} not found");
                return false;
            }

            if (level < colosseum.MinLevel)
            {
                GD.PrintErr($"[ArenaColosseumSystem] Player level {level} too low, required {colosseum.MinLevel}");
                return false;
            }

            // 查找等待中的竞技场
            var activeColosseum = FindWaitingColosseum(colosseumId);
            if (activeColosseum == null)
            {
                // 创建新的竞技场实例
                activeColosseum = CreateColosseum(colosseumId);
                _activeColosseums.Add(activeColosseum);
            }

            // 添加参与者
            var participant = new ArenaColosseumData.Participant
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Level = level,
                Health = health,
                MaxHealth = health,
                Damage = damage,
                Wins = wins,
                Losses = losses,
                IsReady = false,
                Position = Vector2.Zero,
                IsAlive = true,
                Score = 0
            };

            activeColosseum.Participants.Add(participant);
            OnPlayerJoined?.Invoke(activeColosseum, participant);

            // 检查是否满员
            if (activeColosseum.Participants.Count >= colosseum.MaxPlayers)
            {
                // 开始匹配
                StartMatching(activeColosseum);
            }
            else
            {
                activeColosseum.State = ArenaColosseumData.ColosseumState.Waiting;
            }

            return true;
        }

        /// <summary>
        /// 离开角斗场
        /// </summary>
        public void LeaveColosseum(int playerId)
        {
            foreach (var activeColosseum in _activeColosseums)
            {
                var participant = GetParticipant(activeColosseum, playerId);
                if (participant != null)
                {
                    activeColosseum.Participants.Remove(participant);
                    OnPlayerLeft?.Invoke(activeColosseum, participant);

                    if (activeColosseum.Participants.Count == 0)
                    {
                        activeColosseum.State = ArenaColosseumData.ColosseumState.Cancelled;
                        _activeColosseums.Remove(activeColosseum);
                    }
                    else if (activeColosseum.State == ArenaColosseumData.ColosseumState.InProgress)
                    {
                        // 剩余玩家自动获胜
                        EndColosseum(activeColosseum, -1);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 准备就绪
        /// </summary>
        public void SetReady(int playerId, bool ready)
        {
            foreach (var activeColosseum in _activeColosseums)
            {
                var participant = GetParticipant(activeColosseum, playerId);
                if (participant != null)
                {
                    participant.IsReady = ready;
                    CheckAllReady(activeColosseum);
                    break;
                }
            }
        }

        /// <summary>
        /// 造成伤害
        /// </summary>
        public void DealDamage(int playerId, int targetId, int damage)
        {
            foreach (var activeColosseum in _activeColosseums)
            {
                if (activeColosseum.State != ArenaColosseumData.ColosseumState.InProgress)
                    continue;

                var attacker = GetParticipant(activeColosseum, playerId);
                var target = GetParticipant(activeColosseum, targetId);

                if (attacker != null && target != null && target.IsAlive)
                {
                    target.Health -= damage;
                    attacker.Score += damage;

                    OnDamageDealt?.Invoke(activeColosseum, playerId, damage);

                    if (target.Health <= 0)
                    {
                        target.Health = 0;
                        target.IsAlive = false;
                        attacker.Score += 100; // 击杀奖励
                        
                        OnPlayerEliminated?.Invoke(activeColosseum, targetId);

                        CheckWinner(activeColosseum);
                    }
                    break;
                }
            }
        }

        /// <summary>
        /// 移动玩家
        /// </summary>
        public void MovePlayer(int playerId, Vector2 position)
        {
            foreach (var activeColosseum in _activeColosseums)
            {
                var participant = GetParticipant(activeColosseum, playerId);
                if (participant != null)
                {
                    participant.Position = position;
                    break;
                }
            }
        }

        /// <summary>
        /// 获取活跃角斗场
        /// </summary>
        public List<ArenaColosseumData.ActiveColosseum> GetActiveColosseums()
        {
            return new List<ArenaColosseumData.ActiveColosseum>(_activeColosseums);
        }

        /// <summary>
        /// 获取玩家所在的角斗场
        /// </summary>
        public ArenaColosseumData.ActiveColosseum GetPlayerColosseum(int playerId)
        {
            foreach (var activeColosseum in _activeColosseums)
            {
                var participant = GetParticipant(activeColosseum, playerId);
                if (participant != null)
                    return activeColosseum;
            }
            return null;
        }

        /// <summary>
        /// 获取角斗场列表
        /// </summary>
        public List<ArenaColosseumData.Colosseum> GetColosseumList()
        {
            return new List<ArenaColosseumData.Colosseum>(_colosseums);
        }

        /// <summary>
        /// 获取角斗场
        /// </summary>
        public ArenaColosseumData.Colosseum GetColosseum(int id)
        {
            foreach (var c in _colosseums)
            {
                if (c.Id == id) return c;
            }
            return null;
        }

        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public ArenaColosseumData.PlayerColosseumData GetPlayerData(int playerId)
        {
            if (!_playerData.ContainsKey(playerId))
            {
                _playerData[playerId] = new ArenaColosseumData.PlayerColosseumData
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
                    Rating = 1000
                };
            }
            return _playerData[playerId];
        }

        /// <summary>
        /// 更新玩家数据
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
            int ratingChange = isWinner ? 25 : -15;
            data.Rating = Math.Max(100, data.Rating + ratingChange);
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            return new Dictionary<string, object>
            {
                { "activeColosseums", _activeColosseums.Count },
                { "waitingColosseums", CountWaitingColosseums() },
                { "totalPlayers", GetTotalPlayers() },
                { "totalMatches", GetTotalMatches() }
            };
        }

        #endregion

        #region 内部方法

        private ArenaColosseumData.ActiveColosseum FindWaitingColosseum(int colosseumId)
        {
            foreach (var ac in _activeColosseums)
            {
                if (ac.ColosseumId == colosseumId && 
                    ac.State == ArenaColosseumData.ColosseumState.Waiting)
                {
                    var colosseum = GetColosseum(ac.ColosseumId);
                    if (colosseum != null && ac.Participants.Count < colosseum.MaxPlayers)
                        return ac;
                }
            }
            return null;
        }

        private ArenaColosseumData.ActiveColosseum CreateColosseum(int colosseumId)
        {
            return new ArenaColosseumData.ActiveColosseum
            {
                InstanceId = _nextInstanceId++,
                ColosseumId = colosseumId,
                State = ArenaColosseumData.ColosseumState.Waiting,
                TimeRemaining = 0,
                CountdownTime = 5f,
                Round = 1,
                WinnerId = -1,
                StartTime = DateTime.Now
            };
        }

        private ArenaColosseumData.Participant GetParticipant(ArenaColosseumData.ActiveColosseum ac, int playerId)
        {
            foreach (var p in ac.Participants)
            {
                if (p.PlayerId == playerId) return p;
            }
            return null;
        }

        private void StartMatching(ArenaColosseumData.ActiveColosseum ac)
        {
            ac.State = ArenaColosseumData.ColosseumState.Matching;
            
            // 模拟匹配时间
            var colosseum = GetColosseum(ac.ColosseumId);
            
            // 通知所有参与者匹配成功
            foreach (var p in ac.Participants)
            {
                OnMatchFound?.Invoke(ac, p);
            }

            // 开始倒计时
            StartCountdown(ac);
        }

        private void StartCountdown(ArenaColosseumData.ActiveColosseum ac)
        {
            ac.State = ArenaColosseumData.ColosseumState.Countdown;
            OnCountdownStarted?.Invoke(ac);
        }

        public void UpdateCountdown(float delta)
        {
            foreach (var ac in _activeColosseums)
            {
                if (ac.State == ArenaColosseumData.ColosseumState.Countdown)
                {
                    ac.CountdownTime -= delta;
                    if (ac.CountdownTime <= 0)
                    {
                        StartColosseum(ac);
                    }
                }
                else if (ac.State == ArenaColosseumData.ColosseumState.InProgress)
                {
                    ac.TimeRemaining -= delta;
                    if (ac.TimeRemaining <= 0)
                    {
                        // 时间到，根据得分或存活判定胜利
                        DetermineWinner(ac);
                    }
                }
            }
        }

        private void StartColosseum(ArenaColosseumData.ActiveColosseum ac)
        {
            ac.State = ArenaColosseumData.ColosseumState.InProgress;
            
            var colosseum = GetColosseum(ac.ColosseumId);
            if (colosseum != null)
            {
                ac.TimeRemaining = colosseum.Duration;
            }

            // 重置玩家状态
            foreach (var p in ac.Participants)
            {
                p.IsAlive = true;
                p.Health = p.MaxHealth;
                p.Score = 0;
                p.Position = Vector2.Zero;
            }

            OnCountdownEnded?.Invoke(ac);
            OnColosseumStarted?.Invoke(ac);
        }

        private void CheckAllReady(ArenaColosseumData.ActiveColosseum ac)
        {
            bool allReady = true;
            foreach (var p in ac.Participants)
            {
                if (!p.IsReady)
                {
                    allReady = false;
                    break;
                }
            }

            if (allReady && ac.State == ArenaColosseumData.ColosseumState.Waiting)
            {
                StartMatching(ac);
            }
        }

        private void CheckWinner(ArenaColosseumData.ActiveColosseum ac)
        {
            var colosseum = GetColosseum(ac.ColosseumId);
            if (colosseum == null) return;

            int aliveCount = 0;
            ArenaColosseumData.Participant lastAlive = null;

            foreach (var p in ac.Participants)
            {
                if (p.IsAlive)
                {
                    aliveCount++;
                    lastAlive = p;
                }
            }

            // 大乱斗：最后存活者获胜
            if (colosseum.Type == ArenaColosseumData.ColosseumType.FreeForAll)
            {
                if (aliveCount <= 1)
                {
                    EndColosseum(ac, lastAlive?.PlayerId ?? -1);
                }
            }
            // 1v1 或团队战
            else
            {
                if (aliveCount <= 1)
                {
                    EndColosseum(ac, lastAlive?.PlayerId ?? -1);
                }
            }
        }

        private void DetermineWinner(ArenaColosseumData.ActiveColosseum ac)
        {
            var colosseum = GetColosseum(ac.ColosseumId);
            if (colosseum == null) return;

            // 根据得分确定胜利者
            ArenaColosseumData.Participant winner = null;
            int highestScore = -1;

            foreach (var p in ac.Participants)
            {
                if (p.Score > highestScore)
                {
                    highestScore = p.Score;
                    winner = p;
                }
            }

            EndColosseum(ac, winner?.PlayerId ?? -1);
        }

        private void EndColosseum(ArenaColosseumData.ActiveColosseum ac, int winnerId)
        {
            ac.State = ArenaColosseumData.ColosseumState.Completed;
            ac.WinnerId = winnerId;

            var colosseum = GetColosseum(ac.ColosseumId);
            if (colosseum != null)
            {
                // 发放奖励
                foreach (var p in ac.Participants)
                {
                    bool isWinner = (p.PlayerId == winnerId);
                    int prize = isWinner ? colosseum.WinnerReward : colosseum.LoserReward;
                    
                    UpdatePlayerStats(p.PlayerId, isWinner, p.Score, isWinner ? 1 : 0, prize);

                    // 记录历史
                    var data = GetPlayerData(p.PlayerId);
                    data.History.Add(new ArenaColosseumData.ColosseumRecord
                    {
                        ColosseumId = colosseum.Id,
                        Type = colosseum.Type,
                        IsWinner = isWinner,
                        DamageDealt = p.Score,
                        Kills = isWinner ? 1 : 0,
                        PrizeEarned = prize,
                        Timestamp = DateTime.Now
                    });

                    // 保持历史记录不超过50条
                    if (data.History.Count > 50)
                        data.History.RemoveAt(0);
                }
            }

            OnColosseumEnded?.Invoke(ac);
            
            // 移除竞技场
            _activeColosseums.Remove(ac);
        }

        private int CountWaitingColosseums()
        {
            int count = 0;
            foreach (var ac in _activeColosseums)
            {
                if (ac.State == ArenaColosseumData.ColosseumState.Waiting)
                    count++;
            }
            return count;
        }

        private int GetTotalPlayers()
        {
            int count = 0;
            foreach (var ac in _activeColosseums)
            {
                count += ac.Participants.Count;
            }
            return count;
        }

        private int GetTotalMatches()
        {
            int total = 0;
            foreach (var data in _playerData.Values)
            {
                total += data.TotalMatches;
            }
            return total;
        }

        #endregion

        #region 存档支持

        public Dictionary<string, object> GetSaveData()
        {
            var data = new Dictionary<string, object>();
            
            var playerDataList = new List<Dictionary<string, object>>();
            foreach (var pd in _playerData)
            {
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
                    { "rating", pd.Value.Rating }
                });
            }
            
            data["playerData"] = playerDataList;
            return data;
        }

        public void LoadSaveData(Dictionary<string, object> saveData)
        {
            if (saveData == null || !saveData.ContainsKey("playerData"))
                return;

            var playerDataList = saveData["playerData"] as List<object>;
            if (playerDataList == null) return;

            foreach (var pdData in playerDataList)
            {
                var pd = pdData as Dictionary<string, object>;
                if (pd == null) continue;

                int playerId = Convert.ToInt32(pd["playerId"]);
                var data = new ArenaColosseumData.PlayerColosseumData
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
                    Rating = Convert.ToInt32(pd["rating"])
                };
                
                _playerData[playerId] = data;
            }
        }

        #endregion
    }
}
