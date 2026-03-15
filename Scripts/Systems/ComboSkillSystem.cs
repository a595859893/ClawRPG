using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Combo Skill System - 连击技能系统核心
/// </summary>
public class ComboSkillSystem : BaseSystem
{
    // 信号定义 (C# 事件)
    public event Action<string> OnComboExecuted;
    public event Action<string> OnComboCompleted;
    public event Action<string, int> OnComboStepTriggered;
    public event Action<string> OnComboCancelled;
    public event Action<string, float> OnCooldownUpdated;
    public event Action<string> OnComboUnlocked;

    // 枚举定义
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

    // 内部类定义
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

    public class PlayerComboSkill
    {
        public string comboId = "";
        public bool isEquipped = false;
        public float currentCooldown = 0f;
        public int useCount = 0;
    }

    public class ComboExecutionState
    {
        public string comboId = "";
        public int currentStep = 0;
        public bool isExecuting = false;
        public double startTime = 0;
        public int effectsApplied = 0;
    }

    // 单例
    private static ComboSkillSystem instance;

    // 玩家数据
    private List<string> unlockedCombos = new List<string>();
    private List<PlayerComboSkill> equippedCombos = new List<PlayerComboSkill>();
    private List<ComboExecutionState> executionQueue = new List<ComboExecutionState>();

    // 统计
    private int totalCombosExecuted = 0;
    private int totalCombosCompleted = 0;

    // 数据库引用
    private ComboSkillDatabase database;

    protected override void Initialize()
    {
        base.Initialize();
        instance = this;
        database = GetNodeOrNull<ComboSkillDatabase>("/root/ComboSkillDatabase");
    }

    public static ComboSkillSystem GetInstance()
    {
        return instance;
    }

    // ============ 解锁管理 ============

    public bool UnlockCombo(string comboId)
    {
        if (unlockedCombos.Contains(comboId))
            return false;

        var combo = database?.GetCombo(comboId);
        if (combo == null)
            return false;

        unlockedCombos.Add(comboId);
        OnComboUnlocked?.Invoke(comboId);
        return true;
    }

    public bool IsUnlocked(string comboId)
    {
        return unlockedCombos.Contains(comboId);
    }

    public List<string> GetUnlockedCombos()
    {
        return new List<string>(unlockedCombos);
    }

    // ============ 装备管理 ============

    public bool EquipCombo(string comboId)
    {
        if (!unlockedCombos.Contains(comboId))
            return false;

        // 检查是否已装备
        foreach (var equipped in equippedCombos)
        {
            if (equipped.comboId == comboId)
                return true;
        }

        // 限制装备数量
        if (equippedCombos.Count >= 5)
            return false;

        var combo = database?.GetCombo(comboId);
        if (combo == null)
            return false;

        var playerCombo = new PlayerComboSkill
        {
            comboId = comboId,
            isEquipped = true,
            currentCooldown = 0f,
            useCount = 0
        };

        equippedCombos.Add(playerCombo);
        return true;
    }

    public bool UnequipCombo(string comboId)
    {
        for (int i = 0; i < equippedCombos.Count; i++)
        {
            if (equippedCombos[i].comboId == comboId)
            {
                equippedCombos[i].isEquipped = false;
                equippedCombos.RemoveAt(i);
                return true;
            }
        }
        return false;
    }

    public bool IsEquipped(string comboId)
    {
        foreach (var equipped in equippedCombos)
        {
            if (equipped.comboId == comboId)
                return equipped.isEquipped;
        }
        return false;
    }

    public List<PlayerComboSkill> GetEquippedCombos()
    {
        return new List<PlayerComboSkill>(equippedCombos);
    }

    // ============ 执行系统 ============

    public bool ExecuteCombo(string comboId)
    {
        if (!unlockedCombos.Contains(comboId))
            return false;

        // 检查冷却
        var playerCombo = GetPlayerCombo(comboId);
        if (playerCombo != null && playerCombo.currentCooldown > 0)
            return false;

        // 检查是否正在执行
        foreach (var state in executionQueue)
        {
            if (state.comboId == comboId)
                return false;
        }

        var combo = database?.GetCombo(comboId);
        if (combo == null)
            return false;

        // 创建执行状态
        var state = new ComboExecutionState
        {
            comboId = comboId,
            currentStep = 0,
            isExecuting = true,
            startTime = Time.GetUnixTimeFromSystem(),
            effectsApplied = 0
        };

        executionQueue.Add(state);

        // 设置冷却
        if (playerCombo != null)
        {
            playerCombo.currentCooldown = (float)combo.cooldown;
            playerCombo.useCount++;
        }

        totalCombosExecuted++;
        OnComboExecuted?.Invoke(comboId);

        // 开始执行第一步
        ExecuteStep(state);

        return true;
    }

    private async void ExecuteStep(ComboExecutionState state)
    {
        if (!state.isExecuting)
            return;

        var combo = database?.GetCombo(state.comboId);
        if (combo == null || state.currentStep >= combo.steps.Count)
        {
            CompleteCombo(state);
            return;
        }

        var step = combo.steps[state.currentStep];

        // 延迟执行
        if (step.delay > 0)
        {
            await ToSignal(GetTree().CreateTimer(step.delay), "timeout");
        }

        // 检查触发条件
        if (!CheckCondition(step))
        {
            // 条件不满足，跳过此步骤
            state.currentStep++;
            ExecuteStep(state);
            return;
        }

        // 执行效果
        ApplyEffects(step.effects);
        state.effectsApplied += step.effects.Count;
        OnComboStepTriggered?.Invoke(state.comboId, state.currentStep);

        state.currentStep++;

        // 继续下一步或完成
        if (state.currentStep < combo.steps.Count)
        {
            ExecuteStep(state);
        }
        else
        {
            CompleteCombo(state);
        }
    }

    private bool CheckCondition(ComboStep step)
    {
        switch (step.condition)
        {
            case TriggerCondition.Manual:
                return true;
            case TriggerCondition.OnHit:
                return true;
            case TriggerCondition.OnCritical:
                return false;
            case TriggerCondition.OnKill:
                return false;
            case TriggerCondition.OnHealthBelow:
                return false;
            case TriggerCondition.OnManaBelow:
                return false;
            default:
                return true;
        }
    }

    private void ApplyEffects(List<ComboSkillEffect> effects)
    {
        foreach (var effect in effects)
        {
            switch (effect.effectType)
            {
                case EffectType.Damage:
                    ApplyDamage(effect.value);
                    break;
                case EffectType.Heal:
                    ApplyHeal(effect.value);
                    break;
                case EffectType.Shield:
                    ApplyShield(effect.value, effect.duration);
                    break;
                case EffectType.Buff:
                    ApplyBuff(effect.value, effect.duration);
                    break;
                case EffectType.Debuff:
                    ApplyDebuff(effect.value, effect.duration);
                    break;
            }
        }
    }

    private void ApplyDamage(float value)
    {
        GD.Print($"Combo Skill: Dealing {value} damage");
    }

    private void ApplyHeal(float value)
    {
        GD.Print($"Combo Skill: Healing {value} HP");
    }

    private void ApplyShield(float value, float duration)
    {
        GD.Print($"Combo Skill: Shield {value} for {duration} seconds");
    }

    private void ApplyBuff(float value, float duration)
    {
        GD.Print($"Combo Skill: Buff +{value} for {duration} seconds");
    }

    private void ApplyDebuff(float value, float duration)
    {
        GD.Print($"Combo Skill: Debuff {value} for {duration} seconds");
    }

    private void CompleteCombo(ComboExecutionState state)
    {
        state.isExecuting = false;
        executionQueue.Remove(state);
        totalCombosCompleted++;
        OnComboCompleted?.Invoke(state.comboId);
    }

    public bool CancelCombo(string comboId)
    {
        for (int i = 0; i < executionQueue.Count; i++)
        {
            if (executionQueue[i].comboId == comboId)
            {
                executionQueue[i].isExecuting = false;
                executionQueue.RemoveAt(i);
                OnComboCancelled?.Invoke(comboId);
                return true;
            }
        }
        return false;
    }

    // ============ 冷却管理 ============

    public override void _Process(float delta)
    {
        base._Process(delta);
        
        foreach (var equipped in equippedCombos)
        {
            if (equipped.currentCooldown > 0)
            {
                equipped.currentCooldown -= delta;
                if (equipped.currentCooldown < 0)
                    equipped.currentCooldown = 0;
                OnCooldownUpdated?.Invoke(equipped.comboId, equipped.currentCooldown);
            }
        }
    }

    public float GetCooldown(string comboId)
    {
        var playerCombo = GetPlayerCombo(comboId);
        if (playerCombo == null)
            return 0f;
        return playerCombo.currentCooldown;
    }

    public bool IsOnCooldown(string comboId)
    {
        return GetCooldown(comboId) > 0;
    }

    private PlayerComboSkill GetPlayerCombo(string comboId)
    {
        foreach (var pc in equippedCombos)
        {
            if (pc.comboId == comboId)
                return pc;
        }
        return null;
    }

    // ============ 统计信息 ============

    public Dictionary GetStatistics()
    {
        return new Dictionary
        {
            { "total_executed", totalCombosExecuted },
            { "total_completed", totalCombosCompleted },
            { "unlocked_count", unlockedCombos.Count },
            { "equipped_count", equippedCombos.Count }
        };
    }

    // ============ 存档支持 ============

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary
        {
            { "unlocked_combos", new List<string>(unlockedCombos) },
            { "equipped", new List<Dictionary>() },
            { "statistics", new Dictionary
                {
                    { "total_executed", totalCombosExecuted },
                    { "total_completed", totalCombosCompleted }
                }
            }
        };

        var equippedList = new List<Dictionary>();
        foreach (var equipped in equippedCombos)
        {
            equippedList.Add(new Dictionary
            {
                { "combo_id", equipped.comboId },
                { "cooldown", equipped.currentCooldown },
                { "use_count", equipped.useCount }
            });
        }
        data["equipped"] = equippedList;

        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        base.ImportSaveData(data);

        if (data.Contains("unlocked_combos"))
        {
            unlockedCombos = new List<string>((List<string>)data["unlocked_combos"]);
        }

        equippedCombos.Clear();
        if (data.Contains("equipped"))
        {
            var equippedList = (List<object>)data["equipped"];
            foreach (var eqData in equippedList)
            {
                var dict = (Dictionary)eqData;
                var pc = new PlayerComboSkill
                {
                    comboId = (string)dict["combo_id"],
                    isEquipped = true,
                    currentCooldown = dict.Contains("cooldown") ? (float)dict["cooldown"] : 0f,
                    useCount = dict.Contains("use_count") ? (int)dict["use_count"] : 0
                };
                equippedCombos.Add(pc);
            }
        }

        if (data.Contains("statistics"))
        {
            var stats = (Dictionary)data["statistics"];
            totalCombosExecuted = stats.Contains("total_executed") ? (int)stats["total_executed"] : 0;
            totalCombosCompleted = stats.Contains("total_completed") ? (int)stats["total_completed"] : 0;
        }
    }
}
