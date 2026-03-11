using System;
using System.Collections.Generic;
using Godot;

/// <summary>
/// 坐骑战斗竞技场数据库
/// </summary>
public class MountBattleArenaDatabase
{
    public static List<MountBattleArenaData.MountArena> GetAllArenas()
    {
        return new List<MountBattleArenaData.MountArena>
        {
            // Training Ground
            new MountBattleArenaData.MountArena
            {
                Id = "training_ground_easy",
                Name = "训练场",
                Description = "初学者的训练场地，适合练习坐骑战斗技巧",
                Type = MountBattleArenaData.ArenaType.TrainingGround,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Easy,
                RecommendedLevel = 1,
                TotalWaves = 3,
                EnemiesPerWave = 2,
                EnemyHealthMultiplier = 0.5f,
                EnemyDamageMultiplier = 0.5f,
                EntryFee = 0,
                BaseGoldReward = 100,
                BaseExpReward = 50,
                RewardItems = new List<string> { "health_potion" }
            },
            new MountBattleArenaData.MountArena
            {
                Id = "training_ground_normal",
                Name = "进阶训练场",
                Description = "进阶训练场地，需要一定的坐骑战斗经验",
                Type = MountBattleArenaData.ArenaType.TrainingGround,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Normal,
                RecommendedLevel = 15,
                TotalWaves = 5,
                EnemiesPerWave = 3,
                EnemyHealthMultiplier = 0.8f,
                EnemyDamageMultiplier = 0.8f,
                EntryFee = 50,
                BaseGoldReward = 300,
                BaseExpReward = 150,
                RewardItems = new List<string> { "health_potion", "mana_potion" }
            },
            
            // Battle Colosseum
            new MountBattleArenaData.MountArena
            {
                Id = "colosseum_hard",
                Name = "战斗竞技场",
                Description = "真正的战斗竞技场，考验坐骑和骑手的配合",
                Type = MountBattleArenaData.ArenaType.BattleColosseum,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Hard,
                RecommendedLevel = 25,
                TotalWaves = 7,
                EnemiesPerWave = 4,
                EnemyHealthMultiplier = 1.2f,
                EnemyDamageMultiplier = 1.2f,
                EntryFee = 200,
                BaseGoldReward = 800,
                BaseExpReward = 400,
                RewardItems = new List<string> { "enhancement_stone", "rare_armor" }
            },
            new MountBattleArenaData.MountArena
            {
                Id = "colosseum_epic",
                Name = "史诗战斗竞技场",
                Description = "高难度竞技场，只有强大的坐骑才能通过",
                Type = MountBattleArenaData.ArenaType.BattleColosseum,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Epic,
                RecommendedLevel = 35,
                TotalWaves = 10,
                EnemiesPerWave = 5,
                EnemyHealthMultiplier = 1.5f,
                EnemyDamageMultiplier = 1.5f,
                EntryFee = 500,
                BaseGoldReward = 2000,
                BaseExpReward = 1000,
                RewardItems = new List<string> { "epic_weapon", "enhancement_stone" }
            },
            
            // Dragon Arena
            new MountBattleArenaData.MountArena
            {
                Id = "dragon_arena_legendary",
                Name = "龙之战场",
                Description = "传说中与龙战斗的战场，只有最强大的坐骑才能挑战",
                Type = MountBattleArenaData.ArenaType.DragonArena,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Legendary,
                RecommendedLevel = 45,
                TotalWaves = 12,
                EnemiesPerWave = 6,
                EnemyHealthMultiplier = 2.0f,
                EnemyDamageMultiplier = 2.0f,
                EntryFee = 1000,
                BaseGoldReward = 5000,
                BaseExpReward = 2500,
                RewardItems = new List<string> { "legendary_weapon", "dragon_scale" }
            },
            
            // Phoenix Nest
            new MountBattleArenaData.MountArena
            {
                Id = "phoenix_nest_normal",
                Name = "凤凰巢穴",
                Description = "凤凰的栖息地，火焰与重生的试炼",
                Type = MountBattleArenaData.ArenaType.PhoenixNest,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Normal,
                RecommendedLevel = 20,
                TotalWaves = 6,
                EnemiesPerWave = 3,
                EnemyHealthMultiplier = 1.0f,
                EnemyDamageMultiplier = 1.0f,
                EntryFee = 100,
                BaseGoldReward = 500,
                BaseExpReward = 250,
                RewardItems = new List<string> { "fire_essence", "phoenix_feather" }
            },
            new MountBattleArenaData.MountArena
            {
                Id = "phoenix_nest_epic",
                Name = "凤凰巢穴深处",
                Description = "深入凤凰巢穴，面对更强大的火焰生物",
                Type = MountBattleArenaData.ArenaType.PhoenixNest,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Epic,
                RecommendedLevel = 40,
                TotalWaves = 10,
                EnemiesPerWave = 5,
                EnemyHealthMultiplier = 1.8f,
                EnemyDamageMultiplier = 1.8f,
                EntryFee = 800,
                BaseGoldReward = 3000,
                BaseExpReward = 1500,
                RewardItems = new List<string> { "epic_armor", "phoenix_feather" }
            },
            
            // Shadow Realm
            new MountBattleArenaData.MountArena
            {
                Id = "shadow_realm_hard",
                Name = "暗影领域",
                Description = "充满暗影生物的领域，考验战斗技巧",
                Type = MountBattleArenaData.ArenaType.ShadowRealm,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Hard,
                RecommendedLevel = 30,
                TotalWaves = 8,
                EnemiesPerWave = 4,
                EnemyHealthMultiplier = 1.4f,
                EnemyDamageMultiplier = 1.4f,
                EntryFee = 400,
                BaseGoldReward = 1500,
                BaseExpReward = 750,
                RewardItems = new List<string> { "shadow_crystal", "dark_essence" }
            },
            new MountBattleArenaData.MountArena
            {
                Id = "shadow_realm_legendary",
                Name = "暗影之王座",
                Description = "暗影领主的领域，传说级的挑战",
                Type = MountBattleArenaData.ArenaType.ShadowRealm,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Legendary,
                RecommendedLevel = 50,
                TotalWaves = 15,
                EnemiesPerWave = 7,
                EnemyHealthMultiplier = 2.5f,
                EnemyDamageMultiplier = 2.5f,
                EntryFee = 2000,
                BaseGoldReward = 8000,
                BaseExpReward = 4000,
                RewardItems = new List<string> { "legendary_armor", "shadow_crown" }
            },
            
            // Sacred Ground
            new MountBattleArenaData.MountArena
            {
                Id = "sacred_ground_normal",
                Name = "神圣之地",
                Description = "神圣的战场，光明与邪恶的对抗",
                Type = MountBattleArenaData.ArenaType.SacredGround,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Normal,
                RecommendedLevel = 18,
                TotalWaves = 5,
                EnemiesPerWave = 3,
                EnemyHealthMultiplier = 0.9f,
                EnemyDamageMultiplier = 0.9f,
                EntryFee = 80,
                BaseGoldReward = 400,
                BaseExpReward = 200,
                RewardItems = new List<string> { "holy_water", "blessed herb" }
            },
            new MountBattleArenaData.MountArena
            {
                Id = "sacred_ground_epic",
                Name = "神圣战场",
                Description = "传说中诸神战斗过的战场",
                Type = MountBattleArenaData.ArenaType.SacredGround,
                Difficulty = MountBattleArenaData.ArenaDifficulty.Epic,
                RecommendedLevel = 38,
                TotalWaves = 10,
                EnemiesPerWave = 5,
                EnemyHealthMultiplier = 1.7f,
                EnemyDamageMultiplier = 1.7f,
                EntryFee = 600,
                BaseGoldReward = 2500,
                BaseExpReward = 1200,
                RewardItems = new List<string> { "holy_weapon", "sacred_gem" }
            }
        };
    }
    
    public static MountBattleArenaData.MountArena GetArena(string arenaId)
    {
        var arenas = GetAllArenas();
        foreach (var arena in arenas)
        {
            if (arena.Id == arenaId)
                return arena;
        }
        return null;
    }
    
    public static List<MountBattleArenaData.MountArena> GetArenasByType(MountBattleArenaData.ArenaType type)
    {
        var arenas = GetAllArenas();
        var result = new List<MountBattleArenaData.MountArena>();
        foreach (var arena in arenas)
        {
            if (arena.Type == type)
                result.Add(arena);
        }
        return result;
    }
    
    public static List<MountBattleArenaData.MountArena> GetArenasByDifficulty(MountBattleArenaData.ArenaDifficulty difficulty)
    {
        var arenas = GetAllArenas();
        var result = new List<MountBattleArenaData.MountArena>();
        foreach (var arena in arenas)
        {
            if (arena.Difficulty == difficulty)
                result.Add(arena);
        }
        return result;
    }
    
    public static string GetArenaTypeName(MountBattleArenaData.ArenaType type)
    {
        switch (type)
        {
            case MountBattleArenaData.ArenaType.TrainingGround: return "训练场";
            case MountBattleArenaData.ArenaType.BattleColosseum: return "战斗竞技场";
            case MountBattleArenaData.ArenaType.DragonArena: return "龙之战场";
            case MountBattleArenaData.ArenaType.PhoenixNest: return "凤凰巢穴";
            case MountBattleArenaData.ArenaType.ShadowRealm: return "暗影领域";
            case MountBattleArenaData.ArenaType.SacredGround: return "神圣之地";
            default: return "未知";
        }
    }
    
    public static string GetDifficultyName(MountBattleArenaData.ArenaDifficulty difficulty)
    {
        switch (difficulty)
        {
            case MountBattleArenaData.ArenaDifficulty.Easy: return "简单";
            case MountBattleArenaData.ArenaDifficulty.Normal: return "普通";
            case MountBattleArenaData.ArenaDifficulty.Hard: return "困难";
            case MountBattleArenaData.ArenaDifficulty.Epic: return "史诗";
            case MountBattleArenaData.ArenaDifficulty.Legendary: return "传奇";
            default: return "未知";
        }
    }
    
    public static string GetDifficultyColor(MountBattleArenaData.ArenaDifficulty difficulty)
    {
        switch (difficulty)
        {
            case MountBattleArenaData.ArenaDifficulty.Easy: return "#00FF00";
            case MountBattleArenaData.ArenaDifficulty.Normal: return "#00BFFF";
            case MountBattleArenaData.ArenaDifficulty.Hard: return "#FFA500";
            case MountBattleArenaData.ArenaDifficulty.Epic: return "#9400D3";
            case MountBattleArenaData.ArenaDifficulty.Legendary: return "#FF0000";
            default: return "#FFFFFF";
        }
    }
}
