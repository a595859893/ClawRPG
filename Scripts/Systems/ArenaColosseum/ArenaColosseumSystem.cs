using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 角斗场系统 - 玩家实时对战
    /// 协调者：委托给子系统处理具体逻辑
    /// </summary>
    public partial class ArenaColosseumSystem : BaseSystem
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

        protected override string SystemName => "ArenaColosseumSystem";

        // 信号系统 - 转发子系统的信号
        public event Action<ArenaColosseumData.ActiveColosseum> OnColosseumStarted;
        public event Action<ArenaColosseumData.ActiveColosseum> OnColosseumEnded;
        public event Action<ArenaColosseumData.ActiveColosseum, ArenaColosseumData.Participant> OnPlayerJoined;
        public event Action<ArenaColosseumData.ActiveColosseum, ArenaColosseumData.Participant> OnPlayerLeft;
        public event Action<ArenaColosseumData.ActiveColosseum, int, int> OnDamageDealt;
        public event Action<ArenaColosseumData.ActiveColosseum, int> OnPlayerEliminated;
        public event Action<ArenaColosseumData.ActiveColosseum, ArenaColosseumData.Participant> OnMatchFound;
        public event Action<ArenaColosseumData.ActiveColosseum> OnCountdownStarted;
        public event Action<ArenaColosseumData.ActiveColosseum> OnCountdownEnded;

        // 子系统
        private ArenaMatchmakingSystem _matchmakingSystem;
        private ArenaSeasonSystem _seasonSystem;
        private ArenaRewardSystem _rewardSystem;

        // 内部数据（战斗进行中的实时状态）
        private List<ArenaColosseumData.ActiveColosseum> _activeColosseums;

        public ArenaColosseumSystem()
        {
            _activeColosseums = new List<ArenaColosseumData.ActiveColosseum>();
        }

        protected override void Initialize()
        {
            // 初始化子系统
            _matchmakingSystem = ArenaMatchmakingSystem.Instance;
            _seasonSystem = ArenaSeasonSystem.Instance;
            _rewardSystem = ArenaRewardSystem.Instance;

            // 订阅子系统事件
            SubscribeToSubsystems();

            _matchmakingSystem.Initialize();
            _seasonSystem.Initialize();
            _rewardSystem.Initialize();

            GD.Print("[ArenaColosseumSystem] Initialized");
            IsInitialized = true;
        }

        private void SubscribeToSubsystems()
        {
            // 转发匹配系统事件
            _matchmakingSystem.OnPlayerJoined += (ac, p) => OnPlayerJoined?.Invoke(ac, p);
            _matchmakingSystem.OnPlayerLeft += (ac, p) => OnPlayerLeft?.Invoke(ac, p);
            _matchmakingSystem.OnMatchFound += (ac, p) => OnMatchFound?.Invoke(ac, p);
            _matchmakingSystem.OnCountdownStarted += (ac) => OnCountdownStarted?.Invoke(ac);
            _matchmakingSystem.OnCountdownEnded += (ac) => OnCountdownEnded?.Invoke(ac);
            _matchmakingSystem.OnColosseumStarted += (ac) => 
            {
                _activeColosseums.Add(ac);
                OnColosseumStarted?.Invoke(ac);
            };

            // 转发奖励系统事件
            _rewardSystem.OnColosseumEnded += (ac, winnerId) => 
            {
                _activeColosseums.Remove(ac);
                OnColosseumEnded?.Invoke(ac);
            };
        }

        #region 公开接口（保持向后兼容）

        /// <summary>
        /// 加入角斗场
        /// </summary>
        public bool JoinColosseum(int playerId, int colosseumId, string playerName, int level,
            int health, int damage, int wins, int losses)
        {
            return _matchmakingSystem.JoinColosseum(playerId, colosseumId, playerName, level, health, damage, wins, losses);
        }

        /// <summary>
        /// 离开角斗场
        /// </summary>
        public void LeaveColosseum(int playerId)
        {
            _matchmakingSystem.LeaveColosseum(playerId);
        }

        /// <summary>
        /// 准备就绪
        /// </summary>
        public void SetReady(int playerId, bool ready)
        {
            _matchmakingSystem.SetReady(playerId, ready);
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

                var attacker = _matchmakingSystem.GetParticipant(activeColosseum, playerId);
                var target = _matchmakingSystem.GetParticipant(activeColosseum, targetId);

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
                var participant = _matchmakingSystem.GetParticipant(activeColosseum, playerId);
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
            return _matchmakingSystem.GetPlayerColosseum(playerId);
        }

        /// <summary>
        /// 获取角斗场列表
        /// </summary>
        public List<ArenaColosseumData.Colosseum> GetColosseumList()
        {
            return _matchmakingSystem.GetColosseumList();
        }

        /// <summary>
        /// 获取角斗场
        /// </summary>
        public ArenaColosseumData.Colosseum GetColosseum(int id)
        {
            return _matchmakingSystem.GetColosseum(id);
        }

        /// <summary>
        /// 获取玩家数据
        /// </summary>
        public ArenaColosseumData.PlayerColosseumData GetPlayerData(int playerId)
        {
            return _seasonSystem.GetPlayerData(playerId);
        }

        /// <summary>
        /// 获取统计数据
        /// </summary>
        public Dictionary<string, object> GetStatistics()
        {
            var stats = _seasonSystem.GetStatistics();
            stats["activeColosseums"] = _activeColosseums.Count;
            return stats;
        }

        /// <summary>
        /// 获取排名
        /// </summary>
        public int GetRank(int playerId)
        {
            return _seasonSystem.GetRank(playerId);
        }

        /// <summary>
        /// 获取排行榜
        /// </summary>
        public List<ArenaColosseumData.PlayerColosseumData> GetRankings(int limit = 100)
        {
            return _seasonSystem.GetRankings(limit);
        }

        /// <summary>
        /// 获取段位
        /// </summary>
        public string GetRankTier(int playerId)
        {
            return _seasonSystem.GetRankTier(playerId);
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 更新倒计时和战斗时间
        /// </summary>
        public void UpdateCountdown(float delta)
        {
            // 更新匹配系统倒计时
            _matchmakingSystem.UpdateCountdown(delta);

            // 更新战斗中的竞技场时间
            foreach (var ac in _activeColosseums)
            {
                if (ac.State == ArenaColosseumData.ColosseumState.InProgress)
                {
                    ac.TimeRemaining -= delta;
                    if (ac.TimeRemaining <= 0)
                    {
                        // 时间到，根据得分判定胜利
                        DetermineWinner(ac);
                    }
                }
            }
        }

        /// <summary>
        /// 检查胜利者
        /// </summary>
        private void CheckWinner(ArenaColosseumData.ActiveColosseum ac)
        {
            var colosseum = _matchmakingSystem.GetColosseum(ac.ColosseumId);
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

            // 大乱斗或1v1：最后存活者获胜
            if (colosseum.Type == ArenaColosseumData.ColosseumType.FreeForAll ||
                colosseum.Type == ArenaColosseumData.ColosseumType.SoloDuel)
            {
                if (aliveCount <= 1)
                {
                    EndColosseum(ac, lastAlive?.PlayerId ?? -1);
                }
            }
            // 团队战
            else
            {
                if (aliveCount <= 1)
                {
                    EndColosseum(ac, lastAlive?.PlayerId ?? -1);
                }
            }
        }

        /// <summary>
        /// 时间到，判定胜利者
        /// </summary>
        private void DetermineWinner(ArenaColosseumData.ActiveColosseum ac)
        {
            var colosseum = _matchmakingSystem.GetColosseum(ac.ColosseumId);
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

        /// <summary>
        /// 结束角斗场
        /// </summary>
        private void EndColosseum(ArenaColosseumData.ActiveColosseum ac, int winnerId)
        {
            var colosseum = _matchmakingSystem.GetColosseum(ac.ColosseumId);
            
            // 使用奖励系统结束角斗场
            _rewardSystem.EndColosseum(ac, winnerId, colosseum, (playerId, isWinner, score, kills, prize) =>
            {
                // 更新玩家数据
                _seasonSystem.UpdatePlayerStats(playerId, isWinner, score, kills, prize);

                // 记录历史
                if (colosseum != null)
                {
                    var record = new ArenaColosseumData.ColosseumRecord
                    {
                        ColosseumId = colosseum.Id,
                        Type = colosseum.Type,
                        IsWinner = isWinner,
                        DamageDealt = score,
                        Kills = kills,
                        PrizeEarned = prize,
                        Timestamp = DateTime.Now
                    };
                    _seasonSystem.AddColosseumRecord(playerId, record);
                }
            });

            // 移除竞技场
            _activeColosseums.Remove(ac);
        }

        #endregion

        #region 存档支持

        /// <summary>
        /// Export save data for persistence (BaseSystem override)
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            // 委托给子系统
            var data = new Dictionary();
            data["seasonSystem"] = _seasonSystem.ExportSaveData();
            return data;
        }

        /// <summary>
        /// Import save data from persistence (BaseSystem override)
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;

            // 从子系统导入数据
            if (data.Contains("seasonSystem"))
            {
                _seasonSystem.ImportSaveData(data["seasonSystem"] as Dictionary);
            }
        }

        #endregion
    }
}
