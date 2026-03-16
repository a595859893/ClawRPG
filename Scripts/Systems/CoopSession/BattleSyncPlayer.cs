using Godot;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗同步系统 - 玩家与敌人管理模块
    /// 处理会话管理、战斗操作、状态更新
    /// </summary>
    public class BattleSyncPlayer : BaseSystem
    {
        private static BattleSyncPlayer _instance;
        public static BattleSyncPlayer Instance => _instance ??= new BattleSyncPlayer();

        // 线程安全锁
        protected readonly object _lock = new object();

        // 配置引用
        protected BattleSyncData.BattleSyncConfig _config;

        // 状态引用（由主系统注入）
        protected Dictionary<int, BattleSyncData.PlayerBattleState> _playerStates;
        protected Dictionary<int, BattleSyncData.EnemyBattleState> _enemyStates;
        
        // 待广播的战斗操作
        protected Queue<BattleSyncData.BattleAction> _pendingActions;

        #region 状态注入

        /// <summary>
        /// 设置状态引用（由主系统调用）
        /// </summary>
        public void SetStateReferences(
            Dictionary<int, BattleSyncData.PlayerBattleState> playerStates,
            Dictionary<int, BattleSyncData.EnemyBattleState> enemyStates,
            Queue<BattleSyncData.BattleAction> pendingActions,
            BattleSyncData.BattleSyncConfig config)
        {
            _playerStates = playerStates;
            _enemyStates = enemyStates;
            _pendingActions = pendingActions;
            _config = config;
        }

        /// <summary>
        /// 设置配置
        /// </summary>
        public void SetConfig(BattleSyncData.BattleSyncConfig config)
        {
            _config = config;
        }

        #endregion

        #region 信号定义

        [Signal]
        public delegate void BattleActionReceivedEventHandler(BattleSyncData.BattleAction action);

        [Signal]
        public delegate void PlayerStateUpdatedEventHandler(int playerId, BattleSyncData.PlayerBattleState state);

        [Signal]
        public delegate void PlayerHealthChangedEventHandler(int playerId, float currentHealth, float maxHealth, float change);

        [Signal]
        public delegate void PlayerManaChangedEventHandler(int playerId, float currentMana, float maxMana, float change);

        [Signal]
        public delegate void BuffAppliedEventHandler(int playerId, BattleSyncData.BuffState buff);

        [Signal]
        public delegate void BuffRemovedEventHandler(int playerId, string buffId);

        [Signal]
        public delegate void EnemyStateUpdatedEventHandler(int enemyId, BattleSyncData.EnemyBattleState state);

        [Signal]
        public delegate void EnemyKilledEventHandler(int enemyId, int killerId);

        [Signal]
        public delegate void PlayerDiedEventHandler(int playerId);

        [Signal]
        public delegate void PlayerRevivedEventHandler(int playerId);

        [Signal]
        public delegate void AggroChangedEventHandler(int enemyId, int oldTargetId, int newTargetId);

        #endregion

        #region 会话管理

        /// <summary>
        /// 开始战斗同步会话
        /// </summary>
        public virtual void StartBattleSession(string sessionId, List<CoopPlayerData> players)
        {
            lock (_lock)
            {
                _playerStates.Clear();
                _enemyStates.Clear();
                _pendingActions.Clear();

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

                GD.Print($"[BattleSyncPlayer] Battle session started: {sessionId}, players: {players.Count}");
            }
        }

        /// <summary>
        /// 结束战斗同步会话
        /// </summary>
        public virtual void EndBattleSession()
        {
            lock (_lock)
            {
                GD.Print($"[BattleSyncPlayer] Battle session ended");
                _playerStates.Clear();
                _enemyStates.Clear();
                _pendingActions.Clear();
            }
        }

        /// <summary>
        /// 添加玩家到战斗
        /// </summary>
        public virtual void AddPlayer(int playerId, string playerName, float maxHealth = 100, float maxMana = 100)
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
                GD.Print($"[BattleSyncPlayer] Player added to battle: {playerName} (ID: {playerId})");
            }
        }

        /// <summary>
        /// 移除玩家从战斗
        /// </summary>
        public virtual void RemovePlayer(int playerId)
        {
            lock (_lock)
            {
                if (_playerStates.Remove(playerId))
                {
                    GD.Print($"[BattleSyncPlayer] Player removed from battle: {playerId}");
                }
            }
        }

        #endregion

        #region 战斗操作

        /// <summary>
        /// 记录战斗操作（本地）
        /// </summary>
        public virtual BattleSyncData.BattleAction RecordAction(
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
        public virtual void ReceiveRemoteAction(BattleSyncData.BattleAction action)
        {
            // 应用操作效果
            ApplyActionEffect(action);

            // 发出信号
            EmitSignal(SignalName.BattleActionReceived, action);
        }

        /// <summary>
        /// 应用战斗操作效果
        /// </summary>
        protected virtual void ApplyActionEffect(BattleSyncData.BattleAction action)
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

        #region 伤害与治疗

        protected virtual void ApplyDamage(BattleSyncData.BattleAction action)
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

        protected virtual void ApplyHealing(BattleSyncData.BattleAction action)
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

        #region Buff管理

        protected virtual void ApplyBuff(BattleSyncData.BattleAction action)
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

        protected virtual void RemoveBuff(BattleSyncData.BattleAction action)
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

        public virtual void HandlePlayerDeath(int playerId)
        {
            if (_playerStates.TryGetValue(playerId, out var playerState))
            {
                playerState.IsDead = true;
                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                EmitSignal(SignalName.PlayerDied, playerId);
                EmitSignal(SignalName.PlayerStateUpdated, playerId, playerState);
                GD.Print($"[BattleSyncPlayer] Player died: {playerId}");
            }
        }

        public virtual void HandlePlayerRevive(int playerId, float healthPercent)
        {
            if (_playerStates.TryGetValue(playerId, out var playerState))
            {
                playerState.IsDead = false;
                playerState.Health = playerState.MaxHealth * healthPercent;
                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                EmitSignal(SignalName.PlayerRevived, playerId);
                EmitSignal(SignalName.PlayerStateUpdated, playerId, playerState);
                GD.Print($"[BattleSyncPlayer] Player revived: {playerId}");
            }
        }

        #endregion

        #region 敌人管理

        public virtual void AddEnemy(int enemyId, string enemyType, float maxHealth, float x, float y)
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
                GD.Print($"[BattleSyncPlayer] Enemy added: {enemyType} (ID: {enemyId})");
            }
        }

        public virtual void RemoveEnemy(int enemyId)
        {
            lock (_lock)
            {
                if (_enemyStates.Remove(enemyId))
                {
                    GD.Print($"[BattleSyncPlayer] Enemy removed: {enemyId}");
                }
            }
        }

        public virtual void SetEnemyAggro(int enemyId, int targetPlayerId)
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

        public virtual void UpdateEnemyPosition(int enemyId, float x, float y)
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
    }
}
