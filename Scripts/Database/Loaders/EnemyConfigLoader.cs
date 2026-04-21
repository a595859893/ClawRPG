using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using Godot;
using ClawRPG.Scripts.Data.Enemy;

namespace ClawRPG.Scripts.Database.Loaders
{
    /// <summary>
    /// 敌人配置数据（JSON反序列化用）
    /// </summary>
    public class EnemyConfigData
    {
        [JsonPropertyName("id")]
        public string Id { get; set; }
        
        [JsonPropertyName("name")]
        public string Name { get; set; }
        
        [JsonPropertyName("maxHealth")]
        public int MaxHealth { get; set; }
        
        [JsonPropertyName("speed")]
        public float Speed { get; set; }
        
        [JsonPropertyName("damage")]
        public float Damage { get; set; }
        
        [JsonPropertyName("description")]
        public string Description { get; set; }
        
        [JsonPropertyName("attackRange")]
        public float AttackRange { get; set; }
        
        [JsonPropertyName("attackCooldown")]
        public float AttackCooldown { get; set; }
        
        [JsonPropertyName("chaseRange")]
        public float ChaseRange { get; set; }
        
        [JsonPropertyName("detectionRange")]
        public float DetectionRange { get; set; }
        
        [JsonPropertyName("experienceReward")]
        public int ExperienceReward { get; set; }
        
        [JsonPropertyName("goldReward")]
        public int GoldReward { get; set; }
        
        [JsonPropertyName("dropTable")]
        public Dictionary<string, float> DropTable { get; set; }
        
        [JsonPropertyName("statusEffectVulnerability")]
        public Dictionary<string, float> StatusEffectVulnerability { get; set; }
        
        [JsonPropertyName("spriteModulate")]
        public SpriteModulateData SpriteModulate { get; set; }
    }
    
    /// <summary>
    /// 精灵颜色数据（JSON反序列化用）
    /// </summary>
    public class SpriteModulateData
    {
        [JsonPropertyName("r")]
        public float R { get; set; }
        
        [JsonPropertyName("g")]
        public float G { get; set; }
        
        [JsonPropertyName("b")]
        public float B { get; set; }
    }

    /// <summary>
    /// 敌人配置文件结构
    /// </summary>
    public class EnemiesConfigFile
    {
        [JsonPropertyName("version")]
        public string Version { get; set; }
        
        [JsonPropertyName("enemies")]
        public List<EnemyConfigData> Enemies { get; set; }
    }

    /// <summary>
    /// 敌人配置加载器 - 负责从JSON文件加载敌人数据
    /// </summary>
    public class EnemyConfigLoader
    {
        private static EnemyConfigLoader _instance;
        private EnemiesConfigFile _configFile;
        private bool _isLoaded = false;
        private string _lastError = string.Empty;

        /// <summary>
        /// 单例实例
        /// </summary>
        public static EnemyConfigLoader Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EnemyConfigLoader();
                }
                return _instance;
            }
        }

        /// <summary>
        /// 配置是否已加载
        /// </summary>
        public bool IsLoaded => _isLoaded;

        /// <summary>
        /// 最后一次错误信息
        /// </summary>
        public string LastError => _lastError;

        /// <summary>
        /// 加载敌人配置文件
        /// </summary>
        /// <param name="configPath">配置文件路径</param>
        /// <returns>加载是否成功</returns>
        public bool Load(string configPath)
        {
            try
            {
                _lastError = string.Empty;

                if (!System.IO.File.Exists(configPath))
                {
                    _lastError = $"敌人配置文件不存在: {configPath}";
                    GD.PrintErr($"[EnemyConfigLoader] {_lastError}");
                    return false;
                }

                string json = System.IO.File.ReadAllText(configPath);
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                };
                
                _configFile = JsonSerializer.Deserialize<EnemiesConfigFile>(json, options);
                
                if (_configFile == null || _configFile.Enemies == null)
                {
                    _lastError = "敌人配置文件格式错误：无法解析数据";
                    GD.PrintErr($"[EnemyConfigLoader] {_lastError}");
                    return false;
                }

                _isLoaded = true;
                GD.Print($"[EnemyConfigLoader] 成功加载 {_configFile.Enemies.Count} 个敌人配置");
                return true;
            }
            catch (Exception ex)
            {
                _lastError = $"敌人配置加载失败: {ex.Message}";
                GD.PrintErr($"[EnemyConfigLoader] {_lastError}");
                return false;
            }
        }

        /// <summary>
        /// 从配置数据转换为EnemyType
        /// </summary>
        /// <param name="config">敌人配置数据</param>
        /// <returns>EnemyType实例</returns>
        public EnemyType ConvertToEnemyType(EnemyConfigData config)
        {
            if (config == null) return null;

            var enemyType = new EnemyType
            {
                Id = config.Id,
                Name = config.Name,
                MaxHealth = config.MaxHealth,
                Speed = config.Speed,
                Damage = config.Damage,
                Description = config.Description ?? string.Empty,
                AttackRange = config.AttackRange,
                AttackCooldown = config.AttackCooldown,
                ChaseRange = config.ChaseRange,
                DetectionRange = config.DetectionRange,
                ExperienceReward = config.ExperienceReward,
                GoldReward = config.GoldReward,
                DropTable = config.DropTable ?? new Dictionary<string, float>(),
                StatusEffectVulnerability = config.StatusEffectVulnerability ?? new Dictionary<string, float>()
            };

            if (config.SpriteModulate != null)
            {
                enemyType.SpriteModulate = new Color(
                    config.SpriteModulate.R,
                    config.SpriteModulate.G,
                    config.SpriteModulate.B
                );
            }

            return enemyType;
        }

        /// <summary>
        /// 获取所有敌人配置数据
        /// </summary>
        /// <returns>敌人配置列表</returns>
        public List<EnemyConfigData> GetAllEnemyConfigs()
        {
            if (_configFile?.Enemies == null)
            {
                return new List<EnemyConfigData>();
            }
            return new List<EnemyConfigData>(_configFile.Enemies);
        }

        /// <summary>
        /// 获取所有敌人EnemyType列表
        /// </summary>
        /// <returns>EnemyType列表</returns>
        public List<EnemyType> GetAllEnemyTypes()
        {
            var enemyTypes = new List<EnemyType>();
            if (_configFile?.Enemies == null) return enemyTypes;

            foreach (var config in _configFile.Enemies)
            {
                var enemyType = ConvertToEnemyType(config);
                if (enemyType != null)
                {
                    enemyTypes.Add(enemyType);
                }
            }
            return enemyTypes;
        }

        /// <summary>
        /// 根据ID获取敌人配置
        /// </summary>
        /// <param name="id">敌人ID</param>
        /// <returns>敌人配置数据</returns>
        public EnemyConfigData GetEnemyConfigById(string id)
        {
            if (_configFile?.Enemies == null) return null;
            return _configFile.Enemies.Find(e => e.Id == id);
        }

        /// <summary>
        /// 根据ID获取EnemyType
        /// </summary>
        /// <param name="id">敌人ID</param>
        /// <returns>EnemyType实例</returns>
        public EnemyType GetEnemyTypeById(string id)
        {
            var config = GetEnemyConfigById(id);
            return config != null ? ConvertToEnemyType(config) : null;
        }

        /// <summary>
        /// 获取配置版本
        /// </summary>
        public string GetVersion()
        {
            return _configFile?.Version ?? "unknown";
        }

        /// <summary>
        /// 获取敌人总数
        /// </summary>
        public int GetEnemyCount()
        {
            return _configFile?.Enemies?.Count ?? 0;
        }
    }
}
