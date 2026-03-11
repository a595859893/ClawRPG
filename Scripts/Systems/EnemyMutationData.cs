using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems
{
    /// <summary>
    /// 敌人变异数据
    /// 定义敌人的随机变异类型和效果
    /// </summary>
    public class EnemyMutationData
    {
        // 变异类型
        public enum MutationType
        {
            None,
            Armored,          // 装甲化：增加防御
            Swift,            // 迅捷化：增加速度
            Vitalized,        // 生命强化：增加生命
            Frenzied,         // 狂怒化：增加攻击
            Regenerating,     // 再生：生命恢复
            Shielded,         // 护盾：周期性护盾
            Explosive,        // 爆炸化：死亡时爆炸
            Camouflaged,      // 伪装：降低被发现几率
            Vampiric,         // 吸血：攻击回复生命
            Splitting,        // 分裂：死亡时生成小怪
            Enraged,          // 愤怒：血量低时增强
            Reflective,       // 反射：反射部分伤害
            Poisonous,        // 毒化：攻击带毒
            Electric,         // 雷电：攻击带电
            Frost,            // 冰霜：攻击带冰
            Burning           // 燃烧：攻击带火
        }

        // 变异稀有度
        public enum MutationRarity
        {
            Common,      // 普通
            Uncommon,    // 优秀
            Rare,        // 稀有
            Epic,        // 史诗
            Legendary    // 传说
        }

        // 单个变异定义
        public class Mutation
        {
            public MutationType Type { get; set; }
            public MutationRarity Rarity { get; set; }
            public string Name { get; set; }
            public string Description { get; set; }
            
            // 属性修正
            public float HealthMultiplier { get; set; } = 1.0f;
            public float AttackMultiplier { get; set; } = 1.0f;
            public float DefenseMultiplier { get; set; } = 1.0f;
            public float SpeedMultiplier { get; set; } = 1.0f;
            
            // 特殊效果
            public float RegenPerSecond { get; set; } = 0f;
            public float ShieldPerSecond { get; set; } = 0f;
            public float ShieldMax { get; set; } = 0f;
            public float ExplosionDamage { get; set; } = 0f;
            public float ExplosionRadius { get; set; } = 0f;
            public float LifeStealPercent { get; set; } = 0f;
            public float DamageReflectPercent { get; set; } = 0f;
            
            // 特殊行为
            public int SplitCount { get; set; } = 0;
            public float EnrageThreshold { get; set; } = 0.3f;  // 血量低于30%时愤怒
            public float EnrageMultiplier { get; set; } = 1.5f;
            
            // 元素伤害
            public float PoisonDamagePerSecond { get; set; } = 0f;
            public float ElectricDamage { get; set; } = 0f;
            public float FrostSlowPercent { get; set; } = 0f;
            public float BurnDamagePerSecond { get; set; } = 0f;
        }

        // 敌人变异实例
        public class EnemyMutationInstance
        {
            public int EnemyId { get; set; }
            public Mutation Mutation { get; set; }
            public float ActiveShield { get; set; }
            public float TimeSinceLastShield { get; set; }
            public bool HasEnraged { get; set; }
            public bool HasSplit { get; set; }
        }

        // 玩家对变异的了解
        public class DiscoveredMutation
        {
            public MutationType Type { get; set; }
            public int KillCount { get; set; }
            public bool IsDiscovered { get; set; }
        }

        // 变异统计
        public class MutationStatistics
        {
            public int TotalMutationsEncountered { get; set; }
            public int TotalMutationsKilled { get; set; }
            public Dictionary<MutationType, int> MutationKillCount { get; set; } = new();
            public Dictionary<MutationRarity, int> RarityKillCount { get; set; } = new();
        }
    }

    /// <summary>
    /// 敌人变异数据库
    /// 配置所有可能的变异类型
    /// </summary>
    public class EnemyMutationDatabase
    {
        private static Dictionary<EnemyMutationData.MutationType, EnemyMutationData.Mutation> _mutations;
        
        static EnemyMutationDatabase()
        {
            _mutations = new Dictionary<EnemyMutationData.MutationType, EnemyMutationData.Mutation>();
            InitializeMutations();
        }

        private static void InitializeMutations()
        {
            // 普通变异
            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Armored,
                Rarity = EnemyMutationData.MutationRarity.Common,
                Name = "装甲化",
                Description = "敌人拥有厚重的装甲，防御力提升",
                DefenseMultiplier = 1.5f,
                HealthMultiplier = 1.2f,
                SpeedMultiplier = 0.8f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Swift,
                Rarity = EnemyMutationData.MutationRarity.Common,
                Name = "迅捷化",
                Description = "敌人速度极快，难以瞄准",
                SpeedMultiplier = 1.5f,
                AttackMultiplier = 1.1f,
                HealthMultiplier = 0.8f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Vitalized,
                Rarity = EnemyMutationData.MutationRarity.Common,
                Name = "生命强化",
                Description = "敌人生命值大幅提升",
                HealthMultiplier = 1.8f,
                SpeedMultiplier = 0.9f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Frenzied,
                Rarity = EnemyMutationData.MutationRarity.Common,
                Name = "狂怒化",
                Description = "敌人攻击更加凶猛",
                AttackMultiplier = 1.4f,
                SpeedMultiplier = 1.1f,
                HealthMultiplier = 0.9f
            });

            // 优秀变异
            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Regenerating,
                Rarity = EnemyMutationData.MutationRarity.Uncommon,
                Name = "再生",
                Description = "敌人可以缓慢恢复生命",
                HealthMultiplier = 1.2f,
                RegenPerSecond = 2f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Shielded,
                Rarity = EnemyMutationData.MutationRarity.Uncommon,
                Name = "护盾",
                Description = "敌人周期性获得护盾",
                ShieldPerSecond = 5f,
                ShieldMax = 50f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Poisonous,
                Rarity = EnemyMutationData.MutationRarity.Uncommon,
                Name = "毒化",
                Description = "敌人攻击附带毒性",
                AttackMultiplier = 1.1f,
                PoisonDamagePerSecond = 3f
            });

            // 稀有变异
            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Explosive,
                Rarity = EnemyMutationData.MutationRarity.Rare,
                Name = "爆炸化",
                Description = "敌人死亡时发生爆炸",
                HealthMultiplier = 1.1f,
                ExplosionDamage = 30f,
                ExplosionRadius = 80f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Vampiric,
                Rarity = EnemyMutationData.MutationRarity.Rare,
                Name = "吸血",
                Description = "敌人攻击时吸取生命",
                AttackMultiplier = 1.2f,
                LifeStealPercent = 0.15f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Electric,
                Rarity = EnemyMutationData.MutationRarity.Rare,
                Name = "雷电",
                Description = "敌人攻击附带雷电伤害",
                AttackMultiplier = 1.15f,
                ElectricDamage = 10f
            });

            // 史诗变异
            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Enraged,
                Rarity = EnemyMutationData.MutationRarity.Epic,
                Name = "愤怒",
                Description = "敌人血量低时进入愤怒状态",
                HealthMultiplier = 1.3f,
                EnrageThreshold = 0.3f,
                EnrageMultiplier = 1.8f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Splitting,
                Rarity = EnemyMutationData.MutationRarity.Epic,
                Name = "分裂",
                Description = "敌人死亡时分裂成多个小怪",
                HealthMultiplier = 1.5f,
                SplitCount = 3
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Camouflaged,
                Rarity = EnemyMutationData.MutationRarity.Epic,
                Name = "伪装",
                Description = "敌人难以被发现",
                HealthMultiplier = 0.8f,
                SpeedMultiplier = 1.2f
            });

            // 传说变异
            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Frost,
                Rarity = EnemyMutationData.MutationRarity.Legendary,
                Name = "冰霜",
                Description = "敌人攻击附带冰霜效果",
                AttackMultiplier = 1.3f,
                FrostSlowPercent = 0.5f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Burning,
                Rarity = EnemyMutationData.MutationRarity.Legendary,
                Name = "燃烧",
                Description = "敌人攻击附带燃烧效果",
                AttackMultiplier = 1.3f,
                BurnDamagePerSecond = 5f
            });

            AddMutation(new EnemyMutationData.Mutation
            {
                Type = EnemyMutationData.MutationType.Reflective,
                Rarity = EnemyMutationData.MutationRarity.Legendary,
                Name = "反射",
                Description = "敌人反射部分受到的伤害",
                HealthMultiplier = 1.4f,
                DamageReflectPercent = 0.2f
            });
        }

        private static void AddMutation(EnemyMutationData.Mutation mutation)
        {
            _mutations[mutation.Type] = mutation;
        }

        public static EnemyMutationData.Mutation GetMutation(EnemyMutationData.MutationType type)
        {
            return _mutations.ContainsKey(type) ? _mutations[type] : null;
        }

        public static EnemyMutationData.Mutation GetRandomMutation(EnemyMutationData.MutationRarity maxRarity)
        {
            var validMutations = new List<EnemyMutationData.Mutation>();
            
            foreach (var mutation in _mutations.Values)
            {
                if (mutation.Rarity <= maxRarity)
                {
                    validMutations.Add(mutation);
                }
            }
            
            if (validMutations.Count == 0) return null;
            
            // 根据稀有度权重随机选择
            var totalWeight = 0;
            foreach (var m in validMutations)
            {
                totalWeight += GetRarityWeight(m.Rarity);
            }
            
            var random = new Random();
            var roll = random.Next(totalWeight);
            var currentWeight = 0;
            
            foreach (var m in validMutations)
            {
                currentWeight += GetRarityWeight(m.Rarity);
                if (roll < currentWeight)
                {
                    return m;
                }
            }
            
            return validMutations[random.Next(validMutations.Count)];
        }

        private static int GetRarityWeight(EnemyMutationData.MutationRarity rarity)
        {
            return rarity switch
            {
                EnemyMutationData.MutationRarity.Common => 50,
                EnemyMutationData.MutationRarity.Uncommon => 30,
                EnemyMutationData.MutationRarity.Rare => 15,
                EnemyMutationData.MutationRarity.Epic => 4,
                EnemyMutationData.MutationRarity.Legendary => 1,
                _ => 10
            };
        }

        public static List<EnemyMutationData.Mutation> GetAllMutations()
        {
            return new List<EnemyMutationData.Mutation>(_mutations.Values);
        }

        public static Dictionary<EnemyMutationData.MutationRarity, int> GetRarityDistribution()
        {
            var distribution = new Dictionary<EnemyMutationData.MutationRarity, int>();
            foreach (EnemyMutationData.MutationRarity rarity in Enum.GetValues(typeof(EnemyMutationData.MutationRarity)))
            {
                distribution[rarity] = 0;
            }
            
            foreach (var mutation in _mutations.Values)
            {
                distribution[mutation.Rarity]++;
            }
            
            return distribution;
        }
    }
}
