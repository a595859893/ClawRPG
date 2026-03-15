using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 敌人属性缩放系统，负责根据各种因素（楼层、玩家等级、击杀数、时间等）
/// 动态计算敌人的属性缩放值，支持线性、指数、对数和 plateau 等多种增长模式。
/// </summary>
public class EnemyScalingSystem
{
    // 单例
    private static EnemyScalingSystem _instance;
    /// <summary>
    /// 获取单例实例
    /// </summary>
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

    /// <summary>
    /// 初始化缩放数据与数据库
    /// </summary>
    public EnemyScalingSystem()
    {
        _data = new EnemyScalingData();
        _database = EnemyScalingDatabase.Instance;
    }

    /// <summary>
    /// 初始化系统，输出日志信息
    /// </summary>
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

    /// <summary>
    /// 获取缩放后的敌人属性
    /// </summary>
    /// <param name="enemyType">敌人类型</param>
    /// <param name="level">缩放等级</param>
    /// <param name="difficulty">难度等级，默认为 Normal</param>
    /// <returns>包含各项属性（生命、攻击、防御、速度、经验掉落等）的字典</returns>
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

    /// <summary>
    /// 基于楼层获取缩放等级
    /// </summary>
    /// <param name="floor">当前楼层号</param>
    /// <returns>对应的缩放等级，每5层提升1级</returns>
    public int GetScalingLevelForFloor(int floor)
    {
        // 每5层提升1级缩放
        return 1 + (floor - 1) / 5;
    }

    /// <summary>
    /// 基于玩家等级获取缩放等级
    /// </summary>
    /// <param name="playerLevel">玩家等级</param>
    /// <returns>对应的缩放等级（与玩家等级相同）</returns>
    public int GetScalingLevelForPlayer(int playerLevel)
    {
        return playerLevel;
    }

    /// <summary>
    /// 基于击杀数获取缩放等级
    /// </summary>
    /// <param name="kills">已击杀敌人数量</param>
    /// <returns>对应的缩放等级，每50击杀提升1级</returns>
    public int GetScalingLevelForKills(int kills)
    {
        // 每50击杀提升1级缩放
        return 1 + kills / 50;
    }

    /// <summary>
    /// 基于游戏时间获取缩放等级
    /// </summary>
    /// <param name="minutes">已游戏时间（分钟）</param>
    /// <returns>对应的缩放等级，每10分钟提升1级</returns>
    public int GetScalingLevelForTime(float minutes)
    {
        // 每10分钟提升1级缩放
        return 1 + (int)(minutes / 10);
    }

    /// <summary>
    /// 组合多个因素计算最终缩放等级
    /// </summary>
    /// <param name="floor">当前楼层</param>
    /// <param name="playerLevel">玩家等级</param>
    /// <param name="kills">已击杀数量</param>
    /// <param name="minutes">已游戏时间（分钟）</param>
    /// <returns>综合计算后的最终缩放等级（各因素平均值）</returns>
    public int GetCombinedScalingLevel(int floor, int playerLevel, int kills, float minutes)
    {
        int floorLevel = GetScalingLevelForFloor(floor);
        int playerLevel2 = GetScalingLevelForPlayer(playerLevel);
        int killsLevel = GetScalingLevelForKills(kills);
        int timeLevel = GetScalingLevelForTime(minutes);

        // 取平均值，稍微偏向最高值
        return (floorLevel + playerLevel2 + killsLevel + timeLevel) / 4;
    }

    /// <summary>
    /// 更新玩家进度数据
    /// </summary>
    /// <param name="playerLevel">玩家等级</param>
    /// <param name="floor">当前楼层</param>
    /// <param name="enemiesDefeated">已击败敌人数量</param>
    /// <param name="combo">当前连击数</param>
    /// <param name="playTimeMinutes">已游戏时间（分钟）</param>
    public void UpdatePlayerProgress(int playerLevel, int floor, int enemiesDefeated, int combo, float playTimeMinutes)
    {
        _data.PlayerLevel = playerLevel;
        _data.FloorNumber = floor;
        _data.EnemiesDefeated = enemiesDefeated;
        _data.ComboCount = combo;
        _data.PlayTimeMinutes = playTimeMinutes;
    }

    /// <summary>
    /// 获取当前统计信息
    /// </summary>
    /// <returns>包含总缩放计算次数、当前楼层、玩家等级、已击败敌人、连击数、游戏时间和当前波次的字典</returns>
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

    /// <summary>
    /// 重置所有进度数据到初始状态
    /// </summary>
    public void ResetProgress()
    {
        _data.PlayerLevel = 1;
        _data.FloorNumber = 1;
        _data.EnemiesDefeated = 0;
        _data.ComboCount = 0;
        _data.PlayTimeMinutes = 0;
        _data.CurrentWave = 1;
    }

    /// <summary>
    /// 获取当前保存的缩放数据
    /// </summary>
    /// <returns>EnemyScalingData 实例</returns>
    public EnemyScalingData GetData()
    {
        return _data;
    }

    /// <summary>
    /// 加载缩放数据
    /// </summary>
    /// <param name="data">要加载的 EnemyScalingData 实例</param>
    public void LoadData(EnemyScalingData data)
    {
        _data = data;
    }

    /// <summary>
    /// 获取缩放类型的名称字符串
    /// </summary>
    /// <param name="type">缩放类型枚举</param>
    /// <returns>类型的名称（Linear/Exponential/Logarithmic/Plateau/Unknown）</returns>
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
