using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.WorldBoss;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// World boss system coordinator - delegates to spawner, damage tracker, and reward system
    /// </summary>
    public partial class WorldBossSystem : BaseSystem
    {
        public static WorldBossSystem Instance { get; private set; }
        
        // Subsystems
        private WorldBossSpawner _spawner;
        private WorldBossDamageTracker _damageTracker;
        private WorldBossRewardSystem _rewardSystem;
        
        // Signals (forwarded from subsystems)
        [Signal] public delegate void BossSpawnedEventHandler(WorldBossData.ActiveWorldBoss boss);
        [Signal] public delegate void BossDamagedEventHandler(string bossInstanceId, string playerId, int damage);
        [Signal] public delegate void BossDefeatedEventHandler(WorldBossData.ActiveWorldBoss boss);
        [Signal] public delegate void BossEscapedEventHandler(WorldBossData.ActiveWorldBoss boss);
        
        public override void _Ready()
        {
            Instance = this;
            WorldBossDatabase.Initialize();
            
            // Get or create subsystems
            _spawner = GetNode<WorldBossSpawner>("WorldBossSpawner") ?? GetParent()?.GetNode<WorldBossSpawner>("WorldBossSpawner");
            _damageTracker = GetNode<WorldBossDamageTracker>("WorldBossDamageTracker") ?? GetParent()?.GetNode<WorldBossDamageTracker>("WorldBossDamageTracker");
            _rewardSystem = GetNode<WorldBossRewardSystem>("WorldBossRewardSystem") ?? GetParent()?.GetNode<WorldBossRewardSystem>("WorldBossRewardSystem");
            
            // Connect subsystem signals
            ConnectToSubsystems();
        }
        
        private void ConnectToSubsystems()
        {
            if (_spawner != null)
            {
                _spawner.BossSpawned += OnBossSpawned;
                _spawner.BossEscaped += OnBossEscaped;
                _spawner.BossDefeated += OnBossDefeated;
            }
            
            if (_damageTracker != null)
            {
                _damageTracker.BossDamaged += OnBossDamaged;
            }
        }
        
        private void OnBossSpawned(WorldBossData.ActiveWorldBoss boss)
        {
            EmitSignal(SignalName.BossSpawned, boss);
        }
        
        private void OnBossEscaped(WorldBossData.ActiveWorldBoss boss)
        {
            EmitSignal(SignalName.BossEscaped, boss);
        }
        
        private void OnBossDefeated(WorldBossData.ActiveWorldBoss boss)
        {
            EmitSignal(SignalName.BossDefeated, boss);
        }
        
        private void OnBossDamaged(string bossInstanceId, string playerId, int damage)
        {
            EmitSignal(SignalName.BossDamaged, bossInstanceId, playerId, damage);
        }
        
        /// <summary>
        /// Get the spawner subsystem
        /// </summary>
        public WorldBossSpawner GetSpawner()
        {
            return _spawner;
        }
        
        /// <summary>
        /// Get the damage tracker subsystem
        /// </summary>
        public WorldBossDamageTracker GetDamageTracker()
        {
            return _damageTracker;
        }
        
        /// <summary>
        /// Get the reward system subsystem
        /// </summary>
        public WorldBossRewardSystem GetRewardSystem()
        {
            return _rewardSystem;
        }
        
        // ==================== Spawner API ====================
        
        /// <summary>
        /// Spawn a world boss
        /// </summary>
        public WorldBossData.ActiveWorldBoss SpawnBoss(string bossId)
        {
            if (_spawner == null) return null;
            
            var boss = _spawner.SpawnBoss(bossId);
            if (boss != null)
            {
                _damageTracker?.InitializeBossDamageRecords(boss.InstanceId);
            }
            return boss;
        }
        
        /// <summary>
        /// Get all active bosses
        /// </summary>
        public List<WorldBossData.ActiveWorldBoss> GetActiveBosses()
        {
            if (_spawner != null)
                return _spawner.GetActiveBosses();
            return new List<WorldBossData.ActiveWorldBoss>();
        }
        
        /// <summary>
        /// Check if there are any active bosses
        /// </summary>
        public bool HasActiveBosses()
        {
            if (_spawner != null)
                return _spawner.HasActiveBosses();
            return false;
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
        /// Get all spawn timers
        /// </summary>
        public Dictionary<string, DateTime> GetAllSpawnTimers()
        {
            if (_spawner != null)
                return _spawner.GetAllSpawnTimers();
            return new Dictionary<string, DateTime>();
        }
        
        /// <summary>
        /// Force spawn a boss (for testing or events)
        /// </summary>
        public WorldBossData.ActiveWorldBoss ForceSpawnBoss(string bossId)
        {
            if (_spawner == null) return null;
            
            var boss = _spawner.ForceSpawnBoss(bossId);
            if (boss != null)
            {
                _damageTracker?.InitializeBossDamageRecords(boss.InstanceId);
            }
            return boss;
        }
        
        /// <summary>
        /// Update player count for a boss
        /// </summary>
        public void UpdatePlayerCount(string bossInstanceId, int count)
        {
            _spawner?.UpdatePlayerCount(bossInstanceId, count);
        }
        
        // ==================== Damage Tracker API ====================
        
        /// <summary>
        /// Deal damage to a world boss
        /// </summary>
        public void DealDamage(string bossInstanceId, string playerId, string playerName, int damage)
        {
            if (_spawner == null || _damageTracker == null) return;
            
            var boss = _spawner.GetBossByInstanceId(bossInstanceId);
            if (boss == null || boss.IsDefeated) return;
            
            // Update boss health via spawner
            _spawner.UpdateBossHealth(bossInstanceId, damage);
            
            // Update damage record via damage tracker
            _damageTracker.DealDamage(bossInstanceId, playerId, playerName, damage, boss.TotalDamageDealt);
            
            // Check if boss is defeated
            if (boss.CurrentHealth <= 0)
            {
                DefeatBoss(boss);
            }
        }
        
        private void DefeatBoss(WorldBossData.ActiveWorldBoss boss)
        {
            if (_spawner == null || _damageTracker == null || _rewardSystem == null) return;
            
            // Mark boss as defeated
            _spawner.MarkBossDefeated(boss.InstanceId);
            
            // Get damage records
            var damageRecords = _damageTracker.GetDamageRecords(boss.InstanceId);
            
            // Distribute rewards
            _rewardSystem.DistributeRewards(boss.InstanceId, boss.BossId, boss.Rarity, damageRecords);
            
            // Clear damage records
            _damageTracker.ClearDamageRecords(boss.InstanceId);
            
            GD.Print($"[WorldBoss] {boss.BossName} defeated! Total damage: {boss.TotalDamageDealt}, Killers: {damageRecords.Count}");
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
        /// Get damage rankings for a boss
        /// </summary>
        public List<WorldBossData.PlayerDamageRecord> GetDamageRankings(string bossInstanceId)
        {
            if (_damageTracker != null)
                return _damageTracker.GetDamageRankings(bossInstanceId);
            return new List<WorldBossData.PlayerDamageRecord>();
        }
        
        /// <summary>
        /// Get player rank for a boss
        /// </summary>
        public int GetPlayerRank(string bossInstanceId, string playerId)
        {
            if (_damageTracker != null)
                return _damageTracker.GetPlayerRank(bossInstanceId, playerId);
            return -1;
        }
        
        // ==================== Reward System API ====================
        
        /// <summary>
        /// Get player statistics
        /// </summary>
        public WorldBossData.PlayerWorldBossStats GetPlayerStats(string playerId)
        {
            if (_rewardSystem != null)
                return _rewardSystem.GetPlayerStats(playerId);
            return null;
        }
        
        /// <summary>
        /// Get all player stats
        /// </summary>
        public Dictionary<string, WorldBossData.PlayerWorldBossStats> GetAllPlayerStats()
        {
            if (_rewardSystem != null)
                return _rewardSystem.GetAllPlayerStats();
            return new Dictionary<string, WorldBossData.PlayerWorldBossStats>();
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
        /// Get total bosses killed
        /// </summary>
        public int GetTotalBossesKilled()
        {
            if (_rewardSystem != null)
                return _rewardSystem.GetTotalBossesKilled();
            return 0;
        }
        
        /// <summary>
        /// Get leaderboard by total bosses killed
        /// </summary>
        public List<WorldBossData.PlayerWorldBossStats> GetLeaderboardByKills(int limit = 10)
        {
            if (_rewardSystem != null)
                return _rewardSystem.GetLeaderboardByKills(limit);
            return new List<WorldBossData.PlayerWorldBossStats>();
        }
        
        /// <summary>
        /// Get leaderboard by total damage dealt
        /// </summary>
        public List<WorldBossData.PlayerWorldBossStats> GetLeaderboardByDamage(int limit = 10)
        {
            if (_rewardSystem != null)
                return _rewardSystem.GetLeaderboardByDamage(limit);
            return new List<WorldBossData.PlayerWorldBossStats>();
        }
        
        /// <summary>
        /// Get leaderboard by total gold earned
        /// </summary>
        public List<WorldBossData.PlayerWorldBossStats> GetLeaderboardByGold(int limit = 10)
        {
            if (_rewardSystem != null)
                return _rewardSystem.GetLeaderboardByGold(limit);
            return new List<WorldBossData.PlayerWorldBossStats>();
        }
        
        // ==================== Save Data ====================
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // Export from subsystems
            if (_spawner != null)
            {
                var spawnerData = _spawner.ExportSaveData();
                foreach (var kvp in spawnerData)
                {
                    data[kvp.Key] = kvp.Value;
                }
            }
            
            if (_rewardSystem != null)
            {
                var rewardData = _rewardSystem.ExportSaveData();
                foreach (var kvp in rewardData)
                {
                    data[kvp.Key] = kvp.Value;
                }
            }
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // Import to subsystems
            if (_spawner != null)
            {
                _spawner.ImportSaveData(data);
            }
            
            if (_rewardSystem != null)
            {
                _rewardSystem.ImportSaveData(data);
            }
            
            GD.Print("[WorldBossSystem] Save data imported");
        }
    }
}
