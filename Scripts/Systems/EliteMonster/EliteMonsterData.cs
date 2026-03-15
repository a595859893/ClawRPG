using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// Elite Monster System - transforms regular enemies into elite variants with enhanced stats and abilities
    /// Based on roguelike game design patterns and enemy scaling mechanics
    /// </summary>
    public partial class EliteMonsterData : BaseSystem
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
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData()
    {
        return new Dictionary
        {
            { "total_elite_spawns", TotalEliteSpawns },
            { "champions_spawned", ChampionsSpawned },
            { "bosses_spawned", BossesSpawned },
            { "rogues_spawned", RoguesSpawned },
            { "tanks_spawned", TanksSpawned },
            { "mages_spawned", MagesSpawned },
            { "assassins_spawned", AssassinsSpawned },
            { "healers_spawned", HealersSpawned },
            { "brutes_spawned", BrutesSpawned },
            { "swifts_spawned", SwiftsSpawned },
            { "ancients_spawned", AncientsSpawned },
            { "normal_elite_count", NormalEliteCount },
            { "rare_elite_count", RareEliteCount },
            { "epic_elite_count", EpicEliteCount },
            { "legendary_elite_count", LegendaryEliteCount },
            { "elite_monsters_defeated", EliteMonstersDefeated },
            { "total_elite_gold_bonus", TotalEliteGoldBonus },
            { "total_elite_exp_bonus", TotalEliteExpBonus }
        };
    }
    
    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data)
    {
        if (data == null) return;
        
        TotalEliteSpawns = data.GetValueOrDefault("total_elite_spawns", 0);
        ChampionsSpawned = data.GetValueOrDefault("champions_spawned", 0);
        BossesSpawned = data.GetValueOrDefault("bosses_spawned", 0);
        RoguesSpawned = data.GetValueOrDefault("rogues_spawned", 0);
        TanksSpawned = data.GetValueOrDefault("tanks_spawned", 0);
        MagesSpawned = data.GetValueOrDefault("mages_spawned", 0);
        AssassinsSpawned = data.GetValueOrDefault("assassins_spawned", 0);
        HealersSpawned = data.GetValueOrDefault("healers_spawned", 0);
        BrutesSpawned = data.GetValueOrDefault("brutes_spawned", 0);
        SwiftsSpawned = data.GetValueOrDefault("swifts_spawned", 0);
        AncientsSpawned = data.GetValueOrDefault("ancients_spawned", 0);
        NormalEliteCount = data.GetValueOrDefault("normal_elite_count", 0);
        RareEliteCount = data.GetValueOrDefault("rare_elite_count", 0);
        EpicEliteCount = data.GetValueOrDefault("epic_elite_count", 0);
        LegendaryEliteCount = data.GetValueOrDefault("legendary_elite_count", 0);
        EliteMonstersDefeated = data.GetValueOrDefault("elite_monsters_defeated", 0);
        TotalEliteGoldBonus = data.GetValueOrDefault("total_elite_gold_bonus", 0);
        TotalEliteExpBonus = data.GetValueOrDefault("total_elite_exp_bonus", 0);
    }
}
