using Godot;
using System;
using System.Collections.Generic;
using Project;
using ClawRPG.Scripts.Data.Enemy;
using ClawRPG.Scripts.Database.Loaders;

namespace ClawRPG.Scripts.Database
{
    /// <summary>
    /// 敌人数据提供类 - 负责敌人数据的查询和检索
    /// 继承 BaseSystem 以获得生命周期管理支持
    /// </summary>
    public class EnemyDataProvider : BaseSystem
    {
        private Dictionary<string, EnemyType> _enemyTypes = new();
        
        public override void _Ready()
        {
            base._Ready();
            Initialize();
        }
        
        protected override void Initialize()
        {
            base.Initialize();
            LoadEnemyData();
            GD.Print($"[EnemyDataProvider] Loaded {_enemyTypes.Count} enemy types");
        }
        
        /// <summary>
        /// 加载敌人数据
        /// </summary>
        private void LoadEnemyData()
        {
            // 加载配置文件
            string configPath = "res://Resources/Config/enemies_config.json";
            var loader = EnemyConfigLoader.Instance;
            
            if (!loader.Load(configPath))
            {
                GD.PrintErr($"[EnemyDataProvider] Failed to load enemy config: {loader.LastError}");
                return;
            }
            
            // 转换为EnemyType并存储
            var allEnemyTypes = loader.GetAllEnemyTypes();
            foreach (var enemyType in allEnemyTypes)
            {
                _enemyTypes[enemyType.Id] = enemyType;
            }
        }
        
        /// <summary>
        /// 获取敌人类型 (Flyweight Factory)
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
        
        /// <summary>
        /// 获取敌人总数
        /// </summary>
        public int GetEnemyCount()
        {
            return _enemyTypes.Count;
        }
        
        /// <summary>
        /// 根据ID获取敌人数据 (兼容旧API)
        /// </summary>
        public EnemyType GetEnemyById(string id)
        {
            return GetEnemyType(id);
        }
        
        /// <summary>
        /// 获取所有敌人数据 (兼容旧API)
        /// </summary>
        public List<EnemyType> GetEnemyData()
        {
            return GetEnemyTypeList();
        }
        
        /// <summary>
        /// 根据区域获取敌人列表
        /// </summary>
        public List<EnemyType> GetEnemiesByRegion(string region)
        {
            var result = new List<EnemyType>();
            foreach (var enemy in _enemyTypes.Values)
            {
                if (IsEnemyInRegion(enemy.Id, region))
                {
                    result.Add(enemy);
                }
            }
            return result;
        }
        
        /// <summary>
        /// 检查敌人是否在指定区域
        /// </summary>
        private bool IsEnemyInRegion(string enemyId, string region)
        {
            string[] regionKeywords = GetRegionKeywords(region);
            foreach (var keyword in regionKeywords)
            {
                if (enemyId.Contains(keyword, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }
            return false;
        }
        
        /// <summary>
        /// 获取区域关键词
        /// </summary>
        private string[] GetRegionKeywords(string region)
        {
            return region.ToLower() switch
            {
                "forest" => new[] { "goblin", "wolf", "slime", "spider", "bear", "deer", "mushroom", "treant" },
                "cave" => new[] { "bat", "skeleton", "cave", "golem", "centipede", "troll" },
                "fire" => new[] { "fire", "magma", "lava", "imp", "phoenix" },
                "ice" => new[] { "ice", "frost", "yeti" },
                "shadow" => new[] { "shadow", "dark", "wraith", "vampire", "banshee" },
                "holy" => new[] { "holy", "divine", "angel", "guardian" },
                "swamp" => new[] { "swamp", "zombie", "crocodile", "mosquito", "witch" },
                "abyss" => new[] { "void", "abyss", "elder" },
                "dragon" => new[] { "dragon", "drake" },
                _ => new[] { region.ToLower() }
            };
        }
        
        /// <summary>
        /// 根据玩家等级获取合适的敌人
        /// </summary>
        public List<EnemyType> GetEnemiesForLevel(int playerLevel)
        {
            var result = new List<EnemyType>();
            foreach (var enemy in _enemyTypes.Values)
            {
                if (enemy.MaxHealth <= playerLevel * 50 + 100)
                {
                    result.Add(enemy);
                }
            }
            return result;
        }

        #region BaseSystem Persistence

        public override Dictionary ExportSaveData()
        {
            return new Dictionary
            {
                { "enemyCount", _enemyTypes.Count }
            };
        }

        public override void ImportSaveData(Dictionary data)
        {
            if (data == null) return;
        }

        #endregion
    }
}
