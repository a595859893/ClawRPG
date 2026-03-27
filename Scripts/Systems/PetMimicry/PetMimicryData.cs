using System;

namespace ClawRPG.Scripts.Systems.PetMimicry
{
    /// <summary>
    /// 房间环境类型标签 — 支持多标签组合（flags）
    /// 用于宠物行为印记系统：宠物根据房间环境类型记录主人的行为
    /// </summary>
    [Flags]
    public enum RoomEnvironmentType
    {
        /// <summary>无特殊环境（默认普通房间）</summary>
        None = 0,

        /// <summary>火系环境 — 厨房、火源、熔岩等</summary>
        Fire = 1 << 0,

        /// <summary>冰系环境 — 冰霜、寒冷区域</summary>
        Ice = 1 << 1,

        /// <summary>陷阱密集区 — 机关密集的房间</summary>
        TrapDense = 1 << 2,

        /// <summary>Boss房间</summary>
        Boss = 1 << 3,

        /// <summary>撤退/逃生通道区</summary>
        Escape = 1 << 4,

        /// <summary>宝藏房间 — 高价值战利品区</summary>
        Treasure = 1 << 5,

        /// <summary>休息/恢复区 — 篝火、治疗泉</summary>
        Rest = 1 << 6,

        /// <summary>谜题房间</summary>
        Puzzle = 1 << 7,

        /// <summary>精英敌人区域</summary>
        Elite = 1 << 8,

        /// <summary>普通战斗区（无特殊环境）</summary>
        Combat = 1 << 9,

        /// <summary>入口/出生区域</summary>
        Entrance = 1 << 10,

        /// <summary>毒系环境</summary>
        Poison = 1 << 11,

        /// <summary>电系/机械环境</summary>
        Electric = 1 << 12,

        /// <summary>暗系/虚空环境</summary>
        Shadow = 1 << 13,

        /// <summary>神圣环境</summary>
        Holy = 1 << 14,

        /// <summary>自然/藤蔓环境</summary>
        Nature = 1 << 15
    }

    /// <summary>
    /// 玩家行为类型 — 宠物记录的主人在房间内的行为
    /// </summary>
    public enum PlayerBehaviorType
    {
        /// <summary>使用火系技能/法术</summary>
        UseFireSkill,

        /// <summary>使用冰系技能/法术</summary>
        UseIceSkill,

        /// <summary>使用电系技能</summary>
        UseElectricSkill,

        /// <summary>使用暗系技能</summary>
        UseShadowSkill,

        /// <summary>使用神圣技能</summary>
        UseHolySkill,

        /// <summary>使用自然/藤蔓技能</summary>
        UseNatureSkill,

        /// <summary>高频率位移/闪避</summary>
        FrequentDodge,

        /// <summary>积极进攻（高攻击频率）</summary>
        AggressiveAttack,

        /// <summary>防守姿态（高防御/护盾）</summary>
        DefensiveStance,

        /// <summary>低血量时激进出击（背水一战）</summary>
        LowHPAggression,

        /// <summary>快速撤退/脱离战斗</summary>
        QuickRetreat,

        /// <summary>优先击杀精英/Boss</summary>
        FocusElite,

        /// <summary>绕路避开敌人</summary>
        AvoidCombat,

        /// <summary>触发陷阱</summary>
        TriggerTrap,

        /// <summary>解决谜题</summary>
        SolvePuzzle,

        /// <summary>收集战利品</summary>
        CollectLoot,

        /// <summary>使用恢复/治疗</summary>
        UseHealing,

        /// <summary>宠物协战</summary>
        PetSynergy,

        /// <summary>触发特殊互动</summary>
        SpecialInteraction
    }

    /// <summary>
    /// 行为印记记录 — 将环境类型与行为关联
    /// </summary>
    public class BehaviorImprint
    {
        public RoomEnvironmentType EnvironmentType { get; set; }
        public PlayerBehaviorType BehaviorType { get; set; }
        public int ImprintLevel { get; set; }       // 0-5 模仿等级
        public float Xp { get; set; }
        public DateTime LastRecordedAt { get; set; }
        public int TotalTriggers { get; set; }

        public float GetXpForNextLevel()
        {
            // 对数曲线：等级越高所需XP越多
            return 10f * Mathf.Pow(2f, ImprintLevel);
        }

        public bool AddXp(float amount)
        {
            Xp += amount;
            if (Xp >= GetXpForNextLevel() && ImprintLevel < 5)
            {
                ImprintLevel++;
                Xp = 0f;
                return true; // 升级了
            }
            return false;
        }
    }
}
