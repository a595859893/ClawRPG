using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 敌人Spawn管理类
    /// 负责管理敌人的生成、刷新和区域控制
    /// </summary>
    public class EnemySpawnManager : BaseSystem
    {
        private Dictionary<string, List<Node>> _activeEnemies;
        private Dictionary<string, SpawnZone> _spawnZones;
        
        public override void _Ready()
        {
            base._Ready();
            Initialize();
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            _activeEnemies = new Dictionary<string, List<Node>>();
            _spawnZones = new Dictionary<string, SpawnZone>();
            LoadSpawnConfig();
        }
        
        /// <summary>
        /// 加载生成配置
        /// </summary>
        private void LoadSpawnConfig()
        {
            GD.Print("[EnemySpawnManager] Loading spawn configuration...");
        }
        
        /// <summary>
        /// 生成敌人
        /// </summary>
        public Node SpawnEnemy(string enemyId, Vector2 position, string zoneId = "default")
        {
            GD.Print($"[EnemySpawnManager] Spawning enemy: {enemyId} at {position}");
            return null;
        }
        
        /// <summary>
        /// 移除敌人
        /// </summary>
        public void RemoveEnemy(Node enemy, string zoneId = "default")
        {
            if (_activeEnemies.TryGetValue(zoneId, out var enemies))
            {
                enemies.Remove(enemy);
            }
        }
        
        /// <summary>
        /// 获取活跃敌人数量
        /// </summary>
        public int GetActiveEnemyCount(string zoneId = "default")
        {
            if (_activeEnemies.TryGetValue(zoneId, out var enemies))
            {
                return enemies.Count;
            }
            return 0;
        }
        
        /// <summary>
        /// 注册生成区域
        /// </summary>
        public void RegisterSpawnZone(string zoneId, SpawnZone zone)
        {
            _spawnZones[zoneId] = zone;
        }
        
        /// <summary>
        /// 获取生成区域
        /// </summary>
        public SpawnZone GetSpawnZone(string zoneId)
        {
            if (_spawnZones.TryGetValue(zoneId, out var zone))
            {
                return zone;
            }
            return null;
        }
        
        /// <summary>
        /// 清除所有敌人
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemies in _activeEnemies.Values)
            {
                foreach (var enemy in enemies)
                {
                    if (IsInstanceValid(enemy))
                    {
                        enemy.QueueFree();
                    }
                }
                enemies.Clear();
            }
        }

        #region BaseSystem Persistence

        public override Dictionary<string, object> ExportSaveData() => new();
        public override void ImportSaveData(Dictionary<string, object> data) { }

        #endregion
        
        /// <summary>
        /// 生成区域定义
        /// </summary>
        public class SpawnZone
        {
            public string Id;
            public Vector2 Position;
            public Vector2 Size;
            public int MaxEnemies;
            public float RespawnTime;
            public List<string> AllowedEnemyTypes;
        }
    }
}
