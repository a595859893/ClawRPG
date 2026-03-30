using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.WorldBoss;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// WorldBossSystem - 世界Boss系统协调者
    /// 委托给子系统：
    /// - WorldBossSpawner: Boss生成
    /// - WorldBossDamageTracker: 伤害统计
    /// - WorldBossRewardSystem: 奖励分配
    /// </summary>
    public partial class WorldBossSystem : BaseSystem
    {
        public static WorldBossSystem Instance { get; private set; }
        
        // 子系统实例
        private WorldBossSpawner _spawner;
        private WorldBossDamageTracker _damageTracker;
        private WorldBossRewardSystem _rewardSystem;
        
        // Active world bosses (本地缓存)
        private List<WorldBossData.ActiveWorldBoss> _activeBosses = new List<WorldBossData.ActiveWorldBoss>();
        
        // Signals - 委托给子系统
        public delegate void BossSpawnedEventHandler(WorldBossData.ActiveWorldBoss boss);
        public delegate void BossDamagedEventHandler(string bossInstanceId, string playerId, int damage);
        public delegate void BossDefeatedEventHandler(WorldBossData.ActiveWorldBoss boss);
        public delegate void BossEscapedEventHandler(WorldBossData.ActiveWorldBoss boss);
        
        public override void _Ready()
        {
            Instance = this;
            
            // 初始化子系统
            _spawner = new WorldBossSpawner();
            _damageTracker = new WorldBossDamageTracker();
            _rewardSystem = new WorldBossRewardSystem();
            
            // 连接子系统信号
            if (_spawner != null)
            {
                _spawner.BossSpawned += OnBossSpawned;
            }
            if (_damageTracker != null)
            {
                _damageTracker.BossDamaged += OnBossDamaged;
            }
            if (_rewardSystem != null)
            {
                _rewardSystem.BossDefeated += OnBossDefeated;
            }
            
            GD.Print("[WorldBossSystem] Initialized with subsystems");
        }
        
        private void OnBossSpawned(WorldBossData.ActiveWorldBoss boss)
        {
            _activeBosses.Add(boss);
            EmitSignal(SignalName.BossSpawned, boss);
        }
        
        private void OnBossDamaged(string bossInstanceId, string playerId, int damage)
        {
            EmitSignal(SignalName.BossDamaged, bossInstanceId, playerId, damage);
        }
        
        private void OnBossDefeated(WorldBossData.ActiveWorldBoss boss)
        {
            _activeBosses.Remove(boss);
            _damageTracker.ClearBossRecords(boss.InstanceId);
            EmitSignal(SignalName.BossDefeated, boss);
        }
        
        public override void _Process(double delta)
        {
            // 子系统各自处理自己的逻辑
            ProcessActiveBosses(delta);
        }
        
        private void ProcessActiveBosses(double delta)
        {
            List<WorldBossData.ActiveWorldBoss> bossesToRemove = new List<WorldBossData.ActiveWorldBoss>();
            
            foreach (var boss in _activeBosses)
            {
                if (boss.IsDefeated) continue;
                
                // Check lifetime
                var elapsed = DateTime.Now - boss.SpawnTime;
                if (elapsed.TotalMinutes >= boss.LifeTimeMinutes)
                {
                    bossesToRemove.Add(boss);
                    EmitSignal(SignalName.BossEscaped, boss);
                    continue;
                }
                
                // Check if all players left
                if (boss.PlayerCount == 0)
                {
                    bossesToRemove.Add(boss);
                    EmitSignal(SignalName.BossEscaped, boss);
                }
            }
            
            foreach (var boss in bossesToRemove)
            {
                _activeBosses.Remove(boss);
                if (_spawner != null)
                {
                    _spawner.UnregisterBoss(boss.InstanceId);
                }
            }
        }
        
        /// <summary>
        /// Spawn a world boss
        /// </summary>
        public WorldBossData.ActiveWorldBoss SpawnBoss(string bossId)
        {
            if (_spawner == null) return null;
            
            var bossTemplate = WorldBossDatabase.GetBoss(bossId);
            if (bossTemplate == null) return null;
            
            var activeBoss = new WorldBossData.ActiveWorldBoss
            {
                InstanceId = Guid.NewGuid().ToString(),
                BossId = bossId,
                Name = bossTemplate.Name,
                MaxHealth = bossTemplate.Health,
                CurrentHealth = bossTemplate.Health,
                Level = bossTemplate.Level,
                Position = GetRandomSpawnPosition(),
                SpawnTime = DateTime.Now,
                IsElite = bossTemplate.IsElite,
                IsMega = bossTemplate.IsMega,
                LifeTimeMinutes = bossTemplate.SpawnIntervalMinutes > 0 ? bossTemplate.SpawnIntervalMinutes : 30,
                IsDefeated = false,
                TotalDamageDealt = 0,
                PlayerCount = 1
            };
            
            _activeBosses.Add(activeBoss);
            _spawner.RegisterBoss(activeBoss.InstanceId);
            
            EmitSignal(SignalName.BossSpawned, activeBoss);
            
            GD.Print($"[WorldBossSystem] {bossTemplate.Name} spawned!");
            
            return activeBoss;
        }
        
        private Vector2 GetRandomSpawnPosition()
        {
            var player = GetTree().CurrentScene?.GetNode<Player>("Player");
            if (player == null) return Vector2.Zero;
            
            var random = new Random();
            float angle = (float)(random.NextDouble() * Math.PI * 2);
            float distance = (float)(random.NextDouble() * 500 + 300);
            
            return player.GlobalPosition + new Vector2(
                (float)Math.Cos(angle) * distance,
                (float)Math.Sin(angle) * distance
            );
        }
        
        /// <summary>
        /// Deal damage to a world boss
        /// </summary>
        public void DealDamage(string bossInstanceId, string playerId, string playerName, int damage)
        {
            var boss = GetBossByInstanceId(bossInstanceId);
            if (boss == null || boss.IsDefeated) return;
            
            boss.CurrentHealth -= damage;
            boss.TotalDamageDealt += damage;
            
            // 委托给伤害追踪系统
            if (_damageTracker != null)
            {
                _damageTracker.RecordDamage(bossInstanceId, playerId, damage);
            }
            
            // Check if boss is defeated
            if (boss.CurrentHealth <= 0)
            {
                DefeatBoss(boss);
            }
        }
        
        private void DefeatBoss(WorldBossData.ActiveWorldBoss boss)
        {
            boss.IsDefeated = true;
            
            // 委托给奖励系统
            if (_rewardSystem != null && _damageTracker != null)
            {
                var damageRecords = _damageTracker.GetDamageRecords(boss.InstanceId);
                _rewardSystem.ProcessBossDefeat(boss, damageRecords);
            }
            
            _activeBosses.Remove(boss);
            if (_spawner != null)
            {
                _spawner.UnregisterBoss(boss.InstanceId);
            }
            
            GD.Print($"[WorldBossSystem] {boss.Name} defeated!");
        }
        
        /// <summary>
        /// Update player count for a boss
        /// </summary>
        public void UpdatePlayerCount(string bossInstanceId, int count)
        {
            var boss = GetBossByInstanceId(bossInstanceId);
            if (boss != null)
            {
                boss.PlayerCount = count;
            }
        }
        
        private WorldBossData.ActiveWorldBoss GetBossByInstanceId(string instanceId)
        {
            foreach (var boss in _activeBosses)
            {
                if (boss.InstanceId == instanceId)
                    return boss;
            }
            return null;
        }
        
        /// <summary>
        /// Get all active bosses
        /// </summary>
        public List<WorldBossData.ActiveWorldBoss> GetActiveBosses()
        {
            return new List<WorldBossData.ActiveWorldBoss>(_activeBosses);
        }
        
        /// <summary>
        /// Get damage records for a boss
        /// </summary>
        public List<WorldBossData.PlayerDamageRecord> GetDamageRecords(string bossInstanceId)
        {
            if (_damageTracker != null)
                return _damageTracker.GetDamageRecords(bossInstanceId);
            return new List<WorldBossData.PlayerDamageRecord>();
        }
        
        /// <summary>
        /// Get top damage dealers
        /// </summary>
        public List<WorldBossData.PlayerDamageRecord> GetTopDamageDealers(string bossInstanceId, int count = 10)
        {
            if (_damageTracker != null)
                return _damageTracker.GetTopDamageDealers(bossInstanceId, count);
            return new List<WorldBossData.PlayerDamageRecord>();
        }
        
        /// <summary>
        /// Get player damage rank
        /// </summary>
        public int GetPlayerRank(string bossInstanceId, string playerId)
        {
            if (_damageTracker != null)
                return _damageTracker.GetPlayerRank(bossInstanceId, playerId);
            return -1;
        }
        
        /// <summary>
        /// Get player statistics
        /// </summary>
        public WorldBossData.PlayerWorldBossStats GetPlayerStats(string playerId)
        {
            if (_damageTracker != null)
                return _damageTracker.GetPlayerStats(playerId);
            return new WorldBossData.PlayerWorldBossStats { PlayerId = playerId };
        }
        
        /// <summary>
        /// Get kill history
        /// </summary>
        public List<WorldBossData.BossKillRecord> GetKillHistory()
        {
            if (_rewardSystem != null)
                return _rewardSystem.GetKillHistory();
            return new List<WorldBossData.BossKillRecord>();
        }
        
        /// <summary>
        /// Get next spawn time for a boss
        /// </summary>
        public DateTime? GetNextSpawnTime(string bossId)
        {
            if (_spawner != null)
                return _spawner.GetNextSpawnTime(bossId);
            return null;
        }
        
        /// <summary>
        /// Check if a boss type is currently active
        /// </summary>
        public bool IsBossActive(string bossId)
        {
            if (_spawner != null)
                return _spawner.IsBossActive(bossId);
            return false;
        }
        
        /// <summary>
        /// Get all player stats
        /// </summary>
        public Dictionary<string, WorldBossData.PlayerWorldBossStats> GetAllPlayerStats()
        {
            if (_damageTracker != null)
                return _damageTracker.GetAllPlayerStats();
            return new Dictionary<string, WorldBossData.PlayerWorldBossStats>();
        }
        
        /// <summary>
        /// Check if there are any active bosses
        /// </summary>
        public bool HasActiveBosses()
        {
            return _activeBosses.Count > 0;
        }
        
        /// <summary>
        /// Get total bosses killed
        /// </summary>
        public int GetTotalBossesKilled()
        {
            if (_rewardSystem != null)
                return _rewardSystem.GetTotalKills();
            return 0;
        }
        
        /// <summary>
        /// Force spawn a boss (for testing or events)
        /// </summary>
        public WorldBossData.ActiveWorldBoss ForceSpawnBoss(string bossId)
        {
            var boss = WorldBossDatabase.GetBoss(bossId);
            if (boss == null) return null;
            
            // Update spawn timer
            if (_spawner != null)
            {
                _spawner.SetNextSpawnTime(bossId, DateTime.Now.AddMinutes(boss.SpawnIntervalMinutes));
            }
            
            return SpawnBoss(bossId);
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 导出子系统数据
            if (_spawner != null)
            {
                data["spawner"] = _spawner.ExportSaveData();
            }
            if (_damageTracker != null)
            {
                data["damage_tracker"] = _damageTracker.ExportSaveData();
            }
            if (_rewardSystem != null)
            {
                data["reward_system"] = _rewardSystem.ExportSaveData();
            }
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 导入子系统数据
            if (data.Contains("spawner") && _spawner != null)
            {
                _spawner.ImportSaveData((Dictionary)data["spawner"]);
            }
            if (data.Contains("damage_tracker") && _damageTracker != null)
            {
                _damageTracker.ImportSaveData((Dictionary)data["damage_tracker"]);
            }
            if (data.Contains("reward_system") && _rewardSystem != null)
            {
                _rewardSystem.ImportSaveData((Dictionary)data["reward_system"]);
            }
            
            GD.Print("[WorldBossSystem] Save data imported");
        }
    }
}
