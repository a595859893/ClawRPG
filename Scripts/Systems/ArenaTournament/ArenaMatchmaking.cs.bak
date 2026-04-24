using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.ArenaTournament {
    /// <summary>
    /// 竞技场匹配系统 - 处理玩家匹配逻辑
    /// </summary>
    public partial class ArenaMatchmaking : BaseSystem {
        
        /// <summary>
        /// 匹配状态
        /// </summary>
        public enum MatchState {
            Waiting,
            Matching,
            Matched,
            Cancelled
        }
        
        private Dictionary<int, MatchState> _playerMatchStates = new();
        private List<int> _waitingPlayers = new();
        private Dictionary<int, int> _playerRatings = new();
        private float _matchmakingInterval = 1.0f;
        private float _timer = 0f;
        
        public override void _Ready() {
            base._Ready();
        }
        
        /// <summary>
        /// 添加玩家到匹配队列
        /// </summary>
        public void AddToQueue(int playerId, int rating) {
            if (!_waitingPlayers.Contains(playerId)) {
                _waitingPlayers.Add(playerId);
                _playerMatchStates[playerId] = MatchState.Waiting;
                _playerRatings[playerId] = rating;
            }
        }
        
        /// <summary>
        /// 从匹配队列移除
        /// </summary>
        public void RemoveFromQueue(int playerId) {
            _waitingPlayers.Remove(playerId);
            _playerMatchStates[playerId] = MatchState.Cancelled;
        }
        
        /// <summary>
        /// 执行匹配
        /// </summary>
        public (int, int) FindMatch(int playerId) {
            var playerRating = _playerRatings.GetValueOrDefault(playerId, 1000);
            
            foreach (var otherId in _waitingPlayers) {
                if (otherId == playerId) continue;
                
                var otherRating = _playerRatings.GetValueOrDefault(otherId, 1000);
                var ratingDiff = Math.Abs(playerRating - otherRating);
                
                // 匹配阈值
                if (ratingDiff <= 200) {
                    RemoveFromQueue(playerId);
                    RemoveFromQueue(otherId);
                    _playerMatchStates[playerId] = MatchState.Matched;
                    _playerMatchStates[otherId] = MatchState.Matched;
                    
                    return (playerId, otherId);
                }
            }
            
            return (-1, -1);
        }
        
        /// <summary>
        /// 获取等待玩家数量
        /// </summary>
        public int GetWaitingCount() {
            return _waitingPlayers.Count;
        }
        
        /// <summary>
        /// 获取玩家匹配状态
        /// </summary>
        public MatchState GetPlayerMatchState(int playerId) {
            return _playerMatchStates.GetValueOrDefault(playerId, MatchState.Cancelled);
        }
        
        public override void _Process(double delta) {
            base._Process(delta);
            
            _timer += delta;
            if (_timer >= _matchmakingInterval) {
                _timer = 0f;
                // 定时匹配逻辑
            }
        }
        
        public override Dictionary<string, object> ExportSaveData() {
            var data = new Dictionary<string, object>();
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data) {
            // 加载数据
        }
    }
}
