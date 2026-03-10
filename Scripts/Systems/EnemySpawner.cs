using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems {
    /// <summary>
    /// Manages enemy spawning using the EnemyDatabase
    /// </summary>
    public partial class EnemySpawner : Node
    {
        [Export] public bool AutoSpawn = true;
        [Export] public int MaxEnemies = 10;
        [Export] public float SpawnRadius = 500f;
        [Export] public float DespawnRadius = 800f;
        
        // Spawn settings
        [Export] public bool SpawnOnReady = true;
        [Export] public int InitialSpawnCount = 5;
        [Export] public float SpawnInterval = 5.0f;
        
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
        
        public override void _Ready()
        {
            // Find player
            _player = GetTree().GetFirstNodeInGroup("player") as Player;
            
            if (SpawnOnReady && AutoSpawn)
            {
                // Initial spawn
                for (int i = 0; i < InitialSpawnCount; i++)
                {
                    SpawnRandomEnemy();
                }
            }
            
            GD.Print("[EnemySpawner] Ready - Max Enemies: " + MaxEnemies);
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
            
            // Create enemy instance
            var enemyScene = GD.Load<PackedScene>("res://Enemies/Enemy.tscn");
            if (enemyScene == null)
            {
                // Fallback: create enemy programmatically
                return CreateEnemyProgrammatically(enemyType, spawnPos);
            }
            
            var enemy = enemyScene.Instantiate<Node2D>();
            if (enemy != null)
            {
                enemy.GlobalPosition = spawnPos;
                
                // Configure enemy from database
                var enemyScript = enemy as Characters.Enemy;
                if (enemyScript != null)
                {
                    ConfigureEnemyFromDatabase(enemyScript, enemyType);
                }
                
                GetParent().AddChild(enemy);
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
                enemy.QueueFree();
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
    }
}
