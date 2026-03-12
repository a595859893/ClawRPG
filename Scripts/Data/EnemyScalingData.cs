using Godot;
using System;
using System.Collections.Generic;

public class EnemyScalingData
{
    // 缩放类型
    public enum ScalingType
    {
        Linear,       // 线性增长
        Exponential,  // 指数增长
        Logarithmic,  // 对数增长
        Plateau,      //  plateau增长（前期快后期慢）
    }

    // 缩放参数
    public class ScalingParameters
    {
        public float BaseValue;           // 基础值
        public float GrowthRate;          // 增长率
        public float MaxValue;            // 最大值
        public ScalingType Type;          // 缩放类型
    }

    // 敌人缩放配置
    public class EnemyScalingConfig
    {
        public string EnemyType;
        public ScalingParameters HealthScaling;
        public ScalingParameters AttackScaling;
        public ScalingParameters DefenseScaling;
        public ScalingParameters SpeedScaling;
        public float ExperienceMultiplier;
        public float DropRateMultiplier;
    }

    // 玩家进度追踪
    public int PlayerLevel { get; set; }
    public int FloorNumber { get; set; }
    public int EnemiesDefeated { get; set; }
    public int ComboCount { get; set; }
    public float PlayTimeMinutes { get; set; }
    public int CurrentWave { get; set; }

    // 统计
    public int TotalScalingCalculations { get; set; }
    public Dictionary<string, int> ScalingTypeUsage { get; set; }

    public EnemyScalingData()
    {
        PlayerLevel = 1;
        FloorNumber = 1;
        EnemiesDefeated = 0;
        ComboCount = 0;
        PlayTimeMinutes = 0;
        CurrentWave = 1;
        TotalScalingCalculations = 0;
        ScalingTypeUsage = new Dictionary<string, int>();
    }
}
