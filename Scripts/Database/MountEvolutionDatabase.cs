using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Mounts {
    /// <summary>
    /// 坐骑进化数据库
    /// </summary>
    public static class MountEvolutionDatabase {
        // 进化阶段配置
        public static readonly Dictionary<MountEvolutionStage, EvolutionStageConfig> StageConfigs = new Dictionary<MountEvolutionStage, EvolutionStageConfig> {
            { MountEvolutionStage.Basic, new EvolutionStageConfig {
                Stage = MountEvolutionStage.Basic,
                StageName = "基础",
                RequiredLevel = 1,
                RequiredExp = 0,
                RequiredItems = 0,
                HealthBonus = 0f,
                AttackBonus = 0f,
                DefenseBonus = 0f,
                SpeedBonus = 0f,
                CritRateBonus = 0f,
                CritDamageBonus = 0f
            }},
            { MountEvolutionStage.Advanced, new EvolutionStageConfig {
                Stage = MountEvolutionStage.Advanced,
                StageName = "进阶",
                RequiredLevel = 15,
                RequiredExp = 1000,
                RequiredItems = 5,
                HealthBonus = 10f,
                AttackBonus = 8f,
                DefenseBonus = 8f,
                SpeedBonus = 5f,
                CritRateBonus = 2f,
                CritDamageBonus = 5f
            }},
            { MountEvolutionStage.Elite, new EvolutionStageConfig {
                Stage = MountEvolutionStage.Elite,
                StageName = "精英",
                RequiredLevel = 30,
                RequiredExp = 5000,
                RequiredItems = 15,
                HealthBonus = 25f,
                AttackBonus = 20f,
                DefenseBonus = 20f,
                SpeedBonus = 10f,
                CritRateBonus = 5f,
                CritDamageBonus = 15f
            }},
            { MountEvolutionStage.Epic, new EvolutionStageConfig {
                Stage = MountEvolutionStage.Epic,
                StageName = "史诗",
                RequiredLevel = 45,
                RequiredExp = 15000,
                RequiredItems = 30,
                HealthBonus = 40f,
                AttackBonus = 35f,
                DefenseBonus = 35f,
                SpeedBonus = 15f,
                CritRateBonus = 8f,
                CritDamageBonus = 25f
            }},
            { MountEvolutionStage.Legendary, new EvolutionStageConfig {
                Stage = MountEvolutionStage.Legendary,
                StageName = "传说",
                RequiredLevel = 60,
                RequiredExp = 40000,
                RequiredItems = 50,
                HealthBonus = 60f,
                AttackBonus = 50f,
                DefenseBonus = 50f,
                SpeedBonus = 20f,
                CritRateBonus = 12f,
                CritDamageBonus = 40f
            }}
        };

        // 进化类型配置
        public static readonly Dictionary<MountEvolutionType, EvolutionTypeConfig> TypeConfigs = new Dictionary<MountEvolutionType, EvolutionTypeConfig> {
            { MountEvolutionType.Fire, new EvolutionTypeConfig {
                Type = MountEvolutionType.Fire,
                TypeName = "火焰",
                Description = "获得火焰抗性和火焰伤害加成",
                ElementColor = new Color(1f, 0.4f, 0.2f),
                FireResist = 30f,
                IceResist = -10f,
                LightningResist = 0f,
                DarkResist = 10f,
                HolyResist = -5f
            }},
            { MountEvolutionType.Ice, new EvolutionTypeConfig {
                Type = MountEvolutionType.Ice,
                TypeName = "冰霜",
                Description = "获得冰霜抗性和冰霜伤害加成",
                ElementColor = new Color(0.4f, 0.8f, 1f),
                FireResist = -10f,
                IceResist = 30f,
                LightningResist = 0f,
                DarkResist = 5f,
                HolyResist = 10f
            }},
            { MountEvolutionType.Lightning, new EvolutionTypeConfig {
                Type = MountEvolutionType.Lightning,
                TypeName = "闪电",
                Description = "获得闪电抗性和速度加成",
                ElementColor = new Color(1f, 1f, 0.3f),
                FireResist = 5f,
                IceResist = 5f,
                LightningResist = 30f,
                DarkResist = 0f,
                HolyResist = 0f
            }},
            { MountEvolutionType.Dark, new EvolutionTypeConfig {
                Type = MountEvolutionType.Dark,
                TypeName = "黑暗",
                Description = "获得暗影抗性和暴击加成",
                ElementColor = new Color(0.4f, 0.2f, 0.5f),
                FireResist = 5f,
                IceResist = 5f,
                LightningResist = 5f,
                DarkResist = 30f,
                HolyResist = -20f
            }},
            { MountEvolutionType.Holy, new EvolutionTypeConfig {
                Type = MountEvolutionType.Holy,
                TypeName = "神圣",
                Description = "获得神圣抗性和治疗加成",
                ElementColor = new Color(1f, 0.9f, 0.4f),
                FireResist = 10f,
                IceResist = 10f,
                LightningResist = 5f,
                DarkResist = 20f,
                HolyResist = 30f
            }},
            { MountEvolutionType.Nature, new EvolutionTypeConfig {
                Type = MountEvolutionType.Nature,
                TypeName = "自然",
                Description = "获得生命偷取和生命值加成",
                ElementColor = new Color(0.3f, 0.8f, 0.3f),
                FireResist = 10f,
                IceResist = 10f,
                LightningResist = 5f,
                DarkResist = 5f,
                HolyResist = 5f
            }}
        };

        // 进化链配置
        public static readonly Dictionary<MountEvolutionChain, EvolutionChainConfig> ChainConfigs = new Dictionary<MountEvolutionChain, EvolutionChainConfig> {
            { MountEvolutionChain.Horse, new EvolutionChainConfig {
                Chain = MountEvolutionChain.Horse,
                ChainName = "马",
                BaseMountId = "horse_basic",
                EvolutionPaths = new List<string> { "horse_fire", "horse_ice", "horse_lightning", "horse_dark", "horse_holy", "horse_nature" }
            }},
            { MountEvolutionChain.Wolf, new EvolutionChainConfig {
                Chain = MountEvolutionChain.Wolf,
                ChainName = "狼",
                BaseMountId = "wolf_basic",
                EvolutionPaths = new List<string> { "wolf_fire", "wolf_ice", "wolf_lightning", "wolf_dark", "wolf_holy", "wolf_nature" }
            }},
            { MountEvolutionChain.Bear, new EvolutionChainConfig {
                Chain = MountEvolutionChain.Bear,
                ChainName = "熊",
                BaseMountId = "bear_basic",
                EvolutionPaths = new List<string> { "bear_fire", "bear_ice", "bear_lightning", "bear_dark", "bear_holy", "bear_nature" }
            }},
            { MountEvolutionChain.Eagle, new EvolutionChainConfig {
                Chain = MountEvolutionChain.Eagle,
                ChainName = "鹰",
                BaseMountId = "eagle_basic",
                EvolutionPaths = new List<string> { "eagle_fire", "eagle_ice", "eagle_lightning", "eagle_dark", "eagle_holy", "eagle_nature" }
            }},
            { MountEvolutionChain.Dragon, new EvolutionChainConfig {
                Chain = MountEvolutionChain.Dragon,
                ChainName = "龙",
                BaseMountId = "dragon_basic",
                EvolutionPaths = new List<string> { "dragon_fire", "dragon_ice", "dragon_lightning", "dragon_dark", "dragon_holy", "dragon_nature" }
            }},
            { MountEvolutionChain.Phoenix, new EvolutionChainConfig {
                Chain = MountEvolutionChain.Phoenix,
                ChainName = "凤凰",
                BaseMountId = "phoenix_basic",
                EvolutionPaths = new List<string> { "phoenix_fire", "phoenix_ice", "phoenix_lightning", "phoenix_dark", "phoenix_holy", "phoenix_nature" }
            }},
            { MountEvolutionChain.Griffin, new EvolutionChainConfig {
                Chain = MountEvolutionChain.Griffin,
                ChainName = "狮鹫",
                BaseMountId = "griffin_basic",
                EvolutionPaths = new List<string> { "griffin_fire", "griffin_ice", "griffin_lightning", "griffin_dark", "griffin_holy", "griffin_nature" }
            }},
            { MountEvolutionChain.Unicorn, new EvolutionChainConfig {
                Chain = MountEvolutionChain.Unicorn,
                ChainName = "独角兽",
                BaseMountId = "unicorn_basic",
                EvolutionPaths = new List<string> { "unicorn_fire", "unicorn_ice", "unicorn_lightning", "unicorn_dark", "unicorn_holy", "unicorn_nature" }
            }}
        };

        /// <summary>
        /// 获取下一进化阶段
        /// </summary>
        public static MountEvolutionStage GetNextStage(MountEvolutionStage currentStage) {
            switch (currentStage) {
                case MountEvolutionStage.Basic: return MountEvolutionStage.Advanced;
                case MountEvolutionStage.Advanced: return MountEvolutionStage.Elite;
                case MountEvolutionStage.Elite: return MountEvolutionStage.Epic;
                case MountEvolutionStage.Epic: return MountEvolutionStage.Legendary;
                default: return MountEvolutionStage.Legendary;
            }
        }

        /// <summary>
        /// 获取进化阶段配置
        /// </summary>
        public static EvolutionStageConfig GetStageConfig(MountEvolutionStage stage) {
            return StageConfigs.ContainsKey(stage) ? StageConfigs[stage] : null;
        }

        /// <summary>
        /// 获取进化类型配置
        /// </summary>
        public static EvolutionTypeConfig GetTypeConfig(MountEvolutionType type) {
            return TypeConfigs.ContainsKey(type) ? TypeConfigs[type] : null;
        }

        /// <summary>
        /// 获取进化链配置
        /// </summary>
        public static EvolutionChainConfig GetChainConfig(MountEvolutionChain chain) {
            return ChainConfigs.ContainsKey(chain) ? ChainConfigs[chain] : null;
        }

        /// <summary>
        /// 检查是否可以进化到指定阶段
        /// </summary>
        public static bool CanEvolveToStage(MountEvolutionStage currentStage, MountEvolutionStage targetStage) {
            var currentIndex = (int)currentStage;
            var targetIndex = (int)targetStage;
            return targetIndex > currentIndex && targetIndex <= (int)MountEvolutionStage.Legendary;
        }

        /// <summary>
        /// 获取进化所需金币
        /// </summary>
        public static int GetEvolutionGoldCost(MountEvolutionStage stage) {
            switch (stage) {
                case MountEvolutionStage.Advanced: return 500;
                case MountEvolutionStage.Elite: return 2000;
                case MountEvolutionStage.Epic: return 8000;
                case MountEvolutionStage.Legendary: return 30000;
                default: return 0;
            }
        }

        /// <summary>
        /// 获取进化材料名称
        /// </summary>
        public static string GetEvolutionMaterialName(MountEvolutionStage stage) {
            switch (stage) {
                case MountEvolutionStage.Advanced: return "初级进化石";
                case MountEvolutionStage.Elite: return "中级进化石";
                case MountEvolutionStage.Epic: return "高级进化石";
                case MountEvolutionStage.Legendary: return "传说进化石";
                default: return "";
            }
        }
    }
}
