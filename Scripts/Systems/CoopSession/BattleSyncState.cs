using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ClawRPG.Scripts.Systems.CoopSession
{
    /// <summary>
    /// 战斗状态管理 - 玩家/敌人状态、波次管理
    /// </summary>
    public partial class BattleSyncState : BaseSystem
    {
        // 玩家战斗状态
        protected Dictionary<int, BattleSyncData.PlayerBattleState> _playerStates;
        
        // 敌人战斗状态
        protected Dictionary<int, BattleSyncData.EnemyBattleState> _enemyStates;

        // Thread lock for synchronization
        private readonly object _lock = new object();

        // 波次管理
        protected int _currentWave = 0;
        protected int _totalWaves = 1;
        protected bool _waveInProgress = false;

        #region 玩家状态管理

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
                GD.Print($"[BattleSyncState] Player added to battle: {playerName} (ID: {playerId})");
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
                    GD.Print($"[BattleSyncState] Player removed from battle: {playerId}");
                }
            }
        }

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
        /// 初始化玩家状态列表
        /// </summary>
        protected void InitializePlayerStates(List<CoopPlayerData> players)
        {
            _playerStates.Clear();
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
        }

        /// <summary>
        /// 清除所有玩家状态
        /// </summary>
        protected void ClearPlayerStates()
        {
            _playerStates.Clear();
        }

        #endregion

        #region 敌人状态管理

        /// <summary>
        /// 添加敌人到战斗
        /// </summary>
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
                GD.Print($"[BattleSyncState] Enemy added: {enemyType} (ID: {enemyId})");
            }
        }

        /// <summary>
        /// 移除敌人
        /// </summary>
        public virtual void RemoveEnemy(int enemyId)
        {
            lock (_lock)
            {
                if (_enemyStates.Remove(enemyId))
                {
                    GD.Print($"[BattleSyncState] Enemy removed: {enemyId}");
                }
            }
        }

        /// <summary>
        /// 更新敌人仇恨目标（用于配合玩法：吸引仇恨）
        /// </summary>
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

        /// <summary>
        /// 更新敌人位置
        /// </summary>
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
        /// 清除所有敌人状态
        /// </summary>
        protected void ClearEnemyStates()
        {
            _enemyStates.Clear();
        }

        #endregion

        #region 波次管理

        /// <summary>
        /// 获取当前波次
        /// </summary>
        public int CurrentWave => _currentWave;

        /// <summary>
        /// 获取总波次数
        /// </summary>
        public int TotalWaves => _totalWaves;

        /// <summary>
        /// 波次是否在进行中
        /// </summary>
        public bool IsWaveInProgress => _waveInProgress;

        /// <summary>
        /// 开始新波次
        /// </summary>
        public virtual void StartWave(int waveNumber)
        {
            _currentWave = waveNumber;
            _waveInProgress = true;
            GD.Print($"[BattleSyncState] Wave started: {waveNumber}");
        }

        /// <summary>
        /// 结束当前波次
        /// </summary>
        public virtual void EndWave()
        {
            _waveInProgress = false;
            GD.Print($"[BattleSyncState] Wave ended: {_currentWave}");
        }

        /// <summary>
        /// 设置总波次数
        /// </summary>
        public virtual void SetTotalWaves(int total)
        {
            _totalWaves = total;
        }

        /// <summary>
        /// 检查是否所有敌人都已死亡
        /// </summary>
        public virtual bool AreAllEnemiesDead()
        {
            lock (_lock)
            {
                return _enemyStates.Values.All(e => e.IsDead);
            }
        }

        /// <summary>
        /// 检查是否所有玩家都已死亡
        /// </summary>
        public virtual bool AreAllPlayersDead()
        {
            lock (_lock)
            {
                return _playerStates.Values.All(p => p.IsDead);
            }
        }

        #endregion

        #region 快照管理

        /// <summary>
        /// 创建战斗快照（用于全量同步）
        /// </summary>
        public virtual BattleSyncData.BattleSnapshot CreateSnapshot(string sessionId)
        {
            lock (_lock)
            {
                var snapshot = new BattleSyncData.BattleSnapshot
                {
                    SessionId = sessionId,
                    Players = _playerStates.Values.ToList(),
                    Enemies = _enemyStates.Values.ToList()
                };
                return snapshot;
            }
        }

        /// <summary>
        /// 应用战斗快照（用于全量同步）
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
                GD.Print($"[BattleSyncState] Snapshot applied: {snapshot.Players.Count} players, {snapshot.Enemies.Count} enemies");
            }
        }

        #endregion

        #region 存档支持

        public override Dictionary<string, object> ExportSaveData()
        {
            lock (_lock)
            {
                var data = new Dictionary<string, object>();
                
                // 导出玩家状态
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
                
                // 导出波次信息
                data["current_wave"] = _currentWave;
                data["total_waves"] = _totalWaves;
                data["wave_in_progress"] = _waveInProgress;
                
                return data;
            }
        }

        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;

            lock (_lock)
            {
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

                // 导入波次信息
                if (data.ContainsKey("current_wave"))
                    _currentWave = Convert.ToInt32(data["current_wave"]);
                if (data.ContainsKey("total_waves"))
                    _totalWaves = Convert.ToInt32(data["total_waves"]);
                if (data.ContainsKey("wave_in_progress"))
                    _waveInProgress = Convert.ToBoolean(data["wave_in_progress"]);
            }
        }

        #endregion
    }
}
