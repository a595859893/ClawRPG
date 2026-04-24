using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.WorldBoss;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// World boss spawner system - handles world boss spawning and lifecycle
    /// </summary>
    public partial class WorldBossSpawner : BaseSystem
    {
        public static WorldBossSpawner Instance { get; private set; }
        
        // Active world bosses
        private List<WorldBossData.ActiveWorldBoss> _activeBosses = new List<WorldBossData.ActiveWorldBoss>();
        
        // Spawn timers for each boss type
        private Dictionary<string, DateTime> _nextSpawnTime = new Dictionary<string, DateTime>();
        
        // Random for boss spawning
        private Random _random = new Random();
        
        // Signals
        public delegate void BossSpawnedEventHandler(WorldBossData.ActiveWorldBoss boss);
        public delegate void BossEscapedEventHandler(WorldBossData.ActiveWorldBoss boss);
        public delegate void BossDefeatedEventHandler(WorldBossData.ActiveWorldBoss boss);
        
        public override void _Ready()
        {
            Instance = this;
            InitializeSpawnTimers();
        }
        
        private void InitializeSpawnTimers()
        {
            var bosses = WorldBossDatabase.GetAllBosses();
            foreach (var boss in bosses)
            {
                // Schedule first spawn
                int delayMinutes = _random.Next(0, (int)boss.SpawnIntervalMinutes);
                _nextSpawnTime[boss.Id] = DateTime.Now.AddMinutes(delayMinutes);
            }
        }
        
        public override void _Process(double delta)
        {
            CheckBossSpawning();
            ProcessActiveBosses(delta);
        }
        
        private void CheckBossSpawning()
        {
            var bosses = WorldBossDatabase.GetAllBosses();
            foreach (var boss in bosses)
            {
                if (boss.SpawnType == WorldBossData.SpawnCondition.Timer)
                {
                    if (!_nextSpawnTime.ContainsKey(boss.Id))
                    {
                        _nextSpawnTime[boss.Id] = DateTime.Now;
                    }
                    
                    if (DateTime.Now >= _nextSpawnTime[boss.Id] && !IsBossActive(boss.Id))
                    {
                        // Check if boss can spawn based on player level
                        var player = GetTree().CurrentScene?.GetNode<Player>("Player");
                        if (player != null && player.Level >= boss.Level - 10)
                        {
                            SpawnBoss(boss.Id);
                            _nextSpawnTime[boss.Id] = DateTime.Now.AddMinutes(boss.SpawnIntervalMinutes);
                        }
                    }
                }
                else if (boss.SpawnType == WorldBossData.SpawnCondition.Random)
                {
                    // Random spawn chance
                    if (_random.NextDouble() < 0.001 && !IsBossActive(boss.Id)) // ~1.7% per minute
                    {
                        var player = GetTree().CurrentScene?.GetNode<Player>("Player");
                        if (player != null && player.Level >= boss.Level - 10)
                        {
                            SpawnBoss(boss.Id);
                        }
                    }
                }
            }
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
                    // Boss escaped
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
            }
        }
        
        /// <summary>
        /// Check if a boss type is currently active
        /// </summary>
        public bool IsBossActive(string bossId)
        {
            foreach (var boss in _activeBosses)
            {
                if (boss.BossId == bossId && !boss.IsDefeated)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Spawn a world boss
        /// </summary>
        public WorldBossData.ActiveWorldBoss SpawnBoss(string bossId)
        {
            var bossTemplate = WorldBossDatabase.GetBoss(bossId);
            if (bossTemplate == null) return null;
            
            // Get player position for spawn location
            Vector2 spawnPos = new Vector2(0, 0);
            var player = GetTree().CurrentScene?.GetNode<Player>("Player");
            if (player != null)
            {
                float angle = (float)(_random.NextDouble() * Math.PI * 2);
                float distance = (float)(bossTemplate.SpawnRadius * (0.5 + _random.NextDouble() * 0.5));
                spawnPos = player.GlobalPosition + new Vector2(
                    Mathf.Cos(angle) * distance,
                    Mathf.Sin(angle) * distance
                );
            }
            
            var activeBoss = new WorldBossData.ActiveWorldBoss
            {
                BossId = bossId,
                BossName = bossTemplate.Name,
                Rarity = bossTemplate.Rarity,
                CurrentHealth = bossTemplate.Health,
                MaxHealth = bossTemplate.Health,
                X = spawnPos.x,
                Y = spawnPos.y,
                SpawnTime = DateTime.Now,
                LifeTimeMinutes = bossTemplate.SpawnIntervalMinutes > 0 ? bossTemplate.SpawnIntervalMinutes : 30,
                IsDefeated = false,
                TotalDamageDealt = 0,
                PlayerCount = 1
            };
            
            _activeBosses.Add(activeBoss);
            
            EmitSignal(SignalName.BossSpawned, activeBoss);
            
            GD.Print($"[WorldBossSpawner] {bossTemplate.Name} spawned at ({spawnPos.x}, {spawnPos.y})!");
            
            return activeBoss;
        }
        
        /// <summary>
        /// Mark boss as defeated
        /// </summary>
        public void MarkBossDefeated(string bossInstanceId)
        {
            var boss = GetBossByInstanceId(bossInstanceId);
            if (boss != null)
            {
                boss.IsDefeated = true;
                EmitSignal(SignalName.BossDefeated, boss);
            }
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
        
        /// <summary>
        /// Get boss by instance ID
        /// </summary>
        public WorldBossData.ActiveWorldBoss GetBossByInstanceId(string instanceId)
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
        /// Check if there are any active bosses
        /// </summary>
        public bool HasActiveBosses()
        {
            foreach (var boss in _activeBosses)
            {
                if (!boss.IsDefeated)
                    return true;
            }
            return false;
        }
        
        /// <summary>
        /// Get next spawn time for a boss
        /// </summary>
        public DateTime? GetNextSpawnTime(string bossId)
        {
            if (_nextSpawnTime.ContainsKey(bossId))
                return _nextSpawnTime[bossId];
            return null;
        }
        
        /// <summary>
        /// Get all spawn timers
        /// </summary>
        public Dictionary<string, DateTime> GetAllSpawnTimers()
        {
            return new Dictionary<string, DateTime>(_nextSpawnTime);
        }
        
        /// <summary>
        /// Set spawn timer for a boss
        /// </summary>
        public void SetSpawnTimer(string bossId, DateTime time)
        {
            _nextSpawnTime[bossId] = time;
        }
        
        /// <summary>
        /// Force spawn a boss (for testing or events)
        /// </summary>
        public WorldBossData.ActiveWorldBoss ForceSpawnBoss(string bossId)
        {
            var boss = WorldBossDatabase.GetBoss(bossId);
            if (boss == null) return null;
            
            // Update spawn timer
            _nextSpawnTime[bossId] = DateTime.Now.AddMinutes(boss.SpawnIntervalMinutes);
            
            return SpawnBoss(bossId);
        }
        
        /// <summary>
        /// Update boss health
        /// </summary>
        public void UpdateBossHealth(string bossInstanceId, int damage)
        {
            var boss = GetBossByInstanceId(bossInstanceId);
            if (boss != null)
            {
                boss.CurrentHealth -= damage;
                boss.TotalDamageDealt += damage;
            }
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 保存生成计时器
            var timers = new Dictionary<string, object>();
            foreach (var kvp in _nextSpawnTime)
            {
                timers[kvp.Key] = kvp.Value.ToString("o");
            }
            data["spawn_timers"] = timers;
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 加载生成计时器
            if (data.ContainsKey("spawn_timers"))
            {
                var timers = data["spawn_timers"] as Dictionary;
                if (timers != null)
                {
                    _nextSpawnTime.Clear();
                    foreach (var kvp in timers)
                    {
                        if (DateTime.TryParse(kvp.Value?.ToString(), out DateTime dt))
                        {
                            _nextSpawnTime[kvp.Key] = dt;
                        }
                    }
                }
            }
            
            GD.Print("[WorldBossSpawner] Save data imported");
        }
    }
}
