using Godot;
using System;
using System.Collections.Generic;

namespace ClawRPG.Scripts.Mounts {
    /// <summary>
    /// 坐骑进化阶段
    /// </summary>
    public enum MountEvolutionStage {
        Basic,      // 基础
        Advanced,   // 进阶
        Elite,      // 精英
        Epic,       // 史诗
        Legendary   // 传说
    }

    /// <summary>
    /// 坐骑进化类型
    /// </summary>
    public enum MountEvolutionType {
        Fire,       // 火焰
        Ice,        // 冰霜
        Lightning,  // 闪电
        Dark,       // 黑暗
        Holy,       // 神圣
        Nature      // 自然
    }

    /// <summary>
    /// 坐骑进化链
    /// </summary>
    public enum MountEvolutionChain {
        Horse,      // 马
        Wolf,       // 狼
        Bear,       // 熊
        Eagle,      // 鹰
        Dragon,     // 龙
        Phoenix,    // 凤凰
        Griffin,    // 狮鹫
        Unicorn     // 独角兽
    }

    /// <summary>
    /// 坐骑进化阶段配置
    /// </summary>
    [System.Serializable]
    public class EvolutionStageConfig {
        public MountEvolutionStage Stage;
        public string StageName;
        public int RequiredLevel;
        public int RequiredExp;
        public int RequiredItems;  // 进化所需材料数量
        public float HealthBonus;      // 生命加成 %
        public float AttackBonus;      // 攻击加成 %
        public float DefenseBonus;     // 防御加成 %
        public float SpeedBonus;      // 速度加成 %
        public float CritRateBonus;   // 暴击率加成 %
        public float CritDamageBonus;  // 暴击伤害加成 %
    }

    /// <summary>
    /// 坐骑进化类型配置
    /// </summary>
    [System.Serializable]
    public class EvolutionTypeConfig {
        public MountEvolutionType Type;
        public string TypeName;
        public string Description;
        public Color ElementColor;
        public float FireResist;
        public float IceResist;
        public float LightningResist;
        public float DarkResist;
        public float HolyResist;
    }

    /// <summary>
    /// 坐骑进化链配置
    /// </summary>
    [System.Serializable]
    public class EvolutionChainConfig {
        public MountEvolutionChain Chain;
        public string ChainName;
        public string BaseMountId;
        public List<string> EvolutionPaths;  // 可进化的形态ID列表
    }

    /// <summary>
    /// 坐骑进化实例
    /// </summary>
    [System.Serializable]
    public class MountEvolutionInstance {
        public string MountId;
        public MountEvolutionStage CurrentStage;
        public MountEvolutionType CurrentType;
        public MountEvolutionChain EvolutionChain;
        public int TotalEvolutions;    // 总进化次数
        public int BattleExp;          // 战斗经验
        public List<string> UnlockedSkills = new List<string>();
        
        // 进化属性加成
        public float TotalHealthBonus;
        public float TotalAttackBonus;
        public float TotalDefenseBonus;
        public float TotalSpeedBonus;
        public float TotalCritRateBonus;
        public float TotalCritDamageBonus;
    }

    /// <summary>
    /// 坐骑进化结果
    /// </summary>
    public enum EvolutionResult {
        Success,
        Failed,
        MaxStage,
        InsufficientLevel,
        InsufficientExp,
        InsufficientItems,
        MaxType
    }

    /// <summary>
    /// 玩家坐骑进化数据
    /// </summary>
    [System.Serializable]
    public class PlayerMountEvolutionData {
        public Dictionary<string, MountEvolutionInstance> MountEvolutions = new Dictionary<string, MountEvolutionInstance>();
        public int TotalEvolutions;
        public int TotalBattleExp;
        public Dictionary<MountEvolutionStage, int> StageCount = new Dictionary<MountEvolutionStage, int>();
        public Dictionary<MountEvolutionType, int> TypeCount = new Dictionary<MountEvolutionType, int>();
    }
}
