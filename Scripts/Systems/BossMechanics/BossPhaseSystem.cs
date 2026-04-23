using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

/// <summary>
/// Boss阶段管理系统 - 负责Boss阶段管理、愤怒机制、阶段转换
/// </summary>
public partial class BossPhaseSystem : BaseSystem
{
    public static BossPhaseSystem Instance { get; private set; }

    // 信号 - 阶段变化
    public static Action<string, int, int> BossPhaseChanged;
    public static Action<string> BossEnraged;
    public static Action<string, float> BossEnrageProgressChanged;
    public static Action<string, int> PhaseTransitionStarted;
    public static Action<string, int> PhaseTransitionCompleted;
    public static Action<string> BossRageTriggered; // REQ-127: HP-based rage

    private Random _random = new Random();

    // ========== 系统内部状态 (用于 SaveData) ==========
    // 当前关联的 BossBattleInstance 引用
    private BossBattleInstance _currentBattle;

    public override void _Ready()
    {
        Instance = this;
    }

    /// <summary>
    /// 获取或设置当前关联的 BossBattleInstance
    /// </summary>
    public BossBattleInstance CurrentBattle
    {
        get => _currentBattle;
        set => _currentBattle = value;
    }

    /// <summary>
    /// 初始化Boss阶段
    /// </summary>
    public void InitializePhase(BossBattleInstance battle)
    {
        _currentBattle = battle;
        battle.CurrentPhase = 1;
        battle.Phase = BossPhase.Active;
        battle.IsEnraged = false;
        battle.IsRageTriggered = false;
        battle.EnrageProgress = 0f;
        battle.CurrentDamageMultiplier = 1.0f;
        battle.CurrentSpeedMultiplier = 1.0f;
    }

    /// <summary>
    /// 更新Boss阶段状态（每帧调用）
    /// </summary>
    public void UpdatePhase(BossBattleInstance battle, float delta)
    {
        if (!battle.IsAlive) return;

        battle.TimeInCombat += delta;

        // 更新狂暴进度
        UpdateEnrageProgress(battle, delta);

        // 检查HP-based狂暴触发 (REQ-127)
        CheckRageTrigger(battle);

        // 检查阶段转换
        CheckPhaseTransition(battle);
    }

    /// <summary>
    /// 更新狂暴进度
    /// </summary>
    private void UpdateEnrageProgress(BossBattleInstance battle, float delta)
    {
        if (battle.Config.EnrageTimer <= 0) return;

        // 基于战斗时间计算狂暴进度
        float targetProgress = Mathf.Clamp(battle.TimeInCombat / battle.Config.EnrageTimer, 0f, 1f);
        
        if (!battle.IsEnraged && targetProgress >= 1.0f)
        {
            // 触发狂暴
            TriggerEnrage(battle);
        }
        else if (!battle.IsEnraged)
        {
            battle.EnrageProgress = targetProgress;
            BossEnrageProgressChanged?.Invoke(battle.InstanceId, battle.EnrageProgress);
        }
    }

    /// <summary>
    /// 触发狂暴状态
    /// </summary>
    public void TriggerEnrage(BossBattleInstance battle)
    {
        battle.IsEnraged = true;
        battle.EnrageProgress = 1.0f;
        battle.CurrentDamageMultiplier *= 2.0f;
        battle.CurrentSpeedMultiplier *= 1.5f;
        battle.Phase = BossPhase.Enraged;

        BossEnraged?.Invoke(battle.InstanceId);
    }

    /// <summary>
    /// 检查HP-based狂暴触发 (REQ-127: HP < RageThreshold 触发狂暴)
    /// </summary>
    private void CheckRageTrigger(BossBattleInstance battle)
    {
        if (battle.IsRageTriggered || !battle.IsAlive) return;

        // REQ-127-04: Use config values (BalanceManager > BossBalance)
        var balanceConfig = BalanceManager.Instance?.GetConfig();
        float rageThreshold = balanceConfig?.Boss?.RageThreshold ?? (battle.Config.RageThreshold > 0 ? battle.Config.RageThreshold : 0.05f);
        float rageSpeedBonus = balanceConfig?.Boss?.RageSpeedBonus ?? 1.5f;
        float rageDamageBonus = balanceConfig?.Boss?.RageDamageBonus ?? 1.0f;
        
        float healthPercent = battle.CurrentHealth / battle.Config.MaxHealth;

        if (healthPercent <= rageThreshold)
        {
            battle.IsRageTriggered = true;
            // 狂暴状态下攻击速度 +50% (configurable)
            battle.CurrentSpeedMultiplier *= rageSpeedBonus;
            battle.CurrentDamageMultiplier *= rageDamageBonus;

            BossRageTriggered?.Invoke(battle.InstanceId);
            BossEnraged?.Invoke(battle.InstanceId);
        }
    }

    /// <summary>
    /// 检查是否需要阶段转换
    /// </summary>
    private void CheckPhaseTransition(BossBattleInstance battle)
    {
        if (battle.Phase == BossPhase.Transition) return;

        float healthPercent = battle.CurrentHealth / battle.Config.MaxHealth;
        int targetPhase = GetPhaseFromHealth(healthPercent, battle.Config.PhaseCount);

        if (targetPhase > battle.CurrentPhase)
        {
            TransitionToPhase(battle, targetPhase);
        }
    }

    /// <summary>
    /// 根据血量百分比计算目标阶段
    /// </summary>
    public int GetPhaseFromHealth(float healthPercent, int totalPhases)
    {
        if (totalPhases <= 1) return 1;
        
        float phaseThreshold = 1.0f / totalPhases;
        for (int i = totalPhases - 1; i >= 0; i--)
        {
            if (healthPercent <= (i + 1) * phaseThreshold)
                return totalPhases - i;
        }
        return 1;
    }

    /// <summary>
    /// 转换到新阶段
    /// </summary>
    public void TransitionToPhase(BossBattleInstance battle, int newPhase)
    {
        int oldPhase = battle.CurrentPhase;
        battle.CurrentPhase = newPhase;
        battle.Phase = BossPhase.Transition;

        // 应用阶段加成
        float phaseMultiplier = 1.0f + (newPhase - 1) * 0.25f;
        battle.CurrentDamageMultiplier *= phaseMultiplier;
        
        PhaseTransitionStarted?.Invoke(battle.InstanceId, newPhase);
        BossPhaseChanged?.Invoke(battle.InstanceId, oldPhase, newPhase);

        // 延迟完成阶段转换 (REQ-058-11: migrated from Godot 3 .Connect() to C# event +=)
        GetTree().CreateTimer(2.0f).Timeout += () => OnPhaseTransitionComplete(battle.InstanceId, newPhase);
    }

    private void OnPhaseTransitionComplete(string instanceId, int phase)
    {
        // 通过事件系统通知完成
        PhaseTransitionCompleted?.Invoke(instanceId, phase);
    }

    /// <summary>
    /// 完成阶段转换（外部调用）
    /// </summary>
    public void CompletePhaseTransition(BossBattleInstance battle)
    {
        if (battle.Phase == BossPhase.Transition)
        {
            battle.Phase = battle.IsEnraged ? BossPhase.Enraged : BossPhase.Active;
            PhaseTransitionCompleted?.Invoke(battle.InstanceId, battle.CurrentPhase);
        }
    }

    /// <summary>
    /// 获取当前阶段的详细信息
    /// </summary>
    public BossPhaseConfig GetPhaseConfig(BossConfig config, int phase)
    {
        // 从配置中获取阶段配置
        // 这里可以从数据库获取，也可以从Config中解析
        var phaseConfig = new BossPhaseConfig
        {
            PhaseNumber = phase,
            Name = $"阶段 {phase}",
            HealthPercentage = 1.0f - ((float)(phase - 1) / config.PhaseCount),
            DamageMultiplier = 1.0f + (phase - 1) * 0.25f,
            SpeedMultiplier = 1.0f + (phase - 1) * 0.1f,
            IsTransitionPhase = false,
            TransitionDuration = 2.0f
        };
        
        return phaseConfig;
    }

    /// <summary>
    /// 检查Boss是否处于可攻击状态
    /// </summary>
    public bool CanBeAttacked(BossBattleInstance battle)
    {
        return battle.IsAlive && 
               (battle.Phase == BossPhase.Active || 
                battle.Phase == BossPhase.Enraged);
    }

    /// <summary>
    /// 获取狂暴剩余时间
    /// </summary>
    public float GetEnrageTimeRemaining(BossBattleInstance battle)
    {
        if (battle.Config.EnrageTimer <= 0) return float.MaxValue;
        return Mathf.Max(0, battle.Config.EnrageTimer - battle.TimeInCombat);
    }

    /// <summary>
    /// 导出阶段系统数据 (Override基类无参方法)
    /// </summary>
    public override Dictionary<string, object> ExportSaveData()
    {
        var data = new Dictionary<string, object>();

        // 从当前关联的battle导出数据
        if (_currentBattle != null)
        {
            data["currentPhase"] = _currentBattle.CurrentPhase;
            data["isEnraged"] = _currentBattle.IsEnraged;
            data["enrageProgress"] = _currentBattle.EnrageProgress;
            data["timeInCombat"] = _currentBattle.TimeInCombat;
            data["phase"] = (int)_currentBattle.Phase;
            data["currentDamageMultiplier"] = _currentBattle.CurrentDamageMultiplier;
            data["currentSpeedMultiplier"] = _currentBattle.CurrentSpeedMultiplier;
        }

        return data;
    }

    /// <summary>
    /// 导入阶段系统数据 (Override基类无参方法)
    /// </summary>
    public override void ImportSaveData(Dictionary<string, object> data)
    {
        if (data == null) return;

        // 导入到当前关联的battle
        if (_currentBattle != null)
        {
            _currentBattle.CurrentPhase = (int)data.GetValueOrDefault("currentPhase", 1);
            _currentBattle.IsEnraged = (bool)data.GetValueOrDefault("isEnraged", false);
            _currentBattle.EnrageProgress = (float)data.GetValueOrDefault("enrageProgress", 0f);
            _currentBattle.TimeInCombat = (float)data.GetValueOrDefault("timeInCombat", 0f);

            // 恢复阶段状态
            _currentBattle.Phase = (BossPhase)(int)data.GetValueOrDefault("phase", (int)BossPhase.Active);
            _currentBattle.CurrentDamageMultiplier = (float)data.GetValueOrDefault("currentDamageMultiplier", 1.0f);
            _currentBattle.CurrentSpeedMultiplier = (float)data.GetValueOrDefault("currentSpeedMultiplier", 1.0f);
        }
    }

    /*
    /// <summary>
    /// 导出阶段系统数据 (旧方法，保留以兼容)
    /// </summary>
    public Dictionary ExportSaveData(BossBattleInstance battle)
    {
        var data = new Dictionary<string, object>();

        if (battle != null)
        {
            data["currentPhase"] = battle.CurrentPhase;
            data["isEnraged"] = battle.IsEnraged;
            data["enrageProgress"] = battle.EnrageProgress;
            data["timeInCombat"] = battle.TimeInCombat;
        }

        return data;
    }

    /// <summary>
    /// 导入阶段系统数据 (旧方法，保留以兼容)
    /// </summary>
    public void ImportSaveData(BossBattleInstance battle, Dictionary data)
    {
        if (battle == null || data == null) return;

        battle.CurrentPhase = data.GetValueOrDefault("currentPhase", 1);
        battle.IsEnraged = data.GetValueOrDefault("isEnraged", false);
        battle.EnrageProgress = data.GetValueOrDefault("enrageProgress", 0f);
        battle.TimeInCombat = data.GetValueOrDefault("timeInCombat", 0f);

        // 恢复阶段状态
        battle.Phase = battle.IsEnraged ? BossPhase.Enraged : BossPhase.Active;
    }
    */
}
