using System;
using System.Collections.Generic;
using Godot;
using ClawRPG.Scripts.Data.Enemy;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 敌人数据库主控制器 - 协调各子模块，管理敌人数据与实例
    /// </summary>
    public partial class EnemyDatabase : BaseSystem
    {
        private static EnemyDatabase _instance;
        public static EnemyDatabase Instance => _instance;
        
        // 子系统引用
        private EnemyDataProvider _dataProvider;
        private EnemySpawnLogic _spawnLogic;
        private EnemyDataValidator _validator;
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            InitializeSubsystems();
        }
        
        protected override string SystemName => "EnemyDatabase";
        
        /// <summary>
        /// 初始化子系统
        /// </summary>
        private void InitializeSubsystems()
        {
            _dataProvider = GetNodeOrNull<EnemyDataProvider>("EnemyDataProvider");
            _spawnLogic = GetNodeOrNull<EnemySpawnLogic>("EnemySpawnLogic");
            _validator = GetNodeOrNull<EnemyDataValidator>("EnemyDataValidator");
            
            // 如果节点不存在，创建默认实现
            if (_dataProvider == null)
            {
                _dataProvider = new EnemyDataProvider();
                AddChild(_dataProvider);
            }
            
            if (_spawnLogic == null)
            {
                _spawnLogic = new EnemySpawnLogic();
                AddChild(_spawnLogic);
            }
            
            if (_validator == null)
            {
                _validator = new EnemyDataValidator();
                AddChild(_validator);
            }
            
            GD.Print($"[EnemyDatabase] Initialized with {_dataProvider.GetEnemyCount()} enemy types");
        }
        
        #region Delegated Operations - Data Provider
        
        /// <summary>
        /// 获取敌人类型
        /// </summary>
        public EnemyType GetEnemyType(string typeId)
        {
            return _dataProvider?.GetEnemyType(typeId);
        }
        
        /// <summary>
        /// 获取所有敌人类型
        /// </summary>
        public Dictionary<string, EnemyType> GetAllEnemyTypes()
        {
            return _dataProvider?.GetAllEnemyTypes() ?? new Dictionary<string, EnemyType>();
        }
        
        /// <summary>
        /// 注册敌人类型
        /// </summary>
        public void RegisterEnemyType(EnemyType enemyType)
        {
            _dataProvider?.RegisterEnemyType(enemyType);
        }
        
        /// <summary>
        /// 检查敌人类型是否存在
        /// </summary>
        public bool HasEnemyType(string typeId)
        {
            return _dataProvider?.HasEnemyType(typeId) ?? false;
        }
        
        /// <summary>
        /// 获取敌人类型列表
        /// </summary>
        public List<EnemyType> GetEnemyTypeList()
        {
            return _dataProvider?.GetEnemyTypeList() ?? new List<EnemyType>();
        }
        
        /// <summary>
        /// 根据区域获取敌人
        /// </summary>
        public List<EnemyType> GetEnemiesByRegion(string region)
        {
            return _dataProvider?.GetEnemiesByRegion(region) ?? new List<EnemyType>();
        }
        
        /// <summary>
        /// 根据玩家等级获取合适敌人
        /// </summary>
        public List<EnemyType> GetEnemiesForLevel(int playerLevel)
        {
            return _dataProvider?.GetEnemiesForLevel(playerLevel) ?? new List<EnemyType>();
        }
        
        #endregion
        
        #region Delegated Operations - Spawn Logic
        
        /// <summary>
        /// 创建敌人实例
        /// </summary>
        public EnemyInstance CreateEnemyInstance(string typeId)
        {
            return _spawnLogic?.CreateEnemyInstance(typeId);
        }
        
        /// <summary>
        /// 创建带等级的敌人实例
        /// </summary>
        public EnemyInstance CreateEnemyInstanceWithLevel(string typeId, int level)
        {
            return _spawnLogic?.CreateEnemyInstanceWithLevel(typeId, level);
        }
        
        /// <summary>
        /// 获取敌人实例
        /// </summary>
        public EnemyInstance GetEnemyInstance(int instanceId)
        {
            return _spawnLogic?.GetEnemyInstance(instanceId);
        }
        
        /// <summary>
        /// 移除敌人实例
        /// </summary>
        public bool RemoveEnemyInstance(int instanceId)
        {
            return _spawnLogic?.RemoveEnemyInstance(instanceId) ?? false;
        }
        
        /// <summary>
        /// 获取所有活跃敌人
        /// </summary>
        public List<EnemyInstance> GetActiveEnemies()
        {
            return _spawnLogic?.GetActiveEnemies() ?? new List<EnemyInstance>();
        }
        
        /// <summary>
        /// 获取敌人实例数量
        /// </summary>
        public int GetEnemyCount()
        {
            return _spawnLogic?.GetEnemyCount() ?? 0;
        }
        
        /// <summary>
        /// 获取活跃敌人数量
        /// </summary>
        public int GetActiveEnemyCount()
        {
            return _spawnLogic?.GetActiveEnemyCount() ?? 0;
        }
        
        /// <summary>
        /// 清除所有敌人实例
        /// </summary>
        public void ClearAllInstances()
        {
            _spawnLogic?.ClearAllInstances();
        }
        
        /// <summary>
        /// 击杀敌人
        /// </summary>
        public bool KillEnemy(int instanceId)
        {
            return _spawnLogic?.KillEnemy(instanceId) ?? false;
        }
        
        /// <summary>
        /// 对敌人造成伤害
        /// </summary>
        public bool DamageEnemy(int instanceId, float damage)
        {
            return _spawnLogic?.DamageEnemy(instanceId, damage) ?? false;
        }
        
        /// <summary>
        /// 治疗敌人
        /// </summary>
        public bool HealEnemy(int instanceId, float healAmount)
        {
            return _spawnLogic?.HealEnemy(instanceId, healAmount) ?? false;
        }
        
        /// <summary>
        /// 批量创建敌人波次
        /// </summary>
        public List<EnemyInstance> CreateEnemyWave(string[] typeIds)
        {
            return _spawnLogic?.CreateEnemyWave(typeIds) ?? new List<EnemyInstance>();
        }
        
        /// <summary>
        /// 设置刷新冷却
        /// </summary>
        public void SetSpawnCooldown(float cooldown)
        {
            _spawnLogic?.SetSpawnCooldown(cooldown);
        }
        
        #endregion
        
        #region Delegated Operations - Validator
        
        /// <summary>
        /// 验证敌人类型
        /// </summary>
        public EnemyDataValidator.ValidationResult ValidateEnemyType(EnemyType enemyType)
        {
            return _validator?.ValidateEnemyType(enemyType) ?? new EnemyDataValidator.ValidationResult { IsValid = false };
        }
        
        /// <summary>
        /// 验证敌人实例
        /// </summary>
        public EnemyDataValidator.ValidationResult ValidateEnemyInstance(EnemyInstance instance)
        {
            return _validator?.ValidateEnemyInstance(instance) ?? new EnemyDataValidator.ValidationResult { IsValid = false };
        }
        
        /// <summary>
        /// 设置验证严格模式
        /// </summary>
        public void SetValidationStrictMode(bool strict)
        {
            _validator?.SetStrictMode(strict);
        }
        
        /// <summary>
        /// 检查数据一致性
        /// </summary>
        public List<string> CheckDataConsistency()
        {
            return _validator?.CheckDataConsistency() ?? new List<string>();
        }
        
        #endregion
        
        #region Data Loading
        
        /// <summary>
        /// 从数据源加载敌人类型
        /// </summary>
        public void LoadEnemyTypesFromData(string dataPath)
        {
            GD.Print($"[EnemyDatabase] Loading enemy types from {dataPath}");
            // Implementation would load from JSON/CSV file
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>();
            
            // 导出子系统数据
            if (_spawnLogic != null)
            {
                data["spawnLogic"] = _spawnLogic.ExportSaveData();
            }
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("spawnLogic") && _spawnLogic != null)
            {
                _spawnLogic.ImportSaveData((Dictionary)data["spawnLogic"]);
            }
        }
        
        #endregion
        
        #region System Info
        
        /// <summary>
        /// 获取系统状态信息
        /// </summary>
        public string GetSystemStatus()
        {
            return $"[EnemyDatabase] Types: {_dataProvider?.GetEnemyCount() ?? 0}, " +
                   $"Instances: {_spawnLogic?.GetEnemyCount() ?? 0}, " +
                   $"Active: {_spawnLogic?.GetActiveEnemyCount() ?? 0}";
        }
        
        #endregion
    }
}
