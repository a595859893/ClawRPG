using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Data.Enemy;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 敌人数据验证器 - 负责验证敌人数据的有效性和一致性
    /// </summary>
    public partial class EnemyDataValidator : BaseSystem
    {
        private static EnemyDataValidator _instance;
        public static EnemyDataValidator Instance => _instance;
        
        // 验证规则配置
        private bool _strictMode = false;
        private float _maxHealthLimit = 100000f;
        private float _maxDamageLimit = 10000f;
        private float _maxSpeedLimit = 500f;
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "EnemyDataValidator";
        
        #region Configuration
        
        /// <summary>
        /// 设置验证严格模式
        /// </summary>
        public void SetStrictMode(bool strict)
        {
            _strictMode = strict;
            GD.Print($"[EnemyDataValidator] Strict mode: {_strictMode}");
        }
        
        /// <summary>
        /// 设置属性上限
        /// </summary>
        public void SetLimits(float maxHealth, float maxDamage, float maxSpeed)
        {
            _maxHealthLimit = maxHealth;
            _maxDamageLimit = maxDamage;
            _maxSpeedLimit = maxSpeed;
        }
        
        #endregion
        
        #region EnemyType Validation
        
        /// <summary>
        /// 验证敌人类型数据
        /// </summary>
        public ValidationResult ValidateEnemyType(EnemyType enemyType)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (enemyType == null)
            {
                result.IsValid = false;
                result.Errors.Add("Enemy type is null");
                return result;
            }
            
            // 验证ID
            if (string.IsNullOrEmpty(enemyType.Id))
            {
                result.IsValid = false;
                result.Errors.Add("Enemy ID is null or empty");
            }
            
            // 验证名称
            if (string.IsNullOrEmpty(enemyType.Name))
            {
                result.IsValid = false;
                result.Errors.Add("Enemy name is null or empty");
            }
            
            // 验证生命值
            if (enemyType.MaxHealth <= 0)
            {
                result.IsValid = false;
                result.Errors.Add($"Invalid MaxHealth: {enemyType.MaxHealth} (must be > 0)");
            }
            else if (enemyType.MaxHealth > _maxHealthLimit)
            {
                result.Warnings.Add($"MaxHealth exceeds limit: {enemyType.MaxHealth} > {_maxHealthLimit}");
                if (_strictMode) result.IsValid = false;
            }
            
            // 验证移动速度
            if (enemyType.MoveSpeed <= 0)
            {
                result.Warnings.Add($"Invalid MoveSpeed: {enemyType.MoveSpeed} (should be > 0)");
            }
            else if (enemyType.MoveSpeed > _maxSpeedLimit)
            {
                result.Warnings.Add($"MoveSpeed exceeds limit: {enemyType.MoveSpeed} > {_maxSpeedLimit}");
                if (_strictMode) result.IsValid = false;
            }
            
            // 验证攻击力
            if (enemyType.AttackDamage < 0)
            {
                result.Warnings.Add($"Invalid AttackDamage: {enemyType.AttackDamage} (should be >= 0)");
            }
            else if (enemyType.AttackDamage > _maxDamageLimit)
            {
                result.Warnings.Add($"AttackDamage exceeds limit: {enemyType.AttackDamage} > {_maxDamageLimit}");
                if (_strictMode) result.IsValid = false;
            }
            
            // 验证攻击范围
            if (enemyType.AttackRange < 0)
            {
                result.Warnings.Add($"Invalid AttackRange: {enemyType.AttackRange} (should be >= 0)");
            }
            
            // 验证检测范围
            if (enemyType.DetectionRange <= 0)
            {
                result.Warnings.Add($"Invalid DetectionRange: {enemyType.DetectionRange} (should be > 0)");
            }
            
            // 验证追逐范围
            if (enemyType.ChaseRange <= 0)
            {
                result.Warnings.Add($"Invalid ChaseRange: {enemyType.ChaseRange} (should be > 0)");
            }
            
            if (enemyType.ChaseRange > enemyType.DetectionRange)
            {
                result.Warnings.Add($"ChaseRange > DetectionRange: {enemyType.ChaseRange} > {enemyType.DetectionRange}");
            }
            
            // 验证攻击冷却
            if (enemyType.AttackCooldown <= 0)
            {
                result.Warnings.Add($"Invalid AttackCooldown: {enemyType.AttackCooldown} (should be > 0)");
            }
            
            return result;
        }
        
        /// <summary>
        /// 批量验证敌人类型
        /// </summary>
        public List<ValidationResult> ValidateEnemyTypes(Dictionary<string, EnemyType> enemyTypes)
        {
            var results = new List<ValidationResult>();
            foreach (var kvp in enemyTypes)
            {
                var result = ValidateEnemyType(kvp.Value);
                result.EntityId = kvp.Key;
                results.Add(result);
            }
            return results;
        }
        
        #endregion
        
        #region EnemyInstance Validation
        
        /// <summary>
        /// 验证敌人实例数据
        /// </summary>
        public ValidationResult ValidateEnemyInstance(EnemyInstance instance)
        {
            var result = new ValidationResult { IsValid = true };
            
            if (instance == null)
            {
                result.IsValid = false;
                result.Errors.Add("Enemy instance is null");
                return result;
            }
            
            // 验证实例ID
            if (instance.InstanceId <= 0)
            {
                result.IsValid = false;
                result.Errors.Add($"Invalid InstanceId: {instance.InstanceId} (must be > 0)");
            }
            
            // 验证类型ID
            if (string.IsNullOrEmpty(instance.TypeId))
            {
                result.IsValid = false;
                result.Errors.Add("TypeId is null or empty");
            }
            else
            {
                // 检查对应的敌人类型是否存在
                var dataProvider = EnemyDataProvider.Instance;
                if (dataProvider != null && !dataProvider.HasEnemyType(instance.TypeId))
                {
                    result.Warnings.Add($"TypeId '{instance.TypeId}' does not exist in database");
                    if (_strictMode) result.IsValid = false;
                }
            }
            
            // 验证生命值
            if (instance.CurrentHp < 0)
            {
                result.IsValid = false;
                result.Errors.Add($"CurrentHp cannot be negative: {instance.CurrentHp}");
            }
            
            if (instance.MaxHp <= 0)
            {
                result.IsValid = false;
                result.Errors.Add($"Invalid MaxHp: {instance.MaxHp} (must be > 0)");
            }
            
            if (instance.CurrentHp > instance.MaxHp)
            {
                result.Warnings.Add($"CurrentHp ({instance.CurrentHp}) > MaxHp ({instance.MaxHp})");
                if (_strictMode) result.IsValid = false;
            }
            
            // 验证等级
            if (instance.Level < 1)
            {
                result.Warnings.Add($"Invalid Level: {instance.Level} (should be >= 1)");
                if (_strictMode) result.IsValid = false;
            }
            
            return result;
        }
        
        /// <summary>
        /// 批量验证敌人实例
        /// </summary>
        public List<ValidationResult> ValidateEnemyInstances(Dictionary<int, EnemyInstance> instances)
        {
            var results = new List<ValidationResult>();
            foreach (var kvp in instances)
            {
                var result = ValidateEnemyInstance(kvp.Value);
                result.EntityId = kvp.Key.ToString();
                results.Add(result);
            }
            return results;
        }
        
        #endregion
        
        #region Spawn Validation
        
        /// <summary>
        /// 验证生成参数
        /// </summary>
        public ValidationResult ValidateSpawnParams(string typeId, Vector3 position, int level = 1)
        {
            var result = new ValidationResult { IsValid = true };
            
            // 验证类型ID
            if (string.IsNullOrEmpty(typeId))
            {
                result.IsValid = false;
                result.Errors.Add("typeId is null or empty");
                return result;
            }
            
            // 验证位置
            if (position == null)
            {
                result.IsValid = false;
                result.Errors.Add("Position is null");
            }
            
            // 验证等级
            if (level < 1)
            {
                result.Warnings.Add($"Invalid level: {level} (should be >= 1)");
                if (_strictMode) result.IsValid = false;
            }
            
            // 验证敌人类型存在
            var dataProvider = EnemyDataProvider.Instance;
            if (dataProvider != null && !dataProvider.HasEnemyType(typeId))
            {
                result.IsValid = false;
                result.Errors.Add($"Enemy type '{typeId}' does not exist");
            }
            
            return result;
        }
        
        /// <summary>
        /// 检查实例数量限制
        /// </summary>
        public bool CheckInstanceLimit(int currentCount, int maxLimit)
        {
            return currentCount < maxLimit;
        }
        
        #endregion
        
        #region Data Consistency Checks
        
        /// <summary>
        /// 执行数据一致性检查
        /// </summary>
        public List<string> CheckDataConsistency()
        {
            var issues = new List<string>();
            
            var dataProvider = EnemyDataProvider.Instance;
            var spawnLogic = EnemySpawnLogic.Instance;
            
            if (dataProvider == null)
            {
                issues.Add("EnemyDataProvider is not available");
                return issues;
            }
            
            if (spawnLogic == null)
            {
                issues.Add("EnemySpawnLogic is not available");
                return issues;
            }
            
            // 检查所有实例的TypeId是否都有效
            var instances = spawnLogic.GetAllEnemyInstances();
            var validTypeIds = new HashSet<string>();
            foreach (var type in dataProvider.GetAllEnemyTypes().Keys)
            {
                validTypeIds.Add(type);
            }
            
            foreach (var instance in instances)
            {
                if (!validTypeIds.Contains(instance.TypeId))
                {
                    issues.Add($"Instance {instance.InstanceId} has invalid TypeId: {instance.TypeId}");
                }
            }
            
            return issues;
        }
        
        #endregion

        #region BaseSystem Persistence

        public override Dictionary<string, object> ExportSaveData() => new();
        public override void ImportSaveData(Dictionary<string, object> data) { }

        #endregion
        
        #region Validation Result
        
        /// <summary>
        /// 验证结果
        /// </summary>
        public class ValidationResult
        {
            public bool IsValid { get; set; }
            public string EntityId { get; set; }
            public List<string> Errors { get; set; } = new List<string>();
            public List<string> Warnings { get; set; } = new List<string>();
            
            public bool HasErrors => Errors.Count > 0;
            public bool HasWarnings => Warnings.Count > 0;
        }
        
        #endregion
    }
}
