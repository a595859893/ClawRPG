using Godot;
using System;
using System.Collections.Generic;

public class ArenaTournamentDatabase
{
    // 锦标赛类型配置
    public static Dictionary<ArenaTournamentType, Dictionary<string, object>> TournamentTypeConfigs = new Dictionary<ArenaTournamentType, Dictionary<string, object>>
    {
        { ArenaTournamentType.SingleElimination, new Dictionary<string, object>
            {
                { "name", "Single Elimination" },
                { "description", "单败淘汰赛，输一场即被淘汰" },
                { "min_participants", 4 },
                { "max_participants", 32 },
                { "rounds_estimate", 5 }
            }
        },
        { ArenaTournamentType.DoubleElimination, new Dictionary<string, object>
            {
                { "name", "Double Elimination" },
                { "description", "双败淘汰赛，输两场被淘汰" },
                { "min_participants", 4 },
                { "max_participants", 16 },
                { "rounds_estimate", 8 }
            }
        },
        { ArenaTournamentType.RoundRobin, new Dictionary<string, object>
            {
                { "name", "Round Robin" },
                { "description", "循环赛，每人与所有对手交手一次" },
                { "min_participants", 4 },
                { "max_participants", 8 },
                { "rounds_estimate", 7 }
            }
        },
        { ArenaTournamentType.Swiss, new Dictionary<string, object>
            {
                { "name", "Swiss System" },
                { "description", "瑞士制，每轮根据战绩匹配对手" },
                { "min_participants", 4 },
                { "max_participants", 32 },
                { "rounds_estimate", 5 }
            }
        }
    };
    
    // 奖励配置
    public static Dictionary<int, Dictionary<string, int>> PlacementRewards = new Dictionary<int, Dictionary<string, int>>
    {
        { 1, new Dictionary<string, int> { { "gold", 10000 }, { "exp", 5000 } } },
        { 2, new Dictionary<string, int> { { "gold", 5000 }, { "exp", 2500 } } },
        { 3, new Dictionary<string, int> { { "gold", 2500 }, { "exp", 1250 } } },
        { 4, new Dictionary<string, int> { { "gold", 1000 }, { "exp", 500 } } }
    };
    
    // 锦标赛难度配置
    public static Dictionary<string, Dictionary<string, object>> DifficultyConfigs = new Dictionary<string, Dictionary<string, object>>
    {
        { "Easy", new Dictionary<string, object>
            {
                { "name", "Easy" },
                { "description", "简单难度，适合新手" },
                { "reward_multiplier", 0.5f },
                { "enemy_difficulty", 0.8f }
            }
        },
        { "Normal", new Dictionary<string, object>
            {
                { "name", "Normal" },
                { "description", "普通难度" },
                { "reward_multiplier", 1.0f },
                { "enemy_difficulty", 1.0f }
            }
        },
        { "Hard", new Dictionary<string, object>
            {
                { "name", "Hard" },
                { "description", "困难难度" },
                { "reward_multiplier", 1.5f },
                { "enemy_difficulty", 1.5f }
            }
        },
        { "Nightmare", new Dictionary<string, object>
            {
                { "name", "Nightmare" },
                { "description", "噩梦难度" },
                { "reward_multiplier", 2.0f },
                { "enemy_difficulty", 2.0f }
            }
        },
        { "Legendary", new Dictionary<string, object>
            {
                { "name", "Legendary" },
                { "description", "传奇难度" },
                { "reward_multiplier", 3.0f },
                { "enemy_difficulty", 3.0f }
            }
        }
    };
    
    // 获取锦标赛类型名称
    public static string GetTournamentTypeName(ArenaTournamentType type)
    {
        if (TournamentTypeConfigs.TryGetValue(type, out var config))
        {
            return config["name"].ToString();
        }
        return "Unknown";
    }
    
    // 获取锦标赛类型描述
    public static string GetTournamentTypeDescription(ArenaTournamentType type)
    {
        if (TournamentTypeConfigs.TryGetValue(type, out var config))
        {
            return config["description"].ToString();
        }
        return "";
    }
    
    // 获取奖励
    public static (int gold, int exp) GetReward(int placement, string difficulty = "Normal")
    {
        int gold = 0;
        int exp = 0;
        
        if (PlacementRewards.TryGetValue(placement, out var rewards))
        {
            gold = rewards["gold"];
            exp = rewards["exp"];
        }
        
        // 应用难度乘数
        if (DifficultyConfigs.TryGetValue(difficulty, out var diffConfig))
        {
            float multiplier = (float)diffConfig["reward_multiplier"];
            gold = (int)(gold * multiplier);
            exp = (int)(exp * multiplier);
        }
        
        return (gold, exp);
    }
}
