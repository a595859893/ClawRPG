using Godot;
using System;
using System.Collections.Generic;
using ClawRPG.Scripts.Systems;

/// <summary>
/// Combo 掉落系统
/// REQ-155: Boss 狂暴触发时，必定掉落一张传说级 Combo
/// 职责：
/// 1. 订阅 BossEnrageManager.OnEnragedComboDrop
/// 2. 从已发现的传说 Combo 中随机选择一张
/// 3. 如果没有可用的传说 Combo，fallback 到 Epic
/// 4. 调用 ComboSystem.ForceDiscoverCombo() 发放
/// </summary>
public partial class ComboDropSystem : BaseSystem
{
    public static ComboDropSystem Instance { get; private set; }

    // 掉落通知信号
    public static Action<string, ComboData> OnComboDropGranted;

    private Random _random = new Random();

    public override void _Ready()
    {
        Instance = this;

        // 订阅狂暴 Combo 掉落事件
        BossEnrageManager.OnEnragedComboDrop += OnEnragedComboDrop;
    }

    public override void _ExitTree()
    {
        BossEnrageManager.OnEnragedComboDrop -= OnEnragedComboDrop;
    }

    /// <summary>
    /// 狂暴触发时调用：给予传说 Combo
    /// </summary>
    private void OnEnragedComboDrop(string battleInstanceId)
    {
        string dropComboId = PickRandomLegendaryCombo();

        if (string.IsNullOrEmpty(dropComboId))
        {
            GD.PrintErr("[ComboDropSystem] No legendary combo available for drop! Falling back to Epic.");
            dropComboId = PickRandomEpicCombo();
        }

        if (!string.IsNullOrEmpty(dropComboId))
        {
            // 强制发现该 Combo
            ComboSystem.Instance?.ForceDiscoverCombo(dropComboId);

            // 获取 Combo 数据用于通知
            var comboData = ComboSystem.Instance?.GetAllCombos().GetValueOrDefault(dropComboId);
            if (comboData != null)
            {
                GD.Print($"[ComboDropSystem] Granted legendary combo: {comboData.comboName} ({comboData.comboRarity})");
                OnComboDropGranted?.Invoke(dropComboId, comboData);
            }
        }
    }

    /// <summary>
    /// 从已发现的传说 Combo 中随机选择一张
    /// </summary>
    private string PickRandomLegendaryCombo()
    {
        var allCombos = ComboSystem.Instance?.GetAllCombos();
        if (allCombos == null) return null;

        var legendaryIds = new List<string>();
        foreach (var kvp in allCombos)
        {
            if (kvp.Value.comboRarity == ComboData.Rarity.Legendary)
            {
                legendaryIds.Add(kvp.Key);
            }
        }

        if (legendaryIds.Count == 0) return null;

        int idx = _random.Next(legendaryIds.Count);
        return legendaryIds[idx];
    }

    /// <summary>
    /// Fallback：从已发现的 Epic Combo 中随机选择一张
    /// </summary>
    private string PickRandomEpicCombo()
    {
        var allCombos = ComboSystem.Instance?.GetAllCombos();
        if (allCombos == null) return null;

        var epicIds = new List<string>();
        foreach (var kvp in allCombos)
        {
            if (kvp.Value.comboRarity == ComboData.Rarity.Epic)
            {
                epicIds.Add(kvp.Key);
            }
        }

        if (epicIds.Count == 0) return null;

        int idx = _random.Next(epicIds.Count);
        return epicIds[idx];
    }

    public override Dictionary<string, object> ExportSaveData()
    {
        // Combo 掉落跟随 ComboSystem 的 discoveredCombos 持久化，无需单独存储
        return new Dictionary<string, object>();
    }

    public override void ImportSaveData(Dictionary<string, object> data)
    {
        // 无需单独恢复
    }
}
