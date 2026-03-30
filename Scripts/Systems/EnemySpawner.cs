using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Characters;
using ClawRPG.Scripts.Items;
using ClawRPG.Scripts.Database;
using ClawRPG.Scripts.Data.Enemy;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Manages enemy spawning using the EnemyDatabase
    /// Supports Quick Mode with reduced enemy count and strength
    /// </summary>
    public partial class EnemySpawner : BaseSystem
    {
        [Export] public bool AutoSpawn = true;
        [Export] public int MaxEnemies = 10;
        [Export] public float SpawnRadius = 500f;
        [Export] public float DespawnRadius = 800f;
        
        // Spawn settings
        [Export] public bool SpawnOnReady = true;
        [Export] public int InitialSpawnCount = 5;
        [Export] public float SpawnInterval = 5.0f;
        
        // Object pool for performance optimization (2D game optimization: reduce instantiation)
        [Export] public bool UseObjectPool = true;
        private ObjectPool _enemyPool;
        
        // Current enemies
        private List<Node2D> _activeEnemies = new();
        private float _spawnTimer;
        private Player _player;
        
        // Wave system
        [Export] public bool UseWaves = false; 
        [Export] public int EnemiesPerWave = 10;
        [Export] public float WaveDelay = 30f;
        private int _currentWave = 1;
        private float _waveTimer;
        
        // Quick mode support
        private bool _quickModeEnabled = false;
        
        public override void _Ready()
        {
            // Check for quick mode
            if (GameModeManager.Instance != null)
            {
                _quickModeEnabled = GameModeManager.Instance.IsQuickMode();
            }
            
            // Adjust spawn settings for quick mode
            if (_quickModeEnabled)
            {
                AdjustForQuickMode();
            }
            
            // Find player
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            
            // Initialize object pool if enabled
            if (UseObjectPool)
            {
                InitializeObjectPool();
            }
            
            if (SpawnOnReady && AutoSpawn)
            {
                // Initial spawn
                for (int i = 0; i < InitialSpawnCount; i++)
                {
                    SpawnRandomEnemy();
                }
            }
            
            GD.Print("[EnemySpawner] Ready - Max Enemies: " + MaxEnemies);
            if (_quickModeEnabled)
            {
                GD.Print("[EnemySpawner] Quick Mode active - enemies reduced");
            }
        }
        
        /// <summary>
        /// Adjust spawn settings for quick mode
        /// </summary>
        private void AdjustForQuickMode()
        {
            var config = GameModeManager.Instance;
            
            // Reduce max enemies
            MaxEnemies = config.GetMaxEnemies(MaxEnemies);
            MaxEnemies = Math.Max(MaxEnemies, 3); // Minimum 3 enemies
            
            // Reduce initial spawn count
            InitialSpawnCount = (int)(InitialSpawnCount * config.GetEnemyCountMultiplier());
            InitialSpawnCount = Math.Max(InitialSpawnCount, 2);
            
            // Reduce spawn interval for faster pacing
            SpawnInterval = Math.Max(SpawnInterval * 0.7f, 2.0f);
            
            // Reduce wave size
            if (UseWaves)
            {
                EnemiesPerWave = (int)(EnemiesPerWave * config.GetEnemyCountMultiplier());
            }
            
            GD.Print($"[EnemySpawner] Quick Mode adjustments: MaxEnemies={MaxEnemies}, InitialSpawn={InitialSpawnCount}, Interval={SpawnInterval}");
        }
        
        /// <summary>
        /// Initialize the enemy object pool
        /// </summary>
        private void InitializeObjectPool()
        {
            var enemyScene = GD.Load<PackedScene>("res://Enemies/Enemy.tscn");
            if (enemyScene == null)
            {
                GD.Warning("[EnemySpawner] No Enemy.tscn found, object pool disabled");
                UseObjectPool = false; 
                return;
            }
            
            // Create a dedicated object pool node
            var poolNode = new ObjectPool
            {
                Name = "EnemyObjectPool",
                PooledScene = enemyScene,
                InitialPoolSize = Mathf.Min(MaxEnemies, 10),
                MaxPoolSize = Mathf.Min(MaxEnemies * 2, 50),
                AutoExpand = true
            };
            GetTree().CurrentScene.AddChild(poolNode);
            _enemyPool = poolNode;
            
            GD.Print("[EnemySpawner] Object pool initialized");
        }
        
        public override void _PhysicsProcess(double delta)
        {
            float dt = (float)delta;
            
            if (_player == null)
            {
                _player = GetTree().GetFirstNodeInGroup("player") as Player;
                return;
            }
            
            // Clean up dead enemies
            CleanupDeadEnemies();
            
            // Spawn new enemies if needed
            if (AutoSpawn && _activeEnemies.Count < MaxEnemies)
            {
                _spawnTimer += dt;
                if (_spawnTimer >= SpawnInterval)
                {
                    _spawnTimer = 0;
                    SpawnRandomEnemy();
                }
            }
            
            // Wave system
            if (UseWaves)
            {
                _waveTimer += dt;
                if (_waveTimer >= WaveDelay)
                {
                    _waveTimer = 0;
                    StartNextWave();
                }
            }
            
            // Despawn far enemies
            DespawnDistantEnemies();
        }
        
        /// <summary>
        /// Spawn a random enemy from the database
        /// </summary>
        public Node2D SpawnRandomEnemy(Vector2? position = null, string enemyId = null)
        {
            if (_player == null) return null;
            
            // Get random enemy type if not specified
            string targetId = enemyId;
            if (string.IsNullOrEmpty(targetId))
            {
                var enemies = EnemyDatabase.Instance.GetAllEnemies();
                if (enemies.Count == 0) return null;
                
                var random = new Random();
                targetId = enemies[random.Next(enemies.Count)].Id;
            }
            
            return SpawnEnemy(targetId, position);
        }
        
        /// <summary>
        /// Spawn a specific enemy type
        /// </summary>
        public Node2D SpawnEnemy(string enemyId, Vector2? position = null)
        {
            var enemyType = EnemyDatabase.Instance.GetEnemy(enemyId);
            if (enemyType == null)
            {
                GD.Warning($"[EnemySpawner] Unknown enemy type: {enemyId}");
                return null;
            }
            
            // Determine spawn position
            Vector2 spawnPos;
            if (position.HasValue)
            {
                spawnPos = position.Value;
            }
            else
            {
                // Random position around player
                var random = new Random();
                float angle = (float)(random.NextDouble() * Math.PI * 2);
                float distance = 200f + (float)(random.NextDouble() * SpawnRadius);
                spawnPos = _player.GlobalPosition + new Vector2(
                    (float)Math.Cos(angle) * distance,
                    (float)Math.Sin(angle) * distance
                );
            }
            
            // Create enemy instance - use object pool if enabled
            Node2D enemy;
            
            if (UseObjectPool && _enemyPool != null)
            {
                // Get from object pool (performance optimization)
                enemy = _enemyPool.GetObject();
                if (enemy == null)
                {
                    // Pool exhausted, create new
                    var enemyScene = GD.Load<PackedScene>("res://Enemies/Enemy.tscn");
                    enemy = enemyScene?.Instantiate<Node2D>();
                }
            }
            else
            {
                // Traditional instantiation
                var enemyScene = GD.Load<PackedScene>("res://Enemies/Enemy.tscn");
                enemy = enemyScene?.Instantiate<Node2D>();
            }
            
            if (enemy != null)
            {
                enemy.GlobalPosition = spawnPos;
                
                // Configure enemy from database
                var enemyScript = enemy as Characters.Enemy;
                if (enemyScript != null)
                {
                    ConfigureEnemyFromDatabase(enemyScript, enemyType);
                }
                
                // Add to scene
                if (!enemy.IsInsideTree())
                {
                    GetParent().AddChild(enemy);
                }
                
                _activeEnemies.Add(enemy);
                
                GD.Print($"[EnemySpawner] Spawned {enemyType.Name} at {spawnPos}");
            }
            
            return enemy;
        }
        
        /// <summary>
        /// Create enemy programmatically if no scene is available
        /// </summary>
        private Node2D CreateEnemyProgrammatically(EnemyType enemyType, Vector2 position)
        {
            // This would create the enemy using code
            // For now, just log it
            GD.Print($"[EnemySpawner] Would create {enemyType.Name} programmatically at {position}");
            return null;
        }
        
        /// <summary>
        /// Configure enemy script from database type
        /// </summary>
        private void ConfigureEnemyFromDatabase(Characters.Enemy enemy, EnemyType enemyType)
        {
            // Use reflection or direct property access to set enemy properties
            // Since the properties are [Export], we can set them directly
            
            // Set the enemy type ID for drop table lookup
            enemy.EnemyTypeId = enemyType.Id;
            
            // Set other properties from database
            enemy.ExperienceReward = enemyType.ExperienceReward;
            
            // Note: In Godot 4.x with C#, we'd use the properties directly
            // This is a simplified approach
            
            // The enemy will read from its configured values
            // Additional configuration could be added here
        }
        
        /// <summary>
        /// Clean up dead enemies from the list
        /// </summary>
        private void CleanupDeadEnemies()
        {
            _activeEnemies.RemoveAll(e => e == null || !IsInstanceValid(e));
        }
        
        /// <summary>
        /// Despawn enemies that are too far from the player
        /// </summary>
        private void DespawnDistantEnemies()
        {
            if (_player == null) return;
            
            var toRemove = new List<Node2D>();
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null && IsInstanceValid(enemy))
                {
                    float distance = enemy.GlobalPosition.DistanceTo(_player.GlobalPosition);
                    if (distance > DespawnRadius)
                    {
                        toRemove.Add(enemy);
                    }
                }
            }
            
            foreach (var enemy in toRemove)
            {
                // Return to object pool instead of destroying (performance optimization)
                if (UseObjectPool && _enemyPool != null)
                {
                    _enemyPool.ReturnObject(enemy);
                }
                else
                {
                    enemy.QueueFree();
                }
                _activeEnemies.Remove(enemy);
            }
        }
        
        /// <summary>
        /// Start the next wave of enemies
        /// </summary>
        private void StartNextWave()
        {
            _currentWave++;
            int enemyCount = EnemiesPerWave + (_currentWave * 2);
            
            GD.Print($"[EnemySpawner] Starting Wave {_currentWave} with {enemyCount} enemies");
            
            for (int i = 0; i < enemyCount; i++)
            {
                SpawnRandomEnemy();
            }
            
            // Notify player
            var messageSystem = GetTree().GetFirstNodeInGroup("messageSystem");
            if (messageSystem != null)
            {
                // Could trigger a wave notification
            }
        }
        
        /// <summary>
        /// Get count of active enemies
        /// </summary>
        public int GetActiveEnemyCount()
        {
            CleanupDeadEnemies();
            return _activeEnemies.Count;
        }
        
        /// <summary>
        /// Force spawn a wave
        /// </summary>
        public void TriggerWave()
        {
            StartNextWave();
        }
        
        /// <summary>
        /// Clear all active enemies
        /// </summary>
        public void ClearAllEnemies()
        {
            foreach (var enemy in _activeEnemies)
            {
                if (enemy != null && IsInstanceValid(enemy))
                {
                    enemy.QueueFree();
                }
            }
            _activeEnemies.Clear();
            GD.Print("[EnemySpawner] Cleared all enemies");
        }
        
        // === 数据持久化接口 ===
        
        public override Dictionary<string, object> ExportSaveData()
        {
            var data = new Dictionary<string, object>
            {
                ["current_wave"] = _currentWave,
                ["wave_timer"] = _waveTimer
            };
            return new Dictionary(data);
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("current_wave"))
                _currentWave = Convert.ToInt32(data["current_wave"]);
            if (data.ContainsKey("wave_timer"))
                _waveTimer = Convert.ToSingle(data["wave_timer"]);
        }
    }
}
