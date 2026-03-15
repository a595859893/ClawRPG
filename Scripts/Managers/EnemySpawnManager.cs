using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Managers
{
    /// <summary>
    /// 敌人生成管理器 - 负责敌人的生成、波次管理和敌人生成配置
    /// </summary>
    public class EnemySpawnManager : BaseSystem
    {
        public static EnemySpawnManager Instance { get; private set; }
        
        /// <summary>
        /// 优先级（数值越小越先初始化）
        /// </summary>
        public override int Priority => 25;
        
        // Enemy container
        private Node2D _enemyContainer;
        
        // Enemy scenes
        private PackedScene _defaultEnemyScene;
        private Dictionary<string, PackedScene> _enemyScenes = new Dictionary<string, PackedScene>();
        
        // Active enemies
        private List<Node> _activeEnemies = new List<Node>();
        private Dictionary<int, Node> _enemiesById = new Dictionary<int, Node>();
        
        // Spawn configuration
        private int _maxEnemies = 50;
        private float _spawnRadius = 800f;
        private Vector2 _spawnCenter = Vector2.Zero;
        private bool _autoSpawn = true;
        
        // Wave management
        private int _currentWave = 0;
        private int _enemiesInWave = 10;
        private int _enemiesSpawnedInWave = 0;
        private int _enemiesKilledInWave = 0;
        private float _waveTimer = 0f;
        private float _timeBetweenWaves = 30f;
        private bool _waveInProgress = false;
        
        // Spawn rules
        private float _minSpawnInterval = 1f;
        private float _maxSpawnInterval = 3f;
        private float _currentSpawnTimer = 0f;
        
        // Events
        public event Action<Node> OnEnemySpawned;
        public event Action<Node> OnEnemyKilled;
        public event Action<int> OnWaveComplete;
        public event Action<int> OnWaveStart;
        
        public override void _Ready()
        {
            Instance = this;
            base._Ready();
            
            // Create enemy container
            CreateEnemyContainer();
        }
        
        private void CreateEnemyContainer()
        {
            _enemyContainer = new Node2D();
            _enemyContainer.Name = "Enemies";
            
            var mainNode = GetTree().CurrentScene;
            if (mainNode != null)
            {
                mainNode.AddChild(_enemyContainer);
            }
        }
        
        protected override void Initialize()
        {
            GD.Print("[EnemySpawnManager] Initialized");
            StartWave(1);
        }
        
        /// <summary>
        /// 设置默认敌人场景
        /// </summary>
        public void SetDefaultEnemyScene(PackedScene scene)
        {
            _defaultEnemyScene = scene;
        }
        
        /// <summary>
        /// 注册敌人类型场景
        /// </summary>
        public void RegisterEnemyScene(string enemyType, PackedScene scene)
        {
            _enemyScenes[enemyType] = scene;
        }
        
        /// <summary>
        /// 生成敌人
        /// </summary>
        public Node SpawnEnemy(string enemyType = "", Vector2? position = null)
        {
            // Check max enemies limit
            if (_activeEnemies.Count >= _maxEnemies)
            {
                GD.PrintWarn("[EnemySpawnManager] Max enemies reached!");
                return null;
            }
            
            PackedScene scene = null;
            
            // Try to get specific enemy type
            if (!string.IsNullOrEmpty(enemyType) && _enemyScenes.TryGetValue(enemyType, out var typeScene))
            {
                scene = typeScene;
            }
            // Fall back to default
            else if (_defaultEnemyScene != null)
            {
                scene = _defaultEnemyScene;
            }
            
            if (scene == null)
            {
                GD.PrintErr("[EnemySpawnManager] No enemy scene available!");
                return null;
            }
            
            // Instantiate enemy
            var enemy = scene.Instantiate();
            if (enemy == null)
            {
                GD.PrintErr("[EnemySpawnManager] Failed to instantiate enemy!");
                return null;
            }
            
            // Set position
            var spawnPos = position ?? GetRandomSpawnPosition();
            if (enemy is Node2D node2D)
            {
                node2D.GlobalPosition = spawnPos;
            }
            
            // Add to container
            _enemyContainer.AddChild(enemy);
            _activeEnemies.Add(enemy);
            _enemiesById[enemy.GetInstanceId()] = enemy;
            
            // Track wave spawn
            _enemiesSpawnedInWave++;
            
            GD.Print("[EnemySpawnManager] Enemy spawned at " + spawnPos);
            OnEnemySpawned?.Invoke(enemy);
            
            return enemy;
        }
        
        /// <summary>
        /// 批量生成敌人
        /// </summary>
        public List<Node> SpawnEnemies(int count, string enemyType = "")
        {
            var enemies = new List<Node>();
            for (int i = 0; i < count; i++)
            {
                var enemy = SpawnEnemy(enemyType);
                if (enemy != null)
                {
                    enemies.Add(enemy);
                }
            }
            return enemies;
        }
        
        /// <summary>
        /// 移除敌人
        /// </summary>
        public void RemoveEnemy(Node enemy)
        {
            if (enemy == null) return;
            
            _activeEnemies.Remove(enemy);
            _enemiesById.Remove(enemy.GetInstanceId());
            
            if (enemy.GetParent() != null)
            {
                enemy.QueueFree();
            }
            
            _enemiesKilledInWave++;
            OnEnemyKilled?.Invoke(enemy);
            
            // Check wave completion
            if (_waveInProgress && _enemiesKilledInWave >= _enemiesInWave)
            {
                CompleteWave();
            }
        }
        
        /// <summary>
        /// 获取随机生成位置
        /// </summary>
        public Vector2 GetRandomSpawnPosition()
        {
            var random = new Random();
            var angle = (float)(random.NextDouble() * Math.PI * 2);
            var distance = (float)(random.NextDouble() * _spawnRadius);
            
            return _spawnCenter + new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );
        }
        
        /// <summary>
        /// 在指定区域生成敌人
        /// </summary>
        public Node SpawnEnemyInArea(string enemyType, Vector2 center, float radius)
        {
            var random = new Random();
            var angle = (float)(random.NextDouble() * Math.PI * 2);
            var distance = (float)(random.NextDouble() * radius);
            
            var position = center + new Vector2(
                Mathf.Cos(angle) * distance,
                Mathf.Sin(angle) * distance
            );
            
            return SpawnEnemy(enemyType, position);
        }
        
        /// <summary>
        /// 开始新一波敌人
        /// </summary>
        public void StartWave(int waveNumber)
        {
            _currentWave = waveNumber;
            _enemiesSpawnedInWave = 0;
            _enemiesKilledInWave = 0;
            _waveInProgress = true;
            
            // Scale difficulty with wave
            _enemiesInWave = 10 + (waveNumber * 2);
            
            GD.Print($"[EnemySpawnManager] Wave {waveNumber} started with {_enemiesInWave} enemies");
            OnWaveStart?.Invoke(waveNumber);
        }
        
        /// <summary>
        /// 完成当前波次
        /// </summary>
        private void CompleteWave()
        {
            _waveInProgress = false;
            _waveTimer = _timeBetweenWaves;
            
            GD.Print($"[EnemySpawnManager] Wave {_currentWave} completed!");
            OnWaveComplete?.Invoke(_currentWave);
        }
        
        /// <summary>
        /// 设置刷怪范围
        /// </summary>
        public void SetSpawnRadius(float radius)
        {
            _spawnRadius = Math.Max(100f, radius);
        }
        
        /// <summary>
        /// 设置刷怪中心点
        /// </summary>
        public void SetSpawnCenter(Vector2 center)
        {
            _spawnCenter = center;
        }
        
        /// <summary>
        /// 设置最大敌人数量
        /// </summary>
        public void SetMaxEnemies(int max)
        {
            _maxEnemies = Math.Max(0, max);
        }
        
        /// <summary>
        /// 启用/禁用自动刷怪
        /// </summary>
        public void SetAutoSpawn(bool enabled)
        {
            _autoSpawn = enabled;
        }
        
        /// <summary>
        /// 获取活跃敌人列表
        /// </summary>
        public List<Node> GetActiveEnemies()
        {
            return new List<Node>(_activeEnemies);
        }
        
        /// <summary>
        /// 获取敌人数量
        /// </summary>
        public int GetEnemyCount()
        {
            return _activeEnemies.Count;
        }
        
        /// <summary>
        /// 获取当前波次
        /// </summary>
        public int GetCurrentWave()
        {
            return _currentWave;
        }
        
        /// <summary>
        /// 清除所有敌人
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in _activeEnemies.ToArray())
            {
                if (enemy.GetParent() != null)
                {
                    enemy.QueueFree();
                }
            }
            _activeEnemies.Clear();
            _enemiesById.Clear();
        }
        
        // Getters
        public Node2D GetEnemyContainer() => _enemyContainer;
        public int GetMaxEnemies() => _maxEnemies;
        public float GetSpawnRadius() => _spawnRadius;
        public Vector2 GetSpawnCenter() => _spawnCenter;
        public bool IsAutoSpawnEnabled() => _autoSpawn;
        public bool IsWaveInProgress() => _waveInProgress;
        public int GetEnemiesInWave() => _enemiesInWave;
        public int GetEnemiesSpawnedInWave() => _enemiesSpawnedInWave;
        public int GetEnemiesKilledInWave() => _enemiesKilledInWave;
        
        /// <summary>
        /// 导出保存数据
        /// </summary>
        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                { "maxEnemies", _maxEnemies },
                { "spawnRadius", _spawnRadius },
                { "spawnCenterX", _spawnCenter.X },
                { "spawnCenterY", _spawnCenter.Y },
                { "autoSpawn", _autoSpawn },
                { "currentWave", _currentWave },
                { "enemiesInWave", _enemiesInWave },
                { "enemiesKilledInWave", _enemiesKilledInWave },
                { "waveInProgress", _waveInProgress },
                { "timeBetweenWaves", _timeBetweenWaves }
            };
        }
        
        /// <summary>
        /// 导入保存数据
        /// </summary>
        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
            
            if (data.Contains("maxEnemies")) _maxEnemies = Convert.ToInt32(data["maxEnemies"]);
            if (data.Contains("spawnRadius")) _spawnRadius = Convert.ToSingle(data["spawnRadius"]);
            if (data.Contains("spawnCenterX")) _spawnCenter.x = Convert.ToSingle(data["spawnCenterX"]);
            if (data.Contains("spawnCenterY")) _spawnCenter.y = Convert.ToSingle(data["spawnCenterY"]);
            if (data.Contains("autoSpawn")) _autoSpawn = (bool)data["autoSpawn"];
            if (data.Contains("currentWave")) _currentWave = Convert.ToInt32(data["currentWave"]);
            if (data.Contains("enemiesInWave")) _enemiesInWave = Convert.ToInt32(data["enemiesInWave"]);
            if (data.Contains("enemiesKilledInWave")) _enemiesKilledInWave = Convert.ToInt32(data["enemiesKilledInWave"]);
            if (data.Contains("waveInProgress")) _waveInProgress = (bool)data["waveInProgress"];
            if (data.Contains("timeBetweenWaves")) _timeBetweenWaves = Convert.ToSingle(data["timeBetweenWaves"]);
        }
    }
}
