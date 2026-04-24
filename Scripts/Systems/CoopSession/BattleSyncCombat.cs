using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗操作处理 - 攻击、技能、伤害、治疗、Buff管理
    /// </summary>
    public partial class BattleSyncCombat : BaseSystem
    {
        // 线程安全锁
        protected readonly object _lock = new object();

        // Signal delegates — enables SignalName inner class generation
#pragma warning disable GD0201
        [Signal] public delegate void BattleActionReceived(BattleSyncData.BattleAction action);
        [Signal] public delegate void PlayerHealthChanged(string targetId, double health, double maxHealth, double change);
        [Signal] public delegate void PlayerStateUpdated(string targetId, BattleSyncData.PlayerState state);
        [Signal] public delegate void EnemyKilled(string enemyId, string playerId);
        [Signal] public delegate void EnemyStateUpdated(string enemyId, BattleSyncData.EnemyState state);
        [Signal] public delegate void BuffApplied(string targetId, BattleSyncData.BuffState buff);
        [Signal] public delegate void BuffRemoved(string targetId, string buffId);
        [Signal] public delegate void PlayerDied(string playerId);
        [Signal] public delegate void PlayerRevived(string playerId);
#pragma warning restore GD0201

        // 待广播的战斗操作
        protected Queue<BattleSyncData.BattleAction> _pendingActions;
        protected Queue<BattleSyncData.BattleAction> _broadcastBuffer;

        // 配置引用
        protected BattleSyncData.BattleSyncConfig _config;

        // 状态引用（由主系统注入）
        protected Dictionary<int, BattleSyncData.PlayerBattleState> _playerStates;
        protected Dictionary<int, BattleSyncData.EnemyBattleState> _enemyStates;

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
                        // 这些是闪避/格挡/反击行为，不需要数值变化
                        break;
                }
            }
        }

        #endregion

        #region 伤害与治疗

        /// <summary>
        /// 应用伤害
        /// </summary>
        protected virtual void ApplyDamage(BattleSyncData.BattleAction action)
        {
            if (action.TargetId <= 0) return;

            // 检查目标是玩家还是敌人
            if (_playerStates.TryGetValue(action.TargetId, out var playerState))
            {
                float oldHealth = playerState.Health;
                playerState.Health = Math.Max(0, playerState.Health - action.Value);
                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

                float change = playerState.Health - oldHealth;
                EmitSignal(SignalName.PlayerHealthChanged, action.TargetId, playerState.Health, playerState.MaxHealth, change);

                // 检查死亡
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

                // 检查死亡
                if (enemyState.Health <= 0 && oldHealth > 0)
                {
                    enemyState.IsDead = true;
                    EmitSignal(SignalName.EnemyKilled, action.TargetId, action.PlayerId);
                }

                EmitSignal(SignalName.EnemyStateUpdated, action.TargetId, enemyState);
            }
        }

        /// <summary>
        /// 应用治疗
        /// </summary>
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

        /// <summary>
        /// 应用Buff
        /// </summary>
        protected virtual void ApplyBuff(BattleSyncData.BattleAction action)
        {
            if (action.TargetId <= 0 || string.IsNullOrEmpty(action.SkillId)) return;

            if (_playerStates.TryGetValue(action.TargetId, out var playerState))
            {
                var existingBuff = playerState.ActiveBuffs.FirstOrDefault(b => b.BuffId == action.SkillId);
                
                if (existingBuff != null)
                {
                    // 更新现有Buff
                    existingBuff.Stacks = Math.Min(existingBuff.Stacks + 1, _config.MaxBuffsPerPlayer);
                    existingBuff.Duration = action.Value;  // Value作为持续时间
                }
                else
                {
                    // 添加新Buff
                    var newBuff = new BattleSyncData.BuffState
                    {
                        BuffId = action.SkillId,
                        BuffName = action.SkillId,
                        Stacks = 1,
                        Duration = action.Value,
                        IsDebuff = action.Value < 0  // 负值持续时间表示debuff
                    };
                    playerState.ActiveBuffs.Add(newBuff);
                }

                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                EmitSignal(SignalName.BuffApplied, action.TargetId, playerState.ActiveBuffs.First(b => b.BuffId == action.SkillId));
                EmitSignal(SignalName.PlayerStateUpdated, action.TargetId, playerState);
            }
        }

        /// <summary>
        /// 移除Buff
        /// </summary>
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

        /// <summary>
        /// 更新所有玩家Buff持续时间
        /// </summary>
        protected virtual void UpdateAllBuffDurations(float delta)
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

                    // 移除过期的Buff
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

        #endregion

        #region 死亡与复活

        /// <summary>
        /// 处理玩家死亡
        /// </summary>
        protected virtual void HandlePlayerDeath(int playerId)
        {
            if (_playerStates.TryGetValue(playerId, out var playerState))
            {
                playerState.IsDead = true;
                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                EmitSignal(SignalName.PlayerDied, playerId);
                EmitSignal(SignalName.PlayerStateUpdated, playerId, playerState);
                GD.Print($"[BattleSyncCombat] Player died: {playerId}");
            }
        }

        /// <summary>
        /// 处理玩家复活
        /// </summary>
        protected virtual void HandlePlayerRevive(int playerId, float healthPercent)
        {
            if (_playerStates.TryGetValue(playerId, out var playerState))
            {
                playerState.IsDead = false;
                playerState.Health = playerState.MaxHealth * healthPercent;
                playerState.LastUpdate = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                EmitSignal(SignalName.PlayerRevived, playerId);
                EmitSignal(SignalName.PlayerStateUpdated, playerId, playerState);
                GD.Print($"[BattleSyncCombat] Player revived: {playerId}");
            }
        }

        #endregion

        #region 敌人AI触发

        /// <summary>
        /// 触发敌人AI行为
        /// </summary>
        public virtual void TriggerEnemyAI(int enemyId)
        {
            if (!_enemyStates.TryGetValue(enemyId, out var enemyState))
                return;

            if (enemyState.IsDead)
                return;

            // 简单的AI逻辑：攻击当前仇恨目标
            if (enemyState.AggroPlayerId > 0 && _playerStates.TryGetValue((int)enemyState.AggroPlayerId, out var targetPlayer))
            {
                if (!targetPlayer.IsDead)
                {
                    // 造成伤害（简单示例：10点伤害）
                    var action = new BattleSyncData.BattleAction
                    {
                        PlayerId = -enemyId,  // 负数表示敌人
                        PlayerName = enemyState.EnemyType,
                        Type = BattleActionType.Attack,
                        Value = 10f,
                        TargetId = (int)enemyState.AggroPlayerId,
                        TargetX = targetPlayer.PositionX,
                        TargetY = targetPlayer.PositionY
                    };
                    ApplyActionEffect(action);
                }
            }
        }

        /// <summary>
        /// 触发所有存活敌人的AI
        /// </summary>
        public virtual void TriggerAllEnemyAI()
        {
            lock (_lock)
            {
                foreach (var enemy in _enemyStates.Values)
                {
                    if (!enemy.IsDead)
                    {
                        TriggerEnemyAI(enemy.EnemyId);
                    }
                }
            }
        }

        #endregion

        #region 广播处理

        /// <summary>
        /// 处理待广播的操作
        /// </summary>
        protected virtual void ProcessPendingActionsForBroadcast()
        {
            lock (_lock)
            {
                if (_pendingActions.Count == 0) return;

                // 将待处理操作移动到广播缓冲区
                while (_pendingActions.Count > 0)
                {
                    var action = _pendingActions.Dequeue();
                    _broadcastBuffer.Enqueue(action);

                    // 发出信号通知UI/其他系统
                    EmitSignal(SignalName.BattleActionReceived, action);
                }
            }
        }

        /// <summary>
        /// 获取待广播的操作
        /// </summary>
        protected virtual List<BattleSyncData.BattleAction> GetBroadcastActions()
        {
            var actions = new List<BattleSyncData.BattleAction>();
            lock (_lock)
            {
                while (_broadcastBuffer.Count > 0)
                {
                    actions.Add(_broadcastBuffer.Dequeue());
                }
            }
            return actions;
        }

        #endregion

        #region 初始化

        /// <summary>
        /// 初始化战斗操作队列
        /// </summary>
        protected void InitializeCombatQueues()
        {
            _pendingActions = new Queue<BattleSyncData.BattleAction>();
            _broadcastBuffer = new Queue<BattleSyncData.BattleAction>();
        }

        /// <summary>
        /// 清除所有队列
        /// </summary>
        protected void ClearCombatQueues()
        {
            _pendingActions.Clear();
            _broadcastBuffer.Clear();
        }

        #endregion

        #region 持久化

        /// <summary>
        /// 导出持久化数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 序列化待广播的战斗操作
            var pendingActionsData = new ArrayList();
            if (_pendingActions != null)
            {
                foreach (var action in _pendingActions)
                {
                    pendingActionsData.Add(new Dictionary
                    {
                        ["playerId"] = action.PlayerId,
                        ["playerName"] = action.PlayerName,
                        ["type"] = (int)action.Type,
                        ["value"] = action.Value,
                        ["skillId"] = action.SkillId,
                        ["targetId"] = action.TargetId,
                        ["targetX"] = action.TargetX,
                        ["targetY"] = action.TargetY,
                        ["isCritical"] = action.IsCritical,
                        ["timestamp"] = action.Timestamp
                    });
                }
            }
            data["PendingActions"] = pendingActionsData;
            
            // 序列化广播缓冲区
            var broadcastBufferData = new ArrayList();
            if (_broadcastBuffer != null)
            {
                foreach (var action in _broadcastBuffer)
                {
                    broadcastBufferData.Add(new Dictionary
                    {
                        ["playerId"] = action.PlayerId,
                        ["playerName"] = action.PlayerName,
                        ["type"] = (int)action.Type,
                        ["value"] = action.Value,
                        ["skillId"] = action.SkillId,
                        ["targetId"] = action.TargetId,
                        ["targetX"] = action.TargetX,
                        ["targetY"] = action.TargetY,
                        ["isCritical"] = action.IsCritical,
                        ["timestamp"] = action.Timestamp
                    });
                }
            }
            data["BroadcastBuffer"] = broadcastBufferData;
            
            GD.Print($"[BattleSyncCombat] 导出 {pendingActionsData.Count} 待处理操作, {broadcastBufferData.Count} 广播缓冲");
            return data;
        }

        /// <summary>
        /// 导入持久化数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null)
            {
                GD.Print("[BattleSyncCombat] 无数据可导入");
                return;
            }
            
            // 初始化队列
            if (_pendingActions == null) _pendingActions = new Queue<BattleSyncData.BattleAction>();
            if (_broadcastBuffer == null) _broadcastBuffer = new Queue<BattleSyncData.BattleAction>();
            
            // 导入待处理操作
            _pendingActions.Clear();
            if (data.Contains("PendingActions") && data["PendingActions"] is ArrayList pendingActionsData)
            {
                foreach (Dictionary aDict in pendingActionsData)
                {
                    var action = new BattleSyncData.BattleAction
                    {
                        PlayerId = aDict["playerId"] is int pid ? pid : 0,
                        PlayerName = aDict["playerName"]?.ToString() ?? "",
                        Type = aDict["type"] is int t ? (BattleActionType)t : BattleActionType.Attack,
                        Value = aDict["value"] is float v ? v : 0f,
                        SkillId = aDict["skillId"]?.ToString() ?? "",
                        TargetId = aDict["targetId"] is int tid ? tid : -1,
                        TargetX = aDict["targetX"] is float tx ? tx : 0f,
                        TargetY = aDict["targetY"] is float ty ? ty : 0f,
                        IsCritical = aDict["isCritical"] is bool ic ? ic : false,
                        Timestamp = aDict["timestamp"] is long ts ? ts : 0L
                    };
                    _pendingActions.Enqueue(action);
                }
            }
            
            // 导入广播缓冲区
            _broadcastBuffer.Clear();
            if (data.Contains("BroadcastBuffer") && data["BroadcastBuffer"] is ArrayList broadcastBufferData)
            {
                foreach (Dictionary aDict in broadcastBufferData)
                {
                    var action = new BattleSyncData.BattleAction
                    {
                        PlayerId = aDict["playerId"] is int pid ? pid : 0,
                        PlayerName = aDict["playerName"]?.ToString() ?? "",
                        Type = aDict["type"] is int t ? (BattleActionType)t : BattleActionType.Attack,
                        Value = aDict["value"] is float v ? v : 0f,
                        SkillId = aDict["skillId"]?.ToString() ?? "",
                        TargetId = aDict["targetId"] is int tid ? tid : -1,
                        TargetX = aDict["targetX"] is float tx ? tx : 0f,
                        TargetY = aDict["targetY"] is float ty ? ty : 0f,
                        IsCritical = aDict["isCritical"] is bool ic ? ic : false,
                        Timestamp = aDict["timestamp"] is long ts ? ts : 0L
                    };
                    _broadcastBuffer.Enqueue(action);
                }
            }
            
            GD.Print($"[BattleSyncCombat] 导入 {_pendingActions?.Count ?? 0} 待处理操作, {_broadcastBuffer?.Count ?? 0} 广播缓冲");
        }

        #endregion
    }
}
