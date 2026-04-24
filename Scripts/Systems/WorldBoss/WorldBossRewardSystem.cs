using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.WorldBoss;
using ClawRPG.Scripts.Database;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// World boss reward system - handles reward distribution and statistics
    /// </summary>
    public partial class WorldBossRewardSystem : BaseSystem
    {
        public static WorldBossRewardSystem Instance { get; private set; }
        
        // Player statistics
        private Dictionary<string, WorldBossData.PlayerWorldBossStats> _playerStats = new Dictionary<string, WorldBossData.PlayerWorldBossStats>();
        
        // Boss kill history
        private List<WorldBossData.BossKillRecord> _killHistory = new List<WorldBossData.BossKillRecord>();
        
        public override void _Ready()
        {
            Instance = this;
        }
        
        /// <summary>
        /// Calculate and distribute rewards for a defeated boss
        /// </summary>
        public void DistributeRewards(string bossInstanceId, string bossId, WorldBossData.BossRarity rarity, List<WorldBossData.PlayerDamageRecord> damageRecords)
        {
            var bossTemplate = WorldBossDatabase.GetBoss(bossId);
            if (bossTemplate == null) return;
            
            float rarityMultiplier = WorldBossDatabase.GetRarityMultiplier(rarity);
            
            foreach (var record in damageRecords)
            {
                if (record.DamageDealt > 0)
                {
                    int goldReward = (int)(bossTemplate.GoldReward * (record.DamagePercent / 100f) * rarityMultiplier);
                    int expReward = (int)(bossTemplate.ExpReward * (record.DamagePercent / 100f) * rarityMultiplier);
                    
                    // Add to player stats
                    AddPlayerStats(record.PlayerId, goldReward, expReward, bossId, rarity);
                }
            }
            
            // Record kill history
            var killRecord = new WorldBossData.BossKillRecord
            {
                BossId = bossId,
                BossName = bossTemplate.Name,
                Rarity = rarity,
                KillTime = DateTime.Now,
                TotalDamage = 0,
                KillerCount = damageRecords.Count,
                TotalGoldReward = (int)(bossTemplate.GoldReward * rarityMultiplier),
                TotalExpReward = (int)(bossTemplate.ExpReward * rarityMultiplier)
            };
            
            // Calculate total damage
            foreach (var record in damageRecords)
            {
                killRecord.TotalDamage += record.DamageDealt;
            }
            
            _killHistory.Add(killRecord);
            
            // Limit history size
            if (_killHistory.Count > 100)
                _killHistory.RemoveAt(0);
            
            GD.Print($"[WorldBossRewardSystem] Rewards distributed for {bossTemplate.Name}, killers: {damageRecords.Count}");
        }
        
        /// <summary>
        /// Add player statistics
        /// </summary>
        private void AddPlayerStats(string playerId, int gold, int exp, string bossId, WorldBossData.BossRarity rarity)
        {
            var stats = GetPlayerStats(playerId);
            stats.TotalGoldEarned += gold;
            stats.TotalExpEarned += exp;
            
            if (!stats.BossKillCount.ContainsKey(bossId))
                stats.BossKillCount[bossId] = 0;
            stats.BossKillCount[bossId]++;
            
            stats.TotalBossesKilled++;
            stats.TotalDamageDealt += 0; // Will be updated from damage tracker
            
            string rarityKey = rarity.ToString();
            if (!stats.RarityKillCount.ContainsKey(rarityKey))
                stats.RarityKillCount[rarityKey] = 0;
            stats.RarityKillCount[rarityKey]++;
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
        /// Get all player stats
        /// </summary>
        public Dictionary<string, WorldBossData.PlayerWorldBossStats> GetAllPlayerStats()
        {
            return new Dictionary<string, WorldBossData.PlayerWorldBossStats>(_playerStats);
        }
        
        /// <summary>
        /// Get kill history
        /// </summary>
        public List<WorldBossData.BossKillRecord> GetKillHistory()
        {
            return new List<WorldBossData.BossKillRecord>(_killHistory);
        }
        
        /// <summary>
        /// Get total bosses killed
        /// </summary>
        public int GetTotalBossesKilled()
        {
            return _killHistory.Count;
        }
        
        /// <summary>
        /// Get total gold earned by a player
        /// </summary>
        public int GetPlayerTotalGold(string playerId)
        {
            if (_playerStats.ContainsKey(playerId))
                return _playerStats[playerId].TotalGoldEarned;
            return 0;
        }
        
        /// <summary>
        /// Get total exp earned by a player
        /// </summary>
        public int GetPlayerTotalExp(string playerId)
        {
            if (_playerStats.ContainsKey(playerId))
                return _playerStats[playerId].TotalExpEarned;
            return 0;
        }
        
        /// <summary>
        /// Get boss kill count for a player
        /// </summary>
        public int GetPlayerBossKillCount(string playerId, string bossId)
        {
            if (_playerStats.ContainsKey(playerId) && _playerStats[playerId].BossKillCount.ContainsKey(bossId))
                return _playerStats[playerId].BossKillCount[bossId];
            return 0;
        }
        
        /// <summary>
        /// Get rarity kill count for a player
        /// </summary>
        public int GetPlayerRarityKillCount(string playerId, WorldBossData.BossRarity rarity)
        {
            if (_playerStats.ContainsKey(playerId))
            {
                string rarityKey = rarity.ToString();
                if (_playerStats[playerId].RarityKillCount.ContainsKey(rarityKey))
                    return _playerStats[playerId].RarityKillCount[rarityKey];
            }
            return 0;
        }
        
        /// <summary>
        /// Get leaderboard by total bosses killed
        /// </summary>
        public List<WorldBossData.PlayerWorldBossStats> GetLeaderboardByKills(int limit = 10)
        {
            var statsList = new List<WorldBossData.PlayerWorldBossStats>(_playerStats.Values);
            statsList.Sort((a, b) => b.TotalBossesKilled.CompareTo(a.TotalBossesKilled));
            
            if (limit > 0 && statsList.Count > limit)
                return statsList.GetRange(0, limit);
            return statsList;
        }
        
        /// <summary>
        /// Get leaderboard by total damage dealt
        /// </summary>
        public List<WorldBossData.PlayerWorldBossStats> GetLeaderboardByDamage(int limit = 10)
        {
            var statsList = new List<WorldBossData.PlayerWorldBossStats>(_playerStats.Values);
            statsList.Sort((a, b) => b.TotalDamageDealt.CompareTo(a.TotalDamageDealt));
            
            if (limit > 0 && statsList.Count > limit)
                return statsList.GetRange(0, limit);
            return statsList;
        }
        
        /// <summary>
        /// Get leaderboard by total gold earned
        /// </summary>
        public List<WorldBossData.PlayerWorldBossStats> GetLeaderboardByGold(int limit = 10)
        {
            var statsList = new List<WorldBossData.PlayerWorldBossStats>(_playerStats.Values);
            statsList.Sort((a, b) => b.TotalGoldEarned.CompareTo(a.TotalGoldEarned));
            
            if (limit > 0 && statsList.Count > limit)
                return statsList.GetRange(0, limit);
            return statsList;
        }
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 保存击杀历史
            var history = new Godot.Collections.Array();
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
            var stats = new Dictionary<string, object>();
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
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            // 加载击杀历史
            if (data.ContainsKey("kill_history"))
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
            if (data.ContainsKey("player_stats"))
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
            
            GD.Print("[WorldBossRewardSystem] Save data imported");
        }
    }
}
