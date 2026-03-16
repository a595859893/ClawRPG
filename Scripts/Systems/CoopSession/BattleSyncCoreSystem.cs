using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗同步核心系统 - 信号定义、生命周期、会话管理、配置
    /// </summary>
    public class BattleSyncCoreSystem : BaseSystem
    {
        // 单例
        private static BattleSyncCoreSystem _instance;
        public static BattleSyncCoreSystem Instance => _instance ??= new BattleSyncCoreSystem();

        // 线程安全锁
        protected readonly object _lock = new object();

        // 同步配置
        protected BattleSyncData.BattleSyncConfig _config;

        // 当前会话ID
        protected string _currentSessionId = "";

        // 定时器
        protected float _stateSyncTimer = 0f;
        protected float _actionBroadcastTimer = 0f;

        // 延迟追踪
        protected long _lastSyncTime = 0;
        protected float _avgSyncLatency = 0f;

        #region 信号定义 (Godot 4.x)

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
            
            // 默认配置
            _config = new BattleSyncData.BattleSyncConfig();
            
            GD.Print("[BattleSyncCoreSystem] Initialized");
        }

        protected override void Initialize()
        {
            IsInitialized = true;
            GD.Print("[BattleSyncCoreSystem] System initialized");
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
        public virtual void StartBattleSession(string sessionId, List<CoopPlayerData> players)
        {
            lock (_lock)
            {
                _currentSessionId = sessionId;
                _lastSyncTime = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                GD.Print($"[BattleSyncCoreSystem] Battle session started: {sessionId}, players: {players.Count}");
            }
        }

        /// <summary>
        /// 结束战斗同步会话
        /// </summary>
        public virtual void EndBattleSession()
        {
            lock (_lock)
            {
                GD.Print($"[BattleSyncCoreSystem] Battle session ended: {_currentSessionId}");
                _currentSessionId = "";
            }
        }

        #endregion

        #region 配置

        /// <summary>
        /// 设置同步配置
        /// </summary>
        public void SetConfig(BattleSyncData.BattleSyncConfig config)
        {
            _config = config;
            GD.Print($"[BattleSyncCoreSystem] Config updated: sync rate={config.StateSyncRate}Hz, target latency={config.TargetLatencyMs}ms");
        }

        /// <summary>
        /// 获取同步配置
        /// </summary>
        public BattleSyncData.BattleSyncConfig GetConfig() => _config;

        #endregion

        #region 同步处理 (供子类重写)

        /// <summary>
        /// 处理状态同步 - 可由子类重写
        /// </summary>
        protected virtual void ProcessStateSync()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _lastSyncTime = now;
        }

        /// <summary>
        /// 处理战斗操作广播 - 可由子类重写
        /// </summary>
        protected virtual void ProcessActionBroadcast()
        {
            // 子类实现
        }

        /// <summary>
        /// 更新Buff持续时间 - 可由子类重写
        /// </summary>
        protected virtual void UpdateBuffDurations(float delta)
        {
            // 子类实现
        }

        /// <summary>
        /// 检查同步延迟 - 可由子类重写
        /// </summary>
        protected virtual void CheckSyncLatency()
        {
            long now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            long diff = now - _lastSyncTime;
            
            // 简单的延迟估算
            _avgSyncLatency = _avgSyncLatency * 0.9f + diff * 0.1f;

            if (_avgSyncLatency > _config.TargetLatencyMs)
            {
                EmitSignal(SignalName.SyncLatencyWarning, _avgSyncLatency);
            }
        }

        #endregion

        #region 存档支持

        public override Dictionary ExportSaveData()
        {
            lock (_lock)
            {
                var data = new Dictionary();
                data["current_session_id"] = _currentSessionId;
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
            }
        }

        #endregion
    }
}
