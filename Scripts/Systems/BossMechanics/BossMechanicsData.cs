using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Systems.BossMechanics {
    /// <summary>
    /// Boss 阶段类型
    /// </summary>
    public enum BossPhaseType {
        Normal,      // 普通阶段
        Enraged,     // 狂暴阶段
        Desperate,   // 绝望阶段
        Transition,  // 转换阶段
        Final        // 最终阶段
    }

    /// <summary>
    /// 狂暴触发条件类型
    /// </summary>
    public enum EnrageTriggerType {
        TimeBased,       // 基于时间
        HealthBased,     // 基于血量
        DamageBased,     // 基于伤害
        PlayerCount      // 基于玩家数量
    }

    /// <summary>
    /// Boss 阶段配置数据
    /// </summary>
    [System.Serializable]
    public class BossPhaseConfig {
        public string phaseName;
        public BossPhaseType phaseType;
        public float healthPercent;           // 进入该阶段的血量百分比
        public float duration;                // 阶段持续时间(秒)
        public float damageMultiplier;        // 伤害乘数
        public float speedMultiplier;         // 速度乘数
        public float attackSpeedMultiplier;   // 攻击速度乘数
        public List<string> availableAbilities;  // 可用技能列表
        public string phaseEnterEffect;       // 进入阶段特效
        public string phaseExitEffect;        // 退出阶段特效
        public bool showWarning;              // 是否显示警告
        public string warningMessage;         // 警告消息
    }

    /// <summary>
    /// 狂暴配置数据
    /// </summary>
    [System.Serializable]
    public class EnrageConfig {
        public string triggerName;
        public EnrageTriggerType triggerType;
        public float triggerValue;            // 触发值(时间/血量/伤害)
        public float damageBonus;             // 伤害加成
        public float speedBonus;              // 速度加成
        public float attackSpeedBonus;        // 攻击速度加成
        public bool immuneToStun;             // 是否免疫眩晕
        public bool immuneToSlow;             // 是否免疫减速
        public string enrageEffect;          // 狂暴特效
        public string enrageMessage;         // 狂暴消息
    }

    /// <summary>
    /// Boss 特殊机制配置
    /// </summary>
    [System.Serializable]
    public class BossSpecialMechanic {
        public string mechanicName;
        public string description;
        public MechanicType mechanicType;
        public float triggerChance;           // 触发几率
        public float cooldown;               // 冷却时间
        public Dictionary<string, float> effects;  // 效果参数
    }

    public enum MechanicType {
        Teleport,           // 瞬移
        SummonMinions,      // 召唤小怪
        AreaOfEffect,       // 范围攻击
        ProjectileStorm,    // 投射物风暴
        Shield,             // 护盾
        LifeDrain,          // 生命吸取
        TimeSlow,           // 时间减缓
        PhaseShift,         // 阶段转换
        Enrage,             // 狂暴
        Ultimate            // 终极技能
    }

    /// <summary>
    /// 玩家 Boss 战斗记录
    /// </summary>
    [System.Serializable]
    public class PlayerBossRecord {
        public string bossId;
        public int timesFought;
        public int timesDefeated;
        public float bestTime;
        public float totalDamageDealt;
        public float totalDamageTaken;
        public int bestCombo;
        public DateTime lastFightTime;
    }

    /// <summary>
    /// Boss 机制系统数据
    /// </summary>
    [System.Serializable]
    public class BossMechanicsData {
        public Dictionary<string, List<BossPhaseConfig>> bossPhases = new Dictionary<string, List<BossPhaseConfig>>();
        public Dictionary<string, List<EnrageConfig>> bossEnrages = new Dictionary<string, List<EnrageConfig>>();
        public Dictionary<string, List<BossSpecialMechanic>> bossSpecialMechanics = new Dictionary<string, List<BossSpecialMechanic>>();
        public Dictionary<string, PlayerBossRecord> playerRecords = new Dictionary<string, PlayerBossRecord>();
    }

    /// <summary>
    /// 当前 Boss 战斗状态
    /// </summary>
    public class ActiveBossFight {
        public string bossId;
        public string bossName;
        public float maxHealth;
        public float currentHealth;
        public int currentPhase;
        public float timeInCombat;
        public float totalDamageDealt;
        public float totalDamageTaken;
        public int currentCombo;
        public bool isEnraged;
        public bool isInvincible;
        public List<string> activeEffects = new List<string>();
        public Dictionary<string, float> mechanicCooldowns = new Dictionary<string, float>();
    }
}
