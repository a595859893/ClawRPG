using System;
using System.Collections.Generic;
using System.Linq;
using Godot;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 敌人查询系统 - 负责敌人的查询、过滤、排序等功能
    /// </summary>
    public partial class EnemyQuerySystem : BaseSystem
    {
        private static EnemyQuerySystem _instance;
        public static EnemyQuerySystem Instance => _instance;
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "EnemyQuery";
        
        #region Query Methods
        
        /// <summary>
        /// 按ID查询敌人
        /// </summary>
        public EnemyInstance QueryById(int instanceId)
        {
            return EnemyDatabase.Instance?.GetEnemyInstance(instanceId);
        }
        
        /// <summary>
        /// 按类型查询敌人
        /// </summary>
        public List<EnemyInstance> QueryByType(string typeId)
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null)
                return new List<EnemyInstance>();
            
            return allEnemies.Where(e => e.TypeId == typeId).ToList();
        }
        
        /// <summary>
        /// 按等级范围查询敌人
        /// </summary>
        public List<EnemyInstance> QueryByLevelRange(int minLevel, int maxLevel)
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null)
                return new List<EnemyInstance>();
            
            return allEnemies.Where(e => e.Level >= minLevel && e.Level <= maxLevel).ToList();
        }
        
        /// <summary>
        /// 按生命值百分比查询敌人
        /// </summary>
        public List<EnemyInstance> QueryByHealthPercent(float minPercent, float maxPercent)
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null)
                return new List<EnemyInstance>();
            
            return allEnemies.Where(e => {
                var percent = e.MaxHp > 0 ? e.CurrentHp / e.MaxHp : 0;
                return percent >= minPercent && percent <= maxPercent;
            }).ToList();
        }
        
        /// <summary>
        /// 按位置范围查询敌人
        /// </summary>
        public List<EnemyInstance> QueryByPosition(Vector3 center, float radius)
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null)
                return new List<EnemyInstance>();
            
            return allEnemies.Where(e => {
                var distance = e.Position.DistanceTo(center);
                return distance <= radius;
            }).ToList();
        }
        
        #endregion
        
        #region Filter Methods
        
        /// <summary>
        /// 获取最近敌人
        /// </summary>
        public EnemyInstance GetNearestEnemy(Vector3 position)
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null || allEnemies.Count == 0)
                return null;
            
            return allEnemies.OrderBy(e => e.Position.DistanceTo(position)).First();
        }
        
        /// <summary>
        /// 获取最弱敌人
        /// </summary>
        public EnemyInstance GetWeakestEnemy()
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null || allEnemies.Count == 0)
                return null;
            
            return allEnemies.OrderBy(e => e.CurrentHp).First();
        }
        
        /// <summary>
        /// 获取最强敌人
        /// </summary>
        public EnemyInstance GetStrongestEnemy()
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null || allEnemies.Count == 0)
                return null;
            
            return allEnemies.OrderByDescending(e => e.MaxHp).First();
        }
        
        /// <summary>
        /// 获取血量最低敌人
        /// </summary>
        public EnemyInstance GetLowestHealthEnemy()
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null || allEnemies.Count == 0)
                return null;
            
            return allEnemies.Where(e => e.CurrentHp > 0)
                            .OrderBy(e => e.CurrentHp / e.MaxHp)
                            .First();
        }
        
        #endregion
        
        #region Statistics
        
        /// <summary>
        /// 获取敌人数量统计
        /// </summary>
        public Dictionary<string, int> GetEnemyCountByType()
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null)
                return new Dictionary<string, int>();
            
            return allEnemies.GroupBy(e => e.TypeId)
                            .ToDictionary(g => g.Key, g => g.Count());
        }
        
        /// <summary>
        /// 获取总敌人数量
        /// </summary>
        public int GetTotalEnemyCount()
        {
            return EnemyDatabase.Instance?.GetActiveEnemies().Count ?? 0;
        }
        
        /// <summary>
        /// 获取平均等级
        /// </summary>
        public float GetAverageLevel()
        {
            var allEnemies = EnemyDatabase.Instance?.GetActiveEnemies();
            if (allEnemies == null || allEnemies.Count == 0)
                return 0;
            
            return allEnemies.Average(e => e.Level);
        }
        
        #endregion

        #region BaseSystem Persistence

        public override Dictionary ExportSaveData() => new();
        public override void ImportSaveData(Dictionary data) { }

        #endregion
    }
}
