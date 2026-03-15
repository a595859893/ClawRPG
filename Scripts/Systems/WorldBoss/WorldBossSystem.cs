using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.WorldBoss;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// World boss system manager - handles world boss spawning and combat
    /// </summary>
    public partial class WorldBossSystem : BaseSystem
    {
        public static WorldBossSystem Instance { get; private set; }
        
        // Active world bosses
        private List<WorldBossData.ActiveWorldBoss> _activeBosses = new List<WorldBossData.ActiveWorldBoss>();
        
        // Player damage records for each boss
        private Dictionary<string, List<WorldBossData.PlayerDamageRecord>> _bossDamageRecords = new Dictionary<string, List<WorldBossData.PlayerDamageRecord>>();
        
        // Player statistics
        private Dictionary<string, WorldBossData.PlayerWorldBossStats> _playerStats = new Dictionary<string, WorldBossData.PlayerWorldBossStats>();
        
        // Boss kill history
        private List<WorldBossData.BossKillRecord> _killHistory = new List<WorldBossData.BossKillRecord>();
        
        // Spawn timers for each boss type
        private Dictionary<string, DateTime> _nextSpawnTime = new Dictionary<string, DateTime>();
        
        // Random for boss spawning
        private Random _random = new Random();
        
        // Signals
        [Signal] public delegate void BossSpawnedEventHandler(WorldBossData.ActiveWorldBoss boss);
        [Signal] public delegate void BossDamagedEventHandler(string bossInstanceId, string playerId, int damage);
        [Signal] public delegate void BossDefeatedEventHandler(WorldBossData.ActiveWorldBoss boss);
        [Signal] public delegate void BossEscapedEventHandler(WorldBossData.ActiveWorldBoss boss);
        
        public override void _Ready()
        {
            Instance = this;
            WorldBossDatabase.Initialize();
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
        
        private bool IsBossActive(string bossId)
        {
            foreach (var boss in _activeBosses)
            {
                if (boss.BossId == bossId && !boss.IsDefeated)
                    return true;
            }
            return false;
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
            _bossDamageRecords[activeBoss.InstanceId] = new List<WorldBossData.PlayerDamageRecord>();
            
            // Notify player of boss spawn
            if (player != null)
            {
                // Could show notification here
            }
            
            EmitSignal(SignalName.BossSpawned, activeBoss);
            
            GD.Print($"[WorldBoss] {bossTemplate.Name} spawned at ({spawnPos.x}, {spawnPos.y})!");
            
            return activeBoss;
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
            
            // Update player damage record
            if (!_bossDamageRecords.ContainsKey(bossInstanceId))
                _bossDamageRecords[bossInstanceId] = new List<WorldBossData.PlayerDamageRecord>();
            
            var records = _bossDamageRecords[bossInstanceId];
            var playerRecord = records.Find(r => r.PlayerId == playerId);
            
            if (playerRecord == null)
            {
                playerRecord = new WorldBossData.PlayerDamageRecord
                {
                    PlayerId = playerId,
                    PlayerName = playerName,
                    DamageDealt = 0
                };
                records.Add(playerRecord);
            }
            
            playerRecord.DamageDealt += damage;
            playerRecord.LastHitTime = DateTime.Now;
            
            // Update damage percentage
            float totalDamage = boss.TotalDamageDealt;
            foreach (var record in records)
            {
                record.DamagePercent = totalDamage > 0 ? (float)record.DamageDealt / totalDamage * 100f : 0f;
            }
            
            EmitSignal(SignalName.BossDamaged, bossInstanceId, playerId, damage);
            
            // Check if boss is defeated
            if (boss.CurrentHealth <= 0)
            {
                DefeatBoss(boss);
            }
        }
        
        private void DefeatBoss(WorldBossData.ActiveWorldBoss boss)
        {
            boss.IsDefeated = true;
            
            var bossTemplate = WorldBossDatabase.GetBoss(boss.BossId);
            if (bossTemplate == null) return;
            
            // Calculate rewards
            var damageRecords = _bossDamageRecords.ContainsKey(boss.InstanceId) 
                ? _bossDamageRecords[boss.InstanceId] 
                : new List<WorldBossData.PlayerDamageRecord>();
            
            float rarityMultiplier = WorldBossDatabase.GetRarityMultiplier(boss.Rarity);
            
            foreach (var record in damageRecords)
            {
                if (record.DamageDealt > 0)
                {
                    int goldReward = (int)(bossTemplate.GoldReward * (record.DamagePercent / 100f) * rarityMultiplier);
                    int expReward = (int)(bossTemplate.ExpReward * (record.DamagePercent / 100f) * rarityMultiplier);
                    
                    // Add to player stats
                    AddPlayerStats(record.PlayerId, goldReward, expReward, boss.BossId, boss.Rarity);
                }
            }
            
            // Record kill history
            var killRecord = new WorldBossData.BossKillRecord
            {
                BossId = boss.BossId,
                BossName = boss.BossName,
                Rarity = boss.Rarity,
                KillTime = DateTime.Now,
                TotalDamage = boss.TotalDamageDealt,
                KillerCount = damageRecords.Count,
                TotalGoldReward = (int)(bossTemplate.GoldReward * rarityMultiplier),
                TotalExpReward = (int)(bossTemplate.ExpReward * rarityMultiplier)
            };
            _killHistory.Add(killRecord);
            
            // Limit history size
            if (_killHistory.Count > 100)
                _killHistory.RemoveAt(0);
            
            EmitSignal(SignalName.BossDefeated, boss);
            
            GD.Print($"[WorldBoss] {boss.BossName} defeated! Total damage: {boss.TotalDamageDealt}, Killers: {damageRecords.Count}");
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
            if (_bossDamageRecords.ContainsKey(bossInstanceId))
                return new List<WorldBossData.PlayerDamageRecord>(_bossDamageRecords[bossInstanceId]);
            return new List<WorldBossData.PlayerDamageRecord>();
        }
        
        /// <summary>
        /// Get player statistics
        /// </summary>
        public WorldBossData.PlayerWorldBossStats GetPlayerStats(string playerId)
        {
            if (!_playerStats.ContainsKey(playerId))
            {
                _playerStats[playerId] = new WorldBossData.PlayerWorldBossStats { PlayerId = playerId };
            }
            return _playerStats[playerId];
        }
        
        /// <summary>
        /// Get kill history
        /// </summary>
        public List<WorldBossData.BossKillRecord> GetKillHistory()
        {
            return new List<WorldBossData.BossKillRecord>(_killHistory);
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
        
        private void AddPlayerStats(string playerId, int gold, int exp, string bossId, WorldBossData.BossRarity rarity)
        {
            var stats = GetPlayerStats(playerId);
            stats.TotalGoldEarned += gold;
            stats.TotalExpEarned += exp;
            
            if (!stats.BossKillCount.ContainsKey(bossId))
                stats.BossKillCount[bossId] = 0;
            stats.BossKillCount[bossId]++;
            
            string rarityKey = rarity.ToString();
            if (!stats.RarityKillCount.ContainsKey(rarityKey))
                stats.RarityKillCount[rarityKey] = 0;
            stats.RarityKillCount[rarityKey]++;
        }
        
        /// <summary>
        /// Get all player stats
        /// </summary>
        public Dictionary<string, WorldBossData.PlayerWorldBossStats> GetAllPlayerStats()
        {
            return new Dictionary<string, WorldBossData.PlayerWorldBossStats>(_playerStats);
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
        /// Get total bosses killed
        /// </summary>
        public int GetTotalBossesKilled()
        {
            return _killHistory.Count;
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
        /// Get save data
        /// </summary>
        public Dictionary<string, object> GetSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // Save spawn timers
            var timers = new Dictionary<string, string>();
            foreach (var kvp in _nextSpawnTime)
            {
                timers[kvp.Key] = kvp.Value.ToString("o");
            }
            data["spawn_timers"] = timers;
            
            // Save kill history
            var history = new List<Dictionary<string, object>>();
            foreach (var record in _killHistory)
            {
                history.Add(new Dictionary<string, object>
                {
                    { "boss_id", record.BossId },
                    { "boss_name", record.BossName },
                    { "rarity", (int)record.Rarity },
                    { "kill_time", record.KillTime.ToString("o") },
                    { "total_damage", record.TotalDamage },
                    { "killer_count", record.KillerCount },
                    { "total_gold", record.TotalGoldReward },
                    { "total_exp", record.TotalExpReward }
                });
            }
            data["kill_history"] = history;
            
            // Save player stats
            var stats = new Dictionary<string, Dictionary<string, object>>();
            foreach (var kvp in _playerStats)
            {
                stats[kvp.Key] = new Dictionary<string, object>
                {
                    { "total_killed", kvp.Value.TotalBossesKilled },
                    { "total_damage", kvp.Value.TotalDamageDealt },
                    { "total_gold", kvp.Value.TotalGoldEarned },
                    { "total_exp", kvp.Value.TotalExpEarned }
                };
            }
            data["player_stats"] = stats;
            
            return data;
        }
        
        /// <summary>
        /// Load save data
        /// </summary>
        public void LoadSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // Load spawn timers
            if (data.ContainsKey("spawn_timers"))
            {
                var timers = data["spawn_timers"] as Dictionary<string, object>;
                if (timers != null)
                {
                    foreach (var kvp in timers)
                    {
                        if (DateTime.TryParse(kvp.Value?.ToString(), out DateTime dt))
                        {
                            _nextSpawnTime[kvp.Key] = dt;
                        }
                    }
                }
            }
            
            // Load kill history
            if (data.ContainsKey("kill_history"))
            {
                var history = data["kill_history"] as Array;
                if (history != null)
                {
                    _killHistory.Clear();
                    foreach (var item in history)
                    {
                        var dict = item as Dictionary<string, object>;
                        if (dict != null)
                        {
                            var record = new WorldBossData.BossKillRecord
                            {
                                BossId = dict.GetValueOrDefault("boss_id", "")?.ToString() ?? "",
                                BossName = dict.GetValueOrDefault("boss_name", "")?.ToString() ?? "",
                                Rarity = (WorldBossData.BossRarity)(dict.GetValueOrDefault("rarity", 0)),
                                KillTime = DateTime.TryParse(dict.GetValueOrDefault("kill_time", "")?.ToString(), out DateTime kt) ? kt : DateTime.Now,
                                TotalDamage = Convert.ToInt32(dict.GetValueOrDefault("total_damage", 0)),
                                KillerCount = Convert.ToInt32(dict.GetValueOrDefault("killer_count", 0)),
                                TotalGoldReward = Convert.ToInt32(dict.GetValueOrDefault("total_gold", 0)),
                                TotalExpReward = Convert.ToInt32(dict.GetValueOrDefault("total_exp", 0))
                            };
                            _killHistory.Add(record);
                        }
                    }
                }
            }
            
            // Load player stats
            if (data.ContainsKey("player_stats"))
            {
                var stats = data["player_stats"] as Dictionary<string, object>;
                if (stats != null)
                {
                    _playerStats.Clear();
                    foreach (var kvp in stats)
                    {
                        var dict = kvp.Value as Dictionary<string, object>;
                        if (dict != null)
                        {
                            _playerStats[kvp.Key] = new WorldBossData.PlayerWorldBossStats
                            {
                                PlayerId = kvp.Key,
                                TotalBossesKilled = Convert.ToInt32(dict.GetValueOrDefault("total_killed", 0)),
                                TotalDamageDealt = Convert.ToInt32(dict.GetValueOrDefault("total_damage", 0)),
                                TotalGoldEarned = Convert.ToInt32(dict.GetValueOrDefault("total_gold", 0)),
                                TotalExpEarned = Convert.ToInt32(dict.GetValueOrDefault("total_exp", 0))
                            };
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // 保存生成计时器
            var timers = new Dictionary();
            foreach (var kvp in _nextSpawnTime)
            {
                timers[kvp.Key] = kvp.Value.ToString("o");
            }
            data["spawn_timers"] = timers;
            
            // 保存击杀历史
            var history = new Array();
            foreach (var record in _killHistory)
            {
                var recordDict = new Dictionary
                {
                    { "boss_id", record.BossId },
                    { "boss_name", record.BossName },
                    { "rarity", (int)record.Rarity },
                    { "kill_time", record.KillTime.ToString("o") },
                    { "total_damage", record.TotalDamage },
                    { "killer_count", record.KillerCount },
                    { "total_gold", record.TotalGoldReward },
                    { "total_exp", record.TotalExpReward }
                };
                history.Add(recordDict);
            }
            data["kill_history"] = history;
            
            // 保存玩家统计
            var stats = new Dictionary();
            foreach (var kvp in _playerStats)
            {
                var statsDict = new Dictionary
                {
                    { "total_killed", kvp.Value.TotalBossesKilled },
                    { "total_damage", kvp.Value.TotalDamageDealt },
                    { "total_gold", kvp.Value.TotalGoldEarned },
                    { "total_exp", kvp.Value.TotalExpEarned }
                };
                stats[kvp.Key] = statsDict;
            }
            data["player_stats"] = stats;
            
            return data;
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // 加载生成计时器
            if (data.Contains("spawn_timers"))
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
            
            // 加载击杀历史
            if (data.Contains("kill_history"))
            {
                var history = data["kill_history"] as Array;
                if (history != null)
                {
                    _killHistory.Clear();
                    foreach (var item in history)
                    {
                        var dict = item as Dictionary;
                        if (dict != null)
                        {
                            var record = new WorldBossData.BossKillRecord
                            {
                                BossId = (string)dict.GetValueOrDefault("boss_id", ""),
                                BossName = (string)dict.GetValueOrDefault("boss_name", ""),
                                Rarity = (WorldBossData.BossRarity)(dict.GetValueOrDefault("rarity", 0)),
                                KillTime = DateTime.TryParse(dict.GetValueOrDefault("kill_time", "")?.ToString(), out DateTime kt) ? kt : DateTime.Now,
                                TotalDamage = Convert.ToInt32(dict.GetValueOrDefault("total_damage", 0)),
                                KillerCount = Convert.ToInt32(dict.GetValueOrDefault("killer_count", 0)),
                                TotalGoldReward = Convert.ToInt32(dict.GetValueOrDefault("total_gold", 0)),
                                TotalExpReward = Convert.ToInt32(dict.GetValueOrDefault("total_exp", 0))
                            };
                            _killHistory.Add(record);
                        }
                    }
                }
            }
            
            // 加载玩家统计
            if (data.Contains("player_stats"))
            {
                var stats = data["player_stats"] as Dictionary;
                if (stats != null)
                {
                    _playerStats.Clear();
                    foreach (var kvp in stats)
                    {
                        var dict = kvp.Value as Dictionary;
                        if (dict != null)
                        {
                            _playerStats[kvp.Key] = new WorldBossData.PlayerWorldBossStats
                            {
                                PlayerId = kvp.Key,
                                TotalBossesKilled = Convert.ToInt32(dict.GetValueOrDefault("total_killed", 0)),
                                TotalDamageDealt = Convert.ToInt32(dict.GetValueOrDefault("total_damage", 0)),
                                TotalGoldEarned = Convert.ToInt32(dict.GetValueOrDefault("total_gold", 0)),
                                TotalExpEarned = Convert.ToInt32(dict.GetValueOrDefault("total_exp", 0))
                            };
                        }
                    }
                }
            }
            
            GD.Print("[WorldBossSystem] Save data imported");
        }
    }
}
