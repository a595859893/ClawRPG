using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Boss 狂暴管理器
/// REQ-155: 狂暴倒计时作为决策点
/// REQ-156-03: 发射 ModeChanged 信号并触发行为树更新
/// 职责：
/// 1. 订阅 BossPhaseSystem.BossEnraged，触发传说 Combo 掉落
/// 2. 订阅 BossPhaseSystem.BossRageTriggered，触发模式切换
/// 3. 提供狂暴倒计时剩余秒数（供 UI 使用）
/// 4. 防止同一场战斗重复触发
/// 5. 发射 BossModeChanged 信号驱动 BossAI 模式切换
/// </summary>
public partial class BossEnrageManager : BaseSystem
{
    public static BossEnrageManager Instance { get; private set; }

    // 信号：狂暴触发时发放传说 Combo
    public static Action<string> OnEnragedComboDrop;

    // 信号：模式切换时发射 (REQ-156-03)
    // 参数: (battleInstanceId, oldMode, newMode)
    public static Action<string, int, int> OnBossModeChanged;

    // 当前战斗是否已触发过狂暴奖励（防止重复）
    private HashSet<string> _enragedBattleIds = new HashSet<string>();

    // 当前关联的 BossBattleInstance
    private BossBattleInstance _currentBattle;

    // 当前模式（0=Strategic, 1=Enraged）用于信号参数
    private int _currentMode = 0;

    public override void _Ready()
    {
        Instance = this;

        // 订阅狂暴事件
        BossPhaseSystem.BossEnraged += OnBossEnraged;
        BossPhaseSystem.BossRageTriggered += OnBossRageTriggered;
    }

    public override void _ExitTree()
    {
        BossPhaseSystem.BossEnraged -= OnBossEnraged;
        BossPhaseSystem.BossRageTriggered -= OnBossRageTriggered;
    }

    /// <summary>
    /// 关联当前 BossBattleInstance
    /// </summary>
    public void SetCurrentBattle(BossBattleInstance battle)
    {
        _currentBattle = battle;
    }

    /// <summary>
    /// 重置当前战斗的狂暴奖励状态（新战斗开始时调用）
    /// </summary>
    public void ResetForBattle(string battleId)
    {
        _enragedBattleIds.Remove(battleId);
        _currentMode = 0; // 重置为策略模式
    }

    /// <summary>
    /// HP < 5% 狂暴触发时调用（REQ-156-03）
    /// 发射 ModeChanged 信号，驱动 BossAI 模式切换
    /// </summary>
    private void OnBossRageTriggered(string battleInstanceId)
    {
        if (_enragedBattleIds.Contains(battleInstanceId))
            return;

        _enragedBattleIds.Add(battleInstanceId);

        int oldMode = _currentMode;
        _currentMode = 1; // Enraged

        GD.Print($"[BossEnrageManager] Boss RAGE triggered! InstanceId={battleInstanceId}. ModeChanged: {oldMode} → {_currentMode}");

        // REQ-156-03: 发射模式切换信号，驱动 BossAI.SetMode(BossMode.Enraged)
        OnBossModeChanged?.Invoke(battleInstanceId, oldMode, _currentMode);

        // 通知 ComboDropSystem 发放传说 Combo
        OnEnragedComboDrop?.Invoke(battleInstanceId);
    }

    /// <summary>
    /// 狂暴触发时调用（时间-based）：触发传说 Combo 掉落
    /// </summary>
    private void OnBossEnraged(string battleInstanceId)
    {
        // 防止同一场战斗重复触发（HP-based rage 已处理）
        if (_enragedBattleIds.Contains(battleInstanceId))
        {
            return;
        }
        _enragedBattleIds.Add(battleInstanceId);

        int oldMode = _currentMode;
        _currentMode = 1; // Enraged

        GD.Print($"[BossEnrageManager] Boss ENRAGED (timer)! InstanceId={battleInstanceId}. ModeChanged: {oldMode} → {_currentMode}");

        // REQ-156-03: 发射模式切换信号
        OnBossModeChanged?.Invoke(battleInstanceId, oldMode, _currentMode);

        // 通知 ComboDropSystem 发放传说 Combo
        OnEnragedComboDrop?.Invoke(battleInstanceId);
    }

    /// <summary>
    /// 获取狂暴剩余倒计时秒数（向上取整）
    /// </summary>
    public int GetEnrageCountdownSeconds(BossBattleInstance battle)
    {
        if (battle == null || battle.IsEnraged || battle.Config.EnrageTimer <= 0)
            return -1;

        float remaining = battle.Config.EnrageTimer - battle.TimeInCombat;
        return Mathf.Max(0, Mathf.CeilToInt(remaining));
    }

    /// <summary>
    /// 获取当前模式名称（REQ-156-03）
    /// </summary>
    public string GetCurrentModeName()
    {
        return _currentMode == 0 ? "Strategic" : "Enraged";
    }

    public override Dictionary ExportSaveData()
    {
        var data = new Dictionary();
        // 狂暴管理器不保留跨存档数据（每场战斗独立）
        return data;
    }

    public override void ImportSaveData(Dictionary data)
    {
        // 无需持久化
    }
}
