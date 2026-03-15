using Godot;
using System;
using System.Collections.Generic;
using Project;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 难度缩放类
    /// 负责根据玩家等级、游戏进度等动态调整敌人属性
    /// </summary>
    public class EnemyDifficultyScaler : BaseSystem
    {
        private Dictionary<string, ScalingConfig> _scalingConfigs;
        private float _currentDifficultyMultiplier;
        
        public override void _Ready()
        {
            base._Ready();
            Initialize();
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            _scalingConfigs = new Dictionary<string, ScalingConfig>();
            _currentDifficultyMultiplier = 1.0f;
            LoadScalingConfigs();
        }
        
        /// <summary>
        /// 加载缩放配置
        /// </summary>
        private void LoadScalingConfigs()
        {
            GD.Print("[EnemyDifficultyScaler] Loading scaling configurations...");
        }
        
        /// <summary>
        /// 获取缩放后的敌人属性
        /// </summary>
        public EnemyType ScaleEnemy(EnemyType baseEnemy, int playerLevel, float progressMultiplier = 1.0f)
        {
            if (baseEnemy == null) return null;
            
            var scaledEnemy = new EnemyType
            {
                Id = baseEnemy.Id,
                Name = baseEnemy.Name,
                Description = baseEnemy.Description,
                SpritePath = baseEnemy.SpritePath,
                SpriteModulate = baseEnemy.SpriteModulate,
                MaxHealth = (int)(baseEnemy.MaxHealth * _currentDifficultyMultiplier * progressMultiplier),
                MoveSpeed = baseEnemy.MoveSpeed,
                AttackDamage = (int)(baseEnemy.AttackDamage * _currentDifficultyMultiplier * progressMultiplier),
                AttackRange = baseEnemy.AttackRange,
                AttackCooldown = baseEnemy.AttackCooldown,
                ChaseRange = baseEnemy.ChaseRange,
                DetectionRange = baseEnemy.DetectionRange,
                CriticalChance = baseEnemy.CriticalChance,
                CriticalDamage = baseEnemy.CriticalDamage,
                ExperienceReward = (int)(baseEnemy.ExperienceReward * _currentDifficultyMultiplier * progressMultiplier),
                GoldReward = (int)(baseEnemy.GoldReward * _currentDifficultyMultiplier * progressMultiplier),
                DropTable = new Dictionary<string, float>(baseEnemy.DropTable),
                CanChase = baseEnemy.CanChase,
                CanAttack = baseEnemy.CanAttack,
                IsAggressive = baseEnemy.IsAggressive,
                StatusEffectVulnerability = new Dictionary<string, float>(baseEnemy.StatusEffectVulnerability)
            };
            
            return scaledEnemy;
        }
        
        /// <summary>
        /// 设置难度倍数
        /// </summary>
        public void SetDifficultyMultiplier(float multiplier)
        {
            _currentDifficultyMultiplier = Mathf.Max(0.1f, multiplier);
        }
        
        /// <summary>
        /// 获取当前难度倍数
        /// </summary>
        public float GetDifficultyMultiplier()
        {
            return _currentDifficultyMultiplier;
        }
        
        /// <summary>
        /// 计算玩家等级对应的缩放值
        /// </summary>
        public float CalculateLevelScaling(int playerLevel, int baseLevel = 1)
        {
            if (playerLevel <= baseLevel) return 1.0f;
            return 1.0f + (playerLevel - baseLevel) * 0.1f;
        }
        
        /// <summary>
        /// 注册缩放配置
        /// </summary>
        public void RegisterScalingConfig(string enemyId, ScalingConfig config)
        {
            _scalingConfigs[enemyId] = config;
        }
        
        /// <summary>
        /// 获取缩放配置
        /// </summary>
        public ScalingConfig GetScalingConfig(string enemyId)
        {
            if (_scalingConfigs.TryGetValue(enemyId, out var config))
            {
                return config;
            }
            return null;
        }
        
        /// <summary>
        /// 难度缩放配置
        /// </summary>
        public class ScalingConfig
        {
            public string EnemyId;
            public float HealthScaling = 1.0f;
            public float AttackScaling = 1.0f;
            public float SpeedScaling = 1.0f;
            public float ExpScaling = 1.0f;
            public float DropScaling = 1.0f;
            public int MinPlayerLevel;
            public int MaxPlayerLevel;
        }
    }
}
