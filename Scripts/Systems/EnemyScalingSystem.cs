using Godot;
using System;
using System.Collections.Generic;

public class EnemyScalingSystem
{
    // 单例
    private static EnemyScalingSystem _instance;
    public static EnemyScalingSystem Instance
    {
        get
        {
            if (_instance == null) _instance = new EnemyScalingSystem();
            return _instance;
        }
    }

    private EnemyScalingData _data;
    private EnemyScalingDatabase _database;

    public EnemyScalingSystem()
    {
        _data = new EnemyScalingData();
        _database = EnemyScalingDatabase.Instance;
    }

    // 初始化
    public void Initialize()
    {
        GD.Print("[EnemyScalingSystem] Initialized");
    }

    // 计算缩放值
    private float CalculateScaledValue(EnemyScalingData.ScalingParameters parameters, int level)
    {
        float value = parameters.BaseValue;

        switch (parameters.Type)
        {
            case EnemyScalingData.ScalingType.Linear:
                value = parameters.BaseValue + (parameters.GrowthRate * (level - 1));
                break;

            case EnemyScalingData.ScalingType.Exponential:
                value = parameters.BaseValue * (float)Mathf.Pow(1 + parameters.GrowthRate / 10f, level - 1);
                break;

            case EnemyScalingData.ScalingType.Logarithmic:
                value = parameters.BaseValue + parameters.GrowthRate * Mathf.Log(level);
                break;

            case EnemyScalingData.ScalingType.Plateau:
                float plateauProgress = 1 - Mathf.Exp(-0.1f * (level - 1));
                value = parameters.BaseValue + (parameters.MaxValue - parameters.BaseValue) * plateauProgress;
                break;
        }

        // 限制最大值
        if (parameters.MaxValue > 0)
            value = Mathf.Min(value, parameters.MaxValue);

        return value;
    }

    // 获取缩放后的敌人属性
    public Dictionary<string, float> GetScaledEnemyStats(string enemyType, int level, string difficulty = "Normal")
    {
        var config = _database.GetEnemyConfig(enemyType);
        var difficultyConfig = _database.GetDifficultyConfig(difficulty);

        var stats = new Dictionary<string, float>
        {
            ["Health"] = CalculateScaledValue(config.HealthScaling, level) * difficultyConfig.HealthMultiplier,
            ["MaxHealth"] = CalculateScaledValue(config.HealthScaling, level) * difficultyConfig.HealthMultiplier,
            ["Attack"] = CalculateScaledValue(config.AttackScaling, level) * difficultyConfig.AttackMultiplier,
            ["Defense"] = CalculateScaledValue(config.DefenseScaling, level) * difficultyConfig.DefenseMultiplier,
            ["Speed"] = CalculateScaledValue(config.SpeedScaling, level) * difficultyConfig.SpeedMultiplier,
            ["Experience"] = config.ExperienceMultiplier * difficultyConfig.ExpMultiplier * level,
            ["DropRate"] = config.DropRateMultiplier * difficultyConfig.DropMultiplier,
        };

        _data.TotalScalingCalculations++;

        return stats;
    }

    // 基于楼层获取缩放等级
    public int GetScalingLevelForFloor(int floor)
    {
        // 每5层提升1级缩放
        return 1 + (floor - 1) / 5;
    }

    // 基于玩家等级获取缩放等级
    public int GetScalingLevelForPlayer(int playerLevel)
    {
        return playerLevel;
    }

    // 基于击杀数获取缩放等级
    public int GetScalingLevelForKills(int kills)
    {
        // 每50击杀提升1级缩放
        return 1 + kills / 50;
    }

    // 基于时间获取缩放等级 (分钟)
    public int GetScalingLevelForTime(float minutes)
    {
        // 每10分钟提升1级缩放
        return 1 + (int)(minutes / 10);
    }

    // 组合多个因素获取最终缩放等级
    public int GetCombinedScalingLevel(int floor, int playerLevel, int kills, float minutes)
    {
        int floorLevel = GetScalingLevelForFloor(floor);
        int playerLevel2 = GetScalingLevelForPlayer(playerLevel);
        int killsLevel = GetScalingLevelForKills(kills);
        int timeLevel = GetScalingLevelForTime(minutes);

        // 取平均值，稍微偏向最高值
        return (floorLevel + playerLevel2 + killsLevel + timeLevel) / 4;
    }

    // 更新玩家进度
    public void UpdatePlayerProgress(int playerLevel, int floor, int enemiesDefeated, int combo, float playTimeMinutes)
    {
        _data.PlayerLevel = playerLevel;
        _data.FloorNumber = floor;
        _data.EnemiesDefeated = enemiesDefeated;
        _data.ComboCount = combo;
        _data.PlayTimeMinutes = playTimeMinutes;
    }

    // 获取统计信息
    public Dictionary<string, object> GetStatistics()
    {
        return new Dictionary<string, object>
        {
            ["TotalScalingCalculations"] = _data.TotalScalingCalculations,
            ["CurrentFloor"] = _data.FloorNumber,
            ["CurrentPlayerLevel"] = _data.PlayerLevel,
            ["EnemiesDefeated"] = _data.EnemiesDefeated,
            ["CurrentCombo"] = _data.ComboCount,
            ["PlayTimeMinutes"] = _data.PlayTimeMinutes,
            ["CurrentWave"] = _data.CurrentWave,
        };
    }

    // 重置进度
    public void ResetProgress()
    {
        _data.PlayerLevel = 1;
        _data.FloorNumber = 1;
        _data.EnemiesDefeated = 0;
        _data.ComboCount = 0;
        _data.PlayTimeMinutes = 0;
        _data.CurrentWave = 1;
    }

    // 保存数据
    public EnemyScalingData GetData()
    {
        return _data;
    }

    // 加载数据
    public void LoadData(EnemyScalingData data)
    {
        _data = data;
    }

    // 获取缩放类型名称
    public string GetScalingTypeName(EnemyScalingData.ScalingType type)
    {
        switch (type)
        {
            case EnemyScalingData.ScalingType.Linear: return "Linear";
            case EnemyScalingData.ScalingType.Exponential: return "Exponential";
            case EnemyScalingData.ScalingType.Logarithmic: return "Logarithmic";
            case EnemyScalingData.ScalingType.Plateau: return "Plateau";
            default: return "Unknown";
        }
    }
}
