using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗同步系统 - 多人实时战斗同步主系统
    /// 整合 CoreSystem, Combat, Player, EventHandler 四个子模块
    /// </summary>
    public partial class BattleSyncSystem : BaseSystem
    {
        private static BattleSyncSystem _instance;
        public static BattleSyncSystem Instance { get; private set; }

        // 子系统实例
        private BattleSyncCoreSystem _coreSystem;
        private BattleSyncCombat _combatSystem;
        private BattleSyncNetwork _networkSystem;
        private BattleSyncState _stateSystem;
        
        // 玩家与事件处理模块
        private BattleSyncPlayer _playerModule;
        private BattleSyncEventHandler _eventHandlerModule;

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
public delegate void BattleActionReceivedEventHandler(BattleSyncData.BattleAction action);

        /// <summary>
        /// 玩家状态更新
        /// </summary>
public delegate void PlayerStateUpdatedEventHandler(int playerId, BattleSyncData.PlayerBattleState state);

        /// <summary>
        /// 玩家生命值变化
        /// </summary>
public delegate void PlayerHealthChangedEventHandler(int playerId, float currentHealth, float maxHealth, float change);

        /// <summary>
        /// 玩家魔法值变化
        /// </summary>
public delegate void PlayerManaChangedEventHandler(int playerId, float currentMana, float maxMana, float change);

        /// <summary>
        /// Buff已应用
        /// </summary>
public delegate void BuffAppliedEventHandler(int playerId, BattleSyncData.BuffState buff);

        /// <summary>
        /// Buff已移除
        /// </summary>
public delegate void BuffRemovedEventHandler(int playerId, string buffId);

        /// <summary>
        /// 敌人状态更新
        /// </summary>
public delegate void EnemyStateUpdatedEventHandler(int enemyId, BattleSyncData.EnemyBattleState state);

        /// <summary>
        /// 敌人死亡
        /// </summary>
public delegate void EnemyKilledEventHandler(int enemyId, int killerId);

        /// <summary>
        /// 玩家死亡
        /// </summary>
public delegate void PlayerDiedEventHandler(int playerId);

        /// <summary>
        /// 玩家复活
        /// </summary>
public delegate void PlayerRevivedEventHandler(int playerId);

        /// <summary>
        /// 同步延迟警告
        /// </summary>
public delegate void SyncLatencyWarningEventHandler(float latencyMs);

        /// <summary>
        /// 战斗快照同步（用于新玩家加入或全量同步）
        /// </summary>
public delegate void BattleSnapshotReceivedEventHandler(BattleSyncData.BattleSnapshot snapshot);

        /// <summary>
        /// 仇恨转移（用于配合玩法：吸引仇恨）
        /// </summary>
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
            Instance = this;
            
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
            _playerModule = new BattleSyncPlayer();
            _eventHandlerModule = new BattleSyncEventHandler();
            
            // 初始化战斗系统队列
            _combatSystem.InitializeCombatQueues();
            
            // 初始化网络组件
            _networkSystem.InitializeNetworkComponents();
            
            // 注入状态引用到子模块
            InjectStateToModules();
            
            GD.Print("[BattleSyncSystem] Subsystems initialized");
        }

        /// <summary>
        /// 注入状态引用到子模块
        /// </summary>
        private void InjectStateToModules()
        {
            _playerModule.SetStateReferences(_playerStates, _enemyStates, _pendingActions, _config);
            _eventHandlerModule.SetStateReferences(_currentSessionId, _playerStates, _enemyStates, _pendingActions, _broadcastBuffer, _config, _lastSyncTime, _avgSyncLatency);
        }

        protected override void Initialize()
        {
            IsInitialized = true;
            GD.Print("[BattleSyncSystem] System initialized");
        }

        public override void _Process(double delta)
        {
            if (!IsInitialized || string.IsNullOrEmpty(_currentSessionId))
                return;

            // 更新战斗状态同步
            _stateSyncTimer += delta;
            if (_stateSyncTimer >= 1.0f / _config.StateSyncRate)
            {
                _stateSyncTimer = 0;
                _eventHandlerModule.ProcessStateSync();
            }

            // 更新战斗操作广播
            _actionBroadcastTimer += delta;
            if (_actionBroadcastTimer >= 1.0f / _config.ActionBroadcastRate)
            {
                _actionBroadcastTimer = 0;
                _eventHandlerModule.ProcessActionBroadcast();
            }

            // 更新Buff持续时间
            _eventHandlerModule.UpdateBuffDurations(delta);

            // 检查同步延迟
            _eventHandlerModule.CheckSyncLatency();
        }

        #endregion

        #region 委托方法 - 会话管理

        public void StartBattleSession(string sessionId, List<CoopPlayerData> players)
        {
            _playerModule.StartBattleSession(sessionId, players);
        }

        public void EndBattleSession()
        {
            _playerModule.EndBattleSession();
        }

        public void AddPlayer(int playerId, string playerName, float maxHealth = 100, float maxMana = 100)
        {
            _playerModule.AddPlayer(playerId, playerName, maxHealth, maxMana);
        }

        public void RemovePlayer(int playerId)
        {
            _playerModule.RemovePlayer(playerId);
        }

        #endregion

        #region 委托方法 - 战斗操作

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
            return _playerModule.RecordAction(playerId, playerName, type, value, skillId, targetId, targetX, targetY, isCritical);
        }

        public void ReceiveRemoteAction(BattleSyncData.BattleAction action)
        {
            _playerModule.ReceiveRemoteAction(action);
        }

        private void HandleRemoteAction(BattleSyncData.BattleAction action)
        {
            ReceiveRemoteAction(action);
        }

        #endregion

        #region 委托方法 - 敌人管理

        public void AddEnemy(int enemyId, string enemyType, float maxHealth, float x, float y)
        {
            _playerModule.AddEnemy(enemyId, enemyType, maxHealth, x, y);
        }

        public void RemoveEnemy(int enemyId)
        {
            _playerModule.RemoveEnemy(enemyId);
        }

        public void SetEnemyAggro(int enemyId, int targetPlayerId)
        {
            _playerModule.SetEnemyAggro(enemyId, targetPlayerId);
        }

        public void UpdateEnemyPosition(int enemyId, float x, float y)
        {
            _playerModule.UpdateEnemyPosition(enemyId, x, y);
        }

        #endregion

        #region 委托方法 - 状态管理

        public void UpdatePlayerPosition(int playerId, float x, float y)
        {
            _eventHandlerModule.UpdatePlayerPosition(playerId, x, y);
        }

        public void UpdatePlayerStats(int playerId, float health, float mana)
        {
            _eventHandlerModule.UpdatePlayerStats(playerId, health, mana);
        }

        public BattleSyncData.PlayerBattleState? GetPlayerState(int playerId)
        {
            return _eventHandlerModule.GetPlayerState(playerId);
        }

        public List<BattleSyncData.PlayerBattleState> GetAllPlayerStates()
        {
            return _eventHandlerModule.GetAllPlayerStates();
        }

        public BattleSyncData.EnemyBattleState? GetEnemyState(int enemyId)
        {
            return _eventHandlerModule.GetEnemyState(enemyId);
        }

        public List<BattleSyncData.EnemyBattleState> GetAllEnemyStates()
        {
            return _eventHandlerModule.GetAllEnemyStates();
        }

        public BattleSyncData.BattleSnapshot CreateSnapshot()
        {
            return _eventHandlerModule.CreateSnapshot();
        }

        public void ApplySnapshot(BattleSyncData.BattleSnapshot snapshot)
        {
            _eventHandlerModule.ApplySnapshot(snapshot);
        }

        #endregion

        #region 配置

        public void SetConfig(BattleSyncData.BattleSyncConfig config)
        {
            _config = config;
            _playerModule.SetConfig(config);
            _eventHandlerModule.SetConfig(config);
            GD.Print($"[BattleSyncSystem] Config updated: sync rate={config.StateSyncRate}Hz, target latency={config.TargetLatencyMs}ms");
        }

        #endregion

        #region 存档支持

        public override Dictionary<string, object> ExportSaveData()
        {
            return _eventHandlerModule.ExportSaveData();
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            _eventHandlerModule.ImportSaveData(data);
        }

        #endregion
    }
}
