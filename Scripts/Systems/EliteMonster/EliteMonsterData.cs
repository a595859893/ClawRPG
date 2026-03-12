using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Elite Monster System - transforms regular enemies into elite variants with enhanced stats and abilities
    /// Based on roguelike game design patterns and enemy scaling mechanics
    /// </summary>
    public partial class EliteMonsterData : Node
    {
        // Elite monster types
        public enum EliteType
        {
            Champion,      // Enhanced stats
            Boss,          // Mini-boss with abilities
            Rogue,         // High mobility and crit
            Tank,          // High defense and HP
            Mage,          // Elemental attacks
            Assassin,      // High burst damage
            Healer,        // Regenerates allies
            Brute,         // AOE attacks
            Swift,         // Fast attack speed
            Ancient        // Rare, very powerful
        }
        
        // Elite tier levels
        public enum EliteTier
        {
            Normal = 1,    // 1.5x stats
            Rare = 2,      // 2x stats
            Epic = 3,      // 3x stats
            Legendary = 4   // 5x stats
        }
        
        // Track active elite monsters
        public Dictionary<int, EliteMonsterInfo> ActiveEliteMonsters = new();
        
        // Elite spawn history
        public List<EliteSpawnRecord> SpawnHistory = new();
        
        // Statistics
        public int TotalEliteSpawns { get; set; } = 0;
        public int ChampionsSpawned { get; set; } = 0;
        public int BossesSpawned { get; set; } = 0;
        public int RoguesSpawned { get; set; } = 0;
        public int TanksSpawned { get; set; } = 0;
        public int MagesSpawned { get; set; } = 0;
        public int AssassinsSpawned { get; set; } = 0;
        public int HealersSpawned { get; set; } = 0;
        public int BrutesSpawned { get; set; } = 0;
        public int SwiftsSpawned { get; set; } = 0;
        public int AncientsSpawned { get; set; } = 0;
        
        // Elite tier counts
        public int NormalEliteCount { get; set; } = 0;
        public int RareEliteCount { get; set; } = 0;
        public int EpicEliteCount { get; set; } = 0;
        public int LegendaryEliteCount { get; set; } = 0;
        
        // Track defeated elite monsters
        public int EliteMonstersDefeated { get; set; } = 0;
        
        // Bonus rewards from elite kills
        public int TotalEliteGoldBonus { get; set; } = 0;
        public int TotalEliteExpBonus { get; set; } = 0;
    }
    
    public class EliteMonsterInfo
    {
        public int InstanceId;
        public EliteMonsterData.EliteType Type;
        public EliteMonsterData.EliteTier Tier;
        public float HealthMultiplier;
        public float AttackMultiplier;
        public float DefenseMultiplier;
        public float SpeedMultiplier;
        public float DropRateBonus;
        public List<string> Abilities = new();
        public DateTime SpawnTime;
    }
    
    public class EliteSpawnRecord
    {
        public string MonsterType;
        public EliteMonsterData.EliteType EliteType;
        public EliteMonsterData.EliteTier Tier;
        public int Floor;
        public DateTime SpawnTime;
        public bool WasDefeated;
    }
}
