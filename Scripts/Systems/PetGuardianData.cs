using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 宠物守护系统数据
    /// 宠物守护模式 - 宠物在玩家周围巡逻，自动攻击靠近的敌人
    /// </summary>
    public class PetGuardianData
    {
        // 守护模式状态
        public enum GuardianMode
        {
            Inactive,     // 未激活
            Patrol,       // 巡逻中
            Engaging,     // 战斗中
            Returning     // 返回中
        }

        // 守护状态
        public enum GuardianState
        {
            Idle,
            Patrol,
            Chase,
            Attack,
            Return
        }

        // 守护配置
        public class GuardianConfig
        {
            public string PetId { get; set; }
            public float PatrolRadius { get; set; } = 150f;      // 巡逻半径
            public float DetectionRadius { get; set; } = 200f;   // 检测半径
            public float AttackRadius { get; set; } = 80f;       // 攻击半径
            public float ChaseSpeed { get; set; } = 200f;        // 追逐速度
            public float PatrolSpeed { get; set; } = 100f;       // 巡逻速度
            public float ReturnSpeed { get; set; } = 150f;       // 返回速度
            public float AttackCooldown { get; set; } = 1.5f;    // 攻击冷却
            public float DecisionInterval { get; set; } = 0.5f;   // 决策间隔
            public bool AutoAttack { get; set; } = true;          // 自动攻击
            public bool PrioritizeLowHealth { get; set; } = true; // 优先攻击低血量敌人
        }

        // 单个宠物守护数据
        public class PetGuardianInfo
        {
            public string PetId { get; set; }
            public GuardianMode Mode { get; set; } = GuardianMode.Inactive;
            public GuardianState State { get; set; } = GuardianState.Idle;
            public Vector2 PatrolCenter { get; set; }
            public Vector2 CurrentTargetPosition { get; set; }
            public Node2D CurrentTarget { get; set; }
            public float LastAttackTime { get; set; }
            public float LastDecisionTime { get; set; }
            public float TimeInState { get; set; }
            public int EnemiesDetected { get; set; }
            public int EnemiesAttacked { get; set; }
            public int EnemiesDefeated { get; set; }
        }

        // 玩家守护数据
        public class PlayerGuardianData
        {
            public Dictionary<string, PetGuardianInfo> ActivePets { get; set; } = new();
            public bool IsGuardianModeActive { get; set; }
            public float GlobalDetectionRadius { get; set; } = 250f;
            public int TotalEnemiesDefeated { get; set; }
        }

        // 默认配置
        public static GuardianConfig GetDefaultConfig()
        {
            return new GuardianConfig
            {
                PatrolRadius = 150f,
                DetectionRadius = 200f,
                AttackRadius = 80f,
                ChaseSpeed = 200f,
                PatrolSpeed = 100f,
                ReturnSpeed = 150f,
                AttackCooldown = 1.5f,
                DecisionInterval = 0.5f,
                AutoAttack = true,
                PrioritizeLowHealth = true
            };
        }

        // 根据宠物类型获取配置
        public static GuardianConfig GetConfigForPetType(string petType)
        {
            var config = GetDefaultConfig();
            
            switch (petType.ToLower())
            {
                case "wolf":
                case "dog":
                    config.DetectionRadius = 250f;
                    config.AttackRadius = 100f;
                    config.ChaseSpeed = 220f;
                    break;
                    
                case "cat":
                case "cat_":
                    config.DetectionRadius = 180f;
                    config.AttackRadius = 70f;
                    config.PatrolSpeed = 120f;
                    break;
                    
                case "bird":
                case "eagle":
                    config.DetectionRadius = 300f;
                    config.PatrolRadius = 200f;
                    config.ChaseSpeed = 250f;
                    break;
                    
                case "bear":
                    config.DetectionRadius = 150f;
                    config.AttackRadius = 120f;
                    config.ChaseSpeed = 160f;
                    break;
                    
                case "dragon":
                    config.DetectionRadius = 350f;
                    config.AttackRadius = 150f;
                    config.ChaseSpeed = 280f;
                    break;
            }
            
            return config;
        }
    }
}
