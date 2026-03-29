using Godot;
using System;
using System.Collections.Generic;

/// <summary>
/// Boss 狂暴管理器
/// REQ-155: 狂暴倒计时作为决策点
/// 职责：
/// 1. 订阅 BossPhaseSystem.BossEnraged，触发传说 Combo 掉落
/// 2. 提供狂暴倒计时剩余秒数（供 UI 使用）
/// 3. 防止同一场战斗重复掉落
/// </summary>
public partial class BossEnrageManager : BaseSystem
{
    public static BossEnrageManager Instance { get; private set; }

    // 信号：狂暴触发时发放传说 Combo
    public static Action<string> OnEnragedComboDrop;

    // 当前战斗是否已触发过狂暴奖励（防止重复）
    private HashSet<string> _enragedBattleIds = new HashSet<string>();

    // 当前关联的 BossBattleInstance
    private BossBattleInstance _currentBattle;

    public override void _Ready()
    {
        Instance = this;

        // 订阅狂暴事件
        BossPhaseSystem.BossEnraged += OnBossEnraged;
    }

    public override void _ExitTree()
    {
        BossPhaseSystem.BossEnraged -= OnBossEnraged;
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
    }

    /// <summary>
    /// 狂暴触发时调用：触发传说 Combo 掉落
    /// </summary>
    private void OnBossEnraged(string battleInstanceId)
    {
        // 防止同一场战斗重复触发
        if (_enragedBattleIds.Contains(battleInstanceId))
        {
            return;
        }
        _enragedBattleIds.Add(battleInstanceId);

        GD.Print($"[BossEnrageManager] Boss enraged! InstanceId={battleInstanceId}. Triggering combo drop.");

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
