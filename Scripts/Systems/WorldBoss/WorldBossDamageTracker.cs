using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Systems.WorldBoss;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// WorldBossDamageTracker - 负责世界Boss伤害统计和排名
    /// </summary>
    public partial class WorldBossDamageTracker : BaseSystem
    {
        public static WorldBossDamageTracker Instance { get; private set; }
        
        // Player damage records for each boss
        private Dictionary<string, List<WorldBossData.PlayerDamageRecord>> _bossDamageRecords = new Dictionary<string, List<WorldBossData.PlayerDamageRecord>>();
        
        // Player statistics
        private Dictionary<string, WorldBossData.PlayerWorldBossStats> _playerStats = new Dictionary<string, WorldBossData.PlayerWorldBossStats>();
        
        [Signal] public delegate void BossDamagedEventHandler(string bossInstanceId, string playerId, int damage);
        
        public override void _Ready()
        {
            Instance = this;
        }
        
        public void RecordDamage(string bossInstanceId, string playerId, int damage)
        {
            if (!_bossDamageRecords.ContainsKey(bossInstanceId))
            {
                _bossDamageRecords[bossInstanceId] = new List<WorldBossData.PlayerDamageRecord>();
            }
            
            var records = _bossDamageRecords[bossInstanceId];
            var existingRecord = records.Find(r => r.PlayerId == playerId);
            
            if (existingRecord != null)
            {
                existingRecord.TotalDamage += damage;
                existingRecord.LastDamageTime = DateTime.Now;
                existingRecord.HitCount++;
            }
            else
            {
                records.Add(new WorldBossData.PlayerDamageRecord
                {
                    PlayerId = playerId,
                    TotalDamage = damage,
                    FirstHitTime = DateTime.Now,
                    LastDamageTime = DateTime.Now,
                    HitCount = 1
                });
            }
            
            // Update player stats
            if (!_playerStats.ContainsKey(playerId))
            {
                _playerStats[playerId] = new WorldBossData.PlayerWorldBossStats
                {
                    PlayerId = playerId
                };
            }
            
            _playerStats[playerId].TotalDamageDealt += damage;
            _playerStats[playerId].BossesDamaged++;
            
            EmitSignal(SignalName.BossDamaged, bossInstanceId, playerId, damage);
        }
        
        public List<WorldBossData.PlayerDamageRecord> GetDamageRecords(string bossInstanceId)
        {
            if (!_bossDamageRecords.ContainsKey(bossInstanceId))
            {
                return new List<WorldBossData.PlayerDamageRecord>();
            }
            
            var records = _bossDamageRecords[bossInstanceId];
            records.Sort((a, b) => b.TotalDamage.CompareTo(a.TotalDamage));
            return records;
        }
        
        public List<WorldBossData.PlayerDamageRecord> GetTopDamageDealers(string bossInstanceId, int count = 10)
        {
            var records = GetDamageRecords(bossInstanceId);
            if (records.Count <= count) return records;
            return records.GetRange(0, count);
        }
        
        public int GetPlayerRank(string bossInstanceId, string playerId)
        {
            var records = GetDamageRecords(bossInstanceId);
            for (int i = 0; i < records.Count; i++)
            {
                if (records[i].PlayerId == playerId) return i + 1;
            }
            return -1;
        }
        
        public WorldBossData.PlayerWorldBossStats GetPlayerStats(string playerId)
        {
            if (!_playerStats.ContainsKey(playerId))
            {
                _playerStats[playerId] = new WorldBossData.PlayerWorldBossStats
                {
                    PlayerId = playerId
                };
            }
            return _playerStats[playerId];
        }
        
        public Dictionary<string, WorldBossData.PlayerWorldBossStats> GetAllPlayerStats()
        {
            return new Dictionary<string, WorldBossData.PlayerWorldBossStats>(_playerStats);
        }
        
        public void ClearBossRecords(string bossInstanceId)
        {
            if (_bossDamageRecords.ContainsKey(bossInstanceId))
            {
                _bossDamageRecords.Remove(bossInstanceId);
            }
        }
        
        public void ClearAllRecords()
        {
            _bossDamageRecords.Clear();
            _playerStats.Clear();
        }
        
        public Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // Serialize boss damage records
            var bossRecordsData = new Dictionary();
            foreach (var kvp in _bossDamageRecords)
            {
                var recordsList = new List<Dictionary>();
                foreach (var record in kvp.Value)
                {
                    recordsList.Add(new Dictionary
                    {
                        ["player_id"] = record.PlayerId,
                        ["total_damage"] = record.TotalDamage,
                        ["first_hit_time"] = record.FirstHitTime.Ticks,
                        ["last_hit_time"] = record.LastDamageTime.Ticks,
                        ["hit_count"] = record.HitCount
                    });
                }
                bossRecordsData[kvp.Key] = recordsList;
            }
            data["boss_damage_records"] = bossRecordsData;
            
            // Serialize player stats
            var playerStatsData = new Dictionary();
            foreach (var kvp in _playerStats)
            {
                playerStatsData[kvp.Key] = new Dictionary
                {
                    ["total_damage_dealt"] = kvp.Value.TotalDamageDealt,
                    ["bosses_damaged"] = kvp.Value.BossesDamaged,
                    ["bosses_killed"] = kvp.Value.BossesKilled,
                    ["highest_damage"] = kvp.Value.HighestDamage,
                    ["total_damage_taken"] = kvp.Value.TotalDamageTaken
                };
            }
            data["player_stats"] = playerStatsData;
            
            return data;
        }
        
        public void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            // Import boss damage records
            if (data.Contains("boss_damage_records"))
            {
                var bossRecordsData = (Dictionary)data["boss_damage_records"];
                foreach (string bossInstanceId in bossRecordsData.Keys)
                {
                    var recordsList = (Godot.Collections.Array)bossRecordsData[bossInstanceId];
                    var records = new List<WorldBossData.PlayerDamageRecord>();
                    
                    foreach (Dictionary recordData in recordsList)
                    {
                        records.Add(new WorldBossData.PlayerDamageRecord
                        {
                            PlayerId = Convert.ToString(recordData["player_id"]),
                            TotalDamage = Convert.ToInt32(recordData["total_damage"]),
                            FirstHitTime = new DateTime(Convert.ToInt64(recordData["first_hit_time"])),
                            LastDamageTime = new DateTime(Convert.ToInt64(recordData["last_hit_time"])),
                            HitCount = Convert.ToInt32(recordData["hit_count"])
                        });
                    }
                    
                    _bossDamageRecords[bossInstanceId] = records;
                }
            }
            
            // Import player stats
            if (data.Contains("player_stats"))
            {
                var playerStatsData = (Dictionary)data["player_stats"];
                foreach (string playerId in playerStatsData.Keys)
                {
                    var statsData = (Dictionary)playerStatsData[playerId];
                    _playerStats[playerId] = new WorldBossData.PlayerWorldBossStats
                    {
                        PlayerId = playerId,
                        TotalDamageDealt = Convert.ToInt32(statsData["total_damage_dealt"]),
                        BossesDamaged = Convert.ToInt32(statsData["bosses_damaged"]),
                        BossesKilled = Convert.ToInt32(statsData["bosses_killed"]),
                        HighestDamage = Convert.ToInt32(statsData["highest_damage"]),
                        TotalDamageTaken = Convert.ToInt32(statsData["total_damage_taken"])
                    };
                }
            }
        }
    }
}
