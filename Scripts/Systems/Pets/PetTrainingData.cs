using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物训练数据
    /// </summary>
    public class PetTrainingData
    {
        // 训练类型
        public enum TrainingType
        {
            Attack,      // 攻击训练
            Defense,     // 防御训练
            Speed,      // 速度训练
            Health,     // 生命训练
            Critical,   // 暴击训练
            Special     // 特殊训练
        }

        // 训练项目
        public class TrainingProject
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            public TrainingType Type { get; set; }
            public int Level { get; set; }              // 训练等级 1-10
            public int RequiredLevel { get; set; }       // 宠物需要等级
            public int GoldCost { get; set; }            // 金币费用
            public int TrainingPoints { get; set; }      // 训练点数消耗
            public float AttackBonus { get; set; }
            public float DefenseBonus { get; set; }
            public float HealthBonus { get; set; }
            public float SpeedBonus { get; set; }
            public float CriticalRateBonus { get; set; }
            public float CriticalDamageBonus { get; set; }
            public float LifeStealBonus { get; set; }
            public int Duration { get; set; }            // 训练持续时间(秒)
            public int[] RequiredMaterials { get; set; } // 材料ID数组
            public int[] MaterialCounts { get; set; }    // 材料数量
        }

        // 单次训练实例
        public class TrainingSession
        {
            public string Id { get; set; }
            public string PetId { get; set; }
            public string ProjectId { get; set; }
            public DateTime StartTime { get; set; }
            public int Duration { get; set; }
            public bool Completed { get; set; }
            public bool Claimed { get; set; }
        }

        // 玩家训练数据
        public class PlayerTrainingData
        {
            public int TrainingPoints { get; set; }              // 可用训练点数
            public int TotalTrainingPoints { get; set; }         // 累计获得训练点数
            public List<TrainingSession> ActiveSessions { get; set; } = new();
            public List<TrainingSession> CompletedSessions { get; set; } = new();
            public Dictionary<string, int> ProjectLevels { get; set; } = new(); // 项目等级
            public int TotalTrainingCount { get; set; }
            public int GoldSpentOnTraining { get; set; }
        }
    }
}
