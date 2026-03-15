using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Elite Monster System - Core logic for spawning and managing elite monsters
    /// Based on roguelike game design patterns and enemy scaling mechanics
    /// </summary>
    public partial class EliteMonsterSystem : BaseSystem
    {
        public static EliteMonsterSystem Instance { get; private set; }
        
        // Reference to data
        private EliteMonsterData _data;
        
        // Reference to database
        private EliteMonsterDatabase _database;
        
        // Current game state tracking
        private int _currentFloor = 1;
        private int _currentCombo = 0;
        private int _killsThisFloor = 0;
        private TimeSpan _elapsedTime = TimeSpan.Zero;
        
        // Override spawn chance (for testing)
        private float? _spawnChanceOverride;
        
        // Signal for elite spawn
        [Signal]
        public delegate void EliteSpawnedEventHandler(int instanceId, string eliteType, string tier);
        
        [Signal]
        public delegate void EliteDefeatedEventHandler(int instanceId, string eliteType, int goldBonus, int expBonus);
        
        public override void _Ready()
        {
            Instance = this;
            _data = new EliteMonsterData();
            _database = new EliteMonsterDatabase();
            _database._Ready();
        }
        
        public override void _Process(double delta)
        {
            _elapsedTime += TimeSpan.FromSeconds(delta);
        }
        
        /// <summary>
        /// Check if a monster should become elite based on current conditions
        /// </summary>
        public bool ShouldSpawnElite()
        {
            float chance = _spawnChanceOverride ?? _database.BaseSpawnChance;
            
            // Apply floor bonus
            if (_currentFloor >= 20) chance += 0.05f;
            else if (_currentFloor >= 10) chance += 0.03f;
            else if (_currentFloor >= 5) chance += 0.02f;
            
            // Apply combo bonus
            if (_currentCombo >= 10) chance += 0.05f;
            else if (_currentCombo >= 5) chance += 0.02f;
            
            // Apply time bonus
            if (_elapsedTime.TotalSeconds >= 600) chance += 0.03f;
            
            // Clamp chance
            chance = Mathf.Clamp(chance, 0.0f, 0.5f);
            
            return GD.RandDouble() < chance;
        }
        
        /// <summary>
        /// Transform a regular monster into an elite variant
        /// </summary>
        public EliteMonsterInfo TransformToElite(int instanceId, string monsterType)
        {
            var eliteType = _database.GetRandomEliteType();
            var tier = _database.GetRandomEliteTier();
            
            var typeConfig = _database.EliteTypeConfigs[eliteType];
            var tierConfig = _database.TierConfigs[tier];
            
            var eliteInfo = new EliteMonsterInfo
            {
                InstanceId = instanceId,
                Type = eliteType,
                Tier = tier,
                HealthMultiplier = typeConfig.BaseHealthBonus * tierConfig.StatMultiplier,
                AttackMultiplier = typeConfig.BaseAttackBonus * tierConfig.StatMultiplier,
                DefenseMultiplier = typeConfig.BaseDefenseBonus * tierConfig.StatMultiplier,
                SpeedMultiplier = typeConfig.BaseSpeedBonus * (tier == EliteMonsterData.EliteTier.Legendary ? 1.5f : 1.0f),
                DropRateBonus = typeConfig.DropRateBonus * tierConfig.StatMultiplier,
                Abilities = new List<string>(typeConfig.Abilities),
                SpawnTime = DateTime.Now
            };
            
            // Store in active elite monsters
            _data.ActiveEliteMonsters[instanceId] = eliteInfo;
            
            // Update statistics
            _data.TotalEliteSpawns++;
            UpdateEliteTypeCount(eliteType);
            UpdateEliteTierCount(tier);
            
            // Record spawn
            var record = new EliteSpawnRecord
            {
                MonsterType = monsterType,
                EliteType = eliteType,
                Tier = tier,
                Floor = _currentFloor,
                SpawnTime = DateTime.Now,
                WasDefeated = false
            };
            _data.SpawnHistory.Add(record);
            
            // Emit signal
            EmitSignal(SignalName.EliteSpawned, instanceId, eliteType.ToString(), tier.ToString());
            
            GD.Print($"[EliteMonster] Spawned {tierConfig.RarityName} {eliteType} (Instance: {instanceId})");
            
            return eliteInfo;
        }
        
        /// <summary>
        /// Record elite monster defeat and calculate bonuses
        /// </summary>
        public (int goldBonus, int expBonus) RecordEliteDefeat(int instanceId, int baseGold, int baseExp)
        {
            if (!_data.ActiveEliteMonsters.TryGetValue(instanceId, out var eliteInfo))
            {
                return (0, 0);
            }
            
            // Calculate bonuses
            int goldBonus = (int)(baseGold * eliteInfo.DropRateBonus);
            int expBonus = (int)(baseExp * eliteInfo.DropRateBonus);
            
            // Update statistics
            _data.EliteMonstersDefeated++;
            _data.TotalEliteGoldBonus += goldBonus;
            _data.TotalEliteExpBonus += expBonus;
            
            // Mark spawn record as defeated
            foreach (var record in _data.SpawnHistory)
            {
                if (record.SpawnTime == eliteInfo.SpawnTime)
                {
                    record.WasDefeated = true;
                    break;
                }
            }
            
            // Remove from active
            _data.ActiveEliteMonsters.Remove(instanceId);
            
            // Emit signal
            EmitSignal(SignalName.EliteDefeated, instanceId, eliteInfo.Type.ToString(), goldBonus, expBonus);
            
            GD.Print($"[EliteMonster] Defeated {eliteInfo.Type} - Gold Bonus: {goldBonus}, Exp Bonus: {expBonus}");
            
            return (goldBonus, expBonus);
        }
        
        /// <summary>
        /// Update current game state
        /// </summary>
        public void UpdateGameState(int floor, int combo, int kills)
        {
            _currentFloor = floor;
            _currentCombo = combo;
            _killsThisFloor = kills;
        }
        
        /// <summary>
        /// Reset for new run
        /// </summary>
        public void ResetRun()
        {
            _data.ActiveEliteMonsters.Clear();
            _data.SpawnHistory.Clear();
            _currentFloor = 1;
            _currentCombo = 0;
            _killsThisFloor = 0;
            _elapsedTime = TimeSpan.Zero;
            _spawnChanceOverride = null;
        }
        
        /// <summary>
        /// Get elite monster info
        /// </summary>
        public EliteMonsterInfo GetEliteInfo(int instanceId)
        {
            if (_data.ActiveEliteMonsters.TryGetValue(instanceId, out var info))
            {
                return info;
            }
            return null;
        }
        
        /// <summary>
        /// Check if monster is elite
        /// </summary>
        public bool IsElite(int instanceId)
        {
            return _data.ActiveEliteMonsters.ContainsKey(instanceId);
        }
        
        /// <summary>
        /// Get statistics
        /// </summary>
        public Dictionary<string, int> GetStatistics()
        {
            return new Dictionary<string, int>
            {
                { "TotalEliteSpawns", _data.TotalEliteSpawns },
                { "EliteMonstersDefeated", _data.EliteMonstersDefeated },
                { "Champions", _data.ChampionsSpawned },
                { "Bosses", _data.BossesSpawned },
                { "Rogues", _data.RoguesSpawned },
                { "Tanks", _data.TanksSpawned },
                { "Mages", _data.MagesSpawned },
                { "Assassins", _data.AssassinsSpawned },
                { "Healers", _data.HealersSpawned },
                { "Brutes", _data.BrutesSpawned },
                { "Swifts", _data.SwiftsSpawned },
                { "Ancients", _data.AncientsSpawned },
                { "NormalTier", _data.NormalEliteCount },
                { "RareTier", _data.RareEliteCount },
                { "EpicTier", _data.EpicEliteCount },
                { "LegendaryTier", _data.LegendaryEliteCount },
                { "TotalGoldBonus", _data.TotalEliteGoldBonus },
                { "TotalExpBonus", _data.TotalEliteExpBonus }
            };
        }
        
        private void UpdateEliteTypeCount(EliteMonsterData.EliteType type)
        {
            switch (type)
            {
                case EliteMonsterData.EliteType.Champion: _data.ChampionsSpawned++; break;
                case EliteMonsterData.EliteType.Boss: _data.BossesSpawned++; break;
                case EliteMonsterData.EliteType.Rogue: _data.RoguesSpawned++; break;
                case EliteMonsterData.EliteType.Tank: _data.TanksSpawned++; break;
                case EliteMonsterData.EliteType.Mage: _data.MagesSpawned++; break;
                case EliteMonsterData.EliteType.Assassin: _data.AssassinsSpawned++; break;
                case EliteMonsterData.EliteType.Healer: _data.HealersSpawned++; break;
                case EliteMonsterData.EliteType.Brute: _data.BrutesSpawned++; break;
                case EliteMonsterData.EliteType.Swift: _data.SwiftsSpawned++; break;
                case EliteMonsterData.EliteType.Ancient: _data.AncientsSpawned++; break;
            }
        }
        
        private void UpdateEliteTierCount(EliteMonsterData.EliteTier tier)
        {
            switch (tier)
            {
                case EliteMonsterData.EliteTier.Normal: _data.NormalEliteCount++; break;
                case EliteMonsterData.EliteTier.Rare: _data.RareEliteCount++; break;
                case EliteMonsterData.EliteTier.Epic: _data.EpicEliteCount++; break;
                case EliteMonsterData.EliteTier.Legendary: _data.LegendaryEliteCount++; break;
            }
        }
        
        /// <summary>
        /// Set spawn chance override (for testing)
        /// </summary>
        public void SetSpawnChanceOverride(float? chance)
        {
            _spawnChanceOverride = chance;
        }

    // ===== 持久化方法 =====

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        
        data["current_floor"] = _currentFloor;
        data["current_combo"] = _currentCombo;
        data["kills_this_floor"] = _killsThisFloor;
        data["elapsed_time"] = _elapsedTime.TotalSeconds;
        
        if (_data != null)
        {
            data["total_spawned"] = _data.TotalSpawned;
            data["total_killed"] = _data.TotalKilled;
            data["normal_elite_count"] = _data.NormalEliteCount;
            data["rare_elite_count"] = _data.RareEliteCount;
            data["epic_elite_count"] = _data.EpicEliteCount;
            data["legendary_elite_count"] = _data.LegendaryEliteCount;
            data["berserkers_spawned"] = _data.BerserkersSpawned;
            data["healers_spawned"] = _data.HealersSpawned;
            data["brutes_spawned"] = _data.BrutesSpawned;
            data["swifts_spawned"] = _data.SwiftsSpawned;
            data["ancients_spawned"] = _data.AncientsSpawned;
        }
        
        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        _currentFloor = (int)(data.GetValueOrDefault("current_floor", 1));
        _currentCombo = (int)(data.GetValueOrDefault("current_combo", 0));
        _killsThisFloor = (int)(data.GetValueOrDefault("kills_this_floor", 0));
        _elapsedTime = TimeSpan.FromSeconds((float)(data.GetValueOrDefault("elapsed_time", 0f)));
        
        if (_data == null) _data = new EliteMonsterData();
        
        _data.TotalSpawned = (int)(data.GetValueOrDefault("total_spawned", 0));
        _data.TotalKilled = (int)(data.GetValueOrDefault("total_killed", 0));
        _data.NormalEliteCount = (int)(data.GetValueOrDefault("normal_elite_count", 0));
        _data.RareEliteCount = (int)(data.GetValueOrDefault("rare_elite_count", 0));
        _data.EpicEliteCount = (int)(data.GetValueOrDefault("epic_elite_count", 0));
        _data.LegendaryEliteCount = (int)(data.GetValueOrDefault("legendary_elite_count", 0));
        _data.BerserkersSpawned = (int)(data.GetValueOrDefault("berserkers_spawned", 0));
        _data.HealersSpawned = (int)(data.GetValueOrDefault("healers_spawned", 0));
        _data.BrutesSpawned = (int)(data.GetValueOrDefault("brutes_spawned", 0));
        _data.SwiftsSpawned = (int)(data.GetValueOrDefault("swifts_spawned", 0));
        _data.AncientsSpawned = (int)(data.GetValueOrDefault("ancients_spawned", 0));
    }
}
