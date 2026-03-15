using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Systems
{
    /// <summary>
    /// 快速模式匹配队列系统
    /// 支持普通队列和快速队列(quick_queue)优先级匹配
    /// </summary>
    public class MatchmakingQueue : BaseSystem
    {
        public static MatchmakingQueue Instance { get; private set; }

        // 匹配队列条目
        public class QueueEntry
        {
            public int PlayerId;
            public string PlayerName;
            public int Level;
            public string GameMode;        // 游戏模式
            public int PreferredDifficulty; // 偏好难度
            public bool IsQuickQueue;      // 是否快速队列
            public float QueueTime;        // 排队时间
            public float PriorityScore;    // 优先级分数
        }

        // 匹配结果
        public class MatchResult
        {
            public string MatchId;
            public List<QueueEntry> Players;
            public string GameMode;
            public int Difficulty;
            public DateTime CreatedAt;
        }

        // 信号
        public delegate void MatchFoundEvent(MatchResult result);
        public delegate void QueueUpdatedEvent(int queueCount, int quickQueueCount);
        public delegate void MatchStartedEvent(string matchId);
        
        public event MatchFoundEvent OnMatchFound;
        public event QueueUpdatedEvent OnQueueUpdated;
        public event MatchStartedEvent OnMatchStarted;

        // 队列配置
        [Export] public int MinPlayersForMatch = 2;
        [Export] public int MaxPlayersPerMatch = 4;
        [Export] public float QuickQueueThreshold = 30f;  // 快速队列超时时间(秒)
        [Export] public float MatchmakingInterval = 2f;    // 匹配检查间隔(秒)
        [Export] public int LevelDifferenceTolerance = 5;  // 等级差容忍度

        // 队列存储
        private List<QueueEntry> _normalQueue = new List<QueueEntry>();
        private List<QueueEntry> _quickQueue = new List<QueueEntry>();
        private Dictionary<int, QueueEntry> _allEntries = new Dictionary<int, QueueEntry>();
        
        // 当前匹配
        private Dictionary<string, MatchResult> _activeMatches = new Dictionary<string, MatchResult>();
        
        private float _matchmakingTimer = 0f;
        private int _matchIdCounter = 0;

        public override void _Ready()
        {
            Instance = this;
        }

        public override void _Process(float delta)
        {
            _matchmakingTimer += delta;
            
            if (_matchmakingTimer >= MatchmakingInterval)
            {
                _matchmakingTimer = 0f;
                TryMatchmake();
            }
            
            // 更新排队时间
            UpdateQueueTimes(delta);
        }

        #region Queue Management

        /// <summary>
        /// 加入匹配队列
        /// </summary>
        public bool JoinQueue(int playerId, string playerName, int level, string gameMode, int difficulty, bool isQuickQueue = false)
        {
            // 检查是否已在队列中
            if (_allEntries.ContainsKey(playerId))
            {
                GD.PrintErr($"[Matchmaking] Player {playerId} is already in queue");
                return false;
            }

            var entry = new QueueEntry
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Level = level,
                GameMode = gameMode,
                PreferredDifficulty = difficulty,
                IsQuickQueue = isQuickQueue,
                QueueTime = 0f,
                PriorityScore = CalculatePriorityScore(level, isQuickQueue)
            };

            _allEntries[playerId] = entry;

            if (isQuickQueue)
            {
                // 快速队列优先处理
                _quickQueue.Add(entry);
                GD.Print($"[Matchmaking] Player {playerName} (L{level}) joined QUICK queue for {gameMode}");
            }
            else
            {
                _normalQueue.Add(entry);
                GD.Print($"[Matchmaking] Player {playerName} (L{level}) joined normal queue for {gameMode}");
            }

            OnQueueUpdated?.Invoke(_normalQueue.Count + _quickQueue.Count, _quickQueue.Count);
            return true;
        }

        /// <summary>
        /// 离开匹配队列
        /// </summary>
        public bool LeaveQueue(int playerId)
        {
            if (!_allEntries.ContainsKey(playerId))
            {
                return false;
            }

            var entry = _allEntries[playerId];
            _allEntries.Remove(playerId);

            if (entry.IsQuickQueue)
            {
                _quickQueue.Remove(entry);
            }
            else
            {
                _normalQueue.Remove(entry);
            }

            GD.Print($"[Matchmaking] Player {entry.PlayerName} left queue");
            OnQueueUpdated?.Invoke(_normalQueue.Count + _quickQueue.Count, _quickQueue.Count);
            return true;
        }

        /// <summary>
        /// 切换到快速队列
        /// </summary>
        public bool UpgradeToQuickQueue(int playerId)
        {
            if (!_allEntries.ContainsKey(playerId))
            {
                return false;
            }

            var entry = _allEntries[playerId];
            if (entry.IsQuickQueue)
            {
                return true; // 已经在快速队列
            }

            // 从普通队列移除，加入快速队列
            _normalQueue.Remove(entry);
            entry.IsQuickQueue = true;
            entry.PriorityScore = CalculatePriorityScore(entry.Level, true);
            _quickQueue.Add(entry);

            GD.Print($"[Matchmaking] Player {entry.PlayerName} upgraded to QUICK queue");
            OnQueueUpdated?.Invoke(_normalQueue.Count + _quickQueue.Count, _quickQueue.Count);
            return true;
        }

        /// <summary>
        /// 获取队列状态
        /// </summary>
        public Dictionary<string, object> GetQueueStatus()
        {
            return new Dictionary<string, object>
            {
                { "normal_count", _normalQueue.Count },
                { "quick_count", _quickQueue.Count },
                { "total_count", _allEntries.Count },
                { "normal_queue", _normalQueue.ConvertAll(e => new Dictionary<string, object> {
                    { "player_name", e.PlayerName },
                    { "level", e.Level },
                    { "game_mode", e.GameMode },
                    { "queue_time", e.QueueTime }
                })},
                { "quick_queue", _quickQueue.ConvertAll(e => new Dictionary<string, object> {
                    { "player_name", e.PlayerName },
                    { "level", e.Level },
                    { "game_mode", e.GameMode },
                    { "queue_time", e.QueueTime }
                })}
            };
        }

        #endregion

        #region Matchmaking Logic

        /// <summary>
        /// 尝试进行匹配
        /// </summary>
        private void TryMatchmake()
        {
            // 优先处理快速队列
            if (_quickQueue.Count >= MinPlayersForMatch)
            {
                var match = TryCreateMatch(_quickQueue, true);
                if (match != null)
                {
                    CompleteMatch(match);
                    return;
                }
            }

            // 合并队列进行匹配
            var combinedQueue = new List<QueueEntry>();
            combinedQueue.AddRange(_quickQueue);
            combinedQueue.AddRange(_normalQueue);

            if (combinedQueue.Count >= MinPlayersForMatch)
            {
                var match = TryCreateMatch(combinedQueue, false);
                if (match != null)
                {
                    CompleteMatch(match);
                }
            }
        }

        /// <summary>
        /// 尝试创建匹配
        /// </summary>
        private MatchResult TryCreateMatch(List<QueueEntry> queue, bool isQuickMatch)
        {
            // 按优先级分数排序
            queue.Sort((a, b) => b.PriorityScore.CompareTo(a.PriorityScore));

            // 尝试找到兼容的玩家组合
            for (int i = 0; i <= queue.Count - MinPlayersForMatch; i++)
            {
                var matchPlayers = new List<QueueEntry> { queue[i] };
                var baseEntry = queue[i];

                for (int j = i + 1; j < queue.Count && matchPlayers.Count < MaxPlayersPerMatch; j++)
                {
                    var candidate = queue[j];
                    
                    // 检查兼容性
                    if (IsCompatible(baseEntry, candidate))
                    {
                        matchPlayers.Add(candidate);
                    }
                }

                if (matchPlayers.Count >= MinPlayersForMatch)
                {
                    // 创建匹配结果
                    var result = new MatchResult
                    {
                        MatchId = GenerateMatchId(),
                        Players = matchPlayers,
                        GameMode = baseEntry.GameMode,
                        Difficulty = baseEntry.PreferredDifficulty,
                        CreatedAt = DateTime.Now
                    };

                    // 从队列中移除已匹配的玩家
                    foreach (var player in matchPlayers)
                    {
                        _allEntries.Remove(player.PlayerId);
                        _quickQueue.Remove(player);
                        _normalQueue.Remove(player);
                    }

                    OnQueueUpdated?.Invoke(_normalQueue.Count + _quickQueue.Count, _quickQueue.Count);
                    return result;
                }
            }

            return null;
        }

        /// <summary>
        /// 检查两个玩家是否兼容
        /// </summary>
        private bool IsCompatible(QueueEntry a, QueueEntry b)
        {
            // 检查游戏模式
            if (a.GameMode != b.GameMode)
            {
                return false;
            }

            // 检查等级差
            int levelDiff = Math.Abs(a.Level - b.Level);
            if (levelDiff > LevelDifferenceTolerance)
            {
                // 至少一方是快速队列时可以放宽等级限制
                if (!a.IsQuickQueue && !b.IsQuickQueue)
                {
                    return false;
                }
            }

            // 检查难度偏好
            int diffDiff = Math.Abs(a.PreferredDifficulty - b.PreferredDifficulty);
            if (diffDiff > 1)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// 完成匹配
        /// </summary>
        private void CompleteMatch(MatchResult match)
        {
            _activeMatches[match.MatchId] = match;
            
            GD.Print($"[Matchmaking] Match found: {match.MatchId} with {match.Players.Count} players");
            
            // 按等级排序玩家
            match.Players.Sort((a, b) => b.Level.CompareTo(a.Level));
            
            // 通知匹配成功
            OnMatchFound?.Invoke(match);
            
            // 创建游戏会话
            CreateGameSession(match);
        }

        /// <summary>
        /// 创建游戏会话
        /// </summary>
        private void CreateGameSession(MatchResult match)
        {
            // 通知PartySystem创建队伍
            if (PartySystem.Instance != null && match.Players != null && match.Players.Count > 0)
            {
                // 第一个玩家创建队伍
                var leader = match.Players[0];
                PartySystem.Instance.CreateParty(leader.PlayerId);
                
                // 其他玩家加入
                for (int i = 1; i < match.Players.Count; i++)
                {
                    var player = match.Players[i];
                    // 通知服务器让玩家加入队伍
                    // PartySystem.Instance.JoinParty(...)
                }
            }

            // 通知游戏开始
            OnMatchStarted?.Invoke(match.MatchId);
        }

        #endregion

        #region Helper Methods

        /// <summary>
        /// 计算优先级分数
        /// </summary>
        private float CalculatePriorityScore(int level, bool isQuickQueue)
        {
            float baseScore = level * 0.1f;
            
            // 快速队列获得额外优先级
            if (isQuickQueue)
            {
                baseScore += 100f;
            }
            
            return baseScore;
        }

        /// <summary>
        /// 更新排队时间
        /// </summary>
        private void UpdateQueueTimes(float delta)
        {
            foreach (var entry in _allEntries.Values)
            {
                entry.QueueTime += delta;
                
                // 排队超时自动升级到快速队列
                if (!entry.IsQuickQueue && entry.QueueTime >= QuickQueueThreshold)
                {
                    UpgradeToQuickQueue(entry.PlayerId);
                }
            }
        }

        /// <summary>
        /// 生成匹配ID
        /// </summary>
        private string GenerateMatchId()
        {
            _matchIdCounter++;
            return "match_" + DateTimeOffset.Now.ToUnixTimeMilliseconds() + "_" + _matchIdCounter;
        }

        /// <summary>
        /// 获取活动匹配数
        /// </summary>
        public int GetActiveMatchCount()
        {
            return _activeMatches.Count;
        }

        /// <summary>
        /// 取消匹配
        /// </summary>
        public void CancelMatch(string matchId)
        {
            if (_activeMatches.ContainsKey(matchId))
            {
                var match = _activeMatches[matchId];
                
                // 让玩家重新加入队列
                foreach (var player in match.Players)
                {
                    JoinQueue(player.PlayerId, player.PlayerName, player.Level, 
                              player.GameMode, player.PreferredDifficulty, player.IsQuickQueue);
                }
                
                _activeMatches.Remove(matchId);
                GD.Print($"[Matchmaking] Match {matchId} cancelled");
            }
        }

        public override void _ExitTree()
        {
            Instance = null;
        }
    }
}
