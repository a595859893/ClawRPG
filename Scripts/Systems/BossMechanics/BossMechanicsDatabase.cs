using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// Boss 机制数据库 - 存储所有 Boss 的阶段和狂暴配置
    /// </summary>
    public static class BossMechanicsDatabase {
        // 预配置的 Boss 阶段配置
        private static readonly Dictionary<string, List<BossPhaseConfig>> _bossPhases = new Dictionary<string, List<BossPhaseConfig>>() {
            // 森林 Boss - 森林之王
            {
                "forest_boss", new List<BossPhaseConfig> {
                    new BossPhaseConfig {
                        phaseName = "第一阶段",
                        phaseType = BossPhaseType.Normal,
                        healthPercent = 100f,
                        duration = 0f,
                        damageMultiplier = 1.0f,
                        speedMultiplier = 1.0f,
                        attackSpeedMultiplier = 1.0f,
                        availableAbilities = new List<string> { "root_attack", "vine_whip" },
                        phaseEnterEffect = "",
                        showWarning = false
                    },
                    new BossPhaseConfig {
                        phaseName = "第二阶段",
                        phaseType = BossPhaseType.Enraged,
                        healthPercent = 50f,
                        duration = 0f,
                        damageMultiplier = 1.5f,
                        speedMultiplier = 1.3f,
                        attackSpeedMultiplier = 1.2f,
                        availableAbilities = new List<string> { "root_attack", "vine_whip", "entangle", "overgrowth" },
                        phaseEnterEffect = "green_aura",
                        showWarning = true,
                        warningMessage = "森林之王进入狂暴状态!"
                    },
                    new BossPhaseConfig {
                        phaseName = "最终阶段",
                        phaseType = BossPhaseType.Final,
                        healthPercent = 25f,
                        duration = 0f,
                        damageMultiplier = 2.0f,
                        speedMultiplier = 1.5f,
                        attackSpeedMultiplier = 1.5f,
                        availableAbilities = new List<string> { "root_attack", "vine_whip", "entangle", "overgrowth", "nature_wrath" },
                        phaseEnterEffect = "nature_explosion",
                        showWarning = true,
                        warningMessage = "森林之王释放终极力量!"
                    }
                }
            },
            // 火焰 Boss - 炎魔领主
            {
                "fire_boss", new List<BossPhaseConfig> {
                    new BossPhaseConfig {
                        phaseName = "第一阶段",
                        phaseType = BossPhaseType.Normal,
                        healthPercent = 100f,
                        duration = 0f,
                        damageMultiplier = 1.0f,
                        speedMultiplier = 1.0f,
                        attackSpeedMultiplier = 1.0f,
                        availableAbilities = new List<string> { "fireball", "flame_breath" },
                        phaseEnterEffect = "",
                        showWarning = false
                    },
                    new BossPhaseConfig {
                        phaseName = "熔岩形态",
                        phaseType = BossPhaseType.Enraged,
                        healthPercent = 60f,
                        duration = 0f,
                        damageMultiplier = 1.4f,
                        speedMultiplier = 1.2f,
                        attackSpeedMultiplier = 1.3f,
                        availableAbilities = new List<string> { "fireball", "flame_breath", "lava_eruption", "magma_armor" },
                        phaseEnterEffect = "lava_glow",
                        showWarning = true,
                        warningMessage = "炎魔领主进入熔岩形态!"
                    },
                    new BossPhaseConfig {
                        phaseName = "毁灭形态",
                        phaseType = BossPhaseType.Final,
                        healthPercent = 30f,
                        duration = 0f,
                        damageMultiplier = 2.0f,
                        speedMultiplier = 1.4f,
                        attackSpeedMultiplier = 1.6f,
                        availableAbilities = new List<string> { "fireball", "flame_breath", "lava_eruption", "magma_armor", "inferno" },
                        phaseEnterEffect = "fire_explosion",
                        showWarning = true,
                        warningMessage = "炎魔领主释放毁灭之火!"
                    }
                }
            },
            // 冰霜 Boss - 冰霜巨龙
            {
                "ice_boss", new List<BossPhaseConfig> {
                    new BossPhaseConfig {
                        phaseName = "第一阶段",
                        phaseType = BossPhaseType.Normal,
                        healthPercent = 100f,
                        duration = 0f,
                        damageMultiplier = 1.0f,
                        speedMultiplier = 1.0f,
                        attackSpeedMultiplier = 1.0f,
                        availableAbilities = new List<string> { "ice_shard", "frost_breath" },
                        phaseEnterEffect = "",
                        showWarning = false
                    },
                    new BossPhaseConfig {
                        phaseName = "暴风雪形态",
                        phaseType = BossPhaseType.Enraged,
                        healthPercent = 65f,
                        duration = 0f,
                        damageMultiplier = 1.3f,
                        speedMultiplier = 1.4f,
                        attackSpeedMultiplier = 1.2f,
                        availableAbilities = new List<string> { "ice_shard", "frost_breath", "blizzard", "ice_wall" },
                        phaseEnterEffect = "ice_aura",
                        showWarning = true,
                        warningMessage = "冰霜巨龙召唤暴风雪!"
                    },
                    new BossPhaseConfig {
                        phaseName = "绝对零度",
                        phaseType = BossPhaseType.Final,
                        healthPercent = 25f,
                        duration = 0f,
                        damageMultiplier = 1.8f,
                        speedMultiplier = 1.6f,
                        attackSpeedMultiplier = 1.4f,
                        availableAbilities = new List<string> { "ice_shard", "frost_breath", "blizzard", "ice_wall", "absolute_zero" },
                        phaseEnterEffect = "ice_explosion",
                        showWarning = true,
                        warningMessage = "冰霜巨龙释放绝对零度!"
                    }
                }
            },
            // 暗影 Boss - 暗影君王
            {
                "shadow_boss", new List<BossPhaseConfig> {
                    new BossPhaseConfig {
                        phaseName = "第一阶段",
                        phaseType = BossPhaseType.Normal,
                        healthPercent = 100f,
                        duration = 0f,
                        damageMultiplier = 1.0f,
                        speedMultiplier = 1.0f,
                        attackSpeedMultiplier = 1.0f,
                        availableAbilities = new List<string> { "shadow_strike", "dark_bolt" },
                        phaseEnterEffect = "",
                        showWarning = false
                    },
                    new BossPhaseConfig {
                        phaseName = "暗影形态",
                        phaseType = BossPhaseType.Enraged,
                        healthPercent = 55f,
                        duration = 0f,
                        damageMultiplier = 1.5f,
                        speedMultiplier = 1.5f,
                        attackSpeedMultiplier = 1.3f,
                        availableAbilities = new List<string> { "shadow_strike", "dark_bolt", "shadow_clones", "dark_void" },
                        phaseEnterEffect = "shadow_aura",
                        showWarning = true,
                        warningMessage = "暗影君王分裂暗影分身!"
                    },
                    new BossPhaseConfig {
                        phaseName = "虚无形态",
                        phaseType = BossPhaseType.Final,
                        healthPercent = 20f,
                        duration = 0f,
                        damageMultiplier = 2.2f,
                        speedMultiplier = 1.8f,
                        attackSpeedMultiplier = 1.5f,
                        availableAbilities = new List<string> { "shadow_strike", "dark_bolt", "shadow_clones", "dark_void", "void_annihilation" },
                        phaseEnterEffect = "void_explosion",
                        showWarning = true,
                        warningMessage = "暗影君王撕裂现实!"
                    }
                }
            },
            // 神圣 Boss - 光明主教
            {
                "holy_boss", new List<BossPhaseConfig> {
                    new BossPhaseConfig {
                        phaseName = "第一阶段",
                        phaseType = BossPhaseType.Normal,
                        healthPercent = 100f,
                        duration = 0f,
                        damageMultiplier = 1.0f,
                        speedMultiplier = 1.0f,
                        attackSpeedMultiplier = 1.0f,
                        availableAbilities = new List<string> { "holy_bolt", "light_beam" },
                        phaseEnterEffect = "",
                        showWarning = false
                    },
                    new BossPhaseConfig {
                        phaseName = "审判形态",
                        phaseType = BossPhaseType.Enraged,
                        healthPercent = 60f,
                        duration = 0f,
                        damageMultiplier = 1.4f,
                        speedMultiplier = 1.2f,
                        attackSpeedMultiplier = 1.4f,
                        availableAbilities = new List<string> { "holy_bolt", "light_beam", "divine_judgment", "holy_shield" },
                        phaseEnterEffect = "holy_aura",
                        showWarning = true,
                        warningMessage = "光明主教启动审判!"
                    },
                    new BossPhaseConfig {
                        phaseName = "降临形态",
                        phaseType = BossPhaseType.Final,
                        healthPercent = 25f,
                        duration = 0f,
                        damageMultiplier = 2.0f,
                        speedMultiplier = 1.4f,
                        attackSpeedMultiplier = 1.6f,
                        availableAbilities = new List<string> { "holy_bolt", "light_beam", "divine_judgment", "holy_shield", "apotheosis" },
                        phaseEnterEffect = "light_explosion",
                        showWarning = true,
                        warningMessage = "光明主教降临凡间!"
                    }
                }
            }
        };

        // 预配置的 Boss 狂暴配置
        private static readonly Dictionary<string, List<EnrageConfig>> _bossEnrages = new Dictionary<string, List<EnrageConfig>> {
            {
                "forest_boss", new List<EnrageConfig> {
                    new EnrageConfig {
                        triggerName = "时间狂暴",
                        triggerType = EnrageTriggerType.TimeBased,
                        triggerValue = 120f,
                        damageBonus = 0.3f,
                        speedBonus = 0.2f,
                        attackSpeedBonus = 0.2f,
                        immuneToStun = false,
                        enrageEffect = "green_flames",
                        enrageMessage = "森林之王被激怒了!"
                    },
                    new EnrageConfig {
                        triggerName = "血量狂暴",
                        triggerType = EnrageTriggerType.HealthBased,
                        triggerValue = 30f,
                        damageBonus = 0.5f,
                        speedBonus = 0.3f,
                        attackSpeedBonus = 0.3f,
                        immuneToStun = true,
                        enrageEffect = "nature_rage",
                        enrageMessage = "森林之王陷入疯狂!"
                    }
                }
            },
            {
                "fire_boss", new List<EnrageConfig> {
                    new EnrageConfig {
                        triggerName = "时间狂暴",
                        triggerType = EnrageTriggerType.TimeBased,
                        triggerValue = 90f,
                        damageBonus = 0.4f,
                        speedBonus = 0.3f,
                        attackSpeedBonus = 0.3f,
                        immuneToStun = true,
                        enrageEffect = "fire_rage",
                        enrageMessage = "炎魔领主燃烧更旺!"
                    },
                    new EnrageConfig {
                        triggerName = "血量狂暴",
                        triggerType = EnrageTriggerType.HealthBased,
                        triggerValue = 25f,
                        damageBonus = 0.6f,
                        speedBonus = 0.4f,
                        attackSpeedBonus = 0.4f,
                        immuneToStun = true,
                        enrageEffect = "inferno_rage",
                        enrageMessage = "炎魔领主释放毁灭之力!"
                    }
                }
            },
            {
                "ice_boss", new List<EnrageConfig> {
                    new EnrageConfig {
                        triggerName = "时间狂暴",
                        triggerType = EnrageTriggerType.TimeBased,
                        triggerValue = 100f,
                        damageBonus = 0.3f,
                        speedBonus = 0.4f,
                        attackSpeedBonus = 0.2f,
                        immuneToSlow = true,
                        enrageEffect = "blizzard_rage",
                        enrageMessage = "冰霜巨龙更加愤怒!"
                    }
                }
            },
            {
                "shadow_boss", new List<EnrageConfig> {
                    new EnrageConfig {
                        triggerName = "伤害累积狂暴",
                        triggerType = EnrageTriggerType.DamageBased,
                        triggerValue = 5000f,
                        damageBonus = 0.5f,
                        speedBonus = 0.5f,
                        attackSpeedBonus = 0.4f,
                        immuneToStun = true,
                        immuneToSlow = true,
                        enrageEffect = "shadow_rage",
                        enrageMessage = "暗影君王吸收伤害变强!"
                    }
                }
            },
            {
                "holy_boss", new List<EnrageConfig> {
                    new EnrageConfig {
                        triggerName = "时间狂暴",
                        triggerType = EnrageTriggerType.TimeBased,
                        triggerValue = 150f,
                        damageBonus = 0.35f,
                        speedBonus = 0.25f,
                        attackSpeedBonus = 0.35f,
                        immuneToStun = true,
                        enrageEffect = "holy_rage",
                        enrageMessage = "光明主教彰显神威!"
                    }
                }
            }
        };

        // 预配置的 Boss 特殊机制
        private static readonly Dictionary<string, List<BossSpecialMechanic>> _bossSpecialMechanics = new Dictionary<string, List<BossSpecialMechanic>> {
            {
                "forest_boss", new List<BossSpecialMechanic> {
                    new BossSpecialMechanic {
                        mechanicName = "召唤树人",
                        description = "召唤树人助手战斗",
                        mechanicType = MechanicType.SummonMinions,
                        triggerChance = 0.2f,
                        cooldown = 30f,
                        effects = new Dictionary<string, float> {
                            { "minion_count", 3f },
                            { "minion_health", 100f },
                            { "minion_damage", 20f }
                        }
                    },
                    new BossSpecialMechanic {
                        mechanicName = "藤蔓缠绕",
                        description = "定身所有玩家",
                        mechanicType = MechanicType.AreaOfEffect,
                        triggerChance = 0.15f,
                        cooldown = 45f,
                        effects = new Dictionary<string, float> {
                            { "duration", 3f },
                            { "radius", 8f },
                            { "damage", 50f }
                        }
                    },
                    new BossSpecialMechanic {
                        mechanicName = "自然之怒",
                        description = "全屏范围攻击",
                        mechanicType = MechanicType.Ultimate,
                        triggerChance = 0.1f,
                        cooldown = 60f,
                        effects = new Dictionary<string, float> {
                            { "damage", 200f },
                            { "radius", 15f }
                        }
                    }
                }
            },
            {
                "fire_boss", new List<BossSpecialMechanic> {
                    new BossSpecialMechanic {
                        mechanicName = "岩浆爆发",
                        description = "随机位置岩浆喷发",
                        mechanicType = MechanicType.AreaOfEffect,
                        triggerChance = 0.25f,
                        cooldown = 20f,
                        effects = new Dictionary<string, float> {
                            { "damage", 80f },
                            { "radius", 5f },
                            { "count", 5f }
                        }
                    },
                    new BossSpecialMechanic {
                        mechanicName = "火焰护盾",
                        description = "获得火焰护盾",
                        mechanicType = MechanicType.Shield,
                        triggerChance = 0.2f,
                        cooldown = 40f,
                        effects = new Dictionary<string, float> {
                            { "shield_health", 500f },
                            { "duration", 10f },
                            { "reflect_damage", 0.3f }
                        }
                    },
                    new BossSpecialMechanic {
                        mechanicName = "地狱火雨",
                        description = "全屏火焰攻击",
                        mechanicType = MechanicType.Ultimate,
                        triggerChance = 0.08f,
                        cooldown = 90f,
                        effects = new Dictionary<string, float> {
                            { "damage", 300f },
                            { "duration", 10f }
                        }
                    }
                }
            },
            {
                "ice_boss", new List<BossSpecialMechanic> {
                    new BossSpecialMechanic {
                        mechanicName = "冰墙",
                        description = "召唤冰墙阻挡",
                        mechanicType = MechanicType.AreaOfEffect,
                        triggerChance = 0.25f,
                        cooldown = 25f,
                        effects = new Dictionary<string, float> {
                            { "wall_count", 3f },
                            { "health", 300f }
                        }
                    },
                    new BossSpecialMechanic {
                        mechanicName = "时间减缓",
                        description = "减缓所有玩家",
                        mechanicType = MechanicType.TimeSlow,
                        triggerChance = 0.15f,
                        cooldown = 50f,
                        effects = new Dictionary<string, float> {
                            { "duration", 5f },
                            { "slow_amount", 0.5f }
                        }
                    }
                }
            },
            {
                "shadow_boss", new List<BossSpecialMechanic> {
                    new BossSpecialMechanic {
                        mechanicName = "暗影分身",
                        description = "创造暗影分身",
                        mechanicType = MechanicType.SummonMinions,
                        triggerChance = 0.2f,
                        cooldown = 35f,
                        effects = new Dictionary<string, float> {
                            { "clone_count", 2f },
                            { "clone_damage", 40f },
                            { "clone_health", 200f }
                        }
                    },
                    new BossSpecialMechanic {
                        mechanicName = "暗影瞬移",
                        description = "瞬间移动位置",
                        mechanicType = MechanicType.Teleport,
                        triggerChance = 0.3f,
                        cooldown = 15f,
                        effects = new Dictionary<string, float> {
                            { "min_distance", 10f }
                        }
                    },
                    new BossSpecialMechanic {
                        mechanicName = "虚空领域",
                        description = "创造危险领域",
                        mechanicType = MechanicType.AreaOfEffect,
                        triggerChance = 0.1f,
                        cooldown = 60f,
                        effects = new Dictionary<string, float> {
                            { "damage_per_second", 50f },
                            { "radius", 12f },
                            { "duration", 15f }
                        }
                    }
                }
            },
            {
                "holy_boss", new List<BossSpecialMechanic> {
                    new BossSpecialMechanic {
                        mechanicName = "神圣护盾",
                        description = "获得无敌护盾",
                        mechanicType = MechanicType.Shield,
                        triggerChance = 0.25f,
                        cooldown = 45f,
                        effects = new Dictionary<string, float> {
                            { "shield_health", 800f },
                            { "duration", 8f },
                            { "heal_over_time", 20f }
                        }
                    },
                    new BossSpecialMechanic {
                        mechanicName = "神圣审判",
                        description = "选择玩家审判",
                        mechanicType = MechanicType.Ultimate,
                        triggerChance = 0.1f,
                        cooldown = 90f,
                        effects = new Dictionary<string, float> {
                            { "damage", 400f },
                            { "mark_duration", 10f }
                        }
                    }
                }
            }
        };

        /// <summary>
        /// 获取 Boss 阶段配置
        /// </summary>
        public static List<BossPhaseConfig> GetBossPhases(string bossId) {
            if (_bossPhases.ContainsKey(bossId)) {
                return new List<BossPhaseConfig>(_bossPhases[bossId]);
            }
            return GetDefaultPhases();
        }

        /// <summary>
        /// 获取 Boss 狂暴配置
        /// </summary>
        public static List<EnrageConfig> GetBossEnrages(string bossId) {
            if (_bossEnrages.ContainsKey(bossId)) {
                return new List<EnrageConfig>(_bossEnrages[bossId]);
            }
            return new List<EnrageConfig>();
        }

        /// <summary>
        /// 获取 Boss 特殊机制
        /// </summary>
        public static List<BossSpecialMechanic> GetBossSpecialMechanics(string bossId) {
            if (_bossSpecialMechanics.ContainsKey(bossId)) {
                return new List<BossSpecialMechanic>(_bossSpecialMechanics[bossId]);
            }
            return new List<BossSpecialMechanic>();
        }

        /// <summary>
        /// 获取默认阶段配置
        /// </summary>
        private static List<BossPhaseConfig> GetDefaultPhases() {
            return new List<BossPhaseConfig> {
                new BossPhaseConfig {
                    phaseName = "普通阶段",
                    phaseType = BossPhaseType.Normal,
                    healthPercent = 100f,
                    damageMultiplier = 1.0f,
                    speedMultiplier = 1.0f,
                    attackSpeedMultiplier = 1.0f
                },
                new BossPhaseConfig {
                    phaseName = "狂暴阶段",
                    phaseType = BossPhaseType.Enraged,
                    healthPercent = 50f,
                    damageMultiplier = 1.5f,
                    speedMultiplier = 1.3f,
                    attackSpeedMultiplier = 1.3f
                },
                new BossPhaseConfig {
                    phaseName = "最终阶段",
                    phaseType = BossPhaseType.Final,
                    healthPercent = 25f,
                    damageMultiplier = 2.0f,
                    speedMultiplier = 1.5f,
                    attackSpeedMultiplier = 1.5f
                }
            };
        }

        /// <summary>
        /// 获取所有配置的 Boss ID 列表
        /// </summary>
        public static List<string> GetAllConfiguredBossIds() {
            return new List<string>(_bossPhases.Keys);
        }
    }
}
