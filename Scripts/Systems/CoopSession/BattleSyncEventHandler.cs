using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗同步系统 - 事件处理模块
    /// 处理状态管理、同步处理、配置与存档
    /// </summary>
    public partial class BattleSyncEventHandler : BaseSystem
    {
        private static BattleSyncEventHandler _instance;
        public static BattleSyncEventHandler Instance => _instance ??= new BattleSyncEventHandler();

        // 线程安全锁
        protected readonly object _lock = new object();

        // 同步配置
        protected BattleSyncData.BattleSyncConfig _config;

        // 当前会话ID
        protected string _currentSessionId = "";

        // 状态引用（由主系统注入）
        protected Dictionary<int, BattleSyncData.PlayerBattleState> _playerStates;
        protected Dictionary<int, BattleSyncData.EnemyBattleState> _enemyStates;

        // 待广播的战斗操作
        protected Queue<BattleSyncData.BattleAction> _pendingActions;
        protected Queue<BattleSyncData.BattleAction> _broadcastBuffer;

        // 定时器
        protected float _stateSyncTimer = 0f;
        protected float _actionBroadcastTimer = 0f;

        // 延迟追踪
        protected long _lastSyncTime = 0;
        protected float _avgSyncLatency = 0f;

        #region 状态注入

        /// <summary>
        /// 设置状态引用（由主系统调用）
        /// </summary>
        public void SetStateReferences(
            string sessionId,
            Dictionary<int, BattleSyncData.PlayerBattleState> playerStates,
            Dictionary<int, BattleSyncData.EnemyBattleState> enemyStates,
            Queue<BattleSyncData.BattleAction> pendingActions,
            Queue<BattleSyncData.BattleAction> broadcastBuffer,
            BattleSyncData.BattleSyncConfig config,
            long lastSyncTime,
            float avgSyncLatency)
        {
            _currentSessionId = sessionId;
            _playerStates = playerStates;
            _enemyStates = enemyStates;
            _pendingActions = pendingActions;
            _broadcastBuffer = broadcastBuffer;
            _config = config;
            _lastSyncTime = lastSyncTime;
            _avgSyncLatency = avgSyncLatency;
        }

        /// <summary>
        /// 获取最后同步时间（用于外部访问）
        /// </summary>
        public long GetLastSyncTime() => _lastSyncTime;

        /// <summary>
        /// 获取平均同步延迟（用于外部访问）
        /// </summary>
        public float GetAvgSyncLatency() => _avgSyncLatency;

        #endregion

        #region 信号定义
public delegate void PlayerStateUpdatedEventHandler(int playerId, BattleSyncData.PlayerBattleState state);
public delegate void PlayerHealthChangedEventHandler(int playerId, float currentHealth, float maxHealth, float change);
public delegate void PlayerManaChangedEventHandler(int playerId, float currentMana, float maxMana, float change);
public delegate void EnemyStateUpdatedEventHandler(int enemyId, BattleSyncData.EnemyBattleState state);
public delegate void BattleSnapshotReceivedEventHandler(BattleSyncData.BattleSnapshot snapshot);
public delegate void SyncLatencyWarningEventHandler(float latencyMs);
public delegate void BattleActionReceivedEventHandler(BattleSyncData.BattleAction action);

        #endregion

        #region 状态管理

        /// <summary>
        /// 更新玩家位置
        /// </summary>
        public virtual void UpdatePlayerPosition(int playerId, float x, float y)
        {
            lock (_lock)
            {
                if (_playerStates.TryGetValue(playerId, out var state))
                {
                    state.PositionX = x;
                    state.PositionY = y;
                    state.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }
        }

        /// <summary>
        /// 更新玩家属性
        /// </summary>
        public virtual void UpdatePlayerStats(int playerId, float health, float mana)
        {
            lock (_lock)
            {
                if (_playerStates.TryGetValue(playerId, out var state))
                {
                    float oldHealth = state.Health;
                    float oldMana = state.Mana;
                    
                    state.Health = Math.Clamp(health, 0, state.MaxHealth);
                    state.Mana = Math.Clamp(mana, 0, state.MaxMana);
                    state.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                    if (Math.Abs(oldHealth - state.Health) > 0.01f)
                    {
                        EmitSignal(SignalName.PlayerHealthChanged, playerId, state.Health, state.MaxHealth, state.Health - oldHealth);
                    }
                    if (Math.Abs(oldMana - state.Mana) > 0.01f)
                    {
                        EmitSignal(SignalName.PlayerManaChanged, playerId, state.Mana, state.MaxMana, state.Mana - oldMana);
                    }
                }
            }
        }

        /// <summary>
        /// 获取玩家状态
        /// </summary>
        public virtual BattleSyncData.PlayerBattleState? GetPlayerState(int playerId)
        {
            lock (_lock)
            {
                return _playerStates.TryGetValue(playerId, out var state) ? state : null;
            }
        }

        /// <summary>
        /// 获取所有玩家状态
        /// </summary>
        public virtual List<BattleSyncData.PlayerBattleState> GetAllPlayerStates()
        {
            lock (_lock)
            {
                return _playerStates.Values.ToList();
            }
        }

        /// <summary>
        /// 获取敌人状态
        /// </summary>
        public virtual BattleSyncData.EnemyBattleState? GetEnemyState(int enemyId)
        {
            lock (_lock)
            {
                return _enemyStates.TryGetValue(enemyId, out var state) ? state : null;
            }
        }

        /// <summary>
        /// 获取所有敌人状态
        /// </summary>
        public virtual List<BattleSyncData.EnemyBattleState> GetAllEnemyStates()
        {
            lock (_lock)
            {
                return _enemyStates.Values.ToList();
            }
        }

        /// <summary>
        /// 创建战斗快照
        /// </summary>
        public virtual BattleSyncData.BattleSnapshot CreateSnapshot()
        {
            lock (_lock)
            {
                var snapshot = new BattleSyncData.BattleSnapshot
                {
                    SessionId = _currentSessionId,
                    Players = _playerStates.Values.ToList(),
                    Enemies = _enemyStates.Values.ToList()
                };
                return snapshot;
            }
        }

        /// <summary>
        /// 应用战斗快照
        /// </summary>
        public virtual void ApplySnapshot(BattleSyncData.BattleSnapshot snapshot)
        {
            lock (_lock)
            {
                _playerStates.Clear();
                _enemyStates.Clear();

                foreach (var player in snapshot.Players)
                {
                    _playerStates[player.PlayerId] = player;
                    EmitSignal(SignalName.PlayerStateUpdated, player.PlayerId, player);
                }

                foreach (var enemy in snapshot.Enemies)
                {
                    _enemyStates[enemy.EnemyId] = enemy;
                    EmitSignal(SignalName.EnemyStateUpdated, enemy.EnemyId, enemy);
                }

                EmitSignal(SignalName.BattleSnapshotReceived, snapshot);
                GD.Print($"[BattleSyncEventHandler] Snapshot applied: {snapshot.Players.Count} players, {snapshot.Enemies.Count} enemies");
            }
        }

        #endregion

        #region 同步处理

        /// <summary>
        /// 处理状态同步
        /// </summary>
        protected virtual void ProcessStateSync()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _lastSyncTime = now;
        }

        /// <summary>
        /// 处理战斗操作广播
        /// </summary>
        protected virtual void ProcessActionBroadcast()
        {
            lock (_lock)
            {
                if (_pendingActions.Count == 0) return;

                while (_pendingActions.Count > 0)
                {
                    var action = _pendingActions.Dequeue();
                    _broadcastBuffer.Enqueue(action);
                    EmitSignal(SignalName.BattleActionReceived, action);
                }
            }

            // 通过网络层广播
            BroadcastActionsToNetwork();
        }

        /// <summary>
        /// 广播操作到网络
        /// </summary>
        protected virtual void BroadcastActionsToNetwork()
        {
            if (_broadcastBuffer.Count == 0) return;

            if (MultiplayerManager.Instance == null || !MultiplayerManager.Instance.IsInRoom)
                return;

            var actions = new ArrayList();
            while (_broadcastBuffer.Count > 0)
            {
                var action = _broadcastBuffer.Dequeue();
                actions.Add(new Dictionary<string, object>
                {
                    { "actionId", action.ActionId },
                    { "playerId", action.PlayerId },
                    { "playerName", action.PlayerName },
                    { "type", action.Type.ToString() },
                    { "skillId", action.SkillId },
                    { "value", action.Value },
                    { "targetX", action.TargetX },
                    { "targetY", action.TargetY },
                    { "targetId", action.TargetId },
                    { "isCritical", action.IsCritical },
                    { "timestamp", action.Timestamp }
                });
            }

            var message = new Dictionary<string, object>
            {
                { "type", "battle_action" },
                { "room_id", MultiplayerManager.Instance.GetRoomInfo()?.RoomId ?? "" },
                { "actions", actions }
            };

            NetworkClient.Instance.SendJson(message);
            GD.Print($"[BattleSyncEventHandler] Broadcasted {actions.Count} actions to network");
        }

        /// <summary>
        /// 更新Buff持续时间
        /// </summary>
        public virtual void UpdateBuffDurations(float delta)
        {
            lock (_lock)
            {
                foreach (var playerState in _playerStates.Values)
                {
                    var expiredBuffs = new List<BattleSyncData.BuffState>();

                    foreach (var buff in playerState.ActiveBuffs)
                    {
                        buff.Duration -= delta;
                        if (buff.Duration <= 0)
                        {
                            expiredBuffs.Add(buff);
                        }
                    }

                    foreach (var buff in expiredBuffs)
                    {
                        playerState.ActiveBuffs.Remove(buff);
                        EmitSignal(SignalName.BuffRemoved, playerState.PlayerId, buff.BuffId);
                    }

                    if (expiredBuffs.Count > 0)
                    {
                        EmitSignal(SignalName.PlayerStateUpdated, playerState.PlayerId, playerState);
                    }
                }
            }
        }

        /// <summary>
        /// 检查同步延迟
        /// </summary>
        protected virtual void CheckSyncLatency()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long diff = now - _lastSyncTime;
            
            _avgSyncLatency = _avgSyncLatency * 0.9f + diff * 0.1f;

            if (_avgSyncLatency > _config.TargetLatencyMs)
            {
                EmitSignal(SignalName.SyncLatencyWarning, _avgSyncLatency);
            }
        }

        #endregion

        #region 配置

        /// <summary>
        /// 设置配置
        /// </summary>
        public virtual void SetConfig(BattleSyncData.BattleSyncConfig config)
        {
            _config = config;
            GD.Print($"[BattleSyncEventHandler] Config updated: sync rate={config.StateSyncRate}Hz, target latency={config.TargetLatencyMs}ms");
        }

        #endregion

        #region 存档支持

        public override Dictionary<string, object> ExportSaveData()
        {
            lock (_lock)
            {
                var data = new Dictionary<string, object>();
                data["current_session_id"] = _currentSessionId;
                
                var playerStatesList = new Godot.Collections.Array();
                foreach (var kvp in _playerStates)
                {
                    var state = kvp.Value;
                    playerStatesList.Add(new Dictionary
                    {
                        { "player_id", state.PlayerId },
                        { "player_name", state.PlayerName },
                        { "health", state.Health },
                        { "max_health", state.MaxHealth },
                        { "mana", state.Mana },
                        { "max_mana", state.MaxMana },
                        { "is_dead", state.IsDead }
                    });
                }
                data["player_states"] = playerStatesList;
                
                return data;
            }
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            lock (_lock)
            {
                if (data.ContainsKey("current_session_id"))
                {
                    _currentSessionId = data["current_session_id"]?.ToString() ?? "";
                }

                if (data.ContainsKey("player_states") && data["player_states"] is Array playerStatesList)
                {
                    _playerStates.Clear();
                    foreach (Dictionary playerData in playerStatesList)
                    {
                        var state = new BattleSyncData.PlayerBattleState
                        {
                            PlayerId = Convert.ToInt32(playerData["player_id"]),
                            PlayerName = playerData["player_name"]?.ToString() ?? "",
                            Health = Convert.ToSingle(playerData["health"]),
                            MaxHealth = Convert.ToSingle(playerData["max_health"]),
                            Mana = Convert.ToSingle(playerData["mana"]),
                            MaxMana = Convert.ToSingle(playerData["max_mana"]),
                            IsDead = Convert.ToBoolean(playerData["is_dead"])
                        };
                        _playerStates[state.PlayerId] = state;
                    }
                }
            }
        }

        #endregion
    }
}
