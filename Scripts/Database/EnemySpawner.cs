using System;
using System.Collections.Generic;
using Godot;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 敌人生成器 - 负责敌人的生成、刷新、生成点管理等功能
    /// </summary>
    public partial class EnemySpawner : BaseSystem
    {
        private static EnemySpawner _instance;
        public static EnemySpawner Instance => _instance;
        
        // 生成点配置
        private List<SpawnPoint> _spawnPoints = new List<SpawnPoint>();
        
        // 活跃生成器
        private List<Spawner> _activeSpawners = new List<Spawner>();
        
        // 生成配置
        private SpawnConfig _defaultConfig = new SpawnConfig();
        
        public override void _Ready()
        {
            base._Ready();
            _instance = this;
        }
        
        protected override string SystemName => "EnemySpawner";
        
        #region Spawn Point Management
        
        /// <summary>
        /// 添加生成点
        /// </summary>
        public void AddSpawnPoint(SpawnPoint point)
        {
            _spawnPoints.Add(point);
        }
        
        /// <summary>
        /// 移除生成点
        /// </summary>
        public void RemoveSpawnPoint(int index)
        {
            if (index >= 0 && index < _spawnPoints.Count)
            {
                _spawnPoints.RemoveAt(index);
            }
        }
        
        /// <summary>
        /// 获取所有生成点
        /// </summary>
        public List<SpawnPoint> GetSpawnPoints()
        {
            return new List<SpawnPoint>(_spawnPoints);
        }
        
        /// <summary>
        /// 获取随机生成点
        /// </summary>
        public SpawnPoint GetRandomSpawnPoint()
        {
            if (_spawnPoints.Count == 0)
                return null;
            
            var random = new Random();
            return _spawnPoints[random.Next(_spawnPoints.Count)];
        }
        
        /// <summary>
        /// 获取指定区域的生成点
        /// </summary>
        public List<SpawnPoint> GetSpawnPointsInArea(Vector3 center, float radius)
        {
            var result = new List<SpawnPoint>();
            foreach (var point in _spawnPoints)
            {
                if (point.Position.DistanceTo(center) <= radius)
                {
                    result.Add(point);
                }
            }
            return result;
        }
        
        #endregion
        
        #region Spawning
        
        /// <summary>
        /// 生成单个敌人
        /// </summary>
        public EnemyInstance SpawnEnemy(string typeId, Vector3 position)
        {
            var instance = EnemyDatabase.Instance?.CreateEnemyInstance(typeId);
            if (instance != null)
            {
                instance.Position = position;
            }
            return instance;
        }
        
        /// <summary>
        /// 生成一波敌人
        /// </summary>
        public List<EnemyInstance> SpawnWave(List<string> typeIds, Vector3 center, float spread)
        {
            var instances = new List<EnemyInstance>();
            var random = new Random();
            
            foreach (var typeId in typeIds)
            {
                var offset = new Vector3(
                    (float)(random.NextDouble() * 2 - 1) * spread,
                    0,
                    (float)(random.NextDouble() * 2 - 1) * spread
                );
                
                var instance = SpawnEnemy(typeId, center + offset);
                if (instance != null)
                {
                    instances.Add(instance);
                }
            }
            
            return instances;
        }
        
        /// <summary>
        /// 生成指定数量的敌人
        /// </summary>
        public List<EnemyInstance> SpawnCount(string typeId, int count, Vector3 center, float spread)
        {
            var instances = new List<EnemyInstance>();
            var typeIds = new List<string>();
            
            for (int i = 0; i < count; i++)
            {
                typeIds.Add(typeId);
            }
            
            return SpawnWave(typeIds, center, spread);
        }
        
        /// <summary>
        /// 移除敌人
        /// </summary>
        public void DespawnEnemy(int instanceId)
        {
            EnemyDatabase.Instance?.RemoveEnemyInstance(instanceId);
        }
        
        /// <summary>
        /// 移除所有敌人
        /// </summary>
        public void DespawnAll()
        {
            EnemyDatabase.Instance?.ClearAllInstances();
        }
        
        #endregion
        
        #region Spawner Management
        
        /// <summary>
        /// 创建生成器
        /// </summary>
        public Spawner CreateSpawner(SpawnerConfig config)
        {
            var spawner = new Spawner(config);
            _activeSpawners.Add(spawner);
            return spawner;
        }
        
        /// <summary>
        /// 移除生成器
        /// </summary>
        public void RemoveSpawner(Spawner spawner)
        {
            _activeSpawners.Remove(spawner);
        }
        
        /// <summary>
        /// 更新所有生成器
        /// </summary>
        public void UpdateSpawners(float delta)
        {
            var expiredSpawners = new List<Spawner>();
            
            foreach (var spawner in _activeSpawners)
            {
                spawner.Update(delta);
                
                if (spawner.IsExpired)
                {
                    expiredSpawners.Add(spawner);
                }
            }
            
            foreach (var spawner in expiredSpawners)
            {
                _activeSpawners.Remove(spawner);
            }
        }
        
        #endregion
        
        #region Persistence
        
        public override Dictionary ExportSaveData()
        {
            var data = new Dictionary();
            
            // Export spawn points
            var pointsArray = new Array();
            foreach (var point in _spawnPoints)
            {
                pointsArray.Add(JsonSerializer.Serialize(point));
            }
            data["spawnPoints"] = pointsArray;
            
            // Export spawners
            var spawnersArray = new Array();
            foreach (var spawner in _activeSpawners)
            {
                spawnersArray.Add(JsonSerializer.Serialize(spawner.Config));
            }
            data["activeSpawners"] = spawnersArray;
            
            return data;
        }
        
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            _spawnPoints.Clear();
            _activeSpawners.Clear();
            
            // Import spawn points
            if (data.Contains("spawnPoints"))
            {
                var pointsArray = (Array)data["spawnPoints"];
                foreach (string pointJson in pointsArray)
                {
                    var point = JsonSerializer.Deserialize<SpawnPoint>(pointJson);
                    if (point != null)
                    {
                        _spawnPoints.Add(point);
                    }
                }
            }
            
            // Recreate spawners
            if (data.Contains("activeSpawners"))
            {
                var spawnersArray = (Array)data["activeSpawners"];
                foreach (string configJson in spawnersArray)
                {
                    var config = JsonSerializer.Deserialize<SpawnerConfig>(configJson);
                    if (config != null)
                    {
                        CreateSpawner(config);
                    }
                }
            }
        }
        
        #endregion
    }
    
    /// <summary>
    /// 生成点
    /// </summary>
    public class SpawnPoint
    {
        public Vector3 Position { get; set; }
        public float Radius { get; set; } = 5f;
        public bool IsActive { get; set; } = true;
        public string Tag { get; set; }
    }
    
    /// <summary>
    /// 生成配置
    /// </summary>
    public class SpawnConfig
    {
        public float SpawnInterval { get; set; } = 5f;
        public int MaxEnemies { get; set; } = 10;
        public bool AutoRespawn { get; set; } = true;
    }
    
    /// <summary>
    /// 生成器配置
    /// </summary>
    public class SpawnerConfig
    {
        public string EnemyTypeId { get; set; }
        public Vector3 Center { get; set; }
        public float SpawnRadius { get; set; }
        public float Interval { get; set; }
        public int MaxCount { get; set; }
        public float Duration { get; set; }
    }
    
    /// <summary>
    /// 生成器
    /// </summary>
    public class Spawner
    {
        public SpawnerConfig Config { get; }
        private float _elapsedTime;
        private int _spawnedCount;
        
        public Spawner(SpawnerConfig config)
        {
            Config = config;
        }
        
        public float ElapsedTime => _elapsedTime;
        public int SpawnedCount => _spawnedCount;
        public bool IsExpired => Config.Duration > 0 && _elapsedTime >= Config.Duration;
        
        public void Update(float delta)
        {
            _elapsedTime += delta;
            
            if (_spawnedCount < Config.MaxCount)
            {
                var interval = Config.Interval;
                if (_elapsedTime >= interval * (_spawnedCount + 1))
                {
                    SpawnEnemy();
                }
            }
        }
        
        private void SpawnEnemy()
        {
            var random = new Random();
            var offset = new Vector3(
                (float)(random.NextDouble() * 2 - 1) * Config.SpawnRadius,
                0,
                (float)(random.NextDouble() * 2 - 1) * Config.SpawnRadius
            );
            
            EnemyDatabase.Instance?.CreateEnemyInstance(Config.EnemyTypeId);
            _spawnedCount++;
        }
    }
}
