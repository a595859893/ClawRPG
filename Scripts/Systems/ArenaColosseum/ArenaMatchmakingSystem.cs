using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 竞技场匹配系统 - 处理玩家匹配、加入、离开
    /// </summary>
    public partial class ArenaMatchmakingSystem : BaseSystem
    {
        private static ArenaMatchmakingSystem _instance;
        public static ArenaMatchmakingSystem Instance
        {
            get
            {
                if (_instance == null)
                    _instance = new ArenaMatchmakingSystem();
                return _instance;
            }
            private set { _instance = value; }
        }

        protected override string SystemName => "ArenaMatchmakingSystem";

        // 信号系统 - 匹配相关
        public event Action<ArenaColosseumData.ActiveColosseum, ArenaColosseumData.Participant> OnPlayerJoined;
        public event Action<ArenaColosseumData.ActiveColosseum, ArenaColosseumData.Participant> OnPlayerLeft;
        public event Action<ArenaColosseumData.ActiveColosseum, ArenaColosseumData.Participant> OnMatchFound;
        public event Action<ArenaColosseumData.ActiveColosseum> OnCountdownStarted;
        public event Action<ArenaColosseumData.ActiveColosseum> OnCountdownEnded;
        public event Action<ArenaColosseumData.ActiveColosseum> OnColosseumStarted;

        // 数据
        private List<ArenaColosseumData.Colosseum> _colosseums;
        private List<ArenaColosseumData.ActiveColosseum> _activeColosseums;
        private int _nextInstanceId = 1;

        public ArenaMatchmakingSystem()
        {
            _colosseums = ArenaColosseumDatabase.GetDefaultColosseums();
            _activeColosseums = new List<ArenaColosseumData.ActiveColosseum>();
        }

        protected override void Initialize()
        {
            base.Initialize();
            GD.Print("[ArenaMatchmakingSystem] Initialized");
        }

        #region 公开接口

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
        /// 加入角斗场
        /// </summary>
        public bool JoinColosseum(int playerId, int colosseumId, string playerName, int level,
            int health, int damage, int wins, int losses)
        {
            var colosseum = GetColosseum(colosseumId);
            if (colosseum == null)
            {
                GD.PrintErr($"[ArenaMatchmakingSystem] Colosseum {colosseumId} not found");
                return false;
            }

            if (level < colosseum.MinLevel)
            {
                GD.PrintErr($"[ArenaMatchmakingSystem] Player level {level} too low, required {colosseum.MinLevel}");
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
            for (int i = _activeColosseums.Count - 1; i >= 0; i--)
            {
                var activeColosseum = _activeColosseums[i];
                var participant = GetParticipant(activeColosseum, playerId);
                if (participant != null)
                {
                    activeColosseum.Participants.Remove(participant);
                    OnPlayerLeft?.Invoke(activeColosseum, participant);

                    if (activeColosseum.Participants.Count == 0)
                    {
                        activeColosseum.State = ArenaColosseumData.ColosseumState.Cancelled;
                        _activeColosseums.RemoveAt(i);
                    }
                    else if (activeColosseum.State == ArenaColosseumData.ColosseumState.InProgress)
                    {
                        // 剩余玩家自动获胜 - 通知协调者处理
                        EndColosseumWithRemainingPlayers(activeColosseum);
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
        /// 更新倒计时和战斗时间
        /// </summary>
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
            }
        }

        /// <summary>
        /// 检查并移除已完成的竞技场
        /// </summary>
        public void RemoveCompletedColosseum(ArenaColosseumData.ActiveColosseum ac)
        {
            if (ac.State == ArenaColosseumData.ColosseumState.Completed ||
                ac.State == ArenaColosseumData.ColosseumState.Cancelled)
            {
                _activeColosseums.Remove(ac);
            }
        }

        #endregion

        #region 内部方法

        /// <summary>
        /// 查找等待中的竞技场
        /// </summary>
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

        /// <summary>
        /// 创建新的竞技场实例
        /// </summary>
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

        /// <summary>
        /// 获取参与者
        /// </summary>
        public ArenaColosseumData.Participant GetParticipant(ArenaColosseumData.ActiveColosseum ac, int playerId)
        {
            foreach (var p in ac.Participants)
            {
                if (p.PlayerId == playerId) return p;
            }
            return null;
        }

        /// <summary>
        /// 开始匹配
        /// </summary>
        private void StartMatching(ArenaColosseumData.ActiveColosseum ac)
        {
            ac.State = ArenaColosseumData.ColosseumState.Matching;

            // 通知所有参与者匹配成功
            foreach (var p in ac.Participants)
            {
                OnMatchFound?.Invoke(ac, p);
            }

            // 开始倒计时
            StartCountdown(ac);
        }

        /// <summary>
        /// 开始倒计时
        /// </summary>
        private void StartCountdown(ArenaColosseumData.ActiveColosseum ac)
        {
            ac.State = ArenaColosseumData.ColosseumState.Countdown;
            OnCountdownStarted?.Invoke(ac);
        }

        /// <summary>
        /// 开始战斗
        /// </summary>
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

        /// <summary>
        /// 检查是否全部准备就绪
        /// </summary>
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

        /// <summary>
        /// 剩余玩家时的结束处理（委托给协调者）
        /// </summary>
        private void EndColosseumWithRemainingPlayers(ArenaColosseumData.ActiveColosseum ac)
        {
            // 寻找剩余玩家中得分最高的作为胜者
            ArenaColosseumData.Participant winner = null;
            int highestScore = -1;

            foreach (var p in ac.Participants)
            {
                if (p.IsAlive && p.Score > highestScore)
                {
                    highestScore = p.Score;
                    winner = p;
                }
            }

            ac.WinnerId = winner?.PlayerId ?? -1;
            ac.State = ArenaColosseumData.ColosseumState.Completed;
        }

        #endregion

        #region 存档支持

        public override Dictionary<string, object> ExportSaveData()
        {
            // 匹配系统不需要持久化运行时状态
            return new Dictionary<string, object>();
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            // 匹配系统不需要持久化运行时状态
        }

        #endregion
    }
}
