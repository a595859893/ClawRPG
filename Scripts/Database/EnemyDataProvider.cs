using Godot;
using System;
using System.Collections.Generic;
using Project;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 敌人数据提供类
    /// 负责管理敌人类型数据的加载和查询
    /// </summary>
    public class EnemyDataProvider : BaseSystem
    {
        private Dictionary<string, EnemyType> _enemyTypes;
        
        public override void _Ready()
        {
            base._Ready();
            Initialize();
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            _enemyTypes = new Dictionary<string, EnemyType>();
            LoadEnemyData();
        }
        
        /// <summary>
        /// 加载敌人数据
        /// </summary>
        private void LoadEnemyData()
        {
            GD.Print("[EnemyDataProvider] Loading enemy data...");
        }
        
        /// <summary>
        /// 获取敌人类型
        /// </summary>
        public EnemyType GetEnemyType(string enemyId)
        {
            if (_enemyTypes.TryGetValue(enemyId, out var enemyType))
            {
                return enemyType;
            }
            return null;
        }
        
        /// <summary>
        /// 获取所有敌人类型
        /// </summary>
        public Dictionary<string, EnemyType> GetAllEnemyTypes()
        {
            return new Dictionary<string, EnemyType>(_enemyTypes);
        }
        
        /// <summary>
        /// 注册敌人类型
        /// </summary>
        public void RegisterEnemyType(EnemyType enemyType)
        {
            if (enemyType != null && !string.IsNullOrEmpty(enemyType.Id))
            {
                _enemyTypes[enemyType.Id] = enemyType;
            }
        }
        
        /// <summary>
        /// 获取敌人类型列表
        /// </summary>
        public List<EnemyType> GetEnemyTypeList()
        {
            return new List<EnemyType>(_enemyTypes.Values);
        }
        
        /// <summary>
        /// 检查敌人类型是否存在
        /// </summary>
        public bool HasEnemyType(string enemyId)
        {
            return _enemyTypes.ContainsKey(enemyId);
        }
    }
}
