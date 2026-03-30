using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 敌人变异系统
    /// 管理敌人的随机变异和效果
    /// </summary>
    public partial class EnemyMutationSystem : BaseSystem
    {
        // 单例
        private static EnemyMutationSystem _instance;
        public static EnemyMutationSystem Instance => _instance;

        // 活跃的敌人变异实例
        private Dictionary<int, EnemyMutationData.EnemyMutationInstance> _activeMutations = new();

        // 玩家已发现的变异
        private List<EnemyMutationData.DiscoveredMutation> _discoveredMutations = new();

        // 统计
        private EnemyMutationData.MutationStatistics _statistics = new();

        // 配置
        private bool _mutationEnabled = true;
        private float _mutationChance = 0.3f;  // 30%的敌人会有变异
        private EnemyMutationData.MutationRarity _maxRarity = EnemyMutationData.MutationRarity.Rare;

        // 信号
        public Action<EnemyMutationData.Mutation> OnMutationApplied;
        public Action<EnemyMutationData.Mutation> OnMutationDiscovered;
        public Action<int, float, float> OnExplosion;  // enemyId, damage, radius
        public Action<int, int> OnSplit;  // enemyId, splitCount

        public override void _Ready()
        {
            _instance = this;
            LoadSaveData();
            GD.Print("敌人变异系统已初始化");
        }

        /// <summary>
        /// 为敌人应用随机变异
        /// </summary>
        public EnemyMutationData.Mutation ApplyRandomMutation(int enemyId)
        {
            if (!_mutationEnabled) return null;
            if (_activeMutations.ContainsKey(enemyId)) return null;

            var random = new Random();
            if (random.NextDouble() > _mutationChance) return null;

            var mutation = EnemyMutationDatabase.GetRandomMutation(_maxRarity);
            if (mutation == null) return null;

            var instance = new EnemyMutationData.EnemyMutationInstance
            {
                EnemyId = enemyId,
                Mutation = mutation,
                ActiveShield = 0f,
                TimeSinceLastShield = 0f,
                HasEnraged = false,
                HasSplit = false
            };

            _activeMutations[enemyId] = instance;
            _statistics.TotalMutationsEncountered++;

            OnMutationApplied?.Invoke(mutation);

            GD.Print($"敌人 {enemyId} 获得变异: {mutation.Name} ({mutation.Rarity})");

            return mutation;
        }

        /// <summary>
        /// 为敌人应用特定变异
        /// </summary>
        public EnemyMutationData.Mutation ApplyMutation(int enemyId, EnemyMutationData.MutationType type)
        {
            if (_activeMutations.ContainsKey(enemyId)) return null;

            var mutation = EnemyMutationDatabase.GetMutation(type);
            if (mutation == null) return null;

            var instance = new EnemyMutationData.EnemyMutationInstance
            {
                EnemyId = enemyId,
                Mutation = mutation,
                ActiveShield = 0f,
                TimeSinceLastShield = 0f,
                HasEnraged = false,
                HasSplit = false
            };

            _activeMutations[enemyId] = instance;
            _statistics.TotalMutationsEncountered++;

            OnMutationApplied?.Invoke(mutation);

            return mutation;
        }

        /// <summary>
        /// 获取敌人的变异
        /// </summary>
        public EnemyMutationData.Mutation GetMutation(int enemyId)
        {
            if (_activeMutations.TryGetValue(enemyId, out var instance))
            {
                return instance.Mutation;
            }
            return null;
        }

        /// <summary>
        /// 处理变异敌人的更新
        /// </summary>
        public override void _Process(double delta)
        {
            foreach (var instance in _activeMutations.Values)
            {
                // 再生效果
                if (instance.Mutation.RegenPerSecond > 0)
                {
                    // 通知生命恢复系统
                }

                // 护盾周期性生成
                if (instance.Mutation.ShieldPerSecond > 0)
                {
                    instance.TimeSinceLastShield += (float)delta;
                    if (instance.TimeSinceLastShield >= 3f)  // 每3秒生成护盾
                    {
                        instance.ActiveShield = Mathf.Min(
                            instance.ActiveShield + instance.Mutation.ShieldPerSecond * 3f,
                            instance.Mutation.ShieldMax
                        );
                        instance.TimeSinceLastShield = 0f;
                    }
                }
            }
        }

        /// <summary>
        /// 检查敌人是否愤怒
        /// </summary>
        public bool CheckEnrage(int enemyId, float currentHealthPercent)
        {
            if (!_activeMutations.TryGetValue(enemyId, out var instance)) return false;
            if (instance.Mutation.Type != EnemyMutationData.MutationType.Enraged) return false;
            if (instance.HasEnraged) return false;

            if (currentHealthPercent <= instance.Mutation.EnrageThreshold)
            {
                instance.HasEnraged = true;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 获取愤怒加成
        /// </summary>
        public float GetEnrageMultiplier(int enemyId)
        {
            if (!_activeMutations.TryGetValue(enemyId, out var instance)) return 1f;
            if (instance.Mutation.Type != EnemyMutationData.MutationType.Enraged) return 1f;
            return instance.HasEnraged ? instance.Mutation.EnrageMultiplier : 1f;
        }

        /// <summary>
        /// 处理敌人死亡
        /// </summary>
        public void OnEnemyDeath(int enemyId, Vector2 position)
        {
            if (!_activeMutations.TryGetValue(enemyId, out var instance)) return;

            // 记录击杀统计
            _statistics.TotalMutationsKilled++;
            if (_statistics.MutationKillCount.ContainsKey(instance.Mutation.Type))
            {
                _statistics.MutationKillCount[instance.Mutation.Type]++;
            }
            else
            {
                _statistics.MutationKillCount[instance.Mutation.Type] = 1;
            }

            if (_statistics.RarityKillCount.ContainsKey(instance.Mutation.Rarity))
            {
                _statistics.RarityKillCount[instance.Mutation.Rarity]++;
            }
            else
            {
                _statistics.RarityKillCount[instance.Mutation.Rarity] = 1;
            }

            // 发现变异
            DiscoverMutation(instance.Mutation.Type);

            // 爆炸效果
            if (instance.Mutation.ExplosionDamage > 0)
            {
                OnExplosion?.Invoke(enemyId, instance.Mutation.ExplosionDamage, instance.Mutation.ExplosionRadius);
                GD.Print($"敌人爆炸: 伤害 {instance.Mutation.ExplosionDamage}, 半径 {instance.Mutation.ExplosionRadius}");
            }

            // 分裂效果
            if (instance.Mutation.SplitCount > 0 && !instance.HasSplit)
            {
                instance.HasSplit = true;
                OnSplit?.Invoke(enemyId, instance.Mutation.SplitCount);
                GD.Print($"敌人分裂: 生成 {instance.Mutation.SplitCount} 个小怪");
            }

            // 移除变异实例
            _activeMutations.Remove(enemyId);
        }

        /// <summary>
        /// 发现变异
        /// </summary>
        private void DiscoverMutation(EnemyMutationData.MutationType type)
        {
            var existing = _discoveredMutations.Find(d => d.Type == type);
            if (existing != null)
            {
                existing.KillCount++;
                if (!existing.IsDiscovered)
                {
                    existing.IsDiscovered = true;
                    OnMutationDiscovered?.Invoke(existing.Mutation);
                }
            }
            else
            {
                var discovered = new EnemyMutationData.DiscoveredMutation
                {
                    Type = type,
                    KillCount = 1,
                    IsDiscovered = true
                };
                _discoveredMutations.Add(discovered);
                OnMutationDiscovered?.Invoke(EnemyMutationDatabase.GetMutation(type));
            }
        }

        /// <summary>
        /// 获取变异的属性修正
        /// </summary>
        public (float health, float attack, float defense, float speed) GetAttributeMultipliers(int enemyId)
        {
            if (!_activeMutations.TryGetValue(enemyId, out var instance))
            {
                return (1f, 1f, 1f, 1f);
            }

            var mutation = instance.Mutation;
            var health = mutation.HealthMultiplier;
            var attack = mutation.AttackMultiplier;
            var defense = mutation.DefenseMultiplier;
            var speed = mutation.SpeedMultiplier;

            // 愤怒状态加成
            if (mutation.Type == EnemyMutationData.MutationType.Enraged && instance.HasEnraged)
            {
                attack *= mutation.EnrageMultiplier;
                speed *= 1.2f;
            }

            return (health, attack, defense, speed);
        }

        /// <summary>
        /// 获取护盾值
        /// </summary>
        public float GetShield(int enemyId)
        {
            if (_activeMutations.TryGetValue(enemyId, out var instance))
            {
                return instance.ActiveShield;
            }
            return 0f;
        }

        /// <summary>
        /// 受到伤害时触发反射
        /// </summary>
        public float HandleDamage(int enemyId, float damage)
        {
            if (!_activeMutations.TryGetValue(enemyId, out var instance)) return 0f;
            if (instance.Mutation.DamageReflectPercent <= 0) return 0f;

            var reflectDamage = damage * instance.Mutation.DamageReflectPercent;
            return reflectDamage;
        }

        /// <summary>
        /// 获取吸血百分比
        /// </summary>
        public float GetLifeStealPercent(int enemyId)
        {
            if (_activeMutations.TryGetValue(enemyId, out var instance))
            {
                return instance.Mutation.LifeStealPercent;
            }
            return 0f;
        }

        /// <summary>
        /// 获取特殊效果伤害
        /// </summary>
        public (float poison, float electric, float frost, float burn) GetSpecialDamage(int enemyId)
        {
            if (!_activeMutations.TryGetValue(enemyId, out var instance))
            {
                return (0f, 0f, 0f, 0f);
            }

            var m = instance.Mutation;
            return (m.PoisonDamagePerSecond, m.ElectricDamage, m.FrostSlowPercent, m.BurnDamagePerSecond);
        }

        /// <summary>
        /// 获取发现进度
        /// </summary>
        public float GetDiscoveryProgress()
        {
            var allMutations = EnemyMutationDatabase.GetAllMutations();
            if (allMutations.Count == 0) return 0f;

            var discoveredCount = _discoveredMutations.Count;
            return (float)discoveredCount / allMutations.Count;
        }

        /// <summary>
        /// 获取统计信息
        /// </summary>
        public EnemyMutationData.MutationStatistics GetStatistics()
        {
            return _statistics;
        }

        /// <summary>
        /// 获取已发现的变异列表
        /// </summary>
        public List<EnemyMutationData.DiscoveredMutation> GetDiscoveredMutations()
        {
            return _discoveredMutations;
        }

        /// <summary>
        /// 设置变异几率
        /// </summary>
        public void SetMutationChance(float chance)
        {
            _mutationChance = Mathf.Clamp(chance, 0f, 1f);
        }

        /// <summary>
        /// 设置最大稀有度
        /// </summary>
        public void SetMaxRarity(EnemyMutationData.MutationRarity rarity)
        {
            _maxRarity = rarity;
        }

        /// <summary>
        /// 启用/禁用变异
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            _mutationEnabled = enabled;
        }

        /// <summary>
        /// 清除敌人变异
        /// </summary>
        public void ClearEnemyMutation(int enemyId)
        {
            _activeMutations.Remove(enemyId);
        }
        
        // === 数据持久化接口 ===
        
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary(GetSaveData());
        }
        
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("mutation_enabled"))
                _mutationEnabled = Convert.ToBoolean(data["mutation_enabled"]);
            if (data.ContainsKey("mutation_chance"))
                _mutationChance = Convert.ToSingle(data["mutation_chance"]);
            if (data.ContainsKey("max_rarity"))
                _maxRarity = (EnemyRarity)Convert.ToInt32(data["max_rarity"]);
            if (data.ContainsKey("total_encountered"))
                _statistics.TotalMutationsEncountered = Convert.ToInt32(data["total_encountered"]);
            if (data.ContainsKey("total_killed"))
                _statistics.TotalMutationsKilled = Convert.ToInt32(data["total_killed"]);
        }
        
        /// <summary>
        /// 存档数据
        /// </summary>
        public Dictionary<string, Variant> GetSaveData()
        {
            var data = new Dictionary<string, Variant>
            {
                ["mutation_enabled"] = _mutationEnabled,
                ["mutation_chance"] = _mutationChance,
                ["max_rarity"] = (int)_maxRarity,
                ["discovered_count"] = _discoveredMutations.Count,
                ["total_encountered"] = _statistics.TotalMutationsEncountered,
                ["total_killed"] = _statistics.TotalMutationsKilled
            };

            return data;
        }

        #region Data Persistence
        
        /// <summary>
        /// 导出保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override Dictionary<string, object> ExportSaveData()
        {
            return new Dictionary
            {
                { "mutation_enabled", _mutationEnabled },
                { "mutation_chance", _mutationChance },
                { "max_rarity", (int)_maxRarity },
                { "discovered_count", _discoveredMutations.Count },
                { "total_encountered", _statistics.TotalMutationsEncountered },
                { "total_killed", _statistics.TotalMutationsKilled }
            };
        }
        
        /// <summary>
        /// 导入保存数据 - 实现 BaseSystem 接口
        /// </summary>
        public override void ImportSaveData(Dictionary<string, object> data)
        {
            if (data == null) return;
            
            if (data.ContainsKey("mutation_enabled"))
                _mutationEnabled = Convert.ToBoolean(data["mutation_enabled"]);
            if (data.ContainsKey("mutation_chance"))
                _mutationChance = Convert.ToSingle(data["mutation_chance"]);
            if (data.ContainsKey("max_rarity"))
                _maxRarity = (EnemyMutationData.MutationRarity)Convert.ToInt32(data["max_rarity"]);
            if (data.ContainsKey("total_encountered"))
                _statistics.TotalMutationsEncountered = Convert.ToInt32(data["total_encountered"]);
            if (data.ContainsKey("total_killed"))
                _statistics.TotalMutationsKilled = Convert.ToInt32(data["total_killed"]);
        }
        
        #endregion
        
        /// <summary>
        /// 读档数据
        /// </summary>
        public void LoadSaveData()
        {
            // 可以从存档系统加载
            // 暂时使用默认值
        }
    }
}
