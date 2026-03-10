using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// 队伍技能系统
/// 队伍共享技能、团队增益、协同效果
/// </summary>
public class TeamSkillSystem : Node
{
    public static TeamSkillSystem Instance { get; private set; }

    // 队伍技能类型
    public enum TeamSkillType
    {
        HealingRain,      // 治疗雨 - 全队持续回复
        ShieldWall,       // 护盾墙 - 全队护盾
        DamageAura,       // 伤害光环 - 全队增伤
        DefenseAura,      // 防御光环 - 全队减伤
        SpeedAura,        // 速度光环 - 全队加速
        ManaRegen,        // 法力回复 - 全队回蓝
        CritAura,         // 暴击光环 - 全队暴击率
        LifeSteal,        // 生命偷取 - 攻击吸血
        Invincibility,    // 无敌 - 短暂无敌
        Resurrection,     // 复活 - 复活死亡队友
        ElementalResist,  // 元素抗性 - 元素伤害减免
        ExpBoost,         // 经验加成 - 经验获取提升
        LootBoost,        // 掉落加成 - 掉落率提升
    }

    // 队伍技能数据
    public class TeamSkill
    {
        public TeamSkillType Type;
        public string Name;
        public string Description;
        public float Cooldown;        // 冷却时间(秒)
        public float Duration;        // 持续时间(秒)
        public float Value;           // 效果值
        public int RequiredMembers;   // 所需成员数
        public float Range;           // 技能范围
        public float CurrentCooldown; // 当前冷却
        public bool IsActive;         // 是否激活

        public TeamSkill(TeamSkillType type, string name, string desc, float cooldown, float duration, float value, int members, float range)
        {
            Type = type;
            Name = name;
            Description = desc;
            Cooldown = cooldown;
            Duration = duration;
            Value = value;
            RequiredMembers = members;
            Range = range;
            CurrentCooldown = 0;
            IsActive = false;
        }
    }

    // 活跃技能效果
    public class ActiveSkillEffect
    {
        public TeamSkill Skill;
        public float TimeRemaining;
        public List<int> AffectedPlayers = new List<int>();
    }

    // 信号
    public delegate void SkillActivatedEvent(TeamSkill skill);
    public delegate void SkillExpiredEvent(TeamSkill skill);
    public delegate void SkillUsedEvent(TeamSkill skill, int userId);
    public delegate void TeamBuffAppliedEvent(TeamSkillType type, float value);

    public event SkillActivatedEvent OnSkillActivated;
    public event SkillExpiredEvent OnSkillExpired;
    public event SkillUsedEvent OnSkillUsed;
    public event TeamBuffAppliedEvent OnTeamBuffApplied;

    // 状态
    private List<TeamSkill> _teamSkills = new List<TeamSkill>();
    private List<ActiveSkillEffect> _activeEffects = new List<ActiveSkillEffect>();
    private int _localPlayerId = -1;
    private float _deltaTime = 0;

    public override void _Ready()
    {
        Instance = this;
        InitializeSkills();
    }

    /// <summary>
    /// 初始化队伍技能
    /// </summary>
    private void InitializeSkills()
    {
        _teamSkills.Add(new TeamSkill(
            TeamSkillType.HealingRain, "治疗雨",
            "召唤治疗雨，每秒回复范围内队友生命",
            60f, 15f, 10f, 2, 400f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.ShieldWall, "护盾墙",
            "为所有队友施加护盾，吸收伤害",
            90f, 20f, 50f, 2, 350f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.DamageAura, "攻击光环",
            "增加全队攻击力",
            120f, 30f, 0.15f, 2, 500f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.DefenseAura, "防御光环",
            "减少全队受到的伤害",
            120f, 30f, 0.2f, 2, 500f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.SpeedAura, "速度光环",
            "增加全队移动速度",
            90f, 25f, 0.25f, 2, 450f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.ManaRegen, "法力之泉",
            "快速回复全队法力值",
            75f, 12f, 5f, 2, 400f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.CritAura, "暴击光环",
            "增加全队暴击率",
            100f, 25f, 0.1f, 3, 450f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.LifeSteal, "生命偷取",
            "攻击时获得生命偷取效果",
            80f, 20f, 0.08f, 3, 400f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.Invincibility, "无敌时刻",
            "全队进入短暂无敌状态",
            180f, 5f, 0f, 3, 500f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.Resurrection, "复活术",
            "复活距离最近的死亡队友",
            200f, 0f, 1f, 2, 500f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.ElementalResist, "元素护盾",
            "增加全队元素抗性",
            100f, 30f, 0.25f, 2, 450f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.ExpBoost, "经验加成",
            "增加全队获取的经验值",
            150f, 45f, 0.3f, 2, 500f));

        _teamSkills.Add(new TeamSkill(
            TeamSkillType.LootBoost, "掉落加成",
            "增加敌人掉落物品几率",
            180f, 40f, 0.2f, 3, 500f));
    }

    /// <summary>
    /// 更新技能冷却和效果
    /// </summary>
    public override void _Process(float delta)
    {
        _deltaTime = delta;
        UpdateCooldowns(delta);
        UpdateActiveEffects(delta);
    }

    /// <summary>
    /// 更新冷却时间
    /// </summary>
    private void UpdateCooldowns(float delta)
    {
        foreach (var skill in _teamSkills)
        {
            if (skill.CurrentCooldown > 0)
            {
                skill.CurrentCooldown -= delta;
                if (skill.CurrentCooldown < 0) skill.CurrentCooldown = 0;
            }
        }
    }

    /// <summary>
    /// 更新活跃效果
    /// </summary>
    private void UpdateActiveEffects(float delta)
    {
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            var effect = _activeEffects[i];
            effect.TimeRemaining -= delta;

            // 周期性效果
            if (effect.Skill.Type == TeamSkillType.HealingRain)
            {
                ApplyHealingRain(effect, delta);
            }
            else if (effect.Skill.Type == TeamSkillType.ManaRegen)
            {
                ApplyManaRegen(effect, delta);
            }

            if (effect.TimeRemaining <= 0)
            {
                OnSkillExpired?.Invoke(effect.Skill);
                _activeEffects.RemoveAt(i);
            }
        }
    }

    /// <summary>
    /// 应用治疗雨效果
    /// </summary>
    private void ApplyHealingRain(ActiveSkillEffect effect, float delta)
    {
        if (!TeamSystem.Instance.IsInTeam) return;
        
        var team = TeamSystem.Instance.CurrentTeam;
        if (team == null) return;

        float healAmount = effect.Skill.Value * delta;
        foreach (var member in team.Members)
        {
            if (IsPlayerInRange(member.PlayerId, effect.Skill.Range))
            {
                ApplyHealToPlayer(member.PlayerId, healAmount);
            }
        }
    }

    /// <summary>
    /// 应用法力回复效果
    /// </summary>
    private void ApplyManaRegen(ActiveSkillEffect effect, float delta)
    {
        if (!TeamSystem.Instance.IsInTeam) return;
        
        var team = TeamSystem.Instance.CurrentTeam;
        if (team == null) return;

        float manaAmount = effect.Skill.Value * delta;
        foreach (var member in team.Members)
        {
            if (IsPlayerInRange(member.PlayerId, effect.Skill.Range))
            {
                ApplyManaToPlayer(member.PlayerId, manaAmount);
            }
        }
    }

    /// <summary>
    /// 使用队伍技能
    /// </summary>
    public bool UseSkill(TeamSkillType type)
    {
        var skill = _teamSkills.Find(s => s.Type == type);
        if (skill == null) return false;

        if (skill.CurrentCooldown > 0) return false;

        if (!TeamSystem.Instance.IsInTeam) return false;

        var team = TeamSystem.Instance.CurrentTeam;
        if (team == null || team.Members.Count < skill.RequiredMembers) return false;

        _localPlayerId = MultiplayerManager.Instance.LocalPlayerId;

        // 创建活跃效果
        var effect = new ActiveSkillEffect
        {
            Skill = skill,
            TimeRemaining = skill.Duration,
            AffectedPlayers = new List<int>()
        };

        foreach (var member in team.Members)
        {
            if (IsPlayerInRange(member.PlayerId, skill.Range))
            {
                effect.AffectedPlayers.Add(member.PlayerId);
                ApplySkillEffect(skill, member.PlayerId);
            }
        }

        skill.CurrentCooldown = skill.Cooldown;
        skill.IsActive = true;
        _activeEffects.Add(effect);

        OnSkillActivated?.Invoke(skill);
        OnSkillUsed?.Invoke(skill, _localPlayerId);

        return true;
    }

    /// <summary>
    /// 应用技能效果到玩家
    /// </summary>
    private void ApplySkillEffect(TeamSkill skill, int playerId)
    {
        switch (skill.Type)
        {
            case TeamSkillType.ShieldWall:
                ApplyShield(playerId, skill.Value);
                OnTeamBuffApplied?.Invoke(skill.Type, skill.Value);
                break;

            case TeamSkillType.DamageAura:
                ApplyDamageBuff(playerId, skill.Value);
                OnTeamBuffApplied?.Invoke(skill.Type, skill.Value);
                break;

            case TeamSkillType.DefenseAura:
                ApplyDefenseBuff(playerId, skill.Value);
                OnTeamBuffApplied?.Invoke(skill.Type, skill.Value);
                break;

            case TeamSkillType.SpeedAura:
                ApplySpeedBuff(playerId, skill.Value);
                OnTeamBuffApplied?.Invoke(skill.Type, skill.Value);
                break;

            case TeamSkillType.CritAura:
                ApplyCritBuff(playerId, skill.Value);
                OnTeamBuffApplied?.Invoke(skill.Type, skill.Value);
                break;

            case TeamSkillType.LifeSteal:
                ApplyLifeSteal(playerId, skill.Value);
                OnTeamBuffApplied?.Invoke(skill.Type, skill.Value);
                break;

            case TeamSkillType.Invincibility:
                ApplyInvincibility(playerId, skill.Duration);
                OnTeamBuffApplied?.Invoke(skill.Type, 0);
                break;

            case TeamSkillType.ElementalResist:
                ApplyElementalResist(playerId, skill.Value);
                OnTeamBuffApplied?.Invoke(skill.Type, skill.Value);
                break;

            case TeamSkillType.ExpBoost:
                ApplyExpBoost(playerId, skill.Value);
                OnTeamBuffApplied?.Invoke(skill.Type, skill.Value);
                break;

            case TeamSkillType.LootBoost:
                ApplyLootBoost(playerId, skill.Value);
                OnTeamBuffApplied?.Invoke(skill.Type, skill.Value);
                break;

            case TeamSkillType.Resurrection:
                ResurrectPlayer(playerId);
                break;
        }
    }

    // 效果应用方法 - 需要与Player类集成
    private void ApplyHealToPlayer(int playerId, float amount) { /* 调用Player方法 */ }
    private void ApplyManaToPlayer(int playerId, float amount) { /* 调用Player方法 */ }
    private void ApplyShield(int playerId, float amount) { /* 调用Player方法 */ }
    private void ApplyDamageBuff(int playerId, float percent) { /* 调用Player方法 */ }
    private void ApplyDefenseBuff(int playerId, float percent) { /* 调用Player方法 */ }
    private void ApplySpeedBuff(int playerId, float percent) { /* 调用Player方法 */ }
    private void ApplyCritBuff(int playerId, float percent) { /* 调用Player方法 */ }
    private void ApplyLifeSteal(int playerId, float percent) { /* 调用Player方法 */ }
    private void ApplyInvincibility(int playerId, float duration) { /* 调用Player方法 */ }
    private void ApplyElementalResist(int playerId, float percent) { /* 调用Player方法 */ }
    private void ApplyExpBoost(int playerId, float percent) { /* 调用Player方法 */ }
    private void ApplyLootBoost(int playerId, float percent) { /* 调用Player方法 */ }
    private void ResurrectPlayer(int playerId) { /* 调用Player方法 */ }

    /// <summary>
    /// 检查玩家是否在技能范围内
    /// </summary>
    private bool IsPlayerInRange(int playerId, float range)
    {
        // 实现距离检测逻辑
        return true;
    }

    /// <summary>
    /// 获取可用技能列表
    /// </summary>
    public List<TeamSkill> GetAvailableSkills()
    {
        return _teamSkills.FindAll(s => s.CurrentCooldown <= 0);
    }

    /// <summary>
    /// 获取所有技能状态
    /// </summary>
    public List<TeamSkill> GetAllSkills()
    {
        return _teamSkills;
    }

    /// <summary>
    /// 获取活跃效果
    /// </summary>
    public List<ActiveSkillEffect> GetActiveEffects()
    {
        return _activeEffects;
    }

    /// <summary>
    /// 获取技能冷却信息
    /// </summary>
    public float GetSkillCooldown(TeamSkillType type)
    {
        var skill = _teamSkills.Find(s => s.Type == type);
        return skill?.CurrentCooldown ?? 0;
    }

    /// <summary>
    /// 检查是否可以激活技能
    /// </summary>
    public bool CanActivateSkill(TeamSkillType type)
    {
        var skill = _teamSkills.Find(s => s.Type == type);
        if (skill == null) return false;

        if (skill.CurrentCooldown > 0) return false;
        if (!TeamSystem.Instance.IsInTeam) return false;

        var team = TeamSystem.Instance.CurrentTeam;
        return team != null && team.Members.Count >= skill.RequiredMembers;
    }
}
