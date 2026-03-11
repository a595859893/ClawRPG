using Godot;
using System;

namespace ClawRPG.Scripts.Systems.Pets
{
    /// <summary>
    /// 宠物技能数据结构
    /// </summary>
    [GlobalClass]
    public partial class PetSkill : Resource
    {
        [Export] public string SkillId { get; set; } = "";
        [Export] public string SkillName { get; set; } = "";
        [Export] public string Description { get; set; } = "";
        [Export] public PetSkillType Type { get; set; } = PetSkillType.Attack;
        [Export] public PetSkillTarget Target { get; set; } = PetSkillTarget.Enemy;
        [Export] public Texture2D Icon { get; set; }
        
        // 技能属性
        [Export] public int Damage { get; set; } = 0;
        [Export] public float DamageMultiplier { get; set; } = 0f; // 基于宠物攻击的百分比
        [Export] public int HealAmount { get; set; } = 0;
        [Export] public float HealPercent { get; set; } = 0f; // 基于宠物最大生命的百分比
        [Export] public int ShieldAmount { get; set; } = 0;
        
        // 效果
        [Export] public float SlowAmount { get; set; } = 0f; // 减速百分比
        [Export] public float StunDuration { get; set; } = 0f; // 眩晕时长
        [Export] public float BurnDamage { get; set; } = 0f; // 燃烧伤害
        [Export] public float FreezeDuration { get; set; } = 0f; // 冰冻时长
        
        // 冷却和消耗
        [Export] public float Cooldown { get; set; } = 5f;
        [Export] public int ManaCost { get; set; } = 0;
        
        // 范围
        [Export] public float Range { get; set; } = 100f;
        [Export] public float AoeRadius { get; set; } = 0f;
        
        // 解锁
        [Export] public int RequiredLevel { get; set; } = 1;
        [Export] public int SkillPointCost { get; set; } = 1;
        
        // 稀有度
        [Export] public PetSkillRarity Rarity { get; set; } = PetSkillRarity.Common;
    }

    public enum PetSkillType
    {
        Attack,     // 攻击技能
        Defense,    // 防御技能
        Support,    // 辅助技能
        Heal,       // 治疗技能
        Debuff      // 减益技能
    }

    public enum PetSkillTarget
    {
        Enemy,          // 单体敌人
        EnemyAoe,       // 敌人AOE
        Self,           // 自身
        Player,         // 玩家
        PlayerAoe,      // 玩家AOE
        Ally            // 友方
    }

    public enum PetSkillRarity
    {
        Common,     // 普通 - 白色
        Uncommon,   // 优秀 - 绿色
        Rare,       // 稀有 - 蓝色
        Epic,       // 史诗 - 紫色
        Legendary   // 传说 - 橙色
    }

    /// <summary>
    /// 宠物已学习的技能实例
    /// </summary>
    public partial class LearnedPetSkill : Resource
    {
        [Export] public string SkillId { get; set; } = "";
        [Export] public int CurrentLevel { get; set; } = 1;
        [Export] public float CurrentCooldown { get; set; } = 0f;
        [Export] public int TimesUsed { get; set; } = 0;
    }

    /// <summary>
    /// 宠物技能数据
    /// </summary>
    public class PetSkillData
    {
        public Dictionary<string, int> LearnedSkills { get; set; } = new Dictionary<string, int>(); // SkillId -> Level
        public int AvailableSkillPoints { get; set; } = 0;
        public Dictionary<string, LearnedPetSkill> SkillInstances { get; set; } = new Dictionary<string, LearnedPetSkill>();
    }
}
