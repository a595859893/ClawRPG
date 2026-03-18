using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 敌人数据库 - 负责敌人数据的存储和管理
    /// </summary>
    public partial class EnemyDatabase : BaseSystem
    {
        private static EnemyDatabase _instance;
        public static EnemyDatabase Instance => _instance;
        
        // 敌人类型存储
        private Dictionary<string, EnemyType> _enemyTypes = new Dictionary<string, EnemyType>();
        
        // 敌人实例存储
        private Dictionary<int, EnemyInstance> _enemyInstances = new Dictionary<int, EnemyInstance>();
        
        // ID 计数器
        private int _nextInstanceId = 1;
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
            LoadEnemyData();
        }
        
        protected override string SystemName => "EnemyDatabase";
        
        #region Enemy Type Management
        
        /// <summary>
        /// 注册敌人类型
        /// </summary>
        public void RegisterEnemyType(EnemyType enemyType)
        {
            _enemyTypes[enemyType.Id] = enemyType;
        }
        
        /// <summary>
        /// 获取敌人类型
        /// </summary>
        public EnemyType GetEnemyType(string typeId)
        {
            return _enemyTypes.ContainsKey(typeId) ? _enemyTypes[typeId] : null;
        }
        
        /// <summary>
        /// 获取所有敌人类型
        /// </summary>
        public Dictionary<string, EnemyType> GetAllEnemyTypes()
        {
            return new Dictionary<string, EnemyType>(_enemyTypes);
        }
        
        /// <summary>
        /// 移除敌人类型
        /// </summary>
        public bool RemoveEnemyType(string typeId)
        {
            return _enemyTypes.Remove(typeId);
        }
        
        #endregion
        
        #region Enemy Instance Management
        
        /// <summary>
        /// 创建敌人实例
        /// </summary>
        public EnemyInstance CreateEnemyInstance(string typeId)
        {
            var enemyType = GetEnemyType(typeId);
            if (enemyType == null)
                return null;
            
            var instance = new EnemyInstance
            {
                InstanceId = _nextInstanceId++,
                TypeId = typeId,
                CurrentHp = enemyType.MaxHp,
                MaxHp = enemyType.MaxHp,
                Level = enemyType.DefaultLevel,
                IsAlive = true
            };
            
            _enemyInstances[instance.InstanceId] = instance;
            return instance;
        }
        
        /// <summary>
        /// 获取敌人实例
        /// </summary>
        public EnemyInstance GetEnemyInstance(int instanceId)
        {
            return _enemyInstances.ContainsKey(instanceId) ? _enemyInstances[instanceId] : null;
        }
        
        /// <summary>
        /// 移除敌人实例
        /// </summary>
        public bool RemoveEnemyInstance(int instanceId)
        {
            return _enemyInstances.Remove(instanceId);
        }
        
        /// <summary>
        /// 获取所有活跃敌人实例
        /// </summary>
        public List<EnemyInstance> GetActiveEnemies()
        {
            var result = new List<EnemyInstance>();
            foreach (var instance in _enemyInstances.Values)
            {
                if (instance.IsAlive)
                {
                    result.Add(instance);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 清除所有敌人实例
        /// </summary>
        public void ClearAllInstances()
        {
            _enemyInstances.Clear();
        }
        
        #endregion
        
        #region Data Loading
        
        private void LoadEnemyData()
        {
            // Load enemy types from data files
            // This is a placeholder - actual implementation would load from JSON/CSV
            GD.Print($"[EnemyDatabase] Loaded {_enemyTypes.Count} enemy types");
        }
        
        /// <summary>
        /// 从数据源加载敌人类型
        /// </summary>
        public void LoadEnemyTypesFromData(string dataPath)
        {
            // Implementation for loading from file
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // Export enemy instances
            var instancesArray = new Array();
            foreach (var instance in _enemyInstances.Values)
            {
                instancesArray.Add(JsonSerializer.Serialize(instance));
            }
            data["enemyInstances"] = instancesArray;
            
            data["nextInstanceId"] = _nextInstanceId;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            _enemyInstances.Clear();
            
            if (data.Contains("enemyInstances"))
            {
                var instancesArray = (Array)data["enemyInstances"];
                foreach (string instanceJson in instancesArray)
                {
                    var instance = JsonSerializer.Deserialize<EnemyInstance>(instanceJson);
                    if (instance != null)
                    {
                        _enemyInstances[instance.InstanceId] = instance;
                    }
                }
            }
            
            if (data.Contains("nextInstanceId"))
            {
                _nextInstanceId = Convert.ToInt32(data["nextInstanceId"]);
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 敌人实例
    /// </summary>
    public class EnemyInstance
    {
        public int InstanceId { get; set; }
        public string TypeId { get; set; }
        public float CurrentHp { get; set; }
        public float MaxHp { get; set; }
        public int Level { get; set; }
        public bool IsAlive { get; set; }
        public Vector3 Position { get; set; }
    }
}
