using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.CoopSession {
    /// <summary>
    /// 战斗状态同步 - 负责同步玩家和敌人的战斗状态
    /// </summary>
    public partial class BattleStateSync : BaseSystem {
        
        private Dictionary<int, Dictionary> _playerStates = new();
        private Dictionary<int, Dictionary> _enemyStates = new();
        private float _syncInterval = 0.1f;
        private float _syncTimer = 0f;
        
        public override void _Ready() {
            base._Ready();
        }
        
        /// <summary>
        /// 同步玩家状态
        /// </summary>
        public void SyncPlayerState(int playerId, Dictionary state) {
            _playerStates[playerId] = state;
        }
        
        /// <summary>
        /// 同步敌人状态
        /// </summary>
        public void SyncEnemyState(int enemyId, Dictionary state) {
            _enemyStates[enemyId] = state;
        }
        
        /// <summary>
        /// 获取玩家状态
        /// </summary>
        public Dictionary GetPlayerState(int playerId) {
            if (_playerStates.TryGetValue(playerId, out var state)) {
                return state;
            }
            return null;
        }
        
        /// <summary>
        /// 获取敌人状态
        /// </summary>
        public Dictionary GetEnemyState(int enemyId) {
            if (_enemyStates.TryGetValue(enemyId, out var state)) {
                return state;
            }
            return null;
        }
        
        /// <summary>
        /// 批量同步状态
        /// </summary>
        public Dictionary BatchSyncStates() {
            var allStates = new Dictionary {
                { "players", new Dictionary(_playerStates) },
                { "enemies", new Dictionary(_enemyStates) }
            };
            return allStates;
        }
        
        /// <summary>
        /// 设置同步间隔
        /// </summary>
        public void SetSyncInterval(float interval) {
            _syncInterval = Mathf.Max(0.01f, interval);
        }
        
        public override void _Process(double delta) {
            base._Process(delta);
            
            _syncTimer += delta;
            if (_syncTimer >= _syncInterval) {
                _syncTimer = 0f;
                // 定期同步逻辑
            }
        }
        
        public override Dictionary<string, object> ExportSaveData() {
            var data = new Dictionary<string, object>();
            data["syncInterval"] = _syncInterval;
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data) {
            if (data.ContainsKey("syncInterval")) {
                _syncInterval = (float)data["syncInterval"];
            }
        }
    }
}
