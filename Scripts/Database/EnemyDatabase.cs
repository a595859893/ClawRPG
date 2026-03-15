using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Database {
    /// <summary>
    /// Enemy type definition for data-driven enemy spawning
    /// </summary>
    [Serializable]
    public class EnemyType {
        public string Id;
        public string Name;
        public string Description;
        
        // Combat stats
        public int MaxHealth;
        public float MoveSpeed;
        public float AttackDamage;
        public float AttackRange;
        public float AttackCooldown;
        public float ChaseRange;
        public float DetectionRange;
        
        // Combat stats
        public float CriticalChance = 0.05f;
        public float CriticalDamage = 1.5f;
        
        // Rewards
        public int ExperienceReward;
        public int GoldReward;
        
        // Visual
        public string SpritePath;
        public Color SpriteModulate = Colors.White;
        
        // Loot table (itemId -> dropChance)
        public Dictionary<string, float> DropTable = new();
        
        // AI behavior
        public bool CanChase = true;
        public bool CanAttack = true;
        public bool IsAggressive = true;
        
        // Status effect vulnerability
        public Dictionary<string, float> StatusEffectVulnerability = new();
        
        public EnemyType() {
            Id = "";
            Name = "Unknown";
            Description = "";
        }
        
        public EnemyType(string id, string name, int hp, float speed, float damage) {
            Id = id;
            Name = name;
            MaxHealth = hp;
            MoveSpeed = speed;
            AttackDamage = damage;
        }
    }
    
    /// <summary>
    /// Database of all enemy types in the game
    /// 敌人数据库主控制器 - 委托给专用类处理具体逻辑
    /// </summary>
    public class EnemyDatabase {
        private static EnemyDatabase _instance;
        private EnemyDataProvider _dataProvider;
        private EnemySpawnManager _spawnManager;
        private EnemyDifficultyScaler _difficultyScaler;
        
        public static EnemyDatabase Instance {
            get {
                if (_instance == null) {
                    _instance = new EnemyDatabase();
                    _instance.LoadEnemies();
                }
                return _instance;
            }
        }
        
        /// <summary>
        /// 初始化数据库和子系统
        /// </summary>
        public void LoadEnemies() {
            _dataProvider = new EnemyDataProvider();
            _dataProvider.Initialize();
            
            _spawnManager = new EnemySpawnManager();
            _spawnManager.Initialize();
            
            _difficultyScaler = new EnemyDifficultyScaler();
            _difficultyScaler.Initialize();
            
            GD.Print("[EnemyDatabase] Loaded enemy subsystems");
        }
        
        /// <summary>
        /// 获取敌人数据提供者
        /// </summary>
        public EnemyDataProvider GetDataProvider() {
            return _dataProvider;
        }
        
        /// <summary>
        /// 获取生成管理器
        /// </summary>
        public EnemySpawnManager GetSpawnManager() {
            return _spawnManager;
        }
        
        /// <summary>
        /// 获取难度缩放器
        /// </summary>
        public EnemyDifficultyScaler GetDifficultyScaler() {
            return _difficultyScaler;
        }
        
        /// <summary>
        /// 获取敌人 (兼容旧API)
        /// </summary>
        public EnemyType GetEnemy(string id) {
            return _dataProvider?.GetEnemyById(id);
        }
        
        /// <summary>
        /// 获取所有敌人 (兼容旧API)
        /// </summary>
        public List<EnemyType> GetAllEnemies() {
            return _dataProvider?.GetEnemyData() ?? new List<EnemyType>();
        }
        
        /// <summary>
        /// 根据区域获取敌人 (兼容旧API)
        /// </summary>
        public List<EnemyType> GetEnemiesByRegion(string region) {
            return _dataProvider?.GetEnemiesByRegion(region) ?? new List<EnemyType>();
        }
        
        /// <summary>
        /// 根据玩家等级获取敌人 (兼容旧API)
        /// </summary>
        public List<EnemyType> GetEnemiesForLevel(int playerLevel) {
            return _dataProvider?.GetEnemiesForLevel(playerLevel) ?? new List<EnemyType>();
        }
        
        /// <summary>
        /// 检查敌人是否存在 (兼容旧API)
        /// </summary>
        public bool HasEnemy(string id) {
            return _dataProvider?.HasEnemyType(id) ?? false;
        }
        
        /// <summary>
        /// 获取敌人数量 (兼容旧API)
        /// </summary>
        public int GetEnemyCount() {
            return _dataProvider?.GetEnemyCount() ?? 0;
        }
        
        /// <summary>
        /// 获取敌人类型 (兼容旧API - Flyweight Factory)
        /// </summary>
        public EnemyType GetEnemyType(string id) {
            return _dataProvider?.GetEnemyType(id);
        }
        
        /// <summary>
        /// 生成敌人 (委托给SpawnManager)
        /// </summary>
        public Node SpawnEnemy(string enemyId, Vector2 position, string zoneId = "default") {
            return _spawnManager?.SpawnEnemy(enemyId, position, zoneId);
        }
        
        /// <summary>
        /// 缩放敌人难度 (委托给DifficultyScaler)
        /// </summary>
        public EnemyType ScaleDifficulty(EnemyType baseEnemy) {
            return _difficultyScaler?.ScaleDifficulty(baseEnemy);
        }
        
        /// <summary>
        /// 获取缩放后的属性 (委托给DifficultyScaler)
        /// </summary>
        public (int health, float damage, float speed) GetScaledStats(int baseHealth, float baseDamage, float baseSpeed) {
            return _difficultyScaler?.GetScaledStats(baseHealth, baseDamage, baseSpeed) ?? (baseHealth, baseDamage, baseSpeed);
        }
        
        /// <summary>
        /// 计算等级乘数 (委托给DifficultyScaler)
        /// </summary>
        public float CalculateLevelMultiplier(int enemyLevel, int playerLevel) {
            return _difficultyScaler?.CalculateLevelMultiplier(enemyLevel, playerLevel) ?? 1.0f;
        }
    }
}
