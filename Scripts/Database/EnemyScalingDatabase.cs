using Godot;
using System;
using System.Collections.Generic;

public class EnemyScalingDatabase
{
    // 单例
    private static EnemyScalingDatabase _instance;
    public static EnemyScalingDatabase Instance
    {
        get
        {
            if (_instance == null) _instance = new EnemyScalingDatabase();
            return _instance;
        }
    }

    // 敌人类型缩放配置
    private Dictionary<string, EnemyScalingData.EnemyScalingConfig> _enemyConfigs;

    // 难度等级配置
    private Dictionary<string, DifficultyConfig> _difficultyConfigs;

    // 难度配置
    public class DifficultyConfig
    {
        public string Name;
        public float HealthMultiplier;
        public float AttackMultiplier;
        public float DefenseMultiplier;
        public float SpeedMultiplier;
        public float ExpMultiplier;
        public float DropMultiplier;
    }

    public EnemyScalingDatabase()
    {
        InitializeConfigs();
    }

    private void InitializeConfigs()
    {
        _enemyConfigs = new Dictionary<string, EnemyScalingData.EnemyScalingConfig>();
        _difficultyConfigs = new Dictionary<string, DifficultyConfig>();

        // 初始化敌人缩放配置
        InitializeEnemyConfigs();

        // 初始化难度配置
        InitializeDifficultyConfigs();
    }

    private void InitializeEnemyConfigs()
    {
        // 史莱姆 - 低缩放
        _enemyConfigs["Slime"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Slime",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 50, GrowthRate = 5, MaxValue = 500, Type = EnemyScalingData.ScalingType.Linear },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 5, GrowthRate = 0.8, MaxValue = 50, Type = EnemyScalingData.ScalingType.Linear },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 0, GrowthRate = 0.2, MaxValue = 20, Type = EnemyScalingData.ScalingType.Linear },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 50, GrowthRate = 1, MaxValue = 100, Type = EnemyScalingData.ScalingType.Plateau },
            ExperienceMultiplier = 1.0f,
            DropRateMultiplier = 1.0f,
        };

        // 哥布林 - 中等缩放
        _enemyConfigs["Goblin"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Goblin",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 80, GrowthRate = 10, MaxValue = 800, Type = EnemyScalingData.ScalingType.Linear },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 10, GrowthRate = 1.5, MaxValue = 100, Type = EnemyScalingData.ScalingType.Linear },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 2, GrowthRate = 0.5, MaxValue = 40, Type = EnemyScalingData.ScalingType.Linear },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 60, GrowthRate = 1.5, MaxValue = 120, Type = EnemyScalingData.ScalingType.Plateau },
            ExperienceMultiplier = 1.2f,
            DropRateMultiplier = 1.2f,
        };

        // 骷髅 - 物理防御高
        _enemyConfigs["Skeleton"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Skeleton",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 100, GrowthRate = 12, MaxValue = 1000, Type = EnemyScalingData.ScalingType.Linear },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 12, GrowthRate = 1.8, MaxValue = 120, Type = EnemyScalingData.ScalingType.Linear },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 5, GrowthRate = 1, MaxValue = 80, Type = EnemyScalingData.ScalingType.Plateau },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 45, GrowthRate = 0.8, MaxValue = 90, Type = EnemyScalingData.ScalingType.Linear },
            ExperienceMultiplier = 1.3f,
            DropRateMultiplier = 1.3f,
        };

        // 元素生物 - 高缩放
        _enemyConfigs["Elemental"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Elemental",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 120, GrowthRate = 18, MaxValue = 1500, Type = EnemyScalingData.ScalingType.Exponential },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 15, GrowthRate = 2.5, MaxValue = 200, Type = EnemyScalingData.ScalingType.Exponential },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 3, GrowthRate = 0.8, MaxValue = 60, Type = EnemyScalingData.ScalingType.Linear },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 70, GrowthRate = 2, MaxValue = 150, Type = EnemyScalingData.ScalingType.Plateau },
            ExperienceMultiplier = 1.5f,
            DropRateMultiplier = 1.5f,
        };

        // 龙 - 最高缩放
        _enemyConfigs["Dragon"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Dragon",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 500, GrowthRate = 50, MaxValue = 5000, Type = EnemyScalingData.ScalingType.Exponential },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 30, GrowthRate = 5, MaxValue = 300, Type = EnemyScalingData.ScalingType.Exponential },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 10, GrowthRate = 2, MaxValue = 150, Type = EnemyScalingData.ScalingType.Exponential },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 40, GrowthRate = 1, MaxValue = 100, Type = EnemyScalingData.ScalingType.Linear },
            ExperienceMultiplier = 2.0f,
            DropRateMultiplier = 2.0f,
        };

        // 恶魔 - 高攻击
        _enemyConfigs["Demon"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Demon",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 200, GrowthRate = 25, MaxValue = 2000, Type = EnemyScalingData.ScalingType.Exponential },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 25, GrowthRate = 4, MaxValue = 250, Type = EnemyScalingData.ScalingType.Exponential },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 8, GrowthRate = 1.5, MaxValue = 100, Type = EnemyScalingData.ScalingType.Linear },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 65, GrowthRate = 1.5, MaxValue = 130, Type = EnemyScalingData.ScalingType.Plateau },
            ExperienceMultiplier = 1.8f,
            DropRateMultiplier = 1.8f,
        };

        // 机械敌人
        _enemyConfigs["Mechanical"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Mechanical",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 150, GrowthRate = 20, MaxValue = 1800, Type = EnemyScalingData.ScalingType.Exponential },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 18, GrowthRate = 3, MaxValue = 200, Type = EnemyScalingData.ScalingType.Linear },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 15, GrowthRate = 2.5, MaxValue = 180, Type = EnemyScalingData.ScalingType.Plateau },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 35, GrowthRate = 0.5, MaxValue = 70, Type = EnemyScalingData.ScalingType.Linear },
            ExperienceMultiplier = 1.4f,
            DropRateMultiplier = 1.2f,
        };

        // 亡灵
        _enemyConfigs["Undead"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Undead",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 90, GrowthRate = 12, MaxValue = 900, Type = EnemyScalingData.ScalingType.Linear },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 14, GrowthRate = 2, MaxValue = 140, Type = EnemyScalingData.ScalingType.Linear },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 4, GrowthRate = 0.8, MaxValue = 50, Type = EnemyScalingData.ScalingType.Logarithmic },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 50, GrowthRate = 1.2, MaxValue = 110, Type = EnemyScalingData.ScalingType.Linear },
            ExperienceMultiplier = 1.25f,
            DropRateMultiplier = 1.4f,
        };

        // 野兽
        _enemyConfigs["Beast"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Beast",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 110, GrowthRate = 15, MaxValue = 1200, Type = EnemyScalingData.ScalingType.Exponential },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 16, GrowthRate = 2.2, MaxValue = 160, Type = EnemyScalingData.ScalingType.Linear },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 3, GrowthRate = 0.6, MaxValue = 45, Type = EnemyScalingData.ScalingType.Linear },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 80, GrowthRate = 2.5, MaxValue = 180, Type = EnemyScalingData.ScalingType.Plateau },
            ExperienceMultiplier = 1.35f,
            DropRateMultiplier = 1.25f,
        };

        // Boss通用配置
        _enemyConfigs["Boss"] = new EnemyScalingData.EnemyScalingConfig
        {
            EnemyType = "Boss",
            HealthScaling = new EnemyScalingData.ScalingParameters { BaseValue = 1000, GrowthRate = 100, MaxValue = 10000, Type = EnemyScalingData.ScalingType.Exponential },
            AttackScaling = new EnemyScalingData.ScalingParameters { BaseValue = 50, GrowthRate = 8, MaxValue = 500, Type = EnemyScalingData.ScalingType.Exponential },
            DefenseScaling = new EnemyScalingData.ScalingParameters { BaseValue = 20, GrowthRate = 3, MaxValue = 200, Type = EnemyScalingData.ScalingType.Linear },
            SpeedScaling = new EnemyScalingData.ScalingParameters { BaseValue = 30, GrowthRate = 0.5, MaxValue = 80, Type = EnemyScalingData.ScalingType.Linear },
            ExperienceMultiplier = 3.0f,
            DropRateMultiplier = 3.0f,
        };
    }

    private void InitializeDifficultyConfigs()
    {
        _difficultyConfigs["Easy"] = new DifficultyConfig
        {
            Name = "Easy",
            HealthMultiplier = 0.7f,
            AttackMultiplier = 0.7f,
            DefenseMultiplier = 0.7f,
            SpeedMultiplier = 0.9f,
            ExpMultiplier = 0.8f,
            DropMultiplier = 1.0f,
        };

        _difficultyConfigs["Normal"] = new DifficultyConfig
        {
            Name = "Normal",
            HealthMultiplier = 1.0f,
            AttackMultiplier = 1.0f,
            DefenseMultiplier = 1.0f,
            SpeedMultiplier = 1.0f,
            ExpMultiplier = 1.0f,
            DropMultiplier = 1.0f,
        };

        _difficultyConfigs["Hard"] = new DifficultyConfig
        {
            Name = "Hard",
            HealthMultiplier = 1.5f,
            AttackMultiplier = 1.5f,
            DefenseMultiplier = 1.5f,
            SpeedMultiplier = 1.1f,
            ExpMultiplier = 1.3f,
            DropMultiplier = 1.3f,
        };

        _difficultyConfigs["Nightmare"] = new DifficultyConfig
        {
            Name = "Nightmare",
            HealthMultiplier = 2.5f,
            AttackMultiplier = 2.5f,
            DefenseMultiplier = 2.5f,
            SpeedMultiplier = 1.3f,
            ExpMultiplier = 2.0f,
            DropMultiplier = 2.0f,
        };

        _difficultyConfigs["Legendary"] = new DifficultyConfig
        {
            Name = "Legendary",
            HealthMultiplier = 4.0f,
            AttackMultiplier = 4.0f,
            DefenseMultiplier = 4.0f,
            SpeedMultiplier = 1.5f,
            ExpMultiplier = 3.0f,
            DropMultiplier = 3.0f,
        };
    }

    // 获取敌人缩放配置
    public EnemyScalingData.EnemyScalingConfig GetEnemyConfig(string enemyType)
    {
        if (_enemyConfigs.ContainsKey(enemyType))
            return _enemyConfigs[enemyType];
        return _enemyConfigs["Slime"]; // 默认返回史莱姆配置
    }

    // 获取难度配置
    public DifficultyConfig GetDifficultyConfig(string difficulty)
    {
        if (_difficultyConfigs.ContainsKey(difficulty))
            return _difficultyConfigs[difficulty];
        return _difficultyConfigs["Normal"];
    }

    // 获取所有敌人类型
    public List<string> GetAllEnemyTypes()
    {
        return new List<string>(_enemyConfigs.Keys);
    }

    // 获取所有难度等级
    public List<string> GetAllDifficulties()
    {
        return new List<string>(_difficultyConfigs.Keys);
    }
}
