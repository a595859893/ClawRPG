using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Combo Skill Database - 连击技能配置数据库
/// </summary>
public class ComboSkillSystem : BaseSystem
{
    // 单例
    private static ComboSkillSystem instance;

    // 枚举定义 (与 ComboSkillSystem 共享)
    public enum ComboType
    {
        Sequential,
        Parallel,
        Chain,
        Conditional
    }

    public enum TriggerCondition
    {
        OnHit,
        OnCritical,
        OnKill,
        OnDamageTaken,
        OnHealthBelow,
        OnManaBelow,
        OnEnemyType,
        OnComboComplete,
        Manual,
        Cooldown
    }

    public enum EffectType
    {
        Damage,
        Heal,
        Shield,
        Buff,
        Debuff,
        Teleport,
        Summon,
        Transform,
        ClearDebuffs,
        GrantInvulnerability
    }

    // 内部类定义 (与 ComboSkillSystem 共享)
    public class ComboSkillEffect
    {
        public EffectType effectType;
        public float value;
        public float duration = 0f;
        public string description = "";
        public string target = "enemy";
    }

    public class ComboStep
    {
        public string skillId = "";
        public float delay = 0f;
        public TriggerCondition condition = TriggerCondition.Manual;
        public float conditionValue = 0f;
        public List<ComboSkillEffect> effects = new List<ComboSkillEffect>();
    }

    public class ComboSkill
    {
        public string id = "";
        public string name = "";
        public string description = "";
        public ComboType comboType;
        public List<ComboStep> steps = new List<ComboStep>();
        public float totalTime = 0f;
        public float cooldown = 0f;
        public float manaCost = 0f;
        public int levelRequired = 1;
        public int rarity = 0;
    }

    // 数据库
    private Dictionary<string, ComboSkill> combos = new Dictionary<string, ComboSkill>();

    protected override void Initialize()
    {
        base.Initialize();
        instance = this;
        InitCombos();
    }

    public static ComboSkillSystem GetInstance()
    {
        return instance;
    }

    public ComboSkill GetCombo(string comboId)
    {
        return combos.ContainsKey(comboId) ? combos[comboId] : null;
    }

    public List<ComboSkill> GetAllCombos()
    {
        return new List<ComboSkill>(combos.Values);
    }

    public List<ComboSkill> GetCombosByType(ComboType comboType)
    {
        var result = new List<ComboSkill>();
        foreach (var combo in combos.Values)
        {
            if (combo.comboType == comboType)
                result.Add(combo);
        }
        return result;
    }

    public List<ComboSkill> GetCombosByRarity(int rarity)
    {
        var result = new List<ComboSkill>();
        foreach (var combo in combos.Values)
        {
            if (combo.rarity == rarity)
                result.Add(combo);
        }
        return result;
    }

    public List<ComboSkill> GetAvailableCombos(int playerLevel)
    {
        var result = new List<ComboSkill>();
        foreach (var combo in combos.Values)
        {
            if (combo.levelRequired <= playerLevel)
                result.Add(combo);
        }
        return result;
    }

    public Color GetRarityColor(int rarity)
    {
        switch (rarity)
        {
            case 0: return Colors.White;
            case 1: return Colors.Green;
            case 2: return new Color(0.3f, 0.5f, 1.0f);
            case 3: return new Color(0.6f, 0.2f, 0.8f);
            case 4: return new Color(1.0f, 0.6f, 0.0f);
        }
        return Colors.White;
    }

    public string GetRarityName(int rarity)
    {
        switch (rarity)
        {
            case 0: return "普通";
            case 1: return "优秀";
            case 2: return "稀有";
            case 3: return "史诗";
            case 4: return "传说";
        }
        return "未知";
    }

    private void InitCombos()
    {
        InitSequentialCombos();
        InitChainCombos();
        InitParallelCombos();
        InitConditionalCombos();
    }

    private void InitSequentialCombos()
    {
        // 闪电连击 - 顺序触发
        var combo1 = new ComboSkill
        {
            id = "combo_lightning",
            name = "闪电连击",
            description = "召唤三道闪电依次打击敌人",
            comboType = ComboType.Sequential,
            cooldown = 8.0f,
            manaCost = 30.0f,
            levelRequired = 5,
            rarity = 1,
            steps = new List<ComboStep>
            {
                CreateStep("lightning_bolt", 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 50.0f, "闪电打击 50 伤害") }),
                CreateStep("lightning_bolt", 0.5f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 50.0f, "闪电打击 50 伤害") }),
                CreateStep("lightning_bolt", 1.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 75.0f, "终结闪电 75 伤害") })
            },
            totalTime = 2.0f
        };
        combos[combo1.id] = combo1;

        // 治疗链 - 顺序治疗
        var combo2 = new ComboSkill
        {
            id = "combo_healing_chain",
            name = "治疗链",
            description = "依次治疗目标三次",
            comboType = ComboType.Sequential,
            cooldown = 12.0f,
            manaCost = 40.0f,
            levelRequired = 8,
            rarity = 1,
            steps = new List<ComboStep>
            {
                CreateStep("heal", 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Heal, 30.0f, "治疗 30 HP") }),
                CreateStep("heal", 0.8f, new List<ComboSkillEffect> { CreateEffect(EffectType.Heal, 30.0f, "治疗 30 HP") }),
                CreateStep("heal", 1.6f, new List<ComboSkillEffect> { CreateEffect(EffectType.Heal, 50.0f, "强力治疗 50 HP") })
            },
            totalTime = 2.5f
        };
        combos[combo2.id] = combo2;

        // 火焰风暴
        var combo3 = new ComboSkill
        {
            id = "combo_fire_storm",
            name = "火焰风暴",
            description = "召唤火焰陨石轰炸区域",
            comboType = ComboType.Sequential,
            cooldown = 20.0f,
            manaCost = 80.0f,
            levelRequired = 20,
            rarity = 3,
            steps = new List<ComboStep>
            {
                CreateStep("fire_meteor", 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 100.0f, "陨石 100 伤害") }),
                CreateStep("fire_meteor", 0.6f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 100.0f, "陨石 100 伤害") }),
                CreateStep("fire_meteor", 1.2f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 100.0f, "陨石 100 伤害") }),
                CreateStep("fire_explosion", 2.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 150.0f, "爆炸 150 伤害") })
            },
            totalTime = 3.0f
        };
        combos[combo3.id] = combo3;
    }

    private void InitChainCombos()
    {
        // 暗影打击 - 链式触发
        var combo1 = new ComboSkill
        {
            id = "combo_shadow_strike",
            name = "暗影打击",
            description = "穿梭于阴影中连续攻击",
            comboType = ComboType.Chain,
            cooldown = 10.0f,
            manaCost = 35.0f,
            levelRequired = 12,
            rarity = 2,
            steps = new List<ComboStep>
            {
                CreateStep("shadow_strike", 0.0f, TriggerCondition.Manual, 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 40.0f, "暗影斩 40 伤害") }),
                CreateStep("shadow_strike", 0.3f, TriggerCondition.OnHit, 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 50.0f, "穿刺 50 伤害") }),
                CreateStep("shadow_strike", 0.3f, TriggerCondition.OnHit, 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 70.0f, "终结 70 伤害") })
            },
            totalTime = 2.0f
        };
        combos[combo1.id] = combo1;

        // 冰火两重天
        var combo2 = new ComboSkill
        {
            id = "combo_fire_ice",
            name = "冰火两重天",
            description = "冰霜后接火焰，造成额外伤害",
            comboType = ComboType.Chain,
            cooldown = 15.0f,
            manaCost = 50.0f,
            levelRequired = 15,
            rarity = 2,
            steps = new List<ComboStep>
            {
                CreateStep("ice_burst", 0.0f, TriggerCondition.Manual, 0.0f, new List<ComboSkillEffect> { 
                    CreateEffect(EffectType.Damage, 60.0f, "冰霜 60 伤害"),
                    CreateEffect(EffectType.Debuff, 30.0f, "减速 30%", 3.0f)
                }),
                CreateStep("fire_burst", 0.5f, TriggerCondition.OnHit, 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 80.0f, "火焰 80 伤害") })
            },
            totalTime = 1.5f
        };
        combos[combo2.id] = combo2;
    }

    private void InitParallelCombos()
    {
        // 全屏护盾 - 并行效果
        var combo1 = new ComboSkill
        {
            id = "combo_shield_wall",
            name = "护盾壁垒",
            description = "同时施加多重护盾",
            comboType = ComboType.Parallel,
            cooldown = 25.0f,
            manaCost = 60.0f,
            levelRequired = 10,
            rarity = 2,
            steps = new List<ComboStep>
            {
                CreateStep("shield", 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Shield, 100.0f, "护盾 100") }),
                CreateStep("buff", 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Buff, 20.0f, "防御强化 20%", 10.0f) }),
                CreateStep("cleanse", 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.ClearDebuffs, 1.0f, "清除减益") })
            },
            totalTime = 0.5f
        };
        combos[combo1.id] = combo1;

        // 元素爆发
        var combo2 = new ComboSkill
        {
            id = "combo_elemental_burst",
            name = "元素爆发",
            description = "同时触发所有元素之力",
            comboType = ComboType.Parallel,
            cooldown = 30.0f,
            manaCost = 100.0f,
            levelRequired = 25,
            rarity = 4,
            steps = new List<ComboStep>
            {
                CreateStep("fire", 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 120.0f, "火 120 伤害") }),
                CreateStep("ice", 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 100.0f, "冰 100 伤害") }),
                CreateStep("lightning", 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 80.0f, "雷 80 伤害") })
            },
            totalTime = 0.2f
        };
        combos[combo2.id] = combo2;
    }

    private void InitConditionalCombos()
    {
        // 绝地反击 - 条件触发
        var combo1 = new ComboSkill
        {
            id = "combo_desperation",
            name = "绝地反击",
            description = "生命低于30%时触发强力反击",
            comboType = ComboType.Conditional,
            cooldown = 45.0f,
            manaCost = 0.0f,
            levelRequired = 8,
            rarity = 2,
            steps = new List<ComboStep>
            {
                CreateStep("desperation_strike", 0.0f, TriggerCondition.OnHealthBelow, 30.0f, new List<ComboSkillEffect> { 
                    CreateEffect(EffectType.Damage, 150.0f, "反击 150 伤害"),
                    CreateEffect(EffectType.Heal, 50.0f, "吸血 50 HP")
                })
            },
            totalTime = 0.5f
        };
        combos[combo1.id] = combo1;

        // 暴击盛宴
        var combo2 = new ComboSkill
        {
            id = "combo_critical_feast",
            name = "暴击盛宴",
            description = "暴击时触发连击",
            comboType = ComboType.Conditional,
            cooldown = 20.0f,
            manaCost = 25.0f,
            levelRequired = 15,
            rarity = 3,
            steps = new List<ComboStep>
            {
                CreateStep("critical_strike", 0.0f, TriggerCondition.OnCritical, 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 100.0f, "暴击 100 伤害") }),
                CreateStep("follow_up", 0.2f, TriggerCondition.OnHit, 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 80.0f, "追击 80 伤害") })
            },
            totalTime = 1.0f
        };
        combos[combo2.id] = combo2;

        // 凤凰涅槃
        var combo3 = new ComboSkill
        {
            id = "combo_phoenix",
            name = "凤凰涅槃",
            description = "死亡时复活并造成巨额伤害",
            comboType = ComboType.Conditional,
            cooldown = 120.0f,
            manaCost = 0.0f,
            levelRequired = 30,
            rarity = 4,
            steps = new List<ComboStep>
            {
                CreateStep("resurrection", 0.0f, TriggerCondition.OnHealthBelow, 0.0f, new List<ComboSkillEffect> { 
                    CreateEffect(EffectType.Heal, 100.0f, "复活并恢复 100 HP"),
                    CreateEffect(EffectType.GrantInvulnerability, 1.0f, "无敌 3秒", 3.0f)
                }),
                CreateStep("rebirth_damage", 0.5f, TriggerCondition.Manual, 0.0f, new List<ComboSkillEffect> { CreateEffect(EffectType.Damage, 200.0f, "涅槃之火 200 伤害") })
            },
            totalTime = 2.0f
        };
        combos[combo3.id] = combo3;
    }

    // 辅助函数
    private ComboStep CreateStep(string skillId, float delay, List<ComboSkillEffect> effects)
    {
        return CreateStep(skillId, delay, TriggerCondition.Manual, 0f, effects);
    }

    private ComboStep CreateStep(string skillId, float delay, TriggerCondition condition, float condValue, List<ComboSkillEffect> effects)
    {
        return new ComboStep
        {
            skillId = skillId,
            delay = delay,
            condition = condition,
            conditionValue = condValue,
            effects = effects
        };
    }

    private ComboSkillEffect CreateEffect(EffectType effectType, float value, string desc, float duration = 0f)
    {
        return new ComboSkillEffect
        {
            effectType = effectType,
            value = value,
            description = desc,
            duration = duration
        };
    }
    
    /// <summary>
    /// 导出保存数据
    /// </summary>
    public override Dictionary ExportSaveData() {
        var data = new Dictionary();
        // ComboSkillSystem 是静态配置数据库，无运行时持久化状态
        return data;
    }

    /// <summary>
    /// 导入保存数据
    /// </summary>
    public override void ImportSaveData(Dictionary data) {
        if (data == null) return;
        // ComboSkillSystem 是静态配置数据库，无运行时持久化状态
    }
}
