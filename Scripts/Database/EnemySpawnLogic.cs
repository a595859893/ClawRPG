using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 敌人生成与刷新逻辑 - 负责敌人的创建、实例管理和刷新
    /// </summary>
    public partial class EnemySpawnLogic : BaseSystem
    {
        private static EnemySpawnLogic _instance;
        public static EnemySpawnLogic Instance => _instance;
        
        // 敌人实例存储
        private Dictionary<int, EnemyInstance> _enemyInstances = new Dictionary<int, EnemyInstance>();
        
        // ID 计数器
        private int _nextInstanceId = 1;
        
        // 刷新配置
        private float _spawnCooldown = 5.0f;
        private float _timeSinceLastSpawn = 0f;
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "EnemySpawnLogic";
        
        #region Enemy Instance Management
        
        /// <summary>
        /// 创建敌人实例
        /// </summary>
        public EnemyInstance CreateEnemyInstance(string typeId)
        {
            var dataProvider = EnemyDataProvider.Instance;
            if (dataProvider == null)
            {
                GD.PrintErr("[EnemySpawnLogic] EnemyDataProvider not found!");
                return null;
            }
            
            var enemyType = dataProvider.GetEnemyType(typeId);
            if (enemyType == null)
            {
                GD.PrintErr($"[EnemySpawnLogic] Enemy type '{typeId}' not found!");
                return null;
            }
            
            var instance = new EnemyInstance
            {
                InstanceId = _nextInstanceId++,
                TypeId = typeId,
                CurrentHp = enemyType.MaxHealth,
                MaxHp = enemyType.MaxHealth,
                Level = enemyType.DefaultLevel,
                IsAlive = true
            };
            
            _enemyInstances[instance.InstanceId] = instance;
            GD.Print($"[EnemySpawnLogic] Created enemy instance {instance.InstanceId} of type {typeId}");
            
            return instance;
        }
        
        /// <summary>
        /// 创建敌人实例（带等级）
        /// </summary>
        public EnemyInstance CreateEnemyInstanceWithLevel(string typeId, int level)
        {
            var dataProvider = EnemyDataProvider.Instance;
            if (dataProvider == null)
            {
                GD.PrintErr("[EnemySpawnLogic] EnemyDataProvider not found!");
                return null;
            }
            
            var enemyType = dataProvider.GetEnemyType(typeId);
            if (enemyType == null)
            {
                GD.PrintErr($"[EnemySpawnLogic] Enemy type '{typeId}' not found!");
                return null;
            }
            
            // 根据等级调整属性
            float levelMultiplier = 1.0f + (level - 1) * 0.1f;
            
            var instance = new EnemyInstance
            {
                InstanceId = _nextInstanceId++,
                TypeId = typeId,
                CurrentHp = enemyType.MaxHealth * levelMultiplier,
                MaxHp = enemyType.MaxHealth * levelMultiplier,
                Level = level,
                IsAlive = true
            };
            
            _enemyInstances[instance.InstanceId] = instance;
            GD.Print($"[EnemySpawnLogic] Created enemy instance {instance.InstanceId} of type {typeId} at level {level}");
            
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
            if (_enemyInstances.Remove(instanceId))
            {
                GD.Print($"[EnemySpawnLogic] Removed enemy instance {instanceId}");
                return true;
            }
            return false;
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
        /// 获取所有敌人实例
        /// </summary>
        public List<EnemyInstance> GetAllEnemyInstances()
        {
            return new List<EnemyInstance>(_enemyInstances.Values);
        }
        
        /// <summary>
        /// 获取敌人实例数量
        /// </summary>
        public int GetEnemyCount()
        {
            return _enemyInstances.Count;
        }
        
        /// <summary>
        /// 获取活跃敌人数量
        /// </summary>
        public int GetActiveEnemyCount()
        {
            int count = 0;
            foreach (var instance in _enemyInstances.Values)
            {
                if (instance.IsAlive) count++;
            }
            return count;
        }
        
        /// <summary>
        /// 清除所有敌人实例
        /// </summary>
        public void ClearAllInstances()
        {
            int count = _enemyInstances.Count;
            _enemyInstances.Clear();
            _nextInstanceId = 1;
            GD.Print($"[EnemySpawnLogic] Cleared {count} enemy instances");
        }
        
        /// <summary>
        /// 标记敌人为死亡
        /// </summary>
        public bool KillEnemy(int instanceId)
        {
            if (_enemyInstances.TryGetValue(instanceId, out var instance))
            {
                instance.IsAlive = false;
                GD.Print($"[EnemySpawnLogic] Enemy instance {instanceId} killed");
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 对敌人造成伤害
        /// </summary>
        public bool DamageEnemy(int instanceId, float damage)
        {
            if (_enemyInstances.TryGetValue(instanceId, out var instance))
            {
                if (!instance.IsAlive) return false;
                
                instance.CurrentHp -= damage;
                if (instance.CurrentHp <= 0)
                {
                    instance.CurrentHp = 0;
                    instance.IsAlive = false;
                    GD.Print($"[EnemySpawnLogic] Enemy instance {instanceId} died from damage");
                }
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 治疗敌人
        /// </summary>
        public bool HealEnemy(int instanceId, float healAmount)
        {
            if (_enemyInstances.TryGetValue(instanceId, out var instance))
            {
                if (!instance.IsAlive) return false;
                
                instance.CurrentHp = Mathf.Min(instance.CurrentHp + healAmount, instance.MaxHp);
                return true;
            }
            return false;
        }
        
        /// <summary>
        /// 根据类型获取敌人实例
        /// </summary>
        public List<EnemyInstance> GetEnemiesByType(string typeId)
        {
            var result = new List<EnemyInstance>();
            foreach (var instance in _enemyInstances.Values)
            {
                if (instance.TypeId == typeId)
                {
                    result.Add(instance);
                }
            }
            return result;
        }
        
        #endregion
        
        #region Spawn Logic
        
        /// <summary>
        /// 设置刷新冷却时间
        /// </summary>
        public void SetSpawnCooldown(float cooldown)
        {
            _spawnCooldown = Mathf.Max(0.1f, cooldown);
        }
        
        /// <summary>
        /// 获取刷新冷却时间
        /// </summary>
        public float GetSpawnCooldown()
        {
            return _spawnCooldown;
        }
        
        /// <summary>
        /// 处理刷新逻辑（应在 _Process 中调用）
        /// </summary>
        public override void _Process(double delta)
        {
            _timeSinceLastSpawn += (float)delta;
        }
        
        /// <summary>
        /// 检查是否可以刷新
        /// </summary>
        public bool CanSpawn()
        {
            return _timeSinceLastSpawn >= _spawnCooldown;
        }
        
        /// <summary>
        /// 重置刷新计时器
        /// </summary>
        public void ResetSpawnTimer()
        {
            _timeSinceLastSpawn = 0f;
        }
        
        /// <summary>
        /// 批量创建敌人
        /// </summary>
        public List<EnemyInstance> CreateEnemyWave(string[] typeIds)
        {
            var instances = new List<EnemyInstance>();
            foreach (var typeId in typeIds)
            {
                var instance = CreateEnemyInstance(typeId);
                if (instance != null)
                {
                    instances.Add(instance);
                }
            }
            return instances;
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
}
