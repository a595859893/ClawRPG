using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗同步系统 - 多人实时战斗同步主系统
    /// 整合 CoreSystem, Combat, Network, State 四个子模块
    /// </summary>
    public class BattleSyncSystem : BaseSystem
    {
        private static BattleSyncSystem _instance;
        public static BattleSyncSystem Instance => _instance ??= new BattleSyncSystem();

        // 子系统实例
        private BattleSyncCoreSystem _coreSystem;
        private BattleSyncCombat _combatSystem;
        private BattleSyncNetwork _networkSystem;
        private BattleSyncState _stateSystem;

        // 线程安全锁
        private readonly object _lock = new object();

        // 同步配置
        private BattleSyncData.BattleSyncConfig _config;

        // 当前会话ID
        private string _currentSessionId = "";

        // 玩家战斗状态
        private Dictionary<int, BattleSyncData.PlayerBattleState> _playerStates;
        
        // 敌人战斗状态
        private Dictionary<int, BattleSyncData.EnemyBattleState> _enemyStates;

        // 待广播的战斗操作
        private Queue<BattleSyncData.BattleAction> _pendingActions;
        private Queue<BattleSyncData.BattleAction> _broadcastBuffer;

        // 定时器
        private float _stateSyncTimer = 0f;
        private float _actionBroadcastTimer = 0f;

        // 延迟追踪
        private long _lastSyncTime = 0;
        private float _avgSyncLatency = 0f;

        #region 信号定义 (Godot 4.x) - 委托给CoreSystem

        /// <summary>
        /// 战斗操作已广播（本地或远程）
        /// </summary>
        [Signal]
        public delegate void BattleActionReceivedEventHandler(BattleSyncData.BattleAction action);

        /// <summary>
        /// 玩家状态更新
        /// </summary>
        [Signal]
        public delegate void PlayerStateUpdatedEventHandler(int playerId, BattleSyncData.PlayerBattleState state);

        /// <summary>
        /// 玩家生命值变化
        /// </summary>
        [Signal]
        public delegate void PlayerHealthChangedEventHandler(int playerId, float currentHealth, float maxHealth, float change);

        /// <summary>
        /// 玩家魔法值变化
        /// </summary>
        [Signal]
        public delegate void PlayerManaChangedEventHandler(int playerId, float currentMana, float maxMana, float change);

        /// <summary>
        /// Buff已应用
        /// </summary>
        [Signal]
        public delegate void BuffAppliedEventHandler(int playerId, BattleSyncData.BuffState buff);

        /// <summary>
        /// Buff已移除
        /// </summary>
        [Signal]
        public delegate void BuffRemovedEventHandler(int playerId, string buffId);

        /// <summary>
        /// 敌人状态更新
        /// </summary>
        [Signal]
        public delegate void EnemyStateUpdatedEventHandler(int enemyId, BattleSyncData.EnemyBattleState state);

        /// <summary>
        /// 敌人死亡
        /// </summary>
        [Signal]
        public delegate void EnemyKilledEventHandler(int enemyId, int killerId);

        /// <summary>
        /// 玩家死亡
        /// </summary>
        [Signal]
        public delegate void PlayerDiedEventHandler(int playerId);

        /// <summary>
        /// 玩家复活
        /// </summary>
        [Signal]
        public delegate void PlayerRevivedEventHandler(int playerId);

        /// <summary>
        /// 同步延迟警告
        /// </summary>
        [Signal]
        public delegate void SyncLatencyWarningEventHandler(float latencyMs);

        /// <summary>
        /// 战斗快照同步（用于新玩家加入或全量同步）
        /// </summary>
        [Signal]
        public delegate void BattleSnapshotReceivedEventHandler(BattleSyncData.BattleSnapshot snapshot);

        /// <summary>
        /// 仇恨转移（用于配合玩法：吸引仇恨）
        /// </summary>
        [Signal]
        public delegate void AggroChangedEventHandler(int enemyId, int oldTargetId, int newTargetId);

        #endregion

        #region 属性

        public string CurrentSessionId => _currentSessionId;
        public float AverageSyncLatency => _avgSyncLatency;
        public bool IsInBattle => !string.IsNullOrEmpty(_currentSessionId);

        /// <summary>
        /// 同步延迟是否在目标范围内 (< 100ms)
        /// </summary>
        public bool IsSyncHealthy => _avgSyncLatency < _config.TargetLatencyMs;

        #endregion

        #region 生命周期

        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            
            // 初始化子系统
            InitializeSubsystems();
            
            // 初始化数据结构
            _playerStates = new Dictionary<int, BattleSyncData.PlayerBattleState>();
            _enemyStates = new Dictionary<int, BattleSyncData.EnemyBattleState>();
            _pendingActions = new Queue<BattleSyncData.BattleAction>();
            _broadcastBuffer = new Queue<BattleSyncData.BattleAction>();
            
            // 默认配置
            _config = new BattleSyncData.BattleSyncConfig();
            
            // 设置网络回调
            _networkSystem.OnRemoteActionReceived += HandleRemoteAction;
            
            GD.Print("[BattleSyncSystem] Initialized with subsystems");
        }

        /// <summary>
        /// 初始化子系统
        /// </summary>
        private void InitializeSubsystems()
        {
            _coreSystem = new BattleSyncCoreSystem();
            _combatSystem = new BattleSyncCombat();
            _networkSystem = new BattleSyncNetwork();
            _stateSystem = new BattleSyncState();
            
            // 初始化战斗系统队列
            _combatSystem.InitializeCombatQueues();
            
            // 初始化网络组件
            _networkSystem.InitializeNetworkComponents();
            
            GD.Print("[BattleSyncSystem] Subsystems initialized");
        }

        protected override void Initialize()
        {
            IsInitialized = true;
            GD.Print("[BattleSyncSystem] System initialized");
        }

        public override void _Process(float delta)
        {
            if (!IsInitialized || string.IsNullOrEmpty(_currentSessionId))
                return;

            // 更新战斗状态同步
            _stateSyncTimer += delta;
            if (_stateSyncTimer >= 1.0f / _config.StateSyncRate)
            {
                _stateSyncTimer = 0;
                ProcessStateSync();
            }

            // 更新战斗操作广播
            _actionBroadcastTimer += delta;
            if (_actionBroadcastTimer >= 1.0f / _config.ActionBroadcastRate)
            {
                _actionBroadcastTimer = 0;
                ProcessActionBroadcast();
            }

            // 更新Buff持续时间
            UpdateBuffDurations(delta);

            // 检查同步延迟
            CheckSyncLatency();
        }

        #endregion

        #region 会话管理

        /// <summary>
        /// 开始战斗同步会话
        /// </summary>
        public void StartBattleSession(string sessionId, List<CoopPlayerData> players)
        {
            lock (_lock)
            {
                _currentSessionId = sessionId;
                _playerStates.Clear();
                _enemyStates.Clear();
                _pendingActions.Clear();
                _broadcastBuffer.Clear();

                // 初始化玩家状态
                foreach (var player in players)
                {
                    _playerStates[player.PlayerId] = new BattleSyncData.PlayerBattleState
                    {
                        PlayerId = player.PlayerId,
                        PlayerName = player.PlayerName,
                        Health = player.HealthPercent * 100,
                        MaxHealth = 100,
                        Mana = 100,
                        MaxMana = 100,
                        PositionX = player.PositionX,
                        PositionY = player.PositionY,
                        IsDead = player.State == CoopPlayerState.Dead
                    };
                }

                _lastSyncTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                GD.Print($"[BattleSyncSystem] Battle session started: {sessionId}, players: {players.Count}");
            }
        }

        /// <summary>
        /// 结束战斗同步会话
        /// </summary>
        public void EndBattleSession()
        {
            lock (_lock)
            {
                GD.Print($"[BattleSyncSystem] Battle session ended: {_currentSessionId}");
                _currentSessionId = "";
                _playerStates.Clear();
                _enemyStates.Clear();
                _pendingActions.Clear();
                _broadcastBuffer.Clear();
            }
        }

        /// <summary>
        /// 添加玩家到战斗
        /// </summary>
        public void AddPlayer(int playerId, string playerName, float maxHealth = 100, float maxMana = 100)
        {
            lock (_lock)
            {
                _playerStates[playerId] = new BattleSyncData.PlayerBattleState
                {
                    PlayerId = playerId,
                    PlayerName = playerName,
                    Health = maxHealth,
                    MaxHealth = maxHealth,
                    Mana = maxMana,
                    MaxMana = maxMana,
                    IsDead = false
                };
                GD.Print($"[BattleSyncSystem] Player added to battle: {playerName} (ID: {playerId})");
            }
        }

        /// <summary>
        /// 移除玩家从战斗
        /// </summary>
        public void RemovePlayer(int playerId)
        {
            lock (_lock)
            {
                if (_playerStates.Remove(playerId))
                {
                    GD.Print($"[BattleSyncSystem] Player removed from battle: {playerId}");
                }
            }
        }

        #endregion

        #region 战斗操作

        /// <summary>
        /// 记录战斗操作（本地）
        /// </summary>
        public BattleSyncData.BattleAction RecordAction(
            int playerId, 
            string playerName,
            BattleActionType type,
            float value = 0,
            string skillId = "",
            int targetId = -1,
            float targetX = 0,
            float targetY = 0,
            bool isCritical = false)
        {
            var action = new BattleSyncData.BattleAction
            {
                PlayerId = playerId,
                PlayerName = playerName,
                Type = type,
                Value = value,
                SkillId = skillId,
                TargetId = targetId,
                TargetX = targetX,
                TargetY = targetY,
                IsCritical = isCritical
            };

            lock (_lock)
            {
                _pendingActions.Enqueue(action);
                
                // 限制队列大小
                while (_pendingActions.Count > _config.ActionBufferSize)
                {
                    _pendingActions.Dequeue();
                }
            }

            // 应用操作效果
            ApplyActionEffect(action);

            return action;
        }

        /// <summary>
        /// 接收远程战斗操作（由网络层调用）
        /// </summary>
        public void ReceiveRemoteAction(BattleSyncData.BattleAction action)
        {
            // 应用操作效果
            ApplyActionEffect(action);

            // 发出信号
            EmitSignal(SignalName.BattleActionReceived, action);
        }

        /// <summary>
        /// 处理远程操作（网络回调）
        /// </summary>
        private void HandleRemoteAction(BattleSyncData.BattleAction action)
        {
            ReceiveRemoteAction(action);
        }

        /// <summary>
        /// 应用战斗操作效果
        /// </summary>
        private void ApplyActionEffect(BattleSyncData.BattleAction action)
        {
            lock (_lock)
            {
                switch (action.Type)
                {
                    case BattleActionType.Attack:
                    case BattleActionType.Skill:
                        ApplyDamage(action);
                        break;

                    case BattleActionType.Heal:
                        ApplyHealing(action);
                        break;

                    case BattleActionType.Damage:
                        ApplyDamage(action);
                        break;

                    case BattleActionType.BuffApply:
                        ApplyBuff(action);
                        break;

                    case BattleActionType.BuffRemove:
                        RemoveBuff(action);
                        break;

                    case BattleActionType.Death:
                        HandlePlayerDeath(action.PlayerId);
                        break;

                    case BattleActionType.Revive:
                        HandlePlayerRevive(action.PlayerId, action.Value);
                        break;

                    case BattleActionType.Dodge:
                    case BattleActionType.Block:
                    case BattleActionType.Counter:
                        break;
                }
            }
        }

        #endregion

        #region 伤害与治疗 (委托给Combat)

        private void ApplyDamage(BattleSyncData.BattleAction action)
        {
            if (action.TargetId <= 0) return;

            if (_playerStates.TryGetValue(action.TargetId, out var playerState))
            {
                float oldHealth = playerState.Health;
                playerState.Health = Math.Max(0, playerState.Health - action.Value);
                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                float change = playerState.Health - oldHealth;
                EmitSignal(SignalName.PlayerHealthChanged, action.TargetId, playerState.Health, playerState.MaxHealth, change);

                if (playerState.Health <= 0 && oldHealth > 0)
                {
                    HandlePlayerDeath(action.TargetId);
                }

                EmitSignal(SignalName.PlayerStateUpdated, action.TargetId, playerState);
            }
            else if (_enemyStates.TryGetValue(action.TargetId, out var enemyState))
            {
                float oldHealth = enemyState.Health;
                enemyState.Health = Math.Max(0, enemyState.Health - action.Value);
                enemyState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                if (enemyState.Health <= 0 && oldHealth > 0)
                {
                    enemyState.IsDead = true;
                    EmitSignal(SignalName.EnemyKilled, action.TargetId, action.PlayerId);
                }

                EmitSignal(SignalName.EnemyStateUpdated, action.TargetId, enemyState);
            }
        }

        private void ApplyHealing(BattleSyncData.BattleAction action)
        {
            if (action.TargetId <= 0) return;

            if (_playerStates.TryGetValue(action.TargetId, out var playerState))
            {
                float oldHealth = playerState.Health;
                playerState.Health = Math.Min(playerState.MaxHealth, playerState.Health + action.Value);
                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                float change = playerState.Health - oldHealth;
                EmitSignal(SignalName.PlayerHealthChanged, action.TargetId, playerState.Health, playerState.MaxHealth, change);

                EmitSignal(SignalName.PlayerStateUpdated, action.TargetId, playerState);
            }
        }

        #endregion

        #region Buff管理 (委托给Combat)

        private void ApplyBuff(BattleSyncData.BattleAction action)
        {
            if (action.TargetId <= 0 || string.IsNullOrEmpty(action.SkillId)) return;

            if (_playerStates.TryGetValue(action.TargetId, out var playerState))
            {
                var existingBuff = playerState.ActiveBuffs.FirstOrDefault(b => b.BuffId == action.SkillId);
                
                if (existingBuff != null)
                {
                    existingBuff.Stacks = Math.Min(existingBuff.Stacks + 1, _config.MaxBuffsPerPlayer);
                    existingBuff.Duration = action.Value;
                }
                else
                {
                    var newBuff = new BattleSyncData.BuffState
                    {
                        BuffId = action.SkillId,
                        BuffName = action.SkillId,
                        Stacks = 1,
                        Duration = action.Value,
                        IsDebuff = action.Value < 0
                    };
                    playerState.ActiveBuffs.Add(newBuff);
                }

                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                EmitSignal(SignalName.BuffApplied, action.TargetId, playerState.ActiveBuffs.First(b => b.BuffId == action.SkillId));
                EmitSignal(SignalName.PlayerStateUpdated, action.TargetId, playerState);
            }
        }

        private void RemoveBuff(BattleSyncData.BattleAction action)
        {
            if (action.TargetId <= 0 || string.IsNullOrEmpty(action.SkillId)) return;

            if (_playerStates.TryGetValue(action.TargetId, out var playerState))
            {
                var buff = playerState.ActiveBuffs.FirstOrDefault(b => b.BuffId == action.SkillId);
                if (buff != null)
                {
                    playerState.ActiveBuffs.Remove(buff);
                    playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                    EmitSignal(SignalName.BuffRemoved, action.TargetId, action.SkillId);
                    EmitSignal(SignalName.PlayerStateUpdated, action.TargetId, playerState);
                }
            }
        }

        #endregion

        #region 死亡与复活

        private void HandlePlayerDeath(int playerId)
        {
            if (_playerStates.TryGetValue(playerId, out var playerState))
            {
                playerState.IsDead = true;
                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                EmitSignal(SignalName.PlayerDied, playerId);
                EmitSignal(SignalName.PlayerStateUpdated, playerId, playerState);
                GD.Print($"[BattleSyncSystem] Player died: {playerId}");
            }
        }

        private void HandlePlayerRevive(int playerId, float healthPercent)
        {
            if (_playerStates.TryGetValue(playerId, out var playerState))
            {
                playerState.IsDead = false;
                playerState.Health = playerState.MaxHealth * healthPercent;
                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                EmitSignal(SignalName.PlayerRevived, playerId);
                EmitSignal(SignalName.PlayerStateUpdated, playerId, playerState);
                GD.Print($"[BattleSyncSystem] Player revived: {playerId}");
            }
        }

        #endregion

        #region 敌人管理

        public void AddEnemy(int enemyId, string enemyType, float maxHealth, float x, float y)
        {
            lock (_lock)
            {
                _enemyStates[enemyId] = new BattleSyncData.EnemyBattleState
                {
                    EnemyId = enemyId,
                    EnemyType = enemyType,
                    Health = maxHealth,
                    MaxHealth = maxHealth,
                    PositionX = x,
                    PositionY = y,
                    IsDead = false
                };
                GD.Print($"[BattleSyncSystem] Enemy added: {enemyType} (ID: {enemyId})");
            }
        }

        public void RemoveEnemy(int enemyId)
        {
            lock (_lock)
            {
                if (_enemyStates.Remove(enemyId))
                {
                    GD.Print($"[BattleSyncSystem] Enemy removed: {enemyId}");
                }
            }
        }

        public void SetEnemyAggro(int enemyId, int targetPlayerId)
        {
            lock (_lock)
            {
                if (_enemyStates.TryGetValue(enemyId, out var enemyState))
                {
                    int oldTarget = (int)enemyState.AggroPlayerId;
                    if (oldTarget != targetPlayerId)
                    {
                        enemyState.AggroPlayerId = targetPlayerId;
                        EmitSignal(SignalName.AggroChanged, enemyId, oldTarget, targetPlayerId);
                    }
                }
            }
        }

        public void UpdateEnemyPosition(int enemyId, float x, float y)
        {
            lock (_lock)
            {
                if (_enemyStates.TryGetValue(enemyId, out var enemyState))
                {
                    enemyState.PositionX = x;
                    enemyState.PositionY = y;
                    enemyState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }
        }

        #endregion

        #region 状态管理

        public void UpdatePlayerPosition(int playerId, float x, float y)
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

        public void UpdatePlayerStats(int playerId, float health, float mana)
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

        public BattleSyncData.PlayerBattleState? GetPlayerState(int playerId)
        {
            lock (_lock)
            {
                return _playerStates.TryGetValue(playerId, out var state) ? state : null;
            }
        }

        public List<BattleSyncData.PlayerBattleState> GetAllPlayerStates()
        {
            lock (_lock)
            {
                return _playerStates.Values.ToList();
            }
        }

        public BattleSyncData.EnemyBattleState? GetEnemyState(int enemyId)
        {
            lock (_lock)
            {
                return _enemyStates.TryGetValue(enemyId, out var state) ? state : null;
            }
        }

        public List<BattleSyncData.EnemyBattleState> GetAllEnemyStates()
        {
            lock (_lock)
            {
                return _enemyStates.Values.ToList();
            }
        }

        public BattleSyncData.BattleSnapshot CreateSnapshot()
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

        public void ApplySnapshot(BattleSyncData.BattleSnapshot snapshot)
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
                GD.Print($"[BattleSyncSystem] Snapshot applied: {snapshot.Players.Count} players, {snapshot.Enemies.Count} enemies");
            }
        }

        #endregion

        #region 同步处理

        private void ProcessStateSync()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _lastSyncTime = now;
        }

        private void ProcessActionBroadcast()
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

        private void BroadcastActionsToNetwork()
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
            GD.Print($"[BattleSync] Broadcasted {actions.Count} actions to network");
        }

        private void UpdateBuffDurations(float delta)
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

        private void CheckSyncLatency()
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

        public void SetConfig(BattleSyncData.BattleSyncConfig config)
        {
            _config = config;
            GD.Print($"[BattleSyncSystem] Config updated: sync rate={config.StateSyncRate}Hz, target latency={config.TargetLatencyMs}ms");
        }

        #endregion

        #region 存档支持

        public override Dictionary ExportSaveData()
        {
            lock (_lock)
            {
                var data = new Dictionary();
                data["current_session_id"] = _currentSessionId;
                
                var playerStatesList = new Array();
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

        public override void ImportSaveData(Dictionary data)
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
