using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 动态难度数据库 - 配置难度等级数据
/// </summary>
public class DynamicDifficultyDatabase
{
    // 单例
    private static DynamicDifficultyDatabase _instance;
    public static DynamicDifficultyDatabase Instance
    {
        get
        {
            if (_instance == null)
                _instance = new DynamicDifficultyDatabase();
            return _instance;
        }
    }

    // 难度等级名称
    public static string[] DifficultyNames = new string[]
    {
        "简单",
        "普通",
        "困难",
        "史诗",
        "传奇"
    };

    // 难度等级颜色 (RGBA)
    public static Color[] DifficultyColors = new Color[]
    {
        new Color(0.2f, 0.8f, 0.2f),   // Easy - 绿色
        new Color(0.2f, 0.6f, 1.0f),   // Normal - 蓝色
        new Color(1.0f, 0.6f, 0.0f),   // Hard - 橙色
        new Color(0.6f, 0.2f, 1.0f),    // Epic - 紫色
        new Color(1.0f, 0.2f, 0.2f)     // Legendary - 红色
    };

    // 难度等级描述
    public static string[] DifficultyDescriptions = new string[]
    {
        "适合新手的难度，敌人较弱，掉落丰富",
        "标准难度，体验完整的游戏内容",
        "具有挑战性，需要一定的游戏技巧",
        "高难度，适合经验丰富的玩家",
        "极限挑战，只有最熟练的玩家才能通关"
    };

    // 技能评估权重
    public struct SkillWeights
    {
        public float WinRate;
        public float ResourceEfficiency;
        public float SurvivalAbility;
        public float DamageOutput;
        public float TechnicalSkill;

        public static SkillWeights Default = new SkillWeights
        {
            WinRate = 0.25f,
            ResourceEfficiency = 0.15f,
            SurvivalAbility = 0.20f,
            DamageOutput = 0.20f,
            TechnicalSkill = 0.20f
        };
    }

    // 玩家分组阈值
    public struct PlayerGroupThresholds
    {
        public float Beginner;    // 新手阈值
        public float Normal;     // 普通阈值
        public float Skilled;   // 熟练阈值
        public float Expert;    // 专家阈值

        public static PlayerGroupThresholds Default = new PlayerGroupThresholds
        {
            Beginner = 0.3f,
            Normal = 0.5f,
            Skilled = 0.7f,
            Expert = 0.85f
        };
    }

    // 难度调整参数
    public struct AdjustmentParams
    {
        public int SessionsRequiredForAdjustment;  // 调整所需会话数
        public float ScoreChangeThreshold;         // 评分变化阈值
        public float MaxDifficultyJump;           // 最大难度跳跃
        public float MinWinRateForUpgrade;       // 升级所需最低胜率
        public float MaxWinRateForDowngrade;     // 降级所需最高胜率

        public static AdjustmentParams Default = new AdjustmentParams
        {
            SessionsRequiredForAdjustment = 3,
            ScoreChangeThreshold = 0.1f,
            MaxDifficultyJump = 1,
            MinWinRateForUpgrade = 0.7f,
            MaxWinRateForDowngrade = 0.4f
        };
    }

    // 获取难度名称
    public static string GetDifficultyName(DynamicDifficultyData.DifficultyLevel level)
    {
        int index = (int)level;
        if (index >= 0 && index < DifficultyNames.Length)
            return DifficultyNames[index];
        return "未知";
    }

    // 获取难度颜色
    public static Color GetDifficultyColor(DynamicDifficultyData.DifficultyLevel level)
    {
        int index = (int)level;
        if (index >= 0 && index < DifficultyColors.Length)
            return DifficultyColors[index];
        return Colors.White;
    }

    // 获取难度描述
    public static string GetDifficultyDescription(DynamicDifficultyData.DifficultyLevel level)
    {
        int index = (int)level;
        if (index >= 0 && index < DifficultyDescriptions.Length)
            return DifficultyDescriptions[index];
        return "";
    }

    // 获取玩家分组
    public static string GetPlayerGroup(float skillScore)
    {
        PlayerGroupThresholds thresholds = PlayerGroupThresholds.Default;
        
        if (skillScore < thresholds.Beginner)
            return "新手";
        else if (skillScore < thresholds.Normal)
            return "普通";
        else if (skillScore < thresholds.Skilled)
            return "熟练";
        else if (skillScore < thresholds.Expert)
            return "专家";
        else
            return "大师";
    }

    // 获取建议难度
    public static DynamicDifficultyData.DifficultyLevel GetRecommendedDifficulty(float skillScore)
    {
        PlayerGroupThresholds thresholds = PlayerGroupThresholds.Default;
        
        if (skillScore < thresholds.Beginner)
            return DynamicDifficultyData.DifficultyLevel.Easy;
        else if (skillScore < thresholds.Normal)
            return DynamicDifficultyData.DifficultyLevel.Normal;
        else if (skillScore < thresholds.Skilled)
            return DynamicDifficultyData.DifficultyLevel.Hard;
        else if (skillScore < thresholds.Expert)
            return DynamicDifficultyData.DifficultyLevel.Epic;
        else
            return DynamicDifficultyData.DifficultyLevel.Legendary;
    }
}
